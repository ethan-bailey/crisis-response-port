# Release and publication policy

Routine source commits do not publish or deploy the game. The GitHub Actions workflow builds only inside its runner and has no deployment credentials. The current Crisis Response Runtime Site is access-restricted; preserve its current audience.

## Required release evidence

Before publishing, all evidence must refer to one exact source and package:

1. Identify the live develop/wasm SHA and the exact source tree used for the build.
2. Run the relevant source contracts, pinned .NET 8.0.426 Release build and WASM publish. Verify the complete package and byte-audit the deployment archive. Record commands, warnings, errors, file count, package/archive hashes, and the successful CI run for the same GitHub SHA.
3. If the Site keeps a separate source history, record the Site-source commit and a file-level crosswalk to the GitHub tree. Include exact paths, sizes, SHA-256 values, and archive/manifest hashes. A Site commit absent from public GitHub history is not itself proof of differing bytes, but it is not covered by the GitHub run until the crosswalk is verified.
4. Test the exact saved/deployed version in a WebGL-capable browser and on the owner's physical iPhone Safari for the acceptance items in NEXT_AGENT.md. A successful deployment or browser shell is not device acceptance.
5. Record the saved version ID/number, deployment ID and status, Site source commit, artifact hashes, and access policy. Keep the existing restricted audience.

The current source-to-Site relationship is not fully crosswalked. Do not save or deploy another Site version until that comparison is complete and the owner has explicitly authorized publication.

## Hosting

- ChatGPT Sites is the current host for the restricted runtime.
- Do not publish intermediate builds or change Site access settings.
- Keep automatic Git-based deployments disabled. Vercel is not a routine deployment target; use a Vercel fallback only if the owner explicitly selects it and the same exact-SHA gates pass.
- Never put credentials or private recovery archives in the public repository.

For current version IDs and open acceptance gates, read [NEXT_AGENT.md](../NEXT_AGENT.md). For project-wide agent behavior, read [AGENTS.md](../AGENTS.md).
