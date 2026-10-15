# WASM runtime continuation checkpoint — 2026-10-15

## Scope and provenance

Private repository: `ethan-bailey/crisis-response-port`. Working branch `codex/projectile-material-impacts`, draft PR #9. **Starting branch commit**: `4c7a855dd29c4b092dc6c4743cb0ea70bb516141`. This session does not modify `runtime/Original/`, the independently maintained Unity foundation/WebGL branch, `main`, or owner-only publishing settings.

The previously repaired original Wood exit-hole geometry/winding is preserved. The following changes are **presentation and diagnostic adapters for recovered game systems**, not substitute game mechanics.

## Source clock / renderer

- `runtime/Engine/WeaponHost.cs`: `Snapshot()` now reports `sourceTime=Time.time`, the *same scaled Unity clock* recorded on weapon events.
- `game/source-effect-clock.mjs`: finite-clock validation and `max(0, sourceTime - eventTime)`. Browser wall-clock timers must not determine recovered muzzle-flash clip progress.
- `game/weapon-runtime.mjs`: muzzle flashes use each original shot event's `time` and each WASM snapshot's `sourceTime`. Asynchronous texture loading is ignored after an effect has already expired; this avoids retaining a late GPU texture for a removed effect.
- `game/presentation.mjs`: forwards `weaponResult.sourceTime` unchanged.
- `scripts/source_effect_clock_test.mjs`: native Node tests for paused/stalled simulated time, future-event clamping, and invalid clock payloads. The manual-only `game-movement-smoke.yml` includes the new test.

This does not alter recovered source weapon/flash keyframes, event production, recoil, firing cadence, ammunition, or physics. Source flash texture and clip remain unchanged.

## Glass texture Reset, window stamping, and impact telemetry

- `runtime/Engine/BallisticEngine.cs`: `UnityEngine.Material.mainTexture` gains a renderer-side assignment observer. This allows source `BallisticWindow.Reset()` to replace its cloned texture while preserving the original implementation.
- `runtime/Engine/BallisticRuntimeHost.cs`: the WASM bridge reattaches its `windowStamp` listener to each new original texture instance, with the correct window's source object ID; completed diagnostic entry-energy records are removed, and unmatched deferred-only records expire at the next source frame.
- `runtime/Checks/BallisticContracts.cs`: checks a live original window stamp, original `Reset()` restoring solid alpha, subsequent original re-stamping reaching the presentation transport, and no retained stale impact-entry telemetry after a runtime frame.

No invented glass-hole algorithm, texture, penetration calculation or visual pane has been added. Window alpha and impact effects still operate through the recovered C# source.

## One targeted compilation test

The single-purpose `.github/workflows/wasm-continuation-verify.yml` triggers automatically only when **that workflow file itself** changes on this branch; ordinary code or documentation commits do **not** launch it. It also supports deliberate manual dispatch. It executes:
1. .NET 8 original recovered contracts, including the new glass reset assertions.
2. Node 22 syntax and source-clock tests.
3. `dotnet publish` of the actual C# Blazor WASM runtime; asserts produced `.wasm` files.
4. Private artifact containing commit SHA and textual test/build evidence **only**, not recovered original binaries.

First verification input SHA: `66478ac4343e156885527734db485fc46ba0f9a3`. Actions run: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38803362416 . **VERIFIED SUCCESS (2026-10-15 20:38 UTC):** native original C# contracts, original BallisticWindow reset/stamp regression, Node effect-clock tests, JS syntax and real Blazor .NET 8 WebAssembly publish all passed at exact input SHA `66478ac4343e156885527734db485fc46ba0f9a3`. Private metadata-only artifact `12005511474` (`source-wasm-continuation-verification`) contains the SHA, generated WASM file list and textual logs. This does **not** by itself verify loaded runtime in Chromium/WebKit or on an actual iPhone.

