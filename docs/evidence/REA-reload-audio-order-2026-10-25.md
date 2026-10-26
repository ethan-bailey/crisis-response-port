# Original reload audio ordering — 2026-10-25

## Source trace

`runtime/Original/Weapons/ChangeMagazine.cs` owns the original magazine-task
state machine. It removes the active magazine in `State1`, inserts the selected
reserve magazine in `State6`, and finishes the task through the original
`OnEnd`/bolt behavior. The unchanged source methods in
`runtime/Original/Weapons/WeaponMechanism.Methods.cs` emit
`audioExtractMagazine` from `RemoveMagazine` and `audioInsertMagazine` from
`InsertMagazine`; `runtime/Original/Weapons/CyclingBolt.cs` emits
`audioBoltReturn` from `Close`. `WeaponEffects.Emit` timestamps these callbacks
with Unity `Time.time` for the browser transport.

## Change and acceptance

`runtime/Checks/WeaponContracts.cs` previously checked only that the three
expected event names occurred somewhere during one reload. It now requires
the original event order—magazine extract, magazine insert, bolt return—and
requires event timestamps to be finite, monotonic, and no later than the frame
snapshot that reports them. This exercises the original source task and audio
callbacks through the runtime weapon host. No sound, callback, or gameplay
behavior was added.

## Verification

- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -c Release`
  passed under pinned SDK **8.0.426** on the worktree based on remote
  `79adcd944225ed7e120e2714eec5c48d171f8a0e`.
- Weapon output: `PASS`, original reload tasks true, 30 magazine rounds after
  reload, source chamber/ammunition checks pass, no infinite ammunition.
- Full runtime output explicitly reports `fullGameParity:false`; this contract
  proves one source reload ordering, not all cancellation/interruption paths or
  audible iPhone playback.
- Log: `/tmp/cpr-reload-check.log`.

## Remaining reload cases

Add source-driven checks for quick/repeated reload, cancel/input interruption,
empty reserve, weapon switch during each reload phase, and correct audio clip
mapping/playback timing in the browser. Keep physical iPhone audio acceptance
pending until the owner tests it.
