# Original movement port: source audit

Date: 2026-10-14

## Source provenance

The implementation was traced to the recovery artifact documented in [RECOVERY_RUN_2026-10-14.md](RECOVERY_RUN_2026-10-14.md). The decompiled gameplay assembly's SHA-256 matched the assembly extracted from the user's original archive.

Files inspected in that artifact:

- `decompiled/PlayerController.cs`
- `decompiled/CharacterBody.cs`
- `export/ExportedProject/Assets/Resources/characters/Operator.prefab`

The source is decompiled/reconstructed C#, not the developer's original source tree.

## Behavior ported into the browser runtime

- `PlayerController.Update()` chooses left first, otherwise right, and calls `Body.Move(direction)`.
- `CharacterBody.Move(float direction, float magnitude = 1f)` only stores acceleration when direction is nonzero.
- `CharacterBody.FixedUpdate()` compares the absolute Rigidbody X velocity against the configured target speed. It computes the acceleration force as `(targetSpeed - abs(velocity)) * sign(direction) * fixedDeltaTime * 750 * accelerationMagnitude`, applies it as `ForceMode.Acceleration`, then resets acceleration when the speed is not above the target.
- The recovered `Operator.prefab` configures `walkSpeed: 1.5` and `sprintSpeed: 4.2`; those values are used instead of the class defaults.
- The Operator root Rigidbody is configured with `m_Drag: 0`. The browser runtime therefore does not add a release-drag/deceleration mechanic.
- Sprint speed is selected only when sprint is held, the movement direction matches the current facing direction, and the character is not crippled. The browser port currently has no ported physiology/crippling system, so that condition is explicitly treated as false.

## Known gaps — not approximated

This is a partial port of the source movement logic, not full Unity physics parity. The browser build does not yet reproduce the original Rigidbody/PhysX simulation, colliders, body-limb ragdoll, aiming-driven turning, or the complete animation state machine. Facing is deliberately not changed by directional movement because the original `Body.Move()` method does not turn the character; turning is a separate `CharacterBody.Turn()` system. Until that system is ported, movement and turning behavior cannot be claimed to match the original fully.

No weapon, projectile, damage, enemy-AI, or mission behavior was added in this change.

## Next source-traced dependency: facing and aim

Inspection of `PlayerController.cs` shows facing is updated by `HandleChangeDirection()` using `WeaponPointer.WorldPoint.x` compared with the operator's right-shoulder X coordinate. `WeaponPointer.cs` derives that point from the camera ray and a physics raycast against the original Aim layer mask, storing the hit in body-local coordinates. `CharacterBody.Turn()` then updates direction, rotation, and limb flips.

That chain is intentionally not replaced with a screen-side guess: the browser runtime still lacks the original Aim-layer colliders/raycast semantics and weapon muzzle/aim transforms. A faithful next step must port or reconstruct those source-backed dependencies before connecting pointer movement to facing or weapon aim.
