using System.Text;

// 严格按 Godot core/io/file_access_pack.cpp 解析 .pck 目录并校验数据。
// 全程用内存数组 + 游标，避免 BinaryReader 缓冲与 seek 交互的坑。
//
// 用法: PckVerify <file.pck>

if (args.Length < 1) { Console.Error.WriteLine("usage: PckVerify <file.pck>"); return 1; }
var path = args[0];
var b = File.ReadAllBytes(path);
int p = 0;
bool Oob(int need) => p + need > b.Length;

uint U32() { if (Oob(4)) throw new EndOfStreamException($"读 u32 越界 p={p} size={b.Length}"); var v = BitConverter.ToUInt32(b, p); p += 4; return v; }
ulong U64() { if (Oob(8)) throw new EndOfStreamException($"读 u64 越界 p={p} size={b.Length}"); var v = BitConverter.ToUInt64(b, p); p += 8; return v; }
byte[] Bytes(int n) { if (n < 0 || Oob(n)) throw new EndOfStreamException($"读 {n} 字节越界 p={p} size={b.Length}"); var v = new byte[n]; Array.Copy(b, p, v, 0, n); p += n; return v; }

if (U32() != 0x43504447) { Console.Error.WriteLine("bad magic"); return 1; }
uint format = U32();
uint vMaj = U32(), vMin = U32(), vPat = U32();
uint flags = U32();
ulong fileBase = U64();

ulong dirOffset = 0;
if (format >= 3)
{
    dirOffset = U64();
    // 实测（本机 MegaDot v4.5.1.m.14 导出的 pck）的真实布局：
    //   偏移 0..39  头部（magic/format/ver*3/flags/fileBase/dir_offset）
    //   偏移 40     u32 = 0        ← 头部里紧跟着的那个计数**恒为 0**
    //   偏移 dir_offset  u32 = 真实文件数
    // 所以真正的"文件数"在 dir_offset 处，而不是 dir_offset+4。
    p = (int)dirOffset;
}
else { p += 16 * 4; }   // v2: 16 个 reserved u32

uint fileCount = U32();
bool dirEncrypted = (flags & 1) != 0;
Console.WriteLine($"format={format} ver={vMaj}.{vMin}.{vPat} flags=0x{flags:X} dirEncrypted={dirEncrypted} fileBase={fileBase} dirOffset={dirOffset} files={fileCount} totalSize={b.Length}");

int bad = 0;
int parsed = 0;
var decoder = new UTF8Encoding(false, true);
for (uint i = 0; i < fileCount; i++)
{
    try
    {
        uint plen = U32();
        var resPath = Encoding.UTF8.GetString(Bytes((int)plen));
        ulong ofs = U64();
        ulong size = U64();
        // 实测：无论是否加密，目录条目都带 md5(16) + flags(4)。
        Bytes(16);
        U32();
        parsed++;

        long real = (long)fileBase + (long)ofs;
        string verdict;
        if (real < 0 || real + (long)size > b.Length) { verdict = $"❌ 越界 (real={real}, size={size})"; bad++; }
        else
        {
            try
            {
                var text = decoder.GetString(b, (int)real, (int)size);
                var head = text.Replace('\n', ' ');
                if (head.Length > 56) head = head[..56];
                verdict = $"✅ ok  \"{head}\"";
            }
            catch (Exception ex) { verdict = $"（非 UTF-8 文本，通常是 Godot 内部二进制，属正常：{ex.GetType().Name}）"; }
        }
        Console.WriteLine($"  [{i}] size={size,8} ofs={ofs,8} → real={real,8}  {resPath}");
        Console.WriteLine($"       {verdict}");
    }
    catch (EndOfStreamException ex)
    {
        Console.WriteLine($"  ⚠ 目录解析在第 {i} 项中断：{ex.Message}");
        break;
    }
}

if (bad == 0) Console.WriteLine("索引与数据全部有效 ✅");
return bad == 0 ? 0 : 1;
