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
 BallisticResult HandleLimbHit(RaycastHit hit)=>throw new NotSupportedException("Limb ballistics unavailable");
}
