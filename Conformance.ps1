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

function Install-ConformanceDependencies {
    [CmdletBinding()]
    param()

    $moduleName = "powershell-yaml"

    if (-not (Get-Module -ListAvailable -Name $moduleName)) {
        Write-Host "Installing PowerShell module '$moduleName'..."
        Install-Module $moduleName -Scope CurrentUser -Force -AllowClobber -ErrorAction Stop
    }

    if (-not (Get-Module -ListAvailable -Name $moduleName)) {
        throw "PowerShell module '$moduleName' is not available after installation."
    }

    Write-Host "PowerShell module '$moduleName' is available."
}

function Import-YamlSupport {
    [CmdletBinding()]
    param()

    $moduleName = "powershell-yaml"

    if (-not (Get-Module -ListAvailable -Name $moduleName)) {
        throw @"
PowerShell module '$moduleName' is required.
Install it with:
Install-ConformanceDependencies
or:
Install-Module $moduleName -Scope CurrentUser
"@
    }

    Import-Module $moduleName -ErrorAction Stop
}

function Convert-GlobPatternToRegex {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $Pattern
    )

    $normalized = $Pattern.Replace('\', '/').Trim()
    if ([string]::IsNullOrWhiteSpace($normalized)) {
        return $null
    }

    $escaped = [Regex]::Escape($normalized)
    $escaped = $escaped.Replace('\*\*', '__DOUBLE_STAR__')
    $escaped = $escaped.Replace('\*', '[^/]*')
    $escaped = $escaped.Replace('\?', '[^/]')
    $escaped = $escaped.Replace('__DOUBLE_STAR__', '.*')

    return "^$escaped$"
}

