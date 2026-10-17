# NEXT AGENT — Crisis Response Port
Prepared 2026-10-17 UTC. This file completely replaces the previous handoff. Read `AGENTS.md`, especially §9; replace this entire file at the next transfer.

## Current state and branches
The ten requested projectile-to-injury tasks are **not all complete**. PR #56 now compiles and passes controlled native/WASM/browser injury-chain checks. **Live NPC injury gameplay remains disabled; full source character/ragdoll integration and physical iPhone acceptance remain unfinished.**

Repository: private `ethan-bailey/crisis-response-port`. Owner Ethan currently prefers one agent at a time; check current heads and writers before changes.

| Lane | Inspected head | Status / overlap |
| --- | --- | --- |
| `develop/wasm` | `c173099740d685daf597114db5e507527ba2dc7f` before this handoff | Canonical integration; does not contain PR #56 gameplay. |
| PR #56, `feature/original-projectile-limb-hit-runtime` | `eef118868ae185bba5ab5700d7184199d8abede8` before handoff-only commit | Draft, unmerged; latest code tested at SHA below. |
| PR #57, `feature/source-ikarmcontroller-live-callbacks` | `2b73044c107b875d8545d03541bf6b2d35f64654` | Draft; focused native source IK callback run 38994077400 passed; broader/browser integration missing. |
| PR #42, `feature/full-source-characterbody-integration` | `ebf6ef01de3172b72fbab321b4941c1decb3643e` | Draft; original 2,454-line CharacterBody preserved, unified class still does not compile. |
| PR #7, `codex/iphone-input-parity-foundation` | `ff2eed647e4eeb3446453b304bb4cbc4b6d31349` | Separate legacy Unity lane based on main; current focus is WASM. |

Do not bulk merge #42/#57 or rewrite shared branches. Develop changes since #56's integration baseline are documentation/governance, requiring reconciliation before gameplay integration. This handoff is a docs-only replacement on the feature branch and via maintenance PR into develop; it does not merge gameplay.

## Implemented and verified on PR #56
- `runtime/Original/Weapons/CharacterCombatNotifications.cs`, `CharacterScoreCard.cs`, and `ProjectileAttack.Methods.cs` restore original RecordAttackInfo, OnWasAttacked, OnWasHit, compliance/visibility scoring and attacker-success behavior. Original FirstHitCharacter is reassigned for every victim despite its name; non-marksman attacker notification occurs only for the projectile's first victim; ShotsHit deduplicates by swarm ID.
- `runtime/Engine/ProjectileFlightHost.cs` replaces unbound delegates with source calls and projectile events: preflight → tissue → RecordAttackInfo → LastHitLimb → OnWasHit → interaction → limb interaction. Required initialization/capabilities and self-hit rejection occur before tissue mutation. Sorted raycasts skip owner limbs while preserving masks and nearest non-owner hit. All **19 live limb query colliders remain disabled**.
- `CharacterBody.StepOriginalPhysiology` now orders circulation → limb hemorrhage → oxygen/consciousness decisions. `runtime/Engine/Math.cs` preserves Unity's ordered Clamp comparisons when original blood loss produces inverted bounds.
- `runtime/Checks/ProjectileInjuryContracts.cs` exercises real M4 ammunition, authored material and recovered Torso PNG mask through source cavity/wound creation, ongoing bleeding, hypoxia, unconsciousness/death, and an observed impulse. It also checks notification/event order, success credit, source compliance timing, masks and self-hit safety.

These are controlled diagnostics: `liveNpcGameplay=false`. Geometry placement, notification observers and impulse sinks are test fixtures, not genuine NPC spawning, production side-effect bindings or a full PhysX ragdoll. Precision interaction remains unsupported. Do not claim original full-game or visual parity.

