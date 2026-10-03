using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

// Usage: ApiProbe <assemblyDir> <assemblyFile> <outFile> [mode] [query...]
// modes:
//   types <regex>            -> list matching type full names
//   type <FullTypeName>      -> dump members of one type
//   members <regex>          -> list matching member signatures
//   search <regex>           -> search member names + signatures

var asmDir = args[0];
var asmFile = args[1];
var outFile = args[2];
var mode = args.Length > 3 ? args[3] : "types";
var queries = args.Skip(4).ToArray();

var sb = new StringBuilder();

// 关键：游戏目录里同时存在 mscorlib / System.* 兼容性门面程序集，
// 会和 .NET 9 运行时自带的同名程序集冲突（MetadataLoadContext 禁止同名声明的第二个程序集）。
// 做法：按文件名去重，运行时目录优先，其余保留游戏目录的副本。
var runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
var byName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
foreach (var p in Directory.GetFiles(runtimeDir, "*.dll"))
    byName[Path.GetFileName(p)] = p;

int skipped = 0;
foreach (var p in Directory.GetFiles(asmDir, "*.dll"))
{
    var name = Path.GetFileName(p);
    if (byName.ContainsKey(name)) { skipped++; continue; }
    byName[name] = p;
}
// 目标程序集永远用游戏目录里的那一份
byName[asmFile] = Path.Combine(asmDir, asmFile);
Console.Error.WriteLine($"[ApiProbe] 程序集解析表 {byName.Count} 项（因重名跳过 {skipped} 个游戏目录门面程序集）");

var resolver = new PathAssemblyResolver(byName.Values.Distinct().ToList());
using var mlc = new MetadataLoadContext(resolver);
var asm = mlc.LoadFromAssemblyPath(Path.Combine(asmDir, asmFile));

IEnumerable<Type> AllTypes()
{
    Type[] t;
    try { t = asm.GetTypes(); }
    catch (ReflectionTypeLoadException ex) { t = ex.Types.Where(x => x != null).ToArray()!; }
    return t;
}

static string Sig(MemberInfo m)
{
    try
    {
        switch (m)
        {
            case MethodInfo mi:
                var ps = string.Join(", ", mi.GetParameters().Select(p => $"{SafeName(p.ParameterType)} {p.Name}"));
                return $"{SafeName(mi.ReturnType)} {mi.Name}({ps})";
            case PropertyInfo pi:
                return $"{SafeName(pi.PropertyType)} {pi.Name} {{ {(pi.CanRead ? "get; " : "")}{(pi.CanWrite ? "set; " : "")}}}";
            case FieldInfo fi:
                return $"{SafeName(fi.FieldType)} {fi.Name}";
            case ConstructorInfo ci:
                var cps = string.Join(", ", ci.GetParameters().Select(p => $"{SafeName(p.ParameterType)} {p.Name}"));
                return $".ctor({cps})";
            default:
                return m.ToString() ?? m.Name;
        }
    }
    catch (Exception ex)
    {
        return $"{m.Name} <sig-unavailable: {ex.GetType().Name}>";
    }
}

static string SafeName(Type t)
{
    try { return t.FullName ?? t.Name; }
    catch { return "<type>"; }
}

static string SafeBase(Type t)
{
    try { return t.BaseType?.FullName ?? ""; }
    catch { return "<base-unavailable>"; }
}

switch (mode)
{
    case "types":
        foreach (var q in queries)
        {
            var re = new System.Text.RegularExpressions.Regex(q);
            foreach (var t in AllTypes().Where(t => re.IsMatch(t.FullName ?? t.Name)).OrderBy(t => t.FullName))
                sb.AppendLine($"{t.FullName}  [{(t.IsInterface ? "interface" : t.IsEnum ? "enum" : t.IsAbstract && t.IsSealed ? "static" : t.IsAbstract ? "abstract" : "class")}]{(t.BaseType != null ? " : " + SafeBase(t) : "")}");
        }
        break;
    case "type":
        foreach (var q in queries)
        {
            var t = AllTypes().FirstOrDefault(x => x.FullName == q);
            if (t == null) { sb.AppendLine($"### NOT FOUND {q}"); continue; }
            sb.AppendLine($"### {t.FullName}");
            sb.AppendLine($"  base: {SafeBase(t)}");
            sb.AppendLine($"  interfaces: {string.Join(", ", t.GetInterfaces().Select(i => i.Name))}");
            sb.AppendLine($"  isAbstract={t.IsAbstract} isSealed={t.IsSealed} isEnum={t.IsEnum}");
            if (t.IsEnum)
            {
                foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static)) sb.AppendLine($"  ENUM {f.Name} = {f.GetRawConstantValue()}");
            }
            sb.AppendLine("  --- ctors ---");
            foreach (var c in t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                sb.AppendLine($"    {(c.IsPublic ? "pub" : c.IsFamily ? "prot" : "priv")} {Sig(c)}");
            sb.AppendLine("  --- methods ---");
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                     .OrderBy(m => m.Name))
                sb.AppendLine($"    {(m.IsPublic ? "pub" : m.IsFamily ? "prot" : m.IsPrivate ? "priv" : "asm")}{(m.IsStatic ? " static" : "")}{(m.IsAbstract ? " abstract" : "")}{(m.IsVirtual ? " virtual" : "")} {Sig(m)}");
            sb.AppendLine("  --- properties ---");
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                sb.AppendLine($"    {Sig(p)}");
            sb.AppendLine("  --- fields ---");
            foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                sb.AppendLine($"    {(f.IsPublic ? "pub" : f.IsFamily ? "prot" : "priv")}{(f.IsStatic ? " static" : "")} {Sig(f)}");
        }
        break;
    case "members":
    case "search":
        foreach (var q in queries)
        {
            var re = new System.Text.RegularExpressions.Regex(q, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            foreach (var t in AllTypes().OrderBy(t => t.FullName))
            {
                foreach (var m in t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    var s = Sig(m);
                    if (re.IsMatch(t.FullName + "." + s))
                        sb.AppendLine($"{t.FullName}.{s}");
                }
            }
        }
        break;
}

File.WriteAllText(outFile, sb.ToString());
Console.WriteLine($"wrote {outFile} ({sb.Length} chars)");
