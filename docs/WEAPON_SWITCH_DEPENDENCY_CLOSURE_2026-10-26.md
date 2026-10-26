# Weapon-switch dependency closure — 2026-10-26

## Acceptance target

Verify the original player-initiated weapon swap and the AI empty-weapon fallback using the recovered Operator loadout, without inventing a secondary weapon, loading substitute IK curves, or changing the protected single-weapon test host. The acceptance must prove active/inactive slot ownership, the full original arm transition/callback order, selected weapon state, and repeat/reversal behavior.

## Source owner and lifecycle (recovered)

| Edge | Recovered source | Required behavior |
|---|---|---|
| Player input | `runtime/Original/InputMapping.cs`: `switchWeapons` is the Weapons-category Tab binding; `InputHandler` computes Down/Pressed/Up edges and respects suspension. | Only the down edge starts a swap; focus/suspension must not synthesize one. |
| Player owner | `runtime/Original/FullSource/PlayerController.cs`: only in EntryTeam and while not ragdolled; `input.Keys.switchWeapons.Down` calls `Body.armController.SwitchWeapons()`. | Establish that the live Mission's player controller uses this source path and input edge. |
| AI fallback | `runtime/Original/CharacterBehavior/CharacterBehavior/SwitchWeaponCondition.cs` sets PendingAction when the active weapon is permanently empty, its magazine is empty, no ammo exists for it, and Equipment.GetNonEmptyWeapon() returns another weapon. `SwitchWeapon.cs` invokes the same arm controller and clears PendingAction when its sequence ends. | Test the condition separately from manual input; verify no fallback when active weapon can still reload or the other weapon is empty. |
| Weapon slot owner | `CharacterBody.ActiveWeapon/InactiveWeapon`; `Equipment.primaryWeapon/secondaryWeapon`; `CharacterBody.InitializeEquipment`; `EquipmentHelper.SpawnEquipment`; `Equipment.GetNonEmptyWeapon`. | Verify primary/secondary ownership, holster flags and ammunition remain correct after both directions of swap. |
| Swap sequencing | `IKArmController.SwitchWeapons`, `IKArmSequencer`, `IKArmSequenceRuntime`, `IKArmAnimationFrame`, `IKArmAction`, `IKControllerInvoke`; callbacks include holster active, activate pending/previous, move pending to hand, and weapon activated state reset. | Drive original frames to completion and assert both hands, pending/previous weapons, callbacks and sequence termination; include interruption/cancel cleanup if supported. |
| Default loadout source | `runtime/Original/FullSource/OperatorSpawnData.cs`: defaults primary to M4A1 / 5.56-FMJ, secondary to Glock-18 / 9-FMJ; supplies Primary magazine, Secondary magazine and other equipment items. Saved `operators.dat` may override defaults, so the acceptance must explicitly select and report the default source loadout or recover a serialized saved loadout. | Test an actual source `SerializableEquipment` through `EquipmentHelper` and `CharacterBody.InitializeEquipment`; do not set ActiveWeapon or PendingWeapon directly. |

## Asset closure — present versus missing

