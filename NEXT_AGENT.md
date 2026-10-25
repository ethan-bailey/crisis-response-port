# Crisis Response Port — current handoff

## Durable checkpoint

- Development branch: `develop/wasm`.
- Source recovery commit: `414478f5e2f9e0a88f9360f8bfc1ef8f7c9fc0a6`.
- This handoff is a follow-on documentation checkpoint; fetch the live branch
  before editing and confirm it contains this handoff and source recovery files.
- Tracked and byte-verified: original Global Context prefab, `.meta`, and
  provenance at `source-reference/Original/CharacterDependencies/GlobalContext/`.
- The historical recovery ZIP was absent, so its recorded archive checksum was
  not rechecked. Individual prefab and meta hashes matched the extracted export.
- No deployment was performed. Site v39 remains the last owner-accepted iPhone
  Safari build. Keep iPhone acceptance open for new gameplay behavior.

## Latest source-recovery evidence

- Recovered `Assets/Resources/Global context.prefab`, Unity GUID
  `64a19dee4df3d6e42a4ce00a87b8cebe`.
- Regenerated export: 2,060 assets, 40,139 records, 24,856 references, 776
  binary assets, 6 unresolved references. Counts match historical evidence;
  the generated package SHA differs from the historical hash, so no source
  identity claim is based on the generated package hash.
- A serialized-reference traversal from original Mission, MissionUI, and
  GlobalContextInitializer records selected 7,126 of 7,141 Mission 01 records.
  It does not isolate a small source-preserving UI slice.
- Compact execution view for Global Context + Operator: 1,204 assets, 7,367
  records, 59,941,213 JSON bytes / 33,355,400 gzip bytes. Adding full Mission
  01: 1,918 assets, 22,361 records, 160,546,681 JSON bytes / 91,250,300 gzip
  bytes. Detailed measurements: `docs/evidence/GLOBAL_CONTEXT_SOURCE_RECOVERY_2026-10-25.md`.

## Integration constraint discovered

`SceneLoader` assigns serialized fields synchronously. `SerializedAssetResolver`
requires each referenced asset to already exist in the package, and recursive
prefab loads use the same package synchronously. Fetching a missing asset from
the browser during C# field assignment cannot work as-is. Current closure-based
packages therefore need the full reachable package before scene activation.
Do not call the existing 12 MiB Operator-only package a same-world gameplay
package; it does not contain Global Context or Mission UI.

No .NET build, contract suite, browser smoke, or iPhone Safari test was run in
this recovery batch. Nothing was deployed.

## Exact next action

Design and implement an incremental package store that preserves synchronous
C# asset resolution while accepting verified source asset batches from the
browser before activation. Keep all original GUID/fileID references, source
hashes, and component fields. Measure peak browser memory and bytes required
before the first actor input. Do not weaken resolution errors, drop serialized
fields, or create a competing actor. Then load original Global Context,
Mission/MissionUI/InteriorModeUI/InteractionMenu, and Operator into one
RuntimeWorld; only after the existing Mission UI actor contract passes should
page input, PhysX results, rendering snapshots, and weapon callbacks be routed
through that actor. Commit verified work directly to `develop/wasm`, fetch the
live branch to confirm it, and do not deploy before the release gate and owner
iPhone Safari acceptance.