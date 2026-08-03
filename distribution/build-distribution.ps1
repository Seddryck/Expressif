[CmdletBinding()]
param(
    [ValidateSet(
        "Build",
        "Publish",
        "Archive",
        "Bundle",
        "Pack",
        "Distribute"
    )]
    [string] $Mode = "Distribute",

    [string] $Project =
        "../Expressif.Cli/Expressif.Cli.csproj",

    [string] $InstallerScript =
        "./Expressif.iss",

    [string] $Version = $(
        if ([string]::IsNullOrWhiteSpace($env:GitVersion_SemVer)) {
            "0.0.0-local"
        } else {
            $env:GitVersion_SemVer
        }
    ),

    [string[]] $Frameworks = @(
        "net8.0",
        "net9.0",
        "net10.0"
    ),

    [string[]] $Runtimes = @(
        "win-x64",
        "win-arm64",
        "linux-x64",
        "linux-musl-x64"
    ),

    [string] $Configuration =
        "Release",

    [string] $OutputRoot =
        "./bin",

    [string] $SevenZip =
        "7z",

    [string] $InnoSetupCompiler =
        "ISCC.exe"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Distribution identities
# ---------------------------------------------------------------------------

# Build/publish identity:
#   Expressif.Cli.exe
#   Expressif.Cli.dll
#   Expressif.Cli.deps.json
#   Expressif.Cli.runtimeconfig.json
$BuildIdentity = "Expressif.Cli"

# Command exposed to users in ZIP archives and installers.
$CommandName = "expressif"

# NuGet package identity:
#   Expressif-cli.<version>.nupkg
$PackageId = "Expressif-cli"

# Distribution archive/installer prefix:
#   Expressif-2026.7.0-net10.0-win-x64.zip
$DistributionName = "Expressif"

$Project = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot $Project)
)

$InstallerScript = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot $InstallerScript)
)

$OutputRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot $OutputRoot)
)

$PublishRoot = Join-Path $OutputRoot "publish"
$ArchiveRoot = Join-Path $OutputRoot "archives"
$InstallerRoot = Join-Path $OutputRoot "installers"
$PackageRoot = Join-Path $OutputRoot "packages"
$StagingRoot = Join-Path $PSScriptRoot "obj"

Write-Host "=== Distribution Build Script ==="
Write-Host "Distribution mode: $Mode"

if ($Version -eq "0.0.0-local") {
    Write-Warning "No version was provided. Using local version '$Version'."
}

# ---------------------------------------------------------------------------
# General helpers
# ---------------------------------------------------------------------------

function Write-Step {
    param(
        [Parameter(Mandatory)]
        [string] $Message
    )

    Write-Host
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Invoke-ExternalCommand {
    param(
        [Parameter(Mandatory)]
        [string] $Command,

        [Parameter()]
        [string[]] $Arguments = @()
    )

    Write-Host "> $Command $($Arguments -join ' ')" -ForegroundColor DarkGray

    & $Command @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "Command '$Command' failed with exit code $LASTEXITCODE."
    }
}

function Assert-FileExists {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter()]
        [string] $Description = "Required file"
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$Description does not exist: $Path"
    }
}

function Assert-DirectoryExists {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter()]
        [string] $Description = "Required directory"
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "$Description does not exist: $Path"
    }
}

function New-CleanDirectory {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    if (Test-Path -LiteralPath $Path) {
        Remove-Item -LiteralPath $Path -Recurse -Force
    }

    New-Item -ItemType Directory -Path $Path -Force | Out-Null
}

function Get-PublishDirectory {
    param(
        [Parameter(Mandatory)]
        [string] $Framework,

        [Parameter(Mandatory)]
        [string] $Runtime
    )

    return Join-Path $PublishRoot "$Framework/$Runtime"
}

function Get-ArchiveName {
    param(
        [Parameter(Mandatory)]
        [string] $Framework,

        [Parameter(Mandatory)]
        [string] $Runtime
    )

    return "$DistributionName-$Version-$Framework-$Runtime.zip"
}

