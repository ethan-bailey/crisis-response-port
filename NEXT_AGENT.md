# Current handoff — 2026-10-26

## Latest verified release

- Live owner-private Site: `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.
- Site version **68**: `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_52c7de248e00819185bc248800ca7183`.
- Successful deployment: `appgdep_6adeb2efdfe481918fc2eb654eae1c37`, completed `2026-10-26T01:55:06Z`; audience remains owner-only/custom.
- Exact Site source code commit used to build and publish: `a1bdc32cc180e2cccde9bb8da22075da6a22010e` (`Cache verified mission assets and fix absolute stick aim`). It was pushed to the Site source `main` before packaging. A documentation-only handoff commit is being added after publication; it does not alter the published code.
- The same four code/test files were pushed to GitHub `develop/wasm` in commits `d15f0ee7f445172da74c6313f5c0a6135a09865b`, `4b52154921d5982fe22baa0ce780a28f6916cffe`, `f3d6b7786997aa92722a912a35ab3b1b237f019d`, and `ccb846e91f6fe01172afb90e316051687c48b760`. `ccb846e...` is the code checkpoint; GitHub handoff sync follows separately.

## What version 68 changes

1. The movement stick now sets vertical aim directly from its vertical axis (`-1..1`), independent of the previous pointer aim and independent of the horizontal movement axis. Neutral release preserves the last vertical aim.
2. The Mission loader now stores verified compressed source assets in CacheStorage under a key derived from the immutable source profile hash. Every cache hit still checks the manifest's compressed length and SHA-256 before delivery. It requests persistent browser storage when available, without blocking Mission startup; denied storage, quota errors, unsupported browsers, or evicted data fall back to verified network download. This is device-local reuse, not a guarantee against Safari clearing or evicting site data.
3. Added regression coverage that verifies absolute vertical aim and a second Mission load using the cached asset without refetching it.

## Exact build and test evidence

- .NET SDK **8.0.426** Release WASM publish from Site source `a1bdc32...` succeeded. Optional `wasm-tools` optimization workload is unavailable; publish completed without it.
- Node contracts/unit tests: **124 passed, 0 failed** (`node --test scripts/*_test.mjs scripts/*_contracts.mjs`).
- Python tests: **41 passed** (`python3 -m unittest discover -s scripts -p '*test.py'`).
- EngineChecks: **75 passed**. FoundationChecks: **121 passed**.
- Independent package verifier: `status: verified`, **2,684 files**, **199 Blazor resources**, **195 WASM files**, and complete bounded Mission profile (**1,919 assets**, 97,767,549 compressed bytes, 205,514,345 source bytes). `build-manifest.json` SHA-256: `659c52333d40d5c442b176285529abff18588e7e46e37df6e99d8d88e839113a`.
- Profile source package SHA-256 remains `896bc8b990ec7554bdfd2c9c73f7ce3e7254fed1036f51c709c9ecb970d1f62d`; profile was copied from the previously independently verified Site v67 package and revalidated by the new package verifier.
- Exact deployment archive `/tmp/cpr-site-runtime-a1bdc32.tar.gz`: **138,782,818 bytes**, SHA-256 `8c42a77abf507ab36bc3212ba71b67c67b711f54484d0e8dd34609cb5660857d`. A separate tar-stream audit compared byte count and SHA-256 for all **2,686** archive files and confirmed `.openai/hosting.json`.
- The initial package attempt passed the parent publish directory instead of its `wwwroot`; it failed before saving/deployment. Repackaging with `/tmp/cpr-site-publish-a1bdc32/wwwroot` passed and is the only archive deployed.

## Remaining gates in dependency order

1. **Physical iPhone Safari acceptance for exact Site version 68.** Confirm Mission loads beyond asset 1,900; record first-run vs second-run transfer/time and browser storage estimate; verify direct absolute vertical aim, horizontal movement independence, walk/sprint detent, no tap-zoom, and gun/reload timing. Current CacheStorage persistence is best effort and still needs this Safari measurement. Desktop automated browser/WebGL remains unavailable in this environment; do not report it as device acceptance.
2. **Weapon reload audio contract.** The source clip inventory includes magazine insert and bolt-return sounds, but their exact M4 reload event callbacks and timing have not yet been verified end-to-end. Inventory the source weapon/prefab/event dependencies together, then wire callbacks and test fast reload, cancelled reload, empty reload, and repeated-fire synchronization. Keep the gunshot fixed-gain/stale-event protections already covered by audio tests.
3. **Complete the inventoried 51-system particle lifecycle.** Continue from `docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md` and `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`: exact owners/callbacks for Emitter, ProjectileTrace, weapon smoke, blood/wounds, bounce, pause/time-scale, and destruction ordering; active Collision, Clamp Velocity, Inherit Velocity, Force, Rotation, Size, Color, Burst behavior; and Unity random-state reproduction. Inventory dependencies per batch before implementation. Keep `sourceVisualParity:false` until native comparison.
4. **Known exact texture blocker.** Original `Spark.png` (GUID `43907091bffd53b478c985dca2762559`, SHA-256 `6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344`, 1,539 bytes) is absent from recovered and checked-in sources. Do not substitute artwork; continue unrelated particle modules and only defer this texture-dependent visual path.
5. **Broader gameplay parity.** Inventory complete dependency sets, then continue projectile/damage outcomes, ragdoll/joints, authored Mission/NPC flow, remaining animation/IK, and interaction/menu systems. Runtime/build success alone is not parity evidence.
6. **Each release.** Push exact source to both source repositories, build with SDK 8.0.426, regenerate or reuse only the exact verified bounded profile, verify package and archive bytes, and publish only that archive. Update this handoff with immutable identifiers and the next concrete action.

## Next action

Run and record owner iPhone Safari acceptance on Site version 68 when the device is available; meanwhile continue the already inventoried reload-audio callback/dependency closure and 51-system particle batches. Do not block independent code work on the physical-device gate or missing Spark texture.
