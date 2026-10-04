# Batch-decompile Java class files with CFR. All external paths are explicit.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ClassRoot,
    [Parameter(Mandatory = $true)][string]$OutDir,
    [Parameter(Mandatory = $true)][string]$JavaExe,
    [Parameter(Mandatory = $true)][string]$CfrJar,
    [string[]]$Include = @('*.class')
)

$ErrorActionPreference = 'Stop'
$ClassRoot = (Resolve-Path -LiteralPath $ClassRoot).Path
if (-not (Test-Path -LiteralPath $JavaExe -PathType Leaf)) { throw "java.exe not found: $JavaExe" }
if (-not (Test-Path -LiteralPath $CfrJar -PathType Leaf)) { throw "CFR jar not found: $CfrJar" }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$files = @()
foreach($pattern in $Include) { $files += Get-ChildItem -LiteralPath $ClassRoot -Recurse -Filter $pattern -File -ErrorAction SilentlyContinue }
$files = @($files | Sort-Object FullName -Unique)
if ($files.Count -eq 0) { Write-Host '[sts2-workspace] no class files matched.'; exit 0 }
$ok=0; $fail=0
foreach($file in $files) {
    $relative = $file.FullName.Substring($ClassRoot.Length).TrimStart('\\')
    $target = Join-Path $OutDir ([IO.Path]::ChangeExtension($relative,'.java'))
    New-Item -ItemType Directory -Force -Path (Split-Path $target -Parent) | Out-Null
    $text = & $JavaExe -jar $CfrJar $file.FullName --extraclasspath $ClassRoot 2>&1
    if ($LASTEXITCODE -ne 0) { Write-Warning "CFR failed: $($file.FullName)"; $fail++; continue }
    [IO.File]::WriteAllText($target,($text -join [Environment]::NewLine),(New-Object Text.UTF8Encoding($false)))
    $ok++
}
Write-Host "[sts2-workspace] CFR complete: ok=$ok failed=$fail"
if ($fail -gt 0) { exit 1 }
