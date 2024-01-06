using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using CodeWalker.Core.Utils;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Gxt2File : PackedFile
    {
        public string Name { get; set; }
        public RpfFileEntry FileEntry { get; set; }
        public uint EntryCount { get; set; }
        public Gxt2Entry[] TextEntries { get; set; }
        //public Dictionary<uint, string> Dict { get; set; }


        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry?.Name ?? "";
            FileEntry = entry;
            //Dict = new Dictionary<uint, string>();

            var sequence = new ReadOnlySequence<byte>(data);
            var reader = new SequenceReader<byte>(sequence);
            reader.TryReadLittleEndian(out uint gxt2);
            if (gxt2 != 1196971058)
            {
                return;
            }

            reader.TryReadLittleEndian(out uint entryCount);
            EntryCount = entryCount;
            TextEntries = new Gxt2Entry[EntryCount];
            for (uint i = 0; i < EntryCount; i++)
            {
                
                reader.TryReadLittleEndian(out uint hash);
                reader.TryReadLittleEndian(out uint offset);
                TextEntries[i] = new Gxt2Entry(hash, string.Empty, offset);
            }

            reader.TryReadLittleEndian(out gxt2); //another "GXT2"
            if (gxt2 != 1196971058)
            {
                return;
            }

            reader.TryReadLittleEndian(out uint endpos);

            for (uint i = 0; i < EntryCount; i++)
            {
                ref var e = ref TextEntries[i];
                var strReader = new SequenceReader<byte>(new ReadOnlySequence<byte>(data, (int)e.Offset, data.Length - (int)e.Offset));
                strReader.TryReadTo(out ReadOnlySpan<byte> str, 0);
                e = new Gxt2Entry(e.Hash, Encoding.UTF8.GetString(str), e.Offset);
            }
        }
        public byte[] Save()
        {
            if (TextEntries == null)
                TextEntries = [];
            EntryCount = (uint)TextEntries.Length;
            uint offset = 16 + (EntryCount * 8);
            List<byte[]> datas = new List<byte[]>();

            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);

            bw.Write(1196971058); //"GXT2"
            bw.Write(EntryCount);
            foreach (ref var e in TextEntries.AsSpan())
            {
                e = new Gxt2Entry(e.Hash, e.Text, offset);
                var d = Encoding.UTF8.GetBytes($"{e.Text}\0");
                datas.Add(d);
                offset += (uint)d.Length;
                bw.Write(e.Hash);
                bw.Write(e.Offset);
            }
            bw.Write(1196971058); //"GXT2"
            bw.Write(offset);
            foreach (var d in datas)
            {
                bw.Write(d);
            }

            bw.Flush();
            ms.Position = 0;
            var data = new byte[ms.Length];
            ms.Read(data, 0, (int)ms.Length);

            return data;
        }


        public string ToText()
        {
            StringBuilder sb = new StringBuilder();
            if (TextEntries != null)
            {
                foreach (var entry in TextEntries)
                {
                    sb.AppendLine($"0x{entry.Hash:X8} = {entry.Text}");
                }
            }
            return sb.ToString();
        }
        public static Gxt2File FromText(string text)
        {
            var gxt = new Gxt2File();
            var entries = new List<Gxt2Entry>();
            foreach (var line in text.EnumerateSplit('\n'))
            {
                var tline = line.Trim();
                if (tline.Length < 13)
                    continue;
                if (uint.TryParse(tline.Slice(2, 8), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hash))
                {
                    var entry = new Gxt2Entry(hash, (tline.Length > 13) ? tline.Slice(13).ToString() : "");
                    entries.Add(entry);
                }
                else
                {
                    //error parsing hash, probably should tell the user about this somehow
                }
            }
            entries.Sort((a, b) => a.Hash.CompareTo(b.Hash));
            gxt.TextEntries = entries.ToArray();
            gxt.EntryCount = (uint)entries.Count;
            return gxt;
        }
    
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public readonly struct Gxt2Entry
    {
        public uint Hash { get; init; }
        public uint Offset { get; init; }
        public string Text { get; init; }

        public Gxt2Entry()
        {
            
        }

        public Gxt2Entry(uint hash, string text)
        {
            Hash = hash;
            Text = text;
        }

        public Gxt2Entry(uint hash, string text, uint offset)
        {
            Hash = hash;
            Text = text;
            Offset = offset;
        }

        public override readonly string ToString()
        {
            return $"{Hash:X8}: {Text}";
        }
    }








    public static class GlobalText
    {
        public static ConcurrentDictionary<uint, string> Index = new (4, 24000);
        private static object syncRoot = new object();

        public static volatile bool FullIndexBuilt = false;

        public static void Clear()
        {
            Index.Clear();
        }

        public static bool Ensure(string str)
        {
            uint hash = JenkHash.GenHash(str);
            if (hash == 0) return true;
            return !Index.TryAdd(hash, str);
        }

        public static void Ensure(string str, uint hash)
        {
            if (hash == 0)
                return;
            _ = Index.TryAdd(hash, str);
        }

        public static string GetString(uint hash)
        {
            string res;
            if (!Index.TryGetValue(hash, out res))
            {
                res = hash.ToString();
            }
            return res;
        }
        public static string TryGetString(uint hash)
        {
            string res;
            if (!Index.TryGetValue(hash, out res))
            {
                res = string.Empty;
            }
            return res;
        }

        public static bool TryGetString(uint hash, out string res) => Index.TryGetValue(hash, out res);

        public static uint TryFindHash(string text)
        {
            lock (syncRoot)
            {
                foreach (var kvp in Index)
                {
                    if (kvp.Value == text)
                    {
                        return kvp.Key;
                    }
                }
            }
            return 0;
        }

    }



}
