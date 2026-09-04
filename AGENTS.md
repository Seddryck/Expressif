## Issues

Issue titles MUST be descriptive natural-language titles.

Do NOT use Conventional Commit syntax for issue titles.

Prefer:

```text
Map should preserve null values
Add pairwise function
Reduce allocations when mapping arrays
```

Avoid:

```text
fix: preserve null values when mapping arrays
feat: add pairwise function
perf: reduce allocations when mapping arrays
```

Every issue MUST have exactly one change-type label:

* `bug` for a defect
* `new-feature` for new functionality
* `enhancement` for an improvement or refactoring of existing functionality

The label is determined by the nature of the issue.

## Branches and worktrees

Every coding task MUST be performed in its own dedicated worktree and task branch.

For a new task:

1. Fetch the latest remote state.
2. Create the task branch from the latest `origin/main`.
3. Create or use a dedicated worktree for that branch.

Branch names MUST describe the nature of the change:

* `fix/<name>` for bug fixes and issues that correct defective behavior (i.e. when issue is labelled `bug`)
* `feat/<name>` for new functionality (i.e. when issue is labelled `new-feature` or `enhancement`)
* `refactor/<name>` for internal restructuring without changing intended behavior
* `perf/<name>` for performance improvements
* `docs/<name>` for documentation-only changes (i.e. when issue is labelled `docs`)
* `test/<name>` for test-only changes
* `chore/<name>` for maintenance work that does not fit another category

When asked to **fix a bug, defect, regression, or issue describing incorrect behavior**, the branch MUST use the `fix/` prefix.

For example:

```text
fix/predicate-parameter-parsing
fix/coalesce-record-input
feat/max-by
refactor/function-builders
```

Do NOT use tooling-specific prefixes such as:

```text
codex/
chatgpt/
```

The fact that Codex, ChatGPT, or another agent performs the work MUST NOT affect the branch name. Branch names describe the change, not the tool performing it.

The name of the branch should not reference the id of the issue but should be in plain text based on the name of the issue (i.e `fix/predicate-parameter-parsing`).

## Skills

Repository-specific workflows are defined under `.github/skills/`.

When a task matches an existing skill, read and follow that skill before making changes.

In particular:

* Use `/scaffold` when introducing a new function, predicate, or accumulator definition, including documentation metadata and conformance cases.
* Use `/implement` when implementing an operator for which documentation metadata and conformance cases already exist.

For a new operator that requires both steps, run `/scaffold` before `/implement`.

Skills define task-specific procedures. `AGENTS.md` defines repository-wide rules and takes precedence if a skill contains conflicting Git, worktree, branch, issue, commit, or pull-request instructions.

## Version-sensitive integrations

Treat every user-approved version target or version sequence as an invariant. This applies to tasks involving GitVersion, release numbering, staged integrations, package publication, GitHub releases, or commits containing `+semver` directives.

Do NOT predict GitVersion results from Conventional Commit prefixes, branch names, previous experience, or memory. Do NOT assume that multiple `feat` commits collapse into one increment, that `+semver: none` prevents the branch's default increment, or that a result on a feature branch will equal the result when the same commit is the tip of `main`.

### Establish the expected sequence

Before rewriting, pushing, merging, or fast-forwarding any version-sensitive stack:

1. Write down the exact sequence of commits that are expected to become tips of `main`.
2. Write down the exact version expected at every one of those tips.
3. Treat that SHA-to-version table as the approved integration contract.
4. If the user has supplied only the final version, derive the intermediate expectations and present them before integration.
5. If any expectation is ambiguous, stop and ask the user. Do not select a version interpretation on the user's behalf.

An agreed version does not become optional merely because a later GitVersion calculation returns a different value. A different calculation is a divergence from the plan, not permission to revise the plan.

### Simulate the actual `main` history

Before publishing rewritten branches or changing `main`, create an isolated clone and simulate the proposed integration history using the repository's installed GitVersion and configuration.

For every proposed integration tip:

1. Check out the exact commit as a local branch named `main`.
2. Run GitVersion against that checkout.
3. Record the full commit SHA and calculated version.
4. Compare the result mechanically with the approved SHA-to-version table.

The simulation MUST use the actual `main` branch name. Calculating on a feature branch, detached HEAD, pull-request merge ref, or inferred future graph is not an acceptable substitute.

Recreate and rerun the complete simulation after any operation that changes commit messages, parents, ordering, ancestry, or hashes, including:

* amend;
* interactive rebase;
* cherry-pick;
* squash;
* adding, removing, or changing a `+semver` directive;
* moving commits between stacked pull requests;
* changing a pull request base;
* inserting documentation, maintenance, or release commits into the stack.

After a rewrite, record an old-to-new SHA mapping. Update the integration contract with replacement SHAs only after confirming that every calculated version still matches the previously agreed version sequence.

If any simulated version differs from the agreed plan, stop before pushing rewritten branches. Report the exact expected and actual versions and wait for explicit user instructions. Do NOT reinterpret the user's intended version, continue because the result appears internally consistent, or describe the new result as intended.

