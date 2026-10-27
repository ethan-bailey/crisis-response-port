# PR #42 collision list — part 2 of 4

Checkpoint: PR `a653609b62f3e3c79f73d0a17f0bf3dc89eae7b8`; develop `2fc0a8e93be97c4d99123910fad4bd25798e71a8`. PR side wins each collision.

### 51. `Crisis.Runtime.RuntimeContracts` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime`; `RuntimeContracts`; partial `false`; `runtime/Engine/RuntimeContracts.cs:23`

**develop declarations**:
- class; namespace `Crisis.Runtime`; `RuntimeContracts`; partial `false`; `runtime/Engine/RuntimeContracts.cs:23`

Common member signatures: **3**.
- **PR wins** ``method:Check`0(bool, string)``; PR `static void Check(bool, string)` at `runtime/Engine/RuntimeContracts.cs:24`; develop `static void Check(bool, string)` at `runtime/Engine/RuntimeContracts.cs:24`.
- **PR wins** ``method:Near`0(float, float, string)``; PR `static void Near(float, float, string)` at `runtime/Engine/RuntimeContracts.cs:25`; develop `static void Near(float, float, string)` at `runtime/Engine/RuntimeContracts.cs:25`.
- **PR wins** ``method:Run`0()``; PR `public static string Run()` at `runtime/Engine/RuntimeContracts.cs:45`; develop `public static string Run()` at `runtime/Engine/RuntimeContracts.cs:26`.


### 52. `Crisis.Runtime.SceneLoadResult` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime`; `SceneLoadResult`; partial `false`; `runtime/Engine/SceneLoader.cs:9`

**develop declarations**:
- class; namespace `Crisis.Runtime`; `SceneLoadResult`; partial `false`; `runtime/Engine/SceneLoader.cs:8`

Common member signatures: **11**.
- **PR wins** ``method:Report`0()``; PR `public object Report()` at `runtime/Engine/SceneLoader.cs:22`; develop `public object Report()` at `runtime/Engine/SceneLoader.cs:19`.
- **PR wins** `property:boxColliders`; PR `public int boxColliders` at `runtime/Engine/SceneLoader.cs:14`; develop `public int boxColliders` at `runtime/Engine/SceneLoader.cs:13`.
- **PR wins** `property:gameObjects`; PR `public int gameObjects` at `runtime/Engine/SceneLoader.cs:12`; develop `public int gameObjects` at `runtime/Engine/SceneLoader.cs:11`.
- **PR wins** `property:objects`; PR `public Dictionary<string,UObject> objects` at `runtime/Engine/SceneLoader.cs:18`; develop `public Dictionary<string,UObject> objects` at `runtime/Engine/SceneLoader.cs:17`.
- **PR wins** `property:originalComponents`; PR `public int originalComponents` at `runtime/Engine/SceneLoader.cs:15`; develop `public int originalComponents` at `runtime/Engine/SceneLoader.cs:14`.
- **PR wins** `property:scene`; PR `public string scene` at `runtime/Engine/SceneLoader.cs:10`; develop `public string scene` at `runtime/Engine/SceneLoader.cs:9`.
- **PR wins** `property:serializedRecords`; PR `public int serializedRecords` at `runtime/Engine/SceneLoader.cs:11`; develop `public int serializedRecords` at `runtime/Engine/SceneLoader.cs:10`.
- **PR wins** `property:transforms`; PR `public int transforms` at `runtime/Engine/SceneLoader.cs:13`; develop `public int transforms` at `runtime/Engine/SceneLoader.cs:12`.
- **PR wins** `property:unresolvedReferences`; PR `public List<string> unresolvedReferences` at `runtime/Engine/SceneLoader.cs:17`; develop `public List<string> unresolvedReferences` at `runtime/Engine/SceneLoader.cs:16`.
- **PR wins** `property:unsupportedComponents`; PR `public Dictionary<string,int> unsupportedComponents` at `runtime/Engine/SceneLoader.cs:16`; develop `public Dictionary<string,int> unsupportedComponents` at `runtime/Engine/SceneLoader.cs:15`.
- **PR wins** `property:world`; PR `public RuntimeWorld world` at `runtime/Engine/SceneLoader.cs:20`; develop `public RuntimeWorld world` at `runtime/Engine/SceneLoader.cs:18`.


### 53. `Crisis.Runtime.SceneLoader` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime`; `SceneLoader`; partial `false`; `runtime/Engine/SceneLoader.cs:24`

**develop declarations**:
- class; namespace `Crisis.Runtime`; `SceneLoader`; partial `false`; `runtime/Engine/SceneLoader.cs:21`

Common member signatures: **7**.
- **PR wins** ``method:Flag`0(JsonElement, string, bool)``; PR `static bool Flag(JsonElement, string, bool)` at `runtime/Engine/SceneLoader.cs:26`; develop `static bool Flag(JsonElement, string, bool)` at `runtime/Engine/SceneLoader.cs:27`.
- **PR wins** ``method:Load`0(string, string, bool)``; PR `public static SceneLoadResult Load(string, string, bool)` at `runtime/Engine/SceneLoader.cs:42`; develop `public static SceneLoadResult Load(string, string, bool)` at `runtime/Engine/SceneLoader.cs:43`.
- **PR wins** ``method:Number`0(JsonElement, string, float)``; PR `static float Number(JsonElement, string, float)` at `runtime/Engine/SceneLoader.cs:27`; develop `static float Number(JsonElement, string, float)` at `runtime/Engine/SceneLoader.cs:28`.
- **PR wins** ``method:Ref`0(JsonElement)``; PR `static string Ref(JsonElement)` at `runtime/Engine/SceneLoader.cs:25`; develop `static string Ref(JsonElement)` at `runtime/Engine/SceneLoader.cs:26`.
- **PR wins** ``method:Rotation`0(JsonElement)``; PR `static Quaternion Rotation(JsonElement)` at `runtime/Engine/SceneLoader.cs:29`; develop `static Quaternion Rotation(JsonElement)` at `runtime/Engine/SceneLoader.cs:30`.
- **PR wins** ``method:Vector`0(JsonElement)``; PR `static Vector3 Vector(JsonElement)` at `runtime/Engine/SceneLoader.cs:28`; develop `static Vector3 Vector(JsonElement)` at `runtime/Engine/SceneLoader.cs:29`.
- **PR wins** ``method:VerifyImportedRay`0(SceneLoadResult)``; PR `public static object VerifyImportedRay(SceneLoadResult)` at `runtime/Engine/SceneLoader.cs:30`; develop `public static object VerifyImportedRay(SceneLoadResult)` at `runtime/Engine/SceneLoader.cs:31`.

Develop-only member signatures: **1** — **DEVELOP-ONLY / DEFERRED**.
- **DEVELOP-ONLY — DEFERRED** `field:scripts` — `static readonly field Dictionary<string,Type> scripts` at `runtime/Engine/SceneLoader.cs:22`.


### 54. `Crisis.Runtime.Weapons.AmmunitionStack` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `AmmunitionStack`; partial `false`; `runtime/Engine/WeaponHost.cs:53`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `AmmunitionStack`; partial `false`; `runtime/Engine/WeaponHost.cs:53`

Common member signatures: **2**.
- **PR wins** `field:caliber`; PR `public field string caliber` at `runtime/Engine/WeaponHost.cs:53`; develop `public field string caliber` at `runtime/Engine/WeaponHost.cs:53`.
- **PR wins** `field:count`; PR `public field int count` at `runtime/Engine/WeaponHost.cs:53`; develop `public field int count` at `runtime/Engine/WeaponHost.cs:53`.


### 55. `Crisis.Runtime.Weapons.ArmIKSolver` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `ArmIKSolver`; partial `false`; `runtime/Original/Weapons/ArmTransition.cs:8`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `ArmIKSolver`; partial `false`; `runtime/Original/Weapons/ArmTransition.cs:8`

Common member signatures: **40**.
- **PR wins** `field:HalfPi`; PR `private static field float HalfPi` at `runtime/Original/Weapons/ArmTransition.cs:16`; develop `private static field float HalfPi` at `runtime/Original/Weapons/ArmTransition.cs:16`.
- **PR wins** `field:flip`; PR `public field bool flip` at `runtime/Original/Weapons/ArmTransition.cs:10`; develop `public field bool flip` at `runtime/Original/Weapons/ArmTransition.cs:10`.
- **PR wins** `field:hand`; PR `public field Hand hand` at `runtime/Original/Weapons/ArmTransition.cs:11`; develop `public field Hand hand` at `runtime/Original/Weapons/ArmTransition.cs:11`.
- **PR wins** `field:handLimb`; PR `private field Limb handLimb` at `runtime/Original/Weapons/ArmTransition.cs:12`; develop `private field Limb handLimb` at `runtime/Original/Weapons/ArmTransition.cs:12`.
- **PR wins** `field:ikTarget`; PR `public field Transform ikTarget` at `runtime/Original/Weapons/ArmTransition.cs:12`; develop `public field Transform ikTarget` at `runtime/Original/Weapons/ArmTransition.cs:12`.
- **PR wins** `field:interpolation`; PR `public field AnimationCurve interpolation` at `runtime/Original/Weapons/ArmTransition.cs:10`; develop `public field AnimationCurve interpolation` at `runtime/Original/Weapons/ArmTransition.cs:10`.
- **PR wins** `field:lowerArm`; PR `public field Limb lowerArm` at `runtime/Original/Weapons/ArmTransition.cs:11`; develop `public field Limb lowerArm` at `runtime/Original/Weapons/ArmTransition.cs:11`.
- **PR wins** `field:lowerArmAngle`; PR `private field float lowerArmAngle` at `runtime/Original/Weapons/ArmTransition.cs:14`; develop `private field float lowerArmAngle` at `runtime/Original/Weapons/ArmTransition.cs:14`.
- **PR wins** `field:lowerArmLength`; PR `private field float lowerArmLength` at `runtime/Original/Weapons/ArmTransition.cs:14`; develop `private field float lowerArmLength` at `runtime/Original/Weapons/ArmTransition.cs:14`.
- **PR wins** `field:smoothFactor`; PR `public field float smoothFactor` at `runtime/Original/Weapons/ArmTransition.cs:11`; develop `public field float smoothFactor` at `runtime/Original/Weapons/ArmTransition.cs:11`.
- **PR wins** `field:smoothing`; PR `public field bool smoothing` at `runtime/Original/Weapons/ArmTransition.cs:11`; develop `public field bool smoothing` at `runtime/Original/Weapons/ArmTransition.cs:11`.
- **PR wins** `field:transition`; PR `private field bool transition` at `runtime/Original/Weapons/ArmTransition.cs:16`; develop `private field bool transition` at `runtime/Original/Weapons/ArmTransition.cs:16`.
- **PR wins** `field:transitionHandAngle`; PR `private field float transitionHandAngle` at `runtime/Original/Weapons/ArmTransition.cs:15`; develop `private field float transitionHandAngle` at `runtime/Original/Weapons/ArmTransition.cs:15`.
- **PR wins** `field:transitionHandMode`; PR `private field Hand.Mode transitionHandMode` at `runtime/Original/Weapons/ArmTransition.cs:13`; develop `private field Hand.Mode transitionHandMode` at `runtime/Original/Weapons/ArmTransition.cs:13`.
- **PR wins** `field:transitionHandRotationMode`; PR `private field HandRotationMode transitionHandRotationMode` at `runtime/Original/Weapons/ArmTransition.cs:13`; develop `private field HandRotationMode transitionHandRotationMode` at `runtime/Original/Weapons/ArmTransition.cs:13`.
- **PR wins** `field:transitionPeriod`; PR `private field float transitionPeriod` at `runtime/Original/Weapons/ArmTransition.cs:15`; develop `private field float transitionPeriod` at `runtime/Original/Weapons/ArmTransition.cs:15`.
- **PR wins** `field:transitionTarget`; PR `public field Transform transitionTarget` at `runtime/Original/Weapons/ArmTransition.cs:12`; develop `public field Transform transitionTarget` at `runtime/Original/Weapons/ArmTransition.cs:12`.
- **PR wins** `field:transitionTimestamp`; PR `private field float transitionTimestamp` at `runtime/Original/Weapons/ArmTransition.cs:15`; develop `private field float transitionTimestamp` at `runtime/Original/Weapons/ArmTransition.cs:15`.
- **PR wins** `field:upperArm`; PR `public field Limb upperArm` at `runtime/Original/Weapons/ArmTransition.cs:11`; develop `public field Limb upperArm` at `runtime/Original/Weapons/ArmTransition.cs:11`.
- **PR wins** `field:upperArmAngle`; PR `private field float upperArmAngle` at `runtime/Original/Weapons/ArmTransition.cs:14`; develop `private field float upperArmAngle` at `runtime/Original/Weapons/ArmTransition.cs:14`.
- **PR wins** `field:upperArmLength`; PR `private field float upperArmLength` at `runtime/Original/Weapons/ArmTransition.cs:14`; develop `private field float upperArmLength` at `runtime/Original/Weapons/ArmTransition.cs:14`.
- **PR wins** ``method:Awake`0()``; PR `private void Awake()` at `runtime/Original/Weapons/ArmTransition.cs:34`; develop `private void Awake()` at `runtime/Original/Weapons/ArmTransition.cs:34`.
- **PR wins** ``method:GetHandAngle`0(HandRotationMode)``; PR `public float GetHandAngle(HandRotationMode)` at `runtime/Original/Weapons/ArmTransition.cs:137`; develop `public float GetHandAngle(HandRotationMode)` at `runtime/Original/Weapons/ArmTransition.cs:137`.
- **PR wins** ``method:InitializeSourcePose`0()``; PR `public void InitializeSourcePose()` at `runtime/Original/Weapons/ArmTransition.cs:32`; develop `public void InitializeSourcePose()` at `runtime/Original/Weapons/ArmTransition.cs:32`.
- **PR wins** ``method:ReturnTransforms`0()``; PR `public void ReturnTransforms()` at `runtime/Original/Weapons/ArmTransition.cs:119`; develop `public void ReturnTransforms()` at `runtime/Original/Weapons/ArmTransition.cs:119`.
- **PR wins** ``method:SetHandAngle`0(float, HandRotationMode)``; PR `public void SetHandAngle(float, HandRotationMode)` at `runtime/Original/Weapons/ArmTransition.cs:125`; develop `public void SetHandAngle(float, HandRotationMode)` at `runtime/Original/Weapons/ArmTransition.cs:125`.
- **PR wins** ``method:SetOnWeapon`0(WeaponBase, float, bool)``; PR `public void SetOnWeapon(WeaponBase, float, bool)` at `runtime/Original/Weapons/ArmTransition.cs:151`; develop `public void SetOnWeapon(WeaponBase, float, bool)` at `runtime/Original/Weapons/ArmTransition.cs:151`.
- **PR wins** ``method:SetTargetImmediate`0(Transform, Hand.Mode, float, HandRotationMode)``; PR `public void SetTargetImmediate(Transform, Hand.Mode, float, HandRotationMode)` at `runtime/Original/Weapons/ArmTransition.cs:87`; develop `public void SetTargetImmediate(Transform, Hand.Mode, float, HandRotationMode)` at `runtime/Original/Weapons/ArmTransition.cs:87`.
- **PR wins** ``method:SetUnarmed`0()``; PR `public void SetUnarmed()` at `runtime/Original/Weapons/ArmTransition.cs:181`; develop `public void SetUnarmed()` at `runtime/Original/Weapons/ArmTransition.cs:181`.
- **PR wins** ``method:StartWait`0()``; PR `public void StartWait()` at `runtime/Original/Weapons/ArmTransition.cs:115`; develop `public void StartWait()` at `runtime/Original/Weapons/ArmTransition.cs:115`.
- **PR wins** ``method:Step`0()``; PR `public void Step()` at `runtime/Original/Weapons/ArmTransition.cs:33`; develop `public void Step()` at `runtime/Original/Weapons/ArmTransition.cs:33`.
- **PR wins** ``method:TargetTransition`0(Transform, Vector3, float, Hand.Mode, float, HandRotationMode)``; PR `public void TargetTransition(Transform, Vector3, float, Hand.Mode, float, HandRotationMode)` at `runtime/Original/Weapons/ArmTransition.cs:100`; develop `public void TargetTransition(Transform, Vector3, float, Hand.Mode, float, HandRotationMode)` at `runtime/Original/Weapons/ArmTransition.cs:100`.
- **PR wins** ``method:TargetTransition`0(Transform, float, Hand.Mode, float, HandRotationMode)``; PR `public void TargetTransition(Transform, float, Hand.Mode, float, HandRotationMode)` at `runtime/Original/Weapons/ArmTransition.cs:96`; develop `public void TargetTransition(Transform, float, Hand.Mode, float, HandRotationMode)` at `runtime/Original/Weapons/ArmTransition.cs:96`.
- **PR wins** ``method:UpdateTransition`0()``; PR `private void UpdateTransition()` at `runtime/Original/Weapons/ArmTransition.cs:65`; develop `private void UpdateTransition()` at `runtime/Original/Weapons/ArmTransition.cs:65`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `runtime/Original/Weapons/ArmTransition.cs:44`; develop `private void Update()` at `runtime/Original/Weapons/ArmTransition.cs:44`.
- **PR wins** `property:HasSolution`; PR `public bool HasSolution` at `runtime/Original/Weapons/ArmTransition.cs:17`; develop `public bool HasSolution` at `runtime/Original/Weapons/ArmTransition.cs:17`.
- **PR wins** `property:OnTarget`; PR `public bool OnTarget` at `runtime/Original/Weapons/ArmTransition.cs:17`; develop `public bool OnTarget` at `runtime/Original/Weapons/ArmTransition.cs:17`.
- **PR wins** `property:OnWeapon`; PR `public bool OnWeapon` at `runtime/Original/Weapons/ArmTransition.cs:17`; develop `public bool OnWeapon` at `runtime/Original/Weapons/ArmTransition.cs:17`.
- **PR wins** `property:TransitionTarget`; PR `public Transform TransitionTarget` at `runtime/Original/Weapons/ArmTransition.cs:19`; develop `public Transform TransitionTarget` at `runtime/Original/Weapons/ArmTransition.cs:19`.
- **PR wins** `property:Transitioning`; PR `public bool Transitioning` at `runtime/Original/Weapons/ArmTransition.cs:18`; develop `public bool Transitioning` at `runtime/Original/Weapons/ArmTransition.cs:18`.


