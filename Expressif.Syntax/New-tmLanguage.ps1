param(
    [Parameter(Mandatory = $true)]
    [string] $InputFolder,

    [Parameter(Mandatory = $false)]
    [string] $OutputPath = ".\obj\syntaxes\expressif.tmLanguage.json"
)

. "$PSScriptRoot\Get-SyntaxModel.ps1"

$syntax = Get-SyntaxModel -InputFolder $InputFolder
$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$model = [ordered]@{
        functions = $syntax.functions | ForEach-Object {
            [ordered]@{ name = $_.name; scope = $_.scope; regex = [regex]::Escape($_.name) }
        }
        predicates = $syntax.predicates | ForEach-Object {
            [ordered]@{ name = $_.name; scope = $_.scope; regex = [regex]::Escape($_.name) }
        }
        constantRegex = ($syntax.constants | ForEach-Object { [regex]::Escape($_) }) -join '|'
        operatorRegex = ($syntax.operators | ForEach-Object {
            ([regex]::Escape($_)).Replace('\', '\\')
        }) -join '|'
    } | ConvertTo-Json -Depth 20

Write-Host "Running Didot via local installation..."
$model | dotnet tool run didot `
    -t .\Expressif.tmLanguage.json.sbn `
    -e scriban `
    -i `
    -r json `
    -o $OutputPath

if ($LASTEXITCODE -ne 0) {
    throw "Didot execution failed"
}

Write-Host "Generated: $OutputPath"
