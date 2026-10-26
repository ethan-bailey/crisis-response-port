# Branch and integration policy

This page summarizes the repository's current branch policy. [AGENTS.md](../AGENTS.md) is the canonical agent workflow; [NEXT_AGENT.md](../NEXT_AGENT.md) is the live state and must be checked against GitHub.

| Ref | Role |
| --- | --- |
| develop/wasm | Single active integrated C#/.NET WebAssembly development line |
| main | Stable baseline; do not use for routine gameplay integration |
| Existing topic, release, preview, and maintenance refs | Historical evidence or parked work; do not assume they are current |

## Integration

- For owner-authorized work, commit and push directly to the current develop/wasm head using a compare-and-swap lease.
- Use a short-lived branch and PR only when branch protection, review, or isolation materially helps. Keep no more than one active PR for this project.
- Never force-push, rebase shared history, or delete refs during routine work.
- The 78 refs present at the 2026-10-26 workflow reset are preserved. Before pruning any ref, compare its unique commits and unmerged work with develop/wasm and ask the owner to authorize the cleanup.
- No routine commit publishes the owner-private Site. Release publication requires an explicit owner instruction and exact-SHA verification.

GitHub Actions is a read-only build/check gate. See .github/workflows/source-contracts.yml and docs/PORTING_WORKFLOW_RESEARCH.md.
