const TAU=Math.PI*2;
const clamp01=x=>Math.max(0,Math.min(1,x));
const unit=(v)=>{const n=Math.hypot(...v);return n>1e-9?v.map(x=>x/n):[0,0,1];};

/** Sample the recovered Unity 2017 cone-base (4) and circle-edge (10) shapes.
 * Random inputs are explicit so tests can cover deterministic boundary cases.
 * The app supplies a stable approximation because Unity's per-system random
 * seed/state is not part of the browser event contract yet.
 */
export function sampleRecoveredParticleShape(shape,random={}) {
  const value=(key,fallback)=>{
    const x=Number.isFinite(random[key])?random[key]:fallback;
    if(x<0||x>1)throw new RangeError('Particle shape random sample outside [0,1]: '+key);
    return x;
  };
  const type=shape?.type,radius=shape?.radius?.value,arc=shape?.arc?.value;
  if(![4,10].includes(type))throw new RangeError('Unsupported recovered Unity particle shape type: '+type);
  if(!Number.isFinite(radius)||radius<0||!Number.isFinite(arc)||arc<=0||arc>360)
    throw new RangeError('Recovered Unity particle shape has invalid radius or arc');
  const angle=TAU*arc/360*value('arc',.5),radiusSample=value('radius',.5);
  const radial=type===10?radius:radius*Math.sqrt(radiusSample);
  const position=[radial*Math.cos(angle),radial*Math.sin(angle),0];
  let direction;
  if(type===4){
    const coneHalfAngle=(shape.angle||0)*Math.PI/180;
    direction=unit([Math.sin(coneHalfAngle)*Math.cos(angle),Math.sin(coneHalfAngle)*Math.sin(angle),Math.cos(coneHalfAngle)]);
  }else direction=[0,0,1];
  const randomDirection=clamp01(shape.randomDirectionAmount||0);
  if(randomDirection){
    const z=1-2*value('directionZ',.5),phi=TAU*value('directionArc',.5),r=Math.sqrt(Math.max(0,1-z*z));
    const sampled=[r*Math.cos(phi),r*Math.sin(phi),z];
    direction=unit(direction.map((x,i)=>x*(1-randomDirection)+sampled[i]*randomDirection));
  }
  const thickness=clamp01(shape.radiusThickness??1);
  if(type===4&&thickness<1){
    const inner=radius*(1-thickness);
    const scale=Math.sqrt(inner*inner+radius*radiusSample*(radius*radius-inner*inner));
    position[0]=scale*Math.cos(angle);position[1]=scale*Math.sin(angle);
  }
  return {position,direction};
}
