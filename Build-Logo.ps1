[CmdletBinding()]
param(
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'artifacts/branding'),
    [string] $Version
)

$ErrorActionPreference = 'Stop'

if (Get-Command magick -ErrorAction SilentlyContinue) {
    $convertCommand = 'magick'
    $convertPrefix = @()
    $identifyCommand = 'magick'
    $identifyPrefix = @('identify')
}
elseif ((Get-Command convert -ErrorAction SilentlyContinue) -and (Get-Command identify -ErrorAction SilentlyContinue)) {
    $convertCommand = 'convert'
    $convertPrefix = @()
    $identifyCommand = 'identify'
    $identifyPrefix = @()
}
else {
    throw 'ImageMagick is required. Install it with winget install ImageMagick.ImageMagick or apt-get install imagemagick.'
}

$sourceDirectory = Join-Path $PSScriptRoot 'assets/logo'
$outputPath = [System.IO.Path]::GetFullPath($OutputDirectory)
$temporaryPath = Join-Path $outputPath ('.tmp-' + [System.Guid]::NewGuid().ToString('N'))
$iconSizes = 256, 128, 64, 32, 16
$logoSizes = 256, 128
$darkCupColor = '#121729'
$palettes = [ordered]@{
    light = @{
        Background = $darkCupColor
        Structure = '#F9F6F2'
        Accent = '#E1CAAC'
    }
    dark = @{
        Background = '#F9F6F2'
        Structure = $darkCupColor
        Accent = '#9A6D47'
    }
}

function Invoke-ImageMagick {
    param([Parameter(Mandatory)][string[]] $Arguments)

    & $convertCommand @convertPrefix @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "ImageMagick failed with exit code $LASTEXITCODE."
    }
}

function New-ColoredLayer {
    param(
        [Parameter(Mandatory)][string] $Source,
        [Parameter(Mandatory)][string] $Color,
        [Parameter(Mandatory)][string] $Destination
    )

    Invoke-ImageMagick @(
        $Source,
        '-channel', 'RGB',
        '-fill', $Color,
        '-colorize', '100',
        '+channel',
        $Destination
    )
}

function New-Composition {
    param(
        [Parameter(Mandatory)][string[]] $Layers,
        [Parameter(Mandatory)][string] $Destination
    )

    $flattened = "$Destination.flattened.png"
    $trimmed = "$Destination.trimmed.png"

    try {
        Invoke-ImageMagick @(
            $Layers
            '-background', 'none'
            '-layers', 'flatten'
            $flattened
        )
        Invoke-ImageMagick @($flattened, '-trim', '+repage', $trimmed)

        $dimensions = (& $identifyCommand @identifyPrefix -format '%w %h' $trimmed) -split ' '
        if ($LASTEXITCODE -ne 0 -or $dimensions.Count -ne 2) {
            throw 'ImageMagick could not determine the composition dimensions.'
        }

        $contentSize = [Math]::Max([int] $dimensions[0], [int] $dimensions[1])
        $padding = [Math]::Ceiling($contentSize * 0.075)
        $canvasSize = $contentSize + (2 * $padding)

        Invoke-ImageMagick @(
            $trimmed,
            '-background', 'none',
            '-bordercolor', 'none',
            '-border', "${padding}x${padding}",
            '-gravity', 'center',
            '-extent', "${canvasSize}x${canvasSize}",
            $Destination
        )
    }
    finally {
        Remove-Item -LiteralPath $flattened, $trimmed -Force -ErrorAction SilentlyContinue
    }
}

function New-ResizedPng {
    param(
        [Parameter(Mandatory)][string] $Source,
        [Parameter(Mandatory)][int] $Size,
        [Parameter(Mandatory)][string] $Destination
    )

    Invoke-ImageMagick @(
        $Source,
        '-filter', 'Lanczos'
        '-resize', "${Size}x${Size}"
        '-strip'
        '-define', 'png:exclude-chunk=date,time'
        $Destination
    )
}

New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
New-Item -ItemType Directory -Path $temporaryPath -Force | Out-Null

