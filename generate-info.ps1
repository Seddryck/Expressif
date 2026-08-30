#requires -PSEdition Core
param (
    [Parameter(Mandatory)]
    [ValidateSet("function", "predicate", "accumulator")]
    [string] $class,

    [Parameter()]
    [string[]] $name
)

$destinationPath = ".\docs\_data"
$destinationFile = "$($class.ToLower()).json"


########### Check if it's useful to make changes to doc or readme #############
Set-Location -Path $PSScriptRoot

$hash = 0
If(Test-Path -LiteralPath $destinationPath\$destinationFile -PathType leaf) {
    $hash = Get-FileHash $destinationPath\$destinationFile
    Write-Debug "Previous hash for $destinationPath\$destinationFile is $($hash.Hash)"
}

########### Generate JSON file #############

$configuration = "Debug"
$framework = "net8.0"
$projectPath = Join-Path $PSScriptRoot "Expressif\Expressif.csproj"
$assemblyPath = Join-Path $PSScriptRoot "Expressif\bin\$configuration\$framework\Expressif.dll"

Write-Host "Building Expressif before generating metadata"
dotnet build $projectPath --configuration $configuration --framework $framework --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Expressif build failed; metadata was not generated."
}

if ($name.Count -gt 0) {
    Write-Host "Generating JSON for $($class.ToLower())s '$($name -join "', '")' based on $assemblyPath"
} else {
    Write-Host "Generating JSON for all $($class.ToLower())s based on $assemblyPath"
}

$job = Start-Job -ScriptBlock { param($fullDllPath, $class, $names, $destination)
    function Update-JsonArrayEntries {
        param(
            [Parameter(Mandatory)]
            [string] $Path,

            [Parameter(Mandatory)]
            [object[]] $Entries
        )

        $content = Get-Content -LiteralPath $Path -Raw
        $spans = @{}
        $depth = 0
        $start = -1
        $inString = $false
        $escaped = $false

        for ($index = 0; $index -lt $content.Length; $index++) {
            $character = $content[$index]
            if ($inString) {
                if ($escaped) {
                    $escaped = $false
                } elseif ($character -eq '\') {
                    $escaped = $true
                } elseif ($character -eq '"') {
                    $inString = $false
                }
                continue
            }

            if ($character -eq '"') {
                $inString = $true
            } elseif ($character -eq '{') {
                if ($depth -eq 0) {
                    $start = $index
                }
                $depth++
            } elseif ($character -eq '}') {
                $depth--
                if ($depth -eq 0) {
                    $entryText = $content.Substring($start, $index - $start + 1)
                    $entry = $entryText | ConvertFrom-Json
                    $spans[$entry.Name] = @{ Start = $start; Length = $index - $start + 1 }
                }
            }
        }

        $replacements = @()
        $additions = @()
        foreach ($entry in $Entries) {
            $entryText = ($entry | ConvertTo-Json -Depth 4).Replace("`n", "`n  ")
            if ($spans.ContainsKey($entry.Name)) {
                $span = $spans[$entry.Name]
                $replacements += @{ Start = $span.Start; Length = $span.Length; Text = $entryText }
            } else {
                $additions += $entryText
            }
        }

        foreach ($replacement in @($replacements | Sort-Object Start -Descending)) {
            $content = $content.Remove($replacement.Start, $replacement.Length).Insert($replacement.Start, $replacement.Text)
        }

        if ($additions.Count -gt 0) {
            $closingBracket = $content.LastIndexOf(']')
            $beforeClosing = $content.Substring(0, $closingBracket).TrimEnd()
            $separator = if ($beforeClosing.EndsWith('[')) { "`r`n" } else { ",`r`n" }
            $content = $beforeClosing + $separator + ($additions -join ",`r`n") + "`r`n]" + $content.Substring($closingBracket + 1)
        }

        Set-Content -LiteralPath $Path -Value $content -NoNewline -Encoding utf8
    }

    Add-Type -Path "$fullDllPath"
    $elapsed = Measure-Command -Expression {
        $TextInfo = (Get-Culture).TextInfo
        $locator = New-Object -TypeName "Expressif.$($TextInfo.ToTitleCase($class))s.Introspection.$($TextInfo.ToTitleCase($class))Introspector"
        $described = @($locator.Describe() | Sort-Object Scope, Name)
        $functions = @($described |
            Where-Object { $names.Count -eq 0 -or $names -contains $_.Name } |
            Select-Object -Property Name, IsPublic, Aliases, Scope, Input, Output, Summary, Parameters)

        $missingNames = @($names | Where-Object { $_ -notin $functions.Name })
        if ($missingNames.Count -gt 0) {
            throw "Unknown $class name(s): $($missingNames -join ', ')."
        }

        Write-Host  "`t$($functions.Count) $($class.ToLower()) identified"
        $functions | ForEach-Object {
            if ($_.IsPublic) {
                Write-Host "`t`t$($_.Name)"
            } else {
                Write-Warning "`t$($_.Name)"
            }
        }
        $existingEntries = @()
        if (Test-Path -LiteralPath $destination -PathType Leaf) {
            $existingEntries = @(Get-Content -LiteralPath $destination -Raw | ConvertFrom-Json)
        }
        $existing = @{}
        $existingEntries | ForEach-Object { $existing[$_.Name] = $_ }

        $functions | ForEach-Object {
            $previous = $existing[$_.Name]
            if ($null -ne $previous) {
                foreach ($propertyName in @("Behavior", "Examples")) {
                    $property = $previous.PSObject.Properties[$propertyName]
                    if ($null -ne $property) {
                        $_ | Add-Member -NotePropertyName $propertyName -NotePropertyValue $property.Value
                    }
                }
            }
        }

        if ($names.Count -gt 0) {
            Update-JsonArrayEntries -Path $destination -Entries $functions
        } else {
            $functions | ConvertTo-Json -depth 4 | Out-File "$destination"
        }

        if ($class -eq "function") {
            $conversionDestination = Join-Path (Split-Path $destination) "function-conversion.json"
            $conversions = @($described |
                Where-Object { $names.Count -eq 0 -or $names -contains $_.Name } |
                Select-Object -Property Name, Scope, Converted, Input, Output, Reason)
            if ($names.Count -gt 0 -and (Test-Path -LiteralPath $conversionDestination -PathType Leaf)) {
                Update-JsonArrayEntries -Path $conversionDestination -Entries $conversions
            } else {
                $conversions | ConvertTo-Json -Depth 4 | Out-File $conversionDestination
            }
        }
    }
    Write-Host  "File created at $destination in $($elapsed.TotalSeconds) seconds"
} -Args $assemblyPath, $class, $name, "$destinationPath\$destinationFile"
$job | Wait-Job | Out-Null
if ($job.State -eq "Failed") {
    Receive-Job $job
    throw "Metadata generation failed."
}
Receive-Job $job -ErrorAction Stop

########### Check if it's useful to report a change #############

If ($hash.Hash -eq (Get-FileHash $destinationPath\$destinationFile).Hash) {
    Write-Host "No change detected in the list of predicates."
    Exit 0
} else {
    Write-Host "Changes detected in the list of predicates."
    Exit 1
}
