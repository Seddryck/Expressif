$ErrorActionPreference = 'Stop'

function Get-Indent([string]$line) {
  return $line.Length - $line.TrimStart().Length
}

function Get-TestIdFromCaseId([string]$caseId) {
  if ([string]::IsNullOrWhiteSpace($caseId)) { return $caseId }
  $parts = $caseId -split '\.'
  if ($parts.Count -le 2) { return $caseId }
  return (($parts[0..($parts.Count - 3)]) -join '.')
}

function Remove-TraceAndLine([string[]]$lines) {
  $out = New-Object System.Collections.Generic.List[string]

  for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    $trim = $line.TrimStart()
    $indent = Get-Indent $line

    if ($trim -eq '') {
      $out.Add($line)
      continue
    }

    if ($trim -match '^line:\s*') {
      continue
    }

    if ($trim -eq 'trace:') {
      $traceIndent = $indent
      $i++
      while ($i -lt $lines.Count) {
        $candidate = $lines[$i]
        $candidateTrim = $candidate.Trim()
        if ($candidateTrim -eq '') {
          $i++
          continue
        }
        $candidateIndent = Get-Indent $candidate
        if ($candidateIndent -le $traceIndent) {
          $i--
          break
        }
        $i++
      }
      continue
    }

    $out.Add($line)
  }

  return $out
}

function Update-FunctionFile([string]$path) {
  $lines = Get-Content -Path $path
  $lines = Remove-TraceAndLine $lines

  $changes = 0
  for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    $trim = $line.TrimStart()
    $indent = Get-Indent $line

    if ($indent -eq 2 -and $trim -match '^- id:\s*(\S.*?)\s*$') {
      $newTestId = $null
      for ($j = $i + 1; $j -lt $lines.Count; $j++) {
        $candidate = $lines[$j]
        $candidateTrim = $candidate.TrimStart()
        $candidateIndent = Get-Indent $candidate
        if ($candidate.Trim() -ne '' -and $candidateIndent -le 2) {
          break
        }
        if ($candidateIndent -eq 6 -and $candidateTrim -match '^- id:\s*(\S.*?)\s*$') {
          $newTestId = Get-TestIdFromCaseId $Matches[1].Trim()
          break
        }
      }

      if (-not [string]::IsNullOrWhiteSpace($newTestId)) {
        $newLine = '  - id: ' + $newTestId
        if ($lines[$i] -ne $newLine) {
          $lines[$i] = $newLine
          $changes++
        }
      }
    }
  }

  if ($changes -gt 0 -or ($lines -join "`n") -ne ((Get-Content -Path $path) -join "`n")) {
    Set-Content -Path $path -Value $lines
  }
  return $changes
}

function Update-GenericFile([string]$path) {
  $original = Get-Content -Path $path
  $updated = Remove-TraceAndLine $original
  if (($original -join "`n") -ne ($updated -join "`n")) {
    Set-Content -Path $path -Value $updated
    return 1
  }
  return 0
}

$updatedFiles = 0
$renamedTests = 0

$functionFiles = Get-ChildItem -Path 'functions' -Recurse -Filter *.yaml
foreach ($file in $functionFiles) {
  $renamed = Update-FunctionFile $file.FullName
  $updatedFiles += 1
  $renamedTests += $renamed
}

$predicateFiles = Get-ChildItem -Path 'predicates' -Recurse -Filter *.yaml
foreach ($file in $predicateFiles) {
  $changed = Update-GenericFile $file.FullName
  if ($changed -gt 0) {
    $updatedFiles += 0
  }
}

Write-Output "Function files processed: $($functionFiles.Count)"
Write-Output "Predicate files processed: $($predicateFiles.Count)"
Write-Output "Tests renamed: $renamedTests"