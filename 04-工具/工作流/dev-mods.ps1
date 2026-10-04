# Temporarily disable or restore the Steam Workshop content directory.
# Use this only for local testing; the script does not delete workshop data.

[CmdletBinding(DefaultParameterSetName='Status')]
param(
    [Parameter(ParameterSetName='Off')][switch]$Off,
    [Parameter(ParameterSetName='On')][switch]$On,
    [Parameter(ParameterSetName='Status')][switch]$Status,
    [Parameter(Mandatory=$true)][string]$GameDir,
    [string]$WorkshopRoot,
    [string]$AppId = '2868840'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$GameDir = Resolve-GameDirectory $GameDir
if (-not $WorkshopRoot) {
    $common = Split-Path (Split-Path (Split-Path $GameDir -Parent) -Parent) -Parent
    $WorkshopRoot = Join-Path $common "steamapps\workshop\content\$AppId"
}
$WorkshopRoot = [IO.Path]::GetFullPath($WorkshopRoot)
$disabled = "$WorkshopRoot.__disabled"
function Show-Status {
    if (Test-Path -LiteralPath $WorkshopRoot) { Write-Host "Workshop content enabled: $WorkshopRoot" }
    elseif (Test-Path -LiteralPath $disabled) { Write-Host "Workshop content disabled: $disabled" }
    else { Write-Host "Workshop content directory not found: $WorkshopRoot" }
}
if ($Status -or (-not $Off -and -not $On)) { Show-Status; exit 0 }
if (Get-Process 'SlayTheSpire2' -ErrorAction SilentlyContinue) { throw 'Close the game before changing the workshop directory.' }
if ($Off) {
    if (Test-Path -LiteralPath $disabled) { Write-Host 'Already disabled.'; exit 0 }
    if (-not (Test-Path -LiteralPath $WorkshopRoot)) { Write-Host 'Directory is already absent.'; exit 0 }
    Move-Item -LiteralPath $WorkshopRoot -Destination $disabled
    Write-Host "Workshop content disabled: $disabled"
    exit 0
}
if ($On) {
    if (-not (Test-Path -LiteralPath $disabled)) { Write-Host 'No disabled directory found.'; exit 0 }
    if (Test-Path -LiteralPath $WorkshopRoot) { throw "Cannot restore because destination exists: $WorkshopRoot" }
    Move-Item -LiteralPath $disabled -Destination $WorkshopRoot
    Write-Host "Workshop content restored: $WorkshopRoot"
    exit 0
}
