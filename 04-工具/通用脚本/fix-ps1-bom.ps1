# Make sure every non-ASCII .ps1 in this repo carries a UTF-8 BOM.
#
# Why this exists (it cost a broken build):
# these scripts contain Chinese comments. Without a BOM, Windows PowerShell decodes
# the file using the system ANSI codepage (GBK here). In GBK a lead byte consumes the
# FOLLOWING byte - including a newline. So a comment silently swallows the next line of
# real code, and the parse errors that surface are wildly misleading, e.g.
#     Missing expression after ','
#     Unexpected token '') }
#     The string is missing the terminator: '.
# The bytes on disk are perfectly fine; only the BOM is gone.
#
# Editing a .ps1 with a generic text-editing tool can drop the BOM without warning.
# dev.ps1 runs this fixer before invoking child scripts; standalone tool edits should
# still run this script explicitly before Windows PowerShell 5.1 executes them.
#
# Usage:
#   .\fix-ps1-bom.ps1              # fix in place, report what changed
#   .\fix-ps1-bom.ps1 -WhatIfOnly  # report only, change nothing

[CmdletBinding()]
param(
    [string]$Root,
    [switch]$WhatIfOnly
)

$ErrorActionPreference = 'Stop'

# Derive the repo root from this script's location; never hard-code it
# (this repo lives under a non-ASCII path).
if (-not $Root) { $Root = Split-Path -Parent $PSScriptRoot }
if (-not (Test-Path $Root)) { throw "root not found: $Root" }

$utf8Strict = [System.Text.UTF8Encoding]::new($false, $true)
$utf8Bom = [System.Text.UTF8Encoding]::new($true)

$withBom = 0
$fixed = 0
$skipped = 0

foreach ($file in (Get-ChildItem -Path $Root -Recurse -File -Include *.ps1 -ErrorAction SilentlyContinue)) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)

    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        $withBom++
        continue
    }

    # Only rewrite files that are genuinely valid UTF-8 and actually contain non-ASCII.
    # A pure-ASCII script needs no BOM, and a non-UTF-8 file must never be rewritten blindly.
    try {
        $text = $utf8Strict.GetString($bytes)
    }
    catch {
        Write-Warning "not valid UTF-8, left alone: $($file.FullName)"
        $skipped++
        continue
    }

    if ($text -notmatch '[^\x00-\x7F]') {
        $skipped++
        continue
    }

    if ($WhatIfOnly) {
        Write-Host "would add BOM : $($file.FullName)"
    }
    else {
        [System.IO.File]::WriteAllText($file.FullName, $text, $utf8Bom)
        Write-Host "added BOM     : $($file.FullName)"
    }
    $fixed++
}

Write-Host "already had BOM: $withBom | fixed: $fixed | skipped (ascii or invalid): $skipped"
