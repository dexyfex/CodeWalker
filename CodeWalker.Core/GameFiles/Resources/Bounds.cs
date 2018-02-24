/*
    Copyright(c) 2015 Neodymium

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

//shamelessly stolen and mangled


using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;


namespace CodeWalker.GameFiles
{


    [TC(typeof(EXP))] public class BoundsDictionary : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000001
        public uint Unknown_14h { get; set; } // 0x00000001
        public uint Unknown_18h { get; set; } // 0x00000001
        public uint Unknown_1Ch { get; set; } // 0x00000001
        //public ResourceSimpleList64<uint_r> BoundNameHashes;
        public ResourceSimpleList64Ptr BoundNameHashesPtr { get; set; }
        public uint[] BoundNameHashes { get; set; }
        public ResourcePointerList64<Bounds> Bounds { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            //this.BoundNameHashes = reader.ReadBlock<ResourceSimpleList64<uint_r>>();
            this.BoundNameHashesPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.BoundNameHashes = reader.ReadUintsAt(this.BoundNameHashesPtr.EntriesPointer, this.BoundNameHashesPtr.EntriesCount);
            this.Bounds = reader.ReadBlock<ResourcePointerList64<Bounds>>();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            //writer.WriteBlock(this.BoundNameHashes); //TODO: fix!
            //writer.WriteBlock(this.Bounds);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                //new Tuple<long, IResourceBlock>(0x20, BoundNameHashes), //TODO: fix!
                new Tuple<long, IResourceBlock>(0x30, Bounds)
            };
        }
    }

    [TC(typeof(EXP))] public class Bounds : ResourceFileBase, IResourceXXSystemBlock
    {
        public override long BlockLength
        {
            get { return 112; }
        }

        // structure data
        public byte Type { get; set; }
        public byte Unknown_11h { get; set; }
        public ushort Unknown_12h { get; set; }
        public float BoundingSphereRadius { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }
        public SharpDX.Vector3 BoundingBoxMax { get; set; }
        public float Margin { get; set; }
        public SharpDX.Vector3 BoundingBoxMin { get; set; }
        public uint Unknown_3Ch { get; set; }
        public SharpDX.Vector3 BoundingBoxCenter { get; set; }
        public byte MaterialIndex { get; set; }
        public byte ProceduralId { get; set; }
        public byte RoomId_and_PedDensity { get; set; } //5bits for RoomID and then 3bits for PedDensity
        public byte Unknown_4Fh { get; set; } //flags? (bit5 related to Unknown_5Ch, should be a flag called "Has PolyFlags")<-- i don't remember why i wrote this lol
        public SharpDX.Vector3 Center { get; set; }
        public byte PolyFlags { get; set; }
        public byte MaterialColorIndex { get; set; }
        public ushort Unknown_5Eh { get; set; }
        public float Unknown_60h { get; set; }
        public float Unknown_64h { get; set; }
        public float Unknown_68h { get; set; }
        public float BoundingBoxVolume { get; set; }


        public Bounds Parent { get; set; }
        public string OwnerName { get; set; }
        public string GetName()
        {
            string n = OwnerName;
            var p = Parent;
            while (p != null)
            {
                n = p.OwnerName;
                p = p.Parent;
            }
            return n;
        }


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Type = reader.ReadByte();
            this.Unknown_11h = reader.ReadByte();
            this.Unknown_12h = reader.ReadUInt16();
            this.BoundingSphereRadius = reader.ReadSingle();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.BoundingBoxMax = reader.ReadStruct<SharpDX.Vector3>();
            this.Margin = reader.ReadSingle();
            this.BoundingBoxMin = reader.ReadStruct<SharpDX.Vector3>();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.BoundingBoxCenter = reader.ReadStruct<SharpDX.Vector3>();
            this.MaterialIndex = reader.ReadByte();
            this.ProceduralId = reader.ReadByte();
            this.RoomId_and_PedDensity = reader.ReadByte();
            this.Unknown_4Fh = reader.ReadByte();
            this.Center = reader.ReadStruct<SharpDX.Vector3>();
            this.PolyFlags = reader.ReadByte();
            this.MaterialColorIndex = reader.ReadByte();
            this.Unknown_5Eh = reader.ReadUInt16();
            this.Unknown_60h = reader.ReadSingle();
            this.Unknown_64h = reader.ReadSingle();
            this.Unknown_68h = reader.ReadSingle();
            this.BoundingBoxVolume = reader.ReadSingle();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Type);
            writer.Write(this.Unknown_11h);
            writer.Write(this.Unknown_12h);
            writer.Write(this.BoundingSphereRadius);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            //writer.WriteBlock(this.BoundingBoxMax); //TODO: FIX!!
            writer.Write(this.Margin);
            //writer.WriteBlock(this.BoundingBoxMin);
            writer.Write(this.Unknown_3Ch);
            //writer.WriteBlock(this.BoundingBoxCenter);
            writer.Write(this.MaterialIndex);
            writer.Write(this.ProceduralId);
            writer.Write(this.RoomId_and_PedDensity);
            writer.Write(this.Unknown_4Fh);
            //writer.WriteBlock(this.Center);
            writer.Write(this.PolyFlags);
            writer.Write(this.MaterialColorIndex);
            writer.Write(this.Unknown_5Eh);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.BoundingBoxVolume);
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 16;
            var type = reader.ReadByte();
            reader.Position -= 17;

            switch (type)
            {
                case 0: return new BoundSphere();
                case 1: return new BoundCapsule();
                case 3: return new BoundBox();
                case 4: return new BoundGeometry();
                case 8: return new BoundBVH();
                case 10: return new BoundComposite();
                case 12: return new BoundDisc();
                case 13: return new BoundCylinder();
                case 15: return null; //TODO: find out what this is!
                default: return null; // throw new Exception("Unknown bound type");
            }
        }
    }
    [TC(typeof(EXP))] public class BoundSphere : Bounds
    { }
    [TC(typeof(EXP))] public class BoundCapsule : Bounds
    {
        public override long BlockLength
        {
            get { return 128; }
        }

        // structure data
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public uint Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.Unknown_78h = reader.ReadUInt32();
            this.Unknown_7Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_7Ch);
        }
    }
    [TC(typeof(EXP))] public class BoundBox : Bounds
    { }
    [TC(typeof(EXP))] public class BoundGeometry : Bounds
    {
        public override long BlockLength
        {
            get { return 304; }
        }

        // structure data
        public uint Unknown_70h { get; set; }
        public uint Unknown_74h { get; set; }
        public ulong Unknown_78h_Pointer { get; set; }
        public uint Unknown_80h { get; set; }
        public uint Count1 { get; set; }
        public ulong PolygonsPointer { get; set; }
        public Vector3 Quantum { get; set; }
        public float Unknown_9Ch { get; set; }
        public Vector3 CenterGeom { get; set; }
        public float Unknown_ACh { get; set; }
        public ulong VerticesPointer { get; set; }
        public ulong Unknown_B8h_Pointer { get; set; }
        public ulong Unknown_C0h_Pointer { get; set; }
        public ulong Unknown_C8h_Pointer { get; set; }
        public uint VerticesCount { get; set; }
        public uint PolygonsCount { get; set; }
        public uint Unknown_D8h { get; set; } // 0x00000000
        public uint Unknown_DCh { get; set; } // 0x00000000
        public uint Unknown_E0h { get; set; } // 0x00000000
        public uint Unknown_E4h { get; set; } // 0x00000000
        public uint Unknown_E8h { get; set; } // 0x00000000
        public uint Unknown_ECh { get; set; } // 0x00000000
        public ulong MaterialsPointer { get; set; }
        public ulong MaterialColoursPointer { get; set; }
        public uint Unknown_100h { get; set; } // 0x00000000
        public uint Unknown_104h { get; set; } // 0x00000000
        public uint Unknown_108h { get; set; } // 0x00000000
        public uint Unknown_10Ch { get; set; } // 0x00000000
        public uint Unknown_110h { get; set; } // 0x00000000
        public uint Unknown_114h { get; set; } // 0x00000000
        public ulong PolygonMaterialIndicesPointer { get; set; }
        public byte MaterialsCount { get; set; }
        public byte MaterialColoursCount { get; set; }
        public ushort Unknown_122h { get; set; } // 0x0000
        public uint Unknown_124h { get; set; } // 0x00000000
        public uint Unknown_128h { get; set; } // 0x00000000
        public uint Unknown_12Ch { get; set; } // 0x00000000


        public BoundVertex_s[] p1data { get; set; }

        public BoundPolygon[] Polygons { get; set; }

        public Vector3[] Vertices { get; set; }
        public uint[] Unknown_B8h_Data { get; set; }
        public uint[] Unknown_C0h_Data { get; set; }
        public uint[][] Unknown_C8h_Data { get; set; }
        public BoundMaterial_s[] Materials { get; set; }
        public BoundMaterialColour[] MaterialColours { get; set; }
        public byte[] PolygonMaterialIndices { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.Unknown_78h_Pointer = reader.ReadUInt64();
            this.Unknown_80h = reader.ReadUInt32();
            this.Count1 = reader.ReadUInt32();
            this.PolygonsPointer = reader.ReadUInt64();
            this.Quantum = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            this.Unknown_9Ch = reader.ReadSingle();//.ReadUInt32();
            this.CenterGeom = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            this.Unknown_ACh = reader.ReadSingle();
            this.VerticesPointer = reader.ReadUInt64();
            this.Unknown_B8h_Pointer = reader.ReadUInt64();
            this.Unknown_C0h_Pointer = reader.ReadUInt64();
            this.Unknown_C8h_Pointer = reader.ReadUInt64();
            this.VerticesCount = reader.ReadUInt32();
            this.PolygonsCount = reader.ReadUInt32();
            this.Unknown_D8h = reader.ReadUInt32();
            this.Unknown_DCh = reader.ReadUInt32();
            this.Unknown_E0h = reader.ReadUInt32();
            this.Unknown_E4h = reader.ReadUInt32();
            this.Unknown_E8h = reader.ReadUInt32();
            this.Unknown_ECh = reader.ReadUInt32();
            this.MaterialsPointer = reader.ReadUInt64();
            this.MaterialColoursPointer = reader.ReadUInt64();
            this.Unknown_100h = reader.ReadUInt32();
            this.Unknown_104h = reader.ReadUInt32();
            this.Unknown_108h = reader.ReadUInt32();
            this.Unknown_10Ch = reader.ReadUInt32();
            this.Unknown_110h = reader.ReadUInt32();
            this.Unknown_114h = reader.ReadUInt32();
            this.PolygonMaterialIndicesPointer = reader.ReadUInt64();
            this.MaterialsCount = reader.ReadByte();
            this.MaterialColoursCount = reader.ReadByte();
            this.Unknown_122h = reader.ReadUInt16();
            this.Unknown_124h = reader.ReadUInt32();
            this.Unknown_128h = reader.ReadUInt32();
            this.Unknown_12Ch = reader.ReadUInt32();


            this.p1data = reader.ReadStructsAt<BoundVertex_s>(this.Unknown_78h_Pointer, this.VerticesCount);
            if (p1data != null)
            { } //seems to be in YFT's

            ReadPolygons(reader);

            var verts = reader.ReadStructsAt<BoundVertex_s>(this.VerticesPointer, this.VerticesCount);
            if (verts != null)
            {
                Vertices = new Vector3[verts.Length];
                for (int i = 0; i < verts.Length; i++)
                {
                    var bv = verts[i];
                    Vertices[i] = new Vector3(bv.X, bv.Y, bv.Z) * Quantum;
                }
            }

            this.Unknown_B8h_Data = reader.ReadUintsAt(this.Unknown_B8h_Pointer, this.VerticesCount);
            this.Unknown_C0h_Data = reader.ReadUintsAt(this.Unknown_C0h_Pointer, 8);

            if (this.Unknown_C0h_Data != null)
            {
                ulong[] ptrlist = reader.ReadUlongsAt(this.Unknown_C8h_Pointer, (uint)Unknown_C0h_Data.Length);//8
                //reader.Position += Unknown_C0h_Data.Length * 8; //account for ptrlist read
                Unknown_C8h_Data = new uint[Unknown_C0h_Data.Length][]; //8
                for (int i = 0; i < Unknown_C0h_Data.Length; i++) //8
                {
                    Unknown_C8h_Data[i] = reader.ReadUintsAt(ptrlist[i], Unknown_C0h_Data[i]);
                }
            }

            this.Materials = reader.ReadStructsAt<BoundMaterial_s>(this.MaterialsPointer, this.MaterialsCount);

            this.MaterialColours = reader.ReadStructsAt<BoundMaterialColour>(this.MaterialColoursPointer, this.MaterialColoursCount);

            this.PolygonMaterialIndices = reader.ReadBytesAt(this.PolygonMaterialIndicesPointer, (uint)PolygonsCount);

        }

        private void ReadPolygons(ResourceDataReader reader)
        {
            if(PolygonsCount==0)
            { return; }

            Polygons = new BoundPolygon[PolygonsCount];
            uint polybytecount = PolygonsCount * 16;
            var polygonData = reader.ReadBytesAt(PolygonsPointer, polybytecount);
            for (int i = 0; i < PolygonsCount; i++)
            {
                var offset = i * 16;
                byte b0 = polygonData[offset];
                polygonData[offset] = (byte)(b0 & 0x7F);//mask it off
                BoundPolygonType type = (BoundPolygonType)(b0 & 7);
                BoundPolygon p = null;
                switch (type)
                {
                    case BoundPolygonType.Triangle:
                        p = new BoundPolygonTriangle();
                        break;
                    case BoundPolygonType.Sphere:
                        p = new BoundPolygonSphere();
                        break;
                    case BoundPolygonType.Capsule:
                        p = new BoundPolygonCapsule();
                        break;
                    case BoundPolygonType.Box:
                        p = new BoundPolygonBox();
                        break;
                    case BoundPolygonType.Cylinder:
                        p = new BoundPolygonCylinder();
                        break;
                    default:
                        break;
                }
                if (p != null)
                {
                    p.Read(polygonData, offset);
                }
                Polygons[i] = p;
            }
        }

        public Vector3 GetVertex(int index)
        {
            return ((index < 0) || (index >= Vertices.Length)) ? Vector3.Zero : Vertices[index];
        }


        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            //this.Unknown_78h_Pointer = (ulong)(this.p1data != null ? this.p1data.Position : 0);
            //this.PolygonsPointer = (ulong)(this.Polygons != null ? this.Polygons.Position : 0);
            //this.VerticesPointer = (ulong)(this.Vertices != null ? this.Vertices.Position : 0);
            //this.Unknown_B8h_Pointer = (ulong)(this.Unknown_B8h_Data != null ? this.Unknown_B8h_Data.Position : 0);
            //this.Unknown_C0h_Pointer = (ulong)(this.Unknown_C0h_Data != null ? this.Unknown_C0h_Data.Position : 0);
            //this.Unknown_C8h_Pointer = (ulong)(this.Unknown_C8h_Data != null ? this.Unknown_C8h_Data.Position : 0);
            //this.VerticesCount = (uint)(this.Vertices != null ? this.Vertices.Count : 0);
            //this.PolygonsCount = (uint)(this.Polygons != null ? this.Polygons.Count : 0);
            //this.MaterialsPointer = (ulong)(this.Materials != null ? this.Materials.Position : 0);
            //this.Unknown_F8h_Pointer = (ulong)(this.Unknown_F8h_Data != null ? this.Unknown_F8h_Data.Position : 0);
            //this.PolygonMaterialIndicesPointer = (ulong)(this.PolygonMaterialIndices != null ? this.PolygonMaterialIndices.Position : 0);
            //this.MaterialsCount = (byte)(this.Materials != null ? this.Materials.Count : 0);
            //this.Count2 = (byte)(this.Unknown_F8h_Data != null ? this.Unknown_F8h_Data.Count : 0);
            //TODO: fix all this

            // write structure data
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.Write(this.Unknown_78h_Pointer);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Count1);
            writer.Write(this.PolygonsPointer);
            writer.Write(this.Quantum.X);// this.Unknown_90h);
            writer.Write(this.Quantum.Y);// .Unknown_94h);
            writer.Write(this.Quantum.Z);// .Unknown_98h);
            writer.Write(this.Unknown_9Ch);
            writer.Write(this.CenterGeom.X);// .Unknown_A0h);
            writer.Write(this.CenterGeom.Y);// .Unknown_A4h);
            writer.Write(this.CenterGeom.Z);// .Unknown_A8h);
            writer.Write(this.Unknown_ACh);
            writer.Write(this.VerticesPointer);
            writer.Write(this.Unknown_B8h_Pointer);
            writer.Write(this.Unknown_C0h_Pointer);
            writer.Write(this.Unknown_C8h_Pointer);
            writer.Write(this.VerticesCount);
            writer.Write(this.PolygonsCount);
            writer.Write(this.Unknown_D8h);
            writer.Write(this.Unknown_DCh);
            writer.Write(this.Unknown_E0h);
            writer.Write(this.Unknown_E4h);
            writer.Write(this.Unknown_E8h);
            writer.Write(this.Unknown_ECh);
            writer.Write(this.MaterialsPointer);
            writer.Write(this.MaterialColoursPointer);
            writer.Write(this.Unknown_100h);
            writer.Write(this.Unknown_104h);
            writer.Write(this.Unknown_108h);
            writer.Write(this.Unknown_10Ch);
            writer.Write(this.Unknown_110h);
            writer.Write(this.Unknown_114h);
            writer.Write(this.PolygonMaterialIndicesPointer);
            writer.Write(this.MaterialsCount);
            writer.Write(this.MaterialColoursCount);
            writer.Write(this.Unknown_122h);
            writer.Write(this.Unknown_124h);
            writer.Write(this.Unknown_128h);
            writer.Write(this.Unknown_12Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            //if (p1data != null) list.Add(p1data);
            //if (Polygons != null) list.Add(Polygons);
            //if (Vertices != null) list.Add(Vertices);
            //if (Unknown_B8h_Data != null) list.Add(Unknown_B8h_Data);
            //if (Unknown_C0h_Data != null) list.Add(Unknown_C0h_Data);
            //if (Unknown_C8h_Data != null) list.Add(Unknown_C8h_Data);
            //if (Materials != null) list.Add(Materials);
            //if (Unknown_F8h_Data != null) list.Add(Unknown_F8h_Data);
            //if (PolygonMaterialIndices != null) list.Add(PolygonMaterialIndices);
            //TODO: fix all these!
            return list.ToArray();
        }
    }

    [TC(typeof(EXP))] [StructLayout(LayoutKind.Sequential, Pack = 1)] public struct BoundMaterial_s
    {
        public BoundsMaterialType Type { get; set; }
        public byte ProceduralId { get; set; }
        public byte RoomId_and_PedDensity { get; set; }
        public ushort PolyFlags { get; set; } //TOBEFIXED
        public byte MaterialColorIndex { get; set; } //TOBEFIXED
        public ushort Unk4 { get; set; }
        public override string ToString()
        {
            return Type.ToString() + ", " + ProceduralId.ToString() + ", " + RoomId_and_PedDensity.ToString() + ", " + MaterialColorIndex.ToString() + ", " + PolyFlags.ToString() + ", " + Unk4.ToString();
        }
    }

    [TC(typeof(EXP))] public struct BoundMaterialColour
    {
        //public BoundsMaterialType Type { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; } //GIMS EVO saves this as "opacity" 0-100
        public override string ToString()
        {
            //return Type.ToString() + ", " + Unk0.ToString() + ", " + Unk1.ToString() + ", " + Unk2.ToString();
            return R.ToString() + ", " + G.ToString() + ", " + B.ToString() + ", " + A.ToString();
        }
    }
    [TC(typeof(EXP))] public struct BoundVertex_s
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
    }
    public enum BoundPolygonType
    {
        Triangle = 0,
        Sphere = 1,
        Capsule = 2,
        Box = 3,
        Cylinder = 4,
    }
    [TC(typeof(EXP))] public abstract class BoundPolygon
    {
        public BoundPolygonType Type { get; set; }
        public abstract void Read(byte[] bytes, int offset);

        public override string ToString()
        {
            return Type.ToString();
        }
    }
    [TC(typeof(EXP))] public class BoundPolygonTriangle : BoundPolygon
    {
        public float triArea { get; set; }
        public ushort triIndex1 { get; set; }
        public ushort triIndex2 { get; set; }
        public ushort triIndex3 { get; set; }
        public short edgeIndex1 { get; set; }
        public short edgeIndex2 { get; set; }
        public short edgeIndex3 { get; set; }

        public int vertIndex1 { get { return triIndex1 & 0x7FFF; } }
        public int vertIndex2 { get { return triIndex2 & 0x7FFF; } }
        public int vertIndex3 { get { return triIndex3 & 0x7FFF; } }
        public bool vertFlag1 { get { return (triIndex1 & 0x8000) > 0; } }
        public bool vertFlag2 { get { return (triIndex2 & 0x8000) > 0; } }
        public bool vertFlag3 { get { return (triIndex3 & 0x8000) > 0; } }


        public BoundPolygonTriangle()
        {
            Type = BoundPolygonType.Triangle;
        }
        public override void Read(byte[] bytes, int offset)
        {
            triArea = BitConverter.ToSingle(bytes, offset + 0);
            triIndex1 = BitConverter.ToUInt16(bytes, offset + 4);
            triIndex2 = BitConverter.ToUInt16(bytes, offset + 6);
            triIndex3 = BitConverter.ToUInt16(bytes, offset + 8);
            edgeIndex1 = BitConverter.ToInt16(bytes, offset + 10);
            edgeIndex2 = BitConverter.ToInt16(bytes, offset + 12);
            edgeIndex3 = BitConverter.ToInt16(bytes, offset + 14);
        }

        public override string ToString()
        {
            return base.ToString() + ": " + vertIndex1.ToString() + ", " + vertIndex2.ToString() + ", " + vertIndex3.ToString();
        }
    }
    [TC(typeof(EXP))] public class BoundPolygonSphere : BoundPolygon
    {
        public ushort sphereType { get; set; }
        public ushort sphereIndex { get; set; }
        public float sphereRadius { get; set; }
        public uint unused0 { get; set; }
        public uint unused1 { get; set; }

        public BoundPolygonSphere()
        {
            Type = BoundPolygonType.Sphere;
        }
        public override void Read(byte[] bytes, int offset)
        {
            sphereType = BitConverter.ToUInt16(bytes, offset + 0);
            sphereIndex = BitConverter.ToUInt16(bytes, offset + 2);
            sphereRadius = BitConverter.ToSingle(bytes, offset + 4);
            unused0 = BitConverter.ToUInt32(bytes, offset + 8);
            unused1 = BitConverter.ToUInt32(bytes, offset + 12);
        }

        public override string ToString()
        {
            return base.ToString() + ": " + sphereIndex.ToString() + ", " + sphereRadius.ToString();
        }
    }
    [TC(typeof(EXP))] public class BoundPolygonCapsule : BoundPolygon
    {
        public ushort capsuleType { get; set; }
        public ushort capsuleIndex1 { get; set; }
        public float capsuleRadius { get; set; }
        public ushort capsuleIndex2 { get; set; }
        public ushort unused0 { get; set; }
        public uint unused1 { get; set; }

        public BoundPolygonCapsule()
        {
            Type = BoundPolygonType.Capsule;
        }
        public override void Read(byte[] bytes, int offset)
        {
            capsuleType = BitConverter.ToUInt16(bytes, offset + 0);
            capsuleIndex1 = BitConverter.ToUInt16(bytes, offset + 2);
            capsuleRadius = BitConverter.ToSingle(bytes, offset + 4);
            capsuleIndex2 = BitConverter.ToUInt16(bytes, offset + 8);
            unused0 = BitConverter.ToUInt16(bytes, offset + 10);
            unused1 = BitConverter.ToUInt32(bytes, offset + 12);
        }

        public override string ToString()
        {
            return base.ToString() + ": " + capsuleIndex1.ToString() + ", " + capsuleIndex2.ToString() + ", " + capsuleRadius.ToString();
        }
    }
    [TC(typeof(EXP))] public class BoundPolygonBox : BoundPolygon
    {
        public uint boxType { get; set; }
        public short boxIndex1 { get; set; }
        public short boxIndex2 { get; set; }
        public short boxIndex3 { get; set; }
        public short boxIndex4 { get; set; }
        public uint unused0 { get; set; }

        public BoundPolygonBox()
        {
            Type = BoundPolygonType.Box;
        }
        public override void Read(byte[] bytes, int offset)
        {
            boxType = BitConverter.ToUInt32(bytes, offset + 0);
            boxIndex1 = BitConverter.ToInt16(bytes, offset + 4);
            boxIndex2 = BitConverter.ToInt16(bytes, offset + 6);
            boxIndex3 = BitConverter.ToInt16(bytes, offset + 8);
            boxIndex4 = BitConverter.ToInt16(bytes, offset + 10);
            unused0 = BitConverter.ToUInt32(bytes, offset + 12);
        }

        public override string ToString()
        {
            return base.ToString() + ": " + boxIndex1.ToString() + ", " + boxIndex2.ToString() + ", " + boxIndex3.ToString() + ", " + boxIndex4.ToString();
        }
    }
    [TC(typeof(EXP))] public class BoundPolygonCylinder : BoundPolygon
    {
        public ushort cylinderType { get; set; }
        public ushort cylinderIndex1 { get; set; }
        public float cylinderRadius { get; set; }
        public ushort cylinderIndex2 { get; set; }
        public ushort unused0 { get; set; }
        public uint unused1 { get; set; }

        public BoundPolygonCylinder()
        {
            Type = BoundPolygonType.Cylinder;
        }
        public override void Read(byte[] bytes, int offset)
        {
            cylinderType = BitConverter.ToUInt16(bytes, offset + 0);
            cylinderIndex1 = BitConverter.ToUInt16(bytes, offset + 2);
            cylinderRadius = BitConverter.ToSingle(bytes, offset + 4);
            cylinderIndex2 = BitConverter.ToUInt16(bytes, offset + 8);
            unused0 = BitConverter.ToUInt16(bytes, offset + 10);
            unused1 = BitConverter.ToUInt32(bytes, offset + 12);
        }

        public override string ToString()
        {
            return base.ToString() + ": " + cylinderIndex1.ToString() + ", " + cylinderIndex2.ToString() + ", " + cylinderRadius.ToString();
        }
    }
    [TC(typeof(EXP))] public class BoundBVH : BoundGeometry
    {
        public override long BlockLength
        {
            get { return 336; }
        }

        // structure data
        public ulong BvhPointer { get; set; }
        public uint Unknown_138h { get; set; } // 0x00000000
        public uint Unknown_13Ch { get; set; } // 0x00000000
        public ushort Unknown_140h { get; set; } // 0xFFFF
        public ushort Unknown_142h { get; set; } // 0x0000
        public uint Unknown_144h { get; set; } // 0x00000000
        public uint Unknown_148h { get; set; } // 0x00000000
        public uint Unknown_14Ch { get; set; } // 0x00000000

        // reference data
        public BVH BVH { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.BvhPointer = reader.ReadUInt64();
            this.Unknown_138h = reader.ReadUInt32();
            this.Unknown_13Ch = reader.ReadUInt32();
            this.Unknown_140h = reader.ReadUInt16();
            this.Unknown_142h = reader.ReadUInt16();
            this.Unknown_144h = reader.ReadUInt32();
            this.Unknown_148h = reader.ReadUInt32();
            this.Unknown_14Ch = reader.ReadUInt32();

            // read reference data
            if (this.BvhPointer > 65535)
            {
                this.BVH = reader.ReadBlockAt<BVH>(
                    this.BvhPointer // offset
                );
            }
            else
            {
                //this can happen in some ydr's for some reason
            }
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.BvhPointer = (ulong)(this.BVH != null ? this.BVH.FilePosition : 0);

            // write structure data
            writer.Write(this.BvhPointer);
            writer.Write(this.Unknown_138h);
            writer.Write(this.Unknown_13Ch);
            writer.Write(this.Unknown_140h);
            writer.Write(this.Unknown_142h);
            writer.Write(this.Unknown_144h);
            writer.Write(this.Unknown_148h);
            writer.Write(this.Unknown_14Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (BVH != null) list.Add(BVH);
            return list.ToArray();
        }
    }
    [TC(typeof(EXP))] public class BoundComposite : Bounds
    {
        public override long BlockLength
        {
            get { return 176; }
        }

        // structure data
        public ulong ChildrenPointer { get; set; }
        public ulong ChildrenTransformation1Pointer { get; set; }
        public ulong ChildrenTransformation2Pointer { get; set; }
        public ulong ChildrenBoundingBoxesPointer { get; set; }
        public ulong Unknown_90h_Pointer { get; set; }
        public ulong Unknown_98h_Pointer { get; set; }
        public ushort ChildrenCount1 { get; set; }
        public ushort ChildrenCount2 { get; set; }
        public uint Unknown_A4h { get; set; } // 0x00000000
        public ulong BVHPointer { get; set; }

        // reference data
        public ResourcePointerArray64<Bounds> Children { get; set; }
        //public ResourceSimpleArray<Matrix4_r> ChildrenTransformation1 { get; set; }
        //public ResourceSimpleArray<Matrix4_r> ChildrenTransformation2 { get; set; }
        //public ResourceSimpleArray<AABB_r> ChildrenBoundingBoxes { get; set; }
        //public ResourceSimpleArray<BoundComposite_Unknown_B_002> Unknown_90h_Data { get; set; }
        //public ResourceSimpleArray<BoundComposite_Unknown_B_002> Unknown_98h_Data { get; set; }
        public SharpDX.Matrix[] ChildrenTransformation1 { get; set; }
        public SharpDX.Matrix[] ChildrenTransformation2 { get; set; }
        public AABB_s[] ChildrenBoundingBoxes { get; set; }
        public BoundComposite_Unknown_B_002_s[] Unknown_90h_Data { get; set; }
        public BoundComposite_Unknown_B_002_s[] Unknown_98h_Data { get; set; }

        public BVH BVH { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.ChildrenPointer = reader.ReadUInt64();
            this.ChildrenTransformation1Pointer = reader.ReadUInt64();
            this.ChildrenTransformation2Pointer = reader.ReadUInt64();
            this.ChildrenBoundingBoxesPointer = reader.ReadUInt64();
            this.Unknown_90h_Pointer = reader.ReadUInt64();
            this.Unknown_98h_Pointer = reader.ReadUInt64();
            this.ChildrenCount1 = reader.ReadUInt16();
            this.ChildrenCount2 = reader.ReadUInt16();
            this.Unknown_A4h = reader.ReadUInt32();
            this.BVHPointer = reader.ReadUInt64();

            // read reference data
            this.Children = reader.ReadBlockAt<ResourcePointerArray64<Bounds>>(
                this.ChildrenPointer, // offset
                this.ChildrenCount1
            );

            if ((Children != null) && (Children.data_items != null))
            {
                foreach (var child in Children.data_items)
                {
                    if (child != null) child.Parent = this;
                }
            }

            //this.ChildrenTransformation1 = reader.ReadBlockAt<ResourceSimpleArray<Matrix4_r>>(
            //    this.ChildrenTransformation1Pointer, // offset
            //    this.ChildrenCount1
            //);
            //this.ChildrenTransformation2 = reader.ReadBlockAt<ResourceSimpleArray<Matrix4_r>>(
            //    this.ChildrenTransformation2Pointer, // offset
            //    this.ChildrenCount1
            //);
            //this.ChildrenBoundingBoxes = reader.ReadBlockAt<ResourceSimpleArray<AABB_r>>(
            //    this.ChildrenBoundingBoxesPointer, // offset
            //    this.ChildrenCount1
            //);
            //this.Unknown_90h_Data = reader.ReadBlockAt<ResourceSimpleArray<BoundComposite_Unknown_B_002>>(
            //    this.Unknown_90h_Pointer, // offset
            //    this.ChildrenCount1
            //);
            //this.Unknown_98h_Data = reader.ReadBlockAt<ResourceSimpleArray<BoundComposite_Unknown_B_002>>(
            //    this.Unknown_98h_Pointer, // offset
            //    this.ChildrenCount1
            //);


            this.ChildrenTransformation1 = reader.ReadStructsAt<SharpDX.Matrix>(this.ChildrenTransformation1Pointer, this.ChildrenCount1);
            this.ChildrenTransformation2 = reader.ReadStructsAt<SharpDX.Matrix>(this.ChildrenTransformation2Pointer, this.ChildrenCount1);
            this.ChildrenBoundingBoxes = reader.ReadStructsAt<AABB_s>(this.ChildrenBoundingBoxesPointer, this.ChildrenCount1);
            this.Unknown_90h_Data = reader.ReadStructsAt<BoundComposite_Unknown_B_002_s>(this.Unknown_90h_Pointer, this.ChildrenCount1);
            this.Unknown_98h_Data = reader.ReadStructsAt<BoundComposite_Unknown_B_002_s>(this.Unknown_98h_Pointer, this.ChildrenCount1);


            this.BVH = reader.ReadBlockAt<BVH>(
                this.BVHPointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.ChildrenPointer = (ulong)(this.Children != null ? this.Children.FilePosition : 0);
            //this.ChildrenTransformation1Pointer = (ulong)(this.ChildrenTransformation1 != null ? this.ChildrenTransformation1.Position : 0);
            //this.ChildrenTransformation2Pointer = (ulong)(this.ChildrenTransformation2 != null ? this.ChildrenTransformation2.Position : 0);
            //this.ChildrenBoundingBoxesPointer = (ulong)(this.ChildrenBoundingBoxes != null ? this.ChildrenBoundingBoxes.Position : 0);
            //this.Unknown_90h_Pointer = (ulong)(this.Unknown_90h_Data != null ? this.Unknown_90h_Data.Position : 0);
            //this.Unknown_98h_Pointer = (ulong)(this.Unknown_98h_Data != null ? this.Unknown_98h_Data.Position : 0);
            //TODO: fix
            this.ChildrenCount1 = (ushort)(this.Children != null ? this.Children.Count : 0);
            this.ChildrenCount2 = (ushort)(this.Children != null ? this.Children.Count : 0);
            this.BVHPointer = (ulong)(this.BVH != null ? this.BVH.FilePosition : 0);

            // write structure data
            writer.Write(this.ChildrenPointer);
            writer.Write(this.ChildrenTransformation1Pointer);
            writer.Write(this.ChildrenTransformation2Pointer);
            writer.Write(this.ChildrenBoundingBoxesPointer);
            writer.Write(this.Unknown_90h_Pointer);
            writer.Write(this.Unknown_98h_Pointer);
            writer.Write(this.ChildrenCount1);
            writer.Write(this.ChildrenCount2);
            writer.Write(this.Unknown_A4h);
            writer.Write(this.BVHPointer);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Children != null) list.Add(Children);
            //if (ChildrenTransformation1 != null) list.Add(ChildrenTransformation1); //TODO: fix
            //if (ChildrenTransformation2 != null) list.Add(ChildrenTransformation2);
            //if (ChildrenBoundingBoxes != null) list.Add(ChildrenBoundingBoxes);
            //if (Unknown_90h_Data != null) list.Add(Unknown_90h_Data);
            //if (Unknown_98h_Data != null) list.Add(Unknown_98h_Data);
            if (BVH != null) list.Add(BVH);
            return list.ToArray();
        }
    }
    [TC(typeof(EXP))] public struct BoundComposite_Unknown_B_002_s
    {
        public uint Unknown_0h { get; set; }
        public uint Unknown_4h { get; set; }
        public override string ToString()
        {
            return Unknown_0h.ToString() + ", " + Unknown_4h.ToString();
        }
    }

    [TC(typeof(EXP))] public class BoundDisc : Bounds
    {
        public override long BlockLength
        {
            get { return 128; }
        }

        // structure data
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public uint Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.Unknown_78h = reader.ReadUInt32();
            this.Unknown_7Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_7Ch);
        }
    }
    [TC(typeof(EXP))] public class BoundCylinder : Bounds
    {
        public override long BlockLength
        {
            get { return 128; }
        }

        // structure data
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public uint Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.Unknown_78h = reader.ReadUInt32();
            this.Unknown_7Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_7Ch);
        }
    }

    [TC(typeof(EXP))] public class BVH : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 128; }
        }

        // structure data
        public ulong NodesPointer { get; set; }
        public uint NodesCount { get; set; }
        public uint Count2 { get; set; }
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public SharpDX.Vector4 BoundingBoxMin { get; set; }
        public SharpDX.Vector4 BoundingBoxMax { get; set; }
        public SharpDX.Vector4 BoundingBoxCenter { get; set; }
        public SharpDX.Vector4 QuantumInverse { get; set; }
        public SharpDX.Vector4 Quantum { get; set; } // bounding box dimension / 2^16
        //public ResourceSimpleList64<BVHTreeInfo> Trees { get; set; }
        public ResourceSimpleList64Ptr TreesPtr { get; set; }
        public BVHTreeInfo_s[] Trees { get; set; }

        // reference data
        //public ResourceSimpleArray2<BVHNode, BVHNode_Unknown_B_003> Nodes;
        public BVHNode_s[] Nodes { get; set; }
        public BVHNode_s[] Nodes_Unk1 { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.NodesPointer = reader.ReadUInt64();
            this.NodesCount = reader.ReadUInt32();
            this.Count2 = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            //this.BoundingBoxMin = reader.ReadBlock<Vector4_r>();
            //this.BoundingBoxMax = reader.ReadBlock<Vector4_r>();
            //this.BoundingBoxCenter = reader.ReadBlock<Vector4_r>();
            //this.QuantumInverse = reader.ReadBlock<Vector4_r>();
            //this.Quantum = reader.ReadBlock<Vector4_r>();
            this.BoundingBoxMin = reader.ReadStruct<SharpDX.Vector4>();
            this.BoundingBoxMax = reader.ReadStruct<SharpDX.Vector4>();
            this.BoundingBoxCenter = reader.ReadStruct<SharpDX.Vector4>();
            this.QuantumInverse = reader.ReadStruct<SharpDX.Vector4>();
            this.Quantum = reader.ReadStruct<SharpDX.Vector4>();

            //this.Trees = reader.ReadBlock<ResourceSimpleList64<BVHTreeInfo>>();
            this.TreesPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Trees = reader.ReadStructsAt<BVHTreeInfo_s>(this.TreesPtr.EntriesPointer, this.TreesPtr.EntriesCount);

            // read reference data
            //this.Nodes = reader.ReadBlockAt<ResourceSimpleArray2<BVHNode, BVHNode_Unknown_B_003>>(
            //    this.NodesPointer, // offset
            //    this.NodesCount,
            //    this.Count2 - this.NodesCount
            //);

            this.Nodes = reader.ReadStructsAt<BVHNode_s>(this.NodesPointer, this.NodesCount);

            this.Nodes_Unk1 = reader.ReadStructsAt<BVHNode_s>(this.NodesPointer + NodesCount * 16 /*sizeof(BVHNode_s)*/, Count2 - NodesCount);


        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            //this.NodesPointer = (ulong)(this.Nodes != null ? this.Nodes.Position : 0);
            //this.NodesCount = (uint)(this.Nodes != null ? this.Nodes.Array1.Count : 0);
            //this.Count2 = (uint)(this.Nodes != null ? this.Nodes.Array1.Count + this.Nodes.Array2.Count : 0);
            //TODO: fix

            // write structure data
            writer.Write(this.NodesPointer);
            writer.Write(this.NodesCount);
            writer.Write(this.Count2);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            //writer.WriteBlock(this.BoundingBoxMin);
            //writer.WriteBlock(this.BoundingBoxMax);
            //writer.WriteBlock(this.BoundingBoxCenter);
            //writer.WriteBlock(this.QuantumInverse);
            //writer.WriteBlock(this.Quantum);
            //writer.WriteBlock(this.Trees); //TODO: fix
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            //if (Nodes != null) list.Add(Nodes); //TODO: fix!
            //if (Trees != null) list.Add(Trees);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                //new Tuple<long, IResourceBlock>(0x20, BoundingBoxMin),
                //new Tuple<long, IResourceBlock>(0x30, BoundingBoxMax),
                //new Tuple<long, IResourceBlock>(0x40, BoundingBoxCenter),
                //new Tuple<long, IResourceBlock>(0x50, QuantumInverse),
                //new Tuple<long, IResourceBlock>(0x60, Quantum),
                //new Tuple<long, IResourceBlock>(0x70, Trees) //TODO: fix!
            };
        }
    }
    [TC(typeof(EXP))] public struct BVHTreeInfo_s
    {
        public short MinX { get; set; }
        public short MinY { get; set; }
        public short MinZ { get; set; }
        public short MaxX { get; set; }
        public short MaxY { get; set; }
        public short MaxZ { get; set; }
        public short NodeIndex1 { get; set; }
        public short NodeIndex2 { get; set; }

        public override string ToString()
        {
            return NodeIndex1.ToString() + ", " + NodeIndex2.ToString();
        }
    }
    [TC(typeof(EXP))] public struct BVHNode_s
    {
        public short MinX { get; set; }
        public short MinY { get; set; }
        public short MinZ { get; set; }
        public short MaxX { get; set; }
        public short MaxY { get; set; }
        public short MaxZ { get; set; }
        public short PolyId { get; set; }
        public short PolyCount { get; set; }

        public override string ToString()
        {
            return PolyId.ToString() + ": " + PolyCount.ToString();
        }
    }



    [TC(typeof(EXP))] public struct BoundsMaterialType
    {
        public byte Index { get; set; }

        public BoundsMaterialData MaterialData
        {
            get
            {
                return BoundsMaterialTypes.GetMaterial(this);
            }
        }

        public override string ToString()
        {
            return BoundsMaterialTypes.GetMaterialName(this);
        }
    }

    [TC(typeof(EXP))] public class BoundsMaterialData
    {
        public string Name { get; set; }
        public string Filter { get; set; }
        public string FXGroup { get; set; }
        public string VFXDisturbanceType { get; set; }
        public string RumbleProfile { get; set; }
        public string ReactWeaponType { get; set; }
        public string Friction { get; set; }
        public string Elasticity { get; set; }
        public string Density { get; set; }
        public string TyreGrip { get; set; }
        public string WetGrip { get; set; }
        public string TyreDrag { get; set; }
        public string TopSpeedMult { get; set; }
        public string Softness { get; set; }
        public string Noisiness { get; set; }
        public string PenetrationResistance { get; set; }
        public string SeeThru { get; set; }
        public string ShootThru { get; set; }
        public string ShootThruFX { get; set; }
        public string NoDecal { get; set; }
        public string Porous { get; set; }
        public string HeatsTyre { get; set; }
        public string Material { get; set; }

        public Color4 Colour { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class BoundsMaterialTypes
    {
        private static Dictionary<string, Color4> ColourDict;
        private static List<BoundsMaterialData> Materials;

        public static void Init(GameFileCache gameFileCache)
        {
            var rpfman = gameFileCache.RpfMan;

            InitColours();

            var list = new List<BoundsMaterialData>();
            string filename = "common.rpf\\data\\materials\\materials.dat";
            if (gameFileCache.EnableDlc)
            {
                filename = "update\\update.rpf\\common\\data\\materials\\materials.dat";
            }
            string txt = rpfman.GetFileUTF8Text(filename);
            AddMaterialsDat(txt, list);

            Materials = list;
        }

        private static void InitColours()
        {
            var dict = new Dictionary<string, Color4>();
            string txt = File.ReadAllText("Materials.txt");
            string[] lines = txt.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length < 10) continue;
                if (line[0] == '#') continue;
                string[] parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) continue;
                string name = parts[0].Trim();
                string cstr = parts[1].Trim();
                uint cval = Convert.ToUInt32(cstr, 16);
                Color4 c = new Color4(cval);
                dict[name] = c;
            }
            ColourDict = dict;
        }

        private static void AddMaterialsDat(string txt, List<BoundsMaterialData> list)
        {
            list.Clear();
            if (txt == null) return;
            string[] lines = txt.Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length < 20) continue;
                if (line[0] == '#') continue;
                string[] parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 10) continue;
                int cp = 0;
                BoundsMaterialData d = new BoundsMaterialData();
                for (int p = 0; p < parts.Length; p++)
                {
                    string part = parts[p].Trim();
                    if (string.IsNullOrWhiteSpace(part)) continue;
                    switch (cp)
                    {
                        case 0: d.Name = part; break;
                        case 1: d.Filter = part; break;
                        case 2: d.FXGroup = part; break;
                        case 3: d.VFXDisturbanceType = part; break;
                        case 4: d.RumbleProfile = part; break;
                        case 5: d.ReactWeaponType = part; break;
                        case 6: d.Friction = part; break;
                        case 7: d.Elasticity = part; break;
                        case 8: d.Density = part; break;
                        case 9: d.TyreGrip = part; break;
                        case 10: d.WetGrip = part; break;
                        case 11: d.TyreDrag = part; break;
                        case 12: d.TopSpeedMult = part; break;
                        case 13: d.Softness = part; break;
                        case 14: d.Noisiness = part; break;
                        case 15: d.PenetrationResistance = part; break;
                        case 16: d.SeeThru = part; break;
                        case 17: d.ShootThru = part; break;
                        case 18: d.ShootThruFX = part; break;
                        case 19: d.NoDecal = part; break;
                        case 20: d.Porous = part; break;
                        case 21: d.HeatsTyre = part; break;
                        case 22: d.Material = part; break;
                    }
                    cp++;
                }
                if (cp != 23)
                { }

                Color4 c;
                if ((ColourDict != null) && (ColourDict.TryGetValue(d.Name, out c)))
                {
                    d.Colour = c;
                }
                else
                {
                    d.Colour = new Color4(0xFFCCCCCC);
                }


                list.Add(d);
            }


            //StringBuilder sb = new StringBuilder();
            //foreach (var d in list)
            //{
            //    sb.AppendLine(d.Name);
            //}
            //string names = sb.ToString();

        }


        public static BoundsMaterialData GetMaterial(BoundsMaterialType type)
        {
            if (Materials == null) return null;
            if (type.Index >= Materials.Count) return null;
            return Materials[type.Index];
        }

        public static string GetMaterialName(BoundsMaterialType type)
        {
            var m = GetMaterial(type);
            if (m == null) return string.Empty;
            return m.Name;
        }

        public static Color4 GetMaterialColour(BoundsMaterialType type)
        {
            var m = GetMaterial(type);
            if (m == null) return new Color4(0xFFCCCCCC);
            return m.Colour;
        }
    }


}
