# Build all public query-tool source projects.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GameDir,
    [string]$DotnetExe = 'dotnet',
    [string]$OutputRoot,
    [string]$TempRoot,
    [switch]$NoRestore
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
Set-PublicDotnetEnvironment $TempRoot
$GameDir = Resolve-GameDirectory $GameDir
$Sts2DataDir = Get-GameDataDirectory $GameDir
$sourceRoot = Join-Path $WorkspaceRoot '04-工具\查询工具源码'
if (-not $OutputRoot) { $OutputRoot = Join-Path $WorkspaceRoot '.tools\bin' }
New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null
$projects = @(Get-ChildItem -LiteralPath $sourceRoot -Filter '*.csproj' -Recurse -File | Sort-Object FullName)
if ($projects.Count -eq 0) { throw "No query-tool projects found under $sourceRoot" }
foreach ($project in $projects) {
    $name = [IO.Path]::GetFileNameWithoutExtension($project.Name)
    $out = Join-Path $OutputRoot $name
    $args = @('build', $project.FullName, '-c', 'Release', '-o', $out, "-p:Sts2Dir=$GameDir", "-p:Sts2DataDir=$Sts2DataDir")
    if ($NoRestore) { $args += '--no-restore' }
    Write-Host "[sts2-workspace] building $name"
    $dotnetExit = Invoke-PublicDotnet $DotnetExe $args
    if ($dotnetExit -ne 0) { throw "Query tool build failed: $name (exit $dotnetExit)" }
}
Write-Host "[sts2-workspace] query tools built under $OutputRoot" -ForegroundColor Green
