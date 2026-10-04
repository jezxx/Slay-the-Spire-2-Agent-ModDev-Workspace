# Assert live Tower 2 card contracts against the authoritative T1 spec.
#
# Inputs:
#   a T1 card spec JSON file    <- extract-sts1-spec.ps1   (T1 truth, from Java)
#   a T1-to-T2 mapping JSON file (T1 id -> T2 entry key)
#   a runtime T2 contract JSON file (live T2 values, from godot.log)
#   reports\card-contract-allowlist.json (optional) known-acceptable deviations
#
# Exit code = number of NON-allowlisted mismatches (0 = contract holds).
#
# Fields asserted (the ones we can map mechanically and that the manual audit proved meaningful):
#   cost, upgraded cost, type, rarity, target, Damage, Block, and the Damage/Block upgrade deltas.
# Magic is reported as INFO only: T1's baseMagicNumber is often modelled in T2 by a *typed* power
# var (WeakPower / ArmorBreakPower / ...), so a name-based comparison would be noise.
#
# Usage: .\assert-t1-t2-card-contract.ps1 -SpecFile <json> -MapFile <json> -ContractFile <json> [-AllowFile <json>] [-ReportFile <md>]
param(
    [Parameter(Mandatory = $true)][string]$SpecFile,
    [Parameter(Mandatory = $true)][string]$MapFile,
    [Parameter(Mandatory = $true)][string]$ContractFile,
    [string]$AllowFile,
    [string]$ReportFile
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $SpecFile)) { throw "spec file not found: $SpecFile" }
if (-not (Test-Path -LiteralPath $MapFile)) { throw "map file not found: $MapFile" }
if (-not (Test-Path -LiteralPath $ContractFile)) { throw "contract file not found: $ContractFile" }
if (-not $ReportFile) { $ReportFile = Join-Path (Split-Path -Parent $ContractFile) 'card-contract-assert.md' }
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

$spec     = Get-Content $SpecFile     -Raw -Encoding UTF8 | ConvertFrom-Json
$map      = Get-Content $MapFile      -Raw -Encoding UTF8 | ConvertFrom-Json
$contract = Get-Content $ContractFile -Raw -Encoding UTF8 | ConvertFrom-Json

$allow = @{}
if (Test-Path $AllowFile) {
    $a = Get-Content $AllowFile -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($e in $a) { $allow[($e.card + '|' + $e.field)] = $e.reason }
}

$typeMap   = @{ ATTACK='Attack'; SKILL='Skill'; POWER='Power'; STATUS='Status'; CURSE='Curse' }
$rarityMap = @{ BASIC='Basic'; COMMON='Common'; UNCOMMON='Uncommon'; RARE='Rare'; SPECIAL='Token' }
$targMap   = @{ ENEMY='AnyEnemy'; ALL_ENEMY='AllEnemies'; SELF='Self'; NONE='None'; ALL='AllEnemies' }

$specByClass = @{}
foreach ($c in $spec.cards) { $specByClass[$c.className] = $c }
$t2ByEntry = @{}
foreach ($c in $contract.cards) { $t2ByEntry[$c.entry] = $c }

function VarInt($vars, [string]$name) {
    if ($null -eq $vars) { return $null }
    $v = $vars.$name
    if ($null -eq $v) { return $null }
    return [int]$v
}

$issues = New-Object System.Collections.ArrayList
$notes  = New-Object System.Collections.ArrayList
$checked = 0

