using System.Text.Json;
using Crisis.Runtime.Character;
using UnityEngine;

namespace Crisis.Runtime.Weapons {
 // A source-type name bridge while the complete original Grenade class is
 // outside the compiled gameplay subset. No substitute grenade logic.
 public static class GrenadeReference {
  public enum Type { Frag = 0 }
 }
 // Runs the unchanged original ArmsControl and ArmController method bodies.
 // All physical commands are transported to the pinned browser PhysX kernel.
 public sealed class OriginalArmPhysicsHost {
  readonly ArmsControl controller;
  readonly CharacterBody body;
  readonly Dictionary<string,Rigidbody> bodies=new();
  readonly Dictionary<string,HingeJoint> joints=new();
  readonly Dictionary<string,string> componentToBody=new();
  readonly List<object> commands=new();
  readonly JsonElement configuration;
  readonly Dictionary<LimbId,JsonElement> authoredLimbs=new();
  readonly Dictionary<LimbId,Limb> runtimeLimbs=new();
  public int SourceActorCount=>bodies.Count;
  public int SourceJointCount=>joints.Count;
  readonly Dictionary<string,Transform> transforms;
  static Vector3 Vector(JsonElement v)=>new(v.GetProperty("x").GetSingle(),
   v.GetProperty("y").GetSingle(),v.GetProperty("z").GetSingle());
  static float[] Arr(Vector3 v)=>[v.x,v.y,v.z];
  public OriginalArmPhysicsHost(JsonElement source,Dictionary<string,Transform> nodes,
       CharacterBody subject,RuntimeWorld world,JsonElement sourceLimbConfiguration=default){
   if(source.GetProperty("format").GetString()!="crisis-original-arm-physics-runtime-v1")
    throw new InvalidDataException("Original physical arm source version mismatch");
   configuration=source.GetProperty("prefabs").EnumerateArray()
     .First(p=>p.GetProperty("name").GetString()=="Operator").Clone();
   transforms=nodes;body=subject;
   var records=configuration.GetProperty("records").EnumerateArray()
    .ToDictionary(r=>r.GetProperty("fileID").GetString(),r=>r.Clone());
   var transformByGo=records.Values.Where(r=>r.GetProperty("classID").GetInt32()==4)
    .ToDictionary(r=>r.GetProperty("fields").GetProperty("m_GameObject").GetProperty("fileID").GetString(),
                  r=>r.GetProperty("fileID").GetString());
   Transform NodeForBody(string id){
    var record=records[id];
    var go=record.GetProperty("fields").GetProperty("m_GameObject").GetProperty("fileID").GetString();
    return nodes[transformByGo[go]];
   }
   // Imported full original Operator actor graph: 21 serialized rigidbodies
   // (the root is handled by browser contact-physics) and 18 source joints.
   // Preserve exact Unity fileID strings; no guessed body or hinge mass.
   foreach(var record in records.Values.Where(r=>r.GetProperty("classID").GetInt32()==54)){
    string id=record.GetProperty("fileID").GetString();
    var f=record.GetProperty("fields");
    var node=NodeForBody(id);
    var rb=node.GetComponent<Rigidbody>()??node.gameObject.AddComponent<Rigidbody>();
    rb.isKinematic=f.GetProperty("m_IsKinematic").GetInt32()!=0;
    rb.mass=f.GetProperty("m_Mass").GetSingle();
    rb.drag=f.GetProperty("m_Drag").GetSingle();
    rb.angularDrag=f.GetProperty("m_AngularDrag").GetSingle();
    rb.useGravity=f.GetProperty("m_UseGravity").GetInt32()!=0;
    string sourceId=id;
    rb.KinematicChanged=state=>commands.Add(new{type="kinematic",body=sourceId,value=state});
    rb.GravityChanged=state=>commands.Add(new{type="gravity",body=sourceId,value=state});
    rb.DragChanged=value=>commands.Add(new{type="drag",body=sourceId,value});
    rb.PhysicsForceSink=(force,point,mode)=>commands.Add(new{
      type="force",body=sourceId,force=Arr(force),position=Arr(point),mode=mode.ToString()});
    bodies[id]=rb;
   }
   foreach(var record in records.Values.Where(r=>r.GetProperty("classID").GetInt32()==59)){
    var j=record.GetProperty("fields");
    var go=j.GetProperty("m_GameObject").GetProperty("fileID").GetString();
    if(!transformByGo.TryGetValue(go,out var transformId))
       throw new InvalidDataException("Original HingeJoint Transform missing: "+go);
    var jointNode=nodes[transformId];
    var hinge=jointNode.GetComponent<HingeJoint>()??jointNode.gameObject.AddComponent<HingeJoint>();
    var spring=j.GetProperty("m_Spring");
    hinge.spring=new JointSpring{spring=spring.GetProperty("spring").GetSingle(),
      damper=spring.GetProperty("damper").GetSingle(),
      targetPosition=spring.GetProperty("targetPosition").GetSingle()};
    hinge.useSpring=j.GetProperty("m_UseSpring").GetInt32()!=0;
    var jointId=record.GetProperty("fileID").GetString();
    string sourceJoint=jointId;
    hinge.SpringChanged=(state,enabled)=>commands.Add(new{
      type="spring",joint=sourceJoint,enabled,
      spring=state.spring,damper=state.damper,targetPosition=state.targetPosition});
    joints[jointId]=hinge;
   }
   if(bodies.Count!=21||joints.Count!=18)
    throw new InvalidDataException($"Unexpected original Operator physics graph: {bodies.Count} bodies, {joints.Count} hinges");
   // Bind original MonoBehaviour Limb IDs, not a fabricated anatomical table.
   // Original serialized right/left hands have no LimbPhysiology component.
   if(sourceLimbConfiguration.ValueKind==JsonValueKind.Object){
    if(sourceLimbConfiguration.GetProperty("format").GetString()!="crisis-original-character-limb-components-v1")
      throw new InvalidDataException("Original limb source manifest format mismatch");
    var authored=sourceLimbConfiguration.GetProperty("characters").EnumerateArray()
      .Single(c=>c.GetProperty("name").GetString()=="Operator");
    if(authored.GetProperty("prefabSha256").GetString()!=configuration.GetProperty("sha256").GetString())
      throw new InvalidDataException("Source Limb and Rigidbody serialized prefabs disagree");
    foreach(var entry in authored.GetProperty("limbs").EnumerateArray()){
      var id=(LimbId)entry.GetProperty("sourceFields").GetProperty("Limb").GetProperty("id").GetInt32();
      if(id<0||id>=LimbId.NumberOfLimbs||authoredLimbs.ContainsKey(id))
       throw new InvalidDataException("Invalid or duplicated original LimbId: "+id);
      var sourceTransform=entry.GetProperty("transform").GetString();
      if(!nodes.TryGetValue(sourceTransform,out var node))
       throw new InvalidDataException("Unresolved original limb Transform: "+sourceTransform);
      var limb=node.gameObject.GetComponent<Limb>()??node.gameObject.AddComponent<Limb>();
      limb.Body=subject;limb.id=id;
      authoredLimbs[id]=entry.Clone();runtimeLimbs[id]=limb;
      subject.limbs.Add(limb);
    }
    if(authoredLimbs.Count!=(int)LimbId.NumberOfLimbs)
     throw new InvalidDataException("Missing original source limbs");
   }
   controller=subject.gameObject.GetComponent<ArmsControl>()??
     subject.gameObject.AddComponent<ArmsControl>();
   ArmController Arm(string side){
    var arm=configuration.GetProperty("arms").GetProperty(side);
    var source=arm.GetProperty("controller");
    var limbs=arm.GetProperty("limbs").EnumerateArray().ToArray();
    var handNode=nodes[limbs[2].GetProperty("transform").GetString()];
    var hand=handNode.GetComponent<global::Hand>();
    if(hand==null)throw new InvalidOperationException("Original physical Hand missing");
    var result=new ArmController {
      mode=(ArmController.Mode)source.GetProperty("mode").GetInt32(),
      upperArm=nodes[limbs[0].GetProperty("transform").GetString()],
      lowerArm=nodes[limbs[1].GetProperty("transform").GetString()],
      hand=hand,isRightArm=side=="rightArm",
      minimumForce=source.GetProperty("minimumForce").GetSingle(),
      tolerance=source.GetProperty("tolerance").GetSingle(),
      targetPosition=Vector(source.GetProperty("targetPosition"))
    };
    return result;
   }
   controller.leftArm=Arm("leftArm");controller.rightArm=Arm("rightArm");
   // ArmsControl.Start is the unchanged recovered developer lifecycle method.
   world.Invoke(controller,"Start");
   commands.Clear();
  }

