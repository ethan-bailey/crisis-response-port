# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; verified source checkpoint: `e035b9fe8dd40d44dadc0ddbcc5a5f6bb1ccc95b`. A following handoff/runbook/index commit contains no gameplay changes.
- No deployment occurred. Site v39 remains the owner-accepted iPhone Safari build. This pass changed a source contract and status documentation only; it did not alter gameplay behavior.
- Supported target: iPhone Safari. Desktop Chromium/WebKit automation is optional diagnostics. Obtain owner testing for the exact build at the next major playable milestone and before release; do not claim acceptance for features absent from the tested build.
- Keep the owner-reported play-area hold behavior distinct from holding the Fire button; it remains documented for later.

## Latest verified batch

- Corrected Unity 2017 AnimationCurve infinity mapping in `SceneLoader`. Serialized `m_PreInfinity/m_PostInfinity = 2` is now preserved as source metadata and evaluated as clamped instead of being miscast to this runtime's `WrapMode.Loop`.
- Extended the real recovered-prefab lifecycle gate: the actual Operator now runs its original two-arm `IKArmController.Unarmed` sequence to the authored Pelvis target. Both actor-owned solvers reach `OnTarget`, both IK targets bind to the Operator's Pelvis, and both sequencers complete while the independent Occupant exists in the same world.
- Added a focused prefab regression proving clamped out-of-range curve evaluation and independent curve clones. See `docs/evidence/GATE2_AUTHORED_IK_TRANSITION_2026-10-24.md`.
- General runtime contracts, the recovered Operator/Occupant lifecycle contract, EngineChecks (69), FoundationChecks (121), related non-browser JS contracts, full Release runtime build, and Release WASM publish pass with .NET SDK 8.0.426. The full build retains 48 existing warnings and zero errors.
- Exact-source package build and verify pass at `e035b9fe8dd40d44dadc0ddbcc5a5f6bb1ccc95b`: 767 files, 199 hashed Blazor resources, 195 valid WASM files. Package is `/tmp/cpr-site-e035b9-final`; it was not deployed.
- Package invocation correction is persistent in `docs/DOTNET_BUILD_RUNBOOK.md`: when `dotnet publish --output <DIR>` is used, pass `<DIR>/wwwroot` (not `<DIR>`) to `package_wasm_site.py --published`.
- Optional Playwright smoke was unavailable because Playwright is not installed. Desktop browser automation is optional and this did not block the batch.
- The tracked scene package contains 1,504 assets and both character prefabs, but not Global Context or M4A1 resource prefabs. It is not the private full gameplay-resource package; do not retry spawner/equipment contracts against it.
- A clean worktree requires restoring ignored PhysX vendor files with `npm ci --prefix game --ignore-scripts` and `npm run --prefix game vendor`. The checksum-verified SDK 8.0.426 archive plus tracked process shim is the working .NET recovery path; do not use `dotnet-install.sh` or GitHub CLI setup.
- The complete original project recovery ZIP is now durably saved in the private ChatGPT Library as `crisis-response-recovery-direct-v3-1.zip`; SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d` verified on 2026-10-24. Search the Library by exact filename and verify the hash after materializing. Keep the archive and extracted proprietary project out of this public repository. Full instructions are in `docs/DOTNET_BUILD_RUNBOOK.md`.
- Full dependency-closure batch: regenerated the package from that archive, then indexed Global Context, Mission 01, Operator, M4A1/5.56-FMJ, and Glock-18/9-FMJ closure. The index contains 1,918 assets / 22,361 records, zero missing package refs, and 39 assets with unresolved/unsupported refs. Prefab lifecycle + authored IK, full mission startup, and spawner/weapon/physiology contracts passed against the same package. Evidence and reproducible indexer: `docs/evidence/FULLSOURCE_BROWSER_CLOSURE_BATCH_2026-10-24.md` and `scripts/index_fullsource_runtime_closure.mjs`. The generated 925 KB index remains workspace-local by design; do not ship the full closure to the browser.

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

Continue Gate 2 using the recovered archive and closure index: create a compact authored payload for the FullSource Operator and required Global/Mission/UI initialization, then add a single-owner RuntimeBridge entry point. Route browser inputs and source ticks through the FullSource actor; synchronize its root/limb poses and physical commands to the renderer/PhysX adapter; migrate weapon callbacks to that actor before removing transport authority. Do not put the full 1,918-asset closure in the browser payload. Run lifecycle, controller, IK, movement, and weapon checks together. Commit/push through the GitHub API and never deploy.
