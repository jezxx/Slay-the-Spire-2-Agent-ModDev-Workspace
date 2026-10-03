// IlConstScan -- static metadata inspection helpers for sts2.dll.
//
// Usage:
//   IlConstScan.exe props     <asm> <out> <typeRegex> <propName>
//   IlConstScan.exe ctorconst <asm> <out> <typeRegex>
//   IlConstScan.exe enum      <asm> <out> <enumFullName>
//
// ctorconst decodes every instance constructor of matching types and prints,
// for each call instruction, the ldc.i4 constants that were pushed since the
// previous call. STS2 cards pass their CardType/CardRarity into
// CardModel::.ctor(...) as literal arguments, so this recovers CardType.
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;

if (args.Length < 4)
{
    Console.Error.WriteLine("usage: props|ctorconst|enum <asm> <out> ...");
    return 2;
}

string mode = args[0];
string asmPath = args[1];
string outFile = args[2];

Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outFile))!);

using var fs = File.OpenRead(asmPath);
using var pe = new PEReader(fs);
var md = pe.GetMetadataReader();
var sb = new StringBuilder();

var ownerOf = new Dictionary<int, string>();
foreach (var tdh in md.TypeDefinitions)
{
    var td = md.GetTypeDefinition(tdh);
    string full = FullName(md, td);
    foreach (var mh in td.GetMethods()) ownerOf[MetadataTokens.GetToken(mh)] = full;
}

if (mode == "enum")
{
    string want = args[3];
    foreach (var tdh in md.TypeDefinitions)
    {
        var td = md.GetTypeDefinition(tdh);
        string full = FullName(md, td);
        if (full != want) continue;
        sb.AppendLine($"== {full} ==");
        foreach (var fh in td.GetFields())
        {
            var fd = md.GetFieldDefinition(fh);
            string fn = md.GetString(fd.Name);
            if (fn == "value__") continue;
            var ch = fd.GetDefaultValue();
            if (ch.IsNil) { sb.AppendLine($"  {fn} = (no default)"); continue; }
            var c = md.GetConstant(ch);
            var br = md.GetBlobReader(c.Value);
            string cv = c.TypeCode switch
            {
                ConstantTypeCode.SByte => br.ReadSByte().ToString(),
                ConstantTypeCode.Byte => br.ReadByte().ToString(),
                ConstantTypeCode.Int16 => br.ReadInt16().ToString(),
                ConstantTypeCode.UInt16 => br.ReadUInt16().ToString(),
                ConstantTypeCode.Int32 => br.ReadInt32().ToString(),
                ConstantTypeCode.UInt32 => br.ReadUInt32().ToString(),
                ConstantTypeCode.Int64 => br.ReadInt64().ToString(),
                ConstantTypeCode.UInt64 => br.ReadUInt64().ToString(),
                _ => "(other:" + c.TypeCode + ")"
            };
            sb.AppendLine($"  {fn} = {cv}");
        }
    }
}
else if (mode == "props")
{
    var rx = new Regex(args[3], RegexOptions.Compiled);
    string propName = args[4];
    int hit = 0;
    foreach (var tdh in md.TypeDefinitions)
    {
        var td = md.GetTypeDefinition(tdh);
        if (td.IsNested) continue;
        string full = FullName(md, td);
        if (!rx.IsMatch(full)) continue;

        foreach (var ph in td.GetProperties())
        {
            var prop = md.GetPropertyDefinition(ph);
            if (md.GetString(prop.Name) != propName) continue;

            var acc = prop.GetAccessors();
            string ilHex = "";
            string decoded = "(no getter)";
            if (!acc.Getter.IsNil)
            {
                var mdef = md.GetMethodDefinition(acc.Getter);
                if (mdef.RelativeVirtualAddress != 0)
                {
                    var body = pe.GetMethodBody(mdef.RelativeVirtualAddress);
                    var il = body.GetILBytes();
                    ilHex = Convert.ToHexString(il);
                    decoded = DecodeConst(il);
                }
                else decoded = "(no body)";
            }
            sb.AppendLine($"{full}\t{decoded}\t{ilHex}");
            hit++;
        }
    }
    sb.AppendLine($"# total: {hit}");
}
else if (mode == "ctorconst")
{
    var rx = new Regex(args[3], RegexOptions.Compiled);
    var opTable = BuildOpTable();
    int hit = 0;
    foreach (var tdh in md.TypeDefinitions)
    {
        var td = md.GetTypeDefinition(tdh);
        if (td.IsNested) continue;
        string full = FullName(md, td);
        if (!rx.IsMatch(full)) continue;

        foreach (var mh in td.GetMethods())
        {
            var mdef = md.GetMethodDefinition(mh);
            if (md.GetString(mdef.Name) != ".ctor") continue;
            if (mdef.RelativeVirtualAddress == 0) continue;
            var body = pe.GetMethodBody(mdef.RelativeVirtualAddress);
            sb.AppendLine($"{full}\t{RenderCalls(md, ownerOf, body.GetILBytes(), opTable)}");
            hit++;
        }
    }
    sb.AppendLine($"# total: {hit}");
}
else
{
    Console.Error.WriteLine("unknown mode " + mode);
    return 2;
}

File.WriteAllText(outFile, sb.ToString(), new UTF8Encoding(false));
Console.WriteLine($"wrote {outFile} ({sb.Length} chars)");
return 0;

// ---------------------------------------------------------------- helpers

