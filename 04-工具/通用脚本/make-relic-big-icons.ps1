param(
    [Parameter(Mandatory = $true)][string]$SrcDir,
    [Parameter(Mandatory = $true)][string]$OutDir,
    [Parameter(Mandatory = $true)][string[]]$Names,
    [int]$Size = 256,
    [int]$Work = 512,
    [double]$SolidRim = 3.0,
    [double]$Dilate = 8.0,
    [int]$MaskAlphaMin = 128
)

# Generates Size x Size "big icon" pngs with a baked BLACK outline for the relic
# detail/inspect screen (NInspectRelicScreen.cs:400 uses RelicModel.BigIcon,
# and the Large branch of NRelic.Reload never shows the outline texture, so
# vanilla bakes the black edge into the big png artwork itself).
#
# v2 (2026-09-29, user: edges rough):
#   - chamfer 3-4 distance transform (approximates EUCLIDEAN distance, rounded
#     corners) instead of the old Chebyshev BFS (square corners)
#   - smoothstep alpha falloff instead of linear ramp
#   - work at -Work (default 512 = 2x target), downsample with HighQualityBicubic
#
# Pipeline per icon:
#   1. bicubic-upscale the 64x64 source icon to Work x Work
#   2. chamfer distance transform from the opaque mask
#   3. black rim: alpha = solid rim then smoothstep falloff, then
#      alpha-composite the icon on top (icon OVER outline)
#   4. downsample Work -> Size

Add-Type -AssemblyName System.Drawing

if (-not (Test-Path -LiteralPath $OutDir)) {
    New-Item -ItemType Directory -Path $OutDir | Out-Null
}

foreach ($name in $Names) {
    $srcPath = Join-Path $SrcDir ($name + '.png')
    $outPath = Join-Path $OutDir ($name + '.png')
    if (-not (Test-Path -LiteralPath $srcPath)) {
        Write-Output ("BIGICON|SKIP|{0}|src missing: {1}" -f $name, $srcPath)
        continue
    }

    $src = New-Object System.Drawing.Bitmap($srcPath)
    if ($src.Width -ne 64 -or $src.Height -ne 64) {
        Write-Output ("BIGICON|WARN|{0}|src is {1}x{2} (expected 64x64)" -f $name, $src.Width, $src.Height)
    }

    # 1. upscale to working resolution
    $workBmp = New-Object System.Drawing.Bitmap($Work, $Work, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb))
    $g = [System.Drawing.Graphics]::FromImage($workBmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.DrawImage($src, 0, 0, $Work, $Work)
    $g.Dispose()
    $src.Dispose()

    $n = $Work
    $n2 = $n * $n
    $rect = New-Object System.Drawing.Rectangle(0, 0, $n, $n)
    $data = $workBmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadWrite, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $bytes = New-Object byte[] ($n2 * 4)
    [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $bytes, 0, $bytes.Length)

    # 2. opaque mask + chamfer 3-4 distance transform (two passes, d/3 = px)
    $mask = [bool[]]::new($n2)
    for ($i = 0; $i -lt $n2; $i++) {
        if ($bytes[$i * 4 + 3] -ge $MaskAlphaMin) { $mask[$i] = $true }
    }
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

    # 3. black rim profile + composite icon OVER outline
    $scale = $Work / [double]$Size
    $solidPx = $SolidRim * $scale
    $outerPx = $Dilate * $scale
    for ($i = 0; $i -lt $n2; $i++) {
        $i4 = $i * 4
        $aI = $bytes[$i4 + 3] / 255.0
        $aO = 0.0
        if (-not $mask[$i]) {
            $dd = $dist[$i] / 3.0
            if ($dd -le $outerPx) {
                if ($dd -le $solidPx) {
                    $aO = 1.0
                } else {
                    $t = ($dd - $solidPx) / ($outerPx - $solidPx)
                    $aO = 1.0 - $t * $t * (3.0 - 2.0 * $t)   # smoothstep falloff
                }
            }
        }
        $outA = $aI + $aO * (1.0 - $aI)
        if ($outA -le 0.0) {
            $bytes[$i4] = 0; $bytes[$i4 + 1] = 0; $bytes[$i4 + 2] = 0; $bytes[$i4 + 3] = 0
            continue
        }
        $r = ($bytes[$i4 + 2] / 255.0) * $aI / $outA
        $gg = ($bytes[$i4 + 1] / 255.0) * $aI / $outA
        $b = ($bytes[$i4] / 255.0) * $aI / $outA
        $bytes[$i4] = [byte][math]::Round($b * 255.0)
        $bytes[$i4 + 1] = [byte][math]::Round($gg * 255.0)
        $bytes[$i4 + 2] = [byte][math]::Round($r * 255.0)
        $bytes[$i4 + 3] = [byte][math]::Round($outA * 255.0)
    }
    [System.Runtime.InteropServices.Marshal]::Copy($bytes, 0, $data.Scan0, $bytes.Length)
    $workBmp.UnlockBits($data)

    # 4. downsample to the target size (anti-aliasing happens here)
    $outBmp = New-Object System.Drawing.Bitmap($Size, $Size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb))
    $g2 = [System.Drawing.Graphics]::FromImage($outBmp)
    $g2.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g2.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g2.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g2.DrawImage($workBmp, 0, 0, $Size, $Size)
    $g2.Dispose()
    $workBmp.Dispose()
    $outBmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $outBmp.Dispose()

    Write-Output ("BIGICON|OK|{0}|{1}x{2}|work={3}|solidRim={4}|dilate={5}|{6} bytes" -f `
        $name, $Size, $Size, $Work, $SolidRim, $Dilate, (Get-Item -LiteralPath $outPath).Length)
}
