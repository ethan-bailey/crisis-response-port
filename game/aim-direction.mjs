// Touch device adapter: keep a direction, never a persistent screen-space target.
export class AimDirection {
  constructor(){this.vector=[2,0,0];}
  set(point,origin){const next=point.map((v,i)=>v-origin[i]);if(Math.hypot(...next)>0.001)this.vector=next;}
  target(origin){return origin.map((v,i)=>v+this.vector[i]);}
}
