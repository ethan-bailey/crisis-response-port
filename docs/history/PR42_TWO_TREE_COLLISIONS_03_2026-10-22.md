# PR #42 collision list — part 3 of 4

Checkpoint: PR `a653609b62f3e3c79f73d0a17f0bf3dc89eae7b8`; develop `2fc0a8e93be97c4d99123910fad4bd25798e71a8`. PR side wins each collision.

### 101. `IKArmController` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `IKArmController`; partial `false`; `source-reference/Original/Animation/IKArmController.cs:3`
- class; namespace `(global)`; `IKArmController`; partial `false`; `source-reference/Original/CharacterDependencies/IKArmController.cs:3`

**develop declarations**:
- class; namespace `(global)`; `IKArmController`; partial `false`; `source-reference/Original/Animation/IKArmController.cs:3`
- class; namespace `(global)`; `IKArmController`; partial `false`; `source-reference/Original/CharacterExecution/IKArmController.cs:3`

Common member signatures: **46**.
- **PR wins** `field:applyGauze`; PR `public field IKArmSequence applyGauze` at `source-reference/Original/CharacterDependencies/IKArmController.cs:20`, `source-reference/Original/Animation/IKArmController.cs:20`; develop `public field IKArmSequence applyGauze` at `source-reference/Original/CharacterExecution/IKArmController.cs:20`, `source-reference/Original/Animation/IKArmController.cs:20`.
- **PR wins** `field:armStrength`; PR `public field AnimationCurve armStrength` at `source-reference/Original/CharacterDependencies/IKArmController.cs:5`, `source-reference/Original/Animation/IKArmController.cs:5`; develop `public field AnimationCurve armStrength` at `source-reference/Original/CharacterExecution/IKArmController.cs:5`, `source-reference/Original/Animation/IKArmController.cs:5`.
- **PR wins** `field:grenadeThrowTime`; PR `private field float grenadeThrowTime` at `source-reference/Original/CharacterDependencies/IKArmController.cs:24`, `source-reference/Original/Animation/IKArmController.cs:24`; develop `private field float grenadeThrowTime` at `source-reference/Original/CharacterExecution/IKArmController.cs:24`, `source-reference/Original/Animation/IKArmController.cs:24`.
- **PR wins** `field:holsterPistol`; PR `public field IKArmSequence holsterPistol` at `source-reference/Original/CharacterDependencies/IKArmController.cs:7`, `source-reference/Original/Animation/IKArmController.cs:7`; develop `public field IKArmSequence holsterPistol` at `source-reference/Original/CharacterExecution/IKArmController.cs:7`, `source-reference/Original/Animation/IKArmController.cs:7`.
- **PR wins** `field:lowerShoulderWeapon`; PR `public field IKArmSequence lowerShoulderWeapon` at `source-reference/Original/CharacterDependencies/IKArmController.cs:12`, `source-reference/Original/Animation/IKArmController.cs:12`; develop `public field IKArmSequence lowerShoulderWeapon` at `source-reference/Original/CharacterExecution/IKArmController.cs:12`, `source-reference/Original/Animation/IKArmController.cs:12`.
- **PR wins** `field:pickupGrenade`; PR `public field IKArmSequence pickupGrenade` at `source-reference/Original/CharacterDependencies/IKArmController.cs:16`, `source-reference/Original/Animation/IKArmController.cs:16`; develop `public field IKArmSequence pickupGrenade` at `source-reference/Original/CharacterExecution/IKArmController.cs:16`, `source-reference/Original/Animation/IKArmController.cs:16`.
- **PR wins** `field:returnGrenade`; PR `public field IKArmSequence returnGrenade` at `source-reference/Original/CharacterDependencies/IKArmController.cs:18`, `source-reference/Original/Animation/IKArmController.cs:18`; develop `public field IKArmSequence returnGrenade` at `source-reference/Original/CharacterExecution/IKArmController.cs:18`, `source-reference/Original/Animation/IKArmController.cs:18`.
- **PR wins** `field:returnShoulderWeapon`; PR `public field IKArmSequence returnShoulderWeapon` at `source-reference/Original/CharacterDependencies/IKArmController.cs:14`, `source-reference/Original/Animation/IKArmController.cs:14`; develop `public field IKArmSequence returnShoulderWeapon` at `source-reference/Original/CharacterExecution/IKArmController.cs:14`, `source-reference/Original/Animation/IKArmController.cs:14`.
- **PR wins** `field:self`; PR `private field CharacterBody self` at `source-reference/Original/CharacterDependencies/IKArmController.cs:22`, `source-reference/Original/Animation/IKArmController.cs:22`; develop `private field CharacterBody self` at `source-reference/Original/CharacterExecution/IKArmController.cs:22`, `source-reference/Original/Animation/IKArmController.cs:22`.
- **PR wins** `field:unholsterPistol`; PR `public field IKArmSequence unholsterPistol` at `source-reference/Original/CharacterDependencies/IKArmController.cs:10`, `source-reference/Original/Animation/IKArmController.cs:10`; develop `public field IKArmSequence unholsterPistol` at `source-reference/Original/CharacterExecution/IKArmController.cs:10`, `source-reference/Original/Animation/IKArmController.cs:10`.
- **PR wins** ``method:ActivateWeapon`0(WeaponBase)``; PR `public void ActivateWeapon(WeaponBase)` at `source-reference/Original/CharacterDependencies/IKArmController.cs:256`, `source-reference/Original/Animation/IKArmController.cs:256`; develop `public void ActivateWeapon(WeaponBase)` at `source-reference/Original/CharacterExecution/IKArmController.cs:250`, `source-reference/Original/Animation/IKArmController.cs:256`.
- **PR wins** ``method:Awake`0()``; PR `private void Awake()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:48`, `source-reference/Original/Animation/IKArmController.cs:48`; develop `private void Awake()` at `source-reference/Original/CharacterExecution/IKArmController.cs:42`, `source-reference/Original/Animation/IKArmController.cs:48`.
- **PR wins** ``method:EngageSelf`0()``; PR `public void EngageSelf()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:267`, `source-reference/Original/Animation/IKArmController.cs:267`; develop `public void EngageSelf()` at `source-reference/Original/CharacterExecution/IKArmController.cs:261`, `source-reference/Original/Animation/IKArmController.cs:267`.
- **PR wins** ``method:HolsterWeapon`0()``; PR `public void HolsterWeapon()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:160`, `source-reference/Original/Animation/IKArmController.cs:160`; develop `public void HolsterWeapon()` at `source-reference/Original/CharacterExecution/IKArmController.cs:154`, `source-reference/Original/Animation/IKArmController.cs:160`.
- **PR wins** ``method:IK_ActivatePendingWeapon`0()``; PR `public void IK_ActivatePendingWeapon()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:367`, `source-reference/Original/Animation/IKArmController.cs:367`; develop `public void IK_ActivatePendingWeapon()` at `source-reference/Original/CharacterExecution/IKArmController.cs:361`, `source-reference/Original/Animation/IKArmController.cs:367`.
- **PR wins** ``method:IK_ActivatePreviousWeapon`0()``; PR `public void IK_ActivatePreviousWeapon()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:362`, `source-reference/Original/Animation/IKArmController.cs:362`; develop `public void IK_ActivatePreviousWeapon()` at `source-reference/Original/CharacterExecution/IKArmController.cs:356`, `source-reference/Original/Animation/IKArmController.cs:362`.
- **PR wins** ``method:IK_ApplyGauze`0()``; PR `public void IK_ApplyGauze()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:401`, `source-reference/Original/Animation/IKArmController.cs:401`; develop `public void IK_ApplyGauze()` at `source-reference/Original/CharacterExecution/IKArmController.cs:395`, `source-reference/Original/Animation/IKArmController.cs:401`.
- **PR wins** ``method:IK_FinishGauze`0()``; PR `public void IK_FinishGauze()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:406`, `source-reference/Original/Animation/IKArmController.cs:406`; develop `public void IK_FinishGauze()` at `source-reference/Original/CharacterExecution/IKArmController.cs:400`, `source-reference/Original/Animation/IKArmController.cs:406`.
- **PR wins** ``method:IK_FireWeapon`0()``; PR `public void IK_FireWeapon()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:415`, `source-reference/Original/Animation/IKArmController.cs:415`; develop `public void IK_FireWeapon()` at `source-reference/Original/CharacterExecution/IKArmController.cs:409`, `source-reference/Original/Animation/IKArmController.cs:415`.
- **PR wins** ``method:IK_HolsterWeapon`0()``; PR `public void IK_HolsterWeapon()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:357`, `source-reference/Original/Animation/IKArmController.cs:357`; develop `public void IK_HolsterWeapon()` at `source-reference/Original/CharacterExecution/IKArmController.cs:351`, `source-reference/Original/Animation/IKArmController.cs:357`.
- **PR wins** ``method:IK_LowerWeapon`0()``; PR `public void IK_LowerWeapon()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:352`, `source-reference/Original/Animation/IKArmController.cs:352`; develop `public void IK_LowerWeapon()` at `source-reference/Original/CharacterExecution/IKArmController.cs:346`, `source-reference/Original/Animation/IKArmController.cs:352`.
- **PR wins** ``method:IK_MovePendingWeaponToHand`0()``; PR `public void IK_MovePendingWeaponToHand()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:372`, `source-reference/Original/Animation/IKArmController.cs:372`; develop `public void IK_MovePendingWeaponToHand()` at `source-reference/Original/CharacterExecution/IKArmController.cs:366`, `source-reference/Original/Animation/IKArmController.cs:372`.
- **PR wins** ``method:IK_PickupGauze`0()``; PR `public void IK_PickupGauze()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:392`, `source-reference/Original/Animation/IKArmController.cs:392`; develop `public void IK_PickupGauze()` at `source-reference/Original/CharacterExecution/IKArmController.cs:386`, `source-reference/Original/Animation/IKArmController.cs:392`.
- **PR wins** ``method:IK_PickupGrenade`0()``; PR `public void IK_PickupGrenade()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:382`, `source-reference/Original/Animation/IKArmController.cs:382`; develop `public void IK_PickupGrenade()` at `source-reference/Original/CharacterExecution/IKArmController.cs:376`, `source-reference/Original/Animation/IKArmController.cs:382`.
- **PR wins** ``method:IK_Restrain`0()``; PR `public void IK_Restrain()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:420`, `source-reference/Original/Animation/IKArmController.cs:420`; develop `public void IK_Restrain()` at `source-reference/Original/CharacterExecution/IKArmController.cs:414`, `source-reference/Original/Animation/IKArmController.cs:420`.
- **PR wins** ``method:IK_ReturnGrenade`0()``; PR `public void IK_ReturnGrenade()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:387`, `source-reference/Original/Animation/IKArmController.cs:387`; develop `public void IK_ReturnGrenade()` at `source-reference/Original/CharacterExecution/IKArmController.cs:381`, `source-reference/Original/Animation/IKArmController.cs:387`.
- **PR wins** ``method:InitiateGrenadeThrow`0()``; PR `public void InitiateGrenadeThrow()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:70`, `source-reference/Original/Animation/IKArmController.cs:70`; develop `public void InitiateGrenadeThrow()` at `source-reference/Original/CharacterExecution/IKArmController.cs:64`, `source-reference/Original/Animation/IKArmController.cs:70`.
- **PR wins** ``method:LowerCurrentWeapon`0()``; PR `public void LowerCurrentWeapon()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:232`, `source-reference/Original/Animation/IKArmController.cs:232`; develop `public void LowerCurrentWeapon()` at `source-reference/Original/CharacterExecution/IKArmController.cs:226`, `source-reference/Original/Animation/IKArmController.cs:232`.
- **PR wins** ``method:Play`0(IKArmSequenceRuntime)``; PR `public void Play(IKArmSequenceRuntime)` at `source-reference/Original/CharacterDependencies/IKArmController.cs:64`, `source-reference/Original/Animation/IKArmController.cs:64`; develop `public void Play(IKArmSequenceRuntime)` at `source-reference/Original/CharacterExecution/IKArmController.cs:58`, `source-reference/Original/Animation/IKArmController.cs:64`.
- **PR wins** ``method:ReadyGrenade`0(Grenade.Type)``; PR `public void ReadyGrenade(Grenade.Type)` at `source-reference/Original/CharacterDependencies/IKArmController.cs:201`, `source-reference/Original/Animation/IKArmController.cs:201`; develop `public void ReadyGrenade(Grenade.Type)` at `source-reference/Original/CharacterExecution/IKArmController.cs:195`, `source-reference/Original/Animation/IKArmController.cs:201`.
- **PR wins** ``method:Restrained`0()``; PR `public void Restrained()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:328`, `source-reference/Original/Animation/IKArmController.cs:328`; develop `public void Restrained()` at `source-reference/Original/CharacterExecution/IKArmController.cs:322`, `source-reference/Original/Animation/IKArmController.cs:328`.
- **PR wins** ``method:ResumePreviousWeapon`0()``; PR `public bool ResumePreviousWeapon()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:243`, `source-reference/Original/Animation/IKArmController.cs:243`; develop `public bool ResumePreviousWeapon()` at `source-reference/Original/CharacterExecution/IKArmController.cs:237`, `source-reference/Original/Animation/IKArmController.cs:243`.
- **PR wins** ``method:ReturnGrenade`0()``; PR `public void ReturnGrenade()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:214`, `source-reference/Original/Animation/IKArmController.cs:214`; develop `public void ReturnGrenade()` at `source-reference/Original/CharacterExecution/IKArmController.cs:208`, `source-reference/Original/Animation/IKArmController.cs:214`.
- **PR wins** ``method:Surrender`0()``; PR `public void Surrender()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:288`, `source-reference/Original/Animation/IKArmController.cs:288`; develop `public void Surrender()` at `source-reference/Original/CharacterExecution/IKArmController.cs:282`, `source-reference/Original/Animation/IKArmController.cs:288`.
- **PR wins** ``method:SwitchWeapons`0()``; PR `public void SwitchWeapons()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:101`, `source-reference/Original/Animation/IKArmController.cs:101`; develop `public void SwitchWeapons()` at `source-reference/Original/CharacterExecution/IKArmController.cs:95`, `source-reference/Original/Animation/IKArmController.cs:101`.
- **PR wins** ``method:Unarmed`0()``; PR `public void Unarmed()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:312`, `source-reference/Original/Animation/IKArmController.cs:312`; develop `public void Unarmed()` at `source-reference/Original/CharacterExecution/IKArmController.cs:306`, `source-reference/Original/Animation/IKArmController.cs:312`.
- **PR wins** ``method:UpdateHoldGrenade`0()``; PR `private void UpdateHoldGrenade()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:76`, `source-reference/Original/Animation/IKArmController.cs:76`; develop `private void UpdateHoldGrenade()` at `source-reference/Original/CharacterExecution/IKArmController.cs:70`, `source-reference/Original/Animation/IKArmController.cs:76`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `source-reference/Original/CharacterDependencies/IKArmController.cs:57`, `source-reference/Original/Animation/IKArmController.cs:57`; develop `private void Update()` at `source-reference/Original/CharacterExecution/IKArmController.cs:51`, `source-reference/Original/Animation/IKArmController.cs:57`.
- **PR wins** `property:IsPlaying`; PR `public bool IsPlaying` at `source-reference/Original/CharacterDependencies/IKArmController.cs:34`, `source-reference/Original/Animation/IKArmController.cs:34`; develop `public bool IsPlaying` at `source-reference/Original/CharacterExecution/IKArmController.cs:34`, `source-reference/Original/Animation/IKArmController.cs:34`.
- **PR wins** `property:IsThrowingGrenade`; PR `public bool IsThrowingGrenade` at `source-reference/Original/CharacterDependencies/IKArmController.cs:42`, `source-reference/Original/Animation/IKArmController.cs:42`; develop `public bool IsThrowingGrenade` at `source-reference/Original/CharacterExecution/IKArmController.cs:36`, `source-reference/Original/Animation/IKArmController.cs:42`.
- **PR wins** `property:LeftFrame`; PR `public int LeftFrame` at `source-reference/Original/CharacterDependencies/IKArmController.cs:46`, `source-reference/Original/Animation/IKArmController.cs:46`; develop `public int LeftFrame` at `source-reference/Original/CharacterExecution/IKArmController.cs:40`, `source-reference/Original/Animation/IKArmController.cs:46`.
- **PR wins** `property:LeftSequencer`; PR `public IKArmSequencer LeftSequencer` at `source-reference/Original/CharacterDependencies/IKArmController.cs:28`, `source-reference/Original/Animation/IKArmController.cs:28`; develop `public IKArmSequencer LeftSequencer` at `source-reference/Original/CharacterExecution/IKArmController.cs:28`, `source-reference/Original/Animation/IKArmController.cs:28`.
- **PR wins** `property:PendingWeapon`; PR `public WeaponBase PendingWeapon` at `source-reference/Original/CharacterDependencies/IKArmController.cs:32`, `source-reference/Original/Animation/IKArmController.cs:32`; develop `public WeaponBase PendingWeapon` at `source-reference/Original/CharacterExecution/IKArmController.cs:32`, `source-reference/Original/Animation/IKArmController.cs:32`.
- **PR wins** `property:PreviousWeapon`; PR `public WeaponBase PreviousWeapon` at `source-reference/Original/CharacterDependencies/IKArmController.cs:30`, `source-reference/Original/Animation/IKArmController.cs:30`; develop `public WeaponBase PreviousWeapon` at `source-reference/Original/CharacterExecution/IKArmController.cs:30`, `source-reference/Original/Animation/IKArmController.cs:30`.
- **PR wins** `property:RightFrame`; PR `public int RightFrame` at `source-reference/Original/CharacterDependencies/IKArmController.cs:44`, `source-reference/Original/Animation/IKArmController.cs:44`; develop `public int RightFrame` at `source-reference/Original/CharacterExecution/IKArmController.cs:38`, `source-reference/Original/Animation/IKArmController.cs:44`.
- **PR wins** `property:RightSequencer`; PR `public IKArmSequencer RightSequencer` at `source-reference/Original/CharacterDependencies/IKArmController.cs:26`, `source-reference/Original/Animation/IKArmController.cs:26`; develop `public IKArmSequencer RightSequencer` at `source-reference/Original/CharacterExecution/IKArmController.cs:26`, `source-reference/Original/Animation/IKArmController.cs:26`.


### 102. `IKArmSequence` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `IKArmSequence`; partial `false`; `runtime/Original/Animation/IKArmSequence.cs:4`

**develop declarations**:
- class; namespace `(global)`; `IKArmSequence`; partial `false`; `runtime/Original/Animation/IKArmSequence.cs:4`

Common member signatures: **4**.
- **PR wins** `field:left`; PR `public field List<IKArmAnimationFrame> left` at `runtime/Original/Animation/IKArmSequence.cs:8`; develop `public field List<IKArmAnimationFrame> left` at `runtime/Original/Animation/IKArmSequence.cs:8`.
- **PR wins** `field:right`; PR `public field List<IKArmAnimationFrame> right` at `runtime/Original/Animation/IKArmSequence.cs:6`; develop `public field List<IKArmAnimationFrame> right` at `runtime/Original/Animation/IKArmSequence.cs:6`.
- **PR wins** ``method:Sequence`0()``; PR `public IKArmSequenceRuntime Sequence()` at `runtime/Original/Animation/IKArmSequence.cs:10`; develop `public IKArmSequenceRuntime Sequence()` at `runtime/Original/Animation/IKArmSequence.cs:10`.
- **PR wins** ``method:Then`0(IKArmSequence)``; PR `public IKArmSequenceRuntime Then(IKArmSequence)` at `runtime/Original/Animation/IKArmSequence.cs:15`; develop `public IKArmSequenceRuntime Then(IKArmSequence)` at `runtime/Original/Animation/IKArmSequence.cs:15`.


### 103. `IKArmSequenceRuntime` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `IKArmSequenceRuntime`; partial `false`; `runtime/Original/Animation/IKArmSequenceRuntime.cs:3`

**develop declarations**:
- class; namespace `(global)`; `IKArmSequenceRuntime`; partial `false`; `runtime/Original/Animation/IKArmSequenceRuntime.cs:3`

Common member signatures: **6**.
- **PR wins** `constructor:IKArmSequenceRuntime()`; PR `public IKArmSequenceRuntime()` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:9`; develop `public IKArmSequenceRuntime()` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:9`.
- **PR wins** `constructor:IKArmSequenceRuntime(IKArmSequence)`; PR `public IKArmSequenceRuntime(IKArmSequence)` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:13`; develop `public IKArmSequenceRuntime(IKArmSequence)` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:13`.
- **PR wins** `field:left`; PR `public field List<IKArmAnimationFrame> left` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:7`; develop `public field List<IKArmAnimationFrame> left` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:7`.
- **PR wins** `field:right`; PR `public field List<IKArmAnimationFrame> right` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:5`; develop `public field List<IKArmAnimationFrame> right` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:5`.
- **PR wins** ``method:Append`0(IKArmSequence)``; PR `public IKArmSequenceRuntime Append(IKArmSequence)` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:26`; develop `public IKArmSequenceRuntime Append(IKArmSequence)` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:26`.
- **PR wins** ``method:Append`0(IKArmSequenceRuntime)``; PR `public IKArmSequenceRuntime Append(IKArmSequenceRuntime)` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:19`; develop `public IKArmSequenceRuntime Append(IKArmSequenceRuntime)` at `runtime/Original/Animation/IKArmSequenceRuntime.cs:19`.


### 104. `IKArmSequencer` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `IKArmSequencer`; partial `false`; `source-reference/Original/Animation/IKArmSequencer.cs:4`

**develop declarations**:
- class; namespace `(global)`; `IKArmSequencer`; partial `false`; `source-reference/Original/Animation/IKArmSequencer.cs:4`
- class; namespace `(global)`; `IKArmSequencer`; partial `false`; `source-reference/Original/CharacterExecution/IKArmSequencer.cs:4`

Common member signatures: **23**.
- **PR wins** `constructor:IKArmSequencer(IKArmController, CharacterBody, IKArmType, ArmIKSolver)`; PR `public IKArmSequencer(IKArmController, CharacterBody, IKArmType, ArmIKSolver)` at `source-reference/Original/Animation/IKArmSequencer.cs:48`; develop `public IKArmSequencer(IKArmController, CharacterBody, IKArmType, ArmIKSolver)` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:36`, `source-reference/Original/Animation/IKArmSequencer.cs:48`.
- **PR wins** `field:armType`; PR `private field IKArmType armType` at `source-reference/Original/Animation/IKArmSequencer.cs:12`; develop `private field IKArmType armType` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:12`, `source-reference/Original/Animation/IKArmSequencer.cs:12`.
- **PR wins** `field:callback`; PR `private field IKControllerInvoke callback` at `source-reference/Original/Animation/IKArmSequencer.cs:16`; develop `private field IKControllerInvoke callback` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:16`, `source-reference/Original/Animation/IKArmSequencer.cs:16`.
- **PR wins** `field:controller`; PR `private field IKArmController controller` at `source-reference/Original/Animation/IKArmSequencer.cs:6`; develop `private field IKArmController controller` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:6`, `source-reference/Original/Animation/IKArmSequencer.cs:6`.
- **PR wins** `field:currentAction`; PR `private field IKArmAction currentAction` at `source-reference/Original/Animation/IKArmSequencer.cs:8`; develop `private field IKArmAction currentAction` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:8`, `source-reference/Original/Animation/IKArmSequencer.cs:8`.
- **PR wins** `field:frameDone`; PR `private field bool frameDone` at `source-reference/Original/Animation/IKArmSequencer.cs:20`; develop `private field bool frameDone` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:20`, `source-reference/Original/Animation/IKArmSequencer.cs:20`.
- **PR wins** `field:frameTime`; PR `private field float frameTime` at `source-reference/Original/Animation/IKArmSequencer.cs:24`; develop `private field float frameTime` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:24`, `source-reference/Original/Animation/IKArmSequencer.cs:24`.
- **PR wins** `field:self`; PR `private field CharacterBody self` at `source-reference/Original/Animation/IKArmSequencer.cs:14`; develop `private field CharacterBody self` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:14`, `source-reference/Original/Animation/IKArmSequencer.cs:14`.
- **PR wins** `field:sibling`; PR `private field IKArmSequencer sibling` at `source-reference/Original/Animation/IKArmSequencer.cs:18`; develop `private field IKArmSequencer sibling` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:18`, `source-reference/Original/Animation/IKArmSequencer.cs:18`.
- **PR wins** `field:solver`; PR `private field ArmIKSolver solver` at `source-reference/Original/Animation/IKArmSequencer.cs:10`; develop `private field ArmIKSolver solver` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:10`, `source-reference/Original/Animation/IKArmSequencer.cs:10`.
- **PR wins** `field:timestamp`; PR `private field float timestamp` at `source-reference/Original/Animation/IKArmSequencer.cs:22`; develop `private field float timestamp` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:22`, `source-reference/Original/Animation/IKArmSequencer.cs:22`.
- **PR wins** `field:waitForSiblingFrame`; PR `private field int waitForSiblingFrame` at `source-reference/Original/Animation/IKArmSequencer.cs:26`; develop `private field int waitForSiblingFrame` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:26`, `source-reference/Original/Animation/IKArmSequencer.cs:26`.
- **PR wins** ``method:HasFinishedFrame`0(int)``; PR `public bool HasFinishedFrame(int)` at `source-reference/Original/Animation/IKArmSequencer.cs:229`; develop `public bool HasFinishedFrame(int)` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:217`, `source-reference/Original/Animation/IKArmSequencer.cs:229`.
- **PR wins** ``method:Invoke`0(IKControllerInvoke)``; PR `private void Invoke(IKControllerInvoke)` at `source-reference/Original/Animation/IKArmSequencer.cs:186`; develop `private void Invoke(IKControllerInvoke)` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:174`, `source-reference/Original/Animation/IKArmSequencer.cs:186`.
- **PR wins** ``method:NewFrame`0(int)``; PR `private void NewFrame(int)` at `source-reference/Original/Animation/IKArmSequencer.cs:101`; develop `private void NewFrame(int)` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:89`, `source-reference/Original/Animation/IKArmSequencer.cs:101`.
- **PR wins** ``method:Play`0(List<IKArmAnimationFrame>, int)``; PR `public void Play(List<IKArmAnimationFrame>, int)` at `source-reference/Original/Animation/IKArmSequencer.cs:94`; develop `public void Play(List<IKArmAnimationFrame>, int)` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:82`, `source-reference/Original/Animation/IKArmSequencer.cs:94`.
- **PR wins** ``method:SetSibling`0(IKArmSequencer)``; PR `public void SetSibling(IKArmSequencer)` at `source-reference/Original/Animation/IKArmSequencer.cs:56`; develop `public void SetSibling(IKArmSequencer)` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:44`, `source-reference/Original/Animation/IKArmSequencer.cs:56`.
- **PR wins** ``method:TargetTransform`0(IKArmTarget, IKArmType, out Vector3)``; PR `private Transform TargetTransform(IKArmTarget, IKArmType, out Vector3)` at `source-reference/Original/Animation/IKArmSequencer.cs:136`; develop `private Transform TargetTransform(IKArmTarget, IKArmType, out Vector3)` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:124`, `source-reference/Original/Animation/IKArmSequencer.cs:136`.
- **PR wins** ``method:Update`0()``; PR `public void Update()` at `source-reference/Original/Animation/IKArmSequencer.cs:61`; develop `public void Update()` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:49`, `source-reference/Original/Animation/IKArmSequencer.cs:61`.
- **PR wins** `property:CurrentFrame`; PR `public int CurrentFrame` at `source-reference/Original/Animation/IKArmSequencer.cs:46`; develop `public int CurrentFrame` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:34`, `source-reference/Original/Animation/IKArmSequencer.cs:46`.
- **PR wins** `property:IsPlaying`; PR `public bool IsPlaying` at `source-reference/Original/Animation/IKArmSequencer.cs:30`; develop `public bool IsPlaying` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:30`, `source-reference/Original/Animation/IKArmSequencer.cs:30`.
- **PR wins** `property:IsReady`; PR `public bool IsReady` at `source-reference/Original/Animation/IKArmSequencer.cs:38`; develop `public bool IsReady` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:32`, `source-reference/Original/Animation/IKArmSequencer.cs:38`.
- **PR wins** `property:Sequence`; PR `public List<IKArmAnimationFrame> Sequence` at `source-reference/Original/Animation/IKArmSequencer.cs:28`; develop `public List<IKArmAnimationFrame> Sequence` at `source-reference/Original/CharacterExecution/IKArmSequencer.cs:28`, `source-reference/Original/Animation/IKArmSequencer.cs:28`.


### 105. `IKArmTarget` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `IKArmTarget`; partial `false`; `runtime/Original/Animation/IKArmTarget.cs:1`

**develop declarations**:
- enum; namespace `(global)`; `IKArmTarget`; partial `false`; `runtime/Original/Animation/IKArmTarget.cs:1`

Common member signatures: **0**.


### 106. `IKArmType` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `IKArmType`; partial `false`; `runtime/Original/Animation/IKArmType.cs:1`

**develop declarations**:
- enum; namespace `(global)`; `IKArmType`; partial `false`; `runtime/Original/Animation/IKArmType.cs:1`

Common member signatures: **0**.


### 107. `IKControllerInvoke` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `IKControllerInvoke`; partial `false`; `runtime/Original/Animation/IKControllerInvoke.cs:1`

**develop declarations**:
- enum; namespace `(global)`; `IKControllerInvoke`; partial `false`; `runtime/Original/Animation/IKControllerInvoke.cs:1`

Common member signatures: **0**.


### 108. `InputHandler` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `InputHandler`; partial `false`; `runtime/Original/InputHandler.cs:3`

**develop declarations**:
- class; namespace `(global)`; `InputHandler`; partial `false`; `runtime/Original/InputHandler.cs:3`

Common member signatures: **15**.
- **PR wins** `field:DefaultLongPress`; PR `public static field float DefaultLongPress` at `runtime/Original/InputHandler.cs:4`; develop `public static field float DefaultLongPress` at `runtime/Original/InputHandler.cs:4`.
- **PR wins** `field:inputSuspended`; PR `private field bool inputSuspended` at `runtime/Original/InputHandler.cs:5`; develop `private field bool inputSuspended` at `runtime/Original/InputHandler.cs:5`.
- **PR wins** ``method:DisableAll`0()``; PR `public void DisableAll()` at `runtime/Original/InputHandler.cs:87`; develop `public void DisableAll()` at `runtime/Original/InputHandler.cs:87`.
- **PR wins** ``method:DisableOperatorInput`0(bool)``; PR `public void DisableOperatorInput(bool)` at `runtime/Original/InputHandler.cs:101`; develop `public void DisableOperatorInput(bool)` at `runtime/Original/InputHandler.cs:101`.
- **PR wins** ``method:EnableAll`0()``; PR `public void EnableAll()` at `runtime/Original/InputHandler.cs:80`; develop `public void EnableAll()` at `runtime/Original/InputHandler.cs:80`.
- **PR wins** ``method:EnableCategory`0(InputKey.Category, bool)``; PR `public void EnableCategory(InputKey.Category, bool)` at `runtime/Original/InputHandler.cs:121`; develop `public void EnableCategory(InputKey.Category, bool)` at `runtime/Original/InputHandler.cs:121`.
- **PR wins** ``method:FindAxisBindings`0(string, float)``; PR `public List<InputKey> FindAxisBindings(string, float)` at `runtime/Original/InputHandler.cs:64`; develop `public List<InputKey> FindAxisBindings(string, float)` at `runtime/Original/InputHandler.cs:64`.
- **PR wins** ``method:FindKeyCodeBindings`0(KeyCode)``; PR `public List<InputKey> FindKeyCodeBindings(KeyCode)` at `runtime/Original/InputHandler.cs:48`; develop `public List<InputKey> FindKeyCodeBindings(KeyCode)` at `runtime/Original/InputHandler.cs:48`.
- **PR wins** ``method:OnMissionEnd`0()``; PR `public void OnMissionEnd()` at `runtime/Original/InputHandler.cs:98`; develop `public void OnMissionEnd()` at `runtime/Original/InputHandler.cs:98`.
- **PR wins** ``method:OnMissionStart`0()``; PR `public void OnMissionStart()` at `runtime/Original/InputHandler.cs:94`; develop `public void OnMissionStart()` at `runtime/Original/InputHandler.cs:94`.
- **PR wins** ``method:ResetAll`0()``; PR `public void ResetAll()` at `runtime/Original/InputHandler.cs:114`; develop `public void ResetAll()` at `runtime/Original/InputHandler.cs:114`.
- **PR wins** ``method:SetMapping`0(InputMapping)``; PR `public void SetMapping(InputMapping)` at `runtime/Original/InputHandler.cs:7`; develop `public void SetMapping(InputMapping)` at `runtime/Original/InputHandler.cs:7`.
- **PR wins** ``method:SuspendInput`0(bool, bool)``; PR `public void SuspendInput(bool, bool)` at `runtime/Original/InputHandler.cs:106`; develop `public void SuspendInput(bool, bool)` at `runtime/Original/InputHandler.cs:106`.
- **PR wins** ``method:UpdateKeys`0()``; PR `private void UpdateKeys()` at `runtime/Original/InputHandler.cs:11`; develop `private void UpdateKeys()` at `runtime/Original/InputHandler.cs:11`.
- **PR wins** `property:Keys`; PR `public InputMapping Keys` at `runtime/Original/InputHandler.cs:6`; develop `public InputMapping Keys` at `runtime/Original/InputHandler.cs:6`.


### 109. `InputKey` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `InputKey`; partial `false`; `runtime/Original/InputKey.cs:4`

**develop declarations**:
- class; namespace `(global)`; `InputKey`; partial `false`; `runtime/Original/InputKey.cs:4`

Common member signatures: **34**.
- **PR wins** `constructor:InputKey()`; PR `public InputKey()` at `runtime/Original/InputKey.cs:188`; develop `public InputKey()` at `runtime/Original/InputKey.cs:188`.
- **PR wins** `field:axis`; PR `public field string axis` at `runtime/Original/InputKey.cs:20`; develop `public field string axis` at `runtime/Original/InputKey.cs:20`.
- **PR wins** `field:axisDirection`; PR `public field float axisDirection` at `runtime/Original/InputKey.cs:22`; develop `public field float axisDirection` at `runtime/Original/InputKey.cs:22`.
- **PR wins** `field:axisValue`; PR `public field float axisValue` at `runtime/Original/InputKey.cs:30`; develop `public field float axisValue` at `runtime/Original/InputKey.cs:30`.
- **PR wins** `field:category`; PR `public field Category category` at `runtime/Original/InputKey.cs:34`; develop `public field Category category` at `runtime/Original/InputKey.cs:34`.
- **PR wins** `field:description`; PR `public field string description` at `runtime/Original/InputKey.cs:28`; develop `public field string description` at `runtime/Original/InputKey.cs:28`.
- **PR wins** `field:down`; PR `private field bool down` at `runtime/Original/InputKey.cs:36`; develop `private field bool down` at `runtime/Original/InputKey.cs:36`.
- **PR wins** `field:downLong`; PR `private field bool downLong` at `runtime/Original/InputKey.cs:38`; develop `private field bool downLong` at `runtime/Original/InputKey.cs:38`.
- **PR wins** `field:enabled`; PR `private field bool enabled` at `runtime/Original/InputKey.cs:46`; develop `private field bool enabled` at `runtime/Original/InputKey.cs:46`.
- **PR wins** `field:key`; PR `public field KeyCode key` at `runtime/Original/InputKey.cs:18`; develop `public field KeyCode key` at `runtime/Original/InputKey.cs:18`.
- **PR wins** `field:longEnabled`; PR `private field bool longEnabled` at `runtime/Original/InputKey.cs:48`; develop `private field bool longEnabled` at `runtime/Original/InputKey.cs:48`.
- **PR wins** `field:longPress`; PR `public field float longPress` at `runtime/Original/InputKey.cs:32`; develop `public field float longPress` at `runtime/Original/InputKey.cs:32`.
- **PR wins** `field:name`; PR `public field string name` at `runtime/Original/InputKey.cs:26`; develop `public field string name` at `runtime/Original/InputKey.cs:26`.
- **PR wins** `field:universal`; PR `public field bool universal` at `runtime/Original/InputKey.cs:24`; develop `public field bool universal` at `runtime/Original/InputKey.cs:24`.
- **PR wins** `field:up`; PR `private field bool up` at `runtime/Original/InputKey.cs:40`; develop `private field bool up` at `runtime/Original/InputKey.cs:40`.
- **PR wins** `field:upLong`; PR `private field bool upLong` at `runtime/Original/InputKey.cs:44`; develop `private field bool upLong` at `runtime/Original/InputKey.cs:44`.
- **PR wins** `field:upShort`; PR `private field bool upShort` at `runtime/Original/InputKey.cs:42`; develop `private field bool upShort` at `runtime/Original/InputKey.cs:42`.
- **PR wins** ``method:Reset`0()``; PR `public void Reset()` at `runtime/Original/InputKey.cs:194`; develop `public void Reset()` at `runtime/Original/InputKey.cs:194`.
- **PR wins** `property:Down`; PR `public bool Down` at `runtime/Original/InputKey.cs:54`; develop `public bool Down` at `runtime/Original/InputKey.cs:54`.
- **PR wins** `property:DownHandled`; PR `public bool DownHandled` at `runtime/Original/InputKey.cs:114`; develop `public bool DownHandled` at `runtime/Original/InputKey.cs:114`.
- **PR wins** `property:DownLong`; PR `public bool DownLong` at `runtime/Original/InputKey.cs:66`; develop `public bool DownLong` at `runtime/Original/InputKey.cs:66`.
- **PR wins** `property:Enabled`; PR `public bool Enabled` at `runtime/Original/InputKey.cs:144`; develop `public bool Enabled` at `runtime/Original/InputKey.cs:144`.
- **PR wins** `property:IsAxis`; PR `public bool IsAxis` at `runtime/Original/InputKey.cs:136`; develop `public bool IsAxis` at `runtime/Original/InputKey.cs:136`.
- **PR wins** `property:IsKey`; PR `public bool IsKey` at `runtime/Original/InputKey.cs:128`; develop `public bool IsKey` at `runtime/Original/InputKey.cs:128`.
- **PR wins** `property:KeyName`; PR `public string KeyName` at `runtime/Original/InputKey.cs:172`; develop `public string KeyName` at `runtime/Original/InputKey.cs:172`.
- **PR wins** `property:LongEnabled`; PR `public bool LongEnabled` at `runtime/Original/InputKey.cs:160`; develop `public bool LongEnabled` at `runtime/Original/InputKey.cs:160`.
- **PR wins** `property:Pressed`; PR `public bool Pressed` at `runtime/Original/InputKey.cs:50`; develop `public bool Pressed` at `runtime/Original/InputKey.cs:50`.
- **PR wins** `property:PressedLong`; PR `public bool PressedLong` at `runtime/Original/InputKey.cs:52`; develop `public bool PressedLong` at `runtime/Original/InputKey.cs:52`.
- **PR wins** `property:TimePressed`; PR `public float TimePressed` at `runtime/Original/InputKey.cs:120`; develop `public float TimePressed` at `runtime/Original/InputKey.cs:120`.
- **PR wins** `property:Timestamp`; PR `public float Timestamp` at `runtime/Original/InputKey.cs:118`; develop `public float Timestamp` at `runtime/Original/InputKey.cs:118`.
- **PR wins** `property:Up`; PR `public bool Up` at `runtime/Original/InputKey.cs:78`; develop `public bool Up` at `runtime/Original/InputKey.cs:78`.
- **PR wins** `property:UpHandled`; PR `public bool UpHandled` at `runtime/Original/InputKey.cs:116`; develop `public bool UpHandled` at `runtime/Original/InputKey.cs:116`.
- **PR wins** `property:UpLong`; PR `public bool UpLong` at `runtime/Original/InputKey.cs:102`; develop `public bool UpLong` at `runtime/Original/InputKey.cs:102`.
- **PR wins** `property:UpShort`; PR `public bool UpShort` at `runtime/Original/InputKey.cs:90`; develop `public bool UpShort` at `runtime/Original/InputKey.cs:90`.


