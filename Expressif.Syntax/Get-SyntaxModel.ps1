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
                $entries.Add($name, [ordered]@{ name = $name; scope = $item.Scope })
            }
        }
    }

    @($entries.Values)
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
        constants = @('#blank', '#empty', '#false', '#null', '#true')
        operators = @('...', ':=', '|>', '|?', '|OR', '|XOR', '|AND', '!', '#', '$', '&', '.', '@', '|')
    }
}
