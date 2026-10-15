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

## Next bounded action
Source-audit shader GUID `c5c4e0da9b5e8fa4cb7c2f0db89a9c36` and mirror GUID `fb32b7a5134a45a41997e05b8b5dc9e2` from private recovered original assets. After verification, use original shader behavior to build a targeted transport/visual parity fixture; run full browser CI only after meaningful code changes.

## End-of-session requirements
Append commits, source provenance, targeted test conclusions, artifact IDs and explicit untested gaps here. Update `NEXT_AGENT.md` to point at `WORKSPACE.md`. Never describe scheduled future work as active background execution; a later Work session or new user turn must execute it.
