using UnityEngine;
using Crisis.Runtime.Weapons;
namespace Crisis.Runtime.Character;

// Exact original IKArmSequencer.Update / Play / NewFrame timing, Wait,
// sibling-frame synchronization and callback order (recovered 2026-10-16).
// Platform binding is ONLY the original solver/controller dependency surface;
// no simulated reload or invented arm action callbacks.
public interface IOriginalIKArmBindings {
 bool OnTarget(IKArmType arm);
 float WeaponAimAlignment();
 Transform TargetTransform(IKArmTarget target,IKArmType arm,out Vector3 offset);
 void TargetTransition(IKArmType arm,Transform target,Vector3 worldSpaceOffset,float seconds);
 void StartWait(IKArmType arm);
 void Invoke(IKControllerInvoke method);
}
public sealed class OriginalIKArmSequencer {
 readonly IOriginalIKArmBindings bindings;
 readonly IKArmType armType;
 IKArmAction currentAction;
 IKControllerInvoke callback;
 OriginalIKArmSequencer sibling;
 bool frameDone;
 float timestamp,frameTime;
 int waitForSiblingFrame;
 public List<IKArmAnimationFrame> Sequence {get;private set;}
 public bool IsPlaying=>Sequence!=null;
 public bool IsReady=>Sequence==null;
 public int CurrentFrame{get;private set;}
 public OriginalIKArmSequencer(IOriginalIKArmBindings bindings,IKArmType armType){
  this.bindings=bindings??throw new ArgumentNullException(nameof(bindings));
  this.armType=armType;
 }
 public void SetSibling(OriginalIKArmSequencer sibling){this.sibling=sibling;}
 public void Update(float time){
  if(Sequence==null)return;
  if(frameDone){
   if(callback!=IKControllerInvoke.None){
    bindings.Invoke(callback);
    callback=IKControllerInvoke.None;
   }
   NewFrame(++CurrentFrame,time);
   return;
  }
  switch(currentAction){
   case IKArmAction.Wait:
    frameDone=time-timestamp>=frameTime;
    break;
   case IKArmAction.Transition:
    frameDone=bindings.OnTarget(armType);
    break;
   case IKArmAction.WaitUntilLowered:
    frameDone=bindings.WeaponAimAlignment()<=10f;
    break;
   case IKArmAction.WaitForSiblingFrame:
    frameDone=sibling.HasFinishedFrame(waitForSiblingFrame);
    break;
  }
 }
 public void Play(List<IKArmAnimationFrame> sequence,float time,int abort=0){
  Sequence=sequence??throw new ArgumentNullException(nameof(sequence));
  CurrentFrame=0;
  NewFrame(0,time);
 }
 void NewFrame(int index,float time){
  if(index>=Sequence.Count){
   Sequence=null;
   CurrentFrame=0;
   return;
  }
  frameDone=false;
  timestamp=time;
  IKArmAnimationFrame frame=Sequence[index];
  frameTime=frame.time;
  callback=frame.callback;
  currentAction=frame.action;
  waitForSiblingFrame=frame.waitForSiblingFrame;
  Vector3 offset=Vector3.zero;
  if(frame.action==IKArmAction.Transition){
   Transform transform=bindings.TargetTransform(frame.transform,armType,out offset);
   if(transform!=null){
    if(frame.offset.sqrMagnitude!=0f)
     offset=transform.TransformVector(frame.offset);
    bindings.TargetTransition(armType,transform,offset,frame.time);
   }
  }else{
   bindings.StartWait(armType);
  }
 }
 public bool HasFinishedFrame(int frame)=>Sequence==null||CurrentFrame>frame;
}
public sealed class OriginalIKArmControllerPlayback {
 public OriginalIKArmSequencer RightSequencer {get;}
 public OriginalIKArmSequencer LeftSequencer {get;}
 public OriginalIKArmControllerPlayback(IOriginalIKArmBindings bindings){
  RightSequencer=new(bindings,IKArmType.Right);
  LeftSequencer=new(bindings,IKArmType.Left);
  RightSequencer.SetSibling(LeftSequencer);
  LeftSequencer.SetSibling(RightSequencer);
 }
 public void Play(IKArmSequenceRuntime sequence,float time){
  RightSequencer.Play(sequence.right,time);
  LeftSequencer.Play(sequence.left,time);
 }
 public void Update(float time){
  RightSequencer.Update(time);
  LeftSequencer.Update(time);
 }
 public bool IsPlaying=>RightSequencer.IsPlaying||LeftSequencer.IsPlaying;
}
public sealed class OriginalIKSolverBindings : IOriginalIKArmBindings {
 readonly CharacterBody body;
 readonly ArmIKSolver right,left;
 readonly Func<float> alignment;
 readonly Func<IKArmTarget,IKArmType,(Transform target,Vector3 offset)> resolve;
 readonly Action<IKControllerInvoke> callback;
 public OriginalIKSolverBindings(CharacterBody body,ArmIKSolver right,ArmIKSolver left,
     Func<float> alignment,Func<IKArmTarget,IKArmType,(Transform target,Vector3 offset)> resolve,
     Action<IKControllerInvoke> callback){
  this.body=body??throw new ArgumentNullException(nameof(body));
  this.right=right??throw new ArgumentNullException(nameof(right));
  this.left=left??throw new ArgumentNullException(nameof(left));
  this.alignment=alignment??throw new ArgumentNullException(nameof(alignment));
  this.resolve=resolve??throw new ArgumentNullException(nameof(resolve));
  this.callback=callback??throw new ArgumentNullException(nameof(callback));
 }
 ArmIKSolver Arm(IKArmType type)=>type==IKArmType.Right?right:left;
 public bool OnTarget(IKArmType type)=>Arm(type).OnTarget;
 public float WeaponAimAlignment()=>alignment();
 public Transform TargetTransform(IKArmTarget target,IKArmType arm,out Vector3 offset){
  var t=resolve(target,arm);offset=t.offset;return t.target;
 }
 public void TargetTransition(IKArmType arm,Transform target,Vector3 offset,float seconds)
    =>Arm(arm).TargetTransition(target,offset,seconds);
 public void StartWait(IKArmType arm)=>Arm(arm).StartWait();
 public void Invoke(IKControllerInvoke method)=>callback(method);
}
