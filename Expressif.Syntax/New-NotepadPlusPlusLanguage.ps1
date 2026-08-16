param(
    [Parameter(Mandatory = $true)]
    [string] $InputFolder,

    [Parameter(Mandatory = $false)]
    [string] $OutputPath = ".\bin\expressif.xml"
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\Get-SyntaxModel.ps1"

$model = Get-SyntaxModel -InputFolder $InputFolder
$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$settings = [System.Xml.XmlWriterSettings]::new()
$settings.Encoding = [System.Text.UTF8Encoding]::new($false)
$settings.Indent = $true
$settings.NewLineChars = "`n"
$settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace

$writer = [System.Xml.XmlWriter]::Create($OutputPath, $settings)
try {
    $writer.WriteStartDocument()
    $writer.WriteStartElement('NotepadPlus')
    $writer.WriteStartElement('UserLang')
    $writer.WriteAttributeString('name', 'Expressif')
    $writer.WriteAttributeString('ext', 'expr expressif')
    $writer.WriteAttributeString('udlVersion', '2.1')

    $writer.WriteStartElement('Settings')
    $writer.WriteStartElement('Global')
    $writer.WriteAttributeString('caseIgnored', 'yes')
    $writer.WriteAttributeString('allowFoldOfComments', 'no')
    $writer.WriteAttributeString('foldCompact', 'no')
    $writer.WriteAttributeString('forcePureLC', '0')
    $writer.WriteAttributeString('decimalSeparator', '0')
    $writer.WriteEndElement()
    $writer.WriteStartElement('Prefix')
    1..8 | ForEach-Object { $writer.WriteAttributeString("Keywords$_", 'no') }
    $writer.WriteEndElement()
    $writer.WriteEndElement()

    $writer.WriteStartElement('KeywordLists')
    $keywordLists = [ordered]@{
        Comments = ''
        NumbersPrefix1 = ''
        NumbersPrefix2 = ''
        NumbersExtras1 = ''
        NumbersExtras2 = ''
        NumbersSuffix1 = ''
        NumbersSuffix2 = ''
        NumbersRange = ''
        Operators1 = ($model.operators -join ' ')
        Operators2 = ''
        FoldersInCode1Open = '{'
        FoldersInCode1Middle = ''
        FoldersInCode1Close = '}'
        FoldersInCode2Open = ''
        FoldersInCode2Middle = ''
        FoldersInCode2Close = ''
        FoldersInCommentOpen = ''
        FoldersInCommentMiddle = ''
        FoldersInCommentClose = ''
        Keywords1 = (($model.functions | ForEach-Object { $_.name }) -join ' ')
        Keywords2 = (($model.predicates | ForEach-Object { $_.name }) -join ' ')
        Keywords3 = (($model.accumulators | ForEach-Object { $_.name }) -join ' ')
        Keywords4 = ($model.constants -join ' ')
        Keywords5 = ''
        Keywords6 = ''
        Keywords7 = ''
        Keywords8 = ''
        Delimiters = '00" 01\\ 02" 03` 04 05` 06 07 08 09 10 11 12 13 14 15 16 17 18 19 20 21 22 23'
    }
    foreach ($entry in $keywordLists.GetEnumerator()) {
        $writer.WriteStartElement('Keywords')
        $writer.WriteAttributeString('name', $entry.Key)
        $writer.WriteString($entry.Value)
        $writer.WriteEndElement()
    }
    $writer.WriteEndElement()

    $writer.WriteStartElement('Styles')
    $styles = @(
        @{ Name = 'DEFAULT'; StyleId = 0; Foreground = '000000' },
        @{ Name = 'COMMENTS'; StyleId = 1; Foreground = '008000' },
        @{ Name = 'LINE COMMENTS'; StyleId = 2; Foreground = '008000' },
        @{ Name = 'NUMBERS'; StyleId = 3; Foreground = 'FF8000' },
        @{ Name = 'OPERATORS'; StyleId = 4; Foreground = '800080' },
        @{ Name = 'KEYWORDS1'; StyleId = 5; Foreground = '0000FF' },
        @{ Name = 'KEYWORDS2'; StyleId = 6; Foreground = '8000FF' },
        @{ Name = 'KEYWORDS3'; StyleId = 7; Foreground = '0080C0' },
        @{ Name = 'KEYWORDS4'; StyleId = 8; Foreground = 'A31515' },
        @{ Name = 'KEYWORDS5'; StyleId = 9; Foreground = '000000' },
        @{ Name = 'KEYWORDS6'; StyleId = 10; Foreground = '000000' },
        @{ Name = 'KEYWORDS7'; StyleId = 11; Foreground = '000000' },
        @{ Name = 'KEYWORDS8'; StyleId = 12; Foreground = '000000' },
        @{ Name = 'FOLDER IN CODE1'; StyleId = 13; Foreground = '800080' },
        @{ Name = 'FOLDER IN CODE2'; StyleId = 14; Foreground = '000000' },
        @{ Name = 'FOLDER IN COMMENT'; StyleId = 15; Foreground = '000000' },
        @{ Name = 'DELIMITERS1'; StyleId = 16; Foreground = 'A31515' },
        @{ Name = 'DELIMITERS2'; StyleId = 17; Foreground = 'A31515' },
        @{ Name = 'DELIMITERS3'; StyleId = 18; Foreground = '000000' },
        @{ Name = 'DELIMITERS4'; StyleId = 19; Foreground = '000000' },
        @{ Name = 'DELIMITERS5'; StyleId = 20; Foreground = '000000' },
        @{ Name = 'DELIMITERS6'; StyleId = 21; Foreground = '000000' },
        @{ Name = 'DELIMITERS7'; StyleId = 22; Foreground = '000000' },
        @{ Name = 'DELIMITERS8'; StyleId = 23; Foreground = '000000' }
    )
    foreach ($style in $styles) {
        $writer.WriteStartElement('WordsStyle')
        $writer.WriteAttributeString('name', $style.Name)
        $writer.WriteAttributeString('fgColor', $style.Foreground)
        $writer.WriteAttributeString('bgColor', 'FFFFFF')
        $writer.WriteAttributeString('fontName', '')
        $writer.WriteAttributeString('fontStyle', '0')
        $writer.WriteAttributeString('nesting', '0')
        $writer.WriteAttributeString('styleID', [string]$style.StyleId)
        $writer.WriteEndElement()
    }
    $writer.WriteEndElement()
    $writer.WriteEndElement()
    $writer.WriteEndElement()
    $writer.WriteEndDocument()
}
finally {
    $writer.Dispose()
}

Write-Host "Generated: $OutputPath"
