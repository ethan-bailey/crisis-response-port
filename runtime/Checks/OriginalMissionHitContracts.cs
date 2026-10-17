using Crisis.Runtime.Weapons;
using System.Text.Json;
using UnityEngine;
public static class OriginalMissionHitContracts {
 static void Check(bool c,string message){if(!c)throw new Exception(message);}
 public static string Run(){
  var mission=new OriginalMissionInjuryState();
  Check(mission.FirstInjuryTimestamp==0f,"Recovered original injury initial zero");
  mission.FirstInjuryTimestamp=0f;
  mission.FirstInjuryTimestamp=12f;mission.FirstInjuryTimestamp=18f;
  Check(mission.FirstInjuryTimestamp==12f,"Original Mission injury timestamp is not write-once");
  bool refused=false;
  try{
   new OriginalCharacterHitEffectsBinding(mission,()=>true,()=>true,()=>true,()=>true,()=>false,
    null,_=>{},_=>{},_=>{});
  }catch(NotSupportedException){refused=true;}
  Check(refused,"Missing production time-scaler binding accepted");
  var order=new List<string>();
  var bound=new OriginalCharacterHitEffectsBinding(mission,()=>true,()=>true,()=>false,
   ()=>true,()=>false,_=>order.Add("time"),_=>order.Add("marker"),
   _=>order.Add("hurt"),_=>order.Add("drop"));
  bound.ValidateBindings();
  bound.FirstInjuryTimestamp=22f;bound.InvokeBulletTimeOnHit(false);
  bound.Hurt(.3f);bound.DropWeapons(true);bound.ShowStabilizeMarker(true);
  Check(mission.FirstInjuryTimestamp==12f&&order.SequenceEqual(new[]{"time","hurt","drop","marker"}),
    "Original production capability adapter changed event order/timestamp");
  return JsonSerializer.Serialize(new {sourceMissionFirstInjury="PASS",
   mandatoryProductionCapabilities=true,liveNpcGameplay=false});
 }
}