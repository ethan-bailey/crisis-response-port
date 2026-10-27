// EngineChecks deliberately excludes the transport host that normally defines
// UnityAction. Keep the source delegate signatures here so typed UnityEvent
// contracts can run without compiling gameplay or a second actor owner.
namespace UnityEngine.Events
{
    public delegate void UnityAction();
    public delegate void UnityAction<T>(T value);
    public delegate void UnityAction<T, U>(T first, U second);
}

namespace UnityEngine
{
    // The isolated engine checks compile Object without the audio implementation.
    // Preserve only the driver contract required by RuntimeWorld's boundary.
    public interface IAudioSourceDriver { }

    // Object attachment wires this optional component to RuntimeWorld. The
    // isolated engine contracts do not exercise audio playback.
    public sealed class AudioSource : Behaviour, IAudioSourceLifecycle
    {
        public IAudioSourceDriver Driver { get; set; }
        bool IAudioSourceLifecycle.IsLifecyclePlaying => false;
        void IAudioSourceLifecycle.StopLifecyclePlayback() { }
    }
}