A single full Chromium/WebKit browser integration run was launched with commit `455a017cc2900bace19a3eb47cf0276aa71db8cf`: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38803647712 . **VERIFIED SUCCESS** (2026-10-15 20:46 UTC) at exact code/workflow SHA `455a017cc2900bace19a3eb47cf0276aa71db8cf`. Native/PhysX game build, input regressions, movement, original scene, landscape Chromium/WebKit touch, camera/upper-body and complete M4/exit-hole Chromium/WebKit tests passed. Private artifact `12005437510` (`crisis-response-mission01-game-smoke`) includes the built WASM runtime, browser evidence and screenshots. In the workflow logs, `weapon_smoke.mjs` reported `[{"engine":"chromium","passed":true},{"engine":"webkit","passed":true}]`. No physical iPhone device test was performed. No new owner-private deployment was made. Existing owner-only Site version 8 remains the last separately verified publication.

## Exact change boundaries and rollback guidance

Baseline SHA: `4c7a855dd29c4b092dc6c4743cb0ea70bb516141`. Review `git diff 4c7a855..66478ac` for this continuation, excluding any later changes.

Files altered or added in this continuation:

```text
runtime/Engine/WeaponHost.cs
runtime/Engine/BallisticRuntimeHost.cs
runtime/Engine/BallisticEngine.cs
runtime/Checks/BallisticContracts.cs
game/source-effect-clock.mjs                 [added]
game/weapon-runtime.mjs
game/presentation.mjs
scripts/source_effect_clock_test.mjs        [added]
.github/workflows/game-movement-smoke.yml
.github/workflows/wasm-continuation-verify.yml [added]
docs/WASM_CONTINUATION_CHECKPOINT_2026-10-15.md [added]
NEXT_AGENT.md                                [handoff update]
```

If this continuation must be undone, **do not reset the branch or force-push** and do not undo independent Unity work. Make a new rollback commit that restores only the listed paths from the baseline, or revert this session's commits in reverse chronological order after checking for later edits. Use `git diff <baseline>..HEAD -- <path>` to distinguish unrelated changes before reverting. Remove only newly added files if reverting all continuation functionality. Preserve `game/original-impact-mesh.mjs` and its earlier handedness regression, which predate the baseline.

### Atomic change ledger (oldest first)

This ledger allows selective rollback without discarding earlier source-mesh work:

| Commit | Scope |
| --- | --- |
| `4629ce1be5a09b0f5c04b74fd012eb29592ae8df` | C# source clock in weapon snapshot |
| `2ddb13f77cdc1db820320f15ec6bb33621f630fb` | Pure JS original-time adapter |
| `f01076a2e0e549f00b58e15ea9a0b68b3dc80ab6` | Muzzle-flash animation clock and late texture cleanup |
| `074df40b7559c5bb98ccdcd7726e6cc3a8dc7576` | Pass original clock from WASM presentation |
| `1da51a02a1f616c4849644b72f09cf1aa28567fc` | Clock regression tests |
| `5b834ceca6117d80bfd93188efc6478f0c6df5f2` | Include test in manual browser workflow |
| `cfb48174655fc891a645b4a980d6d9fa902969d6` | Clear stale impact diagnostics |
| `40bd47c6186a533775e5e14a9995403968c77450` | Watch original Material.mainTexture replacement |
| `23b0fb260175481865c252985d2fda5e6685eb3f` | Rebind window stamp listener per instance |
| `529cc7013fd3387681393ea2e15df9cb0a563e48` | C# glass Reset and bookkeeping tests |
| `19e69eca3b37947441e00e3d536150911cf240d3` | Correct native assertion escaping |
| `a3aef266a1192f87420242798e12c0c925afe471` | Reject null effect-clock payload |
| `66478ac4343e156885527734db485fc46ba0f9a3` | One-shot native C#/WASM compilation workflow |
| `455a017cc2900bace19a3eb47cf0276aa71db8cf` | Explicitly run complete browser integration once |

Metadata and handoff-only commits follow these. To undo only the glass reset transport, consider reverts of `23b0fb2` and `40bd47c` together and remove its tests, not unrelated source clock work. To undo only the source clock, revert the C#, JS and presentation transport as one unit, not one file in isolation. Use `git revert` to create auditable inverse commits, verify the branch head, and rerun the scoped tests on the resulting rollback commit.

Important boundaries: source window stamps still are not projected onto the combined environment mesh; original particle systems and fragment meshes are not rendered; rigid-body impulses, full physiology, and true native Unity comparison remain outstanding. Never claim 1:1 parity on the basis of these limited adapter checks.