  // Invoke after the real CharacterBody has initialized original Physiology.
  // These settings are taken from the actual serialized Unity LimbPhysiology and
  // LimbMuscle MonoBehaviours, including source-absent hand components.
  public void BindOriginalPhysiology(){
   var p=body.SourcePhysiology??throw new InvalidOperationException("CharacterBody physiology is not initialized");
   foreach(var pair in authoredLimbs){
    var id=pair.Key;var source=pair.Value.GetProperty("sourceFields");
    var limb=runtimeLimbs[id];
    if(source.TryGetProperty("LimbPhysiology",out var physiology)){
     var state=new OriginalLimbPhysiology(p){
      bloodPressureModifier=physiology.GetProperty("bloodPressureModifier").GetSingle(),
      shockModifier=physiology.GetProperty("shockModifier").GetSingle(),
      maxHemorrhaging=physiology.GetProperty("maxHemorrhaging").GetSingle(),
      maxWoundCavityVolume=physiology.GetProperty("maxWoundCavityVolume").GetSingle()
     };
     limb.LimbPhysiology=state;
    }
    if(source.TryGetProperty("LimbMuscle",out var muscle)){
     var hinge=limb.GetComponent<HingeJoint>()
       ??throw new InvalidDataException("Original LimbMuscle missing its source HingeJoint: "+id);
     var rb=limb.Rigidbody;
     limb.SourceMuscle=new OriginalLimbMuscle(hinge,rb,p,
       ()=>body.OnOriginalJointBroken(limb),
       authoredBoneIntegrity:muscle.GetProperty("boneIntegrity").GetSingle(),
       authoredStrength:muscle.GetProperty("strength").GetSingle(),
       authoredExtendAngle:muscle.GetProperty("extendAngle").GetSingle(),
       authoredContractAngle:muscle.GetProperty("contractAngle").GetSingle());
    }
   }
  }
  public void Step(JsonElement frame,RuntimeWorld world){
   if(frame.TryGetProperty("armAngles",out var angles))
    foreach(var entry in angles.EnumerateObject())
     if(joints.TryGetValue(entry.Name,out var hinge))
      hinge.angle=entry.Value.GetSingle();
   // Original normal state has kinematic character arms. Original dynamic
   // transitions may change this via the real CharacterBody lifecycle.
   world.Invoke(controller,"Update");
  }
  public object Snapshot(){
   var current=commands.ToArray();commands.Clear();
   return new {sourceControllerExecuted=true,sourceArmsKinematic=body.Kinematic,
     commands=current};
  }
 }
}
