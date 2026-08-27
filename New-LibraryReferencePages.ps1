#requires -Version 7.0
#requires -PSEdition Core

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet("Function", "Predicate", "Accumulator")]
    [string] $Kind,

    [Parameter()]
    [string] $DataPath,

    [Parameter()]
    [string] $TemplatePath = "docs/_templates/library-reference.md.sbn",

    [Parameter()]
    [string] $CategoryTemplatePath = "docs/_templates/library-category.md.sbn",

    [Parameter()]
    [string] $DestinationRoot = "docs",

    [Parameter()]
    [string[]] $Scope
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$generationStopwatch = [System.Diagnostics.Stopwatch]::StartNew()

function Resolve-ProjectPath {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath(
        [System.IO.Path]::Combine($PSScriptRoot, $Path)
    )
}

function Assert-RequiredProperty {
    param(
        [Parameter(Mandatory)]
        [object] $InputObject,

        [Parameter(Mandatory)]
        [string] $PropertyName,

        [Parameter(Mandatory)]
        [string] $MemberName
    )

    $property = $InputObject.PSObject.Properties[$PropertyName]

    if ($null -eq $property) {
        throw "Library member '$MemberName' is missing '$PropertyName'."
    }

    if (
        $property.Value -is [string] -and
        [string]::IsNullOrWhiteSpace($property.Value)
    ) {
        throw "Library member '$MemberName' has an empty '$PropertyName'."
    }
}

function ConvertTo-Slug {
    param(
        [Parameter(Mandatory)]
        [string] $Value
    )

    return $Value.Trim().ToLowerInvariant() -replace '[^a-z0-9]+', '-'
}

$kindName = $Kind.ToLowerInvariant()
$kindPlural = "$kindName`s"
$kindPluralTitle = (Get-Culture).TextInfo.ToTitleCase($kindPlural)

if ([string]::IsNullOrWhiteSpace($DataPath)) {
    $DataPath = "docs/_data/$kindName.json"
}

$resolvedDataPath = Resolve-ProjectPath $DataPath
$resolvedTemplatePath = Resolve-ProjectPath $TemplatePath
$resolvedCategoryTemplatePath = Resolve-ProjectPath $CategoryTemplatePath
$resolvedDestinationRoot = Resolve-ProjectPath $DestinationRoot

if (-not (Test-Path -LiteralPath $resolvedDataPath -PathType Leaf)) {
    throw "Reference data file not found: $resolvedDataPath"
}

if (-not (Test-Path -LiteralPath $resolvedTemplatePath -PathType Leaf)) {
    throw "Scriban template not found: $resolvedTemplatePath"
}

if (-not (Test-Path -LiteralPath $resolvedCategoryTemplatePath -PathType Leaf)) {
    throw "Scriban category template not found: $resolvedCategoryTemplatePath"
}

$allMembers = Get-Content -LiteralPath $resolvedDataPath -Raw | ConvertFrom-Json
$members = @($allMembers | Where-Object { $_.IsPublic -eq $true })
$selectedScopes = @()

if ($PSBoundParameters.ContainsKey("Scope") -and @($Scope).Count -gt 0) {
    $selectedScopes = @(
        $Scope |
            ForEach-Object { $_.Trim().ToLowerInvariant() } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Sort-Object -Unique
    )
    $members = @(
        $members | Where-Object {
            $memberScope = ([string] $_.Scope).ToLowerInvariant()
            $memberRootScope = ($memberScope -split '/')[0]
            $selectedScopes -contains $memberScope -or
                $selectedScopes -contains $memberRootScope
        }
    )
}

$members = @($members | Sort-Object Scope, Name)

if ($members.Count -eq 0) {
    Write-Host "No public $kindPlural matched the requested selection."
    exit 0
}

$collectionDestination = Join-Path $resolvedDestinationRoot $kindPlural
New-Item -ItemType Directory -Path $collectionDestination -Force | Out-Null

$scopePositions = @{}
$generatedPaths = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase
)

$allScopes = @(
    $members |
        ForEach-Object { ([string] $_.Scope).ToLowerInvariant() } |
        Sort-Object -Unique
)
$rootScopes = @(
    $allScopes |
        ForEach-Object { ($_ -split '/')[0] } |
        Sort-Object -Unique
)
$categoryScopes = @($allScopes | Where-Object { $_ -like '*/*' })
$categoryPositions = @{}