### 110. `InputKey+Category` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `Category`; partial `false`; `runtime/Original/InputKey.cs:6`

**develop declarations**:
- enum; namespace `(global)`; `Category`; partial `false`; `runtime/Original/InputKey.cs:6`

Common member signatures: **0**.


### 111. `InputMapping` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `InputMapping`; partial `false`; `runtime/Original/InputMapping.cs:3`

**develop declarations**:
- class; namespace `(global)`; `InputMapping`; partial `false`; `runtime/Original/InputMapping.cs:3`

Common member signatures: **48**.
- **PR wins** `constructor:InputMapping()`; PR `public InputMapping()` at `runtime/Original/InputMapping.cs:352`; develop `public InputMapping()` at `runtime/Original/InputMapping.cs:352`.
- **PR wins** `field:Filename`; PR `public static field string Filename` at `runtime/Original/InputMapping.cs:6`; develop `public static field string Filename` at `runtime/Original/InputMapping.cs:6`.
- **PR wins** `field:bolt`; PR `public field InputKey bolt` at `runtime/Original/InputMapping.cs:147`; develop `public field InputKey bolt` at `runtime/Original/InputMapping.cs:147`.
- **PR wins** `field:bulletTime`; PR `public field InputKey bulletTime` at `runtime/Original/InputMapping.cs:291`; develop `public field InputKey bulletTime` at `runtime/Original/InputMapping.cs:291`.
- **PR wins** `field:categories`; PR `private field Dictionary<InputKey.Category, List<InputKey>> categories` at `runtime/Original/InputMapping.cs:340`; develop `private field Dictionary<InputKey.Category, List<InputKey>> categories` at `runtime/Original/InputMapping.cs:340`.
- **PR wins** `field:console`; PR `public field InputKey console` at `runtime/Original/InputMapping.cs:331`; develop `public field InputKey console` at `runtime/Original/InputMapping.cs:331`.
- **PR wins** `field:decreaseSlowMotion`; PR `public field InputKey decreaseSlowMotion` at `runtime/Original/InputMapping.cs:307`; develop `public field InputKey decreaseSlowMotion` at `runtime/Original/InputMapping.cs:307`.
- **PR wins** `field:doorGradual`; PR `public field InputKey doorGradual` at `runtime/Original/InputMapping.cs:82`; develop `public field InputKey doorGradual` at `runtime/Original/InputMapping.cs:82`.
- **PR wins** `field:doorPull`; PR `public field InputKey doorPull` at `runtime/Original/InputMapping.cs:73`; develop `public field InputKey doorPull` at `runtime/Original/InputMapping.cs:73`.
- **PR wins** `field:doorPush`; PR `public field InputKey doorPush` at `runtime/Original/InputMapping.cs:64`; develop `public field InputKey doorPush` at `runtime/Original/InputMapping.cs:64`.
- **PR wins** `field:dropWeapon`; PR `public field InputKey dropWeapon` at `runtime/Original/InputMapping.cs:122`; develop `public field InputKey dropWeapon` at `runtime/Original/InputMapping.cs:122`.
- **PR wins** `field:entryTeam`; PR `public field InputKey entryTeam` at `runtime/Original/InputMapping.cs:187`; develop `public field InputKey entryTeam` at `runtime/Original/InputMapping.cs:187`.
- **PR wins** `field:fire`; PR `public field InputKey fire` at `runtime/Original/InputMapping.cs:90`; develop `public field InputKey fire` at `runtime/Original/InputMapping.cs:90`.
- **PR wins** `field:fireMode`; PR `public field InputKey fireMode` at `runtime/Original/InputMapping.cs:130`; develop `public field InputKey fireMode` at `runtime/Original/InputMapping.cs:130`.
- **PR wins** `field:flashlight`; PR `public field InputKey flashlight` at `runtime/Original/InputMapping.cs:155`; develop `public field InputKey flashlight` at `runtime/Original/InputMapping.cs:155`.
- **PR wins** `field:fragGrenade`; PR `public field InputKey fragGrenade` at `runtime/Original/InputMapping.cs:179`; develop `public field InputKey fragGrenade` at `runtime/Original/InputMapping.cs:179`.
- **PR wins** `field:freeAim`; PR `public field InputKey freeAim` at `runtime/Original/InputMapping.cs:106`; develop `public field InputKey freeAim` at `runtime/Original/InputMapping.cs:106`.
- **PR wins** `field:increaseSlowMotion`; PR `public field InputKey increaseSlowMotion` at `runtime/Original/InputMapping.cs:315`; develop `public field InputKey increaseSlowMotion` at `runtime/Original/InputMapping.cs:315`.
- **PR wins** `field:interact`; PR `public field InputKey interact` at `runtime/Original/InputMapping.cs:48`; develop `public field InputKey interact` at `runtime/Original/InputMapping.cs:48`.
- **PR wins** `field:kneel`; PR `public field InputKey kneel` at `runtime/Original/InputMapping.cs:32`; develop `public field InputKey kneel` at `runtime/Original/InputMapping.cs:32`.
- **PR wins** `field:laser`; PR `public field InputKey laser` at `runtime/Original/InputMapping.cs:163`; develop `public field InputKey laser` at `runtime/Original/InputMapping.cs:163`.
- **PR wins** `field:left`; PR `public field InputKey left` at `runtime/Original/InputMapping.cs:8`; develop `public field InputKey left` at `runtime/Original/InputMapping.cs:8`.
- **PR wins** `field:magazine`; PR `public field InputKey magazine` at `runtime/Original/InputMapping.cs:139`; develop `public field InputKey magazine` at `runtime/Original/InputMapping.cs:139`.
- **PR wins** `field:marksman`; PR `public field InputKey marksman` at `runtime/Original/InputMapping.cs:195`; develop `public field InputKey marksman` at `runtime/Original/InputMapping.cs:195`.
- **PR wins** `field:marksmanEngage`; PR `public field InputKey marksmanEngage` at `runtime/Original/InputMapping.cs:275`; develop `public field InputKey marksmanEngage` at `runtime/Original/InputMapping.cs:275`.
- **PR wins** `field:marksmanFree`; PR `public field InputKey marksmanFree` at `runtime/Original/InputMapping.cs:283`; develop `public field InputKey marksmanFree` at `runtime/Original/InputMapping.cs:283`.
- **PR wins** `field:marksmanHold`; PR `public field InputKey marksmanHold` at `runtime/Original/InputMapping.cs:267`; develop `public field InputKey marksmanHold` at `runtime/Original/InputMapping.cs:267`.
- **PR wins** `field:objectives`; PR `public field InputKey objectives` at `runtime/Original/InputMapping.cs:323`; develop `public field InputKey objectives` at `runtime/Original/InputMapping.cs:323`.
- **PR wins** `field:precision`; PR `public field InputKey precision` at `runtime/Original/InputMapping.cs:98`; develop `public field InputKey precision` at `runtime/Original/InputMapping.cs:98`.
- **PR wins** `field:right`; PR `public field InputKey right` at `runtime/Original/InputMapping.cs:16`; develop `public field InputKey right` at `runtime/Original/InputMapping.cs:16`.
- **PR wins** `field:slowMotion`; PR `public field InputKey slowMotion` at `runtime/Original/InputMapping.cs:299`; develop `public field InputKey slowMotion` at `runtime/Original/InputMapping.cs:299`.
- **PR wins** `field:sprint`; PR `public field InputKey sprint` at `runtime/Original/InputMapping.cs:40`; develop `public field InputKey sprint` at `runtime/Original/InputMapping.cs:40`.
- **PR wins** `field:stand`; PR `public field InputKey stand` at `runtime/Original/InputMapping.cs:24`; develop `public field InputKey stand` at `runtime/Original/InputMapping.cs:24`.
- **PR wins** `field:stunGrenade`; PR `public field InputKey stunGrenade` at `runtime/Original/InputMapping.cs:171`; develop `public field InputKey stunGrenade` at `runtime/Original/InputMapping.cs:171`.
- **PR wins** `field:switchWeapons`; PR `public field InputKey switchWeapons` at `runtime/Original/InputMapping.cs:114`; develop `public field InputKey switchWeapons` at `runtime/Original/InputMapping.cs:114`.
- **PR wins** `field:teamClearNext`; PR `public field InputKey teamClearNext` at `runtime/Original/InputMapping.cs:211`; develop `public field InputKey teamClearNext` at `runtime/Original/InputMapping.cs:211`.
- **PR wins** `field:teamDecreaseTempo`; PR `public field InputKey teamDecreaseTempo` at `runtime/Original/InputMapping.cs:251`; develop `public field InputKey teamDecreaseTempo` at `runtime/Original/InputMapping.cs:251`.
- **PR wins** `field:teamFollow`; PR `public field InputKey teamFollow` at `runtime/Original/InputMapping.cs:235`; develop `public field InputKey teamFollow` at `runtime/Original/InputMapping.cs:235`.
- **PR wins** `field:teamHold`; PR `public field InputKey teamHold` at `runtime/Original/InputMapping.cs:227`; develop `public field InputKey teamHold` at `runtime/Original/InputMapping.cs:227`.
- **PR wins** `field:teamIncreaseTempo`; PR `public field InputKey teamIncreaseTempo` at `runtime/Original/InputMapping.cs:243`; develop `public field InputKey teamIncreaseTempo` at `runtime/Original/InputMapping.cs:243`.
- **PR wins** `field:teamMoveOut`; PR `public field InputKey teamMoveOut` at `runtime/Original/InputMapping.cs:219`; develop `public field InputKey teamMoveOut` at `runtime/Original/InputMapping.cs:219`.
- **PR wins** `field:teamMoveTo`; PR `public field InputKey teamMoveTo` at `runtime/Original/InputMapping.cs:203`; develop `public field InputKey teamMoveTo` at `runtime/Original/InputMapping.cs:203`.
- **PR wins** `field:teamToggleStun`; PR `public field InputKey teamToggleStun` at `runtime/Original/InputMapping.cs:259`; develop `public field InputKey teamToggleStun` at `runtime/Original/InputMapping.cs:259`.
- **PR wins** `field:vocalize`; PR `public field InputKey vocalize` at `runtime/Original/InputMapping.cs:56`; develop `public field InputKey vocalize` at `runtime/Original/InputMapping.cs:56`.
- **PR wins** ``method:Overwrite`0(InputMapping)``; PR `private void Overwrite(InputMapping)` at `runtime/Original/InputMapping.cs:376`; develop `private void Overwrite(InputMapping)` at `runtime/Original/InputMapping.cs:376`.
- **PR wins** ``method:RegisterAllKeys`0()``; PR `private void RegisterAllKeys()` at `runtime/Original/InputMapping.cs:356`; develop `private void RegisterAllKeys()` at `runtime/Original/InputMapping.cs:356`.
- **PR wins** `property:All`; PR `public List<InputKey> All` at `runtime/Original/InputMapping.cs:342`; develop `public List<InputKey> All` at `runtime/Original/InputMapping.cs:342`.
- **PR wins** `property:Categories`; PR `public Dictionary<InputKey.Category, List<InputKey>> Categories` at `runtime/Original/InputMapping.cs:344`; develop `public Dictionary<InputKey.Category, List<InputKey>> Categories` at `runtime/Original/InputMapping.cs:344`.


### 112. `InverseKinematics2D` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `InverseKinematics2D`; partial `false`; `runtime/Original/InverseKinematics2D.cs:3`

**develop declarations**:
- class; namespace `(global)`; `InverseKinematics2D`; partial `false`; `runtime/Original/InverseKinematics2D.cs:3`

