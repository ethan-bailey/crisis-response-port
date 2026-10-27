const sourceArmJoint = /(?:^|\/)(?:Left|Right) (?:upper arm|lower arm|hand)$/i;

export function extractSourceArmPoses(snapshot) {
  if (!Array.isArray(snapshot?.poses)) return [];
  return snapshot.poses
    .filter(pose => typeof pose?.path === "string" && sourceArmJoint.test(pose.path))
    .map(({ path, p, q }) => ({ path, position: [...p], rotation: [...q] }));
}

function distance(a, b) {
  if (!Array.isArray(a) || !Array.isArray(b) || a.length !== b.length) return null;
  const values = a.map((value, index) => value - b[index]);
  if (!values.every(Number.isFinite)) return null;
  return Math.hypot(...values);
}

function rotationDelta(a, b) {
  if (!Array.isArray(a) || !Array.isArray(b) || a.length !== 4 || b.length !== 4) return null;
  const normA = Math.hypot(...a), normB = Math.hypot(...b);
  if (!(normA > 0) || !(normB > 0) || !Number.isFinite(normA + normB)) return null;
  const dot = Math.abs(a.reduce((sum, value, index) => sum + value / normA * (b[index] / normB), 0));
  return 1 - Math.min(1, dot);
}

export function compareSourceArmPoses(reference, sample) {
  const byPath = new Map((reference ?? []).map(pose => [pose.path, pose]));
  const deltas = [];
  for (const pose of sample ?? []) {
    const before = byPath.get(pose.path);
    if (!before) continue;
    const position = distance(before.position, pose.position);
    const rotation = rotationDelta(before.rotation, pose.rotation);
    if (position === null || rotation === null) continue;
    deltas.push({ path: pose.path, position, rotation, change: Math.max(position, rotation) });
  }
  return {
    matchedPaths: deltas.map(item => item.path),
    maximum: deltas.reduce((value, item) => Math.max(value, item.change), 0),
    deltas,
  };
}
