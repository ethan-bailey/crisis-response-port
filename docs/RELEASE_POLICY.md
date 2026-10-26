# Release and publication policy

Routine source commits do not publish the game. Preserve the existing owner-private Site audience and do not enable automatic Git deployments.

## Required release evidence

Before publication, bind all evidence to one exact source state:

1. Record the current Site source HEAD and the source tree used for build.
2. Use pinned .NET SDK 8.0.426; run Release runtime build and Browser/WASM publish; build the complete bounded Mission profile; run the package verifier; require embedded source SHA to equal build source SHA; record file counts and package/index/archive hashes.
3. Independently extract the final deploy archive and verify every manifest file's path, size and SHA-256. Confirm `.openai/hosting.json` and the static root.
4. Push the exact source before saving/deploying. Preserve the current private audience; use the private deployment operation only for the confirmed owner-private Site.
5. Record saved version ID/number, source commit, deployment ID/status, archive identities and access policy. Device acceptance is a separate check and is never implied by deployment.

## Current Site

Site v75 is deployed from bound Cloudflare source `main` SHA `d8dbe9f3c038e59afbf0d9903d5d036c1f3169b4`; deployment `appgdep_6adf8880cd4c8191a4d16a6789d45c6a` succeeded. Exact Release/WASM build, Mission package verification and independent archive audit passed before publishing. The deployed joystick sprint thresholds are 1.75× entry / 1.5× release. Physical iPhone Safari checks remain outstanding. See `NEXT_AGENT.md` and `evidence/SITE_DEPLOYMENT_ALIGNMENT_BLOCKER_2026-10-26.md` for exact records.

## Hosting

- ChatGPT Sites is the current host. Vercel is not a routine deployment target.
- Never publish a package whose source SHA differs from Site branch HEAD or whose archive has not passed independent checks.
- Never add credentials or private recovery archives to the public repository.
- Keep `fullGameParity`, `sourceVisualParity`, `nativeParityVerified`, `originalGameComplete` and `offlineComplete` false until their evidence gates pass.