Common member signatures: **2**.
- **PR wins** `field:epsilon`; PR `private static field float epsilon` at `runtime/Original/InverseKinematics2D.cs:5`; develop `private static field float epsilon` at `runtime/Original/InverseKinematics2D.cs:5`.
- **PR wins** ``method:SolveTwoBoneAnalytic`0(out float, out float, bool, float, float, float, float)``; PR `public static bool SolveTwoBoneAnalytic(out float, out float, bool, float, float, float, float)` at `runtime/Original/InverseKinematics2D.cs:7`; develop `public static bool SolveTwoBoneAnalytic(out float, out float, bool, float, float, float, float)` at `runtime/Original/InverseKinematics2D.cs:7`.


### 113. `Limb` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `Limb`; partial `false`; `source-reference/Original/ArmDependencies/Limb.cs:4`
- class; namespace `(global)`; `Limb`; partial `false`; `source-reference/Original/CharacterDependencies/Limb.cs:4`

**develop declarations**:
- class; namespace `(global)`; `Limb`; partial `false`; `source-reference/Original/ArmDependencies/Limb.cs:4`
- class; namespace `(global)`; `Limb`; partial `false`; `source-reference/Original/CharacterExecution/Limb.cs:4`

Common member signatures: **83**.
- **PR wins** `field:accumulatedExitVolume`; PR `private field float accumulatedExitVolume` at `source-reference/Original/CharacterDependencies/Limb.cs:58`, `source-reference/Original/ArmDependencies/Limb.cs:58`; develop `private field float accumulatedExitVolume` at `source-reference/Original/CharacterExecution/Limb.cs:58`, `source-reference/Original/ArmDependencies/Limb.cs:58`.
- **PR wins** `field:aimpoint`; PR `public field Vector2 aimpoint` at `source-reference/Original/CharacterDependencies/Limb.cs:21`, `source-reference/Original/ArmDependencies/Limb.cs:21`; develop `public field Vector2 aimpoint` at `source-reference/Original/CharacterExecution/Limb.cs:21`, `source-reference/Original/ArmDependencies/Limb.cs:21`.
- **PR wins** `field:attachments`; PR `public field List<LimbAttachment> attachments` at `source-reference/Original/CharacterDependencies/Limb.cs:19`, `source-reference/Original/ArmDependencies/Limb.cs:19`; develop `public field List<LimbAttachment> attachments` at `source-reference/Original/CharacterExecution/Limb.cs:19`, `source-reference/Original/ArmDependencies/Limb.cs:19`.
- **PR wins** `field:bleedPoints`; PR `public field List<Vector2> bleedPoints` at `source-reference/Original/CharacterDependencies/Limb.cs:38`, `source-reference/Original/ArmDependencies/Limb.cs:38`; develop `public field List<Vector2> bleedPoints` at `source-reference/Original/CharacterExecution/Limb.cs:38`, `source-reference/Original/ArmDependencies/Limb.cs:38`.
- **PR wins** `field:emitterMainModule`; PR `private field ParticleSystem.MainModule emitterMainModule` at `source-reference/Original/CharacterDependencies/Limb.cs:52`, `source-reference/Original/ArmDependencies/Limb.cs:52`; develop `private field ParticleSystem.MainModule emitterMainModule` at `source-reference/Original/CharacterExecution/Limb.cs:52`, `source-reference/Original/ArmDependencies/Limb.cs:52`.
- **PR wins** `field:emitterShapeModule`; PR `private field ParticleSystem.ShapeModule emitterShapeModule` at `source-reference/Original/CharacterDependencies/Limb.cs:50`, `source-reference/Original/ArmDependencies/Limb.cs:50`; develop `private field ParticleSystem.ShapeModule emitterShapeModule` at `source-reference/Original/CharacterExecution/Limb.cs:50`, `source-reference/Original/ArmDependencies/Limb.cs:50`.
- **PR wins** `field:exitAccumulationFactor`; PR `public field float exitAccumulationFactor` at `source-reference/Original/CharacterDependencies/Limb.cs:25`, `source-reference/Original/ArmDependencies/Limb.cs:25`; develop `public field float exitAccumulationFactor` at `source-reference/Original/CharacterExecution/Limb.cs:25`, `source-reference/Original/ArmDependencies/Limb.cs:25`.
- **PR wins** `field:floorStreaks`; PR `private field List<FloorStreakPainter> floorStreaks` at `source-reference/Original/CharacterDependencies/Limb.cs:54`, `source-reference/Original/ArmDependencies/Limb.cs:54`; develop `private field List<FloorStreakPainter> floorStreaks` at `source-reference/Original/CharacterExecution/Limb.cs:54`, `source-reference/Original/ArmDependencies/Limb.cs:54`.
- **PR wins** `field:hasSpawnedStreaks`; PR `private field bool hasSpawnedStreaks` at `source-reference/Original/CharacterDependencies/Limb.cs:70`, `source-reference/Original/ArmDependencies/Limb.cs:70`; develop `private field bool hasSpawnedStreaks` at `source-reference/Original/CharacterExecution/Limb.cs:70`, `source-reference/Original/ArmDependencies/Limb.cs:70`.
- **PR wins** `field:id`; PR `public field LimbId id` at `source-reference/Original/CharacterDependencies/Limb.cs:6`, `source-reference/Original/ArmDependencies/Limb.cs:6`; develop `public field LimbId id` at `source-reference/Original/CharacterExecution/Limb.cs:6`, `source-reference/Original/ArmDependencies/Limb.cs:6`.
- **PR wins** `field:kinematicParent`; PR `public field Transform kinematicParent` at `source-reference/Original/CharacterDependencies/Limb.cs:10`, `source-reference/Original/ArmDependencies/Limb.cs:10`; develop `public field Transform kinematicParent` at `source-reference/Original/CharacterExecution/Limb.cs:10`, `source-reference/Original/ArmDependencies/Limb.cs:10`.
- **PR wins** `field:lastExitTimestamp`; PR `private field float lastExitTimestamp` at `source-reference/Original/CharacterDependencies/Limb.cs:56`, `source-reference/Original/ArmDependencies/Limb.cs:56`; develop `private field float lastExitTimestamp` at `source-reference/Original/CharacterExecution/Limb.cs:56`, `source-reference/Original/ArmDependencies/Limb.cs:56`.
- **PR wins** `field:limbRenderer`; PR `public field LimbRenderer limbRenderer` at `source-reference/Original/CharacterDependencies/Limb.cs:28`, `source-reference/Original/ArmDependencies/Limb.cs:28`; develop `public field LimbRenderer limbRenderer` at `source-reference/Original/CharacterExecution/Limb.cs:28`, `source-reference/Original/ArmDependencies/Limb.cs:28`.
- **PR wins** `field:physiology`; PR `private field Physiology physiology` at `source-reference/Original/CharacterDependencies/Limb.cs:42`, `source-reference/Original/ArmDependencies/Limb.cs:42`; develop `private field Physiology physiology` at `source-reference/Original/CharacterExecution/Limb.cs:42`, `source-reference/Original/ArmDependencies/Limb.cs:42`.
- **PR wins** `field:potentialForce`; PR `private field Vector3 potentialForce` at `source-reference/Original/CharacterDependencies/Limb.cs:44`, `source-reference/Original/ArmDependencies/Limb.cs:44`; develop `private field Vector3 potentialForce` at `source-reference/Original/CharacterExecution/Limb.cs:44`, `source-reference/Original/ArmDependencies/Limb.cs:44`.
- **PR wins** `field:potentialPosition`; PR `private field Vector3 potentialPosition` at `source-reference/Original/CharacterDependencies/Limb.cs:46`, `source-reference/Original/ArmDependencies/Limb.cs:46`; develop `private field Vector3 potentialPosition` at `source-reference/Original/CharacterExecution/Limb.cs:46`, `source-reference/Original/ArmDependencies/Limb.cs:46`.
- **PR wins** `field:potentialTime`; PR `private field float potentialTime` at `source-reference/Original/CharacterDependencies/Limb.cs:62`, `source-reference/Original/ArmDependencies/Limb.cs:62`; develop `private field float potentialTime` at `source-reference/Original/CharacterExecution/Limb.cs:62`, `source-reference/Original/ArmDependencies/Limb.cs:62`.
- **PR wins** `field:previousPosition`; PR `private field Vector3 previousPosition` at `source-reference/Original/CharacterDependencies/Limb.cs:48`, `source-reference/Original/ArmDependencies/Limb.cs:48`; develop `private field Vector3 previousPosition` at `source-reference/Original/CharacterExecution/Limb.cs:48`, `source-reference/Original/ArmDependencies/Limb.cs:48`.
- **PR wins** `field:radius`; PR `public field float radius` at `source-reference/Original/CharacterDependencies/Limb.cs:13`, `source-reference/Original/ArmDependencies/Limb.cs:13`; develop `public field float radius` at `source-reference/Original/CharacterExecution/Limb.cs:13`, `source-reference/Original/ArmDependencies/Limb.cs:13`.
- **PR wins** `field:renderIndexLeft`; PR `public field int renderIndexLeft` at `source-reference/Original/CharacterDependencies/Limb.cs:34`, `source-reference/Original/ArmDependencies/Limb.cs:34`; develop `public field int renderIndexLeft` at `source-reference/Original/CharacterExecution/Limb.cs:34`, `source-reference/Original/ArmDependencies/Limb.cs:34`.
- **PR wins** `field:renderIndexRight`; PR `public field int renderIndexRight` at `source-reference/Original/CharacterDependencies/Limb.cs:36`, `source-reference/Original/ArmDependencies/Limb.cs:36`; develop `public field int renderIndexRight` at `source-reference/Original/CharacterExecution/Limb.cs:36`, `source-reference/Original/ArmDependencies/Limb.cs:36`.
- **PR wins** `field:rigidBodyDefaultDrag`; PR `private field float rigidBodyDefaultDrag` at `source-reference/Original/CharacterDependencies/Limb.cs:80`, `source-reference/Original/ArmDependencies/Limb.cs:80`; develop `private field float rigidBodyDefaultDrag` at `source-reference/Original/CharacterExecution/Limb.cs:80`, `source-reference/Original/ArmDependencies/Limb.cs:80`.
- **PR wins** `field:useCensoring`; PR `private field bool useCensoring` at `source-reference/Original/CharacterDependencies/Limb.cs:76`, `source-reference/Original/ArmDependencies/Limb.cs:76`; develop `private field bool useCensoring` at `source-reference/Original/CharacterExecution/Limb.cs:76`, `source-reference/Original/ArmDependencies/Limb.cs:76`.
- **PR wins** `field:useImpactEmissions`; PR `private field bool useImpactEmissions` at `source-reference/Original/CharacterDependencies/Limb.cs:78`, `source-reference/Original/ArmDependencies/Limb.cs:78`; develop `private field bool useImpactEmissions` at `source-reference/Original/CharacterExecution/Limb.cs:78`, `source-reference/Original/ArmDependencies/Limb.cs:78`.
- **PR wins** `field:useSurfaceDecals`; PR `private field bool useSurfaceDecals` at `source-reference/Original/CharacterDependencies/Limb.cs:72`, `source-reference/Original/ArmDependencies/Limb.cs:72`; develop `private field bool useSurfaceDecals` at `source-reference/Original/CharacterExecution/Limb.cs:72`, `source-reference/Original/ArmDependencies/Limb.cs:72`.
- **PR wins** `field:useWoundDecals`; PR `private field bool useWoundDecals` at `source-reference/Original/CharacterDependencies/Limb.cs:74`, `source-reference/Original/ArmDependencies/Limb.cs:74`; develop `private field bool useWoundDecals` at `source-reference/Original/CharacterExecution/Limb.cs:74`, `source-reference/Original/ArmDependencies/Limb.cs:74`.
- **PR wins** `field:visual`; PR `public field Transform visual` at `source-reference/Original/CharacterDependencies/Limb.cs:16`, `source-reference/Original/ArmDependencies/Limb.cs:16`; develop `public field Transform visual` at `source-reference/Original/CharacterExecution/Limb.cs:16`, `source-reference/Original/ArmDependencies/Limb.cs:16`.
- **PR wins** `field:woundStreakLongLifetime`; PR `private field float woundStreakLongLifetime` at `source-reference/Original/CharacterDependencies/Limb.cs:68`, `source-reference/Original/ArmDependencies/Limb.cs:68`; develop `private field float woundStreakLongLifetime` at `source-reference/Original/CharacterExecution/Limb.cs:68`, `source-reference/Original/ArmDependencies/Limb.cs:68`.
- **PR wins** `field:woundStreakMediumLifetime`; PR `private field float woundStreakMediumLifetime` at `source-reference/Original/CharacterDependencies/Limb.cs:66`, `source-reference/Original/ArmDependencies/Limb.cs:66`; develop `private field float woundStreakMediumLifetime` at `source-reference/Original/CharacterExecution/Limb.cs:66`, `source-reference/Original/ArmDependencies/Limb.cs:66`.
- **PR wins** `field:woundStreakShortLifetime`; PR `private field float woundStreakShortLifetime` at `source-reference/Original/CharacterDependencies/Limb.cs:64`, `source-reference/Original/ArmDependencies/Limb.cs:64`; develop `private field float woundStreakShortLifetime` at `source-reference/Original/CharacterExecution/Limb.cs:64`, `source-reference/Original/ArmDependencies/Limb.cs:64`.
- **PR wins** `field:zLeft`; PR `public field float zLeft` at `source-reference/Original/CharacterDependencies/Limb.cs:31`, `source-reference/Original/ArmDependencies/Limb.cs:31`; develop `public field float zLeft` at `source-reference/Original/CharacterExecution/Limb.cs:31`, `source-reference/Original/ArmDependencies/Limb.cs:31`.
- **PR wins** `field:zRight`; PR `private field float zRight` at `source-reference/Original/CharacterDependencies/Limb.cs:60`, `source-reference/Original/ArmDependencies/Limb.cs:60`; develop `private field float zRight` at `source-reference/Original/CharacterExecution/Limb.cs:60`, `source-reference/Original/ArmDependencies/Limb.cs:60`.
- **PR wins** ``method:AccumulateExitingProjectiles`0(Projectile)``; PR `private void AccumulateExitingProjectiles(Projectile)` at `source-reference/Original/CharacterDependencies/Limb.cs:423`, `source-reference/Original/ArmDependencies/Limb.cs:423`; develop `private void AccumulateExitingProjectiles(Projectile)` at `source-reference/Original/CharacterExecution/Limb.cs:393`, `source-reference/Original/ArmDependencies/Limb.cs:423`.
- **PR wins** ``method:AddTrauma`0(float, Vector3, Vector3)``; PR `public void AddTrauma(float, Vector3, Vector3)` at `source-reference/Original/CharacterDependencies/Limb.cs:551`, `source-reference/Original/ArmDependencies/Limb.cs:551`; develop `public void AddTrauma(float, Vector3, Vector3)` at `source-reference/Original/CharacterExecution/Limb.cs:521`, `source-reference/Original/ArmDependencies/Limb.cs:551`.
- **PR wins** ``method:AimPointWithExtents`0(Vector3)``; PR `public Vector3 AimPointWithExtents(Vector3)` at `source-reference/Original/CharacterDependencies/Limb.cs:719`, `source-reference/Original/ArmDependencies/Limb.cs:719`; develop `public Vector3 AimPointWithExtents(Vector3)` at `source-reference/Original/CharacterExecution/Limb.cs:689`, `source-reference/Original/ArmDependencies/Limb.cs:719`.
- **PR wins** ``method:ApplyForceAtPosition`0(Vector3, Vector3)``; PR `public void ApplyForceAtPosition(Vector3, Vector3)` at `source-reference/Original/CharacterDependencies/Limb.cs:623`, `source-reference/Original/ArmDependencies/Limb.cs:623`; develop `public void ApplyForceAtPosition(Vector3, Vector3)` at `source-reference/Original/CharacterExecution/Limb.cs:593`, `source-reference/Original/ArmDependencies/Limb.cs:623`.
- **PR wins** ``method:ApplySettings`0(Settings)``; PR `public void ApplySettings(Settings)` at `source-reference/Original/CharacterDependencies/Limb.cs:171`, `source-reference/Original/ArmDependencies/Limb.cs:171`; develop `public void ApplySettings(Settings)` at `source-reference/Original/CharacterExecution/Limb.cs:141`, `source-reference/Original/ArmDependencies/Limb.cs:171`.
- **PR wins** ``method:ArmorEntryTest`0(Projectile)``; PR `private Projectile ArmorEntryTest(Projectile)` at `source-reference/Original/CharacterDependencies/Limb.cs:242`, `source-reference/Original/ArmDependencies/Limb.cs:242`; develop `private Projectile ArmorEntryTest(Projectile)` at `source-reference/Original/CharacterExecution/Limb.cs:212`, `source-reference/Original/ArmDependencies/Limb.cs:242`.
- **PR wins** ``method:ArmorExitTest`0(Projectile)``; PR `private Projectile ArmorExitTest(Projectile)` at `source-reference/Original/CharacterDependencies/Limb.cs:415`, `source-reference/Original/ArmDependencies/Limb.cs:415`; develop `private Projectile ArmorExitTest(Projectile)` at `source-reference/Original/CharacterExecution/Limb.cs:385`, `source-reference/Original/ArmDependencies/Limb.cs:415`.
- **PR wins** ``method:Awake`0()``; PR `public override void Awake()` at `source-reference/Original/CharacterDependencies/Limb.cs:148`, `source-reference/Original/ArmDependencies/Limb.cs:148`; develop `public override void Awake()` at `source-reference/Original/CharacterExecution/Limb.cs:118`, `source-reference/Original/ArmDependencies/Limb.cs:148`.
- **PR wins** ``method:CreateConcussiveWounds`0(float)``; PR `public void CreateConcussiveWounds(float)` at `source-reference/Original/CharacterDependencies/Limb.cs:558`, `source-reference/Original/ArmDependencies/Limb.cs:558`; develop `public void CreateConcussiveWounds(float)` at `source-reference/Original/CharacterExecution/Limb.cs:528`, `source-reference/Original/ArmDependencies/Limb.cs:558`.
- **PR wins** ``method:Detach`0()``; PR `public void Detach()` at `source-reference/Original/CharacterDependencies/Limb.cs:747`, `source-reference/Original/ArmDependencies/Limb.cs:747`; develop `public void Detach()` at `source-reference/Original/CharacterExecution/Limb.cs:717`, `source-reference/Original/ArmDependencies/Limb.cs:747`.
- **PR wins** ``method:FindEntryExitPoints`0(Projectile, LimbLayer, ref Vector3, ref Vector3, bool)``; PR `private LimbRenderer.TraceResult FindEntryExitPoints(Projectile, LimbLayer, ref Vector3, ref Vector3, bool)` at `source-reference/Original/CharacterDependencies/Limb.cs:540`, `source-reference/Original/ArmDependencies/Limb.cs:540`; develop `private LimbRenderer.TraceResult FindEntryExitPoints(Projectile, LimbLayer, ref Vector3, ref Vector3, bool)` at `source-reference/Original/CharacterExecution/Limb.cs:510`, `source-reference/Original/ArmDependencies/Limb.cs:540`.
- **PR wins** ``method:FindInternalDistanceAndExitPosition`0(Projectile)``; PR `protected override void FindInternalDistanceAndExitPosition(Projectile)` at `source-reference/Original/CharacterDependencies/Limb.cs:270`, `source-reference/Original/ArmDependencies/Limb.cs:270`; develop `protected override void FindInternalDistanceAndExitPosition(Projectile)` at `source-reference/Original/CharacterExecution/Limb.cs:240`, `source-reference/Original/ArmDependencies/Limb.cs:270`.
- **PR wins** ``method:Flip`0()``; PR `public void Flip()` at `source-reference/Original/CharacterDependencies/Limb.cs:514`, `source-reference/Original/ArmDependencies/Limb.cs:514`; develop `public void Flip()` at `source-reference/Original/CharacterExecution/Limb.cs:484`, `source-reference/Original/ArmDependencies/Limb.cs:514`.
- **PR wins** ``method:HandleDeflection`0(Projectile, Vector3, bool)``; PR `protected override Projectile HandleDeflection(Projectile, Vector3, bool)` at `source-reference/Original/CharacterDependencies/Limb.cs:229`, `source-reference/Original/ArmDependencies/Limb.cs:229`; develop `protected override Projectile HandleDeflection(Projectile, Vector3, bool)` at `source-reference/Original/CharacterExecution/Limb.cs:199`, `source-reference/Original/ArmDependencies/Limb.cs:229`.
- **PR wins** ``method:HandleEntry`0(Projectile, Vector3, bool)``; PR `protected override Projectile HandleEntry(Projectile, Vector3, bool)` at `source-reference/Original/CharacterDependencies/Limb.cs:255`, `source-reference/Original/ArmDependencies/Limb.cs:255`; develop `protected override Projectile HandleEntry(Projectile, Vector3, bool)` at `source-reference/Original/CharacterExecution/Limb.cs:225`, `source-reference/Original/ArmDependencies/Limb.cs:255`.
- **PR wins** ``method:HandleExit`0(Projectile, bool)``; PR `protected override void HandleExit(Projectile, bool)` at `source-reference/Original/CharacterDependencies/Limb.cs:347`, `source-reference/Original/ArmDependencies/Limb.cs:347`; develop `protected override void HandleExit(Projectile, bool)` at `source-reference/Original/CharacterExecution/Limb.cs:317`, `source-reference/Original/ArmDependencies/Limb.cs:347`.
- **PR wins** ``method:HandleInterior`0(Projectile, Vector3, bool)``; PR `protected override Projectile HandleInterior(Projectile, Vector3, bool)` at `source-reference/Original/CharacterDependencies/Limb.cs:293`, `source-reference/Original/ArmDependencies/Limb.cs:293`; develop `protected override Projectile HandleInterior(Projectile, Vector3, bool)` at `source-reference/Original/CharacterExecution/Limb.cs:263`, `source-reference/Original/ArmDependencies/Limb.cs:293`.
- **PR wins** ``method:OnBecomeRagdoll`0()``; PR `public void OnBecomeRagdoll()` at `source-reference/Original/CharacterDependencies/Limb.cs:221`, `source-reference/Original/ArmDependencies/Limb.cs:221`; develop `public void OnBecomeRagdoll()` at `source-reference/Original/CharacterExecution/Limb.cs:191`, `source-reference/Original/ArmDependencies/Limb.cs:221`.
- **PR wins** ``method:OnCollisionEnter`0(Collision)``; PR `private void OnCollisionEnter(Collision)` at `source-reference/Original/CharacterDependencies/Limb.cs:596`, `source-reference/Original/ArmDependencies/Limb.cs:596`; develop `private void OnCollisionEnter(Collision)` at `source-reference/Original/CharacterExecution/Limb.cs:566`, `source-reference/Original/ArmDependencies/Limb.cs:596`.
- **PR wins** ``method:PlayImpactAudio`0()``; PR `private void PlayImpactAudio()` at `source-reference/Original/CharacterDependencies/Limb.cs:484`, `source-reference/Original/ArmDependencies/Limb.cs:484`; develop `private void PlayImpactAudio()` at `source-reference/Original/CharacterExecution/Limb.cs:454`, `source-reference/Original/ArmDependencies/Limb.cs:484`.
- **PR wins** ``method:SetCensored`0(bool)``; PR `public void SetCensored(bool)` at `source-reference/Original/CharacterDependencies/Limb.cs:725`, `source-reference/Original/ArmDependencies/Limb.cs:725`; develop `public void SetCensored(bool)` at `source-reference/Original/CharacterExecution/Limb.cs:695`, `source-reference/Original/ArmDependencies/Limb.cs:725`.
- **PR wins** ``method:SetFloating`0(bool)``; PR `public void SetFloating(bool)` at `source-reference/Original/CharacterDependencies/Limb.cs:527`, `source-reference/Original/ArmDependencies/Limb.cs:527`; develop `public void SetFloating(bool)` at `source-reference/Original/CharacterExecution/Limb.cs:497`, `source-reference/Original/ArmDependencies/Limb.cs:527`.
- **PR wins** ``method:SetKinematic`0(bool, Vector3)``; PR `public void SetKinematic(bool, Vector3)` at `source-reference/Original/CharacterDependencies/Limb.cs:496`, `source-reference/Original/ArmDependencies/Limb.cs:496`; develop `public void SetKinematic(bool, Vector3)` at `source-reference/Original/CharacterExecution/Limb.cs:466`, `source-reference/Original/ArmDependencies/Limb.cs:496`.
- **PR wins** ``method:SpawnDynamicDecals`0(Wound)``; PR `public void SpawnDynamicDecals(Wound)` at `source-reference/Original/CharacterDependencies/Limb.cs:577`, `source-reference/Original/ArmDependencies/Limb.cs:577`; develop `public void SpawnDynamicDecals(Wound)` at `source-reference/Original/CharacterExecution/Limb.cs:547`, `source-reference/Original/ArmDependencies/Limb.cs:577`.
- **PR wins** ``method:SpawnEntryEffect`0(Projectile, Vector3)``; PR `protected override void SpawnEntryEffect(Projectile, Vector3)` at `source-reference/Original/CharacterDependencies/Limb.cs:451`, `source-reference/Original/ArmDependencies/Limb.cs:451`; develop `protected override void SpawnEntryEffect(Projectile, Vector3)` at `source-reference/Original/CharacterExecution/Limb.cs:421`, `source-reference/Original/ArmDependencies/Limb.cs:451`.
- **PR wins** ``method:SpawnExitEffect`0(Projectile)``; PR `protected override void SpawnExitEffect(Projectile)` at `source-reference/Original/CharacterDependencies/Limb.cs:471`, `source-reference/Original/ArmDependencies/Limb.cs:471`; develop `protected override void SpawnExitEffect(Projectile)` at `source-reference/Original/CharacterExecution/Limb.cs:441`, `source-reference/Original/ArmDependencies/Limb.cs:471`.
- **PR wins** ``method:SpawnGibs`0(Projectile, float)``; PR `private void SpawnGibs(Projectile, float)` at `source-reference/Original/CharacterDependencies/Limb.cs:436`, `source-reference/Original/ArmDependencies/Limb.cs:436`; develop `private void SpawnGibs(Projectile, float)` at `source-reference/Original/CharacterExecution/Limb.cs:406`, `source-reference/Original/ArmDependencies/Limb.cs:436`.
- **PR wins** ``method:Start`0()``; PR `public override void Start()` at `source-reference/Original/CharacterDependencies/Limb.cs:161`, `source-reference/Original/ArmDependencies/Limb.cs:161`; develop `public override void Start()` at `source-reference/Original/CharacterExecution/Limb.cs:131`, `source-reference/Original/ArmDependencies/Limb.cs:161`.
- **PR wins** ``method:StopHemorrhage`0()``; PR `public void StopHemorrhage()` at `source-reference/Original/CharacterDependencies/Limb.cs:569`, `source-reference/Original/ArmDependencies/Limb.cs:569`; develop `public void StopHemorrhage()` at `source-reference/Original/CharacterExecution/Limb.cs:539`, `source-reference/Original/ArmDependencies/Limb.cs:569`.
- **PR wins** ``method:UpdateFloorPool`0()``; PR `private void UpdateFloorPool()` at `source-reference/Original/CharacterDependencies/Limb.cs:691`, `source-reference/Original/ArmDependencies/Limb.cs:691`; develop `private void UpdateFloorPool()` at `source-reference/Original/CharacterExecution/Limb.cs:661`, `source-reference/Original/ArmDependencies/Limb.cs:691`.
- **PR wins** ``method:UpdateFloorStreaks`0()``; PR `private void UpdateFloorStreaks()` at `source-reference/Original/CharacterDependencies/Limb.cs:646`, `source-reference/Original/ArmDependencies/Limb.cs:646`; develop `private void UpdateFloorStreaks()` at `source-reference/Original/CharacterExecution/Limb.cs:616`, `source-reference/Original/ArmDependencies/Limb.cs:646`.
- **PR wins** ``method:Update`0()``; PR `public override void Update()` at `source-reference/Original/CharacterDependencies/Limb.cs:195`, `source-reference/Original/ArmDependencies/Limb.cs:195`; develop `public override void Update()` at `source-reference/Original/CharacterExecution/Limb.cs:165`, `source-reference/Original/ArmDependencies/Limb.cs:195`.
- **PR wins** ``method:WoundEmission`0(int, Vector3, Vector3, float, float)``; PR `public void WoundEmission(int, Vector3, Vector3, float, float)` at `source-reference/Original/CharacterDependencies/Limb.cs:212`, `source-reference/Original/ArmDependencies/Limb.cs:212`; develop `public void WoundEmission(int, Vector3, Vector3, float, float)` at `source-reference/Original/CharacterExecution/Limb.cs:182`, `source-reference/Original/ArmDependencies/Limb.cs:212`.
- **PR wins** `property:AimPoint`; PR `public Vector3 AimPoint` at `source-reference/Original/CharacterDependencies/Limb.cs:86`, `source-reference/Original/ArmDependencies/Limb.cs:86`; develop `public Vector3 AimPoint` at `source-reference/Original/CharacterExecution/Limb.cs:86`, `source-reference/Original/ArmDependencies/Limb.cs:86`.
- **PR wins** `property:Armor`; PR `public LimbArmor Armor` at `source-reference/Original/CharacterDependencies/Limb.cs:84`, `source-reference/Original/ArmDependencies/Limb.cs:84`; develop `public LimbArmor Armor` at `source-reference/Original/CharacterExecution/Limb.cs:84`, `source-reference/Original/ArmDependencies/Limb.cs:84`.
- **PR wins** `property:Body`; PR `public CharacterBody Body` at `source-reference/Original/CharacterDependencies/Limb.cs:96`, `source-reference/Original/ArmDependencies/Limb.cs:96`; develop `public CharacterBody Body` at `source-reference/Original/CharacterExecution/Limb.cs:90`, `source-reference/Original/ArmDependencies/Limb.cs:96`.
- **PR wins** `property:CensorBox`; PR `public GameObject CensorBox` at `source-reference/Original/CharacterDependencies/Limb.cs:82`, `source-reference/Original/ArmDependencies/Limb.cs:82`; develop `public GameObject CensorBox` at `source-reference/Original/CharacterExecution/Limb.cs:82`, `source-reference/Original/ArmDependencies/Limb.cs:82`.
- **PR wins** `property:Censored`; PR `public bool Censored` at `source-reference/Original/CharacterDependencies/Limb.cs:138`, `source-reference/Original/ArmDependencies/Limb.cs:138`; develop `public bool Censored` at `source-reference/Original/CharacterExecution/Limb.cs:114`, `source-reference/Original/ArmDependencies/Limb.cs:138`.
- **PR wins** `property:DynamicDecalRenderer`; PR `public DynamicLimbDecalRenderer DynamicDecalRenderer` at `source-reference/Original/CharacterDependencies/Limb.cs:106`, `source-reference/Original/ArmDependencies/Limb.cs:106`; develop `public DynamicLimbDecalRenderer DynamicDecalRenderer` at `source-reference/Original/CharacterExecution/Limb.cs:100`, `source-reference/Original/ArmDependencies/Limb.cs:106`.
- **PR wins** `property:FloorPoolPainter`; PR `public FloorPoolPainter FloorPoolPainter` at `source-reference/Original/CharacterDependencies/Limb.cs:108`, `source-reference/Original/ArmDependencies/Limb.cs:108`; develop `public FloorPoolPainter FloorPoolPainter` at `source-reference/Original/CharacterExecution/Limb.cs:102`, `source-reference/Original/ArmDependencies/Limb.cs:108`.
- **PR wins** `property:HasArmor`; PR `public bool HasArmor` at `source-reference/Original/CharacterDependencies/Limb.cs:130`, `source-reference/Original/ArmDependencies/Limb.cs:130`; develop `public bool HasArmor` at `source-reference/Original/CharacterExecution/Limb.cs:112`, `source-reference/Original/ArmDependencies/Limb.cs:130`.
- **PR wins** `property:HitCount`; PR `public int HitCount` at `source-reference/Original/CharacterDependencies/Limb.cs:146`, `source-reference/Original/ArmDependencies/Limb.cs:146`; develop `public int HitCount` at `source-reference/Original/CharacterExecution/Limb.cs:116`, `source-reference/Original/ArmDependencies/Limb.cs:146`.
- **PR wins** `property:IsHemorrhaging`; PR `public bool IsHemorrhaging` at `source-reference/Original/CharacterDependencies/Limb.cs:122`, `source-reference/Original/ArmDependencies/Limb.cs:122`; develop `public bool IsHemorrhaging` at `source-reference/Original/CharacterExecution/Limb.cs:110`, `source-reference/Original/ArmDependencies/Limb.cs:122`.
- **PR wins** `property:Joint`; PR `public HingeJoint Joint` at `source-reference/Original/CharacterDependencies/Limb.cs:104`, `source-reference/Original/ArmDependencies/Limb.cs:104`; develop `public HingeJoint Joint` at `source-reference/Original/CharacterExecution/Limb.cs:98`, `source-reference/Original/ArmDependencies/Limb.cs:104`.
- **PR wins** `property:LimbMuscle`; PR `public LimbMuscle LimbMuscle` at `source-reference/Original/CharacterDependencies/Limb.cs:100`, `source-reference/Original/ArmDependencies/Limb.cs:100`; develop `public LimbMuscle LimbMuscle` at `source-reference/Original/CharacterExecution/Limb.cs:94`, `source-reference/Original/ArmDependencies/Limb.cs:100`.
- **PR wins** `property:LimbPhysiology`; PR `public LimbPhysiology LimbPhysiology` at `source-reference/Original/CharacterDependencies/Limb.cs:98`, `source-reference/Original/ArmDependencies/Limb.cs:98`; develop `public LimbPhysiology LimbPhysiology` at `source-reference/Original/CharacterExecution/Limb.cs:92`, `source-reference/Original/ArmDependencies/Limb.cs:98`.
- **PR wins** `property:Rigidbody`; PR `public Rigidbody Rigidbody` at `source-reference/Original/CharacterDependencies/Limb.cs:102`, `source-reference/Original/ArmDependencies/Limb.cs:102`; develop `public Rigidbody Rigidbody` at `source-reference/Original/CharacterExecution/Limb.cs:96`, `source-reference/Original/ArmDependencies/Limb.cs:102`.
- **PR wins** `property:Stationary`; PR `public bool Stationary` at `source-reference/Original/CharacterDependencies/Limb.cs:112`, `source-reference/Original/ArmDependencies/Limb.cs:112`; develop `public bool Stationary` at `source-reference/Original/CharacterExecution/Limb.cs:106`, `source-reference/Original/ArmDependencies/Limb.cs:112`.
- **PR wins** `property:Velocity`; PR `public Vector3 Velocity` at `source-reference/Original/CharacterDependencies/Limb.cs:94`, `source-reference/Original/ArmDependencies/Limb.cs:94`; develop `public Vector3 Velocity` at `source-reference/Original/CharacterExecution/Limb.cs:88`, `source-reference/Original/ArmDependencies/Limb.cs:94`.
- **PR wins** `property:WoundEmitter`; PR `public Emitter WoundEmitter` at `source-reference/Original/CharacterDependencies/Limb.cs:110`, `source-reference/Original/ArmDependencies/Limb.cs:110`; develop `public Emitter WoundEmitter` at `source-reference/Original/CharacterExecution/Limb.cs:104`, `source-reference/Original/ArmDependencies/Limb.cs:110`.
- **PR wins** `property:Wounded`; PR `public bool Wounded` at `source-reference/Original/CharacterDependencies/Limb.cs:114`, `source-reference/Original/ArmDependencies/Limb.cs:114`; develop `public bool Wounded` at `source-reference/Original/CharacterExecution/Limb.cs:108`, `source-reference/Original/ArmDependencies/Limb.cs:114`.


### 114. `MarksmanMode` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `MarksmanMode`; partial `false`; `runtime/Engine/BallisticEngine.cs:6`

**develop declarations**:
- class; namespace `(global)`; `MarksmanMode`; partial `false`; `runtime/Engine/BallisticEngine.cs:46`

Common member signatures: **1**.
- **PR wins** `field:DeflectionModifier`; PR `public static field float DeflectionModifier` at `runtime/Engine/BallisticEngine.cs:6`; develop `public static field float DeflectionModifier` at `runtime/Engine/BallisticEngine.cs:46`.


### 115. `Masks` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `Masks`; partial `false`; `runtime/Engine/BallisticEngine.cs:7`

**develop declarations**:
- class; namespace `(global)`; `Masks`; partial `false`; `runtime/Engine/BallisticEngine.cs:47`

Common member signatures: **4**.
- **PR wins** `field:BallisticTargets`; PR `public field int BallisticTargets` at `runtime/Engine/BallisticEngine.cs:7`; develop `public field int BallisticTargets` at `runtime/Engine/BallisticEngine.cs:47`.
- **PR wins** `field:BallisticTargetsSansLimbs`; PR `public field int BallisticTargetsSansLimbs` at `runtime/Engine/BallisticEngine.cs:7`; develop `public field int BallisticTargetsSansLimbs` at `runtime/Engine/BallisticEngine.cs:47`.
- **PR wins** `field:Instance`; PR `public static field Masks Instance` at `runtime/Engine/BallisticEngine.cs:7`; develop `public static field Masks Instance` at `runtime/Engine/BallisticEngine.cs:47`.
- **PR wins** `field:Limbs`; PR `public field int Limbs` at `runtime/Engine/BallisticEngine.cs:7`; develop `public field int Limbs` at `runtime/Engine/BallisticEngine.cs:47`.


### 116. `Mission` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `Mission`; partial `false`; `source-reference/Original/CharacterDependencies/Mission.cs:6`

**develop declarations**:
- class; namespace `(global)`; `Mission`; partial `false`; `source-reference/Original/CharacterDependencies/Mission.cs:6`

