# Crisis Response Port — current handoff

## Current source and gate status

- Work branch: `develop/wasm`. Current source HEAD: `ad3e536a6485cd1cb43fbdfb921b6f41a071fa36`; live `origin/develop/wasm` matched this SHA after push. The two commits landed this session: `fad29fa` (gate evidence/docs archive) and `ad3e536` (combined environment preflight).
- Current gate list and evidence classes: [docs/GATE_DEPENDENCY_INDEX.md](docs/GATE_DEPENDENCY_INDEX.md). It is the only gate list. Do not add another numbered list.
- Frame-clock/TimeManager test: hosted Actions run #157 (`40572335603`, source `3a33a6df`) passed packaged Mission step 29. Left moved X `−0.6600008 → −0.8176373`; `MovementDelta=−0.030167103`; source moving/backward flags were true; `Body walk backward` selected; release returned to `Body stand`. The frame-clock change moved the prior #155 failure to a pass.
- Post-contact test: run #161 (`40694642865`, source `630d6f71dd64c4cf71ad50cf5d12b542a458e8e8`, tree `eb330d64a0981d5efeb1b7b3414f1d9da5a1338e`) also passed step 29. X `−0.6600008 → −0.8176211`, contact Y `1.3901279 → 1.2841799`, `MovementDelta=−0.048979104`, source moving/backward flags true, `Body walk backward`, and release returned to `Body stand`. Contact-tick work changed `MovementDelta` from `−0.030167103` to `−0.048979104`; final X and velocity were nearly unchanged and the pass remained.
- Reload isolation: run #161's independent `--reload-only` packaged Chromium acceptance passed; original magazine-out/in audio and settling arm pose were observed. Active magazine instance changed, but magazine/reserve counters stayed 30/2, so the full ammo-count transition remains untested.
- These run results correct the audit's then-current statements that no post-frame-clock step-29 result or independent reload result existed. The latest runtime-tested source is `630d6f7`; later commits in this checkout are docs/environment-preflight only.

## Environment

- Combined preflight `source scripts/env/environment-preflight.sh` passed: .NET SDK **8.0.426**, Chromium **156.0.8078.4** at `/root/.cache/ms-playwright/chromium-1248/chrome-linux64/chrome`, and authenticated `git push --dry-run` (`Everything up-to-date`). SDK preflight used the available process-stat shim.
- Push credential was applied only to this checkout for session pushes. Before ending the session, the remote URL was restored to the clean HTTPS address; never print or persist the credential.

## Branch disposition and deletion record

Branch audit corrected the brief's estimated counts. The 57 PR records refer to 56 unique PR-head branch names (PRs #70 and #72 share one); there are 37 ancestor branches, one path-identical merged branch, and 18 owner-decision branches. The remote also has unrelated preview/release refs not included in this PR-head disposition. Deleted 38 branches after checking live tip SHAs, ancestry for group I, and touched-path tree parity for group II; a batched live-ref check confirmed all deleted refs absent.

### (i) Tip is ancestor of `develop/wasm` — safe, deleted

