# Global Context source recovery — 2026-10-25

## Source and identity

- Recovered export directory:
  `/tmp/cpr-recovery-artifact-11903811738/export/ExportedProject`.
- The source ZIP was absent in this session, so its historical SHA-256
  `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d` was
  not independently rechecked. This checksum is recorded by the existing
  source combat evidence and actor-prefab provenance.
- The recovered prefab and `.meta` were byte-compared with their repository
  copies. Prefab SHA-256:
  `42f863d231c8e32521df43e2e0b895df7eb9701afbc8fa7f58ec62e5717b5085`.
  Meta SHA-256:
  `47d01bc5124fff9c1e9a265c5c3bbbecac60ef12f0306cc778b372e6fa6af1be`.
- Preserved Unity GUID: `64a19dee4df3d6e42a4ce00a87b8cebe`.

The exact authored prefab and `.meta` now live at
`source-reference/Original/CharacterDependencies/GlobalContext/`, with
provenance beside them. The full recovered export was regenerated to
`/tmp/cpr-scenes-full-source.json`: 2,060 assets, 40,139 records, 24,856 object
references, 776 binary assets, and six unresolved references. Its generated
JSON SHA-256 was `6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639`;
the historical extraction evidence recorded a different package hash despite
matching counts, so the generated package hash is not treated as a source
identity claim.

## Same-world package sizing

Dependency traversals over the regenerated source package measured:

- Global Context + Operator closures: 1,204 assets, 7,367 records,
  81,208,406 JSON bytes and 36,098,403 gzip bytes.
- Global Context alone: 1,144 assets, 6,925 records, 70,123,026 JSON bytes and
  32,111,172 gzip bytes.
- Mission 01 alone: 825 assets, 15,341 records, 144,339,464 JSON bytes and
  66,876,638 gzip bytes.

The Global Context + Operator closure is larger than the current bounded
Operator-only delivery limits (12 MiB compressed / 32 MiB expanded). The
Mission 01 closure is larger still. `FullSourceActorBridge` also refuses to
tick without the original Mission InteriorMode UI and InteractionMenu in the
same runtime world. The next package design must include that authored UI while
keeping normal phone startup bounded; an isolated or synthetic actor is not an
acceptable substitute.

Further source-preserving slice measurements:

- Starting from the original `Mission`, `MissionUI`, and
  `GlobalContextInitializer` component records in Mission 01, following their
  local fileID references and complete GameObject component lists selected
  7,126 of 7,141 scene records (1,522 GameObjects and 1,131 MonoBehaviours).
  This graph traversal does not isolate the playable UI.
- A compact execution-view package for Global Context + Operator, retaining
  parsed fields, binary data, asset hashes, references, and required GUID maps
  while removing redundant raw YAML/source-text copies, measures 59,941,213
  JSON bytes and 33,355,400 gzip bytes.
- Adding the full Mission 01 root closure to that execution view measures
  160,546,681 JSON bytes and 91,250,300 gzip bytes across 1,918 assets and
  22,361 records.

These measurements leave no source-preserving, fixed-size Mission UI bundle
under the existing 12 MiB / 32 MiB budget. The next implementation needs a
source-faithful way to defer serialized asset resolution or isolate the exact
mission UI dependencies without dropping original record references.

## Verification

- `scripts/extract_runtime_scene.py` completed against the recovered export and
  reported 2,060 assets, 40,139 records, six unresolved GUIDs.
- Recovered Global Context prefab and `.meta` byte comparisons passed.
- Package sizes above were measured by following package-resolved object
  references from each listed root; they are packaging measurements, not
  gameplay acceptance.
- No .NET build, browser smoke, package deployment, or iPhone Safari test was
  run. Nothing was deployed.
