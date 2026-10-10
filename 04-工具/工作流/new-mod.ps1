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
    '- 项目类型：原生 C# 模组',
    '- 当前阶段：原型',
    '',
    '## 目标',
    '',
    $Description,
    '',
    '## 已完成',
    '',
    '- 已生成原生模组基础工程',
    '',
    '## 正在处理',
    '',
    '- 首次编译与运行验证',
    '',
    '## 已知问题',
    '',
    '- 尚未完成首次实机验证',
    '',
    '## 项目专用约定',
    '',
    '- 暂无',
    '',
    '## 常见指代',
    '',
    "- 项目简称：$ModName / $ModId",
    '- 可复用实现关键词：按后续实际内容补充',
    '',
    '## 证据入口',
    '',
    '- 源码：Scripts\',
    "- 本地化与资源：$ModId\",
    '- 任务、决策与测试记录：按需使用同名目录',
    '',
    '## 工具与数据位置',
    '',
    '- 构建与部署：工作区 04-工具\工作流\',
    '- 项目专用工具：尚无',
    '',
    '## 验证记录',
    '',
    '- 编译：尚未验证',
    '- 游戏内运行：尚未验证',
    '- 需要用户确认：首次启动、资源加载和版本兼容性'
) -join [Environment]::NewLine
Write-Utf8Bom (Join-Path $destination 'PROJECT.md') $projectDoc
New-Item -ItemType Directory -Force -Path (Join-Path $destination '任务记录'), (Join-Path $destination '决策记录'), (Join-Path $destination '测试记录') | Out-Null

# 用户项目不进入公共工作区仓库；每个模组单独初始化 Git。
$git = Get-Command git -ErrorAction SilentlyContinue
if ($git) {
    & $git.Source -C $destination init --initial-branch=main | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Git 初始化失败：$destination" }

    $gitName = (& $git.Source -C $destination config user.name 2>$null)
    $gitEmail = (& $git.Source -C $destination config user.email 2>$null)
    if (-not $gitName -or -not $gitEmail) {
        $workspaceGitName = (& $git.Source -C $packageRoot config --local user.name 2>$null)
        $workspaceGitEmail = (& $git.Source -C $packageRoot config --local user.email 2>$null)
        if ($workspaceGitName -and $workspaceGitEmail) {
            & $git.Source -C $destination config --local user.name $workspaceGitName
            & $git.Source -C $destination config --local user.email $workspaceGitEmail
            $gitName = $workspaceGitName
            $gitEmail = $workspaceGitEmail
        }
    }
    if ($gitName -and $gitEmail) {
        $baselinePaths = @(
            '.gitignore',
            'PROJECT.md',
            'Scripts',
            "$ModId.csproj",
            "$ModId.json",
            $ModId,
            'export_presets.cfg',
            'project.godot'
        )
        foreach ($baselinePath in $baselinePaths) {
            & $git.Source -C $destination add -- $baselinePath
            if ($LASTEXITCODE -ne 0) { throw "Git 基线暂存失败：$baselinePath" }
        }
        & $git.Source -C $destination commit -m '项目：建立初始模组基线' | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'Git 基线提交失败。' }
        Write-Host '[sts2-workspace] 已建立独立 Git 和初始基线提交。' -ForegroundColor Green
    }
    else {
        Write-Warning '已初始化独立 Git，但未检测到 user.name / user.email，因此没有创建基线提交。请先配置 Git 身份，再提交初始基线。'
    }
}
else {
    Write-Warning '未找到 git 命令，项目已创建但没有版本历史。正式修改前请安装 Git 并建立项目基线。'
}

Write-Host "[sts2-workspace] 已创建模组工程：$destination" -ForegroundColor Green
Write-Host '下一步：'
Write-Host ('  .\build-native-mod.ps1 -ProjectDir "{0}" -GameDir "<游戏目录>"' -f $destination)
