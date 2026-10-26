export function stretchedBillboardParameters(velocity,size,renderer,basis={right:[1,0,0],up:[0,1,0]}){
  const dot=(a,b)=>a[0]*b[0]+a[1]*b[1]+a[2]*b[2];
  const speed=Math.hypot(...velocity),along=dot(velocity,basis.up),across=dot(velocity,basis.right);
  return {rotation:Math.atan2(-across,along),width:size,
    length:Math.max(.001,size*(renderer.m_LengthScale??1)+speed*(renderer.m_VelocityScale??0))};
}
