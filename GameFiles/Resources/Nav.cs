/*
    Copyright(c) 2016 Neodymium

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

//now with enhanced uglification for codewalker


using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{


    [TypeConverter(typeof(ExpandableObjectConverter))] public class NavMesh : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 368; }
        }


        public NavMeshFlags ContentFlags { get; set; }
        public uint VersionUnk1 { get; set; } // 0x00010011
        public uint Unused_018h { get; set; } // 0x00000000
        public uint Unused_01Ch { get; set; } // 0x00000000
        public Matrix Transform { get; set; } //(1,0,0,NaN),(0,1,0,NaN),(0,0,1,NaN),(0,0,0,NaN)
        public Vector3 AABBSize { get; set; }
        public float AABBUnk { get; set; } // 0x7F800001 //NaN
        public ulong VerticesPointer { get; set; }
        public uint Unused_078h { get; set; } // 0x00000000
        public uint Unused_07Ch { get; set; } // 0x00000000
        public ulong IndicesPointer { get; set; }
        public ulong AdjPolysPointer { get; set; }
        public uint AdjPolysIndicesCount { get; set; }
        public NavMeshUintArray AdjAreaIDs { get; set; }
        public ulong PolysPointer { get; set; }
        public ulong SectorTreePointer { get; set; }
        public ulong PortalsPointer { get; set; }
        public ulong PortalLinksPointer { get; set; }
        public uint VerticesCount { get; set; }
        public uint PolysCount { get; set; }
        public uint AreaID { get; set; } // X + Y*100
        public uint TotalBytes { get; set; }
        public uint SectorUnkCount { get; set; }
        public uint PortalsCount { get; set; }
        public uint PortalLinksCount { get; set; }
        public uint Unused_154h { get; set; } // 0x00000000
        public uint Unused_158h { get; set; } // 0x00000000
        public uint Unused_15Ch { get; set; } // 0x00000000
        public uint VersionUnk2 { get; set; }                //2244687201 (0x85CB3561) for grid ynv's
        public uint Unused_164h { get; set; } // 0x00000000
        public uint Unused_168h { get; set; } // 0x00000000
        public uint Unused_16Ch { get; set; } // 0x00000000


        public NavMeshList<NavMeshVertex> Vertices { get; set; }
        public NavMeshList<ushort> Indices { get; set; }
        public NavMeshList<NavMeshAdjPoly> AdjPolys { get; set; }
        public NavMeshList<NavMeshPoly> Polys { get; set; }
        public NavMeshSector SectorTree { get; set; }
        public NavMeshPortal[] Portals { get; set; }
        public ushort[] PortalLinks { get; set; }




        private ResourceSystemStructBlock<NavMeshPortal> PortalsBlock = null;
        private ResourceSystemStructBlock<ushort> PortalLinksBlock = null;




        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            ContentFlags = (NavMeshFlags)reader.ReadUInt32();
            VersionUnk1 = reader.ReadUInt32();
            Unused_018h = reader.ReadUInt32();
            Unused_01Ch = reader.ReadUInt32();
            Transform = reader.ReadMatrix();
            AABBSize = reader.ReadVector3();
            AABBUnk = reader.ReadSingle();
            VerticesPointer = reader.ReadUInt64();
            Unused_078h = reader.ReadUInt32();
            Unused_07Ch = reader.ReadUInt32();
            IndicesPointer = reader.ReadUInt64();
            AdjPolysPointer = reader.ReadUInt64();
            AdjPolysIndicesCount = reader.ReadUInt32();
            AdjAreaIDs = reader.ReadStruct<NavMeshUintArray>();
            PolysPointer = reader.ReadUInt64();
            SectorTreePointer = reader.ReadUInt64();
            PortalsPointer = reader.ReadUInt64();
            PortalLinksPointer = reader.ReadUInt64();
            VerticesCount = reader.ReadUInt32();
            PolysCount = reader.ReadUInt32();
            AreaID = reader.ReadUInt32();
            TotalBytes = reader.ReadUInt32();
            SectorUnkCount = reader.ReadUInt32();
            PortalsCount = reader.ReadUInt32();
            PortalLinksCount = reader.ReadUInt32();
            Unused_154h = reader.ReadUInt32();
            Unused_158h = reader.ReadUInt32();
            Unused_15Ch = reader.ReadUInt32();
            VersionUnk2 = reader.ReadUInt32();
            Unused_164h = reader.ReadUInt32();
            Unused_168h = reader.ReadUInt32();
            Unused_16Ch = reader.ReadUInt32();



            Vertices = reader.ReadBlockAt<NavMeshList<NavMeshVertex>>(VerticesPointer);
            Indices = reader.ReadBlockAt<NavMeshList<ushort>>(IndicesPointer);
            AdjPolys = reader.ReadBlockAt<NavMeshList<NavMeshAdjPoly>>(AdjPolysPointer);
            Polys = reader.ReadBlockAt<NavMeshList<NavMeshPoly>>(PolysPointer);
            SectorTree = reader.ReadBlockAt<NavMeshSector>(SectorTreePointer);
            Portals = reader.ReadStructsAt<NavMeshPortal>(PortalsPointer, PortalsCount);
            PortalLinks = reader.ReadUshortsAt(PortalLinksPointer, PortalLinksCount);


        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            VerticesPointer = (ulong)(Vertices != null ? Vertices.FilePosition : 0);
            IndicesPointer = (ulong)(Indices != null ? Indices.FilePosition : 0);
            AdjPolysPointer = (ulong)(AdjPolys != null ? AdjPolys.FilePosition : 0);
            PolysPointer = (ulong)(Polys != null ? Polys.FilePosition : 0);
            SectorTreePointer = (ulong)(SectorTree != null ? SectorTree.FilePosition : 0);
            PortalsPointer = (ulong)(PortalsBlock?.FilePosition ?? 0);
            PortalLinksPointer = (ulong)(PortalLinksBlock?.FilePosition ?? 0);



            writer.Write((uint)ContentFlags);
            writer.Write(VersionUnk1);
            writer.Write(Unused_018h);
            writer.Write(Unused_01Ch);
            writer.Write(Transform);
            writer.Write(AABBSize);
            writer.Write(AABBUnk);
            writer.Write(VerticesPointer);
            writer.Write(Unused_078h);
            writer.Write(Unused_07Ch);
            writer.Write(IndicesPointer);
            writer.Write(AdjPolysPointer);
            writer.Write(AdjPolysIndicesCount);
            writer.WriteStruct(AdjAreaIDs);
            writer.Write(PolysPointer);
            writer.Write(SectorTreePointer);
            writer.Write(PortalsPointer);
            writer.Write(PortalLinksPointer);
            writer.Write(VerticesCount);
            writer.Write(PolysCount);
            writer.Write(AreaID);
            writer.Write(TotalBytes);
            writer.Write(SectorUnkCount);
            writer.Write(PortalsCount);
            writer.Write(PortalLinksCount);
            writer.Write(Unused_154h);
            writer.Write(Unused_158h);
            writer.Write(Unused_15Ch);
            writer.Write(VersionUnk2);
            writer.Write(Unused_164h);
            writer.Write(Unused_168h);
            writer.Write(Unused_16Ch);
        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Vertices != null) list.Add(Vertices);
            if (Indices != null) list.Add(Indices);
            if (AdjPolys != null) list.Add(AdjPolys);
            if (Polys != null) list.Add(Polys);
            if (SectorTree != null) list.Add(SectorTree);

            if ((Portals != null) && (Portals.Length > 0))
            {
                PortalsBlock = new ResourceSystemStructBlock<NavMeshPortal>(Portals);
                list.Add(PortalsBlock);
            }

            if ((PortalLinks != null) && (PortalLinks.Length > 0))
            {
                PortalLinksBlock = new ResourceSystemStructBlock<ushort>(PortalLinks);
                list.Add(PortalLinksBlock);
            }




            return list.ToArray();
        }


        public override string ToString()
        {
            return "(Size: " + FloatUtil.GetVector3String(AABBSize) + ")";
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshUintArray
    {
        public uint Count { get; set; }
        public uint v00;
        public uint v01;
        public uint v02;
        public uint v03;
        public uint v04;
        public uint v05;
        public uint v06; // 0x00000000
        public uint v07; // 0x00000000
        public uint v08; // 0x00000000
        public uint v09; // 0x00000000
        public uint v10; // 0x00000000
        public uint v11; // 0x00000000
        public uint v12; // 0x00000000
        public uint v13; // 0x00000000
        public uint v14; // 0x00000000
        public uint v15; // 0x00000000
        public uint v16; // 0x00000000
        public uint v17; // 0x00000000
        public uint v18; // 0x00000000
        public uint v19; // 0x00000000
        public uint v20; // 0x00000000
        public uint v21; // 0x00000000
        public uint v22; // 0x00000000
        public uint v23; // 0x00000000
        public uint v24; // 0x00000000
        public uint v25; // 0x00000000
        public uint v26; // 0x00000000
        public uint v27; // 0x00000000
        public uint v28; // 0x00000000
        public uint v29; // 0x00000000
        public uint v30; // 0x00000000
        public uint v31; // 0x00000000

        public uint[] RawValues
        {
            get
            {
                return new[]{ v00,v01,v02,v03,v04,v05,v06,v07,v08,v09,
                              v10,v11,v12,v13,v14,v15,v16,v17,v18,v19,
                              v20,v21,v22,v23,v24,v25,v26,v27,v28,v29,
                              v30,v31 };
            }
        }

        public uint[] Values
        {
            get
            {
                uint[] vals = new uint[Count];
                uint[] rvals = RawValues;
                for (int i = 0; i < Count; i++)
                {
                    vals[i] = rvals[i];
                }
                return vals;
            }
        }


        public override string ToString()
        {
            return "(Count: " + Count.ToString() + ")";
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class NavMeshList<T> : ResourceSystemBlock where T : struct
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint ItemCount { get; set; }
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public ulong ListPartsPointer { get; set; }
        public ulong ListOffsetsPointer { get; set; }
        public uint ListPartsCount { get; set; }
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        public ResourceSimpleArray<NavMeshListPart<T>> ListParts { get; set; }
        public uint[] ListOffsets { get; set; }

        private ResourceSystemStructBlock<uint> ListOffsetsBlock = null;


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            VFT = reader.ReadUInt32();
            Unknown_04h = reader.ReadUInt32();
            ItemCount = reader.ReadUInt32();
            Unknown_0Ch = reader.ReadUInt32();
            ListPartsPointer = reader.ReadUInt64();
            ListOffsetsPointer = reader.ReadUInt64();
            ListPartsCount = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();

            ListParts = reader.ReadBlockAt<ResourceSimpleArray<NavMeshListPart<T>>>(ListPartsPointer, ListPartsCount);
            ListOffsets = reader.ReadUintsAt(ListOffsetsPointer, ListPartsCount);

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            ListPartsPointer = (ulong)(ListParts != null ? ListParts.FilePosition : 0);
            ListOffsetsPointer = (ulong)(ListOffsetsBlock?.FilePosition ?? 0);
            ListPartsCount = (uint)(ListParts != null ? ListParts.Count : 0);

            writer.Write(VFT);
            writer.Write(Unknown_04h);
            writer.Write(ItemCount);
            writer.Write(Unknown_0Ch);
            writer.Write(ListPartsPointer);
            writer.Write(ListOffsetsPointer);
            writer.Write(ListPartsCount);
            writer.Write(Unknown_24h);
            writer.Write(Unknown_28h);
            writer.Write(Unknown_2Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (ListParts != null) list.Add(ListParts);

            if ((ListOffsets != null) && (ListOffsets.Length > 0))
            {
                ListOffsetsBlock = new ResourceSystemStructBlock<uint>(ListOffsets);
                list.Add(ListOffsetsBlock);
            }

            return list.ToArray();
        }



        public List<T> GetFullList()
        {
            List<T> list = new List<T>((int)ItemCount);

            if (ListParts != null)
            {
                foreach (var part in ListParts)
                {
                    if (part.Items != null)
                    {
                        list.AddRange(part.Items);
                    }
                }
            }

            return list;
        }



        public override string ToString()
        {
            return "(" + ItemCount.ToString() + " total items, " + ListPartsCount.ToString() + " parts)";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class NavMeshListPart<T> : ResourceSystemBlock where T : struct
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        public ulong Pointer { get; set; }
        public uint Count { get; set; }
        public uint Unknown_0Ch { get; set; } // 0x00000000

        public T[] Items { get; set; }

        private ResourceSystemStructBlock<T> ItemsBlock = null;


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            Pointer = reader.ReadUInt64();
            Count = reader.ReadUInt32();
            Unknown_0Ch = reader.ReadUInt32();

            Items = reader.ReadStructsAt<T>(Pointer, Count);

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            Pointer = (ulong)(ItemsBlock?.FilePosition ?? 0);
            Count = (uint)(Items?.Length ?? 0);

            writer.Write(Pointer);
            writer.Write(Count);
            writer.Write(Unknown_0Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if ((Items != null) && (Items.Length > 0))
            {
                ItemsBlock = new ResourceSystemStructBlock<T>(Items);
                list.Add(ItemsBlock);
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            return "(" + Count.ToString() + " items)";
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshVertex
    {
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public ushort Z { get; set; }


        public Vector3 Position { get { return ToVector3(); } set { FromVector3(value); } }

        public Vector3 ToVector3()
        {
            const float usmax = ushort.MaxValue;
            return new Vector3(X / usmax, Y / usmax, Z / usmax);
        }
        public void FromVector3(Vector3 v)
        {
            const float usmax = ushort.MaxValue;
            X = (ushort)(v.X * usmax);
            Y = (ushort)(v.Y * usmax);
            Z = (ushort)(v.Z * usmax);
        }

        public override string ToString()
        {
            return X.ToString() + ", " + Y.ToString() + ", " + Z.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshAABB
    {
        public short MinX { get; set; }
        public short MaxX { get; set; }
        public short MinY { get; set; }
        public short MaxY { get; set; }
        public short MinZ { get; set; }
        public short MaxZ { get; set; }

        public Vector3 Min { get { return new Vector3(MinX / 4.0f, MinY / 4.0f, MinZ / 4.0f); } }
        public Vector3 Max { get { return new Vector3(MaxX / 4.0f, MaxY / 4.0f, MaxZ / 4.0f); } }

        public override string ToString()
        {
            Vector3 min = Min;
            Vector3 max = Max;
            return string.Format("({0}, {1}, {2}) | ({3}, {4}, {5})", min.X, min.Y, min.Z, max.X, max.Y, max.Z);
            //return string.Format("({0}, {1}, {2}) | ({3}, {4}, {5})", MinX, MinY, MinZ, MaxX, MaxY, MaxZ);
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshAdjPoly
    {
        public NavMeshAdjPolyPart Unknown_0h { get; set; }
        public NavMeshAdjPolyPart Unknown_4h { get; set; }

        public override string ToString()
        {
            return Unknown_0h.Bin + " | " + Unknown_4h.Bin + " | " + 
                   Unknown_0h.ToString() + " | " + Unknown_4h.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshAdjPolyPart
    {
        public uint Value { get; set; }

        public string Bin
        {
            get
            {
                return Convert.ToString(Value, 2).PadLeft(32, '0');
            }
        }

        public uint AdjAreaIDInd { get { return (Value >> 0) & 0x1F; } }
        public uint PolyID { get { return (Value >> 5) & 0x3FFF; } }
        public uint Unk2 { get { return (Value >> 19) & 0x3; } }
        public uint Unk3 { get { return (Value >> 21); } }

        public override string ToString()
        {
            return AdjAreaIDInd.ToString() + ", " + PolyID.ToString() + ", " + Unk2.ToString() + ", " + Unk3.ToString();
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshPoly
    {
        public ushort Unknown_00h { get; set; }
        public ushort IndexFlags { get; set; }
        public ushort IndexID { get; set; }
        public ushort AreaID { get; set; }
        public uint Unused_08h { get; set; } // 0x00000000
        public uint Unused_0Ch { get; set; } // 0x00000000
        public uint Unused_10h { get; set; } // 0x00000000
        public uint Unused_14h { get; set; } // 0x00000000
        public NavMeshAABB CellAABB { get; set; }
        public FlagsUint Unknown_24h { get; set; }
        public FlagsUint Unknown_28h { get; set; }
        public ushort PartFlags { get; set; }
        public ushort PortalID { get; set; }


        //public int IndexUnk { get { return (IndexFlags >> 0) & 31; } } //always 0
        public int IndexCount { get { return (IndexFlags >> 5); } }

        //public int PartUnk1 { get { return (PartFlags >> 0) & 0xF; } } //always 0
        public int PartID { get { return (PartFlags >> 4) & 0xFF; } }
        public int PartUnk2 { get { return (PartFlags >> 12) & 0xF; } }


        public uint Unknown_28h_16 { get { return ((Unknown_28h.Value & 65535)); } }
        public uint Unknown_28h_8a { get { return ((Unknown_28h.Value >> 0) & 255); } }
        public uint Unknown_28h_8b { get { return ((Unknown_28h.Value >> 8) & 255); } }


        public override string ToString()
        {
            return
                //Unknown_28h.Bin + ", (" + Unknown_28h_8a.ToString() + ", " + Unknown_28h_8b.ToString() + "), " +
                Unknown_00h.ToString() + ", " +
                //IndexFlags.ToString() + ", " + 
                IndexCount.ToString() + ", " + //IndexUnk.ToString() + ", " +
                IndexID.ToString() + ", " + AreaID.ToString() + ", " +
                CellAABB.ToString() + ", " +
                Unknown_24h.Hex + ", " + 
                Unknown_28h.Hex + ", " + 
                //PartFlags.ToString() + ", " + //PartUnk1.ToString() + ", " + 
                PartID.ToString() + ", " + 
                PartUnk2.ToString() + ", " +
                PortalID.ToString();
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class NavMeshSector : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 96; }
        }

        public Vector4 AABBMin { get; set; } //W==NaN
        public Vector4 AABBMax { get; set; } //W==NaN
        public NavMeshAABB CellAABB { get; set; }
        public ulong DataPointer { get; set; }
        public ulong SubTree1Pointer { get; set; }
        public ulong SubTree2Pointer { get; set; }
        public ulong SubTree3Pointer { get; set; }
        public ulong SubTree4Pointer { get; set; }
        public uint Unused_54h { get; set; } // 0x00000000
        public uint Unused_58h { get; set; } // 0x00000000
        public uint Unused_5Ch { get; set; } // 0x00000000

        public NavMeshSectorData Data { get; set; }
        public NavMeshSector SubTree1 { get; set; }
        public NavMeshSector SubTree2 { get; set; }
        public NavMeshSector SubTree3 { get; set; }
        public NavMeshSector SubTree4 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            AABBMin = reader.ReadVector4();
            AABBMax = reader.ReadVector4();
            CellAABB = reader.ReadStruct<NavMeshAABB>();
            DataPointer = reader.ReadUInt64();
            SubTree1Pointer = reader.ReadUInt64();
            SubTree2Pointer = reader.ReadUInt64();
            SubTree3Pointer = reader.ReadUInt64();
            SubTree4Pointer = reader.ReadUInt64();
            Unused_54h = reader.ReadUInt32();
            Unused_58h = reader.ReadUInt32();
            Unused_5Ch = reader.ReadUInt32();

            Data = reader.ReadBlockAt<NavMeshSectorData>(DataPointer);
            SubTree1 = reader.ReadBlockAt<NavMeshSector>(SubTree1Pointer);
            SubTree2 = reader.ReadBlockAt<NavMeshSector>(SubTree2Pointer);
            SubTree3 = reader.ReadBlockAt<NavMeshSector>(SubTree3Pointer);
            SubTree4 = reader.ReadBlockAt<NavMeshSector>(SubTree4Pointer);
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            DataPointer = (ulong)(Data != null ? Data.FilePosition : 0);
            SubTree1Pointer = (ulong)(SubTree1 != null ? SubTree1.FilePosition : 0);
            SubTree2Pointer = (ulong)(SubTree2 != null ? SubTree2.FilePosition : 0);
            SubTree3Pointer = (ulong)(SubTree3 != null ? SubTree3.FilePosition : 0);
            SubTree4Pointer = (ulong)(SubTree4 != null ? SubTree4.FilePosition : 0);

            writer.Write(AABBMin);
            writer.Write(AABBMax);
            writer.WriteStruct(CellAABB);
            writer.Write(DataPointer);
            writer.Write(SubTree1Pointer);
            writer.Write(SubTree2Pointer);
            writer.Write(SubTree3Pointer);
            writer.Write(SubTree4Pointer);
            writer.Write(Unused_54h);
            writer.Write(Unused_58h);
            writer.Write(Unused_5Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Data != null) list.Add(Data);
            if (SubTree1 != null) list.Add(SubTree1);
            if (SubTree2 != null) list.Add(SubTree2);
            if (SubTree3 != null) list.Add(SubTree3);
            if (SubTree4 != null) list.Add(SubTree4);
            return list.ToArray();
        }


        public override string ToString()
        {
            return "[Min: "+AABBMin.ToString() + "], [Max:" + AABBMax.ToString() + "]";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class NavMeshSectorData : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32; }
        }

        public uint UnkOffset { get; set; }
        public uint Unused_04h { get; set; } // 0x00000000
        public ulong PolyIDsPointer { get; set; }
        public ulong UnkDataPointer { get; set; }
        public ushort PolyIDsCount { get; set; }
        public ushort UnkDataCount { get; set; }
        public uint Unused_1Ch { get; set; } // 0x00000000

        public ushort[] PolyIDs { get; set; }
        public NavMeshSectorDataUnk[] UnkData { get; set; }

        private ResourceSystemStructBlock<ushort> PolyIDsBlock = null;
        private ResourceSystemStructBlock<NavMeshSectorDataUnk> UnkDataBlock = null;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            UnkOffset = reader.ReadUInt32();
            Unused_04h = reader.ReadUInt32();
            PolyIDsPointer = reader.ReadUInt64();
            UnkDataPointer = reader.ReadUInt64();
            PolyIDsCount = reader.ReadUInt16();
            UnkDataCount = reader.ReadUInt16();
            Unused_1Ch = reader.ReadUInt32();

            PolyIDs = reader.ReadUshortsAt(PolyIDsPointer, PolyIDsCount);
            UnkData = reader.ReadStructsAt<NavMeshSectorDataUnk>(UnkDataPointer, UnkDataCount);

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            PolyIDsPointer = (ulong)(PolyIDsBlock?.FilePosition ?? 0);
            PolyIDsCount = (ushort)(PolyIDs?.Length ?? 0);
            UnkDataPointer = (ulong)(UnkDataBlock?.FilePosition ?? 0);
            UnkDataCount = (ushort)(UnkData?.Length ?? 0);


            writer.Write(UnkOffset);
            writer.Write(Unused_04h);
            writer.Write(PolyIDsPointer);
            writer.Write(UnkDataPointer);
            writer.Write(PolyIDsCount);
            writer.Write(UnkDataCount);
            writer.Write(Unused_1Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if ((PolyIDs != null) && (PolyIDs.Length > 0))
            {
                PolyIDsBlock = new ResourceSystemStructBlock<ushort>(PolyIDs);
                list.Add(PolyIDsBlock);
            }
            if ((UnkData != null) && (UnkData.Length > 0))
            {
                UnkDataBlock = new ResourceSystemStructBlock<NavMeshSectorDataUnk>(UnkData);
                list.Add(UnkDataBlock);
            }


            return list.ToArray();
        }

        public override string ToString()
        {
            return "(Polys: " + PolyIDsCount.ToString() + ", UnkOffset: " + UnkOffset.ToString() + ", UnkCount: " + UnkDataCount.ToString() + ")";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshSectorDataUnk
    {
        public ushort Unknown_0h { get; set; }
        public ushort Unknown_2h { get; set; }
        public ushort Unknown_4h { get; set; }
        public ushort Unknown_6h { get; set; }

        public override string ToString()
        {
            return Unknown_0h.ToString() + ", " + Unknown_2h.ToString() + ", " + Unknown_4h.ToString() + ", " + Unknown_6h.ToString();
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshPortal
    {
        public uint Unknown_00h { get; set; }
        public NavMeshVertex Position1 { get; set; }
        public NavMeshVertex Position2 { get; set; }
        public ushort Unknown_10h { get; set; }
        public ushort Unknown_12h { get; set; }
        public ushort Unknown_14h { get; set; }
        public ushort Unknown_16h { get; set; }
        public ushort Unknown_18h { get; set; }
        public ushort Unknown_1Ah { get; set; }
        //public NavMeshAABB AABB1 { get; set; }
        //public NavMeshAABB AABB2 { get; set; }

        public override string ToString()
        {
            return Unknown_00h.ToString() + ", " + 
                   Position1.ToString() + ", " + Position2.ToString() + ", " +
                   Unknown_10h.ToString() + ", " + Unknown_12h.ToString() + ", " +
                   Unknown_14h.ToString() + ", " + Unknown_16h.ToString() + ", " +
                   Unknown_18h.ToString() + ", " + Unknown_1Ah.ToString();
                   //AABB1.ToString() + ", " + AABB2.ToString();
        }
    }




    [Flags] public enum NavMeshFlags : uint
    {
        Vertices = 1,
        Portals = 2,
        Vehicle = 4,
        Unknown8 = 8,
    }


}
