# Release and publication policy

Routine source commits do not publish or deploy the game. The GitHub Actions workflow builds only inside its runner and has no deployment credentials. The current Crisis Response Runtime Site is access-restricted; preserve its current audience.

## Required release evidence

Before publishing, all evidence must refer to one exact source and package:

1. Identify the live develop/wasm SHA and the exact source tree used for the build.
2. Run the relevant source contracts, pinned .NET 8.0.426 Release build and WASM publish. Verify the complete package and byte-audit the deployment archive. Record commands, warnings, errors, file count, package/archive hashes, and the successful CI run for the same GitHub SHA.
3. If the Site keeps a separate source history, record the Site-source commit and a file-level crosswalk to the GitHub tree. Include exact paths, sizes, SHA-256 values, and archive/manifest hashes. A Site commit absent from public GitHub history is not itself proof of differing bytes, but it is not covered by the GitHub run until the crosswalk is verified.
4. After explicit publication authorization, verify the deployed version and access policy, then have the owner test that exact build in physical iPhone Safari against NEXT_AGENT.md. A successful deployment or browser shell is not device acceptance.
5. Record the saved version ID/number, deployment ID and status, Site source commit, artifact hashes, access policy, and remaining device-only acceptance. Keep the existing restricted audience.

Current Site version 71 is deployed and its archive integrity audit was reported passed. The Sites API records archive SHA-256 `9efc3ee61c42624b6c8d078f3c4cb161b9f690656177c2fd08d7440034347040` and source identifier `37e6d5dcebaaba71c405d898d161b3e36e946a78`. The latter does not resolve to a commit in the available public GitHub repository; the current `develop/wasm` code checkpoint is `ccc8b517a6d675ab030972e9dc857e7bf705b77e` and `main` is `d7af007`. The exact Site source crosswalk is open. Physical iPhone Safari acceptance of the 98 MiB Mission load and repeat-run cache is also pending. See [the version 71 reconciliation](evidence/SITE_VERSION_71_RECONCILIATION_2026-10-26.md). Do not publish a subsequent version until source identity, exact build, and package/archive integrity are tied together; future publication still requires owner authorization.

## Hosting

- ChatGPT Sites is the current host for the restricted runtime.
- Do not publish intermediate builds or change Site access settings.
- Keep automatic Git-based deployments disabled. Vercel is not a routine deployment target; use a Vercel fallback only if the owner explicitly selects it and the same exact-SHA gates pass.
- Never put credentials or private recovery archives in the public repository.

For current version IDs and open acceptance gates, read [NEXT_AGENT.md](../NEXT_AGENT.md). For project-wide agent behavior, read [AGENTS.md](../AGENTS.md).
