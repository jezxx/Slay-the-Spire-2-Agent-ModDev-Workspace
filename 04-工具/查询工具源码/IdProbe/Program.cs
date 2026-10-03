using System.Reflection;

// Probe: load the real sts2 assembly and call ModelId/ModelDb helpers to learn the ID scheme.
if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: IdProbe <game-data-directory> [output-file]");
    return 2;
}

var gameDir = Path.GetFullPath(args[0]);
var outFile = args.Length > 1
    ? Path.GetFullPath(args[1])
    : Path.Combine(Environment.CurrentDirectory, "out_idscheme.txt");

AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
{
    var name = new AssemblyName(e.Name).Name + ".dll";
    var p = Path.Combine(gameDir, name);
    return File.Exists(p) ? Assembly.LoadFrom(p) : null;
};

var sb = new List<string>();
void W(string s) { sb.Add(s); Console.WriteLine(s); }

var asm = Assembly.LoadFrom(Path.Combine(gameDir, "sts2.dll"));
var tModelId = asm.GetType("MegaCrit.Sts2.Core.Models.ModelId")!;
var tModelDb = asm.GetType("MegaCrit.Sts2.Core.Models.ModelDb")!;

W($"ModelId: {tModelId.FullName}");

// 1) SlugifyCategory on candidate names
var slug = tModelId.GetMethod("SlugifyCategory", new[] { typeof(string) })!;
foreach (var s in new[] { "Bash", "IroncladCardPool", "ColorlessCardPool", "Strike", "TestCard", "IroncladStrikeCard", "Cards" })
    W($"  SlugifyCategory(\"{s}\") = \"{slug.Invoke(null, new object[] { s })}\"");

// 2) ModelDb.GetCategory / GetEntry for vanilla types
var getCategory = tModelDb.GetMethod("GetCategory", new[] { typeof(Type) })!;
var getEntry = tModelDb.GetMethod("GetEntry", new[] { typeof(Type) })!;
Type[] samples =
{
    asm.GetType("MegaCrit.Sts2.Core.Models.Cards.Bash")!,
    asm.GetType("MegaCrit.Sts2.Core.Models.Cards.StrikeIronclad")!,
    asm.GetType("MegaCrit.Sts2.Core.Models.CardPools.ColorlessCardPool")!,
    asm.GetType("MegaCrit.Sts2.Core.Models.Relics.BurningBlood") ?? asm.GetType("MegaCrit.Sts2.Core.Models.Relics.Anchor")!,
    asm.GetType("MegaCrit.Sts2.Core.Models.Powers.StrengthPower")!,
};
foreach (var t in samples)
{
    if (t == null) { W("  <sample type not found>"); continue; }
    try { W($"  {t.Name}: category=\"{getCategory.Invoke(null, new object[] { t })}\" entry=\"{getEntry.Invoke(null, new object[] { t })}\""); }
    catch (Exception ex) { W($"  {t.Name}: ERROR {ex.InnerException?.Message ?? ex.Message}"); }
}

// 2b) ModelId.ToString() for vanilla and synthetic ids
var ctor = tModelId.GetConstructor(new[] { typeof(string), typeof(string) })!;
var none = tModelId.GetField("none")!.GetValue(null);
W($"  ModelId.none = {none}");
foreach (var (c, e) in new[] { ("CARD", "BASH"), ("Bash", "Bash"), ("card", "bash"), ("TEST", "TEST_CARD") })
{
    var id = ctor.Invoke(new object[] { c, e });
    W($"  ModelId(\"{c}\",\"{e}\").ToString() = \"{id}\"");
}

// 2c) localization key helpers on a model? check LocString / key conventions
var tModManager = asm.GetType("MegaCrit.Sts2.Core.Modding.ModManager")!;
var getLoc = tModManager.GetMethod("GetModdedLocTables")!;
try { var r = getLoc.Invoke(null, new object[] { "zhs", "cards" }); W($"  GetModdedLocTables(zhs, cards) = {(r == null ? "null" : string.Join(", ", ((System.Collections.IEnumerable)r).Cast<object>().Take(8)))}"); }
catch (Exception ex) { W($"  GetModdedLocTables ERROR {ex.InnerException?.Message ?? ex.Message}"); }

// 3) legacy: what does a modded namespace type produce? scan candidates
W("--- SlugifyCategory on a few mod-ish strings ---");
foreach (var s in new[] { "Test", "MyMod", "STS2TEST" })
{
    try { W($"  \"{s}\" -> \"{slug.Invoke(null, new object[] { s })}\""); } catch (Exception ex) { W($"  \"{s}\" ERROR {ex.Message}"); }
}
File.WriteAllLines(outFile, sb);
