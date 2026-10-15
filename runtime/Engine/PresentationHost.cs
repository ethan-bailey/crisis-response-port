using System.Text.Json;
using UnityEngine;
namespace UnityEngine {
 // Renderer/PhysX property adapters, not substitute simulation implementations.
 public sealed class Camera:Component {public float fieldOfView,aspect;}
 public sealed class Rigidbody:Component {public Vector3 velocity;}
}
namespace Crisis.Runtime.Presentation {
 public sealed class CameraDoorSnapshot:Component {}
 public sealed class CameraWeaponSnapshot {public Transform Muzzle;}
 public sealed class CameraSubjectSnapshot:Component {
  public bool IsRagdoll;public float Direction;public Transform Eye,Torso;
  public CameraWeaponSnapshot ActiveWeapon;
  public Vector3 KinematicWorldPosition=>Torso.position;
 }
 public sealed class PresentationHost {
  SceneLoadResult scene;InteriorCameraSlice camera;CameraSubjectSnapshot subject;Rigidbody body;BodyPoseSlice pose;int ballisticMask;
  static Vector3 V(JsonElement a)=>new(a[0].GetSingle(),a[1].GetSingle(),a[2].GetSingle());
  public PresentationHost(string json){
   using var doc=JsonDocument.Parse(json);var c=doc.RootElement;
   scene=SceneLoader.Load(c.GetProperty("queryPackage").GetRawText(),c.GetProperty("queryScene").GetString(),false);
   foreach(var id in c.GetProperty("doors").EnumerateArray())if(scene.objects.TryGetValue(id.GetString(),out var o)&&o is GameObject go)go.AddComponent<CameraDoorSnapshot>();
   var camGO=new GameObject("Recovered InteriorCamera",scene.world);camera=camGO.AddComponent<InteriorCameraSlice>();
   var tr=c.GetProperty("cameraTransform").GetProperty("m_LocalPosition");camGO.transform.position=new(tr.GetProperty("x").GetSingle(),tr.GetProperty("y").GetSingle(),tr.GetProperty("z").GetSingle());
   camera.audioListener=new GameObject("Original listener position",scene.world).transform;camera.interiorCamera=camGO.AddComponent<Camera>();camera.interiorCamera.fieldOfView=c.GetProperty("camera").GetProperty("field of view").GetSingle();
   var s=c.GetProperty("cameraSettings");foreach(var name in new[]{"smoothing","fovSmoothing","centerOffset","obstacleHoldOff","doorHoldOff","fieldOfViewOverride"})typeof(InteriorCameraSlice).GetField(name).SetValue(camera,s.GetProperty(name).GetSingle());
   camera.overrideFieldOfView=s.GetProperty("overrideFieldOfView").GetInt32()!=0;camera.disableScrollPastDoors=s.GetProperty("disableScrollPastDoors").GetInt32()!=0;
   var g=c.GetProperty("graphics");camera.staticFov=g.GetProperty("staticFov").GetSingle();camera.dynamicFovMin=g.GetProperty("dynamicMin").GetSingle();camera.dynamicFovMax=g.GetProperty("dynamicMax").GetSingle();camera.useDynamicFov=g.GetProperty("useDynamicFov").GetBoolean();
   camera.obstacleMask=c.GetProperty("cameraObstacleMask").GetInt32();Physics.queriesHitTriggers=false;
   var subjectGO=new GameObject("Live character camera snapshot",scene.world);subject=subjectGO.AddComponent<CameraSubjectSnapshot>();body=subjectGO.AddComponent<Rigidbody>();subject.Direction=1;
   subject.Eye=new GameObject("Eye snapshot",scene.world).transform;subject.Torso=new GameObject("Torso snapshot",scene.world).transform;
   subject.ActiveWeapon=new(){Muzzle=new GameObject("Muzzle snapshot",scene.world).transform};camera.Subject=subject;camera.Initialize();
   pose=new(scene.world);var w=c.GetProperty("weapon");var ac=c.GetProperty("armConfiguration");
   pose.ActiveWeapon=new(){type=(WeaponBrace)w.GetProperty("type").GetInt32(),weight=w.GetProperty("weight").GetSingle(),alignmentModifier=w.GetProperty("alignmentModifier").GetSingle(),armConfiguration=new(){headAngle=ac.GetProperty("headAngle").GetSingle(),torsoAngle=ac.GetProperty("torsoAngle").GetSingle()}};
   pose.WeaponAimer.useSights=true;ballisticMask=c.GetProperty("cameraBallisticMask").GetInt32();
  }
  public string Step(string json){
   scene.world.MakeCurrent();using var d=JsonDocument.Parse(json);var f=d.RootElement;
   subject.transform.position=V(f.GetProperty("position"));subject.Direction=f.GetProperty("direction").GetSingle();subject.transform.rotation=Quaternion.AngleAxis(subject.Direction==1?0:180,Vector3.up);
   subject.Eye.position=V(f.GetProperty("eye"));subject.Torso.position=V(f.GetProperty("torso"));subject.ActiveWeapon.Muzzle.position=V(f.GetProperty("muzzle"));body.velocity=V(f.GetProperty("velocity"));camera.interiorCamera.aspect=f.GetProperty("aspect").GetSingle();
   camera.Step();
   pose.IsMoving=f.GetProperty("moving").GetBoolean();pose.IsSprinting=f.GetProperty("sprinting").GetBoolean();pose.IsKneeling=f.GetProperty("kneeling").GetBoolean();pose.RotationY=subject.Direction==1?0:180;
   pose.AimPoint=V(f.GetProperty("aim"));pose.Eye.transform.position=subject.Eye.position;
   var shoulder=V(f.GetProperty("shoulder"));var delta=pose.AimPoint-shoulder;
   // PlayerController.HandleWeaponAim, current interaction menu closed and weapon unblocked.
   if(!pose.ActiveWeapon.IsBlocked&&delta.sqrMagnitude<0.122499995f)pose.AimPoint+=delta.normalized*.35f;
   var muzzle=subject.ActiveWeapon.Muzzle.position;var direction=new Vector3(subject.Direction,0,0);var length=(muzzle-shoulder).magnitude;
   pose.ActiveWeapon.IsBlocked=Physics.Raycast(new Ray(shoulder,direction),out var obstruction,length,ballisticMask);
   if(pose.ActiveWeapon.IsBlocked)pose.AimPoint=new Vector3(subject.transform.position.x+subject.Direction*(obstruction.point.x-shoulder.x),0,subject.transform.position.z);
   pose.WeaponAimer.Step();
   // CharacterBody.UpdateWeaponTransform, original shoulder/player branch.
   var vector11=muzzle-V(f.GetProperty("weaponRight"))*length;var vector12=pose.AimPoint-vector11;
   float targetAngle=Mathf.Atan2(vector12.y,Mathf.Abs(vector12.x))*57.29578f;
   pose.ActiveWeapon.Angle=Mathf.LerpAngle(pose.ActiveWeapon.Angle,targetAngle,Time.deltaTime*pose.ActiveWeapon.AlignmentSpeed);
   pose.Step();var arms=Solve(f.GetProperty("arms"));
   return JsonSerializer.Serialize(new{camera=camera.Snapshot(),pose=pose.Snapshot(),arms});
  }
  public static List<object> Solve(JsonElement values){var arms=new List<object>();
   foreach(var a in values.EnumerateArray()){
    bool ok=InverseKinematics2D.SolveTwoBoneAnalytic(out var u,out var l,!a.GetProperty("flip").GetBoolean(),a.GetProperty("length1").GetSingle(),a.GetProperty("length2").GetSingle(),a.GetProperty("x").GetSingle(),a.GetProperty("y").GetSingle());
    // ArmIKSolver.Update's original radians-to-bone-angle conversion.
    u=(u+(float)Math.PI/2f)*57.29578f;l=u+l*57.29578f;arms.Add(new{solved=ok,upper=u,lower=l});
   }
return arms;}
  public static string SolveArms(string json){using var d=JsonDocument.Parse(json);return JsonSerializer.Serialize(Solve(d.RootElement));}
 }
}
