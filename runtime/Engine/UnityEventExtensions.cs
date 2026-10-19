global using UnityEngine.Events;
using System;
using System.Reflection;

namespace UnityEngine.Events
{
    public static class UnityEventExtensions
    {
        private static readonly FieldInfo Callbacks = typeof(UnityEvent).GetField(
            "callbacks", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void RemoveListener(this UnityEvent unityEvent, UnityAction callback)
        {
            ArgumentNullException.ThrowIfNull(unityEvent);
            if (Callbacks == null)
                throw new MissingFieldException(typeof(UnityEvent).FullName, "callbacks");
            var current = (Delegate)Callbacks.GetValue(unityEvent);
            if (current != null) Callbacks.SetValue(unityEvent, Delegate.Remove(current, callback));
        }

        public static void RemoveAllListeners(this UnityEvent unityEvent)
        {
            ArgumentNullException.ThrowIfNull(unityEvent);
            if (Callbacks == null)
                throw new MissingFieldException(typeof(UnityEvent).FullName, "callbacks");
            Callbacks.SetValue(unityEvent, null);
        }
    }
}
