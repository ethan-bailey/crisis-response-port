using UnityEngine;

namespace Crisis.Runtime.FullSource;

public partial class CharacterBody
{
    // The browser contact step runs after RuntimeWorld's source LateUpdate.
    // Reuse the original movement bookkeeping once the solved pose is synced.
    internal void RefreshMovementAfterBrowserContact() => UpdateMovement();
}
