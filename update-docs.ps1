param (
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
    $keywords=@()

    ForEach($member in $members) {
        $keywords += $member.Name
        $doc += "##### $($member.Name)`r`n"
        $doc += "###### Overview`r`n`r`n$($member.Summary)`r`n"
        if($member.Parameters.Length -gt 0) {
            $doc += "`r`n###### Parameter"
            if($member.Parameters.Length -gt 1) {
                $doc += "s"
            }
            $doc += "`r`n"
            foreach ($parameter in $member.Parameters) {
                $doc += "* $($parameter.Name)"
                if($parameter.Optional) {
                    $doc += " (optional) "
                }
                $doc += ": $($parameter.Summary)`r`n"
            }
        }
        $doc += "`r`n"
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
            if ($line.EndsWith("# AUTO-GENERATED KEYWORDS")) {
                $text += "keywords: [$($keywords -join ', ')] # AUTO-GENERATED KEYWORDS`r`n"
            } else {
                $text += $line + "`r`n"
            }
        }

        if ($line -eq "<!-- START AUTO-GENERATED -->"){
            $skip = $true
            $text += $doc
            Write-Host  "`t`tNew content inserted after line $i"
            $j = $i+1
        } 
    }
    Write-Host  $text
    $text | Out-File -FilePath $destinationFile  -NoNewline -Encoding ascii
    Write-Host  "`tNew content written"
}
Write-Host "New version of $destinationFile created in $($elapsed.TotalSeconds) seconds"