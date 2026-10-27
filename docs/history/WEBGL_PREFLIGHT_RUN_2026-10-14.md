# WebGL preflight run — 2026-10-14

## Run result

- GitHub Actions run: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38586732705
- Triggering commit: `5baf9067b3f866e3bdfecd0fd5c48691eb5ac631`
- Workflow result: successful preflight job; Unity build step skipped because Unity license secrets are not configured.
- Static report artifact: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38586732705/artifacts/11904739177
- Build-gate report artifact: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38586732705/artifacts/11904614338

## Verified project inventory

The workflow downloaded the previously verified recovery artifact and successfully ran the new static scanner against the recovered project.

- Unity Editor version recorded in project: 2017.2.0f3
- C# source files scanned: 1,420
- Scene assets found: 6
- Enabled scene entries in EditorBuildSettings: 6

## Candidate compatibility matches

These are static source-line matches, **not** compiler errors or proof that every matching call is reachable in WebGL.

| Pattern | Matching source lines |
|---|---:|
| Managed thread creation | 1 |
| Thread.Abort | 1 |
| System.Net.WebRequest | 2 |
| Microsoft.Win32.Registry | 1 |
| BinaryFormatter | 6 |
| Desktop filesystem API patterns | 23 |
| Keyboard input polling | 33 |
| Mouse input polling | 51 |
| Cursor lock/visibility operations | 31 |
| DllImport / LibraryImport | 0 |

The full location/excerpt report is in the linked Actions artifact.

## What this run proves

- The private recovery artifact is still accessible to the workflow.
- The static scanner executes successfully in GitHub Actions.
- The preflight report and build-gate report upload successfully.
- The workflow correctly detects that `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD` are empty and skips Unity compilation instead of misreporting success.

## What this run does not prove

- No Unity compilation was attempted.
- No WebGL player was produced.
- No browser or iPhone runtime test was performed.
- No gameplay subsystem has passed a behavioral-parity test.

## Next required gate

A valid Unity Editor build environment/activation is still needed to produce the first real compiler log. If a valid license is available, add it only through GitHub Actions repository secrets and rerun the workflow. Do not send credentials through chat or commit them. If valid Unity activation cannot be obtained, this workflow cannot produce the compiler log; proceed with compatibility-runtime research or another legitimate licensed build environment rather than bypassing licensing.
