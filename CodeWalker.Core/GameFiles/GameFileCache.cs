﻿using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    public class GameFileCache
    {
        public RpfManager RpfMan;
        private Action<string> UpdateStatus;
        private Action<string> ErrorLog;
        public int MaxItemsPerLoop = 1; //to keep things flowing...

        private ConcurrentQueue<GameFile> requestQueue = new ConcurrentQueue<GameFile>();

        ////dynamic cache
        private Cache<GameFileCacheKey, GameFile> mainCache;
        public volatile bool IsInited = false;

        private volatile bool archetypesLoaded = false;
        private Dictionary<uint, Archetype> archetypeDict = new Dictionary<uint, Archetype>();
        private Dictionary<uint, RpfFileEntry> textureLookup = new Dictionary<uint, RpfFileEntry>();
        private Dictionary<MetaHash, MetaHash> textureParents;
        private Dictionary<MetaHash, MetaHash> hdtexturelookup;

        private object updateSyncRoot = new object();
        private object requestSyncRoot = new object();
        private object textureSyncRoot = new object(); //for the texture lookup.






        //static indexes
        public Dictionary<uint, RpfFileEntry> YdrDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YddDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YtdDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YmapDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YftDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YbnDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YcdDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YnvDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> Gxt2Dict { get; private set; }


        public Dictionary<uint, RpfFileEntry> AllYmapsDict { get; private set; }


        //static cached data loaded at init
        public Dictionary<uint, YtypFile> YtypDict { get; set; }

        public List<CacheDatFile> AllCacheFiles { get; set; }
        public Dictionary<uint, MapDataStoreNode> YmapHierarchyDict { get; set; }

        public List<YmfFile> AllManifests { get; set; }


        public bool EnableDlc { get; set; } = false;//true;//
        public bool EnableMods { get; set; } = false;

        public List<string> DlcPaths { get; set; } = new List<string>();
        public List<RpfFile> DlcActiveRpfs { get; set; } = new List<RpfFile>();
        public List<DlcSetupFile> DlcSetupFiles { get; set; } = new List<DlcSetupFile>();
        public List<DlcExtraFolderMountFile> DlcExtraFolderMounts { get; set; } = new List<DlcExtraFolderMountFile>();
        public Dictionary<string, string> DlcPatchedPaths { get; set; } = new Dictionary<string, string>();
        public List<string> DlcCacheFileList { get; set; } = new List<string>();
        public List<string> DlcNameList { get; set; } = new List<string>();
        public string SelectedDlc { get; set; } = string.Empty;

        public Dictionary<string, RpfFile> ActiveMapRpfFiles { get; set; } = new Dictionary<string, RpfFile>();

        public Dictionary<uint, World.TimecycleMod> TimeCycleModsDict = new Dictionary<uint, World.TimecycleMod>();

        public Dictionary<MetaHash, VehicleInitData> VehiclesInitDict { get; set; }
        public Dictionary<MetaHash, CPedModelInfo__InitData> PedsInitDict { get; set; }
        public Dictionary<MetaHash, PedFile> PedVariationsDict { get; set; }
        public Dictionary<MetaHash, Dictionary<MetaHash, RpfFileEntry>> PedDrawableDicts { get; set; }
        public Dictionary<MetaHash, Dictionary<MetaHash, RpfFileEntry>> PedTextureDicts { get; set; }
        public Dictionary<MetaHash, Dictionary<MetaHash, RpfFileEntry>> PedClothDicts { get; set; }

        public List<RpfFile> BaseRpfs { get; private set; }
        public List<RpfFile> AllRpfs { get; private set; }
        public List<RpfFile> DlcRpfs { get; private set; }

        public bool DoFullStringIndex = false;
        public bool BuildExtendedJenkIndex = true;
        public bool LoadArchetypes = true;
        public bool LoadVehicles = true;
        public bool LoadPeds = true;
        private bool PreloadedMode = false;

        private string GTAFolder;
        private string ExcludeFolders;



        public int QueueLength
        {
            get
            {
                return requestQueue.Count;
            }
        }
        public int ItemCount
        {
            get
            {
                return mainCache.Count;
            }
        }
        public long MemoryUsage
        {
            get
            {
                return mainCache.CurrentMemoryUsage;
            }
        }



        public GameFileCache(long size, double cacheTime, string folder, string dlc, bool mods, string excludeFolders)
        {
            mainCache = new Cache<GameFileCacheKey, GameFile>(size, cacheTime);//2GB is good as default
            SelectedDlc = dlc;
            EnableDlc = !string.IsNullOrEmpty(SelectedDlc);
            EnableMods = mods;
            GTAFolder = folder;
            ExcludeFolders = excludeFolders;
        }


        public void Clear()
        {
            IsInited = false;

            mainCache.Clear();

            textureLookup.Clear();

            GameFile queueclear;
            while (requestQueue.TryDequeue(out queueclear))
            { } //empty the old queue out...
        }

        public void Init(Action<string> updateStatus, Action<string> errorLog)
        {
            UpdateStatus = updateStatus;
            ErrorLog = errorLog;

            Clear();


            if (RpfMan == null)
            {
                //EnableDlc = !string.IsNullOrEmpty(SelectedDlc);



                RpfMan = new RpfManager();
                RpfMan.ExcludePaths = GetExcludePaths();
                RpfMan.EnableMods = EnableMods;
                RpfMan.BuildExtendedJenkIndex = BuildExtendedJenkIndex;
                RpfMan.Init(GTAFolder, UpdateStatus, ErrorLog);//, true);


                InitGlobal();

                InitDlc();



                //RE test area!
                //TestAudioRels();
                //TestAudioYmts();
                //TestMetas();
                //TestPsos();
                //TestRbfs();
                //TestCuts();
                //TestYlds();
                //TestYcds();
                //TestYtds();
                //TestYbns();
                //TestYdrs();
                //TestYdds();
                //TestYfts();
                //TestYpts();
                //TestYmaps();
                //TestPlacements();
                //TestDrawables();
                //GetArchetypeTimesList();
                //string typestr = PsoTypes.GetTypesString();
            }
            else
            {
                GC.Collect(); //try free up some of the previously used memory..
            }

            UpdateStatus("Scan complete");


            IsInited = true;
        }
        public void Init(Action<string> updateStatus, Action<string> errorLog, List<RpfFile> allRpfs)
        {
            UpdateStatus = updateStatus;
            ErrorLog = errorLog;

            Clear();

            PreloadedMode = true;
            EnableDlc = true;//just so everything (mainly archetypes) will load..
            EnableMods = false;
            RpfMan = new RpfManager(); //try not to use this in this mode...
            RpfMan.Init(allRpfs);

            AllRpfs = allRpfs;
            BaseRpfs = allRpfs;
            DlcRpfs = new List<RpfFile>();

            UpdateStatus("Building global dictionaries...");
            InitGlobalDicts();

            UpdateStatus("Loading manifests...");
            InitManifestDicts();

            UpdateStatus("Loading global texture list...");
            InitGtxds();

            UpdateStatus("Loading archetypes...");
            InitArchetypeDicts();

            UpdateStatus("Loading strings...");
            InitStringDicts();

            IsInited = true;
        }

        private void InitGlobal()
        {
            BaseRpfs = GetModdedRpfList(RpfMan.BaseRpfs);
            AllRpfs = GetModdedRpfList(RpfMan.AllRpfs);
            DlcRpfs = GetModdedRpfList(RpfMan.DlcRpfs);

            UpdateStatus("Building global dictionaries...");
            InitGlobalDicts();
        }

        private void InitDlc()
        {

            UpdateStatus("Building DLC List...");
            InitDlcList();

            UpdateStatus("Building active RPF dictionary...");
            InitActiveMapRpfFiles();

            UpdateStatus("Building map dictionaries...");
            InitMapDicts();

            UpdateStatus("Loading manifests...");
            InitManifestDicts();

            UpdateStatus("Loading global texture list...");
            InitGtxds();

            UpdateStatus("Loading cache...");
            InitMapCaches();

            UpdateStatus("Loading archetypes...");
            InitArchetypeDicts();

            UpdateStatus("Loading strings...");
            InitStringDicts();

            UpdateStatus("Loading vehicles...");
            InitVehicles();

            UpdateStatus("Loading peds...");
            InitPeds();

        }

        private void InitDlcList()
        {
            //if (!EnableDlc) return;

            string dlclistpath = "update\\update.rpf\\common\\data\\dlclist.xml";
            //if (!EnableDlc)
            //{
            //    dlclistpath = "common.rpf\\data\\dlclist.xml";
            //}
            var dlclistxml = RpfMan.GetFileXml(dlclistpath);

            DlcPaths.Clear();
            if ((dlclistxml == null) || (dlclistxml.DocumentElement == null))
            {
                ErrorLog("InitDlcList: Couldn't load " + dlclistpath + ".");
            }
            else
            {
                foreach (XmlNode pathsnode in dlclistxml.DocumentElement)
                {
                    foreach (XmlNode itemnode in pathsnode.ChildNodes)
                    {
                        DlcPaths.Add(itemnode.InnerText.ToLowerInvariant().Replace('\\', '/').Replace("platform:", "x64"));
                    }
                }
            }


            //get dlc path names in the appropriate format for reference by the dlclist paths
            Dictionary<string, RpfFile> dlcDict = new Dictionary<string, RpfFile>();
            Dictionary<string, RpfFile> dlcDict2 = new Dictionary<string, RpfFile>();
            foreach (RpfFile dlcrpf in DlcRpfs)
            {
                if (dlcrpf == null) continue;
                if (dlcrpf.NameLower == "dlc.rpf")
                {
                    string path = GetDlcRpfVirtualPath(dlcrpf.Path);
                    string name = GetDlcNameFromPath(dlcrpf.Path);
                    dlcDict[path] = dlcrpf;
                    dlcDict2[name] = dlcrpf;
                }
            }




            //find all the paths for patched files in update.rpf and build the dict
            DlcPatchedPaths.Clear();
            string updrpfpath = "update\\update.rpf";
            var updrpffile = RpfMan.FindRpfFile(updrpfpath);

            if (updrpffile != null)
            {
                XmlDocument updsetupdoc = RpfMan.GetFileXml(updrpfpath + "\\setup2.xml");
                DlcSetupFile updsetupfile = new DlcSetupFile();
                updsetupfile.Load(updsetupdoc);

                XmlDocument updcontentdoc = RpfMan.GetFileXml(updrpfpath + "\\" + updsetupfile.datFile);
                DlcContentFile updcontentfile = new DlcContentFile();
                updcontentfile.Load(updcontentdoc);

                updsetupfile.DlcFile = updrpffile;
                updsetupfile.ContentFile = updcontentfile;
                updcontentfile.DlcFile = updrpffile;

                updsetupfile.deviceName = "update";
                updcontentfile.LoadDicts(updsetupfile, RpfMan, this);

                if (updcontentfile.ExtraTitleUpdates != null)
                {
                    foreach (var tumount in updcontentfile.ExtraTitleUpdates.Mounts)
                    {
                        var lpath = tumount.path.ToLowerInvariant();
                        var relpath = lpath.Replace('/', '\\').Replace("update:\\", "");
                        var dlcname = GetDlcNameFromPath(relpath);
                        RpfFile dlcfile;
                        dlcDict2.TryGetValue(dlcname, out dlcfile);
                        if (dlcfile == null)
                        { continue; }
                        var dlcpath = dlcfile.Path + "\\";
                        var files = updrpffile.GetFiles(relpath, true);
                        foreach (var file in files)
                        {
                            if (file == null) continue;
                            var fpath = file.Path;
                            var frelpath = fpath.Replace(updrpfpath, "update:").Replace('\\', '/').Replace(lpath, dlcpath).Replace('/', '\\');
                            if (frelpath.StartsWith("mods\\"))
                            {
                                frelpath = frelpath.Substring(5);
                            }
                            DlcPatchedPaths[frelpath] = fpath;
                        }
                    }
                }
            }
            else
            {
                ErrorLog("InitDlcList: update.rpf not found!");
            }




            DlcSetupFiles.Clear();
            DlcExtraFolderMounts.Clear();

            foreach (string path in DlcPaths)
            {
                RpfFile dlcfile;
                if (dlcDict.TryGetValue(path, out dlcfile))
                {
                    try
                    {
                        string setuppath = GetDlcPatchedPath(dlcfile.Path + "\\setup2.xml");
                        XmlDocument setupdoc = RpfMan.GetFileXml(setuppath);
                        DlcSetupFile setupfile = new DlcSetupFile();
                        setupfile.Load(setupdoc);

                        string contentpath = GetDlcPatchedPath(dlcfile.Path + "\\" + setupfile.datFile);
                        XmlDocument contentdoc = RpfMan.GetFileXml(contentpath);
                        DlcContentFile contentfile = new DlcContentFile();
                        contentfile.Load(contentdoc);

                        setupfile.DlcFile = dlcfile;
                        setupfile.ContentFile = contentfile;
                        contentfile.DlcFile = dlcfile;

                        contentfile.LoadDicts(setupfile, RpfMan, this);
                        foreach (var extramount in contentfile.ExtraMounts.Values)
                        {
                            DlcExtraFolderMounts.Add(extramount);
                        }

                        DlcSetupFiles.Add(setupfile);

                    }
                    catch (Exception ex)
                    {
                        ErrorLog("InitDlcList: Error processing DLC " + path + "\n" + ex.ToString());
                    }
                }
            }

            //load the DLC in the correct order.... 
            DlcSetupFiles = DlcSetupFiles.OrderBy(o => o.order).ToList();


            DlcNameList.Clear();
            foreach (var sfile in DlcSetupFiles)
            {
                if ((sfile == null) || (sfile.DlcFile == null)) continue;
                DlcNameList.Add(GetDlcNameFromPath(sfile.DlcFile.Path));
            }

            if (DlcNameList.Count > 0)
            {
                if (string.IsNullOrEmpty(SelectedDlc))
                {
                    SelectedDlc = DlcNameList[DlcNameList.Count - 1];
                }
            }
        }

        private void InitImagesMetas()
        {
            //currently not used..

            ////parse images.meta
            //string imagesmetapath = "common.rpf\\data\\levels\\gta5\\images.meta";
            //if (EnableDlc)
            //{
            //    imagesmetapath = "update\\update.rpf\\common\\data\\levels\\gta5\\images.meta";
            //}
            //var imagesmetaxml = RpfMan.GetFileXml(imagesmetapath);
            //var imagesnodes = imagesmetaxml.DocumentElement.ChildNodes;
            //List<DlcContentDataFile> imagedatafilelist = new List<DlcContentDataFile>();
            //Dictionary<string, DlcContentDataFile> imagedatafiles = new Dictionary<string, DlcContentDataFile>();
            //foreach (XmlNode node in imagesnodes)
            //{
            //    DlcContentDataFile datafile = new DlcContentDataFile(node);
            //    string fname = datafile.filename.ToLower();
            //    fname = fname.Replace('\\', '/');
            //    imagedatafiles[fname] = datafile;
            //    imagedatafilelist.Add(datafile);
            //}


            //filter ActiveMapFiles based on images.meta?

            //DlcContentDataFile imagesdata;
            //if (imagedatafiles.TryGetValue(path, out imagesdata))
            //{
            //    ActiveMapRpfFiles[path] = baserpf;
            //}
        }

        private void InitActiveMapRpfFiles()
        {
            ActiveMapRpfFiles.Clear();

            foreach (RpfFile baserpf in BaseRpfs) //start with all the base rpf's (eg x64a.rpf)
            {
                string path = baserpf.Path.Replace('\\', '/');
                if (path == "common.rpf")
                {
                    ActiveMapRpfFiles["common"] = baserpf;
                }
                else
                {
                    int bsind = path.IndexOf('/');
                    if ((bsind > 0) && (bsind < path.Length))
                    {
                        path = "x64" + path.Substring(bsind);

                        //if (ActiveMapRpfFiles.ContainsKey(path))
                        //{ } //x64d.rpf\levels\gta5\generic\cutsobjects.rpf // x64g.rpf\levels\gta5\generic\cutsobjects.rpf - identical?

                        ActiveMapRpfFiles[path] = baserpf;
                    }
                    else
                    {
                        //do we need to include root rpf files? generally don't seem to contain map data?
                        ActiveMapRpfFiles[path] = baserpf;
                    }
                }
            }

            if (!EnableDlc) return; //don't continue for base title only

            foreach (var rpf in DlcRpfs)
            {
                if (rpf.NameLower == "update.rpf")//include this so that files not in child rpf's can be used..
                {
                    string path = rpf.Path.Replace('\\', '/');
                    ActiveMapRpfFiles[path] = rpf;
                    break;
                }
            }


            DlcActiveRpfs.Clear();
            DlcCacheFileList.Clear();

            //int maxdlcorder = 10000000;

            Dictionary<string, List<string>> overlays = new Dictionary<string, List<string>>();

            foreach (var setupfile in DlcSetupFiles)
            {
                if (setupfile.DlcFile != null)
                {
                    //if (setupfile.order > maxdlcorder)
                    //    break;

                    var contentfile = setupfile.ContentFile;
                    var dlcfile = setupfile.DlcFile;

                    DlcActiveRpfs.Add(dlcfile);

                    for (int i = 1; i <= setupfile.subPackCount; i++)
                    {
                        var subpackPath = dlcfile.Path.Replace("\\dlc.rpf", "\\dlc" + i.ToString() + ".rpf");
                        var subpack = RpfMan.FindRpfFile(subpackPath);
                        if (subpack != null)
                        {
                            DlcActiveRpfs.Add(subpack);

                            if (setupfile.DlcSubpacks == null) setupfile.DlcSubpacks = new List<RpfFile>();
                            setupfile.DlcSubpacks.Add(subpack);
                        }
                    }



                    string dlcname = GetDlcNameFromPath(dlcfile.Path);



                    foreach (var rpfkvp in contentfile.RpfDataFiles)
                    {
                        string umpath = GetDlcUnmountedPath(rpfkvp.Value.filename);
                        string phpath = GetDlcRpfPhysicalPath(umpath, setupfile);

                        //if (rpfkvp.Value.overlay)
                        AddDlcOverlayRpf(rpfkvp.Key, umpath, setupfile, overlays);

                        AddDlcActiveMapRpfFile(rpfkvp.Key, phpath, setupfile);
                    }




                    DlcExtraFolderMountFile extramount;
                    DlcContentDataFile rpfdatafile;


                    foreach (var changeset in contentfile.contentChangeSets)
                    {
                        if (changeset.useCacheLoader)
                        {
                            uint cachehash = JenkHash.GenHash(changeset.changeSetName.ToLowerInvariant());
                            string cachefilename = dlcname + "_" + cachehash.ToString() + "_cache_y.dat";
                            string cachefilepath = dlcfile.Path + "\\x64\\data\\cacheloaderdata_dlc\\" + cachefilename;
                            string cachefilepathpatched = GetDlcPatchedPath(cachefilepath);
                            DlcCacheFileList.Add(cachefilepathpatched);

                            //if ((changeset.mapChangeSetData != null) && (changeset.mapChangeSetData.Count > 0))
                            //{ }
                            //else
                            //{ }
                        }
                        else
                        {
                            //if ((changeset.mapChangeSetData != null) && (changeset.mapChangeSetData.Count > 0))
                            //{ }
                            //if (changeset.executionConditions != null)
                            //{ }
                        }
                        //if (changeset.filesToInvalidate != null)
                        //{ }//not used
                        //if (changeset.filesToDisable != null)
                        //{ }//not used
                        if (changeset.filesToEnable != null)
                        {
                            foreach (string file in changeset.filesToEnable)
                            {
                                string dfn = GetDlcPlatformPath(file).ToLowerInvariant();
                                if (contentfile.ExtraMounts.TryGetValue(dfn, out extramount))
                                {
                                    //foreach (var rpfkvp in contentfile.RpfDataFiles)
                                    //{
                                    //    string umpath = GetDlcUnmountedPath(rpfkvp.Value.filename);
                                    //    string phpath = GetDlcRpfPhysicalPath(umpath, setupfile);
                                    //    //if (rpfkvp.Value.overlay)
                                    //    AddDlcOverlayRpf(rpfkvp.Key, umpath, setupfile, overlays);
                                    //    AddDlcActiveMapRpfFile(rpfkvp.Key, phpath);
                                    //}
                                }
                                else if (contentfile.RpfDataFiles.TryGetValue(dfn, out rpfdatafile))
                                {
                                    string phpath = GetDlcRpfPhysicalPath(rpfdatafile.filename, setupfile);

                                    //if (rpfdatafile.overlay)
                                    AddDlcOverlayRpf(dfn, rpfdatafile.filename, setupfile, overlays);

                                    AddDlcActiveMapRpfFile(dfn, phpath, setupfile);
                                }
                                else
                                {
                                    if (dfn.EndsWith(".rpf"))
                                    { }
                                }
                            }
                        }
                        if (changeset.executionConditions != null)
                        { }

                        if (changeset.mapChangeSetData != null)
                        {
                            foreach (var mapcs in changeset.mapChangeSetData)
                            {
                                //if (mapcs.mapChangeSetData != null)
                                //{ }//not used
                                if (mapcs.filesToInvalidate != null)
                                {
                                    foreach (string file in mapcs.filesToInvalidate)
                                    {
                                        string upath = GetDlcMountedPath(file);
                                        string fpath = GetDlcPlatformPath(upath);
                                        if (fpath.EndsWith(".rpf"))
                                        {
                                            RemoveDlcActiveMapRpfFile(fpath, overlays);
                                        }
                                        else
                                        { } //how to deal with individual files? milo_.interior
                                    }
                                }
                                if (mapcs.filesToDisable != null)
                                { }
                                if (mapcs.filesToEnable != null)
                                {
                                    foreach (string file in mapcs.filesToEnable)
                                    {
                                        string fpath = GetDlcPlatformPath(file);
                                        string umpath = GetDlcUnmountedPath(fpath);
                                        string phpath = GetDlcRpfPhysicalPath(umpath, setupfile);

                                        if (fpath != umpath)
                                        { }

                                        AddDlcOverlayRpf(fpath, umpath, setupfile, overlays);

                                        AddDlcActiveMapRpfFile(fpath, phpath, setupfile);
                                    }
                                }
                            }
                        }
                    }




                    if (dlcname == SelectedDlc)
                    {
                        break; //everything's loaded up to the selected DLC.
                    }

                }
            }
        }

        private void AddDlcActiveMapRpfFile(string vpath, string phpath, DlcSetupFile setupfile)
        {
            vpath = vpath.ToLowerInvariant();
            phpath = phpath.ToLowerInvariant();
            if (phpath.EndsWith(".rpf"))
            {
                RpfFile rpffile = RpfMan.FindRpfFile(phpath);
                if (rpffile != null)
                {
                    ActiveMapRpfFiles[vpath] = rpffile;
                }
                else
                { }
            }
            else
            { } //how to handle individual files? eg interiorProxies.meta
        }
        private void AddDlcOverlayRpf(string path, string umpath, DlcSetupFile setupfile, Dictionary<string, List<string>> overlays)
        {
            string opath = GetDlcOverlayPath(umpath, setupfile);
            if (opath == path) return;
            List<string> overlayList;
            if (!overlays.TryGetValue(opath, out overlayList))
            {
                overlayList = new List<string>();
                overlays[opath] = overlayList;
            }
            overlayList.Add(path);
        }
        private void RemoveDlcActiveMapRpfFile(string vpath, Dictionary<string, List<string>> overlays)
        {
            List<string> overlayList;
            if (overlays.TryGetValue(vpath, out overlayList))
            {
                foreach (string overlayPath in overlayList)
                {
                    if (ActiveMapRpfFiles.ContainsKey(overlayPath))
                    {
                        ActiveMapRpfFiles.Remove(overlayPath);
                    }
                    else
                    { }
                }
                overlays.Remove(vpath);
            }

            if (ActiveMapRpfFiles.ContainsKey(vpath))
            {
                ActiveMapRpfFiles.Remove(vpath);
            }
            else
            { } //nothing to remove?
        }
        private string GetDlcRpfPhysicalPath(string path, DlcSetupFile setupfile)
        {
            string devname = setupfile.deviceName.ToLowerInvariant();
            string fpath = GetDlcPlatformPath(path).ToLowerInvariant();
            string kpath = fpath;//.Replace(devname + ":\\", "");
            string dlcpath = setupfile.DlcFile.Path;
            fpath = fpath.Replace(devname + ":", dlcpath);
            fpath = fpath.Replace("x64:", dlcpath + "\\x64").Replace('/', '\\');
            if (setupfile.DlcSubpacks != null)
            {
                if (RpfMan.FindRpfFile(fpath) == null)
                {
                    foreach (var subpack in setupfile.DlcSubpacks)
                    {
                        dlcpath = subpack.Path;
                        var tpath = kpath.Replace(devname + ":", dlcpath);
                        tpath = tpath.Replace("x64:", dlcpath + "\\x64").Replace('/', '\\');
                        if (RpfMan.FindRpfFile(tpath) != null)
                        {
                            return GetDlcPatchedPath(tpath);
                        }
                    }
                }
            }
            return GetDlcPatchedPath(fpath);
        }
        private string GetDlcOverlayPath(string path, DlcSetupFile setupfile)
        {
            string devname = setupfile.deviceName.ToLowerInvariant();
            string fpath = path.Replace("%PLATFORM%", "x64").Replace('\\', '/').ToLowerInvariant();
            string opath = fpath.Replace(devname + ":/", "");
            return opath;
        }
        private string GetDlcRpfVirtualPath(string path)
        {
            path = path.Replace('\\', '/');
            if (path.StartsWith("mods/"))
            {
                path = path.Substring(5);
            }
            if (path.Length > 7)
            {
                path = path.Substring(0, path.Length - 7);//trim off "dlc.rpf"
            }
            if (path.StartsWith("x64"))
            {
                int bsind = path.IndexOf('/'); //replace x64*.rpf
                if ((bsind > 0) && (bsind < path.Length))
                {
                    path = "x64" + path.Substring(bsind);
                }
                else
                { } //no hits here
            }
            else if (path.StartsWith("update/x64/dlcpacks"))
            {
                path = path.Replace("update/x64/dlcpacks", "dlcpacks:");
            }
            else
            { } //no hits here

            return path;
        }
        private string GetDlcNameFromPath(string path)
        {
            string[] parts = path.ToLowerInvariant().Split('\\');
            if (parts.Length > 1)
            {
                return parts[parts.Length - 2].ToLowerInvariant();
            }
            return path;
        }
        public static string GetDlcPlatformPath(string path)
        {
            return path.Replace("%PLATFORM%", "x64").Replace('\\', '/').Replace("platform:", "x64").ToLowerInvariant();
        }
        private string GetDlcMountedPath(string path)
        {
            foreach (var efm in DlcExtraFolderMounts)
            {
                foreach (var fm in efm.FolderMounts)
                {
                    if ((fm.platform == null) || (fm.platform == "x64"))
                    {
                        if (path.StartsWith(fm.path))
                        {
                            path = path.Replace(fm.path, fm.mountAs);
                        }
                    }
                }
            }
            return path;
        }
        private string GetDlcUnmountedPath(string path)
        {
            foreach (var efm in DlcExtraFolderMounts)
            {
                foreach (var fm in efm.FolderMounts)
                {
                    if ((fm.platform == null) || (fm.platform == "x64"))
                    {
                        if (path.StartsWith(fm.mountAs))
                        {
                            path = path.Replace(fm.mountAs, fm.path);
                        }
                    }
                }
            }
            return path;
        }
        public string GetDlcPatchedPath(string path)
        {
            string p;
            if (DlcPatchedPaths.TryGetValue(path, out p))
            {
                return p;
            }
            return path;
        }

        private List<RpfFile> GetModdedRpfList(List<RpfFile> list)
        {
            //if (!EnableMods) return new List<RpfFile>(list);
            List<RpfFile> rlist = new List<RpfFile>();
            RpfFile f;
            if (!EnableMods)
            {
                foreach (var file in list)
                {
                    if (!file.Path.StartsWith("mods"))
                    {
                        rlist.Add(file);
                    }
                }
            }
            else
            {
                foreach (var file in list)
                {
                    if (RpfMan.ModRpfDict.TryGetValue(file.Path, out f))
                    {
                        rlist.Add(f);
                    }
                    else
                    {
                        if (file.Path.StartsWith("mods"))
                        {
                            var basepath = file.Path.Substring(5);
                            if (!RpfMan.RpfDict.ContainsKey(basepath)) //this file isn't overriding anything
                            {
                                rlist.Add(file);
                            }
                        }
                        else
                        {
                            rlist.Add(file);
                        }
                    }
                }
            }
            return rlist;
        }


        private void InitGlobalDicts()
        {
            YdrDict = new Dictionary<uint, RpfFileEntry>();
            YddDict = new Dictionary<uint, RpfFileEntry>();
            YtdDict = new Dictionary<uint, RpfFileEntry>();
            YftDict = new Dictionary<uint, RpfFileEntry>();
            YcdDict = new Dictionary<uint, RpfFileEntry>();
            foreach (var rpffile in AllRpfs)
            {
                if (rpffile.AllEntries == null) continue;
                foreach (var entry in rpffile.AllEntries)
                {
                    if (entry is RpfFileEntry)
                    {
                        RpfFileEntry fentry = entry as RpfFileEntry;
                        if (entry.NameLower.EndsWith(".ydr"))
                        {
                            YdrDict[entry.NameHash] = fentry; //replaces any existing entries...
                            YdrDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.NameLower.EndsWith(".ydd"))
                        {
                            YddDict[entry.NameHash] = fentry; //replaces any existing entries...
                            YddDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.NameLower.EndsWith(".ytd"))
                        {
                            YtdDict[entry.NameHash] = fentry; //replaces any existing entries...
                            YtdDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.NameLower.EndsWith(".yft"))
                        {
                            YftDict[entry.NameHash] = fentry;
                            YftDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.NameLower.EndsWith(".ycd"))
                        {
                            YcdDict[entry.NameHash] = fentry;
                            YcdDict[entry.ShortNameHash] = fentry;
                        }
                    }
                }
            }

        }

        private void InitMapDicts()
        {
            YmapDict = new Dictionary<uint, RpfFileEntry>();
            YbnDict = new Dictionary<uint, RpfFileEntry>();
            YnvDict = new Dictionary<uint, RpfFileEntry>();
            foreach (var rpffile in ActiveMapRpfFiles.Values) //RpfMan.BaseRpfs)
            {
                if (rpffile.AllEntries == null) continue;
                foreach (var entry in rpffile.AllEntries)
                {
                    if (entry is RpfFileEntry)
                    {
                        RpfFileEntry fentry = entry as RpfFileEntry;
                        if (entry.NameLower.EndsWith(".ymap"))
                        {
                            //YmapDict[entry.NameHash] = fentry;
                            YmapDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.NameLower.EndsWith(".ybn"))
                        {
                            //YbnDict[entry.NameHash] = fentry;
                            YbnDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.NameLower.EndsWith(".ynv"))
                        {
                            YnvDict[entry.ShortNameHash] = fentry;
                        }
                    }
                }
            }

            AllYmapsDict = new Dictionary<uint, RpfFileEntry>();
            foreach (var rpffile in AllRpfs)
            {
                if (rpffile.AllEntries == null) continue;
                foreach (var entry in rpffile.AllEntries)
                {
                    if (entry is RpfFileEntry)
                    {
                        RpfFileEntry fentry = entry as RpfFileEntry;
                        if (entry.NameLower.EndsWith(".ymap"))
                        {
                            AllYmapsDict[entry.ShortNameHash] = fentry;
                        }
                    }
                }
            }

        }

        private void InitManifestDicts()
        {
            AllManifests = new List<YmfFile>();
            hdtexturelookup = new Dictionary<MetaHash, MetaHash>();
            IEnumerable<RpfFile> rpfs = PreloadedMode ? AllRpfs : (IEnumerable<RpfFile>)ActiveMapRpfFiles.Values;
            foreach (RpfFile file in rpfs)
            {
                if (file.AllEntries == null) continue;
                //manifest and meta parsing..
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //sp_manifest.ymt
                    //if (entry.NameLower.EndsWith("zonebind.ymt")/* || entry.Name.EndsWith("vinewood.ymt")*/)
                    //{
                    //    YmtFile ymt = GetFile<YmtFile>(entry);
                    //}
                    if (entry.Name.EndsWith(".ymf"))// || entry.Name.EndsWith(".ymt"))
                    {
                        try
                        {
                            UpdateStatus(string.Format(entry.Path));
                            YmfFile ymffile = RpfMan.GetFile<YmfFile>(entry);
                            if (ymffile != null)
                            {
                                AllManifests.Add(ymffile);

                                if (ymffile.Pso != null)
                                { }
                                else if (ymffile.Rbf != null)
                                { }
                                else if (ymffile.Meta != null)
                                { }
                                else
                                { }


                                if (ymffile.HDTxdAssetBindings != null)
                                {
                                    for (int i = 0; i < ymffile.HDTxdAssetBindings.Length; i++)
                                    {
                                        var b = ymffile.HDTxdAssetBindings[i];
                                        var targetasset = JenkHash.GenHash(b.targetAsset.ToString().ToLowerInvariant());
                                        var hdtxd = JenkHash.GenHash(b.HDTxd.ToString().ToLowerInvariant());
                                        hdtexturelookup[targetasset] = hdtxd;
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            string errstr = entry.Path + "\n" + ex.ToString();
                            ErrorLog(errstr);
                        }
                    }

                }

            }
        }

        private void InitGtxds()
        {

            var parentTxds = new Dictionary<MetaHash, MetaHash>();

            IEnumerable<RpfFile> rpfs = PreloadedMode ? AllRpfs : (IEnumerable<RpfFile>)ActiveMapRpfFiles.Values;

            var addTxdRelationships = new Action<Dictionary<string, string>>((from) =>
            {
                foreach (var kvp in from)
                {
                    uint chash = JenkHash.GenHash(kvp.Key.ToLowerInvariant());
                    uint phash = JenkHash.GenHash(kvp.Value.ToLowerInvariant());
                    if (!parentTxds.ContainsKey(chash))
                    {
                        parentTxds.Add(chash, phash);
                    }
                    else
                    {
                    }
                }
            });

            var addRpfTxdRelationships = new Action<IEnumerable<RpfFile>>((from) =>
            {
                foreach (RpfFile file in from)
                {
                    if (file.AllEntries == null) continue;
                    foreach (RpfEntry entry in file.AllEntries)
                    {
                        try
                        {
                            if ((entry.NameLower == "gtxd.ymt") || (entry.NameLower == "gtxd.meta"))
                            {
                                GtxdFile ymt = RpfMan.GetFile<GtxdFile>(entry);
                                if (ymt.TxdRelationships != null)
                                {
                                    addTxdRelationships(ymt.TxdRelationships);
                                }
                            }
                            else if (entry.NameLower == "vehicles.meta")
                            {
                                VehiclesFile vf = RpfMan.GetFile<VehiclesFile>(entry);//could also get loaded in InitVehicles...
                                if (vf.TxdRelationships != null)
                                {
                                    addTxdRelationships(vf.TxdRelationships);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string errstr = entry.Path + "\n" + ex.ToString();
                            ErrorLog(errstr);
                        }
                    }
                }

            });


            addRpfTxdRelationships(rpfs);


            if (EnableDlc)
            {
                addRpfTxdRelationships(DlcActiveRpfs);
            }


            textureParents = parentTxds;




            //ensure resident global texture dicts:
            YtdFile ytd1 = new YtdFile(GetYtdEntry(JenkHash.GenHash("mapdetail")));
            LoadFile(ytd1);
            AddTextureLookups(ytd1);

            YtdFile ytd2 = new YtdFile(GetYtdEntry(JenkHash.GenHash("vehshare")));
            LoadFile(ytd2);
            AddTextureLookups(ytd2);



        }

        private void InitMapCaches()
        {
            AllCacheFiles = new List<CacheDatFile>();
            YmapHierarchyDict = new Dictionary<uint, MapDataStoreNode>();

            string cachefilepath = "common.rpf\\data\\gta5_cache_y.dat";
            if (EnableDlc)
            {
                cachefilepath = "update\\update.rpf\\common\\data\\gta5_cache_y.dat";
            }

            try
            {
                var maincache = RpfMan.GetFile<CacheDatFile>(cachefilepath);
                if (maincache != null)
                {
                    AllCacheFiles.Add(maincache);
                    foreach (var node in maincache.AllMapNodes)
                    {
                        if (YmapDict.ContainsKey(node.Name))
                        {
                            YmapHierarchyDict[node.Name] = node;
                        }
                        else
                        { } //ymap not found...
                    }
                }
                else
                {
                    ErrorLog(cachefilepath + ": cache not loaded! Possibly an unsupported GTAV installation version.");
                }
            }
            catch (Exception ex)
            {
                ErrorLog(cachefilepath + ": " + ex.ToString());
            }


            if (EnableDlc)
            {
                foreach (string dlccachefile in DlcCacheFileList)
                {
                    try
                    {
                        var dat = RpfMan.GetFile<CacheDatFile>(dlccachefile);
                        if (dat == null)
                        { continue; } //update\\x64\\dlcpacks\\mpspecialraces\\dlc.rpf\\x64\\data\\cacheloaderdata_dlc\\mpspecialraces_3336915258_cache_y.dat (hash of: mpspecialraces_interior_additions)
                        AllCacheFiles.Add(dat);
                        foreach (var node in dat.AllMapNodes)
                        {
                            if (YmapDict.ContainsKey(node.Name))
                            {
                                YmapHierarchyDict[node.Name] = node;
                            }
                            else
                            { } //ymap not found...
                        }
                    }
                    catch (Exception ex)
                    {
                        string errstr = dlccachefile + "\n" + ex.ToString();
                        ErrorLog(errstr);
                    }

                }

                //foreach (var dlcfile in DlcActiveRpfs)
                //{
                //    foreach (RpfEntry entry in dlcfile.AllEntries)
                //    {
                //        try
                //        {
                //            if (entry.NameLower.EndsWith("_cache_y.dat"))
                //            {
                //                var dat = RpfMan.GetFile<CacheDatFile>(entry);
                //                AllCacheFiles.Add(dat);
                //                foreach (var node in dat.AllMapNodes)
                //                {
                //                    if (YmapDict.ContainsKey(node.Name))
                //                    {
                //                        YmapHierarchyDict[node.Name] = node;
                //                    }
                //                    else
                //                    { } //ymap not found...
                //                }
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            string errstr = entry.Path + "\n" + ex.ToString();
                //            ErrorLog(errstr);
                //        }
                //    }
                //}
            }





            //foreach (RpfFile file in RpfMan.BaseRpfs)
            //{
            //    if (file.AllEntries == null) continue;
            //    foreach (RpfEntry entry in file.AllEntries)
            //    {
            //        try
            //        {
            //            //if (entry.Name.EndsWith("_manifest.ymf"))
            //            //{
            //            //    var ymf = GetFile<YmfFile>(entry);
            //            //}
            //            //else 
            //            if (entry.NameLower.EndsWith("_cache_y.dat"))
            //            {
            //                //parse the cache dat files.
            //                var dat = RpfMan.GetFile<CacheDatFile>(entry);
            //                AllCacheFiles.Add(dat);
            //                foreach (var node in dat.AllMapNodes)
            //                {
            //                    YmapHierarchyDict[node.Name] = node;
            //                }
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            string errstr = entry.Path + "\n" + ex.ToString();
            //            ErrorLog(errstr);
            //        }
            //    }
            //}
        }

        private void InitArchetypeDicts()
        {

            YtypDict = new Dictionary<uint, YtypFile>();

            archetypesLoaded = false;
            archetypeDict.Clear();

            if (!LoadArchetypes) return;


            var rpfs = EnableDlc ? AllRpfs : BaseRpfs;

            foreach (RpfFile file in rpfs) //RpfMan.BaseRpfs)RpfMan.AllRpfs)//ActiveMapRpfFiles.Values) // 
            {
                if (file.AllEntries == null) continue;
                if (!EnableDlc && file.Path.StartsWith("update")) continue;

                //parse ytyps
                foreach (RpfEntry entry in file.AllEntries)
                {
                    try
                    {
                        if (entry.NameLower.EndsWith(".ytyp"))
                        {
                            AddYtypToDictionary(entry);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLog(entry.Path + ": " + ex.Message);
                    }
                }
            }

            archetypesLoaded = true;

        }

        private void AddYtypToDictionary(RpfEntry entry)
        {
            UpdateStatus(string.Format(entry.Path));
            YtypFile ytypfile = RpfMan.GetFile<YtypFile>(entry);
            if (ytypfile == null)
            {
                throw new Exception("Couldn't load ytyp file."); //couldn't load the file for some reason... shouldn't happen..
            }
            if (ytypfile.Meta == null)
            {
                throw new Exception("ytyp file was not in meta format.");
            }
            if (YtypDict.ContainsKey(ytypfile.NameHash))
            {
                //throw new Exception("ytyp " + JenkIndex.GetString(ytypfile.NameHash) + " already loaded.");
                //errorAction(entry.Path + ": ytyp " + JenkIndex.GetString(ytypfile.NameHash) + " already loaded.");
                YtypDict[ytypfile.NameHash] = ytypfile; //override ytyp and continue anyway, could be unique archetypes in here still...
            }
            else
            {
                YtypDict.Add(ytypfile.NameHash, ytypfile);
            }



            if ((ytypfile.AllArchetypes == null) || (ytypfile.AllArchetypes.Length == 0))
            {
                ErrorLog(entry.Path + ": no archetypes found");
            }
            else
            {
                foreach (var arch in ytypfile.AllArchetypes)
                {
                    uint hash = arch.Hash;
                    if (hash == 0) continue;
                    if (archetypeDict.ContainsKey(hash))
                    {
                        var oldval = archetypeDict[hash]; //replace old archetype?
                    }
                    archetypeDict[hash] = arch;
                }
            }


            ////if (ytypfile.AudioEmitters != null)
            ////{
            ////    foreach (CExtensionDefAudioEmitter emitter in ytypfile.AudioEmitters)
            ////    {
            ////        //audioind++;
            ////        //uint hash = emitter.name;
            ////        //if (hash == 0) hash = archetype.name;
            ////        //if (hash == 0)
            ////        //    continue;
            ////        //if (AudioArchetypes.ContainsKey(hash))
            ////        //{
            ////        //    var oldval = AudioArchetypes[hash];
            ////        //    //errorAction(entry.Path + ": " + emitter.ToString() + ": (CTimeArchetypeDef) Already in archetype dict. Was in: " + oldval.ToString());
            ////        //    //overwrite with new definition? how to tell?
            ////        //    AudioArchetypes[hash] = new Tuple<YtypFile, int>(ytypfile, audioind);
            ////        //}
            ////        //else
            ////        //{
            ////        //    AudioArchetypes.Add(hash, new Tuple<YtypFile, int>(ytypfile, audioind));
            ////        //}
            ////    }
            ////}

        }

        public void InitStringDicts()
        {
            string langstr = "american_rel"; //todo: make this variable?
            string langstr2 = "americandlc.rpf";
            string langstr3 = "american.rpf";

            Gxt2Dict = new Dictionary<uint, RpfFileEntry>();
            var gxt2files = new List<Gxt2File>();
            foreach (var rpf in AllRpfs)
            {
                foreach (var entry in rpf.AllEntries)
                {
                    if (entry is RpfFileEntry fentry)
                    {
                        var p = entry.Path;
                        if (entry.NameLower.EndsWith(".gxt2") && (p.Contains(langstr) || p.Contains(langstr2) || p.Contains(langstr3)))
                        {
                            Gxt2Dict[entry.ShortNameHash] = fentry;

                            if (DoFullStringIndex)
                            {
                                var gxt2 = RpfMan.GetFile<Gxt2File>(entry);
                                if (gxt2 != null)
                                {
                                    for (int i = 0; i < gxt2.TextEntries.Length; i++)
                                    {
                                        var e = gxt2.TextEntries[i];
                                        GlobalText.Ensure(e.Text, e.Hash);
                                    }
                                    gxt2files.Add(gxt2);
                                }
                            }
                        }
                    }
                }
            }

            if (!DoFullStringIndex)
            {
                string globalgxt2path = "x64b.rpf\\data\\lang\\" + langstr + ".rpf\\global.gxt2";
                var globalgxt2 = RpfMan.GetFile<Gxt2File>(globalgxt2path);
                if (globalgxt2 != null)
                {
                    for (int i = 0; i < globalgxt2.TextEntries.Length; i++)
                    {
                        var e = globalgxt2.TextEntries[i];
                        GlobalText.Ensure(e.Text, e.Hash);
                    }
                }
                return;
            }


            GlobalText.FullIndexBuilt = true;





            foreach (var rpf in AllRpfs)
            {
                foreach (var entry in rpf.AllEntries)
                {
                    if (entry.NameLower.EndsWith("statssetup.xml"))
                    {
                        var xml = RpfMan.GetFileXml(entry.Path);
                        if (xml == null)
                        { continue; }

                        var statnodes = xml.SelectNodes("StatsSetup/stats/stat");

                        foreach (XmlNode statnode in statnodes)
                        {
                            if (statnode == null)
                            { continue; }
                            var statname = Xml.GetStringAttribute(statnode, "Name");
                            if (string.IsNullOrEmpty(statname))
                            { continue; }

                            var statnamel = statname.ToLowerInvariant();
                            StatsNames.Ensure(statname);
                            StatsNames.Ensure(statnamel);

                            StatsNames.Ensure("sp_" + statnamel);
                            StatsNames.Ensure("mp0_" + statnamel);
                            StatsNames.Ensure("mp1_" + statnamel);

                        }
                    }
                }
            }

            StatsNames.FullIndexBuilt = true;
        }

        public void InitVehicles()
        {
            if (!LoadVehicles) return;


            //Neos7
            //Involved files(at least for rendering purpose )
            //Vehicles.meta
            //Carcols.meta
            //Carvariations.meta
            //Vehiclelayouts.meta
            //The other metas shouldn't be important for rendering
            //Then the global carcols.ymt is required too
            //As it contains the general shared tuning options
            //Carcols for modkits and lights kits definitions
            //Carvariations links such modkits and lights kits to each vehicle plus defines colours combinations of spawned vehicles
            //Vehiclelayouts mostly to handle ped interactions with the vehicle





            IEnumerable<RpfFile> rpfs = PreloadedMode ? AllRpfs : (IEnumerable<RpfFile>)ActiveMapRpfFiles.Values;


            var allVehicles = new Dictionary<MetaHash, VehicleInitData>();
            var allCarCols = new List<CarColsFile>();
            var allCarModCols = new List<CarModColsFile>();
            var allCarVariations = new List<CarVariationsFile>();
            var allCarVariationsDict = new Dictionary<MetaHash, CVehicleModelInfoVariation_418053801>();
            var allVehicleLayouts = new List<VehicleLayoutsFile>();

            var addVehicleFiles = new Action<IEnumerable<RpfFile>>((from) =>
            {
                foreach (RpfFile file in from)
                {
                    if (file.AllEntries == null) continue;
                    foreach (RpfEntry entry in file.AllEntries)
                    {
#if !DEBUG
                        try
#endif
                        {
                            if (entry.NameLower == "vehicles.meta")
                            {
                                VehiclesFile vf = RpfMan.GetFile<VehiclesFile>(entry);
                                if (vf.InitDatas != null)
                                {
                                    foreach (var initData in vf.InitDatas)
                                    {
                                        var name = initData.modelName.ToLowerInvariant();
                                        var hash = JenkHash.GenHash(name);
                                        if (allVehicles.ContainsKey(hash))
                                        { }
                                        allVehicles[hash] = initData;
                                    }
                                }
                            }
                            if ((entry.NameLower == "carcols.ymt") || (entry.NameLower == "carcols.meta"))
                            {
                                var cf = RpfMan.GetFile<CarColsFile>(entry);
                                if (cf.VehicleModelInfo != null)
                                { }
                                allCarCols.Add(cf);
                            }
                            if (entry.NameLower == "carmodcols.ymt")
                            {
                                var cf = RpfMan.GetFile<CarModColsFile>(entry);
                                if (cf.VehicleModColours != null)
                                { }
                                allCarModCols.Add(cf);
                            }
                            if ((entry.NameLower == "carvariations.ymt") || (entry.NameLower == "carvariations.meta"))
                            {
                                var cf = RpfMan.GetFile<CarVariationsFile>(entry);
                                if (cf.VehicleModelInfo?.variationData != null)
                                {
                                    foreach (var variation in cf.VehicleModelInfo.variationData)
                                    {
                                        var name = variation.modelName.ToLowerInvariant();
                                        var hash = JenkHash.GenHash(name);
                                        allCarVariationsDict[hash] = variation;
                                    }
                                }
                                allCarVariations.Add(cf);
                            }
                            if (entry.NameLower.StartsWith("vehiclelayouts") && entry.NameLower.EndsWith(".meta"))
                            {
                                var lf = RpfMan.GetFile<VehicleLayoutsFile>(entry);
                                if (lf.Xml != null)
                                { }
                                allVehicleLayouts.Add(lf);
                            }
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            string errstr = entry.Path + "\n" + ex.ToString();
                            ErrorLog(errstr);
                        }
#endif
                    }
                }

            });


            addVehicleFiles(rpfs);

            if (EnableDlc)
            {
                addVehicleFiles(DlcActiveRpfs);
            }


            VehiclesInitDict = allVehicles;

        }

        public void InitPeds()
        {
            if (!LoadPeds) return;

            IEnumerable<RpfFile> rpfs = PreloadedMode ? AllRpfs : (IEnumerable<RpfFile>)ActiveMapRpfFiles.Values;
            List<RpfFile> dlcrpfs = new List<RpfFile>();
            if (EnableDlc)
            {
                foreach (var rpf in DlcActiveRpfs)
                {
                    dlcrpfs.Add(rpf);
                    if (rpf.Children == null) continue;
                    foreach (var crpf in rpf.Children)
                    {
                        dlcrpfs.Add(crpf);
                        if (crpf.Children?.Count > 0)
                        { }
                    }
                }
            }



            var allPeds = new Dictionary<MetaHash, CPedModelInfo__InitData>();
            var allPedsFiles = new List<PedsFile>();
            var allPedYmts = new Dictionary<MetaHash, PedFile>();
            var allPedDrwDicts = new Dictionary<MetaHash, Dictionary<MetaHash, RpfFileEntry>>();
            var allPedTexDicts = new Dictionary<MetaHash, Dictionary<MetaHash, RpfFileEntry>>();
            var allPedClothDicts = new Dictionary<MetaHash, Dictionary<MetaHash, RpfFileEntry>>();


            Dictionary<MetaHash, RpfFileEntry> ensureDict(Dictionary<MetaHash, Dictionary<MetaHash, RpfFileEntry>> coll, MetaHash hash)
            {
                Dictionary<MetaHash, RpfFileEntry> dict;
                if (!coll.TryGetValue(hash, out dict))
                {
                    dict = new Dictionary<MetaHash, RpfFileEntry>();
                    coll[hash] = dict;
                }
                return dict;
            }

            var addPedDicts = new Action<string, MetaHash, RpfDirectoryEntry>((namel, hash, dir) =>
            {
                Dictionary<MetaHash, RpfFileEntry> dict = null;
                var files = dir?.Files;
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file.NameLower == namel + ".yld")
                        {
                            dict = ensureDict(allPedClothDicts, hash);
                            dict[file.ShortNameHash] = file;
                        }
                    }
                }

                if (dir?.Directories != null)
                {
                    foreach (var cdir in dir.Directories)
                    {
                        if (cdir.NameLower == namel)
                        {
                            dir = cdir;
                            break;
                        }
                    }
                    files = dir?.Files;
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            if (file?.NameLower == null) continue;
                            if (file.NameLower.EndsWith(".ydd"))
                            {
                                dict = ensureDict(allPedDrwDicts, hash);
                                dict[file.ShortNameHash] = file;
                            }
                            else if (file.NameLower.EndsWith(".ytd"))
                            {
                                dict = ensureDict(allPedTexDicts, hash);
                                dict[file.ShortNameHash] = file;
                            }
                            else if (file.NameLower.EndsWith(".yld"))
                            {
                                dict = ensureDict(allPedClothDicts, hash);
                                dict[file.ShortNameHash] = file;
                            }
                        }
                    }
                }
            });

            var addPedsFiles = new Action<IEnumerable<RpfFile>>((from) =>
            {
                foreach (RpfFile file in from)
                {
                    if (file.AllEntries == null) continue;
                    foreach (RpfEntry entry in file.AllEntries)
                    {
#if !DEBUG
                        try
#endif
                        {
                            if ((entry.NameLower == "peds.ymt") || (entry.NameLower == "peds.meta"))
                            {
                                var pf = RpfMan.GetFile<PedsFile>(entry);
                                if (pf.InitDataList?.InitDatas != null)
                                {
                                    foreach (var initData in pf.InitDataList.InitDatas)
                                    {
                                        var name = initData.Name.ToLowerInvariant();
                                        var hash = JenkHash.GenHash(name);
                                        if (allPeds.ContainsKey(hash))
                                        { }
                                        allPeds[hash] = initData;
                                    }
                                }
                                allPedsFiles.Add(pf);
                            }
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            string errstr = entry.Path + "\n" + ex.ToString();
                            ErrorLog(errstr);
                        }
#endif
                    }
                }
            });

            var addPedFiles = new Action<IEnumerable<RpfFile>>((from) =>
            {
                foreach (RpfFile file in from)
                {
                    if (file.AllEntries == null) continue;
                    foreach (RpfEntry entry in file.AllEntries)
                    {
#if !DEBUG
                        try
#endif
                        {
                            if (entry.NameLower.EndsWith(".ymt"))
                            {
                                var testname = entry.GetShortNameLower();
                                var testhash = JenkHash.GenHash(testname);
                                if (allPeds.ContainsKey(testhash))
                                {
                                    var pf = RpfMan.GetFile<PedFile>(entry);
                                    if (pf != null)
                                    {
                                        allPedYmts[testhash] = pf;
                                        addPedDicts(testname, testhash, entry.Parent);
                                    }
                                }
                            }
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            string errstr = entry.Path + "\n" + ex.ToString();
                            ErrorLog(errstr);
                        }
#endif
                    }
                }
            });



            addPedsFiles(rpfs);
            addPedsFiles(dlcrpfs);

            addPedFiles(rpfs);
            addPedFiles(dlcrpfs);



            PedsInitDict = allPeds;
            PedVariationsDict = allPedYmts;
            PedDrawableDicts = allPedDrwDicts;
            PedTextureDicts = allPedTexDicts;
            PedClothDicts = allPedClothDicts;


            foreach (var kvp in PedsInitDict)
            {
                if (!PedVariationsDict.ContainsKey(kvp.Key))
                { }//checking we found them all!
            }


        }


        public bool SetDlcLevel(string dlc, bool enable)
        {
            bool dlcchange = (dlc != SelectedDlc);
            bool enablechange = (enable != EnableDlc);
            bool change = (dlcchange && enable) || enablechange;

            if (change)
            {
                lock (updateSyncRoot)
                {
                    //lock (textureSyncRoot)
                    {
                        SelectedDlc = dlc;
                        EnableDlc = enable;

                        //mainCache.Clear();
                        ClearCachedMaps();

                        InitDlc();
                    }
                }
            }

            return change;
        }

        public bool SetModsEnabled(bool enable)
        {
            bool change = (enable != EnableMods);

            if (change)
            {
                lock (updateSyncRoot)
                {
                    //lock (textureSyncRoot)
                    {
                        EnableMods = enable;
                        RpfMan.EnableMods = enable;

                        mainCache.Clear();

                        InitGlobal();
                        InitDlc();
                    }
                }
            }

            return change;
        }


        private void ClearCachedMaps()
        {
            if (AllYmapsDict != null)
            {
                foreach (var ymap in AllYmapsDict.Values)
                {
                    GameFileCacheKey k = new GameFileCacheKey(ymap.ShortNameHash, GameFileType.Ymap);
                    mainCache.Remove(k);
                }
            }
        }



        public void TryLoadEnqueue(GameFile gf)
        {
            if (((!gf.Loaded)) && (requestQueue.Count < 10))// && (!gf.LoadQueued)
            {
                requestQueue.Enqueue(gf);
                gf.LoadQueued = true;
            }
        }


        public Archetype GetArchetype(uint hash)
        {
            if (!archetypesLoaded) return null;
            Archetype arch = null;
            archetypeDict.TryGetValue(hash, out arch);
            return arch;
        }
        public MapDataStoreNode GetMapNode(uint hash)
        {
            if (!IsInited) return null;
            MapDataStoreNode node = null;
            YmapHierarchyDict.TryGetValue(hash, out node);
            return node;
        }

        public YdrFile GetYdr(uint hash)
        {
            if (!IsInited) return null;
            lock (requestSyncRoot)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ydr);
                YdrFile ydr = mainCache.TryGet(key) as YdrFile;
                if (ydr == null)
                {
                    var e = GetYdrEntry(hash);
                    if (e != null)
                    {
                        ydr = new YdrFile(e);
                        if (mainCache.TryAdd(key, ydr))
                        {
                            TryLoadEnqueue(ydr);
                        }
                        else
                        {
                            ydr.LoadQueued = false;
                            //ErrorLog("Out of cache space - couldn't load drawable: " + JenkIndex.GetString(hash)); //too spammy...
                        }
                    }
                    else
                    {
                        //ErrorLog("Drawable not found: " + JenkIndex.GetString(hash)); //too spammy...
                    }
                }
                else if (!ydr.Loaded)
                {
                    TryLoadEnqueue(ydr);
                }
                return ydr;
            }
        }
        public YddFile GetYdd(uint hash)
        {
            if (!IsInited) return null;
            lock (requestSyncRoot)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ydd);
                YddFile ydd = mainCache.TryGet(key) as YddFile;
                if (ydd == null)
                {
                    var e = GetYddEntry(hash);
                    if (e != null)
                    {
                        ydd = new YddFile(e);
                        if (mainCache.TryAdd(key, ydd))
                        {
                            TryLoadEnqueue(ydd);
                        }
                        else
                        {
                            ydd.LoadQueued = false;
                            //ErrorLog("Out of cache space - couldn't load drawable dictionary: " + JenkIndex.GetString(hash)); //too spammy...
                        }
                    }
                    else
                    {
                        //ErrorLog("Drawable dictionary not found: " + JenkIndex.GetString(hash)); //too spammy...
                    }
                }
                else if (!ydd.Loaded)
                {
                    TryLoadEnqueue(ydd);
                }
                return ydd;
            }
        }
        public YtdFile GetYtd(uint hash)
        {
            if (!IsInited) return null;
            lock (requestSyncRoot)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ytd);
                YtdFile ytd = mainCache.TryGet(key) as YtdFile;
                if (ytd == null)
                {
                    var e = GetYtdEntry(hash);
                    if (e != null)
                    {
                        ytd = new YtdFile(e);
                        if (mainCache.TryAdd(key, ytd))
                        {
                            TryLoadEnqueue(ytd);
                        }
                        else
                        {
                            ytd.LoadQueued = false;
                            //ErrorLog("Out of cache space - couldn't load texture dictionary: " + JenkIndex.GetString(hash)); //too spammy...
                        }
                    }
                    else
                    {
                        //ErrorLog("Texture dictionary not found: " + JenkIndex.GetString(hash)); //too spammy...
                    }
                }
                else if (!ytd.Loaded)
                {
                    TryLoadEnqueue(ytd);
                }
                return ytd;
            }
        }
        public YmapFile GetYmap(uint hash)
        {
            if (!IsInited) return null;
            lock (requestSyncRoot)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ymap);
                YmapFile ymap = mainCache.TryGet(key) as YmapFile;
                if (ymap == null)
                {
                    var e = GetYmapEntry(hash);
                    if (e != null)
                    {
                        ymap = new YmapFile(e);
                        if (mainCache.TryAdd(key, ymap))
                        {
                            TryLoadEnqueue(ymap);
                        }
                        else
                        {
                            ymap.LoadQueued = false;
                            //ErrorLog("Out of cache space - couldn't load ymap: " + JenkIndex.GetString(hash));
                        }
                    }
                    else
                    {
                        //ErrorLog("Ymap not found: " + JenkIndex.GetString(hash)); //too spammy...
                    }
                }
                else if (!ymap.Loaded)
                {
                    TryLoadEnqueue(ymap);
                }
                return ymap;
            }
        }
        public YftFile GetYft(uint hash)
        {
            if (!IsInited) return null;
            lock (requestSyncRoot)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Yft);
                YftFile yft = mainCache.TryGet(key) as YftFile;
                if (yft == null)
                {
                    var e = GetYftEntry(hash);
                    if (e != null)
                    {
                        yft = new YftFile(e);
                        if (mainCache.TryAdd(key, yft))
                        {
                            TryLoadEnqueue(yft);
                        }
                        else
                        {
                            yft.LoadQueued = false;
                            //ErrorLog("Out of cache space - couldn't load yft: " + JenkIndex.GetString(hash)); //too spammy...
                        }
                    }
                    else
                    {
                        //ErrorLog("Yft not found: " + JenkIndex.GetString(hash)); //too spammy...
                    }
                }
                else if (!yft.Loaded)
                {
                    TryLoadEnqueue(yft);
                }
                return yft;
            }
        }
        public YbnFile GetYbn(uint hash)
        {
            if (!IsInited) return null;
            lock (requestSyncRoot)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ybn);
                YbnFile ybn = mainCache.TryGet(key) as YbnFile;
                if (ybn == null)
                {
                    var e = GetYbnEntry(hash);
                    if (e != null)
                    {
                        ybn = new YbnFile(e);
                        if (mainCache.TryAdd(key, ybn))
                        {
                            TryLoadEnqueue(ybn);
                        }
                        else
                        {
                            ybn.LoadQueued = false;
                            //ErrorLog("Out of cache space - couldn't load ybn: " + JenkIndex.GetString(hash)); //too spammy...
                        }
                    }
                    else
                    {
                        //ErrorLog("Ybn not found: " + JenkIndex.GetString(hash)); //too spammy...
                    }
                }
                else if (!ybn.Loaded)
                {
                    TryLoadEnqueue(ybn);
                }
                return ybn;
            }
        }
        public YcdFile GetYcd(uint hash)
        {
            if (!IsInited) return null;
            lock (requestSyncRoot)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ycd);
                YcdFile ycd = mainCache.TryGet(key) as YcdFile;
                if (ycd == null)
                {
                    var e = GetYcdEntry(hash);
                    if (e != null)
                    {
                        ycd = new YcdFile(e);
                        if (mainCache.TryAdd(key, ycd))
                        {
                            TryLoadEnqueue(ycd);
                        }
                        else
                        {
                            ycd.LoadQueued = false;
                            //ErrorLog("Out of cache space - couldn't load ycd: " + JenkIndex.GetString(hash)); //too spammy...
                        }
                    }
                    else
                    {
                        //ErrorLog("Ycd not found: " + JenkIndex.GetString(hash)); //too spammy...
                    }
                }
                else if (!ycd.Loaded)
                {
                    TryLoadEnqueue(ycd);
                }
                return ycd;
            }
        }
        public YnvFile GetYnv(uint hash)
        {
            if (!IsInited) return null;
            lock (requestSyncRoot)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ynv);
                YnvFile ynv = mainCache.TryGet(key) as YnvFile;
                if (ynv == null)
                {
                    var e = GetYnvEntry(hash);
                    if (e != null)
                    {
                        ynv = new YnvFile(e);
                        if (mainCache.TryAdd(key, ynv))
                        {
                            TryLoadEnqueue(ynv);
                        }
                        else
                        {
                            ynv.LoadQueued = false;
                            //ErrorLog("Out of cache space - couldn't load ycd: " + JenkIndex.GetString(hash)); //too spammy...
                        }
                    }
                    else
                    {
                        //ErrorLog("Ycd not found: " + JenkIndex.GetString(hash)); //too spammy...
                    }
                }
                else if (!ynv.Loaded)
                {
                    TryLoadEnqueue(ynv);
                }
                return ynv;
            }
        }


        public RpfFileEntry GetYdrEntry(uint hash)
        {
            RpfFileEntry entry;
            YdrDict.TryGetValue(hash, out entry);
            return entry;
        }
        public RpfFileEntry GetYddEntry(uint hash)
        {
            RpfFileEntry entry;
            YddDict.TryGetValue(hash, out entry);
            return entry;
        }
        public RpfFileEntry GetYtdEntry(uint hash)
        {
            RpfFileEntry entry;
            YtdDict.TryGetValue(hash, out entry);
            return entry;
        }
        public RpfFileEntry GetYmapEntry(uint hash)
        {
            RpfFileEntry entry;
            if (!YmapDict.TryGetValue(hash, out entry))
            {
                AllYmapsDict.TryGetValue(hash, out entry);
            }
            return entry;
        }
        public RpfFileEntry GetYftEntry(uint hash)
        {
            RpfFileEntry entry;
            YftDict.TryGetValue(hash, out entry);
            return entry;
        }
        public RpfFileEntry GetYbnEntry(uint hash)
        {
            RpfFileEntry entry;
            YbnDict.TryGetValue(hash, out entry);
            return entry;
        }
        public RpfFileEntry GetYcdEntry(uint hash)
        {
            RpfFileEntry entry;
            YcdDict.TryGetValue(hash, out entry);
            return entry;
        }
        public RpfFileEntry GetYnvEntry(uint hash)
        {
            RpfFileEntry entry;
            YnvDict.TryGetValue(hash, out entry);
            return entry;
        }



        public bool LoadFile<T>(T file) where T : GameFile, PackedFile
        {
            if (file == null) return false;
            RpfFileEntry entry = file.RpfFileEntry;
            if (entry != null)
            {
                return RpfMan.LoadFile(file, entry);
            }
            return false;
        }


        public T GetFileUncached<T>(RpfFileEntry e) where T : GameFile, new()
        {
            var f = new T();
            f.RpfFileEntry = e;
            TryLoadEnqueue(f);
            return f;
        }


        public void BeginFrame()
        {
            lock (requestSyncRoot)
            {
                mainCache.BeginFrame();
            }
        }


        public bool ContentThreadProc()
        {
            Monitor.Enter(updateSyncRoot);

            GameFile req;
            //bool loadedsomething = false;

            int itemcount = 0;

            while (requestQueue.TryDequeue(out req) && (itemcount < MaxItemsPerLoop))
            {
                //process content requests.
                if (req.Loaded)
                    continue; //it's already loaded... (somehow)

                if ((req.LastUseTime - DateTime.Now).TotalSeconds > 0.5)
                    continue; //hasn't been requested lately..! ignore, will try again later if necessary

                itemcount++;
                //if (!loadedsomething)
                //{
                //UpdateStatus("Loading " + req.RpfFileEntry.Name + "...");
                //}

                switch (req.Type)
                {
                    case GameFileType.Ydr:
                        req.Loaded = LoadFile(req as YdrFile);
                        break;
                    case GameFileType.Ydd:
                        req.Loaded = LoadFile(req as YddFile);
                        break;
                    case GameFileType.Ytd:
                        req.Loaded = LoadFile(req as YtdFile);
                        //if (req.Loaded) AddTextureLookups(req as YtdFile);
                        break;
                    case GameFileType.Ymap:
                        YmapFile y = req as YmapFile;
                        req.Loaded = LoadFile(y);
                        if (req.Loaded) y.InitYmapEntityArchetypes(this);
                        break;
                    case GameFileType.Yft:
                        req.Loaded = LoadFile(req as YftFile);
                        break;
                    case GameFileType.Ybn:
                        req.Loaded = LoadFile(req as YbnFile);
                        break;
                    case GameFileType.Ycd:
                        req.Loaded = LoadFile(req as YcdFile);
                        break;
                    case GameFileType.Ynv:
                        req.Loaded = LoadFile(req as YnvFile);
                        break;
                    case GameFileType.Yld:
                        req.Loaded = LoadFile(req as YldFile);
                        break;
                    default:
                        break;
                }

                string str = (req.Loaded ? "Loaded " : "Error loading ") + req.ToString();
                //string str = string.Format("{0}: {1}: {2}", requestQueue.Count, (req.Loaded ? "Loaded" : "Error loading"), req);

                UpdateStatus(str);
                //ErrorLog(str);
                if (!req.Loaded)
                {
                    ErrorLog("Error loading " + req.ToString());
                }


                //loadedsomething = true;
            }

            //whether or not we need another content thread loop
            bool itemsStillPending = (itemcount >= MaxItemsPerLoop);


            Monitor.Exit(updateSyncRoot);


            return itemsStillPending;
        }






        private void AddTextureLookups(YtdFile ytd)
        {
            if (ytd?.TextureDict?.TextureNameHashes?.data_items == null) return;

            lock (textureSyncRoot)
            {
                foreach (uint hash in ytd.TextureDict.TextureNameHashes.data_items)
                {
                    textureLookup[hash] = ytd.RpfFileEntry;
                }

            }
        }
        public YtdFile TryGetTextureDictForTexture(uint hash)
        {
            lock (textureSyncRoot)
            {
                RpfFileEntry e;
                if (textureLookup.TryGetValue(hash, out e))
                {
                    return GetYtd(e.ShortNameHash);
                }

            }
            return null;
        }
        public YtdFile TryGetParentYtd(uint hash)
        {
            MetaHash phash;
            if (textureParents.TryGetValue(hash, out phash))
            {
                return GetYtd(phash);
            }
            return null;
        }
        public uint TryGetParentYtdHash(uint hash)
        {
            MetaHash phash = 0;
            textureParents.TryGetValue(hash, out phash);
            return phash;
        }
        public uint TryGetHDTextureHash(uint txdhash)
        {
            MetaHash hdhash = 0;
            if (hdtexturelookup?.TryGetValue(txdhash, out hdhash) ?? false)
            {
                return hdhash;
            }
            return txdhash;
        }

        public Texture TryFindTextureInParent(uint texhash, uint txdhash)
        {
            Texture tex = null;

            var ytd = TryGetParentYtd(txdhash);
            while ((ytd != null) && (tex == null))
            {
                if (ytd.Loaded && (ytd.TextureDict != null))
                {
                    tex = ytd.TextureDict.Lookup(texhash);
                }
                if (tex == null)
                {
                    ytd = TryGetParentYtd(ytd.Key.Hash);
                }
            }

            return tex;
        }








        public DrawableBase TryGetDrawable(Archetype arche)
        {
            if (arche == null) return null;
            uint drawhash = arche.Hash;
            DrawableBase drawable = null;
            if ((arche.DrawableDict != 0))// && (arche.DrawableDict != arche.Hash))
            {
                //try get drawable from ydd...
                YddFile ydd = GetYdd(arche.DrawableDict);
                if (ydd != null)
                {
                    if (ydd.Loaded && (ydd.Dict != null))
                    {
                        Drawable d;
                        ydd.Dict.TryGetValue(drawhash, out d); //can't out to base class?
                        drawable = d;
                        if (drawable == null)
                        {
                            return null; //drawable wasn't in dict!!
                        }
                    }
                    else
                    {
                        return null; //ydd not loaded yet, or has no dict
                    }
                }
                else
                {
                    //return null; //couldn't find drawable dict... quit now?
                }
            }
            if (drawable == null)
            {
                //try get drawable from ydr.
                YdrFile ydr = GetYdr(drawhash);
                if (ydr != null)
                {
                    if (ydr.Loaded)
                    {
                        drawable = ydr.Drawable;
                    }
                }
                else
                {
                    YftFile yft = GetYft(drawhash);
                    if (yft != null)
                    {
                        if (yft.Loaded)
                        {
                            if (yft.Fragment != null)
                            {
                                drawable = yft.Fragment.Drawable;
                            }
                        }
                    }
                }
            }

            return drawable;
        }

        public DrawableBase TryGetDrawable(Archetype arche, out bool waitingForLoad)
        {
            waitingForLoad = false;
            if (arche == null) return null;
            uint drawhash = arche.Hash;
            DrawableBase drawable = null;
            if ((arche.DrawableDict != 0))// && (arche.DrawableDict != arche.Hash))
            {
                //try get drawable from ydd...
                YddFile ydd = GetYdd(arche.DrawableDict);
                if (ydd != null)
                {
                    if (ydd.Loaded)
                    {
                        if (ydd.Dict != null)
                        {
                            Drawable d;
                            ydd.Dict.TryGetValue(drawhash, out d); //can't out to base class?
                            drawable = d;
                            if (drawable == null)
                            {
                                return null; //drawable wasn't in dict!!
                            }
                        }
                        else
                        {
                            return null; //ydd has no dict
                        }
                    }
                    else
                    {
                        waitingForLoad = true;
                        return null; //ydd not loaded yet
                    }
                }
                else
                {
                    //return null; //couldn't find drawable dict... quit now?
                }
            }
            if (drawable == null)
            {
                //try get drawable from ydr.
                YdrFile ydr = GetYdr(drawhash);
                if (ydr != null)
                {
                    if (ydr.Loaded)
                    {
                        drawable = ydr.Drawable;
                    }
                    else
                    {
                        waitingForLoad = true;
                    }
                }
                else
                {
                    YftFile yft = GetYft(drawhash);
                    if (yft != null)
                    {
                        if (yft.Loaded)
                        {
                            if (yft.Fragment != null)
                            {
                                drawable = yft.Fragment.Drawable;
                            }
                        }
                        else
                        {
                            waitingForLoad = true;
                        }
                    }
                }
            }

            return drawable;
        }










        private string[] GetExcludePaths()
        {
            string[] exclpaths = ExcludeFolders.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (exclpaths.Length > 0)
            {
                for (int i = 0; i < exclpaths.Length; i++)
                {
                    exclpaths[i] = exclpaths[i].ToLowerInvariant();
                }
            }
            else
            {
                exclpaths = null;
            }
            return exclpaths;
        }







        public void TestAudioRels()
        {
            UpdateStatus("Testing Audio REL files");


            bool savetest = true;
            bool xmltest = true;

            foreach (RpfFile rpf in RpfMan.AllRpfs)
            {
                foreach (RpfEntry entry in rpf.AllEntries)
                {
                    var rfe = entry as RpfFileEntry;
                    var rbfe = rfe as RpfBinaryFileEntry;
                    if ((rfe == null) || (rbfe == null)) continue;

                    if (rfe.NameLower.EndsWith(".rel"))
                    {
                        UpdateStatus(string.Format(entry.Path));

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
                                if (rel3.RelDatasSorted?.Length != rel.RelDatasSorted?.Length)
                                { } //check nothing went missing...


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
                        var n = entry.NameLower;
                        if (n.EndsWith(".ymt"))
                        {
                            UpdateStatus(string.Format(entry.Path));
                            //YmtFile ymtfile = RpfMan.GetFile<YmtFile>(entry);
                            //if ((ymtfile != null))
                            //{
                            //}

                            var sn = entry.GetShortName();
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
                        UpdateStatus("Error! " + ex.ToString());
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
                        var n = entry.NameLower;
                        //if (n.EndsWith(".ymap"))
                        //{
                        //    UpdateStatus(string.Format(entry.Path));
                        //    YmapFile ymapfile = RpfMan.GetFile<YmapFile>(entry);
                        //    if ((ymapfile != null) && (ymapfile.Meta != null))
                        //    {
                        //        MetaTypes.EnsureMetaTypes(ymapfile.Meta);
                        //    }
                        //}
                        //else if (n.EndsWith(".ytyp"))
                        //{
                        //    UpdateStatus(string.Format(entry.Path));
                        //    YtypFile ytypfile = RpfMan.GetFile<YtypFile>(entry);
                        //    if ((ytypfile != null) && (ytypfile.Meta != null))
                        //    {
                        //        MetaTypes.EnsureMetaTypes(ytypfile.Meta);
                        //    }
                        //}
                        //else if (n.EndsWith(".ymt"))
                        //{
                        //    UpdateStatus(string.Format(entry.Path));
                        //    YmtFile ymtfile = RpfMan.GetFile<YmtFile>(entry);
                        //    if ((ymtfile != null) && (ymtfile.Meta != null))
                        //    {
                        //        MetaTypes.EnsureMetaTypes(ymtfile.Meta);
                        //    }
                        //}


                        if (n.EndsWith(".ymap") || n.EndsWith(".ytyp") || n.EndsWith(".ymt"))
                        {
                            var rfe = entry as RpfResourceFileEntry;
                            if (rfe == null) continue;

                            UpdateStatus(string.Format(entry.Path));

                            var data = rfe.File.ExtractFile(rfe);
                            ResourceDataReader rd = new ResourceDataReader(rfe, data);
                            var meta = rd.ReadBlock<Meta>();
                            var xml = MetaXml.GetXml(meta);
                            var xdoc = new XmlDocument();
                            xdoc.LoadXml(xml);
                            var meta2 = XmlMeta.GetMeta(xdoc);
                            var xml2 = MetaXml.GetXml(meta2);

                            if (xml.Length != xml2.Length)
                            { }
                            if ((xml != xml2) && (!n.EndsWith("srl.ymt") && !n.StartsWith("des_")))
                            { }

                        }


                    }
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus("Error! " + ex.ToString());
                    //}
                }
            }

            string str = MetaTypes.GetTypesInitString();

        }
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
                        var n = entry.NameLower;
                        if (!(n.EndsWith(".pso") ||
                              n.EndsWith(".ymt") ||
                              n.EndsWith(".ymf") ||
                              n.EndsWith(".ymap") ||
                              n.EndsWith(".ytyp") ||
                              n.EndsWith(".cut")))
                            continue; //PSO files seem to only have these extensions

                        var fentry = entry as RpfFileEntry;
                        var data = entry.File.ExtractFile(fentry);
                        if (data != null)
                        {
                            using (MemoryStream ms = new MemoryStream(data))
                            {
                                if (PsoFile.IsPSO(ms))
                                {
                                    UpdateStatus(string.Format(entry.Path));

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
                        UpdateStatus("Error! " + ex.ToString());
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
        public void TestRbfs()
        {
            var exceptions = new List<Exception>();
            var allrbfs = new List<string>();
            var diffrbfs = new List<string>();

            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    var n = entry.NameLower;
                    if (!(n.EndsWith(".ymt") ||
                          n.EndsWith(".ymf") ||
                          n.EndsWith(".ymap") ||
                          n.EndsWith(".ytyp") ||
                          n.EndsWith(".cut")))
                        continue; //PSO files seem to only have these extensions

                    var fentry = entry as RpfFileEntry;
                    var data = entry.File.ExtractFile(fentry);
                    if (data != null)
                    {
                        using (MemoryStream ms = new MemoryStream(data))
                        {
                            if (RbfFile.IsRBF(ms))
                            {
                                UpdateStatus(string.Format(entry.Path));

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

                        if (rfe.NameLower.EndsWith(".cut"))
                        {
                            UpdateStatus(string.Format(entry.Path));

                            CutFile cut = new CutFile(rfe);
                            RpfMan.LoadFile(cut, rfe);

                            //PsoTypes.EnsurePsoTypes(cut.Pso);
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        UpdateStatus("Error! " + ex.ToString());
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

                        if (rfe.NameLower.EndsWith(".yld"))
                        {
                            UpdateStatus(string.Format(entry.Path));

                            YldFile yld = new YldFile(rfe);
                            RpfMan.LoadFile(yld, rfe);

                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        UpdateStatus("Error! " + ex.ToString());
                        exceptions.Add(ex);
                    }
#endif
                }
            }

            if (exceptions.Count > 0)
            { }
        }
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
                    if (entry.NameLower.EndsWith(".ycd"))
                    {
                        UpdateStatus(string.Format(entry.Path));
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
                    //    UpdateStatus(string.Format(entry.Path));
                    //    AwcFile awcfile = RpfMan.GetFile<AwcFile>(entry);
                    //    if ((awcfile != null))
                    //    { }
                    //}
                    //}
                //catch (Exception ex)
                //{
                //    UpdateStatus("Error! " + ex.ToString());
                //}
                }
            }

            if (errorfiles.Count > 0)
            { }

        }
        public void TestYtds()
        {
            bool savetest = false;
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.NameLower.EndsWith(".ytd"))
                        {
                            UpdateStatus(string.Format(entry.Path));
                            YtdFile ytdfile = null;
                            try
                            {
                                ytdfile = RpfMan.GetFile<YtdFile>(entry);
                            }
                            catch(Exception ex)
                            {
                                UpdateStatus("Error! " + ex.ToString());
                                errorfiles.Add(entry);
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
                    //    UpdateStatus("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count > 0)
            { }
        }
        public void TestYbns()
        {
            bool savetest = false;
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.NameLower.EndsWith(".ybn"))
                        {
                            UpdateStatus(string.Format(entry.Path));
                            YbnFile ybn = null;
                            try
                            {
                                ybn = RpfMan.GetFile<YbnFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus("Error! " + ex.ToString());
                                errorfiles.Add(entry);
                            }
                            if (savetest && (ybn != null) && (ybn.Bounds != null))
                            {
                                var fentry = entry as RpfFileEntry;
                                if (fentry == null)
                                { continue; } //shouldn't happen

                                var bytes = ybn.Save();

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
                    //    UpdateStatus("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count > 0)
            { }
        }
        public void TestYdrs()
        {
            bool savetest = false;
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.NameLower.EndsWith(".ydr"))
                        {
                            UpdateStatus(string.Format(entry.Path));
                            YdrFile ydr = null;
                            try
                            {
                                ydr = RpfMan.GetFile<YdrFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus("Error! " + ex.ToString());
                                errorfiles.Add(entry);
                            }
                            if (savetest && (ydr != null) && (ydr.Drawable != null))
                            {
                                var fentry = entry as RpfFileEntry;
                                if (fentry == null)
                                { continue; } //shouldn't happen

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
                    //    UpdateStatus("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count > 0)
            { }
        }
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
                        if (entry.NameLower.EndsWith(".ydd"))
                        {
                            UpdateStatus(string.Format(entry.Path));
                            YddFile ydd = null;
                            try
                            {
                                ydd = RpfMan.GetFile<YddFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus("Error! " + ex.ToString());
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
                        }
                    }
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count > 0)
            { }
        }
        public void TestYfts()
        {
            bool savetest = false;
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.NameLower.EndsWith(".yft"))
                        {
                            UpdateStatus(string.Format(entry.Path));
                            YftFile yft = null;
                            try
                            {
                                yft = RpfMan.GetFile<YftFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus("Error! " + ex.ToString());
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
                        }
                    }
                    //catch (Exception ex)
                    //{
                    //    UpdateStatus("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count > 0)
            { }
        }
        public void TestYpts()
        {
            var errorfiles = new List<RpfEntry>();
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    //try
                    {
                        if (entry.NameLower.EndsWith(".ypt"))
                        {
                            UpdateStatus(string.Format(entry.Path));
                            YptFile ypt = null;
                            try
                            {
                                ypt = RpfMan.GetFile<YptFile>(entry);
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus("Error! " + ex.ToString());
                                errorfiles.Add(entry);
                            }
                            if ((ypt != null) && (ypt.PtfxList != null))
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
                    //    UpdateStatus("Error! " + ex.ToString());
                    //}
                }
            }
            if (errorfiles.Count > 0)
            { }
        }
        public void TestYmaps()
        {
            foreach (RpfFile file in AllRpfs)
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    try
                    {
                        if (entry.NameLower.EndsWith(".ymap"))
                        {
                            UpdateStatus(string.Format(entry.Path));
                            YmapFile ymapfile = RpfMan.GetFile<YmapFile>(entry);
                            if ((ymapfile != null))// && (ymapfile.Meta != null))
                            { }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus("Error! " + ex.ToString());
                    }
                }
            }
        }
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
            //                UpdateStatus(string.Format(entry.Path));
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

            //UpdateStatus("Ymap scan finished.");

            //sbtest.AppendLine();
            //sbtest.AppendLine(totplacements.ToString() + " total CEntityDef placements parsed");
            //sbtest.AppendLine(tottimedplacements.ToString() + " total CTimeArchetypeDef placements");
            //sbtest.AppendLine(totaudioplacements.ToString() + " total CExtensionDefAudioEmitter placements");

            //string teststr = sbtest.ToString();
            //string testerr = sbterr.ToString();

            //return;
        }
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
                        if (doydr && entry.NameLower.EndsWith(".ydr"))
                        {
                            UpdateStatus(entry.Path);
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
                        else if (doydd & entry.NameLower.EndsWith(".ydd"))
                        {
                            UpdateStatus(entry.Path);
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
                        else if (doyft && entry.NameLower.EndsWith(".yft"))
                        {
                            UpdateStatus(entry.Path);
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
                            if ((yft.Fragment.Clothes != null) && (yft.Fragment.Clothes.data_items != null))
                            {
                                foreach (var cloth in yft.Fragment.Clothes.data_items)
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
                        uint type = (uint)((vd.Types >> (4 * i)) & 0xF);
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

            UpdateStatus((DateTime.Now - starttime).ToString() + " elapsed, " + drawablecount.ToString() + " drawables, " + errs.Count.ToString() + " errors.");

        }
        public void GetArchetypeTimesList()
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name,AssetName,12am,1am,2am,3am,4am,5am,6am,7am,8am,9am,10am,11am,12pm,1pm,2pm,3pm,4pm,5pm,6pm,7pm,8pm,9pm,10pm,11pm,+12am,+1am,+2am,+3am,+4am,+5am,+6am,+7am");
            foreach (var ytyp in YtypDict.Values)
            {
                foreach (var arch in ytyp.AllArchetypes)
                {
                    if (arch.Type == MetaName.CTimeArchetypeDef)
                    {
                        var ta = arch as TimeArchetype;
                        var t = ta.TimeFlags;
                        sb.Append(arch.Name);
                        sb.Append(",");
                        sb.Append(arch.AssetName);
                        sb.Append(",");
                        for (int i = 0; i < 32; i++)
                        {
                            bool v = ((t >> i) & 1) == 1;
                            sb.Append(v ? "1" : "0");
                            sb.Append(",");
                        }
                        sb.AppendLine();
                    }
                }
            }

            var csv = sb.ToString();



        }

    }


}
