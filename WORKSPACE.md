# Project workspace

This file is a map, not a second status log. The live handoff is [NEXT_AGENT.md](NEXT_AGENT.md); it records the current verified source and release checkpoints, open acceptance gates, and one next action. Confirm its SHA against live GitHub before resuming.

## Stable project rules

- develop/wasm is the integrated C#/.NET WASM line. main is a stable baseline.
- Preserve original source, serialized values, asset identity, GUIDs, fileIDs, and lifecycle. Compatibility code supplies Unity behavior to the original gameplay graph.
- The repository is public. Keep credentials and separately stored recovery archives out of it.
- Keep all existing historical branch refs unless a separate audit classifies them and the owner explicitly authorizes cleanup.
- Routine commits do not deploy. Site publication requires explicit authorization and exact-SHA release evidence.

Start with [AGENTS.md](AGENTS.md), [CONTRIBUTING.md](CONTRIBUTING.md), and [the current handoff](NEXT_AGENT.md).
