# Next agent — current state (2026-10-15, 19:17 UTC)

## Start here

## WASM compiler / renderer continuation — verified 2026-10-15

Current branch: `codex/projectile-material-impacts` in draft PR #9. Previous source/renderer checkpoint before this session was `4c7a855dd29c4b092dc6c4743cb0ea70bb516141`. **Do not reset or force-push this branch or touch the independent Unity foundation/WebGL branch.**

The new `docs/WASM_CONTINUATION_CHECKPOINT_2026-10-15.md` is the authoritative source map and rollback guide for this continuation. Changes to the original scaled `Time.time` effect bridge, impact telemetry retention, and glass texture reset/re-stamp callbacks are *engine/rendering adapters*, not rewritten game rules. All recovered developer `runtime/Original/` code remains unmodified. Browser source mesh handedness improvements from the preceding checkpoint are retained.

**Successfully compiled and tested:** native original contracts (including glass reset/re-stamp and impact bookkeeping), Node source-clock regressions, JS syntax, and Blazor WebAssembly publish on input commit `66478ac4343e156885527734db485fc46ba0f9a3`. Run [38803362416](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38803362416) **SUCCESS**. Metadata-only private artifact `12005511474` (`source-wasm-continuation-verification`), retaining source commit and logs. This proves a WASM build, not actual-device runtime parity.

A full browser/PhysX integration check was launched once through an explicit milestone-scoped workflow edit `455a017cc2900bace19a3eb47cf0276aa71db8cf`, run [38803647712](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38803647712). **VERIFIED SUCCESS (2026-10-15 20:46 UTC)**: PhysX original-scene contacts, movement and stopping, landscape input, camera/arm IK, original M4 reload and touch aiming, and recovered exit-hole mesh tests passed in Chromium and WebKit. Browser artifact `12005437510` (`crisis-response-mission01-game-smoke`) includes compiled runtime and screenshots; no physical iPhone test was performed. Both workflow push triggers are scoped to changes to their respective workflow files only; ordinary source commits will not generate repeat expensive runs. Both still support deliberate manual dispatch.

Existing owner-private Site version 8 is still the last published build; this continuation has not deployed a new version. Do not change the Site audience or publish recovered assets publicly. The independent Unity feasibility run [38800542438](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38800542438) completed **SUCCESS**, but it belongs to the separate Unity branch and does not establish whole-game browser parity.


## WASM/browser source-renderer continuation (2026-10-15 20:24 UTC)

On this same PR branch, commits `2a9f1805df6a0be5ddae3c6c0ba97f1e886e65fd` and `38d04afb7834466c1e7236585b318cce9f487ae6` repair the exact original Wood exit-hole Circle.asset renderer's Unity-to-Three handedness conversion: reflect local vertex/normal Z and reverse triangle winding. This preserves original source mesh data, C# ballistic calculations and engine-generated lifetime. `scripts/weapon_smoke.mjs` now checks every recovered position and triangle and verifies source normals if present. A lightweight mathematical geometry check passed for all 100 source faces, and the repository files were read back. **Browser/native CI has not yet run on these new commits; no new deployment was made.** The existing privately hosted version 8 is unchanged. Do not report these modifications as live until verified and published with matching WASM runtime and assets.

Separate Unity Marksman WebGL run `38800542438` (foundation branch) was still executing its actual WebGL build step when checked. An hourly conditional notification was scheduled to report the outcome. Do not edit the separate Unity branch or launch duplicate expensive Actions runs.


