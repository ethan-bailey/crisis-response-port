using UnityEngine;
using Crisis.Runtime.Character;

namespace Crisis.Runtime.Weapons;

// Executable original IKArmController lifecycle binding. The recovered
// IKArmSequencer drives the REAL character's original ArmIKSolver pair. Only
// actions whose actual developer methods exist in the port are dispatched;
// other original callbacks fail explicitly rather than pretending to work.
public sealed class OriginalIKArmControllerLive {
 readonly CharacterBody self;
 readonly OriginalIKArmControllerPlayback sequence;
 readonly Func<float> sourceWeaponAlignment;
 public WeaponBase PreviousWeapon {get;set;}
 public WeaponBase PendingWeapon {get;set;}
 public bool IsPlaying=>sequence.IsPlaying;
 public int RightFrame=>sequence.RightSequencer.CurrentFrame;
 public int LeftFrame=>sequence.LeftSequencer.CurrentFrame;

 public OriginalIKArmControllerLive(CharacterBody character,Func<float> weaponAimAlignment) {
  self=character??throw new ArgumentNullException(nameof(character));
  if(self.rightArm==null||self.leftArm==null)
    throw new InvalidOperationException("Original IKArmController requires both source ArmIKSolvers");
  sourceWeaponAlignment=weaponAimAlignment??throw new ArgumentNullException(nameof(weaponAimAlignment));
  var bindings=new OriginalIKSolverBindings(self,self.rightArm,self.leftArm,
    ()=>sourceWeaponAlignment(),ResolveTarget,Invoke);
  sequence=new OriginalIKArmControllerPlayback(bindings);
 }
 public void Play(IKArmSequenceRuntime source,float sourceTime){
  if(source==null)throw new ArgumentNullException(nameof(source));
  sequence.Play(source,sourceTime);
 }
 public void Update(float sourceTime)=>sequence.Update(sourceTime);

 // Original IKArmSequencer.TargetTransform cases. Unimplemented target classes
 // require original GameObject dependencies and cannot silently become a null
 // target or generic finger pose.
 (Transform target,Vector3 offset) ResolveTarget(IKArmTarget target,IKArmType arm){
  Transform Anchor(WeaponBase weapon)=>arm==IKArmType.Right
    ?weapon?.rightHandAnchor:weapon?.leftHandAnchor;
  return target switch {
   IKArmTarget.CurrentWeapon=>(Anchor(self.ActiveWeapon??PreviousWeapon),Vector3.zero),
   IKArmTarget.PreviousWeapon=>(Anchor(PreviousWeapon),Vector3.zero),
   IKArmTarget.PendingWeapon=>(Anchor(PendingWeapon),Vector3.zero),
   IKArmTarget.GrenadeAnchor=>(self.stunGrenadeAnchor,Vector3.zero),
   IKArmTarget.MagazinePocket=>(self.chestRigIKTarget,Vector3.zero),
   IKArmTarget.PistolIK=>(self.pistolIKTarget,Vector3.zero),
   IKArmTarget.Head=>(self.limbs.Find(l=>l.id==LimbId.Head)?.transform,Vector3.zero),
   IKArmTarget.Pelvis=>(self.Pelvis?.transform,Vector3.zero),
   IKArmTarget.ThighHolster=>throw new NotSupportedException("Original character ThighHolster component not connected"),
   IKArmTarget.Wound=>throw new NotSupportedException("Original ApplyGauzePosition wound target not connected"),
   IKArmTarget.MedicPouch=>throw new NotSupportedException("Original TeamOperator.medicPouchTransform not connected"),
   _=>throw new ArgumentOutOfRangeException(nameof(target),target,null)
  };
 }
 // Original IKArmController.IK_* source method bodies (selectively compiled).
 void Invoke(IKControllerInvoke action){
  switch(action){
   case IKControllerInvoke.None:return;
   case IKControllerInvoke.LowerWeapon:
    self.IsLoweringWeapon=true;
    return;
   case IKControllerInvoke.FireWeapon:
    self.PullTrigger();
    return;
   case IKControllerInvoke.MovePendingWeaponToHand:
    if(PendingWeapon!=null){
     Transform grab=self.rightArm.hand?.grabPoint
       ??throw new InvalidOperationException("Original right hand grabPoint unavailable");
     PendingWeapon.transform.SetParent(grab);
     PendingWeapon.transform.localPosition=Vector3.zero;
    }
    return;
   default:
    throw new NotSupportedException("Original IKArmController callback dependencies unavailable: "+action);
  }
 }
}

public partial class CharacterBody {
 // Original IKArmController.IK_LowerWeapon sets this field. It is separate
 // from rifle reload/weapon manipulation state.
 public bool IsLoweringWeapon;
}
