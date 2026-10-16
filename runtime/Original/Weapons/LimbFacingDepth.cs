using UnityEngine;
using System.Collections.Generic;
namespace Crisis.Runtime.Weapons;

// Fields and Flip body from the recovered developer Limb.cs (lines 514-525).
// Runtime-only attachment reference has original LimbAttachment z fields;
// additional BallisticMaterial/LimbPhysiology wound/force dependencies pending.
public sealed class LimbAttachment : Component {
 public float zLeft, zRight;
}
public partial class Limb {
 public Transform visual;
 public float zLeft;
 public int renderIndexLeft,renderIndexRight;
 public List<LimbAttachment> attachments = new();
 float zRight;
 public void CaptureSourceVisualDepth() {
  zRight = visual==null ? 0f : visual.localPosition.z;
 }
 public void Flip()
 {
  Vector3 localPosition = visual.localPosition;
  localPosition.z = ((!Body.FacingRight) ? zLeft : zRight);
  visual.localPosition = localPosition;
  foreach (LimbAttachment attachment in attachments)
  {
   Vector3 localPosition2 = attachment.transform.localPosition;
   localPosition2.z = ((!Body.FacingRight) ? attachment.zLeft : attachment.zRight);
   attachment.transform.localPosition = localPosition2;
  }
 }
}
