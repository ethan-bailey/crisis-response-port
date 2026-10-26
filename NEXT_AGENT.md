# Current handoff — 2026-10-26

## Exact repository and CI checkpoint

- Integrated branch: develop/wasm. Public GitHub source and WASM checks passed on exact commit 5c049a9a572af952953149cc2749698d6db389c6. This final handoff update changes documentation only; query the live branch head before the next source edit.
- CI run [#40256436576](https://github.com/ethan-bailey/crisis-response-port/actions/runs/40256436576) completed successfully for that exact SHA. Node: 123 passed, 0 failed. Python: 41 passed. Runtime, EngineChecks, and FoundationChecks passed; Release runtime build succeeded with 48 warnings and 0 errors; browser WASM publish and output checks passed.
- The first workflow run, [#40256338353](https://github.com/ethan-bailey/crisis-response-port/actions/runs/40256338353), exposed the missing PyYAML test dependency. The repo now pins PyYAML 6.0.3 in requirements-test.txt; the rerun above passed.
- This Actions gate tests the public GitHub tree only. It does not deploy, test rendered gameplay, or prove iPhone Safari acceptance.
- At the last GitHub check, there were no open PRs into develop/wasm and 78 refs. Preserve all refs until a separate ancestry/ownership audit and owner-authorized cleanup.

## Recent public-branch source update

The current tested tree includes the recent owner changes for absolute joystick vertical aim and repeat-launch source-asset caching. The aim path maps joystick vertical input to an absolute bounded target. The source loader caches immutable compressed assets in CacheStorage under a profile-hash name, verifies every hit against the manifest, requests persistent storage on a best-effort basis, and falls back to verified network bytes when storage is unavailable. These changes have Node regression coverage and are included in CI SHA 5c049a9a.

CacheStorage is optional and may be declined or evicted by Safari. It can improve repeat launches but does not prove the first-run slowdown after about 1,900 assets is fixed. Record phone measurements before making a performance claim.

## Site release provenance and source drift

The connected Sites project reports current version 68 as deployed successfully at the owner-restricted Crisis Response Runtime URL. Version 68 records Site source commit a1bdc32cc180e2cccde9bb8da22075da6a22010e, archive SHA-256 f5bf3cdd67e747a7bd19495c22824b14fd88ddbc2d31b903acf43671af0e8398, 2,686 files, and 177,981,440 bytes. Its access mode is custom with one allowed user. No Site edits or deployments were made during this repository workflow work.

The Site source SHA is not a commit in the public GitHub repository (GitHub returned no commit for it). The previous version 67 handoff described a flattened binary asset bridge, but the current public scene-package loader sends one Base64 string per asset through AddIncrementalSourceGzipAsset. The current version 68 archive has not been inspected file by file in this session, so its exact relationship to public develop/wasm remains unmatched. Do not claim the public Actions run validates that Site package, and do not create another Site version until the source crosswalk is recorded.

Version 67 is a historical checkpoint: Site source e03087487359e61dce2b0db0345f1dc8818bb876, package SHA-256 896bc8b990ec7554bdfd2c9c73f7ce3e7254fed1036f51c709c9ecb970d1f62d, and manifest SHA-256 3b54dc10517c3a77188246dcc8726473ad54288b922aa4a8b0ef981d22e5cf8f. Do not use it as the current deployed version.

## Remaining gates

- Physical iPhone Safari acceptance for exact Site version 68 is not recorded. Cover mission completion, load/memory past asset 1,900, joystick/aim/fire/reload/audio, tap-zoom prevention, readable errors, physics/casings, and particle pixels.
- Complete the inventoried 51-system particle lifecycle from source owners and callbacks: Emitter, ProjectileTrace, weapon smoke, blood/wound effects, bounce, pause/time-scale, destruction ordering; collision, clamp velocity, inherit velocity, force, rotation, size, color, burst, and Unity random-state behavior.
- Spark.png (GUID 43907091bffd53b478c985dca2762559; expected SHA-256 6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344) remains absent from recovered and checked-in sources. Do not substitute artwork.
- Broader source-backed parity remains for projectile/damage, ragdoll/joints, authored mission/NPC flow, animation/IK, interaction/menu systems, and mobile loading/storage.

## Next action

Create a file-level source-to-Site crosswalk for deployed version 68 versus public develop/wasm, recording exact file hashes and both source SHAs. Then use the result to restore one reproducible source authority before another Site release. If the private Site archive cannot be read in this environment, record that exact blocker and continue the independent particle lifecycle batch; do not wait on the device or archive to do unrelated source work.
