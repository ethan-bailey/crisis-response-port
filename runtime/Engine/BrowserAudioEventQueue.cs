using UnityEngine;

namespace Crisis.Runtime;

/// <summary>Bounded bridge from recovered AudioSource callbacks to browser audio.</summary>
public sealed class BrowserAudioEventQueue : IAudioSourceEventDriver, IDeferredAudioSourceDriver
{
    const int MaximumPendingEvents = 64;
    readonly List<object> pending = [];
    readonly Dictionary<int,bool> playback = [];
    readonly Dictionary<int,AudioSource> sources = [];
    public int DroppedEvents { get; private set; }
    public bool IsPlaying => playback.Values.Any(value=>value);
    public bool IsSourcePlaying(AudioSource source)=>source!=null&&playback.GetValueOrDefault(source.GetInstanceID());
    public void ReportPlaybackState(int sourceId,bool isPlaying){if(sourceId==0)return;playback[sourceId]=isPlaying;if(!isPlaying)sources.Remove(sourceId);}

    /// <summary>Current source-owned input for browser-side spatial/mixer/voice evaluation.</summary>
    public object[] ActiveSourceStates()=>playback.Where(item=>item.Value&&sources.TryGetValue(item.Key,out var source)&&source&&source.isActiveAndEnabled)
        .Select(item=>DescribeSource(sources[item.Key])).ToArray();

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
        sources.Clear();
    }

    public void Stop(AudioSource source)
    {
        if(pending.Count>=MaximumPendingEvents){pending.RemoveAt(0);DroppedEvents++;}
        pending.Add(new{action="stop",sourceId=source?.GetInstanceID()??0,time=Time.time});
        if(source!=null){playback[source.GetInstanceID()]=false;sources.Remove(source.GetInstanceID());}
    }

    void Enqueue(string action, AudioClip clip, float volume, float pitch, bool loop, bool muted, int priority,AudioSource source=null)
    {
        ArgumentNullException.ThrowIfNull(clip);
        if (pending.Count >= MaximumPendingEvents) { DroppedEvents++; return; }
        if(source!=null){playback[source.GetInstanceID()]=true;sources[source.GetInstanceID()]=source;}
        var activeListeners=source?.world?.Objects.OfType<AudioListener>()
            .Where(listener=>listener.enabled&&listener.gameObject.activeInHierarchy).ToArray()??Array.Empty<AudioListener>();
        var selectedListener=activeListeners.Length==1?activeListeners[0]:null;
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
            priority,
            sourceState=source==null?null:DescribeSource(source,activeListeners,selectedListener)
        });
    }

    object DescribeSource(AudioSource source)
    {
        var activeListeners=source.world?.Objects.OfType<AudioListener>()
            .Where(listener=>listener.enabled&&listener.gameObject.activeInHierarchy).ToArray()??Array.Empty<AudioListener>();
        return DescribeSource(source,activeListeners,activeListeners.Length==1?activeListeners[0]:null);
    }

    object DescribeSource(AudioSource source,AudioListener[] activeListeners,AudioListener selectedListener)=>new
    {
        sourceId=source.GetInstanceID(),clipId=source.clip?.SourceID,sourcePath=source.clip?.OriginalAssetPath,
        ownerName=source.name,ownerAssetPath=source.OriginalAssetPath,ownerAssetGuid=source.OriginalAssetGuid,ownerAssetFileID=source.OriginalAssetFileID,
        enabled=source.enabled,activeSelf=source.gameObject.activeSelf,activeInHierarchy=source.gameObject.activeInHierarchy,
        priority=source.priority,volume=source.volume,pitch=source.pitch,muted=source.mute,loop=source.loop,
        spatialize=source.spatialize,spatializePostEffects=source.spatializePostEffects,dopplerLevel=source.dopplerLevel,
        minDistance=source.minDistance,maxDistance=source.maxDistance,pan2D=source.pan2D,rolloffMode=source.rolloffMode,
        rolloffCustomCurve=DescribeCurve(source.rolloffCustomCurve),panLevelCustomCurve=DescribeCurve(source.panLevelCustomCurve),
        spread=source.spread,spreadCustomCurve=DescribeCurve(source.spreadCustomCurve),reverbZoneMix=source.reverbZoneMix,
        reverbZoneMixCustomCurve=DescribeCurve(source.reverbZoneMixCustomCurve),bypassEffects=source.bypassEffects,
        bypassListenerEffects=source.bypassListenerEffects,bypassReverbZones=source.bypassReverbZones,
        outputAudioMixerGroupGuid=source.outputAudioMixerGroup?.OriginalAssetGuid,
        sourceTransform=DescribeTransform(source.transform,source),activeListenerCount=activeListeners.Length,
        listenerTransform=selectedListener==null?null:DescribeTransform(selectedListener.transform,selectedListener),
        time=Time.time
    };

    static object DescribeCurve(AnimationCurve curve)=>curve==null?null:new
    {
        keys=curve.keys.Select(key=>new{key.time,key.value,inTangent=float.IsFinite(key.inTangent)?key.inTangent:(float?)null,
            outTangent=float.IsFinite(key.outTangent)?key.outTangent:(float?)null,key.tangentMode,key.serializedVersion}).ToArray(),
        preWrapMode=curve.preWrapMode.ToString(),postWrapMode=curve.postWrapMode.ToString(),
        clampOutside=curve.clampOutside,serializedPreInfinity=curve.serializedPreInfinity,
        serializedPostInfinity=curve.serializedPostInfinity,serializedRotationOrder=curve.serializedRotationOrder
    };

    static object DescribeTransform(Transform transform,UnityEngine.Object owner)
    {
        var position=transform.position;var forward=transform.forward;var rotation=transform.rotation;
        return new
        {
            ownerName=owner.name,ownerAssetPath=owner.OriginalAssetPath,ownerAssetGuid=owner.OriginalAssetGuid,ownerAssetFileID=owner.OriginalAssetFileID,
            position=new[]{position.x,position.y,position.z},forward=new[]{forward.x,forward.y,forward.z},rotation=new[]{rotation.x,rotation.y,rotation.z,rotation.w}
        };
    }

    public object[] Drain()
    {
        var result = pending.ToArray();
        pending.Clear();
        return result;
    }

    public void Clear() => pending.Clear();
}
