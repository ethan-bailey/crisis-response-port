# Crisis Response Port — current handoff

## Checkpoint

- Active branch: `develop/wasm`.
- Source recovery implementation commit: `de7ab755ba63b18051c5302203b5788e434e4a3f`.
- The handoff checkpoint commit follows this implementation commit.
- The exact original Global Context prefab and `.meta` were recovered from the
  mounted extracted export and copied into `source-reference/Original`.
- The historical recovery ZIP is absent in this session; its documented SHA was
  not rechecked. Individual prefab and meta hashes and byte comparisons were
  verified against the mounted export.
- No deployment was performed. Site v39 remains the last owner-accepted iPhone
  Safari build. Keep target-device acceptance open for new gameplay behavior.

## Latest source recovery batch

- Recovered `Assets/Resources/Global context.prefab`, Unity GUID
  `64a19dee4df3d6e42a4ce00a87b8cebe`, at
  `source-reference/Original/CharacterDependencies/GlobalContext/` with `.meta`
  and provenance.
- Regenerated package from the recovered export: 2,060 assets, 40,139 records,
  24,856 references, 776 binary assets, and 6 unresolved references. Counts
  match historical evidence; regenerated package hash differs from the
  historical output, so no package identity claim is made from that hash.
- A serialized-reference traversal from original Mission,
  MissionUI, and GlobalContextInitializer records selected 7,126 of 7,141
  Mission 01 records. It does not yield a small, source-preserving UI slice.
- Global Context + Operator execution-view closure: 1,204 assets, 7,367 records,
  59,941,213 JSON bytes / 33,355,400 gzip bytes. Adding full Mission 01 yields
  1,918 assets, 22,361 records, 160,546,681 JSON bytes / 91,250,300 gzip bytes.
- Details: `docs/evidence/GLOBAL_CONTEXT_SOURCE_RECOVERY_2026-10-25.md`.
- No .NET build, browser smoke, package deployment, or iPhone Safari test was
  run. Nothing was deployed.

## Exact next action

Implement source-faithful deferred serialized-asset resolution for the original
Mission UI package, using exact GUID/fileID references and existing source hashes.
Keep original Mission, MissionUI, InteriorModeUI, InteractionMenu, Global
Context, and Operator components in one RuntimeWorld. Measure the assets
required before first actor input and set an explicit on-demand byte budget
before wiring the page. Do not strip component fields, drop dependencies,
replace source records, or create a second actor to make the package smaller.
Once the same-world source path passes the existing Mission UI actor contract,
wire page input, PhysX results, actor snapshots, and weapon callbacks through
that actor; run combined checks on one SHA. Do not deploy before the release
gate and owner iPhone Safari acceptance.
