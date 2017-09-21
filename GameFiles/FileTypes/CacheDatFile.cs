using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public class CacheDatFile : PackedFile
    {
        public RpfFileEntry FileEntry { get; set; }

        public CacheFileDate[] FileDates { get; set; }

        public Dictionary<uint, MapDataStoreNode> MapNodeDict { get; set; }
        public MapDataStoreNode[] RootMapNodes { get; set; }
        //public Dictionary<MetaHash, CInteriorProxy> InteriorProxyDict { get; set; }
        public Dictionary<MetaHash, BoundsStoreItem> BoundsStoreDict { get; set; }

        public MapDataStoreNode[] AllMapNodes { get; set; }
        public CInteriorProxy[] AllCInteriorProxies { get; set; }
        public BoundsStoreItem[] AllBoundsStoreItems { get; set; }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            FileEntry = entry;

            MemoryStream ms = new MemoryStream(data);
            BinaryReader br = new BinaryReader(ms);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; (i < 100) && (i < data.Length); i++)
            {
                //read version string.
                byte b = data[i];
                if (b == 0) break;
                sb.Append((char)b);
            }
            string versionstr = sb.ToString();
            sb.Clear();
            int lastn = 0;
            int lspos = 0;
            uint structcount = 0;
            uint modlen;
            bool indates = false;
            List<string> lines = new List<string>();
            var dates = new List<CacheFileDate>();
            var allMapNodes = new List<MapDataStoreNode>();
            var allCInteriorProxies = new List<CInteriorProxy>();
            var allBoundsStoreItems = new List<BoundsStoreItem>();
            for (int i = 100; i < data.Length; i++)
            {
                byte b = data[i];
                if (b == 0)
                    break;
                if (b == 0xA)
                {
                    lastn = i;
                    string line = sb.ToString();
                    lines.Add(line);
                    switch (line)
                    {
                        case "<fileDates>":
                            indates = true;
                            break;
                        case "</fileDates>":
                            indates = false;
                            break;
                        case "<module>":
                            break;
                        case "</module>":
                            break;
                        case "fwMapDataStore":
                            ms.Position = i + 1;
                            modlen = br.ReadUInt32();
                            structcount = modlen / 64;
                            lspos = i + (int)modlen + 5;
                            while (ms.Position<lspos)
                            {
                                allMapNodes.Add(new MapDataStoreNode(br));
                            }
                            //if (allMapNodes.Count != structcount)
                            //{ }//test fail due to variable length struct
                            i += (int)(modlen + 4);
                            break;
                        case "CInteriorProxy":
                            ms.Position = i + 1;
                            modlen = br.ReadUInt32();
                            structcount = modlen / 104;
                            lspos = i + (int)modlen + 5;
                            while (ms.Position < lspos)
                            {
                                allCInteriorProxies.Add(new CInteriorProxy(br));
                            }
                            if (allCInteriorProxies.Count != structcount)
                            { }//all pass here
                            i += (int)(modlen + 4);
                            break;
                        case "BoundsStore":
                            ms.Position = i + 1;
                            modlen = br.ReadUInt32();
                            structcount = modlen / 32;
                            lspos = i + (int)modlen + 5;
                            while (ms.Position < lspos)
                            {
                                allBoundsStoreItems.Add(new BoundsStoreItem(br));
                            }
                            if (allBoundsStoreItems.Count != structcount)
                            { }//all pass here
                            i += (int)(modlen + 4);
                            break;
                        default:
                            if (!indates)
                            { } //just testing
                            else
                            {
                                dates.Add(new CacheFileDate(line));//eg: 2740459947 130680580712018938 8944
                            }
                            break;
                    }

                    sb.Clear();
                }
                else
                {
                    sb.Append((char)b);
                }
            }
            FileDates = dates.ToArray();
            AllMapNodes = allMapNodes.ToArray();
            AllCInteriorProxies = allCInteriorProxies.ToArray();
            AllBoundsStoreItems = allBoundsStoreItems.ToArray();

            MapNodeDict = new Dictionary<uint, MapDataStoreNode>();
            var rootMapNodes = new List<MapDataStoreNode>();
            foreach (var mapnode in AllMapNodes)
            {
                MapNodeDict[mapnode.Name] = mapnode;
                if (mapnode.ParentName == 0)
                {
                    rootMapNodes.Add(mapnode);
                }
                if (mapnode.UnkExtra != null)
                { }//notsure what to do with this
            }
            foreach (var mapnode in AllMapNodes)
            {
                MapDataStoreNode pnode;
                if (MapNodeDict.TryGetValue(mapnode.ParentName, out pnode))
                {
                    pnode.AddChildToList(mapnode);
                }
                else if ((mapnode.ParentName != 0))
                { }
            }
            foreach (var mapnode in AllMapNodes)
            {
                mapnode.ChildrenListToArray();
            }
            RootMapNodes = rootMapNodes.ToArray();



            BoundsStoreDict = new Dictionary<MetaHash, BoundsStoreItem>();
            foreach (BoundsStoreItem item in AllBoundsStoreItems)
            {
                BoundsStoreItem mbsi = null;
                if (BoundsStoreDict.TryGetValue(item.Name, out mbsi))
                { }
                BoundsStoreDict[item.Name] = item;
            }

            //InteriorProxyDict = new Dictionary<MetaHash, CInteriorProxy>();
            foreach (CInteriorProxy prx in AllCInteriorProxies)
            {
                //CInteriorProxy mprx = null;
                //if (InteriorProxyDict.TryGetValue(prx.Name, out mprx))
                //{ }
                //InteriorProxyDict[prx.Name] = prx;//can't do this! multiples with same name different pos


                MapDataStoreNode mnode = null;
                if (MapNodeDict.TryGetValue(prx.Parent, out mnode))
                {
                    mnode.AddInteriorToList(prx);
                }
                else
                { }
            }
            foreach (var mapnode in AllMapNodes)
            {
                mapnode.InteriorProxyListToArray();
            }


            br.Dispose();
            ms.Dispose();

        }

        public override string ToString()
        {
            if (FileEntry != null)
            {
                return FileEntry.ToString();
            }
            return base.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class CacheFileDate
    {
        public MetaHash FileName { get; set; }
        public DateTime TimeStamp { get; set; }
        public uint FileID { get; set; }

        public CacheFileDate(string line)
        {
            string[] parts = line.Split(' ');
            if (parts.Length == 3)
            {
                FileName = new MetaHash(uint.Parse(parts[0]));
                TimeStamp = DateTime.FromFileTimeUtc(long.Parse(parts[1]));
                FileID = uint.Parse(parts[2]);
            }
            else
            { } //testing
        }

        public override string ToString()
        {
            return FileName.ToString() + ", " + TimeStamp.ToString() + ", " + FileID.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class BoundsStoreItem
    {
        public MetaHash Name { get; set; }
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }
        public uint Layer { get; set; }

        public BoundsStoreItem(Bounds b)
        {
            Name = 0;
            Min = b.BoundingBoxMin;
            Max = b.BoundingBoxMax;
            Layer = 0;
        }
        public BoundsStoreItem(BinaryReader br)
        {
            Name = new MetaHash(br.ReadUInt32());
            Min = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Max = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Layer = br.ReadUInt32();
        }

        public override string ToString()
        {
            return Name.ToString() + ", " +
                   Min.ToString() + ", " +
                   Max.ToString() + ", " +
                   Layer.ToString();
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class CInteriorProxy
    {
        public uint Unk01 { get; set; }
        public uint Unk02 { get; set; }
        public uint Unk03 { get; set; }
        public MetaHash Name { get; set; }
        public MetaHash Parent { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Orientation { get; set; }
        public Vector3 BBMin { get; set; }
        public Vector3 BBMax { get; set; }
        public float Unk11 { get; set; }
        public uint Unk12 { get; set; }
        public float Unk13 { get; set; }
        public uint Unk14 { get; set; }
        public float Unk15 { get; set; }
        public uint Unk16 { get; set; }
        public uint Unk17 { get; set; }
        public uint Unk18 { get; set; }

        public CInteriorProxy(BinaryReader br)
        {
            Unk01 = br.ReadUInt32();
            Unk02 = br.ReadUInt32();
            Unk03 = br.ReadUInt32();
            Name = new MetaHash(br.ReadUInt32());
            Parent = new MetaHash(br.ReadUInt32());
            Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Orientation = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            BBMin = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            BBMax = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Unk11 = br.ReadSingle();
            Unk12 = br.ReadUInt32();
            Unk13 = br.ReadSingle();
            Unk14 = br.ReadUInt32();
            Unk15 = br.ReadSingle();
            Unk16 = br.ReadUInt32();
            Unk17 = br.ReadUInt32();
            Unk18 = br.ReadUInt32();
        }

        public override string ToString()
        {
            return Unk01.ToString() + ", " +
                   Unk02.ToString() + ", " +
                   Unk03.ToString() + ", " +
                   Name.ToString() + ", " +
                   Parent.ToString() + ", " +
                   Position.ToString() + ", " +
                   Orientation.ToString() + ", " +
                   BBMin.ToString() + ", " +
                   BBMax.ToString() + ", " +
                   Unk11.ToString() + ", " +
                   Unk12.ToString() + ", " +
                   Unk13.ToString() + ", " +
                   Unk14.ToString() + ", " +
                   Unk15.ToString() + ", " +
                   Unk16.ToString() + ", " +
                   Unk17.ToString() + ", " +
                   Unk18.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class MapDataStoreNode
    {
        public MetaHash Name { get; set; }
        public MetaHash ParentName { get; set; }
        public uint ContentFlags { get; set; }
        public Vector3 streamingExtentsMin { get; set; }
        public Vector3 streamingExtentsMax { get; set; }
        public Vector3 entitiesExtentsMin { get; set; }
        public Vector3 entitiesExtentsMax { get; set; }
        public byte Unk02 { get; set; }
        public byte Unk03 { get; set; }
        public byte Unk04 { get; set; }
        public byte Unk05 { get; set; }

        public MapDataStoreNodeExtra UnkExtra { get; set; }

        public MapDataStoreNode[] Children { get; set; }
        private List<MapDataStoreNode> ChildrenList; //used when building the array

        public CInteriorProxy[] InteriorProxies { get; set; }
        private List<CInteriorProxy> InteriorProxyList;

        public MapDataStoreNode(YmapFile ymap)
        {
            Name = ymap._CMapData.name;
            ParentName = ymap._CMapData.parent;
            ContentFlags = ymap._CMapData.contentFlags;
            streamingExtentsMin = ymap._CMapData.streamingExtentsMin;
            streamingExtentsMax = ymap._CMapData.streamingExtentsMax;
            entitiesExtentsMin = ymap._CMapData.entitiesExtentsMin;
            entitiesExtentsMax = ymap._CMapData.entitiesExtentsMax;
        }
        public MapDataStoreNode(BinaryReader br)
        {
            Name = new MetaHash(br.ReadUInt32());
            ParentName = new MetaHash(br.ReadUInt32());
            ContentFlags = br.ReadUInt32();
            streamingExtentsMin = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            streamingExtentsMax = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            entitiesExtentsMin = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            entitiesExtentsMax = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Unk02 = br.ReadByte();
            Unk03 = br.ReadByte();
            Unk04 = br.ReadByte();
            Unk05 = br.ReadByte();

            if (Unk05 == 0xFE)
            {
                UnkExtra = new MapDataStoreNodeExtra(br);
            }
        }


        public void AddChildToList(MapDataStoreNode child)
        {
            if (ChildrenList == null)
            {
                ChildrenList = new List<MapDataStoreNode>();
            }
            ChildrenList.Add(child);
        }
        public void ChildrenListToArray()
        {
            if (ChildrenList != null)
            {
                Children = ChildrenList.ToArray();
                ChildrenList = null; //plz get this GC
            }
        }
        public void AddInteriorToList(CInteriorProxy iprx)
        {
            if (InteriorProxyList == null)
            {
                InteriorProxyList = new List<CInteriorProxy>();
            }
            InteriorProxyList.Add(iprx);
        }
        public void InteriorProxyListToArray()
        {
            if (InteriorProxyList != null)
            {
                InteriorProxies = InteriorProxyList.ToArray();
                InteriorProxyList = null; //plz get this GC
            }
        }

        public override string ToString()
        {
            return Name.ToString() + ", " +
                   ParentName.ToString() + ", " +
                   ContentFlags.ToString() + ", " +
                   streamingExtentsMin.ToString() + ", " +
                   streamingExtentsMax.ToString() + ", " +
                   entitiesExtentsMin.ToString() + ", " +
                   entitiesExtentsMax.ToString();// + ", " +
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class MapDataStoreNodeExtra
    {
        public uint Unk01; //0
        public byte[] Unk02; //1 - 16  (60 bytes)
        public uint Unk03;//16
        public uint Unk04;
        public uint Unk05;
        public uint Unk06;
        public uint Unk07;
        public uint Unk08;
        public uint Unk09;
        public uint Unk10;

        public string Unk02str
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (Unk02 != null)
                {
                    for (int i = 0; i < Unk02.Length; i++)
                    {
                        if (Unk02[i] == 0) break;
                        sb.Append((char)Unk02[i]);
                    }
                }
                return sb.ToString();
            }
        }

        public MapDataStoreNodeExtra(BinaryReader br)
        {
            Unk01 = br.ReadUInt32();

            Unk02 = new byte[60];
            for (int i = 0; i < 60; i++)
            {
                Unk02[i] = br.ReadByte();
            }
            Unk03 = br.ReadUInt32();
            Unk04 = br.ReadUInt32();
            Unk05 = br.ReadUInt32();
            Unk06 = br.ReadUInt32();
            Unk07 = br.ReadUInt32();
            Unk08 = br.ReadUInt32();
            Unk09 = br.ReadUInt32();
            Unk10 = br.ReadUInt32();

        }


        public override string ToString()
        {
            return Unk01.ToString() + ", " + Unk02str;
        }
    }



}