function Get-InstallerName {
    param(
        [Parameter(Mandatory)]
        [string] $Framework,

        [Parameter(Mandatory)]
        [string] $Runtime
    )

    return "$DistributionName-$Version-$Framework-$Runtime-setup"
}

function Get-InstallerArchitecture {
    param(
        [Parameter(Mandatory)]
        [string] $Runtime
    )

    switch ($Runtime) {
        "win-x64" {
            return "x64compatible"
        }

        "win-arm64" {
            return "arm64"
        }

        default {
            throw "No Inno Setup architecture mapping exists for runtime '$Runtime'."
        }
    }
}

function Resolve-Version {
    if ([string]::IsNullOrWhiteSpace($Version)) {
        $script:Version = "0.0.0-local"

        Write-Warning `
            "No version was provided. Using local version '$Version'."
    }
}

function Assert-Project {
    Assert-FileExists `
        -Path $Project `
        -Description "CLI project"
}

function Get-ExecutableFileName {
    param(
        [Parameter(Mandatory)]
        [string] $Runtime,

        [Parameter(Mandatory)]
        [string] $Name
    )

    if ($Runtime.StartsWith(
        "win-",
        [System.StringComparison]::OrdinalIgnoreCase
    )) {
        return "$Name.exe"
    }

    return $Name
}

# ---------------------------------------------------------------------------
# Build
# ---------------------------------------------------------------------------

function Invoke-Build {
    Write-Step "Building $BuildIdentity"

    foreach ($framework in $Frameworks) {
        Invoke-ExternalCommand `
            -Command "dotnet" `
            -Arguments @(
                "build",
                $Project,
                "--configuration", $Configuration,
                "--framework", $framework,
                "-p:version=$Version"
            )
    }
}

# ---------------------------------------------------------------------------
# Publish
# ---------------------------------------------------------------------------

function Invoke-Publish {
    param(
        [switch] $NoBuild
    )

    foreach ($framework in $Frameworks) {
        foreach ($runtime in $Runtimes) {
            $publishDirectory = Get-PublishDirectory `
                -Framework $framework `
                -Runtime $runtime

            Write-Step "Publishing $framework / $runtime"

            New-CleanDirectory -Path $publishDirectory

            $arguments = @(
                "publish",
                $Project,
                "--configuration", $Configuration,
                "--framework", $framework,
                "--runtime", $runtime,
                "--output", $publishDirectory,
                "-p:version=$Version",
                "--no-self-contained"
            )

            if ($NoBuild) {
                $arguments += "--no-build"
            }

            Invoke-ExternalCommand `
                -Command "dotnet" `
                -Arguments $arguments

            $publishedExecutableName = Get-ExecutableFileName `
                -Runtime $runtime `
                -Name $BuildIdentity

            $publishedExecutable = Join-Path `
                $publishDirectory `
                $publishedExecutableName

            Assert-FileExists `
                -Path $publishedExecutable `
                -Description "Published executable"
        }
    }
}

# ---------------------------------------------------------------------------
# Archive
# ---------------------------------------------------------------------------

function Invoke-Archive {
    New-Item `
        -ItemType Directory `
        -Path $ArchiveRoot `
        -Force |
        Out-Null

    New-Item `
        -ItemType Directory `
        -Path $StagingRoot `
        -Force |
        Out-Null

    foreach ($framework in $Frameworks) {
        foreach ($runtime in $Runtimes) {
            $publishDirectory = Get-PublishDirectory `
                -Framework $framework `
                -Runtime $runtime

            Assert-DirectoryExists `
                -Path $publishDirectory `
                -Description "Publish directory"

            $publishedExecutableName = Get-ExecutableFileName `
                -Runtime $runtime `
                -Name $BuildIdentity

            $publishedExecutable = Join-Path `
                $publishDirectory `
                $publishedExecutableName

            Assert-FileExists `
                -Path $publishedExecutable `
                -Description "Published executable"

            $stagingDirectory = Join-Path `
                $StagingRoot `
                "$framework-$runtime"

            if ($runtime.StartsWith(
                "linux-",
                [System.StringComparison]::OrdinalIgnoreCase
            )) {
                $archiveBaseName =
                    "$DistributionName-$Version-$framework-$runtime"

                $tarPath = Join-Path `
                    $ArchiveRoot `
                    "$archiveBaseName.tar"

                $archivePath = "$tarPath.gz"
            } else {
                $archiveName =
                    "$DistributionName-$Version-$framework-$runtime.zip"

                $archivePath = Join-Path `
                    $ArchiveRoot `
                    $archiveName
            }

            Write-Step "Creating archive $archiveName"

            New-CleanDirectory -Path $stagingDirectory

            Copy-Item `
                -Path (Join-Path $publishDirectory "*") `
                -Destination $stagingDirectory `
                -Recurse `
                -Force

            $stagedExecutable = Join-Path `
                $stagingDirectory `
                $publishedExecutableName

            $commandExecutableName = Get-ExecutableFileName `
                -Runtime $runtime `
                -Name $CommandName

            $renamedExecutable = Join-Path `
                $stagingDirectory `
                $commandExecutableName

            Move-Item `
                -LiteralPath $stagedExecutable `
                -Destination $renamedExecutable `
                -Force

            if (Test-Path -LiteralPath $archivePath) {
                Remove-Item `
                    -LiteralPath $archivePath `
                    -Force
            }

            if ($runtime.StartsWith(
                "linux-",
                [System.StringComparison]::OrdinalIgnoreCase
            )) {
                Remove-Item `
                    -LiteralPath $tarPath, $archivePath `
                    -Force `
                    -ErrorAction SilentlyContinue

                # Add the Linux command with executable permissions: rwxr-xr-x.
                Invoke-ExternalCommand `
                    -Command "tar" `
                    -Arguments @(
                        "--create",
                        "--file", $tarPath,
                        "--directory", $stagingDirectory,
                        #"--mode=755",
                        $commandExecutableName
                    )

                # Add all remaining files.
                #
                # u+rwX,go+rX gives:
                #   regular files: rw-r--r--
                #   directories:   rwxr-xr-x
                Invoke-ExternalCommand `
                    -Command "tar" `
                    -Arguments @(
                        "--append",
                        "--file", $tarPath,
                        "--directory", $stagingDirectory,
                        "--exclude=$commandExecutableName",
                        #"--mode=u+rwX,go+rX",
                        "."
                    )

                # Compress archive.tar into archive.tar.gz.
                Invoke-ExternalCommand `
                    -Command $SevenZip `
                    -Arguments @(
                        "a",
                        "-tgzip",
                        "-mx=9",
                        $archivePath,
                        $tarPath
                    )

                Remove-Item `
                    -LiteralPath $tarPath `
                    -Force
            }
            else {
                Remove-Item `
                    -LiteralPath $archivePath `
                    -Force `
                    -ErrorAction SilentlyContinue

                Invoke-ExternalCommand `
                    -Command $SevenZip `
                    -Arguments @(
                        "a",
                        "-tzip",
                        "-mx=9",
                        $archivePath,
                        (Join-Path $stagingDirectory "*")
                    )
            }

            Assert-FileExists `
                -Path $archivePath `
                -Description "Generated archive"
        }
    }
}

