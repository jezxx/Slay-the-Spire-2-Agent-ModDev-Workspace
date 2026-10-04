# Prepare a source-free local package for a manual Steam Workshop upload.
# This script never uploads and never handles credentials.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ProjectDir,
    [Parameter(Mandatory = $true)][string]$OutputDir,
    [string]$ModId,
    [string]$DllPath,
    [string]$PckPath
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$ProjectDir = Resolve-ExistingDirectory $ProjectDir 'Mod project directory'
$ModId = Get-ModProjectId $ProjectDir $ModId
Assert-ModId $ModId
$manifestPath = Join-Path $ProjectDir "$ModId.json"
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) { throw "Manifest not found: $manifestPath" }
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.id -ne $ModId) { throw "Manifest id '$($manifest.id)' does not match '$ModId'." }

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$OutputDir = (Resolve-Path -LiteralPath $OutputDir).Path
Copy-Item -LiteralPath $manifestPath -Destination (Join-Path $OutputDir "$ModId.json") -Force
if ($manifest.has_dll -eq $true) {
    if (-not $DllPath) { $DllPath = Join-Path $ProjectDir "bin\Release\net9.0\$ModId.dll" }
    if (-not (Test-Path -LiteralPath $DllPath -PathType Leaf)) { throw "Manifest requires DLL but it was not found: $DllPath" }
    Copy-Item -LiteralPath $DllPath -Destination (Join-Path $OutputDir "$ModId.dll") -Force
}
if ($manifest.has_pck -eq $true) {
    if (-not $PckPath) { $PckPath = Join-Path $ProjectDir "Build\$ModId.pck" }
    if (-not (Test-Path -LiteralPath $PckPath -PathType Leaf)) { throw "Manifest requires PCK but it was not found: $PckPath" }
    Copy-Item -LiteralPath $PckPath -Destination (Join-Path $OutputDir "$ModId.pck") -Force
}
$sourceLike = Get-ChildItem -LiteralPath $OutputDir -Recurse -File | Where-Object { $_.Extension -in @('.cs','.csproj','.godot','.cfg','.sln') }
if ($sourceLike) { throw 'Source files unexpectedly entered the workshop staging directory.' }
$files = @(Get-ChildItem -LiteralPath $OutputDir -Recurse -File)
Write-Host "[sts2-workspace] workshop package ready: $OutputDir"
Write-Host "[sts2-workspace] files: $($files.Count)"
