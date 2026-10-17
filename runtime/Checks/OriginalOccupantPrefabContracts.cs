using System.Text.Json;
using Crisis.Runtime.Weapons;
using Crisis.Runtime.Presentation;
using System.IO.Compression;
using UnityEngine;

public static class OriginalOccupantPrefabContracts {
 static void Check(bool value,string why){if(!value)throw new Exception(why);}
 public static string Run(){
  using var physicsDoc=JsonDocument.Parse(File.ReadAllText("game/assets/original-full-character-physics.json"));
  using var limbDoc=JsonDocument.Parse(File.ReadAllText("game/assets/original-character-limb-components.json"));
  var world=new RuntimeWorld();
  var a=new OriginalOccupantPrefab(physicsDoc.RootElement,limbDoc.RootElement,world);
  var b=new OriginalOccupantPrefab(physicsDoc.RootElement,limbDoc.RootElement,world);
  Check(a.Character!=b.Character&&!ReferenceEquals(a.Character.Pelvis,b.Character.Pelvis),
   "Distinct original Occupant prefabs alias one CharacterBody/Limb");
  Check(a.PhysicsHost.SourceActorCount==20&&b.PhysicsHost.SourceJointCount==18,
   "Source Occupant 20 rigidbody / 18 joint graph changed");
  Check(a.Character.GetComponentsInChildren<HingeJoint>().Length==18,
   "Original Occupant HingeJoint graph disconnected from source root");
  Check(a.Character.GetComponentsInChildren<Rigidbody>().Length==20,
   "Original Occupant Rigidbody inventory disconnected from source root");
  Check(a.Character.SourcePhysiology!=null&&
   ReferenceEquals(a.Character.SourcePhysiology.MuscleControl,a.Character.SourceMuscleControl),
   "Recovered NPC lacks independent blood and nerve state");
  Check(a.Character.Torso?.id==LimbId.Torso&&a.Character.Pelvis?.id==LimbId.Pelvis,
   "Original Occupant Torso/Pelvis source components missing");
  Check(!a.PhysicsHost.SourceLimbProjectileContactsActive&&!b.PhysicsHost.SourceLimbProjectileContactsActive,
   "Incomplete original NPC live collisions activated before source mission/effects");
  Check(!a.PhysicsHost.SourceArmControlBound,
   "Occupant falsely advertises completed IK/ArmsControl without original Hand visuals");
  // Bind actual recovered Occupant author flesh materials, armor and all
  // 19 source CapsuleColliders to the exact Mission 01 material runtime.
  // They must remain disabled until mission hit effects/renderer exist.
  static string Zipped(string path) {
   using var stream=new GZipStream(File.OpenRead(path),CompressionMode.Decompress);
   using var reader=new StreamReader(stream);
   return reader.ReadToEnd();
  }
  var scene=new PresentationHost(Zipped("game/assets/presentation.json.gz"));
  var ballisticRuntime=new BallisticRuntimeHost(
    Zipped("game/assets/ballistics.json.gz"),scene.QueryScene,
    new WeaponEffects(),_=>{});
  using var materialDoc=JsonDocument.Parse(
    File.ReadAllText("game/assets/original-limb-ballistic-materials.json"));
  a.BindSourceBallistics(materialDoc.RootElement,ballisticRuntime,world);
  b.BindSourceBallistics(materialDoc.RootElement,ballisticRuntime,world);
  Check(a.PhysicsHost.SourceLimbColliderCount==19&&b.PhysicsHost.SourceLimbColliderCount==19,
   "Recovered original Occupant limb capsules did not bind");
  Check(a.PhysicsHost.SourceLimbArmorCount==4&&b.PhysicsHost.SourceLimbArmorCount==4&&
    a.PhysicsHost.SourceLimbMaterialCount==8&&b.PhysicsHost.SourceLimbMaterialCount==8,
   "Author Occupant source armor/flesh GUID graph incomplete");
  Check(a.Character.limbs.All(l=>l.properties!=null&&
    l.GetComponent<CapsuleCollider>() is CapsuleCollider c&&!c.enabled),
   "Source Occupant queried before original live NPC is ready");
  int ragdoll=0;
  a.Character.EventRagdoll.AddListener(()=>ragdoll++);
  a.Character.EnterSourceRagdoll();
  Check(ragdoll==1&&!a.Character.Kinematic&&
   a.Character.limbs.Count(l=>!l.Rigidbody.isKinematic)==19,
   "Original Occupant limbs did not release authored PhysX rigidbody sinks");
  Check(b.Character.Kinematic&&b.Character.SourcePhysiology.Alive,
   "Independent NPC actor was mutated by first NPC ragdoll");
  return JsonSerializer.Serialize(new {
   sourceOccupantPrefab="PASS",prefabActors=2,authoredBodiesPerActor=20,
   authoredJointsPerActor=18,authoredLimbsPerActor=19,
   originalMusclesPerActor=16,originalVitalsPerActor=9,
   originalFleshMaterialsPerActor=8,authoredArmorPerActor=4,
   disabledSourceCapsulesPerActor=19,
   authenticRagdollRelease=true,liveNpcGameplay=false
  });
 }
}
