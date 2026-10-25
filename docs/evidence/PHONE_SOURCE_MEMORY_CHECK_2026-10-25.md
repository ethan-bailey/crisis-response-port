# iPhone source profile memory check

The opt-in WASM route `?source-profile-smoke` now shows a live progress meter
and a copyable JSON report. During sequential profile delivery it samples the
WASM managed heap once per second and after each 25 verified assets. When a
browser exposes `performance.memory`, it also records JavaScript heap usage.
The report includes the iOS/Safari user agent, elapsed time, asset counts,
profile byte totals, largest asset, and peak observed values.

Safari may not expose JavaScript heap or total WebKit process memory. Such
values appear as `null`; .NET managed heap fields are not whole-process or
whole-device memory measurements. A tab reload, crash, or termination before
the report says `complete` is an interrupted run. The last visible asset count
is still useful evidence.

## Run on iPhone

1. Open the exact preview URL for this source build in iPhone Safari and append
   `?source-profile-smoke` (use `&source-profile-smoke` if the URL already has
   a query string).
2. Use Wi-Fi and keep Safari in the foreground until the page says `Complete`.
   The Global/UI/Operator/Version profile contains 1,230 assets and sends about
   36.97 MB of compressed asset data. Do not reload while it is working.
3. Tap **Copy report** and paste the full JSON into the project chat.
4. If Safari reloads or closes before completion, report that and the last
   asset count shown. A screenshot of any Safari memory warning is helpful.

The check performs delivery and hash verification only; it does not start the
Mission scene, actor input, or rendering. No site deployment is made by this
setup change. Use a preview built from the recorded source SHA, not an older
published site.
