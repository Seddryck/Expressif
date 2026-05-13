param(
    [Parameter(Mandatory = $true)]
    [string] $Bundle,
    [Parameter(Mandatory = $true)]
    [string] $Version
)

function New-VsCodeExtension {
    param(
        [Parameter(Mandatory = $true)]
        [string] $InputFolder,
        [Parameter(Mandatory = $true)]
        [string] $OutputPath
    )

    $ErrorActionPreference = "Stop"

    if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
        throw "npm is not installed or not available in PATH."
    }

    Push-Location $InputFolder
    try {
        Write-Host "-> Installing packaging tool through npx"
        npx --yes @vscode/vsce package --out "..\$OutputPath"
    }
    finally {
        Pop-Location
    }

    Write-Host "-> VSIX package generated in $OutputFolder"
}

function Copy-RemoteText {
    param(
        [Parameter(Mandatory)]
        [string] $Url,

        [Parameter(Mandatory)]
        [string] $OutputPath
    )

    $ErrorActionPreference = "Stop"

    Write-Host "-> Downloading from $Url"

    $response = Invoke-WebRequest -Uri $Url

    if (-not $response.Content) {
        throw "No content returned from $Url"
    }

    $directory = Split-Path $OutputPath -Parent
    if ($directory -and -not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    Write-Host "-> Writing to $OutputPath"

    $response.Content | Set-Content -Path $OutputPath -Encoding utf8

    Write-Host "-> Done"
}

$ErrorActionPreference = "Stop"

# --- Build flow ---

Write-Host "== Clean =="
Remove-Item .\obj, .\bin -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "== Create folders =="
New-Item -ItemType Directory -Force .\obj | Out-Null
New-Item -ItemType Directory -Force .\obj\syntaxes | Out-Null
New-Item -ItemType Directory -Force .\obj\images | Out-Null
New-Item -ItemType Directory -Force .\bin | Out-Null

Write-Host "== Generate tmLanguage =="
.\New-TmLanguage.ps1 `
    -InputFolder "..\docs\_data\" `
    -OutputPath ".\obj\syntaxes\expressif.tmLanguage.json"

Write-Host "== Generate package.json =="
.\New-PackageJson.ps1 `
   -Version $Version `
   -OutputPath ".\obj\package.json"

Write-Host "== Copy license =="
Copy-RemoteText `
    -Url "https://raw.githubusercontent.com/Seddryck/Expressif/main/LICENSE" `
    -OutputPath ".\obj\LICENSE"

Write-Host "== Stage extension files =="
Copy-Item .\language-configuration.json .\obj\language-configuration.json
Copy-Item .\README .\obj\README
Copy-Item ..\misc\icon\expressif-icon-128.png .\obj\images\icon.png

Write-Host "== Package extension =="
New-VsCodeExtension `
    -InputFolder ".\obj\" `
    -OutputPath ".\bin\$Bundle-$Version.vsix"

Write-Host "== Done =="
Write-Host "Output: .\bin\$Bundle-$Version.vsix"
