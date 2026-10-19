using System.Runtime.CompilerServices;

namespace UnityEngine.Audio;

// Original AudioMixer.SetFloat is a host-side audio routing operation.
// Unbound mixers fail explicitly instead of reporting fictitious audio changes.
public interface IAudioMixerDriver {
    bool SetFloat(string exposedParameter, float value);
    void TransitionTo(AudioMixerSnapshot snapshot,float timeToReach) => throw new NotSupportedException("Original Unity AudioMixer snapshot driver is not bound");
}
public sealed class AudioMixer : UnityEngine.Object {
    private static readonly ConditionalWeakTable<UnityEngine.RuntimeWorld, IAudioMixerDriver> drivers = new();
    public static void Bind(UnityEngine.RuntimeWorld world, IAudioMixerDriver driver) {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(driver);
        drivers.Remove(world);
        drivers.Add(world, driver);
    }
    public bool SetFloat(string name, float value) {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
        var current = world ?? UnityEngine.RuntimeWorld.Current;
        if (!drivers.TryGetValue(current, out var driver))
            throw new NotSupportedException("Original Unity AudioMixer driver is not bound");
        return driver.SetFloat(name, value);
    }
    internal void TransitionTo(AudioMixerSnapshot snapshot,float timeToReach) {
        if(snapshot==null)throw new ArgumentNullException(nameof(snapshot));
        if(!float.IsFinite(timeToReach)||timeToReach<0)throw new ArgumentOutOfRangeException(nameof(timeToReach));
        var current=world??UnityEngine.RuntimeWorld.Current;
        if(!drivers.TryGetValue(current,out var driver))throw new NotSupportedException("Original Unity AudioMixer snapshot driver is not bound");
        driver.TransitionTo(snapshot,timeToReach);
    }
}
public sealed class AudioMixerSnapshot:UnityEngine.Object {
    public AudioMixer audioMixer {get;set;}
    public void TransitionTo(float timeToReach)=> (audioMixer??throw new NotSupportedException("AudioMixerSnapshot has no bound AudioMixer")).TransitionTo(this,timeToReach);
}
public sealed class AudioMixerGroup:UnityEngine.Object {public AudioMixer audioMixer {get;set;}}
