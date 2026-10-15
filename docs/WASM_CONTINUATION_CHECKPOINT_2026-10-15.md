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

First verification input SHA: `66478ac4343e156885527734db485fc46ba0f9a3`. Actions run: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38803362416 . **Pending at initial writing; inspect conclusion and the failing step before declaring compile success.**

No full Chromium/WebKit browser test or new owner-private deployment was run in this checkpoint. Existing owner-only Site version 8 remains the last separately verified publication.

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

Important boundaries: source window stamps still are not projected onto the combined environment mesh; original particle systems and fragment meshes are not rendered; rigid-body impulses, full physiology, and true native Unity comparison remain outstanding. Never claim 1:1 parity on the basis of these limited adapter checks.
