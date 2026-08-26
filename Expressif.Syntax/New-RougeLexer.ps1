param(
    [Parameter(Mandatory = $true)]
    [string] $InputFolder,

    [Parameter(Mandatory = $false)]
    [string] $OutputPath = ".\bin\expressif.rb"
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\Get-SyntaxModel.ps1"

$syntax = Get-SyntaxModel -InputFolder $InputFolder
$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$catalogEntries = @($syntax.functions) + @($syntax.predicates) + @($syntax.accumulators)
$catalogNames = $catalogEntries |
    ForEach-Object { $_.name } |
    Sort-Object -Unique |
    Sort-Object @{ Expression = { $_.Length }; Descending = $true },
        @{ Expression = { $_ }; Descending = $false }

$model = [ordered]@{
        functionRegexes = @($catalogNames | ForEach-Object { [regex]::Escape($_) })
    } | ConvertTo-Json -Depth 20

Write-Host "Running Didot via local installation..."
$model | dotnet tool run didot `
    -t .\Expressif.Rouge.rb.sbn `
    -e scriban `
    -i `
    -r json `
    -o $OutputPath

if ($LASTEXITCODE -ne 0) {
    throw "Didot execution failed"
}

Write-Host "Generated: $OutputPath"
