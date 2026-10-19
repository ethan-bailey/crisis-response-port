namespace UnityEngine;
// Unity 2017.2 source API surface for unchanged physical ArmController.
[AttributeUsage(AttributeTargets.Field)]
public sealed class RangeAttribute(float minimum,float maximum):Attribute {
 public float min=minimum,max=maximum;
}
public enum ForceMode {Force=0,Acceleration=5,Impulse=1,VelocityChange=2}
public struct JointSpring {
 public float spring,damper,targetPosition;
}
public struct JointLimits {
 public float min,max,bounciness,bounceMinVelocity,contactDistance;
}
public class Joint:Component {
 public Rigidbody connectedBody;
 public Vector3 anchor,connectedAnchor;
 public bool autoConfigureConnectedAnchor=true;
 public float breakForce=float.PositiveInfinity,breakTorque=float.PositiveInfinity;
 public float massScale=1f,connectedMassScale=1f;
 public bool enableCollision,enablePreprocessing=true;
}
public struct JointMotor {public float targetVelocity,force;public bool freeSpin;}
public sealed class FixedJoint:Joint { }
public sealed class HingeJoint:Joint {
 public JointMotor motor;
 public bool useMotor;
 bool springEnabled;
 JointSpring currentSpring;
 JointLimits currentLimits;
 bool limitsEnabled;
 public Action<JointSpring,bool> SpringChanged;
 public Action<JointLimits,bool> LimitsChanged;
 public JointLimits limits {
  get=>currentLimits;
  set {currentLimits=value;LimitsChanged?.Invoke(currentLimits,limitsEnabled);}
 }
 public bool useLimits {
  get=>limitsEnabled;
  set {limitsEnabled=value;LimitsChanged?.Invoke(currentLimits,limitsEnabled);}
 }
 public bool useSpring {
  get=>springEnabled;
  set {springEnabled=value;SpringChanged?.Invoke(currentSpring,springEnabled);}
 }
 public JointSpring spring {
  get=>currentSpring;
  set {currentSpring=value;SpringChanged?.Invoke(currentSpring,springEnabled);}
 }
 // Last measured native PhysX joint angle (Unity degree convention).
 public float angle {get;set;}
}

public static class OriginalArmTransformOverloads {
 public static Vector3 TransformPoint(this Transform transform,float x,float y,float z)
  =>transform.TransformPoint(new Vector3(x,y,z));
}