### 56. `Crisis.Runtime.Weapons.ArmIKSolver+HandRotationMode` — PR type wins

**PR declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `HandRotationMode`; partial `false`; `runtime/Original/Weapons/ArmTransition.cs:9`

**develop declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `HandRotationMode`; partial `false`; `runtime/Original/Weapons/ArmTransition.cs:9`

Common member signatures: **0**.


### 57. `Crisis.Runtime.Weapons.Blackboard` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Blackboard`; partial `false`; `runtime/Engine/WeaponHost.cs:37`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Blackboard`; partial `false`; `runtime/Engine/WeaponHost.cs:37`

Common member signatures: **1**.
- **PR wins** `field:weaponSkill`; PR `public field float weaponSkill` at `runtime/Engine/WeaponHost.cs:37`; develop `public field float weaponSkill` at `runtime/Engine/WeaponHost.cs:37`.


### 58. `Crisis.Runtime.Weapons.Casing` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Casing`; partial `false`; `runtime/Engine/WeaponHost.cs:20`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Casing`; partial `false`; `runtime/Engine/WeaponHost.cs:20`

Common member signatures: **4**.
- **PR wins** `field:Offset`; PR `public field float Offset` at `runtime/Engine/WeaponHost.cs:21`; develop `public field float Offset` at `runtime/Engine/WeaponHost.cs:21`.
- **PR wins** ``method:Detonate`0()``; PR `public void Detonate()` at `runtime/Engine/WeaponHost.cs:22`; develop `public void Detonate()` at `runtime/Engine/WeaponHost.cs:22`.
- **PR wins** `property:Live`; PR `public bool Live` at `runtime/Engine/WeaponHost.cs:21`; develop `public bool Live` at `runtime/Engine/WeaponHost.cs:21`.
- **PR wins** `property:Spent`; PR `public bool Spent` at `runtime/Engine/WeaponHost.cs:21`; develop `public bool Spent` at `runtime/Engine/WeaponHost.cs:21`.


### 59. `Crisis.Runtime.Weapons.ChangeMagazine` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `ChangeMagazine`; partial `false`; `runtime/Original/Weapons/ChangeMagazine.cs:10`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `ChangeMagazine`; partial `false`; `runtime/Original/Weapons/ChangeMagazine.cs:10`

Common member signatures: **20**.
- **PR wins** `field:automaticBoltFeed`; PR `private field bool automaticBoltFeed` at `runtime/Original/Weapons/ChangeMagazine.cs:55`; develop `private field bool automaticBoltFeed` at `runtime/Original/Weapons/ChangeMagazine.cs:55`.
- **PR wins** `field:bolt`; PR `private field CyclingBolt bolt` at `runtime/Original/Weapons/ChangeMagazine.cs:39`; develop `private field CyclingBolt bolt` at `runtime/Original/Weapons/ChangeMagazine.cs:39`.
- **PR wins** `field:boltArm`; PR `private field ArmIKSolver boltArm` at `runtime/Original/Weapons/ChangeMagazine.cs:43`; develop `private field ArmIKSolver boltArm` at `runtime/Original/Weapons/ChangeMagazine.cs:43`.
- **PR wins** `field:hasMoreAmmunition`; PR `private field bool hasMoreAmmunition` at `runtime/Original/Weapons/ChangeMagazine.cs:59`; develop `private field bool hasMoreAmmunition` at `runtime/Original/Weapons/ChangeMagazine.cs:59`.
- **PR wins** `field:input`; PR `private field InputHandler input` at `runtime/Original/Weapons/ChangeMagazine.cs:47`; develop `private field InputHandler input` at `runtime/Original/Weapons/ChangeMagazine.cs:47`.
- **PR wins** `field:isPlayer`; PR `public field bool isPlayer` at `runtime/Original/Weapons/ChangeMagazine.cs:33`; develop `public field bool isPlayer` at `runtime/Original/Weapons/ChangeMagazine.cs:33`.
- **PR wins** `field:magazineArm`; PR `private field ArmIKSolver magazineArm` at `runtime/Original/Weapons/ChangeMagazine.cs:41`; develop `private field ArmIKSolver magazineArm` at `runtime/Original/Weapons/ChangeMagazine.cs:41`.
- **PR wins** `field:openBoltStarted`; PR `private field bool openBoltStarted` at `runtime/Original/Weapons/ChangeMagazine.cs:61`; develop `private field bool openBoltStarted` at `runtime/Original/Weapons/ChangeMagazine.cs:61`.
- **PR wins** `field:quick`; PR `public field bool quick` at `runtime/Original/Weapons/ChangeMagazine.cs:31`; develop `public field bool quick` at `runtime/Original/Weapons/ChangeMagazine.cs:31`.
- **PR wins** `field:self`; PR `private field CharacterBody self` at `runtime/Original/Weapons/ChangeMagazine.cs:35`; develop `private field CharacterBody self` at `runtime/Original/Weapons/ChangeMagazine.cs:35`.
- **PR wins** `field:skillDelay`; PR `private field float skillDelay` at `runtime/Original/Weapons/ChangeMagazine.cs:53`; develop `private field float skillDelay` at `runtime/Original/Weapons/ChangeMagazine.cs:53`.
- **PR wins** `field:state`; PR `private field State state` at `runtime/Original/Weapons/ChangeMagazine.cs:45`; develop `private field State state` at `runtime/Original/Weapons/ChangeMagazine.cs:45`.
- **PR wins** `field:timestamp`; PR `private field float timestamp` at `runtime/Original/Weapons/ChangeMagazine.cs:49`; develop `private field float timestamp` at `runtime/Original/Weapons/ChangeMagazine.cs:49`.
- **PR wins** `field:usingRightArm`; PR `private field bool usingRightArm` at `runtime/Original/Weapons/ChangeMagazine.cs:57`; develop `private field bool usingRightArm` at `runtime/Original/Weapons/ChangeMagazine.cs:57`.
- **PR wins** `field:waitTimestamp`; PR `private field float waitTimestamp` at `runtime/Original/Weapons/ChangeMagazine.cs:51`; develop `private field float waitTimestamp` at `runtime/Original/Weapons/ChangeMagazine.cs:51`.
- **PR wins** `field:weapon`; PR `private field WeaponBase weapon` at `runtime/Original/Weapons/ChangeMagazine.cs:37`; develop `private field WeaponBase weapon` at `runtime/Original/Weapons/ChangeMagazine.cs:37`.
- **PR wins** ``method:OnAwake`0()``; PR `public override void OnAwake()` at `runtime/Original/Weapons/ChangeMagazine.cs:63`; develop `public override void OnAwake()` at `runtime/Original/Weapons/ChangeMagazine.cs:63`.
- **PR wins** ``method:OnEnd`0()``; PR `public override void OnEnd()` at `runtime/Original/Weapons/ChangeMagazine.cs:88`; develop `public override void OnEnd()` at `runtime/Original/Weapons/ChangeMagazine.cs:88`.
- **PR wins** ``method:OnStart`0()``; PR `public override void OnStart()` at `runtime/Original/Weapons/ChangeMagazine.cs:70`; develop `public override void OnStart()` at `runtime/Original/Weapons/ChangeMagazine.cs:70`.
- **PR wins** ``method:OnUpdate`0()``; PR `public override TaskStatus OnUpdate()` at `runtime/Original/Weapons/ChangeMagazine.cs:99`; develop `public override TaskStatus OnUpdate()` at `runtime/Original/Weapons/ChangeMagazine.cs:99`.


### 60. `Crisis.Runtime.Weapons.ChangeMagazine+State` — PR type wins

**PR declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `State`; partial `false`; `runtime/Original/Weapons/ChangeMagazine.cs:12`

**develop declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `State`; partial `false`; `runtime/Original/Weapons/ChangeMagazine.cs:12`

Common member signatures: **0**.


### 61. `Crisis.Runtime.Weapons.CharacterBody` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `CharacterBody`; partial `true`; `runtime/Engine/WeaponHost.cs:38`
- class; namespace `Crisis.Runtime.Weapons`; `CharacterBody`; partial `true`; `runtime/Original/Weapons/CharacterTrigger.Methods.cs:7`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `CharacterBody`; partial `true`; `runtime/Engine/WeaponHost.cs:38`
- class; namespace `Crisis.Runtime.Weapons`; `CharacterBody`; partial `true`; `runtime/Original/Weapons/CharacterAnimationState.cs:8`
- class; namespace `Crisis.Runtime.Weapons`; `CharacterBody`; partial `true`; `runtime/Original/Weapons/CharacterFacing.cs:5`
- class; namespace `Crisis.Runtime.Weapons`; `CharacterBody`; partial `true`; `runtime/Original/Weapons/CharacterLimbRagdollState.cs:98`
- class; namespace `Crisis.Runtime.Weapons`; `CharacterBody`; partial `true`; `runtime/Original/Weapons/CharacterMovement.cs:9`
- class; namespace `Crisis.Runtime.Weapons`; `CharacterBody`; partial `true`; `runtime/Original/Weapons/CharacterTrigger.Methods.cs:7`
- class; namespace `Crisis.Runtime.Weapons`; `CharacterBody`; partial `true`; `runtime/Original/Weapons/OriginalCharacterVitalHost.cs:9`

Common member signatures: **29**.
- **PR wins** `field:ActiveWeapon`; PR `public field WeaponBase ActiveWeapon` at `runtime/Engine/WeaponHost.cs:40`; develop `public field WeaponBase ActiveWeapon` at `runtime/Engine/WeaponHost.cs:40`.
- **PR wins** `field:Blackboard`; PR `public field Blackboard Blackboard` at `runtime/Engine/WeaponHost.cs:40`; develop `public field Blackboard Blackboard` at `runtime/Engine/WeaponHost.cs:40`.
- **PR wins** `field:Direction`; PR `public field float Direction` at `runtime/Engine/WeaponHost.cs:43`; develop `public field float Direction` at `runtime/Engine/WeaponHost.cs:43`.
- **PR wins** `field:Equipment`; PR `public field Equipment Equipment` at `runtime/Engine/WeaponHost.cs:40`; develop `public field Equipment Equipment` at `runtime/Engine/WeaponHost.cs:40`.
- **PR wins** `field:IsManipulatingEquipment`; PR `public field bool IsManipulatingEquipment` at `runtime/Engine/WeaponHost.cs:43`; develop `public field bool IsManipulatingEquipment` at `runtime/Engine/WeaponHost.cs:43`.
- **PR wins** `field:IsPlayer`; PR `public field bool IsPlayer` at `runtime/Engine/WeaponHost.cs:43`; develop `public field bool IsPlayer` at `runtime/Engine/WeaponHost.cs:43`.
- **PR wins** `field:IsUsingPrecision`; PR `public field bool IsUsingPrecision` at `runtime/Engine/WeaponHost.cs:43`; develop `public field bool IsUsingPrecision` at `runtime/Engine/WeaponHost.cs:43`.
- **PR wins** `field:Kinematic`; PR `public field bool Kinematic` at `runtime/Engine/WeaponHost.cs:48`; develop `public field bool Kinematic` at `runtime/Engine/WeaponHost.cs:48`.
- **PR wins** `field:LastFiredTimestamp`; PR `public field float LastFiredTimestamp` at `runtime/Engine/WeaponHost.cs:43`; develop `public field float LastFiredTimestamp` at `runtime/Engine/WeaponHost.cs:43`.
- **PR wins** `field:Pelvis`; PR `public field Limb Pelvis` at `runtime/Engine/WeaponHost.cs:41`; develop `public field Limb Pelvis` at `runtime/Engine/WeaponHost.cs:41`.
- **PR wins** `field:PendingAction`; PR `public field CharacterAction PendingAction` at `runtime/Engine/WeaponHost.cs:45`; develop `public field CharacterAction PendingAction` at `runtime/Engine/WeaponHost.cs:45`.
- **PR wins** `field:PendingGrenadeType`; PR `public field GrenadeReference.Type PendingGrenadeType` at `runtime/Engine/WeaponHost.cs:49`; develop `public field GrenadeReference.Type PendingGrenadeType` at `runtime/Engine/WeaponHost.cs:49`.
- **PR wins** `field:ShotsFired`; PR `public field int ShotsFired` at `runtime/Engine/WeaponHost.cs:43`; develop `public field int ShotsFired` at `runtime/Engine/WeaponHost.cs:43`.
- **PR wins** `field:Torso`; PR `public field Limb Torso` at `runtime/Engine/WeaponHost.cs:41`; develop `public field Limb Torso` at `runtime/Engine/WeaponHost.cs:41`.
- **PR wins** `field:chestRigIKTarget`; PR `public field Transform chestRigIKTarget` at `runtime/Engine/WeaponHost.cs:42`; develop `public field Transform chestRigIKTarget` at `runtime/Engine/WeaponHost.cs:42`.
- **PR wins** `field:fragGrenadeAnchor`; PR `public field Transform fragGrenadeAnchor` at `runtime/Engine/WeaponHost.cs:50`; develop `public field Transform fragGrenadeAnchor` at `runtime/Engine/WeaponHost.cs:50`.
- **PR wins** `field:leftArm`; PR `public field ArmIKSolver leftArm` at `runtime/Engine/WeaponHost.cs:41`; develop `public field ArmIKSolver leftArm` at `runtime/Engine/WeaponHost.cs:41`.
- **PR wins** `field:pistolIKTarget`; PR `public field Transform pistolIKTarget` at `runtime/Engine/WeaponHost.cs:42`; develop `public field Transform pistolIKTarget` at `runtime/Engine/WeaponHost.cs:42`.
- **PR wins** `field:pistolPivot`; PR `public field Transform pistolPivot` at `runtime/Engine/WeaponHost.cs:42`; develop `public field Transform pistolPivot` at `runtime/Engine/WeaponHost.cs:42`.
- **PR wins** `field:rightArm`; PR `public field ArmIKSolver rightArm` at `runtime/Engine/WeaponHost.cs:41`; develop `public field ArmIKSolver rightArm` at `runtime/Engine/WeaponHost.cs:41`.
- **PR wins** `field:stunGrenadeAnchor`; PR `public field Transform stunGrenadeAnchor` at `runtime/Engine/WeaponHost.cs:50`; develop `public field Transform stunGrenadeAnchor` at `runtime/Engine/WeaponHost.cs:50`.
- **PR wins** `field:triggerPulled`; PR `field bool triggerPulled` at `runtime/Engine/WeaponHost.cs:45`; develop `field bool triggerPulled` at `runtime/Engine/WeaponHost.cs:45`.
- **PR wins** ``method:InsertSingleRound`0()``; PR `public bool InsertSingleRound()` at `runtime/Engine/WeaponHost.cs:51`; develop `public bool InsertSingleRound()` at `runtime/Engine/WeaponHost.cs:51`.
- **PR wins** ``method:MoveMagazineToHand`0(bool)``; PR `public bool MoveMagazineToHand(bool)` at `runtime/Original/Weapons/CharacterTrigger.Methods.cs:21`; develop `public bool MoveMagazineToHand(bool)` at `runtime/Original/Weapons/CharacterTrigger.Methods.cs:21`.
- **PR wins** ``method:PullTrigger`0()``; PR `public void PullTrigger()` at `runtime/Original/Weapons/CharacterTrigger.Methods.cs:14`; develop `public void PullTrigger()` at `runtime/Original/Weapons/CharacterTrigger.Methods.cs:14`.
- **PR wins** ``method:QueryTrigger`0()``; PR `public bool QueryTrigger()` at `runtime/Original/Weapons/CharacterTrigger.Methods.cs:8`; develop `public bool QueryTrigger()` at `runtime/Original/Weapons/CharacterTrigger.Methods.cs:8`.
- **PR wins** `property:AudioInstigator`; PR `public object AudioInstigator` at `runtime/Engine/WeaponHost.cs:44`; develop `public object AudioInstigator` at `runtime/Engine/WeaponHost.cs:44`.
- **PR wins** `property:Position`; PR `public float Position` at `runtime/Engine/WeaponHost.cs:44`; develop `public float Position` at `runtime/Engine/WeaponHost.cs:44`.
- **PR wins** `property:RotationY`; PR `public float RotationY` at `runtime/Engine/WeaponHost.cs:43`; develop `public float RotationY` at `runtime/Engine/WeaponHost.cs:43`.

Develop-only member signatures: **41** — **DEVELOP-ONLY / DEFERRED**.
- **DEVELOP-ONLY — DEFERRED** `field:CrippledRatio` — `public field float CrippledRatio` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:103`.
- **DEVELOP-ONLY — DEFERRED** `field:EventDeath` — `public field UnityEvent EventDeath` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:107`.
- **DEVELOP-ONLY — DEFERRED** `field:EventHit` — `public field UnityEvent EventHit` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:104`.
- **DEVELOP-ONLY — DEFERRED** `field:EventRagdoll` — `public field UnityEvent EventRagdoll` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:105`.
- **DEVELOP-ONLY — DEFERRED** `field:EventUnconscious` — `public field UnityEvent EventUnconscious` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:106`.
- **DEVELOP-ONLY — DEFERRED** `field:FloatingBodies` — `public field bool FloatingBodies` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:101`.
- **DEVELOP-ONLY — DEFERRED** `field:NoPlayerLimp` — `public field bool NoPlayerLimp` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:102`.
- **DEVELOP-ONLY — DEFERRED** `field:PinnedBodies` — `public field bool PinnedBodies` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:100`.
- **DEVELOP-ONLY — DEFERRED** `field:acceleration` — `private field float acceleration` at `runtime/Original/Weapons/CharacterMovement.cs:10`.
- **DEVELOP-ONLY — DEFERRED** `field:accelerationMagnitude` — `private field float accelerationMagnitude` at `runtime/Original/Weapons/CharacterMovement.cs:11`.
- **DEVELOP-ONLY — DEFERRED** `field:hashBackwards` — `readonly field int hashBackwards` at `runtime/Original/Weapons/CharacterAnimationState.cs:12`.
- **DEVELOP-ONLY — DEFERRED** `field:hashCrippledRatio` — `readonly field int hashCrippledRatio` at `runtime/Original/Weapons/CharacterAnimationState.cs:15`.
- **DEVELOP-ONLY — DEFERRED** `field:hashKneelRatio` — `readonly field int hashKneelRatio` at `runtime/Original/Weapons/CharacterAnimationState.cs:14`.
- **DEVELOP-ONLY — DEFERRED** `field:hashMoveSpeed` — `readonly field int hashMoveSpeed` at `runtime/Original/Weapons/CharacterAnimationState.cs:13`.
- **DEVELOP-ONLY — DEFERRED** `field:hashMoving` — `readonly field int hashMoving` at `runtime/Original/Weapons/CharacterAnimationState.cs:11`.
- **DEVELOP-ONLY — DEFERRED** `field:limbs` — `public readonly field List<Limb> limbs` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:99`.
- **DEVELOP-ONLY — DEFERRED** `field:sourceAnimator` — `field Animator sourceAnimator` at `runtime/Original/Weapons/CharacterAnimationState.cs:9`.
- **DEVELOP-ONLY — DEFERRED** `field:sourceBodyCollider` — `field CapsuleCollider sourceBodyCollider` at `runtime/Original/Weapons/CharacterAnimationState.cs:10`.
- **DEVELOP-ONLY — DEFERRED** ``method:BodyFacingMe`0(CharacterBody)`` — `public bool BodyFacingMe(CharacterBody)` at `runtime/Original/Weapons/CharacterFacing.cs:28`.
- **DEVELOP-ONLY — DEFERRED** ``method:EnterSourceRagdoll`0()`` — `public void EnterSourceRagdoll()` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:126`.
- **DEVELOP-ONLY — DEFERRED** ``method:ExecuteSourceMovement`0(float, float, float, float, float, bool, bool, bool, bool, bool)`` — `public object ExecuteSourceMovement(float, float, float, float, float, bool, bool, bool, bool, bool)` at `runtime/Original/Weapons/CharacterMovement.cs:31`.
- **DEVELOP-ONLY — DEFERRED** ``method:ExecuteSourceUpdateAnimation`0(bool, bool, bool, float, float, float, float, bool, float, float)`` — `public object ExecuteSourceUpdateAnimation(bool, bool, bool, float, float, float, float, bool, float, float)` at `runtime/Original/Weapons/CharacterAnimationState.cs:23`.
- **DEVELOP-ONLY — DEFERRED** ``method:FacingDirection`0(float)`` — `public bool FacingDirection(float)` at `runtime/Original/Weapons/CharacterFacing.cs:11`.
- **DEVELOP-ONLY — DEFERRED** ``method:Facing`0(CharacterBody)`` — `public bool Facing(CharacterBody)` at `runtime/Original/Weapons/CharacterFacing.cs:19`.
- **DEVELOP-ONLY — DEFERRED** ``method:Facing`0(float)`` — `public bool Facing(float)` at `runtime/Original/Weapons/CharacterFacing.cs:15`.
- **DEVELOP-ONLY — DEFERRED** ``method:InitializeOriginalPhysiology`0(float, float?, float?, float?)`` — `public void InitializeOriginalPhysiology(float, float?, float?, float?)` at `runtime/Original/Weapons/OriginalCharacterVitalHost.cs:11`.
- **DEVELOP-ONLY — DEFERRED** ``method:InitializeSourceAnimation`0(Animator, CapsuleCollider)`` — `public void InitializeSourceAnimation(Animator, CapsuleCollider)` at `runtime/Original/Weapons/CharacterAnimationState.cs:19`.
- **DEVELOP-ONLY — DEFERRED** ``method:KinematicDistanceToPosition`0(float)`` — `public float KinematicDistanceToPosition(float)` at `runtime/Original/Weapons/CharacterFacing.cs:7`.
- **DEVELOP-ONLY — DEFERRED** ``method:Move`0(float, float)`` — `public void Move(float, float)` at `runtime/Original/Weapons/CharacterMovement.cs:14`.
- **DEVELOP-ONLY — DEFERRED** ``method:OnOriginalJointBroken`0(Limb)`` — `public void OnOriginalJointBroken(Limb)` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:117`.
- **DEVELOP-ONLY — DEFERRED** ``method:OnOriginalOrganUnconscious`0()`` — `public void OnOriginalOrganUnconscious()` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:109`.
- **DEVELOP-ONLY — DEFERRED** ``method:ResetAcceleration`0()`` — `private void ResetAcceleration()` at `runtime/Original/Weapons/CharacterMovement.cs:23`.
- **DEVELOP-ONLY — DEFERRED** ``method:SetFloating`0(bool)`` — `public void SetFloating(bool)` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:144`.
- **DEVELOP-ONLY — DEFERRED** ``method:SetKinematic`0(bool)`` — `public void SetKinematic(bool)` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:137`.
- **DEVELOP-ONLY — DEFERRED** ``method:StepOriginalPhysiology`0(float, float, float, float, bool, float, bool, bool, float)`` — `public object StepOriginalPhysiology(float, float, float, float, bool, float, bool, bool, float)` at `runtime/Original/Weapons/OriginalCharacterVitalHost.cs:18`.
- **DEVELOP-ONLY — DEFERRED** `property:FacingRight` — `public bool FacingRight` at `runtime/Engine/WeaponHost.cs:44`.
- **DEVELOP-ONLY — DEFERRED** `property:KinematicPosition` — `public float KinematicPosition` at `runtime/Original/Weapons/CharacterFacing.cs:6`.
- **DEVELOP-ONLY — DEFERRED** `property:KneelRatio` — `public float KneelRatio` at `runtime/Original/Weapons/CharacterAnimationState.cs:17`.
- **DEVELOP-ONLY — DEFERRED** `property:RagdollEntered` — `public bool RagdollEntered` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:108`.
- **DEVELOP-ONLY — DEFERRED** `property:SourcePhysiology` — `public OriginalPhysiologyConditions SourcePhysiology` at `runtime/Original/Weapons/OriginalCharacterVitalHost.cs:10`.
- **DEVELOP-ONLY — DEFERRED** `property:TargetKneelRatio` — `public float TargetKneelRatio` at `runtime/Original/Weapons/CharacterAnimationState.cs:18`.


### 62. `Crisis.Runtime.Weapons.CharacterBody+CharacterAction` — PR type wins

**PR declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `CharacterAction`; partial `false`; `runtime/Engine/WeaponHost.cs:39`

**develop declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `CharacterAction`; partial `false`; `runtime/Engine/WeaponHost.cs:39`

Common member signatures: **0**.


### 63. `Crisis.Runtime.Weapons.ContextView` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `ContextView`; partial `false`; `runtime/Engine/WeaponHost.cs:60`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `ContextView`; partial `false`; `runtime/Engine/WeaponHost.cs:60`

Common member signatures: **2**.
- **PR wins** `field:InputHandler`; PR `public field InputHandler InputHandler` at `runtime/Engine/WeaponHost.cs:60`; develop `public field InputHandler InputHandler` at `runtime/Engine/WeaponHost.cs:60`.
- **PR wins** `field:Settings`; PR `public field SettingsView Settings` at `runtime/Engine/WeaponHost.cs:60`; develop `public field SettingsView Settings` at `runtime/Engine/WeaponHost.cs:60`.


### 64. `Crisis.Runtime.Weapons.CyclingBolt` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `CyclingBolt`; partial `false`; `runtime/Original/Weapons/CyclingBolt.cs:9`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `CyclingBolt`; partial `false`; `runtime/Original/Weapons/CyclingBolt.cs:9`

Common member signatures: **60**.
- **PR wins** `field:audioBoltExtract`; PR `private field PropagatingAudioSample audioBoltExtract` at `runtime/Original/Weapons/CyclingBolt.cs:76`; develop `private field PropagatingAudioSample audioBoltExtract` at `runtime/Original/Weapons/CyclingBolt.cs:76`.
- **PR wins** `field:audioBoltReturn`; PR `private field PropagatingAudioSample audioBoltReturn` at `runtime/Original/Weapons/CyclingBolt.cs:74`; develop `private field PropagatingAudioSample audioBoltReturn` at `runtime/Original/Weapons/CyclingBolt.cs:74`.
- **PR wins** `field:calculatedEjectPosition`; PR `private field float calculatedEjectPosition` at `runtime/Original/Weapons/CyclingBolt.cs:58`; develop `private field float calculatedEjectPosition` at `runtime/Original/Weapons/CyclingBolt.cs:58`.
- **PR wins** `field:calculatedFeedPosition`; PR `private field float calculatedFeedPosition` at `runtime/Original/Weapons/CyclingBolt.cs:60`; develop `private field float calculatedFeedPosition` at `runtime/Original/Weapons/CyclingBolt.cs:60`.
- **PR wins** `field:chamber`; PR `public field Transform chamber` at `runtime/Original/Weapons/CyclingBolt.cs:19`; develop `public field Transform chamber` at `runtime/Original/Weapons/CyclingBolt.cs:19`.
- **PR wins** `field:chargingHandle`; PR `public field Transform chargingHandle` at `runtime/Original/Weapons/CyclingBolt.cs:33`; develop `public field Transform chargingHandle` at `runtime/Original/Weapons/CyclingBolt.cs:33`.
- **PR wins** `field:chargingHandleClosedPosition`; PR `private field Vector3 chargingHandleClosedPosition` at `runtime/Original/Weapons/CyclingBolt.cs:70`; develop `private field Vector3 chargingHandleClosedPosition` at `runtime/Original/Weapons/CyclingBolt.cs:70`.
- **PR wins** `field:closedKeyTime`; PR `private field float closedKeyTime` at `runtime/Original/Weapons/CyclingBolt.cs:64`; develop `private field float closedKeyTime` at `runtime/Original/Weapons/CyclingBolt.cs:64`.
- **PR wins** `field:closedPosition`; PR `private field Vector3 closedPosition` at `runtime/Original/Weapons/CyclingBolt.cs:68`; develop `private field Vector3 closedPosition` at `runtime/Original/Weapons/CyclingBolt.cs:68`.
- **PR wins** `field:cycle`; PR `public field AnimationCurve cycle` at `runtime/Original/Weapons/CyclingBolt.cs:27`; develop `public field AnimationCurve cycle` at `runtime/Original/Weapons/CyclingBolt.cs:27`.
- **PR wins** `field:cycleTimestamp`; PR `private field float cycleTimestamp` at `runtime/Original/Weapons/CyclingBolt.cs:54`; develop `private field float cycleTimestamp` at `runtime/Original/Weapons/CyclingBolt.cs:54`.
- **PR wins** `field:ejectPosition`; PR `public field float ejectPosition` at `runtime/Original/Weapons/CyclingBolt.cs:23`; develop `public field float ejectPosition` at `runtime/Original/Weapons/CyclingBolt.cs:23`.
- **PR wins** `field:ejectionForce`; PR `public field Vector3 ejectionForce` at `runtime/Original/Weapons/CyclingBolt.cs:38`; develop `public field Vector3 ejectionForce` at `runtime/Original/Weapons/CyclingBolt.cs:38`.
- **PR wins** `field:ejectionTorque`; PR `public field Vector3 ejectionTorque` at `runtime/Original/Weapons/CyclingBolt.cs:42`; develop `public field Vector3 ejectionTorque` at `runtime/Original/Weapons/CyclingBolt.cs:42`.
- **PR wins** `field:emitCasings`; PR `public field bool emitCasings` at `runtime/Original/Weapons/CyclingBolt.cs:36`; develop `public field bool emitCasings` at `runtime/Original/Weapons/CyclingBolt.cs:36`.
- **PR wins** `field:feedMalfunctionPosition`; PR `private field float feedMalfunctionPosition` at `runtime/Original/Weapons/CyclingBolt.cs:52`; develop `private field float feedMalfunctionPosition` at `runtime/Original/Weapons/CyclingBolt.cs:52`.
- **PR wins** `field:feedPosition`; PR `public field float feedPosition` at `runtime/Original/Weapons/CyclingBolt.cs:25`; develop `public field float feedPosition` at `runtime/Original/Weapons/CyclingBolt.cs:25`.
- **PR wins** `field:forceRandomness`; PR `public field Vector3 forceRandomness` at `runtime/Original/Weapons/CyclingBolt.cs:40`; develop `public field Vector3 forceRandomness` at `runtime/Original/Weapons/CyclingBolt.cs:40`.
- **PR wins** `field:hasFed`; PR `private field bool hasFed` at `runtime/Original/Weapons/CyclingBolt.cs:50`; develop `private field bool hasFed` at `runtime/Original/Weapons/CyclingBolt.cs:50`.
- **PR wins** `field:isCarrying`; PR `private field bool isCarrying` at `runtime/Original/Weapons/CyclingBolt.cs:48`; develop `private field bool isCarrying` at `runtime/Original/Weapons/CyclingBolt.cs:48`.
- **PR wins** `field:lockOpen`; PR `public field bool lockOpen` at `runtime/Original/Weapons/CyclingBolt.cs:29`; develop `public field bool lockOpen` at `runtime/Original/Weapons/CyclingBolt.cs:29`.
- **PR wins** `field:malfunctionRate`; PR `private field float malfunctionRate` at `runtime/Original/Weapons/CyclingBolt.cs:66`; develop `private field float malfunctionRate` at `runtime/Original/Weapons/CyclingBolt.cs:66`.
- **PR wins** `field:manualChargingCycle`; PR `private field bool manualChargingCycle` at `runtime/Original/Weapons/CyclingBolt.cs:46`; develop `private field bool manualChargingCycle` at `runtime/Original/Weapons/CyclingBolt.cs:46`.
- **PR wins** `field:manualCycleTimestamp`; PR `private field float manualCycleTimestamp` at `runtime/Original/Weapons/CyclingBolt.cs:56`; develop `private field float manualCycleTimestamp` at `runtime/Original/Weapons/CyclingBolt.cs:56`.
- **PR wins** `field:manualLockOpen`; PR `public field bool manualLockOpen` at `runtime/Original/Weapons/CyclingBolt.cs:31`; develop `public field bool manualLockOpen` at `runtime/Original/Weapons/CyclingBolt.cs:31`.
- **PR wins** `field:onClosed`; PR `private field UnityEventBool onClosed` at `runtime/Original/Weapons/CyclingBolt.cs:80`; develop `private field UnityEventBool onClosed` at `runtime/Original/Weapons/CyclingBolt.cs:80`.
- **PR wins** `field:onOpened`; PR `private field UnityEvent onOpened` at `runtime/Original/Weapons/CyclingBolt.cs:78`; develop `private field UnityEvent onOpened` at `runtime/Original/Weapons/CyclingBolt.cs:78`.
- **PR wins** `field:openKeyTime`; PR `private field float openKeyTime` at `runtime/Original/Weapons/CyclingBolt.cs:62`; develop `private field float openKeyTime` at `runtime/Original/Weapons/CyclingBolt.cs:62`.
- **PR wins** `field:torqueRandomness`; PR `public field Vector3 torqueRandomness` at `runtime/Original/Weapons/CyclingBolt.cs:44`; develop `public field Vector3 torqueRandomness` at `runtime/Original/Weapons/CyclingBolt.cs:44`.
- **PR wins** `field:travelRange`; PR `public field float travelRange` at `runtime/Original/Weapons/CyclingBolt.cs:21`; develop `public field float travelRange` at `runtime/Original/Weapons/CyclingBolt.cs:21`.
- **PR wins** `field:weapon`; PR `private field WeaponBase weapon` at `runtime/Original/Weapons/CyclingBolt.cs:72`; develop `private field WeaponBase weapon` at `runtime/Original/Weapons/CyclingBolt.cs:72`.
- **PR wins** ``method:Awake`0()``; PR `private void Awake()` at `runtime/Original/Weapons/CyclingBolt.cs:170`; develop `private void Awake()` at `runtime/Original/Weapons/CyclingBolt.cs:170`.
- **PR wins** ``method:Close`0(bool)``; PR `public void Close(bool)` at `runtime/Original/Weapons/CyclingBolt.cs:282`; develop `public void Close(bool)` at `runtime/Original/Weapons/CyclingBolt.cs:282`.
- **PR wins** ``method:DetonateRound`0()``; PR `public void DetonateRound()` at `runtime/Original/Weapons/CyclingBolt.cs:351`; develop `public void DetonateRound()` at `runtime/Original/Weapons/CyclingBolt.cs:351`.
- **PR wins** ``method:EjectCasing`0(float, int)``; PR `public void EjectCasing(float, int)` at `runtime/Original/Weapons/CyclingBolt.cs:363`; develop `public void EjectCasing(float, int)` at `runtime/Original/Weapons/CyclingBolt.cs:363`.
- **PR wins** ``method:FeedRoundIntoOpen`0()``; PR `public void FeedRoundIntoOpen()` at `runtime/Original/Weapons/CyclingBolt.cs:383`; develop `public void FeedRoundIntoOpen()` at `runtime/Original/Weapons/CyclingBolt.cs:383`.
- **PR wins** ``method:FeedRound`0(bool, bool)``; PR `public void FeedRound(bool, bool)` at `runtime/Original/Weapons/CyclingBolt.cs:368`; develop `public void FeedRound(bool, bool)` at `runtime/Original/Weapons/CyclingBolt.cs:368`.
- **PR wins** ``method:Initialize`0(WeaponBase, UnityAction, UnityAction<bool>, float)``; PR `public void Initialize(WeaponBase, UnityAction, UnityAction<bool>, float)` at `runtime/Original/Weapons/CyclingBolt.cs:182`; develop `public void Initialize(WeaponBase, UnityAction, UnityAction<bool>, float)` at `runtime/Original/Weapons/CyclingBolt.cs:182`.
- **PR wins** ``method:LockOpenOnEmptyMagazine`0()``; PR `public void LockOpenOnEmptyMagazine()` at `runtime/Original/Weapons/CyclingBolt.cs:298`; develop `public void LockOpenOnEmptyMagazine()` at `runtime/Original/Weapons/CyclingBolt.cs:298`.
- **PR wins** ``method:OnClosed`0()``; PR `private void OnClosed()` at `runtime/Original/Weapons/CyclingBolt.cs:336`; develop `private void OnClosed()` at `runtime/Original/Weapons/CyclingBolt.cs:336`.
- **PR wins** ``method:OnOpened`0()``; PR `private void OnOpened()` at `runtime/Original/Weapons/CyclingBolt.cs:306`; develop `private void OnOpened()` at `runtime/Original/Weapons/CyclingBolt.cs:306`.
- **PR wins** ``method:Open`0(bool)``; PR `public void Open(bool)` at `runtime/Original/Weapons/CyclingBolt.cs:265`; develop `public void Open(bool)` at `runtime/Original/Weapons/CyclingBolt.cs:265`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `runtime/Original/Weapons/CyclingBolt.cs:196`; develop `private void Update()` at `runtime/Original/Weapons/CyclingBolt.cs:196`.
- **PR wins** `property:BoltClosingPeriod`; PR `public float BoltClosingPeriod` at `runtime/Original/Weapons/CyclingBolt.cs:88`; develop `public float BoltClosingPeriod` at `runtime/Original/Weapons/CyclingBolt.cs:88`.
- **PR wins** `property:BoltOpeningPeriod`; PR `public float BoltOpeningPeriod` at `runtime/Original/Weapons/CyclingBolt.cs:86`; develop `public float BoltOpeningPeriod` at `runtime/Original/Weapons/CyclingBolt.cs:86`.
- **PR wins** `property:CarryingLiveRound`; PR `public bool CarryingLiveRound` at `runtime/Original/Weapons/CyclingBolt.cs:158`; develop `public bool CarryingLiveRound` at `runtime/Original/Weapons/CyclingBolt.cs:158`.
- **PR wins** `property:CyclePeriod`; PR `public float CyclePeriod` at `runtime/Original/Weapons/CyclingBolt.cs:84`; develop `public float CyclePeriod` at `runtime/Original/Weapons/CyclingBolt.cs:84`.
- **PR wins** `property:IsClosed`; PR `public bool IsClosed` at `runtime/Original/Weapons/CyclingBolt.cs:124`; develop `public bool IsClosed` at `runtime/Original/Weapons/CyclingBolt.cs:124`.
- **PR wins** `property:IsClosing`; PR `public bool IsClosing` at `runtime/Original/Weapons/CyclingBolt.cs:108`; develop `public bool IsClosing` at `runtime/Original/Weapons/CyclingBolt.cs:108`.
- **PR wins** `property:IsMoving`; PR `public bool IsMoving` at `runtime/Original/Weapons/CyclingBolt.cs:98`; develop `public bool IsMoving` at `runtime/Original/Weapons/CyclingBolt.cs:98`.
- **PR wins** `property:IsOpen`; PR `public bool IsOpen` at `runtime/Original/Weapons/CyclingBolt.cs:116`; develop `public bool IsOpen` at `runtime/Original/Weapons/CyclingBolt.cs:116`.
- **PR wins** `property:IsOpening`; PR `public bool IsOpening` at `runtime/Original/Weapons/CyclingBolt.cs:100`; develop `public bool IsOpening` at `runtime/Original/Weapons/CyclingBolt.cs:100`.
- **PR wins** `property:Live`; PR `public bool Live` at `runtime/Original/Weapons/CyclingBolt.cs:142`; develop `public bool Live` at `runtime/Original/Weapons/CyclingBolt.cs:142`.
- **PR wins** `property:LockEngaged`; PR `public bool LockEngaged` at `runtime/Original/Weapons/CyclingBolt.cs:90`; develop `public bool LockEngaged` at `runtime/Original/Weapons/CyclingBolt.cs:90`.
- **PR wins** `property:Malfunctioned`; PR `public bool Malfunctioned` at `runtime/Original/Weapons/CyclingBolt.cs:134`; develop `public bool Malfunctioned` at `runtime/Original/Weapons/CyclingBolt.cs:134`.
- **PR wins** `property:ManualCycle`; PR `public bool ManualCycle` at `runtime/Original/Weapons/CyclingBolt.cs:132`; develop `public bool ManualCycle` at `runtime/Original/Weapons/CyclingBolt.cs:132`.
- **PR wins** `property:Round`; PR `public Casing Round` at `runtime/Original/Weapons/CyclingBolt.cs:168`; develop `public Casing Round` at `runtime/Original/Weapons/CyclingBolt.cs:168`.
- **PR wins** `property:StaticAndNotLive`; PR `public bool StaticAndNotLive` at `runtime/Original/Weapons/CyclingBolt.cs:150`; develop `public bool StaticAndNotLive` at `runtime/Original/Weapons/CyclingBolt.cs:150`.
- **PR wins** `property:Status`; PR `public State Status` at `runtime/Original/Weapons/CyclingBolt.cs:82`; develop `public State Status` at `runtime/Original/Weapons/CyclingBolt.cs:82`.
- **PR wins** `property:UseMalfunctions`; PR `public bool UseMalfunctions` at `runtime/Original/Weapons/CyclingBolt.cs:166`; develop `public bool UseMalfunctions` at `runtime/Original/Weapons/CyclingBolt.cs:166`.


