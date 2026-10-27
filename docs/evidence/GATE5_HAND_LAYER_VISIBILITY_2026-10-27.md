# Gate 5 hand layer visibility — exact source evidence (2026-10-27)

## Exact source and workflow

- Branch: `develop/wasm`
- Tested and remote SHA: `530c483266cf58d4ac6b9b65cb82ccb867c51643`
- Actions run #128: `40444437888`
- Main job: `121992691908` — succeeded
- Independent Chromium job: `121992692081` — succeeded
- SDK: .NET 8.0.426
- Commit tree: `652b7315fb27786cc56193d7834484d6bb0c02c9`

## Source-owned behavior and cause

The original `Hand.SetMode` method changes active state on the hand-mode GameObjects (fist/trigger/forward-assist). Recovered hand Quad renderers are created by `game/original-hand-renderers.mjs` as children of those original nodes. Their initial visibility comes from the serialized renderer enabled property. Before the fix, `FullSourceActorBridge.Snapshot` exported transforms but not `GameObject.activeSelf`, so the page could project updated transforms while retaining stale hand-layer visibility.

The regression cause is falsifiable: after an original source hand-mode transition, the FullSource snapshot lacked the source node active state, and the projected node could not become visible/invisible in sync. The fix adds `activeSelf` to each mapped source transform snapshot and assigns it to the corresponding projected group’s `visible` state. It preserves the original Hand owner, source hierarchy, and child mesh ownership.

Relevant code and regression:
- `runtime/Original/FullSource/Hand.cs` — original hand mode ownership.
- `runtime/Engine/FullSourceActorBridge.cs` — source snapshot `activeSelf`.
- `game/index.html` — source active-state projection.
- `game/original-hand-renderers.mjs` — meshes remain under original hand nodes.
- `scripts/source_character_animator_test.mjs` — regression assertions.

## Acceptance evidence

The source animator Node contract passed inside run #128. Exact CI also passed the packaged source-entry contract, original hurt-audio deferred-decode contract, Mission UI/projectile/physiology/outcome/weapon/reload/BehaviorDesigner/AI fallback contracts, Release/WASM output, complete Mission package integrity and retention, and source recovery/index provenance.

The package profile contains 1,919 assets and 22,365 records; package has 2,688 files, 199 Blazor resources and 195 WASM files. Source index SHA-256: `9d866276582031d440c316c8e60d69d252506cf67a8ae9d03dbdbe0d91980f92`. Recovery archive SHA-256: `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. Deterministic package tar SHA-256: `cc97c7117c0d972db9da40ec0c67f7f9623ecd544beec88771a7c4b2c5a0d4fc`. Retained artifact ID `12708155836`, size 138,602,914 bytes, SHA-256 `def8f8a835647f2367c48136c8fc6a86aa81d7aa3387c62d6fef3d7b2caf1692`.

Full-profile production-loader acceptance in real Chromium IndexedDB passed with 1,919/1,919 persisted, 14,440ms cold, 10,261ms cached, 1,919 cold requests, zero warm requests, and 1,919 warm hits. The Blazor bridge was modeled.

## Limits and next test

This proves source active-state transport and a Node-level regression, not rendered arm IK, gun grip, post-download Mission-page pixels, or iPhone Safari. The existing Site remains v85; no publish was made in this recurring run.

A separate source geometry audit predicts a grounding defect: Operator spawn Y=1.5, capsule center offset=-0.64 and height=1, Mission floor top Y=0. The capsule bottom is rootY-1.14 while the inspected visual foot/toe chain is near root Y. This predicts an actor root around 1.14m above floor if that source capsule is used for contact settlement. It is not yet a measured page result. The next regression/diagnostic must record the same actor root, capsule center/bottom, floor top, browser contact output, and projected render root before/after contact settling on the production Mission path. Compare against the original scene, and do not introduce a synthetic mesh offset without original-source evidence.

Owner device acceptance remains: open the exact authorized release on iPhone Safari; verify full Mission load/interaction, grounded rendered pose, hand/gun alignment through movement and aiming, touch feel/zoom, audio output, cold/warm storage, and sustained frame pacing. Do not mark it passed from Chromium or source contracts.
