# iPhone source profile memory check

## Exact private build

Open this URL in iPhone Safari:

https://crisis-response-memory-check-20261024.craftysnipe13.chatgpt.site/runtime-web/?source-profile-smoke

This is version 1 of the separate owner-private diagnostic Site. Use the `/runtime-web/` path: `/game/` opens the visual game and does not run this check. Open it while signed in to the ChatGPT account that owns the Site. The existing Crisis Response Runtime Site was left unchanged.

The route delivers the Global Context, UI, Operator, and Version profile: 1,230 assets, about 36.97 MB compressed. It verifies asset hashes and reports delivery progress, elapsed time, managed heap samples, and JavaScript heap values when Safari exposes them. It does not start Mission gameplay, input, or rendering.

Safari may not expose total WebKit process memory. The report labels unavailable values as null, and sampled peaks may miss brief spikes.

## Run on iPhone Safari

1. Open the exact URL above. Keep Safari in the foreground.
2. Leave the page alone until it says **Complete**. Do not aim, shoot, move, reload, or switch apps during the run.
3. Tap **Copy report** and paste the full JSON into the project chat.
4. If Safari reloads or closes before completion, report that and the last asset count shown. A screenshot of any Safari memory warning is useful.