### Monitor version calculation in CI first

For every pull-request or `main` pipeline in a version-sensitive integration, actively monitor the version-calculation job as soon as the pipeline starts. Do not merely wait for the complete workflow and inspect the version afterward.

1. Confirm that the workflow is running for the exact expected SHA.
2. Locate the `calculate-version` job, or the repository's equivalent GitVersion job.
3. Monitor that job until it completes.
4. Read the calculated version from its outputs or logs.
5. Compare it immediately with the approved version for that exact SHA, accounting for the pipeline context as described below.

A successful job conclusion is not enough. In a `main` pipeline, the calculated version MUST equal the approved version exactly. In a pull-request pipeline, GitVersion may append its normal `PullRequest` prerelease label and counter to the approved `main`-tip version. This is not a version mismatch when the version that was simulated with the same commit as an actual `main` tip is the exact base version of the pull-request result. For example, these results agree:

* simulated as an actual `main` tip: `2.33.0`;
* pull-request CI: `2.33.0-PullRequest913.3`.

Do not require the pull-request suffix to appear in the approved SHA-to-version table. Compare the pull-request result's base version with the approved `main`-tip version, and treat any other difference as a mismatch.

If the CI-calculated version differs from the expected version:

* stop the integration immediately;
* cancel every still-running workflow for that SHA when cancellation is available;
* do not wait for build, packaging, publication, or deployment jobs;
* do not merge the pull request;
* do not advance `main` to the next planned tip;
* do not create or move tags;
* do not publish packages or releases;
* report the exact SHA, expected version, actual version, and workflows cancelled;
* wait for explicit user instructions.

If the mismatch is detected after a commit has already reached `main`, leave `main` at that exact commit unless the user explicitly authorizes another action. Cancel downstream workflows immediately to prevent publication, and do not attempt an unapproved revert, force-push, compensating release, or version bump.

### Stage version-sensitive integrations

Do not advance `main` from one planned tip to the next until all of the following are true for the current stage:

* `main` points to the exact approved SHA;
* CI ran against that exact SHA;
* the CI version-calculation output equals the approved version, or, for pull-request CI, differs only by the permitted `PullRequest` prerelease suffix described above;
* the associated pull request is marked `MERGED` when a pull request is part of the plan;
* all required non-version checks have completed according to the user's approved exception list;
* no branch, pull-request head, base, tag, or remote tip has diverged from the approved graph.

Immediately before every push to `main`, fetch the remote and repeat the ancestry, SHA, and version checks. The push MUST be a strict fast-forward unless the user explicitly authorizes a different operation.

Version publication is irreversible. Immediately before any push that can publish a package, tag, or GitHub release, rerun the exact-tip simulation and compare it with the approved integration contract. Any mismatch is a hard stop.

### Never redefine success

Do NOT replace an agreed target with the version that GitVersion happened to calculate. Do NOT claim that a different version is correct because of newly discovered `+semver` behavior. Do NOT proceed on the basis that a later feature can repair an unintended release.

When the calculated result and the approved target disagree, the only successful action is to stop before publication and ask the user how to change the history or version directives.

## Git hooks

Repository hooks are stored in `.githooks/`. Enable them after cloning the repository:

```text
git config core.hooksPath .githooks
```

On Windows, the hooks run with the Bash bundled in Git for Windows. On all platforms, they require `commitlint` and the .NET SDK to be available on `PATH`. Restore .NET dependencies before pushing because the StyleCop check runs with `--no-restore`.

The `commit-msg` hook validates every commit message against `commitlint.config.cjs`. Commit messages MUST use an allowed Conventional Commit type and satisfy the configured header and line-length limits.

The `pre-push` hook runs StyleCop analyzers for every branch. The push is rejected when `dotnet format Expressif.sln analyzers --verify-no-changes --no-restore` finds analyzer violations.

Do NOT bypass these hooks. Resolve validation failures before committing or pushing.

## Pull requests

For every completed implementation:

1. Push the task branch.
2. Create a GitHub pull request targeting `main`.
3. Use a Conventional Commit-style PR title.
4. Include a concise description of the change.
5. Include the relevant tests or validation performed.
6. Link the pull request to the corresponding issue when one exists (use wording `close`).

Do NOT use `bug`, `new-feature`, or `enhancement` labels on the pull request unless explicitly requested.

## Completion criteria

A coding task is complete only when:

* implementation was performed in the task's dedicated worktree;
* for a new task, the branch was created from the latest `origin/main`;
* the branch name follows the repository branch naming rules;
* the solution build successfully;
* the relevant tests have been run;
* all intended changes are committed;
* commit messages follow Conventional Commits;
* the branch has been pushed;
* a pull request targeting `main` has been created;
* the PR title follows Conventional Commits;
* the corresponding issue has the appropriate `bug`, `new-feature`, or `enhancement` label;
* the pull request is linked to the issue when one exists;
* the worktree is clean.
