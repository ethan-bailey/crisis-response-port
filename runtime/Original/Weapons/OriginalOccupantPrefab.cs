using System.Text.Json;
using UnityEngine;
using Crisis.Runtime.Character;

namespace Crisis.Runtime.Weapons;

// Instantiates the distinct recovered Occupant prefab rigidbody/joint/limb
// graph without replacing it with a box or synthetic target. A mission actor
// spawn is deliberately separate: this is the original source prefab root,
// not an invented in-level NPC or an AI/behavior implementation.
public sealed class OriginalOccupantPrefab {
 public CharacterBody Character {get;}
 public OriginalArmPhysicsHost PhysicsHost {get;}
 public OriginalOccupantPrefab(JsonElement physics,JsonElement sourceLimbs,RuntimeWorld world) {
  if(world==null)throw new ArgumentNullException(nameof(world));
  world.MakeCurrent();
  if(physics.GetProperty("format").GetString()!="crisis-original-arm-physics-runtime-v1"||
     sourceLimbs.GetProperty("format").GetString()!="crisis-original-character-limb-components-v1"||
     physics.GetProperty("archiveSha256").GetString()!="cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d")
   throw new InvalidDataException("Unverified original Occupant prefab provenance");
  var prefab=physics.GetProperty("prefabs").EnumerateArray()
   .Single(x=>x.GetProperty("name").GetString()=="Occupant");
  var records=prefab.GetProperty("records").EnumerateArray()
   .ToDictionary(x=>x.GetProperty("fileID").GetString(),x=>x);
  var root=records.Values.Where(x=>x.GetProperty("classID").GetInt32()==4)
   .Single(x=>x.GetProperty("fields").GetProperty("m_Father").GetProperty("fileID").GetString()=="0");
  var rootFields=root.GetProperty("fields");
  var gameId=rootFields.GetProperty("m_GameObject").GetProperty("fileID").GetString();
  var gameFields=records.Values.Single(x=>x.GetProperty("classID").GetInt32()==1&&
     x.GetProperty("fileID").GetString()==gameId).GetProperty("fields");
  var game=new GameObject(gameFields.GetProperty("m_Name").GetString(),world);
  var pose=rootFields.GetProperty("m_LocalPosition");
  game.transform.localPosition=new Vector3(pose.GetProperty("x").GetSingle(),
   pose.GetProperty("y").GetSingle(),pose.GetProperty("z").GetSingle());
  var scale=rootFields.GetProperty("m_LocalScale");
  game.transform.localScale=new Vector3(scale.GetProperty("x").GetSingle(),
   scale.GetProperty("y").GetSingle(),scale.GetProperty("z").GetSingle());
  var rot=rootFields.GetProperty("m_LocalRotation");
  game.transform.localRotation=new Quaternion(rot.GetProperty("x").GetSingle(),
    rot.GetProperty("y").GetSingle(),rot.GetProperty("z").GetSingle(),
    rot.GetProperty("w").GetSingle());
  Character=game.AddComponent<CharacterBody>();
  Character.IsPlayer=false;
  Character.InitializeOriginalPhysiology(Time.time);
  var nodes=new Dictionary<string,Transform>{{root.GetProperty("fileID").GetString(),game.transform}};
  PhysicsHost=new OriginalArmPhysicsHost(physics,nodes,Character,world,sourceLimbs,"Occupant");
  PhysicsHost.BindOriginalPhysiology();
  if(Character.limbs.Count!=(int)LimbId.NumberOfLimbs||
     Character.limbs.Count(l=>l.SourceMuscle!=null)!=16||
     Character.limbs.Count(l=>l.LimbPhysiology!=null)!=17||
     Character.limbs.Count(l=>l.SourceVitals!=null)!=9||
     PhysicsHost.SourceActorCount!=20||PhysicsHost.SourceJointCount!=18||
     PhysicsHost.SourceArmControlBound)
   throw new InvalidDataException("Recovered Occupant bone, nerve, organ or physics graph incomplete");
 }
 // The same source material, armor and 19 capsule bindings as Operator.
 // Query colliders stay disabled until actual mission/effects/skin data exists.
 public void BindSourceBallistics(JsonElement source,BallisticRuntimeHost materials,RuntimeWorld world)
  =>PhysicsHost.BindSourceBallistics(source,materials,world);
}
