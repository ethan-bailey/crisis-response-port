# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; last verified code checkpoint at the start of this pass was local and remote `f7a9482af2578392a8791ab96bd55333f1a3e22a`; there were no open pull requests. This handoff records the subsequent documentation-only recovery-policy update.
- Gameplay source remains `e009c00d240b63e67d29f9499d4740bbc6669f55` (tree `6b8011f1b30691981a9a484fe8865b545ff6d68d`). Site v39 remains owner-only; the owner confirmed it starts and is playable on iPhone Safari. No deployment occurred in this pass.
- The supported target is iPhone Safari. Desktop Chromium/WebKit automation is optional diagnostic coverage. Obtain the owner’s exact-build iPhone result at major playable milestones and before release. Do not claim acceptance for features absent from the tested build.
- The owner-reported play-area hold behavior can stop sustained fire during aim changes. Keep it documented and deferred; do not conflate it with held-Fire-button behavior.

## Latest verified work

- `node --test scripts/*_test.mjs`: 78/78 passed on the checkout code at start of this pass.
- `python3 -m unittest discover -s scripts -p 'package_wasm_site_test.py'`: 13/13 passed.
- The first audit-tools run found two failures because `WeaponHost.cs` had a reviewed aim-convergence bridge delta that was absent from the frozen file hash. Updated the freeze manifest, recorded the scope in `docs/audit/actor-migration-boundary-review-2026-10-24.md`, and made the guard validate exception metadata. The targeted audit suite now passes 13/13; `check_actor_migration.py` reports zero violations.
- `npm ci --prefix game --ignore-scripts` followed by `npm run --prefix game vendor` restored the ignored lockfile-pinned PhysX browser adapter required by packaging. The reusable fix is in `docs/DOTNET_BUILD_RUNBOOK.md`.
- The current runner does not have the pinned .NET 8.0.426 SDK or process shim mounted. No .NET build, WASM publish, or exact-source package build was run in this pass. The checksum-verified SDK archive plus the tracked `/proc` shim is a previously successful recovery route; use it before classifying SDK absence as a blocker. GitHub CLI login is unnecessary when the connected GitHub API is available; the tested connector push/fetch path is documented in `AGENT_INSTRUCTIONS.md`.

## Gate state

- Gate 0: open. The bounded M4 source path is classified; normal-page startup, native physics, visible original actors, and supported navigation/presentation remain.
- Gate 1: passed within the scoped headless original-character acceptance.
- Gate 2: partial. Original source sequence playback is not connected to the normal game path; visual/physical continuity remains unverified.
- Gate 3: partial. A source shot reaches original limb physiology in headless evidence; native PhysX, rendered wounds, and full live state/event integration remain.
- Gate 4: partial. Original mission startup and headless spawning/outcome pass; the normal page lacks the complete original mission inputs and instantiation callbacks.
- Gate 5: open. Original trees load and tick, but autonomous perception, navigation, decisions, and combat are not verified.
- Gate 6: partial. Source room-clear and success contracts pass; a complete player/AI mission flow remains.
- Gate 7: open. Original presentation and content fidelity remain.
- Gate 8: closed for existing Site v39 under the owner-approved manual iPhone smoke path. This does not close Gates 0–7 or accept later features.

## Exact next action

Restore .NET SDK 8.0.426 using the checksum-verified archive and tracked `/proc` shim in `docs/DOTNET_BUILD_RUNBOOK.md`, then build and run the relevant source contracts on the current exact SHA. Continue Gate 2 by integrating original `IKArmSequencer` playback and callbacks into the real game path; do not synthesize defaults for missing mission context. Use the connected GitHub API for writes and verify by Git fetch; do not use GitHub CLI login. Keep working through Gates 3–7 in dependency order and do not deploy.
