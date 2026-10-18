namespace UnityEngine;

// Unity mesh resource data shared by recovered mission fragment prefabs and
// native collision/render hosts. Mesh identity and indices are source-owned;
// this type does not construct artificial fragment or collider geometry.
public sealed class Mesh : Object {
    Vector3[] points=Array.Empty<Vector3>();
    int[] indices=Array.Empty<int>();
    public Vector3[] vertices {
        get=>(Vector3[])points.Clone();
        set {
            ArgumentNullException.ThrowIfNull(value);
            if(value.Any(v=>!float.IsFinite(v.x)||!float.IsFinite(v.y)||!float.IsFinite(v.z)))
                throw new ArgumentOutOfRangeException(nameof(value),"Mesh vertices must be finite");
            if(indices.Any(i=>i>=value.Length))
                throw new ArgumentException("Existing triangle indices exceed replacement mesh vertices",nameof(value));
            points=(Vector3[])value.Clone();
        }
    }
    public int[] triangles {
        get=>(int[])indices.Clone();
        set {
            ArgumentNullException.ThrowIfNull(value);
            if(value.Length%3!=0 || value.Any(i=>i<0 || i>=points.Length))
                throw new ArgumentException("Unity mesh triangles must reference valid vertex triplets",nameof(value));
            indices=(int[])value.Clone();
        }
    }
    public int vertexCount=>points.Length;
}
