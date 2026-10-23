# NEXT AGENT — Crisis Response WASM

## Checkpoint

- Working environment: Codex Work Mode repository checkout. Use it for code, builds, and tests; scratch state may disappear, so commit and push repository work.
- Integration branch: `develop/wasm`. This handoff is included in the integration of PR #69; query the live branch head and open-PR list before starting new work.
- PR #69's browser publish link and root SDK pin are present on the integrated source tree `bba140f1e366411f433d1a4a7d34bce029e960b1` (commit `b0c2fb852d275901dbebb667f3353f1943d5683a` before this documentation integration).
- Local relevant check: `python3 scripts/package_wasm_site_test.py` passed 12/12 on a checkout with that exact source tree. The test exercises fixture package generation and manifest integrity.
- Static checks confirmed root `global.json` matches `tools/wasm-compile/global.json`; `runtime/Browser/Crisis.Runtime.Browser.csproj` links the tracked `game/browser-input.mjs` to `wwwroot/browser-input.mjs`. Source SHA-256: `e720dee5dac385e4f4e9a0664924637860df2d82047e56a253a5d71ad0d99725`.
- This Work Mode shell currently has no `dotnet` executable. A fresh Release build, Blazor publish, published-file hash comparison, and generated package verification were not run here. Do not claim the publish gate passed based on static configuration or fixture tests.
- GitHub Actions remain disabled. No deployment was made.

## Outstanding acceptance gates

1. In the Work Mode checkout, use .NET SDK 8.0.426 and run a fresh Release build and browser publish. Confirm published `wwwroot/browser-input.mjs` exists and matches `game/browser-input.mjs` by SHA-256.
2. Assemble the actual site package and verify its full integrity manifest plus original Mission 01 CRG provenance.
3. Run browser smoke for scene loading, Marksman rendering, and input. The Marksman round prefab still references missing Mesh GUID `b4b69dc6c74eeb54cbd22d28e37f9a19`, fileID `4300000`; do not substitute the casing mesh without an equivalence proof.
4. Run the original Operator/Occupant two-actor lifecycle with real Mission/global dependencies. No pass is claimed.

Use `AGENT_INSTRUCTIONS.md`, `AGENTS.md`, and `docs/REPOSITORY_REMEDIATION_2026-10-18.md` for the standing workflow and ownership boundaries.