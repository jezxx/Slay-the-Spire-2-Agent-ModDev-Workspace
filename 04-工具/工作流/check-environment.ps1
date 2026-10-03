param(
    [string]$GameDir,
    [string]$GodotExe,
    [switch]$RequirePython,
    [switch]$RequireGodot
)

$ErrorActionPreference = 'Continue'
$failed = $false
$results = New-Object System.Collections.Generic.List[string]

function Add-Result {
    param([string]$Status, [string]$Message)
    $line = "[$Status] $Message"
    $results.Add($line)
    Write-Host $line
}

function Resolve-CommandPath {
    param([string[]]$Names)
    foreach ($name in $Names) {
        $command = Get-Command $name -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -ne $command) {
            return $command.Source
        }
    }
    return $null
}

Write-Host 'Slay the Spire 2 Mod Development Workspace - environment preflight'
Write-Host "PowerShell: $($PSVersionTable.PSVersion)"
Add-Result 'OK' 'PowerShell 可用。'

$dotnetPath = Resolve-CommandPath @('dotnet.exe', 'dotnet')
if ($null -eq $dotnetPath) {
    Add-Result 'MISSING' '找不到 .NET CLI。原生 C# 模组需要与模板匹配的 .NET 9 SDK。'
    $failed = $true
} else {
    $dotnetVersion = (& $dotnetPath --version 2>$null | Select-Object -First 1).Trim()
    if ($dotnetVersion -match '^9\.') {
        Add-Result 'OK' ".NET SDK $dotnetVersion 可用。"
    } else {
        Add-Result 'BLOCKED' ".NET CLI 版本为 $dotnetVersion；当前模板目标是 net9.0，请安装 .NET 9 SDK，不要只改项目目标框架。"
        $failed = $true
    }
}

$pythonPath = Resolve-CommandPath @('python.exe', 'python')
$pythonArgs = @()
if ($null -eq $pythonPath) {
    $pythonPath = Resolve-CommandPath @('py.exe', 'py')
    $pythonArgs = @('-3')
}
if ($null -eq $pythonPath) {
    if ($RequirePython) {
        Add-Result 'MISSING' '找不到 Python 3；当前任务要求 Python 图片处理脚本。'
        $failed = $true
    } else {
        Add-Result 'OPTIONAL' '未找到 Python 3；当前任务未要求 Python，因此不阻塞原生 C# 路线。'
    }
} else {
    $pythonCheck = & $pythonPath @pythonArgs -c 'import numpy, PIL; print("ok")' 2>$null
    if ($LASTEXITCODE -eq 0) {
        Add-Result 'OK' 'Python 3、numpy 和 Pillow 可用。'
    } elseif ($RequirePython) {
        Add-Result 'BLOCKED' '已找到 Python 3，但 numpy 或 Pillow 导入失败；请补齐图片处理依赖。'
        $failed = $true
    } else {
        Add-Result 'OPTIONAL' '已找到 Python 3，但 numpy 或 Pillow 不完整；当前任务未要求 Python。'
    }
}

if ([string]::IsNullOrWhiteSpace($GodotExe)) {
    $GodotExe = Resolve-CommandPath @('godot.exe', 'godot', 'Godot.exe')
}
if ([string]::IsNullOrWhiteSpace($GodotExe) -or -not (Test-Path -LiteralPath $GodotExe -PathType Leaf)) {
    if ($RequireGodot) {
        Add-Result 'MISSING' '找不到 Godot；当前任务要求 Godot/PCK 或场景导出。'
        $failed = $true
    } else {
        Add-Result 'OPTIONAL' '未找到 Godot；当前任务未要求 Godot，因此不阻塞原生 C# 路线。'
    }
} else {
    Add-Result 'OK' "Godot 可用：$GodotExe"
}

if (-not [string]::IsNullOrWhiteSpace($GameDir)) {
    $resolvedGameDir = (Resolve-Path -LiteralPath $GameDir -ErrorAction SilentlyContinue).Path
    if ($null -eq $resolvedGameDir) {
        Add-Result 'MISSING' "找不到游戏目录：$GameDir"
        $failed = $true
    } else {
        $dataDir = Join-Path $resolvedGameDir 'data_sts2_windows_x86_64'
        if (-not (Test-Path -LiteralPath $dataDir -PathType Container)) {
            Add-Result 'MISSING' "找不到游戏数据目录：$dataDir"
            $failed = $true
        } else {
            $requiredFiles = @('sts2.dll', '0Harmony.dll', 'GodotSharp.dll')
            $missingFiles = @($requiredFiles | Where-Object { -not (Test-Path -LiteralPath (Join-Path $dataDir $_) -PathType Leaf) })
            if ($missingFiles.Count -gt 0) {
                Add-Result 'BLOCKED' "游戏数据目录缺少：$($missingFiles -join ', ')"
                $failed = $true
            } else {
                Add-Result 'OK' "游戏目录和编译所需程序集可用：$resolvedGameDir"
            }
        }
    }
} else {
    Add-Result 'INFO' '未提供 -GameDir；跳过游戏目录和程序集检查。'
}

Write-Host ''
if ($failed) {
    Write-Host 'Preflight failed: 请先处理上面的 MISSING/BLOCKED 项。脚本没有安装或修改任何软件。'
    exit 1
}
Write-Host 'Preflight passed: 当前检查范围内没有阻塞项。'
exit 0
