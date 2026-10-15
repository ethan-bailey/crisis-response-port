import * as THREE from 'three';

// Renderer-only mapping for original Mission 01 BallisticWindow.MeshFilter.
// Verified exact scene source: every one of 68 Window components uses the
// built-in Unity Quad (10210), 56 Window glass and 12 mirror-glass materials.
// This is NOT a replacement BallisticWindow, physics, damage or shader.
const ORIGINAL_SCENE_SHA = 'c43b36c415b4df14ef6223eaced686a8ddcead6c46f7504346aecb1a03c5743e';
const BUILTIN_QUAD_GUID = '0000000000000000e000000000000000';
const reflection = new THREE.Matrix4().makeScale(1, 1, -1);

function bytes(base64) {
  const raw = atob(base64), out = new Uint8Array(raw.length);
  for (let i=0;i<raw.length;i++) out[i]=raw.charCodeAt(i);
  return out;
}

export function createOriginalWindowVisuals(scene, ballisticsData, manifest) {
  if (manifest?.sceneSha256 !== ORIGINAL_SCENE_SHA ||
      manifest?.meshFileID !== '10210' || manifest?.meshGuid !== BUILTIN_QUAD_GUID ||
      manifest.windows?.length !== 68) {
    throw new Error('Source window geometry provenance does not match audited Mission 01');
  }
  const sourceWindows = new Map(ballisticsData.materials.filter(m=>m.kind==='window')
    .map(m=>[String(m.fields.m_GameObject.fileID),m]));
  if (sourceWindows.size!==68) throw new Error('Original material and renderer windows differ');
  const sharedGeometry = new THREE.PlaneGeometry(1,1);
  const registered = new Map();
  const originTextures = ballisticsData.textures;
  let glassCount=0,mirrorCount=0,stamps=0,resets=0;
  for (const record of manifest.windows) {
    const id=String(record.sourceObject), source=sourceWindows.get(id);
    if (!source || source.id!==record.sourceComponent ||
        source.fields.texture?.guid!==record.texture ||
        Boolean(source.fields.holeTexture?.guid)!==record.hasHoleTexture ||
        registered.has(id) || record.matrix?.length!==16 ||
        !record.matrix.every(Number.isFinite)) {
      throw new Error('Original window renderer binding has mismatched source references');
    }
    const origin=originTextures[record.texture];
    if (!origin || !Number.isInteger(origin.width) || !Number.isInteger(origin.height)) {
      throw new Error('Original glass texture metadata unavailable: '+id);
    }
    // This window has a serialized source mirror shader. Its bespoke optics are
    // not recovered yet, so do not silently invent a mirror appearance.
    if (!record.hasHoleTexture) {
      mirrorCount++;
      registered.set(id,{mirror:true});
      continue;
    }
    const initial=bytes(origin.rgba);
    if (initial.length!==origin.width*origin.height*4) {
      throw new Error('Original glass pixel buffer length mismatch');
    }
    const pixels=initial.slice();
    const texture=new THREE.DataTexture(pixels,origin.width,origin.height,THREE.RGBAFormat,THREE.UnsignedByteType);
    // Native Texture2D.GetPixels and the extraction are bottom-row first.
    texture.flipY=false;texture.colorSpace=THREE.SRGBColorSpace;
    texture.magFilter=THREE.LinearFilter;texture.minFilter=THREE.LinearFilter;
    texture.needsUpdate=true;
    // Original Window glass.mat: custom shader, _Opacity=0.25, queue=3000.
    // MeshBasicMaterial is a browser presentation adapter, NOT shader parity.
    const material=new THREE.MeshBasicMaterial({
      map:texture,color:0xffffff,opacity:0.25,transparent:true,
      side:THREE.DoubleSide,depthWrite:false,alphaTest:0,
      polygonOffset:true,polygonOffsetFactor:-1,polygonOffsetUnits:-1
    });
    const mesh=new THREE.Mesh(sharedGeometry,material);
    mesh.name='Original BallisticWindow Unity Quad '+id;
    mesh.userData.originalWindow=id;
    mesh.visible=Boolean(record.active);
    const matrix=new THREE.Matrix4().fromArray(record.matrix);
    mesh.matrix.copy(reflection).multiply(matrix).multiply(reflection);
    mesh.matrixAutoUpdate=false;
    mesh.renderOrder=2;
    scene.add(mesh);
    registered.set(id,{mirror:false,initial,pixels,texture,mesh,material,width:origin.width,height:origin.height});
    glassCount++;
  }
  if (glassCount!==56 || mirrorCount!==12) {
    dispose();
    throw new Error('Audited source glass/mirror distribution changed');
  }
  function update(events) {
    for (const event of events) {
      if (event.type!=='windowStamp' && event.type!=='windowReset') continue;
      const value=event.value, id=String(value?.sourceObject), state=registered.get(id);
      if (!state) throw new Error('Unmapped original window event: '+id);
      if (state.mirror) continue; // do not synthesize unknown mirror shader.
      if (value.texture!==sourceWindows.get(id).fields.texture.guid) {
        throw new Error('Window event refers to a different original texture');
      }
      if(event.type==='windowReset') {
        state.pixels.set(state.initial);state.texture.needsUpdate=true;resets++;
        continue;
      }
      const {x,y,width,height}=value;
      if (![x,y,width,height].every(Number.isInteger) || width<=0 || height<=0 ||
          x<0 || y<0 || x+width>state.width || y+height>state.height) {
        throw new RangeError('Original window stamp outside source pixel bounds');
      }
      const patch=bytes(value.rgba);
      if (patch.length!==width*height*4) throw new Error('Original window RGBA patch size mismatch');
      for(let row=0;row<height;row++) {
        const dst=((y+row)*state.width+x)*4;
        state.pixels.set(patch.subarray(row*width*4,(row+1)*width*4),dst);
      }
      state.texture.needsUpdate=true;stamps++;
    }
  }
  function snapshot() {return {sourceWindows:registered.size,visibleGlass:glassCount,unrenderedMirrorShaders:mirrorCount,stamps,resets};}
  function dispose() {
    for(const state of registered.values())if(!state.mirror) {
      scene.remove(state.mesh);state.material.dispose();state.texture.dispose();
    }
    registered.clear();sharedGeometry.dispose();
  }
  return {update,snapshot,dispose,registered};
}
