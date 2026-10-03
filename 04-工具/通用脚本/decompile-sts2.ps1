# decompile-sts2.ps1 -- one-time (per game build) full C# decompile of sts2.dll.
#
# WHY: every engine-behaviour question (CardCmd.AutoPlay, CardPile.Get, Hook.ShouldPlay,
# CardModel.OnPlayWrapper, NPowerContainer.UpdatePositions, ...) used to be answered by dumping
# IL on demand (api\ildump\*.txt, 127 files and counting). A full source tree can simply be
# grepped, which is faster and shows the surrounding code.
#
# OUTPUT: the OutDir parameter, which defaults to this package's versioned decompile directory.
#         Pass an ASCII-only output path if a local dotnet/mono host mishandles non-ASCII argv.
#
# NOTE (ASCII only on purpose): scripts containing non-ASCII text must be saved as UTF-8 *with BOM*,
# otherwise PowerShell 5.1 decodes them as GBK and reports bogus syntax errors. This file therefore
# uses English comments only -- keep it that way, or add a BOM after every edit.

[CmdletBinding()]
param(
    [string]$GameDir,
    [string]$OutDir  = (Join-Path $PSScriptRoot '..\..\02-游戏反编译资料\版本-0.111.0\反编译源码'),
    [string]$IlSpy,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

# ilspycmd 8.2.0.7535 targets net6.0; use a compatible installed .NET runtime or SDK.
$env:DOTNET_ROLL_FORWARD = 'LatestMajor'

if (-not $GameDir) {
    throw 'GameDir is required. Pass the game data directory containing sts2.dll.'
}
if (-not $IlSpy) {
    throw 'IlSpy is required. Pass the full path to ilspycmd.exe.'
}

$ilspy = $IlSpy
$dll   = Join-Path $GameDir 'sts2.dll'

if (-not (Test-Path -LiteralPath $dll)) {
    throw "sts2.dll not found: $dll"
}
if (-not (Test-Path -LiteralPath $ilspy)) {
    throw ("ilspycmd not found: {0}. Install ILSpy command line tools separately, then pass its full path with -IlSpy." -f $ilspy)
}

$hash  = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash
$stamp = Join-Path $OutDir '.sts2-dll.sha256'

if ((Test-Path -LiteralPath $stamp) -and -not $Force) {
    $old = (Get-Content -LiteralPath $stamp -Raw).Trim()
    if ($old -eq $hash) {
        Write-Host "[sts2-src] up to date ($hash)"
        exit 0
    }
}

Write-Host "[sts2-src] decompiling $dll"
Write-Host "[sts2-src]   sha256 = $hash"
Write-Host "[sts2-src]   output = $OutDir"

if (Test-Path -LiteralPath $OutDir) {
    Remove-Item -LiteralPath $OutDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

& $ilspy -p -o $OutDir -r $GameDir $dll
if ($LASTEXITCODE -ne 0) {
    throw "ilspycmd failed with exit code $LASTEXITCODE"
}

Set-Content -LiteralPath $stamp -Value $hash -Encoding ASCII

$files = Get-ChildItem -LiteralPath $OutDir -Recurse -File -Filter '*.cs'
Write-Host ("[sts2-src] done: {0} .cs files, {1:N1} MB" -f $files.Count, (($files | Measure-Object Length -Sum).Sum / 1MB))
