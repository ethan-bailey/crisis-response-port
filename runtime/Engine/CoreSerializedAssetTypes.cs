namespace UnityEngine;

public class RuntimeAnimatorController : Object { }

public sealed class PhysicMaterial : Object {
    public float dynamicFriction { get; set; }
    public float staticFriction { get; set; }
    public float bounciness { get; set; }
    public int frictionCombine { get; set; }
    public int bounceCombine { get; set; }
}
