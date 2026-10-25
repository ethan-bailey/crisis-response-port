using UnityEngine;
namespace UnityEngine {
 // Renderer/PhysX property adapters, not substitute simulation implementations.
 public enum CameraClearFlags { Skybox,Color,SolidColor,Depth,Nothing }
 public sealed class Camera:Behaviour {
  public static Camera main=>RuntimeWorld.Current.Objects.OfType<Camera>().FirstOrDefault(c=>c.isActiveAndEnabled);
  public float fieldOfView=60f,aspect=16f/9f;
  public int pixelWidth=1920,pixelHeight=1080;
  public int cullingMask=-1;
  public float orthographicSize=5f,nearClipPlane=0.3f,farClipPlane=1000f;
  public RenderTexture targetTexture;
  public DepthTextureMode depthTextureMode;
  public CameraClearFlags clearFlags;
  public Matrix4x4 worldToCameraMatrix {get=>Matrix4x4.Inverse(Matrix4x4FromTransform(transform));set=>throw new NotSupportedException("Custom camera view matrices require a render pipeline driver");}
  public Matrix4x4 projectionMatrix {get;set;}=Matrix4x4.identity;
  static Matrix4x4 Matrix4x4FromTransform(Transform value){var result=new Matrix4x4();result.SetTRS(value.position,value.rotation,Vector3.one);return result;}
  public Ray ScreenPointToRay(Vector2 screenPoint) {
   if(pixelWidth<=0||pixelHeight<=0||!float.IsFinite(fieldOfView)||fieldOfView<=0||fieldOfView>=180)throw new InvalidOperationException("Camera projection settings are invalid");
   float nx=(screenPoint.x/pixelWidth-.5f)*2f,ny=(screenPoint.y/pixelHeight-.5f)*2f;
   float halfHeight=MathF.Tan(fieldOfView*MathF.PI/360f);
   float cameraAspect=aspect>0?aspect:(float)pixelWidth/pixelHeight;
   var direction=(transform.forward+transform.right*(nx*halfHeight*cameraAspect)+transform.up*(ny*halfHeight)).normalized;
   return new Ray(transform.position,direction);
  }
  public Vector3 WorldToScreenPoint(Vector3 point){
   var local=transform.InverseTransformPoint(point);
   float x=pixelWidth*(0.5f+local.x/(2f*MathF.Max(nearClipPlane,local.z)*MathF.Tan(fieldOfView*MathF.PI/360f)*aspect));
   float y=pixelHeight*(0.5f+local.y/(2f*MathF.Max(nearClipPlane,local.z)*MathF.Tan(fieldOfView*MathF.PI/360f)));
   return new Vector3(x,y,local.z);
  }
  public Vector3 WorldToViewportPoint(Vector3 point){var screen=WorldToScreenPoint(point);return new Vector3(pixelWidth==0?0:screen.x/pixelWidth,pixelHeight==0?0:screen.y/pixelHeight,screen.z);}
  public void CopyFrom(Camera other){ArgumentNullException.ThrowIfNull(other);fieldOfView=other.fieldOfView;aspect=other.aspect;pixelWidth=other.pixelWidth;pixelHeight=other.pixelHeight;cullingMask=other.cullingMask;orthographicSize=other.orthographicSize;nearClipPlane=other.nearClipPlane;farClipPlane=other.farClipPlane;targetTexture=other.targetTexture;depthTextureMode=other.depthTextureMode;clearFlags=other.clearFlags;projectionMatrix=other.projectionMatrix;}
  public void RenderWithShader(Shader shader,string replacementTag)=>throw new NotSupportedException("Replacement shader rendering requires a render pipeline driver");
  public void Render()=>throw new NotSupportedException("Camera rendering requires a bound render pipeline driver");
 }
 public sealed class Rigidbody:Component {
  public Vector3 velocity,angularVelocity;
  public float mass=1f,drag,angularDrag=0.05f;
  public float maxAngularVelocity=7f;
  public bool useGravity=true;
  public CollisionDetectionMode collisionDetectionMode;
  public RigidbodyConstraints constraints;
  public RigidbodyInterpolation interpolation;
  public Action<Vector3,Vector3> ForceSink;
  // Physical-arm compatibility: the original component emits force and
  // kinematic operations to the real pinned PhysX adapter, never a JS arm AI.
  public Action<Vector3,Vector3,ForceMode> PhysicsForceSink;
  public Action<Vector3,ForceMode> TorqueSink;
  public Action<bool> KinematicChanged;
  bool kinematic=true;
  public bool isKinematic {
   get=>kinematic;
   set {if(kinematic==value)return;kinematic=value;KinematicChanged?.Invoke(value);}
  }
  public void AddForceAtPosition(Vector3 force,Vector3 position){
   if(PhysicsForceSink!=null){PhysicsForceSink(force,position,ForceMode.Force);return;}
   if(ForceSink!=null){ForceSink(force,position);return;}
   if(world?.PhysicsForceSink is { } worldSink){worldSink(this,force,position,ForceMode.Force);return;}
   throw new NotSupportedException("Dynamic ballistic force sink not connected");
  }
  public void AddForce(Vector3 force,ForceMode mode=ForceMode.Force){
   if(PhysicsForceSink!=null){PhysicsForceSink(force,transform.position,mode);return;}
   if(world?.PhysicsForceSink is { } worldSink){worldSink(this,force,transform.position,mode);return;}
   throw new NotSupportedException("Dynamic rigidbody force sink not connected");
  }
  public void AddForce(float x,float y,float z,ForceMode mode=ForceMode.Force)=>AddForce(new Vector3(x,y,z),mode);
  public void AddForceAtPosition(Vector3 force,Vector3 position,ForceMode mode){
   if(PhysicsForceSink!=null){PhysicsForceSink(force,position,mode);return;}
   if(mode==ForceMode.Force&&ForceSink!=null){ForceSink(force,position);return;}
   if(world?.PhysicsForceSink is { } worldSink){worldSink(this,force,position,mode);return;}
   if(mode==ForceMode.Force){AddForceAtPosition(force,position);return;}
   throw new NotSupportedException("Source physical arm ForceMode requires PhysX");
  }
  public void AddTorque(Vector3 torque,ForceMode mode=ForceMode.Force){if(TorqueSink!=null){TorqueSink(torque,mode);return;}if(world?.PhysicsTorqueSink is { } worldSink){worldSink(this,torque,mode);return;}throw new NotSupportedException("Rigidbody torque requires a bound PhysX driver");}
 }
 [Flags] public enum RigidbodyConstraints {None=0,FreezePositionX=2,FreezePositionY=4,FreezePositionZ=8,FreezeRotationX=16,FreezeRotationY=32,FreezeRotationZ=64,FreezePosition=14,FreezeRotation=112,FreezeAll=126}
 public enum RigidbodyInterpolation {None=0,Interpolate=1,Extrapolate=2}
 public enum CollisionDetectionMode {Discrete,Continuous,ContinuousDynamic,ContinuousSpeculative}
}
