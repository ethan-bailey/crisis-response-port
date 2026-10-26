# Current handoff — 2026-10-26

## Exact durable checkpoints

- Development branch: `develop/wasm`.
- GitHub branch head used for this release: `90cc14b2362d868fd921138a69c74092612394b6` (`Record stretched-billboard checkpoint and open gates`). Its code parent is `914a50b35c468cd1fe91fe116715549e08671653`; it descends from binary-batch transport commit `135749e2b4afbc793ee6c17d582849050673df56`.
- The exact GitHub files added/changed after `135749e2...` were blob-verified and mirrored into the private Site source repository. Site source commit: `e03087487359e61dce2b0db0345f1dc8818bb876` (`Sync source particle shape and render modes`). The separate Site history also retains `.openai/hosting.json`.
- Live owner-private Site: `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.
- Deployed Site version 67: `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_f2e9392519e48191a8a66434e9545d5f`.
- Deployment: `appgdep_6adea77b4ab481918c3aafca4051d9fc`, succeeded at `2026-10-26T01:06:13Z`. Audience remained custom/owner-only.

## What this release closes

1. Replaced the incremental-source bridge's array of Base64 strings / `.NET byte[][]` arguments with one bounded flattened `Uint8Array`, per-entry lengths, and ordered slicing in .NET. This removes both the prior `ByteArrayJsonConverter` argument failure and the Base64 allocation expansion while retaining fail-closed gzip/hash verification and batches of at most 16 assets / about 1 MiB.
2. Preserved the complete six-root Mission profile: Global Context, Interior Mode UI, Interaction Menu, Operator, Version, and Mission 01. The regenerated closure contains 1,919 assets / 22,365 records, 97,767,549 compressed asset bytes, and 205,514,345 source asset bytes. Regenerated source package SHA-256: `896bc8b990ec7554bdfd2c9c73f7ce3e7254fed1036f51c709c9ecb970d1f62d`.
3. Included the current source-backed particle slices: impact-event/controller closure, authored curves/gradients, cone/circle emission, all seven inventoried hemisphere systems, and all six source stretched-billboard renderer records. `sourceVisualParity` correctly remains false.

## Exact build, package, and test evidence

- Recovery archive `crisis-response-recovery-direct-v3-1.zip` was materialized from the private Library and matched pinned SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d` before extraction.
- Pinned .NET SDK 8.0.426 Release publish of `runtime/Browser/Crisis.Runtime.Browser.csproj` succeeded from Site source `e0308748...` with 0 errors. The optional `wasm-tools` optimization workload was unavailable; this is not a correctness failure.
- Node contracts: **122 passed, 0 failed** (`node --test scripts/*_test.mjs`).
- Python suite: **41 passed, 0 failed** (`python3 -m unittest discover -s scripts -p '*test.py'`).
- EngineChecks: **75 passed**. FoundationChecks: **121 passed**.
- Exact package: `/tmp/cpr-site-package-e030874`; independent verifier result: `status: verified`, 2,684 files, 199 Blazor resources, 195 WASM files, and all 1,919 bounded source assets. `build-manifest.json` SHA-256: `3b54dc10517c3a77188246dcc8726473ad54288b922aa4a8b0ef981d22e5cf8f`.
- Exact deployment archive: `/tmp/cpr-site-runtime-e030874.tar.gz`, 138,686,792 bytes, SHA-256 `9c71dbe0ade190d082262c3bf4ab554a127c64f6e7a076d94427dfcbc0f5463a`. A separate streaming audit byte- and SHA-compared all 2,684 manifest files inside `dist/` and verified `.openai/hosting.json` before deployment.
- The interim verified `b184be96...` deployment made earlier in the same run is superseded by version 67 and must not be used as current acceptance evidence.

## Remaining gates in dependency order

1. **Acceptance for exact Site version 67.** Run Mission startup through completion in a WebGL-capable browser and on the owner's physical iPhone Safari. Capture loader timing/memory, completion past asset 1,900, movement/joystick sprint resistance, absolute vertical aim, firing/reload/audio timing, tap-zoom prevention, readable/copyable errors, physics/casing behavior, and particle pixels. The available cloud browser previously reported WebGL disabled; that is a concrete environment blocker only for rendered/browser evidence, not for source work.
2. **Complete the already inventoried 51-system particle lifecycle.** Continue with exact serialized owners/callbacks for `Emitter`, `ProjectileTrace`, weapon smoke, blood/wound effects, bounce, pause/time-scale, and destruction ordering. Implement active Collision, Clamp Velocity, Inherit Velocity, Force, Rotation, Size, Color, and Burst modules plus Unity random-state reproduction. Add source-backed tests per batch. The original `Spark.png` bytes (GUID `43907091bffd53b478c985dca2762559`, expected SHA-256 `6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344`) remain absent from both recovered and checked-in sources; defer only that exact texture-dependent visual path and do not substitute artwork.
3. **Broader gameplay parity.** Inventory and close complete dependency sets before implementation for projectile/damage outcomes, ragdoll/joints, authored mission/NPC flow, remaining animation/IK, and interaction/menu systems. Use runtime failures only for genuinely hidden gaps.
4. **Release discipline.** For every later source batch, push both source remotes as applicable, rebuild with SDK 8.0.426, regenerate/copy the exact bounded Mission profile, run package verification plus archive byte audit, and deploy only the exact pushed Site commit. Update this handoff with immutable hashes and IDs.

## Next action

Begin the next source-backed particle lifecycle batch from the complete 51-system inventory while version 67 awaits target-device acceptance. Do not block independent implementation on the unavailable physical iPhone or missing Spark texture; keep those two acceptance/asset gaps explicit and continue every unaffected module and lifecycle path.
