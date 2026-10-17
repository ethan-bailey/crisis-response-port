using UnityEngine;
using Crisis.Runtime.Weapons;
// Selected non-explosive source projectile lifecycle. Material interactions are
// executed by the recovered source; limb physiology remains unavailable.
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
 // The original Projectile.HandleLimbHit order is: contact -> ballistic tissue
 // interaction -> attacker/victim notifications -> interaction events.
 // The original CharacterBody.OnWasHit / OnWasAttacked / mission score/AI
 // dependencies are not yet executable in this host. Require the genuine
 // notification bridge BEFORE modifying limb physiology; never silently
 // discard callbacks or leave a half-applied injury.
 public Action<Projectile,CharacterBody> RecordOriginalAttackInfo;
 public Action<Projectile,CharacterBody,Limb> NotifyOriginalVictimHit;
 public Action<Projectile> OriginalInteraction;
 public Action<Projectile,Limb> OriginalLimbInteraction;
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
  if(ReferenceEquals(limb.Body,Owner))
   throw new NotSupportedException("Original owner collision filtering is not yet connected");
  if(limb.Body==null||limb.LimbPhysiology==null)
   throw new InvalidOperationException("Original limb collision lacks its CharacterBody or LimbPhysiology");
  if(RecordOriginalAttackInfo==null||NotifyOriginalVictimHit==null||
     OriginalInteraction==null||OriginalLimbInteraction==null)
   throw new NotSupportedException("Original projectile attack, victim, mission and interaction callbacks are not all bound");
  transform.position=hit.point;
  RecalculateVelocity();
  Projectile projectile=limb.HandleProjectile(this,hit.normal);
  RecordOriginalAttackInfo(this,limb.Body);
  NotifyOriginalVictimHit(this,limb.Body,limb);
  OriginalInteraction(this);
  OriginalLimbInteraction(this,limb);
  return projectile.Status;
 }

}
