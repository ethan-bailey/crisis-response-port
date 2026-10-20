namespace UnityEngine;

public struct Ray {
    public Vector3 origin,direction;
    public Ray(Vector3 origin,Vector3 direction){this.origin=origin;this.direction=direction.normalized;}
    public Vector3 GetPoint(float distance)=>origin+direction*distance;
}
public struct RaycastHit {
    public Collider collider;public Vector3 point,normal;public float distance;
    public Transform transform=>collider?.transform;
}
public enum QueryTriggerInteraction {UseGlobal,Ignore,Collide}
public abstract class Collider:Behaviour {
    public PhysicMaterial sharedMaterial {get;set;}
    public Rigidbody attachedRigidbody=>GetComponentInParent<Rigidbody>();
    public bool isTrigger {get;set;}
    public Bounds bounds {
        get {
            Vector3 min=Vector3.zero,max=Vector3.zero;bool initialized=false;
            void Include(Vector3 point){if(!initialized){min=max=point;initialized=true;}else{min=new(MathF.Min(min.x,point.x),MathF.Min(min.y,point.y),MathF.Min(min.z,point.z));max=new(MathF.Max(max.x,point.x),MathF.Max(max.y,point.y),MathF.Max(max.z,point.z));}}
            if(this is BoxCollider box){var half=box.size*0.5f;for(int i=0;i<8;i++)Include(transform.TransformPoint(box.center+new Vector3((i&1)==0?-half.x:half.x,(i&2)==0?-half.y:half.y,(i&4)==0?-half.z:half.z)));}
            else if(this is CapsuleCollider capsule){var(a,b,r)=capsule.Geometry();var e=new Vector3(r,r,r);Include(a-e);Include(a+e);Include(b-e);Include(b+e);}
            else if(this is MeshCollider mesh&&mesh.vertices!=null){foreach(var vertex in mesh.vertices)Include(transform.TransformPoint(vertex));}
            else throw new NotSupportedException("Collider bounds require a supported source collider shape");
            return new Bounds((min+max)*0.5f,max-min);
        }
    }
    public abstract bool Raycast(Ray ray,out RaycastHit hit,float maxDistance);
}
public sealed class BoxCollider:Collider {
    public Vector3 center {get;set;}=Vector3.zero;
    public Vector3 size {get;set;}=Vector3.one;
    public override bool Raycast(Ray ray,out RaycastHit hit,float maxDistance){
        hit=default;if(!isActiveAndEnabled||maxDistance<0||ray.direction.sqrMagnitude<1e-10f)return false;
        var origin=transform.InverseTransformPoint(ray.origin)-center;
        var direction=transform.InverseTransformVector(ray.direction);
        float[] o=[origin.x,origin.y,origin.z], d=[direction.x,direction.y,direction.z], half=[MathF.Abs(size.x)/2,MathF.Abs(size.y)/2,MathF.Abs(size.z)/2];
        // Unity ray queries do not detect a collider containing the ray origin.
        if(Enumerable.Range(0,3).All(i=>MathF.Abs(o[i])<half[i]))return false;
        float near=0,far=maxDistance;int normalAxis=-1;float normalSign=0;
        for(int axis=0;axis<3;axis++){
            if(MathF.Abs(d[axis])<1e-10f){if(o[axis]<-half[axis]||o[axis]>half[axis])return false;continue;}
            float entry=(-half[axis]-o[axis])/d[axis],exit=(half[axis]-o[axis])/d[axis],sign=-1;
            if(entry>exit){(entry,exit)=(exit,entry);sign=1;}
            if(entry>near){near=entry;normalAxis=axis;normalSign=sign;}far=MathF.Min(far,exit);
            if(near>far)return false;
        }
        if(normalAxis<0||near<0||near>maxDistance)return false;
        var normal=normalAxis switch{0=>new Vector3(normalSign,0,0),1=>new Vector3(0,normalSign,0),_=>new Vector3(0,0,normalSign)};
        hit=new RaycastHit{collider=this,distance=near,point=ray.GetPoint(near),normal=transform.TransformNormal(normal)};return true;
    }
}
public static class Physics {
    public static bool queriesHitTriggers {get;set;}=true;
    public static RaycastHit[] RaycastAll(Ray ray,float maxDistance=float.PositiveInfinity,int layerMask=-5,QueryTriggerInteraction queryTriggerInteraction=QueryTriggerInteraction.UseGlobal){
        bool triggers=queryTriggerInteraction==QueryTriggerInteraction.Collide||(queryTriggerInteraction==QueryTriggerInteraction.UseGlobal&&queriesHitTriggers);
        var hits=new List<RaycastHit>();
        foreach(var collider in RuntimeWorld.Current.Objects.OfType<Collider>())
            if(((1<<collider.gameObject.layer)&layerMask)!=0&&(triggers||!collider.isTrigger)&&collider.Raycast(ray,out var hit,maxDistance))hits.Add(hit);
        return hits.OrderBy(h=>h.distance).ThenBy(h=>h.collider.GetInstanceID()).ToArray();
    }
    public static bool Raycast(Ray ray,out RaycastHit hit,float maxDistance=float.PositiveInfinity,int layerMask=-5,QueryTriggerInteraction queryTriggerInteraction=QueryTriggerInteraction.UseGlobal){var hits=RaycastAll(ray,maxDistance,layerMask,queryTriggerInteraction);hit=hits.FirstOrDefault();return hits.Length>0;}
    public static bool Raycast(Vector3 origin,Vector3 direction,out RaycastHit hit,float distance,int mask)=>Raycast(new Ray(origin,direction),out hit,distance,mask);
    public static bool Raycast(Ray ray,float maxDistance,int layerMask=-5)=>Raycast(ray,out _,maxDistance,layerMask);
    public static bool Raycast(Vector3 origin,Vector3 direction,float distance,int layerMask=-5)=>Raycast(new Ray(origin,direction),out _,distance,layerMask);
    public static Collider[] OverlapSphere(Vector3 position,float radius,int layerMask=-1,QueryTriggerInteraction queryTriggerInteraction=QueryTriggerInteraction.UseGlobal){
        if(!float.IsFinite(radius)||radius<0)throw new ArgumentOutOfRangeException(nameof(radius));
        if(RuntimeWorld.Current.OverlapSphereQuery is {} query)return query(position,radius,layerMask,queryTriggerInteraction);
        bool triggers=queryTriggerInteraction==QueryTriggerInteraction.Collide||(queryTriggerInteraction==QueryTriggerInteraction.UseGlobal&&queriesHitTriggers);
        var hits=new List<Collider>();
        foreach(var collider in RuntimeWorld.Current.Objects.OfType<Collider>()){
            if(!collider.isActiveAndEnabled||((1<<collider.gameObject.layer)&layerMask)==0||(!triggers&&collider.isTrigger))continue;
            if(collider is CapsuleCollider capsule){var(a,b,r)=capsule.Geometry();if((position-CapsuleCollider.SegmentPoint(position,a,b)).sqrMagnitude<=(r+radius)*(r+radius))hits.Add(collider);}
            else if(collider is BoxCollider box){
                var axes=new[]{box.transform.TransformVector(Vector3.right),box.transform.TransformVector(Vector3.up),box.transform.TransformVector(Vector3.forward)};
                for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)if(MathF.Abs(Vector3.Dot(axes[i].normalized,axes[j].normalized))>1e-4f)throw new NotSupportedException("Sheared box overlap requires native PhysX queries");
                var local=box.transform.InverseTransformPoint(position)-box.center;var half=box.size*.5f;
                var closest=new Vector3(Math.Clamp(local.x,-MathF.Abs(half.x),MathF.Abs(half.x)),Math.Clamp(local.y,-MathF.Abs(half.y),MathF.Abs(half.y)),Math.Clamp(local.z,-MathF.Abs(half.z),MathF.Abs(half.z)));
                if((position-box.transform.TransformPoint(closest+box.center)).sqrMagnitude<=radius*radius)hits.Add(collider);
            }else throw new NotSupportedException($"Native PhysX overlap query is required for {collider.GetType().Name}");
        }
        return hits.ToArray();
    }
}

