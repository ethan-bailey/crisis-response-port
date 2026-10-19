# REA evidence in the WASM parity workflow

## Decision

Use REA as a bounded static inspection step after a reproducible WASM package
exists. It can verify facts about managed assemblies and WASM modules. Gameplay
parity still depends on the project's original-source, lifecycle, physics,
browser and device checks.

REA's managed-code tools inspect .NET metadata and CIL without loading or
executing the assembly. Its WASM artifact tool uses caller-supplied WABT 1.0.42
to validate a module and report sections, imports, exports and decoded WAT. The
WASM interface comparison reports structural import/export differences; it
does not establish runtime behavior or linking success.

## Per-checkpoint procedure

1. Pin the source commit, reference artifact SHA-256, runtime artifact SHA-256,
   REA package version, and WABT executable versions/digests in the evidence
   record. Keep proprietary game binaries in the existing private artifact
   storage.
2. When the verified original Assembly-CSharp.dll is available, inspect its
   metadata and selected full-source types. Record CIL coverage, exact type and
   method identities, and limitations. Treat recovered C# provenance checks as
   the authority for source identity; REA output is supporting analysis.
3. After the consolidated browser/WASM build, inspect that exact .wasm file:

   ```sh
   REA_WABT_BIN_DIRECTORY=/absolute/path/to/wabt-1.0.42/bin \
     rea inspect-wasm-artifact /absolute/path/to/runtime.wasm \
       --glue_paths /absolute/path/to/runtime.js --format json
   ```

4. Compare the new module's imports and exports with the last accepted module
   using `rea compare-wasm-interfaces`. Review every added, removed, changed,
   duplicate or unknown entry. A structural match is not a behavioral pass.
5. Keep functional checks on the existing path: source-focused C# tests,
   two-actor lifecycle and ownership contracts, then the exact-source WASM build
   and fixed-step Chromium/WebKit interaction scenarios. Compare requested
   browser artifacts only when a matching original reference capture is
   available; missing capture evidence remains unknown.

## Current availability

At the PR #42 head reviewed on 2026-10-19, the branch has no tracked
Assembly-CSharp.dll and no built WASM artifact. The current full-source project
build is a native .NET compile. Therefore no REA result is attached to this
checkpoint, and REA must not be reported as having validated this source graph.
Begin the static REA checks when the matching private reference assembly and
consolidated WASM artifact are both available. Pin exact REA/WABT releases before
adding them to a manual build workflow.

## Upstream references

- [REA managed-code analysis](https://github.com/morluto/rea/blob/main/docs/managed-code-analysis.md)
- [REA WASM artifacts](https://github.com/morluto/rea/blob/main/docs/wasm-artifacts.md)
- [REA browser scenario contract](https://github.com/morluto/rea/blob/main/docs/browser-scenario-contract.md)
