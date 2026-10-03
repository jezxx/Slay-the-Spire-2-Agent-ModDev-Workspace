using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;

// 反编译指定方法的 IL 到文本，用来搞清楚游戏内部实现（只读、不加载程序集）。
// 用法: IlDump <assemblyPath> <outFile> <TypeFullName> [methodNamePrefix ...]
//       IlDump <assemblyPath> <outFile> --list <TypeFullName>
//       IlDump <assemblyPath> <outFile> --types [nameFragment]
//       （--types 只列类型名，nameFragment 可选，用于先定位命名空间）

if (args.Length < 3) { Console.Error.WriteLine("usage: IlDump <asm> <out> <Type> [methodPrefix...] | <asm> <out> --list <Type> | <asm> <out> --types [fragment]"); return 1; }

var asmPath = args[0];
var outPath = args[1];

// --types 模式：只列类型名。arg[2] 必须是 --types，fragment 可选。
if (args[2] == "--types")
{
    string frag = args.Length > 3 ? args[3] : "";
    using var tfs = File.OpenRead(asmPath);
    using var tpe = new PEReader(tfs);
    var tmd = tpe.GetMetadataReader();
    var tsb = new StringBuilder();
    int tn = 0;
    foreach (var th in tmd.TypeDefinitions)
    {
        var t = tmd.GetTypeDefinition(th);
        string tns = t.Namespace.IsNil ? "" : tmd.GetString(t.Namespace);
        string name = tmd.GetString(t.Name);
        string full = string.IsNullOrEmpty(tns) ? name : tns + "." + name;
        if (frag.Length > 0 && full.IndexOf(frag, StringComparison.OrdinalIgnoreCase) < 0) continue;
        tsb.AppendLine(full);
        tn++;
    }
    File.WriteAllText(outPath, tsb.ToString());
    Console.Error.WriteLine($"types written: {tn}");
    return 0;
}

if (args.Length < 4) { Console.Error.WriteLine("usage: IlDump <asm> <out> <Type> [methodPrefix...] | <asm> <out> --list <Type> | <asm> <out> --types [fragment]"); return 1; }

var typeName = args[2];
var listOnly = args[3] == "--list" && args.Length == 4;
// 非 --list 形式：第 3 个参数起是方法名前缀；--list 形式下被类型名占用了那一格
var prefixes = listOnly ? Array.Empty<string>() : args.Skip(3).ToArray();
if (!listOnly && prefixes.Length == 1 && prefixes[0] == "--list") prefixes = Array.Empty<string>();

using var fs = File.OpenRead(asmPath);
using var pe = new PEReader(fs);
var md = pe.GetMetadataReader();

// 建 type -> MethodDefinitionHandle 的索引，以及字符串堆解析
string Str(StringHandle h) => h.IsNil ? "" : md.GetString(h);

TypeDefinition? FindType(string fullName)
{
    // 显式嵌套类型语法：Outer+Nested（Nested 可只给片段，如 "d__43"）
    if (fullName.Contains('+'))
    {
        var parts = fullName.Split('+', 2);
        var outer = FindTypeByName(parts[0]);
        if (outer != null)
        {
            var want = parts[1];
            foreach (var nh in outer.Value.GetNestedTypes())
            {
                var nt = md.GetTypeDefinition(nh);
                var nn = Str(nt.Name);
                if (nn == want || nn.Contains(want.TrimStart('<'))) return nt;
            }
        }
        return null;
    }

    foreach (var h in md.TypeDefinitions)
    {
        var t = md.GetTypeDefinition(h);
        var ns = Str(t.Namespace);
        var n = Str(t.Name);
        var full = string.IsNullOrEmpty(ns) ? n : ns + "." + n;
        if (full == fullName) return t;
        // 允许用名字片段匹配编译器生成的状态机，例如 "d__43"
        if (fullName.StartsWith("d__") && n.Contains(fullName)) return t;
    }
    return null;
}

TypeDefinition? FindTypeByName(string fullName)
{
    foreach (var h in md.TypeDefinitions)
    {
        var t = md.GetTypeDefinition(h);
        var ns = Str(t.Namespace);
        var n = Str(t.Name);
        var full = string.IsNullOrEmpty(ns) ? n : ns + "." + n;
        if (full == fullName) return t;
    }
    return null;
}

