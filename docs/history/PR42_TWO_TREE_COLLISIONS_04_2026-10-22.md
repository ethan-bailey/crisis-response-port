# PR #42 collision list — part 4 of 4

Checkpoint: PR `a653609b62f3e3c79f73d0a17f0bf3dc89eae7b8`; develop `2fc0a8e93be97c4d99123910fad4bd25798e71a8`. PR side wins each collision.

### 151. `UnityEngine.Events.UnityEvent` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine.Events`; `UnityEvent`; partial `false`; `runtime/Engine/UnityEventTyped.cs:5`
- class; namespace `UnityEngine.Events`; `UnityEvent`; partial `false`; `runtime/Engine/UnityEventTyped.cs:16`
- class; namespace `UnityEngine.Events`; `UnityEvent`; partial `false`; `runtime/Engine/WeaponHost.cs:6`

**develop declarations**:
- class; namespace `UnityEngine.Events`; `UnityEvent`; partial `false`; `runtime/Engine/WeaponHost.cs:6`

Common member signatures: **3**.
- **PR wins** `event:callbacks`; PR `event UnityAction callbacks` at `runtime/Engine/WeaponHost.cs:6`; develop `event UnityAction callbacks` at `runtime/Engine/WeaponHost.cs:6`.
- **PR wins** ``method:AddListener`0(UnityAction)``; PR `public void AddListener(UnityAction)` at `runtime/Engine/WeaponHost.cs:6`; develop `public void AddListener(UnityAction)` at `runtime/Engine/WeaponHost.cs:6`.
- **PR wins** ``method:Invoke`0()``; PR `public void Invoke()` at `runtime/Engine/WeaponHost.cs:6`; develop `public void Invoke()` at `runtime/Engine/WeaponHost.cs:6`.


### 152. `UnityEngine.Events.UnityEventBool` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine.Events`; `UnityEventBool`; partial `false`; `runtime/Engine/WeaponHost.cs:7`

**develop declarations**:
- class; namespace `UnityEngine.Events`; `UnityEventBool`; partial `false`; `runtime/Engine/WeaponHost.cs:7`

Common member signatures: **3**.
- **PR wins** `event:callbacks`; PR `event UnityAction<bool> callbacks` at `runtime/Engine/WeaponHost.cs:7`; develop `event UnityAction<bool> callbacks` at `runtime/Engine/WeaponHost.cs:7`.
- **PR wins** ``method:AddListener`0(UnityAction<bool>)``; PR `public void AddListener(UnityAction<bool>)` at `runtime/Engine/WeaponHost.cs:7`; develop `public void AddListener(UnityAction<bool>)` at `runtime/Engine/WeaponHost.cs:7`.
- **PR wins** ``method:Invoke`0(bool)``; PR `public void Invoke(bool)` at `runtime/Engine/WeaponHost.cs:7`; develop `public void Invoke(bool)` at `runtime/Engine/WeaponHost.cs:7`.


### 153. `UnityEngine.ForceMode` — PR type wins

**PR declarations**:
- enum; namespace `UnityEngine`; `ForceMode`; partial `false`; `runtime/Engine/ArmPhysicsComponents.cs:7`

**develop declarations**:
- enum; namespace `UnityEngine`; `ForceMode`; partial `false`; `runtime/Engine/ArmPhysicsComponents.cs:7`

Common member signatures: **0**.


### 154. `UnityEngine.GameObject` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `GameObject`; partial `false`; `runtime/Engine/Objects.cs:41`

**develop declarations**:
- class; namespace `UnityEngine`; `GameObject`; partial `false`; `runtime/Engine/Objects.cs:29`

Common member signatures: **18**.
- **PR wins** `constructor:GameObject(string, RuntimeWorld, bool)`; PR `public UnityEngine.GameObject(string, RuntimeWorld, bool)` at `runtime/Engine/Objects.cs:52`; develop `public UnityEngine.GameObject(string, RuntimeWorld, bool)` at `runtime/Engine/Objects.cs:39`.
- **PR wins** `field:active`; PR `field bool active` at `runtime/Engine/Objects.cs:43`; develop `field bool active` at `runtime/Engine/Objects.cs:31`.
- **PR wins** `field:assetObject`; PR `internal field bool assetObject` at `runtime/Engine/Objects.cs:44`; develop `internal field bool assetObject` at `runtime/Engine/Objects.cs:32`.
- **PR wins** `field:components`; PR `internal readonly field List<Component> components` at `runtime/Engine/Objects.cs:42`; develop `internal readonly field List<Component> components` at `runtime/Engine/Objects.cs:30`.
- **PR wins** ``method:AddComponent`0(Type)``; PR `public Component AddComponent(Type)` at `runtime/Engine/Objects.cs:73`; develop `public Component AddComponent(Type)` at `runtime/Engine/Objects.cs:45`.
- **PR wins** ``method:AddComponent`1()``; PR `public T AddComponent<T>()` at `runtime/Engine/Objects.cs:72`; develop `public T AddComponent<T>()` at `runtime/Engine/Objects.cs:44`.
- **PR wins** ``method:Attach`0(Component)``; PR `internal void Attach(Component)` at `runtime/Engine/Objects.cs:71`; develop `internal void Attach(Component)` at `runtime/Engine/Objects.cs:43`.
- **PR wins** ``method:GetComponent`0(Type)``; PR `public Component GetComponent(Type)` at `runtime/Engine/Objects.cs:81`; develop `public Component GetComponent(Type)` at `runtime/Engine/Objects.cs:53`.
- **PR wins** ``method:GetComponent`1()``; PR `public T GetComponent<T>()` at `runtime/Engine/Objects.cs:80`; develop `public T GetComponent<T>()` at `runtime/Engine/Objects.cs:52`.
- **PR wins** ``method:GetComponentsInChildren`1(bool)``; PR `public T[] GetComponentsInChildren<T>(bool)` at `runtime/Engine/Objects.cs:83`; develop `public T[] GetComponentsInChildren<T>(bool)` at `runtime/Engine/Objects.cs:55`.
- **PR wins** ``method:GetComponents`1()``; PR `public T[] GetComponents<T>()` at `runtime/Engine/Objects.cs:82`; develop `public T[] GetComponents<T>()` at `runtime/Engine/Objects.cs:54`.
- **PR wins** ``method:SetActive`0(bool)``; PR `public void SetActive(bool)` at `runtime/Engine/Objects.cs:88`; develop `public void SetActive(bool)` at `runtime/Engine/Objects.cs:60`.
- **PR wins** `property:activeInHierarchy`; PR `public bool activeInHierarchy` at `runtime/Engine/Objects.cs:51`; develop `public bool activeInHierarchy` at `runtime/Engine/Objects.cs:38`.
- **PR wins** `property:activeSelf`; PR `public bool activeSelf` at `runtime/Engine/Objects.cs:50`; develop `public bool activeSelf` at `runtime/Engine/Objects.cs:37`.
- **PR wins** `property:isStatic`; PR `public bool isStatic` at `runtime/Engine/Objects.cs:46`; develop `public bool isStatic` at `runtime/Engine/Objects.cs:34`.
- **PR wins** `property:layer`; PR `public int layer` at `runtime/Engine/Objects.cs:45`; develop `public int layer` at `runtime/Engine/Objects.cs:33`.
- **PR wins** `property:tag`; PR `public string tag` at `runtime/Engine/Objects.cs:47`; develop `public string tag` at `runtime/Engine/Objects.cs:35`.
- **PR wins** `property:transform`; PR `public Transform transform` at `runtime/Engine/Objects.cs:48`; develop `public Transform transform` at `runtime/Engine/Objects.cs:36`.


### 155. `UnityEngine.HeaderAttribute` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `HeaderAttribute`; partial `false`; `runtime/Engine/Math.cs:125`

**develop declarations**:
- class; namespace `UnityEngine`; `HeaderAttribute`; partial `false`; `runtime/Engine/Math.cs:71`

Common member signatures: **0**.


### 156. `UnityEngine.HingeJoint` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `HingeJoint`; partial `false`; `runtime/Engine/ArmPhysicsComponents.cs:24`

**develop declarations**:
- class; namespace `UnityEngine`; `HingeJoint`; partial `false`; `runtime/Engine/ArmPhysicsComponents.cs:11`

Common member signatures: **6**.
- **PR wins** `field:SpringChanged`; PR `public field Action<JointSpring,bool> SpringChanged` at `runtime/Engine/ArmPhysicsComponents.cs:32`; develop `public field Action<JointSpring,bool> SpringChanged` at `runtime/Engine/ArmPhysicsComponents.cs:14`.
- **PR wins** `field:currentSpring`; PR `field JointSpring currentSpring` at `runtime/Engine/ArmPhysicsComponents.cs:29`; develop `field JointSpring currentSpring` at `runtime/Engine/ArmPhysicsComponents.cs:13`.
- **PR wins** `field:springEnabled`; PR `field bool springEnabled` at `runtime/Engine/ArmPhysicsComponents.cs:28`; develop `field bool springEnabled` at `runtime/Engine/ArmPhysicsComponents.cs:12`.
- **PR wins** `property:angle`; PR `public float angle` at `runtime/Engine/ArmPhysicsComponents.cs:51`; develop `public float angle` at `runtime/Engine/ArmPhysicsComponents.cs:24`.
- **PR wins** `property:spring`; PR `public JointSpring spring` at `runtime/Engine/ArmPhysicsComponents.cs:46`; develop `public JointSpring spring` at `runtime/Engine/ArmPhysicsComponents.cs:19`.
- **PR wins** `property:useSpring`; PR `public bool useSpring` at `runtime/Engine/ArmPhysicsComponents.cs:42`; develop `public bool useSpring` at `runtime/Engine/ArmPhysicsComponents.cs:15`.


### 157. `UnityEngine.Input` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Input`; partial `false`; `runtime/Engine/ClockInput.cs:26`

**develop declarations**:
- class; namespace `UnityEngine`; `Input`; partial `false`; `runtime/Engine/ClockInput.cs:26`

Common member signatures: **17**.
- **PR wins** `field:axes`; PR `static readonly field Dictionary<string,float> axes` at `runtime/Engine/ClockInput.cs:28`; develop `static readonly field Dictionary<string,float> axes` at `runtime/Engine/ClockInput.cs:28`.
- **PR wins** `field:down`; PR `static readonly field HashSet<KeyCode> down` at `runtime/Engine/ClockInput.cs:27`; develop `static readonly field HashSet<KeyCode> down` at `runtime/Engine/ClockInput.cs:27`.
- **PR wins** `field:held`; PR `static readonly field HashSet<KeyCode> held` at `runtime/Engine/ClockInput.cs:27`; develop `static readonly field HashSet<KeyCode> held` at `runtime/Engine/ClockInput.cs:27`.
- **PR wins** `field:previous`; PR `static readonly field HashSet<KeyCode> previous` at `runtime/Engine/ClockInput.cs:27`; develop `static readonly field HashSet<KeyCode> previous` at `runtime/Engine/ClockInput.cs:27`.
- **PR wins** `field:up`; PR `static readonly field HashSet<KeyCode> up` at `runtime/Engine/ClockInput.cs:27`; develop `static readonly field HashSet<KeyCode> up` at `runtime/Engine/ClockInput.cs:27`.
- **PR wins** ``method:BeginFrame`0()``; PR `internal static void BeginFrame()` at `runtime/Engine/ClockInput.cs:39`; develop `internal static void BeginFrame()` at `runtime/Engine/ClockInput.cs:39`.
- **PR wins** ``method:EndFrame`0()``; PR `internal static void EndFrame()` at `runtime/Engine/ClockInput.cs:40`; develop `internal static void EndFrame()` at `runtime/Engine/ClockInput.cs:40`.
- **PR wins** ``method:GetAxis`0(string)``; PR `public static float GetAxis(string)` at `runtime/Engine/ClockInput.cs:34`; develop `public static float GetAxis(string)` at `runtime/Engine/ClockInput.cs:34`.
- **PR wins** ``method:GetKeyDown`0(KeyCode)``; PR `public static bool GetKeyDown(KeyCode)` at `runtime/Engine/ClockInput.cs:32`; develop `public static bool GetKeyDown(KeyCode)` at `runtime/Engine/ClockInput.cs:32`.
- **PR wins** ``method:GetKeyUp`0(KeyCode)``; PR `public static bool GetKeyUp(KeyCode)` at `runtime/Engine/ClockInput.cs:33`; develop `public static bool GetKeyUp(KeyCode)` at `runtime/Engine/ClockInput.cs:33`.
- **PR wins** ``method:GetKey`0(KeyCode)``; PR `public static bool GetKey(KeyCode)` at `runtime/Engine/ClockInput.cs:31`; develop `public static bool GetKey(KeyCode)` at `runtime/Engine/ClockInput.cs:31`.
- **PR wins** ``method:ReleaseAll`0()``; PR `public static void ReleaseAll()` at `runtime/Engine/ClockInput.cs:37`; develop `public static void ReleaseAll()` at `runtime/Engine/ClockInput.cs:37`.
- **PR wins** ``method:Reset`0()``; PR `internal static void Reset()` at `runtime/Engine/ClockInput.cs:38`; develop `internal static void Reset()` at `runtime/Engine/ClockInput.cs:38`.
- **PR wins** ``method:SetAxis`0(string, float)``; PR `public static void SetAxis(string, float)` at `runtime/Engine/ClockInput.cs:36`; develop `public static void SetAxis(string, float)` at `runtime/Engine/ClockInput.cs:36`.
- **PR wins** ``method:SetKey`0(KeyCode, bool)``; PR `public static void SetKey(KeyCode, bool)` at `runtime/Engine/ClockInput.cs:35`; develop `public static void SetKey(KeyCode, bool)` at `runtime/Engine/ClockInput.cs:35`.
- **PR wins** `property:anyKeyDown`; PR `public static bool anyKeyDown` at `runtime/Engine/ClockInput.cs:30`; develop `public static bool anyKeyDown` at `runtime/Engine/ClockInput.cs:30`.
- **PR wins** `property:mousePosition`; PR `public static Vector3 mousePosition` at `runtime/Engine/ClockInput.cs:29`; develop `public static Vector3 mousePosition` at `runtime/Engine/ClockInput.cs:29`.


### 158. `UnityEngine.JointSpring` — PR type wins

**PR declarations**:
- struct; namespace `UnityEngine`; `JointSpring`; partial `false`; `runtime/Engine/ArmPhysicsComponents.cs:8`

**develop declarations**:
- struct; namespace `UnityEngine`; `JointSpring`; partial `false`; `runtime/Engine/ArmPhysicsComponents.cs:8`

Common member signatures: **3**.
- **PR wins** `field:damper`; PR `public field float damper` at `runtime/Engine/ArmPhysicsComponents.cs:9`; develop `public field float damper` at `runtime/Engine/ArmPhysicsComponents.cs:9`.
- **PR wins** `field:spring`; PR `public field float spring` at `runtime/Engine/ArmPhysicsComponents.cs:9`; develop `public field float spring` at `runtime/Engine/ArmPhysicsComponents.cs:9`.
- **PR wins** `field:targetPosition`; PR `public field float targetPosition` at `runtime/Engine/ArmPhysicsComponents.cs:9`; develop `public field float targetPosition` at `runtime/Engine/ArmPhysicsComponents.cs:9`.


### 159. `UnityEngine.KeyCode` — PR type wins

**PR declarations**:
- enum; namespace `UnityEngine`; `KeyCode`; partial `false`; `runtime/Original/KeyCode.cs:1`

**develop declarations**:
- enum; namespace `UnityEngine`; `KeyCode`; partial `false`; `runtime/Original/KeyCode.cs:1`

Common member signatures: **0**.


### 160. `UnityEngine.Keyframe` — PR type wins

**PR declarations**:
- struct; namespace `UnityEngine`; `Keyframe`; partial `false`; `runtime/Engine/AnimationCurve.cs:3`

**develop declarations**:
- struct; namespace `UnityEngine`; `Keyframe`; partial `false`; `runtime/Engine/AnimationCurve.cs:3`

Common member signatures: **4**.
- **PR wins** `field:inTangent`; PR `public field float inTangent` at `runtime/Engine/AnimationCurve.cs:4`; develop `public readonly field float inTangent` at `runtime/Engine/AnimationCurve.cs:4`.
- **PR wins** `field:outTangent`; PR `public field float outTangent` at `runtime/Engine/AnimationCurve.cs:4`; develop `public readonly field float outTangent` at `runtime/Engine/AnimationCurve.cs:4`.
- **PR wins** `field:time`; PR `public field float time` at `runtime/Engine/AnimationCurve.cs:4`; develop `public readonly field float time` at `runtime/Engine/AnimationCurve.cs:4`.
- **PR wins** `field:value`; PR `public field float value` at `runtime/Engine/AnimationCurve.cs:4`; develop `public readonly field float value` at `runtime/Engine/AnimationCurve.cs:4`.


### 161. `UnityEngine.Light` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Light`; partial `false`; `runtime/Engine/Objects.cs:223`

**develop declarations**:
- class; namespace `UnityEngine`; `Light`; partial `false`; `runtime/Engine/Objects.cs:127`

Common member signatures: **1**.
- **PR wins** `property:intensity`; PR `public float intensity` at `runtime/Engine/Objects.cs:226`; develop `public float intensity` at `runtime/Engine/Objects.cs:127`.


### 162. `UnityEngine.Material` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Material`; partial `false`; `runtime/Engine/Rendering/GraphicsPrimitives.cs:24`

**develop declarations**:
- class; namespace `UnityEngine`; `Material`; partial `false`; `runtime/Engine/BallisticEngine.cs:17`

Common member signatures: **3**.
- **PR wins** `field:OnMainTextureAssigned`; PR `public field Action<Texture2D> OnMainTextureAssigned` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:56`; develop `public field Action<Texture2D> OnMainTextureAssigned` at `runtime/Engine/BallisticEngine.cs:22`.
- **PR wins** `field:texture`; PR `field Texture2D texture` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:35`; develop `field Texture2D texture` at `runtime/Engine/BallisticEngine.cs:18`.
- **PR wins** `property:mainTexture`; PR `public Texture2D mainTexture` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:70`; develop `public Texture2D mainTexture` at `runtime/Engine/BallisticEngine.cs:23`.


### 163. `UnityEngine.Mathf` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Mathf`; partial `false`; `runtime/Engine/Math.cs:92`

