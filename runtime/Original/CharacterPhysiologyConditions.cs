using UnityEngine;
namespace Crisis.Runtime.Character;

// Executable physiology condition slice from original Physiology.cs lines
// 367-385. These are original thresholds, not invented health behavior.
// Full Physiology.Update, trauma propagation and limb injury dependencies
// must be integrated before claiming complete physiology.
public sealed class OriginalPhysiologyConditions {
 public float Shock {get;set;}
 public float Concussion {get;set;}
 public float BloodOxygen {get;set;}=1f;
 public float BrainTrauma {get;set;}
 public float ShockResistance {get;set;}
 public float MaxConsciousBrainTrauma {get;}=3f; // Original field default.
 public bool ShouldLoseFooting() {
  return Shock>=2500f;
 }
 public bool ShouldLoseConsciousness() {
  bool flag=Concussion>=0.775f;
  bool flag2=BloodOxygen<=0.5f;
  bool flag3=BrainTrauma>=MaxConsciousBrainTrauma;
  return flag||flag2||flag3;
 }
 public bool ShouldRegainConsciousness() {
  bool flag=Concussion>=0.4f;
  bool flag2=BloodOxygen<=0.6f;
  bool flag3=BrainTrauma>=MaxConsciousBrainTrauma;
  return !flag&&!flag2&&!flag3;
 }
 public void AddShock(float shock) {
  Shock+=shock*(1f-ShockResistance);
 }
}