try {
    foreach ($paletteName in $palettes.Keys) {
        $palette = $palettes[$paletteName]
        $palettePath = Join-Path $temporaryPath $paletteName
        New-Item -ItemType Directory -Path $palettePath -Force | Out-Null

        $circle = Join-Path $palettePath 'circle.png'
        $cup = Join-Path $palettePath 'cup.png'
        $liquid = Join-Path $palettePath 'liquid.png'
        $steam = Join-Path $palettePath 'steam.png'
        $text = Join-Path $palettePath 'text.png'

        New-ColoredLayer -Source (Join-Path $sourceDirectory 'layers/circle.png') -Color $palette.Background -Destination $circle
        New-ColoredLayer -Source (Join-Path $sourceDirectory 'layers/cup.png') -Color $palette.Structure -Destination $cup
        New-ColoredLayer -Source (Join-Path $sourceDirectory 'layers/liquid.png') -Color $palette.Accent -Destination $liquid
        New-ColoredLayer -Source (Join-Path $sourceDirectory 'layers/steam.png') -Color $palette.Accent -Destination $steam
        New-ColoredLayer -Source (Join-Path $sourceDirectory 'text.png') -Color $palette.Accent -Destination $text

        $iconComposition = Join-Path $palettePath 'icon.png'
        $logoComposition = Join-Path $palettePath 'logo.png'
        $backgroundIconComposition = Join-Path $palettePath 'icon-background.png'
        $backgroundLogoComposition = Join-Path $palettePath 'logo-background.png'
        New-Composition -Layers @($cup, $liquid, $steam) -Destination $iconComposition
        New-Composition -Layers @($cup, $liquid, $steam, $text) -Destination $logoComposition
        New-Composition -Layers @($circle, $cup, $liquid, $steam) -Destination $backgroundIconComposition
        New-Composition -Layers @($circle, $cup, $liquid, $steam, $text) -Destination $backgroundLogoComposition

        foreach ($size in $iconSizes) {
            New-ResizedPng -Source $iconComposition -Size $size -Destination (Join-Path $outputPath "expressif-icon-$paletteName-$size.png")
            New-ResizedPng -Source $backgroundIconComposition -Size $size -Destination (Join-Path $outputPath "expressif-icon-background-$paletteName-$size.png")
        }

        foreach ($size in $logoSizes) {
            New-ResizedPng -Source $logoComposition -Size $size -Destination (Join-Path $outputPath "expressif-logo-$paletteName-$size.png")
            New-ResizedPng -Source $backgroundLogoComposition -Size $size -Destination (Join-Path $outputPath "expressif-logo-background-$paletteName-$size.png")
        }

        $iconFrames = @(
            Join-Path $outputPath "expressif-logo-background-$paletteName-256.png"
            Join-Path $outputPath "expressif-logo-background-$paletteName-128.png"
            Join-Path $outputPath "expressif-icon-background-$paletteName-64.png"
            Join-Path $outputPath "expressif-icon-background-$paletteName-32.png"
            Join-Path $outputPath "expressif-icon-background-$paletteName-16.png"
        )
        $iconDestination = Join-Path $outputPath "expressif-$paletteName.ico"
        Invoke-ImageMagick ($iconFrames + $iconDestination)
    }

    Copy-Item `
        -LiteralPath (Join-Path $outputPath 'expressif-dark.ico') `
        -Destination (Join-Path $outputPath 'favicon.ico') `
        -Force

    if ($Version) {
        if ($Version.IndexOfAny([System.IO.Path]::GetInvalidFileNameChars()) -ge 0) {
            throw "Version '$Version' contains characters that are invalid in a file name."
        }

        $archivePath = Join-Path $outputPath "expressif-branding-$Version.zip"
        Remove-Item -LiteralPath $archivePath -Force -ErrorAction SilentlyContinue
        $archiveStream = [System.IO.File]::Open($archivePath, [System.IO.FileMode]::CreateNew)

        try {
            $archive = [System.IO.Compression.ZipArchive]::new(
                $archiveStream,
                [System.IO.Compression.ZipArchiveMode]::Create,
                $false
            )

            try {
                $brandingFiles = Get-ChildItem -LiteralPath $outputPath -File |
                    Where-Object Extension -In @('.png', '.ico') |
                    Sort-Object Name

                foreach ($brandingFile in $brandingFiles) {
                    $entry = $archive.CreateEntry(
                        $brandingFile.Name,
                        [System.IO.Compression.CompressionLevel]::Optimal
                    )
                    $entry.LastWriteTime = [System.DateTimeOffset]::new(1980, 1, 1, 0, 0, 0, [System.TimeSpan]::Zero)
                    $entryStream = $entry.Open()
                    $sourceStream = $brandingFile.OpenRead()

                    try {
                        $sourceStream.CopyTo($entryStream)
                    }
                    finally {
                        $sourceStream.Dispose()
                        $entryStream.Dispose()
                    }
                }
            }
            finally {
                $archive.Dispose()
            }
        }
        finally {
            $archiveStream.Dispose()
        }
    }
}
finally {
    if (Test-Path -LiteralPath $temporaryPath) {
        Remove-Item -LiteralPath $temporaryPath -Recurse -Force
    }
}

Write-Output $outputPath
