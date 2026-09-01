---
layout: docs
title: Windows installer
parent: Command-line interface
nav_order: 15
description: Install, automate, customize, and remove the Expressif CLI with the Windows setup executable.
---

The Windows setup executable installs Expressif for all users. It requires administrator privileges and a matching .NET 10 runtime for the selected architecture.

Download the `win-x64` or `win-arm64` installer from the [Expressif download page]({{ '/download/' | relative_url }}). Installer filenames follow this pattern:

```text
expressif-<version>-net10.0-<runtime>-setup.exe
```

## Interactive installation

Run the downloaded setup executable and follow the prompts. The installer:

- installs Expressif in `C:\Program Files\Expressif` by default;
- adds the installation directory to the system `PATH`;
- verifies that `Microsoft.NETCore.App` 10.x is installed for the selected architecture; and
- registers Expressif in **Settings > Apps > Installed apps**.

Open a new terminal after installation so it receives the updated `PATH`, then verify the installation:

```powershell
expressif version
```

## Silent installation

For deployment scripts, CI environments, and enterprise software distribution, use the following fully unattended invocation:

```powershell
& .\expressif-<version>-net10.0-<runtime>-setup.exe `
  /VERYSILENT `
  /SUPPRESSMSGBOXES `
  /NORESTART
```

`/VERYSILENT` hides both the setup wizard and its progress window. Use `/SILENT` instead when the wizard should remain hidden but installation progress should be visible.

`/SUPPRESSMSGBOXES` suppresses interactive message boxes while silent mode is active. `/NORESTART` prevents setup from restarting Windows. Expressif setup does not normally require a restart, but specifying the switch makes the deployment policy explicit.

The installer still requires elevation. When launched from a process that is not already elevated, Windows may display a User Account Control prompt; the Inno Setup silent switches do not suppress Windows elevation.

The required .NET runtime must be installed before Expressif. If it is missing, setup stops without installing the CLI. Install the x64 runtime for the `win-x64` package or the Arm64 runtime for the `win-arm64` package.

## Installation directory and logging

Use `/DIR` to select a different installation directory. Quote the complete argument when the path contains spaces:

```powershell
& .\expressif-<version>-net10.0-win-x64-setup.exe `
  /VERYSILENT `
  /SUPPRESSMSGBOXES `
  /NORESTART `
  '/DIR=C:\Tools\Expressif'
```

The selected directory is added to the system `PATH` and removed from it during uninstall.

Use `/LOG` to create a troubleshooting log with an automatically generated filename, or `/LOG=<filename>` to choose its location:

```powershell
& .\expressif-<version>-net10.0-win-x64-setup.exe `
  /VERYSILENT `
  /SUPPRESSMSGBOXES `
  /NORESTART `
  '/LOG=C:\Logs\expressif-setup.log'
```

The log can contain system paths and setup details. Review it before sharing it publicly.

For additional generic setup switches, see the [Inno Setup command-line parameters](https://jrsoftware.org/ishelp/topic_setupcmdline.htm). Options not described on this page are provided by Inno Setup and are not specific to Expressif.

## Uninstall

For an interactive uninstall, open **Settings > Apps > Installed apps**, locate **Expressif**, and select **Uninstall**.

For unattended removal, run the uninstaller from the installation directory:

```powershell
& 'C:\Program Files\Expressif\unins000.exe' `
  /VERYSILENT `
  /SUPPRESSMSGBOXES `
  /NORESTART
```

If Expressif was installed in a custom directory, run `unins000.exe` from that directory. The uninstaller removes the installed files and the Expressif entry from the system `PATH`.
