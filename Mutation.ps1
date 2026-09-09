# Run from any directory. The default pilot covers arithmetic conformance;
# -AddOnly restricts both the mutations and the tests to Add.
[CmdletBinding()]
param([switch] $AddOnly)

$ErrorActionPreference = 'Stop'
$testDirectory = Join-Path $PSScriptRoot 'Expressif.Testing'
$configPath = Join-Path $testDirectory 'stryker-config.json'
$temporaryConfig = $null
$previousMutation = $env:ExpressifMutation

try {
    $env:ExpressifMutation = 'true'
    if ($AddOnly) {
        $source = Get-Content (Join-Path $PSScriptRoot 'Expressif/Functions/Numeric/ArithmeticFunctions.cs') -Raw
        $add = [regex]::Match($source, '(?ms)^public class Add : BaseNumericArithmetic\s*\{.*?^\}')
        if (-not $add.Success) {
            throw 'Cannot locate the Add class. Update the mutation scope before running.'
        }

        $config = Get-Content $configPath -Raw | ConvertFrom-Json
        $config.'stryker-config'.mutate = @(
            '**/Functions/Numeric/ArithmeticFunctions.cs{' + $add.Index + '..' + ($add.Index + $add.Length) + '}'
        )
        $config.'stryker-config'.'test-case-filter' = 'TestCategory=conformance&FullyQualifiedName~Expressif.Testing.Functions.Numeric.ArithmeticFunctionsTest.Add_'
        $temporaryConfig = Join-Path ([System.IO.Path]::GetTempPath()) ('expressif-mutation-' + [guid]::NewGuid() + '.json')
        $config | ConvertTo-Json -Depth 10 | Set-Content $temporaryConfig
        $configPath = $temporaryConfig
    }

    Push-Location $testDirectory
    try {
        & dotnet stryker --config-file $configPath
        if ($LASTEXITCODE -ne 0) {
            throw "Mutation testing failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}
finally {
    $env:ExpressifMutation = $previousMutation
    if ($temporaryConfig -and (Test-Path -LiteralPath $temporaryConfig)) {
        Remove-Item -LiteralPath $temporaryConfig
    }
}
