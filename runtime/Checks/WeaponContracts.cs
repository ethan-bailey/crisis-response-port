using System.IO.Compression;using System.Text.Json;using Crisis.Runtime;using Crisis.Runtime.Weapons;using UnityEngine;
public static class WeaponContracts {
 public static string Run(string path){
  using var stream=new GZipStream(File.OpenRead(path),CompressionMode.Decompress);var configRoot=System.Text.Json.Nodes.JsonNode.Parse(new StreamReader(stream).ReadToEnd())!.AsObject();configRoot["armPhysics"]=System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(path)!,"original-arm-physics.json")));string config=configRoot.ToJsonString();
  using var serializedPackage=JsonDocument.Parse(config);var packageRoot=serializedPackage.RootElement;
  var ammoPackage=packageRoot.GetProperty("ammoPackage");
  var resolvedCalibre=AmmunitionLoader.ResolveOriginalCalibreReference(ammoPackage,"57cfc851e0e47374d98db0383b39cddf","114724804207875695",new RuntimeWorld()) as Crisis.Runtime.FullSource.Calibre ?? throw new Exception("Original Calibre reference did not resolve to the source Calibre type");
  if(SerializedAssetIdentity.Guid(resolvedCalibre)!="57cfc851e0e47374d98db0383b39cddf"||SerializedAssetIdentity.FileID(resolvedCalibre)!="114724804207875695"||SerializedAssetIdentity.Path(resolvedCalibre)!="Assets/Resources/weapons/calibers/Caliber 5.56x45mm.prefab"||resolvedCalibre.calibreName!="5.56x45mm")throw new Exception("Recovered Calibre identity mismatch guid="+SerializedAssetIdentity.Guid(resolvedCalibre)+" fileID="+SerializedAssetIdentity.FileID(resolvedCalibre)+" path="+SerializedAssetIdentity.Path(resolvedCalibre)+" name="+resolvedCalibre.calibreName);
  Console.WriteLine("PASS: original Calibre GUID, fileID, type, path, serialized name, and prefab lifetime");
  var sourceAmmo=AmmunitionLoader.Load(packageRoot.GetProperty("ammoPackage").GetRawText(),packageRoot.GetProperty("ammoPath").GetString()!,new RuntimeWorld());
  var calibre=sourceAmmo.calibre??throw new Exception("5.56-FMJ original Calibre reference did not resolve");
  if(SerializedAssetIdentity.Guid(calibre)!="57cfc851e0e47374d98db0383b39cddf"||SerializedAssetIdentity.FileID(calibre)!="114724804207875695"||SerializedAssetIdentity.Path(calibre)!="Assets/Resources/weapons/calibers/Caliber 5.56x45mm.prefab"||calibre.calibreName!="5.56x45mm")throw new Exception("Recovered Calibre GUID/fileID/path/name identity mismatch");
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

