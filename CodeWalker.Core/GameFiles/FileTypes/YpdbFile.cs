using SharpDX;
using System.IO;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{
    public class YpdbFile : GameFile, PackedFile
    {
        public int SerializerVersion { get; set; } // 2
        public int PoseMatcherVersion { get; set; } // 0
        public uint Signature { get; set; }
        public int SamplesCount { get; set; }
        public PoseMatcherMatchSample[] Samples { get; set; }
        public int BoneTagsCount { get; set; }
        public ushort[] BoneTags { get; set; }
        public PoseMatcherWeightSet WeightSet { get; set; }
        public float Unk7 { get; set; } // 0.033333f
        public int Unk8 { get; set; } // 1

        public YpdbFile() : base(null, GameFileType.Ypdb)
        {
        }
        public YpdbFile(RpfFileEntry entry) : base(entry, GameFileType.Ypdb)
        {
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            if (entry != null)
            {
                RpfFileEntry = entry;
                Name = entry.Name;
            }

            using (MemoryStream ms = new MemoryStream(data))
            {
                DataReader r = new DataReader(ms, Endianess.LittleEndian);

                Read(r);
            }

            Loaded = true;

        }

        public byte[] Save()
        {
            MemoryStream s = new MemoryStream();
            DataWriter w = new DataWriter(s);

            Write(w);

            var buf = new byte[s.Length];
            s.Position = 0;
            s.Read(buf, 0, buf.Length);
            return buf;
        }

        private void Read(DataReader r)
        {
            byte magic = r.ReadByte();
            if (magic != 0x1A) // 0x1A indicates to the game deserializer that it's binary instead of text
            { } // no hit

            SerializerVersion = r.ReadInt32();
            PoseMatcherVersion = r.ReadInt32();
            Signature = r.ReadUInt32();

            SamplesCount = r.ReadInt32();
            if (SamplesCount > 0)
            {
                Samples = new PoseMatcherMatchSample[SamplesCount];

                for (int i = 0; i < SamplesCount; i++)
                    Samples[i] = new PoseMatcherMatchSample(r);
            }

            BoneTagsCount = r.ReadInt32();
            if (BoneTagsCount > 0)
            {
                BoneTags = new ushort[BoneTagsCount];

                for (int i = 0; i < BoneTagsCount; i++)
                    BoneTags[i] = r.ReadUInt16();
            }

            WeightSet = new PoseMatcherWeightSet(r);

            Unk7 = r.ReadSingle();
            Unk8 = r.ReadInt32();

            uint signature2 = r.ReadUInt32();

            if (SerializerVersion != 2)
            { } // no hit
            if (PoseMatcherVersion != 0)
            { } // no hit
            if (BoneTagsCount != WeightSet.WeightsCount)
            { } // no hit
            if (Unk7 != 0.033333f)
            { } // no hit
            if (Unk8 != 1)
            { } // no hit
            if (Signature != signature2)
            { } // no hit

            if (r.Position != r.Length)
            { }
        }

        private void Write(DataWriter w)
        {
            w.Write((byte)0x1A);
            w.Write(SerializerVersion);
            w.Write(PoseMatcherVersion);
            w.Write(Signature);

            w.Write(SamplesCount);
            if (SamplesCount > 0)
            {
                foreach (var entry in Samples)
                    entry.Write(w);
            }

            w.Write(BoneTagsCount);
            if (BoneTagsCount > 0)
            {
                foreach (var boneTag in BoneTags)
                    w.Write(boneTag);
            }

            WeightSet.Write(w);

            w.Write(Unk7);
            w.Write(Unk8);

            w.Write(Signature);
        }
    }

    [TC(typeof(EXP))]
    public class PoseMatcherMatchSample
    {
        // rage::crPoseMatcherData::MatchSample
        public MetaHash ClipSet { get; set; } // from clip_sets.ymt/xml
        public MetaHash Clip { get; set; }
        public float Unk3 { get; set; }
        public PoseMatcherPointCloud PointCloud { get; set; }

        public PoseMatcherMatchSample(DataReader r)
        {
            ClipSet = r.ReadUInt32();
            Clip = r.ReadUInt32();

            Unk3 = r.ReadSingle();

            PointCloud = new PoseMatcherPointCloud(r);
        }

        public void Write(DataWriter w)
        {
            w.Write(ClipSet);
            w.Write(Clip);

            w.Write(Unk3);

            PointCloud.Write(w);
        }

        public override string ToString()
        {
            return $"{ClipSet}, {Clip}";
        }
    }

    [TC(typeof(EXP))]
    public class PoseMatcherPointCloud
    {
        // rage::crpmPointCloud
        public int PointsCount { get; set; }
        public Vector3[] Points { get; set; }
        public int Unk2_Count { get; set; } // == PointsCount
        public float[] Unk2_Items { get; set; }
        public Vector3 BoundsMin { get; set; }
        public Vector3 BoundsMax { get; set; }
        public float Unk5 { get; set; }

        public PoseMatcherPointCloud(DataReader r)
        {
            PointsCount = r.ReadInt32();
            if (PointsCount > 0)
            {
                Points = new Vector3[PointsCount];

                for (int i = 0; i < PointsCount; i++)
                    Points[i] = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            }

            Unk2_Count = r.ReadInt32();
            if (Unk2_Count > 0)
            {
                Unk2_Items = new float[Unk2_Count];

                for (int i = 0; i < Unk2_Count; i++)
                    Unk2_Items[i] = r.ReadSingle();
            }

            BoundsMin = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            BoundsMax = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

            Unk5 = r.ReadSingle();

            if (PointsCount != Unk2_Count)
            { } // no hit
        }

        public void Write(DataWriter w)
        {
            w.Write(PointsCount);
            if (PointsCount > 0)
            {
                foreach (var point in Points)
                    w.Write(point);
            }

            w.Write(Unk2_Count);
            if (Unk2_Count > 0)
            {
                foreach (var entry in Unk2_Items)
                    w.Write(entry);
            }

            w.Write(BoundsMin);
            w.Write(BoundsMax);

            w.Write(Unk5);
        }
    }

    [TC(typeof(EXP))]
    public class PoseMatcherWeightSet
    {
        // rage::crWeightSet
        public int WeightsCount { get; set; }
        public float[] Weights { get; set; }

        public PoseMatcherWeightSet(DataReader r)
        {
            WeightsCount = r.ReadInt32();
            if (WeightsCount > 0)
            {
                Weights = new float[WeightsCount];

                for (int i = 0; i < WeightsCount; i++)
                    Weights[i] = r.ReadSingle();
            }
        }

        public void Write(DataWriter w)
        {
            w.Write(WeightsCount);
            if (WeightsCount > 0)
            {
                foreach (var weight in Weights)
                    w.Write(weight);
            }
        }
    }
}
