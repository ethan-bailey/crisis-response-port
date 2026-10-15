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

## Next bounded action
Recover **actual** Shader Forge runtime behavior from compiled/native original Unity reference; dummy exports cannot supply it. In parallel, examine private particle module artifact `12008823420`, determine which original modules are active, and extract a source-hashed emitter renderer manifest without fabricating any particle parameters. Integrate only proven source behaviors, then run full browser CI once at an implementation milestone.

## End-of-session requirements
Append commits, source provenance, targeted test conclusions, artifact IDs and explicit untested gaps here. Update `NEXT_AGENT.md` to point at `WORKSPACE.md`. Never describe scheduled future work as active background execution; a later Work session or new user turn must execute it.