# ---------------------------------------------------------------------------
# Bundle
# ---------------------------------------------------------------------------

function Invoke-Bundle {
    Assert-FileExists `
        -Path $InstallerScript `
        -Description "Inno Setup script"

    New-Item `
        -ItemType Directory `
        -Path $InstallerRoot `
        -Force |
        Out-Null

    $framework = $Frameworks[-1]
    foreach ($runtime in $Runtimes | Where-Object { $_ -notlike "linux-*" }) {
        $publishDirectory = Get-PublishDirectory `
            -Framework $framework `
            -Runtime $runtime   
        Assert-DirectoryExists `
            -Path $publishDirectory `
            -Description "Publish directory"    
        $publishedExecutable = Join-Path `
            $publishDirectory `
            "$BuildIdentity.exe"    
        Assert-FileExists `
            -Path $publishedExecutable `
            -Description "Published executable" 
        $targetArchitecture = Get-InstallerArchitecture `
            -Runtime $runtime   
        $installerName = Get-InstallerName `
            -Framework $framework `
            -Runtime $runtime   
        Write-Step "Bundling installer $installerName.exe"  
        # Bundle deliberately does not build or publish.
        #
        # The Inno Setup script receives both names:
        #
        # BuildIdentity = Expressif-cli
        # CommandName   = expressif
        #
        # It can therefore install Expressif-cli.exe as expressif.exe
        # without changing the files in bin/publish.    
        Invoke-ExternalCommand `
            -Command $InnoSetupCompiler `
            -Arguments @(
                "/DAppVersion=$Version",
                "/DTargetFramework=$framework",
                "/DRuntimeIdentifier=$runtime",
                "/DTargetArchitecture=$targetArchitecture",
                "/DBuildIdentity=$BuildIdentity",
                "/DCommandName=$CommandName",
                "/DPublishDirectory=$publishDirectory",
                "/DOutputDirectory=$InstallerRoot",
                "/DOutputBaseFilename=$installerName",
                $InstallerScript
            )   
        $installerPath = Join-Path `
            $InstallerRoot `
            "$installerName.exe"    
            Assert-FileExists `
                -Path $installerPath `
                -Description "Generated installer"
    }
}

