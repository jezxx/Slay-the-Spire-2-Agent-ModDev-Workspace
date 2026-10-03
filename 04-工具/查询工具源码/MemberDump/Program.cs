using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;

// MemberDump: dump a type's members with full decoded signatures, including
// enum literal values (which IlDump's --list mode omits).
// usage: MemberDump <asm> <out> <TypeFullName> [TypeFullName2 ...]

if (args.Length < 3) { Console.Error.WriteLine("usage: MemberDump <asm> <out> <Type> [Type...]"); return 1; }

var asmPath = args[0];
var outPath = args[1];
var wanted = args.Skip(2).ToArray();

using var fs = File.OpenRead(asmPath);
using var pe = new PEReader(fs);
var md = pe.GetMetadataReader();
var sb = new StringBuilder();
var Provider = new TypeProvider();

string Str(StringHandle h) => h.IsNil ? "" : md.GetString(h);

string TypeName(EntityHandle h)
{
    switch (h.Kind)
    {
        case HandleKind.TypeDefinition:
            {
                var t = md.GetTypeDefinition((TypeDefinitionHandle)h);
                var ns = Str(t.Namespace); var n = Str(t.Name);
                return string.IsNullOrEmpty(ns) ? n : ns + "." + n;
            }
        case HandleKind.TypeReference:
            {
                var t = md.GetTypeReference((TypeReferenceHandle)h);
                var ns = Str(t.Namespace); var n = Str(t.Name);
                return string.IsNullOrEmpty(ns) ? n : ns + "." + n;
            }
        case HandleKind.TypeSpecification:
            {
                var ts = md.GetTypeSpecification((TypeSpecificationHandle)h);
                return ts.DecodeSignature(Provider, null);
            }
        default: return "";
    }
}

string TypeFullName(TypeDefinition t)
{
    var ns = Str(t.Namespace);
    var n = Str(t.Name);
    return string.IsNullOrEmpty(ns) ? n : ns + "." + n;
}

