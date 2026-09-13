param(
    [Parameter(Mandatory = $true)]
    [string] $PackagePath
)

$ErrorActionPreference = 'Stop'

function Assert-Equal {
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Actual,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Expected,

        [Parameter(Mandatory = $true)]
        [string] $Description
    )

    if ($Actual -cne $Expected) {
        throw "Expected $Description to be '$Expected', but found '$Actual'."
    }
}

function Assert-SequenceEqual {
    param(
        [Parameter(Mandatory = $true)]
        [object[]] $Actual,

        [Parameter(Mandatory = $true)]
        [object[]] $Expected,

        [Parameter(Mandatory = $true)]
        [string] $Description
    )

    $actualJson = ConvertTo-Json @($Actual) -Compress
    $expectedJson = ConvertTo-Json @($Expected) -Compress
    Assert-Equal $actualJson $expectedJson $Description
}

function Read-ArchiveJson {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.Compression.ZipArchive] $Archive,

        [Parameter(Mandatory = $true)]
        [string] $EntryName
    )

    $entry = $Archive.GetEntry($EntryName)
    if ($null -eq $entry) {
        throw "Expected VSIX entry '$EntryName', but it was not found."
    }

    $reader = [System.IO.StreamReader]::new($entry.Open())
    try {
        return $reader.ReadToEnd() | ConvertFrom-Json
    }
    finally {
        $reader.Dispose()
    }
}

function Assert-ArchiveEntry {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.Compression.ZipArchive] $Archive,

        [Parameter(Mandatory = $true)]
        [string] $EntryName
    )

    if ($null -eq $Archive.GetEntry($EntryName)) {
        throw "Expected VSIX entry '$EntryName', but it was not found."
    }
}

$resolvedPackage = Resolve-Path -LiteralPath $PackagePath
$archive = [System.IO.Compression.ZipFile]::OpenRead($resolvedPackage.Path)
try {
    $package = Read-ArchiveJson $archive 'extension/package.json'
    $grammar = Read-ArchiveJson $archive 'extension/syntaxes/expressif.tmLanguage.json'
    $null = Read-ArchiveJson $archive 'extension/language-configuration.json'
    Assert-ArchiveEntry $archive 'extension/readme.md'

    Assert-Equal $package.publisher 'seddryck' 'publisher'
    Assert-Equal $package.name 'expressif-syntax-highlighting' 'extension name'
    Assert-Equal "$($package.publisher).$($package.name)" 'seddryck.expressif-syntax-highlighting' 'extension id'

    if ($package.PSObject.Properties.Name -contains 'main' -or
        $package.PSObject.Properties.Name -contains 'browser' -or
        $package.PSObject.Properties.Name -contains 'activationEvents') {
        throw 'The syntax-only extension must not declare a runtime or activation events.'
    }

    Assert-SequenceEqual @($package.contributes.PSObject.Properties.Name) @('languages', 'grammars') 'contribution types'
    Assert-Equal @($package.contributes.languages).Count 1 'language contribution count'
    Assert-Equal @($package.contributes.grammars).Count 1 'grammar contribution count'

    $language = @($package.contributes.languages)[0]
    Assert-Equal $language.id 'expressif' 'language id'
    Assert-SequenceEqual @($language.extensions) @('.expr', '.expressif') 'language extensions'
    Assert-Equal $language.configuration './language-configuration.json' 'language configuration path'

    $grammarContribution = @($package.contributes.grammars)[0]
    Assert-Equal $grammarContribution.language 'expressif' 'grammar language id'
    Assert-Equal $grammarContribution.scopeName 'source.expressif' 'grammar scope name'
    Assert-Equal $grammarContribution.path './syntaxes/expressif.tmLanguage.json' 'grammar path'
    Assert-Equal $grammar.scopeName 'source.expressif' 'generated grammar scope name'
}
finally {
    $archive.Dispose()
}

Write-Host "VS Code extension contract verified: $($resolvedPackage.Path)"
