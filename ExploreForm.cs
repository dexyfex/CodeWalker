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
using System.Xml;
using WeifenLuo.WinFormsUI.Docking;

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
        private GameFileCache FileCache { get; set; } = GameFileCacheFactory.Create();
        private object FileCacheSyncRoot = new object();

        private bool EditMode = false;

        public ThemeBase Theme { get; private set; }


        public ExploreForm()
        {
            InitializeComponent();

            SetTheme(Settings.Default.ExplorerWindowTheme, false);

            ShowMainListViewPathColumn(false);
        }

        private void SetTheme(string themestr, bool changing = true)
        {
            //string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DockPanel.temp.config");
            //MainDockPanel.SaveAsXml(configFile);
            //CloseAllContents();

            foreach (ToolStripMenuItem menu in ViewThemeMenu.DropDownItems)
            {
                menu.Checked = false;
            }

            Theme = null;
            var version = VisualStudioToolStripExtender.VsVersion.Vs2015;

            switch (themestr)
            {
                default:
                case "Windows":
                    //Theme = new VS2005Theme();
                    ViewThemeWindowsMenu.Checked = true;
                    version = VisualStudioToolStripExtender.VsVersion.Unknown;
                    if (changing)
                    {
                        MessageBox.Show("Please reopen RPF Explorer to change back to Windows theme.");
                    }
                    break;
                case "Blue":
                    Theme = new VS2015BlueTheme();
                    ViewThemeBlueMenu.Checked = true;
                    break;
                case "Light":
                    Theme = new VS2015LightTheme();
                    ViewThemeLightMenu.Checked = true;
                    break;
                case "Dark":
                    Theme = new VS2015DarkTheme();
                    ViewThemeDarkMenu.Checked = true;
                    break;
            }

            if (changing)
            {
                Settings.Default.ExplorerWindowTheme = themestr;
                Settings.Default.Save();
            }


            //Theme.Extender.FloatWindowFactory = new ExplorerFloatWindowFactory();
            //MainDockPanel.Theme = Theme;

            if (Theme != null)
            {
                VSExtender.SetStyle(MainMenu, version, Theme);
                VSExtender.SetStyle(MainToolbar, version, Theme);
                VSExtender.SetStyle(MainStatusBar, version, Theme);

                FormTheme.SetTheme(this, Theme);

                MainSplitContainer.BackColor = Theme.ColorPalette.MainWindowActive.Background;
            }


            //if (File.Exists(configFile)) MainDockPanel.LoadFromXml(configFile, m_deserializeDockContent);
        }


        private void Init()
        {
            //called from ExploreForm_Load

            InitFileTypes();

            // This is probably not necessary now that the GTA folder is checked 
            // in the Program.cs when the game is initiated, but we will leave it 
            // here for now to make sure 
            if(!GTAFolder.UpdateGTAFolder(true))
            {
                Close();
                return;
            }


            Task.Run(() =>
            {
                try
                {
                    GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
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
                        bool fcItemsPending = FileCache.ContentThreadProc();

                        if (!fcItemsPending)
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
            InitFileType(".ycd", "Clip Dictionary", 18, FileTypeAction.ViewYcd);
            InitFileType(".ypt", "Particle Effect", 18, FileTypeAction.ViewModel);
            InitFileType(".ybn", "Static Collisions", 19, FileTypeAction.ViewModel);
            InitFileType(".ide", "Item Definitions", 20, FileTypeAction.ViewText);
            InitFileType(".ytyp", "Archetype Definitions", 20, FileTypeAction.ViewYtyp);
            InitFileType(".ymap", "Map Data", 21, FileTypeAction.ViewYmap);
            InitFileType(".ipl", "Item Placements", 21, FileTypeAction.ViewText);
            InitFileType(".awc", "Audio Wave Container", 22, FileTypeAction.ViewAwc);
            InitFileType(".rel", "Audio Data (REL)", 23, FileTypeAction.ViewRel);

            InitSubFileType(".dat", "cache_y.dat", "Cache File", 6, FileTypeAction.ViewCacheDat);
        }
        private void InitFileType(string ext, string name, int imgidx, FileTypeAction defaultAction = FileTypeAction.ViewHex)
        {
            var ft = new FileTypeInfo(ext, name, imgidx, defaultAction);
            FileTypes[ext] = ft;
        }
        private void InitSubFileType(string ext, string subext, string name, int imgidx, FileTypeAction defaultAction = FileTypeAction.ViewHex)
        {
            FileTypeInfo pti = null;
            if (FileTypes.TryGetValue(ext, out pti))
            {
                var ft = new FileTypeInfo(subext, name, imgidx, defaultAction);
                pti.AddSubType(ft);
            }
        }
        public FileTypeInfo GetFileType(string fn)
        {
            var fi = new FileInfo(fn);
            var ext = fi.Extension.ToLowerInvariant();
            if (!string.IsNullOrEmpty(ext))
            {
                FileTypeInfo ft;
                if (FileTypes.TryGetValue(ext, out ft))
                {
                    if (ft.SubTypes != null)
                    {
                        var fnl = fn.ToLowerInvariant();
                        foreach (var sft in ft.SubTypes)
                        {
                            if (fnl.EndsWith(sft.Extension))
                            {
                                return sft;
                            }
                        }
                    }
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


        public void Navigate(MainTreeFolder f)
        {
            if (!Ready) return;
            if (f == CurrentFolder) return; //already there!
            if (f.IsSearchResults)
            {
                AddMainTreeViewRoot(f); //add the current search result node

                TreeNode sr = FindTreeNode(f, null);
                if (sr != null)
                {
                    MainTreeView.SelectedNode = sr; //navigate to the new search results node
                }

                foreach (TreeNode tn in MainTreeView.Nodes)
                {
                    var tnf = tn.Tag as MainTreeFolder;
                    if ((tn != sr) && (tnf != null) && (tnf.IsSearchResults))
                    {
                        MainTreeView.Nodes.Remove(tn); //remove existing search result node
                        break;
                    }
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
            EnsureEditModeWarning();

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
            EditModeButton.Enabled = true;
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
                        canexportxml = canexportxml || CanExportXml(file);
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

            string fullPath = GTAFolder.GetCurrentGTAFolderWithTrailingSlash();

            string[] allpaths = Directory.GetFileSystemEntries(GTAFolder.CurrentGTAFolder, "*", SearchOption.AllDirectories);

            Dictionary<string, MainTreeFolder> nodes = new Dictionary<string, MainTreeFolder>();

            MainTreeFolder root = new MainTreeFolder();
            root.FullPath = GTAFolder.GetCurrentGTAFolderWithTrailingSlash();
            root.Path = "";
            root.Name = "GTA V";
            RootFolder = root;

            UpdateStatus("Scanning...");

            foreach (var path in allpaths)
            {
                var relpath = path.Replace(fullPath, "");
                var filepathl = path.ToLowerInvariant();

                var isFile = File.Exists(path); //could be a folder

                UpdateStatus("Scanning " + relpath + "...");

                MainTreeFolder parentnode = null, prevnode = null, node = null;
                var prevnodepath = "";
                var idx = isFile ? relpath.LastIndexOf('\\') : relpath.Length;
                while (idx > 0) //create the folder tree nodes and build up the hierarchy
                {
                    var parentpath = relpath.Substring(0, idx);
                    var parentidx = parentpath.LastIndexOf('\\');
                    var parentname = parentpath.Substring(parentidx + 1);
                    var exists = nodes.TryGetValue(parentpath, out node);
                    if (!exists)
                    {
                        node = CreateRootDirTreeFolder(parentname, parentpath, fullPath + parentpath);
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

                if (isFile)
                {
                    if (filepathl.EndsWith(".rpf")) //add RPF nodes
                    {
                        RpfFile rpf = new RpfFile(path, relpath);

                        rpf.ScanStructure(UpdateStatus, UpdateErrorLog);

                        node = CreateRpfTreeFolder(rpf, relpath, path);

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
                            parentnode.AddFile(path);
                        }
                        else
                        {
                            root.AddFile(path);
                        }
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
            var rootpath = GTAFolder.GetCurrentGTAFolderWithTrailingSlash();

            var fld = f.RpfFolder;
            if (fld != null)
            {
                if (fld.Directories != null)
                {
                    foreach (var dir in fld.Directories)
                    {
                        var dtnf = CreateRpfDirTreeFolder(dir, dir.Path, rootpath + dir.Path);
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
                        var ctnf = CreateRpfTreeFolder(child, child.Path, rootpath + child.Path);
                        f.AddChildToHierarchy(ctnf);
                        RecurseMainTreeViewRPF(ctnf, allRpfs);
                    }
                }

                //JenkIndex.Ensure(rpf.Name);
                if (rpf.AllEntries != null)
                {
                    foreach (RpfEntry entry in rpf.AllEntries)
                    {
                        if (string.IsNullOrEmpty(entry.NameLower)) continue;
                        var shortnamel = entry.GetShortNameLower();
                        JenkIndex.Ensure(shortnamel);
                        entry.ShortNameHash = JenkHash.GenHash(shortnamel);
                        entry.NameHash = JenkHash.GenHash(entry.NameLower);
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
                    EditModeButton.Enabled = false;
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
                    f.TreeNode = rn;
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

            f.TreeNode = tn;

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
        private void AddNewFolderTreeNode(MainTreeFolder f)
        {
            if (CurrentFolder == null) return;

            RecurseAddMainTreeViewNodes(f, CurrentFolder.TreeNode);

            CurrentFolder.AddChild(f);
            CurrentFolder.ListItems = null;

            RefreshMainListView();
        }
        private MainTreeFolder CreateRpfTreeFolder(RpfFile rpf, string relpath, string fullpath)
        {
            var node = new MainTreeFolder();
            node.RpfFile = rpf;
            node.RpfFolder = rpf.Root;
            node.Name = rpf.Name;
            node.Path = relpath;
            node.FullPath = fullpath;
            return node;
        }
        private MainTreeFolder CreateRpfDirTreeFolder(RpfDirectoryEntry dir, string relpath, string fullpath)
        {
            var node = new MainTreeFolder();
            node.RpfFolder = dir;
            node.Name = dir.Name;
            node.Path = relpath;
            node.FullPath = fullpath;
            return node;
        }
        private MainTreeFolder CreateRootDirTreeFolder(string name, string path, string fullpath)
        {
            var node = new MainTreeFolder();
            node.Name = name;
            node.Path = path;
            node.FullPath = fullpath;
            return node;
        }
        private void RenameTreeFolder(MainTreeFolder f, string newname)
        {
            if (f.Parent == null) return;
            f.Name = newname;
            f.Path = f.Parent.Path + "\\" + newname.ToLowerInvariant();
            f.FullPath = f.Parent.FullPath + "\\" + newname;
            if (f.TreeNode != null)
            {
                f.TreeNode.Text = newname;
            }
            if (f.Children != null)
            {
                foreach (var item in f.Children)
                {
                    RenameTreeFolder(item, item.Name);//just to make sure the all the paths are correct...
                }
            }
            if (f.ListItems != null)
            {
                foreach (var item in f.ListItems)
                {
                    RenameListItem(item, item.Name);
                }
            }
        }
        private void RenameListItem(MainListItem i, string newname)
        {
            if (i.Parent == null) return;
            i.Name = newname;
            i.Path = i.Parent.Path + "\\" + newname.ToLowerInvariant();
            i.FullPath = i.Parent.FullPath + "\\" + newname;

            if (i.Parent == CurrentFolder)
            {
                int idx = CurrentFiles.IndexOf(i);
                if (idx >= 0)
                {
                    MainListView.RedrawItems(idx, idx, false);
                }
            }
        }
        private void RemoveTreeFolder(MainTreeFolder f)
        {
            if (f.Parent == null) return;

            f.Parent.Children?.Remove(f);

            if (f.TreeNode != null)
            {
                f.TreeNode.Remove();
            }
        }
        private void RemoveListItem(MainListItem i)
        {
            if (i.Parent == null) return;

            MainListView.VirtualListSize = 0;

            i.Parent.ListItems?.Remove(i);

            if (i.Parent == CurrentFolder)
            {
                CurrentFiles.Remove(i);//should really be the same list as above, but just in case...
            }

            MainListView.VirtualListSize = CurrentFiles.Count;
        }
        private void EnsureImportedRpf(RpfFileEntry entry, RpfDirectoryEntry parentrpffldr)
        {
            if ((entry == null) || (parentrpffldr == null)) return;
            var newrpf = parentrpffldr.File?.FindChildArchive(entry);
            if (newrpf == null) return;

            //an RPF file was imported. add its structure to the UI!
            var rootpath = GTAFolder.GetCurrentGTAFolderWithTrailingSlash();
            var tnf = CreateRpfTreeFolder(newrpf, newrpf.Path, rootpath + newrpf.Path);
            CurrentFolder.AddChildToHierarchy(tnf);
            var pfolder = tnf.Parent;
            if (pfolder.Children != null)
            {
                //make sure any existing (replaced!) one is removed first!
                foreach (var child in pfolder.Children)
                {
                    if ((child != tnf) && (child.Path == tnf.Path))
                    {
                        pfolder.Children.Remove(child);
                        child.TreeNode.Remove();
                        break;
                    }
                }
            }
            RecurseMainTreeViewRPF(tnf, AllRpfs);
            RecurseAddMainTreeViewNodes(tnf, pfolder.TreeNode);
        }
        private void EnsureImportedFolder(RpfDirectoryEntry entry, RpfDirectoryEntry parentrpffldr)
        {
            if ((entry == null) || (parentrpffldr == null)) return;

            var rootpath = GTAFolder.GetCurrentGTAFolderWithTrailingSlash();
            var tnf = CreateRpfDirTreeFolder(entry, entry.Path, rootpath + entry.Path);
            CurrentFolder.AddChildToHierarchy(tnf);

            if (tnf.Parent.TreeNode != null)
            {
                RecurseAddMainTreeViewNodes(tnf, tnf.Parent.TreeNode);
            }

            foreach (var subdir in entry.Directories)
            {
                EnsureImportedFolder(subdir, entry);
            }
            foreach (var subfile in entry.Files)
            {
                EnsureImportedRpf(subfile, entry);
            }
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
            if (file.Folder != null)
            {
                var entry = file.Folder.RpfFile?.ParentFileEntry;
                if (entry != null)
                {
                    data = entry.File.ExtractFile(entry);//extract an RPF from another.
                }
                else if (!string.IsNullOrEmpty(file.FullPath) && (file.Folder.RpfFile != null))
                {
                    data = File.ReadAllBytes(file.FullPath); //load RPF file from filesystem
                }
            }
            else if (file.File != null) //load file from RPF
            {
                if (file.File.File != null) //need the reference to the RPF archive
                {
                    data = file.File.File.ExtractFile(file.File);
                }
                else
                { }
            }
            else if (!string.IsNullOrEmpty(file.FullPath))
            {
                data = File.ReadAllBytes(file.FullPath); //load file from filesystem
            }
            else
            { }
            return data;
        }
        private byte[] GetFileDataCompressResources(MainListItem file)
        {
            byte[] data = GetFileData(file);
            RpfResourceFileEntry rrfe = file.File as RpfResourceFileEntry;
            if (rrfe != null) //add resource header if this is a resource file.
            {
                data = ResourceBuilder.Compress(data); //not completely ideal to recompress it...
                data = ResourceBuilder.AddResourceHeader(rrfe, data);
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
                case FileTypeAction.ViewYcd:
                case FileTypeAction.ViewCacheDat:
                    return true;
                case FileTypeAction.ViewHex:
                default:
                    break;
            }
            return false;
        }

        private bool CanExportXml(MainListItem item)
        {
            if (item == null) return false;
            if (item.FileType == null) return false;
            switch (item.FileType.DefaultAction)
            {
                case FileTypeAction.ViewYmt:
                case FileTypeAction.ViewYmf:
                case FileTypeAction.ViewYmap:
                case FileTypeAction.ViewYtyp:
                case FileTypeAction.ViewJPso:
                case FileTypeAction.ViewCut:
                    return true;
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
                    case FileTypeAction.ViewYcd:
                        ViewYcd(name, path, data, item.File);
                        break;
                    case FileTypeAction.ViewCacheDat:
                        ViewCacheDat(name, path, data, item.File);
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
            var fe = Path.GetExtension(nl);
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
        private void ViewYcd(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ycd = RpfFile.GetFile<YcdFile>(e, data);
            YcdForm f = new YcdForm();
            f.Show();
            f.LoadYcd(ycd);
        }
        private void ViewCacheDat(string name, string path, byte[] data, RpfFileEntry e)
        {
            var cachedat = RpfFile.GetFile<CacheDatFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(cachedat);
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
            bool isarchive = false;
            bool isfilesys = false;
            bool issearch = CurrentFolder?.IsSearchResults ?? false;
            bool canview = false;
            bool canexportxml = false;
            bool canextract = false;
            bool canimport = EditMode && (CurrentFolder?.RpfFolder != null) && !issearch;
            bool cancreate = EditMode && !issearch;
            bool canedit = false;
            bool candefrag = false;

            if (item != null)
            {
                var entry = item.GetRpfEntry();
                isitem = true;
                isfilesys = (entry == null);
                isarchive = (item.Folder?.RpfFile != null);
                isfolder = (item.Folder != null);
                isfile = !isfolder;
                canview = CanViewFile(item);
                canexportxml = CanExportXml(item);
                canedit = EditMode && !issearch;
                canextract = isfile || (isarchive && !isfilesys);
                candefrag = isarchive && canedit;
            }


            ListContextViewMenu.Enabled = canview;
            ListContextViewHexMenu.Enabled = isfile;

            ListContextExportXmlMenu.Enabled = canexportxml;
            ListContextExtractRawMenu.Enabled = canextract;
            ListContextExtractUncompressedMenu.Enabled = isfile;

            ListContextNewMenu.Visible = cancreate;
            ListContextImportRawMenu.Visible = canimport;
            ListContextImportXmlMenu.Visible = canimport;
            ListContextImportSeparator.Visible = cancreate;

            ListContextCopyMenu.Enabled = isfile;
            ListContextCopyPathMenu.Enabled = isitem;

            ListContextOpenFileLocationMenu.Visible = issearch;
            ListContextOpenFileLocationSeparator.Visible = issearch;

            ListContextRenameMenu.Visible = canedit;
            ListContextReplaceMenu.Visible = canedit;
            ListContextDeleteMenu.Visible = canedit;
            ListContextEditSeparator.Visible = canedit;

            ListContextDefragmentMenu.Visible = candefrag;
            ListContextDefragmentSeparator.Visible = candefrag;

            ListContextMenu.Show(Cursor.Position);

        }




        private void EnableEditMode(bool enable)
        {
            if (EditMode == enable)
            {
                return;
            }

            if (enable)
            {
                if (MessageBox.Show(this, "While in edit mode, all changes are automatically saved.\nDo you want to continue?", "Warning - Entering edit mode", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }
            }

            EditMode = enable;
            EditModeButton.Checked = enable;
            MainListView.LabelEdit = enable;

            EnsureEditModeWarning();
        }

        private void EnsureEditModeWarning()
        {
            bool mods = CurrentFolder.Path.ToLowerInvariant().StartsWith("mods");
            bool srch = CurrentFolder?.IsSearchResults ?? false;
            bool show = EditMode && !mods && !srch;
            int gap = 3;
            int bot = MainListView.Bottom;

            EditModeBaseWarningPanel.Top = gap;
            EditModeModsWarningPanel.Top = gap;
            EditModeModsWarningPanel.Visible = false;

            MainListView.Top = show ? EditModeBaseWarningPanel.Bottom + gap : gap;
            MainListView.Height = bot - MainListView.Top;
        }




        private bool IsFilenameOk(string name)
        {
            foreach (var ic in Path.GetInvalidFileNameChars())
            {
                if (name.Contains(ic))
                {
                    return false;
                }
            }
            return true;
        }





        private bool EnsureRpfValidEncryption(RpfFile file = null)
        {
            if ((file == null) && (CurrentFolder.RpfFolder == null)) return false;

            var rpf = file ?? CurrentFolder.RpfFolder.File;

            if (rpf == null) return false;

            var confirm = new Func<RpfFile, bool>((f) => 
            {
                var msg = "Archive " + f.Name + " is currently set to " + f.Encryption.ToString() + " encryption.\nAre you sure you want to change this archive to OPEN encryption?\nLoading by the game will require OpenIV.asi.";
                return (MessageBox.Show(msg, "Change RPF encryption type", MessageBoxButtons.YesNo) == DialogResult.Yes);
            });

            return RpfFile.EnsureValidEncryption(rpf, confirm);
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
            if (MainListView.SelectedIndices.Count == 1)
            {
                var idx = MainListView.SelectedIndices[0];
                if ((idx < 0) || (idx >= CurrentFiles.Count)) return;
                var file = CurrentFiles[idx];
                if (file.Folder == null)
                {
                    if (CanExportXml(file))
                    {
                        byte[] data = GetFileData(file);
                        if (data == null)
                        {
                            MessageBox.Show("Unable to extract file: " + file.Path);
                            return;
                        }

                        string newfn;
                        string xml = MetaXml.GetXml(file.File, data, out newfn);
                        if (string.IsNullOrEmpty(xml))
                        {
                            MessageBox.Show("Unable to convert file to XML: " + file.Path);
                            return;
                        }

                        SaveFileDialog.FileName = newfn;
                        if (SaveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            string path = SaveFileDialog.FileName;
                            try
                            {
                                File.WriteAllText(path, xml);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error saving file " + path + ":\n" + ex.ToString());
                            }
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
                        if (!CanExportXml(file)) continue;

                        var data = GetFileData(file);
                        if (data == null)
                        {
                            errors.AppendLine("Unable to extract file: " + file.Path);
                            continue;
                        }

                        string newfn;
                        string xml = MetaXml.GetXml(file.File, data, out newfn);
                        if (string.IsNullOrEmpty(xml))
                        {
                            errors.AppendLine("Unable to convert file to XML: " + file.Path);
                            continue;
                        }

                        var path = folderpath + newfn;
                        try
                        {
                            File.WriteAllText(path, xml);
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
        private void ExtractRaw()
        {
            if (MainListView.SelectedIndices.Count == 1)
            {
                var idx = MainListView.SelectedIndices[0];
                if ((idx < 0) || (idx >= CurrentFiles.Count)) return;
                var file = CurrentFiles[idx];

                if ((file.Folder == null) || (file.Folder.RpfFile != null))
                {
                    byte[] data = GetFileDataCompressResources(file);
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
                    if ((file.Folder == null) || (file.Folder.RpfFile != null))
                    {
                        var path = folderpath + file.Name;
                        try
                        {
                            var data = GetFileDataCompressResources(file);
                            if (data == null)
                            {
                                errors.AppendLine("Unable to extract file: " + file.Path);
                                continue;
                            }

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
        private void ExtractUncompressed()
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
                if ((file.Folder == null) || (file.Folder.RpfFile != null))
                {
                    var path = folderpath + file.Name;
                    try
                    {
                        var data = GetFileDataCompressResources(file);
                        if (data == null)
                        {
                            errors.AppendLine("Unable to extract file: " + file.Path);
                            continue;
                        }

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
        private void NewFolder()
        {
            if (CurrentFolder == null) return;//shouldn't happen
            if (CurrentFolder?.IsSearchResults ?? false) return;

            string fname = Prompt.ShowDialog(this, "Enter a name for the new folder:", "Create folder", "folder");
            if (string.IsNullOrEmpty(fname))
            {
                return;//no name was provided.
            }
            if (!IsFilenameOk(fname)) return; //new name contains invalid char(s). don't do anything


            string cpath = (string.IsNullOrEmpty(CurrentFolder.Path) ? "" : CurrentFolder.Path + "\\");
            string relpath = cpath + fname.ToLowerInvariant();
            var rootpath = GTAFolder.GetCurrentGTAFolderWithTrailingSlash();
            string fullpath = rootpath + relpath;

            RpfDirectoryEntry newdir = null;
            MainTreeFolder node = null;

            try
            {
                if (CurrentFolder.RpfFolder != null)
                {
                    if (!EnsureRpfValidEncryption()) return;

                    //create new directory entry in the RPF.

                    newdir = RpfFile.CreateDirectory(CurrentFolder.RpfFolder, fname);

                    node = CreateRpfDirTreeFolder(newdir, relpath, fullpath);
                }
                else
                {
                    //create a folder in the filesystem.
                    if (Directory.Exists(fullpath))
                    {
                        throw new Exception("Folder " + fullpath + " already exists!");
                    }
                    Directory.CreateDirectory(fullpath);

                    node = CreateRootDirTreeFolder(fname, relpath, fullpath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating new folder: " + ex.Message, "Unable to create new folder");
                return;
            }

            if (node != null)
            {
                AddNewFolderTreeNode(node);
            }

        }
        private void NewRpfArchive()
        {
            if (CurrentFolder == null) return;//shouldn't happen
            if (CurrentFolder?.IsSearchResults ?? false) return;

            string fname = Prompt.ShowDialog(this, "Enter a name for the new archive:", "Create RPF7 archive", "new");
            if (string.IsNullOrEmpty(fname))
            {
                return;//no name was provided.
            }
            if (!IsFilenameOk(fname)) return; //new name contains invalid char(s). don't do anything

            if (!fname.ToLowerInvariant().EndsWith(".rpf"))
            {
                fname = fname + ".rpf";//make sure it ends with .rpf
            }
            string cpath = (string.IsNullOrEmpty(CurrentFolder.Path) ? "" : CurrentFolder.Path + "\\");
            string relpath = cpath + fname.ToLowerInvariant();


            RpfEncryption encryption = RpfEncryption.OPEN;//TODO: select encryption mode

            RpfFile newrpf = null;

            try
            {
                if (CurrentFolder.RpfFolder != null)
                {
                    if (!EnsureRpfValidEncryption()) return;

                    //adding a new RPF as a child of another
                    newrpf = RpfFile.CreateNew(CurrentFolder.RpfFolder, fname, encryption);
                }
                else
                {
                    //adding a new RPF in the filesystem
                    newrpf = RpfFile.CreateNew(GTAFolder.CurrentGTAFolder, relpath, encryption);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating archive: " + ex.Message, "Unable to create new archive");
                return;
            }


            if (newrpf != null)
            {
                var node = CreateRpfTreeFolder(newrpf, newrpf.Path, GTAFolder.GetCurrentGTAFolderWithTrailingSlash() + newrpf.Path);
                RecurseMainTreeViewRPF(node, AllRpfs);
                AddNewFolderTreeNode(node);
            }

        }
        private void ImportXml()
        {
            if (!EditMode) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;

            RpfDirectoryEntry parentrpffldr = CurrentFolder.RpfFolder;
            if (parentrpffldr == null)
            {
                MessageBox.Show("No parent RPF folder selected! This shouldn't happen. Refresh the view and try again.");
                return;
            }

            if (!EnsureRpfValidEncryption()) return;


            OpenFileDialog.Filter = "XML Files|*.xml";
            if (OpenFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;//canceled
            }

            var fpaths = OpenFileDialog.FileNames;
            foreach (var fpath in fpaths)
            {
                try
                {
                    if (!File.Exists(fpath))
                    {
                        continue;//this shouldn't happen...
                    }

                    var fi = new FileInfo(fpath);
                    var fname = fi.Name;
                    var fnamel = fname.ToLowerInvariant();

                    if (!fnamel.EndsWith(".xml"))
                    {
                        MessageBox.Show(fname + ": Not an XML file!", "Cannot import XML");
                        continue;
                    }
                    if (fnamel.EndsWith(".pso.xml"))
                    {
                        MessageBox.Show(fname + ": PSO XML import not yet supported.", "Cannot import XML");
                        continue;
                    }
                    if (fnamel.EndsWith(".rbf.xml"))
                    {
                        MessageBox.Show(fname + ": RBF XML import not yet supported.", "Cannot import XML");
                        continue;
                    }

                    fname = fname.Substring(0, fname.Length - 4);
                    fnamel = fnamel.Substring(0, fnamel.Length - 4);

                    var doc = new XmlDocument();
                    string text = File.ReadAllText(fpath);
                    if (!string.IsNullOrEmpty(text))
                    {
                        doc.LoadXml(text);
                    }

                    var meta = XmlMeta.GetMeta(doc);


                    if ((meta.DataBlocks?.Data == null) || (meta.DataBlocks.Count == 0))
                    {
                        MessageBox.Show(fname + ": Schema not supported.", "Cannot import XML");
                        continue;
                    }


                    byte[] data = ResourceBuilder.Build(meta, 2); //meta is RSC V:2


                    RpfFile.CreateFile(parentrpffldr, fname, data);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Unable to import file");
                }
            }

            CurrentFolder.ListItems = null;
            RefreshMainListView();

        }
        private void ImportRaw()
        {
            if (!EditMode) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;

            if (!EnsureRpfValidEncryption()) return;

            OpenFileDialog.Filter = string.Empty;
            if (OpenFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;//canceled
            }

            var fpaths = OpenFileDialog.FileNames;

            ImportRaw(fpaths, false);//already checked encryption before the file dialog...
        }
        private void ImportRaw(string[] fpaths, bool checkEncryption = true)
        {
            if (!EditMode) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;

            RpfDirectoryEntry parentrpffldr = CurrentFolder.RpfFolder;
            if (parentrpffldr == null)
            {
                MessageBox.Show("No parent RPF folder selected! This shouldn't happen. Refresh the view and try again.");
                return;
            }

            if (checkEncryption)
            {
                if (!EnsureRpfValidEncryption()) return;
            }

            var filelist = new List<string>();
            var dirdict = new Dictionary<string, RpfDirectoryEntry>();
            foreach (var fpath in fpaths)
            {
                try
                {
                    if (File.Exists(fpath))
                    {
                        filelist.Add(fpath);
                    }
                    else if (Directory.Exists(fpath)) //create imported directory structure in the RPF.
                    {
                        //create the first directory entry.
                        var fdi = new DirectoryInfo(fpath);
                        var direntry = RpfFile.CreateDirectory(parentrpffldr, fdi.Name);
                        dirdict[fpath] = direntry;
                        var dirpaths = Directory.GetFileSystemEntries(fpath, "*", SearchOption.AllDirectories);
                        var newfiles = new List<string>();
                        foreach (var dirpath in dirpaths)
                        {
                            if (File.Exists(dirpath))
                            {
                                newfiles.Add(dirpath);
                            }
                            else if (Directory.Exists(dirpath))
                            {
                                var cdi = new DirectoryInfo(dirpath);
                                RpfDirectoryEntry pdirentry;
                                if (!dirdict.TryGetValue(cdi.Parent.FullName, out pdirentry))
                                {
                                    pdirentry = direntry;//fallback, shouldn't get here
                                }
                                if (pdirentry != null)
                                {
                                    var cdirentry = RpfFile.CreateDirectory(pdirentry, cdi.Name);
                                    dirdict[dirpath] = cdirentry;
                                }
                            }
                        }
                        foreach (var newfile in newfiles)
                        {
                            var nfi = new FileInfo(newfile);
                            RpfDirectoryEntry ndirentry;
                            if (!dirdict.TryGetValue(nfi.DirectoryName, out ndirentry))
                            {
                                ndirentry = direntry;//fallback, shouldn't get here
                            }
                            filelist.Add(newfile);
                            dirdict[newfile] = ndirentry;
                        }

                        EnsureImportedFolder(direntry, parentrpffldr);
                    }
                    else
                    { } //nothing to see here!
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Unable to import folder");
                }
            }


            foreach (var fpath in filelist)
            {
                try
                {
                    if (!File.Exists(fpath))
                    {
                        continue;//this shouldn't happen...
                    }

                    var fi = new FileInfo(fpath);
                    var fname = fi.Name;

                    if (fi.Length > 0x3FFFFFFF)
                    {
                        MessageBox.Show("File " + fname + " is too big! Max 1GB supported.", "Unable to import file");
                        continue;
                    }

                    Cursor = Cursors.WaitCursor;

                    byte[] data = File.ReadAllBytes(fpath);


                    var rpffldr = parentrpffldr;
                    if (dirdict.ContainsKey(fpath))
                    {
                        rpffldr = dirdict[fpath];
                    }

                    var entry = RpfFile.CreateFile(rpffldr, fname, data);

                    EnsureImportedRpf(entry, rpffldr); //make sure structure is created if an RPF was imported

                    Cursor = Cursors.Default;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Unable to import file");
                }
            }

            CurrentFolder.ListItems = null;
            RefreshMainListView();
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
                Clipboard.SetText(CurrentFolder?.FullPath ?? GTAFolder.GetCurrentGTAFolderWithTrailingSlash());
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
            if (CurrentFolder?.IsSearchResults ?? false) return;
            if (MainListView.SelectedIndices.Count != 1) return;
            var idx = MainListView.SelectedIndices[0];
            if ((CurrentFiles != null) && (CurrentFiles.Count > idx))
            {
                var item = CurrentFiles[idx];
                string newname = Prompt.ShowDialog(this, "Enter the new name for this item:", "Rename item", item.Name);
                if (!string.IsNullOrEmpty(newname))
                {
                    RenameItem(item, newname);
                }
            }
        }
        private void RenameItem(MainListItem item, string newname)
        {
            if (!EditMode) return;
            if (item.Name == newname) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;
            if (!IsFilenameOk(newname)) return; //new name contains invalid char(s). don't do anything


            RpfFile file = item.Folder?.RpfFile;
            RpfEntry entry = item.GetRpfEntry();

            try
            {
                if (file != null)
                {
                    //updates all items in the RPF with the new path - no actual file changes made here
                    RpfFile.RenameArchive(file, newname);
                }
                if (entry != null)
                {
                    if (!EnsureRpfValidEncryption()) return;

                    //renaming an entry in an RPF
                    RpfFile.RenameEntry(entry, newname);
                }
                else
                {
                    //renaming a filesystem item...
                    var dirinfo = new DirectoryInfo(item.FullPath);
                    var newpath = Path.Combine(dirinfo.Parent.FullName, newname);
                    if (item.FullPath.ToLowerInvariant() == newpath.ToLowerInvariant())
                    {
                        return;//filesystem tends to be case-insensitive... paths are the same
                    }
                    if ((item.Folder != null) && (item.Folder.RpfFile == null))
                    {
                        //renaming a filesystem folder...
                        Directory.Move(item.FullPath, newpath);
                    }
                    else
                    {
                        //renaming a filesystem file...
                        File.Move(item.FullPath, newpath);
                    }
                }

                if (item.Folder != null)
                {
                    RenameTreeFolder(item.Folder, newname);
                }

                RenameListItem(item, newname);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error renaming " + item.Path + ": " + ex.Message, "Unable to rename item");
                return;
            }

        }
        private void ReplaceSelected()
        {
            if (!EditMode) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;
            if (MainListView.SelectedIndices.Count != 1) return;
            MessageBox.Show("ReplaceSelected TODO...");
            //delete the selected items, and replace with... choose

            //if (!EnsureRpfEncryptionType()) return;

        }
        private void DeleteSelected()
        {
            if (!EditMode) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;
            if (MainListView.SelectedIndices.Count <= 0) return;
            //if (MainListView.SelectedIndices.Count == 1) //is confirmation always really necessary?
            //{
            //    var item = CurrentFiles[MainListView.SelectedIndices[0]];
            //    if (MessageBox.Show("Are you sure you want to permantly delete " + item.Name + "?\nThis cannot be undone.", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //    {
            //        return;
            //    }
            //}
            //else
            //{
            //    if (MessageBox.Show("Are you sure you want to permantly delete " + MainListView.SelectedIndices.Count.ToString() + " items?\nThis cannot be undone.", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //    {
            //        return;
            //    }
            //}
            var delitems = new List<MainListItem>();
            foreach (int idx in MainListView.SelectedIndices)
            {
                if ((idx < 0) || (idx >= CurrentFiles.Count)) return;
                var f = CurrentFiles[idx];//this could change when deleting.. so need to use the temp list
                delitems.Add(f);
            }
            foreach (var f in delitems)
            {
                DeleteItem(f);
            }
        }
        private void DeleteItem(MainListItem item)
        {
            try
            {
                if (item.Folder?.RpfFile != null)
                {
                    //confirm deletion of RPF archives, just to be friendly.
                    if (MessageBox.Show("Are you sure you want to delete this archive?\n" + item.Path, "Confirm delete archive", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        return;
                    }
                }
                else if ((item.Folder?.GetItemCount() ?? 0) > 0)
                {
                    //confirm deletion of non-empty folders, just to be friendly.
                    if (MessageBox.Show("Are you sure you want to delete this folder and all its contents?\n" + item.Path, "Confirm delete folder", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        return;
                    }
                }

                var parent = item.Parent;
                if (parent.RpfFolder != null)
                {
                    //delete an item in an RPF.
                    if (!EnsureRpfValidEncryption()) return;

                    RpfEntry entry = item.GetRpfEntry();

                    RpfFile.DeleteEntry(entry);
                }
                else
                {
                    //delete an item in the filesystem.
                    if ((item.Folder != null) && (item.Folder.RpfFile == null))
                    {
                        Directory.Delete(item.FullPath);
                    }
                    else
                    {
                        File.Delete(item.FullPath);
                    }
                }


                if (item.Folder != null)
                {
                    RemoveTreeFolder(item.Folder);
                }

                RemoveListItem(item);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting " + item.Path + ": " + ex.Message, "Unable to delete " + item.Name);
                return;
            }
        }
        private void DefragmentSelected()
        {
            if (!EditMode) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;
            if (MainListView.SelectedIndices.Count != 1)
            {
                MessageBox.Show("Can only defragment one item at a time. Please have only one item selected.");
                return;
            }
            var idx = MainListView.SelectedIndices[0];
            if ((CurrentFiles == null) || (CurrentFiles.Count <= idx))
            {
                return;
            }

            var item = CurrentFiles[idx];
            var rpf = item.Folder?.RpfFile;
            if (rpf == null)
            {
                MessageBox.Show("Can only defragment RPF archives!");
                return;
            }

            Form form = new Form() {
                Width = 450,
                Height = 250,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Defragment RPF Archive - CodeWalker by dexyfex",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var addCtrl = new Func<Control, Control>(c => { form.Controls.Add(c); return c; });
            var addLabel = new Func<int, string, Control>((y, t) => {
                return addCtrl(new Label() { Left = 30, Top = y, Width = 370, Height = 20, Text = t });
            });
            var rpfNameLabel = addLabel(20, "Archive: " + rpf.Path);
            var curSizeLabel = addLabel(40, string.Empty);
            var newSizeLabel = addLabel(60, string.Empty);
            var redSizeLabel = addLabel(80, string.Empty);
            var statusLabel = addLabel(110, string.Empty);
            var progressBar = addCtrl(new ProgressBar() { Left = 30, Top = 130, Width = 370, Height = 20, Minimum = 0, Maximum = 1000, MarqueeAnimationSpeed = 50 }) as ProgressBar;
            var beginButton = addCtrl(new Button() { Text = "Begin Defragment", Left = 30, Top = 170, Width = 120 }) as Button;
            var closeButton = addCtrl(new Button() { Text = "Close", Left = 320, Top = 170, Width = 80 }) as Button;
            var inProgress = false;
            var updateProgress = new Action<string, float>((s, p) => { form.Invoke(new Action(() => {
                statusLabel.Text = s;
                progressBar.Value = Math.Max(0, Math.Min((int)(p * 1000), 1000));//p in range 0..1
            }));});
            var updateSizeLabels = new Action<bool>((init) => {
                var curSize = rpf.FileSize;
                var newSize = rpf.GetDefragmentedFileSize();
                var redSize = curSize - newSize;
                curSizeLabel.Text = "Archive current size: " + TextUtil.GetBytesReadable(curSize);
                newSizeLabel.Text = "Defragmented size: " + TextUtil.GetBytesReadable(newSize);
                redSizeLabel.Text = "Size reduction: " + TextUtil.GetBytesReadable(redSize);
                if (init) statusLabel.Text = (redSize <= 0) ? "Defragmentation not required." : "Ready to defragment.";
            });
            var enableUi = new Action<bool>(enable => { form.Invoke(new Action(() => {
                beginButton.Enabled = enable;
                closeButton.Enabled = enable;
            }));});
            var defragment = new Action(() => { Task.Run(() => {
                if (inProgress) return;
                if (!EnsureRpfValidEncryption(rpf)) return;
                inProgress = true;
                enableUi(false);
                RpfFile.Defragment(rpf, updateProgress);
                updateProgress("Defragment complete.", 1.0f);
                enableUi(true);
                form.Invoke(new Action(() => { updateSizeLabels(false); }));
                inProgress = false;
            });});
            updateSizeLabels(true);
            beginButton.Click += (sender, e) => { defragment(); };
            closeButton.Click += (sender, e) => { form.Close(); };
            form.FormClosing += (s, e) => { e.Cancel = inProgress; };
            form.ShowDialog(this);
            RefreshMainListView();
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

            foreach (var item in ViewMenu.DropDownItems)
            {
                var menu = item as ToolStripMenuItem;
                if (menu != null)
                {
                    menu.Checked = false;
                }
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





        private string GetDropFolder()
        {
            return Path.Combine(Path.GetTempPath(), "CodeWalkerDrop");
        }
        private string CreateDropFolder()
        {
            string drop = GetDropFolder();
            if (!Directory.Exists(drop))
            {
                Directory.CreateDirectory(drop);
            }
            string dir = Path.Combine(drop, Guid.NewGuid().ToString());
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }
        private void CleanupDropFolder()
        {
            string drop = GetDropFolder();
            if (Directory.Exists(drop))
            {
                try
                {
                    Directory.Delete(drop, true); //so broken :[
                }
                catch { } //not much we can do here...
            }
        }





        private void ExploreForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void ExploreForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CleanupDropFolder();
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

        private void MainListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if ((CurrentFiles != null) && (CurrentFiles.Count > e.Item) && (!string.IsNullOrEmpty(e.Label)))
            {
                RenameItem(CurrentFiles[e.Item], e.Label);
            }
        }

        private void MainListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (MainListView.SelectedIndices.Count <= 0) return;

            var dir = CreateDropFolder();
            var filenames = new List<string>();
            var errors = new List<string>();

            Cursor = Cursors.WaitCursor;

            var writeFile = new Action<MainListItem, string, bool>((file, outdir, addfilename) => 
            {
                if (file.FileSize > 0x6400000) //100MB
                {
                    errors.Add(file.Name + " is greater than 100MB, drag-drop for large files is disabled.");
                    return;
                }
                try
                {
                    var data = GetFileDataCompressResources(file);
                    var filename = Path.Combine(outdir, file.Name);
                    File.WriteAllBytes(filename, data);
                    if (addfilename)
                    {
                        filenames.Add(filename);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex.ToString());
                }
            });

            for (int i = 0; i < MainListView.SelectedIndices.Count; i++)
            {
                var idx = MainListView.SelectedIndices[i];
                if ((idx < 0) || (idx >= CurrentFiles.Count)) continue;
                var file = CurrentFiles[idx];
                if ((file.Folder == null) || (file.Folder.RpfFile != null))
                {
                    //item is a file (or RPF archive).
                    writeFile(file, dir, true);
                }
                else
                {
                    //item is a folder.
                    var parentpath = file.Parent.Path;
                    var folderstack = new Stack<MainTreeFolder>();
                    folderstack.Push(file.Folder);
                    while (folderstack.Count > 0)
                    {
                        var folder = folderstack.Pop();
                        var folderitems = folder.GetListItems();
                        var relpath = folder.Path.Replace(parentpath, "");
                        var abspath = dir + relpath;
                        if (!Directory.Exists(abspath))
                        {
                            Directory.CreateDirectory(abspath); //create the temp directory...
                        }
                        foreach (var item in folderitems)
                        {
                            if ((item.Folder == null) || (item.Folder.RpfFile != null))
                            {
                                writeFile(item, abspath, false);
                            }
                            else
                            {
                                folderstack.Push(item.Folder);
                            }
                        }
                    }
                    filenames.Add(dir + "\\" + file.Name);
                }
            }

            Cursor = Cursors.Default;

            if (filenames.Count > 0)
            {
                DataObject dataobj = new DataObject(DataFormats.FileDrop, filenames.ToArray());
                DoDragDrop(dataobj, DragDropEffects.Copy);
            }
            else
            {
                if (errors.Count > 0)
                {
                    MessageBox.Show("Errors encountered while dragging:\n" + string.Join("\n", errors.ToArray()));
                }
            }

        }

        private void MainListView_DragEnter(object sender, DragEventArgs e)
        {
            if (!EditMode) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if ((files != null) && (files.Length > 0))
                {
                    if (!files[0].StartsWith(GetDropFolder(), StringComparison.InvariantCultureIgnoreCase)) //don't dry to drop on ourselves...
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                }
            }
        }

        private void MainListView_DragDrop(object sender, DragEventArgs e)
        {
            if (!EditMode) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if ((files == null) || (files.Length <= 0)) return;
                if (files[0].StartsWith(GetDropFolder(), StringComparison.InvariantCultureIgnoreCase)) return; //don't dry to drop on ourselves...
                ImportRaw(files);
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

        private void EditModeButton_Click(object sender, EventArgs e)
        {
            EnableEditMode(!EditMode);
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
            var path = folder?.FullPath ?? GTAFolder.GetCurrentGTAFolderWithTrailingSlash();
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

        private void ListContextExtractUncompressedMenu_Click(object sender, EventArgs e)
        {
            ExtractUncompressed();
        }

        private void ListContextExtractAllMenu_Click(object sender, EventArgs e)
        {
            ExtractAll();
        }

        private void ListContextNewFolderMenu_Click(object sender, EventArgs e)
        {
            NewFolder();
        }

        private void ListContextNewRpfArchiveMenu_Click(object sender, EventArgs e)
        {
            NewRpfArchive();
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

        private void ListContextDefragmentMenu_Click(object sender, EventArgs e)
        {
            DefragmentSelected();
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

        private void ViewThemeWindowsMenu_Click(object sender, EventArgs e)
        {
            SetTheme("Windows");
        }

        private void ViewThemeBlueMenu_Click(object sender, EventArgs e)
        {
            SetTheme("Blue");
        }

        private void ViewThemeLightMenu_Click(object sender, EventArgs e)
        {
            SetTheme("Light");
        }

        private void ViewThemeDarkMenu_Click(object sender, EventArgs e)
        {
            SetTheme("Dark");
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

        private void ToolsBinSearchMenu_Click(object sender, EventArgs e)
        {
            BinarySearchForm f = new BinarySearchForm(FileCache);
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
        public TreeNode TreeNode { get; set; }
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
                    if (tc.Name.Equals(pname, StringComparison.InvariantCultureIgnoreCase))
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
                var rootpath = GTAFolder.GetCurrentGTAFolderWithTrailingSlash();

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

            var rootpath = GTAFolder.GetCurrentGTAFolderWithTrailingSlash();

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
                    Attributes += fld.RpfFile.Encryption.ToString() + " encryption";
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

        public RpfEntry GetRpfEntry()
        {
            RpfFile file = Folder?.RpfFile;
            RpfDirectoryEntry fldr = Folder?.RpfFolder;
            RpfEntry entry = File;
            if (entry == null)
            {
                if (file != null)
                {
                    //for an RPF file, get its entry in the parent (if any).
                    entry = file.ParentFileEntry;
                }
                else if (fldr != null)
                {
                    //RPF folders are referenced in the item.Folder
                    entry = fldr;
                }
            }
            return entry;
        }

    }


    public class FileTypeInfo
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public int ImageIndex { get; set; }
        public FileTypeAction DefaultAction { get; set; }
        public List<FileTypeInfo> SubTypes { get; set; }

        public FileTypeInfo(string extension, string name, int imageindex, FileTypeAction defaultAction)
        {
            Name = name;
            Extension = extension;
            ImageIndex = imageindex;
            DefaultAction = defaultAction;
        }

        public void AddSubType(FileTypeInfo t)
        {
            if (SubTypes == null) SubTypes = new List<FileTypeInfo>();
            SubTypes.Add(t);
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
        ViewYcd = 17,
        ViewCacheDat = 18,
    }









}
