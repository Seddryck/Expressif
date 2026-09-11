param(
    [string] $InputFolder = "../docs/_data",
    [string] $OutputFolder = "./obj/verification"
)
$ErrorActionPreference = 'Stop'
$OutputFolder = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputFolder)
& "$PSScriptRoot/New-tmLanguage.ps1" -InputFolder $InputFolder -OutputPath "$OutputFolder/expressif.tmLanguage.json"
& "$PSScriptRoot/New-RougeLexer.ps1" -InputFolder $InputFolder -OutputPath "$OutputFolder/expressif.rb"
& "$PSScriptRoot/New-NotepadPlusPlusLanguage.ps1" -InputFolder $InputFolder -OutputPath "$OutputFolder/expressif.xml"
[xml] $udl = Get-Content "$OutputFolder/expressif.xml" -Raw
$operators = ($udl.NotepadPlus.UserLang.KeywordLists.Keywords | Where-Object name -eq 'Operators1').'#text' -split ' '
if ('~' -notin $operators -or '-' -in $operators) { throw 'UDL must recognize tilde adjacency without splitting hyphenated names.' }
$keywords = $udl.NotepadPlus.UserLang.KeywordLists.Keywords
foreach ($name in @('subtract', 'greater-than', 'starts-with')) {
    if (-not ($keywords | Where-Object { $_.name -in @('Keywords1', 'Keywords2') -and $name -in ($_.'#text' -split ' ') })) {
        throw "UDL callable missing: $name"
    }
}
$delimiters = ($keywords | Where-Object name -eq 'Delimiters').'#text'
if (-not $delimiters.Contains('00" 01\ 02"') -or -not $delimiters.Contains('03` 04\ 05`')) { throw 'UDL quoted delimiters must preserve escaping.' }
foreach ($style in $udl.NotepadPlus.UserLang.Styles.WordsStyle | Where-Object { $_.name -in @('DELIMITERS1', 'DELIMITERS2') }) {
    if ($style.nesting -ne '0') { throw 'UDL quoted text must not nest operator highlighting.' }
}
Write-Host 'Notepad++ UDL generation checks passed'
node "$PSScriptRoot/tests/textmate.cjs" "$OutputFolder/expressif.tmLanguage.json"
if ($LASTEXITCODE) { throw 'TextMate tokenization failed.' }
ruby -c "$OutputFolder/expressif.rb"
if ($LASTEXITCODE) { throw 'Generated Ruby syntax is invalid.' }
ruby "$PSScriptRoot/tests/rouge.rb" "$OutputFolder/expressif.rb"
if ($LASTEXITCODE) { throw 'Rouge tokenization failed.' }
