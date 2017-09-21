using CodeWalker.Forms;
using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.World;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker
{
    public partial class ExploreForm : Form
    {
        private volatile bool Ready = false;

        private Dictionary<string, FileTypeInfo> FileTypes;

        private MainTreeFolder RootFolder;
        private MainTreeFolder CurrentFolder;
        private List<MainListItem> CurrentFiles;

        private Stack<MainTreeFolder> BackSteps = new Stack<MainTreeFolder>();
        private Stack<MainTreeFolder> ForwardSteps = new Stack<MainTreeFolder>();
        private bool HistoryNavigating = false;

        private int SortColumnIndex = 0;
        private SortOrder SortDirection = SortOrder.Ascending;
        private int PreviousPathColumnWidth = 0;

        public volatile bool Searching = false;
        private MainTreeFolder SearchResults;

        private List<RpfFile> AllRpfs { get; set; }
        private GameFileCache FileCache { get; set; } = new GameFileCache();
        private object FileCacheSyncRoot = new object();

        private bool EditMode = false;


        public ExploreForm()
        {
            InitializeComponent();

            ShowMainListViewPathColumn(false);
        }

        private void Init()
        {
            //called from ExploreForm_Load

            InitFileTypes();

            if (!EnsureGTAFolder())
            {
                Close();
                return;
            }


            Task.Run(() =>
            {
                try
                {
                    GTA5Keys.LoadFromPath(Settings.Default.GTAFolder);
                }
                catch
                {
                    UpdateStatus("Unable to load gta5.exe!");
                    return;
                }

                UpdateStatus("Scanning...");

                RefreshMainTreeView();

                UpdateStatus("Scan complete.");

                InitFileCache();


                while (!IsDisposed) //run the file cache content thread until the form exits.
                {
                    if (FileCache.IsInited)
                    {
                        FileCache.ContentThreadProc();

                        if (!FileCache.ItemsStillPending)
                        {
                            Thread.Sleep(10);
                        }
                    }
                    else
                    {
                        Thread.Sleep(20);
                    }
                }
            });
        }

        private void InitFileCache()
        {
            Task.Run(() =>
            {
                lock (FileCacheSyncRoot)
                {
                    if (!FileCache.IsInited)
                    {
                        UpdateStatus("Loading file cache...");
                        var allRpfs = AllRpfs;
                        FileCache.Init(UpdateStatus, UpdateErrorLog, allRpfs); //inits main dicts and archetypes only...

                        UpdateStatus("Loading materials...");
                        BoundsMaterialTypes.Init(FileCache);

                        UpdateStatus("Loading scenario types...");
                        Scenarios.EnsureScenarioTypes(FileCache);

                        UpdateStatus("File cache loaded.");
                    }
                }
            });
        }
        public GameFileCache GetFileCache()
        {
            lock (FileCacheSyncRoot)
            {
                if (FileCache.IsInited) return FileCache;
            }
            InitFileCache(); //if we got here, it's not inited yet - init it!
            return FileCache; //return it even though it's probably not inited yet..
        }

        private void InitFileTypes()
        {
            FileTypes = new Dictionary<string, FileTypeInfo>();
            InitFileType(".rpf", "Rage Package File", 3);
            InitFileType("", "File", 4);
            InitFileType(".dat", "Data File", 4);
            InitFileType(".cab", "CAB File", 4);
            InitFileType(".txt", "Text File", 5, FileTypeAction.ViewText);
            InitFileType(".gxt2", "Global Text Table", 5, FileTypeAction.ViewGxt);
            InitFileType(".log", "LOG File", 5, FileTypeAction.ViewText);
            InitFileType(".ini", "Config Text", 5, FileTypeAction.ViewText);
            InitFileType(".vdf", "Steam Script File", 5, FileTypeAction.ViewText);
            InitFileType(".sps", "Shader Preset", 5, FileTypeAction.ViewText);
            InitFileType(".xml", "XML File", 6, FileTypeAction.ViewXml);
            InitFileType(".meta", "Metadata (XML)", 6, FileTypeAction.ViewXml);
            InitFileType(".ymt", "Metadata (Binary)", 6, FileTypeAction.ViewYmt);
            InitFileType(".pso", "Metadata (PSO)", 6, FileTypeAction.ViewJPso);
            InitFileType(".gfx", "Scaleform Flash", 7);
            InitFileType(".ynd", "Path Nodes", 8);
            InitFileType(".ynv", "Nav Mesh", 9, FileTypeAction.ViewModel);
            InitFileType(".yvr", "Vehicle Record", 9, FileTypeAction.ViewYvr);
            InitFileType(".ywr", "Waypoint Record", 9, FileTypeAction.ViewYwr);
            InitFileType(".fxc", "Compiled Shaders", 9, FileTypeAction.ViewFxc);
            InitFileType(".yed", "Expression Dictionary", 9);
            InitFileType(".asi", "ASI Plugin", 9);
            InitFileType(".dll", "Dynamic Link Library", 9);
            InitFileType(".exe", "Executable", 10);
            InitFileType(".yft", "Fragment", 11, FileTypeAction.ViewModel);
            InitFileType(".ydr", "Drawable", 11, FileTypeAction.ViewModel);
            InitFileType(".ydd", "Drawable Dictionary", 12, FileTypeAction.ViewModel);
            InitFileType(".cut", "Cutscene", 12, FileTypeAction.ViewCut);
            InitFileType(".ysc", "Script", 13);
            InitFileType(".ymf", "Manifest", 14, FileTypeAction.ViewYmf);
            InitFileType(".bik", "Bink Video", 15);
            InitFileType(".jpg", "JPEG Image", 16);
            InitFileType(".jpeg", "JPEG Image", 16);
            InitFileType(".gif", "GIF Image", 16);
            InitFileType(".png", "Portable Network Graphics", 16);
            InitFileType(".dds", "DirectDraw Surface", 16);
            InitFileType(".ytd", "Texture Dictionary", 16, FileTypeAction.ViewYtd);
            InitFileType(".mrf", "MRF File", 18);
            InitFileType(".ycd", "Clip Dictionary", 18);
            InitFileType(".ypt", "Particle Effect", 18, FileTypeAction.ViewModel);
            InitFileType(".ybn", "Static Collisions", 19, FileTypeAction.ViewModel);
            InitFileType(".ide", "Item Definitions", 20, FileTypeAction.ViewText);
            InitFileType(".ytyp", "Archetype Definitions", 20, FileTypeAction.ViewYtyp);
            InitFileType(".ymap", "Map Data", 21, FileTypeAction.ViewYmap);
            InitFileType(".ipl", "Item Placements", 21, FileTypeAction.ViewText);
            InitFileType(".awc", "Audio Wave Container", 22, FileTypeAction.ViewAwc);
            InitFileType(".rel", "Audio Data (REL)", 23, FileTypeAction.ViewRel);

        }
        private void InitFileType(string ext, string name, int imgidx, FileTypeAction defaultAction = FileTypeAction.ViewHex)
        {
            var ft = new FileTypeInfo(ext, name, imgidx, defaultAction);
            FileTypes[ext] = ft;
        }
        public FileTypeInfo GetFileType(string fn)
        {
            var fi = new FileInfo(fn);
            var ext = fi.Extension.ToLower();
            if (!string.IsNullOrEmpty(ext))
            {
                FileTypeInfo ft;
                if (FileTypes.TryGetValue(ext, out ft))
                {
                    return ft;
                }
                else
                {
                    ft = new FileTypeInfo(ext, ext.Substring(1).ToUpperInvariant() + " File", 4, FileTypeAction.ViewHex);
                    FileTypes[ft.Extension] = ft; //save it for later!
                    return ft;
                }
            }
            else
            {
                return FileTypes[""];
            }
        }

        private bool EnsureGTAFolder()
        {
            string fldr = Settings.Default.GTAFolder;
            if (string.IsNullOrEmpty(fldr) || !Directory.Exists(fldr))
            {
                if (!ChangeGTAFolder())
                {
                    return false;
                }
                fldr = Settings.Default.GTAFolder;
            }
            if (!Directory.Exists(fldr))
            {
                MessageBox.Show("The specified folder does not exist:\n" + fldr);
                return false;
            }
            if (!File.Exists(fldr + "\\gta5.exe"))
            {
                MessageBox.Show("GTA5.exe not found in folder:\n" + fldr);
                return false;
            }
            Settings.Default.GTAFolder = fldr; //seems ok, save it for later
            return true;
        }
        private bool ChangeGTAFolder()
        {
            SelectFolderForm f = new SelectFolderForm();
            f.ShowDialog();
            if (f.Result == DialogResult.OK)
            {
                Settings.Default.GTAFolder = f.SelectedFolder;
                return true;
            }
            else
            {
                return false;
            }
        }


        public void UpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateStatus(text); }));
                }
                else
                {
                    StatusLabel.Text = text;
                }
            }
            catch { }
        }
        public void UpdateErrorLog(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateErrorLog(text); }));
                }
                else
                {
                    //StatusLabel.Text = text;
                }
            }
            catch { }
        }


        public static string GetRootPath()
        {
            var path = Settings.Default.GTAFolder;
            return path.EndsWith("\\") ? path : path + "\\";
        }


        public void Navigate(MainTreeFolder f)
        {
            if (!Ready) return;
            if (f == CurrentFolder) return; //already there!
            if (f.IsSearchResults)
            {
                foreach (TreeNode tn in MainTreeView.Nodes)
                {
                    var tnf = tn.Tag as MainTreeFolder;
                    if ((tnf != null) && (tnf.IsSearchResults))
                    {
                        MainTreeView.Nodes.Remove(tn); //remove existing search result node
                        break;
                    }
                }

                AddMainTreeViewRoot(f); //add the current search result node

                TreeNode sr = FindTreeNode(f, null);
                if (sr != null)
                {
                    MainTreeView.SelectedNode = sr; //navigate to the new search results node
                }
                return;
            }
            List<MainTreeFolder> hierarchy = new List<MainTreeFolder>();
            var pf = f;
            while (pf != null)
            {
                hierarchy.Add(pf);
                pf = pf.Parent;
            }
            TreeNode n = null;
            for (int i = hierarchy.Count - 1; i >= 0; i--)
            {
                n = FindTreeNode(hierarchy[i], n);
            }
            if (n != null)
            {
                if (MainTreeView.SelectedNode != n)
                {
                    MainTreeView.SelectedNode = n; //this will trigger everything else
                }
            }
            else
            {
                Navigate(f.Path); //try again using the path string...
                //NavigateFailed(f.Path); //unable to navigate...
            }
        }
        public void Navigate(string path)
        {
            if (!Ready) return;
            var pathl = path.ToLowerInvariant().Replace('/', '\\');
            if ((CurrentFolder != null) && (CurrentFolder.Path.ToLowerInvariant() == pathl)) return; //already there
            var hierarchy = pathl.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            TreeNode n = MainTreeView.Nodes[0];// FindTreeNode("gta v", null);
            if (!string.IsNullOrEmpty(path))
            {
                for (int i = 0; i < hierarchy.Length; i++)
                {
                    n = FindTreeNode(hierarchy[i], n);
                }
            }
            if (n != null)
            {
                if (MainTreeView.SelectedNode != n)
                {
                    MainTreeView.SelectedNode = n; //this will trigger everything else
                }
            }
            else
            {
                NavigateFailed(path); //unable to navigate...
            }
        }
        private void NavigateFailed(string path)
        {
            MessageBox.Show("Unable to navigate to \"" + path + "\".");
        }
        private void NavigateComplete(MainTreeFolder prevFolder)
        {
            //called after the CurrentFolder and CurrentFiles have changed. 

            UpdateNavigateUI();

            if (!HistoryNavigating) //only do this if not currently navigating forward or back
            {
                if (prevFolder != null)
                {
                    ForwardSteps.Clear();
                    BackSteps.Push(prevFolder);
                }
                UpdateHistoryUI();
            }

            GoButton.Enabled = true;
            RefreshButton.Enabled = true;
            SearchButton.Enabled = true;
        }

        public void GoUp(MainTreeFolder toFolder = null)
        {
            var fld = (toFolder == null) ? CurrentFolder?.Parent : toFolder;
            if (fld == null) return;
            Navigate(fld);
        }
        public void GoBack(MainTreeFolder toFolder = null)
        {
            if (BackSteps.Count == 0) return;
            var s = BackSteps.Pop();
            ForwardSteps.Push(CurrentFolder);
            while ((toFolder != null) && (s != toFolder) && (BackSteps.Count > 0))
            {
                ForwardSteps.Push(s);
                s = BackSteps.Pop();
            }
            HistoryNavigating = true;
            Navigate(s);
            HistoryNavigating = false;
            UpdateHistoryUI();
        }
        public void GoForward(MainTreeFolder toFolder = null)
        {
            if (ForwardSteps.Count == 0) return;
            var s = ForwardSteps.Pop();
            BackSteps.Push(CurrentFolder);
            while ((toFolder != null) && (s != toFolder) && (ForwardSteps.Count > 0))
            {
                BackSteps.Push(s);
                s = ForwardSteps.Pop();
            }
            HistoryNavigating = true;
            Navigate(s);
            HistoryNavigating = false;
            UpdateHistoryUI();
        }

        private void UpdateNavigateUI()
        {
            UpButton.DropDownItems.Clear();
            var pf = CurrentFolder?.Parent;
            int i = 0;
            while (pf != null)
            {
                var button = UpButton.DropDownItems.Add(pf.GetToolText());
                button.Tag = pf;
                button.Click += UpListButton_Click;
                pf = pf.Parent;
                if (i >= 10) break;
            }
            UpButton.Enabled = (UpButton.DropDownItems.Count > 0);
        }
        private void UpdateHistoryUI()
        {
            BackButton.DropDownItems.Clear();
            ForwardButton.DropDownItems.Clear();
            int i = 0;
            foreach (var step in BackSteps)
            {
                var button = BackButton.DropDownItems.Add(step.GetToolText());
                button.Tag = step;
                button.Click += BackListButton_Click;
                i++;
                if (i >= 10) break;
            }
            i = 0;
            foreach (var step in ForwardSteps)
            {
                var button = ForwardButton.DropDownItems.Add(step.GetToolText());
                button.Tag = step;
                button.Click += ForwardListButton_Click;
                i++;
                if (i >= 10) break;
            }
            BackButton.Enabled = (BackSteps.Count > 0);
            ForwardButton.Enabled = (ForwardSteps.Count > 0);
        }
        private void UpdateSelectionUI()
        {
            var ic = MainListView.VirtualListSize;
            var sc = MainListView.SelectedIndices.Count;
            var str = ic.ToString() + " item" + ((ic != 1) ? "s" : "") + " shown";
            bool isitem = false;
            bool isfile = false;
            bool canview = false;
            bool canedit = false;
            bool canexportxml = false;
            bool canimport = false;
            if (sc != 0)
            {
                long bc = 0;
                if (CurrentFiles != null)
                {
                    foreach (int idx in MainListView.SelectedIndices)
                    {
                        if ((idx < 0) || (idx >= CurrentFiles.Count)) continue;
                        var file = CurrentFiles[idx];
                        if ((file.Folder == null) || (file.Folder.RpfFile != null))
                        {
                            bc += file.FileSize;
                        }

                        isitem = true;
                        isfile = isfile || (file.Folder == null);
                        canview = canview || CanViewFile(file);

                    }
                }
                str += ", " + sc.ToString() + " selected";
                if (bc > 0)
                {
                    str += ", " + TextUtil.GetBytesReadable(bc);
                }
            }
            UpdateStatus(str);



            EditViewMenu.Enabled = canview;
            EditViewHexMenu.Enabled = isfile;

            EditExportXmlMenu.Enabled = canexportxml;
            EditExtractRawMenu.Enabled = isfile;

            EditImportRawMenu.Visible = canimport;
            EditImportXmlMenu.Visible = canimport;
            EditImportMenuSeparator.Visible = canimport;

            EditCopyMenu.Enabled = isfile;
            EditCopyPathMenu.Enabled = isitem;

            EditRenameMenu.Visible = canedit;
            EditReplaceMenu.Visible = canedit;
            EditDeleteMenu.Visible = canedit;
            EditEditModeMenuSeparator.Visible = canedit;

        }

        private TreeNode FindTreeNode(MainTreeFolder f, TreeNode parent)
        {
            var tnc = (parent != null) ? parent.Nodes : MainTreeView.Nodes;
            foreach (TreeNode node in tnc)
            {
                if (node.Tag == f)
                {
                    return node;
                }
            }
            return null;
        }
        private TreeNode FindTreeNode(string text, TreeNode parent)
        {
            var tnc = (parent != null) ? parent.Nodes : MainTreeView.Nodes;
            foreach (TreeNode node in tnc)
            {
                if (node.Text.ToLowerInvariant() == text)
                {
                    return node;
                }
            }
            return null;
        }



        private void RefreshMainTreeView()
        {
            Ready = false;

            var allRpfs = new List<RpfFile>();

            ClearMainTreeView();

            var path = Settings.Default.GTAFolder;
            var replpath = path.EndsWith("\\") ? path : path + "\\";

            string[] allfiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            Dictionary<string, MainTreeFolder> nodes = new Dictionary<string, MainTreeFolder>();

            MainTreeFolder root = new MainTreeFolder();
            root.FullPath = GetRootPath();
            root.Path = "";
            root.Name = "GTA V";
            RootFolder = root;

            UpdateStatus("Scanning...");

            foreach (var filepath in allfiles)
            {
                var relpath = filepath.Replace(replpath, "");
                var filepathl = filepath.ToLowerInvariant();

                UpdateStatus("Scanning " + relpath + "...");

                MainTreeFolder parentnode = null, prevnode = null, node = null;
                var prevnodepath = "";
                var idx = relpath.LastIndexOf('\\');
                while (idx > 0) //create the folder tree nodes and build up the hierarchy
                {
                    var parentpath = relpath.Substring(0, idx);
                    var parentidx = parentpath.LastIndexOf('\\');
                    var parentname = parentpath.Substring(parentidx + 1);
                    var exists = nodes.TryGetValue(parentpath, out node);
                    if (!exists)
                    {
                        node = new MainTreeFolder();
                        node.Name = parentname;
                        node.Path = parentpath;
                        node.FullPath = replpath + parentpath;
                        nodes[parentpath] = node;
                    }
                    if (parentnode == null)
                    {
                        parentnode = node;
                    }
                    if (prevnode != null)
                    {
                        node.AddChild(prevnode);
                    }
                    prevnode = node;
                    prevnodepath = parentpath;
                    idx = relpath.LastIndexOf('\\', idx - 1);
                    if (exists) break;
                    if (idx < 0)
                    {
                        root.AddChild(node);
                    }
                }

                if (filepathl.EndsWith(".rpf")) //add RPF nodes
                {
                    RpfFile rpf = new RpfFile(filepath, relpath);

                    rpf.ScanStructure(UpdateStatus, UpdateErrorLog);

                    node = new MainTreeFolder();
                    node.RpfFile = rpf;
                    node.RpfFolder = rpf.Root;
                    node.Name = rpf.Name;
                    node.Path = relpath;
                    node.FullPath = filepath;

                    RecurseMainTreeViewRPF(node, allRpfs);

                    if (parentnode != null)
                    {
                        parentnode.AddChild(node);
                    }
                    else
                    {
                        root.AddChild(node);
                    }
                }
                else
                {
                    if (parentnode != null)
                    {
                        parentnode.AddFile(filepath);
                    }
                    else
                    {
                        root.AddFile(filepath);
                    }
                }
            }


            AddMainTreeViewRoot(root);

            if (root.Children != null)
            {
                root.Children.Sort((n1, n2) => n1.Name.CompareTo(n2.Name));

                foreach (var node in root.Children)
                {
                    AddMainTreeViewNode(node);
                }
            }

            AllRpfs = allRpfs;

            Ready = true;

            MainTreeViewRefreshComplete();
        }
        private void RecurseMainTreeViewRPF(MainTreeFolder f, List<RpfFile> allRpfs)
        {
            var rootpath = GetRootPath();

            var fld = f.RpfFolder;
            if (fld != null)
            {
                if (fld.Directories != null)
                {
                    foreach (var dir in fld.Directories)
                    {
                        MainTreeFolder dtnf = new MainTreeFolder();
                        dtnf.RpfFolder = dir;
                        dtnf.Name = dir.Name;
                        dtnf.Path = dir.Path;
                        dtnf.FullPath = rootpath + dir.Path;
                        f.AddChild(dtnf);
                        RecurseMainTreeViewRPF(dtnf, allRpfs);
                    }
                }
            }

            var rpf = f.RpfFile;
            if (rpf != null)
            {
                allRpfs.Add(rpf);

                if (rpf.Children != null)
                {
                    foreach (var child in rpf.Children)
                    {
                        MainTreeFolder ctnf = new MainTreeFolder();
                        ctnf.RpfFile = child;
                        ctnf.RpfFolder = child.Root;
                        ctnf.Name = child.Name;
                        ctnf.Path = child.Path;
                        ctnf.FullPath = rootpath + child.Path;
                        f.AddChildToHierarchy(ctnf);
                        RecurseMainTreeViewRPF(ctnf, allRpfs);
                    }
                }

                JenkIndex.Ensure(rpf.Name);
                if (rpf.AllEntries != null)
                {
                    foreach (RpfEntry entry in rpf.AllEntries)
                    {
                        if (string.IsNullOrEmpty(entry.NameLower)) continue;
                        JenkIndex.Ensure(entry.Name);
                        JenkIndex.Ensure(entry.NameLower);
                        entry.NameHash = JenkHash.GenHash(entry.NameLower);
                        int ind = entry.Name.LastIndexOf('.');
                        if (ind > 0)
                        {
                            var shortname = entry.Name.Substring(0, ind);
                            var shortnamel = entry.NameLower.Substring(0, ind);
                            JenkIndex.Ensure(shortname);
                            JenkIndex.Ensure(shortnamel);
                            entry.ShortNameHash = JenkHash.GenHash(shortnamel);
                        }
                    }
                }
            }
        }
        private void ClearMainTreeView()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { ClearMainTreeView(); }));
                }
                else
                {
                    GoButton.Enabled = false;
                    UpButton.Enabled = false;
                    BackButton.Enabled = false;
                    ForwardButton.Enabled = false;
                    RefreshButton.Enabled = false;
                    SearchButton.Enabled = false;
                    MainTreeView.Nodes.Clear();
                    MainListView.VirtualListSize = 0; //also clear the list view...
                }
            }
            catch { }
        }
        private void AddMainTreeViewRoot(MainTreeFolder f)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { AddMainTreeViewRoot(f); }));
                }
                else
                {
                    var rn = MainTreeView.Nodes.Add(f.Path, f.Name, 0, 0); //ROOT imageIndex
                    rn.ToolTipText = f.FullPath;
                    rn.Tag = f;
                }
            }
            catch { }
        }
        private void AddMainTreeViewNode(MainTreeFolder f)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { AddMainTreeViewNode(f); }));
                }
                else
                {
                    string n = f.Name;
                    var root = (MainTreeView.Nodes.Count > 0) ? MainTreeView.Nodes[0] : null;

                    RecurseAddMainTreeViewNodes(f, root);

                    root.Expand();
                }
            }
            catch { }
        }
        private void RecurseAddMainTreeViewNodes(MainTreeFolder f, TreeNode parent)
        {
            int imgIndex = 1; //FOLDER imageIndex
            if (f.RpfFile != null) imgIndex = 3; //RPF FILE imageIndex

            var nc = (parent != null) ? parent.Nodes : MainTreeView.Nodes;

            var tn = nc.Add(f.Path, f.Name, imgIndex, imgIndex);

            tn.ToolTipText = f.Path;
            tn.Tag = f;

            if (f.Children != null)
            {
                f.Children.Sort((n1, n2) => n1.Name.CompareTo(n2.Name));

                foreach (var child in f.Children)
                {
                    RecurseAddMainTreeViewNodes(child, tn);
                }
            }
        }
        private void MainTreeViewRefreshComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { MainTreeViewRefreshComplete(); }));
                }
                else
                {
                    if (CurrentFolder != null)
                    {
                        if (CurrentFolder.IsSearchResults)
                        {
                            Search(CurrentFolder.SearchTerm);
                        }
                        else
                        {
                            var path = CurrentFolder.Path;
                            CurrentFolder = null;
                            Navigate(path);
                        }
                    }
                    else
                    {
                        Navigate(RootFolder);
                    }
                }
            }
            catch { }
        }

        private void RefreshMainListView()
        {
            MainListView.VirtualListSize = 0;
            if (CurrentFolder != null)
            {
                CurrentFiles = CurrentFolder.GetListItems();

                foreach (var file in CurrentFiles) //cache all the data for use by the list view.
                {
                    file.CacheDetails(this);
                }

                SortMainListView(SortColumnIndex, SortDirection); //sorts CurrentItems and sets VirtualListSize

                ShowMainListViewPathColumn(CurrentFolder.IsSearchResults);
            }
            else
            {
                UpdateSelectionUI();

                ShowMainListViewPathColumn(false);
            }
        }
        private void SortMainListView(int col, SortOrder dir)
        {
            if (dir == SortOrder.None) dir = SortOrder.Ascending; //none not supported for actual sorting!

            SortColumnIndex = col;
            SortDirection = dir;

            MainListView.SetSortIcon(col, dir);

            MainListView.VirtualListSize = 0;

            if (CurrentFiles == null) return;

            CurrentFiles.Sort((i1, i2) => i1.SortCompare(i2, col, dir));

            MainListView.VirtualListSize = CurrentFiles.Count;

            UpdateSelectionUI();
        }

        private void ShowMainListViewPathColumn(bool show)
        {
            bool visible = (MainPathColumnHeader.Width > 0);
            if (show && !visible)
            {
                MainPathColumnHeader.Width = PreviousPathColumnWidth;
            }
            else if (!show && visible)
            {
                PreviousPathColumnWidth = MainPathColumnHeader.Width;
                MainPathColumnHeader.Width = 0;
                if (SortColumnIndex == 4)//path col
                {
                    SortMainListView(0, SortDirection);//switch sort to name col
                }
            }
        }


        public void Search(string text)
        {
            if (!Ready) return;
            if (Searching) return;
            if (string.IsNullOrEmpty(text)) return;

            SearchTextBox.Text = text;
            SearchButton.Image = SearchGlobalButton.Image;
            SearchButton.Text = SearchGlobalButton.Text;
            SearchGlobalButton.Checked = true;
            SearchFilterButton.Checked = false;

            SearchResults = new MainTreeFolder();
            SearchResults.Name = "Search Results: " + text;
            SearchResults.Path = SearchResults.Name;
            SearchResults.IsSearchResults = true;
            SearchResults.SearchTerm = text;

            Navigate(SearchResults);

            SortDirection = SortOrder.None;
            MainListView.SetSortIcon(SortColumnIndex, SortDirection);
            MainListView.VirtualListSize = 0;

            CurrentFiles.Clear();

            UpdateStatus("Searching...");

            var term = text.ToLowerInvariant();

            //Task.Run(() =>
            //{
                Searching = true;

            Cursor = Cursors.WaitCursor;

                var resultcount = RootFolder.Search(term, this);

                if (Searching)
                {
                    Searching = false;
                    UpdateStatus("Search complete. " + resultcount.ToString() + " items found.");
                }
                else
                {
                    UpdateStatus("Search aborted. " + resultcount.ToString() + " items found.");
                }

            Cursor = Cursors.Default;

            //});
        }

        public void Filter(string text)
        {
            SearchTextBox.Text = text;
            SearchButton.Image = SearchFilterButton.Image;
            SearchButton.Text = SearchFilterButton.Text;
            SearchGlobalButton.Checked = false;
            SearchFilterButton.Checked = true;

            //TODO!
            MessageBox.Show("Filter TODO!");
        }


        public void AddSearchResult(MainListItem item)
        {
            if (SearchResults == null) return;
            if (SearchResults.ListItems != CurrentFiles) return;

            if (item != null)
            {
                item.CacheDetails(this);
                CurrentFiles.Add(item);
            }
            else
            {
                MainListView.VirtualListSize = CurrentFiles.Count;
            }

            //try
            //{
            //    if (InvokeRequired)
            //    {
            //        BeginInvoke(new Action(() => { AddSearchResultInner(item); }));
            //    }
            //    else
            //    {
            //        AddSearchResultInner(item);
            //    }
            //}
            //catch { }
        }




        private byte[] GetFileData(MainListItem file)
        {
            byte[] data = null;
            if (file.File != null)
            {
                //load file from RPF
                if (file.File.File == null) return null; //no RPF file? go no further
                data = file.File.File.ExtractFile(file.File);
            }
            else if (!string.IsNullOrEmpty(file.FullPath))
            {
                //load file from filesystem
                data = File.ReadAllBytes(file.FullPath);
            }
            return data;
        }

        private bool CanViewFile(MainListItem item)
        {
            if (item == null) return false;
            if (item.FileType == null) return false;
            switch (item.FileType.DefaultAction)
            {
                case FileTypeAction.ViewText:
                case FileTypeAction.ViewXml:
                case FileTypeAction.ViewYtd:
                case FileTypeAction.ViewYmt:
                case FileTypeAction.ViewYmf:
                case FileTypeAction.ViewYmap:
                case FileTypeAction.ViewYtyp:
                case FileTypeAction.ViewJPso:
                case FileTypeAction.ViewModel:
                case FileTypeAction.ViewCut:
                case FileTypeAction.ViewAwc:
                case FileTypeAction.ViewGxt:
                case FileTypeAction.ViewRel:
                case FileTypeAction.ViewFxc:
                case FileTypeAction.ViewYwr:
                case FileTypeAction.ViewYvr:
                    return true;
                case FileTypeAction.ViewHex:
                default:
                    break;
            }
            return false;
        }



        private void View(MainListItem item)
        {
#if !DEBUG
            try
#endif
            {
                byte[] data = null;
                string name = "";
                string path = "";
                if (item.File != null)
                {
                    //load file from RPF
                    if (item.File.File == null) return; //no RPF file? go no further
                    data = item.File.File.ExtractFile(item.File);
                    name = item.Name;
                    path = item.FullPath;
                }
                else if (!string.IsNullOrEmpty(item.FullPath))
                {
                    //load file from filesystem
                    data = File.ReadAllBytes(item.FullPath);
                    name = new FileInfo(item.FullPath).Name;
                    path = item.FullPath;
                }

                if (data == null) return;

                var ft = item.FileType;
                switch (ft.DefaultAction)
                {
                    case FileTypeAction.ViewText:
                        ViewText(name, path, data);
                        break;
                    case FileTypeAction.ViewXml:
                        ViewXml(name, path, data);
                        break;
                    case FileTypeAction.ViewYtd:
                        ViewYtd(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewYmt:
                        ViewYmt(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewYmf:
                        ViewYmf(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewYmap:
                        ViewYmap(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewYtyp:
                        ViewYtyp(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewJPso:
                        ViewJPso(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewCut:
                        ViewCut(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewModel:
                        ViewModel(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewAwc:
                        ViewAwc(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewGxt:
                        ViewGxt(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewRel:
                        ViewRel(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewFxc:
                        ViewFxc(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewYwr:
                        ViewYwr(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewYvr:
                        ViewYvr(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewHex:
                    default:
                        ViewHex(name, path, data);
                        break;
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                UpdateErrorLog(ex.ToString());
                return;
            }
#endif
        }
        private void ViewHex(MainListItem item)
        {
            try
            {
                byte[] data = null;
                string name = "";
                string path = "";
                if (item.File != null)
                {
                    //load file from RPF
                    if (item.File.File == null) return; //no RPF file? go no further
                    data = item.File.File.ExtractFile(item.File);
                    name = item.Name;
                    path = item.FullPath;
                }
                else if (!string.IsNullOrEmpty(item.FullPath))
                {
                    //load file from filesystem
                    data = File.ReadAllBytes(item.FullPath);
                    name = new FileInfo(item.FullPath).Name;
                    path = item.FullPath;
                }

                if (data == null) return;

                ViewHex(name, path, data);
            }
            catch (Exception ex)
            {
                UpdateErrorLog(ex.ToString());
                return;
            }
        }
        private void ViewHex(string name, string path, byte[] data)
        {
            HexForm f = new HexForm();
            f.Show();
            f.LoadData(name, path, data);
        }
        private void ViewXml(string name, string path, byte[] data)
        {
            string xml = Encoding.UTF8.GetString(data);
            XmlForm f = new XmlForm();
            f.Show();
            f.LoadXml(name, path, xml);
        }
        private void ViewText(string name, string path, byte[] data)
        {
            string txt = Encoding.UTF8.GetString(data);
            TextForm f = new TextForm();
            f.Show();
            f.LoadText(name, path, txt);
        }
        private void ViewYtd(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ytd = RpfFile.GetFile<YtdFile>(e, data);
            YtdForm f = new YtdForm();
            f.Show();
            f.LoadYtd(ytd);
        }
        private void ViewYmt(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ymt = RpfFile.GetFile<YmtFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(ymt);
        }
        private void ViewYmf(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ymf = RpfFile.GetFile<YmfFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(ymf);
        }
        private void ViewYmap(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ymap = RpfFile.GetFile<YmapFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(ymap);
        }
        private void ViewYtyp(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ytyp = RpfFile.GetFile<YtypFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(ytyp);
        }
        private void ViewJPso(string name, string path, byte[] data, RpfFileEntry e)
        {
            var pso = RpfFile.GetFile<JPsoFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(pso);
        }
        private void ViewModel(string name, string path, byte[] data, RpfFileEntry e)
        {
            var nl = e?.NameLower ?? "";
            var fe = new FileInfo(nl).Extension;
            ModelForm f = new ModelForm(this);
            f.Show();
            switch (fe)
            {
                case ".ydr":
                    var ydr = RpfFile.GetFile<YdrFile>(e, data);
                    f.LoadModel(ydr);
                    break;
                case ".ydd":
                    var ydd = RpfFile.GetFile<YddFile>(e, data);
                    f.LoadModels(ydd);
                    break;
                case ".yft":
                    var yft = RpfFile.GetFile<YftFile>(e, data);
                    f.LoadModel(yft);
                    break;
                case ".ybn":
                    var ybn = RpfFile.GetFile<YbnFile>(e, data);
                    f.LoadModel(ybn);
                    break;
                case ".ypt":
                    var ypt = RpfFile.GetFile<YptFile>(e, data);
                    f.LoadParticles(ypt);
                    break;
                case ".ynv":
                    var ynv = RpfFile.GetFile<YnvFile>(e, data);
                    f.LoadNavmesh(ynv);
                    break;
            }
        }
        private void ViewCut(string name, string path, byte[] data, RpfFileEntry e)
        {
            var cut = RpfFile.GetFile<CutFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(cut);
        }
        private void ViewAwc(string name, string path, byte[] data, RpfFileEntry e)
        {
            var awc = RpfFile.GetFile<AwcFile>(e, data);
            AwcForm f = new AwcForm();
            f.Show();
            f.LoadAwc(awc);
        }
        private void ViewGxt(string name, string path, byte[] data, RpfFileEntry e)
        {
            var gxt = RpfFile.GetFile<Gxt2File>(e, data);
            GxtForm f = new GxtForm();
            f.Show();
            f.LoadGxt2(gxt);
        }
        private void ViewRel(string name, string path, byte[] data, RpfFileEntry e)
        {
            var rel = RpfFile.GetFile<RelFile>(e, data);
            RelForm f = new RelForm();
            f.Show();
            f.LoadRel(rel);
        }
        private void ViewFxc(string name, string path, byte[] data, RpfFileEntry e)
        {
            var fxc = RpfFile.GetFile<FxcFile>(e, data);
            FxcForm f = new FxcForm();
            f.Show();
            f.LoadFxc(fxc);
        }
        private void ViewYwr(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ywr = RpfFile.GetFile<YwrFile>(e, data);
            YwrForm f = new YwrForm();
            f.Show();
            f.LoadYwr(ywr);
        }
        private void ViewYvr(string name, string path, byte[] data, RpfFileEntry e)
        {
            var yvr = RpfFile.GetFile<YvrFile>(e, data);
            YvrForm f = new YvrForm();
            f.Show();
            f.LoadYvr(yvr);
        }



        private void ShowTreeContextMenu(TreeNode n, Point p)
        {
            var f = n?.Tag as MainTreeFolder;

            bool filesys = ((f.RpfFolder == null) && (f.RpfFile == null));
            bool expanded = ((n != null) && (n.IsExpanded));
            bool collapsed = ((n != null) && (!n.IsExpanded));

            if ((f.RpfFile != null) && (f.RpfFile.Parent == null))
            {
                filesys = true; //allow viewing root RPF's in explorer
            }

            TreeContextWinExplorerMenu.Enabled = filesys;
            TreeContextExpandMenu.Enabled = collapsed;
            TreeContextCollapseMenu.Enabled = expanded;


            TreeContextMenu.Show(MainTreeView, p);

        }

        private void ShowListContextMenu(MainListItem item)
        {

            bool isitem = false;
            bool isfile = false;
            bool isfolder = false;
            bool isfilesys = false;
            bool canview = false;
            bool canexportxml = false;
            bool editmode = false;//todo: set this for edit mode
            bool canimport = editmode;
            bool canedit = false;
            bool issearch = CurrentFolder?.IsSearchResults ?? false;

            if (item != null)
            {
                isfolder = (item.Folder != null);
                isfilesys = (item.File == null) && (item.Folder == null);
                canview = CanViewFile(item);
                isitem = true;
                isfile = !isfolder;
                canedit = editmode && !canimport;
            }


            ListContextViewMenu.Enabled = canview;
            ListContextViewHexMenu.Enabled = isfile;

            ListContextExportXmlMenu.Enabled = canexportxml;
            ListContextExtractRawMenu.Enabled = isfile;

            ListContextImportRawMenu.Visible = canimport;
            ListContextImportXmlMenu.Visible = canimport;
            ListContextImportSeparator.Visible = canimport;

            ListContextCopyMenu.Enabled = isfile;
            ListContextCopyPathMenu.Enabled = isitem;

            ListContextOpenFileLocationMenu.Visible = issearch;
            ListContextOpenFileLocationSeparator.Visible = issearch;

            ListContextRenameMenu.Visible = canedit;
            ListContextReplaceMenu.Visible = canedit;
            ListContextDeleteMenu.Visible = canedit;
            ListContextEditSeparator.Visible = canedit;

            ListContextMenu.Show(Cursor.Position);

        }



        private void ViewSelected()
        {
            for (int i = 0; i < MainListView.SelectedIndices.Count; i++)
            {
                var idx = MainListView.SelectedIndices[i];
                if ((idx < 0) || (idx >= CurrentFiles.Count)) continue;
                var file = CurrentFiles[idx];
                if (file.Folder == null)
                {
                    View(file);
                }
            }
        }
        private void ViewSelectedHex()
        {
            for (int i = 0; i < MainListView.SelectedIndices.Count; i++)
            {
                var idx = MainListView.SelectedIndices[i];
                if ((idx < 0) || (idx >= CurrentFiles.Count)) continue;
                var file = CurrentFiles[idx];
                if (file.Folder == null)
                {
                    ViewHex(file);
                }
            }
        }
        private void ExportXml()
        {
            MessageBox.Show("Export XML TODO!");
            for (int i = 0; i < MainListView.SelectedIndices.Count; i++)
            {
                var idx = MainListView.SelectedIndices[i];
                if ((idx < 0) || (idx >= CurrentFiles.Count)) continue;
                var file = CurrentFiles[idx];
                if (file.Folder == null)
                {
                    //todo: meta convert to XML and export...
                }
            }
        }
        private void ExtractRaw()
        {
            if (MainListView.SelectedIndices.Count == 1)
            {
                var idx = MainListView.SelectedIndices[0];
                if ((idx < 0) || (idx >= CurrentFiles.Count)) return;
                var file = CurrentFiles[idx];
                if (file.Folder == null)
                {
                    byte[] data = GetFileData(file);
                    if (data == null)
                    {
                        MessageBox.Show("Unable to extract file: " + file.Path);
                        return;
                    }

                    SaveFileDialog.FileName = file.Name;
                    if (SaveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string path = SaveFileDialog.FileName;
                        try
                        {
                            File.WriteAllBytes(path, data);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error saving file " + path + ":\n" + ex.ToString());
                        }
                    }
                }
            }
            else
            {
                if (FolderBrowserDialog.ShowDialog() != DialogResult.OK) return;
                string folderpath = FolderBrowserDialog.SelectedPath;
                if (!folderpath.EndsWith("\\")) folderpath += "\\";

                StringBuilder errors = new StringBuilder();

                for (int i = 0; i < MainListView.SelectedIndices.Count; i++)
                {
                    var idx = MainListView.SelectedIndices[i];
                    if ((idx < 0) || (idx >= CurrentFiles.Count)) continue;
                    var file = CurrentFiles[idx];
                    if (file.Folder == null)
                    {
                        var path = folderpath + file.Name;
                        var data = GetFileData(file);
                        if (data == null)
                        {
                            errors.AppendLine("Unable to extract file: " + file.Path);
                            continue;
                        }
                        try
                        {
                            File.WriteAllBytes(path, data);
                        }
                        catch (Exception ex)
                        {
                            errors.AppendLine("Error saving file " + path + ":\n" + ex.ToString());
                        }
                    }
                }

                string errstr = errors.ToString();
                if (!string.IsNullOrEmpty(errstr))
                {
                    MessageBox.Show("Errors were encountered:\n" + errstr);
                }
            }
        }
        private void ExtractAll()
        {
            if (CurrentFiles == null) return;
            if (FolderBrowserDialog.ShowDialog() != DialogResult.OK) return;
            string folderpath = FolderBrowserDialog.SelectedPath;
            if (!folderpath.EndsWith("\\")) folderpath += "\\";

            StringBuilder errors = new StringBuilder();

            foreach (var file in CurrentFiles)
            {
                if (file.Folder == null)
                {
                    var path = folderpath + file.Name;
                    var data = GetFileData(file);
                    if (data == null)
                    {
                        errors.AppendLine("Unable to extract file: " + file.Path);
                        continue;
                    }
                    try
                    {
                        File.WriteAllBytes(path, data);
                    }
                    catch (Exception ex)
                    {
                        errors.AppendLine("Error saving file " + path + ":\n" + ex.ToString());
                    }
                }
            }

            string errstr = errors.ToString();
            if (!string.IsNullOrEmpty(errstr))
            {
                MessageBox.Show("Errors were encountered:\n" + errstr);
            }
        }
        private void ImportXml()
        {
            if (!EditMode) return;
            MessageBox.Show("Edit mode functions not yet implemented...");
        }
        private void ImportRaw()
        {
            if (!EditMode) return;
            MessageBox.Show("Edit mode functions not yet implemented...");
        }
        private void CopySelected()
        {
            //only really for edit mode...
            MessageBox.Show("CopySelected TODO!");
        }
        private void CopyPath()
        {
            if (MainListView.SelectedIndices.Count == 0)
            {
                Clipboard.SetText(CurrentFolder?.FullPath ?? GetRootPath());
            }
            else if (MainListView.SelectedIndices.Count == 1)
            {
                var idx = MainListView.SelectedIndices[0];
                if ((idx < 0) || (idx >= CurrentFiles.Count)) return;
                var f = CurrentFiles[idx];
                if (f.FullPath != null)
                {
                    Clipboard.SetText(f.FullPath);
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (int idx in MainListView.SelectedIndices)
                {
                    if ((idx < 0) || (idx >= CurrentFiles.Count)) return;
                    var f = CurrentFiles[idx];
                    if (f.FullPath != null)
                    {
                        sb.AppendLine(f.FullPath);
                    }
                }
                Clipboard.SetText(sb.ToString());
            }
        }
        private void CopyPath(string path)
        {
            Clipboard.SetText(path);
        }
        private void CopyFileList()
        {
            StringBuilder sb = new StringBuilder();
            if (CurrentFiles != null)
            {
                foreach (var file in CurrentFiles)
                {
                    sb.AppendLine(file.Name);
                }
            }
            Clipboard.SetText(sb.ToString());
        }
        private void RenameSelected()
        {
            if (!EditMode) return;
            if (MainListView.SelectedItems.Count != 1) return;
            MessageBox.Show("Edit mode functions not yet implemented...");
        }
        private void ReplaceSelected()
        {
            if (!EditMode) return;
            if (MainListView.SelectedItems.Count != 1) return;
            MessageBox.Show("Edit mode functions not yet implemented...");
        }
        private void DeleteSelected()
        {
            if (!EditMode) return;
            MessageBox.Show("Edit mode functions not yet implemented...");
        }
        private void SelectAll()
        {
            MainListView.SelectAllItems();
            UpdateSelectionUI();
        }
        private void ShowInExplorer(string path)
        {
            try
            {
                Process.Start("explorer", "/select, \"" + path + "\"");
            }
            catch (Exception ex)
            {
                UpdateErrorLog(ex.ToString());
            }
        }
        private void OpenFileLocation()
        {
            var ind = -1;
            if (MainListView.SelectedIndices.Count == 1)
            {
                ind = MainListView.SelectedIndices[0];
            }
            if ((CurrentFiles != null) && (CurrentFiles.Count > ind))
            {
                var file = CurrentFiles[ind];
                var path = file.Path.Replace('/', '\\');
                var bsind = path.LastIndexOf('\\');
                if (bsind > 0)
                {
                    path = path.Substring(0, bsind);
                }
                Navigate(path);
            }
        }


        private void SetView(View v)
        {

            MainListView.View = v;

            foreach (ToolStripMenuItem menu in ViewMenu.DropDownItems)
            {
                menu.Checked = false;
            }

            switch (v)
            {
                case System.Windows.Forms.View.LargeIcon:
                    ViewLargeIconsMenu.Checked = true;
                    break;
                case System.Windows.Forms.View.SmallIcon:
                    ViewSmallIconsMenu.Checked = true;
                    break;
                case System.Windows.Forms.View.List:
                    ViewListMenu.Checked = true;
                    break;
                case System.Windows.Forms.View.Details:
                    ViewDetailsMenu.Checked = true;
                    break;
            }

        }


        private void ExploreForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void MainTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var prev = CurrentFolder;
            CurrentFolder = e.Node?.Tag as MainTreeFolder;
            LocationTextBox.Text = CurrentFolder?.Path ?? "";
            if (!string.IsNullOrEmpty(CurrentFolder?.SearchTerm))
            {
                SearchTextBox.Text = CurrentFolder.SearchTerm;
            }
            RefreshMainListView();
            NavigateComplete(prev);
        }

        private void MainTreeView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

                // Point where the mouse is clicked.
                Point p = new Point(e.X, e.Y);

                // Get the node that the user has clicked.
                TreeNode node = MainTreeView.GetNodeAt(p);
                if (node != null)
                {

                    // Select the node the user has clicked.
                    // The node appears selected until the menu is displayed on the screen.
                    //var oldNode = MainTreeView.SelectedNode;
                    MainTreeView.SelectedNode = node;

                    ShowTreeContextMenu(node, p);

                    // Highlight the selected node.
                    //MainTreeView.SelectedNode = oldNode;
                }
            }
        }

        private void MainListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var lvi = new ListViewItem();
            if ((CurrentFiles != null) && (e.ItemIndex < CurrentFiles.Count))
            {
                var file = CurrentFiles[e.ItemIndex];
                var fld = file.Folder;
                lvi.Tag = file;
                lvi.Text = file.Name;
                lvi.ToolTipText = file.Path;
                lvi.ImageIndex = file.ImageIndex;
                lvi.SubItems.Add(file.FileTypeText); //type column
                lvi.SubItems.Add(file.FileSizeText); //size column
                lvi.SubItems.Add(file.Attributes); //attributes column
                lvi.SubItems.Add(file.Path); //path column
            }
            e.Item = lvi;
        }

        private void MainListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var newdir = SortDirection;
            var idx = e.Column;
            if (idx == SortColumnIndex)
            {
                switch (SortDirection)
                {
                    case SortOrder.None: newdir = SortOrder.Ascending; break;
                    case SortOrder.Ascending: newdir = SortOrder.Descending; break;
                    case SortOrder.Descending: newdir = SortOrder.Ascending; break;
                }
            }

            SortMainListView(idx, newdir);
        }

        private void MainListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                MainListItem item = null;
                if ((MainListView.FocusedItem != null) && (MainListView.FocusedItem.Bounds.Contains(e.Location)))
                {
                    item = MainListView.FocusedItem.Tag as MainListItem;
                }
                ShowListContextMenu(item);
            }

            UpdateSelectionUI(); //need to use this instead of SelectedIndexChanged because of shift-click bug :/
        }

        private void MainListView_KeyDown(object sender, KeyEventArgs e)
        {
            var ctrl = (e.Control && !e.Shift);
            var ctrlshft = (e.Control && e.Shift);
            var shft = (e.Shift && !e.Control);

            switch (e.KeyCode)
            {
                case Keys.P:
                    if (ctrl) ViewSelected();
                    break;
                case Keys.H:
                    if (ctrl) ViewSelectedHex();
                    break;
                case Keys.S:
                    if (ctrl) ExportXml();
                    break;
                case Keys.E:
                    if (ctrlshft) ExtractAll();
                    else if (ctrl) ExtractRaw();
                    break;
                case Keys.Insert:
                    if (MainListView.SelectedIndices.Count == 1)
                    {
                        if (shft) ReplaceSelected();
                    }
                    else
                    {
                        if (shft) ImportXml();
                        else if (!ctrl) ImportRaw();
                    }
                    break;
                case Keys.C:
                    if (ctrlshft) CopyPath();
                    else if (ctrl) CopySelected();
                    break;
                case Keys.F2:
                    RenameSelected();
                    break;
                case Keys.Delete:
                    if (shft) DeleteSelected();
                    break;
                case Keys.A:
                    if (ctrl) SelectAll();
                    break;
            }
        }

        private void MainListView_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateSelectionUI(); //need to use this instead of SelectedIndexChanged because of shift-click bug :/
        }

        private void MainListView_ItemActivate(object sender, EventArgs e)
        {
            if (MainListView.SelectedIndices.Count == 1)
            {
                var idx = MainListView.SelectedIndices[0];
                if ((idx >= 0) && (idx < CurrentFiles.Count))
                {
                    var file = CurrentFiles[idx];
                    if (file.Folder != null)
                    {
                        Navigate(file.Folder);
                    }
                    else
                    {
                        //a file was activated. open it... (or, perform "default action"?)
                        View(file);
                    }
                }
            }
            else
            {
                //how to activate multiple items? open files?
            }
        }

        private void BackButton_ButtonClick(object sender, EventArgs e)
        {
            GoBack();
        }

        private void BackListButton_Click(object sender, EventArgs e)
        {
            var step = (sender as ToolStripItem)?.Tag as MainTreeFolder;
            if (step == null) return;
            GoBack(step);
        }

        private void ForwardButton_ButtonClick(object sender, EventArgs e)
        {
            GoForward();
        }

        private void ForwardListButton_Click(object sender, EventArgs e)
        {
            var step = (sender as ToolStripItem)?.Tag as MainTreeFolder;
            if (step == null) return;
            GoForward(step);
        }

        private void UpButton_ButtonClick(object sender, EventArgs e)
        {
            GoUp();
        }

        private void UpListButton_Click(object sender, EventArgs e)
        {
            var step = (sender as ToolStripItem)?.Tag as MainTreeFolder;
            if (step == null) return;
            GoUp(step);
        }

        private void LocationTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Navigate(LocationTextBox.Text);
                e.Handled = true;
            }
        }

        private void LocationTextBox_Enter(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() => LocationTextBox.SelectAll()));
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            Navigate(LocationTextBox.Text);
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                UpdateStatus("Scanning...");

                RefreshMainTreeView();

                UpdateStatus("Scan complete.");
            });
        }

        private void SearchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                SearchButton_ButtonClick(sender, e);
                e.Handled = true;
            }
        }

        private void SearchTextBox_Enter(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() => SearchTextBox.SelectAll()));
        }

        private void SearchButton_ButtonClick(object sender, EventArgs e)
        {
            if (SearchGlobalButton.Checked)
            {
                Search(SearchTextBox.Text);
            }
            else
            {
                Filter(SearchTextBox.Text);
            }
        }

        private void SearchGlobalButton_Click(object sender, EventArgs e)
        {
            Search(SearchTextBox.Text);
        }

        private void SearchFilterButton_Click(object sender, EventArgs e)
        {
            Filter(SearchTextBox.Text);
        }

        private void TreeContextCopyPathMenu_Click(object sender, EventArgs e)
        {
            var f = MainTreeView.SelectedNode?.Tag as MainTreeFolder;
            if (f != null)
            {
                CopyPath(f.FullPath);
            }
        }

        private void TreeContextWinExplorerMenu_Click(object sender, EventArgs e)
        {
            var folder = MainTreeView.SelectedNode?.Tag as MainTreeFolder;
            var path = folder?.FullPath ?? GetRootPath();
            ShowInExplorer(path);
        }

        private void TreeContextExpandMenu_Click(object sender, EventArgs e)
        {
            if (MainTreeView.SelectedNode != null)
            {
                MainTreeView.SelectedNode.Expand();
            }
        }

        private void TreeContextCollapseMenu_Click(object sender, EventArgs e)
        {
            if (MainTreeView.SelectedNode != null)
            {
                MainTreeView.SelectedNode.Collapse();
            }
        }

        private void TreeContextCollapseAllMenu_Click(object sender, EventArgs e)
        {
            var seln = MainTreeView.SelectedNode;
            foreach (TreeNode root in MainTreeView.Nodes)
            {
                foreach (TreeNode n in root.Nodes)
                {
                    n.Collapse(false); //collapse the first level nodes (not the roots)
                }
            }
            if (MainTreeView.SelectedNode != seln)
            {
                TreeViewEventArgs tve = new TreeViewEventArgs(MainTreeView.SelectedNode);
                MainTreeView_AfterSelect(MainTreeView, tve); //for some reason, this event doesn't get raised when the selected node changes here
            }
        }

        private void ListContextViewMenu_Click(object sender, EventArgs e)
        {
            ViewSelected();
        }

        private void ListContextViewHexMenu_Click(object sender, EventArgs e)
        {
            ViewSelectedHex();
        }

        private void ListContextExportXmlMenu_Click(object sender, EventArgs e)
        {
            ExportXml();
        }

        private void ListContextExtractRawMenu_Click(object sender, EventArgs e)
        {
            ExtractRaw();
        }

        private void ListContextExtractAllMenu_Click(object sender, EventArgs e)
        {
            ExtractAll();
        }

        private void ListContextImportXmlMenu_Click(object sender, EventArgs e)
        {
            ImportXml();
        }

        private void ListContextImportRawMenu_Click(object sender, EventArgs e)
        {
            ImportRaw();
        }

        private void ListContextCopyMenu_Click(object sender, EventArgs e)
        {
            CopySelected();
        }

        private void ListContextCopyPathMenu_Click(object sender, EventArgs e)
        {
            CopyPath();
        }

        private void ListContextCopyFileListMenu_Click(object sender, EventArgs e)
        {
            CopyFileList();
        }

        private void ListContextOpenFileLocationMenu_Click(object sender, EventArgs e)
        {
            OpenFileLocation();
        }

        private void ListContextRenameMenu_Click(object sender, EventArgs e)
        {
            RenameSelected();
        }

        private void ListContextReplaceMenu_Click(object sender, EventArgs e)
        {
            ReplaceSelected();
        }

        private void ListContextDeleteMenu_Click(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        private void ListContextSelectAllMenu_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void FileExitMenu_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void EditViewMenu_Click(object sender, EventArgs e)
        {
            ViewSelected();
        }

        private void EditViewHexMenu_Click(object sender, EventArgs e)
        {
            ViewSelectedHex();
        }

        private void EditExportXmlMenu_Click(object sender, EventArgs e)
        {
            ExportXml();
        }

        private void EditExtractRawMenu_Click(object sender, EventArgs e)
        {
            ExtractRaw();
        }

        private void EditExtractAllMenu_Click(object sender, EventArgs e)
        {
            ExtractAll();
        }

        private void EditImportXmlMenu_Click(object sender, EventArgs e)
        {
            ImportXml();
        }

        private void EditImportRawMenu_Click(object sender, EventArgs e)
        {
            ImportRaw();
        }

        private void EditCopyMenu_Click(object sender, EventArgs e)
        {
            CopySelected();
        }

        private void EditCopyPathMenu_Click(object sender, EventArgs e)
        {
            CopyPath();
        }

        private void EditCopyFileListMenu_Click(object sender, EventArgs e)
        {
            CopyFileList();
        }

        private void EditRenameMenu_Click(object sender, EventArgs e)
        {
            RenameSelected();
        }

        private void EditReplaceMenu_Click(object sender, EventArgs e)
        {
            ReplaceSelected();
        }

        private void EditDeleteMenu_Click(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        private void EditSelectAllMenu_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void ViewLargeIconsMenu_Click(object sender, EventArgs e)
        {
            SetView(System.Windows.Forms.View.LargeIcon);
        }

        private void ViewSmallIconsMenu_Click(object sender, EventArgs e)
        {
            SetView(System.Windows.Forms.View.SmallIcon);
        }

        private void ViewListMenu_Click(object sender, EventArgs e)
        {
            SetView(System.Windows.Forms.View.List);
        }

        private void ViewDetailsMenu_Click(object sender, EventArgs e)
        {
            SetView(System.Windows.Forms.View.Details);
        }

        private void ToolsOptionsMenu_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Options TODO!");
        }

        private void ToolsRpfBrowserMenu_Click(object sender, EventArgs e)
        {
            BrowseForm f = new BrowseForm();
            f.Show(this);
        }
    }



    public class MainTreeFolder
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string FullPath { get; set; }
        public RpfFile RpfFile { get; set; }
        public RpfDirectoryEntry RpfFolder { get; set; }
        public List<string> Files { get; set; }
        public MainTreeFolder Parent { get; set; }
        public List<MainTreeFolder> Children { get; set; }
        public List<MainListItem> ListItems { get; set; }
        public bool IsSearchResults { get; set; }
        public string SearchTerm { get; set; }

        public void AddFile(string file)
        {
            if (Files == null) Files = new List<string>();
            Files.Add(file);
        }
        public void AddChild(MainTreeFolder child)
        {
            if (Children == null) Children = new List<MainTreeFolder>();
            Children.Add(child);
            child.Parent = this;
        }
        public void AddChildToHierarchy(MainTreeFolder child)
        {
            var relpath = child.Path.Replace(Path + "\\", "");
            var idx = relpath.IndexOf('\\');
            var lidx = 0;
            var parent = this;
            while (idx > 0)
            {
                var pname = relpath.Substring(lidx, idx - lidx);
                if (parent.Children == null) break;
                foreach (var tc in parent.Children)
                {
                    if (tc.Name == pname)
                    {
                        parent = tc;
                        break;
                    }
                }
                lidx = idx + 1;
                if (lidx >= relpath.Length) break;
                idx = relpath.IndexOf('\\', lidx);
            }
            parent.AddChild(child);
        }

        public List<MainListItem> GetListItems()
        {
            if (ListItems == null)
            {
                ListItems = new List<MainListItem>();
                var rootpath = ExploreForm.GetRootPath();

                if (Children != null)
                {
                    foreach (var child in Children)
                    {
                        ListItems.Add(new MainListItem(child));
                    }
                }
                if (Files != null)
                {
                    foreach (var file in Files)
                    {
                        ListItems.Add(new MainListItem(file, rootpath, this));
                    }
                }
                if ((RpfFolder != null) && (RpfFolder.Files != null))
                {
                    foreach (var file in RpfFolder.Files)
                    {
                        if (file.NameLower.EndsWith(".rpf")) continue; //RPF files are already added..
                        ListItems.Add(new MainListItem(file, rootpath, this));
                    }
                }
            }
            return ListItems;
        }
        public int GetItemCount()
        {
            int ic = 0;
            if (Children != null) ic += Children.Count;
            if (Files != null) ic += Files.Count;
            if ((RpfFolder != null) && (RpfFolder.Files != null))
            {
                foreach (var file in RpfFolder.Files)
                {
                    if (file.NameLower.EndsWith(".rpf")) continue; //RPF files are already added..
                    ic++;
                }
            }
            return ic;
        }

        public string GetToolText()
        {
            if (string.IsNullOrEmpty(Path)) return Name;
            return Path;
        }

        public int Search(string term, ExploreForm form)
        {
            int resultcount = 0;
            //if (!form.Searching) return resultcount;

            form.UpdateStatus("Searching " + Path + "...");

            if (Name.ToLowerInvariant().Contains(term))
            {
                form.AddSearchResult(new MainListItem(this));
                resultcount++;
            }

            var rootpath = ExploreForm.GetRootPath();

            if (Files != null)
            {
                foreach (var file in Files)
                {
                    //if (!form.Searching) return resultcount;
                    var fi = new FileInfo(file);
                    if (fi.Name.ToLowerInvariant().Contains(term))
                    {
                        form.AddSearchResult(new MainListItem(file, rootpath, this));
                        resultcount++;
                    }
                }
            }
            if ((RpfFolder != null) && (RpfFolder.Files != null))
            {
                foreach (var file in RpfFolder.Files)
                {
                    //if (!form.Searching) return resultcount;
                    if (file.NameLower.EndsWith(".rpf")) continue; //don't search rpf files..
                    if (file.NameLower.Contains(term))
                    {
                        form.AddSearchResult(new MainListItem(file, rootpath, this));
                        resultcount++;
                    }
                }
            }

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    //if (!form.Searching) return resultcount;
                    resultcount += child.Search(term, form);
                }
            }

            form.AddSearchResult(null);

            return resultcount;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MainListItem
    {
        public string Name { get; set; }
        public MainTreeFolder Parent { get; set; }
        public MainTreeFolder Folder { get; set; }
        public RpfFileEntry File { get; set; }
        public string Path { get; set; }
        public string FullPath { get; set; }

        public FileTypeInfo FileType { get; set; }
        public string FileTypeText { get; set; }
        public long FileSize { get; set; }
        public string FileSizeText { get; set; }
        public string Attributes { get; set; }
        public int ImageIndex { get; set; }


        public MainListItem(MainTreeFolder f)
        {
            Parent = f?.Parent;
            Folder = f;
            Name = f.Name;
            Path = f.Path;
            FullPath = f.FullPath;
        }
        public MainListItem(string file, string rootpath, MainTreeFolder parent)
        {
            Parent = parent;
            Name = new FileInfo(file).Name;
            Path = file.Replace(rootpath, "");
            FullPath = file;
        }
        public MainListItem(RpfFileEntry file, string rootpath, MainTreeFolder parent)
        {
            Parent = parent;
            File = file;
            Name = file.Name;
            Path = file.Path;
            FullPath = rootpath + file.Path;
        }

        public override string ToString()
        {
            return Name;
        }

        public void CacheDetails(ExploreForm form)
        {
            var fld = Folder;
            FileType = form.GetFileType(Name);
            FileTypeText = FileType.Name;
            FileSizeText = "";
            Attributes = "";
            ImageIndex = FileType.ImageIndex;
            if (File != null)
            {
                FileSize = File.GetFileSize();
                FileSizeText = TextUtil.GetBytesReadable(FileSize);
                if (File is RpfResourceFileEntry)
                {
                    var resf = File as RpfResourceFileEntry;
                    Attributes += "Resource [V." + resf.Version.ToString() + "]";
                }
                if (File.IsEncrypted)
                {
                    if (Attributes.Length > 0) Attributes += ", ";
                    Attributes += "Encrypted";
                }
            }
            else if (fld != null)
            {
                if (fld.RpfFile != null)
                {
                    FileSize = fld.RpfFile.FileSize;
                    FileSizeText = TextUtil.GetBytesReadable(FileSize);
                }
                else
                {
                    FileTypeText = "Folder";
                    ImageIndex = 1; //FOLDER imageIndex
                    var ic = fld.GetItemCount();
                    FileSize = ic;
                    FileSizeText = ic.ToString() + " item" + ((ic != 1) ? "s" : "");
                }
            }
            else
            {
                var fi = new FileInfo(FullPath);
                if (fi.Exists)
                {
                    FileSize = fi.Length;
                    FileSizeText = TextUtil.GetBytesReadable(fi.Length);
                }
            }

        }

        public int SortCompare(MainListItem i, int col, SortOrder dir)
        {
            var desc = (dir == SortOrder.Descending);
            if (Folder != null)
            {
                if (i.Folder == null) return desc ? 1 : -1;
            }
            else if (i.Folder != null)
            {
                return desc ? -1 : 1;
            }

            var i1 = this;
            var i2 = i;
            if (desc)
            {
                i1 = i;
                i2 = this;
            }

            switch (col)
            {
                default:
                case 0: //Name column
                    return i1.Name.CompareTo(i2.Name);
                case 1: //Type column
                    var ftc = i1.FileTypeText.CompareTo(i2.FileTypeText);
                    if (ftc == 0) return i1.Name.CompareTo(i2.Name); //same type, sort by name...
                    return ftc;
                case 2: //Size column
                    return i1.FileSize.CompareTo(i2.FileSize);
                case 3: //Attributes column
                    var ac = i1.Attributes.CompareTo(i2.Attributes);
                    if (ac == 0) return i1.Name.CompareTo(i2.Name); //same attributes, sort by name...
                    return ac;
                case 4: //path column
                    return i1.Path.CompareTo(i2.Path);
            }

            //return i1.Name.CompareTo(i2.Name);
        }

    }


    public class FileTypeInfo
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public int ImageIndex { get; set; }
        public FileTypeAction DefaultAction { get; set; }

        public FileTypeInfo(string extension, string name, int imageindex, FileTypeAction defaultAction)
        {
            Name = name;
            Extension = extension;
            ImageIndex = imageindex;
            DefaultAction = defaultAction;
        }
    }

    public enum FileTypeAction
    {
        ViewHex = 0,
        ViewText = 1,
        ViewXml = 2,
        ViewYtd = 3,
        ViewYmt = 4,
        ViewYmf = 5,
        ViewYmap = 6,
        ViewYtyp = 7,
        ViewJPso = 8,
        ViewModel = 9,
        ViewCut = 10,
        ViewAwc = 11,
        ViewGxt = 12,
        ViewRel = 13,
        ViewFxc = 14,
        ViewYwr = 15,
        ViewYvr = 16,
    }

}
