# Crisis Response Port — current handoff

## Current source and gate status

- Work branch: `develop/wasm`. Live source HEAD before this session-end commit: `685acec2a3e77ebf87afe140ecbe23d6d71c365d`. This session restored four unchanged original Gate 6 source files from retained branch #64 and added the retained-branch comparison below; no gate was closed or capability behavior verified.
- Current gate list and evidence classes: [docs/GATE_DEPENDENCY_INDEX.md](docs/GATE_DEPENDENCY_INDEX.md). It is the only gate list. Do not add another numbered list.
- Frame-clock/TimeManager test: hosted Actions run #157 (`40572335603`, source `3a33a6df`) passed packaged Mission step 29. Left moved X `−0.6600008 → −0.8176373`; `MovementDelta=−0.030167103`; source moving/backward flags were true; `Body walk backward` selected; release returned to `Body stand`. The frame-clock change moved the prior #155 failure to a pass.
- Post-contact test: run #161 (`40694642865`, source `630d6f71dd64c4cf71ad50cf5d12b542a458e8e8`, tree `eb330d64a0981d5efeb1b7b3414f1d9da5a1338e`) also passed step 29. X `−0.6600008 → −0.8176211`, contact Y `1.3901279 → 1.2841799`, `MovementDelta=−0.048979104`, source moving/backward flags true, `Body walk backward`, and release returned to `Body stand`. Contact-tick work changed `MovementDelta` from `−0.030167103` to `−0.048979104`; final X and velocity were nearly unchanged and the pass remained.
- Reload isolation: run #161's independent `--reload-only` packaged Chromium acceptance passed; original magazine-out/in audio and settling arm pose were observed. Active magazine instance changed, but magazine/reserve counters stayed 30/2, so the full ammo-count transition remains untested.
- These run results correct the audit's then-current statements that no post-frame-clock step-29 result or independent reload result existed. The latest runtime-tested source is `630d6f7`; later commits in this checkout are docs/environment-preflight only.

## Environment and session startup

- Starting checkout for this session: `/workspace/crisis-response-port`; synced `develop/wasm` at baseline `470e43d5ac1d414f31b5cb3dc5840937fd23cbb1`.
- Standing first step: run `scripts/env/bootstrap-environment.sh`, then source `scripts/env/environment-preflight.sh`. The environment wipes installed tools after inactivity, so restore the pinned runner tools through bootstrap instead of treating missing SDK/browser installs as a new project issue.
- Bootstrap installs only .NET SDK 8.0.426 and Playwright-lock Chromium 156.0.8078.4. It now retries Chromium once with `PLAYWRIGHT_DOWNLOAD_HOST=https://playwright-akamai.azureedge.net` if the default host fails. It also applies the runner's no-owner-restore tar wrapper and builds the repository process-stat shim when needed.
- Bootstrap fallback was verified in an isolated cache: SDK 8.0.426 and Chromium 156.0.8078.4 both passed; no cleanup-trap warning. Combined preflight passed SDK and Chromium. Shell push authentication failed as expected (`could not read Username for 'https://github.com'`); use the connected GitHub API for writes.

## Branch disposition

At session start, all 18 requested refs matched their listed tip SHAs. Four safe branches were deleted after checking each live tip SHA; a batched live-ref check confirms all four are absent and the other 14 requested refs still match their expected SHAs. The eight Group 1 branches remain kept; six remain needs-owner-decision. No main, preview, release, or unlisted refs were touched.

### Group 1 — keep