**develop declarations**:
- class; namespace `UnityEngine`; `Mathf`; partial `false`; `runtime/Engine/Math.cs:51`

Common member signatures: **17**.
- **PR wins** ``method:Abs`0(float)``; PR `public static float Abs(float)` at `runtime/Engine/Math.cs:102`; develop `public static float Abs(float)` at `runtime/Engine/Math.cs:54`.
- **PR wins** ``method:Acos`0(float)``; PR `public static float Acos(float)` at `runtime/Engine/Math.cs:118`; develop `public static float Acos(float)` at `runtime/Engine/Math.cs:66`.
- **PR wins** ``method:Atan2`0(float, float)``; PR `public static float Atan2(float, float)` at `runtime/Engine/Math.cs:119`; develop `public static float Atan2(float, float)` at `runtime/Engine/Math.cs:67`.
- **PR wins** ``method:Atan`0(float)``; PR `public static float Atan(float)` at `runtime/Engine/Math.cs:120`; develop `public static float Atan(float)` at `runtime/Engine/Math.cs:68`.
- **PR wins** ``method:Clamp01`0(float)``; PR `public static float Clamp01(float)` at `runtime/Engine/Math.cs:109`; develop `public static float Clamp01(float)` at `runtime/Engine/Math.cs:58`.
- **PR wins** ``method:Clamp`0(float, float, float)``; PR `public static float Clamp(float, float, float)` at `runtime/Engine/Math.cs:107`; develop `public static float Clamp(float, float, float)` at `runtime/Engine/Math.cs:57`.
- **PR wins** ``method:Cos`0(float)``; PR `public static float Cos(float)` at `runtime/Engine/Math.cs:115`; develop `public static float Cos(float)` at `runtime/Engine/Math.cs:63`.
- **PR wins** ``method:FloorToInt`0(float)``; PR `public static int FloorToInt(float)` at `runtime/Engine/Math.cs:94`; develop `public static int FloorToInt(float)` at `runtime/Engine/Math.cs:52`.
- **PR wins** ``method:LerpAngle`0(float, float, float)``; PR `public static float LerpAngle(float, float, float)` at `runtime/Engine/Math.cs:116`; develop `public static float LerpAngle(float, float, float)` at `runtime/Engine/Math.cs:64`.
- **PR wins** ``method:Lerp`0(float, float, float)``; PR `public static float Lerp(float, float, float)` at `runtime/Engine/Math.cs:110`; develop `public static float Lerp(float, float, float)` at `runtime/Engine/Math.cs:59`.
- **PR wins** ``method:Max`0(float, float)``; PR `public static float Max(float, float)` at `runtime/Engine/Math.cs:105`; develop `public static float Max(float, float)` at `runtime/Engine/Math.cs:56`.
- **PR wins** ``method:Min`0(float, float)``; PR `public static float Min(float, float)` at `runtime/Engine/Math.cs:103`; develop `public static float Min(float, float)` at `runtime/Engine/Math.cs:55`.
- **PR wins** ``method:Pow`0(float, float)``; PR `public static float Pow(float, float)` at `runtime/Engine/Math.cs:112`; develop `public static float Pow(float, float)` at `runtime/Engine/Math.cs:60`.
- **PR wins** ``method:Sign`0(float)``; PR `public static float Sign(float)` at `runtime/Engine/Math.cs:101`; develop `public static float Sign(float)` at `runtime/Engine/Math.cs:53`.
- **PR wins** ``method:Sin`0(float)``; PR `public static float Sin(float)` at `runtime/Engine/Math.cs:117`; develop `public static float Sin(float)` at `runtime/Engine/Math.cs:65`.
- **PR wins** ``method:Sqrt`0(float)``; PR `public static float Sqrt(float)` at `runtime/Engine/Math.cs:113`; develop `public static float Sqrt(float)` at `runtime/Engine/Math.cs:61`.
- **PR wins** ``method:Tan`0(float)``; PR `public static float Tan(float)` at `runtime/Engine/Math.cs:114`; develop `public static float Tan(float)` at `runtime/Engine/Math.cs:62`.


### 164. `UnityEngine.MeshCollider` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `MeshCollider`; partial `false`; `runtime/Engine/Rendering/GraphicsPrimitives.cs:126`

**develop declarations**:
- class; namespace `UnityEngine`; `MeshCollider`; partial `false`; `runtime/Engine/BallisticEngine.cs:31`

Common member signatures: **5**.
- **PR wins** `field:convex`; PR `public field bool convex` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:127`; develop `public field bool convex` at `runtime/Engine/BallisticEngine.cs:32`.
- **PR wins** `field:planes`; PR `public field float[][] planes` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:127`; develop `public field float[][] planes` at `runtime/Engine/BallisticEngine.cs:32`.
- **PR wins** `field:triangles`; PR `public field int[][] triangles` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:127`; develop `public field int[][] triangles` at `runtime/Engine/BallisticEngine.cs:32`.
- **PR wins** `field:vertices`; PR `public field Vector3[] vertices` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:127`; develop `public field Vector3[] vertices` at `runtime/Engine/BallisticEngine.cs:32`.
- **PR wins** ``method:Raycast`0(Ray, out RaycastHit, float)``; PR `public override bool Raycast(Ray, out RaycastHit, float)` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:128`; develop `public override bool Raycast(Ray, out RaycastHit, float)` at `runtime/Engine/BallisticEngine.cs:33`.


### 165. `UnityEngine.MonoBehaviour` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `MonoBehaviour`; partial `false`; `runtime/Engine/Objects.cs:109`

**develop declarations**:
- class; namespace `UnityEngine`; `MonoBehaviour`; partial `false`; `runtime/Engine/Objects.cs:78`

Common member signatures: **5**.
- **PR wins** `field:awoken`; PR `internal field bool awoken` at `runtime/Engine/Objects.cs:110`; develop `internal field bool awoken` at `runtime/Engine/Objects.cs:79`.
- **PR wins** `field:enableNotified`; PR `internal field bool enableNotified` at `runtime/Engine/Objects.cs:110`; develop `internal field bool enableNotified` at `runtime/Engine/Objects.cs:79`.
- **PR wins** `field:started`; PR `internal field bool started` at `runtime/Engine/Objects.cs:110`; develop `internal field bool started` at `runtime/Engine/Objects.cs:79`.
- **PR wins** ``method:StartCoroutine`0(IEnumerator)``; PR `public CoroutineHandle StartCoroutine(IEnumerator)` at `runtime/Engine/Objects.cs:111`; develop `public CoroutineHandle StartCoroutine(IEnumerator)` at `runtime/Engine/Objects.cs:80`.
- **PR wins** ``method:StopAllCoroutines`0()``; PR `public void StopAllCoroutines()` at `runtime/Engine/Objects.cs:121`; develop `public void StopAllCoroutines()` at `runtime/Engine/Objects.cs:81`.


### 166. `UnityEngine.Object` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Object`; partial `false`; `runtime/Engine/Objects.cs:8`

**develop declarations**:
- class; namespace `UnityEngine`; `Object`; partial `false`; `runtime/Engine/Objects.cs:7`

Common member signatures: **17**.
- **PR wins** `conversion::bool(Object)`; PR `public static bool(Object)` at `runtime/Engine/Objects.cs:22`; develop `public static bool(Object)` at `runtime/Engine/Objects.cs:14`.
- **PR wins** `field:destroyed`; PR `internal field bool destroyed` at `runtime/Engine/Objects.cs:10`; develop `internal field bool destroyed` at `runtime/Engine/Objects.cs:9`.
- **PR wins** `field:instanceID`; PR `readonly field int instanceID` at `runtime/Engine/Objects.cs:13`; develop `readonly field int instanceID` at `runtime/Engine/Objects.cs:11`.
- **PR wins** `field:nextID`; PR `static field int nextID` at `runtime/Engine/Objects.cs:9`; develop `static field int nextID` at `runtime/Engine/Objects.cs:8`.
- **PR wins** `field:world`; PR `internal field RuntimeWorld world` at `runtime/Engine/Objects.cs:11`; develop `internal field RuntimeWorld world` at `runtime/Engine/Objects.cs:10`.
- **PR wins** ``method:Destroy`0(Object)``; PR `public static void Destroy(Object)` at `runtime/Engine/Objects.cs:30`; develop `public static void Destroy(Object)` at `runtime/Engine/Objects.cs:22`.
- **PR wins** ``method:Equals`0(object)``; PR `public override bool Equals(object)` at `runtime/Engine/Objects.cs:28`; develop `public override bool Equals(object)` at `runtime/Engine/Objects.cs:20`.
- **PR wins** ``method:FindObjectsOfType`1()``; PR `public static T[] FindObjectsOfType<T>()` at `runtime/Engine/Objects.cs:33`; develop `public static T[] FindObjectsOfType<T>()` at `runtime/Engine/Objects.cs:23`.
- **PR wins** ``method:GetHashCode`0()``; PR `public override int GetHashCode()` at `runtime/Engine/Objects.cs:29`; develop `public override int GetHashCode()` at `runtime/Engine/Objects.cs:21`.
- **PR wins** ``method:GetInstanceID`0()``; PR `public int GetInstanceID()` at `runtime/Engine/Objects.cs:21`; develop `public int GetInstanceID()` at `runtime/Engine/Objects.cs:13`.
- **PR wins** ``method:Instantiate`1(T)``; PR `public static T Instantiate<T>(T)` at `runtime/Engine/Objects.cs:36`; develop `public static T Instantiate<T>(T)` at `runtime/Engine/Objects.cs:24`.
- **PR wins** ``method:Instantiate`1(T, Transform)``; PR `public static T Instantiate<T>(T, Transform)` at `runtime/Engine/Objects.cs:37`; develop `public static T Instantiate<T>(T, Transform)` at `runtime/Engine/Objects.cs:25`.
- **PR wins** ``method:Instantiate`1(T, Transform, bool)``; PR `public static T Instantiate<T>(T, Transform, bool)` at `runtime/Engine/Objects.cs:38`; develop `public static T Instantiate<T>(T, Transform, bool)` at `runtime/Engine/Objects.cs:26`.
- **PR wins** ``method:Instantiate`1(T, Vector3, Quaternion)``; PR `public static T Instantiate<T>(T, Vector3, Quaternion)` at `runtime/Engine/Objects.cs:39`; develop `public static T Instantiate<T>(T, Vector3, Quaternion)` at `runtime/Engine/Objects.cs:27`.
- **PR wins** `operator:!=(Object, Object)`; PR `public static bool operator !=(Object, Object)` at `runtime/Engine/Objects.cs:27`; develop `public static bool operator !=(Object, Object)` at `runtime/Engine/Objects.cs:19`.
- **PR wins** `operator:==(Object, Object)`; PR `public static bool operator ==(Object, Object)` at `runtime/Engine/Objects.cs:23`; develop `public static bool operator ==(Object, Object)` at `runtime/Engine/Objects.cs:15`.
- **PR wins** `property:name`; PR `public virtual string name` at `runtime/Engine/Objects.cs:14`; develop `public virtual string name` at `runtime/Engine/Objects.cs:12`.


### 167. `UnityEngine.OriginalArmTransformOverloads` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `OriginalArmTransformOverloads`; partial `false`; `runtime/Engine/ArmPhysicsComponents.cs:54`

**develop declarations**:
- class; namespace `UnityEngine`; `OriginalArmTransformOverloads`; partial `false`; `runtime/Engine/ArmPhysicsComponents.cs:27`

Common member signatures: **1**.
- **PR wins** ``method:TransformPoint`0(this Transform, float, float, float)``; PR `public static Vector3 TransformPoint(this Transform, float, float, float)` at `runtime/Engine/ArmPhysicsComponents.cs:55`; develop `public static Vector3 TransformPoint(this Transform, float, float, float)` at `runtime/Engine/ArmPhysicsComponents.cs:28`.


### 168. `UnityEngine.Physics` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Physics`; partial `false`; `runtime/Engine/Physics.cs:53`

**develop declarations**:
- class; namespace `UnityEngine`; `Physics`; partial `false`; `runtime/Engine/Physics.cs:41`

Common member signatures: **4**.
- **PR wins** ``method:RaycastAll`0(Ray, float, int, QueryTriggerInteraction)``; PR `public static RaycastHit[] RaycastAll(Ray, float, int, QueryTriggerInteraction)` at `runtime/Engine/Physics.cs:55`; develop `public static RaycastHit[] RaycastAll(Ray, float, int, QueryTriggerInteraction)` at `runtime/Engine/Physics.cs:43`.
- **PR wins** ``method:Raycast`0(Ray, out RaycastHit, float, int, QueryTriggerInteraction)``; PR `public static bool Raycast(Ray, out RaycastHit, float, int, QueryTriggerInteraction)` at `runtime/Engine/Physics.cs:62`; develop `public static bool Raycast(Ray, out RaycastHit, float, int, QueryTriggerInteraction)` at `runtime/Engine/Physics.cs:50`.
- **PR wins** ``method:Raycast`0(Vector3, Vector3, out RaycastHit, float, int)``; PR `public static bool Raycast(Vector3, Vector3, out RaycastHit, float, int)` at `runtime/Engine/Physics.cs:63`; develop `public static bool Raycast(Vector3, Vector3, out RaycastHit, float, int)` at `runtime/Engine/Physics.cs:51`.
- **PR wins** `property:queriesHitTriggers`; PR `public static bool queriesHitTriggers` at `runtime/Engine/Physics.cs:54`; develop `public static bool queriesHitTriggers` at `runtime/Engine/Physics.cs:42`.


### 169. `UnityEngine.PrefabCloner` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `PrefabCloner`; partial `false`; `runtime/Engine/PrefabCloner.cs:6`

**develop declarations**:
- class; namespace `UnityEngine`; `PrefabCloner`; partial `false`; `runtime/Engine/PrefabCloner.cs:6`

Common member signatures: **2**.
- **PR wins** ``method:Clone`1(T, Transform, bool, Vector3?, Quaternion?)``; PR `public static T Clone<T>(T, Transform, bool, Vector3?, Quaternion?)` at `runtime/Engine/PrefabCloner.cs:7`; develop `public static T Clone<T>(T, Transform, bool, Vector3?, Quaternion?)` at `runtime/Engine/PrefabCloner.cs:7`.
- **PR wins** ``method:SerializedFields`0(Type)``; PR `static IEnumerable<FieldInfo> SerializedFields(Type)` at `runtime/Engine/PrefabCloner.cs:64`; develop `static IEnumerable<FieldInfo> SerializedFields(Type)` at `runtime/Engine/PrefabCloner.cs:63`.


### 170. `UnityEngine.Quaternion` — PR type wins

**PR declarations**:
- struct; namespace `UnityEngine`; `Quaternion`; partial `false`; `runtime/Engine/Math.cs:71`

**develop declarations**:
- struct; namespace `UnityEngine`; `Quaternion`; partial `false`; `runtime/Engine/Math.cs:38`

Common member signatures: **14**.
- **PR wins** `constructor:Quaternion(float, float, float, float)`; PR `public UnityEngine.Quaternion(float, float, float, float)` at `runtime/Engine/Math.cs:73`; develop `public UnityEngine.Quaternion(float, float, float, float)` at `runtime/Engine/Math.cs:40`.
- **PR wins** `field:w`; PR `public field float w` at `runtime/Engine/Math.cs:72`; develop `public field float w` at `runtime/Engine/Math.cs:39`.
- **PR wins** `field:x`; PR `public field float x` at `runtime/Engine/Math.cs:72`; develop `public field float x` at `runtime/Engine/Math.cs:39`.
- **PR wins** `field:y`; PR `public field float y` at `runtime/Engine/Math.cs:72`; develop `public field float y` at `runtime/Engine/Math.cs:39`.
- **PR wins** `field:z`; PR `public field float z` at `runtime/Engine/Math.cs:72`; develop `public field float z` at `runtime/Engine/Math.cs:39`.
- **PR wins** ``method:AngleAxis`0(float, Vector3)``; PR `public static Quaternion AngleAxis(float, Vector3)` at `runtime/Engine/Math.cs:77`; develop `public static Quaternion AngleAxis(float, Vector3)` at `runtime/Engine/Math.cs:44`.
- **PR wins** ``method:From`0(N.Quaternion)``; PR `internal static Quaternion From(N.Quaternion)` at `runtime/Engine/Math.cs:75`; develop `internal static Quaternion From(N.Quaternion)` at `runtime/Engine/Math.cs:42`.
- **PR wins** ``method:Inverse`0(Quaternion)``; PR `public static Quaternion Inverse(Quaternion)` at `runtime/Engine/Math.cs:82`; develop `public static Quaternion Inverse(Quaternion)` at `runtime/Engine/Math.cs:47`.
- **PR wins** ``method:LookRotation`0(Vector3)``; PR `public static Quaternion LookRotation(Vector3)` at `runtime/Engine/Math.cs:80`; develop `public static Quaternion LookRotation(Vector3)` at `runtime/Engine/Math.cs:45`.
- **PR wins** ``method:LookRotation`0(Vector3, Vector3)``; PR `public static Quaternion LookRotation(Vector3, Vector3)` at `runtime/Engine/Math.cs:81`; develop `public static Quaternion LookRotation(Vector3, Vector3)` at `runtime/Engine/Math.cs:46`.
- **PR wins** `operator:*(Quaternion, Quaternion)`; PR `public static Quaternion operator *(Quaternion, Quaternion)` at `runtime/Engine/Math.cs:89`; develop `public static Quaternion operator *(Quaternion, Quaternion)` at `runtime/Engine/Math.cs:48`.
- **PR wins** `operator:*(Quaternion, Vector3)`; PR `public static Vector3 operator *(Quaternion, Vector3)` at `runtime/Engine/Math.cs:90`; develop `public static Vector3 operator *(Quaternion, Vector3)` at `runtime/Engine/Math.cs:49`.
- **PR wins** `property:Native`; PR `internal N.Quaternion Native` at `runtime/Engine/Math.cs:74`; develop `internal N.Quaternion Native` at `runtime/Engine/Math.cs:41`.
- **PR wins** `property:identity`; PR `public static Quaternion identity` at `runtime/Engine/Math.cs:76`; develop `public static Quaternion identity` at `runtime/Engine/Math.cs:43`.


### 171. `UnityEngine.QueryTriggerInteraction` — PR type wins

