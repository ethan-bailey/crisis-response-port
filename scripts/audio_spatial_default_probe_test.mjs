import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const probe=fs.readFileSync(new URL('./unity_2017/AudioSpatialDefaultProbe.cs',import.meta.url),'utf8');
const closure=fs.readFileSync(new URL('../docs/AUDIO_SPATIAL_RUNTIME_DEPENDENCY_CLOSURE_2026-10-27.md',import.meta.url),'utf8');

test('Unity fixture captures omitted AudioSource defaults and exact serialized YAML without guessing',()=>{
 assert.match(probe,/Application\.unityVersion != RequiredUnityVersion/);
 assert.match(probe,/RequiredUnityVersion = "2017\.2\.0f3"/);
 for(const field of ['spatialBlend','dopplerLevel','minDistance','maxDistance','panStereo','rolloffMode','spread','reverbZoneMix'])
  assert.match(probe,new RegExp(`\\b${field}\\s*=`),`probe must capture ${field}`);
 assert.match(probe,/PrefabUtility\.CreatePrefab/,'fixture must exercise the original prefab serializer');
 assert.match(probe,/AssetDatabase\.ForceReserializeAssets/,'fixture must force deterministic YAML emission');
 assert.match(probe,/serializedYamlSha256\s*=\s*Sha256Hex/,'fixture output must bind its exact serialized bytes');
 assert.match(probe,/serializedYamlBase64\s*=\s*Convert\.ToBase64String/,'fixture must retain the inspected YAML payload');
 assert.match(probe,/Temp\/crisis-response-audio-spatial-default-probe\.json/,'fixture output path must be stable');
 assert.match(closure,/AudioSpatialDefaultProbe\.cs/,'dependency closure must name the executable recovery step');
});
