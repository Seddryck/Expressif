#requires -PSEdition Core
param (
    [string] $class
)

$destinationPath = ".\docs\_data"
$destinationFile = "$($class.ToLower()).json"


########### Check if it's useful to make changes to doc or readme #############
Set-Location ./

$hash = 0
If(Test-Path -LiteralPath $destinationPath\$destinationFile -PathType leaf) {
    $hash = Get-FileHash $destinationPath\$destinationFile
    Write-Debug "Previous hash for $destinationPath\$destinationFile is $($hash.Hash)"
}

########### Generate JSON file #############

$assemblyPath = "Expressif\bin"
Set-Location $assemblyPath
$dllfile = "net6.0\Expressif.dll"

If ((-not (Test-Path -Path "Release\$dllfile")) -or ("Release\$dllfile".CreationTime -lt "Debug\$dllfile".CreationTime)) {
    $directory = "Debug"    
} else {
    $directory = "Release"   
}
Add-Type -Path "$directory\$dllfile"

Set-Location "..\..\"

Write-Host "Generating JSON for $($class.ToLower())s based on $assemblyPath\$directory\$dllfile"
$elapsed = Measure-Command -Expression {
    $TextInfo = (Get-Culture).TextInfo
    $locator = New-Object -TypeName "Expressif.$($TextInfo.ToTitleCase($class))s.Introspection.$($TextInfo.ToTitleCase($class))Introspector"
    $functions = $locator.Locate() | Sort-Object ListingPriority | Select-Object -Property Name, Aliases, Scope, Summary, Parameters
    Write-Host  "`t$($functions.Count) $($class.ToLower()) identified"
    $functions | ForEach-Object {Write-Host "`t`t$($_.Name)"}
    $functions | ConvertTo-Json -depth 4 | Out-File "$destinationPath\$destinationFile"
}
Write-Host  "File created at $destinationPath\$destinationFile in $($elapsed.TotalSeconds) seconds"


########### Check if it's useful to report a change #############

If ($hash.Hash -eq (Get-FileHash $destinationPath\$destinationFile).Hash) {
    Write-Host "No change detected in the list of predicates."
    Exit 0
} else {
    Write-Host "Changes detected in the list of predicates."
    Exit 1
}
