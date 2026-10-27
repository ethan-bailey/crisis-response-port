# Branch and integration policy

[AGENTS.md](../AGENTS.md) is the canonical agent workflow. [`docs/GATE_DEPENDENCY_INDEX.md`](GATE_DEPENDENCY_INDEX.md) is the only gate list. [`NEXT_AGENT.md`](../NEXT_AGENT.md) is a compact current-head handoff, not a history log.

| Ref | Role |
| --- | --- |
| `develop/wasm` | Single integrated C#/.NET WebAssembly development line |
| `main` | Frozen, divergent baseline; do not write until the owner decides its status |
| Other topic, release, preview, and maintenance refs | Historical or parked work; inspect ancestry and unique content before any disposition |

## Integration and push

- Commit one logical change at a time and group all files for that change. Push directly to `develop/wasm` with an expected-HEAD fast-forward compare-and-swap.
- Prefer the connected GitHub API when shell Git credentials are unavailable; verify remote HEAD and tree after every successful update. Never force-push, rewrite shared history, or assume a connector commit SHA matches a local replay SHA; compare exact tree and parent.
- Use a short-lived branch/PR only when branch protection, review, or isolation materially requires it.
- Before deleting a branch, classify its current tip against live `develop/wasm`: ancestor; merged-PR content landed by squash/rebase; unmerged tip with unique work; or exact duplicate content whose PR was not merged. Delete only the explicitly authorized safe categories after listing their refs and tips. Keep unique work until the owner decides. If no authorized branch-delete API or authenticated Git route is available, preserve refs and record that precise capability blocker.
- No routine commit publishes the owner-private Site. Publication requires a separate explicit release request after exact-source build and package-integrity checks; preserve the existing access policy.

## Live branch disposition audit — 2026-10-27

The connected GitHub API listed 79 live branch refs and 57 pull requests (0 open, 44 merged, 13 closed unmerged). The 56 distinct PR head refs were compared with `develop/wasm` at `fdea8000c116b03befa427d63537c58a0493ff4c`:

- 37 current PR-head refs are ancestors of the live integration branch; their content is already in history.
- 7 merged-PR refs have tips outside history because the landing was rewritten: `fix/touch-hold-fire-20261023`, `codex/game-scene-name-normalization-20261023` (PRs #70/#72), `chore/live-state-guard-20261018`, `feature/extract-original-occupant-spawn-contracts`, `feature/extract-original-occupant-spawner-sources`, `feature/extract-original-combat-dependency-sources`, `maintenance/foundation-first-master-plan-20261017`.
- 11 closed-unmerged refs contain unique work and remain protected pending owner decision: `codex/ik-sequence-binding-20261026` (#74), `feature/source-ikarmcontroller-live-callbacks` (#57), `feature/original-projectile-limb-hit-runtime` (#56), `fix/original-impact-mesh-initial-load` (#36), `feature/iphone-original-movement-joystick` (#35), `feature/26-source-arm-rig-import-audit` (#34), `fix/33-configured-grips-chamber` (#33), `codex/wasm-original-arm-sequences` (#21), `codex/full-source-closure-audit` (#20), `codex/wasm-compile-package` (#19), and `codex/iphone-input-parity-foundation` (#7).
- Two closed PR refs are special cases: `codex/projectile-material-impacts` (#9) is already an ancestor despite its PR being unmerged; `codex/weapon-actions-checkpoint` (#8) is not an ancestor, but its only changed file is `vercel.json`, whose blob SHA is identical to `develop/wasm` (`87056607badcfceb2dddb95864942180b10c438f`). Its tip commit is outside history but no unique file content remains.

This live enumeration differs from the older 39/7/11 report: the current API confirms 37 ancestor refs, 7 merged rewritten refs, 11 unique unmerged-work refs, and the two explicit special cases above. Twenty-three other live refs are not current PR head refs; they were not classified as disposable. PR #74's tip `79845d580fe8a186cc39411c5e0cef6801a40221` adds owner-checked binding of seven recovered IK sequences to existing `FullSource IKArmController` fields in `runtime/Engine/OriginalArmSequences.cs`, plus a contract in `ArmSequenceContracts.cs`. Live `develop/wasm` currently has only the sequence load/compose path and no `BindAuthoredSequences` call. This binding is likely a needed Gate 5 integration dependency, but PR #74 explicitly leaves browser playback false and lacks the Glock graph/real weapon-switch path; do not merge blindly. Preserve the branch for reviewed integration in the Gate 5 batch.

The connected GitHub API offers ref updates but no branch-delete operation; shell `git push` authentication returned `fatal: could not read Username for 'https://github.com': No such device or address`. Therefore no refs were deleted in this audit. `main` remains untouched; the owner may either retire `main` or make `develop/wasm` the default branch.
