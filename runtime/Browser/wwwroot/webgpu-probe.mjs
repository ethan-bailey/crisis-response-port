const shaderCode = /* wgsl */ `
struct VertexOutput {
  @builtin(position) position: vec4f,
  @location(0) uv: vec2f,
}

@vertex fn vertexMain(@builtin(vertex_index) index: u32) -> VertexOutput {
  var positions = array<vec2f, 3>(
    vec2f(-1.0, -1.0),
    vec2f(3.0, -1.0),
    vec2f(-1.0, 3.0),
  );
  var uvs = array<vec2f, 3>(
    vec2f(0.0, 1.0),
    vec2f(2.0, 1.0),
    vec2f(0.0, -1.0),
  );
  var output: VertexOutput;
  output.position = vec4f(positions[index], 0.0, 1.0);
  output.uv = uvs[index];
  return output;
}

@group(0) @binding(0) var image: texture_2d<f32>;
@group(0) @binding(1) var imageSampler: sampler;

@fragment fn fragmentMain(input: VertexOutput) -> @location(0) vec4f {
  return textureSample(image, imageSampler, input.uv);
}`;

const texturePixels = new Uint8Array([
  235, 74, 52, 255, 250, 205, 55, 255,
  44, 165, 112, 255, 48, 125, 220, 255,
]);

function setStatus(element, message) {
  if (element) element.textContent = message;
}

/**
 * Capability-gated, standalone WebGPU diagnostic. It does not bind or replace
 * any runtime render driver. Call only from the explicit ?webgpu opt-in path.
 */
export async function initializeWebGpuProbe(canvas, statusElement, environment = globalThis) {
  if (!canvas) throw new TypeError('A canvas is required for the WebGPU probe');
  const gpu = environment.navigator?.gpu;
  if (!gpu) {
    setStatus(statusElement, 'WebGPU is unavailable in this browser. The existing runtime path is unchanged.');
    return { status: 'unsupported' };
  }

  let adapter;
  let device;
  let context;
  let texture;
  let observer;
  let deviceLost = false;
  try {
    adapter = await gpu.requestAdapter();
    if (!adapter) {
      setStatus(statusElement, 'No WebGPU adapter is available. The existing runtime path is unchanged.');
      return { status: 'no-adapter' };
    }

    device = await adapter.requestDevice();
    context = canvas.getContext('webgpu');
    if (!context) throw new Error('The browser did not provide a WebGPU canvas context');

    const format = gpu.getPreferredCanvasFormat();
    const configure = () => {
      const scale = environment.devicePixelRatio || 1;
      const bounds = canvas.getBoundingClientRect?.() ?? { width: canvas.width, height: canvas.height };
      const width = Math.max(1, Math.round(bounds.width * scale));
      const height = Math.max(1, Math.round(bounds.height * scale));
      if (canvas.width !== width) canvas.width = width;
      if (canvas.height !== height) canvas.height = height;
      context.configure({ device, format, alphaMode: 'opaque' });
    };

    texture = device.createTexture({
      size: { width: 2, height: 2, depthOrArrayLayers: 1 },
      format: 'rgba8unorm',
      usage: environment.GPUTextureUsage.TEXTURE_BINDING | environment.GPUTextureUsage.COPY_DST,
    });
    device.queue.writeTexture(
      { texture }, texturePixels,
      { bytesPerRow: 8, rowsPerImage: 2 },
      { width: 2, height: 2, depthOrArrayLayers: 1 },
    );

    const shader = device.createShaderModule({ code: shaderCode });
    const pipeline = device.createRenderPipeline({
      layout: 'auto',
      vertex: { module: shader, entryPoint: 'vertexMain' },
      fragment: {
        module: shader,
        entryPoint: 'fragmentMain',
        targets: [{ format }],
      },
      primitive: { topology: 'triangle-list' },
    });
    const sampler = device.createSampler({ magFilter: 'nearest', minFilter: 'nearest' });
    const bindGroup = device.createBindGroup({
      layout: pipeline.getBindGroupLayout(0),
      entries: [
        { binding: 0, resource: texture.createView() },
        { binding: 1, resource: sampler },
      ],
    });

    const draw = () => {
      if (deviceLost) return;
      const encoder = device.createCommandEncoder();
      const pass = encoder.beginRenderPass({
        colorAttachments: [{
          view: context.getCurrentTexture().createView(),
          clearValue: { r: 0.035, g: 0.055, b: 0.08, a: 1 },
          loadOp: 'clear',
          storeOp: 'store',
        }],
      });
      pass.setPipeline(pipeline);
      pass.setBindGroup(0, bindGroup);
      pass.draw(3);
      pass.end();
      device.queue.submit([encoder.finish()]);
    };

    configure();
    draw();
    if (environment.ResizeObserver) {
      observer = new environment.ResizeObserver(() => {
        try { configure(); draw(); }
        catch (error) { setStatus(statusElement, `Resize failed: ${error.message}`); }
      });
      observer.observe(canvas);
    }

    device.lost.then(info => {
      deviceLost = true;
      observer?.disconnect();
      setStatus(statusElement, `WebGPU device lost (${info.reason || 'unknown'}): ${info.message || 'no details'}`);
    });
    setStatus(statusElement, `WebGPU probe rendered a textured triangle (${format}); resize and device-loss handlers are active.`);
    return {
      status: 'ready', format, adapter, device,
      resize: () => { configure(); draw(); },
      dispose: () => { observer?.disconnect(); texture.destroy(); device.destroy(); },
    };
  } catch (error) {
    observer?.disconnect();
    texture?.destroy();
    device?.destroy();
    setStatus(statusElement, `WebGPU probe failed: ${error.message}. The existing runtime path is unchanged.`);
    return { status: 'failed', error: String(error) };
  }
}
