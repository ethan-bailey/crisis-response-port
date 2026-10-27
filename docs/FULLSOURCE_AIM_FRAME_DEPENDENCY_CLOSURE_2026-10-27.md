# FullSource actor aim/frame dependency closure — 2026-10-27

## Exact dependency chain

| Layer | Recovered owner or record | Relevant behavior |
|---|---|---|
| Serialized owner | `Assets/GameObject/Interior mode UI.prefab`, component script GUID `aaf710daebc68c4c689fd5edcfe1e048` | `WeaponPointer.aimQuadPrefab` links to GUID `347b2eb609587614a9e6a5a7b1b228a4`, prefab GameObject fileID `1709254077376921`. The prefab contains its Transform (`4730242686860875`) and enabled, non-trigger MeshCollider (`64724804207875695`) on Aim layer 19, using mesh GUID `2758acda9e5eb8f47be20fe8219f5acf`.
| Original pointer lifecycle | `runtime/Original/FullSource/WeaponPointer.cs` | `Awake` instantiates the serialized prefab; `Start` binds the Mission camera, input and pause callback; `Update` positions the AimQuad, raycasts only when the interaction menu is closed, stores the hit in body-local `localPoint`, then derives `WorldPoint` from the controlled body's transform. |
| Original player consumer | `runtime/Original/FullSource/PlayerController.cs` | Initialization assigns the scene-owned pointer to the player body. Its update consumes `WorldPoint` for direction and weapon aim before the source actor frame completes. |
| Browser actor bridge | `runtime/Engine/FullSourceActorBridge.cs` and `runtime/Browser/Program.cs` | `SetFullSourceActorAimTarget` writes the source pointer's private body-local point and public world point. `TickFullSourceActorBridge` then executes the original actor update. |
| Page adapter | `game/index.html` | The FullSource branch updates/raycasts the AimQuad, writes the source-owned target, then calls the actor tick. The legacy preview owns a separate aim block. |

## Failure evidence and source cause

Actions #138 (`40488053121`) built and integrity-verified package artifact
`12726611219` (package SHA-256
`201d17be916562345ab32f4009c9ddc3f0e24bc8e319c1e11365fb2406c97e51`). Its
FullSource Mission browser diagnostics artifact `12728530213` (SHA-256
`3a391df31502a99fed4c443d2560c9492a02cebbe829ceb7abab5dd0a3c73a3b`) reports
that all 1,919 profile assets were verified and the actor reached the playable
transition, then the first source-mode frame displayed
`ReferenceError: aimQuadTarget is not defined` at `game/index.html:586`.
`aimQuadTarget` was a `const` local to `load()` while `frame()` is declared at
script scope. The legacy preview's separate aim code could not mask the error
because FullSource mode exits through its own frame branch.

## Repair and regressions

The helper is now declared once at script scope on pushed source SHA
`b9c9d095486a3ef21e57164c11420aa166c12afa`. It retains the same raycast,
shoulder/body-local target conversion and bridge call, and remains ordered
before `TickFullSourceActorBridge`. The packaged-page wait loop checks the
visible runtime-error panel every 500 ms (instead of waiting 15 seconds between
diagnostic samples), so this scope regression fails promptly with the visible
stack.

Local checks on the unpushed repair based on source SHA
`5dc952aaf10de861601d3acdf408d425b477d8c5`:

- `node --test scripts/source_character_animator_test.mjs scripts/browser_input_test.mjs scripts/game_error_panel_test.mjs`: **28/28 passed**; includes top-level helper scope and target-before-tick ordering.
- `node --test scripts/*_test.mjs scripts/*_contracts.mjs`: **170/170 passed** after `npm ci --prefix game --ignore-scripts && npm run --prefix game vendor`.
- `python3 -m unittest discover -s scripts -p '*test.py'`: **58/58 passed**.
- Inline page module `node --check`, browser smoke script `node --check`, and `git diff --check`: **passed**.
- Exact Actions run #140 (`40490722710`) was queued on this repair SHA; actual packaged-page acceptance was **not yet run** at the next batch's start. A follow-on workflow change may supersede it; the latest exact-SHA run is the source of acceptance truth.

This closes no visual, native physics, or iPhone acceptance. Gate 2 touch/aim
feel and gate 5 rendered/native pose remain open until their own evidence passes.

## User-confirmed deployed aim failure — 2026-10-27

