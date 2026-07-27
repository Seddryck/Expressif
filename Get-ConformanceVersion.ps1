[CmdletBinding()]
param(
    [string] $Configuration = "GitVersion.Conformance.yml",
    [string] $ConformancePath = "conformance",
    [switch] $Pack,
    [switch] $Tag
)

$ErrorActionPreference = "Stop"

# Prevent .NET first-run output from polluting captured JSON.
$env:DOTNET_NOLOGO = "true"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "true"

# Ensure remote release tags are available.
git fetch --tags --force

if ($LASTEXITCODE -ne 0) {
    throw "Unable to fetch Git tags."
}

$currentYear = [int](Get-Date -Format "yyyy")
$currentMonth = [int](Get-Date -Format "MM")

$calendarBaseVersion = "$currentYear.$currentMonth.0"

# Find the latest conformance release tag.
$tagPattern =
    "^conformance-(?<year>\d{4})\.(?<month>\d{1,2})\.(?<patch>\d+)$"

$latestRelease =
    git tag --list "conformance-*.*.*" |
    ForEach-Object {
        if ($_ -match $tagPattern) {
            [PSCustomObject]@{
                Tag = $_
                Version = [version]::new(
                    [int] $Matches.year,
                    [int] $Matches.month,
                    [int] $Matches.patch
                )
            }
        }
    } |
    Sort-Object Version -Descending |
    Select-Object -First 1

$startsNewCalendarMonth =
    $null -eq $latestRelease -or
    $latestRelease.Version.Major -lt $currentYear -or
    (
        $latestRelease.Version.Major -eq $currentYear -and
        $latestRelease.Version.Minor -lt $currentMonth
    )

$arguments = @(
    "gitversion"
    "/config"
    $Configuration
    "/output"
    "json"
)

if ($startsNewCalendarMonth) {
    Write-Host "Starting conformance release cycle $currentYear.$currentMonth"

    $arguments += @(
        "/overrideconfig"
        "next-version=$calendarBaseVersion"
    )
}
else {
    Write-Host "Continuing after $($latestRelease.Tag)"
}

Write-Host "Running: dotnet $($arguments -join ' ')"

$gitVersionOutput = & dotnet @arguments 2>&1
$gitVersionExitCode = $LASTEXITCODE

if ($gitVersionExitCode -ne 0) {
    $errorOutput =
        $gitVersionOutput -join [Environment]::NewLine

    throw @"
GitVersion failed with exit code $gitVersionExitCode.

Command:
dotnet $($arguments -join ' ')

Output:
$errorOutput
"@
}

$gitVersionJson =
    $gitVersionOutput -join [Environment]::NewLine

try {
    $gitVersion = $gitVersionJson | ConvertFrom-Json
}
catch {
    throw @"
GitVersion returned invalid JSON.

Output:
$gitVersionJson
"@
}

$conformanceVersion = $gitVersion.SemVer

if ([string]::IsNullOrWhiteSpace($conformanceVersion)) {
    throw "GitVersion returned an empty SemVer value."
}

Write-Host "Conformance version: $conformanceVersion"

if ($Pack) {
    $sevenZip = Get-Command "7z" -ErrorAction SilentlyContinue

    if ($null -eq $sevenZip) {
        throw "7-Zip was not found. Ensure that '7z' is available in PATH."
    }

    if (-not (Test-Path $ConformancePath -PathType Container)) {
        throw "Conformance directory '$ConformancePath' does not exist."
    }

    $archiveDirectory = Join-Path $ConformancePath "bin"
    $archiveName = "Expressif.Conformance.$conformanceVersion.zip"
    $archivePath = Join-Path $archiveDirectory $archiveName

    New-Item `
        -ItemType Directory `
        -Path $archiveDirectory `
        -Force |
        Out-Null

    $archivePath = [System.IO.Path]::GetFullPath($archivePath)

    if (Test-Path $archivePath) {
        Remove-Item $archivePath -Force
    }

    Push-Location $ConformancePath

    try {
        & $sevenZip.Source a `
            -tzip `
            $archivePath `
            ".\*" `
            "-xr!bin"

        if ($LASTEXITCODE -ne 0) {
            throw "7-Zip failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }

    Write-Host "Created archive: $archivePath"
}

# Set environment variable for use in subsequent build steps.
$tagName = "conformance-$conformanceVersion"
$env:GitVersion_Conformance_SemVer = $tagName

Write-Host (
    "Environment variable GitVersion_Conformance_SemVer: {0}" -f
    $env:GitVersion_Conformance_SemVer
)

if ($Tag) {
    . "$env:APPVEYOR_BUILD_FOLDER\github.ps1"
    $semVer = $env:GitVersion_Conformance_SemVer

    # Skip if conformance version is not set (e.g., no modification to conformance/)
    if ([string]::IsNullOrWhiteSpace($semVer)) {
        Write-Host "Conformance version not set. Conformance tagging skipped."
        return
    }

    # Skip pre-release tags
    if ($semVer -match '-') {
        Write-Host "Pre-release detected ($semVer). Conformance tagging skipped."
        return
    }

    if ([string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_NAME)) {
        throw "APPVEYOR_REPO_NAME is empty (expected owner/repo)."
    }
    $owner, $repository = $env:APPVEYOR_REPO_NAME.Split('/')

    $context = [PSCustomObject]@{
        Owner       = $owner
        Repository  = $repository
        SecretToken = $env:GITHUB_TOKEN
    }

    # Tag the exact commit currently built by AppVeyor
    $context | Publish-Commit-Tag -Tag $semVer -CommitSha $env:APPVEYOR_REPO_COMMIT
}

# Return the version to the caller.
$conformanceVersion