### 65. `Crisis.Runtime.Weapons.CyclingBolt+State` — PR type wins

**PR declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `State`; partial `false`; `runtime/Original/Weapons/CyclingBolt.cs:11`

**develop declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `State`; partial `false`; `runtime/Original/Weapons/CyclingBolt.cs:11`

Common member signatures: **0**.


### 66. `Crisis.Runtime.Weapons.Equipment` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Equipment`; partial `false`; `runtime/Original/Weapons/Equipment.cs:8`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Equipment`; partial `false`; `runtime/Original/Weapons/Equipment.cs:8`

Common member signatures: **21**.
- **PR wins** `field:fragGrenades`; PR `public field int fragGrenades` at `runtime/Original/Weapons/Equipment.cs:20`; develop `public field int fragGrenades` at `runtime/Original/Weapons/Equipment.cs:20`.
- **PR wins** `field:gauze`; PR `public field int gauze` at `runtime/Original/Weapons/Equipment.cs:26`; develop `public field int gauze` at `runtime/Original/Weapons/Equipment.cs:26`.
- **PR wins** `field:multiTool`; PR `public field bool multiTool` at `runtime/Original/Weapons/Equipment.cs:24`; develop `public field bool multiTool` at `runtime/Original/Weapons/Equipment.cs:24`.
- **PR wins** `field:plasticuffs`; PR `public field bool plasticuffs` at `runtime/Original/Weapons/Equipment.cs:22`; develop `public field bool plasticuffs` at `runtime/Original/Weapons/Equipment.cs:22`.
- **PR wins** `field:primaryMagazineCount`; PR `public field int primaryMagazineCount` at `runtime/Original/Weapons/Equipment.cs:14`; develop `public field int primaryMagazineCount` at `runtime/Original/Weapons/Equipment.cs:14`.
- **PR wins** `field:primaryWeapon`; PR `public field WeaponBase primaryWeapon` at `runtime/Original/Weapons/Equipment.cs:10`; develop `public field WeaponBase primaryWeapon` at `runtime/Original/Weapons/Equipment.cs:10`.
- **PR wins** `field:secondaryMagazineCount`; PR `public field int secondaryMagazineCount` at `runtime/Original/Weapons/Equipment.cs:16`; develop `public field int secondaryMagazineCount` at `runtime/Original/Weapons/Equipment.cs:16`.
- **PR wins** `field:secondaryWeapon`; PR `public field WeaponBase secondaryWeapon` at `runtime/Original/Weapons/Equipment.cs:12`; develop `public field WeaponBase secondaryWeapon` at `runtime/Original/Weapons/Equipment.cs:12`.
- **PR wins** `field:stunGrenades`; PR `public field int stunGrenades` at `runtime/Original/Weapons/Equipment.cs:18`; develop `public field int stunGrenades` at `runtime/Original/Weapons/Equipment.cs:18`.
- **PR wins** ``method:AddMagazineForWeapon`0(Magazine, WeaponBase)``; PR `public void AddMagazineForWeapon(Magazine, WeaponBase)` at `runtime/Original/Weapons/Equipment.cs:133`; develop `public void AddMagazineForWeapon(Magazine, WeaponBase)` at `runtime/Original/Weapons/Equipment.cs:133`.
- **PR wins** ``method:GetFullestMagazineForWeapon`0(WeaponBase)``; PR `public Magazine GetFullestMagazineForWeapon(WeaponBase)` at `runtime/Original/Weapons/Equipment.cs:111`; develop `public Magazine GetFullestMagazineForWeapon(WeaponBase)` at `runtime/Original/Weapons/Equipment.cs:111`.
- **PR wins** ``method:GetFullestMagazineFromStack`0(string, List<Magazine>)``; PR `private Magazine GetFullestMagazineFromStack(string, List<Magazine>)` at `runtime/Original/Weapons/Equipment.cs:118`; develop `private Magazine GetFullestMagazineFromStack(string, List<Magazine>)` at `runtime/Original/Weapons/Equipment.cs:118`.
- **PR wins** ``method:GetLooseAmmoCountForWeapon`0(WeaponBase)``; PR `public int GetLooseAmmoCountForWeapon(WeaponBase)` at `runtime/Original/Weapons/Equipment.cs:98`; develop `public int GetLooseAmmoCountForWeapon(WeaponBase)` at `runtime/Original/Weapons/Equipment.cs:98`.
- **PR wins** ``method:GetNonEmptyWeapon`0()``; PR `public WeaponBase GetNonEmptyWeapon()` at `runtime/Original/Weapons/Equipment.cs:85`; develop `public WeaponBase GetNonEmptyWeapon()` at `runtime/Original/Weapons/Equipment.cs:85`.
- **PR wins** ``method:HasAmmunitionForWeapon`0(WeaponBase)``; PR `public bool HasAmmunitionForWeapon(WeaponBase)` at `runtime/Original/Weapons/Equipment.cs:70`; develop `public bool HasAmmunitionForWeapon(WeaponBase)` at `runtime/Original/Weapons/Equipment.cs:70`.
- **PR wins** ``method:InitializeWeaponAmmunition`0(WeaponBase, int, List<Magazine>, AmmunitionStack)``; PR `private void InitializeWeaponAmmunition(WeaponBase, int, List<Magazine>, AmmunitionStack)` at `runtime/Original/Weapons/Equipment.cs:46`; develop `private void InitializeWeaponAmmunition(WeaponBase, int, List<Magazine>, AmmunitionStack)` at `runtime/Original/Weapons/Equipment.cs:46`.
- **PR wins** ``method:Initialize`0()``; PR `public void Initialize()` at `runtime/Original/Weapons/Equipment.cs:36`; develop `public void Initialize()` at `runtime/Original/Weapons/Equipment.cs:36`.
- **PR wins** `property:PrimaryMagazines`; PR `public List<Magazine> PrimaryMagazines` at `runtime/Original/Weapons/Equipment.cs:28`; develop `public List<Magazine> PrimaryMagazines` at `runtime/Original/Weapons/Equipment.cs:28`.
- **PR wins** `property:PrimaryRounds`; PR `public AmmunitionStack PrimaryRounds` at `runtime/Original/Weapons/Equipment.cs:32`; develop `public AmmunitionStack PrimaryRounds` at `runtime/Original/Weapons/Equipment.cs:32`.
- **PR wins** `property:SecondaryMagazines`; PR `public List<Magazine> SecondaryMagazines` at `runtime/Original/Weapons/Equipment.cs:30`; develop `public List<Magazine> SecondaryMagazines` at `runtime/Original/Weapons/Equipment.cs:30`.
- **PR wins** `property:SecondaryRounds`; PR `public AmmunitionStack SecondaryRounds` at `runtime/Original/Weapons/Equipment.cs:34`; develop `public AmmunitionStack SecondaryRounds` at `runtime/Original/Weapons/Equipment.cs:34`.


