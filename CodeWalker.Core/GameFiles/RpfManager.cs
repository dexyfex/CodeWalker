using CodeWalker.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    public class RpfManager
    {
        private static RpfManager _instance = new RpfManager();
        public static RpfManager GetInstance()
        {
            return _instance ??= new RpfManager();
        }
        //for caching and management of RPF file data.

        public string Folder { get; private set; }
        public string[] ExcludePaths { get; set; }
        public bool EnableMods { get; set; }
        public bool BuildExtendedJenkIndex { get; set; } = true;
        public event Action<string> UpdateStatus;
        public event Action<string> ErrorLog;

        public List<RpfFile> BaseRpfs { get; private set; }
        public List<RpfFile> ModRpfs { get; private set; }
        public List<RpfFile> DlcRpfs { get; private set; }
        public List<RpfFile> AllRpfs { get; private set; }
        public List<RpfFile> DlcNoModRpfs { get; private set; }
        public List<RpfFile> AllNoModRpfs { get; private set; }
        public Dictionary<string, RpfFile> RpfDict { get; private set; }
        public Dictionary<string, RpfEntry> EntryDict { get; private set; }
        public Dictionary<string, RpfFile> ModRpfDict { get; private set; }
        public Dictionary<string, RpfEntry> ModEntryDict { get; private set; }

        public volatile bool IsInited = false;

        public void Init(string folder, Action<string> updateStatus, Action<string> errorLog, bool rootOnly = false, bool buildIndex = true)
        {
            using var _ = new DisposableTimer("RpfManager.Init");
            UpdateStatus += updateStatus;
            ErrorLog += errorLog;

            string replpath = folder + "\\";
            var sopt = rootOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
            string[] allfiles = Directory.GetFiles(folder, "*.rpf", sopt);

            BaseRpfs = new List<RpfFile>();
            ModRpfs = new List<RpfFile>();
            DlcRpfs = new List<RpfFile>();
            AllRpfs = new List<RpfFile>();
            DlcNoModRpfs = new List<RpfFile>();
            AllNoModRpfs = new List<RpfFile>();
            RpfDict = new Dictionary<string, RpfFile>();
            EntryDict = new Dictionary<string, RpfEntry>();
            ModRpfDict = new Dictionary<string, RpfFile>();
            ModEntryDict = new Dictionary<string, RpfEntry>();

            var rpfs = new ConcurrentBag<RpfFile>();
            Parallel.ForEach(allfiles, (rpfpath) =>
            {
                try
                {
                    RpfFile rf = new RpfFile(rpfpath, rpfpath.Replace(replpath, ""));

                    if (ExcludePaths != null)
                    {
                        bool excl = false;
                        for (int i = 0; i < ExcludePaths.Length; i++)
                        {
                            if (rf.Path.StartsWith(ExcludePaths[i]))
                            {
                                excl = true;
                                break;
                            }
                        }
                        if (excl) return; //skip files in exclude paths.
                    }

                    rf.ScanStructure(updateStatus, errorLog);

                    if (rf.LastException != null) //incase of corrupted rpf (or renamed NG encrypted RPF)
                    {
                        return;
                    }

                    rpfs.Add(rf);
                }
                catch (Exception ex)
                {
                    errorLog(rpfpath + ": " + ex.ToString());
                }
            });

            var calculateSum = (RpfFile rpf) => { return 0; };

            calculateSum = (RpfFile rpf) =>
            {
                return rpf.AllEntries?.Count ?? 0 + rpf.Children?.Sum(calculateSum) ?? 0;
            };

            var minCapacity = rpfs.Sum(calculateSum);
            if (minCapacity > AllRpfs.Capacity)
            {
                AllRpfs.Capacity = minCapacity;
            }

            foreach(var rpf in rpfs)
            {
                AddRpfFile(rpf, false, false);
            }

            if (buildIndex)
            {
                updateStatus?.Invoke("Building jenkindex...");
                Task.Run(() =>
                {
                    BuildBaseJenkIndex();
                    IsInited = true;
                });
                updateStatus?.Invoke("Scan complete");
            }
            else
            {
                updateStatus?.Invoke("Scan complete");
                IsInited = true;
            }
            
            

            
        }

        public void Init(List<RpfFile> allRpfs)
        {
            using var _ = new DisposableTimer("RpfManager.Init");
            //fast init used by RPF explorer's File cache
            AllRpfs = allRpfs;

            BaseRpfs = new List<RpfFile>();
            ModRpfs = new List<RpfFile>();
            DlcRpfs = new List<RpfFile>();
            DlcNoModRpfs = new List<RpfFile>();
            AllNoModRpfs = new List<RpfFile>();
            RpfDict = new Dictionary<string, RpfFile>();
            EntryDict = new Dictionary<string, RpfEntry>();
            ModRpfDict = new Dictionary<string, RpfFile>();
            ModEntryDict = new Dictionary<string, RpfEntry>();
            foreach (var rpf in allRpfs)
            {
                RpfDict[rpf.Path] = rpf;
                if (rpf.AllEntries == null) continue;
                foreach (var entry in rpf.AllEntries)
                {
                    EntryDict[entry.Path] = entry;
                }
            }

            Task.Run(() =>
            {
                BuildBaseJenkIndex();
                IsInited = true;
            });
        }


        private void AddRpfFile(RpfFile file, bool isdlc, bool ismod)
        {
            isdlc = isdlc || (file.NameLower == "update.rpf") || (file.NameLower.StartsWith("dlc") && file.NameLower.EndsWith(".rpf"));
            ismod = ismod || (file.Path.StartsWith("mods\\"));

            if (file.AllEntries != null)
            {
                AllRpfs.Add(file);
                if (!ismod)
                {
                    AllNoModRpfs.Add(file);
                }
                if (isdlc)
                {
                    DlcRpfs.Add(file);
                    if (!ismod)
                    {
                        DlcNoModRpfs.Add(file);
                    }
                }
                else
                {
                    if (ismod)
                    {
                        ModRpfs.Add(file);
                    }
                    else
                    {
                        BaseRpfs.Add(file);
                    }
                }
                if (ismod)
                {
                    ModRpfDict[file.Path.Substring(5)] = file;
                }

                RpfDict[file.Path] = file;

                foreach (RpfEntry entry in file.AllEntries)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            if (ismod)
                            {
                                ModEntryDict[entry.Path] = entry;
                                ModEntryDict[entry.Path.Substring(5)] = entry;
                            }
                            else
                            {
                                EntryDict[entry.Path] = entry;
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        file.LastError = ex.ToString();
                        file.LastException = ex;
                        ErrorLog?.Invoke(entry.Path + ": " + ex.ToString());
                    }
                }
            }

            if (file.Children != null)
            {
                foreach (RpfFile cfile in file.Children)
                {
                    AddRpfFile(cfile, isdlc, ismod);
                }
            }
        }


        public RpfFile FindRpfFile(string path) => FindRpfFile(path, false);


        public RpfFile FindRpfFile(string path, bool exactPathOnly)
        {
            RpfFile file = null; //check the dictionary

            if (EnableMods && ModRpfDict.TryGetValue(path, out file))
            {
                return file;
            }

            if (RpfDict.TryGetValue(path, out file))
            {
                return file;
            }

            string lpath = path.ToLowerInvariant(); //try look at names etc
            foreach (RpfFile tfile in AllRpfs)
            {
                if (!exactPathOnly && tfile.NameLower == lpath)
                {
                    return tfile;
                }
                if (tfile.Path == lpath)
                {
                    return tfile;
                }
            }

            return file;
        }


        public RpfEntry GetEntry(string path)
        {
            RpfEntry entry;
            string pathl = path.ToLowerInvariant();
            if (EnableMods && ModEntryDict.TryGetValue(pathl, out entry))
            {
                return entry;
            }
            EntryDict.TryGetValue(pathl, out entry);
            if (entry == null)
            {
                pathl = pathl.Replace("/", "\\");
                pathl = pathl.Replace("common:", "common.rpf");
                if (EnableMods && ModEntryDict.TryGetValue(pathl, out entry))
                {
                    return entry;
                }
                EntryDict.TryGetValue(pathl, out entry);
            }
            return entry;
        }
        public byte[] GetFileData(string path)
        {
            byte[] data = null;
            RpfFileEntry entry = GetEntry(path) as RpfFileEntry;
            if (entry != null)
            {
                data = entry.File.ExtractFile(entry);
            }
            return data;
        }
        public string GetFileUTF8Text(string path)
        {
            byte[] bytes = GetFileData(path);
            return TextUtil.GetUTF8Text(bytes);
        }
        public XmlDocument GetFileXml(string path)
        {
            XmlDocument doc = new XmlDocument();
            string text = GetFileUTF8Text(path);
            if (!string.IsNullOrEmpty(text))
            {
                doc.LoadXml(text);
            }
            return doc;
        }

        public T GetFile<T>(string path) where T : class, PackedFile, new()
        {
            T file = null;
            byte[] data = null;
            RpfFileEntry entry = GetEntry(path) as RpfFileEntry;
            if (entry != null)
            {
                data = entry.File.ExtractFile(entry);
            }
            if (data != null)
            {
                file = new T();
                file.Load(data, entry);
            }
            return file;
        }
        public T GetFile<T>(RpfEntry e) where T : class, PackedFile, new()
        {
            T file = null;
            byte[] data = null;
            RpfFileEntry entry = e as RpfFileEntry;
            if (entry != null)
            {
                data = entry.File.ExtractFile(entry);
            }
            if (data != null)
            {
                file = new T();
                file.Load(data, entry);
            }
            return file;
        }
        public bool LoadFile<T>(T file, RpfEntry e) where T : class, PackedFile
        {
            byte[] data = null;
            RpfFileEntry entry = e as RpfFileEntry;
            if (entry != null)
            {
                data = entry.File.ExtractFile(entry);
            }
            if (data != null)
            {
                file.Load(data, entry);
                return true;
            }
            return false;
        }



        public void BuildBaseJenkIndex()
        {
            using var _ = new DisposableTimer("BuildBaseJenkIndex");
            Parallel.ForEach(AllRpfs, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (file) =>
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    JenkIndex.Ensure(file.Name);
                    foreach (RpfEntry entry in file.AllEntries)
                    {
                        var nlow = entry.NameLower;
                        if (string.IsNullOrEmpty(nlow)) continue;
                        //JenkIndex.Ensure(entry.Name);
                        //JenkIndex.Ensure(nlow);
                        int ind = nlow.LastIndexOf('.');
                        if (ind > 0)
                        {
                            
                            JenkIndex.Ensure(entry.Name.Substring(0, ind));
                            JenkIndex.Ensure(nlow.Substring(0, ind));

                            //if (ind < entry.Name.Length - 2)
                            //{
                            //    JenkIndex.Ensure(entry.Name.Substring(0, ind) + ".#" + entry.Name.Substring(ind + 2));
                            //    JenkIndex.Ensure(entry.NameLower.Substring(0, ind) + ".#" + entry.NameLower.Substring(ind + 2));
                            //}
                        }
                        else
                        {
                            JenkIndex.Ensure(entry.Name);
                            JenkIndex.Ensure(nlow);
                        }
                        if (BuildExtendedJenkIndex)
                        {
                            if (nlow.EndsWith(".ydr"))// || nlow.EndsWith(".yft")) //do yft's get lods?
                            {
                                var sname = nlow.Substring(0, nlow.Length - 4);
                                JenkIndex.Ensure(sname + "_lod");
                                JenkIndex.Ensure(sname + "_loda");
                                JenkIndex.Ensure(sname + "_lodb");
                            }
                            if (nlow.EndsWith(".ydd"))
                            {
                                if (nlow.EndsWith("_children.ydd"))
                                {
                                    var strn = nlow.Substring(0, nlow.Length - 13);
                                    JenkIndex.Ensure(strn);
                                    JenkIndex.Ensure(strn + "_lod");
                                    JenkIndex.Ensure(strn + "_loda");
                                    JenkIndex.Ensure(strn + "_lodb");
                                }
                                var idx = nlow.LastIndexOf('_');
                                if (idx > 0)
                                {
                                    var str1 = nlow.Substring(0, idx);
                                    var idx2 = str1.LastIndexOf('_');
                                    if (idx2 > 0)
                                    {
                                        var str2 = str1.Substring(0, idx2);
                                        JenkIndex.Ensure(str2 + "_lod");
                                        var maxi = 100;
                                        for (int i = 1; i <= maxi; i++)
                                        {
                                            var str3 = str2 + "_" + i.ToString().PadLeft(2, '0');
                                            //JenkIndex.Ensure(str3);
                                            JenkIndex.Ensure(str3 + "_lod");
                                        }
                                    }
                                }
                            }
                            if (nlow.EndsWith(".sps"))
                            {
                                JenkIndex.Ensure(nlow);//for shader preset filename hashes!
                            }
                            if (nlow.EndsWith(".awc")) //create audio container path hashes...
                            {
                                string[] parts = entry.Path.Split('\\');
                                int pl = parts.Length;
                                if (pl > 2)
                                {
                                    string fn = parts[pl - 1];
                                    string fd = parts[pl - 2];
                                    string hpath = fn.Substring(0, fn.Length - 4);
                                    if (fd.EndsWith(".rpf"))
                                    {
                                        fd = fd.Substring(0, fd.Length - 4);
                                    }
                                    hpath = fd + "/" + hpath;
                                    if (parts[pl - 3] != "sfx")
                                    { }//no hit

                                    JenkIndex.Ensure(hpath);
                                }
                            }
                            if (nlow.EndsWith(".nametable"))
                            {
                                RpfBinaryFileEntry binfe = entry as RpfBinaryFileEntry;
                                if (binfe != null)
                                {
                                    byte[] data = file.ExtractFile(binfe);
                                    if (data != null)
                                    {
                                        sb.Clear();
                                        for (int i = 0; i < data.Length; i++)
                                        {
                                            byte c = data[i];
                                            if (c == 0)
                                            {
                                                string str = sb.ToString();
                                                if (!string.IsNullOrEmpty(str))
                                                {
                                                    string strl = str.ToLowerInvariant();
                                                    //JenkIndex.Ensure(str);
                                                    JenkIndex.Ensure(strl);

                                                    ////DirMod_Sounds_ entries apparently can be used to infer SP audio strings
                                                    ////no luck here yet though
                                                    //if (strl.StartsWith("dirmod_sounds_") && (strl.Length > 14))
                                                    //{
                                                    //    strl = strl.Substring(14);
                                                    //    JenkIndex.Ensure(strl);
                                                    //}
                                                }
                                                sb.Clear();
                                            }
                                            else
                                            {
                                                sb.Append((char)c);
                                            }
                                        }
                                    }
                                }
                                else
                                { }
                            }
                        }
                    }

                }
                catch(Exception err)
                {
                    ErrorLog?.Invoke(err.ToString());
                    //failing silently!! not so good really
                }
            });
            //foreach (RpfFile file in AllRpfs)
            //{
                
            //}

            for (int i = 0; i < 100; i++)
            {
                JenkIndex.Ensure(i.ToString("00"));
            }
        }

    }
}
