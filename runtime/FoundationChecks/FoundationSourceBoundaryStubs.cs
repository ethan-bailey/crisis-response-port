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
    public sealed class Projectile { public string name; }
    public sealed class Calibre { public string name; }
    public sealed partial class Casing { }
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
