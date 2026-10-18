using UnityEngine;
namespace UnityEngine {
 // Renderer/PhysX property adapters, not substitute simulation implementations.
 public sealed class Camera:Component {public float fieldOfView,aspect;}
 public sealed class Rigidbody:Component {
  public Vector3 velocity;
  public Action<Vector3,Vector3> ForceSink;
  // Physical-arm compatibility: the original component emits force and
  // kinematic operations to the real pinned PhysX adapter, never a JS arm AI.
  public Action<Vector3,Vector3,ForceMode> PhysicsForceSink;
  public Action<bool> KinematicChanged;
  bool kinematic=true;
  public bool isKinematic {
   get=>kinematic;
   set {if(kinematic==value)return;kinematic=value;KinematicChanged?.Invoke(value);}
  }
  public void AddForceAtPosition(Vector3 force,Vector3 position){
   if(PhysicsForceSink!=null){PhysicsForceSink(force,position,ForceMode.Force);return;}
   if(ForceSink==null)throw new NotSupportedException("Dynamic ballistic force sink not connected");
   ForceSink(force,position);
  }
  public void AddForceAtPosition(Vector3 force,Vector3 position,ForceMode mode){
   if(PhysicsForceSink!=null){PhysicsForceSink(force,position,mode);return;}
   if(mode==ForceMode.Force){AddForceAtPosition(force,position);return;}
   throw new NotSupportedException("Source physical arm ForceMode requires PhysX");
  }
 }
}
