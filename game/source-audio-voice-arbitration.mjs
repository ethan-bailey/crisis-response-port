// Unity 2018.4-compatible selection contract for source voices. The caller
// supplies audibility after it has applied the source's current spatial and
// mixer state; this module deliberately does not guess those missing inputs.
export function rankSourceAudioVoices(candidates) {
  if (!Array.isArray(candidates)) throw new TypeError('Audio voice candidates must be an array');
  return candidates.map((candidate, index) => {
    const priority = candidate?.priority;
    const audibility = candidate?.audibility;
    const sequence = candidate?.sequence ?? index;
    if (!Number.isInteger(priority) || priority < 0 || priority > 255)
      throw new RangeError(`AudioSource priority must be an integer from 0 to 255: ${priority}`);
    if (!Number.isFinite(audibility) || audibility < 0 || audibility > 1)
      throw new RangeError(`Audio voice audibility must be finite and normalized from 0 to 1: ${audibility}`);
    if (!Number.isFinite(sequence)) throw new TypeError('Audio voice sequence must be finite');
    return { candidate, priority, audibility, sequence, index };
  }).sort((left, right) => left.priority - right.priority ||
      right.audibility - left.audibility || left.sequence - right.sequence || left.index - right.index)
    .map(value => value.candidate);
}

export function planSourceAudioVoices(candidates, realVoiceCount = 64) {
  if (!Number.isInteger(realVoiceCount) || realVoiceCount < 0)
    throw new RangeError('Real voice count must be a non-negative integer');
  const ranked = rankSourceAudioVoices(candidates);
  return {
    real: ranked.slice(0, realVoiceCount),
    virtual: ranked.slice(realVoiceCount),
  };
}
