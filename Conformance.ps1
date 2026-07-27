$ErrorActionPreference = "Stop"

# Prevent .NET first-run output from polluting captured JSON.
$env:DOTNET_NOLOGO = "true"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "true"

$script:ConformanceVersionCache = $null

function Update-ConformanceTags {
    [CmdletBinding()]
    param()

    $isShallowRepository = & git rev-parse --is-shallow-repository 2>$null

    if ($LASTEXITCODE -eq 0 -and $isShallowRepository -match '^true$') {
        git fetch --unshallow --tags --force
    }
    else {
        git fetch --tags --force
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Unable to fetch Git tags."
    }
}

function Resolve-MainReference {
    [CmdletBinding()]
    param()

    foreach ($candidateMainReference in @("origin/main", "main")) {
        & git rev-parse --verify --quiet $candidateMainReference *> $null

        if ($LASTEXITCODE -eq 0) {
            return $candidateMainReference
        }
    }

    return $null
}

function Get-ConformanceVersion {
    [CmdletBinding()]
    param(
        [string] $Configuration = "GitVersion.Conformance.yml",
        [string] $ConformancePath = "conformance",
        [switch] $Warn,
        [switch] $Refresh,
        [switch] $NoEnv
    )

    Write-Host "=== Calculating conformance version ==="

    if (-not $Refresh -and -not [string]::IsNullOrWhiteSpace($script:ConformanceVersionCache)) {
        $conformanceVersion = $script:ConformanceVersionCache

        Write-Host "Conformance version (cached): $conformanceVersion"

        if (-not $NoEnv) {
            $env:GitVersion_Conformance_SemVer = "conformance-$conformanceVersion"

            Write-Host (
                "Environment variable GitVersion_Conformance_SemVer: {0}" -f
                $env:GitVersion_Conformance_SemVer
            )
        }

        return $conformanceVersion
    }

    Update-ConformanceTags

    $currentYear = [int](Get-Date -Format "yyyy")
    $currentMonth = [int](Get-Date -Format "MM")
    $calendarBaseVersion = "$currentYear.$currentMonth.0"
    $tagPattern =
        "^conformance-(?<year>\d{4})\.(?<month>\d{1,2})\.(?<patch>\d+)$"

    $mainReference = Resolve-MainReference

    if ($null -ne $mainReference) {
        $conformanceTags =
            git tag --merged $mainReference --list "conformance-*.*.*"

        if ($Warn) {
            $unreachableConformanceTags =
                git tag --no-merged $mainReference --list "conformance-*.*.*"

            if ($unreachableConformanceTags) {
                Write-Warning (
                    "Ignoring conformance tags not reachable from " +
                    "${mainReference}:"
                )

                $unreachableConformanceTags |
                    Sort-Object |
                    ForEach-Object {
                        Write-Warning " - $_"
                    }
            }
        }
    }
    else {
        $conformanceTags =
            git tag --list "conformance-*.*.*"

        if ($Warn) {
            Write-Warning (
                "Could not resolve 'origin/main' or 'main'. " +
                "Falling back to all conformance tags."
            )
        }
    }

    $latestRelease =
        $conformanceTags |
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

        # Keep GitVersion anchored to the latest conformance tag even when
        # repository history in CI is incomplete (for example, shallow fetch).
        $arguments += @(
            "/overrideconfig"
            "next-version=$($latestRelease.Version.ToString())"
        )
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

    $script:ConformanceVersionCache = $conformanceVersion

    Write-Host "Conformance version: $conformanceVersion"

    if (-not $NoEnv) {
        $env:GitVersion_Conformance_SemVer = "conformance-$conformanceVersion"

        Write-Host (
            "Environment variable GitVersion_Conformance_SemVer: {0}" -f
            $env:GitVersion_Conformance_SemVer
        )
    }

    return $conformanceVersion
}

function Package-Conformance {
    [CmdletBinding()]
    param(
        [string] $ConformancePath = "conformance",
        [string] $Version = "0.0.0"
    )

    Write-Host "=== Packaging conformance ==="

    $conformanceVersion =
        if ([string]::IsNullOrWhiteSpace($Version)) { "0.0.0" } else { $Version.Trim() }

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

function Tag-Conformance {
    [CmdletBinding()]
    param(
        [string] $ConformancePath = "conformance",
        [string] $Version = "0.0.0",
        [switch] $Warn,
        [switch] $Force
    )

    Write-Host "=== Tagging conformance ==="

    . "$PSScriptRoot/github.ps1"

    $conformanceVersion =
        if ([string]::IsNullOrWhiteSpace($Version)) { "0.0.0" } else { $Version.Trim() }

    $semVer = "conformance-$conformanceVersion"

    $taggedCommit = if (
        -not [string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_COMMIT)
    ) {
        $env:APPVEYOR_REPO_COMMIT
    }
    else {
        (& git rev-parse HEAD 2>$null)
    }

    $taggedCommit =
        if ($null -eq $taggedCommit) { "" } else { $taggedCommit.Trim() }

    $changedConformanceFiles = @()

    if (-not [string]::IsNullOrWhiteSpace($taggedCommit)) {
        $changedConformanceFiles = @(
            git diff-tree --no-commit-id --name-only -r $taggedCommit -- conformance/ |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        )
    }

    if ($changedConformanceFiles.Count -eq 0) {
        Write-Host (
            "No changes under conformance/ for commit '$taggedCommit'. " +
            "Conformance tagging skipped."
        )
        return
    }

    Write-Host "Conformance changes detected for tagging:"
    $changedConformanceFiles |
        Sort-Object |
        ForEach-Object {
            Write-Host " - $_"
        }

    if ($conformanceVersion -match '-') {
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
        SecretToken = $env:github_access_token
    }

    $mainReference = Resolve-MainReference

    if ($null -eq $mainReference) {
        $mainReference = "HEAD"
    }

    $tagReference = "refs/tags/$semVer"
    & git show-ref --verify --quiet $tagReference

    if ($LASTEXITCODE -eq 0) {
        $tagCommit = (& git rev-list -n 1 $tagReference 2>$null)
        $tagCommit = if ($null -eq $tagCommit) { "" } else { $tagCommit.Trim() }
        $mainStatus = "unknown"

        if (-not [string]::IsNullOrWhiteSpace($tagCommit)) {
            & git merge-base --is-ancestor $tagCommit $mainReference 2>$null

            $mainStatus = if ($LASTEXITCODE -eq 0) {
                "on $mainReference"
            }
            else {
                "not on $mainReference"
            }
        }

        if ($Force) {
            Write-Warning (
                "Conformance tag '$semVer' already exists at commit '$tagCommit' and is $mainStatus. " +
                "Removing existing tag because -Force was specified."
            )

            & git tag -d $semVer *> $null

            $remoteTagExists =
                @(
                    git ls-remote --tags origin $tagReference
                ).Count -gt 0

            if ($remoteTagExists) {
                & git push origin ":$tagReference"

                if ($LASTEXITCODE -ne 0) {
                    throw "Failed to delete remote tag '$semVer' from origin."
                }
            }

            Update-ConformanceTags
        }
        else {
            throw (
                "Conformance tag '$semVer' already exists at commit '$tagCommit' and is $mainStatus. " +
                "Refusing to create an existing tag. Use -Force to replace it."
            )
        }
    }

    Write-Host "Tagging conformance release '$semVer' for $($context.Owner)/$($context.Repository) on Commit SHA $taggedCommit"
    $context | Publish-Commit-Tag -Tag $semVer -CommitSha $taggedCommit
}
