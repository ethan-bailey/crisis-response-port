# Full Mission package CI investigation — 2026-10-26

## First exact-source run

- Commit: `759922efdc906d2f7eba7086c0ad1025b599428f`
- GitHub Actions run #35: `40276509885`
- Run URL: https://github.com/ethan-bailey/crisis-response-port/actions/runs/40276509885
- Chromium IndexedDB job passed. In the source/package job, Node and Python contracts, RuntimeChecks, EngineChecks, FoundationChecks, Release build, and WASM publish passed.
- Full package stopped before building because the packaging input scan raised `ValueError: Symlink is not a package input: game/node_modules/.bin/playwright`. The workflow first runs `npm ci --prefix game`; package copying already excludes `node_modules`, but a prior `files(repo / "game")` validation rejected its symlink before copy ignore was applied.

## Repair

- Exclude only the top-level `game/node_modules` dependency tree from the package-input symlink scan; the package copy also excludes this directory.
- Preserve symlink rejection for every remaining game source and all package/publish inputs.
- Add regression cases that the excluded Playwright-like symlink does not block packaging and that a symlink elsewhere in `game/` still fails.
- Rerun on the exact pushed SHA. Record package verifier output, manifest source commit, file count, deterministic archive SHA-256, artifact ID, and CI result below. Do not treat run #35 as package-gate pass and do not publish from it.

## Retry result

Pending exact-SHA rerun.
