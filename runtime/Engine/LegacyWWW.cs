using System.Collections;

namespace UnityEngine;

// Minimal compatibility contract for the original ResourceManager coroutine.
// The browser/native host must provide a decoded audio clip; this type never
// treats a URL or downloaded byte buffer as successful audio playback.
public interface ILegacyAudioRequestDriver {
    Task<AudioClip> LoadAudioClip(string uri,bool stream,bool threeD);
}

public sealed class WWW : IEnumerator {
    public static ILegacyAudioRequestDriver Driver {get;set;}
    readonly Task<AudioClip> audioTask;
    public WWW(string url) {
        ArgumentException.ThrowIfNullOrEmpty(url);
        var driver=Driver??throw new NotSupportedException("Legacy audio request driver is not bound");
        audioTask=driver.LoadAudioClip(url,false,false)??throw new InvalidOperationException("Legacy audio driver returned no task");
    }
    public object Current=>null;
    public bool MoveNext()=>!audioTask.IsCompleted;
    public void Reset()=>throw new NotSupportedException();
    public AudioClip GetAudioClip(bool threeD,bool stream) {
        if(!audioTask.IsCompleted)throw new InvalidOperationException("Audio request has not completed");
        return audioTask.GetAwaiter().GetResult()??throw new InvalidOperationException("Audio request produced no clip");
    }
}
