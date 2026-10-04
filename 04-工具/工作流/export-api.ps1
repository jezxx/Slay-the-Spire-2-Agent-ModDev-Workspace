# Export a versioned offline API index from the current game's sts2.dll.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$GameDir,
    [string]$DotnetExe = 'dotnet',
    [string]$OutputDir,
    [string]$TempRoot,
    [string[]]$Pattern,
    [string]$Name = 'custom'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
Set-PublicDotnetEnvironment $TempRoot
$GameDir = Resolve-GameDirectory $GameDir
$data = Get-GameDataDirectory $GameDir
$probeProject = Join-Path $WorkspaceRoot '04-工具\查询工具源码\ApiProbe\ApiProbe.csproj'
$probeOut = Join-Path $WorkspaceRoot '.tools\ApiProbe'
$probeBuildExit = Invoke-PublicDotnet $DotnetExe @('build', $probeProject, '-c', 'Release', '-o', $probeOut)
if ($probeBuildExit -ne 0) { throw "ApiProbe build failed (exit $probeBuildExit)." }
$probe = Join-Path $probeOut 'ApiProbe.dll'
if (-not $OutputDir) {
    $versionFile = Join-Path $GameDir 'release_info.json'
    $version = 'current'
    if (Test-Path $versionFile) { try { $info = Get-Content $versionFile -Raw | ConvertFrom-Json; $version = ($info.version -replace '^v','' -replace '[^A-Za-z0-9._-]','_') } catch {} }
    $OutputDir = Join-Path $WorkspaceRoot "02-游戏反编译资料\版本-$version\API索引\generated"
}
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$jobs = [ordered]@{
    '00-Index'=@('^MegaCrit\.Sts2\.Core\.Modding\.')
    '10-Models'=@('^MegaCrit\.Sts2\.Core\.Models\.')
    '20-Commands'=@('^MegaCrit\.Sts2\.Core\.Commands\.')
    '30-Entities'=@('^MegaCrit\.Sts2\.Core\.Entities\.')
    '40-Localization'=@('^MegaCrit\.Sts2\.Core\.Localization\.')
    '50-ValueProps'=@('^MegaCrit\.Sts2\.Core\.ValueProps\.')
    '60-Hooks'=@('^MegaCrit\.Sts2\.Core\.Hooks\.')
    '70-GameActions'=@('^MegaCrit\.Sts2\.Core\.GameActions\.')
}
if ($Pattern) { $jobs = [ordered]@{ $Name = $Pattern } }
foreach($job in $jobs.GetEnumerator()) {
    $out = Join-Path $OutputDir ($job.Key + '.txt')
    $args = @($data,'sts2.dll',$out,'types') + @($job.Value)
    $exportExit = Invoke-PublicDotnet $DotnetExe (@($probe) + $args)
    if ($exportExit -ne 0) { throw "API export failed: $($job.Key) (exit $exportExit)" }
}
Write-Host "[sts2-workspace] API index written to $OutputDir" -ForegroundColor Green