var sb = new StringBuilder();
var td = FindType(typeName);
if (td == null) { File.WriteAllText(outPath, $"type not found: {typeName}\n"); Console.Error.WriteLine("type not found"); return 1; }
var type = td.Value;

// 收集嵌套类型一起处理
var types = new List<TypeDefinition> { type };
foreach (var nh in type.GetNestedTypes())
{
    types.Add(md.GetTypeDefinition(nh));
}

int dumped = 0;
foreach (var t in types)
{
    var tns = Str(t.Namespace);
    var tn = Str(t.Name);
    var tfull = string.IsNullOrEmpty(tns) ? tn : tns + "." + tn;
    if (listOnly)
    {
        sb.AppendLine($"### {tfull}");
        foreach (var mh in t.GetMethods())
        {
            var m = md.GetMethodDefinition(mh);
            sb.AppendLine($"  {Str(m.Name)}");
        }
        continue;
    }

    foreach (var mh in t.GetMethods())
    {
        var m = md.GetMethodDefinition(mh);
        var name = Str(m.Name);
        // 匹配用前缀 + 包含两种：编译器生成的方法名形如 "<Process>b__0"，
        // 用前缀匹配会漏掉（所以要支持按包含匹配）。
        if (!prefixes.Any(p => name.StartsWith(p, StringComparison.Ordinal) || name.Contains(p, StringComparison.Ordinal))) continue;

        var parms = m.GetParameters().Select(ph => md.GetParameter(ph)).ToArray();
        var sig = string.Join(", ", parms.Select(p => $"{Str(p.Name)}"));
        sb.AppendLine($"===== {tfull}.{name}({sig})  [attrs=0x{(int)m.Attributes:X}] =====");

        int rva = m.RelativeVirtualAddress;
        if (rva == 0) { sb.AppendLine("  <no body / abstract>"); sb.AppendLine(); continue; }
        var body = pe.GetMethodBody(rva);
        if (body == null) { sb.AppendLine("  <no body / abstract>"); sb.AppendLine(); continue; }

        sb.AppendLine($"  maxStack={body.MaxStack} locals={body.LocalSignature}");
        var il = body.GetILBytes();
        sb.AppendLine("  -- IL bytes --");
        sb.AppendLine("  " + BitConverter.ToString(il).Replace("-", " "));
        sb.AppendLine("  -- decoded --");
        sb.AppendLine(DecodeIl(il));
        sb.AppendLine();
        dumped++;
    }
}

File.WriteAllText(outPath, sb.ToString());
Console.WriteLine($"wrote {outPath} ({sb.Length} chars), methods dumped={dumped}");
return 0;

// 极简 IL 解码：把操作码 + 操作数打印出来（不是完整反编译，但足够看调用链和常量）
string DecodeIl(byte[] il)
{
    var o = new StringBuilder();
    int i = 0;
    while (i < il.Length)
    {
        int start = i;
        var (opName, operandSize, operandKind) = ReadOp(il, ref i);
        string operand = "";
        try
        {
            switch (operandKind)
            {
                case "none": break;
                case "i1": operand = ((sbyte)il[i]).ToString(); i += 1; break;
                case "u1": operand = il[i].ToString(); i += 1; break;
                case "i4":
                    operand = BitConverter.ToInt32(il, i).ToString(); i += 4; break;
                case "i8":
                    operand = BitConverter.ToInt64(il, i).ToString(); i += 8; break;
                case "r4":
                    operand = BitConverter.ToSingle(il, i).ToString("R"); i += 4; break;
                case "r8":
                    operand = BitConverter.ToDouble(il, i).ToString("R"); i += 8; break;
                case "str":
                    {
                        var tok = BitConverter.ToInt32(il, i); i += 4;
                        operand = $"\"{ResolveString(tok)}\"";
                        break;
                    }
                case "method":
                    {
                        var tok = BitConverter.ToInt32(il, i); i += 4;
                        operand = ResolveMember(tok, method: true);
                        break;
                    }
                case "field":
                    {
                        var tok = BitConverter.ToInt32(il, i); i += 4;
                        operand = ResolveMember(tok, method: false);
                        break;
                    }
                case "type":
                    {
                        var tok = BitConverter.ToInt32(il, i); i += 4;
                        operand = ResolveType(tok);
                        break;
                    }
                case "tok":
                    {
                        var tok = BitConverter.ToInt32(il, i); i += 4;
                        operand = $"tok(0x{tok:X8})";
                        break;
                    }
                case "switch":
                    {
                        var n = BitConverter.ToInt32(il, i); i += 4;
                        var targets = new List<int>();
                        for (int k = 0; k < n; k++) { targets.Add(BitConverter.ToInt32(il, i)); i += 4; }
                        operand = $"({n}) -> " + string.Join(", ", targets);
                        break;
                    }
                case "br":
                    {
                        var d = BitConverter.ToInt32(il, i); i += 4;
                        operand = $"IL_{i + d:X4}";
                        break;
                    }
                case "brs":
                    {
                        var d = (sbyte)il[i]; i += 1;
                        operand = $"IL_{i + d:X4}";
                        break;
                    }
            }
        }
        catch (Exception ex) { operand = $"<err {ex.Message}>"; }
        o.AppendLine($"    IL_{start:X4}: {opName,-14} {operand}");
    }
    return o.ToString();
}