foreach ($m in $map) {
    $t1 = $specByClass[$m.id]
    if (-not $t1) { [void]$issues.Add((New-Object psobject -Property @{ card=$m.id; field='spec'; exp='-'; act='no T1 spec row' })); continue }

    $t2 = $t2ByEntry[$m.t2Key]
    if (-not $t2) { $t2 = $t2ByEntry[($m.t2Key -replace '_YM$', '')] }
    if (-not $t2) { [void]$issues.Add((New-Object psobject -Property @{ card=$m.id; field='contract'; exp=$m.t2Key; act='no T2 contract row' })); continue }

    $checked++

    # Backward-compatible guards: when the spec file predates a field, fall back to the
    # strict behaviour instead of silently skipping the assertion.
    $blockAssert = if ($null -ne $t1.PSObject.Properties['blockUsed'])  { [bool]$t1.blockUsed }  else { $true }
    $unplayable  = if ($null -ne $t1.PSObject.Properties['unplayable']) { [bool]$t1.unplayable } else { $false }

    function Add-Issue([string]$card, [string]$field, $exp, $act) {
        [void]$issues.Add((New-Object psobject -Property @{ card=$card; field=$field; exp=[string]$exp; act=[string]$act }))
    }

    # ---- cost ----
    # T1 encodes "cannot be played" as cost -2 (a sentinel, not a real cost); T2 represents
    # the same card with CardKeyword.Unplayable and cost 0, so the raw numbers are not
    # comparable for those cards.
    if ($null -ne $t1.cost -and -not $unplayable -and $t2.base.cost -ne $t1.cost) { Add-Issue $m.id 'cost' $t1.cost $t2.base.cost }
    $expUpgCost = if ($null -ne $t1.upgCost) { $t1.upgCost } else { $t1.cost }
    if ($null -ne $expUpgCost -and -not $unplayable -and $t2.upg.cost -ne $expUpgCost) { Add-Issue $m.id 'cost_upg' $expUpgCost $t2.upg.cost }

    # ---- enums ----
    $eT = $typeMap[$t1.type];     if ($eT -and $t2.base.type   -ne $eT) { Add-Issue $m.id 'type'   $eT $t2.base.type }
    $eR = $rarityMap[$t1.rarity]; if ($eR -and $t2.base.rarity -ne $eR) { Add-Issue $m.id 'rarity' $eR $t2.base.rarity }
    # T1's CardTarget.NONE is ambiguous: it covers both playable skills that only affect the
    # player (T2 models those as TargetType.Self) and cards that are never manually played
    # (T2 models those as TargetType.None). Accept either spelling for NONE.
    $eG = $targMap[$t1.target]
    $targetOk = if ($t1.target -eq 'NONE') { $t2.base.target -eq 'None' -or $t2.base.target -eq 'Self' }
                elseif ($eG)               { $t2.base.target -eq $eG }
                else                       { $true }
    if (-not $targetOk) { Add-Issue $m.id 'target' $eG $t2.base.target }

    # ---- Damage / Block base values ----
    $d2 = VarInt $t2.base.vars 'Damage'
    $b2 = VarInt $t2.base.vars 'Block'
    if ($null -ne $t1.baseDamage -and $t1.baseDamage -gt 0) {
        if ($null -eq $d2) { Add-Issue $m.id 'Damage' $t1.baseDamage 'missing var' }
        elseif ($d2 -ne $t1.baseDamage) { Add-Issue $m.id 'Damage' $t1.baseDamage $d2 }
    }
    # blockUsed=false means T1 sets baseBlock but never grants block (dead field, verified
    # to be SlashOfMeditation + SpiderLilyBurial), so asserting Block there is a false positive.
    if ($null -ne $t1.baseBlock -and $t1.baseBlock -gt 0 -and $blockAssert) {
        if ($null -eq $b2) { Add-Issue $m.id 'Block' $t1.baseBlock 'missing var' }
        elseif ($b2 -ne $t1.baseBlock) { Add-Issue $m.id 'Block' $t1.baseBlock $b2 }
    }

    # ---- upgrade deltas ----
    $ud2 = VarInt $t2.upg.vars 'Damage'
    $ub2 = VarInt $t2.upg.vars 'Block'
    if ($null -ne $t1.upgDamage) {
        if ($null -eq $d2 -or $null -eq $ud2) { Add-Issue $m.id 'Damage_upg' ("+" + $t1.upgDamage) 'missing var' }
        elseif (($ud2 - $d2) -ne $t1.upgDamage) { Add-Issue $m.id 'Damage_upg' ("+" + $t1.upgDamage) ("+" + ($ud2 - $d2)) }
    }
    if ($null -ne $t1.upgBlock -and $blockAssert) {
        if ($null -eq $b2 -or $null -eq $ub2) { Add-Issue $m.id 'Block_upg' ("+" + $t1.upgBlock) 'missing var' }
        elseif (($ub2 - $b2) -ne $t1.upgBlock) { Add-Issue $m.id 'Block_upg' ("+" + $t1.upgBlock) ("+" + ($ub2 - $b2)) }
    }

    # ---- INFO: magic not modelled as a "Magic" var ----
    if ($null -ne $t1.baseMagic) {
        $m2 = VarInt $t2.base.vars 'Magic'
        if ($null -eq $m2) { [void]$notes.Add(("{0}: T1 magic={1}, T2 has no Magic var (modelled by a typed power var?)" -f $m.id, $t1.baseMagic)) }
    }
}

$real = @()
$waived = @()
foreach ($i in $issues) {
    if ($allow.ContainsKey($i.card + '|' + $i.field)) { $waived += $i } else { $real += $i }
}

Write-Host ("card-contract: checked {0} pairs | mismatches {1} (waived {2})" -f $checked, $real.Count, $waived.Count)
foreach ($i in $real) { Write-Host ("  MISMATCH {0,-42} {1,-12} T1={2,-14} T2={3}" -f $i.card, $i.field, $i.exp, $i.act) }
foreach ($i in $waived) { Write-Host ("  waived   {0,-42} {1,-12} T1={2,-14} T2={3}" -f $i.card, $i.field, $i.exp, $i.act) }
if ($notes.Count -gt 0) { Write-Host ("  info: {0} cards model T1 magic via a typed var" -f $notes.Count) }

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine('# T1 -> T2 card contract assertion')
[void]$sb.AppendLine('')
[void]$sb.AppendLine(('- generated: {0}' -f (Get-Date).ToString('s')))
[void]$sb.AppendLine(('- pairs checked: {0}' -f $checked))
[void]$sb.AppendLine(('- mismatches: **{0}** (waived: {1})' -f $real.Count, $waived.Count))
[void]$sb.AppendLine('')
if ($real.Count -gt 0) {
    [void]$sb.AppendLine('## Mismatches')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('| card | field | T1 | T2 |')
    [void]$sb.AppendLine('|---|---|---|---|')
    foreach ($i in $real) { [void]$sb.AppendLine(('| {0} | {1} | {2} | {3} |' -f $i.card, $i.field, $i.exp, $i.act)) }
    [void]$sb.AppendLine('')
}
if ($waived.Count -gt 0) {
    [void]$sb.AppendLine('## Waived (known acceptable)')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('| card | field | T1 | T2 | reason |')
    [void]$sb.AppendLine('|---|---|---|---|---|')
    foreach ($i in $waived) { [void]$sb.AppendLine(('| {0} | {1} | {2} | {3} | {4} |' -f $i.card, $i.field, $i.exp, $i.act, $allow[($i.card + '|' + $i.field)])) }
    [void]$sb.AppendLine('')
}
[void]$sb.AppendLine('## Info')
[void]$sb.AppendLine('')
foreach ($n in $notes) { [void]$sb.AppendLine(('- {0}' -f $n)) }

[System.IO.File]::WriteAllText($ReportFile, $sb.ToString(), $utf8NoBom)
Write-Host ("report: {0}" -f $ReportFile)

exit $real.Count