static string FullName(MetadataReader md, TypeDefinition td)
{
    string ns = md.GetString(td.Namespace);
    string n = md.GetString(td.Name);
    return ns.Length == 0 ? n : ns + "." + n;
}

static string DecodeConst(byte[] il)
{
    int start = 0;
    while (start < il.Length && il[start] == 0x00) start++;
    int stop = il.Length;
    while (stop > start && il[stop - 1] == 0x00) stop--;
    if (stop > start && il[stop - 1] == 0x2A) stop--;
    else return "?";

    int len = stop - start;
    if (len == 1)
    {
        byte op = il[start];
        if (op >= 0x16 && op <= 0x1E) return (op - 0x16).ToString();
        if (op == 0x15) return "-1";
        return "?";
    }
    if (len == 2 && il[start] == 0x1F) return ((sbyte)il[start + 1]).ToString();
    if (len == 5 && il[start] == 0x20) return BitConverter.ToInt32(il, start + 1).ToString();
    return "?";
}

static Dictionary<short, OperandType> BuildOpTable()
{
    var t = new Dictionary<short, OperandType>();
    foreach (var f in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
    {
        if (f.FieldType != typeof(OpCode)) continue;
        var oc = (OpCode)f.GetValue(null)!;
        t[oc.Value] = oc.OperandType;
    }
    return t;
}

static int OperandSize(OperandType ot, byte[] il, int p)
{
    switch (ot)
    {
        case OperandType.InlineNone: return 0;
        case OperandType.ShortInlineI:
        case OperandType.ShortInlineVar:
        case OperandType.ShortInlineBrTarget: return 1;
        case OperandType.InlineVar: return 2;
        case OperandType.InlineI:
        case OperandType.InlineBrTarget:
        case OperandType.InlineMethod:
        case OperandType.InlineField:
        case OperandType.InlineType:
        case OperandType.InlineString:
        case OperandType.InlineSig:
        case OperandType.InlineTok:
        case OperandType.ShortInlineR: return 4;
        case OperandType.InlineI8:
        case OperandType.InlineR: return 8;
        case OperandType.InlineSwitch:
            int n = BitConverter.ToInt32(il, p);
            return 4 + 4 * n;
        default: return 0;
    }
}

static string RenderCalls(MetadataReader md, Dictionary<int, string> ownerOf, byte[] il, Dictionary<short, OperandType> opTable)
{
    var parts = new List<string>();
    var pending = new List<long>();
    int p = 0;
    while (p < il.Length)
    {
        short op;
        byte b = il[p];
        if (b == 0xFE)
        {
            op = (short)(0xFE00 | il[p + 1]);
            p += 2;
        }
        else
        {
            op = b;
            p += 1;
        }
        if (!opTable.TryGetValue(op, out var ot)) break;
        int operandPos = p;
        int size = OperandSize(ot, il, p);
        p += size;

        if (op == 0x15) pending.Add(-1);                       // ldc.i4.m1
        else if (op >= 0x16 && op <= 0x1E) pending.Add(op - 0x16); // ldc.i4.0..8
        else if (op == 0x1F) pending.Add((sbyte)il[operandPos]);   // ldc.i4.s
        else if (op == 0x20) pending.Add(BitConverter.ToInt32(il, operandPos)); // ldc.i4
        else if (op == 0x28 || op == 0x6F)                      // call / callvirt
        {
            int token = BitConverter.ToInt32(il, operandPos);
            string name = ResolveMethod(md, ownerOf, token);
            parts.Add($"{name}=({string.Join(",", pending)})");
            pending.Clear();
        }
        else if (op == 0x2A) break;                             // ret
    }
    return parts.Count == 0 ? "(no calls)" : string.Join(" | ", parts);
}

static string ResolveMethod(MetadataReader md, Dictionary<int, string> ownerOf, int token)
{
    if (token == 0) return "?";
    try
    {
        var h = MetadataTokens.EntityHandle(token);
        if (h.Kind == HandleKind.MethodDefinition)
        {
            int t = token;
            return (ownerOf.TryGetValue(t, out var owner) ? owner : "?") + "::" + md.GetString(md.GetMethodDefinition((MethodDefinitionHandle)h).Name);
        }
        if (h.Kind == HandleKind.MemberReference)
        {
            var mr = md.GetMemberReference((MemberReferenceHandle)h);
            string parentName = "?";
            if (mr.Parent.Kind == HandleKind.TypeReference)
            {
                var tr = md.GetTypeReference((TypeReferenceHandle)mr.Parent);
                string ns = md.GetString(tr.Namespace);
                string n = md.GetString(tr.Name);
                parentName = ns.Length == 0 ? n : ns + "." + n;
            }
            else if (mr.Parent.Kind == HandleKind.TypeDefinition)
            {
                parentName = FullName(md, md.GetTypeDefinition((TypeDefinitionHandle)mr.Parent));
            }
            else if (mr.Parent.Kind == HandleKind.TypeSpecification)
            {
                parentName = "typespec";
            }
            return parentName + "::" + md.GetString(mr.Name);
        }
        if (h.Kind == HandleKind.MethodSpecification)
        {
            var ms = md.GetMethodSpecification((MethodSpecificationHandle)h);
            return ResolveMethod(md, ownerOf, MetadataTokens.GetToken(ms.Method)) + "<>";
        }
    }
    catch { }
    return "tok:" + token.ToString("X8");
}
