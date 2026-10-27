# Browser vertical-slice plan — 2026-10-14

## Goal

Find the earliest real, testable browser build that exercises original Crisis Response code and assets. This is a planning artifact based on static inspection; it does not claim either scene currently builds or runs.

## Recommended sequence

### Slice 0 — boot and scene flow

Start with the original enabled build scenes and preserve the existing order:
1. Disclaimer
2. Title
3. Transition
4. Mission 01
5. Marksman range
6. Benchmark

First build gate: get a player to load and render without modifying gameplay. Record missing scripts, shader errors, asset/import errors, and first exception before attempting touch changes.

### Slice 1 — Marksman range

Use `Assets/Scenes/Marksman range.unity` as the first gameplay-focused compatibility test. Its serialized scene is materially smaller than the Title and Mission 01 scenes (roughly 3,160 Unity object records versus 14,066 and 7,141 respectively, counted by YAML object headers; this is a rough comparison, not a memory measurement).

Validate:
- scene transition into and out of the range;
- camera and cursor behavior;
- weapon presentation and aiming;
- fire, reload, bolt, and other weapon interactions supported by the scene;
- physics, audio, particles, and UI;
- touch mapping on iPhone Safari.

This slice is useful for isolating rendering/input/weapon compatibility. It is **not** enough to claim mission parity.

### Slice 2 — Mission 01

Only after Slice 1 works, test `Assets/Scenes/Mission 01.unity` for the broader gameplay loop:
- player movement and collision;
- team/AI behaviors;
- doors and interactions;
- projectiles, damage, and ballistics;
- mission completion/failure and score;
- save/load and resume behavior.

Capture deterministic scenarios where possible and compare the recovered build with observed behavior from the original Windows game. Do not mark behavior verified based only on the presence of scripts or scene objects.

## Browser-specific gating

Before calling a slice playable on iPhone:
- use HTTPS hosting;
- verify all required network requests are optional or use an HTTPS/CORS-compatible endpoint;
- ensure no Windows registry or arbitrary desktop path operation is called;
- verify settings persistence after reload;
- confirm controls work without pointer lock or a physical keyboard/mouse;
- test landscape orientation, safe areas, Safari browser chrome, audio unlock, tab backgrounding, and reload;
- measure actual startup time, memory behavior, and frame rate on the target device.

## Evidence discipline

For each gate record:
- exact project/editor version;
- source commit and artifact identifier;
- build log and output artifact;
- device/browser/OS used;
- steps executed and result;
- known parity gaps and regressions.

A successful build is not a successful port. A playable Marksman range is a milestone, not full-game completion.
