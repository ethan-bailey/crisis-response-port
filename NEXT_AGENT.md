# Crisis Response Port — current handoff

## Checkpoint

- Development branch: `develop/wasm`.
- Runtime source tested: `15d5a0b3a9c24a23b2737596dbb4621c3503e711` (`Load original ammo prefab graph for source weapon fire`).
- This handoff and its Gate 8 evidence are docs-only changes on that tested source tree.
- Open PRs: none (GitHub connector query, 2026-10-23).
- Pinned SDK: .NET 8.0.426; runner shim: `/tmp/libprocshim.so`.
- Avoid Codespaces through 2026-11-01. GitHub Actions remain disabled.
- Preserve the excluded original scene package cache at `/tmp/cpr-original/scene-data/`; do not add it to Git.

## Gate status

1. **Operator lifecycle — passed.** See `docs/evidence/GATE1_CHARACTER_ACCEPTANCE_2026-10-23.md`.
2. **Occupant lifecycle and ownership — passed in source contracts.**
3. **Movement and animation — partial.** Source contracts pass; native physics and visible animation remain unverified.
4. **Equipment and firing — partial.** Original equipment/reload contracts pass. The Gate 8 probe registers one original shot, but does not observe a live projectile afterward.
5. **Hit and injury chain — partial.** The original source collision/injury callbacks are present. Gate 8 has not reached an Occupant limb hit.
6. **Body-state outcomes — partial.** Source unconsciousness/ragdoll/death paths are exercised; live physical outcomes remain open.
7. **Mission and NPC flow — passed at the bounded headless source-runtime scope.** See `docs/evidence/GATE7_MISSION_NPC_ACCEPTANCE_2026-10-23.md`.
8. **Combined acceptance — open.** Live projectile-to-limb injury/body-state, exact-source WASM/browser, and iPhone Safari acceptance remain outstanding.

## Gate 8 checkpoint and landed changes

Evidence: `docs/evidence/GATE8_SOURCE_FIRE_DISCOVERY_2026-10-23.md`.

Commit `15d5a0b` fixes ammunition resource loading to use the original serialized prefab graph. The original M4A1 now resolves `5.56-FMJ`, its source Calibre and FullSource Projectile, and reports ready to fire. The original limb and ballistic masks include layer 14; a direct source-mask torso ray query succeeds. The source trigger/update path increments `ShotsFired` to 1.

The focused probe records one particle emission and two casing force/torque requests through headless adapters. It observes zero live projectile components after the shot, zero Occupant limb hits, and no health or body-state change. Native PhysX and particle rendering/simulation are not claimed. Do not claim Gate 8 completion.

## Verification on tested runtime source `15d5a0b`

- `dotnet build runtime/Crisis.Runtime.csproj --no-restore --no-incremental -v:q`: passed, 0 errors, 48 warnings.
- `--ammunition-resource-contract <package> 5.56-FMJ`: passed; original FullSource ammo, Calibre, projectile, cached lookup, clone lifetime, and fallback checks.
- `--mission-ai-projectile-contract <package>`: Mission 01 started; source fire prerequisites passed; shot count 0→1; no live projectile observed afterward; no victim hit.
- Standard package-driven `Crisis.Runtime.Checks` passed runtime, prefab, arm sequence, presentation, weapon, and ballistic contracts, then stopped at `Program.cs:328` with `Exception: Unresolved supported fields` importing Marksman range.
- `git diff --check`: passed.

## Errors and dispositions

- Diagnostic compile errors (`Mission.scene`, MissionHelper namespace/type, global-vs-FullSource ammunition, and nonexistent `Physiology.Unconscious`/`Dead` members) were fixed with the original Mission object name, explicit FullSource types, and source `Conscious`/`Alive` members.
- Headless particle emission and casing force/torque failures are handled by recording adapters; this does not implement native simulation or rendering.
- Prefab clone runtime delegate failure is fixed by excluding delegate fields from Unity-style serialized cloning.
- `Unresolved supported fields` in Marksman range remains open.
- Most important: source shot registers, but no live source projectile remains observable and no Occupant hit occurs.

## Next action

Trace why `FullSource.Projectile` does not remain active in the Mission world after the registered shot. Keep the investigation within the source fire/clone/update path; then rerun the linked injury contract. Do not publish until the original hit/injury chain and exact-source WASM/browser plus iPhone Safari checks pass.
