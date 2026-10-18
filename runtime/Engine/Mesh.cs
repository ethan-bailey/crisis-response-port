namespace UnityEngine;

// Unity mesh resource data shared by recovered mission fragment prefabs and
// native collision/render hosts. Mesh identity and indices are source-owned;
// this type does not construct artificial fragment or collider geometry.
public sealed class Mesh : Object {
    Vector3[] points=Array.Empty<Vector3>();
    int[] indices=Array.Empty<int>();
    Vector2[] coordinates=Array.Empty<Vector2>();
    Vector3[] surfaceNormals=Array.Empty<Vector3>();
    public Vector3[] vertices {
        get=>(Vector3[])points.Clone();
        set {
            ArgumentNullException.ThrowIfNull(value);
            if(value.Any(v=>!float.IsFinite(v.x)||!float.IsFinite(v.y)||!float.IsFinite(v.z)))
                throw new ArgumentOutOfRangeException(nameof(value),"Mesh vertices must be finite");
            if(indices.Any(i=>i>=value.Length))
                throw new ArgumentException("Existing triangle indices exceed replacement mesh vertices",nameof(value));
            points=(Vector3[])value.Clone();
            if(coordinates.Length!=0&&coordinates.Length!=points.Length)coordinates=Array.Empty<Vector2>();
            if(surfaceNormals.Length!=0&&surfaceNormals.Length!=points.Length)surfaceNormals=Array.Empty<Vector3>();
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
    public Vector2[] uv {
        get=>(Vector2[])coordinates.Clone();
        set {
            ArgumentNullException.ThrowIfNull(value);
            if(value.Length!=points.Length || value.Any(v=>!float.IsFinite(v.x)||!float.IsFinite(v.y)))
                throw new ArgumentException("Mesh UV coordinates must be finite and match the vertex count",nameof(value));
            coordinates=(Vector2[])value.Clone();
        }
    }
    public Vector3[] normals => (Vector3[])surfaceNormals.Clone();
    public void Clear(){points=Array.Empty<Vector3>();indices=Array.Empty<int>();coordinates=Array.Empty<Vector2>();surfaceNormals=Array.Empty<Vector3>();}
    public void RecalculateNormals(){
        var accumulated=new Vector3[points.Length];
        for(int i=0;i<indices.Length;i+=3){
            var a=indices[i];var b=indices[i+1];var c=indices[i+2];
            var normal=Vector3.Cross(points[b]-points[a],points[c]-points[a]);
            if(normal.sqrMagnitude<=1e-20f)continue;
            accumulated[a]+=normal;accumulated[b]+=normal;accumulated[c]+=normal;
        }
        for(int i=0;i<accumulated.Length;i++)accumulated[i]=accumulated[i].normalized;
        surfaceNormals=accumulated;
    }
    public int vertexCount=>points.Length;
}

// The original component owns the mesh instance assigned by its source.
public sealed class MeshFilter:Component {
    Mesh meshValue;
    public Mesh mesh {get=>meshValue;set=>meshValue=value;}
    public Mesh sharedMesh {get=>meshValue;set=>meshValue=value;}
}