- `docs/pr69-publish-verification-20261023` — `9de4061540ceddaac5cf57317660c339e0140d4d`
- `fix/browser-publish-input-sdk-pin-20261023` — `7f38926cc953604499adb5fab07025c52a0103b7`
- `codex/scene-package-from-original-obj-20261022` — `ebc83e3c52e3190653480f50d403d623333e22ad`
- `chore/repository-audit-remediation-2026-10-18` — `389d8e8dcc3cc49d4327cc55588d874271dc4271`
- `maintenance/verified-projectile-handoff-20261017` — `dd51eb575c832fcae4fbe8a43dbe89c6c5736f79`
- `maintenance/fresh-agent-handoff-20261017` — `fee5fada74a703f6e8c221a6d84b3b6496c5e329`
- `maintenance/replace-handoff-on-every-transfer` — `3ba5d970b17cc1ede6f0e338f0d2d8e45d5ccc19`
- `maintenance/repository-hygiene-20261017` — `40c3872b217730ab1edd4f52bf5bd9eede90f8d1`
- `feature/original-projectile-limb-material-closure` — `41472e69ce9dea5a3ed00e98c4bd87e1567e445c`
- `feature/source-limb-texture-importers` — `59990ccdad45225ce17ab10954917b26c33cba0b`
- `feature/live-original-limb-skin-binding` — `2baf1d4a4f16a1fe9792ef406cbbeb11d8c99db0`
- `feature/original-limb-renderer-trace` — `11a2feea31d06744c720a5fb5d8313fc6925c28f`
- `feature/live-character-limb-ballistic-damage` — `b7a4892f71f7dece1924ef5c764397610eb933d9`
- `feature/characterbody-live-physiology-full-rig` — `1956ba2eaa51cf00729c3464857aff4d67f4956d`
- `feature/iphone-radial-joystick-icon-controls` — `b9454d65f1d2ef164890fe770eff55dcd666cb2e`
- `feature/source-limb-muscle-joint-trauma` — `d5672a00041258bb1b2e8c81c655672105197202`
- `feature/source-character-physiology-ragdoll-animation-sequencer` — `66c92d04c2fbdf720189fae534a20b3e1d3348f2`
- `feature/characterbody-original-animation-state` — `1453e685202141ba7924c56e4396a67dbfa53305`
- `feature/source-physiology-blood-oxygen` — `781b6a7115bbb43954eb652439b85d12f38cd60b`
- `feature/characterbody-dependency-primitives` — `cd86735d6ee92441f001fed4fc2601b72b2b7010`
- `feature/characterbody-original-facing-helpers` — `edaaada760a05958a53fe97fa8777f70d696af1b`
- `feature/full-source-characterbody-integration` — `b835196201650e7ef0be6efd9ca038b46e34259e`
- `feature/characterbody-original-movement-execution` — `ec3f67633493ade13d7c9de6b6a4ada0eaade477`
- `feature/analog-joystick-source-movement` — `b2bac4094d7ca702a60ee0f64d25c9bec30e9830`
- `feature/original-character-sequencer-next` — `598dffeb23ec869da306c6b3a341c1a1771fdfd3`
- `feature/animation-dependency-audit` — `6e779cfd7608d703cedc42869c29d582ac41892f`
- `feature/26-arm-kinematic-pose-bridge` — `a745d1faf6580b6a532957572840541d2122830b`
- `feature/26-hinge-spring-capsules` — `deee9e40769e23b74715dee0b27a2e4623f0d156`
- `feature/26-original-arm-dynamics` — `c6c5dab6af0a3e80bb83e682a86aa373a640d2c0`
- `feature/26-arm-physics-closure-gate` — `4db93ba6a4e6496a23af7cce93c7a3e8a497e9e0`
- `fix/original-m4-muzzle-flash-frame` — `3e6fd9d80313984297fb42097aaee161217b2ef9`
- `fix/m4-magazine-last-rounds-chamber` — `a2d51c2679f849b3e15cd24ea39c5294b6987b33`
- `feature/original-m4-magazine-touch-hud` — `337cee572d7047d5facacd8474a333860577e4a9`
- `feature/22-armcontroller-source-closure` — `a31a1dae5581680ba8c9f52f6cfefcce393fa538`
- `feature/22-limb-hand-source` — `5136018aad8d5898660b381c447af556e6849c3b`
- `feature/22-arm-controller` — `2aa507faf63b37e96bb37e47be9bdc591f7861fe`
- `codex/projectile-material-impacts` — `2e23cd20813f6bf3e4d2077146edb8daebb06ed4`

### (ii) Merged PR branch, touched-path content matches — safe, deleted

