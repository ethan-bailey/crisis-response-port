# Unity WebGL build gate

The recovered Unity project is available in the verified recovery artifact documented in [RECOVERY_RUN_2026-10-14.md](RECOVERY_RUN_2026-10-14.md). The cloud workflow downloads that artifact successfully.

## Why the WebGL build is currently skipped

GitHub Actions confirmed that no Unity license secrets are configured for this repository. Unity Editor builds require valid activation; the project cannot be compiled into WebGL merely by running a C# compiler. The former `game-ci/unity-request-activation-file` action is retired, so the workflow no longer pretends it can generate a license request.

## Required GitHub Actions secrets

If a valid Unity license is already available, add the following as repository secrets (do not post these values in chat or commit them to the repository):

- `UNITY_LICENSE` — the license file contents for the Unity account/license used by the build.
- `UNITY_EMAIL` — the Unity account email.
- `UNITY_PASSWORD` — the Unity account password, stored only as a GitHub Actions secret.

On GitHub's mobile website: open the private repository, go to **Settings → Secrets and variables → Actions → New repository secret**, then create each secret. GitHub may require the repository owner or an administrator role to manage secrets.

After the secrets are configured, rerun **Build recovered Crisis Response for WebGL** from the repository's **Actions** tab. The workflow downloads the already verified recovery artifact and attempts a WebGL build with Unity 2017.2.0f3.

If no valid Unity license file exists and there is no computer available to activate Unity Hub, this is a genuine external licensing blocker. Do not share passwords in chat and do not attempt to bypass Unity's licensing checks. The current workflow will keep the build skipped rather than claim a WebGL app exists.

## Validation still required after compilation

A successful compiler run is not enough. The resulting WebGL player must be opened in iPhone 16 Safari and checked for scene loading, UI/input, audio, physics, AI, memory use, performance, reload/resume, and behavioral parity against the original game.
