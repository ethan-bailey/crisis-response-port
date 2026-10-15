import * as THREE from 'three';

// Presentation adapter only. Original BallisticHitMeshEffect.Play/Update and its
// scale animation remain in runtime/Original. This displays the exact recovered
// Circle.asset mesh for the original Wood exit-hole prefab without inventing a
// new effect geometry or gameplay lifetime.
const WOOD_EXIT_PREFAB = 'c0bb8365c7bd54b4597570f085f5a153';
const SOURCE_ASSET = './assets/ballistics/wood-exit-hole-source.json';
const SOURCE_MESH_SHA256 = '1a6a7c334014210f536eadc6e21db582bda8f4de56be1330941bbd3ce619688e';

export function createOriginalImpactMeshRenderer(scene) {
  let resource = null, error = null;
  const visible = new Map();

  const loading = fetch(SOURCE_ASSET)
    .then(response => {
      if (!response.ok) throw new Error(`Original exit-hole asset HTTP ${response.status}`);
      return response.json();
    })
    .then(data => {
      if (data.file_guids?.prefab !== WOOD_EXIT_PREFAB ||
          data.source_sha256?.mesh !== SOURCE_MESH_SHA256 ||
          data.file_guids?.texture !== null ||
          data.counts?.vertices !== 186 || data.counts?.triangles !== 100 ||
          !['Unity 2017 compressed mesh packed floats', 'Unity 2017 raw vertex channel buffers'].includes(data.mesh?.source_format)) {
        throw new Error('Original Wood exit-hole mesh source metadata did not match');
      }
      const { positions, normals, indices, uv } = data.mesh;
      if (positions.length !== 186 || indices.length !== 300 ||
          (normals.length && normals.length !== positions.length) ||
          (uv.length && uv.length !== positions.length) ||
          indices.some(i => !Number.isInteger(i) || i < 0 || i >= positions.length) ||
          positions.some(v => v.length !== 3 || v.some(n => !Number.isFinite(n)))) {
        throw new Error('Original Wood exit-hole mesh buffers are invalid');
      }
      // IMPORTANT: extract_original_exit_hole_mesh.py obtains its geometry
      // from unity_mesh_to_obj.py or its compressed-mesh fallback. BOTH source
      // paths *already* negate Unity Z and reverse triangle winding. These
      // serialized buffers are therefore Three-ready right-handed geometry.
      // Reflect only the LIVE C# world pose below, not the extracted vertices,
      // normals or indices again; double conversion was a visual parity bug.
      const geometry = new THREE.BufferGeometry();
      geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions.flat(), 3));
      geometry.setIndex(indices);
      if (normals.length) geometry.setAttribute('normal', new THREE.Float32BufferAttribute(normals.flat(), 3));
      else geometry.computeVertexNormals(); // renderer-only reconstruction from original faces
      if (uv.length) geometry.setAttribute('uv', new THREE.Float32BufferAttribute(uv.flat(), 2));
      const source = data.source_material;
      const c = source.colors._Color;
      const material = new THREE.MeshStandardMaterial({
        color: new THREE.Color().setRGB(c.r, c.g, c.b),
        metalness: source.floats._Metallic,
        roughness: 1 - source.floats._Glossiness,
        side: THREE.DoubleSide
      });
      resource = { geometry, material };
    }).catch(e => {
      error = String(e?.message || e);
      console.warn('Original exit-hole render asset unavailable:', error);
    });

  function update(events) {
    if (!resource) return; // do not substitute approximate geometry while loading
    const seen = new Set();
    for (const event of events) {
      if (event.type !== 'impactMesh' || event.value?.source !== WOOD_EXIT_PREFAB) continue;
      const source = event.value;
      const id = String(source.instance);
      seen.add(id);
      let mesh = visible.get(id);
      if (!mesh) {
        mesh = new THREE.Mesh(resource.geometry, resource.material);
        mesh.name = 'Original Wood exit-hole Circle.asset';
        mesh.userData.originalEffectSource = WOOD_EXIT_PREFAB;
        visible.set(id, mesh);
        scene.add(mesh);
      }
      const p = source.position, q = source.rotation, s = source.scale;
      if (!p || !q || !s) throw new Error('Original impactMesh transform event is incomplete');
      mesh.position.set(p[0], p[1], -p[2]);
      mesh.quaternion.set(-q[0], -q[1], q[2], q[3]).normalize();
      mesh.scale.set(s[0], s[1], s[2]);
    }
    // The original runtime emits a mesh pose each frame only while the source
    // component is alive; absence of that instance ends its visible lifetime.
    for (const [id, mesh] of visible) {
      if (seen.has(id)) continue;
      scene.remove(mesh);
      visible.delete(id);
    }
  }
  function snapshot() {
    return { loaded: Boolean(resource), active: visible.size,
             originalVertices: resource ? 186 : 0, assetError: error };
  }
  function dispose() {
    for (const mesh of visible.values()) scene.remove(mesh);
    visible.clear();
    resource?.geometry.dispose();
    resource?.material.dispose();
  }
  return { update, snapshot, dispose, loading };
}
