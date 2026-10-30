# Crisis Response Port — current handoff

## Current source and verified result

- Work branch: `develop/wasm`. Live source HEAD is `4691e28edc337ac2647db3f9e44beb6a41d15de6`. The numbered gate list is maintained in [docs/GATE_DEPENDENCY_INDEX.md](docs/GATE_DEPENDENCY_INDEX.md).
- Latest full CI is run #175 (`40917254826`) on tested code SHA `d8bdcbee9bc6d92aefe08d518f7186f67c4366fa`; all jobs passed. Node contracts passed 202/202. Release/WASM and package checks passed, including step 29 in packaged Chromium. Gate 3 counts were magazine/reserve `30/30` before firing, `28/30` after spending two rounds, and `30/28` after reload; chamber live. The final handoff-only commit `4691e28` has no workflow run of its own.
- Fresh SDK 8.0.426 Release source build at `5d1acbb` passed with 0 errors and 49 warnings against the reproduced 45-warning baseline at `c78e8ad`. The complete list and first-seen revisions for the four additional diagnostics are recorded in [docs/evidence/BUILD_WARNINGS_2026-10-29.md](docs/evidence/BUILD_WARNINGS_2026-10-29.md).
- Gates 0–3 are closed for indexed evidence. Gates 4–8 and combined parity remain open. Gate 4's headless source projectile/injury contracts pass; its packaged-browser projectile-hit-to-injury/outcome sequence is the next dependency-ready work. See the index for exact boundaries.
- No Site publication or iPhone Safari acceptance has occurred. Three.js 0.180.0 is vendored and its full tree was verified against the npm archive in the prior round.

## Environment and next work

- This session restored the repository at `/workspace/scratch/crisis-response-port-recovery` on the exact live branch/head. `scripts/env/bootstrap-environment.sh` passed with .NET SDK 8.0.426 at `/root/.dotnet/dotnet` and Chromium 156.0.8078.4 at `/root/.cache/ms-playwright/chromium-1248/chrome-linux64/chrome`.
- Combined preflight now passes the SDK and Chromium checks. Shell push authentication remains unavailable; use the connected GitHub API with an expected-HEAD update and verify committed blob hashes for every API-landed file.
- The checkout contains an untracked 1 MiB `scene-data/.marksman-range.json.mkbTCE`; it was preserved. The local checkout has no `scene-data/fullsource` package; the workflow builds the exact source package from the verified recovery artifact.
- Continue Gate 4 using the original FullSource trigger, projectile, limb and physiology callbacks; record exact package/browser evidence and keep native PhysX, rendered wounds, iPhone audio/pose, and visual parity distinct. Then continue Gates 5–8 and combined acceptance.
