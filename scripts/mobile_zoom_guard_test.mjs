import assert from "node:assert/strict";
import { test } from "node:test";
import { bindMobileZoomGuard } from "../game/mobile-zoom-guard.mjs";

function harness() {
  const handlers = new Map();
  const document = {
    addEventListener(type, handler) { handlers.set(type, handler); },
    removeEventListener(type) { handlers.delete(type); }
  };
  let now = 0;
  const dispose = bindMobileZoomGuard(document, () => now);
  const dispatch = (type, selector) => {
    let prevented = false;
    handlers.get(type)({
      target: { nodeType: 1, closest: query => query.includes(selector) ? {} : null },
      preventDefault() { prevented = true; }
    });
    return prevented;
  };
  return { dispatch, setNow(value) { now = value; }, dispose, handlers };
}

test("rapid taps across the game surface prevent Safari double-tap zoom", () => {
  const h = harness();
  assert.equal(h.dispatch("touchend", "#stage"), false, "first tap keeps normal input");
  h.setNow(180);
  assert.equal(h.dispatch("touchend", "#controls"), true, "rapid second tap is cancelled");
  h.setNow(700);
  assert.equal(h.dispatch("touchend", "#controls"), false, "a deliberate later tap remains usable");
});

test("zoom guards do not interfere with error text selection or scroll", () => {
  const h = harness();
  assert.equal(h.dispatch("touchend", "#errorDetails"), false);
  assert.equal(h.dispatch("gesturestart", "#errorDetails"), false);
});

test("iOS pinch gesture zoom is blocked only over game controls", () => {
  const h = harness();
  assert.equal(h.dispatch("gesturestart", "#movementStick"), true);
  assert.equal(h.dispatch("gesturestart", "#errorDetails"), false);
  h.dispose();
  assert.equal(h.handlers.size, 0, "disposal removes both document listeners");
});
