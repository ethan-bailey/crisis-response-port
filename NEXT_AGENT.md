# Crisis Response Port — handoff

## Checkpoint

- Branch: `develop/wasm`.
- Runtime source tested: `90f9f67ceb8fb6c01ed37742e346196c07c5e395`.
- Documentation head before this checkpoint: `937300033c6ad585dfd71107ddb5564c78bcb78e`; this handoff/evidence update is docs-only.
- Gate 1 evidence: `docs/evidence/GATE1_CHARACTER_ACCEPTANCE_2026-10-23.md`.
- Gate 7 evidence: `docs/evidence/GATE7_MISSION_NPC_ACCEPTANCE_2026-10-23.md`.
- Gate 2–6 source and blocker record: `docs/evidence/GATES_2_TO_6_PROGRESS_2026-10-23.md`.
- Read `AGENT_INSTRUCTIONS.md` and `docs/DOTNET_BUILD_RUNBOOK.md` before .NET work.
- Pinned .NET SDK: 8.0.426; runner shim: `/tmp/libprocshim.so`.
- Avoid Codespaces through 2026-11-01. GitHub Actions remain disabled.
- Preserve any excluded untracked scene package cache; never add it to Git.

## Owner's eight-gate order and status

1. **Operator lifecycle — passed.** Original Operator prefab completes lifecycle and preserves its original components and 19 limb identities; see Gate 1 evidence.
2. **Occupant lifecycle and ownership — passed in source contracts.** Original Occupant body/limbs/physiology state is independent of Operator state; see Gate 1 and Gate 2–6 evidence.
3. **Movement and animation — partial.** Original movement, posture, animation parameter, and force-request paths pass headless contracts. Player input is blocked by the disabled InteriorMode UI in the headless mission fixture; native physics and visible animation remain unverified.
4. **Equipment and firing — partial.** Original arm sequences, reload and bolt-lock contracts pass. Live firing/aiming and browser composition remain open.
5. **Hit and injury chain — partial.** Original FullSource projectile limb callbacks and injury source exist, but live projectile-to-limb execution is unverified; analytic capsule ray queries reject sheared transforms without native PhysX geometry.
6. **Body-state outcomes — partial.** Original unconsciousness, ragdoll dispatch, and timed death are exercised. Physical ragdoll solving, live weapon drop, and full destruction sequence remain open.
7. **Mission and NPC flow — passed at the bounded headless source-runtime scope.** Mission 01 normally initializes in RuntimeWorld with original actors; six original behavior trees run for 60 ticks; original room-clear, Occupant disable/ragdoll, mission success, and authored success-menu callbacks pass. The exact scope and limitations are recorded in Gate 7 evidence.
8. **Combined acceptance — not started.** Requires linked two-actor gameplay plus exact-source WASM/browser and iPhone Safari acceptance.

Gate 7's pass does not close remaining partial details in Gates 3–6. Do not claim full gameplay parity or Gate 8 completion.

## Gate 7 verification

Using SDK 8.0.426, `/tmp/libprocshim.so`, and
`/tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json`:

- `--mission-ai-contract`: passed; Mission 01 started, 2 Operators and 5 Occupants (8 CharacterBody instances), six active original behavior trees with deserialized roots, 60 AI ticks, 240 recorded force commands.
- `--mission-ai-outcome-contract`: passed; seven source room clears, all five Occupants ragdolled, all-disabled event fired once, mission success computed, PauseMenu initialized, camera depth-of-field found, and authored end menu visible.
- `git diff --check`: passed before this docs-only checkpoint.
- Runtime source SHA remains `90f9f67`; no source edits were made for this gate.

The harness uses silent audio and headless presentation/physics adapters. The AI roots remain `Running`; force commands are observed but not integrated by native PhysX. Unsupported scene components and unresolved built-in presentation/resource fields remain. The Gate 7 pass verifies mission/NPC source flow only.

## Errors and blockers

- Gate 7 commands completed with no compile errors or runtime exceptions; the full runtime emitted its existing compiler warnings.
- The earlier input probe failed at `PlayerController.Update` because `InteriorMode.UI.InteractionMenu` was null. Activating the full authored UI stalled mission actor spawning. Do not repeat that workaround until the specific UI bootstrap dependency is isolated.
- The FullSource projectile hit/injury implementation is present. The unsupported `HandleLimbHit` is in a distinct transport partial. A live source hit still needs initialized original `LimbRenderer` dependencies and successful ray geometry; see Gate 2–6 evidence and the .NET runbook.
- Scene contracts still report unsupported presentation/physics components and unresolved serialized resources. Keep these separate from compile diagnostics.

## Next action

Begin Gate 8's combined acceptance by connecting the original Operator equipment/fire path to an Occupant's original projectile/limb injury and body-state outcome path in Mission 01. First build a focused source contract for that linked interaction, preserving original gameplay and reporting exact source/physics dependencies. Do not publish or claim browser/iPhone acceptance until the exact-source WASM and browser checks and physical iPhone Safari acceptance pass.
