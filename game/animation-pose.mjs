// The physics body owns locomotion. Keep pose height/lean, but do not also apply
// the net horizontal travel of a looping locomotion clip when root motion is off.
// Full Unity Animator root extraction, blending and IK remain outside this adapter.
export function posePosition(track,value,clip,applyRootMotion){
 const result=[...value];
 if(applyRootMotion||!clip.loop||track.path!=='Pelvis'||track.property!=='position')return result;
 const first=track.keys[0].value,last=track.keys.at(-1).value;
 for(const axis of [0,2])if(Math.abs(last[axis]-first[axis])>1e-5)result[axis]=first[axis];
 return result;
}
