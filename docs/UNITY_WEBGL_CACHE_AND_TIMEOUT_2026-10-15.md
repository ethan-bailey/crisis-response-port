# Reusable Unity WebGL compilation cache — October 15, 2026

## Why this exists

Previous Marksman Unity 2022.3.62f1 WebGL attempt [38788171468](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38788171468) reached Unity's IL2CPP/WebAssembly build but hit the 25-minute editor timeout (`build_exit=124`). Zero C# compiler errors were observed, but there was **no completed WebGL player**. That ephemeral GitHub Actions machine was discarded, including its uncached partial build products.

## Fix applied (CI infrastructure only)

- `.github/workflows/unity-marksman-webgl.yml`: **90-minute job timeout**; manual-only after a one-off push-triggered diagnostic run. `actions/cache/restore@v4` runs before recovering the private project, and `actions/cache/save@v4` runs with `always()` after Unity has exited and license cleanup was attempted. Paths:
  - `work/unity-spike/recovery/export/ExportedProject/Library` — imported project, shaders, `Library/Bee` incremental compiled WebGL outputs and asset database
  - `work/unity-spike/bee-cache` — machine-level Bee shared build cache
- `scripts/unity_open_spike.sh`: **3,000-second (50-minute)** Unity build `timeout` for both single-pass and fallback WebGL paths. The same activated container runs one Unity import+build command. Docker now passes `BEE_CACHE_DIRECTORY=/spike/bee-cache`, which points to the host-mounted CI work directory.
- Exact cache keys include Unity patch version, recovery version and guard-script hashes, with prefix restores for compatible prior caches. **Cache entries are immutable**; a changed guard-script fingerprint creates a new cache key. Unity is still responsible for validating incremental dependencies.
- Cache upload is conditional on a combined **1–8,192 MiB** uncompressed `Library` + Bee footprint. This guard protects the GitHub repository's default 10-GiB cache budget, but means especially large builds **may not be cached**; the Actions log prints each directory size, whether saving was attempted, and whether it succeeded.
- `.github/workflows/unity-marksman-audio-http.yml` uses the same cache paths/key and a 90-minute job budget for later optional audio correction and local HTTP smoke tests.
- Unity Personal seat remains activated only in the transient test container and is deactivated via `--deactivate-all` and verified with a seat query after the build. The emergency `always()` cleanup remains.

## Validation

One controlled run was started after the change:

[Run 38793686345](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38793686345).

**At dispatch, it is cache-cold** (no known previously saved Library/Bee cache). A test result is only established when its own logs prove: (1) Unity player `build_exit=0`, (2) WebGL output manifest contains loader/data/wasm/HTML, (3) localhost smoke test passes, (4) Unity Personal entitlement was deactivated, and (5) cache save completed or the cache-size guard explicitly skipped the upload.

A GitHub job conclusion `success` **does not** prove Unity player success: the script retains diagnostic logs even for nonzero build exits.

## Limitations

Caching **cannot guarantee** reusing a partially failed IL2CPP task graph. The build may still fail from WebGL or asset postprocessing errors, memory or disk limits, invalidated intermediate artifacts, or a second timeout. Reusing the same project and cache improves the odds of an incremental rebuild; the first cache-cold run may still be expensive. GitHub's hosted Ubuntu runners are ephemeral and cannot retain a live Unity editor or Docker image across runs.

The original recovered archive remains read-only in its private GitHub release. **No gameplay source, scene, prefab, or original binaries were modified or published by this change.**

## Completed cached run — Unity WebGL player built successfully

[Run 38793686345](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38793686345) completed October 15, 2026 (logs artifact `unity-marksman-webgl-feasibility-logs`, ID `12003480877`). This is the **first verified successfully completed Unity 2022.3.62f1 Marksman WebGL player build** on this branch:

- `status.env`: `build_attempted=true`, **`build_exit=0`**, `single_pass=true`; no `error CS####` in the log. Unity exited batchmode successfully after WebGL postprocessing and Brotli compression, reaching `[569/570]` in the emitted Bee progress (the total is not an exact wall-time progress measure).
- The diagnostic workflow job succeeded, as did `actions/cache/save@v4`: **1,461 MiB Unity Library + 14 MiB Bee cache = 1,475 MiB cached**, key `unity-marksman-library-bee-linux-2022.3.62f1-recoveryv3-v2-2498b1cafb6a46940e98e7507f593771dfed8d19bd5b717b90854af38a92698c`.
- Unity Personal `--deactivate-all` exited zero and private license log confirmed **`[200] REVOKE_SEAT`**, followed by successful seat query with no active seats.
- The host-side **output inventory and localhost HTTP smoke did not complete**, because Docker's root-owned `/spike/webgl/Build/webgl.data.br` caused `PermissionError: [Errno 13]` for the host user. Without the JSON inventory, HTTP smoke failed opening `webgl-output-manifest.json`. The underlying Unity player build was not at fault. The generated WebGL binaries themselves were *not* uploaded to GitHub artifacts and are not retrievable from the expired ephemeral runner.
- Fix for **next explicitly authorized/manual** validation: `scripts/unity_open_spike.sh` now applies `docker exec ... chmod -R a+rX /spike/webgl` **after** Unity exits, and **before** manifest/HTTP checking. This changes output file permissions only, not binary contents, gameplay, or original assets. **Not yet tested** by a new Actions run.

The next useful actions are (1) restore the saved incremental cache, (2) produce and privately retain a successful WebGL output on one deliberately scheduled run, and (3) verify its loader/wasm/data files and actual browser execution before a playable claim. Do not start recurring attempts automatically or make original proprietary assets public.

## Next single-run private playtest package — prepared, not dispatched

- `.github/workflows/unity-marksman-audio-http.yml` remains **manual-only** with the same cached `Library` and Bee inputs; it additionally corrects the 250 recovered WAV metadata headers **only in the ephemeral project** and builds the Marksman-only WebGL player.
- The workflow now **fails closed** before producing a private WebGL archive: the generated file manifest must confirm `index.html`, loader, WASM, and data files; its localhost HTTP smoke check must pass; `build_exit` must be zero; and Unity Personal deactivation and seat query must both exit zero.
- On all gates passing, it uploads a **two-day-retention artifact** named `PRIVATE-marksman-webgl-player` to the existing private GitHub Actions run for owner review. This does **not** publish an internet-accessible game or establish iPhone Safari runtime correctness. Private evidence-only logs remain independent.
- The `chmod -R a+rX /spike/webgl` host-output read-permission fix in `scripts/unity_open_spike.sh` is already committed. This is the only build artifact access change and does not rewrite recovered assets.
- **No new licensed Unity compilation was launched** for this packaging-only workflow edit, at the user's request to minimize unnecessary Actions runs. The next WebGL attempt should be one deliberate, cached, consolidated package+audio+smoke run rather than several exploratory jobs.
