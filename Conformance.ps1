$ErrorActionPreference = "Stop"

# Prevent .NET first-run output from polluting captured JSON.
$env:DOTNET_NOLOGO = "true"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "true"

$script:ConformanceVersionCache = $null

<#
.SYNOPSIS
Fetches and refreshes Git tags for the current repository.

.DESCRIPTION
Fetches tags from origin and automatically handles shallow repositories by
unshallowing before fetching tags.
#>
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

<#
.SYNOPSIS
Resolves the best available main branch reference.

.DESCRIPTION
Returns origin/main when available, otherwise main. Returns null when neither
reference exists.
#>
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

<#
.SYNOPSIS
Installs PowerShell dependencies required by conformance tooling.

.DESCRIPTION
Ensures the powershell-yaml module is available for the current user.
#>
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

<#
.SYNOPSIS
Imports YAML support for conformance scripts.

.DESCRIPTION
Validates that powershell-yaml is installed and imports it into the current
session.
#>
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

<#
.SYNOPSIS
Converts a glob-style pattern to a regular expression.

.PARAMETER Pattern
Glob pattern to convert. Supports *, **, and ? tokens.

.OUTPUTS
System.String
#>
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

<#
.SYNOPSIS
Tests whether a relative file path matches exclusion patterns.

.PARAMETER RelativePath
Path relative to the conformance root.

.PARAMETER Exclude
List of glob patterns used to exclude files.

.OUTPUTS
System.Boolean
#>
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

<#
.SYNOPSIS
Normalizes exclusion patterns by removing empty entries.

.PARAMETER Exclude
Raw exclusion patterns.

.OUTPUTS
System.String[]
#>
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

<#
.SYNOPSIS
Writes scope diagnostics for a conformance operation.

.PARAMETER Title
Operation title shown in logs.

.PARAMETER InputPath
Path value received by the function.

.PARAMETER ResolvedPath
Absolute resolved path used by the function.

.PARAMETER Exclude
Effective exclusion patterns applied during the operation.
#>
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

<#
.SYNOPSIS
Builds a conformance manifest file from discovered YAML test content.

.DESCRIPTION
Discovers conformance YAML files, computes manifest metadata, and renders the
manifest by applying the Scriban template with didot-cli.

.PARAMETER Version
Conformance version to include in the manifest.

.PARAMETER CommitSha
Commit SHA used for manifest source revision.

.PARAMETER Path
Root directory of conformance files.

.PARAMETER Exclude
Glob patterns used to exclude files from manifest discovery.

.PARAMETER OutputFilePath
Manifest output file path. Relative values are resolved from Path.