**PR declarations**:
- enum; namespace `UnityEngine`; `QueryTriggerInteraction`; partial `false`; `runtime/Engine/Physics.cs:12`

**develop declarations**:
- enum; namespace `UnityEngine`; `QueryTriggerInteraction`; partial `false`; `runtime/Engine/Physics.cs:12`

Common member signatures: **0**.


### 172. `UnityEngine.Random` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Random`; partial `false`; `runtime/Engine/Math.cs:128`

**develop declarations**:
- class; namespace `UnityEngine`; `Random`; partial `false`; `runtime/Engine/Math.cs:74`

Common member signatures: **6**.
- **PR wins** `field:source`; PR `static readonly field System.Random source` at `runtime/Engine/Math.cs:129`; develop `static readonly field System.Random source` at `runtime/Engine/Math.cs:75`.
- **PR wins** ``method:Range`0(float, float)``; PR `public static float Range(float, float)` at `runtime/Engine/Math.cs:134`; develop `public static float Range(float, float)` at `runtime/Engine/Math.cs:79`.
- **PR wins** ``method:Range`0(int, int)``; PR `public static int Range(int, int)` at `runtime/Engine/Math.cs:135`; develop `public static int Range(int, int)` at `runtime/Engine/Math.cs:80`.
- **PR wins** `property:insideUnitCircle`; PR `public static Vector2 insideUnitCircle` at `runtime/Engine/Math.cs:130`; develop `public static Vector2 insideUnitCircle` at `runtime/Engine/Math.cs:76`.
- **PR wins** `property:onUnitSphere`; PR `public static Vector3 onUnitSphere` at `runtime/Engine/Math.cs:132`; develop `public static Vector3 onUnitSphere` at `runtime/Engine/Math.cs:77`.
- **PR wins** `property:value`; PR `public static float value` at `runtime/Engine/Math.cs:133`; develop `public static float value` at `runtime/Engine/Math.cs:78`.


### 173. `UnityEngine.RangeAttribute` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `RangeAttribute`; partial `false`; `runtime/Engine/ArmPhysicsComponents.cs:3`

**develop declarations**:
- class; namespace `UnityEngine`; `RangeAttribute`; partial `false`; `runtime/Engine/ArmPhysicsComponents.cs:3`

Common member signatures: **2**.
- **PR wins** `field:max`; PR `public field float max` at `runtime/Engine/ArmPhysicsComponents.cs:5`; develop `public field float max` at `runtime/Engine/ArmPhysicsComponents.cs:5`.
- **PR wins** `field:min`; PR `public field float min` at `runtime/Engine/ArmPhysicsComponents.cs:5`; develop `public field float min` at `runtime/Engine/ArmPhysicsComponents.cs:5`.


### 174. `UnityEngine.Ray` — PR type wins

**PR declarations**:
- struct; namespace `UnityEngine`; `Ray`; partial `false`; `runtime/Engine/Physics.cs:3`

**develop declarations**:
- struct; namespace `UnityEngine`; `Ray`; partial `false`; `runtime/Engine/Physics.cs:3`

Common member signatures: **4**.
- **PR wins** `constructor:Ray(Vector3, Vector3)`; PR `public UnityEngine.Ray(Vector3, Vector3)` at `runtime/Engine/Physics.cs:5`; develop `public UnityEngine.Ray(Vector3, Vector3)` at `runtime/Engine/Physics.cs:5`.
- **PR wins** `field:direction`; PR `public field Vector3 direction` at `runtime/Engine/Physics.cs:4`; develop `public field Vector3 direction` at `runtime/Engine/Physics.cs:4`.
- **PR wins** `field:origin`; PR `public field Vector3 origin` at `runtime/Engine/Physics.cs:4`; develop `public field Vector3 origin` at `runtime/Engine/Physics.cs:4`.
- **PR wins** ``method:GetPoint`0(float)``; PR `public Vector3 GetPoint(float)` at `runtime/Engine/Physics.cs:6`; develop `public Vector3 GetPoint(float)` at `runtime/Engine/Physics.cs:6`.


### 175. `UnityEngine.RaycastHit` — PR type wins

**PR declarations**:
- struct; namespace `UnityEngine`; `RaycastHit`; partial `false`; `runtime/Engine/Physics.cs:8`

**develop declarations**:
- struct; namespace `UnityEngine`; `RaycastHit`; partial `false`; `runtime/Engine/Physics.cs:8`

Common member signatures: **5**.
- **PR wins** `field:collider`; PR `public field Collider collider` at `runtime/Engine/Physics.cs:9`; develop `public field Collider collider` at `runtime/Engine/Physics.cs:9`.
- **PR wins** `field:distance`; PR `public field float distance` at `runtime/Engine/Physics.cs:9`; develop `public field float distance` at `runtime/Engine/Physics.cs:9`.
- **PR wins** `field:normal`; PR `public field Vector3 normal` at `runtime/Engine/Physics.cs:9`; develop `public field Vector3 normal` at `runtime/Engine/Physics.cs:9`.
- **PR wins** `field:point`; PR `public field Vector3 point` at `runtime/Engine/Physics.cs:9`; develop `public field Vector3 point` at `runtime/Engine/Physics.cs:9`.
- **PR wins** `property:transform`; PR `public Transform transform` at `runtime/Engine/Physics.cs:10`; develop `public Transform transform` at `runtime/Engine/Physics.cs:10`.


### 176. `UnityEngine.Rect` — PR type wins

**PR declarations**:
- struct; namespace `UnityEngine`; `Rect`; partial `false`; `runtime/Engine/Rendering/GraphicsPrimitives.cs:11`

**develop declarations**:
- struct; namespace `UnityEngine`; `Rect`; partial `false`; `runtime/Engine/BallisticEngine.cs:5`

Common member signatures: **7**.
- **PR wins** `constructor:Rect(float, float, float, float)`; PR `public UnityEngine.Rect(float, float, float, float)` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:13`; develop `public UnityEngine.Rect(float, float, float, float)` at `runtime/Engine/BallisticEngine.cs:5`.
- **PR wins** `field:height`; PR `public field float height` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:12`; develop `public field float height` at `runtime/Engine/BallisticEngine.cs:5`.
- **PR wins** `field:width`; PR `public field float width` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:12`; develop `public field float width` at `runtime/Engine/BallisticEngine.cs:5`.
- **PR wins** `field:x`; PR `public field float x` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:12`; develop `public field float x` at `runtime/Engine/BallisticEngine.cs:5`.
- **PR wins** `field:y`; PR `public field float y` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:12`; develop `public field float y` at `runtime/Engine/BallisticEngine.cs:5`.
- **PR wins** `property:max`; PR `public Vector2 max` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:20`; develop `public Vector2 max` at `runtime/Engine/BallisticEngine.cs:5`.
- **PR wins** `property:min`; PR `public Vector2 min` at `runtime/Engine/Rendering/GraphicsPrimitives.cs:20`; develop `public Vector2 min` at `runtime/Engine/BallisticEngine.cs:5`.


### 177. `UnityEngine.RectTransform` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `RectTransform`; partial `false`; `runtime/Engine/Objects.cs:196`

**develop declarations**:
- class; namespace `UnityEngine`; `RectTransform`; partial `false`; `runtime/Engine/Objects.cs:126`

Common member signatures: **0**.


### 178. `UnityEngine.Renderer` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Renderer`; partial `false`; `runtime/Engine/Rendering/GraphicsPrimitives.cs:75`

**develop declarations**:
- class; namespace `UnityEngine`; `Renderer`; partial `false`; `runtime/Engine/BallisticEngine.cs:28`

Common member signatures: **0**.

Develop-only member signatures: **1** — **DEVELOP-ONLY / DEFERRED**.
- **DEVELOP-ONLY — DEFERRED** `field:material` — `public field Material material` at `runtime/Engine/BallisticEngine.cs:28`.


### 179. `UnityEngine.Resources` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Resources`; partial `false`; `runtime/Engine/Resources.cs:6`

**develop declarations**:
- class; namespace `UnityEngine`; `Resources`; partial `false`; `runtime/Engine/Resources.cs:6`

Common member signatures: **7**.
- **PR wins** `field:assetWorld`; PR `static field RuntimeWorld assetWorld` at `runtime/Engine/Resources.cs:10`; develop `static field RuntimeWorld assetWorld` at `runtime/Engine/Resources.cs:10`.
- **PR wins** `field:cache`; PR `static readonly field Dictionary<string,Object> cache` at `runtime/Engine/Resources.cs:9`; develop `static readonly field Dictionary<string,Object> cache` at `runtime/Engine/Resources.cs:9`.
- **PR wins** `field:packageText`; PR `static field string packageText` at `runtime/Engine/Resources.cs:7`; develop `static field string packageText` at `runtime/Engine/Resources.cs:7`.
- **PR wins** `field:paths`; PR `static readonly field Dictionary<string,string> paths` at `runtime/Engine/Resources.cs:8`; develop `static readonly field Dictionary<string,string> paths` at `runtime/Engine/Resources.cs:8`.
- **PR wins** ``method:Configure`0(string)``; PR `public static void Configure(string)` at `runtime/Engine/Resources.cs:11`; develop `public static void Configure(string)` at `runtime/Engine/Resources.cs:11`.
- **PR wins** ``method:Load`0(string)``; PR `public static Object Load(string)` at `runtime/Engine/Resources.cs:20`; develop `public static Object Load(string)` at `runtime/Engine/Resources.cs:20`.
- **PR wins** ``method:Load`1(string)``; PR `public static T Load<T>(string)` at `runtime/Engine/Resources.cs:38`; develop `public static T Load<T>(string)` at `runtime/Engine/Resources.cs:29`.


### 180. `UnityEngine.Rigidbody` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Rigidbody`; partial `false`; `runtime/Engine/PhysicsComponents.cs:36`

**develop declarations**:
- class; namespace `UnityEngine`; `Rigidbody`; partial `false`; `runtime/Engine/PresentationHost.cs:7`

Common member signatures: **9**.
- **PR wins** `field:ForceSink`; PR `public field Action<Vector3,Vector3> ForceSink` at `runtime/Engine/PhysicsComponents.cs:44`; develop `public field Action<Vector3,Vector3> ForceSink` at `runtime/Engine/PresentationHost.cs:9`.
- **PR wins** `field:KinematicChanged`; PR `public field Action<bool> KinematicChanged` at `runtime/Engine/PhysicsComponents.cs:49`; develop `public field Action<bool> KinematicChanged` at `runtime/Engine/PresentationHost.cs:13`.
- **PR wins** `field:PhysicsForceSink`; PR `public field Action<Vector3,Vector3,ForceMode> PhysicsForceSink` at `runtime/Engine/PhysicsComponents.cs:47`; develop `public field Action<Vector3,Vector3,ForceMode> PhysicsForceSink` at `runtime/Engine/PresentationHost.cs:12`.
- **PR wins** `field:kinematic`; PR `field bool kinematic` at `runtime/Engine/PhysicsComponents.cs:50`; develop `field bool kinematic` at `runtime/Engine/PresentationHost.cs:29`.
- **PR wins** `field:velocity`; PR `public field Vector3 velocity` at `runtime/Engine/PhysicsComponents.cs:37`; develop `public field Vector3 velocity` at `runtime/Engine/PresentationHost.cs:8`.
- **PR wins** ``method:AddForceAtPosition`0(Vector3, Vector3)``; PR `public void AddForceAtPosition(Vector3, Vector3)` at `runtime/Engine/PhysicsComponents.cs:55`; develop `public void AddForceAtPosition(Vector3, Vector3)` at `runtime/Engine/PresentationHost.cs:34`.
- **PR wins** ``method:AddForceAtPosition`0(Vector3, Vector3, ForceMode)``; PR `public void AddForceAtPosition(Vector3, Vector3, ForceMode)` at `runtime/Engine/PhysicsComponents.cs:65`; develop `public void AddForceAtPosition(Vector3, Vector3, ForceMode)` at `runtime/Engine/PresentationHost.cs:39`.
- **PR wins** ``method:AddForce`0(Vector3, ForceMode)``; PR `public void AddForce(Vector3, ForceMode)` at `runtime/Engine/PhysicsComponents.cs:60`; develop `public void AddForce(Vector3, ForceMode)` at `runtime/Engine/PresentationHost.cs:25`.
- **PR wins** `property:isKinematic`; PR `public bool isKinematic` at `runtime/Engine/PhysicsComponents.cs:51`; develop `public bool isKinematic` at `runtime/Engine/PresentationHost.cs:30`.

Develop-only member signatures: **9** — **DEVELOP-ONLY / DEFERRED**.
- **DEVELOP-ONLY — DEFERRED** `field:DragChanged` — `public field Action<float> DragChanged` at `runtime/Engine/PresentationHost.cs:17`.
- **DEVELOP-ONLY — DEFERRED** `field:GravityChanged` — `public field Action<bool> GravityChanged` at `runtime/Engine/PresentationHost.cs:16`.
- **DEVELOP-ONLY — DEFERRED** `field:dragValue` — `field float dragValue` at `runtime/Engine/PresentationHost.cs:23`.
- **DEVELOP-ONLY — DEFERRED** `field:gravity` — `field bool gravity` at `runtime/Engine/PresentationHost.cs:21`.
- **DEVELOP-ONLY — DEFERRED** `property:angularDrag` — `public float angularDrag` at `runtime/Engine/PresentationHost.cs:19`.
- **DEVELOP-ONLY — DEFERRED** `property:drag` — `public float drag` at `runtime/Engine/PresentationHost.cs:24`.
- **DEVELOP-ONLY — DEFERRED** `property:interpolation` — `public RigidbodyInterpolation interpolation` at `runtime/Engine/PresentationHost.cs:20`.
- **DEVELOP-ONLY — DEFERRED** `property:mass` — `public float mass` at `runtime/Engine/PresentationHost.cs:18`.
- **DEVELOP-ONLY — DEFERRED** `property:useGravity` — `public bool useGravity` at `runtime/Engine/PresentationHost.cs:22`.


### 181. `UnityEngine.RigidbodyInterpolation` — PR type wins

**PR declarations**:
- enum; namespace `UnityEngine`; `RigidbodyInterpolation`; partial `false`; `runtime/Engine/PhysicsComponents.cs:73`

**develop declarations**:
- enum; namespace `UnityEngine`; `RigidbodyInterpolation`; partial `false`; `runtime/Engine/PresentationHost.cs:6`

Common member signatures: **0**.


### 182. `UnityEngine.RuntimeWorld` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `RuntimeWorld`; partial `false`; `runtime/Engine/Objects.cs:242`

**develop declarations**:
- class; namespace `UnityEngine`; `RuntimeWorld`; partial `false`; `runtime/Engine/Objects.cs:129`

Common member signatures: **23**.
- **PR wins** `constructor:RuntimeWorld(bool, bool)`; PR `public UnityEngine.RuntimeWorld(bool, bool)` at `runtime/Engine/Objects.cs:254`; develop `public UnityEngine.RuntimeWorld(bool, bool)` at `runtime/Engine/Objects.cs:138`.
- **PR wins** `field:Objects`; PR `public readonly field List<Object> Objects` at `runtime/Engine/Objects.cs:245`; develop `public readonly field List<Object> Objects` at `runtime/Engine/Objects.cs:132`.
- **PR wins** `field:activationDeferrals`; PR `field int activationDeferrals` at `runtime/Engine/Objects.cs:252`; develop `field int activationDeferrals` at `runtime/Engine/Objects.cs:136`.
- **PR wins** `field:current`; PR `static field RuntimeWorld current` at `runtime/Engine/Objects.cs:243`; develop `static field RuntimeWorld current` at `runtime/Engine/Objects.cs:130`.
- **PR wins** `field:destroyQueue`; PR `readonly field HashSet<Object> destroyQueue` at `runtime/Engine/Objects.cs:248`; develop `readonly field HashSet<Object> destroyQueue` at `runtime/Engine/Objects.cs:133`.
- **PR wins** `field:messages`; PR `readonly field Dictionary<(Type,string),MethodInfo> messages` at `runtime/Engine/Objects.cs:250`; develop `readonly field Dictionary<(Type,string),MethodInfo> messages` at `runtime/Engine/Objects.cs:135`.
- **PR wins** `field:routines`; PR `readonly field List<CoroutineHandle> routines` at `runtime/Engine/Objects.cs:249`; develop `readonly field List<CoroutineHandle> routines` at `runtime/Engine/Objects.cs:134`.
- **PR wins** ``method:Activate`0(Component)``; PR `internal void Activate(Component)` at `runtime/Engine/Objects.cs:283`; develop `internal void Activate(Component)` at `runtime/Engine/Objects.cs:151`.
- **PR wins** ``method:Active`0()``; PR `MonoBehaviour[] Active()` at `runtime/Engine/Objects.cs:307`; develop `MonoBehaviour[] Active()` at `runtime/Engine/Objects.cs:175`.
- **PR wins** ``method:DeferActivation`0()``; PR `public IDisposable DeferActivation()` at `runtime/Engine/Objects.cs:318`; develop `public IDisposable DeferActivation()` at `runtime/Engine/Objects.cs:184`.
- **PR wins** ``method:DestroyNow`0(Object)``; PR `internal void DestroyNow(Object)` at `runtime/Engine/Objects.cs:308`; develop `internal void DestroyNow(Object)` at `runtime/Engine/Objects.cs:176`.
- **PR wins** ``method:FlushDestroyed`0()``; PR `internal void FlushDestroyed()` at `runtime/Engine/Objects.cs:306`; develop `internal void FlushDestroyed()` at `runtime/Engine/Objects.cs:174`.
- **PR wins** ``method:Initialize`0()``; PR `public void Initialize()` at `runtime/Engine/Objects.cs:287`; develop `public void Initialize()` at `runtime/Engine/Objects.cs:155`.
- **PR wins** ``method:Invoke`0(MonoBehaviour, string)``; PR `public void Invoke(MonoBehaviour, string)` at `runtime/Engine/Objects.cs:258`; develop `public void Invoke(MonoBehaviour, string)` at `runtime/Engine/Objects.cs:142`.
- **PR wins** ``method:MakeCurrent`0()``; PR `public void MakeCurrent()` at `runtime/Engine/Objects.cs:255`; develop `public void MakeCurrent()` at `runtime/Engine/Objects.cs:139`.
- **PR wins** ``method:QueueDestroy`0(Object)``; PR `internal void QueueDestroy(Object)` at `runtime/Engine/Objects.cs:257`; develop `internal void QueueDestroy(Object)` at `runtime/Engine/Objects.cs:141`.
- **PR wins** ``method:RefreshActivation`0()``; PR `internal void RefreshActivation()` at `runtime/Engine/Objects.cs:288`; develop `internal void RefreshActivation()` at `runtime/Engine/Objects.cs:156`.
- **PR wins** ``method:Register`0(Object)``; PR `internal void Register(Object)` at `runtime/Engine/Objects.cs:256`; develop `internal void Register(Object)` at `runtime/Engine/Objects.cs:140`.
- **PR wins** ``method:StartRoutine`0(MonoBehaviour, IEnumerator)``; PR `internal CoroutineHandle StartRoutine(MonoBehaviour, IEnumerator)` at `runtime/Engine/Objects.cs:314`; develop `internal CoroutineHandle StartRoutine(MonoBehaviour, IEnumerator)` at `runtime/Engine/Objects.cs:182`.
- **PR wins** ``method:StopRoutines`0(MonoBehaviour)``; PR `internal void StopRoutines(MonoBehaviour)` at `runtime/Engine/Objects.cs:317`; develop `internal void StopRoutines(MonoBehaviour)` at `runtime/Engine/Objects.cs:183`.
- **PR wins** ``method:Tick`0(float, Action)``; PR `public void Tick(float, Action)` at `runtime/Engine/Objects.cs:292`; develop `public void Tick(float, Action)` at `runtime/Engine/Objects.cs:160`.
- **PR wins** `property:Current`; PR `public static RuntimeWorld Current` at `runtime/Engine/Objects.cs:244`; develop `public static RuntimeWorld Current` at `runtime/Engine/Objects.cs:131`.
- **PR wins** `property:Initialized`; PR `public bool Initialized` at `runtime/Engine/Objects.cs:253`; develop `public bool Initialized` at `runtime/Engine/Objects.cs:137`.


### 183. `UnityEngine.RuntimeWorld+ActivationScope` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `ActivationScope`; partial `false`; `runtime/Engine/Objects.cs:319`

**develop declarations**:
- class; namespace `UnityEngine`; `ActivationScope`; partial `false`; `runtime/Engine/Objects.cs:185`

Common member signatures: **2**.
- **PR wins** `field:value`; PR `field RuntimeWorld value` at `runtime/Engine/Objects.cs:320`; develop `field RuntimeWorld value` at `runtime/Engine/Objects.cs:186`.
- **PR wins** ``method:Dispose`0()``; PR `public void Dispose()` at `runtime/Engine/Objects.cs:321`; develop `public void Dispose()` at `runtime/Engine/Objects.cs:187`.


