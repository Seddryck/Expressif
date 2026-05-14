
Remove-Item .\docs\Gemfile.lock -Force -ErrorAction SilentlyContinue

$ErrorActionPreference = "Stop"

$ComposeFile = "jekyll-compose.yml"

Write-Host "Stopping existing Jekyll container if any..."
docker compose -f $ComposeFile down --remove-orphans

Write-Host "Starting Jekyll..."
docker compose -f $ComposeFile up