param(
    [Parameter(Mandatory=$true)][string]$Marker,
    [string]$From = '2EAD7A',
    [string]$To   = '5EEAD4',
    [switch]$DryRun
)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$f=$From.TrimStart('#'); $t=$To.TrimStart('#')
if($f.Length -ne 6 -or $t.Length -ne 6){throw 'hex must be 6 digits'}
$fr=[Convert]::ToInt32($f.Substring(0,2),16); $fg=[Convert]::ToInt32($f.Substring(2,2),16); $fb=[Convert]::ToInt32($f.Substring(4,2),16)
$tr=[Convert]::ToInt32($t.Substring(0,2),16); $tg=[Convert]::ToInt32($t.Substring(2,2),16); $tb=[Convert]::ToInt32($t.Substring(4,2),16)
"recolor #$f -> #$t"

$bmp=[System.Drawing.Bitmap]::new($Marker)
try{
  $W=$bmp.Width;$H=$bmp.Height
  $changed=0; $skippedA0=0
  $alphas=@{}
  $out=[System.Drawing.Bitmap]::new($W,$H,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  try{
    for($y=0;$y -lt $H;$y++){
      for($x=0;$x -lt $W;$x++){
        $c=$bmp.GetPixel($x,$y)
        if($c.R -eq $fr -and $c.G -eq $fg -and $c.B -eq $fb){
          if($c.A -eq 0){ $skippedA0++ }
          else {
            $changed++
            $ak="A$($c.A)"; if($alphas.ContainsKey($ak)){$alphas[$ak]++}else{$alphas[$ak]=1}
          }
          $out.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($c.A,$tr,$tg,$tb))
        } else {
          $out.SetPixel($x,$y,$c)
        }
      }
    }
    "  matched pixels      : $changed"
    "  skipped (A==0)      : $skippedA0"
    "  distinct alpha kept : $($alphas.Count)"
    if(-not $DryRun){
      $tmp="$Marker.recolor.tmp.png"
      $out.Save($tmp,[System.Drawing.Imaging.ImageFormat]::Png)
    }
  } finally { $out.Dispose() }
} finally { $bmp.Dispose() }

if(-not $DryRun){
  Copy-Item -LiteralPath "$Marker.recolor.tmp.png" -Destination $Marker -Force
  Remove-Item -LiteralPath "$Marker.recolor.tmp.png" -Force
  "  written: $Marker"
}
