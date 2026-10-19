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
    public sealed class Calibre { }
    public sealed class Casing { }
    public sealed class UIAmmoRenderItem { }
    public sealed class MarksmanRound { }
}
