---
name: install-local
description: Download and install the portable Expressif CLI for .NET 10 on Windows x64 from an open or closed GitHub pull request, a named release, or the latest release into a user-specified local directory. Use when testing a CI-built or released CLI locally; do not use for building the CLI from source or running the Windows installer.
---

# Install Local

Use `scripts/install-local.ps1` to resolve, download, and extract the portable .NET 10 `win-x64` Expressif CLI. The script requires authenticated GitHub CLI access (`gh auth status`).

## Select the source

- For a pull request, run `./scripts/install-local.ps1 -InstallDirectory <path> -PullRequest <number>`. An open PR resolves the newest successful `ci.yml` pull-request run for the PR head SHA and downloads its `distribution-net10.0-win-x64` artifact. A merged PR resolves the GitHub release whose tag points to the merge commit. A closed, unmerged PR is not installable.
- For a named release, run `./scripts/install-local.ps1 -InstallDirectory <path> -Release <tag>`.
- For the latest release, run `./scripts/install-local.ps1 -InstallDirectory <path>`, or pass `-Release latest` explicitly.

The install directory is mandatory. By default, it must be empty or not yet exist. If the user explicitly wants to reuse a non-empty directory, pass `-Force`; files from the archive can then overwrite files with the same names, while unrelated existing files remain in place. **Never overwrite an existing `expressif.config.json`, even with `-Force`.** Preserve it byte-for-byte; install the packaged defaults only when no configuration file exists. Do not delete or reset user configuration before installation. The script downloads exactly one `expressif-*-net10.0-win-x64.zip`, extracts it into that directory, and verifies `expressif.exe`. It uses a unique temporary download directory and removes that directory afterward.

Run the command from this skill directory, or invoke the script by its full path. Report the selected PR run or release tag and the installed executable path. If no successful, unexpired PR artifact or commit-matching release exists, stop and explain that specific condition instead of silently falling back to another source.
