﻿$classes = @("Function", "Predicate")
$destinationFile = ".\docs\_docs\library-index.md"

Write-Host "Creating new version of $destinationFile ..."
$elapsed = Measure-Command -Expression {
    foreach ($class in $classes) {
        $sourceFile = ".\docs\_data\$($class.ToLower()).json"  
        $top += "[$($class)s](#$($class.ToLower())s) - "  

        ########### Create an index ##########
        Write-Host "`tUsing $sourceFile as reference for $($class.ToLower())s ..."
        $members = Get-Content -Path $sourceFile | ConvertFrom-Json 
        $members = $members | Sort-Object Name
        Write-Host  "`t$($members.Count) $($class.ToLower())s found"
        $doc += "## $($class)s`r`n`r`n"

        ForEach($member in $members) {
            $doc += "* [$($member.Name)](/docs/$($member.Scope.ToLower())-$($class.ToLower())s/#$($member.Name))`r`n"
        }
        $doc += "`r`n`r`n"
    }
    $top = $top.Remove($top.Length-2, 2)
}

########### Update the sub-part of the docs file ##########
Write-Host  "`tReplacing content in $destinationFile ..."
$text = ""
[bool] $skip = $false
foreach ($line in Get-Content -Path $destinationFile) {
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
        $text += "$top`r`n`r`n`r`n"
        $text += $doc
        Write-Host  "`t`tNew content inserted after line $i"
        $j = $i+1
    } 
}


$text | Out-File -FilePath $destinationFile  -NoNewline -Encoding ascii
Write-Host  "`tNew content written"
Write-Host "New version of $destinationFile created in $($elapsed.TotalSeconds) seconds"