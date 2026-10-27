# Gate 7 — full-profile packaged browser acceptance — 2026-10-27

## Result and provenance

GitHub Actions run **#157** (`40572335603`) completed successfully on `develop/wasm` source SHA `3a33a6df7d74681cf4074976ec9ca9d3845a56ea`; the reverb repair's code tree is `49e905df226b5a794b2c15970bf434514c1e6e02`. The run passed the pinned SDK 8.0.426 Node/Python/runtime/engine/foundation contracts, Release/WASM publish, full-source index and BehaviorDesigner provenance, complete Mission package and integrity check, Chromium IndexedDB persistence, packaged Mission grounding, and independent reload browser acceptance. This is a Gate 7 slice pass, not overall visual/native/mobile parity.

| Hosted artifact | ID | Size | SHA-256 | Retention through |
|---|---:|---:|---|---|
| FullSource Mission grounding browser evidence | `12766925944` | 254,160 bytes | `8ff3d1c1b3309f49a90f6f2e6832b05228e230fa48170329aa7cd12f7e90fcee` | 2026-11-10 |
| Independent reload browser evidence | `12766536372` | 315,837 bytes | `561fdf16016fd6286b6e4c46f422fdd4a9ffa8fe9fcab70962472038d69aca83` | 2026-11-10 |
| Integrity-verified Site package | `12765678309` | 138,632,720 bytes | `23e1876f2ac5082b5dd4137d51e2dd5f799272c10745ec8eaa5fc1b1258af24c` | 2026-11-10 |

The grounding artifact was downloaded and its ZIP SHA-256 matched GitHub's artifact digest. Its files are `mission-grounding.json` and two screenshots. The JSON reports `status=passed`, exact `sourceCommit=3a33a6d…`, runtime path `packaged /game/ → Start original Mission → verified FullSource EntryTeam actor → browser contact solver → projected Three.js hierarchy`, Chromium with SwiftShader WebGL, and zero page errors.

## Observed packaged flow

- Mission transfer exercised the exact seven-root profile: 1,920 assets, 97,767,924 compressed bytes, 205,515,099 decoded source bytes. Six concurrent downloads took **38,018 ms** in this hosted Chromium run. The largest source asset was 14,858,177 bytes (largest compressed asset 3,108,968 bytes). These numbers are one CI-runner observation, not a mobile-device speed guarantee.
- Failure/retry UI was exercised. A first attempt received HTTP 503 for `Assets/Texture2D/sf_conc_floor_01_d.png`; the page re-enabled Start and exposed retry/error/copy controls. The retry completed and displayed `EntryTeam Mission ready · 6 original actors · 94.1 MiB verified`. A separate injected failure at profile asset `6d5b319c6897d3ba599afeb0.json.gz` also failed, copied diagnostics, and succeeded on retry. Thus the old silent-grey loading failure is **not reproduced** on this packaged path; error/retry behavior is covered.
- The contact solver changed the actor root from `[−3,1.4411399,−1.5083258]` to `[−3,1.358736,−1.5083258]`; source, contact and projected roots agree to the reported precision after synchronization. This verifies the tested numeric grounding path, not pixel-perfect contact or complete physical parity.
- Held input moved the actor from `x=−0.6600008` to `−0.8176373`, source reported `Body walk backward` at capped `sourceDeltaTime=0.05`, and release returned velocity to approximately zero and animation to `Body stand`.
- Reload evidence includes original magazine-out/in audio events, no recorded dropped/unloaded/stale/decode/unlock failures, an active-reload arm-pose maximum delta `0.2272`, and settled pose maximum delta `0.0000439`. The before/after serialized weapon counters in this browser observation remain `30` magazine / live chamber / `30` reserve; this artifact does not prove a completed magazine-count transition.
- Rapid taps under Chromium mobile touch emulation left page zoom at `1`. This is not iPhone Safari acceptance. `visualAcceptance` is explicitly `untested`; the retained screenshot and numeric hierarchy checks do not establish pixel parity or correct visible foot contact. Native audio, listener/reverb output, spatial sound, source particle rendering and iPhone performance remain open.
