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
| Glock weapon mechanics | Original WeaponBase, CyclingBolt, barrel, component slots, magazine component and serialized references from `Assets/Resources/weapons/Glock-18.prefab`. | Present in the canonical recovery archive, absent from the tracked `source-reference/Original/Assets/Resources/weapons` subset and current M4-only `game/assets/weapon-runtime.json.gz` build path. Exact root hash and archive evidence are recorded below. |
| Glock ammunition | `9-FMJ` prefab, projectile/casing/UI graph, 9mm caliber and every GUID/fileID reference; source magazine and loose-round counts from the default loadout. | Present in the canonical recovery archive, absent from the tracked extracted runtime graph. The source default loadout names it, but the current switch fixture cannot instantiate it. |
| Authored arm sequences | Operator references seven `IKArmSequence` components: holster pistol GUID `def346a0b435e414f91d1f240c7d016e`; unholster pistol `afe828104b427ef47b9ac7ce4e2e1b70`; lower shoulder weapon `77a7fc0e3d201394cb8d8ddd031df902`; raise shoulder weapon `95f92e749f4341449a76c5052f911111`; pickup grenade `572f009943cda614aa21d5dbd0dbbbea`; return grenade `67a7d386af05bf346b569e4f4dfaa594`; apply gauze `0b10c0f14d5ea4b41ae81ab8b177805f`. | The seven original `IKArmSequence` component values and source hashes are already recovered in `game/assets/original-arm-sequences.json`; source C# types are in `runtime/Original/Animation`, and `OriginalArmSequences` loads/composes those records. Playback is explicitly false in the manifest; neither binding the records to the original Operator controller nor executing the authored callbacks is verified. Preserve the exact frames; do not replace them with guessed curves. |
| Mission UI and pointer | PlayerController binds `UI.weaponPointer`; it also depends on Mission, InteriorMode, Global Context, Settings and Operators. | Existing owner wiring must be present in any full-player acceptance; WeaponHost bypasses it. |

## Test/fixture boundary

- The completed #74 `WeaponHost` contract builds one active detachable-magazine M4A1. Its `CharacterAction` has None/SwitchMagazine/WorkBolt only, and it does not run the source PlayerController input owner or a second serialized weapon.
- `runtime/Engine/WeaponHost.cs` is protected by the Python gameplay-owner frozen-hash checks. Do not edit it to add a shortcut.
- Directly assigning a fabricated secondary `WeaponBase`, changing `ActiveWeapon`/holster flags, or constructing runtime IK frames by hand would not test the recovered original data and is not acceptable closure.
- Existing source files prove the action owner path only. They do not prove that the current browser Mission creates or persists this default dual-weapon setup, that an authored transition finishes, or that a second magazine/ammunition graph is valid in browser runtime.

## Recovery and acceptance batch

