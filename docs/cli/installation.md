---
layout: docs
title: Installation
parent: Command-line interface
nav_order: 10
description: Install the Expressif CLI as a .NET global tool, Windows application, or portable executable.
---

The Expressif CLI targets .NET 10 and requires a matching `Microsoft.NETCore.App` 10.x runtime with the same architecture as the selected distribution.

## Install as a .NET global tool

The global tool is the simplest cross-platform option when the .NET SDK is already installed.

```bash
dotnet tool install --global Expressif-cli
expressif version
```

`Expressif-cli` is the package identifier. The installed command is `expressif`.

To install a specific version, update it, or remove it:

```bash
dotnet tool install --global Expressif-cli --version <version>
dotnet tool update --global Expressif-cli
dotnet tool uninstall --global Expressif-cli
```

Open a new terminal if the command is not immediately available after installation.

## Install on Windows

Download the installer for the machine architecture from the [Expressif releases](https://github.com/Seddryck/Expressif/releases) page. Installer filenames follow this pattern:

```text
Expressif-<version>-net10.0-<runtime>-setup.exe
```

For example:

```text
Expressif-1.32.0-net10.0-win-x64-setup.exe
```

The installer places the executable under `Program Files`, adds it to the system `PATH`, and checks for the required .NET runtime. Remove it from **Settings > Apps > Installed apps**.

## Use a portable archive

Portable archives are useful for pinned, isolated, or manually managed deployments.

Choose the archive that matches the platform:

| Platform | Runtime identifier | Format |
|:--|:--|:--|
| Windows x64 | `win-x64` | `.zip` |
| Windows Arm64 | `win-arm64` | `.zip` |
| Linux x64 | `linux-x64` | `.tar.gz` |
| Alpine Linux x64 | `linux-musl-x64` | `.tar.gz` |

On Windows with PowerShell:

```powershell
Expand-Archive `
  -LiteralPath .\Expressif-<version>-net10.0-win-x64.zip `
  -DestinationPath "$env:LOCALAPPDATA\Expressif"

& "$env:LOCALAPPDATA\Expressif\expressif.exe" version
```

On Linux:

```bash
mkdir -p "$HOME/.local/share/expressif"
tar -xzf Expressif-<version>-net10.0-linux-x64.tar.gz \
  -C "$HOME/.local/share/expressif"
chmod +x "$HOME/.local/share/expressif/expressif"
```

Add the extraction directory to `PATH`, or create a symbolic link in a directory already on `PATH`. For Alpine Linux, use the `linux-musl-x64` archive.

## Verify the installation

```bash
expressif version
expressif --help
expressif evaluate --help
```

`expressif version` prints both the CLI version and the Expressif library version. Include both lines when reporting an issue because the versions can differ.

## Troubleshooting

If `expressif` is not found, open a new terminal and inspect command resolution:

```powershell
Get-Command expressif -All
```

```bash
command -v -a expressif
```

For a .NET global tool, verify that the user tool directory is on `PATH`: `%USERPROFILE%\.dotnet\tools` on Windows or `$HOME/.dotnet/tools` on Linux.

If the runtime is missing, inspect the installed runtimes:

```bash
dotnet --list-runtimes
```

Install `Microsoft.NETCore.App` 10.x for the executable architecture. The ASP.NET Core or Windows Desktop runtime alone is not a substitute.
