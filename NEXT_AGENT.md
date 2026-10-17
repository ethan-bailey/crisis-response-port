# NEXT AGENT — Crisis Response Port
**Handoff:** 2026-10-17  
**Working branch:** `feature/original-projectile-limb-hit-runtime` — [draft PR #56](https://github.com/ethan-bailey/crisis-response-port/pull/56)  
**Baseline integration:** `develop/wasm` (recheck before modifying/merging)

## Start here
Read `AGENTS.md` §9. **Replace the entire contents of this file at every handoff; do not append.** Recheck GitHub PRs, HEADs and CI. Preserve original developer gameplay/assets, branch privacy, existing approved circular iPhone joystick, and chamber-inclusive ammo counter. This feature is separate from [IK PR #57](https://github.com/ethan-bailey/crisis-response-port/pull/57), [full CharacterBody PR #42](https://github.com/ethan-bailey/crisis-response-port/pull/42), and legacy Unity [PR #7](https://github.com/ethan-bailey/crisis-response-port/pull/7).

## This handoff's concrete code changes
In `runtime/Engine/ProjectileFlightHost.cs` on PR #56, commits `9b0199a` and `2d4d491` added pre-interaction fail-closed checks for projectile owner collision, mismatch between hit limb and actual character limb registry, and missing initialized source blood/oxygen physiology. The checks occur **before** the source `Limb.HandleProjectile` would mutate tissue. The previous `HandleLimbHit` dispatch was added at `d027829`, and is still **uncompiled and untested** as a whole. No workflow, browser or deployment run was performed in this handoff.

## What was inspected
- `source-reference/Original/ProjectileInteractions/Projectile.cs`: original handler calls `RecordAttackInfo`, stores victim `LastHitLimb`, calls `OnWasHit`, then invokes onInteraction and onLimbInteraction. On first non-marksman victim, it notifies shooter once per projectile; always notifies victim via `OnWasAttacked`. Recover that ordering, not invented no-ops.
- `runtime/Original/Weapons/CharacterLimbBallisticMethods.cs`: authored penetration/deflection/interior/exit, organ injury and wound calculations run in scoped native fixtures.
- `runtime/Original/Weapons/OriginalCharacterVitalHost.cs`: source `CharacterBody.StepOriginalPhysiology` updates vital dynamics, iterates registered limbs and hemorrhaging, enters source ragdoll at shock/unconsciousness and emits death. **Reuse this existing function** instead of implementing duplicate physiology.
- `runtime/Original/Weapons/CharacterLimbRagdollState.cs`: actual character-owned `limbs`, `EventHit`, `EventUnconscious`, `EventDeath`, source limb force and kinematic transitions.
- `runtime/Checks/Program.cs`: `--source-tissue-penetration` and `--source-limb-interior` cover original material, energy, skin, wound and blood-volume decrement; **not** live shot/attacker notification or fatality tests.

## Verification status and blockers
- Earlier PR #56 source implementation at SHA `69e635579ff1361fad8f0821a6d70af785f38b2b` reportedly passed [native/.NET WASM/Chromium/WebKit run 38993588700](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38993588700). **That result does not validate the newer dispatcher or preflight commits.**
- `Projectile.HandleLimbHit` still needs source-true `RecordOriginalAttackInfo`, `NotifyOriginalVictimHit`, interaction events, victim `LastHitLimb`, shooter success, score/mission/AI and per-swarm behavior; its delegate slots are currently null. Do not connect no-op delegates.
- Live source limb projectile colliders remain intentionally disabled; no validated distinct non-owner target or self-hit mask. Not all `CharacterBody.cs` original methods compile in unified form. Full bullet→wound→blood/oxygen/shock→unconsciousness/death is NOT demonstrated.
- No physical iPhone test or updated browser deploy. `preview/wasm-current-iphone` manifest previously had `sourceCommit=b9454d65f1d2ef164890fe770eff55dcd666cb2e`, predating #56.

## Next ordered actions
1. **Compile and test exact PR #56 HEAD** to catch any source/adapter signatures or fixture regressions caused by the new hit handler and ownership/physiology gates. Do not merge based on old test evidence.
2. Port original attack callbacks and victim state from `source-reference/Original/CharacterBody.cs` and actual mission/score/AI dependencies, preserving developer invocation order. Make the projectile's first-character tracking explicit. Use the recovered `CharacterBody.StepOriginalPhysiology` for post-wound dynamics.
3. Build focused native target fixture with **two different** characters, initialized source physiology, registered limb, actual source material and collider; ensure attacker events and duplicate swarm behavior, no owner collision, wound and blood loss, oxygen/shock and unconsciousness/death transitions. Then exercise original raycast/layer masking in WASM/Chromium/WebKit.
4. Only then activate query colliders and run one milestone browser suite. Verify the package SHA before optional manual Vercel/iPhone deployment. Keep original assets and private CI artifacts safe.
5. Rewrite this file from scratch again at next handoff, preserving earlier versions via Git history.
