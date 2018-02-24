using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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
            Name = entry.Name;
            FileEntry = entry;
            //Dict = new Dictionary<uint, string>();

            using (BinaryReader br = new BinaryReader(new MemoryStream(data)))
            {
                uint gxt2 = br.ReadUInt32(); //"GXT2" - 1196971058
                if (gxt2 != 1196971058)
                { return; }

                EntryCount = br.ReadUInt32();
                TextEntries = new Gxt2Entry[EntryCount];
                for (uint i = 0; i < EntryCount; i++)
                {
                    var e = new Gxt2Entry();
                    e.Hash = br.ReadUInt32();
                    e.Offset = br.ReadUInt32();
                    TextEntries[i] = e;
                }

                gxt2 = br.ReadUInt32(); //another "GXT2"
                if (gxt2 != 1196971058)
                { return; }

                uint endpos = br.ReadUInt32();

                List<byte> buf = new List<byte>();

                for (uint i = 0; i < EntryCount; i++)
                {
                    var e = TextEntries[i];
                    br.BaseStream.Position = e.Offset;

                    buf.Clear();
                    byte b = br.ReadByte();
                    while ((b != 0) && (br.BaseStream.Position<endpos))
                    {
                        buf.Add(b);
                        b = br.ReadByte();
                    }
                    e.Text = Encoding.UTF8.GetString(buf.ToArray());

                    //Dict[e.Hash] = e.Text;
                }

            }
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class Gxt2Entry
    {
        public uint Hash { get; set; }
        public uint Offset { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return Convert.ToString(Hash, 16).ToUpper().PadLeft(8, '0') + ": " + Text;
        }
    }








    public static class GlobalText
    {
        public static Dictionary<uint, string> Index = new Dictionary<uint, string>();
        private static object syncRoot = new object();

        public static volatile bool FullIndexBuilt = false;

        public static void Clear()
        {
            lock (syncRoot)
            {
                Index.Clear();
            }
        }

        public static bool Ensure(string str)
        {
            uint hash = JenkHash.GenHash(str);
            if (hash == 0) return true;
            lock (syncRoot)
            {
                if (!Index.ContainsKey(hash))
                {
                    Index.Add(hash, str);
                    return false;
                }
            }
            return true;
        }

        public static bool Ensure(string str, uint hash)
        {
            if (hash == 0) return true;
            lock (syncRoot)
            {
                if (!Index.ContainsKey(hash))
                {
                    Index.Add(hash, str);
                    return false;
                }
            }
            return true;
        }

        public static string GetString(uint hash)
        {
            string res;
            lock (syncRoot)
            {
                if (!Index.TryGetValue(hash, out res))
                {
                    res = hash.ToString();
                }
            }
            return res;
        }
        public static string TryGetString(uint hash)
        {
            string res;
            lock (syncRoot)
            {
                if (!Index.TryGetValue(hash, out res))
                {
                    res = string.Empty;
                }
            }
            return res;
        }

    }



}
