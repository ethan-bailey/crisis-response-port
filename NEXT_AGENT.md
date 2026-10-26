# Current handoff — 2026-10-26

## Public repository and verification

- Integrated branch: develop/wasm. The current code checkpoint at f2241ba68c6ba370ceb6ef3283736e6707e72c49 passed the full source-and-WASM workflow. The live branch head before this documentation refresh was ff733303e6a5aec3f8e3c209965434d9f36c6fb8; this refresh changes documentation only. Query live GitHub before the next edit.
- GitHub Actions [run #40256847368](https://github.com/ethan-bailey/crisis-response-port/actions/runs/40256847368) passed on exact code SHA f2241ba68c6ba370ceb6ef3283736e6707e72c49. Node: 125 passed, 0 failed. Python: 41 passed. Runtime, EngineChecks, and FoundationChecks passed. Release runtime build: 48 warnings, 0 errors. Browser WASM publish and required output checks passed.
- The initial run [#40256338353](https://github.com/ethan-bailey/crisis-response-port/actions/runs/40256338353) found Python contracts lacked PyYAML. requirements-test.txt now pins PyYAML 6.0.3; rerun #40256436576 passed, and the exact latest code SHA also passed.
- CI checks the public GitHub source only. It does not deploy or prove rendered gameplay or device parity.
- Last GitHub check: no open PRs into develop/wasm, and 78 refs. Preserve legacy refs until separately audited and owner-authorized for cleanup.

## Current public-branch behavior

The exact code SHA above includes recent owner changes for:

- Absolute bounded joystick vertical aim, independent of prior pointer position.
- Repeat-launch source-asset caching: immutable compressed assets are stored in CacheStorage under a source-profile hash. Cache hits are rechecked against manifest size and SHA. Persistent storage is requested best-effort; unavailable or evicted storage falls back to verified network bytes.
- Original weapon-audio preload before weapon readiness, plus runtime checks that reload emits the authored magazine-extract, magazine-insert, and bolt-return audio events.

These changes pass the repository contracts and compile/publish workflow. CacheStorage remains optional on Safari. No phone measurement proves that it fixes first-run loading after about asset 1,900. Physical iPhone Safari acceptance of the latest Site build is not recorded.

## Current Site release and provenance

The connected Sites project currently reports version 69 deployed successfully:

- Version ID: appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_c2bffdc3c2388191a6ebd776719f85a3
- Site source commit: 0542735282607dee06504a8887cc8a8d8486ebe6
- Deployment: appgdep_6adeb47f004481918f38c0782cd9d781, succeeded at 2026-10-26T02:01:44Z
- Archive: SHA-256 8335c571a04fdf563a3daad58761d3a2916b11d1012a6eacb672ba958354c58d; 2,686 files; 177,981,440 bytes
- Access remains custom with one allowed user.

No Site write or deployment was made during repository workflow work. The Site source SHA is not a commit in the public GitHub repository. That alone does not prove file differences because the Site has a separate source history, but no file-level crosswalk for version 69 was available in this session. Also, the prior version 67 handoff describes a flattened binary asset bridge, while the public scene-package loader at the current GitHub checkpoint still uses per-asset Base64 interop. The current Site archive has not been inspected file by file. Do not claim GitHub Actions validates the Site archive or create another Site version until the source relationship is recorded.

Historical version 67 was built from Site source e03087487359e61dce2b0db0345f1dc8818bb876, package SHA-256 896bc8b990ec7554bdfd2c9c73f7ce3e7254fed1036f51c709c9ecb970d1f62d, and manifest SHA-256 3b54dc10517c3a77188246dcc8726473ad54288b922aa4a8b0ef981d22e5cf8f. It is superseded.

## Remaining gates

- Physical iPhone Safari acceptance of exact Site version 69 is not recorded. Check mission completion, load/memory after asset 1,900, joystick/aim/fire/reload/audio, tap-zoom prevention, readable errors, physics/casings, and particle rendering.
- Finish the inventoried 51-system particle lifecycle from authored owners/callbacks: Emitter, ProjectileTrace, weapon smoke, blood/wound effects, bounce, pause/time-scale, destruction order; collision, clamp velocity, inherit velocity, force, rotation, size, color, burst, and Unity random-state behavior.
- Spark.png (GUID 43907091bffd53b478c985dca2762559; expected SHA-256 6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344) remains absent from recovered and checked-in sources. Preserve the gap and do not substitute artwork.
- Broader source-backed work remains for projectile/damage, ragdoll/joints, authored mission/NPC flow, animation/IK, interaction/menu systems, and mobile loading/storage.

## Next action

Create a file-level source-to-Site crosswalk for deployed version 69 against public develop/wasm, recording both source SHAs and exact file hashes. Establish one reproducible source authority before another Site release. If the private archive cannot be read from this environment, record the exact blocker and continue the independent particle lifecycle batch; do not wait on the device or archive to do unrelated source work.
