using CodeWalker.GameFiles;
using CodeWalker.Project;
using CodeWalker.Properties;
using CodeWalker.World;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker
{
    public partial class ProjectFormOLD : Form
    {
        WorldForm WorldForm;
        GameFileCache GameFileCache;
        RpfManager RpfMan;

        ProjectFile CurrentProjectFile;
        YmapFile CurrentYmapFile;
        YmapEntityDef CurrentEntity;
        YmapCarGen CurrentCarGen;

        YndFile CurrentYndFile;
        YndNode CurrentPathNode;
        YndLink CurrentPathLink;

        YnvFile CurrentYnvFile;
        YnvPoly CurrentNavPoly;

        TrainTrack CurrentTrainTrack;
        TrainTrackNode CurrentTrainNode;

        YmtFile CurrentScenario;
        ScenarioNode CurrentScenarioNode;
        MCScenarioChainingEdge CurrentScenarioChainEdge;


        bool renderentities = true;
        bool hidegtavmap = false;

        object ymapsyncroot = new object();
        object yndsyncroot = new object();
        object ynvsyncroot = new object();
        object trainsyncroot = new object();
        object scenariosyncroot = new object();

        Dictionary<int, YndFile> visibleynds = new Dictionary<int, YndFile>();
        Dictionary<int, YnvFile> visibleynvs = new Dictionary<int, YnvFile>();
        Dictionary<string, TrainTrack> visibletrains = new Dictionary<string, TrainTrack>();
        Dictionary<string, YmtFile> visiblescenarios = new Dictionary<string, YmtFile>();


        bool populatingui = false;



        public bool IsProjectLoaded
        {
            get { return CurrentProjectFile != null; }
        }


        public ProjectFormOLD(WorldForm worldForm)
        {
            InitializeComponent();

            MainTabControl.TabPages.Remove(YnvTabPage);//TODO
            EntityTabControl.TabPages.Remove(EntityExtensionsTabPage);//TODO

            LoadDropDowns();

            WorldForm = worldForm;
            if ((WorldForm != null) && (WorldForm.GameFileCache != null))
            {
                GameFileCache = WorldForm.GameFileCache;
                RpfMan = GameFileCache.RpfMan;
            }
            else
            {
                GameFileCache = GameFileCacheFactory.Create();
                new Thread(new ThreadStart(() => {
                    GameFileCache.Init(UpdateStatus, UpdateStatus);
                    RpfMan = GameFileCache.RpfMan;
                })).Start();
            }

        }


        private void LoadDropDowns()
        {
            EntityLodLevelComboBox.Items.Clear();
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_ORPHANHD);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_HD);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_LOD);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_SLOD1);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_SLOD2);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_SLOD3);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_SLOD4);

            EntityPriorityLevelComboBox.Items.Clear();
            EntityPriorityLevelComboBox.Items.Add(Unk_648413703.PRI_REQUIRED);
            EntityPriorityLevelComboBox.Items.Add(Unk_648413703.PRI_OPTIONAL_HIGH);
            EntityPriorityLevelComboBox.Items.Add(Unk_648413703.PRI_OPTIONAL_MEDIUM);
            EntityPriorityLevelComboBox.Items.Add(Unk_648413703.PRI_OPTIONAL_LOW);
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

            if (WorldForm != null)
            {
                WorldForm.EnableYmapUI(enable, CurrentYmapFile?.Name ?? "");
            }
        }

        private void RefreshEntityUI()
        {
            bool enable = (CurrentEntity != null);
            bool isinproj = false;

            if (CurrentEntity != null)
            {
                isinproj = YmapExistsInProject(CurrentEntity.Ymap);
            }

            EntityAddToProjectButton.Enabled = !isinproj;
            EntityDeleteButton.Enabled = isinproj;
        }

        private void RefreshCarGenUI()
        {
            bool enable = (CurrentCarGen != null);
            bool isinproj = false;

            if (CurrentCarGen != null)
            {
                isinproj = YmapExistsInProject(CurrentCarGen.Ymap);
            }

            CarAddToProjectButton.Enabled = !isinproj;
            CarDeleteButton.Enabled = isinproj;
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


            if (WorldForm != null)
            {
                WorldForm.EnableScenarioUI(enable, CurrentScenario?.Name ?? "");
            }
        }

        private void RefreshUI()
        {
            RefreshYmapUI();
            RefreshEntityUI();
            RefreshCarGenUI();
            RefreshYndUI();
            RefreshYnvUI();
            RefreshTrainTrackUI();
            RefreshScenarioUI();
            SetCurrentSaveItem();
            LoadYmapTabPage();
            LoadEntityTabPage();
            LoadCarGenTabPage();
            LoadYndTabPage();
            LoadYnvTabPage();
            LoadPathNodeTabPage();
            LoadTrainTrackTabPage();
            LoadTrainNodeTabPage();
            LoadScenarioTabPage();
            LoadScenarioNodeTabPages();
        }


        private void SetCurrentSaveItem()
        {
            string filename = null;
            if (CurrentYmapFile != null)
            {
                filename = CurrentYmapFile.RpfFileEntry?.Name;
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
            }
            else
            {
                FileSaveItemMenu.Text = "Save";
                FileSaveItemAsMenu.Text = "Save As...";
            }

            FileSaveItemMenu.Tag = filename;
            FileSaveItemAsMenu.Tag = filename;

            FileSaveItemMenu.Enabled = enable;
            FileSaveItemMenu.Visible = enable;
            FileSaveItemAsMenu.Enabled = enable;
            FileSaveItemAsMenu.Visible = enable;

            if (WorldForm != null)
            {
                WorldForm.SetCurrentSaveItem(filename);
            }
        }



        public void Save()
        {
            if (CurrentYmapFile != null)
            {
                SaveYmap();
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
                    LoadYmapTabPage();
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
                    LoadYndTabPage();
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
                    LoadYnvTabPage();
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
                    LoadTrainTrackTabPage();
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
                    LoadScenarioTabPage();
                }


                SaveProject();
            }
        }


        private void SaveCurrentItem(bool saveas = false)
        {
            if (CurrentYmapFile != null)
            {
                SaveYmap(saveas);
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

        private void CloseProject()
        {
            if (CurrentProjectFile == null) return;

            foreach (var ymap in CurrentProjectFile.YmapFiles)
            {
                if ((ymap != null) && (ymap.HasChanged))
                {
                    //save the current ymap first?
                    if (MessageBox.Show("Would you like to save "+ymap.Name+" before closing?", "Save .ymap before closing?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CurrentYmapFile = ymap;
                        SaveYmap();
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

            CurrentProjectFile = null;
            CurrentYmapFile = null;
            CurrentYndFile = null;

            LoadProjectUI();


            if (WorldForm != null)
            {
                WorldForm.SelectItem(null);//make sure current selected item isn't still selected...
            }
        }

        private void LoadProjectUI()
        {
            RefreshProjectUI();
            UpdateFormTitleText();
            LoadProjectTree();
            LoadProjectTabPage();
            RefreshUI();
        }


        private void LoadProjectTree()
        {
            ProjectTreeView.Nodes.Clear();

            if (CurrentProjectFile == null) return;

            var pcstr = CurrentProjectFile.HasChanged ? "*" : "";

            var projnode = ProjectTreeView.Nodes.Add(pcstr + CurrentProjectFile.Name);
            projnode.Tag = CurrentProjectFile;


            if (CurrentProjectFile.YmapFiles.Count > 0)
            {
                var ymapsnode = projnode.Nodes.Add("Ymap Files");
                ymapsnode.Name = "Ymap";

                foreach (var ymapfile in CurrentProjectFile.YmapFiles)
                {
                    var ycstr = ymapfile.HasChanged ? "*" : "";
                    string name = ymapfile.Name;
                    if (ymapfile.RpfFileEntry != null)
                    {
                        name = ymapfile.RpfFileEntry.Name;
                    }
                    var ymapnode = ymapsnode.Nodes.Add(ycstr + name);
                    ymapnode.Tag = ymapfile;

                    LoadYmapTreeNodes(ymapfile, ymapnode);

                    JenkIndex.Ensure(name);
                    JenkIndex.Ensure(Path.GetFileNameWithoutExtension(name));
                }
                ymapsnode.Expand();
            }

            if (CurrentProjectFile.YndFiles.Count > 0)
            {
                var yndsnode = projnode.Nodes.Add("Ynd Files");
                yndsnode.Name = "Ynd";

                foreach (var yndfile in CurrentProjectFile.YndFiles)
                {
                    var ycstr = yndfile.HasChanged ? "*" : "";
                    string name = yndfile.Name;
                    if (yndfile.RpfFileEntry != null)
                    {
                        name = yndfile.RpfFileEntry.Name;
                    }
                    var yndnode = yndsnode.Nodes.Add(ycstr + name);
                    yndnode.Tag = yndfile;

                    LoadYndTreeNodes(yndfile, yndnode);
                }
                yndsnode.Expand();
            }

            if (CurrentProjectFile.YnvFiles.Count > 0)
            {
                var ynvsnode = projnode.Nodes.Add("Ynv Files");
                ynvsnode.Name = "Ynv";

                foreach (var ynvfile in CurrentProjectFile.YnvFiles)
                {
                    var ycstr = ynvfile.HasChanged ? "*" : "";
                    string name = ynvfile.Name;
                    if (ynvfile.RpfFileEntry != null)
                    {
                        name = ynvfile.RpfFileEntry.Name;
                    }
                    var ynvnode = ynvsnode.Nodes.Add(ycstr + name);
                    ynvnode.Tag = ynvfile;

                    LoadYnvTreeNodes(ynvfile, ynvnode);
                }
                ynvsnode.Expand();
            }

            if (CurrentProjectFile.TrainsFiles.Count > 0)
            {
                var trainsnode = projnode.Nodes.Add("Trains Files");
                trainsnode.Name = "Trains";

                foreach (var trainfile in CurrentProjectFile.TrainsFiles)
                {
                    var tcstr = trainfile.HasChanged ? "*" : "";
                    string name = trainfile.Name;
                    if (trainfile.RpfFileEntry != null)
                    {
                        name = trainfile.RpfFileEntry.Name;
                    }
                    var trainnode = trainsnode.Nodes.Add(tcstr + name);
                    trainnode.Tag = trainfile;

                    LoadTrainTrackTreeNodes(trainfile, trainnode);
                }
                trainsnode.Expand();
            }

            if (CurrentProjectFile.ScenarioFiles.Count > 0)
            {
                var scenariosnode = projnode.Nodes.Add("Scenario Files");
                scenariosnode.Name = "Scenarios";

                foreach (var scenariofile in CurrentProjectFile.ScenarioFiles)
                {
                    var scstr = scenariofile.HasChanged ? "*" : "";
                    string name = scenariofile.Name;
                    if (scenariofile.RpfFileEntry != null)
                    {
                        name = scenariofile.RpfFileEntry.Name;
                    }
                    var scenarionode = scenariosnode.Nodes.Add(scstr + name);
                    scenarionode.Tag = scenariofile;

                    LoadScenarioTreeNodes(scenariofile, scenarionode);
                }
                scenariosnode.Expand();
            }

            projnode.Expand();

        }

        private void LoadProjectTabPage()
        {
            if (CurrentProjectFile == null)
            {
                ProjectPanel.Enabled = false;
                ProjectNameTextBox.Text = "<No project loaded>";
                ProjectVersionLabel.Text = "Version: -";
            }
            else
            {
                ProjectPanel.Enabled = true;
                ProjectNameTextBox.Text = CurrentProjectFile.Name;
                ProjectVersionLabel.Text = "Version: " + CurrentProjectFile.Version.ToString();
            }
        }

        private void SetProjectHasChanged(bool changed)
        {
            if (CurrentProjectFile == null) return;

            CurrentProjectFile.HasChanged = changed;

            if (ProjectTreeView.Nodes.Count > 0)
            {
                //first node is the project...
                string changestr = changed ? "*" : "";
                ProjectTreeView.Nodes[0].Text = changestr + CurrentProjectFile.Name;
            }

            UpdateFormTitleText();
        }


        private void GenerateProjectManifest()
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, YtypFile> deps = new Dictionary<string, YtypFile>();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
            sb.AppendLine("<CPackFileMetaData>");
            sb.AppendLine("  <MapDataGroups/>");
            sb.AppendLine("  <HDTxdBindingArray/>");
            sb.AppendLine("  <imapDependencies/>");


            if ((CurrentProjectFile != null) && (CurrentProjectFile.YmapFiles.Count > 0))
            {
                sb.AppendLine("  <imapDependencies_2>");
                foreach (var ymap in CurrentProjectFile.YmapFiles)
                {
                    var ymapname = ymap.RpfFileEntry?.NameLower;
                    if (string.IsNullOrEmpty(ymapname))
                    {
                        ymapname = ymap.Name.ToLowerInvariant();
                    }
                    if (ymapname.EndsWith(".ymap"))
                    {
                        ymapname = ymapname.Substring(0, ymapname.Length - 5);
                    }

                    deps.Clear();
                    if (ymap.AllEntities != null)
                    {
                        foreach (var ent in ymap.AllEntities)
                        {
                            var ytyp = ent.Archetype?.Ytyp;
                            if (ytyp != null)
                            {
                                var ytypname = ytyp.RpfFileEntry?.NameLower;
                                if (string.IsNullOrEmpty(ytypname))
                                {
                                    ytypname = ytyp.RpfFileEntry?.Name?.ToLowerInvariant();
                                    if (ytypname == null) ytypname = "";
                                }
                                if (ytypname.EndsWith(".ytyp"))
                                {
                                    ytypname = ytypname.Substring(0, ytypname.Length - 5);
                                }
                                deps[ytypname] = ytyp;
                            }
                        }
                    }

                    sb.AppendLine("    <Item>");
                    sb.AppendLine("      <imapName>" + ymapname + "</imapName>");
                    sb.AppendLine("      <manifestFlags/>");
                    sb.AppendLine("      <itypDepArray>");
                    foreach (var kvp in deps)
                    {
                        sb.AppendLine("        <Item>" + kvp.Key + "</Item>");
                    }
                    sb.AppendLine("      </itypDepArray>");
                    sb.AppendLine("    </Item>");
                }
                sb.AppendLine("  </imapDependencies_2>");
            }
            else
            {
                sb.AppendLine("  <imapDependencies_2/>");
            }

            sb.AppendLine("  <itypDependencies_2/>");
            sb.AppendLine("  <Interiors/>");
            sb.AppendLine("</CPackFileMetaData>");

            ProjectManifestTextBox.Text = sb.ToString();
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

            lock (ymapsyncroot)
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
            lock (ymapsyncroot) //need to sync writes to ymap objects...
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
                LoadYmapTabPage();
                if (CurrentProjectFile != null)
                {
                    //string newname = CurrentYmapFile.Name;
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

        private void AddYmapToProject(YmapFile ymap)
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
                TrySelectEntityTreeNode(CurrentEntity);
            }
            else if (CurrentCarGen != null)
            {
                TrySelectCarGenTreeNode(CurrentCarGen);
            }
        }

        private void RemoveYmapFromProject()
        {
            if (CurrentYmapFile == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveYmapFile(CurrentYmapFile);
            CurrentYmapFile = null;
            LoadProjectTree();
            RefreshUI();
        }

        private void LoadYmapTabPage()
        {
            if (CurrentYmapFile == null)
            {
                YmapPanel.Enabled = false;
                YmapNameTextBox.Text = "<No ymap selected>";
                YmapNameHashLabel.Text = "Hash: 0";
                YmapParentTextBox.Text = string.Empty;
                YmapParentHashLabel.Text = "Hash: 0";
                YmapFlagsTextBox.Text = string.Empty;
                YmapContentFlagsTextBox.Text = string.Empty;
                YmapCFlagsHDCheckBox.Checked = false;
                YmapCFlagsLODCheckBox.Checked = false;
                YmapCFlagsSLOD2CheckBox.Checked = false;
                YmapCFlagsInteriorCheckBox.Checked = false;
                YmapCFlagsSLODCheckBox.Checked = false;
                YmapCFlagsOcclusionCheckBox.Checked = false;
                YmapCFlagsPhysicsCheckBox.Checked = false;
                YmapCFlagsLODLightsCheckBox.Checked = false;
                YmapCFlagsDistLightsCheckBox.Checked = false;
                YmapCFlagsCriticalCheckBox.Checked = false;
                YmapCFlagsGrassCheckBox.Checked = false;
                YmapFlagsScriptedCheckBox.Checked = false;
                YmapFlagsLODCheckBox.Checked = false;
                YmapPhysicsDictionariesTextBox.Text = string.Empty;
                YmapEntitiesExtentsMinTextBox.Text = string.Empty;
                YmapEntitiesExtentsMaxTextBox.Text = string.Empty;
                YmapStreamingExtentsMinTextBox.Text = string.Empty;
                YmapStreamingExtentsMaxTextBox.Text = string.Empty;
                YmapFileLocationTextBox.Text = string.Empty;
                YmapProjectPathTextBox.Text = string.Empty;
            }
            else
            {
                populatingui = true;
                var md = CurrentYmapFile.CMapData;
                if (md.name.Hash == 0)
                {
                    string name = Path.GetFileNameWithoutExtension(CurrentYmapFile.Name);
                    JenkIndex.Ensure(name);
                    md.name = new MetaHash(JenkHash.GenHash(name));
                }

                YmapPanel.Enabled = true;
                YmapNameTextBox.Text = md.name.ToString();
                YmapNameHashLabel.Text = "Hash: " + md.name.Hash.ToString();
                YmapParentTextBox.Text = md.parent.ToString();
                YmapParentHashLabel.Text = "Hash: " + md.parent.Hash.ToString();
                YmapEntitiesExtentsMinTextBox.Text = FloatUtil.GetVector3String(md.entitiesExtentsMin);
                YmapEntitiesExtentsMaxTextBox.Text = FloatUtil.GetVector3String(md.entitiesExtentsMax);
                YmapStreamingExtentsMinTextBox.Text = FloatUtil.GetVector3String(md.streamingExtentsMin);
                YmapStreamingExtentsMaxTextBox.Text = FloatUtil.GetVector3String(md.streamingExtentsMax);
                YmapFileLocationTextBox.Text = CurrentYmapFile.FilePath;
                YmapProjectPathTextBox.Text = (CurrentProjectFile != null) ? CurrentProjectFile.GetRelativePath(CurrentYmapFile.FilePath) : CurrentYmapFile.FilePath;

                UpdateYmapFlagsUI(true, true);

                UpdateYmapPhysicsDictionariesUI();

                populatingui = false;

                ////struct CMapData:
                //MetaHash name { get; set; } //8   8: Hash: 0: name
                //MetaHash parent { get; set; } //12   12: Hash: 0: parent
                //uint flags { get; set; } //16   16: UnsignedInt: 0: flags
                //uint contentFlags { get; set; } //20   20: UnsignedInt: 0: contentFlags//1785155637
                //Vector3 streamingExtentsMin { get; set; } //32   32: Float_XYZ: 0: streamingExtentsMin//3710026271
                //Vector3 streamingExtentsMax { get; set; } //48   48: Float_XYZ: 0: streamingExtentsMax//2720965429
                //Vector3 entitiesExtentsMin { get; set; } //64   64: Float_XYZ: 0: entitiesExtentsMin//477478129
                //Vector3 entitiesExtentsMax { get; set; } //80   80: Float_XYZ: 0: entitiesExtentsMax//1829192759
                //Array_StructurePointer entities { get; set; } //96   96: Array: 0: entities  {0: StructurePointer: 0: 256}
                //Array_Structure containerLods { get; set; } //112   112: Array: 0: containerLods//2935983381  {0: Structure: 372253349: 256}
                //Array_Structure boxOccluders { get; set; } //128   128: Array: 0: boxOccluders//3983590932  {0: Structure: SectionUNKNOWN7: 256}
                //Array_Structure occludeModels { get; set; } //144   144: Array: 0: occludeModels//2132383965  {0: Structure: SectionUNKNOWN5: 256}
                //Array_uint physicsDictionaries { get; set; } //160   160: Array: 0: physicsDictionaries//949589348  {0: Hash: 0: 256}
                //rage__fwInstancedMapData instancedData { get; set; } //176   176: Structure: rage__fwInstancedMapData: instancedData//2569067561
                //Array_Structure timeCycleModifiers { get; set; } //224   224: Array: 0: timeCycleModifiers  {0: Structure: CTimeCycleModifier: 256}
                //Array_Structure carGenerators { get; set; } //240   240: Array: 0: carGenerators//3254823756  {0: Structure: CCarGen: 256}
                //CLODLight LODLightsSOA { get; set; } //256   256: Structure: CLODLight: LODLightsSOA//1774371066
                //CDistantLODLight DistantLODLightsSOA { get; set; } //392   392: Structure: CDistantLODLight: DistantLODLightsSOA//2954466641
                //CBlockDesc block { get; set; } //440   440: Structure: CBlockDesc//3072355914: block

            }
        }

        private void LoadYmapFromFile(YmapFile ymap, string filename)
        {
            byte[] data = File.ReadAllBytes(filename);

            ymap.Load(data);

            GameFileCache.InitYmapEntityArchetypes(ymap); //this needs to be done after calling YmapFile.Load()
        }

        private void LoadYmapTreeNodes(YmapFile ymap, TreeNode node)
        {
            if (ymap == null) return;

            if (!string.IsNullOrEmpty(node.Name)) return; //named nodes are eg Entities and CarGens

            node.Nodes.Clear();

            if ((ymap.AllEntities != null)&& (ymap.AllEntities.Length > 0))
            {
                var entsnode = node.Nodes.Add("Entities (" + ymap.AllEntities.Length.ToString() + ")");
                entsnode.Name = "Entities";
                entsnode.Tag = ymap;
                var ents = ymap.AllEntities;
                for (int i = 0; i < ents.Length; i++)
                {
                    var ent = ents[i];
                    var edef = ent.CEntityDef;
                    var enode = entsnode.Nodes.Add(edef.archetypeName.ToString());
                    enode.Tag = ent;
                }
            }
            if ((ymap.CarGenerators != null) && (ymap.CarGenerators.Length > 0))
            {
                var cargensnode = node.Nodes.Add("Car Generators (" + ymap.CarGenerators.Length.ToString() + ")");
                cargensnode.Name = "CarGens";
                cargensnode.Tag = ymap;
                var cargens = ymap.CarGenerators;
                for (int i = 0; i < cargens.Length; i++)
                {
                    var cargen = cargens[i];
                    var ccgnode = cargensnode.Nodes.Add(cargen.ToString());
                    ccgnode.Tag = cargen;
                }
            }
            if ((ymap.GrassInstanceBatches != null) && (ymap.GrassInstanceBatches.Length > 0))
            {
                var grassbatchesnodes = node.Nodes.Add("Grass Batches (" + ymap.GrassInstanceBatches.Length.ToString() + ")");
                grassbatchesnodes.Name = "GrassBatches";
                grassbatchesnodes.Tag = ymap;
                var grassbatches = ymap.GrassInstanceBatches;
                for (int i = 0; i < grassbatches.Length; i++)
                {
                    var batch = grassbatches[i];
                    var gbnode = grassbatchesnodes.Nodes.Add(batch.ToString());
                    gbnode.Tag = batch;
                }
            }

        }

        private void SetYmapHasChanged(bool changed)
        {
            if (CurrentYmapFile == null) return;

            bool changechange = changed != CurrentYmapFile.HasChanged;
            if (!changechange) return;

            CurrentYmapFile.HasChanged = changed;

            SetYmapHasChangedUI(changed);
        }

        private void SetYmapHasChangedUI(bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var pnode = ProjectTreeView.Nodes[0];
                var ymnode = GetChildTreeNode(pnode, "Ymap");
                if (ymnode == null) return;
                string changestr = changed ? "*" : "";
                for (int i = 0; i < ymnode.Nodes.Count; i++)
                {
                    var ynode = ymnode.Nodes[i];
                    if (ynode.Tag == CurrentYmapFile)
                    {
                        string name = CurrentYmapFile.Name;
                        if (CurrentYmapFile.RpfFileEntry != null)
                        {
                            name = CurrentYmapFile.RpfFileEntry.Name;
                        }
                        ynode.Text = changestr + name;
                        break;
                    }
                }
            }
        }

        private void UpdateYmapFlagsUI(bool updateCheckboxes, bool updateTextboxes)
        {
            if (CurrentYmapFile == null) return;

            var md = CurrentYmapFile.CMapData;
            var flags = md.flags;
            var contentFlags = md.contentFlags;

            if (updateCheckboxes)
            {
                YmapCFlagsHDCheckBox.Checked = IsBitSet(contentFlags, 0); //1
                YmapCFlagsLODCheckBox.Checked = IsBitSet(contentFlags, 1); //2
                YmapCFlagsSLOD2CheckBox.Checked = IsBitSet(contentFlags, 2); //4
                YmapCFlagsInteriorCheckBox.Checked = IsBitSet(contentFlags, 3); //8
                YmapCFlagsSLODCheckBox.Checked = IsBitSet(contentFlags, 4); //16
                YmapCFlagsOcclusionCheckBox.Checked = IsBitSet(contentFlags, 5); //32
                YmapCFlagsPhysicsCheckBox.Checked = IsBitSet(contentFlags, 6); //64
                YmapCFlagsLODLightsCheckBox.Checked = IsBitSet(contentFlags, 7); //128
                YmapCFlagsDistLightsCheckBox.Checked = IsBitSet(contentFlags, 8); //256
                YmapCFlagsCriticalCheckBox.Checked = IsBitSet(contentFlags, 9); //512
                YmapCFlagsGrassCheckBox.Checked = IsBitSet(contentFlags, 10); //1024

                YmapFlagsScriptedCheckBox.Checked = IsBitSet(flags, 0); //1
                YmapFlagsLODCheckBox.Checked = IsBitSet(flags, 1); //2
            }
            if (updateTextboxes)
            {
                YmapFlagsTextBox.Text = flags.ToString();
                YmapContentFlagsTextBox.Text = contentFlags.ToString();
            }
        }

        private void SetYmapFlagsFromCheckBoxes()
        {
            if (populatingui) return;
            if (CurrentYmapFile == null) return;

            uint flags = 0;
            uint contentFlags = 0;

            contentFlags = UpdateBit(contentFlags, 0, YmapCFlagsHDCheckBox.Checked);
            contentFlags = UpdateBit(contentFlags, 1, YmapCFlagsLODCheckBox.Checked);
            contentFlags = UpdateBit(contentFlags, 2, YmapCFlagsSLOD2CheckBox.Checked);
            contentFlags = UpdateBit(contentFlags, 3, YmapCFlagsInteriorCheckBox.Checked);
            contentFlags = UpdateBit(contentFlags, 4, YmapCFlagsSLODCheckBox.Checked);
            contentFlags = UpdateBit(contentFlags, 5, YmapCFlagsOcclusionCheckBox.Checked);
            contentFlags = UpdateBit(contentFlags, 6, YmapCFlagsPhysicsCheckBox.Checked);
            contentFlags = UpdateBit(contentFlags, 7, YmapCFlagsLODLightsCheckBox.Checked);
            contentFlags = UpdateBit(contentFlags, 8, YmapCFlagsDistLightsCheckBox.Checked);
            contentFlags = UpdateBit(contentFlags, 9, YmapCFlagsCriticalCheckBox.Checked);
            contentFlags = UpdateBit(contentFlags, 10, YmapCFlagsGrassCheckBox.Checked);

            flags = UpdateBit(flags, 0, YmapFlagsScriptedCheckBox.Checked);
            flags = UpdateBit(flags, 1, YmapFlagsLODCheckBox.Checked);


            lock (ymapsyncroot)
            {
                if (CurrentYmapFile._CMapData.flags != flags)
                {
                    CurrentYmapFile._CMapData.flags = flags;
                    SetYmapHasChanged(true);
                }
                if (CurrentYmapFile._CMapData.contentFlags != contentFlags)
                {
                    CurrentYmapFile._CMapData.contentFlags = contentFlags;
                    SetYmapHasChanged(true);
                }
            }

            populatingui = true;
            UpdateYmapFlagsUI(false, true); //update textbox
            populatingui = false;
        }

        private void SetYmapFlagsFromTextBoxes()
        {
            if (populatingui) return;
            if (CurrentYmapFile == null) return;

            uint flags = 0;
            uint contentFlags = 0;
            uint.TryParse(YmapFlagsTextBox.Text, out flags);
            uint.TryParse(YmapContentFlagsTextBox.Text, out contentFlags);
            lock (ymapsyncroot)
            {
                if (CurrentYmapFile._CMapData.flags != flags)
                {
                    CurrentYmapFile._CMapData.flags = flags;
                    SetYmapHasChanged(true);
                }
                if (CurrentYmapFile._CMapData.contentFlags != contentFlags)
                {
                    CurrentYmapFile._CMapData.contentFlags = contentFlags;
                    SetYmapHasChanged(true);
                }
            }

            populatingui = true;
            UpdateYmapFlagsUI(true, false); //update checkboxes
            populatingui = false;
        }

        private void CalcYmapFlags()
        {
            if (populatingui) return;
            if (CurrentYmapFile == null) return;

            lock (ymapsyncroot)
            {
                if (CurrentYmapFile.CalcFlags())
                {
                    SetYmapHasChanged(true);
                }
            }

            populatingui = true;
            UpdateYmapFlagsUI(true, true); //update checkboxes and textboxes
            populatingui = false;
        }

        private void CalcYmapExtents()
        {
            if (CurrentYmapFile == null) return;

            var allents = CurrentYmapFile.AllEntities;
            var allbatches = CurrentYmapFile.GrassInstanceBatches;

            if ((allents == null) && (allbatches == null))
            {
                MessageBox.Show("No items to calculate extents from.");
                return;
            }

            lock (ymapsyncroot)
            {
                if (CurrentYmapFile.CalcExtents())
                {
                    SetYmapHasChanged(true);
                }
            }

            populatingui = true;
            var md = CurrentYmapFile.CMapData;
            YmapEntitiesExtentsMinTextBox.Text = FloatUtil.GetVector3String(md.entitiesExtentsMin);
            YmapEntitiesExtentsMaxTextBox.Text = FloatUtil.GetVector3String(md.entitiesExtentsMax);
            YmapStreamingExtentsMinTextBox.Text = FloatUtil.GetVector3String(md.streamingExtentsMin);
            YmapStreamingExtentsMaxTextBox.Text = FloatUtil.GetVector3String(md.streamingExtentsMax);
            populatingui = false;
        }

        private void UpdateYmapPhysicsDictionariesUI()
        {
            if ((CurrentYmapFile == null) || (CurrentYmapFile.physicsDictionaries == null))
            {
                YmapPhysicsDictionariesTextBox.Text = string.Empty;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var hash in CurrentYmapFile.physicsDictionaries)
                {
                    sb.AppendLine(hash.ToString());
                }
                YmapPhysicsDictionariesTextBox.Text = sb.ToString();
            }
        }

        private void SetYmapPhysicsDictionariesFromTextbox()
        {
            if (populatingui) return;
            if (CurrentYmapFile == null) return;

            List<MetaHash> hashes = new List<MetaHash>();

            var strs = YmapPhysicsDictionariesTextBox.Text.Split('\n');
            foreach (var str in strs)
            {
                var tstr = str.Trim();
                if (!string.IsNullOrEmpty(tstr))
                {
                    uint h = 0;
                    if (uint.TryParse(tstr, out h))
                    {
                        hashes.Add(h);
                    }
                    else
                    {
                        h = JenkHash.GenHash(tstr.ToLowerInvariant());
                        hashes.Add(h);
                    }
                }
            }

            lock (ymapsyncroot)
            {
                CurrentYmapFile.physicsDictionaries = (hashes.Count > 0) ? hashes.ToArray() : null;
                SetYmapHasChanged(true);
            }
        }


        private Vector3 GetSpawnPos(float dist)
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

            TrySelectEntityTreeNode(ent);
            CurrentEntity = ent;
            LoadEntityTabPage();
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

            //find this now to remove it later.
            var tn = FindEntityTreeNode(CurrentEntity);

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

            SetYmapHasChangedUI(true);

            CurrentEntity = null;
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Entities (" + CurrentYmapFile.AllEntities.Length.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
            else
            {
                //no need for this when removing the node above will select something else.
                LoadEntityTabPage(); //this case really shouldn't happen... 
            }

            return true;
        }

        private void AddEntityToProject()
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
                TrySelectEntityTreeNode(ent);
            }
        }

        private void LoadEntityTabPage()
        {
            if (CurrentEntity == null)
            {
                EntityPanel.Enabled = false;
                EntityArchetypeTextBox.Text = string.Empty;
                EntityArchetypeHashLabel.Text = "Hash: 0";
                EntityFlagsTextBox.Text = string.Empty;
                EntityGuidTextBox.Text = string.Empty;
                EntityPositionTextBox.Text = string.Empty;
                EntityRotationTextBox.Text = string.Empty;
                EntityScaleXYTextBox.Text = string.Empty;
                EntityScaleZTextBox.Text = string.Empty;
                EntityParentIndexTextBox.Text = string.Empty;
                EntityLodDistTextBox.Text = string.Empty;
                EntityChildLodDistTextBox.Text = string.Empty;
                EntityLodLevelComboBox.SelectedIndex = 0;// Math.Max(EntityLodLevelComboBox.FindString(), 0);
                EntityNumChildrenTextBox.Text = string.Empty;
                EntityPriorityLevelComboBox.SelectedIndex = 0; //Math.Max(..
                EntityAOMultiplierTextBox.Text = string.Empty;
                EntityArtificialAOTextBox.Text = string.Empty;
                EntityTintValueTextBox.Text = string.Empty;
                EntityPivotEditCheckBox.Checked = false;
                EntityPivotPositionTextBox.Text = string.Empty;
                EntityPivotRotationTextBox.Text = string.Empty;
                foreach (int i in EntityFlagsCheckedListBox.CheckedIndices)
                {
                    EntityFlagsCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            else
            {
                populatingui = true;
                var e = CurrentEntity.CEntityDef;
                var po = CurrentEntity.PivotOrientation;
                EntityPanel.Enabled = true;
                EntityArchetypeTextBox.Text = e.archetypeName.ToString();
                EntityArchetypeHashLabel.Text = "Hash: " + e.archetypeName.Hash.ToString();
                EntityFlagsTextBox.Text = e.flags.ToString();
                EntityGuidTextBox.Text = e.guid.ToString();
                EntityPositionTextBox.Text = FloatUtil.GetVector3String(e.position);
                EntityRotationTextBox.Text = FloatUtil.GetVector4String(e.rotation);
                EntityScaleXYTextBox.Text = FloatUtil.ToString(e.scaleXY);
                EntityScaleZTextBox.Text = FloatUtil.ToString(e.scaleZ);
                EntityParentIndexTextBox.Text = e.parentIndex.ToString();
                EntityLodDistTextBox.Text = FloatUtil.ToString(e.lodDist);
                EntityChildLodDistTextBox.Text = FloatUtil.ToString(e.childLodDist);
                EntityLodLevelComboBox.SelectedIndex = Math.Max(EntityLodLevelComboBox.FindString(e.lodLevel.ToString()), 0);
                EntityNumChildrenTextBox.Text = e.numChildren.ToString();
                EntityPriorityLevelComboBox.SelectedIndex = Math.Max(EntityPriorityLevelComboBox.FindString(e.priorityLevel.ToString()), 0);
                EntityAOMultiplierTextBox.Text = e.ambientOcclusionMultiplier.ToString();
                EntityArtificialAOTextBox.Text = e.artificialAmbientOcclusion.ToString();
                EntityTintValueTextBox.Text = e.tintValue.ToString();
                EntityPivotPositionTextBox.Text = FloatUtil.GetVector3String(CurrentEntity.PivotPosition);
                EntityPivotRotationTextBox.Text = FloatUtil.GetVector4String(new Vector4(po.X, po.Y, po.Z, po.W));
                for (int i = 0; i < EntityFlagsCheckedListBox.Items.Count; i++)
                {
                    var cv = ((e.flags & (1u << i)) > 0);
                    EntityFlagsCheckedListBox.SetItemCheckState(i, cv ? CheckState.Checked : CheckState.Unchecked);
                }
                populatingui = false;



                if (WorldForm != null)
                {
                    WorldForm.SelectEntity(CurrentEntity); //hopefully the drawable is already loaded - this will try get from cache
                }

                ////struct CEntityDef:
                //MetaHash archetypeName { get; set; } //8   8: Hash: 0: archetypeName
                //uint flags { get; set; } //12   12: UnsignedInt: 0: flags
                //uint guid { get; set; } //16   16: UnsignedInt: 0: guid
                //Vector3 position { get; set; } //32   32: Float_XYZ: 0: position
                //Vector4 rotation { get; set; } //48   48: Float_XYZW: 0: rotation
                //float scaleXY { get; set; } //64   64: Float: 0: 2627937847
                //float scaleZ { get; set; } //68   68: Float: 0: 284916802
                //int parentIndex { get; set; } //72   72: SignedInt: 0: parentIndex
                //float lodDist { get; set; } //76   76: Float: 0: lodDist
                //float childLodDist { get; set; } //80   80: Float: 0: childLodDist//3398912973
                //Unk_1264241711 lodLevel { get; set; } //84   84: IntEnum: 1264241711: lodLevel  //LODTYPES_DEPTH_
                //uint numChildren { get; set; } //88   88: UnsignedInt: 0: numChildren//2793909385
                //Unk_648413703 priorityLevel { get; set; } //92   92: IntEnum: 648413703: priorityLevel//647098393
                //Array_StructurePointer extensions { get; set; } //96   96: Array: 0: extensions  {0: StructurePointer: 0: 256}
                //int ambientOcclusionMultiplier { get; set; } //112   112: SignedInt: 0: ambientOcclusionMultiplier//415356295
                //int artificialAmbientOcclusion { get; set; } //116   116: SignedInt: 0: artificialAmbientOcclusion//599844163
                //uint tintValue { get; set; } //120   120: UnsignedInt: 0: tintValue//1015358759
            }
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

            TrySelectCarGenTreeNode(cg);
            CurrentCarGen = cg;
            LoadCarGenTabPage();
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

            //find this now to remove it later.
            var tn = FindCarGenTreeNode(CurrentCarGen);

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

            SetYmapHasChangedUI(true);

            CurrentCarGen = null;
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Car Generators (" + CurrentYmapFile.CarGenerators.Length.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
            else
            {
                //no need for this when removing the node above will select something else.
                LoadCarGenTabPage(); //this case really shouldn't happen... 
            }

            return true;
        }

        private void AddCarGenToProject()
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
                TrySelectCarGenTreeNode(cargen);
            }
        }

        private void LoadCarGenTabPage()
        {

            if (CurrentCarGen == null)
            {
                CarGenPanel.Enabled = false;
                CarModelTextBox.Text = string.Empty;
                CarModelHashLabel.Text = "Hash: 0";
                CarPopGroupTextBox.Text = string.Empty;
                CarPopGroupHashLabel.Text = "Hash: 0";
                CarFlagsTextBox.Text = string.Empty;
                CarPositionTextBox.Text = string.Empty;
                CarOrientXTextBox.Text = string.Empty;
                CarOrientYTextBox.Text = string.Empty;
                CarPerpendicularLengthTextBox.Text = string.Empty;
                CarBodyColorRemap1TextBox.Text = string.Empty;
                CarBodyColorRemap2TextBox.Text = string.Empty;
                CarBodyColorRemap3TextBox.Text = string.Empty;
                CarBodyColorRemap4TextBox.Text = string.Empty;
                CarLiveryTextBox.Text = string.Empty;
                foreach (int i in CarFlagsCheckedListBox.CheckedIndices)
                {
                    CarFlagsCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            else
            {
                populatingui = true;
                var c = CurrentCarGen.CCarGen;
                CarGenPanel.Enabled = true;
                CarModelTextBox.Text = c.carModel.ToString();
                CarModelHashLabel.Text = "Hash: " + c.carModel.Hash.ToString();
                CarPopGroupTextBox.Text = c.popGroup.ToString();
                CarPopGroupHashLabel.Text = "Hash: " + c.popGroup.Hash.ToString();
                CarFlagsTextBox.Text = c.flags.ToString();
                CarPositionTextBox.Text = FloatUtil.GetVector3String(c.position);
                CarOrientXTextBox.Text = FloatUtil.ToString(c.orientX);
                CarOrientYTextBox.Text = FloatUtil.ToString(c.orientY);
                CarPerpendicularLengthTextBox.Text = FloatUtil.ToString(c.perpendicularLength);
                CarBodyColorRemap1TextBox.Text = c.bodyColorRemap1.ToString();
                CarBodyColorRemap2TextBox.Text = c.bodyColorRemap2.ToString();
                CarBodyColorRemap3TextBox.Text = c.bodyColorRemap3.ToString();
                CarBodyColorRemap4TextBox.Text = c.bodyColorRemap4.ToString();
                CarLiveryTextBox.Text = c.livery.ToString();
                for (int i = 0; i < CarFlagsCheckedListBox.Items.Count; i++)
                {
                    var cv = ((c.flags & (1u << i)) > 0);
                    CarFlagsCheckedListBox.SetItemCheckState(i, cv ? CheckState.Checked : CheckState.Unchecked);
                }
                populatingui = false;

                if (WorldForm != null)
                {
                    WorldForm.SelectCarGen(CurrentCarGen);
                }

                ////struct CCarGen:
                //Vector3 position { get; set; } //16   16: Float_XYZ: 0: position
                //float orientX { get; set; } //32   32: Float: 0: orientX=735213009
                //float orientY { get; set; } //36   36: Float: 0: orientY=979440342
                //float perpendicularLength { get; set; } //40   40: Float: 0: perpendicularLength=124715667
                //MetaHash carModel { get; set; } //44   44: Hash: 0: carModel
                //uint flags { get; set; } //48   48: UnsignedInt: 0: flags
                //int bodyColorRemap1 { get; set; } //52   52: SignedInt: 0: bodyColorRemap1=1429703670
                //int bodyColorRemap2 { get; set; } //56   56: SignedInt: 0: bodyColorRemap2=1254848286
                //int bodyColorRemap3 { get; set; } //60   60: SignedInt: 0: bodyColorRemap3=1880965569
                //int bodyColorRemap4 { get; set; } //64   64: SignedInt: 0: bodyColorRemap4=1719152247
                //MetaHash popGroup { get; set; } //68   68: Hash: 0: popGroup=911358791
                //sbyte livery { get; set; } //72   72: SignedByte: 0: livery
            }
        }

        public bool IsCurrentCarGen(YmapCarGen cargen)
        {
            return CurrentCarGen == cargen;
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

            lock (yndsyncroot)
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
            lock (yndsyncroot) //need to sync writes to ynd objects...
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
                LoadYndTabPage();
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

        private void AddYndToProject(YndFile ynd)
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
                TrySelectPathNodeTreeNode(CurrentPathNode);
            }
        }

        private void RemoveYndFromProject()
        {
            if (CurrentYndFile == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveYndFile(CurrentYndFile);
            CurrentYndFile = null;
            LoadProjectTree();
            RefreshUI();
        }

        private void LoadYndTabPage()
        {
            if (CurrentYndFile == null)
            {
                YndPanel.Enabled = false;
                YndRpfPathTextBox.Text = string.Empty;
                YndFilePathTextBox.Text = string.Empty;
                YndProjectPathTextBox.Text = string.Empty;
                YndAreaIDXUpDown.Value = 0;
                YndAreaIDYUpDown.Value = 0;
                YndAreaIDInfoLabel.Text = "ID: 0";
                YndTotalNodesLabel.Text = "Total Nodes: 0";
                YndVehicleNodesUpDown.Value = 0;
                YndVehicleNodesUpDown.Maximum = 0;
                YndPedNodesUpDown.Value = 0;
                YndPedNodesUpDown.Maximum = 0;
            }
            else
            {
                populatingui = true;
                var nd = CurrentYndFile.NodeDictionary;
                YndPanel.Enabled = true;
                YndRpfPathTextBox.Text = CurrentYndFile.RpfFileEntry.Path;
                YndFilePathTextBox.Text = CurrentYndFile.FilePath;
                YndProjectPathTextBox.Text = (CurrentProjectFile != null) ? CurrentProjectFile.GetRelativePath(CurrentYndFile.FilePath) : CurrentYndFile.FilePath;
                YndAreaIDXUpDown.Value = CurrentYndFile.CellX;
                YndAreaIDYUpDown.Value = CurrentYndFile.CellY;
                YndAreaIDInfoLabel.Text = "ID: " + CurrentYndFile.AreaID.ToString();
                YndTotalNodesLabel.Text = "Total Nodes: " + (nd?.NodesCount.ToString()??"0");
                YndVehicleNodesUpDown.Maximum = nd?.NodesCount??0;
                YndVehicleNodesUpDown.Value = Math.Min(nd?.NodesCountVehicle??0, YndVehicleNodesUpDown.Maximum);
                YndPedNodesUpDown.Maximum = nd?.NodesCount??0;
                YndPedNodesUpDown.Value = Math.Min(nd?.NodesCountPed??0, YndPedNodesUpDown.Maximum);
                populatingui = false;
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

        private void LoadYndTreeNodes(YndFile ynd, TreeNode node)
        {
            if (ynd == null) return;

            if (!string.IsNullOrEmpty(node.Name)) return; //named nodes are eg Nodes

            node.Nodes.Clear();



            if ((ynd.Nodes != null) && (ynd.Nodes.Length > 0))
            {
                var nodesnode = node.Nodes.Add("Nodes (" + ynd.Nodes.Length.ToString() + ")");
                nodesnode.Name = "Nodes";
                nodesnode.Tag = ynd;
                var nodes = ynd.Nodes;
                for (int i = 0; i < nodes.Length; i++)
                {
                    var ynode = nodes[i];
                    var nnode = ynode.RawData;
                    var tnode = nodesnode.Nodes.Add(nnode.ToString());
                    tnode.Tag = ynode;
                }
            }

        }

        private void SetYndHasChanged(bool changed)
        {
            if (CurrentYndFile == null) return;

            bool changechange = changed != CurrentYndFile.HasChanged;
            if (!changechange) return;

            CurrentYndFile.HasChanged = changed;

            SetYndHasChangedUI(changed);
        }

        private void SetYndHasChangedUI(bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var pnode = ProjectTreeView.Nodes[0];
                var ynnode = GetChildTreeNode(pnode, "Ynd");
                if (ynnode == null) return;
                string changestr = changed ? "*" : "";
                for (int i = 0; i < ynnode.Nodes.Count; i++)
                {
                    var ynode = ynnode.Nodes[i];
                    if (ynode.Tag == CurrentYndFile)
                    {
                        string name = CurrentYndFile.Name;
                        if (CurrentYndFile.RpfFileEntry != null)
                        {
                            name = CurrentYndFile.RpfFileEntry.Name;
                        }
                        ynode.Text = changestr + name;
                        break;
                    }
                }
            }
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

            TrySelectPathNodeTreeNode(n);
            CurrentPathNode = n;
            LoadYndTabPage();
            LoadPathNodeTabPage();


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

            //find this now to remove it later.
            var tn = FindPathNodeTreeNode(CurrentPathNode);

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

            SetYndHasChangedUI(true);

            CurrentPathNode = null;
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Nodes (" + CurrentYndFile.Nodes.Length.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
            else
            {
                //no need for this when removing the node above will select something else.
                LoadPathNodeTabPage(); //this case really shouldn't happen... 
            }

            if (WorldForm != null)
            {
                WorldForm.UpdatePathYndGraphics(CurrentYndFile, false);
            }


            return true;
        }

        private void AddPathNodeToProject()
        {
            if (CurrentPathNode == null) return;
            if (CurrentYndFile != null)
            {
                AddYndToProject(CurrentYndFile);
            }
        }

        private void LoadPathNodeTabPage()
        {

            CurrentPathLink = null;

            if (CurrentPathNode == null)
            {
                YndNodePanel.Enabled = false;
                PathNodeDeleteButton.Enabled = false;
                PathNodeAddToProjectButton.Enabled = false;
                PathNodeAreaIDUpDown.Value = 0;
                PathNodeNodeIDUpDown.Value = 0;
                PathNodePositionTextBox.Text = string.Empty;
                PathNodeStreetHashTextBox.Text = string.Empty;
                PathNodeStreetNameLabel.Text = "Name: [None]";

                UpdatePathNodeFlagsUI(true, true);

                PathNodeLinkCountLabel.Text = "Link Count: 0";
                PathNodeLinksListBox.Items.Clear();

            }
            else
            {
                populatingui = true;
                var n = CurrentPathNode.RawData;
                YndNodePanel.Enabled = true;
                PathNodeDeleteButton.Enabled = YndExistsInProject(CurrentYndFile);
                PathNodeAddToProjectButton.Enabled = !PathNodeDeleteButton.Enabled;
                var streetname = GlobalText.TryGetString(n.StreetName.Hash);
                PathNodeAreaIDUpDown.Value = n.AreaID;
                PathNodeNodeIDUpDown.Value = n.NodeID;
                PathNodePositionTextBox.Text = FloatUtil.GetVector3String(CurrentPathNode.Position);
                PathNodeStreetHashTextBox.Text = n.StreetName.Hash.ToString();
                PathNodeStreetNameLabel.Text = "Name: " + ((n.StreetName.Hash == 0) ? "[None]" : (string.IsNullOrEmpty(streetname) ? "[Not found]" : streetname));

                UpdatePathNodeFlagsUI(true, true);

                PathNodeLinkCountLabel.Text = "Link Count: " + CurrentPathNode.LinkCount.ToString();
                PathNodeLinksListBox.Items.Clear();
                if (CurrentPathNode.Links != null)
                {
                    foreach (var link in CurrentPathNode.Links)
                    {
                        PathNodeLinksListBox.Items.Add(link);
                    }
                }
                populatingui = false;


                if (WorldForm != null)
                {
                    WorldForm.SelectPathNode(CurrentPathNode);
                }

            }


            LoadPathNodeJunctionPage();

            LoadPathNodeLinkPage();
        }

        private void LoadPathNodeLinkPage()
        {
            if (CurrentPathLink == null)
            {
                PathNodeLinkPanel.Enabled = false;
                PathNodeLinkAreaIDUpDown.Value = 0;
                PathNodeLinkNodeIDUpDown.Value = 0;

                UpdatePathNodeLinkFlagsUI(true, true);

                PathNodeLinkLengthUpDown.Value = 0;
                PathNodeLinkageStatusLabel.Text = "";
            }
            else
            {
                populatingui = true;
                PathNodeLinkPanel.Enabled = true;
                PathNodeLinkAreaIDUpDown.Value = CurrentPathLink._RawData.AreaID;
                PathNodeLinkNodeIDUpDown.Value = CurrentPathLink._RawData.NodeID;

                UpdatePathNodeLinkFlagsUI(true, true);

                PathNodeLinkLengthUpDown.Value = CurrentPathLink.LinkLength.Value;
                PathNodeLinkageStatusLabel.Text = "";
                populatingui = false;

                if (WorldForm != null)
                {
                    WorldForm.SelectPathLink(CurrentPathLink);
                }
            }

        }

        private void LoadPathNodeJunctionPage()
        {

            var junc = CurrentPathNode?.Junction;
            if (junc == null)
            {
                PathNodeJunctionEnableCheckBox.Checked = false;
                PathNodeJunctionPanel.Enabled = false;
                PathNodeJunctionMaxZUpDown.Value = 0;
                PathNodeJunctionMinZUpDown.Value = 0;
                PathNodeJunctionPosXUpDown.Value = 0;
                PathNodeJunctionPosYUpDown.Value = 0;
                PathNodeJunctionHeightmapDimXUpDown.Value = 1;
                PathNodeJunctionHeightmapDimYUpDown.Value = 1;
                PathNodeJunctionHeightmapBytesTextBox.Text = string.Empty;
            }
            else
            {
                populatingui = true;
                PathNodeJunctionEnableCheckBox.Checked = CurrentPathNode.HasJunction;
                PathNodeJunctionPanel.Enabled = PathNodeJunctionEnableCheckBox.Checked;
                PathNodeJunctionMaxZUpDown.Value = junc.MaxZ;
                PathNodeJunctionMinZUpDown.Value = junc.MinZ;
                PathNodeJunctionPosXUpDown.Value = junc.PositionX;
                PathNodeJunctionPosYUpDown.Value = junc.PositionY;
                PathNodeJunctionHeightmapDimXUpDown.Value = junc.Heightmap.CountX;
                PathNodeJunctionHeightmapDimYUpDown.Value = junc.Heightmap.CountY;
                PathNodeJunctionHeightmapBytesTextBox.Text = junc.Heightmap?.GetDataString() ?? "";
                populatingui = false;
            }


        }

        public bool IsCurrentPathNode(YndNode pathnode)
        {
            return CurrentPathNode == pathnode;
        }


        private void UpdatePathNodeFlagsUI(bool updateCheckboxes, bool updateUpDowns)
        {

            var flags0 = CurrentPathNode?.Flags0.Value ?? 0;
            var flags1 = CurrentPathNode?.Flags1.Value ?? 0;
            var flags2 = CurrentPathNode?.Flags2.Value ?? 0;
            var flags3 = CurrentPathNode?.Flags3.Value ?? 0;
            var flags4 = CurrentPathNode?.Flags4.Value ?? 0;
            var flags5 = (uint)(CurrentPathNode?.LinkCountUnk ?? 0);


            if (updateCheckboxes)
            {
                PathNodeFlags01CheckBox.Checked = IsBitSet(flags0, 0);
                PathNodeFlags02CheckBox.Checked = IsBitSet(flags0, 1);
                PathNodeFlags03CheckBox.Checked = IsBitSet(flags0, 2);
                PathNodeFlags04CheckBox.Checked = IsBitSet(flags0, 3);
                PathNodeFlags05CheckBox.Checked = IsBitSet(flags0, 4);
                PathNodeFlags06CheckBox.Checked = IsBitSet(flags0, 5);
                PathNodeFlags07CheckBox.Checked = IsBitSet(flags0, 6);
                PathNodeFlags08CheckBox.Checked = IsBitSet(flags0, 7);

                PathNodeFlags11CheckBox.Checked = IsBitSet(flags1, 0);
                PathNodeFlags12CheckBox.Checked = IsBitSet(flags1, 1);
                PathNodeFlags13CheckBox.Checked = IsBitSet(flags1, 2);
                PathNodeFlags14CheckBox.Checked = IsBitSet(flags1, 3);
                PathNodeFlags15CheckBox.Checked = IsBitSet(flags1, 4);
                PathNodeFlags16CheckBox.Checked = IsBitSet(flags1, 5);
                PathNodeFlags17CheckBox.Checked = IsBitSet(flags1, 6);
                PathNodeFlags18CheckBox.Checked = IsBitSet(flags1, 7);

                PathNodeFlags21CheckBox.Checked = IsBitSet(flags2, 0);
                PathNodeFlags22CheckBox.Checked = IsBitSet(flags2, 1);
                PathNodeFlags23CheckBox.Checked = IsBitSet(flags2, 2);
                PathNodeFlags24CheckBox.Checked = IsBitSet(flags2, 3);
                PathNodeFlags25CheckBox.Checked = IsBitSet(flags2, 4);
                PathNodeFlags26CheckBox.Checked = IsBitSet(flags2, 5);
                PathNodeFlags27CheckBox.Checked = IsBitSet(flags2, 6);
                PathNodeFlags28CheckBox.Checked = IsBitSet(flags2, 7);

                PathNodeFlags31CheckBox.Checked = IsBitSet(flags3, 0);
                PathNodeFlags32UpDown.Value = (flags3 >> 1) & 127;

                PathNodeFlags41CheckBox.Checked = IsBitSet(flags4, 0);
                PathNodeFlags42UpDown.Value = (flags4 >> 1) & 7;
                PathNodeFlags45CheckBox.Checked = IsBitSet(flags4, 4);
                PathNodeFlags46CheckBox.Checked = IsBitSet(flags4, 5);
                PathNodeFlags47CheckBox.Checked = IsBitSet(flags4, 6);
                PathNodeFlags48CheckBox.Checked = IsBitSet(flags4, 7);

                PathNodeFlags51CheckBox.Checked = IsBitSet(flags5, 0);
                PathNodeFlags52CheckBox.Checked = IsBitSet(flags5, 1);
                PathNodeFlags53CheckBox.Checked = IsBitSet(flags5, 2);
            }
            if (updateUpDowns)
            {
                PathNodeFlags0UpDown.Value = flags0;
                PathNodeFlags1UpDown.Value = flags1;
                PathNodeFlags2UpDown.Value = flags2;
                PathNodeFlags3UpDown.Value = flags3;
                PathNodeFlags4UpDown.Value = flags4;
                PathNodeFlags5UpDown.Value = flags5;
            }

            var n = CurrentPathNode;
            if (n != null)
            {
                PathNodeFlags0Label.Text = n.Flags0.ToHexString();
                PathNodeFlags1Label.Text = n.Flags1.ToHexString();
                PathNodeFlags2Label.Text = n.Flags2.ToHexString();
                PathNodeFlags3Label.Text = n.Flags3.ToHexString();
                PathNodeFlags4Label.Text = n.Flags4.ToHexString();
            }
            else
            {
                PathNodeFlags0Label.Text = "0x00";
                PathNodeFlags1Label.Text = "0x00";
                PathNodeFlags2Label.Text = "0x00";
                PathNodeFlags3Label.Text = "0x00";
                PathNodeFlags4Label.Text = "0x00";
            }
        }

        private void SetPathNodeFlagsFromCheckBoxes()
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;

            uint flags0 = 0;
            uint flags1 = 0;
            uint flags2 = 0;
            uint flags3 = 0;
            uint flags4 = 0;
            uint flags5 = 0;
            flags0 = UpdateBit(flags0, 0, PathNodeFlags01CheckBox.Checked);
            flags0 = UpdateBit(flags0, 1, PathNodeFlags02CheckBox.Checked);
            flags0 = UpdateBit(flags0, 2, PathNodeFlags03CheckBox.Checked);
            flags0 = UpdateBit(flags0, 3, PathNodeFlags04CheckBox.Checked);
            flags0 = UpdateBit(flags0, 4, PathNodeFlags05CheckBox.Checked);
            flags0 = UpdateBit(flags0, 5, PathNodeFlags06CheckBox.Checked);
            flags0 = UpdateBit(flags0, 6, PathNodeFlags07CheckBox.Checked);
            flags0 = UpdateBit(flags0, 7, PathNodeFlags08CheckBox.Checked);

            flags1 = UpdateBit(flags1, 0, PathNodeFlags11CheckBox.Checked);
            flags1 = UpdateBit(flags1, 1, PathNodeFlags12CheckBox.Checked);
            flags1 = UpdateBit(flags1, 2, PathNodeFlags13CheckBox.Checked);
            flags1 = UpdateBit(flags1, 3, PathNodeFlags14CheckBox.Checked);
            flags1 = UpdateBit(flags1, 4, PathNodeFlags15CheckBox.Checked);
            flags1 = UpdateBit(flags1, 5, PathNodeFlags16CheckBox.Checked);
            flags1 = UpdateBit(flags1, 6, PathNodeFlags17CheckBox.Checked);
            flags1 = UpdateBit(flags1, 7, PathNodeFlags18CheckBox.Checked);

            flags2 = UpdateBit(flags2, 0, PathNodeFlags21CheckBox.Checked);
            flags2 = UpdateBit(flags2, 1, PathNodeFlags22CheckBox.Checked);
            flags2 = UpdateBit(flags2, 2, PathNodeFlags23CheckBox.Checked);
            flags2 = UpdateBit(flags2, 3, PathNodeFlags24CheckBox.Checked);
            flags2 = UpdateBit(flags2, 4, PathNodeFlags25CheckBox.Checked);
            flags2 = UpdateBit(flags2, 5, PathNodeFlags26CheckBox.Checked);
            flags2 = UpdateBit(flags2, 6, PathNodeFlags27CheckBox.Checked);
            flags2 = UpdateBit(flags2, 7, PathNodeFlags28CheckBox.Checked);

            flags3 = UpdateBit(flags3, 0, PathNodeFlags31CheckBox.Checked);
            flags3 += (((uint)PathNodeFlags32UpDown.Value & 127u) << 1);

            flags4 = UpdateBit(flags4, 0, PathNodeFlags41CheckBox.Checked);
            flags4 += (((uint)PathNodeFlags42UpDown.Value & 7u) << 1);
            flags4 = UpdateBit(flags4, 4, PathNodeFlags45CheckBox.Checked);
            flags4 = UpdateBit(flags4, 5, PathNodeFlags46CheckBox.Checked);
            flags4 = UpdateBit(flags4, 6, PathNodeFlags47CheckBox.Checked);
            flags4 = UpdateBit(flags4, 7, PathNodeFlags48CheckBox.Checked);

            flags5 = UpdateBit(flags5, 0, PathNodeFlags51CheckBox.Checked);
            flags5 = UpdateBit(flags5, 1, PathNodeFlags52CheckBox.Checked);
            flags5 = UpdateBit(flags5, 2, PathNodeFlags53CheckBox.Checked);


            lock (yndsyncroot)
            {
                if (CurrentPathNode.Flags0.Value != flags0)
                {
                    CurrentPathNode.Flags0 = (byte)flags0;
                    SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags1.Value != flags1)
                {
                    CurrentPathNode.Flags1 = (byte)flags1;
                    SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags2.Value != flags2)
                {
                    CurrentPathNode.Flags2 = (byte)flags2;
                    SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags3.Value != flags3)
                {
                    CurrentPathNode.Flags3 = (byte)flags3;
                    SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags4.Value != flags4)
                {
                    CurrentPathNode.Flags4 = (byte)flags4;
                    SetYndHasChanged(true);
                }
                if (CurrentPathNode.LinkCountUnk != flags5)
                {
                    CurrentPathNode.LinkCountUnk = (byte)flags5;
                    SetYndHasChanged(true);
                }
            }

            populatingui = true;
            UpdatePathNodeFlagsUI(false, true); //update updowns
            populatingui = false;
        }

        private void SetPathNodeFlagsFromUpDowns()
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;

            uint flags0 = (uint)PathNodeFlags0UpDown.Value;
            uint flags1 = (uint)PathNodeFlags1UpDown.Value;
            uint flags2 = (uint)PathNodeFlags2UpDown.Value;
            uint flags3 = (uint)PathNodeFlags3UpDown.Value;
            uint flags4 = (uint)PathNodeFlags4UpDown.Value;
            uint flags5 = (uint)PathNodeFlags5UpDown.Value;

            lock (yndsyncroot)
            {
                if (CurrentPathNode.Flags0.Value != flags0)
                {
                    CurrentPathNode.Flags0 = (byte)flags0;
                    SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags1.Value != flags1)
                {
                    CurrentPathNode.Flags1 = (byte)flags1;
                    SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags2.Value != flags2)
                {
                    CurrentPathNode.Flags2 = (byte)flags2;
                    SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags3.Value != flags3)
                {
                    CurrentPathNode.Flags3 = (byte)flags3;
                    SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags4.Value != flags4)
                {
                    CurrentPathNode.Flags4 = (byte)flags4;
                    SetYndHasChanged(true);
                }
                if (CurrentPathNode.LinkCountUnk != flags5)
                {
                    CurrentPathNode.LinkCountUnk = (byte)flags5;
                    SetYndHasChanged(true);
                }
            }

            populatingui = true;
            UpdatePathNodeFlagsUI(true, false); //update checkboxes
            populatingui = false;
        }


        private void UpdatePathNodeLinkFlagsUI(bool updateCheckboxes, bool updateUpDowns)
        {
            var flags0 = CurrentPathLink?.Flags0.Value ?? 0;
            var flags1 = CurrentPathLink?.Flags1.Value ?? 0;
            var flags2 = CurrentPathLink?.Flags2.Value ?? 0;


            if (updateCheckboxes)
            {
                PathNodeLinkFlags01CheckBox.Checked = IsBitSet(flags0, 0);
                PathNodeLinkFlags02CheckBox.Checked = IsBitSet(flags0, 1);
                PathNodeLinkFlags03UpDown.Value = (flags0 >> 2) & 7;
                PathNodeLinkFlags04UpDown.Value = (flags0 >> 5) & 7;

                PathNodeLinkFlags11CheckBox.Checked = IsBitSet(flags1, 0);
                PathNodeLinkFlags12CheckBox.Checked = IsBitSet(flags1, 1);
                PathNodeLinkFlags13CheckBox.Checked = IsBitSet(flags1, 2);
                PathNodeLinkFlags14CheckBox.Checked = IsBitSet(flags1, 3);
                PathNodeLinkOffsetSizeUpDown.Value = (flags1 >> 4) & 7;
                PathNodeLinkFlags18CheckBox.Checked = IsBitSet(flags1, 7);

                PathNodeLinkFlags21CheckBox.Checked = IsBitSet(flags2, 0);
                PathNodeLinkFlags22CheckBox.Checked = IsBitSet(flags2, 1);
                PathNodeLinkBackLanesUpDown.Value = (flags2 >> 2) & 7;
                PathNodeLinkFwdLanesUpDown.Value = (flags2 >> 5) & 7;
            }
            if (updateUpDowns)
            {
                PathNodeLinkFlags0UpDown.Value = flags0;
                PathNodeLinkFlags1UpDown.Value = flags1;
                PathNodeLinkFlags2UpDown.Value = flags2;
            }

            var l = CurrentPathLink;
            if (l != null)
            {
                PathNodeLinkFlags0Label.Text = l.Flags0.ToHexString();
                PathNodeLinkFlags1Label.Text = l.Flags1.ToHexString();
                PathNodeLinkFlags2Label.Text = l.Flags2.ToHexString();
            }
            else
            {
                PathNodeLinkFlags0Label.Text = "0x00";
                PathNodeLinkFlags1Label.Text = "0x00";
                PathNodeLinkFlags2Label.Text = "0x00";
            }
        }

        private void SetPathNodeLinkFlagsFromCheckBoxes()
        {
            if (populatingui) return;
            if (CurrentPathLink == null) return;

            uint flags0 = 0;
            uint flags1 = 0;
            uint flags2 = 0;
            flags0 = UpdateBit(flags0, 0, PathNodeLinkFlags01CheckBox.Checked);
            flags0 = UpdateBit(flags0, 1, PathNodeLinkFlags02CheckBox.Checked);
            flags0 += (((uint)PathNodeLinkFlags03UpDown.Value & 7u) << 2);
            flags0 += (((uint)PathNodeLinkFlags04UpDown.Value & 7u) << 5);

            flags1 = UpdateBit(flags1, 0, PathNodeLinkFlags11CheckBox.Checked);
            flags1 = UpdateBit(flags1, 1, PathNodeLinkFlags12CheckBox.Checked);
            flags1 = UpdateBit(flags1, 2, PathNodeLinkFlags13CheckBox.Checked);
            flags1 = UpdateBit(flags1, 3, PathNodeLinkFlags14CheckBox.Checked);
            flags1 += (((uint)PathNodeLinkOffsetSizeUpDown.Value & 7u) << 4);
            flags1 = UpdateBit(flags1, 7, PathNodeLinkFlags18CheckBox.Checked);

            flags2 = UpdateBit(flags2, 0, PathNodeLinkFlags21CheckBox.Checked);
            flags2 = UpdateBit(flags2, 1, PathNodeLinkFlags22CheckBox.Checked);
            flags2 += (((uint)PathNodeLinkBackLanesUpDown.Value & 7u) << 2);
            flags2 += (((uint)PathNodeLinkFwdLanesUpDown.Value & 7u) << 5);

            bool updgfx = false;
            lock (yndsyncroot)
            {
                if (CurrentPathLink.Flags0.Value != flags0)
                {
                    CurrentPathLink.Flags0 = (byte)flags0;
                    SetYndHasChanged(true);
                }
                if (CurrentPathLink.Flags1.Value != flags1)
                {
                    CurrentPathLink.Flags1 = (byte)flags1;
                    SetYndHasChanged(true);
                    updgfx = true;
                }
                if (CurrentPathLink.Flags2.Value != flags2)
                {
                    CurrentPathLink.Flags2 = (byte)flags2;
                    SetYndHasChanged(true);
                    updgfx = true;
                }
            }

            populatingui = true;
            UpdatePathNodeLinkFlagsUI(false, true); //update updowns
            populatingui = false;

            if (updgfx && (WorldForm != null) && (CurrentYndFile != null))
            {
                WorldForm.UpdatePathYndGraphics(CurrentYndFile, false);
            }
        }

        private void SetPathNodeLinkFlagsFromUpDowns()
        {
            if (populatingui) return;
            if (CurrentPathLink == null) return;

            uint flags0 = (uint)PathNodeLinkFlags0UpDown.Value;
            uint flags1 = (uint)PathNodeLinkFlags1UpDown.Value;
            uint flags2 = (uint)PathNodeLinkFlags2UpDown.Value;

            bool updgfx = false;
            lock (yndsyncroot)
            {
                if (CurrentPathLink.Flags0.Value != flags0)
                {
                    CurrentPathLink.Flags0 = (byte)flags0;
                    SetYndHasChanged(true);
                }
                if (CurrentPathLink.Flags1.Value != flags1)
                {
                    CurrentPathLink.Flags1 = (byte)flags1;
                    SetYndHasChanged(true);
                }
                if (CurrentPathLink.Flags2.Value != flags2)
                {
                    CurrentPathLink.Flags2 = (byte)flags2;
                    SetYndHasChanged(true);
                    updgfx = true;
                }
            }

            populatingui = true;
            UpdatePathNodeLinkFlagsUI(true, false); //update checkboxes
            populatingui = false;

            if (updgfx && (WorldForm != null) && (CurrentYndFile != null))
            {
                WorldForm.UpdatePathYndGraphics(CurrentYndFile, false);
            }
        }



        private void AddPathLink()
        {
            if (CurrentPathNode == null) return;

            var l = CurrentPathNode.AddLink();

            LoadPathNodeTabPage();

            PathNodeLinksListBox.SelectedItem = l;

            if (WorldForm != null)
            {
                WorldForm.UpdatePathNodeGraphics(CurrentPathNode, false);
            }
        }

        private void RemovePathLink()
        {
            if (CurrentPathLink == null) return;
            if (CurrentPathNode == null) return;

            var r = CurrentPathNode.RemoveLink(CurrentPathLink);

            if (!r) return;

            LoadPathNodeTabPage();

            if (WorldForm != null)
            {
                WorldForm.UpdatePathNodeGraphics(CurrentPathNode, false);
            }
        }

        private void UpdatePathNodeLinkage()
        {
            if (CurrentPathLink == null) return;
            if (CurrentYndFile == null) return;

            YndNode linknode = null;
            ushort areaid = CurrentPathLink._RawData.AreaID;
            ushort nodeid = CurrentPathLink._RawData.NodeID;
            if (areaid == CurrentYndFile.AreaID)
            {
                //link to the same ynd. find the new node in the current ynd.
                if ((CurrentYndFile.Nodes != null) && (nodeid < CurrentYndFile.Nodes.Length))
                {
                    linknode = CurrentYndFile.Nodes[nodeid];
                }
            }
            else
            {
                //try lookup the link node from the space.
                if (WorldForm != null)
                {
                    linknode = WorldForm.GetPathNodeFromSpace(areaid, nodeid);
                }
            }

            if (linknode == null)
            {
                PathNodeLinkageStatusLabel.Text = "Unable to find node " + areaid.ToString() + ":" + nodeid.ToString() + ".";
            }
            else
            {
                PathNodeLinkageStatusLabel.Text = "";
            }

            CurrentPathLink.Node2 = linknode;
            CurrentPathLink.UpdateLength();


            ////need to rebuild the link verts.. updating the graphics should do it...
            if (WorldForm != null)
            {
                WorldForm.UpdatePathYndGraphics(CurrentYndFile, false);
            }
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

        private void AddYnvToProject(YnvFile ynv)//TODO!
        {
        }

        private void RemoveYnvFromProject()//TODO!
        {
        }

        private void LoadYnvTabPage()//TODO!
        {
        }

        private void LoadYnvFromFile(YnvFile ynv, string filename)//TODO!
        {
        }

        private void LoadYnvTreeNodes(YnvFile ynv, TreeNode node)//TODO!
        {
        }

        private void SetYnvHasChanged(bool changed)//TODO!
        {
        }

        private void SetYnvHasChangedUI(bool changed)//TODO!
        {
        }


        public void NewNavPoly(YnvPoly copy = null, bool copyposition = false)//TODO!
        {
        }

        public bool DeleteNavPoly()//TODO!
        {
            return false;
        }

        private void AddNavPolyToProject()//TODO!
        {
        }

        private void LoadNavPolyTabPage()//TODO!
        {
        }

        public bool IsCurrentNavPoly(YnvPoly poly)
        {
            return poly == CurrentNavPoly;
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

            lock (trainsyncroot)
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
            lock (trainsyncroot) //need to sync writes to ynd objects...
            {
                saveas = saveas || string.IsNullOrEmpty(filepath);
                if (saveas)
                {
                    filepath = ShowSaveDialog("Dat files|*.dat", filepath);
                    if (string.IsNullOrEmpty(filepath))
                    { return; }

                    string newname = Path.GetFileNameWithoutExtension(filepath);
                    //JenkIndex.Ensure(newname);
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
                LoadTrainTrackTabPage();
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

        private void AddTrainTrackToProject(TrainTrack track)
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
                TrySelectTrainNodeTreeNode(CurrentTrainNode);
            }
        }

        private void RemoveTrainTrackFromProject()
        {
            if (CurrentTrainTrack == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveTrainsFile(CurrentTrainTrack);
            CurrentTrainTrack = null;
            LoadProjectTree();
            RefreshUI();
        }

        private void LoadTrainTrackTabPage()
        {
            if (CurrentTrainTrack == null)
            {
                TrainTrackFilePanel.Enabled = false;
                TrainTrackFilenameTextBox.Text = string.Empty;
                TrainTrackConfigNameTextBox.Text = string.Empty;
                TrainTrackIsPingPongCheckBox.Checked = false;
                TrainTrackStopsAtStationsCheckBox.Checked = false;
                TrainTrackMPStopsAtStationsCheckBox.Checked = false;
                TrainTrackSpeedTextBox.Text = string.Empty;
                TrainTrackBrakingDistTextBox.Text = string.Empty;
                TrainTrackRpfPathTextBox.Text = string.Empty;
                TrainTrackFilePathTextBox.Text = string.Empty;
                TrainTrackProjectPathTextBox.Text = string.Empty;
                TrainTrackInfoLabel.Text = string.Empty;
            }
            else
            {
                populatingui = true;
                TrainTrackFilePanel.Enabled = true;
                TrainTrackFilenameTextBox.Text = CurrentTrainTrack.filename;
                TrainTrackConfigNameTextBox.Text = CurrentTrainTrack.trainConfigName;
                TrainTrackIsPingPongCheckBox.Checked = CurrentTrainTrack.isPingPongTrack;
                TrainTrackStopsAtStationsCheckBox.Checked = CurrentTrainTrack.stopsAtStations;
                TrainTrackMPStopsAtStationsCheckBox.Checked = CurrentTrainTrack.MPstopsAtStations;
                TrainTrackSpeedTextBox.Text = FloatUtil.ToString(CurrentTrainTrack.speed);
                TrainTrackBrakingDistTextBox.Text = FloatUtil.ToString(CurrentTrainTrack.brakingDist);
                TrainTrackRpfPathTextBox.Text = CurrentTrainTrack.RpfFileEntry?.Path ?? string.Empty;
                TrainTrackFilePathTextBox.Text = string.Empty; //todo
                TrainTrackProjectPathTextBox.Text = string.Empty; //todo
                TrainTrackInfoLabel.Text = CurrentTrainTrack.StationCount.ToString() + " stations";
                populatingui = false;
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

        private void LoadTrainTrackTreeNodes(TrainTrack track, TreeNode node)
        {
            if (track == null) return;

            if (!string.IsNullOrEmpty(node.Name)) return; //named nodes are eg Nodes

            node.Nodes.Clear();



            if ((track.Nodes != null) && (track.Nodes.Count > 0))
            {
                var nodesnode = node.Nodes.Add("Nodes (" + track.Nodes.Count.ToString() + ")");
                nodesnode.Name = "Nodes";
                nodesnode.Tag = track;
                var nodes = track.Nodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var ynode = nodes[i];
                    var tnode = nodesnode.Nodes.Add(ynode.ToString());
                    tnode.Tag = ynode;
                }
            }

        }

        private void SetTrainTrackHasChanged(bool changed)
        {
            if (CurrentTrainTrack == null) return;

            bool changechange = changed != CurrentTrainTrack.HasChanged;
            if (!changechange) return;

            CurrentTrainTrack.HasChanged = changed;

            SetTrainTrackHasChangedUI(changed);
        }

        private void SetTrainTrackHasChangedUI(bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var pnode = ProjectTreeView.Nodes[0];
                var trnode = GetChildTreeNode(pnode, "Trains");
                if (trnode == null) return;
                string changestr = changed ? "*" : "";
                for (int i = 0; i < trnode.Nodes.Count; i++)
                {
                    var tnode = trnode.Nodes[i];
                    if (tnode.Tag == CurrentTrainTrack)
                    {
                        string name = CurrentTrainTrack.Name;
                        if (CurrentTrainTrack.RpfFileEntry != null)
                        {
                            name = CurrentTrainTrack.RpfFileEntry.Name;
                        }
                        tnode.Text = changestr + name;
                        break;
                    }
                }
            }
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

            TrySelectTrainNodeTreeNode(n);
            CurrentTrainNode = n;
            LoadTrainNodeTabPage();


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

            //find this now to remove it later.
            var tn = FindTrainNodeTreeNode(CurrentTrainNode);

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

            SetTrainTrackHasChangedUI(true);

            CurrentTrainNode = null;
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Nodes (" + CurrentTrainTrack.Nodes.Count.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
            else
            {
                //no need for this when removing the node above will select something else.
                LoadTrainNodeTabPage(); //this case really shouldn't happen... 
            }

            if (WorldForm != null)
            {
                WorldForm.UpdateTrainTrackGraphics(CurrentTrainTrack, false);
            }


            return true;
        }

        private void AddTrainNodeToProject()
        {
            if (CurrentTrainNode == null) return;
            if (CurrentTrainTrack != null)
            {
                AddTrainTrackToProject(CurrentTrainTrack);
            }
        }

        private void LoadTrainNodeTabPage()
        {
            if (CurrentTrainNode == null)
            {
                TrainNodePanel.Enabled = false;
                TrainNodeDeleteButton.Enabled = false;
                TrainNodeAddToProjectButton.Enabled = false;
                TrainNodePositionTextBox.Text = string.Empty;
                TrainNodeTypeComboBox.SelectedIndex = -1;
            }
            else
            {
                populatingui = true;
                TrainNodePanel.Enabled = true;
                TrainNodeDeleteButton.Enabled = TrainTrackExistsInProject(CurrentTrainTrack);
                TrainNodeAddToProjectButton.Enabled = !TrainNodeDeleteButton.Enabled;
                TrainNodePositionTextBox.Text = FloatUtil.GetVector3String(CurrentTrainNode.Position);
                TrainNodeTypeComboBox.SelectedIndex = CurrentTrainNode.NodeType;
                populatingui = false;

                if (WorldForm != null)
                {
                    WorldForm.SelectTrainTrackNode(CurrentTrainNode);
                }
            }
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

            lock (scenariosyncroot)
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
            lock (scenariosyncroot) //need to sync writes to scenario...
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
                LoadScenarioTabPage();
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

        private void AddScenarioToProject(YmtFile ymt)
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
                TrySelectScenarioNodeTreeNode(CurrentScenarioNode);
            }
        }

        private void RemoveScenarioFromProject()
        {
            if (CurrentScenario == null) return;
            if (CurrentProjectFile == null) return;
            CurrentProjectFile.RemoveScenarioFile(CurrentScenario);
            CurrentScenario = null;
            LoadProjectTree();
            RefreshUI();
        }

        private void LoadScenarioTabPage()
        {
            if (CurrentScenario == null)
            {
                populatingui = true;
                ScenarioYmtPanel.Enabled = false;
                ScenarioYmtNameTextBox.Text = string.Empty;
                ScenarioYmtVersionTextBox.Text = string.Empty;
                ScenarioYmtGridMinTextBox.Text = string.Empty;
                ScenarioYmtGridMaxTextBox.Text = string.Empty;
                ScenarioYmtGridScaleTextBox.Text = string.Empty;
                ScenarioYmtGridInfoLabel.Text = "Total grid points: 0";
                ScenarioYmtExtentsMinTextBox.Text = string.Empty;
                ScenarioYmtExtentsMaxTextBox.Text = string.Empty;
                ScenarioYmtFileLocationTextBox.Text = string.Empty;
                ScenarioYmtProjectPathTextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                var rgn = CurrentScenario.CScenarioPointRegion;
                var accg = rgn?._Data.AccelGrid ?? new rage__spdGrid2D();
                var bvh = CurrentScenario.ScenarioRegion?.BVH;
                var emin = bvh?.Box.Minimum ?? Vector3.Zero;
                var emax = bvh?.Box.Maximum ?? Vector3.Zero;

                populatingui = true;
                ScenarioYmtPanel.Enabled = true;
                ScenarioYmtNameTextBox.Text = CurrentScenario.Name;
                ScenarioYmtVersionTextBox.Text = rgn?.VersionNumber.ToString() ?? "";
                ScenarioYmtGridMinTextBox.Text = FloatUtil.GetVector2String(accg.Min);
                ScenarioYmtGridMaxTextBox.Text = FloatUtil.GetVector2String(accg.Max);
                ScenarioYmtGridScaleTextBox.Text = FloatUtil.GetVector2String(accg.Scale);
                ScenarioYmtGridInfoLabel.Text = "Total grid points: " + (rgn?.Unk_3844724227?.Length ?? 0).ToString();
                ScenarioYmtExtentsMinTextBox.Text = FloatUtil.GetVector3String(emin);
                ScenarioYmtExtentsMaxTextBox.Text = FloatUtil.GetVector3String(emax);
                ScenarioYmtFileLocationTextBox.Text = CurrentScenario.RpfFileEntry?.Path ?? "";
                ScenarioYmtProjectPathTextBox.Text = (CurrentProjectFile != null) ? CurrentProjectFile.GetRelativePath(CurrentScenario.FilePath) : CurrentScenario.FilePath;
                populatingui = false;
            }
        }

        private void LoadScenarioFromFile(YmtFile ymt, string filename)
        {
            byte[] data = File.ReadAllBytes(filename);

            ymt.LoadRSC(data);
        }

        private void LoadScenarioTreeNodes(YmtFile ymt, TreeNode node)
        {
            if (!string.IsNullOrEmpty(node.Name)) return; //named nodes are eg Points

            node.Nodes.Clear();

            var region = ymt?.ScenarioRegion;

            if (region == null) return;

            var nodes = region.Nodes;
            if ((nodes == null) || (nodes.Count == 0)) return;

            var pointsnode = node.Nodes.Add("Points (" + nodes.Count.ToString() + ")");
            pointsnode.Name = "Points";
            pointsnode.Tag = ymt;
            for (int i = 0; i < nodes.Count; i++)
            {
                var snode = nodes[i];
                var tnode = pointsnode.Nodes.Add(snode.MedTypeName + ": " + snode.StringText);
                tnode.Tag = snode;
            }

            //var sr = region.Region;
            //if (sr == null) return;
            //int pointCount = (sr.Points?.LoadSavePoints?.Length ?? 0) + (sr.Points?.MyPoints?.Length ?? 0);
            //int entityOverrideCount = (sr.EntityOverrides?.Length ?? 0);
            //int chainCount = (sr.Paths?.Chains?.Length ?? 0);
            //int clusterCount = (sr.Clusters?.Length ?? 0);
            //TreeNode pointsNode = null;
            //TreeNode entityOverridesNode = null;
            //TreeNode chainsNode = null;
            //TreeNode clustersNode = null;
            //if (pointCount > 0)
            //{
            //    pointsNode = node.Nodes.Add("Points (" + pointCount.ToString() + ")");
            //}
            //if (entityOverrideCount > 0)
            //{
            //    entityOverridesNode = node.Nodes.Add("Entity Overrides (" + entityOverrideCount.ToString() + ")");
            //}
            //if (chainCount > 0)
            //{
            //    chainsNode = node.Nodes.Add("Chains (" + chainsNode.ToString() + ")");
            //}
            //if (clusterCount > 0)
            //{
            //    clustersNode = node.Nodes.Add("Clusters (" + clusterCount.ToString() + ")");
            //}
            //for (int i = 0; i < nodes.Count; i++)
            //{
            //    var snode = nodes[i];
            //    if (snode == null) continue;
            //    if ((pointsNode != null) && ((snode.LoadSavePoint != null) || (snode.MyPoint != null)))
            //    {
            //        pointsNode.Nodes.Add(snode.ToString()).Tag = snode;
            //    }
            //    if ((entityOverridesNode != null) && ((snode.EntityOverride != null) || (snode.EntityPoint != null)))
            //    {
            //        entityOverridesNode.Nodes.Add(snode.ToString()).Tag = snode;
            //    }
            //    if ((chainsNode != null) && (snode.ChainingNode != null))
            //    {
            //        chainsNode.Nodes.Add(snode.ToString()).Tag = snode;
            //    }
            //    if ((clustersNode != null) && ((snode.Cluster != null) || (snode.ClusterLoadSavePoint != null) || (snode.ClusterMyPoint != null)))
            //    {
            //        clustersNode.Nodes.Add(snode.ToString()).Tag = snode;
            //    }
            //}

        }

        private void SetScenarioHasChanged(bool changed)
        {
            if (CurrentScenario == null) return;

            bool changechange = changed != CurrentScenario.HasChanged;
            if (!changechange) return;

            CurrentScenario.HasChanged = changed;

            SetScenarioHasChangedUI(changed);
        }

        private void SetScenarioHasChangedUI(bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var pnode = ProjectTreeView.Nodes[0];
                var scnode = GetChildTreeNode(pnode, "Scenarios");
                if (scnode == null) return;
                string changestr = changed ? "*" : "";
                for (int i = 0; i < scnode.Nodes.Count; i++)
                {
                    var snode = scnode.Nodes[i];
                    if (snode.Tag == CurrentScenario)
                    {
                        string name = CurrentScenario.Name;
                        if (CurrentScenario.RpfFileEntry != null)
                        {
                            name = CurrentScenario.RpfFileEntry.Name;
                        }
                        snode.Text = changestr + name;
                        break;
                    }
                }
            }
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

            TrySelectScenarioNodeTreeNode(n);
            CurrentScenarioNode = n;
            LoadScenarioTabPage();
            LoadScenarioNodeTabPages();


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

            //find this now to remove it later.
            var tn = FindScenarioNodeTreeNode(CurrentScenarioNode);

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

            SetScenarioHasChangedUI(true);

            CurrentScenarioNode = null;
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Points (" + (CurrentScenario?.ScenarioRegion?.Nodes?.Count ?? 0).ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
            else
            {
                //no need for this when removing the node above will select something else.
                LoadScenarioNodeTabPages(); //this case really shouldn't happen... 
            }

            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }


            return true;
        }

        private void AddScenarioNodeToProject()
        {
            if (CurrentScenarioNode == null) return;
            if (CurrentScenario != null)
            {
                AddScenarioToProject(CurrentScenario);
            }
        }

        private void LoadScenarioNodeTabPages()
        {
            populatingui = true;

            LoadScenarioDropDowns();

            LoadScenarioPointTabPage();
            LoadScenarioEntityTabPage();
            LoadScenarioEntityPointTabPage();
            LoadScenarioChainTabPage();
            LoadScenarioChainEdgeTabPage();
            LoadScenarioChainNodeTabPage();
            LoadScenarioClusterTabPage();
            LoadScenarioClusterPointTabPage();
            populatingui = false;

            if (CurrentScenarioNode != null)
            {
                if (WorldForm != null)
                {
                    WorldForm.SelectScenarioNode(CurrentScenarioNode);
                }
            }
        }

        private void LoadScenarioDropDowns()
        {
            if (ScenarioPointTypeComboBox.Items.Count > 0) return;

            var types = Scenarios.ScenarioTypes; //these are loaded by Scenarios.Init
            if (types == null)
            { return; }

            var stypes = types.GetScenarioTypes();
            if (stypes == null) return;

            var pmsets = types.GetPedModelSets();
            if (pmsets == null) return;

            var vmsets = types.GetVehicleModelSets();
            if (vmsets == null) return;

            ScenarioPointTypeComboBox.Items.Clear();
            ScenarioPointTypeComboBox.Items.Add("");
            ScenarioClusterPointTypeComboBox.Items.Clear();
            ScenarioClusterPointTypeComboBox.Items.Add("");
            ScenarioChainNodeTypeComboBox.Items.Clear();
            ScenarioChainNodeTypeComboBox.Items.Add("");
            foreach (var stype in stypes)
            {
                ScenarioPointTypeComboBox.Items.Add(stype);
                ScenarioClusterPointTypeComboBox.Items.Add(stype);
                ScenarioChainNodeTypeComboBox.Items.Add(stype);
            }

            ScenarioPointModelSetComboBox.Items.Clear();
            ScenarioPointModelSetComboBox.Items.Add("");
            ScenarioClusterPointModelSetComboBox.Items.Clear();
            ScenarioClusterPointModelSetComboBox.Items.Add("");
            foreach (var pmset in pmsets)
            {
                ScenarioPointModelSetComboBox.Items.Add(pmset);
                ScenarioClusterPointModelSetComboBox.Items.Add(pmset);
            }
            foreach (var vmset in vmsets)
            {
                ScenarioPointModelSetComboBox.Items.Add(vmset);
                ScenarioClusterPointModelSetComboBox.Items.Add(vmset);
            }


            ScenarioEntityPointAvailableInMpSpComboBox.Items.Clear();
            ScenarioEntityPointAvailableInMpSpComboBox.Items.Add(Unk_3573596290.kBoth);
            ScenarioEntityPointAvailableInMpSpComboBox.Items.Add(Unk_3573596290.kOnlySp);
            ScenarioEntityPointAvailableInMpSpComboBox.Items.Add(Unk_3573596290.kOnlyMp);


            ScenarioChainEdgeActionComboBox.Items.Clear();
            ScenarioChainEdgeActionComboBox.Items.Add(Unk_3609807418.Move);
            ScenarioChainEdgeActionComboBox.Items.Add(Unk_3609807418.Unk_7865678);
            ScenarioChainEdgeActionComboBox.Items.Add(Unk_3609807418.MoveFollowMaster);

            ScenarioChainEdgeNavModeComboBox.Items.Clear();
            ScenarioChainEdgeNavModeComboBox.Items.Add(Unk_3971773454.Direct);
            ScenarioChainEdgeNavModeComboBox.Items.Add(Unk_3971773454.NavMesh);
            ScenarioChainEdgeNavModeComboBox.Items.Add(Unk_3971773454.Roads);

            ScenarioChainEdgeNavSpeedComboBox.Items.Clear();
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_00_3279574318);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_01_2212923970);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_02_4022799658);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_03_1425672334);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_04_957720931);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_05_3795195414);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_06_2834622009);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_07_1876554076);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_08_698543797);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_09_1544199634);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_10_2725613303);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_11_4033265820);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_12_3054809929);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_13_3911005380);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_14_3717649022);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_15_3356026130);

        }

        private void LoadScenarioPointTabPage()
        {
            var p = CurrentScenarioNode?.MyPoint;
            if (p == null)
            {
                ScenarioPointPanel.Enabled = false;
                ScenarioPointCheckBox.Checked = false;
                ScenarioPointAddToProjectButton.Enabled = false;
                ScenarioPointDeleteButton.Enabled = false;
                ScenarioPointPositionTextBox.Text = "";
                ScenarioPointDirectionTextBox.Text = "";
                ScenarioPointTypeComboBox.SelectedItem = null;
                ScenarioPointModelSetComboBox.SelectedItem = null;
                ScenarioPointInteriorTextBox.Text = "";
                ScenarioPointInteriorHashLabel.Text = "Hash: 0";
                ScenarioPointGroupTextBox.Text = "";
                ScenarioPointGroupHashLabel.Text = "Hash: 0";
                ScenarioPointImapTextBox.Text = "";
                ScenarioPointImapHashLabel.Text = "Hash: 0";
                ScenarioPointTimeStartUpDown.Value = 0;
                ScenarioPointTimeEndUpDown.Value = 0;
                ScenarioPointProbabilityUpDown.Value = 0;
                ScenarioPointSpOnlyFlagUpDown.Value = 0;
                ScenarioPointRadiusUpDown.Value = 0;
                ScenarioPointWaitTimeUpDown.Value = 0;
                ScenarioPointFlagsValueUpDown.Value = 0;
                foreach (int i in ScenarioPointFlagsCheckedListBox.CheckedIndices)
                {
                    ScenarioPointFlagsCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            else
            {
                ScenarioPointPanel.Enabled = true;
                ScenarioPointCheckBox.Checked = true;
                ScenarioPointDeleteButton.Enabled = ScenarioExistsInProject(CurrentScenario);
                ScenarioPointAddToProjectButton.Enabled = !ScenarioPointDeleteButton.Enabled;
                ScenarioPointPositionTextBox.Text = FloatUtil.GetVector3String(p.Position);
                ScenarioPointDirectionTextBox.Text = FloatUtil.ToString(p.Direction);
                ScenarioPointTypeComboBox.SelectedItem = ((object)p.Type) ?? "";
                ScenarioPointModelSetComboBox.SelectedItem = ((object)p.ModelSet) ?? "";
                ScenarioPointInteriorTextBox.Text = p.InteriorName.ToString();
                ScenarioPointInteriorHashLabel.Text = "Hash: " + p.InteriorName.Hash.ToString();
                ScenarioPointGroupTextBox.Text = p.GroupName.ToString();
                ScenarioPointGroupHashLabel.Text = "Hash: " + p.GroupName.Hash.ToString();
                ScenarioPointImapTextBox.Text = p.IMapName.ToString();
                ScenarioPointImapHashLabel.Text = "Hash: " + p.IMapName.Hash.ToString();
                ScenarioPointTimeStartUpDown.Value = p.TimeStart;
                ScenarioPointTimeEndUpDown.Value = p.TimeEnd;
                ScenarioPointProbabilityUpDown.Value = p.Probability;
                ScenarioPointSpOnlyFlagUpDown.Value = p.AvailableMpSp;
                ScenarioPointRadiusUpDown.Value = p.Radius;
                ScenarioPointWaitTimeUpDown.Value = p.WaitTime;
                var iflags = (int)p.Flags;
                ScenarioPointFlagsValueUpDown.Value = iflags;
                for (int i = 0; i < ScenarioPointFlagsCheckedListBox.Items.Count; i++)
                {
                    var c = ((iflags & (1 << i)) > 0);
                    ScenarioPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
                }
            }
        }

        private void LoadScenarioEntityTabPage()
        {
            var e = CurrentScenarioNode?.Entity;
            if (e == null)
            {
                ScenarioEntityPanel.Enabled = false;
                ScenarioEntityCheckBox.Checked = false;
                ScenarioEntityAddToProjectButton.Enabled = false;
                ScenarioEntityDeleteButton.Enabled = false;
                ScenarioEntityPositionTextBox.Text = "";
                ScenarioEntityTypeTextBox.Text = "";
                ScenarioEntityTypeHashLabel.Text = "Hash: 0";
                ScenarioEntityUnk1UpDown.Value = 0;
                ScenarioEntityUnk2UpDown.Value = 0;
                ScenarioEntityInfoLabel.Text = "0 override points";
                ScenarioEntityPointsListBox.Items.Clear();
                ScenarioEntityAddPointButton.Enabled = false;
            }
            else
            {
                ScenarioEntityPanel.Enabled = true;
                ScenarioEntityCheckBox.Checked = true;
                ScenarioEntityDeleteButton.Enabled = ScenarioExistsInProject(CurrentScenario);
                ScenarioEntityAddToProjectButton.Enabled = !ScenarioEntityDeleteButton.Enabled;
                ScenarioEntityPositionTextBox.Text = FloatUtil.GetVector3String(e.Position);
                ScenarioEntityTypeTextBox.Text = e.TypeName.ToString();
                ScenarioEntityTypeHashLabel.Text = "Hash: " + e.TypeName.Hash.ToString();
                ScenarioEntityUnk1UpDown.Value = e.Unk1;
                ScenarioEntityUnk2UpDown.Value = e.Unk2;
                var pc = e.ScenarioPoints?.Length ?? 0;
                ScenarioEntityInfoLabel.Text = pc.ToString() + " override point" + ((pc != 1) ? "s" : "");
                ScenarioEntityPointsListBox.Items.Clear();
                ScenarioEntityAddPointButton.Enabled = true;

                if (e.ScenarioPoints != null)
                {
                    foreach (var point in e.ScenarioPoints)
                    {
                        ScenarioEntityPointsListBox.Items.Add(point);
                    }
                    if (CurrentScenarioNode.EntityPoint != null)
                    {
                        ScenarioEntityPointsListBox.SelectedItem = CurrentScenarioNode.EntityPoint;
                    }
                }
            }
        }

        private void LoadScenarioEntityPointTabPage()
        {
            var p = CurrentScenarioNode?.EntityPoint;
            if (p == null)
            {
                ScenarioEntityPointPanel.Enabled = false;
                ScenarioEntityPointCheckBox.Checked = false;
                ScenarioEntityPointAddToProjectButton.Enabled = false;
                ScenarioEntityPointDeleteButton.Enabled = false;
                ScenarioEntityPointNameTextBox.Text = "";
                ScenarioEntityPointNameHashLabel.Text = "Hash: 0";
                ScenarioEntityPointPositionTextBox.Text = "";
                ScenarioEntityPointRotationTextBox.Text = "";
                ScenarioEntityPointSpawnTypeTextBox.Text = "";
                ScenarioEntityPointSpawnTypeHashLabel.Text = "Hash: 0";
                ScenarioEntityPointPedTypeTextBox.Text = "";
                ScenarioEntityPointPedTypeHashLabel.Text = "Hash: 0";
                ScenarioEntityPointGroupTextBox.Text = "";
                ScenarioEntityPointGroupHashLabel.Text = "Hash: 0";
                ScenarioEntityPointInteriorTextBox.Text = "";
                ScenarioEntityPointInteriorHashLabel.Text = "Hash: 0";
                ScenarioEntityPointRequiredImapTextBox.Text = "";
                ScenarioEntityPointRequiredImapHashLabel.Text = "Hash: 0";
                ScenarioEntityPointAvailableInMpSpComboBox.SelectedItem = null;
                ScenarioEntityPointProbabilityTextBox.Text = "";
                ScenarioEntityPointTimeTillPedLeavesTextBox.Text = "";
                ScenarioEntityPointRadiusTextBox.Text = "";
                ScenarioEntityPointStartUpDown.Value = 0;
                ScenarioEntityPointEndUpDown.Value = 0;
                ScenarioEntityPointExtendedRangeCheckBox.Checked = false;
                ScenarioEntityPointShortRangeCheckBox.Checked = false;
                ScenarioEntityPointHighPriCheckBox.Checked = false;
                ScenarioEntityPointFlagsUpDown.Value = 0;
                foreach (int i in ScenarioEntityPointFlagsCheckedListBox.CheckedIndices)
                {
                    ScenarioEntityPointFlagsCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            else
            {
                ScenarioEntityPointPanel.Enabled = true;
                ScenarioEntityPointCheckBox.Checked = true;
                ScenarioEntityPointDeleteButton.Enabled = ScenarioExistsInProject(CurrentScenario);
                ScenarioEntityPointAddToProjectButton.Enabled = !ScenarioEntityPointDeleteButton.Enabled;
                ScenarioEntityPointNameTextBox.Text = p.NameHash.ToString();
                ScenarioEntityPointNameHashLabel.Text = "Hash: " + p.NameHash.Hash.ToString();
                ScenarioEntityPointPositionTextBox.Text = FloatUtil.GetVector3String(p.OffsetPosition);
                ScenarioEntityPointRotationTextBox.Text = FloatUtil.GetVector4String(p.OffsetRotation);
                ScenarioEntityPointSpawnTypeTextBox.Text = p.SpawnType.ToString();
                ScenarioEntityPointSpawnTypeHashLabel.Text = "Hash: " + p.SpawnType.Hash.ToString();
                ScenarioEntityPointPedTypeTextBox.Text = p.PedType.ToString();
                ScenarioEntityPointPedTypeHashLabel.Text = "Hash: " + p.PedType.Hash.ToString();
                ScenarioEntityPointGroupTextBox.Text = p.Group.ToString();
                ScenarioEntityPointGroupHashLabel.Text = "Hash: " + p.Group.Hash.ToString();
                ScenarioEntityPointInteriorTextBox.Text = p.Interior.ToString();
                ScenarioEntityPointInteriorHashLabel.Text = "Hash: " + p.Interior.Hash.ToString();
                ScenarioEntityPointRequiredImapTextBox.Text = p.RequiredImap.ToString();
                ScenarioEntityPointRequiredImapHashLabel.Text = "Hash: " + p.RequiredImap.Hash.ToString();
                ScenarioEntityPointAvailableInMpSpComboBox.SelectedItem = p.AvailableInMpSp;
                ScenarioEntityPointProbabilityTextBox.Text = FloatUtil.ToString(p.Probability);
                ScenarioEntityPointTimeTillPedLeavesTextBox.Text = FloatUtil.ToString(p.TimeTillPedLeaves);
                ScenarioEntityPointRadiusTextBox.Text = FloatUtil.ToString(p.Radius);
                ScenarioEntityPointStartUpDown.Value = p.StartTime;
                ScenarioEntityPointEndUpDown.Value = p.EndTime;
                ScenarioEntityPointExtendedRangeCheckBox.Checked = p.ExtendedRange;
                ScenarioEntityPointShortRangeCheckBox.Checked = p.ShortRange;
                ScenarioEntityPointHighPriCheckBox.Checked = p.HighPri;
                var iflags = (int)p.Flags;
                ScenarioEntityPointFlagsUpDown.Value = 0;
                for (int i = 0; i < ScenarioEntityPointFlagsCheckedListBox.Items.Count; i++)
                {
                    var c = ((iflags & (1 << i)) > 0);
                    ScenarioEntityPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
                }

            }
        }

        private void LoadScenarioChainTabPage()
        {
            CurrentScenarioChainEdge = null;

            var n = CurrentScenarioNode?.ChainingNode;
            if (n == null)
            {
                ScenarioChainAddToProjectButton.Enabled = false;
                ScenarioChainDeleteButton.Enabled = false;
                ScenarioChainEdgesListBox.Items.Clear();
                ScenarioChainEdgeCountLabel.Text = "Edge Count: 0";
                ScenarioChainUnk1UpDown.Value = 0;
            }
            else
            {
                ScenarioChainDeleteButton.Enabled = ScenarioChainNodeDeleteButton.Enabled;// ScenarioExistsInProject(CurrentScenario);
                ScenarioChainAddToProjectButton.Enabled = !ScenarioChainDeleteButton.Enabled;
                ScenarioChainEdgesListBox.Items.Clear();
                ScenarioChainEdgeCountLabel.Text = "Edge Count: " + (n.Chain?.EdgeIds?.Length ?? 0).ToString();
                ScenarioChainUnk1UpDown.Value = n.Chain?.Unk1 ?? 0;

                if ((n.Chain != null) && (n.Chain.Edges != null))
                {
                    foreach (var edge in n.Chain.Edges)
                    {
                        ScenarioChainEdgesListBox.Items.Add(edge);
                    }
                }
                else
                { }
            }
        }

        private void LoadScenarioChainEdgeTabPage()
        {
            var e = CurrentScenarioChainEdge;
            if (e == null)
            {
                ScenarioChainEdgePanel.Enabled = false;
                ScenarioChainEdgeNodeIndexFromUpDown.Value = 0;
                ScenarioChainEdgeNodeIndexToUpDown.Value = 0;
                ScenarioChainEdgeActionComboBox.SelectedItem = null;
                ScenarioChainEdgeNavModeComboBox.SelectedItem = null;
                ScenarioChainEdgeNavSpeedComboBox.SelectedItem = null;
                ScenarioChainMoveEdgeDownButton.Enabled = false;
                ScenarioChainMoveEdgeUpButton.Enabled = false;
            }
            else
            {
                ScenarioChainEdgePanel.Enabled = true;
                ScenarioChainEdgeNodeIndexFromUpDown.Value = e.NodeIndexFrom;
                ScenarioChainEdgeNodeIndexToUpDown.Value = e.NodeIndexTo;
                ScenarioChainEdgeActionComboBox.SelectedItem = e.Action;
                ScenarioChainEdgeNavModeComboBox.SelectedItem = e.NavMode;
                ScenarioChainEdgeNavSpeedComboBox.SelectedItem = e.NavSpeed;
                ScenarioChainMoveEdgeDownButton.Enabled = true;
                ScenarioChainMoveEdgeUpButton.Enabled = true;

                if (WorldForm != null)
                {
                    WorldForm.SelectScenarioEdge(CurrentScenarioNode, e);
                }
            }
        }

        private void LoadScenarioChainNodeTabPage()
        {
            var n = CurrentScenarioNode?.ChainingNode;
            if (n == null)
            {
                ScenarioChainNodePanel.Enabled = false;
                ScenarioChainNodeCheckBox.Checked = false;
                ScenarioChainNodeAddToProjectButton.Enabled = false;
                ScenarioChainNodeDeleteButton.Enabled = false;
                ScenarioChainNodePositionTextBox.Text = "";
                ScenarioChainNodeUnk1TextBox.Text = "";
                ScenarioChainNodeUnk1HashLabel.Text = "Hash: 0";
                ScenarioChainNodeTypeComboBox.SelectedItem = null;
                ScenarioChainNodeFirstCheckBox.Checked = false;
                ScenarioChainNodeLastCheckBox.Checked = false;
                ScenarioChainNodeIndexTextBox.Text = "";
            }
            else
            {
                ScenarioChainNodePanel.Enabled = true;
                ScenarioChainNodeCheckBox.Checked = true;
                ScenarioChainNodeDeleteButton.Enabled = ScenarioExistsInProject(CurrentScenario);
                ScenarioChainNodeAddToProjectButton.Enabled = !ScenarioChainNodeDeleteButton.Enabled;
                ScenarioChainNodePositionTextBox.Text = FloatUtil.GetVector3String(n.Position);
                ScenarioChainNodeUnk1TextBox.Text = n.Unk1.ToString();
                ScenarioChainNodeUnk1HashLabel.Text = "Hash: " + n.Unk1.Hash.ToString();
                ScenarioChainNodeTypeComboBox.SelectedItem = ((object)n.Type) ?? "";
                ScenarioChainNodeFirstCheckBox.Checked = !n.NotFirst;
                ScenarioChainNodeLastCheckBox.Checked = !n.NotLast;
                ScenarioChainNodeIndexTextBox.Text = n.NodeIndex.ToString();
            }
        }

        private void LoadScenarioClusterTabPage()
        {
            var c = CurrentScenarioNode?.Cluster;
            if (c == null)
            {
                ScenarioClusterPanel.Enabled = false;
                ScenarioClusterCheckBox.Checked = false;
                ScenarioClusterAddToProjectButton.Enabled = false;
                ScenarioClusterDeleteButton.Enabled = false;
                ScenarioClusterCenterTextBox.Text = "";
                ScenarioClusterRadiusTextBox.Text = "";
                ScenarioClusterUnk1TextBox.Text = "";
                ScenarioClusterUnk2CheckBox.Checked = false;
                ScenarioClusterPointsListBox.Items.Clear();
                ScenarioClusterAddPointButton.Enabled = false;
            }
            else
            {
                ScenarioClusterPanel.Enabled = true;
                ScenarioClusterCheckBox.Checked = true;
                ScenarioClusterDeleteButton.Enabled = ScenarioExistsInProject(CurrentScenario);
                ScenarioClusterAddToProjectButton.Enabled = !ScenarioClusterDeleteButton.Enabled;
                ScenarioClusterCenterTextBox.Text = FloatUtil.GetVector3String(c.Position);
                ScenarioClusterRadiusTextBox.Text = FloatUtil.ToString(c.Radius);
                ScenarioClusterUnk1TextBox.Text = FloatUtil.ToString(c.Unk1);
                ScenarioClusterUnk2CheckBox.Checked = c.Unk2;
                ScenarioClusterPointsListBox.Items.Clear();
                ScenarioClusterAddPointButton.Enabled = true;

                if (c.Points != null)
                {
                    if (c.Points.MyPoints != null)
                    {
                        foreach (var point in c.Points.MyPoints)
                        {
                            ScenarioClusterPointsListBox.Items.Add(point);
                        }
                        if (CurrentScenarioNode.ClusterMyPoint != null)
                        {
                            ScenarioClusterPointsListBox.SelectedItem = CurrentScenarioNode.ClusterMyPoint;
                        }
                    }
                    if (c.Points.LoadSavePoints != null)
                    {
                        foreach (var point in c.Points.LoadSavePoints)
                        {
                            ScenarioClusterPointsListBox.Items.Add(point);
                        }
                        if (CurrentScenarioNode.ClusterLoadSavePoint != null)
                        {
                            ScenarioClusterPointsListBox.SelectedItem = CurrentScenarioNode.ClusterLoadSavePoint;
                        }
                    }
                }

            }
        }

        private void LoadScenarioClusterPointTabPage()
        {
            var p = CurrentScenarioNode?.ClusterMyPoint;
            if (p == null)
            {
                ScenarioClusterPointPanel.Enabled = false;
                ScenarioClusterPointCheckBox.Checked = false;
                ScenarioClusterPointAddToProjectButton.Enabled = false;
                ScenarioClusterPointDeleteButton.Enabled = false;
                ScenarioClusterPointPositionTextBox.Text = "";
                ScenarioClusterPointDirectionTextBox.Text = "";
                ScenarioClusterPointTypeComboBox.SelectedItem = null;
                ScenarioClusterPointModelSetComboBox.SelectedItem = null;
                ScenarioClusterPointInteriorTextBox.Text = "";
                ScenarioClusterPointInteriorHashLabel.Text = "Hash: 0";
                ScenarioClusterPointGroupTextBox.Text = "";
                ScenarioClusterPointGroupHashLabel.Text = "Hash: 0";
                ScenarioClusterPointImapTextBox.Text = "";
                ScenarioClusterPointImapHashLabel.Text = "Hash: 0";
                ScenarioClusterPointTimeStartUpDown.Value = 0;
                ScenarioClusterPointTimeEndUpDown.Value = 0;
                ScenarioClusterPointProbabilityUpDown.Value = 0;
                ScenarioClusterPointAnimalFlagUpDown.Value = 0;
                ScenarioClusterPointRadiusUpDown.Value = 0;
                ScenarioClusterPointWaitTimeUpDown.Value = 0;
                ScenarioClusterPointFlagsUpDown.Value = 0;
                foreach (int i in ScenarioClusterPointFlagsCheckedListBox.CheckedIndices)
                {
                    ScenarioClusterPointFlagsCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            else
            {
                ScenarioClusterPointPanel.Enabled = true;
                ScenarioClusterPointCheckBox.Checked = true;
                ScenarioClusterPointDeleteButton.Enabled = ScenarioExistsInProject(CurrentScenario);
                ScenarioClusterPointAddToProjectButton.Enabled = !ScenarioClusterPointDeleteButton.Enabled;
                ScenarioClusterPointPositionTextBox.Text = FloatUtil.GetVector3String(p.Position);
                ScenarioClusterPointDirectionTextBox.Text = FloatUtil.ToString(p.Direction);
                ScenarioClusterPointTypeComboBox.SelectedItem = ((object)p.Type) ?? "";
                ScenarioClusterPointModelSetComboBox.SelectedItem = ((object)p.ModelSet) ?? "";
                ScenarioClusterPointInteriorTextBox.Text = p.InteriorName.ToString();
                ScenarioClusterPointInteriorHashLabel.Text = "Hash: " + p.InteriorName.Hash.ToString();
                ScenarioClusterPointGroupTextBox.Text = p.GroupName.ToString();
                ScenarioClusterPointGroupHashLabel.Text = "Hash: " + p.GroupName.Hash.ToString();
                ScenarioClusterPointImapTextBox.Text = p.IMapName.ToString();
                ScenarioClusterPointImapHashLabel.Text = "Hash: " + p.IMapName.Hash.ToString();
                ScenarioClusterPointTimeStartUpDown.Value = p.TimeStart;
                ScenarioClusterPointTimeEndUpDown.Value = p.TimeEnd;
                ScenarioClusterPointProbabilityUpDown.Value = p.Probability;
                ScenarioClusterPointAnimalFlagUpDown.Value = p.AvailableMpSp;
                ScenarioClusterPointRadiusUpDown.Value = p.Radius;
                ScenarioClusterPointWaitTimeUpDown.Value = p.WaitTime;
                var iflags = (int)p.Flags;
                ScenarioClusterPointFlagsUpDown.Value = iflags;
                for (int i = 0; i < ScenarioClusterPointFlagsCheckedListBox.Items.Count; i++)
                {
                    var c = ((iflags & (1 << i)) > 0);
                    ScenarioClusterPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
                }
            }
        }

        private void SelectScenarioNodeTabPages(ScenarioNode node)
        {
            //select the appropriate tab page(s) for the given node.

            if (node == null) return;

            bool reseltree = ProjectTreeView.Focused;
            bool change = false;

            if (node.MyPoint != null)
            {
                bool sw = true;
                if ((node.Entity != null) && (ScenarioTabControl.SelectedTab == ScenarioEntityTabPage))
                {
                    sw = false;
                }
                if ((node.EntityPoint != null) && (ScenarioTabControl.SelectedTab == ScenarioEntityPointTabPage))
                {
                    sw = false;
                }
                if ((node.Cluster != null) && (ScenarioTabControl.SelectedTab == ScenarioClusterTabPage))
                {
                    sw = false;
                }
                if ((node.ClusterMyPoint != null) && (ScenarioTabControl.SelectedTab == ScenarioClusterPointTabPage))
                {
                    sw = false;
                }
                if ((node.ChainingNode != null) && (ScenarioTabControl.SelectedTab == ScenarioChainTabPage))
                {
                    sw = false;
                }
                if ((node.ChainingNode != null) && (ScenarioTabControl.SelectedTab == ScenarioChainNodeTabPage))
                {
                    sw = false;
                }

                if (sw)
                {
                    change = ScenarioTabControl.SelectedTab != ScenarioPointTabPage;
                    ScenarioTabControl.SelectedTab = ScenarioPointTabPage;
                }
            }
            else if (node.EntityPoint != null)
            {
                if (ScenarioTabControl.SelectedTab != ScenarioEntityTabPage)
                {
                    change = ScenarioTabControl.SelectedTab != ScenarioEntityPointTabPage;
                    ScenarioTabControl.SelectedTab = ScenarioEntityPointTabPage;
                }
            }
            else if (node.Entity != null)
            {
                change = ScenarioTabControl.SelectedTab != ScenarioEntityTabPage;
                ScenarioTabControl.SelectedTab = ScenarioEntityTabPage;
            }
            else if (node.ClusterMyPoint != null)
            {
                if (ScenarioTabControl.SelectedTab != ScenarioClusterTabPage)
                {
                    change = ScenarioTabControl.SelectedTab != ScenarioClusterPointTabPage;
                    ScenarioTabControl.SelectedTab = ScenarioClusterPointTabPage;
                }
            }
            else if (node.Cluster != null)
            {
                change = ScenarioTabControl.SelectedTab != ScenarioClusterTabPage;
                ScenarioTabControl.SelectedTab = ScenarioClusterTabPage;
            }
            else if (node.ChainingNode != null)
            {
                if (ScenarioTabControl.SelectedTab != ScenarioChainTabPage)
                {
                    change = ScenarioTabControl.SelectedTab != ScenarioChainNodeTabPage;
                    ScenarioTabControl.SelectedTab = ScenarioChainNodeTabPage;
                }
            }
            else //if (node.MyPoint != null)
            {
                change = ScenarioTabControl.SelectedTab != ScenarioPointTabPage;
                ScenarioTabControl.SelectedTab = ScenarioPointTabPage;
            }

            if (reseltree && change)
            {
                ProjectTreeView.Focus();
            }
        }

        public bool IsCurrentScenarioNode(ScenarioNode node)
        {
            return node == CurrentScenarioNode;
        }




        private void AddScenarioChain()
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






            LoadProjectTree();

            TrySelectScenarioNodeTreeNode(n1);
            CurrentScenarioNode = n1;
            CurrentScenarioChainEdge = ed;
            LoadScenarioChainTabPage();
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

        private void AddScenarioEdge()
        {
            var chain = CurrentScenarioNode?.ChainingNode?.Chain;
            if (chain == null) return;
            var paths = CurrentScenario?.CScenarioPointRegion?.Paths;
            if (paths == null) return;

            MCScenarioChainingEdge edge = new MCScenarioChainingEdge();
            if (CurrentScenarioChainEdge != null)
            {
                edge.Data = CurrentScenarioChainEdge.Data;
            }

            paths.AddEdge(edge);
            chain.AddEdge(edge);

            CurrentScenarioChainEdge = edge;

            UpdateScenarioEdgeLinkage();

            LoadScenarioChainTabPage();

            ScenarioChainEdgesListBox.SelectedItem = edge;
        }

        private void RemoveScenarioEdge()
        {
            if (CurrentScenarioChainEdge == null) return;
            if (CurrentScenario == null) return;

            var chain = CurrentScenarioNode?.ChainingNode?.Chain;
            if (chain == null) return;
            var paths = CurrentScenario?.CScenarioPointRegion?.Paths;
            if (paths == null) return;

            if (MessageBox.Show("Are you sure you want to delete this scenario chain edge?\n" + CurrentScenarioChainEdge.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            chain.RemoveEdge(CurrentScenarioChainEdge);
            paths.RemoveEdge(CurrentScenarioChainEdge);

            LoadScenarioChainTabPage();

            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                WorldForm.SelectScenarioEdge(CurrentScenarioNode, null);
            }
        }

        private void MoveScenarioEdge(bool moveDown)
        {

            var chain = CurrentScenarioNode?.ChainingNode?.Chain;
            if (chain == null) return;
            if (chain.Edges == null) return;
            if (chain.EdgeIds == null) return;

            if (CurrentScenarioChainEdge == null) return;

            var edges = CurrentScenario?.CScenarioPointRegion?.Paths?.Edges;
            if (edges == null) return;


            int lasti = (chain.Edges?.Length ?? 0) - 1;

            var edgeid = 0;
            for (int i = 0; i < chain.Edges.Length; i++)
            {
                if (chain.Edges[i] == CurrentScenarioChainEdge)
                {
                    edgeid = i;
                    break;
                }
            }

            if (!moveDown && (edgeid <= 0)) return;
            if (moveDown && (edgeid >= lasti)) return;

            var swapid = edgeid + (moveDown ? 1 : -1);
            var swaped = chain.Edges[swapid];

            chain.Edges[swapid] = CurrentScenarioChainEdge;
            chain.EdgeIds[swapid] = (ushort)CurrentScenarioChainEdge.EdgeIndex;
            chain.Edges[edgeid] = swaped;
            chain.EdgeIds[edgeid] = (ushort)swapid;

            var ce = CurrentScenarioChainEdge;

            LoadScenarioChainTabPage();

            CurrentScenarioChainEdge = ce;

            ScenarioChainEdgesListBox.SelectedItem = ce;

            //LoadScenarioChainEdgeTabPage();

        }

        private void UpdateScenarioEdgeLinkage()
        {
            if (CurrentScenarioChainEdge == null) return;
            if (CurrentScenario == null) return;


            var chains = CurrentScenario?.CScenarioPointRegion?.Paths;
            if (chains == null) return;

            var nodes = chains.Nodes;
            if (nodes == null) return;

            ushort nifrom = CurrentScenarioChainEdge.NodeIndexFrom;
            ushort nito = CurrentScenarioChainEdge.NodeIndexTo;

            if (nifrom < nodes.Length) CurrentScenarioChainEdge.NodeFrom = nodes[nifrom];
            if (nito < nodes.Length) CurrentScenarioChainEdge.NodeTo = nodes[nito];

            ////need to rebuild the link verts.. updating the graphics should do it...
            if (WorldForm != null)
            {
                WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }
        }

        private void DeleteScenarioChain()
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

            LoadProjectTree();

            if (!delpoints && (cn != null))
            {
                TrySelectScenarioNodeTreeNode(cn);
            }
            else
            {
                TrySelectScenarioTreeNode(cs);
            }


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




        private void AddScenarioCluster()//TODO: add defualt cluster points to new cluster
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








            LoadProjectTree();

            TrySelectScenarioNodeTreeNode(nc);
            CurrentScenarioNode = nc;
            LoadScenarioClusterTabPage();


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

        private void DeleteScenarioCluster()
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

            LoadProjectTree();

            if (!delpoints && (cn != null))
            {
                TrySelectScenarioNodeTreeNode(cn);
            }
            else
            {
                TrySelectScenarioTreeNode(cs);
            }


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

        private void AddScenarioClusterPoint()
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


            LoadProjectTree();

            TrySelectScenarioNodeTreeNode(n);
            CurrentScenarioNode = n;
            LoadScenarioClusterTabPage();
            LoadScenarioClusterPointTabPage();
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


        private void AddScenarioEntity()//TODO: add default entity point(s) to entity
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








            LoadProjectTree();

            TrySelectScenarioNodeTreeNode(ne);
            CurrentScenarioNode = ne;
            LoadScenarioEntityTabPage();


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

        private void DeleteScenarioEntity()
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

            LoadProjectTree();


            TrySelectScenarioTreeNode(cs);


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

        private void AddScenarioEntityPoint()
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


            LoadProjectTree();

            TrySelectScenarioNodeTreeNode(n);
            CurrentScenarioNode = n;
            LoadScenarioEntityTabPage();
            LoadScenarioEntityPointTabPage();
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








        private void ImportScenarioChain()
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
                TrySelectScenarioNodeTreeNode(lastnode);
                CurrentScenarioNode = lastnode;
            }

            CurrentScenarioChainEdge = lastedge;
            LoadScenarioChainTabPage();
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
            lock (ymapsyncroot)
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


            LoadProjectTree();



            CalcYmapFlags();

            CalcYmapExtents();


            MessageBox.Show(entcount.ToString() + " entities imported. \n" + carcount.ToString() + " car generators imported. \n" + pedcount.ToString() + " peds ignored. \n" + unkcount.ToString() + " others ignored.");

        }
















        public void GetVisibleYmaps(Camera camera, Dictionary<MetaHash, YmapFile> ymaps)
        {
            if (hidegtavmap)
            {
                ymaps.Clear(); //remove all the gtav ymaps.
            }

            if (renderentities && (CurrentProjectFile != null))
            {
                lock (ymapsyncroot)
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
            //eventually will need to change this to use a list of Ybn's...
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

            lock (yndsyncroot)
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

            lock (ynvsyncroot)
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

            lock (trainsyncroot)
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

            lock (scenariosyncroot)
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
                    var pathnode = sel.PathNode;
                    var pathlink = sel.PathLink;
                    var navpoly = sel.NavPoly;
                    var trainnode = sel.TrainTrackNode;
                    var scenariond = sel.ScenarioNode;
                    YmapFile ymap = ent?.Ymap;
                    YndFile ynd = pathnode?.Ynd;
                    YnvFile ynv = navpoly?.Ynv;
                    TrainTrack traintrack = trainnode?.Track;
                    YmtFile scenario = scenariond?.Ymt;

                    if (cargen != null)
                    {
                        ymap = cargen.Ymap;
                    }

                    if (YmapExistsInProject(ymap))
                    {
                        if (ent != CurrentEntity)
                        {
                            TrySelectEntityTreeNode(ent);
                        }
                        if (cargen != CurrentCarGen)
                        {
                            TrySelectCarGenTreeNode(cargen);
                        }
                    }
                    else if (YndExistsInProject(ynd))
                    {
                        if (pathnode != CurrentPathNode)
                        {
                            TrySelectPathNodeTreeNode(pathnode);
                        }
                    }
                    else if (YnvExistsInProject(ynv))
                    {
                        if (navpoly != CurrentNavPoly)
                        {
                            TrySelectNavPolyTreeNode(navpoly);
                        }
                    }
                    else if (TrainTrackExistsInProject(traintrack))
                    {
                        if (trainnode != CurrentTrainNode)
                        {
                            TrySelectTrainNodeTreeNode(trainnode);
                        }
                    }
                    else if (ScenarioExistsInProject(scenario))
                    {
                        if ((scenariond != null) && (scenariond != CurrentScenarioNode))
                        {
                            TrySelectScenarioNodeTreeNode(scenariond);
                        }
                    }
                    else
                    {
                        ProjectTreeView.SelectedNode = null;
                        if (ymap != null)
                        {
                            MainTabControl.SelectedTab = YmapTabPage;
                        }
                        else if (ynd != null)
                        {
                            MainTabControl.SelectedTab = YndTabPage;
                        }
                        else if (ynv != null)
                        {
                            MainTabControl.SelectedTab = YnvTabPage;
                        }
                        else if (traintrack != null)
                        {
                            MainTabControl.SelectedTab = TrainsTabPage;
                        }
                        else if (scenario != null)
                        {
                            MainTabControl.SelectedTab = ScenarioTabPage;
                        }
                        if (ent != null)
                        {
                            YmapTabControl.SelectedTab = YmapEntityTabPage;
                        }
                        else if (cargen != null)
                        {
                            YmapTabControl.SelectedTab = YmapCarGenTabPage;
                        }
                        if (pathnode != null)
                        {
                            YndTabControl.SelectedTab = YndNodeTabPage;
                        }
                        if (trainnode != null)
                        {
                            TrainsTabControl.SelectedTab = TrainNodeTabPage;
                        }
                        if (scenariond != null)
                        {
                            SelectScenarioNodeTabPages(scenariond);
                        }
                    }

                    CurrentYmapFile = ymap;
                    CurrentEntity = ent;
                    CurrentCarGen = cargen;
                    CurrentYndFile = ynd;
                    CurrentPathNode = pathnode;
                    CurrentPathLink = pathlink;
                    CurrentYnvFile = ynv;
                    CurrentNavPoly = navpoly;
                    CurrentTrainTrack = traintrack;
                    CurrentTrainNode = trainnode;
                    CurrentScenario = scenario;
                    CurrentScenarioNode = scenariond;
                    RefreshUI();
                }
            }
            catch { }
        }

        public void OnWorldSelectionModified(MapSelection sel, List<MapSelection> items)
        {
            if (sel.MultipleSelection)
            {
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
                        TrySelectEntityTreeNode(ent);
                    }

                    if (ent != CurrentEntity)
                    {
                        CurrentEntity = ent;
                        TrySelectEntityTreeNode(ent);
                    }

                    if (ent == CurrentEntity)
                    {
                        LoadEntityTabPage();

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
                        TrySelectCarGenTreeNode(cargen);
                    }

                    if (cargen != CurrentCarGen)
                    {
                        CurrentCarGen = cargen;
                        TrySelectCarGenTreeNode(cargen);
                    }

                    if (cargen == CurrentCarGen)
                    {
                        LoadCarGenTabPage();

                        UpdateCarGenTreeNode(cargen);

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
                        TrySelectPathNodeTreeNode(node);
                    }

                    if (node != CurrentPathNode)
                    {
                        CurrentPathNode = node;
                        TrySelectPathNodeTreeNode(node);
                    }

                    //if (link != CurrentPathLink)
                    //{
                    //    CurrentPathLink = link;
                    //    LoadPathNodeLinkPage();
                    //}

                    if (node == CurrentPathNode)
                    {
                        //LoadYndTabPage();
                        LoadPathNodeTabPage();

                        //UpdatePathNodeTreeNode(node);

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
                        TrySelectNavPolyTreeNode(poly);
                    }

                    if (poly != CurrentNavPoly)
                    {
                        CurrentNavPoly = poly;
                        TrySelectNavPolyTreeNode(poly);
                    }

                    if (poly == CurrentNavPoly)
                    {
                        LoadYnvTabPage();

                        //UpdateNavPolyTreeNode(poly);

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
                        TrySelectTrainNodeTreeNode(node);
                    }

                    if (node != CurrentTrainNode)
                    {
                        CurrentTrainNode = node;
                        TrySelectTrainNodeTreeNode(node);
                    }

                    if (node == CurrentTrainNode)
                    {
                        LoadTrainNodeTabPage();

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
                        TrySelectScenarioNodeTreeNode(node);
                    }

                    if (node != CurrentScenarioNode)
                    {
                        CurrentScenarioNode = node;
                        TrySelectScenarioNodeTreeNode(node);
                    }

                    if (node == CurrentScenarioNode)
                    {
                        LoadScenarioTabPage();
                        LoadScenarioNodeTabPages();

                        if (node?.Ymt != null)
                        {
                            SetScenarioHasChanged(true);
                        }
                    }
                }
            }
            catch { }
        }










        private bool IsBitSet(uint value, int bit)
        {
            return (((value >> bit) & 1) > 0);
        }
        private uint SetBit(uint value, int bit)
        {
            return (value | (1u << bit));
        }
        private uint ClearBit(uint value, int bit)
        {
            return (value & (~(1u << bit)));
        }
        private uint UpdateBit(uint value, int bit, bool flag)
        {
            if (flag) return SetBit(value, bit);
            else return ClearBit(value, bit);
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







        private RpfFileEntry FindParentYmapEntry(uint hash)
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

        private bool YmapExistsInProject(YmapFile ymap)
        {
            if (ymap == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsYmap(ymap);
        }
        private bool YndExistsInProject(YndFile ynd)
        {
            if (ynd == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsYnd(ynd);
        }
        private bool YnvExistsInProject(YnvFile ynv)
        {
            if (ynv == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsYnv(ynv);
        }
        private bool TrainTrackExistsInProject(TrainTrack track)
        {
            if (track == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsTrainTrack(track);
        }
        private bool ScenarioExistsInProject(YmtFile ymt)
        {
            if (ymt == null) return false;
            if (CurrentProjectFile == null) return false;
            return CurrentProjectFile.ContainsScenario(ymt);
        }

        private TreeNode GetChildTreeNode(TreeNode node, string name)
        {
            if (node == null) return null;
            var nodes = node.Nodes.Find(name, false);
            if ((nodes == null) || (nodes.Length != 1)) return null;
            return nodes[0];
        }
        private TreeNode FindYmapTreeNode(YmapFile ymap)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var ymapsnode = GetChildTreeNode(projnode, "Ymap");
            if (ymapsnode == null) return null;
            for (int i = 0; i < ymapsnode.Nodes.Count; i++)
            {
                var ymapnode = ymapsnode.Nodes[i];
                if (ymapnode.Tag == ymap) return ymapnode;
            }
            return null;
        }
        private TreeNode FindEntityTreeNode(YmapEntityDef ent)
        {
            if (ent == null) return null;
            TreeNode ymapnode = FindYmapTreeNode(ent.Ymap);
            if (ymapnode == null) return null;
            var entsnode = GetChildTreeNode(ymapnode, "Entities");
            if (entsnode == null) return null;
            for (int i = 0; i < entsnode.Nodes.Count; i++)
            {
                TreeNode entnode = entsnode.Nodes[i];
                if (entnode.Tag == ent) return entnode;
            }
            return null;
        }
        private TreeNode FindCarGenTreeNode(YmapCarGen cargen)
        {
            if (cargen == null) return null;
            TreeNode ymapnode = FindYmapTreeNode(cargen.Ymap);
            if (ymapnode == null) return null;
            var cargensnode = GetChildTreeNode(ymapnode, "CarGens");
            if (cargensnode == null) return null;
            for (int i = 0; i < cargensnode.Nodes.Count; i++)
            {
                TreeNode cargennode = cargensnode.Nodes[i];
                if (cargennode.Tag == cargen) return cargennode;
            }
            return null;
        }
        private TreeNode FindYndTreeNode(YndFile ynd)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var yndsnode = GetChildTreeNode(projnode, "Ynd");
            if (yndsnode == null) return null;
            for (int i = 0; i < yndsnode.Nodes.Count; i++)
            {
                var yndnode = yndsnode.Nodes[i];
                if (yndnode.Tag == ynd) return yndnode;
            }
            return null;
        }
        private TreeNode FindPathNodeTreeNode(YndNode n)
        {
            if (n == null) return null;
            TreeNode yndnode = FindYndTreeNode(n.Ynd);
            var nodesnode = GetChildTreeNode(yndnode, "Nodes");
            if (nodesnode == null) return null;
            for (int i = 0; i < nodesnode.Nodes.Count; i++)
            {
                TreeNode nnode = nodesnode.Nodes[i];
                if (nnode.Tag == n) return nnode;
            }
            return null;
        }
        private TreeNode FindYnvTreeNode(YnvFile ynv)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var ynvsnode = GetChildTreeNode(projnode, "Ynv");
            if (ynvsnode == null) return null;
            for (int i = 0; i < ynvsnode.Nodes.Count; i++)
            {
                var yndnode = ynvsnode.Nodes[i];
                if (yndnode.Tag == ynv) return yndnode;
            }
            return null;
        }
        private TreeNode FindNavPolyTreeNode(YnvPoly p)
        {
            if (p == null) return null;
            TreeNode ynvnode = FindYnvTreeNode(p.Ynv);
            var polysnode = GetChildTreeNode(ynvnode, "Polygons");
            if (polysnode == null) return null;
            for (int i = 0; i < polysnode.Nodes.Count; i++)
            {
                TreeNode pnode = polysnode.Nodes[i];
                if (pnode.Tag == p) return pnode;
            }
            return null;
        }
        private TreeNode FindTrainTrackTreeNode(TrainTrack track)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var trainsnode = GetChildTreeNode(projnode, "Trains");
            if (trainsnode == null) return null;
            for (int i = 0; i < trainsnode.Nodes.Count; i++)
            {
                var trainnode = trainsnode.Nodes[i];
                if (trainnode.Tag == track) return trainnode;
            }
            return null;
        }
        private TreeNode FindTrainNodeTreeNode(TrainTrackNode n)
        {
            if (n == null) return null;
            TreeNode tracknode = FindTrainTrackTreeNode(n.Track);
            var nodesnode = GetChildTreeNode(tracknode, "Nodes");
            if (nodesnode == null) return null;
            for (int i = 0; i < nodesnode.Nodes.Count; i++)
            {
                TreeNode nnode = nodesnode.Nodes[i];
                if (nnode.Tag == n) return nnode;
            }
            return null;
        }
        private TreeNode FindScenarioTreeNode(YmtFile ymt)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var scenariosnode = GetChildTreeNode(projnode, "Scenarios");
            if (scenariosnode == null) return null;
            for (int i = 0; i < scenariosnode.Nodes.Count; i++)
            {
                var ymtnode = scenariosnode.Nodes[i];
                if (ymtnode.Tag == ymt) return ymtnode;
            }
            return null;
        }
        private TreeNode FindScenarioNodeTreeNode(ScenarioNode p)
        {
            if (p == null) return null;
            TreeNode ymtnode = FindScenarioTreeNode(p.Ymt);
            var pointsnode = GetChildTreeNode(ymtnode, "Points");
            if (pointsnode == null) return null;
            for (int i = 0; i < pointsnode.Nodes.Count; i++)
            {
                TreeNode pnode = pointsnode.Nodes[i];
                if (pnode.Tag == p) return pnode;
            }
            return null;
        }


        private void TrySelectEntityTreeNode(YmapEntityDef ent)
        {
            TreeNode entnode = FindEntityTreeNode(ent);
            if (entnode != null)
            {
                ProjectTreeView.SelectedNode = entnode;
            }
        }

        private void TrySelectCarGenTreeNode(YmapCarGen cargen)
        {
            TreeNode cargennode = FindCarGenTreeNode(cargen);
            if (cargennode != null)
            {
                ProjectTreeView.SelectedNode = cargennode;
            }
        }
        private void UpdateCarGenTreeNode(YmapCarGen cargen)
        {
            var tn = FindCarGenTreeNode(cargen);
            if (tn != null)
            {
                tn.Text = cargen.ToString();
            }
        }

        private void TrySelectPathNodeTreeNode(YndNode node)
        {
            TreeNode tnode = FindPathNodeTreeNode(node);
            if (tnode == null)
            {
                tnode = FindYndTreeNode(node?.Ynd);
            }
            if (tnode != null)
            {
                ProjectTreeView.SelectedNode = tnode;
            }
        }
        private void UpdatePathNodeTreeNode(YndNode node)
        {
            var tn = FindPathNodeTreeNode(node);
            if (tn != null)
            {
                tn.Text = node._RawData.ToString();
            }
        }

        private void TrySelectNavPolyTreeNode(YnvPoly poly)
        {
            TreeNode tnode = FindNavPolyTreeNode(poly);
            if (tnode == null)
            {
                tnode = FindYnvTreeNode(poly?.Ynv);
            }
            if (tnode != null)
            {
                ProjectTreeView.SelectedNode = tnode;
            }
        }
        private void UpdateNavPolyTreeNode(YnvPoly poly)
        {
            var tn = FindNavPolyTreeNode(poly);
            if (tn != null)
            {
                tn.Text = poly._RawData.ToString();
            }
        }

        private void TrySelectTrainNodeTreeNode(TrainTrackNode node)
        {
            TreeNode tnode = FindTrainNodeTreeNode(node);
            if (tnode == null)
            {
                tnode = FindTrainTrackTreeNode(node?.Track);
            }
            if (tnode != null)
            {
                ProjectTreeView.SelectedNode = tnode;
            }
        }
        private void UpdateTrainNodeTreeNode(TrainTrackNode node)
        {
            var tn = FindTrainNodeTreeNode(node);
            if (tn != null)
            {
                tn.Text = node.ToString();
            }
        }

        private void TrySelectScenarioTreeNode(YmtFile scenario)
        {
            TreeNode tnode = FindScenarioTreeNode(scenario);
            if (tnode != null)
            {
                ProjectTreeView.SelectedNode = tnode;
            }
        }
        private void TrySelectScenarioNodeTreeNode(ScenarioNode node)
        {
            TreeNode tnode = FindScenarioNodeTreeNode(node);
            if (tnode == null)
            {
                tnode = FindScenarioTreeNode(node?.Ymt);
            }
            if (tnode != null)
            {
                ProjectTreeView.SelectedNode = tnode;
            }
        }
        private void UpdateScenarioNodeTreeNode(ScenarioNode node)
        {
            var tn = FindScenarioNodeTreeNode(node);
            if (tn != null)
            {
                tn.Text = node.MedTypeName + ": " + node.StringText;
            }
        }




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


        private void FileNewProjectMenu_Click(object sender, EventArgs e)
        {
            NewProject();
        }

        private void FileNewYmapMenu_Click(object sender, EventArgs e)
        {
            NewYmap();
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
            if (CurrentProjectFile != null)
            {
                ////unload current project first?
                //if (MessageBox.Show("Close the current project and open an existing one?", "Confirm close project", MessageBoxButtons.YesNo) != DialogResult.Yes)
                //{
                //    return;
                //}
                CloseProject();
            }

            OpenProject();
        }

        private void FileOpenYmapMenu_Click(object sender, EventArgs e)
        {
            OpenYmap();
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
            SaveProject(false);
        }

        private void FileSaveProjectAsMenu_Click(object sender, EventArgs e)
        {
            SaveProject(true);
        }

        private void FileSaveItemMenu_Click(object sender, EventArgs e)
        {
            SaveCurrentItem(false);
        }

        private void FileSaveItemAsMenu_Click(object sender, EventArgs e)
        {
            SaveCurrentItem(true);
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
            //AddYnvToProject(CurrentYnvFile);
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

        private void ToolsImportMenyooXmlMenu_Click(object sender, EventArgs e)
        {
            ImportMenyooXml();
        }

        private void OptionsHideGTAVMapMenu_Click(object sender, EventArgs e)
        {
            ProjectHideMapCheckBox.Checked = !hidegtavmap;
        }







        private void ProjectNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CurrentProjectFile != null)
            {
                if (CurrentProjectFile.Name != ProjectNameTextBox.Text)
                {
                    CurrentProjectFile.Name = ProjectNameTextBox.Text;
                    SetProjectHasChanged(true);
                }
            }
        }

        private void ProjectTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            YmapFile ymap = null;
            YmapEntityDef yent = null;
            YmapCarGen ycgen = null;
            YndFile ynd = null;
            YndNode ynode = null;
            YnvFile ynv = null;
            YnvPoly ypoly = null;
            TrainTrack tt = null;
            TrainTrackNode ttnode = null;
            YmtFile scenario = null;
            ScenarioNode scenarionode = null;
            object tag = null;
            var node = ProjectTreeView.SelectedNode;
            if (node != null)
            {
                tag = node.Tag;
                ymap = tag as YmapFile;
                yent = tag as YmapEntityDef;
                ycgen = tag as YmapCarGen;
                ynd = tag as YndFile;
                ynode = tag as YndNode;
                ynv = tag as YnvFile;
                ypoly = tag as YnvPoly;
                tt = tag as TrainTrack;
                ttnode = tag as TrainTrackNode;
                scenario = tag as YmtFile; if ((scenario != null) && (scenario.ScenarioRegion == null)) scenario = null;//incase other types of ymt files make it into the project...
                scenarionode = tag as ScenarioNode;
            }

            CurrentYmapFile = ymap;
            CurrentEntity = yent;
            CurrentCarGen = ycgen;
            CurrentYndFile = ynd;
            CurrentPathNode = ynode;
            CurrentYnvFile = ynv;
            CurrentNavPoly = ypoly;
            CurrentTrainTrack = tt;
            CurrentTrainNode = ttnode;
            CurrentScenario = scenario;
            CurrentScenarioNode = scenarionode;

            if (tag == CurrentProjectFile)
            {
                MainTabControl.SelectedTab = ProjectTabPage;
            }
            if (yent != null)
            {
                CurrentYmapFile = yent.Ymap;
                MainTabControl.SelectedTab = YmapTabPage;
                YmapTabControl.SelectedTab = YmapEntityTabPage;
            }
            else if (ycgen != null)
            {
                CurrentYmapFile = ycgen.Ymap;
                MainTabControl.SelectedTab = YmapTabPage;
                YmapTabControl.SelectedTab = YmapCarGenTabPage;
            }
            else if (ymap != null)
            {
                CurrentYmapFile = ymap;
                MainTabControl.SelectedTab = YmapTabPage;
                YmapTabControl.SelectedTab = YmapYmapTabPage;
            }
            if (ynode != null)
            {
                CurrentYndFile = ynode.Ynd;
                MainTabControl.SelectedTab = YndTabPage;
                YndTabControl.SelectedTab = YndNodeTabPage;
            }
            else if (ynd != null)
            {
                MainTabControl.SelectedTab = YndTabPage;
                YndTabControl.SelectedTab = YndYndTabPage;
            }
            if (ypoly != null)
            {
                CurrentYnvFile = ypoly.Ynv;
                MainTabControl.SelectedTab = YnvTabPage;
                YnvTabControl.SelectedTab = YnvPolyTabPage;
            }
            else if (ynv != null)
            {
                MainTabControl.SelectedTab = YnvTabPage;
                YnvTabControl.SelectedTab = YnvYnvTabPage;
            }
            if (ttnode != null)
            {
                CurrentTrainTrack = ttnode.Track;
                MainTabControl.SelectedTab = TrainsTabPage;
                TrainsTabControl.SelectedTab = TrainNodeTabPage;
            }
            else if (tt != null)
            {
                MainTabControl.SelectedTab = TrainsTabPage;
                TrainsTabControl.SelectedTab = TrainTrackTabPage;
            }
            if (scenarionode != null)
            {
                CurrentScenario = scenarionode.Ymt;
                MainTabControl.SelectedTab = ScenarioTabPage;
                SelectScenarioNodeTabPages(scenarionode);
            }
            else if (scenario != null)
            {
                MainTabControl.SelectedTab = ScenarioTabPage;
                ScenarioTabControl.SelectedTab = ScenarioYmtTabPage;
            }

            RefreshUI();
        }

        private void ProjectShowEntitiesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderentities = ProjectShowEntitiesCheckBox.Checked;
        }

        private void ProjectHideMapCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            hidegtavmap = ProjectHideMapCheckBox.Checked;
            OptionsHideGTAVMapMenu.Checked = hidegtavmap;
        }

        private void ProjectManifestGenerateButton_Click(object sender, EventArgs e)
        {
            GenerateProjectManifest();
        }






        private void YmapNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            uint hash = 0;
            string name = YmapNameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            YmapNameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentYmapFile != null)
            {
                lock (ymapsyncroot)
                {
                    string ymname = name + ".ymap";
                    if (CurrentYmapFile.Name != ymname)
                    {
                        CurrentYmapFile.Name = ymname;
                        CurrentYmapFile._CMapData.name = new MetaHash(hash);
                        SetYmapHasChanged(true);
                    }
                }
            }
        }

        private void YmapParentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            uint hash = 0;
            string name = YmapParentTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            YmapParentHashLabel.Text = "Hash: " + hash.ToString();

            if (hash != 0)
            {
                var entry = FindParentYmapEntry(hash);
                if (entry == null)
                {
                    YmapParentHashLabel.Text += " (not found!)";
                }
            }

            if (CurrentYmapFile != null)
            {
                lock (ymapsyncroot)
                {
                    if (CurrentYmapFile._CMapData.parent.Hash != hash)
                    {
                        CurrentYmapFile._CMapData.parent = new MetaHash(hash);
                        SetYmapHasChanged(true);

                        //TODO: confirm entity parent linkage?
                    }
                }
            }

        }

        private void YmapFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromTextBoxes();
        }

        private void YmapContentFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromTextBoxes();
        }

        private void YmapCFlagsHDCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsLODCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsSLOD2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsInteriorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsSLODCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsOcclusionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsPhysicsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsLODLightsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsDistLightsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsCriticalCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsGrassCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapFlagsScriptedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapFlagsLODCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCalculateFlagsButton_Click(object sender, EventArgs e)
        {
            CalcYmapFlags();
        }

        private void YmapEntitiesExtentsMinTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentYmapFile == null) return;
            Vector3 v = FloatUtil.ParseVector3String(YmapEntitiesExtentsMinTextBox.Text);
            lock (ymapsyncroot)
            {
                if (CurrentYmapFile._CMapData.entitiesExtentsMin != v)
                {
                    CurrentYmapFile._CMapData.entitiesExtentsMin = v;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void YmapEntitiesExtentsMaxTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentYmapFile == null) return;
            Vector3 v = FloatUtil.ParseVector3String(YmapEntitiesExtentsMaxTextBox.Text);
            lock (ymapsyncroot)
            {
                if (CurrentYmapFile._CMapData.entitiesExtentsMax != v)
                {
                    CurrentYmapFile._CMapData.entitiesExtentsMax = v;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void YmapStreamingExtentsMinTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentYmapFile == null) return;
            Vector3 v = FloatUtil.ParseVector3String(YmapStreamingExtentsMinTextBox.Text);
            lock (ymapsyncroot)
            {
                if (CurrentYmapFile._CMapData.streamingExtentsMin != v)
                {
                    CurrentYmapFile._CMapData.streamingExtentsMin = v;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void YmapStreamingExtentsMaxTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentYmapFile == null) return;
            Vector3 v = FloatUtil.ParseVector3String(YmapStreamingExtentsMaxTextBox.Text);
            lock (ymapsyncroot)
            {
                if (CurrentYmapFile._CMapData.streamingExtentsMax != v)
                {
                    CurrentYmapFile._CMapData.streamingExtentsMax = v;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void YmapCalculateExtentsButton_Click(object sender, EventArgs e)
        {
            CalcYmapExtents();
        }

        private void YmapPhysicsDictionariesTextBox_TextChanged(object sender, EventArgs e)
        {
            SetYmapPhysicsDictionariesFromTextbox();
        }


        private void EntityArchetypeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint hash = 0;
            string name = EntityArchetypeTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            EntityArchetypeHashLabel.Text = "Hash: " + hash.ToString();

            var arch = GameFileCache.GetArchetype(hash);
            if (arch == null)
            {
                EntityArchetypeHashLabel.Text += " (not found)";
            }

            TreeNode tn = FindEntityTreeNode(CurrentEntity);
            if (tn != null)
            {
                tn.Text = name;
            }

            if (CurrentEntity != null)
            {
                lock (ymapsyncroot)
                {
                    CurrentEntity._CEntityDef.archetypeName = new MetaHash(hash);
                    if (CurrentEntity.Archetype != arch)
                    {
                        CurrentEntity.SetArchetype(arch);
                        SetYmapHasChanged(true);
                    }
                }
            }
        }

        private void EntityFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint flags = 0;
            uint.TryParse(EntityFlagsTextBox.Text, out flags);
            populatingui = true;
            for (int i = 0; i < EntityFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((flags & (1u << i)) > 0);
                EntityFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.flags != flags)
                {
                    CurrentEntity._CEntityDef.flags = flags;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint flags = 0;
            for (int i = 0; i < EntityFlagsCheckedListBox.Items.Count; i++)
            {
                if (e.Index == i)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        flags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (EntityFlagsCheckedListBox.GetItemChecked(i))
                    {
                        flags += (uint)(1 << i);
                    }
                }
            }
            populatingui = true;
            EntityFlagsTextBox.Text = flags.ToString();
            populatingui = false;
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.flags != flags)
                {
                    CurrentEntity._CEntityDef.flags = flags;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityGuidTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint guid = 0;
            uint.TryParse(EntityGuidTextBox.Text, out guid);
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.guid != guid)
                {
                    CurrentEntity._CEntityDef.guid = guid;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Vector3 v = FloatUtil.ParseVector3String(EntityPositionTextBox.Text);
            lock (ymapsyncroot)
            {
                if (CurrentEntity.MloParent != null)
                {
                    //TODO: positioning for interior entities!
                }
                else
                {
                    if (CurrentEntity.Position != v)
                    {
                        CurrentEntity.SetPosition(v);
                        SetYmapHasChanged(true);
                        if (WorldForm != null)
                        {
                            WorldForm.BeginInvoke(new Action(() =>
                            {
                                WorldForm.SetWidgetPosition(CurrentEntity.WidgetPosition, true);
                            }));
                        }
                    }
                }
            }
        }

        private void EntityRotationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Vector4 v = FloatUtil.ParseVector4String(EntityRotationTextBox.Text);
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.rotation != v)
                {
                    Quaternion q = new Quaternion(v);
                    CurrentEntity.SetOrientationInv(q);
                    SetYmapHasChanged(true);
                    if (WorldForm != null)
                    {
                        WorldForm.BeginInvoke(new Action(() =>
                        {
                            WorldForm.SetWidgetRotation(CurrentEntity.WidgetOrientation, true);
                        }));
                    }
                }
            }
        }

        private void EntityScaleXYTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            float sxy = 0;
            FloatUtil.TryParse(EntityScaleXYTextBox.Text, out sxy);
            lock (ymapsyncroot)
            {
                if (CurrentEntity.Scale.X != sxy)
                {
                    Vector3 newscale = new Vector3(sxy, sxy, CurrentEntity.Scale.Z);
                    CurrentEntity.SetScale(newscale);
                    SetYmapHasChanged(true);
                    if (WorldForm != null)
                    {
                        WorldForm.BeginInvoke(new Action(() =>
                        {
                            WorldForm.SetWidgetScale(newscale, true);
                        }));
                    }
                }
            }
        }

        private void EntityScaleZTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            float sz = 0;
            FloatUtil.TryParse(EntityScaleZTextBox.Text, out sz);
            lock (ymapsyncroot)
            {
                if (CurrentEntity.Scale.Z != sz)
                {
                    Vector3 newscale = new Vector3(CurrentEntity.Scale.X, CurrentEntity.Scale.Y, sz);
                    CurrentEntity.SetScale(newscale);
                    SetYmapHasChanged(true);
                    if (WorldForm != null)
                    {
                        WorldForm.BeginInvoke(new Action(() =>
                        {
                            WorldForm.SetWidgetScale(newscale, true);
                        }));
                    }
                }
            }
        }

        private void EntityParentIndexTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            int pind = 0;
            int.TryParse(EntityParentIndexTextBox.Text, out pind);
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.parentIndex != pind)
                {
                    CurrentEntity._CEntityDef.parentIndex = pind; //Needs more work for LOD linking!
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityLodDistTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            float lodDist = 0;
            FloatUtil.TryParse(EntityLodDistTextBox.Text, out lodDist);
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.lodDist != lodDist)
                {
                    CurrentEntity._CEntityDef.lodDist = lodDist;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityChildLodDistTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            float childLodDist = 0;
            FloatUtil.TryParse(EntityChildLodDistTextBox.Text, out childLodDist);
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.childLodDist != childLodDist)
                {
                    CurrentEntity._CEntityDef.childLodDist = childLodDist;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityLodLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Unk_1264241711 lodLevel = (Unk_1264241711)EntityLodLevelComboBox.SelectedItem;
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.lodLevel != lodLevel)
                {
                    CurrentEntity._CEntityDef.lodLevel = lodLevel;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityNumChildrenTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint numChildren = 0;
            uint.TryParse(EntityNumChildrenTextBox.Text, out numChildren);
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.numChildren != numChildren)
                {
                    CurrentEntity._CEntityDef.numChildren = numChildren;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityPriorityLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Unk_648413703 priorityLevel = (Unk_648413703)EntityPriorityLevelComboBox.SelectedItem;
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.priorityLevel != priorityLevel)
                {
                    CurrentEntity._CEntityDef.priorityLevel = priorityLevel;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityAOMultiplierTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            int aomult = 0;
            int.TryParse(EntityAOMultiplierTextBox.Text, out aomult);
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.ambientOcclusionMultiplier != aomult)
                {
                    CurrentEntity._CEntityDef.ambientOcclusionMultiplier = aomult;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityArtificialAOTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            int artao = 0;
            int.TryParse(EntityArtificialAOTextBox.Text, out artao);
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.artificialAmbientOcclusion != artao)
                {
                    CurrentEntity._CEntityDef.artificialAmbientOcclusion = artao;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityTintValueTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint tintValue = 0;
            uint.TryParse(EntityTintValueTextBox.Text, out tintValue);
            lock (ymapsyncroot)
            {
                if (CurrentEntity._CEntityDef.tintValue != tintValue)
                {
                    CurrentEntity._CEntityDef.tintValue = tintValue;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void EntityGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentEntity == null) return;
            if (WorldForm == null) return;
            WorldForm.GoToPosition(CurrentEntity.Position);
        }

        private void EntityNormalizeRotationButton_Click(object sender, EventArgs e)
        {
            Vector4 v = FloatUtil.ParseVector4String(EntityRotationTextBox.Text);
            Quaternion q = Quaternion.Normalize(new Quaternion(v));
            EntityRotationTextBox.Text = FloatUtil.GetVector4String(new Vector4(q.X, q.Y, q.Z, q.W));
        }

        private void EntityAddToProjectButton_Click(object sender, EventArgs e)
        {
            AddEntityToProject();
        }

        private void EntityDeleteButton_Click(object sender, EventArgs e)
        {
            DeleteEntity();
        }

        private void EntityPivotEditCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (WorldForm != null)
            {
                WorldForm.EditEntityPivot = EntityPivotEditCheckBox.Checked;
            }
        }

        private void EntityPivotPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Vector3 v = FloatUtil.ParseVector3String(EntityPivotPositionTextBox.Text);
            lock (ymapsyncroot)
            {
                if (CurrentEntity.PivotPosition != v)
                {
                    CurrentEntity.SetPivotPosition(v);
                    //SetYmapHasChanged(true);
                    if (WorldForm != null)
                    {
                        WorldForm.BeginInvoke(new Action(() => 
                        {
                            bool editpivot = WorldForm.EditEntityPivot;
                            WorldForm.EditEntityPivot = true;
                            WorldForm.SetWidgetPosition(CurrentEntity.WidgetPosition, true);
                            WorldForm.EditEntityPivot = editpivot;
                        }));
                    }
                }
            }
        }

        private void EntityPivotRotationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Vector4 v = FloatUtil.ParseVector4String(EntityPivotRotationTextBox.Text);
            Quaternion q = new Quaternion(v);
            lock (ymapsyncroot)
            {
                if (CurrentEntity.PivotOrientation != q)
                {
                    CurrentEntity.SetPivotOrientation(q);
                    //SetYmapHasChanged(true);
                    if (WorldForm != null)
                    {
                        WorldForm.BeginInvoke(new Action(() =>
                        {
                            bool editpivot = WorldForm.EditEntityPivot;
                            WorldForm.EditEntityPivot = true;
                            WorldForm.SetWidgetRotation(CurrentEntity.WidgetOrientation, true);
                            WorldForm.EditEntityPivot = editpivot;
                        }));
                    }
                }
            }
        }

        private void EntityPivotRotationNormalizeButton_Click(object sender, EventArgs e)
        {
            Vector4 v = FloatUtil.ParseVector4String(EntityPivotRotationTextBox.Text);
            Quaternion q = Quaternion.Normalize(new Quaternion(v));
            EntityPivotRotationTextBox.Text = FloatUtil.GetVector4String(new Vector4(q.X, q.Y, q.Z, q.W));
        }


        private void CarModelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            uint hash = 0;
            string name = CarModelTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            CarModelHashLabel.Text = "Hash: " + hash.ToString();

            //var model = GameFileCache.GetCarInfo(hash); //todo: something like this for car info?
            //if (model == null)
            //{
            //    CarModelHashLabel.Text += " (not found)";
            //}

            if (CurrentCarGen != null)
            {
                lock (ymapsyncroot)
                {
                    var modelhash = new MetaHash(hash);
                    if (CurrentCarGen._CCarGen.carModel != modelhash)
                    {
                        CurrentCarGen._CCarGen.carModel = modelhash;
                        SetYmapHasChanged(true);
                    }
                }
            }

            UpdateCarGenTreeNode(CurrentCarGen);

        }

        private void CarPopGroupTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            uint hash = 0;
            string name = CarPopGroupTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            CarPopGroupHashLabel.Text = "Hash: " + hash.ToString();

            //var grp = GameFileCache.GetCarPopGroup(hash); //todo: something like this for popgroup info?
            //if (grp == null)
            //{
            //    CarPopGroupHashLabel.Text += " (not found)";
            //}

            if (CurrentCarGen != null)
            {
                lock (ymapsyncroot)
                {
                    var pghash = new MetaHash(hash);
                    if (CurrentCarGen._CCarGen.popGroup != pghash)
                    {
                        CurrentCarGen._CCarGen.popGroup = pghash;
                        SetYmapHasChanged(true);
                    }
                }
            }

            UpdateCarGenTreeNode(CurrentCarGen);
        }

        private void CarFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            uint flags = 0;
            uint.TryParse(CarFlagsTextBox.Text, out flags);
            populatingui = true;
            for (int i = 0; i < CarFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((flags & (1u << i)) > 0);
                CarFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            lock (ymapsyncroot)
            {
                if (CurrentCarGen._CCarGen.flags != flags)
                {
                    CurrentCarGen._CCarGen.flags = flags;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void CarFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            uint flags = 0;
            for (int i = 0; i < CarFlagsCheckedListBox.Items.Count; i++)
            {
                if (e.Index == i)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        flags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (CarFlagsCheckedListBox.GetItemChecked(i))
                    {
                        flags += (uint)(1 << i);
                    }
                }
            }
            populatingui = true;
            CarFlagsTextBox.Text = flags.ToString();
            populatingui = false;
            lock (ymapsyncroot)
            {
                if (CurrentCarGen._CCarGen.flags != flags)
                {
                    CurrentCarGen._CCarGen.flags = flags;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void CarPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            Vector3 v = FloatUtil.ParseVector3String(CarPositionTextBox.Text);
            lock (ymapsyncroot)
            {
                if (CurrentCarGen.Position != v)
                {
                    CurrentCarGen.SetPosition(v);
                    SetYmapHasChanged(true);
                    if (WorldForm != null)
                    {
                        WorldForm.SetWidgetPosition(v);
                    }
                }
            }

            UpdateCarGenTreeNode(CurrentCarGen);
        }

        private void CarOrientXTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            float ox = FloatUtil.Parse(CarOrientXTextBox.Text);
            lock (ymapsyncroot)
            {
                if (CurrentCarGen._CCarGen.orientX != ox)
                {
                    CurrentCarGen._CCarGen.orientX = ox;
                    CurrentCarGen.CalcOrientation();
                    SetYmapHasChanged(true);
                    if (WorldForm != null)
                    {
                        WorldForm.SetWidgetRotation(CurrentCarGen.Orientation);
                    }
                }
            }
        }

        private void CarOrientYTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            float oy = FloatUtil.Parse(CarOrientYTextBox.Text);
            lock (ymapsyncroot)
            {
                if (CurrentCarGen._CCarGen.orientY != oy)
                {
                    CurrentCarGen._CCarGen.orientY = oy;
                    CurrentCarGen.CalcOrientation();
                    SetYmapHasChanged(true);
                    if (WorldForm != null)
                    {
                        WorldForm.SetWidgetRotation(CurrentCarGen.Orientation);
                    }
                }
            }
        }

        private void CarPerpendicularLengthTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            float len = FloatUtil.Parse(CarPerpendicularLengthTextBox.Text);
            lock (ymapsyncroot)
            {
                if (CurrentCarGen._CCarGen.perpendicularLength != len)
                {
                    CurrentCarGen.SetLength(len);
                    SetYmapHasChanged(true);
                    if (WorldForm != null)
                    {
                        WorldForm.SetWidgetScale(new Vector3(len));
                    }
                }
            }
        }

        private void CarBodyColorRemap1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            int cr = 0;
            int.TryParse(CarBodyColorRemap1TextBox.Text, out cr);
            lock (ymapsyncroot)
            {
                if (CurrentCarGen._CCarGen.bodyColorRemap1 != cr)
                {
                    CurrentCarGen._CCarGen.bodyColorRemap1 = cr;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void CarBodyColorRemap2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            int cr = 0;
            int.TryParse(CarBodyColorRemap2TextBox.Text, out cr);
            lock (ymapsyncroot)
            {
                if (CurrentCarGen._CCarGen.bodyColorRemap2 != cr)
                {
                    CurrentCarGen._CCarGen.bodyColorRemap2 = cr;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void CarBodyColorRemap3TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            int cr = 0;
            int.TryParse(CarBodyColorRemap3TextBox.Text, out cr);
            lock (ymapsyncroot)
            {
                if (CurrentCarGen._CCarGen.bodyColorRemap3 != cr)
                {
                    CurrentCarGen._CCarGen.bodyColorRemap3 = cr;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void CarBodyColorRemap4TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            int cr = 0;
            int.TryParse(CarBodyColorRemap4TextBox.Text, out cr);
            lock (ymapsyncroot)
            {
                if (CurrentCarGen._CCarGen.bodyColorRemap4 != cr)
                {
                    CurrentCarGen._CCarGen.bodyColorRemap4 = cr;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void CarLiveryTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            sbyte cr = 0;
            sbyte.TryParse(CarLiveryTextBox.Text, out cr);
            lock (ymapsyncroot)
            {
                if (CurrentCarGen._CCarGen.livery != cr)
                {
                    CurrentCarGen._CCarGen.livery = cr;
                    SetYmapHasChanged(true);
                }
            }
            UpdateCarGenTreeNode(CurrentCarGen);
        }

        private void CarGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentCarGen == null) return;
            if (WorldForm == null) return;
            WorldForm.GoToPosition(CurrentCarGen.Position);
        }

        private void CarAddToProjectButton_Click(object sender, EventArgs e)
        {
            AddCarGenToProject();
        }

        private void CarDeleteButton_Click(object sender, EventArgs e)
        {
            DeleteCarGen();
        }









        private void YndAreaIDUpDownChange()
        {
            if (populatingui) return;
            if (CurrentYndFile == null) return;
            int x = (int)YndAreaIDXUpDown.Value;
            int y = (int)YndAreaIDYUpDown.Value;
            lock (yndsyncroot)
            {
                var areaid = y * 32 + x;
                if (CurrentYndFile.AreaID != areaid)
                {
                    CurrentYndFile.AreaID = areaid;
                    CurrentYndFile.Name = "nodes" + areaid.ToString() + ".ynd";
                    YndAreaIDInfoLabel.Text = "ID: " + areaid.ToString();
                    SetYndHasChanged(true);
                }
            }
        }

        private void YndAreaIDXUpDown_ValueChanged(object sender, EventArgs e)
        {
            YndAreaIDUpDownChange();
        }

        private void YndAreaIDYUpDown_ValueChanged(object sender, EventArgs e)
        {
            YndAreaIDUpDownChange();
        }

        private void YndVehicleNodesUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentYndFile == null) return;
            if (CurrentYndFile.NodeDictionary == null) return;
            lock (yndsyncroot)
            {
                var vehnodes = (int)YndVehicleNodesUpDown.Value;
                if (CurrentYndFile.NodeDictionary.NodesCountVehicle != vehnodes)
                {
                    CurrentYndFile.NodeDictionary.NodesCountVehicle = (uint)vehnodes;
                    SetYndHasChanged(true);
                }
            }
        }

        private void YndPedNodesUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentYndFile == null) return;
            if (CurrentYndFile.NodeDictionary == null) return;
            lock (yndsyncroot)
            {
                var pednodes = (int)YndPedNodesUpDown.Value;
                if (CurrentYndFile.NodeDictionary.NodesCountPed != pednodes)
                {
                    CurrentYndFile.NodeDictionary.NodesCountPed = (uint)pednodes;
                    SetYndHasChanged(true);
                }
            }
        }


        private void PathNodeAreaIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            ushort areaid = (ushort)PathNodeAreaIDUpDown.Value;
            lock (yndsyncroot)
            {
                if (CurrentPathNode.AreaID != areaid)
                {
                    CurrentPathNode.AreaID = areaid;
                    SetYndHasChanged(true);
                }
            }

            UpdatePathNodeTreeNode(CurrentPathNode);
        }

        private void PathNodeNodeIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            ushort nodeid = (ushort)PathNodeNodeIDUpDown.Value;
            lock (yndsyncroot)
            {
                if (CurrentPathNode.NodeID != nodeid)
                {
                    CurrentPathNode.NodeID = nodeid;
                    SetYndHasChanged(true);
                }
            }

            UpdatePathNodeTreeNode(CurrentPathNode);
        }

        private void PathNodePositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(PathNodePositionTextBox.Text);
            bool change = false;
            lock (yndsyncroot)
            {
                if (CurrentPathNode.Position != v)
                {
                    CurrentPathNode.SetPosition(v);
                    SetYndHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.SetWidgetPosition(CurrentPathNode.Position);
                    WorldForm.UpdatePathNodeGraphics(CurrentPathNode, false);
                }
                //PathNodePositionTextBox.Text = FloatUtil.GetVector3String(CurrentPathNode.Position);
            }
        }

        private void PathNodeGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentPathNode == null) return;
            if (WorldForm == null) return;
            WorldForm.GoToPosition(CurrentPathNode.Position);
        }

        private void PathNodeAddToProjectButton_Click(object sender, EventArgs e)
        {
            AddPathNodeToProject();
        }

        private void PathNodeDeleteButton_Click(object sender, EventArgs e)
        {
            DeletePathNode();
        }

        private void PathNodeStreetHashTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            uint hash;
            uint.TryParse(PathNodeStreetHashTextBox.Text, out hash);
            var streetname = GlobalText.TryGetString(hash);
            PathNodeStreetNameLabel.Text = "Name: " + ((hash == 0) ? "[None]" : (string.IsNullOrEmpty(streetname) ? "[Not found]" : streetname));

            lock (yndsyncroot)
            {
                if (CurrentPathNode.StreetName.Hash != hash)
                {
                    CurrentPathNode.StreetName = hash;
                    SetYndHasChanged(true);
                }
            }

            UpdatePathNodeTreeNode(CurrentPathNode);
        }

        private void PathNodeFlags0UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags1UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags2UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags3UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags4UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags5UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags01CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags02CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags03CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags04CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags05CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags06CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags07CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags08CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags11CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags12CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags13CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags14CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags15CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags16CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags17CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags18CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags21CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags22CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags23CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags24CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags25CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags26CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags27CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags28CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags31CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags32UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes(); //treat this one like checkboxes
        }

        private void PathNodeFlags51CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags41CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags45CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags46CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags47CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags48CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags42UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes(); //treat this one like checkboxes
        }

        private void PathNodeFlags52CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags53CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }


        private void PathNodeLinksListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentPathLink = PathNodeLinksListBox.SelectedItem as YndLink;
            LoadPathNodeLinkPage();
        }

        private void PathNodeAddLinkButton_Click(object sender, EventArgs e)
        {
            AddPathLink();
        }

        private void PathNodeRemoveLinkButton_Click(object sender, EventArgs e)
        {
            RemovePathLink();
        }

        private void PathNodeLinkAreaIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathLink == null) return;
            ushort areaid = (ushort)PathNodeLinkAreaIDUpDown.Value;
            bool change = false;
            lock (yndsyncroot)
            {
                if (CurrentPathLink._RawData.AreaID != areaid)
                {
                    CurrentPathLink._RawData.AreaID = areaid;
                    SetYndHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                UpdatePathNodeLinkage();

                //refresh the selected item in the list box, to update the text, and the other controls.
                PathNodeLinksListBox.Items[PathNodeLinksListBox.SelectedIndex] = PathNodeLinksListBox.SelectedItem;
            }
        }

        private void PathNodeLinkNodeIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathLink == null) return;
            ushort nodeid = (ushort)PathNodeLinkNodeIDUpDown.Value;
            bool change = false;
            lock (yndsyncroot)
            {
                if (CurrentPathLink._RawData.NodeID != nodeid)
                {
                    CurrentPathLink._RawData.NodeID = nodeid;
                    SetYndHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                UpdatePathNodeLinkage();

                //refresh the selected item in the list box, to update the text, and the other controls.
                PathNodeLinksListBox.Items[PathNodeLinksListBox.SelectedIndex] = PathNodeLinksListBox.SelectedItem;
            }
        }

        private void PathNodeLinkFlags0UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromUpDowns();
        }

        private void PathNodeLinkFlags1UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromUpDowns();
        }

        private void PathNodeLinkFlags2UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromUpDowns();
        }

        private void PathNodeLinkFlags01CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags02CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags03UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags04UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags11CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags12CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags13CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags14CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags18CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkOffsetSizeUpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags21CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags22CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFwdLanesUpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkBackLanesUpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkLengthUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathLink == null) return;
            byte length = (byte)PathNodeLinkLengthUpDown.Value;
            lock (yndsyncroot)
            {
                if (CurrentPathLink.LinkLength.Value != length)
                {
                    CurrentPathLink.LinkLength = length;
                    CurrentPathLink._RawData.LinkLength = length;
                    SetYndHasChanged(true);
                }
            }
        }

        private void PathNodeJunctionEnableCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            lock (yndsyncroot)
            {
                if (CurrentPathNode.HasJunction != PathNodeJunctionEnableCheckBox.Checked)
                {
                    CurrentPathNode.HasJunction = PathNodeJunctionEnableCheckBox.Checked;
                    if (CurrentPathNode.HasJunction && (CurrentPathNode.Junction == null))
                    {
                        var j = new YndJunction();
                        //init new junction
                        j._RawData.HeightmapDimX = 1;
                        j._RawData.HeightmapDimY = 1;
                        j.Heightmap = new YndJunctionHeightmap(new byte[] { 255 }, j);
                        j.RefData = new NodeJunctionRef() { AreaID = (ushort)CurrentPathNode.AreaID, NodeID = (ushort)CurrentPathNode.NodeID };

                        CurrentPathNode.Junction = j;
                    }
                    SetYndHasChanged(true);
                }
            }
            LoadPathNodeJunctionPage();
        }

        private void PathNodeJunctionMaxZUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            short val = (short)PathNodeJunctionMaxZUpDown.Value;
            lock (yndsyncroot)
            {
                if (CurrentPathNode.Junction.MaxZ != val)
                {
                    CurrentPathNode.Junction.MaxZ = val;
                    CurrentPathNode.Junction._RawData.MaxZ = val;
                    SetYndHasChanged(true);
                }
            }
        }

        private void PathNodeJunctionMinZUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            short val = (short)PathNodeJunctionMinZUpDown.Value;
            lock (yndsyncroot)
            {
                if (CurrentPathNode.Junction.MinZ != val)
                {
                    CurrentPathNode.Junction.MinZ = val;
                    CurrentPathNode.Junction._RawData.MinZ = val;
                    SetYndHasChanged(true);
                }
            }
        }

        private void PathNodeJunctionPosXUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            short val = (short)PathNodeJunctionPosXUpDown.Value;
            lock (yndsyncroot)
            {
                if (CurrentPathNode.Junction.PositionX != val)
                {
                    CurrentPathNode.Junction.PositionX = val;
                    CurrentPathNode.Junction._RawData.PositionX = val;
                    SetYndHasChanged(true);
                }
            }
        }

        private void PathNodeJunctionPosYUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            short val = (short)PathNodeJunctionPosYUpDown.Value;
            lock (yndsyncroot)
            {
                if (CurrentPathNode.Junction.PositionY != val)
                {
                    CurrentPathNode.Junction.PositionY = val;
                    CurrentPathNode.Junction._RawData.PositionY = val;
                    SetYndHasChanged(true);
                }
            }
        }

        private void PathNodeJunctionHeightmapDimXUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            byte val = (byte)PathNodeJunctionHeightmapDimXUpDown.Value;
            lock (yndsyncroot)
            {
                if (CurrentPathNode.Junction._RawData.HeightmapDimX != val)
                {
                    CurrentPathNode.Junction._RawData.HeightmapDimX = val;
                    CurrentPathNode.Junction.ResizeHeightmap();
                    SetYndHasChanged(true);
                }
            }
            LoadPathNodeJunctionPage();
        }

        private void PathNodeJunctionHeightmapDimYUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            byte val = (byte)PathNodeJunctionHeightmapDimYUpDown.Value;
            lock (yndsyncroot)
            {
                if (CurrentPathNode.Junction._RawData.HeightmapDimY != val)
                {
                    CurrentPathNode.Junction._RawData.HeightmapDimY = val;
                    CurrentPathNode.Junction.ResizeHeightmap();
                    SetYndHasChanged(true);
                }
            }
            LoadPathNodeJunctionPage();
        }

        private void PathNodeJunctionHeightmapBytesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            lock (yndsyncroot)
            {
                CurrentPathNode.Junction.SetHeightmap(PathNodeJunctionHeightmapBytesTextBox.Text);
                SetYndHasChanged(true);
            }
            //LoadPathNodeJunctionPage();
        }








        private void TrainNodePositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentTrainNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(TrainNodePositionTextBox.Text);
            bool change = false;
            lock (trainsyncroot)
            {
                if (CurrentTrainNode.Position != v)
                {
                    CurrentTrainNode.SetPosition(v);
                    SetTrainTrackHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.SetWidgetPosition(CurrentTrainNode.Position);
                    WorldForm.UpdateTrainTrackNodeGraphics(CurrentTrainNode, false);
                }
                //TrainNodePositionTextBox.Text = FloatUtil.GetVector3String(CurrentTrainNode.Position);
            }
        }

        private void TrainNodeGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentTrainNode == null) return;
            if (WorldForm == null) return;
            WorldForm.GoToPosition(CurrentTrainNode.Position);
        }

        private void TrainNodeTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentTrainNode == null) return;
            int type = TrainNodeTypeComboBox.SelectedIndex;
            bool change = false;
            lock (trainsyncroot)
            {
                if (CurrentTrainNode.NodeType != type)
                {
                    CurrentTrainNode.NodeType = type;
                    SetTrainTrackHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.UpdateTrainTrackNodeGraphics(CurrentTrainNode, false); //change the colour...
                }
            }
            UpdateTrainNodeTreeNode(CurrentTrainNode);
        }

        private void TrainNodeAddToProjectButton_Click(object sender, EventArgs e)
        {
            AddTrainNodeToProject();
        }

        private void TrainNodeDeleteButton_Click(object sender, EventArgs e)
        {
            DeleteTrainNode();
        }









        private void ScenarioYmtVersionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenario == null) return;
            if (CurrentScenario.CScenarioPointRegion == null) return;
            lock (scenariosyncroot)
            {
                int v = 0;
                int.TryParse(ScenarioYmtVersionTextBox.Text, out v);
                if (CurrentScenario.CScenarioPointRegion.VersionNumber != v)
                {
                    CurrentScenario.CScenarioPointRegion.VersionNumber = v;
                    SetScenarioHasChanged(true);
                }
            }
        }


        private void ScenarioPointCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ScenarioPointAddToProjectButton_Click(object sender, EventArgs e)
        {
            AddScenarioNodeToProject();
        }

        private void ScenarioPointDeleteButton_Click(object sender, EventArgs e)
        {
            DeleteScenarioNode();
        }

        private void ScenarioPointGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (WorldForm == null) return;
            WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioPointPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioPointPositionTextBox.Text);
            bool change = false;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioPointDirectionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            float dir = FloatUtil.Parse(ScenarioPointDirectionTextBox.Text);
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.Direction != dir)
                {
                    CurrentScenarioNode.MyPoint.Direction = dir;
                    CurrentScenarioNode.Orientation = CurrentScenarioNode.MyPoint.Orientation;
                    SetScenarioHasChanged(true);
                    if (WorldForm != null)
                    {
                        WorldForm.SetWidgetRotation(CurrentScenarioNode.Orientation);
                    }
                }
            }
        }

        private void ScenarioPointTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            ScenarioType stype = ScenarioPointTypeComboBox.SelectedItem as ScenarioType;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.Type != stype)
                {
                    CurrentScenarioNode.MyPoint.Type = stype;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);

            if (CurrentScenarioNode.ChainingNode != null)
            {
                ScenarioChainNodeTypeComboBox.SelectedItem = stype;
            }
        }

        private void ScenarioPointModelSetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            AmbientModelSet mset = ScenarioPointModelSetComboBox.SelectedItem as AmbientModelSet;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.ModelSet != mset)
                {
                    CurrentScenarioNode.MyPoint.ModelSet = mset;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioPointInteriorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            uint hash = 0;
            string name = ScenarioPointInteriorTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioPointInteriorHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.InteriorName != hash)
                {
                    CurrentScenarioNode.MyPoint.InteriorName = hash;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointGroupTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            uint hash = 0;
            string name = ScenarioPointGroupTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioPointGroupHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.GroupName != hash)
                {
                    CurrentScenarioNode.MyPoint.GroupName = hash;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointImapTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            uint hash = 0;
            string name = ScenarioPointImapTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioPointImapHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.IMapName != hash)
                {
                    CurrentScenarioNode.MyPoint.IMapName = hash;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointTimeStartUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte t = (byte)ScenarioPointTimeStartUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.TimeStart != t)
                {
                    CurrentScenarioNode.MyPoint.TimeStart = t;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioPointTimeEndUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte t = (byte)ScenarioPointTimeEndUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.TimeEnd != t)
                {
                    CurrentScenarioNode.MyPoint.TimeEnd = t;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioPointProbabilityUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte v = (byte)ScenarioPointProbabilityUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.Probability != v)
                {
                    CurrentScenarioNode.MyPoint.Probability = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointSpOnlyFlagUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte v = (byte)ScenarioPointSpOnlyFlagUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.AvailableMpSp != v)
                {
                    CurrentScenarioNode.MyPoint.AvailableMpSp = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointRadiusUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte v = (byte)ScenarioPointRadiusUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.Radius != v)
                {
                    CurrentScenarioNode.MyPoint.Radius = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointWaitTimeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte v = (byte)ScenarioPointWaitTimeUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.WaitTime != v)
                {
                    CurrentScenarioNode.MyPoint.WaitTime = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointFlagsValueUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            var iflags = (uint)ScenarioPointFlagsValueUpDown.Value;
            populatingui = true;
            for (int i = 0; i < ScenarioPointFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((iflags & (1 << i)) > 0);
                ScenarioPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.Flags != f)
                {
                    CurrentScenarioNode.MyPoint.Flags = f;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            uint iflags = 0;
            for (int i = 0; i < ScenarioPointFlagsCheckedListBox.Items.Count; i++)
            {
                if (e.Index == i)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        iflags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (ScenarioPointFlagsCheckedListBox.GetItemChecked(i))
                    {
                        iflags += (uint)(1 << i);
                    }
                }
            }
            populatingui = true;
            ScenarioPointFlagsValueUpDown.Value = iflags;
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.MyPoint.Flags != f)
                {
                    CurrentScenarioNode.MyPoint.Flags = f;
                    SetScenarioHasChanged(true);
                }
            }
        }


        private void ScenarioEntityCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ScenarioEntityAddToProjectButton_Click(object sender, EventArgs e)
        {
            AddScenarioNodeToProject();
        }

        private void ScenarioEntityDeleteButton_Click(object sender, EventArgs e)
        {
            DeleteScenarioEntity();
        }

        private void ScenarioEntityGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (WorldForm == null) return;
            WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioEntityPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Entity == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioEntityPositionTextBox.Text);
            bool change = false;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioEntityTypeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Entity == null) return;
            uint hash = 0;
            string name = ScenarioEntityTypeTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityTypeHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.Entity.TypeName != hash)
                {
                    CurrentScenarioNode.Entity.TypeName = hash;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioEntityUnk1UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Entity == null) return;
            byte v = (byte)ScenarioEntityUnk1UpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.Entity.Unk1 != v)
                {
                    CurrentScenarioNode.Entity.Unk1 = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityUnk2UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Entity == null) return;
            byte v = (byte)ScenarioEntityUnk2UpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.Entity.Unk2 != v)
                {
                    CurrentScenarioNode.Entity.Unk2 = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointsListBox_DoubleClick(object sender, EventArgs e)
        {
            var item = ScenarioEntityPointsListBox.SelectedItem as MCExtensionDefSpawnPoint;
            if (item == null) return;

            var nodes = CurrentScenario?.ScenarioRegion?.Nodes;
            if (nodes == null) return;

            ScenarioNode node = null;
            foreach (var snode in nodes)
            {
                if (snode.EntityPoint == item)
                {
                    node = snode;
                    break;
                }
            }

            if (node == null) return;

            TrySelectScenarioNodeTreeNode(node);

        }

        private void ScenarioEntityAddPointButton_Click(object sender, EventArgs e)
        {
            AddScenarioEntityPoint();
        }


        private void ScenarioEntityPointCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ScenarioEntityPointAddToProjectButton_Click(object sender, EventArgs e)
        {
            AddScenarioNodeToProject();
        }

        private void ScenarioEntityPointDeleteButton_Click(object sender, EventArgs e)
        {
            DeleteScenarioNode();
        }

        private void ScenarioEntityPointGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (WorldForm == null) return;
            WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioEntityPointPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioEntityPointPositionTextBox.Text);
            v += CurrentScenarioNode.EntityPoint.ParentPosition;
            bool change = false;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioEntityPointRotationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            Vector4 v = FloatUtil.ParseVector4String(ScenarioEntityPointRotationTextBox.Text);
            bool change = false;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.OffsetRotation != v)
                {
                    CurrentScenarioNode.EntityPoint.OffsetRotation = v;
                    CurrentScenarioNode.Orientation = new Quaternion(v);
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.SetWidgetRotation(CurrentScenarioNode.Orientation);
                }
            }
        }

        private void ScenarioEntityPointNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointNameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointNameHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.NameHash != hash)
                {
                    CurrentScenarioNode.EntityPoint.NameHash = hash;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioEntityPointSpawnTypeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointSpawnTypeTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointSpawnTypeHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.SpawnType != hash)
                {
                    CurrentScenarioNode.EntityPoint.SpawnType = hash;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioEntityPointPedTypeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointPedTypeTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointPedTypeHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.PedType != hash)
                {
                    CurrentScenarioNode.EntityPoint.PedType = hash;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioEntityPointGroupTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointGroupTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointGroupHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.Group != hash)
                {
                    CurrentScenarioNode.EntityPoint.Group = hash;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointInteriorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointInteriorTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointInteriorHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.Interior != hash)
                {
                    CurrentScenarioNode.EntityPoint.Interior = hash;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointRequiredImapTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointRequiredImapTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointRequiredImapHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.RequiredImap != hash)
                {
                    CurrentScenarioNode.EntityPoint.RequiredImap = hash;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointAvailableInMpSpComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            Unk_3573596290 v = (Unk_3573596290)ScenarioEntityPointAvailableInMpSpComboBox.SelectedItem;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.AvailableInMpSp != v)
                {
                    CurrentScenarioNode.EntityPoint.AvailableInMpSp = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointProbabilityTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            float v = FloatUtil.Parse(ScenarioEntityPointProbabilityTextBox.Text);
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.Probability != v)
                {
                    CurrentScenarioNode.EntityPoint.Probability = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointTimeTillPedLeavesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            float v = FloatUtil.Parse(ScenarioEntityPointTimeTillPedLeavesTextBox.Text);
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.TimeTillPedLeaves != v)
                {
                    CurrentScenarioNode.EntityPoint.TimeTillPedLeaves = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            float v = FloatUtil.Parse(ScenarioEntityPointRadiusTextBox.Text);
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.Radius != v)
                {
                    CurrentScenarioNode.EntityPoint.Radius = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointStartUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            byte v = (byte)ScenarioEntityPointStartUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.StartTime != v)
                {
                    CurrentScenarioNode.EntityPoint.StartTime = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointEndUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            byte v = (byte)ScenarioEntityPointEndUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.EndTime != v)
                {
                    CurrentScenarioNode.EntityPoint.EndTime = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointExtendedRangeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            bool v = ScenarioEntityPointExtendedRangeCheckBox.Checked;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.ExtendedRange != v)
                {
                    CurrentScenarioNode.EntityPoint.ExtendedRange = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointShortRangeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            bool v = ScenarioEntityPointShortRangeCheckBox.Checked;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.ShortRange != v)
                {
                    CurrentScenarioNode.EntityPoint.ShortRange = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointHighPriCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            bool v = ScenarioEntityPointHighPriCheckBox.Checked;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.HighPri != v)
                {
                    CurrentScenarioNode.EntityPoint.HighPri = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointFlagsUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            var iflags = (uint)ScenarioEntityPointFlagsUpDown.Value;
            populatingui = true;
            for (int i = 0; i < ScenarioEntityPointFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((iflags & (1 << i)) > 0);
                ScenarioEntityPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.Flags != f)
                {
                    CurrentScenarioNode.EntityPoint.Flags = f;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint iflags = 0;
            for (int i = 0; i < ScenarioEntityPointFlagsCheckedListBox.Items.Count; i++)
            {
                if (e.Index == i)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        iflags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (ScenarioEntityPointFlagsCheckedListBox.GetItemChecked(i))
                    {
                        iflags += (uint)(1 << i);
                    }
                }
            }
            populatingui = true;
            ScenarioEntityPointFlagsUpDown.Value = iflags;
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.EntityPoint.Flags != f)
                {
                    CurrentScenarioNode.EntityPoint.Flags = f;
                    SetScenarioHasChanged(true);
                }
            }
        }


        private void ScenarioChainNodeCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ScenarioChainNodeAddToProjectButton_Click(object sender, EventArgs e)
        {
            AddScenarioNodeToProject();
        }

        private void ScenarioChainNodeDeleteButton_Click(object sender, EventArgs e)
        {
            DeleteScenarioNode();
        }

        private void ScenarioChainNodeGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (WorldForm == null) return;
            WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioChainNodePositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioChainNodePositionTextBox.Text);
            bool change = false;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioChainNodeUnk1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ChainingNode == null) return;
            uint hash = 0;
            string name = ScenarioChainNodeUnk1TextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioChainNodeUnk1HashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ChainingNode.Unk1 != hash)
                {
                    CurrentScenarioNode.ChainingNode.Unk1 = hash;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioChainNodeTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ChainingNode == null) return;
            ScenarioType stype = ScenarioChainNodeTypeComboBox.SelectedItem as ScenarioType;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ChainingNode.Type != stype)
                {
                    CurrentScenarioNode.ChainingNode.Type = stype;
                    CurrentScenarioNode.ChainingNode.TypeHash = stype?.NameHash ?? 0;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioChainNodeFirstCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ChainingNode == null) return;
            bool v = !ScenarioChainNodeFirstCheckBox.Checked;
            bool change = false;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ChainingNode.NotFirst != v)
                {
                    CurrentScenarioNode.ChainingNode.NotFirst = v;
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioChainNodeLastCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ChainingNode == null) return;
            bool v = !ScenarioChainNodeLastCheckBox.Checked;
            bool change = false;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ChainingNode.NotLast != v)
                {
                    CurrentScenarioNode.ChainingNode.NotLast = v;
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }


        private void ScenarioChainAddToProjectButton_Click(object sender, EventArgs e)
        {
            AddScenarioNodeToProject();
        }

        private void ScenarioChainDeleteButton_Click(object sender, EventArgs e)
        {
            DeleteScenarioChain();
        }

        private void ScenarioChainEdgesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            CurrentScenarioChainEdge = ScenarioChainEdgesListBox.SelectedItem as MCScenarioChainingEdge;
            populatingui = true;
            LoadScenarioChainEdgeTabPage();
            populatingui = false;
        }

        private void ScenarioChainAddEdgeButton_Click(object sender, EventArgs e)
        {
            AddScenarioEdge();
        }

        private void ScenarioChainRemoveEdgeButton_Click(object sender, EventArgs e)
        {
            RemoveScenarioEdge();
        }

        private void ScenarioChainMoveEdgeUpButton_Click(object sender, EventArgs e)
        {
            MoveScenarioEdge(false);
        }

        private void ScenarioChainMoveEdgeDownButton_Click(object sender, EventArgs e)
        {
            MoveScenarioEdge(true);
        }

        private void ScenarioChainUnk1UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ChainingNode == null) return;
            if (CurrentScenarioNode.ChainingNode.Chain == null) return;
            byte v = (byte)ScenarioChainUnk1UpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ChainingNode.Chain.Unk1 != v)
                {
                    CurrentScenarioNode.ChainingNode.Chain.Unk1 = v;
                    SetScenarioHasChanged(true);
                }
            }
        }


        private void ScenarioChainEdgeNodeIndexFromUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioChainEdge == null) return;
            ushort nodeid = (ushort)ScenarioChainEdgeNodeIndexFromUpDown.Value;
            bool change = false;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioChainEdge.NodeIndexFrom != nodeid)
                {
                    CurrentScenarioChainEdge.NodeIndexFrom = nodeid;
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                UpdateScenarioEdgeLinkage();

                //refresh the selected item in the list box, to update the text, and the other controls.
                ScenarioChainEdgesListBox.Items[ScenarioChainEdgesListBox.SelectedIndex] = ScenarioChainEdgesListBox.SelectedItem;
            }
        }

        private void ScenarioChainEdgeNodeIndexToUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioChainEdge == null) return;
            ushort nodeid = (ushort)ScenarioChainEdgeNodeIndexToUpDown.Value;
            bool change = false;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioChainEdge.NodeIndexTo != nodeid)
                {
                    CurrentScenarioChainEdge.NodeIndexTo = nodeid;
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                UpdateScenarioEdgeLinkage();

                //refresh the selected item in the list box, to update the text, and the other controls.
                ScenarioChainEdgesListBox.Items[ScenarioChainEdgesListBox.SelectedIndex] = ScenarioChainEdgesListBox.SelectedItem;
            }
        }

        private void ScenarioChainEdgeActionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioChainEdge == null) return;
            Unk_3609807418 v = (Unk_3609807418)ScenarioChainEdgeActionComboBox.SelectedItem;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioChainEdge.Action != v)
                {
                    CurrentScenarioChainEdge.Action = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioChainEdgeNavModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioChainEdge == null) return;
            Unk_3971773454 v = (Unk_3971773454)ScenarioChainEdgeNavModeComboBox.SelectedItem;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioChainEdge.NavMode != v)
                {
                    CurrentScenarioChainEdge.NavMode = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioChainEdgeNavSpeedComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioChainEdge == null) return;
            Unk_941086046 v = (Unk_941086046)ScenarioChainEdgeNavSpeedComboBox.SelectedItem;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioChainEdge.NavSpeed != v)
                {
                    CurrentScenarioChainEdge.NavSpeed = v;
                    SetScenarioHasChanged(true);
                }
            }
        }


        private void ScenarioClusterCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ScenarioClusterAddToProjectButton_Click(object sender, EventArgs e)
        {
            AddScenarioNodeToProject();
        }

        private void ScenarioClusterDeleteButton_Click(object sender, EventArgs e)
        {
            DeleteScenarioCluster();
        }

        private void ScenarioClusterGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (WorldForm == null) return;
            WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioClusterCenterTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioClusterCenterTextBox.Text);
            bool change = false;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    SetScenarioHasChanged(true);
                    change = true;
                }
                if ((CurrentScenarioNode.Cluster != null) && (CurrentScenarioNode.Cluster.Position != v))
                {
                    CurrentScenarioNode.Cluster.Position = v;
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioClusterRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            float r = FloatUtil.Parse(ScenarioClusterRadiusTextBox.Text);
            bool change = false;
            lock (scenariosyncroot)
            {
                if ((CurrentScenarioNode.Cluster != null) && (CurrentScenarioNode.Cluster.Radius != r))
                {
                    CurrentScenarioNode.Cluster.Radius = r;
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioClusterUnk1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Cluster == null) return;
            float v = FloatUtil.Parse(ScenarioClusterUnk1TextBox.Text);
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.Cluster.Unk1 != v)
                {
                    CurrentScenarioNode.Cluster.Unk1 = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterUnk2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Cluster == null) return;
            bool v = ScenarioClusterUnk2CheckBox.Checked;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.Cluster.Unk2 != v)
                {
                    CurrentScenarioNode.Cluster.Unk2 = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointsListBox_DoubleClick(object sender, EventArgs e)
        {
            var item = ScenarioClusterPointsListBox.SelectedItem as MCScenarioPoint;
            if (item == null) return;

            var nodes = CurrentScenario?.ScenarioRegion?.Nodes;
            if (nodes == null) return;

            ScenarioNode node = null;
            foreach (var snode in nodes)
            {
                if (snode.ClusterMyPoint == item)
                {
                    node = snode;
                    break;
                }
            }

            if (node == null) return;

            TrySelectScenarioNodeTreeNode(node);

        }

        private void ScenarioClusterAddPointButton_Click(object sender, EventArgs e)
        {
            AddScenarioClusterPoint();
        }


        private void ScenarioClusterPointCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ScenarioClusterPointAddToProjectButton_Click(object sender, EventArgs e)
        {
            AddScenarioNodeToProject();
        }

        private void ScenarioClusterPointDeleteButton_Click(object sender, EventArgs e)
        {
            DeleteScenarioNode();
        }

        private void ScenarioClusterPointGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (WorldForm == null) return;
            WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioClusterPointPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioClusterPointPositionTextBox.Text);
            bool change = false;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (WorldForm != null)
                {
                    WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioClusterPointDirectionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            float dir = FloatUtil.Parse(ScenarioClusterPointDirectionTextBox.Text);
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Direction != dir)
                {
                    CurrentScenarioNode.ClusterMyPoint.Direction = dir;
                    CurrentScenarioNode.Orientation = CurrentScenarioNode.ClusterMyPoint.Orientation;
                    SetScenarioHasChanged(true);
                    if (WorldForm != null)
                    {
                        WorldForm.SetWidgetRotation(CurrentScenarioNode.Orientation);
                    }
                }
            }
        }

        private void ScenarioClusterPointTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            ScenarioType stype = ScenarioClusterPointTypeComboBox.SelectedItem as ScenarioType;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Type != stype)
                {
                    CurrentScenarioNode.ClusterMyPoint.Type = stype;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);

            if (CurrentScenarioNode.ChainingNode != null)
            {
                ScenarioChainNodeTypeComboBox.SelectedItem = stype;
            }
        }

        private void ScenarioClusterPointModelSetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            AmbientModelSet mset = ScenarioClusterPointModelSetComboBox.SelectedItem as AmbientModelSet;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.ModelSet != mset)
                {
                    CurrentScenarioNode.ClusterMyPoint.ModelSet = mset;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioClusterPointInteriorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            uint hash = 0;
            string name = ScenarioClusterPointInteriorTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioClusterPointInteriorHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.InteriorName != hash)
                {
                    CurrentScenarioNode.ClusterMyPoint.InteriorName = hash;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointGroupTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            uint hash = 0;
            string name = ScenarioClusterPointGroupTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioClusterPointGroupHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.GroupName != hash)
                {
                    CurrentScenarioNode.ClusterMyPoint.GroupName = hash;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointImapTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            uint hash = 0;
            string name = ScenarioClusterPointImapTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioClusterPointImapHashLabel.Text = "Hash: " + hash.ToString();
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.IMapName != hash)
                {
                    CurrentScenarioNode.ClusterMyPoint.IMapName = hash;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointTimeStartUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte t = (byte)ScenarioClusterPointTimeStartUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.TimeStart != t)
                {
                    CurrentScenarioNode.ClusterMyPoint.TimeStart = t;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioClusterPointTimeEndUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte t = (byte)ScenarioClusterPointTimeEndUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.TimeEnd != t)
                {
                    CurrentScenarioNode.ClusterMyPoint.TimeEnd = t;
                    SetScenarioHasChanged(true);
                }
            }
            UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioClusterPointProbabilityUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte v = (byte)ScenarioClusterPointProbabilityUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Probability != v)
                {
                    CurrentScenarioNode.ClusterMyPoint.Probability = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointAnimalFlagUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte v = (byte)ScenarioClusterPointAnimalFlagUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.AvailableMpSp != v)
                {
                    CurrentScenarioNode.ClusterMyPoint.AvailableMpSp = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointRadiusUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte v = (byte)ScenarioClusterPointRadiusUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Radius != v)
                {
                    CurrentScenarioNode.ClusterMyPoint.Radius = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointWaitTimeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte v = (byte)ScenarioClusterPointWaitTimeUpDown.Value;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.WaitTime != v)
                {
                    CurrentScenarioNode.ClusterMyPoint.WaitTime = v;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointFlagsUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            var iflags = (uint)ScenarioClusterPointFlagsUpDown.Value;
            populatingui = true;
            for (int i = 0; i < ScenarioClusterPointFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((iflags & (1 << i)) > 0);
                ScenarioClusterPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Flags != f)
                {
                    CurrentScenarioNode.ClusterMyPoint.Flags = f;
                    SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            uint iflags = 0;
            for (int i = 0; i < ScenarioClusterPointFlagsCheckedListBox.Items.Count; i++)
            {
                if (e.Index == i)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        iflags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (ScenarioClusterPointFlagsCheckedListBox.GetItemChecked(i))
                    {
                        iflags += (uint)(1 << i);
                    }
                }
            }
            populatingui = true;
            ScenarioClusterPointFlagsUpDown.Value = iflags;
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (scenariosyncroot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Flags != f)
                {
                    CurrentScenarioNode.ClusterMyPoint.Flags = f;
                    SetScenarioHasChanged(true);
                }
            }
        }
    }
}
