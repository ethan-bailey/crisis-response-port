# Crisis Response browser port

This repository is a source-preserving effort to make the original Crisis Response game playable in an iPhone Safari browser through a C#/.NET WebAssembly runtime. The game is incomplete. A successful compile or browser boot is not gameplay parity.

## Start here

- [Agent prompt and project workflow](AGENTS.md)
- [Current evidence, blockers, and next action](NEXT_AGENT.md)
- [Contributor guide](CONTRIBUTING.md)
- [Gameplay completion contract](docs/GAMEPLAY_COMPLETION_ROADMAP.md)
- [WASM port workflow research](docs/PORTING_WORKFLOW_RESEARCH.md)
- [.NET recovery runbook](docs/DOTNET_BUILD_RUNBOOK.md)

NEXT_AGENT.md is the live handoff. Verify its branch and SHA before using it as current state.

## Project shape

- runtime/Original contains recovered original gameplay sources.
- runtime/Engine contains the Unity-compatible runtime surface used by those sources.
- runtime/Browser hosts the .NET 8 Blazor WebAssembly application and browser adapters.
- game and scripts contain the browser input, scene/source package, physics vendor, audit, and contract tooling.
- global.json pins SDK 8.0.426.

Compatibility code should supply services to original gameplay rules. Preserve authored behavior and asset provenance; keep missing source assets as explicit gaps.

## Verify

The GitHub Actions workflow at .github/workflows/source-contracts.yml runs the repository's Node and Python contracts, .NET runtime checks, and Release WASM publish for relevant code changes. It has no deployment credentials and does not publish the game.

The key local commands are mirrored in [CONTRIBUTING.md](CONTRIBUTING.md). CI and local build results do not replace exact-build acceptance on the owner's iPhone Safari.

## Repository policy

The public integration branch is develop/wasm. Routine owner work is pushed there with a lease against the inspected head. Preserve legacy refs; do not rewrite or delete branches without an explicit audit. Do not publish an intermediate runtime build.
