# hexdump.ps1 -- dump first/last N bytes of a file as hex
# ASCII only, no BOM. PS 5.1 compatible.
[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)][string]$Path,
  [int]$Head = 96,
  [int]$Tail = 0,
  [long]$Offset = 0
)
$fs = [System.IO.File]::OpenRead($Path)
$len = $fs.Length
Write-Host ("File : {0}" -f $Path)
Write-Host ("Size : {0:N0} (0x{1:X})" -f $len, $len)
if($Head -gt 0){
  $n = [Math]::Min($Head, [int]($len - $Offset))
  $fs.Seek($Offset,'Begin') | Out-Null
  $b = New-Object byte[] $n
  [void]$fs.Read($b,0,$n)
  Write-Host "--- head @ $Offset ---"
  for($i=0; $i -lt $n; $i+=16){
    $seg = $b[$i..([Math]::Min($i+15,$n-1))]
    $hex = ($seg | ForEach-Object { $_.ToString('X2') }) -join ' '
    $asc = -join ($seg | ForEach-Object { if($_ -ge 32 -and $_ -lt 127){[char]$_}else{'.'} })
    Write-Host ("{0:X8}  {1,-47}  {2}" -f ($Offset+$i), $hex, $asc)
  }
}
if($Tail -gt 0){
  $start = $len - $Tail
  $fs.Seek($start,'Begin') | Out-Null
  $b = New-Object byte[] $Tail
  [void]$fs.Read($b,0,$Tail)
  Write-Host "--- tail @ $start ---"
  for($i=0; $i -lt $Tail; $i+=16){
    $seg = $b[$i..([Math]::Min($i+15,$Tail-1))]
    $hex = ($seg | ForEach-Object { $_.ToString('X2') }) -join ' '
    $asc = -join ($seg | ForEach-Object { if($_ -ge 32 -and $_ -lt 127){[char]$_}else{'.'} })
    Write-Host ("{0:X8}  {1,-47}  {2}" -f ($start+$i), $hex, $asc)
  }
}
$fs.Close()
