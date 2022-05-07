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

        public uint Unknown_10h; // 0x00000000
        public uint Unknown_14h; // 0x00000000
        public ulong EntriesPointer;
        public uint EntriesCount;
        public uint Unknown_24h; // 0x00000000
        public uint Unknown_28h; // 0x00000000
        public uint Unknown_2Ch; // 0x00000000

        public ResourceSimpleArray<WaypointRecordEntry> Entries;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
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
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
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
        public ushort Unk0;
        public ushort Unk1;
        public ushort Unk2;
        public ushort Unk3;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Position = reader.ReadVector3();
            this.Unk0 = reader.ReadUInt16();
            this.Unk1 = reader.ReadUInt16();
            this.Unk2 = reader.ReadUInt16();
            this.Unk3 = reader.ReadUInt16();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Position);
            writer.Write(this.Unk0);
            writer.Write(this.Unk1);
            writer.Write(this.Unk2);
            writer.Write(this.Unk3);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YwrXml.SelfClosingTag(sb, indent, "Position " + FloatUtil.GetVector3XmlString(Position));
            YwrXml.ValueTag(sb, indent, "Unk0", Unk0.ToString());
            YwrXml.ValueTag(sb, indent, "Unk1", Unk1.ToString());
            YwrXml.ValueTag(sb, indent, "Unk2", Unk2.ToString());
            YwrXml.ValueTag(sb, indent, "Unk3", Unk3.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Position = Xml.GetChildVector3Attributes(node, "Position");
            Unk0 = (ushort)Xml.GetChildUIntAttribute(node, "Unk0", "value");
            Unk1 = (ushort)Xml.GetChildUIntAttribute(node, "Unk1", "value");
            Unk2 = (ushort)Xml.GetChildUIntAttribute(node, "Unk2", "value");
            Unk3 = (ushort)Xml.GetChildUIntAttribute(node, "Unk3", "value");
        }

    }


}
