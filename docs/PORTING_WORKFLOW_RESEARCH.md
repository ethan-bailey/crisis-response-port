# WASM port workflow: research and rationale

## Findings

The most useful pattern for an AI-assisted port is a durable engineering loop, not a longer one-shot prompt. The agent should make a bounded hypothesis, inspect the complete dependency closure, change one coherent slice, run checks that observe behavior, and save the exact inputs and result so the next run can continue without repeating work.

The March 2026 Wesnoth-to-browser case report describes hundreds of agent-driven build experiments. Its authors found that about one third of roughly 100 flag combinations were duplicates when experiment state did not persist. Their fix recorded a named experiment, hypothesis, active configuration, Git commit, resolved flags, artifact hashes, diff, and Playwright outcome. The report says that this let later agents build on prior runs and converge through named steps. This is a project-authored case study, not an independent benchmark, and its C++/Emscripten configuration does not transfer directly to this C#/.NET port. The transferable lesson is to make every experiment reproducible and handoff-safe.

A second pattern is to keep the platform boundary narrow. The Wesnoth report moved browser-specific I/O to a small number of source files. MDN describes WebAssembly and JavaScript as cooperating through explicit imports, exports, and memory objects. For this project that means keeping original gameplay rules in their existing C# graph and putting browser APIs in a narrow host layer. It also favors bounded, compact calls across the JS/.NET boundary over many per-asset calls or large Base64 expansions.

Browser constraints must be part of the acceptance plan from the start. Unity's current Web performance guidance calls out draw-call overhead, browser frame scheduling, and memory settings. Its Web memory guidance describes asset data occupying browser memory after unpacking, mobile heap pressure, and persistent data caching through IndexedDB. Those Unity-specific implementation details do not define .NET Blazor limits, but they identify failure modes to measure in a mobile browser: startup memory, allocation spikes, long main-thread work, render cost, and whether a later launch reuses local data. The target device and exact browser remain the final authority for this port.

Finally, measure behavior in layers. A source inventory, contract test, successful C# compile, WASM publish, browser boot, original gameplay execution, visual comparison, and iPhone acceptance are different claims. Record each separately against an exact commit and artifact. A green compile must never be used to imply playability.

## Workflow applied in this repository

1. Read the live branch, current handoff, relevant original source/asset graph, existing checks, and open workflow state.
2. Write the requested behavior as an observable acceptance contract.
3. Inventory the full dependency closure. Batch recoverable dependencies and related fixes before implementing the dependent slice.
4. Implement in original gameplay code or the smallest required browser compatibility adapter. Keep source identity and serialized data intact.
5. Run existing relevant checks and the pinned .NET 8.0.426 browser build. The Actions workflow performs those checks for code changes; it does not deploy.
6. Record exact SHA, commands, outcomes, measurements, limitations, and one next action in NEXT_AGENT.md. Keep device-only gates explicit and continue independent work.

See the canonical agent prompt in AGENTS.md. NEXT_AGENT.md is the live progress record.

## Sources

- [Porting Wesnoth with Amnesiac Agents](https://lab.rosebud.ai/engineering/porting-wesnoth-with-amnesiac-agents) (March 19, 2026; project-authored case report about stateful AI port experiments)
- [Unity Web performance considerations](https://docs.unity.com/en-us/engine/7000.0/manual/platform-specific/webgl/develop/performance) (current Unity documentation; WebGL-specific performance constraints)
- [Memory in Unity WebGL](https://docs.unity.cn/Manual/webgl-memory.html) (Unity documentation; heap, asset-data memory, and IndexedDB caching)
- [Using the WebAssembly JavaScript API](https://developer.mozilla.org/en-US/docs/WebAssembly/Guides/Using_the_JavaScript_API) (MDN; module imports/exports and memory interoperability)
- [WebAssembly for Native Games on the Web](https://hacks.mozilla.org/2017/07/webassembly-for-native-games-on-the-web/) (Mozilla; older overview of browser execution and platform boundaries)
