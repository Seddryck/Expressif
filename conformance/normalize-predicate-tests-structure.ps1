$ErrorActionPreference = 'Stop'

function Get-Indent([string]$line) {
  return $line.Length - $line.TrimStart().Length
}

$files = Get-ChildItem -Path predicates -Recurse -Filter *.yaml
$updated = 0

foreach ($file in $files) {
  $lines = Get-Content -Path $file.FullName

  $hasTests = $false
  $casesIndex = -1
  $operatorValue = ''

  for ($i = 0; $i -lt $lines.Count; $i++) {
    $t = $lines[$i].TrimStart()
    $ind = Get-Indent $lines[$i]

    if ($t -match '^operator:\s*(.*)$') {
      $operatorValue = $Matches[1].Trim().Trim('"',"'")
    }
    if ($t -eq 'tests:') { $hasTests = $true }
    if ($ind -eq 0 -and $t -eq 'cases:') { $casesIndex = $i }
  }

  if ($hasTests -or $casesIndex -lt 0) { continue }

  if ([string]::IsNullOrWhiteSpace($operatorValue)) { $operatorValue = 'predicate' }

  $out = New-Object System.Collections.Generic.List[string]

  for ($i = 0; $i -lt $casesIndex; $i++) {
    $out.Add($lines[$i])
  }

  $out.Add('tests:')
  $out.Add('  - id: ' + $operatorValue + '.valid')
  $out.Add('    cases:')

  for ($i = $casesIndex + 1; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    if ($line.Trim() -eq '') {
      $out.Add($line)
      continue
    }
    $out.Add('    ' + $line)
  }

  Set-Content -Path $file.FullName -Value $out
  $updated++
}

Write-Output "Predicate files scanned: $($files.Count)"
Write-Output "Files normalized: $updated"
