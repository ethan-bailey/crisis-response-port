using System.IO.Compression;using System.Reflection;using System.Text.Json;using Crisis.Runtime;using Crisis.Runtime.Weapons;using UnityEngine;
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
  JsonElement StepOn(WeaponHost target,bool fire=false,bool reload=false){input.Tick(.016f);return JsonSerializer.SerializeToElement(target.Step(JsonSerializer.Serialize(new{direction=1,position=new[]{-3f,1f,1.5f},operatorTransforms=new{},weaponPose=new{p=new[]{-3f,1.5f,1.5f},q=new[]{0f,0f,0f,1f}},fire,reload})));}JsonElement Step(bool fire=false,bool reload=false)=>StepOn(host,fire,reload);
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
  var reloadAudio=new List<(string Name,float Time,float FrameTime)>();
  void CaptureReloadAudio(JsonElement frame){var frameTime=frame.GetProperty("sourceTime").GetSingle();foreach(var item in frame.GetProperty("events").EnumerateArray())if(item.GetProperty("type").GetString()=="audio")reloadAudio.Add((item.GetProperty("value").GetString()!,item.GetProperty("time").GetSingle(),frameTime));}
  state=Step(false,true);CaptureReloadAudio(state);for(int i=0;i<300;i++){state=Step();CaptureReloadAudio(state);}
  var reloadAudioOrder=new[]{"audioExtractMagazine","audioInsertMagazine","audioBoltReturn"};
  var reloadAudioIndices=reloadAudioOrder.Select(expected=>reloadAudio.FindIndex(item=>item.Name==expected)).ToArray();
  for(var index=0;index<reloadAudioOrder.Length;index++)if(reloadAudioIndices[index]<0)throw new Exception("Original reload did not emit its source audio event: "+reloadAudioOrder[index]+" (seen: "+string.Join(",",reloadAudio.Select(item=>item.Name))+")");
  if(!(reloadAudioIndices[0]<reloadAudioIndices[1]&&reloadAudioIndices[1]<reloadAudioIndices[2]))throw new Exception("Original reload audio events were out of source order: "+string.Join(" -> ",reloadAudio.Select(item=>item.Name)));
  if(reloadAudio.Any(item=>!float.IsFinite(item.Time)||!float.IsFinite(item.FrameTime)||item.Time>item.FrameTime+.0001f)||!reloadAudio.Select(item=>item.Time).SequenceEqual(reloadAudio.Select(item=>item.Time).OrderBy(time=>time)))throw new Exception("Original reload audio timestamps were invalid or nonmonotonic: "+string.Join(",",reloadAudio));
  if(state.GetProperty("reloading").GetBoolean()||!state.GetProperty("chamberLive").GetBoolean()||state.GetProperty("magazine").GetInt32()!=29||state.GetProperty("reserve").GetInt32()!=0)throw new Exception("Source reload/bolt/arm sequence failed or duplicated ammunition");
  int shots=state.GetProperty("shots").GetInt32();for(int i=0;i<20;i++)state=Step();if(state.GetProperty("shots").GetInt32()!=shots)throw new Exception("Weapon fires without trigger input");
  var quickHost=new WeaponHost(config,input.handler);var privateFields=BindingFlags.Instance|BindingFlags.NonPublic;var quickBody=(CharacterBody)typeof(WeaponHost).GetField("body",privateFields)!.GetValue(quickHost)!;var quickWeapon=(WeaponBase)typeof(WeaponHost).GetField("weapon",privateFields)!.GetValue(quickHost)!;var quickTask=(ChangeMagazine)typeof(WeaponHost).GetField("reload",privateFields)!.GetValue(quickHost)!;quickBody.Equipment.AddMagazineForWeapon(new Magazine(quickWeapon),quickWeapon);quickTask.quick=true;JsonElement quickState=default;for(int i=0;i<20;i++)quickState=StepOn(quickHost);
  for(int i=0;i<600;i++)quickState=StepOn(quickHost,true);
  if(quickState.GetProperty("shots").GetInt32()!=31||quickState.GetProperty("magazine").GetInt32()!=0)throw new Exception("Quick-reload fixture did not empty its original M4A1 magazine");
  var quickAudio=new List<string>();void CaptureQuick(JsonElement frame){foreach(var item in frame.GetProperty("events").EnumerateArray())if(item.GetProperty("type").GetString()=="audio")quickAudio.Add(item.GetProperty("value").GetString()!);}
  quickState=StepOn(quickHost,false,true);CaptureQuick(quickState);for(int i=0;i<300;i++){quickState=StepOn(quickHost);CaptureQuick(quickState);}
  string[] quickEvents={"audioExtractMagazine","audioInsertMagazine","audioBoltReturn"};
  foreach(var name in quickEvents)if(quickAudio.Count(x=>x==name)!=1)throw new Exception("Quick source reload expected exactly one "+name+" callback, got "+quickAudio.Count(x=>x==name));
  if(!(quickAudio.IndexOf(quickEvents[0])<quickAudio.IndexOf(quickEvents[1])&&quickAudio.IndexOf(quickEvents[1])<quickAudio.IndexOf(quickEvents[2])))throw new Exception("Quick source reload audio order invalid: "+string.Join(" -> ",quickAudio));
  if(quickState.GetProperty("reloading").GetBoolean()||quickState.GetProperty("manipulating").GetBoolean()||quickState.GetProperty("magazine").GetInt32()!=29||quickState.GetProperty("reserve").GetInt32()!=30||!quickState.GetProperty("chamberLive").GetBoolean()||quickState.GetProperty("shots").GetInt32()!=31)throw new Exception("Quick source reload did not settle to 30+1 with one full reserve");
  quickAudio.Clear();for(int i=0;i<600;i++)quickState=StepOn(quickHost,true);
  if(quickState.GetProperty("shots").GetInt32()<=31||quickState.GetProperty("magazine").GetInt32()!=0||quickState.GetProperty("bolt").GetString()!="Open")throw new Exception("Second source magazine did not reach the empty state");
  quickState=StepOn(quickHost,false,true);CaptureQuick(quickState);for(int i=0;i<300;i++){quickState=StepOn(quickHost);CaptureQuick(quickState);}
  if(quickState.GetProperty("magazine").GetInt32()!=29||quickState.GetProperty("reserve").GetInt32()!=0||quickState.GetProperty("reloading").GetBoolean()||quickState.GetProperty("manipulating").GetBoolean())throw new Exception("Second quick reload with final reserve did not settle correctly");
  if(quickAudio.Count(x=>x=="audioInsertMagazine")!=1||quickAudio.Count(x=>x=="audioExtractMagazine")!=1)throw new Exception("Second quick reload did not emit exactly one extract and insert event");
  if(!quickBody.Equipment.PrimaryMagazines.Remove(quickWeapon.Magazine!))throw new Exception("Could not remove active magazine from the test reserve list");if(quickBody.Equipment.HasAmmunitionForWeapon(quickWeapon))throw new Exception("No-reserve fixture still reports source ammunition");var noReserveShots=quickState.GetProperty("shots").GetInt32();var noReserveMagazineBefore=quickState.GetProperty("magazine").GetInt32();var noReserveChamberBefore=quickState.GetProperty("chamberLive").GetBoolean();var noReserveAmmoBefore=noReserveMagazineBefore+(noReserveChamberBefore?1:0);var noReserveBoltBefore=quickState.GetProperty("bolt").GetString();quickAudio.Clear();quickState=StepOn(quickHost,false,true);CaptureQuick(quickState);for(int i=0;i<120;i++){quickState=StepOn(quickHost);CaptureQuick(quickState);}
  var noReserveMagazineAfter=quickState.GetProperty("magazine").GetInt32();var noReserveChamberAfter=quickState.GetProperty("chamberLive").GetBoolean();var noReserveAmmoAfter=noReserveMagazineAfter+(noReserveChamberAfter?1:0);var noReserveReserveAfter=quickState.GetProperty("reserve").GetInt32();var noReserveShotsAfter=quickState.GetProperty("shots").GetInt32();var noReserveBoltAfter=quickState.GetProperty("bolt").GetString();var noReserveLoading=quickState.GetProperty("reloading").GetBoolean();var noReserveManipulating=quickState.GetProperty("manipulating").GetBoolean();if(noReserveMagazineAfter!=29||noReserveReserveAfter!=0||noReserveShotsAfter!=noReserveShots||noReserveAmmoAfter>noReserveAmmoBefore||noReserveLoading||noReserveManipulating)throw new Exception("No-reserve retry snapshot before={magazine:"+noReserveMagazineBefore+", chamberLive:"+noReserveChamberBefore+", ammo:"+noReserveAmmoBefore+", bolt:"+noReserveBoltBefore+", shots:"+noReserveShots+"} after={magazine:"+noReserveMagazineAfter+", reserve:"+noReserveReserveAfter+", chamberLive:"+noReserveChamberAfter+", ammo:"+noReserveAmmoAfter+", bolt:"+noReserveBoltAfter+", shots:"+noReserveShotsAfter+", reloading:"+noReserveLoading+", manipulating:"+noReserveManipulating+"} events="+string.Join(" -> ",quickAudio));
  if(quickAudio.Any(x=>x=="audioInsertMagazine"))throw new Exception("Genuinely empty-reserve retry emitted an insert callback: "+string.Join(" -> ",quickAudio));
  return JsonSerializer.Serialize(new{weapons="PASS",initialRounds=31,emptyBoltLocks=true,originalReloadTasks=true,reloadedRounds=30,noInfiniteAmmo=true,localeIndependentTangents=true,originalIKUpdate=true,originalHandVisualActivation=true,quickReload=true,repeatReloadWithoutReserve=true,noReserveRetry=new{ammoBefore=noReserveAmmoBefore,ammoAfter=noReserveAmmoAfter,magazineBefore=noReserveMagazineBefore,magazine=noReserveMagazineAfter,reserve=noReserveReserveAfter,chamberLiveBefore=noReserveChamberBefore,chamberLive=noReserveChamberAfter,boltBefore=noReserveBoltBefore,bolt=noReserveBoltAfter,shotsBefore=noReserveShots,shots=noReserveShotsAfter,reloading=noReserveLoading,manipulating=noReserveManipulating,events=quickAudio.ToArray()}});

 }
}

