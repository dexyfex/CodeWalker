

using CodeWalker.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

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

        private const int DefaultEntryDictCapacity = 354878;
        private const int DefaultRpfDictCapacity = 4650;

        public void Init(string folder, Action<string> updateStatus, Action<string> errorLog, bool rootOnly = false, bool buildIndex = true)
        {
            using var _ = new DisposableTimer("RpfManager.Init");
            UpdateStatus += updateStatus;
            ErrorLog += errorLog;

            string replpath = folder + "\\";
            var sopt = rootOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
            string[] allfiles = Directory.GetFiles(folder, "*.rpf", sopt);

            BaseRpfs = new List<RpfFile>(1300);
            ModRpfs = new List<RpfFile>(0);
            DlcRpfs = new List<RpfFile>(3500);
            AllRpfs = new List<RpfFile>(5000);
            DlcNoModRpfs = new List<RpfFile>(3500);
            AllNoModRpfs = new List<RpfFile>(5000);
            RpfDict = new Dictionary<string, RpfFile>(DefaultRpfDictCapacity, StringComparer.OrdinalIgnoreCase);
            EntryDict = new Dictionary<string, RpfEntry>(DefaultEntryDictCapacity, StringComparer.OrdinalIgnoreCase);
            ModRpfDict = new Dictionary<string, RpfFile>(StringComparer.OrdinalIgnoreCase);
            ModEntryDict = new Dictionary<string, RpfEntry>(StringComparer.OrdinalIgnoreCase);

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
                            if (rf.Path.StartsWith(ExcludePaths[i], StringComparison.OrdinalIgnoreCase))
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
                        Console.WriteLine(rf.LastException);
                        return;
                    }

                    rpfs.Add(rf);
                }
                catch (Exception ex)
                {
                    errorLog(rpfpath + ": " + ex.ToString());
                }
            });

            static int calculateSum(RpfFile rpf)
            {
                return rpf.AllEntries?.Count ?? 0 + rpf.Children?.Sum(calculateSum) ?? 0;
            }

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
                Task.Run(() =>
                {
                    updateStatus?.Invoke("Building jenkindex...");
                    BuildBaseJenkIndex();
                    IsInited = true;
                    updateStatus?.Invoke("Scan complete");
                });
            }
            else
            {
                updateStatus?.Invoke("Scan complete");
                IsInited = true;
            }



            Console.WriteLine($"AllRpfs: {AllRpfs.Count}; RpfDict: {RpfDict.Count}; EntryDict: {EntryDict.Count}; BaseRpfs: {BaseRpfs.Count}; ModRpfs: {ModRpfs.Count}; DlcRpfs: {DlcRpfs.Count}; DlcNoModRpfs: {DlcNoModRpfs.Count}; AllNoModRpfs: {AllNoModRpfs.Count}; ModRpfDict: {ModRpfDict.Count}; ModEntryDict: {ModEntryDict.Count}");
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
            RpfDict = new Dictionary<string, RpfFile>(DefaultRpfDictCapacity, StringComparer.OrdinalIgnoreCase);
            EntryDict = new Dictionary<string, RpfEntry>(DefaultEntryDictCapacity, StringComparer.OrdinalIgnoreCase);
            ModRpfDict = new Dictionary<string, RpfFile>(StringComparer.OrdinalIgnoreCase);
            ModEntryDict = new Dictionary<string, RpfEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var rpf in allRpfs)
            {
                RpfDict[rpf.Path] = rpf;
                if (rpf.AllEntries == null)
                    continue;
                foreach (var entry in rpf.AllEntries)
                {
                    EntryDict[entry.Path] = entry;
                }
            }

            Console.WriteLine($"RpfDict: {RpfDict.Count}; EntryDict: {EntryDict.Count}");

            Task.Run(() =>
            {
                BuildBaseJenkIndex();
                IsInited = true;
            });
        }


        private void AddRpfFile(RpfFile file, bool isdlc, bool ismod)
        {
            if (file.AllEntries == null && file.Children == null)
                return;

            isdlc = isdlc || (file.Name.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase) && (file.Name.StartsWith("dlc", StringComparison.OrdinalIgnoreCase) || file.Name.Equals("update.rpf", StringComparison.OrdinalIgnoreCase)));
            ismod = ismod || (file.Path.StartsWith("mods\\", StringComparison.OrdinalIgnoreCase));

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
            RpfFile file;
            if (EnableMods && ModRpfDict.TryGetValue(path, out file))
            {
                return file;
            }

            if (RpfDict.TryGetValue(path, out file))
            {
                return file;
            }

            foreach (RpfFile tfile in AllRpfs)
            {
                if (!exactPathOnly && tfile.Name.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    return tfile;
                }
                if (tfile.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    return tfile;
                }
            }

            return file;
        }


        public RpfEntry GetEntry(string path)
        {
            RpfEntry entry;
            if (EnableMods && ModEntryDict.TryGetValue(path, out entry))
            {
                return entry;
            }
            EntryDict.TryGetValue(path, out entry);
            if (entry == null)
            {
                path = path.Replace("/", "\\");
                path = path.Replace("common:", "common.rpf");
                if (EnableMods && ModEntryDict.TryGetValue(path, out entry))
                {
                    return entry;
                }
                EntryDict.TryGetValue(path, out entry);
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
        public async Task<T> GetFileAsync<T>(RpfEntry e) where T : class, PackedFile, new()
        {
            try
            {
                T file = null;
                byte[] data = null;
                RpfFileEntry entry = e as RpfFileEntry;
                if (entry != null)
                {
                    data = await entry.File.ExtractFileAsync(entry).ConfigureAwait(false);
                }
                if (data != null)
                {
                    file = new T();
                    file.Load(data, entry);
                }
                return file;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
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
                try
                {
                    file.Load(data, entry);
                    return true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error occured while loading {entry.Name} at {entry.Path}:\n{ex}");
                    throw;
                }
            }
            return false;
        }

        public async ValueTask<bool> LoadFileAsync<T>(T file, RpfEntry e) where T : class, PackedFile
        {
            byte[] data = null;
            RpfFileEntry entry = e as RpfFileEntry;
            if (entry != null)
            {
                data = await entry.File.ExtractFileAsync(entry).ConfigureAwait(false);
            }
            if (data != null && data.Length > 0)
            {
                try
                {
                    file.Load(data, entry);
                    return true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error occured while loading {entry.Name} at {entry.Path}:\n{ex}");
                    throw;
                }
            }
            return false;
        }

        private ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
        public void AddAllLods(string name)
        {
            var idx = name.LastIndexOf('_');
            if (idx > 0)
            {
                var str1 = name.AsSpan(0, idx);
                var idx2 = str1.LastIndexOf('_');
                if (idx2 > 0)
                {
                    // Filter some peds and clothing models (ydd's) which don't have LOD hashes we're interested in.
                    // This saves about 50% of the time it takes to do initial hashing
                    var str2 = str1.Slice(0, idx2 + 1);
                    if (str2.Length <= 2)
                    {
                        return;
                    }
                    switch(str2)
                    {
                        case "uppr_":
                        case "p_":
                        case "accs_":
                        case "decl_":
                        case "berd_":
                        case "hair_":
                        case "teef_":
                        case "lowr_":
                        case "jbib_":
                        case "hand_":
                        case "feet_":
                        case "task_":
                        case "head_":
                        case "s_m_y_":
                        case "s_m_":
                        case "s_m_m_":
                        case "s_f_y_":
                        case "a_m_y_":
                        case "ig_":
                        case "u_m_y_":
                        case "u_m_m_":
                        case "u_m_":
                        case "minimap_":
                        case "a_":
                        case "u_f_":
                        case "csb_":
                        case "g_m_y_":
                        case "a_f_m_":
                        case "a_m_m_":
                        case "g_m_m_":
                        case "mp_m_":
                        case "mp_f_":
                        case "hand_000_":
                        case "hand_001_":
                        case "hair_000_":
                        case "a_f_y_":
                            return;
                        default:
                            break;
                    }
                    Span<char> buff = stackalloc char[str2.Length + 2 + 4];
                    str2.CopyTo(buff.Slice(0, str2.Length));
                    "lod".AsSpan().CopyTo(buff.Slice(str2.Length, 3));
                    //Console.WriteLine(buff.Slice(0, str2.Length + 3).ToString());
                    JenkIndex.EnsureLower(buff.Slice(0, str2.Length + 3));
                    var maxi = 99;

                    "00_lod".AsSpan().CopyTo(buff.Slice(str2.Length, 6));
                    for (int i = 1; i <= maxi; i++)
                    {
                        if (i < 10)
                        {
                            i.ToString().AsSpan().CopyTo(buff.Slice(str2.Length + 1, 1));
                        }
                        else
                        {
                            i.ToString().AsSpan().CopyTo(buff.Slice(str2.Length, 2));
                        }

                        //Console.WriteLine(buff.ToString());
                        //JenkIndex.Ensure(buff);
                        JenkIndex.EnsureLower(buff);
                    }
                }
            }
        }

        public void BuildBaseJenkIndex()
        {
            using var _ = new DisposableTimer("BuildBaseJenkIndex");
            Parallel.ForEach(AllRpfs, (file) =>
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    JenkIndex.Ensure(file.Name);
                    foreach (RpfEntry entry in file.AllEntries)
                    {
                        var name = entry.Name;
                        if (string.IsNullOrEmpty(name))
                            continue;
                        //JenkIndex.Ensure(entry.Name);
                        //JenkIndex.Ensure(nlow);
                        var nameWithoutExtension = entry.ShortName;
                        JenkIndex.EnsureBoth(nameWithoutExtension);

                        //if (ind < entry.Name.Length - 2)
                        //{
                        //    JenkIndex.Ensure(entry.Name.Substring(0, ind) + ".#" + entry.Name.Substring(ind + 2));
                        //    JenkIndex.Ensure(entry.NameLower.Substring(0, ind) + ".#" + entry.NameLower.Substring(ind + 2));
                        //}
                        if (BuildExtendedJenkIndex)
                        {
                            if (name.EndsWith(".ydr", StringComparison.OrdinalIgnoreCase))// || nlow.EndsWith(".yft")) //do yft's get lods?
                            {
                                var sname = entry.ShortName;
                                var nameLod = sname + "_lod";
                                JenkIndex.EnsureLower(nameLod);
                                JenkIndex.EnsureLower(nameLod + 'a');
                                JenkIndex.EnsureLower(nameLod + 'b');
                            }
                            else if (name.EndsWith(".ydd", StringComparison.OrdinalIgnoreCase))
                            {
                                if (name.EndsWith("_children.ydd", StringComparison.OrdinalIgnoreCase))
                                {
                                    var strn = entry.Name.Substring(0, name.Length - 13);
                                    JenkIndex.EnsureLower(strn);
                                    var nameChildrenLod = strn + "_lod";
                                    JenkIndex.EnsureLower(nameChildrenLod);
                                    JenkIndex.EnsureLower(nameChildrenLod + 'a');
                                    JenkIndex.EnsureLower(nameChildrenLod + 'b');
                                }
                                //var idx = name.LastIndexOf('_');
                                //if (idx > 0)
                                //{
                                //    var str1 = name.Substring(0, idx);
                                //    var idx2 = str1.LastIndexOf('_');
                                //    if (idx2 > 0)
                                //    {
                                //        var str2 = str1.Substring(0, idx2);
                                //        JenkIndex.EnsureLower(str2 + "_lod");
                                //        var maxi = 100;

                                //        for (int i = 1; i <= maxi; i++)
                                //        {
                                //            var str3 = str2 + '_' + i.ToString().PadLeft(2, '0') + "_lod";
                                //            //JenkIndex.Ensure(str3);
                                //            JenkIndex.EnsureLower(str3);
                                //        }
                                //    }
                                //}
                                AddAllLods(name);
                            }
                            else if(name.EndsWith(".sps", StringComparison.OrdinalIgnoreCase))
                            {
                                JenkIndex.EnsureLower(entry.Name);//for shader preset filename hashes!
                            }
                            else if(name.EndsWith(".awc", StringComparison.OrdinalIgnoreCase)) //create audio container path hashes...
                            {
                                string[] parts = entry.Path.Split('\\');
                                int pl = parts.Length;
                                if (pl > 2)
                                {
                                    string fn = parts[pl - 1];
                                    string fd = parts[pl - 2];
                                    string hpath = fn.Substring(0, fn.Length - 4);
                                    if (fd.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fd = fd.Substring(0, fd.Length - 4);
                                    }
                                    hpath = fd + '/' + hpath;

                                    JenkIndex.EnsureLower(hpath);
                                }
                            }
                            else if(name.EndsWith(".nametable", StringComparison.OrdinalIgnoreCase))
                            {
                                if (entry is RpfBinaryFileEntry binfe)
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
                                                    //JenkIndex.Ensure(str);
                                                    JenkIndex.EnsureLower(str);

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
                            }
                        }
                    }

                }
                catch(Exception err)
                {
                    ErrorLog?.Invoke(err.ToString());
                    Console.WriteLine(err.ToString());
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

            //Task.Run(() =>
            //{
            //    foreach (var count in counts.OrderBy(p => p.Value))
            //    {
            //        Console.WriteLine($"{count.Key,30}: {count.Value,3}");
            //    }
            //});
        }

    }
}
