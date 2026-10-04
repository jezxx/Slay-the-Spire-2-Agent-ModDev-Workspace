# Shared helpers for public STS2 workspace scripts.
# Callers provide the game directory; this file has no author-specific paths.

$ErrorActionPreference = 'Stop'
$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
function Set-PublicDotnetEnvironment([string]$TempRoot) {
    if (-not $TempRoot) { $TempRoot = [IO.Path]::GetTempPath() }
    $TempRoot = [IO.Path]::GetFullPath($TempRoot)
    New-Item -ItemType Directory -Force -Path $TempRoot | Out-Null
    $dotnetHome = Join-Path $TempRoot 'sts2-agent-dotnet-home'
    New-Item -ItemType Directory -Force -Path $dotnetHome | Out-Null
    $env:TEMP = $TempRoot
    $env:TMP = $TempRoot
    if (-not $env:USERPROFILE) { throw 'USERPROFILE is missing; .NET/NuGet cannot determine the user cache root.' }
    if (-not $env:APPDATA) { $env:APPDATA = Join-Path $env:USERPROFILE 'AppData\Roaming' }
    if (-not $env:LOCALAPPDATA) { $env:LOCALAPPDATA = Join-Path $env:USERPROFILE 'AppData\Local' }
    if (-not $env:SystemDrive) { $env:SystemDrive = [IO.Path]::GetPathRoot($env:USERPROFILE).TrimEnd('\') }
    if (-not $env:ProgramData) { $env:ProgramData = Join-Path $env:SystemDrive 'ProgramData' }
    if (-not $env:ProgramFiles) { $env:ProgramFiles = Join-Path $env:SystemDrive 'Program Files' }
    if (-not ${env:ProgramFiles(x86)}) { ${env:ProgramFiles(x86)} = Join-Path $env:SystemDrive 'Program Files (x86)' }
    if (-not $env:CommonProgramFiles) { $env:CommonProgramFiles = Join-Path $env:ProgramFiles 'Common Files' }
    if (-not ${env:CommonProgramFiles(x86)}) { ${env:CommonProgramFiles(x86)} = Join-Path ${env:ProgramFiles(x86)} 'Common Files' }
    $env:DOTNET_CLI_HOME = $dotnetHome
    $env:NUGET_PACKAGES = Join-Path $TempRoot 'nuget-packages'
    $env:NUGET_HTTP_CACHE_PATH = Join-Path $TempRoot 'nuget-http-cache'
    $env:NUGET_PLUGINS_CACHE_PATH = Join-Path $TempRoot 'nuget-plugins-cache'
    New-Item -ItemType Directory -Force -Path $env:NUGET_PACKAGES, $env:NUGET_HTTP_CACHE_PATH, $env:NUGET_PLUGINS_CACHE_PATH | Out-Null
    $env:DOTNET_NOLOGO = '1'
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
}


function ConvertTo-PublicProcessArgument([AllowEmptyString()][string]$Value) {
    if ($null -eq $Value) { return '""' }
    return '"' + $Value.Replace('"', '\"') + '"'
}

function Invoke-PublicDotnet([string]$DotnetExe, [string[]]$Arguments) {
    if (-not (Test-Path -LiteralPath $DotnetExe -PathType Leaf) -and -not (Get-Command $DotnetExe -ErrorAction SilentlyContinue)) {
        throw ".NET CLI was not found: $DotnetExe"
    }
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $DotnetExe
    $psi.Arguments = (($Arguments | ForEach-Object { ConvertTo-PublicProcessArgument ([string]$_) }) -join ' ')
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $psi
    if (-not $process.Start()) { throw "Could not start .NET CLI: $DotnetExe" }
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()
    if ($stdout) { Write-Host $stdout.TrimEnd() }
    if ($stderr) { Write-Host $stderr.TrimEnd() -ForegroundColor DarkYellow }
    return $process.ExitCode
}

function Resolve-WorkspacePath([string]$RelativePath) {
    return [IO.Path]::GetFullPath((Join-Path $WorkspaceRoot $RelativePath))
}

function Resolve-ExistingDirectory([string]$Path, [string]$Label) {
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "$Label does not exist: $Path"
    }
    return (Resolve-Path -LiteralPath $Path).Path
}

function Resolve-GameDirectory([string]$GameDir) {
    $resolved = Resolve-ExistingDirectory $GameDir 'Game directory'
    $data = Join-Path $resolved 'data_sts2_windows_x86_64'
    if (-not (Test-Path -LiteralPath (Join-Path $data 'sts2.dll') -PathType Leaf)) {
        throw "sts2.dll was not found under: $data"
    }
    return $resolved
}

function Get-GameDataDirectory([string]$GameDir) {
    return (Join-Path (Resolve-GameDirectory $GameDir) 'data_sts2_windows_x86_64')
}

function Get-GameLogPath([string]$LogPath) {
    if ($LogPath) { return [IO.Path]::GetFullPath($LogPath) }
    return (Join-Path $env:APPDATA 'SlayTheSpire2\logs\godot.log')
}

function Resolve-ToolCommand([string]$Requested, [string[]]$Names, [string]$Label) {
    if ($Requested) {
        if (-not (Test-Path -LiteralPath $Requested -PathType Leaf)) {
            throw "$Label was not found: $Requested"
        }
        return (Resolve-Path -LiteralPath $Requested).Path
    }
    foreach ($name in $Names) {
        $cmd = Get-Command $name -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($cmd) { return $cmd.Source }
    }
    throw "Could not find $Label. Pass its path explicitly."
}

function Get-ModProjectId([string]$ProjectDir, [string]$ModId) {
    if ($ModId) { return $ModId }
    $csproj = Get-ChildItem -LiteralPath $ProjectDir -Filter '*.csproj' -File | Select-Object -First 1
    if (-not $csproj) { throw "No .csproj found in $ProjectDir" }
    return [IO.Path]::GetFileNameWithoutExtension($csproj.Name)
}

function Assert-ModId([string]$ModId) {
    if ($ModId -notmatch '^[A-Za-z][A-Za-z0-9_]*$') {
        throw "Invalid ModId '$ModId'. Use ASCII letters, digits and underscores, starting with a letter."
    }
}
