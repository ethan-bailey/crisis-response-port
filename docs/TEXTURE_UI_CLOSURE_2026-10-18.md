# Original texture and UI dependency continuation — 2026-10-18

Full CharacterBody compiler closure remains incomplete. At executable source
`a41ae49b33139b6400089036f4984378f91382d3` (tree
`d7bfb42dd8459058533c2cf008e83c24a30d0d4a`), the complete normal runtime build
exits **1**, with **35 distinct diagnostics / 28 missing names**.

## Inspected state

Started from live `feature/full-source-characterbody-integration` head
`0a829220367ac971b91433f7313f3dd68c00b0eb`, whose executable source was
`23e4cf0` (42 diagnostics / 34 names). Inspected `develop/wasm` at `2fc0a8e`, all
branches, open PRs #7/#42/#56/#57, recent merges #62–#67, recent Actions and the
remediation log. No overlapping new source work was present. The latest comparable
full CI checkpoint remains failed run 39091701268 (`407c66d`), and narrow
remediation CI remains run 39093133256 (`389d8e8`). Neither verifies this batch.

The exact local Git checkout was reconciled with the live head before changes.
.NET SDK 8.0.426 and the SHA-pinned original recovery ZIP were restored. Branch
heads were checked again before publishing, with expected-SHA leases.

The 42-error graph supersedes older 30/21, 144/60 and 117/50 historical local
checkpoints for this comparison. The new normalized diagnostic report resolves
**seven** diagnostics and introduces **zero** relative to executable source
`23e4cf0`. Missing declarations still hide further method-body and identity
errors: 35 is not an estimate of all remaining work.

## Original sources and compatible engine services

`aa80a0f` recovers complete original LimbTextureData, DynamicLimbTextureData,
DynamicLimbTexturePool and StaticDebug. Every file matches its indexed archive
bytes after reversing the recorded namespace wrapper and Unity aliases. Required
texture constructors, wrap addressing, bulk pixels, render-target allocation and
release, Blit and ReadPixels now have compatible runtime entry points. The CPU
backend performs actual same-resolution mask copies and readback with independent
storage, validated regions, destination offsets and per-world ownership.

`c47788d` recovers complete original UIHitAnnouncements, UIHitAnnouncement,
UIVocalizationText, UIInteriorModeTextIndicator, UIGrenadeTimer and AudioPlayer.
Original methods, formulas, callbacks and order are unchanged. Generic uGUI
text/image/slider component state and presentation bindings support their typed
dependencies. A follow-up (`a41ae49`) preserves serialized text/color when cloning
UI prefabs, drops presentation-driver bindings from clones, and copies Color as
a native value type rather than attempting to construct a gameplay replacement.

The authored Text and color fields are retained as serialized CPU component state.
Actual UI drawing requires a bound presentation driver; Present throws if none is
bound. Slider data clamps and forwards actual value changes. None of these engine
types owns character health, equipment, combat decisions or mission rules.

The native/GPU boundary remains explicit: CPU transfers do not rasterize GL
wounds, resample textures, convert color spaces, implement mip chains, or allocate
a physical depth buffer. RenderTexture depth is retained as descriptor metadata.
Unsupported transfer cases fail explicitly. These tests do not establish browser
rendering, original material/shader execution or wound-pixel parity.

API references checked against Unity documentation:

- https://docs.unity.cn/2018.4/Documentation/ScriptReference/Texture2D.GetPixel.html
- https://docs.unity3d.com/ja/2019.4/ScriptReference/Texture2D.ReadPixels.html

## Verification

Local source inputs exactly match committed tree `d7bfb42`. Logs and normalized
diagnostics are preserved under `docs/evidence/texture-ui-closure-20261018/`, with
a SHA256 manifest pinned to source `a41ae49`.

| Gate | Result |
| --- | --- |
| Original foundations / Behavior Designer | **120 assertions and all ten RuntimeContracts groups pass** |
| Isolated engine graphics | **45 assertions pass** |
| Original/archive identity checks | **197 identities pass** |
| Audit-tool regression suite | **Eight tests pass** |
| Actor migration and source ownership | Pass; zero migration violations |
| Regenerated archive index and dependency inventory | Fresh; 360 local sources, 1,613 symbols, 1,036 conservatively reachable files, 83 ambiguous names |
| Full normal runtime compilation | **FAIL, exit 1; 35 diagnostics / 28 missing names** |

New engine checks exercise real pixel content, row orientation, subrectangle
placement, snapshot isolation, rejected partial writes, release/recreation,
wrap behavior and cross-world rejection. No empty backend callback is counted as
a successful pixel transfer.

New foundation checks execute the **complete unchanged original UIHitAnnouncement
and AudioPlayer classes**, including original scale/alpha curves, random travel
bounds, lifetime destruction, audio throttle and result-clip order, and cloned UI
data isolation. Audio uses a recording driver with declared decoded fixtures;
actual speaker output and original clip decoding remain unverified.

The remaining recovered UI managers, grenade/actor bindings, texture-pool consumers
and original limb renderer have not been executed through a full original actor.
Their presence in the still-failing project is not a gameplay success claim.

No production source exclusions were added, no frozen transport gameplay files
were changed, and no replacement actor/vital owner was introduced. No successful
full compile, new GitHub Actions run, merge, deployment or browser/iPhone combat
verification is claimed. Existing UI controls and preview assets are unchanged.

## Reproduction and remaining gate

Use the commands in `SETTINGS_SERIALIZATION_CLOSURE_2026-10-18.md`, with source
`a41ae49b33139b6400089036f4984378f91382d3` and the same pinned recovery ZIP.
Foundation/engine projects run the added checks automatically. Summarize the full
build against `docs/evidence/settings-closure-20261018/full-diagnostics.json` and
preserve the actual exit code. On this host, direct invocation of SDK MSBuild.dll
avoids the previously observed SDK CLI process-stat issue.

Next unmet gate remains full original compilation: mission/world/mode/UI owners,
room interactions/lighting, blood/decal/painter dependencies, Usage, and then the
exposed original actor/equipment/projectile/IK identity and engine API errors.
Obtain a zero-error complete build, verify the exact source SHA in GitHub Actions,
then separately prove initialization, two-character combat and WASM/browser flow.

Rollback this continuation by reverting `a41ae49`, then `c47788d`, then `aa80a0f`.
Revert the separate evidence/documentation commit independently. Do not reset the
integration branch or replace older compiler evidence.
