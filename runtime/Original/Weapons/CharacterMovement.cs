using UnityEngine;
using System.Text.Json;
namespace Crisis.Runtime.Weapons;

// Source-backed CharacterBody.Move + FixedUpdate locomotion logic.
// All forces are returned to the browser and applied by the existing real PhysX actor.
// The complete recovered CharacterBody remains pinned at source-reference/Original/CharacterBody.cs.
// This partial implementation must NOT be confused with full CharacterBody/physiology playback.
public partial class CharacterBody {
 private float acceleration;
 private float accelerationMagnitude;

 // Original CharacterBody.cs lines 1354-1361, unchanged.
 public void Move(float direction, float magnitude = 1f)
 {
     if (direction != 0f)
     {
         acceleration = direction;
         accelerationMagnitude = magnitude;
     }
 }

 private void ResetAcceleration() {
     acceleration = 0f;
 }

 // Adapted from original CharacterBody.FixedUpdate() lines 1373-1402.
 // Input/physics boundary replaces only Rigidbody.velocity and AddForce with
 // live PhysX snapshot and returned ForceMode.Acceleration command.
 // Owner-approved analogue target-speed scaling is deliberately retained.
 public object ExecuteSourceMovement(float velocityX,float fixedDeltaTime,
      float walkSpeed,float sprintSpeed,float analogMagnitude,
      bool sprinting,bool crippled,bool isOperator,bool noPlayerLimp,
      bool usingPrecision) {
     float appliedForceX = 0f;
     float animatorSpeed = 1f;
     bool reset = false;
     if (acceleration != 0f) {
         float num = Mathf.Sign(acceleration);
         bool flag = num != Direction;
         bool flag2 = crippled && (!isOperator || !noPlayerLimp);
         float num2 = walkSpeed * ((!flag2) ? 1f : 0.8f);
         if (sprinting && !flag && !flag2)
             num2 = sprintSpeed * ((!usingPrecision) ? 1f : 0.5f);
         if (isOperator && flag2)
             animatorSpeed = 0.8f;
         else
             animatorSpeed = ((!sprinting || flag || flag2 || !usingPrecision) ? 1f : 1.35f);
         // Intentional owner-approved mobile enhancement: full original speed
         // at full thumb travel, proportional below it; digital movement is 1.
         num2 *= Mathf.Clamp01(analogMagnitude);
         float num3 = Mathf.Abs(velocityX);
         if (!(num3 > num2)) {
             float num4 = num2 - num3;
             appliedForceX = num4 * num * fixedDeltaTime * 750f * accelerationMagnitude;
             ResetAcceleration();
             reset = true;
         }
     }
     return new { forceX=appliedForceX, reset, animatorSpeed };
 }
}
