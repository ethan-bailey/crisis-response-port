using System.IO.Compression;using System.Text.Json;using Crisis.Runtime;using Crisis.Runtime.Weapons;using UnityEngine;
public static class WeaponContracts {
 public static string Run(string path){
  using var stream=new GZipStream(File.OpenRead(path),CompressionMode.Decompress);string config=new StreamReader(stream).ReadToEnd();
  var input=new OriginalInputHost(new RuntimeWorld());input.Tick(.1f);var before=Time.time;var host=new WeaponHost(config,input.handler);if(Time.time!=before)throw new Exception("Weapon import reset clock");
  JsonElement Step(bool fire=false,bool reload=false){input.Tick(.016f);return JsonSerializer.SerializeToElement(host.Step(JsonSerializer.Serialize(new{direction=1,position=new[]{-3f,1f,1.5f},operatorTransforms=new{},weaponPose=new{p=new[]{-3f,1.5f,1.5f},q=new[]{0f,0f,0f,1f}},fire,reload})));}
  JsonElement state=default;for(int i=0;i<20;i++)state=Step();
  if(!state.GetProperty("chamberLive").GetBoolean()||state.GetProperty("magazine").GetInt32()!=30)throw new Exception("Original source starts with chamber plus full magazine");
  for(int i=0;i<600;i++)state=Step(true);
  if(state.GetProperty("shots").GetInt32()!=31||state.GetProperty("bolt").GetString()!="Open")throw new Exception("Original bolt must lock open after 30+1, no infinite ammunition");
  state=Step(false,true);for(int i=0;i<300;i++)state=Step();
  if(state.GetProperty("reloading").GetBoolean()||!state.GetProperty("chamberLive").GetBoolean()||state.GetProperty("magazine").GetInt32()!=29||state.GetProperty("reserve").GetInt32()!=0)throw new Exception("Source reload/bolt/arm sequence failed or duplicated ammunition");
  int shots=state.GetProperty("shots").GetInt32();for(int i=0;i<20;i++)state=Step();if(state.GetProperty("shots").GetInt32()!=shots)throw new Exception("Weapon fires without trigger input");
  return JsonSerializer.Serialize(new{weapons="PASS",initialRounds=31,emptyBoltLocks=true,originalReloadTasks=true,reloadedRounds=30,noInfiniteAmmo=true});
 }
}
