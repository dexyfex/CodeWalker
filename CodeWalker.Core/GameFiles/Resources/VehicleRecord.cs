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
        public short VelocityX; //factor to convert to m/s is 273.0583 .. or 1/0.0036622214, or 32767/120
        public short VelocityY;
        public short VelocityZ;
        public sbyte RightX;
        public sbyte RightY;
        public sbyte RightZ;
        public sbyte ForwardX;
        public sbyte ForwardY;
        public sbyte ForwardZ;
        public sbyte SteeringAngle; // factor to convert to game angle is 20  (ie radians)
        public sbyte GasPedalPower; //-100 to +100, negative = reverse
        public sbyte BrakePedalPower;//0 to 100
        public byte HandbrakeUsed;//0 or 1
        public Vector3 Position;

        public Vector3 Velocity
        {
            get
            {
                return new Vector3(VelocityX / 273.0583f, VelocityY / 273.0583f, VelocityZ / 273.0583f);
            }
            set
            {
                VelocityX = (short)Math.Round(value.X * 273.0583f);
                VelocityY = (short)Math.Round(value.Y * 273.0583f);
                VelocityZ = (short)Math.Round(value.Z * 273.0583f);
            }
        }
        public Vector3 Forward
        {
            get
            {
                return new Vector3(ForwardX / 127.0f, ForwardY / 127.0f, ForwardZ / 127.0f);
            }
            set
            {
                ForwardX = (sbyte)Math.Round(value.X * 127.0f);
                ForwardY = (sbyte)Math.Round(value.Y * 127.0f);
                ForwardZ = (sbyte)Math.Round(value.Z * 127.0f);
            }
        }
        public Vector3 Right
        {
            get
            {
                return new Vector3(RightX / 127.0f, RightY / 127.0f, RightZ / 127.0f);
            }
            set
            {
                RightX = (sbyte)Math.Round(value.X * 127.0f);
                RightY = (sbyte)Math.Round(value.Y * 127.0f);
                RightZ = (sbyte)Math.Round(value.Z * 127.0f);
            }
        }
        public float Steering
        {
            get
            {
                return SteeringAngle / 20.0f;
            }
            set
            {
                SteeringAngle = (sbyte)Math.Round(value * 20.0f);
            }
        }
        public float GasPedal
        {
            get
            {
                return GasPedalPower / 100.0f;
            }
            set
            {
                GasPedalPower = (sbyte)Math.Round(value * 100.0f);
            }
        }
        public float BrakePedal
        {
            get
            {
                return BrakePedalPower / 100.0f;
            }
            set
            {
                BrakePedalPower = (sbyte)Math.Round(value * 100.0f);
            }
        }
        public bool Handbrake
        {
            get
            {
                return HandbrakeUsed == 1;
            }
            set
            {
                HandbrakeUsed = value ? (byte)1 : (byte)0;
            }
        }

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
            this.ForwardX = (sbyte)reader.ReadByte();
            this.ForwardY = (sbyte)reader.ReadByte();
            this.ForwardZ = (sbyte)reader.ReadByte();
            this.SteeringAngle = (sbyte)reader.ReadByte();
            this.GasPedalPower = (sbyte)reader.ReadByte();
            this.BrakePedalPower = (sbyte)reader.ReadByte();
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
            writer.Write((byte)this.ForwardX);
            writer.Write((byte)this.ForwardY);
            writer.Write((byte)this.ForwardZ);
            writer.Write((byte)this.SteeringAngle);
            writer.Write((byte)this.GasPedalPower);
            writer.Write((byte)this.BrakePedalPower);
            writer.Write(this.HandbrakeUsed);
            writer.Write(this.Position);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YvrXml.ValueTag(sb, indent, "Time", Time.ToString());
            YvrXml.SelfClosingTag(sb, indent, "Position " + FloatUtil.GetVector3XmlString(Position));
            YvrXml.SelfClosingTag(sb, indent, "Velocity " + FloatUtil.GetVector3XmlString(Velocity));
            YvrXml.SelfClosingTag(sb, indent, "Forward " + FloatUtil.GetVector3XmlString(Forward));
            YvrXml.SelfClosingTag(sb, indent, "Right " + FloatUtil.GetVector3XmlString(Right));
            YvrXml.ValueTag(sb, indent, "Steering", FloatUtil.ToString(Steering));
            YvrXml.ValueTag(sb, indent, "GasPedal", FloatUtil.ToString(GasPedal));
            YvrXml.ValueTag(sb, indent, "BrakePedal", FloatUtil.ToString(BrakePedal));
            YvrXml.ValueTag(sb, indent, "Handbrake", Handbrake.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Time = Xml.GetChildUIntAttribute(node, "Time", "value");
            Position = Xml.GetChildVector3Attributes(node, "Position");
            Velocity = Xml.GetChildVector3Attributes(node, "Velocity");
            Forward = Xml.GetChildVector3Attributes(node, "Forward");
            Right = Xml.GetChildVector3Attributes(node, "Right");
            Steering = Xml.GetChildFloatAttribute(node, "Steering", "value");
            GasPedal = Xml.GetChildFloatAttribute(node, "GasPedal", "value");
            BrakePedal = Xml.GetChildFloatAttribute(node, "BrakePedal", "value");
            Handbrake = Xml.GetChildBoolAttribute(node, "Handbrake", "value");
        }

    }


}
