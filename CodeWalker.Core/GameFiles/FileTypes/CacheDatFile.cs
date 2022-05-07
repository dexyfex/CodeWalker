using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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

        public byte[] Save()
        {
            MemoryStream s = new MemoryStream();
            DataWriter w = new DataWriter(s, Endianess.LittleEndian);

            w.Write("[VERSION]\n" + Version + "\n");
            while (w.Position < 100) w.Write((byte)0);

            WriteString(w, "<fileDates>\n");
            if (FileDates != null)
            {
                foreach (var d in FileDates)
                {
                    WriteString(w, d.ToCacheFileString());
                    WriteString(w, "\n");
                }
            }
            WriteString(w, "</fileDates>\n");
            WriteString(w, "<module>\nfwMapDataStore\n");
            var modlen = (AllMapNodes?.Length ?? 0) * 64;
            //if (AllMapNodes != null)
            //{
            //    foreach (var n in AllMapNodes)
            //    {
            //        if (n.Unk4 == 0xFE)
            //        {
            //            modlen += 96;
            //        }
            //    }
            //}
            w.Write(modlen);
            if (AllMapNodes != null)
            {
                foreach (var n in AllMapNodes)
                {
                    n.Write(w);
                }
            }
            WriteString(w, "</module>\n");
            WriteString(w, "<module>\nCInteriorProxy\n");
            w.Write((AllCInteriorProxies?.Length ?? 0) * 104);
            if (AllCInteriorProxies != null)
            {
                foreach (var p in AllCInteriorProxies)
                {
                    p.Write(w);
                }
            }
            WriteString(w, "</module>\n");
            WriteString(w, "<module>\nBoundsStore\n");
            w.Write((AllBoundsStoreItems?.Length ?? 0) * 32);
            if (AllBoundsStoreItems != null)
            {
                foreach (var b in AllBoundsStoreItems)
                {
                    b.Write(w);
                }
            }
            WriteString(w, "</module>\n");


            var buf = new byte[s.Length];
            s.Position = 0;
            s.Read(buf, 0, buf.Length);
            return buf;
        }

        private void WriteString(DataWriter w, string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                w.Write((byte)s[i]);
            }
        }


        public void WriteXml(StringBuilder sb, int indent)
        {
            CacheDatXml.ValueTag(sb, indent, "Version", Version);
            CacheDatXml.WriteItemArray(sb, FileDates, indent, "FileDates");
            CacheDatXml.WriteItemArray(sb, AllMapNodes, indent, "MapDataStore");
            CacheDatXml.WriteItemArray(sb, AllCInteriorProxies, indent, "InteriorProxies");
            CacheDatXml.WriteItemArray(sb, AllBoundsStoreItems, indent, "BoundsStore");
        }
        public void ReadXml(XmlNode node)
        {
            Version = Xml.GetChildStringAttribute(node, "Version");
            FileDates = XmlMeta.ReadItemArray<CacheFileDate>(node, "FileDates");
            AllMapNodes = XmlMeta.ReadItemArray<MapDataStoreNode>(node, "MapDataStore");
            AllCInteriorProxies = XmlMeta.ReadItemArray<CInteriorProxy>(node, "InteriorProxies");
            AllBoundsStoreItems = XmlMeta.ReadItemArray<BoundsStoreItem>(node, "BoundsStore");
        }



        public string GetXmlOLD()
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
                    sb.AppendLine(string.Format("   <unknowns2 unk11=\"{0}\" unk12=\"{1}\" unk13=\"{2}\" unk14=\"{3}\" />", intprox.Unk11, intprox.Unk12, intprox.Unk13, intprox.Unk14));
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

    [TypeConverter(typeof(ExpandableObjectConverter))] public class CacheFileDate : IMetaXmlItem
    {
        public MetaHash FileName { get; set; } //"resource_surrogate:/%s.rpf"
        public DateTime TimeStamp { get; set; }
        public uint FileID { get; set; }

        public CacheFileDate()
        { }
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

        public void WriteXml(StringBuilder sb, int indent)
        {
            CacheDatXml.StringTag(sb, indent, "fileName", CacheDatXml.HashString(FileName));
            CacheDatXml.ValueTag(sb, indent, "timeStamp", TimeStamp.ToFileTimeUtc().ToString());
            CacheDatXml.ValueTag(sb, indent, "fileID", FileID.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            FileName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "fileName"));
            TimeStamp = DateTime.FromFileTimeUtc((long)Xml.GetChildULongAttribute(node, "timeStamp"));
            FileID = Xml.GetChildUIntAttribute(node, "fileID");
        }

        public override string ToString()
        {
            return FileName.ToString() + ", " + TimeStamp.ToString() + ", " + FileID.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class BoundsStoreItem : IMetaXmlItem
    {
        public MetaHash Name { get; set; }
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }
        public uint Layer { get; set; }

        public BoundsStoreItem()
        { }
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

        public void Write(DataWriter w)
        {
            w.Write(Name);
            w.Write(Min);
            w.Write(Max);
            w.Write(Layer);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            CacheDatXml.StringTag(sb, indent, "name", CacheDatXml.HashString(Name));
            CacheDatXml.SelfClosingTag(sb, indent, "bbMin " + FloatUtil.GetVector3XmlString(Min));
            CacheDatXml.SelfClosingTag(sb, indent, "bbMax " + FloatUtil.GetVector3XmlString(Max));
            CacheDatXml.ValueTag(sb, indent, "layer", Layer.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "name"));
            Min = Xml.GetChildVector3Attributes(node, "bbMin");
            Max = Xml.GetChildVector3Attributes(node, "bbMax");
            Layer = Xml.GetChildUIntAttribute(node, "layer");
        }

        public override string ToString()
        {
            return Name.ToString() + ", " +
                   Min.ToString() + ", " +
                   Max.ToString() + ", " +
                   Layer.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class CInteriorProxy : IMetaXmlItem
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
        public ulong Unk11 { get; set; }//possibly file offsets..?
        public ulong Unk12 { get; set; }//possibly file offsets..?
        public ulong Unk13 { get; set; }//possibly file offsets..?
        public ulong Unk14 { get; set; }//possibly file offsets..?

        public CInteriorProxy()
        { }
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
            Unk11 = br.ReadUInt64();
            Unk12 = br.ReadUInt64();
            Unk13 = br.ReadUInt64();
            Unk14 = br.ReadUInt64(); //(unk14-unk13)=~ 5500000


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
                case 120:
                case 119:
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
                case 43:
                    break;
                default:
                    break;
            }

            switch (Unk11)
            {
                case 5390106048:
                case 50578352072311817:
                case 140699061757388:
                case 738537932908210:
                case 65651138218412653:
                case 19678574933270533:
                case 67944777457713148:
                case 42661052301639680:
                case 64551944402634707:
                case 6851324394453320:
                case 5376209604:
                case 7160325822942775:
                case 140695211737544:
                case 5379565000:
                case 12610731652297560:
                case 0:
                case 5836226042334228:
                    break;
                default:
                    break;
            }
            switch (Unk12)
            {
                case 5394258664:
                case 1788295895209:
                case 2301144275160:
                case 2301144295745:
                case 2301144271926:
                case 2293121037876:
                case 2301144284982:
                case 2301144280511:
                case 2301144292239:
                case 2301144276247:
                case 2297948090320:
                case 2301144281267:
                case 2301144273909:
                case 2301144274429:
                case 2301144278131:
                case 2301144276931:
                case 2301144285912:
                case 2301144279392:
                case 2301144278900:
                case 2301144280746:
                case 2301144276750:
                case 2301144279385:
                case 794564824:
                case 794585409:
                case 3426812900:
                case 3363906997:
                case 2488666884898:
                case 1551090123535:
                case 3581544739:
                case 2697016884:
                case 2697019190:
                case 2697014452:
                case 2697013894:
                case 2697026447:
                case 2697010756:
                case 2697012560:
                case 2697010345:
                case 2697015248:
                case 2697009368:
                case 2697014442:
                case 2697008117:
                case 2697008069:
                case 2697018851:
                case 2697012339:
                case 2697010263:
                case 2697019078:
                case 2697013518:
                case 2697013308:
                case 2697013108:
                case 2079647991056:
                case 2333569996536:
                case 3433367119:
                case 2293344373240:
                case 1527735255327:
                case 1581974815172:
                case 2067312412743:
                case 2240565805648:
                    break;
                default:
                    break;
            }
            switch (Unk13)
            {
                case 5416947112:
                case 140699301996180:
                case 140699066065044:
                case 5381321844:
                case 5385188756:
                case 5385376164:
                case 140696605328676:
                case 140701349594068:
                case 5387902360:
                case 5380079992:
                case 140695959352556:
                case 140695215813968:
                case 5383741744:
                case 140697989613796:
                case 140701810993260:
                case 140701892506988:
                case 140701188531008:
                    break;
                default:
                    break;
            }
            switch (Unk14)
            {
                case 9:
                case 140699307584676:
                case 140699071655044:
                case 5386712196:
                case 5390629684:
                case 5390817140:
                case 140696610778260:
                case 140701355067892:
                case 5393331812:
                case 5385368100:
                case 140695964849172:
                case 140695221335892:
                case 5389196308:
                case 140697995052276:
                case 140701816510228:
                case 140701898076100:
                case 140701194017116:
                    break;
                default:
                    break;
            }

        }

        public void Write(DataWriter w)
        {
            w.Write(Unk01);
            w.Write(Unk02);
            w.Write(Unk03);
            w.Write(Name);
            w.Write(Parent);
            w.Write(Position);
            w.Write(Orientation.ToVector4());
            w.Write(BBMin);
            w.Write(BBMax);
            w.Write(Unk11);
            w.Write(Unk12);
            w.Write(Unk13);
            w.Write(Unk14);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            CacheDatXml.StringTag(sb, indent, "name", CacheDatXml.HashString(Name));
            CacheDatXml.StringTag(sb, indent, "parent", CacheDatXml.HashString(Parent));
            CacheDatXml.SelfClosingTag(sb, indent, "position " + FloatUtil.GetVector3XmlString(Position));
            CacheDatXml.SelfClosingTag(sb, indent, "rotation " + FloatUtil.GetVector4XmlString(Orientation.ToVector4()));
            CacheDatXml.SelfClosingTag(sb, indent, "bbMin " + FloatUtil.GetVector3XmlString(BBMin));
            CacheDatXml.SelfClosingTag(sb, indent, "bbMax " + FloatUtil.GetVector3XmlString(BBMax));
            CacheDatXml.ValueTag(sb, indent, "unk01", Unk01.ToString());
            CacheDatXml.ValueTag(sb, indent, "unk02", Unk02.ToString());
            CacheDatXml.ValueTag(sb, indent, "unk03", Unk03.ToString());
            CacheDatXml.ValueTag(sb, indent, "unk11", Unk11.ToString());
            CacheDatXml.ValueTag(sb, indent, "unk12", Unk12.ToString());
            CacheDatXml.ValueTag(sb, indent, "unk13", Unk13.ToString());
            CacheDatXml.ValueTag(sb, indent, "unk14", Unk14.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "name"));
            Parent = XmlMeta.GetHash(Xml.GetChildInnerText(node, "parent"));
            Position = Xml.GetChildVector3Attributes(node, "position");
            Orientation = Xml.GetChildVector4Attributes(node, "rotation").ToQuaternion();
            BBMin = Xml.GetChildVector3Attributes(node, "bbMin");
            BBMax = Xml.GetChildVector3Attributes(node, "bbMax");
            Unk01 = Xml.GetChildUIntAttribute(node, "unk01");
            Unk02 = Xml.GetChildUIntAttribute(node, "unk02");
            Unk03 = Xml.GetChildUIntAttribute(node, "unk03");
            Unk11 = Xml.GetChildULongAttribute(node, "unk11");
            Unk12 = Xml.GetChildULongAttribute(node, "unk12");
            Unk13 = Xml.GetChildULongAttribute(node, "unk13");
            Unk14 = Xml.GetChildULongAttribute(node, "unk14");
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
                   Unk14.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class MapDataStoreNode : IMetaXmlItem
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

        public MapDataStoreNode()
        { }
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

            //if (Unk4 == 0xFE)
            //{
            //    //this seems to never be hit anymore...
            //    UnkExtra = new MapDataStoreNodeExtra(br);
            //}
        }

        public void Write(DataWriter w)
        {
            w.Write(Name);
            w.Write(ParentName);
            w.Write(ContentFlags);
            w.Write(streamingExtentsMin);
            w.Write(streamingExtentsMax);
            w.Write(entitiesExtentsMin);
            w.Write(entitiesExtentsMax);
            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);

            //if (Unk4 == 0xFE)
            //{
            //    if (UnkExtra == null)
            //    {
            //        UnkExtra = new MapDataStoreNodeExtra();
            //    }
            //    UnkExtra.Write(w);
            //}
        }


        public void WriteXml(StringBuilder sb, int indent)
        {
            CacheDatXml.StringTag(sb, indent, "name", CacheDatXml.HashString(Name));
            CacheDatXml.StringTag(sb, indent, "parent", CacheDatXml.HashString(ParentName));
            CacheDatXml.ValueTag(sb, indent, "contentFlags", ContentFlags.ToString());
            CacheDatXml.SelfClosingTag(sb, indent, "streamingExtentsMin " + FloatUtil.GetVector3XmlString(streamingExtentsMin));
            CacheDatXml.SelfClosingTag(sb, indent, "streamingExtentsMax " + FloatUtil.GetVector3XmlString(streamingExtentsMax));
            CacheDatXml.SelfClosingTag(sb, indent, "entitiesExtentsMin " + FloatUtil.GetVector3XmlString(entitiesExtentsMin));
            CacheDatXml.SelfClosingTag(sb, indent, "entitiesExtentsMax " + FloatUtil.GetVector3XmlString(entitiesExtentsMax));
            CacheDatXml.ValueTag(sb, indent, "unk1", Unk1.ToString());
            CacheDatXml.ValueTag(sb, indent, "unk2", Unk2.ToString());
            CacheDatXml.ValueTag(sb, indent, "unk3", Unk3.ToString());
            CacheDatXml.ValueTag(sb, indent, "unk4", Unk4.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "name"));
            ParentName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "parent"));
            ContentFlags = Xml.GetChildUIntAttribute(node, "contentFlags");
            streamingExtentsMin = Xml.GetChildVector3Attributes(node, "streamingExtentsMin");
            streamingExtentsMax = Xml.GetChildVector3Attributes(node, "streamingExtentsMax");
            entitiesExtentsMin = Xml.GetChildVector3Attributes(node, "entitiesExtentsMin");
            entitiesExtentsMax = Xml.GetChildVector3Attributes(node, "entitiesExtentsMax");
            Unk1 = (byte)Xml.GetChildUIntAttribute(node, "unk1");
            Unk2 = (byte)Xml.GetChildUIntAttribute(node, "unk2");
            Unk3 = (byte)Xml.GetChildUIntAttribute(node, "unk3");
            Unk4 = (byte)Xml.GetChildUIntAttribute(node, "unk4");
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

        public MapDataStoreNodeExtra() 
        { }
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

        public void Write(DataWriter w)
        {
            w.Write(Unk01);
            var alen = Unk02?.Length ?? 0;
            for (int i = 0; i < 60; i++)
            {
                w.Write((i < alen) ? Unk02[i] : (byte)0);
            }
            w.Write(Unk03);
            w.Write(Unk04);
            w.Write(Unk05);
            w.Write(Unk06);
            w.Write(Unk07);
            w.Write(Unk08);
            w.Write(Unk09);
            w.Write(Unk10);
        }

        public override string ToString()
        {
            return Unk01.ToString() + ", " + Unk02str;
        }
    }



    public class CacheDatXml : MetaXmlBase
    {

        public static string GetXml(CacheDatFile cdf)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if (cdf != null)
            {
                var name = "CacheFile";

                OpenTag(sb, 0, name);

                cdf.WriteXml(sb, 1);

                CloseTag(sb, 0, name);
            }

            return sb.ToString();
        }


    }


    public class XmlCacheDat
    {

        public static CacheDatFile GetCacheDat(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetCacheDat(doc);
        }

        public static CacheDatFile GetCacheDat(XmlDocument doc)
        {
            CacheDatFile cdf = new CacheDatFile();
            cdf.ReadXml(doc.DocumentElement);
            return cdf;
        }


    }

}
