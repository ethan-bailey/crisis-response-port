const finite = value => Number.isFinite(value);
const fileId = value => value && typeof value === 'object' ? String(value.fileID ?? value.fileId ?? '') : String(value ?? '');
const idField = value => typeof value === 'string' ? value : fileId(value);
const dbGain = value => value <= -80 ? 0 : 10 ** (value / 20);

/** Replays recovered Unity mixer groups and authored snapshot transitions. */
export function createSourceAudioMixer() {
  let groups = new Map(), snapshots = new Map(), parents = new Map(), active = null, mixerGuid = null;
  let configurationLoaded = false;
  const stats = { configurationErrors: 0, transitions: 0, unknownTransitions: 0, unknownGroups: 0 };

  function loadConfiguration(configuration) {
    if (!configuration) return false;
    if (!Array.isArray(configuration.groups) || !Array.isArray(configuration.snapshots) || !Array.isArray(configuration.mixers)) {
      stats.configurationErrors++;
      return false;
    }
    if ((Number.isInteger(configuration.expectedGroupCount) && configuration.groups.length !== configuration.expectedGroupCount) ||
        (Number.isInteger(configuration.expectedSnapshotCount) && configuration.snapshots.length !== configuration.expectedSnapshotCount)) {
      stats.configurationErrors++;
      return false;
    }
    if (configuration.mixers.length !== 1 || typeof configuration.mixers[0]?.guid !== 'string') {
      stats.configurationErrors++;
      return false;
    }
    const nextMixerGuid = configuration.mixers[0].guid;
    const nextGroups = new Map(), nextSnapshots = new Map(), nextParents = new Map();
    for (const group of configuration.groups) {
      const id = String(group?.fileID ?? '');
      const fields = group?.fields;
      if (!id || group.guid !== nextMixerGuid || typeof fields?.m_GroupID !== 'string' || typeof fields?.m_Volume !== 'string' || typeof fields?.m_Pitch !== 'string' || !Array.isArray(fields?.m_Children)) {
        stats.configurationErrors++;
        return false;
      }
      nextGroups.set(id, { ...group, fields });
      for (const child of fields.m_Children) nextParents.set(fileId(child), id);
    }
    for (const snapshot of configuration.snapshots) {
      const id = String(snapshot?.fileID ?? ''), values = snapshot?.fields?.m_FloatValues;
      if (!id || snapshot.guid !== nextMixerGuid || !values || typeof values !== 'object' || Array.isArray(values)) {
        stats.configurationErrors++;
        return false;
      }
      nextSnapshots.set(id, { ...snapshot, values });
    }
    groups = nextGroups; snapshots = nextSnapshots; parents = nextParents; mixerGuid = nextMixerGuid;
    configurationLoaded = true;
    const mixerFields = configuration.mixers[0]?.fields ?? {};
    const initialId = idField(mixerFields.m_TargetSnapshot) || idField(mixerFields.m_StartSnapshot);
    const initial = snapshots.get(initialId) ?? snapshots.values().next().value;
    if (initial) active = { from: { ...initial.values }, to: { ...initial.values }, start: 0, duration: 0 };
    return true;
  }

  function valueAt(time) {
    if (!active) return {};
    if (!(active.duration > 0) || time >= active.start + active.duration) return active.to;
    const t = Math.max(0, (time - active.start) / active.duration), result = {};
    for (const key of new Set([...Object.keys(active.from), ...Object.keys(active.to)])) {
      const from = active.from[key] ?? active.to[key] ?? 0, to = active.to[key] ?? from;
      result[key] = from + (to - from) * t;
    }
    return result;
  }

  function applyTransition(event) {
    if (event?.action !== 'transition') return;
    const destination = snapshots.get(String(event.snapshotFileID));
    const target = event.snapshotFields?.m_FloatValues ?? destination?.values;
    if (!target || typeof target !== 'object' || !finite(event.time) || !finite(event.duration) || event.duration < 0) {
      stats.unknownTransitions++;
      return;
    }
    active = { from: valueAt(event.time), to: { ...target }, start: event.time, duration: event.duration };
    stats.transitions++;
  }

  function gainAndPitch(groupAssetGuid, groupId, time) {
    if (!groupId) return { gain: 1, pitch: 1 };
    if (groupAssetGuid !== mixerGuid) { stats.unknownGroups++; return null; }
    let id = String(groupId), gain = 1, pitch = 1, traversed = 0;
    const values = valueAt(time);
    while (id && traversed < groups.size) {
      traversed++;
      const group = groups.get(id);
      if (!group) { stats.unknownGroups++; return null; }
      const fields = group.fields;
      if (fields.m_Mute === 1 || fields.m_Mute === true) return { gain: 0, pitch };
      const db = values[fields.m_Volume], semitonePitch = values[fields.m_Pitch];
      if (finite(db)) gain *= dbGain(db);
      if (finite(semitonePitch)) pitch *= Math.max(0, semitonePitch);
      id = parents.get(id);
    }
    if (id) { stats.configurationErrors++; return null; }
    return { gain: Math.max(0, gain), pitch };
  }

  return {
    update(configuration, events, sourceTime) {
      if (configuration && !configurationLoaded) loadConfiguration(configuration);
      for (const event of events ?? []) applyTransition(event);
      return { loaded: configurationLoaded, transitions: stats.transitions, configurationErrors: stats.configurationErrors };
    },
    gainFor(groupAssetGuid, groupFileID, sourceTime) { return gainAndPitch(groupAssetGuid, groupFileID, sourceTime); },
    snapshot(sourceTime = 0) { return { ...stats, loaded: configurationLoaded, groupCount: groups.size, snapshotCount: snapshots.size, values: valueAt(sourceTime) }; }
  };
}
