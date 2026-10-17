using UnityEngine;

namespace Crisis.Runtime.Weapons;

// Original Mission.FirstInjuryTimestamp, recovered verbatim from
// source-reference/Original/CharacterDependencies/Mission.cs.  Shared mission
// state, never an actor-local event-observer timestamp.
public sealed class OriginalMissionInjuryState {
 float firstInjuryTimestamp;
 public float FirstInjuryTimestamp {
  get { return firstInjuryTimestamp; }
  set {
   if(firstInjuryTimestamp==0f)firstInjuryTimestamp=value;
  }
 }
}

// Production adapter is deliberately strict: no one can claim a complete hit
// without the original audio, time, marker and actual equipment side effects.
public sealed class OriginalCharacterHitEffectsBinding : IOriginalCharacterHitEffects {
 readonly OriginalMissionInjuryState mission;
 readonly Func<bool> roomsChecked,hasOccupants,occupantsSafe,markers,voicePlaying;
 readonly Action<bool> bulletTime,marker;
 readonly Action<float> hurt;
 readonly Action<bool> dropWeapons;
 public OriginalCharacterHitEffectsBinding(
  OriginalMissionInjuryState mission,Func<bool> roomsChecked,
  Func<bool> hasOccupants,Func<bool> occupantsSafe,Func<bool> markers,
  Func<bool> voicePlaying,Action<bool> bulletTime,Action<bool> marker,
  Action<float> hurt,Action<bool> dropWeapons){
  this.mission=mission;this.roomsChecked=roomsChecked;this.hasOccupants=hasOccupants;
  this.occupantsSafe=occupantsSafe;this.markers=markers;
  this.voicePlaying=voicePlaying;this.bulletTime=bulletTime;
  this.marker=marker;this.hurt=hurt;this.dropWeapons=dropWeapons;
  ValidateBindings();
 }
 public void ValidateBindings(){
  if(mission==null||roomsChecked==null||hasOccupants==null||
     occupantsSafe==null||markers==null||voicePlaying==null||
     bulletTime==null||marker==null||hurt==null||dropWeapons==null)
   throw new NotSupportedException("Original Mission/TimeScaler/CharacterAudio/markers/weapon-drop production graph incomplete");
 }
 public float FirstInjuryTimestamp {set { ValidateBindings();mission.FirstInjuryTimestamp=value; }}
 public bool AllRoomsChecked => roomsChecked();
 public bool HasOccupants => hasOccupants();
 public bool AllOccupantsRenderedSafe => occupantsSafe();
 public bool ShowInteractionMarkers => markers();
 public bool VoicePlaying => voicePlaying();
 public void InvokeBulletTimeOnHit(bool isPlayer){ValidateBindings();bulletTime(isPlayer);}
 public void ShowStabilizeMarker(bool show){ValidateBindings();marker(show);}
 public void Hurt(float delay){ValidateBindings();hurt(delay);}
 public void DropWeapons(bool attachToRagdoll){ValidateBindings();dropWeapons(attachToRagdoll);}
}
