# 编译并可选部署一个公开工作区中的原生 C# 模组
#
# 用法：
#   .\build-native-mod.ps1 -ProjectDir <模组目录> -GameDir <游戏目录>
#   .\build-native-mod.ps1 -ProjectDir <目录> -GameDir <游戏目录> -Deploy false
#
# 这个脚本只负责原生 C# 模组的 DLL、清单和资源目录。
# PCK、Godot 导出和第三方框架需要按照对应教程单独处理。

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectDir,

    [Parameter(Mandatory = $true)]
    [string]$GameDir,

    [string]$ModId,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$DotnetExe = 'dotnet',
    [string]$Sts2DataDir,
    [bool]$Deploy = $true,
    [switch]$Rebuild
)

$ErrorActionPreference = 'Stop'

# NuGet 可能在系统临时目录留下无法访问的 NuGetScratch 锁文件。
# 使用包内缓存目录可以避免把一次残留锁误判成 SDK、工程或游戏引用错误。
$packageRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$scratchRoot = Join-Path $packageRoot '.build-tmp'
New-Item -ItemType Directory -Force -Path $scratchRoot | Out-Null
$env:TEMP = $scratchRoot
$env:TMP = $scratchRoot

function Resolve-ExistingDirectory([string]$Path, [string]$Label) {
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "$Label 不存在：$Path"
    }
    return (Resolve-Path -LiteralPath $Path).Path
}

$ProjectDir = Resolve-ExistingDirectory $ProjectDir '模组工程目录'
$GameDir = Resolve-ExistingDirectory $GameDir '游戏目录'
if (-not $Sts2DataDir) {
    $Sts2DataDir = Join-Path $GameDir 'data_sts2_windows_x86_64'
}
$Sts2DataDir = Resolve-ExistingDirectory $Sts2DataDir '游戏数据目录'

$csproj = Get-ChildItem -LiteralPath $ProjectDir -Filter *.csproj -File | Select-Object -First 1
if (-not $csproj) { throw "在 $ProjectDir 中找不到 .csproj 文件" }
if (-not $ModId) { $ModId = [IO.Path]::GetFileNameWithoutExtension($csproj.Name) }
if ($ModId -notmatch '^[A-Za-z][A-Za-z0-9_]*$') { throw "ModId 不符合规则：$ModId" }

$manifest = Join-Path $ProjectDir "$ModId.json"
if (-not (Test-Path -LiteralPath $manifest -PathType Leaf)) {
    throw "找不到与工程同名的清单：$manifest"
}

try {
    $null = Get-Command $DotnetExe -ErrorAction Stop
}
catch {
    throw "找不到 .NET CLI：$DotnetExe。请先安装 .NET SDK，或用 -DotnetExe 提供完整路径。"
}

$gameProcess = Get-Process 'SlayTheSpire2' -ErrorAction SilentlyContinue
if ($Deploy -and $gameProcess) {
    $ids = ($gameProcess | Select-Object -ExpandProperty Id) -join ', '
    throw "游戏正在运行（PID $ids）。请先关闭游戏，或使用 -Deploy false 只编译。"
}

$buildArgs = @(
    'build', $csproj.FullName,
    '--configuration', $Configuration,
    "-p:Sts2Dir=$GameDir",
    "-p:Sts2DataDir=$Sts2DataDir",
    '-p:CopyModOnBuild=false'
)
if ($Rebuild) { $buildArgs += '--no-incremental' }

Write-Host "[sts2-workspace] 工程：$($csproj.Name)"
Write-Host "[sts2-workspace] ModId：$ModId"
Write-Host "[sts2-workspace] 游戏数据：$Sts2DataDir"
Write-Host '[sts2-workspace] 编译中……'
& $DotnetExe @buildArgs
if ($LASTEXITCODE -ne 0) { throw "编译失败（exit $LASTEXITCODE）" }

$targetPath = $null
$propertyOutput = & $DotnetExe msbuild $csproj.FullName '-getProperty:TargetPath' "-p:Configuration=$Configuration" "-p:Sts2Dir=$GameDir" "-p:Sts2DataDir=$Sts2DataDir" 2>$null
if ($LASTEXITCODE -eq 0) {
    $targetPath = $propertyOutput | Where-Object { $_ -match '\.(dll|DLL)\s*$' } | Select-Object -Last 1
    if ($targetPath) { $targetPath = $targetPath.Trim().Trim('"') }
}
if (-not $targetPath -or -not (Test-Path -LiteralPath $targetPath -PathType Leaf)) {
    $targetPath = Join-Path $ProjectDir "bin\$Configuration\net9.0\$ModId.dll"
}
if (-not (Test-Path -LiteralPath $targetPath -PathType Leaf)) {
    throw "编译成功但找不到 DLL：$targetPath"
}

$dll = Get-Item -LiteralPath $targetPath
$newestSource = Get-ChildItem -LiteralPath $ProjectDir -Recurse -File -Include *.cs | Where-Object {
    $_.FullName -notmatch '\\(bin|obj|\.godot)\\'
} | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($newestSource -and $dll.LastWriteTime -lt $newestSource.LastWriteTime) {
    throw "DLL 早于最近修改的源码，可能没有真正重编译：$($dll.FullName)"
}
Write-Host "[sts2-workspace] 编译产物：$($dll.FullName)"

if (-not $Deploy) {
    Write-Host '[sts2-workspace] 已完成编译，按 -Deploy false 未部署。'
    exit 0
}

$modsRoot = Join-Path $GameDir 'mods'
$deployRoot = Join-Path $modsRoot $ModId
New-Item -ItemType Directory -Force -Path $deployRoot | Out-Null
Copy-Item -LiteralPath $dll.FullName -Destination (Join-Path $deployRoot "$ModId.dll") -Force
Copy-Item -LiteralPath $manifest -Destination (Join-Path $deployRoot "$ModId.json") -Force

# 模板资源位于 <工程>\<ModId>，游戏模组根目录需要直接看到 localization、assets 等目录。
$resourceRoot = Join-Path $ProjectDir $ModId
if (Test-Path -LiteralPath $resourceRoot -PathType Container) {
    Get-ChildItem -LiteralPath $resourceRoot -Recurse -File | ForEach-Object {
        $relative = $_.FullName.Substring($resourceRoot.Length).TrimStart('\')
        $destination = Join-Path $deployRoot $relative
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $destination -Force
    }
}

Write-Host "[sts2-workspace] 已部署：$deployRoot" -ForegroundColor Green
Get-ChildItem -LiteralPath $deployRoot -Recurse -File | Sort-Object FullName | ForEach-Object {
    Write-Host ('  {0}' -f $_.FullName.Substring($deployRoot.Length + 1))
}
