# Repository visibility and GitHub Actions

**Owner decision, verified 2026-10-24:** `ethan-bailey/crisis-response-port` is public. GitHub Actions is enabled for the repository, with **Allow all actions and reusable workflows** selected. Do not follow older instructions that say the repository must remain private or Actions must stay disabled.

## Working rules

- Keep the repository public and Actions enabled unless the owner explicitly changes that decision.
- Standard GitHub-hosted runner usage is free for public repositories under GitHub's public-repository policy. Larger runners may still incur charges, and artifact/cache/storage limits still apply. Check current GitHub usage and billing terms before changing runner types or usage patterns.
- Workflows can run according to their configured triggers. Inspect the workflow file and target branch before dispatching or enabling a new workflow; enabling Actions does not add workflow files to a branch.
- Public repository contents, commit history, workflow run details, logs, and artifacts may be visible to the public. Never print credentials or secret values. GitHub Actions secrets are not exposed by making a repository public, but workflows must not echo them.
- The ChatGPT Sites deployment audience is a separate setting and remains owner-only unless the owner explicitly directs otherwise.
- This policy is persistent in repository documentation. If repository settings change, update this file and the agent instructions/handoff in the same session.

The owner directed these settings on 2026-10-24 and confirmed the public-visibility change. GitHub's repository settings showed Actions allowed, and the Actions page loaded the workflow list and historical runs.
