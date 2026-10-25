# Authored UI import and FullSource bridge input batch — 2026-10-25

## Scope

Base branch `develop/wasm` was fetched at `6f91910f45db36a1caac39168a1fa3b2ad494c2a` before editing. This batch closes the authored UI import gaps needed to start Mission 01 in EntryTeam mode with its original UI active, and fixes bridge input levels for shared physical keys.

## Changes

- Imported Unity `Canvas` state, `RectTransform` anchors/position/size/pivot, `CanvasScaler`, `GraphicRaycaster`, and the serialized `Graphic`/`Image`/`Text` state used by the source UI.
- Preserved UI material/sprite references across prefab clones. `RectTransform` layout fields and Canvas scale factor also survive clones.
- Changed reference binding so a serialized reference with an external GUID resolves through that asset GUID before considering a same-numbered local file ID. The recovered `WeaponPointer.aimQuadPrefab` collided with a local ID and had been resolving to `Interior mode UI.prefab`, which recursively cloned the UI during activation.
- Changed `FullSourceActorBridge.Step` to compute held keyboard keys from the union of pressed source actions. Multiple source actions can share one key; processing one false action after a true action previously cleared that key.
- Added source UI and active EntryTeam mission contracts. The latter verifies the original `InteractionMenu` initializes and the source `InputHandler` observes a bridge left-action press and release.

## Verification

All checks below passed after the final code changes, using the recovered source package at `/tmp/cpr-scenes-full-source.json`:

- Release runtime build: `dotnet build runtime/Crisis.Runtime.csproj --no-restore --no-incremental -v:q` — 0 errors.
- `EngineChecks` — 69 assertions passed.
- `FoundationChecks` — 121 assertions passed.
- Runtime checks — original hierarchy/lifecycle/input/IK/weapon/ballistics contracts passed.
- Source UI prefab contract — 29 `RectTransform`s, one `Canvas`, one `CanvasScaler`, one `GraphicRaycaster`; `WeaponPointer` resolves `Assets/GameObject/Aim quad.prefab`; the original oxygen-overlay material remains assigned after cloning.
- Headless Mission contract — the intentional missing-UI refusal still passes.
- Active EntryTeam Mission UI contract — Mission 01 started, 2 Operators and 5 Occupants spawned, original `InteractionMenu` initialized, and `FullSourceActorBridge.Step` press/release reached the original `InputHandler`.
- Release Browser/WASM publish completed at `runtime/Browser/bin/Release/net8.0/publish/`.

These are source/runtime checks. They do not show the active browser page using the bridge, renderer/physics projection, or iPhone Safari gameplay acceptance. The Mission report still identifies unsupported original built-in sprites/shaders, scene components, and references; no substitute assets were invented.

## Dependency sizing and disposition

The recovered Global Context + Mission 01 + Operator closure remains about 1,918 assets, 22,361 records, and 122,132,020 source bytes, with 39 assets containing unresolved/unsupported references. Keep this closure out of default phone startup until the page payload and memory use are reduced. The UI importer makes the original UI data executable in the test host; it does not implement the browser layout/rendering driver.

## Next action

Batch the browser page migration: route `game/index.html` input and frame ticks through the existing FullSource bridge endpoints, project the bridge's actor-owned transform snapshot to rendering and physics, and retire transport-actor authority only after the combined source/bridge/weapon contracts pass. Then request owner iPhone Safari testing at that playable milestone. Never deploy.