### 67. `Crisis.Runtime.Weapons.GeneralView` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `GeneralView`; partial `false`; `runtime/Engine/WeaponHost.cs:59`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `GeneralView`; partial `false`; `runtime/Engine/WeaponHost.cs:59`

Common member signatures: **1**.
- **PR wins** `field:reloadHelp`; PR `public field bool reloadHelp` at `runtime/Engine/WeaponHost.cs:59`; develop `public field bool reloadHelp` at `runtime/Engine/WeaponHost.cs:59`.


### 68. `Crisis.Runtime.Weapons.Global` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Global`; partial `false`; `runtime/Engine/WeaponHost.cs:61`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Global`; partial `false`; `runtime/Engine/WeaponHost.cs:61`

Common member signatures: **1**.
- **PR wins** `field:Context`; PR `public static field ContextView Context` at `runtime/Engine/WeaponHost.cs:61`; develop `public static field ContextView Context` at `runtime/Engine/WeaponHost.cs:61`.


### 69. `Crisis.Runtime.Weapons.GrenadeReference` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `GrenadeReference`; partial `false`; `runtime/Engine/OriginalArmPhysicsHost.cs:7`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `GrenadeReference`; partial `false`; `runtime/Engine/OriginalArmPhysicsHost.cs:8`

Common member signatures: **0**.


### 70. `Crisis.Runtime.Weapons.GrenadeReference+Type` — PR type wins

**PR declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `Type`; partial `false`; `runtime/Engine/OriginalArmPhysicsHost.cs:8`

**develop declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `Type`; partial `false`; `runtime/Engine/OriginalArmPhysicsHost.cs:9`

Common member signatures: **0**.


### 71. `Crisis.Runtime.Weapons.HammerUnsupported` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `HammerUnsupported`; partial `false`; `runtime/Engine/WeaponHost.cs:24`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `HammerUnsupported`; partial `false`; `runtime/Engine/WeaponHost.cs:24`

Common member signatures: **2**.
- **PR wins** ``method:Release`0()``; PR `public void Release()` at `runtime/Engine/WeaponHost.cs:24`; develop `public void Release()` at `runtime/Engine/WeaponHost.cs:24`.
- **PR wins** ``method:SetLevel`0(float)``; PR `public void SetLevel(float)` at `runtime/Engine/WeaponHost.cs:24`; develop `public void SetLevel(float)` at `runtime/Engine/WeaponHost.cs:24`.


### 72. `Crisis.Runtime.Weapons.Hand` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Hand`; partial `false`; `runtime/Engine/WeaponHost.cs:31`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Hand`; partial `false`; `runtime/Engine/WeaponHost.cs:31`

Common member signatures: **2**.
- **PR wins** `field:SourceVisuals`; PR `public readonly field Dictionary<string,GameObject> SourceVisuals` at `runtime/Engine/WeaponHost.cs:33`; develop `public readonly field Dictionary<string,GameObject> SourceVisuals` at `runtime/Engine/WeaponHost.cs:33`.
- **PR wins** `property:mode`; PR `public Mode mode` at `runtime/Engine/WeaponHost.cs:32`; develop `public Mode mode` at `runtime/Engine/WeaponHost.cs:32`.


### 73. `Crisis.Runtime.Weapons.Limb` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Limb`; partial `false`; `runtime/Engine/WeaponHost.cs:35`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Limb`; partial `true`; `runtime/Engine/WeaponHost.cs:35`
- class; namespace `Crisis.Runtime.Weapons`; `Limb`; partial `true`; `runtime/Original/Weapons/CharacterLimbRagdollState.cs:14`
- class; namespace `Crisis.Runtime.Weapons`; `Limb`; partial `true`; `runtime/Original/Weapons/LimbFacingDepth.cs:11`

Common member signatures: **1**.
- **PR wins** `field:Body`; PR `public field CharacterBody Body` at `runtime/Engine/WeaponHost.cs:35`; develop `public field CharacterBody Body` at `runtime/Engine/WeaponHost.cs:35`.

