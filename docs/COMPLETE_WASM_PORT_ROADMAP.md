# Crisis Response — full original-game WebAssembly completion plan

**Complete-game acceptance checklist · execution-order review 2026-10-16.** The verified selected-source route is recovered C# on .NET 8 WebAssembly with Unity adapters and WebGL/PhysX. The separate native Unity Marksman compile also succeeded; actual browser execution is still unverified.

**Follow [NEXT_AGENT.md](../NEXT_AGENT.md) for current evidence and the dependency-ordered work queue.** First review the existing package/source-audit branches, then retain and browser-test a native feasibility player on a new isolated branch before choosing further engine work. Sections 1–8 below are acceptance gates, not a requirement to finish every source/visual task before testing a bounded scenario. Earlier run links remain historical evidence; latest scoped browser proof is run 38846445335, SDK-8 package proof 38849201407 and UI identity proof 38892941728. None establishes a finished game.

This is the step-by-step plan to deliver the **entire original game**, not merely a compiled Marksman/Mission 01 vertical slice. "100%" means every required original system and asset works to validated behavioral parity; it **cannot** be claimed from a successful compiler run. UI type identity is recovered (24/24 original types, PR #20), but layout/events/runtime behavior remain unfinished. The all-scene audit found 35 reachable dummy shader exports; the shader gap is not limited to windows.

No projected completion date or guaranteed 1:1 parity is assumed.

## 0. Evidence and immutable baseline — DONE for current slice, extend to the entire game

- [x] Recover original Unity 2017.2.0f3 / Windows x86/Mono game archive, preserve source hashes, ILSpy-exported gameplay C#, AssetRipper scenes and project files in private evidence. Existing recovery has 1,333 decompiled C# files and six scenes.
- [x] Identify a working browser path: .NET 8 Blazor WebAssembly executes selected original C# source in a Unity compatibility shim; browser uses original source-derived game assets, Three.js renderer and pinned PhysX WebAssembly.
- [x] Pass scoped native-contract and WASM publish tests [#38803362416](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38803362416).
- [x] Pass scoped Mission 01 C#/WASM + Chromium/WebKit tests [#38809439277](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38809439277), tested game SHA `ed3d80d89f5e4e797e0370d9152bcc828bd8becb`, private artifact `12007339085`.
- [ ] Build a **single complete source/asset manifest**, with SHA256, GUID/fileID dependency edges, original implementation path, adapter path, test fixture and current verification level for every game subsystem and scene.
- [ ] Establish original native Unity baseline runs or captures for each important gameplay sequence, where possible. Marksman-only Unity WebGL compile succeeded in run 38800542438; retained evidence contains logs/manifests only, with no actual browser gameplay proof. PR #7 remains read-only.

**Rollback:** Keep the currently passing browser SHA and owner-private live v8 as separate labeled baselines. Do not reset branches. Previous Site v8 lacks the newer glass renderer.

## 1. Recover every required original source and content dependency — [#14](https://github.com/ethan-bailey/crisis-response-port/issues/14)

- [x] Inventory all six scenes and static asset/GUID dependencies in the private source-audit report (PR #20).
- [ ] Extend that inventory to complete hierarchy/prefab override, local-fileID, built-in, dynamic Resources and scene-transition resolution; inventory alone is not closure.
- [ ] Recover original mesh topology/UVs/normals/material assignments/skin weights, textures, shaders, animations/curves/state machines, audio samples, UI assets, colliders, particle settings and resource references. Record lossy extraction and unresolved GUIDs.
- [ ] Resolve the known .308 casing mesh GUID `b4b69dc6c74eeb54cbd22d28e37f9a19` and two unsupported built-in mesh collider references, or fail loudly rather than substitute geometry.
- [ ] Inventory native library, network, file/registry, threading and Unity engine API call sites from **all** recovered gameplay files. Categorize mandatory, optional and unsupported dependencies.
- [ ] Confirm original ScriptableObjects, Resources paths and all prefab references resolve deterministically; provide per-scene manifest integrity tests.

**Gate 1:** A dependency checker walks every required source scene/prefab reference and reports zero *unaccounted* mandatory dependencies; all unknowns explicitly block parity (a successful build is not a waiver).

## 2. Complete compilation of original C# gameplay and Unity API compatibility — [#4](https://github.com/ethan-bailey/crisis-response-port/issues/4), [#12](https://github.com/ethan-bailey/crisis-response-port/issues/12)

- [ ] Extend `runtime/Crisis.Runtime.csproj`'s explicitly selected `Original/**/*.cs` and `Engine/**/*.cs` beyond its current subset. Add recovered original files incrementally; each addition must compile without replacing gameplay rules.
- [ ] Implement source-exercised Unity APIs: hierarchy/MonoBehaviour lifecycle and callbacks, object identity/destruction, prefab Instantiate/reference remapping, Transform incl. nonuniform shear, event/coroutine timing, Input and scaled/unscaled `Time`.
- [ ] Implement or source-validate Unity physics: rigidbody/contacts/constraints, collision events/triggers, layers/masks, compound/convex/mesh colliders, FixedUpdate ordering, transform synchronization, force/impulse application and PhysX settings.
- [ ] Implement animation: original Animator/controller states, triggers, transitions, IK, legacy Animation, ragdolls, blend trees, curve tangents/infinity modes and animation-event ordering.
- [ ] Implement Unity services needed by original game: AudioSource/mixer, resources, UI/EventSystem, particle system, scene manager, serialization, pathfinding, platform-facing APIs and editor-stripped behaviors.
- [ ] Compare original source method call traces with the adapter under deterministic time/input/RNG. Fail when a method is silently bypassed or substituted.
- [ ] Keep `runtime/Original/` immutable/source-grounded; necessary compatibility patches must be isolated, minimized, explicitly mapped and regression tested.

**Gate 2 (build):** `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj` and `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release -o <build-output>` succeed for every required original gameplay source. Confirm actual emitted .wasm binary. This verifies compilation and native contracts **only**, not game parity.

## 3. Recover all original gameplay behavior — [#12](https://github.com/ethan-bailey/crisis-response-port/issues/12), [#15](https://github.com/ethan-bailey/crisis-response-port/issues/15)

- [ ] Character locomotion, AI/navigation/cover, directional pose, animation state, standing/kneeling/sprinting, obstacle behavior, ragdolls and death.
- [ ] Weapon arsenal and ammunition: original firearms, cycling/recoil/aiming, reload tasks, magazines, malfunction/bolt actions, casing/shell meshes, firearm audio and animations.
- [ ] Projectiles/ballistics/penetration/ricochet/fragmentation, material/exit holes, gravity/raycast rules, wind/spin, window pass-through and damage events at source timing.
- [ ] Rigid-body reaction impulses, door interaction/breaching/traps, grenades/fuses/explosions, inventory/equipment, object activation, team commands and cover logic.
- [ ] Physiology: limbs, wounds, hemorrhage, vital functions, consciousness, treatment, death and associated UI/AI effects.
- [ ] Missions, win/lose scoring, objectives, campaign progression, scene transitions and original reset/restart handling.
- [ ] Compare every flow with original native Unity reference traces and tolerance-bound numeric calculations. Avoid inventing game rules even where a feature remains unavailable.

**Gate 3 (behavior):** All required original game interactions, not just a single room or weapon, have repeatable tests that pass against native reference behavior. Unit fixtures alone do not count.

## 4. Restore all original presentation systems — [#10](https://github.com/ethan-bailey/crisis-response-port/issues/10), [#11](https://github.com/ethan-bailey/crisis-response-port/issues/11)

- [x] Render original Wood Circle exit-hole geometry from original compressed 186-vertex/100-triangle source, with verified Unity-to-Three handedness.
- [x] Bind 56 original glass window Quads to source C# pixel stamps/reset and exact world matrices; expose that 12 mirror-glass panes are **not yet rendered**.
- [x] Read-only audit of **26 original ParticleSystems and 26 ParticleSystemRenderers** from 14 original entry/exit effect prefabs; private run [#38812872182](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38812872182), evidence artifact `12008823420`. Serialization inventory is not rendering.
- [x] Classify **enabled** original particle modules and renderer modes, private follow-up [#38813633440](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38813633440), artifact `12010215957`: **14 billboard (`mode 0`), 12 mesh (`mode 4`)**; Initial/Emission/Shape enabled for all; Collision enabled for 12, Color/Size/Rotation/InheritVelocity/ClampVelocity for 14 each. All other inventoried modules serialized but inactive on these 26. Still no particle simulation or native visual parity.
- [x] Audit two original Shader Forge window shader exports, private run [#38812611701](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38812611701) artifact `12008763302`. **Both are marked AssetRipper dummy exports**, not functioning native shaders: glass HLSL merely samples `_MainTex`, mirror HLSL returns constant white. Do not use them as parity references.
- [x] Recover original Unity `Particle shard.asset` (12 vertices, 8 triangles), `Tetraedron.asset` (12 vertices, 4 triangles) and `Splinter.asset` (12 vertices, 4 triangles) into private source-checked right-handed mesh buffers. [Private run 38815708747](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38815708747) **SUCCESS**, asset commit `8c713bf3797098a7988a3a142b33ce8d62b635e4`, artifact `12009763698`; separate three source geometry regressions passed locally. Particle rendering **not** implemented.
- [x] Recover **original native packed Splinter normals** from serialized X/Y pairs and sign bits, using AssetStudio's Unity decoder semantics and the exact original `.asset` YAML hex lexemes (the generic YAML parser may coerce all-digit hex to integers). [Private run 38816798907](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38816798907) **SUCCESS**, source mesh commit `da8e08ad6ec17e4f0ee2be55583658be1045498a`, private artifact `12010519099`, **3/3** source geometry/normal tests passed in CI. These are source data only, not rendered particles.
- [x] Inventory eight native particle material assets, four referenced source textures, and one engine built-in shader dependency. [Original asset provenance run 38815413956](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38815413956) **SUCCESS**, artifact `12009828520`; built-in shader GUID `0000000000000000f000000000000000` has no recoverable stand-alone source and must be treated as a parity blocker until verified.
- [ ] Obtain real shader behavior via compiled shader/native Unity capture or other independently verified original reference; reproduce regular and mirror glass blending, depth, lighting, alpha and UVs without guessing.
- [ ] Unpack original particle modules from private artifact (Emission/Shape/Initial/Color/Size/Rotation/Velocity/Collision/UV/Noise, renderer sort/material/sprites), preserve source source hashes/GUID and faithfully execute as engine adapter; never substitute spark/smoke generators.
- [ ] Recover original fragments, decals, impact effects, wall and glass occlusion, soft particles, shader variants, original camera/light/shadow/post-processing and animation/render ordering.
- [ ] Restore original audio sample selection, spatial falloff, mixer groups, 3D panning and user gesture unlock behavior. Compare original output under fixed shot/scene sequences.
- [ ] Record exact unsupported GPU behaviors, texture sampling, UV origin and transparency limitations on WebGL/iPhone.

**Gate 4 (visual):** Automated browser raster screenshot/geometry/material and event-lifetime tests pass against native reference captures across supported scenes, plus direct physical iPhone Safari visual inspection. Browser-only checks do not establish shader parity.

## 5. UI, platform abstraction and persistence — [#16](https://github.com/ethan-bailey/crisis-response-port/issues/16)

- [ ] Recreate source UI flow through original prefabs/Canvas/EventSystem, menus/settings/rebind/pause/mission/score screens; maintain original input semantics.
- [ ] Replace Windows Registry, file pickers, blocking HTTP/network threads, filesystem and other non-browser calls with explicit compatible platform adapters and tests; do **not** patch game rules around unsupported platform services.
- [ ] Match original save/load/reset data (or clearly document any incompatible storage format), persistence migrations and interrupted-session recovery.
- [ ] Touch controls are separate adapters to original actions. Verify touch ownership/multitouch, same-spot firing, cancellation, pointer lock, orientation, offline/online/cached assets and WebAudio unlock/interrupts.

**Gate 5:** Start, play, pause, load, save, lose/win, restart and resume work in the browser with native-equivalent game state and device-appropriate input.

## 6. Reference trace and regression evidence — [#17](https://github.com/ethan-bailey/crisis-response-port/issues/17)

- [ ] Capture input/timestep/RNG seeds, scene/asset SHA and original Unity reference state after each frame for representative scenarios.
- [ ] Compare transforms, interactions, shot trajectories, energy, armor, contacts, character/AI states, score, inventory, event/audio emissions, particles, shader-visible pixels and lifecycle.
- [ ] Distinguish exact comparisons for discrete logic from justified numeric tolerances for physics and renderer outputs.
- [ ] Build test-to-feature mapping with statuses **not assessed / recovered / parsed / executed / browser tested / native-parity checked / blocked**.
- [ ] Run reusable tests across Chromium and WebKit; add native comparison fixtures for every new original source subsystem.
- [ ] Keep original code/asset checksums, trace artifacts, failure reproductions and selected rollback commits reproducible.

**Gate 6:** There are no unresolved *required* parity blockers. If an original feature is unverified or missing, label the full game as incomplete.

## 7. iPhone Safari hardware acceptance — [#13](https://github.com/ethan-bailey/crisis-response-port/issues/13), [#18](https://github.com/ethan-bailey/crisis-response-port/issues/18)

- [ ] Test **on actual iPhone 16 Safari** at normal screen/landscape resolution: touch aim/firing/movement/reload; both simultaneous inputs and gesture cancellation.
- [ ] Check WebGL/WebAssembly startup and cache reload, TLS/MIME/security headers, asset loading, GPU memory, sustained frame time, overheating, background suspend/resume and audio behavior.
- [ ] Verify full game story/mission playthrough on device, not merely a 30-second movement/firing smoke test.
- [ ] Set performance acceptance thresholds from original gameplay and actual device observation; profile conversion/adapters without changing simulation.
- [ ] Store test date, iOS version, screenshots, measured stability and untested limitations. Browser-engine WebKit tests in GitHub CI are *not* an actual iPhone test.

**Gate 7:** User/device verifies the same commit deployed, with no unaddressed crashes or gameplay regressions across required scenarios.

## 8. Reproducible compile and private release — [#13](https://github.com/ethan-bailey/crisis-response-port/issues/13), [#18](https://github.com/ethan-bailey/crisis-response-port/issues/18)

**Exact existing known-working CI build flow**, from `.github/workflows/game-movement-smoke.yml`:

```sh
npm ci --prefix game --ignore-scripts --no-audit --no-fund
node scripts/vendor_physics.mjs
python3 scripts/build_scene_binary.py game/assets/mission01.obj game/assets/mission01.crg
dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj
cp game/browser-input.mjs runtime/Browser/wwwroot/browser-input.mjs
dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release -o work/game-runtime
mkdir -p work/game-site/runtime-web
cp -a game work/game-site/game
cp -a work/game-runtime/wwwroot/. work/game-site/runtime-web/
sed -i 's#<base href="/">#<base href="/runtime-web/">#' work/game-site/runtime-web/index.html
# CI runs Node tests, serves work/game-site via HTTP and exercises Chromium+WebKit Playwright.
```

Do not run these steps on the user's iPhone; use the private GitHub Actions cloud runners and deliver one-click owner-only website.

- [x] Current scoped pipeline produces .NET WASM and browser assets and passes headless smoke tests.
- [ ] Expand build inputs for *all* original scenes and systems, pinned compiler/tool versions, deterministic source/asset hashes, JS package integrity and PhysX backend.
- [x] Add source/output SHA inventories and strict boot/WASM integrity checks for the selected-source package (PR #19, run 38849201407).
- [ ] Extend verified provenance to the complete-game release, including original archive/assets and all selected route inputs; never mix tested commits.
- [ ] In CI, run native C# contracts, test suites, browser input/camera/physics/weapons/AI/UI/mission/particles/shaders against matching source.
- [ ] Package one self-contained protected release with working base path, caching, CSP/CORS, offline considerations and correct MIME types; assert no missing assets at HTTP runtime.
- [ ] Deploy to verified **owner-restricted** host only after checks; retain old deployment ID, independent verification, and rollback procedure; do not automatically merge PR or publish original-game files publicly.
- [ ] Record source SHA, compiled artifact hash, actions run, private deployment ID/URL, rollback ID and physical iPhone result before calling a build final.

**Gate 8 (release):** The uploaded protected website serves the exact tested complete-game binary/asset set and passes regression plus actual-device acceptance. A link to an older playable prototype does not satisfy this gate.

## 9. Definition of done — no guessing a completion percentage

Full compilation is **technically feasible for the existing slice and already verified**; full original game reconstruction is a separate, unfinished engineering program. Completion requires every gate (1–8) above, no unaccounted required source dependencies, source-backed gameplay implementation and representative reference parity across all six scenes. Maintain a clear list of blocked/unverified systems. Native Unity Shader Forge original compiled graphics may not be recoverable from AssetRipper dummy exports, so 100% exact visual parity may require original playable runtime/reference and further research.

**Session protocol:** Work on one dependency-ordered issue; preserve original code; commit only testable changes; append exact hashes/CI/deployment evidence to `docs/WORKSPACE_STATUS.md` and update `NEXT_AGENT.md`. CI is milestone-triggered, not per-source-edit. Stop only at a documented checkpoint if tools/credits/session access end. Do not promise work after the session without an actual scheduled automation.
