#requires -PSEdition Core

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Assert-Contains {
    param(
        [Parameter(Mandatory)]
        [string] $Content,

        [Parameter(Mandatory)]
        [string] $Expected,

        [Parameter(Mandatory)]
        [string] $Context
    )

    if (-not $Content.Contains($Expected, [System.StringComparison]::Ordinal)) {
        throw "$Context does not contain expected text:`n$Expected"
    }
}

function Assert-NotContains {
    param(
        [Parameter(Mandatory)]
        [string] $Content,

        [Parameter(Mandatory)]
        [string] $Unexpected,

        [Parameter(Mandatory)]
        [string] $Context
    )

    if ($Content.Contains($Unexpected, [System.StringComparison]::Ordinal)) {
        throw "$Context contains unexpected text:`n$Unexpected"
    }
}

$repositoryRoot = $PSScriptRoot
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) "expressif-reference-$([guid]::NewGuid().ToString('N'))"
$temporaryRoot = [System.IO.Path]::GetFullPath($temporaryRoot)
$systemTemporaryRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())

if (-not $temporaryRoot.StartsWith($systemTemporaryRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Temporary test path '$temporaryRoot' is outside '$systemTemporaryRoot'."
}

New-Item -ItemType Directory -Path $temporaryRoot | Out-Null

try {
    $functionCatalogPath = Join-Path $temporaryRoot "function.json"
    $predicateCatalogPath = Join-Path $temporaryRoot "predicate.json"
    $destinationRoot = Join-Path $temporaryRoot "docs"

    @'
[
  {
    "Name": "map-contract",
    "IsPublic": true,
    "Aliases": [],
    "Scope": "test",
    "Input": "array",
    "Output": "array",
    "Summary": "Maps values.",
    "Semantics": {
      "Cardinality": "preserved",
      "Dependency": "per-element",
      "Ordering": "preserved"
    },
    "Parameters": [
      {
        "Name": "transformation",
        "Type": "expression",
        "Optional": false,
        "Summary": "Transforms a value."
      }
    ],
    "Schema": {
      "Classification": "contract",
      "Input": "array<T>",
      "Output": "array<U>",
      "Parameters": {
        "transformation": {
          "Input": "T",
          "Output": "U"
        }
      },
      "NullableWhen": ["input"]
    }
  },
  {
    "Name": "coalesce-contract",
    "IsPublic": true,
    "Aliases": [],
    "Scope": "test",
    "Input": "any",
    "Output": "any",
    "Summary": "Coalesces values.",
    "Parameters": [
      {
        "Name": "expressions",
        "Type": "expression",
        "Optional": false,
        "Variadic": true,
        "AllowsSpread": false,
        "MinimumCardinality": 2,
        "Summary": "Candidate expressions."
      }
    ],
    "Schema": {
      "Classification": "contract",
      "Input": "T",
      "Output": "nullable<U>",
      "Parameters": {
        "expressions": {
          "Input": "T",
          "Output": "U",
          "Combine": "union"
        }
      }
    }
  },
  {
    "Name": "incremental-sum",
    "IsPublic": true,
    "Aliases": [],
    "Scope": "test",
    "Input": "array",
    "Output": "numeric",
    "Summary": "Sums values.",
    "Incremental": true,
    "Parameters": [],
    "Schema": { "Classification": "fixed" }
  },
  {
    "Name": "replacement",
    "IsPublic": true,
    "Aliases": [],
    "Scope": "test",
    "Input": "any",
    "Output": "any",
    "Summary": "Replacement function.",
    "Parameters": [],
    "Schema": { "Classification": "fixed" }
  },
  {
    "Name": "deprecated-equivalent",
    "IsPublic": true,
    "Aliases": [],
    "Scope": "test",
    "Input": "any",
    "Output": "any",
    "Summary": "Deprecated equivalent function.",
    "Deprecated": true,
    "Replacement": "replacement",
    "ReplacementIsEquivalent": true,
    "Sunset": "3.0",
    "Parameters": [],
    "Schema": { "Classification": "fixed" }
  },
  {
    "Name": "deprecated-different",
    "IsPublic": true,
    "Aliases": [],
    "Scope": "test",
    "Input": "any",
    "Output": "any",
    "Summary": "Deprecated non-equivalent function.",
    "Deprecated": true,
    "Replacement": "replacement",
    "ReplacementIsEquivalent": false,
    "MigrationNotes": "Preserve null input before applying the replacement.",
    "Sunset": "3.0",
    "Parameters": [],
    "Schema": { "Classification": "fixed" }
  },
  {
    "Name": "deprecated-unknown",
    "IsPublic": true,
    "Aliases": [],
    "Scope": "test",
    "Input": "any",
    "Output": "any",
    "Summary": "Deprecated function with unknown compatibility.",
    "Deprecated": true,
    "Replacement": "replacement",
    "Sunset": "3.0",
    "Parameters": [],
    "Schema": { "Classification": "fixed" }
  },
  {
    "Name": "deprecated-without-replacement",
    "IsPublic": true,
    "Aliases": [],
    "Scope": "test",
    "Input": "any",
    "Output": "any",
    "Summary": "Deprecated function without a replacement.",
    "Deprecated": true,
    "Sunset": "3.0",
    "Parameters": [],
    "Schema": { "Classification": "fixed" }
  }
]
'@ | Set-Content -LiteralPath $functionCatalogPath -Encoding utf8NoBOM

    @'
[
  {
    "Name": "predicate-contract",
    "IsPublic": true,
    "Aliases": [],
    "Scope": "test",
    "Input": "array",
    "Output": "boolean",
    "Summary": "Checks an array.",
    "Parameters": [],
    "Schema": {
      "Classification": "contract",
      "Input": "array<T>",
      "Output": "boolean"
    }
  }
]
'@ | Set-Content -LiteralPath $predicateCatalogPath -Encoding utf8NoBOM

    foreach ($catalog in @(
        @{ Kind = "Function"; Path = $functionCatalogPath },
        @{ Kind = "Predicate"; Path = $predicateCatalogPath }
    )) {
        & (Join-Path $repositoryRoot "New-LibraryReferencePages.ps1") `
            -Kind $catalog.Kind `
            -DataPath $catalog.Path `
            -DestinationRoot $destinationRoot
        if ($LASTEXITCODE -ne 0) {
            throw "$($catalog.Kind) reference generation failed with exit code $LASTEXITCODE."
        }
    }

    $functionRoot = Join-Path $destinationRoot "functions/test"
    $mapPage = Get-Content -LiteralPath (Join-Path $functionRoot "map-contract.md") -Raw
    foreach ($expected in @(
        "array<T> →`nmap-contract(`n    transformation: expression`n) → array<U>",
        "## Value shape",
        "- Pipeline input: ``array<T>``",
        "- Returns: ``array<U>``",
        "- ``transformation``: Receives ``T`` and returns ``U``.",
        "- Nullability: The result is nullable when the pipeline input is nullable.",
        "``T``, ``U``, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.",
        "- Cardinality: ``preserved`` <span class=`"semantics-info`" title=`"The output contains the same number of elements as the visited input.`"",
        "- Dependency: ``per-element`` <span class=`"semantics-info`" title=`"An output element depends only on its corresponding visited input element.`"",
        "- Ordering: ``preserved`` <span class=`"semantics-info`" title=`"Relative source order is retained.`""
    )) {
        Assert-Contains -Content $mapPage -Expected $expected -Context "map-contract page"
    }
    Assert-NotContains -Content $mapPage -Unexpected "Classification" -Context "map-contract page"
    if ($mapPage.IndexOf("## Examples", [System.StringComparison]::Ordinal) -gt $mapPage.IndexOf("## Value shape", [System.StringComparison]::Ordinal)) {
        throw "map-contract page does not place Examples immediately after Parameters."
    }

    $coalescePage = Get-Content -LiteralPath (Join-Path $functionRoot "coalesce-contract.md") -Raw
    Assert-Contains -Content $coalescePage -Expected "- Combination: When multiple values are supplied, their output types are combined as a union." -Context "coalesce-contract page"

    $incrementalPage = Get-Content -LiteralPath (Join-Path $functionRoot "incremental-sum.md") -Raw
    Assert-Contains -Content $incrementalPage -Expected "## Aggregation support" -Context "incremental-sum page"
    Assert-Contains -Content $incrementalPage -Expected "This function supports incremental aggregation and can be used with ``fold``, ``scan``, and ``broadcast``." -Context "incremental-sum page"
    Assert-NotContains -Content $incrementalPage -Unexpected "## Value shape" -Context "incremental-sum page"

    $replacementLink = "[``replacement``]({{ '/functions/test/replacement/' | relative_url }})"
    $equivalentPage = Get-Content -LiteralPath (Join-Path $functionRoot "deprecated-equivalent.md") -Raw
    Assert-Contains -Content $equivalentPage -Expected "> Use $replacementLink instead. This replacement is behavior-equivalent." -Context "deprecated-equivalent page"

    $differentPage = Get-Content -LiteralPath (Join-Path $functionRoot "deprecated-different.md") -Raw
    Assert-Contains -Content $differentPage -Expected "> Use $replacementLink instead. This replacement is not behavior-equivalent." -Context "deprecated-different page"
    Assert-Contains -Content $differentPage -Expected "> **Migration:** Preserve null input before applying the replacement." -Context "deprecated-different page"

    $unknownPage = Get-Content -LiteralPath (Join-Path $functionRoot "deprecated-unknown.md") -Raw
    Assert-Contains -Content $unknownPage -Expected "> Use $replacementLink instead. Review the replacement before migrating because compatibility information is unavailable." -Context "deprecated-unknown page"

    $noReplacementPage = Get-Content -LiteralPath (Join-Path $functionRoot "deprecated-without-replacement.md") -Raw
    Assert-Contains -Content $noReplacementPage -Expected "> No direct replacement is available." -Context "deprecated-without-replacement page"

    $predicatePage = Get-Content -LiteralPath (Join-Path $destinationRoot "predicates/test/predicate-contract.md") -Raw
    Assert-Contains -Content $predicatePage -Expected "## Value shape" -Context "predicate-contract page"
    Assert-Contains -Content $predicatePage -Expected "- Pipeline input: ``array<T>``" -Context "predicate-contract page"

    Write-Host "Validated developer-facing library reference metadata."
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }
}