### 184. `UnityEngine.SerializeField` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `SerializeField`; partial `false`; `runtime/Engine/Math.cs:126`

**develop declarations**:
- class; namespace `UnityEngine`; `SerializeField`; partial `false`; `runtime/Engine/Math.cs:72`

Common member signatures: **0**.


### 185. `UnityEngine.Texture2D` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Texture2D`; partial `false`; `runtime/Engine/Rendering/Texture2D.cs:67`

**develop declarations**:
- class; namespace `UnityEngine`; `Texture2D`; partial `false`; `runtime/Engine/BallisticEngine.cs:8`

Common member signatures: **9**.
- **PR wins** `constructor:Texture2D(int, int, byte[], RuntimeWorld)`; PR `public UnityEngine.Texture2D(int, int, byte[], RuntimeWorld)` at `runtime/Engine/Rendering/Texture2D.cs:81`; develop `public UnityEngine.Texture2D(int, int, byte[], RuntimeWorld)` at `runtime/Engine/BallisticEngine.cs:10`.
- **PR wins** `field:Changed`; PR `public field Action<Texture2D,int,int,int,int,byte[]> Changed` at `runtime/Engine/Rendering/Texture2D.cs:75`; develop `public field Action<Texture2D,int,int,int,int,byte[]> Changed` at `runtime/Engine/BallisticEngine.cs:9`.
- **PR wins** `field:SourceID`; PR `public field string SourceID` at `runtime/Engine/Rendering/Texture2D.cs:74`; develop `public field string SourceID` at `runtime/Engine/BallisticEngine.cs:9`.
- **PR wins** `field:rgba`; PR `field byte[] rgba` at `runtime/Engine/Rendering/Texture2D.cs:70`; develop `field byte[] rgba` at `runtime/Engine/BallisticEngine.cs:9`.
- **PR wins** ``method:Apply`0()``; PR `public void Apply()` at `runtime/Engine/Rendering/Texture2D.cs:144`; develop `public void Apply()` at `runtime/Engine/BallisticEngine.cs:15`.
- **PR wins** ``method:CloneTexture`0(RuntimeWorld)``; PR `public Texture2D CloneTexture(RuntimeWorld)` at `runtime/Engine/Rendering/Texture2D.cs:96`; develop `public Texture2D CloneTexture(RuntimeWorld)` at `runtime/Engine/BallisticEngine.cs:11`.
- **PR wins** ``method:GetPixel`0(int, int)``; PR `public Color GetPixel(int, int)` at `runtime/Engine/Rendering/Texture2D.cs:105`; develop `public Color GetPixel(int, int)` at `runtime/Engine/BallisticEngine.cs:12`.
- **PR wins** ``method:GetPixels`0(int, int, int, int, int)``; PR `public Color[] GetPixels(int, int, int, int, int)` at `runtime/Engine/Rendering/Texture2D.cs:113`; develop `public Color[] GetPixels(int, int, int, int, int)` at `runtime/Engine/BallisticEngine.cs:13`.
- **PR wins** ``method:SetPixels`0(int, int, int, int, Color[])``; PR `public void SetPixels(int, int, int, int, Color[])` at `runtime/Engine/Rendering/Texture2D.cs:121`; develop `public void SetPixels(int, int, int, int, Color[])` at `runtime/Engine/BallisticEngine.cs:14`.

Develop-only member signatures: **2** — **DEVELOP-ONLY / DEFERRED**.
- **DEVELOP-ONLY — DEFERRED** `field:height` — `public field int height` at `runtime/Engine/BallisticEngine.cs:9`.
- **DEVELOP-ONLY — DEFERRED** `field:width` — `public field int width` at `runtime/Engine/BallisticEngine.cs:9`.


### 186. `UnityEngine.Time` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Time`; partial `false`; `runtime/Engine/ClockInput.cs:5`

**develop declarations**:
- class; namespace `UnityEngine`; `Time`; partial `false`; `runtime/Engine/ClockInput.cs:5`

Common member signatures: **16**.
- **PR wins** `field:accumulator`; PR `static field float accumulator` at `runtime/Engine/ClockInput.cs:15`; develop `static field float accumulator` at `runtime/Engine/ClockInput.cs:15`.
- **PR wins** `field:frameDelta`; PR `static field float frameDelta` at `runtime/Engine/ClockInput.cs:15`; develop `static field float frameDelta` at `runtime/Engine/ClockInput.cs:15`.
- **PR wins** `field:frameTime`; PR `static field float frameTime` at `runtime/Engine/ClockInput.cs:15`; develop `static field float frameTime` at `runtime/Engine/ClockInput.cs:15`.
- **PR wins** ``method:BeginFixedStep`0()``; PR `internal static bool BeginFixedStep()` at `runtime/Engine/ClockInput.cs:23`; develop `internal static bool BeginFixedStep()` at `runtime/Engine/ClockInput.cs:23`.
- **PR wins** ``method:BeginFrame`0(float)``; PR `internal static void BeginFrame(float)` at `runtime/Engine/ClockInput.cs:17`; develop `internal static void BeginFrame(float)` at `runtime/Engine/ClockInput.cs:17`.
- **PR wins** ``method:BeginUpdate`0()``; PR `internal static void BeginUpdate()` at `runtime/Engine/ClockInput.cs:24`; develop `internal static void BeginUpdate()` at `runtime/Engine/ClockInput.cs:24`.
- **PR wins** ``method:Reset`0()``; PR `internal static void Reset()` at `runtime/Engine/ClockInput.cs:16`; develop `internal static void Reset()` at `runtime/Engine/ClockInput.cs:16`.
- **PR wins** `property:deltaTime`; PR `public static float deltaTime` at `runtime/Engine/ClockInput.cs:11`; develop `public static float deltaTime` at `runtime/Engine/ClockInput.cs:11`.
- **PR wins** `property:fixedDeltaTime`; PR `public static float fixedDeltaTime` at `runtime/Engine/ClockInput.cs:7`; develop `public static float fixedDeltaTime` at `runtime/Engine/ClockInput.cs:7`.
- **PR wins** `property:fixedTime`; PR `public static float fixedTime` at `runtime/Engine/ClockInput.cs:10`; develop `public static float fixedTime` at `runtime/Engine/ClockInput.cs:10`.
- **PR wins** `property:frameCount`; PR `public static int frameCount` at `runtime/Engine/ClockInput.cs:14`; develop `public static int frameCount` at `runtime/Engine/ClockInput.cs:14`.
- **PR wins** `property:maximumDeltaTime`; PR `public static float maximumDeltaTime` at `runtime/Engine/ClockInput.cs:8`; develop `public static float maximumDeltaTime` at `runtime/Engine/ClockInput.cs:8`.
- **PR wins** `property:realtimeSinceStartup`; PR `public static float realtimeSinceStartup` at `runtime/Engine/ClockInput.cs:13`; develop `public static float realtimeSinceStartup` at `runtime/Engine/ClockInput.cs:13`.
- **PR wins** `property:time`; PR `public static float time` at `runtime/Engine/ClockInput.cs:9`; develop `public static float time` at `runtime/Engine/ClockInput.cs:9`.
- **PR wins** `property:timeScale`; PR `public static float timeScale` at `runtime/Engine/ClockInput.cs:6`; develop `public static float timeScale` at `runtime/Engine/ClockInput.cs:6`.
- **PR wins** `property:unscaledDeltaTime`; PR `public static float unscaledDeltaTime` at `runtime/Engine/ClockInput.cs:12`; develop `public static float unscaledDeltaTime` at `runtime/Engine/ClockInput.cs:12`.


### 187. `UnityEngine.TooltipAttribute` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `TooltipAttribute`; partial `false`; `runtime/Engine/Math.cs:124`

**develop declarations**:
- class; namespace `UnityEngine`; `TooltipAttribute`; partial `false`; `runtime/Engine/Math.cs:70`

Common member signatures: **0**.


### 188. `UnityEngine.Transform` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `Transform`; partial `false`; `runtime/Engine/Objects.cs:132`

**develop declarations**:
- class; namespace `UnityEngine`; `Transform`; partial `false`; `runtime/Engine/Objects.cs:83`

