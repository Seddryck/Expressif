param ()

$destinationFile = ".\docs\_data\navigation_boxes.yml"

########### Create a markdown table ##########
Write-Host "Creating new version of $destinationFile based on $sourceFile ..."
$elapsed = Measure-Command -Expression {
    $classes = @("function", "predicate")
    $counts = @{}
    foreach($class in $classes) {
        $sourceFile = ".\docs\_data\$($class.ToLower()).json"
        $count = (Get-Content -Path $sourceFile | ConvertFrom-Json | Measure).Count
        Write-Host "`t$count $($class.ToLower())s found"
        $counts.Add($class, $count)
    }
     
    ########### Update the sub-part of the docs file ##########

    Write-Host  "`tReplacing content in $destinationFile ..."
    $text = ""
    [string] $template = ""
    foreach ($line in Get-Content -Path $destinationFile) {
        if ($template -eq "") {
            $text += $line
        } else {
            $text += $template
        }   

        $template=""
        if($line.Trim().StartsWith("#Template#")) {
            $template = $line.Trim().Replace("#Template#", "")
            foreach($count in $counts.keys) {
                $template = $template.Replace("<$count>", "$($counts[$count])")
            }
        }
        $text += "`r`n"
    }
    $text | Out-File -FilePath $destinationFile -NoNewline -Encoding ascii
}
Write-Host "New version of $destinationFile created in $($elapsed.TotalSeconds) seconds"