namespace UnityEngine;

public enum AudioDataLoadState {Unloaded=0,Loading=1,Loaded=2,Failed=3}
public sealed class AudioClip:Object {
    public string SourceID {get;}
    public float length {get;}
    public AudioDataLoadState loadState {get;private set;}=AudioDataLoadState.Unloaded;
    public AudioClip(string sourceID,float length){
        ArgumentException.ThrowIfNullOrEmpty(sourceID);if(!float.IsFinite(length)||length<0)throw new ArgumentOutOfRangeException(nameof(length));
        SourceID=sourceID;this.length=length;world=RuntimeWorld.Current;world.Register(this);
    }
    // The browser/native decoder reports this; a source declaration alone is
    // not evidence that a sample was decoded or played on the audio device.
    public void ReportLoadState(AudioDataLoadState state)=>loadState=state;
}
public interface IAudioSourceDriver {
    bool IsPlaying {get;}
    void PlayOneShot(AudioClip clip,float volume,int priority);
    void Stop();
}
public sealed class AudioSource:Behaviour {
    public int priority {get;set;}=128;
    public IAudioSourceDriver Driver {get;set;}
    IAudioSourceDriver RequiredDriver=>Driver??throw new NotSupportedException("Decoded original audio playback driver is not bound");
    public bool isPlaying=>RequiredDriver.IsPlaying;
    public void PlayOneShot(AudioClip clip,float volumeScale=1){
        ArgumentNullException.ThrowIfNull(clip);
        if(clip.loadState!=AudioDataLoadState.Loaded)throw new InvalidOperationException($"Audio sample {clip.SourceID} is not decoded");
        RequiredDriver.PlayOneShot(clip,volumeScale,priority);
    }
    public void Stop()=>RequiredDriver.Stop();
}