The user reports the live page still throws `Can't find variable: aimQuadTarget` at `game/:581:22`. Read-only Sites metadata confirms the publication remains v85; the source fix is present on current `develop/wasm` (declaration at script scope before `frame()`) but has not been published. This is a deployed/source version mismatch, not evidence that the current source fix failed. Exact-source Actions #141 and its packaged Mission browser acceptance are still pending. No Site publish or access-policy change is authorized in this recurring run. Following the separate release gate, publish only after exact source build and package integrity pass and only under a separate explicit release request.

## Newly exposed JS interop serialization edge — Actions #143

The current source frame now reaches the original aim bridge; the earlier
script-scope aimQuadTarget failure is no longer the observed failure on this
source revision. On exact source SHA
`2781d978d7d1c672c3b7749294ef38cc9268122a`, Actions #143 run
`40494519086` passed the pinned .NET 8.0.426 source build, WASM publish/output,
full Mission package integrity, and the 1,919-asset cold/warm IndexedDB loader.
The independent Chromium cache/audio job also passed. The packaged FullSource
Mission grounding step failed when its first frame called
`RuntimeBridge.SetFullSourceActorAimTarget`:

```
System.Text.Json.JsonException: A possible object cycle was detected
Path: $.normalized.normalized...sqrMagnitude
at RuntimeBridge.SetFullSourceActorAimTarget(Single x, Single y, Single z)
```

The caller in `game/index.html` passes a target and awaits but does not consume
the return value. The source bridge in
`runtime/Engine/FullSourceActorBridge.cs` correctly applies the supplied world
point to the original `WeaponPointer.localPoint` and `WorldPoint`; however,
`runtime/Browser/Program.cs` wraps the returned Unity `Vector3` in
`JsonSerializer.Serialize`. Unity's computed `normalized` property returns
another `Vector3`, recursively exposing the same property graph to
System.Text.Json and throwing at the JS interop response boundary after the
source state mutation. This is a serialization boundary failure, not evidence
that the source pointer/raycast or body-local conversion is incorrect.

Acceptance dependency and regression: retain the current exact-packaged-page
Mission grounding browser smoke. It checks the visible runtime error panel is
absent, then exercises the aim/frame path on the integrity-verified package
whose manifest sourceCommit matches workflow `GITHUB_SHA`. This assertion
failed before the response-shape repair. The repair must preserve original
bridge side effects and return only plain scalar coordinates (or an equally
finite JSON-safe DTO); rerun the package build, hash/integrity verification, and
this exact browser step on the new source SHA. Do not infer a successful aim
pixel or native pointer visualization from an error-free interop response.


## Browser acceptance world-point measurement dependency — Actions #144

Exact Actions #144 (run `40497736559`) tested source SHA
`f4cd527335f2dc575d70e2c3dc8474c8a8064298`. The pinned .NET 8.0.426 build, WASM output, source/provenance,
complete Mission package integrity, 1,919-asset FullSource profile, Chromium
IndexedDB cold/warm acceptance, and independent cache/audio job passed. The
packaged FullSource page loaded without page errors and reached the playable
grounding state. The grounding smoke then failed at
`scripts/mission_grounding_browser_smoke.mjs:199` on:

```
touch-down on the movement stick must preserve the current absolute aim
```

Trace the assertion before changing gameplay: `game/index.html` reports
`aimPoint` by applying the locked aim direction from the currently animated
source shoulder. The smoke reads that point before and after separate
Playwright evaluations. The original animation may advance the shoulder
between those reads, moving world-space `aimPoint.y` even when the
`WeaponPointer` direction and vertical input anchor are unchanged. The
movement-stick callback in `game/browser-input.mjs` anchors aim on pointer
start and applies vertical-only corrections; horizontal movement is
independent. This is a test measurement defect, not evidence that the input
path reset aim.

The smoke already exposes `aimDirection` from `window.__gameState()`. Repair
the acceptance assertions to measure the stable direction: a neutral
pointer-down must retain the current direction, vertical stick travel must
change its vertical component, horizontal travel and release must preserve
that vertical component. Do not compare the shoulder-relative world target
across frames and do not freeze, offset, or otherwise alter source animation.
Keep the exact-package/sourceCommit check, intentional HTTP 503 retry case,
grounding evidence, sprint wall geometry and walk/release assertions. Rerun
the packaged page on the next exact pushed source SHA; until then aim browser
acceptance is **failed** on #144 and the test repair is **untested**.
