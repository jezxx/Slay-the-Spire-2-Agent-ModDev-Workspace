// SigDump - dump full member signatures (with TYPES) from a .NET assembly using only
// System.Reflection.Metadata (part of the shared framework, no NuGet packages needed).
//
// usage:
//   SigDump <asm> <out> --ns <NamespacePrefix>          list types in namespace (name : base)
//   SigDump <asm> <out> --type <TypeFullName>           all members with full signatures
//   SigDump <asm> <out> --member <TypeFullName> <Name>  only members whose name matches
//   SigDump <asm> <out> --derived <TypeFullName>        all types whose base chain starts here
//   SigDump <asm> <out> --all <substring>               all type names containing substring

using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;

internal sealed class Ctx
{
    public string[] TypeParams = Array.Empty<string>();
    public string[] MethodParams = Array.Empty<string>();
}

internal sealed class Provider : ISignatureTypeProvider<string, Ctx>
{
    private readonly MetadataReader _r;
    public Provider(MetadataReader r) { _r = r; }

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
        PrimitiveTypeCode.IntPtr => "nint",
        PrimitiveTypeCode.UIntPtr => "nuint",
        PrimitiveTypeCode.Object => "object",
        PrimitiveTypeCode.String => "string",
        PrimitiveTypeCode.TypedReference => "TypedReference",
        PrimitiveTypeCode.Void => "void",
        _ => c.ToString()
    };

    public string TypeDefName(TypeDefinitionHandle h) => TypeDefName(_r, h);

    private string TypeDefName(MetadataReader reader, TypeDefinitionHandle h)
    {
        var td = reader.GetTypeDefinition(h);
        var decl = td.GetDeclaringType();
        var n = reader.GetString(td.Name);
        if (!decl.IsNil) return TypeDefName(reader, decl) + "+" + n;
        var ns = reader.GetString(td.Namespace);
        return ns.Length == 0 ? n : ns + "." + n;
    }

    public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        => TypeDefName(reader, handle);

    public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
    {
        var tr = reader.GetTypeReference(handle);
        var ns = reader.GetString(tr.Namespace);
        var n = reader.GetString(tr.Name);
        return ns.Length == 0 ? n : ns + "." + n;
    }

    public string GetTypeFromSpecification(MetadataReader reader, Ctx genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);

    public string GetSZArrayType(string elementType) => elementType + "[]";
    public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[" + new string(',', shape.Rank - 1) + "]";
    public string GetByReferenceType(string elementType) => elementType + "&";
    public string GetPointerType(string elementType) => elementType + "*";
    public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments)
        => genericType + "<" + string.Join(", ", typeArguments) + ">";
    public string GetGenericMethodParameter(Ctx ctx, int index)
        => (ctx != null && index < ctx.MethodParams.Length) ? ctx.MethodParams[index] : "!!" + index;
    public string GetGenericTypeParameter(Ctx ctx, int index)
        => (ctx != null && index < ctx.TypeParams.Length) ? ctx.TypeParams[index] : "!" + index;
    public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
    public string GetPinnedType(string elementType) => elementType;
    public string GetFunctionPointerType(MethodSignature<string> signature)
        => "delegate*<" + string.Join(", ", signature.ParameterTypes) + ", " + signature.ReturnType + ">";
}

internal static class Program
{
    private static MetadataReader R;
    private static Provider P;

