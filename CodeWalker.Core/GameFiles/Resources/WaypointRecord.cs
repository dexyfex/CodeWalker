using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            //// update structure data
            //this.EntriesPointer = (ulong)(this.Entries?.Position ?? 0);
            //this.EntriesCount = (uint)(this.Entries?.Count ?? 0);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.EntriesPointer);
            writer.Write(this.EntriesCount);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
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

        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public ushort Unk0;
        public ushort Unk1;
        public ushort Unk2;
        public ushort Unk3;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.PositionX = reader.ReadSingle();
            this.PositionY = reader.ReadSingle();
            this.PositionZ = reader.ReadSingle();
            this.Unk0 = reader.ReadUInt16();
            this.Unk1 = reader.ReadUInt16();
            this.Unk2 = reader.ReadUInt16();
            this.Unk3 = reader.ReadUInt16();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.PositionX);
            writer.Write(this.PositionY);
            writer.Write(this.PositionZ);
            writer.Write(this.Unk0);
            writer.Write(this.Unk1);
            writer.Write(this.Unk2);
            writer.Write(this.Unk3);
        }
    }


}
