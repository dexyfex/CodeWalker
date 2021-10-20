using CodeWalker.GameFiles;
using CodeWalker.Project.Panels;
using CodeWalker.Properties;
using CodeWalker.Utils;
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

        private MapSelection[] CurrentMulti;

        private YmapFile CurrentYmapFile;
        private YmapEntityDef CurrentEntity;
        private YmapCarGen CurrentCarGen;
        private YmapLODLight CurrentLodLight;
        private YmapBoxOccluder CurrentBoxOccluder;
        private YmapOccludeModel CurrentOccludeModel;
        private YmapOccludeModelTriangle CurrentOccludeModelTri;
        private YmapGrassInstanceBatch CurrentGrassBatch;

        private YtypFile CurrentYtypFile;
        private Archetype CurrentArchetype;
        private MCEntityDef CurrentMloEntity;
        private MCMloRoomDef CurrentMloRoom;
        private MCMloPortalDef CurrentMloPortal;
        private MCMloEntitySet CurrentMloEntitySet;

        private YndFile CurrentYndFile;
        private YndNode CurrentPathNode;
        private YndLink CurrentPathLink;

        private YnvFile CurrentYnvFile;
        private YnvPoly CurrentNavPoly;
        private YnvPoint CurrentNavPoint;
        private YnvPortal CurrentNavPortal;

        private TrainTrack CurrentTrainTrack;
        private TrainTrackNode CurrentTrainNode;

        private YmtFile CurrentScenario;
        private ScenarioNode CurrentScenarioNode;
        private MCScenarioChainingEdge CurrentScenarioChainEdge;

        private RelFile CurrentAudioFile;
        private AudioPlacement CurrentAudioZone;
        private AudioPlacement CurrentAudioEmitter;
        private Dat151AmbientZoneList CurrentAudioZoneList;
        private Dat151AmbientEmitterList CurrentAudioEmitterList;
        private Dat151Interior CurrentAudioInterior;
        private Dat151InteriorRoom CurrentAudioInteriorRoom;

        private YbnFile CurrentYbnFile;
        private Bounds CurrentCollisionBounds;
        private BoundPolygon CurrentCollisionPoly;
        private BoundVertex CurrentCollisionVertex;

        private bool renderitems = true;
        private bool hidegtavmap = false;
        private bool autoymapflags = true;
        private bool autoymapextents = true;

        private object projectsyncroot = new object();
        public object ProjectSyncRoot { get { return projectsyncroot; } }

        private Dictionary<string, YbnFile> visibleybns = new Dictionary<string, YbnFile>();
        private Dictionary<int, YndFile> visibleynds = new Dictionary<int, YndFile>();
        private Dictionary<int, YnvFile> visibleynvs = new Dictionary<int, YnvFile>();
        private Dictionary<string, TrainTrack> visibletrains = new Dictionary<string, TrainTrack>();
        private Dictionary<string, YmtFile> visiblescenarios = new Dictionary<string, YmtFile>();
        private Dictionary<uint, YmapEntityDef> visiblemloentities = new Dictionary<uint, YmapEntityDef>();
        private Dictionary<uint, RelFile> visibleaudiofiles = new Dictionary<uint, RelFile>();

        private Dictionary<uint, Archetype> projectarchetypes = new Dictionary<uint, Archetype>();//used to override archetypes in world view
        private Dictionary<uint, YbnFile> projectybns = new Dictionary<uint, YbnFile>();//used for handling interior ybns

        private List<YmapEntityDef> interiorslist = new List<YmapEntityDef>(); //used for handling interiors ybns

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
        public void ShowDefaultPanels()
        {
            ShowProjectExplorer();
            ShowWelcomePanel();
        }
        public void ShowProjectExplorer()
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
        public void ShowWelcomePanel()
        {
            ShowPreviewPanel(() => { return new WelcomePanel(); });
        }
        public void ShowPreviewPanel<T>(Func<T> createFunc, Action<T> updateAction = null) where T : ProjectPanel
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
        public void ShowPanel<T>(bool promote, Func<T> createFunc, Action<T> updateAction, Func<T,bool> findFunc) where T : ProjectPanel
        {
            T found = FindPanel(findFunc);
            if ((found != null) && (found != PreviewPanel))
            {
                if (found.IsHidden)
                {
                    found.Show();
                }
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
        public void ShowEditProjectPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditProjectPanel(this); }, //createFunc
                (panel) => { panel.SetProject(CurrentProjectFile); },  //updateFunc
                (panel) => { return true; }); //findFunc
        }
        public void ShowEditProjectManifestPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditProjectManifestPanel(this); }, //createFunc
                (panel) => { panel.SetProject(CurrentProjectFile); }, //updateFunc
                (panel) => { return true; }); //findFunc
        }
        public void ShowGenerateLODLightsPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new GenerateLODLightsPanel(this); }, //createFunc
                (panel) => { panel.SetProject(CurrentProjectFile); }, //updateFunc
                (panel) => { return true; }); //findFunc
        }
        public void ShowGenerateNavMeshPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new GenerateNavMeshPanel(this); }, //createFunc
                (panel) => { panel.SetProject(CurrentProjectFile); }, //updateFunc
                (panel) => { return true; }); //findFunc
        }
        public void ShowEditMultiPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditMultiPanel(this); }, //createFunc
                (panel) => { panel.SetItems(CurrentMulti); }, //updateFunc
                (panel) => { return panel.Items == CurrentMulti; }); //findFunc
        }
        public void ShowEditYmapPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapPanel(this); }, //createFunc
                (panel) => { panel.SetYmap(CurrentYmapFile); }, //updateFunc
                (panel) => { return panel.Ymap == CurrentYmapFile; }); //findFunc
        }
        public void ShowEditYmapEntityPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapEntityPanel(this); }, //createFunc
                (panel) => { panel.SetEntity(CurrentEntity); }, //updateFunc
                (panel) => { return panel.CurrentEntity == CurrentEntity; }); //findFunc
        }
        public void ShowEditYmapCarGenPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapCarGenPanel(this); }, //createFunc
                (panel) => { panel.SetCarGen(CurrentCarGen); }, //updateFunc
                (panel) => { return panel.CurrentCarGen == CurrentCarGen; }); //findFunc
        }
        public void ShowEditYmapLodLightPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapLodLightPanel(this); }, //createFunc
                (panel) => { panel.SetLodLight(CurrentLodLight); }, //updateFunc
                (panel) => { return panel.CurrentLodLight == CurrentLodLight; }); //findFunc
        }
        public void ShowEditYmapBoxOccluderPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapBoxOccluderPanel(this); }, //createFunc
                (panel) => { panel.SetBoxOccluder(CurrentBoxOccluder); }, //updateFunc
                (panel) => { return panel.CurrentBoxOccluder == CurrentBoxOccluder; }); //findFunc
        }
        public void ShowEditYmapOccludeModelPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapOccludeModelPanel(this); }, //createFunc
                (panel) => { panel.SetOccludeModel(CurrentOccludeModel); }, //updateFunc
                (panel) => { return panel.CurrentOccludeModel == CurrentOccludeModel; }); //findFunc
        }
        public void ShowEditYmapOccludeModelTrianglePanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapOccludeModelPanel(this); }, //createFunc
                (panel) => { panel.SetOccludeModelTriangle(CurrentOccludeModelTri); }, //updateFunc
                (panel) => { return panel.CurrentOccludeModel == CurrentOccludeModel; }); //findFunc
        }
        public void ShowEditYmapGrassBatchPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYmapGrassPanel(this); }, //createFunc
                (panel) => { panel.SetBatch(CurrentGrassBatch); }, //updateFunc
                (panel) => { return panel.CurrentBatch == CurrentGrassBatch; }); //findFunc
        }
        public void ShowEditYtypPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYtypPanel(this); }, //createFunc
                (panel) => { panel.SetYtyp(CurrentYtypFile); }, //updateFunc
                (panel) => { return panel.Ytyp == CurrentYtypFile; }); //findFunc
        }
        public void ShowEditArchetypePanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYtypArchetypePanel(this); }, //createFunc
                (panel) => { panel.SetArchetype(CurrentArchetype); }, //updateFunc
                (panel) => { return panel.CurrentArchetype == CurrentArchetype; }); //findFunc
        }
        public void ShowEditYbnPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYbnPanel(this); }, //createFunc
                (panel) => { panel.SetYbn(CurrentYbnFile); }, //updateFunc
                (panel) => { return panel.Ybn == CurrentYbnFile; }); //findFunc
        }
        public void ShowEditYbnBoundsPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYbnBoundsPanel(this); }, //createFunc
                (panel) => { panel.SetCollisionBounds(CurrentCollisionBounds); }, //updateFunc
                (panel) => { return panel.CollisionBounds == CurrentCollisionBounds; }); //findFunc
        }
        public void ShowEditYbnBoundPolyPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYbnBoundPolyPanel(this); }, //createFunc
                (panel) => { panel.SetCollisionPoly(CurrentCollisionPoly); }, //updateFunc
                (panel) => { return panel.CollisionPoly == CurrentCollisionPoly; }); //findFunc
        }
        public void ShowEditYbnBoundVertexPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYbnBoundVertexPanel(this); }, //createFunc
                (panel) => { panel.SetCollisionVertex(CurrentCollisionVertex); }, //updateFunc
                (panel) => { return panel.CollisionVertex == CurrentCollisionVertex; }); //findFunc
        }
        public void ShowEditYndPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYndPanel(this); }, //createFunc
                (panel) => { panel.SetYnd(CurrentYndFile); }, //updateFunc
                (panel) => { return panel.Ynd == CurrentYndFile; }); //findFunc
        }
        public void ShowEditYndNodePanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYndNodePanel(this); }, //createFunc
                (panel) => { panel.SetPathNode(CurrentPathNode); }, //updateFunc
                (panel) => { return panel.CurrentPathNode == CurrentPathNode; }); //findFunc
        }
        public void ShowEditYnvPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYnvPanel(this); }, //createFunc
                (panel) => { panel.SetYnv(CurrentYnvFile); }, //updateFunc
                (panel) => { return panel.Ynv == CurrentYnvFile; }); //findFunc
        }
        public void ShowEditYnvPolyPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYnvPolyPanel(this); }, //createFunc
                (panel) => { panel.SetYnvPoly(CurrentNavPoly); }, //updateFunc
                (panel) => { return panel.YnvPoly == CurrentNavPoly; }); //findFunc
        }
        public void ShowEditYnvPointPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYnvPointPanel(this); }, //createFunc
                (panel) => { panel.SetYnvPoint(CurrentNavPoint); }, //updateFunc
                (panel) => { return panel.YnvPoint == CurrentNavPoint; }); //findFunc
        }
        public void ShowEditYnvPortalPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYnvPortalPanel(this); }, //createFunc
                (panel) => { panel.SetYnvPortal(CurrentNavPortal); }, //updateFunc
                (panel) => { return panel.YnvPortal == CurrentNavPortal; }); //findFunc
        }
        public void ShowEditTrainTrackPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditTrainTrackPanel(this); }, //createFunc
                (panel) => { panel.SetTrainTrack(CurrentTrainTrack); }, //updateFunc
                (panel) => { return panel.Track == CurrentTrainTrack; }); //findFunc
        }
        public void ShowEditTrainNodePanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditTrainNodePanel(this); }, //createFunc
                (panel) => { panel.SetTrainNode(CurrentTrainNode); }, //updateFunc
                (panel) => { return panel.TrainNode == CurrentTrainNode; }); //findFunc
        }
        public void ShowEditScenarioYmtPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditScenarioYmtPanel(this); }, //createFunc
                (panel) => { panel.SetScenarioYmt(CurrentScenario); }, //updateFunc
                (panel) => { return panel.CurrentScenario == CurrentScenario; }); //findFunc
        }
        public void ShowEditScenarioNodePanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditScenarioNodePanel(this); }, //createFunc
                (panel) => { panel.SetScenarioNode(CurrentScenarioNode); }, //updateFunc
                (panel) => { return panel.CurrentScenarioNode == CurrentScenarioNode; }); //findFunc
        }
        public void ShowEditYtypMloRoomPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYtypMloRoomPanel(this); }, //createFunc
                (panel) => { panel.SetRoom(CurrentMloRoom); }, //updateFunc
                (panel) => { return panel.CurrentRoom == CurrentMloRoom; }); //findFunc
        }
        public void ShowEditYtypMloPortalPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYtypMloPortalPanel(this); }, //createFunc
                (panel) => { panel.SetPortal(CurrentMloPortal); }, //updateFunc
                (panel) => { return panel.CurrentPortal == CurrentMloPortal; }); //findFunc
        }
        public void ShowEditYtypMloEntSetPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditYtypMloEntSetPanel(this); }, //createFunc
                (panel) => { panel.SetEntitySet(CurrentMloEntitySet); }, //updateFunc
                (panel) => { return panel.CurrentEntitySet == CurrentMloEntitySet; }); //findFunc
        }
        public void ShowEditAudioFilePanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditAudioFilePanel(this); }, //createFunc
                (panel) => { panel.SetFile(CurrentAudioFile); }, //updateFunc
                (panel) => { return panel.CurrentFile == CurrentAudioFile; }); //findFunc
        }
        public void ShowEditAudioZonePanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditAudioZonePanel(this); }, //createFunc
                (panel) => { panel.SetZone(CurrentAudioZone); }, //updateFunc
                (panel) => { return panel.CurrentZone?.AudioZone == CurrentAudioZone?.AudioZone; }); //findFunc
        }
        public void ShowEditAudioEmitterPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditAudioEmitterPanel(this); }, //createFunc
                (panel) => { panel.SetEmitter(CurrentAudioEmitter); }, //updateFunc
                (panel) => { return panel.CurrentEmitter?.AudioEmitter == CurrentAudioEmitter?.AudioEmitter; }); //findFunc
        }
        public void ShowEditAudioZoneListPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditAudioZoneListPanel(this); }, //createFunc
                (panel) => { panel.SetZoneList(CurrentAudioZoneList); }, //updateFunc
                (panel) => { return panel.CurrentZoneList == CurrentAudioZoneList; }); //findFunc
        }
        public void ShowEditAudioEmitterListPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditAudioEmitterListPanel(this); }, //createFunc
                (panel) => { panel.SetEmitterList(CurrentAudioEmitterList); }, //updateFunc
                (panel) => { return panel.CurrentEmitterList == CurrentAudioEmitterList; }); //findFunc
        }
        public void ShowEditAudioInteriorPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditAudioInteriorPanel(this); }, //createFunc
                (panel) => { panel.SetInterior(CurrentAudioInterior); }, //updateFunc
                (panel) => { return panel.CurrentInterior == CurrentAudioInterior; }); //findFunc
        }
        public void ShowEditAudioInteriorRoomPanel(bool promote)
        {
            ShowPanel(promote,
                () => { return new EditAudioInteriorRoomPanel(this); }, //createFunc
                (panel) => { panel.SetRoom(CurrentAudioInteriorRoom); }, //updateFunc
                (panel) => { return panel.CurrentRoom == CurrentAudioInteriorRoom; }); //findFunc
        }

        private void ShowCurrentProjectItem(bool promote)
        {
            if (CurrentMulti != null)
            {
                ShowEditMultiPanel(promote);
            }
            else if (CurrentMloEntity != null)
            {
                ShowEditYmapEntityPanel(promote);
            }
            else if (CurrentMloRoom != null)
            {
                ShowEditYtypMloRoomPanel(promote);
            }
            else if (CurrentMloPortal != null)
            {
                ShowEditYtypMloPortalPanel(promote);
            }
            else if (CurrentMloEntitySet != null)
            {
                ShowEditYtypMloEntSetPanel(promote);
            }
            else if (CurrentYbnFile != null)
            {
                if (CurrentCollisionVertex != null)
                {
                    ShowEditYbnBoundVertexPanel(promote);
                }
                else if (CurrentCollisionPoly != null)
                {
                    ShowEditYbnBoundPolyPanel(promote);
                }
                else if (CurrentCollisionBounds != null)
                {
                    ShowEditYbnBoundsPanel(promote);
                }
                else
                {
                    ShowEditYbnPanel(promote);
                }
            }
            else if (CurrentEntity != null)
            {
                ShowEditYmapEntityPanel(promote);
            }
            else if (CurrentArchetype != null)
            {
                ShowEditArchetypePanel(promote);
            }
            else if (CurrentCarGen != null)
            {
                ShowEditYmapCarGenPanel(promote);
            }
            else if (CurrentLodLight != null)
            {
                ShowEditYmapLodLightPanel(promote);
            }
            else if (CurrentBoxOccluder != null)
            {
                ShowEditYmapBoxOccluderPanel(promote);
            }
            else if (CurrentOccludeModelTri != null)
            {
                ShowEditYmapOccludeModelTrianglePanel(promote);
            }
            else if (CurrentOccludeModel != null)
            {
                ShowEditYmapOccludeModelPanel(promote);
            }
            else if (CurrentGrassBatch != null)
            {
                ShowEditYmapGrassBatchPanel(promote);
            }
            else if (CurrentYmapFile != null)
            {
                ShowEditYmapPanel(promote);
            }
            else if (CurrentYtypFile != null)
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
            else if (CurrentNavPoint != null)
            {
                ShowEditYnvPointPanel(promote);
            }
            else if (CurrentNavPortal != null)
            {
                ShowEditYnvPortalPanel(promote);
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
            if (CurrentAudioZone != null)
            {
                ShowEditAudioZonePanel(promote);
            }
            else if (CurrentAudioEmitter != null)
            {
                ShowEditAudioEmitterPanel(promote);
            }
            else if (CurrentAudioZoneList != null)
            {
                ShowEditAudioZoneListPanel(promote);
            }
            else if (CurrentAudioEmitterList != null)
            {
                ShowEditAudioEmitterListPanel(promote);
            }
            else if (CurrentAudioInterior != null)
            {
                ShowEditAudioInteriorPanel(promote);
            }
            else if (CurrentAudioInteriorRoom != null)
            {
                ShowEditAudioInteriorRoomPanel(promote);
            }
            else if (CurrentAudioFile != null)
            {
                ShowEditAudioFilePanel(promote);
            }

        }
        public void ShowProjectItem(object item, bool promote)
        {
            if (item is object[] arr)
            {
                var multisel = MapSelection.FromProjectObject(arr); //convert to MapSelection array
                item = multisel.MultipleSelectionItems;
            }

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

            if (item is MapSelection[] multi)
            {
                WorldForm?.SelectMulti(multi, false, false);
            }

            ShowProjectItemInProcess = false;
        }
        public void SetProjectItem(object item, bool refreshUI = true)
        {
            if (item is MapSelection[] multi)
            {
                CurrentMulti = multi;
                return;
            }
            else
            {
                CurrentMulti = null;
            }

            CurrentYmapFile = item as YmapFile;
            CurrentMloEntity = item as MCEntityDef;
            CurrentEntity = item as YmapEntityDef;
            CurrentCarGen = item as YmapCarGen;
            CurrentLodLight = item as YmapLODLight;
            CurrentBoxOccluder = item as YmapBoxOccluder;
            CurrentOccludeModelTri = item as YmapOccludeModelTriangle;
            CurrentOccludeModel = (item as YmapOccludeModel) ?? CurrentOccludeModelTri?.Model;
            CurrentGrassBatch = item as YmapGrassInstanceBatch;
            CurrentYtypFile = item as YtypFile;
            CurrentArchetype = item as Archetype;
            CurrentYbnFile = item as YbnFile;
            CurrentCollisionBounds = item as Bounds;
            CurrentCollisionPoly = item as BoundPolygon;
            CurrentCollisionVertex = item as BoundVertex;
            CurrentYndFile = item as YndFile;
            CurrentPathNode = item as YndNode;
            CurrentYnvFile = item as YnvFile;
            CurrentNavPoly = item as YnvPoly;
            CurrentNavPoint = item as YnvPoint;
            CurrentNavPortal = item as YnvPortal;
            CurrentTrainTrack = item as TrainTrack;
            CurrentTrainNode = item as TrainTrackNode;
            CurrentScenario = item as YmtFile;
            CurrentScenarioNode = item as ScenarioNode;
            CurrentScenarioChainEdge = item as MCScenarioChainingEdge;
            CurrentAudioFile = item as RelFile;
            CurrentAudioZone = item as AudioPlacement;
            CurrentAudioEmitter = item as AudioPlacement;
            CurrentAudioZoneList = item as Dat151AmbientZoneList;
            CurrentAudioEmitterList = item as Dat151AmbientEmitterList;
            CurrentAudioInterior = item as Dat151Interior;
            CurrentAudioInteriorRoom = item as Dat151InteriorRoom;
            CurrentMloRoom = item as MCMloRoomDef;
            CurrentMloPortal = item as MCMloPortalDef;
            CurrentMloEntitySet = item as MCMloEntitySet;

            if (CurrentAudioZone?.AudioZone == null) CurrentAudioZone = null;
            if (CurrentAudioEmitter?.AudioEmitter == null) CurrentAudioEmitter = null;

            //need to create a temporary AudioPlacement wrapper for these, since AudioPlacements usually come from WorldForm
            var daz = item as Dat151AmbientZone;
            var dae = item as Dat151AmbientEmitter;
            if (daz != null) CurrentAudioZone = new AudioPlacement(daz.Rel, daz);
            if (dae != null) CurrentAudioEmitter = new AudioPlacement(dae.Rel, dae);



            if (CurrentMloEntity != null)
            {
                MloInstanceData instance = TryGetMloInstance(CurrentMloEntity.OwnerMlo);

                if (instance != null)
                {
                    CurrentEntity = instance.TryGetYmapEntity(CurrentMloEntity);
                    CurrentYmapFile = instance.Owner?.Ymap;
                }

                CurrentArchetype = CurrentEntity?.MloParent?.Archetype;
            }
            else if (CurrentEntity != null)
            {
                if (CurrentEntity.MloParent != null)
                {
                    CurrentArchetype = CurrentEntity?.MloParent?.Archetype;
                }
                else
                {
                    CurrentArchetype = CurrentEntity.Archetype;
                    CurrentYmapFile = CurrentEntity.Ymap;
                }
            }
            else if (CurrentCarGen != null)
            {
                CurrentYmapFile = CurrentCarGen.Ymap;
            }
            else if (CurrentLodLight != null)
            {
                CurrentYmapFile = CurrentLodLight.Ymap;
            }
            else if (CurrentBoxOccluder != null)
            {
                CurrentYmapFile = CurrentBoxOccluder.Ymap;
            }
            else if (CurrentOccludeModel != null)
            {
                CurrentYmapFile = CurrentOccludeModel.Ymap;
            }
            else if (CurrentOccludeModelTri != null)
            {
                CurrentYmapFile = CurrentOccludeModelTri.Ymap;
            }
            else if (CurrentGrassBatch != null)
            {
                CurrentYmapFile = CurrentGrassBatch.Ymap;
            }
            if (CurrentMloRoom != null)
            {
                CurrentArchetype = CurrentMloRoom.OwnerMlo;
            }
            if (CurrentMloPortal != null)
            {
                CurrentArchetype = CurrentMloPortal.OwnerMlo;
            }
            if (CurrentMloEntitySet != null)
            {
                CurrentArchetype = CurrentMloEntitySet.OwnerMlo;
            }
            if (CurrentArchetype != null)
            {
                CurrentYtypFile = CurrentEntity?.MloParent?.Archetype?.Ytyp ?? CurrentArchetype?.Ytyp;
            }
            if (CurrentCollisionVertex != null)
            {
                CurrentCollisionBounds = CurrentCollisionVertex.Owner;
            }
            if (CurrentCollisionPoly != null)
            {
                CurrentCollisionBounds = CurrentCollisionPoly.Owner;
            }
            if (CurrentCollisionBounds != null)
            {
                CurrentYbnFile = CurrentCollisionBounds.GetRootYbn();
            }
            if (CurrentPathNode != null)
            {
                CurrentYndFile = CurrentPathNode.Ynd;
            }
            if (CurrentNavPoly != null)
            {
                CurrentYnvFile = CurrentNavPoly.Ynv;
            }
            if (CurrentNavPoint != null)
            {
                CurrentYnvFile = CurrentNavPoint.Ynv;
            }
            if (CurrentNavPortal != null)
            {
                CurrentYnvFile = CurrentNavPortal.Ynv;
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
            if (CurrentAudioZone != null)
            {
                CurrentAudioFile = CurrentAudioZone.RelFile;
            }
            if (CurrentAudioEmitter != null)
            {
                CurrentAudioFile = CurrentAudioEmitter.RelFile;
            }
            if (CurrentAudioZoneList != null)
            {
                CurrentAudioFile = CurrentAudioZoneList.Rel;
            }
            if (CurrentAudioEmitterList != null)
            {
                CurrentAudioFile = CurrentAudioEmitterList.Rel;
            }
            if (CurrentAudioInterior != null)
            {
                CurrentAudioFile = CurrentAudioInterior.Rel;
            }
            if (CurrentAudioInteriorRoom != null)
            {
                CurrentAudioFile = CurrentAudioInteriorRoom.Rel;
            }

            if (refreshUI)
            {
                RefreshUI();
            }

        }
        public void SetCurrentArchetype(Archetype arch)
        {
            CurrentArchetype = arch;
            if (CurrentArchetype != null)
            {
                CurrentYtypFile = CurrentEntity?.MloParent?.Archetype?.Ytyp ?? CurrentArchetype?.Ytyp;
            }
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

        public bool CanPaintInstances()
        {
            // Possibly future proofing for procedural prop instances
            if (CurrentGrassBatch != null)
            {
                if (CurrentGrassBatch.BrushEnabled)
                    return true;
            }

            return false;
        }
        public float GetInstanceBrushRadius()
        {
            if (CurrentGrassBatch != null)
                return CurrentGrassBatch.BrushRadius;

            return 0f;
        }

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

            foreach (var ybn in CurrentProjectFile.YbnFiles)
            {
                string filename = ybn.FilePath;
                if (!File.Exists(filename))
                {
                    filename = cpath + "\\" + filename;
                }
                if (File.Exists(filename))
                {
                    LoadYbnFromFile(ybn, filename);
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

            foreach (var datrel in CurrentProjectFile.AudioRelFiles)
            {
                string filename = datrel.FilePath;
                if (!File.Exists(filename))
                {
                    filename = cpath + "\\" + filename;
                }
                if (File.Exists(filename))
                {
                    LoadAudioRelFromFile(datrel, filename);
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

            foreach (var ybn in CurrentProjectFile.YbnFiles)
            {
                if ((ybn != null) && (ybn.HasChanged))
                {
                    //save the current ybn first?
                    if (MessageBox.Show("Would you like to save " + ybn.Name + " before closing?", "Save .ybn before closing?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CurrentYbnFile = ybn;
                        SaveYbn();
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

            foreach (var datrel in CurrentProjectFile.AudioRelFiles)
            {
                if ((datrel != null) && (datrel.HasChanged))
                {
                    //save the current scenario file first?
                    if (MessageBox.Show("Would you like to save " + datrel.Name + " before closing?", "Save scenario file before closing?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CurrentAudioFile = datrel;
                        SaveAudioFile();
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

            lock (projectsyncroot)
            {

                CloseAllProjectItems();

                CurrentProjectFile = null;
                CurrentYmapFile = null;
                CurrentYtypFile = null;
                CurrentYbnFile = null;
                CurrentYndFile = null;
                CurrentYnvFile = null;
                CurrentTrainTrack = null;
                CurrentScenario = null;

                LoadProjectUI();

            }


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
            else if (CurrentYbnFile != null)
            {
                SaveYbn();
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
            else if (CurrentAudioFile != null)
            {
                SaveAudioFile();
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

                if (CurrentProjectFile.YbnFiles != null)
                {
                    var cybn = CurrentYbnFile;
                    foreach (var ybn in CurrentProjectFile.YbnFiles)
                    {
                        CurrentYbnFile = ybn;
                        SaveYbn();
                    }
                    CurrentYbnFile = cybn;
                    //ShowEditYbnPanel(false);
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

                if (CurrentProjectFile.AudioRelFiles != null)
                {
                    var caudf = CurrentAudioFile;
                    foreach (var audf in CurrentProjectFile.AudioRelFiles)
                    {
                        CurrentAudioFile = audf;
                        SaveAudioFile();
                    }
                    CurrentAudioFile = caudf;
                    //ShowEditAudioFilePanel(false);
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
            else if (CurrentYbnFile != null)
            {
                SaveYbn(saveas);
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
            else if (CurrentAudioFile != null)
            {
                SaveAudioFile(saveas);
            }
        }





        public object NewObject(MapSelection sel, bool copyPosition = false, bool selectNew = true)
        {
            //general method to add a new object, given a map selection
            if (sel.MultipleSelectionItems != null)
            {
                var objs = new List<object>();
                for (int i = 0; i < sel.MultipleSelectionItems.Length; i++)
                {
                    objs.Add(NewObject(sel.MultipleSelectionItems[i], copyPosition, false));
                }
                LoadProjectTree();
                CurrentMulti = sel.MultipleSelectionItems;
                return objs.ToArray();
            }
            else if (sel.CollisionPoly != null) return NewCollisionPoly(sel.CollisionPoly.Type, sel.CollisionPoly, copyPosition, selectNew);
            else if (sel.CollisionBounds != null) return NewCollisionBounds(sel.CollisionBounds.Type, sel.CollisionBounds, copyPosition, selectNew);
            else if (sel.EntityDef != null) return NewEntity(sel.EntityDef, copyPosition, selectNew);
            else if (sel.CarGenerator != null) return NewCarGen(sel.CarGenerator, copyPosition, selectNew);
            else if (sel.LodLight != null) return NewLodLight(sel.LodLight, copyPosition, selectNew);
            else if (sel.BoxOccluder != null) return NewBoxOccluder(sel.BoxOccluder, copyPosition, selectNew);
            else if (sel.OccludeModelTri != null) return NewOccludeModelTriangle(sel.OccludeModelTri, copyPosition, selectNew);
            else if (sel.PathNode != null) return NewPathNode(sel.PathNode, copyPosition, selectNew);
            else if (sel.NavPoly != null) return NewNavPoly(sel.NavPoly, copyPosition, selectNew);
            else if (sel.NavPoint != null) return NewNavPoint(sel.NavPoint, copyPosition, selectNew);
            else if (sel.NavPortal != null) return NewNavPortal(sel.NavPortal, copyPosition, selectNew);
            else if (sel.TrainTrackNode != null) return NewTrainNode(sel.TrainTrackNode, copyPosition, selectNew);
            else if (sel.ScenarioNode != null) return NewScenarioNode(sel.ScenarioNode, copyPosition, selectNew);
            else if (sel.Audio?.AudioZone != null) return NewAudioZone(sel.Audio, copyPosition, selectNew);
            else if (sel.Audio?.AudioEmitter != null) return NewAudioEmitter(sel.Audio, copyPosition, selectNew);
            return null;
        }
        public void DeleteObject(MapSelection sel)
        {
            SetObject(ref sel);
            if (sel.MultipleSelectionItems != null)
            {
                for (int i = 0; i < sel.MultipleSelectionItems.Length; i++)
                {
                    DeleteObject(sel.MultipleSelectionItems[i]);
                }
            }
            else if (sel.CollisionVertex != null) DeleteCollisionVertex();
            else if (sel.CollisionPoly != null) DeleteCollisionPoly();
            else if (sel.CollisionBounds != null) DeleteCollisionBounds();
            else if (sel.EntityDef != null) DeleteEntity();
            else if (sel.CarGenerator != null) DeleteCarGen();
            else if (sel.LodLight != null) DeleteLodLight();
            else if (sel.BoxOccluder != null) DeleteBoxOccluder();
            else if (sel.OccludeModelTri != null) DeleteOccludeModelTriangle();
            else if (sel.PathNode != null) DeletePathNode();
            else if (sel.NavPoly != null) DeleteNavPoly();
            else if (sel.NavPoint != null) DeleteNavPoint();
            else if (sel.NavPortal != null) DeleteNavPortal();
            else if (sel.TrainTrackNode != null) DeleteTrainNode();
            else if (sel.ScenarioNode != null) DeleteScenarioNode();
            else if (sel.Audio?.AudioZone != null) DeleteAudioZone();
            else if (sel.Audio?.AudioEmitter != null) DeleteAudioEmitter();
        }
        private void SetObject(ref MapSelection sel)
        {
            if (sel.MultipleSelectionItems != null) SetProjectItem(sel.MultipleSelectionItems, false);
            else if (sel.CollisionVertex != null) SetProjectItem(sel.CollisionVertex, false);
            else if (sel.CollisionPoly != null) SetProjectItem(sel.CollisionPoly, false);
            else if (sel.CollisionBounds != null) SetProjectItem(sel.CollisionBounds, false);
            else if (sel.EntityDef != null) SetProjectItem(sel.EntityDef, false);
            else if (sel.CarGenerator != null) SetProjectItem(sel.CarGenerator, false);
            else if (sel.LodLight != null) SetProjectItem(sel.LodLight, false);
            else if (sel.BoxOccluder != null) SetProjectItem(sel.BoxOccluder, false);
            else if (sel.OccludeModelTri != null) SetProjectItem(sel.OccludeModelTri, false);
            else if (sel.PathNode != null) SetProjectItem(sel.PathNode, false);
            else if (sel.NavPoly != null) SetProjectItem(sel.NavPoly, false);
            else if (sel.NavPoint != null) SetProjectItem(sel.NavPoint, false);
            else if (sel.NavPortal != null) SetProjectItem(sel.NavPortal, false);
            else if (sel.TrainTrackNode != null) SetProjectItem(sel.TrainTrackNode, false);
            else if (sel.ScenarioNode != null) SetProjectItem(sel.ScenarioNode, false);
            else if (sel.Audio?.AudioZone != null) SetProjectItem(sel.Audio, false);
            else if (sel.Audio?.AudioEmitter != null) SetProjectItem(sel.Audio, false);
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

            AutoUpdateYmapFlagsExtents();

            byte[] data;
            lock (projectsyncroot) //need to sync writes to ymap objects...
            {
                saveas = saveas || string.IsNullOrEmpty(filepath);
                if (saveas)
                {
                    filepath = ShowSaveDialog("Ymap files|*.ymap", filepath);
                    if (string.IsNullOrEmpty(filepath))
                    { return; }
                }

                filepath = filepath.ToLowerInvariant();
                string newname = Path.GetFileNameWithoutExtension(filepath);
                JenkIndex.Ensure(newname);
                CurrentYmapFile.FilePath = filepath;
                CurrentYmapFile.RpfFileEntry.Name = new FileInfo(filepath).Name;
                CurrentYmapFile.Name = CurrentYmapFile.RpfFileEntry.Name;
                CurrentYmapFile._CMapData.name = new MetaHash(JenkHash.GenHash(newname));

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
                        //MessageBox.Show("Couldn't rename ymap in project! This shouldn't happen - check the project file XML.");
                    }

                    ProjectExplorer?.UpdateYmapTreeNode(CurrentYmapFile);
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
            else if (CurrentLodLight != null)
            {
                ProjectExplorer?.TrySelectLodLightTreeNode(CurrentLodLight);
            }
            else if (CurrentGrassBatch != null)
            {
                ProjectExplorer?.TrySelectGrassBatchTreeNode(CurrentGrassBatch);
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
        public void AutoUpdateYmapFlagsExtents()
        {
            if (CurrentYmapFile == null) return;
            if (autoymapextents)
            {
                CurrentYmapFile.CalcExtents();
            }
            if (autoymapflags)
            {
                CurrentYmapFile.CalcFlags();
            }
            var panel = FindPanel((EditYmapPanel p) => p.Tag == CurrentYmapFile);
            if (panel != null)
            {
                panel.SetYmap(CurrentYmapFile);
            }
        }

        public YmapEntityDef NewEntity(YmapEntityDef copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (copy != null)
            {
                //create the entity in MLO instead of a ymap, if the copy is in an MLO
                var instance = copy.MloParent?.MloInstance;
                var entdef = instance?.TryGetArchetypeEntity(copy);
                if (entdef != null)
                {
                    return NewMloEntity(copy, copyPosition, selectNew);
                }
            }


            if (CurrentYmapFile == null) return null;

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
                cent.flags = 32; //1572872;
                cent.parentIndex = -1;
                cent.lodDist = 200.0f;
                cent.lodLevel = rage__eLodType.LODTYPES_DEPTH_ORPHANHD;
                cent.priorityLevel = rage__ePriorityLevel.PRI_REQUIRED;
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



            if (selectNew)
            {
                LoadProjectTree();
                ProjectExplorer?.TrySelectEntityTreeNode(ent);
                CurrentEntity = ent;
                ShowEditYmapEntityPanel(false);
            }

            return ent;
        }
        public void AddEntityToProject()
        {
            try
            {
                if (CurrentEntity == null) return;
                if (CurrentEntity.Ymap == null)
                {
                    CurrentYtypFile = CurrentEntity.MloParent?.Archetype?.Ytyp;

                    if (!YtypExistsInProject(CurrentYtypFile))
                    {
                        if (CurrentEntity.MloParent?.MloInstance != null)
                        {
                            var inst = CurrentEntity.MloParent.MloInstance;
                            var mcEntity = inst.TryGetArchetypeEntity(CurrentEntity);
                            if (mcEntity != null)
                            {
                                YmapEntityDef ent = CurrentEntity;
                                CurrentYtypFile.HasChanged = true;
                                AddYtypToProject(CurrentYtypFile);
                                CurrentEntity = ent;
                                CurrentYtypFile = ent.MloParent.Archetype.Ytyp;
                                ProjectExplorer?.TrySelectMloEntityTreeNode(mcEntity);
                            }
                        }
                    }
                    return;
                }

                CurrentYmapFile = CurrentEntity.Ymap;
                if (!YmapExistsInProject(CurrentYmapFile))
                {
                    YmapEntityDef ent = CurrentEntity;
                    CurrentYmapFile.HasChanged = true;
                    AddYmapToProject(CurrentYmapFile);

                    CurrentEntity = ent; //bug fix for some reason the treeview selects the project node here.
                    CurrentYmapFile = ent.Ymap;
                    ProjectExplorer?.TrySelectEntityTreeNode(ent);
                }
            }
            catch
            { }
        }
        public bool DeleteEntity()
        {
            if (CurrentEntity == null) return false;
            return CurrentYmapFile != null ? DeleteYmapEntity() : DeleteMloEntity();
        }

        private bool DeleteYmapEntity()
        {
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

            //if (MessageBox.Show("Are you sure you want to delete this entity?\n" + CurrentEntity._CEntityDef.archetypeName.ToString() + "\n" + CurrentEntity.Position.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

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

            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool IsCurrentEntity(YmapEntityDef ent)
        {
            return CurrentEntity == ent;
        }

        public YmapGrassInstanceBatch NewGrassBatch(YmapGrassInstanceBatch copy = null)
        {
            if (CurrentYmapFile == null) return null;

            rage__fwGrassInstanceListDef fwBatch = new rage__fwGrassInstanceListDef();
            rage__fwGrassInstanceListDef__InstanceData[] instances = new rage__fwGrassInstanceListDef__InstanceData[0];

            if (copy != null)
            {
                fwBatch = copy.Batch;
                instances = copy.Instances;
            }
            else
            {
                fwBatch.archetypeName = new MetaHash(JenkHash.GenHash("proc_grasses01"));
                fwBatch.lodDist = 120;
                fwBatch.LodFadeStartDist = 15;
                fwBatch.LodInstFadeRange = 0.75f;
                fwBatch.OrientToTerrain = 1.0f;
                fwBatch.ScaleRange = new Vector3(0.3f, 0.2f, 0.7f);
            }

            YmapGrassInstanceBatch batch = new YmapGrassInstanceBatch
            {
                AABBMin = fwBatch.BatchAABB.min.XYZ(),
                AABBMax = fwBatch.BatchAABB.max.XYZ(),
                Archetype = GameFileCache.GetArchetype(fwBatch.archetypeName),
                Batch = fwBatch,
                Instances = instances
            };

            batch.Position = (batch.AABBMin + batch.AABBMax) * 0.5f;
            batch.Radius = (batch.AABBMax - batch.AABBMin).Length() * 0.5f;
            batch.Ymap = CurrentYmapFile;
            
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    CurrentYmapFile.AddGrassBatch(batch);
                }
            }
            else
            {
                CurrentYmapFile.AddGrassBatch(batch);
            }

            LoadProjectTree();

            ProjectExplorer?.TrySelectGrassBatchTreeNode(batch);
            CurrentGrassBatch = batch;
            ShowEditYmapGrassBatchPanel(false);

            return batch;
        }
        public void AddGrassBatchToProject()
        {
            if (CurrentGrassBatch == null) return;

            CurrentYmapFile = CurrentGrassBatch.Ymap;
            if (!YmapExistsInProject(CurrentYmapFile))
            {
                var grassBatch = CurrentGrassBatch;
                CurrentYmapFile.HasChanged = true;
                AddYmapToProject(CurrentYmapFile);

                CurrentGrassBatch = grassBatch; //bug fix for some reason the treeview selects the project node here.
                CurrentYmapFile = grassBatch.Ymap;
                ProjectExplorer?.TrySelectGrassBatchTreeNode(grassBatch);
            }
        }
        public bool DeleteGrassBatch()
        {
            if (CurrentYmapFile == null) return false;
            if (CurrentGrassBatch == null) return false;
            if (CurrentGrassBatch.Ymap != CurrentYmapFile) return false;
            if (CurrentYmapFile.GrassInstanceBatches == null) return false; //nothing to delete..

            if (MessageBox.Show("Are you sure you want to delete this grass batch?\n" + CurrentGrassBatch.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentYmapFile.RemoveGrassBatch(CurrentGrassBatch);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentYmapFile.RemoveGrassBatch(CurrentGrassBatch);
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the grass batch. This shouldn't happen!");
            }

            var delbatch = CurrentGrassBatch;

            ProjectExplorer?.RemoveGrassBatchTreeNode(CurrentGrassBatch);
            ProjectExplorer?.SetYmapHasChanged(CurrentYmapFile, true);

            ClosePanel((EditYmapGrassPanel p) => { return p.Tag == delbatch; });

            CurrentGrassBatch = null;

            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public void PaintGrass(SpaceRayIntersectResult mouseRay, bool erase)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { PaintGrass(mouseRay, erase); }));
                    return;
                }

                if (!mouseRay.Hit || !mouseRay.TestComplete) return;
                if (CurrentGrassBatch == null || (!CurrentGrassBatch.BrushEnabled)) return; // brush isn't enabled right now
                EditYmapGrassPanel panel = FindPanel<EditYmapGrassPanel>(x => x.CurrentBatch == CurrentGrassBatch);
                if (panel == null) return; // no panels with this batch

                // TODO: Maybe move these functions into the batch instead of the grass panel?
                // although, the panel does have the brush settings.
                if (!erase)
                    panel.CreateInstancesAtMouse(mouseRay);
                else panel.EraseInstancesAtMouse(mouseRay);
            }
            catch { }
        }
        public bool GrassBatchExistsInProject(YmapGrassInstanceBatch batch)
        {
            if (CurrentProjectFile?.YmapFiles == null) return false;
            if (CurrentProjectFile.YmapFiles.Count <= 0) return false;
            foreach (var ymapFile in CurrentProjectFile.YmapFiles)
            {
                if (ymapFile.GrassInstanceBatches == null) continue;
                foreach (var b in ymapFile.GrassInstanceBatches)
                {
                    if (batch == b)
                        return true;
                }
            }
            return false;
        }

        public YmapCarGen NewCarGen(YmapCarGen copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (CurrentYmapFile == null) return null;

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
                ccg.bodyColorRemap1 = -1;
                ccg.bodyColorRemap2 = -1;
                ccg.bodyColorRemap3 = -1;
                ccg.bodyColorRemap4 = -1;
                ccg.livery = -1;
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


            if (selectNew)
            {
                LoadProjectTree();
                ProjectExplorer?.TrySelectCarGenTreeNode(cg);
                CurrentCarGen = cg;
                ShowEditYmapCarGenPanel(false);
            }
            return cg;
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

            //if (MessageBox.Show("Are you sure you want to delete this car generator?\n" + CurrentCarGen.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

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

            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool IsCurrentCarGen(YmapCarGen cargen)
        {
            return CurrentCarGen == cargen;
        }

        public YmapLODLight NewLodLight(YmapLODLight copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (CurrentYmapFile == null) return null;

            Vector3 pos = GetSpawnPos(10.0f);

            YmapLODLight yll = new YmapLODLight();

            if (copy != null)
            {
                yll.CopyFrom(copy);
            }
            else
            {
                yll.TimeAndStateFlags = 0x00FFFFFF;
                yll.Type = LightType.Point;
                yll.Colour = new SharpDX.Color(255, 255, 255, 127);
                yll.Direction = Vector3.ForwardRH;
                yll.Falloff = 10.0f;
                //...
            }

            if (!copyPosition || (copy == null))
            {
                yll.Position = pos;
            }



            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    CurrentYmapFile.AddLodLight(yll);
                }

                WorldForm.UpdateLodLightGraphics(yll);

            }
            else
            {
                CurrentYmapFile.AddLodLight(yll);
            }


            if (selectNew)
            {
                LoadProjectTree();
                ProjectExplorer?.TrySelectLodLightTreeNode(yll);
                CurrentLodLight = yll;
                ShowEditYmapLodLightPanel(false);
            }
            return yll;
        }
        public void AddLodLightToProject()
        {
            if (CurrentLodLight == null) return;

            if (!YmapExistsInProject(CurrentLodLight.Ymap))
            {
                var lodlight = CurrentLodLight;
                if (lodlight.DistLodLights?.Ymap != null)
                {
                    AddYmapToProject(lodlight.DistLodLights.Ymap);
                    CurrentYmapFile.HasChanged = true;
                }

                CurrentYmapFile = lodlight.Ymap;
                CurrentYmapFile.HasChanged = true;
                AddYmapToProject(CurrentYmapFile);

                CurrentLodLight = lodlight; //bug fix for some reason the treeview selects the project node here.
                CurrentYmapFile = lodlight.Ymap;
                ProjectExplorer?.TrySelectLodLightTreeNode(lodlight);
            }
        }
        public bool DeleteLodLight()
        {
            if (CurrentYmapFile == null) return false;
            if (CurrentLodLight == null) return false;
            if (CurrentLodLight.Ymap != CurrentYmapFile) return false;
            //if (CurrentYmapFile.LODLights == null) return false; //nothing to delete..

            //if (MessageBox.Show("Are you sure you want to delete this LOD light?\n" + CurrentLodLight.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

            var delyll = CurrentLodLight;
            var lodlights = delyll.LodLights;

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentYmapFile.RemoveLodLight(CurrentLodLight);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentYmapFile.RemoveLodLight(CurrentLodLight);
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the LOD light. This shouldn't happen!");
            }

            ProjectExplorer?.RemoveLodLightTreeNode(CurrentLodLight);
            ProjectExplorer?.SetYmapHasChanged(CurrentYmapFile, true);

            ClosePanel((EditYmapLodLightPanel p) => { return p.Tag == delyll; });

            CurrentLodLight = null;
            CurrentYmapFile = null;

            if (WorldForm != null)
            {
                if ((lodlights?.LodLights != null) && (lodlights.LodLights.Length > 0))
                {
                    WorldForm.UpdateLodLightGraphics(lodlights.LodLights[0]);
                }

                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool IsCurrentLodLight(YmapLODLight lodlight)
        {
            return CurrentLodLight == lodlight;
        }

        public YmapBoxOccluder NewBoxOccluder(YmapBoxOccluder copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (CurrentYmapFile == null) return null;

            Vector3 pos = GetSpawnPos(10.0f);

            YmapBoxOccluder bo;

            if (copy != null)
            {
                bo = new YmapBoxOccluder(CurrentYmapFile, copy._Box);
            }
            else
            {
                bo = new YmapBoxOccluder(CurrentYmapFile, new BoxOccluder());
                //...
            }

            if (!copyPosition || (copy == null))
            {
                bo.Position = pos;
            }



            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    CurrentYmapFile.AddBoxOccluder(bo);
                }

                WorldForm.UpdateBoxOccluderGraphics(bo);

            }
            else
            {
                CurrentYmapFile.AddBoxOccluder(bo);
            }


            if (selectNew)
            {
                LoadProjectTree();
                ProjectExplorer?.TrySelectBoxOccluderTreeNode(bo);
                CurrentBoxOccluder = bo;
                ShowEditYmapBoxOccluderPanel(false);
            }
            return bo;
        }
        public void AddBoxOccluderToProject()
        {
            if (CurrentBoxOccluder == null) return;

            if (!YmapExistsInProject(CurrentBoxOccluder.Ymap))
            {
                var box = CurrentBoxOccluder;

                CurrentYmapFile = box.Ymap;
                CurrentYmapFile.HasChanged = true;
                AddYmapToProject(CurrentYmapFile);

                CurrentBoxOccluder = box; //bug fix for some reason the treeview selects the project node here.
                CurrentYmapFile = box.Ymap;
                ProjectExplorer?.TrySelectBoxOccluderTreeNode(box);
            }
        }
        public bool DeleteBoxOccluder()
        {
            if (CurrentYmapFile == null) return false;
            if (CurrentBoxOccluder == null) return false;
            if (CurrentBoxOccluder.Ymap != CurrentYmapFile) return false;

            //if (MessageBox.Show("Are you sure you want to delete this box occluder?\n" + CurrentBoxOccluder.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

            var delbox = CurrentBoxOccluder;
            
            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentYmapFile.RemoveBoxOccluder(CurrentBoxOccluder);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentYmapFile.RemoveBoxOccluder(CurrentBoxOccluder);
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the box occluder. This shouldn't happen!");
            }

            ProjectExplorer?.RemoveBoxOccluderTreeNode(CurrentBoxOccluder);
            ProjectExplorer?.SetYmapHasChanged(CurrentYmapFile, true);

            ClosePanel((EditYmapBoxOccluderPanel p) => { return p.Tag == delbox; });

            CurrentBoxOccluder = null;
            CurrentYmapFile = null;

            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool IsCurrentBoxOccluder(YmapBoxOccluder box)
        {
            return CurrentBoxOccluder == box;
        }

        public YmapOccludeModel NewOccludeModel(YmapOccludeModel copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (CurrentYmapFile == null) return null;

            Vector3 pos = GetSpawnPos(10.0f);

            YmapOccludeModel om;

            if (copy != null)
            {
                om = new YmapOccludeModel(CurrentYmapFile, copy._OccludeModel);
            }
            else
            {
                om = new YmapOccludeModel(CurrentYmapFile, new OccludeModel());
                //...
            }

            if (!copyPosition || (copy == null))
            {
                //om.Center = pos;
            }



            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    CurrentYmapFile.AddOccludeModel(om);
                }

                WorldForm.UpdateOccludeModelGraphics(om);

            }
            else
            {
                CurrentYmapFile.AddOccludeModel(om);
            }


            if (selectNew)
            {
                LoadProjectTree();
                ProjectExplorer?.TrySelectOccludeModelTreeNode(om);
                CurrentOccludeModel = om;
                ShowEditYmapOccludeModelPanel(false);
            }
            return om;
        }
        public void AddOccludeModelToProject()
        {
            if (CurrentOccludeModel == null) return;

            if (!YmapExistsInProject(CurrentOccludeModel.Ymap))
            {
                var model = CurrentOccludeModel;

                CurrentYmapFile = model.Ymap;
                CurrentYmapFile.HasChanged = true;
                AddYmapToProject(CurrentYmapFile);

                CurrentOccludeModel = model; //bug fix for some reason the treeview selects the project node here.
                CurrentYmapFile = model.Ymap;
                ProjectExplorer?.TrySelectOccludeModelTreeNode(model);
            }
        }
        public bool DeleteOccludeModel()
        {
            if (CurrentYmapFile == null) return false;
            if (CurrentOccludeModel == null) return false;
            if (CurrentOccludeModel.Ymap != CurrentYmapFile) return false;

            //if (MessageBox.Show("Are you sure you want to delete this occlude model?\n" + CurrentOccludeModel.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

            var delmodel = CurrentOccludeModel;

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentYmapFile.RemoveOccludeModel(CurrentOccludeModel);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentYmapFile.RemoveOccludeModel(CurrentOccludeModel);
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the occlude model. This shouldn't happen!");
            }

            ProjectExplorer?.RemoveOccludeModelTreeNode(CurrentOccludeModel);
            ProjectExplorer?.SetYmapHasChanged(CurrentYmapFile, true);

            ClosePanel((EditYmapOccludeModelPanel p) => { return p.Tag == delmodel; });

            CurrentOccludeModel = null;
            CurrentYmapFile = null;

            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool IsCurrentOccludeModel(YmapOccludeModel model)
        {
            return CurrentOccludeModel == model;
        }

        public YmapOccludeModelTriangle NewOccludeModelTriangle(YmapOccludeModelTriangle copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (CurrentYmapFile == null) return null;

            Vector3 pos = GetSpawnPos(10.0f);

            YmapOccludeModelTriangle ot;

            if (copy != null)
            {
                ot = new YmapOccludeModelTriangle(copy.Model, copy.Corner1, copy.Corner2, copy.Corner3, copy.Model.Triangles?.Length ?? 0);
            }
            else
            {
                ot = new YmapOccludeModelTriangle(CurrentOccludeModel, pos, pos + Vector3.UnitY, pos + Vector3.UnitX, CurrentOccludeModel?.Triangles?.Length ?? 0);
                //...
            }

            if (!copyPosition || (copy == null))
            {
                //om.Center = pos;
            }



            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    CurrentYmapFile.AddOccludeModelTriangle(ot);
                }

                WorldForm.UpdateOccludeModelGraphics(ot.Model);

            }
            else
            {
                CurrentYmapFile.AddOccludeModelTriangle(ot);
            }


            if (selectNew)
            {
                LoadProjectTree();
                ProjectExplorer?.TrySelectOccludeModelTriangleTreeNode(ot);
                CurrentOccludeModel = ot.Model;
                CurrentOccludeModelTri = ot;
                ShowEditYmapOccludeModelTrianglePanel(false);
            }
            return ot;
        }
        public void AddOccludeModelTriangleToProject()
        {
            if (CurrentOccludeModelTri == null) return;

            if (!YmapExistsInProject(CurrentOccludeModelTri.Ymap))
            {
                var tri = CurrentOccludeModelTri;

                CurrentYmapFile = tri.Ymap;
                CurrentYmapFile.HasChanged = true;
                AddYmapToProject(CurrentYmapFile);

                CurrentOccludeModelTri = tri; //bug fix for some reason the treeview selects the project node here.
                CurrentOccludeModel = tri.Model;
                CurrentYmapFile = tri.Ymap;
                ProjectExplorer?.TrySelectOccludeModelTriangleTreeNode(tri);
            }
        }
        public bool DeleteOccludeModelTriangle()
        {
            if (CurrentYmapFile == null) return false;
            if (CurrentOccludeModelTri == null) return false;
            if (CurrentOccludeModelTri.Ymap != CurrentYmapFile) return false;

            //if (MessageBox.Show("Are you sure you want to delete this occlude model triangle?\n" + CurrentOccludeModelTri.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

            var deltri = CurrentOccludeModelTri;

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentYmapFile.RemoveOccludeModelTriangle(CurrentOccludeModelTri);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentYmapFile.RemoveOccludeModelTriangle(CurrentOccludeModelTri);
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the occlude model triangle. This shouldn't happen!");
            }

            ProjectExplorer?.UpdateOccludeModelTreeNode(CurrentOccludeModelTri?.Model);
            ProjectExplorer?.SetYmapHasChanged(CurrentYmapFile, true);

            ClosePanel((EditYmapOccludeModelPanel p) => { return p.CurrentTriangle == deltri; });

            CurrentOccludeModelTri = null;
            CurrentOccludeModel = null;
            CurrentYmapFile = null;

            if (WorldForm != null)
            {
                WorldForm.UpdateOccludeModelGraphics(deltri.Model);
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool IsCurrentOccludeModelTriangle(YmapOccludeModelTriangle tri)
        {
            return CurrentOccludeModelTri == tri;
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
                    ccg.bodyColorRemap1 = -1;
                    ccg.bodyColorRemap2 = -1;
                    ccg.bodyColorRemap3 = -1;
                    ccg.bodyColorRemap4 = -1;

                    if (sbyte.TryParse(placement.VehicleProperties.FirstOrDefault(p => p.Name == "Livery")?.Value, out sbyte livery))
                    {
                        ccg.livery = livery;
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
                    cent.flags = placement.Dynamic ? 0 : 32u;// 1572872; //32 = static
                    cent.parentIndex = -1;
                    cent.lodDist = (placement.LodDistance < 10000) ? placement.LodDistance : -1;
                    cent.lodLevel = rage__eLodType.LODTYPES_DEPTH_ORPHANHD;
                    cent.priorityLevel = rage__ePriorityLevel.PRI_REQUIRED;
                    cent.ambientOcclusionMultiplier = 255;
                    cent.artificialAmbientOcclusion = 255;
                    if(uint.TryParse(placement.ObjectProperties.FirstOrDefault(p => p.Name == "TextureVariation")?.Value, out uint tint))
                    {
                        cent.tintValue = tint;
                    }

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
        public void SaveYtyp(bool saveas = false)
        {
            if (CurrentYtypFile == null) return;
            string ytypname = CurrentYtypFile.Name;
            string filepath = CurrentYtypFile.FilePath;
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = ytypname;
            }
            string origfile = filepath;
            if (!File.Exists(filepath))
            {
                saveas = true;
            }


            byte[] data;
            lock (projectsyncroot) //need to sync writes to ytyp objects...
            {
                saveas = saveas || string.IsNullOrEmpty(filepath);
                if (saveas)
                {
                    filepath = ShowSaveDialog("Ytyp files|*.ytyp", filepath);
                    if (string.IsNullOrEmpty(filepath))
                    { return; }

                    string newname = Path.GetFileNameWithoutExtension(filepath);
                    JenkIndex.Ensure(newname);
                    CurrentYtypFile.FilePath = filepath;
                    CurrentYtypFile.RpfFileEntry.Name = new FileInfo(filepath).Name;
                    CurrentYtypFile.Name = CurrentYtypFile.RpfFileEntry.Name;
                    CurrentYtypFile._CMapTypes.name = new MetaHash(JenkHash.GenHash(newname));
                }

                data = CurrentYtypFile.Save();
            }


            if (data != null)
            {
                File.WriteAllBytes(filepath, data);
            }

            SetYtypHasChanged(false);

            if (saveas)
            {
                if (CurrentProjectFile != null)
                {
                    string origpath = CurrentProjectFile.GetRelativePath(origfile);
                    string newpath = CurrentProjectFile.GetRelativePath(CurrentYtypFile.FilePath);

                    if (!CurrentProjectFile.RenameYtyp(origpath, newpath))
                    { //couldn't rename it in the project?
                        //MessageBox.Show("Couldn't rename ytyp in project! This shouldn't happen - check the project file XML.");
                    }

                    ProjectExplorer?.UpdateYtypTreeNode(CurrentYtypFile);
                }
                SetProjectHasChanged(true);
                SetCurrentSaveItem();
            }

            if (CurrentYtypFile.SaveWarnings != null)
            {
                string w = string.Join("\n", CurrentYtypFile.SaveWarnings);
                MessageBox.Show(CurrentYtypFile.SaveWarnings.Count.ToString() + " warnings were generated while saving the ytyp:\n" + w);
                CurrentYtypFile.SaveWarnings = null;//clear it out for next time..
            }
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
            AddProjectArchetypes(ytyp);
        }
        public void RemoveYtypFromProject()
        {
            if (CurrentYtypFile == null) return;
            if (CurrentProjectFile == null) return;
            RemoveProjectArchetypes(CurrentYtypFile);
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

        public Archetype NewArchetype(Archetype copy = null)
        {
            if (CurrentYtypFile == null) return null;
            var archetype = CurrentYtypFile.AddArchetype();
            var archetypeDef = archetype._BaseArchetypeDef;
            if (copy == null)
            {
                copy = CurrentArchetype;
                archetype._BaseArchetypeDef.name = JenkHash.GenHash("prop_alien_egg_01");
            }
            if (copy != null)
            {
                archetype.Init(CurrentYtypFile, ref copy._BaseArchetypeDef);
            }
            archetype._BaseArchetypeDef = archetypeDef;

            LoadProjectTree();
            ProjectExplorer?.TrySelectArchetypeTreeNode(archetype);
            CurrentArchetype = archetype;

            AddProjectArchetype(archetype);

            return archetype;
        }
        public void NewArchetypeFromYdr()
        {
            if (CurrentYtypFile == null) return;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;
            dlg.CheckFileExists = true;
            dlg.AddExtension = true;
            dlg.DefaultExt = ".ydr";
            dlg.Filter = "Ydr files|*.ydr";
            
            bool? result = dlg.ShowDialog();
            if (result == true && dlg.FileNames.Length > -0)
            {
                foreach(string path in dlg.FileNames)
                {
                    var archetype = CurrentYtypFile.AddArchetype();
                    YdrFile fileYdr = new YdrFile();
                    RpfFile.LoadResourceFile<YdrFile>(fileYdr, File.ReadAllBytes(path), 165);
                    fileYdr.Drawable.Name = fileYdr.Drawable.Name.Replace(".#dr", "");
                    var name_hash = JenkHash.GenHash(fileYdr.Drawable.Name);
                    archetype._BaseArchetypeDef.name = name_hash;
                    archetype._BaseArchetypeDef.assetName = name_hash;
                    archetype._BaseArchetypeDef.assetType = rage__fwArchetypeDef__eAssetType.ASSET_TYPE_DRAWABLE;

                    if (fileYdr.Drawable.ShaderGroup.TextureDictionary != null) archetype._BaseArchetypeDef.textureDictionary = name_hash;
                    if (fileYdr.Drawable.Bound != null) archetype._BaseArchetypeDef.physicsDictionary = name_hash;

                    archetype._BaseArchetypeDef.specialAttribute = 0;
                    archetype._BaseArchetypeDef.flags = 32;
                    archetype._BaseArchetypeDef.bbMin = fileYdr.Drawable.BoundingBoxMin;
                    archetype._BaseArchetypeDef.bbMax = fileYdr.Drawable.BoundingBoxMax;
                    archetype._BaseArchetypeDef.bsCentre = fileYdr.Drawable.BoundingCenter;
                    archetype._BaseArchetypeDef.bsRadius = fileYdr.Drawable.BoundingSphereRadius;
                    archetype._BaseArchetypeDef.hdTextureDist = 60.0f;
                    archetype._BaseArchetypeDef.lodDist = 60.0f;

                    LoadProjectTree();
                    ProjectExplorer?.TrySelectArchetypeTreeNode(archetype);
                    CurrentArchetype = archetype;

                    AddProjectArchetype(archetype);
                }
            }
        }
        public YmapEntityDef NewMloEntity(YmapEntityDef copy = null, bool copyTransform = false, bool selectNew = true)
        {
            if ((CurrentArchetype == null) || !(CurrentArchetype is MloArchetype mloArch))
            {
                mloArch = (CurrentEntity?.MloParent.Archetype as MloArchetype) ?? CurrentMloRoom?.OwnerMlo ?? CurrentMloPortal?.OwnerMlo ?? CurrentMloEntitySet?.OwnerMlo;
                if (mloArch == null) return null;
                CurrentArchetype = mloArch;
            }

            var mloInstance = TryGetMloInstance(mloArch);
            if (mloInstance == null)
            {
                MessageBox.Show("Unable to find MLO instance for this interior! Try adding an MLO instance ymap to the project.");
                return null;
            }

            if ((CurrentMloEntity == null) && (CurrentEntity != null))
            {
                CurrentMloEntity = mloInstance.TryGetArchetypeEntity(CurrentEntity);
            }

            if ((CurrentMloRoom == null) && (CurrentMloPortal == null) && (CurrentMloEntitySet == null))
            {
                if (CurrentMloEntity != null)
                {
                    CurrentMloRoom = mloArch.GetEntityRoom(CurrentMloEntity);
                    CurrentMloPortal = mloArch.GetEntityPortal(CurrentMloEntity);
                    CurrentMloEntitySet = mloArch.GetEntitySet(CurrentMloEntity);
                }
                else
                {
                    if ((mloArch.rooms?.Length??0) <= 0)
                    {
                        MessageBox.Show($@"Mlo {mloArch.Name} has no rooms! Cannot create entity.");
                        return null;
                    }
                    CurrentMloRoom = mloArch.rooms[0];
                }
            }


            int roomIndex = CurrentMloRoom?.Index ?? -1;
            if (roomIndex >= (mloArch.rooms?.Length ?? 0))
            {
                MessageBox.Show($@"Room at index {roomIndex} does not exist in {mloArch.Name}! {mloArch.Name} only has {(mloArch.rooms?.Length ?? 0)} rooms.");
                return null;
            }

            int portalIndex = CurrentMloPortal?.Index ?? -1;
            if (portalIndex >= (mloArch.portals?.Length ?? 0))
            {
                MessageBox.Show($@"Portal at index {portalIndex} does not exist in {mloArch.Name}! {mloArch.Name} only has {(mloArch.portals?.Length ?? 0)} portals.");
                return null;
            }

            int entsetIndex = CurrentMloEntitySet?.Index ?? -1;
            if (entsetIndex >= (mloArch.entitySets?.Length ?? 0))
            {
                MessageBox.Show($@"EntitySet at index {entsetIndex} does not exist in {mloArch.Name}! {mloArch.Name} only has {(mloArch.entitySets?.Length ?? 0)} entitySets.");
                return null;
            }


            float spawndist = 5.0f; //use archetype BSradius if starting with a copy...
            if (copy != null)
            {
                spawndist = copy.BSRadius * 2.5f;
            }

            bool cp = copyTransform && (copy != null);
            Vector3 pos = cp ? copy.CEntityDef.position : GetSpawnPosRel(spawndist, mloInstance.Owner.Position, mloInstance.Owner.Orientation);
            Quaternion rot = cp ? copy.CEntityDef.rotation.ToQuaternion() : Quaternion.Identity;


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
                cent.lodLevel = rage__eLodType.LODTYPES_DEPTH_ORPHANHD;
                cent.priorityLevel = rage__ePriorityLevel.PRI_REQUIRED;
                cent.ambientOcclusionMultiplier = 255;
                cent.artificialAmbientOcclusion = 255;
            }

            cent.position = pos;
            cent.rotation = rot.ToVector4();

            var createindex = mloArch.entities.Length;
            var ment = new MCEntityDef(ref cent, mloArch);
            var outEnt = mloInstance.CreateYmapEntity(mloInstance.Owner, ment, createindex);

            try
            {
                if (WorldForm != null)
                {
                    lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                    {
                        mloArch.AddEntity(outEnt, roomIndex, portalIndex, entsetIndex);
                        mloInstance.AddEntity(outEnt);
                        outEnt.SetArchetype(GameFileCache.GetArchetype(cent.archetypeName));
                    }
                }
                else
                {
                    mloArch.AddEntity(outEnt, roomIndex, portalIndex, entsetIndex);
                    mloInstance.AddEntity(outEnt);
                    outEnt.SetArchetype(GameFileCache.GetArchetype(cent.archetypeName));
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(this, e.Message, "Create MLO Entity Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            ment = mloInstance.TryGetArchetypeEntity(outEnt);

            if (selectNew)
            {
                LoadProjectTree();
                ProjectExplorer?.TrySelectMloEntityTreeNode(ment);
                CurrentEntity = outEnt;
                CurrentMloEntity = ment;
                CurrentYtypFile = CurrentEntity.MloParent?.Archetype?.Ytyp;
            }

            return outEnt;
        }
        public MCMloRoomDef NewMloRoom(MCMloRoomDef copy = null)
        {
            var mlo = CurrentMloRoom?.OwnerMlo ?? CurrentMloPortal?.OwnerMlo ?? CurrentMloEntitySet?.OwnerMlo ?? (CurrentEntity?.MloParent.Archetype as MloArchetype) ?? (CurrentArchetype as MloArchetype);
            if (mlo == null) return null;

            if (copy == null)
            {
                copy = CurrentMloRoom;
            }

            var room = new MCMloRoomDef();
            if (copy != null)
            {
                room._Data = copy._Data;
                room.RoomName = copy.RoomName;
            }
            else
            {
                room._Data.flags = 96;
                room._Data.blend = 1.0f;
                room._Data.exteriorVisibiltyDepth = -1;
                room.RoomName = "NewRoom";
            }

            mlo.AddRoom(room);

            var mloInstance = TryGetMloInstance(mlo);
            if (mloInstance != null)
            {
            }

            LoadProjectTree();
            ProjectExplorer?.TrySelectMloRoomTreeNode(room);
            CurrentMloRoom = room;
            CurrentYtypFile = room?.OwnerMlo?.Ytyp;

            return room;
        }
        public MCMloPortalDef NewMloPortal(MCMloPortalDef copy = null)
        {
            var mlo = CurrentMloRoom?.OwnerMlo ?? CurrentMloPortal?.OwnerMlo ?? CurrentMloEntitySet?.OwnerMlo ?? (CurrentEntity?.MloParent.Archetype as MloArchetype) ?? (CurrentArchetype as MloArchetype);
            if (mlo == null) return null;

            if (copy == null)
            {
                copy = CurrentMloPortal;
            }

            var portal = new MCMloPortalDef();
            if (copy != null)
            {
                portal._Data = copy._Data;
                portal.Corners = (Vector4[])copy.Corners?.Clone();
            }
            else
            {
                portal._Data.roomFrom = 1;
                portal._Data.roomTo = 0;
            }
            if (portal.Corners == null)
            {
                portal.Corners = new[] { new Vector4(0, 0, 0, float.NaN), new Vector4(0, 0, 1, float.NaN), new Vector4(0, 1, 1, float.NaN), new Vector4(0, 1, 0, float.NaN) };
            }

            mlo.AddPortal(portal);

            var mloInstance = TryGetMloInstance(mlo);
            if (mloInstance != null)
            {
            }

            LoadProjectTree();
            ProjectExplorer?.TrySelectMloPortalTreeNode(portal);
            CurrentMloPortal = portal;
            CurrentYtypFile = portal?.OwnerMlo?.Ytyp;

            return portal;
        }
        public MCMloEntitySet NewMloEntitySet(MCMloEntitySet copy = null)
        {
            var mlo = CurrentMloRoom?.OwnerMlo ?? CurrentMloPortal?.OwnerMlo ?? CurrentMloEntitySet?.OwnerMlo ?? (CurrentEntity?.MloParent.Archetype as MloArchetype) ?? (CurrentArchetype as MloArchetype);
            if (mlo == null) return null;

            if (copy == null)
            {
                copy = CurrentMloEntitySet;
            }

            var set = new MCMloEntitySet();
            if (copy != null)
            {
                set._Data.name = copy._Data.name;
            }
            else
            {
                JenkIndex.Ensure("NewEntitySet");//why is this here though
                set._Data.name = JenkHash.GenHash("NewEntitySet");
            }

            mlo.AddEntitySet(set);

            var mloInstance = TryGetMloInstance(mlo);
            if (mloInstance != null)
            {
                mloInstance.AddEntitySet(set);
            }

            LoadProjectTree();
            ProjectExplorer?.TrySelectMloEntitySetTreeNode(set);
            CurrentMloEntitySet = set;
            CurrentYtypFile = set?.OwnerMlo?.Ytyp;

            return set;
        }
        public bool DeleteArchetype()
        {
            if (CurrentArchetype == null) return false;
            if (CurrentArchetype.Ytyp != CurrentYtypFile) return false;

            if (MessageBox.Show("Are you sure you want to delete this archetype?\n" + CurrentArchetype._BaseArchetypeDef.name.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentArchetype.Ytyp.RemoveArchetype(CurrentArchetype);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentArchetype.Ytyp.RemoveArchetype(CurrentArchetype);
            }
            if (!res)
            {
                MessageBox.Show("Archetype couldn't be removed!");
                return false;
            }

            var delarch = CurrentArchetype;
            var delytyp = delarch.Ytyp;

            RemoveProjectArchetype(delarch);

            ProjectExplorer?.RemoveArchetypeTreeNode(delarch);
            ProjectExplorer?.SetYtypHasChanged(delytyp, true);

            ClosePanel((EditYtypArchetypePanel p) => { return p.Tag == delarch; });

            CurrentArchetype = null;

            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool DeleteMloEntity()
        {
            if (CurrentEntity?.MloParent?.Archetype?.Ytyp == null) return false;
            if (CurrentEntity.MloParent.Archetype.Ytyp != CurrentYtypFile) return false;
            if (!(CurrentEntity.MloParent.Archetype is MloArchetype mloArchetype)) return false;
            if (mloArchetype.entities == null) return false; //nothing to delete..
            //if (mloArchetype.InstancedEntities == null) return false; //nothing to delete..

            if (CurrentEntity._CEntityDef.numChildren != 0)
            {
                MessageBox.Show("This entity's numChildren is not 0 - deleting entities with children is not currently supported by CodeWalker.");
                return true;
            }

            //if (MessageBox.Show("Are you sure you want to delete this entity?\n" + CurrentEntity._CEntityDef.archetypeName.ToString() + "\n" + CurrentEntity.Position.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

            MloInstanceData mloInstance = CurrentEntity.MloParent.MloInstance;
            if (mloInstance == null) return false;


            var delent = CurrentEntity; //CurrentEntity could get changed when we remove the tree node..
            var delytyp = CurrentEntity.MloParent.Archetype.Ytyp;
            var mcEnt = mloInstance.TryGetArchetypeEntity(CurrentEntity);

            ProjectExplorer?.RemoveMloEntityTreeNode(mcEnt);
            ProjectExplorer?.SetYtypHasChanged(delytyp, true);

            try
            {
                if (WorldForm != null)
                {
                    lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                    {
                        mloArchetype.RemoveEntity(delent);
                        mloInstance.DeleteEntity(delent);
                        //WorldForm.SelectItem(null, null, null);
                    }
                }
                else
                {
                    mloArchetype.RemoveEntity(delent);
                    mloInstance.DeleteEntity(delent);
                }
            }
            catch (Exception e) // various failures could happen so we'll use a trycatch for when an exception is thrown.
            {
                MessageBox.Show(this, "Cannot delete entity: " + Environment.NewLine + e.Message);
                return false;
            }


            ClosePanel((EditYmapEntityPanel p) => { return p.Tag == delent; });
            CurrentEntity = null;
            CurrentMloEntity = null;

            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool DeleteMloRoom()
        {
            var mlo = CurrentMloRoom?.OwnerMlo;
            if (mlo == null) return false;

            if (MessageBox.Show("Are you sure you want to delete this room?\n" + CurrentMloRoom.Name + "\n\nDeleting existing rooms is generally not recommended, as it will mess up all the room IDs.\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            mlo.RemoveRoom(CurrentMloRoom);

            var mloInstance = TryGetMloInstance(mlo);
            if (mloInstance != null)
            {
            }

            ProjectExplorer?.RemoveMloRoomTreeNode(CurrentMloRoom);
            ProjectExplorer?.SetYtypHasChanged(mlo.Ytyp, true);
            ClosePanel((EditYtypMloRoomPanel p) => { return p.Tag == CurrentMloRoom; });
            CurrentMloRoom = null;

            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool DeleteMloPortal()
        {
            var mlo = CurrentMloPortal?.OwnerMlo;
            if (mlo == null) return false;

            if (MessageBox.Show("Are you sure you want to delete this portal?\n" + CurrentMloPortal.Name + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            mlo.RemovePortal(CurrentMloPortal);

            var mloInstance = TryGetMloInstance(mlo);
            if (mloInstance != null)
            {
            }

            ProjectExplorer?.RemoveMloPortalTreeNode(CurrentMloPortal);
            ProjectExplorer?.SetYtypHasChanged(mlo.Ytyp, true);
            ClosePanel((EditYtypMloPortalPanel p) => { return p.Tag == CurrentMloPortal; });
            CurrentMloPortal = null;

            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool DeleteMloEntitySet()
        {
            var mlo = CurrentMloEntitySet?.OwnerMlo;
            if (mlo == null) return false;

            if (MessageBox.Show("Are you sure you want to delete this entity set?\n" + CurrentMloEntitySet.Name + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            mlo.RemoveEntitySet(CurrentMloEntitySet);

            var mloInstance = TryGetMloInstance(mlo);
            if (mloInstance != null)
            {
                mloInstance.DeleteEntitySet(CurrentMloEntitySet);
            }

            ProjectExplorer?.RemoveMloEntitySetTreeNode(CurrentMloEntitySet);
            ProjectExplorer?.SetYtypHasChanged(mlo.Ytyp, true);
            ClosePanel((EditYtypMloEntSetPanel p) => { return p.Tag == CurrentMloEntitySet; });
            CurrentMloEntitySet = null;

            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }

        private void AddProjectArchetypes(YtypFile ytyp)
        {
            if (ytyp?.AllArchetypes == null) return;
            foreach (var arch in ytyp.AllArchetypes)
            {
                AddProjectArchetype(arch);
            }
        }
        private void AddProjectArchetype(Archetype arch)
        {
            if ((arch?.Hash ?? 0) == 0) return;
            lock (projectsyncroot)
            {
                projectarchetypes[arch.Hash] = arch;
            }
        }
        private void RemoveProjectArchetypes(YtypFile ytyp)
        {
            if (ytyp?.AllArchetypes == null) return;
            foreach (var arch in ytyp.AllArchetypes)
            {
                RemoveProjectArchetype(arch);
            }
        }
        private void RemoveProjectArchetype(Archetype arch)
        {
            if ((arch?.Hash ?? 0) == 0) return;
            Archetype tarch = null;
            lock (projectsyncroot)
            {
                projectarchetypes.TryGetValue(arch.Hash, out tarch);
                if (tarch == arch)
                {
                    projectarchetypes.Remove(arch.Hash);
                }
            }
        }







        public void NewYbn()
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
                fname = "bounds" + testi.ToString() + ".ybn";
                filenameok = !CurrentProjectFile.ContainsYbn(fname);
                testi++;
            }

            lock (projectsyncroot)
            {
                YbnFile ybn = CurrentProjectFile.AddYbnFile(fname);
                if (ybn != null)
                {
                    ybn.Loaded = true;
                    ybn.HasChanged = true; //new ynd, flag as not saved
                }
            }

            CurrentProjectFile.HasChanged = true;

            LoadProjectTree();
        }
        public void OpenYbn()
        {
            string[] files = ShowOpenDialogMulti("Ybn files|*.ybn", string.Empty);
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

                var ybn = CurrentProjectFile.AddYbnFile(file);

                if (ybn != null)
                {
                    SetProjectHasChanged(true);

                    LoadYbnFromFile(ybn, file);

                    LoadProjectTree();
                }
                else
                {
                    MessageBox.Show("Couldn't add\n" + file + "\n - the file already exists in the project.");
                }

            }
        }
        public void SaveYbn(bool saveas = false)
        {
            if ((CurrentYbnFile == null) && (CurrentCollisionBounds != null)) CurrentYbnFile = CurrentCollisionBounds.GetRootYbn();
            if (CurrentYbnFile == null) return;


            string ybnname = CurrentYbnFile.Name;
            string filepath = CurrentYbnFile.FilePath;
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = ybnname;
            }
            string origfile = filepath;
            if (!File.Exists(filepath))
            {
                saveas = true;
            }


            byte[] data;
            lock (projectsyncroot) //need to sync writes to ybn objects...
            {
                saveas = saveas || string.IsNullOrEmpty(filepath);
                if (saveas)
                {
                    filepath = ShowSaveDialog("Ybn files|*.ybn", filepath);
                    if (string.IsNullOrEmpty(filepath))
                    { return; }

                    string newname = Path.GetFileNameWithoutExtension(filepath);
                    JenkIndex.Ensure(newname);
                    CurrentYbnFile.FilePath = filepath;
                    CurrentYbnFile.RpfFileEntry.Name = new FileInfo(filepath).Name;
                    CurrentYbnFile.Name = CurrentYbnFile.RpfFileEntry.Name;
                }


                data = CurrentYbnFile.Save();
            }


            if (data != null)
            {
                File.WriteAllBytes(filepath, data);
            }

            SetYbnHasChanged(false);

            if (saveas)
            {
                //ShowEditYbnPanel(false);
                if (CurrentProjectFile != null)
                {
                    string origpath = CurrentProjectFile.GetRelativePath(origfile);
                    string newpath = CurrentProjectFile.GetRelativePath(CurrentYbnFile.FilePath);
                    if (!CurrentProjectFile.RenameYbn(origpath, newpath))
                    { //couldn't rename it in the project? happens when project not saved yet...
                        //MessageBox.Show("Couldn't rename ybn in project! This shouldn't happen - check the project file XML.");
                    }
                    ProjectExplorer?.UpdateYbnTreeNode(CurrentYbnFile);
                }
                SetProjectHasChanged(true);
                SetCurrentSaveItem();
            }

        }
        public void AddYbnToProject(YbnFile ybn)
        {
            if (ybn == null) return;
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (YbnExistsInProject(ybn)) return;
            if (CurrentProjectFile.AddYbnFile(ybn))
            {
                ybn.HasChanged = true;
                CurrentProjectFile.HasChanged = true;
                LoadProjectTree();
            }
            CurrentYbnFile = ybn;
            RefreshUI();
            if (CurrentCollisionVertex != null)
            {
                ProjectExplorer?.TrySelectCollisionVertexTreeNode(CurrentCollisionVertex);
            }
            else if (CurrentCollisionPoly != null)
            {
                ProjectExplorer?.TrySelectCollisionPolyTreeNode(CurrentCollisionPoly);
            }
            else if (CurrentCollisionBounds != null)
            {
                ProjectExplorer?.TrySelectCollisionBoundsTreeNode(CurrentCollisionBounds);
            }
        }
        public void RemoveYbnFromProject()
        {
            if (CurrentYbnFile == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveYbnFile(CurrentYbnFile);
            CurrentYbnFile = null;
            LoadProjectTree();
            RefreshUI();
        }
        public bool YbnExistsInProject(YbnFile ybn)
        {
            if (ybn == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsYbn(ybn);
        }

        public Bounds NewCollisionBounds(BoundsType type, Bounds copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (CurrentYbnFile == null) return null;

            Bounds b = null;
            switch (type)
            {
                case BoundsType.Sphere:
                    b = new BoundSphere();
                    break;
                case BoundsType.Capsule:
                    b = new BoundCapsule();
                    break;
                case BoundsType.Box:
                    b = new BoundBox();
                    break;
                case BoundsType.Geometry:
                    b = new BoundGeometry();
                    break;
                case BoundsType.GeometryBVH:
                    b = new BoundBVH();
                    break;
                case BoundsType.Composite:
                    b = new BoundComposite();
                    break;
                case BoundsType.Disc:
                    b = new BoundDisc();
                    break;
                case BoundsType.Cylinder:
                    b = new BoundCylinder();
                    break;
                case BoundsType.Cloth:
                    b = new BoundCloth();
                    break;
            }

            if (b == null) return null;

            if ((copy != null) && (copy.Type == type))
            {
                b.CopyFrom(copy);
            }
            else
            {
                var pos = GetSpawnPos(10.0f);

                b.Type = type;
                b.Margin = 0.005f;
                b.Volume = 1.0f;
                b.Unknown_60h = Vector3.One;
                b.Unknown_3Ch = 1;
                b.SphereCenter = Vector3.Zero;
                b.SphereRadius = 1.0f;
                b.BoxCenter = Vector3.Zero;
                b.BoxMin = -Vector3.One;
                b.BoxMax = Vector3.One;
                b.Position = pos;

                if (b is BoundBVH bbvh)
                { }
                if (b is BoundGeometry bgeo)
                {
                    bgeo.CenterGeom = Vector3.Zero;
                    bgeo.Quantum = Vector3.Zero;
                    bgeo.Unknown_9Ch = 7.629627e-8f;
                    bgeo.Unknown_ACh = 0.0025f;
                    bgeo.Position = Vector3.Zero;//start geometry transform at 0
                }
                if (b is BoundComposite bcmp)
                {
                    bcmp.Position = Vector3.Zero;//start composite transform at 0
                }
            }


            var bcomp = (CurrentCollisionBounds as BoundComposite) ?? CurrentCollisionBounds?.Parent;
            if (bcomp != null)
            {
                bcomp.AddChild(b);
            }
            else
            {
                CurrentYbnFile.AddBounds(b);
            }


            if (selectNew)
            {
                LoadProjectTree();
                ProjectExplorer?.TrySelectCollisionBoundsTreeNode(b);
                CurrentCollisionBounds = b;
                //ShowEditYbnPanel(false);;
                ShowEditYbnBoundsPanel(false);
            }

            if (bcomp != null)
            {
                if (WorldForm != null)
                {
                    WorldForm.UpdateCollisionBoundsGraphics(bcomp);
                }
            }


            return b;
        }
        public void AddCollisionBoundsToProject()
        {
            try
            {
                if (CurrentCollisionBounds == null) return;

                CurrentYbnFile = CurrentCollisionBounds.GetRootYbn();
                if (CurrentYbnFile == null)
                {
                    MessageBox.Show("Sorry, only YBN collisions can currently be added to the project. Embedded collisions TODO!");
                    return;
                }

                if (!YbnExistsInProject(CurrentYbnFile))
                {
                    var b = CurrentCollisionBounds;
                    CurrentYbnFile.HasChanged = true;
                    AddYbnToProject(CurrentYbnFile);

                    CurrentCollisionBounds = b; //bug fix for some reason the treeview selects the project node here.
                    CurrentYbnFile = b.GetRootYbn();
                    ProjectExplorer?.TrySelectCollisionBoundsTreeNode(b);
                }
            }
            catch
            { }
        }
        public bool DeleteCollisionBounds()
        {
            if (CurrentCollisionBounds == null) return false;
            if (CurrentYbnFile == null) return false;
            if (CurrentCollisionBounds.GetRootYbn() != CurrentYbnFile) return false;

            if (MessageBox.Show("Are you sure you want to delete this collision bounds?\n" + CurrentCollisionBounds.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            var parent = CurrentCollisionBounds.Parent;

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    if (parent != null)
                    {
                        res = parent.DeleteChild(CurrentCollisionBounds);
                    }
                    else
                    {
                        res = CurrentYbnFile.RemoveBounds(CurrentCollisionBounds);
                    }
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                if (parent != null)
                {
                    res = parent.DeleteChild(CurrentCollisionBounds);
                }
                else
                {
                    res = CurrentYbnFile.RemoveBounds(CurrentCollisionBounds);
                }
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the collision bounds. This shouldn't happen!");
            }

            var delb = CurrentCollisionBounds;

            ProjectExplorer?.RemoveCollisionBoundsTreeNode(CurrentCollisionBounds);
            ProjectExplorer?.SetYbnHasChanged(CurrentYbnFile, true);

            ClosePanel((EditYbnBoundsPanel p) => { return p.Tag == delb; });

            CurrentCollisionBounds = null;

            if (WorldForm != null)
            {
                if (parent != null)
                {
                    WorldForm.UpdateCollisionBoundsGraphics(parent);
                }
                WorldForm.SelectItem(null);
            }


            return true;
        }
        public bool IsCurrentCollisionBounds(Bounds bounds)
        {
            return bounds == CurrentCollisionBounds;
        }

        public BoundPolygon NewCollisionPoly(BoundPolygonType type, BoundPolygon copy = null, bool copyPosition = false, bool selectNew = true)
        {
            var bgeom = CurrentCollisionBounds as BoundGeometry;
            if (bgeom == null) return null;


            var poly = bgeom.AddPolygon(type);
            var ptri = poly as BoundPolygonTriangle;
            var psph = poly as BoundPolygonSphere;
            var pcap = poly as BoundPolygonCapsule;
            var pbox = poly as BoundPolygonBox;
            var pcyl = poly as BoundPolygonCylinder;
            var ctri = copy as BoundPolygonTriangle;
            var csph = copy as BoundPolygonSphere;
            var ccap = copy as BoundPolygonCapsule;
            var cbox = copy as BoundPolygonBox;
            var ccyl = copy as BoundPolygonCylinder;

            if (ptri != null)
            {
                ptri.edgeIndex1 = -1;
                ptri.edgeIndex2 = -1;
                ptri.edgeIndex3 = -1;
            }

            if (copy != null)
            {
                poly.VertexPositions = copy.VertexPositions;
                poly.Material = copy.Material;
                switch (type)
                {
                    case BoundPolygonType.Triangle:
                        if ((ptri != null) && (ctri != null))
                        {
                            ptri.vertFlag1 = ctri.vertFlag1;
                            ptri.vertFlag2 = ctri.vertFlag2;
                            ptri.vertFlag3 = ctri.vertFlag3;
                        }
                        break;
                    case BoundPolygonType.Sphere:
                        if ((psph != null) && (csph != null))
                        {
                            psph.sphereRadius = csph.sphereRadius;
                        }
                        break;
                    case BoundPolygonType.Capsule:
                        if ((pcap != null) && (ccap != null))
                        {
                            pcap.capsuleRadius = ccap.capsuleRadius;
                        }
                        break;
                    case BoundPolygonType.Box:
                        if ((pbox != null) && (cbox != null))
                        {
                        }
                        break;
                    case BoundPolygonType.Cylinder:
                        if ((pcyl != null) && (ccyl != null))
                        {
                            pcyl.cylinderRadius = ccyl.cylinderRadius;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                var pos = GetSpawnPos(10.0f);
                var x = Vector3.UnitX;
                var y = Vector3.UnitY;
                var z = Vector3.UnitZ;

                if (ptri != null)
                {
                    ptri.VertexPositions = new[] { pos, pos + x, pos + y };
                }
                if (psph != null)
                {
                    psph.VertexPositions = new[] { pos };
                    psph.sphereRadius = 1.0f;
                }
                if (pcap != null)
                {
                    pcap.VertexPositions = new[] { pos - x, pos + x };
                    pcap.capsuleRadius = 1.0f;
                }
                if (pbox != null)
                {
                    pbox.VertexPositions = new[] { pos - x + y - z, pos - x - y + z, pos + x + y + z, pos + x - y - z };
                }
                if (pcyl != null)
                {
                    pcyl.VertexPositions = new[] { pos - x, pos + x };
                    pcyl.cylinderRadius = 1.0f;
                }
            }

            if (selectNew)
            {
                //LoadProjectTree();//is this necessary?
                ProjectExplorer?.TrySelectCollisionPolyTreeNode(poly);
                CurrentCollisionPoly = poly;
                //ShowEditYbnPanel(false);;
                ShowEditYbnBoundPolyPanel(false);
            }

            if (WorldForm != null)
            {
                WorldForm.UpdateCollisionBoundsGraphics(bgeom);
            }


            return poly;
        }
        public void AddCollisionPolyToProject()
        {
            try
            {
                if (CurrentCollisionPoly == null) return;

                CurrentYbnFile = CurrentCollisionPoly.Owner?.GetRootYbn();
                if (CurrentYbnFile == null)
                {
                    MessageBox.Show("Sorry, only YBN collisions can currently be added to the project. Embedded collisions TODO!");
                    return;
                }

                if (!YbnExistsInProject(CurrentYbnFile))
                {
                    var p = CurrentCollisionPoly;
                    CurrentYbnFile.HasChanged = true;
                    AddYbnToProject(CurrentYbnFile);

                    CurrentCollisionPoly = p; //bug fix for some reason the treeview selects the project node here.
                    CurrentCollisionBounds = p.Owner;
                    CurrentYbnFile = p.Owner?.GetRootYbn();
                    ProjectExplorer?.TrySelectCollisionPolyTreeNode(p);
                }
            }
            catch
            { }
        }
        public bool DeleteCollisionPoly()
        {
            if (CurrentCollisionBounds == null) return false;
            if (CurrentCollisionPoly == null) return false;
            if (CurrentYbnFile == null) return false;
            if (CurrentCollisionPoly.Owner != CurrentCollisionBounds) return false;

            //if (MessageBox.Show("Are you sure you want to delete this collision poly?\n" + CurrentCollisionPoly.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentCollisionPoly.Owner.DeletePolygon(CurrentCollisionPoly);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentCollisionPoly.Owner.DeletePolygon(CurrentCollisionPoly);
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the collision poly. This shouldn't happen!");
            }

            var delp = CurrentCollisionPoly;

            //ProjectExplorer?.RemoveCollisionPolyTreeNode(CurrentCollisionPoly);
            ProjectExplorer?.SetYbnHasChanged(CurrentYbnFile, true);

            ClosePanel((EditYbnBoundPolyPanel p) => { return p.Tag == delp; });

            CurrentCollisionPoly = null;

            if (WorldForm != null)
            {
                WorldForm.UpdateCollisionBoundsGraphics(CurrentCollisionBounds);
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool IsCurrentCollisionPoly(BoundPolygon poly)
        {
            return poly == CurrentCollisionPoly;
        }

        public void AddCollisionVertexToProject()
        {
            try
            {
                if (CurrentCollisionVertex == null) return;

                CurrentYbnFile = CurrentCollisionVertex.Owner?.GetRootYbn();
                if (CurrentYbnFile == null)
                {
                    MessageBox.Show("Sorry, only YBN collisions can currently be added to the project. Embedded collisions TODO!");
                    return;
                }

                if (!YbnExistsInProject(CurrentYbnFile))
                {
                    var v = CurrentCollisionVertex;
                    CurrentYbnFile.HasChanged = true;
                    AddYbnToProject(CurrentYbnFile);

                    CurrentCollisionVertex = v; //bug fix for some reason the treeview selects the project node here.
                    CurrentCollisionBounds = v.Owner;
                    CurrentYbnFile = v.Owner?.GetRootYbn();
                    ProjectExplorer?.TrySelectCollisionVertexTreeNode(v);
                }
            }
            catch
            { }
        }
        public bool DeleteCollisionVertex()
        {
            if (CurrentCollisionBounds == null) return false;
            if (CurrentCollisionVertex == null) return false;
            if (CurrentYbnFile == null) return false;
            if (CurrentCollisionVertex.Owner != CurrentCollisionBounds) return false;

            //if (MessageBox.Show("Are you sure you want to delete this collision vertex, and all attached polygons?\n" + CurrentCollisionVertex.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentCollisionVertex.Owner.DeleteVertex(CurrentCollisionVertex.Index);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentCollisionVertex.Owner.DeleteVertex(CurrentCollisionVertex.Index);
            }
            if (!res)
            {
                MessageBox.Show("Unable to delete the collision vertex. This shouldn't happen!");
            }

            var delv = CurrentCollisionVertex;

            //ProjectExplorer?.RemoveCollisionVertexTreeNode(CurrentCollisionVertex);
            ProjectExplorer?.SetYbnHasChanged(CurrentYbnFile, true);

            ClosePanel((EditYbnBoundVertexPanel p) => { return p.Tag == delv; });

            CurrentCollisionVertex = null;

            if (WorldForm != null)
            {
                WorldForm.UpdateCollisionBoundsGraphics(CurrentCollisionBounds);
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool IsCurrentCollisionVertex(BoundVertex vertex)
        {
            return vertex == CurrentCollisionVertex;
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

            // Check that vehicle nodes and ped nodes add up to total nodes
            if(CurrentYndFile.NodeDictionary != null && (CurrentYndFile.NodeDictionary.NodesCountPed + CurrentYndFile.NodeDictionary.NodesCountVehicle != CurrentYndFile.NodeDictionary.NodesCount))
            {
                var result = MessageBox.Show($"YND Area {CurrentYndFile.AreaID}: The total number of nodes ({CurrentYndFile.NodeDictionary.NodesCount}) does not match the total number of ped ({CurrentYndFile.NodeDictionary.NodesCountPed}) and vehicle ({CurrentYndFile.NodeDictionary.NodesCountVehicle}) nodes. You should manually adjust the number of nodes on the YND screen.\n\nDo you want to continue saving the YND file? Some of your nodes may not work in game.", $"Node count mismatch in Area {CurrentYndFile.AreaID}", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if(result == DialogResult.Cancel)
                {
                    return;
                }
            }

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
                    ProjectExplorer?.UpdateYndTreeNode(CurrentYndFile);
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

        public YndNode NewPathNode(YndNode copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (CurrentYndFile == null) return null;

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


            if (selectNew)
            {
                LoadProjectTree();
                ProjectExplorer?.TrySelectPathNodeTreeNode(n);
                CurrentPathNode = n;
                //ShowEditYndPanel(false);;
                ShowEditYndNodePanel(false);
            }


            if (WorldForm != null)
            {
                WorldForm.UpdatePathYndGraphics(CurrentYndFile, false);
            }

            return n;
        }
        public bool DeletePathNode()
        {
            if (CurrentYndFile == null) return false;
            if (CurrentPathNode == null) return false;
            if (CurrentPathNode.Ynd != CurrentYndFile) return false;
            if (CurrentYndFile.Nodes == null) return false; //nothing to delete..

            //if (MessageBox.Show("Are you sure you want to delete this path node?\n" + CurrentPathNode.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

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
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool IsCurrentPathNode(YndNode pathnode)
        {
            return CurrentPathNode == pathnode;
        }








        public void NewYnv()
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
                fname = "navmesh" + testi.ToString() + ".ynv";
                filenameok = !CurrentProjectFile.ContainsYnv(fname);
                testi++;
            }

            lock (projectsyncroot)
            {
                YnvFile ynv = CurrentProjectFile.AddYnvFile(fname);
                if (ynv != null)
                {
                    ynv.Loaded = true;
                    ynv.HasChanged = true; //new ynd, flag as not saved

                    //TODO: set new ynv default values...
                    ynv.Nav = new NavMesh();
                }
            }

            CurrentProjectFile.HasChanged = true;

            LoadProjectTree();
        }
        public void OpenYnv()
        {
            string[] files = ShowOpenDialogMulti("Ynv files|*.ynv", string.Empty);
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

                var ynv = CurrentProjectFile.AddYnvFile(file);

                if (ynv != null)
                {
                    SetProjectHasChanged(true);

                    LoadYnvFromFile(ynv, file);

                    LoadProjectTree();
                }
                else
                {
                    MessageBox.Show("Couldn't add\n" + file + "\n - the file already exists in the project.");
                }

            }
        }
        public void SaveYnv(bool saveas = false)
        {
            if ((CurrentYnvFile == null) && (CurrentNavPoly != null)) CurrentYnvFile = CurrentNavPoly.Ynv;
            if (CurrentYnvFile == null) return;
            string ynvname = CurrentYnvFile.Name;
            string filepath = CurrentYnvFile.FilePath;
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = ynvname;
            }
            string origfile = filepath;
            if (!File.Exists(filepath))
            {
                saveas = true;
            }


            byte[] data;
            lock (projectsyncroot) //need to sync writes to ynv objects...
            {
                saveas = saveas || string.IsNullOrEmpty(filepath);
                if (saveas)
                {
                    filepath = ShowSaveDialog("Ynv files|*.ynv", filepath);
                    if (string.IsNullOrEmpty(filepath))
                    { return; }

                    string newname = Path.GetFileNameWithoutExtension(filepath);
                    JenkIndex.Ensure(newname);
                    CurrentYnvFile.FilePath = filepath;
                    CurrentYnvFile.RpfFileEntry.Name = new FileInfo(filepath).Name;
                    CurrentYnvFile.Name = CurrentYnvFile.RpfFileEntry.Name;
                }


                data = CurrentYnvFile.Save();
            }


            if (data != null)
            {
                File.WriteAllBytes(filepath, data);
            }

            SetYnvHasChanged(false);

            if (saveas)
            {
                //ShowEditYnvPanel(false);
                if (CurrentProjectFile != null)
                {
                    string origpath = CurrentProjectFile.GetRelativePath(origfile);
                    string newpath = CurrentProjectFile.GetRelativePath(CurrentYnvFile.FilePath);
                    if (!CurrentProjectFile.RenameYnv(origpath, newpath))
                    { //couldn't rename it in the project? happens when project not saved yet...
                        //MessageBox.Show("Couldn't rename ynv in project! This shouldn't happen - check the project file XML.");
                    }
                    ProjectExplorer?.UpdateYnvTreeNode(CurrentYnvFile);
                }
                SetProjectHasChanged(true);
                SetCurrentSaveItem();
            }
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

        public YnvPoly NewNavPoly(YnvPoly copy = null, bool copyposition = false, bool selectNew = true)//TODO!
        {
            return null;
        }
        public bool DeleteNavPoly()//TODO!
        {
            return false;
        }
        public bool IsCurrentNavPoly(YnvPoly poly)
        {
            return poly == CurrentNavPoly;
        }

        public YnvPoint NewNavPoint(YnvPoint copy = null, bool copyposition = false, bool selectNew = true)//TODO!
        {
            return null;
        }
        public bool DeleteNavPoint()//TODO!
        {
            return false;
        }
        public bool IsCurrentNavPoint(YnvPoint point)
        {
            return point == CurrentNavPoint;
        }

        public YnvPortal NewNavPortal(YnvPortal copy = null, bool copyposition = false, bool selectNew = true)//TODO!
        {
            return null;
        }
        public bool DeleteNavPortal()//TODO!
        {
            return false;
        }
        public bool IsCurrentNavPortal(YnvPortal portal)
        {
            return portal == CurrentNavPortal;
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
                    ProjectExplorer?.UpdateTrainTrackTreeNode(CurrentTrainTrack);
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

        public TrainTrackNode NewTrainNode(TrainTrackNode copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (CurrentTrainTrack == null) return null;

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


            if (selectNew)
            {
                LoadProjectTree();
                ProjectExplorer?.TrySelectTrainNodeTreeNode(n);
                CurrentTrainNode = n;
                ShowEditTrainNodePanel(false);
            }


            if (WorldForm != null)
            {
                WorldForm.UpdateTrainTrackGraphics(CurrentTrainTrack, false);
            }

            return n;
        }
        public bool DeleteTrainNode()
        {
            if (CurrentTrainTrack == null) return false;
            if (CurrentTrainNode == null) return false;
            if (CurrentTrainNode.Track != CurrentTrainTrack) return false;
            if (CurrentTrainTrack.Nodes == null) return false; //nothing to delete..

            //if (MessageBox.Show("Are you sure you want to delete this train track node?\n" + CurrentTrainNode.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

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
                WorldForm.SelectItem(null);
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
                    ProjectExplorer?.UpdateScenarioTreeNode(CurrentScenario);
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

        public ScenarioNode NewScenarioNode(ScenarioNode copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (CurrentScenario == null) return null;
            if (CurrentScenario.ScenarioRegion == null) return null;

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


            if (selectNew)
            {
                LoadProjectTree();
                ProjectExplorer?.TrySelectScenarioNodeTreeNode(n);
                CurrentScenarioNode = n;
                //ShowEditScenarioPanel(false);
                ShowEditScenarioNodePanel(false);
            }


            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }
            else
            {
                CurrentScenario.ScenarioRegion.BuildBVH();
                CurrentScenario.ScenarioRegion.BuildVertices(); //for the graphics...
            }

            return n;
        }
        public bool DeleteScenarioNode()
        {
            if (CurrentScenario == null) return false;
            if (CurrentScenario.ScenarioRegion == null) return false;
            if (CurrentScenarioNode == null) return false;


            //if (MessageBox.Show("Are you sure you want to delete this scenario node?\n" + CurrentScenarioNode.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

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
                WorldForm.SelectItem(null);
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
                var action = CScenarioChainingEdge__eAction.Move;
                var navMode = CScenarioChainingEdge__eNavMode.Direct;
                var navSpeed = CScenarioChainingEdge__eNavSpeed.Unk_00_3279574318;
                var stype = new ScenarioTypeRef(defaulttype);
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
                    navSpeed = (CScenarioChainingEdge__eNavSpeed)nsb;
                }
                if (vals.Length > 5)
                {
                    switch (vals[5].Trim())
                    {
                        case "Direct": navMode = CScenarioChainingEdge__eNavMode.Direct; break;
                        case "NavMesh": navMode = CScenarioChainingEdge__eNavMode.NavMesh; break;
                        case "Roads": navMode = CScenarioChainingEdge__eNavMode.Roads; break;
                    }
                }
                if (vals.Length > 6)
                {
                    var sthash = JenkHash.GenHash(vals[6].Trim().ToLowerInvariant());
                    var st = stypes?.GetScenarioType(sthash);
                    if (st != null)
                    {
                        stype = new ScenarioTypeRef(st);
                    }
                    else
                    {
                        var stg = stypes?.GetScenarioTypeGroup(sthash);
                        if (stg != null)
                        {
                            stype = new ScenarioTypeRef(stg);
                        }
                        else
                        {
                            stype = new ScenarioTypeRef(defaulttype);
                        }
                    }
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
                thisnode.MyPoint.Flags = (CScenarioPointFlags__Flags)flags;

                thisnode.ChainingNode = new MCScenarioChainingNode();
                thisnode.ChainingNode.ScenarioNode = thisnode;
                thisnode.ChainingNode.Chain = chain;
                thisnode.ChainingNode.Type = stype;
                thisnode.ChainingNode.TypeHash = stype.NameHash;
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









        public void NewAudioFile()
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
                fname = "dlc" + testi.ToString() + "_game.dat151.rel";
                filenameok = !CurrentProjectFile.ContainsAudioRel(fname);
                testi++;
            }

            lock (projectsyncroot)
            {
                RelFile rel = CurrentProjectFile.AddAudioRelFile(fname);
                if (rel != null)
                {
                    rel.RelType = RelDatFileType.Dat151; //TODO: different types

                }
            }

            CurrentProjectFile.HasChanged = true;

            LoadProjectTree();
        }
        public void OpenAudioFile()
        {
            string[] files = ShowOpenDialogMulti("DatRel files|*.rel", string.Empty); //TODO: better filter?
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

                var rel = CurrentProjectFile.AddAudioRelFile(file);

                if (rel != null)
                {
                    SetProjectHasChanged(true);

                    LoadAudioRelFromFile(rel, file);

                    LoadProjectTree();
                }
                else
                {
                    MessageBox.Show("Couldn't add\n" + file + "\n - the file already exists in the project.");
                }

            }
        }
        public void SaveAudioFile(bool saveas = false)
        {
            if (CurrentAudioFile == null) return;
            string relname = CurrentAudioFile.Name;
            string filepath = CurrentAudioFile.FilePath;
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = relname;
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
                    filepath = ShowSaveDialog("DatRel files|*.rel", filepath);
                    if (string.IsNullOrEmpty(filepath))
                    {
                        return;
                    }

                    string newname = Path.GetFileNameWithoutExtension(filepath);
                    JenkIndex.Ensure(newname);
                    CurrentAudioFile.FilePath = filepath;
                    CurrentAudioFile.RpfFileEntry.Name = new FileInfo(filepath).Name;
                    CurrentAudioFile.Name = CurrentAudioFile.RpfFileEntry.Name;
                }

                data = CurrentAudioFile.Save();
            }

            if (data != null)
            {
                File.WriteAllBytes(filepath, data);
            }

            SetAudioFileHasChanged(false);

            if (saveas)
            {
                //ShowEditAudioFilePanel(false);
                if (CurrentProjectFile != null)
                {
                    string origpath = CurrentProjectFile.GetRelativePath(origfile);
                    string newpath = CurrentProjectFile.GetRelativePath(CurrentAudioFile.FilePath);
                    if (!CurrentProjectFile.RenameAudioRel(origpath, newpath))
                    { //couldn't rename it in the project? happens when project not saved yet...
                        //MessageBox.Show("Couldn't rename audio rel in project! This shouldn't happen - check the project file XML.");
                    }
                    ProjectExplorer?.UpdateAudioRelTreeNode(CurrentAudioFile);
                }
                SetProjectHasChanged(true);
                SetCurrentSaveItem();
            }
        }
        public void AddAudioFileToProject(RelFile rel)
        {
            if (rel == null) return;
            if (CurrentProjectFile == null)
            {
                NewProject();
            }
            if (AudioFileExistsInProject(rel)) return;
            if (CurrentProjectFile.AddAudioRelFile(rel))
            {
                rel.HasChanged = true;
                CurrentProjectFile.HasChanged = true;
                LoadProjectTree();
            }
            CurrentAudioFile = rel;
            RefreshUI();
            if (CurrentAudioZone != null)
            {
                ProjectExplorer?.TrySelectAudioZoneTreeNode(CurrentAudioZone);
            }
            else if (CurrentAudioEmitter != null)
            {
                ProjectExplorer?.TrySelectAudioEmitterTreeNode(CurrentAudioEmitter);
            }
        }
        public void RemoveAudioFileFromProject()
        {
            if (CurrentAudioFile == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveAudioRelFile(CurrentAudioFile);
            CurrentAudioFile = null;
            LoadProjectTree();
            RefreshUI();
        }
        public bool AudioFileExistsInProject(RelFile rel)
        {
            if (rel == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsAudioRel(rel);
        }

        public AudioPlacement NewAudioZone(AudioPlacement copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (CurrentAudioFile == null) return null;

            if (copy == null)
            {
                copy = CurrentAudioZone;
            }

            bool cp = copyPosition && (copy != null);

            var zone = new Dat151AmbientZone(CurrentAudioFile);

            //AA800424 box, line
            //AA800420 sphere
            zone.Flags0 = cp ? copy.AudioZone.Flags0.Value : 0xAA800424;
            zone.Flags1 = cp ? copy.AudioZone.Flags1 : 0;
            zone.Flags2 = cp ? copy.AudioZone.Flags2 : 0;
            zone.Shape = cp ? copy.AudioZone.Shape : Dat151ZoneShape.Box;
            zone.InnerSize = cp ? copy.AudioZone.InnerSize : Vector3.One * 10.0f;
            zone.InnerAngle = cp ? copy.AudioZone.InnerAngle : 0;
            zone.InnerVec1 = cp ? copy.AudioZone.InnerVec1 : Vector4.Zero;
            zone.InnerVec2 = cp ? copy.AudioZone.InnerVec2 : new Vector4(1, 1, 1, 0);
            zone.InnerVec3 = cp ? copy.AudioZone.InnerVec3 : Vector3.Zero;
            zone.OuterSize = cp ? copy.AudioZone.OuterSize : Vector3.One * 15.0f;
            zone.OuterAngle = cp ? copy.AudioZone.OuterAngle : 0;
            zone.OuterVec1 = cp ? copy.AudioZone.OuterVec1 : Vector4.Zero;
            zone.OuterVec2 = cp ? copy.AudioZone.OuterVec2 : new Vector4(1, 1, 1, 0);
            zone.OuterVec3 = cp ? copy.AudioZone.OuterVec3 : Vector3.Zero;
            zone.UnkVec1 = cp ? copy.AudioZone.UnkVec1 : new Vector4(0, 0, 1, 0);
            zone.UnkVec2 = cp ? copy.AudioZone.UnkVec2 : new Vector4(1, -1, -1, 0);
            zone.UnkHash0 = cp ? copy.AudioZone.UnkHash0 : 0;
            zone.UnkHash1 = cp ? copy.AudioZone.UnkHash1 : 0;
            zone.UnkVec3 = cp ? copy.AudioZone.UnkVec3 : new Vector2(-1, 0);
            zone.Unk14 = cp ? copy.AudioZone.Unk14 : (byte)4;
            zone.Unk15 = cp ? copy.AudioZone.Unk15 : (byte)1;
            zone.Unk16 = cp ? copy.AudioZone.Unk16 : (byte)0;
            zone.HashesCount = cp ? copy.AudioZone.HashesCount: (byte)0;
            zone.Hashes = cp ? copy.AudioZone.Hashes : null;
            zone.ExtParamsCount = cp ? copy.AudioZone.ExtParamsCount : 0;
            zone.ExtParams = cp ? copy.AudioZone.ExtParams : null;
            zone.Name = "zone1";
            zone.NameHash = JenkHash.GenHash(zone.Name);

            var ap = new AudioPlacement(CurrentAudioFile, zone);
            ap.Name = zone.Name;
            ap.NameHash = zone.NameHash;

            Vector3 pos = cp ? copy.Position : GetSpawnPos(20.0f);
            Quaternion ori = cp ? copy.Orientation : Quaternion.Identity;
            ap.SetPosition(pos);
            ap.SetOrientation(ori);


            CurrentAudioFile.AddRelData(zone);

            if (selectNew)
            {
                LoadProjectTree();

                ProjectExplorer?.TrySelectAudioZoneTreeNode(ap);
                CurrentAudioZone = ap;
            
                ShowEditAudioZonePanel(false);
            }

            if (WorldForm != null)
            {
                WorldForm.UpdateAudioPlacementGraphics(CurrentAudioFile);
            }

            return ap;
        }
        public bool DeleteAudioZone()
        {
            if (CurrentAudioZone?.RelFile != CurrentAudioFile) return false;
            if (CurrentAudioFile?.RelDatas == null) return false; //nothing to delete..
            if (CurrentAudioFile?.RelDatasSorted == null) return false; //nothing to delete..
            if (CurrentAudioZone?.AudioZone == null) return false;


            //if (MessageBox.Show("Are you sure you want to delete this audio zone?\n" + CurrentAudioZone.GetNameString() + "\n" + CurrentAudioZone.Position.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentAudioFile.RemoveRelData(CurrentAudioZone.AudioZone);

                    WorldForm.UpdateAudioPlacementGraphics(CurrentAudioFile);
                }
            }
            else
            {
                res = CurrentAudioFile.RemoveRelData(CurrentAudioZone.AudioZone);
            }
            if (!res)
            {
                MessageBox.Show("Unspecified error occurred when removing the audio zone from the file!");
            }

            var delzone = CurrentAudioZone;
            var delrel = CurrentAudioFile;

            ProjectExplorer?.RemoveAudioZoneTreeNode(delzone);
            ProjectExplorer?.SetAudioRelHasChanged(delrel, true);

            ClosePanel((EditAudioZonePanel p) => { return p.CurrentZone.AudioZone == delzone.AudioZone; });

            CurrentAudioZone = null;

            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool IsCurrentAudioZone(AudioPlacement zone)
        {
            return zone == CurrentAudioZone;
        }

        public AudioPlacement NewAudioEmitter(AudioPlacement copy = null, bool copyPosition = false, bool selectNew = true)
        {
            if (CurrentAudioFile == null) return null;

            if (copy == null)
            {
                copy = CurrentAudioEmitter;
            }

            bool cp = copyPosition && (copy != null);

            var emitter = new Dat151AmbientEmitter(CurrentAudioFile);

            emitter.Flags0 = cp ? copy.AudioEmitter.Flags0.Value : 0xAA001100;
            emitter.Flags5 = cp ? copy.AudioEmitter.Flags5.Value : 0xFFFFFFFF;
            emitter.InnerRad = cp ? copy.AudioEmitter.InnerRad : 0.0f;
            emitter.OuterRad = cp ? copy.AudioEmitter.OuterRad : 20.0f;
            emitter.Unk01 = cp ? copy.AudioEmitter.Unk01 : 1.0f;
            emitter.Unk02 = cp ? copy.AudioEmitter.Unk02.Value : (byte)0;
            emitter.Unk03 = cp ? copy.AudioEmitter.Unk03.Value : (byte)0;
            emitter.Unk04 = cp ? copy.AudioEmitter.Unk04.Value : (byte)160;
            emitter.Unk05 = cp ? copy.AudioEmitter.Unk05.Value : (byte)5;
            emitter.Unk06 = cp ? copy.AudioEmitter.Unk06.Value : (ushort)0;
            emitter.Unk07 = cp ? copy.AudioEmitter.Unk07.Value : (ushort)0;
            emitter.Unk08 = cp ? copy.AudioEmitter.Unk08.Value : (byte)0;
            emitter.Unk09 = cp ? copy.AudioEmitter.Unk09.Value : (byte)1;
            emitter.Unk10 = cp ? copy.AudioEmitter.Unk10.Value : (byte)1;
            emitter.Unk11 = cp ? copy.AudioEmitter.Unk11.Value : (byte)1;
            emitter.Unk12 = cp ? copy.AudioEmitter.Unk12.Value : (byte)100;
            emitter.Unk13 = cp ? copy.AudioEmitter.Unk13.Value : (byte)3;


            emitter.Name = "emitter1";
            emitter.NameHash = JenkHash.GenHash(emitter.Name);

            var ap = new AudioPlacement(CurrentAudioFile, emitter);
            ap.Name = emitter.Name;
            ap.NameHash = emitter.NameHash;

            Vector3 pos = cp ? copy.Position : GetSpawnPos(20.0f);
            Quaternion ori = cp ? copy.Orientation : Quaternion.Identity;
            ap.SetPosition(pos);
            ap.SetOrientation(ori);


            CurrentAudioFile.AddRelData(emitter);

            if (selectNew)
            {
                LoadProjectTree();

                ProjectExplorer?.TrySelectAudioEmitterTreeNode(ap);
                CurrentAudioEmitter = ap;

                ShowEditAudioEmitterPanel(false);
            }

            if (WorldForm != null)
            {
                WorldForm.UpdateAudioPlacementGraphics(CurrentAudioFile);
            }

            return ap;
        }
        public bool DeleteAudioEmitter()
        {
            if (CurrentAudioEmitter?.RelFile != CurrentAudioFile) return false;
            if (CurrentAudioFile?.RelDatas == null) return false; //nothing to delete..
            if (CurrentAudioFile?.RelDatasSorted == null) return false; //nothing to delete..
            if (CurrentAudioEmitter?.AudioEmitter == null) return false;


            //if (MessageBox.Show("Are you sure you want to delete this audio emitter?\n" + CurrentAudioEmitter.GetNameString() + "\n" + CurrentAudioEmitter.Position.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentAudioFile.RemoveRelData(CurrentAudioEmitter.AudioEmitter);
                    
                    WorldForm.UpdateAudioPlacementGraphics(CurrentAudioFile);
                }
            }
            else
            {
                res = CurrentAudioFile.RemoveRelData(CurrentAudioEmitter.AudioEmitter);
            }
            if (!res)
            {
                MessageBox.Show("Unspecified error occurred when removing the audio emitter from the file!");
            }

            var delem = CurrentAudioEmitter;
            var delrel = CurrentAudioFile;

            ProjectExplorer?.RemoveAudioEmitterTreeNode(delem);
            ProjectExplorer?.SetAudioRelHasChanged(delrel, true);

            CurrentAudioEmitter = null;

            ClosePanel((EditAudioEmitterPanel p) => { return p.CurrentEmitter.AudioEmitter == delem.AudioEmitter; });


            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);
            }

            return true;
        }
        public bool IsCurrentAudioEmitter(AudioPlacement emitter)
        {
            return emitter == CurrentAudioEmitter;
        }

        public void NewAudioZoneList()
        {
            if (CurrentAudioFile == null) return;


            var zonelist = new Dat151AmbientZoneList(CurrentAudioFile);

            zonelist.Name = "zonelist1";
            zonelist.NameHash = JenkHash.GenHash(zonelist.Name);

            CurrentAudioFile.AddRelData(zonelist);

            LoadProjectTree();

            ProjectExplorer?.TrySelectAudioZoneListTreeNode(zonelist);
            CurrentAudioZoneList = zonelist;

            ShowEditAudioZoneListPanel(false);
        }
        public bool DeleteAudioZoneList()
        {
            if (CurrentAudioZoneList?.Rel != CurrentAudioFile) return false;
            if (CurrentAudioFile?.RelDatas == null) return false; //nothing to delete..
            if (CurrentAudioFile?.RelDatasSorted == null) return false; //nothing to delete..


            //if (MessageBox.Show("Are you sure you want to delete this audio zone list?\n" + CurrentAudioZoneList.GetNameString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentAudioFile.RemoveRelData(CurrentAudioZoneList);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentAudioFile.RemoveRelData(CurrentAudioZoneList);
            }
            if (!res)
            {
                MessageBox.Show("Unspecified error occurred when removing the audio zone list from the file!");
            }

            var delzl = CurrentAudioZoneList;
            var delrel = CurrentAudioFile;

            ProjectExplorer?.RemoveAudioZoneListTreeNode(delzl);
            ProjectExplorer?.SetAudioRelHasChanged(delrel, true);

            ClosePanel((EditAudioZoneListPanel p) => { return p.Tag == delzl; });

            CurrentAudioZoneList = null;

            return true;
        }
        public bool IsCurrentAudioZoneList(Dat151AmbientZoneList list)
        {
            return list == CurrentAudioZoneList;
        }

        public void NewAudioEmitterList()
        {
            if (CurrentAudioFile == null) return;


            var emlist = new Dat151AmbientEmitterList(CurrentAudioFile);

            emlist.Name = "emitterlist1";
            emlist.NameHash = JenkHash.GenHash(emlist.Name);


            CurrentAudioFile.AddRelData(emlist);

            LoadProjectTree();

            ProjectExplorer?.TrySelectAudioEmitterListTreeNode(emlist);
            CurrentAudioEmitterList = emlist;

            ShowEditAudioEmitterListPanel(false);
        }
        public bool DeleteAudioEmitterList()
        {
            if (CurrentAudioEmitterList?.Rel != CurrentAudioFile) return false;
            if (CurrentAudioFile?.RelDatas == null) return false; //nothing to delete..
            if (CurrentAudioFile?.RelDatasSorted == null) return false; //nothing to delete..


            //if (MessageBox.Show("Are you sure you want to delete this audio emitter list?\n" + CurrentAudioEmitterList.GetNameString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //{
            //    return true;
            //}

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentAudioFile.RemoveRelData(CurrentAudioEmitterList);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentAudioFile.RemoveRelData(CurrentAudioEmitterList);
            }
            if (!res)
            {
                MessageBox.Show("Unspecified error occurred when removing the audio emitter list from the file!");
            }

            var delel = CurrentAudioEmitterList;
            var delrel = CurrentAudioFile;

            ProjectExplorer?.RemoveAudioEmitterListTreeNode(delel);
            ProjectExplorer?.SetAudioRelHasChanged(delrel, true);

            ClosePanel((EditAudioEmitterListPanel p) => { return p.Tag == delel; });

            CurrentAudioEmitterList = null;

            return true;
        }
        public bool IsCurrentAudioEmitterList(Dat151AmbientEmitterList list)
        {
            return list == CurrentAudioEmitterList;
        }

        public void NewAudioInterior()
        {
            if (CurrentAudioFile == null) return;


            var interior = new Dat151Interior(CurrentAudioFile);

            interior.Name = "interior1";
            interior.NameHash = JenkHash.GenHash(interior.Name);
            interior.Unk0 = 0xAAAAA844;
            interior.Unk1 = 0xD4855127;

            CurrentAudioFile.AddRelData(interior);

            LoadProjectTree();

            ProjectExplorer?.TrySelectAudioInteriorTreeNode(interior);
            CurrentAudioInterior = interior;

            ShowEditAudioInteriorPanel(false);
        }
        public bool DeleteAudioInterior()
        {
            if (CurrentAudioInterior?.Rel != CurrentAudioFile) return false;
            if (CurrentAudioFile?.RelDatas == null) return false; //nothing to delete..
            if (CurrentAudioFile?.RelDatasSorted == null) return false; //nothing to delete..


            if (MessageBox.Show("Are you sure you want to delete this audio interior?\n" + CurrentAudioInterior.GetNameString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentAudioFile.RemoveRelData(CurrentAudioInterior);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentAudioFile.RemoveRelData(CurrentAudioInterior);
            }
            if (!res)
            {
                MessageBox.Show("Unspecified error occurred when removing the audio interior from the file!");
            }

            var delel = CurrentAudioInterior;
            var delrel = CurrentAudioFile;

            ProjectExplorer?.RemoveAudioInteriorTreeNode(delel);
            ProjectExplorer?.SetAudioRelHasChanged(delrel, true);

            ClosePanel((EditAudioInteriorPanel p) => { return p.Tag == delel; });

            CurrentAudioInterior = null;

            return true;
        }
        public bool IsCurrentAudioInterior(Dat151Interior interior)
        {
            return interior == CurrentAudioInterior;
        }

        public void NewAudioInteriorRoom()
        {
            if (CurrentAudioFile == null) return;


            var room = new Dat151InteriorRoom(CurrentAudioFile);

            room.Name = "room1";
            room.NameHash = JenkHash.GenHash(room.Name);

            room.Flags0 = 0xAAAAAAAA;
            room.Unk06 = (uint)MetaName.null_sound;
            room.Unk14 = 3565506855;//?


            CurrentAudioFile.AddRelData(room);

            LoadProjectTree();

            ProjectExplorer?.TrySelectAudioInteriorRoomTreeNode(room);
            CurrentAudioInteriorRoom = room;

            ShowEditAudioInteriorRoomPanel(false);
        }
        public bool DeleteAudioInteriorRoom()
        {
            if (CurrentAudioInteriorRoom?.Rel != CurrentAudioFile) return false;
            if (CurrentAudioFile?.RelDatas == null) return false; //nothing to delete..
            if (CurrentAudioFile?.RelDatasSorted == null) return false; //nothing to delete..


            if (MessageBox.Show("Are you sure you want to delete this audio interior room?\n" + CurrentAudioInteriorRoom.GetNameString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return true;
            }

            bool res = false;
            if (WorldForm != null)
            {
                lock (WorldForm.RenderSyncRoot) //don't try to do this while rendering...
                {
                    res = CurrentAudioFile.RemoveRelData(CurrentAudioInteriorRoom);
                    //WorldForm.SelectItem(null, null, null);
                }
            }
            else
            {
                res = CurrentAudioFile.RemoveRelData(CurrentAudioInteriorRoom);
            }
            if (!res)
            {
                MessageBox.Show("Unspecified error occurred when removing the audio interior from the file!");
            }

            var delel = CurrentAudioInteriorRoom;
            var delrel = CurrentAudioFile;

            ProjectExplorer?.RemoveAudioInteriorRoomTreeNode(delel);
            ProjectExplorer?.SetAudioRelHasChanged(delrel, true);

            ClosePanel((EditAudioInteriorRoomPanel p) => { return p.Tag == delel; });

            CurrentAudioInteriorRoom = null;

            return true;
        }
        public bool IsCurrentAudioInteriorRoom(Dat151InteriorRoom room)
        {
            return room == CurrentAudioInteriorRoom;
        }













        public void GetVisibleYmaps(Camera camera, Dictionary<MetaHash, YmapFile> ymaps)
        {
            if (hidegtavmap)
            {
                ymaps.Clear(); //remove all the gtav ymaps.
            }

            lock (projectsyncroot)
            {
                if (renderitems && (CurrentProjectFile != null))
                {
                    for (int i = 0; i < CurrentProjectFile.YmapFiles.Count; i++)
                    {
                        var ymap = CurrentProjectFile.YmapFiles[i];
                        if (ymap.Loaded)
                        {
                            // make sure we're replacing ymaps that have been added by the end-user.
                            if (ymap.RpfFileEntry.ShortNameHash == 0)
                            {
                                ymap.RpfFileEntry.ShortNameHash = JenkHash.GenHash(ymap.RpfFileEntry.GetShortNameLower());
                            }

                            ymaps[ymap.RpfFileEntry.ShortNameHash] = ymap;
                        }
                    }

                    visiblemloentities.Clear();
                    foreach (var kvp in ymaps)
                    {
                        var ymap = kvp.Value;
                        if (ymap.AllEntities != null)
                        {
                            foreach (var ent in ymap.AllEntities)
                            {
                                if (ent.Archetype == null) continue;

                                Archetype parch = null;
                                projectarchetypes.TryGetValue(ent.Archetype.Hash, out parch);
                                if ((parch != null) && (ent.Archetype != parch))
                                {
                                    ent.SetArchetype(parch); //swap archetype to project one...
                                    if (ent.IsMlo)
                                    {
                                        ent.MloInstance.InitYmapEntityArchetypes(GameFileCache);
                                    }
                                }

                            }
                        }
                        if (ymap.MloEntities != null)
                        {
                            foreach (var mloDef in ymap.MloEntities)
                            {
                                if (mloDef.Archetype == null) continue; // archetype was changed from an mlo to a regular archetype
                                visiblemloentities[mloDef.Archetype._BaseArchetypeDef.name] = mloDef;
                            }
                        }
                    }
                }
            }
        }
        public void GetVisibleYbns(Camera camera, List<YbnFile> ybns, Dictionary<YmapEntityDef, YbnFile> interiors)
        {
            if (hidegtavmap)
            {
                ybns.Clear();
            }

            lock (projectsyncroot)
            {
                if (CurrentProjectFile == null) return;

                visibleybns.Clear();
                for (int i = 0; i < ybns.Count; i++)
                {
                    var ybn = ybns[i];
                    visibleybns[ybn.Name] = ybn;
                }

                for (int i = 0; i < CurrentProjectFile.YbnFiles.Count; i++)
                {
                    var ybn = CurrentProjectFile.YbnFiles[i];
                    if (ybn.Loaded)
                    {
                        if (!visiblemloentities.ContainsKey((ybn.RpfFileEntry != null) ? ybn.RpfFileEntry.ShortNameHash : 0))
                        {
                            visibleybns[ybn.Name] = ybn;
                        }
                    }
                }

                ybns.Clear();
                foreach (var ybn in visibleybns.Values)
                {
                    ybns.Add(ybn);
                }



                //messy way to gather the interior ybns!
                projectybns.Clear();
                for (int i = 0; i < CurrentProjectFile.YbnFiles.Count; i++)
                {
                    var ybn = CurrentProjectFile.YbnFiles[i];
                    if (ybn.Loaded)
                    {
                        projectybns[ybn.RpfFileEntry?.ShortNameHash ?? JenkHash.GenHash(ybn.Name)] = ybn;
                    }
                }
                interiorslist.Clear();
                interiorslist.AddRange(interiors.Keys);
                for (int i = 0; i < interiorslist.Count; i++)
                {
                    var mlo = interiorslist[i];
                    var hash = mlo._CEntityDef.archetypeName;
                    if (projectybns.TryGetValue(hash, out YbnFile ybn))
                    {
                        if ((ybn != null) && (ybn.Loaded))
                        {
                            interiors[mlo] = ybn;
                        }
                    }
                }


            }

        }
        public void GetVisibleYnds(Camera camera, List<YndFile> ynds)
        {
            if (hidegtavmap)
            {
                ynds.Clear();
            }

            lock (projectsyncroot)
            {
                if (CurrentProjectFile == null) return;

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

            lock (projectsyncroot)
            {
                if (CurrentProjectFile == null) return;

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


            lock (projectsyncroot)
            {
                if (CurrentProjectFile == null) return;

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


            lock (projectsyncroot)
            {
                if (CurrentProjectFile == null) return;

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
        public void GetVisibleAudioFiles(Camera camera, List<RelFile> rels)
        {
            if (hidegtavmap)
            {
                rels.Clear();
            }

            lock (projectsyncroot)
            {
                if (CurrentProjectFile == null) return;

                visibleaudiofiles.Clear();
                for (int i = 0; i < rels.Count; i++)
                {
                    var rel = rels[i];
                    visibleaudiofiles[rel.RpfFileEntry.NameHash] = rel;
                }

                for (int i = 0; i < CurrentProjectFile.AudioRelFiles.Count; i++)
                {
                    var rel = CurrentProjectFile.AudioRelFiles[i];
                    if (rel.Loaded)
                    {
                        visibleaudiofiles[rel.RpfFileEntry.NameHash] = rel;
                    }
                }

                rels.Clear();
                foreach (var rel in visibleaudiofiles.Values)
                {
                    rels.Add(rel);
                }
            }


        }
        public void GetVisibleWaterQuads(Camera camera, List<WaterQuad> quads)
        {
            if (hidegtavmap)
            {
                quads.Clear();
            }
        }

        public void GetMouseCollision(Camera camera, ref MapSelection curHit)
        {
            var mray = new Ray();
            mray.Position = camera.MouseRay.Position + camera.Position;
            mray.Direction = camera.MouseRay.Direction;

            var bounds = curHit.CollisionBounds ?? curHit.CollisionPoly?.Owner ?? curHit.CollisionVertex?.Owner;
            var curybn = bounds?.GetRootYbn();
            var eray = mray;

            if (hidegtavmap && (curybn != null))
            {
                curHit.Clear();
            }


            lock (projectsyncroot)
            {
                if (renderitems && (CurrentProjectFile != null))
                {
                    for (int i = 0; i < CurrentProjectFile.YbnFiles.Count; i++)
                    {
                        var ybn = CurrentProjectFile.YbnFiles[i];
                        if (ybn.Loaded)
                        {
                            if (ybn.Name == curybn?.Name)
                            {
                                curHit.Clear();
                            }

                            if (ybn.Bounds != null)
                            {
                                var hit = ybn.Bounds.RayIntersect(ref mray);
                                if (hit.Hit && (hit.HitDist < curHit.HitDist))
                                {
                                    curHit.UpdateCollisionFromRayHit(ref hit, camera);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < interiorslist.Count; i++)
                    {
                        var mlo = interiorslist[i];
                        var hash = mlo._CEntityDef.archetypeName;
                        if (projectybns.TryGetValue(hash, out YbnFile ybn))
                        {
                            if ((ybn != null) && (ybn.Loaded) && (ybn.Bounds != null))
                            {
                                var eorinv = Quaternion.Invert(mlo.Orientation);
                                eray.Position = eorinv.Multiply(mray.Position - mlo.Position);
                                eray.Direction = eorinv.Multiply(mray.Direction);
                                var hit = ybn.Bounds.RayIntersect(ref eray);
                                if (hit.Hit && (hit.HitDist < curHit.HitDist))
                                {
                                    hit.HitYbn = ybn;
                                    hit.HitEntity = mlo;
                                    hit.Position = mlo.Orientation.Multiply(hit.Position) + mlo.Position;
                                    hit.Normal = mlo.Orientation.Multiply(hit.Normal);
                                    curHit.UpdateCollisionFromRayHit(ref hit, camera);
                                }
                            }
                        }
                    }

                }
            }


        }

        public MloInstanceData TryGetMloInstance(MloArchetype arch)
        {
            lock (projectsyncroot)
            {
                if (arch == null) return null;
                MetaHash name = arch._BaseArchetypeDef.name;
                if (name == 0) return null;
                if (!visiblemloentities.ContainsKey(name)) return null;
                return visiblemloentities[name]?.MloInstance;
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
                    var wasmult = (CurrentMulti != null);
                    if ((sel.MultipleSelectionItems != null) && (sel.MultipleSelectionItems.Length > 0))
                    {
                        SetObject(ref sel.MultipleSelectionItems[sel.MultipleSelectionItems.Length - 1]);
                        SetObject(ref sel);
                        ProjectExplorer?.DeselectNode();
                        ShowProjectItemInProcess = true;
                        ShowCurrentProjectItem(false);
                        ShowProjectItemInProcess = false;
                        return;
                    }
                    else
                    {
                        CurrentMulti = null;
                    }

                    var mlo = sel.MloEntityDef;
                    var room = sel.MloRoomDef;
                    var ent = sel.EntityDef;
                    var cargen = sel.CarGenerator;
                    var lodlight = sel.LodLight;
                    var boxoccluder = sel.BoxOccluder;
                    var occludetri = sel.OccludeModelTri;
                    var grassbatch = sel.GrassBatch;
                    var collvert = sel.CollisionVertex;
                    var collpoly = sel.CollisionPoly;
                    var collbound = sel.CollisionBounds ?? collpoly?.Owner ?? collvert?.Owner;
                    var pathnode = sel.PathNode;
                    var pathlink = sel.PathLink;
                    var navpoly = sel.NavPoly;
                    var navpoint = sel.NavPoint;
                    var navportal = sel.NavPortal;
                    var trainnode = sel.TrainTrackNode;
                    var scenariond = sel.ScenarioNode;
                    var scenarioedge = sel.ScenarioEdge;
                    var audiopl = sel.Audio;
                    Archetype arch = mlo?.Archetype ?? ent?.MloParent?.Archetype ?? ent?.Archetype;
                    YtypFile ytyp = mlo?.Archetype?.Ytyp ?? ent?.MloParent?.Archetype?.Ytyp ?? ent?.Archetype?.Ytyp ?? room?.OwnerMlo?.Ytyp;
                    YmapFile ymap = ent?.Ymap ?? cargen?.Ymap ?? lodlight?.Ymap ?? boxoccluder?.Ymap ?? occludetri?.Ymap ?? grassbatch?.Ymap ?? mlo?.Ymap;
                    YbnFile ybn = collbound?.GetRootYbn();
                    YndFile ynd = pathnode?.Ynd;
                    YnvFile ynv = navpoly?.Ynv ?? navpoint?.Ynv ?? navportal?.Ynv;
                    TrainTrack traintrack = trainnode?.Track;
                    YmtFile scenario = scenariond?.Ymt ?? scenarioedge?.Region?.Ymt;
                    RelFile audiofile = audiopl?.RelFile;
                    bool showcurrent = false;

                    if (YmapExistsInProject(ymap) && (ybn == null))
                    {
                        if (wasmult || (ent != CurrentEntity))
                        {
                            ProjectExplorer?.TrySelectEntityTreeNode(ent);
                        }
                        if (wasmult || (cargen != CurrentCarGen))
                        {
                            ProjectExplorer?.TrySelectCarGenTreeNode(cargen);
                        }
                        if (wasmult || (lodlight != CurrentLodLight))
                        {
                            ProjectExplorer?.TrySelectLodLightTreeNode(lodlight);
                        }
                        if (wasmult || (boxoccluder != CurrentBoxOccluder))
                        {
                            ProjectExplorer?.TrySelectBoxOccluderTreeNode(boxoccluder);
                        }
                        if (wasmult || (occludetri != CurrentOccludeModelTri))
                        {
                            ProjectExplorer?.TrySelectOccludeModelTriangleTreeNode(occludetri);
                        }
                        if (wasmult || (grassbatch != CurrentGrassBatch))
                        {
                            ProjectExplorer?.TrySelectGrassBatchTreeNode(grassbatch);
                        }

                    }
                    else if (YtypExistsInProject(ytyp))
                    {
                        if (wasmult || (arch != CurrentArchetype))
                        {
                            ProjectExplorer?.TrySelectArchetypeTreeNode(mlo?.Archetype);
                        }
                        if (wasmult || (ent != CurrentEntity))
                        {
                            MloInstanceData mloInstance = ent.MloParent?.MloInstance;
                            if (mloInstance != null)
                            {
                                MCEntityDef entityDef = mloInstance.TryGetArchetypeEntity(ent);
                                ProjectExplorer?.TrySelectMloEntityTreeNode(entityDef);
                            }
                        }
                        if (wasmult || (room != CurrentMloRoom))
                        {
                            ProjectExplorer?.TrySelectMloRoomTreeNode(room);
                        }
                    }
                    else if (YbnExistsInProject(ybn))
                    {
                        if ((collvert != null) && (wasmult || (collvert != CurrentCollisionVertex)))
                        {
                            ProjectExplorer?.TrySelectCollisionVertexTreeNode(collvert);
                        }
                        else if ((collpoly != null) && (wasmult || (collpoly != CurrentCollisionPoly)))
                        {
                            ProjectExplorer?.TrySelectCollisionPolyTreeNode(collpoly);
                        }
                        else if (wasmult || (collbound != CurrentCollisionBounds))
                        {
                            ProjectExplorer?.TrySelectCollisionBoundsTreeNode(collbound);
                        }
                    }
                    else if (YndExistsInProject(ynd))
                    {
                        if (wasmult || (pathnode != CurrentPathNode))
                        {
                            ProjectExplorer?.TrySelectPathNodeTreeNode(pathnode);
                        }
                    }
                    else if (YnvExistsInProject(ynv))
                    {
                        if (wasmult || (navpoly != CurrentNavPoly))
                        {
                            ProjectExplorer?.TrySelectNavPolyTreeNode(navpoly);
                        }
                        if (wasmult || (navpoint != CurrentNavPoint))
                        {
                            ProjectExplorer?.TrySelectNavPointTreeNode(navpoint);
                        }
                        if (wasmult || (navportal != CurrentNavPortal))
                        {
                            ProjectExplorer?.TrySelectNavPortalTreeNode(navportal);
                        }
                    }
                    else if (TrainTrackExistsInProject(traintrack))
                    {
                        if (wasmult || (trainnode != CurrentTrainNode))
                        {
                            ProjectExplorer?.TrySelectTrainNodeTreeNode(trainnode);
                        }
                    }
                    else if (ScenarioExistsInProject(scenario))
                    {
                        if ((scenariond != null) && (wasmult || (scenariond != CurrentScenarioNode)))
                        {
                            ProjectExplorer?.TrySelectScenarioNodeTreeNode(scenariond);
                        }
                    }
                    else if (AudioFileExistsInProject(audiofile))
                    {
                        if ((audiopl?.AudioZone != null) && (wasmult || (audiopl != CurrentAudioZone)))
                        {
                            ProjectExplorer?.TrySelectAudioZoneTreeNode(audiopl);
                        }
                        if ((audiopl?.AudioEmitter != null) && (wasmult || (audiopl != CurrentAudioEmitter)))
                        {
                            ProjectExplorer?.TrySelectAudioEmitterTreeNode(audiopl);
                        }
                    }
                    else
                    {
                        ProjectExplorer?.DeselectNode();

                        showcurrent = true;
                    }

                    CurrentMloRoom = room;
                    CurrentMloPortal = null;
                    CurrentMloEntitySet = null;
                    CurrentYmapFile = ymap;
                    CurrentYtypFile = ytyp;
                    CurrentArchetype = arch;
                    CurrentEntity = ent ?? mlo;
                    CurrentCarGen = cargen;
                    CurrentLodLight = lodlight;
                    CurrentBoxOccluder = boxoccluder;
                    CurrentOccludeModelTri = occludetri;
                    CurrentOccludeModel = occludetri?.Model;
                    CurrentGrassBatch = grassbatch;
                    CurrentYbnFile = ybn;
                    CurrentCollisionVertex = collvert;
                    CurrentCollisionPoly = collpoly;
                    CurrentCollisionBounds = collbound;
                    CurrentYndFile = ynd;
                    CurrentPathNode = pathnode;
                    CurrentPathLink = pathlink;
                    CurrentYnvFile = ynv;
                    CurrentNavPoly = navpoly;
                    CurrentNavPoint = navpoint;
                    CurrentNavPortal = navportal;
                    CurrentTrainTrack = traintrack;
                    CurrentTrainNode = trainnode;
                    CurrentScenario = scenario;
                    CurrentScenarioNode = scenariond;
                    CurrentScenarioChainEdge = scenarioedge;
                    CurrentAudioFile = audiofile;
                    CurrentAudioZone = (audiopl?.AudioZone != null) ? audiopl : null;
                    CurrentAudioEmitter = (audiopl?.AudioEmitter != null) ? audiopl : null;
                    CurrentAudioZoneList = null;
                    CurrentAudioEmitterList = null;
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
        public void OnWorldSelectionModified(MapSelection sel)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { OnWorldSelectionModified(sel); }));
                }
                else
                {
                    if (sel.MultipleSelectionItems != null)
                    {
                        OnWorldMultiModified(sel.MultipleSelectionItems);
                    }
                    else if (sel.CollisionVertex != null)
                    {
                        OnWorldCollisionVertexModified(sel.CollisionVertex);
                    }
                    else if (sel.CollisionPoly != null)
                    {
                        OnWorldCollisionPolyModified(sel.CollisionPoly);
                    }
                    else if (sel.CollisionBounds != null)
                    {
                        OnWorldCollisionBoundsModified(sel.CollisionBounds);
                    }
                    else if (sel.EntityDef != null)
                    {
                        OnWorldEntityModified(sel.EntityDef);
                    }
                    else if (sel.CarGenerator != null)
                    {
                        OnWorldCarGenModified(sel.CarGenerator);
                    }
                    else if (sel.LodLight != null)
                    {
                        OnWorldLodLightModified(sel.LodLight);
                    }
                    else if (sel.BoxOccluder != null)
                    {
                        OnWorldBoxOccluderModified(sel.BoxOccluder);
                    }
                    else if (sel.OccludeModelTri != null)
                    {
                        OnWorldOccludeModelTriModified(sel.OccludeModelTri);
                    }
                    else if (sel.PathNode != null)
                    {
                        OnWorldPathNodeModified(sel.PathNode, sel.PathLink);
                    }
                    else if (sel.NavPoly != null)
                    {
                        OnWorldNavPolyModified(sel.NavPoly);
                    }
                    else if (sel.NavPoint != null)
                    {
                        OnWorldNavPointModified(sel.NavPoint);
                    }
                    else if (sel.NavPortal != null)
                    {
                        OnWorldNavPortalModified(sel.NavPortal);
                    }
                    else if (sel.TrainTrackNode != null)
                    {
                        OnWorldTrainNodeModified(sel.TrainTrackNode);
                    }
                    else if (sel.ScenarioNode != null)
                    {
                        OnWorldScenarioNodeModified(sel.ScenarioNode);
                    }
                    else if (sel.Audio != null)
                    {
                        OnWorldAudioPlacementModified(sel.Audio);
                    }
                }
            }
            catch { }
        }
        private void OnWorldMultiModified(MapSelection[] items)
        {

            if (items == CurrentMulti)
            {
                ShowEditMultiPanel(false);
            }

        }
        private void OnWorldEntityModified(YmapEntityDef ent)
        {
            if ((ent.Ymap == null) && (ent.MloParent == null))
            {
                return;//TODO: properly handle interior entities!
            }

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            if (ent.MloParent == null && ent.Ymap != null)
            {
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
            else if (ent.MloParent != null && ent.Ymap == null)
            {
                MloInstanceData mloInstance = ent.MloParent?.MloInstance;
                if (mloInstance != null)
                {
                    var mcEntity = mloInstance.TryGetArchetypeEntity(ent);
                    if (mcEntity != null)
                    {
                        if (!YtypExistsInProject(ent.MloParent.Archetype.Ytyp))
                        {
                            ent.MloParent.Archetype.Ytyp.HasChanged = true;
                            AddYtypToProject(ent.MloParent.Archetype.Ytyp);
                            ProjectExplorer?.TrySelectMloEntityTreeNode(mcEntity);
                        }

                        if (ent != CurrentEntity)
                        {
                            CurrentEntity = ent;
                            ProjectExplorer?.TrySelectMloEntityTreeNode(mcEntity);
                        }
                    }
                }

                if (ent == CurrentEntity)
                {
                    ShowEditYmapEntityPanel(false);

                    if (ent.MloParent.Archetype.Ytyp != null)
                    {
                        SetYtypHasChanged(true);
                    }
                }
            }
        }
        private void OnWorldCarGenModified(YmapCarGen cargen)
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
        private void OnWorldLodLightModified(YmapLODLight lodlight)
        {
            if (lodlight?.Ymap == null) return;

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            if (!YmapExistsInProject(lodlight.Ymap))
            {
                lodlight.Ymap.HasChanged = true;
                AddYmapToProject(lodlight.Ymap);
                ProjectExplorer?.TrySelectLodLightTreeNode(lodlight);
            }

            if (lodlight != CurrentLodLight)
            {
                CurrentLodLight = lodlight;
                ProjectExplorer?.TrySelectLodLightTreeNode(lodlight);
            }

            if (lodlight == CurrentLodLight)
            {
                ShowEditYmapLodLightPanel(false);

                ProjectExplorer?.UpdateLodLightTreeNode(lodlight);

                if (lodlight.Ymap != null)
                {
                    SetYmapHasChanged(true);
                }
            }

        }
        private void OnWorldBoxOccluderModified(YmapBoxOccluder box)
        {
            if (box?.Ymap == null) return;

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            if (!YmapExistsInProject(box.Ymap))
            {
                box.Ymap.HasChanged = true;
                AddYmapToProject(box.Ymap);
                ProjectExplorer?.TrySelectBoxOccluderTreeNode(box);
            }

            if (box != CurrentBoxOccluder)
            {
                CurrentBoxOccluder = box;
                ProjectExplorer?.TrySelectBoxOccluderTreeNode(box);
            }

            if (box == CurrentBoxOccluder)
            {
                ShowEditYmapBoxOccluderPanel(false);

                ProjectExplorer?.UpdateBoxOccluderTreeNode(box);

                if (box.Ymap != null)
                {
                    SetYmapHasChanged(true);
                }
            }

        }
        private void OnWorldOccludeModelTriModified(YmapOccludeModelTriangle tri)
        {
            if (tri?.Ymap == null) return;

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            if (!YmapExistsInProject(tri.Ymap))
            {
                tri.Ymap.HasChanged = true;
                AddYmapToProject(tri.Ymap);
                ProjectExplorer?.TrySelectOccludeModelTriangleTreeNode(tri);
            }

            if (tri != CurrentOccludeModelTri)
            {
                CurrentOccludeModelTri = tri;
                ProjectExplorer?.TrySelectOccludeModelTriangleTreeNode(tri);
            }

            if (tri == CurrentOccludeModelTri)
            {
                ShowEditYmapOccludeModelTrianglePanel(false);

                //ProjectExplorer?.UpdateOccludeModelTriangleTreeNode(tri);

                if (tri.Ymap != null)
                {
                    SetYmapHasChanged(true);
                }
            }

        }
        private void OnWorldCollisionVertexModified(BoundVertex vert)
        {
            var ybn = vert?.Owner?.GetRootYbn();
            if (ybn == null) return;

            CurrentYbnFile = ybn;

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            if (!YbnExistsInProject(ybn))
            {
                ybn.HasChanged = true;
                vert.Owner.HasChanged = true;
                AddYbnToProject(ybn);
                ProjectExplorer?.TrySelectCollisionVertexTreeNode(vert);
            }

            if (vert != CurrentCollisionVertex)
            {
                CurrentCollisionVertex = vert;
                ProjectExplorer?.TrySelectCollisionVertexTreeNode(vert);
            }

            if (vert == CurrentCollisionVertex)
            {
                ShowEditYbnBoundVertexPanel(false);

                //////UpdateCollisionVertexTreeNode(poly);

                if (ybn != null)
                {
                    SetYbnHasChanged(true);
                }
            }

        }
        private void OnWorldCollisionPolyModified(BoundPolygon poly)
        {
            var ybn = poly?.Owner?.GetRootYbn();
            if (ybn == null) return;

            CurrentYbnFile = ybn;

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            if (!YbnExistsInProject(ybn))
            {
                ybn.HasChanged = true;
                poly.Owner.HasChanged = true;
                AddYbnToProject(ybn);
                ProjectExplorer?.TrySelectCollisionPolyTreeNode(poly);
            }

            if (poly != CurrentCollisionPoly)
            {
                CurrentCollisionPoly = poly;
                ProjectExplorer?.TrySelectCollisionPolyTreeNode(poly);
            }

            if (poly == CurrentCollisionPoly)
            {
                ShowEditYbnBoundPolyPanel(false);

                //////UpdateCollisionPolyTreeNode(poly);

                if (ybn != null)
                {
                    SetYbnHasChanged(true);
                }
            }

        }
        private void OnWorldCollisionBoundsModified(Bounds bounds)
        {
            var ybn = bounds?.GetRootYbn();
            if (ybn == null) return;

            CurrentYbnFile = ybn;

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            if (!YbnExistsInProject(ybn))
            {
                ybn.HasChanged = true;
                bounds.HasChanged = true;
                AddYbnToProject(ybn);
                ProjectExplorer?.TrySelectCollisionBoundsTreeNode(bounds);
            }

            if (bounds != CurrentCollisionBounds)
            {
                CurrentCollisionBounds = bounds;
                ProjectExplorer?.TrySelectCollisionBoundsTreeNode(bounds);
            }

            if (bounds == CurrentCollisionBounds)
            {
                ShowEditYbnBoundsPanel(false);

                //////UpdateCollisionBoundsTreeNode(bounds);

                if (ybn != null)
                {
                    SetYbnHasChanged(true);
                }
            }

        }
        private void OnWorldPathNodeModified(YndNode node, YndLink link)
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
        private void OnWorldNavPolyModified(YnvPoly poly)
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
        private void OnWorldNavPointModified(YnvPoint point)
        {
            if (point?.Ynv == null) return;

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            if (!YnvExistsInProject(point.Ynv))
            {
                point.Ynv.HasChanged = true;
                AddYnvToProject(point.Ynv);
                ProjectExplorer?.TrySelectNavPointTreeNode(point);
            }

            if (point != CurrentNavPoint)
            {
                CurrentNavPoint = point;
                ProjectExplorer?.TrySelectNavPointTreeNode(point);
            }

            if (point == CurrentNavPoint)
            {
                ShowEditYnvPointPanel(false);

                //////UpdateNavPointTreeNode(poly);

                if (point.Ynv != null)
                {
                    SetYnvHasChanged(true);
                }
            }

        }
        private void OnWorldNavPortalModified(YnvPortal portal)
        {
            if (portal?.Ynv == null) return;

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            if (!YnvExistsInProject(portal.Ynv))
            {
                portal.Ynv.HasChanged = true;
                AddYnvToProject(portal.Ynv);
                ProjectExplorer?.TrySelectNavPortalTreeNode(portal);
            }

            if (portal != CurrentNavPortal)
            {
                CurrentNavPortal = portal;
                ProjectExplorer?.TrySelectNavPortalTreeNode(portal);
            }

            if (portal == CurrentNavPortal)
            {
                ShowEditYnvPortalPanel(false);

                //////UpdateNavPortalTreeNode(poly);

                if (portal.Ynv != null)
                {
                    SetYnvHasChanged(true);
                }
            }

        }
        private void OnWorldTrainNodeModified(TrainTrackNode node)
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
        private void OnWorldScenarioNodeModified(ScenarioNode node)
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
        private void OnWorldAudioPlacementModified(AudioPlacement audio)
        {
            if (audio?.RelFile == null) return;

            if (CurrentProjectFile == null)
            {
                NewProject();
            }

            if (!AudioFileExistsInProject(audio.RelFile))
            {
                audio.RelFile.HasChanged = true;
                AddAudioFileToProject(audio.RelFile);
                if (audio.AudioZone != null)
                {
                    ProjectExplorer?.TrySelectAudioZoneTreeNode(audio);
                }
                if (audio.AudioEmitter != null)
                {
                    ProjectExplorer?.TrySelectAudioEmitterTreeNode(audio);
                }
            }

            if ((audio.AudioZone != null) && (audio != CurrentAudioZone))
            {
                CurrentAudioZone = audio;
                ProjectExplorer?.TrySelectAudioZoneTreeNode(audio);
            }
            if ((audio.AudioEmitter != null) && (audio != CurrentAudioEmitter))
            {
                CurrentAudioEmitter = audio;
                ProjectExplorer?.TrySelectAudioEmitterTreeNode(audio);
            }
            if (audio == CurrentAudioZone)
            {
                ShowEditAudioZonePanel(false);
                if (audio.RelFile != null)
                {
                    SetAudioFileHasChanged(true);
                }
            }
            else if (audio == CurrentAudioEmitter)
            {
                ShowEditAudioEmitterPanel(false);
                if (audio.RelFile != null)
                {
                    SetAudioFileHasChanged(true);
                }
            }
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
        public void SetYbnHasChanged(bool changed)
        {
            if (CurrentYbnFile == null) return;

            bool changechange = changed != CurrentYbnFile.HasChanged;
            if (!changechange) return;

            CurrentYbnFile.HasChanged = changed;

            ProjectExplorer?.SetYbnHasChanged(CurrentYbnFile, changed);

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
        public void SetAudioFileHasChanged(bool changed)
        {
            if (CurrentAudioFile == null) return;

            bool changechange = changed != CurrentAudioFile.HasChanged;
            if (!changechange) return;

            CurrentAudioFile.HasChanged = changed;

            ProjectExplorer?.SetAudioRelHasChanged(CurrentAudioFile, changed);

            PromoteIfPreviewPanelActive();
        }
        public void SetGrassBatchHasChanged(bool changed)
        {
            if (CurrentGrassBatch == null) return;

            bool changechange = changed != CurrentGrassBatch.HasChanged;
            if (!changechange) return;

            CurrentGrassBatch.HasChanged = true;

            ProjectExplorer?.SetGrassBatchHasChanged(CurrentGrassBatch, changed);

            PromoteIfPreviewPanelActive();
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

        public Vector3 GetSpawnPosRel(float dist, Vector3 relPos, Quaternion relRot)
        {
            Vector3 pos = Vector3.Zero;
            if (WorldForm != null)
            {
                Vector3 campos = WorldForm.GetCameraPosition();
                Vector3 camdir = WorldForm.GetCameraViewDir();
                pos = campos + camdir * dist;

                Quaternion rot = Quaternion.Invert(relRot);
                Vector3 delta = pos - relPos;
                Vector3 relativePos = rot.Multiply(delta);
                pos = relativePos;
            }
            return pos;
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

            ymap.InitYmapEntityArchetypes(GameFileCache); //this needs to be done after calling YmapFile.Load()
        }
        private void LoadYtypFromFile(YtypFile ytyp, string filename)
        {
            byte[] data = File.ReadAllBytes(filename);

            ytyp.Load(data);

            AddProjectArchetypes(ytyp);
        }
        private void LoadYbnFromFile(YbnFile ybn, string filename)
        {
            byte[] data = File.ReadAllBytes(filename);

            ybn.Load(data);

            if (WorldForm != null)
            {
                if (ybn?.Bounds != null)
                {
                    WorldForm.UpdateCollisionBoundsGraphics(ybn?.Bounds);
                }
            }
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
        private void LoadYnvFromFile(YnvFile ynv, string filename)
        {
            byte[] data = File.ReadAllBytes(filename);

            ynv.Load(data);

            if (WorldForm != null)
            {
                WorldForm.UpdateNavYnvGraphics(ynv, true); //polys don't get drawn until something changes otherwise
            }
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

            ymt.Load(data);
        }
        private void LoadAudioRelFromFile(RelFile rel, string filename)
        {
            byte[] data = File.ReadAllBytes(filename);

            rel.Load(data, rel?.RpfFileEntry);
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
            RefreshYbnUI();
            RefreshYndUI();
            RefreshYnvUI();
            RefreshTrainTrackUI();
            RefreshScenarioUI();
            RefreshAudioUI();
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
            YmapNewGrassBatchMenu.Enabled = enable && inproj;

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
            bool ismlo = ((CurrentEntity != null) && (CurrentEntity.MloParent != null)) || (CurrentMloRoom != null) || (CurrentMloPortal != null) || (CurrentMloEntitySet != null) || (CurrentArchetype is MloArchetype);

            YtypNewArchetypeMenu.Enabled = enable && inproj;
            YtypNewArchetypeFromYdrMenu.Enabled = enable && inproj;
            YtypMloToolStripMenuItem.Enabled = enable && inproj && ismlo;
            YtypMloNewEntityToolStripMenuItem.Enabled = YtypMloToolStripMenuItem.Enabled;

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
        private void RefreshYbnUI()
        {
            bool enable = (CurrentYbnFile != null);
            bool inproj = YbnExistsInProject(CurrentYbnFile);

            YbnNewBoundsMenu.Enabled = enable && inproj;
            YbnNewPolygonMenu.Enabled = (CurrentCollisionBounds is BoundGeometry bgeom) && inproj;

            if (CurrentYbnFile != null)
            {
                YbnNameMenu.Text = "(" + CurrentYbnFile.Name + ")";
            }
            else
            {
                YbnNameMenu.Text = "(No .ybn file selected)";
            }

            YbnAddToProjectMenu.Enabled = enable && !inproj;
            YbnRemoveFromProjectMenu.Enabled = inproj;
            YbnMenu.Visible = enable;

            if (WorldForm != null)
            {
                WorldForm.EnableYbnUI(enable, CurrentYbnFile?.Name ?? "");
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
        private void RefreshAudioUI()
        {
            bool enable = (CurrentAudioFile != null);
            bool inproj = AudioFileExistsInProject(CurrentAudioFile);

            AudioNewAmbientEmitterMenu.Enabled = enable && inproj;
            AudioNewAmbientEmitterListMenu.Enabled = enable && inproj;
            AudioNewAmbientZoneMenu.Enabled = enable && inproj;
            AudioNewAmbientZoneListMenu.Enabled = enable && inproj;
            AudioNewInteriorMenu.Enabled = enable && inproj;
            AudioNewInteriorRoomMenu.Enabled = enable && inproj;

            if (CurrentAudioFile != null)
            {
                AudioNameMenu.Text = "(" + CurrentAudioFile.Name + ")";
            }
            else
            {
                AudioNameMenu.Text = "(No audio dat file selected)";
            }

            AudioAddToProjectMenu.Enabled = enable && !inproj;
            AudioRemoveFromProjectMenu.Enabled = inproj;
            AudioMenu.Visible = enable;

            if (WorldForm != null)
            {
                WorldForm.EnableAudioUI(enable, CurrentAudioFile?.Name ?? "");
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
            else if (CurrentYbnFile != null)
            {
                filename = CurrentYbnFile.RpfFileEntry?.Name;
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
            else if (CurrentAudioFile != null)
            {
                filename = CurrentAudioFile.RpfFileEntry?.Name;
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
            if (CurrentProjectFile != null)
            {
                var msg = "Are you sure you want to close the project window?";
                var tit = "Confirm close";
                if (e.CloseReason == CloseReason.FormOwnerClosing)
                {
                    msg = "Are you sure you want to quit CodeWalker?";
                    tit = "Confirm quit";
                }
                if (MessageBox.Show(msg, tit, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
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
        private void FileNewYbnMenu_Click(object sender, EventArgs e)
        {
            NewYbn();
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
        private void FileNewAudioDatMenu_Click(object sender, EventArgs e)
        {
            NewAudioFile();
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
        private void FileOpenYbnMenu_Click(object sender, EventArgs e)
        {
            OpenYbn();
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
        private void FileOpenAudioDatMenu_Click(object sender, EventArgs e)
        {
            OpenAudioFile();
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
        private void YmapNewGrassBatchMenu_Click(object sender, EventArgs e)
        {
            NewGrassBatch();
        }
        private void YmapAddToProjectMenu_Click(object sender, EventArgs e)
        {
            AddYmapToProject(CurrentYmapFile);
        }
        private void YmapRemoveFromProjectMenu_Click(object sender, EventArgs e)
        {
            RemoveYmapFromProject();
        }

        private void YtypAddToProjectMenu_Click(object sender, EventArgs e)
        {
            AddYtypToProject(CurrentYtypFile);
        }
        private void YtypRemoveFromProjectMenu_Click(object sender, EventArgs e)
        {
            RemoveYtypFromProject();
        }
        private void YtypNewArchetypeMenu_Click(object sender, EventArgs e)
        {
            NewArchetype();
        }
        private void YtypNewArchetypeFromYdrMenu_Click(object sender, EventArgs e)
        {
            NewArchetypeFromYdr();
        }
        private void YtypMloNewEntityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewMloEntity();
        }
        private void YtypMloNewRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewMloRoom();
        }
        private void YtypMloNewPortalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewMloPortal();
        }
        private void YtypMloNewEntitySetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewMloEntitySet();
        }

        private void YbnNewBoundBoxMenu_Click(object sender, EventArgs e)
        {
            NewCollisionBounds(BoundsType.Box);
        }
        private void YbnNewBoundSphereMenu_Click(object sender, EventArgs e)
        {
            NewCollisionBounds(BoundsType.Sphere);
        }
        private void YbnNewBoundCapsuleMenu_Click(object sender, EventArgs e)
        {
            NewCollisionBounds(BoundsType.Capsule);
        }
        private void YbnNewBoundCylinderMenu_Click(object sender, EventArgs e)
        {
            NewCollisionBounds(BoundsType.Cylinder);
        }
        private void YbnNewBoundDiscMenu_Click(object sender, EventArgs e)
        {
            NewCollisionBounds(BoundsType.Disc);
        }
        private void YbnNewBoundClothMenu_Click(object sender, EventArgs e)
        {
            NewCollisionBounds(BoundsType.Cloth);
        }
        private void YbnNewBoundGeometryMenu_Click(object sender, EventArgs e)
        {
            NewCollisionBounds(BoundsType.Geometry);
        }
        private void YbnNewBoundGeometryBVHMenu_Click(object sender, EventArgs e)
        {
            NewCollisionBounds(BoundsType.GeometryBVH);
        }
        private void YbnNewBoundCompositeMenu_Click(object sender, EventArgs e)
        {
            NewCollisionBounds(BoundsType.Composite);
        }
        private void YbnNewPolygonTriangleMenu_Click(object sender, EventArgs e)
        {
            NewCollisionPoly(BoundPolygonType.Triangle);
        }
        private void YbnNewPolygonSphereMenu_Click(object sender, EventArgs e)
        {
            NewCollisionPoly(BoundPolygonType.Sphere);
        }
        private void YbnNewPolygonCapsuleMenu_Click(object sender, EventArgs e)
        {
            NewCollisionPoly(BoundPolygonType.Capsule);
        }
        private void YbnNewPolygonBoxMenu_Click(object sender, EventArgs e)
        {
            NewCollisionPoly(BoundPolygonType.Box);
        }
        private void YbnNewPolygonCylinderMenu_Click(object sender, EventArgs e)
        {
            NewCollisionPoly(BoundPolygonType.Cylinder);
        }
        private void YbnAddToProjectMenu_Click(object sender, EventArgs e)
        {
            AddYbnToProject(CurrentYbnFile);
        }
        private void YbnRemoveFromProjectMenu_Click(object sender, EventArgs e)
        {
            RemoveYbnFromProject();
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

        private void AudioNewAmbientEmitterMenu_Click(object sender, EventArgs e)
        {
            NewAudioEmitter();
        }
        private void AudioNewAmbientEmitterListMenu_Click(object sender, EventArgs e)
        {
            NewAudioEmitterList();
        }
        private void AudioNewAmbientZoneMenu_Click(object sender, EventArgs e)
        {
            NewAudioZone();
        }
        private void AudioNewAmbientZoneListMenu_Click(object sender, EventArgs e)
        {
            NewAudioZoneList();
        }
        private void AudioNewInteriorMenu_Click(object sender, EventArgs e)
        {
            NewAudioInterior();
        }
        private void AudioNewInteriorRoomMenu_Click(object sender, EventArgs e)
        {
            NewAudioInteriorRoom();
        }
        private void AudioAddToProjectMenu_Click(object sender, EventArgs e)
        {
            AddAudioFileToProject(CurrentAudioFile);
        }
        private void AudioRemoveFromProjectMenu_Click(object sender, EventArgs e)
        {
            RemoveAudioFileFromProject();
        }

        private void ToolsManifestGeneratorMenu_Click(object sender, EventArgs e)
        {
            ShowEditProjectManifestPanel(false);
        }
        private void ToolsLODLightsGeneratorMenu_Click(object sender, EventArgs e)
        {
            ShowGenerateLODLightsPanel(false);
        }
        private void ToolsNavMeshGeneratorMenu_Click(object sender, EventArgs e)
        {
            ShowGenerateNavMeshPanel(false);
        }
        private void ToolsImportMenyooXmlMenu_Click(object sender, EventArgs e)
        {
            ImportMenyooXml();
        }

        private void OptionsRenderGtavMapMenu_Click(object sender, EventArgs e)
        {
            OptionsRenderGtavMapMenu.Checked = !OptionsRenderGtavMapMenu.Checked;
            hidegtavmap = !OptionsRenderGtavMapMenu.Checked;
        }
        private void OptionsRenderProjectItemsMenu_Click(object sender, EventArgs e)
        {
            OptionsRenderProjectItemsMenu.Checked = !OptionsRenderProjectItemsMenu.Checked;
            renderitems = OptionsRenderProjectItemsMenu.Checked;
        }
        private void OptionsAutoCalcYmapFlagsMenu_Click(object sender, EventArgs e)
        {
            OptionsAutoCalcYmapFlagsMenu.Checked = !OptionsAutoCalcYmapFlagsMenu.Checked;
            autoymapflags = OptionsAutoCalcYmapFlagsMenu.Checked;
        }
        private void OptionsAutoCalcYmapExtentsMenu_Click(object sender, EventArgs e)
        {
            OptionsAutoCalcYmapExtentsMenu.Checked = !OptionsAutoCalcYmapExtentsMenu.Checked;
            autoymapextents = OptionsAutoCalcYmapExtentsMenu.Checked;
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
        private void ToolbarNewYbnMenu_Click(object sender, EventArgs e)
        {
            NewYbn();
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
        private void ToolbarOpenYbnMenu_Click(object sender, EventArgs e)
        {
            OpenYbn();
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
