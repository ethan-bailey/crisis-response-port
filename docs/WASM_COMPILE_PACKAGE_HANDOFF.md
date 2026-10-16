# WASM compile/package continuation — 2026-10-16

## Scope and baseline

This isolated continuation starts at `3dd71366d96b048512fca24ee2d2dbd85d55befd`
on `codex/projectile-material-impacts` (draft PR #9). New branch:
`codex/wasm-compile-package`. No original C#, gameplay, input, renderer, Unity
PR #7, main or hosting changes. The user subsequently confirmed no other agent
was working, but branch isolation is retained for review and selective rollback.

Latest prior full browser proof: run `38846445335`, tested gameplay SHA
`0933f2d4f5c5047f4e4a7f7c701fb13fd0f47e8c`, artifact `12024020540`.
Source native impact contract: run `38847238826`, SHA
`12f292657267d0c0521f00140c4ddec1976b7cd2`.

## Problem and change

The existing movement-smoke artifact keeps `work/game-runtime/wwwroot/` and
browser evidence, while the complete game directory is assembled separately.
That runtime artifact alone cannot be deployed as the full game. This change
compiles the same existing original-source subset and packages both `/game/`
and `/runtime-web/` with an entry page, source inventory and output checksums.
It advances roadmap Gate 8 packaging, not all-game Gate 2 or behavioral parity.

Files:

- `scripts/package_wasm_site.py`: standard-library packager and standalone
  checksum verifier. Requires real WASM headers, hashed Blazor boot resources,
  both original-code assemblies, equal game/runtime input adapters and exact
  runtime mount path. Rejects changed tracked game/runtime inputs, existing
  output directories, overlapping output/input, symlinks, missing/extra or
  damaged package files, duplicate/traversing manifest paths and stale assemblies.
- `scripts/package_wasm_site_test.py`: twelve failure-oriented tests using **test
  fixtures**, not a substitute game/runtime or evidence of actual compilation.
- `.github/workflows/wasm-package-checkpoint.yml`: one intentional private
  compile/package milestone; no browser engine installation or deployment.
  Normal source pushes do not run the workflow. It runs native source contracts,
  existing input/source tests, actual .NET WASM publish, archive/unpack verification
  and saves the complete site plus separate evidence privately.
- `tools/wasm-compile/global.json`: exact stable SDK `8.0.426`, roll-forward
  disabled. Only the new checkpoint workflow copies it to an untracked root
  `global.json` before invoking dotnet. Existing gameplay/Unity workflows are
  unchanged. SDK selection and pin SHA are recorded and cross-checked.

Manifest (`build-manifest.json`) records exact checkout SHA; SHA256/size for
tracked game/runtime/scripts/workflows inputs and every package file; measured
SDK/Node/npm/Python versions; CI run URL; actual binary/boot verification counts;
external import-map URLs and explicit verification limits. Compressed runtime
index variants are removed after changing the `<base>` path, so servers cannot
serve a stale precompressed index pointing at the wrong framework directory.
The tar uses stable file order, timestamps, ownership and gzip metadata. Builds
are traceable; SDK servicing and run metadata mean byte-identical archives
across independent runs are not promised.

## Commands and evidence

Local targeted validation (this workspace has Python/Node/git, **no .NET SDK**):

```sh
python3 -m unittest discover -s scripts -p package_wasm_site_test.py -v
python3 -m py_compile scripts/package_wasm_site.py scripts/package_wasm_site_test.py
```

Result after SDK-pin correction: **12/12 targeted tests passed**. Actual cloud compile
evidence is recorded below. Never call local
fixture tests proof that the actual original C# game compiled.

Reverify a previously downloaded site without compiling or installing packages:

```sh
tar -xzf crisis-response-private-wasm-site.tar.gz -C <new-empty-directory>
python3 scripts/package_wasm_site.py verify <new-empty-directory>
```

Treat the archive as private original-game content; it does not enforce access
control. It must be served at the **origin root**, with `/game/` and
`/runtime-web/` siblings; .wasm MIME must be `application/wasm`. Three.js still
loads from the source-pinned jsDelivr import map, so this is **not** a fully
offline package. No new CDN, shader substitute or rendering algorithm is added.

## Review and next steps

1. Check cloud result, exact SHA, artifact IDs and archive hash below. Confirm
   source/runtime files differ from baseline only where explicitly listed.
2. Review this small compiler/package PR into `codex/projectile-material-impacts`
   only. Do not merge main or touch the independent Unity branch.
3. Reuse the verified package for an owner-restricted deployment once appropriate;
   no deployment occurred in this session. Existing live Site v8 stays older.
4. Resume original gameplay/engine dependencies via `WORKSPACE.md`,
   `docs/COMPLETE_WASM_PORT_ROADMAP.md`, `NEXT_AGENT.md` and issues #10–#18.
   Original particle execution, shaders, complete C# source coverage, scenes,
   AI/missions, native comparison and physical iPhone tests remain unfinished.

## Rollback

This continuation adds only the five files above including this document.
Remove them in a new selective revert after reviewing later dependencies. Do
not reset/force-push shared branches, revert existing original source/assets or
delete prior evidence. No hosting rollback is needed because nothing is deployed.

## First actual cloud compile — successful, SDK selection corrected afterward

- Code/workflow SHA `4811f86e29892b9b9bd010ef9055a524c5485c3c`.
- [Run 38848858817](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38848858817)
  **SUCCESS**: native original C# contracts; **38/38** existing input/source/mesh
  tests; initial **10/10** package tests; actual .NET WASM publish; full package
  boot/hash validation and tar/unpack/revalidation.
- Private complete site artifact `12024397634` (34,720,699-byte GitHub ZIP);
  separate evidence artifact `12024546760`.
- Inner tar.gz SHA256
  `8c9919fa4d8fddf0a76694185977c7e9038e80a0972ad0248e31206fb3ecc50d`.
- **722** package files; **199** boot-resource hashes; **195** WASM binaries;
  **245** source inventory files including **35** selected recovered original
  C# files. This is a subset of the original game, not full-source coverage.
- Downloaded artifact independently extracted and reverified locally with the
  committed checker: identical source SHA/file/resource/binary counts.
- Actual first toolchain: **SDK 10.0.401**, Node v22.23.3, npm 10.9.9,
  Python 3.12.3. Target remains `net8.0`. The runner installed SDK 8.0.426 but,
  without global.json, selected its higher preinstalled SDK. The first archive
  is internally valid, but must not be described as SDK-8-compiled.
- Follow-up adds workflow-local exact SDK selection plus tests rejecting SDK
  drift. One necessary corrected build replaces the first as the recommended
  package; no repeated full browser test or deployment is requested.
