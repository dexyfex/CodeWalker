using CodeWalker.Forms;
using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.Tools;
using CodeWalker.Utils;
using CodeWalker.World;
using CodeWalker.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using System.Runtime.CompilerServices;
using static CodeWalker.GameFiles.GameFileCache;
using System.Diagnostics.CodeAnalysis;

namespace CodeWalker
{
    public partial class ExploreForm : Form
    {
        private volatile bool Ready = false;
        private bool IsInited = false;

        public static ExploreForm? Instance;

        private static Dictionary<string, FileTypeInfo>? FileTypes;
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        public MainTreeFolder? RootFolder;
        public List<MainTreeFolder> ExtraRootFolders = new List<MainTreeFolder>();
        public MainTreeFolder? CurrentFolder;
        public List<MainListItem>? CurrentFiles;
        private bool FirstRefreshed = false;
        private List<MainListItem> CopiedFiles = new List<MainListItem>();

        private Stack<MainTreeFolder> BackSteps = new Stack<MainTreeFolder>();
        private Stack<MainTreeFolder> ForwardSteps = new Stack<MainTreeFolder>();
        private bool HistoryNavigating = false;

        private int SortColumnIndex = 0;
        private SortOrder SortDirection = SortOrder.Ascending;
        private int PreviousPathColumnWidth = 0;

        public volatile bool Searching = false;
        private MainTreeFolder SearchResults;

        private List<RpfFile>? AllRpfs { get; set; }
        private static GameFileCache FileCache => GameFileCacheFactory.Instance;
        private readonly object FileCacheSyncRoot = new object();

        public bool EditMode { get; private set; } = false;

        public ThemeBase? Theme { get; private set; }

        public CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public static event Action? ForceTreeRefresh;

        static ExploreForm()
        {
            InitFileTypes();
        }

        public ExploreForm()
        {
            ETWEvents.Log.CreatingForm(nameof(ExploreForm));
            Instance = this;
            SuspendLayout();
            InitializeComponent();
            ResumeLayout();

            SetTheme(Settings.Default.ExplorerWindowTheme, false);

            ShowMainListViewPathColumn(false);

            LoadSettings();

            UpdateStatus += UpdateStatus_EventHandler;
            ErrorLog += UpdateErrorLog;
            ForceTreeRefresh += RefreshMainTreeView_EventHandler;
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

        private void LoadSettings()
        {
            var s = Settings.Default;

            OptionsStartInEditModeMenu.Checked = s.RPFExplorerStartInEditMode;
            OptionsStartInFolderValueMenu.Text = string.IsNullOrEmpty(s.RPFExplorerStartFolder) ? "(Default)" : s.RPFExplorerStartFolder;

            var folders = s.RPFExplorerExtraFolders?.Split('\n');
            if (folders != null)
            {
                foreach (var folder in folders)
                {
                    var folderPath = folder?.Trim();
                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        var root = new MainTreeFolder
                        {
                            FullPath = folderPath,
                            Path = folderPath,
                            Name = Path.GetFileName(Path.GetDirectoryName(folderPath)) ?? folderPath,
                            IsExtraFolder = true
                        };
                        ExtraRootFolders.Add(root);
                    }
                }
            }
        }
        private void SaveSettings()
        {
            var extrafolders = new StringBuilder();
            foreach (var folder in ExtraRootFolders)
            {
                extrafolders.AppendLine(folder.FullPath);
            }
            Settings.Default.RPFExplorerExtraFolders = extrafolders.ToString();

            Settings.Default.Save();
        }

