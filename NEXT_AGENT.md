# Crisis Response browser port — next-agent instructions

Updated 2026-10-15 after the firing/reload milestone was verified and privately published.

## Current result

- Browser branch: `codex/weapon-actions-checkpoint`.
- Draft PR: https://github.com/ethan-bailey/crisis-response-port/pull/8
- Verified game code: `6b94297cb07c3b0684933cb055419aea9c185da5`. Subsequent documentation-only commits do not change this build.
- Private playable preview: https://crisis-response-runtime.craftysnipe13.chatgpt.site
- Published Site source: `c6daf0b4568f112a2c7154ed71e9f87e3f039040`.
- Site project: `appgprj_6acfef7174888191bc2bc2f269f60acc`.
- Successful deployment: `appgdep_6ad1076460888191b8f5f5658942364d`.
- Version: `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_9066e047ec848191b51d240dea88d4e2`.

The preview now supports drag-to-aim, tap-to-fire, held FIRE, source M4 ammunition/bolt mechanics, recoil and the source reload sequence. Released aim stays in its last world direction relative to the moving character. Native and Chromium/WebKit tests passed; this is NOT complete 1:1 game parity or an actual iPhone device test.

A separate agent owns Unity feasibility/utility repairs. **Do not edit their Unity files/workflows or reset main/the foundation branch to this branch.** This browser branch intentionally starts before their Unity commits. Integrate later through reviewed changes; preserve both lines of work. PR #7 on `codex/iphone-input-parity-foundation` contains the prior camera milestone and subsequent Unity work.

## User constraints

- Target: play the original Crisis Response in a browser on iPhone 16, eventually 1:1 behavior.
- ONLY port recovered developer systems. Do not invent substitute gameplay. Mark engine/device/rendering adapters and unavailable systems explicitly.
- The user wants proposed new milestones verified with them before execution. The firing/ammunition/reload and touch-aim milestone was authorized and is now complete within the documented scope. Ask before beginning another milestone.
- Touch controls: dragging aims and never fires; a short tap fires the last direction without retargeting; FIRE hold requests automatic fire. Releasing a drag must not leave a fixed invisible screen/world point to track.
- The user has no computer and depends on hosted results. Keep updates concise and avoid unnecessary token use.
- Preserve private hosting; do not merge main without authorization.

## Verified evidence

These runs test the exact code commit above:

- Native/runtime plus WebAssembly: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38772950394
- Complete game/browser checks: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38772950366
- Game artifact: `11991399536`, `crisis-response-mission01-game-smoke`. Contains compiled `work/game-runtime/wwwroot/`, JSON reports and screenshots.

Coverage:

- 23 Node regressions: independent input ownership, cancellation/lifecycle, touch tap vs drag, retained direction, locomotion poses, loading/binary geometry.
- Native WeaponContracts: clock preservation; 30 magazine rounds plus chamber; 31 shots then empty-bolt lock; source reload/bolt tasks complete; reserve consumed; no infinite ammunition/no idle firing; serialized infinite tangents parse under localized infinity symbols.
- Chromium and WebKit: scene contacts, movement/stopping, landscape controls/sprint/deferred sidearm, perspective camera and original head/torso aiming.
- `scripts/weapon_smoke.mjs` in both engines: no startup shots, aim retained while walking in both directions, screen tap fires once without retargeting, tactical reload conserves ammo, held FIRE empties the original 30+1 mechanism, empty lock and empty reload work, finite IK and preserved camera settings. Native touch drag without firing is tested in Chromium; WebKit touch tap is native, while drag separation is additionally covered by the shared input unit test. No physical iPhone was available.

Inspect `weapon-smoke.json`, `weapon-chromium.png`, `weapon-webkit.png`, presentation and input reports in the artifact. Both final screenshots were visually checked.

## Implementation map

