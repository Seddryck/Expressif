#requires -PSEdition Core
param (
    [Parameter()]
    [string] $CatalogPath = "docs/_data",

    [Parameter()]
    [string] $SitePath = "docs/_site"
)

$ErrorActionPreference = "Stop"
Set-Location -Path $PSScriptRoot

$expected = @{}
@(
    foreach ($kind in @("function", "predicate", "accumulator")) {
        $catalog = Get-Content -LiteralPath (Join-Path $CatalogPath "$kind.json") -Raw | ConvertFrom-Json
        foreach ($member in @($catalog | Where-Object { $_.IsPublic -eq $true -and $_.Deprecated -eq $true })) {
            [ordered] @{
                Key         = "$kind|$($member.Name)"
                Replacement = if ($null -ne $member.Replacement) { [string] $member.Replacement } else { "" }
                Sunset      = if ($null -ne $member.Sunset) { [string] $member.Sunset } else { "" }
            }
        }
    }
) | ForEach-Object { $expected[$_.Key] = $_ }

$pagePath = Join-Path $SitePath "deprecations/index.html"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Rendered deprecations page '$pagePath' does not exist."
}

$content = Get-Content -LiteralPath $pagePath -Raw
$actual = @{}
[regex]::Matches(
    $content,
    '<tr data-kind="(function|predicate|accumulator)" data-name="([a-z0-9-]+)" data-replacement="([a-z0-9-]*)" data-sunset="([0-9.]*)">'
) | ForEach-Object {
    $key = "$($_.Groups[1].Value)|$($_.Groups[2].Value)"
    $actual[$key] = [ordered] @{
        Key         = $key
        Replacement = $_.Groups[3].Value
        Sunset      = $_.Groups[4].Value
    }
}

$difference = @(Compare-Object -ReferenceObject @($expected.Keys) -DifferenceObject @($actual.Keys))
if ($difference.Count -gt 0) {
    $details = $difference | ForEach-Object {
        $meaning = if ($_.SideIndicator -eq "<=") { "missing from page" } else { "unexpected on page" }
        "  $($_.InputObject): $meaning"
    }
    throw "Rendered language deprecations do not match the catalogs:`n$($details -join "`n")"
}

foreach ($key in $expected.Keys) {
    foreach ($property in @("Replacement", "Sunset")) {
        if ($actual[$key][$property] -ne $expected[$key][$property]) {
            throw "Rendered $property for '$key' is '$($actual[$key][$property])'; expected '$($expected[$key][$property])'."
        }
    }
}

Write-Host "Validated $($actual.Keys.Count) rendered language deprecation(s)."
