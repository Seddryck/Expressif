param (
    [string] $page,
    [string] $class
)

function ParseTestFile {
    param( 
      [Parameter(Mandatory = $True)] 
      [String]$fileName,
      [String]$testName
    )
    $sourceFile = "Expressif.Testing/$fileName"
    [bool] $capture = $false
    [string] $leadingSpace = ""
    ForEach ($line in Get-Content -Path $sourceFile) {
        if ($capture -and $line.Trim() -eq ("}")) {
            $capture = $false
        }
        
        if ($capture -and $line.Trim() -ne ("{")) {
            if ($leadingSpace.Length -eq 0 -and $line.Length -gt 0) {
                $leadingSpace = $line.Substring(0,$line.IndexOf($line.TrimStart()[0]))
            }
            $textCaptured += $line.Replace($leadingSpace, "")
            $textCaptured += "`r`n"
        }
        
        if ($line.Trim() -eq ("public void $($testName)()")) {
               $capture = $true
        }
    }
    return $textCaptured
}

$docFile = ".\docs\_docs\$($page.ToLower())-$($class.ToLower()).md"

Write-Host "Creating new version of $docFile ..."
$elapsed = Measure-Command -Expression {
    ########### Update the sub-part of the docs file ##########
    Write-Host  "`tReplacing content in $docFile ..."
    $text = ""
    [bool] $skip = $false
    ForEach ($line in Get-Content -Path $docFile) {
        if($line.Trim() -eq ("<!-- END INCLUDE -->")) {
            $skip = $false
        }

        if (-not $skip) {
            $text += $line + "`r`n"
        }

        if ($line.Trim().StartsWith("<!-- START INCLUDE ")){
            $skip = $true
            $line = $line.Trim()
            $filePath = $line.Substring(19, $line.Length - 23).Trim().Trim("`"")
            $fileName = $filePath.Substring(0, $filePath.IndexOf('/'))
            $testName = $filePath.Replace($fileName, "").Trim('/')
            $captured = "``````csharp`r`n"
            $captured += ParseTestFile $fileName $testName
            $captured += "```````r`n"
            Write-Host $captured
            $text += $captured
        } 
    }
    $text | Out-File -FilePath $docFile  -NoNewline -Encoding ascii
}
Write-Host "New version of $docFile created in $($elapsed.TotalSeconds) seconds"

