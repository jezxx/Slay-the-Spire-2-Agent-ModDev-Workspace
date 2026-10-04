# Export a public STS2 mod resource project to a Godot PCK.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ProjectDir,
    [Parameter(Mandatory = $true)][string]$GameDir,
    [string]$ModId,
    [string]$GodotExe,
    [string]$Preset = 'Windows Desktop',
    [string]$OutFile,
    [switch]$Deploy
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$ProjectDir = Resolve-ExistingDirectory $ProjectDir 'Mod project directory'
$GameDir = Resolve-GameDirectory $GameDir
$ModId = Get-ModProjectId $ProjectDir $ModId
Assert-ModId $ModId
$GodotExe = Resolve-ToolCommand $GodotExe @('godot.exe','godot','Godot.exe') 'Godot'

if (-not (Test-Path -LiteralPath (Join-Path $ProjectDir 'project.godot') -PathType Leaf)) { throw "Missing project.godot in $ProjectDir" }
if (-not (Test-Path -LiteralPath (Join-Path $ProjectDir 'export_presets.cfg') -PathType Leaf)) { throw "Missing export_presets.cfg in $ProjectDir" }
if (-not $OutFile) { $OutFile = Join-Path $ProjectDir "Build\$ModId.pck" }
$OutFile = [IO.Path]::GetFullPath($OutFile)
New-Item -ItemType Directory -Force -Path (Split-Path $OutFile -Parent) | Out-Null

Write-Host "[sts2-workspace] exporting PCK: $ModId"
& $GodotExe --headless --path $ProjectDir --export-pack $Preset $OutFile
if ($LASTEXITCODE -ne 0) { throw "Godot PCK export failed (exit $LASTEXITCODE)" }
if (-not (Test-Path -LiteralPath $OutFile -PathType Leaf)) { throw "Godot reported success but did not create: $OutFile" }
Write-Host "[sts2-workspace] PCK: $OutFile ($((Get-Item $OutFile).Length) bytes)"

if ($Deploy) {
    $deployRoot = Join-Path $GameDir "mods\$ModId"
    New-Item -ItemType Directory -Force -Path $deployRoot | Out-Null
    Copy-Item -LiteralPath $OutFile -Destination (Join-Path $deployRoot "$ModId.pck") -Force
    Write-Host "[sts2-workspace] deployed: $deployRoot\$ModId.pck" -ForegroundColor Green
}