Detailed evidence and source dependencies: [PROJECTILE_COMBAT_INTEGRATION_2026-10-17.md on PR #56](https://github.com/ethan-bailey/crisis-response-port/blob/feature/original-projectile-limb-hit-runtime/docs/PROJECTILE_COMBAT_INTEGRATION_2026-10-17.md).

## Exact test and deployment evidence
- **Tested gameplay SHA:** `a3e792744b590502b9c8af1c98ed0bd7d84b8702`.
- **Passing full run:** [38998115604](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38998115604): native checks, source provenance, .NET 8 WASM, pinned PhysX, Chromium and WebKit diagnostics/gameplay. Includes 31-shot/reload, touch/movement, original hand rendering, 50 source/input checks and 12 package-integrity checks.
- Evidence artifact **12088152433**; complete private site artifact **12087928555**; expire **2026-11-16**. Package verified: **742 files, 199 Blazor resources, 195 WASM files**.
- Verified package reused without rebuild by staging run **38998497740**. Deployed snapshot on `preview/wasm-current-iphone`: `d6122e004d66a7d80746d24bba06ebc60475e92b`. Manifest sourceCommit matches tested SHA above.
- Vercel deployment **dpl_HJk265kAiwBra1A8JypKZyQmJ4QF**, **READY**. Stable preview: https://crisis-response-port-git-preview-w-6bd30d-ethanbaileymarsh-8332.vercel.app/game/
- Preview branch later advanced to `185c35305618b1aa121d06363ace83d831af349c` solely to restore manual-only staging workflow. Git auto-deploy stays disabled; SSO protection remains enabled.
- Remote protected fetch failed with 403 at read_protection_bypass. READY metadata and CI browser tests are verified; independent remote browser acceptance and **physical iPhone Safari are unverified**. Do not loosen protection or repeat unchanged blocked authentication attempts.

Keep the current verified deployment as rollback baseline; if a later preview regresses, restore this retained snapshot/package through the established manual path. Artifact retention is limited; Git snapshot/provenance remain durable.

## Next actions, in order
1. Recheck develop, all open PR heads, source and CI; reconcile governance/handoff changes without resurrecting old handoff text. Keep #56 draft until its live graph is complete.
2. Bind a real production `IOriginalCharacterHitEffects` (currently test observers only) to Mission, TimeScaler, CharacterAudio/AudioPropagator, source markers and DropWeapons. Preserve Mission.FirstInjuryTimestamp's write-once-zero setter; original audio selection/delay/cooldown/consciousness; hit time scaling; mission marker conditions; and `DropWeapons(bool attachToRagdoll)` equipment/physics/dead-grip/IK behavior. Implement genuine typed `IOriginalMarksmanAttack` host before marksman support.
3. Load distinct NPCs through original mission/prefab initialization, original limb registry, physiology/combat, source visibility, colliders and masks. Do not invent synthetic live targets or substitute AI.
4. Complete source exit wounds, organs/fractures and rendering dependencies. Original `Limb.HandleExit` creates wounds and consumes RNG inside `useWoundDecals` renderer branches; copying formulas alone changes behavior. Dynamic damage masks, wound decals and original blood stream/particles remain incomplete. `LimbTextureData.Initialize` creates a new staticDamage texture: original PNG importer clamp does not establish its runtime wrap mode. Resolve unsupported out-of-bounds skin sampling from source; do not guess clamp/repeat.
5. Complete original body-state/MuscleControl graph. SetRagdoll includes timed weapon drop, arm disabling, control relaxation, kinematic/target changes, grenade fuse, Limb.OnBecomeRagdoll and EventRagdoll. SetUnconscious stops voice/releases grabs; Physiology.SetCharacterState includes spasms/recovery/death. Reuse StepOriginalPhysiology; do not add a parallel health system. Coordinate #57 weapon-drop/IK callbacks.
6. Only then enable the 19 colliders and test two genuine source characters with owner exclusion, original ammo/skin/armor, ordered events/credit, entry/exit/organ/fracture wounds, hemorrhage/shock/oxygen/state transitions and actual PhysX injury/joint/muscle responses.
7. Run focused checks first: `python3 scripts/verify_original_combat_source.py`; `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -- --projectile-injury-chain`, plus `--source-tissue-penetration`, `--source-limb-interior`, `--character-systems` and full checks as relevant. Use `scripts/projectile_injury_smoke.mjs` and `.github/workflows/wasm-arm-sequences.yml` for one exact-SHA full milestone. Reuse its passing package for a manual preview, then obtain real iPhone Safari evidence.

## Preservation and operating rules
Preserve original source and recovery provenance (`recovery-preservation-2026-10-15`, archive SHA256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`). No invented gameplay, no production no-op callbacks, fake particles/shaders or parity percentages.

Preserve approved circular joystick: 16% horizontal dead zone, 65% walk-to-sprint breakpoint, analog speed with keyboard full speed, icon controls/no sprint button, and chamber-inclusive ammo count. Follow `docs/APPROVED_IPHONE_ANALOG_MOVEMENT.md`.

Use one controlled heavy build at a time, manual deployments only, private artifacts, and no runtime rebuild for handoff-only edits. Re-read the committed handoff and confirm complete replacement; history belongs in Git and dated reports.
