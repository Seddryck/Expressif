param(
    [Parameter(Mandatory = $true)]
    [string] $Version,

    [Parameter(Mandatory = $false)]
    [string] $OutputPath = ".\obj\package.json"
)

$model = [ordered]@{
        version = $Version
    }
    | ConvertTo-Json -Depth 20

Write-Host "Model sets version to value '$Version'."

Write-Host "Running Didot via local installation..."
$model | dotnet tool run didot `
    -t .\package.json.sbn `
    -e scriban `
    -i `
    -r json `
    -o $OutputPath `

if ($LASTEXITCODE -ne 0) {
    throw "Didot execution failed"
}

Write-Host "Generated: $OutputPath"