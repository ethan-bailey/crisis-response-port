// Original M4 magazine touch control. Uses the recovered Magazine AR 30 round
// prefab texture; the x-ray rounds are CSS HUD marks, NOT substitute game ammo.
// Magazine count, firing and reload come exclusively from StepWeapons() output.
export const ORIGINAL_M4_MAGAZINE_TEXTURE = "./assets/weapon-runtime/M4-magazine/textures/material_00.png";
export const ORIGINAL_M4_MAGAZINE_CAPACITY = 30; // Magazine AR 30 round source prefab
// Only original, recovered game artwork is permitted here. Glock currently has
// a gun prefab texture but NO separately extracted matching magazine sprite.
// For that equipped gun show its own original art, not a fake M4 magazine.
// Future playable weapons must provide their actual original source identity,
// magazine capacity and recovered sprite before a magazine cutaway is displayed.
export const SOURCE_WEAPON_VISUALS = Object.freeze({
  M4A1: Object.freeze({
    label: "M4A1",
    texture: ORIGINAL_M4_MAGAZINE_TEXTURE,
    magazineSprite: true, caliber: "5.56"
  }),
  "Glock-18": Object.freeze({
    label: "Glock-18",
    texture: "./assets/weapons/Glock-18/textures/material_00.png",
    magazineSprite: false, caliber: "9mm"
  })
});
export const X_RAY_SLOTS = 10;
export const DROP_DISTANCE_CSS_PX = 45;

export function magazineView(count, capacity = ORIGINAL_M4_MAGAZINE_CAPACITY, slots = X_RAY_SLOTS, chamberLive = false, weaponLabel = "M4A1") {
  if (!Number.isInteger(capacity) || capacity <= 0 || !Number.isInteger(slots) || slots <= 0)
    throw new RangeError("Original magazine capacity and HUD slots must be positive integers");
  const chamber = chamberLive === true ? 1 : 0;
  if (!Number.isInteger(count) || count < 0 || count > capacity)
    return {known:false, count:null, capacity, visible:0, chamber:0, ready:null,
      label:`${weaponLabel} ammunition unavailable`};
  const ready = count + chamber;
  return {
    known:true, count, capacity, chamber, ready,
    // Show EACH of the final 10 magazine cartridges one-to-one. Above 10,
    // the cutaway is full; the exact numeric total remains authoritative.
    visible: Math.min(count, slots),
    label:`${weaponLabel}: ${count} in magazine, ${chamber} chambered; ${ready} ready`
  };
}

export function magazinePresentation(state) {
  // Never use the M4 magazine asset for an unknown or different equipped gun.
  // The source C# runtime currently equips M4A1 only; Glock-18 is a scene
  // preview until its original weapon mechanics are ported.
  const weaponId = typeof state?.weaponId === "string" ? state.weaponId : null;
  const art = weaponId ? SOURCE_WEAPON_VISUALS[weaponId] : null;
  const capacity = Number.isInteger(state?.magazineCapacity) && state.magazineCapacity > 0
    ? state.magazineCapacity
    : weaponId === "M4A1" ? ORIGINAL_M4_MAGAZINE_CAPACITY : null;
  const view = capacity && weaponId
    ? magazineView(state?.magazine, capacity, X_RAY_SLOTS, state?.chamberLive, art?.label ?? weaponId)
    : {known:false,count:null,capacity:null,visible:0,chamber:0,ready:null,
       label:`${weaponId ?? "Equipped weapon"} ammunition unavailable`};
  return {weaponId, art:art ?? null, ...view};
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

export function createEquippedMagazineHud(element,{onReload}){
  if(!element || typeof onReload!=="function")throw new TypeError("Magazine control needs an element and source-action callback");
  const image=element.querySelector("[data-mag-image]");
  const rack=element.querySelector("[data-mag-rounds]");
  const counter=element.querySelector("[data-mag-count]");
  if(!image || !rack || !counter)throw new TypeError("Equipped magazine HUD elements missing");
  const rounds=[];
  for(let i=0;i<X_RAY_SLOTS;i++){
    const round=rack.ownerDocument.createElement("span");
    round.className="mag-round";
    round.setAttribute("aria-hidden","true");
    rack.appendChild(round);
    rounds.push(round);
  }
  let lastReady=null, lastWeapon=null, ready=false,reloading=false;
  const gesture=bindMagazineDrag(element,onReload,{
    canDrop:()=>ready&&!reloading,
    onPull:distance=>element.style.setProperty("--mag-pull",`${distance}px`)
  });
  const render=state=>{
    const v=magazinePresentation(state);
    ready=v.known;
    reloading=state?.reloading===true;
    const originalArt=v.art;
    const originalTexture=originalArt?.texture;
    if(originalTexture && image.getAttribute("src")!==originalTexture)image.src=originalTexture;
    image.hidden=!originalTexture;
    // Only render an x-ray cutaway on a verified original magazine sprite;
    // never pretend the Glock pistol frame is an M4 magazine.
    rack.hidden=!originalArt?.magazineSprite;
    element.classList.toggle("mag-weapon-only",!!originalArt&&!originalArt.magazineSprite);
    element.classList.toggle("mag-unavailable",!ready);
    element.classList.toggle("mag-reloading",reloading);
    const switched=lastWeapon!==null && lastWeapon!==v.weaponId;
    if(switched)gesture.reset();
    element.setAttribute("aria-label", v.label + (reloading ? ". Reloading" : ". Drag down to reload"));
    // This number means rounds READY TO FIRE (chamber + inserted magazine).
    // No separate chamber icon: the number includes the chambered round.
    counter.textContent=v.known?String(v.ready):"–";
    for(let i=0;i<rounds.length;i++)rounds[i].hidden=i>=v.visible;
    const fired=state?.events?.some(e=>e.type==="shot")===true ||
      (!switched && lastReady!==null && v.ready!==null && v.ready<lastReady && !reloading);
    if(fired){
      element.classList.remove("mag-fired");
      // Restart original-fire-linked UI animation even during automatic fire.
      void element.offsetWidth;
      element.classList.add("mag-fired");
    }
    lastReady=v.ready;
    lastWeapon=v.weaponId;
  };
  const finish=()=>element.classList.remove("mag-fired");
  element.addEventListener("animationend",finish);
  render(null);
  return {render,dispose:()=>{gesture.dispose();element.removeEventListener("animationend",finish);},resetGesture:gesture.reset};
}

// Older call sites can migrate without breaking their existing browser modules.
export const createOriginalM4MagazineHud = createEquippedMagazineHud;
