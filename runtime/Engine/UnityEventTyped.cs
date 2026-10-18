using System;

namespace UnityEngine.Events
{
    // Compatibility surface for Unity's typed event. Invocation and listener
    // ordering are delegated to .NET's ordered multicast invocation list.
    // Persistent serialized UnityEvent listeners require scene importer binding.
    public class UnityEvent<T>
    {
        private event Action<T> listeners;

        public void AddListener(Action<T> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            listeners += callback;
        }

        public void RemoveListener(Action<T> callback)
        {
            listeners -= callback;
        }

        public void RemoveAllListeners()
        {
            listeners = null;
        }

        public void Invoke(T argument)
        {
            listeners?.Invoke(argument);
        }
    }
}
