# Crisis Response — verified archive inventory

## Artifact and extraction

- Original archive: `Crisis Response.rar`
- Archive size: 67,925,884 bytes (64.78 MiB)
- Archive SHA-256: `5a12ff494b26713d149f9be026619732247f817ddf8944a399b6ee1d12ea0ab3`
- Format: RAR v5, successfully read using the system `libarchive.so.13`.
- Working extraction path: `/mnt/data/work/CrisisResponseOriginal/Crisis Response/`
- Result: 415 regular files, 204,344,165 bytes total (about 194.9 MiB). All regular files were extracted without reported errors. Paths were checked against absolute paths and parent-directory traversal.
- The original archive was not modified.

## Verified build facts

- Unity version string: `2017.2.0f3`, found in `Crisis Response_Data/globalgamemanagers`.
- Player: Windows standalone x86 Mono build (`Crisis Response.exe` plus `UnityPlayer.dll`).
- `Crisis Response.exe`: 641,024 bytes; SHA-256 `d4fec2d365c1a5c04fd385f51b815bf80cf39e0858fcaf497ae1a448a09b89f1`.
- `Assembly-CSharp.dll`: 1,504,768 bytes; SHA-256 `482f8bd6ef4546965b060463377470fc2da2db996de7ec16f29eb88f56a021b3`.
- ECMA-335 metadata inspection of `Assembly-CSharp.dll`: 1,534 TypeDef rows, 8,640 MethodDef rows, 9,025 Field rows, and 1,112 Property rows. The prior type/method counts are confirmed.
- `Assembly-CSharp-firstpass.dll` is present (118,784 bytes).
- Managed folder: 143 top-level files, including 56 DLLs, 44 MDB debug-symbol files, and 43 XML documentation files.
- Mono runtime folder: 12 files.
- Unity resources include `level0` through `level5`, `resources.assets`, `resources.assets.resS`, `sharedassets*.assets`, and `globalgamemanagers.assets`.
- The archive does not contain the original editable Unity project structure (`Assets/`, `ProjectSettings/`) or original `.cs` source files.

## Gameplay symbols found

This is a symbol-level inventory, **not a decompilation or behavioral validation**.

- **Characters and AI:** `CharacterBehavior`, `CharacterBody`, `CharacterPsychologyDriver`, `TeamController`, `TeamOperator`, `TeamActivity`, `PlayerController`, `CharacterAction`, `CharacterTemplate`.
- **Weapons and ballistics:** `Firearm`, `FirearmBarrel`, `FirearmBolt`, `WeaponBase`, `WeaponAimer`, `WeaponComponent`, `WeaponComponentMagazine`, `WeaponTaser`, `MarksmanRifle`, `ProjectileLimbEvent`, `Projectile`.
- **Doors and environment:** `DoorController`, `DoorTrap`, `DoorExplosive`, `DoorMechanismState`, `DoorInteraction`, `Obstacle`, `ExteriorController`, `LightSwitchController`.
- **Grenades and combat actions:** `Grenade`, `FragmentationGrenade`, `GrenadeSpoon`, `ReadyStunGrenade`, `ThrowStunThroughDoor`, `WaitForStunDetonation`.
- **Physiology and injury:** `Physiology`, `LimbPhysiology`, `LimbVitalFunction`, `VitalFunctions`, `Wound`, `WoundsConditionType`, `BloodStream`, `ApplyPhysiologicalEffectsToCharacter`.
- **Campaign, missions, and UI:** `CampaignManager`, `Campaign`, `Mission`, `MissionHelper`, `MissionMetaData`, `MissionUI`, `MainMenu`, `PauseMenu`, `UIMissionBriefing`, `UIMissionDebrief`, `UICampaign`, `UIWeaponSetupPanel`.
- **Input and persistence:** `InputHandler`, `InputMapping`, campaign serialization types, weapon serialization types, and team serialization types.

## Porting implications

1. The Windows executable and its Mono runtime cannot simply be embedded into a standard iPhone Safari web app.
2. The likely route is to recover Unity assets, reconstruct enough of a Unity project, decompile the managed assembly for analysis, then build and test with a compatible Unity Editor/WebGL toolchain.
3. Decompiled C# is reconstructed source, not the original source. Original comments, project layout, and some build-time settings may be unrecoverable.
4. Rust is not the default rewrite target. Preserve Unity/C# behavior wherever possible and use Rust only for a concrete, testable need.
5. Archive extraction and metadata inspection are complete. Full C# decompilation, Unity asset export, a WebGL build, and behavioral parity tests are **not yet complete**.
6. The current execution environment has no Unity Editor, .NET SDK/runtime, `monodis`, or ILSpy CLI, and cannot install packages from registries. Those missing tools block the next full reverse-engineering/build stage here.

## Integrity

The source archive hash above identifies the immutable original. A local per-file SHA-256 manifest was generated for all 415 extracted files; it remains in the working environment alongside the extracted copy. Binary game assets are deliberately not committed to this repository.
