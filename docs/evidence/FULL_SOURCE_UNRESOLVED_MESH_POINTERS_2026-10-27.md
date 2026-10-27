# Full-source unresolved .308 mesh pointers — 2026-10-27

## Finding

The verified `crisis-response-recovery-direct-v3-1.zip` export is complete enough to index, but it does **not** contain the Unity asset metadata for Mesh GUID `b4b69dc6c74eeb54cbd22d28e37f9a19`, fileID `4300000`. `scripts/extract_runtime_scene.py` reports six unresolved pointers to this same source mesh in the full 2,060-asset export. They are not six different missing GUIDs. The seven-root `global-ui-mission-operator` profile has zero unresolved references because these ammunition assets are outside that profile closure.

The six exact `m_Mesh` edges are:

| Source asset | Source component fileID | Target |
| --- | ---: | --- |
| `Assets/Resources/weapons/ammunition/casings/Casing .308.prefab` | `33884547391421755` | missing GUID `b4b69dc6c74eeb54cbd22d28e37f9a19`, fileID `4300000` |
| `Assets/Resources/weapons/ammunition/marksman/Marksman round .308 Winchester BT.prefab` | `64587765826080250` | same missing Mesh |
| `Assets/Resources/weapons/ammunition/marksman/Marksman round .308 Winchester FMJ.prefab` | `64587765826080250` | same missing Mesh |
| `Assets/Resources/weapons/ammunition/marksman/Marksman round .308 Winchester HEI.prefab` | `64587765826080250` | same missing Mesh |
| `Assets/Resources/weapons/ammunition/marksman/Marksman round .308 Winchester JHP.prefab` | `64587765826080250` | same missing Mesh |
| `Assets/Resources/weapons/ammunition/marksman/Marksman round .308 Winchester SP.prefab` | `64587765826080250` | same missing Mesh |

## Known disposition and remaining action

The existing owner-approved approximation applies **only** to the root MeshFilter in `Casing .308.prefab`: `Assets/Mesh/308 Winchester casing hi.asset`, candidate GUID `cd74722033652fb409431b2947740490`, source SHA-256 `a31736991aa6345f4d9dffa4a5532681a184ac88520867a79fa7cbef859392b6`. The candidate is not an identity recovery. `scripts/replace_missing_308_casing_mesh.py` scopes the change to that root and explicitly leaves the child Projectile and all projectile-round prefabs untouched. The packaged ammunition bundle's approved casing mapping does not imply that the other five references are fixed.

The current SceneLoader/application Mission profile does not depend on these six edges, so they do not block its 1,920-asset package. The ammunition/projectile asset gate remains open: recover the exact mesh+`.meta` identity or obtain explicit owner approval for a separately evidenced candidate mapping for the five marksman-round MeshFilters. Do not broaden the existing casing substitution, fabricate the missing GUID, or claim the full-source index has zero unresolved edges. Reproduce the inventory from the verified full source index at `/tmp/cpr-scenes-full-source-current.json` (SHA-256 `1b196dafa059a3aadb739b294134d7fe8d9d28510f119214b35c7415059722f7`).

## Reproduction of the approved casing-only disposition — 2026-10-27

To preserve the canonical recovered export, a separate copy was created at `/tmp/cpr-profile-recovery-v3-1-308-working-copy-1955`; `python3 scripts/replace_missing_308_casing_mesh.py <copy> --write` succeeded and recorded `identity_verified=false`. Re-indexing that copy produced SHA-256 `9e1787d3ae31fb41a55a6ad99a415a7d38e551c582dfbe10bf9157f8afccdb31`, 2,060 assets / 40,139 records / 24,856 PPtr edges / 776 binary assets, and **five** unresolved references to the original GUID. They are exactly the five marksman-round MeshFilters above. The casing edge alone changed to the separately GUID-verified candidate. The unmodified canonical index remains SHA `1b196d…` with six unresolved references; its archive and extracted original working tree were not modified.
