// Evaluators for Unity 2017 serialized ParticleSystem curves and gradients.
// These preserve the authored curve keys and tangent interpolation instead
// of replacing them with hand-authored timelines.
const clamp01 = value => Math.max(0, Math.min(1, value));

function wrapTime(time, first, last, mode) {
  const duration = last - first;
  if (!(duration > 0) || (time >= first && time <= last)) return time;
  if (mode === 2) {
    const offset = ((time - first) % duration + duration) % duration;
    return first + offset;
  }
  if (mode === 4) {
    const offset = ((time - first) % (duration * 2) + duration * 2) % (duration * 2);
    return first + (offset <= duration ? offset : duration * 2 - offset);
  }
  return time < first ? first : last;
}

/** Evaluate a recovered AnimationCurve with Unity's cubic Hermite key tangents. */
export function evaluateUnityCurve(curve, time) {
  const keys = curve?.m_Curve;
  if (!Array.isArray(keys) || keys.length === 0) return 0;
  if (keys.length === 1) return keys[0].value;
  if (!Number.isFinite(time)) throw new RangeError('Unity curve time must be finite');
  const first = keys[0], last = keys[keys.length - 1];
  const t = wrapTime(time, first.time, last.time,
    time < first.time ? curve.m_PreInfinity : curve.m_PostInfinity);
  if (t <= first.time) return first.value;
  if (t >= last.time) return last.value;
  let right = 1;
  while (right < keys.length && keys[right].time < t) right++;
  const a = keys[right - 1], b = keys[right], span = b.time - a.time;
  if (!(span > 0)) return b.value;
  const u = (t - a.time) / span, u2 = u * u, u3 = u2 * u;
  const h00 = 2 * u3 - 3 * u2 + 1;
  const h10 = u3 - 2 * u2 + u;
  const h01 = -2 * u3 + 3 * u2;
  const h11 = u3 - u2;
  return h00 * a.value + h10 * span * a.outSlope + h01 * b.value + h11 * span * b.inSlope;
}

/** Evaluate Unity MinMaxCurve's four serialized modes using a stable random sample [0,1]. */
export function evaluateUnityMinMaxCurve(value, time, random = 0) {
  if (!Number.isFinite(random)) throw new RangeError('Unity curve random sample must be finite');
  const r = clamp01(random);
  switch (value?.minMaxState) {
    case 0: return value.scalar;
    case 1: return evaluateUnityCurve(value.maxCurve, time) * value.scalar;
    case 2: return evaluateUnityCurve(value.minCurve, time) * (1 - r) + evaluateUnityCurve(value.maxCurve, time) * r;
    case 3: return value.minScalar * (1 - r) + value.scalar * r;
    default: throw new RangeError('Unsupported Unity MinMaxCurve mode: ' + value?.minMaxState);
  }
}

function keysFromGradient(gradient, prefix) {
  const countName = prefix === 'a' ? 'm_NumAlphaKeys' : 'm_NumColorKeys';
  const count = gradient[countName];
  if (!Number.isInteger(count) || count < 1 || count > 8) throw new RangeError('Invalid Unity gradient key count');
  const keys = [];
  for (let i = 0; i < count; i++) {
    const time = gradient[`${prefix}time${i}`] / 65535;
    const value = gradient[`key${i}`];
    if (!Number.isFinite(time) || !value) throw new RangeError('Invalid Unity gradient key');
    keys.push({ time, value });
  }
  return keys.sort((a, b) => a.time - b.time);
}

function lerp(a, b, t) { return a + (b - a) * t; }

function evaluateColorKeys(keys, time, channels) {
  const t = clamp01(time);
  if (t <= keys[0].time) return keys[0].value;
  if (t >= keys.at(-1).time) return keys.at(-1).value;
  let i = 1;
  while (keys[i].time < t) i++;
  const a = keys[i - 1], b = keys[i], u = (t - a.time) / (b.time - a.time);
  return Object.fromEntries(channels.map(k => [k, lerp(a.value[k], b.value[k], u)]));
}

/** Evaluate a serialized Unity Gradient's authored color and alpha key sets. */
export function evaluateUnityGradient(gradient, time) {
  if (!Number.isFinite(time)) throw new RangeError('Unity gradient time must be finite');
  const colors = evaluateColorKeys(keysFromGradient(gradient, 'c'), time, ['r', 'g', 'b']);
  const alphas = keysFromGradient(gradient, 'a');
  const alpha = evaluateColorKeys(alphas, time, ['a']).a;
  return { ...colors, a: alpha };
}

/** Evaluate the source ColorModule's constant, gradient, two-color, or two-gradient form. */
export function evaluateUnityMinMaxGradient(value, time, random = 0) {
  const r = clamp01(random);
  switch (value?.minMaxState) {
    case 0: return { ...value.maxColor };
    case 1: return evaluateUnityGradient(value.maxGradient, time);
    case 2: {
      const a = evaluateUnityGradient(value.minGradient, time), b = evaluateUnityGradient(value.maxGradient, time);
      return Object.fromEntries(['r', 'g', 'b', 'a'].map(k => [k, lerp(a[k], b[k], r)]));
    }
    case 3: {
      const a = value.minColor, b = value.maxColor;
      return Object.fromEntries(['r', 'g', 'b', 'a'].map(k => [k, lerp(a[k], b[k], r)]));
    }
    default: throw new RangeError('Unsupported Unity MinMaxGradient mode: ' + value?.minMaxState);
  }
}

