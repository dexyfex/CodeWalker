﻿using SharpDX;
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

        public string Version { get; set; }
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
            Version = sb.ToString().Replace("[VERSION]", "").Replace("\r", "").Replace("\n", "");
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
                                dates.Add(new CacheFileDate(line));//eg: 2740459947 (hash of: platform:/data/cdimages/scaleform_frontend.rpf) 130680580712018938 8944
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


        public void LoadXml(string xml)
        {
        }

        public string GetXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(MetaXmlBase.XmlHeader);
            sb.AppendLine(string.Format("<CacheDatFile version=\"{0}\">", Version));
            sb.AppendLine(" <fileDates>");
            if (FileDates != null)
            {
                foreach (var date in FileDates)
                {
                    sb.AppendLine(string.Format("  <fileDate>{0}</fileDate>", date.ToCacheFileString()));
                }
            }
            sb.AppendLine(" </fileDates>");
            sb.AppendLine(" <module type=\"fwMapDataStore\">");
            if (AllMapNodes != null)
            {
                foreach (var mapnode in AllMapNodes)
                {
                    sb.AppendLine("  <Item>");
                    sb.AppendLine(string.Format("   <name>{0}</name>", mapnode.Name.ToCleanString()));
                    sb.AppendLine(string.Format("   <parent>{0}</parent>", mapnode.ParentName.ToCleanString()));
                    sb.AppendLine(string.Format("   <contentFlags value=\"{0}\" />", mapnode.ContentFlags.ToString()));
                    sb.AppendLine(string.Format("   <streamingExtentsMin {0} />", FloatUtil.GetVector3XmlString(mapnode.streamingExtentsMin)));
                    sb.AppendLine(string.Format("   <streamingExtentsMax {0} />", FloatUtil.GetVector3XmlString(mapnode.streamingExtentsMax)));
                    sb.AppendLine(string.Format("   <entitiesExtentsMin {0} />", FloatUtil.GetVector3XmlString(mapnode.entitiesExtentsMin)));
                    sb.AppendLine(string.Format("   <entitiesExtentsMax {0} />", FloatUtil.GetVector3XmlString(mapnode.entitiesExtentsMax)));
                    sb.AppendLine(string.Format("   <flags unk1=\"{0}\" unk2=\"{1}\" unk3=\"{2}\" />", mapnode.Unk1, mapnode.Unk2, mapnode.Unk3));
                    sb.AppendLine("  </Item>");
                }
            }
            sb.AppendLine(" </module>");
            sb.AppendLine(" <module type=\"CInteriorProxy\">");
            if (AllCInteriorProxies != null)
            {
                foreach (var intprox in AllCInteriorProxies)
                {
                    sb.AppendLine("  <Item>");
                    sb.AppendLine(string.Format("   <name>{0}</name>", intprox.Name.ToCleanString()));
                    sb.AppendLine(string.Format("   <parent>{0}</parent>", intprox.Parent.ToCleanString()));
                    sb.AppendLine(string.Format("   <position {0} />", FloatUtil.GetVector3XmlString(intprox.Position)));
                    sb.AppendLine(string.Format("   <rotation {0} />", FloatUtil.GetQuaternionXmlString(intprox.Orientation)));
                    sb.AppendLine(string.Format("   <aabbMin {0} />", FloatUtil.GetVector3XmlString(intprox.BBMin)));
                    sb.AppendLine(string.Format("   <aabbMax {0} />", FloatUtil.GetVector3XmlString(intprox.BBMax)));
                    sb.AppendLine(string.Format("   <unknowns1 unk01=\"{0}\" unk03=\"{1}\" />", intprox.Unk01, intprox.Unk03));
                    sb.AppendLine(string.Format("   <unknowns2 unk11=\"{0}\" unk12=\"{1}\" unk13=\"{1}\" unk14=\"{1}\" />", intprox.Unk11, intprox.Unk12, intprox.Unk13, intprox.Unk14));
                    sb.AppendLine(string.Format("   <unknowns3 unk15=\"{0}\" unk16=\"{1}\" unk17=\"{1}\" unk18=\"{1}\" />", intprox.Unk15, intprox.Unk16, intprox.Unk17, intprox.Unk18));
                    sb.AppendLine("  </Item>");
                }
            }
            sb.AppendLine(" </module>");
            sb.AppendLine(" <module type=\"BoundsStore\">");
            if (AllBoundsStoreItems != null)
            {
                foreach (var bndstore in AllBoundsStoreItems)
                {
                    sb.AppendLine("  <Item>");
                    sb.AppendLine(string.Format("   <name>{0}</name>", bndstore.Name.ToCleanString()));
                    sb.AppendLine(string.Format("   <aabbMin {0} />", FloatUtil.GetVector3XmlString(bndstore.Min)));
                    sb.AppendLine(string.Format("   <aabbMax {0} />", FloatUtil.GetVector3XmlString(bndstore.Max)));
                    sb.AppendLine(string.Format("   <layer value=\"{0}\" />", bndstore.Layer));
                    sb.AppendLine("  </Item>");
                }
            }
            sb.AppendLine(" </module>");
            sb.AppendLine("</CacheDatFile>");
            return sb.ToString();
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
        public MetaHash FileName { get; set; } //"resource_surrogate:/%s.rpf"
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

        public string ToCacheFileString()
        {
            return FileName.Hash.ToString() + " " + TimeStamp.ToFileTimeUtc().ToString() + " " + FileID.ToString();
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
            Min = b.BoxMin;
            Max = b.BoxMax;
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
            Unk17 = br.ReadUInt32();//could be float
            Unk18 = br.ReadUInt32();



            switch (Unk01)
            {
                case 0: //v_cashdepot
                case 19: //dt1_02_carpark
                case 20: //dt1_03_carpark
                case 21: //dt1_05_carpark
                case 4: //dt1_rd1_tun
                case 14: //id1_11_tunnel1_int
                case 24: //v_firedept
                case 3: //id2_21_a_tun1
                case 22: //po1_08_warehouseint1
                case 11: //sc1_rd_inttun1
                case 10: //sc1_rd_inttun2b_end
                case 18: //bt1_04_carpark
                case 16: //v_hanger
                case 1: //ap1_03_lisapark_subway
                case 13: //kt1_03_carpark_int
                case 5: //sm20_tun1
                case 2: //vbca_tunnel1
                case 15: //cs1_12_tunnel01_int
                case 6: //cs1_14brailway1
                case 9: //cs2_roadsb_tunnel_01
                case 7: //cs3_03railtunnel_int1
                case 8: //cs4_rwayb_tunnelint
                case 12: //ch1_roadsdint_tun1
                case 100: //hei_int_mph_carrierhang3
                case 47: //xm_x17dlc_int_base_ent
                    break;
                default:
                    break;
            }

            if (Unk02 != 0)
            { }

            switch (Unk03)
            {
                case 6: //v_cashdepot
                case 2: //dt1_02_carpark
                case 8: //v_fib01
                case 4: //v_fib03
                case 0: //v_fib04
                case 7: //v_clotheslo
                case 1: //v_gun
                case 3: //v_genbank
                case 11: //v_hospital
                case 5: //v_shop_247
                case 32: //v_abattoir
                case 13: //v_franklins
                case 15: //v_michael
                case 18: //v_faceoffice
                case 29: //v_recycle
                case 9: //v_stadium
                case 54: //v_farmhouse
                case 12: //v_ranch
                case 26: //hei_gta_milo_bar
                case 17: //hei_gta_milo_bedrm
                case 14: //hei_gta_milo_bridge
                case 48: //apa_mpapa_yacht
                    break;
                default:
                    break;
            }


            if ((Unk12 == 0) || (Unk12 > 0xFFFFFF))
            { }

            switch (Unk14)
            {
                case 1:
                case 0:
                case 580:
                case 355: //sm_smugdlc_int_01
                case 579: //xm_x17dlc_int_01
                    break;
                default:
                    break;
            }
            switch (Unk16)
            {
                case 1:
                case 32758: //0x7FF6
                    break;
                default:
                    break;
            }
            switch (Unk17) //could be a float..!
            {
                case 9:
                    break;
                case 0x415CBC04: //13.7959f
                case 0x7B81AC94:
                case 0x40FE3224: //7.94362f v_gun
                case 0x41515774: //13.0839f v_gun
                case 0x414E7B34: //12.9051f bkr_biker_dlc_int_03
                case 0x41389C14: //11.5381f imp_impexp_int_01
                case 0x4177B664: //15.482f gr_grdlc_int_01
                case 0xCE0404F4: //         sm_smugdlc_int_01
                    break;
                default:
                    //string str = JenkIndex.GetString(Unk17);
                    break;
            }
            switch(Unk18)
            {
                case 0:
                case 1:
                case 32758: //0x7FF6
                    break;
                default:
                    break;
            }

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
        public byte Unk1 { get; set; }
        public byte Unk2 { get; set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }

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
            Unk1 = br.ReadByte(); //HD flag? (critical, long, strm)
            Unk2 = br.ReadByte(); //lod flag? - primary map files
            Unk3 = br.ReadByte(); //slod flag?
            Unk4 = br.ReadByte();

            if (Unk1 != 0)
            { }
            if (Unk2 != 0)
            { }
            if (Unk3 != 0)
            { }
            if (Unk4 != 0)
            { } //no hits here now..

            if (Unk4 == 0xFE)
            {
                //this seems to never be hit anymore...
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
