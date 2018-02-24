using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{

    public class VehicleRecordList : ResourceFileBase
    {
        public override long BlockLength => 0x20;

        public ResourceSimpleList64<VehicleRecordEntry> Entries;

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
        public float PositionX;
        public float PositionY;
        public float PositionZ;

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
            this.PositionX = reader.ReadSingle();
            this.PositionY = reader.ReadSingle();
            this.PositionZ = reader.ReadSingle();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Time);
            writer.Write(this.VelocityX);
            writer.Write(this.VelocityY);
            writer.Write(this.VelocityZ);
            writer.Write(this.RightX);
            writer.Write(this.RightY);
            writer.Write(this.RightZ);
            writer.Write(this.TopX);
            writer.Write(this.TopY);
            writer.Write(this.TopZ);
            writer.Write(this.SteeringAngle);
            writer.Write(this.GasPedalPower);
            writer.Write(this.BrakePedalPower);
            writer.Write(this.HandbrakeUsed);
            writer.Write(this.PositionX);
            writer.Write(this.PositionY);
            writer.Write(this.PositionZ);
        }
    }


}
