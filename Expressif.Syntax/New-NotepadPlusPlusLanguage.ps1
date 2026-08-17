param(
    [Parameter(Mandatory = $true)]
    [string] $InputFolder,

    [Parameter(Mandatory = $false)]
    [string] $OutputPath = ".\bin\expressif.xml"
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\Get-SyntaxModel.ps1"

$syntax = Get-SyntaxModel -InputFolder $InputFolder
$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$model = [ordered]@{
        functions = [System.Security.SecurityElement]::Escape(
            (($syntax.functions | ForEach-Object { $_.name }) -join ' '))
        predicates = [System.Security.SecurityElement]::Escape(
            (($syntax.predicates | ForEach-Object { $_.name }) -join ' '))
        accumulators = [System.Security.SecurityElement]::Escape(
            (($syntax.accumulators | ForEach-Object { $_.name }) -join ' '))
        constants = [System.Security.SecurityElement]::Escape(($syntax.constants -join ' '))
        operators = [System.Security.SecurityElement]::Escape(($syntax.operators -join ' '))
    } | ConvertTo-Json -Depth 20

Write-Host "Running Didot via local installation..."
$model | dotnet tool run didot `
    -t .\Expressif.NotepadPlusPlus.xml.sbn `
    -e scriban `
    -i `
    -r json `
    -o $OutputPath

if ($LASTEXITCODE -ne 0) {
    throw "Didot execution failed"
}

Write-Host "Generated: $OutputPath"
