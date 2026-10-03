# read-pck.ps1 -- list / extract files inside a Godot 4.x .pck
# NOTE: ASCII only, no BOM. Windows PowerShell 5.1 compatible.
# Usage:
#   .\read-pck.ps1 -Pck <path.pck> -List -Match "card_portraits" -Max 200
#   .\read-pck.ps1 -Pck <path.pck> -Extract -Match "relic" -Out "<output directory>"
[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)][string]$Pck,
  [switch]$List,
  [switch]$Extract,
  [string]$Match = "",
  [string]$Out = (Join-Path (Get-Location) 'pck-out'),
  [int]$Max = 300,
  [int64]$IndexLimit = 0
)

$ErrorActionPreference = 'Stop'
$fs = [System.IO.File]::OpenRead($Pck)
$br = New-Object System.IO.BinaryReader($fs)
$script:pos = 0

function RdU32 { [void]$fs.Seek($script:pos,'Begin'); $v=$br.ReadUInt32(); $script:pos+=4; return $v }
function RdU64 { [void]$fs.Seek($script:pos,'Begin'); $v=$br.ReadUInt64(); $script:pos+=8; return $v }
function RdBytes([int]$n) { [void]$fs.Seek($script:pos,'Begin'); $b=$br.ReadBytes($n); $script:pos+=$n; return $b }
function Align4 { $script:pos = $script:pos + ((4 - ($script:pos % 4)) % 4) }

$magic = RdU32
$fmtv  = RdU32
$vMaj  = RdU32
$vMin  = RdU32
$vPat  = RdU32
$flags = RdU32
$fileBase = RdBytes 16
$count  = RdU64

Write-Host "Pck       : $Pck"
Write-Host ("Size      : {0:N0} bytes" -f $fs.Length)
Write-Host ("Magic     : 0x{0:X8} packfmt={1} godot={2}.{3}.{4} flags=0x{5:X}" -f $magic,$fmtv,$vMaj,$vMin,$vPat,$flags)
Write-Host ("FileCount : {0:N0}" -f $count)

$headerEnd = $script:pos

# pack_flags bit0 = REL_FILEBASE: directory block stored at offset from file_base
if(($flags -band 1) -ne 0){
  $dirOff = [uint64]$fileBase[0] -bor ([uint64]$fileBase[1] -shl 8) -bor ([uint64]$fileBase[2] -shl 16) -bor ([uint64]$fileBase[3] -shl 24)
  Write-Host "DirOffset : $dirOff (REL_FILEBASE flag set)"
  $script:pos = [int64]$dirOff
  $cmagic = RdU32
  $dflags = RdU32
  $count = RdU64
  if($cmagic -ne 0x43504447){ throw ("dir magic mismatch 0x{0:X8}" -f $cmagic) }
  $script:pos += 16   # reserved[4] uint32
  $headerEnd = $script:pos
}

if($IndexLimit -gt 0 -and $count -gt $IndexLimit){ $count = [uint64]$IndexLimit }
$script:pos = $headerEnd

$entries = New-Object System.Collections.Generic.List[object]
for($i=0; $i -lt $count; $i++){
  $plen = RdU32
  if($plen -le 0 -or $plen -gt 4096){ break }
  $pb = RdBytes $plen
  $p = [System.Text.Encoding]::UTF8.GetString($pb)
  Align4
  $off = RdU64
  $size = RdU64
  $md5 = RdBytes 16
  $fflags = RdU32
  $entries.Add([PSCustomObject]@{ Path=$p; Offset=$off; Size=$size; Flags=$fflags })
}

$br.Close()
$fs.Close()
Write-Host ("Indexed  : {0:N0} entries" -f $entries.Count)

$sel = $entries
if($Match -ne ""){ $sel = @($entries | Where-Object { $_.Path -match $Match }) }
Write-Host ("Matched  : {0:N0}" -f $sel.Count)

if($List){
  $sel | Select-Object -First $Max | ForEach-Object { Write-Host ("{0,13:N0}  {1}" -f $_.Size, $_.Path) }
}

if($Extract -and $sel.Count -gt 0){
  if(-not (Test-Path $Out)){ New-Item -ItemType Directory -Path $Out -Force | Out-Null }
  $fs2 = [System.IO.File]::OpenRead($Pck)
  foreach($e in $sel){
    $rel = $e.Path -replace '^res://','' -replace '/','\'
    $dest = Join-Path $Out $rel
    $dd = Split-Path $dest -Parent
    if(-not (Test-Path $dd)){ New-Item -ItemType Directory -Path $dd -Force | Out-Null }
    [void]$fs2.Seek([int64]$e.Offset,'Begin')
    $in = New-Object byte[] ([int]$e.Size)
    $got = $fs2.Read($in,0,[int]$e.Size)
    if($got -eq $e.Size){ [System.IO.File]::WriteAllBytes($dest,$in) }
    else { Write-Warning "SHORT $rel ($got/$($e.Size))" }
  }
  $fs2.Close()
  Write-Host "Extracted to $Out"
}
