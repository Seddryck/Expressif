# Offline regression test: mock only GitHub downloads, run the real installation script.
$ErrorActionPreference = 'Stop'
$testRoot = Join-Path ([IO.Path]::GetTempPath()) "expressif-install-test-$([guid]::NewGuid())"
New-Item -ItemType Directory -Path $testRoot | Out-Null
try {
    $source = Join-Path $testRoot 'source'
    New-Item -ItemType Directory -Path $source | Out-Null
    Set-Content (Join-Path $source 'expressif.exe') 'new executable'
    Set-Content (Join-Path $source 'expressif.config.json') '{"output-style":"compact","indent":2}'
    $archive = Join-Path $testRoot 'expressif-test-net10.0-win-x64.zip'
    Compress-Archive -Path (Join-Path $source '*') -DestinationPath $archive
    function gh {
        if ($args[0] -eq 'api') {
            '{"tag_name":"test"}'
        }
        elseif ($args[0] -eq 'release' -and $args[1] -eq 'download') {
            $destination = $args[[Array]::IndexOf($args, '--dir') + 1]
            Copy-Item -LiteralPath $archive -Destination $destination
        }
        else {
            throw "Unexpected GitHub command: $args"
        }
        $global:LASTEXITCODE = 0
    }

    $destination = Join-Path $testRoot 'installed'
    & "$PSScriptRoot/install-local.ps1" -InstallDirectory $destination
    $config = Join-Path $destination 'expressif.config.json'
    if ((Get-Content $config -Raw).Trim() -ne '{"output-style":"compact","indent":2}') {
        throw 'Fresh install did not include packaged defaults.'
    }
    [IO.File]::WriteAllText($config, '{"repl":{"output-style":"pretty"},"indent":"tab"}')
    $before = [IO.File]::ReadAllBytes($config)
    Set-Content (Join-Path $destination 'expressif.exe') 'old executable'
    & "$PSScriptRoot/install-local.ps1" -InstallDirectory $destination -Force
    if ([Convert]::ToBase64String($before) -ne [Convert]::ToBase64String([IO.File]::ReadAllBytes($config))) {
        throw 'Forced install changed existing configuration.'
    }
    if ((Get-Content (Join-Path $destination 'expressif.exe') -Raw).Trim() -ne 'new executable') {
        throw 'Forced install did not update executable.'
    }
    Remove-Item -LiteralPath $config
    & "$PSScriptRoot/install-local.ps1" -InstallDirectory $destination -Force
    if (-not (Test-Path -LiteralPath $config)) {
        throw 'Forced install did not supply missing configuration.'
    }
    Write-Host 'Passed: fresh install, config preservation, executable update, and missing config.'
}
finally {
    $resolved = [IO.Path]::GetFullPath($testRoot)
    $temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if (-not $resolved.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Test cleanup path is outside temporary directory.'
    }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
