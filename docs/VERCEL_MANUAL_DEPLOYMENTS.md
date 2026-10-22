# Hosting policy — ChatGPT Sites primary, manual Vercel fallback

Updated 2026-10-22 for the owner-authorized hosting plan. The repository and raw recovered code/assets remain private. ChatGPT Sites is the primary host for an explicitly authorized compiled package. Vercel is a manual fallback only. Automatic Vercel deployments remain disabled by vercel.json.

## Required local verification

On the exact develop/wasm source SHA:

1. Run a fresh full-source build, relevant contracts, and provenance checks; record commands, results, warnings, and the exact SHA.
2. Build the compiled browser runtime and required game assets into a package with a SHA-256 integrity manifest. Record file count, total bytes, and manifest verification.
3. Serve that exact package locally and use a real browser to confirm the page loads, WebAssembly boots, the Marksman scene loads, and input responds. Do not publish a package that fails this gate.

## ChatGPT Sites — primary

Create the Site from the verified package without rewriting, regenerating, or substituting any game files. Set access to public only when explicitly authorized. Confirm the package size and file count are accepted, .wasm is served with an executable media type, and audio/assets load. In a fresh browser session, repeat the local smoke checks against the live URL. A URL alone is not a live smoke pass.

If Sites cannot host a package that passed local smoke, make one bounded repackaging-only attempt. If it still fails, record the exact failure and deploy the same verified package through the Vercel fallback.

## Vercel — manual fallback only

The Vercel project is ethan-bailey/crisis-response-port (project ID prj_EgsDBXSKhvina2fDecbdMf6ylTKE). Automatic Git-source deployments are disabled with:

    {
      "$schema": "https://openapi.vercel.sh/vercel.json",
      "git": { "deploymentEnabled": false }
    }

Do not enable automatic deployments or change billing. Use Vercel only after a Sites hosting failure for a package that already passed local smoke. Deploy the same verified package manually, retain access protection unless the owner directs otherwise, verify the resulting URL with the same live smoke, and record its deployment ID and URL. Git commits alone do not advance existing Vercel aliases.

## Verification record for 2026-10-22

At develop/wasm source SHA 35f077f293305694f5a279bc073f23e159aba0fe, the fresh full-source build and browser-runtime publish succeeded, but the local browser smoke failed: the page displayed “Runtime error. Reload to reset.” and requested browser-input.mjs with HTTP 404. The Marksman scene did not load and input did not update. No Sites or Vercel deployment was attempted. See NEXT_AGENT.md for the package manifest, exact results, and next action.