# Restart the game through Steam and wait for a real mod initialization line.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ModId,
    [Parameter(Mandatory = $true)][string]$GameDir,
    [string]$SteamExe,
    [string]$AppId = '2868840',
    [string]$LogPath,
    [int]$TimeoutSec = 240,
    [switch]$NoLaunch
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$GameDir = Resolve-GameDirectory $GameDir
Assert-ModId $ModId
$log = Get-GameLogPath $LogPath
$beforeCount = if (Test-Path -LiteralPath $log) { ([IO.File]::ReadAllLines($log)).Count } else { 0 }
$game = Get-Process 'SlayTheSpire2' -ErrorAction SilentlyContinue
if ($game) {
    Write-Host "[sts2-workspace] stopping game process: $($game.Id -join ', ')"
    $game | Stop-Process -Force
    for ($i=0; $i -lt 30 -and (Get-Process 'SlayTheSpire2' -ErrorAction SilentlyContinue); $i++) { Start-Sleep -Milliseconds 500 }
}
Start-Sleep -Seconds 2
if ($NoLaunch) { Write-Host '[sts2-workspace] -NoLaunch: stopped only.'; exit 0 }
$SteamExe = Resolve-ToolCommand $SteamExe @('steam.exe') 'Steam'
Write-Host "[sts2-workspace] launching Steam app $AppId"
Start-Process -FilePath $SteamExe -ArgumentList '-applaunch', $AppId | Out-Null
$deadline = (Get-Date).AddSeconds($TimeoutSec)
$pattern = "Finished mod initialization for.*$([regex]::Escape($ModId))"
while ((Get-Date) -lt $deadline) {
    if (Test-Path -LiteralPath $log) {
        $now = [IO.File]::ReadAllLines($log)
        $newLines = if ($now.Count -gt $beforeCount) { $now[$beforeCount..($now.Count - 1)] } else { $now }
        $match = $newLines | Select-String -Pattern $pattern
        if ($match) { Write-Host 'VERIFY PASS: mod initialization completed.' -ForegroundColor Green; $match | Select-Object -Last 5 | ForEach-Object { Write-Host $_.Line }; exit 0 }
    }
    Start-Sleep -Seconds 2
}
throw "Timed out after $TimeoutSec seconds waiting for $ModId initialization. Check: $log"
