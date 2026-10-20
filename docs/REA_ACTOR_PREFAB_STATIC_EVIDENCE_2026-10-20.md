# REA static evidence for original actor prefabs — 2026-10-20

## Scope and provenance

This is static inspection evidence for choosing the next source-recovery slice. REA parses PE/CLI metadata and CIL; it does not load or execute the assembly and does not establish runtime behavior.

The source was the hash-verified original recovery artifact downloaded from workflow artifact `11903811738` (ZIP SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`). The artifact contains `export/AuxiliaryFiles/GameAssemblies/Assembly-CSharp.dll`. Extracted DLL SHA-256: `f5b73879f976e082ce91c238fd3443675ecc18dda00e51e6046b75303656a0cd`; length 1,482,240 bytes; MVID `277b46fb-f585-4670-973c-6eedbc68ece9`. It is a complete Unity Mono PE/CLI assembly with IL-only CIL and 1,534 TypeDef rows. The binary is present in the recovery archive; it is not a tracked repository assembly.

Inspection used cached `rea-agents@6.7.0` CLI commands `inspect-managed-artifact` and `inspect-managed-members --type LimbAudio --type ControlRenderOrder`. Both selected types have complete metadata coverage; all inspected method bodies were present and decoded without truncated instructions.

## Prefab-to-source evidence

The generated execution package contains these preserved prefab records:

| Prefab | SHA-256 | MonoBehaviour component references |
| --- | --- | ---: |
| `Assets/Resources/characters/Operator.prefab` | `94bcda5de74f45d83f4bbf88e80874b8f77bf0339d307294187b085c03170536` | 120 |
| `Assets/Resources/characters/Occupant.prefab` | `9094b840c49d6c53ffa077c274561ab5cbeedb202f85dbaade023ad67cb8c419` | 117 |

Resolving each prefab component's script GUID against the recovered source metadata finds:

| Original script | Operator | Occupant | Current FullSource status |
| --- | ---: | ---: | --- |
| `ControlRenderOrder` | 2 | 0 | Missing |
| `LimbAudio` | 18 | 16 | Missing |

Both GUIDs resolve in the recovery package; there were no unresolved non-builtin script GUIDs in these two actor prefabs. The whole package's unrelated unresolved GUID inventory must not be attributed to these actors.

## Managed-code observations

- `ControlRenderOrder` TypeDef `0x02000319` has `.ctor` MethodDef `0x06000ef4` (18 bytes of CIL) and `Start` MethodDef `0x06000ef5` (7 bytes). The constructor stores integer `3000` to `renderOrder`; `Start` calls its target and returns. The original recovered C# source is also present in the archive and defines the target as object destruction. Preserve that source behavior when integrating.
- `LimbAudio` TypeDef `0x020002dc` has six methods: `.ctor` (116 bytes), `Awake` (149), `Start` (28), `Update` (482), `OnCollisionEnter` (121), and `.cctor` (11). Every method body was decoded. Its call graph includes `Limb.get_Body`, `CharacterBody.get_Audio`, `AudioPropagator.get_Instance`, `UnityEngine.Component.GetComponent`, and Unity time/transform APIs. This is a real runtime dependency with movement and collision audio behavior; do not replace it with a stub.
- CIL tokens are build-local. Match them only with the assembly SHA-256 and MVID above; do not treat token numbers as stable source identities.

## Actual-loader blockers identified by static comparison

The real prefabs exercise more than the synthetic local-reference fixture. The current loader's builtin field binder does not consume these serialized fields:

| Builtin class | Unsupported serialized members observed |
| --- | ---: |
| MeshRenderer (class ID 23) | 20 |
| Animator (class ID 95) | 7 |
| BoxCollider2D (class ID 61) | 1 (`m_SpriteTilingProperty`) |
| AudioSource (class ID 82) | 15 |

The current `ReadValue` path resolves local prefab fileIDs only. It does not load referenced mesh/material/texture/audio/controller/prefab assets into the generated execution package. Static counts: Operator has 309 serialized external references to 149 targets; Occupant has 278 references to 127 targets. Actor external GUID paths resolve in the recovered archive, but the runtime must still instantiate/type-bind those assets before a complete `requireComplete:true` load can pass. These observations explain why synthetic fixture success is not evidence that either real actor prefab loads.

## Next implementation gate

1. Add original `LimbAudio` and `ControlRenderOrder` from the verified recovered source to the full-source dependency closure, preserving GUID/type identity and source behavior. Refresh the source dependency inventory and review ambiguous symbols before compilation.
2. Extend actual prefab loading for serialized builtin fields and package-backed object references, failing visibly on unresolved entries. Use preserved Operator/Occupant records and dependencies rather than synthetic substitutes.
3. Load one complete actor graph, then verify original CharacterBody/Limb ownership and lifecycle after mission, mask, audio, and presentation services are bound. Only then proceed to two-actor integration and the consolidated WASM/browser gate.

No source code, runtime behavior, WASM package, preview, or deployment was changed by this static analysis. The focused `--prefab-only` fixture remains synthetic and the broader Checks project was previously blocked at `WeaponContracts` (`AmmunitionLoader.Load: Availability`).