| Asset family | Evidence at `develop/wasm` | Closure status |
|---|---|---|
| Operator actor hierarchy | `source-reference/Original/CharacterDependencies/ActorPrefabs/Operator.prefab`; `game/assets/operator/operator.json`; animator and hand exports. | Partial; visual hierarchy/renderer data exists. |
| M4A1 primary weapon | `game/assets/weapons/M4A1/weapon.json` (27 hierarchy nodes, 5 renderers); M4-specific mechanics/ammunition export and 5.56-FMJ source prefab graph. | Recovered for the existing M4 firing/reload fixture. |
| Glock-18 secondary visual | `game/assets/weapons/Glock-18/weapon.json` (23 hierarchy nodes, 6 renderers, 5 textures). | Visual hierarchy only. It is not the serialized original `Assets/Resources/weapons/Glock-18.prefab` component graph. |
| Glock weapon mechanics | Original WeaponBase, CyclingBolt, barrel, arms configuration, magazine component and serialized references from `Assets/Resources/weapons/Glock-18.prefab`. | Missing from tracked `source-reference/Original/Assets/Resources/weapons` and current M4-only `game/assets/weapon-runtime.json.gz` build path. |
| Glock ammunition | `9-FMJ` prefab, projectile/casing/UI graph, 9mm caliber and every GUID/fileID reference; source magazine and loose-round counts from the default loadout. | Not recovered in the tracked extracted source graph. The source default loadout names the ammunition but the switch fixture cannot instantiate it. |
| Authored arm sequences | Operator prefab references seven IKArmSequence assets: holster pistol GUID `def346a0b435e414f91d1f240c7d016e`; unholster pistol `afe828104b427ef47b9ac7ce4e2e1b70`; lower shoulder weapon `77a7fc0e3d201394cb8d8ddd031df902`; return shoulder weapon `95f92e749f4341449a76c5052f911111`; pickup grenade `572f009943cda614aa21d5dbd0dbbbea`; return grenade `67a7d386af05bf346b569e4f4dfaa594`; apply gauze `0b10c0f14d5ea4b41ae81ab8b177805f`. | Missing from tracked authored `.asset` files and `game/assets/operator` runtime export. Do not replace them with guessed frame curves. |
| Mission UI and pointer | PlayerController binds `UI.weaponPointer`; it also depends on Mission, InteriorMode, Global Context, Settings and Operators. | Existing owner wiring must be present in any full-player acceptance; WeaponHost bypasses it. |

## Test/fixture boundary

- The completed #74 `WeaponHost` contract builds one active detachable-magazine M4A1. Its `CharacterAction` has None/SwitchMagazine/WorkBolt only, and it does not run the source PlayerController input owner or a second serialized weapon.
- `runtime/Engine/WeaponHost.cs` is protected by the Python gameplay-owner frozen-hash checks. Do not edit it to add a shortcut.
- Directly assigning a fabricated secondary `WeaponBase`, changing `ActiveWeapon`/holster flags, or constructing runtime IK frames by hand would not test the recovered original data and is not acceptable closure.
- Existing source files prove the action owner path only. They do not prove that the current browser Mission creates or persists this default dual-weapon setup, that an authored transition finishes, or that a second magazine/ammunition graph is valid in browser runtime.

## Recovery and acceptance batch

1. Inspect the canonical recovered export and its manifest for all seven sequence GUIDs plus `Glock-18.prefab`, `9-FMJ.prefab`, the Glock magazine and their transitive refs. Record per-file SHA-256, GUID, local fileID, asset type and dependency edges before writing an importer.
2. If present in the canonical source archive, recover the complete asset graphs and authored IK assets together, extend the bounded profile only after exact GUID closure, and add strict missing/stale-hash checks. If absent, report those precise source objects as unrecoverable inputs; do not generate analog assets.
3. Build a source-driven fixture through the default `OperatorSpawnData` or a fully serialized `SerializableEquipment`; run the original PlayerController input edge and AI condition independently.
4. Assert primary→secondary and secondary→primary transition callbacks, exact IK frame/event order, one active weapon at completion, retained ammo/magazine counts, no duplicate audio, no lingering pending action/sequence, and safe cancellation/re-entry where source defines it.
5. Run the dependent Node, Python boundary, runtime, engine, foundation, Release WASM, full source/profile/package integrity and Chromium IndexedDB/audio workflow on the exact SHA. No publish until all source and package identity checks pass.

## Current concrete blocker and next action

The source switch task and player input owner are present; the blocker is bounded and asset-specific: the tracked recovery does not contain the original Glock serialized mechanics/ammunition graph or the seven referenced authored IK sequence assets, and the current protected test host has no dual-weapon integration. The next action is to search the canonical recovered source archive/index for those exact GUIDs and prefab paths, record a complete closure/hash manifest, and then proceed to a source-driven fixture only if the full dependency set resolves. Meanwhile continue independent gates in the dependency index.
