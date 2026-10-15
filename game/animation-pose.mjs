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

export function locomotionBlend(speed,walkSpeed,sprintSpeed,thresholds){
 const parameter=Math.max(0,Math.min(1,(Math.abs(speed)-walkSpeed)/(sprintSpeed-walkSpeed)))*.5;
 return Math.max(0,Math.min(1,(parameter-thresholds[0])/(thresholds[1]-thresholds[0])));
}
// Both clips share normalized gait phase. Weight changes affect phase speed,
// never reset either clip or put their feet at different points in the stride.
export function advanceGait(phase,dt,weight,walkDuration,sprintDuration){
 const duration=walkDuration*(1-weight)+sprintDuration*weight;
 const next=(phase+dt/duration)%1;
 return {phase:next,walkTime:next*walkDuration,sprintTime:next*sprintDuration};
}
