# Crisis Response — foundation-first completion execution plan

**Adopted:** 2026-10-17 · **Target runtime:** original C# on .NET 8 WebAssembly, pinned PhysX and source-authored assets · **Integration:** `develop/wasm`.

This document is the current **execution-order decision**. The older [full game parity inventory](COMPLETE_WASM_PORT_ROADMAP.md) remains the comprehensive scope checklist; this document gives its cross-workstream order, intermediate acceptance gates and branch ownership. The original source and assets must be retained. No invented AI, wounds, equipment, NPC target behavior, mission state or visual substitutes. No parity percentage without tested reference behavior.

## Status and distinctions at adoption

- `develop/wasm` is the canonical integrated game. Open work: [#56](https://github.com/ethan-bailey/crisis-response-port/pull/56) projectile/limb physiology + original Occupant and spawner integration; [#57](https://github.com/ethan-bailey/crisis-response-port/pull/57) IK callbacks; [#42](https://github.com/ethan-bailey/crisis-response-port/pull/42) full 2,454-line original CharacterBody dependency closure; [#7](https://github.com/ethan-bailey/crisis-response-port/pull/7) legacy Unity route, **not** current WASM integration.
- At inspection, #56 was **157 commits / 163 files ahead** of `develop/wasm`. Do not bulk-merge incomplete source/mission/renderer dependencies; extract verified slices to small reviewable integration PRs if useful, preserving Git history and provenance. Do not rewrite published feature history.
- #56 has controlled native and browser evidence for original ammunition/tissue/hemorrhage/hypoxia, independent Occupant C# physics/renderer source-record bridge and focused mission spawn tests. These **do not** establish actual mission auto-spawning, source NPC AI, complete CharacterBody, full ragdoll or live projectile injury.
- Focused original Occupant mission-bridge and source/package checks **passed** at `fb96e92a848409014b85f6e094d995a9376fb731`: runs [39080683868](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39080683868) and [39080683913](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39080683913). That is not a full WASM/browser build. Verified older full browser baseline: [39056615029](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39056615029) on `a41b49cb6182327f792bb0118d51910f197c5d43` (test-only Occupant source record, not mission AI). The currently staged protected iPhone preview is older and has not been physically accepted on iPhone Safari.
- Legacy [operations audit](REPOSITORY_OPERATIONS_2026-10-17.md) contains historic SHAs: inspect current GitHub before edits, merges or release. This plan is **not** a claim of completed implementation.

## Critical path: work in this order, but parallelize independent recovery

### Gate 0 — Source dependencies and reproducible baseline (P0)

Owner: foundation/source closure, with #42 providing original reference. Do first:
1. Freeze exact branch heads and currently passing native/browser gameplay SHA, source archive hash, and protected preview rollback.
2. Inventory the source-to-runtime dependency edges for CharacterBody, Physiology, Muscles, Limb, Animator, IKArmController, Equipment, Mission, TimeScaler, CharacterAudio, Occupants, OccupantSpawner, navigation and Behavior Designer tasks.
3. Classify each dependency as original compiled / original preserved only / engine binding incomplete / absent / tested / unsupported. Name the exact source path, runtime path, owner PR, blocking callsite and cheapest test.
4. Extract required author scripts/prefab GUIDs from the pinned private recovery archive, preserving file provenance. Never change original rule bodies to satisfy a compile.
5. Build a **focused** compiler probe for the current complete CharacterBody graph, capture concrete errors and close them in dependency order. Do not repeatedly launch a full WASM build for simple source closure.
6. Audit NPC Behavior Designer task/tree assets and Unity pathfinding/audio/lifecycle adapters **before** declaring AI implementable.
**Acceptance:** An actionable dependency matrix with zero *unclassified* mandatory dependencies for the first playable scenario, compiling foundation slice and exact native evidence. Known unsupported mandatory dependencies remain explicit blockers.

### Gate 1 — One complete source character (P0)

Owner: #42 / foundation integration, coordinating #56:
1. Unify original CharacterBody source fields/methods into the actual live C# type without shadow health/state implementations.
2. Wire Awake/Start/Update/FixedUpdate source ordering, state, Animator, Physiology, Limb registry and source collision/transform/PhysX adapters.
3. Execute original movement, directional aim, posture, event and character action pathways; retain owner-approved proportional iPhone joystick and full-speed keyboard.
4. Execute source circulation → bleeding → oxygen/shock → consciousness/death with real character ownership and source timing.
5. Verify unique actor identity, independent state and full state cleanup; assert this across two original instantiated characters.
**Acceptance:** Complete scoped original character lifecycle compiles and passes native focused source tests. Do not mislabel this as all-game CharacterBody parity before unsupported branches are closed.

### Gate 2 — Weapon actions, original arm IK and equipment (P0)

Owner: #57 and weapon integration:
1. Close original IKArmController/IKArmSequencer task/action callback coverage rather than leaving default no-ops.
2. Connect original arm/hand transforms, attack/reload/equip/lower/draw/grab/weapon-drop interactions and authored animation events.
3. Bind native equipment ownership and original firearms/magazines/ammunition, preserving correct round-in-chamber UI.
4. Verify source state and IK event ordering with source contracts. At a major playable milestone, have the owner check the exact build in iPhone Safari; desktop Chromium/WebKit runs are optional diagnostics.
**Acceptance:** Original equipped character can complete full weapon sequences and interruptions without visual/physical discontinuities.

### Gate 3 — Physics, injury, ragdoll and life-state graph (P0)

Owner: #56 physiology/ballistics and #42 state graph:
1. Bind source collisions and event preflight to real non-owner source-body limbs, correct masks, closest hit, source hit credit and mission notifications.
2. Finish original source limb entry/exit wounds, organ, nerve, fracture, impulses, muscle/joint responses and blood/oxygen/death paths. Respect exact source RNG consumption; don't invent generated-texture wrapping.
3. Bind production Mission/TimeScaler/CharacterAudio/AudioPropagator/markers and source DropWeapons; reject missing dependencies *before* tissue mutation.
4. Execute exact SetRagdoll/SetUnconscious/SetCharacterState transitions, weapon-drop timing, Target, grenade, voice, releases and events with source-owned physics.
5. Keep all **19 NPC limb projectile-query colliders gated** until those live dependencies pass two-character source integration tests.
**Acceptance:** Real source player ↔ NPC shot-to-physiology-to-state transition with correct event/order and physical outcome. No fake diagnostics counted as live gameplay.

### Gate 4 — Mission startup, world interactions and authoritative NPC spawning (P1)

Owner: mission + Occupant:
1. Load original Global.Context, saved OccupantSettings, EquipmentList and source mission/campaign mode; avoid fabricated defaults.
2. Connect real Unity RNG and PhysX BallisticTargets raycasts, authored source spawn areas/groups/obstruction iteration.
3. Invoke recovered `game/source-occupant-mission-integration.mjs` automatically through **real** `game/index.html` startup; preserve source order and asynchronous registration.
4. Spawn multiple independent original Occupant physics+visual bodies in the real Three.js scene, using author prefab skin/material/animation metadata.
5. Bind authoritative mission/objective/marker/state and noncombat interactions.
**Acceptance:** A fresh normal game launch automatically produces visible real source Occupants at authored positions, with genuine native state and no test-only bridge invocation.

### Gate 5 — Original NPC AI behavior graph (P1)

Owner: new **AI-specific** branch/PR from up-to-date `develop/wasm`, after shared core gates:
1. Recover original Behavior Designer trees/tasks, blackboards, perception/navigation and character/weapon action adapters.
2. Compile the **author** task/action graph with initialization/enable/disable/update order and explicit missing-capability boundaries.
3. Bind source morale, aggression, fear, negotiability, weapon skill, sight/hearing, source audio/line-of-sight/AI awareness and target selection.
4. Run source patrol, investigation, pathfinding/obstruction, cover/movement and timing against actual mission geometry; no invented waypoint choices.
5. Connect draw/aim/fire/reload, threat/compliance/surrender, interruption by injury/unconsciousness/death and teardown to Gates 1–4.
6. Test independent NPCs in controlled seeded deterministic scenarios, then real automatic mission spawn.
**Acceptance:** On a normal startup, original Occupants autonomously perceive, navigate, patrol, decide, attack/react and stop safely on incapacitation. Verified source/AI fixtures and owner iPhone Safari gameplay on one exact SHA at the major milestone. Desktop Chromium/WebKit gameplay is optional diagnostic coverage. **This is the NPC-AI-ready-for-preview milestone.**

### Gate 6 — Complete playable mission and broad parity (P1)

1. Restore original level collision, objectives, doors/props, campaign states, items/weapons, grenades, scoring, scene transitions and source UI/game mode behavior.
2. Validate a full mission from start → objective outcome → transition/reset, including both AI and player.
3. Expand coverage to all original scenes, equipment, character archetypes and interactions. A Mission 01/Marksman demonstration is not entire-game parity.
**Acceptance:** A complete end-to-end mission passes source/runtime assertions, owner iPhone Safari gameplay on the exact milestone build, and reference behavioral comparisons; additional scenes remain tracked explicitly. Desktop browser automation is optional.

### Gate 7 — Original presentation/content fidelity (P2, or earlier when an execution dependency)

Complete original animation/state transitions, Occupant skins, renderer materials/shaders, authored dynamic limb damage/wound decals, blood particles/streams, audio samples/spatial propagation, UI and supported iPhone visual controls. Rendering and source RNG-affecting wound operations must execute before calling Gate 3 accepted. Inventory every remaining scene/content gap.
**Acceptance:** Visual and audible evidence from the corresponding native original wherever available, no missing required assets, correct gameplay event ordering.

### Gate 8 — Consolidate, build, browser/device verify, and release (P0 for each release)

1. Run targeted native/Node source+provenance tests *before* one full integration milestone.
2. Compile .NET 8 WASM and pinned PhysX at an exact gameplay SHA; prove complete package manifest.
3. Run source/Node checks and exact-source package integrity checks. At major playable milestones and before release, have the owner manually verify the exact build in iPhone Safari and record results with the tested SHA. Desktop Chromium/WebKit boot and gameplay checks are optional diagnostics and do not block acceptance.
4. Confirm original mobile analog stick (16% dead zone / 65% walk–sprint breakpoint), icon controls, hand IK, magazine count including chamber, no chamber widget and performance.
5. Only after passing real smoke tests, use **manual-only** protected Vercel staging and branch-stable preview alias. Verify deployed source manifest; obtain physical iPhone Safari feedback. No automatic push deploys.
6. Merge reviewed, passing, independently useful source lanes into `develop/wasm`, retaining original assets and verified rollback; do not merge #42/#56/#57 wholesale while dependencies remain blocked.
**Acceptance:** Verified tested SHA = package source SHA; exact-build iPhone Safari results are recorded at major milestones/releases; remote hosting and actual device experience are separately stated; no unsupported 1:1-completion claims. Desktop Chromium/WebKit results may be recorded but are not required.

## Priority and merge policy

- **P0 now:** Gate 0 concrete source dependency closure, then Gate 1 CharacterBody, Gate 2 IK/equipment and Gate 3 health/physics. Do not postpone small NPC-source recovery that can be done independently.
- **P1 next:** Mission and Occupant startup, then source AI execution, then full mission completion.
- **P2 parallel:** Presentation/asset extraction; promote any rendering/audio task that affects source simulation, physics, AI, RNG or required usability to P0/P1 immediately.
- **PR boundaries:** #42 owns complete CharacterBody; #57 owns source IK callbacks; #56 owns projectile/injury/Occupant scaffolding already implemented. **New AI work gets its own PR**, with no duplicate NPC-spawn adapters. Divide the very large #56 into verified, independent cherry-pick/merge slices when practical, not uncontrolled history rewrites.
- **Testing:** Compiled ≠ booted ≠ playable ≠ original-equivalent. One heavy full-build run at a meaningful milestone, lightweight checks between.
- **Handoff:** Replace the *entire* `NEXT_AGENT.md` on each transfer; never append chronological logs. History remains in Git and dated reports.
- **Preservation:** Private recovery release `recovery-preservation-2026-10-15`, original archive SHA256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`; no public derivative asset publishing.