function Test-ConformanceExclude {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $RelativePath,

        [Parameter(Mandatory)]
        [string[]] $Exclude
    )

    $normalizedPath = $RelativePath.Replace('\', '/').TrimStart('/')
    $fileName = [System.IO.Path]::GetFileName($normalizedPath)

    foreach ($rawPattern in $Exclude) {
        if ([string]::IsNullOrWhiteSpace($rawPattern)) {
            continue
        }

        $pattern = $rawPattern.Replace('\', '/').Trim()
        $isRootOnly = $pattern.StartsWith('/')

        if ($isRootOnly) {
            $pattern = $pattern.TrimStart('/')

            if ($normalizedPath.Contains('/')) {
                continue
            }

            $regex = Convert-GlobPatternToRegex -Pattern $pattern
            if ($null -ne $regex -and $normalizedPath -match $regex) {
                return $true
            }

            continue
        }

        if ($pattern.Contains('/')) {
            $regex = Convert-GlobPatternToRegex -Pattern $pattern
            if ($null -ne $regex -and $normalizedPath -match $regex) {
                return $true
            }

            continue
        }

        $regex = Convert-GlobPatternToRegex -Pattern $pattern
        if ($null -ne $regex -and $fileName -match $regex) {
            return $true
        }
    }

    return $false
}

function Get-ConformanceEffectiveExclude {
    [CmdletBinding()]
    param(
        [string[]] $Exclude
    )

    return @(
        $Exclude |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Write-ConformanceScope {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $Title,

        [Parameter(Mandatory)]
        [string] $InputPath,

        [Parameter(Mandatory)]
        [string] $ResolvedPath,

        [Parameter(Mandatory)]
        [string[]] $Exclude
    )

    Write-Host ("{0} scope:" -f $Title) -ForegroundColor DarkCyan
    Write-Host ("  input-path    : '{0}'" -f $InputPath) -ForegroundColor DarkCyan
    Write-Host ("  resolved-path : '{0}'" -f $ResolvedPath) -ForegroundColor DarkCyan

    if ($Exclude.Count -gt 0) {
        Write-Host ("  exclusions    : {0} pattern(s)" -f $Exclude.Count) -ForegroundColor DarkCyan

        foreach ($pattern in $Exclude) {
            Write-Host ("    - {0}" -f $pattern) -ForegroundColor DarkCyan
        }
    }
    else {
        Write-Host "  exclusions    : <none>" -ForegroundColor DarkCyan
    }
}

function Validate-Conformance {
    [CmdletBinding()]
    param(
        [string] $Path = "conformance",
        [string[]] $Exclude = @("bin/**", "/*.yaml", "/*.yml"),
        [switch] $FailOnError
    )

    Write-Host "=== Validation ===" -ForegroundColor Cyan

    $resolvedPath = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path

    if (-not (Test-Path -LiteralPath $resolvedPath -PathType Container)) {
        throw "Conformance directory '$Path' does not exist."
    }

    $schemaPath = Join-Path $resolvedPath "conformance.schema.json"

    if (-not (Test-Path -LiteralPath $schemaPath -PathType Leaf)) {
        throw "Conformance schema file '$schemaPath' was not found."
    }

    Import-YamlSupport

    $npx = Get-Command "npx" -ErrorAction SilentlyContinue

    if ($null -eq $npx) {
        throw "'npx' was not found. Install Node.js to validate YAML files against the JSON schema."
    }

    $allYamlFiles = @(
        Get-ChildItem `
            -LiteralPath $resolvedPath `
            -Recurse `
            -File |
        Where-Object {
            $_.Extension -in @(".yaml", ".yml")
        }
    )

    $candidateFiles = @(
        $allYamlFiles |
        Where-Object {
            $relativePath = [System.IO.Path]::GetRelativePath(
                $resolvedPath,
                $_.FullName
            ).Replace('\', '/')

            -not (Test-ConformanceExclude -RelativePath $relativePath -Exclude $Exclude)
        }
    )

    $testedCount = $candidateFiles.Count
    $excludedCount = $allYamlFiles.Count - $testedCount
    $schemaFailureCount = 0
    $duplicateTestIdCount = 0
    $duplicateCaseIdCount = 0
    $validatedTestIdCount = 0
    $validatedCaseIdCount = 0

    $selectionRate = if ($allYamlFiles.Count -eq 0) {
        "0.00"
    }
    else {
        (($testedCount / [double]$allYamlFiles.Count) * 100).ToString(
            "F2",
            [System.Globalization.CultureInfo]::InvariantCulture
        )
    }

    $effectiveExclude = Get-ConformanceEffectiveExclude -Exclude $Exclude

    Write-ConformanceScope `
        -Title "Validation" `
        -InputPath $Path `
        -ResolvedPath $resolvedPath `
        -Exclude $effectiveExclude

    Write-Host "Validation discovery:" -ForegroundColor DarkCyan
    Write-Host ("  yaml-found     : {0}" -f $allYamlFiles.Count) -ForegroundColor DarkCyan
    Write-Host ("  yaml-selected  : {0}" -f $testedCount) -ForegroundColor DarkCyan
    Write-Host ("  yaml-excluded  : {0}" -f $excludedCount) -ForegroundColor DarkCyan
    Write-Host ("  selection-rate : {0}%" -f $selectionRate) -ForegroundColor DarkCyan

    $entriesToValidate = @()
    $parsedEntries = @()
    $tempDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("expressif-conformance-" + [Guid]::NewGuid().ToString("N"))

    New-Item -ItemType Directory -Path $tempDirectory -Force | Out-Null

    if ($testedCount -eq 0) {
        Write-Warning "No conformance YAML files were considered for validation in '$resolvedPath'."
    }

    try {
        $index = 0

        foreach ($file in $candidateFiles) {
            $relativePath = [System.IO.Path]::GetRelativePath(
                $resolvedPath,
                $file.FullName
            ).Replace('\', '/')

            try {
                $yamlText = Get-Content -LiteralPath $file.FullName -Raw

                if ([string]::IsNullOrWhiteSpace($yamlText)) {
                    throw "YAML file is empty."
                }

                $yamlDocument = ConvertFrom-Yaml -Yaml $yamlText
                $jsonDocument = $yamlDocument | ConvertTo-Json -Depth 100
                $jsonPath = Join-Path $tempDirectory ("case-{0:0000}.json" -f $index)

                Set-Content -LiteralPath $jsonPath -Value $jsonDocument -Encoding UTF8

                $entriesToValidate += [PSCustomObject]@{
                    RelativePath = $relativePath
                    JsonPath      = [System.IO.Path]::GetFullPath($jsonPath)
                }

                $parsedEntries += [PSCustomObject]@{
                    RelativePath = $relativePath
                    Document     = $yamlDocument
                }

                $index++
            }
            catch {
                $schemaFailureCount++
                Write-Warning "Schema validation failed for '$relativePath'."
            }
        }

        $chunkSize = 40

        for ($offset = 0; $offset -lt $entriesToValidate.Count; $offset += $chunkSize) {
            $chunk = @($entriesToValidate | Select-Object -Skip $offset -First $chunkSize)
            $statusByJsonPath = @{}

            foreach ($entry in $chunk) {
                $statusByJsonPath[$entry.JsonPath] = $false
            }

            $arguments = @(
                "--yes",
                "ajv-cli",
                "validate",
                "--spec=draft2020",
                "-s",
                $schemaPath
            )

            foreach ($entry in $chunk) {
                $arguments += @("-d", $entry.JsonPath)
            }

            $validationOutput = @(& $npx.Source @arguments 2>&1)

            foreach ($line in $validationOutput) {
                $text = "$line"

                if ($text -match '^(?<path>.+?)\s+(?<result>valid|invalid)$') {
                    $jsonPath = [System.IO.Path]::GetFullPath($Matches.path.Trim())
                    $statusByJsonPath[$jsonPath] = ($Matches.result -eq "valid")
                }
            }

            foreach ($entry in $chunk) {
                if (-not $statusByJsonPath[$entry.JsonPath]) {
                    $schemaFailureCount++
                    Write-Warning "Schema validation failed for '$($entry.RelativePath)'."
                }
            }
        }

        $seenTestIds = @{}
        $seenCaseIds = @{}

        foreach ($entry in $parsedEntries) {
            $tests = @($entry.Document.tests)

            foreach ($test in $tests) {
                if ($null -eq $test) {
                    continue
                }

                $testId = [string]$test.id
                if (-not [string]::IsNullOrWhiteSpace($testId)) {
                    $validatedTestIdCount++

                    if ($seenTestIds.ContainsKey($testId)) {
                        $duplicateTestIdCount++
                        Write-Warning (
                            "Duplicate test id '{0}' first seen in '{1}', duplicated in '{2}'." -f
                            $testId,
                            $seenTestIds[$testId],
                            $entry.RelativePath
                        )
                    }
                    else {
                        $seenTestIds[$testId] = $entry.RelativePath
                    }
                }

                $cases = @($test.cases)
                foreach ($case in $cases) {
                    if ($null -eq $case) {
                        continue
                    }

                    $caseId = [string]$case.id
                    if (-not [string]::IsNullOrWhiteSpace($caseId)) {
                        $validatedCaseIdCount++

                        if ($seenCaseIds.ContainsKey($caseId)) {
                            $duplicateCaseIdCount++
                            Write-Warning (
                                "Duplicate case id '{0}' first seen in '{1}', duplicated in '{2}'." -f
                                $caseId,
                                $seenCaseIds[$caseId],
                                $entry.RelativePath
                            )
                        }
                        else {
                            $seenCaseIds[$caseId] = $entry.RelativePath
                        }
                    }
                }
            }
        }
    }
    finally {
        if (Test-Path -LiteralPath $tempDirectory) {
            Remove-Item -LiteralPath $tempDirectory -Recurse -Force
        }
    }

    $schemaSuccessfulCount = $testedCount - $schemaFailureCount
    $totalFailureCount = $schemaFailureCount + $duplicateTestIdCount + $duplicateCaseIdCount

    $schemaSummary = (
        "Schema validation summary: tested={0}; successful={1}; failed={2}" -f
        $testedCount,
        $schemaSuccessfulCount,
        $schemaFailureCount
    )

    $uniquenessSummary = (
        "Uniqueness summary: test-ids-validated={0}; duplicate-test-ids={1}; case-ids-validated={2}; duplicate-case-ids={3}" -f
        $validatedTestIdCount,
        $duplicateTestIdCount,
        $validatedCaseIdCount,
        $duplicateCaseIdCount
    )

    $globalSummary = (
        "Conformance validation summary: total-violations={0}" -f
        $totalFailureCount
    )

    $schemaPass = ($schemaFailureCount -eq 0)
    $uniquenessPass = (($duplicateTestIdCount + $duplicateCaseIdCount) -eq 0)
    $globalPass = ($totalFailureCount -eq 0)

    $schemaColor = if ($schemaPass) { "Green" } else { "Red" }
    $uniquenessColor = if ($uniquenessPass) { "Green" } else { "Red" }
    $globalColor = if ($globalPass) { "Green" } else { "Red" }

    $schemaStatus = if ($schemaPass) { "PASS" } else { "FAIL" }
    $uniquenessStatus = if ($uniquenessPass) { "PASS" } else { "FAIL" }
    $globalStatus = if ($globalPass) { "PASS" } else { "FAIL" }

    Write-Host ("[{0}] {1}" -f $schemaStatus, $schemaSummary) -ForegroundColor $schemaColor
    Write-Host ("[{0}] {1}" -f $uniquenessStatus, $uniquenessSummary) -ForegroundColor $uniquenessColor
    Write-Host ("[{0}] {1}" -f $globalStatus, $globalSummary) -ForegroundColor $globalColor


    if ($FailOnError -and $totalFailureCount -gt 0) {
        throw (
            "Conformance validation failed with {0} violation(s): schema={1}; duplicate-test-ids={2}; duplicate-case-ids={3}." -f
            $totalFailureCount,
            $schemaFailureCount,
            $duplicateTestIdCount,
            $duplicateCaseIdCount
        )
    }

    return [int]$totalFailureCount
}

function Get-ConformanceVersion {
    [CmdletBinding()]
    param(
        [string] $Configuration = "GitVersion.Conformance.yml",
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
        [string] $Path = "conformance",
        [string[]] $Exclude = @("bin/**"),
        [string] $Version = "0.0.0"
    )

    Write-Host "=== Packaging conformance ==="

    $conformanceVersion =
        if ([string]::IsNullOrWhiteSpace($Version)) { "0.0.0" } else { $Version.Trim() }

    $resolvedPath = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path

    $sevenZip = Get-Command "7z" -ErrorAction SilentlyContinue

    if ($null -eq $sevenZip) {
        throw "7-Zip was not found. Ensure that '7z' is available in PATH."
    }

    if (-not (Test-Path -LiteralPath $resolvedPath -PathType Container)) {
        throw "Conformance directory '$Path' does not exist."
    }

    $effectiveExclude = Get-ConformanceEffectiveExclude -Exclude $Exclude

    $allFiles = @(
        Get-ChildItem `
            -LiteralPath $resolvedPath `
            -Recurse `
            -File
    )

    $filesToPackage = @(
        $allFiles |
        Where-Object {
            $relativePath = [System.IO.Path]::GetRelativePath(
                $resolvedPath,
                $_.FullName
            ).Replace('\\', '/')

            -not (Test-ConformanceExclude -RelativePath $relativePath -Exclude $effectiveExclude)
        }
    )

    if ($filesToPackage.Count -eq 0) {
        throw "No files were selected for packaging after applying exclusions."
    }

    $archiveDirectory = Join-Path $resolvedPath "bin"
    $archiveName = "Expressif.Conformance.$conformanceVersion.zip"
    $archivePath = Join-Path $archiveDirectory $archiveName
    $stagingDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("expressif-conformance-package-" + [Guid]::NewGuid().ToString("N"))

    New-Item `
        -ItemType Directory `
        -Path $archiveDirectory `
        -Force |
        Out-Null

    $archivePath = [System.IO.Path]::GetFullPath($archivePath)

    if (Test-Path $archivePath) {
        Remove-Item $archivePath -Force
    }

    New-Item -ItemType Directory -Path $stagingDirectory -Force | Out-Null

    try {
        foreach ($file in $filesToPackage) {
            $relativePath = [System.IO.Path]::GetRelativePath(
                $resolvedPath,
                $file.FullName
            )

            $targetPath = Join-Path $stagingDirectory $relativePath
            $targetDirectory = Split-Path -Path $targetPath -Parent

            if (-not [string]::IsNullOrWhiteSpace($targetDirectory)) {
                New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
            }

            Copy-Item -LiteralPath $file.FullName -Destination $targetPath -Force
        }

        Push-Location $stagingDirectory

        & $sevenZip.Source a `
            -tzip `
            $archivePath `
            ".\*"

        if ($LASTEXITCODE -ne 0) {
            throw "7-Zip failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        if ((Get-Location).Path -eq $stagingDirectory) {
            Pop-Location
        }

        if (Test-Path -LiteralPath $stagingDirectory) {
            Remove-Item -LiteralPath $stagingDirectory -Recurse -Force
        }
    }

    Write-ConformanceScope `
        -Title "Packaging" `
        -InputPath $Path `
        -ResolvedPath $resolvedPath `
        -Exclude $effectiveExclude

    Write-Host "Packaging discovery:" -ForegroundColor DarkCyan
    Write-Host ("  files-found    : {0}" -f $allFiles.Count) -ForegroundColor DarkCyan
    Write-Host ("  files-selected : {0}" -f $filesToPackage.Count) -ForegroundColor DarkCyan
    Write-Host ("  files-excluded : {0}" -f ($allFiles.Count - $filesToPackage.Count)) -ForegroundColor DarkCyan

    Write-Host "Created archive: $archivePath"
}

function Tag-Conformance {
    [CmdletBinding()]
    param(
        [Alias("ConformancePath")]
        [string] $Path = "conformance",
        [string[]] $Exclude = @("bin/**"),
        [string] $Version = "0.0.0",
        [switch] $Warn,
        [switch] $Force,
        [switch] $DryRun
    )

    Write-Host "=== Tagging conformance ==="

    . "$PSScriptRoot/github.ps1"

    $conformanceVersion =
        if ([string]::IsNullOrWhiteSpace($Version)) { "0.0.0" } else { $Version.Trim() }

    $resolvedPath = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path

    if (-not (Test-Path -LiteralPath $resolvedPath -PathType Container)) {
        throw "Conformance directory '$Path' does not exist."
    }

    $effectiveExclude = Get-ConformanceEffectiveExclude -Exclude $Exclude

    $pathSpec = $Path.Replace('\\', '/').Trim()
    $pathSpec = $pathSpec.TrimStart('.')
    $pathSpec = $pathSpec.Trim('/')

    if ([string]::IsNullOrWhiteSpace($pathSpec)) {
        throw "Path '$Path' cannot be converted to a Git pathspec."
    }

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
            git diff-tree --no-commit-id --name-only -r $taggedCommit -- "$pathSpec/" |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        )
    }

    $selectedChangedConformanceFiles = @(
        $changedConformanceFiles |
        Where-Object {
            $normalizedPath = $_.Replace('\\', '/').TrimStart('/')
            $prefix = "$pathSpec/"

            if (-not $normalizedPath.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
                return $false
            }

            $relativePath = $normalizedPath.Substring($prefix.Length)
            -not (Test-ConformanceExclude -RelativePath $relativePath -Exclude $effectiveExclude)
        }
    )

    Write-ConformanceScope `
        -Title "Tagging" `
        -InputPath $Path `
        -ResolvedPath $resolvedPath `
        -Exclude $effectiveExclude

    Write-Host "Tagging discovery:" -ForegroundColor DarkCyan
    Write-Host ("  files-found    : {0}" -f $changedConformanceFiles.Count) -ForegroundColor DarkCyan
    Write-Host ("  files-selected : {0}" -f $selectedChangedConformanceFiles.Count) -ForegroundColor DarkCyan
    Write-Host ("  files-excluded : {0}" -f ($changedConformanceFiles.Count - $selectedChangedConformanceFiles.Count)) -ForegroundColor DarkCyan

    if ($DryRun) {
        Write-Host "Dry run mode is enabled. All tagging calculations will run, but no tag mutation/publish operation will be executed." -ForegroundColor Yellow
    }

    if ($selectedChangedConformanceFiles.Count -eq 0) {
        Write-Host (
            "No selected changes under '$pathSpec/' for commit '$taggedCommit'. " +
            "Conformance tagging skipped."
        )
        return
    }

    Write-Host "Conformance changes detected for tagging:"
    $selectedChangedConformanceFiles |
        Sort-Object |
        ForEach-Object {
            Write-Host " - $_"
        }

    if ($conformanceVersion -match '-') {
        Write-Host "Pre-release detected ($semVer). Conformance tagging skipped."

        if ($DryRun) {
            Write-Host "Dry run completed. Tag was not created due to -DryRun." -ForegroundColor Yellow
        }

        return
    }

    $owner = ""
    $repository = ""

    if ([string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_NAME)) {
        if ($DryRun) {
            Write-Warning "[DRY-RUN] APPVEYOR_REPO_NAME is empty (expected owner/repo). Using '<unknown>/<unknown>' for reporting only."
            $owner = "<unknown>"
            $repository = "<unknown>"
        }
        else {
            throw "APPVEYOR_REPO_NAME is empty (expected owner/repo)."
        }
    }
    else {
        $owner, $repository = $env:APPVEYOR_REPO_NAME.Split('/')
    }

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

    Write-Host "Tagging calculations:" -ForegroundColor DarkCyan
    Write-Host ("  semver         : '{0}'" -f $semVer) -ForegroundColor DarkCyan
    Write-Host ("  commit-sha     : '{0}'" -f $taggedCommit) -ForegroundColor DarkCyan
    Write-Host ("  main-reference : '{0}'" -f $mainReference) -ForegroundColor DarkCyan
    Write-Host ("  tag-reference  : '{0}'" -f $tagReference) -ForegroundColor DarkCyan
    Write-Host ("  force          : {0}" -f $Force.IsPresent) -ForegroundColor DarkCyan
    Write-Host ("  dry-run        : {0}" -f $DryRun.IsPresent) -ForegroundColor DarkCyan

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
            if ($DryRun) {
                Write-Warning (
                    "[DRY-RUN] Conformance tag '$semVer' already exists at commit '$tagCommit' and is $mainStatus. " +
                    "The existing tag would be replaced because -Force was specified."
                )

                $remoteTagExists =
                    @(
                        git ls-remote --tags origin $tagReference
                    ).Count -gt 0

                Write-Host ("[DRY-RUN] Remote tag exists on origin: {0}" -f $remoteTagExists) -ForegroundColor Yellow
                Write-Host "[DRY-RUN] Dry run completed. Tag was not modified due to -DryRun." -ForegroundColor Yellow
                return
            }

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
            if ($DryRun) {
                Write-Warning (
                    "[DRY-RUN] Conformance tag '$semVer' already exists at commit '$tagCommit' and is $mainStatus. " +
                    "A non-dry run would fail because -Force was not specified."
                )
                Write-Host "[DRY-RUN] Dry run completed. Tag was not created due to -DryRun." -ForegroundColor Yellow
                return
            }

            throw (
                "Conformance tag '$semVer' already exists at commit '$tagCommit' and is $mainStatus. " +
                "Refusing to create an existing tag. Use -Force to replace it."
            )
        }
    }

    if ($DryRun) {
        Write-Host "[DRY-RUN] Tagging calculations completed successfully." -ForegroundColor Yellow
        Write-Host ("[DRY-RUN] Tag '{0}' would be created for commit '{1}' on repository '{2}/{3}'." -f $semVer, $taggedCommit, $context.Owner, $context.Repository) -ForegroundColor Yellow
        Write-Host "[DRY-RUN] Dry run completed. Tag was not created due to -DryRun." -ForegroundColor Yellow
        return
    }

    Write-Host "Tagging conformance release '$semVer' for $($context.Owner)/$($context.Repository) on Commit SHA $taggedCommit"
    $context | Publish-Commit-Tag -Tag $semVer -CommitSha $taggedCommit
}
