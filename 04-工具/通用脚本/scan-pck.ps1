# scan-pck.ps1 -- locate the GDPC directory block inside a Godot 4.x .pck
# Reads the header, then scans the whole file for the 'GDPC' magic using a
# compiled C# helper (byte-by-byte PowerShell over 2GB is far too slow).
# ASCII only, no BOM. PS 5.1 compatible.
[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)][string]$Pck,
  [switch]$ExtractDir,
  [string]$Out = (Join-Path (Get-Location) 'pck-out'),
  [int64]$DirOffset = -1,
  [int]$MaxList = 200,
  [string]$Match = "",
  [switch]$ListDir
)

$ErrorActionPreference = 'Stop'

Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.IO;

public static class PckScan {
    static readonly byte[] Magic = new byte[] { 0x47, 0x44, 0x50, 0x43 };

    public static long[] FindMagic(string path) {
        var hits = new List<long>();
        const int CHUNK = 32 * 1024 * 1024;
        byte[] buf = new byte[CHUNK + 3];
        long fileLen = new FileInfo(path).Length;
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            long filePos = 0;
            int carry = 0;
            while (filePos < fileLen) {
                fs.Seek(filePos, SeekOrigin.Begin);
                int want = (int)Math.Min(CHUNK, fileLen - filePos);
                int got = 0;
                while (got < want) {
                    int r = fs.Read(buf, carry + got, want - got);
                    if (r <= 0) break;
                    got += r;
                }
                int total = carry + got;
                for (int i = 0; i + 4 <= total; i++) {
                    if (buf[i] == Magic[0] && buf[i+1] == Magic[1] && buf[i+2] == Magic[2] && buf[i+3] == Magic[3]) {
                        hits.Add(filePos - carry + i);
                    }
                }
                filePos += got;
                carry = 3;
                Buffer.BlockCopy(buf, total - 3, buf, 0, 3);
            }
        }
        return hits.ToArray();
    }
}
'@

$hits = [PckScan]::FindMagic($Pck)
Write-Host ("File  : {0}" -f $Pck)
Write-Host ("Size  : {0:N0} (0x{1:X})" -f (Get-Item $Pck).Length, (Get-Item $Pck).Length)
Write-Host ("GDPC magic found {0} time(s):" -f $hits.Count)
foreach($h in $hits){ Write-Host ("  0x{0:X8}  ({0})" -f $h) }

$dir = if($DirOffset -ge 0){ [int64]$DirOffset } elseif($hits.Count -ge 2){ $hits[$hits.Count-1] } else { $hits[0] }
Write-Host ("Using dir offset: 0x{0:X8}" -f $dir)

$fs = [System.IO.File]::OpenRead($Pck)
$br = New-Object System.IO.BinaryReader($fs)
$script:pos = $dir
function RdU32 { [void]$fs.Seek($script:pos,'Begin'); $v=$br.ReadUInt32(); $script:pos+=4; return $v }
function RdU64 { [void]$fs.Seek($script:pos,'Begin'); $v=$br.ReadUInt64(); $script:pos+=8; return $v }
function RdBytes([int]$n) { [void]$fs.Seek($script:pos,'Begin'); $b=$br.ReadBytes($n); $script:pos+=$n; return $b }
function Align4 { $script:pos = $script:pos + ((4 - ($script:pos % 4)) % 4) }

$dmagic = RdU32
$dflags = RdU32
$dfilebase = RdU64
$doffset = RdU64
$dcount  = RdU64
Write-Host ("DirHeader: magic=0x{0:X8} flags=0x{1:X} file_base={2} dir_offset={3} file_count={4}" -f $dmagic,$dflags,$dfilebase,$doffset,$dcount)

$entries = New-Object System.Collections.Generic.List[object]
for($i=0; $i -lt $dcount; $i++){
  $plen = RdU32
  if($plen -le 0 -or $plen -gt 4096){ Write-Host ("bad path_len at entry {0}" -f $i); break }
  $pb = RdBytes $plen
  $p = [System.Text.Encoding]::UTF8.GetString($pb)
  Align4
  $off = RdU64
  $size = RdU64
  $md5 = RdBytes 16
  $fflags = RdU32
  $entries.Add([PSCustomObject]@{ Path=$p; Offset=$off; Size=$size; Flags=$fflags })
}
$br.Close(); $fs.Close()
Write-Host ("Indexed : {0:N0} entries (expected {1:N0})" -f $entries.Count, $dcount)

if($ListDir){
  $sel = $entries
  if($Match -ne ""){ $sel = @($entries | Where-Object { $_.Path -match $Match }) }
  Write-Host ("Matched : {0:N0}" -f $sel.Count)
  $sel | Select-Object -First $MaxList | ForEach-Object { Write-Host ("{0,13:N0}  {1}" -f $_.Size, $_.Path) }
}

if($ExtractDir){
  if(-not (Test-Path $Out)){ New-Item -ItemType Directory -Path $Out -Force | Out-Null }
  $fs2 = [System.IO.File]::OpenRead($Pck)
  foreach($e in $entries){
    $rel = $e.Path -replace '^res://','' -replace '/','\'
    $dest = Join-Path $Out $rel
    $dd = Split-Path $dest -Parent
    if(-not (Test-Path $dd)){ New-Item -ItemType Directory -Path $dd -Force | Out-Null }
    [void]$fs2.Seek([int64]$e.Offset + [int64]$dfilebase,'Begin')
    $in = New-Object byte[] ([int]$e.Size)
    $got = $fs2.Read($in,0,[int]$e.Size)
    if($got -ne $e.Size){ Write-Warning "SHORT $rel" }
    else { [System.IO.File]::WriteAllBytes($dest,$in) }
  }
  $fs2.Close()
  Write-Host "Extracted $($entries.Count) files to $Out"
}
