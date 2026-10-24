# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; live head was `eb8afdcfa2772d6d65ba736cc94ae29fe7543b0c` before this batch. Record the pushed source and final handoff SHAs below after the GitHub API commits.
- No deployment occurred. Site v39 remains the owner-accepted iPhone Safari build. This pass changed a source contract and status documentation only; it did not alter gameplay behavior.
- Supported target: iPhone Safari. Desktop Chromium/WebKit automation is optional diagnostics. Obtain owner testing for the exact build at the next major playable milestone and before release; do not claim acceptance for features absent from the tested build.
- Keep the owner-reported play-area hold behavior distinct from holding the Fire button; it remains documented for later.

## Latest verified batch

- Corrected Unity 2017 AnimationCurve infinity mapping in `SceneLoader`. Serialized `m_PreInfinity/m_PostInfinity = 2` is now preserved as source metadata and evaluated as clamped instead of being miscast to this runtime's `WrapMode.Loop`.
- Extended the real recovered-prefab lifecycle gate: the actual Operator now runs its original two-arm `IKArmController.Unarmed` sequence to the authored Pelvis target. Both actor-owned solvers reach `OnTarget`, both IK targets bind to the Operator's Pelvis, and both sequencers complete while the independent Occupant exists in the same world.
- Added a focused prefab regression proving clamped out-of-range curve evaluation and independent curve clones. See `docs/evidence/GATE2_AUTHORED_IK_TRANSITION_2026-10-24.md`.
- General runtime contracts, the recovered Operator/Occupant lifecycle contract, EngineChecks (69), FoundationChecks (121), related non-browser JS contracts, full Release runtime build, and Release WASM publish pass with .NET SDK 8.0.426. The full build retains 48 existing warnings and zero errors.
- Optional Playwright smoke was unavailable because Playwright is not installed. Desktop browser automation is optional and this did not block the batch.
- The tracked scene package contains 1,504 assets and both character prefabs, but not Global Context or M4A1 resource prefabs. It is not the private full gameplay-resource package; do not retry spawner/equipment contracts against it.
- A clean worktree requires restoring ignored PhysX vendor files with `npm ci --prefix game --ignore-scripts` and `npm run --prefix game vendor`. The checksum-verified SDK 8.0.426 archive plus tracked process shim is the working .NET recovery path; do not use `dotnet-install.sh` or GitHub CLI setup.

## Gate state

- Gate 0: open. Dependency inventory and bounded M4 source-fire path are classified; normal-page original actor flow, native physics, authored visible actors, and broader navigation/presentation remain.
- Gate 1: passed within scoped headless original-character acceptance; not full-game parity.
- Gate 2: partial. FullSource controller/sequencer compilation, selected callbacks, actor isolation, sibling-frame waiting, and a real recovered Operator two-arm transition to its authored Pelvis target pass headlessly. The normal page still uses the narrower transport actor rather than this FullSource actor; weapon/equipment sequence composition and visual/physical continuity remain.
- Gate 3: partial. Source projectile-to-physiology evidence exists; native PhysX, rendered wounds, and complete live state/event integration remain.
- Gate 4: partial. Headless source mission startup/spawning/outcome evidence exists; normal-page authored mission inputs and instantiation callbacks remain incomplete.
- Gate 5: open. Original trees load and tick; autonomous perception, navigation, decisions, and combat remain unverified.
- Gate 6: partial. Source room-clear/success contracts pass headlessly; complete normal-path player/AI mission flow remains.
- Gate 7: open. Original presentation and content fidelity remain.
- Gate 8: closed for the previously owner-accepted Site v39. Exact-source build/package checks also pass at `fc727026b54d008b6e19aaf7881f272adf90aa79`; no new gameplay behavior or device-acceptance claim is implied, and nothing was deployed.

## Exact next action

Continue Gate 2 by migrating the normal page's explicit `Crisis.Runtime.Weapons.CharacterBody` transport boundary toward the compiled `Crisis.Runtime.FullSource.CharacterBody` actor without running two competing arm solvers on the same visual rig. The tracked scene package is already classified and is not the full gameplay-resource package; do not repeat that probe or invent missing Global/M4 resources. If the normal-page migration requires the unavailable private package, move in the same batch to an unblocked Gate 3 native-physics/live-wound task or Gate 7 original-presentation task. Run related checks together, commit/push through the GitHub API, and never deploy.
