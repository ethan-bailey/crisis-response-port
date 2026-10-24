# Crisis Response Port — current handoff

## Checkpoint

- Development branch: `develop/wasm`.
- Runtime source tested: `1a50b671c8766a7e610134d65666357691e7cb98` (`Record Gate 8 source fire findings`), with uncommitted Gate 8 projectile/injury probe updates in the current workspace.
- Open PRs: none (GitHub connector query, 2026-10-23).
- Pinned SDK: .NET 8.0.426; runner shim: `/tmp/libprocshim.so`.
- Avoid Codespaces through 2026-11-01. GitHub Actions remain disabled.
- Preserve the excluded original scene package cache at `/tmp/cpr-original/scene-data/`; do not add it to Git.

## Gate status

1. **Operator lifecycle — passed.** See `docs/evidence/GATE1_CHARACTER_ACCEPTANCE_2026-10-23.md`.
2. **Occupant lifecycle and ownership — passed in source contracts.**
3. **Movement and animation — partial.** Source contracts pass; native physics and visible animation remain unverified.
4. **Equipment and firing — partial.** Original equipment/reload contracts pass. The Gate 8 source contract fires through the original trigger and weapon path.
5. **Hit and injury chain — partial.** The Gate 8 headless probe reaches an Occupant Abdomen and verifies original wound, tissue-volume, and shock updates. Wound decal rasterization remains unverified.
6. **Body-state outcomes — partial.** Source unconsciousness/ragdoll/death paths are exercised; live physical outcomes remain open.
7. **Mission and NPC flow — passed at the bounded headless source-runtime scope.** See `docs/evidence/GATE7_MISSION_NPC_ACCEPTANCE_2026-10-23.md`.
8. **Combined acceptance — open.** Live projectile-to-limb injury/body-state, exact-source WASM/browser, and iPhone Safari acceptance remain outstanding.

## Gate 8 checkpoint and landed changes

Evidence: `docs/evidence/GATE8_SOURCE_FIRE_DISCOVERY_2026-10-23.md` and `docs/evidence/GATE8_SOURCE_PROJECTILE_INJURY_2026-10-24.md`.

Commit `15d5a0b` fixes ammunition resource loading to use the original serialized prefab graph. The original M4A1 now resolves `5.56-FMJ`, its source Calibre and FullSource Projectile, and reports ready to fire. The original limb and ballistic masks include layer 14; a direct source-mask torso ray query succeeds. The source trigger/update path increments `ShotsFired` to 1.

The follow-up trace corrected a false zero-projectile report: the probe had queried the global/transport `Projectile` type instead of `Crisis.Runtime.FullSource.Projectile`. The source projectile is created, registered, owned by the Operator, and enters the original source collision path. An unstaged shot hits an authored roof collider; the contract stages the source Operator inside the roof and aims at an Occupant abdomen. It verifies one Occupant abdomen hit, one wound, tissue volume 3.9282825, and body shock 9.895712 in a passing run. For headless execution, wound decals are disabled on the mission's source limbs so original physiology can run without pretending to rasterize pixels. Native PhysX, decal rendering, particle rendering/simulation, downstream body-state outcomes, WASM/browser, and iPhone Safari remain open.

## Verification on tested runtime source `1a50b67` plus current workspace changes

- `dotnet build runtime/Crisis.Runtime.csproj --no-restore --no-incremental -v:q`: passed, 0 errors, 48 warnings.
- Current workspace: `dotnet build runtime/Crisis.Runtime.csproj --no-restore --no-incremental -v:q`: passed, 0 errors, 48 existing warnings.
- `--ammunition-resource-contract <package> 5.56-FMJ`: passed; original FullSource ammo, Calibre, projectile, cached lookup, clone lifetime, and fallback checks.
- `--mission-ai-projectile-contract <package>`: Mission 01 started; source fire prerequisites passed; shot count 0→1; original FullSource projectile hit an Occupant abdomen; original wound/tissue/shock state increased.
- Standard package-driven `Crisis.Runtime.Checks` passed runtime, prefab, arm sequence, presentation, weapon, and ballistic contracts, then stopped at `Program.cs:328` with `Exception: Unresolved supported fields` importing Marksman range.
- `git diff --check`: passed.
- Current workspace `git diff --check`: passed.

## Errors and dispositions

- Diagnostic compile errors (`Mission.scene`, MissionHelper namespace/type, global-vs-FullSource ammunition, and nonexistent `Physiology.Unconscious`/`Dead` members) were fixed with the original Mission object name, explicit FullSource types, and source `Conscious`/`Alive` members.
- Headless particle emission and casing force/torque failures are handled by recording adapters; this does not implement native simulation or rendering.
- Unbound texture-transfer driver stopped the first original limb hit in the headless test. The contract disables only wound decals on its source mission instance to verify tissue/physiology state; real wound-pixel output still needs a GL backend.
- `victim.Position.x` compile error and anonymous-object member syntax error during probe edits were fixed.
- Prefab clone runtime delegate failure is fixed by excluding delegate fields from Unity-style serialized cloning.
- `Unresolved supported fields` in Marksman range remains open.
- The `Projectile` global/source type ambiguity caused a false zero-projectile observation; explicit FullSource aliases and the spawn event now trace the actual original projectile.

## Next action

Continue Gate 8 from the original wound/injury response into body-state outcomes, then exact-source WASM/browser and iPhone Safari acceptance. Add real graphics and physics backends before claiming visual wound or native physics parity. Do not publish until the combined acceptance gates pass.
