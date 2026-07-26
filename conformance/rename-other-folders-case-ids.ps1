$ErrorActionPreference = 'Stop'

function Convert-ToKebab([string]$s) {
  if ([string]::IsNullOrWhiteSpace($s)) { return '' }
  $r = $s -creplace '([a-z0-9])([A-Z])', '$1-$2'
  $r = $r -creplace '([A-Za-z])(\d)', '$1-$2'
  $r = $r -creplace '(\d)([A-Za-z])', '$1-$2'
  $r = $r -replace '_', '-'
  $r = $r -replace '-+', '-'
  return $r.Trim('-').ToLowerInvariant()
}

function Get-Indent([string]$line) {
  return $line.Length - $line.TrimStart().Length
}

function Get-Category([string]$suite, [string]$valueToken) {
  if ($valueToken -in @('null','empty','blank')) { return 'special' }
  switch ($suite) {
    'io' { return 'io' }
    'text' { return 'text' }
    'temporal' { return 'temporal' }
    'special' { return 'special' }
    'boolean' { return 'boolean' }
    'numeric' { return 'numeric' }
    default { return $suite }
  }
}

function Get-ShortNumberWord([string]$digits) {
  $map = @{
    '0'='zero'; '1'='one'; '2'='two'; '3'='three'; '4'='four'; '5'='five';
    '6'='six'; '7'='seven'; '8'='eight'; '9'='nine'; '10'='ten'; '12'='twelve';
    '15'='fifteen'; '16'='sixteen'; '24'='twenty-four'; '32'='thirty-two';
    '45'='forty-five'; '67'='sixty-seven'; '104'='one-hundred-four'
  }
  if ($map.ContainsKey($digits)) { return $map[$digits] }
  return $digits
}

function Get-ValueToken([string]$value) {
  if ($null -eq $value) { return 'empty' }
  $t = $value.Trim()
  if ($t -eq '') { return 'empty' }
  switch -Regex ($t.ToLowerInvariant()) {
    '^\(null\)$' { return 'null' }
    '^\(empty\)$' { return 'empty' }
    '^\(blank\)$' { return 'blank' }
    '^null$' { return 'null' }
    '^true$' { return 'true' }
    '^false$' { return 'false' }
  }
  if ($t -match '^-?\d+$') {
    if ($t.StartsWith('-')) { return 'minus-' + (Get-ShortNumberWord $t.Substring(1)) }
    return Get-ShortNumberWord $t
  }
  if ($t -match '^-?\d+\.\d+$') {
    $negative = $t.StartsWith('-')
    $abs = if ($negative) { $t.Substring(1) } else { $t }
    switch ($abs) {
      '0.5' {
        if ($negative) { return 'negative-half' }
        return 'positive-half'
      }
      '0.25' {
        if ($negative) { return 'negative-quarter' }
        return 'positive-quarter'
      }
    }
    $slug = $abs -replace '\.', '-'
    if ($negative) { return 'negative-' + $slug }
    return 'positive-' + $slug
  }
  $u = $t.ToLowerInvariant()
  $u = $u -replace 'c:\\', 'drive-c-'
  $u = $u -replace '[\\/]', '-'
  $u = $u -replace '[^a-z0-9]+', '-'
  $u = $u -replace '-+', '-'
  $u = $u.Trim('-')
  if ($u.Length -gt 40) { $u = $u.Substring(0, 40).Trim('-') }
  if ([string]::IsNullOrWhiteSpace($u)) { return 'value' }
  return $u
}

function Get-ParamsToken([System.Collections.Generic.List[string]]$params) {
  if ($null -eq $params -or $params.Count -eq 0) { return '' }
  $tokens = @()
  foreach ($param in $params) { $tokens += (Get-ValueToken $param) }
  return ($tokens -join '-and-')
}