Develop-only member signatures: **24** — **DEVELOP-ONLY / DEFERRED**.
- **DEVELOP-ONLY — DEFERRED** `field:LimbPhysiology` — `public field OriginalLimbPhysiology LimbPhysiology` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:17`.
- **DEVELOP-ONLY — DEFERRED** `field:SourceMuscle` — `public field OriginalLimbMuscle SourceMuscle` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:18`.
- **DEVELOP-ONLY — DEFERRED** `field:SourceSkinTrace` — `public field OriginalLimbSkinTrace SourceSkinTrace` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:20`.
- **DEVELOP-ONLY — DEFERRED** `field:SourceVitals` — `public field OriginalVitalFunctions SourceVitals` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:19`.
- **DEVELOP-ONLY — DEFERRED** `field:attachments` — `public field List<LimbAttachment> attachments` at `runtime/Original/Weapons/LimbFacingDepth.cs:15`.
- **DEVELOP-ONLY — DEFERRED** `field:id` — `public field LimbId id` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:15`.
- **DEVELOP-ONLY — DEFERRED** `field:kinematicParent` — `public field Transform kinematicParent` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:16`.
- **DEVELOP-ONLY — DEFERRED** `field:potentialForce` — `field Vector3 potentialForce` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:22`.
- **DEVELOP-ONLY — DEFERRED** `field:potentialPosition` — `field Vector3 potentialPosition` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:22`.
- **DEVELOP-ONLY — DEFERRED** `field:potentialTime` — `field float potentialTime` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:23`.
- **DEVELOP-ONLY — DEFERRED** `field:renderIndexLeft` — `public field int renderIndexLeft` at `runtime/Original/Weapons/LimbFacingDepth.cs:14`.
- **DEVELOP-ONLY — DEFERRED** `field:renderIndexRight` — `public field int renderIndexRight` at `runtime/Original/Weapons/LimbFacingDepth.cs:14`.
- **DEVELOP-ONLY — DEFERRED** `field:rigidBodyDefaultDrag` — `field float rigidBodyDefaultDrag` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:23`.
- **DEVELOP-ONLY — DEFERRED** `field:visual` — `public field Transform visual` at `runtime/Original/Weapons/LimbFacingDepth.cs:12`.
- **DEVELOP-ONLY — DEFERRED** `field:zLeft` — `public field float zLeft` at `runtime/Original/Weapons/LimbFacingDepth.cs:13`.
- **DEVELOP-ONLY — DEFERRED** `field:zRight` — `field float zRight` at `runtime/Original/Weapons/LimbFacingDepth.cs:16`.
- **DEVELOP-ONLY — DEFERRED** ``method:AddTrauma`0(float, Vector3, Vector3)`` — `public void AddTrauma(float, Vector3, Vector3)` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:73`.
- **DEVELOP-ONLY — DEFERRED** ``method:ApplyForceAtPosition`0(Vector3, Vector3)`` — `public void ApplyForceAtPosition(Vector3, Vector3)` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:80`.
- **DEVELOP-ONLY — DEFERRED** ``method:ApplyResolvedSourceImpact`0(float, float, float, float, Vector3, Vector3, OriginalWound)`` — `public void ApplyResolvedSourceImpact(float, float, float, float, Vector3, Vector3, OriginalWound)` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:40`.
- **DEVELOP-ONLY — DEFERRED** ``method:CaptureSourceVisualDepth`0()`` — `public void CaptureSourceVisualDepth()` at `runtime/Original/Weapons/LimbFacingDepth.cs:17`.
- **DEVELOP-ONLY — DEFERRED** ``method:Flip`0()`` — `public void Flip()` at `runtime/Original/Weapons/LimbFacingDepth.cs:20`.
- **DEVELOP-ONLY — DEFERRED** ``method:SetFloating`0(bool)`` — `public void SetFloating(bool)` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:64`.
- **DEVELOP-ONLY — DEFERRED** ``method:SetKinematic`0(bool, Vector3)`` — `public void SetKinematic(bool, Vector3)` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:24`.
- **DEVELOP-ONLY — DEFERRED** `property:Rigidbody` — `public Rigidbody Rigidbody` at `runtime/Original/Weapons/CharacterLimbRagdollState.cs:21`.


### 74. `Crisis.Runtime.Weapons.Magazine` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Magazine`; partial `false`; `runtime/Original/Weapons/Magazine.cs:8`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Magazine`; partial `false`; `runtime/Original/Weapons/Magazine.cs:8`

Common member signatures: **14**.
- **PR wins** `constructor:Magazine(WeaponBase)`; PR `public Crisis.Runtime.Weapons.Magazine(WeaponBase)` at `runtime/Original/Weapons/Magazine.cs:48`; develop `public Crisis.Runtime.Weapons.Magazine(WeaponBase)` at `runtime/Original/Weapons/Magazine.cs:48`.
- **PR wins** `field:calibre`; PR `public field string calibre` at `runtime/Original/Weapons/Magazine.cs:14`; develop `public field string calibre` at `runtime/Original/Weapons/Magazine.cs:14`.
- **PR wins** `field:capacity`; PR `public field int capacity` at `runtime/Original/Weapons/Magazine.cs:10`; develop `public field int capacity` at `runtime/Original/Weapons/Magazine.cs:10`.
- **PR wins** `field:fireFromMagazine`; PR `private field bool fireFromMagazine` at `runtime/Original/Weapons/Magazine.cs:20`; develop `private field bool fireFromMagazine` at `runtime/Original/Weapons/Magazine.cs:20`.
- **PR wins** `field:malfunctionRate`; PR `public field float malfunctionRate` at `runtime/Original/Weapons/Magazine.cs:16`; develop `public field float malfunctionRate` at `runtime/Original/Weapons/Magazine.cs:16`.
- **PR wins** `field:roundCount`; PR `public field int roundCount` at `runtime/Original/Weapons/Magazine.cs:12`; develop `public field int roundCount` at `runtime/Original/Weapons/Magazine.cs:12`.
- **PR wins** `field:weapon`; PR `private field WeaponBase weapon` at `runtime/Original/Weapons/Magazine.cs:18`; develop `private field WeaponBase weapon` at `runtime/Original/Weapons/Magazine.cs:18`.
- **PR wins** ``method:AddOne`0()``; PR `public void AddOne()` at `runtime/Original/Weapons/Magazine.cs:75`; develop `public void AddOne()` at `runtime/Original/Weapons/Magazine.cs:75`.
- **PR wins** ``method:Fill`0()``; PR `public void Fill()` at `runtime/Original/Weapons/Magazine.cs:83`; develop `public void Fill()` at `runtime/Original/Weapons/Magazine.cs:83`.
- **PR wins** ``method:RemoveOne`0(bool)``; PR `public bool RemoveOne(bool)` at `runtime/Original/Weapons/Magazine.cs:61`; develop `public bool RemoveOne(bool)` at `runtime/Original/Weapons/Magazine.cs:61`.
- **PR wins** `property:Empty`; PR `public bool Empty` at `runtime/Original/Weapons/Magazine.cs:22`; develop `public bool Empty` at `runtime/Original/Weapons/Magazine.cs:22`.
- **PR wins** `property:Full`; PR `public bool Full` at `runtime/Original/Weapons/Magazine.cs:30`; develop `public bool Full` at `runtime/Original/Weapons/Magazine.cs:30`.
- **PR wins** `property:Ratio`; PR `public float Ratio` at `runtime/Original/Weapons/Magazine.cs:38`; develop `public float Ratio` at `runtime/Original/Weapons/Magazine.cs:38`.
- **PR wins** `property:UseMalfunctions`; PR `public bool UseMalfunctions` at `runtime/Original/Weapons/Magazine.cs:46`; develop `public bool UseMalfunctions` at `runtime/Original/Weapons/Magazine.cs:46`.


### 75. `Crisis.Runtime.Weapons.MissionView` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `MissionView`; partial `false`; `runtime/Engine/WeaponHost.cs:88`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `MissionView`; partial `false`; `runtime/Engine/WeaponHost.cs:88`

Common member signatures: **1**.
- **PR wins** `field:LastWeaponFireTimestamp`; PR `public field float LastWeaponFireTimestamp` at `runtime/Engine/WeaponHost.cs:88`; develop `public field float LastWeaponFireTimestamp` at `runtime/Engine/WeaponHost.cs:88`.


### 76. `Crisis.Runtime.Weapons.OriginalArmPhysicsHost` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `OriginalArmPhysicsHost`; partial `false`; `runtime/Engine/OriginalArmPhysicsHost.cs:12`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `OriginalArmPhysicsHost`; partial `false`; `runtime/Engine/OriginalArmPhysicsHost.cs:13`

Common member signatures: **12**.
- **PR wins** `field:bodies`; PR `readonly field Dictionary<string,Rigidbody> bodies` at `runtime/Engine/OriginalArmPhysicsHost.cs:15`; develop `readonly field Dictionary<string,Rigidbody> bodies` at `runtime/Engine/OriginalArmPhysicsHost.cs:16`.
- **PR wins** `field:body`; PR `readonly field CharacterBody body` at `runtime/Engine/OriginalArmPhysicsHost.cs:14`; develop `readonly field CharacterBody body` at `runtime/Engine/OriginalArmPhysicsHost.cs:15`.
- **PR wins** `field:commands`; PR `readonly field List<object> commands` at `runtime/Engine/OriginalArmPhysicsHost.cs:18`; develop `readonly field List<object> commands` at `runtime/Engine/OriginalArmPhysicsHost.cs:19`.
- **PR wins** `field:componentToBody`; PR `readonly field Dictionary<string,string> componentToBody` at `runtime/Engine/OriginalArmPhysicsHost.cs:17`; develop `readonly field Dictionary<string,string> componentToBody` at `runtime/Engine/OriginalArmPhysicsHost.cs:18`.
- **PR wins** `field:configuration`; PR `readonly field JsonElement configuration` at `runtime/Engine/OriginalArmPhysicsHost.cs:19`; develop `readonly field JsonElement configuration` at `runtime/Engine/OriginalArmPhysicsHost.cs:20`.
- **PR wins** `field:controller`; PR `readonly field ArmsControl controller` at `runtime/Engine/OriginalArmPhysicsHost.cs:13`; develop `readonly field ArmsControl controller` at `runtime/Engine/OriginalArmPhysicsHost.cs:14`.
- **PR wins** `field:joints`; PR `readonly field Dictionary<string,HingeJoint> joints` at `runtime/Engine/OriginalArmPhysicsHost.cs:16`; develop `readonly field Dictionary<string,HingeJoint> joints` at `runtime/Engine/OriginalArmPhysicsHost.cs:17`.
- **PR wins** `field:transforms`; PR `readonly field Dictionary<string,Transform> transforms` at `runtime/Engine/OriginalArmPhysicsHost.cs:20`; develop `readonly field Dictionary<string,Transform> transforms` at `runtime/Engine/OriginalArmPhysicsHost.cs:34`.
- **PR wins** ``method:Arr`0(Vector3)``; PR `static float[] Arr(Vector3)` at `runtime/Engine/OriginalArmPhysicsHost.cs:23`; develop `static float[] Arr(Vector3)` at `runtime/Engine/OriginalArmPhysicsHost.cs:37`.
- **PR wins** ``method:Snapshot`0()``; PR `public object Snapshot()` at `runtime/Engine/OriginalArmPhysicsHost.cs:107`; develop `public object Snapshot()` at `runtime/Engine/OriginalArmPhysicsHost.cs:358`.
- **PR wins** ``method:Step`0(JsonElement, RuntimeWorld)``; PR `public void Step(JsonElement, RuntimeWorld)` at `runtime/Engine/OriginalArmPhysicsHost.cs:98`; develop `public void Step(JsonElement, RuntimeWorld)` at `runtime/Engine/OriginalArmPhysicsHost.cs:349`.
- **PR wins** ``method:Vector`0(JsonElement)``; PR `static Vector3 Vector(JsonElement)` at `runtime/Engine/OriginalArmPhysicsHost.cs:21`; develop `static Vector3 Vector(JsonElement)` at `runtime/Engine/OriginalArmPhysicsHost.cs:35`.

Develop-only member signatures: **15** — **DEVELOP-ONLY / DEFERRED**.
- **DEVELOP-ONLY — DEFERRED** `constructor:OriginalArmPhysicsHost(JsonElement, Dictionary<string,Transform>, CharacterBody, RuntimeWorld, JsonElement)` — `public Crisis.Runtime.Weapons.OriginalArmPhysicsHost(JsonElement, Dictionary<string,Transform>, CharacterBody, RuntimeWorld, JsonElement)` at `runtime/Engine/OriginalArmPhysicsHost.cs:38`.
- **DEVELOP-ONLY — DEFERRED** `field:authenticatedLimbColliders` — `readonly field List<CapsuleCollider> authenticatedLimbColliders` at `runtime/Engine/OriginalArmPhysicsHost.cs:25`.
- **DEVELOP-ONLY — DEFERRED** `field:authoredCapsulesByGo` — `readonly field Dictionary<string,JsonElement> authoredCapsulesByGo` at `runtime/Engine/OriginalArmPhysicsHost.cs:21`.
- **DEVELOP-ONLY — DEFERRED** `field:authoredLimbs` — `readonly field Dictionary<LimbId,JsonElement> authoredLimbs` at `runtime/Engine/OriginalArmPhysicsHost.cs:22`.
- **DEVELOP-ONLY — DEFERRED** `field:runtimeLimbs` — `readonly field Dictionary<LimbId,Limb> runtimeLimbs` at `runtime/Engine/OriginalArmPhysicsHost.cs:23`.
- **DEVELOP-ONLY — DEFERRED** `field:sourceBallisticProperties` — `readonly field Dictionary<string,BallisticMaterialProperties> sourceBallisticProperties` at `runtime/Engine/OriginalArmPhysicsHost.cs:24`.
- **DEVELOP-ONLY — DEFERRED** ``method:BindOriginalPhysiology`0()`` — `public void BindOriginalPhysiology()` at `runtime/Engine/OriginalArmPhysicsHost.cs:314`.
- **DEVELOP-ONLY — DEFERRED** ``method:BindOriginalSkinCoverage`0(JsonElement)`` — `public void BindOriginalSkinCoverage(JsonElement)` at `runtime/Engine/OriginalArmPhysicsHost.cs:181`.
- **DEVELOP-ONLY — DEFERRED** ``method:BindSourceBallistics`0(JsonElement, BallisticRuntimeHost, RuntimeWorld)`` — `public void BindSourceBallistics(JsonElement, BallisticRuntimeHost, RuntimeWorld)` at `runtime/Engine/OriginalArmPhysicsHost.cs:219`.
- **DEVELOP-ONLY — DEFERRED** `property:SourceActorCount` — `public int SourceActorCount` at `runtime/Engine/OriginalArmPhysicsHost.cs:32`.
- **DEVELOP-ONLY — DEFERRED** `property:SourceJointCount` — `public int SourceJointCount` at `runtime/Engine/OriginalArmPhysicsHost.cs:33`.
- **DEVELOP-ONLY — DEFERRED** `property:SourceLimbArmorCount` — `public int SourceLimbArmorCount` at `runtime/Engine/OriginalArmPhysicsHost.cs:28`.
- **DEVELOP-ONLY — DEFERRED** `property:SourceLimbColliderCount` — `public int SourceLimbColliderCount` at `runtime/Engine/OriginalArmPhysicsHost.cs:26`.
- **DEVELOP-ONLY — DEFERRED** `property:SourceLimbMaterialCount` — `public int SourceLimbMaterialCount` at `runtime/Engine/OriginalArmPhysicsHost.cs:27`.
- **DEVELOP-ONLY — DEFERRED** `property:SourceLimbProjectileContactsActive` — `public bool SourceLimbProjectileContactsActive` at `runtime/Engine/OriginalArmPhysicsHost.cs:31`.


### 77. `Crisis.Runtime.Weapons.ProjectileEvent` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `ProjectileEvent`; partial `false`; `runtime/Engine/WeaponHost.cs:87`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `ProjectileEvent`; partial `false`; `runtime/Engine/WeaponHost.cs:87`

Common member signatures: **2**.
- **PR wins** `field:callback`; PR `public field Action<global::Projectile> callback` at `runtime/Engine/WeaponHost.cs:87`; develop `public field Action<global::Projectile> callback` at `runtime/Engine/WeaponHost.cs:87`.
- **PR wins** ``method:Invoke`0(global::Projectile)``; PR `public void Invoke(global::Projectile)` at `runtime/Engine/WeaponHost.cs:87`; develop `public void Invoke(global::Projectile)` at `runtime/Engine/WeaponHost.cs:87`.


### 78. `Crisis.Runtime.Weapons.PropagatingAudioSample` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `PropagatingAudioSample`; partial `false`; `runtime/Engine/WeaponHost.cs:15`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `PropagatingAudioSample`; partial `false`; `runtime/Engine/WeaponHost.cs:15`

Common member signatures: **4**.
- **PR wins** `constructor:PropagatingAudioSample(WeaponEffects, string)`; PR `public Crisis.Runtime.Weapons.PropagatingAudioSample(WeaponEffects, string)` at `runtime/Engine/WeaponHost.cs:17`; develop `public Crisis.Runtime.Weapons.PropagatingAudioSample(WeaponEffects, string)` at `runtime/Engine/WeaponHost.cs:17`.
- **PR wins** `field:effects`; PR `readonly field WeaponEffects effects` at `runtime/Engine/WeaponHost.cs:16`; develop `readonly field WeaponEffects effects` at `runtime/Engine/WeaponHost.cs:16`.
- **PR wins** `field:name`; PR `readonly field string name` at `runtime/Engine/WeaponHost.cs:16`; develop `readonly field string name` at `runtime/Engine/WeaponHost.cs:16`.
- **PR wins** ``method:Play`0(float, object)``; PR `public void Play(float, object)` at `runtime/Engine/WeaponHost.cs:18`; develop `public void Play(float, object)` at `runtime/Engine/WeaponHost.cs:18`.


### 79. `Crisis.Runtime.Weapons.Recoil` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Recoil`; partial `false`; `runtime/Original/Weapons/Recoil.cs:7`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `Recoil`; partial `false`; `runtime/Original/Weapons/Recoil.cs:7`

Common member signatures: **4**.
- **PR wins** `constructor:Recoil(float, float, float)`; PR `public Crisis.Runtime.Weapons.Recoil(float, float, float)` at `runtime/Original/Weapons/Recoil.cs:15`; develop `public Crisis.Runtime.Weapons.Recoil(float, float, float)` at `runtime/Original/Weapons/Recoil.cs:15`.
- **PR wins** `field:climb`; PR `public field float climb` at `runtime/Original/Weapons/Recoil.cs:13`; develop `public field float climb` at `runtime/Original/Weapons/Recoil.cs:13`.
- **PR wins** `field:kick`; PR `public field float kick` at `runtime/Original/Weapons/Recoil.cs:11`; develop `public field float kick` at `runtime/Original/Weapons/Recoil.cs:11`.
- **PR wins** `field:timestamp`; PR `public field float timestamp` at `runtime/Original/Weapons/Recoil.cs:9`; develop `public field float timestamp` at `runtime/Original/Weapons/Recoil.cs:9`.


