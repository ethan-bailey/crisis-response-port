namespace UnityEngine;

public enum AudioDataLoadState {Unloaded=0,Loading=1,Loaded=2,Failed=3}
public sealed class AudioClip:Object {
    public string SourceID {get;}
    public byte[] SourceData {get;internal set;}=Array.Empty<byte>();
    public float length {get;}
    public AudioDataLoadState loadState {get;private set;}=AudioDataLoadState.Unloaded;
    public AudioClip(string sourceID,float length){
        ArgumentException.ThrowIfNullOrEmpty(sourceID);if(!float.IsFinite(length)||length<0)throw new ArgumentOutOfRangeException(nameof(length));
        SourceID=sourceID;this.length=length;world=RuntimeWorld.Current;world.Register(this);
    }
    // The browser/native decoder reports this; a source declaration alone is
    // not evidence that a sample was decoded or played on the audio device.
    public void ReportLoadState(AudioDataLoadState state)=>loadState=state;
    // Once decoded, the browser package can re-read the verified source bytes
    // if the bounded WebAudio cache later evicts this clip.
    public void ReleaseSourceData()=>SourceData=Array.Empty<byte>();
}
/// <summary>Recovered Unity listener marker. Host spatialization must select an active source listener explicitly.</summary>
public sealed class AudioListener:Behaviour { }
/// <summary>Serialized Unity reverb-zone definition. Browser output remains a separate host responsibility.</summary>
public sealed class AudioReverbZone:Behaviour {
    public float minDistance,maxDistance,roomHF,decayTime,decayHFRatio,reflections,reflectionsDelay;
    public float reverb,reverbDelay,hfReference,diffusion,density,lfReference,roomLF;
    public int reverbPreset,room;
}
public interface IAudioSourceDriver {
    bool IsPlaying {get;}
    void PlayOneShot(AudioClip clip,float volume,int priority);
    void Stop();
}
// Browser and network-backed drivers can queue source requests while their
// own decoder loads the referenced clip. Local playback drivers stay strict.
public interface IDeferredAudioSourceDriver : IAudioSourceDriver { }
public interface IAudioClipPlaybackDriver:IAudioSourceDriver {
    void Play(AudioClip clip,float volume,float pitch,bool loop,bool muted,int priority);
}
// Drivers that transport the originating AudioSource can preserve per-source
// stop/loop state across an asynchronous browser decoder.
public interface IAudioSourceEventDriver:IAudioClipPlaybackDriver {
    bool IsSourcePlaying(AudioSource source);
    void ReportPlaybackState(int sourceId,bool isPlaying);
    void PlayOneShot(AudioSource source,AudioClip clip,float volumeScale,int priority);
    void Play(AudioSource source,AudioClip clip,float volume,float pitch,bool loop,bool muted,int priority);
    void Stop(AudioSource source);
}
public sealed class AudioSource:Behaviour,IAudioSourceLifecycle {
    public UnityEngine.Audio.AudioMixerGroup outputAudioMixerGroup {get;set;}
    public bool playOnAwake=true,spatialize,spatializePostEffects,bypassEffects,bypassListenerEffects,bypassReverbZones;
    // The recovered Unity YAML omits this field. Null means the source default
    // has not been proven; an explicitly serialized value remains transportable.
    public float? serializedSpatialBlend;
    public float dopplerLevel=1,minDistance=1,maxDistance=500,pan2D,reverbZoneMix=1;
    public int rolloffMode,spread;
    public AnimationCurve rolloffCustomCurve,panLevelCustomCurve,spreadCustomCurve,reverbZoneMixCustomCurve;
    public int priority {get;set;}=128;
    public AudioClip clip;
    public float volume=1f,pitch=1f;
    public bool loop,mute;
    public IAudioSourceDriver Driver {get;set;}
    IAudioSourceDriver RequiredDriver=>Driver??throw new NotSupportedException("Decoded original audio playback driver is not bound");
    public bool isPlaying { get { var driver=RequiredDriver;return driver is IAudioSourceEventDriver sourceEvents?sourceEvents.IsSourcePlaying(this):driver.IsPlaying; } }
    bool IAudioSourceLifecycle.IsLifecyclePlaying=>Driver is IAudioSourceEventDriver sourceEvents&&sourceEvents.IsSourcePlaying(this);
    void IAudioSourceLifecycle.StopLifecyclePlayback(){if(Driver is IAudioSourceEventDriver sourceEvents)sourceEvents.Stop(this);}
    public void PlayOneShot(AudioClip clip,float volumeScale=1){
        ArgumentNullException.ThrowIfNull(clip);
        var driver=RequiredDriver;
        if(clip.loadState!=AudioDataLoadState.Loaded&&driver is not IDeferredAudioSourceDriver)
            throw new InvalidOperationException($"Audio sample {clip.SourceID} is not decoded");
        if(driver is IAudioSourceEventDriver sourceEvents)sourceEvents.PlayOneShot(this,clip,volumeScale,priority);
        else driver.PlayOneShot(clip,volumeScale,priority);
    }
    public void Play(){
        ArgumentNullException.ThrowIfNull(clip);
        var driver=RequiredDriver;
        if(clip.loadState!=AudioDataLoadState.Loaded&&driver is not IDeferredAudioSourceDriver)
            throw new InvalidOperationException($"Audio sample {clip.SourceID} is not decoded");
        if(driver is not IAudioClipPlaybackDriver playback)throw new NotSupportedException("Audio clip playback driver is not bound");
        if(driver is IAudioSourceEventDriver sourceEvents)sourceEvents.Play(this,clip,volume,pitch,loop,mute,priority);
        else playback.Play(clip,volume,pitch,loop,mute,priority);
    }
    public void Stop(){var driver=RequiredDriver;if(driver is IAudioSourceEventDriver sourceEvents)sourceEvents.Stop(this);else driver.Stop();}
}
