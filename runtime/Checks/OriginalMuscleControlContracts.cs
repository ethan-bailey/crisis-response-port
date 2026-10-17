using System.Text.Json;
using Crisis.Runtime.Character;
using Crisis.Runtime.Weapons;
using UnityEngine;

public static class OriginalMuscleControlContracts {
 static void Require(bool value,string reason) {
  if(!value)throw new Exception(reason);
 }
 public static string Run(){
  var world=new RuntimeWorld();
  world.MakeCurrent();
  var root=new GameObject("original nerve / joint fixture",world);
  var body=root.AddComponent<CharacterBody>();
  body.InitializeOriginalPhysiology(Time.time,4500f,0f,0f);
  var muscleControl=body.SourceMuscleControl;
  Require(muscleControl!=null&&ReferenceEquals(body.SourcePhysiology.MuscleControl,muscleControl),
   "Source MuscleControl not installed on original CharacterBody physiology");
  var bone=new GameObject("authored-joint signal fixture",world);
  bone.transform.SetParent(root.transform,false);
  var limb=bone.AddComponent<Limb>();
  limb.id=LimbId.RightUpperArm;
  limb.Body=body;
  body.limbs.Add(limb);
  var rigid=bone.AddComponent<Rigidbody>();
  rigid.isKinematic=false;
  var hinge=bone.AddComponent<HingeJoint>();
  hinge.spring=new JointSpring{spring=80f,damper=10f,targetPosition=0f};
  int breaks=0;
  limb.SourceMuscle=new OriginalLimbMuscle(hinge,rigid,body.SourcePhysiology,
   ()=>breaks++,authoredBoneIntegrity:500f,authoredStrength:100f,
   authoredExtendAngle:42f,authoredContractAngle:-37f);
  muscleControl.SingleImpulse(.5f,.2f);
  Require(muscleControl.SpasmsActive,"Original single spasm not started");
  muscleControl.Update();
  limb.SourceMuscle.Update(Time.time);
  Require(hinge.useSpring&&hinge.spring.spring>0f&&Math.Abs(hinge.spring.targetPosition-42f)<.00001f,
   "Original MuscleControl signal did not reach authored HingeJoint spring");
  var clock=new RuntimeWorld(false,false);
  clock.Tick(.25f);world.MakeCurrent();
  muscleControl.Update();
  Require(!muscleControl.SpasmsActive&&!hinge.useSpring,
   "Original single-spasm expiry did not relax the real joint");
  muscleControl.ActivateCramp(.1f);
  Require(muscleControl.CrampsActive,"Original cramp not started");
  muscleControl.Update();
  clock.Tick(.2f);world.MakeCurrent();
  muscleControl.Update();
  Require(!muscleControl.CrampsActive,"Original cramp duration did not expire");
  body.SourcePhysiology.AddBrainTrauma(500f);
  Require(body.SourcePhysiology.BrainTrauma>=500f&&body.SourcePhysiology.RespirationDisabled
    &&muscleControl.SpasmsActive,
   "Original severe brain-nerve response not connected to MuscleControl");
  muscleControl.OnDeath();
  Require(!muscleControl.SpasmsActive&&!muscleControl.CrampsActive&&!hinge.useSpring,
   "Original MuscleControl.OnDeath failed to relax PhysX joint");
  Require(breaks==0,"Muscle response must not invent a bone fracture");
  return JsonSerializer.Serialize(new {
   sourceMuscleControl="PASS",sourceBranch="MuscleControl.cs",
   authoredJointSignals=true,brainTraumaNerves=true,
   crampAndSpasmTiming=true,deathRelaxation=true,liveNpcGameplay=false
  });
 }
}