    private static int Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("usage: SigDump <asm> <out> --ns|--type|--member|--derived|--all <arg> [arg2]");
            return 1;
        }
        var asm = args[0];
        var outPath = args[1];
        var mode = args[2];
        var arg = args[3];

        if (!File.Exists(asm)) { Console.Error.WriteLine("asm not found: " + asm); return 1; }
        using var fs = File.OpenRead(asm);
        using var pe = new PEReader(fs);
        if (!pe.HasMetadata) { Console.Error.WriteLine("no metadata"); return 1; }
        R = pe.GetMetadataReader();
        P = new Provider(R);

        var sb = new StringBuilder();
        int hits = 0;
        try
        {
            switch (mode)
            {
                case "--ns": hits = DumpNs(sb, arg); break;
                case "--type": hits = DumpType(sb, arg, null); break;
                case "--member": hits = DumpType(sb, arg, args.Length > 4 ? args[4] : null); break;
                case "--derived": hits = DumpDerived(sb, arg); break;
                case "--all":
                    foreach (var h in R.TypeDefinitions)
                    {
                        var td = R.GetTypeDefinition(h);
                        var full = FullName(td);
                        if (full.Contains(arg, StringComparison.OrdinalIgnoreCase)) { sb.AppendLine(full); hits++; }
                    }
                    break;
                default:
                    Console.Error.WriteLine("unknown mode " + mode);
                    return 1;
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine("!! EXCEPTION: " + ex);
            Console.Error.WriteLine(ex.ToString());
        }

        File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine($"wrote {outPath} ({sb.Length} chars), hits={hits}");
        return 0;
    }

    private static string FullName(TypeDefinition td)
    {
        var decl = td.GetDeclaringType();
        var n = R.GetString(td.Name);
        if (!decl.IsNil) return FullName(R.GetTypeDefinition(decl)) + "+" + n;
        var ns = R.GetString(td.Namespace);
        return ns.Length == 0 ? n : ns + "." + n;
    }

    private static bool TryResolve(EntityHandle h, out string name)
    {
        name = null;
        if (h.IsNil) return false;
        switch (h.Kind)
        {
            case HandleKind.TypeDefinition: name = FullName(R.GetTypeDefinition((TypeDefinitionHandle)h)); return true;
            case HandleKind.TypeReference:
                var tr = R.GetTypeReference((TypeReferenceHandle)h);
                var ns = R.GetString(tr.Namespace); var n = R.GetString(tr.Name);
                name = ns.Length == 0 ? n : ns + "." + n; return true;
            case HandleKind.TypeSpecification:
                name = R.GetTypeSpecification((TypeSpecificationHandle)h).DecodeSignature(P, new Ctx()); return true;
            default: return false;
        }
    }

    private static TypeDefinitionHandle FindType(string full)
    {
        foreach (var h in R.TypeDefinitions)
        {
            var td = R.GetTypeDefinition(h);
            if (FullName(td) == full) return h;
        }
        // case-insensitive / suffix fallback
        foreach (var h in R.TypeDefinitions)
        {
            var td = R.GetTypeDefinition(h);
            var f = FullName(td);
            if (f.Equals(full, StringComparison.OrdinalIgnoreCase)) return h;
        }
        foreach (var h in R.TypeDefinitions)
        {
            var td = R.GetTypeDefinition(h);
            var f = FullName(td);
            if (f.EndsWith("." + full, StringComparison.OrdinalIgnoreCase)) return h;
        }
        return default;
    }

    private static int DumpNs(StringBuilder sb, string nsPrefix)
    {
        int c = 0;
        foreach (var h in R.TypeDefinitions)
        {
            var td = R.GetTypeDefinition(h);
            var full = FullName(td);
            var ns = R.GetString(td.Namespace);
            if (ns == nsPrefix || ns.StartsWith(nsPrefix + ".", StringComparison.Ordinal))
            {
                string baseN = TryResolve(td.BaseType, out var b) ? b : "";
                bool isEnum = baseN == "System.Enum";
                sb.Append(full).Append(isEnum ? "  [enum]" : "  [type]");
                if (baseN.Length > 0) sb.Append(" : ").Append(baseN);
                sb.AppendLine();
                c++;
            }
        }
        return c;
    }

    private static int DumpDerived(StringBuilder sb, string baseFull)
    {
        int c = 0;
        foreach (var h in R.TypeDefinitions)
        {
            var td = R.GetTypeDefinition(h);
            var cur = td.BaseType;
            int guard = 0;
            while (!cur.IsNil && guard++ < 12)
            {
                if (!TryResolve(cur, out var bn)) break;
                if (bn == baseFull) { sb.AppendLine(FullName(td) + "  :  " + bn); c++; break; }
                if (cur.Kind != HandleKind.TypeDefinition) break;
                cur = R.GetTypeDefinition((TypeDefinitionHandle)cur).BaseType;
            }
        }
        return c;
    }

    private static string Vis(MethodAttributes a)
    {
        if ((a & MethodAttributes.Public) != 0) return "public";
        if ((a & MethodAttributes.Family) != 0) return "protected";
        if ((a & MethodAttributes.Assembly) != 0) return "internal";
        if ((a & MethodAttributes.Private) != 0) return "private";
        return "";
    }

    private static string FieldVis(FieldAttributes a)
    {
        if ((a & FieldAttributes.Public) != 0) return "public";
        if ((a & FieldAttributes.Family) != 0) return "protected";
        if ((a & FieldAttributes.Assembly) != 0) return "internal";
        if ((a & FieldAttributes.Private) != 0) return "private";
        return "";
    }

    private static int DumpType(StringBuilder sb, string typeFull, string nameFilter)
    {
        var th = FindType(typeFull);
        if (th.IsNil) { sb.AppendLine("type not found: " + typeFull); return 0; }
        var td = R.GetTypeDefinition(th);
        int c = 0;

        var typeParams = new List<string>();
        foreach (var gph in td.GetGenericParameters())
            typeParams.Add(R.GetString(R.GetGenericParameter(gph).Name));
        var typeCtx = new Ctx { TypeParams = typeParams.ToArray() };

        var kind = td.Attributes.HasFlag(TypeAttributes.Interface) ? "interface"
                 : (TryResolve(td.BaseType, out var b0) && b0 == "System.Enum") ? "enum"
                 : "class";
        var vis = (td.Attributes & TypeAttributes.VisibilityMask) switch
        {
            TypeAttributes.Public => "public",
            TypeAttributes.NotPublic => "internal",
            _ => "public"
        };
        string tp = typeParams.Count > 0 ? "<" + string.Join(", ", typeParams) + ">" : "";
        sb.AppendLine($"### {vis} {kind} {FullName(td)}{tp}");
        if (TryResolve(td.BaseType, out var bn)) sb.AppendLine($"    base: {bn}");
        foreach (var ih in td.GetInterfaceImplementations())
        {
            var ii = R.GetInterfaceImplementation(ih);
            if (TryResolve(ii.Interface, out var iname)) sb.AppendLine($"    implements: {iname}");
        }
        sb.AppendLine();

        if (kind == "enum")
        {
            foreach (var fh in td.GetFields())
            {
                var fd = R.GetFieldDefinition(fh);
                var fn = R.GetString(fd.Name);
                if (fn == "value__") continue;
                string val = "";
                try
                {
                    var ch = fd.GetDefaultValue();
                    if (!ch.IsNil)
                    {
                        var cv = R.GetConstant(ch);
                        var br = R.GetBlobReader(cv.Value);
                        val = cv.TypeCode switch
                        {
                            ConstantTypeCode.Byte => " = " + br.ReadByte(),
                            ConstantTypeCode.SByte => " = " + br.ReadSByte(),
                            ConstantTypeCode.Int16 => " = " + br.ReadInt16(),
                            ConstantTypeCode.UInt16 => " = " + br.ReadUInt16(),
                            ConstantTypeCode.Int32 => " = " + br.ReadInt32(),
                            ConstantTypeCode.UInt32 => " = " + br.ReadUInt32(),
                            ConstantTypeCode.Int64 => " = " + br.ReadInt64(),
                            ConstantTypeCode.UInt64 => " = " + br.ReadUInt64(),
                            _ => ""
                        };
                    }
                }
                catch { }
                sb.AppendLine($"    enum {fn}{val}");
                c++;
            }
            return c;
        }

        // ---- fields ----
        foreach (var fh in td.GetFields())
        {
            var fd = R.GetFieldDefinition(fh);
            var fn = R.GetString(fd.Name);
            if (fn == "value__") continue;
            if (nameFilter != null && fn != nameFilter) continue;
            string ft;
            try { ft = fd.DecodeSignature(P, typeCtx); } catch { ft = "?"; }
            var mods = (fd.Attributes & FieldAttributes.Static) != 0 ? "static " : "";
            var ro = (fd.Attributes & FieldAttributes.InitOnly) != 0 ? "readonly " : "";
            sb.AppendLine($"    field  {FieldVis(fd.Attributes)} {mods}{ro}{ft} {fn}");
            c++;
        }

        // ---- properties ----
        foreach (var ph in td.GetProperties())
        {
            var pd = R.GetPropertyDefinition(ph);
            var pn = R.GetString(pd.Name);
            if (nameFilter != null && pn != nameFilter) continue;
            string pt;
            try { pt = pd.DecodeSignature(P, typeCtx).ReturnType; } catch { pt = "?"; }
            var acc = pd.GetAccessors();
            var access = "";
            if (!acc.Getter.IsNil)
            {
                var g = R.GetMethodDefinition(acc.Getter);
                access += Vis(g.Attributes) + " get; ";
            }
            if (!acc.Setter.IsNil)
            {
                var s = R.GetMethodDefinition(acc.Setter);
                access += Vis(s.Attributes) + " set; ";
            }
            sb.AppendLine($"    prop   {pt} {pn} {{ {access}}}");
            c++;
        }

        // ---- methods ----
        foreach (var mh in td.GetMethods())
        {
            var md = R.GetMethodDefinition(mh);
            var mn = R.GetString(md.Name);
            if (nameFilter != null && mn != nameFilter && !mn.StartsWith(nameFilter, StringComparison.Ordinal)) continue;

            var mparams = new List<string>();
            foreach (var gph in md.GetGenericParameters())
                mparams.Add(R.GetString(R.GetGenericParameter(gph).Name));
            var ctx = new Ctx { TypeParams = typeParams.ToArray(), MethodParams = mparams.ToArray() };

            string ret = "?"; ImmutableArray<string> pts = ImmutableArray<string>.Empty;
            try
            {
                var sig = md.DecodeSignature(P, ctx);
                ret = sig.ReturnType; pts = sig.ParameterTypes;
            }
            catch (Exception ex) { ret = "??" + ex.GetType().Name; }

            // parameter names
            var pnames = new string[pts.Length];
            foreach (var phh in md.GetParameters())
            {
                var pd2 = R.GetParameter(phh);
                int seq = pd2.SequenceNumber;
                if (seq >= 1 && seq <= pnames.Length) pnames[seq - 1] = R.GetString(pd2.Name);
            }
            var parts = new List<string>();
            for (int i = 0; i < pts.Length; i++)
                parts.Add(pts[i] + " " + (pnames[i] ?? ("arg" + i)));

            var mods = (md.Attributes & MethodAttributes.Static) != 0 ? "static " : "";
            string mg = mparams.Count > 0 ? "<" + string.Join(", ", mparams) + ">" : "";
            var vis2 = Vis(md.Attributes);
            sb.AppendLine($"    method {vis2} {mods}{ret} {mn}{mg}({string.Join(", ", parts)})");
            c++;
        }

        // ---- events ----
        foreach (var eh in td.GetEvents())
        {
            var ed = R.GetEventDefinition(eh);
            var en = R.GetString(ed.Name);
            if (nameFilter != null && en != nameFilter) continue;
            string et = "?";
            try { if (!ed.Type.IsNil) TryResolve(ed.Type, out et); } catch { }
            sb.AppendLine($"    event  {et} {en}");
            c++;
        }

        return c;
    }
}