function Get-TestPrefix([string]$operator, [string]$testId) {
  $parts = $testId -split '_'
  if ($parts.Count -eq 0) { return $operator }
  $validity = ''
  $variant = ''
  if ($parts.Count -ge 2) {
    $validity = $parts[-1].ToLowerInvariant()
    if ($parts.Count -gt 2) {
      $variant = Convert-ToKebab (($parts[1..($parts.Count - 2)] -join '-'))
    }
  }
  $segments = New-Object System.Collections.Generic.List[string]
  $segments.Add($operator)
  if (-not [string]::IsNullOrWhiteSpace($validity)) { $segments.Add($validity) }
  if (-not [string]::IsNullOrWhiteSpace($variant)) { $segments.Add($variant) }
  return ($segments -join '.')
}

function Update-FunctionFile([string]$path) {
  $lines = Get-Content -Path $path
  $suite = ''
  $operator = ''
  for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match '^suite:\s*(.+?)\s*$') { $suite = $Matches[1].Trim() }
    if ($lines[$i] -match '^operator:\s*(.+?)\s*$') { $operator = $Matches[1].Trim() }
  }
  if ([string]::IsNullOrWhiteSpace($suite) -or [string]::IsNullOrWhiteSpace($operator)) { return 0 }

  $currentTestId = ''
  $currentPrefix = ''
  $changes = 0
  $caseInfos = New-Object System.Collections.Generic.List[object]

  for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    $indent = Get-Indent $line
    $trim = $line.TrimStart()

    if ($indent -eq 2 -and $trim -match '^- id:\s*(\S.*?)\s*$') {
      $currentTestId = $Matches[1].Trim()
      $currentPrefix = Get-TestPrefix $operator $currentTestId
      continue
    }

    if ($indent -eq 6 -and $trim -match '^- id:\s*(\S.*?)\s*$') {
      $oldId = $Matches[1].Trim()
      $block = New-Object System.Collections.Generic.List[string]
      $j = $i + 1
      while ($j -lt $lines.Count) {
        $cand = $lines[$j]
        $candTrim = $cand.Trim()
        $candIndent = Get-Indent $cand
        if ($candTrim -ne '' -and $candIndent -le 6) { break }
        $block.Add($cand)
        $j++
      }

      $value = ''
      $expected = ''
      $params = New-Object System.Collections.Generic.List[string]
      $lineNo = ''
      $mode = ''
      foreach ($b in $block) {
        $t = $b.Trim()
        if ($t -match '^value:\s*(.*)$') { $value = $Matches[1]; $mode = ''; continue }
        if ($t -match '^expected:\s*(.*)$') { $expected = $Matches[1]; $mode = ''; continue }
        if ($t -match '^line:\s*(.*)$') { $lineNo = $Matches[1]; $mode = ''; continue }
        if ($t -match '^parameters:\s*$') { $mode = 'parameters'; continue }
        if ($mode -eq 'parameters' -and $t -match '^-\s+(.*)$') { $params.Add($Matches[1]); continue }
      }

      $valueToken = Get-ValueToken $value
      $expectedToken = Get-ValueToken $expected
      $paramsToken = Get-ParamsToken $params
      $category = Get-Category $suite $valueToken
      $baseScenario = $valueToken

      $caseInfos.Add([pscustomobject]@{
        LineIndex = $i
        Prefix = $currentPrefix
        OldId = $oldId
        Category = $category
        BaseScenario = $baseScenario
        ExpectedToken = $expectedToken
        ParamsToken = $paramsToken
        SourceLine = $lineNo
      })
    }
  }

  $byPrefix = $caseInfos | Group-Object Prefix
  foreach ($prefixGroup in $byPrefix) {
    $cases = @($prefixGroup.Group)
    $byBase = $cases | Group-Object BaseScenario
    foreach ($baseGroup in $byBase) {
      $sameBase = @($baseGroup.Group)
      if ($sameBase.Count -eq 1) {
        $sameBase[0] | Add-Member -NotePropertyName Scenario -NotePropertyValue $sameBase[0].BaseScenario -Force
        continue
      }

      $byExpected = @{}
      foreach ($case in $sameBase) {
        $scenario = "$($case.BaseScenario)-to-$($case.ExpectedToken)"
        if (-not $byExpected.ContainsKey($scenario)) { $byExpected[$scenario] = @() }
        $byExpected[$scenario] += $case
      }
      foreach ($scenario in $byExpected.Keys) {
        $sameScenario = @($byExpected[$scenario])
        if ($sameScenario.Count -eq 1) {
          $sameScenario[0] | Add-Member -NotePropertyName Scenario -NotePropertyValue $scenario -Force
          continue
        }
        $byParams = @{}
        foreach ($case in $sameScenario) {
          $full = if ([string]::IsNullOrWhiteSpace($case.ParamsToken)) { $scenario } else { "$scenario-with-$($case.ParamsToken)" }
          if (-not $byParams.ContainsKey($full)) { $byParams[$full] = @() }
          $byParams[$full] += $case
        }
        foreach ($full in $byParams.Keys) {
          $sameFull = @($byParams[$full])
          if ($sameFull.Count -eq 1) {
            $sameFull[0] | Add-Member -NotePropertyName Scenario -NotePropertyValue $full -Force
            continue
          }
          foreach ($case in $sameFull) {
            $case | Add-Member -NotePropertyName Scenario -NotePropertyValue "$full-source-line-$($case.SourceLine)" -Force
          }
        }
      }
    }

    foreach ($case in $cases) {
      if (-not ($case.PSObject.Properties.Name -contains 'Scenario')) {
        $case | Add-Member -NotePropertyName Scenario -NotePropertyValue $case.BaseScenario -Force
      }
      $newId = "$($case.Prefix).$($case.Category).$($case.Scenario)"
      $newLine = (' ' * 6) + '- id: ' + $newId
      if ($lines[$case.LineIndex] -ne $newLine) {
        $lines[$case.LineIndex] = $newLine
        $changes++
      }
    }
  }

  if ($changes -gt 0) { Set-Content -Path $path -Value $lines }
  return $changes
}