- `scripts/extract_weapon_runtime.py` extracts settings, source hashes, selected original methods/classes and assets from the original recovery. `game/assets/weapon-runtime.json.gz` carries configuration/provenance. Audio, muzzle-flash atlas and magazine assets are in `game/assets/weapon-runtime/`.
- `runtime/Original/Weapons/`: source trigger/recoil/spawn methods, full source Magazine/Equipment and bolt mechanics, ChangeMagazine/WorkBolt tasks, selected ArmIKSolver transitions and projectile flight methods. Namespace/import/transport adaptations are explicit; this is not the entire WeaponBase/CharacterBody lifecycle.
- `runtime/Engine/WeaponHost.cs`: live transport views, configuration, task lifecycle scheduler and browser effect events. Original mechanical algorithms remain in Original/Weapons. Uses an explicit test loadout: full 30-round magazine + chambered round and one reserve magazine, NOT recovered saved-player equipment.
- `runtime/Engine/ProjectileFlightHost.cs`: selected non-explosive projectile setup; source flight calls the scene query adapter. Material/limb interactions are unavailable and currently terminate with Error. Do not describe this as working damage or penetration.
- Browser bridge: `InitializeWeapons` after `InitializePresentation`, then `StepWeapons`. The old live runtime cannot service these calls; always deploy matching compiled runtime and game sources.
- `game/presentation.mjs`: original pose/recoil outputs, source arm transition targets and original analytic IK; source smoothing is applied only when its transition/smoothing flag requests it. Hand/grab-point transforms and magazine prefab hierarchy bind through exact serialized references to rendered world poses.
- `game/weapon-runtime.mjs`: source sampled audio, source muzzle-flash sprites/animation and original magazine model. No invented tracer: this source M4 projectile has no trace prefab.
- `game/aim-direction.mjs` and `game/browser-input.mjs`: device adapters. Touch tap threshold is 8 CSS pixels/300 ms. Canvas down alone neither aims nor fires; movement initiates a drag. Real mouse movement can aim; mouse hold/FIRE hold request the original trigger. Tap pulses pass through original InputKey frames; lifecycle loss clears queued taps/reloads.
- `game/index.html` consumes pointer coordinates only when a new aim event arrives, then moves the target with the shoulder using the stored world vector. Do not return to per-frame raycasting of stale pointer coordinates.
- `scripts/extract_runtime_sources.py` emits a partial global Projectile class for the selected original flight methods.
- `scripts/export_weapon_manifest.py` accepts an optional original component prefab and output name for the recovered magazine.

## Known limits and next milestone

Before new work, propose the scope to the user. A reasonable next milestone is original projectile/material interactions and impact effects, before adding damage/AI; confirm rather than executing this suggestion automatically.

Preserve these boundaries:

1. No material impacts, verified penetration/fragmentation or limb damage yet. Import actual serialized BallisticMaterial dependencies and source methods. Do not replace them with invented hitscan/damage logic.
2. Casing ejection and dropped-magazine physics are explicit transport events, not physical source simulation. The original magazine model follows reload poses, but discarded copies/rigid bodies remain unported.
3. Smoke, muzzle blast forces, bullet time and action music are recorded event boundaries. Audio uses original samples but not full source propagation/mixer behavior.
4. Complete original behavior tree/CharacterBody update ordering, physiology/respiration/injury, interactive doors, alternate cameras and post effects remain unfinished. Original camera/upper-body normal methods run, but full visual parity is not proven.
5. Selected weapon AnimationCurves clamp outside their key ranges. Verify the native serialized wrap mapping in Unity before certifying this as exact parity. Other curves retain strict default behavior. PrefabCloner preserves the selected clamp setting.
6. Euler getters support the planar yaw/Z presentation used here; validate before using them for arbitrary 3D gameplay.
7. WeaponBase.direction stays at its spawn direction: original CharacterBody.Turn does NOT call weapon.Turn afterward. Assigning it every frame double-flips projectile X when aiming left.
8. Reload manipulation must not be treated as IsLoweringWeapon; source has separate flags. Do not invent that coupling.
9. Full default component/equipment selection and complete weight/alignment parity are not recovered by this scoped loadout. Base weapon and magazine behavior must not be presented as all loadout combinations.

