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
    public Rigidbody attachedRigidbody=>GetComponentInParent<Rigidbody>();
    public bool isTrigger {get;set;}
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
}
