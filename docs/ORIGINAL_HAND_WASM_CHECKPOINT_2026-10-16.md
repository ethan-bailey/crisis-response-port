# Original `Hand.SetMode` WebAssembly execution checkpoint

**2026-10-16 · issue #22 · `feature/22-limb-hand-source`.** This is one exact original developer-component execution milestone. It is **not** `IKArmSequencer` or `IKArmController` playback.

## Pinned original code and exact source boundary

Recovered privately from archive `crisis-response-recovery-direct-v3-1.zip`, SHA256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`:

| Original developer class | Source SHA256 | Bytes | Source/compile state |
| --- | --- | ---: | --- |
| `Hand.cs` | `d5e6b25be76a184dc6b117ae35127018b0cb65eccaa69db9f6d8922434e76537` | 799 | Unchanged original Git blob moved into `runtime/Original/Animation/Hand.cs`; included by native C# / WASM compiler |
| `ArmsControl.cs` | `3e623b5568950ff4efe6ad280df84e41566af31e4a03dba08c30d6a7ce34162c` | 903 | Original source preserved under `source-reference/Original/ArmDependencies/`, not compiled |
| `Limb.cs` | `4057e0b5dbb0fd4c7cab847f6168d655dc09fdabf0564587b403274210e6b8de` | 25,867 | Preserved, not compiled; real `BallisticMaterial`, physics, wound, physiology and renderer dependencies |
| `WeaponBase.cs` | `cc60b54d30225be4d0f770d4bdaaab1ebd32e805be98ba5704745c48ed1be979` | 41,257 | Preserved, not compiled |
| `TeamOperator.cs` | `cbc91bfe42d2d5c5c3562e20e16fbf60348af13309697016c3f3d2c9476e7ba7` | 9,212 | Preserved, not compiled |

The type `Joint` referenced by original `ArmIKSolver` is **`UnityEngine.Joint`**, not an `Assembly-CSharp/Joint.cs` developer file; source recovery initially detected this missing path and was corrected. Do not fabricate a developer `Joint` class. Unity built-in `Joint`/`HingeJoint` semantics are future engine-adapter prerequisites.

The newly compiled original `Hand.cs` contains `Hand.Mode.Trigger=0`, `Fist=1`, `ForwardAssist=2`. Its real `SetMode()` calls Unity GameObject activation, not any approximated gameplay. A source-bounded diagnostic `OriginalHandModes.Run` instantiates a separate test world, invokes the unchanged recovered `Hand.SetMode` code, and checks:
- **Fist:** fist=true, trigger=false, forwardAssist=false
- **ForwardAssist:** fist=false, trigger=false, forwardAssist=true
- **Trigger:** fist=false, trigger=true, forwardAssist=false
- Current original enum mode and isolation from current game clock/world.
  
This diagnostic runs via the `VerifyOriginalHandModes` C# browser bridge only in tests; it is **not connected to the live CharacterBody/Hand game meshes**, nor does it claim original IK arm-controller animation playback.

## Source and test provenance

- Recovered original developer source and archive hashes: [Actions #38907971876](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38907971876) **SUCCESS**, source reference artifact `12048798449`, preservation commit `4a1cbf9655589b3ec2f3852fefd19523a53512c5`.
- One original-only native+full .NET 8 WASM/PhysX/Chromium/WebKit milestone [Actions #38908722534](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38908722534) at test input `81b269dba69e79cbb325d9d92e5e45a1fd926e6b`. **VERIFIED SUCCESS**, 23 workflow steps. Native developer source/IK/hand contracts, .NET 8 WASM publish, PhysX + package SHA/unpack checks, and **Chromium + WebKit original `Hand.SetMode` execution passed**, each checking the 3 hand states and 28 existing source arm-sequence composition frames while explicitly asserting `playbackImplemented=false`. Private compiled browser site artifact `12049028233` (34.5MB), detailed evidence artifact `12048874054` (14/30 day retention per workflow). This proves the real original hand source method, not live character animation or full-game parity.
- `scripts/verify_original_arm_limb_sources.py` checks the five exact original source SHAs, real `Hand.cs` compiler path, separation of other classes, and no false controller-playback claim. `scripts/preserve_original_arm_limb_dependencies.py` can reproduce the source manifest against the pinned recovery ZIP without overwriting differing files.
- No Vercel auto deploy. The source `vercel.json` stays `git.deploymentEnabled=false`; any protected iPhone test URL requires a deliberate matching release.

## Next original source dependency

Real `IKArmSequencer` and `ArmIKSolver` require actual `CharacterBody`, `Limb`, `WeaponBase`/controller and `UnityEngine.Joint` physics; `ArmsControl.cs` also depends on original `ArmController` (a different class from `IKArmController`). Continue smallest dependency-ordered original code groups; do not convert this diagnostic into a fake in-game hand animation. [Issue #22](https://github.com/ethan-bailey/crisis-response-port/issues/22) remains open.

**Rollback:** selectively revert the `Hand.cs` exact source move and its diagnostic bridge/native/browser tests after checking downstream references; preserve the four original source reference blobs and provenance as historical immutable evidence. Avoid branch resets/forced merges and never publish proprietary source or assets.
