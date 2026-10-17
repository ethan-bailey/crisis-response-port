using UnityEngine;
using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.Character;

// Adapted directly from the recovered original MuscleControl.cs. LimbMuscle
// calls target the original HingeJoint/PhysX-backed OriginalLimbMuscle instances.
// The source's CharacterAudio and hand posture effects remain explicitly gated
// when their actual NPC equipment/audio dependencies are not installed.
public sealed class OriginalMuscleControl {
 readonly CharacterBody body;
 Vector2 spasmIntervalRange,spasmIntensityRange;
 float startTime,spasmTimestamp,spasmDuration,spasmIntensity,crampDuration;
 bool singleSpasm,decorticate,decerberateLeft,decerberateRight;
 public bool SpasmsActive {get;private set;}
 public bool CrampsActive {get;private set;}
 public bool PainMovementsActive {get;set;}
 public OriginalMuscleControl(CharacterBody body) {
  this.body=body??throw new ArgumentNullException(nameof(body));
 }
 IEnumerable<OriginalLimbMuscle> Muscles()
  =>body.limbs.Where(l=>l.SourceMuscle!=null).Select(l=>l.SourceMuscle);
 void Signal(LimbId id,float extend,float contract,float intensity,float duration=1f) {
  body.limbs.FirstOrDefault(l=>l.id==id)?.SourceMuscle
   ?.SetSignal(extend,contract,duration,intensity,now:Time.time);
 }
 public void Update() {
  if(decorticate)AnimateDecorticate();
  else if(decerberateLeft||decerberateRight)AnimateDecerberate();
  else if(CrampsActive)AnimateCramp();
  else if(SpasmsActive||PainMovementsActive)AnimateSpasms();
 }
 public void SingleSpasmOnBrainHit(){
  bool spasmsActive=(CrampsActive=(decerberateLeft=(decerberateRight=(decorticate=false))));
  SpasmsActive=spasmsActive;
  SingleImpulse(body.SourcePhysiology.BrainTissue/1000f,0.05f);
 }
 public void StartHighIntensityMuscleSpasms(){
  float num=UnityEngine.Random.Range(.01f,.5f);
  StartSpasms(new Vector2(num,num*2f),new Vector2(0f,.3f));
  SpasmsActive=true;
 }
 public void StartLowIntensityMuscleSpasms(){
  if(!SpasmsActive){
   float period=body.SourcePhysiology.HeartRate==0f?float.PositiveInfinity:60f/body.SourcePhysiology.HeartRate;
   StartSpasms(new Vector2(period*2f,period*4f),new Vector2(.05f,.35f));
   SpasmsActive=true;
  }
 }
 public void ActivateMusclesOnRegainedConsciousness(){
  PainMovementsActive=true;
  StartSpasms(new Vector2(1f,3f),new Vector2(.1f,.25f));
 }
 void StartSpasms(Vector2 interval,Vector2 intensity){
  spasmIntervalRange=interval;spasmIntensityRange=intensity;
 }
 public void SingleImpulse(float strength,float duration){
  spasmTimestamp=Time.time;spasmDuration=duration;
  SpasmsActive=(singleSpasm=true);
  foreach(var muscle in Muscles())
   muscle.SetSignal(1f,0f,UnityEngine.Random.Range(.025f,.15f),now:Time.time);
 }
 public void ActivateCramp(float duration=3f){
  SpasmsActive=false;CrampsActive=true;
  startTime=Time.time;crampDuration=duration;
 }
 public void SetDecorticate(){
  SpasmsActive=(decorticate=true);
  startTime=Time.time;crampDuration=60f;
 }
 public void SetDecerberate(){
  float value=UnityEngine.Random.value;
  if(value<.5f){decerberateLeft=true;decerberateRight=true;}
  else if(value<.75f){decerberateLeft=false;decerberateRight=true;}
  else {decerberateLeft=true;decerberateRight=false;}
  SpasmsActive=true;startTime=Time.time;crampDuration=60f;
 }
 public void Relax(){
  foreach(var muscle in Muscles())muscle.Relax();
  PainMovementsActive=false;
 }
 void AnimateSpasms(){
  float num=Time.time-spasmTimestamp;
  if(num>=spasmDuration){
   if(singleSpasm){
    Relax();SpasmsActive=(singleSpasm=false);return;
   }
   spasmTimestamp=Time.time;
   spasmDuration=UnityEngine.Random.Range(spasmIntervalRange.x,spasmIntervalRange.y);
   spasmIntensity=UnityEngine.Random.Range(spasmIntensityRange.x,spasmIntensityRange.y);
   // Original pain-generated CharacterAudio.Hurt stays gated, not silently
   // replaced by a generic WeaponEffects sound event.
   foreach(var limb in body.limbs)
    if(limb.SourceMuscle!=null&&limb.id!=LimbId.RightLowerLeg&&limb.id!=LimbId.LeftLowerLeg)
     limb.SourceMuscle.StartSpasm(UnityEngine.Random.value,UnityEngine.Random.value,
        spasmDuration,spasmIntensity,PainMovementsActive,now:Time.time);
   return;
  }
  float num2=1f-Mathf.Max(num*2f/spasmDuration,0f);
  foreach(var muscle in Muscles())
   muscle.UpdateStrength(num2*body.SourcePhysiology.BloodOxygen);
 }
 void AnimateCramp(){
  float num=Time.time-startTime;
  float num2=crampDuration*.75f,num3=crampDuration*.25f;
  float num4=1f-Mathf.Max(num-num2,0f)/num3;
  foreach(var muscle in Muscles())
   muscle.SetSignal(1f,1f,1f,num4*body.SourcePhysiology.BloodOxygen,now:Time.time);
  if(num4<=0f)CrampsActive=false;
 }
 void FistHands(){
  // Original Hand.SetMode requires the actual source arm IK/hand inventory.
  // Do not synthesize missing NPC hands.
  body.rightArm?.hand?.SetMode(global::Hand.Mode.Fist);
  body.leftArm?.hand?.SetMode(global::Hand.Mode.Fist);
 }
 void AnimateDecerberate(){
  if(Time.time>=spasmTimestamp+spasmDuration){
   spasmTimestamp=Time.time;spasmIntensity=UnityEngine.Random.Range(.2f,1f);
   spasmDuration=body.SourcePhysiology.HeartRate==0f?float.PositiveInfinity:60f/body.SourcePhysiology.HeartRate;
  }
  FistHands();
  float oxygen=body.SourcePhysiology.BloodOxygen;
  Signal(LimbId.Head,spasmIntensity,0f,oxygen);
  if(decerberateRight){
   Signal(LimbId.RightUpperArm,spasmIntensity,0f,oxygen);
   Signal(LimbId.RightLowerArm,spasmIntensity,0f,oxygen);
   Signal(LimbId.RightHand,spasmIntensity,0f,oxygen);
   Signal(LimbId.RightUpperLeg,spasmIntensity,0f,oxygen);
   Signal(LimbId.RightLowerLeg,spasmIntensity,0f,oxygen);
   Signal(LimbId.RightFoot,spasmIntensity,0f,oxygen);
  }
  if(decerberateLeft){
   Signal(LimbId.LeftUpperArm,spasmIntensity,0f,oxygen);
   Signal(LimbId.LeftLowerArm,spasmIntensity,0f,oxygen);
   Signal(LimbId.LeftHand,spasmIntensity,0f,oxygen);
   Signal(LimbId.LeftUpperLeg,spasmIntensity,0f,oxygen);
   Signal(LimbId.LeftLowerLeg,spasmIntensity,0f,oxygen);
   Signal(LimbId.LeftFoot,spasmIntensity,0f,oxygen);
  }
 }
 void AnimateDecorticate(){
  if(Time.time>=spasmTimestamp+spasmDuration){
   spasmTimestamp=Time.time;spasmIntensity=UnityEngine.Random.Range(.2f,1f);
   spasmDuration=body.SourcePhysiology.HeartRate==0f?float.PositiveInfinity:60f/body.SourcePhysiology.HeartRate;
  }
  float oxygen=body.SourcePhysiology.BloodOxygen;
  FistHands();
  Signal(LimbId.RightUpperArm,spasmIntensity,.2f,oxygen);
  Signal(LimbId.LeftUpperArm,spasmIntensity,.2f,oxygen);
  Signal(LimbId.RightLowerArm,0f,spasmIntensity,oxygen);
  Signal(LimbId.LeftLowerArm,0f,spasmIntensity,oxygen);
  Signal(LimbId.RightHand,spasmIntensity,0f,oxygen);
  Signal(LimbId.LeftHand,spasmIntensity,0f,oxygen);
  Signal(LimbId.RightUpperLeg,spasmIntensity,0f,oxygen);
  Signal(LimbId.LeftUpperLeg,spasmIntensity,0f,oxygen);
  Signal(LimbId.RightLowerLeg,spasmIntensity,0f,oxygen);
  Signal(LimbId.LeftLowerLeg,spasmIntensity,0f,oxygen);
  Signal(LimbId.RightFoot,spasmIntensity,0f,oxygen);
  Signal(LimbId.LeftFoot,spasmIntensity,0f,oxygen);
 }
 public void OnDeath(){
  Relax();SpasmsActive=false;CrampsActive=false;
  decorticate=false;decerberateLeft=false;decerberateRight=false;
 }
}
