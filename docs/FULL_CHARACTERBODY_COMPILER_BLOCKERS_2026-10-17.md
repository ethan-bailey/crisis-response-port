# Full original CharacterBody — concrete compiler dependency closure

**Baseline:** captured `compile-closure` GitHub Actions [run 38967474639](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38967474639), job `117197426524` (**FAILED**, before the later ArmController alias fix). Exact developer-authored 2,454-line class: `runtime/Original/CharacterBody.Full.cs` in `Crisis.Runtime.FullSource`; its original method logic must not be replaced with guessed implementations.

**Latest applied targeted correction:** [commit 0ebcf6b](https://github.com/ethan-bailey/crisis-response-port/commit/0ebcf6b9556be5fc879b5e39f830f5b13d73b54c) removes a `using CharacterBody` alias colliding with the global source name in `runtime/Original/Animation/ArmController.cs`. It qualifies the existing `Crisis.Runtime.Weapons.CharacterBody` references explicitly while preserving runtime method logic. The [fast source/package run 39081956735](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39081956735) **SUCCESS** is static/source verification only, **not proof that full CharacterBody compiles**. Do not infer that the two CS0576 messages are solved until a corresponding compiler test passes.

## Actual errors from the failed compile

| Kind | Source symbol(s) (first reported source line when relevant) | Existing basis | Next exact resolution |
|---|---|---|---|
| Behavior Designer integration | `BehaviorDesigner.Runtime` namespace (3) | External Unity Behavior Designer runtime/task assets; not one of the 31 `CharacterDependencies` scripts | Recover developer-used plugin API, serialized AI tasks, and behavior lifecycle; implement exercised compatibility against original code; do not add a namespace-only stub. |
| Game logic: mission/sensing/audio | `Mission` (295), `Communication` (297), `CharacterDialog` (307), `CharacterAudio` (317), `AudioSurface` (321), `Room` (361), `TimeScaler` (199), `Target` (323) | Preserved original scripts under `source-reference/Original/CharacterDependencies/` | Map each script to an executable source class and then restore real Mission/Time/Audio/BehaviorDesigner/physics services required by those classes. |
| Game logic: equipment/action/IK | `WeaponMode` (69), `IKArmController` (163), `WeaponAimer` (299), `Grenade` (309), `SerializableEquipment` (828), `SerializableExtraSettings` (750), `Settings` (253), `ArmorClass` (917) | Original source recovered, partial weapon/IK engine adapters exist across #56 and #57 | Reconcile source type identity and all required action callbacks; do not copy identical class names as unused parallel systems. |
| Game logic: bodies/physics/visuals | `MuscleControl` (305), `CharacterScoreCard` (311), `Physiology` (397), `LimbId` (171/417), `Emitter` (94), `FloorStreakPainter` (97), `DynamicInteractionMarker` (203), `DoorController` (1542), `BallisticObstacle` (1697), `DialogTree` (822) | Preserved in the original script manifest; some selected source functions are already implemented in #56 | Bind author components to real source CharacterBody ownership and engine graphics/physics; preserve event/RNG order. |
| Unity engine adapter | `CapsuleCollider` (201), `Animator` (211), `SpringJoint` (249), `Collision` (1830) | `recovery-manifest.json` classifies these four as Unity engine types, **not missing original game scripts** | Port only the exercised authentic Unity adapter semantics (physics, collision callbacks, Animator state, joints), with focused source tests and PhysX evidence. |
| Global-name/alias collision | `ArmController.cs` CS0576 near (40,78) | Source-adapter collision between global `CharacterBody` and prior type alias | Explicitly-qualified owner type committed at `0ebcf6b`; compile to verify. |

Compiler evidence includes **31 distinct missing types/namespaces** in `CharacterBody.Full.cs`: 26 recovered game symbols, four Unity types, and one external Behavior Designer namespace. The original source recovery manifest lists **31 developer script files** overall (not all reported as missing in the initial compile). This first-pass error list will expand as dependencies are linked. It is not a comprehensive transitive graph, and it is from the exact older failed run, **not** a fresh compile of the modified head.

## Dependency-ordered execution

1. Re-run the focused **full source** compiler probe at current branch head to validate the alias repair and regenerate error evidence; avoid a complete WASM rebuild for this compiler-only stage.
2. Resolve actual engine type identity without fake placeholders (Behavior Designer task lifecycle; `Animator`, `CapsuleCollider`, `SpringJoint`, `Collision`), using source/plugin contracts and real PhysX/animation integration.
3. Recover and incorporate original source dependency families incrementally from the **pinned private archive** and `source-reference/Original/CharacterDependencies`, verifying each family against archived hashes and recording true source ownership. Source only in `source-reference/` does not make a class executable.
4. Resolve nested dependencies one family at a time; guard against shadow `CharacterBody` ownership and duplicate `CharacterScoreCard`, equipment and physiology implementations already in #56.
5. Pass focused native original method, event and physics tests, then compile complete recovered `CharacterBody` and verify its **live integration** to Player/Occupant classes rather than treating the isolated `FullSource` class as the active game.
6. Only after stable native compile/behavior gate, run full exact-SHA .NET 8 WASM + PhysX + Chromium/WebKit once, with screenshot and action proof.

## Nonnegotiable invariants

- Do not "fix" CS0246 by adding no-op dummy implementations, skipping source methods or silently dropping original class branches.
- Preserve the original circular iPhone movement control, analog proportional walk/sprint (16% dead zone, 65% breakpoint), keyboard full speed, icon buttons, and magazine ammo count inclusive of chamber.
- Keep #42 draft until full source integration passes; #56 and #57 remain separate owners for wound/physiology and weapon-arm logic.
- This document is a **Gate 0 error inventory**, not a working CharacterBody claim. The [canonical foundation-first master plan](https://github.com/ethan-bailey/crisis-response-port/blob/develop/wasm/docs/FOUNDATION_FIRST_COMPLETION_PLAN_2026-10-17.md) controls milestone priority.
