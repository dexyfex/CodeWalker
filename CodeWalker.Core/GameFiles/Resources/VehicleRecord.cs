using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{

    public class VehicleRecordList : ResourceFileBase
    {
        public override long BlockLength => 0x20;

        public ResourceSimpleList64<VehicleRecordEntry> Entries { get; set; }

        public VehicleRecordList()
        {
            this.Entries = new ResourceSimpleList64<VehicleRecordEntry>();
        }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            this.Entries = reader.ReadBlock<ResourceSimpleList64<VehicleRecordEntry>>();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            writer.WriteBlock(this.Entries);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {

            if (Entries?.data_items != null)
            {
                foreach (var e in Entries.data_items)
                {
                    YvrXml.OpenTag(sb, indent, "Item");
                    e.WriteXml(sb, indent + 1);
                    YvrXml.CloseTag(sb, indent, "Item");
                }
            }

        }
        public void ReadXml(XmlNode node)
        {
            var entries = new List<VehicleRecordEntry>();

            var inodes = node.SelectNodes("Item");
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var e = new VehicleRecordEntry();
                    e.ReadXml(inode);
                    entries.Add(e);
                }
            }

            Entries = new ResourceSimpleList64<VehicleRecordEntry>();
            Entries.data_items = entries.ToArray();

        }
        public static void WriteXmlNode(VehicleRecordList l, StringBuilder sb, int indent, string name = "VehicleRecordList")
        {
            if (l == null) return;
            if ((l.Entries?.data_items == null) || (l.Entries.data_items.Length == 0))
            {
                YvrXml.SelfClosingTag(sb, indent, name);
            }
            else
            {
                YvrXml.OpenTag(sb, indent, name);
                l.WriteXml(sb, indent + 1);
                YvrXml.CloseTag(sb, indent, name);
            }
        }
        public static VehicleRecordList ReadXmlNode(XmlNode node)
        {
            if (node == null) return null;
            var l = new VehicleRecordList();
            l.ReadXml(node);
            return l;
        }



        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, Entries)
            };
        }
    }


    public class VehicleRecordEntry : ResourceSystemBlock
    {
        // this looks exactly like an rrr entry:
        // -> http://www.gtamodding.com/wiki/Carrec

        public override long BlockLength => 0x20;

        // structure data
        public uint Time;
        public short VelocityX;
        public short VelocityY;
        public short VelocityZ;
        public sbyte RightX;
        public sbyte RightY;
        public sbyte RightZ;
        public sbyte TopX;
        public sbyte TopY;
        public sbyte TopZ;
        public byte SteeringAngle;
        public byte GasPedalPower;
        public byte BrakePedalPower;
        public byte HandbrakeUsed;
        public Vector3 Position;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Time = reader.ReadUInt32();
            this.VelocityX = reader.ReadInt16();
            this.VelocityY = reader.ReadInt16();
            this.VelocityZ = reader.ReadInt16();
            this.RightX = (sbyte)reader.ReadByte();
            this.RightY = (sbyte)reader.ReadByte();
            this.RightZ = (sbyte)reader.ReadByte();
            this.TopX = (sbyte)reader.ReadByte();
            this.TopY = (sbyte)reader.ReadByte();
            this.TopZ = (sbyte)reader.ReadByte();
            this.SteeringAngle = reader.ReadByte();
            this.GasPedalPower = reader.ReadByte();
            this.BrakePedalPower = reader.ReadByte();
            this.HandbrakeUsed = reader.ReadByte();
            this.Position = reader.ReadVector3();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Time);
            writer.Write(this.VelocityX);
            writer.Write(this.VelocityY);
            writer.Write(this.VelocityZ);
            writer.Write((byte)this.RightX);
            writer.Write((byte)this.RightY);
            writer.Write((byte)this.RightZ);
            writer.Write((byte)this.TopX);
            writer.Write((byte)this.TopY);
            writer.Write((byte)this.TopZ);
            writer.Write(this.SteeringAngle);
            writer.Write(this.GasPedalPower);
            writer.Write(this.BrakePedalPower);
            writer.Write(this.HandbrakeUsed);
            writer.Write(this.Position);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YvrXml.ValueTag(sb, indent, "Time", Time.ToString());
            YvrXml.SelfClosingTag(sb, indent, "Position " + FloatUtil.GetVector3XmlString(Position));
            YvrXml.SelfClosingTag(sb, indent, "Velocity " + string.Format("x=\"{0}\" y=\"{1}\" z=\"{2}\"", VelocityX.ToString(), VelocityY.ToString(), VelocityZ.ToString()));
            YvrXml.SelfClosingTag(sb, indent, "Right " + string.Format("x=\"{0}\" y=\"{1}\" z=\"{2}\"", RightX.ToString(), RightY.ToString(), RightZ.ToString()));
            YvrXml.SelfClosingTag(sb, indent, "Top " + string.Format("x=\"{0}\" y=\"{1}\" z=\"{2}\"", TopX.ToString(), TopY.ToString(), TopZ.ToString()));
            YvrXml.ValueTag(sb, indent, "SteeringAngle", SteeringAngle.ToString());
            YvrXml.ValueTag(sb, indent, "GasPedalPower", GasPedalPower.ToString());
            YvrXml.ValueTag(sb, indent, "BrakePedalPower", BrakePedalPower.ToString());
            YvrXml.ValueTag(sb, indent, "HandbrakeUsed", HandbrakeUsed.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Time = Xml.GetChildUIntAttribute(node, "Time", "value");
            Position = Xml.GetChildVector3Attributes(node, "Position");
            VelocityX = (short)Xml.GetChildIntAttribute(node, "Velocity", "x");
            VelocityY = (short)Xml.GetChildIntAttribute(node, "Velocity", "y");
            VelocityZ = (short)Xml.GetChildIntAttribute(node, "Velocity", "z");
            RightX = (sbyte)Xml.GetChildIntAttribute(node, "Right", "x");
            RightY = (sbyte)Xml.GetChildIntAttribute(node, "Right", "y");
            RightZ = (sbyte)Xml.GetChildIntAttribute(node, "Right", "z");
            TopX = (sbyte)Xml.GetChildIntAttribute(node, "Top", "x");
            TopY = (sbyte)Xml.GetChildIntAttribute(node, "Top", "y");
            TopZ = (sbyte)Xml.GetChildIntAttribute(node, "Top", "z");
            SteeringAngle = (byte)Xml.GetChildUIntAttribute(node, "SteeringAngle", "value");
            GasPedalPower = (byte)Xml.GetChildUIntAttribute(node, "GasPedalPower", "value");
            BrakePedalPower = (byte)Xml.GetChildUIntAttribute(node, "BrakePedalPower", "value");
            HandbrakeUsed = (byte)Xml.GetChildUIntAttribute(node, "HandbrakeUsed", "value");
        }

    }


}
