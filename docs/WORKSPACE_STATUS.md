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

## Next bounded action
Recover **actual** Shader Forge runtime behavior from compiled/native original Unity reference; dummy exports cannot supply it. In parallel, examine private particle module artifact `12008823420`, determine which original modules are active, and extract a source-hashed emitter renderer manifest without fabricating any particle parameters. Integrate only proven source behaviors, then run full browser CI once at an implementation milestone.

## End-of-session requirements
Append commits, source provenance, targeted test conclusions, artifact IDs and explicit untested gaps here. Update `NEXT_AGENT.md` to point at `WORKSPACE.md`. Never describe scheduled future work as active background execution; a later Work session or new user turn must execute it.
