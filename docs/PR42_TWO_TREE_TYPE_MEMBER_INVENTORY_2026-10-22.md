# PR #42 two-tree type and member inventory — 2026-10-22

This is the attempt 6 collision checklist. No merged tree was created or built. The PR side wins every cross-tree type and member collision. Develop-only members on shared types are deferred behavior and are not silently retained.

## Pinned inputs

| Tree | Commit | Git tree | Tracked C# files |
|---|---|---|---:|
| PR #42 (`feature/full-source-characterbody-integration`) | `a653609b62f3e3c79f73d0a17f0bf3dc89eae7b8` | `a1fd6dbb15a230c91d08fd7bcdded77192001726` | 675 |
| `develop/wasm` | `2fc0a8e93be97c4d99123910fad4bd25798e71a8` | `ed14d3af2e9cf161ac256f36ee9d6183c7e86864` | 130 |

The prompt’s develop SHA had one transposed character. GitHub’s live `develop/wasm` ref resolved to `2fc0a8e93be97c4d99123910fad4bd25798e71a8`; the inventory uses that exact commit. Every local C# file was verified against its Git blob SHA from the pinned recursive tree: 675/675 PR files and 130/130 develop files matched. The snapshots are in separate roots; no combined tree exists.

## Counts and method

| Measure | PR tree | develop tree |
|---|---:|---:|
| Tracked `.cs` files | 675 | 130 |
| Type declarations (including partial/repeated declarations) | 1129 | 283 |
| Unique fully qualified type identities after union | 1071 | 235 |
| Explicit member declarations across type declarations | 13469 | 4361 |
| Unique member signatures after per-type union | 12478 | 3214 |

| Cross-tree type collisions (same namespace/nesting/name/arity) | **199** | |
| Cross-tree member signature collisions within shared types | **2599** | |
| Develop-only member signatures in shared types (deferred) | **106** | |
| PR-only member signatures in shared types | **321** | |
| Develop-only type identities | **36** | |
| Total type plus member collision records | **2798** | |

The parser used a C# tree-sitter grammar over every tracked `.cs` blob in both pinned trees. It grouped declarations by namespace, containing type, name, and generic arity, then unioned members across partial declarations before comparison. Member collision identity uses kind, name, generic arity, and parameter types/ref modifiers for callables; field/event/property identities use their declared name (indexer parameters included). The report preserves full source signatures and every definition location. Explicit constructors, operators, conversions, indexers, and enum values are included alongside methods, properties, fields, and events. No syntax-error files remained after an equal-length parser accommodation for legacy `required` field names; offsets and signatures came from original bytes.

## Self-test — passed

All required known collisions were found before the full list was accepted:
- `UnityEngine.Animator`: all eight expected `GetBool`/`SetBool`/`GetFloat`/`SetFloat` accessors matched; 10 shared member signatures were extracted for the type.
- `UnityEngine.CapsuleCollider.Raycast(Ray, out RaycastHit, float)`: PR `runtime/Engine/CharacterPhysics.cs`; develop `runtime/Engine/OriginalCapsuleCollider.cs`.
- `ArmorClass.PenetrationTest(Type, float, Projectile)`: both PR declarations and the develop declaration were included, with each source location in the collision list.
- `Crisis.Runtime.Weapons.WeaponHost`: shared type and 29 shared member signatures.
- `Crisis.Runtime.Weapons.OriginalArmPhysicsHost`: shared type and 12 shared member signatures.

**Self-test passed.** The collision extraction contains every named case, including the same-type/different-file CapsuleCollider case and the ArmorClass member collision across its source locations.

## Disposition rules

- Every shared type identity below is a type collision. Keep the PR-side type definition(s); the declaration paths on both sides are recorded.
- Every shared member signature below is a member collision. Keep the PR-side signature/definition. When multiple source-reference copies exist on a side, all locations are listed.
- Every develop-only member under a shared type is marked **DEVELOP-ONLY — DEFERRED** and belongs on the exception list for attempt 6.
- Develop-only type identities are listed separately after the shared-type collision list. They are not silently carried into the combined tree.
- The machine-readable compressed inventory contains every type declaration, member signature, location, and source blob manifest.

## Full collision list

The full list is split into four files by type identity:

- [Collision list part 1](PR42_TWO_TREE_COLLISIONS_01_2026-10-22.md)
- [Collision list part 2](PR42_TWO_TREE_COLLISIONS_02_2026-10-22.md)
- [Collision list part 3](PR42_TWO_TREE_COLLISIONS_03_2026-10-22.md)
- [Collision list part 4](PR42_TWO_TREE_COLLISIONS_04_2026-10-22.md)

[Develop-only types, deferred behavior, consumers, and completeness](PR42_TWO_TREE_DEFERRED_2026-10-22.md).

