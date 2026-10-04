# Extract upgrade() statements from decompiled Slay the Spire 1 card Java sources.
# This is a source-reading aid, not a Tower 2 implementation translator.
# Usage: .\extract-sts1-upgrade.ps1 -JavaDir <cards> [-Ids A,B] [-OutFile <csv>]
[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$JavaDir,[string[]]$Ids,[string]$OutFile)
$ErrorActionPreference='Stop'; if(-not(Test-Path -LiteralPath $JavaDir)){throw "Java source directory not found: $JavaDir"}
function Get-Body([string]$text,[string]$signature){$i=$text.IndexOf($signature);if($i-lt 0){return ''};$j=$text.IndexOf('{',$i);if($j-lt 0){return ''};$depth=0;for($k=$j;$k-lt $text.Length;$k++){if($text[$k]-eq '{'){$depth++}elseif($text[$k]-eq '}'){$depth--;if($depth-eq 0){return $text.Substring($j+1,$k-$j-1)}}};return ''}
$rows=New-Object System.Collections.Generic.List[object]
foreach($f in (Get-ChildItem -LiteralPath $JavaDir -Recurse -File -Filter '*.java'|Sort-Object FullName)){
  $id=$f.BaseName;if($Ids -and ($Ids -notcontains $id)){continue};$t=Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8
  $costs=@();foreach($m in [regex]::Matches($t,'this\.cost\s*=\s*([^;]+);')){$costs+=($m.Groups[1].Value-replace '\s+','')}
  $mc=[regex]::Match($t,'int\s+COST\s*=\s*(-?\d+)');$constCost=if($mc.Success){$mc.Groups[1].Value}else{''}
  $body=Get-Body $t 'void upgrade()';$stmts=@();foreach($line in($body -split '\r?\n')){ $l=($line-replace '//.*$','').Trim();if($l -eq '' -or $l -eq '{' -or $l -eq '}'){continue};$stmts+=(($l-replace '\s+',' ')-replace '^this\.','') }
  $rows.Add([pscustomobject]@{Id=$id;ConstCost=$constCost;CostAssignments=($costs -join ' | ');Upgrade=($stmts -join ' ; ')})
}
foreach($r in $rows){Write-Output ("{0,-46} COST={1,-3} costAssignments=[{2}] upgrade: {3}" -f $r.Id,$r.ConstCost,$r.CostAssignments,$r.Upgrade)}
if($OutFile){$dir=Split-Path -Parent $OutFile;if($dir -and -not(Test-Path $dir)){New-Item -ItemType Directory -Force -Path $dir|Out-Null};$rows|Export-Csv -LiteralPath $OutFile -NoTypeInformation -Encoding UTF8;Write-Host "wrote $OutFile"}
