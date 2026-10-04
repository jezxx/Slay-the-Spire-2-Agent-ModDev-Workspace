# Rename a mod project without assuming a particular old project name.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ProjectDir,
    [Parameter(Mandatory = $true)][string]$OldId,
    [Parameter(Mandatory = $true)][string]$NewId
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$ProjectDir = Resolve-ExistingDirectory $ProjectDir 'Mod project directory'
Assert-ModId $NewId
if ([string]::IsNullOrWhiteSpace($OldId)) { throw 'OldId cannot be empty.' }
$utf8Bom = New-Object System.Text.UTF8Encoding($true)
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$textExt = @('.cs','.csproj','.json','.cfg','.godot','.md','.props','.targets')
$files = Get-ChildItem -LiteralPath $ProjectDir -Recurse -File | Where-Object {
    $textExt -contains $_.Extension -and $_.FullName -notmatch '\\(bin|obj|\.godot|Build)\\'
}
foreach($file in $files) {
    $text = [IO.File]::ReadAllText($file.FullName)
    if ($text.Contains($OldId)) {
        $next = $text.Replace($OldId,$NewId)
        $enc = if ($file.Extension -in @('.cfg','.godot')) { $utf8NoBom } else { $utf8Bom }
        [IO.File]::WriteAllText($file.FullName,$next,$enc)
        Write-Host "content: $($file.FullName)"
    }
}
Get-ChildItem -LiteralPath $ProjectDir -Recurse -File | Where-Object { $_.Name.Contains($OldId) -and $_.FullName -notmatch '\\(bin|obj|\.godot|Build)\\' } | Sort-Object { $_.FullName.Length } -Descending | ForEach-Object { Rename-Item -LiteralPath $_.FullName -NewName $_.Name.Replace($OldId,$NewId) -Force }
Get-ChildItem -LiteralPath $ProjectDir -Recurse -Directory | Where-Object { $_.Name.Contains($OldId) -and $_.FullName -notmatch '\\(bin|obj|\.godot|Build)\\' } | Sort-Object { $_.FullName.Length } -Descending | ForEach-Object { Rename-Item -LiteralPath $_.FullName -NewName $_.Name.Replace($OldId,$NewId) -Force }
Write-Host "[sts2-workspace] renamed $OldId -> $NewId" -ForegroundColor Green
