# Release and publication policy

Routine source commits do not publish or deploy the game. The GitHub Actions workflow builds only inside its runner and has no deployment credentials. The current Crisis Response Runtime Site is access-restricted; preserve its current audience.

## Required release evidence

Before publishing, all evidence must refer to one exact source and package:

1. Identify the live develop/wasm SHA and the exact source tree used for the build.
2. Run the relevant source contracts, pinned .NET 8.0.426 Release build and WASM publish. Build the complete Mission Site package from that publish output and tracked bounded source profile; independently run the package verifier; assert the embedded source SHA matches the workflow SHA; record package file count and package/archive hashes. Byte-audit the eventual deployment archive and record commands, warnings, errors, and the successful CI run for the same GitHub SHA.
3. If the Site keeps a separate source history, record the Site-source commit and a file-level crosswalk to the GitHub tree. Include exact paths, sizes, SHA-256 values, and archive/manifest hashes. A Site commit absent from public GitHub history is not itself proof of differing bytes, but it is not covered by the GitHub run until the crosswalk is verified.
4. After explicit publication authorization, verify the deployed version and access policy, then have the owner test that exact build in physical iPhone Safari against NEXT_AGENT.md. A successful deployment or browser shell is not device acceptance.
5. Record the saved version ID/number, deployment ID and status, Site source commit, artifact hashes, access policy, and remaining device-only acceptance. Keep the existing restricted audience.

Current Site version 74 is deployed from its bound Cloudflare artifact repository at source commit `ae134ef153149e6b6ab262502dd978c4737087ca`; deployment `appgdep_6adf84df5dd481919cd70f72f33f0889` succeeded. The pinned .NET 8.0.426 Release build, WASM publish, full Mission package verifier and independent archive byte audit all passed against that exact Site commit before publication. Version ID, archive hashes, package counts and the Site-to-develop tracked-blob crosswalk are recorded in [the current Site deployment evidence](evidence/SITE_DEPLOYMENT_ALIGNMENT_BLOCKER_2026-10-26.md). The Site retains its custom owner-only audience. Physical iPhone Safari acceptance remains open: cold/warm storage, late Mission-load progress, joystick aim/sprint, rapid-tap zoom and device audio. Public develop run #79 is separate SHA-specific evidence and is not presented as CI for the Site-bound commit. See [the version 71 reconciliation](evidence/SITE_VERSION_71_RECONCILIATION_2026-10-26.md) for historical context. Do not publish a subsequent version until its exact-source build/package/archive integrity gates pass and owner publication authorization applies.

## Hosting

- ChatGPT Sites is the current host for the restricted runtime.
- Do not publish intermediate builds or change Site access settings.
- Keep automatic Git-based deployments disabled. Vercel is not a routine deployment target; use a Vercel fallback only if the owner explicitly selects it and the same exact-SHA gates pass.
- Never put credentials or private recovery archives in the public repository.

For current version IDs and open acceptance gates, read [NEXT_AGENT.md](../NEXT_AGENT.md). For project-wide agent behavior, read [AGENTS.md](../AGENTS.md).
