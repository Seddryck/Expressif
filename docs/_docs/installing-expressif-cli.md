---
title: Installing the Expressif CLI
sub-title: Install Expressif as a .NET tool, with the Windows installer, or from a portable archive
tags:
  - expressif
  - cli
  - installation
  - dotnet-tool
  - windows
  - linux
---

The Expressif CLI can be installed as a .NET global tool, through a Windows installer, or from a portable archive.

The best option depends on your platform and on whether you already use the .NET SDK:

| Method | Recommended for |
|---|---|
| .NET global tool | Developers and cross-platform users with the .NET SDK |
| Windows installer | Windows users who want a conventional installation |
| Portable archive | Users who want direct control over where the CLI is stored |

Container-based usage is documented separately on the [Using Expressif with Docker](./using-dockerfile-cli.md) page.

## Requirements

Expressif CLI is framework-dependent and requires the corresponding .NET runtime.

The current CLI targets .NET 8.0, .NET 9.0 and .NET 10 and requires on of them:

```text
Microsoft.NETCore.App 8.x
Microsoft.NETCore.App 9.x
Microsoft.NETCore.App 10.x
```

Check the runtimes installed on your machine:

```console
dotnet --list-runtimes
```

A compatible installation includes an entry similar to:

```text
Microsoft.NETCore.App 10.0.x [...]
```

The runtime architecture must also match the selected Expressif distribution. For example, the Windows x64 distribution requires the x64 .NET runtime.

## Install as a .NET global tool

Installing Expressif as a .NET global tool is the recommended option when the .NET SDK is already available.

```console
dotnet tool install --global Expressif-cli
```

The package installs the `expressif` command and selects the package matching the current operating system and architecture.

After installation, open a new terminal, to receive the updated `PATH`, and verify the command:

```console
expressif version
```

### Install a specific version

Use `--version` when the installation must be pinned:

```console
dotnet tool install --global Expressif-cli --version <version>
```

For example:

```console
dotnet tool install --global Expressif-cli --version 1.32.0
```

### Update the tool

Update Expressif to the latest available version:

```console
dotnet tool update --global Expressif-cli
```

Update to a specific version:

```console
dotnet tool update --global Expressif-cli --version <version>
```

### Uninstall the tool

```console
dotnet tool uninstall --global Expressif-cli
```

## Install on Windows

Windows installers are provided for the supported processor architectures:

| Architecture | Runtime identifier |
|---|---|
| Windows x64 | `win-x64` |
| Windows Arm64 | `win-arm64` |

Download the installer matching your machine from the [GitHub release](http://github.com/Seddryck/Expressif/releases/latest).

Installer filenames follow this pattern:

```text
Expressif-<version>-net10.0-<runtime>-setup.exe
```

For example:

```text
Expressif-1.32.0-net10.0-win-x64-setup.exe
```

Run the installer and follow the displayed instructions. The installer:

- installs the CLI under the Windows program files directory;
- exposes the command as `expressif.exe`;
- adds the installation directory to the system `PATH`;
- checks that the required .NET runtime is installed.

Open a new terminal after installation so it receives the updated `PATH`, then verify the installation:

```console
expressif version
```

### Uninstall on Windows

Open **Settings**, navigate to **Apps > Installed apps**, locate **Expressif**, and select **Uninstall**.

The uninstaller removes the installed files and the Expressif entry from `PATH`.

## Install from a portable archive

Portable archives contain the published CLI files without an installer. They can be extracted into any directory for which you have write access.

Archive filenames follow this pattern:

```text
Expressif-<version>-<framework>-<runtime>.<extension>
```

Choose the archive matching the target platform:

| Platform | Runtime identifier | Archive |
|---|---|---|
| Windows x64 | `win-x64` | `.zip` |
| Windows Arm64 | `win-arm64` | `.zip` |
| Ubuntu, Debian, and most Linux distributions | `linux-x64` | `.tar.gz` |
| Alpine Linux | `linux-musl-x64` | `.tar.gz` |

Do not use the `linux-x64` archive on Alpine. Alpine uses musl libc and requires `linux-musl-x64`.

### Install from a ZIP archive on Windows

Download and extract the archive:

```powershell
Expand-Archive `
    -LiteralPath .\Expressif-<version>-net10.0-win-x64.zip `
    -DestinationPath "$env:LOCALAPPDATA\Expressif"
```

Run the CLI directly:

```powershell
& "$env:LOCALAPPDATA\Expressif\expressif.exe" version
```

To invoke `expressif` from any directory, add the extraction directory to your user `PATH`.

For the current PowerShell session:

```powershell
$env:Path += ";$env:LOCALAPPDATA\Expressif"
```

For a permanent installation, add the directory through the Windows environment-variable settings.

### Install from a TAR.GZ archive on Linux

Create an installation directory and extract the archive:

```bash
mkdir -p "$HOME/.local/share/expressif"
tar -xzf Expressif-<version>-net10.0-linux-x64.tar.gz \
    -C "$HOME/.local/share/expressif"
```

Ensure that the command is executable:

```bash
chmod +x "$HOME/.local/share/expressif/expressif"
```

Create a symbolic link in a directory already included in `PATH`:

```bash
mkdir -p "$HOME/.local/bin"
ln -sf "$HOME/.local/share/expressif/expressif" \
    "$HOME/.local/bin/expressif"
```

Ensure that `$HOME/.local/bin` is included in `PATH`:

```bash
export PATH="$HOME/.local/bin:$PATH"
```

Add that line to the shell profile, such as `~/.bashrc`, to make it permanent.

For Alpine, use the same commands with the `linux-musl-x64` archive:

```bash
tar -xzf Expressif-<version>-net10.0-linux-musl-x64.tar.gz \
    -C "$HOME/.local/share/expressif"
```

## Verify the installation

Whichever installation method you choose, verify both the CLI and library versions:

```console
expressif version
```

The output follows this format:

```text
Expressif CLI <version>
Expressif <version>
```

You can also display the available commands:

```console
expressif --help
```

## Troubleshooting

### `expressif` is not recognized or not found

Open a new terminal first. Existing terminals do not automatically receive changes made to `PATH`.

For a .NET global tool installation, check that the .NET tools directory is in `PATH`:

```text
Windows: %USERPROFILE%\.dotnet\tools
Linux:   $HOME/.dotnet/tools
```

For an installer or portable archive, verify that the directory containing `expressif.exe` or `expressif` is included in `PATH`.

### The required .NET runtime is missing

List the installed runtimes:

```console
dotnet --list-runtimes
```

Expressif requires a matching `Microsoft.NETCore.App 10.x` runtime.

Installing only the ASP.NET Core or Windows Desktop runtime does not replace the base .NET runtime requirement.

### The wrong architecture was installed

Use the distribution matching the operating system and processor:

```text
Windows x64   -> win-x64
Windows Arm64 -> win-arm64
Standard Linux -> linux-x64
Alpine Linux   -> linux-musl-x64
```

On Windows, inspect the .NET installation and architecture with:

```console
dotnet --info
```

### Linux reports `Permission denied`

Make the extracted command executable:

```bash
chmod +x expressif
```

Then run it again:

```bash
./expressif version
```

### An older version is still executed

Check which executable is found first:

```powershell
Get-Command expressif -All
```

On Linux:

```bash
command -v -a expressif
```

Remove obsolete installations or reorder `PATH` so that the intended installation is found first.

## Next steps

After installation, continue with [Using the Expressif CLI](./using-expressif-cli) to evaluate and validate expressions from the command line.