Regression lesson: WebKit's locale rejected `float.Parse("Infinity")`. The asset reader now explicitly recognizes `Infinity`/`-Infinity` and uses invariant parsing for other strings. Keep the localized-symbol native regression. Browser error UI includes the actual .NET exception message plus stack, rather than stack alone.

## Recovery and commands

Private repo: https://github.com/ethan-bailey/crisis-response-port

Some sessions have only a partial checkout, not a usable local Git clone. Use the GitHub connector's blob/tree/commit/ref tools with a base tree and expected branch SHA when necessary. Binary assets use base64 blobs. Never overwrite concurrent branch changes.

Existing Actions recover the original project using:

```bash
gh run download 38582072502 --repo "$GITHUB_REPOSITORY" --name crisis-response-recovery-direct-v3-1 --dir work/recovery
python3 -m pip install PyYAML==6.0.2
python3 scripts/extract_runtime_sources.py work/recovery/export/ExportedProject runtime/Original
python3 scripts/extract_presentation.py work/recovery/export/ExportedProject game/assets/presentation.json runtime/Original
python3 scripts/extract_weapon_runtime.py work/recovery/export/ExportedProject game/assets/weapon-runtime.json.gz runtime/Original/Weapons
python3 scripts/extract_runtime_scene.py work/recovery/export/ExportedProject work/runtime/scene-package.json
dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -- work/runtime/scene-package.json
```

`.github/workflows/game-movement-smoke.yml` builds .NET WebAssembly and pinned PhysX and runs browser tests. `.github/workflows/runtime-foundation.yml` regenerates original selections and runs runtime/native/WebAssembly checks. Reuse automatic runs for the current commit instead of dispatching duplicates. No Unity key/license is needed for this selected browser runtime; Unity feasibility is a separate effort owned by the other agent.

Paths if the workspace survives:

- Repo: `/workspace/scratch/0234318e821a/crisis-response-port`
- Recovered Unity data/source: `/workspace/scratch/0234318e821a/recovery/export/ExportedProject`
- Decompiled reference: `/workspace/scratch/0234318e821a/recovery/decompiled`
- Site: `/workspace/sites/crisis-response-runtime`
- Latest extracted browser artifact: `/tmp/weapon-browser-final` (temporary).

Do not assume `/tmp` SDKs or artifacts persist. Restore from GitHub/Actions when needed. Repository-backed code should remain in GitHub rather than being duplicated in Library.

## Publishing another verified browser build

Read the current Sites skill. Reuse the project ID and owner-private audience above; get_site confirmed only the owner is allowed, with no groups/external visitors. Open the existing Site through the canonical source helper before editing. Do not create a new Site or change its audience.

1. Finish source and checks on the isolated browser branch.
2. Use the compiled runtime artifact from the SAME verified game commit. Replace `dist/runtime-web/` with `work/game-runtime/wwwroot/`; change its index base from `/` to `/runtime-web/`.
3. Copy changed game files/assets into `dist/game/`. Keep the original .openai/hosting.json identity and static.directory=dist.
4. Refresh the short-lived source credential; keep it only in memory and pass through helper stdin, never files/arguments/logs.
5. Run the canonical site-workflow helper to commit, push and package the exact source. Pass its complete opening result and ordered commands; no duplicate build is necessary for already compiled static assets.
6. Validate the archive with `gzip -t`. The TTY helper has sometimes produced a truncated archive despite a successful result. Rebuild the SAME source using the supported non-TTY package-site helper when that happens, then validate before upload.
7. Publish with save_version_and_deploy_private; retain saved-version/deployment IDs on retries and poll only non-terminal deployments. Successful native deployment status/URL verifies publication.
8. Update this handoff and draft PR with exact code SHA, evidence, Site source and limits. Do not merge main automatically.

Managed Linux lacks the supported local browser plugin in this session, so QA runs in Actions. Do not improvise an unsupported local browser/server. Keep the private preview on its last verified build if checks fail.
