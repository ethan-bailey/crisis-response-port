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
public sealed class HingeJoint:Component {
 bool springEnabled;
 JointSpring currentSpring;
 public Action<JointSpring,bool> SpringChanged;
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
