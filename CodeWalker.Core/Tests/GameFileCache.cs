using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace CodeWalker.GameFiles
{
    public partial class GameFileCache
    {
        [Conditional("TEST_ALL"), Conditional("TEST_CACHES")]
        public void TestCacheFiles()
        {
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    try
                    {
                        if (entry.Name.EndsWith("cache_y.dat", StringComparison.OrdinalIgnoreCase))// || entry.NameLower.EndsWith("cache_y_bank.dat"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            var cdfile = RpfMan.GetFile<CacheDatFile>(entry);
                            if (cdfile != null)
                            {
                                var odata = entry.File.ExtractFile(entry as RpfFileEntry);
                                //var ndata = cdfile.Save();

                                var xml = CacheDatXml.GetXml(cdfile);
                                var cdf2 = XmlCacheDat.GetCacheDat(xml);
                                var ndata = cdf2.Save();

                                if (ndata.Length == odata.Length)
                                {
                                    for (int i = 0; i < ndata.Length; i++)
                                    {
                                        if (ndata[i] != odata[i])
                                        { break; }
                                    }
                                }
                                else
                                { }
                            }
                            else
                            { }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                    }
                }
            }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_HEIGHTMAPS")]
        public void TestHeightmaps()
        {
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    if (entry.IsExtension(".dat") && entry.Name.StartsWith("heightmap", StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateStatus?.Invoke(string.Format(entry.Path));
                        HeightmapFile hmf = null;
                        hmf = RpfMan.GetFile<HeightmapFile>(entry);
                        var d1 = hmf.RawFileData;
                        //var d2 = hmf.Save();
                        var xml = HmapXml.GetXml(hmf);
                        var hmf2 = XmlHmap.GetHeightmap(xml);
                        var d2 = hmf2.Save();

                        if (d1.Length == d2.Length)
                        {
                            for (int i = 0; i < d1.Length; i++)
                            {
                                if (d1[i] != d2[i])
                                { }
                            }
                        }
                        else
                        { }

                    }
                }
            }
            if (errorfiles.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_WATERMAPS")]
        public void TestWatermaps()
        {
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    if (entry.IsExtension(".dat") && entry.Name.StartsWith("waterheight", StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateStatus?.Invoke(string.Format(entry.Path));
                        WatermapFile wmf = null;
                        wmf = RpfMan.GetFile<WatermapFile>(entry);
                        //var d1 = wmf.RawFileData;
                        //var d2 = wmf.Save();
                        //var xml = WatermapXml.GetXml(wmf);
                        //var wmf2 = XmlWatermap.GetWatermap(xml);
                        //var d2 = wmf2.Save();

                        //if (d1.Length == d2.Length)
                        //{
                        //    for (int i = 0; i < d1.Length; i++)
                        //    {
                        //        if (d1[i] != d2[i])
                        //        { }
                        //    }
                        //}
                        //else
                        //{ }

                    }
                }
            }
            if (errorfiles.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_RELS")]
        public void TestAudioRels()
        {
            UpdateStatus?.Invoke("Testing Audio REL files");


            bool savetest = true;
            bool xmltest = true;
            bool asmtest = true;

            foreach (RpfFile rpf in RpfMan.AllRpfs)
            {
                foreach (RpfEntry entry in rpf.AllEntries)
                {
                    var rfe = entry as RpfFileEntry;
                    var rbfe = rfe as RpfBinaryFileEntry;
                    if ((rfe == null) || (rbfe == null)) continue;

                    if (rfe.IsExtension(".rel"))
                    {
                        UpdateStatus?.Invoke(string.Format(entry.Path));

                        RelFile rel = new RelFile(rfe);
                        RpfMan.LoadFile(rel, rfe);



                        byte[] data;

                        if (savetest)
                        {

                            data = rel.Save();
                            if (data != null)
                            {
                                if (data.Length != rbfe.FileUncompressedSize)
                                { }
                                else if (data.Length != rel.RawFileData.Length)
                                { }
                                else
                                {
                                    for (int i = 0; i < data.Length; i++) //raw file test
                                        if (data[i] != rel.RawFileData[i])
                                        { break; }
                                }


                                RelFile rel2 = new RelFile();
                                rel2.Load(data, rfe);//roundtrip test

                                if (rel2.IndexCount != rel.IndexCount)
                                { }
                                if (rel2.RelDatas == null)
                                { }

                            }
                            else
                            { }

                        }

                        if (xmltest)
                        {
                            var relxml = RelXml.GetXml(rel); //XML test...
                            var rel3 = XmlRel.GetRel(relxml);
                            if (rel3 != null)
                            {
                                data = rel3.Save(); //full roundtrip!
                                if (data != null)
                                {
                                    var rel4 = new RelFile();
                                    rel4.Load(data, rfe); //insanity check

                                    if (data.Length != rbfe.FileUncompressedSize)
                                    { }
                                    else if (data.Length != rel.RawFileData.Length)
                                    { }
                                    else
                                    {
                                        for (int i = 0; i < data.Length; i++) //raw file test
                                            if (data[i] != rel.RawFileData[i])
                                            { break; }
                                    }

                                    var relxml2 = RelXml.GetXml(rel4); //full insanity
                                    if (relxml2.Length != relxml.Length)
                                    { }
                                    if (relxml2 != relxml)
                                    { }

                                }
                                else
                                { }
                            }
                            else
                            { }

                        }

                        if (asmtest)
                        {
                            if (rel.RelType == RelDatFileType.Dat10ModularSynth)
                            {
                                foreach (var d in rel.RelDatasSorted)
                                {
                                    if (d is Dat10Synth synth)
                                    {
                                        synth.TestDisassembly();
                                    }
                                }
                            }
                        }

                    }

                }

            }



            var hashmap = RelFile.HashesMap;
            if (hashmap.Count > 0)
            { }


            var sb2 = new StringBuilder();
            foreach (var kvp in hashmap)
            {
                string itemtype = kvp.Key.ItemType.ToString();
                if (kvp.Key.FileType == RelDatFileType.Dat151)
                {
                    itemtype = ((Dat151RelType)kvp.Key.ItemType).ToString();
                }
                else if (kvp.Key.FileType == RelDatFileType.Dat54DataEntries)
                {
                    itemtype = ((Dat54SoundType)kvp.Key.ItemType).ToString();
                }
                else
                {
                    itemtype = kvp.Key.FileType.ToString() + ".Unk" + kvp.Key.ItemType.ToString();
                }
                if (kvp.Key.IsContainer)
                {
                    itemtype += " (container)";
                }

                //if (kvp.Key.FileType == RelDatFileType.Dat151)
                {
                    sb2.Append(itemtype);
                    sb2.Append("     ");
                    foreach (var val in kvp.Value)
                    {
                        sb2.Append(val.ToString());
                        sb2.Append("   ");
                    }

                    sb2.AppendLine();
                }

            }

            var hashmapstr = sb2.ToString();
            if (!string.IsNullOrEmpty(hashmapstr))
            { }

        }

        [Conditional("TEST_ALL"), Conditional("TEST_YMTS")]
        public void TestAudioYmts()
        {

            StringBuilder sb = new StringBuilder();

            Dictionary<uint, int> allids = new Dictionary<uint, int>();

            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    try
                    {
                        if (entry.IsExtension(".ymt"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            //YmtFile ymtfile = RpfMan.GetFile<YmtFile>(entry);
                            //if ((ymtfile != null))
                            //{
                            //}

                            var sn = entry.ShortName;
                            uint un;
                            if (uint.TryParse(sn, out un))
                            {
                                if (allids.ContainsKey(un))
                                {
                                    allids[un] = allids[un] + 1;
                                }
                                else
                                {
                                    allids[un] = 1;
                                    //ushort s1 = (ushort)(un & 0x1FFFu);
                                    //ushort s2 = (ushort)((un >> 13) & 0x1FFFu);
                                    uint s1 = un % 80000;
                                    uint s2 = (un / 80000);
                                    float f1 = s1 / 5000.0f;
                                    float f2 = s2 / 5000.0f;
                                    sb.AppendFormat("{0}, {1}, 0, {2}\r\n", f1, f2, sn);
                                }
                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                    }
                }
            }

            var skeys = allids.Keys.ToList();
            skeys.Sort();

            var hkeys = new List<string>();
            foreach (var skey in skeys)
            {
                FlagsUint fu = new FlagsUint(skey);
                //hkeys.Add(skey.ToString("X"));
                hkeys.Add(fu.Bin);
            }

            string nstr = string.Join("\r\n", hkeys.ToArray());
            string pstr = sb.ToString();
            if (pstr.Length > 0)
            { }


        }

        [Conditional("TEST_ALL"), Conditional("TEST_AWCS")]
        public void TestAudioAwcs()
        {

            StringBuilder sb = new StringBuilder();

            Dictionary<uint, int> allids = new Dictionary<uint, int>();

            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    //{
                    if (entry.IsExtension(".awc"))
                    {
                        UpdateStatus?.Invoke(string.Format(entry.Path));
                        var awcfile = RpfMan.GetFile<AwcFile>(entry);
                        if (awcfile != null)
                        { }
                    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus?.Invoke("Error! " + ex.ToString());
                    //}
                }
            }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_METAS")]
        public void TestMetas()
        {
            //find all RSC meta files and generate the MetaTypes init code

            MetaTypes.Clear();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        var n = entry.Name;
                        //if (n.EndsWith(".ymap", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    UpdateStatus?.Invoke(string.Format(entry.Path));
                        //    YmapFile ymapfile = RpfMan.GetFile<YmapFile>(entry);
                        //    if ((ymapfile != null) && (ymapfile.Meta != null))
                        //    {
                        //        MetaTypes.EnsureMetaTypes(ymapfile.Meta);
                        //    }
                        //}
                        //else if (n.EndsWith(".ytyp", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    UpdateStatus?.Invoke(string.Format(entry.Path));
                        //    YtypFile ytypfile = RpfMan.GetFile<YtypFile>(entry);
                        //    if ((ytypfile != null) && (ytypfile.Meta != null))
                        //    {
                        //        MetaTypes.EnsureMetaTypes(ytypfile.Meta);
                        //    }
                        //}
                        //else if (n.EndsWith(".ymt", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    UpdateStatus?.Invoke(string.Format(entry.Path));
                        //    YmtFile ymtfile = RpfMan.GetFile<YmtFile>(entry);
                        //    if ((ymtfile != null) && (ymtfile.Meta != null))
                        //    {
                        //        MetaTypes.EnsureMetaTypes(ymtfile.Meta);
                        //    }
                        //}


                        if (n.EndsWith(".ymap", StringComparison.OrdinalIgnoreCase) || n.EndsWith(".ytyp", StringComparison.OrdinalIgnoreCase) || n.EndsWith(".ymt", StringComparison.OrdinalIgnoreCase))
                        {
                            var rfe = entry as RpfResourceFileEntry;
                            if (rfe == null) continue;

                            UpdateStatus?.Invoke(string.Format(entry.Path));

                            var data = rfe.File.ExtractFile(rfe);
                            using var rd = new ResourceDataReader(rfe, data);
                            var meta = rd.ReadBlock<Meta>();
                            var xml = MetaXml.GetXml(meta);
                            var xdoc = new XmlDocument();
                            xdoc.LoadXml(xml);
                            var meta2 = XmlMeta.GetMeta(xdoc);
                            var xml2 = MetaXml.GetXml(meta2);

                            if (xml.Length != xml2.Length)
                            { }
                            if ((xml != xml2) && (!n.EndsWith("srl.ymt", StringComparison.OrdinalIgnoreCase) && !n.StartsWith("des_", StringComparison.OrdinalIgnoreCase)))
                            { }

                        }


                    }
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus?.Invoke("Error! " + ex.ToString());
                    //}
                }
            }

            string str = MetaTypes.GetTypesInitString();

        }

        [Conditional("TEST_ALL"), Conditional("TEST_PSOS")]
        public void TestPsos()
        {
            //find all PSO meta files and generate the PsoTypes init code
            PsoTypes.Clear();

            var exceptions = new List<Exception>();
            var allpsos = new List<string>();
            var diffpsos = new List<string>();

            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
#if !DEBUG
                    try
#endif
                    {
                        var n = entry.Name;
                        if (!(n.EndsWith(".pso", StringComparison.OrdinalIgnoreCase) ||
                              n.EndsWith(".ymt", StringComparison.OrdinalIgnoreCase) ||
                              n.EndsWith(".ymf", StringComparison.OrdinalIgnoreCase) ||
                              n.EndsWith(".ymap", StringComparison.OrdinalIgnoreCase) ||
                              n.EndsWith(".ytyp", StringComparison.OrdinalIgnoreCase) ||
                              n.EndsWith(".cut", StringComparison.OrdinalIgnoreCase)))
                            continue; //PSO files seem to only have these extensions

                        var fentry = entry as RpfFileEntry;
                        var data = entry.File.ExtractFile(fentry);
                        if (data != null)
                        {
                            using (MemoryStream ms = new MemoryStream(data))
                            {
                                if (PsoFile.IsPSO(ms))
                                {
                                    UpdateStatus?.Invoke(string.Format(entry.Path));

                                    var pso = new PsoFile();
                                    pso.Load(ms);

                                    allpsos.Add(fentry.Path);

                                    PsoTypes.EnsurePsoTypes(pso);

                                    var xml = PsoXml.GetXml(pso);
                                    if (!string.IsNullOrEmpty(xml))
                                    { }

                                    var xdoc = new XmlDocument();
                                    xdoc.LoadXml(xml);
                                    var pso2 = XmlPso.GetPso(xdoc);
                                    var pso2b = pso2.Save();

                                    var pso3 = new PsoFile();
                                    pso3.Load(pso2b);
                                    var xml3 = PsoXml.GetXml(pso3);

                                    if (xml.Length != xml3.Length)
                                    { }
                                    if (xml != xml3)
                                    {
                                        diffpsos.Add(fentry.Path);
                                    }


                                    //if (entry.NameLower == "clip_sets.ymt")
                                    //{ }
                                    //if (entry.NameLower == "vfxinteriorinfo.ymt")
                                    //{ }
                                    //if (entry.NameLower == "vfxvehicleinfo.ymt")
                                    //{ }
                                    //if (entry.NameLower == "vfxpedinfo.ymt")
                                    //{ }
                                    //if (entry.NameLower == "vfxregioninfo.ymt")
                                    //{ }
                                    //if (entry.NameLower == "vfxweaponinfo.ymt")
                                    //{ }
                                    //if (entry.NameLower == "physicstasks.ymt")
                                    //{ }

                                }
                            }
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                        exceptions.Add(ex);
                    }
#endif
                }
            }

            string allpsopaths = string.Join("\r\n", allpsos);
            string diffpsopaths = string.Join("\r\n", diffpsos);

            string str = PsoTypes.GetTypesInitString();
            if (!string.IsNullOrEmpty(str))
            {
            }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_RBFS")]
        public void TestRbfs()
        {
            var exceptions = new List<Exception>();
            var allrbfs = new List<string>();
            var diffrbfs = new List<string>();

            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    var n = entry.Name;
                    if (!(n.EndsWith(".ymt", StringComparison.OrdinalIgnoreCase) ||
                          n.EndsWith(".ymf", StringComparison.OrdinalIgnoreCase) ||
                          n.EndsWith(".ymap", StringComparison.OrdinalIgnoreCase) ||
                          n.EndsWith(".ytyp", StringComparison.OrdinalIgnoreCase) ||
                          n.EndsWith(".cut", StringComparison.OrdinalIgnoreCase)))
                        continue; //PSO files seem to only have these extensions

                    var fentry = entry as RpfFileEntry;
                    var data = entry.File.ExtractFile(fentry);
                    if (data != null)
                    {
                        using (MemoryStream ms = new MemoryStream(data))
                        {
                            if (RbfFile.IsRBF(ms))
                            {
                                UpdateStatus?.Invoke(string.Format(entry.Path));

                                var rbf = new RbfFile();
                                rbf.Load(ms);

                                allrbfs.Add(fentry.Path);

                                var xml = RbfXml.GetXml(rbf);
                                if (!string.IsNullOrEmpty(xml))
                                { }

                                var xdoc = new XmlDocument();
                                xdoc.LoadXml(xml);
                                var rbf2 = XmlRbf.GetRbf(xdoc);
                                var rbf2b = rbf2.Save();

                                var rbf3 = new RbfFile();
                                rbf3.Load(rbf2b);
                                var xml3 = RbfXml.GetXml(rbf3);

                                if (xml.Length != xml3.Length)
                                { }
                                if (xml != xml3)
                                {
                                    diffrbfs.Add(fentry.Path);
                                }

                                if (data.Length != rbf2b.Length)
                                {
                                    //File.WriteAllBytes("C:\\GitHub\\CodeWalkerResearch\\RBF\\" + fentry.Name + ".dat0", data);
                                    //File.WriteAllBytes("C:\\GitHub\\CodeWalkerResearch\\RBF\\" + fentry.Name + ".dat1", rbf2b);
                                }
                                else
                                {
                                    for (int i = 0; i < data.Length; i++)
                                    {
                                        if (data[i] != rbf2b[i])
                                        {
                                            diffrbfs.Add(fentry.Path);
                                            break;
                                        }
                                    }
                                }

                            }
                        }
                    }

                }
            }

            string allrbfpaths = string.Join("\r\n", allrbfs);
            string diffrbfpaths = string.Join("\r\n", diffrbfs);

        }

        [Conditional("TEST_ALL"), Conditional("TEST_CUTS")]
        public void TestCuts()
        {

            var exceptions = new List<Exception>();

            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
#if !DEBUG
                    try
#endif
                    {
                        var rfe = entry as RpfFileEntry;
                        if (rfe == null) continue;

                        if (rfe.IsExtension(".cut"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));

                            CutFile cut = new CutFile(rfe);
                            RpfMan.LoadFile(cut, rfe);

                            //PsoTypes.EnsurePsoTypes(cut.Pso);
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                        exceptions.Add(ex);
                    }
#endif
                }
            }

            string str = PsoTypes.GetTypesInitString();
            if (!string.IsNullOrEmpty(str))
            {
            }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YLDS")]
        public void TestYlds()
        {

            var exceptions = new List<Exception>();

            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
#if !DEBUG
                    try
#endif
                    {
                        var rfe = entry as RpfFileEntry;
                        if (rfe == null) continue;

                        if (rfe.IsExtension(".yld"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));

                            YldFile yld = new YldFile(rfe);
                            RpfMan.LoadFile(yld, rfe);

                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                        exceptions.Add(ex);
                    }
#endif
                }
            }

            if (exceptions.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YEDS")]
        public void TestYeds()
        {

            var exceptions = new List<Exception>();

            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
#if !DEBUG
                    try
#endif
                    {
                        var rfe = entry as RpfFileEntry;
                        if (rfe == null) continue;

                        if (rfe.IsExtension(".yed"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));

                            YedFile yed = new YedFile(rfe);
                            RpfMan.LoadFile(yed, rfe);

                            var xml = YedXml.GetXml(yed);
                            var yed2 = XmlYed.GetYed(xml);
                            var data2 = yed2.Save();
                            var yed3 = new YedFile();
                            RpfFile.LoadResourceFile(yed3, data2, 25);//full roundtrip
                            var xml2 = YedXml.GetXml(yed3);
                            if (xml != xml2)
                            { }

                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                        exceptions.Add(ex);
                    }
#endif
                }
            }

            if (exceptions.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YCDS")]
        public void TestYcds()
        {
            bool savetest = false;
            var errorfiles = new List<YcdFile>();
            var errorentries = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    //{
                    if (entry.IsExtension(".ycd"))
                    {
                        UpdateStatus?.Invoke(string.Format(entry.Path));
                        YcdFile ycd1 = RpfMan.GetFile<YcdFile>(entry);
                        if (ycd1 == null)
                        {
                            errorentries.Add(entry);
                        }
                        else if (ycd1?.LoadException != null)
                        {
                            errorfiles.Add(ycd1);//these ones have file corruption issues and won't load as resource...
                        }
                        else if (savetest)
                        {
                            if (ycd1.ClipDictionary == null)
                            { continue; }

                            //var data1 = ycd1.Save();

                            var xml = YcdXml.GetXml(ycd1);
                            var ycdX = XmlYcd.GetYcd(xml);
                            var data = ycdX.Save();
                            var ycd2 = new YcdFile();
                            RpfFile.LoadResourceFile(ycd2, data, 46);//full roundtrip

                            {
                                if (ycd2 == null)
                                { continue; }
                                if (ycd2.ClipDictionary == null)
                                { continue; }

                                var c1 = ycd1.ClipDictionary.Clips?.data_items;
                                var c2 = ycd2.ClipDictionary.Clips?.data_items;
                                if ((c1 == null) || (c2 == null))
                                { continue; }
                                if (c1.Length != c2.Length)
                                { continue; }

                                var a1 = ycd1.ClipDictionary.Animations?.Animations?.data_items;
                                var a2 = ycd2.ClipDictionary.Animations?.Animations?.data_items;
                                if ((a1 == null) || (a2 == null))
                                { continue; }
                                if (a1.Length != a2.Length)
                                { continue; }

                                var m1 = ycd1.AnimMap;
                                var m2 = ycd2.AnimMap;
                                if ((m1 == null) || (m2 == null))
                                { continue; }
                                if (m1.Count != m2.Count)
                                { continue; }
                                foreach (var kvp1 in m1)
                                {
                                    var an1 = kvp1.Value;
                                    var an2 = an1;
                                    if (!m2.TryGetValue(kvp1.Key, out an2))
                                    { continue; }

                                    var sa1 = an1?.Animation?.Sequences?.data_items;
                                    var sa2 = an2?.Animation?.Sequences?.data_items;
                                    if ((sa1 == null) || (sa2 == null))
                                    { continue; }
                                    if (sa1.Length != sa2.Length)
                                    { continue; }
                                    for (int s = 0; s < sa1.Length; s++)
                                    {
                                        var s1 = sa1[s];
                                        var s2 = sa2[s];
                                        if ((s1?.Sequences == null) || (s2?.Sequences == null))
                                        { continue; }

                                        if (s1.NumFrames != s2.NumFrames)
                                        { }
                                        if (s1.ChunkSize != s2.ChunkSize)
                                        { }
                                        if (s1.FrameOffset != s2.FrameOffset)
                                        { }
                                        if (s1.DataLength != s2.DataLength)
                                        { }
                                        else
                                        {
                                            //for (int b = 0; b < s1.DataLength; b++)
                                            //{
                                            //    var b1 = s1.Data[b];
                                            //    var b2 = s2.Data[b];
                                            //    if (b1 != b2)
                                            //    { }
                                            //}
                                        }

                                        for (int ss = 0; ss < s1.Sequences.Length; ss++)
                                        {
                                            var ss1 = s1.Sequences[ss];
                                            var ss2 = s2.Sequences[ss];
                                            if ((ss1?.Channels == null) || (ss2?.Channels == null))
                                            { continue; }
                                            if (ss1.Channels.Length != ss2.Channels.Length)
                                            { continue; }


                                            for (int c = 0; c < ss1.Channels.Length; c++)
                                            {
                                                var sc1 = ss1.Channels[c];
                                                var sc2 = ss2.Channels[c];
                                                if ((sc1 == null) || (sc2 == null))
                                                { continue; }
                                                if (sc1.Type == AnimChannelType.LinearFloat)
                                                { continue; }
                                                if (sc1.Type != sc2.Type)
                                                { continue; }
                                                if (sc1.Index != sc2.Index)
                                                { continue; }
                                                if (sc1.Type == AnimChannelType.StaticQuaternion)
                                                {
                                                    var acsq1 = sc1 as AnimChannelStaticQuaternion;
                                                    var acsq2 = sc2 as AnimChannelStaticQuaternion;
                                                    var vdiff = acsq1.Value - acsq2.Value;
                                                    var len = vdiff.Length();
                                                    var v1len = Math.Max(acsq1.Value.Length(), 1);
                                                    if (len > 1e-2f * v1len)
                                                    { continue; }
                                                }
                                                else if (sc1.Type == AnimChannelType.StaticVector3)
                                                {
                                                    var acsv1 = sc1 as AnimChannelStaticVector3;
                                                    var acsv2 = sc2 as AnimChannelStaticVector3;
                                                    var vdiff = acsv1.Value - acsv2.Value;
                                                    var len = vdiff.Length();
                                                    var v1len = Math.Max(acsv1.Value.Length(), 1);
                                                    if (len > 1e-2f * v1len)
                                                    { continue; }
                                                }
                                                else if (sc1.Type == AnimChannelType.StaticFloat)
                                                {
                                                    var acsf1 = sc1 as AnimChannelStaticFloat;
                                                    var acsf2 = sc2 as AnimChannelStaticFloat;
                                                    var vdiff = Math.Abs(acsf1.Value - acsf2.Value);
                                                    var v1len = Math.Max(Math.Abs(acsf1.Value), 1);
                                                    if (vdiff > 1e-2f * v1len)
                                                    { continue; }
                                                }
                                                else if (sc1.Type == AnimChannelType.RawFloat)
                                                {
                                                    var acrf1 = sc1 as AnimChannelRawFloat;
                                                    var acrf2 = sc2 as AnimChannelRawFloat;
                                                    for (int v = 0; v < acrf1.Values.Length; v++)
                                                    {
                                                        var v1 = acrf1.Values[v];
                                                        var v2 = acrf2.Values[v];
                                                        var vdiff = Math.Abs(v1 - v2);
                                                        var v1len = Math.Max(Math.Abs(v1), 1);
                                                        if (vdiff > 1e-2f * v1len)
                                                        { break; }
                                                    }
                                                }
                                                else if (sc1.Type == AnimChannelType.QuantizeFloat)
                                                {
                                                    var acqf1 = sc1 as AnimChannelQuantizeFloat;
                                                    var acqf2 = sc2 as AnimChannelQuantizeFloat;
                                                    if (acqf1.ValueBits != acqf2.ValueBits)
                                                    { continue; }
                                                    if (Math.Abs(acqf1.Offset - acqf2.Offset) > (0.001f * Math.Abs(acqf1.Offset)))
                                                    { continue; }
                                                    if (Math.Abs(acqf1.Quantum - acqf2.Quantum) > 0.00001f)
                                                    { continue; }
                                                    for (int v = 0; v < acqf1.Values.Length; v++)
                                                    {
                                                        var v1 = acqf1.Values[v];
                                                        var v2 = acqf2.Values[v];
                                                        var vdiff = Math.Abs(v1 - v2);
                                                        var v1len = Math.Max(Math.Abs(v1), 1);
                                                        if (vdiff > 1e-2f * v1len)
                                                        { break; }
                                                    }
                                                }
                                                else if (sc1.Type == AnimChannelType.IndirectQuantizeFloat)
                                                {
                                                    var aciqf1 = sc1 as AnimChannelIndirectQuantizeFloat;
                                                    var aciqf2 = sc2 as AnimChannelIndirectQuantizeFloat;
                                                    if (aciqf1.FrameBits != aciqf2.FrameBits)
                                                    { continue; }
                                                    if (aciqf1.ValueBits != aciqf2.ValueBits)
                                                    { continue; }
                                                    if (Math.Abs(aciqf1.Offset - aciqf2.Offset) > (0.001f * Math.Abs(aciqf1.Offset)))
                                                    { continue; }
                                                    if (Math.Abs(aciqf1.Quantum - aciqf2.Quantum) > 0.00001f)
                                                    { continue; }
                                                    for (int f = 0; f < aciqf1.Frames.Length; f++)
                                                    {
                                                        if (aciqf1.Frames[f] != aciqf2.Frames[f])
                                                        { break; }
                                                    }
                                                    for (int v = 0; v < aciqf1.Values.Length; v++)
                                                    {
                                                        var v1 = aciqf1.Values[v];
                                                        var v2 = aciqf2.Values[v];
                                                        var vdiff = Math.Abs(v1 - v2);
                                                        var v1len = Math.Max(Math.Abs(v1), 1);
                                                        if (vdiff > 1e-2f * v1len)
                                                        { break; }
                                                    }
                                                }
                                                else if ((sc1.Type == AnimChannelType.CachedQuaternion1) || (sc1.Type == AnimChannelType.CachedQuaternion2))
                                                {
                                                    var acrf1 = sc1 as AnimChannelCachedQuaternion;
                                                    var acrf2 = sc2 as AnimChannelCachedQuaternion;
                                                    if (acrf1.QuatIndex != acrf2.QuatIndex)
                                                    { continue; }
                                                }




                                            }


                                            //for (int f = 0; f < s1.NumFrames; f++)
                                            //{
                                            //    var v1 = ss1.EvaluateVector(f);
                                            //    var v2 = ss2.EvaluateVector(f);
                                            //    var vdiff = v1 - v2;
                                            //    var len = vdiff.Length();
                                            //    var v1len = Math.Max(v1.Length(), 1);
                                            //    if (len > 1e-2f*v1len)
                                            //    { }
                                            //}
                                        }


                                    }


                                }


                            }

                        }
                    }
                    //if (entry.NameLower.EndsWith(".awc")) //awcs can also contain clip dicts..
                    //{
                    //    UpdateStatus?.Invoke(string.Format(entry.Path));
                    //    AwcFile awcfile = RpfMan.GetFile<AwcFile>(entry);
                    //    if ((awcfile != null))
                    //    { }
                    //}
                    //}
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus?.Invoke("Error! " + ex.ToString());
                    //}
                }
            }

            if (errorfiles.Count > 0)
            { }

        }

        [Conditional("TEST_ALL"), Conditional("TEST_YTDS")]
        public void TestYtds()
        {
            bool ddstest = false;
            bool savetest = false;
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.IsExtension(".ytd"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            YtdFile ytdfile = null;
                            try
                            {
                                ytdfile = RpfMan.GetFile<YtdFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus?.Invoke("Error! " + ex.ToString());
                                errorfiles.Add(entry);
                            }
                            if (ddstest && (ytdfile != null) && (ytdfile.TextureDict != null))
                            {
                                foreach (var tex in ytdfile.TextureDict.Textures.data_items)
                                {
                                    var dds = Utils.DDSIO.GetDDSFile(tex);
                                    var tex2 = Utils.DDSIO.GetTexture(dds);
                                    if (!tex.Name.StartsWith("script_rt", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (tex.Data?.FullData?.Length != tex2.Data?.FullData?.Length)
                                        { }
                                        if (tex.Stride != tex2.Stride)
                                        { }
                                    }
                                    if ((tex.Format != tex2.Format) || (tex.Width != tex2.Width) || (tex.Height != tex2.Height) || (tex.Depth != tex2.Depth) || (tex.Levels != tex2.Levels))
                                    { }
                                }
                            }
                            if (savetest && (ytdfile != null) && (ytdfile.TextureDict != null))
                            {
                                var fentry = entry as RpfFileEntry;
                                if (fentry == null)
                                { continue; } //shouldn't happen

                                var bytes = ytdfile.Save();

                                string origlen = TextUtil.GetBytesReadable(fentry.FileSize);
                                string bytelen = TextUtil.GetBytesReadable(bytes.Length);

                                if (ytdfile.TextureDict.Textures?.Count == 0)
                                { }


                                var ytd2 = new YtdFile();
                                //ytd2.Load(bytes, fentry);
                                RpfFile.LoadResourceFile(ytd2, bytes, 13);

                                if (ytd2.TextureDict == null)
                                { continue; }
                                if (ytd2.TextureDict.Textures?.Count != ytdfile.TextureDict.Textures?.Count)
                                { continue; }

                                for (int i = 0; i < ytdfile.TextureDict.Textures.Count; i++)
                                {
                                    var tx1 = ytdfile.TextureDict.Textures[i];
                                    var tx2 = ytd2.TextureDict.Textures[i];
                                    var td1 = tx1.Data;
                                    var td2 = tx2.Data;
                                    if (td1.FullData.Length != td2.FullData.Length)
                                    { continue; }

                                    for (int j = 0; j < td1.FullData.Length; j++)
                                    {
                                        if (td1.FullData[j] != td2.FullData[j])
                                        { break; }
                                    }

                                }

                            }
                        }
                    }
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus?.Invoke("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YBNS")]
        public void TestYbns()
        {
            bool xmltest = false;
            bool savetest = false;
            bool reloadtest = false;
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.IsExtension(".ybn"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            YbnFile ybn = null;
                            try
                            {
                                ybn = RpfMan.GetFile<YbnFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus?.Invoke("Error! " + ex.ToString());
                                errorfiles.Add(entry);
                            }
                            if (xmltest && (ybn != null) && (ybn.Bounds != null))
                            {
                                var xml = YbnXml.GetXml(ybn);
                                var ybn2 = XmlYbn.GetYbn(xml);
                                var xml2 = YbnXml.GetXml(ybn2);
                                if (xml.Length != xml2.Length)
                                { }
                            }
                            if (savetest && (ybn != null) && (ybn.Bounds != null))
                            {
                                var fentry = entry as RpfFileEntry;
                                if (fentry == null)
                                { continue; } //shouldn't happen

                                var bytes = ybn.Save();

                                if (!reloadtest)
                                { continue; }

                                string origlen = TextUtil.GetBytesReadable(fentry.FileSize);
                                string bytelen = TextUtil.GetBytesReadable(bytes.Length);


                                var ybn2 = new YbnFile();
                                RpfFile.LoadResourceFile(ybn2, bytes, 43);

                                if (ybn2.Bounds == null)
                                { continue; }
                                if (ybn2.Bounds.Type != ybn.Bounds.Type)
                                { continue; }

                                //quick check of roundtrip
                                switch (ybn2.Bounds.Type)
                                {
                                    case BoundsType.Sphere:
                                        {
                                            var a = ybn.Bounds as BoundSphere;
                                            var b = ybn2.Bounds as BoundSphere;
                                            if (b == null)
                                            { continue; }
                                            break;
                                        }
                                    case BoundsType.Capsule:
                                        {
                                            var a = ybn.Bounds as BoundCapsule;
                                            var b = ybn2.Bounds as BoundCapsule;
                                            if (b == null)
                                            { continue; }
                                            break;
                                        }
                                    case BoundsType.Box:
                                        {
                                            var a = ybn.Bounds as BoundBox;
                                            var b = ybn2.Bounds as BoundBox;
                                            if (b == null)
                                            { continue; }
                                            break;
                                        }
                                    case BoundsType.Geometry:
                                        {
                                            var a = ybn.Bounds as BoundGeometry;
                                            var b = ybn2.Bounds as BoundGeometry;
                                            if (b == null)
                                            { continue; }
                                            if (a.Polygons?.Length != b.Polygons?.Length)
                                            { continue; }
                                            for (int i = 0; i < a.Polygons.Length; i++)
                                            {
                                                var pa = a.Polygons[i];
                                                var pb = b.Polygons[i];
                                                if (pa.Type != pb.Type)
                                                { }
                                            }
                                            break;
                                        }
                                    case BoundsType.GeometryBVH:
                                        {
                                            var a = ybn.Bounds as BoundBVH;
                                            var b = ybn2.Bounds as BoundBVH;
                                            if (b == null)
                                            { continue; }
                                            if (a.BVH?.Nodes?.data_items?.Length != b.BVH?.Nodes?.data_items?.Length)
                                            { }
                                            if (a.Polygons?.Length != b.Polygons?.Length)
                                            { continue; }
                                            for (int i = 0; i < a.Polygons.Length; i++)
                                            {
                                                var pa = a.Polygons[i];
                                                var pb = b.Polygons[i];
                                                if (pa.Type != pb.Type)
                                                { }
                                            }
                                            break;
                                        }
                                    case BoundsType.Composite:
                                        {
                                            var a = ybn.Bounds as BoundComposite;
                                            var b = ybn2.Bounds as BoundComposite;
                                            if (b == null)
                                            { continue; }
                                            if (a.Children?.data_items?.Length != b.Children?.data_items?.Length)
                                            { }
                                            break;
                                        }
                                    case BoundsType.Disc:
                                        {
                                            var a = ybn.Bounds as BoundDisc;
                                            var b = ybn2.Bounds as BoundDisc;
                                            if (b == null)
                                            { continue; }
                                            break;
                                        }
                                    case BoundsType.Cylinder:
                                        {
                                            var a = ybn.Bounds as BoundCylinder;
                                            var b = ybn2.Bounds as BoundCylinder;
                                            if (b == null)
                                            { continue; }
                                            break;
                                        }
                                    case BoundsType.Cloth:
                                        {
                                            var a = ybn.Bounds as BoundCloth;
                                            var b = ybn2.Bounds as BoundCloth;
                                            if (b == null)
                                            { continue; }
                                            break;
                                        }
                                    default: //return null; // throw new Exception("Unknown bound type");
                                        break;
                                }



                            }
                        }
                    }
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus?.Invoke("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YDRS")]
        public void TestYdrs()
        {
            bool savetest = false;
            bool boundsonly = true;
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.IsExtension(".ydr"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            YdrFile ydr = null;
                            try
                            {
                                ydr = RpfMan.GetFile<YdrFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus?.Invoke("Error! " + ex.ToString());
                                errorfiles.Add(entry);
                            }
                            if (savetest && (ydr != null) && (ydr.Drawable != null))
                            {
                                var fentry = entry as RpfFileEntry;
                                if (fentry == null)
                                { continue; } //shouldn't happen

                                if (boundsonly && (ydr.Drawable.Bound == null))
                                { continue; }

                                var bytes = ydr.Save();

                                string origlen = TextUtil.GetBytesReadable(fentry.FileSize);
                                string bytelen = TextUtil.GetBytesReadable(bytes.Length);

                                var ydr2 = new YdrFile();
                                RpfFile.LoadResourceFile(ydr2, bytes, 165);

                                if (ydr2.Drawable == null)
                                { continue; }
                                if (ydr2.Drawable.AllModels?.Length != ydr.Drawable.AllModels?.Length)
                                { continue; }

                            }
                        }
                    }
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus?.Invoke("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count != 13)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YDDS")]
        public void TestYdds()
        {
            bool savetest = false;
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.IsExtension(".ydd"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            YddFile ydd = null;
                            try
                            {
                                ydd = RpfMan.GetFile<YddFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus?.Invoke("Error! " + ex.ToString());
                                errorfiles.Add(entry);
                            }
                            if (savetest && (ydd != null) && (ydd.DrawableDict != null))
                            {
                                var fentry = entry as RpfFileEntry;
                                if (fentry == null)
                                { continue; } //shouldn't happen

                                var bytes = ydd.Save();

                                string origlen = TextUtil.GetBytesReadable(fentry.FileSize);
                                string bytelen = TextUtil.GetBytesReadable(bytes.Length);


                                var ydd2 = new YddFile();
                                RpfFile.LoadResourceFile(ydd2, bytes, 165);

                                if (ydd2.DrawableDict == null)
                                { continue; }
                                if (ydd2.DrawableDict.Drawables?.Count != ydd.DrawableDict.Drawables?.Count)
                                { continue; }

                            }
                            if (ydd?.DrawableDict?.Hashes != null)
                            {
                                uint h = 0;
                                foreach (uint th in ydd.DrawableDict.Hashes)
                                {
                                    if (th <= h)
                                    { } //should never happen
                                    h = th;
                                }
                            }
                        }
                    }
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus?.Invoke("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YFTS")]
        public void TestYfts()
        {
            bool savetest = false;
            var errorfiles = new List<RpfEntry>();
            var sb = new StringBuilder();
            var flagdict = new Dictionary<uint, int>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.IsExtension(".yft"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            YftFile yft = null;
                            try
                            {
                                yft = RpfMan.GetFile<YftFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus?.Invoke("Error! " + ex.ToString());
                                errorfiles.Add(entry);
                            }
                            if (savetest && (yft != null) && (yft.Fragment != null))
                            {
                                var fentry = entry as RpfFileEntry;
                                if (fentry == null)
                                { continue; } //shouldn't happen

                                var bytes = yft.Save();


                                string origlen = TextUtil.GetBytesReadable(fentry.FileSize);
                                string bytelen = TextUtil.GetBytesReadable(bytes.Length);

                                var yft2 = new YftFile();
                                RpfFile.LoadResourceFile(yft2, bytes, 162);

                                if (yft2.Fragment == null)
                                { continue; }
                                if (yft2.Fragment.Drawable?.AllModels?.Length != yft.Fragment.Drawable?.AllModels?.Length)
                                { continue; }

                            }

                            if (yft?.Fragment?.GlassWindows?.data_items != null)
                            {
                                var lastf = -1;
                                for (int i = 0; i < yft.Fragment.GlassWindows.data_items.Length; i++)
                                {
                                    var w = yft.Fragment.GlassWindows.data_items[i];
                                    if (w.Flags == lastf) continue;
                                    lastf = w.Flags;
                                    flagdict.TryGetValue(w.Flags, out int n);
                                    if (n < 10)
                                    {
                                        flagdict[w.Flags] = n + 1;
                                        sb.AppendLine(entry.Path + " Window " + i.ToString() + ": Flags " + w.Flags.ToString() + ", Low:" + w.FlagsLo.ToString() + ", High:" + w.FlagsHi.ToString());
                                    }
                                }
                            }

                        }
                    }
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus?.Invoke("Error! " + ex.ToString());
                    //}
                }
            }
            var teststr = sb.ToString();

            if (errorfiles.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YPTS")]
        public void TestYpts()
        {
            var savetest = false;
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.IsExtension(".ypt"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            YptFile ypt = null;
                            try
                            {
                                ypt = RpfMan.GetFile<YptFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus?.Invoke("Error! " + ex.ToString());
                                errorfiles.Add(entry);
                            }
                            if (savetest && (ypt != null) && (ypt.PtfxList != null))
                            {
                                var fentry = entry as RpfFileEntry;
                                if (fentry == null)
                                { continue; } //shouldn't happen

                                var bytes = ypt.Save();


                                string origlen = TextUtil.GetBytesReadable(fentry.FileSize);
                                string bytelen = TextUtil.GetBytesReadable(bytes.Length);

                                var ypt2 = new YptFile();
                                RpfFile.LoadResourceFile(ypt2, bytes, 68);

                                if (ypt2.PtfxList == null)
                                { continue; }
                                if (ypt2.PtfxList.Name?.Value != ypt.PtfxList.Name?.Value)
                                { continue; }

                            }
                        }
                    }
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus?.Invoke("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YNVS")]
        public void TestYnvs()
        {
            bool xmltest = true;
            var savetest = false;
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.IsExtension(".ynv"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            YnvFile ynv = null;
                            try
                            {
                                ynv = RpfMan.GetFile<YnvFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus?.Invoke("Error! " + ex.ToString());
                                errorfiles.Add(entry);
                            }
                            if (xmltest && (ynv != null) && (ynv.Nav != null))
                            {
                                var xml = YnvXml.GetXml(ynv);
                                if (xml != null)
                                { }
                                var ynv2 = XmlYnv.GetYnv(xml);
                                if (ynv2 != null)
                                { }
                                var ynv2b = ynv2.Save();
                                if (ynv2b != null)
                                { }
                                var ynv3 = new YnvFile();
                                RpfFile.LoadResourceFile(ynv3, ynv2b, 2);
                                var xml3 = YnvXml.GetXml(ynv3);
                                if (xml.Length != xml3.Length)
                                { }
                                var xmllines = xml.Split('\n');
                                var xml3lines = xml3.Split('\n');
                                if (xmllines.Length != xml3lines.Length)
                                { }
                            }
                            if (savetest && (ynv != null) && (ynv.Nav != null))
                            {
                                var fentry = entry as RpfFileEntry;
                                if (fentry == null)
                                { continue; } //shouldn't happen

                                var bytes = ynv.Save();

                                string origlen = TextUtil.GetBytesReadable(fentry.FileSize);
                                string bytelen = TextUtil.GetBytesReadable(bytes.Length);

                                var ynv2 = new YnvFile();
                                RpfFile.LoadResourceFile(ynv2, bytes, 2);

                                if (ynv2.Nav == null)
                                { continue; }

                            }
                        }
                    }
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus?.Invoke("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YVRS")]
        public void TestYvrs()
        {

            var exceptions = new List<Exception>();

            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
#if !DEBUG
                    try
#endif
                    {
                        var rfe = entry as RpfFileEntry;
                        if (rfe == null) continue;

                        if (rfe.IsExtension(".yvr"))
                        {
                            if (rfe.Name.Equals("agencyprep001.yvr", StringComparison.OrdinalIgnoreCase)) continue; //this file seems corrupted

                            UpdateStatus?.Invoke(string.Format(entry.Path));

                            YvrFile yvr = new YvrFile(rfe);
                            RpfMan.LoadFile(yvr, rfe);

                            var xml = YvrXml.GetXml(yvr);
                            var yvr2 = XmlYvr.GetYvr(xml);
                            var data2 = yvr2.Save();
                            var yvr3 = new YvrFile();
                            RpfFile.LoadResourceFile(yvr3, data2, 1);//full roundtrip
                            var xml2 = YvrXml.GetXml(yvr3);
                            if (xml != xml2)
                            { }

                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                        exceptions.Add(ex);
                    }
#endif
                }
            }

            if (exceptions.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YWRS")]
        public void TestYwrs()
        {

            var exceptions = new List<Exception>();

            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
#if !DEBUG
                    try
#endif
                    {
                        if (entry is not RpfFileEntry rfe || rfe == null) continue;

                        if (rfe.IsExtension(".ywr"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));

                            YwrFile ywr = new YwrFile(rfe);
                            RpfMan.LoadFile(ywr, rfe);

                            var xml = YwrXml.GetXml(ywr);
                            var ywr2 = XmlYwr.GetYwr(xml);
                            var data2 = ywr2.Save();
                            var ywr3 = new YwrFile();
                            RpfFile.LoadResourceFile(ywr3, data2, 1);//full roundtrip
                            var xml2 = YwrXml.GetXml(ywr3);
                            if (xml != xml2)
                            { }

                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                        exceptions.Add(ex);
                    }
#endif
                }
            }

            if (exceptions.Count > 0)
            { }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YMAPS")]
        public void TestYmaps()
        {
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    try
                    {
                        if (entry.IsExtension(".ymap"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            YmapFile ymapfile = RpfMan.GetFile<YmapFile>(entry);
                            if ((ymapfile != null))// && (ymapfile.Meta != null))
                            { }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                    }
                }
            }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YPDBS")]
        public void TestYpdbs()
        {
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    var rfe = entry as RpfFileEntry;
                    if (rfe == null) continue;

                    try
                    {
                        if (rfe.IsExtension(".ypdb"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            YpdbFile ypdb = RpfMan.GetFile<YpdbFile>(entry);
                            if (ypdb != null)
                            {
                                var odata = entry.File.ExtractFile(entry as RpfFileEntry);
                                //var ndata = ypdb.Save();

                                var xml = YpdbXml.GetXml(ypdb);
                                var ypdb2 = XmlYpdb.GetYpdb(xml);
                                var ndata = ypdb2.Save();

                                if (ndata.Length == odata.Length)
                                {
                                    for (int i = 0; i < ndata.Length; i++)
                                    {
                                        if (ndata[i] != odata[i])
                                        { break; }
                                    }
                                }
                                else
                                { }
                            }
                            else
                            { }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                    }

                }
            }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_YFDS")]
        public void TestYfds()
        {
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    var rfe = entry as RpfFileEntry;
                    if (rfe == null) continue;

                    try
                    {
                        if (rfe.IsExtension(".yfd"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            YfdFile yfd = RpfMan.GetFile<YfdFile>(entry);
                            if (yfd != null)
                            {
                                if (yfd.FrameFilterDictionary != null)
                                {
                                    // check that all signatures can be re-calculated
                                    foreach (var f in yfd.FrameFilterDictionary.Filters.data_items)
                                    {
                                        if (f.Signature != f.CalculateSignature())
                                        { }
                                    }
                                }

                                var xml = YfdXml.GetXml(yfd);
                                var yfd2 = XmlYfd.GetYfd(xml);
                                var data2 = yfd2.Save();
                                var yfd3 = new YfdFile();
                                RpfFile.LoadResourceFile(yfd3, data2, 4);//full roundtrip
                                var xml2 = YfdXml.GetXml(yfd3);
                                if (xml != xml2)
                                { }
                            }
                            else
                            { }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                    }

                }
            }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_MRFS")]
        public void TestMrfs()
        {
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    try
                    {
                        if (entry.IsExtension(".mrf"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            MrfFile mrffile = RpfMan.GetFile<MrfFile>(entry);
                            if (mrffile != null)
                            {
                                var odata = entry.File.ExtractFile(entry as RpfFileEntry);
                                var ndata = mrffile.Save();
                                if (ndata.Length == odata.Length)
                                {
                                    for (int i = 0; i < ndata.Length; i++)
                                    {
                                        if (ndata[i] != odata[i])
                                        { break; }
                                    }
                                }
                                else
                                { }

                                var xml = MrfXml.GetXml(mrffile);
                                var mrf2 = XmlMrf.GetMrf(xml);
                                var ndata2 = mrf2.Save();
                                if (ndata2.Length == odata.Length)
                                {
                                    for (int i = 0; i < ndata2.Length; i++)
                                    {
                                        if (ndata2[i] != odata[i] && !mrfDiffCanBeIgnored(i, mrffile))
                                        { break; }
                                    }
                                }
                                else
                                { }

                                bool mrfDiffCanBeIgnored(int fileOffset, MrfFile originalMrf)
                                {
                                    foreach (var n in originalMrf.AllNodes)
                                    {
                                        if (n is MrfNodeStateBase state)
                                        {
                                            // If TransitionCount is 0, the TransitionsOffset value can be ignored.
                                            // TransitionsOffset in original MRFs isn't always set to 0 in this case,
                                            // XML-imported MRFs always set it to 0
                                            if (state.TransitionCount == 0 && fileOffset == (state.FileOffset + 0x1C))
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }
                            }
                            else
                            { }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                    }
                }
            }

            // create and save a custom MRF
            {
                // Usage example:
                //  RequestAnimDict("move_m@alien")
                //  TaskMoveNetworkByName(PlayerPedId(), "mymrf", 0.0, true, 0, 0)
                //  SetTaskMoveNetworkSignalFloat(PlayerPedId(), "sprintrate", 2.0)
                var mymrf = new MrfFile();
                var clip1 = new MrfNodeClip
                {
                    NodeIndex = 0,
                    Name = JenkHash.GenHash("clip1"),
                    ClipType = MrfValueType.Literal,
                    ClipContainerType = MrfClipContainerType.ClipDictionary,
                    ClipContainerName = JenkHash.GenHash("move_m@alien"),
                    ClipName = JenkHash.GenHash("alien_run"),
                    LoopedType = MrfValueType.Literal,
                    Looped = true,
                };
                var clip2 = new MrfNodeClip
                {
                    NodeIndex = 0,
                    Name = JenkHash.GenHash("clip2"),
                    ClipType = MrfValueType.Literal,
                    ClipContainerType = MrfClipContainerType.ClipDictionary,
                    ClipContainerName = JenkHash.GenHash("move_m@alien"),
                    ClipName = JenkHash.GenHash("alien_sprint"),
                    LoopedType = MrfValueType.Literal,
                    Looped = true,
                    RateType = MrfValueType.Parameter,
                    RateParameterName = JenkHash.GenHash("sprintrate"),
                };
                var clipstate1 = new MrfNodeState
                {
                    NodeIndex = 0,
                    Name = JenkHash.GenHash("clipstate1"),
                    InitialNode = clip1,
                    Transitions = new[]
                    {
                        new MrfStateTransition
                        {
                            Duration = 2.5f,
                            HasDurationParameter = false,
                            //TargetState = clipstate2,
                            Conditions = new[]
                            {
                                new MrfConditionTimeGreaterThan { Value = 4.0f },
                            },
                        }
                    },
                };
                var clipstate2 = new MrfNodeState
                {
                    NodeIndex = 1,
                    Name = JenkHash.GenHash("clipstate2"),
                    InitialNode = clip2,
                    Transitions = new[]
                    {
                        new MrfStateTransition
                        {
                            Duration = 2.5f,
                            HasDurationParameter = false,
                            //TargetState = clipstate1,
                            Conditions = new[]
                            {
                                new MrfConditionTimeGreaterThan { Value = 4.0f },
                            },
                }
                    },
                };
                clipstate1.Transitions[0].TargetState = clipstate2;
                clipstate2.Transitions[0].TargetState = clipstate1;
                var rootsm = new MrfNodeStateMachine
                {
                    NodeIndex = 0,
                    Name = JenkHash.GenHash("statemachine"),
                    States = new[]
                    {
                        new MrfStateRef { StateName = clipstate1.Name, State = clipstate1 },
                        new MrfStateRef { StateName = clipstate2.Name, State = clipstate2 },
                    },
                    InitialNode = clipstate1,
                };
                mymrf.AllNodes = new MrfNode[]
                {
                    rootsm,
                    clipstate1,
                    clip1,
                    clipstate2,
                    clip2,
                };
                mymrf.RootState = rootsm;

                var mymrfData = mymrf.Save();
                //File.WriteAllBytes("mymrf.mrf", mymrfData);
                //File.WriteAllText("mymrf.dot", mymrf.DumpStateGraph());
            }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_FXCS")]
        public void TestFxcs()
        {
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    try
                    {
                        if (entry.IsExtension(".fxc"))
                        {
                            UpdateStatus?.Invoke(string.Format(entry.Path));
                            var fxcfile = RpfMan.GetFile<FxcFile>(entry);
                            if (fxcfile != null)
                            {
                                var odata = entry.File.ExtractFile(entry as RpfFileEntry);
                                var ndata = fxcfile.Save();
                                if (ndata.Length == odata.Length)
                                {
                                    for (int i = 0; i < ndata.Length; i++)
                                    {
                                        if (ndata[i] != odata[i])
                                        { break; }
                                    }
                                }
                                else
                                { }

                                var xml1 = FxcXml.GetXml(fxcfile);//won't output bytecodes with no output folder
                                var fxc1 = XmlFxc.GetFxc(xml1);
                                var xml2 = FxcXml.GetXml(fxc1);
                                if (xml1 != xml2)
                                { }


                                for (int i = 0; i < fxcfile.Shaders.Length; i++)
                                {
                                    if (fxc1.Shaders[i].Name != fxcfile.Shaders[i].Name)
                                    { }
                                    fxc1.Shaders[i].ByteCode = fxcfile.Shaders[i].ByteCode;
                                }

                                var xdata = fxc1.Save();
                                if (xdata.Length == odata.Length)
                                {
                                    for (int i = 0; i < xdata.Length; i++)
                                    {
                                        if (xdata[i] != odata[i])
                                        { break; }
                                    }
                                }
                                else
                                { }


                            }
                            else
                            { }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus?.Invoke("Error! " + ex.ToString());
                    }
                }
            }
        }

        [Conditional("TEST_ALL"), Conditional("TEST_PLACEMENTS")]
        public void TestPlacements()
        {
            //int totplacements = 0;
            //int tottimedplacements = 0;
            //int totaudioplacements = 0;
            //StringBuilder sbtest = new StringBuilder();
            //StringBuilder sbterr = new StringBuilder();
            //sbtest.AppendLine("X, Y, Z, name, assetName, drawableDictionary, textureDictionary, ymap");
            //foreach (RpfFile file in RpfMan.AllRpfs)
            //{
            //    foreach (RpfEntry entry in file.AllEntries)
            //    {
            //        try
            //        {
            //            if (entry.NameLower.EndsWith(".ymap"))
            //            {
            //                UpdateStatus?.Invoke(string.Format(entry.Path));
            //                YmapFile ymapfile = RpfMan.GetFile<YmapFile>(entry);
            //                if ((ymapfile != null))// && (ymapfile.Meta != null))
            //                {
            //                    //if (ymapfile.CMapData.parent == 0) //root ymap output
            //                    //{
            //                    //    sbtest.AppendLine(JenkIndex.GetString(ymapfile.CMapData.name) + ": " + entry.Path);
            //                    //}
            //                    if (ymapfile.CEntityDefs != null)
            //                    {
            //                        for (int n = 0; n < ymapfile.CEntityDefs.Length; n++)
            //                        {
            //                            //find ytyp...
            //                            var entdef = ymapfile.CEntityDefs[n];
            //                            var pos = entdef.position;
            //                            bool istimed = false;
            //                            Tuple<YtypFile, int> archetyp;
            //                            if (!BaseArchetypes.TryGetValue(entdef.archetypeName, out archetyp))
            //                            {
            //                                sbterr.AppendLine("Couldn't find ytyp for " + entdef.ToString());
            //                            }
            //                            else
            //                            {
            //                                int ymapbasecount = (archetyp.Item1.CBaseArchetypeDefs != null) ? archetyp.Item1.CBaseArchetypeDefs.Length : 0;
            //                                int baseoffset = archetyp.Item2 - ymapbasecount;
            //                                if (baseoffset >= 0)
            //                                {
            //                                    if ((archetyp.Item1.CTimeArchetypeDefs == null) || (baseoffset > archetyp.Item1.CTimeArchetypeDefs.Length))
            //                                    {
            //                                        sbterr.AppendLine("Couldn't lookup CTimeArchetypeDef... " + archetyp.ToString());
            //                                        continue;
            //                                    }

            //                                    istimed = true;

            //                                    //it's a CTimeArchetypeDef...
            //                                    CTimeArchetypeDef ctad = archetyp.Item1.CTimeArchetypeDefs[baseoffset];

            //                                    //if (ctad.ToString().Contains("spider"))
            //                                    //{
            //                                    //}
            //                                    //sbtest.AppendFormat("{0}, {1}, {2}, {3}, {4}", pos.X, pos.Y, pos.Z, ctad.ToString(), entry.Name);
            //                                    //sbtest.AppendLine();

            //                                    tottimedplacements++;
            //                                }
            //                                totplacements++;
            //                            }

            //                            Tuple<YtypFile, int> audiotyp;
            //                            if (AudioArchetypes.TryGetValue(entdef.archetypeName, out audiotyp))
            //                            {
            //                                if (istimed)
            //                                {
            //                                }
            //                                if (!BaseArchetypes.TryGetValue(entdef.archetypeName, out archetyp))
            //                                {
            //                                    sbterr.AppendLine("Couldn't find ytyp for " + entdef.ToString());
            //                                }
            //                                if (audiotyp.Item1 != archetyp.Item1)
            //                                {
            //                                }

            //                                CBaseArchetypeDef cbad = archetyp.Item1.CBaseArchetypeDefs[archetyp.Item2];
            //                                CExtensionDefAudioEmitter emitr = audiotyp.Item1.AudioEmitters[audiotyp.Item2];

            //                                if (emitr.name != cbad.name)
            //                                {
            //                                }

            //                                string hashtest = JenkIndex.GetString(emitr.effectHash);

            //                                sbtest.AppendFormat("{0}, {1}, {2}, {3}, {4}, {5}", pos.X, pos.Y, pos.Z, cbad.ToString(), entry.Name, hashtest);
            //                                sbtest.AppendLine();

            //                                totaudioplacements++;
            //                            }

            //                        }
            //                    }

            //                    //if (ymapfile.TimeCycleModifiers != null)
            //                    //{
            //                    //    for (int n = 0; n < ymapfile.TimeCycleModifiers.Length; n++)
            //                    //    {
            //                    //        var tcmod = ymapfile.TimeCycleModifiers[n];
            //                    //        Tuple<YtypFile, int> archetyp;
            //                    //        if (BaseArchetypes.TryGetValue(tcmod.name, out archetyp))
            //                    //        {
            //                    //        }
            //                    //        else
            //                    //        {
            //                    //        }
            //                    //    }
            //                    //}
            //                }
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            sbterr.AppendLine(entry.Path + ": " + ex.ToString());
            //        }
            //    }
            //}

            //UpdateStatus?.Invoke("Ymap scan finished.");

            //sbtest.AppendLine();
            //sbtest.AppendLine(totplacements.ToString() + " total CEntityDef placements parsed");
            //sbtest.AppendLine(tottimedplacements.ToString() + " total CTimeArchetypeDef placements");
            //sbtest.AppendLine(totaudioplacements.ToString() + " total CExtensionDefAudioEmitter placements");

            //string teststr = sbtest.ToString();
            //string testerr = sbterr.ToString();

            //return;
        }

        [Conditional("RUN_TESTS"), Conditional("TEST_DRAWABLES")]
        public void TestDrawables()
        {


            DateTime starttime = DateTime.Now;

            bool doydr = false;
            bool doydd = false;
            bool doyft = true;

            List<string> errs = new List<string>();
            Dictionary<ulong, VertexDeclaration> vdecls = new Dictionary<ulong, VertexDeclaration>();
            Dictionary<ulong, int> vdecluse = new Dictionary<ulong, int>();
            int drawablecount = 0;
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    try
                    {
                        if (doydr && entry.IsExtension(".ydr"))
                        {
                            UpdateStatus?.Invoke(entry.Path);
                            YdrFile ydr = RpfMan.GetFile<YdrFile>(entry);

                            if (ydr == null)
                            {
                                errs.Add(entry.Path + ": Couldn't read file");
                                continue;
                            }
                            if (ydr.Drawable == null)
                            {
                                errs.Add(entry.Path + ": Couldn't read drawable data");
                                continue;
                            }
                            drawablecount++;
                            foreach (var kvp in ydr.Drawable.VertexDecls)
                            {
                                if (!vdecls.ContainsKey(kvp.Key))
                                {
                                    vdecls.Add(kvp.Key, kvp.Value);
                                    vdecluse.Add(kvp.Key, 1);
                                }
                                else
                                {
                                    vdecluse[kvp.Key]++;
                                }
                            }
                        }
                        else if (doydd & entry.IsExtension(".ydd"))
                        {
                            UpdateStatus?.Invoke(entry.Path);
                            YddFile ydd = RpfMan.GetFile<YddFile>(entry);

                            if (ydd == null)
                            {
                                errs.Add(entry.Path + ": Couldn't read file");
                                continue;
                            }
                            if (ydd.Dict == null)
                            {
                                errs.Add(entry.Path + ": Couldn't read drawable dictionary data");
                                continue;
                            }
                            foreach (var drawable in ydd.Dict.Values)
                            {
                                drawablecount++;
                                foreach (var kvp in drawable.VertexDecls)
                                {
                                    if (!vdecls.ContainsKey(kvp.Key))
                                    {
                                        vdecls.Add(kvp.Key, kvp.Value);
                                        vdecluse.Add(kvp.Key, 1);
                                    }
                                    else
                                    {
                                        vdecluse[kvp.Key]++;
                                    }
                                }
                            }
                        }
                        else if (doyft && entry.IsExtension(".yft"))
                        {
                            UpdateStatus?.Invoke(entry.Path);
                            YftFile yft = RpfMan.GetFile<YftFile>(entry);

                            if (yft == null)
                            {
                                errs.Add(entry.Path + ": Couldn't read file");
                                continue;
                            }
                            if (yft.Fragment == null)
                            {
                                errs.Add(entry.Path + ": Couldn't read fragment data");
                                continue;
                            }
                            if (yft.Fragment.Drawable != null)
                            {
                                drawablecount++;
                                foreach (var kvp in yft.Fragment.Drawable.VertexDecls)
                                {
                                    if (!vdecls.ContainsKey(kvp.Key))
                                    {
                                        vdecls.Add(kvp.Key, kvp.Value);
                                        vdecluse.Add(kvp.Key, 1);
                                    }
                                    else
                                    {
                                        vdecluse[kvp.Key]++;
                                    }
                                }
                            }
                            if ((yft.Fragment.Cloths != null) && (yft.Fragment.Cloths.data_items != null))
                            {
                                foreach (var cloth in yft.Fragment.Cloths.data_items)
                                {
                                    drawablecount++;
                                    foreach (var kvp in cloth.Drawable.VertexDecls)
                                    {
                                        if (!vdecls.ContainsKey(kvp.Key))
                                        {
                                            vdecls.Add(kvp.Key, kvp.Value);
                                            vdecluse.Add(kvp.Key, 1);
                                        }
                                        else
                                        {
                                            vdecluse[kvp.Key]++;
                                        }
                                    }
                                }
                            }
                            if ((yft.Fragment.DrawableArray != null) && (yft.Fragment.DrawableArray.data_items != null))
                            {
                                foreach (var drawable in yft.Fragment.DrawableArray.data_items)
                                {
                                    drawablecount++;
                                    foreach (var kvp in drawable.VertexDecls)
                                    {
                                        if (!vdecls.ContainsKey(kvp.Key))
                                        {
                                            vdecls.Add(kvp.Key, kvp.Value);
                                            vdecluse.Add(kvp.Key, 1);
                                        }
                                        else
                                        {
                                            vdecluse[kvp.Key]++;
                                        }
                                    }
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        errs.Add(entry.Path + ": " + ex.ToString());
                    }
                }
            }


            string errstr = string.Join("\r\n", errs);



            //build vertex types code string
            errs.Clear();
            StringBuilder sbverts = new StringBuilder();
            foreach (var kvp in vdecls)
            {
                var vd = kvp.Value;
                int usage = vdecluse[kvp.Key];
                sbverts.AppendFormat("public struct VertexType{0} //id: {1}, stride: {2}, flags: {3}, types: {4}, refs: {5}", vd.Flags, kvp.Key, vd.Stride, vd.Flags, vd.Types, usage);
                sbverts.AppendLine();
                sbverts.AppendLine("{");
                uint compid = 1;
                for (int i = 0; i < 16; i++)
                {
                    if (((vd.Flags >> i) & 1) == 1)
                    {
                        string typestr = "Unknown";
                        uint type = (uint)(((ulong)vd.Types >> (4 * i)) & 0xF);
                        switch (type)
                        {
                            case 0: typestr = "ushort"; break;// Data[i] = new ushort[1 * count]; break;
                            case 1: typestr = "ushort2"; break;// Data[i] = new ushort[2 * count]; break;
                            case 2: typestr = "ushort3"; break;// Data[i] = new ushort[3 * count]; break;
                            case 3: typestr = "ushort4"; break;// Data[i] = new ushort[4 * count]; break;
                            case 4: typestr = "float"; break;// Data[i] = new float[1 * count]; break;
                            case 5: typestr = "Vector2"; break;// Data[i] = new float[2 * count]; break;
                            case 6: typestr = "Vector3"; break;// Data[i] = new float[3 * count]; break;
                            case 7: typestr = "Vector4"; break;// Data[i] = new float[4 * count]; break;
                            case 8: typestr = "uint"; break;// Data[i] = new uint[count]; break;
                            case 9: typestr = "uint"; break;// Data[i] = new uint[count]; break;
                            case 10: typestr = "uint"; break;// Data[i] = new uint[count]; break;
                            default:
                                break;
                        }
                        sbverts.AppendLine("   public " + typestr + " Component" + compid.ToString() + ";");
                        compid++;
                    }

                }
                sbverts.AppendLine("}");
                sbverts.AppendLine();
            }

            string vertstr = sbverts.ToString();
            string verrstr = string.Join("\r\n", errs);

            UpdateStatus?.Invoke((DateTime.Now - starttime).ToString() + " elapsed, " + drawablecount.ToString() + " drawables, " + errs.Count.ToString() + " errors.");

        }
    }
}
