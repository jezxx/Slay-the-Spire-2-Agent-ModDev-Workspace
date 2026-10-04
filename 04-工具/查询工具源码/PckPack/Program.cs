using System.Security.Cryptography;
using System.Text;

// 打包 Godot 4.x 的 .pck（不加密目录）。
// 用法: PckPack <源目录> <输出.pck> <res前缀> [--no-md5]
// 目录格式与 PckList/PckVerify 保持一致：数据区在前，目录在 dirOffset，
// 每个目录条目包含路径、相对 fileBase 的偏移、大小、16 字节 MD5 和 flags。

if (args.Length < 3)
{
    Console.Error.WriteLine("usage: PckPack <sourceDir> <out.pck> <resPrefix> [--no-md5]");
    return 1;
}

var srcDir = Path.GetFullPath(args[0]);
var outPck = Path.GetFullPath(args[1]);
var prefix = args[2];
var writeMd5 = !args.Contains("--no-md5");
if (!Directory.Exists(srcDir)) { Console.Error.WriteLine($"源目录不存在: {srcDir}"); return 1; }
if (!prefix.StartsWith("res://")) { Console.Error.WriteLine("resPrefix 必须以 res:// 开头"); return 1; }
if (!prefix.EndsWith("/")) prefix += "/";

var files = Directory.GetFiles(srcDir, "*", SearchOption.AllDirectories)
    .Select(f => (abs: f, rel: Path.GetRelativePath(srcDir, f).Replace('\\', '/')))
    .OrderBy(x => x.rel, StringComparer.Ordinal)
    .ToList();

// Godot 4 v3 头部固定为 112 字节：前 40 字节是字段，后面是保留区。
const long HEADER_SIZE = 112;
const uint PACK_FORMAT_VERSION = 3;
const uint PACK_VERSION_MAJOR = 4, PACK_VERSION_MINOR = 5, PACK_VERSION_PATCH = 1;
const uint PACK_REL_FILEBASE = 1 << 1;
const ulong FILE_BASE = HEADER_SIZE;

long dataStart = HEADER_SIZE;
var offsets = new List<long>(files.Count);
long relativeOffset = 0;
foreach (var f in files)
{
    offsets.Add(relativeOffset);
    relativeOffset += new FileInfo(f.abs).Length;
}

long dirOffset = dataStart + relativeOffset;
long dirEntriesSize = 4;
foreach (var f in files)
    dirEntriesSize += 4 + Encoding.UTF8.GetByteCount(prefix + f.rel) + 8 + 8 + 16 + 4;

Directory.CreateDirectory(Path.GetDirectoryName(outPck)!);
using var fs = File.Create(outPck);
using var bw = new BinaryWriter(fs);

bw.Write(0x43504447u);          // GDPC
bw.Write(PACK_FORMAT_VERSION);
bw.Write(PACK_VERSION_MAJOR);
bw.Write(PACK_VERSION_MINOR);
bw.Write(PACK_VERSION_PATCH);
bw.Write(PACK_REL_FILEBASE);
bw.Write(FILE_BASE);
bw.Write((ulong)dirOffset);
for (int i = 0; i < 18; i++) bw.Write(0u); // 保留区，补足 112 字节头部

foreach (var f in files)
{
    using var input = File.OpenRead(f.abs);
    input.CopyTo(fs);
}

bw.Write((uint)files.Count);
for (int i = 0; i < files.Count; i++)
{
    var resPath = prefix + files[i].rel;
    var pathBytes = Encoding.UTF8.GetBytes(resPath);
    bw.Write((uint)pathBytes.Length);
    bw.Write(pathBytes);
    bw.Write((ulong)offsets[i]);
    bw.Write((ulong)new FileInfo(files[i].abs).Length);
    if (writeMd5)
    {
        using var input = File.OpenRead(files[i].abs);
        bw.Write(MD5.HashData(input));
    }
    else
    {
        bw.Write(new byte[16]);
    }
    bw.Write(0u); // entry flags
}

bw.Flush();
Console.WriteLine($"已写出 {outPck}");
Console.WriteLine($"  {files.Count} 个文件，dirOffset={dirOffset}，数据区起点={dataStart}，总计 {fs.Length} 字节");
foreach (var f in files.Take(15)) Console.WriteLine($"    {prefix}{f.rel}");
if (files.Count > 15) Console.WriteLine($"    ...（其余 {files.Count - 15} 个）");
return 0;
