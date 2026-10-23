# Crisis Response Port — current handoff

## Checkpoint

- Development line: `develop/wasm`; the code source checkpoint remains `5bb158f0bb18ab3fbf17fcd478f2266226cd057e`. Documentation-only commit `c297cc1c1e6c3179902d19b3e74ab4646fe7f6dd` records the recurring workspace issue. Live GitHub confirms that head and no open PRs. GitHub Actions remain disabled.
- Codespaces use is at 90% through 2026-11-01; avoid it unless the owner requests it or a required gate cannot run elsewhere.
- Full original CharacterBody compilation passed at exact code source SHA `1237e360b829a5a80289d0136d2f8be7a7159050` (0 errors, 45 warnings). The later changes are documentation only.

## Current blocker

- This Work Mode session has no Crisis Response checkout mounted. The prior `/workspace/scratch/c5901021c6f6/crisis-response-port` path is absent; the only Git checkout found is an unrelated Sites repository.
- Shell `git ls-remote` failed with `could not read Username ... No such device or address`; this runner cannot prompt for shell GitHub credentials. GitHub's connected API can verify the head and read/write individual text files, but its fetch tool rejected the repository tarball endpoint and cannot provide a buildable checkout.
- The pinned .NET SDK 8.0.426 and `/tmp/libprocshim.so` were not found in the current runner. Gate 1 has not started; no gameplay source changes or gameplay gates are claimed.
- Recovery steps and exact observed failures are documented in `docs/DOTNET_BUILD_RUNBOOK.md` under “Repeated workspace/checkout unavailability” and linked from `AGENT_INSTRUCTIONS.md`.
- The existing “Crisis Response Gates 1–7” hourly automation is paused to avoid repeating this same environment failure. It may be resumed once a usable project checkout and toolchain are available.

## Gameplay gates requested

1. Load, initialize, and destroy the original Operator prefab; preserve authored components and 19 limb identities.
2. Load Occupant and prove body, limbs, health, and equipment are independent of Operator.
3. Connect original movement/input, animation, and arm IK.
4. Equip original weapon; verify aim, trigger, and reload.
5. Verify projectile-to-limb collision, damage, and original physiology.
6. Verify unconsciousness/death, ragdoll, weapon drop, and destruction.
7. Run these systems with real actors in a normally initialized mission.

## Next action

Restore the `develop/wasm` checkout and pinned .NET 8.0.426 runner in Work Mode. Verify branch/head/worktree, then start Gate 1 by tracing the authentic Operator prefab’s source-backed load/initialization/destruction path and its complete serialized-reference closure. Avoid Codespaces through 2026-11-01 where practical.