using UnityEngine;
using Crisis.Runtime.Weapons;
// Selected non-explosive source projectile lifecycle. Material interactions are
// executed by recovered source with explicit combat dependency preflight.
public class TutorialSoftTarget:Component {}
public partial class Projectile {
 public int Id {get;set;} public int SwarmId {get;set;} public int FlightMask {get;set;}
 public float ForceMultiplier {get;set;} public float SpawnTime {get;set;} public float Spin {get;set;} public float RiflingTwistRate {get;set;}
 public bool Finished {get;set;} public bool FiredWithPrecision {get;set;} public bool IsStray {get;set;}
 public Vector3 InteractionPosition {get;set;} public Vector3 Tumble {get;set;} public Vector3 NextPosition {get;set;}
 float segmentBeginMagnitude,segmentEndMagnitude;Ray raycastRay;RaycastHit nonDiscriminatingResult,limbRaycastResult;
 int ballisticMask=>FlightMask;int limbMask=>0;
 public void InitializeXY(WeaponBase weapon,bool usePrecision,float segmentStart=0,float segmentLength=0,float velocityModifier=1){
  if(usePrecision)throw new NotSupportedException("Precision limb interactions unavailable");
  Owner=weapon.Owner;Status=BallisticResult.Continue;Ammunition=weapon.ammunition;Mass=Ammunition.mass;diameter=Ammunition.diameter;
  InitializeVelocity(velocityModifier);SpawnTime=Time.time;freeFallTimestamp=Time.time;
  StartPosition=transform.position;InteractionPosition=transform.position;
  if(weapon.rifledBarrel){RiflingTwistRate=weapon.rifleTwistRate;AssignNewSpin();}
 }
 // Source UnityEvent boundaries permit zero listeners, as native Unity does.
 public ProjectileLimbEvent onLimbInteraction=new();
 // Engine query adapter excludes only the owning character, retaining nearest
 // non-owner material/limb hits, triggers and the exact requested layer mask.
 bool FlightRaycast(Ray ray,out RaycastHit hit,float distance,int mask){
  foreach(var candidate in Physics.RaycastAll(ray,distance,mask)){
   var limb=candidate.collider.GetComponent<Limb>();
   if(limb!=null&&Owner!=null&&ReferenceEquals(limb.Body,Owner))continue;
   hit=candidate;return true;
  }
  hit=default;return false;
 }
 BallisticResult HandleLimbHit(RaycastHit hit){
  if(hit.collider==null)return BallisticResult.Error;
  var limb=hit.collider.GetComponent<Limb>();
  if(limb==null){
   var material=hit.collider.GetComponent<BallisticMaterial>();
   if(material==null)return BallisticResult.Error;
   transform.position=hit.point;
   RecalculateVelocity();
   return material.HandleProjectile(this,hit.normal).Status;
  }
  if(Owner!=null&&ReferenceEquals(limb.Body,Owner))
   return BallisticResult.Continue;
  if(limb.Body==null||limb.LimbPhysiology==null)
   throw new InvalidOperationException("Original limb collision lacks its CharacterBody or LimbPhysiology");
  if(!limb.Body.limbs.Contains(limb))
   throw new InvalidOperationException("Collision limb is not registered on its source CharacterBody");
  if(limb.Body.SourcePhysiology==null)
   throw new InvalidOperationException("Collision victim has no initialized source blood/oxygen physiology");
  limb.Body.RequireOriginalCombat(victimHit:true);
  if(FiredByMarksman){
   if(MarksmanOwner is not IOriginalMarksmanAttack marksman)
    throw new NotSupportedException("Original Marksman success notification is not bound");
   marksman.ValidateBindings();
  }else if(Owner!=null)Owner.RequireOriginalCombat(victimHit:false);
  transform.position=hit.point;
  RecalculateVelocity();
  Projectile projectile=limb.HandleProjectile(this,hit.normal);
  RecordAttackInfo(limb.Body);
  limb.Body.LastHitLimb=limb;
  limb.Body.OnWasHit(this);
  onInteraction.Invoke(this);
  onLimbInteraction.Invoke(this,limb);
  return projectile.Status;
 }

}


// Engine equivalent of original UnityEvent<Projectile,Limb>.
[Serializable] public sealed class ProjectileLimbEvent {
 event Action<Projectile,Limb> listeners;
 public void AddListener(Action<Projectile,Limb> listener)=>listeners+=listener;
 public void Invoke(Projectile p,Limb limb)=>listeners?.Invoke(p,limb);
}
