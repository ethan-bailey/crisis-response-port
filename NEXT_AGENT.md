# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; verified source checkpoint: `fc727026b54d008b6e19aaf7881f272adf90aa79`. The final handoff/index commit contains no gameplay changes.
- No deployment occurred. Site v39 remains the owner-accepted iPhone Safari build. This pass changed a source contract and status documentation only; it did not alter gameplay behavior.
- Supported target: iPhone Safari. Desktop Chromium/WebKit automation is optional diagnostics. Obtain owner testing for the exact build at the next major playable milestone and before release; do not claim acceptance for features absent from the tested build.
- Keep the owner-reported play-area hold behavior distinct from holding the Fire button; it remains documented for later.

## Latest verified batch

- Extended `FullSourceControllerPlaybackContracts` to cover original `WaitForSiblingFrame` ordering across both arm sequencers. The left sequence stays blocked until the right sequence advances frame 0, then runs its callback on the following update. The test also asserts both sequencers complete.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore` passes on `fc727026b54d008b6e19aaf7881f272adf90aa79`; output reports `siblingWaitOrderingImplemented: true` and all source prefab, presentation, weapon, and ballistics contracts pass.
- Full runtime build and Release WASM publish pass with .NET SDK 8.0.426 (48 existing warnings, zero errors).
- Exact-source package build and verify pass at `fc727026b54d008b6e19aaf7881f272adf90aa79`: 767 files, 199 hashed Blazor resources, 195 valid WASM files. Package is `/tmp/cpr-site-fc7270`; it was not deployed.
- Repository file index regenerated and checked against that package: 2,183 files.
- A clean worktree requires restoring ignored PhysX vendor files with `npm ci --prefix game --ignore-scripts` and `npm run --prefix game vendor`. The checksum-verified SDK 8.0.426 archive plus tracked process shim is the working .NET recovery path; do not switch to CLI-based SDK setup.

## Gate state

- Gate 0: open. Dependency inventory and bounded M4 source-fire path are classified; normal-page original actor flow, native physics, authored visible actors, and broader navigation/presentation remain.
- Gate 1: passed within scoped headless original-character acceptance; not full-game parity.
- Gate 2: partial. FullSource controller/sequencer compilation, selected callbacks, actor isolation, and sibling-frame waiting pass headlessly. The normal page still does not play composed sequences on the real authored actor/rig; transition targets and visual/physical continuity remain.
- Gate 3: partial. Source projectile-to-physiology evidence exists; native PhysX, rendered wounds, and complete live state/event integration remain.
- Gate 4: partial. Headless source mission startup/spawning/outcome evidence exists; normal-page authored mission inputs and instantiation callbacks remain incomplete.
- Gate 5: open. Original trees load and tick; autonomous perception, navigation, decisions, and combat remain unverified.
- Gate 6: partial. Source room-clear/success contracts pass headlessly; complete normal-path player/AI mission flow remains.
- Gate 7: open. Original presentation and content fidelity remain.
- Gate 8: closed for the previously owner-accepted Site v39. Exact-source build/package checks also pass at `fc727026b54d008b6e19aaf7881f272adf90aa79`; no new gameplay behavior or device-acceptance claim is implied, and nothing was deployed.

## Exact next action

Continue Gate 2/Gate 4 by tracing the real normal-page actor startup and all required authored scene/prefab inputs for `FullSource.CharacterBody`, its controller/solvers, weapon targets, and equipment. Use an existing verified full scene package if present; do not repeatedly search/extract the recovery archive or invent defaults. If the authored package is unavailable, record that dependency once and continue with the next unblocked Gate 3 or Gate 7 implementation task. Run related checks in a batch, commit/push through the GitHub API, and never deploy.