string ResolveString(int tok)
{
    try { return md.GetUserString(MetadataTokens.UserStringHandle(tok)); } catch { return $"<str 0x{tok:X8}>"; }
}

string ResolveType(int tok)
{
    try
    {
        var h = MetadataTokens.EntityHandle(tok);
        if (h.Kind == HandleKind.TypeReference)
        {
            var tr = md.GetTypeReference((TypeReferenceHandle)h);
            var ns = Str(tr.Namespace); var n = Str(tr.Name);
            return string.IsNullOrEmpty(ns) ? n : ns + "." + n;
        }
        if (h.Kind == HandleKind.TypeDefinition)
        {
            var t = md.GetTypeDefinition((TypeDefinitionHandle)h);
            var ns = Str(t.Namespace); var n = Str(t.Name);
            return string.IsNullOrEmpty(ns) ? n : ns + "." + n;
        }
        if (h.Kind == HandleKind.TypeSpecification)
            return "<typespec>";
    }
    catch { }
    return $"<type 0x{tok:X8}>";
}

string ResolveMember(int tok, bool method)
{
    try
    {
        var h = MetadataTokens.EntityHandle(tok);
        if (h.Kind == HandleKind.MemberReference)
        {
            var mr = md.GetMemberReference((MemberReferenceHandle)h);
            var parent = ResolveType(MetadataTokens.GetToken(mr.Parent));
            return $"{parent}::{Str(mr.Name)}";
        }
        if (h.Kind == HandleKind.MethodDefinition)
        {
            var m = md.GetMethodDefinition((MethodDefinitionHandle)h);
            var dt = md.GetTypeDefinition(m.GetDeclaringType());
            return $"{Str(dt.Namespace)}.{Str(dt.Name)}::{Str(m.Name)}";
        }
        if (h.Kind == HandleKind.FieldDefinition)
        {
            var f = md.GetFieldDefinition((FieldDefinitionHandle)h);
            var dt = md.GetTypeDefinition(f.GetDeclaringType());
            return $"{Str(dt.Namespace)}.{Str(dt.Name)}::{Str(f.Name)}";
        }
        if (h.Kind == HandleKind.MethodSpecification)
        {
            var ms = md.GetMethodSpecification((MethodSpecificationHandle)h);
            return ResolveMember(MetadataTokens.GetToken(ms.Method), true) + "<...>";
        }
    }
    catch { }
    return method ? $"<method 0x{tok:X8}>" : $"<field 0x{tok:X8}>";
}

