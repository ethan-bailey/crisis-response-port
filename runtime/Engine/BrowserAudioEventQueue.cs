using UnityEngine;

namespace Crisis.Runtime;

/// <summary>Bounded bridge from recovered AudioSource callbacks to browser audio.</summary>
public sealed class BrowserAudioEventQueue : IAudioSourceEventDriver, IDeferredAudioSourceDriver
{
    const int MaximumPendingEvents = 64;
    readonly List<object> pending = [];
    readonly Dictionary<int,bool> playback = [];
    public int DroppedEvents { get; private set; }
    public bool IsPlaying => playback.Values.Any(value=>value);
    public bool IsSourcePlaying(AudioSource source)=>source!=null&&playback.GetValueOrDefault(source.GetInstanceID());
    public void ReportPlaybackState(int sourceId,bool isPlaying){if(sourceId!=0)playback[sourceId]=isPlaying;}

    public void PlayOneShot(AudioClip clip, float volume, int priority) => Enqueue("oneShot", clip, volume, 1f, false, false, priority);
    public void PlayOneShot(AudioSource source,AudioClip clip,float volumeScale,int priority) => Enqueue("oneShot",clip,source.volume*volumeScale,1f,false,source.mute,priority,source);

    public void Play(AudioClip clip, float volume, float pitch, bool loop, bool muted, int priority) =>
        Enqueue("play", clip, volume, pitch, loop, muted, priority);
    public void Play(AudioSource source,AudioClip clip,float volume,float pitch,bool loop,bool muted,int priority) =>
        Enqueue("play",clip,volume,pitch,loop,muted,priority,source);

    public void Stop()
    {
        if (pending.Count >= MaximumPendingEvents) { pending.RemoveAt(0); DroppedEvents++; }
        pending.Add(new { action = "stop", sourceId = 0, time = Time.time });
        foreach(var sourceId in playback.Keys.ToArray())playback[sourceId]=false;
    }

    public void Stop(AudioSource source)
    {
        if(pending.Count>=MaximumPendingEvents){pending.RemoveAt(0);DroppedEvents++;}
        pending.Add(new{action="stop",sourceId=source?.GetInstanceID()??0,time=Time.time});
        if(source!=null)playback[source.GetInstanceID()]=false;
    }

    void Enqueue(string action, AudioClip clip, float volume, float pitch, bool loop, bool muted, int priority,AudioSource source=null)
    {
        ArgumentNullException.ThrowIfNull(clip);
        if (pending.Count >= MaximumPendingEvents) { DroppedEvents++; return; }
        if(source!=null)playback[source.GetInstanceID()]=true;
        pending.Add(new
        {
            action,
            sourceId=source?.GetInstanceID()??0,
            clipId = clip.SourceID,
            sourcePath = clip.OriginalAssetPath,
            time = Time.time,
            volume = float.IsFinite(volume) ? Math.Clamp(volume, 0f, 1f) : 0f,
            pitch = float.IsFinite(pitch) ? Math.Clamp(pitch, -3f, 3f) : 1f,
            loop,
            muted,
            priority
        });
    }

    public object[] Drain()
    {
        var result = pending.ToArray();
        pending.Clear();
        return result;
    }

    public void Clear() => pending.Clear();
}
