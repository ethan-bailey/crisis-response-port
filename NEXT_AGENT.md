# Crisis Response Port — current handoff

- Work branch: `develop/wasm`; latest camera-sync commit: `613db77cfe3f2ccba24dd0debdccb14c22b649b4`. Canonical gate status and evidence are in [docs/GATE_DEPENDENCY_INDEX.md](docs/GATE_DEPENDENCY_INDEX.md).
- CI #185 passed Node contracts, source/runtime checks, Release WASM publish, package integrity and IndexedDB acceptance. Packaged step 29 confirmed source/render camera agreement and authored movement, but the nearest Occupant limb projected at x≈1369 outside the 1280px viewport; no firing attempt began.
- This follow-up sets the recovered `InteriorCamera.centerOffset` to zero so the original follow camera centers its subject, while Three.js mirrors the same camera and `WeaponPointer` continues raycasting through it. The packaged test checks the center offset and then requires visible target hit/injury evidence.
- Gate 3 remains closed on packaged reload evidence: magazine/reserve 30/30 → 28/30 → 30/28, chamber live. The 49-warning full-source build and four excess warnings/first revisions remain recorded in the gate index and [docs/evidence/BUILD_WARNINGS_2026-10-29.md](docs/evidence/BUILD_WARNINGS_2026-10-29.md).
- Three.js 0.180.0 vendor tree remains hash-verified against its npm archive. No Site publication or new iPhone Safari acceptance occurred.
- This session began without a Git checkout; repository-root bootstrap could not run in the bare workspace. CI hosts build/package the committed source. Shell push authentication is unavailable; use the GitHub API with expected-head protection and verify committed blob SHAs.
- Next: inspect the next hosted step-29 result. Continue camera/aim/combat repair until the packaged view frames original actors, normal movement remains authored, and original projectile injury passes.
