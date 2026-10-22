# Original settings and serialization compiler batch — 2026-10-18

Full CharacterBody compiler closure is **not complete**. The normal
`runtime/Crisis.Runtime.csproj` build exits **1**, with **42 distinct diagnostics
and 34 missing names** at source commit
`23e4cf0db8ca9a9aad09af44059f5939d70879c5`.

## Live-state gate and comparison

Inspected all branches and the four open PRs (#7, #42, #56, #57), current
`develop/wasm` (`2fc0a8e93be97c4d99123910fad4bd25798e71a8`), and the full-source
branch (`8949d029e90d4700e2d7beaa2b4297ac4cdeb2be`). The latest comparable full
GitHub compiler evidence remains run 39091701268 at `407c66d`; narrow remediation
evidence is run 39093133256 at `389d8e8`. Those runs do not test this batch.
Verified the target head immediately before edits and again before committing;
the branch update used an expected-SHA lease.

An exact Git-object sparse checkout supplied every runtime source file; only
unrelated preview/assets were deferred. Git, Python and .NET SDK 8.0.426 are
available. The recovery ZIP was downloaded through authenticated GitHub access
and verified against its pinned SHA256
`cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.

The actual starting graph at `8949d029` reproduces **64 diagnostics / 44 names**.
The earlier 30/21, 144/60 and 117/50 counts are historical checkpoints, not the
starting state for this batch. Exact-source builds of both the starting and new
source are preserved in `docs/evidence/settings-closure-20261018/`.
The normalized comparison resolves **22 diagnostics**, introduces **zero**, and
leaves 42. Missing declarations still mask later method-body/type-identity
errors; these numbers are not a complete remaining-work estimate.

## Changes

- Recovered the complete 61-file original FullSerializer package, including its
  version manager, reflected/cyclic object converters and Unity value converters.
  Replaced the prior metadata-only fsProperty declaration with the original.
- Recovered original settings models and their entire historical migration
  chains, bullet-time/range records, typed events, WeaponType, QualityLevels,
  Serializer and Encryption. All new source bytes match indexed archive entries.
  Namespace wrappers/imports and Unity Random/Object aliases are recorded;
  original methods, formulas and call order are unchanged.
- Added serialized Unity value records, curve-key assignment, rectangle edge
  setters, two-argument events and display/quality driver bindings. QualityLevels
  executes its original complete preset method against the bound renderer.
- Enabled .NET 8's legacy BinaryFormatter compilation support for the recovered
  original disk serializer. Disk encryption and settings-file compatibility have
  **not** been executed/verified; the source's legacy cryptographic requirements
  need separate runtime verification.
- Refreshed the dependency inventory: 347 checked-in sources, 1,601 symbols,
  1,023 conservatively reachable files and 83 ambiguous identities. Ambiguity
  remains explicit; this lexical graph is not semantic binding evidence.

The complete original CharacterBody and existing failing sources remain normal
runtime compiler inputs. No production Compile exclusions, substitute actor,
mutable health owner or frozen transport gameplay changes were introduced.

## Validation at the committed source

Local .NET SDK 8.0.426, source `23e4cf0`:

| Check | Result |
| --- | --- |
| Foundation build | Exit 0 |
| Foundation/Behavior Designer execution | 104 assertions and all ten RuntimeContracts groups pass |
| Isolated engine build/execution | Exit 0; 21 assertions pass |
| Audit-tool regression suite | Eight tests pass |
| Archive provenance | 187 original/decompiled identities pass |
| Actor migration and ownership | Pass; zero migration violations |
| Inventory freshness | Pass |
| Complete original runtime build | Exit 1; 42 diagnostics / 34 missing names |

New execution checks cover original serializer cyclic identity, fsProperty opt-in
selection, version-metadata migration constructors, callbacks, original curve,
rectangle and gradient converters, typed-event values/listener clearing, and
complete original quality preset values/application order.

JSON for persistent Unity event listeners, display changes and renderer quality
settings requires the relevant host drivers. Unbound services throw explicitly;
tests do not claim that native rendering or original persistent inspector
listeners exist in the browser. The original game settings Load/Save methods,
character initialization, combat and browser integration have not been verified.

An early SDK CLI invocation failed on process-stat retrieval; an attempted
exact-source MSBuild invocation also produced MSB1025. Successful direct-MSBuild
invocations supplied the actual results above; failed infrastructure invocations
are not represented as compiler or test success. The browser timed out opening
GitHub Actions; the connector exposes no workflow-dispatch operation. No new
GitHub Actions run is claimed. Tests and build logs are preserved in this private
repository pending exact-SHA CI reproduction. No merge or deployment occurred.

## Reproduce

```sh
dotnet build runtime/FoundationChecks/Crisis.Runtime.FoundationChecks.csproj
dotnet runtime/FoundationChecks/bin/Debug/net8.0/Crisis.Runtime.FoundationChecks.dll
dotnet build runtime/EngineChecks/Crisis.Runtime.EngineChecks.csproj
dotnet runtime/EngineChecks/bin/Debug/net8.0/Crisis.Runtime.EngineChecks.dll
python3 scripts/verify_foundation_sources.py --archive /path/to/original-recovery.zip
python3 scripts/check_actor_migration.py
python3 scripts/check_characterbody_ownership.py --strict
python3 scripts/inventory_source_dependencies.py --check
python3 -m unittest discover -s scripts -p audit_tools_test.py -v
dotnet build runtime/Crisis.Runtime.csproj -v:q > full-build.log 2>&1
# Preserve and supply the actual failing build exit status, currently 1:
python3 scripts/summarize_characterbody_compile.py full-build.log \
  --source-sha 23e4cf0db8ca9a9aad09af44059f5939d70879c5 --build-exit-code 1 \
  --baseline docs/evidence/settings-closure-20261018/baseline-diagnostics.json \
  --output full-diagnostics.json
```

## Remaining gate and rollback

Next unmet gate is still full original compiler closure. Outstanding declared
dependencies include mission/world/UI owners, Usage statistics, original
limb-texture/decal/blood/painter systems, powered lights/doors, interaction,
marksman/occupant/operator/tutorial modes and audio mixer consumers. Then resolve
the exposed method-body and actor/equipment/projectile/IK identity errors without
introducing transport substitutes. Obtain a zero-error complete build and test
that exact source SHA in GitHub Actions before reporting closure.

Two-character initialization/combat and consolidated WASM/browser evidence follow
that gate. The frozen transport owners remain frozen. Revert source commit
`23e4cf0` as a whole to roll back this cohesive settings/serializer batch and its
compatibility changes; revert its separate evidence/documentation commit
independently. Preserve earlier compiler baselines and build history.