foreach (var want in wanted)
{
    TypeDefinition? found = null;
    foreach (var h in md.TypeDefinitions)
    {
        var t = md.GetTypeDefinition(h);
        var full = TypeFullName(t);
        if (full == want) { found = t; break; }
        if (want.Contains('+') && string.Equals(full, want.Split('+')[0], StringComparison.Ordinal))
        {
            var nestedName = want.Split('+', 2)[1];
            foreach (var nh in t.GetNestedTypes())
            {
                var nt = md.GetTypeDefinition(nh);
                if (Str(nt.Name) == nestedName) { found = nt; break; }
            }
            if (found != null) break;
        }
    }
    if (found == null) { sb.AppendLine($"### TYPE NOT FOUND: {want}"); sb.AppendLine(); continue; }

    var td = found.Value;
    var attrs = td.Attributes;
    string kind;
    if ((attrs & TypeAttributes.Interface) != 0) kind = "interface";
    else if ((attrs & TypeAttributes.Abstract) != 0 && (attrs & TypeAttributes.Sealed) != 0) kind = "static class";
    else if ((attrs & TypeAttributes.Abstract) != 0) kind = "abstract class";
    else if ((attrs & TypeAttributes.Sealed) != 0) kind = "sealed class";
    else kind = "class";

    sb.AppendLine($"### {TypeFullName(td)}   [{kind}]");
    sb.AppendLine($"  base: {TypeName(td.BaseType)}");
    var ifaceNames = new List<string>();
    foreach (var ih in td.GetInterfaceImplementations())
        ifaceNames.Add(TypeName(md.GetInterfaceImplementation(ih).Interface));
    sb.AppendLine($"  interfaces: {string.Join(", ", ifaceNames)}");
    sb.AppendLine();

    // ---- enum literals ----
    var enumFields = new List<string>();
    foreach (var fh in td.GetFields())
    {
        var f = md.GetFieldDefinition(fh);
        if ((f.Attributes & FieldAttributes.Literal) == 0) continue;
        string val = "?";
        if (!f.GetDefaultValue().IsNil)
        {
            var c = md.GetConstant(f.GetDefaultValue());
            var br = md.GetBlobReader(c.Value);
            try
            {
                switch (c.TypeCode)
                {
                    case ConstantTypeCode.SByte: val = br.ReadSByte().ToString(); break;
                    case ConstantTypeCode.Byte: val = br.ReadByte().ToString(); break;
                    case ConstantTypeCode.Int16: val = br.ReadInt16().ToString(); break;
                    case ConstantTypeCode.UInt16: val = br.ReadUInt16().ToString(); break;
                    case ConstantTypeCode.Int32: val = br.ReadInt32().ToString(); break;
                    case ConstantTypeCode.UInt32: val = br.ReadUInt32().ToString(); break;
                    case ConstantTypeCode.Int64: val = br.ReadInt64().ToString(); break;
                    case ConstantTypeCode.UInt64: val = br.ReadUInt64().ToString(); break;
                    default: val = c.TypeCode.ToString(); break;
                }
            }
            catch { val = "?"; }
        }
        enumFields.Add($"    {Str(f.Name)} = {val}");
    }
    if (enumFields.Count > 0)
    {
        sb.AppendLine("  --- literal fields (enum values) ---");
        foreach (var l in enumFields) sb.AppendLine(l);
        sb.AppendLine();
    }

    // ---- non-literal fields ----
    var fieldLines = new List<string>();
    foreach (var fh in td.GetFields())
    {
        var f = md.GetFieldDefinition(fh);
        if ((f.Attributes & FieldAttributes.Literal) != 0) continue;
        var acc = Acc(f.Attributes);
        var st = (f.Attributes & FieldAttributes.Static) != 0 ? "static " : "";
        var ro = (f.Attributes & FieldAttributes.InitOnly) != 0 ? "readonly " : "";
        fieldLines.Add($"    {acc}{st}{ro}{f.DecodeSignature(Provider, null)} {Str(f.Name)}");
    }
    if (fieldLines.Count > 0)
    {
        sb.AppendLine("  --- fields ---");
        foreach (var l in fieldLines) sb.AppendLine(l);
        sb.AppendLine();
    }

    // ---- methods ----
    var ctorLines = new List<string>();
    var methodLines = new List<string>();
    foreach (var mh in td.GetMethods())
    {
        var m = md.GetMethodDefinition(mh);
        var name = Str(m.Name);
        var sig = m.DecodeSignature(Provider, null);
        var ps = string.Join(", ", sig.ParameterTypes);
        var ma = m.Attributes;
        var isCtor = name == ".ctor" || name == ".cctor";
        var vis = Vis(ma);
        var flags = new List<string>();
        if (!isCtor)
        {
            if ((ma & MethodAttributes.Abstract) != 0) flags.Add("abstract");
            if ((ma & MethodAttributes.Virtual) != 0)
            {
                if ((ma & MethodAttributes.NewSlot) != 0) flags.Add("virtual(newslot)");
                else if ((ma & MethodAttributes.Final) != 0) flags.Add("sealed-override");
                else flags.Add("OVERRIDE");
            }
            if ((ma & MethodAttributes.Static) != 0) flags.Add("static");
        }
        var flagStr = flags.Count > 0 ? "[" + string.Join(",", flags) + "] " : "";
        var line = isCtor
            ? $"    {vis} {name}({ps})"
            : $"    {vis} {flagStr}{sig.ReturnType} {name}({ps})";
        if (isCtor) ctorLines.Add(line); else methodLines.Add(line);
    }
    if (ctorLines.Count > 0)
    {
        sb.AppendLine("  --- ctors ---");
        foreach (var l in ctorLines) sb.AppendLine(l);
        sb.AppendLine();
    }
    if (methodLines.Count > 0)
    {
        sb.AppendLine("  --- methods ---");
        foreach (var l in methodLines) sb.AppendLine(l);
        sb.AppendLine();
    }

    // ---- properties ----
    var propLines = new List<string>();
    foreach (var ph in td.GetProperties())
    {
        var p = md.GetPropertyDefinition(ph);
        var acc = p.GetAccessors();
        var sig = p.DecodeSignature(Provider, null);
        var access = "";
        if (!acc.Getter.IsNil)
        {
            var gm = md.GetMethodDefinition(acc.Getter);
            access += AccessorFlags(gm.Attributes) + "get; ";
        }
        if (!acc.Setter.IsNil)
        {
            var sm = md.GetMethodDefinition(acc.Setter);
            access += AccessorFlags(sm.Attributes) + "set; ";
        }
        propLines.Add($"    {sig.ReturnType} {Str(p.Name)} {{ {access}}}");
    }
    if (propLines.Count > 0)
    {
        sb.AppendLine("  --- properties ---");
        foreach (var l in propLines) sb.AppendLine(l);
        sb.AppendLine();
    }

    var nested = td.GetNestedTypes().Select(nh => Str(md.GetTypeDefinition(nh).Name)).ToArray();
    if (nested.Length > 0)
        sb.AppendLine($"  --- nested types --- {string.Join(", ", nested)}");
    sb.AppendLine();
}

