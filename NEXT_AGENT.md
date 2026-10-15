# Crisis Response browser port — next-agent handoff

Checkpoint: 2026-10-15. Branch: `codex/weapon-actions-checkpoint`.

## Read this first

The user asked us to wrap up and leave this instruction set. **This checkpoint is work in progress and has NOT been deployed or browser-verified.** Do not describe firing/reloading or the latest aim fix as delivered to the live site.

Last fully verified game commit: `0a676c5d02085ee47509bff84744aebb0132c86e` on `codex/iphone-input-parity-foundation`, draft PR [#7](https://github.com/ethan-bailey/crisis-response-port/pull/7). Main remains unchanged. Preserve this known-good baseline until the checkpoint passes the checks below.

Last verified private preview: https://crisis-response-runtime.craftysnipe13.chatgpt.site

It contains the original camera/upper-body aiming milestone, **not this checkpoint**. Its published Site source is `0f9b4a41b569247f835274b5102245ce484c36a4`. Site identity is `appgprj_6acfef7174888191bc2bc2f269f60acc`; preserve its owner-private audience. No new Site is needed.

## User requirements and authorization

- Target: Crisis Response in a browser on iPhone 16, eventually 1:1 behavior.
- ONLY use recovered original developer systems. Do not replace unported systems with invented gameplay. Engine/device/renderer adapters are necessary but must be explicit and scoped.
- Current authorized milestone: original firing, ammunition and reload logic connected to the existing weapon/touch controls, verified, then another private build.
- Touch: **drag to aim; a short tap fires the last aimed direction. Dragging alone must not fire.** A dedicated hold-to-fire button can retain automatic fire.
- Latest correction: after a drag ends, preserve its **world direction relative to the character**. Do not keep raycasting the last screen coordinate every frame; that creates an invisible fixed pointer and makes the character turn when walking past it.
- Ask the user to verify the plan before starting a new milestone outside this scope. They authorized this milestone already.
- Keep updates short and avoid unnecessary token use. They have no computer and rely on repository/hosted results.

## What this checkpoint contains

- `scripts/extract_weapon_runtime.py`: recovers source M4 settings, original selected weapon methods, full magazine and equipment logic, bolt cycle, reload/bolt tasks, arm transition methods, sampled audio and muzzle-flash sprite animation. Source hashes and adaptations are in `game/assets/weapon-runtime.json.gz`.
- `runtime/Original/Weapons/`: generated original methods/classes in a scoped namespace. Includes `ChangeMagazine`, `WorkBolt`, `CyclingBolt`, `Magazine`, `Equipment`, `WeaponBarrel`, source trigger/recoil methods, original projectile spawning and selected flight methods.
- `runtime/Engine/WeaponHost.cs`: explicit transport host and task scheduler; invokes original task lifecycle and source mechanics. Test loadout is a full 30-round magazine + chambered round and ONE reserve magazine. This is a scoped test loadout, not recovered saved-player equipment.
- `runtime/Engine/ProjectileFlightHost.cs`: non-explosive projectile setup, bridge and explicit unavailable material/limb boundary. Source flight runs, but damage/penetration/fragmentation is NOT integrated.
- Browser bridge adds `InitializeWeapons` and `StepWeapons`. Presentation takes the weapon's original recoil values; rendering takes original transition targets before the existing original analytic IK solve.
- `game/weapon-runtime.mjs`: original audio clips and source sprite atlas/animation for muzzle flash. No synthesized weapon audio or invented tracer. The source M4 projectile has no trace prefab.
- `game/aim-direction.mjs`: device adapter retaining a character-relative world vector. `game/index.html` consumes pointer coordinates only when a new aim event arrives, then reconstructs a moving target from the shoulder plus that vector.
- `game/browser-input.mjs`: touch tap/drag distinction (8 CSS pixel threshold, 300 ms maximum tap). Touch down alone does not aim or fire. Mouse hover still updates aim when actual mouse events arrive; mouse hold and FIRE hold use original trigger input. Tap presses are queued through original InputKey frames and cleared on page lifecycle loss.
- `runtime/Checks/WeaponContracts.cs` and `scripts/aim_direction_test.mjs` added. Existing shared-input tests updated for the new touch semantics.
- `scripts/extract_runtime_sources.py` now emits a partial Projectile class so the selected original flight methods can share its original ballistics math.
- Runtime workflow regenerates weapon selections from original recovery and runs the native contracts. Game workflow adds aim-direction unit tests. **No end-to-end weapon browser test exists yet.**

## Verification actually completed

- All Engine + Original C# sources compile with .NET 8 Roslyn locally.
- The native `WeaponContracts` passes: initialization preserves clock; 30+1 shots exhaust the weapon; bolt locks open; original reload and bolt/arm tasks finish; reload produces 29 magazine rounds + one chambered round; reserve is depleted; no firing without trigger input.
- 23 Node regressions pass across input ownership/lifecycle, touch tap vs drag, direction retention, locomotion poses, loading/binary geometry.
- New JS modules pass `node --check`.
- **Not verified:** actual WebAssembly publish, Chromium/WebKit rendering and audio, actual iPhone touch behavior, magazine visual motion, muzzle flash orientation/pivot, runtime update ordering, camera/upper-body regressions with the added weapon host. Do not publish based only on the native result.

## Immediate next steps, in order

1. Check out this checkpoint branch and read any CI results associated with its head. Fix compilation/runtime failures before considering publication. The checkpoint push may start existing workflows automatically; reuse those runs rather than dispatching duplicates.
2. Add a meaningful browser weapon smoke test in Chromium and WebKit at 852×393, and wire it into `.github/workflows/game-movement-smoke.yml`. Verify:
   - No startup/idle shots.
   - Touch drag changes aim and never fires.
   - A short screen tap fires exactly once without changing the last aim direction.
   - Move left/right after releasing an upward or leftward drag; direction vector remains unchanged and facing does not flip toward an old screen point.
   - FIRE hold cycles the original M4; release stops requesting shots; magazine/chamber accounting and empty-bolt lock are correct.
   - Reload visibly advances source arm transitions, blocks inappropriate fire, finishes, consumes reserve, and works after empty lock as well as tactical reload.
   - Cancellation, blur, visibility loss, and simultaneous movement touch do not leave firing stuck or deliver queued shots later.
   - No page errors, finite IK, original camera/FOV preserved.
3. Review these known incomplete adapters before claiming parity:
   - `CharacterBody.MoveMagazineToHand` is now an original source method, but its transport hand/grab-point transforms are constructed separately from actual rendered hands. Bind them to the exact serialized hand/grab-point references; current reload magazine visual pose is not verified.
   - `WeaponComponentMagazine.Drop` emits a removal event and hides a transport object. This is NOT original dropped-magazine physics. The JS weapon model is not yet synchronized with this transport magazine, so appearance may remain attached during reload. Implement the actual source presentation/physics bridge or disclose and leave unsupported; do not fake it.
   - Projectile generic hits explicitly terminate with Error because material/limb components are not yet imported. This checkpoint does not do damage or verified penetration. Do not turn this into an invented hitscan system.
   - Casing ejection emits a transport event; physical casing rendering/rigid-body simulation remains pending.
   - Bullet time, smoke, muzzle blast forces, and action-music calls are recorded as explicit events, not implemented. Muzzle audio propagation/mixer parity is also absent; sampled clip playback is only a presentation bridge.
   - `AnimationCurve.clampOutside` was added for selected weapon curves; verify Unity's actual clamp/wrap interpretation for these serialized curves before calling it parity. Existing strict behavior remains default for other curves.
   - Euler getters were added to the engine for source arm/weapon transforms. The current adapter is intended for this planar yaw/Z pose; validate its conventions with left/right aim and hand transitions.
   - Original `WeaponBase.direction` stays at its initial spawn direction, as original CharacterBody.Turn does not call weapon.Turn afterward. Do not assign it every frame; that would double-flip projectile X while aiming left.
   - Native source steps and rendering use a scoped scheduler, not the complete original CharacterBody/behavior tree/update ordering. Verify the actual ordering rather than claiming full runtime parity.
4. Run native runtime checks, existing movement/contact/input/presentation browser tests, and the new weapon browser test. Collect artifacts; visually inspect the weapon/aim screenshots. Fix observed regressions, and make a final tested commit.
5. Publish only that verified build. Replace BOTH changed game files/assets and the compiled `runtime-web` output from the SAME final workflow artifact. New JS calls methods absent from the old live runtime. Fix the artifact runtime index base path from `/` to `/runtime-web/`.
6. Update a draft PR or PR #7 with exact scope, passing workflow links, limitations and live preview. Do not merge main without authorization. Report the playable result concisely; do not claim complete 1:1 parity.

## Recovery, build and repository details

Repository: https://github.com/ethan-bailey/crisis-response-port (private).

The workspace is a partial checkout, not a usable local Git clone. Repo writes in this session use GitHub blob/tree/commit/ref tools. Use a base tree and branch lease to avoid overwriting unrelated work. Binary assets need base64 blobs. Keep code and recovery in GitHub; no duplicate Library repository is necessary.

Original recovery is downloadable by existing Actions using:

```bash
gh run download 38582072502 --repo "$GITHUB_REPOSITORY" --name crisis-response-recovery-direct-v3-1 --dir work/recovery
python3 -m pip install PyYAML==6.0.2
python3 scripts/extract_runtime_sources.py work/recovery/export/ExportedProject runtime/Original
python3 scripts/extract_presentation.py work/recovery/export/ExportedProject game/assets/presentation.json runtime/Original
python3 scripts/extract_weapon_runtime.py work/recovery/export/ExportedProject game/assets/weapon-runtime.json.gz runtime/Original/Weapons
python3 scripts/extract_runtime_scene.py work/recovery/export/ExportedProject work/runtime/scene-package.json
dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -- work/runtime/scene-package.json
```

Use `.github/workflows/game-movement-smoke.yml` for the complete browser build and tests. It vendors pinned PhysX, generates `mission01.crg`, builds .NET WebAssembly, and uploads the compiled runtime plus evidence. No Unity license/key is needed for this selected .NET/Three/PhysX runtime.

Last verified previous milestone workflows (NOT validation of this checkpoint):

- Browser: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38738153273
- Runtime: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38738153275

## Workspace paths if this environment survives

- Repo: `/workspace/scratch/0234318e821a/crisis-response-port`
- Recovery: `/workspace/scratch/0234318e821a/recovery/export/ExportedProject`
- Decompiled reference: `/workspace/scratch/0234318e821a/recovery/decompiled`
- Site: `/workspace/sites/crisis-response-runtime`
- Local SDK: `/tmp/crisis-dotnet` (.NET 8.0.415). Full MSBuild has sandbox process limitations here; direct Roslyn works. Prefer the existing Actions for ordinary build/QA.
- Local direct native contract compile response: `/tmp/weapon-check.rsp`, runner `/tmp/weapon-contract-runner.cs`; executable `/tmp/weapon-check.dll`. These temporary files are not required by the repo.

If these paths disappear, restore from GitHub and the recovery artifact. Do not rely on transient files or conversational memory as the only copy of a change.

## Private hosting workflow

Use the current Sites skill. Open the existing Site through its canonical source helper before editing, preserve `.openai/hosting.json`, and refresh the short-lived credential through native Sites tools. Never persist or print the credential. Package and push the exact final source with the helper, then use `save_version_and_deploy_private`. Do not ask for a separate publishing confirmation within the authorized task.

The managed environment has no supported local browser plugin; previous browser QA runs in GitHub Actions. Do not improvise a local browser/server against the managed Sites policy. If a packaging helper reports success but the archive fails `gzip -t`, rebuild the same source archive with the supported non-TTY `package-site.mjs`, then validate it before upload. Retain deployment/version IDs on retries; preserve privacy.
