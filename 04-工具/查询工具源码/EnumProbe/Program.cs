using System;
using System.IO;
using System.Linq;
using System.Reflection;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: EnumProbe <game-data-directory>");
    Environment.ExitCode = 2;
    return;
}

var gameDataDir = Path.GetFullPath(args[0]);
var asm = Assembly.LoadFrom(Path.Combine(gameDataDir, "sts2.dll"));
Type[] types;
try { types = asm.GetTypes(); }
catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray()!; }

foreach (var name in new[] { "PotionUsage", "RelicRarity", "RelicTier", "CardRarity", "TargetType", "PotionRarity" })
{
    var t = types.FirstOrDefault(x => x.Name == name);
    if (t != null)
    {
        var names = Enum.GetNames(t);
        Console.WriteLine($"{name}: {string.Join(", ", names)}");
    }
    else
    {
        Console.WriteLine($"{name}: NOT FOUND");
    }
}
