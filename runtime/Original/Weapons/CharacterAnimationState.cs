using UnityEngine;
namespace Crisis.Runtime.Weapons;

// Recovered CharacterBody.UpdateAnimation() (source lines 1054-1068):
// its parameter/capsule state now executes inside the compiled C# runtime.
// This intentionally does not substitute for the original AnimatorController
// clip graph or full Physiology, which remain explicit separate dependencies.
public partial class CharacterBody {
 Animator sourceAnimator;
 CapsuleCollider sourceBodyCollider;
 readonly int hashMoving=Animator.StringToHash("Moving");
 readonly int hashBackwards=Animator.StringToHash("Backwards");
 readonly int hashMoveSpeed=Animator.StringToHash("MoveSpeed");
 readonly int hashKneelRatio=Animator.StringToHash("KneelRatio");
 readonly int hashCrippledRatio=Animator.StringToHash("CrippledRatio");

 public float KneelRatio {get;private set;}
 public float TargetKneelRatio {get;private set;}
 public void InitializeSourceAnimation(Animator animator,CapsuleCollider collider) {
  sourceAnimator=animator??throw new ArgumentNullException(nameof(animator));
  sourceBodyCollider=collider??throw new ArgumentNullException(nameof(collider));
 }
 public object ExecuteSourceUpdateAnimation(bool isKneeling,bool isMoving,bool isMovingBackwards,
    float velocity,float walkSpeed,float sprintSpeed,float crippledRatio,bool isOperator,
    float deltaTime,float colliderVerticalOffset) {
  if(sourceAnimator==null||sourceBodyCollider==null)
   throw new InvalidOperationException("Original character Animator/CapsuleCollider not bound");
  // From original CharacterBody.UpdateAnimation, retaining every equation and source parameter.
  TargetKneelRatio = ((!isKneeling) ? 0f : 1f);
  float f = TargetKneelRatio - KneelRatio;
  float num = 5f * Mathf.Clamp(Mathf.Abs(f), 0.025f, 1f);
  KneelRatio += Mathf.Sign(f) * deltaTime * num;
  sourceBodyCollider.height = ((!(KneelRatio < 0.5f)) ? 1.25f : 1f);
  sourceBodyCollider.center = new Vector3((!(KneelRatio < 0.5f)) ? (-0.15f) : 0f, colliderVerticalOffset, 0f);
  float num2 = Mathf.Abs(velocity);
  sourceAnimator.SetBool(hashMoving, isMoving);
  sourceAnimator.SetBool(hashBackwards, isMovingBackwards);
  sourceAnimator.SetFloat(hashMoveSpeed, (!(num2 < walkSpeed)) ? (Mathf.Clamp01((num2 - walkSpeed) / (sprintSpeed - walkSpeed)) * 0.5f) : 0f);
  sourceAnimator.SetFloat(hashKneelRatio, KneelRatio);
  sourceAnimator.SetFloat(hashCrippledRatio, (!isOperator) ? crippledRatio : 0f);
  return new {KneelRatio,TargetKneelRatio,colliderHeight=sourceBodyCollider.height,
   centerX=sourceBodyCollider.center.x,centerY=sourceBodyCollider.center.y,
   moving=sourceAnimator.GetBool(hashMoving),
   backwards=sourceAnimator.GetBool(hashBackwards),
   moveSpeed=sourceAnimator.GetFloat(hashMoveSpeed),
   crippled=sourceAnimator.GetFloat(hashCrippledRatio)};
 }
}
