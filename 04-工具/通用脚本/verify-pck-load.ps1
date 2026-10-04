# Verify that a real Godot PCK serves the resource paths listed in a manifest.
# Manifest CSV columns: kind,res (res must start with res://).
# A pre-existing resource is reported separately and is not counted as proof.
# Usage: .\verify-pck-load.ps1 -Pck <file.pck> -ManifestFile <paths.csv> -Godot <godot.exe>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$Pck,
    [Parameter(Mandatory=$true)][string]$ManifestFile,
    [Parameter(Mandatory=$true)][string]$Godot,
    [string]$HostDir=(Join-Path $env:TEMP 'sts2-pck-verify-host')
)
$ErrorActionPreference='Stop'
foreach($p in @($Pck,$ManifestFile,$Godot)){if(-not(Test-Path -LiteralPath $p)){throw "path not found: $p"}}
$rows=Import-Csv -LiteralPath $ManifestFile;if(-not $rows){throw 'manifest is empty'}
$checks=New-Object System.Collections.Generic.List[object]
foreach($r in $rows){if(-not $r.kind -or -not $r.res){throw 'manifest requires kind,res columns'};if($r.res -notlike 'res://*'){throw "resource path must start with res://: $($r.res)"};$checks.Add([pscustomobject]@{kind=$r.kind;res=$r.res})}
New-Item -ItemType Directory -Force -Path $HostDir|Out-Null
$probe=New-Object System.Text.StringBuilder
[void]$probe.AppendLine('extends SceneTree');[void]$probe.AppendLine('var paths = [')
foreach($c in $checks){$kind=$c.kind.Replace('"','\"');$res=$c.res.Replace('"','\"');[void]$probe.AppendLine(('    ["{0}", "{1}"],' -f $kind,$res))}
[void]$probe.AppendLine(']');[void]$probe.AppendLine('func _init():');[void]$probe.AppendLine('    var before = {}');[void]$probe.AppendLine('    for e in paths:');[void]$probe.AppendLine('        before[e[1]] = ResourceLoader.exists(e[1])');[void]$probe.AppendLine('    var mounted = ProjectSettings.load_resource_pack("res://undertest.pck", true)');[void]$probe.AppendLine('    print("PACK_MOUNTED=", mounted)');[void]$probe.AppendLine('    var pass_n = 0');[void]$probe.AppendLine('    var fail_n = 0');[void]$probe.AppendLine('    var preexisting_n = 0');[void]$probe.AppendLine('    for e in paths:');[void]$probe.AppendLine('        if before[e[1]]:');[void]$probe.AppendLine('            preexisting_n += 1');[void]$probe.AppendLine('            print("PREEXISTING(not-proof) ", e[0], " ", e[1])');[void]$probe.AppendLine('            continue');[void]$probe.AppendLine('        var r = ResourceLoader.load(e[1])');[void]$probe.AppendLine('        if r == null:');[void]$probe.AppendLine('            fail_n += 1');[void]$probe.AppendLine('            print("FAIL ", e[0], " ", e[1])');[void]$probe.AppendLine('        else:');[void]$probe.AppendLine('            pass_n += 1');[void]$probe.AppendLine('            print("OK ", e[0], " ", r.get_class(), " ", e[1])');[void]$probe.AppendLine('    print("SUMMARY pass=", pass_n, " fail=", fail_n, " preexisting=", preexisting_n)');[void]$probe.AppendLine('    quit(1 if fail_n > 0 else 0)')
$probeFile=Join-Path $HostDir 'verify.gd';[IO.File]::WriteAllText($probeFile,$probe.ToString(),(New-Object Text.UTF8Encoding($false)));[IO.File]::Copy($Pck,(Join-Path $HostDir 'undertest.pck'),$true)
$project=Join-Path $HostDir 'project.godot';if(-not(Test-Path -LiteralPath $project)){$nl=[string][char]10;$projectText='config_version=5'+$nl+$nl+'[application]'+$nl+'config/name="pck_verify_host"'+$nl+'config/features=PackedStringArray("4.5")'+$nl;[IO.File]::WriteAllText($project,$projectText,(New-Object Text.UTF8Encoding($false)))}
$out=& $Godot --headless --path $HostDir --script $probeFile 2>&1;$code=$LASTEXITCODE;$out|ForEach-Object{Write-Output $_};exit $code