Common member signatures: **121**.
- **PR wins** `field:ObjectName`; PR `public static field string ObjectName` at `source-reference/Original/CharacterDependencies/Mission.cs:24`; develop `public static field string ObjectName` at `source-reference/Original/CharacterDependencies/Mission.cs:24`.
- **PR wins** `field:UI`; PR `public field MissionUI UI` at `source-reference/Original/CharacterDependencies/Mission.cs:26`; develop `public field MissionUI UI` at `source-reference/Original/CharacterDependencies/Mission.cs:26`.
- **PR wins** `field:ambientLight`; PR `public field Light ambientLight` at `source-reference/Original/CharacterDependencies/Mission.cs:28`; develop `public field Light ambientLight` at `source-reference/Original/CharacterDependencies/Mission.cs:28`.
- **PR wins** `field:audioPropagator`; PR `private field AudioPropagator audioPropagator` at `source-reference/Original/CharacterDependencies/Mission.cs:102`; develop `private field AudioPropagator audioPropagator` at `source-reference/Original/CharacterDependencies/Mission.cs:102`.
- **PR wins** `field:bloodStreamPrefab`; PR `public field BloodStream bloodStreamPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:48`; develop `public field BloodStream bloodStreamPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:48`.
- **PR wins** `field:currentMode`; PR `private field MissionView currentMode` at `source-reference/Original/CharacterDependencies/Mission.cs:104`; develop `private field MissionView currentMode` at `source-reference/Original/CharacterDependencies/Mission.cs:104`.
- **PR wins** `field:dataHasBeenRecorded`; PR `private field bool dataHasBeenRecorded` at `source-reference/Original/CharacterDependencies/Mission.cs:114`; develop `private field bool dataHasBeenRecorded` at `source-reference/Original/CharacterDependencies/Mission.cs:114`.
- **PR wins** `field:defaultAmbientLightIntensity`; PR `private field float defaultAmbientLightIntensity` at `source-reference/Original/CharacterDependencies/Mission.cs:112`; develop `private field float defaultAmbientLightIntensity` at `source-reference/Original/CharacterDependencies/Mission.cs:112`.
- **PR wins** `field:disableEndCriteria`; PR `private field bool disableEndCriteria` at `source-reference/Original/CharacterDependencies/Mission.cs:116`; develop `private field bool disableEndCriteria` at `source-reference/Original/CharacterDependencies/Mission.cs:116`.
- **PR wins** `field:firstInjuryTimestamp`; PR `private field float firstInjuryTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:110`; develop `private field float firstInjuryTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:110`.
- **PR wins** `field:floorPoolPrefab`; PR `public field FloorPoolPainter floorPoolPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:53`; develop `public field FloorPoolPainter floorPoolPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:53`.
- **PR wins** `field:headCensorPrefab`; PR `public field GameObject headCensorPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:59`; develop `public field GameObject headCensorPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:59`.
- **PR wins** `field:input`; PR `private field InputHandler input` at `source-reference/Original/CharacterDependencies/Mission.cs:100`; develop `private field InputHandler input` at `source-reference/Original/CharacterDependencies/Mission.cs:100`.
- **PR wins** `field:interiorModePrefab`; PR `public field InteriorMode interiorModePrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:45`; develop `public field InteriorMode interiorModePrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:45`.
- **PR wins** `field:marksmen`; PR `public field Marksmen marksmen` at `source-reference/Original/CharacterDependencies/Mission.cs:33`; develop `public field Marksmen marksmen` at `source-reference/Original/CharacterDependencies/Mission.cs:33`.
- **PR wins** `field:missionEnded`; PR `public field bool missionEnded` at `source-reference/Original/CharacterDependencies/Mission.cs:76`; develop `public field bool missionEnded` at `source-reference/Original/CharacterDependencies/Mission.cs:76`.
- **PR wins** `field:missionSuccess`; PR `public field bool missionSuccess` at `source-reference/Original/CharacterDependencies/Mission.cs:78`; develop `public field bool missionSuccess` at `source-reference/Original/CharacterDependencies/Mission.cs:78`.
- **PR wins** `field:negotiationMode`; PR `public field NegotiationMode negotiationMode` at `source-reference/Original/CharacterDependencies/Mission.cs:37`; develop `public field NegotiationMode negotiationMode` at `source-reference/Original/CharacterDependencies/Mission.cs:37`.
- **PR wins** `field:onAllClear`; PR `public field UnityEventVoid onAllClear` at `source-reference/Original/CharacterDependencies/Mission.cs:85`; develop `public field UnityEventVoid onAllClear` at `source-reference/Original/CharacterDependencies/Mission.cs:85`.
- **PR wins** `field:onMissionBegin`; PR `public field UnityEventVoid onMissionBegin` at `source-reference/Original/CharacterDependencies/Mission.cs:82`; develop `public field UnityEventVoid onMissionBegin` at `source-reference/Original/CharacterDependencies/Mission.cs:82`.
- **PR wins** `field:onMissionEnd`; PR `public field UnityEventVoid onMissionEnd` at `source-reference/Original/CharacterDependencies/Mission.cs:88`; develop `public field UnityEventVoid onMissionEnd` at `source-reference/Original/CharacterDependencies/Mission.cs:88`.
- **PR wins** `field:onMissionFailure`; PR `public field UnityEventVoid onMissionFailure` at `source-reference/Original/CharacterDependencies/Mission.cs:94`; develop `public field UnityEventVoid onMissionFailure` at `source-reference/Original/CharacterDependencies/Mission.cs:94`.
- **PR wins** `field:onMissionSuccess`; PR `public field UnityEventVoid onMissionSuccess` at `source-reference/Original/CharacterDependencies/Mission.cs:91`; develop `public field UnityEventVoid onMissionSuccess` at `source-reference/Original/CharacterDependencies/Mission.cs:91`.
- **PR wins** `field:onPaintTextureResolutionChanged`; PR `public field UnityEventInt onPaintTextureResolutionChanged` at `source-reference/Original/CharacterDependencies/Mission.cs:80`; develop `public field UnityEventInt onPaintTextureResolutionChanged` at `source-reference/Original/CharacterDependencies/Mission.cs:80`.
- **PR wins** `field:onPhoneDelivered`; PR `public field UnityEventVoid onPhoneDelivered` at `source-reference/Original/CharacterDependencies/Mission.cs:97`; develop `public field UnityEventVoid onPhoneDelivered` at `source-reference/Original/CharacterDependencies/Mission.cs:97`.
- **PR wins** `field:perfMgr`; PR `private field PerformanceManager perfMgr` at `source-reference/Original/CharacterDependencies/Mission.cs:108`; develop `private field PerformanceManager perfMgr` at `source-reference/Original/CharacterDependencies/Mission.cs:108`.
- **PR wins** `field:projectileFragments`; PR `public field List<Mesh> projectileFragments` at `source-reference/Original/CharacterDependencies/Mission.cs:39`; develop `public field List<Mesh> projectileFragments` at `source-reference/Original/CharacterDependencies/Mission.cs:39`.
- **PR wins** `field:projectileRange`; PR `public field float projectileRange` at `source-reference/Original/CharacterDependencies/Mission.cs:43`; develop `public field float projectileRange` at `source-reference/Original/CharacterDependencies/Mission.cs:43`.
- **PR wins** `field:restrainMarker`; PR `public field DynamicInteractionMarker restrainMarker` at `source-reference/Original/CharacterDependencies/Mission.cs:67`; develop `public field DynamicInteractionMarker restrainMarker` at `source-reference/Original/CharacterDependencies/Mission.cs:67`.
- **PR wins** `field:rooms`; PR `public field List<Room> rooms` at `source-reference/Original/CharacterDependencies/Mission.cs:41`; develop `public field List<Room> rooms` at `source-reference/Original/CharacterDependencies/Mission.cs:41`.
- **PR wins** `field:sandboxStartMode`; PR `public field MissionView sandboxStartMode` at `source-reference/Original/CharacterDependencies/Mission.cs:35`; develop `public field MissionView sandboxStartMode` at `source-reference/Original/CharacterDependencies/Mission.cs:35`.
- **PR wins** `field:secureWeaponMarker`; PR `public field DynamicInteractionMarker secureWeaponMarker` at `source-reference/Original/CharacterDependencies/Mission.cs:63`; develop `public field DynamicInteractionMarker secureWeaponMarker` at `source-reference/Original/CharacterDependencies/Mission.cs:63`.
- **PR wins** `field:shadowLightSources`; PR `public field List<Light> shadowLightSources` at `source-reference/Original/CharacterDependencies/Mission.cs:30`; develop `public field List<Light> shadowLightSources` at `source-reference/Original/CharacterDependencies/Mission.cs:30`.
- **PR wins** `field:stabilizeMarker`; PR `public field DynamicInteractionMarker stabilizeMarker` at `source-reference/Original/CharacterDependencies/Mission.cs:70`; develop `public field DynamicInteractionMarker stabilizeMarker` at `source-reference/Original/CharacterDependencies/Mission.cs:70`.
- **PR wins** `field:streamPoolPrefab`; PR `public field FloorPoolPainter streamPoolPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:51`; develop `public field FloorPoolPainter streamPoolPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:51`.
- **PR wins** `field:texturePool`; PR `private field DynamicLimbTexturePool texturePool` at `source-reference/Original/CharacterDependencies/Mission.cs:106`; develop `private field DynamicLimbTexturePool texturePool` at `source-reference/Original/CharacterDependencies/Mission.cs:106`.
- **PR wins** `field:toggleElectricity`; PR `public field ToggleElectricity toggleElectricity` at `source-reference/Original/CharacterDependencies/Mission.cs:73`; develop `public field ToggleElectricity toggleElectricity` at `source-reference/Original/CharacterDependencies/Mission.cs:73`.
- **PR wins** `field:wallStreakPrefab`; PR `public field WallStreakPainter wallStreakPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:55`; develop `public field WallStreakPainter wallStreakPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:55`.
- **PR wins** `field:weaponSmokeEmitterPrefab`; PR `public field ParticleSystem weaponSmokeEmitterPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:61`; develop `public field ParticleSystem weaponSmokeEmitterPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:61`.
- **PR wins** `field:woundEmitterPrefab`; PR `public field Emitter woundEmitterPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:57`; develop `public field Emitter woundEmitterPrefab` at `source-reference/Original/CharacterDependencies/Mission.cs:57`.
- **PR wins** ``method:AddOccupantsComponent`0()``; PR `public void AddOccupantsComponent()` at `source-reference/Original/CharacterDependencies/Mission.cs:357`; develop `public void AddOccupantsComponent()` at `source-reference/Original/CharacterDependencies/Mission.cs:357`.
- **PR wins** ``method:AddOperatorsComponent`0()``; PR `public void AddOperatorsComponent()` at `source-reference/Original/CharacterDependencies/Mission.cs:352`; develop `public void AddOperatorsComponent()` at `source-reference/Original/CharacterDependencies/Mission.cs:352`.
- **PR wins** ``method:ApplySettings`0()``; PR `private void ApplySettings()` at `source-reference/Original/CharacterDependencies/Mission.cs:417`; develop `private void ApplySettings()` at `source-reference/Original/CharacterDependencies/Mission.cs:417`.
- **PR wins** ``method:Awake`0()``; PR `private void Awake()` at `source-reference/Original/CharacterDependencies/Mission.cs:248`; develop `private void Awake()` at `source-reference/Original/CharacterDependencies/Mission.cs:248`.
- **PR wins** ``method:BeginMission`0()``; PR `public void BeginMission()` at `source-reference/Original/CharacterDependencies/Mission.cs:362`; develop `public void BeginMission()` at `source-reference/Original/CharacterDependencies/Mission.cs:362`.
- **PR wins** ``method:CreateContainers`0()``; PR `private void CreateContainers()` at `source-reference/Original/CharacterDependencies/Mission.cs:320`; develop `private void CreateContainers()` at `source-reference/Original/CharacterDependencies/Mission.cs:320`.
- **PR wins** ``method:EndMission`0(float)``; PR `public void EndMission(float)` at `source-reference/Original/CharacterDependencies/Mission.cs:519`; develop `public void EndMission(float)` at `source-reference/Original/CharacterDependencies/Mission.cs:519`.
- **PR wins** ``method:EntryTeamStandDown`0()``; PR `public void EntryTeamStandDown()` at `source-reference/Original/CharacterDependencies/Mission.cs:572`; develop `public void EntryTeamStandDown()` at `source-reference/Original/CharacterDependencies/Mission.cs:572`.
- **PR wins** ``method:GetSpatiallySortedBodies`0(bool, bool)``; PR `public List<CharacterBody> GetSpatiallySortedBodies(bool, bool)` at `source-reference/Original/CharacterDependencies/Mission.cs:765`; develop `public List<CharacterBody> GetSpatiallySortedBodies(bool, bool)` at `source-reference/Original/CharacterDependencies/Mission.cs:765`.
- **PR wins** ``method:LateUpdate`0()``; PR `private void LateUpdate()` at `source-reference/Original/CharacterDependencies/Mission.cs:389`; develop `private void LateUpdate()` at `source-reference/Original/CharacterDependencies/Mission.cs:389`.
- **PR wins** ``method:OnAllOccupantsDisabled`0()``; PR `private void OnAllOccupantsDisabled()` at `source-reference/Original/CharacterDependencies/Mission.cs:713`; develop `private void OnAllOccupantsDisabled()` at `source-reference/Original/CharacterDependencies/Mission.cs:713`.
- **PR wins** ``method:OnAllOccupantsRestrained`0()``; PR `private void OnAllOccupantsRestrained()` at `source-reference/Original/CharacterDependencies/Mission.cs:724`; develop `private void OnAllOccupantsRestrained()` at `source-reference/Original/CharacterDependencies/Mission.cs:724`.
- **PR wins** ``method:OnAllOperatorsDisabled`0()``; PR `private void OnAllOperatorsDisabled()` at `source-reference/Original/CharacterDependencies/Mission.cs:702`; develop `private void OnAllOperatorsDisabled()` at `source-reference/Original/CharacterDependencies/Mission.cs:702`.
- **PR wins** ``method:OnAmbientLightChanged`0(float)``; PR `private void OnAmbientLightChanged(float)` at `source-reference/Original/CharacterDependencies/Mission.cs:437`; develop `private void OnAmbientLightChanged(float)` at `source-reference/Original/CharacterDependencies/Mission.cs:437`.
- **PR wins** ``method:OnCensoringChanged`0(bool)``; PR `private void OnCensoringChanged(bool)` at `source-reference/Original/CharacterDependencies/Mission.cs:457`; develop `private void OnCensoringChanged(bool)` at `source-reference/Original/CharacterDependencies/Mission.cs:457`.
- **PR wins** ``method:OnDestroy`0()``; PR `private void OnDestroy()` at `source-reference/Original/CharacterDependencies/Mission.cs:227`; develop `private void OnDestroy()` at `source-reference/Original/CharacterDependencies/Mission.cs:227`.
- **PR wins** ``method:OnExtrasChanged`0(SerializableExtraSettings)``; PR `private void OnExtrasChanged(SerializableExtraSettings)` at `source-reference/Original/CharacterDependencies/Mission.cs:424`; develop `private void OnExtrasChanged(SerializableExtraSettings)` at `source-reference/Original/CharacterDependencies/Mission.cs:424`.
- **PR wins** ``method:OnShadowsChanged`0(int)``; PR `private void OnShadowsChanged(int)` at `source-reference/Original/CharacterDependencies/Mission.cs:445`; develop `private void OnShadowsChanged(int)` at `source-reference/Original/CharacterDependencies/Mission.cs:445`.
- **PR wins** ``method:PauseAndShowMenu`0()``; PR `public void PauseAndShowMenu()` at `source-reference/Original/CharacterDependencies/Mission.cs:680`; develop `public void PauseAndShowMenu()` at `source-reference/Original/CharacterDependencies/Mission.cs:680`.
- **PR wins** ``method:Pause`0()``; PR `public void Pause()` at `source-reference/Original/CharacterDependencies/Mission.cs:668`; develop `public void Pause()` at `source-reference/Original/CharacterDependencies/Mission.cs:668`.
- **PR wins** ``method:ReadPlayerInput`0()``; PR `private void ReadPlayerInput()` at `source-reference/Original/CharacterDependencies/Mission.cs:394`; develop `private void ReadPlayerInput()` at `source-reference/Original/CharacterDependencies/Mission.cs:394`.
- **PR wins** ``method:RecordMissionData`0(MissionResult, int)``; PR `private void RecordMissionData(MissionResult, int)` at `source-reference/Original/CharacterDependencies/Mission.cs:481`; develop `private void RecordMissionData(MissionResult, int)` at `source-reference/Original/CharacterDependencies/Mission.cs:481`.
- **PR wins** ``method:Restart`0()``; PR `public void Restart()` at `source-reference/Original/CharacterDependencies/Mission.cs:696`; develop `public void Restart()` at `source-reference/Original/CharacterDependencies/Mission.cs:696`.
- **PR wins** ``method:ResumeAndHideMenu`0()``; PR `public void ResumeAndHideMenu()` at `source-reference/Original/CharacterDependencies/Mission.cs:688`; develop `public void ResumeAndHideMenu()` at `source-reference/Original/CharacterDependencies/Mission.cs:688`.
- **PR wins** ``method:Resume`0()``; PR `public void Resume()` at `source-reference/Original/CharacterDependencies/Mission.cs:674`; develop `public void Resume()` at `source-reference/Original/CharacterDependencies/Mission.cs:674`.
- **PR wins** ``method:ReturnToTitle`0()``; PR `public void ReturnToTitle()` at `source-reference/Original/CharacterDependencies/Mission.cs:511`; develop `public void ReturnToTitle()` at `source-reference/Original/CharacterDependencies/Mission.cs:511`.
- **PR wins** ``method:RoomAtPosition`0(float)``; PR `public Room RoomAtPosition(float)` at `source-reference/Original/CharacterDependencies/Mission.cs:658`; develop `public Room RoomAtPosition(float)` at `source-reference/Original/CharacterDependencies/Mission.cs:658`.
- **PR wins** ``method:SetAmbientLightScale`0(float)``; PR `public void SetAmbientLightScale(float)` at `source-reference/Original/CharacterDependencies/Mission.cs:429`; develop `public void SetAmbientLightScale(float)` at `source-reference/Original/CharacterDependencies/Mission.cs:429`.
- **PR wins** ``method:SetLevelExtents`0()``; PR `private void SetLevelExtents()` at `source-reference/Original/CharacterDependencies/Mission.cs:635`; develop `private void SetLevelExtents()` at `source-reference/Original/CharacterDependencies/Mission.cs:635`.
- **PR wins** ``method:SetMode`0(MissionView)``; PR `public void SetMode(MissionView)` at `source-reference/Original/CharacterDependencies/Mission.cs:544`; develop `public void SetMode(MissionView)` at `source-reference/Original/CharacterDependencies/Mission.cs:544`.
- **PR wins** ``method:SetWalkableRange`0()``; PR `private void SetWalkableRange()` at `source-reference/Original/CharacterDependencies/Mission.cs:620`; develop `private void SetWalkableRange()` at `source-reference/Original/CharacterDependencies/Mission.cs:620`.
- **PR wins** ``method:SpawnCellPhone`0(Vector3)``; PR `public void SpawnCellPhone(Vector3)` at `source-reference/Original/CharacterDependencies/Mission.cs:749`; develop `public void SpawnCellPhone(Vector3)` at `source-reference/Original/CharacterDependencies/Mission.cs:749`.
- **PR wins** ``method:SpawnFragments`0(int, Vector3, AmmunitionBase, float, float, float, float, float, float, float, float, float, float)``; PR `public void SpawnFragments(int, Vector3, AmmunitionBase, float, float, float, float, float, float, float, float, float, float)` at `source-reference/Original/CharacterDependencies/Mission.cs:780`; develop `public void SpawnFragments(int, Vector3, AmmunitionBase, float, float, float, float, float, float, float, float, float, float)` at `source-reference/Original/CharacterDependencies/Mission.cs:780`.
- **PR wins** ``method:Start`0()``; PR `private void Start()` at `source-reference/Original/CharacterDependencies/Mission.cs:278`; develop `private void Start()` at `source-reference/Original/CharacterDependencies/Mission.cs:278`.
- **PR wins** ``method:TimeToString`0(float, bool)``; PR `public static string TimeToString(float, bool)` at `source-reference/Original/CharacterDependencies/Mission.cs:736`; develop `public static string TimeToString(float, bool)` at `source-reference/Original/CharacterDependencies/Mission.cs:736`.
- **PR wins** ``method:TriggerAllClear`0()``; PR `public void TriggerAllClear()` at `source-reference/Original/CharacterDependencies/Mission.cs:730`; develop `public void TriggerAllClear()` at `source-reference/Original/CharacterDependencies/Mission.cs:730`.
- **PR wins** ``method:UpdateMissionState`0()``; PR `private void UpdateMissionState()` at `source-reference/Original/CharacterDependencies/Mission.cs:578`; develop `private void UpdateMissionState()` at `source-reference/Original/CharacterDependencies/Mission.cs:578`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `source-reference/Original/CharacterDependencies/Mission.cs:380`; develop `private void Update()` at `source-reference/Original/CharacterDependencies/Mission.cs:380`.
- **PR wins** `property:ActionScheduler`; PR `public ActionScheduler ActionScheduler` at `source-reference/Original/CharacterDependencies/Mission.cs:144`; develop `public ActionScheduler ActionScheduler` at `source-reference/Original/CharacterDependencies/Mission.cs:144`.
- **PR wins** `property:AllClear`; PR `public bool AllClear` at `source-reference/Original/CharacterDependencies/Mission.cs:178`; develop `public bool AllClear` at `source-reference/Original/CharacterDependencies/Mission.cs:178`.
- **PR wins** `property:AllClearTimestamp`; PR `public float AllClearTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:184`; develop `public float AllClearTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:184`.
- **PR wins** `property:AllOperatorsDisabled`; PR `public bool AllOperatorsDisabled` at `source-reference/Original/CharacterDependencies/Mission.cs:180`; develop `public bool AllOperatorsDisabled` at `source-reference/Original/CharacterDependencies/Mission.cs:180`.
- **PR wins** `property:AllRoomsChecked`; PR `public bool AllRoomsChecked` at `source-reference/Original/CharacterDependencies/Mission.cs:174`; develop `public bool AllRoomsChecked` at `source-reference/Original/CharacterDependencies/Mission.cs:174`.
- **PR wins** `property:AllWeaponsSecured`; PR `public bool AllWeaponsSecured` at `source-reference/Original/CharacterDependencies/Mission.cs:176`; develop `public bool AllWeaponsSecured` at `source-reference/Original/CharacterDependencies/Mission.cs:176`.
- **PR wins** `property:Benchmark`; PR `public Benchmark Benchmark` at `source-reference/Original/CharacterDependencies/Mission.cs:136`; develop `public Benchmark Benchmark` at `source-reference/Original/CharacterDependencies/Mission.cs:136`.
- **PR wins** `property:BodyPartsContainer`; PR `public Transform BodyPartsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:162`; develop `public Transform BodyPartsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:162`.
- **PR wins** `property:CasingsContainer`; PR `public Transform CasingsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:160`; develop `public Transform CasingsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:160`.
- **PR wins** `property:CellPhone`; PR `public CellPhone CellPhone` at `source-reference/Original/CharacterDependencies/Mission.cs:132`; develop `public CellPhone CellPhone` at `source-reference/Original/CharacterDependencies/Mission.cs:132`.
- **PR wins** `property:CensorBoxContainer`; PR `public Transform CensorBoxContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:168`; develop `public Transform CensorBoxContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:168`.
- **PR wins** `property:Containers`; PR `public Transform Containers` at `source-reference/Original/CharacterDependencies/Mission.cs:150`; develop `public Transform Containers` at `source-reference/Original/CharacterDependencies/Mission.cs:150`.
- **PR wins** `property:Doors`; PR `public Doors Doors` at `source-reference/Original/CharacterDependencies/Mission.cs:130`; develop `public Doors Doors` at `source-reference/Original/CharacterDependencies/Mission.cs:130`.
- **PR wins** `property:EffectsContainer`; PR `public Transform EffectsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:164`; develop `public Transform EffectsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:164`.
- **PR wins** `property:EmittersContainer`; PR `public Transform EmittersContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:166`; develop `public Transform EmittersContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:166`.
- **PR wins** `property:EndPosition`; PR `public float EndPosition` at `source-reference/Original/CharacterDependencies/Mission.cs:192`; develop `public float EndPosition` at `source-reference/Original/CharacterDependencies/Mission.cs:192`.
- **PR wins** `property:EndTimestamp`; PR `public float EndTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:186`; develop `public float EndTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:186`.
- **PR wins** `property:FirstInjuryTimestamp`; PR `public float FirstInjuryTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:198`; develop `public float FirstInjuryTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:198`.
- **PR wins** `property:FloorPoolsContainer`; PR `public Transform FloorPoolsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:172`; develop `public Transform FloorPoolsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:172`.
- **PR wins** `property:GibsContainer`; PR `public Transform GibsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:170`; develop `public Transform GibsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:170`.
- **PR wins** `property:InteriorMode`; PR `public InteriorMode InteriorMode` at `source-reference/Original/CharacterDependencies/Mission.cs:134`; develop `public InteriorMode InteriorMode` at `source-reference/Original/CharacterDependencies/Mission.cs:134`.
- **PR wins** `property:LastWeaponFireTimestamp`; PR `public float LastWeaponFireTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:188`; develop `public float LastWeaponFireTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:188`.
- **PR wins** `property:LevelLength`; PR `public float LevelLength` at `source-reference/Original/CharacterDependencies/Mission.cs:194`; develop `public float LevelLength` at `source-reference/Original/CharacterDependencies/Mission.cs:194`.
- **PR wins** `property:LiveGrenadesContainer`; PR `public Transform LiveGrenadesContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:156`; develop `public Transform LiveGrenadesContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:156`.
- **PR wins** `property:Marksman`; PR `public Marksman Marksman` at `source-reference/Original/CharacterDependencies/Mission.cs:126`; develop `public Marksman Marksman` at `source-reference/Original/CharacterDependencies/Mission.cs:126`.
- **PR wins** `property:MarksmanMode`; PR `public MarksmanMode MarksmanMode` at `source-reference/Original/CharacterDependencies/Mission.cs:138`; develop `public MarksmanMode MarksmanMode` at `source-reference/Original/CharacterDependencies/Mission.cs:138`.
- **PR wins** `property:MissionStartTimestamp`; PR `public float MissionStartTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:182`; develop `public float MissionStartTimestamp` at `source-reference/Original/CharacterDependencies/Mission.cs:182`.
- **PR wins** `property:Mixer`; PR `public Mixer Mixer` at `source-reference/Original/CharacterDependencies/Mission.cs:122`; develop `public Mixer Mixer` at `source-reference/Original/CharacterDependencies/Mission.cs:122`.
- **PR wins** `property:Mode`; PR `public MissionMode Mode` at `source-reference/Original/CharacterDependencies/Mission.cs:213`; develop `public MissionMode Mode` at `source-reference/Original/CharacterDependencies/Mission.cs:213`.
- **PR wins** `property:Occupants`; PR `public Occupants Occupants` at `source-reference/Original/CharacterDependencies/Mission.cs:128`; develop `public Occupants Occupants` at `source-reference/Original/CharacterDependencies/Mission.cs:128`.
- **PR wins** `property:Operators`; PR `public Operators Operators` at `source-reference/Original/CharacterDependencies/Mission.cs:124`; develop `public Operators Operators` at `source-reference/Original/CharacterDependencies/Mission.cs:124`.
- **PR wins** `property:Paused`; PR `public bool Paused` at `source-reference/Original/CharacterDependencies/Mission.cs:196`; develop `public bool Paused` at `source-reference/Original/CharacterDependencies/Mission.cs:196`.
- **PR wins** `property:ProjectilesContainer`; PR `public Transform ProjectilesContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:154`; develop `public Transform ProjectilesContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:154`.
- **PR wins** `property:ScoreKeeper`; PR `public ScoreKeeper ScoreKeeper` at `source-reference/Original/CharacterDependencies/Mission.cs:120`; develop `public ScoreKeeper ScoreKeeper` at `source-reference/Original/CharacterDependencies/Mission.cs:120`.
- **PR wins** `property:SpentGrenadesContainer`; PR `public Transform SpentGrenadesContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:158`; develop `public Transform SpentGrenadesContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:158`.
- **PR wins** `property:StartPosition`; PR `public float StartPosition` at `source-reference/Original/CharacterDependencies/Mission.cs:190`; develop `public float StartPosition` at `source-reference/Original/CharacterDependencies/Mission.cs:190`.
- **PR wins** `property:Started`; PR `public bool Started` at `source-reference/Original/CharacterDependencies/Mission.cs:118`; develop `public bool Started` at `source-reference/Original/CharacterDependencies/Mission.cs:118`.
- **PR wins** `property:TimeScaler`; PR `public TimeScaler TimeScaler` at `source-reference/Original/CharacterDependencies/Mission.cs:140`; develop `public TimeScaler TimeScaler` at `source-reference/Original/CharacterDependencies/Mission.cs:140`.
- **PR wins** `property:TutorialMode`; PR `public TutorialMode TutorialMode` at `source-reference/Original/CharacterDependencies/Mission.cs:142`; develop `public TutorialMode TutorialMode` at `source-reference/Original/CharacterDependencies/Mission.cs:142`.
- **PR wins** `property:View`; PR `public MissionView View` at `source-reference/Original/CharacterDependencies/Mission.cs:215`; develop `public MissionView View` at `source-reference/Original/CharacterDependencies/Mission.cs:215`.
- **PR wins** `property:WeaponSmokeEmitter`; PR `public ParticleSystem WeaponSmokeEmitter` at `source-reference/Original/CharacterDependencies/Mission.cs:148`; develop `public ParticleSystem WeaponSmokeEmitter` at `source-reference/Original/CharacterDependencies/Mission.cs:148`.
- **PR wins** `property:WeaponsContainer`; PR `public Transform WeaponsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:152`; develop `public Transform WeaponsContainer` at `source-reference/Original/CharacterDependencies/Mission.cs:152`.
- **PR wins** `property:WoundEmitter`; PR `public Emitter WoundEmitter` at `source-reference/Original/CharacterDependencies/Mission.cs:146`; develop `public Emitter WoundEmitter` at `source-reference/Original/CharacterDependencies/Mission.cs:146`.


### 117. `Mission+MissionResult` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `MissionResult`; partial `false`; `source-reference/Original/CharacterDependencies/Mission.cs:16`

**develop declarations**:
- enum; namespace `(global)`; `MissionResult`; partial `false`; `source-reference/Original/CharacterDependencies/Mission.cs:16`

Common member signatures: **0**.


### 118. `Mission+MissionView` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `MissionView`; partial `false`; `source-reference/Original/CharacterDependencies/Mission.cs:8`

**develop declarations**:
- enum; namespace `(global)`; `MissionView`; partial `false`; `source-reference/Original/CharacterDependencies/Mission.cs:8`

Common member signatures: **0**.


### 119. `MissionHelper` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `MissionHelper`; partial `false`; `runtime/Engine/BallisticEngine.cs:8`

**develop declarations**:
- class; namespace `(global)`; `MissionHelper`; partial `false`; `runtime/Engine/BallisticEngine.cs:48`

Common member signatures: **2**.
- **PR wins** `field:EffectsContainer`; PR `public field Transform EffectsContainer` at `runtime/Engine/BallisticEngine.cs:8`; develop `public field Transform EffectsContainer` at `runtime/Engine/BallisticEngine.cs:48`.
- **PR wins** `field:Instance`; PR `public static field MissionHelper Instance` at `runtime/Engine/BallisticEngine.cs:8`; develop `public static field MissionHelper Instance` at `runtime/Engine/BallisticEngine.cs:48`.


### 120. `MuscleControl` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `MuscleControl`; partial `false`; `source-reference/Original/CharacterDependencies/MuscleControl.cs:3`

**develop declarations**:
- class; namespace `(global)`; `MuscleControl`; partial `false`; `source-reference/Original/CharacterDependencies/MuscleControl.cs:3`

Common member signatures: **31**.
- **PR wins** `field:body`; PR `private field CharacterBody body` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:5`; develop `private field CharacterBody body` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:5`.
- **PR wins** `field:crampDuration`; PR `private field float crampDuration` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:19`; develop `private field float crampDuration` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:19`.
- **PR wins** `field:decerberateLeft`; PR `private field bool decerberateLeft` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:25`; develop `private field bool decerberateLeft` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:25`.
- **PR wins** `field:decerberateRight`; PR `private field bool decerberateRight` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:27`; develop `private field bool decerberateRight` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:27`.
- **PR wins** `field:decorticate`; PR `private field bool decorticate` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:23`; develop `private field bool decorticate` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:23`.
- **PR wins** `field:singleSpasm`; PR `private field bool singleSpasm` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:21`; develop `private field bool singleSpasm` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:21`.
- **PR wins** `field:spasmDuration`; PR `private field float spasmDuration` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:15`; develop `private field float spasmDuration` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:15`.
- **PR wins** `field:spasmIntensity`; PR `private field float spasmIntensity` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:17`; develop `private field float spasmIntensity` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:17`.
- **PR wins** `field:spasmIntensityRange`; PR `private field Vector2 spasmIntensityRange` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:9`; develop `private field Vector2 spasmIntensityRange` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:9`.
- **PR wins** `field:spasmIntervalRange`; PR `private field Vector2 spasmIntervalRange` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:7`; develop `private field Vector2 spasmIntervalRange` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:7`.
- **PR wins** `field:spasmTimestamp`; PR `private field float spasmTimestamp` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:13`; develop `private field float spasmTimestamp` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:13`.
- **PR wins** `field:startTime`; PR `private field float startTime` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:11`; develop `private field float startTime` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:11`.
- **PR wins** ``method:ActivateCramp`0(float)``; PR `public void ActivateCramp(float)` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:108`; develop `public void ActivateCramp(float)` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:108`.
- **PR wins** ``method:ActivateMusclesOnRegainedConsciousness`0()``; PR `public void ActivateMusclesOnRegainedConsciousness()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:82`; develop `public void ActivateMusclesOnRegainedConsciousness()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:82`.
- **PR wins** ``method:AnimateCramp`0()``; PR `private void AnimateCramp()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:197`; develop `private void AnimateCramp()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:197`.
- **PR wins** ``method:AnimateDecerberate`0()``; PR `private void AnimateDecerberate()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:216`; develop `private void AnimateDecerberate()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:216`.
- **PR wins** ``method:AnimateDecorticate`0()``; PR `private void AnimateDecorticate()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:248`; develop `private void AnimateDecorticate()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:248`.
- **PR wins** ``method:AnimateSpasms`0()``; PR `private void AnimateSpasms()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:158`; develop `private void AnimateSpasms()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:158`.
- **PR wins** ``method:Awake`0()``; PR `private void Awake()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:33`; develop `private void Awake()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:33`.
- **PR wins** ``method:OnDeath`0()``; PR `public void OnDeath()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:273`; develop `public void OnDeath()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:273`.
- **PR wins** ``method:Relax`0()``; PR `public void Relax()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:146`; develop `public void Relax()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:146`.
- **PR wins** ``method:SetDecerberate`0()``; PR `public void SetDecerberate()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:123`; develop `public void SetDecerberate()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:123`.
- **PR wins** ``method:SetDecorticate`0()``; PR `public void SetDecorticate()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:116`; develop `public void SetDecorticate()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:116`.
- **PR wins** ``method:SingleImpulse`0(float, float)``; PR `public void SingleImpulse(float, float)` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:94`; develop `public void SingleImpulse(float, float)` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:94`.
- **PR wins** ``method:SingleSpasmOnBrainHit`0()``; PR `public void SingleSpasmOnBrainHit()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:58`; develop `public void SingleSpasmOnBrainHit()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:58`.
- **PR wins** ``method:StartHighIntensityMuscleSpasms`0()``; PR `public void StartHighIntensityMuscleSpasms()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:65`; develop `public void StartHighIntensityMuscleSpasms()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:65`.
- **PR wins** ``method:StartLowIntensityMuscleSpasms`0()``; PR `public void StartLowIntensityMuscleSpasms()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:72`; develop `public void StartLowIntensityMuscleSpasms()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:72`.
- **PR wins** ``method:StartSpasms`0(Vector2, Vector2)``; PR `private void StartSpasms(Vector2, Vector2)` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:88`; develop `private void StartSpasms(Vector2, Vector2)` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:88`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:38`; develop `private void Update()` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:38`.
- **PR wins** `property:CrampsActive`; PR `public bool CrampsActive` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:31`; develop `public bool CrampsActive` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:31`.
- **PR wins** `property:SpasmsActive`; PR `public bool SpasmsActive` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:29`; develop `public bool SpasmsActive` at `source-reference/Original/CharacterDependencies/MuscleControl.cs:29`.


### 121. `ObjectRemover` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `ObjectRemover`; partial `false`; `runtime/Original/ObjectRemover.cs:4`

**develop declarations**:
- class; namespace `(global)`; `ObjectRemover`; partial `false`; `runtime/Original/ObjectRemover.cs:4`

Common member signatures: **2**.
- **PR wins** `field:maxObjects`; PR `public field int maxObjects` at `runtime/Original/ObjectRemover.cs:6`; develop `public field int maxObjects` at `runtime/Original/ObjectRemover.cs:6`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `runtime/Original/ObjectRemover.cs:9`; develop `private void Update()` at `runtime/Original/ObjectRemover.cs:9`.


### 122. `PerformanceManager` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `PerformanceManager`; partial `true`; `runtime/Engine/BallisticEffects.cs:3`
- class; namespace `(global)`; `PerformanceManager`; partial `true`; `runtime/Original/Ballistics/EffectPool.Methods.cs:6`

**develop declarations**:
- class; namespace `(global)`; `PerformanceManager`; partial `true`; `runtime/Engine/BallisticEffects.cs:3`
- class; namespace `(global)`; `PerformanceManager`; partial `true`; `runtime/Original/Ballistics/EffectPool.Methods.cs:6`

Common member signatures: **10**.
- **PR wins** `field:Instance`; PR `public static field PerformanceManager Instance` at `runtime/Engine/BallisticEffects.cs:4`; develop `public static field PerformanceManager Instance` at `runtime/Engine/BallisticEffects.cs:4`.
- **PR wins** `field:MaxEnvironmentHitEffects`; PR `public field int MaxEnvironmentHitEffects` at `runtime/Engine/BallisticEffects.cs:5`; develop `public field int MaxEnvironmentHitEffects` at `runtime/Engine/BallisticEffects.cs:5`.
- **PR wins** `field:MaxEnvironmentHitMeshes`; PR `public field int MaxEnvironmentHitMeshes` at `runtime/Engine/BallisticEffects.cs:5`; develop `public field int MaxEnvironmentHitMeshes` at `runtime/Engine/BallisticEffects.cs:5`.
- **PR wins** `field:environmentHitEffects`; PR `readonly field LinkedList<GameObject> environmentHitEffects` at `runtime/Engine/BallisticEffects.cs:8`; develop `readonly field LinkedList<GameObject> environmentHitEffects` at `runtime/Engine/BallisticEffects.cs:8`.
- **PR wins** `field:environmentHitMeshes`; PR `readonly field LinkedList<GameObject> environmentHitMeshes` at `runtime/Engine/BallisticEffects.cs:8`; develop `readonly field LinkedList<GameObject> environmentHitMeshes` at `runtime/Engine/BallisticEffects.cs:8`.
- **PR wins** ``method:CreateEnvironmentalHitEffect`0(BallisticHitEffect)``; PR `public BallisticHitEffect CreateEnvironmentalHitEffect(BallisticHitEffect)` at `runtime/Original/Ballistics/EffectPool.Methods.cs:7`; develop `public BallisticHitEffect CreateEnvironmentalHitEffect(BallisticHitEffect)` at `runtime/Original/Ballistics/EffectPool.Methods.cs:7`.
- **PR wins** ``method:SpawnEnvironmentalHitMesh`0(BallisticHitMeshEffect, Projectile, Transform)``; PR `public BallisticHitMeshEffect SpawnEnvironmentalHitMesh(BallisticHitMeshEffect, Projectile, Transform)` at `runtime/Original/Ballistics/EffectPool.Methods.cs:25`; develop `public BallisticHitMeshEffect SpawnEnvironmentalHitMesh(BallisticHitMeshEffect, Projectile, Transform)` at `runtime/Original/Ballistics/EffectPool.Methods.cs:25`.
- **PR wins** `property:EnvironmentHitEffectsEnabled`; PR `public bool EnvironmentHitEffectsEnabled` at `runtime/Engine/BallisticEffects.cs:7`; develop `public bool EnvironmentHitEffectsEnabled` at `runtime/Engine/BallisticEffects.cs:7`.
- **PR wins** `property:UnlimitedEnvironmentHitEffects`; PR `public bool UnlimitedEnvironmentHitEffects` at `runtime/Engine/BallisticEffects.cs:6`; develop `public bool UnlimitedEnvironmentHitEffects` at `runtime/Engine/BallisticEffects.cs:6`.
- **PR wins** `property:UnlimitedEnvironmentHitMeshes`; PR `public bool UnlimitedEnvironmentHitMeshes` at `runtime/Engine/BallisticEffects.cs:6`; develop `public bool UnlimitedEnvironmentHitMeshes` at `runtime/Engine/BallisticEffects.cs:6`.


### 123. `Physiology` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `Physiology`; partial `false`; `source-reference/Original/CharacterDependencies/Physiology.cs:5`

**develop declarations**:
- class; namespace `(global)`; `Physiology`; partial `false`; `source-reference/Original/CharacterExecution/Physiology.cs:5`
- class; namespace `(global)`; `Physiology`; partial `false`; `source-reference/Original/ProjectileInteractions/Physiology.cs:5`

