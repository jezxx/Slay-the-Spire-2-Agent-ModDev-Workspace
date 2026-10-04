# Generate StS2-style relic outline pngs (white dilated silhouette, smooth edge).
#
# Why: the engine draws the outline texture BEHIND the icon at the same position
# (NRelic.cs:135 Outline.Texture = Model.IconOutline, always visible for small size).
# An outline that exactly matches the icon shape is fully covered => invisible.
#
# v2 (2026-09-29, user: edges rough, white rim "sparse"):
#   - supersample: work at -Work (default 512), downsample to -OutSize (128)
#     with HighQualityBicubic => anti-aliased smooth output
#   - chamfer 3-4 distance transform (approximates EUCLIDEAN distance, rounded
#     corners) instead of the old Chebyshev BFS ring dilation (square corners)
#   - smoothstep alpha falloff instead of the old linear ramp (denser, even rim)
#
# Usage:
#   .\make-relic-outlines.ps1 -SrcDir <icons dir> -OutDir <outline dir>
#     [-Names a.png,b.png] [-OutSize 128] [-Work 512] [-SolidRim 1.5] [-Dilate 3.0]
#
# Notes:
#   - Pure ASCII only (PS 5.1 reads non-ASCII scripts as GBK; see docs rule K1/H1).
#   - Input: 64x64 RGBA icon pngs. Output: OutSize x OutSize white RGBA png.
#   - SolidRim/Dilate are in 64px-source pixels: 1.5px solid rim + soft falloff
#     to 3.0px matches the vanilla look at ~68px display size.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$SrcDir,
    [Parameter(Mandatory = $true)][string]$OutDir,
    [string]$Names,
    [int]$OutSize = 128,
    [int]$Work = 512,
    [double]$SolidRim = 1.5,
    [double]$Dilate = 3.0,
    [int]$MaskAlphaMin = 128
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $SrcDir)) { throw "SrcDir not found: $SrcDir" }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$files = if ($Names) {
    $Names -split ',' | ForEach-Object { Join-Path $SrcDir $_.Trim() }
} else {
    Get-ChildItem $SrcDir -Filter *.png | ForEach-Object { $_.FullName }
}

foreach ($src in $files) {
    if (-not (Test-Path $src)) { throw "missing input: $src" }
    $srcBmp = [System.Drawing.Bitmap]::new($src)
    if ($srcBmp.Width -ne 64 -or $srcBmp.Height -ne 64) {
        Write-Host ("OUTLINE|WARN|{0}|src is {1}x{2} (expected 64x64)" -f (Split-Path $src -Leaf), $srcBmp.Width, $srcBmp.Height)
    }

    # 1. supersample the source to the working resolution
    $workBmp = [System.Drawing.Bitmap]::new($Work, $Work, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($workBmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.DrawImage($srcBmp, 0, 0, $Work, $Work)
    $g.Dispose()
    $srcBmp.Dispose()

    $n = $Work
    $n2 = $n * $n
    $rect = New-Object System.Drawing.Rectangle(0, 0, $n, $n)
    $data = $workBmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadWrite, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $bytes = New-Object byte[] ($n2 * 4)
    [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $bytes, 0, $bytes.Length)

    # 2. opaque mask (alpha >= MaskAlphaMin) at working resolution
    $mask = [bool[]]::new($n2)
    for ($i = 0; $i -lt $n2; $i++) {
        if ($bytes[$i * 4 + 3] -ge $MaskAlphaMin) { $mask[$i] = $true }
    }

    # 3. chamfer 3-4 distance transform, two passes (d/3 approximates true px)
    $INF = 1000000
    $dist = [int[]]::new($n2)
    for ($i = 0; $i -lt $n2; $i++) { $dist[$i] = $INF }
    for ($y = 0; $y -lt $n; $y++) {
        $row = $y * $n
        for ($x = 0; $x -lt $n; $x++) {
            $i = $row + $x
            if ($mask[$i]) { $dist[$i] = 0; continue }
            $v = $dist[$i]
            if ($x -gt 0 -and $dist[$i - 1] + 3 -lt $v) { $v = $dist[$i - 1] + 3 }
            if ($y -gt 0) {
                $u = $i - $n
                if ($dist[$u] + 3 -lt $v) { $v = $dist[$u] + 3 }
                if ($x -gt 0 -and $dist[$u - 1] + 4 -lt $v) { $v = $dist[$u - 1] + 4 }
                if ($x -lt ($n - 1) -and $dist[$u + 1] + 4 -lt $v) { $v = $dist[$u + 1] + 4 }
            }
            $dist[$i] = $v
        }
    }
    for ($y = $n - 1; $y -ge 0; $y--) {
        $row = $y * $n
        for ($x = $n - 1; $x -ge 0; $x--) {
            $i = $row + $x
            if ($mask[$i]) { continue }
            $v = $dist[$i]
            if ($x -lt ($n - 1) -and $dist[$i + 1] + 3 -lt $v) { $v = $dist[$i + 1] + 3 }
            if ($y -lt ($n - 1)) {
                $u = $i + $n
                if ($dist[$u] + 3 -lt $v) { $v = $dist[$u] + 3 }
                if ($x -lt ($n - 1) -and $dist[$u + 1] + 4 -lt $v) { $v = $dist[$u + 1] + 4 }
                if ($x -gt 0 -and $dist[$u - 1] + 4 -lt $v) { $v = $dist[$u - 1] + 4 }
            }
            $dist[$i] = $v
        }
    }

    # 4. alpha profile: solid rim then smoothstep falloff (distances in true px)
    $scale = $Work / 64.0
    $solidPx = $SolidRim * $scale
    $outerPx = $Dilate * $scale
    for ($i = 0; $i -lt $n2; $i++) {
        $a = 0
        if ($mask[$i]) {
            $a = 255
        } else {
            $dd = $dist[$i] / 3.0
            if ($dd -le $outerPx) {
                if ($dd -le $solidPx) {
                    $a = 255
                } else {
                    $t = ($dd - $solidPx) / ($outerPx - $solidPx)
                    $s = $t * $t * (3.0 - 2.0 * $t)     # smoothstep
                    $a = [int][Math]::Round(255.0 * (1.0 - $s))
                }
            }
        }
        $i4 = $i * 4
        $bytes[$i4] = 255; $bytes[$i4 + 1] = 255; $bytes[$i4 + 2] = 255; $bytes[$i4 + 3] = [byte]$a
    }
    [System.Runtime.InteropServices.Marshal]::Copy($bytes, 0, $data.Scan0, $bytes.Length)
    $workBmp.UnlockBits($data)

    # 5. downsample to the output size (anti-aliasing happens here)
    $outBmp = [System.Drawing.Bitmap]::new($OutSize, $OutSize, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g2 = [System.Drawing.Graphics]::FromImage($outBmp)
    $g2.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g2.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g2.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g2.DrawImage($workBmp, 0, 0, $OutSize, $OutSize)
    $g2.Dispose()
    $workBmp.Dispose()

    $dest = Join-Path $OutDir (Split-Path $src -Leaf)
    $outBmp.Save($dest, [System.Drawing.Imaging.ImageFormat]::Png)
    $outBmp.Dispose()
    Write-Host ("OUTLINE|{0}|{1}x{2}|work={3}|solidRim={4}|dilate={5}|bytes={6}" -f `
        (Split-Path $dest -Leaf), $OutSize, $OutSize, $Work, $SolidRim, $Dilate, (Get-Item $dest).Length)
}
Write-Host 'OUTLINE|DONE'
