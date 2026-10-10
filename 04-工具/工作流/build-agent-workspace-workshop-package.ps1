# Build a source-free Workshop resource package for this public Agent workspace.
[CmdletBinding()]
param(
 [Parameter(Mandatory = $true)][string]$WorkspaceDir,
 [Parameter(Mandatory = $true)][string]$OutputDir,
 [string]$PreviewImagePath
)
$ErrorActionPreference = 'Stop'
$WorkspaceDir = (Resolve-Path -LiteralPath $WorkspaceDir).Path
$OutputDir = [IO.Path]::GetFullPath($OutputDir)
if ($WorkspaceDir.TrimEnd('\') -eq $OutputDir.TrimEnd('\')) { throw 'OutputDir must be outside WorkspaceDir.' }
if (Test-Path -LiteralPath $OutputDir) { Remove-Item -LiteralPath $OutputDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path (Join-Path $OutputDir 'content') | Out-Null
$staging = Join-Path $OutputDir '_workspace'
New-Item -ItemType Directory -Force -Path $staging | Out-Null
$exclude = @('.git','.build-tmp','.dotnet-home','bin','obj','Build','.godot','.work','.preview','.backup','__pycache__')
$excludeExtensions = @('.pck','.dll','.pdb','.log','.tmp','.bak','.orig')
Get-ChildItem -LiteralPath $WorkspaceDir -Recurse -Force | Where-Object {
 $relative = $_.FullName.Substring($WorkspaceDir.Length).TrimStart('\')
 $parts = $relative -split '[\\/]'
 -not ($parts | Where-Object { $exclude -contains $_ }) -and
 ($_.PSIsContainer -or $excludeExtensions -notcontains $_.Extension.ToLowerInvariant())
} | ForEach-Object {
 $relative = $_.FullName.Substring($WorkspaceDir.Length).TrimStart('\')
 $target = Join-Path $staging $relative
 if ($_.PSIsContainer) { New-Item -ItemType Directory -Force -Path $target | Out-Null }
 else { New-Item -ItemType Directory -Force -Path (Split-Path $target) | Out-Null; Copy-Item -LiteralPath $_.FullName -Destination $target -Force }
}
$manifest = [ordered]@{ id='sts2_agent_moddev_workspace'; name='STS2 Agent ModDev Workspace'; author='jezxx'; description='Agent 模组制作资料工作区；不是可加载的玩法模组。'; version='1.0.0'; min_game_version='0.111.0'; has_pck=$false; has_dll=$false; dependencies=@(); affects_gameplay=$false }
$manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $OutputDir 'content/sts2_agent_moddev_workspace.json') -Encoding UTF8
Compress-Archive -Path (Join-Path $staging '*') -DestinationPath (Join-Path $OutputDir 'content/workspace.zip') -CompressionLevel Optimal
Remove-Item -LiteralPath $staging -Recurse -Force
if ($PreviewImagePath) { Copy-Item -LiteralPath $PreviewImagePath -Destination (Join-Path $OutputDir 'image.png') -Force }
if (-not (Test-Path -LiteralPath (Join-Path $OutputDir 'image.png'))) { Set-Content -LiteralPath (Join-Path $OutputDir 'image-placeholder.txt') -Value '请替换为小于 1 MB 的正式 image.png。' -Encoding UTF8; Write-Warning 'image.png is missing.' }
Write-Host "Workshop resource package ready: $OutputDir"
