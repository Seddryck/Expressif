Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:TestIdentifierPropertyName = "Test-Identifier"
$script:CaseIdentifierPropertyName = "Case-Identifier"
$script:SupportedOutcomes = @("successful", "failed", "not-run")

function Import-ConformanceYamlSupport {
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
    param(
        [string[]] $Exclude
    )

    return @(
        $Exclude |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Write-ConformanceScope {
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

function Get-XmlAttribute {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlElement] $Element,

        [Parameter(Mandatory)]
        [string] $Name
    )

    return $Element.GetAttribute($Name)
}

function Get-TestCaseProperty {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlElement] $TestCase,

        [Parameter(Mandatory)]
        [string] $PropertyName
    )

    foreach ($property in $TestCase.SelectNodes("./*[local-name()='properties']/*[local-name()='property']")) {
        $name = Get-XmlAttribute -Element $property -Name "name"

        if ([string]::Equals(
            $name,
            $PropertyName,
            [System.StringComparison]::OrdinalIgnoreCase
        )) {
            $value = Get-XmlAttribute -Element $property -Name "value"

            if ([string]::IsNullOrWhiteSpace($value)) {
                $value = [string] $property.InnerText
            }

            if (-not [string]::IsNullOrWhiteSpace($value)) {
                return $value.Trim()
            }
        }
    }

    return $null
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

function Convert-JUnitOutcome {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlElement] $TestCase
    )

    if ($null -ne $TestCase.SelectSingleNode("./*[local-name()='failure' or local-name()='error']")) {
        return "failed"
    }

    if ($null -ne $TestCase.SelectSingleNode("./*[local-name()='skipped']")) {
        return "not-run"
    }

    return "successful"
}

function Get-JUnitCaseIdentifier {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlElement] $TestCase,

        [Parameter(Mandatory)]
        [System.Collections.IDictionary] $KnownCaseIds
    )

    $propertyCaseId = Get-TestCaseProperty `
        -TestCase $TestCase `
        -PropertyName $script:CaseIdentifierPropertyName

    if (-not [string]::IsNullOrWhiteSpace($propertyCaseId)) {
        return $propertyCaseId
    }

    $name = Get-XmlAttribute -Element $TestCase -Name "name"

    if ([string]::IsNullOrWhiteSpace($name)) {
        return $null
    }

    # pytest writes the parameter id between the final square brackets.
    if ($name -match '\[(?<id>[^\[\]]+)\]$') {
        $candidate = $Matches["id"].Trim()

        if ($KnownCaseIds.Contains($candidate)) {
            return $candidate
        }
    }

    # Also support producers that place the exact case id in the test name.
    if ($KnownCaseIds.Contains($name)) {
        return $name
    }

    return $null
}

function Get-NUnitResultIndex {
    param(
        [Parameter(Mandatory)]
        [xml] $Xml,

        [Parameter(Mandatory)]
        [string] $ReportPath
    )

    $index = @{}
    $ignored = 0

    foreach ($testCase in $Xml.SelectNodes("//*[local-name()='test-case']")) {
        $caseId = Get-TestCaseProperty `
            -TestCase $testCase `
            -PropertyName $script:CaseIdentifierPropertyName

        if ([string]::IsNullOrWhiteSpace($caseId)) {
            $ignored++
            continue
        }

        if ($index.Contains($caseId)) {
            throw "Duplicate NUnit Case-Identifier '$caseId' in '$ReportPath'."
        }

        $index[$caseId] = [ordered]@{
            testId = Get-TestCaseProperty `
                -TestCase $testCase `
                -PropertyName $script:TestIdentifierPropertyName

            outcome = Convert-NUnitOutcome `
                -Result (Get-XmlAttribute -Element $testCase -Name "result") `
                -RunState (Get-XmlAttribute -Element $testCase -Name "runstate")
        }
    }

    if ($ignored -gt 0) {
        Write-Verbose "$ignored NUnit test case(s) had no Case-Identifier and were ignored."
    }

    return $index
}

function Get-JUnitResultIndex {
    param(
        [Parameter(Mandatory)]
        [xml] $Xml,

        [Parameter(Mandatory)]
        [System.Collections.IDictionary] $KnownCaseIds,

        [Parameter(Mandatory)]
        [System.Collections.IDictionary] $CaseToTestId,

        [Parameter(Mandatory)]
        [string] $ReportPath
    )

    $index = @{}
    $ignored = 0

    foreach ($testCase in $Xml.SelectNodes("//*[local-name()='testcase']")) {
        $caseId = Get-JUnitCaseIdentifier `
            -TestCase $testCase `
            -KnownCaseIds $KnownCaseIds

        if ([string]::IsNullOrWhiteSpace($caseId)) {
            $ignored++
            continue
        }

        if ($index.Contains($caseId)) {
            throw "Duplicate JUnit case identifier '$caseId' in '$ReportPath'."
        }

        $reportedTestId = Get-TestCaseProperty `
            -TestCase $testCase `
            -PropertyName $script:TestIdentifierPropertyName

        if ([string]::IsNullOrWhiteSpace($reportedTestId)) {
            $reportedTestId = $CaseToTestId[$caseId]
        }

        $index[$caseId] = [ordered]@{
            testId  = $reportedTestId
            outcome = Convert-JUnitOutcome -TestCase $testCase
        }
    }

    if ($ignored -gt 0) {
        Write-Verbose "$ignored JUnit test case(s) did not match a conformance case and were ignored."
    }

    return $index
}

