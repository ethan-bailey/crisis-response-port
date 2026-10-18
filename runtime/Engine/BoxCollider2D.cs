namespace UnityEngine;

// Serialized source 2D limb-skin bounds. The authoritative wound collision
// remains the parent 3D limb Collider; this component does not simulate 2D physics.
public sealed class BoxCollider2D : Collider2D {
    Vector2 boundsSize=new(1f,1f);
    Vector2 boundsOffset=Vector2.zero;
    public Vector2 size {
        get=>boundsSize;
        set {
            if (!float.IsFinite(value.x)||!float.IsFinite(value.y)||value.x<0||value.y<0)
                throw new ArgumentOutOfRangeException(nameof(value));
            boundsSize=value;
        }
    }
    public Vector2 offset {
        get=>boundsOffset;
        set {
            if (!float.IsFinite(value.x)||!float.IsFinite(value.y))
                throw new ArgumentOutOfRangeException(nameof(value));
            boundsOffset=value;
        }
    }
}
