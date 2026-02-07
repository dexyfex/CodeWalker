using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{

    public class WaypointRecordList : ResourceFileBase
    {
        public override long BlockLength => 0x30;

        public uint Flags1; // 0x00000000
        public uint Flags2; // 0x00000000
        public ulong EntriesPointer;
        public uint EntriesCount;
        public uint Unknown_24h; // 0x00000000
        public uint Unknown_28h; // 0x00000000
        public uint Unknown_2Ch; // 0x00000000

        public ResourceSimpleArray<WaypointRecordEntry> Entries;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            this.Flags1 = reader.ReadUInt32();
            this.Flags2 = reader.ReadUInt32();
            this.EntriesPointer = reader.ReadUInt64();
            this.EntriesCount = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();

            this.Entries = reader.ReadBlockAt<ResourceSimpleArray<WaypointRecordEntry>>(
                this.EntriesPointer, // offset
                this.EntriesCount
            );
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.EntriesPointer = (ulong)(this.Entries?.FilePosition ?? 0);
            this.EntriesCount = (uint)(this.Entries?.Count ?? 0);

            // write structure data
            writer.Write(this.Flags1);
            writer.Write(this.Flags2);
            writer.Write(this.EntriesPointer);
            writer.Write(this.EntriesCount);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {

            if (Entries?.Data != null)
            {
                foreach (var e in Entries.Data)
                {
                    YwrXml.OpenTag(sb, indent, "Item");
                    e.WriteXml(sb, indent + 1);
                    YwrXml.CloseTag(sb, indent, "Item");
                }
            }

        }
        public void ReadXml(XmlNode node)
        {
            var entries = new List<WaypointRecordEntry>();

            var inodes = node.SelectNodes("Item");
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var e = new WaypointRecordEntry();
                    e.ReadXml(inode);
                    entries.Add(e);
                }
            }

            Entries = new ResourceSimpleArray<WaypointRecordEntry>();
            Entries.Data = entries;

        }
        public static void WriteXmlNode(WaypointRecordList l, StringBuilder sb, int indent, string name = "WaypointRecordList")
        {
            if (l == null) return;
            if ((l.Entries?.Data == null) || (l.Entries.Data.Count == 0))
            {
                YwrXml.SelfClosingTag(sb, indent, name);
            }
            else
            {
                YwrXml.OpenTag(sb, indent, name);
                l.WriteXml(sb, indent + 1);
                YwrXml.CloseTag(sb, indent, name);
            }
        }
        public static WaypointRecordList ReadXmlNode(XmlNode node)
        {
            if (node == null) return null;
            var l = new WaypointRecordList();
            l.ReadXml(node);
            return l;
        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Entries != null) list.Add(Entries);
            return list.ToArray();
        }
    }


    public class WaypointRecordEntry : ResourceSystemBlock
    {
        public override long BlockLength => 20;

        public Vector3 Position;
        public Struct0 Flags0;
        public Struct1 Flags1;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            Position = reader.ReadVector3();
            Flags0 = new Struct0(reader.ReadUInt32());
            Flags1 = new Struct1(reader.ReadUInt32());
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            writer.Write(Position);
            writer.Write(Flags0.Value);
            writer.Write(Flags1.Value);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            YwrXml.SelfClosingTag(sb, indent, "Position " + FloatUtil.GetVector3XmlString(Position));
            Flags0.WriteXml(sb, indent, "Flags0");
            Flags1.WriteXml(sb, indent, "Flags1");
        }

        public void ReadXml(XmlNode node)
        {
            Position = Xml.GetChildVector3Attributes(node, "Position");
            Flags0 = Struct0.FromXml(Xml.GetChild((XmlElement)node, "Flags0"));
            Flags1 = Struct1.FromXml(Xml.GetChild((XmlElement)node, "Flags1"));
        }

        public struct Struct0
        {
            public uint Value;
            public ushort Flags // 1-15
            {
                get => (ushort)(Value & 0xFFFF);
                set => Value = (Value & 0xFFFF0000) | (uint)value;
            }
            public byte Heading // 16–23
            {
                get => (byte)((Value >> 16) & 0xFF);
                set => Value = (Value & 0xFF00FFFF) | ((uint)value << 16);
            }
            public byte MoveBlendRatio // 24–31
            {
                get => (byte)((Value >> 24) & 0xFF);
                set => Value = (Value & 0x00FFFFFF) | ((uint)value << 24);
            }
            public Struct0(uint v) => Value = v;
            public void WriteXml(StringBuilder sb, int indent, string name)
            {
                YwrXml.OpenTag(sb, indent, name);
                YwrXml.ValueTag(sb, indent + 1, "Flags", Flags.ToString());
                YwrXml.ValueTag(sb, indent + 1, "Heading", Heading.ToString());
                YwrXml.ValueTag(sb, indent + 1, "MoveBlendRatio", MoveBlendRatio.ToString());
                YwrXml.CloseTag(sb, indent, name);
            }
            public static Struct0 FromXml(XmlNode node)
            {
                var f = new Struct0();
                if (node == null) return f;
                f.Flags = (ushort)Xml.GetChildUIntAttribute(node, "Flags", "value");
                f.Heading = (byte)Xml.GetChildUIntAttribute(node, "Heading", "value");
                f.MoveBlendRatio = (byte)Xml.GetChildUIntAttribute(node, "MoveBlendRatio", "value");
                return f;
            }
        }

        public struct Struct1
        {
            public uint Value;
            public byte FreeSpaceOnLeft // 0–7
            {
                get => (byte)(Value & 0xFF);
                set => Value = (Value & 0xFFFFFF00) | value;
            }
            public byte FreeSpaceOnRight // 8–15
            {
                get => (byte)((Value >> 8) & 0xFF);
                set => Value = (Value & 0xFFFF00FF) | ((uint)value << 8);
            }
            public ushort Unused // 16–31
            {
                get => (ushort)((Value >> 16) & 0xFFFF);
                set => Value = (Value & 0x0000FFFF) | ((uint)value << 16);
            }
            public Struct1(uint v) => Value = v;
            public void WriteXml(StringBuilder sb, int indent, string name)
            {
                YwrXml.OpenTag(sb, indent, name);
                YwrXml.ValueTag(sb, indent + 1, "FreeSpaceOnLeft", FreeSpaceOnLeft.ToString());
                YwrXml.ValueTag(sb, indent + 1, "FreeSpaceOnRight", FreeSpaceOnRight.ToString());
                YwrXml.ValueTag(sb, indent + 1, "Unused", Unused.ToString());
                YwrXml.CloseTag(sb, indent, name);
            }
            public static Struct1 FromXml(XmlNode node)
            {
                var f = new Struct1();
                if (node == null) return f;
                f.FreeSpaceOnLeft = (byte)Xml.GetChildUIntAttribute(node, "FreeSpaceOnLeft", "value");
                f.FreeSpaceOnRight = (byte)Xml.GetChildUIntAttribute(node, "FreeSpaceOnRight", "value");
                f.Unused = (ushort)Xml.GetChildUIntAttribute(node, "Unused", "value");
                return f;
            }
        }
    }
}
