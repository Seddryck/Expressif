function Start-DockerIfNeeded {
    docker info *> $null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Docker is already running."
        return
    }

    Write-Host "Starting Docker Desktop..."
    Start-Process "$Env:ProgramFiles\Docker\Docker\Docker Desktop.exe"

    do {
        Start-Sleep -Seconds 2
        docker info *> $null
    } until ($LASTEXITCODE -eq 0)

    Write-Host "Docker is ready."
}

Start-DockerIfNeeded

docker compose jekyll-compose.yml run --rm jekyll bundle exec jekyll build