/** Sample the recovered initial and over-lifetime fields for one source particle. */
export function evaluateSourceParticleFields(emitter, normalizedAge, random = {}) {
  if (!Number.isFinite(normalizedAge)) throw new RangeError('Particle age must be finite');
  const age = clamp01(normalizedAge), initial = emitter?.enabledModules?.InitialModule;
  if (!initial?.enabled) throw new RangeError('Source emitter InitialModule is disabled or missing');
  const sample = name => clamp01(Number.isFinite(random[name]) ? random[name] : .5);
  const sizeStart = evaluateUnityMinMaxCurve(initial.startSize, 0, sample('startSize'));
  const speed = evaluateUnityMinMaxCurve(initial.startSpeed, 0, sample('startSpeed'));
  const lifetime = evaluateUnityMinMaxCurve(initial.startLifetime, 0, sample('startLifetime'));
  const colorModule = emitter.enabledModules.ColorModule;
  const sourceColor = colorModule?.enabled
    ? evaluateUnityMinMaxGradient(colorModule.gradient, age, sample('color'))
    : evaluateUnityMinMaxGradient(initial.startColor, 0, sample('color'));
  const sizeModule = emitter.enabledModules.SizeModule;
  const sizeMultiplier = sizeModule?.enabled
    ? evaluateUnityMinMaxCurve(sizeModule.curve, age, sample('sizeOverLifetime')) : 1;
  const rotationModule = emitter.enabledModules.RotationModule;
  const rotationMultiplier = rotationModule?.enabled
    ? evaluateUnityMinMaxCurve(rotationModule.curve, age, sample('rotationOverLifetime')) : 1;
  return {speed,lifetime,size:sizeStart*sizeMultiplier,color:sourceColor,
    rotation: evaluateUnityMinMaxCurve(initial.startRotation,0,sample('startRotation'))*rotationMultiplier};
}

/** Return the authored initial inherited-velocity multiplier, or zero when disabled. */
export function evaluateSourceInheritedVelocity(emitter, normalizedAge, random = 0) {
  const module = emitter?.enabledModules?.InheritVelocityModule;
  if (!module || !module.enabled) return 0;
  if (module.m_Mode !== 0)
    throw new RangeError('Unsupported Unity InheritVelocity mode: ' + module.m_Mode);
  return evaluateUnityMinMaxCurve(module.m_Curve, normalizedAge, random);
}

/** Apply the recovered SingleEmissionEffect count/speed/lifetime rules to a material impact event. */
export function evaluateOriginalImpactEmission(controller,impact,phase,settings={}) {
  const f=controller?.serializedFields||controller;
  if(!f||(controller.scriptGuid||f.m_Script?.guid)!=='7f9bb5f5ef1d9fb3ca6dbc9296820005')
    throw new RangeError('Original SingleEmissionEffect controller is missing');
  const number=(name,value)=>{if(!Number.isFinite(value))throw new RangeError('Impact emission input is invalid: '+name);return value;};
  const velocity=number('Velocity',impact?.Velocity), energy=number('energy',impact?.energy);
  const scale=number('particleCountScaler',impact?.particleCountScaler);
  const modifier=number('emissionsModifier',settings.emissionsModifier??1);
  const volumeVelocity=number('volumeVelocity',f.volumeVelocity);
  if(volumeVelocity<=0||scale<0||modifier<0)throw new RangeError('Original emission scale inputs are out of range');
  let count, speedRange, sizeMultiplier=1;
  const velocityFactor=clamp01(velocity/volumeVelocity);
  if(phase==='entry'){
    const diameter=number('metricDiameter',impact.metricDiameter);
    count=Math.floor(energy*diameter*number('entryRatio',f.entryRatio)*velocityFactor*modifier*scale);
    const maxSpeed=velocity*number('entryVelocityRatio',f.entryVelocityRatio);
    speedRange=[maxSpeed*.25,maxSpeed];
  } else if(phase==='exit') {
    const displaced=number('displacedMaterialVolume',impact.displacedMaterialVolume);
    count=Math.floor(displaced*number('volumeRatio',f.volumeRatio)*velocityFactor*modifier*scale);
    const maxSpeed=velocity*number('exitVelocityRatio',f.exitVelocityRatio);
    speedRange=[0,maxSpeed];
    if(f.chunkEmission){
      const diameter=number('projectileDiameter',impact.projectileDiameter);
      const size=clamp01((diameter-4)/26)*1+0.75;
      sizeMultiplier=size*number('chunkSizeMultiplier',settings.chunkSizeMultiplier??1);
      count=Math.ceil(count/sizeMultiplier);
    }
    count=Math.min(count,500);
  } else throw new RangeError('Unsupported original impact emission phase: '+phase);
  const lifetime=f.dontUseGlobalLifetime
    ? number('lifeTime',f.lifeTime)
    : number('particlesLifetime',settings.particlesLifetime);
  return {count:Math.max(0,count),speedRange,lifetime,sizeMultiplier,
    randomRotation:!!f.randomRotation,randomRotationMagnitude:f.randomRotationMagnitude};
}
