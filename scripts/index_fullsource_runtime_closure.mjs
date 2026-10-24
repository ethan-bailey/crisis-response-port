#!/usr/bin/env node
// Build a provenance-preserving dependency index for the original browser actor path.
import fs from 'node:fs';
import crypto from 'node:crypto';

const [input, output] = process.argv.slice(2);
if (!input || !output) {
  console.error('Usage: node scripts/index_fullsource_runtime_closure.mjs <scene-package.json> <output.json>');
  process.exit(2);
}

const roots = [
  'Assets/Resources/Global context.prefab',
  'Assets/Scenes/Mission 01.unity',
  'Assets/Resources/characters/Operator.prefab',
  'Assets/Resources/weapons/M4A1.prefab',
  'Assets/Resources/weapons/ammunition/5.56-FMJ.prefab',
  'Assets/Resources/weapons/Glock-18.prefab',
  'Assets/Resources/weapons/ammunition/9-FMJ.prefab',
];
const packageData = JSON.parse(fs.readFileSync(input, 'utf8'));
const byGuid = new Map(Object.entries(packageData.guidPaths ?? {}));
const assets = new Map();
const queue = [...roots];
const missingRoots = [];

while (queue.length) {
  const path = queue.pop();
  if (assets.has(path)) continue;
  const asset = packageData.assets?.[path];
  if (!asset) {
    missingRoots.push(path);
    continue;
  }
  const references = (packageData.objectReferences ?? []).filter(ref => ref.source === path);
  const packageReferences = references.filter(ref => ref.resolution === 'package');
  for (const ref of packageReferences) {
    const target = byGuid.get(ref.guid);
    if (target) queue.push(target);
  }
  assets.set(path, {
    sha256: asset.sha256,
    sourceBytes: asset.size,
    records: asset.records?.length ?? 0,
    externalGuidCount: asset.externalGuids?.length ?? 0,
    packageReferenceCount: packageReferences.length,
    packageDependencies: [...new Set(packageReferences.map(ref => byGuid.get(ref.guid)).filter(Boolean))].sort(),
    unresolvedReferences: references.filter(ref => ref.resolution?.startsWith('unresolved') || ref.resolution?.startsWith('unsupported'))
      .map(({sourceFileID, fieldPath, guid, fileID, resolution, objectType}) => ({sourceFileID, fieldPath, guid, fileID, resolution, objectType}))
  });
}

const ordered = Object.fromEntries([...assets.entries()].sort(([a], [b]) => a.localeCompare(b)));
const result = {
  format: 'crisis-response-fullsource-browser-closure-v1',
  sourcePackageSha256: crypto.createHash('sha256').update(fs.readFileSync(input)).digest('hex'),
  roots,
  assetCount: assets.size,
  recordCount: [...assets.values()].reduce((sum, asset) => sum + asset.records, 0),
  missingRoots: [...new Set(missingRoots)].sort(),
  assets: ordered,
  limits: [
    'This is an authored package-reference closure, not proof every runtime component or field is implemented.',
    'Unresolved and unsupported reference classifications are preserved per asset.',
    'Do not package the full closure to the browser without a separate payload-size and runtime-capability review.'
  ]
};
fs.writeFileSync(output, JSON.stringify(result, null, 2) + '\n');
console.log(JSON.stringify({format: result.format, assetCount: result.assetCount, recordCount: result.recordCount, missingRoots: result.missingRoots.length, output}));
