using UnityEngine;

namespace Crisis.Runtime;

/// <summary>Bounded bridge from recovered AudioSource callbacks to browser audio.</summary>
public sealed class BrowserAudioEventQueue : IAudioClipPlaybackDriver
{
    const int MaximumPendingEvents = 64;
    readonly List<object> pending = [];
    public int DroppedEvents { get; private set; }
    public bool IsPlaying => false;

    public void PlayOneShot(AudioClip clip, float volume, int priority) => Enqueue("oneShot", clip, volume, 1f, false, false, priority);

    public void Play(AudioClip clip, float volume, float pitch, bool loop, bool muted, int priority) =>
        Enqueue("play", clip, volume, pitch, loop, muted, priority);

    public void Stop()
    {
        if (pending.Count >= MaximumPendingEvents) { DroppedEvents++; return; }
        pending.Add(new { action = "stop", time = Time.time });
    }

    void Enqueue(string action, AudioClip clip, float volume, float pitch, bool loop, bool muted, int priority)
    {
        ArgumentNullException.ThrowIfNull(clip);
        if (pending.Count >= MaximumPendingEvents) { DroppedEvents++; return; }
        pending.Add(new
        {
            action,
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
