using System.IO.Compression;using System.Text.Json;using Crisis.Runtime;using Crisis.Runtime.Weapons;using UnityEngine;
public static class WeaponContracts {
 public static string Run(string path){
  using var stream=new GZipStream(File.OpenRead(path),CompressionMode.Decompress);string config=new StreamReader(stream).ReadToEnd();
  config=ComposeSourceConfiguration(config,Path.GetDirectoryName(path));
  var input=new OriginalInputHost(new RuntimeWorld());input.Tick(.1f);var before=Time.time;
  var previousCulture=System.Globalization.CultureInfo.CurrentCulture;
  var localized=(System.Globalization.CultureInfo)System.Globalization.CultureInfo.InvariantCulture.Clone();localized.NumberFormat.PositiveInfinitySymbol="∞";localized.NumberFormat.NegativeInfinitySymbol="-∞";
  WeaponHost host;try{System.Globalization.CultureInfo.CurrentCulture=localized;host=new WeaponHost(config,input.handler);}finally{System.Globalization.CultureInfo.CurrentCulture=previousCulture;}
  var npc=(JsonElement)JsonSerializer.SerializeToElement(host.SpawnSourceOccupant(JsonSerializer.Serialize(new{
   bodyPrefab="Occupant",position=new[]{5f,1.25f,-1.5f}
  })));
  if(npc.GetProperty("sourceActorCount").GetInt32()!=20||
     npc.GetProperty("limbs").GetInt32()!=(int)LimbId.NumberOfLimbs||
     npc.GetProperty("originalProjectileContactsActive").GetBoolean()||
     npc.GetProperty("skinChoices").EnumerateObject().Count()!=16||
     host.SourceOccupantCount!=1)
   throw new Exception("Original live WeaponHost Occupant source graph spawn mismatch");
  var sourceSkillActor=JsonSerializer.SerializeToElement(host.SpawnSourceOccupant(
   JsonSerializer.Serialize(new{bodyPrefab="Occupant",position=new[]{6f,1.25f,-1.5f},sourceWeaponSkill=.65f})));
  if(sourceSkillActor.GetProperty("index").GetInt32()!=1 || host.SourceOccupantCount!=2 ||
     MathF.Abs(sourceSkillActor.GetProperty("sourceWeaponSkill").GetSingle()-.65f)>1e-6f)
   throw new Exception("Source Occupant native weapon-skill actor failed registration");
  var stateActor=JsonSerializer.SerializeToElement(host.SpawnSourceOccupant(
   JsonSerializer.Serialize(new{bodyPrefab="Occupant",position=new[]{6.5f,1.25f,-1.5f},
    morale=.5f,aggression=.8f,fear=.2f,blindFireDistance=4f,willBlindFire=true,negotiable=false})));
  if(stateActor.GetProperty("index").GetInt32()!=2||host.SourceOccupantCount!=3||
     MathF.Abs(stateActor.GetProperty("sourceBlackboard").GetProperty("morale").GetSingle()-.5f)>1e-6f||
     !stateActor.GetProperty("sourceBlackboard").GetProperty("willBlindFire").GetBoolean())
   throw new Exception("Original Occupant native blackboard registration mismatch");
  var physiologyActor=JsonSerializer.SerializeToElement(host.SpawnSourceOccupant(
   JsonSerializer.Serialize(new{bodyPrefab="Occupant",position=new[]{6.75f,1.25f,-1.5f},
    sourcePhysiology=new{fitness=.7f,shockResistance=.25f,shockRecovery=.55f}})));
  var livePhysiology=physiologyActor.GetProperty("sourcePhysiology");
  if(host.SourceOccupantCount!=4||
     MathF.Abs(livePhysiology.GetProperty("fitness").GetSingle()-.7f)>1e-6f||
     MathF.Abs(livePhysiology.GetProperty("shockResistance").GetSingle()-.25f)>1e-6f||
     MathF.Abs(livePhysiology.GetProperty("shockRecovery").GetSingle()-.55f)>1e-6f)
   throw new Exception("Original Occupant physiology native state not applied");
  int beforeInvalidPhysiology=host.SourceOccupantCount;
  try{host.SpawnSourceOccupant(JsonSerializer.Serialize(new{bodyPrefab="Occupant",
   position=new[]{7f,1.25f,-1.5f},sourcePhysiology=new{fitness=.7f}}));
   throw new Exception("Accepted incomplete original Occupant physiology");}
  catch(InvalidDataException){}
  if(host.SourceOccupantCount!=beforeInvalidPhysiology)
   throw new Exception("Invalid physiology allocated native rigidbodies");
  try{host.SpawnSourceOccupant(JsonSerializer.Serialize(new{bodyPrefab="Occupant",
   position=new[]{7f,1.25f,-1.5f},sourcePhysiology=new{fitness=2f,shockResistance=.2f,shockRecovery=.5f}}));
   throw new Exception("Accepted out-of-range original Occupant fitness");}
  catch(InvalidDataException){}
  if(host.SourceOccupantCount!=beforeInvalidPhysiology)
   throw new Exception("Out-of-range physiology allocated native actor");
  int beforeInvalidBlackboard=host.SourceOccupantCount;
  try{host.SpawnSourceOccupant(JsonSerializer.Serialize(new{bodyPrefab="Occupant",
   position=new[]{7f,1.25f,-1.5f},morale=.5f}));
   throw new Exception("Accepted partial original Occupant blackboard");}
  catch(InvalidDataException){}
  if(host.SourceOccupantCount!=beforeInvalidBlackboard)
   throw new Exception("Partial original Occupant blackboard allocated a physics actor");
  int beforeInvalidSkill=host.SourceOccupantCount;
  try{host.SpawnSourceOccupant(JsonSerializer.Serialize(new{bodyPrefab="Occupant",
    position=new[]{7f,1.25f,-1.5f},sourceWeaponSkill=2f}));
    throw new Exception("Accepted invalid source Occupant weapon skill");}
  catch(InvalidDataException){}
  if(host.SourceOccupantCount!=beforeInvalidSkill)
   throw new Exception("Invalid source weapon skill allocated a native actor");
  try{host.SpawnSourceOccupant(JsonSerializer.Serialize(new{bodyPrefab="Unverified",
      position=new[]{0f,1f,-1.5f}}));throw new Exception("Accepted unauthenticated actor prefab");}
  catch(NotSupportedException){}
  if(Time.time!=before)throw new Exception("Weapon import reset clock");
  JsonElement Step(bool fire=false,bool reload=false){input.Tick(.016f);return JsonSerializer.SerializeToElement(host.Step(JsonSerializer.Serialize(new{direction=1,position=new[]{-3f,1f,1.5f},operatorTransforms=new{},weaponPose=new{p=new[]{-3f,1.5f,1.5f},q=new[]{0f,0f,0f,1f}},fire,reload})));}
  JsonElement state=default;for(int i=0;i<20;i++)state=Step();
  var bindings=JsonDocument.Parse(config).RootElement.GetProperty("armRigBindings");
  foreach(var side in new[]{"right","left"}){
   var arm=state.GetProperty(side);var rig=bindings.GetProperty(side);
   if(!arm.GetProperty("OnTarget").GetBoolean())throw new Exception("Original IK transition did not finish");
   var expected=side=="right"?0:2;if(arm.GetProperty("handMode").GetInt32()!=expected)throw new Exception("Original hand mode not executed");
   foreach(var visual in arm.GetProperty("handObjects").EnumerateArray()){
    var id=visual.GetProperty("id").GetString();bool selected=false;
    foreach(var v in rig.GetProperty("variants").EnumerateObject())if(v.Value.GetProperty("gameObject").GetString()==id){
     var mode=v.Name=="fist"?1:v.Name=="trigger"?0:2;if(mode==expected)selected=true;
    }
    if(visual.GetProperty("active").GetBoolean()!=selected)throw new Exception("Original Hand.SetMode did not activate authored visual");
   }
   foreach(var bone in arm.GetProperty("bones").EnumerateObject())foreach(var value in bone.Value.GetProperty("q").EnumerateArray())if(!float.IsFinite(value.GetSingle()))throw new Exception("Original IK produced nonfinite pose");
  }
  if(!state.GetProperty("chamberLive").GetBoolean()||state.GetProperty("magazine").GetInt32()!=30)throw new Exception("Original source starts with chamber plus full magazine");
  for(int i=0;i<600;i++)state=Step(true);
  if(state.GetProperty("shots").GetInt32()!=31||state.GetProperty("bolt").GetString()!="Open")throw new Exception("Original bolt must lock open after 30+1, no infinite ammunition");
  state=Step(false,true);for(int i=0;i<300;i++)state=Step();
  if(state.GetProperty("reloading").GetBoolean()||!state.GetProperty("chamberLive").GetBoolean()||state.GetProperty("magazine").GetInt32()!=29||state.GetProperty("reserve").GetInt32()!=0)throw new Exception("Source reload/bolt/arm sequence failed or duplicated ammunition");
  int shots=state.GetProperty("shots").GetInt32();for(int i=0;i<20;i++)state=Step();if(state.GetProperty("shots").GetInt32()!=shots)throw new Exception("Weapon fires without trigger input");
  return JsonSerializer.Serialize(new{weapons="PASS",initialRounds=31,emptyBoltLocks=true,originalReloadTasks=true,reloadedRounds=30,noInfiniteAmmo=true,localeIndependentTangents=true,originalIKUpdate=true,originalHandVisualActivation=true});
 }
 public static string ComposeSourceConfiguration(string config,string assets){
  // Mirror browser composition of preserved separate source assets.
  var composed=System.Text.Json.Nodes.JsonNode.Parse(config);
  composed["armPhysics"]=System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(assets,"original-full-character-physics.json")));
  composed["sourceLimbComponents"]=System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(assets,"original-character-limb-components.json")));
  composed["sourceOccupantVisual"]=System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(assets,"occupant/source-visual-spawn.json")));
  return composed.ToJsonString();
 }

}

