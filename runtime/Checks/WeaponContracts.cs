using System.IO.Compression;using System.Text.Json;using Crisis.Runtime;using Crisis.Runtime.Weapons;using UnityEngine;
public static class WeaponContracts {
 public static string Run(string path){
  using var stream=new GZipStream(File.OpenRead(path),CompressionMode.Decompress);string config=new StreamReader(stream).ReadToEnd();
  var input=new OriginalInputHost(new RuntimeWorld());input.Tick(.1f);var before=Time.time;
  var previousCulture=System.Globalization.CultureInfo.CurrentCulture;
  var localized=(System.Globalization.CultureInfo)System.Globalization.CultureInfo.InvariantCulture.Clone();localized.NumberFormat.PositiveInfinitySymbol="∞";localized.NumberFormat.NegativeInfinitySymbol="-∞";
  WeaponHost host;try{System.Globalization.CultureInfo.CurrentCulture=localized;host=new WeaponHost(config,input.handler);}finally{System.Globalization.CultureInfo.CurrentCulture=previousCulture;}
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
}

