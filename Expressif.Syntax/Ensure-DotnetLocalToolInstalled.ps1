param(
    [Parameter(Mandatory = $true)]
    [string] $ToolName
)

Write-Host "Checking availability of $ToolName as local installation..."

$tools = dotnet tool list --local |
    Select-Object -Skip 2 |
    ForEach-Object {
        ($_ -split '\s{2,}')[0]
    }

if (!($tools -contains $ToolName)) {
    Write-Host "Locally installing $ToolName..."
    dotnet tool install --local $ToolName
} else {
    Write-Host "$ToolName is already installed locally."
}