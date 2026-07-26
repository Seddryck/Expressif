$ErrorActionPreference = 'Stop'

$files = Get-ChildItem -Recurse -Filter *.yaml
$updated = 0

foreach ($file in $files) {
  $lines = Get-Content -Path $file.FullName
  $out = New-Object System.Collections.Generic.List[string]
  $changed = $false

  for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    $trim = $line.TrimStart()
    $indent = $line.Length - $trim.Length

    if ($trim -eq 'metadata:') {
      $changed = $true
      $metaIndent = $indent
      $i++
      while ($i -lt $lines.Count) {
        $candidate = $lines[$i]
        $candTrim = $candidate.Trim()
        if ($candTrim -eq '') {
          $i++
          continue
        }
        $candIndent = $candidate.Length - $candidate.TrimStart().Length
        if ($candIndent -le $metaIndent) {
          $i--
          break
        }
        $i++
      }
      continue
    }

    $out.Add($line)
  }

  if ($changed) {
    Set-Content -Path $file.FullName -Value $out
    $updated++
  }
}

Write-Output "YAML files scanned: $($files.Count)"
Write-Output "Files updated: $updated"
