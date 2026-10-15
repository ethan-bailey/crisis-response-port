# Original material interactions checkpoint

Browser work is isolated from the concurrent Unity feasibility branch. Source baseline: `8790c88fd86755d532bdeb526cc4b2ffed01f4d0` (`codex/weapon-actions-checkpoint`).

This milestone imports the original Mission 01 BallisticMaterial/BallisticWindow properties and source interaction/fragment algorithms. It connects source penetration, ricochet, delayed exits, texture-alpha glass holes, fragmentation and sampled impact audio to the live weapon path. Windows now use the original full projectile layer mask. The weapon and queries share one world; delayed bullets do not receive flight updates, and newly spawned fragments join the next flight iteration.

Extraction: `python scripts/extract_ballistic_runtime.py RECOVERED_PROJECT game/assets/ballistics.json.gz runtime/Original/Ballistics`. Requires pinned dependencies in runtime-foundation.yml. Packed vertex/index decoding recovers 61 of 63 mesh colliders from original data. The two unresolved built-in mesh references are reported, not replaced. Convex hull queries use source vertices; exact Unity/PhysX cooking parity has not been verified.

Local native checks cover original thin/thick/impenetrable behavior, delayed suspension/resume, glass alpha hole repeat passage, source randomized fragment mass conservation and registration. Existing input, presentation, weapon/reload contracts and 23 Node regressions pass. Browser CI evidence must be added before publishing this milestone.

## Remaining boundaries

- Native particles, source exit-hole meshes and fragment meshes are effect requests only; browser rendering is not implemented. Do not claim visual impact parity. Glass alpha changes affect collisions but are not yet bound to the combined gray environment mesh.
- Source ammo curves use the same explicit endpoint clamp adapter as weapon curves when both serialized infinity fields are 2. Native Unity mapping still needs comparison; this is not certified 1:1 behavior. Do not infer WrapMode enums directly from serialized infinity numbers.
- Dynamic rigid-body impulses and limb physiology are unsupported. No invented damage, targets or particles.
- Exact Unity random sequence, native particle random consumption, texture quantization and convex cooking still need comparison.
- Original source self-assignment `Weapon = Weapon` is not corrected into a new gameplay rule in InitializeXY. Owner is set from the source weapon; fragments retain original initialization methods.

Next: finish exact source effect geometry/texture bindings, native particles and source scene visual bindings, using actual assets. Preserve owner-private hosting and do not touch the other agent's Unity branch/workflows. Update NEXT_AGENT.md after CI/deployment evidence is available.
