namespace UnityEngine;

// Ray-query implementation of Unity CapsuleCollider with source prefab's
// axis convention (0=X, 1=Y, 2=Z). Dynamic contact solver stays PhysX JS;
// this component exposes actual geometric queries, not a no-op collider.
public sealed class CapsuleCollider : Collider {
 public Vector3 center {get;set;}=Vector3.zero;
 public float radius {get;set;}=.5f;
 public float height {get;set;}=2f;
 public int direction {get;set;}=1;
 public override bool Raycast(Ray ray,out RaycastHit hit,float maxDistance) {
  hit=default;
  if(!isActiveAndEnabled||maxDistance<0||radius<=0||height<=0||direction<0||direction>2)return false;
  var o=transform.InverseTransformPoint(ray.origin)-center;
  var d=transform.InverseTransformVector(ray.direction);
  if(d.sqrMagnitude<1e-10f)return false;
  float Half() => MathF.Max(0f,height*.5f-radius);
  float axisO=Axis(o),axisD=Axis(d),half=Half(),r2=radius*radius;
  float distAxis=MathF.Max(-half,MathF.Min(half,axisO));
  if((o-Unit()*distAxis).sqrMagnitude<r2)return false;
  float best=float.PositiveInfinity;
  Vector3 normal=Vector3.zero;
  void Candidate(float t,Vector3 n){
   if(t>=0f&&t<=maxDistance&&t<best){best=t;normal=n.normalized;}
  }
  void Sphere(Vector3 c){
   var rel=o-c;float a=Vector3.Dot(d,d),b=2*Vector3.Dot(rel,d),c0=Vector3.Dot(rel,rel)-r2;
   float disc=b*b-4*a*c0;if(disc<0)return;
   float t=(-b-MathF.Sqrt(disc))/(2*a);
   if(t>=0){var q=o+d*t-c;Candidate(t,q);}
  }
  Sphere(Unit()*half);Sphere(Unit()*-half);
  float aC=d.sqrMagnitude-axisD*axisD;
  if(aC>1e-8f){
   float bC=2*(Vector3.Dot(o,d)-axisO*axisD),cC=o.sqrMagnitude-axisO*axisO-r2;
   float disc=bC*bC-4*aC*cC;
   if(disc>=0){
    float t=(-bC-MathF.Sqrt(disc))/(2*aC);
    float along=axisO+axisD*t;
    if(t>=0&&along>=-half&&along<=half)Candidate(t,o+d*t-Unit()*along);
   }
  }
  if(!float.IsFinite(best))return false;
  var hitPoint=ray.GetPoint(best);
  hit=new RaycastHit{collider=this,point=hitPoint,normal=transform.TransformNormal(normal),distance=best};
  return true;
 }
 float Axis(Vector3 v)=>direction switch{0=>v.x,1=>v.y,_=>v.z};
 Vector3 Unit()=>direction switch{0=>Vector3.right,1=>Vector3.up,_=>Vector3.forward};
}
