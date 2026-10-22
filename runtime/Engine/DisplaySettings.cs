using System.Runtime.CompilerServices;

namespace UnityEngine;

public struct Resolution { public int width, height, refreshRate; }
public enum AnisotropicFiltering { Disable, Enable, ForceEnable }
public enum ShadowQuality { Disable, HardOnly, All }
public enum ShadowResolution { Low, Medium, High, VeryHigh }
public enum ShadowProjection { CloseFit, StableFit }
public enum ShadowmaskMode { Shadowmask, DistanceShadowmask }

public interface IDisplaySettingsDriver {
    Resolution CurrentResolution { get; }
    IReadOnlyList<Resolution> Resolutions=>new[]{CurrentResolution};
    bool FullScreen { get; }
    void SetResolution(int width,int height,bool fullScreen,int refreshRate);
}
public static class Screen {
    static readonly ConditionalWeakTable<RuntimeWorld,IDisplaySettingsDriver> drivers=new();
    public static void Bind(RuntimeWorld world,IDisplaySettingsDriver driver) { ArgumentNullException.ThrowIfNull(world);ArgumentNullException.ThrowIfNull(driver);drivers.Remove(world);drivers.Add(world,driver); }
    static IDisplaySettingsDriver Driver=>drivers.TryGetValue(RuntimeWorld.Current,out var value)?value:throw new NotSupportedException("Display settings driver is not bound");
    public static Resolution currentResolution=>Driver.CurrentResolution;
    public static Resolution[] resolutions=>Driver.Resolutions.ToArray();
    public static bool fullScreen=>Driver.FullScreen;
    public static void SetResolution(int width,int height,bool fullscreen,int preferredRefreshRate=0) {
        if(width<=0||height<=0||preferredRefreshRate<0)throw new ArgumentOutOfRangeException(nameof(width));
        Driver.SetResolution(width,height,fullscreen,preferredRefreshRate);
    }
}

// The original QualityLevels method supplies the decisions and call order.
// Every property is forwarded to the renderer that owns the current world.
public interface IQualitySettingsDriver {
    T Get<T>(string property);
    void Set<T>(string property,T value);
}
public static class QualitySettings {
    static readonly ConditionalWeakTable<RuntimeWorld,IQualitySettingsDriver> drivers=new();
    public static void Bind(RuntimeWorld world,IQualitySettingsDriver driver) { ArgumentNullException.ThrowIfNull(world);ArgumentNullException.ThrowIfNull(driver);drivers.Remove(world);drivers.Add(world,driver); }
    static IQualitySettingsDriver Driver=>drivers.TryGetValue(RuntimeWorld.Current,out var value)?value:throw new NotSupportedException("Quality settings renderer is not bound");
    static T Get<T>(string property)=>Driver.Get<T>(property);
    static void Set<T>(string property,T value)=>Driver.Set(property,value);
    public static int vSyncCount {get=>Get<int>(nameof(vSyncCount));set=>Set(nameof(vSyncCount),value);}
    public static int pixelLightCount {get=>Get<int>(nameof(pixelLightCount));set=>Set(nameof(pixelLightCount),value);}
    public static int masterTextureLimit {get=>Get<int>(nameof(masterTextureLimit));set=>Set(nameof(masterTextureLimit),value);}
    public static AnisotropicFiltering anisotropicFiltering {get=>Get<AnisotropicFiltering>(nameof(anisotropicFiltering));set=>Set(nameof(anisotropicFiltering),value);}
    public static int antiAliasing {get=>Get<int>(nameof(antiAliasing));set=>Set(nameof(antiAliasing),value);}
    public static bool softParticles {get=>Get<bool>(nameof(softParticles));set=>Set(nameof(softParticles),value);}
    public static bool realtimeReflectionProbes {get=>Get<bool>(nameof(realtimeReflectionProbes));set=>Set(nameof(realtimeReflectionProbes),value);}
    public static bool billboardsFaceCameraPosition {get=>Get<bool>(nameof(billboardsFaceCameraPosition));set=>Set(nameof(billboardsFaceCameraPosition),value);}
    public static float resolutionScalingFixedDPIFactor {get=>Get<float>(nameof(resolutionScalingFixedDPIFactor));set=>Set(nameof(resolutionScalingFixedDPIFactor),value);}
    public static ShadowQuality shadows {get=>Get<ShadowQuality>(nameof(shadows));set=>Set(nameof(shadows),value);}
    public static ShadowResolution shadowResolution {get=>Get<ShadowResolution>(nameof(shadowResolution));set=>Set(nameof(shadowResolution),value);}
    public static ShadowProjection shadowProjection {get=>Get<ShadowProjection>(nameof(shadowProjection));set=>Set(nameof(shadowProjection),value);}
    public static float shadowDistance {get=>Get<float>(nameof(shadowDistance));set=>Set(nameof(shadowDistance),value);}
    public static ShadowmaskMode shadowmaskMode {get=>Get<ShadowmaskMode>(nameof(shadowmaskMode));set=>Set(nameof(shadowmaskMode),value);}
    public static float shadowNearPlaneOffset {get=>Get<float>(nameof(shadowNearPlaneOffset));set=>Set(nameof(shadowNearPlaneOffset),value);}
    public static int shadowCascades {get=>Get<int>(nameof(shadowCascades));set=>Set(nameof(shadowCascades),value);}
    public static float lodBias {get=>Get<float>(nameof(lodBias));set=>Set(nameof(lodBias),value);}
    public static int maximumLODLevel {get=>Get<int>(nameof(maximumLODLevel));set=>Set(nameof(maximumLODLevel),value);}
    public static int particleRaycastBudget {get=>Get<int>(nameof(particleRaycastBudget));set=>Set(nameof(particleRaycastBudget),value);}
    public static int asyncUploadTimeSlice {get=>Get<int>(nameof(asyncUploadTimeSlice));set=>Set(nameof(asyncUploadTimeSlice),value);}
    public static int asyncUploadBufferSize {get=>Get<int>(nameof(asyncUploadBufferSize));set=>Set(nameof(asyncUploadBufferSize),value);}
}
