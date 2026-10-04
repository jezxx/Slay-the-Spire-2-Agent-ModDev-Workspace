# Verify a mod from the game log. This checks actual loading, not file existence.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ModId,
    [string]$LogPath,
    [switch]$Errors
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
Assert-ModId $ModId
$log = Get-GameLogPath $LogPath
if (-not (Test-Path -LiteralPath $log -PathType Leaf)) { throw "Game log was not found: $log" }
$lines = [IO.File]::ReadAllLines($log)
$escaped = [regex]::Escape($ModId)
$hits = @($lines | Select-String -Pattern $escaped)
$finished = @($lines | Select-String -Pattern "Finished mod initialization for.*$escaped")
$loading = @($lines | Select-String -Pattern "Loading (assembly DLL|Godot PCK).*$escaped")

Write-Host "[sts2-workspace] log: $log"
Write-Host "[sts2-workspace] lines: $($lines.Count)"
Write-Host '=== loading evidence ==='
$loading | Select-Object -Last 20 | ForEach-Object { Write-Host $_.Line }
Write-Host '=== initialization evidence ==='
$finished | Select-Object -Last 20 | ForEach-Object { Write-Host $_.Line }
if ($Errors) {
    Write-Host '=== recent errors ==='
    $lines | Select-String -Pattern 'ERROR|Exception|failed|Failed|error' | Select-Object -Last 40 | ForEach-Object { Write-Host $_.Line }
}
if ($finished.Count -gt 0) { Write-Host "VERIFY PASS: $ModId reached Finished mod initialization." -ForegroundColor Green; exit 0 }
if ($hits.Count -gt 0) { Write-Warning "The log mentions $ModId, but no completed initialization line was found." }
else { Write-Warning "No log line mentions $ModId." }
exit 1
