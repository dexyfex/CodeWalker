using System;
using System.IO;
using System.Text;
using System.Xml;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeWalker.GameFiles
{
    public class AudioWorldSectorsFile : GameFile, PackedFile
    {
        public byte[] RawFileData { get; set; }

        public const int NumSectorsX = 100;
        public const int NumSectorsY = 100;
        public const int NumSectors = NumSectorsX * NumSectorsY;

        [TC(typeof(EXP))]
        public struct AudSector
        {
            public byte numHighwayNodes { get; set; }
            public byte tallestBuilding { get; set; }
            public byte numBuildings { get; set; }
            public byte numTrees { get; set; }
            public bool isWaterSector { get; set; }

            public override string ToString()
            {
                return numHighwayNodes.ToString() + ": " + tallestBuilding.ToString() + ": " + numBuildings.ToString() + ": " + numTrees.ToString() + ": " + isWaterSector.ToString();
            }
        }

        public AudSector[] Sectors { get; set; } = new AudSector[NumSectors];
        public AudioWorldSectorsFile() : base(null, GameFileType.AudioWorldSectors) { }
        public AudioWorldSectorsFile(RpfFileEntry entry) : base(entry, GameFileType.AudioWorldSectors)
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

            using (var ms = new MemoryStream(data, false))
            {
                var r = new DataReader(ms);
                Read(r, (int)ms.Length);
            }
        }

        public byte[] Save()
        {
            using (var s = new MemoryStream())
            {
                var w = new DataWriter(s);
                Write(w);
                return s.ToArray();
            }
        }

        private void Read(DataReader r, int dataLength)
        {
            int expectedBytes = NumSectors * 4;
            int toReadBytes = Math.Min(expectedBytes, dataLength);

            for (int i = 0; i < NumSectors; i++)
            {
                if ((i * 4 + 4) > toReadBytes)
                {
                    Sectors[i] = default;
                    continue;
                }

                byte numHighwayNodes = r.ReadByte();
                byte tallestBuilding = r.ReadByte();
                byte numBuildings = r.ReadByte();
                byte bitfield = r.ReadByte();

                Sectors[i] = new AudSector
                {
                    numHighwayNodes = numHighwayNodes,
                    tallestBuilding = tallestBuilding,
                    numBuildings = numBuildings,
                    numTrees = (byte)(bitfield >> 1),
                    isWaterSector = (bitfield & 0x1) != 0
                };
            }
        }

        private void Write(DataWriter w)
        {
            for (int i = 0; i < NumSectors; i++)
            {
                var s = Sectors[i];
                byte bitfield = (byte)((s.numTrees << 1) | (s.isWaterSector ? 0x1 : 0x0));

                w.Write(s.numHighwayNodes);
                w.Write(s.tallestBuilding);
                w.Write(s.numBuildings);
                w.Write(bitfield);
            }
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            AudXml.OpenTag(sb, indent, "Sectors");

            for (int y = 0; y < NumSectorsY; y++)
            {
                for (int x = 0; x < NumSectorsX; x++)
                {
                    int i = y * NumSectorsX + x;
                    var s = Sectors[i];

                    AudXml.OpenTag(sb, indent + 1, $"Sector Index=\"{i}\" X=\"{x}\" Y=\"{y}\"");
                    AudXml.SelfClosingTag(sb, indent + 2, $"HighwayNodes value=\"{s.numHighwayNodes}\"");
                    AudXml.SelfClosingTag(sb, indent + 2, $"TallestBuilding value=\"{s.tallestBuilding}\"");
                    AudXml.SelfClosingTag(sb, indent + 2, $"NumBuildings value=\"{s.numBuildings}\"");
                    AudXml.SelfClosingTag(sb, indent + 2, $"NumTrees value=\"{s.numTrees}\"");
                    AudXml.SelfClosingTag(sb, indent + 2, $"IsWaterSector value=\"{(s.isWaterSector ? "true" : "false")}\"");
                    AudXml.CloseTag(sb, indent + 1, "Sector");
                }
            }
            AudXml.CloseTag(sb, indent, "Sectors");
        }

        public void ReadXml(XmlNode node)
        {
            var sectorsNode = Xml.GetChild((XmlElement)node, "Sectors");
            if (sectorsNode == null) throw new InvalidDataException("Missing <Sectors> node");

            var tmp = new AudSector[NumSectors];

            foreach (XmlNode sectorNode in sectorsNode.ChildNodes)
            {
                var se = sectorNode as XmlElement;
                if (se == null || se.Name != "Sector")
                    continue;

                int x = -1, y = -1, idx = -1;

                var sx = Xml.GetStringAttribute(se, "X");
                var sy = Xml.GetStringAttribute(se, "Y");
                var sIdx = Xml.GetStringAttribute(se, "Index");

                if (!string.IsNullOrEmpty(sx)) int.TryParse(sx, out x);
                if (!string.IsNullOrEmpty(sy)) int.TryParse(sy, out y);
                if (!string.IsNullOrEmpty(sIdx)) int.TryParse(sIdx, out idx);

                if (x < 0 || y < 0)
                {
                    if (idx < 0)
                        throw new InvalidDataException("Sector missing X/Y and Index");
                    y = idx / NumSectorsX;
                    x = idx - (y * NumSectorsX);
                }

                if ((uint)x >= NumSectorsX || (uint)y >= NumSectorsY)
                    throw new InvalidDataException($"Sector out of range X={x}, Y={y}");

                int i = y * NumSectorsX + x;

                byte numHighwayNodes = (byte)Xml.GetChildIntAttribute(se, "HighwayNodes", "value");
                byte tallestBuilding = (byte)Xml.GetChildIntAttribute(se, "TallestBuilding", "value");
                byte numBuildings = (byte)Xml.GetChildIntAttribute(se, "NumBuildings", "value");
                byte numTrees = (byte)Xml.GetChildIntAttribute(se, "NumTrees", "value");
                bool isWaterSector = Xml.GetChildBoolAttribute(se, "IsWaterSector", "value");

                tmp[i] = new AudSector
                {
                    numHighwayNodes = numHighwayNodes,
                    tallestBuilding = tallestBuilding,
                    numBuildings = numBuildings,
                    numTrees = numTrees,
                    isWaterSector = isWaterSector
                };
            }

            Sectors = tmp;
        }
    }
    public class AudXml : MetaXmlBase
    {
        public static string GetXml(AudioWorldSectorsFile awsf)
        {
            var sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if (awsf != null && awsf.Sectors != null)
            {
                var name = "AudioWorldSectors";
                OpenTag(sb, 0, name);

                awsf.WriteXml(sb, 1);

                CloseTag(sb, 0, name);
            }

            return sb.ToString();
        }
    }
    public class XmlAud
    {
        public static AudioWorldSectorsFile GetAudWorldSectors(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetAudWorldSectors(doc);
        }

        public static AudioWorldSectorsFile GetAudWorldSectors(XmlDocument doc)
        {
            var awsf = new AudioWorldSectorsFile();
            awsf.ReadXml(doc.DocumentElement);
            return awsf;
        }
    }
}