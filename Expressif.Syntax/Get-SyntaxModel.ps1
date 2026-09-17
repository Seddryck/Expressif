function Get-IntrospectionEntries {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Introspection file '$Path' does not exist."
    }

    $entries = [System.Collections.Generic.SortedDictionary[string, object]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)
    foreach ($item in @(Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json) |
        Where-Object { $_.IsPublic -eq $true }) {
        foreach ($name in @($item.Name) + @($item.Aliases)) {
            if (-not [string]::IsNullOrWhiteSpace($name) -and -not $entries.ContainsKey($name)) {
                $entry = [ordered]@{ name = $name; scope = $item.Scope }
                if ($null -ne $item.PSObject.Properties["DeprecatedAliases"]) {
                    $lifecycle = @($item.DeprecatedAliases | Where-Object Name -EQ $name)
                    if ($lifecycle.Count -gt 0) {
                        $entry["deprecated"] = $true
                        $entry["replacement"] = $lifecycle[0].Replacement
                        $entry["message"] = $lifecycle[0].Message
                    }
                }
                $entries.Add($name, $entry)
            }
        }
    }

    @($entries.Values)
}

function Get-TypeEntries {
    param([Parameter(Mandatory = $true)][string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Introspection file '$Path' does not exist."
    }

    @(Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json) |
        ForEach-Object { ":$($_.Name)" } |
        Sort-Object -Unique
}

function Get-SyntaxModel {
    param(
        [Parameter(Mandatory = $true)]
        [string] $InputFolder
    )

    $resolvedInputFolder = (Resolve-Path -LiteralPath $InputFolder).Path

    [ordered]@{
        functions = @(Get-IntrospectionEntries -Path (Join-Path $resolvedInputFolder 'function.json'))
        predicates = @(Get-IntrospectionEntries -Path (Join-Path $resolvedInputFolder 'predicate.json'))
        accumulators = @(Get-IntrospectionEntries -Path (Join-Path $resolvedInputFolder 'accumulator.json'))
        types = @(Get-TypeEntries -Path (Join-Path $resolvedInputFolder 'type.json'))
        constants = @('#blank', '#empty', '#false', '#null', '#true')
        operators = @('...', ':>', ':=', '->', '|>', '|?', '|OR', '|XOR', '|AND', '~', '!', '#', '$', '&', '.', '@', '|')
    }
}
