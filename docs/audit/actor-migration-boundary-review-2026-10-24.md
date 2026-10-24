# Actor migration boundary review — 2026-10-24

The frozen `runtime/Engine/WeaponHost.cs` baseline from `407c66d026784b72c96490eabae6085dd87c22e6` no longer matched the checked-in source. The reviewed bridge delta adds source-arm aim-convergence state to the result snapshot and, when `requireAimConvergence` is set, delays the call to the original `CharacterBody.PullTrigger` until both original arm solvers report `OnTarget` and the muzzle alignment is within two degrees.

The source `CharacterBody` and `WeaponBase` still own firing, ammunition, and weapon state. This exception records the current adapter boundary; it does not accept the feature as iPhone gameplay behavior or close Gate 2/8. The separate owner-reported play-area hold behavior remains deferred.

The accepted `WeaponHost.cs` SHA-256 is `7e495504f486790a5736371784318cb2734d6306d04df3075f5bef101834da11`; the prior frozen hash is retained in `actor-migration-boundary.json`. Existing source-combat evidence is in [`SOURCE_COMBAT_INTEGRATION_MILESTONE_2026-10-24.md`](../evidence/SOURCE_COMBAT_INTEGRATION_MILESTONE_2026-10-24.md), and the browser aim-fire contract is covered by `scripts/aim_fire_gate_test.mjs`.

This review updates the freeze record only. The current runner has no pinned .NET SDK, so this pass did not compile `WeaponHost.cs` or establish exact-SHA WASM acceptance.
