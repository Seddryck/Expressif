param(
    [Parameter(Mandatory = $true)]
    [string] $InputFolder,

    [Parameter(Mandatory = $false)]
    [string] $OutputPath = ".\obj\syntaxes\expressif.tmLanguage.json"
)

$model = [ordered]@{
        functions =  Get-Content $InputFolder\function.json -Raw |
            ConvertFrom-Json |
            Where-Object { $_.IsPublic -eq $true } |
            ForEach-Object {
                [ordered]@{
                    name = $_.Name
                    scope = $_.Scope
                    regex = [regex]::Escape($_.Name)
                }
            } 
        predicates =  Get-Content $InputFolder\predicate.json -Raw |
            ConvertFrom-Json |
            Where-Object { $_.IsPublic -eq $true } |
            ForEach-Object {
                [ordered]@{
                    name = $_.Name
                    scope = $_.Scope
                    regex = [regex]::Escape($_.Name)
                }
            }
    }
    | ConvertTo-Json -Depth 20

Write-Host "Running Didot via local installation..."
$model | dotnet tool run didot `
    -t .\Expressif.tmLanguage.json.sbn `
    -e scriban `
    -i `
    -r json `
    -o $OutputPath `

if ($LASTEXITCODE -ne 0) {
    throw "Didot execution failed"
}

Write-Host "Generated: $OutputPath"