- `feature/extract-original-combat-dependency-sources` — `8b058baed299fd63918ebead1fd0b2efe0912ff1` (PR #63; zero differences over all PR-touched paths).

### (iii) Unique tip/content or touched-path differences — needs owner decision; preserved

- #74 `codex/ik-sequence-binding-20261026` — `79845d580fe8a186cc39411c5e0cef6801a40221`: binds seven recovered IK sequences to original FullSource controller fields and updates contracts. `develop/wasm` does not call `BindAuthoredSequences`; likely needed for Gate 5 integration, but retain pending owner decision.
- #73 `fix/touch-hold-fire-20261023` — `e6ac79c1c6439e543ad126a14e71f09dd75ea037`: touch hold-fire changes differ on three touched paths.
- #70/#72 `codex/game-scene-name-normalization-20261023` — `b3640f02009147eb7a3350489db382bb9ad30983`: scene-name/SpriteRenderer/ARGB32 load and Site touch parity changes; 24 touched-path differences.
- #67 `chore/live-state-guard-20261018` — `fa426667bfb24ecd08d8b3332d63bf563f30819b`: stale-state guard and handoff changes remain different.
- #65 `feature/extract-original-occupant-spawn-contracts` — `65d24413730b79fc4ee3c6d7994f24312646b3fd`: occupant spawn contract/tests; branch-only workflow included.
- #64 `feature/extract-original-occupant-spawner-sources` — `6eba31d557ecdbb6e007c8e3be9cfe34d4772f71`: recovered EquipmentList/OccupantSpawner/Occupants/SerializableOccupantSettings source refs absent at those paths.
- #62 `maintenance/foundation-first-master-plan-20261017` — `a2988eef5c2213fc877b74c09af26e631c5a8786`: old plan/handoff differs from current archived/rewritten records.
- #57 `feature/source-ikarmcontroller-live-callbacks` — `2b73044c107b875d8545d03541bf6b2d35f64654`: connects original IK controller callbacks to live character arms.
- #56 `feature/original-projectile-limb-hit-runtime` — `fb96e92a848409014b85f6e094d995a9376fb731`: projectile combat notifications and source injury chain.
- #36 `fix/original-impact-mesh-initial-load` — `2d832abf685cec6cd4a9dabe751aa7becca05952`: preserves Wood impact visuals during source mesh loading.
- #35 `feature/iphone-original-movement-joystick` — `7e5eec42b2c272b132272851977d5bc3145383ce`: original-action iPhone movement joystick.
- #34 `feature/26-source-arm-rig-import-audit` — `e71873f3d8c67eb330ff7ccb2b13cda819b0bc60`: authored arm hierarchy/physics import audit.
- #33 `fix/33-configured-grips-chamber` — `72662c9ddd2bef362d87e00c8403cff0633f3101`: configured-grip/chamber contract change; unique tip absent from other remote refs (audit count correction).
- #21 `codex/wasm-original-arm-sequences` — `097f1dfed194c670e736d7a4234c56a79766d16d`: original arm animation sequence dependencies.
- #20 `codex/full-source-closure-audit` — `e888d2938c6ac6b12c28381a4aa06e79cd7bfa5d`: full-source dependency audit and Unity UI type inventory.
- #19 `codex/wasm-compile-package` — `d67d446bd062466a4ec62fcf24bf49818825a5c9`: verifiable game/WASM package.
- #8 `codex/weapon-actions-checkpoint` — `a824915c08b2d945a77de7c33297453680ef8034`: browser M4 firing/reload/retained aim.
- #7 `codex/iphone-input-parity-foundation` — `ff2eed647e4eeb3446453b304bb4cbc4b6d31349`: movement/camera/scoped upper-body aim.

No branch in group iii was deleted. PR-head totals differ from the brief's 39/7/11: observed counts are 37/1/18 across 56 unique branch names; the 18 owner-decision set includes the 11 expected tips plus PR #33 and six merged nonancestor branches whose touched-path diffs were nonempty.

## Records and remaining work

- Docs top-level file count: **57 → 52**; docs subtree total: **284** files before and after. Five dated snapshots were moved to `docs/history/`; 13 retired-numbering gate docs carry the required superseded-numbering first line. The existing deployment-alignment document was already in history.
- `scripts/env/environment-preflight.sh` now checks pinned SDK, pinned Playwright Chromium, and push authentication together. The SDK and Chromium checks passed locally; no project source work was done.
- Main remains untouched. Owner options: **retire `main`**; **make `develop/wasm` the default branch**.
- Next work should follow the single-focus rule and the canonical gate index. Step 29 currently passes in hosted packaged Chromium; visual gait/foot-contact and device acceptance remain outside that demonstrated slice. Do not resume frozen audio, particle, inventory, or lineage threads unless they are on the locomotion critical path.
