# 从公开工作区模板创建一个新的原生 C# 模组工程
#
# 用法：
#   .\new-mod.ps1 -ModId MyFirstMod
#   .\new-mod.ps1 -ModId MyFirstMod -ModName "我的第一个模组" -Author "作者"
#
# 默认输出到：<工作区>\09-用户项目区\<ModId>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]*$')]
    [string]$ModId,

    [string]$ModName,
    [string]$Author = '作者',
    [string]$Description,
    [string]$MinGameVersion = '0.111.0',
    [string]$OutRoot,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$packageRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if (-not $ModName) { $ModName = $ModId }
if (-not $Description) { $Description = "$ModName 模组" }
if (-not $OutRoot) { $OutRoot = Join-Path $packageRoot '09-用户项目区' }

$template = Join-Path $packageRoot '05-模板\原生模组模板'
$destination = Join-Path $OutRoot $ModId
if (-not (Test-Path $template)) { throw "找不到模板目录：$template" }
if (Test-Path $destination) {
    if (-not $Force) { throw "目录已存在：$destination（加 -Force 覆盖）" }
    Remove-Item -LiteralPath $destination -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $OutRoot | Out-Null
Copy-Item -LiteralPath $template -Destination $destination -Recurse -Force

$utf8Bom = New-Object System.Text.UTF8Encoding($true)
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
function Read-Utf8([string]$Path) {
    return [System.IO.File]::ReadAllText($Path, $utf8NoBom)
}
function Write-Utf8Bom([string]$Path, [string]$Text) {
    [System.IO.File]::WriteAllText($Path, $Text, $utf8Bom)
}

$csproj = Join-Path $destination 'ModTemplate.csproj'
$manifestTemplate = Join-Path $destination 'ModTemplate.json'
Rename-Item -LiteralPath $csproj -NewName "$ModId.csproj"
Rename-Item -LiteralPath $manifestTemplate -NewName "$ModId.json"

Get-ChildItem -LiteralPath $destination -Recurse -File | ForEach-Object {
    if ($_.Extension -notin @('.cs', '.csproj', '.godot', '.cfg')) { return }
    $text = Read-Utf8 $_.FullName
    $text = $text -replace '__MODID__', $ModId
    $text = $text -replace 'ModTemplate\.Scripts\.Cards', "$ModId.Scripts.Cards"
    $text = $text -replace 'ModTemplate\.Scripts', "$ModId.Scripts"
    $text = $text -replace 'ModTemplate', $ModId
    if ($_.Extension -in @('.godot', '.cfg')) {
        [System.IO.File]::WriteAllText($_.FullName, $text, $utf8NoBom)
    }
    else {
        Write-Utf8Bom $_.FullName $text
    }
}

$manifestPath = Join-Path $destination "$ModId.json"
$manifest = Read-Utf8 $manifestPath | ConvertFrom-Json
$manifest.id = $ModId
$manifest.name = $ModName
$manifest.author = $Author
$manifest.description = $Description
$manifest.min_game_version = $MinGameVersion
Write-Utf8Bom $manifestPath ($manifest | ConvertTo-Json -Depth 6)

# 防止 Godot 把 C# 源码当作需要导出的脚本资源；构建仍由 dotnet 完成。
New-Item -ItemType Directory -Force -Path (Join-Path $destination 'Scripts') | Out-Null
[System.IO.File]::WriteAllText((Join-Path $destination 'Scripts\.gdignore'), '', $utf8NoBom)
New-Item -ItemType Directory -Force -Path (Join-Path $destination 'Build') | Out-Null

$localization = Join-Path $destination "$ModId\localization\zhs"
New-Item -ItemType Directory -Force -Path $localization | Out-Null
$cardsJson = @{ 'EXAMPLE_CARD.title' = '示例卡牌'; 'EXAMPLE_CARD.description' = '造成{Damage:diff()}点伤害。' } | ConvertTo-Json
Write-Utf8Bom (Join-Path $localization 'cards.json') $cardsJson

$projectDoc = @(
    "# $ModName",
    '',
    "- 模组 ID：$ModId",
    "- 作者：$Author",
    "- 目标游戏版本：$MinGameVersion",
    '- 当前阶段：原型',
    '',
    '## 目标',
    '',
    $Description,
    '',
    '## 验证记录',
    '',
    '- 编译：尚未验证',
    '- 游戏内运行：尚未验证',
    '- 需要用户确认：首次启动、资源加载和版本兼容性'
) -join [Environment]::NewLine
Write-Utf8Bom (Join-Path $destination 'PROJECT.md') $projectDoc
New-Item -ItemType Directory -Force -Path (Join-Path $destination '任务记录'), (Join-Path $destination '决策记录'), (Join-Path $destination '测试记录') | Out-Null

Write-Host "[sts2-workspace] 已创建模组工程：$destination" -ForegroundColor Green
Write-Host '下一步：'
Write-Host ('  .\build-native-mod.ps1 -ProjectDir "{0}" -GameDir "<游戏目录>"' -f $destination)
