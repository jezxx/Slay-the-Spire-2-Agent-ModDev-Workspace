# Verifies gen-art-map.ps1's Slugify against the ENGINE's Slugify, byte for byte.
#
# Engine algorithm, recovered from sts2.dll IL (api/ildump/StringHelper.txt):
#   MegaCrit.Sts2.Core.Helpers.StringHelper::Slugify(txt)
#     loc0 = CamelCaseRegex.Replace(txt.Trim(), "$1_$2")
#     loc1 = WhitespaceRegex.Replace(loc0.ToUpperInvariant(), "_")
#     return SpecialCharRegex.Replace(loc1, "")
#
# Regex values recovered from the assembly's UTF-16 string heap:
#   CamelCaseRegex  = ([A-Za-z0-9]|\G(?!^))([A-Z])
#   WhitespaceRegex = [\t\r\n]
#   SpecialCharRegex= [^A-Z0-9_]
#
# Every art filename in the mod derives from this function, so a silent
# disagreement here means permanently invisible art. This script fails loudly.
param(
    [Parameter(Mandatory = $true)][string]$MapFile,
    [string]$ReportFile = "",
    [string]$Prefix = ""
)

$ErrorActionPreference = "Stop"

$ToolsDir = $PSScriptRoot
$DevDir   = Split-Path $ToolsDir -Parent
if (-not (Test-Path -LiteralPath $MapFile)) { throw "map file not found: $MapFile" }
if (-not $ReportFile) { $ReportFile = Join-Path (Split-Path -Parent $MapFile) "slugify-audit.txt" }

# --- the engine's algorithm, transcribed from IL + regex literals ---
$camel   = New-Object System.Text.RegularExpressions.Regex '([A-Za-z0-9]|\G(?!^))([A-Z])'
$space   = New-Object System.Text.RegularExpressions.Regex '[\t\r\n]'
$special = New-Object System.Text.RegularExpressions.Regex '[^A-Z0-9_]'

function EngineSlugify([string]$txt) {
    if ($null -eq $txt) { return "" }
    $s0 = $camel.Replace($txt.Trim(), '$1_$2')
    $s1 = $space.Replace($s0.ToUpperInvariant(), '_')
    return $special.Replace($s1, '')
}

# --- the generator's algorithm, as currently written in gen-art-map.ps1 ---
function GenSlugify([string]$cls) {
    $sb = New-Object System.Text.StringBuilder
    for ($i = 0; $i -lt $cls.Length; $i++) {
        $c = $cls[$i]
        if ([char]::IsUpper($c)) {
            if ($i -gt 0) { [void]$sb.Append('_') }
            [void]$sb.Append($c)
        } else {
            [void]$sb.Append([char]::ToUpperInvariant($c))
        }
    }
    $sb.ToString()
}

# --- regression vectors: engine-documented SlugifyCategory outputs (ModelId-scheme.txt) ---
$known = @{
    'Bash'              = 'BASH'
    'IroncladCardPool'  = 'IRONCLAD_CARD_POOL'
    'ColorlessCardPool' = 'COLORLESS_CARD_POOL'
    'Strike'            = 'STRIKE'
    'TestCard'          = 'TEST_CARD'
    'IroncladStrikeCard'= 'IRONCLAD_STRIKE_CARD'
    'Cards'             = 'CARDS'
}

$lines = New-Object System.Collections.Generic.List[string]
[void]$lines.Add("Slugify audit (engine algorithm vs map)")
[void]$lines.Add("engine: CamelCaseRegex=([A-Za-z0-9]|\G(?!^))([A-Z])  replacement=`$1_`$2")
[void]$lines.Add("        WhitespaceRegex=[\t\r\n] -> '_'   then  SpecialCharRegex=[^A-Z0-9_] -> ''")
[void]$lines.Add("")

$fail = 0

# 1. sanity: does our transcription reproduce the engine's own documented outputs?
[void]$lines.Add("--- known-good vectors (from api/ModelId-scheme.txt) ---")
foreach ($k in ($known.Keys | Sort-Object)) {
    $got = EngineSlugify $k
    $ok  = ($got -eq $known[$k])
    if (-not $ok) { $fail++ }
    [void]$lines.Add(("  {0,-18} expect={1,-24} got={2,-24} {3}" -f $k, $known[$k], $got, $(if ($ok) { "OK" } else { "MISMATCH" })))
}
[void]$lines.Add("")

# 2. does the transcription agree with the generator on the map source names?
$rows = Import-Csv $MapFile
[void]$lines.Add("--- map source class names ($($rows.Count) rows) ---")
$mismatch = @()
foreach ($r in $rows) {
    $name = $r.src_class
    $e = $Prefix + (EngineSlugify $name)
    $g = $r.entry
    if ($e -ne $g) { $mismatch += [pscustomobject]@{ kind=$r.kind; name=$name; csv=$g; engine=$e } }
}
if ($mismatch.Count -eq 0) {
    [void]$lines.Add("  all $($rows.Count) entries match the engine exactly.")
} else {
    $fail += $mismatch.Count
    foreach ($m in $mismatch) {
        [void]$lines.Add(("  MISMATCH {0} {1}: csv={2} engine={3}" -f $m.kind, $m.name, $m.csv, $m.engine))
    }
}
[void]$lines.Add("")

# 3. demonstrate WHERE the two algorithms can diverge, so the guard is justified
[void]$lines.Add("--- divergence probes (why this audit matters) ---")
foreach ($probe in @('ASingleThoughtAndInfiniteKalpas','ABCard','Power2Card','XMLParser','A')) {
    $e = EngineSlugify $probe
    $g = GenSlugify $probe
    $same = ($e -eq $g)
    [void]$lines.Add(("  {0,-34} engine={1,-34} generator={2,-34} {3}" -f $probe, $e, $g, $(if ($same) { "same" } else { "DIVERGES" })))
}
[void]$lines.Add("")
[void]$lines.Add("RESULT: $fail problem(s)")

[System.IO.File]::WriteAllLines($ReportFile, $lines, (New-Object System.Text.UTF8Encoding($true)))
$lines | ForEach-Object { Write-Output $_ }
if ($fail -gt 0) { exit 1 }
exit 0
