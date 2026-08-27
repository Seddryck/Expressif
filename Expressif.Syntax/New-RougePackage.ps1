param(
    [Parameter(Mandatory = $true)]
    [string] $Version,

    [Parameter(Mandatory = $false)]
    [string] $InputFolder = "..\docs\_data",

    [Parameter(Mandatory = $false)]
    [string] $OutputFolder = ".\bin",

    [Parameter(Mandatory = $false)]
    [string] $StagingFolder = ".\obj"
)

$ErrorActionPreference = 'Stop'

if ($Version -notmatch '^[0-9A-Za-z][0-9A-Za-z.+-]*$') {
    throw "Version '$Version' is not valid for a package name."
}

$packageIdentity = "expressif-$Version-rouge"
$packageFolder = Join-Path $StagingFolder $packageIdentity
$outputPath = Join-Path $OutputFolder "$packageIdentity.zip"

if (Test-Path -LiteralPath $packageFolder) {
    Remove-Item -LiteralPath $packageFolder -Recurse -Force
}

New-Item -ItemType Directory -Path $packageFolder -Force | Out-Null
New-Item -ItemType Directory -Path $OutputFolder -Force | Out-Null

Write-Host "Generating standalone Rouge lexer..."
& "$PSScriptRoot\New-RougeLexer.ps1" `
    -InputFolder $InputFolder `
    -OutputPath (Join-Path $packageFolder 'expressif-rouge.rb') `
    -Standalone $true

Write-Host "Copying Rouge package README..."
Copy-Item `
    -LiteralPath "$PSScriptRoot\README-Rouge.md" `
    -Destination (Join-Path $packageFolder 'README.md')

if (Test-Path -LiteralPath $outputPath) {
    Remove-Item -LiteralPath $outputPath -Force
}

Write-Host "Creating Rouge package..."
Compress-Archive -LiteralPath $packageFolder -DestinationPath $outputPath
Remove-Item -LiteralPath $packageFolder -Recurse -Force

Write-Host "Generated: $outputPath"
