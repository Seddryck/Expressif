﻿param (
    [string] $class,
    [string] $scope
)

$sourceFile = ".\docs\_data\$($class.ToLower()).json"
$destinationFile = ".\docs\_docs\$($scope.ToLower())-$($class.ToLower())s.md"

########### Create a markdown table ##########
Write-Host "Creating new version of $destinationFile based on $sourceFile ..."
$elapsed = Measure-Command -Expression {
    $members = Get-Content -Path $sourceFile | ConvertFrom-Json 
    $members = $members | where {$_.Scope -ieq $scope} | Sort-Object Name
    Write-Host  "`t$($members.Count) $($class.ToLower())s found within scope $scope"
    $text=""

    ForEach($member in $members) {
        $doc += "### $($member.Name)`r`n`r`n$($member.Summary)`r`n`r`n"
    }

    Write-Host $doc
    
    ########### Update the sub-part of the docs file ##########

    Write-Host  "`tReplacing content in $destinationFile ..."
    $text = ""
    [bool] $skip = $false
    ForEach ($line in Get-Content -Path $destinationFile) {
        $i+=1
        if($line -eq "<!-- END AUTO-GENERATED -->") {
            $skip = $false
            Write-Host  "`t`tPrevious content skipped between lines $j and $i"
        }

        if (-not $skip) {
            $text += $line + "`r`n"
        }

        if ($line -eq "<!-- START AUTO-GENERATED -->"){
            $skip = $true
            $text += $doc
            Write-Host  "`t`tNew content inserted after line $i"
            $j = $i+1
        } 
    }
    Write-Host  $text
    $text | Out-File -FilePath $destinationFile -Encoding utf8 -NoNewline
    Write-Host  "`tNew content written"
}
Write-Host "New version of $destinationFile created in $($elapsed.TotalSeconds) seconds"