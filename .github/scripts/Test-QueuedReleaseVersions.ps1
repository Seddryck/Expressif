# A later release on origin/main must not influence an earlier queued commit.
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '../../Conformance.ps1')

function Update-ConformanceTags {}

function git {
    $global:LASTEXITCODE = 0
    switch ($args[0]) {
        'rev-parse' {
            if ($args -contains '--show-toplevel') { (Get-Location).Path }
            elseif ($args -contains '--verify') {
                if ($args[-1] -eq 'missing^{commit}') { $global:LASTEXITCODE = 1 }
            }
            else { 'queued-commit' }
        }
        'diff-tree' {}
        'tag' {
            if ($args[1] -ne '--merged') { throw "Unexpected tag query: $args" }
            'conformance-2026.9.1'
            if ($args[2] -eq 'origin/main') { 'conformance-2026.9.2' }
        }
        'diff' {}
        default { throw "Unexpected git command: $args" }
    }
}

Push-Location (Join-Path $PSScriptRoot '../..')
try {
    $originalCommit = $env:APPVEYOR_REPO_COMMIT
    $originalExpectedSha = $env:EXPECTED_SHA
    $env:APPVEYOR_REPO_COMMIT = $null
    $workflow = Get-Content (Join-Path $PSScriptRoot '../workflows/ci.yml') -Raw
    $block = [regex]::Match($workflow, '(?s)          \$headSha = git rev-parse HEAD.*?(?=\r?\n\r?\n          \$versionJson)').Value
    if (-not $block) { throw 'Could not locate release checkout guard.' }
    $guard = [scriptblock]::Create(($block -replace '(?m)^          ', ''))
    $env:EXPECTED_SHA = 'queued-commit'
    & $guard
    Write-Host 'PASS: Checkout guard accepts the queued commit without querying main'
    $env:EXPECTED_SHA = 'different-commit'
    $failure = $null
    try { & $guard } catch { $failure = $_.Exception.Message }
    if ($failure -notlike '*Release checkout does not identify*') {
        throw "Wrong checkout was not rejected: $failure"
    }
    Write-Host 'PASS: Checkout guard rejects artifacts from the wrong commit'

    $version = Get-ConformanceVersion -Refresh -NoEnv
    if ($version -ne '2026.9.2') { throw "Default main reference returned $version." }
    Write-Host 'PASS: Default callers retain main tag selection'

    # Explicit tag references must also bypass a value cached for another ref.
    $version = Get-ConformanceVersion -NoEnv -TagReference HEAD
    if ($version -ne '2026.9.1') { throw "Queued commit used later release $version." }
    Write-Host 'PASS: Queued commit ignores later main tags and cached versions'

    $version = Get-ConformanceVersion -NoEnv
    if ($version -ne '2026.9.2') { throw "Explicit reference changed the default cache to $version." }
    Write-Host 'PASS: Explicit reference leaves the default cache intact'

    $failure = $null
    try { Get-ConformanceVersion -Refresh -NoEnv -TagReference missing }
    catch { $failure = $_.Exception.Message }
    if ($failure -notlike '*Cannot resolve conformance tag reference*') {
        throw "Invalid reference was not rejected: $failure"
    }
    Write-Host 'PASS: Invalid explicit reference is rejected'
    # The expected mocked Git failure must not fail the PowerShell CI step.
    $global:LASTEXITCODE = 0
}
finally {
    $env:APPVEYOR_REPO_COMMIT = $originalCommit
    $env:EXPECTED_SHA = $originalExpectedSha
    Pop-Location
}
