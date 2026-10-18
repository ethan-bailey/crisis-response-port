namespace UnityEngine;

// Analytic query shape, matching the primitive used by the PhysX bridge.
// Capsule dimensions include both hemispheres. Nonuniform scale preserves
// circular caps using the larger of the two perpendicular scale factors.
public sealed class CapsuleCollider:Collider {
    public Vector3 center {get;set;}
    public float radius {get;set;}=.5f;
    public float height {get;set;}=2f;
    int axisIndex=1;
    public int direction {get=>axisIndex;set{if(value<0||value>2)throw new ArgumentOutOfRangeException(nameof(value));axisIndex=value;}}
    internal (Vector3 a,Vector3 b,float radius) Geometry(){
        if(!float.IsFinite(radius)||!float.IsFinite(height)||radius<0||height<0)throw new InvalidOperationException("Invalid capsule dimensions");
        Vector3[] basis=[transform.TransformVector(Vector3.right),transform.TransformVector(Vector3.up),transform.TransformVector(Vector3.forward)];
        for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)
            if(MathF.Abs(Vector3.Dot(basis[i].normalized,basis[j].normalized))>1e-4f)
                throw new NotSupportedException("Sheared capsule transforms require native PhysX shape geometry");
        var scale=basis.Select(v=>v.magnitude).ToArray();
        if(scale.Any(s=>s<=0))throw new NotSupportedException("Singular capsule transform");
        float r=radius*MathF.Max(scale[(direction+1)%3],scale[(direction+2)%3]);
        float half=MathF.Max(0,height*scale[direction]*.5f-r);
        var c=transform.TransformPoint(center);var axis=basis[direction].normalized;
        return(c-axis*half,c+axis*half,r);
    }
    internal static Vector3 SegmentPoint(Vector3 p,Vector3 a,Vector3 b){var ab=b-a;return a+ab*(ab.sqrMagnitude==0?0:Math.Clamp(Vector3.Dot(p-a,ab)/ab.sqrMagnitude,0,1));}
    public override bool Raycast(Ray ray,out RaycastHit hit,float maxDistance){
        hit=default;if(!isActiveAndEnabled||maxDistance<0||ray.direction.sqrMagnitude<1e-10f)return false;
        var(a,b,r)=Geometry();if(r<=0)return false;
        if((ray.origin-SegmentPoint(ray.origin,a,b)).sqrMagnitude<r*r)return false;
        var axis=(b-a).normalized;float length=(b-a).magnitude;
        float closest=float.PositiveInfinity;Vector3 normal=Vector3.zero;
        void Consider(float t){if(t<0||t>maxDistance||t>=closest)return;var p=ray.GetPoint(t);var n=p-SegmentPoint(p,a,b);if(n.sqrMagnitude<1e-12f)return;closest=t;normal=n.normalized;}
        // Cylinder intersections restricted to the finite axial segment.
        var oa=ray.origin-a;var rd=ray.direction;
        float da=Vector3.Dot(rd,axis),oaxis=Vector3.Dot(oa,axis);
        var dperp=rd-axis*da;var operp=oa-axis*oaxis;
        float qa=dperp.sqrMagnitude,qb=Vector3.Dot(operp,dperp),qc=operp.sqrMagnitude-r*r;
        float disc=qb*qb-qa*qc;
        if(length>0&&qa>1e-10f&&disc>=0){float root=MathF.Sqrt(disc);foreach(float t in new[]{(-qb-root)/qa,(-qb+root)/qa}){float y=oaxis+t*da;if(y>=0&&y<=length)Consider(t);}}
        // Only the outer hemisphere of each end contributes to the surface.
        foreach(var cap in new[]{(a,false),(b,true)}){
            var oc=ray.origin-cap.Item1;float qb2=Vector3.Dot(oc,rd);float disc2=qb2*qb2-oc.sqrMagnitude+r*r;
            if(disc2<0)continue;
            float root=MathF.Sqrt(disc2);
            foreach(float t in new[]{-qb2-root,-qb2+root}){
                float y=Vector3.Dot(ray.GetPoint(t)-a,axis);
                if(length==0||(cap.Item2?y>=length:y<=0))Consider(t);
            }
        }
        if(!float.IsFinite(closest))return false;
        hit=new(){collider=this,distance=closest,point=ray.GetPoint(closest),normal=normal};return true;
    }
}

public readonly struct ContactPoint {
    public Vector3 point {get;init;}
    public Vector3 normal {get;init;}
    public float separation {get;init;}
    public Collider thisCollider {get;init;}
    public Collider otherCollider {get;init;}
}
public sealed class Collision {
    readonly ContactPoint[] points;
    public Collider collider {get;}
    public Rigidbody rigidbody=>collider.attachedRigidbody;
    public Transform transform=>(rigidbody!=null?rigidbody.transform:collider.transform);
    public GameObject gameObject=>transform.gameObject;
    public Vector3 relativeVelocity {get;}
    public Vector3 impulse {get;}
    public ContactPoint[] contacts=>(ContactPoint[])points.Clone();
    public int contactCount=>points.Length;
    public ContactPoint GetContact(int index)=>points[index];
    public Collision(Collider collider,Vector3 relativeVelocity,Vector3 impulse,params ContactPoint[] contacts){
        this.collider=collider??throw new ArgumentNullException(nameof(collider));this.relativeVelocity=relativeVelocity;this.impulse=impulse;points=(ContactPoint[])contacts.Clone();
    }
}