- #74 `codex/ik-sequence-binding-20261026` — `79845d580fe8a186cc39411c5e0cef6801a40221`: binds seven recovered IK sequences to original controller fields; Gate 5 authored animation/IK integration.
- #57 `feature/source-ikarmcontroller-live-callbacks` — `2b73044c107b875d8545d03541bf6b2d35f64654`: original IK callbacks, live arm solver ordering, weapon grabs; Gate 5.
- #56 `feature/original-projectile-limb-hit-runtime` — `fb96e92a848409014b85f6e094d995a9376fb731`: original projectile/injury runtime plus NPC source-weight and patrol-integrity regressions; Gates 4 and 6.
- #65 `feature/extract-original-occupant-spawn-contracts` — `65d24413730b79fc4ee3c6d7994f24312646b3fd`: isolated original Occupant spawn contracts and serialized-area tests; Gate 6.
- #64 `feature/extract-original-occupant-spawner-sources` — `6eba31d557ecdbb6e007c8e3be9cfe34d4772f71`: exact EquipmentList, SerializableOccupantSettings, Occupants and OccupantSpawner sources; Gate 6 source recovery.
- #21 `codex/wasm-original-arm-sequences` — `097f1dfed194c670e736d7a4234c56a79766d16d`: six original arm-sequence classes with source-backed compile/Chromium/WebKit checks; Gate 5.
- #34 `feature/26-source-arm-rig-import-audit` — `e71873f3d8c67eb330ff7ccb2b13cda819b0bc60`: exact ArmController rig/import dependency audit and compile gate; future Gate 5 integration.
- #33 `fix/33-configured-grips-chamber` — `72662c9ddd2bef362d87e00c8403cff0633f3101`: configured grip/chamber contract and combined ready-ammo HUD without a chamber visual; Gates 3–4.

### Group 2 — touched-path comparison

- #70/#72 `codex/game-scene-name-normalization-20261023` — `b3640f02009147eb7a3350489db382bb9ad30983`: 24 develop-versus-tip differences span browser input, scene/package loading, runtime scene/UI/serialized resolvers, tests, and audit indexes; not all differences could be tied to later superseding commits. **Needs owner decision; retained.**
- #73 `fix/touch-hold-fire-20261023` — `e6ac79c1c6439e543ad126a14e71f09dd75ea037`: three differences in browser input, page wiring, and its test; hold-fire behavior is not proven superseded. **Needs owner decision; retained.**
- #67 `chore/live-state-guard-20261018` — `fa426667bfb24ecd08d8b3332d63bf563f30819b`: two differences are old AGENTS/NEXT_AGENT guidance; later canonical AGENTS and current handoff replace that live-state guard/checkpoint wording. **Deleted 2026-10-28 after live tip verification; batched ref check confirms absent.**
- #35 `feature/iphone-original-movement-joystick` — `7e5eec42b2c272b132272851977d5bc3145383ce`: two differences in browser input and page wiring; its horizontal-slider behavior differs from the current joystick/aim/sprint behavior and is not fully mapped to a later superseding change. **Needs owner decision; retained.**

### Group 3 — unique-work check

- #62 `maintenance/foundation-first-master-plan-20261017` — `a2988eef5c2213fc877b74c09af26e631c5a8786`: old plan is archived at `docs/history/FOUNDATION_FIRST_COMPLETION_PLAN_2026-10-17.md`, but the branch copy differs by five insertions/seven deletions and its handoff also differs. Historical/non-runtime, but exact supersession is incomplete. **Needs owner decision; retained.**
- #36 `fix/original-impact-mesh-initial-load` — `2d832abf685cec6cd4a9dabe751aa7becca05952`: both unique commits' touched runtime path `game/original-impact-mesh.mjs` is identical to develop. **Deleted 2026-10-28 after live tip verification; batched ref check confirms absent.**
- #20 `codex/full-source-closure-audit` — `e888d2938c6ac6b12c28381a4aa06e79cd7bfa5d`: six unique commits contain audit-only scripts/workflow and historical reports; no current workflow or source references those tools, so nothing is runtime/load-bearing. **Deleted 2026-10-28 after live tip verification; batched ref check confirms absent.**
- #19 `codex/wasm-compile-package` — `d67d446bd062466a4ec62fcf24bf49818825a5c9`: four unique commits include a private package workflow, handoff, SDK pin, and package-script/test changes not fully reconciled with the current packager. **Needs owner decision; retained.**
- #8 `codex/weapon-actions-checkpoint` — `a824915c08b2d945a77de7c33297453680ef8034`: its sole unique tip commit disables automatic Vercel deployment; `vercel.json` is identical on develop. **Deleted 2026-10-28 after live tip verification; batched ref check confirms absent.**
- #7 `codex/iphone-input-parity-foundation` — `ff2eed647e4eeb3446453b304bb4cbc4b6d31349`: the recovery-preservation-2026-10-15 release contains the 7,231-file original Unity source archive, and `docs/history/NEXT_AGENT_BEFORE_DEVELOP_WASM_REORGANIZATION_2026-10-16.md` preserves the Unity run #38800542438 / artifact #12004975378 result (18 output files, 36,117,455 bytes; logs/manifests retained, compiled player not retained). The four detailed Oct 15 spike reports on this branch are absent from current `docs/history`, so full spike-history preservation is not confirmed. **Needs owner decision; retained.**

