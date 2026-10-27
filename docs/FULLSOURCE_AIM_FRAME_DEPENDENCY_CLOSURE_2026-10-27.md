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
