# Convert common T1 card placeholders into T2 localization fields.
# Only mechanically safe placeholder conversions are enabled by default.
# Usage: .\gen-card-loc.ps1 -CardsJson <json> -OutFile <json> [-ModPrefix MOD_] [-OnlyIds A,B]
[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$CardsJson,[Parameter(Mandatory=$true)][string]$OutFile,[string]$ModPrefix='MOD_',[string[]]$OnlyIds,[string[]]$ColorKeywords)
$ErrorActionPreference='Stop';if(-not(Test-Path -LiteralPath $CardsJson)){throw "input JSON not found: $CardsJson"}
function ToKey([string]$id){$s=[regex]::Replace($id,'([a-z0-9])([A-Z])','$1_$2');return ($ModPrefix+$s.ToUpperInvariant())}
function ConvertDesc([string]$d){if(-not$d){return $null};$d=$d.Replace(' NL ',[Environment]::NewLine).Replace('!D!','{Damage:diff()}').Replace('!B!','{Block:diff()}').Replace('!M!','{Magic:diff()}').Replace('[E]','{Energy:energyIcons()}');foreach($kw in $ColorKeywords){if($kw){$escaped=[regex]::Escape($kw);$d=[regex]::Replace($d,"(?<![\w\[]])$escaped(?![\w\[]])",'[gold]'+$kw+'[/gold]')}};return $d}
$cards=Get-Content -LiteralPath $CardsJson -Raw -Encoding UTF8|ConvertFrom-Json;if($OnlyIds){$cards=$cards|Where-Object{$OnlyIds -contains $_.id}};$out=[ordered]@{};$empty=0
foreach($c in $cards){$key=ToKey $c.id;$out["$key.title"]=$c.name;$d=ConvertDesc $c.description;if([string]::IsNullOrWhiteSpace($d)){$empty++}else{$out["$key.description"]=$d};if($c.upgradeDesc){$u=ConvertDesc $c.upgradeDesc;if(-not[string]::IsNullOrWhiteSpace($u)){$out["$key.descriptionUpgraded"]=$u}}}
$dir=Split-Path -Parent $OutFile;if($dir -and -not(Test-Path $dir)){New-Item -ItemType Directory -Force -Path $dir|Out-Null};[IO.File]::WriteAllText($OutFile,($out|ConvertTo-Json -Depth 4),(New-Object System.Text.UTF8Encoding($false)));Write-Host ("wrote {0} entries for {1} cards -> {2}; empty descriptions: {3}" -f $out.Count,$cards.Count,$OutFile,$empty)
