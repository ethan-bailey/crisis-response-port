// Read-only comparison of the source actor, contact body, and rendered root.
// Unity CapsuleCollider direction is an axis, not always the vertical axis.
export function describeGrounding({sourcePosition,physics,projectedPosition,capsule,center,boxes=[]}) {
  if (!Array.isArray(sourcePosition) || sourcePosition.length !== 3 ||
      !Array.isArray(projectedPosition) || projectedPosition.length !== 3 ||
      !capsule || ![capsule.m_Radius,capsule.m_Height,capsule.m_Direction].every(Number.isFinite)) return null;
  const [x,y,z]=sourcePosition;
  const offset=center||capsule.m_Center||{x:0,y:0,z:0};
  const verticalHalfExtent=capsule.m_Direction===1?capsule.m_Height/2:capsule.m_Radius;
  const capsuleCenterY=y+(offset.y||0);
  const bottom=capsuleCenterY-verticalHalfExtent;
  const floorCandidates=boxes.filter(b=>Array.isArray(b.position)&&Array.isArray(b.halfExtents))
    .map(b=>({name:b.name,top:b.position[1]+b.halfExtents[1]}))
    .filter(b=>b.top<=y+0.25&&b.top>=bottom-2)
    .sort((a,b)=>b.top-a.top);
  return {
    sourceRoot:[x,y,z],
    contactRoot:physics?[physics.x,physics.y,physics.z]:null,
    projectedRoot:[...projectedPosition],
    capsule:{direction:capsule.m_Direction,radius:capsule.m_Radius,height:capsule.m_Height,
      center:[offset.x||0,offset.y||0,offset.z||0],verticalHalfExtent,bottom},
    floorCandidates:floorCandidates.slice(0,5)
  };
}
