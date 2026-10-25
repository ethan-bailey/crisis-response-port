# Current handoff — 2026-10-25

## Latest source and release checkpoint

- GitHub branch: `develop/wasm`.
- Exact tested GitHub source commit: `0291d458c7cde7295eb404bea1dc59c00a9c6d45` (`Show startup errors when WebGL is unavailable`). The GitHub branch was fetched after push and matched this SHA; the checkout was clean at verification.
- Site source branch: `main`; exact pushed Site source commit: `ab0e353fd8930f8107bf9144279e7b14217a9e9e`. This Site checkout contains the same tracked project source plus its required `.openai/hosting.json`; the Site commit SHA is therefore the source identity for the deployed package.
- Release WASM package built from Site commit `ab0e353fd8930f8107bf9144279e7b14217a9e9` at `/tmp/cpr-site-package-ab0e353`. `python3 scripts/package_wasm_site.py verify /tmp/cpr-site-package-ab0e353` returned `status: verified`: 2,683 manifest files, 199 Blazor resources, 195 WASM files, and the bounded `global-ui-mission-operator` profile (1,919 assets; 97,767,549 compressed bytes; 205,514,345 source bytes). The 220 MB original-scene diagnostic archive is omitted (`sceneSmokeIncluded:false`).
- Package `build-manifest.json` SHA-256: `12b53390d40b58a48e985ecffa178a9f8168674415966acbb3ec056d1fbdfcbd`. Toolchain: .NET SDK 8.0.426, Node 24.22.1, npm 11.9.0, Python 3.12.14, linux-x64; `wasmTools:false` (publish succeeded without optional AOT optimization).
- Final Sites archive `/tmp/cpr-site-release-ab0e353.tar.gz` SHA-256: `e82bc7d6bb8ce80c8a8091ccd5680484ade37d6ab0ad703a05d04b8cd9b5867a`. Independent archive check matched all 2,684 package files byte-for-byte and the one additional `.openai/hosting.json` entry (2,685 total files; no missing, unexpected, or mismatched entries).
- Private production deployment **succeeded**: Site `appgprj_6acfef7174888191bc2bc2f269f60acc`, version `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_db407a2b8668819182dd0dcdc48bbfec`, deployment `appgdep_6ade79e673c88191ab07d01028c40f55`, updated 2026-10-25 21:51:44 UTC. URL: `https://crisis-response-runtime.craftysnipe13.chatgpt.site` (owner-only access preserved; publish-on-push is disabled).

## Exact verification completed

- `node --test scripts/*_test.mjs`: **116 passed, 0 failed** at source `0291d458`.
- `python3 -m unittest discover -s scripts -p '*test.py'`: **41 passed, 0 failed** at source `0291d458`.
- `runtime/EngineChecks`: **75 assertions passed**.
- `runtime/FoundationChecks`: **121 assertions passed**.
- `dotnet build runtime/Crisis.Runtime.csproj --no-restore --no-incremental -v:q`: passed, **48 warnings, 0 errors**.
- Release `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release` passed from the GitHub checkout and again from the exact Site source checkout. The only publish limitation was the optional `wasm-tools` optimization workload.
- Live browser acceptance of the error path on the deployed version: reloaded `/game/` in the available cloud Chrome and confirmed the error panel, `Copy details`, and readable stack are shown when the renderer fails. The browser reports `GL_VENDOR = Disabled` and `Error creating WebGL context` at `game/index.html:179`; this runner cannot create WebGL. This is a concrete runner capability blocker for the playable graphics smoke, not evidence that iPhone Safari lacks WebGL. No successful Mission startup was claimed.
- Sites deployment status was read back as `succeeded`; Site Worker logs returned no events in the 180-minute window.

## Changes in the latest commit

`game/index.html` now registers top-level `error` and `unhandledrejection` handlers before constructing `THREE.WebGLRenderer`. Failures that occur before the regular Mission initialization catch now use the existing accessible, scrollable, copyable error panel. The deployed cloud-browser check confirms the WebGL failure is visible and copyable instead of leaving a blank stage with no explanation.

## Dependency-ordered remaining gates

1. **Playable browser acceptance on a WebGL-capable runner.** The current cloud Chrome has hardware/software WebGL disabled, so renderer construction stops the module before Mission controls are enabled. Use a browser runner that can create a WebGL context and test the deployed version above. Check the actual mission end-to-end: 1,919 asset delivery completes (especially the reported slowdown near 24 MiB and prior stall near 1,900 assets), actor initialization, rendering, physics/casings, input, absolute aim, walk/sprint rim resistance, weapon/audio timing, and visible error/copy flows. Record transfer diagnostics, timing, memory, and exact version ID. Do not diagnose a network stall from the WebGL-disabled run.
2. **Physical iPhone Safari acceptance.** On iPhone Safari, validate that the 99 MB bounded source load completes, the end-of-load UI stays responsive, long-session memory remains stable, and movement/aim/weapon/audio/tap-zoom behavior meets the interaction contract. The physical device is not available to this runner. The previously supplied Safari memory trace is from an earlier deployment and does not validate this version.
3. **Close source particle behavior against the already completed dependency inventory.** The full 51-system asset/material/texture/mesh/shader/module/owner closure and the 14 impact-controller/26 emitter closure are recorded in `docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md` and `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`. Browser rendering now covers the 26 one-shot impact emitters using source curves/gradients and cone-base/circle-edge shapes. Remaining concrete behaviors: reproduce Unity's PRNG from the transported seed; shape type 2; renderer mode 1 and shader parity; active bursts/modules, collision, clamping/inherited/force velocity, and lifecycle across all 51 systems; blood/wound, tracer, smoke, bounce, pause/time-scale, and destruction ordering. Add source-backed cases per behavior before claiming parity.
4. **Close remaining gameplay parity in dependency batches.** Inventory each source gate's complete prefab, component, serialized field, animation/state-machine, material, and callback dependencies before implementation. Then run exact-source tests/build/package verification. Areas still outside the current completion claim include broader NPC/occupant behavior, ragdoll/joints, interactions/menus, remaining animation/IK paths, and full mission scene behavior.
5. **For any source change after this checkpoint:** commit and push to `develop/wasm`; sync the exact source tree to the Site `main` branch; build/publish from that pushed Site commit; run the package verifier and archive byte comparison; only then save and privately deploy. Update this file with new source/Site/version/deployment IDs and observed results. No older package or manifest hash is evidence for a later commit.

## Do not conflate these states

- The build, bounded package integrity, Site archive integrity, and private deployment gates passed for Site source commit `ab0e353`.
- This is not a 100% game parity claim. The playable WebGL smoke and physical iPhone Safari acceptance are not verified on the current version; the WebGL runner incapability and device unavailability are the exact blockers. Continue source-backed implementation and any independently testable gates instead of treating those labels alone as a stopping point.
- The 86 MB original-scene diagnostic archive is intentionally absent from the phone startup package; its omission is safe for the bounded profile and recorded as `sceneSmokeIncluded:false`.
