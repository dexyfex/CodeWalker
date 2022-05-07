using SharpDX;
using System.IO;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Text;
using System.Xml;

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


        public void WriteXml(StringBuilder sb, int indent)
        {
            //YpdbXml.ValueTag(sb, indent, "SerializerVersion", SerializerVersion.ToString());
            //YpdbXml.ValueTag(sb, indent, "PoseMatcherVersion", PoseMatcherVersion.ToString());
            YpdbXml.ValueTag(sb, indent, "Signature", Signature.ToString());
            //YpdbXml.ValueTag(sb, indent, "Unk7", FloatUtil.ToString(Unk7));
            //YpdbXml.ValueTag(sb, indent, "Unk8", Unk8.ToString());
            YpdbXml.WriteRawArray(sb, BoneTags, indent, "BoneTags", "");
            WeightSet?.WriteXml(sb, indent);
            YpdbXml.WriteItemArray(sb, Samples, indent, "Samples");
        }
        public void ReadXml(XmlNode node)
        {
            SerializerVersion = 2;// Xml.GetChildIntAttribute(node, "SerializerVersion");
            PoseMatcherVersion = 0;// Xml.GetChildIntAttribute(node, "PoseMatcherVersion");
            Signature = Xml.GetChildUIntAttribute(node, "Signature");
            Unk7 = 0.033333f;// Xml.GetChildFloatAttribute(node, "Unk7");
            Unk8 = 1;// Xml.GetChildIntAttribute(node, "Unk8");
            BoneTags = Xml.GetChildRawUshortArray(node, "BoneTags");
            BoneTagsCount = (BoneTags?.Length ?? 0);
            WeightSet = new PoseMatcherWeightSet(node);
            Samples = XmlMeta.ReadItemArray<PoseMatcherMatchSample>(node, "Samples");
            SamplesCount = (Samples?.Length ?? 0);
        }
    }

    [TC(typeof(EXP))] public class PoseMatcherMatchSample : IMetaXmlItem
    {
        // rage::crPoseMatcherData::MatchSample
        public MetaHash ClipSet { get; set; } // from clip_sets.ymt/xml
        public MetaHash Clip { get; set; }
        public float Offset { get; set; }//probably time offset, allows for multiple samples per clip
        public PoseMatcherPointCloud PointCloud { get; set; }

        public PoseMatcherMatchSample()
        { }
        public PoseMatcherMatchSample(DataReader r)
        {
            ClipSet = r.ReadUInt32();
            Clip = r.ReadUInt32();

            Offset = r.ReadSingle();

            PointCloud = new PoseMatcherPointCloud(r);

            switch (Offset)
            {
                case 0:
                case 0.266f:
                case 0.576f:
                case 0.366933f:
                case 0.599466f:
                case 0.506333f:
                case 1.09f:
                    break;
                default:
                    break;
            }
        }

        public void Write(DataWriter w)
        {
            w.Write(ClipSet);
            w.Write(Clip);

            w.Write(Offset);

            PointCloud.Write(w);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            YpdbXml.StringTag(sb, indent, "ClipSet", YpdbXml.HashString(ClipSet));
            YpdbXml.StringTag(sb, indent, "Clip", YpdbXml.HashString(Clip));
            YpdbXml.ValueTag(sb, indent, "Offset", FloatUtil.ToString(Offset));
            PointCloud?.WriteXml(sb, indent);
        }
        public void ReadXml(XmlNode node)
        {
            ClipSet = XmlMeta.GetHash(Xml.GetChildInnerText(node, "ClipSet"));
            Clip = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Clip"));
            Offset = Xml.GetChildFloatAttribute(node, "Offset");
            PointCloud = new PoseMatcherPointCloud(node);
        }

        public override string ToString()
        {
            return $"{ClipSet}, {Clip}";
        }
    }

    [TC(typeof(EXP))] public class PoseMatcherPointCloud
    {
        // rage::crpmPointCloud
        public int PointsCount { get; set; }
        public Vector3[] Points { get; set; }
        public int WeightsCount { get; set; } // == PointsCount
        public float[] Weights { get; set; }
        public Vector3 BoundsMin { get; set; }
        public Vector3 BoundsMax { get; set; }
        public float WeightsSum { get; set; }

        public PoseMatcherPointCloud(XmlNode n)
        {
            BoundsMin = Xml.GetChildVector3Attributes(n, "BoundsMin");
            BoundsMax = Xml.GetChildVector3Attributes(n, "BoundsMax");
            Points = Xml.GetChildRawVector3Array(n, "Points");
            PointsCount = (Points?.Length ?? 0);
            Weights = Xml.GetChildRawFloatArray(n, "Weights");
            WeightsCount = (Weights?.Length ?? 0);

            var sum = 0.0f;
            if (Weights != null) foreach (var v in Weights) sum += v;
            WeightsSum = sum;
        }
        public PoseMatcherPointCloud(DataReader r)
        {
            PointsCount = r.ReadInt32();
            if (PointsCount > 0)
            {
                Points = new Vector3[PointsCount];

                for (int i = 0; i < PointsCount; i++)
                    Points[i] = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            }

            WeightsCount = r.ReadInt32();
            if (WeightsCount > 0)
            {
                Weights = new float[WeightsCount];

                for (int i = 0; i < WeightsCount; i++)
                    Weights[i] = r.ReadSingle();
            }

            BoundsMin = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            BoundsMax = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

            WeightsSum = r.ReadSingle();

            if (PointsCount != WeightsCount)
            { } // no hit

            //var sum = 0.0f;
            //if (Weights != null) foreach (var v in Weights) sum += v;
            //if (WeightsSum != sum)
            //{ } //no hit
        }

        public void Write(DataWriter w)
        {
            w.Write(PointsCount);
            if (PointsCount > 0)
            {
                foreach (var point in Points)
                    w.Write(point);
            }

            w.Write(WeightsCount);
            if (WeightsCount > 0)
            {
                foreach (var entry in Weights)
                    w.Write(entry);
            }

            w.Write(BoundsMin);
            w.Write(BoundsMax);

            w.Write(WeightsSum);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            YpdbXml.SelfClosingTag(sb, indent, "BoundsMin " + FloatUtil.GetVector3XmlString(BoundsMin));
            YpdbXml.SelfClosingTag(sb, indent, "BoundsMax " + FloatUtil.GetVector3XmlString(BoundsMax));
            YpdbXml.WriteRawArray(sb, Points, indent, "Points", "", YpdbXml.FormatVector3, 1);
            YpdbXml.WriteRawArray(sb, Weights, indent, "Weights", "", FloatUtil.ToString);
        }
    }

    [TC(typeof(EXP))] public class PoseMatcherWeightSet
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
        public PoseMatcherWeightSet(XmlNode n)
        {
            Weights = Xml.GetChildRawFloatArray(n, "Weights");
            WeightsCount = (Weights?.Length ?? 0);
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
        
        public void WriteXml(StringBuilder sb, int indent)
        {
            YpdbXml.WriteRawArray(sb, Weights, indent, "Weights", "", FloatUtil.ToString);
        }
    }





    public class YpdbXml : MetaXmlBase
    {

        public static string GetXml(YpdbFile ypdb)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if ((ypdb != null) && (ypdb.WeightSet != null))
            {
                var name = "PoseMatcher";

                OpenTag(sb, 0, name);

                ypdb.WriteXml(sb, 1);

                CloseTag(sb, 0, name);
            }

            return sb.ToString();
        }

    }

    public class XmlYpdb
    {

        public static YpdbFile GetYpdb(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYpdb(doc);
        }

        public static YpdbFile GetYpdb(XmlDocument doc)
        {
            YpdbFile ypdb = new YpdbFile();
            ypdb.ReadXml(doc.DocumentElement);
            return ypdb;
        }

    }


}
