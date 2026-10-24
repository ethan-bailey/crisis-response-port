# Original source combat integration milestone — 2026-10-24

## Result

At `develop/wasm` checkout `729b2c2447ea8c89f0906da8633bd11fc1402158`, the recovered original project was regenerated into a temporary scene package and the headless source contracts passed for actor lifecycle, original weapon/arm actions, projectile-to-limb injury, Mission 01 startup, behavior-tree initialization, and mission outcome. This is a source combat integration milestone; it does not close Gates 0, 2–7 or claim browser, visual, native-physics, or autonomous-AI parity.

Gameplay source remains `e009c00d240b63e67d29f9499d4740bbc6669f55` (source tree `6b8011f1b30691981a9a484fe8865b545ff6d68d`). The pinned recovery archive was SHA-256 verified as `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.

## Reproducible source package

The Git-tracked trimmed WASM scene package does not include `Assets/Resources/Global context.prefab`. The pinned recovery archive does. Extract the original source and regenerate the package in a temporary directory:

```sh
mkdir -p /tmp/cpr-recovery-artifact-11903811738
unzip -q -o /tmp/cpr-original-recovery.zip 'export/ExportedProject/*' -d /tmp/cpr-recovery-artifact-11903811738
python3 scripts/extract_runtime_scene.py \
  /tmp/cpr-recovery-artifact-11903811738/export/ExportedProject \
  /tmp/cpr-scenes-full-source.json
```

The generated package reported 2,060 assets, 40,139 records, 24,856 object references, 776 binary assets, and six unresolved references; its SHA-256 was `c63ec4f4c04e8c6d682ab00886d558d310ecd776d14c8de23c0065b9e25c8025`. All six point to one absent source Mesh GUID, `b4b69dc6c74eeb54cbd22d28e37f9a19` / fileID `4300000`, used by the .308 casing and round prefabs. The recovered project has no matching `.meta` owner. This is outside the M4 5.56 test path, but remains a known Gate 0 dependency for .308 use; do not substitute a similarly named mesh as original.

## Contracts run

All .NET commands used SDK 8.0.426 with `/workspace/libprocshim.so` as recorded in `docs/DOTNET_BUILD_RUNBOOK.md`.

- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore` — passed Runtime contracts, source prefab lifecycle/clone checks, 7 original arm-sequence prefabs / 21 frames, presentation/IK checks, weapon checks (31 initial rounds, empty-bolt lock, reload to 30), and ballistics checks (386 source materials, 61 mesh colliders, penetration and delayed-exit cases).
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- --prefab-lifecycle /tmp/cpr-scenes-full-source.json` — passed Operator and Occupant lifecycle; 134/132 active behaviors reached Awake, OnEnable and Start. A test-owned Equipment fixture was attached, so this is not complete mission equipment proof.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- --spawner-contract /tmp/cpr-scenes-full-source.json` — passed original Operator/Occupant actors, M4A1/Glock-18 ownership, original IK callbacks, directional aim/turn, movement/posture, independent physiology, unconscious/ragdoll/death timing, and PhysX command dispatch. The force sink records requests; it does not integrate native PhysX.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- --mission-ai-projectile-contract /tmp/cpr-scenes-full-source.json` — passed. Original Operator `CharacterBody.PullTrigger` fired one M4 round; the source projectile hit an original Occupant Torso capsule on layer 14. Wounds increased 0→1, damaged tissue 0→101.31728, and body shock 0→524.0983. The contract stages a test lane, disables wound decals, and does not verify rendered pixels or native physics.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- --mission-ai-contract /tmp/cpr-scenes-full-source.json` — passed Mission 01 startup; five original behavior trees had deserialized roots and ran for 60 simulation ticks. The run spawned four Occupants and recorded 204 force commands. Behavior-tree liveness and recorded forces do not establish autonomous decisions, navigation, or PhysX integration.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- --mission-ai-outcome-contract /tmp/cpr-scenes-full-source.json` — passed the source room-clear and success path: seven rooms cleared, four Occupants entered ragdoll, all-disabled fired once, and the authored end menu appeared. Audio used a silent test adapter.

These checks do not exercise a browser or physical iPhone. Occupant count variation from four to five is expected: original `SerializableOccupantSettings` defaults to `minCount=3`, `maxCount=6`, and `OccupantSpawner.CreateSpecificGroup` calls integer `Random.Range(minCount, maxCount)` (upper bound exclusive). It is not a fixed-count acceptance criterion. The older four-tree result is from a different package or fixture; only compare tree counts when the scene package and loaded prefab set match.

## Remaining gate state

- Gate 0: open — the six unresolved edges are classified as references to the absent original .308 Mesh; finish the full source-to-runtime matrix and keep .308 blocked until original identity is recovered or a separately verified substitution is approved.
- Gate 1: passed within scoped headless source-character acceptance; no full CharacterBody parity claim.
- Gate 2: partial — headless arm/weapon contracts pass; source sequence composition and browser visual/physical continuity remain.
- Gate 3: partial — one source shot reaches physiology; wound visuals, native PhysX, full state/event ordering and two-character live integration remain.
- Gate 4: partial — original Mission 01 starts and source spawning/outcome contracts pass; normal HTML startup, authored spawning, rendered actors and real physics remain.
- Gate 5: open — trees deserialize and stay enabled, but autonomous perception, navigation, decisions, and combat are not verified.
- Gate 6: partial — source room-clear/end-menu contract passes; a complete player/AI mission in browser remains.
- Gate 7: open — original presentation and content fidelity remain.
- Gate 8: closed under the owner-approved manual iPhone Safari smoke path for Site v39. It does not close Gates 0–7.

Deferred owner-reported behavior: holding the play area while changing aim can stop sustained fire. Keep it distinct from the held Fire-button fix and investigate later as instructed.
