using System.Runtime.CompilerServices;

namespace UnityEngine;

// Original RenderTexture.active selects the target for Unity GL skin-mask
// drawing. A native/browser graphics driver must actually bind the target;
// selecting one here never pretends that any pixels were drawn.
public interface IRenderTargetDriver {
    RenderTexture ActiveTarget { get; }
    void SetActive(RenderTexture target);
}
public sealed class RenderTexture : Texture {
    private static readonly ConditionalWeakTable<RuntimeWorld, IRenderTargetDriver> drivers = new();
    public static void Bind(RuntimeWorld world, IRenderTargetDriver driver) {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(driver);
        drivers.Remove(world);
        drivers.Add(world, driver);
    }
    private static IRenderTargetDriver Driver {
        get {
            if (drivers.TryGetValue(RuntimeWorld.Current, out var driver)) return driver;
            throw new NotSupportedException("Original limb-skin render target driver is not bound");
        }
    }
    public static RenderTexture active {
        get => Driver.ActiveTarget;
        set => Driver.SetActive(value);
    }
}
