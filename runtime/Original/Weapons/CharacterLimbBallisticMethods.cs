using UnityEngine;
using Crisis.Runtime.Character;

namespace Crisis.Runtime.Weapons;

// Recovered Limb : BallisticMaterial source interior/deflection method bodies.
// This is not enabled in live projectile queries until exact hit callbacks,
// texture-scoring and armor effects are verified end-to-end.
public partial class Limb {
 // Original Limb exit-accumulation state. This remains meaningful even with
 // wound decals disabled; dynamic renderer/GPU effects still require source bindings.
 public float exitAccumulationFactor=0.25f;
 // Developer Limb.radius/aimpoint and AimPoint property are used by real
 // exit-wound placement. Bind the authored prefab's per-limb coordinates.
 public float radius=0.15f;
 public Vector2 aimpoint;
 public Vector3 AimPoint=>transform.TransformPoint(new Vector3(aimpoint.x,aimpoint.y,0f));
 private float lastExitTimestamp;
 private float accumulatedExitVolume;
 public LimbArmor Armor=>GetComponent<LimbArmor>();

 // From original Limb.HandleDeflection: energy converted into force and shock.
 protected override Projectile HandleDeflection(Projectile projectile,Vector3 normal,bool simulation=false){
  float energy=projectile.Energy;
  base.HandleDeflection(projectile,normal,simulation);
  float num=energy-projectile.Energy;
  if(num>0f){
   if(LimbPhysiology==null)throw new InvalidOperationException("Original limb has no physiology for projectile deflection: "+id);
   AddTrauma(0f,projectile.transform.forward*num*projectile.ForceMultiplier,projectile.transform.position);
   LimbPhysiology.AddShock(num*projectile.Ammunition.shockModifier);
  }
  return projectile;
 }

 // From original Limb.ArmorEntryTest, preserving source penetration curve.
 Projectile ArmorEntryTest(Projectile projectile){
  float energy=projectile.Energy;
  Armor.Test(projectile,entry:true);
  float num=energy-projectile.Energy;
  if(num>0f){
   AddTrauma(0f,projectile.transform.forward*num*projectile.ForceMultiplier,projectile.transform.position);
   LimbPhysiology.AddShock(num);
  }
  return projectile;
 }

 // Original Limb.HandleEntry delegates the actual thickness and friction math
 // to unchanged compiled developer BallisticMaterial.HandleEntry.
 protected override Projectile HandleEntry(Projectile projectile,Vector3 normal,bool simulation=false){
  if(LimbPhysiology==null)
   throw new InvalidOperationException("Unresolved original hand LimbPhysiology interaction: "+id);
  if(Armor!=null&&Armor.type!=ArmorClass.Type.None)
   ArmorEntryTest(projectile);
  // Original PlayImpactAudio requires source Limb.Audio and sound events;
  // do not synthesize a gameplay sound as proof of source integration.
  float energy=projectile.Energy;
  base.HandleEntry(projectile,normal,simulation);
  float num=energy-projectile.Energy;
  AddTrauma(projectile.DisplacedMaterialVolume,projectile.transform.forward*num*projectile.ForceMultiplier,projectile.transform.position);
  LimbPhysiology.AddShock(num);
  return projectile;
 }

 // Original Limb.FindInternalDistanceAndExitPosition / FindEntryExitPoints.
 // Uses the actual author PNG alpha-derived skin mask, never capsule length.
 protected override void FindInternalDistanceAndExitPosition(Projectile projectile){
  if(SourceSkinTrace==null){
   // Original source logs "no LimbRenderer" and returns; no guessed hitbox
   // thickness is inserted here.
   availableInternalDistance=0f;
   foundPotentialExit=false;
   return;
  }
  availableInternalDistance=0f;
  foundPotentialExit=false;
  if(projectile.FiredByMarksman){
   Vector3 position=projectile.transform.position;
   position.z=transform.position.z;
   Vector3 vector=position-projectile.transform.position;
   Vector3 vector2=Vector3.Project(vector,projectile.transform.forward);
   projectile.transform.position+=vector2;
  }
  var trace=projectile.FiredByMarksman
    ?SourceSkinTrace.FindMarksmanEntryAndExit(projectile.transform.position,projectile.transform.forward,
        ref worldEntryPosition,ref worldExitPosition)
    :SourceSkinTrace.FindEntryExit(projectile.transform.position,projectile.transform.forward,
        0.45f,ref worldEntryPosition,ref worldExitPosition);
  if(trace==OriginalLimbSkinTrace.TraceResult.EntryExit){
   foundPotentialExit=true;
   availableInternalDistance=projectile.FiredByMarksman?depth:(worldEntryPosition-worldExitPosition).magnitude;
  }
 }

 // Unchanged source BallisticMaterial.HandleInterior is the actual energy /
 // permanent-cavity solver; use the exact returned source displacement and
 // organ-specific recovered VitalFunctions rather than a generic HP penalty.
 protected override Projectile HandleInterior(Projectile projectile,Vector3 normal,bool simulation=false){
  if(LimbPhysiology==null)throw new InvalidOperationException("Missing source physiology for limb interior: "+id);
  float energy=projectile.Energy;
  float diameter=projectile.Diameter;
  float temporalCavityFactor=projectile.TemporalCavityFactor;
  base.HandleInterior(projectile,normal,simulation);
  float num=energy-projectile.Energy;
  float shock=num*temporalCavityFactor;
  AddTrauma(projectile.DisplacedMaterialVolume,
    projectile.transform.forward*num*projectile.ForceMultiplier,projectile.transform.position);
  SourceVitals?.HitTest(projectile.DisplacedMaterialVolume,worldEntryPosition,worldExitPosition,shock);
  LimbPhysiology.AddShock(shock);
  float num2=projectile.Ammunition.diameter*.5f;
  float entryHoleModifier=projectile.Ammunition.entryHoleModifier;
  float num3=num2*3f*entryHoleModifier;
  LimbPhysiology.CreateWoundFromSourceProjectile(this,projectile.Diameter,
    projectile.DisplacedMaterialVolume,num3*.0005f,worldEntryPosition,normal);
  // Source LimbRenderer.DrawDamage/DrawBurn operate on live GPU dynamic wound
  // textures. They are not yet ported; source skin CPU mask remains unchanged.
  return projectile;
 }

 // Original armor exit check, preserving the inherited source material exit
 // energy, without fabricating unimplemented dynamic skin decals or gore.
 protected override void HandleExit(Projectile projectile,bool simulated=false){
  base.HandleExit(projectile,simulated);
  if(projectile.Status!=BallisticResult.Continue)return;
  AccumulateExitingProjectiles(projectile);
  if(Armor!=null&&Armor.type!=ArmorClass.Type.None)Armor.Test(projectile,entry:false);
 }
 // Unchanged developer Limb.AccumulateExitingProjectiles behavior:
 // same-frame exits accumulate within 0.0167s up to 1000; otherwise reset.
 private void AccumulateExitingProjectiles(Projectile projectile)
 {
  if (Time.time - lastExitTimestamp < 0.0167f)
  {
   accumulatedExitVolume = Mathf.Min(accumulatedExitVolume + projectile.DisplacedMaterialVolume, 1000f);
  }
  else
  {
   accumulatedExitVolume = 0f;
  }
  lastExitTimestamp = Time.time;
 }
}
