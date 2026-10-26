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
| Task cancellation/teardown | `ChangeMagazine.OnEnd`, `WorkBolt.OnStart/OnUpdate/OnEnd`, CharacterBody pending action, `WeaponHost.Step` task lifecycle | Completion clears manipulating state and pending action; `OnEnd` may schedule WorkBolt when the bolt is not live and the existing magazine has rounds. Acceptance must include this source callback path rather than treating an empty reserve as an unconditional no-op. |
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

Inventory is complete. The current CI-discovered boundary requires all fixture controls to remain in the contract test; do not edit `runtime/Engine/WeaponHost.cs`. Next implementation: use test-only reflection to add one original `Magazine` to Equipment and set the original `ChangeMagazine.quick` field, then drive the source task and assert quick/empty-reserve paths. No production gameplay behavior is planned to change.

## Hidden dependency discovered — run #69 boundary audit

The first implementation placed quick/reserve fixture controls in `runtime/Engine/WeaponHost.cs`. Node and Chromium passed, but Python failed 3/45 boundary tests because the legacy gameplay-owner audit rejects any change to that file unless a frozen-hash exception is reviewed. This is intentional: the host is explicitly protected as a transport adapter. The Release job stopped before runtime/engine checks, WASM publish, source profile, or package verification; therefore run #69 produced no verified package.

Correction before retry: revert the host edit completely. Keep fixture setup in `runtime/Checks/WeaponContracts.cs` using reflection to access the already constructed host's original M4A1, Equipment reserve list, and ChangeMagazine task. No runtime host or original gameplay behavior change is needed. This setup still drives the original task/IK/bolt ticks; it does not directly mutate reload state. If reflection cannot provide a safe fixture, defer the quick-path test rather than changing the protected owner.


## Hidden dependency discovered — run #70 empty-reserve callback

With the host fixture correctly left untouched and all controls in `WeaponContracts.cs`, Node and Python passed, both Chromium checks passed, and the runtime contract reached the final no-reserve retry. The quick reload through the two reserve magazines and the subsequent no-reserve ammo/task snapshot assertions passed. The final assertion failed because it rejected an `audioBoltReturn` callback. The original source permits an OnEnd→WorkBolt path when the bolt is not live and a magazine is still nonempty. However, the run #70 combined assertion did not establish that this path fired. Run #71's exact event output showed only extract→insert, so active-magazine reselection explains the observed failure. After test setup removes the active magazine from the reserve list, capture and assert only the newly observed callbacks/state. Run #70 stopped before runtime checks completed, Release WASM, full package verification, or artifact retention; no package was produced.


## Hidden dependency discovered — run #71 reserve ownership in the fixture

Run #71 passed Node/Python and Chromium, and the standard plus two quick reload source contracts completed. The empty-reserve assertion then failed with source events `audioExtractMagazine -> audioInsertMagazine`. This was a fixture ownership mismatch: `WeaponHost` explicitly adds its active M4A1 Magazine to `Equipment.PrimaryMagazines`, while its `Snapshot` reserve count intentionally excludes the active object. `Equipment.GetFullestMagazineForWeapon`, however, searches that entire list and therefore treats the active 29-round magazine as available reserve. The observed insertion is not a new source/gameplay bug and the test did not reach a genuine empty-reserve state.

Before the next source test, use the contract's fixture reflection to remove `quickWeapon.Magazine` from `quickBody.Equipment.PrimaryMagazines`, retaining it as the equipped magazine. This represents an actually empty reserve list; then issue the reload and record extract/insert/bolt callbacks, ammo counts, chamber and bolt state. Keep this adjustment in the test only. Do not modify Equipment, WeaponHost, ChangeMagazine, or original inventory behavior to make the test pass.


## Evidence correction — run #70 cause narrowed by run #71

The run #70 failure message combined `audioInsertMagazine` and `audioBoltReturn` in one predicate, so the log did not establish which event caused it. The immediately following handoff inferred WorkBolt as the cause; that inference was unverified. Run #71 printed the exact event sequence `audioExtractMagazine -> audioInsertMagazine` and no bolt-return event, which identifies active-magazine reselection as the observed cause. Keep the source-level OnEnd→WorkBolt dependency in this inventory, but do not claim that WorkBolt emitted during the failing run #70 fixture. With the active magazine removed from the reserve list, record the new exact event/state output before deciding whether that source branch is reached.



## Run #72 — genuine empty-reserve state reached; result needs diagnostics

After the contract removed the active M4A1 Magazine from `Equipment.PrimaryMagazines`, the precondition `HasAmmunitionForWeapon(quickWeapon)==false` passed. The no-reserve retry then failed the bundled postcondition `No-reserve retry changed ammo unexpectedly or failed to settle`. That assertion combines active magazine count, reserve count, shot count, total magazine+chamber count, task state, and manipulation state; the job log does not identify which value differed. The test is no longer reselecting the active magazine, but the final source behavior is not yet characterized. Before changing expected gameplay results, add each before/after value to the thrown message and success record, then rerun the exact source contracts. Run #72 passed Node/Python and Chromium completed separately; Release runtime/package gates stopped at this runtime failure, so no package was verified.


## Run #73 — exact genuine-empty-reserve result

After the test removed the active magazine from the reserve collection and verified `Equipment.HasAmmunitionForWeapon == false`, the source task produced this exact before/after report: before: magazine 29, chamber live true, total magazine+chamber rounds 30, bolt Closed, shots 61. After: magazine 0 (source Magazine reference was cleared), reserve 0, chamber live true, total rounds 1, bolt Closed, shots 61, reloading false, manipulating false. Events: `audioExtractMagazine` only. Thus the original task first removes/drops the detachable magazine, then sees `hasMoreAmmunition=false` and returns at State3; it does not insert or cycle the bolt. This loses the remaining magazine rounds, but it is the recovered source ordering and is now the contract boundary for this deliberately requested empty-reserve path. Do not silently redesign it in a parity test.

Run #73 passed Node/Python and Chromium IndexedDB/audio, then failed only because the diagnostic harness still expected magazine=29. Update the postcondition to the exact source result above and rerun. Release WASM/package checks did not run on #73; no package was built or verified.
