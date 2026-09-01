$ErrorActionPreference = "Stop"

$RepositoryRoot = $PSScriptRoot
$ComposeFile = Join-Path $RepositoryRoot "jekyll-compose.yml"

Push-Location $RepositoryRoot

try {
    Write-Host "Restoring .NET tools..."
    dotnet tool restore

    Write-Host "Generating Rouge lexer..."
    .\Expressif.Syntax\New-RougeLexer.ps1 `
        -InputFolder .\docs\_data `
        -OutputPath .\docs\_plugins\expressif.rb `
        -Standalone $true

    Write-Host "Stopping existing Jekyll container if any..."
    docker compose -f $ComposeFile down --remove-orphans

    Write-Host "Starting Jekyll..."
    docker compose -f $ComposeFile up
}
finally {
    Pop-Location
}
