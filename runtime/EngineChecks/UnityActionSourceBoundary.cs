// EngineChecks deliberately excludes the transport host that normally defines
// UnityAction. Keep the source delegate signatures here so typed UnityEvent
// contracts can run without compiling gameplay or a second actor owner.
namespace UnityEngine.Events;

public delegate void UnityAction();
public delegate void UnityAction<T>(T value);
public delegate void UnityAction<T, U>(T first, U second);