### 80. `Crisis.Runtime.Weapons.SettingsView` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `SettingsView`; partial `false`; `runtime/Engine/WeaponHost.cs:58`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `SettingsView`; partial `false`; `runtime/Engine/WeaponHost.cs:58`

Common member signatures: **1**.
- **PR wins** `field:General`; PR `public field GeneralView General` at `runtime/Engine/WeaponHost.cs:58`; develop `public field GeneralView General` at `runtime/Engine/WeaponHost.cs:58`.


### 81. `Crisis.Runtime.Weapons.TaskStatus` — PR type wins

**PR declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `TaskStatus`; partial `false`; `runtime/Engine/WeaponHost.cs:54`

**develop declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `TaskStatus`; partial `false`; `runtime/Engine/WeaponHost.cs:54`

Common member signatures: **0**.


### 82. `Crisis.Runtime.Weapons.WeaponAmmunition` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponAmmunition`; partial `false`; `runtime/Engine/WeaponHost.cs:25`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponAmmunition`; partial `false`; `runtime/Engine/WeaponHost.cs:25`

Common member signatures: **2**.
- **PR wins** `field:casing`; PR `public field Casing casing` at `runtime/Engine/WeaponHost.cs:25`; develop `public field Casing casing` at `runtime/Engine/WeaponHost.cs:25`.
- **PR wins** `field:projectile`; PR `public field global::Projectile projectile` at `runtime/Engine/WeaponHost.cs:25`; develop `public field global::Projectile projectile` at `runtime/Engine/WeaponHost.cs:25`.


### 83. `Crisis.Runtime.Weapons.WeaponArmConfiguration` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponArmConfiguration`; partial `false`; `runtime/Engine/WeaponHost.cs:36`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponArmConfiguration`; partial `false`; `runtime/Engine/WeaponHost.cs:36`

Common member signatures: **4**.
- **PR wins** `field:leftHandAngle`; PR `public field float leftHandAngle` at `runtime/Engine/WeaponHost.cs:36`; develop `public field float leftHandAngle` at `runtime/Engine/WeaponHost.cs:36`.
- **PR wins** `field:leftHandMode`; PR `public field Hand.Mode leftHandMode` at `runtime/Engine/WeaponHost.cs:36`; develop `public field Hand.Mode leftHandMode` at `runtime/Engine/WeaponHost.cs:36`.
- **PR wins** `field:rightHandAngle`; PR `public field float rightHandAngle` at `runtime/Engine/WeaponHost.cs:36`; develop `public field float rightHandAngle` at `runtime/Engine/WeaponHost.cs:36`.
- **PR wins** `field:rightHandMode`; PR `public field Hand.Mode rightHandMode` at `runtime/Engine/WeaponHost.cs:36`; develop `public field Hand.Mode rightHandMode` at `runtime/Engine/WeaponHost.cs:36`.


### 84. `Crisis.Runtime.Weapons.WeaponBarrel` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponBarrel`; partial `false`; `runtime/Original/Weapons/WeaponBarrel.cs:8`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponBarrel`; partial `false`; `runtime/Original/Weapons/WeaponBarrel.cs:8`

Common member signatures: **18**.
- **PR wins** `field:angle`; PR `public field AnimationCurve angle` at `runtime/Original/Weapons/WeaponBarrel.cs:18`; develop `public field AnimationCurve angle` at `runtime/Original/Weapons/WeaponBarrel.cs:18`.
- **PR wins** `field:animateBarrel`; PR `public field bool animateBarrel` at `runtime/Original/Weapons/WeaponBarrel.cs:12`; develop `public field bool animateBarrel` at `runtime/Original/Weapons/WeaponBarrel.cs:12`.
- **PR wins** `field:animateChamber`; PR `public field bool animateChamber` at `runtime/Original/Weapons/WeaponBarrel.cs:21`; develop `public field bool animateChamber` at `runtime/Original/Weapons/WeaponBarrel.cs:21`.
- **PR wins** `field:barrelAngle`; PR `private field Vector3 barrelAngle` at `runtime/Original/Weapons/WeaponBarrel.cs:33`; develop `private field Vector3 barrelAngle` at `runtime/Original/Weapons/WeaponBarrel.cs:33`.
- **PR wins** `field:barrelPosition`; PR `private field Vector3 barrelPosition` at `runtime/Original/Weapons/WeaponBarrel.cs:31`; develop `private field Vector3 barrelPosition` at `runtime/Original/Weapons/WeaponBarrel.cs:31`.
- **PR wins** `field:barrelToMagazineDistance`; PR `private field float barrelToMagazineDistance` at `runtime/Original/Weapons/WeaponBarrel.cs:41`; develop `private field float barrelToMagazineDistance` at `runtime/Original/Weapons/WeaponBarrel.cs:41`.
- **PR wins** `field:chamber`; PR `public field Transform chamber` at `runtime/Original/Weapons/WeaponBarrel.cs:23`; develop `public field Transform chamber` at `runtime/Original/Weapons/WeaponBarrel.cs:23`.
- **PR wins** `field:chamberAngle`; PR `private field Vector3 chamberAngle` at `runtime/Original/Weapons/WeaponBarrel.cs:39`; develop `private field Vector3 chamberAngle` at `runtime/Original/Weapons/WeaponBarrel.cs:39`.
- **PR wins** `field:chamberPosition`; PR `private field Vector3 chamberPosition` at `runtime/Original/Weapons/WeaponBarrel.cs:37`; develop `private field Vector3 chamberPosition` at `runtime/Original/Weapons/WeaponBarrel.cs:37`.
- **PR wins** `field:closedPosition`; PR `private field Vector3 closedPosition` at `runtime/Original/Weapons/WeaponBarrel.cs:35`; develop `private field Vector3 closedPosition` at `runtime/Original/Weapons/WeaponBarrel.cs:35`.
- **PR wins** `field:feedAngle`; PR `public field AnimationCurve feedAngle` at `runtime/Original/Weapons/WeaponBarrel.cs:29`; develop `public field AnimationCurve feedAngle` at `runtime/Original/Weapons/WeaponBarrel.cs:29`.
- **PR wins** `field:feedOffset`; PR `public field AnimationCurve feedOffset` at `runtime/Original/Weapons/WeaponBarrel.cs:27`; develop `public field AnimationCurve feedOffset` at `runtime/Original/Weapons/WeaponBarrel.cs:27`.
- **PR wins** `field:followDistance`; PR `public field float followDistance` at `runtime/Original/Weapons/WeaponBarrel.cs:14`; develop `public field float followDistance` at `runtime/Original/Weapons/WeaponBarrel.cs:14`.
- **PR wins** `field:magazine`; PR `public field Transform magazine` at `runtime/Original/Weapons/WeaponBarrel.cs:25`; develop `public field Transform magazine` at `runtime/Original/Weapons/WeaponBarrel.cs:25`.
- **PR wins** `field:offset`; PR `public field AnimationCurve offset` at `runtime/Original/Weapons/WeaponBarrel.cs:16`; develop `public field AnimationCurve offset` at `runtime/Original/Weapons/WeaponBarrel.cs:16`.
- **PR wins** ``method:Animate`0(bool, float, float, float)``; PR `public void Animate(bool, float, float, float)` at `runtime/Original/Weapons/WeaponBarrel.cs:53`; develop `public void Animate(bool, float, float, float)` at `runtime/Original/Weapons/WeaponBarrel.cs:53`.
- **PR wins** ``method:Awake`0()``; PR `private void Awake()` at `runtime/Original/Weapons/WeaponBarrel.cs:43`; develop `private void Awake()` at `runtime/Original/Weapons/WeaponBarrel.cs:43`.
- **PR wins** ``method:SetClosed`0()``; PR `public void SetClosed()` at `runtime/Original/Weapons/WeaponBarrel.cs:75`; develop `public void SetClosed()` at `runtime/Original/Weapons/WeaponBarrel.cs:75`.


### 85. `Crisis.Runtime.Weapons.WeaponBase` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponBase`; partial `true`; `runtime/Engine/WeaponHost.cs:62`
- class; namespace `Crisis.Runtime.Weapons`; `WeaponBase`; partial `true`; `runtime/Original/Weapons/WeaponMechanism.Methods.cs:7`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponBase`; partial `true`; `runtime/Engine/WeaponHost.cs:62`
- class; namespace `Crisis.Runtime.Weapons`; `WeaponBase`; partial `true`; `runtime/Original/Weapons/WeaponMechanism.Methods.cs:7`

Common member signatures: **91**.
- **PR wins** `field:ChamberStatus`; PR `public field ChamberState ChamberStatus` at `runtime/Engine/WeaponHost.cs:72`; develop `public field ChamberState ChamberStatus` at `runtime/Engine/WeaponHost.cs:72`.
- **PR wins** `field:CombinedInaccuracy`; PR `public field float CombinedInaccuracy` at `runtime/Engine/WeaponHost.cs:80`; develop `public field float CombinedInaccuracy` at `runtime/Engine/WeaponHost.cs:80`.
- **PR wins** `field:ComponentMagazine`; PR `public field WeaponComponentMagazine ComponentMagazine` at `runtime/Engine/WeaponHost.cs:65`; develop `public field WeaponComponentMagazine ComponentMagazine` at `runtime/Engine/WeaponHost.cs:65`.
- **PR wins** `field:Effects`; PR `public field WeaponEffects Effects` at `runtime/Engine/WeaponHost.cs:64`; develop `public field WeaponEffects Effects` at `runtime/Engine/WeaponHost.cs:64`.
- **PR wins** `field:HammerReady`; PR `public field bool HammerReady` at `runtime/Engine/WeaponHost.cs:72`; develop `public field bool HammerReady` at `runtime/Engine/WeaponHost.cs:72`.
- **PR wins** `field:IsHeld`; PR `public field bool IsHeld` at `runtime/Engine/WeaponHost.cs:66`; develop `public field bool IsHeld` at `runtime/Engine/WeaponHost.cs:66`.
- **PR wins** `field:IsPrimary`; PR `public field bool IsPrimary` at `runtime/Engine/WeaponHost.cs:66`; develop `public field bool IsPrimary` at `runtime/Engine/WeaponHost.cs:66`.
- **PR wins** `field:IsRagdoll`; PR `public field bool IsRagdoll` at `runtime/Engine/WeaponHost.cs:66`; develop `public field bool IsRagdoll` at `runtime/Engine/WeaponHost.cs:66`.
- **PR wins** `field:MalfunctionsMultiplier`; PR `public field float MalfunctionsMultiplier` at `runtime/Engine/WeaponHost.cs:67`; develop `public field float MalfunctionsMultiplier` at `runtime/Engine/WeaponHost.cs:67`.
- **PR wins** `field:Muzzle`; PR `public field Transform Muzzle` at `runtime/Engine/WeaponHost.cs:70`; develop `public field Transform Muzzle` at `runtime/Engine/WeaponHost.cs:70`.
- **PR wins** `field:Owner`; PR `public field CharacterBody Owner` at `runtime/Engine/WeaponHost.cs:64`; develop `public field CharacterBody Owner` at `runtime/Engine/WeaponHost.cs:64`.
- **PR wins** `field:PrecisionBegin`; PR `public field float PrecisionBegin` at `runtime/Engine/WeaponHost.cs:80`; develop `public field float PrecisionBegin` at `runtime/Engine/WeaponHost.cs:80`.
- **PR wins** `field:PrecisionDistance`; PR `public field float PrecisionDistance` at `runtime/Engine/WeaponHost.cs:80`; develop `public field float PrecisionDistance` at `runtime/Engine/WeaponHost.cs:80`.
- **PR wins** `field:ammunition`; PR `public field WeaponAmmunition ammunition` at `runtime/Engine/WeaponHost.cs:65`; develop `public field WeaponAmmunition ammunition` at `runtime/Engine/WeaponHost.cs:65`.
- **PR wins** `field:armConfiguration`; PR `public field WeaponArmConfiguration armConfiguration` at `runtime/Engine/WeaponHost.cs:65`; develop `public field WeaponArmConfiguration armConfiguration` at `runtime/Engine/WeaponHost.cs:65`.
- **PR wins** `field:audioBoltExtract`; PR `public field PropagatingAudioSample audioBoltExtract` at `runtime/Engine/WeaponHost.cs:77`; develop `public field PropagatingAudioSample audioBoltExtract` at `runtime/Engine/WeaponHost.cs:77`.
- **PR wins** `field:audioBoltReturn`; PR `public field PropagatingAudioSample audioBoltReturn` at `runtime/Engine/WeaponHost.cs:77`; develop `public field PropagatingAudioSample audioBoltReturn` at `runtime/Engine/WeaponHost.cs:77`.
- **PR wins** `field:audioExtractMagazine`; PR `public field PropagatingAudioSample audioExtractMagazine` at `runtime/Engine/WeaponHost.cs:77`; develop `public field PropagatingAudioSample audioExtractMagazine` at `runtime/Engine/WeaponHost.cs:77`.
- **PR wins** `field:audioFiringPin`; PR `public field PropagatingAudioSample audioFiringPin` at `runtime/Engine/WeaponHost.cs:77`; develop `public field PropagatingAudioSample audioFiringPin` at `runtime/Engine/WeaponHost.cs:77`.
- **PR wins** `field:audioInsertMagazine`; PR `public field PropagatingAudioSample audioInsertMagazine` at `runtime/Engine/WeaponHost.cs:77`; develop `public field PropagatingAudioSample audioInsertMagazine` at `runtime/Engine/WeaponHost.cs:77`.
- **PR wins** `field:barrel`; PR `public field WeaponBarrel barrel` at `runtime/Engine/WeaponHost.cs:64`; develop `public field WeaponBarrel barrel` at `runtime/Engine/WeaponHost.cs:64`.
- **PR wins** `field:boltHandleTransform`; PR `public field Transform boltHandleTransform` at `runtime/Engine/WeaponHost.cs:70`; develop `public field Transform boltHandleTransform` at `runtime/Engine/WeaponHost.cs:70`.
- **PR wins** `field:boltManipulationTransitionTime`; PR `public field float boltManipulationTransitionTime` at `runtime/Engine/WeaponHost.cs:69`; develop `public field float boltManipulationTransitionTime` at `runtime/Engine/WeaponHost.cs:69`.
- **PR wins** `field:boltReleaseAnimation`; PR `public field BoltReleaseAnimation boltReleaseAnimation` at `runtime/Engine/WeaponHost.cs:69`; develop `public field BoltReleaseAnimation boltReleaseAnimation` at `runtime/Engine/WeaponHost.cs:69`.
- **PR wins** `field:boltReleasePosition`; PR `public field Vector3 boltReleasePosition` at `runtime/Engine/WeaponHost.cs:69`; develop `public field Vector3 boltReleasePosition` at `runtime/Engine/WeaponHost.cs:69`.
- **PR wins** `field:boreDiameter`; PR `public field float boreDiameter` at `runtime/Engine/WeaponHost.cs:80`; develop `public field float boreDiameter` at `runtime/Engine/WeaponHost.cs:80`.
- **PR wins** `field:burstCount`; PR `public field int burstCount` at `runtime/Engine/WeaponHost.cs:71`; develop `public field int burstCount` at `runtime/Engine/WeaponHost.cs:71`.
- **PR wins** `field:burstRoundsRemaining`; PR `public field int burstRoundsRemaining` at `runtime/Engine/WeaponHost.cs:71`; develop `public field int burstRoundsRemaining` at `runtime/Engine/WeaponHost.cs:71`.
- **PR wins** `field:calibre`; PR `public field string calibre` at `runtime/Engine/WeaponHost.cs:66`; develop `public field string calibre` at `runtime/Engine/WeaponHost.cs:66`.
- **PR wins** `field:climbValue`; PR `public field float climbValue` at `runtime/Engine/WeaponHost.cs:68`; develop `public field float climbValue` at `runtime/Engine/WeaponHost.cs:68`.
- **PR wins** `field:consecutiveRounds`; PR `public field int consecutiveRounds` at `runtime/Engine/WeaponHost.cs:71`; develop `public field int consecutiveRounds` at `runtime/Engine/WeaponHost.cs:71`.
- **PR wins** `field:cyclingBolt`; PR `public field CyclingBolt cyclingBolt` at `runtime/Engine/WeaponHost.cs:64`; develop `public field CyclingBolt cyclingBolt` at `runtime/Engine/WeaponHost.cs:64`.
- **PR wins** `field:direction`; PR `public field float direction` at `runtime/Engine/WeaponHost.cs:80`; develop `public field float direction` at `runtime/Engine/WeaponHost.cs:80`.
- **PR wins** `field:fireFromMagazine`; PR `public field bool fireFromMagazine` at `runtime/Engine/WeaponHost.cs:66`; develop `public field bool fireFromMagazine` at `runtime/Engine/WeaponHost.cs:66`.
- **PR wins** `field:fireModeIndex`; PR `public field int fireModeIndex` at `runtime/Engine/WeaponHost.cs:71`; develop `public field int fireModeIndex` at `runtime/Engine/WeaponHost.cs:71`.
- **PR wins** `field:fireModes`; PR `public field List<FireMode> fireModes` at `runtime/Engine/WeaponHost.cs:71`; develop `public field List<FireMode> fireModes` at `runtime/Engine/WeaponHost.cs:71`.
- **PR wins** `field:hammer`; PR `public field HammerUnsupported hammer` at `runtime/Engine/WeaponHost.cs:72`; develop `public field HammerUnsupported hammer` at `runtime/Engine/WeaponHost.cs:72`.
- **PR wins** `field:kickValue`; PR `public field float kickValue` at `runtime/Engine/WeaponHost.cs:68`; develop `public field float kickValue` at `runtime/Engine/WeaponHost.cs:68`.
- **PR wins** `field:leftHandAnchor`; PR `public field Transform leftHandAnchor` at `runtime/Engine/WeaponHost.cs:70`; develop `public field Transform leftHandAnchor` at `runtime/Engine/WeaponHost.cs:70`.
- **PR wins** `field:leftHandBoltClose`; PR `public field bool leftHandBoltClose` at `runtime/Engine/WeaponHost.cs:69`; develop `public field bool leftHandBoltClose` at `runtime/Engine/WeaponHost.cs:69`.
- **PR wins** `field:leftHandBoltManipulation`; PR `public field bool leftHandBoltManipulation` at `runtime/Engine/WeaponHost.cs:69`; develop `public field bool leftHandBoltManipulation` at `runtime/Engine/WeaponHost.cs:69`.
- **PR wins** `field:magazineInsertTrack`; PR `public field Vector3 magazineInsertTrack` at `runtime/Engine/WeaponHost.cs:69`; develop `public field Vector3 magazineInsertTrack` at `runtime/Engine/WeaponHost.cs:69`.
- **PR wins** `field:magazineTransform`; PR `public field Transform magazineTransform` at `runtime/Engine/WeaponHost.cs:70`; develop `public field Transform magazineTransform` at `runtime/Engine/WeaponHost.cs:70`.
- **PR wins** `field:manualCycling`; PR `public field bool manualCycling` at `runtime/Engine/WeaponHost.cs:66`; develop `public field bool manualCycling` at `runtime/Engine/WeaponHost.cs:66`.
- **PR wins** `field:mission`; PR `public field MissionView mission` at `runtime/Engine/WeaponHost.cs:83`; develop `public field MissionView mission` at `runtime/Engine/WeaponHost.cs:83`.
- **PR wins** `field:onNewMagazine`; PR `public field UnityEngine.Events.UnityEvent onNewMagazine` at `runtime/Engine/WeaponHost.cs:76`; develop `public field UnityEngine.Events.UnityEvent onNewMagazine` at `runtime/Engine/WeaponHost.cs:76`.
- **PR wins** `field:onSpawnProjectile`; PR `public field ProjectileEvent onSpawnProjectile` at `runtime/Engine/WeaponHost.cs:82`; develop `public field ProjectileEvent onSpawnProjectile` at `runtime/Engine/WeaponHost.cs:82`.
- **PR wins** `field:onSpawnProjectileCallback`; PR `public field Action<global::Projectile> onSpawnProjectileCallback` at `runtime/Engine/WeaponHost.cs:81`; develop `public field Action<global::Projectile> onSpawnProjectileCallback` at `runtime/Engine/WeaponHost.cs:81`.
- **PR wins** `field:projectileForceMultiplier`; PR `public field float projectileForceMultiplier` at `runtime/Engine/WeaponHost.cs:80`; develop `public field float projectileForceMultiplier` at `runtime/Engine/WeaponHost.cs:80`.
- **PR wins** `field:projectilesContainer`; PR `public field Transform projectilesContainer` at `runtime/Engine/WeaponHost.cs:81`; develop `public field Transform projectilesContainer` at `runtime/Engine/WeaponHost.cs:81`.
- **PR wins** `field:recoilClimbCycle`; PR `public field AnimationCurve recoilClimbCycle` at `runtime/Engine/WeaponHost.cs:73`; develop `public field AnimationCurve recoilClimbCycle` at `runtime/Engine/WeaponHost.cs:73`.
- **PR wins** `field:recoilClimbModifier`; PR `public field float recoilClimbModifier` at `runtime/Engine/WeaponHost.cs:67`; develop `public field float recoilClimbModifier` at `runtime/Engine/WeaponHost.cs:67`.
- **PR wins** `field:recoilCycle`; PR `public field AnimationCurve recoilCycle` at `runtime/Engine/WeaponHost.cs:73`; develop `public field AnimationCurve recoilCycle` at `runtime/Engine/WeaponHost.cs:73`.
- **PR wins** `field:recoilKickModifier`; PR `public field float recoilKickModifier` at `runtime/Engine/WeaponHost.cs:67`; develop `public field float recoilKickModifier` at `runtime/Engine/WeaponHost.cs:67`.
- **PR wins** `field:recoilList`; PR `readonly field List<Recoil> recoilList` at `runtime/Engine/WeaponHost.cs:73`; develop `readonly field List<Recoil> recoilList` at `runtime/Engine/WeaponHost.cs:73`.
- **PR wins** `field:recoilPeriod`; PR `public field float recoilPeriod` at `runtime/Engine/WeaponHost.cs:68`; develop `public field float recoilPeriod` at `runtime/Engine/WeaponHost.cs:68`.
- **PR wins** `field:recoilSuppressorModifier`; PR `public field float recoilSuppressorModifier` at `runtime/Engine/WeaponHost.cs:68`; develop `public field float recoilSuppressorModifier` at `runtime/Engine/WeaponHost.cs:68`.
- **PR wins** `field:recoilToWeightFactor`; PR `public field float recoilToWeightFactor` at `runtime/Engine/WeaponHost.cs:68`; develop `public field float recoilToWeightFactor` at `runtime/Engine/WeaponHost.cs:68`.
- **PR wins** `field:rifleTwistRate`; PR `public field float rifleTwistRate` at `runtime/Engine/WeaponHost.cs:80`; develop `public field float rifleTwistRate` at `runtime/Engine/WeaponHost.cs:80`.
- **PR wins** `field:rifledBarrel`; PR `public field bool rifledBarrel` at `runtime/Engine/WeaponHost.cs:80`; develop `public field bool rifledBarrel` at `runtime/Engine/WeaponHost.cs:80`.
- **PR wins** `field:rightHandAnchor`; PR `public field Transform rightHandAnchor` at `runtime/Engine/WeaponHost.cs:70`; develop `public field Transform rightHandAnchor` at `runtime/Engine/WeaponHost.cs:70`.
- **PR wins** `field:roundsPerMinute`; PR `public field float roundsPerMinute` at `runtime/Engine/WeaponHost.cs:67`; develop `public field float roundsPerMinute` at `runtime/Engine/WeaponHost.cs:67`.
- **PR wins** `field:staticWeight`; PR `public field float staticWeight` at `runtime/Engine/WeaponHost.cs:68`; develop `public field float staticWeight` at `runtime/Engine/WeaponHost.cs:68`.
- **PR wins** `field:type`; PR `public field WeaponBrace type` at `runtime/Engine/WeaponHost.cs:75`; develop `public field WeaponBrace type` at `runtime/Engine/WeaponHost.cs:75`.
- **PR wins** `field:useInternalMagazine`; PR `public field bool useInternalMagazine` at `runtime/Engine/WeaponHost.cs:66`; develop `public field bool useInternalMagazine` at `runtime/Engine/WeaponHost.cs:66`.
- **PR wins** `field:velocityModifier`; PR `public field float velocityModifier` at `runtime/Engine/WeaponHost.cs:80`; develop `public field float velocityModifier` at `runtime/Engine/WeaponHost.cs:80`.
- **PR wins** `field:weight`; PR `public field float weight` at `runtime/Engine/WeaponHost.cs:67`; develop `public field float weight` at `runtime/Engine/WeaponHost.cs:67`.
- **PR wins** ``method:AddSingleRecoil`0()``; PR `private void AddSingleRecoil()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:151`; develop `private void AddSingleRecoil()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:151`.
- **PR wins** ``method:CalculateRecoil`0()``; PR `private void CalculateRecoil()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:123`; develop `private void CalculateRecoil()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:123`.
- **PR wins** ``method:DetonateRound`0()``; PR `private void DetonateRound()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:60`; develop `private void DetonateRound()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:60`.
- **PR wins** ``method:EmitMuzzleAudio`0(bool)``; PR `public void EmitMuzzleAudio(bool)` at `runtime/Engine/WeaponHost.cs:84`; develop `public void EmitMuzzleAudio(bool)` at `runtime/Engine/WeaponHost.cs:84`.
- **PR wins** ``method:GetNextRoundFromMagazine`0(bool)``; PR `public Casing GetNextRoundFromMagazine(bool)` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:111`; develop `public Casing GetNextRoundFromMagazine(bool)` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:111`.
- **PR wins** ``method:InitializeRecoil`0()``; PR `public void InitializeRecoil()` at `runtime/Engine/WeaponHost.cs:79`; develop `public void InitializeRecoil()` at `runtime/Engine/WeaponHost.cs:79`.
- **PR wins** ``method:InsertMagazine`0(Magazine)``; PR `public bool InsertMagazine(Magazine)` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:173`; develop `public bool InsertMagazine(Magazine)` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:173`.
- **PR wins** ``method:MagazineWeight`0()``; PR `private float MagazineWeight()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:160`; develop `private float MagazineWeight()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:160`.
- **PR wins** ``method:OnBoltClosed`0(bool)``; PR `public void OnBoltClosed(bool)` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:104`; develop `public void OnBoltClosed(bool)` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:104`.
- **PR wins** ``method:OnBoltOpened`0()``; PR `public void OnBoltOpened()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:91`; develop `public void OnBoltOpened()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:91`.
- **PR wins** ``method:RecoilImpulse`0()``; PR `public float RecoilImpulse()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:147`; develop `public float RecoilImpulse()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:147`.
- **PR wins** ``method:ReleaseFiringPin`0()``; PR `private void ReleaseFiringPin()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:30`; develop `private void ReleaseFiringPin()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:30`.
- **PR wins** ``method:RemoveMagazine`0(bool, bool)``; PR `public void RemoveMagazine(bool, bool)` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:190`; develop `public void RemoveMagazine(bool, bool)` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:190`.
- **PR wins** ``method:ResetBurstCount`0()``; PR `public void ResetBurstCount()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:86`; develop `public void ResetBurstCount()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:86`.
- **PR wins** ``method:ReturnMagazine`0()``; PR `public void ReturnMagazine()` at `runtime/Engine/WeaponHost.cs:85`; develop `public void ReturnMagazine()` at `runtime/Engine/WeaponHost.cs:85`.
- **PR wins** ``method:SpawnProjectiles`0()``; PR `private void SpawnProjectiles()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:221`; develop `private void SpawnProjectiles()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:221`.
- **PR wins** ``method:Step`0()``; PR `public void Step()` at `runtime/Engine/WeaponHost.cs:78`; develop `public void Step()` at `runtime/Engine/WeaponHost.cs:78`.
- **PR wins** ``method:StrikeFiringPin`0()``; PR `public void StrikeFiringPin()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:44`; develop `public void StrikeFiringPin()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:44`.
- **PR wins** ``method:UpdateTriggerMechanism`0()``; PR `private void UpdateTriggerMechanism()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:8`; develop `private void UpdateTriggerMechanism()` at `runtime/Original/Weapons/WeaponMechanism.Methods.cs:8`.
- **PR wins** `property:ActiveFireMode`; PR `public FireMode ActiveFireMode` at `runtime/Engine/WeaponHost.cs:75`; develop `public FireMode ActiveFireMode` at `runtime/Engine/WeaponHost.cs:75`.
- **PR wins** `property:Magazine`; PR `public Magazine Magazine` at `runtime/Engine/WeaponHost.cs:72`; develop `public Magazine Magazine` at `runtime/Engine/WeaponHost.cs:72`.
- **PR wins** `property:RecoilClimb`; PR `public float RecoilClimb` at `runtime/Engine/WeaponHost.cs:74`; develop `public float RecoilClimb` at `runtime/Engine/WeaponHost.cs:74`.
- **PR wins** `property:RecoilKick`; PR `public float RecoilKick` at `runtime/Engine/WeaponHost.cs:74`; develop `public float RecoilKick` at `runtime/Engine/WeaponHost.cs:74`.
- **PR wins** `property:TotalWeight`; PR `public float TotalWeight` at `runtime/Engine/WeaponHost.cs:74`; develop `public float TotalWeight` at `runtime/Engine/WeaponHost.cs:74`.


