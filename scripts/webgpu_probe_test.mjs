import assert from 'node:assert/strict';
import { initializeWebGpuProbe } from '../runtime/Browser/wwwroot/webgpu-probe.mjs';

function makeHarness({ adapter = true } = {}) {
  const calls = [];
  let resizeCallback;
  let loseDevice;
  let width = 0;
  let height = 0;
  const status = { textContent: '' };
  const texture = { createView: () => ({ kind: 'texture-view' }), destroy: () => calls.push('texture.destroy') };
  const pass = {
    setPipeline: () => calls.push('pipeline'),
    setBindGroup: (index) => calls.push(`bind:${index}`),
    draw: (count) => calls.push(`draw:${count}`),
    end: () => calls.push('pass.end'),
  };
  const device = {
    lost: new Promise(resolve => { loseDevice = resolve; }),
    queue: {
      writeTexture: (...args) => calls.push(['writeTexture', ...args]),
      submit: commands => calls.push(['submit', commands]),
    },
    createTexture: descriptor => { calls.push(['createTexture', descriptor]); return texture; },
    createShaderModule: descriptor => { calls.push(['shader', descriptor]); return {}; },
    createRenderPipeline: descriptor => { calls.push(['pipeline.create', descriptor]); return { getBindGroupLayout: index => ({ index }) }; },
    createSampler: descriptor => { calls.push(['sampler', descriptor]); return {}; },
    createBindGroup: descriptor => { calls.push(['bindGroup', descriptor]); return {}; },
    createCommandEncoder: () => ({
      beginRenderPass: descriptor => { calls.push(['beginRenderPass', descriptor]); return pass; },
      finish: () => ({ kind: 'commands' }),
    }),
    destroy: () => calls.push('device.destroy'),
  };
  const gpu = {
    requestAdapter: async () => adapter ? {
      requestDevice: async () => device,
    } : null,
    getPreferredCanvasFormat: () => 'bgra8unorm',
  };
  const context = {
    configure: descriptor => calls.push(['configure', descriptor]),
    getCurrentTexture: () => ({ createView: () => ({ kind: 'canvas-view' }) }),
  };
  const canvas = {
    get width() { return width; }, set width(value) { width = value; },
    get height() { return height; }, set height(value) { height = value; },
    getContext: kind => kind === 'webgpu' ? context : null,
    getBoundingClientRect: () => ({ width: 200, height: 100 }),
  };
  const environment = {
    navigator: { gpu },
    devicePixelRatio: 2,
    GPUTextureUsage: { TEXTURE_BINDING: 4, COPY_DST: 2 },
    ResizeObserver: class {
      constructor(callback) { resizeCallback = callback; }
      observe(target) { calls.push(['observe', target]); }
      disconnect() { calls.push('observer.disconnect'); }
    },
  };
  return { calls, canvas, device, environment, loseDevice: info => loseDevice(info), status, triggerResize: () => resizeCallback() };
}

const unsupportedStatus = { textContent: '' };
assert.deepEqual(await initializeWebGpuProbe({ getContext() { throw new Error('must not request a context'); } }, unsupportedStatus, { navigator: {} }), { status: 'unsupported' });
assert.match(unsupportedStatus.textContent, /unavailable/);

const noAdapter = makeHarness({ adapter: false });
assert.deepEqual(await initializeWebGpuProbe(noAdapter.canvas, noAdapter.status, noAdapter.environment), { status: 'no-adapter' });
assert.match(noAdapter.status.textContent, /No WebGPU adapter/);

const probe = makeHarness();
const result = await initializeWebGpuProbe(probe.canvas, probe.status, probe.environment);
assert.equal(result.status, 'ready');
assert.equal(result.format, 'bgra8unorm');
assert.equal(probe.canvas.width, 400);
assert.equal(probe.canvas.height, 200);
assert.equal(probe.calls.filter(call => Array.isArray(call) && call[0] === 'submit').length, 1);
assert.ok(probe.calls.some(call => Array.isArray(call) && call[0] === 'writeTexture' && call[2] instanceof Uint8Array));
assert.ok(probe.calls.includes('draw:3'));
assert.match(probe.status.textContent, /textured triangle/);

probe.triggerResize();
assert.equal(probe.calls.filter(call => Array.isArray(call) && call[0] === 'submit').length, 2);
probe.loseDevice({ reason: 'destroyed', message: 'test loss' });
await Promise.resolve();
assert.match(probe.status.textContent, /device lost.*test loss/);
assert.ok(probe.calls.includes('observer.disconnect'));

result.dispose();
assert.ok(probe.calls.includes('texture.destroy'));
assert.ok(probe.calls.includes('device.destroy'));
console.log('PASS: WebGPU capability fallback, textured draw, resize, device loss, and cleanup');
