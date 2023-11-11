using CodeWalker.Core.Utils;
using CodeWalker.World;
using Dasync.Collections;
using SharpDX;
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
    public partial class GameFileCache
    {
        public RpfManager RpfMan;
        public event Action<string> UpdateStatus;
        public event Action<string> ErrorLog;
        public int MaxItemsPerLoop = 3; //to keep things flowing...

        private ConcurrentQueue<GameFile> requestQueue = new ConcurrentQueue<GameFile>();

        ////dynamic cache
        private Cache<GameFileCacheKey, GameFile> mainCache;
        public volatile bool IsInited = false;

        private volatile bool archetypesLoaded = false;
        private ConcurrentDictionary<uint, Archetype> archetypeDict = new ConcurrentDictionary<uint, Archetype>();
        private ConcurrentDictionary<uint, RpfFileEntry> textureLookup = new ConcurrentDictionary<uint, RpfFileEntry>();
        private Dictionary<MetaHash, MetaHash> textureParents;
        private Dictionary<MetaHash, MetaHash> hdtexturelookup;

        private object updateSyncRoot = new object();
        private object requestSyncRoot = new object();
        private ReaderWriterLockSlim archetypeLock = new ReaderWriterLockSlim();

        private ConcurrentDictionary<GameFileCacheKey, GameFile> projectFiles = new ConcurrentDictionary<GameFileCacheKey, GameFile>(); //for cache files loaded in project window: ydr,ydd,ytd,yft
        private Dictionary<uint, Archetype> projectArchetypes = new Dictionary<uint, Archetype>(); //used to override archetypes in world view with project ones




        //static indexes
        public Dictionary<uint, RpfFileEntry> YdrDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YddDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YtdDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YmapDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YftDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YbnDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YcdDict { get; private set; }
        public Dictionary<uint, RpfFileEntry> YedDict { get; private set; }
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


        public List<RelFile> AudioDatRelFiles = new List<RelFile>();
        public Dictionary<MetaHash, RelData> AudioConfigDict = new Dictionary<MetaHash, RelData>();
        public Dictionary<MetaHash, RelData> AudioSpeechDict = new Dictionary<MetaHash, RelData>();
        public Dictionary<MetaHash, RelData> AudioSynthsDict = new Dictionary<MetaHash, RelData>();
        public Dictionary<MetaHash, RelData> AudioMixersDict = new Dictionary<MetaHash, RelData>();
        public Dictionary<MetaHash, RelData> AudioCurvesDict = new Dictionary<MetaHash, RelData>();
        public Dictionary<MetaHash, RelData> AudioCategsDict = new Dictionary<MetaHash, RelData>();
        public Dictionary<MetaHash, RelData> AudioSoundsDict = new Dictionary<MetaHash, RelData>();
        public Dictionary<MetaHash, RelData> AudioGameDict = new Dictionary<MetaHash, RelData>();



        public List<RpfFile> BaseRpfs { get; private set; }
        public List<RpfFile> AllRpfs { get; private set; }
        public List<RpfFile> DlcRpfs { get; private set; }

        public bool DoFullStringIndex = false;
        public bool BuildExtendedJenkIndex = true;
        public bool LoadArchetypes = true;
        public bool LoadVehicles = true;
        public bool LoadPeds = true;
        public bool LoadAudio = true;

        public bool PedsLoaded = false;
        public bool VehiclesLoaded = false;

        private bool PreloadedMode = true;

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

        public long MaxMemoryUsage
        {
            get
            {
                return mainCache.MaxMemoryUsage;
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

            while (requestQueue.TryDequeue(out _))
            { } //empty the old queue out...
        }

        public void SetGtaFolder(string folder)
        {
            Clear();

            GTAFolder = folder;
        }

        public volatile bool IsIniting = false;
        public void Init(Action<string> updateStatus = null, Action<string> errorLog = null, bool force = true)
        {
            if (IsIniting)
            {
                return;
            }
            IsIniting = true;
            try
            {
                using var _ = new DisposableTimer("GameFileCache.Init");
                if (updateStatus != null) UpdateStatus += updateStatus;
                if (errorLog != null) ErrorLog += errorLog;


                if (RpfMan == null)
                {
                    //EnableDlc = !string.IsNullOrEmpty(SelectedDlc);

                    RpfMan = RpfManager.GetInstance();
                    RpfMan.ExcludePaths = GetExcludePaths();
                    RpfMan.EnableMods = EnableMods;
                    RpfMan.BuildExtendedJenkIndex = BuildExtendedJenkIndex;
                    RpfMan.Init(GTAFolder, UpdateStatus, ErrorLog);//, true);


                    InitGlobal();

                    InitDlc();




                    //RE test area!
                    //TestAudioRels();
                    //TestAudioYmts();
                    //TestAudioAwcs();
                    //TestMetas();
                    //TestPsos();
                    //TestRbfs();
                    //TestCuts();
                    //TestYlds();
                    //TestYeds();
                    //TestYcds();
                    //TestYtds();
                    //TestYbns();
                    //TestYdrs();
                    //TestYdds();
                    //TestYfts();
                    //TestYpts();
                    //TestYnvs();
                    //TestYvrs();
                    //TestYwrs();
                    //TestYmaps();
                    //TestYpdbs();
                    //TestYfds();
                    //TestMrfs();
                    //TestFxcs();
                    //TestPlacements();
                    //TestDrawables();
                    //TestCacheFiles();
                    //TestHeightmaps();
                    //TestWatermaps();
                    //GetShadersXml();
                    //GetArchetypeTimesList();
                    //string typestr = PsoTypes.GetTypesString();
                }
                else
                {
                    //GC.Collect(); //try free up some of the previously used memory..
                }

                UpdateStatus?.Invoke("Scan complete");


                IsInited = true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                IsIniting = false;
            }
        }
        public void Init(Action<string> updateStatus, Action<string> errorLog, List<RpfFile> allRpfs, bool force = true)
        {
            if (IsIniting)
            {
                return;
            }
            IsIniting = true;
            try
            {
                using var _ = new DisposableTimer("GameFileCache.Init");
                if (updateStatus != null)
                {
                    UpdateStatus += updateStatus;
                }
                if (errorLog != null)
                {
                    ErrorLog += errorLog;
                }
                Clear();

                PreloadedMode = true;
                EnableDlc = true;//just so everything (mainly archetypes) will load..
                EnableMods = false;
                RpfMan ??= RpfManager.GetInstance();
                RpfMan.Init(allRpfs);


                AllRpfs = allRpfs;
                BaseRpfs = allRpfs;
                DlcRpfs = new List<RpfFile>();


                Task.WhenAll(
                    Task.Run(() => InitGlobalDicts(force)),
                    Task.Run(() => InitManifestDicts(force)),
                    Task.Run(() => InitGtxds(force)),
                    Task.Run(() => InitArchetypeDicts(force)),
                    Task.Run(() => InitStringDictsAsync(force)),
                    Task.Run(() => InitAudio(force))
                ).GetAwaiter().GetResult();

                vehicleFiles.Clear();

                IsInited = true;
            }
            catch (Exception ex) {
                ErrorLog?.Invoke(ex.ToString());
                throw;
            }
            finally
            {
                IsIniting = false;
            }
        }

        private void InitGlobal()
        {
            using var _ = new DisposableTimer("InitGlobal");
            BaseRpfs = GetModdedRpfList(RpfMan.BaseRpfs);
            AllRpfs = GetModdedRpfList(RpfMan.AllRpfs);
            DlcRpfs = GetModdedRpfList(RpfMan.DlcRpfs);

            InitGlobalDicts();
        }

        private void InitDlc(bool force = true)
        {
            InitDlcList();

            InitActiveMapRpfFiles();

            Task.WhenAll(
                Task.Run(() => {
                    InitMapDicts(force);
                    InitMapCaches(force);
                }),
                Task.Run(() => InitManifestDicts(force)),
                Task.Run(() => InitArchetypeDicts(force)),
                Task.Run(() => InitStringDictsAsync(force)),
                Task.Run(() => InitVehicles(force)),
                Task.Run(() => InitPeds(force)),
                Task.Run(() => InitAudio(force)),
                Task.Run(() => InitGtxds(force))
            ).GetAwaiter().GetResult();

            vehicleFiles.Clear();
        }

        private void InitDlcList()
        {
            UpdateStatus?.Invoke("Building DLC List...");
            using var _ = new DisposableTimer("InitDlcList");
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
            Dictionary<string, RpfFile> dlcDict = new Dictionary<string, RpfFile>(80, StringComparer.OrdinalIgnoreCase);
            Dictionary<string, RpfFile> dlcDict2 = new Dictionary<string, RpfFile>(80, StringComparer.OrdinalIgnoreCase);
            foreach (RpfFile dlcrpf in DlcRpfs)
            {
                if (dlcrpf == null)
                    continue;
                if (dlcrpf.Name.Equals("dlc.rpf", StringComparison.OrdinalIgnoreCase))
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
                            if (frelpath.StartsWith("mods\\", StringComparison.OrdinalIgnoreCase))
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
                DlcNameList.Add(GetDlcNameFromPath(sfile.DlcFile.Path).ToLowerInvariant());
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
            UpdateStatus?.Invoke("Building active RPF dictionary...");
            using var _ = new DisposableTimer("InitActiveMapRpfFiles");
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
                if (rpf.Name.Equals("update.rpf", StringComparison.OrdinalIgnoreCase))//include this so that files not in child rpf's can be used..
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
                    if ((dlcname == "patchday27ng") && (!SelectedDlc.Equals(dlcname, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue; //hack to fix map getting completely broken by this DLC.. but why? need to investigate further!
                    }



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




                    if (dlcname.Equals(SelectedDlc, StringComparison.OrdinalIgnoreCase))
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
            if (path.StartsWith("mods/", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(5);
            }
            if (path.Length > 7)
            {
                path = path.Substring(0, path.Length - 7);//trim off "dlc.rpf"
            }
            if (path.StartsWith("x64", StringComparison.OrdinalIgnoreCase))
            {
                int bsind = path.IndexOf('/'); //replace x64*.rpf
                if ((bsind > 0) && (bsind < path.Length))
                {
                    path = "x64" + path.Substring(bsind);
                }
                else
                { } //no hits here
            }
            else if (path.StartsWith("update/x64/dlcpacks", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Replace("update/x64/dlcpacks", "dlcpacks:");
            }
            else
            { } //no hits here

            return path;
        }
        private string GetDlcNameFromPath(string path)
        {
            string[] parts = path.Split('\\');
            if (parts.Length > 1)
            {
                return parts[parts.Length - 2];
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
                        if (path.StartsWith(fm.path, StringComparison.OrdinalIgnoreCase))
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
                        if (path.StartsWith(fm.mountAs, StringComparison.OrdinalIgnoreCase))
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
                    if (!file.Path.StartsWith("mods", StringComparison.OrdinalIgnoreCase))
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
                        if (file.Path.StartsWith("mods", StringComparison.OrdinalIgnoreCase))
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


        private void InitGlobalDicts(bool force = false)
        {
            UpdateStatus?.Invoke("Building global dictionaries...");
            using var timer = new DisposableTimer("InitGlobalDicts");
            YdrDict ??= new Dictionary<uint, RpfFileEntry>(80000);
            YddDict ??= new Dictionary<uint, RpfFileEntry>(11000);
            YtdDict ??= new Dictionary<uint, RpfFileEntry>(40000);
            YftDict ??= new Dictionary<uint, RpfFileEntry>(40000);
            YcdDict ??= new Dictionary<uint, RpfFileEntry>(20000);
            YedDict ??= new Dictionary<uint, RpfFileEntry>(300);
            foreach (var rpffile in AllRpfs)
            {
                if (rpffile.AllEntries == null) continue;
                foreach (var entry in rpffile.AllEntries)
                {
                    if (entry is RpfFileEntry fentry)
                    {
                        if (entry.IsExtension(".ydr"))
                        {
                            YdrDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.IsExtension(".ydd"))
                        {
                            YddDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.IsExtension(".ytd"))
                        {
                            YtdDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.IsExtension(".yft"))
                        {
                            YftDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.IsExtension(".ycd"))
                        {
                            YcdDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.IsExtension(".yed"))
                        {
                            YedDict[entry.ShortNameHash] = fentry;
                        }
                    }
                }
            }
            Console.WriteLine($"YdrDict: {YdrDict.Count}; YddDict: {YddDict.Count}; YtdDict: {YtdDict.Count}; YftDict: {YftDict.Count}; YcdDict: {YcdDict.Count}; YedDict: {YedDict.Count}");
        }

        private void InitMapDicts(bool force = true)
        {
            UpdateStatus?.Invoke("Building map dictionaries...");
            using var _ = new DisposableTimer("InitMapDicts");
            YmapDict ??= new Dictionary<uint, RpfFileEntry>(4600);
            YbnDict ??= new Dictionary<uint, RpfFileEntry>(8800);
            YnvDict ??= new Dictionary<uint, RpfFileEntry>(4500);
            AllYmapsDict = new Dictionary<uint, RpfFileEntry>(11000);
            foreach (var rpffile in ActiveMapRpfFiles.Values) //RpfMan.BaseRpfs)
            {
                if (rpffile.AllEntries == null) continue;
                foreach (var entry in rpffile.AllEntries)
                {
                    if (entry is RpfFileEntry fentry)
                    {
                        if (entry.IsExtension(".ymap"))
                        {
                            //YmapDict[entry.NameHash] = fentry;
                            YmapDict[entry.ShortNameHash] = fentry;
                            AllYmapsDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.IsExtension(".ybn"))
                        {
                            //YbnDict[entry.NameHash] = fentry;
                            YbnDict[entry.ShortNameHash] = fentry;
                        }
                        else if (entry.IsExtension(".ynv"))
                        {
                            YnvDict[entry.ShortNameHash] = fentry;
                        }
                    }
                }
            }

            Console.WriteLine($"YmapDict: {YmapDict.Count}; YbnDict: {YbnDict.Count}; YnvDict: {YnvDict.Count}; AllYmapsDict: {AllYmapsDict.Count};");
        }

        private void InitManifestDicts(bool force = true)
        {
            UpdateStatus?.Invoke("Loading manifests...");
            using var _ = new DisposableTimer("InitManifestDicts");
            AllManifests = new List<YmfFile>(2000);
            hdtexturelookup = new Dictionary<MetaHash, MetaHash>(24000);
            IEnumerable<RpfFile> rpfs = PreloadedMode ? AllRpfs : ActiveMapRpfFiles.Values;
            foreach (RpfFile file in rpfs)
            {
                if (file.AllEntries == null) continue;
                //manifest and meta parsing..
                foreach (RpfEntry entry in file.AllEntries)
                {
                    if (!entry.IsExtension(".ymf"))
                    {
                        continue;
                    }
                    try
                    {
                        UpdateStatus?.Invoke(string.Format(entry.Path));
                        YmfFile ymffile = RpfMan.GetFile<YmfFile>(entry);
                        if (ymffile != null)
                        {
                            AllManifests.Add(ymffile);

                            if (ymffile.HDTxdAssetBindings != null)
                            {
                                for (int i = 0; i < ymffile.HDTxdAssetBindings.Length; i++)
                                {
                                    var b = ymffile.HDTxdAssetBindings[i];
                                    var targetasset = JenkHash.GenHashLower(b.targetAsset.ToString());
                                    var hdtxd = JenkHash.GenHashLower(b.HDTxd.ToString());
                                    hdtexturelookup[targetasset] = hdtxd;
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        string errstr = entry.Path + "\n" + ex.ToString();
                        ErrorLog?.Invoke(errstr);
                        Console.WriteLine(errstr);
                    }
                }
            }

            Console.WriteLine($"hdtexturelookup: {hdtexturelookup.Count}; AllManifests: {AllManifests.Count};");
        }

        private static ConcurrentDictionary<string, VehiclesFile> vehicleFiles = new(4, 72, StringComparer.OrdinalIgnoreCase);
        private async Task InitGtxds(bool force = true)
        {
            UpdateStatus?.Invoke("Loading global texture list...");
            using var timer = new DisposableTimer("InitGtxds");
            var parentTxds = new ConcurrentDictionary<MetaHash, MetaHash>(4, 4000);

            IEnumerable<RpfFile> rpfs = PreloadedMode ? AllRpfs : (IEnumerable<RpfFile>)ActiveMapRpfFiles.Values;

            static void addTxdRelationships(Dictionary<string, string> from, ConcurrentDictionary<MetaHash, MetaHash> parentTxds) {
                foreach (var kvp in from)
                {
                    uint chash = JenkHash.GenHashLower(kvp.Key);
                    uint phash = JenkHash.GenHashLower(kvp.Value);

                    parentTxds.TryAdd(chash, phash);
                }
            }

            IEnumerable<RpfFile> allRpfs = rpfs;


            if (EnableDlc && DlcActiveRpfs.Count > 0)
            {
                allRpfs = allRpfs.Concat(DlcActiveRpfs);
            }

            await allRpfs.Where(p => p.AllEntries != null).ParallelForEachAsync(async (file) =>
            {
                foreach (RpfEntry entry in file.AllEntries)
                {
                    if (
                        entry.Name.Equals("gtxd.ymt", StringComparison.OrdinalIgnoreCase)
                        || entry.Name.Equals("gtxd.meta", StringComparison.OrdinalIgnoreCase)
                        || entry.Name.Equals("mph4_gtxd.ymt", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        GtxdFile ymt = await RpfMan.GetFileAsync<GtxdFile>(entry).ConfigureAwait(false);
                        if (ymt.TxdRelationships != null)
                        {
                            addTxdRelationships(ymt.TxdRelationships, parentTxds);
                        }
                    }
                    else if (entry.Name.Equals("vehicles.meta", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!vehicleFiles.TryGetValue(entry.Path, out var vf))
                        {
                            vf = vehicleFiles[entry.Path] = await RpfMan.GetFileAsync<VehiclesFile>(entry).ConfigureAwait(false);
                        }
                        if (vf.TxdRelationships != null)
                        {
                            addTxdRelationships(vf.TxdRelationships, parentTxds);
                        }
                    }
                }
            });

            textureParents = parentTxds.ToDictionary(p => p.Key, p => p.Value);

            //ensure resident global texture dicts:
            YtdFile ytd1 = new YtdFile(GetYtdEntry(JenkHash.GenHash("mapdetail")));
            LoadFile(ytd1);
            AddTextureLookups(ytd1);

            YtdFile ytd2 = new YtdFile(GetYtdEntry(JenkHash.GenHash("vehshare")));
            LoadFile(ytd2);
            AddTextureLookups(ytd2);


            Console.WriteLine($"textureParents: {textureParents.Count}");
        }

        private void InitMapCaches(bool force = true)
        {
            UpdateStatus?.Invoke("Loading cache...");
            using var _ = new DisposableTimer("InitMapCaches");
            AllCacheFiles = new List<CacheDatFile>(1);
            YmapHierarchyDict = new Dictionary<uint, MapDataStoreNode>(5000);


            CacheDatFile loadCacheFile(string path, bool finalAttempt)
            {
                try
                {
                    var cache = RpfMan.GetFile<CacheDatFile>(path);
                    if (cache != null)
                    {
                        AllCacheFiles.Add(cache);
                        foreach (var node in cache.AllMapNodes)
                        {
                            if (YmapDict.ContainsKey(node.Name))
                            {
                                YmapHierarchyDict[node.Name] = node;
                            }
                            else
                            {
                                Console.WriteLine($"Couldn't find {node.Name} in YmapDict");
                            } //ymap not found...
                        }
                    }
                    else if (finalAttempt)
                    {
                        ErrorLog(path + ": main cachefile not loaded! Possibly an unsupported GTAV installation version.");
                    }
                    else //update\x64\dlcpacks\mpspecialraces\dlc.rpf\x64\data\cacheloaderdata_dlc\mpspecialraces_3336915258_cache_y.dat (hash of: mpspecialraces_interior_additions)
                    { }
                    return cache;
                }
                catch (Exception ex)
                {
                    ErrorLog(path + ": " + ex.ToString());
                }
                return null;
            }


            CacheDatFile maincache = null;
            if (EnableDlc)
            {
                maincache = loadCacheFile("update\\update.rpf\\common\\data\\gta5_cache_y.dat", false);
                if (maincache == null)
                {
                    maincache = loadCacheFile("update\\update2.rpf\\common\\data\\gta5_cache_y.dat", true);
                }
            }
            else
            {
                maincache = loadCacheFile("common.rpf\\data\\gta5_cache_y.dat", true);
            }





            if (EnableDlc)
            {
                foreach (string dlccachefile in DlcCacheFileList)
                {
                    loadCacheFile(dlccachefile, false);
                }
            }

            Console.WriteLine($"AllCacheFiles: {AllCacheFiles.Count}; YmapHierarchyDict: {YmapHierarchyDict.Count};");
        }

        private ConcurrentBag<YtypFile> ytypCache;
        private async Task InitArchetypeDicts(bool force = true)
        {
            UpdateStatus?.Invoke("Loading archetypes...");

            ytypCache = new ConcurrentBag<YtypFile>();

            archetypesLoaded = false;
            try
            {
                archetypeDict.Clear();
                using var timer = new DisposableTimer("InitArchetypeDicts");

                var rpfs = EnableDlc ? AllRpfs : BaseRpfs;

                var allYtypes = rpfs
                    .Where(p => p.AllEntries != null && (EnableDlc || !p.Path.StartsWith("update", StringComparison.OrdinalIgnoreCase)));
                //.SelectMany(p => p.AllEntries.Where(p => p.IsExtension(".ytyp")));

                //await allYtypes.ParallelForEachAsync(async (file) =>
                //{
                //    foreach (var entry in file.AllEntries) {
                //        if (!entry.Name.EndsWith(".ytyp", StringComparison.OrdinalIgnoreCase)) continue;
                //        try
                //        {
                //            AddYtypToDictionary(entry);
                //        }
                //        catch (Exception ex)
                //        {
                //            ErrorLog(entry.Path + ": " + ex.ToString());
                //        }
                //    }
                //}, maxDegreeOfParallelism: 8);

                await allYtypes.ParallelForEachAsync(async (file) =>
                {
                    foreach (var entry in file.AllEntries)
                    {
                        if (entry.IsExtension(".ytyp"))
                        {
                            try
                            {
                                ytypCache.Add(await RpfMan.GetFileAsync<YtypFile>(entry).ConfigureAwait(false));
                            }
                            catch (Exception ex)
                            {
                                ErrorLog(entry.Path + ": " + ex.ToString());
                            }
                        }
                    }
                }).ConfigureAwait(false);

                YtypDict = new Dictionary<uint, YtypFile>(ytypCache.Count);

                foreach (var ytyp in ytypCache)
                {
                    AddYtypToDictionary(ytyp.RpfFileEntry, ytyp);
                }

                //Parallel.ForEach(allYtypes, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (file) =>
                //{
                //    foreach(var entry in file.AllEntries)
                //    {
                //        if (entry.IsExtension(".ytyp"))
                //        {
                //            try
                //            {
                //                AddYtypToDictionary(entry);
                //            }
                //            catch (Exception ex)
                //            {
                //                ErrorLog(entry.Path + ": " + ex.ToString());
                //            }
                //        }
                //    }
                //});

                //Parallel.ForEach(allYtypes, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (entry) =>
                //{
                //    try
                //    {
                //        AddYtypToDictionary(entry);
                //    }
                //    catch (Exception ex)
                //    {
                //        ErrorLog(entry.Path + ": " + ex.ToString());
                //    }
                //});
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            archetypesLoaded = true;

        }

        private void AddYtypToDictionary(RpfEntry entry, YtypFile ytypFile)
        {
            if (ytypFile == null)
            {
                throw new Exception("Couldn't load ytyp file."); //couldn't load the file for some reason... shouldn't happen..
            }
            if (ytypFile.Meta == null)
            {
                if (ytypFile.Pso == null && ytypFile.Rbf == null)
                {
                    throw new Exception("ytyp file was not in meta format.");
                }
                return;
            }

            ytypCache.Add(ytypFile);
            YtypDict[ytypFile.NameHash] = ytypFile;

            if ((ytypFile.AllArchetypes == null) || (ytypFile.AllArchetypes.Length == 0))
            {
                ErrorLog(entry.Path + ": no archetypes found");
            }
            else
            {
                foreach (var arch in ytypFile.AllArchetypes)
                {
                    uint hash = arch.Hash;
                    if (hash == 0) continue;
                    archetypeDict[hash] = arch;
                }
            }
            
            //if (YtypDict.ContainsKey(ytypfile.NameHash))
            //{
            //    //throw new Exception("ytyp " + JenkIndex.GetString(ytypfile.NameHash) + " already loaded.");
            //    //errorAction(entry.Path + ": ytyp " + JenkIndex.GetString(ytypfile.NameHash) + " already loaded.");
            //    YtypDict[ytypfile.NameHash] = ytypfile; //override ytyp and continue anyway, could be unique archetypes in here still...
            //}
            //else
            //{
            //    YtypDict.Add(ytypfile.NameHash, ytypfile);
            //}






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

        public async ValueTask InitStringDictsAsync(bool force = true)
        {
            UpdateStatus?.Invoke("Loading strings...");
            using var timer = new DisposableTimer("InitStringDicts");
            string langstr = "american_rel"; //todo: make this variable?
            string langstr2 = "americandlc.rpf";
            string langstr3 = "american.rpf";

            Gxt2Dict = new Dictionary<uint, RpfFileEntry>(1000);
            var gxt2files = new List<Gxt2File>();
            foreach (var rpf in AllRpfs.Where(p => p.AllEntries != null))
            {
                foreach (var entry in rpf.AllEntries)
                {
                    if (entry is RpfFileEntry fentry)
                    {
                        var p = entry.Path;
                        if (entry.IsExtension(".gxt2") && (p.Contains(langstr, StringComparison.OrdinalIgnoreCase) || p.Contains(langstr2, StringComparison.OrdinalIgnoreCase) || p.Contains(langstr3, StringComparison.OrdinalIgnoreCase)))
                        {
                            Gxt2Dict[entry.ShortNameHash] = fentry;

                            if (DoFullStringIndex)
                            {
                                var gxt2 = await RpfMan.GetFileAsync<Gxt2File>(entry).ConfigureAwait(false);
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
                    if (entry.Name.EndsWith("statssetup.xml", StringComparison.OrdinalIgnoreCase))
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

                            var statnamel = statname;
                            StatsNames.EnsureLower(statname);
                            StatsNames.EnsureLower(statnamel);

                            StatsNames.EnsureLower("sp_" + statnamel);
                            StatsNames.EnsureLower("mp0_" + statnamel);
                            StatsNames.EnsureLower("mp1_" + statnamel);

                        }
                    }
                }
            }

            StatsNames.FullIndexBuilt = true;
        }

        public void InitVehicles(bool force = true)
        {
            if (!LoadVehicles) return;
            if (!force && VehiclesLoaded) return;


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


            UpdateStatus?.Invoke("Loading vehicles...");
            using var _ = new DisposableTimer("InitVehicles");


            IEnumerable<RpfFile> rpfs = PreloadedMode ? AllRpfs : (IEnumerable<RpfFile>)ActiveMapRpfFiles.Values;


            var allVehicles = new Dictionary<MetaHash, VehicleInitData>(900);
            //var allCarCols = new List<CarColsFile>();
            //var allCarModCols = new List<CarModColsFile>();
            //var allCarVariations = new List<CarVariationsFile>();
            //var allCarVariationsDict = new Dictionary<MetaHash, CVehicleModelInfoVariation_418053801>();
            //var allVehicleLayouts = new List<VehicleLayoutsFile>();

            IEnumerable<RpfFile> allRpfs = rpfs;

            if (EnableDlc && DlcActiveRpfs.Count > 0)
            {
                allRpfs = allRpfs.Concat(DlcActiveRpfs);
            }

            Parallel.ForEach(allRpfs.Where(p => p.AllEntries != null).SelectMany(p => p.AllEntries), (entry) =>
            {
                if (!entry.Name.Equals("vehicles.meta", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                try
                {
                    vehicleFiles[entry.Path] = RpfMan.GetFile<VehiclesFile>(entry);
                    //if ((entry.Name.Equals("carcols.ymt", StringComparison.OrdinalIgnoreCase)) || (entry.Name.Equals("carcols.meta", StringComparison.OrdinalIgnoreCase)))
                    //{
                    //    var cf = RpfMan.GetFile<CarColsFile>(entry);
                    //    if (cf.VehicleModelInfo != null)
                    //    { }
                    //    allCarCols.Add(cf);
                    //}
                    //if (entry.Name.Equals("carmodcols.ymt", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    var cf = RpfMan.GetFile<CarModColsFile>(entry);
                    //    if (cf.VehicleModColours != null)
                    //    { }
                    //    allCarModCols.Add(cf);
                    //}
                    //if ((entry.Name.Equals("carvariations.ymt", StringComparison.OrdinalIgnoreCase)) || (entry.Name.Equals("carvariations.meta", StringComparison.OrdinalIgnoreCase)))
                    //{
                    //    var cf = RpfMan.GetFile<CarVariationsFile>(entry);
                    //    if (cf.VehicleModelInfo?.variationData != null)
                    //    {
                    //        foreach (var variation in cf.VehicleModelInfo.variationData)
                    //        {
                    //            var name = variation.modelName.ToLowerInvariant();
                    //            var hash = JenkHash.GenHash(name);
                    //            allCarVariationsDict[hash] = variation;
                    //        }
                    //    }
                    //    allCarVariations.Add(cf);
                    //}
                    //if (entry.Name.StartsWith("vehiclelayouts", StringComparison.OrdinalIgnoreCase) && entry.Name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    var lf = RpfMan.GetFile<VehicleLayoutsFile>(entry);
                    //    if (lf.Xml != null)
                    //    { }
                    //    allVehicleLayouts.Add(lf);
                    //}
                }
                catch (Exception ex)
                {
                    string errstr = entry.Path + "\n" + ex.ToString();
                    ErrorLog(errstr);
                    Console.WriteLine(errstr);
                }
            });

            foreach(var vf in vehicleFiles.Values)
            {
                if (vf.InitDatas != null)
                {
                    foreach (var initData in vf.InitDatas)
                    {
                        var name = initData.modelName;
                        var hash = JenkHash.GenHashLower(name);
                        if (!allVehicles.ContainsKey(hash))
                        {
                            allVehicles[hash] = initData;
                        }
                    }
                }
            }

            Console.WriteLine($"allVehicles: {allVehicles.Count}; vehicleFiles: {vehicleFiles.Count}");

            VehiclesInitDict = allVehicles;
        }

        public void InitPeds(bool force = true)
        {
            if (!LoadPeds) return;
            if (!force && PedsLoaded) return;

            UpdateStatus?.Invoke("Loading peds...");
            using var _ = new DisposableTimer("InitPeds");
            IEnumerable<RpfFile> rpfs = PreloadedMode ? AllRpfs : (IEnumerable<RpfFile>)ActiveMapRpfFiles.Values;
            List<RpfFile> dlcrpfs = null;
            if (EnableDlc)
            {
                dlcrpfs = new List<RpfFile>();
                foreach (var rpf in DlcActiveRpfs)
                {
                    dlcrpfs.Add(rpf);
                    if (rpf.Children == null) continue;
                    dlcrpfs.AddRange(rpf.Children);
                }
            }



            var allPeds = new ConcurrentDictionary<MetaHash, CPedModelInfo__InitData>(4, 1100);
            //var allPedsFiles = new List<PedsFile>();
            var allPedYmts = new ConcurrentDictionary<MetaHash, PedFile>(4, 1100);
            var allPedDrwDicts = new ConcurrentDictionary<MetaHash, Dictionary<MetaHash, RpfFileEntry>>(4, 1100);
            var allPedTexDicts = new ConcurrentDictionary<MetaHash, Dictionary<MetaHash, RpfFileEntry>>(4, 1100);
            var allPedClothDicts = new ConcurrentDictionary<MetaHash, Dictionary<MetaHash, RpfFileEntry>>(4, 200);


            Dictionary<MetaHash, RpfFileEntry> ensureDict(ConcurrentDictionary<MetaHash, Dictionary<MetaHash, RpfFileEntry>> coll, MetaHash hash)
            {
                return coll.GetOrAdd(hash, (key) => new Dictionary<MetaHash, RpfFileEntry>());
            }

            var addPedDicts = new Action<string, MetaHash, RpfDirectoryEntry>((name, hash, dir) =>
            {
                Dictionary<MetaHash, RpfFileEntry> pedClotsDict = null;
                var files = dir?.Files;
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file.Name.Equals(name + ".yld", StringComparison.OrdinalIgnoreCase))
                        {
                            pedClotsDict ??= ensureDict(allPedClothDicts, hash);
                            pedClotsDict[file.ShortNameHash] = file;
                        }
                    }
                }

                if (dir?.Directories != null)
                {
                    foreach (var cdir in dir.Directories)
                    {
                        if (cdir.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            dir = cdir;
                            break;
                        }
                    }
                    files = dir?.Files;
                    if (files != null)
                    {
                        Dictionary<MetaHash, RpfFileEntry> pedDrwDicts = null;
                        Dictionary<MetaHash, RpfFileEntry> pedTextDicts = null;
                        foreach (var file in files)
                        {
                            if (file?.Name == null) continue;
                            if (file.IsExtension(".ydd"))
                            {
                                pedDrwDicts ??= ensureDict(allPedDrwDicts, hash);
                                pedDrwDicts[file.ShortNameHash] = file;
                            }
                            else if (file.IsExtension(".ytd"))
                            {
                                pedTextDicts ??= ensureDict(allPedTexDicts, hash);
                                pedTextDicts[file.ShortNameHash] = file;
                            }
                            else if (file.IsExtension(".yld"))
                            {
                                pedClotsDict ??= ensureDict(allPedClothDicts, hash);
                                pedClotsDict[file.ShortNameHash] = file;
                            }
                        }
                    }
                }
            });

            var numDlcs = dlcrpfs?.Count ?? 0;

            var allRpfs = rpfs;

            if (numDlcs > 0)
            {
                allRpfs = rpfs.Concat(dlcrpfs);
            }

            var allEntries = allRpfs
                .Where(p => p.AllEntries != null)
                .SelectMany(p => p.AllEntries)
                .Where(p => p.Name.EndsWithAny(".ymt", ".meta"));

            Parallel.ForEach(allEntries, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (entry) =>
            {
                if (!entry.Name.Equals("peds.ymt", StringComparison.OrdinalIgnoreCase) && !entry.Name.Equals("peds.meta", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                try
                {
                    var pf = RpfMan.GetFile<PedsFile>(entry);
                    if (pf.InitDataList?.InitDatas != null)
                    {
                        foreach (var initData in pf.InitDataList.InitDatas)
                        {
                            var name = initData.Name;
                            var hash = JenkHash.GenHashLower(name);
                            allPeds.TryAdd(hash, initData);
                        }
                    }
                    //allPedsFiles.Add(pf);
                }
                catch (Exception ex)
                {
                    string errstr = entry.Path + "\n" + ex.ToString();
                    ErrorLog?.Invoke(errstr);
                    Console.WriteLine(errstr);
                }
            });

            Parallel.ForEach(allEntries, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (entry) =>
            {
                if (!entry.IsExtension(".ymt"))
                {
                    return;
                }
                try
                {
                    var shortName = entry.ShortName;
                    var shortHash = entry.ShortNameHash;
                    if (allPeds.ContainsKey(shortHash))
                    {
                        var pf = RpfMan.GetFile<PedFile>(entry);
                        if (pf != null)
                        {
                            allPedYmts.TryAdd(shortHash, pf);
                            addPedDicts(shortName, shortHash, entry.Parent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errstr = entry.Path + "\n" + ex.ToString();
                    ErrorLog?.Invoke(errstr);
                    Console.WriteLine(errstr);
                }
            });

            PedsInitDict = allPeds.ToDictionary(p => p.Key, p => p.Value);
            PedVariationsDict = allPedYmts.ToDictionary(p => p.Key, p => p.Value);
            PedDrawableDicts = allPedDrwDicts.ToDictionary(p => p.Key, p => p.Value);
            PedTextureDicts = allPedTexDicts.ToDictionary(p => p.Key, p => p.Value);
            PedClothDicts = allPedClothDicts.ToDictionary(p => p.Key, p => p.Value);


            //foreach (var kvp in PedsInitDict)
            //{
            //    if (!PedVariationsDict.ContainsKey(kvp.Key))
            //    { }//checking we found them all!
            //}


        }

        private bool AudioLoaded = false;
        public readonly ReaderWriterLockSlim AudioDatRelFilesLock = new ReaderWriterLockSlim();
        public async ValueTask InitAudio(bool force = true)
        {
            if (!LoadAudio) return;
            if (AudioLoaded && !force) return;

            UpdateStatus?.Invoke("Loading audio...");
            using var timer = new DisposableTimer("InitAudio");
            Dictionary<uint, RpfFileEntry> datrelentries = new Dictionary<uint, RpfFileEntry>();
            void addRpfDatRelEntries(RpfFile rpffile)
            {
                if (rpffile.AllEntries == null) return;
                foreach (var entry in rpffile.AllEntries)
                {
                    if (entry is RpfFileEntry fentry)
                    {
                        if (entry.IsExtension(".rel"))
                        {
                            datrelentries[entry.NameHash] = fentry;
                        }
                    }
                }
            }

            var audrpf = RpfMan.FindRpfFile("x64\\audio\\audio_rel.rpf");
            if (audrpf != null)
            {
                addRpfDatRelEntries(audrpf);
            }

            if (EnableDlc)
            {
                var updrpf = RpfMan.FindRpfFile("update\\update.rpf");
                if (updrpf != null)
                {
                    addRpfDatRelEntries(updrpf);
                }
                foreach (var dlcrpf in DlcActiveRpfs) //load from current dlc rpfs
                {
                    addRpfDatRelEntries(dlcrpf);
                }
                if (DlcActiveRpfs.Count == 0) //when activated from RPF explorer... DLCs aren't initialised fully
                {
                    foreach (var rpf in AllRpfs) //this is a bit of a hack - DLC orders won't be correct so likely will select wrong versions of things
                    {
                        if (rpf.Name.StartsWith("dlc", StringComparison.OrdinalIgnoreCase))
                        {
                            addRpfDatRelEntries(rpf);
                        }
                    }
                }
            }

            try
            {
                AudioDatRelFiles ??= new List<RelFile>(20);
                AudioConfigDict ??= new Dictionary<MetaHash, RelData>(150);
                AudioSpeechDict ??= new Dictionary<MetaHash, RelData>(90000);
                AudioSynthsDict ??= new Dictionary<MetaHash, RelData>(1500);
                AudioMixersDict ??= new Dictionary<MetaHash, RelData>(3500);
                AudioCurvesDict ??= new Dictionary<MetaHash, RelData>(1000);
                AudioCategsDict ??= new Dictionary<MetaHash, RelData>(250);
                AudioSoundsDict ??= new Dictionary<MetaHash, RelData>(60000);
                AudioGameDict ??= new Dictionary<MetaHash, RelData>(20000);

                var audioDatRelFiles = AudioDatRelFiles;
                var audioConfigDict = AudioConfigDict;
                var audioSpeechDict = AudioSpeechDict;
                var audioSynthsDict = AudioSynthsDict;
                var audioMixersDict = AudioMixersDict;
                var audioCurvesDict = AudioCurvesDict;
                var audioCategsDict = AudioCategsDict;
                var audioSoundsDict = AudioSoundsDict;
                var audioGameDict = AudioGameDict;

                var relFiles = new ConcurrentBag<RelFile>();

                await datrelentries.Values.ParallelForEachAsync(async (datrelentry) =>
                {
                    var relfile = await RpfMan.GetFileAsync<RelFile>(datrelentry).ConfigureAwait(false);
                    if (relfile == null)
                        return;

                    relFiles.Add(relfile);
                }).ConfigureAwait(false);

                foreach (var relfile in relFiles)
                {
                    audioDatRelFiles.Add(relfile);

                    var d = audioGameDict;
                    var t = relfile.RelType;
                    switch (t)
                    {
                        case RelDatFileType.Dat4:
                            d = relfile.IsAudioConfig ? audioConfigDict : audioSpeechDict;
                            break;
                        case RelDatFileType.Dat10ModularSynth:
                            d = audioSynthsDict;
                            break;
                        case RelDatFileType.Dat15DynamicMixer:
                            d = audioMixersDict;
                            break;
                        case RelDatFileType.Dat16Curves:
                            d = audioCurvesDict;
                            break;
                        case RelDatFileType.Dat22Categories:
                            d = audioCategsDict;
                            break;
                        case RelDatFileType.Dat54DataEntries:
                            d = audioSoundsDict;
                            break;
                        case RelDatFileType.Dat149:
                        case RelDatFileType.Dat150:
                        case RelDatFileType.Dat151:
                        default:
                            d = audioGameDict;
                            break;
                    }

                    foreach (var reldata in relfile.RelDatas)
                    {
                        if (reldata.NameHash == 0)
                            continue;
                        d[reldata.NameHash] = reldata;
                    }

                }
            } catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Console.WriteLine(
                $"AudioDatRelFiles: {AudioDatRelFiles.Count}; " +
                $"AudioConfigDict: {AudioConfigDict.Count}; " +
                $"AudioSpeechDict: {AudioSpeechDict.Count}; " +
                $"AudioSynthsDict: {AudioSynthsDict.Count}; " +
                $"AudioMixersDict: {AudioMixersDict.Count}; " +
                $"AudioCurvesDict: {AudioCurvesDict.Count}; " +
                $"AudioCategsDict: {AudioCategsDict.Count}; " +
                $"AudioSoundsDict: {AudioSoundsDict.Count}; " +
                $"AudioGameDict: {AudioGameDict.Count};");

            AudioLoaded = true;
        }





        public bool SetDlcLevel(string dlc, bool enable)
        {
            bool dlcchange = (!dlc.Equals(SelectedDlc, StringComparison.OrdinalIgnoreCase));
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




        public void AddProjectFile(GameFile f)
        {
            if (f == null) return;
            if (f.RpfFileEntry == null) return;
            if (f.RpfFileEntry.ShortNameHash == 0) return;
            var key = new GameFileCacheKey(f.RpfFileEntry.ShortNameHash, f.Type);
            projectFiles[key] = f;
        }
        public void RemoveProjectFile(GameFile f)
        {
            if (f == null) return;
            if (f.RpfFileEntry == null) return;
            if (f.RpfFileEntry.ShortNameHash == 0) return;
            var key = new GameFileCacheKey(f.RpfFileEntry.ShortNameHash, f.Type);
            projectFiles.TryRemove(key, out _);
        }
        public void ClearProjectFiles()
        {
            projectFiles.Clear();
        }

        public void AddProjectArchetype(Archetype a)
        {
            if ((a?.Hash ?? 0) == 0) return;
            projectArchetypes[a.Hash] = a;
        }
        public void RemoveProjectArchetype(Archetype a)
        {
            if ((a?.Hash ?? 0) == 0) return;
            try
            {
                projectArchetypes.TryGetValue(a.Hash, out var tarch);
                if (tarch == a)
                {
                    projectArchetypes.Remove(a.Hash);
                }
            }
            finally
            {
                archetypeLock.ExitWriteLock();
            }

        }
        public void ClearProjectArchetypes()
        {
            archetypeLock.EnterWriteLock();
            try
            {
                projectArchetypes.Clear();
            }
            finally
            {
                archetypeLock.ExitWriteLock();
            }
            
        }

        private readonly SemaphoreSlim maxCacheLock = new SemaphoreSlim(Environment.ProcessorCount / 2, Environment.ProcessorCount / 2);
        public async Task TryLoadEnqueue(GameFile gf)
        {
            if (gf.Loaded || gf.LoadQueued)
            {
                return;
            }
            if (requestQueue.Count < 20)// && (!gf.LoadQueued)
            {
                gf.LoadQueued = true;
                gf.LastLoadTime = DateTime.Now;

                await Task.Run(async () =>
                {
                    await maxCacheLock.WaitAsync();
                    try
                    {
                        await LoadFileAsync(gf).ConfigureAwait(false);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                        throw ex;
                    }
                    finally
                    {
                        maxCacheLock.Release();
                    }
                });

                //requestQueue.Enqueue(gf);
            }
        }


        public Archetype GetArchetype(uint hash)
        {
            if (!archetypesLoaded) return null;
            archetypeLock.EnterReadLock();
            try
            {
                if (projectArchetypes.TryGetValue(hash, out var arch) && arch != null)
                {
                    return arch;
                }
                archetypeDict.TryGetValue(hash, out arch);
                return arch;
            }
            finally
            {
                archetypeLock.ExitReadLock();
            }
        }
        public MapDataStoreNode GetMapNode(uint hash)
        {
            if (!IsInited) return null;
            YmapHierarchyDict.TryGetValue(hash, out var node);
            return node;
        }

        private readonly object ydrLock = new object();
        public YdrFile GetYdr(uint hash, bool immediate = false)
        {
            if (!IsInited) return null;
            lock (ydrLock)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ydr);
                if (projectFiles.TryGetValue(key, out GameFile pgf))
                {
                    return pgf as YdrFile;
                }
                YdrFile ydr = mainCache.TryGet(key) as YdrFile;
                if (ydr == null)
                {
                    var e = GetYdrEntry(hash);

                    if (e == null)
                    {
                        return null;
                    }
                    ydr = new YdrFile(e);
                    if (!mainCache.TryAdd(key, ydr))
                    {
                        return null;
                    }
                }

                if (!ydr.Loaded && !ydr.LoadQueued)
                {
                    TryLoadEnqueue(ydr);
                }
                return ydr;
            }
        }

        private readonly object yddLock = new object();
        public YddFile GetYdd(uint hash)
        {
            if (!IsInited) return null;
            lock (yddLock)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ydd);
                if (projectFiles.TryGetValue(key, out GameFile pgf))
                {
                    return pgf as YddFile;
                }
                YddFile ydd = mainCache.TryGet(key) as YddFile;
                if (ydd == null)
                {
                    var e = GetYddEntry(hash);
                    if (e == null) return null;

                    ydd = new YddFile(e);
                    if (!mainCache.TryAdd(key, ydd))
                    {
                        return null;
                    }
                }

                if (!ydd.Loaded && !ydd.LoadQueued)
                {
                    TryLoadEnqueue(ydd);
                }
                return ydd;
            }
        }

        private readonly SemaphoreSlim ytdLock = new SemaphoreSlim(1, 1);
        public YtdFile GetYtd(uint hash)
        {
            if (!IsInited) return null;
            ytdLock.Wait();
            try {
                var key = new GameFileCacheKey(hash, GameFileType.Ytd);
                if (projectFiles.TryGetValue(key, out GameFile pgf))
                {
                    return pgf as YtdFile;
                }
                YtdFile ytd = mainCache.TryGet(key) as YtdFile;
                if (ytd == null)
                {
                    var e = GetYtdEntry(hash);
                    if (e == null)
                    {
                        return null;
                    }
                    ytd = new YtdFile(e);
                    if (!mainCache.TryAdd(key, ytd))
                    {
                        return null;
                    }
                }
                if (!ytd.Loaded && !ytd.LoadQueued)
                {
                    _ = TryLoadEnqueue(ytd);
                }
                return ytd;
            } finally
            {
                ytdLock.Release();
            }
        }

        private readonly object ymapLock = new object();
        public YmapFile GetYmap(uint hash)
        {
            if (!IsInited) return null;
            lock (ymapLock)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ymap);
                YmapFile ymap = mainCache.TryGet(key) as YmapFile;
                if (ymap == null)
                {
                    var e = GetYmapEntry(hash);
                    if (e == null)
                    {
                        return null;
                    }
                    ymap = new YmapFile(e);
                    if (!mainCache.TryAdd(key, ymap))
                    {
                        return null;
                    }
                }
                if (!ymap.Loaded && !ymap.LoadQueued)
                {
                    TryLoadEnqueue(ymap);
                }
                return ymap;
            }
        }

        private readonly object yftLock = new object();
        public YftFile GetYft(uint hash)
        {
            if (!IsInited) return null;
            lock (yftLock)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Yft);
                YftFile yft = mainCache.TryGet(key) as YftFile;
                if (projectFiles.TryGetValue(key, out GameFile pgf))
                {
                    return pgf as YftFile;
                }
                if (yft == null)
                {
                    var e = GetYftEntry(hash);
                    if (e == null)
                    {
                        return null;
                    }
                    yft = new YftFile(e);
                    if (!mainCache.TryAdd(key, yft))
                    {
                        return null;
                    }
                }
                if (!yft.Loaded && !yft.LoadQueued)
                {
                    TryLoadEnqueue(yft);
                }
                return yft;
            }
        }

        private readonly object ybnLock = new object();
        public YbnFile GetYbn(uint hash)
        {
            if (!IsInited) return null;
            lock (ybnLock)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ybn);
                YbnFile ybn = mainCache.TryGet(key) as YbnFile;
                if (ybn == null)
                {
                    var e = GetYbnEntry(hash);
                    if (e == null)
                    {
                        return null;
                    }
                    ybn = new YbnFile(e);
                    if (!mainCache.TryAdd(key, ybn))
                    {
                        return null;
                    }
                }
                if (!ybn.Loaded && !ybn.LoadQueued)
                {
                    TryLoadEnqueue(ybn);
                }
                return ybn;
            }
        }

        private readonly object ycdLock = new object();
        public YcdFile GetYcd(uint hash)
        {
            if (!IsInited) return null;
            lock (ycdLock)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ycd);
                YcdFile ycd = mainCache.TryGet(key) as YcdFile;
                if (ycd == null)
                {
                    var e = GetYcdEntry(hash);
                    if (e == null)
                    {
                        return null;
                    }
                    ycd = new YcdFile(e);
                    if (!mainCache.TryAdd(key, ycd))
                    {
                        return null;
                    }
                }
                if (!ycd.Loaded && !ycd.LoadQueued)
                {
                    TryLoadEnqueue(ycd);
                }
                return ycd;
            }
        }

        private readonly object yedLock = new object();
        public YedFile GetYed(uint hash)
        {
            if (!IsInited) return null;
            lock (yedLock)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Yed);
                YedFile yed = mainCache.TryGet(key) as YedFile;
                if (yed == null)
                {
                    var e = GetYedEntry(hash);
                    if (e == null)
                    {
                        return null;
                    }
                    yed = new YedFile(e);
                    if (!mainCache.TryAdd(key, yed))
                    {
                        return null;
                    }
                }
                if (!yed.Loaded && !yed.LoadQueued)
                {
                    TryLoadEnqueue(yed);
                }
                return yed;
            }
        }

        private readonly object yvnLock = new object();
        public YnvFile GetYnv(uint hash)
        {
            if (!IsInited) return null;
            lock (yvnLock)
            {
                var key = new GameFileCacheKey(hash, GameFileType.Ynv);
                YnvFile ynv = mainCache.TryGet(key) as YnvFile;
                if (ynv == null)
                {
                    var e = GetYnvEntry(hash);
                    if (e == null)
                    {
                        return null;
                    }
                    ynv = new YnvFile(e);
                    if (!mainCache.TryAdd(key, ynv))
                    {
                        return null;
                    }
                }
                if (!ynv.Loaded && !ynv.LoadQueued)
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
                if (!AllYmapsDict.TryGetValue(hash, out entry))
                {
                    Console.WriteLine($"Couldn't find ymap {JenkIndex.GetString(hash)}");
                }
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
        public RpfFileEntry GetYedEntry(uint hash)
        {
            RpfFileEntry entry;
            YedDict.TryGetValue(hash, out entry);
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

        public ValueTask<bool> LoadFileAsync<T>(T file) where T : GameFile, PackedFile
        {
            if (file == null) return new ValueTask<bool>(false);
            RpfFileEntry entry = file.RpfFileEntry;
            if (entry != null)
            {
                return RpfMan.LoadFileAsync(file, entry);
            }
            return new ValueTask<bool>(false);
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
            mainCache.BeginFrame();
        }

        private async ValueTask LoadFileAsync(GameFile req)
        {
            //process content requests.
            if (req.Loaded)
                return; //it's already loaded... (somehow)

            if ((req.LastUseTime - DateTime.Now).TotalSeconds > 3.0)
                return; //hasn't been requested lately..! ignore, will try again later if necessary
            try
            {
                //if (!loadedsomething)
                //{
                //UpdateStatus?.Invoke("Loading " + req.RpfFileEntry.Name + "...");
                //}

                switch (req.Type)
                {
                    case GameFileType.Ydr:
                        req.Loaded = await LoadFileAsync(req as YdrFile);
                        break;
                    case GameFileType.Ydd:
                        req.Loaded = await LoadFileAsync(req as YddFile);
                        break;
                    case GameFileType.Ytd:
                        req.Loaded = await LoadFileAsync(req as YtdFile);
                        //if (req.Loaded) AddTextureLookups(req as YtdFile);
                        break;
                    case GameFileType.Ymap:
                        YmapFile y = req as YmapFile;
                        req.Loaded = await LoadFileAsync(y);
                        if (req.Loaded) y.InitYmapEntityArchetypes(this);
                        break;
                    case GameFileType.Yft:
                        req.Loaded = await LoadFileAsync(req as YftFile);
                        break;
                    case GameFileType.Ybn:
                        req.Loaded = await LoadFileAsync(req as YbnFile);
                        break;
                    case GameFileType.Ycd:
                        req.Loaded = await LoadFileAsync(req as YcdFile);
                        break;
                    case GameFileType.Yed:
                        req.Loaded = await LoadFileAsync(req as YedFile);
                        break;
                    case GameFileType.Ynv:
                        req.Loaded = await LoadFileAsync(req as YnvFile);
                        break;
                    case GameFileType.Yld:
                        req.Loaded = await LoadFileAsync(req as YldFile);
                        break;
                    default:
                        break;
                }

                string str = (req.Loaded ? "Loaded " : "Error loading ") + req.ToString();
                //string str = string.Format("{0}: {1}: {2}", requestQueue.Count, (req.Loaded ? "Loaded" : "Error loading"), req);

                UpdateStatus?.Invoke(str);
                //ErrorLog(str);
                if (!req.Loaded)
                {
                    ErrorLog("Error loading " + req.ToString());
                }
            }
            catch (Exception e)
            {
                req.LoadQueued = false;
                req.Loaded = false;
                Console.WriteLine(e);
                TryLoadEnqueue(req);
            }
            finally
            {
                req.LoadQueued = false;
            }
        }

        private void LoadFile(GameFile req)
        {
            lock (req)
            {
                try
                {
                    //process content requests.
                    if (req.Loaded)
                        return; //it's already loaded... (somehow)

                    if ((req.LastUseTime - DateTime.Now).TotalSeconds > 3.0)
                        return; //hasn't been requested lately..! ignore, will try again later if necessary

                    //if (!loadedsomething)
                    //{
                    //UpdateStatus?.Invoke("Loading " + req.RpfFileEntry.Name + "...");
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
                        case GameFileType.Yed:
                            req.Loaded = LoadFile(req as YedFile);
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

                    UpdateStatus?.Invoke(str);
                    //ErrorLog(str);
                    if (!req.Loaded)
                    {
                        ErrorLog("Error loading " + req.ToString());
                    }
                }
                catch (Exception e)
                {
                    req.LoadQueued = false;
                    req.Loaded = false;
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    req.LoadQueued = false;
                }
            }
        }

        public bool ContentThreadProc()
        {
            mainCache.BeginFrame();
            int itemcount = 0;
            lock (updateSyncRoot)
            {
                GameFile req;

                while ((itemcount < MaxItemsPerLoop) && requestQueue.TryDequeue(out req))
                {
                    LoadFile(req);

                    itemcount++;
                }
            }

            return itemcount >= 1;
        }






        private void AddTextureLookups(YtdFile ytd)
        {
            if (ytd?.TextureDict?.TextureNameHashes?.data_items == null) return;

            foreach (uint hash in ytd.TextureDict.TextureNameHashes.data_items)
            {
                textureLookup[hash] = ytd.RpfFileEntry;
            }
        }
        public YtdFile TryGetTextureDictForTexture(uint hash)
        {
            if (textureLookup.TryGetValue(hash, out var e))
            {
                return GetYtd(e.ShortNameHash);
            }

            return null;
        }
        public YtdFile TryGetParentYtd(uint hash)
        {
            MetaHash phash;
            if (textureParents is not null && textureParents.TryGetValue(hash, out phash))
            {
                return GetYtd(phash);
            }
            return null;
        }
        public uint TryGetParentYtdHash(uint hash)
        {
            if (textureParents is null)
            {
                return 0u;
            }
            textureParents.TryGetValue(hash, out MetaHash phash);
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

        public void GetShadersXml()
        {
            bool doydr = true;
            bool doydd = true;
            bool doyft = true;
            bool doypt = true;

            var data = new Dictionary<MetaHash, ShaderXmlDataCollection>();

            void collectDrawable(DrawableBase d)
            {
                if (d?.AllModels == null) return;
                foreach (var model in d.AllModels)
                {
                    if (model?.Geometries == null) continue;
                    foreach (var geom in model.Geometries)
                    {
                        var s = geom?.Shader;
                        if (s == null) continue;
                        ShaderXmlDataCollection dc = null;
                        if (!data.TryGetValue(s.Name, out dc))
                        {
                            dc = new ShaderXmlDataCollection();
                            dc.Name = s.Name;
                            data.Add(s.Name, dc);
                        }
                        dc.AddShaderUse(s, geom);
                    }
                }
            }



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

                            if (ydr == null) { continue; }
                            if (ydr.Drawable == null) { continue; }
                            collectDrawable(ydr.Drawable);
                        }
                        else if (doydd & entry.IsExtension(".ydd"))
                        {
                            UpdateStatus?.Invoke(entry.Path);
                            YddFile ydd = RpfMan.GetFile<YddFile>(entry);

                            if (ydd == null) { continue; }
                            if (ydd.Dict == null) { continue; }
                            foreach (var drawable in ydd.Dict.Values)
                            {
                                collectDrawable(drawable);
                            }
                        }
                        else if (doyft && entry.IsExtension(".yft"))
                        {
                            UpdateStatus?.Invoke(entry.Path);
                            YftFile yft = RpfMan.GetFile<YftFile>(entry);

                            if (yft == null) { continue; }
                            if (yft.Fragment == null) { continue; }
                            if (yft.Fragment.Drawable != null)
                            {
                                collectDrawable(yft.Fragment.Drawable);
                            }
                            if ((yft.Fragment.Cloths != null) && (yft.Fragment.Cloths.data_items != null))
                            {
                                foreach (var cloth in yft.Fragment.Cloths.data_items)
                                {
                                    collectDrawable(cloth.Drawable);
                                }
                            }
                            if ((yft.Fragment.DrawableArray != null) && (yft.Fragment.DrawableArray.data_items != null))
                            {
                                foreach (var drawable in yft.Fragment.DrawableArray.data_items)
                                {
                                    collectDrawable(drawable);
                                }
                            }
                        }
                        else if (doypt && entry.IsExtension(".ypt"))
                        {
                            UpdateStatus?.Invoke(entry.Path);
                            YptFile ypt = RpfMan.GetFile<YptFile>(entry);

                            if (ypt == null) { continue; }
                            if (ypt.DrawableDict == null) { continue; }
                            foreach (var drawable in ypt.DrawableDict.Values)
                            {
                                collectDrawable(drawable);
                            }
                        }
                    }
                    catch //(Exception ex)
                    { }
                }
            }




            var shaders = data.Values.ToList();
            shaders.Sort((a, b) => { return b.GeomCount.CompareTo(a.GeomCount); });


            StringBuilder sb = new StringBuilder();

            sb.AppendLine(MetaXml.XmlHeader);
            MetaXml.OpenTag(sb, 0, "Shaders");
            foreach (var s in shaders)
            {
                MetaXml.OpenTag(sb, 1, "Item");
                MetaXml.StringTag(sb, 2, "Name", MetaXml.HashString(s.Name));
                MetaXml.WriteHashItemArray(sb, s.GetSortedList(s.FileNames).ToArray(), 2, "FileName");
                MetaXml.WriteRawArray(sb, s.GetSortedList(s.RenderBuckets).ToArray(), 2, "RenderBucket", "");
                MetaXml.OpenTag(sb, 2, "Layout");
                var layouts = s.GetSortedList(s.VertexLayouts);
                foreach (var l in layouts)
                {
                    var vd = new VertexDeclaration();
                    vd.Types = l.Types;
                    vd.Flags = l.Flags;
                    vd.WriteXml(sb, 3, "Item");
                }
                MetaXml.CloseTag(sb, 2, "Layout");
                MetaXml.OpenTag(sb, 2, "Parameters");
                var otstr = "Item name=\"{0}\" type=\"{1}\"";
                var texparams = s.GetSortedList(s.TexParams);
                var valparams = s.ValParams;
                var arrparams = s.ArrParams;
                foreach (var tp in texparams)
                {
                    MetaXml.SelfClosingTag(sb, 3, string.Format(otstr, ((ShaderParamNames)tp).ToString(), "Texture"));
                }
                foreach (var vp in valparams)
                {
                    var svp = s.GetSortedList(vp.Value);
                    var defval = svp.FirstOrDefault();
                    MetaXml.SelfClosingTag(sb, 3, string.Format(otstr, ((ShaderParamNames)vp.Key).ToString(), "Vector") + " " + FloatUtil.GetVector4XmlString(defval));
                }
                foreach (var ap in arrparams)
                {
                    var defval = ap.Value.FirstOrDefault();
                    MetaXml.OpenTag(sb, 3, string.Format(otstr, ((ShaderParamNames)ap.Key).ToString(), "Array"));
                    foreach (var vec in defval)
                    {
                        MetaXml.SelfClosingTag(sb, 4, "Value " + FloatUtil.GetVector4XmlString(vec));
                    }
                    MetaXml.CloseTag(sb, 3, "Item");
                }
                MetaXml.CloseTag(sb, 2, "Parameters");
                MetaXml.CloseTag(sb, 1, "Item");
            }
            MetaXml.CloseTag(sb, 0, "Shaders");

            var xml = sb.ToString();

            File.WriteAllText("C:\\Shaders.xml", xml);


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


        public class ShaderTextureData
        {
            public HashSet<string> Textures { get; set; } = new HashSet<string>();
            public HashSet<string> Geometries { get; set; } = new HashSet<string>();
            public int Count { get; set; } = 0;
        }

        public class ShaderXmlDataCollection
        {
            public MetaHash Name { get; set; }
            public Dictionary<MetaHash, int> FileNames { get; set; } = new Dictionary<MetaHash, int>();
            public Dictionary<byte, int> RenderBuckets { get; set; } = new Dictionary<byte, int>();
            public Dictionary<ShaderXmlVertexLayout, int> VertexLayouts { get; set; } = new Dictionary<ShaderXmlVertexLayout, int>();
            public Dictionary<MetaName, int> TexParams { get; set; } = new Dictionary<MetaName, int>();

            public Dictionary<MetaName, ShaderTextureData> TextureData { get; set; } = new Dictionary<MetaName, ShaderTextureData>();

            public HashSet<string> Textures { get; set; } = new HashSet<string>();
            public Dictionary<MetaName, Dictionary<Vector4, int>> ValParams { get; set; } = new Dictionary<MetaName, Dictionary<Vector4, int>>();
            public Dictionary<MetaName, List<Vector4[]>> ArrParams { get; set; } = new Dictionary<MetaName, List<Vector4[]>>();
            public int GeomCount { get; set; } = 0;


            public void AddShaderUse(ShaderFX s, DrawableGeometry g, DrawableBase d = null)
            {
                GeomCount++;

                AddItem(s.FileName, FileNames);
                AddItem(s.RenderBucket, RenderBuckets);

                var info = g.VertexBuffer?.Info;
                if (info != null)
                {
                    AddItem(new ShaderXmlVertexLayout() { Flags = info.Flags, Types = info.Types }, VertexLayouts);
                }

                if (s.ParametersList?.Parameters == null) return;
                if (s.ParametersList?.Hashes == null) return;

                for (int i = 0; i < s.ParametersList.Count; i++)
                {
                    var h = s.ParametersList.Hashes[i];
                    var p = s.ParametersList.Parameters[i];

                    if (p.DataType == 0)//texture
                    {
                        AddItem(h, TexParams);

                        if (!TextureData.TryGetValue(h, out var tex))
                        {
                            tex = new ShaderTextureData();
                            TextureData[h] = tex;
                        }

                        tex.Count++;

                        if (p.Data is TextureBase texture)
                        {
                            tex.Textures.Add($"{texture.Name} ({(d.Owner as GameFile)?.Name})");
                        }
                        
                    }
                    else if (p.DataType == 1)//vector
                    {
                        var vp = GetItem(h, ValParams);
                        if (p.Data is Vector4 vec)
                        {
                            AddItem(vec, vp);
                        }
                    }
                    else if (p.DataType > 1)//array
                    {
                        var ap = GetItem(h, ArrParams);
                        if (p.Data is Vector4[] arr)
                        {
                            bool found = false;
                            foreach (var exarr in ap)
                            {
                                if (exarr.Length != arr.Length) continue;
                                bool match = true;
                                for (int j = 0; j < exarr.Length; j++)
                                {
                                    if (exarr[j] != arr[j])
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                                if (match)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                ap.Add(arr);
                            }
                        }
                    }
                }
            }
            public void AddItem<T>(T t, Dictionary<T, int> d)
            {
                if (d.ContainsKey(t))
                {
                    d[t]++;
                }
                else
                {
                    d[t] = 1;
                }
            }
            public U GetItem<T, U>(T t, Dictionary<T, U> d) where U:new()
            {
                U r = default(U);
                if (!d.TryGetValue(t, out r))
                {
                    r = new U();
                    d[t] = r;
                }
                return r;
            }
            public List<T> GetSortedList<T>(Dictionary<T, int> d)
            {
                var kvps = d.ToList();
                kvps.Sort((a, b) => { return b.Value.CompareTo(a.Value); });
                return kvps.Select((a) => { return a.Key; }).ToList();
            }

            public List<KeyValuePair<T, ShaderTextureData>> GetSortedList<T>(Dictionary<T, ShaderTextureData> d)
            {
                var kvps = d.ToList();
                kvps.Sort((a, b) => { return b.Value.Count.CompareTo(a.Value.Count); });
                return kvps.ToList();
            }
        }
        public struct ShaderXmlVertexLayout
        {
            public VertexDeclarationTypes Types { get; set; }
            public uint Flags { get; set; }
            public VertexType VertexType { get { return (VertexType)Flags; } }
            public override string ToString()
            {
                return Types.ToString() + ", " + VertexType.ToString();
            }
        }
    }


}