File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(false));
Console.WriteLine($"wrote {outPath} (types: {wanted.Length})");
return 0;

static string AccessorFlags(MethodAttributes a)
{
    if ((a & MethodAttributes.Virtual) == 0) return "";
    if ((a & MethodAttributes.NewSlot) != 0) return "[virtual] ";
    if ((a & MethodAttributes.Final) != 0) return "[sealed-override] ";
    return "[OVERRIDE] ";
}

static string Acc(FieldAttributes a) => (a & FieldAttributes.FieldAccessMask) switch
{
    FieldAttributes.Public => "pub ",
    FieldAttributes.Private => "priv ",
    FieldAttributes.Family => "prot ",
    FieldAttributes.Assembly => "internal ",
    FieldAttributes.FamORAssem => "protint ",
    FieldAttributes.FamANDAssem => "privprot ",
    _ => ""
};

static string Vis(MethodAttributes a) => (a & MethodAttributes.MemberAccessMask) switch
{
    MethodAttributes.Public => "pub",
    MethodAttributes.Private => "priv",
    MethodAttributes.Family => "prot",
    MethodAttributes.Assembly => "internal",
    MethodAttributes.FamORAssem => "protint",
    MethodAttributes.FamANDAssem => "privprot",
    _ => ""
};

sealed class TypeProvider : ISignatureTypeProvider<string, object>
{
    public string GetArrayType(string e, ArrayShape s) => e + "[]";
    public string GetByReferenceType(string e) => e + "&";
    public string GetFunctionPointerType(MethodSignature<string> s) => "fnptr";
    public string GetGenericInstantiation(string g, ImmutableArray<string> a) => g + "<" + string.Join(", ", a) + ">";
    public string GetGenericMethodParameter(object c, int i) => "!!" + i;
    public string GetGenericTypeParameter(object c, int i) => "!" + i;
    public string GetModifiedType(string m, string u, bool r) => u;
    public string GetPinnedType(string e) => e;
    public string GetPointerType(string e) => e + "*";
    public string GetPrimitiveType(PrimitiveTypeCode c) => c switch
    {
        PrimitiveTypeCode.Boolean => "bool",
        PrimitiveTypeCode.Byte => "byte",
        PrimitiveTypeCode.SByte => "sbyte",
        PrimitiveTypeCode.Char => "char",
        PrimitiveTypeCode.Int16 => "short",
        PrimitiveTypeCode.UInt16 => "ushort",
        PrimitiveTypeCode.Int32 => "int",
        PrimitiveTypeCode.UInt32 => "uint",
        PrimitiveTypeCode.Int64 => "long",
        PrimitiveTypeCode.UInt64 => "ulong",
        PrimitiveTypeCode.Single => "float",
        PrimitiveTypeCode.Double => "double",
        PrimitiveTypeCode.String => "string",
        PrimitiveTypeCode.Object => "object",
        PrimitiveTypeCode.Void => "void",
        PrimitiveTypeCode.IntPtr => "IntPtr",
        PrimitiveTypeCode.UIntPtr => "UIntPtr",
        PrimitiveTypeCode.TypedReference => "TypedReference",
        _ => c.ToString()
    };
    public string GetSZArrayType(string e) => e + "[]";
    public string GetTypeFromDefinition(MetadataReader r, TypeDefinitionHandle h, byte c) => NameFrom(r, h);
    public string GetTypeFromReference(MetadataReader r, TypeReferenceHandle h, byte c) => NameFrom(r, h);
    public string GetTypeFromSpecification(MetadataReader r, object c, TypeSpecificationHandle h, byte b)
        => r.GetTypeSpecification(h).DecodeSignature(this, c);

    static string NameFrom(MetadataReader r, TypeDefinitionHandle h)
    {
        var t = r.GetTypeDefinition(h);
        var ns = r.GetString(t.Namespace); var n = r.GetString(t.Name);
        return string.IsNullOrEmpty(ns) ? n : ns + "." + n;
    }
    static string NameFrom(MetadataReader r, TypeReferenceHandle h)
    {
        var t = r.GetTypeReference(h);
        var ns = r.GetString(t.Namespace); var n = r.GetString(t.Name);
        return string.IsNullOrEmpty(ns) ? n : ns + "." + n;
    }
}
