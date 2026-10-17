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
  readonly Dictionary<string,JsonElement> authoredCapsulesByGo=new();
  readonly Dictionary<LimbId,JsonElement> authoredLimbs=new();
  readonly Dictionary<LimbId,Limb> runtimeLimbs=new();
  readonly Dictionary<string,BallisticMaterialProperties> sourceBallisticProperties=new();
  readonly List<CapsuleCollider> authenticatedLimbColliders=new();
  public int SourceLimbColliderCount=>authenticatedLimbColliders.Count;
  public int SourceLimbMaterialCount=>sourceBallisticProperties.Count;
  public int SourceLimbArmorCount=>runtimeLimbs.Values.Count(l=>l.GetComponent<LimbArmor>()!=null);
  // Authoritative gameplay contacts remain gated until recovered original
  // Projectile.HandleLimbHit and Limb.HandleInterior are wired end-to-end.
  public bool SourceLimbProjectileContactsActive=>authenticatedLimbColliders.Any(c=>c.enabled);
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
   foreach(var cap in records.Values.Where(r=>r.GetProperty("classID").GetInt32()==136)){
    var fields=cap.GetProperty("fields");
    authoredCapsulesByGo.Add(fields.GetProperty("m_GameObject").GetProperty("fileID").GetString(),fields.Clone());
   }
   var transformByGo=records.Values.Where(r=>r.GetProperty("classID").GetInt32()==4)
    .ToDictionary(r=>r.GetProperty("fields").GetProperty("m_GameObject").GetProperty("fileID").GetString(),
                  r=>r.GetProperty("fileID").GetString());
   // Original weapon fixture previously imported only upper-body transforms.
   // Restore any missing real Operator transforms directly from the SAME
   // serialized Unity prefab rather than assigning synthetic body/leg poses.
   var missingTransforms=new HashSet<string>();
   foreach(var record in records.Values.Where(r=>r.GetProperty("classID").GetInt32()==4)){
    var id=record.GetProperty("fileID").GetString();
    if(nodes.ContainsKey(id))continue;
    var go=record.GetProperty("fields").GetProperty("m_GameObject").GetProperty("fileID").GetString();
    var goRecord=records[go];
    var name=goRecord.GetProperty("fields").GetProperty("m_Name").GetString();
    nodes[id]=new GameObject(name,world).transform;
    missingTransforms.Add(id);
   }
   foreach(var id in missingTransforms){
    var f=records[id].GetProperty("fields");
    var node=nodes[id];var parent=f.GetProperty("m_Father").GetProperty("fileID").GetString();
    if(nodes.TryGetValue(parent,out var parentNode))node.SetParent(parentNode,false);
    node.localPosition=Vector(f.GetProperty("m_LocalPosition"));
    node.localScale=Vector(f.GetProperty("m_LocalScale"));
    var q=f.GetProperty("m_LocalRotation");
    node.localRotation=new Quaternion(q.GetProperty("x").GetSingle(),
      q.GetProperty("y").GetSingle(),q.GetProperty("z").GetSingle(),
      q.GetProperty("w").GetSingle());
   }
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
    if(sourceLimbConfiguration.GetProperty("archiveSha256").GetString()!="cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d")
      throw new InvalidDataException("Original Limb components have an unverified source archive");
    if(authored.GetProperty("limbs").GetArrayLength()!=(int)LimbId.NumberOfLimbs)
      throw new InvalidDataException("Original full character limb inventory incomplete");
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

  // Attach original LimbTextureData green coverage to the actual rendered
  // developer-authored limb visual Transform. The exact source visual IDs and
  // texture SHA payloads are not replaceable with capsule silhouettes.
  // This does NOT yet enable Projectile.HandleLimbHit or dynamic blood masks.
  public void BindOriginalSkinCoverage(JsonElement sourceMasks){
   if(sourceMasks.GetProperty("format").GetString()!="crisis-original-limb-initial-damage-green-v1"
     ||sourceMasks.GetProperty("sourceManifestSha256").GetString()!="c93d90610a6db65edf4304bf82b590bc35dc56e5d7580899a511c10ef4c416f6")
    throw new InvalidDataException("Original limb skin source identity mismatch");
   if(sourceMasks.GetProperty("originalTextureCount").GetInt32()!=11||
      sourceMasks.GetProperty("originalRendererCount").GetInt32()!=17)
    throw new InvalidDataException("Incomplete source LimbRenderer skin inventory");
   var byVisual=authoredLimbs.ToDictionary(
     p=>p.Value.GetProperty("sourceFields").GetProperty("Limb")
       .GetProperty("visual").GetProperty("fileID").GetString(),
     p=>runtimeLimbs[p.Key]);
   var matched=new HashSet<string>();
   foreach(var skin in sourceMasks.GetProperty("skins").EnumerateArray()){
    var width=skin.GetProperty("width").GetInt32();
    var height=skin.GetProperty("height").GetInt32();
    var green=Convert.FromBase64String(
      skin.GetProperty("originalStaticDamageGreenBottomUpBase64").GetString());
    foreach(var renderer in skin.GetProperty("sourceRenderers").EnumerateArray()){
     var visualId=renderer.GetProperty("transformFileID").GetString();
     if(!matched.Add(visualId))
      throw new InvalidDataException("Duplicate source LimbRenderer visual");
     if(!byVisual.TryGetValue(visualId,out var limb)||
       !transforms.TryGetValue(visualId,out var originalVisual))
      throw new InvalidDataException("Original LimbRenderer visual not connected to live character: "+visualId);
     var sourceLimb=authoredLimbs[limb.id].GetProperty("sourceFields").GetProperty("Limb");
     if(sourceLimb.GetProperty("limbRenderer").GetProperty("fileID").GetString()=="0")
      throw new InvalidDataException("Cannot synthesize a missing source hand LimbRenderer");
     limb.SourceSkinTrace=new OriginalLimbSkinTrace(originalVisual,width,height,green);
    }
   }
   if(matched.Count!=17||body.limbs.Count(l=>l.SourceSkinTrace!=null)!=17)
    throw new InvalidDataException("Failed to bind all 17 actual original limb skin renderers");
  }
  // Recover the original serialized flesh resistance, authored layers,
  // CapsuleColliders and LimbArmor from their actual Unity prefab records.
  // These are real source objects, but do not include them in projectile queries
  // until original Limb.HandleProjectile is executable; otherwise a self-shot
  // would dispatch to an incomplete injury handler during ordinary gameplay.
  public void BindSourceBallistics(JsonElement source,BallisticRuntimeHost ballistics,RuntimeWorld world){
   if(ballistics==null||world==null)throw new ArgumentNullException(nameof(ballistics));
   if(source.GetProperty("format").GetString()!="crisis-original-limb-ballistic-materials-v1"||
      source.GetProperty("archiveSha256").GetString()!="cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d")
     throw new InvalidDataException("Unverified source limb ballistic materials");
   world.MakeCurrent();
   var records=configuration.GetProperty("records").EnumerateArray()
     .ToDictionary(r=>r.GetProperty("fileID").GetString(),r=>r.Clone());
   var games=records.Values.Where(r=>r.GetProperty("classID").GetInt32()==1)
     .ToDictionary(r=>r.GetProperty("fileID").GetString(),r=>r.GetProperty("fields").Clone());
   var materialData=source.GetProperty("materials");
   if(materialData.EnumerateObject().Count()!=8)
     throw new InvalidDataException("Original limb source requires eight distinct flesh material assets");
   var armorData=source.GetProperty("armor").GetProperty("Operator");
   if(armorData.EnumerateObject().Count()!=4)
     throw new InvalidDataException("Operator source must contain four armor components");
   string Guid(JsonElement value)
     =>value.TryGetProperty("guid",out var guid)?guid.GetString():null;
   static bool Flag(JsonElement value)
     =>value.ValueKind==JsonValueKind.True||value.ValueKind==JsonValueKind.Number&&value.GetInt32()!=0;
   static Vector2 Vec2(JsonElement v)
     =>new(v.GetProperty("x").GetSingle(),v.GetProperty("y").GetSingle());
   foreach(var pair in authoredLimbs){
    var limb=runtimeLimbs[pair.Key];
    var goid=pair.Value.GetProperty("gameObject").GetString();
    var sourceFields=pair.Value.GetProperty("sourceFields").GetProperty("Limb");
    if(!games.TryGetValue(goid,out var go))
     throw new InvalidDataException("Source limb GameObject is missing: "+goid);
    var layer=go.GetProperty("m_Layer").GetInt32();
    if(layer!=14&&layer!=24)
     throw new InvalidDataException("Invalid recovered limb collider layer "+layer);
    limb.gameObject.layer=layer;
    if(!authoredCapsulesByGo.TryGetValue(goid,out var fields))
     throw new InvalidDataException("Original limb CapsuleCollider missing: "+limb.id);
    var capsule=limb.GetComponent<CapsuleCollider>()??limb.gameObject.AddComponent<CapsuleCollider>();
    capsule.center=Vector(fields.GetProperty("m_Center"));
    capsule.radius=fields.GetProperty("m_Radius").GetSingle();
    capsule.height=fields.GetProperty("m_Height").GetSingle();
    capsule.direction=fields.GetProperty("m_Direction").GetInt32();
    capsule.isTrigger=Flag(fields.GetProperty("m_IsTrigger"));
    // No half-port gameplay: a projectile may not query limb colliders until
    // the source tissue/armor/skin interior and event graph is connected.
    capsule.enabled=false;
    authenticatedLimbColliders.Add(capsule);
    var guid=Guid(sourceFields.GetProperty("properties"));
    if(guid==null||!materialData.TryGetProperty(guid,out var entry))
     throw new InvalidDataException("Unknown original ballistic material GUID for "+limb.id);
    if(!sourceBallisticProperties.TryGetValue(guid,out var prop)){
     var fields2=entry.GetProperty("fields");
     prop=new GameObject("Original flesh material "+entry.GetProperty("assetPath").GetString(),world)
       .AddComponent<BallisticMaterialProperties>();
     prop.gameObject.assetObject=true;
     prop.surfaceThickness=fields2.GetProperty("surfaceThickness").GetSingle();
     prop.surfaceDensity=fields2.GetProperty("surfaceDensity").GetSingle();
     prop.maximumDeflection=fields2.GetProperty("maximumDeflection").GetSingle();
     prop.density=fields2.GetProperty("density").GetSingle();
     prop.densityRandomness=Vec2(fields2.GetProperty("densityRandomness"));
     prop.entryEffect=ballistics.SourceEntryOrExitEffect(Guid(fields2.GetProperty("entryEffect")));
     prop.exitEffect=ballistics.SourceEntryOrExitEffect(Guid(fields2.GetProperty("exitEffect")));
     prop.exitHole=ballistics.SourceExitHole(Guid(fields2.GetProperty("exitHole")));
     sourceBallisticProperties.Add(guid,prop);
    }
    limb.properties=prop;
    limb.width=sourceFields.GetProperty("width").GetSingle();
    limb.depth=sourceFields.GetProperty("depth").GetSingle();
    // Preserve each developer-authored aim target and exit accumulation factor
    // (including any source-omitted Unity field defaults). No generic aimpoint.
    if(sourceFields.TryGetProperty("radius",out var authoredRadius))
      limb.radius=authoredRadius.GetSingle();
    if(sourceFields.TryGetProperty("aimpoint",out var authoredAim))
      limb.aimpoint=Vec2(authoredAim);
    if(sourceFields.TryGetProperty("exitAccumulationFactor",out var authoredAccumulation))
      limb.exitAccumulationFactor=authoredAccumulation.GetSingle();
    limb.impenetrable=Flag(sourceFields.GetProperty("impenetrable"));
    limb.surfaceTestOnExit=Flag(sourceFields.GetProperty("surfaceTestOnExit"));
    limb.enabled=Flag(sourceFields.GetProperty("m_Enabled"));
    // The recovered Limb is a BallisticMaterial by inheritance, so these
    // lifecycle calls initialize its actual source density/pressure contracts.
    limb.Awake();
    limb.Start();
    if(armorData.TryGetProperty(goid,out var armorRow)){
     var a=armorRow.GetProperty("fields");
     var armor=limb.gameObject.AddComponent<LimbArmor>();
     armor.type=(ArmorClass.Type)a.GetProperty("type").GetInt32();
     armor.hasFrontArmor=Flag(a.GetProperty("hasFrontArmor"));
     armor.hasBackArmor=Flag(a.GetProperty("hasBackArmor"));
     armor.frontCoverage=Vec2(a.GetProperty("frontCoverage"));
     armor.backCoverage=Vec2(a.GetProperty("backCoverage"));
     armor.frontDamage=a.GetProperty("frontDamage").GetSingle();
     armor.backDamage=a.GetProperty("backDamage").GetSingle();
     armor.enabled=Flag(a.GetProperty("m_Enabled"));
     world.Invoke(armor,"Awake");
    }
   }
   if(authenticatedLimbColliders.Count!=(int)LimbId.NumberOfLimbs||
      sourceBallisticProperties.Count!=8||SourceLimbArmorCount!=4||
      SourceLimbProjectileContactsActive)
    throw new InvalidDataException("Original ballistic limb source graph incomplete or prematurely enabled");
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
    if(source.TryGetProperty("VitalFunctions",out var vital)){
     var go=pair.Value.GetProperty("gameObject").GetString();
     if(!authoredCapsulesByGo.TryGetValue(go,out var capsule))
       throw new InvalidDataException("Original vital function missing its source capsule: "+id);
     limb.SourceVitals=new OriginalVitalFunctions(limb,p,vital,
       capsule.GetProperty("m_Height").GetSingle(),
       capsule.GetProperty("m_Center").GetProperty("y").GetSingle());
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