Common member signatures: **138**.
- **PR wins** `field:ConcussionBleedThreshold`; PR `private const field float ConcussionBleedThreshold` at `source-reference/Original/CharacterDependencies/Physiology.cs:127`; develop `private const field float ConcussionBleedThreshold` at `source-reference/Original/ProjectileInteractions/Physiology.cs:127`, `source-reference/Original/CharacterExecution/Physiology.cs:127`.
- **PR wins** `field:GauzeFillVolume`; PR `public const field float GauzeFillVolume` at `source-reference/Original/CharacterDependencies/Physiology.cs:22`; develop `public const field float GauzeFillVolume` at `source-reference/Original/ProjectileInteractions/Physiology.cs:22`, `source-reference/Original/CharacterExecution/Physiology.cs:22`.
- **PR wins** `field:MaxConsciousConcussion`; PR `private const field float MaxConsciousConcussion` at `source-reference/Original/CharacterDependencies/Physiology.cs:129`; develop `private const field float MaxConsciousConcussion` at `source-reference/Original/ProjectileInteractions/Physiology.cs:129`, `source-reference/Original/CharacterExecution/Physiology.cs:129`.
- **PR wins** `field:MaxHemorrhagingScore`; PR `private const field float MaxHemorrhagingScore` at `source-reference/Original/CharacterDependencies/Physiology.cs:141`; develop `private const field float MaxHemorrhagingScore` at `source-reference/Original/ProjectileInteractions/Physiology.cs:141`, `source-reference/Original/CharacterExecution/Physiology.cs:141`.
- **PR wins** `field:MaxShock`; PR `private const field float MaxShock` at `source-reference/Original/CharacterDependencies/Physiology.cs:123`; develop `private const field float MaxShock` at `source-reference/Original/ProjectileInteractions/Physiology.cs:123`, `source-reference/Original/CharacterExecution/Physiology.cs:123`.
- **PR wins** `field:MaxTraumaScore`; PR `private const field float MaxTraumaScore` at `source-reference/Original/CharacterDependencies/Physiology.cs:143`; develop `private const field float MaxTraumaScore` at `source-reference/Original/ProjectileInteractions/Physiology.cs:143`, `source-reference/Original/CharacterExecution/Physiology.cs:143`.
- **PR wins** `field:MinConsciousBloodOxygen`; PR `private const field float MinConsciousBloodOxygen` at `source-reference/Original/CharacterDependencies/Physiology.cs:135`; develop `private const field float MinConsciousBloodOxygen` at `source-reference/Original/ProjectileInteractions/Physiology.cs:135`, `source-reference/Original/CharacterExecution/Physiology.cs:135`.
- **PR wins** `field:MuscleSpasmsOxygenLevel`; PR `private const field float MuscleSpasmsOxygenLevel` at `source-reference/Original/CharacterDependencies/Physiology.cs:133`; develop `private const field float MuscleSpasmsOxygenLevel` at `source-reference/Original/ProjectileInteractions/Physiology.cs:133`, `source-reference/Original/CharacterExecution/Physiology.cs:133`.
- **PR wins** `field:RegainConsciousnessBloodOxygen`; PR `private const field float RegainConsciousnessBloodOxygen` at `source-reference/Original/CharacterDependencies/Physiology.cs:137`; develop `private const field float RegainConsciousnessBloodOxygen` at `source-reference/Original/ProjectileInteractions/Physiology.cs:137`, `source-reference/Original/CharacterExecution/Physiology.cs:137`.
- **PR wins** `field:RegainConsciousnessConcussion`; PR `private const field float RegainConsciousnessConcussion` at `source-reference/Original/CharacterDependencies/Physiology.cs:131`; develop `private const field float RegainConsciousnessConcussion` at `source-reference/Original/ProjectileInteractions/Physiology.cs:131`, `source-reference/Original/CharacterExecution/Physiology.cs:131`.
- **PR wins** `field:RegainConsciousnessShock`; PR `private const field float RegainConsciousnessShock` at `source-reference/Original/CharacterDependencies/Physiology.cs:125`; develop `private const field float RegainConsciousnessShock` at `source-reference/Original/ProjectileInteractions/Physiology.cs:125`, `source-reference/Original/CharacterExecution/Physiology.cs:125`.
- **PR wins** `field:RestHeartRate`; PR `public const field float RestHeartRate` at `source-reference/Original/CharacterDependencies/Physiology.cs:24`; develop `public const field float RestHeartRate` at `source-reference/Original/ProjectileInteractions/Physiology.cs:24`, `source-reference/Original/CharacterExecution/Physiology.cs:24`.
- **PR wins** `field:RestRespirationRate`; PR `public const field float RestRespirationRate` at `source-reference/Original/CharacterDependencies/Physiology.cs:26`; develop `public const field float RestRespirationRate` at `source-reference/Original/ProjectileInteractions/Physiology.cs:26`, `source-reference/Original/CharacterExecution/Physiology.cs:26`.
- **PR wins** `field:SpasmsOnLowOxygenFraction`; PR `private const field float SpasmsOnLowOxygenFraction` at `source-reference/Original/CharacterDependencies/Physiology.cs:139`; develop `private const field float SpasmsOnLowOxygenFraction` at `source-reference/Original/ProjectileInteractions/Physiology.cs:139`, `source-reference/Original/CharacterExecution/Physiology.cs:139`.
- **PR wins** `field:StunResponseThreshold`; PR `private const field float StunResponseThreshold` at `source-reference/Original/CharacterDependencies/Physiology.cs:145`; develop `private const field float StunResponseThreshold` at `source-reference/Original/ProjectileInteractions/Physiology.cs:145`, `source-reference/Original/CharacterExecution/Physiology.cs:145`.
- **PR wins** `field:bloodDropOxygen`; PR `private field float bloodDropOxygen` at `source-reference/Original/CharacterDependencies/Physiology.cs:115`; develop `private field float bloodDropOxygen` at `source-reference/Original/ProjectileInteractions/Physiology.cs:115`, `source-reference/Original/CharacterExecution/Physiology.cs:115`.
- **PR wins** `field:bloodSquirtAudioVolume`; PR `private field float bloodSquirtAudioVolume` at `source-reference/Original/CharacterDependencies/Physiology.cs:121`; develop `private field float bloodSquirtAudioVolume` at `source-reference/Original/ProjectileInteractions/Physiology.cs:121`, `source-reference/Original/CharacterExecution/Physiology.cs:121`.
- **PR wins** `field:body`; PR `private field CharacterBody body` at `source-reference/Original/CharacterDependencies/Physiology.cs:81`; develop `private field CharacterBody body` at `source-reference/Original/ProjectileInteractions/Physiology.cs:81`, `source-reference/Original/CharacterExecution/Physiology.cs:81`.
- **PR wins** `field:brainDamageFromLackOfOxygenSeconds`; PR `public field float brainDamageFromLackOfOxygenSeconds` at `source-reference/Original/CharacterDependencies/Physiology.cs:68`; develop `public field float brainDamageFromLackOfOxygenSeconds` at `source-reference/Original/ProjectileInteractions/Physiology.cs:68`, `source-reference/Original/CharacterExecution/Physiology.cs:68`.
- **PR wins** `field:brainDamageRateWithoutOxygen`; PR `public field float brainDamageRateWithoutOxygen` at `source-reference/Original/CharacterDependencies/Physiology.cs:72`; develop `public field float brainDamageRateWithoutOxygen` at `source-reference/Original/ProjectileInteractions/Physiology.cs:72`, `source-reference/Original/CharacterExecution/Physiology.cs:72`.
- **PR wins** `field:brainDeathFromLackOfOxygenSeconds`; PR `public field float brainDeathFromLackOfOxygenSeconds` at `source-reference/Original/CharacterDependencies/Physiology.cs:75`; develop `public field float brainDeathFromLackOfOxygenSeconds` at `source-reference/Original/ProjectileInteractions/Physiology.cs:75`, `source-reference/Original/CharacterExecution/Physiology.cs:75`.
- **PR wins** `field:brainTissue`; PR `private field float brainTissue` at `source-reference/Original/CharacterDependencies/Physiology.cs:89`; develop `private field float brainTissue` at `source-reference/Original/ProjectileInteractions/Physiology.cs:89`, `source-reference/Original/CharacterExecution/Physiology.cs:89`.
- **PR wins** `field:concussion`; PR `private field float concussion` at `source-reference/Original/CharacterDependencies/Physiology.cs:93`; develop `private field float concussion` at `source-reference/Original/ProjectileInteractions/Physiology.cs:93`, `source-reference/Original/CharacterExecution/Physiology.cs:93`.
- **PR wins** `field:concussionCurve`; PR `public field AnimationCurve concussionCurve` at `source-reference/Original/CharacterDependencies/Physiology.cs:65`; develop `public field AnimationCurve concussionCurve` at `source-reference/Original/ProjectileInteractions/Physiology.cs:65`, `source-reference/Original/CharacterExecution/Physiology.cs:65`.
- **PR wins** `field:fitness`; PR `public field float fitness` at `source-reference/Original/CharacterDependencies/Physiology.cs:56`; develop `public field float fitness` at `source-reference/Original/ProjectileInteractions/Physiology.cs:56`, `source-reference/Original/CharacterExecution/Physiology.cs:56`.
- **PR wins** `field:heartCapacity`; PR `public field float heartCapacity` at `source-reference/Original/CharacterDependencies/Physiology.cs:28`; develop `public field float heartCapacity` at `source-reference/Original/ProjectileInteractions/Physiology.cs:28`, `source-reference/Original/CharacterExecution/Physiology.cs:28`.
- **PR wins** `field:heartCompensationThreshold`; PR `private field float heartCompensationThreshold` at `source-reference/Original/CharacterDependencies/Physiology.cs:119`; develop `private field float heartCompensationThreshold` at `source-reference/Original/ProjectileInteractions/Physiology.cs:119`, `source-reference/Original/CharacterExecution/Physiology.cs:119`.
- **PR wins** `field:heartOxygenDeliver`; PR `public field float heartOxygenDeliver` at `source-reference/Original/CharacterDependencies/Physiology.cs:40`; develop `public field float heartOxygenDeliver` at `source-reference/Original/ProjectileInteractions/Physiology.cs:40`, `source-reference/Original/CharacterExecution/Physiology.cs:40`.
- **PR wins** `field:heartOxygenTake`; PR `public field float heartOxygenTake` at `source-reference/Original/CharacterDependencies/Physiology.cs:36`; develop `public field float heartOxygenTake` at `source-reference/Original/ProjectileInteractions/Physiology.cs:36`, `source-reference/Original/CharacterExecution/Physiology.cs:36`.
- **PR wins** `field:heartRate`; PR `private field float heartRate` at `source-reference/Original/CharacterDependencies/Physiology.cs:95`; develop `private field float heartRate` at `source-reference/Original/ProjectileInteractions/Physiology.cs:95`, `source-reference/Original/CharacterExecution/Physiology.cs:95`.
- **PR wins** `field:heartRateCompensation`; PR `private field float heartRateCompensation` at `source-reference/Original/CharacterDependencies/Physiology.cs:117`; develop `private field float heartRateCompensation` at `source-reference/Original/ProjectileInteractions/Physiology.cs:117`, `source-reference/Original/CharacterExecution/Physiology.cs:117`.
- **PR wins** `field:heartRateMax`; PR `public field float heartRateMax` at `source-reference/Original/CharacterDependencies/Physiology.cs:61`; develop `public field float heartRateMax` at `source-reference/Original/ProjectileInteractions/Physiology.cs:61`, `source-reference/Original/CharacterExecution/Physiology.cs:61`.
- **PR wins** `field:heartbeatTimestamp`; PR `private field float heartbeatTimestamp` at `source-reference/Original/CharacterDependencies/Physiology.cs:109`; develop `private field float heartbeatTimestamp` at `source-reference/Original/ProjectileInteractions/Physiology.cs:109`, `source-reference/Original/CharacterExecution/Physiology.cs:109`.
- **PR wins** `field:hitAnnouncements`; PR `private field UIHitAnnouncements hitAnnouncements` at `source-reference/Original/CharacterDependencies/Physiology.cs:83`; develop `private field UIHitAnnouncements hitAnnouncements` at `source-reference/Original/ProjectileInteractions/Physiology.cs:83`, `source-reference/Original/CharacterExecution/Physiology.cs:83`.
- **PR wins** `field:invulnerable`; PR `public field bool invulnerable` at `source-reference/Original/CharacterDependencies/Physiology.cs:78`; develop `public field bool invulnerable` at `source-reference/Original/ProjectileInteractions/Physiology.cs:78`, `source-reference/Original/CharacterExecution/Physiology.cs:78`.
- **PR wins** `field:isInhaling`; PR `private field bool isInhaling` at `source-reference/Original/CharacterDependencies/Physiology.cs:107`; develop `private field bool isInhaling` at `source-reference/Original/ProjectileInteractions/Physiology.cs:107`, `source-reference/Original/CharacterExecution/Physiology.cs:107`.
- **PR wins** `field:lastBrainOxygenTimestamp`; PR `private field float lastBrainOxygenTimestamp` at `source-reference/Original/CharacterDependencies/Physiology.cs:113`; develop `private field float lastBrainOxygenTimestamp` at `source-reference/Original/ProjectileInteractions/Physiology.cs:113`, `source-reference/Original/CharacterExecution/Physiology.cs:113`.
- **PR wins** `field:lungsLeakage`; PR `private field float lungsLeakage` at `source-reference/Original/CharacterDependencies/Physiology.cs:103`; develop `private field float lungsLeakage` at `source-reference/Original/ProjectileInteractions/Physiology.cs:103`, `source-reference/Original/CharacterExecution/Physiology.cs:103`.
- **PR wins** `field:maxAliveBrainTrauma`; PR `private field float maxAliveBrainTrauma` at `source-reference/Original/CharacterDependencies/Physiology.cs:87`; develop `private field float maxAliveBrainTrauma` at `source-reference/Original/ProjectileInteractions/Physiology.cs:87`, `source-reference/Original/CharacterExecution/Physiology.cs:87`.
- **PR wins** `field:maxConsciousBrainTrauma`; PR `private field float maxConsciousBrainTrauma` at `source-reference/Original/CharacterDependencies/Physiology.cs:85`; develop `private field float maxConsciousBrainTrauma` at `source-reference/Original/ProjectileInteractions/Physiology.cs:85`, `source-reference/Original/CharacterExecution/Physiology.cs:85`.
- **PR wins** `field:respirationDepth`; PR `private field float respirationDepth` at `source-reference/Original/CharacterDependencies/Physiology.cs:99`; develop `private field float respirationDepth` at `source-reference/Original/ProjectileInteractions/Physiology.cs:99`, `source-reference/Original/CharacterExecution/Physiology.cs:99`.
- **PR wins** `field:respirationDepthAdaptionRate`; PR `public field float respirationDepthAdaptionRate` at `source-reference/Original/CharacterDependencies/Physiology.cs:52`; develop `public field float respirationDepthAdaptionRate` at `source-reference/Original/ProjectileInteractions/Physiology.cs:52`, `source-reference/Original/CharacterExecution/Physiology.cs:52`.
- **PR wins** `field:respirationRate`; PR `private field float respirationRate` at `source-reference/Original/CharacterDependencies/Physiology.cs:97`; develop `private field float respirationRate` at `source-reference/Original/ProjectileInteractions/Physiology.cs:97`, `source-reference/Original/CharacterExecution/Physiology.cs:97`.
- **PR wins** `field:respirationRateAdaptionRate`; PR `public field float respirationRateAdaptionRate` at `source-reference/Original/CharacterDependencies/Physiology.cs:48`; develop `public field float respirationRateAdaptionRate` at `source-reference/Original/ProjectileInteractions/Physiology.cs:48`, `source-reference/Original/CharacterExecution/Physiology.cs:48`.
- **PR wins** `field:respirationRateMax`; PR `public field float respirationRateMax` at `source-reference/Original/CharacterDependencies/Physiology.cs:63`; develop `public field float respirationRateMax` at `source-reference/Original/ProjectileInteractions/Physiology.cs:63`, `source-reference/Original/CharacterExecution/Physiology.cs:63`.
- **PR wins** `field:respirationTimestamp`; PR `private field float respirationTimestamp` at `source-reference/Original/CharacterDependencies/Physiology.cs:111`; develop `private field float respirationTimestamp` at `source-reference/Original/ProjectileInteractions/Physiology.cs:111`, `source-reference/Original/CharacterExecution/Physiology.cs:111`.
- **PR wins** `field:respirationToLungsFactor`; PR `public field float respirationToLungsFactor` at `source-reference/Original/CharacterDependencies/Physiology.cs:32`; develop `public field float respirationToLungsFactor` at `source-reference/Original/ProjectileInteractions/Physiology.cs:32`, `source-reference/Original/CharacterExecution/Physiology.cs:32`.
- **PR wins** `field:skipHeartBeat`; PR `private field bool skipHeartBeat` at `source-reference/Original/CharacterDependencies/Physiology.cs:105`; develop `private field bool skipHeartBeat` at `source-reference/Original/ProjectileInteractions/Physiology.cs:105`, `source-reference/Original/CharacterExecution/Physiology.cs:105`.
- **PR wins** `field:startingBrainTissue`; PR `private field float startingBrainTissue` at `source-reference/Original/CharacterDependencies/Physiology.cs:91`; develop `private field float startingBrainTissue` at `source-reference/Original/ProjectileInteractions/Physiology.cs:91`, `source-reference/Original/CharacterExecution/Physiology.cs:91`.
- **PR wins** `field:startingHeartCapacity`; PR `private field float startingHeartCapacity` at `source-reference/Original/CharacterDependencies/Physiology.cs:101`; develop `private field float startingHeartCapacity` at `source-reference/Original/ProjectileInteractions/Physiology.cs:101`, `source-reference/Original/CharacterExecution/Physiology.cs:101`.
- **PR wins** `field:workOxygenFactor`; PR `public field float workOxygenFactor` at `source-reference/Original/CharacterDependencies/Physiology.cs:44`; develop `public field float workOxygenFactor` at `source-reference/Original/ProjectileInteractions/Physiology.cs:44`, `source-reference/Original/CharacterExecution/Physiology.cs:44`.
- **PR wins** ``method:AddBrainTrauma`0(float, bool)``; PR `public void AddBrainTrauma(float, bool)` at `source-reference/Original/CharacterDependencies/Physiology.cs:423`; develop `public void AddBrainTrauma(float, bool)` at `source-reference/Original/ProjectileInteractions/Physiology.cs:423`, `source-reference/Original/CharacterExecution/Physiology.cs:339`.
- **PR wins** ``method:AddBrainTrauma`0(float, float, bool)``; PR `public void AddBrainTrauma(float, float, bool)` at `source-reference/Original/CharacterDependencies/Physiology.cs:457`; develop `public void AddBrainTrauma(float, float, bool)` at `source-reference/Original/ProjectileInteractions/Physiology.cs:457`, `source-reference/Original/CharacterExecution/Physiology.cs:373`.
- **PR wins** ``method:AddConcussion`0(float)``; PR `public void AddConcussion(float)` at `source-reference/Original/CharacterDependencies/Physiology.cs:393`; develop `public void AddConcussion(float)` at `source-reference/Original/ProjectileInteractions/Physiology.cs:393`, `source-reference/Original/CharacterExecution/Physiology.cs:309`.
- **PR wins** ``method:AddConcussion`0(float, float)``; PR `public void AddConcussion(float, float)` at `source-reference/Original/CharacterDependencies/Physiology.cs:415`; develop `public void AddConcussion(float, float)` at `source-reference/Original/ProjectileInteractions/Physiology.cs:415`, `source-reference/Original/CharacterExecution/Physiology.cs:331`.
- **PR wins** ``method:AddHeartTrauma`0(float, float)``; PR `public void AddHeartTrauma(float, float)` at `source-reference/Original/CharacterDependencies/Physiology.cs:472`; develop `public void AddHeartTrauma(float, float)` at `source-reference/Original/ProjectileInteractions/Physiology.cs:472`, `source-reference/Original/CharacterExecution/Physiology.cs:388`.
- **PR wins** ``method:AddLungsTrauma`0(float, float)``; PR `public void AddLungsTrauma(float, float)` at `source-reference/Original/CharacterDependencies/Physiology.cs:465`; develop `public void AddLungsTrauma(float, float)` at `source-reference/Original/ProjectileInteractions/Physiology.cs:465`, `source-reference/Original/CharacterExecution/Physiology.cs:381`.
- **PR wins** ``method:AddShock`0(float)``; PR `public void AddShock(float)` at `source-reference/Original/CharacterDependencies/Physiology.cs:388`; develop `public void AddShock(float)` at `source-reference/Original/ProjectileInteractions/Physiology.cs:388`, `source-reference/Original/CharacterExecution/Physiology.cs:304`.
- **PR wins** ``method:AnnounceHit`0(string, Projectile)``; PR `public void AnnounceHit(string, Projectile)` at `source-reference/Original/CharacterDependencies/Physiology.cs:994`; develop `public void AnnounceHit(string, Projectile)` at `source-reference/Original/ProjectileInteractions/Physiology.cs:994`, `source-reference/Original/CharacterExecution/Physiology.cs:910`.
- **PR wins** ``method:Awake`0()``; PR `private void Awake()` at `source-reference/Original/CharacterDependencies/Physiology.cs:637`; develop `private void Awake()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:637`, `source-reference/Original/CharacterExecution/Physiology.cs:553`.
- **PR wins** ``method:HemorrhagingScore`0()``; PR `public float HemorrhagingScore()` at `source-reference/Original/CharacterDependencies/Physiology.cs:963`; develop `public float HemorrhagingScore()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:963`, `source-reference/Original/CharacterExecution/Physiology.cs:879`.
- **PR wins** ``method:OnJointBroken`0()``; PR `public void OnJointBroken()` at `source-reference/Original/CharacterDependencies/Physiology.cs:520`; develop `public void OnJointBroken()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:520`, `source-reference/Original/CharacterExecution/Physiology.cs:436`.
- **PR wins** ``method:PlayBloodSquirtAudio`0()``; PR `private void PlayBloodSquirtAudio()` at `source-reference/Original/CharacterDependencies/Physiology.cs:1014`; develop `private void PlayBloodSquirtAudio()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:1014`, `source-reference/Original/CharacterExecution/Physiology.cs:930`.
- **PR wins** ``method:PlayInhalationAudio`0()``; PR `private void PlayInhalationAudio()` at `source-reference/Original/CharacterDependencies/Physiology.cs:1002`; develop `private void PlayInhalationAudio()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:1002`, `source-reference/Original/CharacterExecution/Physiology.cs:918`.
- **PR wins** ``method:RemoveBlood`0(float)``; PR `public void RemoveBlood(float)` at `source-reference/Original/CharacterDependencies/Physiology.cs:501`; develop `public void RemoveBlood(float)` at `source-reference/Original/ProjectileInteractions/Physiology.cs:501`, `source-reference/Original/CharacterExecution/Physiology.cs:417`.
- **PR wins** ``method:RemoveLungsOxygen`0(float)``; PR `public void RemoveLungsOxygen(float)` at `source-reference/Original/CharacterDependencies/Physiology.cs:507`; develop `public void RemoveLungsOxygen(float)` at `source-reference/Original/ProjectileInteractions/Physiology.cs:507`, `source-reference/Original/CharacterExecution/Physiology.cs:423`.
- **PR wins** ``method:Respirate`0()``; PR `private void Respirate()` at `source-reference/Original/CharacterDependencies/Physiology.cs:812`; develop `private void Respirate()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:812`, `source-reference/Original/CharacterExecution/Physiology.cs:728`.
- **PR wins** ``method:Score`0()``; PR `public float Score()` at `source-reference/Original/CharacterDependencies/Physiology.cs:528`; develop `public float Score()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:528`, `source-reference/Original/CharacterExecution/Physiology.cs:444`.
- **PR wins** ``method:SetBloodOxygen`0()``; PR `private void SetBloodOxygen()` at `source-reference/Original/CharacterDependencies/Physiology.cs:858`; develop `private void SetBloodOxygen()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:858`, `source-reference/Original/CharacterExecution/Physiology.cs:774`.
- **PR wins** ``method:SetBloodPressure`0()``; PR `private void SetBloodPressure()` at `source-reference/Original/CharacterDependencies/Physiology.cs:775`; develop `private void SetBloodPressure()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:775`, `source-reference/Original/CharacterExecution/Physiology.cs:691`.
- **PR wins** ``method:SetBrainOxygen`0()``; PR `private void SetBrainOxygen()` at `source-reference/Original/CharacterDependencies/Physiology.cs:863`; develop `private void SetBrainOxygen()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:863`, `source-reference/Original/CharacterExecution/Physiology.cs:779`.
- **PR wins** ``method:SetCharacterState`0()``; PR `private void SetCharacterState()` at `source-reference/Original/CharacterDependencies/Physiology.cs:885`; develop `private void SetCharacterState()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:885`, `source-reference/Original/CharacterExecution/Physiology.cs:801`.
- **PR wins** ``method:SetDead`0()``; PR `public void SetDead()` at `source-reference/Original/CharacterDependencies/Physiology.cs:951`; develop `public void SetDead()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:951`, `source-reference/Original/CharacterExecution/Physiology.cs:867`.
- **PR wins** ``method:SetHeartRate`0()``; PR `private void SetHeartRate()` at `source-reference/Original/CharacterDependencies/Physiology.cs:687`; develop `private void SetHeartRate()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:687`, `source-reference/Original/CharacterExecution/Physiology.cs:603`.
- **PR wins** ``method:SetHemorrhaging`0()``; PR `private void SetHemorrhaging()` at `source-reference/Original/CharacterDependencies/Physiology.cs:798`; develop `private void SetHemorrhaging()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:798`, `source-reference/Original/CharacterExecution/Physiology.cs:714`.
- **PR wins** ``method:SetParalyzed`0()``; PR `public void SetParalyzed()` at `source-reference/Original/CharacterDependencies/Physiology.cs:491`; develop `public void SetParalyzed()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:491`, `source-reference/Original/CharacterExecution/Physiology.cs:407`.
- **PR wins** ``method:SetRagdoll`0()``; PR `public void SetRagdoll()` at `source-reference/Original/CharacterDependencies/Physiology.cs:942`; develop `public void SetRagdoll()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:942`, `source-reference/Original/CharacterExecution/Physiology.cs:858`.
- **PR wins** ``method:SetRespirationRate`0()``; PR `private void SetRespirationRate()` at `source-reference/Original/CharacterDependencies/Physiology.cs:757`; develop `private void SetRespirationRate()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:757`, `source-reference/Original/CharacterExecution/Physiology.cs:673`.
- **PR wins** ``method:SetShockValue`0()``; PR `private void SetShockValue()` at `source-reference/Original/CharacterDependencies/Physiology.cs:875`; develop `private void SetShockValue()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:875`, `source-reference/Original/CharacterExecution/Physiology.cs:791`.
- **PR wins** ``method:SetUnconscious`0()``; PR `public void SetUnconscious()` at `source-reference/Original/CharacterDependencies/Physiology.cs:931`; develop `public void SetUnconscious()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:931`, `source-reference/Original/CharacterExecution/Physiology.cs:847`.
- **PR wins** ``method:SetWorkLevel`0()``; PR `private void SetWorkLevel()` at `source-reference/Original/CharacterDependencies/Physiology.cs:677`; develop `private void SetWorkLevel()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:677`, `source-reference/Original/CharacterExecution/Physiology.cs:593`.
- **PR wins** ``method:ShouldLoseConsciousness`0()``; PR `public bool ShouldLoseConsciousness()` at `source-reference/Original/CharacterDependencies/Physiology.cs:372`; develop `public bool ShouldLoseConsciousness()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:372`, `source-reference/Original/CharacterExecution/Physiology.cs:288`.
- **PR wins** ``method:ShouldLoseFooting`0()``; PR `public bool ShouldLoseFooting()` at `source-reference/Original/CharacterDependencies/Physiology.cs:367`; develop `public bool ShouldLoseFooting()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:367`, `source-reference/Original/CharacterExecution/Physiology.cs:283`.
- **PR wins** ``method:ShouldRegainConsciousness`0()``; PR `public bool ShouldRegainConsciousness()` at `source-reference/Original/CharacterDependencies/Physiology.cs:380`; develop `public bool ShouldRegainConsciousness()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:380`, `source-reference/Original/CharacterExecution/Physiology.cs:296`.
- **PR wins** ``method:StopHemorrhage`0()``; PR `public void StopHemorrhage()` at `source-reference/Original/CharacterDependencies/Physiology.cs:512`; develop `public void StopHemorrhage()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:512`, `source-reference/Original/CharacterExecution/Physiology.cs:428`.
- **PR wins** ``method:Summary`0()``; PR `public string Summary()` at `source-reference/Original/CharacterDependencies/Physiology.cs:533`; develop `public string Summary()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:533`, `source-reference/Original/CharacterExecution/Physiology.cs:449`.
- **PR wins** ``method:TraumaScore`0()``; PR `private float TraumaScore()` at `source-reference/Original/CharacterDependencies/Physiology.cs:976`; develop `private float TraumaScore()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:976`, `source-reference/Original/CharacterExecution/Physiology.cs:892`.
- **PR wins** ``method:UpdateHeart`0()``; PR `private void UpdateHeart()` at `source-reference/Original/CharacterDependencies/Physiology.cs:705`; develop `private void UpdateHeart()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:705`, `source-reference/Original/CharacterExecution/Physiology.cs:621`.
- **PR wins** ``method:UpdateLimbs`0()``; PR `private void UpdateLimbs()` at `source-reference/Original/CharacterDependencies/Physiology.cs:783`; develop `private void UpdateLimbs()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:783`, `source-reference/Original/CharacterExecution/Physiology.cs:699`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `source-reference/Original/CharacterDependencies/Physiology.cs:655`; develop `private void Update()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:655`, `source-reference/Original/CharacterExecution/Physiology.cs:571`.
- **PR wins** ``method:VitalFunctionsScore`0()``; PR `private float VitalFunctionsScore()` at `source-reference/Original/CharacterDependencies/Physiology.cs:989`; develop `private float VitalFunctionsScore()` at `source-reference/Original/ProjectileInteractions/Physiology.cs:989`, `source-reference/Original/CharacterExecution/Physiology.cs:905`.
- **PR wins** `property:Alive`; PR `public bool Alive` at `source-reference/Original/CharacterDependencies/Physiology.cs:147`; develop `public bool Alive` at `source-reference/Original/ProjectileInteractions/Physiology.cs:147`, `source-reference/Original/CharacterExecution/Physiology.cs:147`.
- **PR wins** `property:AnnouncePosition`; PR `public Vector3 AnnouncePosition` at `source-reference/Original/CharacterDependencies/Physiology.cs:315`; develop `public Vector3 AnnouncePosition` at `source-reference/Original/ProjectileInteractions/Physiology.cs:315`, `source-reference/Original/CharacterExecution/Physiology.cs:249`.
- **PR wins** `property:AudibleRespiration`; PR `public bool AudibleRespiration` at `source-reference/Original/CharacterDependencies/Physiology.cs:317`; develop `public bool AudibleRespiration` at `source-reference/Original/ProjectileInteractions/Physiology.cs:317`, `source-reference/Original/CharacterExecution/Physiology.cs:251`.
- **PR wins** `property:BloodOxygen`; PR `public float BloodOxygen` at `source-reference/Original/CharacterDependencies/Physiology.cs:231`; develop `public float BloodOxygen` at `source-reference/Original/ProjectileInteractions/Physiology.cs:231`, `source-reference/Original/CharacterExecution/Physiology.cs:207`.
- **PR wins** `property:BloodPressure`; PR `public float BloodPressure` at `source-reference/Original/CharacterDependencies/Physiology.cs:209`; develop `public float BloodPressure` at `source-reference/Original/ProjectileInteractions/Physiology.cs:209`, `source-reference/Original/CharacterExecution/Physiology.cs:197`.
- **PR wins** `property:BloodPressureAverage`; PR `public float BloodPressureAverage` at `source-reference/Original/CharacterDependencies/Physiology.cs:211`; develop `public float BloodPressureAverage` at `source-reference/Original/ProjectileInteractions/Physiology.cs:211`, `source-reference/Original/CharacterExecution/Physiology.cs:199`.
- **PR wins** `property:BloodSquirtAudioVolume`; PR `public float BloodSquirtAudioVolume` at `source-reference/Original/CharacterDependencies/Physiology.cs:326`; develop `public float BloodSquirtAudioVolume` at `source-reference/Original/ProjectileInteractions/Physiology.cs:326`, `source-reference/Original/CharacterExecution/Physiology.cs:260`.
- **PR wins** `property:BloodVolume`; PR `public float BloodVolume` at `source-reference/Original/CharacterDependencies/Physiology.cs:193`; develop `public float BloodVolume` at `source-reference/Original/ProjectileInteractions/Physiology.cs:193`, `source-reference/Original/CharacterExecution/Physiology.cs:187`.
- **PR wins** `property:BloodVolumeNormalized`; PR `public float BloodVolumeNormalized` at `source-reference/Original/CharacterDependencies/Physiology.cs:197`; develop `public float BloodVolumeNormalized` at `source-reference/Original/ProjectileInteractions/Physiology.cs:197`, `source-reference/Original/CharacterExecution/Physiology.cs:191`.
- **PR wins** `property:BrainTissueNormalized`; PR `public float BrainTissueNormalized` at `source-reference/Original/CharacterDependencies/Physiology.cs:271`; develop `public float BrainTissueNormalized` at `source-reference/Original/ProjectileInteractions/Physiology.cs:271`, `source-reference/Original/CharacterExecution/Physiology.cs:229`.
- **PR wins** `property:BrainTrauma`; PR `public float BrainTrauma` at `source-reference/Original/CharacterDependencies/Physiology.cs:261`; develop `public float BrainTrauma` at `source-reference/Original/ProjectileInteractions/Physiology.cs:261`, `source-reference/Original/CharacterExecution/Physiology.cs:225`.
- **PR wins** `property:BrainTraumaNormalized`; PR `public float BrainTraumaNormalized` at `source-reference/Original/CharacterDependencies/Physiology.cs:263`; develop `public float BrainTraumaNormalized` at `source-reference/Original/ProjectileInteractions/Physiology.cs:263`, `source-reference/Original/CharacterExecution/Physiology.cs:227`.
- **PR wins** `property:CirculationDisabled`; PR `public bool CirculationDisabled` at `source-reference/Original/CharacterDependencies/Physiology.cs:189`; develop `public bool CirculationDisabled` at `source-reference/Original/ProjectileInteractions/Physiology.cs:189`, `source-reference/Original/CharacterExecution/Physiology.cs:183`.
- **PR wins** `property:Concussion`; PR `public float Concussion` at `source-reference/Original/CharacterDependencies/Physiology.cs:253`; develop `public float Concussion` at `source-reference/Original/ProjectileInteractions/Physiology.cs:253`, `source-reference/Original/CharacterExecution/Physiology.cs:223`.
- **PR wins** `property:Conscious`; PR `public bool Conscious` at `source-reference/Original/CharacterDependencies/Physiology.cs:149`; develop `public bool Conscious` at `source-reference/Original/ProjectileInteractions/Physiology.cs:149`, `source-reference/Original/CharacterExecution/Physiology.cs:149`.
- **PR wins** `property:ConsciousAndKinematic`; PR `public bool ConsciousAndKinematic` at `source-reference/Original/CharacterDependencies/Physiology.cs:151`; develop `public bool ConsciousAndKinematic` at `source-reference/Original/ProjectileInteractions/Physiology.cs:151`, `source-reference/Original/CharacterExecution/Physiology.cs:151`.
- **PR wins** `property:CoughedThisRespirationCycle`; PR `public bool CoughedThisRespirationCycle` at `source-reference/Original/CharacterDependencies/Physiology.cs:249`; develop `public bool CoughedThisRespirationCycle` at `source-reference/Original/ProjectileInteractions/Physiology.cs:249`, `source-reference/Original/CharacterExecution/Physiology.cs:219`.
- **PR wins** `property:DiastolicPressure`; PR `public float DiastolicPressure` at `source-reference/Original/CharacterDependencies/Physiology.cs:207`; develop `public float DiastolicPressure` at `source-reference/Original/ProjectileInteractions/Physiology.cs:207`, `source-reference/Original/CharacterExecution/Physiology.cs:195`.
- **PR wins** `property:Exsanguinated`; PR `public bool Exsanguinated` at `source-reference/Original/CharacterDependencies/Physiology.cs:307`; develop `public bool Exsanguinated` at `source-reference/Original/ProjectileInteractions/Physiology.cs:307`, `source-reference/Original/CharacterExecution/Physiology.cs:247`.
- **PR wins** `property:HealthCondition`; PR `public HealthConditionType HealthCondition` at `source-reference/Original/CharacterDependencies/Physiology.cs:161`; develop `public HealthConditionType HealthCondition` at `source-reference/Original/ProjectileInteractions/Physiology.cs:161`, `source-reference/Original/CharacterExecution/Physiology.cs:155`.
- **PR wins** `property:HeartBeatAudio`; PR `public AudioSource HeartBeatAudio` at `source-reference/Original/CharacterDependencies/Physiology.cs:227`; develop `public AudioSource HeartBeatAudio` at `source-reference/Original/ProjectileInteractions/Physiology.cs:227`, `source-reference/Original/CharacterExecution/Physiology.cs:203`.
- **PR wins** `property:HeartBeatCycle`; PR `public float HeartBeatCycle` at `source-reference/Original/CharacterDependencies/Physiology.cs:205`; develop `public float HeartBeatCycle` at `source-reference/Original/ProjectileInteractions/Physiology.cs:205`, `source-reference/Original/CharacterExecution/Physiology.cs:193`.
- **PR wins** `property:HeartBeatPeriod`; PR `public float HeartBeatPeriod` at `source-reference/Original/CharacterDependencies/Physiology.cs:219`; develop `public float HeartBeatPeriod` at `source-reference/Original/ProjectileInteractions/Physiology.cs:219`, `source-reference/Original/CharacterExecution/Physiology.cs:201`.
- **PR wins** `property:HeartCapacity`; PR `private float HeartCapacity` at `source-reference/Original/CharacterDependencies/Physiology.cs:351`; develop `private float HeartCapacity` at `source-reference/Original/ProjectileInteractions/Physiology.cs:351`, `source-reference/Original/CharacterExecution/Physiology.cs:279`.
- **PR wins** `property:HeartEffectiveness`; PR `private float HeartEffectiveness` at `source-reference/Original/CharacterDependencies/Physiology.cs:343`; develop `private float HeartEffectiveness` at `source-reference/Original/ProjectileInteractions/Physiology.cs:343`, `source-reference/Original/CharacterExecution/Physiology.cs:277`.
- **PR wins** `property:Hemorrhaging`; PR `public float Hemorrhaging` at `source-reference/Original/CharacterDependencies/Physiology.cs:303`; develop `public float Hemorrhaging` at `source-reference/Original/ProjectileInteractions/Physiology.cs:303`, `source-reference/Original/CharacterExecution/Physiology.cs:243`.
- **PR wins** `property:LungsCapacity`; PR `public float LungsCapacity` at `source-reference/Original/CharacterDependencies/Physiology.cs:235`; develop `public float LungsCapacity` at `source-reference/Original/ProjectileInteractions/Physiology.cs:235`, `source-reference/Original/CharacterExecution/Physiology.cs:211`.
- **PR wins** `property:LungsEffectiveness`; PR `public float LungsEffectiveness` at `source-reference/Original/CharacterDependencies/Physiology.cs:237`; develop `public float LungsEffectiveness` at `source-reference/Original/ProjectileInteractions/Physiology.cs:237`, `source-reference/Original/CharacterExecution/Physiology.cs:213`.
- **PR wins** `property:LungsOxygen`; PR `public float LungsOxygen` at `source-reference/Original/CharacterDependencies/Physiology.cs:233`; develop `public float LungsOxygen` at `source-reference/Original/ProjectileInteractions/Physiology.cs:233`, `source-reference/Original/CharacterExecution/Physiology.cs:209`.
- **PR wins** `property:LungsOxygenNormalized`; PR `public float LungsOxygenNormalized` at `source-reference/Original/CharacterDependencies/Physiology.cs:239`; develop `public float LungsOxygenNormalized` at `source-reference/Original/ProjectileInteractions/Physiology.cs:239`, `source-reference/Original/CharacterExecution/Physiology.cs:215`.
- **PR wins** `property:MotorCortexDisabled`; PR `public bool MotorCortexDisabled` at `source-reference/Original/CharacterDependencies/Physiology.cs:191`; develop `public bool MotorCortexDisabled` at `source-reference/Original/ProjectileInteractions/Physiology.cs:191`, `source-reference/Original/CharacterExecution/Physiology.cs:185`.
- **PR wins** `property:Paralyzed`; PR `public bool Paralyzed` at `source-reference/Original/CharacterDependencies/Physiology.cs:159`; develop `public bool Paralyzed` at `source-reference/Original/ProjectileInteractions/Physiology.cs:159`, `source-reference/Original/CharacterExecution/Physiology.cs:153`.
- **PR wins** `property:Respiration`; PR `public float Respiration` at `source-reference/Original/CharacterDependencies/Physiology.cs:229`; develop `public float Respiration` at `source-reference/Original/ProjectileInteractions/Physiology.cs:229`, `source-reference/Original/CharacterExecution/Physiology.cs:205`.
- **PR wins** `property:RespirationBlocked`; PR `public bool RespirationBlocked` at `source-reference/Original/CharacterDependencies/Physiology.cs:247`; develop `public bool RespirationBlocked` at `source-reference/Original/ProjectileInteractions/Physiology.cs:247`, `source-reference/Original/CharacterExecution/Physiology.cs:217`.
- **PR wins** `property:RespirationDisabled`; PR `public bool RespirationDisabled` at `source-reference/Original/CharacterDependencies/Physiology.cs:187`; develop `public bool RespirationDisabled` at `source-reference/Original/ProjectileInteractions/Physiology.cs:187`, `source-reference/Original/CharacterExecution/Physiology.cs:181`.
- **PR wins** `property:RespirationPeriod`; PR `private float RespirationPeriod` at `source-reference/Original/CharacterDependencies/Physiology.cs:359`; develop `private float RespirationPeriod` at `source-reference/Original/ProjectileInteractions/Physiology.cs:359`, `source-reference/Original/CharacterExecution/Physiology.cs:281`.
- **PR wins** `property:RespirationRateFactor`; PR `public float RespirationRateFactor` at `source-reference/Original/CharacterDependencies/Physiology.cs:281`; develop `public float RespirationRateFactor` at `source-reference/Original/ProjectileInteractions/Physiology.cs:281`, `source-reference/Original/CharacterExecution/Physiology.cs:233`.
- **PR wins** `property:RespirationTimestamp`; PR `public float RespirationTimestamp` at `source-reference/Original/CharacterDependencies/Physiology.cs:279`; develop `public float RespirationTimestamp` at `source-reference/Original/ProjectileInteractions/Physiology.cs:279`, `source-reference/Original/CharacterExecution/Physiology.cs:231`.
- **PR wins** `property:Shock`; PR `public float Shock` at `source-reference/Original/CharacterDependencies/Physiology.cs:289`; develop `public float Shock` at `source-reference/Original/ProjectileInteractions/Physiology.cs:289`, `source-reference/Original/CharacterExecution/Physiology.cs:235`.
- **PR wins** `property:ShockNormalized`; PR `public float ShockNormalized` at `source-reference/Original/CharacterDependencies/Physiology.cs:291`; develop `public float ShockNormalized` at `source-reference/Original/ProjectileInteractions/Physiology.cs:291`, `source-reference/Original/CharacterExecution/Physiology.cs:237`.
- **PR wins** `property:ShockRecovery`; PR `public float ShockRecovery` at `source-reference/Original/CharacterDependencies/Physiology.cs:301`; develop `public float ShockRecovery` at `source-reference/Original/ProjectileInteractions/Physiology.cs:301`, `source-reference/Original/CharacterExecution/Physiology.cs:241`.
- **PR wins** `property:ShockResistance`; PR `public float ShockResistance` at `source-reference/Original/CharacterDependencies/Physiology.cs:299`; develop `public float ShockResistance` at `source-reference/Original/ProjectileInteractions/Physiology.cs:299`, `source-reference/Original/CharacterExecution/Physiology.cs:239`.
- **PR wins** `property:StartingBloodVolume`; PR `public float StartingBloodVolume` at `source-reference/Original/CharacterDependencies/Physiology.cs:195`; develop `public float StartingBloodVolume` at `source-reference/Original/ProjectileInteractions/Physiology.cs:195`, `source-reference/Original/CharacterExecution/Physiology.cs:189`.
- **PR wins** `property:UnconsciousTimestamp`; PR `public float UnconsciousTimestamp` at `source-reference/Original/CharacterDependencies/Physiology.cs:341`; develop `public float UnconsciousTimestamp` at `source-reference/Original/ProjectileInteractions/Physiology.cs:341`, `source-reference/Original/CharacterExecution/Physiology.cs:275`.
- **PR wins** `property:WorkIntensity`; PR `public float WorkIntensity` at `source-reference/Original/CharacterDependencies/Physiology.cs:251`; develop `public float WorkIntensity` at `source-reference/Original/ProjectileInteractions/Physiology.cs:251`, `source-reference/Original/CharacterExecution/Physiology.cs:221`.
- **PR wins** `property:WoundVolume`; PR `public float WoundVolume` at `source-reference/Original/CharacterDependencies/Physiology.cs:305`; develop `public float WoundVolume` at `source-reference/Original/ProjectileInteractions/Physiology.cs:305`, `source-reference/Original/CharacterExecution/Physiology.cs:245`.
- **PR wins** `property:WoundsCondition`; PR `public WoundsConditionType WoundsCondition` at `source-reference/Original/CharacterDependencies/Physiology.cs:185`; develop `public WoundsConditionType WoundsCondition` at `source-reference/Original/ProjectileInteractions/Physiology.cs:185`, `source-reference/Original/CharacterExecution/Physiology.cs:179`.


### 124. `Physiology+HealthConditionType` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `HealthConditionType`; partial `false`; `source-reference/Original/CharacterDependencies/Physiology.cs:7`

**develop declarations**:
- enum; namespace `(global)`; `HealthConditionType`; partial `false`; `source-reference/Original/CharacterExecution/Physiology.cs:7`
- enum; namespace `(global)`; `HealthConditionType`; partial `false`; `source-reference/Original/ProjectileInteractions/Physiology.cs:7`

Common member signatures: **0**.


### 125. `Physiology+WoundsConditionType` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `WoundsConditionType`; partial `false`; `source-reference/Original/CharacterDependencies/Physiology.cs:16`

**develop declarations**:
- enum; namespace `(global)`; `WoundsConditionType`; partial `false`; `source-reference/Original/CharacterExecution/Physiology.cs:16`
- enum; namespace `(global)`; `WoundsConditionType`; partial `false`; `source-reference/Original/ProjectileInteractions/Physiology.cs:16`

Common member signatures: **0**.


### 126. `PresentationContracts` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `PresentationContracts`; partial `false`; `runtime/Checks/PresentationContracts.cs:5`

**develop declarations**:
- class; namespace `(global)`; `PresentationContracts`; partial `false`; `runtime/Checks/PresentationContracts.cs:5`

Common member signatures: **1**.
- **PR wins** ``method:Run`0(string)``; PR `public static string Run(string)` at `runtime/Checks/PresentationContracts.cs:6`; develop `public static string Run(string)` at `runtime/Checks/PresentationContracts.cs:6`.