foreach ($categoryScope in @($rootScopes) + @($categoryScopes)) {
    $scopeSegments = @($categoryScope -split '/')
    $rootScopeName = $scopeSegments[0]
    $categoryName = $scopeSegments[-1]
    $rootScopeTitle = (Get-Culture).TextInfo.ToTitleCase($rootScopeName)
    $categoryTitle = (Get-Culture).TextInfo.ToTitleCase($categoryName)
    $isRootCategory = $scopeSegments.Count -eq 1

    if (-not $categoryPositions.ContainsKey($rootScopeName)) {
        $categoryPositions[$rootScopeName] = 0
    }

    if ($isRootCategory) {
        $categoryDestinationPath = Join-Path $collectionDestination "$rootScopeName-$kindPlural.md"
        $existingNavOrder = 0
        if (Test-Path -LiteralPath $categoryDestinationPath -PathType Leaf) {
            $navOrderMatch = Select-String -LiteralPath $categoryDestinationPath -Pattern '^nav_order:\s*(\d+)\s*$' |
                Select-Object -First 1
            if ($null -ne $navOrderMatch) {
                $existingNavOrder = [int] $navOrderMatch.Matches[0].Groups[1].Value
            }
        }

        $categoryNavOrder = if ($existingNavOrder -gt 0) {
            $existingNavOrder
        } else {
            ([array]::IndexOf($rootScopes, $rootScopeName) + 1) * 10
        }
        $categoryPageTitle = "$rootScopeTitle $kindPlural"
        $categoryParentTitle = "$kindPluralTitle library"
        $categoryGrandParentLine = ""
        $categoryPermalink = "$rootScopeName-$kindPlural"
        $categoryMembers = @(
            $members | Where-Object {
                (([string] $_.Scope).ToLowerInvariant() -split '/')[0] -eq $rootScopeName
            } | Sort-Object Name
        )
    } else {
        $categoryPositions[$rootScopeName]++
        $categoryNavOrder = $categoryPositions[$rootScopeName] * 10
        $categoryPageTitle = "$categoryTitle $kindPlural"
        $categoryParentTitle = "$rootScopeTitle $kindPlural"
        $categoryGrandParentLine = "grand_parent: `"$kindPluralTitle library`""
        $categoryPermalink = $categoryScope
        $categoryDestination = Join-Path $collectionDestination $categoryScope
        New-Item -ItemType Directory -Path $categoryDestination -Force | Out-Null
        $categoryDestinationPath = Join-Path $categoryDestination "index.md"
        $categoryMembers = @(
            $members | Where-Object { ([string] $_.Scope).ToLowerInvariant() -eq $categoryScope } |
                Sort-Object Name
        )
    }

    $memberRows = foreach ($categoryMember in $categoryMembers) {
        $memberScope = ([string] $categoryMember.Scope).ToLowerInvariant()
        $memberSummary = ([string] $categoryMember.Summary) -replace '\|', '\|' -replace '[\r\n]+', ' '
        $memberUrl = "/$kindPlural/$memberScope/$($categoryMember.Name)/"
        "| [``$($categoryMember.Name)``]({{ '$memberUrl' | relative_url }}) | $memberSummary |"
    }

    $categoryModel = [ordered] @{
        title               = $categoryPageTitle
        parent_title        = $categoryParentTitle
        grand_parent_line   = $categoryGrandParentLine
        nav_order           = $categoryNavOrder
        kind_plural         = $kindPlural
        scope_path          = $categoryPermalink
        root_scope          = $rootScopeName
        category            = $categoryName
        category_tag_line   = if ($isRootCategory) { "" } else { "  - $categoryName" }
        display_scope       = $categoryScope
        member_rows         = $memberRows -join "`n"
    }

    $generatedPaths.Add([System.IO.Path]::GetFullPath($categoryDestinationPath)) | Out-Null
    Write-Host "Generating $Kind category: $categoryDestinationPath"

    $categoryModel |
        ConvertTo-Json -Depth 10 |
        dotnet tool run didot -- `
            -t $resolvedCategoryTemplatePath `
            -e scriban `
            -i `
            -r json `
            -o $categoryDestinationPath

    if ($LASTEXITCODE -ne 0) {
        throw "Didot failed while generating '$categoryDestinationPath'."
    }
}

foreach ($member in $members) {
    $memberName = [string] $member.Name

    foreach ($propertyName in @("Name", "Scope", "Summary", "Parameters", "Aliases")) {
        Assert-RequiredProperty `
            -InputObject $member `
            -PropertyName $propertyName `
            -MemberName $memberName
    }

    if ($memberName -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') {
        throw "Member name '$memberName' is not safe kebab-case."
    }

    $scopeName = ([string] $member.Scope).ToLowerInvariant()
    $scopeSegments = @($scopeName -split '/')
    $rootScopeName = $scopeSegments[0]
    $scopeSlug = ($scopeSegments | ForEach-Object { ConvertTo-Slug $_ }) -join '/'
    $rootScopeTitle = (Get-Culture).TextInfo.ToTitleCase($rootScopeName)
    $hasCategory = $scopeSegments.Count -gt 1
    if ($hasCategory) {
        $categoryTitle = (Get-Culture).TextInfo.ToTitleCase($scopeSegments[-1])
        $parentTitle = "$categoryTitle $kindPlural"
        $grandParentTitle = "$rootScopeTitle $kindPlural"
    } else {
        $parentTitle = "$rootScopeTitle $kindPlural"
        $grandParentTitle = "$kindPluralTitle library"
    }

    if (-not $scopePositions.ContainsKey($scopeSlug)) {
        $scopePositions[$scopeSlug] = 0
    }

    $scopePositions[$scopeSlug]++
    $navOrder = $scopePositions[$scopeSlug] * 10

    $parameters = @(
        foreach ($parameter in @($member.Parameters)) {
            foreach ($propertyName in @("Name", "Optional")) {
                Assert-RequiredProperty `
                    -InputObject $parameter `
                    -PropertyName $propertyName `
                    -MemberName "$memberName parameter"
            }

            $parameterType = ""
            if ($null -ne $parameter.PSObject.Properties["Type"]) {
                $parameterType = [string] $parameter.Type
            }

            if ([string]::IsNullOrWhiteSpace($parameterType) -and $null -ne $parameter.PSObject.Properties["Kind"]) {
                $parameterType = [string] $parameter.Kind
            }

            $parameterVariadic = $false
            if ($null -ne $parameter.PSObject.Properties["Variadic"]) {
                $parameterVariadic = [bool] $parameter.Variadic
            }

            $minimumCardinality = if ($parameterVariadic) {
                if ($null -ne $parameter.PSObject.Properties["MinimumCardinality"]) {
                    [int] $parameter.MinimumCardinality
                } elseif ([bool] $parameter.Optional) {
                    0
                } else {
                    1
                }
            } elseif ([bool] $parameter.Optional) {
                0
            } else {
                1
            }

            $parameterSummary = ""
            if ($null -ne $parameter.PSObject.Properties["Summary"]) {
                $parameterSummary = [string] $parameter.Summary
            }

            [ordered] @{
                name     = [string] $parameter.Name
                type     = $parameterType
                has_type = -not [string]::IsNullOrWhiteSpace($parameterType)
                optional = [bool] $parameter.Optional
                variadic = $parameterVariadic
                minimum_cardinality = $minimumCardinality
                summary  = $parameterSummary
            }
        }
    )

    $aliases = @($member.Aliases | ForEach-Object { [string] $_ })

    $examples = @()
    if ($null -ne $member.PSObject.Properties["Examples"]) {
        $examples = @($member.Examples | ForEach-Object { [string] $_ })
    }

    $behavior = ""
    if ($null -ne $member.PSObject.Properties["Behavior"]) {
        $behavior = [string] $member.Behavior
    }

    $inputType = ""
    if ($null -ne $member.PSObject.Properties["Input"]) {
        $inputType = [string] $member.Input
    }

    $outputType = ""
    if ($null -ne $member.PSObject.Properties["Output"]) {
        $outputType = [string] $member.Output
    }

    $hasContract = -not [string]::IsNullOrWhiteSpace($inputType) -and
                   -not [string]::IsNullOrWhiteSpace($outputType)
    $hasParameterTypes = @($parameters | Where-Object has_type).Count -gt 0

    $signatureLines = [System.Collections.Generic.List[string]]::new()
    if ($hasContract) {
        $signatureLines.Add("$inputType →")
    }

    if ($parameters.Count -eq 0) {
        $signatureLines.Add("$memberName()$(if ($hasContract) { " → $outputType" })")
    } else {
        $signatureLines.Add("$memberName(")
        for ($index = 0; $index -lt $parameters.Count; $index++) {
            $parameter = $parameters[$index]
            $optionalMarker = if ($parameter.optional -and -not $parameter.variadic) { "?" } else { "" }
            $variadicMarker = if ($parameter.variadic) { "..." } else { "" }
            $typeAnnotation = if ($parameter.has_type) { ": $($parameter.type)" } else { "" }
            $separator = if ($index -lt $parameters.Count - 1) { "," } else { "" }
            $signatureLines.Add("    $variadicMarker$($parameter.name)$optionalMarker$typeAnnotation$separator")
        }

        $signatureLines.Add(")$(if ($hasContract) { " → $outputType" })")
    }

    $parameterRows = foreach ($parameter in $parameters) {
        $required = if ($parameter.variadic) {
            $minimumLabel = switch ($parameter.minimum_cardinality) {
                0 { "zero" }
                1 { "one" }
                2 { "two" }
                default { [string] $parameter.minimum_cardinality }
            }
            "Variadic ($minimumLabel or more)"
        } elseif ($parameter.optional) { "No" } else { "Yes" }
        $summary = ([string] $parameter.summary) -replace '\|', '\|' -replace '[\r\n]+', ' '
        if ($hasParameterTypes) {
            $type = if ($parameter.has_type) { "``$($parameter.type)``" } else { "Not specified" }
            "| ``$($parameter.name)`` | $type | $required | $summary |"
        } else {
            "| ``$($parameter.name)`` | $required | $summary |"
        }
    }

    $aliasesText = if ($aliases.Count -eq 0) {
        "None"
    } else {
        ($aliases | ForEach-Object { "``$_``" }) -join ", "
    }

    $referenceText = @(
        "**Kind:** $Kind"
        "**Scope:** ``$scopeName``"
        "**Aliases:** $aliasesText"
    ) -join "  `n"

    $model = [ordered] @{
        name                = $memberName
        kind                = $kindName
        kind_title          = $Kind
        kind_plural         = $kindPlural
        kind_plural_title   = $kindPluralTitle
        scope               = $scopeName
        parent_title        = $parentTitle
        grand_parent_title  = $grandParentTitle
        scope_slug          = $scopeSlug
        input               = $inputType
        output              = $outputType
        has_contract        = $hasContract
        signature           = $signatureLines -join "`n"
        summary             = [string] $member.Summary
        behavior            = $behavior
        has_behavior        = -not [string]::IsNullOrWhiteSpace($behavior)
        parameters          = $parameters
        has_parameter_types = $hasParameterTypes
        parameter_rows      = $parameterRows -join "`n"
        aliases             = $aliases
        aliases_text        = $aliasesText
        reference_text      = $referenceText
        examples            = $examples
        examples_text       = $examples -join "`n"
        nav_order           = $navOrder
    }

    $scopeDestination = Join-Path $collectionDestination $scopeSlug
    New-Item -ItemType Directory -Path $scopeDestination -Force | Out-Null

    $destinationPath = Join-Path $scopeDestination "$memberName.md"
    $generatedPaths.Add([System.IO.Path]::GetFullPath($destinationPath)) | Out-Null
    Write-Host "Generating $Kind reference: $destinationPath"

    $model |
        ConvertTo-Json -Depth 20 |
        dotnet tool run didot -- `
            -t $resolvedTemplatePath `
            -e scriban `
            -i `
            -r json `
            -o $destinationPath

    if ($LASTEXITCODE -ne 0) {
        throw "Didot failed while generating '$destinationPath'."
    }

    if (-not (Test-Path -LiteralPath $destinationPath -PathType Leaf)) {
        throw "Didot did not create '$destinationPath'."
    }
}

$deletedFileCount = 0
foreach ($existingPage in Get-ChildItem -LiteralPath $collectionDestination -Recurse -File -Filter "*.md") {
    $existingPath = [System.IO.Path]::GetFullPath($existingPage.FullName)
    if ($generatedPaths.Contains($existingPath)) {
        continue
    }

    if ($selectedScopes.Count -gt 0) {
        $relativePath = [System.IO.Path]::GetRelativePath($collectionDestination, $existingPath)
        $relativeDirectory = [System.IO.Path]::GetDirectoryName($relativePath)
        $existingScope = if ([string]::IsNullOrWhiteSpace($relativeDirectory)) {
            $existingPage.BaseName -replace "-$([regex]::Escape($kindPlural))$", ""
        } else {
            $relativeDirectory.Replace([System.IO.Path]::DirectorySeparatorChar, '/').ToLowerInvariant()
        }

        $belongsToSelectedScope = @(
            $selectedScopes | Where-Object {
                $existingScope -eq $_ -or $existingScope.StartsWith("$_/", [System.StringComparison]::Ordinal)
            }
        ).Count -gt 0

        if (-not $belongsToSelectedScope) {
            continue
        }
    }

    if (Select-String -LiteralPath $existingPath -Pattern '^generated: true$' -Quiet) {
        Remove-Item -LiteralPath $existingPath
        $deletedFileCount++
        Write-Host "Removed stale generated page: $existingPath"
    }
}

$generationStopwatch.Stop()

Write-Host
Write-Host "Generation summary"
Write-Host "------------------"
Write-Host ("Kind:                {0}" -f $Kind)
Write-Host ("Member pages:        {0}" -f $members.Count)
Write-Host ("Category pages:      {0}" -f ($rootScopes.Count + $categoryScopes.Count))
Write-Host ("Pages generated:     {0}" -f ($members.Count + $rootScopes.Count + $categoryScopes.Count))
Write-Host ("Files deleted:       {0}" -f $deletedFileCount)
Write-Host ("Elapsed time:         {0:hh\:mm\:ss\.fff}" -f $generationStopwatch.Elapsed)
