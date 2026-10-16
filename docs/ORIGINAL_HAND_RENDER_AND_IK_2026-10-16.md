# Original hand renderers and source IK update

## Cause and implementation

The visible hand omission is independent of the ragdoll physics bridge. `export_operator_manifest.py` selects `LimbRenderer` scripts, but all four original Operator hand variants are ordinary `MeshRenderer` components using Unity's built-in Quad. They therefore had hierarchy nodes and source hand-mode references but no geometry or textures in the browser.

Recover those four actual renderers, their three PNGs, material properties and source IDs with `scripts/recover_original_hand_renderers.py`. Original Operator prefab SHA256 is `94bcda5de74f45d83f4bbf88e80874b8f77bf0339d307294187b085c03170536`. The renderer attaches only the original 1x1 Quad to existing authored transforms; no new art, offsets, or hand animation is generated. It uses the existing texture transport style and preserves material alpha cutoff, tint, UV scale/offset and depth-write state. **The exported custom shader is a dummy; native lighting/shader parity is not claimed.** Source PNGs are copied byte-for-byte, with SHA256 metadata.

The former `Crisis.Runtime.Weapons.Hand` transport only stored an enum. It now inherits the already compiled, unchanged global original `Hand` and binds its actual `fist`, `trigger` and `forwardAssist` GameObjects. The existing selected original transition methods invoke **the original Hand.SetMode**, and the browser applies the resulting distinct source object states rather than reimplementing its switch. Aliased hand modes remain aliased.

Extend the existing explicitly scoped original IK method selection with the original `ArmIKSolver.Awake` and `Update`. Use the actual source upper/lower/hand hierarchy, source interpolation and per-arm settings. The browser applies the resulting C# bone world rotations; it no longer runs a separate analytic presentation solve for the live arms. `scripts/source_arm_runtime.py` selects 12 original method bodies and verifies reproducible output against preserved full original source (SHA256 `ddc4291b1ab5f73e74ddb65b8df67232bff2afddd1a591403e8aceed13a9c5dc`). Compatibility changes are explicit: existing namespace/state views and host entry points, two unread Joint query locals omitted, debug DrawLine omitted. **This is selected source execution, not compilation of the entire original IKArmSolver/CharacterBody/IKArmController dependency graph.**

## Dependency correction

Original `ArmController.Update` returns immediately while `CharacterBody.Kinematic` is true. Normal character hand placement uses `ArmIKSolver`; dynamic physical-arm recovery is a distinct requirement. Unity 2017.2's official hinge spring contract establishes an initial rest angle of zero, degree-valued targets and independent useSpring enablement: https://docs.unity3d.com/2017.2/Documentation/ScriptReference/HingeJoint-spring.html . PhysX joint frames and radian units: https://nvidia-omniverse.github.io/PhysX/physx/5.6.1/docs/Joints.html . This explains why normal hand rendering/IK should not wait for ragdoll springs.

## Verification and remaining scope

Local native .NET 8 compilation and existing runtime, original Hand/sequence and original M4 firing/reload contracts pass. Native weapon contracts also assert real source visual activation and finite source IK bone rotations. `scripts/verify_original_hand_runtime.py` verifies exact generated original method selection, PNG SHA256, original Quad IDs, source hierarchy and aliased hand references. Full WASM/browser evidence is pending until its exact-SHA milestone run succeeds; do not call a compiled native fixture a browser result.

The browser smoke now requires four loaded source hand renderers, three original textures, two active hand visuals, actual WebGL draw calls, original C# hand state transport, and source IK telemetry; it captures full landscape screenshots with the existing original magazine HUD. Existing aim, moving/facing, firing, ammo conservation, muzzle flash, collisions and magazine drag/reload regressions remain required.

Still unfinished: dynamic source capsule/hinge spring compatibility, unchanged physical ArmController execution, whole original CharacterBody/IKArmController/IKArmSequencer callbacks, complete game/native parity and actual physical iPhone acceptance. Do not mark the seven-step program complete. Revert this feature's renderer, bindings, source selection, host/presentation edits, tests and recovered asset additions to roll back; preserve prior arm physics recovery and source archive.
