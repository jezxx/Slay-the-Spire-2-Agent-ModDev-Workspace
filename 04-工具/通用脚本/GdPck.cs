// GdPck.cs -- minimal Godot 4.x .pck (packfmt 2/3) reader
// Layout confirmed empirically against StS2 (Godot 4.5.1) packs:
//   header : 'GDPC' magic, u32 packfmt, u32 major, u32 minor, u32 patch,
//            u32 packflags, u64 file_base, u64 dir_offset, u8 reserved[16]
//   dir    : u32 file_count
//   entry  : u32 path_len, char path[path_len] (null padded to 4),
//            u64 offset, u64 size, u8 md5[16], u32 entry_flags
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public sealed class PckEntry {
    public string Path;
    public long Offset;
    public long Size;
    public byte[] Md5;
    public uint Flags;
    public override string ToString() {
        return string.Format("{0,13:N0}  {1}", Size, Path);
    }
}

public static class GdPck {
    public static List<PckEntry> Read(string file) {
        var list = new List<PckEntry>();
        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            var br = new BinaryReader(fs);
            uint magic = br.ReadUInt32();
            if (magic != 0x43504447u) throw new Exception("not a GDPC file, magic=0x" + magic.ToString("X8"));
            uint packfmt = br.ReadUInt32();
            uint major = br.ReadUInt32();
            uint minor = br.ReadUInt32();
            uint patch = br.ReadUInt32();
            uint packflags = br.ReadUInt32();
            long fileBase = br.ReadInt64();
            long dirOffset = br.ReadInt64();
            fs.Seek(16, SeekOrigin.Current); // reserved
            Console.WriteLine("  engine   : {0}.{1}.{2}  packfmt={3} packflags=0x{4:X} file_base={5}",
                major, minor, patch, packfmt, packflags, fileBase);
            if (packfmt < 2) throw new Exception("packfmt " + packfmt + " not supported (need >= 2)");

            // Godot PackFlags: PACK_DIR_ENCRYPTED = 1<<0, PACK_REL_FILEBASE = 1<<1.
            // With REL_FILEBASE, entry offsets in the directory are RELATIVE to
            // file_base, so the real offset is fileBase + stored. Without this
            // every extracted file is short by file_base bytes.
            // Confirmed on StS2 packs: packflags=0x2, file_base=112.
            long entryBase = ((packflags & 2) != 0) ? fileBase : 0;

            fs.Seek(dirOffset, SeekOrigin.Begin);
            uint count = br.ReadUInt32();
            Console.WriteLine("  dir      : offset={0} file_count={1} entry_base={2}", dirOffset, count, entryBase);
            for (uint i = 0; i < count; i++) {
                uint plen = br.ReadUInt32();
                if (plen == 0 || plen > 4096) throw new Exception("bad path_len " + plen + " at entry " + i);
                byte[] pb = br.ReadBytes((int)plen);
                string p = Encoding.UTF8.GetString(pb).TrimEnd('\0');
                long ofs = br.ReadInt64();
                long size = br.ReadInt64();
                byte[] md5 = br.ReadBytes(16);
                uint eflags = br.ReadUInt32();
                list.Add(new PckEntry { Path = p, Offset = entryBase + ofs, Size = size, Md5 = md5, Flags = eflags });
            }
        }
        return list;
    }

    public static void Extract(string pckFile, IEnumerable<PckEntry> entries, string outDir) {
        Directory.CreateDirectory(outDir);
        using (var fs = new FileStream(pckFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            foreach (var e in entries) {
                string rel = e.Path.Replace(':', '_').Replace('\\', '/').TrimStart('/');
                string dest = Path.Combine(outDir, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                fs.Seek(e.Offset, SeekOrigin.Begin);
                byte[] buf = new byte[e.Size];
                int got = 0;
                while (got < buf.Length) {
                    int r = fs.Read(buf, got, buf.Length - got);
                    if (r <= 0) break;
                    got += r;
                }
                if (got != buf.Length) { Console.WriteLine("  SHORT " + e.Path); continue; }
                File.WriteAllBytes(dest, buf);
            }
        }
    }
}
