# clean-loc-whitespace.ps1 -- strip stray ASCII spaces inside localization string VALUES.
#
# Why: translated localization can retain source-language spaces that
# vanilla Chinese descriptions do not have (space before CJK punctuation, spaces hugging
# rich-text tags / {Placeholder} tokens, spaces between CJK characters, spaces around digits).
#
# ASCII-only on purpose (including the patterns: CJK punctuation is written with .NET \uXXXX
# escapes) so the file can never be mis-decoded by Windows PowerShell.
#
# Usage:
#   .\clean-loc-whitespace.ps1 -LocDir <localization-dir>                 # report only (no writes)
#   .\clean-loc-whitespace.ps1 -LocDir <localization-dir> -Apply          # rewrite the files
#   .\clean-loc-whitespace.ps1 -LocDir <localization-dir> -Apply -Show 16 # print samples
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$LocDir,
    [switch]$Apply,
    [int]$Show = 8
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $LocDir)) { throw "localization dir not found: $LocDir" }

# CJK punctuation, written as escapes so this file stays pure ASCII.
$punctClose = '\uFF0C\u3002\uFF1A\uFF1B\uFF01\uFF1F\u3001\uFF09\u3011\u300B'   # fullwidth , . : ; ! ? and close brackets
$punctOpen  = '\uFF0C\u3002\uFF1A\uFF1B\uFF01\uFF1F\u3001\uFF08\u3010\u300A'   # fullwidth , . : ; ! ? and open brackets
$cjk        = '\u4E00-\u9FFF'

$rules = [ordered]@{
    'space-before-cjk-punct'   = " (?=[$punctClose])"
    'space-after-cjk-punct'    = "(?<=[$punctOpen]) "
    'space-before-tag'         = ' (?=\[[^\[\]]*\])'
    'space-after-tag'          = '(?<=\[[^\[\]]*\]) '
    'space-before-placeholder' = ' (?=\{[A-Za-z0-9_]+:[A-Za-z0-9_]+\(\)\})'
    'space-after-placeholder'  = '(?<=\{[A-Za-z0-9_]+:[A-Za-z0-9_]+\(\)\}) '
    'space-between-cjk'        = "(?<=[$cjk]) (?=[$cjk])"
    'space-cjk-digit'          = "(?<=[$cjk]) (?=[0-9%])|(?<=[0-9%]) (?=[$cjk])"
}

$compiled = @{}
foreach ($k in $rules.Keys) { $compiled[$k] = [regex]$rules[$k] }

# One JSON key per physical line ("KEY": "VALUE",) -- JSON strings cannot contain raw newlines,
# so this stays line-oriented and preserves the file's exact formatting.
$lineRx = [regex]'^(?<pre>[ \t]*"[^"]+":[ \t]*")(?<val>.*)(?<post>",?[ \t]*)$'

$grandTotal = 0
$samples = New-Object System.Collections.Generic.List[object]

foreach ($file in Get-ChildItem $LocDir -Filter *.json | Sort-Object Name) {
    $path     = $file.FullName
    $bytes    = [System.IO.File]::ReadAllBytes($path)
    $hasBom   = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    $text     = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)

    $beforeKeys = ([regex]::Matches($text, '(?m)^\s*"[^"]+":')).Count
    $hitKeys    = 0
    $perRule    = [ordered]@{}
    foreach ($k in $rules.Keys) { $perRule[$k] = 0 }
    $out = New-Object System.Collections.Generic.List[string]

    # keep the file's original line ending style (CRLF files must stay CRLF)
    $nl = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }

    foreach ($line in ($text -split "\r?\n")) {
        $m = $lineRx.Match($line)
        if (-not $m.Success) { $out.Add($line); continue }

        $val = $m.Groups['val'].Value
        $new = $val
        foreach ($k in $rules.Keys) {
            $rx = $compiled[$k]
            $n  = $rx.Matches($new).Count
            if ($n -gt 0) {
                $perRule[$k] += $n
                $new = $rx.Replace($new, '')
            }
        }
        # edge spaces of the value itself
        $trimmed = $new.Trim(' ')
        if ($trimmed -ne $new) { $perRule['trim-edge'] = $perRule['trim-edge'] + 1; $new = $trimmed }

        if ($new -ne $val) {
            $hitKeys++
            if ($samples.Count -lt $Show) {
                $samples.Add([pscustomobject]@{ File = $file.Name; Key = $m.Groups['pre'].Value.Trim(); Before = $val; After = $new })
            }
        }
        $out.Add($m.Groups['pre'].Value + $new + $m.Groups['post'].Value)
    }

    $newText = ($out -join $nl)
    $afterKeys = ([regex]::Matches($newText, '(?m)^\s*"[^"]+":')).Count

    $valid = $true
    $parseErr = ''
    try { $null = $newText | ConvertFrom-Json } catch { $valid = $false; $parseErr = $_.Exception.Message }

    $counts = ($perRule.GetEnumerator() | Where-Object { $_.Value -gt 0 } | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ' '
    Write-Output ("{0,-16} keys {1} -> {2} changed {3} jsonOk {4} {5}" -f `
        $file.Name, $beforeKeys, $afterKeys, $hitKeys, $valid, $counts)
    if (-not $valid) { Write-Output ("    JSON INVALID: " + $parseErr) }

    if ($Apply -and $valid -and $hitKeys -gt 0) {
        [System.IO.File]::WriteAllText($path, $newText, (New-Object System.Text.UTF8Encoding($hasBom)))
        Write-Output ("    written (bom={0}, bytes {1} -> {2})" -f $hasBom, $bytes.Length, (Get-Item $path).Length)
    }

    $grandTotal += $hitKeys
}

Write-Output ""
Write-Output ("TOTAL changed values: {0}   mode: {1}" -f $grandTotal, $(if ($Apply) { 'APPLIED' } else { 'REPORT ONLY (pass -Apply to write)' }))

if ($samples.Count -gt 0) {
    Write-Output ""
    Write-Output "samples (key / before / after):"
    foreach ($s in $samples) {
        Write-Output ("  [{0}] {1}" -f $s.File, $s.Key)
        Write-Output ("    before: {0}" -f ($s.Before -replace "`r", ''))
        Write-Output ("    after : {0}" -f ($s.After  -replace "`r", ''))
    }
}
