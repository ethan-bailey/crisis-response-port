# WASM port roadmap

The old point-in-time roadmap mixed historical build state and workflow rules. Use the current [agent prompt](../AGENTS.md), [gameplay completion contract](GAMEPLAY_COMPLETION_ROADMAP.md), and [live handoff](../NEXT_AGENT.md) instead.

The durable port sequence is:

1. Recover and provenance-check original source, serialized data, scene/prefab dependencies, and assets.
2. Implement the minimum Unity-compatible services required to execute the original C# graph.
3. Keep browser APIs behind narrow, asynchronous host adapters and transfer source data in bounded batches.
4. Close gameplay dependencies in source order and compare behavior to native/reference evidence.
5. Build and package the pinned .NET WebAssembly runtime; measure load, memory, responsiveness, persistence, and failure recovery on mobile.
6. Accept the exact build on physical iPhone Safari and publish only with explicit owner authorization.

A successful compiler run is one gate, not a completion claim. Do not use stale SHA values or old check results as current evidence.