# ---------------------------------------------------------------------------
# NuGet pack
# ---------------------------------------------------------------------------

function Invoke-Pack {
    param(
        [switch] $NoBuild
    )

    Write-Step "Packing NuGet package $PackageId"

    New-Item `
        -ItemType Directory `
        -Path $PackageRoot `
        -Force |
        Out-Null

    $arguments = @(
        "pack",
        $Project,
        "--configuration", $Configuration,
        "--output", $PackageRoot,
        "-p:PackageVersion=$Version",
        "-p:Version=$Version"
    )

    if ($NoBuild) {
        $arguments += "--no-build"
    }

    Invoke-ExternalCommand `
        -Command "dotnet" `
        -Arguments $arguments

    $expectedPackage = Join-Path `
        $PackageRoot `
        "$PackageId.$Version.nupkg"

    Assert-FileExists `
        -Path $expectedPackage `
        -Description "Generated NuGet package"
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

Assert-Project

New-Item `
    -ItemType Directory `
    -Path $OutputRoot `
    -Force |
    Out-Null

switch ($Mode) {
    "Build" {
        Invoke-Build
    }

    "Publish" {
        # dotnet publish builds automatically.
        Invoke-Publish
    }

    "Archive" {
        # Archive consumes existing publish output.
        # It never invokes dotnet publish.
        Invoke-Archive
    }

    "Bundle" {
        # Bundle consumes existing publish output.
        # It never invokes dotnet build or dotnet publish.
        Invoke-Bundle
    }

    "Pack" {
        # A standalone Pack command builds when needed.
        #
        # No executable is renamed inside the NuGet package.
        Invoke-Pack
    }

    "Distribute" {
        # Build once, then reuse those outputs where possible.
        Invoke-Publish
        Invoke-Archive
        Invoke-Bundle
        Invoke-Pack -NoBuild
    }

    default {
        throw "Unsupported mode '$Mode'."
    }
}

if (Test-Path -LiteralPath $StagingRoot) {
    Remove-Item `
        -LiteralPath $StagingRoot `
        -Recurse `
        -Force
}

Write-Host
Write-Host "Distribution operation '$Mode' completed." -ForegroundColor Green
Write-Host "Output: $OutputRoot"