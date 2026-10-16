// Original M4 magazine touch control. Uses the recovered Magazine AR 30 round
// prefab texture; the x-ray rounds are CSS HUD marks, NOT substitute game ammo.
// Magazine count, firing and reload come exclusively from StepWeapons() output.
export const ORIGINAL_M4_MAGAZINE_TEXTURE = "./assets/weapon-runtime/M4-magazine/textures/material_00.png";
export const ORIGINAL_M4_MAGAZINE_CAPACITY = 30; // Magazine AR 30 round source prefab
export const X_RAY_SLOTS = 10;
export const DROP_DISTANCE_CSS_PX = 45;

export function magazineView(count, capacity = ORIGINAL_M4_MAGAZINE_CAPACITY, slots = X_RAY_SLOTS) {
  if (!Number.isInteger(capacity) || capacity <= 0 || !Number.isInteger(slots) || slots <= 0)
    throw new RangeError("Original magazine capacity and HUD slots must be positive integers");
  if (!Number.isInteger(count) || count < 0 || count > capacity)
    return {known:false, count:null, capacity, visible:0, label:"Magazine ammunition unavailable"};
  return {
    known:true, count, capacity,
    visible: count === 0 ? 0 : Math.ceil(count / capacity * slots),
    label:`M4 magazine: ${count} of ${capacity} rounds`
  };
}

export function isDropGesture(dx, dy, threshold = DROP_DISTANCE_CSS_PX) {
  return Number.isFinite(dx) && Number.isFinite(dy) &&
    dy >= threshold && dy >= Math.abs(dx) * 1.2;
}

// Uses its own pointer ID/capture: unrelated aim/move/fire fingers are unaffected.
// Only a completed downwards drag is the source magazine-action input; a simple
// tap, short drag or pointer cancel never initiates reload.
export function bindMagazineDrag(element, onDrop, {threshold=DROP_DISTANCE_CSS_PX, canDrop=()=>true, onPull=()=>{}}={}) {
  let pointer=null;
  const reset = (id, type) => {
    if (!pointer || id !== pointer.id) return;
    const old=pointer;
    pointer=null;
    onPull(0);
    if (element.hasPointerCapture?.(id)) element.releasePointerCapture(id);
    if (type==="up" && isDropGesture(old.x-old.startX, old.y-old.startY, threshold) && canDrop()) onDrop();
  };
  const down=e=>{
    if (pointer || (e.pointerType==="mouse" && e.button!==0) || !canDrop()) return;
    pointer={id:e.pointerId,startX:e.clientX,startY:e.clientY,x:e.clientX,y:e.clientY};
    element.setPointerCapture?.(e.pointerId);
    e.preventDefault?.();
  };
  const move=e=>{
    if (!pointer || e.pointerId!==pointer.id) return;
    pointer.x=e.clientX;
    pointer.y=e.clientY;
    onPull(Math.max(0,Math.min(80,e.clientY-pointer.startY)));
    e.preventDefault?.();
  };
  const up=e=>{
    if (!pointer || e.pointerId!==pointer.id) return;
    pointer.x=e.clientX;pointer.y=e.clientY;
    reset(e.pointerId,"up");
    e.preventDefault?.();
  };
  const cancel=e=>reset(e.pointerId,"cancel");
  const lost=e=>reset(e.pointerId,"cancel");
  element.addEventListener("pointerdown",down);
  element.addEventListener("pointermove",move);
  element.addEventListener("pointerup",up);
  element.addEventListener("pointercancel",cancel);
  element.addEventListener("lostpointercapture",lost);
  return {
    reset:()=>{if(pointer)reset(pointer.id,"cancel");},
    dispose:()=>{
      if(pointer)reset(pointer.id,"cancel");
      element.removeEventListener("pointerdown",down);
      element.removeEventListener("pointermove",move);
      element.removeEventListener("pointerup",up);
      element.removeEventListener("pointercancel",cancel);
      element.removeEventListener("lostpointercapture",lost);
    }
  };
}

export function createOriginalM4MagazineHud(element,{onReload}){
  if(!element || typeof onReload!=="function")throw new TypeError("Magazine control needs an element and source-action callback");
  const image=element.querySelector("[data-mag-image]");
  const rack=element.querySelector("[data-mag-rounds]");
  const counter=element.querySelector("[data-mag-count]");
  if(!image || !rack || !counter)throw new TypeError("Original M4 magazine HUD elements missing");
  image.src=ORIGINAL_M4_MAGAZINE_TEXTURE;
  const rounds=[];
  for(let i=0;i<X_RAY_SLOTS;i++){
    const round=rack.ownerDocument.createElement("span");
    round.className="mag-round";
    round.setAttribute("aria-hidden","true");
    rack.appendChild(round);
    rounds.push(round);
  }
  let lastCount=null,ready=false,reloading=false;
  const gesture=bindMagazineDrag(element,onReload,{
    canDrop:()=>ready&&!reloading,
    onPull:distance=>element.style.setProperty("--mag-pull",`${distance}px`)
  });
  const render=state=>{
    const v=magazineView(state?.magazine);
    ready=v.known;
    reloading=state?.reloading===true;
    element.classList.toggle("mag-unavailable",!ready);
    element.classList.toggle("mag-reloading",reloading);
    element.setAttribute("aria-label", v.label + (reloading ? ". Reloading" : ". Drag down to reload"));
    counter.textContent=v.known?String(v.count):"–";
    for(let i=0;i<rounds.length;i++)rounds[i].hidden=i>=v.visible;
    const fired=state?.events?.some(e=>e.type==="shot")===true ||
      (lastCount!==null && v.count!==null && v.count<lastCount && !reloading);
    if(fired){
      element.classList.remove("mag-fired");
      // Restart original-fire-linked UI animation even during automatic fire.
      void element.offsetWidth;
      element.classList.add("mag-fired");
    }
    lastCount=v.count;
  };
  const finish=()=>element.classList.remove("mag-fired");
  element.addEventListener("animationend",finish);
  render(null);
  return {render,dispose:()=>{gesture.dispose();element.removeEventListener("animationend",finish);},resetGesture:gesture.reset};
}
