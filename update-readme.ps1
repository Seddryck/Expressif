param ([string] $class)

Write-Host($class)
Write-Host($class.ToLower())
Write-Host("$($class.ToLower())")

$sourceFile = ".\Docs\_data\$($class.ToLower()).json"
$destinationFile = ".\README.md"

########### Create a markdown table ##########
Write-Host "Creating new version of $destinationFile based on $sourceFile ..."
$elapsed = Measure-Command -Expression {
    $members = Get-Content -Path $sourceFile | ConvertFrom-Json | Sort-Object Scope, Name
    Write-Host  "`t$($members.Count) $($class.ToLower())s read"
    $items = @()
    $columns = @{}

    ForEach($member in $members) {
        $member.Aliases = $member.Aliases -Join ", "
        $items += $member

        $member.PSObject.Properties | %{
            if(-not $columns.ContainsKey($_.Name) -or $columns[$_.Name] -lt $_.Value.ToString().Length) {
                $columns[$_.Name] = $_.Value.ToString().Length
            }
        }
        Write-Host "`t`t$($member.Name)"
    }

    ForEach($key in $($columns.Keys)) {
        $columns[$key] = [Math]::Max($columns[$key], $key.Length)
    }

    $keys = "Scope", "Name", "Aliases"

    $table=""
    $header = @()
    ForEach($key in $keys) {
        $header += ('{0,-' + $columns[$key] + '}') -f ($key -creplace '([A-Z])', ' $1').Trim()
    }
    $table += '|' + ($header -join ' | ') + "|`r`n"

    $separator = @()
    ForEach($key in $keys) {
        $separator += '-' * $columns[$key]
    }
    $table += '|' + ($separator -join ' | ') + "|`r`n"

    ForEach($item in $items) {
        $values = @()
        ForEach($key in $keys) {
            $values += ('{0,-' + $columns[$key] + '}') -f $item.($key)
        }
        $table += '|' + ($values -join ' | ') + "|`r`n"
    }
    Write-Host  "`tCreated markdown table with $($table.Split("`r`n").GetUpperBound(0)) lines and a width of $($table.Split("`r`n")[0].Length) chars"

    ########### Update the sub-part of the readme ##########

    Write-Host  "`tReplacing content in $destinationFile ..."
    $text = ""
    [bool] $skip = $false
    ForEach ($line in Get-Content -Path $destinationFile) {
        $i+=1
        if($line -eq "<!-- END $($class.ToUpper()) TABLE -->") {
            $skip = $false
            Write-Host  "`t`tPrevious content skipped between lines $j and $i"
        }

        if (-not $skip) {
            $text += $line + "`r`n"
        }

        if ($line -eq "<!-- START $($class.ToUpper()) TABLE -->"){
            $skip = $true
            $text += $table 
            Write-Host  "`t`tNew content inserted after line $i"
            $j = $i+1
        } 
    }
    $text | Out-File -Path $destinationFile
    Write-Host  "`tNew content written"
}
Write-Host "New version of $destinationFile created in $($elapsed.TotalSeconds) seconds"