// 操作码表：返回 (名字, 操作数长度, 操作数类型)
static (string, int, string) ReadOp(byte[] il, ref int i)
{
    byte b = il[i++];
    if (b == 0xFE)
    {
        byte b2 = il[i++];
        return b2 switch
        {
            0x01 => ("ceq", 0, "none"),
            0x02 => ("cgt", 0, "none"),
            0x03 => ("cgt.un", 0, "none"),
            0x04 => ("clt", 0, "none"),
            0x05 => ("clt.un", 0, "none"),
            0x06 => ("ldftn", 4, "method"),
            0x07 => ("ldvirtftn", 4, "method"),
            0x09 => ("ldarg", 2, "u1"),
            0x0A => ("ldarga", 2, "u1"),
            0x0B => ("starg", 2, "u1"),
            0x0C => ("ldloc", 2, "u1"),
            0x0D => ("ldloca", 2, "u1"),
            0x0E => ("stloc", 2, "u1"),
            0x0F => ("localloc", 0, "none"),
            0x11 => ("endfilter", 0, "none"),
            0x12 => ("unaligned.", 1, "u1"),
            0x13 => ("volatile.", 0, "none"),
            0x14 => ("tail.", 0, "none"),
            0x15 => ("initobj", 4, "type"),
            0x16 => ("constrained.", 4, "type"),
            0x17 => ("cpblk", 0, "none"),
            0x18 => ("initblk", 0, "none"),
            0x19 => ("no.", 1, "u1"),
            0x1A => ("rethrow", 0, "none"),
            0x1C => ("sizeof", 4, "type"),
            0x1D => ("refanytype", 0, "none"),
            0x1E => ("readonly.", 0, "none"),
            _ => ($"fe{b2:X2}", 0, "none")
        };
    }
    return b switch
    {
        0x00 => ("nop", 0, "none"),
        0x01 => ("break", 0, "none"),
        0x02 => ("ldarg.0", 0, "none"),
        0x03 => ("ldarg.1", 0, "none"),
        0x04 => ("ldarg.2", 0, "none"),
        0x05 => ("ldarg.3", 0, "none"),
        0x06 => ("ldloc.0", 0, "none"),
        0x07 => ("ldloc.1", 0, "none"),
        0x08 => ("ldloc.2", 0, "none"),
        0x09 => ("ldloc.3", 0, "none"),
        0x0A => ("stloc.0", 0, "none"),
        0x0B => ("stloc.1", 0, "none"),
        0x0C => ("stloc.2", 0, "none"),
        0x0D => ("stloc.3", 0, "none"),
        0x0E => ("ldarg.s", 1, "u1"),
        0x0F => ("ldarga.s", 1, "u1"),
        0x10 => ("starg.s", 1, "u1"),
        0x11 => ("ldloc.s", 1, "u1"),
        0x12 => ("ldloca.s", 1, "u1"),
        0x13 => ("stloc.s", 1, "u1"),
        0x14 => ("ldnull", 0, "none"),
        0x15 => ("ldc.i4.m1", 0, "none"),
        0x16 => ("ldc.i4.0", 0, "none"),
        0x17 => ("ldc.i4.1", 0, "none"),
        0x18 => ("ldc.i4.2", 0, "none"),
        0x19 => ("ldc.i4.3", 0, "none"),
        0x1A => ("ldc.i4.4", 0, "none"),
        0x1B => ("ldc.i4.5", 0, "none"),
        0x1C => ("ldc.i4.6", 0, "none"),
        0x1D => ("ldc.i4.7", 0, "none"),
        0x1E => ("ldc.i4.8", 0, "none"),
        0x1F => ("ldc.i4.s", 1, "i1"),
        0x20 => ("ldc.i4", 4, "i4"),
        0x21 => ("ldc.i8", 8, "i8"),
        0x22 => ("ldc.r4", 4, "r4"),
        0x23 => ("ldc.r8", 8, "r8"),
        0x25 => ("dup", 0, "none"),
        0x26 => ("pop", 0, "none"),
        0x27 => ("jmp", 4, "method"),
        0x28 => ("call", 4, "method"),
        0x29 => ("calli", 4, "tok"),
        0x2A => ("ret", 0, "none"),
        0x2B => ("br.s", 1, "brs"),
        0x2C => ("brfalse.s", 1, "brs"),
        0x2D => ("brtrue.s", 1, "brs"),
        0x2E => ("beq.s", 1, "brs"),
        0x2F => ("bge.s", 1, "brs"),
        0x30 => ("bgt.s", 1, "brs"),
        0x31 => ("ble.s", 1, "brs"),
        0x32 => ("blt.s", 1, "brs"),
        0x33 => ("bne.un.s", 1, "brs"),
        0x34 => ("bge.un.s", 1, "brs"),
        0x35 => ("bgt.un.s", 1, "brs"),
        0x36 => ("ble.un.s", 1, "brs"),
        0x37 => ("blt.un.s", 1, "brs"),
        0x38 => ("br", 4, "br"),
        0x39 => ("brfalse", 4, "br"),
        0x3A => ("brtrue", 4, "br"),
        0x3B => ("beq", 4, "br"),
        0x3C => ("bge", 4, "br"),
        0x3D => ("bgt", 4, "br"),
        0x3E => ("ble", 4, "br"),
        0x3F => ("blt", 4, "br"),
        0x40 => ("bne.un", 4, "br"),
        0x41 => ("bge.un", 4, "br"),
        0x42 => ("bgt.un", 4, "br"),
        0x43 => ("ble.un", 4, "br"),
        0x44 => ("blt.un", 4, "br"),
        0x45 => ("switch", -1, "switch"),
        0x46 => ("ldind.i1", 0, "none"),
        0x47 => ("ldind.u1", 0, "none"),
        0x48 => ("ldind.i2", 0, "none"),
        0x49 => ("ldind.u2", 0, "none"),
        0x4A => ("ldind.i4", 0, "none"),
        0x4B => ("ldind.u4", 0, "none"),
        0x4C => ("ldind.i8", 0, "none"),
        0x4D => ("ldind.i", 0, "none"),
        0x4E => ("ldind.r4", 0, "none"),
        0x4F => ("ldind.r8", 0, "none"),
        0x50 => ("ldind.ref", 0, "none"),
        0x51 => ("stind.ref", 0, "none"),
        0x52 => ("stind.i1", 0, "none"),
        0x53 => ("stind.i2", 0, "none"),
        0x54 => ("stind.i4", 0, "none"),
        0x55 => ("stind.i8", 0, "none"),
        0x56 => ("stind.r4", 0, "none"),
        0x57 => ("stind.r8", 0, "none"),
        0x58 => ("add", 0, "none"),
        0x59 => ("sub", 0, "none"),
        0x5A => ("mul", 0, "none"),
        0x5B => ("div", 0, "none"),
        0x5C => ("div.un", 0, "none"),
        0x5D => ("rem", 0, "none"),
        0x5E => ("rem.un", 0, "none"),
        0x5F => ("and", 0, "none"),
        0x60 => ("or", 0, "none"),
        0x61 => ("xor", 0, "none"),
        0x62 => ("shl", 0, "none"),
        0x63 => ("shr", 0, "none"),
        0x64 => ("shr.un", 0, "none"),
        0x65 => ("neg", 0, "none"),
        0x66 => ("not", 0, "none"),
        0x67 => ("conv.i1", 0, "none"),
        0x68 => ("conv.i2", 0, "none"),
        0x69 => ("conv.i4", 0, "none"),
        0x6A => ("conv.i8", 0, "none"),
        0x6B => ("conv.r4", 0, "none"),
        0x6C => ("conv.r8", 0, "none"),
        0x6D => ("conv.u4", 0, "none"),
        0x6E => ("conv.u8", 0, "none"),
        0x6F => ("callvirt", 4, "method"),
        0x70 => ("cpobj", 4, "type"),
        0x71 => ("ldobj", 4, "type"),
        0x72 => ("ldstr", 4, "str"),
        0x73 => ("newobj", 4, "method"),
        0x74 => ("castclass", 4, "type"),
        0x75 => ("isinst", 4, "type"),
        0x76 => ("conv.r.un", 0, "none"),
        0x79 => ("unbox", 4, "type"),
        0x7A => ("throw", 0, "none"),
        0x7B => ("ldfld", 4, "field"),
        0x7C => ("ldflda", 4, "field"),
        0x7D => ("stfld", 4, "field"),
        0x7E => ("ldsfld", 4, "field"),
        0x7F => ("ldsflda", 4, "field"),
        0x80 => ("stsfld", 4, "field"),
        0x81 => ("stobj", 4, "type"),
        0x82 => ("conv.ovf.i1.un", 0, "none"),
        0x83 => ("conv.ovf.i2.un", 0, "none"),
        0x84 => ("conv.ovf.i4.un", 0, "none"),
        0x85 => ("conv.ovf.i8.un", 0, "none"),
        0x86 => ("conv.ovf.u1.un", 0, "none"),
        0x87 => ("conv.ovf.u2.un", 0, "none"),
        0x88 => ("conv.ovf.u4.un", 0, "none"),
        0x89 => ("conv.ovf.u8.un", 0, "none"),
        0x8A => ("conv.ovf.i.un", 0, "none"),
        0x8B => ("conv.ovf.u.un", 0, "none"),
        0x8C => ("box", 4, "type"),
        0x8D => ("newarr", 4, "type"),
        0x8E => ("ldlen", 0, "none"),
        0x8F => ("ldelema", 4, "type"),
        0x90 => ("ldelem.i1", 0, "none"),
        0x91 => ("ldelem.u1", 0, "none"),
        0x92 => ("ldelem.i2", 0, "none"),
        0x93 => ("ldelem.u2", 0, "none"),
        0x94 => ("ldelem.i4", 0, "none"),
        0x95 => ("ldelem.u4", 0, "none"),
        0x96 => ("ldelem.i8", 0, "none"),
        0x97 => ("ldelem.i", 0, "none"),
        0x98 => ("ldelem.r4", 0, "none"),
        0x99 => ("ldelem.r8", 0, "none"),
        0x9A => ("ldelem.ref", 0, "none"),
        0x9B => ("stelem.i", 0, "none"),
        0x9C => ("stelem.i1", 0, "none"),
        0x9D => ("stelem.i2", 0, "none"),
        0x9E => ("stelem.i4", 0, "none"),
        0x9F => ("stelem.i8", 0, "none"),
        0xA0 => ("stelem.r4", 0, "none"),
        0xA1 => ("stelem.r8", 0, "none"),
        0xA2 => ("stelem.ref", 0, "none"),
        0xA3 => ("ldelem", 4, "type"),
        0xA4 => ("stelem", 4, "type"),
        0xA5 => ("unbox.any", 4, "type"),
        0xB3 => ("conv.ovf.i1", 0, "none"),
        0xB4 => ("conv.ovf.u1", 0, "none"),
        0xB5 => ("conv.ovf.i2", 0, "none"),
        0xB6 => ("conv.ovf.u2", 0, "none"),
        0xB7 => ("conv.ovf.i4", 0, "none"),
        0xB8 => ("conv.ovf.u4", 0, "none"),
        0xB9 => ("conv.ovf.i8", 0, "none"),
        0xBA => ("conv.ovf.u8", 0, "none"),
        0xC2 => ("refanyval", 4, "type"),
        0xC3 => ("ckfinite", 0, "none"),
        0xC6 => ("mkrefany", 4, "type"),
        0xD0 => ("ldtoken", 4, "tok"),
        0xD1 => ("conv.u2", 0, "none"),
        0xD2 => ("conv.u1", 0, "none"),
        0xD3 => ("conv.i", 0, "none"),
        0xD4 => ("conv.ovf.i", 0, "none"),
        0xD5 => ("conv.ovf.u", 0, "none"),
        0xD6 => ("add.ovf", 0, "none"),
        0xD7 => ("add.ovf.un", 0, "none"),
        0xD8 => ("mul.ovf", 0, "none"),
        0xD9 => ("mul.ovf.un", 0, "none"),
        0xDA => ("sub.ovf", 0, "none"),
        0xDB => ("sub.ovf.un", 0, "none"),
        0xDC => ("endfinally", 0, "none"),
        0xDD => ("leave", 4, "br"),
        0xDE => ("leave.s", 1, "brs"),
        0xDF => ("stind.i", 0, "none"),
        0xE0 => ("conv.u", 0, "none"),
        _ => ($"op{b:X2}", 0, "none")
    };
}