### 86. `Crisis.Runtime.Weapons.WeaponBase+BoltReleaseAnimation` — PR type wins

**PR declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `BoltReleaseAnimation`; partial `false`; `runtime/Engine/WeaponHost.cs:63`

**develop declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `BoltReleaseAnimation`; partial `false`; `runtime/Engine/WeaponHost.cs:63`

Common member signatures: **0**.


### 87. `Crisis.Runtime.Weapons.WeaponBase+ChamberState` — PR type wins

**PR declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `ChamberState`; partial `false`; `runtime/Engine/WeaponHost.cs:63`

**develop declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `ChamberState`; partial `false`; `runtime/Engine/WeaponHost.cs:63`

Common member signatures: **0**.


### 88. `Crisis.Runtime.Weapons.WeaponBase+FireMode` — PR type wins

**PR declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `FireMode`; partial `false`; `runtime/Engine/WeaponHost.cs:63`

**develop declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `FireMode`; partial `false`; `runtime/Engine/WeaponHost.cs:63`

Common member signatures: **0**.


### 89. `Crisis.Runtime.Weapons.WeaponComponentMagazine` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponComponentMagazine`; partial `false`; `runtime/Engine/WeaponHost.cs:26`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponComponentMagazine`; partial `false`; `runtime/Engine/WeaponHost.cs:26`

Common member signatures: **6**.
- **PR wins** `field:Effects`; PR `public field WeaponEffects Effects` at `runtime/Engine/WeaponHost.cs:27`; develop `public field WeaponEffects Effects` at `runtime/Engine/WeaponHost.cs:27`.
- **PR wins** `field:capacity`; PR `public field int capacity` at `runtime/Engine/WeaponHost.cs:27`; develop `public field int capacity` at `runtime/Engine/WeaponHost.cs:27`.
- **PR wins** `field:ikTarget`; PR `public field Transform ikTarget` at `runtime/Engine/WeaponHost.cs:27`; develop `public field Transform ikTarget` at `runtime/Engine/WeaponHost.cs:27`.
- **PR wins** `field:malfunctionRate`; PR `public field float malfunctionRate` at `runtime/Engine/WeaponHost.cs:27`; develop `public field float malfunctionRate` at `runtime/Engine/WeaponHost.cs:27`.
- **PR wins** `field:weight`; PR `public field float weight` at `runtime/Engine/WeaponHost.cs:27`; develop `public field float weight` at `runtime/Engine/WeaponHost.cs:27`.
- **PR wins** ``method:Drop`0()``; PR `public void Drop()` at `runtime/Engine/WeaponHost.cs:28`; develop `public void Drop()` at `runtime/Engine/WeaponHost.cs:28`.


### 90. `Crisis.Runtime.Weapons.WeaponEffects` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponEffects`; partial `false`; `runtime/Engine/WeaponHost.cs:11`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponEffects`; partial `false`; `runtime/Engine/WeaponHost.cs:11`

Common member signatures: **2**.
- **PR wins** `field:events`; PR `public readonly field List<object> events` at `runtime/Engine/WeaponHost.cs:12`; develop `public readonly field List<object> events` at `runtime/Engine/WeaponHost.cs:12`.
- **PR wins** ``method:Emit`0(string, object)``; PR `public void Emit(string, object)` at `runtime/Engine/WeaponHost.cs:13`; develop `public void Emit(string, object)` at `runtime/Engine/WeaponHost.cs:13`.


### 91. `Crisis.Runtime.Weapons.WeaponHost` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponHost`; partial `false`; `runtime/Engine/WeaponHost.cs:89`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponHost`; partial `false`; `runtime/Engine/WeaponHost.cs:89`

