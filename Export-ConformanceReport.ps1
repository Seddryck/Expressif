[CmdletBinding(DefaultParameterSetName = "NUnit")]
param(
    [Parameter(Mandatory)]
    [Alias("ConformancePath")]
    [string] $Path,

    [Alias("Excludes")]
    [string[]] $Exclude = @("bin/**", "/*.yaml", "/*.yml"),

    [Parameter(Mandatory)]
    [Alias("XmlPath")]
    [string] $ReportPath,

    [Parameter(Mandatory)]
    [string] $OutputYamlPath,

    [Parameter(Mandatory)]
    [string] $Platform,

    [Parameter(Mandatory, ParameterSetName = "NUnit")]
    [switch] $NUnit,

    [Parameter(Mandatory, ParameterSetName = "JUnit")]
    [switch] $JUnit,

    [string] $PlatformVersion = "$env:GitVersion_SemVer",

    [string] $ConformanceVersion = "$env:GitVersion_Conformance_SemVer"
)

Import-Module (Join-Path $PSScriptRoot "ConformanceReport.psm1") -Force

$parameters = @{}

foreach ($key in $PSBoundParameters.Keys) {
    $parameters[$key] = $PSBoundParameters[$key]
}

if ($parameters.ContainsKey("Path")) {
    $parameters["ConformancePath"] = $parameters["Path"]
    $parameters.Remove("Path") | Out-Null
}

Export-ConformanceReport @parameters