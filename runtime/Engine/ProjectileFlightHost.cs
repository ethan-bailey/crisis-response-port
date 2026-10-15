using UnityEngine;
using Crisis.Runtime.Weapons;
// Selected non-explosive source projectile lifecycle. Material/limb interaction is
// explicitly unavailable, so an imported collider without a material returns Error.
public enum BallisticResult {Continue,Stop,Error}
public class TutorialSoftTarget:Component {}
public partial class Projectile {
 public int Id,SwarmId,FlightMask;public float ForceMultiplier,SpawnTime,Spin,RiflingTwistRate;
 public bool Finished,FiredWithPrecision,IsStray;public Vector3 InteractionPosition,Tumble,NextPosition;
 float segmentBeginMagnitude,segmentEndMagnitude;Ray raycastRay;RaycastHit nonDiscriminatingResult,limbRaycastResult;
 int ballisticMask=>FlightMask;int limbMask=>0;
 public void InitializeXY(WeaponBase weapon,bool usePrecision,float segmentStart=0,float segmentLength=0,float velocityModifier=1){
  if(usePrecision)throw new NotSupportedException("Precision limb interactions unavailable");
  Ammunition=weapon.ammunition;Mass=Ammunition.mass;diameter=Ammunition.diameter;
  InitializeVelocity(velocityModifier);SpawnTime=Time.time;freeFallTimestamp=Time.time;
  StartPosition=transform.position;InteractionPosition=transform.position;
  if(weapon.rifledBarrel){RiflingTwistRate=weapon.rifleTwistRate;AssignNewSpin();}
 }
 BallisticResult HandleGenericHit(RaycastHit hit){RecalculateVelocity();Finished=true;return BallisticResult.Error;}
 BallisticResult HandleLimbHit(RaycastHit hit)=>throw new NotSupportedException("Limb ballistics unavailable");
}
