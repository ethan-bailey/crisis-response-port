// Compile-only names for migrated FullSource dependencies. These declarations
// exist only in the FoundationChecks assembly; they supply no gameplay state
// or behavior and must not be used by Crisis.Runtime.
namespace Utilities
{
    public enum Availability { }
}

namespace Crisis.Runtime.FullSource
{
    public sealed class CharacterBody { }
    public sealed class WeaponBase { }
    // Browser audio snapshots compile against the boundary type here. Keep
    // these members as compile-only signatures; production behavior lives in
    // the recovered FullSource Projectile implementation.
    public sealed class Projectile
    {
        public string name;
        public UnityEngine.Transform transform;
        public float Velocity { get; }
        public UnityEngine.Vector3 GravityVelocityAtTime(float time) => default;
    }
    public sealed class Calibre { public string name; }
    // The FoundationChecks build excludes the recovered Casing body but still
    // compiles the browser adapter. Keep only the private target signature so
    // that compile-boundary check can validate the adapter without simulating
    // Casing behavior.
    public sealed partial class Casing { private void Disable() { } }
    public sealed class UIAmmoRenderItem { }
    public sealed class MarksmanRound { }
    public sealed class AmmunitionBase : UnityEngine.MonoBehaviour
    {
        public float mass;
        public UnityEngine.AnimationCurve expansionFunction;
        public Calibre calibre;
        public Projectile projectile;
        public static AmmunitionBase GetAmmunition(string name, AmmunitionBase fallback) => fallback;
    }
}
