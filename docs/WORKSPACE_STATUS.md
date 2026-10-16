# Workspace status — 2026-10-15

## Verified baseline and workspace start
- Workspace index: `WORKSPACE.md`; backlog [#10](https://github.com/ethan-bailey/crisis-response-port/issues/10), [#11](https://github.com/ethan-bailey/crisis-response-port/issues/11), [#12](https://github.com/ethan-bailey/crisis-response-port/issues/12), [#13](https://github.com/ethan-bailey/crisis-response-port/issues/13).
- Browser branch `codex/projectile-material-impacts`, PR #9, beginning HEAD `c8722f1db365117d2a4d9c13e1b5e3cbcd6ba451`. The separate Unity branch and main must remain untouched.
- Latest *tested gameplay commit*: `ed3d80d89f5e4e797e0370d9152bcc828bd8becb`, full native C#/WASM/PhysX/Chromium/WebKit **SUCCESS**, [run 38809439277](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38809439277), private artifact `12007339085` including built runtime and screenshots.
- Owner-private live Site v8: https://crisis-response-runtime.craftysnipe13.chatgpt.site; **not upgraded**. Live original-game parity and actual iPhone 16 testing are not established.
- Original window references: scene SHA `c43b36c415b4df14ef6223eaced686a8ddcead6c46f7504346aecb1a03c5743e`; 68 Quad MeshFilters, 56 regular glass and 12 mirror-glass; two original custom shaders are not yet ported. See `docs/WASM_ORIGINAL_WINDOW_VISUALS_2026-10-15.md`.

## Current track: #10 shader/source audit
Goal: inspect source shader assets before implementing exact glass/mirror blending, alpha clipping and UV behavior. No guessed ShaderMaterial or substitute mirror behavior. Use private source recovery artifact only. Record recovered shader source hash/path, referenced textures, custom shader stages, applicable render queues, and what is actually implementable in WebGL before coding.

## Change ledger
| Scope | Baseline / commit | Test evidence | Rollback |
| --- | --- | --- | --- |
| Workspace index `WORKSPACE.md` | `e8252206c6cb22f0c54f3e72e23345f1564cb6de` | Readback/commit evidence; documentation only | Remove file with new commit only, leave historical source/Unity branches alone |
| GitHub issue tracker | #10–#13 | GitHub issue records | Close/revise tickets, not code reset |

## Expanded work and complete-compile plan — 2026-10-15

- Full, dependency-ordered original game port roadmap (Gates 1–8 and actual production compile command sequence) committed: [`docs/COMPLETE_WASM_PORT_ROADMAP.md`](COMPLETE_WASM_PORT_ROADMAP.md), commit `7cc7888f146f9b6033b181904b72a7b47f246603`.
- Existing work issue links #10–#13 supplemented by [#14 all six scenes](https://github.com/ethan-bailey/crisis-response-port/issues/14), [#15 original AI/doors/grenades/missions](https://github.com/ethan-bailey/crisis-response-port/issues/15), [#16 UI/saves/platform](https://github.com/ethan-bailey/crisis-response-port/issues/16), [#17 deterministic native-versus-WASM comparison](https://github.com/ethan-bailey/crisis-response-port/issues/17), and [#18 private release build/device acceptance](https://github.com/ethan-bailey/crisis-response-port/issues/18).
- **Regular-glass shader:** recovered AssetRipper exporter file `Assets/Shader/Shader Forge_Window glass.shader`, GUID `c5c4e0da9b5e8fa4cb7c2f0db89a9c36`, SHA256 `7d9d9f70262201d46628597adb278caefe2ab11bb43b25c9f86c2ee37783b38f`, 63 lines, flagged `//DummyShaderTextExporter`. Its fragment only samples `_MainTex`, not source lighting/transparent behavior. Regular window browser visuals are explicitly an *adapter*, not native shader parity.
- **Mirror-glass shader:** recovered exporter file `Assets/Shader/Shader Forge_Mirror glass.shader`, GUID `fb32b7a5134a45a41997e05b8b5dc9e2`, SHA256 `f1b929eb65edb52abf46b352bc7bdd6f3aaf5684652c87b44a37a791011b17eb`, 48 lines, `//DummyShaderTextExporter` and a trivial constant-white fragment. Do not render guessed mirrors. Both shader audits: [Actions run 38812611701](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38812611701) **SUCCESS**, metadata-only artifact `12008763302`. [Issue #10](https://github.com/ethan-bailey/crisis-response-port/issues/10) updated with exact blockers.
- **Native original entry/exit particles:** original prefab field audit [run 38812872182](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38812872182) **SUCCESS**, 12 source BallisticMaterialProperties, 14 effect prefabs, **26 ParticleSystems + 26 ParticleSystemRenderers**. Every audited system includes serialized Initial/Emission/Shape/Color/Size/Rotation/Velocity/Noise/Collision/UV and other Unity modules; whether each is *enabled* must still be inspected. Private artifact `12008823420` holds original module and renderer fields, without adding fabricated particles. [Issue #11](https://github.com/ethan-bailey/crisis-response-port/issues/11) is next source-backed implementation track.
- No new original `runtime/Original/` edits, no new gameplay code deployed, no new browser/physical iPhone parity claims. Last complete browser verification remains `ed3d80d...`; documentation and source audit commits require no expensive game build.

### New change ledger

| Commit | Scope / rollback |
| --- | --- |
| `e8252206c6cb22f0c54f3e72e23345f1564cb6de` | Created workspace index; selectively remove new `WORKSPACE.md` only |
| `559355b1d99737e4d4ffcecb080457e84c0ab2a7` | Created checkpoint status ledger |
| `28d9f541e58ceed7053e5752070cdfe443a89ffc` | Added read-only original shader source audit; undo only audit script |
| `5ff8b30c8632482339f23f86ee295a223ed1d588` | One-shot source shader audit workflow, no browser rebuild |
| `a6c8dac58a65a5489e022b1cbfc8cd25cc56756d` | Added original serialized particle module audit script |
| `9f849323e6eb2070c8a6970b4ee8932f39ce02cb` | One-shot original particle source inventory workflow |
| `7cc7888f146f9b6033b181904b72a7b47f246603` | Created comprehensive full-game compiler/engine/parity/private-release checklist |
| `f152dc25b5703714ac0b381260689506099be7ec` | Updated workspace index with all issues and source shader/particle evidence |

Use GitHub `git revert` only after inspecting current HEAD and concurrent changes, or undo the listed changed docs/scripts/workflows via a new restoring commit. These commits do **not** change the verified v8 live deployment, main or independent Unity branch.

## Source-exact original ParticleSystem asset recovery — 2026-10-15, verified

- **Private branch remains isolated**; no edits to recovered gameplay `runtime/Original/`, independent Unity WebGL branch or `main`. Original compiled browser/gameplay test is still `ed3d80d89f5e4e797e0370d9152bcc828bd8becb` at successful run `38809439277`; source-data recovery and source-only test do **not** establish new gameplay parity.
- `scripts/audit_original_particle_sources.py` update `61a5e07f228c31dd8f97963a4673c81e88444a82` inventories source enabled/disabled module flags, render modes and original renderer refs. One-shot audit workflow `2c8f326033b515690954936c5d1725a1297b0084`: [run 38813633440](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38813633440) **SUCCESS**, private artifact `12010215957`.
- Full native audited source artifact was downloaded into the working container; inspected 14 prefabs, 26 emitter/renderer pairs, 14 original billboard modes and 12 original mesh modes. Source-only compact exporter `scripts/export_original_particle_manifest.py` committed at `ce5bfc0e662ab18bce28d71809ae631ed75e7633`.
- Source-preserving workflow `.github/workflows/recover-original-particle-source.yml` commit `1957364603832e5645c75115c5c067e2fbfc558e` ran [38814360107](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38814360107) **SUCCESS**; it first verified the repo remained private and branch SHA unmodified, then added only the original source data asset `game/assets/ballistics/original-particles-source.json.gz` at commit **`10c8fab81511a707a0b34408d11a80d487a77e7c`**.
- **Exact immutable recovered manifest compressed SHA256:** `75bbdbeadf3595f9c8fa32563a689fc119d6747abe0efb6c5bd0c347cd70cf92`. Uncompressed payload SHA256: `b2f02f66a909104049e6de8ed16d433027a1564740f129e196973779cf636c06`; 320,926 source bytes, 10,199 gzip bytes, gzip mtime 0. Source manifest includes original effect GUID, source prefab sha, unique original ParticleSystem/ParticleSystemRenderer file IDs, every enabled Unity module's original serialized fields/curves, sorted disabled module list, full original renderer fields and serialized original local Transform.
- The private artifact `12009642688` (`source-original-wasm-particle-manifest`) contains the manifest, original audit provenance, SHA file and exact input/output commits.
- Source-only Node regression `scripts/original_particle_source_test.mjs`, commit `6204fc5570fb9080c9581e6f0de45068f2fe1eab`. **2/2 Node tests PASSED** against the exact gzip recovered from the private GitHub artifact and validated its matching compressed SHA. This was a local terminal verification, *not* an additional GitHub browser/PhysX build. Use `node --test scripts/original_particle_source_test.mjs` from repo root in future CI.
- **No particle simulation or visual renderer has yet been added.** The next required step is implementing the **original Unity 2017 emitter modules** in explicit engine/render adapters, using immutable source fields and original event `Time.time`, then native reference/Chromium/WebKit/Safari parity tests. Do not draw guessed sprites, sparks, smoke or mesh effects.
- **Selective rollback**: if the source recovery is later invalidated, new reverting commits may remove `game/assets/ballistics/original-particles-source.json.gz`, `scripts/export_original_particle_manifest.py`, `scripts/original_particle_source_test.mjs` and `.github/workflows/recover-original-particle-source.yml` after checking HEAD and newer dependencies. Never reset PR branch or touch unrelated source, original shaders, Wood hole meshes, or Unity branch. Keep artifact `12009642688` as evidence.

## Full WASM browser validation and original particle mesh source recovery — 2026-10-15

**New complete game/browser regression SUCCESS:** [run 38814978462](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38814978462), tested exact gameplay/workflow SHA `f72102ec796784b1bdf02c44856e93cc2e7002ec`, private compiled WASM game/screenshots artifact `12009679203`. Original C# native contracts, WASM compile, PhysX, source material/window tests, game movement, landscape touch, presentation and weapons passed; the final original weapon smoke logs explicitly report **Chromium PASS / WebKit PASS**. New original source particle manifest Node tests ran and passed as part of the original input regression test step. No physical iPhone Safari test or new private hosting deployment occurred.

**Original Wood hole handedness bug fix:** `scripts/extract_original_exit_hole_mesh.py` (both raw and compressed paths) already negates Z and reverses original Unity triangle winding. Previously `game/original-impact-mesh.mjs` reflected the extracted vertices/winding a second time. Renderer commit `70c2c4e81d5ded19934c190cf069e1c7eaba8ece` now consumes right-handed source geometry directly and still converts live C# world poses. Chromium/WebKit regression commit `dd44bea6993f5c0a18f453b37dfbbb8055326dda` guards original positions/indices/source extraction format. [Exact geometry basis and selective rollback guide](ORIGINAL_MESH_BASIS_CHECKPOINT_2026-10-15.md).

**Original particle rendering dependency scan:** One audit run `38815290757` stopped because an original material referenced a Unity *built-in shader* with no recoverable `.meta` source file, GUID `0000000000000000f000000000000000`. The source-only script `scripts/audit_original_particle_mesh_materials.py` was corrected at commit `0124192f1b3600b41ba61df3a55de01e11bf6f05` to preserve that exact built-in GUID as an **unresolved Unity engine dependency**, never fake a shader. One scoped corrected [audit 38815413956](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38815413956) **SUCCESS**, private artifact `12009828520`.

The original 12 mesh-mode particle emitters reference three original source meshes:
- **Particle shard**, GUID `8ed663818ef63e741bd160cb133722fe`, `Assets/Mesh/Particle shard.asset`, SHA256 `661b5a9d7f3f45d558e9f6f11864eacb449e031a38561fffe51b6bff3d4fbdb1`; 12 original vertices/8 triangles; original normals decoded.
- **Tetraedron**, GUID `eedec0cf1e8d71a49b3949fbbc2cf3b4`, `Assets/Mesh/Tetraedron.asset`, SHA256 `906bd249eb17b8d5e2d41877b53a2eee335bb2590e55cd97de8263d09cc19c3f`; 12 vertices/4 triangles; original normals decoded.
- **Splinter**, GUID `f6fe08f20af97e149b26c77ded5823d7`, `Assets/Mesh/Splinter.asset`, SHA256 `37d2f797c49d4bb3055a65a25f50b059ebf41583638e6896fca76f75a4013876`; 12 vertices/4 triangles; source packed normals **exist but still require a verified decoder**.

The original materials are `Wood splinters.mat`, `Exterior concrete.mat`, `Wall chunks.mat`, `Glass particles.mat`, `White plastic particles.mat`, `Wood entry dust.mat`, `Plaster smoke.mat`, and `Projectile chunks.mat` (eight distinct source assets). Four original referenced textures and two shader GUID classes were audited (one built-in engine shader remains unavailable). No original source geometry was fabricated.

**Private original particle geometry import:** `scripts/extract_original_particle_meshes.py` commit `b80d6ebe32c0b08d9b93e68c97bbf8990041313f` validates original source asset SHA256 and decodes right-handed 3D geometry (no double reflection). One-shot protected source [run 38815708747](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38815708747) **SUCCESS** committed the three original source JSONs at SHA **`8c713bf3797098a7988a3a142b33ce8d62b635e4`** under `game/assets/ballistics/particle-meshes/`. Private evidence artifact **`12009763698`** includes exact original mesh output, input/committed SHA and file checks. The source-only integrity test `scripts/original_particle_mesh_source_test.mjs` commit `9d93cf982b64bcb27a059a11442da0468005d8bd` **passed 3/3 tests locally** for mesh GUID/hash/vertex counts/bounds/nondegenerate faces and source winding/normal agreement where decoded; *not* a new full game/browser CI build.

**Next source-backed integration gate:** Verify original particle mesh/material/shader/texture constraints (including built-in shader) and source ParticleSystem simulation semantics; connect the recovered emitter module fields and geometry to original C# material-effect event timing; test against native Unity capture before claiming graphics parity. Current new particle manifest and meshes are **data**, not rendered particles. The existing owner-private Site v8 remains unchanged.

**Selective rollback:** The new original-particle data can be undone without losing earlier game progress by reverting the source manifest addition commit `10c8fab` and the three-mesh addition `8c713bf`, plus companion extractor/tests/workflow commits only after verifying HEAD and newer dependencies. The Wood basis fix reverts as paired `70c2c4e`+`dd44bea`. Never force-push, reset PR #9 or touch independent Unity PR #7.

## Exact original Unity packed Splinter normals recovered — 2026-10-15 22:24 UTC

The original `Splinter.asset` packed normal stream has been decoded **without generated/synthetic normals**, closing the earlier source geometry blocker. AssetStudio's recovered compressed-mesh implementation confirms Unity packs normal X/Y float vectors plus a separate Z-sign bitstream. This exact native rule was implemented in `scripts/unity_packed_mesh.py` commit `7418d70e151455ec41b19a952c48f361e278cff2` and consumed in `scripts/extract_original_particle_meshes.py` commit `95824dc5636e02a03327db263580264044c6e3d0`. The generic source YAML loader coerced an all-decimal-digit hexadecimal sign stream to an integer; first recovery [38816598913](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38816598913) **failed safely**, never changed original mesh assets. Decoder handling `d219fdad02b11764d2dd34a873ac17ea8924123d` fails closed on ambiguous nonzero parsed integers. Source parser correction `4be14862ff53f8d3f523fdeb9e282ca4b9b0857d` **reads exact original `m_Normals` and `m_NormalSigns` hexadecimal bytes directly from Unity `.asset` YAML**, preserving leading zeros without guessing. Updated test `350d2b90db32149d619194d93edc7da3283acdc7` checks all three native meshes' decoded normals and their unit length.

Source-specific workflow run [38816798907](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38816798907) **SUCCESS**, exact workflow/source input commit `62c977f1433ba6041c21268069c2c46ba5d57860`. The three original mesh SHA/geometry/winding/normal tests passed (**3/3**, no errors). A SHA-guarded private CI commit **`da8e08ad6ec17e4f0ee2be55583658be1045498a`** updated only `game/assets/ballistics/particle-meshes/particle-mesh-f6fe08f20af97e149b26c77ded5823d7.json` with original packed Splinter normals; Particle shard and Tetraedron meshes unchanged. Private evidence artifact `12010519099` includes exact mesh outputs and input/output commits. `Splinter` now has 12 original decoded source normals (12 vertices/4 triangles), and all three mesh sources are already right-handed.

**Verification scope:** original-source SHA, packed decoding, geometry normal alignment and tests; **no particle simulator, shader behavior, material renderer, native Unity image parity, complete gameplay parity, or new physical iPhone test**. The latest fully tested *game* compilation is still [run 38814978462](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38814978462); no expensive full browser run for this source-data-only mesh patch. The owner-private published v8 remains unchanged. The original particle project work is tracked in [issue #11](https://github.com/ethan-bailey/crisis-response-port/issues/11).

**Rollback:** If the packed normals require correction after native Unity comparison, selectively revert commit `da8e08ad` and decoder/test scripts in reverse dependency order after checking newer edits, retaining earlier correct manifest/geometry and recovered evidence. Never force-push or reset PR #9, Unity PR #7 or main.

## Next bounded action
Recover **actual** Shader Forge runtime behavior from compiled/native original Unity reference; dummy exports cannot supply it. In parallel, examine private particle module artifact `12008823420`, determine which original modules are active, and extract a source-hashed emitter renderer manifest without fabricating any particle parameters. Integrate only proven source behaviors, then run full browser CI once at an implementation milestone.

## End-of-session requirements
Append commits, source provenance, targeted test conclusions, artifact IDs and explicit untested gaps here. Update `NEXT_AGENT.md` to point at `WORKSPACE.md`. Never describe scheduled future work as active background execution; a later Work session or new user turn must execute it.


## C# native impact event-to-source particle binder — 2026-10-15 (current session)

**New audited input contract:** `game/original-particle-event-binding.mjs` binds the *existing* C# `materialEffect` emission events to their exact source Unity impact effect GUIDs, 14 source prefabs and 26 emitter/renderer component IDs; validates material cross-references (8 original material GUIDs / 4 source texture assets), mandatory enabled modules, original renderer modes (14 billboard, 12 mesh), disabled module invariants, source phase, world pose and original scaled `UnityEngine.Time.time`. It exposes a bounded `recent` source-event journal (max 32) and counts of unsupported emitter requests.

**Important parity boundary:** No browser particle simulation, shader, synthesized shapes or placeholder visuals have been introduced; `renderedParticles=0` and `sourceVisualParity=false` are exposed explicitly. Source 3D meshes, original normals, material and emitter payloads are preserved for a later real Unity 2017 ParticleSystem compatibility engine, not used to fake one. Browser game startup now loads `original-particles-source.json.gz` and `original-particle-materials-source.json.gz`, and attaches provenance-checked bindings alongside the original `WeaponEffects` transport. Runtime `runtime/Original/` is unchanged.

### Exact commits / files (selective rollback)

| Commit | File and action |
| --- | --- |
| `3cc019ca7ab70a908b8e909232f9d1f3ca7d0d7f` | **Added** `game/original-particle-event-binding.mjs` |
| `b1bd9f55d9b9a4d63d8661263da4f78034d0d64c` | Updated `game/weapon-runtime.mjs` to validate original material-effect events |
| `918817a3c21e1177facb02b7509eedc9b6deb06d` | Updated `game/presentation.mjs` diagnostics |
| `61609054b78f167e593b93570288de746275a735` | Updated `game/index.html` to preload original particle assets and pass the source binder |
| `d28e7cc03c3b8eb0d8710f1acda78aae667e17b8` | **Added** `scripts/original_particle_event_binding_test.mjs` |
| `f44f4f607ef5dbd006438b760bf1f8f57ca520d0` | Updated `scripts/weapon_smoke.mjs` for Chromium/WebKit startup/diagnostic assertions |
| `06f7c8b643a77f53a5ad26ec8086cf1f6aa0e2cb` | Updated workflow `.github/workflows/verify-original-particle-assets.yml` with targeted Node unit tests |
| `0933f2d4f5c5047f4e4a7f7c701fb13fd0f47e8c` | Updated `.github/workflows/game-movement-smoke.yml` with one complete WASM/browser milestone test |

**Targeted tests VERIFIED SUCCESS:** source emitter/native normals/source material integrity + new binder Node tests, [Actions #38846393711](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38846393711), exact code SHA `06f7c8b643a77f53a5ad26ec8086cf1f6aa0e2cb`, private metadata artifact `12022284968`. Browser full milestone [#38846445335](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38846445335), exact code/workflow SHA `0933f2d4f5c5047f4e4a7f7c701fb13fd0f47e8c`, **pending at initial documentation**. Do not claim browser pass until status verified.

**Rollback:** Check current HEAD and later agents' commits. Revert new binder and its tests together with the three browser integration files, in reverse change order; do not reset/rebase/force-push the branch. The source gzip manifests, original normals, earlier glass/exit hole rendering, `runtime/Original/`, independent Unity PR #7 and owner-private live v8 are not affected. Re-run the targeted Node source verification at the reverted HEAD.

**Next exact task (#11):** capture native ParticleSystem execution reference for the source `InitialModule`, `EmissionModule`, `ShapeModule`, collision and renderer material shader of each of the 26 recovered effect systems. Then implement only verified original native module behaviors and renderer states; match native Unity 2017 event/position/seed traces, not guessed particle visuals. Do not close #11 or claim full-game parity based on the event binder.

