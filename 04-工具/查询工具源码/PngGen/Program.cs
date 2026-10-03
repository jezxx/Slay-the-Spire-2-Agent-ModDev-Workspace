using System.IO.Compression;

// 生成一张最小但合法的 PNG（用于验证"mod 能否提供卡图"）。
// 用法: PngGen <输出路径> <宽> <高> <R> <G> <B>
// 我们手写 PNG 分块，不依赖任何图形库。

if (args.Length < 7)
{
    Console.Error.WriteLine("usage: PngGen <out.png> <w> <h> <r> <g> <b>");
    return 1;
}
var outPath = args[0];
int w = int.Parse(args[1]), h = int.Parse(args[2]);
byte r = byte.Parse(args[3]), g = byte.Parse(args[4]), b = byte.Parse(args[5]);

static void WriteChunk(Stream s, string type, byte[] data)
{
    var len = BitConverter.GetBytes(data.Length);
    if (BitConverter.IsLittleEndian) Array.Reverse(len);
    s.Write(len, 0, 4);
    var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
    s.Write(typeBytes, 0, 4);
    s.Write(data, 0, data.Length);
    var crcInput = new byte[4 + data.Length];
    Array.Copy(typeBytes, 0, crcInput, 0, 4);
    Array.Copy(data, 0, crcInput, 4, data.Length);
    var crc = Crc32(crcInput);
    var crcBytes = BitConverter.GetBytes(crc);
    if (BitConverter.IsLittleEndian) Array.Reverse(crcBytes);
    s.Write(crcBytes, 0, 4);
}

static uint Crc32(byte[] buf)
{
    uint crc = 0xFFFFFFFF;
    foreach (var by in buf)
    {
        crc ^= by;
        for (int i = 0; i < 8; i++)
            crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
    }
    return ~crc;
}

Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
using var fs = File.Create(outPath);
fs.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);

// IHDR：8bit RGB（color type 2）
var ihdr = new byte[13];
Array.Copy(BitConverter.GetBytes(w).Reverse().ToArray(), 0, ihdr, 0, 4);
Array.Copy(BitConverter.GetBytes(h).Reverse().ToArray(), 0, ihdr, 4, 4);
ihdr[8] = 8;    // bit depth
ihdr[9] = 2;    // color type: truecolor
ihdr[10] = 0; ihdr[11] = 0; ihdr[12] = 0;
WriteChunk(fs, "IHDR", ihdr);

// IDAT：每行前 1 字节 filter(0)，然后 RGB
using var ms = new MemoryStream();
for (int y = 0; y < h; y++)
{
    ms.WriteByte(0);
    for (int x = 0; x < w; x++) { ms.WriteByte(r); ms.WriteByte(g); ms.WriteByte(b); }
}
var raw = ms.ToArray();
using var comp = new MemoryStream();
using (var z = new ZLibStream(comp, CompressionLevel.Optimal, leaveOpen: true)) z.Write(raw, 0, raw.Length);
WriteChunk(fs, "IDAT", comp.ToArray());
WriteChunk(fs, "IEND", Array.Empty<byte>());

Console.WriteLine($"wrote {outPath} ({new FileInfo(outPath).Length} bytes, {w}x{h})");
return 0;
