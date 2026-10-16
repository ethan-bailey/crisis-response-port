# Vercel deploy policy — manual iPhone-test releases only

Updated October 16, 2026. This is the active deployment policy for the **existing private** `ethan-bailey/crisis-response-port` repository and its Vercel `crisis-response-port` project (ID `prj_EgsDBXSKhvina2fDecbdMf6ylTKE`). It is NOT permission to make recovered game source/assets public.

## Automatic deployments disabled

The Vercel project was confirmed to create new Git-source deployments for ordinary `codex/*` commits. Vercel's Pro/Enterprise-only `deploymentPolicy` API was rejected by Vercel with `403 pro_plan_required`, so **do not retry that paid-only policy or upgrade billing**.

Instead, official supported Hobby-compatible configuration is committed as `vercel.json` to **every currently existing branch**, including default `main`:

```json
{
  "$schema": "https://openapi.vercel.sh/vercel.json",
  "git": {
    "deploymentEnabled": false
  }
}
```

Original documentation: https://vercel.com/docs/project-configuration/git-configuration .

This explicitly prevents **automatic Git-triggered Vercel deployments** of future commits when Vercel reads that branch's configuration, for both production and previews. The existing Vercel project, branch aliases, deployment history, access controls and the previously owner-private hosted game remain unchanged; GitHub CI workflows remain enabled. The file is deliberately present on `main` so new branches normally inherit the rule. **Any branch created from an older ref lacking this file must add it before pushing to Vercel**. Do not introduce `github.enabled:false` (deprecated, and blocks deploy hooks).

Initial application commit log:

| Branch | Configuration commit |
| --- | --- |
| `main` | `37a83b82503b83ae4f247d65c5c198f45d765ae6` |
| `codex/full-source-closure-audit` | `e888d2938c6ac6b12c28381a4aa06e79cd7bfa5d` |
| `codex/iphone-input-parity-foundation` | `ff2eed647e4eeb3446453b304bb4cbc4b6d31349` |
| `codex/native-marksman-webgl-runtime-validation` | `796f164eb8f37bf1ef43c4489a0ee490dd84eff6` |
| `codex/projectile-material-impacts` | `52c5f05f166ca61b053178907fa0874132917bbc` |
| `codex/wasm-compile-package` | `d67d446bd062466a4ec62fcf24bf49818825a5c9` |
| `codex/wasm-original-arm-sequences` | `097f1dfed194c670e736d7a4234c56a79766d16d` |
| `codex/weapon-actions-checkpoint` | `a824915c08b2d945a77de7c33297453680ef8034` |

## Standard branch layout (2026-10-16)

The repository now uses **`main`** for stable repository baselines and **`develop/wasm`** for the integrated game. `develop/wasm` was created from the latest validated original-code PR #9 tree and **inherits** root `vercel.json` with `git.deploymentEnabled=false`. New feature branches should be cut from `develop/wasm` so they inherit this rule; research/experiment branches must do the same or explicitly add the policy before creating Git-triggered deploys. Existing branch-alias URLs still point to the last **actual deployed build**. Manual device-test releases require a matching verified package and access protection. Contributor rules: [branch policy](BRANCHING_AND_COLLABORATION.md).

## Explicit device-test deployment protocol

1. First run the original C#/WASM CI tests and verify the exact intended code SHA, private built artifact, original asset hashes, scene/weapon input, browser tests and WebKit smoke evidence. A green documentation/source audit does not establish a playable build.
2. Confirm `crisis-response-port` is the intended Vercel project, team/account scope, and that deployment protection remains enforced. Do not release unreviewed proprietary assets to an openly accessible Vercel domain.
3. **Only when an actual iPhone Safari test URL is required**, explicitly deploy the selected **verified** commit/package using Vercel's manual deployment UI (Project → Deployments → Create Deployment → enter branch or exact commit SHA) or an authenticated Vercel deployment tool / CLI / deploy hook. Git integration stays connected; there is no need to toggle auto-deploy back on. A deployment from the raw repository may not package the verified CI WASM `/game/` + `/runtime-web/` output; verify the uploaded content matches the tested source/artifact before declaring success.
4. Inspect deployment status; confirm HTTPS, required WebAssembly MIME/base paths, actual protected Safari login/access, touch behavior, version and gameplay. Update `NEXT_AGENT.md` with tested SHA, deployment ID/URL, known limitations and rollback ID.
5. Existing **branch alias URLs point to their latest actual deployment, not to the latest Git commit**. Disabling automatic deployment means commits alone do not advance those URLs. Manual deployment can create a new preview URL or reassign branch aliases according to its source; verify the actual resulting alias rather than assuming a CLI-created preview takes the usual Git branch alias.

## Rollback

To restore the previous automatic Vercel Git behavior for a specific branch, remove `vercel.json` from that branch in a reviewed commit (or explicitly change `git.deploymentEnabled` back to `true`). Do not reset whole branch history, merge unpublished gameplay, change the repo privacy or disable GitHub Actions. Because the policy is repeated per branch, keep the rule on other branches until they deliberately opt back in.

Verification on October 16, 2026: all eight branch configurations were re-read from GitHub and each reported `git.deploymentEnabled=false`. A subsequent Vercel deployment-list inspection found the most recent completed deployment still sourced from a pre-policy Git commit `f0c9330fe0c5156d54f53c89c5f6817a465881f8`; no policy-commit deployment had appeared at that time. This is configuration and snapshot evidence, not a guarantee that an older queued job will never finish. Do not launch an unwanted test deployment just to validate the gate.
