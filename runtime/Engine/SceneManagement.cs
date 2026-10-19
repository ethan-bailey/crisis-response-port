using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEngine.SceneManagement;

// Original Unity scene API boundary. The host must bind real scene-loading
// behavior; no invented or silently ignored scene transitions are allowed.
public readonly struct Scene {
    public int buildIndex { get; }
    public string name { get; }
    public Scene(int buildIndex, string name) {
        if (buildIndex < 0) throw new ArgumentOutOfRangeException(nameof(buildIndex));
        this.buildIndex = buildIndex;
        this.name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
public interface ISceneManagerDriver {
    Scene GetActiveScene();
    void LoadScene(int buildIndex);
    void LoadScene(string sceneName)=>throw new NotSupportedException("Name-based scene loading is not implemented by this scene driver");
}
public static class SceneManager {
    private static readonly ConditionalWeakTable<RuntimeWorld, ISceneManagerDriver> drivers = new();
    public static void Bind(RuntimeWorld world, ISceneManagerDriver driver) {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(driver);
        drivers.Remove(world);
        drivers.Add(world, driver);
    }
    private static ISceneManagerDriver Driver {
        get {
            if (drivers.TryGetValue(RuntimeWorld.Current, out var driver)) return driver;
            throw new NotSupportedException("Original Unity scene transition driver is not bound to this runtime world");
        }
    }
    public static Scene GetActiveScene() => Driver.GetActiveScene();
    public static void LoadScene(int buildIndex) {
        if (buildIndex < 0) throw new ArgumentOutOfRangeException(nameof(buildIndex));
        Driver.LoadScene(buildIndex);
    }
    public static void LoadScene(string sceneName){ArgumentException.ThrowIfNullOrWhiteSpace(sceneName);Driver.LoadScene(sceneName);}
}
