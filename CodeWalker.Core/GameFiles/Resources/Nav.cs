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
using System.Xml;

namespace CodeWalker.GameFiles
{


    [TypeConverter(typeof(ExpandableObjectConverter))] public class NavMesh : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 368; }
        }


        public NavMeshFlags ContentFlags { get; set; }
        public uint VersionUnk1 { get; set; } = 0x00010011; // 0x00010011
        public uint Unused_018h { get; set; } // 0x00000000
        public uint Unused_01Ch { get; set; } // 0x00000000
        public Matrix Transform { get; set; } //(1,0,0,NaN),(0,1,0,NaN),(0,0,1,NaN),(0,0,0,NaN)
        public Vector3 AABBSize { get; set; }
        public uint AABBUnk { get; set; } = 0x7F800001; // 0x7F800001 //NaN
        public ulong VerticesPointer { get; set; }
        public uint Unused_078h { get; set; } // 0x00000000
        public uint Unused_07Ch { get; set; } // 0x00000000
        public ulong IndicesPointer { get; set; }
        public ulong EdgesPointer { get; set; }
        public uint EdgesIndicesCount { get; set; }
        public NavMeshUintArray AdjAreaIDs { get; set; }
        public ulong PolysPointer { get; set; }
        public ulong SectorTreePointer { get; set; }
        public ulong PortalsPointer { get; set; }
        public ulong PortalLinksPointer { get; set; }
        public uint VerticesCount { get; set; }
        public uint PolysCount { get; set; }
        public uint AreaID { get; set; } // X + Y*100
        public uint TotalBytes { get; set; }
        public uint PointsCount { get; set; }
        public uint PortalsCount { get; set; }
        public uint PortalLinksCount { get; set; }
        public uint Unused_154h { get; set; } // 0x00000000
        public uint Unused_158h { get; set; } // 0x00000000
        public uint Unused_15Ch { get; set; } // 0x00000000
        public MetaHash VersionUnk2 { get; set; }                //2244687201 (0x85CB3561) for grid ynv's
        public uint Unused_164h { get; set; } // 0x00000000
        public uint Unused_168h { get; set; } // 0x00000000
        public uint Unused_16Ch { get; set; } // 0x00000000


        public NavMeshList<NavMeshVertex> Vertices { get; set; }
        public NavMeshList<ushort> Indices { get; set; }
        public NavMeshList<NavMeshEdge> Edges { get; set; }
        public NavMeshList<NavMeshPoly> Polys { get; set; }
        public NavMeshSector SectorTree { get; set; }
        public NavMeshPortal[] Portals { get; set; }
        public ushort[] PortalLinks { get; set; }




        private ResourceSystemStructBlock<NavMeshPortal> PortalsBlock = null;
        private ResourceSystemStructBlock<ushort> PortalLinksBlock = null;


        public Vector3 AABBMin
        {
            get
            {
                if (SectorTree != null) return SectorTree.AABBMin.XYZ();
                return AABBSize * -0.5f;//shouldn't get here
            }
            set
            {
                if (SectorTree != null)
                {
                    SectorTree.AABBMin = new Vector4(value, 0.0f);
                }
            }
        }
        public Vector3 AABBMax
        {
            get
            {
                if (SectorTree != null) return SectorTree.AABBMax.XYZ();
                return AABBSize * 0.5f;//shouldn't get here
            }
            set
            {
                if (SectorTree != null)
                {
                    SectorTree.AABBMax = new Vector4(value, 0.0f);
                }
            }
        }



        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            ContentFlags = (NavMeshFlags)reader.ReadUInt32();
            VersionUnk1 = reader.ReadUInt32();
            Unused_018h = reader.ReadUInt32();
            Unused_01Ch = reader.ReadUInt32();
            Transform = reader.ReadMatrix();
            AABBSize = reader.ReadVector3();
            AABBUnk = reader.ReadUInt32();
            VerticesPointer = reader.ReadUInt64();
            Unused_078h = reader.ReadUInt32();
            Unused_07Ch = reader.ReadUInt32();
            IndicesPointer = reader.ReadUInt64();
            EdgesPointer = reader.ReadUInt64();
            EdgesIndicesCount = reader.ReadUInt32();
            AdjAreaIDs = reader.ReadStruct<NavMeshUintArray>();
            PolysPointer = reader.ReadUInt64();
            SectorTreePointer = reader.ReadUInt64();
            PortalsPointer = reader.ReadUInt64();
            PortalLinksPointer = reader.ReadUInt64();
            VerticesCount = reader.ReadUInt32();
            PolysCount = reader.ReadUInt32();
            AreaID = reader.ReadUInt32();
            TotalBytes = reader.ReadUInt32();
            PointsCount = reader.ReadUInt32();
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
            Edges = reader.ReadBlockAt<NavMeshList<NavMeshEdge>>(EdgesPointer);
            Polys = reader.ReadBlockAt<NavMeshList<NavMeshPoly>>(PolysPointer);
            SectorTree = reader.ReadBlockAt<NavMeshSector>(SectorTreePointer);
            Portals = reader.ReadStructsAt<NavMeshPortal>(PortalsPointer, PortalsCount);
            PortalLinks = reader.ReadUshortsAt(PortalLinksPointer, PortalLinksCount);




            ////testing!
            //if (VersionUnk1 != 0x00010011)
            //{ }
            //if (Unused_018h != 0)
            //{ }
            //if (Unused_01Ch != 0)
            //{ }
            //if (AABBUnk != 0x7F800001)
            //{ }
            //if (Unused_078h != 0)
            //{ }
            //if (Unused_07Ch != 0)
            //{ }
            //if (Unused_154h != 0)
            //{ }
            //if (Unused_158h != 0)
            //{ }
            //if (Unused_15Ch != 0)
            //{ }
            //if (Unused_164h != 0)
            //{ }
            //if (Unused_168h != 0)
            //{ }
            //if (Unused_16Ch != 0)
            //{ }
            //switch (VersionUnk2.Hash)
            //{
            //    case 0: //vehicle
            //        break;
            //    case 0x85CB3561: //grid
            //        break;
            //    default:
            //        break;
            //}
            //UpdateCounts();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            VerticesPointer = (ulong)(Vertices != null ? Vertices.FilePosition : 0);
            IndicesPointer = (ulong)(Indices != null ? Indices.FilePosition : 0);
            EdgesPointer = (ulong)(Edges != null ? Edges.FilePosition : 0);
            PolysPointer = (ulong)(Polys != null ? Polys.FilePosition : 0);
            SectorTreePointer = (ulong)(SectorTree != null ? SectorTree.FilePosition : 0);
            PortalsPointer = (ulong)(PortalsBlock?.FilePosition ?? 0);
            PortalLinksPointer = (ulong)(PortalLinksBlock?.FilePosition ?? 0);


            UpdateCounts();


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
            writer.Write(EdgesPointer);
            writer.Write(EdgesIndicesCount);
            writer.WriteStruct(AdjAreaIDs);
            writer.Write(PolysPointer);
            writer.Write(SectorTreePointer);
            writer.Write(PortalsPointer);
            writer.Write(PortalLinksPointer);
            writer.Write(VerticesCount);
            writer.Write(PolysCount);
            writer.Write(AreaID);
            writer.Write(TotalBytes);
            writer.Write(PointsCount);
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
            if (Edges != null) list.Add(Edges);
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





        public void UpdateCounts()
        {
            EdgesIndicesCount = Indices?.ItemCount ?? 0;
            VerticesCount = Vertices?.ItemCount ?? 0;
            PolysCount = Polys?.ItemCount ?? 0;
            PortalsCount = (uint)(Portals?.Length ?? 0);
            PortalLinksCount = (uint)(PortalLinks?.Length ?? 0);



            uint totbytes = 0;
            uint pointcount = 0;
            var treestack = new Stack<NavMeshSector>();
            if (SectorTree != null)
            {
                treestack.Push(SectorTree);
            }
            while (treestack.Count > 0)
            {
                var sector = treestack.Pop();
                totbytes += sector.ByteCount;
                pointcount += sector.PointCount;
                if (sector.SubTree1 != null) treestack.Push(sector.SubTree1);
                if (sector.SubTree2 != null) treestack.Push(sector.SubTree2);
                if (sector.SubTree3 != null) treestack.Push(sector.SubTree3);
                if (sector.SubTree4 != null) treestack.Push(sector.SubTree4);
            }

            totbytes += Vertices?.ByteCount ?? 0;
            totbytes += Indices?.ByteCount ?? 0;
            totbytes += Edges?.ByteCount ?? 0;
            totbytes += Polys?.ByteCount ?? 0;
            totbytes += PortalsCount * 28;
            if ((TotalBytes != totbytes) && (TotalBytes != 0))
            { }
            TotalBytes = totbytes;



            if ((PointsCount != pointcount) && (PointsCount != 0))
            { }
            PointsCount = pointcount;
        }



        public void SetDefaults(bool vehicle)
        {
            VersionUnk1 = 0x00010011;
            VersionUnk2 = vehicle ? 0 : 0x85CB3561;
            Transform = Matrix.Identity;
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
            set
            {
                Count = (uint)value.Length;
                v00 = (Count > 0) ? value[0] : 0;
                v01 = (Count > 1) ? value[1] : 0;
                v02 = (Count > 2) ? value[2] : 0;
                v03 = (Count > 3) ? value[3] : 0;
                v04 = (Count > 4) ? value[4] : 0;
                v05 = (Count > 5) ? value[5] : 0;
                v06 = (Count > 6) ? value[6] : 0;
                v07 = (Count > 7) ? value[7] : 0;
                v08 = (Count > 8) ? value[8] : 0;
                v09 = (Count > 9) ? value[9] : 0;
                v10 = (Count > 10) ? value[10] : 0;
                v11 = (Count > 11) ? value[11] : 0;
                v12 = (Count > 12) ? value[12] : 0;
                v13 = (Count > 13) ? value[13] : 0;
                v14 = (Count > 14) ? value[14] : 0;
                v15 = (Count > 15) ? value[15] : 0;
                v16 = (Count > 16) ? value[16] : 0;
                v17 = (Count > 17) ? value[17] : 0;
                v18 = (Count > 18) ? value[18] : 0;
                v19 = (Count > 19) ? value[19] : 0;
                v20 = (Count > 20) ? value[20] : 0;
                v21 = (Count > 21) ? value[21] : 0;
                v22 = (Count > 22) ? value[22] : 0;
                v23 = (Count > 23) ? value[23] : 0;
                v24 = (Count > 24) ? value[24] : 0;
                v25 = (Count > 25) ? value[25] : 0;
                v26 = (Count > 26) ? value[26] : 0;
                v27 = (Count > 27) ? value[27] : 0;
                v28 = (Count > 28) ? value[28] : 0;
                v29 = (Count > 29) ? value[29] : 0;
                v30 = (Count > 30) ? value[30] : 0;
                v31 = (Count > 31) ? value[31] : 0;
            }
        }

        public uint Get(uint i)
        {
            switch (i)
            {
                default:
                case 0: return v00;
                case 1: return v01;
                case 2: return v02;
                case 3: return v03;
                case 4: return v04;
                case 5: return v05;
                case 6: return v06;
                case 7: return v07;
                case 8: return v08;
                case 9: return v09;
                case 10: return v10;
                case 11: return v11;
                case 12: return v12;
                case 13: return v13;
                case 14: return v14;
                case 15: return v15;
                case 16: return v16;
                case 17: return v17;
                case 18: return v18;
                case 19: return v19;
                case 20: return v20;
                case 21: return v21;
                case 22: return v22;
                case 23: return v23;
                case 24: return v24;
                case 25: return v25;
                case 26: return v26;
                case 27: return v27;
                case 28: return v28;
                case 29: return v29;
                case 30: return v30;
                case 31: return v31;
            }
        }

        public void Set(uint[] arr)
        {
            Values = arr;
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
        public int ItemSize { get { return System.Runtime.InteropServices.Marshal.SizeOf<T>(); } }

        public uint ByteCount
        {
            get
            {
                return ItemCount * (uint)ItemSize;
            }
        }



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


        public void RebuildList(List<T> items)
        {

            //max bytes per part: 16384
            int maxpartbytes = 16384; //0x4000
            int itembytes = ItemSize;
            int itemsperpart = maxpartbytes / itembytes;
            int currentitem = 0;

            var parts = new ResourceSimpleArray<NavMeshListPart<T>>();
            var partitems = new List<T>();
            var offsets = new List<uint>();

            while (currentitem < items.Count)
            {
                partitems.Clear();
                int lastitem = currentitem + itemsperpart;
                if (lastitem > items.Count) lastitem = items.Count;
                for (int i = currentitem; i < lastitem; i++)
                {
                    partitems.Add(items[i]);
                }
                var part = new NavMeshListPart<T>();
                part.Items = partitems.ToArray();
                part.Unknown_0Ch = 0;
                parts.Add(part);
                offsets.Add((uint)currentitem);
                currentitem = lastitem;
            }
            ListParts = parts;
            ListOffsets = offsets.ToArray();
            ItemCount = (uint)items.Count;
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
            X = (ushort)Math.Round(v.X * usmax);
            Y = (ushort)Math.Round(v.Y * usmax);
            Z = (ushort)Math.Round(v.Z * usmax);
        }

        public static NavMeshVertex Create(Vector3 v)
        {
            var nmv = new NavMeshVertex();
            nmv.FromVector3(v);
            return nmv;
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

        public Vector3 Min
        {
            get { return new Vector3(MinX / 4.0f, MinY / 4.0f, MinZ / 4.0f); }
            set { var v = value * 4.0f; MinX = (short)Math.Floor(v.X); MinY = (short)Math.Floor(v.Y); MinZ = (short)Math.Floor(v.Z); }
        }
        public Vector3 Max
        {
            get { return new Vector3(MaxX / 4.0f, MaxY / 4.0f, MaxZ / 4.0f); }
            set { var v = value * 4.0f; MaxX = (short)Math.Ceiling(v.X); MaxY = (short)Math.Ceiling(v.Y); MaxZ = (short)Math.Ceiling(v.Z); }
        }

        public override string ToString()
        {
            Vector3 min = Min;
            Vector3 max = Max;
            return string.Format("({0}, {1}, {2}) | ({3}, {4}, {5})", min.X, min.Y, min.Z, max.X, max.Y, max.Z);
            //return string.Format("({0}, {1}, {2}) | ({3}, {4}, {5})", MinX, MinY, MinZ, MaxX, MaxY, MaxZ);
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshEdge
    {
        public NavMeshEdgePart _Poly1;
        public NavMeshEdgePart _Poly2;
        public NavMeshEdgePart Poly1 { get { return _Poly1; } set { _Poly1 = value; } }
        public NavMeshEdgePart Poly2 { get { return _Poly2; } set { _Poly2 = value; } }

        public override string ToString()
        {
            return //Poly1.Bin + " | " + Poly2.Bin + " | " + 
                   _Poly1.ToString() + " | " + _Poly2.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshEdgePart
    {
        public uint Value { get; set; }

        public string Bin
        {
            get
            {
                return Convert.ToString(Value, 2).PadLeft(32, '0');
            }
        }

        public uint AreaIDInd { get { return (Value >> 0) & 0x1F; } set { Value = (Value & 0xFFFFFFE0) | (value & 0x1F); } }
        public uint PolyID { get { return (Value >> 5) & 0x3FFF; } set { Value = (Value & 0xFFF8001F) | ((value & 0x3FFF) << 5); } }
        public uint Unk2 { get { return (Value >> 19) & 0x3; } set { Value = (Value & 0xFFE7FFFF) | ((value & 0x3) << 19); } }
        public uint Unk3 { get { return (Value >> 21) & 0x7FF; } set { Value = (Value & 0x001FFFFF) | ((value & 0x7FF) << 21); } }

        public override string ToString()
        {
            string pid = (PolyID == 0x3FFF) ? "-" : PolyID.ToString();
            return AreaIDInd.ToString() + ", " + pid + ", " + Unk2.ToString() + ", " + Unk3.ToString();
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshPoly
    {
        public ushort PolyFlags0 { get; set; }
        public ushort IndexFlags { get; set; }
        public ushort IndexID { get; set; }
        public ushort AreaID { get; set; } //always current ynv's AreaID
        public uint Unused_08h { get; set; } // 0x00000000
        public uint Unused_0Ch { get; set; } // 0x00000000
        public uint Unused_10h { get; set; } // 0x00000000
        public uint Unused_14h { get; set; } // 0x00000000
        public NavMeshAABB CellAABB { get; set; }
        public uint PolyFlags1 { get; set; }
        public uint PolyFlags2 { get; set; }
        public uint PartFlags { get; set; }


        //public int IndexUnk { get { return (IndexFlags >> 0) & 31; } } //always 0
        public int IndexCount { get { return (IndexFlags >> 5); } set { IndexFlags = (ushort)((IndexFlags & 31) | ((value & 0x7FF) << 5)); } }

        //public int PartUnk1 { get { return (PartFlags >> 0) & 0xF; } } //always 0
        public ushort PartID { get { return (ushort)((PartFlags >> 4) & 0xFF); } set { PartFlags = ((PartFlags & 0xFFFFF00F) | (((uint)value & 0xFF) << 4)); } }
        public byte PortalLinkCount { get { return (byte)((PartFlags >> 12) & 0x7); } set { PartFlags = ((PartFlags & 0xFFFF8FFF) | (((uint)value & 0x7) << 12)); } }
        public uint PortalLinkID { get { return ((PartFlags >> 15) & 0x1FFFF); } set { PartFlags = ((PartFlags & 0x7FFF) | ((value & 0x1FFFF) << 15)); } }


        public byte UnkX { get { return (byte)((PolyFlags2 >> 0) & 0xFF); } set { PolyFlags2 = (PolyFlags2 & 0xFFFFFF00) | ((value & 0xFFu)<<0); } }
        public byte UnkY { get { return (byte)((PolyFlags2 >> 8) & 0xFF); } set { PolyFlags2 = (PolyFlags2 & 0xFFFF00FF) | ((value & 0xFFu)<<8); } }

        public byte Flags1 { get { return (byte)(PolyFlags0 & 0xFF); } set { PolyFlags0 = (ushort)((PolyFlags0 & 0xFF00) | (value & 0xFF)); } }
        public byte Flags2 { get { return (byte)((PolyFlags1 >> 0) & 0xFF); } set { PolyFlags1 = ((PolyFlags1 & 0xFFFFFF00u) | ((value & 0xFFu) << 0)); } }
        public byte Flags3 { get { return (byte)((PolyFlags1 >> 9) & 0xFF); } set { PolyFlags1 = ((PolyFlags1 & 0xFFFE01FFu) | ((value & 0xFFu) << 9)); } }
        public byte Flags4 { get { return (byte)((PolyFlags2 >> 16) & 0xFF); } set { PolyFlags2 = ((PolyFlags2 & 0xFF00FFFFu) | ((value & 0xFFu) << 16)); } }

        //public uint UnkFlags0 { get { return (uint)((PolyFlags0 >> 8) & 0xFF); } } //always 0
        //public uint UnkFlags1 { get { return (uint)((PolyFlags1 >> 17) & 0xFFFF); } } //always 0
        //public uint UnkFlags2 { get { return (uint)((PolyFlags2 >> 24) & 0xFF); } } //always 0


        public override string ToString()
        {
            return
                PolyFlags0.ToString() + ", " +
                //IndexFlags.ToString() + ", " + 
                IndexCount.ToString() + ", " + //IndexUnk.ToString() + ", " +
                IndexID.ToString() + ", " + AreaID.ToString() + ", " +
                CellAABB.ToString() + ", " +
                //PolyFlags1.ToString() + ", " + 
                //PolyFlags2.ToString() + ", " + 
                //PartFlags.ToString() + ", " + //PartUnk1.ToString() + ", " + 
                PartID.ToString() + ", " + 
                PortalLinkCount.ToString() + ", " +
                PortalLinkID.ToString();
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

        public uint ByteCount
        {
            get
            {
                uint totbytes = (uint)BlockLength;
                if (Data != null)
                {
                    totbytes += ((uint)Data.BlockLength);
                    totbytes += ((uint)Data.PolyIDsCount * 2);
                    totbytes += ((uint)Data.PointsCount * 8);
                }
                return totbytes;
            }
        }
        public uint PointCount
        {
            get
            {
                if (Data == null) return 0;
                return Data.PointsCount;
            }
        }

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


        public void SetAABBs(Vector3 min, Vector3 max)
        {
            AABBMin = new Vector4(min, float.NaN);
            AABBMax = new Vector4(max, float.NaN);
            CellAABB = new NavMeshAABB() { Min = min, Max = max };
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

        public uint PointsStartID { get; set; }
        public uint Unused_04h { get; set; } // 0x00000000
        public ulong PolyIDsPointer { get; set; }
        public ulong PointsPointer { get; set; }
        public ushort PolyIDsCount { get; set; }
        public ushort PointsCount { get; set; }
        public uint Unused_1Ch { get; set; } // 0x00000000

        public ushort[] PolyIDs { get; set; }
        public NavMeshPoint[] Points { get; set; }

        public ResourceSystemStructBlock<ushort> PolyIDsBlock = null;
        public ResourceSystemStructBlock<NavMeshPoint> PointsBlock = null;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            PointsStartID = reader.ReadUInt32();
            Unused_04h = reader.ReadUInt32();
            PolyIDsPointer = reader.ReadUInt64();
            PointsPointer = reader.ReadUInt64();
            PolyIDsCount = reader.ReadUInt16();
            PointsCount = reader.ReadUInt16();
            Unused_1Ch = reader.ReadUInt32();

            PolyIDs = reader.ReadUshortsAt(PolyIDsPointer, PolyIDsCount);
            Points = reader.ReadStructsAt<NavMeshPoint>(PointsPointer, PointsCount);

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            PolyIDsPointer = (ulong)(PolyIDsBlock?.FilePosition ?? 0);
            PolyIDsCount = (ushort)(PolyIDs?.Length ?? 0);
            PointsPointer = (ulong)(PointsBlock?.FilePosition ?? 0);
            PointsCount = (ushort)(Points?.Length ?? 0);


            writer.Write(PointsStartID);
            writer.Write(Unused_04h);
            writer.Write(PolyIDsPointer);
            writer.Write(PointsPointer);
            writer.Write(PolyIDsCount);
            writer.Write(PointsCount);
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
            if ((Points != null) && (Points.Length > 0))
            {
                PointsBlock = new ResourceSystemStructBlock<NavMeshPoint>(Points);
                list.Add(PointsBlock);
            }


            return list.ToArray();
        }

        public override string ToString()
        {
            return "(Polys: " + PolyIDsCount.ToString() + ", PointsCount: " + PointsCount.ToString() + ", PointsStartID: " + PointsStartID.ToString() + ")";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshPoint
    {
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public ushort Z { get; set; }
        public byte Angle { get; set; }
        public byte Type { get; set; }//0,1,2,3,4,5,128,171,254


        public Vector3 Position
        {
            get
            {
                const float usmax = ushort.MaxValue;
                return new Vector3(X / usmax, Y / usmax, Z / usmax);
            }
            set
            {
                const float usmax = ushort.MaxValue;
                X = (ushort)Math.Round(value.X * usmax);
                Y = (ushort)Math.Round(value.Y * usmax);
                Z = (ushort)Math.Round(value.Z * usmax);
            }
        }

        public override string ToString()
        {
            return Type.ToString() + ": " + Angle.ToString() + ", " + Position.ToString();
        }


    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NavMeshPortal
    {
        public byte Type { get; set; }//1,2,3
        public byte Angle { get; set; }
        public ushort FlagsUnk { get; set; }//always 0
        public NavMeshVertex PositionFrom { get; set; }
        public NavMeshVertex PositionTo { get; set; }
        public ushort PolyIDFrom1 { get; set; }
        public ushort PolyIDFrom2 { get; set; } //always same as PolyIDFrom1
        public ushort PolyIDTo1 { get; set; }
        public ushort PolyIDTo2 { get; set; } //always same as PolyIDTo1
        public uint AreaFlags { get; set; }

        public ushort AreaIDFrom { get { return (ushort)(AreaFlags & 0x3FFF); } set { AreaFlags = (AreaFlags & 0xFFFFC000) | (value & 0x3FFFu); } }//always Ynv.AreaID
        public ushort AreaIDTo { get { return (ushort)((AreaFlags >> 14) & 0x3FFF); } set { AreaFlags = (AreaFlags & 0xF0003FFF) | ((value & 0x3FFFu) << 14); } }//always Ynv.AreaID
        public byte AreaUnk { get { return (byte)((AreaFlags >> 28) & 0xF); } set { AreaFlags = (AreaFlags & 0x0FFFFFFF) | ((value & 0xFu) << 28); } }//always 0

        public override string ToString()
        {
            return AreaIDFrom.ToString() + ", " + AreaIDTo.ToString() + ", " + AreaUnk.ToString() + ", " +
                   PolyIDFrom1.ToString() + ", " + PolyIDFrom2.ToString() + ", " +
                   PolyIDTo1.ToString() + ", " + PolyIDTo2.ToString() + ", " +
                   Type.ToString() + ", " + Angle.ToString() + ", " + FlagsUnk.ToString() + ", " +
                   "(" + PositionFrom.ToString() + " | " + PositionTo.ToString() + ")";
        }
    }




    [Flags] public enum NavMeshFlags : uint
    {
        None = 0,
        Polygons = 1,
        Portals = 2,
        Vehicle = 4,
        Unknown8 = 8,
        Unknown16 = 16,
    }


}
