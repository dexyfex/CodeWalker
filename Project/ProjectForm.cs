using CodeWalker.GameFiles;
using CodeWalker.Project.Panels;
using CodeWalker.Properties;
using CodeWalker.World;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.Project
{
    public partial class ProjectForm : Form
    {
        public WorldForm WorldForm { get; private set; }
        public ThemeBase Theme { get; private set; }
        public ProjectExplorerPanel ProjectExplorer { get; set; }
        public ProjectPanel PreviewPanel { get; set; }


        public GameFileCache GameFileCache { get; private set; }
        public RpfManager RpfMan { get; private set; }


        public bool IsProjectLoaded
        {
            get { return CurrentProjectFile != null; }
        }
        public ProjectFile CurrentProjectFile;

        private YmapFile CurrentYmapFile;
        private YmapEntityDef CurrentEntity;
        private YmapCarGen CurrentCarGen;
        private YmapGrassInstanceBatch CurrentGrassBatch;

        private YtypFile CurrentYtypFile;
        //private Archetype CurrentArchetype;

        private YndFile CurrentYndFile;
        private YndNode CurrentPathNode;
        private YndLink CurrentPathLink;

        private YnvFile CurrentYnvFile;
        private YnvPoly CurrentNavPoly;

        private TrainTrack CurrentTrainTrack;
        private TrainTrackNode CurrentTrainNode;

        private YmtFile CurrentScenario;
        private ScenarioNode CurrentScenarioNode;
        private MCScenarioChainingEdge CurrentScenarioChainEdge;

        private bool renderitems = true;
        private bool hidegtavmap = false;

        private object projectsyncroot = new object();
        public object ProjectSyncRoot { get { return projectsyncroot; } }

        private Dictionary<int, YndFile> visibleynds = new Dictionary<int, YndFile>();
        private Dictionary<int, YnvFile> visibleynvs = new Dictionary<int, YnvFile>();
        private Dictionary<string, TrainTrack> visibletrains = new Dictionary<string, TrainTrack>();
        private Dictionary<string, YmtFile> visiblescenarios = new Dictionary<string, YmtFile>();

        private bool ShowProjectItemInProcess = false;


        public ProjectForm(WorldForm worldForm = null)
        {
            WorldForm = worldForm;

            InitializeComponent();

            SetTheme(Settings.Default.ProjectWindowTheme, false);
            ShowDefaultPanels();


            if ((WorldForm != null) && (WorldForm.GameFileCache != null))
            {
                GameFileCache = WorldForm.GameFileCache;
                RpfMan = GameFileCache.RpfMan;
            }
            else
            {
                GameFileCache = GameFileCacheFactory.Create();
                new Thread(new ThreadStart(() => {
                    GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
                    GameFileCache.Init(UpdateStatus, UpdateError);
                    RpfMan = GameFileCache.RpfMan;
                })).Start();
            }

        }

        private void UpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateStatus(text); }));
                }
                else
                {
                    //TODO: status text
                    //StatusLabel.Text = text;
                }
            }
            catch { }
        }
        private void UpdateError(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateError(text); }));
                }
                else
                {
                    //TODO: error text
                    //ErrorLabel.Text = text;
                }
            }
            catch { }
        }


        private void SetTheme(string themestr, bool changing = true)
        {
            if (changing && (CurrentProjectFile != null))
            {
                if (MessageBox.Show("Project will be closed before changing the theme. Are you sure you want to continue?", "Theme change", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }


            CloseProject();

            //string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DockPanel.temp.config");
            //MainDockPanel.SaveAsXml(configFile);
            CloseAllContents();

            ProjectExplorer = null;
            PreviewPanel = null;


            foreach (ToolStripMenuItem menu in ViewThemeMenu.DropDownItems)
            {
                menu.Checked = false;
            }

            Theme = null;

            switch (themestr)
            {
                default:
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
                Settings.Default.ProjectWindowTheme = themestr;
                Settings.Default.Save();
            }


            Theme.Extender.FloatWindowFactory = new ProjectFloatWindowFactory();

            MainDockPanel.Theme = Theme;

            var version = VisualStudioToolStripExtender.VsVersion.Vs2015;
            VSExtender.SetStyle(MainMenu, version, Theme);
            VSExtender.SetStyle(MainToolbar, version, Theme);
            //VSExtender.SetStyle(MainStatusBar, version, theme);


            //if (File.Exists(configFile)) MainDockPanel.LoadFromXml(configFile, m_deserializeDockContent);


            if (changing)
            {
                ShowDefaultPanels();
            }


        }

        private T FindPanel<T>(Func<T, bool> findFunc) where T : ProjectPanel
        {
            foreach (var pane in MainDockPanel.Panes)
            {
                foreach (var content in pane.Contents)
                {
                    var test = content as T;
                    if ((test != null) && findFunc(test))
                    {
                        return test;
                    }
                }
            }
            return null;
        }
        private void ShowDefaultPanels()
        {
            ShowProjectExplorer();
            ShowWelcomePanel();
        }
        private void ShowProjectExplorer()
        {
            if ((ProjectExplorer == null) || (ProjectExplorer.IsDisposed) || (ProjectExplorer.Disposing))
            {
                ProjectExplorer = new ProjectExplorerPanel(this);
                ProjectExplorer.OnItemSelected += ProjectExplorer_OnItemSelected;
                ProjectExplorer.OnItemActivated += ProjectExplorer_OnItemActivated;
                ProjectExplorer.SetTheme(Theme);
                ProjectExplorer.Show(MainDockPanel, DockState.DockLeft);
            }
            else
            {
                ProjectExplorer.Show();
            }
        }
        private void ShowWelcomePanel()
        {
            ShowPreviewPanel(() => { return new WelcomePanel(); });
        }
        private void ShowPreviewPanel<T>(Func<T> createFunc, Action<T> updateAction = null) where T : ProjectPanel
        {
            if ((PreviewPanel != null) && (PreviewPanel is T))
            {
                PreviewPanel.BringToFront();//.Show();
                updateAction?.Invoke(PreviewPanel as T);
            }
            else
            {
                var panel = createFunc();
                panel.HideOnClose = true;
                panel.SetTheme(Theme);
                panel.Show(MainDockPanel, DockState.Document);
                updateAction?.Invoke(panel);
                if (PreviewPanel != null)
                {
                    PreviewPanel.Close();
                }
                PreviewPanel = panel;
            }
        }
        private void ShowPanel<T>(bool promote, Func<T> createFunc, Action<T> updateAction, Func<T,bool> findFunc) where T : ProjectPanel
        {
            T found = FindPanel(findFunc);
            if ((found != null) && (found != PreviewPanel))
            {
                found.BringToFront();//.Show();
                updateAction?.Invoke(found);
            }
            else
            {
                if (promote)
                {
                    PromoteIfPreviewPanel(PreviewPanel);
                    if (found != null)
                    {
                        found.BringToFront();//.Show();
                        updateAction?.Invoke(found);
                    }
                    else
                    {
                        ShowPreviewPanel(createFunc, updateAction);
                        PreviewPanel = null;
                    }
                }
                else
                {
                    ShowPreviewPanel(createFunc, updateAction);
                }
            }
        }
        private void ShowEditProjectPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditProjectPanel(this); }, //createFunc
                (panel) => { panel.SetProject(CurrentProjectFile); },  //updateFunc
                (panel) => { return true; }); //findFunc
        }
        private void ShowEditProjectManifestPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditProjectManifestPanel(this); }, //createFunc
                (panel) => { panel.SetProject(CurrentProjectFile); }, //updateFunc
                (panel) => { return true; }); //findFunc
        }
        private void ShowEditYmapPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapPanel(this); }, //createFunc
                (panel) => { panel.SetYmap(CurrentYmapFile); }, //updateFunc
                (panel) => { return panel.Ymap == CurrentYmapFile; }); //findFunc
        }
        private void ShowEditYmapEntityPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapEntityPanel(this); }, //createFunc
                (panel) => { panel.SetEntity(CurrentEntity); }, //updateFunc
                (panel) => { return panel.CurrentEntity == CurrentEntity; }); //findFunc
        }
        private void ShowEditYmapCarGenPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapCarGenPanel(this); }, //createFunc
                (panel) => { panel.SetCarGen(CurrentCarGen); }, //updateFunc
                (panel) => { return panel.CurrentCarGen == CurrentCarGen; }); //findFunc
        }
        private void ShowEditYmapGrassBatchPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapGrassPanel(this); }, //createFunc
                (panel) => { panel.SetBatch(CurrentGrassBatch); }, //updateFunc
                (panel) => { return panel.CurrentBatch == CurrentGrassBatch; }); //findFunc
        }
        private void ShowEditYtypPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYtypPanel(this); }, //createFunc
                (panel) => { panel.SetYtyp(CurrentYtypFile); }, //updateFunc
                (panel) => { return panel.Ytyp == CurrentYtypFile; }); //findFunc
        }
        private void ShowEditYndPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYndPanel(this); }, //createFunc
                (panel) => { panel.SetYnd(CurrentYndFile); }, //updateFunc
                (panel) => { return panel.Ynd == CurrentYndFile; }); //findFunc
        }
        private void ShowEditYndNodePanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYndNodePanel(this); }, //createFunc
                (panel) => { panel.SetPathNode(CurrentPathNode); }, //updateFunc
                (panel) => { return panel.CurrentPathNode == CurrentPathNode; }); //findFunc
        }
        private void ShowEditYnvPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYnvPanel(this); }, //createFunc
                (panel) => { panel.SetYnv(CurrentYnvFile); }, //updateFunc
                (panel) => { return panel.Ynv == CurrentYnvFile; }); //findFunc
        }
        private void ShowEditYnvPolyPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYnvPolyPanel(this); }, //createFunc
                (panel) => { panel.SetYnvPoly(CurrentNavPoly); }, //updateFunc
                (panel) => { return panel.YnvPoly == CurrentNavPoly; }); //findFunc
        }
        private void ShowEditTrainTrackPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditTrainTrackPanel(this); }, //createFunc
                (panel) => { panel.SetTrainTrack(CurrentTrainTrack); }, //updateFunc
                (panel) => { return panel.Track == CurrentTrainTrack; }); //findFunc
        }
        private void ShowEditTrainNodePanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditTrainNodePanel(this); }, //createFunc
                (panel) => { panel.SetTrainNode(CurrentTrainNode); }, //updateFunc
                (panel) => { return panel.TrainNode == CurrentTrainNode; }); //findFunc
        }
        private void ShowEditScenarioYmtPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditScenarioYmtPanel(this); }, //createFunc
                (panel) => { panel.SetScenarioYmt(CurrentScenario); }, //updateFunc
                (panel) => { return panel.CurrentScenario == CurrentScenario; }); //findFunc
        }
        private void ShowEditScenarioNodePanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditScenarioNodePanel(this); }, //createFunc
                (panel) => { panel.SetScenarioNode(CurrentScenarioNode); }, //updateFunc
                (panel) => { return panel.CurrentScenarioNode == CurrentScenarioNode; }); //findFunc
        }

        private void ShowCurrentProjectItem(bool promote)
        {
            if (CurrentEntity != null)
            {
                ShowEditYmapEntityPanel(promote);
            }
            else if (CurrentCarGen != null)
            {
                ShowEditYmapCarGenPanel(promote);
            }
            else if (CurrentGrassBatch != null)
            {
                ShowEditYmapGrassBatchPanel(promote);
            }
            else if (CurrentYmapFile != null)
            {
                ShowEditYmapPanel(promote);
            }
            if (CurrentYtypFile != null)
            {
                ShowEditYtypPanel(promote);
            }
            if (CurrentPathNode != null)
            {
                ShowEditYndNodePanel(promote);
            }
            else if (CurrentYndFile != null)
            {
                ShowEditYndPanel(promote);
            }
            if (CurrentNavPoly != null)
            {
                ShowEditYnvPolyPanel(promote);
            }
            else if (CurrentYnvFile != null)
            {
                ShowEditYnvPanel(promote);
            }
            if (CurrentTrainNode != null)
            {
                ShowEditTrainNodePanel(promote);
            }
            else if (CurrentTrainTrack != null)
            {
                ShowEditTrainTrackPanel(promote);
            }
            if (CurrentScenarioNode != null)
            {
                ShowEditScenarioNodePanel(promote);
            }
            else if (CurrentScenario != null)
            {
                ShowEditScenarioYmtPanel(promote);
            }
        }
        public void ShowProjectItem(object item, bool promote)
        {
            ShowProjectItemInProcess = true;

            SetProjectItem(item);

            if (item == CurrentProjectFile)
            {
                ShowEditProjectPanel(promote);
            }
            else
            {
                ShowCurrentProjectItem(promote);
            }

            ShowProjectItemInProcess = false;
        }
        public void SetProjectItem(object item)
        {
            CurrentYmapFile = item as YmapFile;
            CurrentEntity = item as YmapEntityDef;
            CurrentCarGen = item as YmapCarGen;
            CurrentGrassBatch = item as YmapGrassInstanceBatch;
            CurrentYtypFile = item as YtypFile;
            CurrentYndFile = item as YndFile;
            CurrentPathNode = item as YndNode;
            CurrentYnvFile = item as YnvFile;
            CurrentNavPoly = item as YnvPoly;
            CurrentTrainTrack = item as TrainTrack;
            CurrentTrainNode = item as TrainTrackNode;
            CurrentScenario = item as YmtFile;
            CurrentScenarioNode = item as ScenarioNode;
            CurrentScenarioChainEdge = item as MCScenarioChainingEdge;

            if (CurrentEntity != null)
            {
                CurrentYmapFile = CurrentEntity.Ymap;
            }
            else if (CurrentCarGen != null)
            {
                CurrentYmapFile = CurrentCarGen.Ymap;
            }
            else if (CurrentGrassBatch != null)
            {
                CurrentYmapFile = CurrentGrassBatch.Ymap;
            }
            if (CurrentPathNode != null)
            {
                CurrentYndFile = CurrentPathNode.Ynd;
            }
            if (CurrentNavPoly != null)
            {
                CurrentYnvFile = CurrentNavPoly.Ynv;
            }
            if (CurrentTrainNode != null)
            {
                CurrentTrainTrack = CurrentTrainNode.Track;
            }
            if ((CurrentScenario != null) && (CurrentScenario.ScenarioRegion == null))
            {
                CurrentScenario = null;//incase other types of ymt files make it into the project...
            }
            if (CurrentScenarioNode != null)
            {
                CurrentScenario = CurrentScenarioNode.Ymt;
            }
            if (CurrentScenarioChainEdge != null)
            {
                CurrentScenario = CurrentScenarioChainEdge.Region?.Ymt;
            }

            RefreshUI();

        }

        private void ClosePanel<T>(Func<T,bool> findFunc) where T : ProjectPanel
        {
            var panel = FindPanel(findFunc);
            if (PreviewPanel == panel)
            {
                PreviewPanel = null;
            }
            panel?.Close();
        }
        private void CloseAllDocuments()
        {
            if (MainDockPanel.DocumentStyle == DocumentStyle.SystemMdi)
            {
                foreach (Form form in MdiChildren) form.Close();
            }
            else
            {
                foreach (IDockContent document in MainDockPanel.DocumentsToArray())
                {
                    // IMPORANT: dispose all panes.
                    document.DockHandler.DockPanel = null;
                    document.DockHandler.Close();
                }
            }
        }
        private void CloseAllContents()
        {
            // we don't want to create another instance of tool window, set DockPanel to null
            if (ProjectExplorer != null) ProjectExplorer.DockPanel = null;
            if (PreviewPanel != null) PreviewPanel.DockPanel = null;

            // Close all other document windows
            CloseAllDocuments();

            // IMPORTANT: dispose all float windows.
            foreach (var window in MainDockPanel.FloatWindows.ToList()) window.Dispose();

            System.Diagnostics.Debug.Assert(MainDockPanel.Panes.Count == 0);
            System.Diagnostics.Debug.Assert(MainDockPanel.Contents.Count == 0);
            System.Diagnostics.Debug.Assert(MainDockPanel.FloatWindows.Count == 0);
        }
        private void CloseAllProjectItems()
        {
            foreach (var pane in MainDockPanel.Panes.ToArray())
            {
                foreach (var content in pane.Contents.ToArray())
                {
                    var panel = content as ProjectPanel;
                    if (panel?.Tag != null)
                    {
                        panel.Close();
                    }
                }
            }
        }

        private void PromoteIfPreviewPanel(IDockContent panel)
        {
            if (panel == PreviewPanel)
            {
                if (PreviewPanel != null)
                {
                    PreviewPanel.HideOnClose = false;
                }
                PreviewPanel = null;
            }
        }
        private void PromoteIfPreviewPanelActive()
        {
            PromoteIfPreviewPanel(MainDockPanel.ActiveContent);
        }



        //######## Public methods

        public void NewProject()
        {
            if (CurrentProjectFile != null)
            {
                ////unload current project first
                CloseProject();
            }

            CurrentProjectFile = new ProjectFile();
            CurrentProjectFile.Name = "New CodeWalker Project";
            CurrentProjectFile.Version = 1;
            CurrentProjectFile.HasChanged = true;
            LoadProjectUI();
        }
        public void OpenProject()
        {
            string file = ShowOpenDialog("CodeWalker Projects|*.cwproj", string.Empty);
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            CloseProject();

            CurrentProjectFile = new ProjectFile();
            CurrentProjectFile.Load(file);

            string cpath = new FileInfo(CurrentProjectFile.Filepath).Directory.FullName;

            foreach (var ymap in CurrentProjectFile.YmapFiles)
            {
                string filename = ymap.FilePath;
                if (!File.Exists(filename))
                {
                    filename = cpath + "\\" + filename;
                }
                if (File.Exists(filename))
                {
                    LoadYmapFromFile(ymap, filename);
                }
                else
                {
                    MessageBox.Show("Couldn't find file: " + filename);
                }
            }

            foreach (var ytyp in CurrentProjectFile.YtypFiles)
            {
                string filename = ytyp.FilePath;
                if (!File.Exists(filename))
                {
                    filename = cpath + "\\" + filename;
                }
                if (File.Exists(filename))
                {
                    LoadYtypFromFile(ytyp, filename);
                }
                else
                {
                    MessageBox.Show("Couldn't find file: " + filename);
                }
            }

            foreach (var ynd in CurrentProjectFile.YndFiles)
            {
                string filename = ynd.FilePath;
                if (!File.Exists(filename))
                {
                    filename = cpath + "\\" + filename;
                }
                if (File.Exists(filename))
                {
                    LoadYndFromFile(ynd, filename);
                }
                else
                {
                    MessageBox.Show("Couldn't find file: " + filename);
                }
            }

            foreach (var ynv in CurrentProjectFile.YnvFiles)
            {
                string filename = ynv.FilePath;
                if (!File.Exists(filename))
                {
                    filename = cpath + "\\" + filename;
                }
                if (File.Exists(filename))
                {
                    LoadYnvFromFile(ynv, filename);
                }
                else
                {
                    MessageBox.Show("Couldn't find file: " + filename);
                }
            }

            foreach (var track in CurrentProjectFile.TrainsFiles)
            {
                string filename = track.FilePath;
                if (!File.Exists(filename))
                {
                    filename = cpath + "\\" + filename;
                }
                if (File.Exists(filename))
                {
                    LoadTrainTrackFromFile(track, filename);
                }
                else
                {
                    MessageBox.Show("Couldn't find file: " + filename);
                }
            }

            foreach (var scenario in CurrentProjectFile.ScenarioFiles)
            {
                string filename = scenario.FilePath;
                if (!File.Exists(filename))
                {
                    filename = cpath + "\\" + filename;
                }
                if (File.Exists(filename))
                {
                    LoadScenarioFromFile(scenario, filename);
                }
                else
                {
                    MessageBox.Show("Couldn't find file: " + filename);
                }
            }


            LoadProjectUI();
        }
        public void CloseProject()
        {
            if (CurrentProjectFile == null) return;

            foreach (var ymap in CurrentProjectFile.YmapFiles)
            {
                if ((ymap != null) && (ymap.HasChanged))
                {
                    //save the current ymap first?
                    if (MessageBox.Show("Would you like to save " + ymap.Name + " before closing?", "Save .ymap before closing?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CurrentYmapFile = ymap;
                        SaveYmap();
                    }
                }
            }

            foreach (var ytyp in CurrentProjectFile.YtypFiles)
            {
                if ((ytyp != null) && (ytyp.HasChanged))
                {
                    //save the current ytyp first?
                    if (MessageBox.Show("Would you like to save " + ytyp.Name + " before closing?", "Save .ytyp before closing?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CurrentYtypFile = ytyp;
                        SaveYtyp();
                    }
                }
            }

            foreach (var ynd in CurrentProjectFile.YndFiles)
            {
                if ((ynd != null) && (ynd.HasChanged))
                {
                    //save the current ynd first?
                    if (MessageBox.Show("Would you like to save " + ynd.Name + " before closing?", "Save .ynd before closing?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CurrentYndFile = ynd;
                        SaveYnd();
                    }
                }
            }

            foreach (var ynv in CurrentProjectFile.YnvFiles)
            {
                if ((ynv != null) && (ynv.HasChanged))
                {
                    //save the current ynv first?
                    if (MessageBox.Show("Would you like to save " + ynv.Name + " before closing?", "Save .ynv before closing?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CurrentYnvFile = ynv;
                        SaveYnv();
                    }
                }
            }

            foreach (var trains in CurrentProjectFile.TrainsFiles)
            {
                if ((trains != null) && (trains.HasChanged))
                {
                    //save the current trains file first?
                    if (MessageBox.Show("Would you like to save " + trains.Name + " before closing?", "Save trains file before closing?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CurrentTrainTrack = trains;
                        SaveTrainTrack();
                    }
                }
            }

            foreach (var scenario in CurrentProjectFile.ScenarioFiles)
            {
                if ((scenario != null) && (scenario.HasChanged))
                {
                    //save the current scenario file first?
                    if (MessageBox.Show("Would you like to save " + scenario.Name + " before closing?", "Save scenario file before closing?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CurrentScenario = scenario;
                        SaveScenario();
                    }
                }
            }


            if (CurrentProjectFile.HasChanged)
            {
                //save the current project first?
                if (MessageBox.Show("Would you like to save the current project before closing?", "Save project before closing?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveProject();
                }
            }

            CloseAllProjectItems();

            CurrentProjectFile = null;
            CurrentYmapFile = null;
            CurrentYtypFile = null;
            CurrentYndFile = null;
            CurrentYnvFile = null;
            CurrentTrainTrack = null;
            CurrentScenario = null;

            LoadProjectUI();


            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);//make sure current selected item isn't still selected...
            }
        }
        public void SaveProject(bool saveas = false)
        {
            if (CurrentProjectFile == null) return;
            if (string.IsNullOrEmpty(CurrentProjectFile.Filepath) || saveas)
            {
                string fileName = ShowSaveDialog("CodeWalker Projects|*.cwproj", CurrentProjectFile.Filepath);
                if (string.IsNullOrEmpty(fileName))
                { return; } //user cancelled

                string oldpath = CurrentProjectFile.Filepath;
                CurrentProjectFile.Filepath = fileName;
                CurrentProjectFile.Filename = new FileInfo(fileName).Name;
                CurrentProjectFile.UpdateFilenames(oldpath);
            }

            CurrentProjectFile.Save();

            SetProjectHasChanged(false);
        }

        public void Save()
        {
            if (CurrentYmapFile != null)
            {
                SaveYmap();
            }
            else if (CurrentYtypFile != null)
            {
                SaveYtyp();
            }
            else if (CurrentYndFile != null)
            {
                SaveYnd();
            }
            else if (CurrentYnvFile != null)
            {
                SaveYnv();
            }
            else if (CurrentTrainTrack != null)
            {
                SaveTrainTrack();
            }
            else if (CurrentScenario != null)
            {
                SaveScenario();
            }
            else if (CurrentProjectFile != null)
            {
                SaveProject();
            }
        }
        public void SaveAll()
        {
            if (CurrentProjectFile != null)
            {
                if (CurrentProjectFile.YmapFiles != null)
                {
                    var cymap = CurrentYmapFile;
                    foreach (var ymap in CurrentProjectFile.YmapFiles)
                    {
                        CurrentYmapFile = ymap;
                        SaveYmap();
                    }
                    CurrentYmapFile = cymap;
                    //ShowEditYmapPanel(false);
                }

                if (CurrentProjectFile.YtypFiles != null)
                {
                    var cytyp = CurrentYtypFile;
                    foreach (var ytyp in CurrentProjectFile.YtypFiles)
                    {
                        CurrentYtypFile = ytyp;
                        SaveYtyp();
                    }
                    CurrentYtypFile = cytyp;
                    //ShowEditYtypPanel(false);
                }

                if (CurrentProjectFile.YndFiles != null)
                {
                    var cynd = CurrentYndFile;
                    foreach (var ynd in CurrentProjectFile.YndFiles)
                    {
                        CurrentYndFile = ynd;
                        SaveYnd();
                    }
                    CurrentYndFile = cynd;
                    //ShowEditYndPanel(false);
                }

                if (CurrentProjectFile.YnvFiles != null)
                {
                    var cynv = CurrentYnvFile;
                    foreach (var ynv in CurrentProjectFile.YnvFiles)
                    {
                        CurrentYnvFile = ynv;
                        SaveYnv();
                    }
                    CurrentYnvFile = cynv;
                    //ShowEditYnvPanel(false);
                }

                if (CurrentProjectFile.TrainsFiles != null)
                {
                    var ctrack = CurrentTrainTrack;
                    foreach (var track in CurrentProjectFile.TrainsFiles)
                    {
                        CurrentTrainTrack = track;
                        SaveYnd();
                    }
                    CurrentTrainTrack = ctrack;
                    //ShowEditTrainTrackPanel(false);
                }

                if (CurrentProjectFile.ScenarioFiles != null)
                {
                    var cscen = CurrentScenario;
                    foreach (var scen in CurrentProjectFile.ScenarioFiles)
                    {
                        CurrentScenario = scen;
                        SaveScenario();
                    }
                    CurrentScenario = cscen;
                    //ShowEditScenarioPanel(false);
                }


                SaveProject();
            }
        }
        public void SaveCurrentItem(bool saveas = false)
        {
            if (CurrentYmapFile != null)
            {
                SaveYmap(saveas);
            }
            else if (CurrentYtypFile != null)
            {
                SaveYtyp(saveas);
            }
            else if (CurrentYndFile != null)
            {
                SaveYnd(saveas);
            }
            else if (CurrentYnvFile != null)
            {
                SaveYnv(saveas);
            }
            else if (CurrentTrainTrack != null)
            {
                SaveTrainTrack(saveas);
            }
            else if (CurrentScenario != null)
            {
                SaveScenario(saveas);
            }
        }



        public void NewYmap()
        {
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (CurrentProjectFile == null) return;

            int testi = 1;
            string fname = string.Empty;
            bool filenameok = false;
            while (!filenameok)
            {
                fname = "map" + testi.ToString() + ".ymap";
                filenameok = !CurrentProjectFile.ContainsYmap(fname);
                testi++;
            }

            lock (projectsyncroot)
            {
                YmapFile ymap = CurrentProjectFile.AddYmapFile(fname);
                if (ymap != null)
                {
                    ymap.Loaded = true;
                    ymap.HasChanged = true; //new ymap, flag as not saved
                    ymap._CMapData.contentFlags = 65; //stream flags value
                }
            }

            CurrentProjectFile.HasChanged = true;

            LoadProjectTree();
        }
        public void OpenYmap()
        {
            string[] files = ShowOpenDialogMulti("Ymap files|*.ymap", string.Empty);
            if (files == null)
            {
                return;
            }

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            foreach (string file in files)
            {
                if (!File.Exists(file)) continue;

                var ymap = CurrentProjectFile.AddYmapFile(file);

                if (ymap != null)
                {
                    SetProjectHasChanged(true);

                    LoadYmapFromFile(ymap, file);

                    LoadProjectTree();
                }
                else
                {
                    MessageBox.Show("Couldn't add\n" + file + "\n - the file already exists in the project.");
                }

            }
        }
        public void SaveYmap(bool saveas = false)
        {
            if (CurrentYmapFile == null) return;
            string ymapname = CurrentYmapFile.Name;
            string filepath = CurrentYmapFile.FilePath;
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = ymapname;
            }
            string origfile = filepath;
            if (!File.Exists(filepath))
            {
                saveas = true;
            }


            byte[] data;
            lock (projectsyncroot) //need to sync writes to ymap objects...
            {
                saveas = saveas || string.IsNullOrEmpty(filepath);
                if (saveas)
                {
                    filepath = ShowSaveDialog("Ymap files|*.ymap", filepath);
                    if (string.IsNullOrEmpty(filepath))
                    { return; }

                    string newname = Path.GetFileNameWithoutExtension(filepath);
                    JenkIndex.Ensure(newname);
                    CurrentYmapFile.FilePath = filepath;
                    CurrentYmapFile.RpfFileEntry.Name = new FileInfo(filepath).Name;
                    CurrentYmapFile.Name = CurrentYmapFile.RpfFileEntry.Name;
                    CurrentYmapFile._CMapData.name = new MetaHash(JenkHash.GenHash(newname));
                }

                data = CurrentYmapFile.Save();
            }


            if (data != null)
            {
                File.WriteAllBytes(filepath, data);
            }

            SetYmapHasChanged(false);

            if (saveas)
            {
                //ShowEditYmapPanel(false);
                if (CurrentProjectFile != null)
                {
                    string origpath = CurrentProjectFile.GetRelativePath(origfile);
                    string newpath = CurrentProjectFile.GetRelativePath(CurrentYmapFile.FilePath);

                    if (!CurrentProjectFile.RenameYmap(origpath, newpath))
                    { //couldn't rename it in the project?
                        MessageBox.Show("Couldn't rename ymap in project! This shouldn't happen - check the project file XML.");
                    }
                }
                SetProjectHasChanged(true);
                SetCurrentSaveItem();
            }

            if (CurrentYmapFile.SaveWarnings != null)
            {
                string w = string.Join("\n", CurrentYmapFile.SaveWarnings);
                MessageBox.Show(CurrentYmapFile.SaveWarnings.Count.ToString() + " warnings were generated while saving the ymap:\n" + w);
                CurrentYmapFile.SaveWarnings = null;//clear it out for next time..
            }
        }
        public void AddYmapToProject(YmapFile ymap)
        {
            if (ymap == null) return;
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (YmapExistsInProject(ymap)) return;
            if (CurrentProjectFile.AddYmapFile(ymap))
            {
                ymap.HasChanged = true;
                CurrentProjectFile.HasChanged = true;
                LoadProjectTree();
            }
            CurrentYmapFile = ymap;
            RefreshUI();
            if (CurrentEntity != null)
            {
                ProjectExplorer?.TrySelectEntityTreeNode(CurrentEntity);
            }
            else if (CurrentCarGen != null)
            {
                ProjectExplorer?.TrySelectCarGenTreeNode(CurrentCarGen);
            }
        }
        public void RemoveYmapFromProject()
        {
            if (CurrentYmapFile == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveYmapFile(CurrentYmapFile);
            CurrentYmapFile = null;
            LoadProjectTree();
            RefreshUI();
        }
        public bool YmapExistsInProject(YmapFile ymap)
        {
            if (ymap == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsYmap(ymap);
        }

        public void NewEntity(YmapEntityDef copy = null, bool copyPosition = false)
        {
            if (CurrentYmapFile == null) return;

            float spawndist = 5.0f; //use archetype BSradius if starting with a copy...
            if (copy != null)
            {
                spawndist = copy.BSRadius * 2.5f;
            }
            bool cp = copyPosition && (copy != null);
            Vector3 pos = cp ? copy.Position : GetSpawnPos(spawndist);


            CEntityDef cent = new CEntityDef();

            if (copy != null)
            {
                cent = copy.CEntityDef;
                //TODO: copy entity extensions!
            }
            else
            {
                cent.archetypeName = new MetaHash(JenkHash.GenHash("prop_alien_egg_01"));
                cent.rotation = new Vector4(0, 0, 0, 1);
                cent.scaleXY = 1.0f;
                cent.scaleZ = 1.0f;
                cent.flags = 1572872;
                cent.parentIndex = -1;
                cent.lodDist = 200.0f;
                cent.lodLevel = Unk_1264241711.LODTYPES_DEPTH_ORPHANHD;
                cent.priorityLevel = Unk_648413703.PRI_REQUIRED;
                cent.ambientOcclusionMultiplier = 255;
                cent.artificialAmbientOcclusion = 255;
            }

            cent.position = pos;


            YmapEntityDef ent = new YmapEntityDef(CurrentYmapFile, 0, ref cent);

            ent.SetArchetype(GameFileCache.GetArchetype(cent.archetypeName));

            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    CurrentYmapFile.AddEntity(ent);
                }
            }
            else
            {
                CurrentYmapFile.AddEntity(ent);
            }


            LoadProjectTree();

            ProjectExplorer?.TrySelectEntityTreeNode(ent);
            CurrentEntity = ent;
            ShowEditYmapEntityPanel(false);
        }
        public void AddEntityToProject()
        {
            if (CurrentEntity == null) return;

            if (CurrentEntity.Ymap == null)
            {
                MessageBox.Show("Sorry, interior entities cannot currently be added to the project.");
                return;
            }

            CurrentYmapFile = CurrentEntity.Ymap;
            if (!YmapExistsInProject(CurrentYmapFile))
            {
                var ent = CurrentEntity;
                CurrentYmapFile.HasChanged = true;
                AddYmapToProject(CurrentYmapFile);

                CurrentEntity = ent; //bug fix for some reason the treeview selects the project node here.
                CurrentYmapFile = ent.Ymap;
                ProjectExplorer?.TrySelectEntityTreeNode(ent);
            }
        }
        public bool DeleteEntity()
        {
            if (CurrentYmapFile == null) return false;
            if (CurrentEntity == null) return false;
            if (CurrentEntity.Ymap != CurrentYmapFile) return false;
            if (CurrentYmapFile.AllEntities == null) return false; //nothing to delete..
            if (CurrentYmapFile.RootEntities == null) return false; //nothing to delete..

            if (CurrentEntity._CEntityDef.numChildren != 0)
            {
                MessageBox.Show("This entity's numChildren is not 0 - deleting entities with children is not currently supported by CodeWalker.");
                return true;
            }

            int idx = CurrentEntity.Index;
            for (int i = idx + 1; i < CurrentYmapFile.AllEntities.Length; i++)
            {
                var ent = CurrentYmapFile.AllEntities[i];
                if (ent._CEntityDef.numChildren != 0)
                {
                    MessageBox.Show("There are other entities present in this .ymap that have children. Deleting this entity is not currently supported by CodeWalker.");
                    return true;
                }
            }

            if (MessageBox.Show("Are you sure you want to delete this entity?\n" + CurrentEntity._CEntityDef.archetypeName.ToString() + "\n" + CurrentEntity.Position.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentYmapFile.RemoveEntity(CurrentEntity);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentYmapFile.RemoveEntity(CurrentEntity);
            }
            if (!res)
            {
                MessageBox.Show("Entity.Index didn't match the index of the entity in the ymap. This shouldn't happen, check LOD linkages!");
            }

            var delent = CurrentEntity;
            var delymap = CurrentYmapFile;

            ProjectExplorer?.RemoveEntityTreeNode(delent);
            ProjectExplorer?.SetYmapHasChanged(delymap, true);

            ClosePanel((EditYmapEntityPanel p) => { return p.Tag == delent; });

            CurrentEntity = null;

            return true;
        }
        public bool IsCurrentEntity(YmapEntityDef ent)
        {
            return CurrentEntity == ent;
        }

        public void NewCarGen(YmapCarGen copy = null, bool copyPosition = false)
        {
            if (CurrentYmapFile == null) return;

            Vector3 pos = GetSpawnPos(10.0f);

            CCarGen ccg = new CCarGen();

            if (copy != null)
            {
                ccg = copy.CCarGen;
            }
            else
            {
                ccg.flags = 3680;
                ccg.orientX = 5.0f;
                ccg.perpendicularLength = 2.6f;
                //TODO: set default values for cargen
            }

            if (!copyPosition || (copy == null))
            {
                ccg.position = pos;
            }


            YmapCarGen cg = new YmapCarGen(CurrentYmapFile, ccg);

            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    CurrentYmapFile.AddCarGen(cg);
                }
            }
            else
            {
                CurrentYmapFile.AddCarGen(cg);
            }


            LoadProjectTree();

            ProjectExplorer?.TrySelectCarGenTreeNode(cg);
            CurrentCarGen = cg;
            ShowEditYmapCarGenPanel(false);
        }
        public void AddCarGenToProject()
        {
            if (CurrentCarGen == null) return;

            CurrentYmapFile = CurrentCarGen.Ymap;
            if (!YmapExistsInProject(CurrentYmapFile))
            {
                var cargen = CurrentCarGen;
                CurrentYmapFile.HasChanged = true;
                AddYmapToProject(CurrentYmapFile);

                CurrentCarGen = cargen; //bug fix for some reason the treeview selects the project node here.
                CurrentYmapFile = cargen.Ymap;
                ProjectExplorer?.TrySelectCarGenTreeNode(cargen);
            }
        }
        public bool DeleteCarGen()
        {
            if (CurrentYmapFile == null) return false;
            if (CurrentCarGen == null) return false;
            if (CurrentCarGen.Ymap != CurrentYmapFile) return false;
            if (CurrentYmapFile.CarGenerators == null) return false; //nothing to delete..

            if (MessageBox.Show("Are you sure you want to delete this car generator?\n" + CurrentCarGen.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentYmapFile.RemoveCarGen(CurrentCarGen);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentYmapFile.RemoveCarGen(CurrentCarGen);
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the car generator. This shouldn't happen!");
            }

            var delgen = CurrentCarGen;

            ProjectExplorer?.RemoveCarGenTreeNode(CurrentCarGen);
            ProjectExplorer?.SetYmapHasChanged(CurrentYmapFile, true);

            ClosePanel((EditYmapCarGenPanel p) => { return p.Tag == delgen; });

            CurrentCarGen = null;

            return true;
        }
        public bool IsCurrentCarGen(YmapCarGen cargen)
        {
            return CurrentCarGen == cargen;
        }

        private void ImportMenyooXml()
        {
            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            var xmlpath = ShowOpenDialog("XML Files|*.xml", string.Empty);

            if (string.IsNullOrEmpty(xmlpath)) return;


            var xmlstr = string.Empty;
            try
            {
                xmlstr = File.ReadAllText(xmlpath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading file!\n" + ex.ToString());
            }

            if (string.IsNullOrEmpty(xmlstr)) return;

            var finf = new FileInfo(xmlpath);

            MenyooXml menyooXml = new MenyooXml();
            menyooXml.FilePath = xmlpath;
            menyooXml.FileName = finf.Name;
            menyooXml.Name = Path.GetFileNameWithoutExtension(finf.Name);
            menyooXml.Init(xmlstr);



            string fname = menyooXml.Name + ".ymap";
            lock (ProjectSyncRoot)
            {
                YmapFile ymap = CurrentProjectFile.AddYmapFile(fname);
                if (ymap != null)
                {
                    ymap.Loaded = true;
                    ymap.HasChanged = true; //new ymap, flag as not saved
                    ymap._CMapData.contentFlags = 65; //stream flags value
                }
                CurrentYmapFile = ymap;
            }

            CurrentProjectFile.HasChanged = true;


            int pedcount = 0;
            int carcount = 0;
            int entcount = 0;
            int unkcount = 0;

            foreach (var placement in menyooXml.Placements)
            {
                if (placement.Type == 1)
                {
                    pedcount++;
                }
                else if (placement.Type == 2)
                {
                    CCarGen ccg = new CCarGen();
                    var rotq = Quaternion.Invert(new Quaternion(placement.Rotation));
                    Vector3 cdir = rotq.Multiply(new Vector3(0, 5, 0));
                    ccg.flags = 3680;
                    ccg.orientX = cdir.X;
                    ccg.orientY = cdir.Y;
                    ccg.perpendicularLength = 2.6f;
                    ccg.position = placement.Position;
                    ccg.carModel = placement.ModelHash;

                    YmapCarGen cg = new YmapCarGen(CurrentYmapFile, ccg);

                    if (WorldForm != null)
                    {
                        lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                        {
                            CurrentYmapFile.AddCarGen(cg);
                        }
                    }
                    else
                    {
                        CurrentYmapFile.AddCarGen(cg);
                    }

                    carcount++;
                }
                else if (placement.Type == 3) //standard entity
                {
                    CEntityDef cent = new CEntityDef();
                    cent.archetypeName = placement.ModelHash;
                    cent.position = placement.Position;
                    cent.rotation = placement.Rotation;
                    cent.scaleXY = 1.0f;
                    cent.scaleZ = 1.0f;
                    cent.flags = placement.Dynamic ? 32u : 0;// 1572872; //?
                    cent.parentIndex = -1;
                    cent.lodDist = placement.LodDistance;
                    cent.lodLevel = Unk_1264241711.LODTYPES_DEPTH_ORPHANHD;
                    cent.priorityLevel = Unk_648413703.PRI_REQUIRED;
                    cent.ambientOcclusionMultiplier = 255;
                    cent.artificialAmbientOcclusion = 255;

                    YmapEntityDef ent = new YmapEntityDef(CurrentYmapFile, 0, ref cent);

                    ent.SetArchetype(GameFileCache.GetArchetype(cent.archetypeName));

                    if (WorldForm != null)
                    {
                        lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                        {
                            CurrentYmapFile.AddEntity(ent);
                        }
                    }
                    else
                    {
                        CurrentYmapFile.AddEntity(ent);
                    }

                    entcount++;
                }
                else
                {
                    unkcount++;
                }
            }

            lock (ProjectSyncRoot)
            {
                CurrentYmapFile.CalcFlags();
                CurrentYmapFile.CalcExtents();
            }

            LoadProjectTree();


            ShowProjectItem(CurrentYmapFile, false);


            MessageBox.Show(entcount.ToString() + " entities imported. \n" + carcount.ToString() + " car generators imported. \n" + pedcount.ToString() + " peds ignored. \n" + unkcount.ToString() + " others ignored.");

        }



        public void NewYtyp()
        {
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (CurrentProjectFile == null) return;

            int testi = 1;
            string fname = string.Empty;
            bool filenameok = false;
            while (!filenameok)
            {
                fname = "types" + testi.ToString() + ".ytyp";
                filenameok = !CurrentProjectFile.ContainsYtyp(fname);
                testi++;
            }

            lock (projectsyncroot)
            {
                YtypFile ytyp = CurrentProjectFile.AddYtypFile(fname);
                if (ytyp != null)
                {
                    //ytyp.Loaded = true;
                    ytyp.HasChanged = true; //new ytyp, flag as not saved
                }
            }

            CurrentProjectFile.HasChanged = true;

            LoadProjectTree();
        }
        public void OpenYtyp()
        {
            string[] files = ShowOpenDialogMulti("Ytyp files|*.ytyp", string.Empty);
            if (files == null)
            {
                return;
            }

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            foreach (string file in files)
            {
                if (!File.Exists(file)) continue;

                var ytyp = CurrentProjectFile.AddYtypFile(file);

                if (ytyp != null)
                {
                    SetProjectHasChanged(true);

                    LoadYtypFromFile(ytyp, file);

                    LoadProjectTree();
                }
                else
                {
                    MessageBox.Show("Couldn't add\n" + file + "\n - the file already exists in the project.");
                }

            }
        }
        public void SaveYtyp(bool saveas = false) //TODO!
        {
        }
        public void AddYtypToProject(YtypFile ytyp)
        {
            if (ytyp == null) return;
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (YtypExistsInProject(ytyp)) return;
            if (CurrentProjectFile.AddYtypFile(ytyp))
            {
                ytyp.HasChanged = true;
                CurrentProjectFile.HasChanged = true;
                LoadProjectTree();
            }
            CurrentYtypFile = ytyp;
            RefreshUI();
        }
        public void RemoveYtypFromProject()
        {
            if (CurrentYtypFile == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveYtypFile(CurrentYtypFile);
            CurrentYtypFile = null;
            LoadProjectTree();
            RefreshUI();
        }
        public bool YtypExistsInProject(YtypFile ytyp)
        {
            if (ytyp == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsYtyp(ytyp);
        }


        public void NewYnd()
        {
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (CurrentProjectFile == null) return;

            int testi = 1;
            string fname = string.Empty;
            bool filenameok = false;
            while (!filenameok)
            {
                fname = "nodes" + testi.ToString() + ".ynd";
                filenameok = !CurrentProjectFile.ContainsYnd(fname);
                testi++;
            }

            lock (projectsyncroot)
            {
                YndFile ynd = CurrentProjectFile.AddYndFile(fname);
                if (ynd != null)
                {
                    ynd.Loaded = true;
                    ynd.HasChanged = true; //new ynd, flag as not saved

                    //TODO: set new ynd default values...
                    ynd.NodeDictionary = new NodeDictionary();
                }
            }

            CurrentProjectFile.HasChanged = true;

            LoadProjectTree();
        }
        public void OpenYnd()
        {
            string[] files = ShowOpenDialogMulti("Ynd files|*.ynd", string.Empty);
            if (files == null)
            {
                return;
            }

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            foreach (string file in files)
            {
                if (!File.Exists(file)) continue;

                var ynd = CurrentProjectFile.AddYndFile(file);

                if (ynd != null)
                {
                    SetProjectHasChanged(true);

                    LoadYndFromFile(ynd, file);

                    LoadProjectTree();
                }
                else
                {
                    MessageBox.Show("Couldn't add\n" + file + "\n - the file already exists in the project.");
                }

            }
        }
        public void SaveYnd(bool saveas = false)
        {
            if ((CurrentYndFile == null) && (CurrentPathNode != null)) CurrentYndFile = CurrentPathNode.Ynd;
            if (CurrentYndFile == null) return;
            string yndname = CurrentYndFile.Name;
            string filepath = CurrentYndFile.FilePath;
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = yndname;
            }
            string origfile = filepath;
            if (!File.Exists(filepath))
            {
                saveas = true;
            }


            byte[] data;
            lock (projectsyncroot) //need to sync writes to ynd objects...
            {
                saveas = saveas || string.IsNullOrEmpty(filepath);
                if (saveas)
                {
                    filepath = ShowSaveDialog("Ynd files|*.ynd", filepath);
                    if (string.IsNullOrEmpty(filepath))
                    { return; }

                    string newname = Path.GetFileNameWithoutExtension(filepath);
                    JenkIndex.Ensure(newname);
                    CurrentYndFile.FilePath = filepath;
                    CurrentYndFile.RpfFileEntry.Name = new FileInfo(filepath).Name;
                    CurrentYndFile.Name = CurrentYndFile.RpfFileEntry.Name;
                }


                data = CurrentYndFile.Save();
            }


            if (data != null)
            {
                File.WriteAllBytes(filepath, data);
            }

            SetYndHasChanged(false);

            if (saveas)
            {
                //ShowEditYndPanel(false);
                if (CurrentProjectFile != null)
                {
                    string origpath = CurrentProjectFile.GetRelativePath(origfile);
                    string newpath = CurrentProjectFile.GetRelativePath(CurrentYndFile.FilePath);
                    if (!CurrentProjectFile.RenameYnd(origpath, newpath))
                    { //couldn't rename it in the project? happens when project not saved yet...
                        //MessageBox.Show("Couldn't rename ynd in project! This shouldn't happen - check the project file XML.");
                    }
                }
                SetProjectHasChanged(true);
                SetCurrentSaveItem();
            }

        }
        public void AddYndToProject(YndFile ynd)
        {
            if (ynd == null) return;
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (YndExistsInProject(ynd)) return;
            if (CurrentProjectFile.AddYndFile(ynd))
            {
                ynd.HasChanged = true;
                CurrentProjectFile.HasChanged = true;
                LoadProjectTree();
            }
            CurrentYndFile = ynd;
            RefreshUI();
            if (CurrentPathNode != null)
            {
                ProjectExplorer?.TrySelectPathNodeTreeNode(CurrentPathNode);
            }
        }
        public void RemoveYndFromProject()
        {
            if (CurrentYndFile == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveYndFile(CurrentYndFile);
            CurrentYndFile = null;
            LoadProjectTree();
            RefreshUI();
        }
        public bool YndExistsInProject(YndFile ynd)
        {
            if (ynd == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsYnd(ynd);
        }

        public void NewPathNode(YndNode copy = null, bool copyPosition = false)
        {
            if (CurrentYndFile == null) return;

            var n = CurrentYndFile.AddNode();
            var areaid = n.AreaID;
            var nodeid = n.NodeID;
            if (copy == null)
            {
                copy = CurrentPathNode;
            }
            if (copy != null)
            {
                n.Init(CurrentYndFile, copy.RawData);
                n.LinkCountUnk = copy.LinkCountUnk;
            }
            n.AreaID = areaid;
            n.NodeID = nodeid;

            bool cp = copyPosition && (copy != null);
            Vector3 pos = cp ? copy.Position : GetSpawnPos(10.0f);
            n.SetPosition(pos);


            if (copy != null)
            {
                var link1 = n.AddLink(copy);
                var link2 = copy.AddLink(n);
                if ((copy.Links != null) && (copy.Links.Length > 0))
                {
                    var clink = copy.Links[0];
                    link1.CopyFlags(clink);
                    var clnode = clink.Node2;
                    if (clnode.Links != null)
                    {
                        for (int i = 0; i < clnode.Links.Length; i++)
                        {
                            var clnlink = clnode.Links[i];
                            if (clnlink.Node2 == copy)
                            {
                                link2.CopyFlags(clnlink);
                                break;
                            }
                        }
                    }
                }
            }

            CurrentYndFile.UpdateAllNodePositions(); //for the graphics...
            CurrentYndFile.BuildBVH();


            LoadProjectTree();

            ProjectExplorer?.TrySelectPathNodeTreeNode(n);
            CurrentPathNode = n;
            //ShowEditYndPanel(false);;
            ShowEditYndNodePanel(false);


            if (WorldForm != null)
            {
                WorldForm.UpdatePathYndGraphics(CurrentYndFile, false);
            }
        }
        public bool DeletePathNode()
        {
            if (CurrentYndFile == null) return false;
            if (CurrentPathNode == null) return false;
            if (CurrentPathNode.Ynd != CurrentYndFile) return false;
            if (CurrentYndFile.Nodes == null) return false; //nothing to delete..

            if (MessageBox.Show("Are you sure you want to delete this path node?\n" + CurrentPathNode.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentYndFile.RemoveNode(CurrentPathNode);

                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentYndFile.RemoveNode(CurrentPathNode);
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the path node. This shouldn't happen!");
            }

            var delnode = CurrentPathNode;

            ProjectExplorer?.RemovePathNodeTreeNode(CurrentPathNode);
            ProjectExplorer?.SetYndHasChanged(CurrentYndFile, true);

            ClosePanel((EditYndNodePanel p) => { return p.Tag == delnode; });

            CurrentPathNode = null;

            if (WorldForm != null)
            {
                WorldForm.UpdatePathYndGraphics(CurrentYndFile, false);
            }

            return true;
        }
        public bool IsCurrentPathNode(YndNode pathnode)
        {
            return CurrentPathNode == pathnode;
        }



        public void NewYnv()//TODO!
        {
        }
        public void OpenYnv()//TODO!
        {
        }
        public void SaveYnv(bool saveas = false)//TODO!
        {
        }
        public void AddYnvToProject(YnvFile ynv)
        {
            if (ynv == null) return;
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (YnvExistsInProject(ynv)) return;
            if (CurrentProjectFile.AddYnvFile(ynv))
            {
                ynv.HasChanged = true;
                CurrentProjectFile.HasChanged = true;
                LoadProjectTree();
            }
            CurrentYnvFile = ynv;
            RefreshUI();
            if (CurrentNavPoly != null)
            {
                ProjectExplorer?.TrySelectNavPolyTreeNode(CurrentNavPoly);
            }
        }
        public void RemoveYnvFromProject()
        {
            if (CurrentYnvFile == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveYnvFile(CurrentYnvFile);
            CurrentYnvFile = null;
            LoadProjectTree();
            RefreshUI();
        }
        public bool YnvExistsInProject(YnvFile ynv)
        {
            if (ynv == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsYnv(ynv);
        }

        public void NewNavPoly(YnvPoly copy = null, bool copyposition = false)//TODO!
        {
        }
        public bool DeleteNavPoly()//TODO!
        {
            return false;
        }
        public bool IsCurrentNavPoly(YnvPoly poly)
        {
            return false;// poly == CurrentNavPoly;
        }



        public void NewTrainTrack()
        {
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (CurrentProjectFile == null) return;

            int testi = 13;
            string fname = string.Empty;
            bool filenameok = false;
            while (!filenameok)
            {
                fname = "trains" + testi.ToString() + ".dat";
                filenameok = !CurrentProjectFile.ContainsTrainTrack(fname);
                testi++;
            }

            lock (projectsyncroot)
            {
                TrainTrack track = CurrentProjectFile.AddTrainsFile(fname);
                if (track != null)
                {
                    track.Loaded = true;
                    track.HasChanged = true; //new track, flag as not saved

                    //TODO: set new train track default values...

                }
            }

            CurrentProjectFile.HasChanged = true;

            LoadProjectTree();
        }
        public void OpenTrainTrack()
        {
            string[] files = ShowOpenDialogMulti("Dat files|*.dat", string.Empty);
            if (files == null)
            {
                return;
            }

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            foreach (string file in files)
            {
                if (!File.Exists(file)) continue;

                var track = CurrentProjectFile.AddTrainsFile(file);

                if (track != null)
                {
                    SetProjectHasChanged(true);

                    LoadTrainTrackFromFile(track, file);

                    LoadProjectTree();
                }
                else
                {
                    MessageBox.Show("Couldn't add\n" + file + "\n - the file already exists in the project.");
                }

            }
        }
        public void SaveTrainTrack(bool saveas = false)
        {
            if ((CurrentTrainTrack == null) && (CurrentTrainNode != null)) CurrentTrainTrack = CurrentTrainNode.Track;
            if (CurrentTrainTrack == null) return;
            string trackname = CurrentTrainTrack.Name;
            string filepath = CurrentTrainTrack.FilePath;
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = trackname;
            }
            string origfile = filepath;
            if (!File.Exists(filepath))
            {
                saveas = true;
            }


            byte[] data;
            lock (projectsyncroot) //need to sync writes to objects...
            {
                saveas = saveas || string.IsNullOrEmpty(filepath);
                if (saveas)
                {
                    filepath = ShowSaveDialog("Dat files|*.dat", filepath);
                    if (string.IsNullOrEmpty(filepath))
                    { return; }

                    string newname = Path.GetFileNameWithoutExtension(filepath);
                    //////JenkIndex.Ensure(newname);
                    CurrentTrainTrack.FilePath = filepath;
                    CurrentTrainTrack.RpfFileEntry.Name = new FileInfo(filepath).Name;
                    CurrentTrainTrack.Name = CurrentTrainTrack.RpfFileEntry.Name;
                }


                data = CurrentTrainTrack.Save();
            }


            if (data != null)
            {
                File.WriteAllBytes(filepath, data);
            }

            SetTrainTrackHasChanged(false);

            if (saveas)
            {
                //ShowEditTrainTrackPanel(false);
                if (CurrentProjectFile != null)
                {
                    string origpath = CurrentProjectFile.GetRelativePath(origfile);
                    string newpath = CurrentProjectFile.GetRelativePath(CurrentTrainTrack.FilePath);
                    if (!CurrentProjectFile.RenameTrainTrack(origpath, newpath))
                    { //couldn't rename it in the project? happens when project not saved yet...
                        //MessageBox.Show("Couldn't rename train track in project! This shouldn't happen - check the project file XML.");
                    }
                }
                SetProjectHasChanged(true);
                SetCurrentSaveItem();
            }

        }
        public void AddTrainTrackToProject(TrainTrack track)
        {
            if (track == null) return;
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (TrainTrackExistsInProject(track)) return;
            if (CurrentProjectFile.AddTrainsFile(track))
            {
                track.HasChanged = true;
                CurrentProjectFile.HasChanged = true;
                LoadProjectTree();
            }
            CurrentTrainTrack = track;
            RefreshUI();
            if (CurrentTrainNode != null)
            {
                ProjectExplorer?.TrySelectTrainNodeTreeNode(CurrentTrainNode);
            }
        }
        public void RemoveTrainTrackFromProject()
        {
            if (CurrentTrainTrack == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveTrainsFile(CurrentTrainTrack);
            CurrentTrainTrack = null;
            LoadProjectTree();
            RefreshUI();
        }
        public bool TrainTrackExistsInProject(TrainTrack track)
        {
            if (track == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsTrainTrack(track);
        }

        public void NewTrainNode(TrainTrackNode copy = null, bool copyPosition = false)
        {
            if (CurrentTrainTrack == null) return;

            var afternode = copyPosition ? copy : null;

            var n = CurrentTrainTrack.AddNode(afternode);
            if (copy == null)
            {
                copy = CurrentTrainNode;
            }
            if (copy != null)
            {
                n.NodeType = copy.NodeType;
            }

            bool cp = copyPosition && (copy != null);
            Vector3 pos = cp ? copy.Position : GetSpawnPos(10.0f);
            n.SetPosition(pos);


            //CurrentTrainTrack.BuildVertices(); //for the graphics...
            CurrentTrainTrack.BuildBVH();


            LoadProjectTree();

            ProjectExplorer?.TrySelectTrainNodeTreeNode(n);
            CurrentTrainNode = n;
            ShowEditTrainNodePanel(false);


            if (WorldForm != null)
            {
                WorldForm.UpdateTrainTrackGraphics(CurrentTrainTrack, false);
            }
        }
        public bool DeleteTrainNode()
        {
            if (CurrentTrainTrack == null) return false;
            if (CurrentTrainNode == null) return false;
            if (CurrentTrainNode.Track != CurrentTrainTrack) return false;
            if (CurrentTrainTrack.Nodes == null) return false; //nothing to delete..

            if (MessageBox.Show("Are you sure you want to delete this train track node?\n" + CurrentTrainNode.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentTrainTrack.RemoveNode(CurrentTrainNode);

                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentTrainTrack.RemoveNode(CurrentTrainNode);
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the train track node. This shouldn't happen!");
            }

            var delnode = CurrentTrainNode;

            ProjectExplorer?.RemoveTrainNodeTreeNode(CurrentTrainNode);
            ProjectExplorer?.SetTrainTrackHasChanged(CurrentTrainTrack, true);

            ClosePanel((EditTrainNodePanel p) => { return p.Tag == delnode; });

            CurrentTrainNode = null;

            if (WorldForm != null)
            {
                WorldForm.UpdateTrainTrackGraphics(CurrentTrainTrack, false);
            }

            return true;
        }
        public bool IsCurrentTrainNode(TrainTrackNode node)
        {
            return node == CurrentTrainNode;
        }



        public void NewScenario()
        {
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (CurrentProjectFile == null) return;

            int testi = 1;
            string fname = string.Empty;
            bool filenameok = false;
            while (!filenameok)
            {
                fname = "scenario" + testi.ToString() + ".ymt";
                filenameok = !CurrentProjectFile.ContainsScenario(fname);
                testi++;
            }

            lock (projectsyncroot)
            {
                YmtFile ymt = CurrentProjectFile.AddScenarioFile(fname);
                if (ymt != null)
                {
                    ymt.CScenarioPointRegion = new MCScenarioPointRegion();
                    ymt.CScenarioPointRegion.Ymt = ymt;
                    ymt.CScenarioPointRegion.Points = new MCScenarioPointContainer(ymt.CScenarioPointRegion);
                    ymt.CScenarioPointRegion.Paths = new MUnk_4023740759(ymt.CScenarioPointRegion);
                    ymt.CScenarioPointRegion.LookUps = new MCScenarioPointLookUps(ymt.CScenarioPointRegion);

                    ymt.ScenarioRegion = new ScenarioRegion();
                    ymt.ScenarioRegion.Region = ymt.CScenarioPointRegion;
                    ymt.ScenarioRegion.Ymt = ymt;

                    ymt.ScenarioRegion.BuildNodes(); //should be empty
                    ymt.ScenarioRegion.BuildBVH(); //should be empty
                    ymt.ScenarioRegion.BuildVertices(); //should be empty

                    ymt.HasChanged = true; //new ymt, flag as not saved
                    ymt.Loaded = true;
                }
            }

            CurrentProjectFile.HasChanged = true;

            LoadProjectTree();
        }
        public void OpenScenario()
        {
            string[] files = ShowOpenDialogMulti("Ymt files|*.ymt", string.Empty);
            if (files == null)
            {
                return;
            }

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            foreach (string file in files)
            {
                if (!File.Exists(file)) continue;

                var ymt = CurrentProjectFile.AddScenarioFile(file);

                if (ymt != null)
                {
                    SetProjectHasChanged(true);

                    LoadScenarioFromFile(ymt, file);

                    LoadProjectTree();
                }
                else
                {
                    MessageBox.Show("Couldn't add\n" + file + "\n - the file already exists in the project.");
                }

            }
        }
        public void SaveScenario(bool saveas = false)
        {
            if ((CurrentScenario == null) && (CurrentScenarioNode != null)) CurrentScenario = CurrentScenarioNode.Ymt;
            if (CurrentScenario == null) return;
            string ymtname = CurrentScenario.Name;
            string filepath = CurrentScenario.FilePath;
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = ymtname;
            }
            string origfile = filepath;
            if (!File.Exists(filepath))
            {
                saveas = true;
            }


            byte[] data;
            lock (projectsyncroot) //need to sync writes to scenario...
            {
                saveas = saveas || string.IsNullOrEmpty(filepath);
                if (saveas)
                {
                    filepath = ShowSaveDialog("Ymt files|*.ymt", filepath);
                    if (string.IsNullOrEmpty(filepath))
                    { return; }

                    string newname = Path.GetFileNameWithoutExtension(filepath);
                    JenkIndex.Ensure(newname);
                    CurrentScenario.FilePath = filepath;
                    CurrentScenario.RpfFileEntry.Name = new FileInfo(filepath).Name;
                    CurrentScenario.Name = CurrentScenario.RpfFileEntry.Name;
                }


                CurrentScenario.ContentType = YmtFileContentType.ScenarioPointRegion;//just to be sure..

                data = CurrentScenario.Save();
            }


            if (data != null)
            {
                File.WriteAllBytes(filepath, data);
            }

            SetScenarioHasChanged(false);

            if (saveas)
            {
                //ShowEditScenarioPanel(false);
                if (CurrentProjectFile != null)
                {
                    string origpath = CurrentProjectFile.GetRelativePath(origfile);
                    string newpath = CurrentProjectFile.GetRelativePath(CurrentScenario.FilePath);
                    if (!CurrentProjectFile.RenameScenario(origpath, newpath))
                    { //couldn't rename it in the project? happens when project not saved yet...
                        //MessageBox.Show("Couldn't rename scenario in project! This shouldn't happen - check the project file XML.");
                    }
                }
                SetProjectHasChanged(true);
                SetCurrentSaveItem();
            }
        }
        public void AddScenarioToProject(YmtFile ymt)
        {
            if (ymt == null) return;
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (ScenarioExistsInProject(ymt)) return;
            if (CurrentProjectFile.AddScenarioFile(ymt))
            {
                ymt.HasChanged = true;
                CurrentProjectFile.HasChanged = true;
                LoadProjectTree();
            }
            CurrentScenario = ymt;
            RefreshUI();
            if (CurrentScenarioNode != null)
            {
                ProjectExplorer?.TrySelectScenarioNodeTreeNode(CurrentScenarioNode);
            }
        }
        public void RemoveScenarioFromProject()
        {
            if (CurrentScenario == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveScenarioFile(CurrentScenario);
            CurrentScenario = null;
            LoadProjectTree();
            RefreshUI();
        }
        public bool ScenarioExistsInProject(YmtFile ymt)
        {
            if (ymt == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsScenario(ymt);
        }

        public void NewScenarioNode(ScenarioNode copy = null, bool copyPosition = false)
        {
            if (CurrentScenario == null) return;
            if (CurrentScenario.ScenarioRegion == null) return;

            if (copy == null)
            {
                copy = CurrentScenarioNode;
            }

            var n = CurrentScenario.ScenarioRegion.AddNode(copy);

            bool cp = copyPosition && (copy != null);
            Vector3 pos = cp ? copy.Position : GetSpawnPos(10.0f);
            Quaternion ori = cp ? copy.Orientation : Quaternion.Identity;
            n.SetPosition(pos);
            n.SetOrientation(ori);


            LoadProjectTree();

            ProjectExplorer?.TrySelectScenarioNodeTreeNode(n);
            CurrentScenarioNode = n;
            //ShowEditScenarioPanel(false);
            ShowEditScenarioNodePanel(false);


            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }
            else
            {
                CurrentScenario.ScenarioRegion.BuildBVH();
                CurrentScenario.ScenarioRegion.BuildVertices(); //for the graphics...
            }
        }
        public bool DeleteScenarioNode()
        {
            if (CurrentScenario == null) return false;
            if (CurrentScenario.ScenarioRegion == null) return false;
            if (CurrentScenarioNode == null) return false;


            if (MessageBox.Show("Are you sure you want to delete this scenario node?\n" + CurrentScenarioNode.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentScenario.ScenarioRegion.RemoveNode(CurrentScenarioNode);
                }
            }
            else
            {
                res = CurrentScenario.ScenarioRegion.RemoveNode(CurrentScenarioNode);
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the scenario node. This shouldn't happen!");
            }

            var delnode = CurrentScenarioNode;

            ProjectExplorer?.RemoveScenarioNodeTreeNode(CurrentScenarioNode);
            ProjectExplorer?.SetScenarioHasChanged(CurrentScenario, true);

            ClosePanel((EditScenarioNodePanel p) => { return p.Tag == delnode; });

            CurrentScenarioNode = null;

            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }


            return true;
        }
        public bool IsCurrentScenarioNode(ScenarioNode node)
        {
            return node == CurrentScenarioNode;
        }


        public void SetScenarioChainEdge(MCScenarioChainingEdge e)
        {
            CurrentScenarioChainEdge = e;
        }

        public void AddScenarioChain()
        {
            if (CurrentScenario.ScenarioRegion == null) return;
            var paths = CurrentScenario?.CScenarioPointRegion?.Paths;
            if (paths == null) return;

            var copy = CurrentScenarioNode?.ChainingNode?.Chain;
            var copyn = CurrentScenarioNode?.ChainingNode;
            var copyp = CurrentScenarioNode?.MyPoint;
            var copye = CurrentScenarioChainEdge;
            var copycl = CurrentScenarioNode?.Cluster;

            MCScenarioChain chain = new MCScenarioChain();
            chain.Unk1 = 1; //default value
            if (copy != null)
            {
                chain.Data = copy.Data;
                chain._Data.EdgeIds = new Array_ushort(); //start empty.. not really necessary
            }

            paths.AddChain(chain);


            //add 2 new nodes to the new chain.
            var pos1 = GetSpawnPos(10.0f);
            var pos2 = pos1 + Vector3.UnitX;

            ScenarioNode n1 = null;// CurrentScenario.ScenarioRegion.AddNode();

            if (copycl != null)
            {
                ScenarioNode copyclnode = new ScenarioNode(CurrentScenario);
                copyclnode.Cluster = copycl;
                copyclnode.ClusterMyPoint = new MCScenarioPoint(CurrentScenario.CScenarioPointRegion);
                copyclnode.ClusterMyPoint.InteriorName = 493038497; //JenkHash.GenHash("none");
                copyclnode.ClusterMyPoint.GroupName = 493038497;
                copyclnode.ClusterMyPoint.IMapName = 493038497;
                copyclnode.ClusterMyPoint.TimeStart = 0;
                copyclnode.ClusterMyPoint.TimeEnd = 24;
                n1 = CurrentScenario.ScenarioRegion.AddNode(copyclnode);
            }
            else
            {
                n1 = CurrentScenario.ScenarioRegion.AddNode();
            }

            ScenarioNode n2 = CurrentScenario.ScenarioRegion.AddNode();

            if (copyp != null)
            {
                n1.MyPoint.CopyFrom(copyp);
                n2.MyPoint.CopyFrom(copyp);
            }

            n1.ChainingNode = new MCScenarioChainingNode();
            n2.ChainingNode = new MCScenarioChainingNode();

            if (copyn != null)
            {
                n1.ChainingNode.CopyFrom(copyn);
                n2.ChainingNode.CopyFrom(copyn);
            }

            n1.ChainingNode.NotLast = true;
            n2.ChainingNode.NotFirst = true;

            n1.ChainingNode.ScenarioNode = n1;
            n2.ChainingNode.ScenarioNode = n2;

            paths.AddNode(n1.ChainingNode);
            paths.AddNode(n2.ChainingNode);

            n1.ChainingNode.Chain = chain;
            n2.ChainingNode.Chain = chain;

            var ed = new MCScenarioChainingEdge();

            if (copye != null)
            {
                ed.Data = copye.Data;
            }

            ed.NodeFrom = n1.ChainingNode;
            ed.NodeTo = n2.ChainingNode;
            ed.NodeIndexFrom = (ushort)n1.ChainingNode.NodeIndex;
            ed.NodeIndexTo = (ushort)n2.ChainingNode.NodeIndex;

            paths.AddEdge(ed);
            chain.AddEdge(ed);



            n1.SetPosition(pos1);
            n2.SetPosition(pos2);






            ProjectExplorer?.LoadProjectTree(CurrentProjectFile);

            ProjectExplorer?.TrySelectScenarioNodeTreeNode(n1);

            CurrentScenarioNode = n1;
            CurrentScenarioChainEdge = ed;
            //LoadScenarioChainTabPage();
            ////LoadScenarioTabPage();
            ////LoadScenarioNodeTabPages();


            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }
            else
            {
                CurrentScenario.ScenarioRegion.BuildBVH();
                CurrentScenario.ScenarioRegion.BuildVertices(); //for the graphics...
            }
        }
        public void AddScenarioCluster()//TODO: add defualt cluster points to new cluster
        {
            if (CurrentScenario == null) return;

            var sr = CurrentScenario.ScenarioRegion;
            if (sr == null) return;

            var rgn = CurrentScenario.CScenarioPointRegion;
            if (rgn == null) return;

            var copy = CurrentScenarioNode?.Cluster;


            MCScenarioPointCluster cluster = new MCScenarioPointCluster(rgn, copy);
            List<MCScenarioPoint> clusterpoints = new List<MCScenarioPoint>();

            if (copy != null)
            {
                if (copy.Points?.MyPoints != null)
                {
                    clusterpoints.AddRange(copy.Points.MyPoints);
                }
            }



            rgn.AddCluster(cluster); //add the base cluster to the region.


            var pos1 = GetSpawnPos(10.0f);

            var ncopy = new ScenarioNode(CurrentScenario);//copy an empty node to start with, to avoid creating default MyPoint
            var nc = sr.AddNode(ncopy); //add the base cluster's display node.
            nc.Cluster = cluster;

            nc.SetPosition(pos1);

            if (cluster.Points != null)
            {
                foreach (var cpt in clusterpoints)
                {
                    //TODO: copy cluster points....
                    //or create some default points!
                }
            }








            ProjectExplorer?.LoadProjectTree(CurrentProjectFile);

            ProjectExplorer?.TrySelectScenarioNodeTreeNode(nc);

            CurrentScenarioNode = nc;
            //LoadScenarioClusterTabPage();


            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }
            else
            {
                CurrentScenario.ScenarioRegion.BuildBVH();
                CurrentScenario.ScenarioRegion.BuildVertices(); //for the graphics...
            }

        }
        public void AddScenarioClusterPoint()
        {
            if (CurrentScenario == null) return;

            var sr = CurrentScenario.ScenarioRegion;
            if (sr == null) return;

            var rgn = CurrentScenario.CScenarioPointRegion;
            if (rgn == null) return;

            var cluster = CurrentScenarioNode?.Cluster;
            if (cluster == null) return;

            if (cluster.Points == null)
            {
                cluster.Points = new MCScenarioPointContainer(rgn);
                cluster.Points.Parent = cluster;
            }

            var copy = CurrentScenarioNode?.ClusterMyPoint;

            var pos1 = GetSpawnPos(10.0f);
            var ori1 = copy?.Orientation ?? Quaternion.Identity;

            var cn = new ScenarioNode(CurrentScenario);//copy a blank node
            var n = sr.AddNode(cn);

            var np = new MCScenarioPoint(rgn, copy);
            cluster.Points.AddMyPoint(np);

            n.ClusterMyPoint = np;
            n.Cluster = cluster;

            n.SetPosition(pos1);
            n.SetOrientation(ori1);


            ProjectExplorer?.LoadProjectTree(CurrentProjectFile);

            ProjectExplorer?.TrySelectScenarioNodeTreeNode(n);

            CurrentScenarioNode = n;
            //LoadScenarioClusterTabPage();
            //LoadScenarioClusterPointTabPage();
            ////LoadScenarioTabPage();
            ////LoadScenarioNodeTabPages();


            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }
            else
            {
                CurrentScenario.ScenarioRegion.BuildBVH();
                CurrentScenario.ScenarioRegion.BuildVertices(); //for the graphics...
            }
        }
        public void AddScenarioEntity()//TODO: add default entity point(s) to entity
        {
            if (CurrentScenario == null) return;

            var sr = CurrentScenario.ScenarioRegion;
            if (sr == null) return;

            var rgn = CurrentScenario.CScenarioPointRegion;
            if (rgn == null) return;

            var copy = CurrentScenarioNode?.Entity;


            MCScenarioEntityOverride entity = new MCScenarioEntityOverride(rgn, copy);
            List<MCExtensionDefSpawnPoint> entpoints = new List<MCExtensionDefSpawnPoint>();

            if (copy != null)
            {
                if (copy.ScenarioPoints != null)
                {
                    entpoints.AddRange(copy.ScenarioPoints);
                }
            }






            rgn.AddEntity(entity); //add the base entity to the region.


            var pos1 = GetSpawnPos(10.0f);

            var ncopy = new ScenarioNode(CurrentScenario);//copy an empty node to start with, to avoid creating default MyPoint
            var ne = sr.AddNode(ncopy); //add the base entity's display node.
            ne.Entity = entity;

            ne.SetPosition(pos1);

            if (entity.ScenarioPoints != null)
            {
                foreach (var cpt in entpoints)
                {
                    //TODO: copy entity points....
                    //or create some default points!
                }
            }








            ProjectExplorer?.LoadProjectTree(CurrentProjectFile);

            ProjectExplorer?.TrySelectScenarioNodeTreeNode(ne);

            CurrentScenarioNode = ne;
            //LoadScenarioEntityTabPage();


            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }
            else
            {
                CurrentScenario.ScenarioRegion.BuildBVH();
                CurrentScenario.ScenarioRegion.BuildVertices(); //for the graphics...
            }

        }
        public void AddScenarioEntityPoint()
        {
            if (CurrentScenario == null) return;

            var sr = CurrentScenario.ScenarioRegion;
            if (sr == null) return;

            var rgn = CurrentScenario.CScenarioPointRegion;
            if (rgn == null) return;

            var entity = CurrentScenarioNode?.Entity;
            if (entity == null) return;

            var copy = CurrentScenarioNode?.EntityPoint;

            var pos1 = GetSpawnPos(10.0f);
            var ori1 = copy?.Orientation ?? Quaternion.Identity;

            var cn = new ScenarioNode(CurrentScenario);//copy a blank node
            var n = sr.AddNode(cn);

            var np = new MCExtensionDefSpawnPoint(rgn, copy);
            entity.AddScenarioPoint(np);

            n.EntityPoint = np;
            n.Entity = entity;

            n.SetPosition(pos1);
            n.SetOrientation(ori1);


            ProjectExplorer?.LoadProjectTree(CurrentProjectFile);

            ProjectExplorer?.TrySelectScenarioNodeTreeNode(n);

            CurrentScenarioNode = n;
            //LoadScenarioEntityTabPage();
            //LoadScenarioEntityPointTabPage();
            ////LoadScenarioTabPage();
            ////LoadScenarioNodeTabPages();


            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }
            else
            {
                CurrentScenario.ScenarioRegion.BuildBVH();
                CurrentScenario.ScenarioRegion.BuildVertices(); //for the graphics...
            }
        }

        public void DeleteScenarioChain()
        {
            if (CurrentScenario == null) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ChainingNode == null) return;
            if (CurrentScenarioNode.ChainingNode.Chain == null) return;

            var chain = CurrentScenarioNode.ChainingNode.Chain;

            var paths = CurrentScenario.CScenarioPointRegion?.Paths;
            if (paths == null) return;

            var rgn = CurrentScenario.ScenarioRegion;
            if (rgn == null) return;


            if (MessageBox.Show("Are you sure you want to delete this scenario chain?\n" + chain.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            bool delpoints = false;
            if (MessageBox.Show("Delete all Scenario Points for this chain as well?", "Confirm delete points", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                delpoints = true;
            }



            Dictionary<MCScenarioChainingNode, int> ndict = new Dictionary<MCScenarioChainingNode, int>();

            var edges = chain.Edges;
            if (edges != null)
            {
                foreach (var edge in edges)
                {
                    //paths.RemoveEdge(edge); //removing nodes also removes edges!
                    paths.RemoveNode(edge.NodeFrom);
                    paths.RemoveNode(edge.NodeTo);

                    ndict[edge.NodeFrom] = 1;
                    ndict[edge.NodeTo] = 1;
                }
            }

            paths.RemoveChain(chain);




            List<ScenarioNode> delnodes = new List<ScenarioNode>();
            foreach (var node in rgn.Nodes)
            {
                if ((node.ChainingNode != null) && (ndict.ContainsKey(node.ChainingNode)))
                {
                    delnodes.Add(node);
                }
            }
            foreach (var delnode in delnodes)
            {
                delnode.ChainingNode = null;//this chaining node has been removed from the region. remove this association.
                if (delpoints)
                {
                    rgn.RemoveNode(delnode);
                }
            }



            var cn = CurrentScenarioNode;
            var cs = CurrentScenario;

            ProjectExplorer?.LoadProjectTree(CurrentProjectFile);

            if (!delpoints && (cn != null))
            {
                ProjectExplorer?.TrySelectScenarioNodeTreeNode(cn);
            }
            else
            {
                ProjectExplorer?.TrySelectScenarioTreeNode(cs);
            }

            ClosePanel((EditScenarioNodePanel p) => { return p.Tag == cn; });

            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(cs, false);
                if (delpoints)
                {
                    WorldForm.SelectItem(null);
                }
            }
            else if (cs?.ScenarioRegion != null)
            {
                cs.ScenarioRegion.BuildBVH();
                cs.ScenarioRegion.BuildVertices(); //for the graphics...
            }
        }
        public void DeleteScenarioCluster()
        {
            if (CurrentScenario == null) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Cluster == null) return;

            var cluster = CurrentScenarioNode.Cluster;

            var rgn = CurrentScenario.ScenarioRegion;
            if (rgn == null) return;

            var crgn = CurrentScenario.CScenarioPointRegion;
            if (crgn == null) return;


            if (MessageBox.Show("Are you sure you want to delete this scenario cluster?\n" + cluster.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            bool delpoints = false;
            if (MessageBox.Show("Delete all Scenario Points for this cluster as well?", "Confirm delete points", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                delpoints = true;
            }


            crgn.RemoveCluster(cluster);





            Dictionary<MCScenarioPoint, int> ndict = new Dictionary<MCScenarioPoint, int>();
            if (cluster?.Points?.MyPoints != null)
            {
                foreach (var point in cluster.Points.MyPoints)
                {
                    ndict[point] = 1;
                }
            }
            List<ScenarioNode> delnodes = new List<ScenarioNode>();
            foreach (var node in rgn.Nodes)
            {
                if ((node.ClusterMyPoint != null) && (ndict.ContainsKey(node.ClusterMyPoint)))
                {
                    delnodes.Add(node);
                }
                else if (node.Cluster == cluster)
                {
                    delnodes.Add(node);
                }
            }
            foreach (var delnode in delnodes)
            {
                if (!delpoints && (crgn.Points != null) && (delnode.ClusterMyPoint != null))
                {
                    var copypt = new MCScenarioPoint(crgn, delnode.ClusterMyPoint);
                    crgn.Points.AddMyPoint(copypt);
                    delnode.MyPoint = copypt;
                }
                bool iscl = false;
                if ((delnode.Cluster != null) && (delnode.ClusterMyPoint == null) && (delnode.ClusterLoadSavePoint == null))
                {
                    iscl = true;
                }
                delnode.Cluster = null;
                delnode.ClusterMyPoint = null;//this cluster point has been removed from the region. remove this association.
                delnode.ClusterLoadSavePoint = null;
                if (delpoints)
                {
                    //if ((delnode.ChainingNode == null) && (delnode.EntityPoint == null))
                    {
                        rgn.RemoveNode(delnode);
                    }
                }
                else if (iscl)
                {
                    rgn.RemoveNode(delnode); //remove the cluster node itself.
                }
            }



            var cn = CurrentScenarioNode;
            var cs = CurrentScenario;

            ProjectExplorer?.LoadProjectTree(CurrentProjectFile);

            if (!delpoints && (cn != null))
            {
                ProjectExplorer?.TrySelectScenarioNodeTreeNode(cn);
            }
            else
            {
                ProjectExplorer?.TrySelectScenarioTreeNode(cs);
            }

            ClosePanel((EditScenarioNodePanel p) => { return p.Tag == cn; });

            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(cs, false);
                if (delpoints || ((cn != null) && (cn.MyPoint == null)))
                {
                    WorldForm.SelectItem(null);
                }
            }
            else if (cs?.ScenarioRegion != null)
            {
                cs.ScenarioRegion.BuildBVH();
                cs.ScenarioRegion.BuildVertices(); //for the graphics...
            }

        }
        public void DeleteScenarioEntity()
        {
            if (CurrentScenario == null) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Entity == null) return;

            var entity = CurrentScenarioNode.Entity;

            var rgn = CurrentScenario.ScenarioRegion;
            if (rgn == null) return;

            var crgn = CurrentScenario.CScenarioPointRegion;
            if (crgn == null) return;


            if (MessageBox.Show("Are you sure you want to delete this scenario entity override, and all its override points?\n" + entity.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            //bool delpoints = false;
            //if (MessageBox.Show("Delete all Scenario Points for this entity override as well?", "Confirm delete points", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //{
            //    delpoints = true;
            //}


            crgn.RemoveEntity(entity);





            Dictionary<MCExtensionDefSpawnPoint, int> ndict = new Dictionary<MCExtensionDefSpawnPoint, int>();
            if (entity.ScenarioPoints != null)
            {
                foreach (var point in entity.ScenarioPoints)
                {
                    ndict[point] = 1;
                }
            }
            List<ScenarioNode> delnodes = new List<ScenarioNode>();
            foreach (var node in rgn.Nodes)
            {
                if ((node.EntityPoint != null) && (ndict.ContainsKey(node.EntityPoint)))
                {
                    delnodes.Add(node);
                }
                else if (node.Entity == entity)
                {
                    delnodes.Add(node);
                }
            }
            foreach (var delnode in delnodes)
            {
                delnode.Entity = null;
                delnode.EntityPoint = null;//this entity point has been removed from the region. remove this association.
                rgn.RemoveNode(delnode);
            }



            var cn = CurrentScenarioNode;
            var cs = CurrentScenario;

            ProjectExplorer?.LoadProjectTree(CurrentProjectFile);

            ProjectExplorer?.TrySelectScenarioTreeNode(cs);

            ClosePanel((EditScenarioNodePanel p) => { return p.Tag == cn; });

            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(cs, false);
                WorldForm.SelectItem(null);
            }
            else if (cs?.ScenarioRegion != null)
            {
                cs.ScenarioRegion.BuildBVH();
                cs.ScenarioRegion.BuildVertices(); //for the graphics...
            }
        }

        public void ImportScenarioChain()
        {
            var paths = CurrentScenario?.CScenarioPointRegion?.Paths;
            if (paths == null) return;
            var rgn = CurrentScenario.ScenarioRegion;
            if (rgn == null) return;

            TextInputForm f = new TextInputForm();
            f.TitleText = "Import scenario chain points";
            f.PromptText = "Input chain points in CSV (or TSV) format. Direction is in radians. NavSpeed is from 0 to 15. NavMode can be either Direct, NavMesh, or Roads. ScenarioType is the name of the scenario type to use.";
            f.MainText = "X, Y, Z, Direction, NavSpeed, NavMode, ScenarioType, ModelSet, Flags";
            if (f.ShowDialog() == DialogResult.Cancel) return;

            var stypes = Scenarios.ScenarioTypes; //these are loaded by Scenarios.Init
            ScenarioType defaulttype = null;
            if (stypes != null)
            {
                defaulttype = stypes.GetScenarioType(1194480618); //"drive";
            }

            AmbientModelSet defaultmodelset = null;
            uint defaultflags = 0;

            ScenarioNode thisnode = null;
            ScenarioNode lastnode = null;
            MCScenarioChainingEdge lastedge = null;

            var str = f.MainText;
            var lines = str.Split('\n');


            if (lines.Length < 2)
            {
                return;//need at least 2 lines (1 point) to work with...
            }



            MCScenarioChain chain = new MCScenarioChain();

            paths.AddChain(chain);


            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var delim = line.Contains(",") ? "," : " ";
                var vals = line.Split(new[] { delim }, StringSplitOptions.RemoveEmptyEntries);
                if (vals.Length < 3) continue;
                if (vals[0].StartsWith("X")) continue;
                Vector3 pos = Vector3.Zero;
                float dir = 0;
                var action = Unk_3609807418.Move;
                var navMode = Unk_3971773454.Direct;
                var navSpeed = Unk_941086046.Unk_00_3279574318;
                var stype = defaulttype;
                var modelset = defaultmodelset;
                var flags = defaultflags;
                var ok = true;
                ok = ok && FloatUtil.TryParse(vals[0].Trim(), out pos.X);
                ok = ok && FloatUtil.TryParse(vals[1].Trim(), out pos.Y);
                ok = ok && FloatUtil.TryParse(vals[2].Trim(), out pos.Z);
                if (vals.Length > 3)
                {
                    ok = ok && FloatUtil.TryParse(vals[3].Trim(), out dir);
                    while (dir > Math.PI) dir -= 2.0f * (float)Math.PI;
                    while (dir < -Math.PI) dir += 2.0f * (float)Math.PI;
                }
                if (vals.Length > 4)
                {
                    byte nsb = 0;
                    byte.TryParse(vals[4].Trim(), out nsb);
                    if (nsb > 15) nsb = 15;
                    navSpeed = (Unk_941086046)nsb;
                }
                if (vals.Length > 5)
                {
                    switch (vals[5].Trim())
                    {
                        case "Direct": navMode = Unk_3971773454.Direct; break;
                        case "NavMesh": navMode = Unk_3971773454.NavMesh; break;
                        case "Roads": navMode = Unk_3971773454.Roads; break;
                    }
                }
                if (vals.Length > 6)
                {
                    var sthash = JenkHash.GenHash(vals[6].Trim().ToLowerInvariant());
                    stype = stypes?.GetScenarioType(sthash) ?? defaulttype;
                }
                if (vals.Length > 7)
                {
                    var mshash = JenkHash.GenHash(vals[7].Trim().ToLowerInvariant());
                    modelset = stypes?.GetPedModelSet(mshash) ?? null;
                    if (modelset == null) modelset = stypes?.GetVehicleModelSet(mshash) ?? null;
                }
                if (vals.Length > 8)
                {
                    if (!uint.TryParse(vals[8].Trim(), out flags)) flags = defaultflags;
                }

                if (!ok) continue;



                thisnode = rgn.AddNode();

                thisnode.MyPoint.Direction = dir;
                thisnode.MyPoint.Type = stype;
                thisnode.MyPoint.ModelSet = modelset;
                thisnode.MyPoint.Flags = (Unk_700327466)flags;

                thisnode.ChainingNode = new MCScenarioChainingNode();
                thisnode.ChainingNode.ScenarioNode = thisnode;
                thisnode.ChainingNode.Chain = chain;
                thisnode.ChainingNode.Type = stype;
                thisnode.ChainingNode.TypeHash = stype?.NameHash ?? 0;
                thisnode.ChainingNode.NotLast = (i < (lines.Length - 1));
                thisnode.ChainingNode.NotFirst = (lastnode != null);

                thisnode.SetPosition(pos);
                thisnode.Orientation = thisnode.MyPoint.Orientation;

                paths.AddNode(thisnode.ChainingNode);


                if (lastnode != null)
                {
                    var edge = new MCScenarioChainingEdge();

                    edge.NodeFrom = lastnode.ChainingNode;
                    edge.NodeTo = thisnode.ChainingNode;
                    edge.NodeIndexFrom = (ushort)lastnode.ChainingNode.NodeIndex;
                    edge.NodeIndexTo = (ushort)thisnode.ChainingNode.NodeIndex;

                    edge.Action = action;
                    edge.NavMode = navMode;
                    edge.NavSpeed = navSpeed;

                    paths.AddEdge(edge);
                    chain.AddEdge(edge);

                    lastedge = edge;
                }


                lastnode = thisnode;
            }






            LoadProjectTree();

            if (lastnode != null)
            {
                ProjectExplorer?.TrySelectScenarioNodeTreeNode(lastnode);
                CurrentScenarioNode = lastnode;
            }

            //CurrentScenarioChainEdge = lastedge;
            //LoadScenarioChainTabPage();
            //LoadScenarioTabPage();
            //LoadScenarioNodeTabPages();


            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }
            else
            {
                CurrentScenario.ScenarioRegion.BuildBVH();
                CurrentScenario.ScenarioRegion.BuildVertices(); //for the graphics...
            }


        }






        public void GetVisibleYmaps(Camera camera, Dictionary<MetaHash, YmapFile> ymaps)
        {
            if (hidegtavmap)
            {
                ymaps.Clear(); //remove all the gtav ymaps.
            }

            if (renderitems && (CurrentProjectFile != null))
            {
                lock (projectsyncroot)
                {
                    for (int i = 0; i < CurrentProjectFile.YmapFiles.Count; i++)
                    {
                        var ymap = CurrentProjectFile.YmapFiles[i];
                        if (ymap.Loaded)
                        {
                            ymaps[ymap._CMapData.name] = ymap;
                        }
                    }
                }
            }
        }
        public void GetVisibleCollisionMeshes(Camera camera, List<BoundsStoreItem> items)
        {
            if (hidegtavmap)
            {
                items.Clear();
            }
        }
        public void GetVisibleWaterQuads(Camera camera, List<WaterQuad> quads)
        {
            if (hidegtavmap)
            {
                quads.Clear();
            }
        }
        public void GetVisibleYnds(Camera camera, List<YndFile> ynds)
        {
            if (hidegtavmap)
            {
                ynds.Clear();
            }

            if (CurrentProjectFile == null) return;

            lock (projectsyncroot)
            {
                visibleynds.Clear();
                for (int i = 0; i < ynds.Count; i++)
                {
                    var ynd = ynds[i];
                    visibleynds[ynd.AreaID] = ynd;
                }

                for (int i = 0; i < CurrentProjectFile.YndFiles.Count; i++)
                {
                    var ynd = CurrentProjectFile.YndFiles[i];
                    if (ynd.Loaded)
                    {
                        visibleynds[ynd.AreaID] = ynd;
                    }
                }

                ynds.Clear();
                foreach (var ynd in visibleynds.Values)
                {
                    ynds.Add(ynd);
                }
            }

        }
        public void GetVisibleYnvs(Camera camera, List<YnvFile> ynvs)
        {
            if (hidegtavmap)
            {
                ynvs.Clear();
            }

            if (CurrentProjectFile == null) return;

            lock (projectsyncroot)
            {
                visibleynvs.Clear();
                for (int i = 0; i < ynvs.Count; i++)
                {
                    var ynv = ynvs[i];
                    visibleynvs[ynv.AreaID] = ynv;
                }

                for (int i = 0; i < CurrentProjectFile.YnvFiles.Count; i++)
                {
                    var ynv = CurrentProjectFile.YnvFiles[i];
                    if (ynv.Loaded)
                    {
                        visibleynvs[ynv.AreaID] = ynv;
                    }
                }

                ynvs.Clear();
                foreach (var ynv in visibleynvs.Values)
                {
                    ynvs.Add(ynv);
                }
            }

        }
        public void GetVisibleTrainTracks(Camera camera, List<TrainTrack> tracks)
        {
            if (hidegtavmap)
            {
                tracks.Clear();
            }


            if (CurrentProjectFile == null) return;

            lock (projectsyncroot)
            {
                visibletrains.Clear();
                for (int i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i];
                    visibletrains[track.Name] = track;
                }

                for (int i = 0; i < CurrentProjectFile.TrainsFiles.Count; i++)
                {
                    var track = CurrentProjectFile.TrainsFiles[i];
                    if (track.Loaded)
                    {
                        visibletrains[track.Name] = track;
                    }
                }

                tracks.Clear();
                foreach (var track in visibletrains.Values)
                {
                    tracks.Add(track);
                }
            }

        }
        public void GetVisibleScenarios(Camera camera, List<YmtFile> ymts)
        {
            if (hidegtavmap)
            {
                ymts.Clear();
            }


            if (CurrentProjectFile == null) return;

            lock (projectsyncroot)
            {
                visiblescenarios.Clear();
                for (int i = 0; i < ymts.Count; i++)
                {
                    var ymt = ymts[i];
                    visiblescenarios[ymt.Name] = ymt;
                }

                for (int i = 0; i < CurrentProjectFile.ScenarioFiles.Count; i++)
                {
                    var scenario = CurrentProjectFile.ScenarioFiles[i];
                    if (scenario.Loaded)
                    {
                        visiblescenarios[scenario.Name] = scenario;
                    }
                }

                ymts.Clear();
                foreach (var ymt in visiblescenarios.Values)
                {
                    ymts.Add(ymt);
                }
            }

        }


        public void OnWorldSelectionChanged(MapSelection sel)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { OnWorldSelectionChanged(sel); }));
                }
                else
                {
                    var ent = sel.EntityDef;
                    var cargen = sel.CarGenerator;
                    var grassbatch = sel.GrassBatch;
                    var pathnode = sel.PathNode;
                    var pathlink = sel.PathLink;
                    var navpoly = sel.NavPoly;
                    var trainnode = sel.TrainTrackNode;
                    var scenariond = sel.ScenarioNode;
                    var scenarioedge = sel.ScenarioEdge;
                    YmapFile ymap = ent?.Ymap ?? cargen?.Ymap ?? grassbatch?.Ymap;
                    YndFile ynd = pathnode?.Ynd;
                    YnvFile ynv = navpoly?.Ynv;
                    TrainTrack traintrack = trainnode?.Track;
                    YmtFile scenario = scenariond?.Ymt ?? scenarioedge?.Region?.Ymt;
                    bool showcurrent = false;

                    if (YmapExistsInProject(ymap))
                    {
                        if (ent != CurrentEntity)
                        {
                            ProjectExplorer?.TrySelectEntityTreeNode(ent);
                        }
                        if (cargen != CurrentCarGen)
                        {
                            ProjectExplorer?.TrySelectCarGenTreeNode(cargen);
                        }
                    }
                    else if (YndExistsInProject(ynd))
                    {
                        if (pathnode != CurrentPathNode)
                        {
                            ProjectExplorer?.TrySelectPathNodeTreeNode(pathnode);
                        }
                    }
                    else if (YnvExistsInProject(ynv))
                    {
                        if (navpoly != CurrentNavPoly)
                        {
                            ProjectExplorer?.TrySelectNavPolyTreeNode(navpoly);
                        }
                    }
                    else if (TrainTrackExistsInProject(traintrack))
                    {
                        if (trainnode != CurrentTrainNode)
                        {
                            ProjectExplorer?.TrySelectTrainNodeTreeNode(trainnode);
                        }
                    }
                    else if (ScenarioExistsInProject(scenario))
                    {
                        if ((scenariond != null) && (scenariond != CurrentScenarioNode))
                        {
                            ProjectExplorer?.TrySelectScenarioNodeTreeNode(scenariond);
                        }
                    }
                    else
                    {
                        ProjectExplorer?.DeselectNode();

                        showcurrent = true;
                    }

                    CurrentYmapFile = ymap;
                    CurrentYtypFile = null;//TODO: interiors!
                    CurrentEntity = ent;
                    CurrentCarGen = cargen;
                    CurrentGrassBatch = grassbatch;
                    CurrentYndFile = ynd;
                    CurrentPathNode = pathnode;
                    CurrentPathLink = pathlink;
                    CurrentYnvFile = ynv;
                    CurrentNavPoly = navpoly;
                    CurrentTrainTrack = traintrack;
                    CurrentTrainNode = trainnode;
                    CurrentScenario = scenario;
                    CurrentScenarioNode = scenariond;
                    CurrentScenarioChainEdge = scenarioedge;
                    RefreshUI();
                    if (showcurrent)
                    {
                        ShowProjectItemInProcess = true;
                        ShowCurrentProjectItem(false);
                        ShowProjectItemInProcess = false;
                    }
                }
            }
            catch { }
        }
        public void OnWorldSelectionModified(MapSelection sel, List<MapSelection> items)
        {
            if (sel.MultipleSelection)
            {
                //TODO!!
            }
            else if (sel.EntityDef != null)
            {
                OnWorldEntityModified(sel.EntityDef);
            }
            else if (sel.CarGenerator != null)
            {
                OnWorldCarGenModified(sel.CarGenerator);
            }
            else if (sel.PathNode != null)
            {
                OnWorldPathNodeModified(sel.PathNode, sel.PathLink);
            }
            else if (sel.NavPoly != null)
            {
                OnWorldNavPolyModified(sel.NavPoly);
            }
            else if (sel.TrainTrackNode != null)
            {
                OnWorldTrainNodeModified(sel.TrainTrackNode);
            }
            else if (sel.ScenarioNode != null)
            {
                OnWorldScenarioNodeModified(sel.ScenarioNode);
            }
        }

        private void OnWorldEntityModified(YmapEntityDef ent)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { OnWorldEntityModified(ent); }));
                }
                else
                {
                    if ((ent.Ymap == null) || (ent.MloParent != null))
                    {
                        return;//TODO: properly handle interior entities!
                    }

                    if (CurrentProjectFile == null)
                    {
                        NewProject();
                    }

                    if (!YmapExistsInProject(ent.Ymap))
                    {
                        ent.Ymap.HasChanged = true;
                        AddYmapToProject(ent.Ymap);
                        ProjectExplorer?.TrySelectEntityTreeNode(ent);
                    }

                    if (ent != CurrentEntity)
                    {
                        CurrentEntity = ent;
                        ProjectExplorer?.TrySelectEntityTreeNode(ent);
                    }

                    if (ent == CurrentEntity)
                    {
                        ShowEditYmapEntityPanel(false);

                        if (ent.Ymap != null)
                        {
                            SetYmapHasChanged(true);
                        }
                    }
                }
            }
            catch { }
        }
        private void OnWorldCarGenModified(YmapCarGen cargen)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { OnWorldCarGenModified(cargen); }));
                }
                else
                {
                    if (cargen?.Ymap == null) return;

                    if (CurrentProjectFile == null)
                    {
                        NewProject();
                    }

                    if (!YmapExistsInProject(cargen.Ymap))
                    {
                        cargen.Ymap.HasChanged = true;
                        AddYmapToProject(cargen.Ymap);
                        ProjectExplorer?.TrySelectCarGenTreeNode(cargen);
                    }

                    if (cargen != CurrentCarGen)
                    {
                        CurrentCarGen = cargen;
                        ProjectExplorer?.TrySelectCarGenTreeNode(cargen);
                    }

                    if (cargen == CurrentCarGen)
                    {
                        ShowEditYmapCarGenPanel(false);

                        ProjectExplorer?.UpdateCarGenTreeNode(cargen);

                        if (cargen.Ymap != null)
                        {
                            SetYmapHasChanged(true);
                        }
                    }

                }
            }
            catch { }
        }
        private void OnWorldPathNodeModified(YndNode node, YndLink link)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { OnWorldPathNodeModified(node, link); }));
                }
                else
                {
                    if (node?.Ynd == null) return;

                    if (CurrentProjectFile == null)
                    {
                        NewProject();
                    }

                    if (!YndExistsInProject(node.Ynd))
                    {
                        node.Ynd.HasChanged = true;
                        AddYndToProject(node.Ynd);
                        ProjectExplorer?.TrySelectPathNodeTreeNode(node);
                    }

                    if (node != CurrentPathNode)
                    {
                        CurrentPathNode = node;
                        ProjectExplorer?.TrySelectPathNodeTreeNode(node);
                    }

                    //////if (link != CurrentPathLink)
                    //////{
                    //////    CurrentPathLink = link;
                    //////    ShowEditYndLinkPanel(false);
                    //////}

                    if (node == CurrentPathNode)
                    {
                        //////ShowEditYndPanel(false);
                        ShowEditYndNodePanel(false);

                        //////UpdatePathNodeTreeNode(node);

                        if (node.Ynd != null)
                        {
                            SetYndHasChanged(true);
                        }
                    }

                }
            }
            catch { }
        }
        private void OnWorldNavPolyModified(YnvPoly poly)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { OnWorldNavPolyModified(poly); }));
                }
                else
                {
                    if (poly?.Ynv == null) return;

                    if (CurrentProjectFile == null)
                    {
                        NewProject();
                    }

                    if (!YnvExistsInProject(poly.Ynv))
                    {
                        poly.Ynv.HasChanged = true;
                        AddYnvToProject(poly.Ynv);
                        ProjectExplorer?.TrySelectNavPolyTreeNode(poly);
                    }

                    if (poly != CurrentNavPoly)
                    {
                        CurrentNavPoly = poly;
                        ProjectExplorer?.TrySelectNavPolyTreeNode(poly);
                    }

                    if (poly == CurrentNavPoly)
                    {
                        ShowEditYnvPolyPanel(false);

                        //////UpdateNavPolyTreeNode(poly);

                        if (poly.Ynv != null)
                        {
                            SetYnvHasChanged(true);
                        }
                    }

                }
            }
            catch { }
        }
        private void OnWorldTrainNodeModified(TrainTrackNode node)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { OnWorldTrainNodeModified(node); }));
                }
                else
                {
                    if (node?.Track == null) return;

                    if (CurrentProjectFile == null)
                    {
                        NewProject();
                    }

                    if (!TrainTrackExistsInProject(node.Track))
                    {
                        node.Track.HasChanged = true;
                        AddTrainTrackToProject(node.Track);
                        ProjectExplorer?.TrySelectTrainNodeTreeNode(node);
                    }

                    if (node != CurrentTrainNode)
                    {
                        CurrentTrainNode = node;
                        ProjectExplorer?.TrySelectTrainNodeTreeNode(node);
                    }

                    if (node == CurrentTrainNode)
                    {
                        ShowEditTrainNodePanel(false);

                        if (node.Track != null)
                        {
                            SetTrainTrackHasChanged(true);
                        }
                    }
                }
            }
            catch { }
        }
        private void OnWorldScenarioNodeModified(ScenarioNode node)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { OnWorldScenarioNodeModified(node); }));
                }
                else
                {
                    if (node?.Ymt == null) return;

                    if (CurrentProjectFile == null)
                    {
                        NewProject();
                    }

                    if (!ScenarioExistsInProject(node.Ymt))
                    {
                        node.Ymt.HasChanged = true;
                        AddScenarioToProject(node.Ymt);
                        ProjectExplorer?.TrySelectScenarioNodeTreeNode(node);
                    }

                    if (node != CurrentScenarioNode)
                    {
                        CurrentScenarioNode = node;
                        ProjectExplorer?.TrySelectScenarioNodeTreeNode(node);
                    }

                    if (node == CurrentScenarioNode)
                    {
                        //ShowEditScenarioPanel(false);
                        ShowEditScenarioNodePanel(false);

                        if (node?.Ymt != null)
                        {
                            SetScenarioHasChanged(true);
                        }
                    }
                }
            }
            catch { }
        }






        public Vector3 GetSpawnPos(float dist)
        {
            Vector3 pos = Vector3.Zero;
            if (WorldForm != null)
            {
                Vector3 campos = WorldForm.GetCameraPosition();
                Vector3 camdir = WorldForm.GetCameraViewDir();
                pos = campos + camdir * dist;
            }
            return pos;
        }



        public void SetProjectHasChanged(bool changed)
        {
            if (CurrentProjectFile == null) return;

            CurrentProjectFile.HasChanged = changed;

            ProjectExplorer?.SetProjectHasChanged(changed);

            UpdateFormTitleText();
        }
        public void SetYmapHasChanged(bool changed)
        {
            if (CurrentYmapFile == null) return;

            bool changechange = changed != CurrentYmapFile.HasChanged;
            if (!changechange) return;

            CurrentYmapFile.HasChanged = changed;

            ProjectExplorer?.SetYmapHasChanged(CurrentYmapFile, changed);

            PromoteIfPreviewPanelActive();
        }
        public void SetYtypHasChanged(bool changed)
        {
            if (CurrentYtypFile == null) return;

            bool changechange = changed != CurrentYtypFile.HasChanged;
            if (!changechange) return;

            CurrentYtypFile.HasChanged = changed;

            ProjectExplorer?.SetYtypHasChanged(CurrentYtypFile, changed);

            PromoteIfPreviewPanelActive();
        }
        public void SetYndHasChanged(bool changed)
        {
            if (CurrentYndFile == null) return;

            bool changechange = changed != CurrentYndFile.HasChanged;
            if (!changechange) return;

            CurrentYndFile.HasChanged = changed;

            ProjectExplorer?.SetYndHasChanged(CurrentYndFile, changed);

            PromoteIfPreviewPanelActive();
        }
        public void SetYnvHasChanged(bool changed)
        {
            if (CurrentYnvFile == null) return;

            bool changechange = changed != CurrentYnvFile.HasChanged;
            if (!changechange) return;

            CurrentYnvFile.HasChanged = changed;

            ProjectExplorer?.SetYnvHasChanged(CurrentYnvFile, changed);

            PromoteIfPreviewPanelActive();
        }
        public void SetTrainTrackHasChanged(bool changed)
        {
            if (CurrentTrainTrack == null) return;

            bool changechange = changed != CurrentTrainTrack.HasChanged;
            if (!changechange) return;

            CurrentTrainTrack.HasChanged = changed;

            ProjectExplorer?.SetTrainTrackHasChanged(CurrentTrainTrack, changed);

            PromoteIfPreviewPanelActive();
        }
        public void SetScenarioHasChanged(bool changed)
        {
            if (CurrentScenario == null) return;

            bool changechange = changed != CurrentScenario.HasChanged;
            if (!changechange) return;

            CurrentScenario.HasChanged = changed;

            ProjectExplorer?.SetScenarioHasChanged(CurrentScenario, changed);

            PromoteIfPreviewPanelActive();
        }


        public RpfFileEntry FindParentYmapEntry(uint hash)
        {
            if (CurrentProjectFile != null)
            {
                foreach (var ymap in CurrentProjectFile.YmapFiles)
                {
                    if ((ymap._CMapData.name.Hash == hash) || (JenkHash.GenHash(Path.GetFileNameWithoutExtension(ymap.Name)) == hash))
                    {
                        return ymap.RpfFileEntry;
                    }
                }
            }

            if ((GameFileCache != null) && (GameFileCache.IsInited))
            {
                return GameFileCache.GetYmapEntry(hash);
            }

            return null;
        }









        //######## Private methods

        private void LoadYmapFromFile(YmapFile ymap, string filename)
        {
            byte[] data = File.ReadAllBytes(filename);

            ymap.Load(data);

            GameFileCache.InitYmapEntityArchetypes(ymap); //this needs to be done after calling YmapFile.Load()
        }
        private void LoadYtypFromFile(YtypFile ytyp, string filename)
        {
            byte[] data = File.ReadAllBytes(filename);

            ytyp.Load(data);
        }
        private void LoadYndFromFile(YndFile ynd, string filename)
        {
            byte[] data = File.ReadAllBytes(filename);

            ynd.Load(data);

            if (WorldForm != null)
            {
                WorldForm.UpdatePathYndGraphics(ynd, true); //links don't get drawn until something changes otherwise
                //note: this is actually necessary to properly populate junctions data........
            }
        }
        private void LoadYnvFromFile(YnvFile ynv, string filename)//TODO!
        {
        }
        private void LoadTrainTrackFromFile(TrainTrack track, string filename)
        {
            byte[] data = File.ReadAllBytes(filename);

            string fname = new FileInfo(filename).Name;

            track.Load(data);
            track.Name = fname;
            track.FilePath = filename;
            track.RpfFileEntry.Name = fname;
            track.RpfFileEntry.NameLower = fname.ToLowerInvariant();

            if (WorldForm != null)
            {
                WorldForm.UpdateTrainTrackGraphics(track, true); //links don't get drawn until something changes otherwise
            }
        }
        private void LoadScenarioFromFile(YmtFile ymt, string filename)
        {
            byte[] data = File.ReadAllBytes(filename);

            ymt.LoadRSC(data);
        }



        private void LoadProjectUI()
        {
            RefreshProjectUI();
            UpdateFormTitleText();
            LoadProjectTree();
            RefreshUI();
        }
        private void LoadProjectTree()
        {
            ProjectExplorer?.LoadProjectTree(CurrentProjectFile);
        }


        private void UpdateFormTitleText()
        {
            if (CurrentProjectFile == null)
            {
                Text = "Project - CodeWalker by dexyfex";
            }
            else
            {
                Text = CurrentProjectFile.Name + " - CodeWalker by dexyfex";
            }
        }

        private void RefreshProjectUI()
        {
            bool enable = (CurrentProjectFile != null);
            FileCloseProjectMenu.Enabled = enable;
            FileSaveProjectMenu.Enabled = enable;
            FileSaveProjectAsMenu.Enabled = enable;
        }
        private void RefreshUI()
        {
            RefreshYmapUI();
            RefreshEntityUI();
            RefreshCarGenUI();
            RefreshYtypUI();
            RefreshYndUI();
            RefreshYnvUI();
            RefreshTrainTrackUI();
            RefreshScenarioUI();
            SetCurrentSaveItem();
            //ShowEditYmapPanel(false);
            //ShowEditYmapEntityPanel(false);
            //ShowEditYmapCarGenPanel(false);
            //ShowEditYtypPanel(false);
            //ShowEditYndPanel(false);
            //ShowEditYnvPanel(false);
            //ShowEditYndNodePanel(false);
            //ShowEditTrainTrackPanel(false);
            //ShowEditTrainNodePanel(false);
            //ShowEditScenarioPanel(false);
            //ShowEditScenarioNodePanel(false);
        }
        private void RefreshYmapUI()
        {
            bool enable = (CurrentYmapFile != null);
            bool inproj = YmapExistsInProject(CurrentYmapFile);

            YmapNewEntityMenu.Enabled = enable && inproj;
            YmapNewCarGenMenu.Enabled = enable && inproj;

            if (CurrentYmapFile != null)
            {
                YmapNameMenu.Text = "(" + CurrentYmapFile.Name + ")";
            }
            else
            {
                YmapNameMenu.Text = "(No .ymap file selected)";
            }

            YmapAddToProjectMenu.Enabled = enable && !inproj;
            YmapRemoveFromProjectMenu.Enabled = inproj;
            YmapMenu.Visible = enable;

            if (WorldForm != null)
            {
                WorldForm.EnableYmapUI(enable, CurrentYmapFile?.Name ?? "");
            }
        }
        private void RefreshEntityUI()
        {
            //bool enable = (CurrentEntity != null);
            //bool isinproj = false;

            //if (CurrentEntity != null)
            //{
            //    isinproj = YmapExistsInProject(CurrentEntity.Ymap);
            //}

            //EntityAddToProjectButton.Enabled = !isinproj;
            //EntityDeleteButton.Enabled = isinproj;
        }
        private void RefreshCarGenUI()
        {
            //bool enable = (CurrentCarGen != null);
            //bool isinproj = false;

            //if (CurrentCarGen != null)
            //{
            //    isinproj = YmapExistsInProject(CurrentCarGen.Ymap);
            //}

            //CarAddToProjectButton.Enabled = !isinproj;
            //CarDeleteButton.Enabled = isinproj;
        }
        private void RefreshYtypUI()
        {
            bool enable = (CurrentYtypFile != null);
            bool inproj = YtypExistsInProject(CurrentYtypFile);

            YtypNewArchetypeMenu.Enabled = enable && inproj;

            if (CurrentYtypFile != null)
            {
                YtypNameMenu.Text = "(" + CurrentYtypFile.Name + ")";
            }
            else
            {
                YtypNameMenu.Text = "(No .ytyp file selected)";
            }

            YtypAddToProjectMenu.Enabled = enable && !inproj;
            YtypRemoveFromProjectMenu.Enabled = inproj;
            YtypMenu.Visible = enable;

            if (WorldForm != null)
            {
                //WorldForm.EnableYtypUI(enable, CurrentYtypFile?.Name ?? "");
            }
        }
        private void RefreshYndUI()
        {
            bool enable = (CurrentYndFile != null);
            bool inproj = YndExistsInProject(CurrentYndFile);

            YndNewNodeMenu.Enabled = enable && inproj;

            if (CurrentYndFile != null)
            {
                YndNameMenu.Text = "(" + CurrentYndFile.Name + ")";
            }
            else
            {
                YndNameMenu.Text = "(No .ynd file selected)";
            }

            YndAddToProjectMenu.Enabled = enable && !inproj;
            YndRemoveFromProjectMenu.Enabled = inproj;
            YndMenu.Visible = enable;

            if (WorldForm != null)
            {
                WorldForm.EnableYndUI(enable, CurrentYndFile?.Name ?? "");
            }
        }
        private void RefreshYnvUI()
        {
            bool enable = (CurrentYnvFile != null);
            bool inproj = YnvExistsInProject(CurrentYnvFile);

            YnvNewPolygonMenu.Enabled = enable && inproj;

            if (CurrentYnvFile != null)
            {
                YnvNameMenu.Text = "(" + CurrentYnvFile.Name + ")";
            }
            else
            {
                YnvNameMenu.Text = "(No .ynv file selected)";
            }

            YnvAddToProjectMenu.Enabled = enable && !inproj;
            YnvRemoveFromProjectMenu.Enabled = inproj;
            YnvMenu.Visible = enable;

            if (WorldForm != null)
            {
                WorldForm.EnableYnvUI(enable, CurrentYnvFile?.Name ?? "");
            }
        }
        private void RefreshTrainTrackUI()
        {
            bool enable = (CurrentTrainTrack != null);
            bool inproj = TrainTrackExistsInProject(CurrentTrainTrack);

            TrainsNewNodeMenu.Enabled = enable && inproj;

            if (CurrentTrainTrack != null)
            {
                TrainsNameMenu.Text = "(" + CurrentTrainTrack.Name + ")";
            }
            else
            {
                TrainsNameMenu.Text = "(No train track selected)";
            }

            TrainsAddToProjectMenu.Enabled = enable && !inproj;
            TrainsRemoveFromProjectMenu.Enabled = inproj;
            TrainsMenu.Visible = enable;

            if (WorldForm != null)
            {
                WorldForm.EnableTrainsUI(enable, CurrentTrainTrack?.Name ?? "");
            }
        }
        private void RefreshScenarioUI()
        {
            bool enable = (CurrentScenario != null);
            bool inproj = ScenarioExistsInProject(CurrentScenario);

            ScenarioNewPointMenu.Enabled = enable && inproj;
            ScenarioNewPointFromSelectedMenu.Enabled = enable && inproj && (CurrentScenarioNode != null);
            ScenarioNewEntityOverrideMenu.Enabled = enable && inproj;
            ScenarioNewChainMenu.Enabled = enable && inproj;
            ScenarioNewClusterMenu.Enabled = enable && inproj;
            ScenarioImportChainMenu.Enabled = enable && inproj;

            if (CurrentScenario != null)
            {
                ScenarioNameMenu.Text = "(" + CurrentScenario.Name + ")";
            }
            else
            {
                ScenarioNameMenu.Text = "(No scenario region selected)";
            }

            ScenarioAddToProjectMenu.Enabled = enable && !inproj;
            ScenarioRemoveFromProjectMenu.Enabled = inproj;
            ScenarioMenu.Visible = enable;

            if (WorldForm != null)
            {
                WorldForm.EnableScenarioUI(enable, CurrentScenario?.Name ?? "");
            }
        }


        private void SetCurrentSaveItem()
        {
            string filename = null;
            if (CurrentYmapFile != null)
            {
                filename = CurrentYmapFile.RpfFileEntry?.Name;
            }
            else if (CurrentYtypFile != null)
            {
                filename = CurrentYtypFile.RpfFileEntry?.Name;
            }
            else if (CurrentYndFile != null)
            {
                filename = CurrentYndFile.RpfFileEntry?.Name;
            }
            else if (CurrentYnvFile != null)
            {
                filename = CurrentYnvFile.RpfFileEntry?.Name;
            }
            else if (CurrentTrainTrack != null)
            {
                filename = CurrentTrainTrack.RpfFileEntry?.Name;
            }
            else if (CurrentScenario != null)
            {
                filename = CurrentScenario.RpfFileEntry?.Name;
            }

            bool enable = !string.IsNullOrEmpty(filename);

            if (enable)
            {
                FileSaveItemMenu.Text = "Save " + filename;
                FileSaveItemAsMenu.Text = "Save " + filename + " As...";
                ToolbarSaveButton.Text = "Save " + filename;
            }
            else
            {
                FileSaveItemMenu.Text = "Save";
                FileSaveItemAsMenu.Text = "Save As...";
                ToolbarSaveButton.Text = "Save";
            }
            
            FileSaveItemMenu.Tag = filename;
            FileSaveItemAsMenu.Tag = filename;

            FileSaveItemMenu.Enabled = enable;
            FileSaveItemMenu.Visible = enable;
            FileSaveItemAsMenu.Enabled = enable;
            FileSaveItemAsMenu.Visible = enable;
            ToolbarSaveButton.Enabled = enable;

            if (WorldForm != null)
            {
                WorldForm.SetCurrentSaveItem(filename);
            }
        }








        private string ShowSaveDialog(string filter, string filename)
        {
            SaveFileDialog.FileName = filename;
            SaveFileDialog.Filter = filter;
            if (SaveFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return string.Empty;
            }
            return SaveFileDialog.FileName;
        }
        private string ShowOpenDialog(string filter, string filename)
        {
            OpenFileDialog.FileName = filename;
            OpenFileDialog.Filter = filter;
            OpenFileDialog.Multiselect = false;
            if (OpenFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return string.Empty;
            }
            return OpenFileDialog.FileName;
        }
        private string[] ShowOpenDialogMulti(string filter, string filename)
        {
            OpenFileDialog.FileName = filename;
            OpenFileDialog.Filter = filter;
            OpenFileDialog.Multiselect = true;
            if (OpenFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return null;
            }
            return OpenFileDialog.FileNames;
        }







        //######## events

        private void ProjectForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseProject();
        }
        private void ProjectForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (WorldForm != null)
            {
                WorldForm.OnProjectFormClosed();
            }
        }

        private void ProjectExplorer_OnItemSelected(object item)
        {
            ShowProjectItem(item, false);
        }
        private void ProjectExplorer_OnItemActivated(object item)
        {
            //promote from preview panel to full panel...
            ShowProjectItem(item, true);
        }

        private void MainDockPanel_ActiveContentChanged(object sender, EventArgs e)
        {
            if (!ShowProjectItemInProcess)
            {
                var panel = MainDockPanel.ActiveContent as ProjectPanel;
                if (panel != null)
                {
                    MainDockPanel.DefaultFloatWindowSize = panel.Size;
                }
                if (panel?.Tag != null)
                {
                    SetProjectItem(panel.Tag);
                    RefreshUI();
                }
            }
        }
        private void MainDockPanel_DocumentDragged(object sender, EventArgs e)
        {
            PromoteIfPreviewPanel(MainDockPanel.ActiveContent);
        }

        private void FileNewProjectMenu_Click(object sender, EventArgs e)
        {
            NewProject();
        }
        private void FileNewYmapMenu_Click(object sender, EventArgs e)
        {
            NewYmap();
        }
        private void FileNewYtypMenu_Click(object sender, EventArgs e)
        {
            NewYtyp();
        }
        private void FileNewYndMenu_Click(object sender, EventArgs e)
        {
            NewYnd();
        }
        private void FileNewYnvMenu_Click(object sender, EventArgs e)
        {
            NewYnv();
        }
        private void FileNewTrainsMenu_Click(object sender, EventArgs e)
        {
            NewTrainTrack();
        }
        private void FileNewScenarioMenu_Click(object sender, EventArgs e)
        {
            NewScenario();
        }
        private void FileOpenProjectMenu_Click(object sender, EventArgs e)
        {
            OpenProject();
        }
        private void FileOpenYmapMenu_Click(object sender, EventArgs e)
        {
            OpenYmap();
        }
        private void FileOpenYtypMenu_Click(object sender, EventArgs e)
        {
            OpenYtyp();
        }
        private void FileOpenYndMenu_Click(object sender, EventArgs e)
        {
            OpenYnd();
        }
        private void FileOpenYnvMenu_Click(object sender, EventArgs e)
        {
            OpenYnv();
        }
        private void FileOpenTrainsMenu_Click(object sender, EventArgs e)
        {
            OpenTrainTrack();
        }
        private void FileOpenScenarioMenu_Click(object sender, EventArgs e)
        {
            OpenScenario();
        }
        private void FileCloseProjectMenu_Click(object sender, EventArgs e)
        {
            CloseProject();
        }
        private void FileSaveProjectMenu_Click(object sender, EventArgs e)
        {
            SaveProject();
        }
        private void FileSaveProjectAsMenu_Click(object sender, EventArgs e)
        {
            SaveProject(true);
        }
        private void FileSaveItemMenu_Click(object sender, EventArgs e)
        {
            SaveCurrentItem();
        }
        private void FileSaveItemAsMenu_Click(object sender, EventArgs e)
        {
            SaveCurrentItem(true);
        }

        private void ViewProjectExplorerMenu_Click(object sender, EventArgs e)
        {
            ShowProjectExplorer();
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

        private void YmapNewEntityMenu_Click(object sender, EventArgs e)
        {
            NewEntity();
        }
        private void YmapNewCarGenMenu_Click(object sender, EventArgs e)
        {
            NewCarGen();
        }
        private void YmapAddToProjectMenu_Click(object sender, EventArgs e)
        {
            AddYmapToProject(CurrentYmapFile);
        }
        private void YmapRemoveFromProjectMenu_Click(object sender, EventArgs e)
        {
            RemoveYmapFromProject();
        }

        private void YtypNewArchetypeMenu_Click(object sender, EventArgs e)
        {
            //NewArchetype();
        }
        private void YtypAddToProjectMenu_Click(object sender, EventArgs e)
        {
            AddYtypToProject(CurrentYtypFile);
        }
        private void YtypRemoveFromProjectMenu_Click(object sender, EventArgs e)
        {
            RemoveYtypFromProject();
        }

        private void YndNewNodeMenu_Click(object sender, EventArgs e)
        {
            NewPathNode();
        }
        private void YndAddToProjectMenu_Click(object sender, EventArgs e)
        {
            AddYndToProject(CurrentYndFile);
        }
        private void YndRemoveFromProjectMenu_Click(object sender, EventArgs e)
        {
            RemoveYndFromProject();
        }

        private void YnvNewPolygonMenu_Click(object sender, EventArgs e)
        {
            NewNavPoly();
        }
        private void YnvAddToProjectMenu_Click(object sender, EventArgs e)
        {
            AddYnvToProject(CurrentYnvFile);
        }
        private void YnvRemoveFromProjectMenu_Click(object sender, EventArgs e)
        {
            RemoveYnvFromProject();
        }

        private void TrainsNewNodeMenu_Click(object sender, EventArgs e)
        {
            NewTrainNode();
        }
        private void TrainsAddToProjectMenu_Click(object sender, EventArgs e)
        {
            AddTrainTrackToProject(CurrentTrainTrack);
        }
        private void TrainsRemoveFromProjectMenu_Click(object sender, EventArgs e)
        {
            RemoveTrainTrackFromProject();
        }

        private void ScenarioNewPointMenu_Click(object sender, EventArgs e)
        {
            CurrentScenarioNode = null;
            NewScenarioNode();
        }
        private void ScenarioNewPointFromSelectedMenu_Click(object sender, EventArgs e)
        {
            NewScenarioNode();
        }
        private void ScenarioNewEntityOverrideMenu_Click(object sender, EventArgs e)
        {
            AddScenarioEntity();
        }
        private void ScenarioNewChainMenu_Click(object sender, EventArgs e)
        {
            AddScenarioChain();
        }
        private void ScenarioNewClusterMenu_Click(object sender, EventArgs e)
        {
            AddScenarioCluster();
        }
        private void ScenarioImportChainMenu_Click(object sender, EventArgs e)
        {
            ImportScenarioChain();
        }
        private void ScenarioAddToProjectMenu_Click(object sender, EventArgs e)
        {
            AddScenarioToProject(CurrentScenario);
        }
        private void ScenarioRemoveFromProjectMenu_Click(object sender, EventArgs e)
        {
            RemoveScenarioFromProject();
        }

        private void ToolsManifestGeneratorMenu_Click(object sender, EventArgs e)
        {
            ShowEditProjectManifestPanel(false);
        }
        private void ToolsImportMenyooXmlMenu_Click(object sender, EventArgs e)
        {
            ImportMenyooXml();
        }

        private void RenderShowGtavMapMenu_Click(object sender, EventArgs e)
        {
            RenderShowGtavMapMenu.Checked = !RenderShowGtavMapMenu.Checked;
            hidegtavmap = !RenderShowGtavMapMenu.Checked;
        }
        private void RenderShowProjectItemsMenu_Click(object sender, EventArgs e)
        {
            RenderShowProjectItemsMenu.Checked = !RenderShowProjectItemsMenu.Checked;
            renderitems = RenderShowProjectItemsMenu.Checked;
        }

        private void ToolbarNewButton_ButtonClick(object sender, EventArgs e)
        {
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            else
            {
                NewYmap();
            }
        }
        private void ToolbarNewProjectMenu_Click(object sender, EventArgs e)
        {
            NewProject();
        }
        private void ToolbarNewYmapMenu_Click(object sender, EventArgs e)
        {
            NewYmap();
        }
        private void ToolbarNewYtypMenu_Click(object sender, EventArgs e)
        {
            NewYtyp();
        }
        private void ToolbarNewYndMenu_Click(object sender, EventArgs e)
        {
            NewYnd();
        }
        private void ToolbarNewYnvMenu_Click(object sender, EventArgs e)
        {
            NewYnv();
        }
        private void ToolbarNewTrainsMenu_Click(object sender, EventArgs e)
        {
            NewTrainTrack();
        }
        private void ToolbarNewScenarioMenu_Click(object sender, EventArgs e)
        {
            NewScenario();
        }
        private void ToolbarOpenButton_ButtonClick(object sender, EventArgs e)
        {
            if (CurrentProjectFile == null)
            {
                OpenProject();
            }
            else
            {
                OpenYmap();
            }
        }
        private void ToolbarOpenProjectMenu_Click(object sender, EventArgs e)
        {
            OpenProject();
        }
        private void ToolbarOpenYmapMenu_Click(object sender, EventArgs e)
        {
            OpenYmap();
        }
        private void ToolbarOpenYtypMenu_Click(object sender, EventArgs e)
        {
            OpenYtyp();
        }
        private void ToolbarOpenYndMenu_Click(object sender, EventArgs e)
        {
            OpenYnd();
        }
        private void ToolbarOpenYnvMenu_Click(object sender, EventArgs e)
        {
            OpenYnv();
        }
        private void ToolbarOpenTrainsMenu_Click(object sender, EventArgs e)
        {
            OpenTrainTrack();
        }
        private void ToolbarOpenScenarioMenu_Click(object sender, EventArgs e)
        {
            OpenScenario();
        }
        private void ToolbarSaveButton_Click(object sender, EventArgs e)
        {
            Save();
        }
        private void ToolbarSaveAllButton_Click(object sender, EventArgs e)
        {
            SaveAll();
        }
    }
}
