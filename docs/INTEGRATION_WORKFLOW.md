# Integration workflow

The project has one active integration line: develop/wasm. Follow [AGENTS.md](../AGENTS.md) for the canonical workflow and [NEXT_AGENT.md](../NEXT_AGENT.md) for live task state.

Owner-authorized source work is committed directly to the inspected develop/wasm head with a lease. Use one short-lived PR only when review, isolation, or branch protection materially requires it. Preserve existing refs. Run the relevant contracts and the read-only GitHub Actions workflow. Do not deploy intermediate builds; release publication requires explicit owner authorization and exact-SHA evidence.

The former multi-branch and stacked-PR instructions on this page are retired.
