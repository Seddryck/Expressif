#requires -PSEdition Core

$destinationPath = ".\docs\_data"
$destinationFile = "predicate.json"


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

Write-Host "Generating JSON for predicates based on $assemblyPath\$directory\$dllfile"
$elapsed = Measure-Command -Expression {
    $locator = New-Object  Expressif.Predicates.Introspection.PredicateIntrospector
    $functions = $locator.Locate() | Sort-Object ListingPriority | Select-Object -Property Name, Aliases, Scope
    Write-Host  "`t$($functions.Count) predicates identified"
    $functions | ForEach-Object {Write-Host "`t`t$($_.Name)"}
    $functions | ConvertTo-Json | Out-File "$destinationPath\$destinationFile"
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
