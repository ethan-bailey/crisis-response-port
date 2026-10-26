# Contributing

Read [AGENTS.md](AGENTS.md) before changing the port. It is the canonical agent workflow and source-fidelity prompt. Read [NEXT_AGENT.md](NEXT_AGENT.md) for the current task and verified state, then confirm the live develop/wasm head and current Actions/PR state.

## Workflow

1. Establish the exact live source SHA and clean working state.
2. Define one observable acceptance target from original source behavior.
3. Inventory the complete dependency closure before implementation: scripts, serialized scene/prefab references, GUIDs/fileIDs, assets, lifecycle, engine APIs, browser adapters, and checks.
4. Batch all recoverable related dependencies and supported fixes in dependency order. Keep original gameplay rules and authored data intact; use the smallest compatibility adapter needed for the browser.
5. Run the relevant existing contracts and the pinned .NET 8.0.426 checks. Code changes are also checked by .github/workflows/source-contracts.yml.
6. Commit and push owner-authorized work to develop/wasm using the inspected head as a lease. Use one short-lived PR only when review, branch protection, or isolation materially helps; keep at most one active PR. Never force-push or delete historical branches as routine cleanup.
7. Update NEXT_AGENT.md once at the end with exact SHA, commands/results, remaining limitations, and one next action.

## Verification commands

~~~sh
node --test scripts/*_test.mjs
python -m pip install --requirement requirements-test.txt
python -m unittest discover -s scripts -p '*test.py'
dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -c Release
dotnet run --project runtime/EngineChecks/Crisis.Runtime.EngineChecks.csproj -c Release
dotnet run --project runtime/FoundationChecks/Crisis.Runtime.FoundationChecks.csproj -c Release
dotnet build runtime/Crisis.Runtime.csproj -c Release --no-incremental
npm ci --prefix game --ignore-scripts
npm run --prefix game vendor
dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release
~~~

These checks cover only what their commands exercise. Keep compile, browser boot, original behavior, visual parity, and iPhone Safari acceptance as separate claims.

Read [the release policy](docs/RELEASE_POLICY.md) before any publication. The repository is public. Keep credentials and separately stored recovery archives out of Git. Do not deploy or publish an intermediate build; release work requires owner authorization and exact-SHA evidence. Avoid creating a Codespace for routine work through 2026-11-01.
