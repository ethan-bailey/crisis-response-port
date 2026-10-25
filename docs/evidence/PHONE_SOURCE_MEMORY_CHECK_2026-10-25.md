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

## Owner run result (2026-10-25)

The iPhone Safari run completed in 270,623 ms (about 4m31s) on iOS 18.7.1.
All 1,230 assets arrived and the report's top-level error was null. The
delivery metadata reported 37,646,794 compressed bytes and 82,624,418 source
bytes. The final progress sample reported 36,971,121 compressed bytes and
77,806,659 source bytes for those same 1,230 assets. The byte totals therefore
disagree by 675,673 compressed bytes and 4,817,759 source bytes; this report
does not explain the discrepancy, so the totals should not be treated as
reconciled.

Peak managed allocation was 68,869,128 bytes (about 65.7 MiB), with a reported
managed heap peak of 74,776,576 bytes (about 71.3 MiB). JavaScript heap and
device memory were unavailable in Safari, so these values are not total
WebKit or device memory. One sample at 15 ms ran before incremental package
initialization and returned an error; later samples succeeded and delivery
completed. No browser error or early termination was reported.

This confirms the profile completed on this device and run. It took 4m31s;
the report alone does not establish a universal startup-time or memory limit.