The bootstrap-fix and branch-disposition commits are written through the GitHub API. Main and all preview/release refs remain untouched. Docs archive counts from the earlier restructure remain 57→52 top-level files and 284 total files under `docs/`.


## Retained-branch comparison and selection

Compared each of the 14 retained branch tips against live `develop/wasm` at `685acec2a3e77ebf87afe140ecbe23d6d71c365d`. Selected the current develop versions except for #64's missing exact original source files, which are now restored unchanged. No tests were run; this commit only adds archival source and this handoff.

### Group 1 — keep

- #74 `codex/ik-sequence-binding-20261026` (`79845d580fe8a186cc39411c5e0cef6801a40221`): keep the branch. Its helper binds recovered sequence assets to controller fields, but the branch only extends the headless contract and does not connect packaged Mission sequence selection; use current Gate 5 implementation until that integration is verified.
- #57 `feature/source-ikarmcontroller-live-callbacks` (`2b73044c107b875d8545d03541bf6b2d35f64654`): keep. The branch has a selective callback dispatcher with explicit unsupported cases; current FullSource controller playback contracts are the better integrated path.
- #56 `feature/original-projectile-limb-hit-runtime` (`fb96e92a848409014b85f6e094d995a9376fb731`): keep. Its broad older combat and occupant snapshot is superseded by the later current runtime; the four exact occupant source files it shares with #64 were selected from #64 and restored.
- #65 `feature/extract-original-occupant-spawn-contracts` (`65d24413730b79fc4ee3c6d7994f24312646b3fd`): keep. Its spawner, sandbox settings, tests, and visual-spawn data are already present on develop at the same file contents; no copy is needed.
- #64 `feature/extract-original-occupant-spawner-sources` (`6eba31d557ecdbb6e007c8e3be9cfe34d4772f71`): keep. Selected its four exact original source files for Gate 6 recovery. Each source blob matches #56's copy; all four were absent from develop and are now added unchanged under `source-reference/Original/NPCRecovery/Assets/Scripts/Assembly-CSharp/`.
- #21 `codex/wasm-original-arm-sequences` (`097f1dfed194c670e736d7a4234c56a79766d16d`): keep. The sequence data and original runtime types are already on develop; the branch's much older packaging and workflow snapshot is not selected.
- #34 `feature/26-source-arm-rig-import-audit` (`e71873f3d8c67eb330ff7ccb2b13cda819b0bc60`): keep. Its unique files are historical audit tooling, not a better runtime version; no code is selected.
- #33 `fix/33-configured-grips-chamber` (`72662c9ddd2bef362d87e00c8403cff0633f3101`): keep. The configured grip and combined ready-ammo behavior are already represented in the current weapon/HUD implementation; retain the current evolved files.

### Groups 2–3 — retained pending owner disposition

- #70/#72 `codex/game-scene-name-normalization-20261023` (`b3640f02009147eb7a3350489db382bb9ad30983`): keep. Its old scene/package and browser-input snapshot is far behind current scene and packaged-runtime code; current develop selected.
- #73 `fix/touch-hold-fire-20261023` (`e6ac79c1c6439e543ad126a14e71f09dd75ea037`): keep. The hold-fire behavior is already present in current `game/browser-input.mjs`, alongside later input handling; current develop selected.
- #35 `feature/iphone-original-movement-joystick` (`7e5eec42b2c272b132272851977d5bc3145383ce`): keep. Its horizontal thumb adjustment is an older input snapshot; current develop has the later movement, aim, sprint, and source-actor paths.
- #62 `maintenance/foundation-first-master-plan-20261017` (`a2988eef5c2213fc877b74c09af26e631c5a8786`): keep. It is an older plan/handoff version, not a runtime alternative; current policy and history documents selected.
- #19 `codex/wasm-compile-package` (`d67d446bd062466a4ec62fcf24bf49818825a5c9`): keep. Its old package workflow/script snapshot is superseded by the current packager; current develop selected.
- #7 `codex/iphone-input-parity-foundation` (`ff2eed647e4eeb3446453b304bb4cbc4b6d31349`): keep. Its Unity spike history is archival and its old touch-input code is superseded by current develop; current develop selected.
