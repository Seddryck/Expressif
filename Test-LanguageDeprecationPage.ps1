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
    '<tr data-kind="(function|predicate|accumulator)" data-name="([^"]+)" data-replacement="([^"]*)" data-sunset="([^"]*)">'
) | ForEach-Object {
    $key = "$($_.Groups[1].Value)|$($_.Groups[2].Value)"
    $actual[$key] = [ordered] @{
        Key         = $key
        Replacement = [System.Net.WebUtility]::HtmlDecode($_.Groups[3].Value)
        Sunset      = [System.Net.WebUtility]::HtmlDecode($_.Groups[4].Value)
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

$rules = @(Get-Content -LiteralPath (Join-Path $CatalogPath 'usage-lifecycle.json') -Raw | ConvertFrom-Json | Where-Object Active)
$sections = @([regex]::Matches($content, '(?s)<section id="[^"]*" data-rule-id="([^"]+)" data-deprecated-since="([^"]*)" data-sunset="([^"]*)" data-introduced-commit="([^"]*)">(.*?)</section>'))
if ($sections.Count -ne $rules.Count) { throw 'Rendered usage rule count does not match the shared lifecycle rules.' }
foreach ($rule in $rules) {
    $matches = @($sections | Where-Object { $_.Groups[1].Value -ceq $rule.Id })
    if ($matches.Count -ne 1) { throw "Expected exactly one section for stable rule '$($rule.Id)'." }
    $section = $matches[0]
    foreach ($field in @(@('DeprecatedSince', 2), @('Sunset', 3), @('IntroducedCommit', 4))) {
        $value = [System.Net.WebUtility]::HtmlDecode($section.Groups[$field[1]].Value)
        if ($value -cne [string] $rule.($field[0])) { throw "Incorrect $($field[0]) for '$($rule.Id)'." }
    }
    $body = $section.Groups[5].Value
    $visible = [System.Net.WebUtility]::HtmlDecode([regex]::Replace($body, '<[^>]+>', ''))
    if (-not $visible.Contains('Deprecation: Active now.')) { throw "Missing active deprecation for '$($rule.Id)'." }
    $sunset = if ($rule.Sunset) { "Removal planned for v$($rule.Sunset)" } else { 'Not scheduled' }
    if (-not $visible.Contains($sunset)) { throw "Missing sunset wording for '$($rule.Id)'." }
    $version = if ($rule.DeprecatedSince) { "Since Expressif $($rule.DeprecatedSince)." } else { 'Introducing release version pending' }
    if (-not $visible.Contains($version)) { throw "Missing introducing version for '$($rule.Id)'." }
    $rows = @([regex]::Matches($body, '(?s)<tr data-deprecated="([^"]*)" data-replacement="([^"]*)" data-availability="([^"]*)">(.*?)</tr>'))
    if ($rows.Count -ne $rule.Examples.Count) { throw "Incorrect migration example count for '$($rule.Id)'." }
    for ($index = 0; $index -lt $rows.Count; $index++) {
        $example = $rule.Examples[$index]
        foreach ($field in @(@('Deprecated', 1), @('Replacement', 2), @('Availability', 3))) {
            $value = [System.Net.WebUtility]::HtmlDecode($rows[$index].Groups[$field[1]].Value)
            if ($value -cne $example.($field[0])) { throw "Truncated or incorrect $($field[0]) for '$($rule.Id)'." }
        }
        $row = [System.Net.WebUtility]::HtmlDecode([regex]::Replace($rows[$index].Groups[4].Value, '<[^>]+>', ''))
        foreach ($value in @($example.Deprecated, $example.Replacement, $example.AppliesWhen, $example.Expected)) {
            if (-not $row.Contains($value)) { throw "Missing visible migration text '$value'." }
        }
        $availability = if ($example.Availability -eq 'supported') { 'Supported on this development line.' } else { 'Planned; not yet supported.' }
        if (-not $row.Contains($availability)) { throw "Incorrect replacement availability for '$($rule.Id)'." }
    }
}
foreach ($state in @(
    @('There are currently no deprecated public language callables.', $expected.Count),
    @('There are currently no deprecated usage patterns.', $rules.Count)
)) {
    if ($content.Contains($state[0]) -ne ($state[1] -eq 0)) { throw "Incorrect empty state: $($state[0])" }
}
Write-Host "Validated $($rules.Count) rendered usage lifecycle rule(s)."
