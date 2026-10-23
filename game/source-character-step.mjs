// Browser-side transport adapters for the original CharacterBody.FixedUpdate
// and UpdateAnimation formulas. These keep the source formulas callable while
// the deferred RuntimeBridge methods are absent from the deployed WASM build.
export function advanceOriginalKneelRatio(ratio, kneeling, deltaTime) {
  const target = kneeling ? 1 : 0;
  const difference = target - ratio;
  const rate = 5 * Math.min(1, Math.max(0.025, Math.abs(difference)));
  return ratio + Math.sign(difference) * deltaTime * rate;
}

export function stepBodyMovement({
  direction, velocity, step, facing, walkSpeed, sprintSpeed,
  analogMagnitude = 1, sprint = false, crippledRatio = 0,
  operator = true, noPlayerLimp = true, precision = false,
}) {
  const sign = Math.sign(direction);
  const reversing = sign !== facing;
  const crippled = crippledRatio > 0.25 && (!operator || !noPlayerLimp);
  let speedLimit = walkSpeed * (crippled ? 0.8 : 1);
  if (sprint && !reversing && !crippled) speedLimit = sprintSpeed * (precision ? 0.5 : 1);
  const speed = Math.abs(velocity);
  if (speed > speedLimit) return { forceX: 0, reset: false, speedLimit };
  return {
    forceX: (speedLimit - speed) * sign * step * 750 * analogMagnitude,
    reset: true,
    speedLimit,
  };
}

export function stepCharacterAnimation({
  kneelRatio, velocity, direction,
  walkSpeed, sprintSpeed, crippledRatio = 0, operator = true,
}) {
  // This browser physics adapter exposes fixed-step positions without Unity's
  // Rigidbody interpolation. Use the last known-good velocity predicate so
  // render frames between fixed steps do not alternate walk and idle states.
  const moving = Math.abs(velocity) > 0.15;
  const speed = Math.abs(velocity);
  const moveSpeed = speed < walkSpeed
    ? 0
    : Math.min(1, Math.max(0, (speed - walkSpeed) / (sprintSpeed - walkSpeed))) * 0.5;
  return {
    moving,
    backwards: moving && Math.sign(velocity) !== direction,
    moveSpeed,
    KneelRatio: kneelRatio,
    crippled: operator ? 0 : crippledRatio,
    ragdoll: false,
  };
}
