using System;

namespace UnityEngine.Events
{
    public class UnityEvent<T,U>
    {
        private event UnityAction<T,U> listeners;
        public void AddListener(UnityAction<T,U> callback) { ArgumentNullException.ThrowIfNull(callback); listeners+=callback; }
        public void RemoveListener(UnityAction<T,U> callback) { listeners-=callback; }
        public void RemoveAllListeners() { listeners=null; }
        public void Invoke(T first,U second) { listeners?.Invoke(first,second); }
    }
    // Compatibility surface for Unity's typed event. Invocation and listener
    // ordering are delegated to .NET's ordered multicast invocation list.
    // Persistent serialized UnityEvent listeners require scene importer binding.
    public class UnityEvent<T>
    {
        private event UnityAction<T> listeners;

        public void AddListener(UnityAction<T> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            listeners += callback;
        }

        public void RemoveListener(UnityAction<T> callback)
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
