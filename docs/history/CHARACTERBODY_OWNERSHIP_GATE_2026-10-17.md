SUPERSEDED NUMBERING — see docs/GATE_DEPENDENCY_INDEX.md for current gate status.

# Complete CharacterBody: type-ownership gate (2026-10-17)

Integration target: `develop/wasm`. Working lane: PR #42 `feature/full-source-characterbody-integration`. This is a **blocking dependency map**, not a native-build success claim.

## Observed source facts

- `runtime/Original/CharacterBody.Full.cs` contains the staged **2,460-line** `Crisis.Runtime.FullSource.CharacterBody`, while `runtime/Crisis.Runtime.csproj` compiles `Engine/**/*.cs` and `Original/**/*.cs`.
- It imports `Crisis.Runtime.Weapons`; `Equipment` is currently defined in `runtime/Original/Weapons/Equipment.cs` in that namespace, with `WeaponBase` fields and a mutable magazine/ammunition inventory.
- `CharacterBody.Awake()` fetches `Blackboard`, `Physiology`, `MuscleControl`, `IKArmController`, `CharacterAudio`, `Rigidbody` and `Target` from one GameObject, initializes `limbs`, and registers `OnMissionBegin` with `Global.Context.Mission.onMissionBegin`.
- `CharacterBody.InitializeEquipment()` creates a spawned equipment GameObject via `EquipmentHelper.Instance.SpawnEquipment`; it assigns `Equipment` and `WeaponBase` references from that object. Ownership must follow that original object relationship, **not** a mirrored browser inventory.
- Source-archived implementations exist in `source-reference/Original/CharacterExecution/` (Limb, LimbPhysiology, LimbMuscle) and `source-reference/Original/ArmDependencies/` (Limb, WeaponBase), but those references are **not** automatically executable in the native runtime.
- PR #56 carries projectile / tissue / physiology integration; PR #57 carries arm IK callbacks. Neither is evidence that the complete staged source class currently compiles.

## Required type decisions before adding more source classes

| Type group | Sole live owner / source of truth | Must not become a second state owner |
| --- | --- | --- |
| Character root | Full original `CharacterBody` component per actor GameObject | Existing partial browser character facade |
| Limbs | Original Limb components referenced by `CharacterBody.limbs`, `limbMap`, and named limb fields | Browser collision proxy limbs |
| Physiology / muscles | Original Physiology, LimbPhysiology, LimbMuscle and MuscleControl on the actor/limbs | Diagnostic injury and vital-state copies |
| Equipment | Original spawned Equipment component parented to actor | Browser ammo/inventory transport model |
| Weapons | Original WeaponBase per spawned weapon; magazine and trigger state owned by its original components | Browser weapon host or ammo UI model |
| Blackboard | Original per-actor Blackboard component queried in `Awake()` | Behavior Designer shared-variable serialization or JS blackboard mirror |
| Character animation / IK | Source Animator/IKArmController/ArmsControl lifecycle on actor, incorporating verified PR #57 callbacks | Browser animation-only state |
| Mission / audio / time | Original mission and audio components plus engine services reached by source call sites | Invented replacement gameplay services |

Browser adapters may **observe or issue inputs** to these components. They may not create competing authoritative state or silently convert distinct original types into one wrapper.

## Ordered acceptance gates

1. **Type ownership:** identify the exact declarations, namespaces, serialized component bindings, and all cross-type assignments for the groups above. Use a *single live component instance per actor* and ensure no source class binds a browser transport equivalent. Record any incompatible identities as explicit compiler blockers.
2. **Physiology and muscles:** integrate verified slices of PR #56 only after limb identity is stable. Assert independent actor vitals, per-limb wound propagation, update order, and death/ragdoll events.
3. **Animation / equipment:** close original animation component dependencies and PR #57's IK callbacks against the same actor/equipment/weapon objects.
4. **Mission / audio / time / interactions:** preserve original service dependencies and component lookup; do not substitute silent, inert implementations to force compilation.
5. **Proof:** native compile of the **entire** `CharacterBody.Full.cs`, then two independent actors including spawn → equip → move → wound → unconscious/death → cleanup; only after those pass, one fixed-SHA WASM/browser milestone with saved reports.

## Explicit blockers still requiring proof

- The full source's field names compile syntactically as references but currently rely on incomplete or mixed runtime type definitions; this audit is **not** a compiler error inventory.
- No passing full native build, two-actor lifecycle test, or WASM/browser milestone at the combined dependency graph has been established in this work.
- Do not merge PR #42/#56/#57 wholesale on the strength of static preservation or isolated callback tests.

Maintain the original source logic. Preserve iPhone analog movement behavior and manual-only deployment controls from `AGENTS.md`.
