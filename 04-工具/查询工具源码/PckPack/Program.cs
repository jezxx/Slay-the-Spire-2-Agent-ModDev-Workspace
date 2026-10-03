using System.Text;

// 打包 Godot 4.x 的 .pck（不加密、不加密目录）。
// 用途：本工作流不装 Godot 编辑器，用这个工具把模组资源目录打成 pck，
//       这样游戏才能通过 res://<ModId>/... 访问（Godot 只会挂载 pck，不会挂载散装目录）。
//
// 用法: PckPack <源目录> <输出.pck> <res前缀> [--no-md5]
//   PckPack .\MyMod\data out\MyMod.pck res://MyMod/

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

// 收集文件（相对路径统一用 / 分隔）
var files = Directory.GetFiles(srcDir, "*", SearchOption.AllDirectories)
    .Select(f => (abs: f, rel: Path.GetRelativePath(srcDir, f).Replace('\\', '/')))
    .OrderBy(x => x.rel, StringComparer.Ordinal)
    .ToList();

// ---- 头部（PACK_FORMAT_VERSION_V3，共 32 字节）----
// magic(4) + version(4) + ver_major(4) + ver_minor(4) + ver_patch(4) + flags(4) + file_base(8) = 32
// v3 没有 v2 的 16 个 reserved u32，而是紧跟一个 dir_offset(u64) —— 目录在文件里的绝对偏移。
// 这个 dir_offset 我之前漏了（按 v2 布局写），导致 Godot 把目录数据当成头部继续解析，
// 于是把偏移字段误当路径长度，报 "Invalid UTF-8 leading byte" 并且卡死启动。实测确认。
const long HEADER_SIZE = 32;
const long DIR_OFFSET_FIELD_POS = 32;   // dir_offset 字段在文件中的位置
const uint PACK_FORMAT_VERSION = 3;
const uint PACK_VERSION_MAJOR = 4, PACK_VERSION_MINOR = 5, PACK_VERSION_PATCH = 1;
const uint PACK_REL_FILEBASE = 1 << 1;  // v3 恒开

// 目录条目的字节数（未加密时：pathlen u32 + path + offset u64 + size u64）
long dirEntriesSize = 0;
foreach (var f in files)
    dirEntriesSize += 4 + Encoding.UTF8.GetByteCount(prefix + f.rel) + 8 + 8;

// 目录位置 = 头部 + dir_offset 字段(8) + 文件数(4) + 所有目录条目
long DIR_COUNT_POS = HEADER_SIZE + 8;
long dirOffset = DIR_COUNT_POS + 4 + dirEntriesSize;
long dataStart = dirOffset;

Directory.CreateDirectory(Path.GetDirectoryName(outPck)!);
using var fs = File.Create(outPck);
using var bw = new BinaryWriter(fs);

bw.Write(0x43504447u);          // "GDPC"
bw.Write(PACK_FORMAT_VERSION);
bw.Write(PACK_VERSION_MAJOR);
bw.Write(PACK_VERSION_MINOR);
bw.Write(PACK_VERSION_PATCH);
bw.Write(PACK_REL_FILEBASE);    // flags：未加密（PACK_DIR_ENCRYPTED 未置位）
bw.Write(0UL);                  // fileBase
bw.Write((ulong)dirOffset);     // v3：目录绝对偏移

// 目录：文件数 + 条目
bw.Write((uint)files.Count);

// 计算每个文件的偏移
long offset = dataStart;
var offsets = new List<long>(files.Count);
foreach (var f in files)
{
    offsets.Add(offset);
    offset += new FileInfo(f.abs).Length;
}

// 写目录条目（未加密：只有 pathlen + path + offset + size）
for (int i = 0; i < files.Count; i++)
{
    var resPath = prefix + files[i].rel;
    var bytes = Encoding.UTF8.GetBytes(resPath);
    bw.Write((uint)bytes.Length);
    bw.Write(bytes);
    bw.Write((ulong)offsets[i]);
    bw.Write((ulong)new FileInfo(files[i].abs).Length);
}

// 写数据区
foreach (var f in files)
{
    using var fsin = File.OpenRead(f.abs);
    fsin.CopyTo(fs);
}

bw.Flush();
Console.WriteLine($"已写出 {outPck}");
Console.WriteLine($"  {files.Count} 个文件，dirOffset={dirOffset}，数据区起点={dataStart}，总计 {fs.Length} 字节");
foreach (var f in files.Take(15)) Console.WriteLine($"    {prefix}{f.rel}");
if (files.Count > 15) Console.WriteLine($"    ...（其余 {files.Count - 15} 个）");
return 0;