1. Keep the canonical recovered archive identified by SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d` outside the public repository. The exact archive was inspected read-only; its bytes match the prior recovery evidence.
2. Add the exact Glock, magazine, ammunition/caliber and transitive source graph to the bounded source package/profile with original GUID/fileIDs, per-file hashes and strict missing/stale-hash checks. Do not commit the recovery archive itself or substitute reconstructed data. Reuse the already tracked sequence JSON and add the GUID binding needed for the original Operator to consume those exact records.
3. Build a source-driven fixture through the default `OperatorSpawnData` or a fully serialized `SerializableEquipment`; run the original PlayerController input edge and AI condition independently, then advance the original IK sequence to its authored callbacks.
4. Assert primary→secondary and secondary→primary transition callbacks, exact IK frame/event order, one active weapon at completion, retained ammo/magazine counts, no duplicate audio, no lingering pending action/sequence, and safe cancellation/re-entry where source defines it.
5. Run the dependent Node, Python boundary, runtime, engine, foundation, Release WASM, full source/profile/package integrity and Chromium IndexedDB/audio workflow on the exact SHA. No publish until all source and package identity checks pass.

## Current concrete blocker and next action

The source switch task, player input owner, Glock serialized mechanics, ammunition dependencies, and all seven authored IK sequence prefabs are present in the canonical archive. Their sequence frame data is already represented in the tracked JSON asset and source classes. The earlier statement that source objects were unavailable was based only on the tracked source index and was incorrect. The current blocker is integration and verification: the Glock resource graph is not in the tracked runtime package, and the protected one-M4A1 `WeaponHost` cannot host it. The original arm sequence bridge can parse/compose frames but declares no playback. Do not modify that host. The next bounded implementation is to add the hash-checked Glock resource slice, bind the existing sequence records to the original Operator controller, and exercise reload interruption through the original PlayerController/IK path.

## Canonical archive inventory correction — 2026-10-26

The original recovery archive was resolved and checked against its recorded SHA-256 above. It contains the exact source records listed here. The archive remains separate from the public repository; hashes and extracted records are sufficient for this inventory, not a claim that the runtime consumes them yet.

| Source asset | Bytes | SHA-256 |
|---|---:|---|
| `Assets/Resources/weapons/Glock-18.prefab` | 46,958 | `0810e5f6cae02736b262e2da70aea6009a442598a241620cc1eda3b49b3ec2a0` |
| `Assets/Resources/weapons/components/Magazine Glock 17 round.prefab` | 10,851 | `f259dbbdddbd8938868df47b5d31fa1682048f4c0e20b119cefb872b7057b873` |
| `Assets/Resources/weapons/ammunition/9-FMJ.prefab` | 3,799 | `dc3cbd9cd41dfe2897af78242f45ef8329971044acbb347b33facd3fdcc9c8f6` |
| `Assets/Resources/weapons/calibers/Caliber 9mm.prefab` | 1,493 | `42b99eafb1ba6bde29b7f9f0aee7a0776e20aaba619aca2ab6493856a129073f` |
| `Assets/Resources/weapons/ammunition/projectiles/Projectile 9mm FMJ.prefab` | 6,498 | `5694fd9c83b6710f2208094111dbf54f5cd6e8350944e14a676c42c4d8de8129` |
| `Assets/Resources/weapons/ammunition/casings/Casing 9x19mm.prefab` | 7,327 | `c3d10bef15432ac46d90c69cadbe2892d05c0d0ebda5a08c0c28bb6a8a14e1cb` |
| `Assets/Resources/weapons/ammunition/ui models/9x19mm FMJ.prefab` | 9,082 | `0364246634eacaffedf221152c65d2d82b561e509426589d53efb0ec2a20cc43` |

The Glock prefab is 75 Unity records with 28 external GUIDs. Its WeaponBase is serialized as Freehand (`type=1`), caliber GUID `882d0d4f30272a444a9c7efeac2597c2`, and default-ammunition GUID `b110b20b77ba07e4196c8c873c63987f`. Its direct source dependencies include `WeaponBase`, `CyclingBolt`, `WeaponBarrel`, `WeaponUI`, and `WeaponComponentSlot` scripts; the 17-round Glock magazine and 9mm caliber/ammunition prefabs; the Glock visual and muzzle-flash prefabs; source materials, audio clips, mixer and weapon PhysicMaterial. The exact direct edges are preserved in the original serialized prefab and must be resolved from the archive when building the bounded import manifest.

The seven sequence GUIDs resolve as follows. Each source prefab has four Unity records and one authored `IKArmSequence` component; the hash covers the original prefab bytes. Exact sequence field data and these hashes are already tracked in the original arm-sequence recovery bundle; the remaining dependency is runtime GUID binding and playback, not another source recovery pass.

| GUID | Source prefab | Bytes | SHA-256 |
|---|---|---:|---|
| `def346a0b435e414f91d1f240c7d016e` | `Assets/GameObject/Holster pistol.prefab` | 1,810 | `2f4aa20255d361a2725879c2d6df2cd4f15ab6c7d6c2306efba40f24909c2ee3` |
| `afe828104b427ef47b9ac7ce4e2e1b70` | `Assets/GameObject/Unholster pistol.prefab` | 1,799 | `c0356970e9828e981b4d0d4b23bfbc9499d6544a4994b748a31012489e7c06de` |
| `77a7fc0e3d201394cb8d8ddd031df902` | `Assets/GameObject/Lower weapon.prefab` | 1,800 | `741bad97110ccea44742705dee120bf970e6efe6cb60d93a31547edf185c6567` |
| `95f92e749f4341449a76c5052f911111` | `Assets/GameObject/Raise shoulder weapon.prefab` | 1,668 | `6e9525f3b896c162d8290de64335a8b63056cad499d25d0701da2d8119e0645e` |
| `572f009943cda614aa21d5dbd0dbbbea` | `Assets/GameObject/Pickup grenade.prefab` | 2,082 | `c87b99b28d24aa2ab4af89f02a5420ef2f8374c824368010c35a3c2b9237d4e7` |
| `67a7d386af05bf346b569e4f4dfaa594` | `Assets/GameObject/Return grenade.prefab` | 1,665 | `fd9e0565ce4f977670dfef0421e5a18aa9c7271a652d45ad2d5fd8a69ecaeb89` |
| `0b10c0f14d5ea4b41ae81ab8b177805f` | `Assets/GameObject/Apply gauze.prefab` | 2,750 | `7262845b5531f5e770b5cab1e7072f1299bc4faee8d84a1cd1b01cb37de87bfb` |

For the default shoulder-M4A1 → freehand-Glock transition, the original controller appends the authored Lower weapon sequence followed by Unholster pistol. The reverse transition uses Holster pistol followed by Raise shoulder weapon. The serialized sequence frames include callbacks and timings; runtime verification must invoke these source owners rather than assign active weapons, holster flags or sequence frames directly.

## Mission source switch contract — 2026-10-26

The six-root Mission source profile already contains the Glock-18, its 17-round magazine, 9-FMJ/caliber/projectile/casing/UI graph, and all seven serialized IK sequence prefabs. The extra-root export resolved to the same profile inventory (1,919 assets, 22,365 records, 205,514,345 source bytes and 97,767,549 compressed bytes); no additional source import is needed. `BrowserEntryTeamMission.Start` creates Operator 1 with the default M4A1/Glock-18 equipment and binds sequence references from the serialized Operator prefab.

`--mission-weapon-switch-contract` passed on the complete profile. It starts at M4A1 with 30 rounds and Glock-18 with 17, sends the original `switchWeapons` input edge after the authored weapons category enables, and advances original PlayerController/IKArmController updates. M4A1→Glock-18 settled in 45 frames with `lower-primary → holster-M4A1 → move-Glock-18-to-hand → activate-Glock-18`; the reverse settled in 32 frames with `holster-Glock-18 → activate-M4A1`. Slot ownership, held/holstered state, both magazine counts and shot count remained correct. Sequence references were checked against all seven source prefab GUIDs, paths and raw-source hashes. The serialized “Raise shoulder weapon” callback is `ActivatePendingWeapon`, which is what the source check now verifies.

Exact run #124 (SHA 27ab7a17a9c18ff32a4c80e23aebd946a02afe78, Actions 40441240466) passed both reload interruption and AI empty-weapon fallback contracts on the full six-root profile. The player reload test enabled the original player BehaviorTree, sent the authored magazine input, observed the source ChangeMagazine action Running, then sent the original switchWeapons edge during active reload; weapon switching settled in 42 frames, the task returned Success via its active-weapon cancellation result, and OnEnd cleared equipment manipulation and PendingAction. The AI test used Mission.BeginMission-enabled nonplayer Operator 2: source magazines/reserves and chamber were emptied by original owners; BehaviorManager ran SwitchWeaponCondition→SwitchWeapon; M4A1→Glock settled in 46 frames with both sequencers ready, no shots, preserved 17 Glock rounds, then the both-empty negative case made no swap. Transient task statuses become Inactive after a tick, so evidence captures SwitchWeapon Running and sequence progression rather than an instantaneous condition Success/final action Failure enum. Casing ejection uses a no-op headless force sink. Keep runtime/Engine/WeaponHost.cs frozen; iPhone pose/audio and native physics remain open.


## 2026-10-27 discovered serialized compatibility-field import dependency

The #120 Mission source-entry trace reaches original M4A1 compatibility during Operator equipment initialization and calls WeaponComponentSlot.QualifierMatch with a null qualifier argument. Resources.Load routes authored weapon prefabs through SceneLoader.LoadPrefab, which binds present serialized fields before the deferred activation callbacks. The matcher source already treats empty strings as the no-qualifier case, but the recovered fields can be absent or explicitly null at the import boundary. The bounded port rule is to initialize only the four original WeaponComponent/WeaponComponentSlot qualifier string fields to empty after serialized field binding and before activation. Keep both recovered matcher files byte-identical; do not change weapon ownership, compatibility token logic, or initialization order.

The serialized-component importer contract passed within runtime contracts on SHA 552319ed9a3a0e4f93e1622351def0e4f79fafe2. Its first implementation failed the separate FoundationChecks assembly because that assembly omits FullSource; exact result and fix are in the qualifier evidence file. The full authored Mission package source-entry acceptance has not rerun successfully yet. Required next assertion: the packaged Operator initializes its authored M4A1 equipment and Mission BeginMission reaches a playable source actor without qualifier exceptions. Airborne pose/device visuals remain separate.
