# iPhone firing and aiming interaction plan — original-game parity

Date: 2026-10-15. Design note only; **not** a claim that live weapon firing works.

## Recovered evidence

- `runtime/Original/InputMapping.cs`: `fire` maps to `KeyCode.Mouse0`, `fireMode` maps to `KeyCode.T` and has a `longPress` threshold of 0.4 s. `precision` maps to right mouse. Preserve the original binding semantics.
- `runtime/Original/InputKey.cs`, `runtime/Original/InputHandler.cs`: recovered original key edge and held-state logic (`Down`, `Pressed`, `Up`, `UpShort`, `UpLong`, `DownLong`). Do not implement independent browser-mode timing for firing.
- `game/assets/weapons/M4A1/weapon.json`: recovered prefab metadata lists `roundsPerMinute: 900`. `game/assets/weapons/Glock-18/weapon.json`: 1200. These values are **not proof** of which fire modes are available or how the original game schedules/cancels firing. Recover the original weapon/selector logic before claiming semi-auto/full-auto parity.
- `game/browser-input.mjs`: current drag/tap adapter suppresses shooting on drag, hover, cancelled/lost capture, and long holds. `game/tap-fire-queue.mjs` sends distinct raw fire press/release ticks into the original C# input adapter. This currently does not fire a projectile.
- `game/index.html` correctly disables the on-screen FIRE/RELOAD/SWAP controls while original gameplay systems remain unavailable.

## Proposed controls: separate physical gesture from weapon logic

| Gesture | Browser input | Original runtime behavior once ported |
| --- | --- | --- |
| Drag one finger across scene | Change aim point only; retain last body-local aim on release | No trigger activation or fabricated fire |
| Quick tap in scene | One raw trigger down/up pair through original `InputHandler` | Weapon fires according to its *original selected mode*; intended for accurate single shots |
| Press and hold independent FIRE button | Continuous raw `fire.Pressed=true` while held, false on pointer up/cancel | Semi-auto: one round per trigger press. Automatic: original firing cadence continues while held. Burst/other modes: original weapon decides |
| Drag to aim **while another finger holds FIRE** | Two independently captured pointers, no shared ownership | Continue adjusting aim and sending original held-fire state concurrently |
| Press weapon selector button | Original `fireMode` input, with correct short/long press semantics as recovered | Original code chooses mode; UI reads mode from weapon state, not a guessed browser enum |
| Phone background/blur/cancel/resize | Release all held inputs; clear queued tap input | Stop firing immediately. Prevent stuck triggers or resumed auto fire |

This avoids the ambiguous one-finger pattern where a long press could mean either aim or sustained automatic firing. Do **not** assume that holding a finger stationary on the scene should automatically fire. If user later explicitly chooses that option, confirm gesture safety and provide corresponding regression cases.

For a semi-auto weapon, a held trigger must not repeatedly create shots; additional shots require a genuine release and new press, as governed by the recovered weapon logic. For automatic, do not use `setInterval` or a JavaScript ammo counter to synthesize rounds: execute original weapon/bolt/projectile cycle, ammunition state, recoil, safety and timing. Changing fire mode must never silently change the weapon model's behavior.

## Implementation order and gates

1. Recover and execute complete original `PlayerController`, `CharacterBody`, `Firearm`/`WeaponBase`, fire-mode selector, bolt and projectile/magazine chain, with source hashes and runtime compatibility dependencies. No substitute systems.
2. Connect both tap event pulses and the independent held FIRE surface to the **same original `fire` InputKey**. Merge touch sources carefully so lifting one source does not release another. Ensure queued taps do not create extra rounds after switching modes/weapon, suspension or pause.
3. Implement a fire-mode action driven by the recovered `fireMode` binding; preserve its 0.4-second long-press classification where relevant. Display mode and ammo from original state only.
4. Test per-mode edge transitions: tap once, multiple quick taps, sustained hold, hold with dragging, fire cancellation, release outside control, multi-touch, weapon switch, empty magazine, reload, safety, loss of focus and 30/60/120 Hz frame rates. Compare original Unity traces; no synthetic pass from a fake counter.
5. Validate full browser assembly in Chromium and WebKit with CI, then physical iPhone Safari; keep original fire controls marked pending until the end-to-end original chain has been verified.

## ChatGPT Sites as hosting option

There was an earlier private ChatGPT Site named
`https://crisis-response-runtime.craftysnipe13.chatgpt.site`
recorded in PR #7, published from an **older** source commit. This repository
cannot automatically update it. A *new or updated* Site should be created from
ChatGPT **Work** using `@Sites`, or by opening the existing Site's edit conversation.
ChatGPT Sites is in beta and may have runtime/file-size restrictions. Check
browser WebAssembly, `.wasm` MIME, compiled Blazor `/_framework` loading,
same-origin `/game` + `/runtime-web`, recovered game asset volume, and
iPhone Safari behavior before claiming suitability.

Hosting original recovered third-party game assets must remain owner-only/private
unless the user has appropriate rights to publish them. A live Site requires a
Sites deployment; an HTTP server running in a local workspace is not remotely
reachable from the iPhone.
