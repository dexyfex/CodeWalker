using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class DistantLightsFile : GameFile, PackedFile
    {
        public bool HD { get; set; } = true;
        public uint GridSize { get; set; } = 32;
        public uint CellSize { get; set; } = 512;
        public uint CellCount { get; set; } = 1024;
        public uint NodeCount { get; set; }
        public uint PathCount { get; set; }
        public uint[] PathIndices { get; set; } //CellCount
        public uint[] PathCounts1 { get; set; } //CellCount
        public uint[] PathCounts2 { get; set; } //CellCount
        public DistantLightsNode[] Nodes { get; set; } //NodeCount
        public DistantLightsPath[] Paths { get; set; } //PathCount
        public DistantLightsCell[] Cells { get; set; } //CellCount (built from loaded data)


        public DistantLightsFile() : base(null, GameFileType.DistantLights)
        {
        }
        public DistantLightsFile(RpfFileEntry entry) : base(entry, GameFileType.DistantLights)
        {
            RpfFileEntry = entry;
        }


        public void Load(byte[] data, RpfFileEntry entry)
        {
            if (entry != null)
            {
                RpfFileEntry = entry;
                Name = entry.Name;

                if (!entry.NameLower.EndsWith("_hd.dat"))
                {
                    HD = false;
                    GridSize = 16;
                    CellSize = 1024;
                    CellCount = 256;
                }
            }

            using (MemoryStream ms = new MemoryStream(data))
            {
                DataReader r = new DataReader(ms, Endianess.BigEndian);

                Read(r);
            };

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
            NodeCount = r.ReadUInt32();
            PathCount = r.ReadUInt32();
            PathIndices = new uint[CellCount];
            PathCounts1 = new uint[CellCount];
            PathCounts2 = new uint[CellCount];
            Nodes = new DistantLightsNode[NodeCount];
            Paths = new DistantLightsPath[PathCount];
            for (uint i = 0; i < CellCount; i++)
            {
                PathIndices[i] = r.ReadUInt32();
            }
            for (uint i = 0; i < CellCount; i++)
            {
                PathCounts1[i] = r.ReadUInt32();
            }
            for (uint i = 0; i < CellCount; i++)
            {
                PathCounts2[i] = r.ReadUInt32();
            }
            for (uint i = 0; i < NodeCount; i++)
            {
                Nodes[i] = new DistantLightsNode(r);
            }
            for (uint i = 0; i < PathCount; i++)
            {
                Paths[i] = new DistantLightsPath(r, HD);
            }


            BuildCells();

        }
        private void Write(DataWriter w)
        {
            w.Write(NodeCount);
            w.Write(PathCount);

            for (uint i = 0; i < CellCount; i++)
            {
                w.Write(PathIndices[i]);
            }
            for (uint i = 0; i < CellCount; i++)
            {
                w.Write(PathCounts1[i]);
            }
            for (uint i = 0; i < CellCount; i++)
            {
                w.Write(PathCounts2[i]);
            }
            for (uint i = 0; i < NodeCount; i++)
            {
                Nodes[i].Write(w);
            }
            for (uint i = 0; i < PathCount; i++)
            {
                Paths[i].Write(w, HD);
            }

        }


        private void BuildCells()
        {
            for (uint i = 0; i < PathCount; i++)
            {
                var path = Paths[i];
                path.Nodes = new DistantLightsNode[path.NodeCount];
                for (uint n = 0; n < path.NodeCount; n++)
                {
                    path.Nodes[n] = Nodes[path.NodeIndex + n];
                }
            }

            Cells = new DistantLightsCell[CellCount];
            for (uint x = 0; x < GridSize; x++)
            {
                for (uint y = 0; y < GridSize; y++)
                {
                    var i = x * GridSize + y;
                    var cell = new DistantLightsCell();
                    cell.Index = i;
                    cell.CellX = x;
                    cell.CellY = y;
                    cell.CellMin = new Vector2(x, y) * CellSize - 8192.0f;
                    cell.CellMax = cell.CellMin + CellSize;
                    var pc1 = PathCounts1[i];
                    if (pc1 > 0)
                    {
                        cell.Paths1 = new DistantLightsPath[pc1];
                        for (uint l = 0; l < pc1; l++)
                        {
                            cell.Paths1[l] = Paths[PathIndices[i] + l];
                        }
                    }
                    var pc2 = PathCounts2[i];
                    if (pc2 > 0)
                    {
                        cell.Paths2 = new DistantLightsPath[pc2];
                        for (uint l = 0; l < pc2; l++)
                        {
                            cell.Paths2[l] = Paths[PathIndices[i] + l + pc1];
                        }
                    }
                    Cells[i] = cell;
                }
            }

        }

    }

    [TC(typeof(EXP))] public class DistantLightsNode 
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }

        public DistantLightsNode()
        { }
        public DistantLightsNode(DataReader r)
        {
            Read(r);
        }

        public void Read(DataReader r)
        {
            X = r.ReadInt16();
            Y = r.ReadInt16();
            Z = r.ReadInt16();
        }
        public void Write(DataWriter w)
        {
            w.Write(X);
            w.Write(Y);
            w.Write(Z);
        }

        public Vector3 Vector
        {
            get { return new Vector3(X, Y, Z); }
            set { X = (short)Math.Round(value.X); Y = (short)Math.Round(value.Y); Z = (short)Math.Round(value.Z); }
        }

        public override string ToString()
        {
            return Vector.ToString();
        }
    }

    [TC(typeof(EXP))] public class DistantLightsPath 
    {
        public short CenterX { get; set; }
        public short CenterY { get; set; }
        public ushort SizeX { get; set; }
        public ushort SizeY { get; set; }
        public ushort NodeIndex { get; set; }
        public ushort NodeCount { get; set; }
        public ushort Short7 { get; set; }
        public ushort Short8 { get; set; }
        public float Float1 { get; set; }
        public byte Byte1 { get; set; }
        public byte Byte2 { get; set; }
        public byte Byte3 { get; set; }
        public byte Byte4 { get; set; }

        public DistantLightsNode[] Nodes { get; set; }

        public DistantLightsPath()
        { }
        public DistantLightsPath(DataReader r, bool hd)
        {
            Read(r, hd);
        }

        public void Read(DataReader r, bool hd)
        {
            CenterX = r.ReadInt16();
            CenterY = r.ReadInt16();
            SizeX = r.ReadUInt16();
            SizeY = r.ReadUInt16();
            NodeIndex = r.ReadUInt16();
            NodeCount = r.ReadUInt16();
            if (hd)
            {
                Short7 = r.ReadUInt16();
                Short8 = r.ReadUInt16();
                Float1 = r.ReadSingle();
                Byte1 = r.ReadByte();
                Byte2 = r.ReadByte();
                Byte3 = r.ReadByte();
                Byte4 = r.ReadByte();
            }
            else
            {
                Byte1 = r.ReadByte();
                Byte2 = r.ReadByte();
            }
        }
        public void Write(DataWriter w, bool hd)
        {
            w.Write(CenterX);
            w.Write(CenterY);
            w.Write(SizeX);
            w.Write(SizeY);
            w.Write(NodeIndex);
            w.Write(NodeCount);
            if (hd)
            {
                w.Write(Short7);
                w.Write(Short8);
                w.Write(Float1);
                w.Write(Byte1);
                w.Write(Byte2);
                w.Write(Byte3);
                w.Write(Byte4);
            }
            else
            {
                w.Write(Byte1);
                w.Write(Byte2);
            }
        }

        public override string ToString()
        {
            return CenterX.ToString() + ", " + CenterY.ToString() + ", " + SizeX.ToString() + ", " + SizeY.ToString() + ", " +
                NodeIndex.ToString() + ", " + NodeCount.ToString() + ", " + Short7.ToString() + ", " + Short8.ToString() + ", " +
                FloatUtil.ToString(Float1) + ", " + Byte1.ToString() + ", " + Byte2.ToString() + ", " + Byte3.ToString() + ", " + Byte4.ToString();
        }
    }

    [TC(typeof(EXP))] public class DistantLightsCell 
    {
        public uint Index { get; set; }
        public uint CellX { get; set; }
        public uint CellY { get; set; }
        public Vector2 CellMin { get; set; }
        public Vector2 CellMax { get; set; }
        public DistantLightsPath[] Paths1 { get; set; }
        public DistantLightsPath[] Paths2 { get; set; }

        public override string ToString()
        {
            return Index.ToString() + " (" + CellX.ToString() + ", " + CellY.ToString() + ") - " +
                (Paths1?.Length ?? 0).ToString() + ", " + (Paths2?.Length ?? 0).ToString() + " - (" +
                CellMin.ToString() + " - " + CellMax.ToString() + ")";
        }
    }

}
