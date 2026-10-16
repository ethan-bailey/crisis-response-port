using System.Text.Json;
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
  readonly Dictionary<string,Transform> transforms;
  static Vector3 Vector(JsonElement v)=>new(v.GetProperty("x").GetSingle(),
   v.GetProperty("y").GetSingle(),v.GetProperty("z").GetSingle());
  static float[] Arr(Vector3 v)=>[v.x,v.y,v.z];
  public OriginalArmPhysicsHost(JsonElement source,Dictionary<string,Transform> nodes,
       CharacterBody subject,RuntimeWorld world){
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
   // Original serialized torso and twelve limb rigidbodies, not test-box actors.
   foreach(var arm in configuration.GetProperty("arms").EnumerateObject()){
    foreach(var limb in arm.Value.GetProperty("limbs").EnumerateArray()){
     foreach(var key in new[]{"connectedBody","rigidbody"}){
      var id=limb.GetProperty(key).GetString();
      if(bodies.ContainsKey(id))continue;
      var f=records[id].GetProperty("fields");
      var node=NodeForBody(id);
      var rb=node.GetComponent<Rigidbody>()??node.gameObject.AddComponent<Rigidbody>();
      rb.isKinematic=f.GetProperty("m_IsKinematic").GetInt32()!=0;
      string sourceId=id;
      rb.KinematicChanged=state=>commands.Add(new{type="kinematic",body=sourceId,value=state});
      rb.GravityChanged=state=>commands.Add(new{type="gravity",body=sourceId,value=state});
      rb.DragChanged=value=>commands.Add(new{type="drag",body=sourceId,value});
      rb.PhysicsForceSink=(force,point,mode)=>commands.Add(new {
       type="force",body=sourceId,force=Arr(force),position=Arr(point),mode=mode.ToString()});
      bodies[id]=rb;
     }
     var jointId=limb.GetProperty("joint").GetString();
     if(joints.ContainsKey(jointId))continue;
     var jointFields=records[jointId].GetProperty("fields");
     var jointNode=nodes[limb.GetProperty("transform").GetString()];
     var hinge=jointNode.GetComponent<HingeJoint>()??jointNode.gameObject.AddComponent<HingeJoint>();
     var spring=jointFields.GetProperty("m_Spring");
     hinge.spring=new JointSpring{spring=spring.GetProperty("spring").GetSingle(),
       damper=spring.GetProperty("damper").GetSingle(),targetPosition=spring.GetProperty("targetPosition").GetSingle()};
     hinge.useSpring=jointFields.GetProperty("m_UseSpring").GetInt32()!=0;
     string sourceJoint=jointId;
     hinge.SpringChanged=(state,enabled)=>commands.Add(new {
       type="spring",joint=sourceJoint,enabled,
       spring=state.spring,damper=state.damper,targetPosition=state.targetPosition});
     joints[jointId]=hinge;
    }
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
