[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet('ubuntu', 'alpine')]
    [string] $Distribution
)

$contextPath = Join-Path $PSScriptRoot $Distribution
$imageTag = "expressif:$($Distribution.ToLowerInvariant())"

if (-not (Test-Path -Path $contextPath -PathType Container)) {
    throw "Container directory not found: $contextPath"
}

$dockerfilePath = Join-Path $contextPath 'Dockerfile'

if (-not (Test-Path -Path $dockerfilePath -PathType Leaf)) {
    throw "Dockerfile not found: $dockerfilePath"
}

Write-Host "Building Docker image '$imageTag' from '$contextPath'..."

docker build `
    --no-cache `
    --tag $imageTag `
    $contextPath

if ($LASTEXITCODE -ne 0) {
    throw "Docker build failed with exit code $LASTEXITCODE."
}

Write-Host "Successfully built Docker image '$imageTag'."