# Original room control dependencies — 2026-10-18

Full original CharacterBody compiler closure remains incomplete. Source commit
`7b592b18fd8972cd31f03725124b10c7aadc290f` builds through the normal
`runtime/Crisis.Runtime.csproj` project and exits **1** with **31 diagnostics / 24
missing names**. The previous executable checkpoint `a41ae49b33139b6400089036f4984378f91382d3`
had 35 diagnostics / 28 missing names. This batch resolves four diagnostics and
introduces none. The comparison is pinned to those exact source graphs; it does
not estimate later errors hidden by unresolved declarations.

## Recovered source

The commit recovers original `Coroutine`, `Interaction`, `Interactable`,
`ToggleElectricity`, `Doors` and `FireAlarm` classes from the SHA-pinned game
archive. Their original method bodies and execution order are byte-identical
after removing the documented `Crisis.Runtime.FullSource` namespace wrapper.
No scheduler, interaction, room-power or alarm gameplay was substituted.

The regenerated dependency inventory now reports **366 local C# sources, 1,613
symbols, 1,042 conservatively reachable files and 84 ambiguous names for ownership
review**. The full source identity verifier passes for 203 original/decompiled
identities.

## Verification

| Gate | Result |
| --- | --- |
| `runtime/Crisis.Runtime.csproj` | **FAIL, exit 1; 31 diagnostics / 24 missing names** |
| Diagnostic comparison with `a41ae49` | **4 resolved; 0 introduced** |
| FoundationChecks project rebuild | Pass; **120 assertions** |
| Isolated EngineChecks binary | Pass; **45 assertions** |
| Archive source identity | Pass; **203 identities** |
| Inventory freshness and audit tests | Pass; **8 audit tests** |
| Actor migration / ownership checks | Pass; zero violations |

The EngineChecks binary ran successfully, but rebuilding that separate test
project hit a transient .NET process-stat startup failure. Its source inputs are
unchanged by this batch. This limitation does not affect the actual runtime
project build log, which completed and returned its compiler diagnostics.

Compiler logs, exit codes, normalized diagnostic comparison, tests, inventory,
source identities and SHA256 manifest are preserved in
`docs/evidence/room-controls-20261018/`, pinned to this source SHA and tree
`5726855a5f8548a5e5c9ee5fa0cd93e0e68f6676`.

The remaining compile names include original room lighting and fog, door traps
and grenade spoons, blood/decal/painter systems, mission modes/UI and ownership,
score, audio mixing, and Usage. The existing `MarksmanMode` static/instance type
conflict also remains. These declarations must be recovered with their original
dependencies and compatible engine services. No full-source exclusions,
replacement actor, or frozen transport gameplay changes were added. No successful
full compile, GitHub Actions verification, initialization, combat, or browser
result is claimed.

Next gate: continue with a cohesive original subsystem batch, run the normal
runtime build after each batch, and preserve its exact exit status. Full closure
still requires a zero-error project build and GitHub Actions verification at that
exact committed source SHA; actor initialization, two-character combat and
WASM/browser validation follow as separate gates.
