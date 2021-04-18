using System;
using System.Collections.Generic;
using System.Text;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using SharpDX;
using System.IO;
using System.Xml;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))]
    public class HeightmapFile : GameFile, PackedFile
    {
        public byte[] RawFileData { get; set; }
        public Endianess Endianess { get; set; } = Endianess.BigEndian;

        public uint Magic { get; set; } = 0x484D4150; //'HMAP'
        public byte VersionMajor { get; set; } = 1;
        public byte VersionMinor { get; set; } = 1;
        public ushort Pad { get; set; }
        public uint Compressed { get; set; } = 1;
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public Vector3 BBMin { get; set; }
        public Vector3 BBMax { get; set; }
        public uint Length { get; set; }
        public CompHeader[] CompHeaders { get; set; }
        public byte[] MaxHeights { get; set; }
        public byte[] MinHeights { get; set; }

        public HeightmapFile() : base(null, GameFileType.Heightmap)
        {
        }
        public HeightmapFile(RpfFileEntry entry) : base(entry, GameFileType.Heightmap)
        {
            RpfFileEntry = entry;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RawFileData = data;
            if (entry != null)
            {
                RpfFileEntry = entry;
                Name = entry.Name;
            }

            if (BitConverter.ToUInt32(data, 0) == Magic)
            {
                Endianess = Endianess.LittleEndian;
            }

            using (MemoryStream ms = new MemoryStream(data))
            {
                DataReader r = new DataReader(ms, Endianess);

                Read(r);
            }

            //var pgm = GetPGM();

        }

        public byte[] Save()
        {
            MemoryStream s = new MemoryStream();
            DataWriter w = new DataWriter(s, Endianess);

            Write(w);

            var buf = new byte[s.Length];
            s.Position = 0;
            s.Read(buf, 0, buf.Length);
            return buf;
        }

        private void Read(DataReader r)
        {
            Magic = r.ReadUInt32();
            VersionMajor = r.ReadByte();
            VersionMinor = r.ReadByte();
            Pad = r.ReadUInt16();
            Compressed = r.ReadUInt32();
            Width = r.ReadUInt16();
            Height = r.ReadUInt16();
            BBMin = r.ReadVector3();
            BBMax = r.ReadVector3();
            Length = r.ReadUInt32();


            if (Length != (r.Length - r.Position))
            { }


            var dlen = (int)Length;
            if (Compressed > 0)
            {
                CompHeaders = new CompHeader[Height];
                for (int i = 0; i < Height; i++)
                {
                    CompHeaders[i].Read(r);
                }
                dlen -= (Height * 8);
            }

            if ((r.Length - r.Position) != dlen)
            { }

            var d = r.ReadBytes(dlen);

            if ((r.Length - r.Position) != 0)
            { }

            if (Compressed > 0)
            {
                MaxHeights = new byte[Width * Height];
                MinHeights = new byte[Width * Height];
                var h2off = dlen / 2; //is this right?
                for (int y = 0; y < Height; y++)
                {
                    var h = CompHeaders[y];
                    for (int i = 0; i < h.Count; i++)
                    {
                        int x = h.Start + i;
                        int o = h.DataOffset + x;
                        MaxHeights[y * Width + x] = d[o];
                        MinHeights[y * Width + x] = d[o + h2off];
                    }
                    for (int x = 0; x < Width; x++)
                    {
                        var hm1v = MaxHeights[y * Width + x];
                        var hm2v = MinHeights[y * Width + x];
                        var diff = hm1v - hm2v;
                        if ((diff <= 0) && (hm1v != 0))
                        { }
                    }
                }
            }
            else
            {
                MaxHeights = d; //no way to test this as vanilla heightmaps are compressed...
                MinHeights = d; //this won't work anyway.
            }

        }
        private void Write(DataWriter w)
        {
            var d = MaxHeights;
            if (Compressed > 0)
            {
                var ch = new CompHeader[Height];
                var d1 = new List<byte>();
                var d2 = new List<byte>();
                for (int y = 0; y < Height; y++)
                {
                    var start = 0;
                    var end = 0;
                    for (int x = 0; x < Width; x++)
                    {
                        if (MaxHeights[y * Width + x] != 0) { start = x; break; }
                    }
                    for (int x = Width - 1; x >= 0; x--)
                    {
                        if (MaxHeights[y * Width + x] != 0) { end = x + 1; break; }
                    }
                    var count = end - start;
                    var offset = (count > 0) ? d1.Count - start : 0;
                    for (int i = 0; i < count; i++)
                    {
                        var x = start + i;
                        var n = y * Width + x;
                        d1.Add(MaxHeights[n]);
                        d2.Add(MinHeights[n]);
                    }
                    var h = new CompHeader() { Start = (ushort)start, Count = (ushort)count, DataOffset = offset };
                    ch[y] = h;
                }
                d1.AddRange(d2);//the 2 sets of compressed data are just smushed together
                d = d1.ToArray();
                CompHeaders = ch;
                Length = (uint)(d.Length + Height * 8);
            }
            else
            {
                Length = (uint)d.Length;
            }


            w.Write(Magic);
            w.Write(VersionMajor);
            w.Write(VersionMinor);
            w.Write(Pad);
            w.Write(Compressed);
            w.Write(Width);
            w.Write(Height);
            w.Write(BBMin);
            w.Write(BBMax);
            w.Write(Length);
            if (Compressed > 0)
            {
                for (int i = 0; i < Height; i++)
                {
                    CompHeaders[i].Write(w);
                }
            }
            w.Write(d);
        }


        public void WriteXml(StringBuilder sb, int indent)
        {
            if (Endianess != Endianess.BigEndian)
            {
                HmapXml.StringTag(sb, indent, "Endianess", Endianess.ToString());
            }
            HmapXml.ValueTag(sb, indent, "Width", Width.ToString());
            HmapXml.ValueTag(sb, indent, "Height", Height.ToString());
            HmapXml.SelfClosingTag(sb, indent, "BBMin " + FloatUtil.GetVector3XmlString(BBMin));
            HmapXml.SelfClosingTag(sb, indent, "BBMax " + FloatUtil.GetVector3XmlString(BBMax));
            HmapXml.WriteRawArray(sb, InvertImage(MaxHeights, Width, Height), indent, "MaxHeights", "", HmapXml.FormatHexByte, Width);
            HmapXml.WriteRawArray(sb, InvertImage(MinHeights, Width, Height), indent, "MinHeights", "", HmapXml.FormatHexByte, Width);
        }
        public void ReadXml(XmlNode node)
        {
            var endianess = Xml.GetChildInnerText(node, "Endianess");
            if (!string.IsNullOrEmpty(endianess))
            {
                var end = Endianess.BigEndian;
                Enum.TryParse(endianess, out end);
                Endianess = end;
            }
            Width = (ushort)Xml.GetChildUIntAttribute(node, "Width");
            Height = (ushort)Xml.GetChildUIntAttribute(node, "Height");
            BBMin = Xml.GetChildVector3Attributes(node, "BBMin");
            BBMax = Xml.GetChildVector3Attributes(node, "BBMax");
            MaxHeights = InvertImage(Xml.GetChildRawByteArray(node, "MaxHeights"), Width, Height);
            MinHeights = InvertImage(Xml.GetChildRawByteArray(node, "MinHeights"), Width, Height);
        }






        private byte[] InvertImage(byte[] i, int w, int h)
        {
            //inverts the image vertically
            byte[] o = new byte[i.Length];
            for (int y = 0; y < h; y++)
            {
                int io = y * w;
                int oo = (h - y - 1) * w;
                Buffer.BlockCopy(i, io, o, oo, w);
            }
            return o;
        }





        public string GetPGM()
        {
            if (MaxHeights == null) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendFormat("P2\n{0} {1}\n255\n", Width, Height);

            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    var h = MaxHeights[y * Width + x];
                    sb.Append(h.ToString());
                    sb.Append(" ");
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }



        public struct CompHeader
        {
            public ushort Start;
            public ushort Count;
            public int DataOffset;

            public void Read(DataReader r)
            {
                Start = r.ReadUInt16();
                Count = r.ReadUInt16();
                DataOffset = r.ReadInt32();
            }
            public void Write(DataWriter w)
            {
                w.Write(Start);
                w.Write(Count);
                w.Write(DataOffset);
            }

            public override string ToString()
            {
                return Start.ToString() + ", " + Count.ToString() + ", " + DataOffset.ToString();
            }
        }

    }


    public class HmapXml : MetaXmlBase
    {

        public static string GetXml(HeightmapFile hmf)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if ((hmf != null) && (hmf.MaxHeights != null))
            {
                var name = "Heightmap";

                OpenTag(sb, 0, name);

                hmf.WriteXml(sb, 1);

                CloseTag(sb, 0, name);
            }

            return sb.ToString();
        }


    }


    public class XmlHmap
    {

        public static HeightmapFile GetHeightmap(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetHeightmap(doc);
        }

        public static HeightmapFile GetHeightmap(XmlDocument doc)
        {
            HeightmapFile hmf = new HeightmapFile();
            hmf.ReadXml(doc.DocumentElement);
            return hmf;
        }


    }



}
