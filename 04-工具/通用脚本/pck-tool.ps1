# pck-tool.ps1 -- list / extract Godot 4.x .pck resources
# ASCII only, no BOM. Windows PowerShell 5.1 compatible.
#   .\pck-tool.ps1 -Pck <path.pck> -List -Match "card_portraits" -Max 100
#   .\pck-tool.ps1 -Pck <path.pck> -Extract -Match "relic" -Out "<output directory>"
[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)][string]$Pck,
  [switch]$List,
  [switch]$Extract,
  [string]$Match = "",
  [string]$Out = (Join-Path (Get-Location) 'pck-out'),
  [int]$Max = 200,
  [switch]$Manifest   # write full path+size listing to a .txt next to $Out
)

$ErrorActionPreference = 'Stop'
$src = Join-Path $PSScriptRoot 'GdPck.cs'
Add-Type -Path $src

Write-Host "Pck : $Pck"
Write-Host ("Size: {0:N0} bytes" -f (Get-Item $Pck).Length)
$entries = [GdPck]::Read($Pck)
Write-Host ("Entries: {0:N0}" -f $entries.Count)

# Keep the concrete generic type: PowerShell's Where-Object output is Object[]
# and cannot be passed straight to a typed C# method.
$sel = New-Object 'System.Collections.Generic.List[PckEntry]'
foreach($e in $entries){
  if($Match -eq "" -or $e.Path -match $Match){ $sel.Add($e) }
}
Write-Host ("Matched: {0:N0}" -f $sel.Count)

if($List){
  $sel | Select-Object -First $Max | ForEach-Object { Write-Host $_.ToString() }
}

if($Manifest){
  if(-not (Test-Path $Out)){ New-Item -ItemType Directory -Path $Out -Force | Out-Null }
  $mf = Join-Path $Out 'pck-manifest.txt'
  $sel | ForEach-Object { "{0}`t{1}" -f $_.Size, $_.Path } | Set-Content -Path $mf -Encoding UTF8
  Write-Host "Manifest -> $mf"
}

if($Extract){
  [GdPck]::Extract($Pck, $sel, $Out)
  Write-Host "Extracted $($sel.Count) files -> $Out"
}