### 127. `Projectile` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `Projectile`; partial `true`; `runtime/Engine/ProjectileFlightHost.cs:6`
- class; namespace `(global)`; `Projectile`; partial `true`; `runtime/Engine/ProjectileMaterialHost.cs:4`
- class; namespace `(global)`; `Projectile`; partial `true`; `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:7`
- class; namespace `(global)`; `Projectile`; partial `true`; `runtime/Original/Projectile.cs:2`
- class; namespace `(global)`; `Projectile`; partial `true`; `runtime/Original/Weapons/ProjectileFlight.Methods.cs:5`
- class; namespace `(global)`; `Projectile`; partial `false`; `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:5`

**develop declarations**:
- class; namespace `(global)`; `Projectile`; partial `true`; `runtime/Engine/ProjectileFlightHost.cs:6`
- class; namespace `(global)`; `Projectile`; partial `true`; `runtime/Engine/ProjectileMaterialHost.cs:4`
- class; namespace `(global)`; `Projectile`; partial `true`; `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:7`
- class; namespace `(global)`; `Projectile`; partial `true`; `runtime/Original/Projectile.cs:2`
- class; namespace `(global)`; `Projectile`; partial `true`; `runtime/Original/Weapons/ProjectileFlight.Methods.cs:5`
- class; namespace `(global)`; `Projectile`; partial `false`; `source-reference/Original/ProjectileInteractions/Projectile.cs:5`