Common member signatures: **29**.
- **PR wins** `constructor:WeaponHost(string, InputHandler, RuntimeWorld)`; PR `public Crisis.Runtime.Weapons.WeaponHost(string, InputHandler, RuntimeWorld)` at `runtime/Engine/WeaponHost.cs:104`; develop `public Crisis.Runtime.Weapons.WeaponHost(string, InputHandler, RuntimeWorld)` at `runtime/Engine/WeaponHost.cs:110`.
- **PR wins** `field:armPhysics`; PR `readonly field OriginalArmPhysicsHost armPhysics` at `runtime/Engine/WeaponHost.cs:92`; develop `readonly field OriginalArmPhysicsHost armPhysics` at `runtime/Engine/WeaponHost.cs:92`.
- **PR wins** `field:ballisticMask`; PR `public field int ballisticMask` at `runtime/Engine/WeaponHost.cs:90`; develop `public field int ballisticMask` at `runtime/Engine/WeaponHost.cs:90`.
- **PR wins** `field:body`; PR `readonly field CharacterBody body` at `runtime/Engine/WeaponHost.cs:91`; develop `readonly field CharacterBody body` at `runtime/Engine/WeaponHost.cs:91`.
- **PR wins** `field:config`; PR `field JsonElement config` at `runtime/Engine/WeaponHost.cs:90`; develop `field JsonElement config` at `runtime/Engine/WeaponHost.cs:90`.
- **PR wins** `field:effects`; PR `public readonly field WeaponEffects effects` at `runtime/Engine/WeaponHost.cs:94`; develop `public readonly field WeaponEffects effects` at `runtime/Engine/WeaponHost.cs:94`.
- **PR wins** `field:magazineTransforms`; PR `readonly field Dictionary<string,Transform> magazineTransforms` at `runtime/Engine/WeaponHost.cs:93`; develop `readonly field Dictionary<string,Transform> magazineTransforms` at `runtime/Engine/WeaponHost.cs:93`.
- **PR wins** `field:operatorTransforms`; PR `readonly field Dictionary<string,Transform> operatorTransforms` at `runtime/Engine/WeaponHost.cs:93`; develop `readonly field Dictionary<string,Transform> operatorTransforms` at `runtime/Engine/WeaponHost.cs:93`.
- **PR wins** `field:projectiles`; PR `readonly field List<global::Projectile> projectiles` at `runtime/Engine/WeaponHost.cs:90`; develop `readonly field List<global::Projectile> projectiles` at `runtime/Engine/WeaponHost.cs:90`.
- **PR wins** `field:queryWorld`; PR `public field RuntimeWorld queryWorld` at `runtime/Engine/WeaponHost.cs:90`; develop `public field RuntimeWorld queryWorld` at `runtime/Engine/WeaponHost.cs:90`.
- **PR wins** `field:reload`; PR `readonly field ChangeMagazine reload` at `runtime/Engine/WeaponHost.cs:91`; develop `readonly field ChangeMagazine reload` at `runtime/Engine/WeaponHost.cs:91`.
- **PR wins** `field:task`; PR `field WeaponTask task` at `runtime/Engine/WeaponHost.cs:91`; develop `field WeaponTask task` at `runtime/Engine/WeaponHost.cs:91`.
- **PR wins** `field:weapon`; PR `readonly field WeaponBase weapon` at `runtime/Engine/WeaponHost.cs:91`; develop `readonly field WeaponBase weapon` at `runtime/Engine/WeaponHost.cs:91`.
- **PR wins** `field:weaponTransforms`; PR `readonly field Dictionary<string,Transform> weaponTransforms` at `runtime/Engine/WeaponHost.cs:93`; develop `readonly field Dictionary<string,Transform> weaponTransforms` at `runtime/Engine/WeaponHost.cs:93`.
- **PR wins** `field:workBolt`; PR `readonly field WorkBolt workBolt` at `runtime/Engine/WeaponHost.cs:91`; develop `readonly field WorkBolt workBolt` at `runtime/Engine/WeaponHost.cs:91`.
- **PR wins** `field:world`; PR `readonly field RuntimeWorld world` at `runtime/Engine/WeaponHost.cs:91`; develop `readonly field RuntimeWorld world` at `runtime/Engine/WeaponHost.cs:91`.
- **PR wins** ``method:Curve`0(JsonElement)``; PR `public static AnimationCurve Curve(JsonElement)` at `runtime/Engine/WeaponHost.cs:98`; develop `public static AnimationCurve Curve(JsonElement)` at `runtime/Engine/WeaponHost.cs:99`.
- **PR wins** ``method:Hierarchy`0(JsonElement, Dictionary<string,Transform>)``; PR `void Hierarchy(JsonElement, Dictionary<string,Transform>)` at `runtime/Engine/WeaponHost.cs:100`; develop `void Hierarchy(JsonElement, Dictionary<string,Transform>)` at `runtime/Engine/WeaponHost.cs:101`.
- **PR wins** ``method:InitializeBallistics`0(string, SceneLoadResult)``; PR `public void InitializeBallistics(string, SceneLoadResult)` at `runtime/Engine/WeaponHost.cs:103`; develop `public void InitializeBallistics(string, SceneLoadResult)` at `runtime/Engine/WeaponHost.cs:104`.
- **PR wins** ``method:Make`0(string)``; PR `Transform Make(string)` at `runtime/Engine/WeaponHost.cs:99`; develop `Transform Make(string)` at `runtime/Engine/WeaponHost.cs:100`.
- **PR wins** ``method:SetWorld`0(Transform, JsonElement)``; PR `static void SetWorld(Transform, JsonElement)` at `runtime/Engine/WeaponHost.cs:180`; develop `static void SetWorld(Transform, JsonElement)` at `runtime/Engine/WeaponHost.cs:192`.
- **PR wins** ``method:Snapshot`0()``; PR `public object Snapshot()` at `runtime/Engine/WeaponHost.cs:181`; develop `public object Snapshot()` at `runtime/Engine/WeaponHost.cs:193`.
- **PR wins** ``method:Step`0(string)``; PR `public object Step(string)` at `runtime/Engine/WeaponHost.cs:164`; develop `public object Step(string)` at `runtime/Engine/WeaponHost.cs:174`.
- **PR wins** ``method:V`0(JsonElement)``; PR `public static Vector3 V(JsonElement)` at `runtime/Engine/WeaponHost.cs:96`; develop `public static Vector3 V(JsonElement)` at `runtime/Engine/WeaponHost.cs:97`.
- **PR wins** ``method:Vec`0(JsonElement)``; PR `static Vector3 Vec(JsonElement)` at `runtime/Engine/WeaponHost.cs:97`; develop `static Vector3 Vec(JsonElement)` at `runtime/Engine/WeaponHost.cs:98`.
- **PR wins** `property:Ballistics`; PR `public BallisticRuntimeHost Ballistics` at `runtime/Engine/WeaponHost.cs:102`; develop `public BallisticRuntimeHost Ballistics` at `runtime/Engine/WeaponHost.cs:103`.
- **PR wins** `property:Climb`; PR `public float Climb` at `runtime/Engine/WeaponHost.cs:95`; develop `public float Climb` at `runtime/Engine/WeaponHost.cs:95`.
- **PR wins** `property:Kick`; PR `public float Kick` at `runtime/Engine/WeaponHost.cs:95`; develop `public float Kick` at `runtime/Engine/WeaponHost.cs:95`.
- **PR wins** `property:Manipulating`; PR `public bool Manipulating` at `runtime/Engine/WeaponHost.cs:95`; develop `public bool Manipulating` at `runtime/Engine/WeaponHost.cs:95`.

Develop-only member signatures: **2** — **DEVELOP-ONLY / DEFERRED**.
- **DEVELOP-ONLY — DEFERRED** ``method:BindOriginalLimbPhysiology`0()`` — `public void BindOriginalLimbPhysiology()` at `runtime/Engine/WeaponHost.cs:173`.
- **DEVELOP-ONLY — DEFERRED** `property:Character` — `public CharacterBody Character` at `runtime/Engine/WeaponHost.cs:96`.


### 92. `Crisis.Runtime.Weapons.WeaponTask` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponTask`; partial `false`; `runtime/Engine/WeaponHost.cs:55`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WeaponTask`; partial `false`; `runtime/Engine/WeaponHost.cs:55`

Common member signatures: **4**.
- **PR wins** ``method:OnAwake`0()``; PR `public virtual void OnAwake()` at `runtime/Engine/WeaponHost.cs:56`; develop `public virtual void OnAwake()` at `runtime/Engine/WeaponHost.cs:56`.
- **PR wins** ``method:OnEnd`0()``; PR `public virtual void OnEnd()` at `runtime/Engine/WeaponHost.cs:56`; develop `public virtual void OnEnd()` at `runtime/Engine/WeaponHost.cs:56`.
- **PR wins** ``method:OnStart`0()``; PR `public virtual void OnStart()` at `runtime/Engine/WeaponHost.cs:56`; develop `public virtual void OnStart()` at `runtime/Engine/WeaponHost.cs:56`.
- **PR wins** ``method:OnUpdate`0()``; PR `public abstract TaskStatus OnUpdate()` at `runtime/Engine/WeaponHost.cs:56`; develop `public abstract TaskStatus OnUpdate()` at `runtime/Engine/WeaponHost.cs:56`.


### 93. `Crisis.Runtime.Weapons.WorkBolt` — PR type wins

**PR declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WorkBolt`; partial `false`; `runtime/Original/Weapons/WorkBolt.cs:10`

**develop declarations**:
- class; namespace `Crisis.Runtime.Weapons`; `WorkBolt`; partial `false`; `runtime/Original/Weapons/WorkBolt.cs:10`

Common member signatures: **12**.
- **PR wins** `field:action`; PR `private field Action action` at `runtime/Original/Weapons/WorkBolt.cs:24`; develop `private field Action action` at `runtime/Original/Weapons/WorkBolt.cs:24`.
- **PR wins** `field:arm`; PR `private field ArmIKSolver arm` at `runtime/Original/Weapons/WorkBolt.cs:26`; develop `private field ArmIKSolver arm` at `runtime/Original/Weapons/WorkBolt.cs:26`.
- **PR wins** `field:bolt`; PR `private field CyclingBolt bolt` at `runtime/Original/Weapons/WorkBolt.cs:22`; develop `private field CyclingBolt bolt` at `runtime/Original/Weapons/WorkBolt.cs:22`.
- **PR wins** `field:manipulationStarted`; PR `private field bool manipulationStarted` at `runtime/Original/Weapons/WorkBolt.cs:28`; develop `private field bool manipulationStarted` at `runtime/Original/Weapons/WorkBolt.cs:28`.
- **PR wins** `field:self`; PR `private field CharacterBody self` at `runtime/Original/Weapons/WorkBolt.cs:18`; develop `private field CharacterBody self` at `runtime/Original/Weapons/WorkBolt.cs:18`.
- **PR wins** `field:skillDelay`; PR `private field float skillDelay` at `runtime/Original/Weapons/WorkBolt.cs:32`; develop `private field float skillDelay` at `runtime/Original/Weapons/WorkBolt.cs:32`.
- **PR wins** `field:waitTimestamp`; PR `private field float waitTimestamp` at `runtime/Original/Weapons/WorkBolt.cs:30`; develop `private field float waitTimestamp` at `runtime/Original/Weapons/WorkBolt.cs:30`.
- **PR wins** `field:weapon`; PR `private field WeaponBase weapon` at `runtime/Original/Weapons/WorkBolt.cs:20`; develop `private field WeaponBase weapon` at `runtime/Original/Weapons/WorkBolt.cs:20`.
- **PR wins** ``method:OnAwake`0()``; PR `public override void OnAwake()` at `runtime/Original/Weapons/WorkBolt.cs:34`; develop `public override void OnAwake()` at `runtime/Original/Weapons/WorkBolt.cs:34`.
- **PR wins** ``method:OnEnd`0()``; PR `public override void OnEnd()` at `runtime/Original/Weapons/WorkBolt.cs:77`; develop `public override void OnEnd()` at `runtime/Original/Weapons/WorkBolt.cs:77`.
- **PR wins** ``method:OnStart`0()``; PR `public override void OnStart()` at `runtime/Original/Weapons/WorkBolt.cs:40`; develop `public override void OnStart()` at `runtime/Original/Weapons/WorkBolt.cs:40`.
- **PR wins** ``method:OnUpdate`0()``; PR `public override TaskStatus OnUpdate()` at `runtime/Original/Weapons/WorkBolt.cs:85`; develop `public override TaskStatus OnUpdate()` at `runtime/Original/Weapons/WorkBolt.cs:85`.


### 94. `Crisis.Runtime.Weapons.WorkBolt+Action` — PR type wins

**PR declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `Action`; partial `false`; `runtime/Original/Weapons/WorkBolt.cs:12`

**develop declarations**:
- enum; namespace `Crisis.Runtime.Weapons`; `Action`; partial `false`; `runtime/Original/Weapons/WorkBolt.cs:12`

Common member signatures: **0**.


### 95. `DecayingObject` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `DecayingObject`; partial `false`; `runtime/Original/DecayingObject.cs:4`

**develop declarations**:
- class; namespace `(global)`; `DecayingObject`; partial `false`; `runtime/Original/DecayingObject.cs:4`

Common member signatures: **7**.
- **PR wins** `field:activateOnStart`; PR `public field bool activateOnStart` at `runtime/Original/DecayingObject.cs:9`; develop `public field bool activateOnStart` at `runtime/Original/DecayingObject.cs:9`.
- **PR wins** `field:active`; PR `private field bool active` at `runtime/Original/DecayingObject.cs:12`; develop `private field bool active` at `runtime/Original/DecayingObject.cs:12`.
- **PR wins** `field:lifeTime`; PR `public field float lifeTime` at `runtime/Original/DecayingObject.cs:6`; develop `public field float lifeTime` at `runtime/Original/DecayingObject.cs:6`.
- **PR wins** `field:startTime`; PR `private field float startTime` at `runtime/Original/DecayingObject.cs:14`; develop `private field float startTime` at `runtime/Original/DecayingObject.cs:14`.
- **PR wins** ``method:Activate`0()``; PR `public void Activate()` at `runtime/Original/DecayingObject.cs:24`; develop `public void Activate()` at `runtime/Original/DecayingObject.cs:24`.
- **PR wins** ``method:Start`0()``; PR `private void Start()` at `runtime/Original/DecayingObject.cs:16`; develop `private void Start()` at `runtime/Original/DecayingObject.cs:16`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `runtime/Original/DecayingObject.cs:30`; develop `private void Update()` at `runtime/Original/DecayingObject.cs:30`.


### 96. `DelayedBallisticExit` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `DelayedBallisticExit`; partial `false`; `runtime/Original/Ballistics/DelayedBallisticExit.cs:7`

**develop declarations**:
- class; namespace `(global)`; `DelayedBallisticExit`; partial `false`; `runtime/Original/Ballistics/DelayedBallisticExit.cs:7`

Common member signatures: **3**.
- **PR wins** `field:activationTime`; PR `public field float activationTime` at `runtime/Original/Ballistics/DelayedBallisticExit.cs:9`; develop `public field float activationTime` at `runtime/Original/Ballistics/DelayedBallisticExit.cs:9`.
- **PR wins** `field:localResumePosition`; PR `public field Vector3 localResumePosition` at `runtime/Original/Ballistics/DelayedBallisticExit.cs:13`; develop `public field Vector3 localResumePosition` at `runtime/Original/Ballistics/DelayedBallisticExit.cs:13`.
- **PR wins** `field:projectile`; PR `public field Projectile projectile` at `runtime/Original/Ballistics/DelayedBallisticExit.cs:11`; develop `public field Projectile projectile` at `runtime/Original/Ballistics/DelayedBallisticExit.cs:11`.


### 97. `Hand` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `Hand`; partial `false`; `runtime/Original/Animation/Hand.cs:3`

**develop declarations**:
- class; namespace `(global)`; `Hand`; partial `false`; `runtime/Original/Animation/Hand.cs:3`

Common member signatures: **6**.
- **PR wins** `field:fist`; PR `public field GameObject fist` at `runtime/Original/Animation/Hand.cs:14`; develop `public field GameObject fist` at `runtime/Original/Animation/Hand.cs:14`.
- **PR wins** `field:forwardAssist`; PR `public field GameObject forwardAssist` at `runtime/Original/Animation/Hand.cs:18`; develop `public field GameObject forwardAssist` at `runtime/Original/Animation/Hand.cs:18`.
- **PR wins** `field:grabPoint`; PR `public field Transform grabPoint` at `runtime/Original/Animation/Hand.cs:12`; develop `public field Transform grabPoint` at `runtime/Original/Animation/Hand.cs:12`.
- **PR wins** `field:trigger`; PR `public field GameObject trigger` at `runtime/Original/Animation/Hand.cs:16`; develop `public field GameObject trigger` at `runtime/Original/Animation/Hand.cs:16`.
- **PR wins** ``method:SetMode`0(Mode)``; PR `public void SetMode(Mode)` at `runtime/Original/Animation/Hand.cs:22`; develop `public void SetMode(Mode)` at `runtime/Original/Animation/Hand.cs:22`.
- **PR wins** `property:CurrentMode`; PR `public Mode CurrentMode` at `runtime/Original/Animation/Hand.cs:20`; develop `public Mode CurrentMode` at `runtime/Original/Animation/Hand.cs:20`.


### 98. `Hand+Mode` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `Mode`; partial `false`; `runtime/Original/Animation/Hand.cs:5`

**develop declarations**:
- enum; namespace `(global)`; `Mode`; partial `false`; `runtime/Original/Animation/Hand.cs:5`

Common member signatures: **0**.


### 99. `IKArmAction` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `IKArmAction`; partial `false`; `runtime/Original/Animation/IKArmAction.cs:1`

**develop declarations**:
- enum; namespace `(global)`; `IKArmAction`; partial `false`; `runtime/Original/Animation/IKArmAction.cs:1`

Common member signatures: **0**.


### 100. `IKArmAnimationFrame` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `IKArmAnimationFrame`; partial `false`; `runtime/Original/Animation/IKArmAnimationFrame.cs:4`

**develop declarations**:
- class; namespace `(global)`; `IKArmAnimationFrame`; partial `false`; `runtime/Original/Animation/IKArmAnimationFrame.cs:4`

Common member signatures: **7**.
- **PR wins** `field:action`; PR `public field IKArmAction action` at `runtime/Original/Animation/IKArmAnimationFrame.cs:9`; develop `public field IKArmAction action` at `runtime/Original/Animation/IKArmAnimationFrame.cs:9`.
- **PR wins** `field:callback`; PR `public field IKControllerInvoke callback` at `runtime/Original/Animation/IKArmAnimationFrame.cs:17`; develop `public field IKControllerInvoke callback` at `runtime/Original/Animation/IKArmAnimationFrame.cs:17`.
- **PR wins** `field:name`; PR `public field string name` at `runtime/Original/Animation/IKArmAnimationFrame.cs:7`; develop `public field string name` at `runtime/Original/Animation/IKArmAnimationFrame.cs:7`.
- **PR wins** `field:offset`; PR `public field Vector3 offset` at `runtime/Original/Animation/IKArmAnimationFrame.cs:13`; develop `public field Vector3 offset` at `runtime/Original/Animation/IKArmAnimationFrame.cs:13`.
- **PR wins** `field:time`; PR `public field float time` at `runtime/Original/Animation/IKArmAnimationFrame.cs:15`; develop `public field float time` at `runtime/Original/Animation/IKArmAnimationFrame.cs:15`.
- **PR wins** `field:transform`; PR `public field IKArmTarget transform` at `runtime/Original/Animation/IKArmAnimationFrame.cs:11`; develop `public field IKArmTarget transform` at `runtime/Original/Animation/IKArmAnimationFrame.cs:11`.
- **PR wins** `field:waitForSiblingFrame`; PR `public field int waitForSiblingFrame` at `runtime/Original/Animation/IKArmAnimationFrame.cs:19`; develop `public field int waitForSiblingFrame` at `runtime/Original/Animation/IKArmAnimationFrame.cs:19`.

