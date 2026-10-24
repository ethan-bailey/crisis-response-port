# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; base before this evidence update: `729b2c2447ea8c89f0906da8633bd11fc1402158`.
- Gameplay source: `e009c00d240b63e67d29f9499d4740bbc6669f55` (source tree `6b8011f1b30691981a9a484fe8865b545ff6d68d`). Current owner-only Site v39 remains live at `https://crisis-response-runtime.craftysnipe13.chatgpt.site`; this work did not deploy.
- Owner confirmed Site v39 starts and is playable on iPhone Safari. Gate 8 is closed under that approved manual smoke path. Cloud desktop WebGL gameplay was not verified.
- Owner-reported touch-and-hold-on-screen behavior can stop sustained fire during aim changes. This is distinct from the held Fire-button fix and is deferred per owner direction.
- Pinned SDK: .NET 8.0.426 at `/workspace/.dotnet-8.0.426`; process shim `/workspace/libprocshim.so`. Read `docs/DOTNET_BUILD_RUNBOOK.md` before .NET commands.

## Latest verified milestone

`docs/evidence/SOURCE_COMBAT_INTEGRATION_MILESTONE_2026-10-24.md` records successful current-checkout source contracts regenerated from the checksum-pinned recovery archive. Actor lifecycle, arm/weapon, spawner physiology, source projectile-to-limb injury, Mission 01 startup/AI-tree initialization, and room-clear/end-menu outcome checks passed. The source projectile increased an original Occupant's wounds, damaged tissue, and body shock.

This is a headless source integration milestone, not full gate closure. The regenerated package has six unresolved references to one absent original .308 Mesh GUID (`b4b69dc6c74eeb54cbd22d28e37f9a19`, fileID `4300000`); it is outside the M4 5.56 mission test. Occupant counts of four and five across runs are both consistent with the original `SerializableOccupantSettings` default count range 3–6 and integer `Random.Range(minCount, maxCount)` behavior. The current recovered scene has five running trees; older results with four trees came from a different package or fixture and should not be compared as a fixed count.

## Gate state and next work

- Gate 0: first-playable dependency matrix added at `docs/evidence/GATE0_FIRST_PLAYABLE_DEPENDENCY_MATRIX_2026-10-24.md`; bounded M4 source path classified, full runtime build passed, and spawner/input contracts passed. Broader gate remains open for page startup, native physics, visible original actors, and AI navigation.
- Gate 1 passed within its scoped headless source-character acceptance.
- Gates 2 and 3 partial: browser sequence composition, real rendered injury effects, native PhysX, and complete live hit/state/event integration remain.
- Gate 4 partial: source mission startup/spawn/outcome pass headlessly; automatic normal-page startup, authored visible actors, and real physics remain. The source spawner module requires original settings, spawn data, campaign state, Unity RNG, collision query, and prefab-instantiation callbacks not supplied by `game/index.html`; do not call it with fabricated defaults.
- Gate 5 open: original behavior trees deserialize and stay enabled, but autonomous perception, navigation, decisions, and combat are unverified. The plan's NPC-AI-ready-for-preview milestone has not been reached.
- Gate 6 partial: source room-clear/success path passes headlessly; browser full-mission play remains.
- Gate 7 open: original presentation/content fidelity remains.
- Gate 8 closed for Site v39 under the owner-approved manual iPhone check; it does not close Gates 0–7. No Gate 9 exists in the foundation-first plan.

## Exact next action

Continue Gate 2 by connecting original sequencer playback to the normal browser actor path, preserving the original IKArmController/IKArmSequencer callbacks and actor ownership. Keep the explicit browser limitation visible: this workspace has Playwright packages but no Chromium/WebKit binaries, so code contracts can run but browser acceptance cannot be claimed until a real browser is available. Continue Gates 3–7 in dependency order; classify missing native PhysX/render/audio as blockers and keep those behaviors gated. Do not deploy or change Site access. Keep the play-area hold behavior deferred and recorded. The randomized 3–6 Occupant count is expected; do not treat it as a failure. Keep the absent .308 Mesh identity explicit and do not silently substitute.
