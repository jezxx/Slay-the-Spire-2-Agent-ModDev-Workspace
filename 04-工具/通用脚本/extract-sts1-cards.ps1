# Extract mechanically readable fields from decompiled Slay the Spire 1 card Java sources.
# This does not translate card effects; use the source and a manual semantic audit for that.
# Usage: .\extract-sts1-cards.ps1 -JavaDir <cards> -OutFile <json> [-LocFile <json>]
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$JavaDir,
    [Parameter(Mandatory = $true)][string]$OutFile,
    [string]$LocFile
)
$ErrorActionPreference = 'Stop'
$utf8 = New-Object System.Text.UTF8Encoding($false)
if (-not (Test-Path -LiteralPath $JavaDir)) { throw "Java source directory not found: $JavaDir" }
if ($LocFile -and -not (Test-Path -LiteralPath $LocFile)) { throw "localization file not found: $LocFile" }
$nameById=@{}; $descById=@{}; $upgById=@{}
if ($LocFile) {
    $loc = Get-Content -LiteralPath $LocFile -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($p in $loc.PSObject.Properties) {
        $entry=$p.Value
        if ($entry.NAME) { $nameById[$p.Name]=$entry.NAME }
        if ($entry.DESCRIPTION) { $descById[$p.Name]=$entry.DESCRIPTION }
        if ($entry.UPGRADE_DESCRIPTION) { $upgById[$p.Name]=$entry.UPGRADE_DESCRIPTION }
    }
}
function Grab([string]$text,[string]$pattern,[int]$group=1) { $m=[regex]::Match($text,$pattern); if($m.Success){return $m.Groups[$group].Value}; return $null }
function Resolve-Num([string]$token,[string]$text) {
    if([string]::IsNullOrWhiteSpace($token)){return $null}; if($token -match '^-?\d+$'){return [int]$token}
    $m=[regex]::Match($text,('\b(?:int|static final int)\s+'+[regex]::Escape($token)+'\s*=\s*(-?\d+)')); if($m.Success){return [int]$m.Groups[1].Value}; return $null
}
$cards=@()
foreach($f in (Get-ChildItem -LiteralPath $JavaDir -Recurse -File -Filter '*.java' | Sort-Object FullName)) {
    $t=Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8
    $id=Grab $t 'public static final String ID = "([^"]+)"'; if(-not $id){$id=$f.BaseName}
    $costRaw=Grab $t 'private static final int COST = (-?\d+)'
    $dmgRaw=Grab $t 'this\.baseDamage = (\w+)'; $blockRaw=Grab $t 'this\.baseBlock = (\w+)'; $magicRaw=Grab $t 'this\.baseMagicNumber = (\w+)'
    $cards += [pscustomobject]@{
        id=$id; className=$f.BaseName
        name=if($nameById.ContainsKey($id)){$nameById[$id]}else{$null}
        description=if($descById.ContainsKey($id)){$descById[$id]}else{$null}
        upgradeDesc=if($upgById.ContainsKey($id)){$upgById[$id]}else{$null}
        cost=if($costRaw){[int]$costRaw}else{$null}
        type=Grab $t 'CardType\.(\w+)'; rarity=Grab $t 'CardRarity\.(\w+)'; target=Grab $t 'CardTarget\.(\w+)'
        baseDamage=Resolve-Num $dmgRaw $t; baseBlock=Resolve-Num $blockRaw $t; baseMagic=Resolve-Num $magicRaw $t
        upgradeDamage=Resolve-Num (Grab $t 'UPGRADE_PLUS_DMG\s*=\s*(-?\d+)') $t
        upgradeBlock=Resolve-Num (Grab $t 'UPGRADE_PLUS_BLOCK\s*=\s*(-?\d+)') $t
        upgradeMagic=Resolve-Num (Grab $t 'UPGRADE_PLUS_MAGIC\s*=\s*(-?\d+)') $t
        hasLoc=$nameById.ContainsKey($id)
    }
}
$outDir=Split-Path -Parent $OutFile; if($outDir -and -not(Test-Path $outDir)){New-Item -ItemType Directory -Force -Path $outDir|Out-Null}
[IO.File]::WriteAllText($OutFile,($cards|ConvertTo-Json -Depth 5),$utf8)
Write-Host ("extracted {0} cards -> {1}" -f $cards.Count,$OutFile)
Write-Host ("missing localization: {0}" -f (($cards|Where-Object{-not $_.hasLoc}).Count))
