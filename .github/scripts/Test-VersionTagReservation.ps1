# Exercise the actual workflow reservation block without publishing tags.
$ErrorActionPreference = 'Stop'
$workflow = Get-Content (Join-Path $PSScriptRoot '../workflows/ci.yml') -Raw
$block = [regex]::Match($workflow, '(?s)          function Get-RemoteTagCommit.*?(?=\r?\n\r?\n  publish-nuget:)').Value
if (-not $block) { throw 'Could not locate version tag reservation block.' }
$reservation = [scriptblock]::Create(($block -replace '(?m)^          ', ''))
$env:EXPECTED_SHA = 'expected'
$currentVersion = '2.0.1'
$currentConformanceVersion = '2026.9.1'

function git {
    $global:LASTEXITCODE = 0
    switch ($args[0]) {
        'ls-remote' {
            $name = $args[2] -replace '^refs/tags/', ''
            if ($script:remote.ContainsKey($name)) {
                "$($script:remote[$name]) refs/tags/$name"
                if ($script:annotated) { "expected refs/tags/$name^{}" }
            }
        }
        'merge-base' { $global:LASTEXITCODE = $script:ancestorExit }
        'diff' { $global:LASTEXITCODE = $script:diffExit }
        'tag' {
            $script:local[$args[1]] = $args[2]
            $global:LASTEXITCODE = $script:localExit
        }
        'push' {
            $name = $args[2] -replace '^refs/tags/', ''
            $script:pushes.Add($name)
            if ($script:race) {
                $script:remote[$name] = $script:race
                $global:LASTEXITCODE = 1
            }
            elseif ($script:pushExit) { $global:LASTEXITCODE = $script:pushExit }
            else { $script:remote[$name] = $script:local[$name] }
        }
        default { throw "Unexpected git command: $args" }
    }
}

$cases = @(
    @{ Name = 'Create both tags'; Pushes = 2 },
    @{ Name = 'Existing Expressif tag still creates conformance'; Remote = @{ 'v2.0.1' = 'expected' }; Pushes = 1 },
    @{ Name = 'Existing conformance tag still creates Expressif'; Remote = @{ 'conformance-2026.9.1' = 'expected' }; Pushes = 1 },
    @{ Name = 'Both existing tags'; Remote = @{ 'v2.0.1' = 'expected'; 'conformance-2026.9.1' = 'expected' }; Pushes = 0 },
    @{ Name = 'Annotated tags use peeled commit'; Remote = @{ 'v2.0.1' = 'tag-object'; 'conformance-2026.9.1' = 'tag-object' }; Annotated = $true; Pushes = 0 },
    @{ Name = 'Wrong Expressif SHA'; Remote = @{ 'v2.0.1' = 'wrong' }; Error = 'points to'; Pushes = 0 },
    @{ Name = 'Unchanged conformance retains earlier tag'; Remote = @{ 'conformance-2026.9.1' = 'earlier' }; Pushes = 1 },
    @{ Name = 'Changed conformance rejects earlier tag before any push'; Remote = @{ 'conformance-2026.9.1' = 'earlier' }; Diff = 1; Error = 'files changed'; Pushes = 0 },
    @{ Name = 'Unrelated conformance tag'; Remote = @{ 'conformance-2026.9.1' = 'wrong' }; Ancestor = 1; Error = 'not an ancestor'; Pushes = 0 },
    @{ Name = 'Concurrent creation at expected SHA'; Race = 'expected'; Pushes = 2 },
    @{ Name = 'Concurrent creation at wrong SHA'; Race = 'wrong'; Error = 'Failed to reserve'; Pushes = 1 },
    @{ Name = 'Push fails without remote tag'; PushExit = 1; Error = 'Failed to reserve'; Pushes = 1 },
    @{ Name = 'Local tag creation fails'; LocalExit = 1; Error = 'Failed to create local'; Pushes = 0 }
)

foreach ($case in $cases) {
    $script:remote = if ($case.Remote) { $case.Remote.Clone() } else { @{} }
    $script:local = @{}
    $script:pushes = [Collections.Generic.List[string]]::new()
    $script:annotated = $case.Annotated
    $script:ancestorExit = [int]$case.Ancestor
    $script:diffExit = [int]$case.Diff
    $script:localExit = [int]$case.LocalExit
    $script:pushExit = [int]$case.PushExit
    $script:race = $case.Race
    $tags = @("v$currentVersion", "conformance-$currentConformanceVersion")
    $failure = $null
    try { & $reservation } catch { $failure = $_.Exception.Message }
    if ($case.Error) {
        if ($failure -notlike "*$($case.Error)*") { throw "$($case.Name): unexpected failure '$failure'." }
    }
    elseif ($failure) { throw "$($case.Name): $failure" }
    if ($script:pushes.Count -ne $case.Pushes) { throw "$($case.Name): unexpected pushes $($script:pushes -join ', ')." }
    Write-Host "PASS: $($case.Name)"
}
