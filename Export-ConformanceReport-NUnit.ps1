[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateScript({
        if (-not (Test-Path -LiteralPath $_ -PathType Container)) {
            throw "Conformance folder '$_' does not exist."
        }
        $true
    })]
    [Alias("ConformancePath")]
    [string] $Path,

    [string[]] $Exclude = @("bin/**", "/*.yaml", "/*.yml"),

    [Parameter(Mandatory)]
    [ValidateScript({
        if (-not (Test-Path -LiteralPath $_ -PathType Leaf)) {
            throw "NUnit XML report '$_' does not exist."
        }
        $true
    })]
    [string] $XmlPath,

    [Parameter(Mandatory)]
    [string] $OutputYamlPath
)

Write-Host "=== Creating conformance report for .NET ==="
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$TestIdentifierPropertyName = "Test-Identifier"
$CaseIdentifierPropertyName = "Case-Identifier"

function Import-YamlSupport {
    $moduleName = "powershell-yaml"

    $module = Get-Module -ListAvailable -Name $moduleName |
        Sort-Object Version -Descending |
        Select-Object -First 1

    if ($null -eq $module) {
        Write-Host "Installing PowerShell module '$moduleName'..."

        if ($null -eq (Get-PSRepository -Name PSGallery -ErrorAction SilentlyContinue)) {
            throw "The PowerShell Gallery repository is not registered."
        }

        Install-Module `
            -Name $moduleName `
            -Repository PSGallery `
            -Scope CurrentUser `
            -Force `
            -AllowClobber
    }

    Import-Module $moduleName -Force
}

function Convert-GlobPatternToRegex {
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

function Test-GlobPatternMatch {
    param(
        [Parameter(Mandatory)]
        [string] $Candidate,

        [Parameter(Mandatory)]
        [string] $Pattern
    )

    $regex = Convert-GlobPatternToRegex -Pattern $Pattern

    return $null -ne $regex -and $Candidate -match $regex
}

function Test-ConformanceExclude {
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

            if (Test-GlobPatternMatch -Candidate $normalizedPath -Pattern $pattern) {
                return $true
            }

            continue
        }

        if ($pattern.Contains('/')) {
            if (Test-GlobPatternMatch -Candidate $normalizedPath -Pattern $pattern) {
                return $true
            }

            continue
        }

        if (Test-GlobPatternMatch -Candidate $fileName -Pattern $pattern) {
            return $true
        }
    }

    return $false
}

function Get-DictionaryValue {
    param(
        [Parameter(Mandatory)]
        [System.Collections.IDictionary] $Dictionary,

        [Parameter(Mandatory)]
        [string] $Name
    )

    foreach ($key in $Dictionary.Keys) {
        if ([string]::Equals(
            [string] $key,
            $Name,
            [System.StringComparison]::OrdinalIgnoreCase
        )) {
            return $Dictionary[$key]
        }
    }

    return $null
}

function Set-DictionaryValue {
    param(
        [Parameter(Mandatory)]
        [System.Collections.IDictionary] $Dictionary,

        [Parameter(Mandatory)]
        [string] $Name,

        [AllowNull()]
        [object] $Value
    )

    foreach ($key in @($Dictionary.Keys)) {
        if ([string]::Equals(
            [string] $key,
            $Name,
            [System.StringComparison]::OrdinalIgnoreCase
        )) {
            $Dictionary[$key] = $Value
            return
        }
    }

    $Dictionary[$Name] = $Value
}

function Copy-CaseWithOutcome {
    param(
        [Parameter(Mandatory)]
        [System.Collections.IDictionary] $Case,

        [Parameter(Mandatory)]
        [ValidateSet("successful", "failed", "not-run")]
        [string] $Outcome
    )

    $result = [ordered]@{}
    $outcomeAdded = $false

    foreach ($key in $Case.Keys) {
        if ([string]::Equals(
            [string] $key,
            "outcome",
            [System.StringComparison]::OrdinalIgnoreCase
        )) {
            continue
        }

        if (
            -not $outcomeAdded -and
            [string]::Equals(
                [string] $key,
                "parameters",
                [System.StringComparison]::OrdinalIgnoreCase
            )
        ) {
            $result["outcome"] = $Outcome
            $outcomeAdded = $true
        }

        $result[$key] = $Case[$key]
    }

    if (-not $outcomeAdded) {
        $result["outcome"] = $Outcome
    }

    return $result
}

function Get-RequiredId {
    param(
        [Parameter(Mandatory)]
        [System.Collections.IDictionary] $Node,

        [Parameter(Mandatory)]
        [string] $Description
    )

    $id = Get-DictionaryValue -Dictionary $Node -Name "id"

    if ([string]::IsNullOrWhiteSpace([string] $id)) {
        throw "$Description has no 'id' property."
    }

    return ([string] $id).Trim()
}

function Convert-NUnitOutcome {
    param(
        [AllowNull()]
        [string] $Result,

        [AllowNull()]
        [string] $RunState
    )

    if ($RunState -in @("Explicit", "Ignored", "NotRunnable", "Skipped")) {
        return "not-run"
    }

    switch ($Result) {
        "Passed"       { return "successful" }
        "Failed"       { return "failed" }
        "Skipped"      { return "not-run" }
        "Inconclusive" { return "not-run" }
        default        { return "not-run" }
    }
}


function Get-XmlAttribute {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlElement] $Element,

        [Parameter(Mandatory)]
        [string] $Name
    )

    return $Element.GetAttribute($Name)
}

function Get-NUnitProperty {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlElement] $TestCase,

        [Parameter(Mandatory)]
        [string] $PropertyName
    )

    foreach ($property in $TestCase.SelectNodes("properties/property")) {
        if ([string]::Equals(
            [string] $property.name,
            $PropertyName,
            [System.StringComparison]::OrdinalIgnoreCase
        )) {
            $value = [string] $property.value

            if (-not [string]::IsNullOrWhiteSpace($value)) {
                return $value.Trim()
            }
        }
    }

    return $null
}

function Get-NUnitCaseIndex {
    param(
        [Parameter(Mandatory)]
        [xml] $Xml
    )

    $index = @{}
    $missingCaseIdentifier = 0

    foreach ($testCase in $Xml.SelectNodes("//test-case")) {
        $caseId = Get-NUnitProperty `
            -TestCase $testCase `
            -PropertyName $CaseIdentifierPropertyName

        if ([string]::IsNullOrWhiteSpace($caseId)) {
            $missingCaseIdentifier++
            continue
        }

        if ($index.ContainsKey($caseId)) {
            throw "Duplicate NUnit Case-Identifier '$caseId' in '$XmlPath'."
        }

        $index[$caseId] = [ordered]@{
            testId = Get-NUnitProperty `
                -TestCase $testCase `
                -PropertyName $TestIdentifierPropertyName

            outcome = Convert-NUnitOutcome `
                -Result (Get-XmlAttribute -Element $testCase -Name "result") `
                -RunState (Get-XmlAttribute -Element $testCase -Name "runstate")
        }
    }

    if ($missingCaseIdentifier -gt 0) {
        Write-Warning (
            "$missingCaseIdentifier NUnit test case(s) do not expose a " +
            "$CaseIdentifierPropertyName property."
        )
    }

    return $index
}

Import-YamlSupport

$conformanceRoot = (Resolve-Path -LiteralPath $Path).Path
$xmlFile = (Resolve-Path -LiteralPath $XmlPath).Path
$outputFile = [System.IO.Path]::GetFullPath($OutputYamlPath)

[xml] $nunitXml = Get-Content `
    -LiteralPath $xmlFile `
    -Raw `
    -Encoding UTF8

$nunitCases = Get-NUnitCaseIndex -Xml $nunitXml

$yamlFiles = Get-ChildItem `
    -LiteralPath $conformanceRoot `
    -Recurse `
    -File |
    Where-Object {
        $_.Extension -in @(".yaml", ".yml") -and
        -not (Test-ConformanceExclude -RelativePath ([System.IO.Path]::GetRelativePath($conformanceRoot, $_.FullName).Replace("\", "/")) -Exclude $Exclude) -and
        $_.Name -notlike "*.template.yaml" -and
        $_.FullName -ne $outputFile -and
        $_.FullName -notmatch "[\\/](bin|obj)[\\/]"
    } |
    Sort-Object FullName

$caseIds = @{}
$testIds = @{}
$report = [System.Collections.Generic.List[object]]::new()

$successful = 0
$failed = 0
$notRun = 0

foreach ($yamlFile in $yamlFiles) {
    $relativePath = [System.IO.Path]::GetRelativePath(
        $conformanceRoot,
        $yamlFile.FullName
    ).Replace("\", "/")

    $yamlText = Get-Content `
        -LiteralPath $yamlFile.FullName `
        -Raw `
        -Encoding UTF8

    if ([string]::IsNullOrWhiteSpace($yamlText)) {
        continue
    }

    $document = ConvertFrom-Yaml -Yaml $yamlText

    if ($document -isnot [System.Collections.IDictionary]) {
        throw "The root of '$relativePath' must be a YAML mapping."
    }

    $tests = Get-DictionaryValue -Dictionary $document -Name "tests"

    if ($null -eq $tests) {
        Write-Warning "'$relativePath' contains no 'tests' collection."
        continue
    }

    foreach ($test in $tests) {
        if ($test -isnot [System.Collections.IDictionary]) {
            throw "A test entry in '$relativePath' is not a YAML mapping."
        }

        $testId = Get-RequiredId `
            -Node $test `
            -Description "A test in '$relativePath'"

        if ($testIds.ContainsKey($testId)) {
            throw (
                "Duplicate test id '$testId' in '$relativePath'. " +
                "It was already declared in '$($testIds[$testId])'."
            )
        }

        $testIds[$testId] = $relativePath

        $cases = Get-DictionaryValue -Dictionary $test -Name "cases"

        if ($null -eq $cases) {
            Write-Warning "Test '$testId' in '$relativePath' contains no cases."
            continue
        }

        $reportedCases = [System.Collections.Generic.List[object]]::new()

        foreach ($case in $cases) {
            if ($case -isnot [System.Collections.IDictionary]) {
                throw "A case under test '$testId' in '$relativePath' is not a YAML mapping."
            }

            $caseId = Get-RequiredId `
                -Node $case `
                -Description "A case under test '$testId' in '$relativePath'"

            if ($caseIds.ContainsKey($caseId)) {
                throw (
                    "Duplicate case id '$caseId' in '$relativePath'. " +
                    "It was already declared in '$($caseIds[$caseId])'."
                )
            }

            $caseIds[$caseId] = $relativePath

            if ($nunitCases.ContainsKey($caseId)) {
                $result = $nunitCases[$caseId]

                if (
                    -not [string]::IsNullOrWhiteSpace([string] $result.testId) -and
                    -not [string]::Equals(
                        [string] $result.testId,
                        $testId,
                        [System.StringComparison]::Ordinal
                    )
                ) {
                    throw (
                        "Case '$caseId' belongs to test '$testId' in YAML, " +
                        "but NUnit reports Test-Identifier '$($result.testId)'."
                    )
                }

                $outcome = [string] $result.outcome
            }
            else {
                $outcome = "not-run"
            }

            $reportedCases.Add(
                (Copy-CaseWithOutcome `
                    -Case $case `
                    -Outcome $outcome)
            )

            switch ($outcome) {
                "successful" { $successful++ }
                "failed"     { $failed++ }
                default      { $notRun++ }
            }
        }

        Set-DictionaryValue `
            -Dictionary $test `
            -Name "cases" `
            -Value @($reportedCases)
    }

    $report.Add(
        [ordered]@{
            operator = Get-DictionaryValue -Dictionary $document -Name "operator"
            kind     = Get-DictionaryValue -Dictionary $document -Name "kind"
            suite    = Get-DictionaryValue -Dictionary $document -Name "suite"
            tests    = @($tests)
        }
    )
}

$unexpectedResults = @(
    $nunitCases.Keys |
    Where-Object { -not $caseIds.ContainsKey($_) } |
    Sort-Object
)

$output = [ordered]@{
    header = [ordered]@{
        release = [ordered]@{
            target  = "dotnet"
            version = "$env:GitVersion_SemVer"
        }
        conformance = "$env:GitVersion_Conformance_SemVer"
    }
    operators = @($report)
}

$outputDirectory = Split-Path -Parent -Path $outputFile

if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
    New-Item `
        -ItemType Directory `
        -Path $outputDirectory `
        -Force |
        Out-Null
}

$output |
    ConvertTo-Yaml |
    Set-Content `
        -LiteralPath $outputFile `
        -Encoding UTF8

Write-Host ""
Write-Host "Conformance report created:"
Write-Host "  $outputFile"
Write-Host ""
Write-Host "Cases:      $($caseIds.Count)"
Write-Host "Successful: $successful"
Write-Host "Failed:     $failed"
Write-Host "Not run:    $notRun"

if ($unexpectedResults.Count -gt 0) {
    Write-Warning (
        "$($unexpectedResults.Count) NUnit result(s) have a Case-Identifier " +
        "that does not exist in the YAML conformance files."
    )
}