function Get-ConformanceDocuments {
    param(
        [Parameter(Mandatory)]
        [string] $ConformanceRoot,

        [Parameter(Mandatory)]
        [string] $OutputFile,

        [string[]] $Exclude = @("bin/**", "/*.yaml", "/*.yml")
    )

    $documents = [System.Collections.Generic.List[object]]::new()
    $caseIds = @{}
    $testIds = @{}
    $caseToTestId = @{}

    $effectiveExclude = Get-ConformanceEffectiveExclude -Exclude $Exclude

    $allYamlFiles = @(
        Get-ChildItem `
        -LiteralPath $ConformanceRoot `
        -Recurse `
        -File |
        Where-Object {
            $_.Extension -in @(".yaml", ".yml")
        } |
        Sort-Object FullName
    )

    $yamlFiles = @(
        $allYamlFiles |
        Where-Object {
            $relativePath = [System.IO.Path]::GetRelativePath(
                $ConformanceRoot,
                $_.FullName
            ).Replace("\", "/")

            $_.FullName -ne $OutputFile -and
            -not (Test-ConformanceExclude -RelativePath $relativePath -Exclude $effectiveExclude)
        }
    )

    foreach ($yamlFile in $yamlFiles) {
        $relativePath = [System.IO.Path]::GetRelativePath(
            $ConformanceRoot,
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

            if ($testIds.Contains($testId)) {
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

            foreach ($case in $cases) {
                if ($case -isnot [System.Collections.IDictionary]) {
                    throw "A case under test '$testId' in '$relativePath' is not a YAML mapping."
                }

                $caseId = Get-RequiredId `
                    -Node $case `
                    -Description "A case under test '$testId' in '$relativePath'"

                if ($caseIds.Contains($caseId)) {
                    throw (
                        "Duplicate case id '$caseId' in '$relativePath'. " +
                        "It was already declared in '$($caseIds[$caseId])'."
                    )
                }

                $caseIds[$caseId] = $relativePath
                $caseToTestId[$caseId] = $testId
            }
        }

        $documents.Add([ordered]@{
            path     = $relativePath
            document = $document
            tests    = $tests
        })
    }

    return [ordered]@{
        documents    = $documents
        caseIds      = $caseIds
        testIds      = $testIds
        caseToTestId = $caseToTestId
        yamlFound    = $allYamlFiles.Count
        yamlSelected = $yamlFiles.Count
        yamlExcluded = ($allYamlFiles.Count - $yamlFiles.Count)
        exclude      = $effectiveExclude
    }
}

function Add-OutcomesToDocuments {
    param(
        [Parameter(Mandatory)]
        [System.Collections.IEnumerable] $Documents,

        [Parameter(Mandatory)]
        [System.Collections.IDictionary] $Results
    )

    $operators = [System.Collections.Generic.List[object]]::new()
    $counts = [ordered]@{
        successful = 0
        failed     = 0
        notRun     = 0
    }

    foreach ($entry in $Documents) {
        $document = $entry.document
        $tests = $entry.tests

        foreach ($test in $tests) {
            $testId = Get-RequiredId -Node $test -Description "A test in '$($entry.path)'"
            $cases = Get-DictionaryValue -Dictionary $test -Name "cases"

            if ($null -eq $cases) {
                continue
            }

            $reportedCases = [System.Collections.Generic.List[object]]::new()

            foreach ($case in $cases) {
                $caseId = Get-RequiredId `
                    -Node $case `
                    -Description "A case under test '$testId' in '$($entry.path)'"

                if ($Results.Contains($caseId)) {
                    $result = $Results[$caseId]

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
                            "but the test report associates it with '$($result.testId)'."
                        )
                    }

                    $outcome = [string] $result.outcome
                }
                else {
                    $outcome = "not-run"
                }

                $reportedCases.Add((Copy-CaseWithOutcome -Case $case -Outcome $outcome))

                switch ($outcome) {
                    "successful" { $counts.successful++ }
                    "failed"     { $counts.failed++ }
                    default      { $counts.notRun++ }
                }
            }

            Set-DictionaryValue -Dictionary $test -Name "cases" -Value @($reportedCases)
        }

        $operators.Add([ordered]@{
            operator = Get-DictionaryValue -Dictionary $document -Name "operator"
            kind     = Get-DictionaryValue -Dictionary $document -Name "kind"
            suite    = Get-DictionaryValue -Dictionary $document -Name "suite"
            tests    = @($tests)
        })
    }

    return [ordered]@{
        operators = $operators
        counts    = $counts
    }
}

function Export-ConformanceReport {
    [CmdletBinding(DefaultParameterSetName = "NUnit")]
    param(
        [Parameter(Mandatory)]
        [ValidateScript({
            if (-not (Test-Path -LiteralPath $_ -PathType Container)) {
                throw "Conformance folder '$_' does not exist."
            }
            $true
        })]
        [Alias("Path")]
        [string] $ConformancePath,

        [Alias("Excludes")]
        [string[]] $Exclude = @("bin/**", "/*.yaml", "/*.yml"),

        [Parameter(Mandatory)]
        [ValidateScript({
            if (-not (Test-Path -LiteralPath $_ -PathType Leaf)) {
                throw "XML report '$_' does not exist."
            }
            $true
        })]
        [Alias("XmlPath")]
        [string] $ReportPath,

        [Parameter(Mandatory)]
        [string] $OutputYamlPath,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string] $Platform,

        [Parameter(Mandatory, ParameterSetName = "NUnit")]
        [switch] $NUnit,

        [Parameter(Mandatory, ParameterSetName = "JUnit")]
        [switch] $JUnit,

        [string] $PlatformVersion = "$env:GitVersion_SemVer",

        [string] $ConformanceVersion = "$env:GitVersion_Conformance_SemVer"
    )

    Import-ConformanceYamlSupport

    $conformanceRoot = (Resolve-Path -LiteralPath $ConformancePath).Path
    $reportFile = (Resolve-Path -LiteralPath $ReportPath).Path
    $outputFile = [System.IO.Path]::GetFullPath($OutputYamlPath)

    $conformance = Get-ConformanceDocuments `
        -ConformanceRoot $conformanceRoot `
        -OutputFile $outputFile `
        -Exclude $Exclude

    Write-ConformanceScope `
        -Title "Report export" `
        -InputPath $ConformancePath `
        -ResolvedPath $conformanceRoot `
        -Exclude $conformance.exclude

    Write-Host "Report export discovery:" -ForegroundColor DarkCyan
    Write-Host ("  yaml-found     : {0}" -f $conformance.yamlFound) -ForegroundColor DarkCyan
    Write-Host ("  yaml-selected  : {0}" -f $conformance.yamlSelected) -ForegroundColor DarkCyan
    Write-Host ("  yaml-excluded  : {0}" -f $conformance.yamlExcluded) -ForegroundColor DarkCyan

    [xml] $xml = Get-Content `
        -LiteralPath $reportFile `
        -Raw `
        -Encoding UTF8

    $format = $PSCmdlet.ParameterSetName

    $results = switch ($format) {
        "NUnit" {
            Get-NUnitResultIndex -Xml $xml -ReportPath $reportFile
        }
        "JUnit" {
            Get-JUnitResultIndex `
                -Xml $xml `
                -KnownCaseIds $conformance.caseIds `
                -CaseToTestId $conformance.caseToTestId `
                -ReportPath $reportFile
        }
        default {
            throw "Unsupported report format '$format'."
        }
    }

    $merged = Add-OutcomesToDocuments `
        -Documents $conformance.documents `
        -Results $results

    $unexpectedResults = @(
        $results.Keys |
        Where-Object { -not $conformance.caseIds.Contains($_) } |
        Sort-Object
    )

    $output = [ordered]@{
        header = [ordered]@{
            release = [ordered]@{
                target  = $Platform
                version = $PlatformVersion
            }
            conformance = $ConformanceVersion
        }
        operators = @($merged.operators)
    }

    $outputDirectory = Split-Path -Parent -Path $outputFile

    if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    }

    $output |
        ConvertTo-Yaml |
        Set-Content -LiteralPath $outputFile -Encoding UTF8

    Write-Host ""
    Write-Host "Conformance report created:"
    Write-Host "  $outputFile"
    Write-Host ""
    Write-Host "Format:     $format"
    Write-Host "Platform:   $Platform"
    Write-Host "Cases:      $($conformance.caseIds.Count)"
    Write-Host "Successful: $($merged.counts.successful)"
    Write-Host "Failed:     $($merged.counts.failed)"
    Write-Host "Not run:    $($merged.counts.notRun)"

    if ($unexpectedResults.Count -gt 0) {
        Write-Warning (
            "$($unexpectedResults.Count) $format result(s) identify cases " +
            "that do not exist in the YAML conformance files."
        )
    }

    return [pscustomobject]@{
        OutputPath = $outputFile
        Format     = $format
        Platform   = $Platform
        Cases      = $conformance.caseIds.Count
        Successful = $merged.counts.successful
        Failed     = $merged.counts.failed
        NotRun     = $merged.counts.notRun
    }
}

Export-ModuleMember -Function Export-ConformanceReport
