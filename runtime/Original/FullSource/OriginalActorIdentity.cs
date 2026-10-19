using System;
using System.Collections.Generic;

namespace Crisis.Runtime.FullSource
{
    // A non-owning validation boundary for migration of the original projectile
    // and arm-IK callers. This stores no character, limb, equipment or vital state.
    // It does not implement any replacement combat or IK behavior.
    public static class OriginalActorIdentity
    {
        public static CharacterBody RequireActor(CharacterBody actor)
        {
            if (actor == null)
                throw new ArgumentNullException(nameof(actor));
            return actor;
        }

        public static Limb RequireLimb(CharacterBody actor, Limb limb)
        {
            RequireActor(actor);
            if (limb == null)
                throw new ArgumentNullException(nameof(limb));
            if (!object.ReferenceEquals(limb.Body, actor))
                throw new InvalidOperationException("Projectile limb belongs to a different original CharacterBody");
            if (actor.limbs == null || !actor.limbs.Contains(limb))
                throw new InvalidOperationException("Projectile limb is absent from the original actor limb registry");
            if (actor.limbMap == null || !actor.limbMap.TryGetValue(limb.id, out Limb registered)
                || !object.ReferenceEquals(registered, limb))
                throw new InvalidOperationException("Projectile limb identity differs from the original actor limb map");
            return limb;
        }

        public static Physiology RequirePhysiology(CharacterBody actor, Physiology physiology)
        {
            RequireActor(actor);
            if (physiology == null || !object.ReferenceEquals(actor.Physiology, physiology))
                throw new InvalidOperationException("Physiology is not the original actor-owned instance");
            return physiology;
        }

        public static IKArmController RequireController(CharacterBody actor, IKArmController controller)
        {
            RequireActor(actor);
            if (controller == null || !object.ReferenceEquals(actor.armController, controller))
                throw new InvalidOperationException("IK controller is not the original actor-owned instance");
            return controller;
        }

        public static Crisis.Runtime.Weapons.Equipment RequireEquipment(
            CharacterBody actor, Crisis.Runtime.Weapons.Equipment equipment)
        {
            RequireActor(actor);
            if (equipment == null || !object.ReferenceEquals(actor.Equipment, equipment))
                throw new InvalidOperationException("Equipment is not the original actor-owned instance");
            return equipment;
        }

        // Validate before entering the original source callback sequence.
        // The caller must still execute RecordAttackInfo / OnWasAttacked /
        // OnWasHit in source order; this method intentionally dispatches none.
        public static CharacterBody RequireProjectileHitContext(CharacterBody victim,
            Limb hitLimb, Projectile projectile)
        {
            RequireLimb(victim, hitLimb);
            RequirePhysiology(victim, victim.Physiology);
            if (projectile == null)
                throw new ArgumentNullException(nameof(projectile));
            return victim;
        }
    }
}