Common member signatures: **122**.
- **PR wins** `field:ForceScale`; PR `public const field float ForceScale` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:13`; develop `public const field float ForceScale` at `source-reference/Original/ProjectileInteractions/Projectile.cs:13`.
- **PR wins** `field:FragmentOffset`; PR `public const field float FragmentOffset` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:9`; develop `public const field float FragmentOffset` at `source-reference/Original/ProjectileInteractions/Projectile.cs:9`.
- **PR wins** `field:LeastBounceEnergy`; PR `public const field float LeastBounceEnergy` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:11`; develop `public const field float LeastBounceEnergy` at `source-reference/Original/ProjectileInteractions/Projectile.cs:11`.
- **PR wins** `field:MaxIterations`; PR `public const field int MaxIterations` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:7`; develop `public const field int MaxIterations` at `source-reference/Original/ProjectileInteractions/Projectile.cs:7`.
- **PR wins** `field:MaxPositionX`; PR `private const field float MaxPositionX` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:34`; develop `private const field float MaxPositionX` at `source-reference/Original/ProjectileInteractions/Projectile.cs:34`.
- **PR wins** `field:MaxPositionZ`; PR `private const field float MaxPositionZ` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:36`; develop `private const field float MaxPositionZ` at `source-reference/Original/ProjectileInteractions/Projectile.cs:36`.
- **PR wins** `field:ballisticMask`; PR `private field int ballisticMask` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:44`; develop `private field int ballisticMask` at `source-reference/Original/ProjectileInteractions/Projectile.cs:44`.
- **PR wins** `field:diameter`; PR `private field float diameter` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:50`, `runtime/Original/Projectile.cs:4`; develop `private field float diameter` at `source-reference/Original/ProjectileInteractions/Projectile.cs:50`, `runtime/Original/Projectile.cs:4`.
- **PR wins** `field:fragmentMaterial`; PR `public field Material fragmentMaterial` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:21`; develop `public field Material fragmentMaterial` at `source-reference/Original/ProjectileInteractions/Projectile.cs:21`.
- **PR wins** `field:fragmentMeshes`; PR `public field List<MeshFilter> fragmentMeshes` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:18`; develop `public field List<MeshFilter> fragmentMeshes` at `source-reference/Original/ProjectileInteractions/Projectile.cs:18`.
- **PR wins** `field:freeFallTimestamp`; PR `private field float freeFallTimestamp` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:52`, `runtime/Original/Projectile.cs:5`; develop `private field float freeFallTimestamp` at `source-reference/Original/ProjectileInteractions/Projectile.cs:52`, `runtime/Original/Projectile.cs:5`.
- **PR wins** `field:limbMask`; PR `private field int limbMask` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:46`; develop `private field int limbMask` at `source-reference/Original/ProjectileInteractions/Projectile.cs:46`.
- **PR wins** `field:limbRaycastResult`; PR `private field RaycastHit limbRaycastResult` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:38`, `runtime/Engine/ProjectileFlightHost.cs:11`; develop `private field RaycastHit limbRaycastResult` at `source-reference/Original/ProjectileInteractions/Projectile.cs:38`, `runtime/Engine/ProjectileFlightHost.cs:11`.
- **PR wins** `field:meshTransform`; PR `public field Transform meshTransform` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:15`, `runtime/Original/Projectile.cs:3`; develop `public field Transform meshTransform` at `source-reference/Original/ProjectileInteractions/Projectile.cs:15`, `runtime/Original/Projectile.cs:3`.
- **PR wins** `field:nonDiscriminatingResult`; PR `private field RaycastHit nonDiscriminatingResult` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:40`, `runtime/Engine/ProjectileFlightHost.cs:11`; develop `private field RaycastHit nonDiscriminatingResult` at `source-reference/Original/ProjectileInteractions/Projectile.cs:40`, `runtime/Engine/ProjectileFlightHost.cs:11`.
- **PR wins** `field:obstacleMask`; PR `private field int obstacleMask` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:48`; develop `private field int obstacleMask` at `source-reference/Original/ProjectileInteractions/Projectile.cs:48`.
- **PR wins** `field:onInteraction`; PR `public field ProjectileEvent onInteraction` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:30`, `runtime/Engine/ProjectileMaterialHost.cs:9`; develop `public field ProjectileEvent onInteraction` at `source-reference/Original/ProjectileInteractions/Projectile.cs:30`, `runtime/Engine/ProjectileMaterialHost.cs:8`.
- **PR wins** `field:onLimbInteraction`; PR `public field ProjectileLimbEvent onLimbInteraction` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:32`; develop `public field ProjectileLimbEvent onLimbInteraction` at `source-reference/Original/ProjectileInteractions/Projectile.cs:32`.
- **PR wins** `field:particleCountScaler`; PR `public field float particleCountScaler` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:26`; develop `public field float particleCountScaler` at `source-reference/Original/ProjectileInteractions/Projectile.cs:26`.
- **PR wins** `field:raycastRay`; PR `private field Ray raycastRay` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:42`, `runtime/Engine/ProjectileFlightHost.cs:11`; develop `private field Ray raycastRay` at `source-reference/Original/ProjectileInteractions/Projectile.cs:42`, `runtime/Engine/ProjectileFlightHost.cs:11`.
- **PR wins** `field:segmentBeginMagnitude`; PR `private field float segmentBeginMagnitude` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:54`, `runtime/Engine/ProjectileFlightHost.cs:11`; develop `private field float segmentBeginMagnitude` at `source-reference/Original/ProjectileInteractions/Projectile.cs:54`, `runtime/Engine/ProjectileFlightHost.cs:11`.
- **PR wins** `field:segmentEndMagnitude`; PR `private field float segmentEndMagnitude` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:56`, `runtime/Engine/ProjectileFlightHost.cs:11`; develop `private field float segmentEndMagnitude` at `source-reference/Original/ProjectileInteractions/Projectile.cs:56`, `runtime/Engine/ProjectileFlightHost.cs:11`.
- **PR wins** `field:segmentLength`; PR `private field float segmentLength` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:58`, `runtime/Engine/ProjectileMaterialHost.cs:10`; develop `private field float segmentLength` at `source-reference/Original/ProjectileInteractions/Projectile.cs:58`, `runtime/Engine/ProjectileMaterialHost.cs:9`.
- **PR wins** `field:tracePrefab`; PR `public field ProjectileTrace tracePrefab` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:24`, `runtime/Engine/ProjectileMaterialHost.cs:9`; develop `public field ProjectileTrace tracePrefab` at `source-reference/Original/ProjectileInteractions/Projectile.cs:24`, `runtime/Engine/ProjectileMaterialHost.cs:8`.
- **PR wins** ``method:AssignNewSpinAndTumble`0()``; PR `public void AssignNewSpinAndTumble()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:793`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:70`; develop `public void AssignNewSpinAndTumble()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:793`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:70`.
- **PR wins** ``method:AssignNewSpin`0()``; PR `public void AssignNewSpin()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:778`, `runtime/Original/Weapons/ProjectileFlight.Methods.cs:81`; develop `public void AssignNewSpin()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:778`, `runtime/Original/Weapons/ProjectileFlight.Methods.cs:81`.
- **PR wins** ``method:BeginNewInteraction`0()``; PR `public void BeginNewInteraction()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:448`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:38`; develop `public void BeginNewInteraction()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:448`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:38`.
- **PR wins** ``method:ChangeToFragmentedMesh`0(bool)``; PR `public void ChangeToFragmentedMesh(bool)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:427`, `runtime/Engine/ProjectileMaterialHost.cs:17`; develop `public void ChangeToFragmentedMesh(bool)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:427`, `runtime/Engine/ProjectileMaterialHost.cs:16`.
- **PR wins** ``method:CommonInitialize`0()``; PR `private void CommonInitialize()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:302`, `runtime/Engine/ProjectileMaterialHost.cs:11`; develop `private void CommonInitialize()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:302`, `runtime/Engine/ProjectileMaterialHost.cs:10`.
- **PR wins** ``method:ConvertEnergyToFriction`0(float)``; PR `public float ConvertEnergyToFriction(float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:623`, `runtime/Original/Projectile.cs:157`; develop `public float ConvertEnergyToFriction(float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:623`, `runtime/Original/Projectile.cs:157`.
- **PR wins** ``method:ConvertRemainingEnergyToFriction`0()``; PR `public void ConvertRemainingEnergyToFriction()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:632`, `runtime/Original/Projectile.cs:165`; develop `public void ConvertRemainingEnergyToFriction()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:632`, `runtime/Original/Projectile.cs:165`.
- **PR wins** ``method:Deflect`0(float)``; PR `public void Deflect(float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:783`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:61`; develop `public void Deflect(float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:783`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:61`.
- **PR wins** ``method:DiameterAtFrictionEnergy`0(float, bool)``; PR `public float DiameterAtFrictionEnergy(float, bool)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:291`, `runtime/Original/Projectile.cs:139`; develop `public float DiameterAtFrictionEnergy(float, bool)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:291`, `runtime/Original/Projectile.cs:139`.
- **PR wins** ``method:DistanceAtPosition`0(Vector3)``; PR `public float DistanceAtPosition(Vector3)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:271`, `runtime/Original/Projectile.cs:123`; develop `public float DistanceAtPosition(Vector3)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:271`, `runtime/Original/Projectile.cs:123`.
- **PR wins** ``method:DragVelocityAtPosition`0(Vector3)``; PR `public float DragVelocityAtPosition(Vector3)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:276`, `runtime/Original/Projectile.cs:127`; develop `public float DragVelocityAtPosition(Vector3)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:276`, `runtime/Original/Projectile.cs:127`.
- **PR wins** ``method:EnergyToPenetrate`0(float, float)``; PR `public float EnergyToPenetrate(float, float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:655`, `runtime/Original/Projectile.cs:180`; develop `public float EnergyToPenetrate(float, float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:655`, `runtime/Original/Projectile.cs:180`.
- **PR wins** ``method:EnergyToPenetrate`0(float, float, float, float)``; PR `public float EnergyToPenetrate(float, float, float, float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:660`, `runtime/Original/Projectile.cs:184`; develop `public float EnergyToPenetrate(float, float, float, float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:660`, `runtime/Original/Projectile.cs:184`.
- **PR wins** ``method:Explode`0(Vector3)``; PR `public void Explode(Vector3)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:713`, `runtime/Engine/ProjectileMaterialHost.cs:19`; develop `public void Explode(Vector3)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:713`, `runtime/Engine/ProjectileMaterialHost.cs:18`.
- **PR wins** ``method:GravityVelocityAtTime`0(float)``; PR `public Vector3 GravityVelocityAtTime(float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:286`, `runtime/Original/Projectile.cs:135`; develop `public Vector3 GravityVelocityAtTime(float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:286`, `runtime/Original/Projectile.cs:135`.
- **PR wins** ``method:HandleGenericHit`0(RaycastHit)``; PR `private BallisticResult HandleGenericHit(RaycastHit)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:545`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:8`; develop `private BallisticResult HandleGenericHit(RaycastHit)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:545`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:8`.
- **PR wins** ``method:HandleLimbHit`0(RaycastHit)``; PR `private BallisticResult HandleLimbHit(RaycastHit)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:576`, `runtime/Engine/ProjectileFlightHost.cs:24`; develop `private BallisticResult HandleLimbHit(RaycastHit)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:576`, `runtime/Engine/ProjectileFlightHost.cs:20`.
- **PR wins** ``method:InitializeBenchmark`0(AmmunitionBase)``; PR `public void InitializeBenchmark(AmmunitionBase)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:339`; develop `public void InitializeBenchmark(AmmunitionBase)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:339`.
- **PR wins** ``method:InitializeExplosionFragment`0(AmmunitionBase, int, Transform, Vector3, float, float, float, float, float)``; PR `public void InitializeExplosionFragment(AmmunitionBase, int, Transform, Vector3, float, float, float, float, float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:396`; develop `public void InitializeExplosionFragment(AmmunitionBase, int, Transform, Vector3, float, float, float, float, float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:396`.
- **PR wins** ``method:InitializeMarksman`0(AmmunitionBase, float, AudioInstigator)``; PR `public void InitializeMarksman(AmmunitionBase, float, AudioInstigator)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:353`; develop `public void InitializeMarksman(AmmunitionBase, float, AudioInstigator)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:353`.
- **PR wins** ``method:InitializeProjectileFragment`0(Projectile, int, Vector3, float, float, float, float, float, float)``; PR `public void InitializeProjectileFragment(Projectile, int, Vector3, float, float, float, float, float, float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:365`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:83`; develop `public void InitializeProjectileFragment(Projectile, int, Vector3, float, float, float, float, float, float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:365`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:83`.
- **PR wins** ``method:InitializeVelocity`0(float)``; PR `private void InitializeVelocity(float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:419`, `runtime/Original/Weapons/ProjectileFlight.Methods.cs:70`; develop `private void InitializeVelocity(float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:419`, `runtime/Original/Weapons/ProjectileFlight.Methods.cs:70`.
- **PR wins** ``method:InitializeXY`0(WeaponBase, bool, float, float, float)``; PR `public void InitializeXY(WeaponBase, bool, float, float, float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:318`, `runtime/Engine/ProjectileFlightHost.cs:13`; develop `public void InitializeXY(WeaponBase, bool, float, float, float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:318`, `runtime/Engine/ProjectileFlightHost.cs:13`.
- **PR wins** ``method:KineticEnergy`0(float, float)``; PR `public static float KineticEnergy(float, float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:252`, `runtime/Original/Projectile.cs:115`; develop `public static float KineticEnergy(float, float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:252`, `runtime/Original/Projectile.cs:115`.
- **PR wins** ``method:MaximumRange`0(float, float, float)``; PR `public static float MaximumRange(float, float, float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:674`, `runtime/Original/Projectile.cs:196`; develop `public static float MaximumRange(float, float, float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:674`, `runtime/Original/Projectile.cs:196`.
- **PR wins** ``method:NewTracePosition`0()``; PR `public void NewTracePosition()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:457`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:46`; develop `public void NewTracePosition()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:457`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:46`.
- **PR wins** ``method:OnFragmentation`0()``; PR `public void OnFragmentation()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:679`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:75`; develop `public void OnFragmentation()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:679`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:75`.
- **PR wins** ``method:PotentialExpansion`0(float, float, float, float)``; PR `public float PotentialExpansion(float, float, float, float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:640`; develop `public float PotentialExpansion(float, float, float, float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:640`.
- **PR wins** ``method:PotentialPenetrationDistance`0(float)``; PR `public float PotentialPenetrationDistance(float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:650`, `runtime/Original/Projectile.cs:176`; develop `public float PotentialPenetrationDistance(float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:650`, `runtime/Original/Projectile.cs:176`.
- **PR wins** ``method:PotentialPenetrationDistance`0(float, float, float, float)``; PR `public float PotentialPenetrationDistance(float, float, float, float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:645`, `runtime/Original/Projectile.cs:172`; develop `public float PotentialPenetrationDistance(float, float, float, float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:645`, `runtime/Original/Projectile.cs:172`.
- **PR wins** ``method:RaycastLimbs`0(Ray, float)``; PR `public Limb RaycastLimbs(Ray, float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:257`; develop `public Limb RaycastLimbs(Ray, float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:257`.
- **PR wins** ``method:RecalculateVelocity`0()``; PR `public void RecalculateVelocity()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:266`, `runtime/Original/Projectile.cs:119`; develop `public void RecalculateVelocity()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:266`, `runtime/Original/Projectile.cs:119`.
- **PR wins** ``method:RecordAttackInfo`0(CharacterBody)``; PR `private void RecordAttackInfo(CharacterBody)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:595`; develop `private void RecordAttackInfo(CharacterBody)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:595`.
- **PR wins** ``method:RemoveEnergy`0(float)``; PR `public void RemoveEnergy(float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:618`, `runtime/Original/Projectile.cs:153`; develop `public void RemoveEnergy(float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:618`, `runtime/Original/Projectile.cs:153`.
- **PR wins** ``method:RestartAtPosition`0(Vector3)``; PR `public void RestartAtPosition(Vector3)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:439`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:53`; develop `public void RestartAtPosition(Vector3)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:439`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:53`.
- **PR wins** ``method:SetActiveSegment`0(float, float)``; PR `public void SetActiveSegment(float, float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:465`, `runtime/Engine/ProjectileMaterialHost.cs:18`; develop `public void SetActiveSegment(float, float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:465`, `runtime/Engine/ProjectileMaterialHost.cs:17`.
- **PR wins** ``method:SpawnRandomFragments`0(Vector3, UnityAction<Projectile, bool>)``; PR `public void SpawnRandomFragments(Vector3, UnityAction<Projectile, bool>)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:755`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:129`; develop `public void SpawnRandomFragments(Vector3, UnityAction<Projectile, bool>)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:755`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:129`.
- **PR wins** ``method:SpawnSpecificFragments`0(Vector3, UnityAction<Projectile, bool>)``; PR `public void SpawnSpecificFragments(Vector3, UnityAction<Projectile, bool>)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:738`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:113`; develop `public void SpawnSpecificFragments(Vector3, UnityAction<Projectile, bool>)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:738`, `runtime/Original/Ballistics/ProjectileMaterial.Methods.cs:113`.
- **PR wins** ``method:StepFlight`0()``; PR `public void StepFlight()` at `runtime/Original/Weapons/ProjectileFlight.Methods.cs:6`; develop `public void StepFlight()` at `runtime/Original/Weapons/ProjectileFlight.Methods.cs:6`.
- **PR wins** ``method:TestExplosion`0(float)``; PR `private void TestExplosion(float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:688`; develop `private void TestExplosion(float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:688`.
- **PR wins** ``method:TestFragmentation`0()``; PR `public bool TestFragmentation()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:665`, `runtime/Original/Projectile.cs:188`; develop `public bool TestFragmentation()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:665`, `runtime/Original/Projectile.cs:188`.
- **PR wins** ``method:TestTermination`0()``; PR `public bool TestTermination()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:540`, `runtime/Original/Weapons/ProjectileFlight.Methods.cs:77`; develop `public bool TestTermination()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:540`, `runtime/Original/Weapons/ProjectileFlight.Methods.cs:77`.
- **PR wins** ``method:UpdateRotation`0()``; PR `public void UpdateRotation()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:799`, `runtime/Original/Weapons/ProjectileFlight.Methods.cs:85`; develop `public void UpdateRotation()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:799`, `runtime/Original/Weapons/ProjectileFlight.Methods.cs:85`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:475`; develop `private void Update()` at `source-reference/Original/ProjectileInteractions/Projectile.cs:475`.
- **PR wins** ``method:VelocityAtPosition`0(Vector3)``; PR `public float VelocityAtPosition(Vector3)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:281`, `runtime/Original/Projectile.cs:131`; develop `public float VelocityAtPosition(Vector3)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:281`, `runtime/Original/Projectile.cs:131`.
- **PR wins** ``method:VelocityToEnergy`0(float)``; PR `public float VelocityToEnergy(float)` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:613`, `runtime/Original/Projectile.cs:149`; develop `public float VelocityToEnergy(float)` at `source-reference/Original/ProjectileInteractions/Projectile.cs:613`, `runtime/Original/Projectile.cs:149`.
- **PR wins** `property:Ammunition`; PR `public AmmunitionBase Ammunition` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:82`, `runtime/Original/Projectile.cs:7`; develop `public AmmunitionBase Ammunition` at `source-reference/Original/ProjectileInteractions/Projectile.cs:82`, `runtime/Original/Projectile.cs:7`.
- **PR wins** `property:AudioInstigator`; PR `public AudioInstigator AudioInstigator` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:80`, `runtime/Engine/ProjectileMaterialHost.cs:8`; develop `public AudioInstigator AudioInstigator` at `source-reference/Original/ProjectileInteractions/Projectile.cs:80`, `runtime/Engine/ProjectileMaterialHost.cs:7`.
- **PR wins** `property:BallisticCoefficient`; PR `public float BallisticCoefficient` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:172`, `runtime/Original/Projectile.cs:47`; develop `public float BallisticCoefficient` at `source-reference/Original/ProjectileInteractions/Projectile.cs:172`, `runtime/Original/Projectile.cs:47`.
- **PR wins** `property:BaseVelocity`; PR `public float BaseVelocity` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:136`, `runtime/Original/Projectile.cs:17`; develop `public float BaseVelocity` at `source-reference/Original/ProjectileInteractions/Projectile.cs:136`, `runtime/Original/Projectile.cs:17`.
- **PR wins** `property:DeformationRatio`; PR `public float DeformationRatio` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:164`, `runtime/Original/Projectile.cs:40`; develop `public float DeformationRatio` at `source-reference/Original/ProjectileInteractions/Projectile.cs:164`, `runtime/Original/Projectile.cs:40`.
- **PR wins** `property:Diameter`; PR `public float Diameter` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:140`, `runtime/Original/Projectile.cs:19`; develop `public float Diameter` at `source-reference/Original/ProjectileInteractions/Projectile.cs:140`, `runtime/Original/Projectile.cs:19`.
- **PR wins** `property:DisplacedMaterialVolume`; PR `public float DisplacedMaterialVolume` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:206`, `runtime/Original/Projectile.cs:76`; develop `public float DisplacedMaterialVolume` at `source-reference/Original/ProjectileInteractions/Projectile.cs:206`, `runtime/Original/Projectile.cs:76`.
- **PR wins** `property:Distance`; PR `public float Distance` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:210`, `runtime/Original/Projectile.cs:78`; develop `public float Distance` at `source-reference/Original/ProjectileInteractions/Projectile.cs:210`, `runtime/Original/Projectile.cs:78`.
- **PR wins** `property:Drag`; PR `public float Drag` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:220`, `runtime/Original/Projectile.cs:86`; develop `public float Drag` at `source-reference/Original/ProjectileInteractions/Projectile.cs:220`, `runtime/Original/Projectile.cs:86`.
- **PR wins** `property:Energy`; PR `public float Energy` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:236`, `runtime/Original/Projectile.cs:100`; develop `public float Energy` at `source-reference/Original/ProjectileInteractions/Projectile.cs:236`, `runtime/Original/Projectile.cs:100`.
- **PR wins** `property:EnergyNormalized`; PR `public float EnergyNormalized` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:244`, `runtime/Original/Projectile.cs:107`; develop `public float EnergyNormalized` at `source-reference/Original/ProjectileInteractions/Projectile.cs:244`, `runtime/Original/Projectile.cs:107`.
- **PR wins** `property:Finished`; PR `public bool Finished` at `runtime/Engine/ProjectileFlightHost.cs:9`; develop `public bool Finished` at `runtime/Engine/ProjectileFlightHost.cs:9`.
- **PR wins** `property:FiredByMarksman`; PR `public bool FiredByMarksman` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:102`, `runtime/Engine/ProjectileMaterialHost.cs:6`; develop `public bool FiredByMarksman` at `source-reference/Original/ProjectileInteractions/Projectile.cs:102`, `runtime/Engine/ProjectileMaterialHost.cs:6`.
- **PR wins** `property:FiredWithPrecision`; PR `public bool FiredWithPrecision` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:90`, `runtime/Engine/ProjectileFlightHost.cs:9`; develop `public bool FiredWithPrecision` at `source-reference/Original/ProjectileInteractions/Projectile.cs:90`, `runtime/Engine/ProjectileFlightHost.cs:9`.
- **PR wins** `property:FirstHitCharacter`; PR `public CharacterBody FirstHitCharacter` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:84`; develop `public CharacterBody FirstHitCharacter` at `source-reference/Original/ProjectileInteractions/Projectile.cs:84`.
- **PR wins** `property:FlightMask`; PR `public int FlightMask` at `runtime/Engine/ProjectileFlightHost.cs:7`; develop `public int FlightMask` at `runtime/Engine/ProjectileFlightHost.cs:7`.
- **PR wins** `property:ForceMultiplier`; PR `public float ForceMultiplier` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:112`, `runtime/Engine/ProjectileFlightHost.cs:8`; develop `public float ForceMultiplier` at `source-reference/Original/ProjectileInteractions/Projectile.cs:112`, `runtime/Engine/ProjectileFlightHost.cs:8`.
- **PR wins** `property:FragmentationTime`; PR `public float FragmentationTime` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:66`, `runtime/Engine/ProjectileMaterialHost.cs:5`; develop `public float FragmentationTime` at `source-reference/Original/ProjectileInteractions/Projectile.cs:66`, `runtime/Engine/ProjectileMaterialHost.cs:5`.
- **PR wins** `property:FrictionEnergy`; PR `public float FrictionEnergy` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:68`, `runtime/Original/Projectile.cs:6`; develop `public float FrictionEnergy` at `source-reference/Original/ProjectileInteractions/Projectile.cs:68`, `runtime/Original/Projectile.cs:6`.
- **PR wins** `property:FrictionEnergyNormalized`; PR `public float FrictionEnergyNormalized` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:156`, `runtime/Original/Projectile.cs:33`; develop `public float FrictionEnergyNormalized` at `source-reference/Original/ProjectileInteractions/Projectile.cs:156`, `runtime/Original/Projectile.cs:33`.
- **PR wins** `property:HasExploded`; PR `public bool HasExploded` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:100`, `runtime/Engine/ProjectileMaterialHost.cs:5`; develop `public bool HasExploded` at `source-reference/Original/ProjectileInteractions/Projectile.cs:100`, `runtime/Engine/ProjectileMaterialHost.cs:5`.
- **PR wins** `property:HasFragmented`; PR `public bool HasFragmented` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:98`, `runtime/Original/Projectile.cs:114`; develop `public bool HasFragmented` at `source-reference/Original/ProjectileInteractions/Projectile.cs:98`, `runtime/Original/Projectile.cs:114`.
- **PR wins** `property:Id`; PR `public int Id` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:60`, `runtime/Engine/ProjectileFlightHost.cs:7`; develop `public int Id` at `source-reference/Original/ProjectileInteractions/Projectile.cs:60`, `runtime/Engine/ProjectileFlightHost.cs:7`.
- **PR wins** `property:InteractionEnergy`; PR `public float InteractionEnergy` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:208`, `runtime/Original/Projectile.cs:77`; develop `public float InteractionEnergy` at `source-reference/Original/ProjectileInteractions/Projectile.cs:208`, `runtime/Original/Projectile.cs:77`.
- **PR wins** `property:InteractionPosition`; PR `public Vector3 InteractionPosition` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:88`, `runtime/Engine/ProjectileFlightHost.cs:10`; develop `public Vector3 InteractionPosition` at `source-reference/Original/ProjectileInteractions/Projectile.cs:88`, `runtime/Engine/ProjectileFlightHost.cs:10`.
- **PR wins** `property:IsStray`; PR `public bool IsStray` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:96`, `runtime/Engine/ProjectileFlightHost.cs:9`; develop `public bool IsStray` at `source-reference/Original/ProjectileInteractions/Projectile.cs:96`, `runtime/Engine/ProjectileFlightHost.cs:9`.
- **PR wins** `property:MarksmanOwner`; PR `public Marksman MarksmanOwner` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:78`, `runtime/Engine/ProjectileMaterialHost.cs:6`; develop `public Marksman MarksmanOwner` at `source-reference/Original/ProjectileInteractions/Projectile.cs:78`, `runtime/Engine/ProjectileMaterialHost.cs:6`.
- **PR wins** `property:Mass`; PR `public float Mass` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:126`, `runtime/Original/Projectile.cs:9`; develop `public float Mass` at `source-reference/Original/ProjectileInteractions/Projectile.cs:126`, `runtime/Original/Projectile.cs:9`.
- **PR wins** `property:MetricDiameter`; PR `public float MetricDiameter` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:148`, `runtime/Original/Projectile.cs:26`; develop `public float MetricDiameter` at `source-reference/Original/ProjectileInteractions/Projectile.cs:148`, `runtime/Original/Projectile.cs:26`.
- **PR wins** `property:MetricMass`; PR `public float MetricMass` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:128`, `runtime/Original/Projectile.cs:10`; develop `public float MetricMass` at `source-reference/Original/ProjectileInteractions/Projectile.cs:128`, `runtime/Original/Projectile.cs:10`.
- **PR wins** `property:NextPosition`; PR `public Vector3 NextPosition` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:124`, `runtime/Engine/ProjectileFlightHost.cs:10`; develop `public Vector3 NextPosition` at `source-reference/Original/ProjectileInteractions/Projectile.cs:124`, `runtime/Engine/ProjectileFlightHost.cs:10`.
- **PR wins** `property:Owner`; PR `public CharacterBody Owner` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:76`, `runtime/Engine/ProjectileMaterialHost.cs:7`; develop `public CharacterBody Owner` at `source-reference/Original/ProjectileInteractions/Projectile.cs:76`, `runtime/Engine/ProjectileMaterialHost.cs:7`.
- **PR wins** `property:PenetrationDistance`; PR `public float PenetrationDistance` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:204`, `runtime/Original/Projectile.cs:75`; develop `public float PenetrationDistance` at `source-reference/Original/ProjectileInteractions/Projectile.cs:204`, `runtime/Original/Projectile.cs:75`.
- **PR wins** `property:PermanentCavityFactor`; PR `public float PermanentCavityFactor` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:196`, `runtime/Original/Projectile.cs:68`; develop `public float PermanentCavityFactor` at `source-reference/Original/ProjectileInteractions/Projectile.cs:196`, `runtime/Original/Projectile.cs:68`.
- **PR wins** `property:Position`; PR `public Vector3 Position` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:116`, `runtime/Engine/ProjectileMaterialHost.cs:10`; develop `public Vector3 Position` at `source-reference/Original/ProjectileInteractions/Projectile.cs:116`, `runtime/Engine/ProjectileMaterialHost.cs:9`.
- **PR wins** `property:PotentialFlightRangeLeft`; PR `public float PotentialFlightRangeLeft` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:228`, `runtime/Original/Projectile.cs:93`; develop `public float PotentialFlightRangeLeft` at `source-reference/Original/ProjectileInteractions/Projectile.cs:228`, `runtime/Original/Projectile.cs:93`.
- **PR wins** `property:RiflingTwistRate`; PR `private float RiflingTwistRate` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:72`, `runtime/Engine/ProjectileFlightHost.cs:8`; develop `private float RiflingTwistRate` at `source-reference/Original/ProjectileInteractions/Projectile.cs:72`, `runtime/Engine/ProjectileFlightHost.cs:8`.
- **PR wins** `property:SegmentStart`; PR `public Vector3 SegmentStart` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:92`; develop `public Vector3 SegmentStart` at `source-reference/Original/ProjectileInteractions/Projectile.cs:92`.
- **PR wins** `property:SpawnTime`; PR `public float SpawnTime` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:64`, `runtime/Engine/ProjectileFlightHost.cs:8`; develop `public float SpawnTime` at `source-reference/Original/ProjectileInteractions/Projectile.cs:64`, `runtime/Engine/ProjectileFlightHost.cs:8`.
- **PR wins** `property:Spin`; PR `public float Spin` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:70`, `runtime/Engine/ProjectileFlightHost.cs:8`; develop `public float Spin` at `source-reference/Original/ProjectileInteractions/Projectile.cs:70`, `runtime/Engine/ProjectileFlightHost.cs:8`.
- **PR wins** `property:StartEnergy`; PR `public float StartEnergy` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:138`, `runtime/Original/Projectile.cs:18`; develop `public float StartEnergy` at `source-reference/Original/ProjectileInteractions/Projectile.cs:138`, `runtime/Original/Projectile.cs:18`.
- **PR wins** `property:StartPosition`; PR `public Vector3 StartPosition` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:86`, `runtime/Original/Projectile.cs:8`; develop `public Vector3 StartPosition` at `source-reference/Original/ProjectileInteractions/Projectile.cs:86`, `runtime/Original/Projectile.cs:8`.
- **PR wins** `property:Status`; PR `public BallisticResult Status` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:114`, `runtime/Engine/ProjectileMaterialHost.cs:5`; develop `public BallisticResult Status` at `source-reference/Original/ProjectileInteractions/Projectile.cs:114`, `runtime/Engine/ProjectileMaterialHost.cs:5`.
- **PR wins** `property:SuperSonic`; PR `public bool SuperSonic` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:180`, `runtime/Original/Projectile.cs:54`; develop `public bool SuperSonic` at `source-reference/Original/ProjectileInteractions/Projectile.cs:180`, `runtime/Original/Projectile.cs:54`.
- **PR wins** `property:SwarmId`; PR `public int SwarmId` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:62`, `runtime/Engine/ProjectileFlightHost.cs:7`; develop `public int SwarmId` at `source-reference/Original/ProjectileInteractions/Projectile.cs:62`, `runtime/Engine/ProjectileFlightHost.cs:7`.
- **PR wins** `property:TemporalCavityFactor`; PR `public float TemporalCavityFactor` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:188`, `runtime/Original/Projectile.cs:61`; develop `public float TemporalCavityFactor` at `source-reference/Original/ProjectileInteractions/Projectile.cs:188`, `runtime/Original/Projectile.cs:61`.
- **PR wins** `property:Trace`; PR `public ProjectileTrace Trace` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:110`, `runtime/Engine/ProjectileMaterialHost.cs:9`; develop `public ProjectileTrace Trace` at `source-reference/Original/ProjectileInteractions/Projectile.cs:110`, `runtime/Engine/ProjectileMaterialHost.cs:8`.
- **PR wins** `property:Tumble`; PR `public Vector3 Tumble` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:94`, `runtime/Engine/ProjectileFlightHost.cs:10`; develop `public Vector3 Tumble` at `source-reference/Original/ProjectileInteractions/Projectile.cs:94`, `runtime/Engine/ProjectileFlightHost.cs:10`.
- **PR wins** `property:Velocity`; PR `public float Velocity` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:218`, `runtime/Original/Projectile.cs:85`; develop `public float Velocity` at `source-reference/Original/ProjectileInteractions/Projectile.cs:218`, `runtime/Original/Projectile.cs:85`.
- **PR wins** `property:Weapon`; PR `public WeaponBase Weapon` at `source-reference/Original/Assets/Scripts/Assembly-CSharp/Projectile.cs:74`, `runtime/Engine/ProjectileMaterialHost.cs:7`; develop `public WeaponBase Weapon` at `source-reference/Original/ProjectileInteractions/Projectile.cs:74`, `runtime/Engine/ProjectileMaterialHost.cs:7`.
- **PR wins** `property:ballisticMask`; PR `int ballisticMask` at `runtime/Engine/ProjectileFlightHost.cs:12`; develop `int ballisticMask` at `runtime/Engine/ProjectileFlightHost.cs:12`.
- **PR wins** `property:limbMask`; PR `int limbMask` at `runtime/Engine/ProjectileFlightHost.cs:12`; develop `int limbMask` at `runtime/Engine/ProjectileFlightHost.cs:12`.


### 128. `ProjectileEvent` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `ProjectileEvent`; partial `false`; `runtime/Engine/BallisticEngine.cs:3`

**develop declarations**:
- class; namespace `(global)`; `ProjectileEvent`; partial `false`; `runtime/Engine/BallisticEngine.cs:43`

Common member signatures: **3**.
- **PR wins** `event:listeners`; PR `event Action<Projectile> listeners` at `runtime/Engine/BallisticEngine.cs:3`; develop `event Action<Projectile> listeners` at `runtime/Engine/BallisticEngine.cs:43`.
- **PR wins** ``method:AddListener`0(Action<Projectile>)``; PR `public void AddListener(Action<Projectile>)` at `runtime/Engine/BallisticEngine.cs:3`; develop `public void AddListener(Action<Projectile>)` at `runtime/Engine/BallisticEngine.cs:43`.
- **PR wins** ``method:Invoke`0(Projectile)``; PR `public void Invoke(Projectile)` at `runtime/Engine/BallisticEngine.cs:3`; develop `public void Invoke(Projectile)` at `runtime/Engine/BallisticEngine.cs:43`.


### 129. `ProjectileTrace` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `ProjectileTrace`; partial `false`; `runtime/Engine/BallisticEngine.cs:4`

**develop declarations**:
- class; namespace `(global)`; `ProjectileTrace`; partial `false`; `runtime/Engine/BallisticEngine.cs:44`

Common member signatures: **3**.
- **PR wins** ``method:AddPosition`0(Vector3)``; PR `public void AddPosition(Vector3)` at `runtime/Engine/BallisticEngine.cs:4`; develop `public void AddPosition(Vector3)` at `runtime/Engine/BallisticEngine.cs:44`.
- **PR wins** ``method:Initialize`0(Projectile)``; PR `public void Initialize(Projectile)` at `runtime/Engine/BallisticEngine.cs:4`; develop `public void Initialize(Projectile)` at `runtime/Engine/BallisticEngine.cs:44`.
- **PR wins** ``method:SpawnHitEffect`0(Vector3, Transform, Vector3)``; PR `public void SpawnHitEffect(Vector3, Transform, Vector3)` at `runtime/Engine/BallisticEngine.cs:4`; develop `public void SpawnHitEffect(Vector3, Transform, Vector3)` at `runtime/Engine/BallisticEngine.cs:44`.


### 130. `RuntimeBridge` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `RuntimeBridge`; partial `false`; `runtime/Browser/Program.cs:15`

**develop declarations**:
- class; namespace `(global)`; `RuntimeBridge`; partial `false`; `runtime/Browser/Program.cs:15`

Common member signatures: **21**.
- **PR wins** `field:input`; PR `static field OriginalInputHost input` at `runtime/Browser/Program.cs:18`; develop `static field OriginalInputHost input` at `runtime/Browser/Program.cs:18`.
- **PR wins** `field:presentation`; PR `static field Crisis.Runtime.Presentation.PresentationHost presentation` at `runtime/Browser/Program.cs:25`; develop `static field Crisis.Runtime.Presentation.PresentationHost presentation` at `runtime/Browser/Program.cs:50`.
- **PR wins** `field:scene`; PR `static field SceneLoadResult scene` at `runtime/Browser/Program.cs:24`; develop `static field SceneLoadResult scene` at `runtime/Browser/Program.cs:49`.
- **PR wins** `field:weapons`; PR `static field Crisis.Runtime.Weapons.WeaponHost weapons` at `runtime/Browser/Program.cs:19`; develop `static field Crisis.Runtime.Weapons.WeaponHost weapons` at `runtime/Browser/Program.cs:19`.
- **PR wins** ``method:ComposeOriginalArmSequences`0(string, string)``; PR `public static string ComposeOriginalArmSequences(string, string)` at `runtime/Browser/Program.cs:16`; develop `public static string ComposeOriginalArmSequences(string, string)` at `runtime/Browser/Program.cs:16`.
- **PR wins** ``method:InitializeBallistics`0(string)``; PR `public static string InitializeBallistics(string)` at `runtime/Browser/Program.cs:21`; develop `public static string InitializeBallistics(string)` at `runtime/Browser/Program.cs:46`.
- **PR wins** ``method:InitializePresentation`0(string)``; PR `public static string InitializePresentation(string)` at `runtime/Browser/Program.cs:26`; develop `public static string InitializePresentation(string)` at `runtime/Browser/Program.cs:51`.
- **PR wins** ``method:InitializeWeapons`0(string)``; PR `public static string InitializeWeapons(string)` at `runtime/Browser/Program.cs:20`; develop `public static string InitializeWeapons(string)` at `runtime/Browser/Program.cs:20`.
- **PR wins** ``method:LoadScene`0(string, string)``; PR `public static string LoadScene(string, string)` at `runtime/Browser/Program.cs:37`; develop `public static string LoadScene(string, string)` at `runtime/Browser/Program.cs:84`.
- **PR wins** ``method:OriginalWindowVisuals`0()``; PR `public static string OriginalWindowVisuals()` at `runtime/Browser/Program.cs:23`; develop `public static string OriginalWindowVisuals()` at `runtime/Browser/Program.cs:48`.
- **PR wins** ``method:Raycast`0(float, float, float, float, float, float, float, int)``; PR `public static string Raycast(float, float, float, float, float, float, float, int)` at `runtime/Browser/Program.cs:41`; develop `public static string Raycast(float, float, float, float, float, float, float, int)` at `runtime/Browser/Program.cs:88`.
- **PR wins** ``method:ResetInput`0()``; PR `public static string ResetInput()` at `runtime/Browser/Program.cs:30`; develop `public static string ResetInput()` at `runtime/Browser/Program.cs:77`.
- **PR wins** ``method:SolvePresentationArms`0(string)``; PR `public static string SolvePresentationArms(string)` at `runtime/Browser/Program.cs:27`; develop `public static string SolvePresentationArms(string)` at `runtime/Browser/Program.cs:52`.
- **PR wins** ``method:StepPresentation`0(string)``; PR `public static string StepPresentation(string)` at `runtime/Browser/Program.cs:28`; develop `public static string StepPresentation(string)` at `runtime/Browser/Program.cs:53`.
- **PR wins** ``method:StepWeapons`0(string)``; PR `public static string StepWeapons(string)` at `runtime/Browser/Program.cs:22`; develop `public static string StepWeapons(string)` at `runtime/Browser/Program.cs:47`.
- **PR wins** ``method:Tick`0(float, string)``; PR `public static string Tick(float, string)` at `runtime/Browser/Program.cs:31`; develop `public static string Tick(float, string)` at `runtime/Browser/Program.cs:78`.
- **PR wins** ``method:VerifyAmmunition`0(string, string)``; PR `public static string VerifyAmmunition(string, string)` at `runtime/Browser/Program.cs:38`; develop `public static string VerifyAmmunition(string, string)` at `runtime/Browser/Program.cs:85`.
- **PR wins** ``method:VerifyOriginalHandModes`0()``; PR `public static string VerifyOriginalHandModes()` at `runtime/Browser/Program.cs:17`; develop `public static string VerifyOriginalHandModes()` at `runtime/Browser/Program.cs:17`.
- **PR wins** ``method:VerifyResource`0(string, string)``; PR `public static string VerifyResource(string, string)` at `runtime/Browser/Program.cs:39`; develop `public static string VerifyResource(string, string)` at `runtime/Browser/Program.cs:86`.
- **PR wins** ``method:VerifySceneRay`0()``; PR `public static string VerifySceneRay()` at `runtime/Browser/Program.cs:40`; develop `public static string VerifySceneRay()` at `runtime/Browser/Program.cs:87`.
- **PR wins** ``method:Verify`0()``; PR `public static string Verify()` at `runtime/Browser/Program.cs:29`; develop `public static string Verify()` at `runtime/Browser/Program.cs:76`.

Develop-only member signatures: **3** — **DEVELOP-ONLY / DEFERRED**.
- **DEVELOP-ONLY — DEFERRED** `field:movementSource` — `static readonly field Crisis.Runtime.Weapons.CharacterBody movementSource` at `runtime/Browser/Program.cs:56`.
- **DEVELOP-ONLY — DEFERRED** ``method:StepBodyMovement`0(string)`` — `public static string StepBodyMovement(string)` at `runtime/Browser/Program.cs:57`.
- **DEVELOP-ONLY — DEFERRED** ``method:StepCharacterAnimation`0(string)`` — `public static string StepCharacterAnimation(string)` at `runtime/Browser/Program.cs:31`.


### 131. `StaticUtilities` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `StaticUtilities`; partial `false`; `runtime/Original/Ballistics/StaticUtilities.cs:7`

**develop declarations**:
- class; namespace `(global)`; `StaticUtilities`; partial `false`; `runtime/Original/Ballistics/StaticUtilities.cs:7`

Common member signatures: **1**.
- **PR wins** ``method:RectIntersect`0(Rect, Rect, out Rect)``; PR `public static bool RectIntersect(Rect, Rect, out Rect)` at `runtime/Original/Ballistics/StaticUtilities.cs:9`; develop `public static bool RectIntersect(Rect, Rect, out Rect)` at `runtime/Original/Ballistics/StaticUtilities.cs:9`.


### 132. `TeamOperator` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `TeamOperator`; partial `false`; `source-reference/Original/ArmDependencies/TeamOperator.cs:3`
- class; namespace `(global)`; `TeamOperator`; partial `false`; `source-reference/Original/CharacterDependencies/TeamOperator.cs:3`

**develop declarations**:
- class; namespace `(global)`; `TeamOperator`; partial `false`; `source-reference/Original/ArmDependencies/TeamOperator.cs:3`

Common member signatures: **54**.
- **PR wins** `field:EventOperatorDown`; PR `public field UnityEventVoid EventOperatorDown` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:46`, `source-reference/Original/ArmDependencies/TeamOperator.cs:46`; develop `public field UnityEventVoid EventOperatorDown` at `source-reference/Original/ArmDependencies/TeamOperator.cs:46`.
- **PR wins** `field:EventSuspectDown`; PR `public field UnityEventVoid EventSuspectDown` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:44`, `source-reference/Original/ArmDependencies/TeamOperator.cs:44`; develop `public field UnityEventVoid EventSuspectDown` at `source-reference/Original/ArmDependencies/TeamOperator.cs:44`.
- **PR wins** `field:administerFirstAid`; PR `public field bool administerFirstAid` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:32`, `source-reference/Original/ArmDependencies/TeamOperator.cs:32`; develop `public field bool administerFirstAid` at `source-reference/Original/ArmDependencies/TeamOperator.cs:32`.
- **PR wins** `field:heldGauze`; PR `private field GameObject heldGauze` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:50`, `source-reference/Original/ArmDependencies/TeamOperator.cs:50`; develop `private field GameObject heldGauze` at `source-reference/Original/ArmDependencies/TeamOperator.cs:50`.
- **PR wins** `field:heldPlasticuff`; PR `private field GameObject heldPlasticuff` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:48`, `source-reference/Original/ArmDependencies/TeamOperator.cs:48`; develop `private field GameObject heldPlasticuff` at `source-reference/Original/ArmDependencies/TeamOperator.cs:48`.
- **PR wins** `field:medicPouchTransform`; PR `public field Transform medicPouchTransform` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:41`, `source-reference/Original/ArmDependencies/TeamOperator.cs:41`; develop `public field Transform medicPouchTransform` at `source-reference/Original/ArmDependencies/TeamOperator.cs:41`.
- **PR wins** `field:medicPouchVisual`; PR `public field GameObject medicPouchVisual` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:39`, `source-reference/Original/ArmDependencies/TeamOperator.cs:39`; develop `public field GameObject medicPouchVisual` at `source-reference/Original/ArmDependencies/TeamOperator.cs:39`.
- **PR wins** `field:plasticuffsVisual`; PR `public field GameObject plasticuffsVisual` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:36`, `source-reference/Original/ArmDependencies/TeamOperator.cs:36`; develop `public field GameObject plasticuffsVisual` at `source-reference/Original/ArmDependencies/TeamOperator.cs:36`.
- **PR wins** `field:restrainOccupants`; PR `public field bool restrainOccupants` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:29`, `source-reference/Original/ArmDependencies/TeamOperator.cs:29`; develop `public field bool restrainOccupants` at `source-reference/Original/ArmDependencies/TeamOperator.cs:29`.
- **PR wins** `field:secureWeapons`; PR `public field bool secureWeapons` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:34`, `source-reference/Original/ArmDependencies/TeamOperator.cs:34`; develop `public field bool secureWeapons` at `source-reference/Original/ArmDependencies/TeamOperator.cs:34`.
- **PR wins** `field:vocalAnnounce`; PR `public field CharacterVocalization vocalAnnounce` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:12`, `source-reference/Original/ArmDependencies/TeamOperator.cs:12`; develop `public field CharacterVocalization vocalAnnounce` at `source-reference/Original/ArmDependencies/TeamOperator.cs:12`.
- **PR wins** `field:vocalDropWeapon`; PR `public field CharacterVocalization vocalDropWeapon` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:17`, `source-reference/Original/ArmDependencies/TeamOperator.cs:17`; develop `public field CharacterVocalization vocalDropWeapon` at `source-reference/Original/ArmDependencies/TeamOperator.cs:17`.
- **PR wins** `field:vocalMoveBackwards`; PR `public field CharacterVocalization vocalMoveBackwards` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:23`, `source-reference/Original/ArmDependencies/TeamOperator.cs:23`; develop `public field CharacterVocalization vocalMoveBackwards` at `source-reference/Original/ArmDependencies/TeamOperator.cs:23`.
- **PR wins** `field:vocalOnGauze`; PR `public field CharacterVocalization vocalOnGauze` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:27`, `source-reference/Original/ArmDependencies/TeamOperator.cs:27`; develop `public field CharacterVocalization vocalOnGauze` at `source-reference/Original/ArmDependencies/TeamOperator.cs:27`.
- **PR wins** `field:vocalOnKnees`; PR `public field CharacterVocalization vocalOnKnees` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:25`, `source-reference/Original/ArmDependencies/TeamOperator.cs:25`; develop `public field CharacterVocalization vocalOnKnees` at `source-reference/Original/ArmDependencies/TeamOperator.cs:25`.
- **PR wins** `field:vocalShowHands`; PR `public field CharacterVocalization vocalShowHands` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:19`, `source-reference/Original/ArmDependencies/TeamOperator.cs:19`; develop `public field CharacterVocalization vocalShowHands` at `source-reference/Original/ArmDependencies/TeamOperator.cs:19`.
- **PR wins** `field:vocalStop`; PR `public field CharacterVocalization vocalStop` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:15`, `source-reference/Original/ArmDependencies/TeamOperator.cs:15`; develop `public field CharacterVocalization vocalStop` at `source-reference/Original/ArmDependencies/TeamOperator.cs:15`.
- **PR wins** `field:vocalTurnAround`; PR `public field CharacterVocalization vocalTurnAround` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:21`, `source-reference/Original/ArmDependencies/TeamOperator.cs:21`; develop `public field CharacterVocalization vocalTurnAround` at `source-reference/Original/ArmDependencies/TeamOperator.cs:21`.
- **PR wins** ``method:AssignNewInteriorCameraSubject`0()``; PR `private void AssignNewInteriorCameraSubject()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:278`, `source-reference/Original/ArmDependencies/TeamOperator.cs:278`; develop `private void AssignNewInteriorCameraSubject()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:278`.
- **PR wins** ``method:Awake`0()``; PR `private void Awake()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:100`, `source-reference/Original/ArmDependencies/TeamOperator.cs:100`; develop `private void Awake()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:100`.
- **PR wins** ``method:CommandClearNextRoomAvailable`0()``; PR `public bool CommandClearNextRoomAvailable()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:331`, `source-reference/Original/ArmDependencies/TeamOperator.cs:331`; develop `public bool CommandClearNextRoomAvailable()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:331`.
- **PR wins** ``method:CoveringLeader`0()``; PR `public bool CoveringLeader()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:217`, `source-reference/Original/ArmDependencies/TeamOperator.cs:217`; develop `public bool CoveringLeader()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:217`.
- **PR wins** ``method:DestroyPlasticuffInHand`0()``; PR `public void DestroyPlasticuffInHand()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:350`, `source-reference/Original/ArmDependencies/TeamOperator.cs:350`; develop `public void DestroyPlasticuffInHand()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:350`.
- **PR wins** ``method:DropCellPhone`0()``; PR `public void DropCellPhone()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:241`, `source-reference/Original/ArmDependencies/TeamOperator.cs:241`; develop `public void DropCellPhone()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:241`.
- **PR wins** ``method:FollowDistance`0(TeamOperator)``; PR `public float FollowDistance(TeamOperator)` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:195`, `source-reference/Original/ArmDependencies/TeamOperator.cs:195`; develop `public float FollowDistance(TeamOperator)` at `source-reference/Original/ArmDependencies/TeamOperator.cs:195`.
- **PR wins** ``method:InstantiateGauzeInHand`0()``; PR `public void InstantiateGauzeInHand()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:358`, `source-reference/Original/ArmDependencies/TeamOperator.cs:358`; develop `public void InstantiateGauzeInHand()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:358`.
- **PR wins** ``method:InstantiatePlasticuffInHand`0()``; PR `public void InstantiatePlasticuffInHand()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:342`, `source-reference/Original/ArmDependencies/TeamOperator.cs:342`; develop `public void InstantiatePlasticuffInHand()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:342`.
- **PR wins** ``method:OnUnconscious`0()``; PR `private void OnUnconscious()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:273`, `source-reference/Original/ArmDependencies/TeamOperator.cs:273`; develop `private void OnUnconscious()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:273`.
- **PR wins** ``method:RemoveGauzeFromHand`0()``; PR `public void RemoveGauzeFromHand()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:366`, `source-reference/Original/ArmDependencies/TeamOperator.cs:366`; develop `public void RemoveGauzeFromHand()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:366`.
- **PR wins** ``method:ReportOperatorDown`0()``; PR `public void ReportOperatorDown()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:236`, `source-reference/Original/ArmDependencies/TeamOperator.cs:236`; develop `public void ReportOperatorDown()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:236`.
- **PR wins** ``method:ReportSuspectDown`0()``; PR `public void ReportSuspectDown()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:231`, `source-reference/Original/ArmDependencies/TeamOperator.cs:231`; develop `public void ReportSuspectDown()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:231`.
- **PR wins** ``method:SetEngagePosition`0()``; PR `private void SetEngagePosition()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:139`, `source-reference/Original/ArmDependencies/TeamOperator.cs:139`; develop `private void SetEngagePosition()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:139`.
- **PR wins** ``method:SetFollowPosition`0()``; PR `private void SetFollowPosition()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:156`, `source-reference/Original/ArmDependencies/TeamOperator.cs:156`; develop `private void SetFollowPosition()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:156`.
- **PR wins** ``method:SetLeadPosition`0()``; PR `private void SetLeadPosition()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:181`, `source-reference/Original/ArmDependencies/TeamOperator.cs:181`; develop `private void SetLeadPosition()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:181`.
- **PR wins** ``method:Start`0()``; PR `private void Start()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:113`, `source-reference/Original/ArmDependencies/TeamOperator.cs:113`; develop `private void Start()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:113`.
- **PR wins** ``method:UpdateAttention`0()``; PR `private void UpdateAttention()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:246`, `source-reference/Original/ArmDependencies/TeamOperator.cs:246`; develop `private void UpdateAttention()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:246`.
- **PR wins** ``method:UpdateFlashlight`0()``; PR `private void UpdateFlashlight()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:255`, `source-reference/Original/ArmDependencies/TeamOperator.cs:255`; develop `private void UpdateFlashlight()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:255`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:119`, `source-reference/Original/ArmDependencies/TeamOperator.cs:119`; develop `private void Update()` at `source-reference/Original/ArmDependencies/TeamOperator.cs:119`.
- **PR wins** ``method:Vocalize`0(CharacterBody)``; PR `public void Vocalize(CharacterBody)` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:287`, `source-reference/Original/ArmDependencies/TeamOperator.cs:287`; develop `public void Vocalize(CharacterBody)` at `source-reference/Original/ArmDependencies/TeamOperator.cs:287`.
- **PR wins** `property:Blackboard`; PR `public Blackboard Blackboard` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:52`, `source-reference/Original/ArmDependencies/TeamOperator.cs:52`; develop `public Blackboard Blackboard` at `source-reference/Original/ArmDependencies/TeamOperator.cs:52`.
- **PR wins** `property:Body`; PR `public CharacterBody Body` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:54`, `source-reference/Original/ArmDependencies/TeamOperator.cs:54`; develop `public CharacterBody Body` at `source-reference/Original/ArmDependencies/TeamOperator.cs:54`.
- **PR wins** `property:Communication`; PR `public Communication Communication` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:56`, `source-reference/Original/ArmDependencies/TeamOperator.cs:56`; develop `public Communication Communication` at `source-reference/Original/ArmDependencies/TeamOperator.cs:56`.
- **PR wins** `property:CurrentRoom`; PR `public Room CurrentRoom` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:66`, `source-reference/Original/ArmDependencies/TeamOperator.cs:66`; develop `public Room CurrentRoom` at `source-reference/Original/ArmDependencies/TeamOperator.cs:66`.
- **PR wins** `property:Door`; PR `public DoorController Door` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:96`, `source-reference/Original/ArmDependencies/TeamOperator.cs:96`; develop `public DoorController Door` at `source-reference/Original/ArmDependencies/TeamOperator.cs:96`.
- **PR wins** `property:FacingDoor`; PR `public bool FacingDoor` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:98`, `source-reference/Original/ArmDependencies/TeamOperator.cs:98`; develop `public bool FacingDoor` at `source-reference/Original/ArmDependencies/TeamOperator.cs:98`.
- **PR wins** `property:Follower`; PR `public TeamOperator Follower` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:64`, `source-reference/Original/ArmDependencies/TeamOperator.cs:64`; develop `public TeamOperator Follower` at `source-reference/Original/ArmDependencies/TeamOperator.cs:64`.
- **PR wins** `property:IsGroupLeader`; PR `public bool IsGroupLeader` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:80`, `source-reference/Original/ArmDependencies/TeamOperator.cs:80`; develop `public bool IsGroupLeader` at `source-reference/Original/ArmDependencies/TeamOperator.cs:80`.
- **PR wins** `property:IsRearGuard`; PR `public bool IsRearGuard` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:88`, `source-reference/Original/ArmDependencies/TeamOperator.cs:88`; develop `public bool IsRearGuard` at `source-reference/Original/ArmDependencies/TeamOperator.cs:88`.
- **PR wins** `property:Leader`; PR `public TeamOperator Leader` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:62`, `source-reference/Original/ArmDependencies/TeamOperator.cs:62`; develop `public TeamOperator Leader` at `source-reference/Original/ArmDependencies/TeamOperator.cs:62`.
- **PR wins** `property:Mission`; PR `public Mission Mission` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:60`, `source-reference/Original/ArmDependencies/TeamOperator.cs:60`; develop `public Mission Mission` at `source-reference/Original/ArmDependencies/TeamOperator.cs:60`.
- **PR wins** `property:NextRoom`; PR `public Room NextRoom` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:74`, `source-reference/Original/ArmDependencies/TeamOperator.cs:74`; develop `public Room NextRoom` at `source-reference/Original/ArmDependencies/TeamOperator.cs:74`.
- **PR wins** `property:Team`; PR `public TeamController Team` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:58`, `source-reference/Original/ArmDependencies/TeamOperator.cs:58`; develop `public TeamController Team` at `source-reference/Original/ArmDependencies/TeamOperator.cs:58`.
- **PR wins** `property:TeamPosition`; PR `public int TeamPosition` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:78`, `source-reference/Original/ArmDependencies/TeamOperator.cs:78`; develop `public int TeamPosition` at `source-reference/Original/ArmDependencies/TeamOperator.cs:78`.
- **PR wins** `property:TeamTask`; PR `public TeamActivity TeamTask` at `source-reference/Original/CharacterDependencies/TeamOperator.cs:76`, `source-reference/Original/ArmDependencies/TeamOperator.cs:76`; develop `public TeamActivity TeamTask` at `source-reference/Original/ArmDependencies/TeamOperator.cs:76`.


### 133. `TeamOperator+TeamActivity` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `TeamActivity`; partial `false`; `source-reference/Original/ArmDependencies/TeamOperator.cs:5`
- enum; namespace `(global)`; `TeamActivity`; partial `false`; `source-reference/Original/CharacterDependencies/TeamOperator.cs:5`

**develop declarations**:
- enum; namespace `(global)`; `TeamActivity`; partial `false`; `source-reference/Original/ArmDependencies/TeamOperator.cs:5`

Common member signatures: **0**.


### 134. `TimeScaler` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `TimeScaler`; partial `false`; `source-reference/Original/CharacterDependencies/TimeScaler.cs:4`

**develop declarations**:
- class; namespace `(global)`; `TimeScaler`; partial `false`; `source-reference/Original/CharacterDependencies/TimeScaler.cs:4`

Common member signatures: **40**.
- **PR wins** `field:bulletTimeDisabled`; PR `public field bool bulletTimeDisabled` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:20`; develop `public field bool bulletTimeDisabled` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:20`.
- **PR wins** `field:bulletTimeRate`; PR `private field float bulletTimeRate` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:38`; develop `private field float bulletTimeRate` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:38`.
- **PR wins** `field:bulletTimestamp`; PR `private field float bulletTimestamp` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:44`; develop `private field float bulletTimestamp` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:44`.
- **PR wins** `field:duration`; PR `private field float duration` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:34`; develop `private field float duration` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:34`.
- **PR wins** `field:easeIn`; PR `private field float easeIn` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:30`; develop `private field float easeIn` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:30`.
- **PR wins** `field:easeInFunction`; PR `public field AnimationCurve easeInFunction` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:16`; develop `public field AnimationCurve easeInFunction` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:16`.
- **PR wins** `field:easeOut`; PR `private field float easeOut` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:32`; develop `private field float easeOut` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:32`.
- **PR wins** `field:easeOutFunction`; PR `public field AnimationCurve easeOutFunction` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:18`; develop `public field AnimationCurve easeOutFunction` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:18`.
- **PR wins** `field:inputHandler`; PR `private field InputHandler inputHandler` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:28`; develop `private field InputHandler inputHandler` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:28`.
- **PR wins** `field:mixer`; PR `public field AudioMixer mixer` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:14`; develop `public field AudioMixer mixer` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:14`.
- **PR wins** `field:rateBeforePause`; PR `private field float rateBeforePause` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:40`; develop `private field float rateBeforePause` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:40`.
- **PR wins** `field:rateTimestamp`; PR `private field float rateTimestamp` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:42`; develop `private field float rateTimestamp` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:42`.
- **PR wins** `field:settings`; PR `private field SerializableTimeSettings settings` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:24`; develop `private field SerializableTimeSettings settings` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:24`.
- **PR wins** `field:slowMotionDisabled`; PR `public field bool slowMotionDisabled` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:22`; develop `public field bool slowMotionDisabled` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:22`.
- **PR wins** `field:startBulletTime`; PR `private field bool startBulletTime` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:46`; develop `private field bool startBulletTime` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:46`.
- **PR wins** `field:textIndicator`; PR `private field UIInteriorModeTextIndicator textIndicator` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:26`; develop `private field UIInteriorModeTextIndicator textIndicator` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:26`.
- **PR wins** `field:timeRate`; PR `private field float timeRate` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:36`; develop `private field float timeRate` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:36`.
- **PR wins** ``method:Awake`0()``; PR `private void Awake()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:110`; develop `private void Awake()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:110`.
- **PR wins** ``method:BulletTime`0(SerializableBulletTimeEntry)``; PR `public void BulletTime(SerializableBulletTimeEntry)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:164`; develop `public void BulletTime(SerializableBulletTimeEntry)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:164`.
- **PR wins** ``method:BulletTime`0(float, float, float, float)``; PR `private void BulletTime(float, float, float, float)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:172`; develop `private void BulletTime(float, float, float, float)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:172`.
- **PR wins** ``method:EnableSlowMotion`0(bool, bool)``; PR `public void EnableSlowMotion(bool, bool)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:196`; develop `public void EnableSlowMotion(bool, bool)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:196`.
- **PR wins** ``method:IndicateTargetRate`0()``; PR `private void IndicateTargetRate()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:330`; develop `private void IndicateTargetRate()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:330`.
- **PR wins** ``method:Instant`0(float)``; PR `public void Instant(float)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:152`; develop `public void Instant(float)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:152`.
- **PR wins** ``method:NormalizedTimeInMode`0()``; PR `private float NormalizedTimeInMode()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:312`; develop `private float NormalizedTimeInMode()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:312`.
- **PR wins** ``method:Pause`0(bool)``; PR `public void Pause(bool)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:124`; develop `public void Pause(bool)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:124`.
- **PR wins** ``method:SetSlowMotion`0(float, float, bool)``; PR `public void SetSlowMotion(float, float, bool)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:231`; develop `public void SetSlowMotion(float, float, bool)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:231`.
- **PR wins** ``method:SetTimeScale`0(float)``; PR `private void SetTimeScale(float)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:282`; develop `private void SetTimeScale(float)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:282`.
- **PR wins** ``method:SetTutorialMode`0(bool)``; PR `public void SetTutorialMode(bool)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:145`; develop `public void SetTutorialMode(bool)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:145`.
- **PR wins** ``method:Start`0()``; PR `private void Start()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:115`; develop `private void Start()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:115`.
- **PR wins** ``method:TimeRate`0(float, float)``; PR `public void TimeRate(float, float)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:157`; develop `public void TimeRate(float, float)` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:157`.
- **PR wins** ``method:ToggleBulletTime`0()``; PR `public void ToggleBulletTime()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:189`; develop `public void ToggleBulletTime()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:189`.
- **PR wins** ``method:ToggleSlowMotion`0()``; PR `public void ToggleSlowMotion()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:215`; develop `public void ToggleSlowMotion()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:215`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:240`; develop `private void Update()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:240`.
- **PR wins** ``method:UserInput`0()``; PR `private void UserInput()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:288`; develop `private void UserInput()` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:288`.
- **PR wins** `property:CurrentMode`; PR `public Mode CurrentMode` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:76`; develop `public Mode CurrentMode` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:76`.
- **PR wins** `property:InBulletTime`; PR `public bool InBulletTime` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:66`; develop `public bool InBulletTime` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:66`.
- **PR wins** `property:InRealTime`; PR `public bool InRealTime` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:50`; develop `public bool InRealTime` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:50`.
- **PR wins** `property:InSlowMotion`; PR `public bool InSlowMotion` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:58`; develop `public bool InSlowMotion` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:58`.
- **PR wins** `property:Paused`; PR `public bool Paused` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:48`; develop `public bool Paused` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:48`.
- **PR wins** `property:TargetRate`; PR `public float TargetRate` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:68`; develop `public float TargetRate` at `source-reference/Original/CharacterDependencies/TimeScaler.cs:68`.


### 135. `TimeScaler+Mode` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `Mode`; partial `false`; `source-reference/Original/CharacterDependencies/TimeScaler.cs:6`

**develop declarations**:
- enum; namespace `(global)`; `Mode`; partial `false`; `source-reference/Original/CharacterDependencies/TimeScaler.cs:6`

Common member signatures: **0**.


### 136. `TutorialSoftTarget` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `TutorialSoftTarget`; partial `false`; `runtime/Engine/ProjectileFlightHost.cs:5`

**develop declarations**:
- class; namespace `(global)`; `TutorialSoftTarget`; partial `false`; `runtime/Engine/ProjectileFlightHost.cs:5`

Common member signatures: **0**.


### 137. `UnityEngine.AnimationCurve` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `AnimationCurve`; partial `false`; `runtime/Engine/AnimationCurve.cs:7`

**develop declarations**:
- class; namespace `UnityEngine`; `AnimationCurve`; partial `false`; `runtime/Engine/AnimationCurve.cs:6`

Common member signatures: **8**.
- **PR wins** `constructor:AnimationCurve()`; PR `public UnityEngine.AnimationCurve()` at `runtime/Engine/AnimationCurve.cs:28`; develop `public UnityEngine.AnimationCurve()` at `runtime/Engine/AnimationCurve.cs:13`.
- **PR wins** `field:values`; PR `field Keyframe[] values` at `runtime/Engine/AnimationCurve.cs:8`; develop `readonly field Keyframe[] values` at `runtime/Engine/AnimationCurve.cs:7`.
- **PR wins** ``method:Evaluate`0(float)``; PR `public float Evaluate(float)` at `runtime/Engine/AnimationCurve.cs:30`; develop `public float Evaluate(float)` at `runtime/Engine/AnimationCurve.cs:15`.
- **PR wins** `property:clampOutside`; PR `public bool clampOutside` at `runtime/Engine/AnimationCurve.cs:25`; develop `public bool clampOutside` at `runtime/Engine/AnimationCurve.cs:10`.
- **PR wins** `property:keys`; PR `public Keyframe[] keys` at `runtime/Engine/AnimationCurve.cs:15`; develop `public Keyframe[] keys` at `runtime/Engine/AnimationCurve.cs:8`.
- **PR wins** `property:length`; PR `public int length` at `runtime/Engine/AnimationCurve.cs:24`; develop `public int length` at `runtime/Engine/AnimationCurve.cs:9`.
- **PR wins** `property:serializedPostInfinity`; PR `public int serializedPostInfinity` at `runtime/Engine/AnimationCurve.cs:27`; develop `public int serializedPostInfinity` at `runtime/Engine/AnimationCurve.cs:12`.
- **PR wins** `property:serializedPreInfinity`; PR `public int serializedPreInfinity` at `runtime/Engine/AnimationCurve.cs:26`; develop `public int serializedPreInfinity` at `runtime/Engine/AnimationCurve.cs:11`.


### 138. `UnityEngine.Animator` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Animator`; partial `false`; `runtime/Engine/Animator.cs:13`

**develop declarations**:
- class; namespace `UnityEngine`; `Animator`; partial `false`; `runtime/Engine/OriginalAnimatorParameters.cs:8`

Common member signatures: **10**.
- **PR wins** ``method:GetBool`0(int)``; PR `public bool GetBool(int)` at `runtime/Engine/Animator.cs:47`; develop `public bool GetBool(int)` at `runtime/Engine/OriginalAnimatorParameters.cs:22`.
- **PR wins** ``method:GetBool`0(string)``; PR `public bool GetBool(string)` at `runtime/Engine/Animator.cs:55`; develop `public bool GetBool(string)` at `runtime/Engine/OriginalAnimatorParameters.cs:27`.
- **PR wins** ``method:GetFloat`0(int)``; PR `public float GetFloat(int)` at `runtime/Engine/Animator.cs:48`; develop `public float GetFloat(int)` at `runtime/Engine/OriginalAnimatorParameters.cs:24`.
- **PR wins** ``method:GetFloat`0(string)``; PR `public float GetFloat(string)` at `runtime/Engine/Animator.cs:56`; develop `public float GetFloat(string)` at `runtime/Engine/OriginalAnimatorParameters.cs:28`.
- **PR wins** ``method:SetBool`0(int, bool)``; PR `public void SetBool(int, bool)` at `runtime/Engine/Animator.cs:42`; develop `public void SetBool(int, bool)` at `runtime/Engine/OriginalAnimatorParameters.cs:21`.
- **PR wins** ``method:SetBool`0(string, bool)``; PR `public void SetBool(string, bool)` at `runtime/Engine/Animator.cs:50`; develop `public void SetBool(string, bool)` at `runtime/Engine/OriginalAnimatorParameters.cs:25`.
- **PR wins** ``method:SetFloat`0(int, float)``; PR `public void SetFloat(int, float)` at `runtime/Engine/Animator.cs:43`; develop `public void SetFloat(int, float)` at `runtime/Engine/OriginalAnimatorParameters.cs:23`.
- **PR wins** ``method:SetFloat`0(string, float)``; PR `public void SetFloat(string, float)` at `runtime/Engine/Animator.cs:51`; develop `public void SetFloat(string, float)` at `runtime/Engine/OriginalAnimatorParameters.cs:26`.
- **PR wins** ``method:StringToHash`0(string)``; PR `public static int StringToHash(string)` at `runtime/Engine/Animator.cs:32`; develop `public static int StringToHash(string)` at `runtime/Engine/OriginalAnimatorParameters.cs:14`.
- **PR wins** `property:speed`; PR `public float speed` at `runtime/Engine/Animator.cs:24`; develop `public float speed` at `runtime/Engine/OriginalAnimatorParameters.cs:11`.

Develop-only member signatures: **4** — **DEVELOP-ONLY / DEFERRED**.
- **DEVELOP-ONLY — DEFERRED** `event:BoolChanged` — `public event Action<int,bool> BoolChanged` at `runtime/Engine/OriginalAnimatorParameters.cs:12`.
- **DEVELOP-ONLY — DEFERRED** `event:FloatChanged` — `public event Action<int,float> FloatChanged` at `runtime/Engine/OriginalAnimatorParameters.cs:13`.
- **DEVELOP-ONLY — DEFERRED** `field:bools` — `readonly field Dictionary<int,bool> bools` at `runtime/Engine/OriginalAnimatorParameters.cs:9`.
- **DEVELOP-ONLY — DEFERRED** `field:floats` — `readonly field Dictionary<int,float> floats` at `runtime/Engine/OriginalAnimatorParameters.cs:10`.


### 139. `UnityEngine.Behaviour` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Behaviour`; partial `false`; `runtime/Engine/Objects.cs:104`

**develop declarations**:
- class; namespace `UnityEngine`; `Behaviour`; partial `false`; `runtime/Engine/Objects.cs:73`

Common member signatures: **3**.
- **PR wins** `field:value`; PR `field bool value` at `runtime/Engine/Objects.cs:105`; develop `field bool value` at `runtime/Engine/Objects.cs:74`.
- **PR wins** `property:enabled`; PR `public virtual bool enabled` at `runtime/Engine/Objects.cs:106`; develop `public bool enabled` at `runtime/Engine/Objects.cs:75`.
- **PR wins** `property:isActiveAndEnabled`; PR `public bool isActiveAndEnabled` at `runtime/Engine/Objects.cs:107`; develop `public bool isActiveAndEnabled` at `runtime/Engine/Objects.cs:76`.


### 140. `UnityEngine.BoxCollider` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `BoxCollider`; partial `false`; `runtime/Engine/Physics.cs:30`

**develop declarations**:
- class; namespace `UnityEngine`; `BoxCollider`; partial `false`; `runtime/Engine/Physics.cs:18`

Common member signatures: **3**.
- **PR wins** ``method:Raycast`0(Ray, out RaycastHit, float)``; PR `public override bool Raycast(Ray, out RaycastHit, float)` at `runtime/Engine/Physics.cs:33`; develop `public override bool Raycast(Ray, out RaycastHit, float)` at `runtime/Engine/Physics.cs:21`.
- **PR wins** `property:center`; PR `public Vector3 center` at `runtime/Engine/Physics.cs:31`; develop `public Vector3 center` at `runtime/Engine/Physics.cs:19`.
- **PR wins** `property:size`; PR `public Vector3 size` at `runtime/Engine/Physics.cs:32`; develop `public Vector3 size` at `runtime/Engine/Physics.cs:20`.


### 141. `UnityEngine.Camera` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Camera`; partial `false`; `runtime/Engine/PhysicsComponents.cs:5`

**develop declarations**:
- class; namespace `UnityEngine`; `Camera`; partial `false`; `runtime/Engine/PresentationHost.cs:5`

Common member signatures: **2**.
- **PR wins** `field:aspect`; PR `public field float aspect` at `runtime/Engine/PhysicsComponents.cs:7`; develop `public field float aspect` at `runtime/Engine/PresentationHost.cs:5`.
- **PR wins** `field:fieldOfView`; PR `public field float fieldOfView` at `runtime/Engine/PhysicsComponents.cs:7`; develop `public field float fieldOfView` at `runtime/Engine/PresentationHost.cs:5`.


### 142. `UnityEngine.CapsuleCollider` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `CapsuleCollider`; partial `false`; `runtime/Engine/CharacterPhysics.cs:6`

**develop declarations**:
- class; namespace `UnityEngine`; `CapsuleCollider`; partial `false`; `runtime/Engine/OriginalCapsuleCollider.cs:6`

Common member signatures: **5**.
- **PR wins** ``method:Raycast`0(Ray, out RaycastHit, float)``; PR `public override bool Raycast(Ray, out RaycastHit, float)` at `runtime/Engine/CharacterPhysics.cs:26`; develop `public override bool Raycast(Ray, out RaycastHit, float)` at `runtime/Engine/OriginalCapsuleCollider.cs:11`.
- **PR wins** `property:center`; PR `public Vector3 center` at `runtime/Engine/CharacterPhysics.cs:7`; develop `public Vector3 center` at `runtime/Engine/OriginalCapsuleCollider.cs:7`.
- **PR wins** `property:direction`; PR `public int direction` at `runtime/Engine/CharacterPhysics.cs:11`; develop `public int direction` at `runtime/Engine/OriginalCapsuleCollider.cs:10`.
- **PR wins** `property:height`; PR `public float height` at `runtime/Engine/CharacterPhysics.cs:9`; develop `public float height` at `runtime/Engine/OriginalCapsuleCollider.cs:9`.
- **PR wins** `property:radius`; PR `public float radius` at `runtime/Engine/CharacterPhysics.cs:8`; develop `public float radius` at `runtime/Engine/OriginalCapsuleCollider.cs:8`.

Develop-only member signatures: **2** — **DEVELOP-ONLY / DEFERRED**.
- **DEVELOP-ONLY — DEFERRED** ``method:Axis`0(Vector3)`` — `float Axis(Vector3)` at `runtime/Engine/OriginalCapsuleCollider.cs:48`.
- **DEVELOP-ONLY — DEFERRED** ``method:Unit`0()`` — `Vector3 Unit()` at `runtime/Engine/OriginalCapsuleCollider.cs:49`.


### 143. `UnityEngine.Collider` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Collider`; partial `false`; `runtime/Engine/Physics.cs:13`

**develop declarations**:
- class; namespace `UnityEngine`; `Collider`; partial `false`; `runtime/Engine/Physics.cs:13`

Common member signatures: **3**.
- **PR wins** ``method:Raycast`0(Ray, out RaycastHit, float)``; PR `public abstract bool Raycast(Ray, out RaycastHit, float)` at `runtime/Engine/Physics.cs:28`; develop `public abstract bool Raycast(Ray, out RaycastHit, float)` at `runtime/Engine/Physics.cs:16`.
- **PR wins** `property:attachedRigidbody`; PR `public Rigidbody attachedRigidbody` at `runtime/Engine/Physics.cs:15`; develop `public Rigidbody attachedRigidbody` at `runtime/Engine/Physics.cs:14`.
- **PR wins** `property:isTrigger`; PR `public bool isTrigger` at `runtime/Engine/Physics.cs:16`; develop `public bool isTrigger` at `runtime/Engine/Physics.cs:15`.


### 144. `UnityEngine.Color` — PR type wins

**PR declarations**:
- struct; namespace `UnityEngine`; `Color`; partial `false`; `runtime/Engine/Rendering/GraphicsPrimitives.cs:3`

**develop declarations**:
- struct; namespace `UnityEngine`; `Color`; partial `false`; `runtime/Engine/BallisticEngine.cs:4`

Common member signatures: **5**.
- **PR wins** `constructor:Color(float, float, float, float)`; PR `public UnityEngine.Color(float, float, float, float)` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:7`; develop `public UnityEngine.Color(float, float, float, float)` at `runtime/Engine/BallisticEngine.cs:4`.
- **PR wins** `field:a`; PR `public field float a` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:7`; develop `public field float a` at `runtime/Engine/BallisticEngine.cs:4`.
- **PR wins** `field:b`; PR `public field float b` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:7`; develop `public field float b` at `runtime/Engine/BallisticEngine.cs:4`.
- **PR wins** `field:g`; PR `public field float g` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:7`; develop `public field float g` at `runtime/Engine/BallisticEngine.cs:4`.
- **PR wins** `field:r`; PR `public field float r` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:7`; develop `public field float r` at `runtime/Engine/BallisticEngine.cs:4`.


### 145. `UnityEngine.Component` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Component`; partial `false`; `runtime/Engine/Objects.cs:90`

**develop declarations**:
- class; namespace `UnityEngine`; `Component`; partial `false`; `runtime/Engine/Objects.cs:62`

Common member signatures: **7**.
- **PR wins** ``method:GetComponentInParent`1()``; PR `public T GetComponentInParent<T>()` at `runtime/Engine/Objects.cs:100`; develop `public T GetComponentInParent<T>()` at `runtime/Engine/Objects.cs:69`.
- **PR wins** ``method:GetComponent`1()``; PR `public T GetComponent<T>()` at `runtime/Engine/Objects.cs:97`; develop `public T GetComponent<T>()` at `runtime/Engine/Objects.cs:66`.
- **PR wins** ``method:GetComponentsInChildren`1(bool)``; PR `public T[] GetComponentsInChildren<T>(bool)` at `runtime/Engine/Objects.cs:99`; develop `public T[] GetComponentsInChildren<T>(bool)` at `runtime/Engine/Objects.cs:68`.
- **PR wins** ``method:GetComponents`1()``; PR `public T[] GetComponents<T>()` at `runtime/Engine/Objects.cs:98`; develop `public T[] GetComponents<T>()` at `runtime/Engine/Objects.cs:67`.
- **PR wins** `property:gameObject`; PR `public GameObject gameObject` at `runtime/Engine/Objects.cs:94`; develop `public GameObject gameObject` at `runtime/Engine/Objects.cs:63`.
- **PR wins** `property:name`; PR `public override string name` at `runtime/Engine/Objects.cs:96`; develop `public override string name` at `runtime/Engine/Objects.cs:65`.
- **PR wins** `property:transform`; PR `public Transform transform` at `runtime/Engine/Objects.cs:95`; develop `public Transform transform` at `runtime/Engine/Objects.cs:64`.


### 146. `UnityEngine.CoroutineHandle` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `CoroutineHandle`; partial `false`; `runtime/Engine/ClockInput.cs:45`

**develop declarations**:
- class; namespace `UnityEngine`; `CoroutineHandle`; partial `false`; `runtime/Engine/ClockInput.cs:44`

Common member signatures: **8**.
- **PR wins** `constructor:CoroutineHandle(MonoBehaviour, IEnumerator)`; PR `internal UnityEngine.CoroutineHandle(MonoBehaviour, IEnumerator)` at `runtime/Engine/ClockInput.cs:52`; develop `internal UnityEngine.CoroutineHandle(MonoBehaviour, IEnumerator)` at `runtime/Engine/ClockInput.cs:49`.
- **PR wins** `field:finished`; PR `internal field bool finished` at `runtime/Engine/ClockInput.cs:47`; develop `internal field bool finished` at `runtime/Engine/ClockInput.cs:46`.
- **PR wins** `field:frame`; PR `field int frame` at `runtime/Engine/ClockInput.cs:51`; develop `field int frame` at `runtime/Engine/ClockInput.cs:48`.
- **PR wins** `field:owner`; PR `internal readonly field MonoBehaviour owner` at `runtime/Engine/ClockInput.cs:46`; develop `internal readonly field MonoBehaviour owner` at `runtime/Engine/ClockInput.cs:45`.
- **PR wins** `field:real`; PR `field bool real` at `runtime/Engine/ClockInput.cs:51`; develop `field bool real` at `runtime/Engine/ClockInput.cs:48`.
- **PR wins** `field:stack`; PR `readonly field Stack<IEnumerator> stack` at `runtime/Engine/ClockInput.cs:50`; develop `readonly field Stack<IEnumerator> stack` at `runtime/Engine/ClockInput.cs:47`.
- **PR wins** `field:wake`; PR `field float wake` at `runtime/Engine/ClockInput.cs:51`; develop `field float wake` at `runtime/Engine/ClockInput.cs:48`.
- **PR wins** ``method:Advance`0(bool)``; PR `internal void Advance(bool)` at `runtime/Engine/ClockInput.cs:53`; develop `internal void Advance(bool)` at `runtime/Engine/ClockInput.cs:50`.


### 147. `UnityEngine.Debug` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Debug`; partial `false`; `runtime/Engine/Math.cs:137`

**develop declarations**:
- class; namespace `UnityEngine`; `Debug`; partial `false`; `runtime/Engine/Math.cs:82`

Common member signatures: **1**.
- **PR wins** ``method:LogWarningFormat`0(string)``; PR `public static void LogWarningFormat(string)` at `runtime/Engine/Math.cs:145`; develop `public static void LogWarningFormat(string)` at `runtime/Engine/Math.cs:82`.


### 148. `UnityEngine.Events.UnityAction` — PR type wins

**PR declarations**:
- delegate; namespace `UnityEngine.Events`; `UnityAction`; partial `false`; `runtime/Engine/WeaponHost.cs:5`
- delegate; namespace `UnityEngine.Events`; `UnityAction`; partial `false`; `runtime/EngineChecks/UnityActionSourceBoundary.cs:6`

**develop declarations**:
- delegate; namespace `UnityEngine.Events`; `UnityAction`; partial `false`; `runtime/Engine/WeaponHost.cs:5`

Common member signatures: **0**.


### 149. ``UnityEngine.Events.UnityAction`1`` — PR type wins

**PR declarations**:
- delegate; namespace `UnityEngine.Events`; `UnityAction`; partial `false`; `runtime/Engine/WeaponHost.cs:5`
- delegate; namespace `UnityEngine.Events`; `UnityAction`; partial `false`; `runtime/EngineChecks/UnityActionSourceBoundary.cs:7`

**develop declarations**:
- delegate; namespace `UnityEngine.Events`; `UnityAction`; partial `false`; `runtime/Engine/WeaponHost.cs:5`

Common member signatures: **1**.
- **PR wins** `constructor:UnityAction(T)`; PR `UnityAction(T)` at `runtime/EngineChecks/UnityActionSourceBoundary.cs:7`, `runtime/Engine/WeaponHost.cs:5`; develop `UnityAction(T)` at `runtime/Engine/WeaponHost.cs:5`.


### 150. ``UnityEngine.Events.UnityAction`2`` — PR type wins

**PR declarations**:
- delegate; namespace `UnityEngine.Events`; `UnityAction`; partial `false`; `runtime/Engine/WeaponHost.cs:5`
- delegate; namespace `UnityEngine.Events`; `UnityAction`; partial `false`; `runtime/EngineChecks/UnityActionSourceBoundary.cs:8`

**develop declarations**:
- delegate; namespace `UnityEngine.Events`; `UnityAction`; partial `false`; `runtime/Engine/WeaponHost.cs:5`

Common member signatures: **1**.
- **PR wins** `constructor:UnityAction(T, U)`; PR `UnityAction(T, U)` at `runtime/EngineChecks/UnityActionSourceBoundary.cs:8`, `runtime/Engine/WeaponHost.cs:5`; develop `UnityAction(T, U)` at `runtime/Engine/WeaponHost.cs:5`.