Common member signatures: **30**.
- **PR wins** `field:children`; PR `internal readonly field List<Transform> children` at `runtime/Engine/Objects.cs:133`; develop `internal readonly field List<Transform> children` at `runtime/Engine/Objects.cs:84`.
- **PR wins** `field:parentValue`; PR `field Transform parentValue` at `runtime/Engine/Objects.cs:134`; develop `field Transform parentValue` at `runtime/Engine/Objects.cs:85`.
- **PR wins** ``method:Euler`0(Quaternion)``; PR `static Vector3 Euler(Quaternion)` at `runtime/Engine/Objects.cs:165`; develop `static Vector3 Euler(Quaternion)` at `runtime/Engine/Objects.cs:97`.
- **PR wins** ``method:GetChild`0(int)``; PR `public Transform GetChild(int)` at `runtime/Engine/Objects.cs:173`; develop `public Transform GetChild(int)` at `runtime/Engine/Objects.cs:103`.
- **PR wins** ``method:GetEnumerator`0()``; PR `public IEnumerator<Transform> GetEnumerator()` at `runtime/Engine/Objects.cs:193`, `runtime/Engine/Objects.cs:194`; develop `public IEnumerator<Transform> GetEnumerator()` at `runtime/Engine/Objects.cs:123`, `runtime/Engine/Objects.cs:124`.
- **PR wins** ``method:InverseTransformDirection`0(Vector3)``; PR `public Vector3 InverseTransformDirection(Vector3)` at `runtime/Engine/Objects.cs:180`; develop `public Vector3 InverseTransformDirection(Vector3)` at `runtime/Engine/Objects.cs:110`.
- **PR wins** ``method:InverseTransformPoint`0(Vector3)``; PR `public Vector3 InverseTransformPoint(Vector3)` at `runtime/Engine/Objects.cs:175`; develop `public Vector3 InverseTransformPoint(Vector3)` at `runtime/Engine/Objects.cs:105`.
- **PR wins** ``method:InverseTransformVector`0(Vector3)``; PR `public Vector3 InverseTransformVector(Vector3)` at `runtime/Engine/Objects.cs:177`; develop `public Vector3 InverseTransformVector(Vector3)` at `runtime/Engine/Objects.cs:107`.
- **PR wins** ``method:Rotate`0(Vector3, float)``; PR `public void Rotate(Vector3, float)` at `runtime/Engine/Objects.cs:167`; develop `public void Rotate(Vector3, float)` at `runtime/Engine/Objects.cs:99`.
- **PR wins** ``method:SetParent`0(Transform, bool)``; PR `public void SetParent(Transform, bool)` at `runtime/Engine/Objects.cs:182`; develop `public void SetParent(Transform, bool)` at `runtime/Engine/Objects.cs:112`.
- **PR wins** ``method:TransformDirection`0(Vector3)``; PR `public Vector3 TransformDirection(Vector3)` at `runtime/Engine/Objects.cs:179`; develop `public Vector3 TransformDirection(Vector3)` at `runtime/Engine/Objects.cs:109`.
- **PR wins** ``method:TransformDirection`0(float, float, float)``; PR `public Vector3 TransformDirection(float, float, float)` at `runtime/Engine/Objects.cs:178`; develop `public Vector3 TransformDirection(float, float, float)` at `runtime/Engine/Objects.cs:108`.
- **PR wins** ``method:TransformNormal`0(Vector3)``; PR `internal Vector3 TransformNormal(Vector3)` at `runtime/Engine/Objects.cs:181`; develop `internal Vector3 TransformNormal(Vector3)` at `runtime/Engine/Objects.cs:111`.
- **PR wins** ``method:TransformPoint`0(Vector3)``; PR `public Vector3 TransformPoint(Vector3)` at `runtime/Engine/Objects.cs:174`; develop `public Vector3 TransformPoint(Vector3)` at `runtime/Engine/Objects.cs:104`.
- **PR wins** ``method:TransformVector`0(Vector3)``; PR `public Vector3 TransformVector(Vector3)` at `runtime/Engine/Objects.cs:176`; develop `public Vector3 TransformVector(Vector3)` at `runtime/Engine/Objects.cs:106`.
- **PR wins** `property:InverseMatrix`; PR `internal N.Matrix4x4 InverseMatrix` at `runtime/Engine/Objects.cs:161`; develop `internal N.Matrix4x4 InverseMatrix` at `runtime/Engine/Objects.cs:93`.
- **PR wins** `property:LocalMatrix`; PR `internal N.Matrix4x4 LocalMatrix` at `runtime/Engine/Objects.cs:159`; develop `internal N.Matrix4x4 LocalMatrix` at `runtime/Engine/Objects.cs:91`.
- **PR wins** `property:WorldMatrix`; PR `internal N.Matrix4x4 WorldMatrix` at `runtime/Engine/Objects.cs:160`; develop `internal N.Matrix4x4 WorldMatrix` at `runtime/Engine/Objects.cs:92`.
- **PR wins** `property:childCount`; PR `public int childCount` at `runtime/Engine/Objects.cs:139`; develop `public int childCount` at `runtime/Engine/Objects.cs:90`.
- **PR wins** `property:eulerAngles`; PR `public Vector3 eulerAngles` at `runtime/Engine/Objects.cs:164`; develop `public Vector3 eulerAngles` at `runtime/Engine/Objects.cs:96`.
- **PR wins** `property:forward`; PR `public Vector3 forward` at `runtime/Engine/Objects.cs:172`; develop `public Vector3 forward` at `runtime/Engine/Objects.cs:102`.
- **PR wins** `property:localEulerAngles`; PR `public Vector3 localEulerAngles` at `runtime/Engine/Objects.cs:166`; develop `public Vector3 localEulerAngles` at `runtime/Engine/Objects.cs:98`.
- **PR wins** `property:localPosition`; PR `public Vector3 localPosition` at `runtime/Engine/Objects.cs:135`; develop `public Vector3 localPosition` at `runtime/Engine/Objects.cs:86`.
- **PR wins** `property:localRotation`; PR `public Quaternion localRotation` at `runtime/Engine/Objects.cs:137`; develop `public Quaternion localRotation` at `runtime/Engine/Objects.cs:88`.
- **PR wins** `property:localScale`; PR `public Vector3 localScale` at `runtime/Engine/Objects.cs:136`; develop `public Vector3 localScale` at `runtime/Engine/Objects.cs:87`.
- **PR wins** `property:parent`; PR `public Transform parent` at `runtime/Engine/Objects.cs:138`; develop `public Transform parent` at `runtime/Engine/Objects.cs:89`.
- **PR wins** `property:position`; PR `public Vector3 position` at `runtime/Engine/Objects.cs:162`; develop `public Vector3 position` at `runtime/Engine/Objects.cs:94`.
- **PR wins** `property:right`; PR `public Vector3 right` at `runtime/Engine/Objects.cs:171`; develop `public Vector3 right` at `runtime/Engine/Objects.cs:101`.
- **PR wins** `property:rotation`; PR `public Quaternion rotation` at `runtime/Engine/Objects.cs:163`; develop `public Quaternion rotation` at `runtime/Engine/Objects.cs:95`.
- **PR wins** `property:up`; PR `public Vector3 up` at `runtime/Engine/Objects.cs:170`; develop `public Vector3 up` at `runtime/Engine/Objects.cs:100`.


### 189. `UnityEngine.Vector2` — PR type wins

**PR declarations**:
- struct; namespace `UnityEngine`; `Vector2`; partial `false`; `runtime/Engine/Math.cs:6`

**develop declarations**:
- struct; namespace `UnityEngine`; `Vector2`; partial `false`; `runtime/Engine/Math.cs:5`

Common member signatures: **5**.
- **PR wins** `constructor:Vector2(float, float)`; PR `public UnityEngine.Vector2(float, float)` at `runtime/Engine/Math.cs:16`; develop `public UnityEngine.Vector2(float, float)` at `runtime/Engine/Math.cs:7`.
- **PR wins** `conversion::Vector3(Vector2)`; PR `public static Vector3(Vector2)` at `runtime/Engine/Math.cs:18`; develop `public static Vector3(Vector2)` at `runtime/Engine/Math.cs:8`.
- **PR wins** `field:x`; PR `public field float x` at `runtime/Engine/Math.cs:7`; develop `public field float x` at `runtime/Engine/Math.cs:6`.
- **PR wins** `field:y`; PR `public field float y` at `runtime/Engine/Math.cs:7`; develop `public field float y` at `runtime/Engine/Math.cs:6`.
- **PR wins** `operator:*(Vector2, float)`; PR `public static Vector2 operator *(Vector2, float)` at `runtime/Engine/Math.cs:21`; develop `public static Vector2 operator *(Vector2, float)` at `runtime/Engine/Math.cs:9`.


### 190. `UnityEngine.Vector3` — PR type wins

**PR declarations**:
- struct; namespace `UnityEngine`; `Vector3`; partial `false`; `runtime/Engine/Math.cs:25`

**develop declarations**:
- struct; namespace `UnityEngine`; `Vector3`; partial `false`; `runtime/Engine/Math.cs:11`

Common member signatures: **27**.
- **PR wins** `constructor:Vector3(float, float, float)`; PR `public UnityEngine.Vector3(float, float, float)` at `runtime/Engine/Math.cs:28`; develop `public UnityEngine.Vector3(float, float, float)` at `runtime/Engine/Math.cs:13`.
- **PR wins** `field:x`; PR `public field float x` at `runtime/Engine/Math.cs:26`; develop `public field float x` at `runtime/Engine/Math.cs:12`.
- **PR wins** `field:y`; PR `public field float y` at `runtime/Engine/Math.cs:26`; develop `public field float y` at `runtime/Engine/Math.cs:12`.
- **PR wins** `field:z`; PR `public field float z` at `runtime/Engine/Math.cs:26`; develop `public field float z` at `runtime/Engine/Math.cs:12`.
- **PR wins** ``method:Angle`0(Vector3, Vector3)``; PR `public static float Angle(Vector3, Vector3)` at `runtime/Engine/Math.cs:64`; develop `public static float Angle(Vector3, Vector3)` at `runtime/Engine/Math.cs:33`.
- **PR wins** ``method:Cross`0(Vector3, Vector3)``; PR `public static Vector3 Cross(Vector3, Vector3)` at `runtime/Engine/Math.cs:63`; develop `public static Vector3 Cross(Vector3, Vector3)` at `runtime/Engine/Math.cs:32`.
- **PR wins** ``method:Dot`0(Vector3, Vector3)``; PR `public static float Dot(Vector3, Vector3)` at `runtime/Engine/Math.cs:62`; develop `public static float Dot(Vector3, Vector3)` at `runtime/Engine/Math.cs:31`.
- **PR wins** ``method:From`0(N.Vector3)``; PR `internal static Vector3 From(N.Vector3)` at `runtime/Engine/Math.cs:30`; develop `internal static Vector3 From(N.Vector3)` at `runtime/Engine/Math.cs:15`.
- **PR wins** ``method:Lerp`0(Vector3, Vector3, float)``; PR `public static Vector3 Lerp(Vector3, Vector3, float)` at `runtime/Engine/Math.cs:51`; develop `public static Vector3 Lerp(Vector3, Vector3, float)` at `runtime/Engine/Math.cs:30`.
- **PR wins** ``method:Reflect`0(Vector3, Vector3)``; PR `public static Vector3 Reflect(Vector3, Vector3)` at `runtime/Engine/Math.cs:65`; develop `public static Vector3 Reflect(Vector3, Vector3)` at `runtime/Engine/Math.cs:34`.
- **PR wins** ``method:Scale`0(Vector3, Vector3)``; PR `public static Vector3 Scale(Vector3, Vector3)` at `runtime/Engine/Math.cs:66`; develop `public static Vector3 Scale(Vector3, Vector3)` at `runtime/Engine/Math.cs:35`.
- **PR wins** ``method:ToString`0()``; PR `public override string ToString()` at `runtime/Engine/Math.cs:67`; develop `public override string ToString()` at `runtime/Engine/Math.cs:36`.
- **PR wins** `operator:*(Vector3, float)`; PR `public static Vector3 operator *(Vector3, float)` at `runtime/Engine/Math.cs:45`; develop `public static Vector3 operator *(Vector3, float)` at `runtime/Engine/Math.cs:27`.
- **PR wins** `operator:*(float, Vector3)`; PR `public static Vector3 operator *(float, Vector3)` at `runtime/Engine/Math.cs:46`; develop `public static Vector3 operator *(float, Vector3)` at `runtime/Engine/Math.cs:28`.
- **PR wins** `operator:+(Vector3, Vector3)`; PR `public static Vector3 operator +(Vector3, Vector3)` at `runtime/Engine/Math.cs:42`; develop `public static Vector3 operator +(Vector3, Vector3)` at `runtime/Engine/Math.cs:24`.
- **PR wins** `operator:-(Vector3)`; PR `public static Vector3 operator -(Vector3)` at `runtime/Engine/Math.cs:44`; develop `public static Vector3 operator -(Vector3)` at `runtime/Engine/Math.cs:26`.
- **PR wins** `operator:-(Vector3, Vector3)`; PR `public static Vector3 operator -(Vector3, Vector3)` at `runtime/Engine/Math.cs:43`; develop `public static Vector3 operator -(Vector3, Vector3)` at `runtime/Engine/Math.cs:25`.
- **PR wins** `operator:/(Vector3, float)`; PR `public static Vector3 operator /(Vector3, float)` at `runtime/Engine/Math.cs:47`; develop `public static Vector3 operator /(Vector3, float)` at `runtime/Engine/Math.cs:29`.
- **PR wins** `property:Native`; PR `internal N.Vector3 Native` at `runtime/Engine/Math.cs:29`; develop `internal N.Vector3 Native` at `runtime/Engine/Math.cs:14`.
- **PR wins** `property:forward`; PR `public static Vector3 forward` at `runtime/Engine/Math.cs:37`; develop `public static Vector3 forward` at `runtime/Engine/Math.cs:20`.
- **PR wins** `property:magnitude`; PR `public float magnitude` at `runtime/Engine/Math.cs:40`; develop `public float magnitude` at `runtime/Engine/Math.cs:22`.
- **PR wins** `property:normalized`; PR `public Vector3 normalized` at `runtime/Engine/Math.cs:41`; develop `public Vector3 normalized` at `runtime/Engine/Math.cs:23`.
- **PR wins** `property:one`; PR `public static Vector3 one` at `runtime/Engine/Math.cs:32`; develop `public static Vector3 one` at `runtime/Engine/Math.cs:17`.
- **PR wins** `property:right`; PR `public static Vector3 right` at `runtime/Engine/Math.cs:33`; develop `public static Vector3 right` at `runtime/Engine/Math.cs:18`.
- **PR wins** `property:sqrMagnitude`; PR `public float sqrMagnitude` at `runtime/Engine/Math.cs:39`; develop `public float sqrMagnitude` at `runtime/Engine/Math.cs:21`.
- **PR wins** `property:up`; PR `public static Vector3 up` at `runtime/Engine/Math.cs:35`; develop `public static Vector3 up` at `runtime/Engine/Math.cs:19`.
- **PR wins** `property:zero`; PR `public static Vector3 zero` at `runtime/Engine/Math.cs:31`; develop `public static Vector3 zero` at `runtime/Engine/Math.cs:16`.


### 191. `UnityEngine.WaitForSeconds` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `WaitForSeconds`; partial `false`; `runtime/Engine/ClockInput.cs:42`

**develop declarations**:
- class; namespace `UnityEngine`; `WaitForSeconds`; partial `false`; `runtime/Engine/ClockInput.cs:42`

Common member signatures: **1**.
- **PR wins** `field:seconds`; PR `internal readonly field float seconds` at `runtime/Engine/ClockInput.cs:42`; develop `internal readonly field float seconds` at `runtime/Engine/ClockInput.cs:42`.


### 192. `UnityEngine.WaitForSecondsRealtime` — PR type wins

**PR declarations**:
- class; namespace `UnityEngine`; `WaitForSecondsRealtime`; partial `false`; `runtime/Engine/ClockInput.cs:43`

**develop declarations**:
- class; namespace `UnityEngine`; `WaitForSecondsRealtime`; partial `false`; `runtime/Engine/ClockInput.cs:43`

Common member signatures: **1**.
- **PR wins** `field:seconds`; PR `internal readonly field float seconds` at `runtime/Engine/ClockInput.cs:43`; develop `internal readonly field float seconds` at `runtime/Engine/ClockInput.cs:43`.


### 193. `WeaponBase` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `WeaponBase`; partial `false`; `source-reference/Original/ArmDependencies/WeaponBase.cs:8`
- class; namespace `(global)`; `WeaponBase`; partial `false`; `source-reference/Original/CharacterDependencies/WeaponBase.cs:8`

**develop declarations**:
- class; namespace `(global)`; `WeaponBase`; partial `false`; `source-reference/Original/ArmDependencies/WeaponBase.cs:8`

Common member signatures: **228**.
- **PR wins** `field:ComponentsPath`; PR `private static field string ComponentsPath` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:44`, `source-reference/Original/ArmDependencies/WeaponBase.cs:44`; develop `private static field string ComponentsPath` at `source-reference/Original/ArmDependencies/WeaponBase.cs:44`.
- **PR wins** `field:FreehandMaxAlignmentSpeed`; PR `private const field float FreehandMaxAlignmentSpeed` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:52`, `source-reference/Original/ArmDependencies/WeaponBase.cs:52`; develop `private const field float FreehandMaxAlignmentSpeed` at `source-reference/Original/ArmDependencies/WeaponBase.cs:52`.
- **PR wins** `field:FreehandMaxWeight`; PR `private const field float FreehandMaxWeight` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:60`, `source-reference/Original/ArmDependencies/WeaponBase.cs:60`; develop `private const field float FreehandMaxWeight` at `source-reference/Original/ArmDependencies/WeaponBase.cs:60`.
- **PR wins** `field:FreehandMinAlignmentSpeed`; PR `private const field float FreehandMinAlignmentSpeed` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:50`, `source-reference/Original/ArmDependencies/WeaponBase.cs:50`; develop `private const field float FreehandMinAlignmentSpeed` at `source-reference/Original/ArmDependencies/WeaponBase.cs:50`.
- **PR wins** `field:FreehandMinWeight`; PR `private const field float FreehandMinWeight` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:58`, `source-reference/Original/ArmDependencies/WeaponBase.cs:58`; develop `private const field float FreehandMinWeight` at `source-reference/Original/ArmDependencies/WeaponBase.cs:58`.
- **PR wins** `field:ShoulderedMaxAlignmentSpeed`; PR `private const field float ShoulderedMaxAlignmentSpeed` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:48`, `source-reference/Original/ArmDependencies/WeaponBase.cs:48`; develop `private const field float ShoulderedMaxAlignmentSpeed` at `source-reference/Original/ArmDependencies/WeaponBase.cs:48`.
- **PR wins** `field:ShoulderedMaxWeight`; PR `private const field float ShoulderedMaxWeight` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:56`, `source-reference/Original/ArmDependencies/WeaponBase.cs:56`; develop `private const field float ShoulderedMaxWeight` at `source-reference/Original/ArmDependencies/WeaponBase.cs:56`.
- **PR wins** `field:ShoulderedMinAlignmentSpeed`; PR `private const field float ShoulderedMinAlignmentSpeed` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:46`, `source-reference/Original/ArmDependencies/WeaponBase.cs:46`; develop `private const field float ShoulderedMinAlignmentSpeed` at `source-reference/Original/ArmDependencies/WeaponBase.cs:46`.
- **PR wins** `field:ShoulderedMinWeight`; PR `private const field float ShoulderedMinWeight` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:54`, `source-reference/Original/ArmDependencies/WeaponBase.cs:54`; develop `private const field float ShoulderedMinWeight` at `source-reference/Original/ArmDependencies/WeaponBase.cs:54`.
- **PR wins** `field:WeaponsPath`; PR `public static field string WeaponsPath` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:42`, `source-reference/Original/ArmDependencies/WeaponBase.cs:42`; develop `public static field string WeaponsPath` at `source-reference/Original/ArmDependencies/WeaponBase.cs:42`.
- **PR wins** `field:affectedByMuzzleBlastsMask`; PR `private field int affectedByMuzzleBlastsMask` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:314`, `source-reference/Original/ArmDependencies/WeaponBase.cs:314`; develop `private field int affectedByMuzzleBlastsMask` at `source-reference/Original/ArmDependencies/WeaponBase.cs:314`.
- **PR wins** `field:alignmentModifier`; PR `public field float alignmentModifier` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:99`, `source-reference/Original/ArmDependencies/WeaponBase.cs:99`; develop `public field float alignmentModifier` at `source-reference/Original/ArmDependencies/WeaponBase.cs:99`.
- **PR wins** `field:ammunition`; PR `public field AmmunitionBase ammunition` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:87`, `source-reference/Original/ArmDependencies/WeaponBase.cs:87`; develop `public field AmmunitionBase ammunition` at `source-reference/Original/ArmDependencies/WeaponBase.cs:87`.
- **PR wins** `field:armConfiguration`; PR `public field WeaponArmConfiguration armConfiguration` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:221`, `source-reference/Original/ArmDependencies/WeaponBase.cs:221`; develop `public field WeaponArmConfiguration armConfiguration` at `source-reference/Original/ArmDependencies/WeaponBase.cs:221`.
- **PR wins** `field:audioBoltExtract`; PR `public field PropagatingAudioSample audioBoltExtract` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:198`, `source-reference/Original/ArmDependencies/WeaponBase.cs:198`; develop `public field PropagatingAudioSample audioBoltExtract` at `source-reference/Original/ArmDependencies/WeaponBase.cs:198`.
- **PR wins** `field:audioBoltReturn`; PR `public field PropagatingAudioSample audioBoltReturn` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:200`, `source-reference/Original/ArmDependencies/WeaponBase.cs:200`; develop `public field PropagatingAudioSample audioBoltReturn` at `source-reference/Original/ArmDependencies/WeaponBase.cs:200`.
- **PR wins** `field:audioExtractMagazine`; PR `public field PropagatingAudioSample audioExtractMagazine` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:206`, `source-reference/Original/ArmDependencies/WeaponBase.cs:206`; develop `public field PropagatingAudioSample audioExtractMagazine` at `source-reference/Original/ArmDependencies/WeaponBase.cs:206`.
- **PR wins** `field:audioFire`; PR `public field PropagatingAudioSample audioFire` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:183`, `source-reference/Original/ArmDependencies/WeaponBase.cs:183`; develop `public field PropagatingAudioSample audioFire` at `source-reference/Original/ArmDependencies/WeaponBase.cs:183`.
- **PR wins** `field:audioFireSelect`; PR `public field PropagatingAudioSample audioFireSelect` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:204`, `source-reference/Original/ArmDependencies/WeaponBase.cs:204`; develop `public field PropagatingAudioSample audioFireSelect` at `source-reference/Original/ArmDependencies/WeaponBase.cs:204`.
- **PR wins** `field:audioFiringPin`; PR `public field PropagatingAudioSample audioFiringPin` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:202`, `source-reference/Original/ArmDependencies/WeaponBase.cs:202`; develop `public field PropagatingAudioSample audioFiringPin` at `source-reference/Original/ArmDependencies/WeaponBase.cs:202`.
- **PR wins** `field:audioInsertMagazine`; PR `public field PropagatingAudioSample audioInsertMagazine` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:208`, `source-reference/Original/ArmDependencies/WeaponBase.cs:208`; develop `public field PropagatingAudioSample audioInsertMagazine` at `source-reference/Original/ArmDependencies/WeaponBase.cs:208`.
- **PR wins** `field:audioInsertRound`; PR `public field PropagatingAudioSample audioInsertRound` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:210`, `source-reference/Original/ArmDependencies/WeaponBase.cs:210`; develop `public field PropagatingAudioSample audioInsertRound` at `source-reference/Original/ArmDependencies/WeaponBase.cs:210`.
- **PR wins** `field:audioPowerMultiplier`; PR `public field float audioPowerMultiplier` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:212`, `source-reference/Original/ArmDependencies/WeaponBase.cs:212`; develop `public field float audioPowerMultiplier` at `source-reference/Original/ArmDependencies/WeaponBase.cs:212`.
- **PR wins** `field:audioSupersonic`; PR `public field PropagatingAudioSample audioSupersonic` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:192`, `source-reference/Original/ArmDependencies/WeaponBase.cs:192`; develop `public field PropagatingAudioSample audioSupersonic` at `source-reference/Original/ArmDependencies/WeaponBase.cs:192`.
- **PR wins** `field:audioSuppressedSubsonic`; PR `public field PropagatingAudioSample audioSuppressedSubsonic` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:189`, `source-reference/Original/ArmDependencies/WeaponBase.cs:189`; develop `public field PropagatingAudioSample audioSuppressedSubsonic` at `source-reference/Original/ArmDependencies/WeaponBase.cs:189`.
- **PR wins** `field:audioSuppressedSupersonic`; PR `public field PropagatingAudioSample audioSuppressedSupersonic` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:186`, `source-reference/Original/ArmDependencies/WeaponBase.cs:186`; develop `public field PropagatingAudioSample audioSuppressedSupersonic` at `source-reference/Original/ArmDependencies/WeaponBase.cs:186`.
- **PR wins** `field:availability`; PR `public field Availability availability` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:62`, `source-reference/Original/ArmDependencies/WeaponBase.cs:62`; develop `public field Availability availability` at `source-reference/Original/ArmDependencies/WeaponBase.cs:62`.
- **PR wins** `field:barrel`; PR `public field WeaponBarrel barrel` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:167`, `source-reference/Original/ArmDependencies/WeaponBase.cs:167`; develop `public field WeaponBarrel barrel` at `source-reference/Original/ArmDependencies/WeaponBase.cs:167`.
- **PR wins** `field:boltCycle`; PR `public field AnimationCurve boltCycle` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:121`, `source-reference/Original/ArmDependencies/WeaponBase.cs:121`; develop `public field AnimationCurve boltCycle` at `source-reference/Original/ArmDependencies/WeaponBase.cs:121`.
- **PR wins** `field:boltFeedMalfunctionRate`; PR `public field float boltFeedMalfunctionRate` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:109`, `source-reference/Original/ArmDependencies/WeaponBase.cs:109`; develop `public field float boltFeedMalfunctionRate` at `source-reference/Original/ArmDependencies/WeaponBase.cs:109`.
- **PR wins** `field:boltHandleTransform`; PR `public field Transform boltHandleTransform` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:256`, `source-reference/Original/ArmDependencies/WeaponBase.cs:256`; develop `public field Transform boltHandleTransform` at `source-reference/Original/ArmDependencies/WeaponBase.cs:256`.
- **PR wins** `field:boltManipulationTransitionTime`; PR `public field float boltManipulationTransitionTime` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:230`, `source-reference/Original/ArmDependencies/WeaponBase.cs:230`; develop `public field float boltManipulationTransitionTime` at `source-reference/Original/ArmDependencies/WeaponBase.cs:230`.
- **PR wins** `field:boltReleaseAnimation`; PR `public field BoltReleaseAnimation boltReleaseAnimation` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:233`, `source-reference/Original/ArmDependencies/WeaponBase.cs:233`; develop `public field BoltReleaseAnimation boltReleaseAnimation` at `source-reference/Original/ArmDependencies/WeaponBase.cs:233`.
- **PR wins** `field:boltReleasePosition`; PR `public field Vector3 boltReleasePosition` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:242`, `source-reference/Original/ArmDependencies/WeaponBase.cs:242`; develop `public field Vector3 boltReleasePosition` at `source-reference/Original/ArmDependencies/WeaponBase.cs:242`.
- **PR wins** `field:boreDiameter`; PR `public field float boreDiameter` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:78`, `source-reference/Original/ArmDependencies/WeaponBase.cs:78`; develop `public field float boreDiameter` at `source-reference/Original/ArmDependencies/WeaponBase.cs:78`.
- **PR wins** `field:burstCount`; PR `public field int burstCount` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:160`, `source-reference/Original/ArmDependencies/WeaponBase.cs:160`; develop `public field int burstCount` at `source-reference/Original/ArmDependencies/WeaponBase.cs:160`.
- **PR wins** `field:burstRoundsRemaining`; PR `private field int burstRoundsRemaining` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:296`, `source-reference/Original/ArmDependencies/WeaponBase.cs:296`; develop `private field int burstRoundsRemaining` at `source-reference/Original/ArmDependencies/WeaponBase.cs:296`.
- **PR wins** `field:calibre`; PR `public field Calibre calibre` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:71`, `source-reference/Original/ArmDependencies/WeaponBase.cs:71`; develop `public field Calibre calibre` at `source-reference/Original/ArmDependencies/WeaponBase.cs:71`.
- **PR wins** `field:climbValue`; PR `private field float climbValue` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:302`, `source-reference/Original/ArmDependencies/WeaponBase.cs:302`; develop `private field float climbValue` at `source-reference/Original/ArmDependencies/WeaponBase.cs:302`.
- **PR wins** `field:colliderTransform`; PR `public field Transform colliderTransform` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:261`, `source-reference/Original/ArmDependencies/WeaponBase.cs:261`; develop `public field Transform colliderTransform` at `source-reference/Original/ArmDependencies/WeaponBase.cs:261`.
- **PR wins** `field:componentSlots`; PR `public field List<WeaponComponentSlot> componentSlots` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:90`, `source-reference/Original/ArmDependencies/WeaponBase.cs:90`; develop `public field List<WeaponComponentSlot> componentSlots` at `source-reference/Original/ArmDependencies/WeaponBase.cs:90`.
- **PR wins** `field:consecutiveRounds`; PR `private field int consecutiveRounds` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:298`, `source-reference/Original/ArmDependencies/WeaponBase.cs:298`; develop `private field int consecutiveRounds` at `source-reference/Original/ArmDependencies/WeaponBase.cs:298`.
- **PR wins** `field:cyclingBolt`; PR `public field CyclingBolt cyclingBolt` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:165`, `source-reference/Original/ArmDependencies/WeaponBase.cs:165`; develop `public field CyclingBolt cyclingBolt` at `source-reference/Original/ArmDependencies/WeaponBase.cs:165`.
- **PR wins** `field:defaultAmmunition`; PR `public field AmmunitionBase defaultAmmunition` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:84`, `source-reference/Original/ArmDependencies/WeaponBase.cs:84`; develop `public field AmmunitionBase defaultAmmunition` at `source-reference/Original/ArmDependencies/WeaponBase.cs:84`.
- **PR wins** `field:description`; PR `public field string description` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:264`, `source-reference/Original/ArmDependencies/WeaponBase.cs:264`; develop `public field string description` at `source-reference/Original/ArmDependencies/WeaponBase.cs:264`.
- **PR wins** `field:designation`; PR `public field string designation` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:64`, `source-reference/Original/ArmDependencies/WeaponBase.cs:64`; develop `public field string designation` at `source-reference/Original/ArmDependencies/WeaponBase.cs:64`.
- **PR wins** `field:detonationSource`; PR `public field AudioSource detonationSource` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:180`, `source-reference/Original/ArmDependencies/WeaponBase.cs:180`; develop `public field AudioSource detonationSource` at `source-reference/Original/ArmDependencies/WeaponBase.cs:180`.
- **PR wins** `field:direction`; PR `private field float direction` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:306`, `source-reference/Original/ArmDependencies/WeaponBase.cs:306`; develop `private field float direction` at `source-reference/Original/ArmDependencies/WeaponBase.cs:306`.
- **PR wins** `field:emitFlash`; PR `public field bool emitFlash` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:119`, `source-reference/Original/ArmDependencies/WeaponBase.cs:119`; develop `public field bool emitFlash` at `source-reference/Original/ArmDependencies/WeaponBase.cs:119`.
- **PR wins** `field:emitSmoke`; PR `public field bool emitSmoke` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:117`, `source-reference/Original/ArmDependencies/WeaponBase.cs:117`; develop `public field bool emitSmoke` at `source-reference/Original/ArmDependencies/WeaponBase.cs:117`.
- **PR wins** `field:feedMalfunction`; PR `public field float feedMalfunction` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:175`, `source-reference/Original/ArmDependencies/WeaponBase.cs:175`; develop `public field float feedMalfunction` at `source-reference/Original/ArmDependencies/WeaponBase.cs:175`.
- **PR wins** `field:fireFromMagazine`; PR `public field bool fireFromMagazine` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:169`, `source-reference/Original/ArmDependencies/WeaponBase.cs:169`; develop `public field bool fireFromMagazine` at `source-reference/Original/ArmDependencies/WeaponBase.cs:169`.
- **PR wins** `field:fireModeIndex`; PR `private field int fireModeIndex` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:294`, `source-reference/Original/ArmDependencies/WeaponBase.cs:294`; develop `private field int fireModeIndex` at `source-reference/Original/ArmDependencies/WeaponBase.cs:294`.
- **PR wins** `field:fireModes`; PR `public field List<FireMode> fireModes` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:157`, `source-reference/Original/ArmDependencies/WeaponBase.cs:157`; develop `public field List<FireMode> fireModes` at `source-reference/Original/ArmDependencies/WeaponBase.cs:157`.
- **PR wins** `field:hammer`; PR `public field WeaponHammer hammer` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:162`, `source-reference/Original/ArmDependencies/WeaponBase.cs:162`; develop `public field WeaponHammer hammer` at `source-reference/Original/ArmDependencies/WeaponBase.cs:162`.
- **PR wins** `field:hammerReady`; PR `private field bool hammerReady` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:312`, `source-reference/Original/ArmDependencies/WeaponBase.cs:312`; develop `private field bool hammerReady` at `source-reference/Original/ArmDependencies/WeaponBase.cs:312`.
- **PR wins** `field:handJoint`; PR `private field FixedJoint handJoint` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:292`, `source-reference/Original/ArmDependencies/WeaponBase.cs:292`; develop `private field FixedJoint handJoint` at `source-reference/Original/ArmDependencies/WeaponBase.cs:292`.
- **PR wins** `field:inaccuracy`; PR `public field float inaccuracy` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:144`, `source-reference/Original/ArmDependencies/WeaponBase.cs:144`; develop `public field float inaccuracy` at `source-reference/Original/ArmDependencies/WeaponBase.cs:144`.
- **PR wins** `field:initialized`; PR `private field bool initialized` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:310`, `source-reference/Original/ArmDependencies/WeaponBase.cs:310`; develop `private field bool initialized` at `source-reference/Original/ArmDependencies/WeaponBase.cs:310`.
- **PR wins** `field:interactionMarker`; PR `private field DynamicInteractionMarker interactionMarker` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:290`, `source-reference/Original/ArmDependencies/WeaponBase.cs:290`; develop `private field DynamicInteractionMarker interactionMarker` at `source-reference/Original/ArmDependencies/WeaponBase.cs:290`.
- **PR wins** `field:kickPenalty`; PR `public field float kickPenalty` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:154`, `source-reference/Original/ArmDependencies/WeaponBase.cs:154`; develop `public field float kickPenalty` at `source-reference/Original/ArmDependencies/WeaponBase.cs:154`.
- **PR wins** `field:kickValue`; PR `private field float kickValue` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:300`, `source-reference/Original/ArmDependencies/WeaponBase.cs:300`; develop `private field float kickValue` at `source-reference/Original/ArmDependencies/WeaponBase.cs:300`.
- **PR wins** `field:leftHandAnchor`; PR `public field Transform leftHandAnchor` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:249`, `source-reference/Original/ArmDependencies/WeaponBase.cs:249`; develop `public field Transform leftHandAnchor` at `source-reference/Original/ArmDependencies/WeaponBase.cs:249`.
- **PR wins** `field:leftHandBoltClose`; PR `public field bool leftHandBoltClose` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:239`, `source-reference/Original/ArmDependencies/WeaponBase.cs:239`; develop `public field bool leftHandBoltClose` at `source-reference/Original/ArmDependencies/WeaponBase.cs:239`.
- **PR wins** `field:leftHandBoltManipulation`; PR `public field bool leftHandBoltManipulation` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:236`, `source-reference/Original/ArmDependencies/WeaponBase.cs:236`; develop `public field bool leftHandBoltManipulation` at `source-reference/Original/ArmDependencies/WeaponBase.cs:236`.
- **PR wins** `field:lowerArmsRecoilFactor`; PR `public field float lowerArmsRecoilFactor` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:137`, `source-reference/Original/ArmDependencies/WeaponBase.cs:137`; develop `public field float lowerArmsRecoilFactor` at `source-reference/Original/ArmDependencies/WeaponBase.cs:137`.
- **PR wins** `field:magazineInsertTrack`; PR `public field Vector3 magazineInsertTrack` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:224`, `source-reference/Original/ArmDependencies/WeaponBase.cs:224`; develop `public field Vector3 magazineInsertTrack` at `source-reference/Original/ArmDependencies/WeaponBase.cs:224`.
- **PR wins** `field:magazineTransform`; PR `public field Transform magazineTransform` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:254`, `source-reference/Original/ArmDependencies/WeaponBase.cs:254`; develop `public field Transform magazineTransform` at `source-reference/Original/ArmDependencies/WeaponBase.cs:254`.
- **PR wins** `field:manualCycling`; PR `public field bool manualCycling` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:106`, `source-reference/Original/ArmDependencies/WeaponBase.cs:106`; develop `public field bool manualCycling` at `source-reference/Original/ArmDependencies/WeaponBase.cs:106`.
- **PR wins** `field:mechanicsSource`; PR `public field AudioSource mechanicsSource` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:195`, `source-reference/Original/ArmDependencies/WeaponBase.cs:195`; develop `public field AudioSource mechanicsSource` at `source-reference/Original/ArmDependencies/WeaponBase.cs:195`.
- **PR wins** `field:mission`; PR `private field Mission mission` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:288`, `source-reference/Original/ArmDependencies/WeaponBase.cs:288`; develop `private field Mission mission` at `source-reference/Original/ArmDependencies/WeaponBase.cs:288`.
- **PR wins** `field:movementPenalty`; PR `public field float movementPenalty` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:151`, `source-reference/Original/ArmDependencies/WeaponBase.cs:151`; develop `public field float movementPenalty` at `source-reference/Original/ArmDependencies/WeaponBase.cs:151`.
- **PR wins** `field:muzzleBlastRange`; PR `private field float muzzleBlastRange` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:316`, `source-reference/Original/ArmDependencies/WeaponBase.cs:316`; develop `private field float muzzleBlastRange` at `source-reference/Original/ArmDependencies/WeaponBase.cs:316`.
- **PR wins** `field:muzzleBlastRangeSquared`; PR `private field float muzzleBlastRangeSquared` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:318`, `source-reference/Original/ArmDependencies/WeaponBase.cs:318`; develop `private field float muzzleBlastRangeSquared` at `source-reference/Original/ArmDependencies/WeaponBase.cs:318`.
- **PR wins** `field:muzzleFlash`; PR `public field GameObject muzzleFlash` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:112`, `source-reference/Original/ArmDependencies/WeaponBase.cs:112`; develop `public field GameObject muzzleFlash` at `source-reference/Original/ArmDependencies/WeaponBase.cs:112`.
- **PR wins** `field:muzzleFlashScale`; PR `public field float muzzleFlashScale` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:115`, `source-reference/Original/ArmDependencies/WeaponBase.cs:115`; develop `public field float muzzleFlashScale` at `source-reference/Original/ArmDependencies/WeaponBase.cs:115`.
- **PR wins** `field:muzzleSmokeEmitter`; PR `private field ParticleSystem muzzleSmokeEmitter` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:280`, `source-reference/Original/ArmDependencies/WeaponBase.cs:280`; develop `private field ParticleSystem muzzleSmokeEmitter` at `source-reference/Original/ArmDependencies/WeaponBase.cs:280`.
- **PR wins** `field:muzzleTransform`; PR `public field Transform muzzleTransform` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:252`, `source-reference/Original/ArmDependencies/WeaponBase.cs:252`; develop `public field Transform muzzleTransform` at `source-reference/Original/ArmDependencies/WeaponBase.cs:252`.
- **PR wins** `field:onSpawnProjectile`; PR `public field ProjectileEvent onSpawnProjectile` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:270`, `source-reference/Original/ArmDependencies/WeaponBase.cs:270`; develop `public field ProjectileEvent onSpawnProjectile` at `source-reference/Original/ArmDependencies/WeaponBase.cs:270`.
- **PR wins** `field:origin`; PR `public field string origin` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:268`, `source-reference/Original/ArmDependencies/WeaponBase.cs:268`; develop `public field string origin` at `source-reference/Original/ArmDependencies/WeaponBase.cs:268`.
- **PR wins** `field:projectileForceMultiplier`; PR `private field float projectileForceMultiplier` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:274`, `source-reference/Original/ArmDependencies/WeaponBase.cs:274`; develop `private field float projectileForceMultiplier` at `source-reference/Original/ArmDependencies/WeaponBase.cs:274`.
- **PR wins** `field:projectilesContainer`; PR `private field Transform projectilesContainer` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:282`, `source-reference/Original/ArmDependencies/WeaponBase.cs:282`; develop `private field Transform projectilesContainer` at `source-reference/Original/ArmDependencies/WeaponBase.cs:282`.
- **PR wins** `field:rangePenalty`; PR `public field AnimationCurve rangePenalty` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:148`, `source-reference/Original/ArmDependencies/WeaponBase.cs:148`; develop `public field AnimationCurve rangePenalty` at `source-reference/Original/ArmDependencies/WeaponBase.cs:148`.
- **PR wins** `field:raycastHit`; PR `private field RaycastHit raycastHit` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:284`, `source-reference/Original/ArmDependencies/WeaponBase.cs:284`; develop `private field RaycastHit raycastHit` at `source-reference/Original/ArmDependencies/WeaponBase.cs:284`.
- **PR wins** `field:rearSightPosition`; PR `public field Vector3 rearSightPosition` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:227`, `source-reference/Original/ArmDependencies/WeaponBase.cs:227`; develop `public field Vector3 rearSightPosition` at `source-reference/Original/ArmDependencies/WeaponBase.cs:227`.
- **PR wins** `field:recoilClimbCycle`; PR `public field AnimationCurve recoilClimbCycle` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:128`, `source-reference/Original/ArmDependencies/WeaponBase.cs:128`; develop `public field AnimationCurve recoilClimbCycle` at `source-reference/Original/ArmDependencies/WeaponBase.cs:128`.
- **PR wins** `field:recoilClimbModifier`; PR `public field float recoilClimbModifier` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:134`, `source-reference/Original/ArmDependencies/WeaponBase.cs:134`; develop `public field float recoilClimbModifier` at `source-reference/Original/ArmDependencies/WeaponBase.cs:134`.
- **PR wins** `field:recoilCycle`; PR `public field AnimationCurve recoilCycle` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:125`, `source-reference/Original/ArmDependencies/WeaponBase.cs:125`; develop `public field AnimationCurve recoilCycle` at `source-reference/Original/ArmDependencies/WeaponBase.cs:125`.
- **PR wins** `field:recoilKickModifier`; PR `public field float recoilKickModifier` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:131`, `source-reference/Original/ArmDependencies/WeaponBase.cs:131`; develop `public field float recoilKickModifier` at `source-reference/Original/ArmDependencies/WeaponBase.cs:131`.
- **PR wins** `field:recoilList`; PR `private field List<Recoil> recoilList` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:278`, `source-reference/Original/ArmDependencies/WeaponBase.cs:278`; develop `private field List<Recoil> recoilList` at `source-reference/Original/ArmDependencies/WeaponBase.cs:278`.
- **PR wins** `field:recoilPeriod`; PR `private field float recoilPeriod` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:178`, `source-reference/Original/ArmDependencies/WeaponBase.cs:178`; develop `private field float recoilPeriod` at `source-reference/Original/ArmDependencies/WeaponBase.cs:178`.
- **PR wins** `field:recoilSuppressorModifier`; PR `private field float recoilSuppressorModifier` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:276`, `source-reference/Original/ArmDependencies/WeaponBase.cs:276`; develop `private field float recoilSuppressorModifier` at `source-reference/Original/ArmDependencies/WeaponBase.cs:276`.
- **PR wins** `field:recoilToWeightFactor`; PR `private field float recoilToWeightFactor` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:308`, `source-reference/Original/ArmDependencies/WeaponBase.cs:308`; develop `private field float recoilToWeightFactor` at `source-reference/Original/ArmDependencies/WeaponBase.cs:308`.
- **PR wins** `field:reloadTime`; PR `public field float reloadTime` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:103`, `source-reference/Original/ArmDependencies/WeaponBase.cs:103`; develop `public field float reloadTime` at `source-reference/Original/ArmDependencies/WeaponBase.cs:103`.
- **PR wins** `field:rifleTwistRate`; PR `public field float rifleTwistRate` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:81`, `source-reference/Original/ArmDependencies/WeaponBase.cs:81`; develop `public field float rifleTwistRate` at `source-reference/Original/ArmDependencies/WeaponBase.cs:81`.
- **PR wins** `field:rifledBarrel`; PR `public field bool rifledBarrel` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:76`, `source-reference/Original/ArmDependencies/WeaponBase.cs:76`; develop `public field bool rifledBarrel` at `source-reference/Original/ArmDependencies/WeaponBase.cs:76`.
- **PR wins** `field:rightHandAnchor`; PR `public field Transform rightHandAnchor` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:245`, `source-reference/Original/ArmDependencies/WeaponBase.cs:245`; develop `public field Transform rightHandAnchor` at `source-reference/Original/ArmDependencies/WeaponBase.cs:245`.
- **PR wins** `field:roundsPerMinute`; PR `public field float roundsPerMinute` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:73`, `source-reference/Original/ArmDependencies/WeaponBase.cs:73`; develop `public field float roundsPerMinute` at `source-reference/Original/ArmDependencies/WeaponBase.cs:73`.
- **PR wins** `field:settings`; PR `private field Settings settings` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:272`, `source-reference/Original/ArmDependencies/WeaponBase.cs:272`; develop `private field Settings settings` at `source-reference/Original/ArmDependencies/WeaponBase.cs:272`.
- **PR wins** `field:slotsTransform`; PR `public field Transform slotsTransform` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:259`, `source-reference/Original/ArmDependencies/WeaponBase.cs:259`; develop `public field Transform slotsTransform` at `source-reference/Original/ArmDependencies/WeaponBase.cs:259`.
- **PR wins** `field:staticWeight`; PR `private field float staticWeight` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:304`, `source-reference/Original/ArmDependencies/WeaponBase.cs:304`; develop `private field float staticWeight` at `source-reference/Original/ArmDependencies/WeaponBase.cs:304`.
- **PR wins** `field:suppressedMixerGroup`; PR `public field AudioMixerGroup suppressedMixerGroup` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:218`, `source-reference/Original/ArmDependencies/WeaponBase.cs:218`; develop `public field AudioMixerGroup suppressedMixerGroup` at `source-reference/Original/ArmDependencies/WeaponBase.cs:218`.
- **PR wins** `field:timeScaler`; PR `private field TimeScaler timeScaler` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:286`, `source-reference/Original/ArmDependencies/WeaponBase.cs:286`; develop `private field TimeScaler timeScaler` at `source-reference/Original/ArmDependencies/WeaponBase.cs:286`.
- **PR wins** `field:type`; PR `public field WeaponBrace type` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:69`, `source-reference/Original/ArmDependencies/WeaponBase.cs:69`; develop `public field WeaponBrace type` at `source-reference/Original/ArmDependencies/WeaponBase.cs:69`.
- **PR wins** `field:unsuppressedMixerGroup`; PR `public field AudioMixerGroup unsuppressedMixerGroup` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:215`, `source-reference/Original/ArmDependencies/WeaponBase.cs:215`; develop `public field AudioMixerGroup unsuppressedMixerGroup` at `source-reference/Original/ArmDependencies/WeaponBase.cs:215`.
- **PR wins** `field:upperArmsRecoilFactor`; PR `public field float upperArmsRecoilFactor` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:141`, `source-reference/Original/ArmDependencies/WeaponBase.cs:141`; develop `public field float upperArmsRecoilFactor` at `source-reference/Original/ArmDependencies/WeaponBase.cs:141`.
- **PR wins** `field:useInternalMagazine`; PR `public field bool useInternalMagazine` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:172`, `source-reference/Original/ArmDependencies/WeaponBase.cs:172`; develop `public field bool useInternalMagazine` at `source-reference/Original/ArmDependencies/WeaponBase.cs:172`.
- **PR wins** `field:velocityModifier`; PR `public field float velocityModifier` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:95`, `source-reference/Original/ArmDependencies/WeaponBase.cs:95`; develop `public field float velocityModifier` at `source-reference/Original/ArmDependencies/WeaponBase.cs:95`.
- **PR wins** `field:weaponType`; PR `public field WeaponType weaponType` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:67`, `source-reference/Original/ArmDependencies/WeaponBase.cs:67`; develop `public field WeaponType weaponType` at `source-reference/Original/ArmDependencies/WeaponBase.cs:67`.
- **PR wins** `field:weight`; PR `public field float weight` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:92`, `source-reference/Original/ArmDependencies/WeaponBase.cs:92`; develop `public field float weight` at `source-reference/Original/ArmDependencies/WeaponBase.cs:92`.
- **PR wins** ``method:Activate`0(CharacterBody)``; PR `public void Activate(CharacterBody)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:880`, `source-reference/Original/ArmDependencies/WeaponBase.cs:880`; develop `public void Activate(CharacterBody)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:880`.
- **PR wins** ``method:AddFixedJoint`0(Transform, float)``; PR `private void AddFixedJoint(Transform, float)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1414`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1414`; develop `private void AddFixedJoint(Transform, float)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1414`.
- **PR wins** ``method:AddSingleRecoil`0()``; PR `private void AddSingleRecoil()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1460`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1460`; develop `private void AddSingleRecoil()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1460`.
- **PR wins** ``method:ApplyExtraSettings`0(SerializableExtraSettings)``; PR `private void ApplyExtraSettings(SerializableExtraSettings)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:620`, `source-reference/Original/ArmDependencies/WeaponBase.cs:620`; develop `private void ApplyExtraSettings(SerializableExtraSettings)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:620`.
- **PR wins** ``method:ApplyMuzzleBlastForces`0()``; PR `private void ApplyMuzzleBlastForces()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1162`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1162`; develop `private void ApplyMuzzleBlastForces()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1162`.
- **PR wins** ``method:AssignComponentFunctions`0(WeaponBase, WeaponComponent)``; PR `public static void AssignComponentFunctions(WeaponBase, WeaponComponent)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:927`, `source-reference/Original/ArmDependencies/WeaponBase.cs:927`; develop `public static void AssignComponentFunctions(WeaponBase, WeaponComponent)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:927`.
- **PR wins** ``method:AttachForPeriod`0(CharacterBody, float)``; PR `public void AttachForPeriod(CharacterBody, float)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1408`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1408`; develop `public void AttachForPeriod(CharacterBody, float)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1408`.
- **PR wins** ``method:Awake`0()``; PR `private void Awake()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:635`, `source-reference/Original/ArmDependencies/WeaponBase.cs:635`; develop `private void Awake()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:635`.
- **PR wins** ``method:CalculateRecoil`0()``; PR `private void CalculateRecoil()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1430`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1430`; develop `private void CalculateRecoil()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1430`.
- **PR wins** ``method:CalculateStaticWeight`0()``; PR `public void CalculateStaticWeight()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:737`, `source-reference/Original/ArmDependencies/WeaponBase.cs:737`; develop `public void CalculateStaticWeight()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:737`.
- **PR wins** ``method:CycleFireModes`0()``; PR `public void CycleFireModes()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1342`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1342`; develop `public void CycleFireModes()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1342`.
- **PR wins** ``method:DefaultAmmunition`0(string)``; PR `public static AmmunitionBase DefaultAmmunition(string)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1583`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1583`; develop `public static AmmunitionBase DefaultAmmunition(string)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1583`.
- **PR wins** ``method:Deserialize`0(SerializableWeapon, bool)``; PR `public static WeaponBase Deserialize(SerializableWeapon, bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:797`, `source-reference/Original/ArmDependencies/WeaponBase.cs:797`; develop `public static WeaponBase Deserialize(SerializableWeapon, bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:797`.
- **PR wins** ``method:DestroyJoint`0()``; PR `private void DestroyJoint()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1421`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1421`; develop `private void DestroyJoint()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1421`.
- **PR wins** ``method:DetonateRound`0()``; PR `private void DetonateRound()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1129`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1129`; develop `private void DetonateRound()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1129`.
- **PR wins** ``method:DetonationAudioPower`0(AmmunitionBase, WeaponComponentSuppressor, bool)``; PR `public float DetonationAudioPower(AmmunitionBase, WeaponComponentSuppressor, bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1220`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1220`; develop `public float DetonationAudioPower(AmmunitionBase, WeaponComponentSuppressor, bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1220`.
- **PR wins** ``method:EmitMuzzleAudio`0(bool)``; PR `private void EmitMuzzleAudio(bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1227`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1227`; develop `private void EmitMuzzleAudio(bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1227`.
- **PR wins** ``method:EmitMuzzleFlash`0()``; PR `private void EmitMuzzleFlash()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1265`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1265`; develop `private void EmitMuzzleFlash()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1265`.
- **PR wins** ``method:EmitMuzzleSmoke`0()``; PR `private void EmitMuzzleSmoke()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1204`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1204`; develop `private void EmitMuzzleSmoke()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1204`.
- **PR wins** ``method:EnableLaser`0(bool)``; PR `public void EnableLaser(bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1318`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1318`; develop `public void EnableLaser(bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1318`.
- **PR wins** ``method:EnableLight`0(bool)``; PR `public void EnableLight(bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1334`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1334`; develop `public void EnableLight(bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1334`.
- **PR wins** ``method:FindCompatibleComponents`0()``; PR `public List<WeaponComponent> FindCompatibleComponents()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1542`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1542`; develop `public List<WeaponComponent> FindCompatibleComponents()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1542`.
- **PR wins** ``method:FireLineStart`0(bool)``; PR `public Vector3 FireLineStart(bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1578`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1578`; develop `public Vector3 FireLineStart(bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1578`.
- **PR wins** ``method:GetDirection`0()``; PR `public float GetDirection()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1079`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1079`; develop `public float GetDirection()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1079`.
- **PR wins** ``method:GetNextRoundFromMagazine`0(bool)``; PR `public Casing GetNextRoundFromMagazine(bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1556`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1556`; develop `public Casing GetNextRoundFromMagazine(bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1556`.
- **PR wins** ``method:Holster`0()``; PR `public void Holster()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:889`, `source-reference/Original/ArmDependencies/WeaponBase.cs:889`; develop `public void Holster()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:889`.
- **PR wins** ``method:InitializeAmmunition`0()``; PR `private void InitializeAmmunition()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:717`, `source-reference/Original/ArmDependencies/WeaponBase.cs:717`; develop `private void InitializeAmmunition()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:717`.
- **PR wins** ``method:InitializeAudio`0()``; PR `private void InitializeAudio()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:694`, `source-reference/Original/ArmDependencies/WeaponBase.cs:694`; develop `private void InitializeAudio()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:694`.
- **PR wins** ``method:InitializeForUI`0()``; PR `public void InitializeForUI()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:774`, `source-reference/Original/ArmDependencies/WeaponBase.cs:774`; develop `public void InitializeForUI()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:774`.
- **PR wins** ``method:InitializeSettings`0()``; PR `private void InitializeSettings()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:613`, `source-reference/Original/ArmDependencies/WeaponBase.cs:613`; develop `private void InitializeSettings()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:613`.
- **PR wins** ``method:Initialize`0(CharacterBody)``; PR `public void Initialize(CharacterBody)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:750`, `source-reference/Original/ArmDependencies/WeaponBase.cs:750`; develop `public void Initialize(CharacterBody)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:750`.
- **PR wins** ``method:InsertMagazine`0(Magazine)``; PR `public bool InsertMagazine(Magazine)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1024`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1024`; develop `public bool InsertMagazine(Magazine)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1024`.
- **PR wins** ``method:InsertRoundIntoOpenBolt`0()``; PR `public void InsertRoundIntoOpenBolt()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1013`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1013`; develop `public void InsertRoundIntoOpenBolt()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1013`.
- **PR wins** ``method:InsertRound`0()``; PR `public bool InsertRound()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:995`, `source-reference/Original/ArmDependencies/WeaponBase.cs:995`; develop `public bool InsertRound()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:995`.
- **PR wins** ``method:InstantiateComponent`0(WeaponBase, GameObject, WeaponComponentSlot)``; PR `public static void InstantiateComponent(WeaponBase, GameObject, WeaponComponentSlot)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:847`, `source-reference/Original/ArmDependencies/WeaponBase.cs:847`; develop `public static void InstantiateComponent(WeaponBase, GameObject, WeaponComponentSlot)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:847`.
- **PR wins** ``method:InvokeBulletTime`0()``; PR `private void InvokeBulletTime()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1189`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1189`; develop `private void InvokeBulletTime()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1189`.
- **PR wins** ``method:Length`0()``; PR `public float Length()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1551`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1551`; develop `public float Length()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1551`.
- **PR wins** ``method:MagazineWeight`0()``; PR `private float MagazineWeight()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:858`, `source-reference/Original/ArmDependencies/WeaponBase.cs:858`; develop `private float MagazineWeight()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:858`.
- **PR wins** ``method:MovementFactor`0()``; PR `private float MovementFactor()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1084`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1084`; develop `private float MovementFactor()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1084`.
- **PR wins** ``method:OnBoltClosed`0(bool)``; PR `public void OnBoltClosed(bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1257`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1257`; develop `public void OnBoltClosed(bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1257`.
- **PR wins** ``method:OnBoltOpened`0()``; PR `public void OnBoltOpened()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1243`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1243`; develop `public void OnBoltOpened()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1243`.
- **PR wins** ``method:OnDisable`0()``; PR `private void OnDisable()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:658`, `source-reference/Original/ArmDependencies/WeaponBase.cs:658`; develop `private void OnDisable()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:658`.
- **PR wins** ``method:PickUpInitialize`0(CharacterBody, bool)``; PR `public void PickUpInitialize(CharacterBody, bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:872`, `source-reference/Original/ArmDependencies/WeaponBase.cs:872`; develop `public void PickUpInitialize(CharacterBody, bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:872`.
- **PR wins** ``method:PopulateRequiredComponentsForUI`0()``; PR `public void PopulateRequiredComponentsForUI()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:671`, `source-reference/Original/ArmDependencies/WeaponBase.cs:671`; develop `public void PopulateRequiredComponentsForUI()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:671`.
- **PR wins** ``method:RecoilImpulse`0()``; PR `public float RecoilImpulse()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1455`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1455`; develop `public float RecoilImpulse()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1455`.
- **PR wins** ``method:RefreshComponentFunctionsForUI`0()``; PR `public void RefreshComponentFunctionsForUI()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:686`, `source-reference/Original/ArmDependencies/WeaponBase.cs:686`; develop `public void RefreshComponentFunctionsForUI()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:686`.
- **PR wins** ``method:ReleaseFiringPin`0()``; PR `private void ReleaseFiringPin()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1097`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1097`; develop `private void ReleaseFiringPin()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1097`.
- **PR wins** ``method:ReleaseFromOwner`0()``; PR `public void ReleaseFromOwner()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1395`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1395`; develop `public void ReleaseFromOwner()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1395`.
- **PR wins** ``method:RemoveInteractionMarker`0()``; PR `private void RemoveInteractionMarker()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:663`, `source-reference/Original/ArmDependencies/WeaponBase.cs:663`; develop `private void RemoveInteractionMarker()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:663`.
- **PR wins** ``method:RemoveMagazine`0(bool, bool)``; PR `public void RemoveMagazine(bool, bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1042`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1042`; develop `public void RemoveMagazine(bool, bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1042`.
- **PR wins** ``method:RenderBounds`0()``; PR `public Rect RenderBounds()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1486`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1486`; develop `public Rect RenderBounds()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1486`.
- **PR wins** ``method:RequiredCleareance`0(float)``; PR `public float RequiredCleareance(float)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1569`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1569`; develop `public float RequiredCleareance(float)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1569`.
- **PR wins** ``method:ResetBurstCount`0()``; PR `public void ResetBurstCount()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1156`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1156`; develop `public void ResetBurstCount()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1156`.
- **PR wins** ``method:ReturnMagazine`0()``; PR `public void ReturnMagazine()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1019`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1019`; develop `public void ReturnMagazine()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1019`.
- **PR wins** ``method:Serialize`0()``; PR `public SerializableWeapon Serialize()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:783`, `source-reference/Original/ArmDependencies/WeaponBase.cs:783`; develop `public SerializableWeapon Serialize()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:783`.
- **PR wins** ``method:SetFireMode`0(FireMode, bool)``; PR `public void SetFireMode(FireMode, bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1356`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1356`; develop `public void SetFireMode(FireMode, bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1356`.
- **PR wins** ``method:SetKinematic`0()``; PR `public void SetKinematic()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1373`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1373`; develop `public void SetKinematic()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1373`.
- **PR wins** ``method:SetNonKinematic`0()``; PR `public void SetNonKinematic()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1379`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1379`; develop `public void SetNonKinematic()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1379`.
- **PR wins** ``method:ShowInteractionMarker`0(bool)``; PR `public void ShowInteractionMarker(bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1470`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1470`; develop `public void ShowInteractionMarker(bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1470`.
- **PR wins** ``method:SpawnProjectiles`0()``; PR `private void SpawnProjectiles()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1277`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1277`; develop `private void SpawnProjectiles()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1277`.
- **PR wins** ``method:Start`0()``; PR `private void Start()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:645`, `source-reference/Original/ArmDependencies/WeaponBase.cs:645`; develop `private void Start()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:645`.
- **PR wins** ``method:StrikeFiringPin`0()``; PR `public void StrikeFiringPin()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1112`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1112`; develop `public void StrikeFiringPin()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1112`.
- **PR wins** ``method:SuspendComponents`0(bool)``; PR `public void SuspendComponents(bool)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:897`, `source-reference/Original/ArmDependencies/WeaponBase.cs:897`; develop `public void SuspendComponents(bool)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:897`.
- **PR wins** ``method:ToggleLaser`0()``; PR `public void ToggleLaser()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1310`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1310`; develop `public void ToggleLaser()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1310`.
- **PR wins** ``method:ToggleLight`0()``; PR `public void ToggleLight()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1326`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1326`; develop `public void ToggleLight()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1326`.
- **PR wins** ``method:Turn`0(float)``; PR `public void Turn(float)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:1074`, `source-reference/Original/ArmDependencies/WeaponBase.cs:1074`; develop `public void Turn(float)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:1074`.
- **PR wins** ``method:UpdateBlockedStatus`0()``; PR `private void UpdateBlockedStatus()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:977`, `source-reference/Original/ArmDependencies/WeaponBase.cs:977`; develop `private void UpdateBlockedStatus()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:977`.
- **PR wins** ``method:UpdateTransform`0()``; PR `private void UpdateTransform()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:989`, `source-reference/Original/ArmDependencies/WeaponBase.cs:989`; develop `private void UpdateTransform()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:989`.
- **PR wins** ``method:UpdateTriggerMechanism`0()``; PR `private void UpdateTriggerMechanism()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:954`, `source-reference/Original/ArmDependencies/WeaponBase.cs:954`; develop `private void UpdateTriggerMechanism()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:954`.
- **PR wins** ``method:Update`0()``; PR `private void Update()` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:909`, `source-reference/Original/ArmDependencies/WeaponBase.cs:909`; develop `private void Update()` at `source-reference/Original/ArmDependencies/WeaponBase.cs:909`.
- **PR wins** ``method:WeaponTypeToString`0(WeaponType)``; PR `public static string WeaponTypeToString(WeaponType)` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:596`, `source-reference/Original/ArmDependencies/WeaponBase.cs:596`; develop `public static string WeaponTypeToString(WeaponType)` at `source-reference/Original/ArmDependencies/WeaponBase.cs:596`.
- **PR wins** `property:ActiveFireMode`; PR `public FireMode ActiveFireMode` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:404`, `source-reference/Original/ArmDependencies/WeaponBase.cs:404`; develop `public FireMode ActiveFireMode` at `source-reference/Original/ArmDependencies/WeaponBase.cs:404`.
- **PR wins** `property:AlignmentSpeed`; PR `public float AlignmentSpeed` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:354`, `source-reference/Original/ArmDependencies/WeaponBase.cs:354`; develop `public float AlignmentSpeed` at `source-reference/Original/ArmDependencies/WeaponBase.cs:354`.
- **PR wins** `property:AlignmentSpeedNormalized`; PR `public float AlignmentSpeedNormalized` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:366`, `source-reference/Original/ArmDependencies/WeaponBase.cs:366`; develop `public float AlignmentSpeedNormalized` at `source-reference/Original/ArmDependencies/WeaponBase.cs:366`.
- **PR wins** `property:Angle`; PR `public float Angle` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:422`, `source-reference/Original/ArmDependencies/WeaponBase.cs:422`; develop `public float Angle` at `source-reference/Original/ArmDependencies/WeaponBase.cs:422`.
- **PR wins** `property:AngleWithClimb`; PR `public float AngleWithClimb` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:440`, `source-reference/Original/ArmDependencies/WeaponBase.cs:440`; develop `public float AngleWithClimb` at `source-reference/Original/ArmDependencies/WeaponBase.cs:440`.
- **PR wins** `property:BoltClosingPeriod`; PR `public float BoltClosingPeriod` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:336`, `source-reference/Original/ArmDependencies/WeaponBase.cs:336`; develop `public float BoltClosingPeriod` at `source-reference/Original/ArmDependencies/WeaponBase.cs:336`.
- **PR wins** `property:BoltCyclePeriod`; PR `public float BoltCyclePeriod` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:320`, `source-reference/Original/ArmDependencies/WeaponBase.cs:320`; develop `public float BoltCyclePeriod` at `source-reference/Original/ArmDependencies/WeaponBase.cs:320`.
- **PR wins** `property:BoltOpeningPeriod`; PR `public float BoltOpeningPeriod` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:328`, `source-reference/Original/ArmDependencies/WeaponBase.cs:328`; develop `public float BoltOpeningPeriod` at `source-reference/Original/ArmDependencies/WeaponBase.cs:328`.
- **PR wins** `property:BoltStatus`; PR `public CyclingBolt.State BoltStatus` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:394`, `source-reference/Original/ArmDependencies/WeaponBase.cs:394`; develop `public CyclingBolt.State BoltStatus` at `source-reference/Original/ArmDependencies/WeaponBase.cs:394`.
- **PR wins** `property:BorePoint`; PR `public Vector3 BorePoint` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:534`, `source-reference/Original/ArmDependencies/WeaponBase.cs:534`; develop `public Vector3 BorePoint` at `source-reference/Original/ArmDependencies/WeaponBase.cs:534`.
- **PR wins** `property:CasingsContainer`; PR `public Transform CasingsContainer` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:344`, `source-reference/Original/ArmDependencies/WeaponBase.cs:344`; develop `public Transform CasingsContainer` at `source-reference/Original/ArmDependencies/WeaponBase.cs:344`.
- **PR wins** `property:ChamberPermanentlyEmpty`; PR `public bool ChamberPermanentlyEmpty` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:498`, `source-reference/Original/ArmDependencies/WeaponBase.cs:498`; develop `public bool ChamberPermanentlyEmpty` at `source-reference/Original/ArmDependencies/WeaponBase.cs:498`.
- **PR wins** `property:ChamberStatus`; PR `public ChamberState ChamberStatus` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:412`, `source-reference/Original/ArmDependencies/WeaponBase.cs:412`; develop `public ChamberState ChamberStatus` at `source-reference/Original/ArmDependencies/WeaponBase.cs:412`.
- **PR wins** `property:CollisionClearance`; PR `public float CollisionClearance` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:584`, `source-reference/Original/ArmDependencies/WeaponBase.cs:584`; develop `public float CollisionClearance` at `source-reference/Original/ArmDependencies/WeaponBase.cs:584`.
- **PR wins** `property:CombinedInaccuracy`; PR `public float CombinedInaccuracy` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:576`, `source-reference/Original/ArmDependencies/WeaponBase.cs:576`; develop `public float CombinedInaccuracy` at `source-reference/Original/ArmDependencies/WeaponBase.cs:576`.
- **PR wins** `property:ComponentFlashlight`; PR `public WeaponComponentFlashlight ComponentFlashlight` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:588`, `source-reference/Original/ArmDependencies/WeaponBase.cs:588`; develop `public WeaponComponentFlashlight ComponentFlashlight` at `source-reference/Original/ArmDependencies/WeaponBase.cs:588`.
- **PR wins** `property:ComponentLaser`; PR `public WeaponComponentLaser ComponentLaser` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:590`, `source-reference/Original/ArmDependencies/WeaponBase.cs:590`; develop `public WeaponComponentLaser ComponentLaser` at `source-reference/Original/ArmDependencies/WeaponBase.cs:590`.
- **PR wins** `property:ComponentMagazine`; PR `public WeaponComponentMagazine ComponentMagazine` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:586`, `source-reference/Original/ArmDependencies/WeaponBase.cs:586`; develop `public WeaponComponentMagazine ComponentMagazine` at `source-reference/Original/ArmDependencies/WeaponBase.cs:586`.
- **PR wins** `property:ComponentSuppressor`; PR `public WeaponComponentSuppressor ComponentSuppressor` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:592`, `source-reference/Original/ArmDependencies/WeaponBase.cs:592`; develop `public WeaponComponentSuppressor ComponentSuppressor` at `source-reference/Original/ArmDependencies/WeaponBase.cs:592`.
- **PR wins** `property:HammerReady`; PR `public bool HammerReady` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:460`, `source-reference/Original/ArmDependencies/WeaponBase.cs:460`; develop `public bool HammerReady` at `source-reference/Original/ArmDependencies/WeaponBase.cs:460`.
- **PR wins** `property:IsBlocked`; PR `public bool IsBlocked` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:448`, `source-reference/Original/ArmDependencies/WeaponBase.cs:448`; develop `public bool IsBlocked` at `source-reference/Original/ArmDependencies/WeaponBase.cs:448`.
- **PR wins** `property:IsCycling`; PR `public bool IsCycling` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:506`, `source-reference/Original/ArmDependencies/WeaponBase.cs:506`; develop `public bool IsCycling` at `source-reference/Original/ArmDependencies/WeaponBase.cs:506`.
- **PR wins** `property:IsFiring`; PR `public bool IsFiring` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:450`, `source-reference/Original/ArmDependencies/WeaponBase.cs:450`; develop `public bool IsFiring` at `source-reference/Original/ArmDependencies/WeaponBase.cs:450`.
- **PR wins** `property:IsHeld`; PR `public bool IsHeld` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:484`, `source-reference/Original/ArmDependencies/WeaponBase.cs:484`; develop `public bool IsHeld` at `source-reference/Original/ArmDependencies/WeaponBase.cs:484`.
- **PR wins** `property:IsHolstered`; PR `public bool IsHolstered` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:486`, `source-reference/Original/ArmDependencies/WeaponBase.cs:486`; develop `public bool IsHolstered` at `source-reference/Original/ArmDependencies/WeaponBase.cs:486`.
- **PR wins** `property:IsPrimary`; PR `public bool IsPrimary` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:458`, `source-reference/Original/ArmDependencies/WeaponBase.cs:458`; develop `public bool IsPrimary` at `source-reference/Original/ArmDependencies/WeaponBase.cs:458`.
- **PR wins** `property:IsRagdoll`; PR `public bool IsRagdoll` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:488`, `source-reference/Original/ArmDependencies/WeaponBase.cs:488`; develop `public bool IsRagdoll` at `source-reference/Original/ArmDependencies/WeaponBase.cs:488`.
- **PR wins** `property:Magazine`; PR `public Magazine Magazine` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:402`, `source-reference/Original/ArmDependencies/WeaponBase.cs:402`; develop `public Magazine Magazine` at `source-reference/Original/ArmDependencies/WeaponBase.cs:402`.
- **PR wins** `property:MalfunctionsMultiplier`; PR `public float MalfunctionsMultiplier` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:522`, `source-reference/Original/ArmDependencies/WeaponBase.cs:522`; develop `public float MalfunctionsMultiplier` at `source-reference/Original/ArmDependencies/WeaponBase.cs:522`.
- **PR wins** `property:Muzzle`; PR `public Transform Muzzle` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:376`, `source-reference/Original/ArmDependencies/WeaponBase.cs:376`; develop `public Transform Muzzle` at `source-reference/Original/ArmDependencies/WeaponBase.cs:376`.
- **PR wins** `property:Owner`; PR `public CharacterBody Owner` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:374`, `source-reference/Original/ArmDependencies/WeaponBase.cs:374`; develop `public CharacterBody Owner` at `source-reference/Original/ArmDependencies/WeaponBase.cs:374`.
- **PR wins** `property:PendingPickup`; PR `public bool PendingPickup` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:496`, `source-reference/Original/ArmDependencies/WeaponBase.cs:496`; develop `public bool PendingPickup` at `source-reference/Original/ArmDependencies/WeaponBase.cs:496`.
- **PR wins** `property:Pivot`; PR `public Vector3 Pivot` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:378`, `source-reference/Original/ArmDependencies/WeaponBase.cs:378`; develop `public Vector3 Pivot` at `source-reference/Original/ArmDependencies/WeaponBase.cs:378`.
- **PR wins** `property:PivotToMuzzle`; PR `public Vector3 PivotToMuzzle` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:386`, `source-reference/Original/ArmDependencies/WeaponBase.cs:386`; develop `public Vector3 PivotToMuzzle` at `source-reference/Original/ArmDependencies/WeaponBase.cs:386`.
- **PR wins** `property:Position`; PR `public float Position` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:414`, `source-reference/Original/ArmDependencies/WeaponBase.cs:414`; develop `public float Position` at `source-reference/Original/ArmDependencies/WeaponBase.cs:414`.
- **PR wins** `property:PrecisionBegin`; PR `public float PrecisionBegin` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:560`, `source-reference/Original/ArmDependencies/WeaponBase.cs:560`; develop `public float PrecisionBegin` at `source-reference/Original/ArmDependencies/WeaponBase.cs:560`.
- **PR wins** `property:PrecisionCenterCoord`; PR `public float PrecisionCenterCoord` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:542`, `source-reference/Original/ArmDependencies/WeaponBase.cs:542`; develop `public float PrecisionCenterCoord` at `source-reference/Original/ArmDependencies/WeaponBase.cs:542`.
- **PR wins** `property:PrecisionDistance`; PR `public float PrecisionDistance` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:544`, `source-reference/Original/ArmDependencies/WeaponBase.cs:544`; develop `public float PrecisionDistance` at `source-reference/Original/ArmDependencies/WeaponBase.cs:544`.
- **PR wins** `property:PrecisionEnd`; PR `public float PrecisionEnd` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:568`, `source-reference/Original/ArmDependencies/WeaponBase.cs:568`; develop `public float PrecisionEnd` at `source-reference/Original/ArmDependencies/WeaponBase.cs:568`.
- **PR wins** `property:PrecisionHalfDistance`; PR `public float PrecisionHalfDistance` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:552`, `source-reference/Original/ArmDependencies/WeaponBase.cs:552`; develop `public float PrecisionHalfDistance` at `source-reference/Original/ArmDependencies/WeaponBase.cs:552`.
- **PR wins** `property:ReadyToFire`; PR `public bool ReadyToFire` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:514`, `source-reference/Original/ArmDependencies/WeaponBase.cs:514`; develop `public bool ReadyToFire` at `source-reference/Original/ArmDependencies/WeaponBase.cs:514`.
- **PR wins** `property:ReadynessValue`; PR `public int ReadynessValue` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:524`, `source-reference/Original/ArmDependencies/WeaponBase.cs:524`; develop `public int ReadynessValue` at `source-reference/Original/ArmDependencies/WeaponBase.cs:524`.
- **PR wins** `property:RecoilClimb`; PR `public float RecoilClimb` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:432`, `source-reference/Original/ArmDependencies/WeaponBase.cs:432`; develop `public float RecoilClimb` at `source-reference/Original/ArmDependencies/WeaponBase.cs:432`.
- **PR wins** `property:RecoilKick`; PR `public float RecoilKick` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:424`, `source-reference/Original/ArmDependencies/WeaponBase.cs:424`; develop `public float RecoilKick` at `source-reference/Original/ArmDependencies/WeaponBase.cs:424`.
- **PR wins** `property:RigidBody`; PR `public Rigidbody RigidBody` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:482`, `source-reference/Original/ArmDependencies/WeaponBase.cs:482`; develop `public Rigidbody RigidBody` at `source-reference/Original/ArmDependencies/WeaponBase.cs:482`.
- **PR wins** `property:TotalWeight`; PR `public float TotalWeight` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:346`, `source-reference/Original/ArmDependencies/WeaponBase.cs:346`; develop `public float TotalWeight` at `source-reference/Original/ArmDependencies/WeaponBase.cs:346`.
- **PR wins** `property:onNewMagazine`; PR `public UnityEvent onNewMagazine` at `source-reference/Original/CharacterDependencies/WeaponBase.cs:594`, `source-reference/Original/ArmDependencies/WeaponBase.cs:594`; develop `public UnityEvent onNewMagazine` at `source-reference/Original/ArmDependencies/WeaponBase.cs:594`.


### 194. `WeaponBase+BoltReleaseAnimation` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `BoltReleaseAnimation`; partial `false`; `source-reference/Original/ArmDependencies/WeaponBase.cs:35`
- enum; namespace `(global)`; `BoltReleaseAnimation`; partial `false`; `source-reference/Original/CharacterDependencies/WeaponBase.cs:35`

**develop declarations**:
- enum; namespace `(global)`; `BoltReleaseAnimation`; partial `false`; `source-reference/Original/ArmDependencies/WeaponBase.cs:35`

Common member signatures: **0**.


### 195. `WeaponBase+BoltState` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `BoltState`; partial `false`; `source-reference/Original/ArmDependencies/WeaponBase.cs:17`
- enum; namespace `(global)`; `BoltState`; partial `false`; `source-reference/Original/CharacterDependencies/WeaponBase.cs:17`

**develop declarations**:
- enum; namespace `(global)`; `BoltState`; partial `false`; `source-reference/Original/ArmDependencies/WeaponBase.cs:17`

Common member signatures: **0**.


### 196. `WeaponBase+ChamberState` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `ChamberState`; partial `false`; `source-reference/Original/ArmDependencies/WeaponBase.cs:10`
- enum; namespace `(global)`; `ChamberState`; partial `false`; `source-reference/Original/CharacterDependencies/WeaponBase.cs:10`

**develop declarations**:
- enum; namespace `(global)`; `ChamberState`; partial `false`; `source-reference/Original/ArmDependencies/WeaponBase.cs:10`

Common member signatures: **0**.


### 197. `WeaponBase+FireMode` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `FireMode`; partial `false`; `source-reference/Original/ArmDependencies/WeaponBase.cs:27`
- enum; namespace `(global)`; `FireMode`; partial `false`; `source-reference/Original/CharacterDependencies/WeaponBase.cs:27`

**develop declarations**:
- enum; namespace `(global)`; `FireMode`; partial `false`; `source-reference/Original/ArmDependencies/WeaponBase.cs:27`

Common member signatures: **0**.


### 198. `WeaponBrace` — PR type wins

**PR declarations**:
- enum; namespace `(global)`; `WeaponBrace`; partial `false`; `runtime/Original/WeaponBrace.cs:1`

**develop declarations**:
- enum; namespace `(global)`; `WeaponBrace`; partial `false`; `runtime/Original/WeaponBrace.cs:1`

Common member signatures: **0**.


### 199. `WeaponContracts` — PR type wins

**PR declarations**:
- class; namespace `(global)`; `WeaponContracts`; partial `false`; `runtime/Checks/WeaponContracts.cs:2`

**develop declarations**:
- class; namespace `(global)`; `WeaponContracts`; partial `false`; `runtime/Checks/WeaponContracts.cs:2`

Common member signatures: **1**.
- **PR wins** ``method:Run`0(string)``; PR `public static string Run(string)` at `runtime/Checks/WeaponContracts.cs:3`; develop `public static string Run(string)` at `runtime/Checks/WeaponContracts.cs:3`.