function Get-TracePrefix([string]$traceId) {
  if ([string]::IsNullOrWhiteSpace($traceId)) { return 'case' }
  $parts = $traceId -split '_'
  $segments = @()
  foreach ($part in $parts) {
    $segments += (Convert-ToKebab $part)
  }
  return (($segments | Where-Object { $_ -ne '' }) -join '.')
}

function Update-PredicateFile([string]$path) {
  $lines = Get-Content -Path $path
  $suite = ''
  for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match '^suite:\s*(.+?)\s*$') { $suite = $Matches[1].Trim() }
  }
  if ([string]::IsNullOrWhiteSpace($suite)) { return 0 }

  $out = New-Object System.Collections.Generic.List[string]
  $changes = 0
  $inCases = $false
  $caseInfos = New-Object System.Collections.Generic.List[object]

  for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    $indent = Get-Indent $line
    $trim = $line.TrimStart()

    if ($indent -eq 0 -and $trim -eq 'cases:') {
      $inCases = $true
      continue
    }

    if ($inCases -and $indent -eq 2 -and $trim.StartsWith('- ')) {
      $block = New-Object System.Collections.Generic.List[string]
      $start = $i
      $j = $i
      while ($j -lt $lines.Count) {
        $cand = $lines[$j]
        $candTrim = $cand.Trim()
        $candIndent = Get-Indent $cand
        if ($j -gt $start -and $candTrim -ne '' -and $candIndent -le 2) { break }
        $block.Add($cand)
        $j++
      }

      $value = ''
      $expected = ''
      $traceId = ''
      $lineNo = ''
      $mode = ''
      foreach ($b in $block) {
        $t = $b.Trim()
        if ($t -match '^-\s+id:\s*(.*)$') { continue }
        if ($t -match '^-\s+value:\s*(.*)$') { $value = $Matches[1]; $mode = ''; continue }
        if ($t -match '^value:\s*(.*)$') { $value = $Matches[1]; $mode = ''; continue }
        if ($t -match '^expected:\s*(.*)$') { $expected = $Matches[1]; $mode = ''; continue }
        if ($t -match '^id:\s*(.*)$' -and $mode -eq 'trace') { $traceId = $Matches[1].Trim(); continue }
        if ($t -match '^line:\s*(.*)$' -and $mode -eq 'trace') { $lineNo = $Matches[1].Trim(); continue }
        if ($t -match '^trace:\s*$') { $mode = 'trace'; continue }
      }

      $prefix = Get-TracePrefix $traceId
      $valueToken = Get-ValueToken $value
      $expectedToken = Get-ValueToken $expected
      $category = Get-Category $suite $valueToken

      $caseInfos.Add([pscustomobject]@{
        StartIndex = $i
        Prefix = $prefix
        ValueToken = $valueToken
        ExpectedToken = $expectedToken
        Category = $category
        SourceLine = $lineNo
        Block = @($block)
      })
    }
  }

  $byPrefix = $caseInfos | Group-Object Prefix
  foreach ($prefixGroup in $byPrefix) {
    $cases = @($prefixGroup.Group)
    $byValue = $cases | Group-Object ValueToken
    foreach ($valueGroup in $byValue) {
      $sameValue = @($valueGroup.Group)
      if ($sameValue.Count -eq 1) {
        $sameValue[0] | Add-Member -NotePropertyName Scenario -NotePropertyValue $sameValue[0].ValueToken -Force
        continue
      }

      $byExpected = @{}
      foreach ($case in $sameValue) {
        $scenario = "$($case.ValueToken)-to-$($case.ExpectedToken)"
        if (-not $byExpected.ContainsKey($scenario)) { $byExpected[$scenario] = @() }
        $byExpected[$scenario] += $case
      }
      foreach ($scenario in $byExpected.Keys) {
        $sameScenario = @($byExpected[$scenario])
        if ($sameScenario.Count -eq 1) {
          $sameScenario[0] | Add-Member -NotePropertyName Scenario -NotePropertyValue $scenario -Force
          continue
        }
        foreach ($case in $sameScenario) {
          $case | Add-Member -NotePropertyName Scenario -NotePropertyValue "$scenario-source-line-$($case.SourceLine)" -Force
        }
      }
    }
  }

  for ($i = 0; $i -lt $lines.Count; ) {
    $line = $lines[$i]
    $indent = Get-Indent $line
    $trim = $line.TrimStart()

    if ($indent -eq 0 -and $trim -eq 'cases:') {
      $out.Add($line)
      $inCases = $true
      $i++
      continue
    }

    if ($inCases -and $indent -eq 2 -and $trim.StartsWith('- ')) {
      $caseInfo = $caseInfos | Where-Object { $_.StartIndex -eq $i } | Select-Object -First 1
      if ($null -eq $caseInfo) {
        $out.Add($line)
        $i++
        continue
      }
      if (-not ($caseInfo.PSObject.Properties.Name -contains 'Scenario')) {
        $caseInfo | Add-Member -NotePropertyName Scenario -NotePropertyValue $caseInfo.ValueToken -Force
      }
      $newId = "$($caseInfo.Prefix).$($caseInfo.Category).$($caseInfo.Scenario)"
      $out.Add('  - id: ' + $newId)

      $block = @($caseInfo.Block)
      $firstTrim = $block[0].TrimStart()
      if ($firstTrim -match '^-\s+id:\s*') {
        $startAt = 1
      } else {
        $startAt = 0
      }
      for ($k = $startAt; $k -lt $block.Count; $k++) {
        if ($k -eq $startAt -and $block[$k].TrimStart().StartsWith('- ')) {
          $out.Add('    ' + $block[$k].TrimStart().Substring(2))
        } else {
          $out.Add($block[$k])
        }
      }
      $changes++
      $i += $block.Count
      continue
    }

    $out.Add($line)
    $i++
  }

  if ($changes -gt 0) { Set-Content -Path $path -Value $out }
  return $changes
}

$totalFiles = 0
$totalIds = 0

$functionFiles = Get-ChildItem -Path 'functions' -Recurse -Filter *.yaml | Where-Object { $_.FullName -notmatch '\\functions\\numeric\\' }
foreach ($file in $functionFiles) {
  $changes = Update-FunctionFile $file.FullName
  if ($changes -gt 0) {
    $totalFiles++
    $totalIds += $changes
  }
}

$predicateFiles = Get-ChildItem -Path 'predicates' -Recurse -Filter *.yaml
foreach ($file in $predicateFiles) {
  $changes = Update-PredicateFile $file.FullName
  if ($changes -gt 0) {
    $totalFiles++
    $totalIds += $changes
  }
}

Write-Output "Updated files: $totalFiles"
Write-Output "Updated case ids: $totalIds"
