// Executes the recovered Unity Character animator.controller (4 original states,
// 11 original transitions, authored transition durations and blend thresholds).
// The data is exported from the exact AssetRipper YAML, never hand-designed.
export class SourceCharacterAnimator {
 constructor(data){
  if(data?.schema!==1||data?.sourceSha256!=='d1ef99a73f2f600c09be9f099e6f88b10aff8328099621c45ce19a6c5635803b')
   throw Error('Original Character AnimatorController identity mismatch');
  if(Object.keys(data.states??{}).length!==4)throw Error('Incomplete source AnimatorController');
  this.data=data;this.state=data.defaultState;
  this.transition=null;this.stateTime=0;this.fromTime=0;this.toTime=0;
  this.lastParams={Moving:false,Backwards:false,KneelRatio:0,CrippledRatio:0,MoveSpeed:0};
 }
 static matches(transition,params){
  return transition.conditions.every(({mode,parameter,threshold})=>{
   if(!(parameter in params))throw Error('Missing source Animator parameter '+parameter);
   const v=params[parameter];
   if(mode===1)return v===true;
   if(mode===2)return v===false;
   if(mode===3)return v>threshold;
   if(mode===4)return v<threshold;
   throw Error('Unsupported source AnimatorConditionMode '+mode);
  });
 }
 clipWeights(state,params,time){
  const node=this.data.states[state];
  if(!node)throw Error('Missing source Animator state: '+state);
  const motion=node.motion;
  if(motion.clip)return [{clip:motion.clip,weight:1,time}];
  const tree=motion.blendTree;
  if(tree?.blendType!==0||tree.children.length!==2)throw Error('Unsupported original BlendTree');
  const [a,b]=tree.children,x=Number(params[tree.parameter]);
  const fraction=Math.max(0,Math.min(1,(x-a.threshold)/(b.threshold-a.threshold)));
  return [{clip:a.clip,weight:1-fraction,time},{clip:b.clip,weight:fraction,time}].filter(c=>c.weight>0);
 }
 step(dt,params){
  if(!Number.isFinite(dt)||dt<0)throw Error('Invalid original Animator time');
  const p={...this.lastParams,...params};
  for(const key of this.data.parameters)if(!(key in p))throw Error('Missing Animator parameter '+key);
  this.lastParams=p;
  if(!this.transition){
   const current=this.data.states[this.state];
   const next=current.transitions.find(t=>!t.hasExitTime&&SourceCharacterAnimator.matches(t,p));
   if(next){
    if(!next.fixedDuration||next.interruptionSource!==0)throw Error('Unsupported original interruption semantics');
    this.transition={from:this.state,to:next.destination,duration:next.duration,elapsed:0};
    this.fromTime=this.stateTime;this.toTime=0;
   }
  }
  if(this.transition){
   const tr=this.transition;
   tr.elapsed+=dt;this.fromTime+=dt;this.toTime+=dt;
   const blend=tr.duration>0?Math.min(1,tr.elapsed/tr.duration):1;
   const clips=[...this.clipWeights(tr.from,p,this.fromTime).map(c=>({...c,weight:c.weight*(1-blend)})),
                ...this.clipWeights(tr.to,p,this.toTime).map(c=>({...c,weight:c.weight*blend}))].filter(c=>c.weight>0);
   if(blend>=1){
    this.state=tr.to;this.stateTime=this.toTime;this.transition=null;
   }
   return {state:this.data.states[this.state].name,transition:blend<1?{from:this.data.states[tr.from].name,to:this.data.states[tr.to].name,blend,duration:tr.duration}:null,clips};
  }
  this.stateTime+=dt;
  return {state:this.data.states[this.state].name,transition:null,clips:this.clipWeights(this.state,p,this.stateTime)};
 }
}
