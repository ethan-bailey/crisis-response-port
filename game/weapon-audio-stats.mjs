// Keep the weapon-effects facade aligned with the production audio runtime.
// Source mixer initialization happens once before the first Mission frame, so
// both startup and per-frame source snapshots must pass through this adapter.
export function createWeaponAudioStats(audio) {
  return {
    snapshot: () => audio.snapshot(),
    updateSourceAudioMixer: (configuration, events, sourceTime) =>
      audio.updateSourceAudioMixer(configuration, events, sourceTime),
    updateSourceAudioStates: (states, sourceTime) =>
      audio.updateSourceAudioStates(states, sourceTime),
  };
}
