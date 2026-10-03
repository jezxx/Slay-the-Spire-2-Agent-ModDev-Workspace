using System.Text;

// 读取 Godot .pck 的文件目录（只读索引，不解包内容）。
// 用法: PckList <file.pck> [filterRegex]
//       PckList <file.pck> --extract <resPath> <outFile>   提取单个文件
// Godot 4 的 pck 头上是 magic "GDPC"，index 里存的是 res:// 路径、偏移和大小。

if (args.Length < 1) { Console.Error.WriteLine("usage: PckList <file.pck> [filter] | PckList <file.pck> --extract <resPath> <outFile>"); return 1; }
var path = args[0];
bool extractMode = args.Length >= 4 && args[1] == "--extract";
string extractPath = extractMode ? args[2] : null;
string extractOut = extractMode ? args[3] : null;
var filter = (!extractMode && args.Length > 1) ? new System.Text.RegularExpressions.Regex(args[1], System.Text.RegularExpressions.RegexOptions.IgnoreCase) : null;

using var fs = File.OpenRead(path);
using var br = new BinaryReader(fs);

uint magic = br.ReadUInt32();
if (magic != 0x43504447) { Console.Error.WriteLine($"不是 pck（magic=0x{magic:X8}）"); return 1; }
uint packFormat = br.ReadUInt32();
uint verMajor = br.ReadUInt32();
uint verMinor = br.ReadUInt32();
uint verPatch = br.ReadUInt32();
uint packFlags = br.ReadUInt32();
ulong fileBase = br.ReadUInt64();

// v3：紧跟一个 dir_offset(u64) 指向目录；v2：目录就在头部之后（16 个 reserved u32）。
if (packFormat >= 3)
{
    ulong dirOffset = br.ReadUInt64();
    br.BaseStream.Position = (long)dirOffset;   // BinaryReader 按需读、不预读，seek 后无需额外处理
}
else
{
    br.ReadBytes(16 * 4);
}
uint fileCount = br.ReadUInt32();

Console.WriteLine($"格式版本 {verMajor}.{verMinor}.{verPatch}  packFormat={packFormat}  flags=0x{packFlags:X}  files={fileCount}");

var rows = new List<(string path, ulong offset, ulong size)>();
bool dirEncrypted = (packFlags & 1) != 0;
for (uint i = 0; i < fileCount; i++)
{
    uint len = br.ReadUInt32();
    var raw = br.ReadBytes((int)len);
    var resPath = Encoding.UTF8.GetString(raw).TrimEnd('\0');
    ulong offset = br.ReadUInt64();
    ulong size = br.ReadUInt64();
    // 每项恒有 md5(16B)，packFormat>=2 再跟 flags(u32)。
    // （旧代码只在目录加密时读这两段 → 从第 2 项起错位，读到垃圾 len）
    br.ReadBytes(16);
    if (packFormat >= 2) br.ReadUInt32();
    rows.Add((resPath, offset, size));
}

if (extractMode)
{
    var hit = rows.FirstOrDefault(r => r.path == extractPath);
    if (hit.path == null) { Console.Error.WriteLine($"not found: {extractPath}"); return 2; }
    // packFormat=3 且 flags=0x2：offset 相对 fileBase（实测差值恰为 112）
    fs.Position = (long)(hit.offset + fileBase);
    using var outFs = File.Create(extractOut);
    var buf = new byte[81920];
    long remaining = (long)hit.size;
    while (remaining > 0)
    {
        int n = fs.Read(buf, 0, (int)Math.Min(buf.Length, remaining));
        if (n <= 0) break;
        outFs.Write(buf, 0, n);
        remaining -= n;
    }
    Console.WriteLine($"extracted {hit.size} bytes -> {extractOut}");
    return 0;
}

foreach (var r in rows.Select(r => $"{r.size,10}  @{r.offset,-12}  {r.path}").Where(r => filter == null || filter.IsMatch(r)).OrderBy(r => r))
    Console.WriteLine(r);
Console.WriteLine($"\n共 {rows.Count} 项" + (filter != null ? $"，匹配 {rows.Count(r => filter.IsMatch(r.path))} 项" : ""));
return 0;
