using UnityEngine;
namespace Crisis.Runtime.Weapons;
// Verbatim independent direction/distance helpers from the recovered
// CharacterBody.cs; original methods, no replacement gameplay systems.
public partial class CharacterBody {
 public float KinematicPosition => Torso.transform.position.x;
 public float KinematicDistanceToPosition(float position)
 {
     return Mathf.Abs(position - KinematicPosition);
 }
 public bool FacingDirection(float direction)
 {
     return Direction == direction;
 }
 public bool Facing(float position)
 {
     return Mathf.Sign(position - Position) == Direction;
 }
 public bool Facing(CharacterBody body)
 {
     if (body == null)
     {
         return false;
     }
     float num = Mathf.Sign(body.Position - Position);
     return Direction == num;
 }
 public bool BodyFacingMe(CharacterBody body)
 {
     if (body == null)
     {
         return false;
     }
     return body.Facing(this);
 }
}
