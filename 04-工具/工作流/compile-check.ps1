# Compile a mod without deploying files or stopping the game.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ProjectDir,
    [Parameter(Mandatory = $true)][string]$GameDir,
    [string]$ModId,
    [string]$DotnetExe = 'dotnet',
    [string]$TempRoot,
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [switch]$Rebuild
)

$ErrorActionPreference = 'Stop'
$build = Join-Path $PSScriptRoot 'build-native-mod.ps1'
& $build -ProjectDir $ProjectDir -GameDir $GameDir -ModId $ModId -DotnetExe $DotnetExe -TempRoot $TempRoot -Configuration $Configuration -NoDeploy -Rebuild:$Rebuild
if ($LASTEXITCODE -ne 0) { throw 'Compile check failed.' }
