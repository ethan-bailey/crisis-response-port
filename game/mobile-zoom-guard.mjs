const GAME_SURFACE = "#stage, #controls, #movementStick, #magazineTouch, #startOriginalMission, #missionProgress, #controlsHelp";

// iOS Safari can still double-tap zoom around touch controls even when the
// viewport requests a fixed scale. Keep the guard on game surfaces so native
// text selection and scrolling in the error report remain available.
export function bindMobileZoomGuard(documentRef = globalThis.document, clock = () => Date.now()) {
  if (!documentRef?.addEventListener) throw new TypeError("A document event target is required");
  let lastTapAt = -Infinity;
  const onTouchEnd = event => {
    const target = event.target?.nodeType === 1 ? event.target : event.target?.parentElement;
    if (!target?.closest?.(GAME_SURFACE)) return;
    const now = clock();
    if (now - lastTapAt >= 0 && now - lastTapAt < 400) event.preventDefault();
    lastTapAt = now;
  };
  const onGestureStart = event => {
    const target = event.target?.nodeType === 1 ? event.target : event.target?.parentElement;
    if (target?.closest?.(GAME_SURFACE)) event.preventDefault();
  };
  documentRef.addEventListener("touchend", onTouchEnd, { passive: false });
  documentRef.addEventListener("gesturestart", onGestureStart, { passive: false });
  return () => {
    documentRef.removeEventListener("touchend", onTouchEnd);
    documentRef.removeEventListener("gesturestart", onGestureStart);
  };
}