Browser work: branch `codex/projectile-material-impacts`, draft [PR #9](https://github.com/ethan-bailey/crisis-response-port/pull/9). Last gameplay source: `cd47a520f31a8b96bd1304f6bfc2c4ecd290db04`; subsequent commits through `2175e7e3bc61408aa59062cf9ee3516e581d1991` only changed documentation. This handoff commit is documentation only.

**All PR #9 gameplay updates are now published privately:** https://crisis-response-runtime.craftysnipe13.chatgpt.site

- Site project: `appgprj_6acfef7174888191bc2bc2f269f60acc`.
- Current saved version 8: `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_6aeb63256a6c81918ca22f1d32b20c26`.
- Successful deployment: `appgdep_6ad126a348c88191aab17f03e00c5e11`.
- Canonical Site source commit: `a20353a9b4a917e9dd285d5e6fef63f47c0aed94`.
- Owner-only audience was verified and preserved. Do not make original-game assets public.
- Earlier claims that publication is blocked, version 6 is live, or hotfix/UI are unpublished are obsolete. Version 7 has the known firing crash; do not roll back to it.

## Included and verified

- Original material/window/fragment algorithms and imported Mission 01 collision data.
- Firing crash fixed through scoped `ReparentBeforeScaleReset` adapter: preserve position/rotation before original Play immediately resets localScale. Do not replace this with speculative global shear decomposition or suppress the exception.
- Compact translucent edge controls, seven 44px-or-larger targets, ammo-only HUD; help and pending swap hidden.
- New-location touch tap aims; a tap within 24 CSS pixels of the latest touch aim fires once. Drag aims and retains direction; Fire button/mouse hold unchanged. Cancellation cannot fire; lifecycle resets clear the comparison anchor.
- Full final game/build CI [38790640197](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38790640197) succeeded on substantive commit `cd47a520f31a8b96bd1304f6bfc2c4ecd290db04`, including Chromium/WebKit tests. Artifact `11999739004`, `crisis-response-mission01-game-smoke`; SHA256 `9e15693b580ba5352eedaa8ddca6fb2fce1d6148fead39830decaf7c06f5faf2`.
- Native/WASM hotfix regression previously passed run `38782716412`.
- Published game/index.html and game/browser-input.mjs match PR head `2175e7e3bc61408aa59062cf9ee3516e581d1991`; runtime-web was replaced entirely with final artifact `work/game-runtime/wwwroot`, changing only base href to `/runtime-web/`.
- ZIP integrity, JS syntax and complete deployment gzip/tar integrity passed. Hosting returned succeeded. No physical iPhone test or fresh browser inspection of the deployed version was performed.

## File map and recovery

- `game/`: browser renderer, touch/input, camera/animation integration and recovered assets.
- `runtime/Original/`: recovered developer algorithms; preserve original behavior.
- `runtime/Engine/`: explicit engine adapters and hosting services.
- `runtime/Checks/`: native behavioral contracts, including sheared impact regression.
- `scripts/`: extraction, source provenance, asset preparation and targeted smoke tests.
- `docs/`: audits and native Unity feasibility records.
- Another agent owns Unity feasibility/compilation, related branches/workflows and main. Do not reset, merge or edit that work as part of browser continuation.

Local paths are caches, not authoritative source:
- Site checkout: `/workspace/sites/crisis-response-runtime` (canonical Site Git checkout).
- Old partial port cache: `/workspace/scratch/0234318e821a/crisis-response-port` is stale, lacks Git metadata and does NOT contain all current source. Recover current files from GitHub.
- Final artifact ZIP/extraction: `/workspace/scratch/0234318e821a/pr9-build/pr9-latest.zip` and `pr9-build/extracted/`.
- Verified deployed archive: `/workspace/scratch/0234318e821a/pr9-build/pr9-publish.tar.gz`.
- Older `crisis-response-runtime-site.tar.gz` at scratch root is truncated and must not be reused.

Artifact download workaround: GitHub download_workflow_artifact returns a reusable OpenAI file_id/file_name. Raw signed-URL curl returned 502 here. prepare_materialize with that exact returned file reference and a scratch destination successfully placed the ZIP locally; it did not create/save any Library item. Retain the returned workspace_path. Do not search unrelated user files.

For another publication, read the current Sites skill, open the same project with fresh short-lived credentials through canonical site-workflow.mjs, modify source, package/push with the helper, then publish its returned Site source SHA and unchanged archive. GitHub code SHA is NOT the Site source SHA. Credentials only in session memory/stdin, never repository or files.

## Next bounded work

First collect the user's iPhone feedback on this live version. User is low on tokens and prefers quick manual playtests over repeating the full CI suite for every small edit. Run only checks appropriate to actual changes.

Remaining parity gaps: native particle/exit-hole/fragment rendering and visible glass binding, dynamic rigidbody impulses and physiology, two unresolved built-in meshes, native comparison for hull cooking and curve infinity mapping. Do not invent placeholder gameplay or claim complete 1:1 parity.

## Historical implementation notes

The material below is retained for source details only. Its deployment/checkpoint statements are historical; current state above takes precedence.

# Active continuation — original material impacts

Updated 2026-10-15. Current browser work is on `codex/projectile-material-impacts`, draft PR https://github.com/ethan-bailey/crisis-response-port/pull/9, based on the firing milestone below. Code checkpoint: `37eac226a91ed2fa13c02d89f5666cf3f1fdbb37`.

The user authorized the next step. Original material penetration, deflection/ricochet, delayed exits, glass-alpha holes and randomized fragmentation are now connected to live projectile flight. Source sampled impact audio is transported to Web Audio. Imported coverage is 386 material components (including 68 glass windows) and 61 mesh colliders; two built-in mesh references remain unresolved. The full original projectile mask now includes Windows. Weapons and queries share the source scene world.

Privately published and verified: https://crisis-response-runtime.craftysnipe13.chatgpt.site . Site source `e31222855f1fabb6a8758230b66f812899830f03`; successful deployment `appgdep_6ad1142f55a08191b6cc64e10061bcb7`; version `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_b3d2f109b0f881919ab515d9b2ab5e8b`. The hosted runtime came from the exact successful game artifact below. Owner-only access was preserved. Documentation-only commits after the code checkpoint do not alter that build.

Read `IMPACT_CHECKPOINT.md` first for implementation and explicit boundaries. Native original/material/input/presentation/reload contracts and 23 Node regressions pass locally. An additional native probe cast successful rays against all 45 active imported mesh colliders. Final browser artifact `11996256322` contains the matching compiled runtime, screenshots and reports. Chromium recorded 46 material contacts and 4 fragments; WebKit recorded 49 contacts and 12 fragments. Both screenshots were visually checked; there was no physical iPhone test. CI runs for this exact code: game/browser `38780482295` (SUCCESS), native/WASM `38780482387` (SUCCESS). The first browser run failed only because the host called original Start on an inactive object; deferred Start now follows active/enabled state. All 21 imported unique meshes also match the source serialized AABBs.

Do not claim complete 1:1 parity. Native particles, visible hole/fragment meshes, visual glass texture bindings, rigid-body impulses and limb damage remain unfinished. Ammo curve endpoints use an explicit clamp adapter like weapon curves; native infinity mapping, PhysX hull cooking and random sequence parity still need comparison. No substitute damage or particle gameplay has been added. Preserve private hosting and the separate agent's Unity feasibility work. Do not merge/reset their branch or main.

The following section retains the earlier verified firing baseline. Its “no material interactions” statements are superseded by this checkpoint; its remaining non-material limitations still apply.

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
