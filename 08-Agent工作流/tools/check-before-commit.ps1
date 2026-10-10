# 提交前检查：默认检查指定仓库的暂存区。
[CmdletBinding()]
param(
    [string]$RepoRoot = '.',
    [switch]$WorkingTree
)

$ErrorActionPreference = 'Stop'

try {
    [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
    $OutputEncoding = [System.Text.UTF8Encoding]::new($false)
}
catch {
    # 某些宿主不允许修改控制台编码。
}

$resolvedRepoRoot = & git -C $RepoRoot rev-parse --show-toplevel
if ($LASTEXITCODE -ne 0 -or -not $resolvedRepoRoot) {
    Write-Error '指定路径不在 Git 仓库中，无法执行提交前检查。'
    exit 2
}
$RepoRoot = $resolvedRepoRoot.Trim()

$violations = New-Object System.Collections.Generic.List[string]
$pathRules = @(
    @{ Pattern = '(?i)\.pck$'; Reason = 'PCK 打包产物' },
    @{ Pattern = '(?i)\.(dll|pdb)$'; Reason = '编译产物' },
    @{ Pattern = '(?i)\.log$'; Reason = '日志文件' },
    @{ Pattern = '(?i)(^|/)hs_err_pid'; Reason = 'JVM 崩溃转储' },
    @{ Pattern = '(?i)\.(tmp|bak|orig)$'; Reason = '临时、备份或冲突残留文件' },
    @{ Pattern = '(?i)(^|/)__pycache__(/|$)'; Reason = 'Python 缓存目录' },
    @{ Pattern = '(?i)(^|/)(bin|obj|Build|\.godot|\.work|\.preview|\.backup)(/|$)'; Reason = '构建或临时目录' },
    @{ Pattern = '^11-缓存文件存放区\(定期清理/(?!README\.md$)'; Reason = '工作区缓存目录' }
)

function Invoke-GitPaths([string[]]$Arguments) {
    & git -C $RepoRoot -c core.quotepath=false @Arguments
    if ($LASTEXITCODE -ne 0) { throw "git 命令失败：$($Arguments -join ' ')" }
}

$paths = New-Object System.Collections.Generic.List[string]
foreach ($path in (Invoke-GitPaths @('diff', '--cached', '--name-only', '--diff-filter=ACMR'))) {
    if ($path) { $paths.Add($path.Trim()) }
}
if ($WorkingTree) {
    foreach ($path in (Invoke-GitPaths @('diff', '--name-only', '--diff-filter=ACMR'))) {
        if ($path) { $paths.Add($path.Trim()) }
    }
    foreach ($path in (Invoke-GitPaths @('ls-files', '--others', '--exclude-standard'))) {
        if ($path) { $paths.Add($path.Trim()) }
    }
}
$paths = @($paths | Sort-Object -Unique)

foreach ($path in $paths) {
    foreach ($rule in $pathRules) {
        if ($path -match $rule.Pattern) {
            $violations.Add("[$($rule.Reason)] $path")
            break
        }
    }
}

foreach ($path in $paths) {
    if ($path -notmatch '(?i)\.ps1$') { continue }
    $fullPath = Join-Path $RepoRoot ($path -replace '/', '\')
    if (-not (Test-Path -LiteralPath $fullPath)) { continue }

    $bytes = [System.IO.File]::ReadAllBytes($fullPath)
    $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    if ($hasBom) { continue }

    $hasNonAscii = $false
    foreach ($byte in $bytes) {
        if ($byte -ge 0x80) { $hasNonAscii = $true; break }
    }
    if ($hasNonAscii) {
        $violations.Add("[PowerShell 含非 ASCII 但缺少 UTF-8 BOM] $path")
    }
}

$source = if ($WorkingTree) { '暂存区和工作区' } else { '暂存区' }
Write-Host "check-before-commit：检查 $source，共 $($paths.Count) 个路径。"

if ($violations.Count -gt 0) {
    foreach ($violation in $violations) { Write-Host "  $violation" }
    exit 1
}

Write-Host '通过 OK'
exit 0
