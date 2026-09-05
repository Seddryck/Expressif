param(
    [Parameter(Mandatory = $true)]
    [string] $PackageDirectory,
    [string] $PackageId = 'Expressif-cli',
    [string] $CommandName = 'expressif',
    [string] $TargetFrameworks = 'net8.0;net9.0;net10.0',
    [string] $RuntimeIdentifiers = 'win-x64;win-arm64;linux-x64;linux-musl-x64'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

$expectedFrameworks = @($TargetFrameworks.Split(';', [StringSplitOptions]::RemoveEmptyEntries))
$expectedRuntimes = @($RuntimeIdentifiers.Split(';', [StringSplitOptions]::RemoveEmptyEntries))
$packages = @(Get-ChildItem -LiteralPath $PackageDirectory -File -Filter '*.nupkg' |
    Where-Object { $_.Name -notlike '*.symbols.nupkg' })

if ($packages.Count -ne ($expectedRuntimes.Count + 1)) {
    throw "Expected $($expectedRuntimes.Count + 1) CLI packages, found $($packages.Count)."
}

function Read-ZipEntry {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.Compression.ZipArchiveEntry] $Entry
    )

    $reader = [IO.StreamReader]::new($Entry.Open())
    try {
        return $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }
}

function Assert-CommandSettings {
    param(
        [Parameter(Mandatory = $true)]
        [xml] $Settings,
        [Parameter(Mandatory = $true)]
        [string] $PackageName,
        [string] $ExpectedEntryPoint,
        [switch] $Pointer
    )

    if ($Settings.DotNetCliTool.GetAttribute('Version') -ne '2') {
        throw "$PackageName has an unsupported DotnetToolSettings.xml version."
    }

    $commands = @($Settings.DotNetCliTool.Commands.Command)
    if ($commands.Count -ne 1 -or $commands[0].GetAttribute('Name') -ne $CommandName) {
        throw "$PackageName must expose exactly the '$CommandName' command."
    }

    if ($Pointer) {
        if ($commands[0].GetAttribute('EntryPoint') -or $commands[0].GetAttribute('Runner')) {
            throw "$PackageName pointer settings must not declare an entry point or runner."
        }

        $runtimePackages = @($Settings.DotNetCliTool.RuntimeIdentifierPackages.RuntimeIdentifierPackage)
        if ($runtimePackages.Count -ne $expectedRuntimes.Count) {
            throw "$PackageName must reference exactly $($expectedRuntimes.Count) runtime packages."
        }

        foreach ($runtime in $expectedRuntimes) {
            $matchingRuntimePackages = @($runtimePackages | Where-Object {
                $_.GetAttribute('RuntimeIdentifier') -eq $runtime -and
                    $_.GetAttribute('Id') -eq "$PackageId.$runtime"
            })
            if ($matchingRuntimePackages.Count -ne 1) {
                throw "$PackageName must reference runtime '$runtime' as '$PackageId.$runtime' exactly once."
            }
        }
    }
    elseif ($commands[0].GetAttribute('EntryPoint') -ne $ExpectedEntryPoint -or
        $commands[0].GetAttribute('Runner') -ne 'executable') {
        throw "$PackageName must run '$ExpectedEntryPoint' with the executable runner."
    }
}

$seenPointer = $false
$seenRuntimes = @{}

foreach ($package in $packages) {
    $archive = [IO.Compression.ZipFile]::OpenRead($package.FullName)
    try {
        $nuspecEntries = @($archive.Entries | Where-Object { $_.Name -like '*.nuspec' })
        if ($nuspecEntries.Count -ne 1) {
            throw "$($package.Name) must contain exactly one nuspec file."
        }

        [xml] $nuspec = Read-ZipEntry -Entry $nuspecEntries[0]
        $metadata = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']")
        $actualPackageId = $metadata.SelectSingleNode("*[local-name()='id']").InnerText
        $packageTypes = @($metadata.SelectNodes("*[local-name()='packageTypes']/*[local-name()='packageType']") |
            ForEach-Object { $_.GetAttribute('name') })
        $settingsEntries = @($archive.Entries | Where-Object { $_.Name -eq 'DotnetToolSettings.xml' })

        if ($packageTypes -contains 'DotnetTool') {
            if ($seenPointer -or $actualPackageId -ne $PackageId) {
                throw "Expected one pointer package with ID '$PackageId'."
            }

            $seenPointer = $true
            $expectedPath = 'tools/any/any/DotnetToolSettings.xml'
            $matchingSettingsEntries = @($settingsEntries | Where-Object FullName -eq $expectedPath)
            if ($settingsEntries.Count -ne 1 -or $matchingSettingsEntries.Count -ne 1) {
                throw "$($package.Name) must contain exactly one settings file at '$expectedPath'."
            }

            [xml] $settings = Read-ZipEntry -Entry $matchingSettingsEntries[0]
            Assert-CommandSettings -Settings $settings -PackageName $package.Name -Pointer
            continue
        }

        if ($packageTypes -notcontains 'DotnetToolRidPackage') {
            throw "$($package.Name) has an unexpected package type."
        }

        $runtime = $expectedRuntimes | Where-Object { $actualPackageId -eq "$PackageId.$_" }
        if (@($runtime).Count -ne 1 -or $seenRuntimes.ContainsKey($runtime)) {
            throw "$($package.Name) has an unexpected or duplicate runtime package ID '$actualPackageId'."
        }
        $seenRuntimes[$runtime] = $true

        if ($settingsEntries.Count -ne $expectedFrameworks.Count) {
            throw "$($package.Name) must contain one settings file per target framework."
        }

        $entryPoint = if ($runtime.StartsWith('win-')) { 'Expressif.Cli.exe' } else { 'Expressif.Cli' }
        foreach ($framework in $expectedFrameworks) {
            $expectedPath = "tools/$framework/$runtime/DotnetToolSettings.xml"
            $matchingSettingsEntries = @($settingsEntries | Where-Object FullName -eq $expectedPath)
            if ($matchingSettingsEntries.Count -ne 1) {
                throw "$($package.Name) must contain exactly one settings file at '$expectedPath'."
            }

            [xml] $settings = Read-ZipEntry -Entry $matchingSettingsEntries[0]
            Assert-CommandSettings -Settings $settings -PackageName $package.Name -ExpectedEntryPoint $entryPoint
        }
    }
    finally {
        $archive.Dispose()
    }
}

if (-not $seenPointer -or $seenRuntimes.Count -ne $expectedRuntimes.Count) {
    throw 'The CLI package set is incomplete.'
}

Write-Host "Validated $($packages.Count) CLI tool packages and their DotnetToolSettings.xml files."