.OUTPUTS
PSCustomObject
#>
function Build-ConformanceManifest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $Version,

        [Parameter(Mandatory)]
        [string] $CommitSha,

        [string] $Path = "conformance",
        [string[]] $Exclude = @("bin/**", "/*.yaml", "/*.yml"),
        [Alias("OutputPath")]
        [string] $OutputFilePath = "bin/conformance.manifest.yaml"
    )

    $resolvedPath = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path
    $resolvedOutputFilePath = if ([System.IO.Path]::IsPathRooted($OutputFilePath)) {
        $OutputFilePath
    }
    else {
        Join-Path $resolvedPath $OutputFilePath
    }

    $effectiveExclude = Get-ConformanceEffectiveExclude -Exclude $Exclude

    $templatePath = Join-Path $resolvedPath "conformance.manifest.template.yaml"

    if (-not (Test-Path -LiteralPath $templatePath -PathType Leaf)) {
        throw "Conformance manifest template '$templatePath' was not found."
    }

    $didotInstalled = @(
        dotnet tool list --local 2>$null |
        Where-Object { $_ -match '^didot-cli\s' }
    ).Count -gt 0

    if (-not $didotInstalled) {
        throw @"
didot-cli local tool is required to build conformance manifest.
Install/restore it with:
dotnet tool restore
"@
    }

    $didotProbeOutput = @(& dotnet tool run didot --help 2>&1)

    if ($LASTEXITCODE -ne 0) {
        $probeText = $didotProbeOutput -join [Environment]::NewLine
        throw @"
didot-cli local tool is declared but not runnable.
Run:
dotnet tool restore

Output:
$probeText
"@
    }

    Import-YamlSupport

    $allYamlFiles = @(
        Get-ChildItem `
            -LiteralPath $resolvedPath `
            -Recurse `
            -File |
        Where-Object {
            $_.Extension -in @(".yaml", ".yml")
        }
    )

    $selectedYamlFiles = @(
        $allYamlFiles |
        Where-Object {
            $relativePath = [System.IO.Path]::GetRelativePath(
                $resolvedPath,
                $_.FullName
            ).Replace('\', '/')

            (-not (Test-ConformanceExclude -RelativePath $relativePath -Exclude $effectiveExclude)) -and
            $relativePath.Contains('/')
        }
    )

    $patternSet = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparer]::OrdinalIgnoreCase)
    $testCount = 0
    $testCaseCount = 0

    foreach ($file in $selectedYamlFiles) {
        $relativePath = [System.IO.Path]::GetRelativePath(
            $resolvedPath,
            $file.FullName
        ).Replace('\', '/')

        if ($relativePath.Contains('/')) {
            $topFolder = $relativePath.Split('/')[0]

            if (-not [string]::IsNullOrWhiteSpace($topFolder)) {
                [void]$patternSet.Add("$topFolder/**/*.yaml")
            }
        }

        $yamlText = Get-Content -LiteralPath $file.FullName -Raw

        if ([string]::IsNullOrWhiteSpace($yamlText)) {
            continue
        }

        $yamlDocument = ConvertFrom-Yaml -Yaml $yamlText
        $tests = @($yamlDocument.tests)
        $testCount += $tests.Count

        foreach ($test in $tests) {
            $cases = @($test.cases)
            $testCaseCount += $cases.Count
        }
    }

    $patterns = @([System.Linq.Enumerable]::ToArray($patternSet) | Sort-Object)
    $revision = if ([string]::IsNullOrWhiteSpace($CommitSha)) { "<unknown>" } else { $CommitSha.Trim() }
    $repository = if ([string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_NAME)) {
        "https://github.com/Seddryck/Expressif"
    }
    else {
        "https://github.com/$($env:APPVEYOR_REPO_NAME.Trim())"
    }

    $model = [ordered]@{
        suite = [ordered]@{
            version = $Version
        }
        source = [ordered]@{
            repository = $repository
            revision   = $revision
            tag        = "conformance-$Version"
        }
        contents = [ordered]@{
            patterns = $patterns
            counts   = [ordered]@{
                files     = $selectedYamlFiles.Count
                tests     = $testCount
                testCases = $testCaseCount
            }
        }
    } | ConvertTo-Json -Depth 20

    $outputDirectory = Split-Path -Path $resolvedOutputFilePath -Parent
    if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    }

    $model | dotnet tool run didot `
        -t $templatePath `
        -e scriban `
        -i `
        -r json `
        -o $resolvedOutputFilePath `

    if ($LASTEXITCODE -ne 0) {
        throw "Didot execution failed while building conformance manifest."
    }

    return [PSCustomObject]@{
        ManifestPath = [System.IO.Path]::GetFullPath($resolvedOutputFilePath)
        Version      = $Version
        Tag          = "conformance-$Version"
        Revision     = $revision
        PatternCount = $patterns.Count
        FileCount    = $selectedYamlFiles.Count
        TestCount    = $testCount
        TestCaseCount = $testCaseCount
    }
}

<#
.SYNOPSIS
Validates conformance YAML files against schema and uniqueness rules.

.DESCRIPTION
Runs JSON schema validation using ajv-cli and checks for duplicate test and
case identifiers across selected YAML files.

.PARAMETER Path
Root directory of conformance files.

.PARAMETER Exclude
Glob patterns used to exclude files from validation.

.PARAMETER FailOnError
Throws when one or more violations are detected.

.OUTPUTS
System.Int32
#>
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

<#
.SYNOPSIS
Calculates the conformance version from Git history and tags.

.DESCRIPTION
Uses GitVersion with conformance-specific configuration and reuses the latest
conformance release version when no selected conformance files changed.

.PARAMETER Path
Root directory used to detect conformance file changes.

.PARAMETER Exclude
Glob patterns used to exclude changed files from version-impact detection.

.PARAMETER Configuration
GitVersion configuration file path.

.PARAMETER Warn
Emits additional warnings for tag visibility conditions.

.PARAMETER Refresh
Bypasses cached value and recalculates the version.

.PARAMETER NoEnv
Skips writing the GitVersion_Conformance_SemVer environment variable.

.OUTPUTS
System.String
#>
function Get-ConformanceVersion {
    [CmdletBinding()]
    param(
        [Alias("ConformancePath")]
        [string] $Path = "conformance",
        [string[]] $Exclude = @("bin/**"),
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

    $resolvedPath = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path
    $effectiveExclude = Get-ConformanceEffectiveExclude -Exclude $Exclude

    $pathSpec = $Path.Replace('\', '/').Trim()
    $pathSpec = $pathSpec.TrimStart('.')
    $pathSpec = $pathSpec.Trim('/')

    if ([string]::IsNullOrWhiteSpace($pathSpec)) {
        throw "Path '$Path' cannot be converted to a Git pathspec."
    }

    $versionedCommit = if (-not [string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_COMMIT)) {
        $env:APPVEYOR_REPO_COMMIT.Trim()
    }
    else {
        (& git rev-parse HEAD 2>$null)
    }

    $versionedCommit =
        if ($null -eq $versionedCommit) { "" } else { $versionedCommit.Trim() }

    $changedConformanceFiles = @()

    if (-not [string]::IsNullOrWhiteSpace($versionedCommit)) {
        $changedConformanceFiles = @(
            git diff-tree --no-commit-id --name-only -r $versionedCommit -- "$pathSpec/" |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        )
    }

    $selectedChangedConformanceFiles = @(
        $changedConformanceFiles |
        Where-Object {
            $normalizedPath = $_.Replace('\', '/').TrimStart('/')
            $prefix = "$pathSpec/"

            if (-not $normalizedPath.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
                return $false
            }

            $relativePath = $normalizedPath.Substring($prefix.Length)
            -not (Test-ConformanceExclude -RelativePath $relativePath -Exclude $effectiveExclude)
        }
    )

    Write-Host "Conformance changes for version calculation:" -ForegroundColor DarkCyan
    Write-Host ("  input-path     : '{0}'" -f $Path) -ForegroundColor DarkCyan
    Write-Host ("  resolved-path  : '{0}'" -f $resolvedPath) -ForegroundColor DarkCyan
    Write-Host ("  git-pathspec   : '{0}/'" -f $pathSpec) -ForegroundColor DarkCyan
    if ($effectiveExclude.Count -gt 0) {
        Write-Host ("  exclusions     : {0} pattern(s)" -f $effectiveExclude.Count) -ForegroundColor DarkCyan
        foreach ($pattern in $effectiveExclude) {
            Write-Host ("    - {0}" -f $pattern) -ForegroundColor DarkCyan
        }
    }
    else {
        Write-Host "  exclusions     : <none>" -ForegroundColor DarkCyan
    }
    Write-Host ("  commit-sha     : '{0}'" -f $versionedCommit) -ForegroundColor DarkCyan
    Write-Host ("  files-found    : {0}" -f $changedConformanceFiles.Count) -ForegroundColor DarkCyan
    Write-Host ("  files-selected : {0}" -f $selectedChangedConformanceFiles.Count) -ForegroundColor DarkCyan
    Write-Host ("  files-excluded : {0}" -f ($changedConformanceFiles.Count - $selectedChangedConformanceFiles.Count)) -ForegroundColor DarkCyan

    if ($selectedChangedConformanceFiles.Count -gt 0) {
        $selectedChangedConformanceFiles |
            Sort-Object |
            ForEach-Object {
                Write-Host ("    - {0}" -f $_) -ForegroundColor DarkCyan
            }
    }
    else {
        Write-Host "    - <none>" -ForegroundColor DarkCyan
    }

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

    if ($selectedChangedConformanceFiles.Count -eq 0) {
        if ($null -ne $latestRelease) {
            $conformanceVersion = $latestRelease.Version.ToString()
            $script:ConformanceVersionCache = $conformanceVersion

            Write-Host (
                "No files changed under '{2}/' for commit '{0}'. Reusing latest conformance release version: {1}" -f
                $versionedCommit,
                $conformanceVersion,
                $pathSpec
            ) -ForegroundColor DarkYellow

            if (-not $NoEnv) {
                $env:GitVersion_Conformance_SemVer = "conformance-$conformanceVersion"

                Write-Host (
                    "Environment variable GitVersion_Conformance_SemVer: {0}" -f
                    $env:GitVersion_Conformance_SemVer
                )
            }

            return $conformanceVersion
        }

        Write-Host (
            "No files changed under 'conformance/' for commit '{0}', but no prior conformance tag was found. Falling back to GitVersion calculation." -f
            $versionedCommit
        ) -ForegroundColor DarkYellow
    }

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

<#
.SYNOPSIS
Packages conformance files into a zip archive.

.DESCRIPTION
Collects selected files, optionally builds a manifest, stages content in a
temporary directory, and creates a zip archive with 7-Zip.

.PARAMETER Path
Root directory of conformance files.

.PARAMETER Exclude
Glob patterns used to exclude files from packaging.

.PARAMETER Version
Conformance version used in the archive name and manifest.

.PARAMETER NoManifest
Skips manifest generation and inclusion when specified.
#>
function Package-Conformance {
    [CmdletBinding()]
    param(
        [string] $Path = "conformance",
        [string[]] $Exclude = @("bin/**", "/*.yaml", "/*.yml"),
        [string] $Version = "0.0.0",
        [switch] $NoManifest
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
            ).Replace('\', '/')

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

    $commitSha = if (-not [string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_COMMIT)) {
        $env:APPVEYOR_REPO_COMMIT.Trim()
    }
    else {
        $head = (& git rev-parse HEAD 2>$null)
        if ([string]::IsNullOrWhiteSpace($head)) { "<unknown>" } else { $head.Trim() }
    }

    $manifestInfo = $null

    if (-not $NoManifest) {
        $manifestOutputPath = Join-Path $archiveDirectory "conformance.manifest.yaml"
        $manifestInfo = Build-ConformanceManifest `
            -Version $conformanceVersion `
            -CommitSha $commitSha `
            -Path $resolvedPath `
            -Exclude $effectiveExclude `
            -OutputFilePath $manifestOutputPath
    }

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

        if (-not $NoManifest -and $null -ne $manifestInfo) {
            Copy-Item `
                -LiteralPath $manifestInfo.ManifestPath `
                -Destination (Join-Path $stagingDirectory "conformance.manifest.yaml") `
                -Force
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

    if ($NoManifest) {
        Write-Host "Manifest generation skipped because -NoManifest was specified." -ForegroundColor DarkYellow
    }
    else {
        Write-Host "Manifest details:" -ForegroundColor DarkCyan
        Write-Host ("  path        : {0}" -f $manifestInfo.ManifestPath) -ForegroundColor DarkCyan
        Write-Host ("  version     : {0}" -f $manifestInfo.Version) -ForegroundColor DarkCyan
        Write-Host ("  tag         : {0}" -f $manifestInfo.Tag) -ForegroundColor DarkCyan
        Write-Host ("  revision    : {0}" -f $manifestInfo.Revision) -ForegroundColor DarkCyan
        Write-Host ("  patterns    : {0}" -f $manifestInfo.PatternCount) -ForegroundColor DarkCyan
        Write-Host ("  files       : {0}" -f $manifestInfo.FileCount) -ForegroundColor DarkCyan
        Write-Host ("  tests       : {0}" -f $manifestInfo.TestCount) -ForegroundColor DarkCyan
        Write-Host ("  test-cases  : {0}" -f $manifestInfo.TestCaseCount) -ForegroundColor DarkCyan
    }

    Write-Host "Created archive: $archivePath"
}

<#
.SYNOPSIS
Creates or updates the conformance Git tag when eligible.

.DESCRIPTION
Tags the selected commit with the conformance semantic version when selected
files changed and branch conditions are met.

.PARAMETER Path
Root directory used to detect changed conformance files.

.PARAMETER Exclude
Glob patterns used to exclude changed files from tag impact detection.

.PARAMETER Version
Conformance version used to build the tag name.

.PARAMETER Warn
Enables warning output for additional diagnostics.

.PARAMETER Force
Allows replacing an existing conformance tag.

.PARAMETER DryRun
Evaluates and reports actions without mutating or publishing tags.
#>
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

    $pathSpec = $Path.Replace('\', '/').Trim()
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
            $normalizedPath = $_.Replace('\', '/').TrimStart('/')
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
