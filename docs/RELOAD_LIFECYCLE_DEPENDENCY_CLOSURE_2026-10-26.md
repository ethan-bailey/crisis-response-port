# Magazine reload lifecycle dependency closure — 2026-10-26

## Scope and acceptance boundary

This batch adds source-driven acceptance for the recovered M4A1 detachable-magazine quick reload and the no-reserve repeat reload. It does not claim every authored weapon's reload graph, native joint physics, or iPhone interaction/audio. The active host is a single M4A1 fixture; unsupported weapon-switch cases are listed separately instead of being simulated with fabricated state.

## Pre-implementation dependency inventory

| Dependency | Recovered owner / data | Why the test needs it |
|---|---|---|
| Reload state machine | `runtime/Original/Weapons/ChangeMagazine.cs`, states Begin through State11; `OnStart`, `OnUpdate`, `OnEnd` | Source identity and selected weapon are captured on start. The active-weapon switch guard and fire interruption guard are before the state switch. Quick reload branches after extraction to State3; standard reload uses State2 chest staging. |
| Active M4A1 and source task host | `runtime/Engine/WeaponHost.cs`; source `WeaponBase`, `CharacterBody`, `ArmIKSolver`, `Hand`, task scheduler, `WeaponEffects` | The fixture ticks original arms before the task, starts `ChangeMagazine` on a reload edge, then steps original bolt, trigger, and time. It can check completion, ammo counts, and source events. |
| Magazine ownership and reserve selection | `runtime/Original/Weapons/Equipment.cs`, `Magazine.cs`; recovered M4A1 weapon manifest and original 5.56 ammunition package | `HasAmmunitionForWeapon` and `GetFullestMagazineForWeapon` decide whether State3 returns early or traverses State4–State7. Existing test loadout is one active plus one reserve magazine. A two-reserve fixture is required to exercise a second reload after the first consumes the only reserve. |
| Magazine/IK callbacks | `CharacterBody.MoveMagazineToHand`; source arm transitions and authored IK curves embedded in the weapon contract | Quick reload moves the magazine directly to the hand path, while standard reload stages the old magazine at the chest. Completion must wait for `OnTarget`; no frame or transition is skipped by the acceptance. |
| Source event/audio mapping | `WeaponBase.RemoveMagazine`, `InsertMagazine`, `CyclingBolt.Open/Close`; `WeaponEffects.Emit`; `game/weapon-runtime.json.gz`; original extract/insert/bolt WAVs | A source reload can emit extraction/insertion/bolt callbacks; an empty-reserve retry should not emit insertion or duplicate ammunition. Browser tests already prove each authored source key maps to decoded original audio; this batch checks emitted events from the source task only. |
| Task cancellation/teardown | `ChangeMagazine.OnEnd`, CharacterBody pending action, `WeaponHost.Step` task lifecycle | Completion clears manipulating state and pending action; early termination may request WorkBolt. Acceptance checks the task settles and source state remains coherent, without inventing a weapon switch. |
| Regression suites / package | `runtime/Checks/WeaponContracts.cs`; source-contract workflow and full Release/profile/package verifier | The same exact SHA must pass Node, Python, runtime, engine, foundation, Release WASM, source/profile/package integrity, and Chromium IndexedDB/audio jobs before any publishing decision. |

## Lifecycle paths to verify in this batch

1. **Quick detachable reload with two reserve magazines available:** drain the initial M4A1 magazine using the existing source fire loop; start `quick=true`; tick the original IK/task/bolt sequence until task completion; assert the source inserted a full magazine, the chamber is live, no extra shots occurred, extraction precedes insertion, no duplicate insertion event occurred, and the expected one spare full reserve remains.
2. **Repeated reload with no reserve:** request another quick reload after the last reserve is consumed; tick to completion; assert magazine/chamber/reserve counts do not gain rounds, task/manipulation state clears, and no insert event is emitted. Preserve the original source's extraction callback if it emits one; do not suppress or fabricate it.
3. **Existing standard reload:** retain the prior 30+1 empty-to-live reload contract and timestamped extract→insert→bolt-return ordering.
4. **Quick reload event mapping:** compare source event names with manifest source keys. This is a source-callback check; actual browser decoding/playback remains covered by the separate Chromium job, not this runtime test.

## Exact unreachable paths and next dependencies

- The current M4A1 manifest selects a detachable-magazine path; after insertion it completes at State7. The fire interruption condition is only active for `state >= State9`, used by the internal-magazine path. The current host rejects unsupported internal-magazine return/feed operations, and only loads the M4A1. Therefore fire-during-State9+ is unreachable in this fixture.
- Weapon switching is also untestable: `WeaponHost` constructs exactly one active M4A1 and the fixture has no source SecondaryWeapon prefab, weapon-swap action host, or switch task. To verify this guard, first inventory and recover a supported second source weapon, its dependencies, and the original swap task; then drive the source active-weapon change at each reachable reload phase. Do not use a fake weapon reference or direct state mutation as acceptance.
- Interrupted audio playback and quick repeated gesture timing on iPhone remain device tests. Chromium tests do not prove speaker output or iOS Safari timing.

## Hidden-gap protocol

If the source test shows unexpected ammo changes, state completion, or event ordering, record the actual task state, source counts, and emitted event sequence here and in the gate index before changing code. Keep test-harness-only controls explicit; do not change original task behavior to simplify a contract.

## Status before implementation

Inventory is complete. The next implementation is limited to explicit contract-fixture controls for reserve count/quick mode plus source-driven quick and empty-reserve reload assertions; no production gameplay behavior is planned to change.