param ()

$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

$pages = @(
    @{ Class = 'function'; Scope = 'special' }
    @{ Class = 'function'; Scope = 'text' }
    @{ Class = 'function'; Scope = 'numeric' }
    @{ Class = 'function'; Scope = 'temporal' }
    @{ Class = 'function'; Scope = 'io' }
    @{ Class = 'function'; Scope = 'array' }
    @{ Class = 'function'; Scope = 'record' }
    @{ Class = 'accumulator'; Scope = 'array' }
    @{ Class = 'predicate'; Scope = 'special' }
    @{ Class = 'predicate'; Scope = 'text' }
    @{ Class = 'predicate'; Scope = 'numeric' }
    @{ Class = 'predicate'; Scope = 'temporal' }
    @{ Class = 'predicate'; Scope = 'boolean' }
)

foreach ($page in $pages) {
    & ./update-docs.ps1 -Class $page.Class -Scope $page.Scope
}

& ./update-index.ps1
