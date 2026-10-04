# Full public development loop: build, export resources, deploy, restart, verify.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ModId,
    [Parameter(Mandatory = $true)][string]$GameDir,
    [string]$ProjectDir,
    [string]$DotnetExe = 'dotnet',
    [string]$GodotExe,
    [string]$TempRoot,
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [switch]$SkipPack,
    [switch]$NoRestart,
    [switch]$Rebuild,
    [int]$TimeoutSec = 240
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$GameDir = Resolve-GameDirectory $GameDir
Assert-ModId $ModId
if (-not $ProjectDir) { $ProjectDir = Join-Path $WorkspaceRoot "09-用户项目区\$ModId" }
$ProjectDir = Resolve-ExistingDirectory $ProjectDir 'Mod project directory'

$build = Join-Path $PSScriptRoot 'build-native-mod.ps1'
& $build -ProjectDir $ProjectDir -GameDir $GameDir -ModId $ModId -DotnetExe $DotnetExe -TempRoot $TempRoot -Configuration $Configuration -Deploy $true -Rebuild:$Rebuild
if ($LASTEXITCODE -ne 0) { throw 'Native build/deploy failed.' }

$resourceRoot = Join-Path $ProjectDir $ModId
if (-not $SkipPack -and (Test-Path -LiteralPath $resourceRoot -PathType Container)) {
    $pack = Join-Path $PSScriptRoot 'pack-pck.ps1'
    & $pack -ProjectDir $ProjectDir -GameDir $GameDir -ModId $ModId -GodotExe $GodotExe -Deploy
    if ($LASTEXITCODE -ne 0) { throw 'PCK export/deploy failed.' }
} elseif ($SkipPack) {
    Write-Warning 'Skipped PCK export. Use this only when no resource or localization file changed.'
}

if ($NoRestart) { Write-Host '[sts2-workspace] build/deploy complete; restart skipped.'; exit 0 }
$restart = Join-Path $PSScriptRoot 'restart-and-verify.ps1'
& $restart -ModId $ModId -GameDir $GameDir -TimeoutSec $TimeoutSec
if ($LASTEXITCODE -ne 0) { throw 'Restart or runtime verification failed.' }