        [MemberNotNull(nameof(FileCache))]
        private void Init()
        {
            ETWEvents.Log.LoadingForm(nameof(ExploreForm));
            //called from ExploreForm_Load

            // This is probably not necessary now that the GTA folder is checked 
            // in the Program.cs when the game is initiated, but we will leave it 
            // here for now to make sure 
            if (!GTAFolder.UpdateGTAFolder(true))
            {
                Close();
                throw new Exception("No GTA Folder found!");
            }

            new Task(async () =>
            {
                try
                {
                    try
                    {
                        GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
                    }
                    catch
                    {
                        UpdateStatus?.Invoke("Unable to load gta5.exe!");
                        return;
                    }

                    await RefreshMainTreeView();

                    UpdateStatus?.Invoke("Scan complete.");

                    await InitFileCache();


                    while (!IsDisposed && !CancellationTokenSource.IsCancellationRequested) //run the file cache content thread until the form exits.
                    {
                        if (FileCache.IsInited)
                        {
                            FileCache.BeginFrame();

                            bool fcItemsPending = FileCache.ContentThreadProc();

                            if (!fcItemsPending)
                            {
                                await Task.Delay(10);
                            }
                        }
                        else
                        {
                            await Task.Delay(20);
                        }
                    }
                } catch(Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, CancellationTokenSource.Token, TaskCreationOptions.LongRunning).Start(TaskScheduler.Default);
        }

        private ValueTask InitFileCache()
        {
            if (FileCache.IsInited)
            {
                return new ValueTask();
            }
            return new ValueTask(Task.Run(() =>
            {
                lock (FileCacheSyncRoot)
                {
                    if (!FileCache.IsInited)
                    {
                        try
                        {
                            UpdateStatus?.Invoke("Loading file cache...");
                            var allRpfs = AllRpfs;

                            FileCache.Init(updateStatus: null, ErrorLog, allRpfs); //inits main dicts and archetypes only...

                            UpdateStatus?.Invoke("Loading materials...");
                            BoundsMaterialTypes.Init(FileCache);

                            UpdateStatus?.Invoke("Loading scenario types...");
                            Scenarios.EnsureScenarioTypes(FileCache);

                            UpdateStatus?.Invoke("File cache loaded.");
                        } catch(Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), ex.Message);
                        }

                    }
                }
            }, CancellationTokenSource.Token));
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

        [MemberNotNull(nameof(FileTypes))]
        public static void InitFileTypes()
        {
            FileTypes ??= new Dictionary<string, FileTypeInfo>(StringComparer.OrdinalIgnoreCase);
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
            InitFileType(".ugc", "User-Generated Content", 5, FileTypeAction.ViewText);
            InitFileType(".xml", "XML File", 6, FileTypeAction.ViewXml);
            InitFileType(".meta", "Metadata (XML)", 6, FileTypeAction.ViewXml);
            InitFileType(".ymt", "Metadata (Binary)", 6, FileTypeAction.ViewYmt, true);
            InitFileType(".pso", "Metadata (PSO)", 6, FileTypeAction.ViewJPso, true);
            InitFileType(".gfx", "Scaleform Flash", 7);
            InitFileType(".ynd", "Path Nodes", 8, FileTypeAction.ViewYnd, true);
            InitFileType(".ynv", "Nav Mesh", 9, FileTypeAction.ViewModel, true);
            InitFileType(".yvr", "Vehicle Record", 9, FileTypeAction.ViewYvr, true);
            InitFileType(".ywr", "Waypoint Record", 9, FileTypeAction.ViewYwr, true);
            InitFileType(".fxc", "Compiled Shaders", 9, FileTypeAction.ViewFxc, true);
            InitFileType(".yed", "Expression Dictionary", 9, FileTypeAction.ViewYed, true);
            InitFileType(".yld", "Cloth Dictionary", 9, FileTypeAction.ViewYld, true);
            InitFileType(".yfd", "Frame Filter Dictionary", 9, FileTypeAction.ViewYfd, true);
            InitFileType(".asi", "ASI Plugin", 9);
            InitFileType(".dll", "Dynamic Link Library", 9);
            InitFileType(".exe", "Executable", 10);
            InitFileType(".yft", "Fragment", 11, FileTypeAction.ViewModel, true);
            InitFileType(".ydr", "Drawable", 11, FileTypeAction.ViewModel, true);
            InitFileType(".ydd", "Drawable Dictionary", 12, FileTypeAction.ViewModel, true);
            InitFileType(".cut", "Cutscene", 12, FileTypeAction.ViewCut, true);
            InitFileType(".ysc", "Script", 13);
            InitFileType(".ymf", "Manifest", 14, FileTypeAction.ViewYmf, true);
            InitFileType(".bik", "Bink Video", 15);
            InitFileType(".jpg", "JPEG Image", 16);
            InitFileType(".jpeg", "JPEG Image", 16);
            InitFileType(".gif", "GIF Image", 16);
            InitFileType(".png", "Portable Network Graphics", 16);
            InitFileType(".dds", "DirectDraw Surface", 16);
            InitFileType(".ytd", "Texture Dictionary", 16, FileTypeAction.ViewYtd, true);
            InitFileType(".mrf", "Move Network File", 18, FileTypeAction.ViewMrf, true);
            InitFileType(".ycd", "Clip Dictionary", 18, FileTypeAction.ViewYcd, true);
            InitFileType(".ypt", "Particle Effect", 18, FileTypeAction.ViewModel, true);
            InitFileType(".ybn", "Static Collisions", 19, FileTypeAction.ViewModel, true);
            InitFileType(".ide", "Item Definitions", 20, FileTypeAction.ViewText);
            InitFileType(".ytyp", "Archetype Definitions", 20, FileTypeAction.ViewYtyp, true);
            InitFileType(".ymap", "Map Data", 21, FileTypeAction.ViewYmap, true);
            InitFileType(".ipl", "Item Placements", 21, FileTypeAction.ViewText);
            InitFileType(".awc", "Audio Wave Container", 22, FileTypeAction.ViewAwc, true);
            InitFileType(".rel", "Audio Data (REL)", 23, FileTypeAction.ViewRel, true);
            InitFileType(".nametable", "Name Table", 5, FileTypeAction.ViewNametable);
            InitFileType(".ypdb", "Pose Matcher Database", 9, FileTypeAction.ViewYpdb, true);

            InitSubFileType(".dat", "cache_y.dat", "Cache File", 6, FileTypeAction.ViewCacheDat, true);
            InitSubFileType(".dat", "heightmap.dat", "Heightmap", 6, FileTypeAction.ViewHeightmap, true);
            InitSubFileType(".dat", "heightmapheistisland.dat", "Heightmap", 6, FileTypeAction.ViewHeightmap, true);
            InitSubFileType(".dat", "distantlights.dat", "Distant Lights", 6, FileTypeAction.ViewDistantLights);
            InitSubFileType(".dat", "distantlights_hd.dat", "Distant Lights", 6, FileTypeAction.ViewDistantLights);
        }

        [MemberNotNull(nameof(FileTypes))]
        private static void EnsureFileTypesInitialized()
        {
            if (FileTypes is null)
            {
                throw new InvalidOperationException($"FileTypes are not initialized yet, please ensure {nameof(InitFileTypes)} is called before using FileTypes");
            }
        }

        private static void InitFileType(string ext, string name, int imgidx, FileTypeAction defaultAction = FileTypeAction.ViewHex, bool xmlConvertible = false)
        {
            EnsureFileTypesInitialized();
            FileTypes ??= new Dictionary<string, FileTypeInfo>(StringComparer.OrdinalIgnoreCase);
            var ft = new FileTypeInfo(ext, name, imgidx, defaultAction, xmlConvertible);
            FileTypes[ext] = ft;
        }

        private static void InitSubFileType(string ext, string subext, string name, int imgidx, FileTypeAction defaultAction = FileTypeAction.ViewHex, bool xmlConvertible = false)
        {
            EnsureFileTypesInitialized();
            if (FileTypes.TryGetValue(ext, out var pti))
            {
                var ft = new FileTypeInfo(subext, name, imgidx, defaultAction, xmlConvertible);
                pti.AddSubType(ft);
            }
        }
        public static FileTypeInfo GetFileType(string fn)
        {
            EnsureFileTypesInitialized();
            if (fn.IndexOfAny(InvalidFileNameChars) != -1)
            {
                return FileTypes[""];
            }

            var ext = Path.GetExtension(fn);
            if (!string.IsNullOrEmpty(ext))
            {
                FileTypeInfo ft;
                if (FileTypes.TryGetValue(ext, out ft))
                {
                    if (ft.SubTypes != null)
                    {
                        foreach (var sft in ft.SubTypes)
                        {
                            if (fn.EndsWith(sft.Extension, StringComparison.OrdinalIgnoreCase))
                            {
                                return sft;
                            }
                        }
                    }
                    return ft;
                }
                else
                {
                    ft = new FileTypeInfo(ext, ext[1..].ToUpperInvariant() + " File", 4, FileTypeAction.ViewHex, false);
                    FileTypes[ft.Extension] = ft; //save it for later!
                    return ft;
                }
            }
            else
            {
                return FileTypes[""];
            }
        }

        public event Action<string> UpdateStatus;
        public event Action<string> ErrorLog;

        public void InvokeUpdateStatus(string msg)
        {
            UpdateStatus?.Invoke(msg);
        }

        public void UpdateStatus_EventHandler(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateStatus_EventHandler(text); }));
                }
                else
                {
                    StatusLabel.Text = text;
                }
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);
            }
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
                    Debug.WriteLine(text);
                    Console.WriteLine(text);
                    StatusLabel.Text = text;
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                Console.WriteLine(ex);
            }
        }


        public void Navigate(MainTreeFolder? f)
        {
            if (!Ready) return;
            if (f == CurrentFolder) return; //already there!
            ArgumentNullException.ThrowIfNull(f);
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
                    if ((tn != sr) && (tn.Tag is MainTreeFolder tnf) && (tnf.IsSearchResults))
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
            TreeNode? n = null;
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
            var pathl = path.Replace('/', '\\');
            if ((CurrentFolder != null) && (CurrentFolder.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase)))
                return; //already there
            var hierarchy = pathl.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            TreeNode? n = MainTreeView.Nodes[0];// FindTreeNode("gta v", null);
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

        public void GoUp(MainTreeFolder? toFolder = null)
        {
            var fld = (toFolder == null) ? CurrentFolder?.Parent : toFolder;
            if (fld == null) return;
            Navigate(fld);
        }
        public void GoBack(MainTreeFolder? toFolder = null)
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
        public void GoForward(MainTreeFolder? toFolder = null)
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
            bool issearch = CurrentFolder?.IsSearchResults ?? false;
            bool canview = false;
            bool canedit = false;
            bool canexportxml = false;
            bool canimport = EditMode && !issearch;// && (CurrentFolder?.RpfFolder != null);
            bool canpaste = EditMode && (CopiedFiles.Count > 0);

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
                        canedit = EditMode && !issearch;
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
            EditImportFbxMenu.Visible = canimport;
            EditImportXmlMenu.Visible = canimport;
            EditImportMenuSeparator.Visible = canimport;

            EditCopyMenu.Enabled = isfile;
            EditCopyPathMenu.Enabled = isitem;
            EditPasteMenu.Enabled = canpaste;
            EditPasteMenu.Visible = EditMode;

            EditRenameMenu.Visible = canedit;
            EditDeleteMenu.Visible = canedit;
            EditEditModeMenuSeparator.Visible = canedit;

        }

        private TreeNode? FindTreeNode(MainTreeFolder f, TreeNode? parent)
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
        private TreeNode? FindTreeNode(string text, TreeNode? parent)
        {
            var tnc = (parent != null) ? parent.Nodes : MainTreeView.Nodes;
            foreach (TreeNode node in tnc)
            {
                if (node.Text.Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    return node;
                }
            }
            return null;
        }


        [MemberNotNull(nameof(RootFolder))]
        [MemberNotNull(nameof(AllRpfs))]
        private async Task RefreshMainTreeView()
        {
            using var _ = new DisposableTimer("RefreshMainTreeView");
            Ready = false;
            AllRpfs = null;

            BeginInvoke(() => MainTreeView.BeginUpdate);

            UpdateStatus?.Invoke("Scanning...");


            var root = new MainTreeFolder
            {
                FullPath = GTAFolder.GetCurrentGTAFolderWithTrailingSlash(),
                Path = "",
                Name = "GTA V"
            };
            RootFolder = root;

            var tasks = new List<Task<TreeNode>>
            {
                Task.Run(() => RefreshMainTreeViewRoot(root, true))
            };


            var remFolders = new List<MainTreeFolder>();

            foreach (var extraroot in ExtraRootFolders)
            {
                extraroot.Clear();

                if (Directory.Exists(extraroot.FullPath))
                {
                    try
                    {
                        tasks.Add(Task.Run(() => RefreshMainTreeViewRoot(extraroot)));
                    }
                    catch(Exception ex)
                    {
                        remFolders.Add(extraroot);
                    }
                    
                }
                else
                {
                    remFolders.Add(extraroot);
                }
            }

            foreach (var remFolder in remFolders)
            {
                ExtraRootFolders.Remove(remFolder);
            }

            var nodes = await Task.WhenAll(tasks);

            BeginInvoke(() =>
            {
                SuspendLayout();
                MainTreeView.BeginUpdate();
                ClearMainTreeView();

                MainTreeView.Nodes.AddRange(nodes);

                Ready = true;

                MainTreeView.EndUpdate();
                ResumeLayout();
                MainTreeViewRefreshComplete();
            });

        }

        private TreeNode RefreshMainTreeViewRoot(MainTreeFolder f, bool gameRoot = false)
        {
            var allRpfs = new ConcurrentBag<RpfFile>();
            var fullPath = f.FullPath;
            var subPath = f.Path;
            FileSystemInfo[] allpaths;

            using (new DisposableTimer("RefreshMainTreeViewRoot -> EnumerateFileSystemInfos"))
            {
                allpaths = new DirectoryInfo(fullPath).EnumerateFileSystemInfos("*", SearchOption.AllDirectories).ToArray();
            }

            using var _ = new DisposableTimer("RefreshMainTreeViewRoot");

            //var allpaths = Directory.EnumerateFileSystemEntries(fullPath, "*", SearchOption.AllDirectories);
            //var allDirs = new HashSet<string>(Directory.EnumerateDirectories(fullPath, "*", SearchOption.AllDirectories));
            //var allfiles = new HashSet<string>(Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories));
            var nodes = new ConcurrentDictionary<string, MainTreeFolder>(4, allpaths.Length, StringComparer.OrdinalIgnoreCase);

            var partitioner = Partitioner.Create(allpaths, EnumerablePartitionerOptions.NoBuffering);

            Parallel.ForEach(partitioner, new ParallelOptions { MaxDegreeOfParallelism = 4, TaskScheduler = TaskScheduler.Default }, (fileEntryInfo) =>
            {
                try
                {
                    var path = fileEntryInfo.FullName;
                    var relpath = path.Substring(fullPath.Length);

                    var isFile = fileEntryInfo is FileInfo;
                    //var isFile = !path.I.Contains(path); //could be a folder

                    //UpdateStatus?.Invoke("Scanning " + relpath + "...");

                    MainTreeFolder parentnode = null, prevnode = null, node = null;
                    var prevnodepath = "";
                    var idx = isFile ? relpath.LastIndexOf('\\') : relpath.Length;
                    while (idx > 0) //create the folder tree nodes and build up the hierarchy
                    {
                        var parentpath = relpath.Substring(0, idx);
                        var parentidx = parentpath.LastIndexOf('\\');
                        var parentname = parentpath.Substring(parentidx + 1);
                        var exists = true;
                        MainTreeFolder _node = null;
                        node = nodes.GetOrAdd(parentpath, (key) =>
                        {
                            //Console.WriteLine($"Creating treenode for {parentpath}");
                            var node = CreateRootDirTreeFolder(parentname, subPath + parentpath, fullPath + parentpath);
                            _node = node;
                            exists = false;
                            return node;
                        });
                        if (_node != null && node != _node)
                        {
                            exists = true;
                        }
                        parentnode ??= node;
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
                            lock (f)
                            {
                                f.AddChild(node);
                            }

                        }
                    }

                    if (fileEntryInfo is FileInfo fileInfo)
                    {
                        if (path.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase)) //add RPF nodes
                        {
                            RpfFile rpf = new RpfFile(path, relpath);

                            rpf.ScanStructure(updateStatus: null, ErrorLog);

                            if (rpf.LastException != null) //incase of corrupted rpf (or renamed NG encrypted RPF)
                            {
                                Console.WriteLine(rpf.LastException);
                                return;
                            }

                            node = CreateRpfTreeFolder(rpf, relpath, path);

                            RecurseMainTreeViewRPF(node, allRpfs);

                            if (parentnode != null)
                            {
                                parentnode.AddChild(node);
                            }
                            else
                            {
                                f.AddChild(node);
                            }
                        }
                        else
                        {
                            JenkIndex.EnsureLower(Path.GetFileNameWithoutExtension(path));
                            if (parentnode != null)
                            {
                                parentnode.AddFile(path);
                            }
                            else
                            {
                                f.AddFile(path);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex);
                    ErrorLog?.Invoke(ex.ToString());
                }
            });

            var rootNode = CreateTreeNode(f);

            if (f.Children != null)
            {
                f.Children.Sort((n1, n2) => StringComparer.OrdinalIgnoreCase.Compare(n1.Name, n2.Name));

                foreach (var node in f.Children)
                {
                    AddMainTreeViewNode(node);
                }
            }

            if (gameRoot)
            {
                AllRpfs = allRpfs.ToList();
            }

            return rootNode;
        }
        private void RecurseMainTreeViewRPF(MainTreeFolder f, ConcurrentBag<RpfFile> allRpfs)
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
                        if (string.IsNullOrEmpty(entry.Name)) continue;
                        var shortName = entry.ShortName;
                        JenkIndex.EnsureLower(shortName);
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

        private TreeNode CreateTreeNode(MainTreeFolder f)
        {
            int imgIndex = 1; //FOLDER imageIndex
            if (string.IsNullOrEmpty(f.Path)) imgIndex = 0; //ROOT imageIndex
            var rn = new TreeNode(f.Name, imgIndex, imgIndex);
            rn.Name = f.Path;
            rn.ToolTipText = f.FullPath;
            rn.Tag = f;

            f.TreeNode = rn;

            return rn;
        }

        private void AddMainTreeViewRoot(MainTreeFolder f)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { AddMainTreeViewRoot(f); }));
                }
                else
                {
                    var node = CreateTreeNode(f);
                    MainTreeView.Nodes.Add(node);
                }
            }
            catch { }
        }
        private void AddMainTreeViewNode(MainTreeFolder f)
        {
            try
            {
                //if (InvokeRequired)
                //{
                //    Invoke(new Action(() => { AddMainTreeViewNode(f); }));
                //}
                //else
                //{
                    string n = f.Name;
                    var root = f.Parent?.TreeNode;

                    RecurseAddMainTreeViewNodes(f, root);

                    root.Expand();
                //}
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
                f.Children.Sort((n1, n2) => StringComparer.OrdinalIgnoreCase.Compare(n1.Name, n2.Name));

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
                    if (!FirstRefreshed && !string.IsNullOrEmpty(Settings.Default.RPFExplorerStartFolder))
                    {
                        Navigate(Settings.Default.RPFExplorerStartFolder);
                    }
                    else if (CurrentFolder != null)
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


                    if (!FirstRefreshed && Settings.Default.RPFExplorerStartInEditMode)
                    {
                        EnableEditMode(true, false);
                    }
                    FirstRefreshed = true;
                }
            }
            catch { }
        }
        private void AddNewFolderTreeNode(MainTreeFolder f)
        {
            if (CurrentFolder == null) return;

            RecurseAddMainTreeViewNodes(f, CurrentFolder.TreeNode);

            CurrentFolder.AddChild(f);

            RefreshMainListView();
        }
        private MainTreeFolder CreateRpfTreeFolder(RpfFile rpf, string relpath, string fullpath)
        {
            var node = new MainTreeFolder
            {
                RpfFile = rpf,
                RpfFolder = rpf.Root,
                Name = rpf.Name,
                Path = relpath,
                FullPath = fullpath,
            };
            return node;
        }
        private MainTreeFolder CreateRpfDirTreeFolder(RpfDirectoryEntry dir, string relpath, string fullpath)
        {
            var node = new MainTreeFolder
            {
                RpfFolder = dir,
                Name = dir.Name,
                Path = relpath,
                FullPath = fullpath,
            };
            return node;
        }
        private MainTreeFolder CreateRootDirTreeFolder(string name, string path, string fullpath)
        {
            var node = new MainTreeFolder
            {
                Name = name,
                Path = path,
                FullPath = fullpath,
            };
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

            f.Parent.RemoveChild(f);

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
                        pfolder.RemoveChild(child);
                        child.TreeNode.Remove();
                        break;
                    }
                }
            }
            RecurseMainTreeViewRPF(tnf, new ConcurrentBag<RpfFile>(AllRpfs));
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

        private void RefreshMainTreeView_EventHandler()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => { RefreshMainListView(); }));
            }
            else
            {
                RefreshMainListView();
            }
        }

        public static void RefreshMainListViewInvoke()
        {
            ForceTreeRefresh?.Invoke();
        }


        private void RefreshMainListView()
        {
            MainListView.VirtualListSize = 0;
            if (CurrentFolder != null)
            {
                if (!CurrentFolder.IsSearchResults)
                {
                    CurrentFolder.ListItems = null; //makes sure to rebuild the current files list
                }
                CurrentFiles = CurrentFolder.GetListItems();

                //foreach (var file in CurrentFiles) //cache all the data for use by the list view.
                //{
                //    file.CacheDetails();
                //}

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

            SearchResults = new MainTreeFolder
            {
                Name = "Search Results: " + text,
                Path = "Search Results: " + text,
                IsSearchResults = true,
                SearchTerm = text,
            };

            Navigate(SearchResults);

            SortDirection = SortOrder.None;
            MainListView.SetSortIcon(SortColumnIndex, SortDirection);
            MainListView.VirtualListSize = 0;

            CurrentFiles?.Clear();

            UpdateStatus?.Invoke("Searching...");

            var term = text;

            //Task.Run(() =>
            //{
            Searching = true;

            Cursor = Cursors.WaitCursor;

            var resultcount = RootFolder?.Search(term, this) ?? 0;

            foreach(var extraFolder in ExtraRootFolders)
            {
                resultcount += extraFolder.Search(term, this);
            }

            if (Searching)
            {
                Searching = false;
                UpdateStatus?.Invoke("Search complete. " + resultcount.ToString() + " items found.");
            }
            else
            {
                UpdateStatus?.Invoke("Search aborted. " + resultcount.ToString() + " items found.");
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


        public void AddSearchResult(MainListItem? item)
        {
            if (SearchResults == null) return;
            if (SearchResults.ListItems != CurrentFiles) return;

            if (item != null)
            {
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
                case FileTypeAction.ViewYnd:
                case FileTypeAction.ViewCacheDat:
                case FileTypeAction.ViewYed:
                case FileTypeAction.ViewYld:
                case FileTypeAction.ViewYfd:
                case FileTypeAction.ViewHeightmap:
                case FileTypeAction.ViewMrf:
                case FileTypeAction.ViewDistantLights:
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
            return item.FileType.XmlConvertible;
        }

        public static void View(MainListItem item)
        {
#if !DEBUG
            try
#endif
            {
                var exform = FindExistingForm(item?.File);
                if (exform != null)
                {
                    exform.Focus();
                    return;
                }

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
                var fe = item.File;
                if (fe == null)
                { 
                    //this should only happen when opening a file from filesystem...
                    fe = RpfFile.CreateFileEntry(name, path, ref data);
                }

                switch (ft.DefaultAction)
                {
                    case FileTypeAction.ViewText:
                        ViewText(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewXml:
                        ViewXml(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYtd:
                        ViewYtd(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYmt:
                        ViewYmt(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYmf:
                        ViewYmf(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYmap:
                        ViewYmap(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYtyp:
                        ViewYtyp(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewJPso:
                        ViewJPso(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewCut:
                        ViewCut(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewModel:
                        ViewModel(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewAwc:
                        ViewAwc(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewGxt:
                        ViewGxt(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewRel:
                        ViewRel(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewFxc:
                        ViewFxc(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYwr:
                        ViewYwr(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYvr:
                        ViewYvr(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYcd:
                        ViewYcd(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYnd:
                        ViewYnd(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewCacheDat:
                        ViewCacheDat(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYed:
                        ViewYed(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYld:
                        ViewYld(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYfd:
                        ViewYfd(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewHeightmap:
                        ViewHeightmap(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewMrf:
                        ViewMrf(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewNametable:
                        ViewNametable(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewDistantLights:
                        ViewDistantLights(name, path, data, fe);
                        break;
                    case FileTypeAction.ViewYpdb:
                        ViewYpdb(name, path, data, fe);
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
                Instance?.UpdateErrorLog(ex.ToString());
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
                ErrorLog?.Invoke(ex.ToString());
                return;
            }
        }
        private static void ViewHex(string name, string path, byte[] data)
        {
            HexForm f = new HexForm();
            f.Show();
            f.LoadData(name, path, data);
        }
        private static void ViewXml(string name, string path, byte[] data, RpfFileEntry e)
        {
            string xml = Encoding.UTF8.GetString(data);
            XmlForm f = new XmlForm();
            f.Show();
            f.LoadXml(name, path, xml, e);
        }
        private static void ViewText(string name, string path, byte[] data, RpfFileEntry e)
        {
            string txt = Encoding.UTF8.GetString(data);
            TextForm f = new TextForm();
            f.Show();
            f.LoadText(name, path, txt, e);
        }
        private static void ViewYtd(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ytd = RpfFile.GetFile<YtdFile>(e, data);
            YtdForm f = new YtdForm();
            f.Show();
            f.LoadYtd(ytd);
        }
        private static void ViewYmt(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ymt = RpfFile.GetFile<YmtFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(ymt);
        }
        private static void ViewYmf(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ymf = RpfFile.GetFile<YmfFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(ymf);
        }
        private static void ViewYmap(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ymap = RpfFile.GetFile<YmapFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(ymap);
        }
        private static void ViewYtyp(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ytyp = RpfFile.GetFile<YtypFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(ytyp);
        }
        private static void ViewJPso(string name, string path, byte[] data, RpfFileEntry e)
        {
            var pso = RpfFile.GetFile<JPsoFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(pso);
        }
        private static void ViewModel(string name, string path, byte[] data, RpfFileEntry e)
        {
            ModelForm f = new ModelForm();
            f.ViewModel(data, e);
        }
        private static void ViewCut(string name, string path, byte[] data, RpfFileEntry e)
        {
            var cut = RpfFile.GetFile<CutFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(cut);
        }
        private static void ViewAwc(string name, string path, byte[] data, RpfFileEntry e)
        {
            var awc = RpfFile.GetFile<AwcFile>(e, data);
            AwcForm f = new AwcForm();
            f.Show();
            f.LoadAwc(awc);
        }
        private static void ViewGxt(string name, string path, byte[] data, RpfFileEntry e)
        {
            var gxt = RpfFile.GetFile<Gxt2File>(e, data);
            TextForm f = new TextForm();
            f.Show();
            f.LoadGxt2(name, path, gxt);
        }
        private static void ViewRel(string name, string path, byte[] data, RpfFileEntry e)
        {
            var rel = RpfFile.GetFile<RelFile>(e, data);
            RelForm f = new RelForm();
            f.Show();
            f.LoadRel(rel);
        }
        private static void ViewFxc(string name, string path, byte[] data, RpfFileEntry e)
        {
            var fxc = RpfFile.GetFile<FxcFile>(e, data);
            FxcForm f = new FxcForm();
            f.Show();
            f.LoadFxc(fxc);
        }
        private static void ViewYwr(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ywr = RpfFile.GetFile<YwrFile>(e, data);
            YwrForm f = new YwrForm();
            f.Show();
            f.LoadYwr(ywr);
        }
        private static void ViewYvr(string name, string path, byte[] data, RpfFileEntry e)
        {
            var yvr = RpfFile.GetFile<YvrFile>(e, data);
            YvrForm f = new YvrForm();
            f.Show();
            f.LoadYvr(yvr);
        }
        private static void ViewYcd(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ycd = RpfFile.GetFile<YcdFile>(e, data);
            YcdForm f = new YcdForm();
            f.Show();
            f.LoadYcd(ycd);
        }
        private static void ViewYnd(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ynd = RpfFile.GetFile<YndFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(ynd);
        }
        private static void ViewYed(string name, string path, byte[] data, RpfFileEntry e)
        {
            var yed = RpfFile.GetFile<YedFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(yed);
        }
        private static void ViewYld(string name, string path, byte[] data, RpfFileEntry e)
        {
            var yld = RpfFile.GetFile<YldFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(yld);
        }
        private static void ViewYfd(string name, string path, byte[] data, RpfFileEntry e)
        {
            var yfd = RpfFile.GetFile<YfdFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(yfd);
        }
        private static void ViewCacheDat(string name, string path, byte[] data, RpfFileEntry e)
        {
            var cachedat = RpfFile.GetFile<CacheDatFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(cachedat);
        }
        private static void ViewHeightmap(string name, string path, byte[] data, RpfFileEntry e)
        {
            var heightmap = RpfFile.GetFile<HeightmapFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(heightmap);
        }
        private static void ViewMrf(string name, string path, byte[] data, RpfFileEntry e)
        {
            var mrf = RpfFile.GetFile<MrfFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(mrf);
        }
        private static void ViewNametable(string name, string path, byte[] data, RpfFileEntry e)
        {
            TextForm f = new TextForm();
            f.Show();
            f.LoadNametable(name, path, data, e);
        }
        private static void ViewDistantLights(string name, string path, byte[] data, RpfFileEntry e)
        {
            var dlf = RpfFile.GetFile<DistantLightsFile>(e, data);
            GenericForm f = new GenericForm();
            f.Show();
            f.LoadFile(dlf, dlf.RpfFileEntry);
        }
        private static void ViewYpdb(string name, string path, byte[] data, RpfFileEntry e)
        {
            var ypdb = RpfFile.GetFile<YpdbFile>(e, data);
            MetaForm f = new MetaForm();
            f.Show();
            f.LoadMeta(ypdb);
        }


        public static Form FindExistingForm(RpfFileEntry e)
        {
            if (e == null) return null;
            var allforms = Application.OpenForms;
            var path = e.Path;
            foreach (var form in allforms)
            {
                if (form is MetaForm metaForm)
                {
                    if (metaForm.rpfFileEntry == e) return metaForm;
                    if (metaForm.rpfFileEntry?.Path?.Equals(path, StringComparison.OrdinalIgnoreCase) ?? false)
                        return metaForm; //need to test the path as well since the file entry may have been replaced by a new version..!
                }
            }
            return null;
        }


        private void ShowTreeContextMenu(TreeNode n, Point p)
        {
            var f = n?.Tag as MainTreeFolder;

            bool filesys = ((f.RpfFolder == null) && (f.RpfFile == null));
            bool expanded = ((n != null) && (n.IsExpanded));
            bool collapsed = ((n != null) && (!n.IsExpanded));
            bool extrafldr = ((n?.Parent == null) && (f?.IsExtraFolder ?? false));

            if ((f.RpfFile != null) && (f.RpfFile.Parent == null))
            {
                filesys = true; //allow viewing root RPF's in explorer
            }

            TreeContextWinExplorerMenu.Enabled = filesys;
            TreeContextExpandMenu.Enabled = collapsed;
            TreeContextCollapseMenu.Enabled = expanded;
            TreeContextCloseFolderSeparator.Visible = extrafldr;
            TreeContextCloseFolderMenu.Visible = extrafldr;


            TreeContextMenu.Show(MainTreeView, p);

        }

        private void ShowListContextMenu(MainListItem item)
        {

            bool isitem = false;
            bool isfile = false;
            bool isfolder = false;
            bool isarchive = false;
            bool isfilesys = false;
            bool isrpffolder = (CurrentFolder?.RpfFolder != null);
            bool issearch = CurrentFolder?.IsSearchResults ?? false;
            bool canview = false;
            bool canexportxml = false;
            bool canextract = false;
            bool canimport = EditMode && !issearch;// && isrpffolder;
            bool cancreate = EditMode && !issearch;
            bool canedit = false;
            bool candefrag = false;
            bool canpaste = EditMode && (CopiedFiles.Count > 0);

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
            ListContextImportFbxMenu.Visible = canimport;
            ListContextImportXmlMenu.Visible = canimport;
            ListContextImportSeparator.Visible = cancreate;

            ListContextCopyMenu.Enabled = isfile;
            ListContextCopyPathMenu.Enabled = isitem;
            ListContextPasteMenu.Enabled = canpaste;
            ListContextPasteMenu.Visible = EditMode;

            ListContextOpenFileLocationMenu.Visible = issearch;
            ListContextOpenFileLocationSeparator.Visible = issearch;

            ListContextRenameMenu.Visible = canedit;
            ListContextDeleteMenu.Visible = canedit;
            ListContextEditSeparator.Visible = canedit;

            ListContextDefragmentMenu.Visible = candefrag;
            ListContextDefragmentSeparator.Visible = candefrag;

            ListContextMenu.Show(Cursor.Position);

        }




        private void EnableEditMode(bool enable, bool warn = true)
        {
            if (EditMode == enable)
            {
                return;
            }

            if (enable && warn)
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
            UpdateSelectionUI();
        }

        private void EnsureEditModeWarning()
        {
            bool mods = CurrentFolder.Path.StartsWith("mods", StringComparison.OrdinalIgnoreCase);
            bool srch = CurrentFolder?.IsSearchResults ?? false;
            bool fsys = CurrentFolder?.RpfFolder == null;
            bool game = CurrentFolder?.Path != CurrentFolder?.FullPath;
            bool show = EditMode && !mods && !srch && (!fsys || game);
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





        public static bool EnsureRpfValidEncryption(RpfFile file)
        {
            if (file == null) return false;

            var confirm = new Func<RpfFile, bool>((f) => 
            {
                var msg = "Archive " + f.Name + " is currently set to " + f.Encryption.ToString() + " encryption.\nAre you sure you want to change this archive to OPEN encryption?\nLoading by the game will require OpenIV.asi.";
                return (MessageBox.Show(msg, "Change RPF encryption type", MessageBoxButtons.YesNo) == DialogResult.Yes);
            });

            return RpfFile.EnsureValidEncryption(file, confirm);
        }


        public bool EnsureCurrentFolderEditable()
        {
            if (!EditMode) return false;

            var parentrpffldr = CurrentFolder.RpfFolder;
            var parentfullpath = CurrentFolder.FullPath;
            if ((parentrpffldr == null) && (string.IsNullOrEmpty(parentfullpath)))
            {
                MessageBox.Show("No parent folder selected! This shouldn't happen. Refresh the view and try again.");
                return false;
            }

            return true;
        }


        public string SelectFolder()
        {
            var selpath = Settings.Default.RPFExplorerSelectedFolder;
            if (!string.IsNullOrEmpty(selpath))
            {
                FolderBrowserDialog.SelectedPath = selpath;
            }

            if (FolderBrowserDialog.ShowDialogNew() != DialogResult.OK) return "";
            string folderpath = FolderBrowserDialog.SelectedPath;
            if (!folderpath.EndsWith("\\")) folderpath += "\\";

            Settings.Default.RPFExplorerSelectedFolder = folderpath;

            return folderpath;
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

        private class TextureUsageData
        {
            public HashSet<string> Models { get; set; } = new HashSet<string>();
            public HashSet<string> ContainedIn { get; set; } = new HashSet<string>();
            public int Count { get; set; } = 0;
            public string TextureName { get; set; }
        }

        private void GetShadersXml()
        {
            bool doydr = true;
            bool doydd = true;
            bool doyft = true;
            bool doypt = true;

            var data = new Dictionary<MetaHash, ShaderXmlDataCollection>();
            var textures = new Dictionary<string, TextureUsageData>();

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
                        dc.AddShaderUse(s, geom, d);
                    }
                }
            }

            void collectTextures(DrawableBase d)
            {
                collectTexturesFromDict(d?.ShaderGroup?.TextureDictionary, (d.Owner as GameFile)?.Name ?? "");
                if (d?.AllModels == null) return;
                foreach (var model in d.AllModels)
                {
                    if (model?.Geometries == null) continue;
                    foreach (var geom in model.Geometries)
                    {
                        var s = geom?.Shader;
                        if (s == null) continue;
                        addTextureuse(s, geom, d);
                    }
                }
            }

            void collectTexturesFromDict(TextureDictionary textureDictionary, string ownerName)
            {
                if (textureDictionary == null) return;
                foreach(var texture in textureDictionary.Dict)
                {
                    if (!textures.TryGetValue(texture.Value.Name, out var tex))
                    {
                        tex = new TextureUsageData()
                        {
                            TextureName = texture.Value.Name
                        };
                        textures[texture.Value.Name] = tex;
                    }

                    tex.ContainedIn.Add(ownerName);
                }
            }

            void addTextureuse(ShaderFX s, DrawableGeometry g, DrawableBase d = null)
            {
                if (s.ParametersList?.Parameters == null) return;
                if (s.ParametersList?.Hashes == null) return;

                for (int i = 0; i < s.ParametersList.Count; i++)
                {
                    var h = s.ParametersList.Hashes[i];
                    var p = s.ParametersList.Parameters[i];

                    if (p.DataType == 0 && d.Owner is GameFile gameFile)//texture
                    {
                        if (p.Data is TextureBase texture)
                        {
                            if (!textures.TryGetValue(texture.Name, out var tex))
                            {
                                tex = new TextureUsageData()
                                {
                                    TextureName = texture.Name
                                };
                                textures[texture.Name] = tex;
                            }

                            tex.Count++;
                            tex.Models.Add($"{gameFile.Name}");
                        }

                    }
                }
            }

            bool isModelFile(MainListItem item)
            {
                return item.FullPath.EndsWithAny(".ydr", ".ydd", ".yft", ".ypt", ".ytd");
            }

            var queue = new ConcurrentQueue<MainTreeFolder>();

            var file = CurrentFolder.RpfFile;

            queue.Enqueue(CurrentFolder);

            while (queue.TryDequeue(out var folder))
            {
                foreach (var item in folder.GetListItems())
                {
                    try
                    {
                        if (item.Folder != null)
                        {
                            queue.Enqueue(item.Folder);
                            continue;
                        }
                        if (!isModelFile(item))
                        {
                            continue;
                        }

                        byte[] fileData = null;
                        string name = "";
                        string path = "";

                        if (item.File != null)
                        {
                            if (item.File.File == null) continue;
                            fileData = item.File.File.ExtractFile(item.File);
                            name = item.Name;
                            path = item.FullPath;
                        }
                        else if (!string.IsNullOrEmpty(item.FullPath))
                        {
                            //load file from filesystem
                            fileData = File.ReadAllBytes(item.FullPath);
                            name = new FileInfo(item.FullPath).Name;
                            path = item.FullPath;
                        }
                        var entry = item.File;
                        if (entry == null)
                        {
                            entry = RpfFile.CreateFileEntry(name, path, ref fileData);
                        }
                        if (doydr && entry.IsExtension(".ydr"))
                        {
                            UpdateStatus?.Invoke(entry.Path);
                            YdrFile ydr = RpfFile.GetFile<YdrFile>(entry, fileData);

                            if (ydr == null) { continue; }
                            if (ydr.Drawable == null) { continue; }
                            collectDrawable(ydr.Drawable);
                            collectTextures(ydr.Drawable);
                        }
                        else if (doydd & entry.IsExtension(".ydd"))
                        {
                            UpdateStatus?.Invoke(entry.Path);
                            YddFile ydd = RpfFile.GetFile<YddFile>(entry, fileData);

                            if (ydd == null) { continue; }
                            if (ydd.Dict == null) { continue; }
                            foreach (var drawable in ydd.Dict.Values)
                            {
                                collectDrawable(drawable);
                                collectTextures(drawable);
                            }
                        }
                        else if (doyft && entry.IsExtension(".yft"))
                        {
                            UpdateStatus?.Invoke(entry.Path);
                            YftFile yft = RpfFile.GetFile<YftFile>(entry, fileData);

                            if (yft == null) { continue; }
                            if (yft.Fragment == null) { continue; }
                            if (yft.Fragment.Drawable != null)
                            {
                                collectDrawable(yft.Fragment.Drawable);
                                collectTextures(yft.Fragment.Drawable);
                            }
                            if ((yft.Fragment.Cloths != null) && (yft.Fragment.Cloths.data_items != null))
                            {
                                foreach (var cloth in yft.Fragment.Cloths.data_items)
                                {
                                    collectDrawable(cloth.Drawable);
                                    collectTextures(cloth.Drawable);
                                }
                            }
                            if ((yft.Fragment.DrawableArray != null) && (yft.Fragment.DrawableArray.data_items != null))
                            {
                                foreach (var drawable in yft.Fragment.DrawableArray.data_items)
                                {
                                    collectDrawable(drawable);
                                    collectTextures(drawable);
                                }
                            }
                        }
                        else if (doypt && entry.IsExtension(".ypt"))
                        {
                            UpdateStatus?.Invoke(entry.Path);
                            YptFile ypt = RpfFile.GetFile<YptFile>(entry, fileData);

                            if (ypt == null) { continue; }
                            if (ypt.DrawableDict == null) { continue; }
                            foreach (var drawable in ypt.DrawableDict.Values)
                            {
                                collectDrawable(drawable);
                                collectTextures(drawable);
                            }
                        } else if (entry.IsExtension(".ytd"))
                        {
                            UpdateStatus?.Invoke(entry.Path);
                            YtdFile ytd = RpfFile.GetFile<YtdFile>(entry, fileData);
                            var textureName = ytd.Name;
                            if (string.IsNullOrEmpty(name))
                            {
                                textureName = entry.Name;
                            }

                            collectTexturesFromDict(ytd.TextureDict, textureName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }



            
            File.WriteAllText(Path.Combine(CurrentFolder.FullPath, "texture_usage.json"), System.Text.Json.JsonSerializer.Serialize(textures.Values.OrderBy(p => p.Count), new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

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
                var texparams = s.GetSortedList(s.TextureData);
                var valparams = s.ValParams;
                var arrparams = s.ArrParams;
                foreach (var tp in texparams)
                {
                    //MetaXml.WriteCustomItemArray(sb, tp.Value.Textures, 3, "Texture");
                    MetaXml.OpenTag(sb, 3, string.Format(otstr, ((ShaderParamNames)tp.Key).ToString(), "Texture"));

                    foreach(var texture in tp.Value.Textures)
                    {
                        sb.AppendLine(texture);
                    }

                    MetaXml.CloseTag(sb, 3, "Item");
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

            
            File.WriteAllText(Path.Combine(CurrentFolder.FullPath, "shaders.xml"), xml);
        }

        private void ExportXml()
        {
            bool needfolder = false;//need a folder to output ytd XML to, for the texture .dds files
            if (MainListView.SelectedIndices.Count == 1)
            {
                var idx = MainListView.SelectedIndices[0];
                if ((idx < 0) || (idx >= CurrentFiles.Count)) return;
                var file = CurrentFiles[idx];
                var nl = file?.File?.Name ?? file?.Name;
                if (!string.IsNullOrEmpty(nl))
                {
                    needfolder = nl.EndsWith(".ytd", StringComparison.OrdinalIgnoreCase)
                        || nl.EndsWith(".ydr", StringComparison.OrdinalIgnoreCase)
                        || nl.EndsWith(".ydd", StringComparison.OrdinalIgnoreCase)
                        || nl.EndsWith(".yft", StringComparison.OrdinalIgnoreCase)
                        || nl.EndsWith(".ypt", StringComparison.OrdinalIgnoreCase)
                        || nl.EndsWith(".awc", StringComparison.OrdinalIgnoreCase)
                        || nl.EndsWith(".fxc", StringComparison.OrdinalIgnoreCase);
                }
            }

            var refreshNeeded = false;

            if ((MainListView.SelectedIndices.Count == 1) && (!needfolder))
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
                        var fentry = file?.File;
                        if (fentry == null)
                        {
                            //this should only happen when opening a file from filesystem...
                            var name = new FileInfo(file.FullPath).Name;
                            fentry = RpfFile.CreateFileEntry(name, file.FullPath, ref data);
                        }

                        string xml = MetaXml.GetXml(fentry, data, out newfn);
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
                                if (CurrentFolder.FullPath.Equals(Path.GetDirectoryName(path), StringComparison.OrdinalIgnoreCase))
                                {
                                    CurrentFolder.EnsureFile(path);
                                    refreshNeeded = true;
                                }
                                
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
                var folderpath = SelectFolder();
                if (string.IsNullOrEmpty(folderpath)) return;

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
                        var fentry = file?.File;
                        if (fentry == null)
                        {
                            //this should only happen when opening a file from filesystem...
                            var name = new FileInfo(file.FullPath).Name;
                            fentry = RpfFile.CreateFileEntry(name, file.FullPath, ref data);
                        }

                        string xml = MetaXml.GetXml(fentry, data, out newfn, folderpath);
                        if (string.IsNullOrEmpty(xml))
                        {
                            errors.AppendLine("Unable to convert file to XML: " + file.Path);
                            continue;
                        }

                        var path = folderpath + newfn;
                        try
                        {
                            File.WriteAllText(path, xml);
                            if (CurrentFolder?.FullPath?.Equals(Path.GetDirectoryName(path), StringComparison.OrdinalIgnoreCase) ?? false)
                            {
                                CurrentFolder.EnsureFile(path);
                                refreshNeeded = true;
                            }
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

            if (refreshNeeded) {
                RefreshMainListView();
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
                var folderpath = SelectFolder();
                if (string.IsNullOrEmpty(folderpath)) return;

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
                var folderpath = SelectFolder();
                if (string.IsNullOrEmpty(folderpath)) return;

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
            
            var folderpath = SelectFolder();
            if (string.IsNullOrEmpty(folderpath)) return;

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

            var fnames = fname.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            RpfDirectoryEntry newdir = null;
            MainTreeFolder node = null;
            MainTreeFolder cnode = null;
            string cpath = (string.IsNullOrEmpty(CurrentFolder.Path) ? "" : CurrentFolder.Path + "\\");
            var rootpath = GTAFolder.GetCurrentGTAFolderWithTrailingSlash();
            var csubpath = "";

            foreach (var name in fnames)
            {
                if (!IsFilenameOk(name)) break; //new name contains invalid char(s). don't continue

                csubpath += name;

                string relpath = cpath + csubpath.ToLowerInvariant();
                string fullpath = rootpath + relpath;

                try
                {
                    if (CurrentFolder.RpfFolder != null)
                    {
                        if (!EnsureRpfValidEncryption(CurrentFolder.RpfFolder.File)) return;

                        //create new directory entry in the RPF.

                        newdir = RpfFile.CreateDirectory(newdir ?? CurrentFolder.RpfFolder, name);

                        var newnode = CreateRpfDirTreeFolder(newdir, relpath, fullpath);

                        if (node == null)
                        {
                            node = newnode;
                            cnode = newnode;
                        }
                        else
                        {
                            cnode.AddChild(newnode);
                            cnode = newnode;
                        }
                    }
                    else
                    {
                        //create a folder in the filesystem.
                        if (Directory.Exists(fullpath))
                        {
                            throw new Exception("Folder " + fullpath + " already exists!");
                        }
                        Directory.CreateDirectory(fullpath);

                        var newnode = CreateRootDirTreeFolder(name, relpath, fullpath);
                        if (node == null)
                        {
                            node = newnode;
                            cnode = newnode;
                        }
                        else
                        {
                            cnode.AddChild(newnode);
                            cnode = newnode;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating new folder: " + ex.Message, "Unable to create new folder");
                    return;
                }

                csubpath += "\\";
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

            if (!fname.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
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
                    if (!EnsureRpfValidEncryption(CurrentFolder.RpfFolder.File)) return;

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
                RecurseMainTreeViewRPF(node, new ConcurrentBag<RpfFile>(AllRpfs));
                AddNewFolderTreeNode(node);
            }

        }
        private void NewYtdFile()
        {
            if (CurrentFolder == null) return;//shouldn't happen
            if (CurrentFolder?.IsSearchResults ?? false) return;

            string fname = Prompt.ShowDialog(this, "Enter a name for the new YTD file:", "Create YTD (Texture Dictionary)", "new");
            if (string.IsNullOrEmpty(fname))
            {
                return;//no name was provided.
            }
            if (!IsFilenameOk(fname)) return; //new name contains invalid char(s). don't do anything

            if (!fname.EndsWith(".ytd", StringComparison.OrdinalIgnoreCase))
            {
                fname = fname + ".ytd";//make sure it ends with .ytd
            }

            var ytd = new YtdFile();
            ytd.TextureDict = new TextureDictionary();
            ytd.TextureDict.Textures = new ResourcePointerList64<Texture>();
            ytd.TextureDict.TextureNameHashes = new ResourceSimpleList64_uint();
            var data = ytd.Save();

            if (CurrentFolder.RpfFolder != null) //create in RPF archive
            {
                RpfFile.CreateFile(CurrentFolder.RpfFolder, fname, data);
            }
            else //create in filesystem
            {
                var outfpath = Path.Combine(CurrentFolder.FullPath, fname);
                File.WriteAllBytes(outfpath, data);
                CurrentFolder.EnsureFile(outfpath);
            }

            RefreshMainListView();
        }
        private void ImportFbx()
        {
            if (!EditMode) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;

            if (!EnsureCurrentFolderEditable()) return;

            if (!EnsureRpfValidEncryption(CurrentFolder.RpfFolder.File) && (CurrentFolder.RpfFolder != null)) return;


            OpenFileDialog.Filter = "FBX Files|*.fbx";
            if (OpenFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;//canceled
            }

            var fpaths = OpenFileDialog.FileNames;
            var fdict = new Dictionary<string, byte[]>();

            foreach (var fpath in fpaths)
            {
#if !DEBUG
                try
#endif
                {
                    if (!File.Exists(fpath))
                    {
                        continue;//this shouldn't happen...
                    }

                    var fi = new FileInfo(fpath);
                    var fname = fi.Name;
                    var trimlength = 4;

                    if (!fname.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show(fname + ": Not an FBX file!", "Cannot import FBX");
                        continue;
                    }

                    fname = fname.Substring(0, fname.Length - trimlength);

                    var data = File.ReadAllBytes(fpath);
                    fdict[fname] = data;

                }
#if !DEBUG
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Unable to read file " + fpath);
                }
#endif

            }


            var fbxForm = new ImportFbxForm();
            fbxForm.SetInputFiles(fdict);
            fbxForm.ShowDialog();

            if (fbxForm.DialogResult != DialogResult.OK)
            {
                return; //fbx import canceled
            }

            var converted = fbxForm.GetOutputFiles();
            if (converted == null)
            {
                return;
            }

            foreach (var kvp in converted)
            {
                var fname = kvp.Key;
                var data = kvp.Value;
                if (data != null)
                {
                    if (CurrentFolder.RpfFolder != null)
                    {
                        RpfFile.CreateFile(CurrentFolder.RpfFolder, fname, data);
                    }
                    else if (!string.IsNullOrEmpty(CurrentFolder.FullPath))
                    {
                        var outfpath = Path.Combine(CurrentFolder.FullPath, fname);
                        File.WriteAllBytes(outfpath, data);
                        CurrentFolder.EnsureFile(outfpath);
                    }

                }
            }


            RefreshMainListView();

        }
        private void ImportXmlDialog()
        {
            if (!EditMode) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;

            if (!EnsureCurrentFolderEditable()) return;

            if ((CurrentFolder.RpfFolder != null) && !EnsureRpfValidEncryption(CurrentFolder.RpfFolder?.File)) return;

            if (MainListView.SelectedIndices.Count > 0)
            {
                var paths = new List<string>();
                for (int i = 0; i < MainListView.SelectedIndices.Count; i++)
                {
                    var idx = MainListView.SelectedIndices[i];
                    if ((idx < 0) || (idx >= CurrentFiles.Count)) continue;

                    paths.Add(CurrentFiles[idx].FullPath);
                }
                ImportXml(paths.ToArray());
                return;
            }

            OpenFileDialog.Filter = "XML Files|*.xml";
            if (OpenFileDialog.ShowDialog(this) != DialogResult.OK) return;
            ImportXml(OpenFileDialog.FileNames);
        }
        private void ImportXml(string[] fpaths)
        {
            foreach (var fpath in fpaths)
            {
#if !DEBUG
                try
#endif
                {
                    if (!File.Exists(fpath))
                    {
                        continue;//this shouldn't happen...
                    }

                    var fi = new FileInfo(fpath);
                    var fname = fi.Name;
                    var fpathin = fpath;

                    if (!fname.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show(fname + ": Not an XML file!", "Cannot import XML");
                        continue;
                    }

                    var trimlength = 4;
                    var mformat = XmlMeta.GetXMLFormat(fname, out trimlength);

                    fname = fname.Substring(0, fname.Length - trimlength);
                    fpathin = fpathin.Substring(0, fpathin.Length - trimlength);
                    fpathin = Path.Combine(Path.GetDirectoryName(fpathin), Path.GetFileNameWithoutExtension(fpathin));

                    var doc = new XmlDocument();
                    string text = File.ReadAllText(fpath);
                    if (!string.IsNullOrEmpty(text))
                    {
                        doc.LoadXml(text);
                    }

                    byte[] data = XmlMeta.GetData(doc, mformat, fpathin);

                    if (data != null)
                    {
                        if (CurrentFolder.RpfFolder != null)
                        {
                            RpfFile.CreateFile(CurrentFolder.RpfFolder, fname, data);
                        }
                        else if (!string.IsNullOrEmpty(CurrentFolder.FullPath))
                        {
                            var outfpath = Path.Combine(CurrentFolder.FullPath, fname);
                            File.WriteAllBytes(outfpath, data);
                            CurrentFolder.EnsureFile(outfpath);
                        }
                    }
                    else
                    {
                        MessageBox.Show(fname + ": Schema not supported.", "Cannot import " + XmlMeta.GetXMLFormatName(mformat));
                    }

                }
#if !DEBUG
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Unable to import file");
                }
#endif

            }

            RefreshMainListView();

        }
        private void ImportRaw()
        {
            if (!EditMode) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;

            if (!EnsureRpfValidEncryption(CurrentFolder.RpfFolder.File) && (CurrentFolder.RpfFolder != null)) return;

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

            if (!EnsureCurrentFolderEditable()) return;

            if (checkEncryption)
            {
                if (!EnsureRpfValidEncryption(CurrentFolder.RpfFolder.File) && (CurrentFolder.RpfFolder != null)) return;
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
                    else if (Directory.Exists(fpath) && (CurrentFolder.RpfFolder != null)) //create imported directory structure in the RPF.
                    {
                        //create the first directory entry.
                        var fdi = new DirectoryInfo(fpath);
                        var direntry = RpfFile.CreateDirectory(CurrentFolder.RpfFolder, fdi.Name);
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

                        EnsureImportedFolder(direntry, CurrentFolder.RpfFolder);
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

                    if (CurrentFolder.RpfFolder != null)
                    {
                        var rpffldr = CurrentFolder.RpfFolder;
                        if (dirdict.ContainsKey(fpath))
                        {
                            rpffldr = dirdict[fpath];
                        }

                        var entry = RpfFile.CreateFile(rpffldr, fname, data);

                        EnsureImportedRpf(entry, rpffldr); //make sure structure is created if an RPF was imported
                    }
                    else
                    {

                        var outfpath = Path.Combine(CurrentFolder.FullPath, fname);
                        File.WriteAllBytes(outfpath, data);
                        CurrentFolder.EnsureFile(outfpath);

                        //TODO: folders...
                    }

                    Cursor = Cursors.Default;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Unable to import file");
                }
            }

            RefreshMainListView();
        }
        private void CopySelected()
        {
            CopiedFiles.Clear();
            if (MainListView.SelectedIndices.Count <= 0) return;
            var fnames = new StringBuilder();
            foreach (int idx in MainListView.SelectedIndices)
            {
                if ((idx < 0) || (idx >= CurrentFiles.Count)) continue;
                var f = CurrentFiles[idx];
                CopiedFiles.Add(f);
                if (fnames.Length > 0) fnames.AppendLine();
                fnames.Append(f.Name);
            }
            if (fnames.Length > 0)
            {
                Clipboard.SetText(fnames.ToString());
            }
            UpdateStatus?.Invoke(CopiedFiles.Count.ToString() + " item" + ((CopiedFiles.Count != 1) ? "s" : "") + " copied");
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
                    if (!EnsureRpfValidEncryption(CurrentFolder.RpfFolder.File)) return;

                    //renaming an entry in an RPF
                    RpfFile.RenameEntry(entry, newname);
                }
                else
                {
                    //renaming a filesystem item...
                    var dirinfo = new DirectoryInfo(item.FullPath);
                    var newpath = Path.Combine(dirinfo.Parent.FullName, newname);
                    if (item.FullPath.Equals(newpath, StringComparison.OrdinalIgnoreCase))
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
                    if (!EnsureRpfValidEncryption(CurrentFolder.RpfFolder.File)) return;

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

                        item.Parent?.RemoveFile(item.FullPath);
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
                ErrorLog?.Invoke(ex.ToString());
            }
        }
        private void OpenFileLocation()
        {
            var ind = -1;
            if (MainListView.SelectedIndices.Count == 1)
            {
                ind = MainListView.SelectedIndices[0];
            }
            if (ind < 0) return;
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
        private void OpenFolder()
        {
            var folderPath = SelectFolder();
            if (string.IsNullOrEmpty(folderPath)) return;
            if (!Directory.Exists(folderPath)) return;

            foreach (var folder in ExtraRootFolders)
            {
                if (folder.FullPath == folderPath) return;
            }

            var root = new MainTreeFolder
            {
                FullPath = folderPath,
                Path = folderPath,
                Name = Path.GetFileName(Path.GetDirectoryName(folderPath)),
                IsExtraFolder = true,
            };
            ExtraRootFolders.Add(root);

            Task.Run(() =>
            {
                var node = RefreshMainTreeViewRoot(root);

                Invoke(new Action(() => 
                {
                    MainTreeView.Nodes.Add(node);
                    MainTreeView.SelectedNode = root.TreeNode;
                }));
            }, CancellationTokenSource.Token);
        }
        private void CloseFolder(MainTreeFolder folder)
        {
            if (folder == null) return;
            if (folder.IsExtraFolder == false) return;

            folder.TreeNode.Remove();
            ExtraRootFolders.Remove(folder);
        }
        private void Paste()
        {
            if (!EditMode) return;
            if (CopiedFiles.Count == 0) return;
            if (CurrentFolder?.IsSearchResults ?? false) return;

            if (!EnsureCurrentFolderEditable()) return;

            if (!EnsureRpfValidEncryption(CurrentFolder.RpfFolder.File) && (CurrentFolder.RpfFolder != null)) return;

            foreach (var file in CopiedFiles)
            {
                if (file.Parent?.FullPath == CurrentFolder?.FullPath) continue; //don't try to paste into the same folder
                if ((file.Folder == null) || (file.Folder.RpfFile != null)) //it's a file (not a folder) [todo: copy/paste folders..]
                {
                    var data = GetFileDataCompressResources(file);
                    if (data != null)
                    {
                        var fname = file.Name;
                        if (CurrentFolder.RpfFolder != null)
                        {
                            var rpffldr = CurrentFolder.RpfFolder;
                            var entry = RpfFile.CreateFile(rpffldr, fname, data);

                            EnsureImportedRpf(entry, rpffldr); //make sure structure is created if an RPF was imported
                        }
                        else
                        {
                            var outfpath = Path.Combine(CurrentFolder.FullPath, fname);
                            File.WriteAllBytes(outfpath, data);
                            CurrentFolder.EnsureFile(outfpath);

                            //TODO: folders...
                        }

                    }
                }
            }


            RefreshMainListView();


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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void WndProc(ref Message m)
        {
            //handle back/forward buttons globally for all the form
            if (m.Msg == 0x319) //WM_APPCOMMAND
            {
                var cmd = (m.LParam.ToInt64() >> 16) & 0xFFF;
                if (cmd == 1)
                {
                    GoBack(); //APPCOMMAND_BROWSER_BACKWARD
                }
                else if (cmd == 2)
                {
                    GoForward(); //APPCOMMAND_BROWSER_FORWARD
                }
            }
            base.WndProc(ref m);

            if (m.Msg == 0x10) // WM_CLOSE
            {
                ConsoleWindow.Close();
                CancellationTokenSource.Cancel();
            }
        }


        private void ExploreForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void ExploreForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Console.WriteLine($"Form is closing! {DateTime.Now}");
            CleanupDropFolder();
            SaveSettings();
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
                    if (shft) ImportXmlDialog();
                    else if (!ctrl) ImportRaw();
                    break;
                case Keys.C:
                    if (ctrlshft) CopyPath();
                    else if (ctrl) CopySelected();
                    break;
                case Keys.V:
                    if (ctrl) Paste();
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
            if (e.Control || (e.KeyCode == Keys.ControlKey)) return;
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

                //Import as raw regardless of file type while pressing shift
                if ((e.KeyState & 4) == 4)
                {
                    ImportRaw(files);
                    return;
                }

                var xml = files.Where(x => x.EndsWith(".xml") && (x.IndexOf('.') != x.LastIndexOf('.')));
                var raw = files.Except(xml);

                if (raw.Count() > 0) ImportRaw(raw.ToArray());
                if (xml.Count() > 0) ImportXml(xml.ToArray());
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
            if (!Ready) return;
            Task.Run(async () =>
            {
                await RefreshMainTreeView();

                UpdateStatus?.Invoke("Scan complete.");
            }, CancellationTokenSource.Token);
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

        private void TreeContextCloseFolderMenu_Click(object sender, EventArgs e)
        {
            var folder = MainTreeView.SelectedNode?.Tag as MainTreeFolder;
            CloseFolder(folder);
        }

        private void ListContextViewMenu_Click(object sender, EventArgs e)
        {
            ViewSelected();
        }

        private void ListContextViewHexMenu_Click(object sender, EventArgs e)
        {
            ViewSelectedHex();
        }

        private void ListContextExportShaders_Click(object sender, EventArgs e)
        {
            GetShadersXml();
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

        private void ListContextNewYtdFileMenu_Click(object sender, EventArgs e)
        {
            NewYtdFile();
        }

        private void ListContextImportFbxMenu_Click(object sender, EventArgs e)
        {
            ImportFbx();
        }

        private void ListContextImportXmlMenu_Click(object sender, EventArgs e)
        {
            ImportXmlDialog();
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

        private void ListContextPasteMenu_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void ListContextOpenFileLocationMenu_Click(object sender, EventArgs e)
        {
            OpenFileLocation();
        }

        private void ListContextRenameMenu_Click(object sender, EventArgs e)
        {
            RenameSelected();
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

        private void FileOpenFolderMenu_Click(object sender, EventArgs e)
        {
            OpenFolder();
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

        private void EditImportFbxMenu_Click(object sender, EventArgs e)
        {
            ImportFbx();
        }

        private void EditImportXmlMenu_Click(object sender, EventArgs e)
        {
            ImportXmlDialog();
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

        private void EditPasteMenu_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void EditRenameMenu_Click(object sender, EventArgs e)
        {
            RenameSelected();
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

        private void ViewConsoleMenu_Click(object sender, EventArgs e)
        {
            ConsoleWindow.Show();
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

        private void ToolsRpfBrowserMenu_Click(object sender, EventArgs e)
        {
            BrowseForm f = new BrowseForm();
            f.Show(this);
        }

        private void ToolsBinSearchMenu_Click(object sender, EventArgs e)
        {
            BinarySearchForm f = new BinarySearchForm();
            f.Show(this);
        }

        private void ToolsAudioExplorerMenu_Click(object sender, EventArgs e)
        {
            AudioExplorerForm f = new AudioExplorerForm();
            f.Show(this);
        }

        private void ToolsJenkGenMenu_Click(object sender, EventArgs e)
        {
            JenkGenForm f = new JenkGenForm();
            f.Show(this);
        }

        private void ToolsJenkIndMenu_Click(object sender, EventArgs e)
        {
            JenkIndForm f = new JenkIndForm();
            f.Show(this);
        }

        private void OptionsStartInEditModeMenu_Click(object sender, EventArgs e)
        {
            OptionsStartInEditModeMenu.Checked = !OptionsStartInEditModeMenu.Checked;
            Settings.Default.RPFExplorerStartInEditMode = OptionsStartInEditModeMenu.Checked;
        }

        private void OptionsStartInFolderDefaultMenu_Click(object sender, EventArgs e)
        {
            Settings.Default.RPFExplorerStartFolder = string.Empty;
            OptionsStartInFolderValueMenu.Text = "(Default)";
        }

        private void OptionsStartInFolderCurrentMenu_Click(object sender, EventArgs e)
        {
            if (CurrentFolder == null) return;
            Settings.Default.RPFExplorerStartFolder = CurrentFolder.Path;
            OptionsStartInFolderValueMenu.Text = string.IsNullOrEmpty(CurrentFolder.Path) ? "(Default)" : CurrentFolder.Path;
        }
    }



    public class MainTreeFolder
    {
        public required string Name { get; set; }

        private string? _nameLower;
        public string NameLower
        {
            get
            {
                return _nameLower ??= Name.ToLowerInvariant();
            }
            set
            {
                _nameLower = value;
            }
        }
        public required string Path { get; set; }
        public string? FullPath { get; set; }
        public RpfFile? RpfFile { get; set; }
        public RpfDirectoryEntry? RpfFolder { get; set; }
        public HashSet<string>? Files { get; set; }
        private object filesLock = new object();
        public MainTreeFolder? Parent { get; set; }
        public List<MainTreeFolder>? Children { get; set; }
        private readonly object childrenLock = new object();
        public List<MainListItem>? ListItems { get; set; }
        public TreeNode? TreeNode { get; set; }
        public bool IsSearchResults { get; set; } = false;
        public string? SearchTerm { get; set; }
        public bool IsExtraFolder { get; set; } = false;

        public void AddFile(string file)
        {
            lock (filesLock)
            {
                if (Files == null)
                {
                    Files = new();
                }
                Files.Add(file);
            }
        }
        public void AddChild(MainTreeFolder child)
        {
            lock(childrenLock)
            {
                if (Children == null) Children = new List<MainTreeFolder>();
                Children.Add(child);
                child.Parent = this;
            }
        }

        public void RemoveChild(MainTreeFolder child) {
            if (Children == null) return;

            lock(childrenLock)
            {
                Children.Remove(child);
                child.Parent = null;
            }
        }

        public void AddChildToHierarchy(MainTreeFolder child)
        {
            var relpath = child.Path.Replace(Path + '\\', "");
            var idx = relpath.IndexOf('\\');
            var lidx = 0;
            var parent = this;
            while (idx > 0)
            {
                var pname = relpath.Substring(lidx, idx - lidx);
                if (parent.Children == null) break;
                foreach (var tc in parent.Children)
                {
                    if (tc.Name.Equals(pname, StringComparison.OrdinalIgnoreCase))
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

        public void EnsureFile(string file)
        {
            if (Files != null)
            {
                if (Files.Contains(file)) return;
            }
            AddFile(file);
        }
        public void RemoveFile(string file)
        {
            lock(filesLock)
            {
                Files?.Remove(file);
            }
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
                    lock(filesLock)
                    {
                        foreach (var file in Files)
                        {
                            ListItems.Add(new MainListItem(file, rootpath, this));
                        }
                    }
                }
                if ((RpfFolder != null) && (RpfFolder.Files != null))
                {
                    foreach (var file in RpfFolder.Files)
                    {
                        if (file.IsExtension(".rpf"))
                            continue; //RPF files are already added..
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
                    if (file.IsExtension(".rpf"))
                        continue; //RPF files are already added..
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

            form.InvokeUpdateStatus("Searching " + Path + "...");

            if (Name.Contains(term, StringComparison.OrdinalIgnoreCase))
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
                    if (fi.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
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
                    if (file.IsExtension(".rpf"))
                        continue; //don't search rpf files..
                    if (file.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
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

            if (this == form.RootFolder)
            {
                var texDict = form.GetFileCache().TryGetTextureDictForTexture(JenkHash.GenHashLower(term)) ?? form.GetFileCache().TryGetTextureDictForTexture(JenkHash.GenHash(term));
                if (texDict != null)
                {
                    form.AddSearchResult(new MainListItem(texDict.RpfFileEntry, rootpath, this));
                    resultcount++;
                }
            }

            form.AddSearchResult(null);

            return resultcount;
        }

        public void Clear()
        {
            RpfFile = null;
            RpfFolder = null;
            Files = null;
            Parent = null;
            Children = null;
            ListItems = null;
            TreeNode = null;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MainListItem
    {
        private string path;
        private string attributes;
        private long fileSize = -1l;
        private string fileSizeText;
        private string fileTypeText;

        public string Name { get; set; }
        public MainTreeFolder Parent { get; set; }
        public MainTreeFolder Folder { get; set; }
        public RpfFileEntry File { get; set; }
        public string Path { get => path; set => path = value; }
        public string FullPath { get; set; }

        public FileTypeInfo FileType { get => ExploreForm.GetFileType(Name); }
        public string FileTypeText {
            get
            {
                if (fileTypeText is null)
                {
                    CacheDetails();
                }
                return fileTypeText;
            }
            set => fileTypeText = value;
        }
        public long FileSize {
            get
            {
                if (fileSize == -1)
                {
                    CacheDetails();
                }
                return fileSize;
            }
            set => fileSize = value;
        }
        public string FileSizeText {
            get
            {
                if (fileSizeText is null)
                {
                    CacheDetails();
                }
                return fileSizeText;
            }
            set => fileSizeText = value;
        }
        public string Attributes {
            get
            {
                if (attributes is null)
                {
                    var fld = Folder;
                    attributes = "";
                    if (File != null)
                    {
                        if (File is RpfResourceFileEntry resf)
                        {
                            attributes += "Resource [V." + resf.Version.ToString() + "]";
                        }
                        if (File.IsEncrypted)
                        {
                            if (Attributes.Length > 0)
                            {
                                attributes += ", ";
                            }
                            attributes += "Encrypted";
                        }
                    }
                    else if (fld != null)
                    {
                        if (fld.RpfFile != null)
                        {
                            attributes += fld.RpfFile.Encryption.ToString() + " encryption";
                        }
                    }
                }
                return attributes;
            }
            set => attributes = value;
        }
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
            Name = System.IO.Path.GetFileName(file);
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

        public void CacheDetails()
        {
            var fld = Folder;
            var fileType = FileType;
            fileTypeText = fileType.Name;
            ImageIndex = fileType.ImageIndex;
            if (File != null)
            {
                fileSize = File.GetFileSize();
                fileSizeText = TextUtil.GetBytesReadable(FileSize);
            }
            else if (fld != null)
            {
                if (fld.RpfFile != null)
                {
                    fileSize = fld.RpfFile.FileSize;
                    fileSizeText = TextUtil.GetBytesReadable(FileSize);
                }
                else
                {
                    fileTypeText = "Folder";
                    ImageIndex = 1; //FOLDER imageIndex
                    var ic = fld.GetItemCount();
                    fileSize = ic;
                    fileSizeText = ic.ToString() + " item" + ((ic != 1) ? "s" : "");
                }
            }
            else
            {
                var fi = new FileInfo(FullPath);
                if (fi.Exists)
                {
                    fileSize = fi.Length;
                    fileSizeText = TextUtil.GetBytesReadable(fi.Length);
                }
            }
            fileSizeText ??= "";
            fileTypeText ??= "";
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
                    return StringComparer.OrdinalIgnoreCase.Compare(i1.Name, i2.Name);
                case 1: //Type column
                    var ftc = StringComparer.OrdinalIgnoreCase.Compare(i1.FileTypeText, i2.FileTypeText);
                    if (ftc == 0) return StringComparer.OrdinalIgnoreCase.Compare(i1.Name, i2.Name); //same type, sort by name...
                    return ftc;
                case 2: //Size column
                    return StringComparer.OrdinalIgnoreCase.Compare(i1.FileSize, i2.FileSize);
                case 3: //Attributes column
                    var ac = StringComparer.OrdinalIgnoreCase.Compare(i1.Attributes, i2.Attributes);
                    if (ac == 0) return StringComparer.OrdinalIgnoreCase.Compare(i1.Name, i2.Name); //same attributes, sort by name...
                    return ac;
                case 4: //path column
                    return StringComparer.OrdinalIgnoreCase.Compare(i1.Path, i2.Path);
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
        public bool XmlConvertible { get; set; }

        public FileTypeInfo(string extension, string name, int imageindex, FileTypeAction defaultAction, bool xmlConvertible)
        {
            Name = name;
            Extension = extension;
            ImageIndex = imageindex;
            DefaultAction = defaultAction;
            XmlConvertible = xmlConvertible;
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
        ViewYnd = 18,
        ViewCacheDat = 19,
        ViewYed = 20,
        ViewYld = 21,
        ViewYfd = 22,
        ViewHeightmap = 23,
        ViewMrf = 24,
        ViewNametable = 25,
        ViewDistantLights = 26,
        ViewYpdb = 27,
    }









}
