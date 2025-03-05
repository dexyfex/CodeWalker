using CodeWalker.WinForms;

namespace CodeWalker
{
    partial class WorldForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorldForm));
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MousedLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ModelComboBox = new System.Windows.Forms.ComboBox();
            this.ToolsPanel = new System.Windows.Forms.Panel();
            this.ToolsDragPanel = new System.Windows.Forms.Panel();
            this.AboutButton = new System.Windows.Forms.Button();
            this.ToolsButton = new System.Windows.Forms.Button();
            this.ToolsPanelExpandButton = new System.Windows.Forms.Button();
            this.ToolsTabControl = new System.Windows.Forms.TabControl();
            this.ViewTabPage = new System.Windows.Forms.TabPage();
            this.StatusBarCheckBox = new System.Windows.Forms.CheckBox();
            this.ViewTabControl = new System.Windows.Forms.TabControl();
            this.ViewWorldTabPage = new System.Windows.Forms.TabPage();
            this.EnableModsCheckBox = new System.Windows.Forms.CheckBox();
            this.label30 = new System.Windows.Forms.Label();
            this.DlcLevelComboBox = new System.Windows.Forms.ComboBox();
            this.EnableDlcCheckBox = new System.Windows.Forms.CheckBox();
            this.WorldYmapWeatherFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.WorldYmapTimeFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.WorldScriptedYmapsCheckBox = new System.Windows.Forms.CheckBox();
            this.WorldDetailDistLabel = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.WorldDetailDistTrackBar = new System.Windows.Forms.TrackBar();
            this.WorldLodDistLabel = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.WorldLodDistTrackBar = new System.Windows.Forms.TrackBar();
            this.label15 = new System.Windows.Forms.Label();
            this.WorldMaxLodComboBox = new System.Windows.Forms.ComboBox();
            this.ViewYmapsTabPage = new System.Windows.Forms.TabPage();
            this.ShowYmapChildrenCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.DetailTrackBar = new System.Windows.Forms.TrackBar();
            this.DynamicLODCheckBox = new System.Windows.Forms.CheckBox();
            this.YmapsTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.ViewModelTabPage = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ViewModeComboBox = new System.Windows.Forms.ComboBox();
            this.ShowToolbarCheckBox = new System.Windows.Forms.CheckBox();
            this.ErrorConsoleCheckBox = new System.Windows.Forms.CheckBox();
            this.MarkersTabPage = new System.Windows.Forms.TabPage();
            this.label27 = new System.Windows.Forms.Label();
            this.CameraPositionTextBox = new System.Windows.Forms.TextBox();
            this.AddSelectionMarkerButton = new System.Windows.Forms.Button();
            this.AddCurrentPositonMarkerButton = new System.Windows.Forms.Button();
            this.ResetMarkersButton = new System.Windows.Forms.Button();
            this.ClearMarkersButton = new System.Windows.Forms.Button();
            this.GoToButton = new System.Windows.Forms.Button();
            this.ShowLocatorCheckBox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.LocateTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.AddMarkersButton = new System.Windows.Forms.Button();
            this.MultiFindTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.SelectionTabPage = new System.Windows.Forms.TabPage();
            this.label25 = new System.Windows.Forms.Label();
            this.SelectionModeComboBox = new System.Windows.Forms.ComboBox();
            this.SelectionNameTextBox = new System.Windows.Forms.TextBox();
            this.SelectionTabControl = new System.Windows.Forms.TabControl();
            this.SelectionEntityTabPage = new System.Windows.Forms.TabPage();
            this.SelEntityPropertyGrid = new CodeWalker.WinForms.ReadOnlyPropertyGrid();
            this.SelectionArchetypeTabPage = new System.Windows.Forms.TabPage();
            this.SelArchetypePropertyGrid = new CodeWalker.WinForms.ReadOnlyPropertyGrid();
            this.SelectionDrawableTabPage = new System.Windows.Forms.TabPage();
            this.tabControl3 = new System.Windows.Forms.TabControl();
            this.tabPage11 = new System.Windows.Forms.TabPage();
            this.SelDrawablePropertyGrid = new CodeWalker.WinForms.ReadOnlyPropertyGrid();
            this.tabPage12 = new System.Windows.Forms.TabPage();
            this.SelDrawableModelsTreeView = new CodeWalker.WinForms.TreeViewFix();
            this.tabPage13 = new System.Windows.Forms.TabPage();
            this.SelDrawableTexturesTreeView = new CodeWalker.WinForms.TreeViewFix();
            this.SelectionExtensionTabPage = new System.Windows.Forms.TabPage();
            this.SelExtensionPropertyGrid = new CodeWalker.WinForms.ReadOnlyPropertyGrid();
            this.MouseSelectCheckBox = new System.Windows.Forms.CheckBox();
            this.OptionsTabPage = new System.Windows.Forms.TabPage();
            this.OptionsTabControl = new System.Windows.Forms.TabControl();
            this.OptionsGeneralTabPage = new System.Windows.Forms.TabPage();
            this.SaveTimeOfDayCheckBox = new System.Windows.Forms.CheckBox();
            this.SavePositionCheckBox = new System.Windows.Forms.CheckBox();
            this.ResetSettingsButton = new System.Windows.Forms.Button();
            this.CarGeneratorsCheckBox = new System.Windows.Forms.CheckBox();
            this.RenderEntitiesCheckBox = new System.Windows.Forms.CheckBox();
            this.AdvancedSettingsButton = new System.Windows.Forms.Button();
            this.SaveSettingsButton = new System.Windows.Forms.Button();
            this.ControlSettingsButton = new System.Windows.Forms.Button();
            this.MapViewDetailLabel = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.MapViewDetailTrackBar = new System.Windows.Forms.TrackBar();
            this.CameraModeComboBox = new System.Windows.Forms.ComboBox();
            this.label24 = new System.Windows.Forms.Label();
            this.WaterQuadsCheckBox = new System.Windows.Forms.CheckBox();
            this.FieldOfViewLabel = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.TimedEntitiesAlwaysOnCheckBox = new System.Windows.Forms.CheckBox();
            this.GrassCheckBox = new System.Windows.Forms.CheckBox();
            this.InteriorsCheckBox = new System.Windows.Forms.CheckBox();
            this.CollisionMeshLayerDrawableCheckBox = new System.Windows.Forms.CheckBox();
            this.CollisionMeshLayer2CheckBox = new System.Windows.Forms.CheckBox();
            this.CollisionMeshLayer1CheckBox = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.CollisionMeshLayer0CheckBox = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.CollisionMeshRangeTrackBar = new System.Windows.Forms.TrackBar();
            this.CollisionMeshesCheckBox = new System.Windows.Forms.CheckBox();
            this.FullScreenCheckBox = new System.Windows.Forms.CheckBox();
            this.TimedEntitiesCheckBox = new System.Windows.Forms.CheckBox();
            this.FieldOfViewTrackBar = new System.Windows.Forms.TrackBar();
            this.OptionsRenderTabPage = new System.Windows.Forms.TabPage();
            this.FarClipUpDown = new System.Windows.Forms.NumericUpDown();
            this.label32 = new System.Windows.Forms.Label();
            this.NearClipUpDown = new System.Windows.Forms.NumericUpDown();
            this.label31 = new System.Windows.Forms.Label();
            this.HDTexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.WireframeCheckBox = new System.Windows.Forms.CheckBox();
            this.RenderModeComboBox = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.TextureSamplerComboBox = new System.Windows.Forms.ComboBox();
            this.TextureCoordsComboBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.AnisotropicFilteringCheckBox = new System.Windows.Forms.CheckBox();
            this.ProxiesCheckBox = new System.Windows.Forms.CheckBox();
            this.WaitForChildrenCheckBox = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.OptionsHelpersTabPage = new System.Windows.Forms.TabPage();
            this.SnapAngleUpDown = new System.Windows.Forms.NumericUpDown();
            this.label33 = new System.Windows.Forms.Label();
            this.SnapGridSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label26 = new System.Windows.Forms.Label();
            this.SkeletonsCheckBox = new System.Windows.Forms.CheckBox();
            this.AudioOuterBoundsCheckBox = new System.Windows.Forms.CheckBox();
            this.PopZonesCheckBox = new System.Windows.Forms.CheckBox();
            this.NavMeshesCheckBox = new System.Windows.Forms.CheckBox();
            this.TrainPathsCheckBox = new System.Windows.Forms.CheckBox();
            this.PathsDepthClipCheckBox = new System.Windows.Forms.CheckBox();
            this.PathBoundsCheckBox = new System.Windows.Forms.CheckBox();
            this.SelectionWidgetCheckBox = new System.Windows.Forms.CheckBox();
            this.MarkerStyleComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.LocatorStyleComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.MarkerDepthClipCheckBox = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.PathsCheckBox = new System.Windows.Forms.CheckBox();
            this.SelectionBoundsCheckBox = new System.Windows.Forms.CheckBox();
            this.BoundsDepthClipCheckBox = new System.Windows.Forms.CheckBox();
            this.BoundsRangeTrackBar = new System.Windows.Forms.TrackBar();
            this.BoundsStyleComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.OptionsLightingTabPage = new System.Windows.Forms.TabPage();
            this.HDLightsCheckBox = new System.Windows.Forms.CheckBox();
            this.DeferredShadingCheckBox = new System.Windows.Forms.CheckBox();
            this.WeatherRegionComboBox = new System.Windows.Forms.ComboBox();
            this.label29 = new System.Windows.Forms.Label();
            this.CloudParamTrackBar = new System.Windows.Forms.TrackBar();
            this.CloudParamComboBox = new System.Windows.Forms.ComboBox();
            this.label23 = new System.Windows.Forms.Label();
            this.CloudsComboBox = new System.Windows.Forms.ComboBox();
            this.label21 = new System.Windows.Forms.Label();
            this.TimeSpeedLabel = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.TimeSpeedTrackBar = new System.Windows.Forms.TrackBar();
            this.TimeStartStopButton = new System.Windows.Forms.Button();
            this.ArtificialAmbientLightCheckBox = new System.Windows.Forms.CheckBox();
            this.NaturalAmbientLightCheckBox = new System.Windows.Forms.CheckBox();
            this.LODLightsCheckBox = new System.Windows.Forms.CheckBox();
            this.HDRRenderingCheckBox = new System.Windows.Forms.CheckBox();
            this.ControlTimeOfDayCheckBox = new System.Windows.Forms.CheckBox();
            this.TimeOfDayLabel = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.TimeOfDayTrackBar = new System.Windows.Forms.TrackBar();
            this.WeatherComboBox = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.ControlLightDirectionCheckBox = new System.Windows.Forms.CheckBox();
            this.SkydomeCheckBox = new System.Windows.Forms.CheckBox();
            this.ShadowsCheckBox = new System.Windows.Forms.CheckBox();
            this.ToolsPanelHideButton = new System.Windows.Forms.Button();
            this.ToolsPanelShowButton = new System.Windows.Forms.Button();
            this.ConsolePanel = new System.Windows.Forms.Panel();
            this.ConsoleTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.StatsUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.SelectedMarkerPanel = new System.Windows.Forms.Panel();
            this.SelectedMarkerPositionTextBox = new System.Windows.Forms.TextBox();
            this.SelectedMarkerNameTextBox = new System.Windows.Forms.TextBox();
            this.ToolsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolsMenuConfigureGame = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuRPFBrowser = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuRPFExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuSelectionInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuProjectWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuCutsceneViewer = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuAudioExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuWorldSearch = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuBinarySearch = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuJenkGen = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuJenkInd = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuExtractScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuExtractTextures = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuExtractRawFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuExtractShaders = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenuOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarPanel = new System.Windows.Forms.Panel();
            this.Toolbar = new CodeWalker.WinForms.ToolStripFix();
            this.ToolbarNewButton = new System.Windows.Forms.ToolStripSplitButton();
            this.ToolbarNewProjectButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarNewYmapButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarNewYtypButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarNewYbnButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarNewYndButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarNewTrainsButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarNewScenarioButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarOpenButton = new System.Windows.Forms.ToolStripSplitButton();
            this.ToolbarOpenProjectButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarOpenFilesButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarOpenFolderButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSaveButton = new System.Windows.Forms.ToolStripButton();
            this.ToolbarSaveAllButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarSelectButton = new CodeWalker.WinForms.ToolStripSplitButtonFix();
            this.ToolbarSelectEntityButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectEntityExtensionButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectArchetypeExtensionButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectTimeCycleModifierButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectCarGeneratorButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectGrassButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectWaterQuadButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectCalmingQuadButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectWaveQuadButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectCollisionButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectNavMeshButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectPathButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectTrainTrackButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectLodLightsButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectMloInstanceButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectScenarioButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectAudioButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSelectOcclusionButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarMoveButton = new System.Windows.Forms.ToolStripButton();
            this.ToolbarRotateButton = new System.Windows.Forms.ToolStripButton();
            this.ToolbarScaleButton = new System.Windows.Forms.ToolStripButton();
            this.ToolbarTransformSpaceButton = new System.Windows.Forms.ToolStripSplitButton();
            this.ToolbarObjectSpaceButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarWorldSpaceButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSnapButton = new CodeWalker.WinForms.ToolStripSplitButtonFix();
            this.ToolbarSnapToGroundButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSnapToGridButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSnapToGroundGridButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSnapGridSizeButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarRotationSnappingButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarRotationSnappingOffButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarRotationSnapping1Button = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarRotationSnapping2Button = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarRotationSnapping5Button = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarRotationSnapping10Button = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarRotationSnapping45Button = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarRotationSnapping90Button = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarRotationSnappingCustomButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarUndoButton = new System.Windows.Forms.ToolStripSplitButton();
            this.ToolbarUndoListButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarRedoButton = new System.Windows.Forms.ToolStripSplitButton();
            this.ToolbarRedoListButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarInfoWindowButton = new System.Windows.Forms.ToolStripButton();
            this.ToolbarProjectWindowButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarAddItemButton = new System.Windows.Forms.ToolStripButton();
            this.ToolbarDeleteItemButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarCopyButton = new System.Windows.Forms.ToolStripButton();
            this.ToolbarPasteButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarCameraModeButton = new System.Windows.Forms.ToolStripSplitButton();
            this.ToolbarCameraPerspectiveButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarCameraMapViewButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarCameraOrthographicButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SubtitleLabel = new System.Windows.Forms.Label();
            this.SubtitleTimer = new System.Windows.Forms.Timer(this.components);
            this.StatusStrip.SuspendLayout();
            this.ToolsPanel.SuspendLayout();
            this.ToolsTabControl.SuspendLayout();
            this.ViewTabPage.SuspendLayout();
            this.ViewTabControl.SuspendLayout();
            this.ViewWorldTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WorldDetailDistTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WorldLodDistTrackBar)).BeginInit();
            this.ViewYmapsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DetailTrackBar)).BeginInit();
            this.ViewModelTabPage.SuspendLayout();
            this.MarkersTabPage.SuspendLayout();
            this.SelectionTabPage.SuspendLayout();
            this.SelectionTabControl.SuspendLayout();
            this.SelectionEntityTabPage.SuspendLayout();
            this.SelectionArchetypeTabPage.SuspendLayout();
            this.SelectionDrawableTabPage.SuspendLayout();
            this.tabControl3.SuspendLayout();
            this.tabPage11.SuspendLayout();
            this.tabPage12.SuspendLayout();
            this.tabPage13.SuspendLayout();
            this.SelectionExtensionTabPage.SuspendLayout();
            this.OptionsTabPage.SuspendLayout();
            this.OptionsTabControl.SuspendLayout();
            this.OptionsGeneralTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MapViewDetailTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CollisionMeshRangeTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FieldOfViewTrackBar)).BeginInit();
            this.OptionsRenderTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FarClipUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NearClipUpDown)).BeginInit();
            this.OptionsHelpersTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SnapAngleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SnapGridSizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BoundsRangeTrackBar)).BeginInit();
            this.OptionsLightingTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CloudParamTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TimeSpeedTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TimeOfDayTrackBar)).BeginInit();
            this.ConsolePanel.SuspendLayout();
            this.SelectedMarkerPanel.SuspendLayout();
            this.ToolsMenu.SuspendLayout();
            this.ToolbarPanel.SuspendLayout();
            this.Toolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel,
            this.MousedLabel,
            this.StatsLabel});
            this.StatusStrip.Location = new System.Drawing.Point(0, 689);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(984, 22);
            this.StatusStrip.TabIndex = 0;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.BackColor = System.Drawing.SystemColors.Control;
            this.StatusLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(878, 17);
            this.StatusLabel.Spring = true;
            this.StatusLabel.Text = "Initialising";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MousedLabel
            // 
            this.MousedLabel.BackColor = System.Drawing.SystemColors.Control;
            this.MousedLabel.Name = "MousedLabel";
            this.MousedLabel.Size = new System.Drawing.Size(16, 17);
            this.MousedLabel.Text = "   ";
            // 
            // StatsLabel
            // 
            this.StatsLabel.BackColor = System.Drawing.SystemColors.Control;
            this.StatsLabel.DoubleClickEnabled = true;
            this.StatsLabel.Name = "StatsLabel";
            this.StatsLabel.Size = new System.Drawing.Size(75, 17);
            this.StatsLabel.Text = "0 geometries";
            this.StatsLabel.DoubleClick += new System.EventHandler(this.StatsLabel_DoubleClick);
            // 
            // ModelComboBox
            // 
            this.ModelComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModelComboBox.FormattingEnabled = true;
            this.ModelComboBox.Items.AddRange(new object[] {
            "dt1_lod_slod3",
            "dt1_tc_dufo_core",
            "dt1_tc_ufocore",
            "ex_office_citymodel_01",
            "id1_30_build3_dtl2",
            "imp_prop_ship_01a",
            "prop_alien_egg_01",
            "prop_fruit_stand_02",
            "prop_fruit_stand_03",
            "dune",
            "dune2",
            "dune2_hi",
            "adder",
            "adder_hi",
            "kuruma2",
            "kuruma2_hi",
            "infernus",
            "infernus_hi",
            "buzzard",
            "buzzard_hi",
            "rhino",
            "rhino_hi",
            "lazer",
            "lazer_hi",
            "duster",
            "duster_hi",
            "marquis",
            "marquis_hi",
            "submersible",
            "submersible_hi",
            "cargobob",
            "cargobob_hi",
            "sanchez",
            "sanchez_hi"});
            this.ModelComboBox.Location = new System.Drawing.Point(44, 7);
            this.ModelComboBox.Name = "ModelComboBox";
            this.ModelComboBox.Size = new System.Drawing.Size(150, 21);
            this.ModelComboBox.TabIndex = 11;
            this.ModelComboBox.SelectedIndexChanged += new System.EventHandler(this.ModelComboBox_SelectedIndexChanged);
            this.ModelComboBox.TextUpdate += new System.EventHandler(this.ModelComboBox_TextUpdate);
            // 
            // ToolsPanel
            // 
            this.ToolsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolsPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ToolsPanel.Controls.Add(this.ToolsDragPanel);
            this.ToolsPanel.Controls.Add(this.AboutButton);
            this.ToolsPanel.Controls.Add(this.ToolsButton);
            this.ToolsPanel.Controls.Add(this.ToolsPanelExpandButton);
            this.ToolsPanel.Controls.Add(this.ToolsTabControl);
            this.ToolsPanel.Controls.Add(this.ToolsPanelHideButton);
            this.ToolsPanel.Location = new System.Drawing.Point(754, 12);
            this.ToolsPanel.Name = "ToolsPanel";
            this.ToolsPanel.Size = new System.Drawing.Size(218, 665);
            this.ToolsPanel.TabIndex = 2;
            this.ToolsPanel.Visible = false;
            // 
            // ToolsDragPanel
            // 
            this.ToolsDragPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ToolsDragPanel.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.ToolsDragPanel.Location = new System.Drawing.Point(0, 0);
            this.ToolsDragPanel.Name = "ToolsDragPanel";
            this.ToolsDragPanel.Size = new System.Drawing.Size(4, 665);
            this.ToolsDragPanel.TabIndex = 16;
            this.ToolsDragPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ToolsDragPanel_MouseDown);
            this.ToolsDragPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ToolsDragPanel_MouseMove);
            this.ToolsDragPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolsDragPanel_MouseUp);
            // 
            // AboutButton
            // 
            this.AboutButton.Location = new System.Drawing.Point(64, 3);
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(55, 23);
            this.AboutButton.TabIndex = 15;
            this.AboutButton.Text = "About...";
            this.AboutButton.UseVisualStyleBackColor = true;
            this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
            // 
            // ToolsButton
            // 
            this.ToolsButton.Location = new System.Drawing.Point(3, 3);
            this.ToolsButton.Name = "ToolsButton";
            this.ToolsButton.Size = new System.Drawing.Size(55, 23);
            this.ToolsButton.TabIndex = 14;
            this.ToolsButton.Text = "Tools...";
            this.ToolsButton.UseVisualStyleBackColor = true;
            this.ToolsButton.Click += new System.EventHandler(this.ToolsButton_Click);
            // 
            // ToolsPanelExpandButton
            // 
            this.ToolsPanelExpandButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolsPanelExpandButton.Location = new System.Drawing.Point(149, 3);
            this.ToolsPanelExpandButton.Name = "ToolsPanelExpandButton";
            this.ToolsPanelExpandButton.Size = new System.Drawing.Size(30, 23);
            this.ToolsPanelExpandButton.TabIndex = 13;
            this.ToolsPanelExpandButton.Text = "<<";
            this.ToolsPanelExpandButton.UseVisualStyleBackColor = true;
            this.ToolsPanelExpandButton.Click += new System.EventHandler(this.ToolsPanelExpandButton_Click);
            // 
            // ToolsTabControl
            // 
            this.ToolsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolsTabControl.Controls.Add(this.ViewTabPage);
            this.ToolsTabControl.Controls.Add(this.MarkersTabPage);
            this.ToolsTabControl.Controls.Add(this.SelectionTabPage);
            this.ToolsTabControl.Controls.Add(this.OptionsTabPage);
            this.ToolsTabControl.Location = new System.Drawing.Point(3, 30);
            this.ToolsTabControl.Name = "ToolsTabControl";
            this.ToolsTabControl.SelectedIndex = 0;
            this.ToolsTabControl.Size = new System.Drawing.Size(213, 632);
            this.ToolsTabControl.TabIndex = 12;
            // 
            // ViewTabPage
            // 
            this.ViewTabPage.Controls.Add(this.StatusBarCheckBox);
            this.ViewTabPage.Controls.Add(this.ViewTabControl);
            this.ViewTabPage.Controls.Add(this.label3);
            this.ViewTabPage.Controls.Add(this.ViewModeComboBox);
            this.ViewTabPage.Controls.Add(this.ShowToolbarCheckBox);
            this.ViewTabPage.Controls.Add(this.ErrorConsoleCheckBox);
            this.ViewTabPage.Location = new System.Drawing.Point(4, 22);
            this.ViewTabPage.Name = "ViewTabPage";
            this.ViewTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ViewTabPage.Size = new System.Drawing.Size(205, 606);
            this.ViewTabPage.TabIndex = 0;
            this.ViewTabPage.Text = "View";
            this.ViewTabPage.UseVisualStyleBackColor = true;
            // 
            // StatusBarCheckBox
            // 
            this.StatusBarCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StatusBarCheckBox.AutoSize = true;
            this.StatusBarCheckBox.Checked = true;
            this.StatusBarCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.StatusBarCheckBox.Location = new System.Drawing.Point(10, 544);
            this.StatusBarCheckBox.Name = "StatusBarCheckBox";
            this.StatusBarCheckBox.Size = new System.Drawing.Size(74, 17);
            this.StatusBarCheckBox.TabIndex = 45;
            this.StatusBarCheckBox.Text = "Status bar";
            this.StatusBarCheckBox.UseVisualStyleBackColor = true;
            this.StatusBarCheckBox.CheckedChanged += new System.EventHandler(this.StatusBarCheckBox_CheckedChanged);
            // 
            // ViewTabControl
            // 
            this.ViewTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ViewTabControl.Controls.Add(this.ViewWorldTabPage);
            this.ViewTabControl.Controls.Add(this.ViewYmapsTabPage);
            this.ViewTabControl.Controls.Add(this.ViewModelTabPage);
            this.ViewTabControl.Location = new System.Drawing.Point(0, 32);
            this.ViewTabControl.Name = "ViewTabControl";
            this.ViewTabControl.SelectedIndex = 0;
            this.ViewTabControl.Size = new System.Drawing.Size(202, 506);
            this.ViewTabControl.TabIndex = 12;
            // 
            // ViewWorldTabPage
            // 
            this.ViewWorldTabPage.Controls.Add(this.EnableModsCheckBox);
            this.ViewWorldTabPage.Controls.Add(this.label30);
            this.ViewWorldTabPage.Controls.Add(this.DlcLevelComboBox);
            this.ViewWorldTabPage.Controls.Add(this.EnableDlcCheckBox);
            this.ViewWorldTabPage.Controls.Add(this.WorldYmapWeatherFilterCheckBox);
            this.ViewWorldTabPage.Controls.Add(this.WorldYmapTimeFilterCheckBox);
            this.ViewWorldTabPage.Controls.Add(this.WorldScriptedYmapsCheckBox);
            this.ViewWorldTabPage.Controls.Add(this.WorldDetailDistLabel);
            this.ViewWorldTabPage.Controls.Add(this.label18);
            this.ViewWorldTabPage.Controls.Add(this.WorldDetailDistTrackBar);
            this.ViewWorldTabPage.Controls.Add(this.WorldLodDistLabel);
            this.ViewWorldTabPage.Controls.Add(this.label16);
            this.ViewWorldTabPage.Controls.Add(this.WorldLodDistTrackBar);
            this.ViewWorldTabPage.Controls.Add(this.label15);
            this.ViewWorldTabPage.Controls.Add(this.WorldMaxLodComboBox);
            this.ViewWorldTabPage.Location = new System.Drawing.Point(4, 22);
            this.ViewWorldTabPage.Name = "ViewWorldTabPage";
            this.ViewWorldTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ViewWorldTabPage.Size = new System.Drawing.Size(194, 480);
            this.ViewWorldTabPage.TabIndex = 0;
            this.ViewWorldTabPage.Text = "World";
            this.ViewWorldTabPage.UseVisualStyleBackColor = true;
            // 
            // EnableModsCheckBox
            // 
            this.EnableModsCheckBox.AutoSize = true;
            this.EnableModsCheckBox.Enabled = false;
            this.EnableModsCheckBox.Location = new System.Drawing.Point(6, 280);
            this.EnableModsCheckBox.Name = "EnableModsCheckBox";
            this.EnableModsCheckBox.Size = new System.Drawing.Size(88, 17);
            this.EnableModsCheckBox.TabIndex = 68;
            this.EnableModsCheckBox.Text = "Enable Mods";
            this.EnableModsCheckBox.UseVisualStyleBackColor = true;
            this.EnableModsCheckBox.CheckedChanged += new System.EventHandler(this.EnableModsCheckBox_CheckedChanged);
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(1, 337);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(60, 13);
            this.label30.TabIndex = 70;
            this.label30.Text = "DLC Level:";
            // 
            // DlcLevelComboBox
            // 
            this.DlcLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DlcLevelComboBox.Enabled = false;
            this.DlcLevelComboBox.FormattingEnabled = true;
            this.DlcLevelComboBox.Items.AddRange(new object[] {
            "<Loading...>"});
            this.DlcLevelComboBox.Location = new System.Drawing.Point(62, 334);
            this.DlcLevelComboBox.Name = "DlcLevelComboBox";
            this.DlcLevelComboBox.Size = new System.Drawing.Size(126, 21);
            this.DlcLevelComboBox.TabIndex = 70;
            this.DlcLevelComboBox.SelectedIndexChanged += new System.EventHandler(this.DlcLevelComboBox_SelectedIndexChanged);
            this.DlcLevelComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DlcLevelComboBox_KeyPress);
            // 
            // EnableDlcCheckBox
            // 
            this.EnableDlcCheckBox.AutoSize = true;
            this.EnableDlcCheckBox.Enabled = false;
            this.EnableDlcCheckBox.Location = new System.Drawing.Point(6, 311);
            this.EnableDlcCheckBox.Name = "EnableDlcCheckBox";
            this.EnableDlcCheckBox.Size = new System.Drawing.Size(83, 17);
            this.EnableDlcCheckBox.TabIndex = 69;
            this.EnableDlcCheckBox.Text = "Enable DLC";
            this.EnableDlcCheckBox.UseVisualStyleBackColor = true;
            this.EnableDlcCheckBox.CheckedChanged += new System.EventHandler(this.EnableDlcCheckBox_CheckedChanged);
            // 
            // WorldYmapWeatherFilterCheckBox
            // 
            this.WorldYmapWeatherFilterCheckBox.AutoSize = true;
            this.WorldYmapWeatherFilterCheckBox.Checked = true;
            this.WorldYmapWeatherFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.WorldYmapWeatherFilterCheckBox.Location = new System.Drawing.Point(6, 237);
            this.WorldYmapWeatherFilterCheckBox.Name = "WorldYmapWeatherFilterCheckBox";
            this.WorldYmapWeatherFilterCheckBox.Size = new System.Drawing.Size(136, 17);
            this.WorldYmapWeatherFilterCheckBox.TabIndex = 67;
            this.WorldYmapWeatherFilterCheckBox.Text = "Filter ymaps by weather";
            this.WorldYmapWeatherFilterCheckBox.UseVisualStyleBackColor = true;
            this.WorldYmapWeatherFilterCheckBox.CheckedChanged += new System.EventHandler(this.WorldYmapWeatherFilterCheckBox_CheckedChanged);
            // 
            // WorldYmapTimeFilterCheckBox
            // 
            this.WorldYmapTimeFilterCheckBox.AutoSize = true;
            this.WorldYmapTimeFilterCheckBox.Checked = true;
            this.WorldYmapTimeFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.WorldYmapTimeFilterCheckBox.Location = new System.Drawing.Point(6, 214);
            this.WorldYmapTimeFilterCheckBox.Name = "WorldYmapTimeFilterCheckBox";
            this.WorldYmapTimeFilterCheckBox.Size = new System.Drawing.Size(149, 17);
            this.WorldYmapTimeFilterCheckBox.TabIndex = 66;
            this.WorldYmapTimeFilterCheckBox.Text = "Filter ymaps by time of day";
            this.WorldYmapTimeFilterCheckBox.UseVisualStyleBackColor = true;
            this.WorldYmapTimeFilterCheckBox.CheckedChanged += new System.EventHandler(this.WorldYmapTimeFilterCheckBox_CheckedChanged);
            // 
            // WorldScriptedYmapsCheckBox
            // 
            this.WorldScriptedYmapsCheckBox.AutoSize = true;
            this.WorldScriptedYmapsCheckBox.Checked = true;
            this.WorldScriptedYmapsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.WorldScriptedYmapsCheckBox.Location = new System.Drawing.Point(6, 182);
            this.WorldScriptedYmapsCheckBox.Name = "WorldScriptedYmapsCheckBox";
            this.WorldScriptedYmapsCheckBox.Size = new System.Drawing.Size(126, 17);
            this.WorldScriptedYmapsCheckBox.TabIndex = 65;
            this.WorldScriptedYmapsCheckBox.Text = "Show scripted ymaps";
            this.WorldScriptedYmapsCheckBox.UseVisualStyleBackColor = true;
            this.WorldScriptedYmapsCheckBox.CheckedChanged += new System.EventHandler(this.WorldScriptedYmapsCheckBox_CheckedChanged);
            // 
            // WorldDetailDistLabel
            // 
            this.WorldDetailDistLabel.AutoSize = true;
            this.WorldDetailDistLabel.Location = new System.Drawing.Point(87, 94);
            this.WorldDetailDistLabel.Name = "WorldDetailDistLabel";
            this.WorldDetailDistLabel.Size = new System.Drawing.Size(22, 13);
            this.WorldDetailDistLabel.TabIndex = 64;
            this.WorldDetailDistLabel.Text = "1.0";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(1, 94);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(80, 13);
            this.label18.TabIndex = 63;
            this.label18.Text = "Detail distance:";
            // 
            // WorldDetailDistTrackBar
            // 
            this.WorldDetailDistTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WorldDetailDistTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.WorldDetailDistTrackBar.LargeChange = 10;
            this.WorldDetailDistTrackBar.Location = new System.Drawing.Point(6, 110);
            this.WorldDetailDistTrackBar.Maximum = 50;
            this.WorldDetailDistTrackBar.Name = "WorldDetailDistTrackBar";
            this.WorldDetailDistTrackBar.Size = new System.Drawing.Size(182, 45);
            this.WorldDetailDistTrackBar.TabIndex = 62;
            this.WorldDetailDistTrackBar.TickFrequency = 2;
            this.WorldDetailDistTrackBar.Value = 10;
            this.WorldDetailDistTrackBar.Scroll += new System.EventHandler(this.WorldDetailDistTrackBar_Scroll);
            // 
            // WorldLodDistLabel
            // 
            this.WorldLodDistLabel.AutoSize = true;
            this.WorldLodDistLabel.Location = new System.Drawing.Point(82, 39);
            this.WorldLodDistLabel.Name = "WorldLodDistLabel";
            this.WorldLodDistLabel.Size = new System.Drawing.Size(22, 13);
            this.WorldLodDistLabel.TabIndex = 61;
            this.WorldLodDistLabel.Text = "1.0";
            this.WorldLodDistLabel.Visible = false;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(1, 39);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(75, 13);
            this.label16.TabIndex = 60;
            this.label16.Text = "LOD distance:";
            this.label16.Visible = false;
            // 
            // WorldLodDistTrackBar
            // 
            this.WorldLodDistTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WorldLodDistTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.WorldLodDistTrackBar.LargeChange = 10;
            this.WorldLodDistTrackBar.Location = new System.Drawing.Point(6, 55);
            this.WorldLodDistTrackBar.Maximum = 30;
            this.WorldLodDistTrackBar.Name = "WorldLodDistTrackBar";
            this.WorldLodDistTrackBar.Size = new System.Drawing.Size(182, 45);
            this.WorldLodDistTrackBar.TabIndex = 59;
            this.WorldLodDistTrackBar.TickFrequency = 2;
            this.WorldLodDistTrackBar.Value = 10;
            this.WorldLodDistTrackBar.Visible = false;
            this.WorldLodDistTrackBar.Scroll += new System.EventHandler(this.WorldLodDistTrackBar_Scroll);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(1, 9);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(55, 13);
            this.label15.TabIndex = 58;
            this.label15.Text = "Max LOD:";
            // 
            // WorldMaxLodComboBox
            // 
            this.WorldMaxLodComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WorldMaxLodComboBox.FormattingEnabled = true;
            this.WorldMaxLodComboBox.Items.AddRange(new object[] {
            "ORPHANHD",
            "HD",
            "LOD",
            "SLOD1",
            "SLOD2",
            "SLOD3",
            "SLOD4"});
            this.WorldMaxLodComboBox.Location = new System.Drawing.Point(62, 6);
            this.WorldMaxLodComboBox.Name = "WorldMaxLodComboBox";
            this.WorldMaxLodComboBox.Size = new System.Drawing.Size(126, 21);
            this.WorldMaxLodComboBox.TabIndex = 57;
            this.WorldMaxLodComboBox.SelectedIndexChanged += new System.EventHandler(this.WorldMaxLodComboBox_SelectedIndexChanged);
            this.WorldMaxLodComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.WorldMaxLodComboBox_KeyPress);
            // 
            // ViewYmapsTabPage
            // 
            this.ViewYmapsTabPage.Controls.Add(this.ShowYmapChildrenCheckBox);
            this.ViewYmapsTabPage.Controls.Add(this.label2);
            this.ViewYmapsTabPage.Controls.Add(this.DetailTrackBar);
            this.ViewYmapsTabPage.Controls.Add(this.DynamicLODCheckBox);
            this.ViewYmapsTabPage.Controls.Add(this.YmapsTextBox);
            this.ViewYmapsTabPage.Location = new System.Drawing.Point(4, 22);
            this.ViewYmapsTabPage.Name = "ViewYmapsTabPage";
            this.ViewYmapsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ViewYmapsTabPage.Size = new System.Drawing.Size(194, 480);
            this.ViewYmapsTabPage.TabIndex = 1;
            this.ViewYmapsTabPage.Text = "Ymaps";
            this.ViewYmapsTabPage.UseVisualStyleBackColor = true;
            // 
            // ShowYmapChildrenCheckBox
            // 
            this.ShowYmapChildrenCheckBox.AutoSize = true;
            this.ShowYmapChildrenCheckBox.Enabled = false;
            this.ShowYmapChildrenCheckBox.Location = new System.Drawing.Point(101, 6);
            this.ShowYmapChildrenCheckBox.Name = "ShowYmapChildrenCheckBox";
            this.ShowYmapChildrenCheckBox.Size = new System.Drawing.Size(93, 17);
            this.ShowYmapChildrenCheckBox.TabIndex = 35;
            this.ShowYmapChildrenCheckBox.Text = "Show children";
            this.ShowYmapChildrenCheckBox.UseVisualStyleBackColor = true;
            this.ShowYmapChildrenCheckBox.CheckedChanged += new System.EventHandler(this.ShowYmapChildrenCheckBox_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Ymaps to load:";
            // 
            // DetailTrackBar
            // 
            this.DetailTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DetailTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.DetailTrackBar.Location = new System.Drawing.Point(6, 24);
            this.DetailTrackBar.Maximum = 20;
            this.DetailTrackBar.Name = "DetailTrackBar";
            this.DetailTrackBar.Size = new System.Drawing.Size(182, 45);
            this.DetailTrackBar.TabIndex = 34;
            this.DetailTrackBar.Value = 5;
            this.DetailTrackBar.Scroll += new System.EventHandler(this.DetailTrackBar_Scroll);
            // 
            // DynamicLODCheckBox
            // 
            this.DynamicLODCheckBox.AutoSize = true;
            this.DynamicLODCheckBox.Checked = true;
            this.DynamicLODCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DynamicLODCheckBox.Location = new System.Drawing.Point(2, 6);
            this.DynamicLODCheckBox.Name = "DynamicLODCheckBox";
            this.DynamicLODCheckBox.Size = new System.Drawing.Size(92, 17);
            this.DynamicLODCheckBox.TabIndex = 33;
            this.DynamicLODCheckBox.Text = "Dynamic LOD";
            this.DynamicLODCheckBox.UseVisualStyleBackColor = true;
            this.DynamicLODCheckBox.CheckedChanged += new System.EventHandler(this.DynamicLODCheckBox_CheckedChanged);
            // 
            // YmapsTextBox
            // 
            this.YmapsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.YmapsTextBox.Location = new System.Drawing.Point(0, 72);
            this.YmapsTextBox.Multiline = true;
            this.YmapsTextBox.Name = "YmapsTextBox";
            this.YmapsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.YmapsTextBox.Size = new System.Drawing.Size(194, 408);
            this.YmapsTextBox.TabIndex = 36;
            this.YmapsTextBox.Text = resources.GetString("YmapsTextBox.Text");
            this.YmapsTextBox.TextChanged += new System.EventHandler(this.YmapsTextBox_TextChanged);
            // 
            // ViewModelTabPage
            // 
            this.ViewModelTabPage.Controls.Add(this.label1);
            this.ViewModelTabPage.Controls.Add(this.ModelComboBox);
            this.ViewModelTabPage.Location = new System.Drawing.Point(4, 22);
            this.ViewModelTabPage.Name = "ViewModelTabPage";
            this.ViewModelTabPage.Size = new System.Drawing.Size(194, 480);
            this.ViewModelTabPage.TabIndex = 2;
            this.ViewModelTabPage.Text = "Model";
            this.ViewModelTabPage.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-1, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Model:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Mode:";
            // 
            // ViewModeComboBox
            // 
            this.ViewModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ViewModeComboBox.FormattingEnabled = true;
            this.ViewModeComboBox.Items.AddRange(new object[] {
            "World view",
            "Ymap view",
            "Model view"});
            this.ViewModeComboBox.Location = new System.Drawing.Point(48, 5);
            this.ViewModeComboBox.Name = "ViewModeComboBox";
            this.ViewModeComboBox.Size = new System.Drawing.Size(111, 21);
            this.ViewModeComboBox.TabIndex = 10;
            this.ViewModeComboBox.SelectedIndexChanged += new System.EventHandler(this.ViewModeComboBox_SelectedIndexChanged);
            this.ViewModeComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ViewModeComboBox_KeyPress);
            // 
            // ShowToolbarCheckBox
            // 
            this.ShowToolbarCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShowToolbarCheckBox.AutoSize = true;
            this.ShowToolbarCheckBox.Location = new System.Drawing.Point(10, 586);
            this.ShowToolbarCheckBox.Name = "ShowToolbarCheckBox";
            this.ShowToolbarCheckBox.Size = new System.Drawing.Size(108, 17);
            this.ShowToolbarCheckBox.TabIndex = 47;
            this.ShowToolbarCheckBox.Text = "Show Toolbar (T)";
            this.ShowToolbarCheckBox.UseVisualStyleBackColor = true;
            this.ShowToolbarCheckBox.CheckedChanged += new System.EventHandler(this.ShowToolbarCheckBox_CheckedChanged);
            // 
            // ErrorConsoleCheckBox
            // 
            this.ErrorConsoleCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ErrorConsoleCheckBox.AutoSize = true;
            this.ErrorConsoleCheckBox.Location = new System.Drawing.Point(10, 565);
            this.ErrorConsoleCheckBox.Name = "ErrorConsoleCheckBox";
            this.ErrorConsoleCheckBox.Size = new System.Drawing.Size(88, 17);
            this.ErrorConsoleCheckBox.TabIndex = 46;
            this.ErrorConsoleCheckBox.Text = "Error console";
            this.ErrorConsoleCheckBox.UseVisualStyleBackColor = true;
            this.ErrorConsoleCheckBox.CheckedChanged += new System.EventHandler(this.ErrorConsoleCheckBox_CheckedChanged);
            // 
            // MarkersTabPage
            // 
            this.MarkersTabPage.Controls.Add(this.label27);
            this.MarkersTabPage.Controls.Add(this.CameraPositionTextBox);
            this.MarkersTabPage.Controls.Add(this.AddSelectionMarkerButton);
            this.MarkersTabPage.Controls.Add(this.AddCurrentPositonMarkerButton);
            this.MarkersTabPage.Controls.Add(this.ResetMarkersButton);
            this.MarkersTabPage.Controls.Add(this.ClearMarkersButton);
            this.MarkersTabPage.Controls.Add(this.GoToButton);
            this.MarkersTabPage.Controls.Add(this.ShowLocatorCheckBox);
            this.MarkersTabPage.Controls.Add(this.label6);
            this.MarkersTabPage.Controls.Add(this.LocateTextBox);
            this.MarkersTabPage.Controls.Add(this.label7);
            this.MarkersTabPage.Controls.Add(this.AddMarkersButton);
            this.MarkersTabPage.Controls.Add(this.MultiFindTextBox);
            this.MarkersTabPage.Location = new System.Drawing.Point(4, 22);
            this.MarkersTabPage.Name = "MarkersTabPage";
            this.MarkersTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.MarkersTabPage.Size = new System.Drawing.Size(205, 606);
            this.MarkersTabPage.TabIndex = 1;
            this.MarkersTabPage.Text = "Markers";
            this.MarkersTabPage.UseVisualStyleBackColor = true;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(-2, 50);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(121, 13);
            this.label27.TabIndex = 22;
            this.label27.Text = "Current camera position:";
            // 
            // CameraPositionTextBox
            // 
            this.CameraPositionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CameraPositionTextBox.Location = new System.Drawing.Point(0, 67);
            this.CameraPositionTextBox.Name = "CameraPositionTextBox";
            this.CameraPositionTextBox.Size = new System.Drawing.Size(205, 20);
            this.CameraPositionTextBox.TabIndex = 16;
            this.CameraPositionTextBox.Text = "0, 0, 0";
            // 
            // AddSelectionMarkerButton
            // 
            this.AddSelectionMarkerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddSelectionMarkerButton.Location = new System.Drawing.Point(0, 577);
            this.AddSelectionMarkerButton.Name = "AddSelectionMarkerButton";
            this.AddSelectionMarkerButton.Size = new System.Drawing.Size(97, 23);
            this.AddSelectionMarkerButton.TabIndex = 22;
            this.AddSelectionMarkerButton.Text = "Add selection";
            this.AddSelectionMarkerButton.UseVisualStyleBackColor = true;
            this.AddSelectionMarkerButton.Click += new System.EventHandler(this.AddSelectionMarkerButton_Click);
            // 
            // AddCurrentPositonMarkerButton
            // 
            this.AddCurrentPositonMarkerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddCurrentPositonMarkerButton.Location = new System.Drawing.Point(0, 548);
            this.AddCurrentPositonMarkerButton.Name = "AddCurrentPositonMarkerButton";
            this.AddCurrentPositonMarkerButton.Size = new System.Drawing.Size(97, 23);
            this.AddCurrentPositonMarkerButton.TabIndex = 20;
            this.AddCurrentPositonMarkerButton.Text = "Add current pos";
            this.AddCurrentPositonMarkerButton.UseVisualStyleBackColor = true;
            this.AddCurrentPositonMarkerButton.Click += new System.EventHandler(this.AddCurrentPositonMarkerButton_Click);
            // 
            // ResetMarkersButton
            // 
            this.ResetMarkersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ResetMarkersButton.Location = new System.Drawing.Point(108, 548);
            this.ResetMarkersButton.Name = "ResetMarkersButton";
            this.ResetMarkersButton.Size = new System.Drawing.Size(97, 23);
            this.ResetMarkersButton.TabIndex = 21;
            this.ResetMarkersButton.Text = "Default markers";
            this.ResetMarkersButton.UseVisualStyleBackColor = true;
            this.ResetMarkersButton.Click += new System.EventHandler(this.ResetMarkersButton_Click);
            // 
            // ClearMarkersButton
            // 
            this.ClearMarkersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ClearMarkersButton.Location = new System.Drawing.Point(108, 519);
            this.ClearMarkersButton.Name = "ClearMarkersButton";
            this.ClearMarkersButton.Size = new System.Drawing.Size(97, 23);
            this.ClearMarkersButton.TabIndex = 19;
            this.ClearMarkersButton.Text = "Clear markers";
            this.ClearMarkersButton.UseVisualStyleBackColor = true;
            this.ClearMarkersButton.Click += new System.EventHandler(this.ClearMarkersButton_Click);
            // 
            // GoToButton
            // 
            this.GoToButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GoToButton.Location = new System.Drawing.Point(162, 24);
            this.GoToButton.Name = "GoToButton";
            this.GoToButton.Size = new System.Drawing.Size(43, 22);
            this.GoToButton.TabIndex = 15;
            this.GoToButton.Text = "Go to";
            this.GoToButton.UseVisualStyleBackColor = true;
            this.GoToButton.Click += new System.EventHandler(this.GoToButton_Click);
            // 
            // ShowLocatorCheckBox
            // 
            this.ShowLocatorCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowLocatorCheckBox.AutoSize = true;
            this.ShowLocatorCheckBox.Location = new System.Drawing.Point(101, 8);
            this.ShowLocatorCheckBox.Name = "ShowLocatorCheckBox";
            this.ShowLocatorCheckBox.Size = new System.Drawing.Size(88, 17);
            this.ShowLocatorCheckBox.TabIndex = 13;
            this.ShowLocatorCheckBox.Text = "Show marker";
            this.ShowLocatorCheckBox.UseVisualStyleBackColor = true;
            this.ShowLocatorCheckBox.CheckedChanged += new System.EventHandler(this.ShowLocatorCheckBox_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(-2, 8);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Locate: X, Y, Z";
            // 
            // LocateTextBox
            // 
            this.LocateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LocateTextBox.Location = new System.Drawing.Point(0, 25);
            this.LocateTextBox.Name = "LocateTextBox";
            this.LocateTextBox.Size = new System.Drawing.Size(156, 20);
            this.LocateTextBox.TabIndex = 14;
            this.LocateTextBox.Text = "0, 0, 0";
            this.LocateTextBox.TextChanged += new System.EventHandler(this.LocateTextBox_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(-2, 101);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(122, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Multi-find: X, Y, Z, Name";
            // 
            // AddMarkersButton
            // 
            this.AddMarkersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddMarkersButton.Location = new System.Drawing.Point(0, 519);
            this.AddMarkersButton.Name = "AddMarkersButton";
            this.AddMarkersButton.Size = new System.Drawing.Size(97, 23);
            this.AddMarkersButton.TabIndex = 18;
            this.AddMarkersButton.Text = "Add markers";
            this.AddMarkersButton.UseVisualStyleBackColor = true;
            this.AddMarkersButton.Click += new System.EventHandler(this.AddMarkersButton_Click);
            // 
            // MultiFindTextBox
            // 
            this.MultiFindTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MultiFindTextBox.Location = new System.Drawing.Point(0, 117);
            this.MultiFindTextBox.MaxLength = 1048576;
            this.MultiFindTextBox.Multiline = true;
            this.MultiFindTextBox.Name = "MultiFindTextBox";
            this.MultiFindTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.MultiFindTextBox.Size = new System.Drawing.Size(205, 392);
            this.MultiFindTextBox.TabIndex = 17;
            // 
            // SelectionTabPage
            // 
            this.SelectionTabPage.Controls.Add(this.label25);
            this.SelectionTabPage.Controls.Add(this.SelectionModeComboBox);
            this.SelectionTabPage.Controls.Add(this.SelectionNameTextBox);
            this.SelectionTabPage.Controls.Add(this.SelectionTabControl);
            this.SelectionTabPage.Controls.Add(this.MouseSelectCheckBox);
            this.SelectionTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionTabPage.Name = "SelectionTabPage";
            this.SelectionTabPage.Size = new System.Drawing.Size(205, 606);
            this.SelectionTabPage.TabIndex = 2;
            this.SelectionTabPage.Text = "Selection";
            this.SelectionTabPage.UseVisualStyleBackColor = true;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(6, 33);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(37, 13);
            this.label25.TabIndex = 28;
            this.label25.Text = "Mode:";
            // 
            // SelectionModeComboBox
            // 
            this.SelectionModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SelectionModeComboBox.FormattingEnabled = true;
            this.SelectionModeComboBox.Items.AddRange(new object[] {
            "Entity",
            "Entity Extension",
            "Archetype Extension",
            "Time Cycle Modifier",
            "Car Generator",
            "Grass",
            "Water Quad",
            "Water Calming Quad",
            "Water Wave Quad",
            "Collision",
            "Nav Mesh",
            "Path",
            "Train Track",
            "Lod Lights",
            "Mlo Instance",
            "Scenario",
            "Audio",
            "Occlusion"});
            this.SelectionModeComboBox.Location = new System.Drawing.Point(51, 30);
            this.SelectionModeComboBox.Name = "SelectionModeComboBox";
            this.SelectionModeComboBox.Size = new System.Drawing.Size(121, 21);
            this.SelectionModeComboBox.TabIndex = 23;
            this.SelectionModeComboBox.SelectedIndexChanged += new System.EventHandler(this.SelectionModeComboBox_SelectedIndexChanged);
            this.SelectionModeComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SelectionModeComboBox_KeyPress);
            // 
            // SelectionNameTextBox
            // 
            this.SelectionNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectionNameTextBox.BackColor = System.Drawing.Color.White;
            this.SelectionNameTextBox.Location = new System.Drawing.Point(3, 66);
            this.SelectionNameTextBox.Name = "SelectionNameTextBox";
            this.SelectionNameTextBox.ReadOnly = true;
            this.SelectionNameTextBox.Size = new System.Drawing.Size(199, 20);
            this.SelectionNameTextBox.TabIndex = 26;
            this.SelectionNameTextBox.Text = "Nothing selected";
            // 
            // SelectionTabControl
            // 
            this.SelectionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectionTabControl.Controls.Add(this.SelectionEntityTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionArchetypeTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionDrawableTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionExtensionTabPage);
            this.SelectionTabControl.Location = new System.Drawing.Point(0, 95);
            this.SelectionTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.SelectionTabControl.Name = "SelectionTabControl";
            this.SelectionTabControl.SelectedIndex = 0;
            this.SelectionTabControl.Size = new System.Drawing.Size(205, 511);
            this.SelectionTabControl.TabIndex = 28;
            // 
            // SelectionEntityTabPage
            // 
            this.SelectionEntityTabPage.Controls.Add(this.SelEntityPropertyGrid);
            this.SelectionEntityTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionEntityTabPage.Name = "SelectionEntityTabPage";
            this.SelectionEntityTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SelectionEntityTabPage.Size = new System.Drawing.Size(197, 485);
            this.SelectionEntityTabPage.TabIndex = 0;
            this.SelectionEntityTabPage.Text = "Entity";
            this.SelectionEntityTabPage.UseVisualStyleBackColor = true;
            // 
            // SelEntityPropertyGrid
            // 
            this.SelEntityPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelEntityPropertyGrid.HelpVisible = false;
            this.SelEntityPropertyGrid.Location = new System.Drawing.Point(0, 6);
            this.SelEntityPropertyGrid.Name = "SelEntityPropertyGrid";
            this.SelEntityPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.SelEntityPropertyGrid.ReadOnly = true;
            this.SelEntityPropertyGrid.Size = new System.Drawing.Size(197, 476);
            this.SelEntityPropertyGrid.TabIndex = 35;
            this.SelEntityPropertyGrid.ToolbarVisible = false;
            // 
            // SelectionArchetypeTabPage
            // 
            this.SelectionArchetypeTabPage.Controls.Add(this.SelArchetypePropertyGrid);
            this.SelectionArchetypeTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionArchetypeTabPage.Name = "SelectionArchetypeTabPage";
            this.SelectionArchetypeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SelectionArchetypeTabPage.Size = new System.Drawing.Size(197, 485);
            this.SelectionArchetypeTabPage.TabIndex = 1;
            this.SelectionArchetypeTabPage.Text = "Archetype";
            this.SelectionArchetypeTabPage.UseVisualStyleBackColor = true;
            // 
            // SelArchetypePropertyGrid
            // 
            this.SelArchetypePropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelArchetypePropertyGrid.HelpVisible = false;
            this.SelArchetypePropertyGrid.Location = new System.Drawing.Point(0, 6);
            this.SelArchetypePropertyGrid.Name = "SelArchetypePropertyGrid";
            this.SelArchetypePropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.SelArchetypePropertyGrid.ReadOnly = true;
            this.SelArchetypePropertyGrid.Size = new System.Drawing.Size(197, 476);
            this.SelArchetypePropertyGrid.TabIndex = 36;
            this.SelArchetypePropertyGrid.ToolbarVisible = false;
            // 
            // SelectionDrawableTabPage
            // 
            this.SelectionDrawableTabPage.Controls.Add(this.tabControl3);
            this.SelectionDrawableTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionDrawableTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.SelectionDrawableTabPage.Name = "SelectionDrawableTabPage";
            this.SelectionDrawableTabPage.Size = new System.Drawing.Size(197, 485);
            this.SelectionDrawableTabPage.TabIndex = 2;
            this.SelectionDrawableTabPage.Text = "Drawable";
            this.SelectionDrawableTabPage.UseVisualStyleBackColor = true;
            // 
            // tabControl3
            // 
            this.tabControl3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl3.Controls.Add(this.tabPage11);
            this.tabControl3.Controls.Add(this.tabPage12);
            this.tabControl3.Controls.Add(this.tabPage13);
            this.tabControl3.Location = new System.Drawing.Point(-4, 7);
            this.tabControl3.Name = "tabControl3";
            this.tabControl3.SelectedIndex = 0;
            this.tabControl3.Size = new System.Drawing.Size(205, 478);
            this.tabControl3.TabIndex = 28;
            // 
            // tabPage11
            // 
            this.tabPage11.Controls.Add(this.SelDrawablePropertyGrid);
            this.tabPage11.Location = new System.Drawing.Point(4, 22);
            this.tabPage11.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage11.Name = "tabPage11";
            this.tabPage11.Size = new System.Drawing.Size(197, 452);
            this.tabPage11.TabIndex = 0;
            this.tabPage11.Text = "Info";
            this.tabPage11.UseVisualStyleBackColor = true;
            // 
            // SelDrawablePropertyGrid
            // 
            this.SelDrawablePropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawablePropertyGrid.HelpVisible = false;
            this.SelDrawablePropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.SelDrawablePropertyGrid.Name = "SelDrawablePropertyGrid";
            this.SelDrawablePropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.SelDrawablePropertyGrid.ReadOnly = true;
            this.SelDrawablePropertyGrid.Size = new System.Drawing.Size(197, 452);
            this.SelDrawablePropertyGrid.TabIndex = 37;
            this.SelDrawablePropertyGrid.ToolbarVisible = false;
            // 
            // tabPage12
            // 
            this.tabPage12.Controls.Add(this.SelDrawableModelsTreeView);
            this.tabPage12.Location = new System.Drawing.Point(4, 22);
            this.tabPage12.Name = "tabPage12";
            this.tabPage12.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage12.Size = new System.Drawing.Size(197, 452);
            this.tabPage12.TabIndex = 1;
            this.tabPage12.Text = "Models";
            this.tabPage12.UseVisualStyleBackColor = true;
            // 
            // SelDrawableModelsTreeView
            // 
            this.SelDrawableModelsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableModelsTreeView.CheckBoxes = true;
            this.SelDrawableModelsTreeView.Location = new System.Drawing.Point(0, 0);
            this.SelDrawableModelsTreeView.Name = "SelDrawableModelsTreeView";
            this.SelDrawableModelsTreeView.ShowRootLines = false;
            this.SelDrawableModelsTreeView.Size = new System.Drawing.Size(197, 452);
            this.SelDrawableModelsTreeView.TabIndex = 39;
            this.SelDrawableModelsTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.SelDrawableModelsTreeView_AfterCheck);
            this.SelDrawableModelsTreeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.SelDrawableModelsTreeView_NodeMouseDoubleClick);
            this.SelDrawableModelsTreeView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SelDrawableModelsTreeView_KeyPress);
            // 
            // tabPage13
            // 
            this.tabPage13.Controls.Add(this.SelDrawableTexturesTreeView);
            this.tabPage13.Location = new System.Drawing.Point(4, 22);
            this.tabPage13.Name = "tabPage13";
            this.tabPage13.Size = new System.Drawing.Size(197, 452);
            this.tabPage13.TabIndex = 2;
            this.tabPage13.Text = "Textures";
            this.tabPage13.UseVisualStyleBackColor = true;
            // 
            // SelDrawableTexturesTreeView
            // 
            this.SelDrawableTexturesTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableTexturesTreeView.Location = new System.Drawing.Point(0, 0);
            this.SelDrawableTexturesTreeView.Name = "SelDrawableTexturesTreeView";
            this.SelDrawableTexturesTreeView.ShowRootLines = false;
            this.SelDrawableTexturesTreeView.Size = new System.Drawing.Size(197, 452);
            this.SelDrawableTexturesTreeView.TabIndex = 40;
            // 
            // SelectionExtensionTabPage
            // 
            this.SelectionExtensionTabPage.Controls.Add(this.SelExtensionPropertyGrid);
            this.SelectionExtensionTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionExtensionTabPage.Name = "SelectionExtensionTabPage";
            this.SelectionExtensionTabPage.Size = new System.Drawing.Size(197, 485);
            this.SelectionExtensionTabPage.TabIndex = 3;
            this.SelectionExtensionTabPage.Text = "Ext";
            this.SelectionExtensionTabPage.UseVisualStyleBackColor = true;
            // 
            // SelExtensionPropertyGrid
            // 
            this.SelExtensionPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelExtensionPropertyGrid.HelpVisible = false;
            this.SelExtensionPropertyGrid.Location = new System.Drawing.Point(0, 6);
            this.SelExtensionPropertyGrid.Name = "SelExtensionPropertyGrid";
            this.SelExtensionPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.SelExtensionPropertyGrid.ReadOnly = true;
            this.SelExtensionPropertyGrid.Size = new System.Drawing.Size(197, 476);
            this.SelExtensionPropertyGrid.TabIndex = 36;
            this.SelExtensionPropertyGrid.ToolbarVisible = false;
            // 
            // MouseSelectCheckBox
            // 
            this.MouseSelectCheckBox.AutoSize = true;
            this.MouseSelectCheckBox.Location = new System.Drawing.Point(8, 7);
            this.MouseSelectCheckBox.Name = "MouseSelectCheckBox";
            this.MouseSelectCheckBox.Size = new System.Drawing.Size(143, 17);
            this.MouseSelectCheckBox.TabIndex = 22;
            this.MouseSelectCheckBox.Text = "Mouse select (right click)";
            this.MouseSelectCheckBox.UseVisualStyleBackColor = true;
            this.MouseSelectCheckBox.CheckedChanged += new System.EventHandler(this.MouseSelectCheckBox_CheckedChanged);
            // 
            // OptionsTabPage
            // 
            this.OptionsTabPage.Controls.Add(this.OptionsTabControl);
            this.OptionsTabPage.Location = new System.Drawing.Point(4, 22);
            this.OptionsTabPage.Name = "OptionsTabPage";
            this.OptionsTabPage.Size = new System.Drawing.Size(205, 606);
            this.OptionsTabPage.TabIndex = 3;
            this.OptionsTabPage.Text = "Options";
            this.OptionsTabPage.UseVisualStyleBackColor = true;
            // 
            // OptionsTabControl
            // 
            this.OptionsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OptionsTabControl.Controls.Add(this.OptionsGeneralTabPage);
            this.OptionsTabControl.Controls.Add(this.OptionsRenderTabPage);
            this.OptionsTabControl.Controls.Add(this.OptionsHelpersTabPage);
            this.OptionsTabControl.Controls.Add(this.OptionsLightingTabPage);
            this.OptionsTabControl.Location = new System.Drawing.Point(0, 3);
            this.OptionsTabControl.Name = "OptionsTabControl";
            this.OptionsTabControl.SelectedIndex = 0;
            this.OptionsTabControl.Size = new System.Drawing.Size(208, 603);
            this.OptionsTabControl.TabIndex = 50;
            // 
            // OptionsGeneralTabPage
            // 
            this.OptionsGeneralTabPage.Controls.Add(this.SaveTimeOfDayCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.SavePositionCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.ResetSettingsButton);
            this.OptionsGeneralTabPage.Controls.Add(this.CarGeneratorsCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.RenderEntitiesCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.AdvancedSettingsButton);
            this.OptionsGeneralTabPage.Controls.Add(this.SaveSettingsButton);
            this.OptionsGeneralTabPage.Controls.Add(this.ControlSettingsButton);
            this.OptionsGeneralTabPage.Controls.Add(this.MapViewDetailLabel);
            this.OptionsGeneralTabPage.Controls.Add(this.label28);
            this.OptionsGeneralTabPage.Controls.Add(this.MapViewDetailTrackBar);
            this.OptionsGeneralTabPage.Controls.Add(this.CameraModeComboBox);
            this.OptionsGeneralTabPage.Controls.Add(this.label24);
            this.OptionsGeneralTabPage.Controls.Add(this.WaterQuadsCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.FieldOfViewLabel);
            this.OptionsGeneralTabPage.Controls.Add(this.label22);
            this.OptionsGeneralTabPage.Controls.Add(this.TimedEntitiesAlwaysOnCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.GrassCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.InteriorsCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.CollisionMeshLayerDrawableCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.CollisionMeshLayer2CheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.CollisionMeshLayer1CheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.label13);
            this.OptionsGeneralTabPage.Controls.Add(this.CollisionMeshLayer0CheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.label12);
            this.OptionsGeneralTabPage.Controls.Add(this.CollisionMeshRangeTrackBar);
            this.OptionsGeneralTabPage.Controls.Add(this.CollisionMeshesCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.FullScreenCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.TimedEntitiesCheckBox);
            this.OptionsGeneralTabPage.Controls.Add(this.FieldOfViewTrackBar);
            this.OptionsGeneralTabPage.Location = new System.Drawing.Point(4, 22);
            this.OptionsGeneralTabPage.Name = "OptionsGeneralTabPage";
            this.OptionsGeneralTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.OptionsGeneralTabPage.Size = new System.Drawing.Size(200, 577);
            this.OptionsGeneralTabPage.TabIndex = 0;
            this.OptionsGeneralTabPage.Text = "General";
            this.OptionsGeneralTabPage.UseVisualStyleBackColor = true;
            // 
            // SaveTimeOfDayCheckBox
            // 
            this.SaveTimeOfDayCheckBox.AutoSize = true;
            this.SaveTimeOfDayCheckBox.Location = new System.Drawing.Point(10, 485);
            this.SaveTimeOfDayCheckBox.Name = "SaveTimeOfDayCheckBox";
            this.SaveTimeOfDayCheckBox.Size = new System.Drawing.Size(139, 17);
            this.SaveTimeOfDayCheckBox.TabIndex = 149;
            this.SaveTimeOfDayCheckBox.Text = "Save time of day on exit";
            this.SaveTimeOfDayCheckBox.UseVisualStyleBackColor = true;
            // 
            // SavePositionCheckBox
            // 
            this.SavePositionCheckBox.AutoSize = true;
            this.SavePositionCheckBox.Location = new System.Drawing.Point(10, 464);
            this.SavePositionCheckBox.Name = "SavePositionCheckBox";
            this.SavePositionCheckBox.Size = new System.Drawing.Size(124, 17);
            this.SavePositionCheckBox.TabIndex = 148;
            this.SavePositionCheckBox.Text = "Save position on exit";
            this.SavePositionCheckBox.UseVisualStyleBackColor = true;
            // 
            // ResetSettingsButton
            // 
            this.ResetSettingsButton.Location = new System.Drawing.Point(4, 551);
            this.ResetSettingsButton.Name = "ResetSettingsButton";
            this.ResetSettingsButton.Size = new System.Drawing.Size(93, 23);
            this.ResetSettingsButton.TabIndex = 146;
            this.ResetSettingsButton.Text = "Reset settings";
            this.ResetSettingsButton.UseVisualStyleBackColor = true;
            this.ResetSettingsButton.Click += new System.EventHandler(this.ResetSettingsButton_Click);
            // 
            // CarGeneratorsCheckBox
            // 
            this.CarGeneratorsCheckBox.AutoSize = true;
            this.CarGeneratorsCheckBox.Location = new System.Drawing.Point(10, 72);
            this.CarGeneratorsCheckBox.Name = "CarGeneratorsCheckBox";
            this.CarGeneratorsCheckBox.Size = new System.Drawing.Size(124, 17);
            this.CarGeneratorsCheckBox.TabIndex = 31;
            this.CarGeneratorsCheckBox.Text = "Show car generators";
            this.CarGeneratorsCheckBox.UseVisualStyleBackColor = true;
            this.CarGeneratorsCheckBox.CheckedChanged += new System.EventHandler(this.CarGeneratorsCheckBox_CheckedChanged);
            // 
            // RenderEntitiesCheckBox
            // 
            this.RenderEntitiesCheckBox.AutoSize = true;
            this.RenderEntitiesCheckBox.Checked = true;
            this.RenderEntitiesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RenderEntitiesCheckBox.Location = new System.Drawing.Point(10, 30);
            this.RenderEntitiesCheckBox.Name = "RenderEntitiesCheckBox";
            this.RenderEntitiesCheckBox.Size = new System.Drawing.Size(89, 17);
            this.RenderEntitiesCheckBox.TabIndex = 29;
            this.RenderEntitiesCheckBox.Text = "Show entities";
            this.RenderEntitiesCheckBox.UseVisualStyleBackColor = true;
            this.RenderEntitiesCheckBox.CheckedChanged += new System.EventHandler(this.RenderEntitiesCheckBox_CheckedChanged);
            // 
            // AdvancedSettingsButton
            // 
            this.AdvancedSettingsButton.Location = new System.Drawing.Point(103, 522);
            this.AdvancedSettingsButton.Name = "AdvancedSettingsButton";
            this.AdvancedSettingsButton.Size = new System.Drawing.Size(93, 23);
            this.AdvancedSettingsButton.TabIndex = 46;
            this.AdvancedSettingsButton.Text = "Advanced...";
            this.AdvancedSettingsButton.UseVisualStyleBackColor = true;
            this.AdvancedSettingsButton.Click += new System.EventHandler(this.AdvancedSettingsButton_Click);
            // 
            // SaveSettingsButton
            // 
            this.SaveSettingsButton.Location = new System.Drawing.Point(103, 551);
            this.SaveSettingsButton.Name = "SaveSettingsButton";
            this.SaveSettingsButton.Size = new System.Drawing.Size(93, 23);
            this.SaveSettingsButton.TabIndex = 147;
            this.SaveSettingsButton.Text = "Save settings";
            this.SaveSettingsButton.UseVisualStyleBackColor = true;
            this.SaveSettingsButton.Click += new System.EventHandler(this.SaveSettingsButton_Click);
            // 
            // ControlSettingsButton
            // 
            this.ControlSettingsButton.Location = new System.Drawing.Point(4, 522);
            this.ControlSettingsButton.Name = "ControlSettingsButton";
            this.ControlSettingsButton.Size = new System.Drawing.Size(93, 23);
            this.ControlSettingsButton.TabIndex = 45;
            this.ControlSettingsButton.Text = "Controls...";
            this.ControlSettingsButton.UseVisualStyleBackColor = true;
            this.ControlSettingsButton.Click += new System.EventHandler(this.ControlSettingsButton_Click);
            // 
            // MapViewDetailLabel
            // 
            this.MapViewDetailLabel.AutoSize = true;
            this.MapViewDetailLabel.Location = new System.Drawing.Point(94, 391);
            this.MapViewDetailLabel.Name = "MapViewDetailLabel";
            this.MapViewDetailLabel.Size = new System.Drawing.Size(22, 13);
            this.MapViewDetailLabel.TabIndex = 66;
            this.MapViewDetailLabel.Text = "1.0";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(4, 391);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(84, 13);
            this.label28.TabIndex = 65;
            this.label28.Text = "Map view detail:";
            // 
            // MapViewDetailTrackBar
            // 
            this.MapViewDetailTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MapViewDetailTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.MapViewDetailTrackBar.Enabled = false;
            this.MapViewDetailTrackBar.LargeChange = 1;
            this.MapViewDetailTrackBar.Location = new System.Drawing.Point(6, 407);
            this.MapViewDetailTrackBar.Maximum = 30;
            this.MapViewDetailTrackBar.Minimum = 2;
            this.MapViewDetailTrackBar.Name = "MapViewDetailTrackBar";
            this.MapViewDetailTrackBar.Size = new System.Drawing.Size(188, 45);
            this.MapViewDetailTrackBar.TabIndex = 44;
            this.MapViewDetailTrackBar.TickFrequency = 2;
            this.MapViewDetailTrackBar.Value = 10;
            this.MapViewDetailTrackBar.Scroll += new System.EventHandler(this.MapViewDetailTrackBar_Scroll);
            // 
            // CameraModeComboBox
            // 
            this.CameraModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CameraModeComboBox.FormattingEnabled = true;
            this.CameraModeComboBox.Items.AddRange(new object[] {
            "Perspective",
            "Orthographic",
            "2D Map"});
            this.CameraModeComboBox.Location = new System.Drawing.Point(82, 305);
            this.CameraModeComboBox.Name = "CameraModeComboBox";
            this.CameraModeComboBox.Size = new System.Drawing.Size(112, 21);
            this.CameraModeComboBox.TabIndex = 42;
            this.CameraModeComboBox.SelectedIndexChanged += new System.EventHandler(this.CameraModeComboBox_SelectedIndexChanged);
            this.CameraModeComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CameraModeComboBox_KeyPress);
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(4, 308);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(75, 13);
            this.label24.TabIndex = 63;
            this.label24.Text = "Camera mode:";
            // 
            // WaterQuadsCheckBox
            // 
            this.WaterQuadsCheckBox.AutoSize = true;
            this.WaterQuadsCheckBox.Checked = true;
            this.WaterQuadsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.WaterQuadsCheckBox.Location = new System.Drawing.Point(10, 135);
            this.WaterQuadsCheckBox.Name = "WaterQuadsCheckBox";
            this.WaterQuadsCheckBox.Size = new System.Drawing.Size(114, 17);
            this.WaterQuadsCheckBox.TabIndex = 35;
            this.WaterQuadsCheckBox.Text = "Show water quads";
            this.WaterQuadsCheckBox.UseVisualStyleBackColor = true;
            this.WaterQuadsCheckBox.CheckedChanged += new System.EventHandler(this.WaterQuadsCheckBox_CheckedChanged);
            // 
            // FieldOfViewLabel
            // 
            this.FieldOfViewLabel.AutoSize = true;
            this.FieldOfViewLabel.Location = new System.Drawing.Point(79, 335);
            this.FieldOfViewLabel.Name = "FieldOfViewLabel";
            this.FieldOfViewLabel.Size = new System.Drawing.Size(22, 13);
            this.FieldOfViewLabel.TabIndex = 59;
            this.FieldOfViewLabel.Text = "1.0";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(4, 335);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(69, 13);
            this.label22.TabIndex = 58;
            this.label22.Text = "Field of view:";
            // 
            // TimedEntitiesAlwaysOnCheckBox
            // 
            this.TimedEntitiesAlwaysOnCheckBox.AutoSize = true;
            this.TimedEntitiesAlwaysOnCheckBox.Location = new System.Drawing.Point(131, 93);
            this.TimedEntitiesAlwaysOnCheckBox.Name = "TimedEntitiesAlwaysOnCheckBox";
            this.TimedEntitiesAlwaysOnCheckBox.Size = new System.Drawing.Size(58, 17);
            this.TimedEntitiesAlwaysOnCheckBox.TabIndex = 33;
            this.TimedEntitiesAlwaysOnCheckBox.Text = "always";
            this.TimedEntitiesAlwaysOnCheckBox.UseVisualStyleBackColor = true;
            this.TimedEntitiesAlwaysOnCheckBox.CheckedChanged += new System.EventHandler(this.TimedEntitiesAlwaysOnCheckBox_CheckedChanged);
            // 
            // GrassCheckBox
            // 
            this.GrassCheckBox.AutoSize = true;
            this.GrassCheckBox.Checked = true;
            this.GrassCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.GrassCheckBox.Location = new System.Drawing.Point(10, 51);
            this.GrassCheckBox.Name = "GrassCheckBox";
            this.GrassCheckBox.Size = new System.Drawing.Size(81, 17);
            this.GrassCheckBox.TabIndex = 30;
            this.GrassCheckBox.Text = "Show grass";
            this.GrassCheckBox.UseVisualStyleBackColor = true;
            this.GrassCheckBox.CheckedChanged += new System.EventHandler(this.GrassCheckBox_CheckedChanged);
            // 
            // InteriorsCheckBox
            // 
            this.InteriorsCheckBox.AutoSize = true;
            this.InteriorsCheckBox.Checked = true;
            this.InteriorsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.InteriorsCheckBox.Location = new System.Drawing.Point(10, 114);
            this.InteriorsCheckBox.Name = "InteriorsCheckBox";
            this.InteriorsCheckBox.Size = new System.Drawing.Size(92, 17);
            this.InteriorsCheckBox.TabIndex = 34;
            this.InteriorsCheckBox.Text = "Show interiors";
            this.InteriorsCheckBox.UseVisualStyleBackColor = true;
            this.InteriorsCheckBox.CheckedChanged += new System.EventHandler(this.InteriorsCheckBox_CheckedChanged);
            // 
            // CollisionMeshLayerDrawableCheckBox
            // 
            this.CollisionMeshLayerDrawableCheckBox.AutoSize = true;
            this.CollisionMeshLayerDrawableCheckBox.Checked = true;
            this.CollisionMeshLayerDrawableCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CollisionMeshLayerDrawableCheckBox.Location = new System.Drawing.Point(118, 262);
            this.CollisionMeshLayerDrawableCheckBox.Name = "CollisionMeshLayerDrawableCheckBox";
            this.CollisionMeshLayerDrawableCheckBox.Size = new System.Drawing.Size(71, 17);
            this.CollisionMeshLayerDrawableCheckBox.TabIndex = 41;
            this.CollisionMeshLayerDrawableCheckBox.Text = "Drawable";
            this.CollisionMeshLayerDrawableCheckBox.UseVisualStyleBackColor = true;
            this.CollisionMeshLayerDrawableCheckBox.CheckedChanged += new System.EventHandler(this.CollisionMeshLayerDrawableCheckBox_CheckedChanged);
            // 
            // CollisionMeshLayer2CheckBox
            // 
            this.CollisionMeshLayer2CheckBox.AutoSize = true;
            this.CollisionMeshLayer2CheckBox.Checked = true;
            this.CollisionMeshLayer2CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CollisionMeshLayer2CheckBox.Location = new System.Drawing.Point(82, 262);
            this.CollisionMeshLayer2CheckBox.Name = "CollisionMeshLayer2CheckBox";
            this.CollisionMeshLayer2CheckBox.Size = new System.Drawing.Size(32, 17);
            this.CollisionMeshLayer2CheckBox.TabIndex = 40;
            this.CollisionMeshLayer2CheckBox.Text = "2";
            this.CollisionMeshLayer2CheckBox.UseVisualStyleBackColor = true;
            this.CollisionMeshLayer2CheckBox.CheckedChanged += new System.EventHandler(this.CollisionMeshLayer2CheckBox_CheckedChanged);
            // 
            // CollisionMeshLayer1CheckBox
            // 
            this.CollisionMeshLayer1CheckBox.AutoSize = true;
            this.CollisionMeshLayer1CheckBox.Checked = true;
            this.CollisionMeshLayer1CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CollisionMeshLayer1CheckBox.Location = new System.Drawing.Point(46, 262);
            this.CollisionMeshLayer1CheckBox.Name = "CollisionMeshLayer1CheckBox";
            this.CollisionMeshLayer1CheckBox.Size = new System.Drawing.Size(32, 17);
            this.CollisionMeshLayer1CheckBox.TabIndex = 39;
            this.CollisionMeshLayer1CheckBox.Text = "1";
            this.CollisionMeshLayer1CheckBox.UseVisualStyleBackColor = true;
            this.CollisionMeshLayer1CheckBox.CheckedChanged += new System.EventHandler(this.CollisionMeshLayer1CheckBox_CheckedChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(4, 244);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(106, 13);
            this.label13.TabIndex = 54;
            this.label13.Text = "Collision mesh layers:";
            // 
            // CollisionMeshLayer0CheckBox
            // 
            this.CollisionMeshLayer0CheckBox.AutoSize = true;
            this.CollisionMeshLayer0CheckBox.Checked = true;
            this.CollisionMeshLayer0CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CollisionMeshLayer0CheckBox.Location = new System.Drawing.Point(10, 262);
            this.CollisionMeshLayer0CheckBox.Name = "CollisionMeshLayer0CheckBox";
            this.CollisionMeshLayer0CheckBox.Size = new System.Drawing.Size(32, 17);
            this.CollisionMeshLayer0CheckBox.TabIndex = 38;
            this.CollisionMeshLayer0CheckBox.Text = "0";
            this.CollisionMeshLayer0CheckBox.UseVisualStyleBackColor = true;
            this.CollisionMeshLayer0CheckBox.CheckedChanged += new System.EventHandler(this.CollisionMeshLayer0CheckBox_CheckedChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(4, 193);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(129, 13);
            this.label12.TabIndex = 51;
            this.label12.Text = "Collision/nav mesh range:";
            // 
            // CollisionMeshRangeTrackBar
            // 
            this.CollisionMeshRangeTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CollisionMeshRangeTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.CollisionMeshRangeTrackBar.LargeChange = 1;
            this.CollisionMeshRangeTrackBar.Location = new System.Drawing.Point(6, 209);
            this.CollisionMeshRangeTrackBar.Maximum = 15;
            this.CollisionMeshRangeTrackBar.Minimum = 1;
            this.CollisionMeshRangeTrackBar.Name = "CollisionMeshRangeTrackBar";
            this.CollisionMeshRangeTrackBar.Size = new System.Drawing.Size(188, 45);
            this.CollisionMeshRangeTrackBar.TabIndex = 37;
            this.CollisionMeshRangeTrackBar.Value = 6;
            this.CollisionMeshRangeTrackBar.Scroll += new System.EventHandler(this.CollisionMeshRangeTrackBar_Scroll);
            // 
            // CollisionMeshesCheckBox
            // 
            this.CollisionMeshesCheckBox.AutoSize = true;
            this.CollisionMeshesCheckBox.Location = new System.Drawing.Point(10, 171);
            this.CollisionMeshesCheckBox.Name = "CollisionMeshesCheckBox";
            this.CollisionMeshesCheckBox.Size = new System.Drawing.Size(132, 17);
            this.CollisionMeshesCheckBox.TabIndex = 36;
            this.CollisionMeshesCheckBox.Text = "Show collision meshes";
            this.CollisionMeshesCheckBox.UseVisualStyleBackColor = true;
            this.CollisionMeshesCheckBox.CheckedChanged += new System.EventHandler(this.CollisionMeshesCheckBox_CheckedChanged);
            // 
            // FullScreenCheckBox
            // 
            this.FullScreenCheckBox.AutoSize = true;
            this.FullScreenCheckBox.Location = new System.Drawing.Point(10, 9);
            this.FullScreenCheckBox.Name = "FullScreenCheckBox";
            this.FullScreenCheckBox.Size = new System.Drawing.Size(173, 17);
            this.FullScreenCheckBox.TabIndex = 28;
            this.FullScreenCheckBox.Text = "Full screen (borderless window)";
            this.FullScreenCheckBox.UseVisualStyleBackColor = true;
            this.FullScreenCheckBox.CheckedChanged += new System.EventHandler(this.FullScreenCheckBox_CheckedChanged);
            // 
            // TimedEntitiesCheckBox
            // 
            this.TimedEntitiesCheckBox.AutoSize = true;
            this.TimedEntitiesCheckBox.Checked = true;
            this.TimedEntitiesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TimedEntitiesCheckBox.Location = new System.Drawing.Point(10, 93);
            this.TimedEntitiesCheckBox.Name = "TimedEntitiesCheckBox";
            this.TimedEntitiesCheckBox.Size = new System.Drawing.Size(117, 17);
            this.TimedEntitiesCheckBox.TabIndex = 32;
            this.TimedEntitiesCheckBox.Text = "Show timed entities";
            this.TimedEntitiesCheckBox.UseVisualStyleBackColor = true;
            this.TimedEntitiesCheckBox.CheckedChanged += new System.EventHandler(this.TimedEntitiesCheckBox_CheckedChanged);
            // 
            // FieldOfViewTrackBar
            // 
            this.FieldOfViewTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FieldOfViewTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.FieldOfViewTrackBar.LargeChange = 1;
            this.FieldOfViewTrackBar.Location = new System.Drawing.Point(6, 351);
            this.FieldOfViewTrackBar.Maximum = 200;
            this.FieldOfViewTrackBar.Minimum = 10;
            this.FieldOfViewTrackBar.Name = "FieldOfViewTrackBar";
            this.FieldOfViewTrackBar.Size = new System.Drawing.Size(188, 45);
            this.FieldOfViewTrackBar.TabIndex = 43;
            this.FieldOfViewTrackBar.TickFrequency = 10;
            this.FieldOfViewTrackBar.Value = 100;
            this.FieldOfViewTrackBar.Scroll += new System.EventHandler(this.FieldOfViewTrackBar_Scroll);
            // 
            // OptionsRenderTabPage
            // 
            this.OptionsRenderTabPage.Controls.Add(this.FarClipUpDown);
            this.OptionsRenderTabPage.Controls.Add(this.label32);
            this.OptionsRenderTabPage.Controls.Add(this.NearClipUpDown);
            this.OptionsRenderTabPage.Controls.Add(this.label31);
            this.OptionsRenderTabPage.Controls.Add(this.HDTexturesCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.WireframeCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.RenderModeComboBox);
            this.OptionsRenderTabPage.Controls.Add(this.label11);
            this.OptionsRenderTabPage.Controls.Add(this.TextureSamplerComboBox);
            this.OptionsRenderTabPage.Controls.Add(this.TextureCoordsComboBox);
            this.OptionsRenderTabPage.Controls.Add(this.label10);
            this.OptionsRenderTabPage.Controls.Add(this.AnisotropicFilteringCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.ProxiesCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.WaitForChildrenCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.label14);
            this.OptionsRenderTabPage.Location = new System.Drawing.Point(4, 22);
            this.OptionsRenderTabPage.Name = "OptionsRenderTabPage";
            this.OptionsRenderTabPage.Size = new System.Drawing.Size(200, 577);
            this.OptionsRenderTabPage.TabIndex = 3;
            this.OptionsRenderTabPage.Text = "Render";
            this.OptionsRenderTabPage.UseVisualStyleBackColor = true;
            // 
            // FarClipUpDown
            // 
            this.FarClipUpDown.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.FarClipUpDown.Location = new System.Drawing.Point(80, 346);
            this.FarClipUpDown.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.FarClipUpDown.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.FarClipUpDown.Name = "FarClipUpDown";
            this.FarClipUpDown.Size = new System.Drawing.Size(114, 20);
            this.FarClipUpDown.TabIndex = 61;
            this.FarClipUpDown.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.FarClipUpDown.ValueChanged += new System.EventHandler(this.FarClipUpDown_ValueChanged);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(4, 348);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(45, 13);
            this.label32.TabIndex = 60;
            this.label32.Text = "Far Clip:";
            // 
            // NearClipUpDown
            // 
            this.NearClipUpDown.DecimalPlaces = 3;
            this.NearClipUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.NearClipUpDown.Location = new System.Drawing.Point(80, 320);
            this.NearClipUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.NearClipUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.NearClipUpDown.Name = "NearClipUpDown";
            this.NearClipUpDown.Size = new System.Drawing.Size(114, 20);
            this.NearClipUpDown.TabIndex = 59;
            this.NearClipUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.NearClipUpDown.ValueChanged += new System.EventHandler(this.NearClipUpDown_ValueChanged);
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(4, 322);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(53, 13);
            this.label31.TabIndex = 58;
            this.label31.Text = "Near Clip:";
            // 
            // HDTexturesCheckBox
            // 
            this.HDTexturesCheckBox.AutoSize = true;
            this.HDTexturesCheckBox.Checked = true;
            this.HDTexturesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.HDTexturesCheckBox.Location = new System.Drawing.Point(10, 231);
            this.HDTexturesCheckBox.Name = "HDTexturesCheckBox";
            this.HDTexturesCheckBox.Size = new System.Drawing.Size(82, 17);
            this.HDTexturesCheckBox.TabIndex = 57;
            this.HDTexturesCheckBox.Text = "HD textures";
            this.HDTexturesCheckBox.UseVisualStyleBackColor = true;
            this.HDTexturesCheckBox.CheckedChanged += new System.EventHandler(this.HDTexturesCheckBox_CheckedChanged);
            // 
            // WireframeCheckBox
            // 
            this.WireframeCheckBox.AutoSize = true;
            this.WireframeCheckBox.Location = new System.Drawing.Point(10, 115);
            this.WireframeCheckBox.Name = "WireframeCheckBox";
            this.WireframeCheckBox.Size = new System.Drawing.Size(74, 17);
            this.WireframeCheckBox.TabIndex = 49;
            this.WireframeCheckBox.Text = "Wireframe";
            this.WireframeCheckBox.UseVisualStyleBackColor = true;
            this.WireframeCheckBox.CheckedChanged += new System.EventHandler(this.WireframeCheckBox_CheckedChanged);
            // 
            // RenderModeComboBox
            // 
            this.RenderModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RenderModeComboBox.FormattingEnabled = true;
            this.RenderModeComboBox.Items.AddRange(new object[] {
            "Default",
            "Single texture",
            "Vertex normals",
            "Vertex tangents",
            "Vertex colour 1",
            "Vertex colour 2",
            "Texture coord 1",
            "Texture coord 2",
            "Texture coord 3"});
            this.RenderModeComboBox.Location = new System.Drawing.Point(80, 16);
            this.RenderModeComboBox.Name = "RenderModeComboBox";
            this.RenderModeComboBox.Size = new System.Drawing.Size(114, 21);
            this.RenderModeComboBox.TabIndex = 46;
            this.RenderModeComboBox.SelectedIndexChanged += new System.EventHandler(this.RenderModeComboBox_SelectedIndexChanged);
            this.RenderModeComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RenderModeComboBox_KeyPress);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(4, 46);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(67, 13);
            this.label11.TabIndex = 50;
            this.label11.Text = "Tex sampler:";
            // 
            // TextureSamplerComboBox
            // 
            this.TextureSamplerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TextureSamplerComboBox.Enabled = false;
            this.TextureSamplerComboBox.FormattingEnabled = true;
            this.TextureSamplerComboBox.Location = new System.Drawing.Point(80, 43);
            this.TextureSamplerComboBox.Name = "TextureSamplerComboBox";
            this.TextureSamplerComboBox.Size = new System.Drawing.Size(114, 21);
            this.TextureSamplerComboBox.TabIndex = 47;
            this.TextureSamplerComboBox.SelectedIndexChanged += new System.EventHandler(this.TextureSamplerComboBox_SelectedIndexChanged);
            this.TextureSamplerComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextureSamplerComboBox_KeyPress);
            // 
            // TextureCoordsComboBox
            // 
            this.TextureCoordsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TextureCoordsComboBox.Enabled = false;
            this.TextureCoordsComboBox.FormattingEnabled = true;
            this.TextureCoordsComboBox.Items.AddRange(new object[] {
            "Texture coord 1",
            "Texture coord 2",
            "Texture coord 3"});
            this.TextureCoordsComboBox.Location = new System.Drawing.Point(80, 70);
            this.TextureCoordsComboBox.Name = "TextureCoordsComboBox";
            this.TextureCoordsComboBox.Size = new System.Drawing.Size(114, 21);
            this.TextureCoordsComboBox.TabIndex = 48;
            this.TextureCoordsComboBox.SelectedIndexChanged += new System.EventHandler(this.TextureCoordsComboBox_SelectedIndexChanged);
            this.TextureCoordsComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextureCoordsComboBox_KeyPress);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 19);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(74, 13);
            this.label10.TabIndex = 48;
            this.label10.Text = "Render mode:";
            // 
            // AnisotropicFilteringCheckBox
            // 
            this.AnisotropicFilteringCheckBox.AutoSize = true;
            this.AnisotropicFilteringCheckBox.Checked = true;
            this.AnisotropicFilteringCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AnisotropicFilteringCheckBox.Location = new System.Drawing.Point(10, 138);
            this.AnisotropicFilteringCheckBox.Name = "AnisotropicFilteringCheckBox";
            this.AnisotropicFilteringCheckBox.Size = new System.Drawing.Size(114, 17);
            this.AnisotropicFilteringCheckBox.TabIndex = 50;
            this.AnisotropicFilteringCheckBox.Text = "Anisotropic filtering";
            this.AnisotropicFilteringCheckBox.UseVisualStyleBackColor = true;
            this.AnisotropicFilteringCheckBox.CheckedChanged += new System.EventHandler(this.AnisotropicFilteringCheckBox_CheckedChanged);
            // 
            // ProxiesCheckBox
            // 
            this.ProxiesCheckBox.AutoSize = true;
            this.ProxiesCheckBox.Location = new System.Drawing.Point(10, 199);
            this.ProxiesCheckBox.Name = "ProxiesCheckBox";
            this.ProxiesCheckBox.Size = new System.Drawing.Size(89, 17);
            this.ProxiesCheckBox.TabIndex = 52;
            this.ProxiesCheckBox.Text = "Show proxies";
            this.ProxiesCheckBox.UseVisualStyleBackColor = true;
            this.ProxiesCheckBox.CheckedChanged += new System.EventHandler(this.ProxiesCheckBox_CheckedChanged);
            // 
            // WaitForChildrenCheckBox
            // 
            this.WaitForChildrenCheckBox.AutoSize = true;
            this.WaitForChildrenCheckBox.Checked = true;
            this.WaitForChildrenCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.WaitForChildrenCheckBox.Location = new System.Drawing.Point(10, 161);
            this.WaitForChildrenCheckBox.Name = "WaitForChildrenCheckBox";
            this.WaitForChildrenCheckBox.Size = new System.Drawing.Size(138, 17);
            this.WaitForChildrenCheckBox.TabIndex = 51;
            this.WaitForChildrenCheckBox.Text = "Wait for children to load";
            this.WaitForChildrenCheckBox.UseVisualStyleBackColor = true;
            this.WaitForChildrenCheckBox.CheckedChanged += new System.EventHandler(this.WaitForChildrenCheckBox_CheckedChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(4, 73);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(63, 13);
            this.label14.TabIndex = 56;
            this.label14.Text = "Tex coords:";
            // 
            // OptionsHelpersTabPage
            // 
            this.OptionsHelpersTabPage.Controls.Add(this.SnapAngleUpDown);
            this.OptionsHelpersTabPage.Controls.Add(this.label33);
            this.OptionsHelpersTabPage.Controls.Add(this.SnapGridSizeUpDown);
            this.OptionsHelpersTabPage.Controls.Add(this.label26);
            this.OptionsHelpersTabPage.Controls.Add(this.SkeletonsCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.AudioOuterBoundsCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.PopZonesCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.NavMeshesCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.TrainPathsCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.PathsDepthClipCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.PathBoundsCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.SelectionWidgetCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.MarkerStyleComboBox);
            this.OptionsHelpersTabPage.Controls.Add(this.label4);
            this.OptionsHelpersTabPage.Controls.Add(this.LocatorStyleComboBox);
            this.OptionsHelpersTabPage.Controls.Add(this.label5);
            this.OptionsHelpersTabPage.Controls.Add(this.MarkerDepthClipCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.label9);
            this.OptionsHelpersTabPage.Controls.Add(this.PathsCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.SelectionBoundsCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.BoundsDepthClipCheckBox);
            this.OptionsHelpersTabPage.Controls.Add(this.BoundsRangeTrackBar);
            this.OptionsHelpersTabPage.Controls.Add(this.BoundsStyleComboBox);
            this.OptionsHelpersTabPage.Controls.Add(this.label8);
            this.OptionsHelpersTabPage.Location = new System.Drawing.Point(4, 22);
            this.OptionsHelpersTabPage.Name = "OptionsHelpersTabPage";
            this.OptionsHelpersTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.OptionsHelpersTabPage.Size = new System.Drawing.Size(200, 577);
            this.OptionsHelpersTabPage.TabIndex = 1;
            this.OptionsHelpersTabPage.Text = "Helpers";
            this.OptionsHelpersTabPage.UseVisualStyleBackColor = true;
            // 
            // SnapAngleUpDown
            // 
            this.SnapAngleUpDown.DecimalPlaces = 1;
            this.SnapAngleUpDown.Location = new System.Drawing.Point(98, 279);
            this.SnapAngleUpDown.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.SnapAngleUpDown.Name = "SnapAngleUpDown";
            this.SnapAngleUpDown.Size = new System.Drawing.Size(96, 20);
            this.SnapAngleUpDown.TabIndex = 32;
            this.SnapAngleUpDown.Value = new decimal(new int[] {
            50,
            0,
            0,
            65536});
            this.SnapAngleUpDown.ValueChanged += new System.EventHandler(this.SnapAngleUpDown_ValueChanged);
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(4, 281);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(91, 13);
            this.label33.TabIndex = 31;
            this.label33.Text = "Snap angle (deg):";
            // 
            // SnapGridSizeUpDown
            // 
            this.SnapGridSizeUpDown.DecimalPlaces = 2;
            this.SnapGridSizeUpDown.Location = new System.Drawing.Point(98, 253);
            this.SnapGridSizeUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.SnapGridSizeUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.SnapGridSizeUpDown.Name = "SnapGridSizeUpDown";
            this.SnapGridSizeUpDown.Size = new System.Drawing.Size(96, 20);
            this.SnapGridSizeUpDown.TabIndex = 30;
            this.SnapGridSizeUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            131072});
            this.SnapGridSizeUpDown.ValueChanged += new System.EventHandler(this.SnapGridSizeUpDown_ValueChanged);
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(4, 255);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(76, 13);
            this.label26.TabIndex = 29;
            this.label26.Text = "Snap grid size:";
            // 
            // SkeletonsCheckBox
            // 
            this.SkeletonsCheckBox.AutoSize = true;
            this.SkeletonsCheckBox.Location = new System.Drawing.Point(10, 411);
            this.SkeletonsCheckBox.Name = "SkeletonsCheckBox";
            this.SkeletonsCheckBox.Size = new System.Drawing.Size(101, 17);
            this.SkeletonsCheckBox.TabIndex = 38;
            this.SkeletonsCheckBox.Text = "Show skeletons";
            this.SkeletonsCheckBox.UseVisualStyleBackColor = true;
            this.SkeletonsCheckBox.CheckedChanged += new System.EventHandler(this.SkeletonsCheckBox_CheckedChanged);
            // 
            // AudioOuterBoundsCheckBox
            // 
            this.AudioOuterBoundsCheckBox.AutoSize = true;
            this.AudioOuterBoundsCheckBox.Checked = true;
            this.AudioOuterBoundsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AudioOuterBoundsCheckBox.Location = new System.Drawing.Point(10, 457);
            this.AudioOuterBoundsCheckBox.Name = "AudioOuterBoundsCheckBox";
            this.AudioOuterBoundsCheckBox.Size = new System.Drawing.Size(147, 17);
            this.AudioOuterBoundsCheckBox.TabIndex = 40;
            this.AudioOuterBoundsCheckBox.Text = "Show audio outer bounds";
            this.AudioOuterBoundsCheckBox.UseVisualStyleBackColor = true;
            this.AudioOuterBoundsCheckBox.CheckedChanged += new System.EventHandler(this.AudioOuterBoundsCheckBox_CheckedChanged);
            // 
            // PopZonesCheckBox
            // 
            this.PopZonesCheckBox.AutoSize = true;
            this.PopZonesCheckBox.Location = new System.Drawing.Point(10, 388);
            this.PopZonesCheckBox.Name = "PopZonesCheckBox";
            this.PopZonesCheckBox.Size = new System.Drawing.Size(136, 17);
            this.PopZonesCheckBox.TabIndex = 37;
            this.PopZonesCheckBox.Text = "Show population zones";
            this.PopZonesCheckBox.UseVisualStyleBackColor = true;
            this.PopZonesCheckBox.CheckedChanged += new System.EventHandler(this.PopZonesCheckBox_CheckedChanged);
            // 
            // NavMeshesCheckBox
            // 
            this.NavMeshesCheckBox.AutoSize = true;
            this.NavMeshesCheckBox.Location = new System.Drawing.Point(10, 365);
            this.NavMeshesCheckBox.Name = "NavMeshesCheckBox";
            this.NavMeshesCheckBox.Size = new System.Drawing.Size(113, 17);
            this.NavMeshesCheckBox.TabIndex = 36;
            this.NavMeshesCheckBox.Text = "Show nav meshes";
            this.NavMeshesCheckBox.UseVisualStyleBackColor = true;
            this.NavMeshesCheckBox.CheckedChanged += new System.EventHandler(this.NavMeshesCheckBox_CheckedChanged);
            // 
            // TrainPathsCheckBox
            // 
            this.TrainPathsCheckBox.AutoSize = true;
            this.TrainPathsCheckBox.Location = new System.Drawing.Point(10, 342);
            this.TrainPathsCheckBox.Name = "TrainPathsCheckBox";
            this.TrainPathsCheckBox.Size = new System.Drawing.Size(105, 17);
            this.TrainPathsCheckBox.TabIndex = 35;
            this.TrainPathsCheckBox.Text = "Show train paths";
            this.TrainPathsCheckBox.UseVisualStyleBackColor = true;
            this.TrainPathsCheckBox.CheckedChanged += new System.EventHandler(this.TrainPathsCheckBox_CheckedChanged);
            // 
            // PathsDepthClipCheckBox
            // 
            this.PathsDepthClipCheckBox.AutoSize = true;
            this.PathsDepthClipCheckBox.Checked = true;
            this.PathsDepthClipCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PathsDepthClipCheckBox.Location = new System.Drawing.Point(10, 434);
            this.PathsDepthClipCheckBox.Name = "PathsDepthClipCheckBox";
            this.PathsDepthClipCheckBox.Size = new System.Drawing.Size(102, 17);
            this.PathsDepthClipCheckBox.TabIndex = 39;
            this.PathsDepthClipCheckBox.Text = "Paths depth clip";
            this.PathsDepthClipCheckBox.UseVisualStyleBackColor = true;
            this.PathsDepthClipCheckBox.CheckedChanged += new System.EventHandler(this.PathsDepthClipCheckBox_CheckedChanged);
            // 
            // PathBoundsCheckBox
            // 
            this.PathBoundsCheckBox.AutoSize = true;
            this.PathBoundsCheckBox.Checked = true;
            this.PathBoundsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PathBoundsCheckBox.Location = new System.Drawing.Point(98, 319);
            this.PathBoundsCheckBox.Name = "PathBoundsCheckBox";
            this.PathBoundsCheckBox.Size = new System.Drawing.Size(86, 17);
            this.PathBoundsCheckBox.TabIndex = 34;
            this.PathBoundsCheckBox.Text = "Path bounds";
            this.PathBoundsCheckBox.UseVisualStyleBackColor = true;
            this.PathBoundsCheckBox.CheckedChanged += new System.EventHandler(this.PathBoundsCheckBox_CheckedChanged);
            // 
            // SelectionWidgetCheckBox
            // 
            this.SelectionWidgetCheckBox.AutoSize = true;
            this.SelectionWidgetCheckBox.Checked = true;
            this.SelectionWidgetCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SelectionWidgetCheckBox.Location = new System.Drawing.Point(10, 231);
            this.SelectionWidgetCheckBox.Name = "SelectionWidgetCheckBox";
            this.SelectionWidgetCheckBox.Size = new System.Drawing.Size(87, 17);
            this.SelectionWidgetCheckBox.TabIndex = 28;
            this.SelectionWidgetCheckBox.Text = "Show widget";
            this.SelectionWidgetCheckBox.UseVisualStyleBackColor = true;
            this.SelectionWidgetCheckBox.CheckedChanged += new System.EventHandler(this.SelectionWidgetCheckBox_CheckedChanged);
            // 
            // MarkerStyleComboBox
            // 
            this.MarkerStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MarkerStyleComboBox.FormattingEnabled = true;
            this.MarkerStyleComboBox.Location = new System.Drawing.Point(80, 6);
            this.MarkerStyleComboBox.Name = "MarkerStyleComboBox";
            this.MarkerStyleComboBox.Size = new System.Drawing.Size(114, 21);
            this.MarkerStyleComboBox.TabIndex = 18;
            this.MarkerStyleComboBox.SelectedIndexChanged += new System.EventHandler(this.MarkerStyleComboBox_SelectedIndexChanged);
            this.MarkerStyleComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MarkerStyleComboBox_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Marker style:";
            // 
            // LocatorStyleComboBox
            // 
            this.LocatorStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LocatorStyleComboBox.FormattingEnabled = true;
            this.LocatorStyleComboBox.Location = new System.Drawing.Point(80, 33);
            this.LocatorStyleComboBox.Name = "LocatorStyleComboBox";
            this.LocatorStyleComboBox.Size = new System.Drawing.Size(114, 21);
            this.LocatorStyleComboBox.TabIndex = 20;
            this.LocatorStyleComboBox.SelectedIndexChanged += new System.EventHandler(this.LocatorStyleComboBox_SelectedIndexChanged);
            this.LocatorStyleComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LocatorStyleComboBox_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 36);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Locator style:";
            // 
            // MarkerDepthClipCheckBox
            // 
            this.MarkerDepthClipCheckBox.AutoSize = true;
            this.MarkerDepthClipCheckBox.Location = new System.Drawing.Point(10, 60);
            this.MarkerDepthClipCheckBox.Name = "MarkerDepthClipCheckBox";
            this.MarkerDepthClipCheckBox.Size = new System.Drawing.Size(108, 17);
            this.MarkerDepthClipCheckBox.TabIndex = 21;
            this.MarkerDepthClipCheckBox.Text = "Marker depth clip";
            this.MarkerDepthClipCheckBox.UseVisualStyleBackColor = true;
            this.MarkerDepthClipCheckBox.Visible = false;
            this.MarkerDepthClipCheckBox.CheckedChanged += new System.EventHandler(this.MarkerDepthClipCheckBox_CheckedChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 136);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(76, 13);
            this.label9.TabIndex = 25;
            this.label9.Text = "Bounds range:";
            // 
            // PathsCheckBox
            // 
            this.PathsCheckBox.AutoSize = true;
            this.PathsCheckBox.Location = new System.Drawing.Point(10, 319);
            this.PathsCheckBox.Name = "PathsCheckBox";
            this.PathsCheckBox.Size = new System.Drawing.Size(82, 17);
            this.PathsCheckBox.TabIndex = 33;
            this.PathsCheckBox.Text = "Show paths";
            this.PathsCheckBox.UseVisualStyleBackColor = true;
            this.PathsCheckBox.CheckedChanged += new System.EventHandler(this.PathsCheckBox_CheckedChanged);
            // 
            // SelectionBoundsCheckBox
            // 
            this.SelectionBoundsCheckBox.AutoSize = true;
            this.SelectionBoundsCheckBox.Checked = true;
            this.SelectionBoundsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SelectionBoundsCheckBox.Location = new System.Drawing.Point(10, 194);
            this.SelectionBoundsCheckBox.Name = "SelectionBoundsCheckBox";
            this.SelectionBoundsCheckBox.Size = new System.Drawing.Size(136, 17);
            this.SelectionBoundsCheckBox.TabIndex = 27;
            this.SelectionBoundsCheckBox.Text = "Show selection bounds";
            this.SelectionBoundsCheckBox.UseVisualStyleBackColor = true;
            this.SelectionBoundsCheckBox.CheckedChanged += new System.EventHandler(this.SelectionBoundsCheckBox_CheckedChanged);
            // 
            // BoundsDepthClipCheckBox
            // 
            this.BoundsDepthClipCheckBox.AutoSize = true;
            this.BoundsDepthClipCheckBox.Checked = true;
            this.BoundsDepthClipCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.BoundsDepthClipCheckBox.Location = new System.Drawing.Point(10, 114);
            this.BoundsDepthClipCheckBox.Name = "BoundsDepthClipCheckBox";
            this.BoundsDepthClipCheckBox.Size = new System.Drawing.Size(111, 17);
            this.BoundsDepthClipCheckBox.TabIndex = 24;
            this.BoundsDepthClipCheckBox.Text = "Bounds depth clip";
            this.BoundsDepthClipCheckBox.UseVisualStyleBackColor = true;
            this.BoundsDepthClipCheckBox.CheckedChanged += new System.EventHandler(this.BoundsDepthClipCheckBox_CheckedChanged);
            // 
            // BoundsRangeTrackBar
            // 
            this.BoundsRangeTrackBar.AutoSize = false;
            this.BoundsRangeTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.BoundsRangeTrackBar.LargeChange = 10;
            this.BoundsRangeTrackBar.Location = new System.Drawing.Point(6, 152);
            this.BoundsRangeTrackBar.Maximum = 100;
            this.BoundsRangeTrackBar.Minimum = 1;
            this.BoundsRangeTrackBar.Name = "BoundsRangeTrackBar";
            this.BoundsRangeTrackBar.Size = new System.Drawing.Size(188, 33);
            this.BoundsRangeTrackBar.TabIndex = 26;
            this.BoundsRangeTrackBar.TickFrequency = 10;
            this.BoundsRangeTrackBar.Value = 100;
            this.BoundsRangeTrackBar.Scroll += new System.EventHandler(this.BoundsRangeTrackBar_Scroll);
            // 
            // BoundsStyleComboBox
            // 
            this.BoundsStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BoundsStyleComboBox.FormattingEnabled = true;
            this.BoundsStyleComboBox.Items.AddRange(new object[] {
            "None",
            "Boxes",
            "Spheres"});
            this.BoundsStyleComboBox.Location = new System.Drawing.Point(80, 87);
            this.BoundsStyleComboBox.Name = "BoundsStyleComboBox";
            this.BoundsStyleComboBox.Size = new System.Drawing.Size(114, 21);
            this.BoundsStyleComboBox.TabIndex = 23;
            this.BoundsStyleComboBox.SelectedIndexChanged += new System.EventHandler(this.BoundsStyleComboBox_SelectedIndexChanged);
            this.BoundsStyleComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BoundsStyleComboBox_KeyPress);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 90);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(70, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "Bounds style:";
            // 
            // OptionsLightingTabPage
            // 
            this.OptionsLightingTabPage.Controls.Add(this.HDLightsCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.DeferredShadingCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.WeatherRegionComboBox);
            this.OptionsLightingTabPage.Controls.Add(this.label29);
            this.OptionsLightingTabPage.Controls.Add(this.CloudParamTrackBar);
            this.OptionsLightingTabPage.Controls.Add(this.CloudParamComboBox);
            this.OptionsLightingTabPage.Controls.Add(this.label23);
            this.OptionsLightingTabPage.Controls.Add(this.CloudsComboBox);
            this.OptionsLightingTabPage.Controls.Add(this.label21);
            this.OptionsLightingTabPage.Controls.Add(this.TimeSpeedLabel);
            this.OptionsLightingTabPage.Controls.Add(this.label20);
            this.OptionsLightingTabPage.Controls.Add(this.TimeSpeedTrackBar);
            this.OptionsLightingTabPage.Controls.Add(this.TimeStartStopButton);
            this.OptionsLightingTabPage.Controls.Add(this.ArtificialAmbientLightCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.NaturalAmbientLightCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.LODLightsCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.HDRRenderingCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.ControlTimeOfDayCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.TimeOfDayLabel);
            this.OptionsLightingTabPage.Controls.Add(this.label19);
            this.OptionsLightingTabPage.Controls.Add(this.TimeOfDayTrackBar);
            this.OptionsLightingTabPage.Controls.Add(this.WeatherComboBox);
            this.OptionsLightingTabPage.Controls.Add(this.label17);
            this.OptionsLightingTabPage.Controls.Add(this.ControlLightDirectionCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.SkydomeCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.ShadowsCheckBox);
            this.OptionsLightingTabPage.Location = new System.Drawing.Point(4, 22);
            this.OptionsLightingTabPage.Name = "OptionsLightingTabPage";
            this.OptionsLightingTabPage.Size = new System.Drawing.Size(200, 577);
            this.OptionsLightingTabPage.TabIndex = 2;
            this.OptionsLightingTabPage.Text = "Lighting";
            this.OptionsLightingTabPage.UseVisualStyleBackColor = true;
            // 
            // HDLightsCheckBox
            // 
            this.HDLightsCheckBox.AutoSize = true;
            this.HDLightsCheckBox.Checked = true;
            this.HDLightsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.HDLightsCheckBox.Location = new System.Drawing.Point(10, 93);
            this.HDLightsCheckBox.Name = "HDLightsCheckBox";
            this.HDLightsCheckBox.Size = new System.Drawing.Size(69, 17);
            this.HDLightsCheckBox.TabIndex = 34;
            this.HDLightsCheckBox.Text = "HD lights";
            this.HDLightsCheckBox.UseVisualStyleBackColor = true;
            this.HDLightsCheckBox.CheckedChanged += new System.EventHandler(this.HDLightsCheckBox_CheckedChanged);
            // 
            // DeferredShadingCheckBox
            // 
            this.DeferredShadingCheckBox.AutoSize = true;
            this.DeferredShadingCheckBox.Checked = true;
            this.DeferredShadingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DeferredShadingCheckBox.Location = new System.Drawing.Point(10, 5);
            this.DeferredShadingCheckBox.Name = "DeferredShadingCheckBox";
            this.DeferredShadingCheckBox.Size = new System.Drawing.Size(107, 17);
            this.DeferredShadingCheckBox.TabIndex = 30;
            this.DeferredShadingCheckBox.Text = "Deferred shading";
            this.DeferredShadingCheckBox.UseVisualStyleBackColor = true;
            this.DeferredShadingCheckBox.CheckedChanged += new System.EventHandler(this.DeferredShadingCheckBox_CheckedChanged);
            // 
            // WeatherRegionComboBox
            // 
            this.WeatherRegionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WeatherRegionComboBox.FormattingEnabled = true;
            this.WeatherRegionComboBox.Items.AddRange(new object[] {
            "GLOBAL",
            "URBAN"});
            this.WeatherRegionComboBox.Location = new System.Drawing.Point(61, 355);
            this.WeatherRegionComboBox.Name = "WeatherRegionComboBox";
            this.WeatherRegionComboBox.Size = new System.Drawing.Size(133, 21);
            this.WeatherRegionComboBox.TabIndex = 50;
            this.WeatherRegionComboBox.SelectedIndexChanged += new System.EventHandler(this.WeatherRegionComboBox_SelectedIndexChanged);
            this.WeatherRegionComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.WeatherRegionComboBox_KeyPress);
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(4, 358);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(44, 13);
            this.label29.TabIndex = 49;
            this.label29.Text = "Region:";
            // 
            // CloudParamTrackBar
            // 
            this.CloudParamTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CloudParamTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.CloudParamTrackBar.LargeChange = 10;
            this.CloudParamTrackBar.Location = new System.Drawing.Point(6, 436);
            this.CloudParamTrackBar.Maximum = 200;
            this.CloudParamTrackBar.Name = "CloudParamTrackBar";
            this.CloudParamTrackBar.Size = new System.Drawing.Size(188, 45);
            this.CloudParamTrackBar.TabIndex = 55;
            this.CloudParamTrackBar.TickFrequency = 10;
            this.CloudParamTrackBar.Value = 100;
            this.CloudParamTrackBar.Scroll += new System.EventHandler(this.CloudParamTrackBar_Scroll);
            // 
            // CloudParamComboBox
            // 
            this.CloudParamComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CloudParamComboBox.FormattingEnabled = true;
            this.CloudParamComboBox.Items.AddRange(new object[] {
            "<Loading...>"});
            this.CloudParamComboBox.Location = new System.Drawing.Point(78, 409);
            this.CloudParamComboBox.Name = "CloudParamComboBox";
            this.CloudParamComboBox.Size = new System.Drawing.Size(116, 21);
            this.CloudParamComboBox.TabIndex = 54;
            this.CloudParamComboBox.SelectedIndexChanged += new System.EventHandler(this.CloudParamComboBox_SelectedIndexChanged);
            this.CloudParamComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CloudParamComboBox_KeyPress);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(4, 412);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(69, 13);
            this.label23.TabIndex = 53;
            this.label23.Text = "Cloud param:";
            // 
            // CloudsComboBox
            // 
            this.CloudsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CloudsComboBox.FormattingEnabled = true;
            this.CloudsComboBox.Items.AddRange(new object[] {
            "<Loading...>"});
            this.CloudsComboBox.Location = new System.Drawing.Point(61, 382);
            this.CloudsComboBox.Name = "CloudsComboBox";
            this.CloudsComboBox.Size = new System.Drawing.Size(133, 21);
            this.CloudsComboBox.TabIndex = 52;
            this.CloudsComboBox.SelectedIndexChanged += new System.EventHandler(this.CloudsComboBox_SelectedIndexChanged);
            this.CloudsComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CloudsComboBox_KeyPress);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(4, 385);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(42, 13);
            this.label21.TabIndex = 51;
            this.label21.Text = "Clouds:";
            // 
            // TimeSpeedLabel
            // 
            this.TimeSpeedLabel.AutoSize = true;
            this.TimeSpeedLabel.Location = new System.Drawing.Point(78, 263);
            this.TimeSpeedLabel.Name = "TimeSpeedLabel";
            this.TimeSpeedLabel.Size = new System.Drawing.Size(63, 13);
            this.TimeSpeedLabel.TabIndex = 44;
            this.TimeSpeedLabel.Text = "0.5 min/sec";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(3, 263);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(65, 13);
            this.label20.TabIndex = 43;
            this.label20.Text = "Time speed:";
            // 
            // TimeSpeedTrackBar
            // 
            this.TimeSpeedTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TimeSpeedTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.TimeSpeedTrackBar.Location = new System.Drawing.Point(61, 279);
            this.TimeSpeedTrackBar.Maximum = 100;
            this.TimeSpeedTrackBar.Minimum = 40;
            this.TimeSpeedTrackBar.Name = "TimeSpeedTrackBar";
            this.TimeSpeedTrackBar.Size = new System.Drawing.Size(133, 45);
            this.TimeSpeedTrackBar.TabIndex = 46;
            this.TimeSpeedTrackBar.TickFrequency = 5;
            this.TimeSpeedTrackBar.Value = 50;
            this.TimeSpeedTrackBar.Scroll += new System.EventHandler(this.TimeSpeedTrackBar_Scroll);
            // 
            // TimeStartStopButton
            // 
            this.TimeStartStopButton.Location = new System.Drawing.Point(10, 279);
            this.TimeStartStopButton.Name = "TimeStartStopButton";
            this.TimeStartStopButton.Size = new System.Drawing.Size(45, 23);
            this.TimeStartStopButton.TabIndex = 45;
            this.TimeStartStopButton.Text = "Start";
            this.TimeStartStopButton.UseVisualStyleBackColor = true;
            this.TimeStartStopButton.Click += new System.EventHandler(this.TimeStartStopButton_Click);
            // 
            // ArtificialAmbientLightCheckBox
            // 
            this.ArtificialAmbientLightCheckBox.AutoSize = true;
            this.ArtificialAmbientLightCheckBox.Checked = true;
            this.ArtificialAmbientLightCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ArtificialAmbientLightCheckBox.Location = new System.Drawing.Point(10, 137);
            this.ArtificialAmbientLightCheckBox.Name = "ArtificialAmbientLightCheckBox";
            this.ArtificialAmbientLightCheckBox.Size = new System.Drawing.Size(124, 17);
            this.ArtificialAmbientLightCheckBox.TabIndex = 37;
            this.ArtificialAmbientLightCheckBox.Text = "Artificial ambient light";
            this.ArtificialAmbientLightCheckBox.UseVisualStyleBackColor = true;
            this.ArtificialAmbientLightCheckBox.CheckedChanged += new System.EventHandler(this.ArtificialAmbientLightCheckBox_CheckedChanged);
            // 
            // NaturalAmbientLightCheckBox
            // 
            this.NaturalAmbientLightCheckBox.AutoSize = true;
            this.NaturalAmbientLightCheckBox.Checked = true;
            this.NaturalAmbientLightCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.NaturalAmbientLightCheckBox.Location = new System.Drawing.Point(10, 115);
            this.NaturalAmbientLightCheckBox.Name = "NaturalAmbientLightCheckBox";
            this.NaturalAmbientLightCheckBox.Size = new System.Drawing.Size(122, 17);
            this.NaturalAmbientLightCheckBox.TabIndex = 36;
            this.NaturalAmbientLightCheckBox.Text = "Natural ambient light";
            this.NaturalAmbientLightCheckBox.UseVisualStyleBackColor = true;
            this.NaturalAmbientLightCheckBox.CheckedChanged += new System.EventHandler(this.NaturalAmbientLightCheckBox_CheckedChanged);
            // 
            // LODLightsCheckBox
            // 
            this.LODLightsCheckBox.AutoSize = true;
            this.LODLightsCheckBox.Checked = true;
            this.LODLightsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.LODLightsCheckBox.Location = new System.Drawing.Point(89, 93);
            this.LODLightsCheckBox.Name = "LODLightsCheckBox";
            this.LODLightsCheckBox.Size = new System.Drawing.Size(75, 17);
            this.LODLightsCheckBox.TabIndex = 35;
            this.LODLightsCheckBox.Text = "LOD lights";
            this.LODLightsCheckBox.UseVisualStyleBackColor = true;
            this.LODLightsCheckBox.CheckedChanged += new System.EventHandler(this.LODLightsCheckBox_CheckedChanged);
            // 
            // HDRRenderingCheckBox
            // 
            this.HDRRenderingCheckBox.AutoSize = true;
            this.HDRRenderingCheckBox.Checked = true;
            this.HDRRenderingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.HDRRenderingCheckBox.Location = new System.Drawing.Point(10, 27);
            this.HDRRenderingCheckBox.Name = "HDRRenderingCheckBox";
            this.HDRRenderingCheckBox.Size = new System.Drawing.Size(97, 17);
            this.HDRRenderingCheckBox.TabIndex = 31;
            this.HDRRenderingCheckBox.Text = "HDR rendering";
            this.HDRRenderingCheckBox.UseVisualStyleBackColor = true;
            this.HDRRenderingCheckBox.CheckedChanged += new System.EventHandler(this.HDRRenderingCheckBox_CheckedChanged);
            // 
            // ControlTimeOfDayCheckBox
            // 
            this.ControlTimeOfDayCheckBox.AutoSize = true;
            this.ControlTimeOfDayCheckBox.Checked = true;
            this.ControlTimeOfDayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ControlTimeOfDayCheckBox.Location = new System.Drawing.Point(10, 181);
            this.ControlTimeOfDayCheckBox.Name = "ControlTimeOfDayCheckBox";
            this.ControlTimeOfDayCheckBox.Size = new System.Drawing.Size(166, 17);
            this.ControlTimeOfDayCheckBox.TabIndex = 39;
            this.ControlTimeOfDayCheckBox.Text = "Control time of day (right-drag)";
            this.ControlTimeOfDayCheckBox.UseVisualStyleBackColor = true;
            this.ControlTimeOfDayCheckBox.CheckedChanged += new System.EventHandler(this.ControlTimeOfDayCheckBox_CheckedChanged);
            // 
            // TimeOfDayLabel
            // 
            this.TimeOfDayLabel.AutoSize = true;
            this.TimeOfDayLabel.Location = new System.Drawing.Point(75, 208);
            this.TimeOfDayLabel.Name = "TimeOfDayLabel";
            this.TimeOfDayLabel.Size = new System.Drawing.Size(34, 13);
            this.TimeOfDayLabel.TabIndex = 41;
            this.TimeOfDayLabel.Text = "12:00";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(4, 208);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(65, 13);
            this.label19.TabIndex = 40;
            this.label19.Text = "Time of day:";
            // 
            // TimeOfDayTrackBar
            // 
            this.TimeOfDayTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TimeOfDayTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.TimeOfDayTrackBar.LargeChange = 60;
            this.TimeOfDayTrackBar.Location = new System.Drawing.Point(6, 224);
            this.TimeOfDayTrackBar.Maximum = 1440;
            this.TimeOfDayTrackBar.Name = "TimeOfDayTrackBar";
            this.TimeOfDayTrackBar.Size = new System.Drawing.Size(188, 45);
            this.TimeOfDayTrackBar.TabIndex = 42;
            this.TimeOfDayTrackBar.TickFrequency = 60;
            this.TimeOfDayTrackBar.Value = 720;
            this.TimeOfDayTrackBar.Scroll += new System.EventHandler(this.TimeOfDayTrackBar_Scroll);
            // 
            // WeatherComboBox
            // 
            this.WeatherComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WeatherComboBox.FormattingEnabled = true;
            this.WeatherComboBox.Items.AddRange(new object[] {
            "<Loading...>"});
            this.WeatherComboBox.Location = new System.Drawing.Point(61, 328);
            this.WeatherComboBox.Name = "WeatherComboBox";
            this.WeatherComboBox.Size = new System.Drawing.Size(133, 21);
            this.WeatherComboBox.TabIndex = 48;
            this.WeatherComboBox.SelectedIndexChanged += new System.EventHandler(this.WeatherComboBox_SelectedIndexChanged);
            this.WeatherComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.WeatherComboBox_KeyPress);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(4, 331);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(51, 13);
            this.label17.TabIndex = 47;
            this.label17.Text = "Weather:";
            // 
            // ControlLightDirectionCheckBox
            // 
            this.ControlLightDirectionCheckBox.AutoSize = true;
            this.ControlLightDirectionCheckBox.Location = new System.Drawing.Point(10, 159);
            this.ControlLightDirectionCheckBox.Name = "ControlLightDirectionCheckBox";
            this.ControlLightDirectionCheckBox.Size = new System.Drawing.Size(177, 17);
            this.ControlLightDirectionCheckBox.TabIndex = 38;
            this.ControlLightDirectionCheckBox.Text = "Control light direction (right-drag)";
            this.ControlLightDirectionCheckBox.UseVisualStyleBackColor = true;
            this.ControlLightDirectionCheckBox.CheckedChanged += new System.EventHandler(this.ControlLightDirectionCheckBox_CheckedChanged);
            // 
            // SkydomeCheckBox
            // 
            this.SkydomeCheckBox.AutoSize = true;
            this.SkydomeCheckBox.Checked = true;
            this.SkydomeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SkydomeCheckBox.Location = new System.Drawing.Point(10, 71);
            this.SkydomeCheckBox.Name = "SkydomeCheckBox";
            this.SkydomeCheckBox.Size = new System.Drawing.Size(70, 17);
            this.SkydomeCheckBox.TabIndex = 33;
            this.SkydomeCheckBox.Text = "Skydome";
            this.SkydomeCheckBox.UseVisualStyleBackColor = true;
            this.SkydomeCheckBox.CheckedChanged += new System.EventHandler(this.SkydomeCheckbox_CheckedChanged);
            // 
            // ShadowsCheckBox
            // 
            this.ShadowsCheckBox.AutoSize = true;
            this.ShadowsCheckBox.Location = new System.Drawing.Point(10, 49);
            this.ShadowsCheckBox.Name = "ShadowsCheckBox";
            this.ShadowsCheckBox.Size = new System.Drawing.Size(70, 17);
            this.ShadowsCheckBox.TabIndex = 32;
            this.ShadowsCheckBox.Text = "Shadows";
            this.ShadowsCheckBox.UseVisualStyleBackColor = true;
            this.ShadowsCheckBox.CheckedChanged += new System.EventHandler(this.ShadowsCheckBox_CheckedChanged);
            // 
            // ToolsPanelHideButton
            // 
            this.ToolsPanelHideButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolsPanelHideButton.Location = new System.Drawing.Point(185, 3);
            this.ToolsPanelHideButton.Name = "ToolsPanelHideButton";
            this.ToolsPanelHideButton.Size = new System.Drawing.Size(30, 23);
            this.ToolsPanelHideButton.TabIndex = 4;
            this.ToolsPanelHideButton.Text = ">>";
            this.ToolsPanelHideButton.UseVisualStyleBackColor = true;
            this.ToolsPanelHideButton.Click += new System.EventHandler(this.ToolsPanelHideButton_Click);
            // 
            // ToolsPanelShowButton
            // 
            this.ToolsPanelShowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolsPanelShowButton.Location = new System.Drawing.Point(939, 15);
            this.ToolsPanelShowButton.Name = "ToolsPanelShowButton";
            this.ToolsPanelShowButton.Size = new System.Drawing.Size(30, 23);
            this.ToolsPanelShowButton.TabIndex = 0;
            this.ToolsPanelShowButton.Text = "<<";
            this.ToolsPanelShowButton.UseVisualStyleBackColor = true;
            this.ToolsPanelShowButton.Click += new System.EventHandler(this.ToolsPanelShowButton_Click);
            // 
            // ConsolePanel
            // 
            this.ConsolePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConsolePanel.BackColor = System.Drawing.SystemColors.Control;
            this.ConsolePanel.Controls.Add(this.ConsoleTextBox);
            this.ConsolePanel.Location = new System.Drawing.Point(12, 576);
            this.ConsolePanel.Name = "ConsolePanel";
            this.ConsolePanel.Size = new System.Drawing.Size(736, 101);
            this.ConsolePanel.TabIndex = 3;
            this.ConsolePanel.Visible = false;
            // 
            // ConsoleTextBox
            // 
            this.ConsoleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConsoleTextBox.Location = new System.Drawing.Point(3, 3);
            this.ConsoleTextBox.Multiline = true;
            this.ConsoleTextBox.Name = "ConsoleTextBox";
            this.ConsoleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ConsoleTextBox.Size = new System.Drawing.Size(730, 95);
            this.ConsoleTextBox.TabIndex = 0;
            // 
            // StatsUpdateTimer
            // 
            this.StatsUpdateTimer.Enabled = true;
            this.StatsUpdateTimer.Interval = 500;
            this.StatsUpdateTimer.Tick += new System.EventHandler(this.StatsUpdateTimer_Tick);
            // 
            // SelectedMarkerPanel
            // 
            this.SelectedMarkerPanel.BackColor = System.Drawing.Color.White;
            this.SelectedMarkerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SelectedMarkerPanel.Controls.Add(this.SelectedMarkerPositionTextBox);
            this.SelectedMarkerPanel.Controls.Add(this.SelectedMarkerNameTextBox);
            this.SelectedMarkerPanel.Location = new System.Drawing.Point(12, 64);
            this.SelectedMarkerPanel.Name = "SelectedMarkerPanel";
            this.SelectedMarkerPanel.Size = new System.Drawing.Size(180, 42);
            this.SelectedMarkerPanel.TabIndex = 5;
            this.SelectedMarkerPanel.Visible = false;
            // 
            // SelectedMarkerPositionTextBox
            // 
            this.SelectedMarkerPositionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectedMarkerPositionTextBox.BackColor = System.Drawing.Color.White;
            this.SelectedMarkerPositionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SelectedMarkerPositionTextBox.Location = new System.Drawing.Point(3, 22);
            this.SelectedMarkerPositionTextBox.Name = "SelectedMarkerPositionTextBox";
            this.SelectedMarkerPositionTextBox.ReadOnly = true;
            this.SelectedMarkerPositionTextBox.Size = new System.Drawing.Size(172, 13);
            this.SelectedMarkerPositionTextBox.TabIndex = 1;
            // 
            // SelectedMarkerNameTextBox
            // 
            this.SelectedMarkerNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectedMarkerNameTextBox.BackColor = System.Drawing.Color.White;
            this.SelectedMarkerNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SelectedMarkerNameTextBox.Location = new System.Drawing.Point(3, 3);
            this.SelectedMarkerNameTextBox.Name = "SelectedMarkerNameTextBox";
            this.SelectedMarkerNameTextBox.ReadOnly = true;
            this.SelectedMarkerNameTextBox.Size = new System.Drawing.Size(172, 13);
            this.SelectedMarkerNameTextBox.TabIndex = 0;
            // 
            // ToolsMenu
            // 
            this.ToolsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolsMenuConfigureGame,
            this.ToolsMenuRPFBrowser,
            this.ToolsMenuRPFExplorer,
            this.ToolsMenuSelectionInfo,
            this.ToolsMenuProjectWindow,
            this.ToolsMenuCutsceneViewer,
            this.ToolsMenuAudioExplorer,
            this.ToolsMenuWorldSearch,
            this.ToolsMenuBinarySearch,
            this.ToolsMenuJenkGen,
            this.ToolsMenuJenkInd,
            this.ToolsMenuExtractScripts,
            this.ToolsMenuExtractTextures,
            this.ToolsMenuExtractRawFiles,
            this.ToolsMenuExtractShaders,
            this.ToolsMenuOptions});
            this.ToolsMenu.Name = "ToolsMenu";
            this.ToolsMenu.Size = new System.Drawing.Size(171, 356);
            // 
            // ToolsMenuConfigureGame
            // 
            this.ToolsMenuConfigureGame.Name = "ToolsMenuConfigureGame";
            this.ToolsMenuConfigureGame.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuConfigureGame.Text = "Configure Game...";
            this.ToolsMenuConfigureGame.Click += new System.EventHandler(this.ToolsMenuConfigureGame_Click);
            // 
            // ToolsMenuRPFBrowser
            // 
            this.ToolsMenuRPFBrowser.Name = "ToolsMenuRPFBrowser";
            this.ToolsMenuRPFBrowser.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuRPFBrowser.Text = "RPF Browser...";
            this.ToolsMenuRPFBrowser.Visible = false;
            this.ToolsMenuRPFBrowser.Click += new System.EventHandler(this.ToolsMenuRPFBrowser_Click);
            // 
            // ToolsMenuRPFExplorer
            // 
            this.ToolsMenuRPFExplorer.Name = "ToolsMenuRPFExplorer";
            this.ToolsMenuRPFExplorer.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuRPFExplorer.Text = "RPF Explorer...";
            this.ToolsMenuRPFExplorer.Click += new System.EventHandler(this.ToolsMenuRPFExplorer_Click);
            // 
            // ToolsMenuSelectionInfo
            // 
            this.ToolsMenuSelectionInfo.Name = "ToolsMenuSelectionInfo";
            this.ToolsMenuSelectionInfo.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuSelectionInfo.Text = "Selection info...";
            this.ToolsMenuSelectionInfo.Click += new System.EventHandler(this.ToolsMenuSelectionInfo_Click);
            // 
            // ToolsMenuProjectWindow
            // 
            this.ToolsMenuProjectWindow.Enabled = false;
            this.ToolsMenuProjectWindow.Name = "ToolsMenuProjectWindow";
            this.ToolsMenuProjectWindow.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuProjectWindow.Text = "Project window...";
            this.ToolsMenuProjectWindow.Click += new System.EventHandler(this.ToolsMenuProjectWindow_Click);
            // 
            // ToolsMenuCutsceneViewer
            // 
            this.ToolsMenuCutsceneViewer.Enabled = false;
            this.ToolsMenuCutsceneViewer.Name = "ToolsMenuCutsceneViewer";
            this.ToolsMenuCutsceneViewer.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuCutsceneViewer.Text = "Cutscene viewer...";
            this.ToolsMenuCutsceneViewer.Click += new System.EventHandler(this.ToolsMenuCutsceneViewer_Click);
            // 
            // ToolsMenuAudioExplorer
            // 
            this.ToolsMenuAudioExplorer.Enabled = false;
            this.ToolsMenuAudioExplorer.Name = "ToolsMenuAudioExplorer";
            this.ToolsMenuAudioExplorer.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuAudioExplorer.Text = "Audio explorer...";
            this.ToolsMenuAudioExplorer.Click += new System.EventHandler(this.ToolsMenuAudioExplorer_Click);
            // 
            // ToolsMenuWorldSearch
            // 
            this.ToolsMenuWorldSearch.Name = "ToolsMenuWorldSearch";
            this.ToolsMenuWorldSearch.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuWorldSearch.Text = "World search...";
            this.ToolsMenuWorldSearch.Click += new System.EventHandler(this.ToolsMenuWorldSearch_Click);
            // 
            // ToolsMenuBinarySearch
            // 
            this.ToolsMenuBinarySearch.Enabled = false;
            this.ToolsMenuBinarySearch.Name = "ToolsMenuBinarySearch";
            this.ToolsMenuBinarySearch.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuBinarySearch.Text = "Binary search...";
            this.ToolsMenuBinarySearch.Click += new System.EventHandler(this.ToolsMenuBinarySearch_Click);
            // 
            // ToolsMenuJenkGen
            // 
            this.ToolsMenuJenkGen.Name = "ToolsMenuJenkGen";
            this.ToolsMenuJenkGen.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuJenkGen.Text = "JenkGen...";
            this.ToolsMenuJenkGen.Click += new System.EventHandler(this.ToolsMenuJenkGen_Click);
            // 
            // ToolsMenuJenkInd
            // 
            this.ToolsMenuJenkInd.Enabled = false;
            this.ToolsMenuJenkInd.Name = "ToolsMenuJenkInd";
            this.ToolsMenuJenkInd.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuJenkInd.Text = "JenkInd...";
            this.ToolsMenuJenkInd.Click += new System.EventHandler(this.ToolsMenuJenkInd_Click);
            // 
            // ToolsMenuExtractScripts
            // 
            this.ToolsMenuExtractScripts.Name = "ToolsMenuExtractScripts";
            this.ToolsMenuExtractScripts.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuExtractScripts.Text = "Extract scripts...";
            this.ToolsMenuExtractScripts.Click += new System.EventHandler(this.ToolsMenuExtractScripts_Click);
            // 
            // ToolsMenuExtractTextures
            // 
            this.ToolsMenuExtractTextures.Name = "ToolsMenuExtractTextures";
            this.ToolsMenuExtractTextures.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuExtractTextures.Text = "Extract textures...";
            this.ToolsMenuExtractTextures.Click += new System.EventHandler(this.ToolsMenuExtractTextures_Click);
            // 
            // ToolsMenuExtractRawFiles
            // 
            this.ToolsMenuExtractRawFiles.Name = "ToolsMenuExtractRawFiles";
            this.ToolsMenuExtractRawFiles.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuExtractRawFiles.Text = "Extract raw files...";
            this.ToolsMenuExtractRawFiles.Click += new System.EventHandler(this.ToolsMenuExtractRawFiles_Click);
            // 
            // ToolsMenuExtractShaders
            // 
            this.ToolsMenuExtractShaders.Name = "ToolsMenuExtractShaders";
            this.ToolsMenuExtractShaders.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuExtractShaders.Text = "Extract shaders...";
            this.ToolsMenuExtractShaders.Click += new System.EventHandler(this.ToolsMenuExtractShaders_Click);
            // 
            // ToolsMenuOptions
            // 
            this.ToolsMenuOptions.Name = "ToolsMenuOptions";
            this.ToolsMenuOptions.Size = new System.Drawing.Size(170, 22);
            this.ToolsMenuOptions.Text = "Options...";
            this.ToolsMenuOptions.Click += new System.EventHandler(this.ToolsMenuOptions_Click);
            // 
            // ToolbarPanel
            // 
            this.ToolbarPanel.BackColor = System.Drawing.SystemColors.Control;
            this.ToolbarPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ToolbarPanel.Controls.Add(this.Toolbar);
            this.ToolbarPanel.Location = new System.Drawing.Point(12, 12);
            this.ToolbarPanel.Name = "ToolbarPanel";
            this.ToolbarPanel.Size = new System.Drawing.Size(557, 26);
            this.ToolbarPanel.TabIndex = 7;
            this.ToolbarPanel.Visible = false;
            // 
            // Toolbar
            // 
            this.Toolbar.Dock = System.Windows.Forms.DockStyle.None;
            this.Toolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.Toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarNewButton,
            this.ToolbarOpenButton,
            this.ToolbarSaveButton,
            this.ToolbarSaveAllButton,
            this.toolStripSeparator5,
            this.ToolbarSelectButton,
            this.toolStripSeparator1,
            this.ToolbarMoveButton,
            this.ToolbarRotateButton,
            this.ToolbarScaleButton,
            this.ToolbarTransformSpaceButton,
            this.ToolbarSnapButton,
            this.toolStripSeparator2,
            this.ToolbarUndoButton,
            this.ToolbarRedoButton,
            this.toolStripSeparator3,
            this.ToolbarInfoWindowButton,
            this.ToolbarProjectWindowButton,
            this.toolStripSeparator4,
            this.ToolbarAddItemButton,
            this.ToolbarDeleteItemButton,
            this.toolStripSeparator6,
            this.ToolbarCopyButton,
            this.ToolbarPasteButton,
            this.toolStripSeparator7,
            this.ToolbarCameraModeButton});
            this.Toolbar.Location = new System.Drawing.Point(1, 0);
            this.Toolbar.Name = "Toolbar";
            this.Toolbar.Size = new System.Drawing.Size(554, 25);
            this.Toolbar.TabIndex = 6;
            this.Toolbar.Text = "toolStrip1";
            // 
            // ToolbarNewButton
            // 
            this.ToolbarNewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarNewButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarNewProjectButton,
            this.ToolbarNewYmapButton,
            this.ToolbarNewYtypButton,
            this.ToolbarNewYbnButton,
            this.ToolbarNewYndButton,
            this.ToolbarNewTrainsButton,
            this.ToolbarNewScenarioButton});
            this.ToolbarNewButton.Enabled = false;
            this.ToolbarNewButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarNewButton.Image")));
            this.ToolbarNewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarNewButton.Name = "ToolbarNewButton";
            this.ToolbarNewButton.Size = new System.Drawing.Size(32, 22);
            this.ToolbarNewButton.Text = "New...";
            this.ToolbarNewButton.ToolTipText = "New... (Ctrl+N)";
            this.ToolbarNewButton.ButtonClick += new System.EventHandler(this.ToolbarNewButton_ButtonClick);
            // 
            // ToolbarNewProjectButton
            // 
            this.ToolbarNewProjectButton.Name = "ToolbarNewProjectButton";
            this.ToolbarNewProjectButton.Size = new System.Drawing.Size(167, 22);
            this.ToolbarNewProjectButton.Text = "New Project";
            this.ToolbarNewProjectButton.Click += new System.EventHandler(this.ToolbarNewProjectButton_Click);
            // 
            // ToolbarNewYmapButton
            // 
            this.ToolbarNewYmapButton.Name = "ToolbarNewYmapButton";
            this.ToolbarNewYmapButton.Size = new System.Drawing.Size(167, 22);
            this.ToolbarNewYmapButton.Text = "New Ymap File";
            this.ToolbarNewYmapButton.Click += new System.EventHandler(this.ToolbarNewYmapButton_Click);
            // 
            // ToolbarNewYtypButton
            // 
            this.ToolbarNewYtypButton.Name = "ToolbarNewYtypButton";
            this.ToolbarNewYtypButton.Size = new System.Drawing.Size(167, 22);
            this.ToolbarNewYtypButton.Text = "New Ytyp File";
            this.ToolbarNewYtypButton.Click += new System.EventHandler(this.ToolbarNewYtypButton_Click);
            // 
            // ToolbarNewYbnButton
            // 
            this.ToolbarNewYbnButton.Name = "ToolbarNewYbnButton";
            this.ToolbarNewYbnButton.Size = new System.Drawing.Size(167, 22);
            this.ToolbarNewYbnButton.Text = "New Ybn File";
            this.ToolbarNewYbnButton.Click += new System.EventHandler(this.ToolbarNewYbnButton_Click);
            // 
            // ToolbarNewYndButton
            // 
            this.ToolbarNewYndButton.Name = "ToolbarNewYndButton";
            this.ToolbarNewYndButton.Size = new System.Drawing.Size(167, 22);
            this.ToolbarNewYndButton.Text = "New Ynd File";
            this.ToolbarNewYndButton.Click += new System.EventHandler(this.ToolbarNewYndButton_Click);
            // 
            // ToolbarNewTrainsButton
            // 
            this.ToolbarNewTrainsButton.Name = "ToolbarNewTrainsButton";
            this.ToolbarNewTrainsButton.Size = new System.Drawing.Size(167, 22);
            this.ToolbarNewTrainsButton.Text = "New Trains File";
            this.ToolbarNewTrainsButton.Click += new System.EventHandler(this.ToolbarNewTrainsButton_Click);
            // 
            // ToolbarNewScenarioButton
            // 
            this.ToolbarNewScenarioButton.Name = "ToolbarNewScenarioButton";
            this.ToolbarNewScenarioButton.Size = new System.Drawing.Size(167, 22);
            this.ToolbarNewScenarioButton.Text = "New Scenario File";
            this.ToolbarNewScenarioButton.Click += new System.EventHandler(this.ToolbarNewScenarioButton_Click);
            // 
            // ToolbarOpenButton
            // 
            this.ToolbarOpenButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarOpenButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarOpenProjectButton,
            this.ToolbarOpenFilesButton,
            this.ToolbarOpenFolderButton});
            this.ToolbarOpenButton.Enabled = false;
            this.ToolbarOpenButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarOpenButton.Image")));
            this.ToolbarOpenButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarOpenButton.Name = "ToolbarOpenButton";
            this.ToolbarOpenButton.Size = new System.Drawing.Size(32, 22);
            this.ToolbarOpenButton.Text = "Open...";
            this.ToolbarOpenButton.ToolTipText = "Open... (Ctrl+O)";
            this.ToolbarOpenButton.ButtonClick += new System.EventHandler(this.ToolbarOpenButton_ButtonClick);
            // 
            // ToolbarOpenProjectButton
            // 
            this.ToolbarOpenProjectButton.Name = "ToolbarOpenProjectButton";
            this.ToolbarOpenProjectButton.Size = new System.Drawing.Size(152, 22);
            this.ToolbarOpenProjectButton.Text = "Open Project...";
            this.ToolbarOpenProjectButton.Click += new System.EventHandler(this.ToolbarOpenProjectButton_Click);
            // 
            // ToolbarOpenFilesButton
            // 
            this.ToolbarOpenFilesButton.Name = "ToolbarOpenFilesButton";
            this.ToolbarOpenFilesButton.Size = new System.Drawing.Size(152, 22);
            this.ToolbarOpenFilesButton.Text = "Open Files...";
            this.ToolbarOpenFilesButton.Click += new System.EventHandler(this.ToolbarOpenFilesButton_Click);
            // 
            // ToolbarOpenFolderButton
            // 
            this.ToolbarOpenFolderButton.Name = "ToolbarOpenFolderButton";
            this.ToolbarOpenFolderButton.Size = new System.Drawing.Size(152, 22);
            this.ToolbarOpenFolderButton.Text = "Open Folder...";
            this.ToolbarOpenFolderButton.Click += new System.EventHandler(this.ToolbarOpenFolderButton_Click);
            // 
            // ToolbarSaveButton
            // 
            this.ToolbarSaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarSaveButton.Enabled = false;
            this.ToolbarSaveButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarSaveButton.Image")));
            this.ToolbarSaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarSaveButton.Name = "ToolbarSaveButton";
            this.ToolbarSaveButton.Size = new System.Drawing.Size(23, 22);
            this.ToolbarSaveButton.Text = "Save";
            this.ToolbarSaveButton.ToolTipText = "Save (Ctrl+S)";
            this.ToolbarSaveButton.Click += new System.EventHandler(this.ToolbarSaveButton_Click);
            // 
            // ToolbarSaveAllButton
            // 
            this.ToolbarSaveAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarSaveAllButton.Enabled = false;
            this.ToolbarSaveAllButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarSaveAllButton.Image")));
            this.ToolbarSaveAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarSaveAllButton.Name = "ToolbarSaveAllButton";
            this.ToolbarSaveAllButton.Size = new System.Drawing.Size(23, 22);
            this.ToolbarSaveAllButton.Text = "Save All";
            this.ToolbarSaveAllButton.ToolTipText = "Save All (Ctrl+Shift+S)";
            this.ToolbarSaveAllButton.Click += new System.EventHandler(this.ToolbarSaveAllButton_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarSelectButton
            // 
            this.ToolbarSelectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarSelectButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarSelectEntityButton,
            this.ToolbarSelectEntityExtensionButton,
            this.ToolbarSelectArchetypeExtensionButton,
            this.ToolbarSelectTimeCycleModifierButton,
            this.ToolbarSelectCarGeneratorButton,
            this.ToolbarSelectGrassButton,
            this.ToolbarSelectWaterQuadButton,
            this.ToolbarSelectCalmingQuadButton,
            this.ToolbarSelectWaveQuadButton,
            this.ToolbarSelectCollisionButton,
            this.ToolbarSelectNavMeshButton,
            this.ToolbarSelectPathButton,
            this.ToolbarSelectTrainTrackButton,
            this.ToolbarSelectLodLightsButton,
            this.ToolbarSelectMloInstanceButton,
            this.ToolbarSelectScenarioButton,
            this.ToolbarSelectAudioButton,
            this.ToolbarSelectOcclusionButton});
            this.ToolbarSelectButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarSelectButton.Image")));
            this.ToolbarSelectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarSelectButton.Name = "ToolbarSelectButton";
            this.ToolbarSelectButton.Size = new System.Drawing.Size(32, 22);
            this.ToolbarSelectButton.Text = "Select objects / Exit edit mode";
            this.ToolbarSelectButton.ToolTipText = "Select objects / Exit edit mode (C, Q)";
            this.ToolbarSelectButton.ButtonClick += new System.EventHandler(this.ToolbarSelectButton_ButtonClick);
            // 
            // ToolbarSelectEntityButton
            // 
            this.ToolbarSelectEntityButton.Checked = true;
            this.ToolbarSelectEntityButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToolbarSelectEntityButton.Name = "ToolbarSelectEntityButton";
            this.ToolbarSelectEntityButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectEntityButton.Text = "Entity";
            this.ToolbarSelectEntityButton.Click += new System.EventHandler(this.ToolbarSelectEntityButton_Click);
            // 
            // ToolbarSelectEntityExtensionButton
            // 
            this.ToolbarSelectEntityExtensionButton.Name = "ToolbarSelectEntityExtensionButton";
            this.ToolbarSelectEntityExtensionButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectEntityExtensionButton.Text = "Entity Extension";
            this.ToolbarSelectEntityExtensionButton.Click += new System.EventHandler(this.ToolbarSelectEntityExtensionButton_Click);
            // 
            // ToolbarSelectArchetypeExtensionButton
            // 
            this.ToolbarSelectArchetypeExtensionButton.Name = "ToolbarSelectArchetypeExtensionButton";
            this.ToolbarSelectArchetypeExtensionButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectArchetypeExtensionButton.Text = "Archetype Extension";
            this.ToolbarSelectArchetypeExtensionButton.Click += new System.EventHandler(this.ToolbarSelectArchetypeExtensionButton_Click);
            // 
            // ToolbarSelectTimeCycleModifierButton
            // 
            this.ToolbarSelectTimeCycleModifierButton.Name = "ToolbarSelectTimeCycleModifierButton";
            this.ToolbarSelectTimeCycleModifierButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectTimeCycleModifierButton.Text = "Time Cycle Modifier";
            this.ToolbarSelectTimeCycleModifierButton.Click += new System.EventHandler(this.ToolbarSelectTimeCycleModifierButton_Click);
            // 
            // ToolbarSelectCarGeneratorButton
            // 
            this.ToolbarSelectCarGeneratorButton.Name = "ToolbarSelectCarGeneratorButton";
            this.ToolbarSelectCarGeneratorButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectCarGeneratorButton.Text = "Car Generator";
            this.ToolbarSelectCarGeneratorButton.Click += new System.EventHandler(this.ToolbarSelectCarGeneratorButton_Click);
            // 
            // ToolbarSelectGrassButton
            // 
            this.ToolbarSelectGrassButton.Name = "ToolbarSelectGrassButton";
            this.ToolbarSelectGrassButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectGrassButton.Text = "Grass";
            this.ToolbarSelectGrassButton.Click += new System.EventHandler(this.ToolbarSelectGrassButton_Click);
            // 
            // ToolbarSelectWaterQuadButton
            // 
            this.ToolbarSelectWaterQuadButton.Name = "ToolbarSelectWaterQuadButton";
            this.ToolbarSelectWaterQuadButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectWaterQuadButton.Text = "Water Quad";
            this.ToolbarSelectWaterQuadButton.Click += new System.EventHandler(this.ToolbarSelectWaterQuadButton_Click);
            // 
            // ToolbarSelectCalmingQuadButton
            // 
            this.ToolbarSelectCalmingQuadButton.Name = "ToolbarSelectCalmingQuadButton";
            this.ToolbarSelectCalmingQuadButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectCalmingQuadButton.Text = "Water Calming Quad";
            this.ToolbarSelectCalmingQuadButton.Click += new System.EventHandler(this.ToolbarSelectCalmingQuadButton_Click);
            // 
            // ToolbarSelectWaveQuadButton
            // 
            this.ToolbarSelectWaveQuadButton.Name = "ToolbarSelectWaveQuadButton";
            this.ToolbarSelectWaveQuadButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectWaveQuadButton.Text = "Water Wave Quad";
            this.ToolbarSelectWaveQuadButton.Click += new System.EventHandler(this.ToolbarSelectWaveQuadButton_Click);
            // 
            // ToolbarSelectCollisionButton
            // 
            this.ToolbarSelectCollisionButton.Name = "ToolbarSelectCollisionButton";
            this.ToolbarSelectCollisionButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectCollisionButton.Text = "Collision";
            this.ToolbarSelectCollisionButton.Click += new System.EventHandler(this.ToolbarSelectCollisionButton_Click);
            // 
            // ToolbarSelectNavMeshButton
            // 
            this.ToolbarSelectNavMeshButton.Name = "ToolbarSelectNavMeshButton";
            this.ToolbarSelectNavMeshButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectNavMeshButton.Text = "Nav Mesh";
            this.ToolbarSelectNavMeshButton.Click += new System.EventHandler(this.ToolbarSelectNavMeshButton_Click);
            // 
            // ToolbarSelectPathButton
            // 
            this.ToolbarSelectPathButton.Name = "ToolbarSelectPathButton";
            this.ToolbarSelectPathButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectPathButton.Text = "Traffic Path";
            this.ToolbarSelectPathButton.Click += new System.EventHandler(this.ToolbarSelectPathButton_Click);
            // 
            // ToolbarSelectTrainTrackButton
            // 
            this.ToolbarSelectTrainTrackButton.Name = "ToolbarSelectTrainTrackButton";
            this.ToolbarSelectTrainTrackButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectTrainTrackButton.Text = "Train Track";
            this.ToolbarSelectTrainTrackButton.Click += new System.EventHandler(this.ToolbarSelectTrainTrackButton_Click);
            // 
            // ToolbarSelectLodLightsButton
            // 
            this.ToolbarSelectLodLightsButton.Name = "ToolbarSelectLodLightsButton";
            this.ToolbarSelectLodLightsButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectLodLightsButton.Text = "Lod Lights";
            this.ToolbarSelectLodLightsButton.Click += new System.EventHandler(this.ToolbarSelectLodLightsButton_Click);
            // 
            // ToolbarSelectMloInstanceButton
            // 
            this.ToolbarSelectMloInstanceButton.Name = "ToolbarSelectMloInstanceButton";
            this.ToolbarSelectMloInstanceButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectMloInstanceButton.Text = "Interior Instance";
            this.ToolbarSelectMloInstanceButton.Click += new System.EventHandler(this.ToolbarSelectMloInstanceButton_Click);
            // 
            // ToolbarSelectScenarioButton
            // 
            this.ToolbarSelectScenarioButton.Name = "ToolbarSelectScenarioButton";
            this.ToolbarSelectScenarioButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectScenarioButton.Text = "Scenario";
            this.ToolbarSelectScenarioButton.Click += new System.EventHandler(this.ToolbarSelectScenarioButton_Click);
            // 
            // ToolbarSelectAudioButton
            // 
            this.ToolbarSelectAudioButton.Name = "ToolbarSelectAudioButton";
            this.ToolbarSelectAudioButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectAudioButton.Text = "Audio";
            this.ToolbarSelectAudioButton.Click += new System.EventHandler(this.ToolbarSelectAudioButton_Click);
            // 
            // ToolbarSelectOcclusionButton
            // 
            this.ToolbarSelectOcclusionButton.Name = "ToolbarSelectOcclusionButton";
            this.ToolbarSelectOcclusionButton.Size = new System.Drawing.Size(185, 22);
            this.ToolbarSelectOcclusionButton.Text = "Occlusion";
            this.ToolbarSelectOcclusionButton.Click += new System.EventHandler(this.ToolbarSelectOcclusionButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarMoveButton
            // 
            this.ToolbarMoveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarMoveButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarMoveButton.Image")));
            this.ToolbarMoveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarMoveButton.Name = "ToolbarMoveButton";
            this.ToolbarMoveButton.Size = new System.Drawing.Size(23, 22);
            this.ToolbarMoveButton.Text = "Move";
            this.ToolbarMoveButton.ToolTipText = "Move (W)";
            this.ToolbarMoveButton.Click += new System.EventHandler(this.ToolbarMoveButton_Click);
            // 
            // ToolbarRotateButton
            // 
            this.ToolbarRotateButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarRotateButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarRotateButton.Image")));
            this.ToolbarRotateButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarRotateButton.Name = "ToolbarRotateButton";
            this.ToolbarRotateButton.Size = new System.Drawing.Size(23, 22);
            this.ToolbarRotateButton.Text = "Rotate";
            this.ToolbarRotateButton.ToolTipText = "Rotate (E)";
            this.ToolbarRotateButton.Click += new System.EventHandler(this.ToolbarRotateButton_Click);
            // 
            // ToolbarScaleButton
            // 
            this.ToolbarScaleButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarScaleButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarScaleButton.Image")));
            this.ToolbarScaleButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarScaleButton.Name = "ToolbarScaleButton";
            this.ToolbarScaleButton.Size = new System.Drawing.Size(23, 22);
            this.ToolbarScaleButton.Text = "Scale";
            this.ToolbarScaleButton.ToolTipText = "Scale (R)";
            this.ToolbarScaleButton.Click += new System.EventHandler(this.ToolbarScaleButton_Click);
            // 
            // ToolbarTransformSpaceButton
            // 
            this.ToolbarTransformSpaceButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarTransformSpaceButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarObjectSpaceButton,
            this.ToolbarWorldSpaceButton});
            this.ToolbarTransformSpaceButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarTransformSpaceButton.Image")));
            this.ToolbarTransformSpaceButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarTransformSpaceButton.Name = "ToolbarTransformSpaceButton";
            this.ToolbarTransformSpaceButton.Size = new System.Drawing.Size(32, 22);
            this.ToolbarTransformSpaceButton.Text = "Toggle transform space";
            this.ToolbarTransformSpaceButton.ButtonClick += new System.EventHandler(this.ToolbarTransformSpaceButton_ButtonClick);
            // 
            // ToolbarObjectSpaceButton
            // 
            this.ToolbarObjectSpaceButton.Checked = true;
            this.ToolbarObjectSpaceButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToolbarObjectSpaceButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarObjectSpaceButton.Image")));
            this.ToolbarObjectSpaceButton.Name = "ToolbarObjectSpaceButton";
            this.ToolbarObjectSpaceButton.Size = new System.Drawing.Size(142, 22);
            this.ToolbarObjectSpaceButton.Text = "Object space";
            this.ToolbarObjectSpaceButton.Click += new System.EventHandler(this.ToolbarObjectSpaceButton_Click);
            // 
            // ToolbarWorldSpaceButton
            // 
            this.ToolbarWorldSpaceButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarWorldSpaceButton.Image")));
            this.ToolbarWorldSpaceButton.Name = "ToolbarWorldSpaceButton";
            this.ToolbarWorldSpaceButton.Size = new System.Drawing.Size(142, 22);
            this.ToolbarWorldSpaceButton.Text = "World space";
            this.ToolbarWorldSpaceButton.Click += new System.EventHandler(this.ToolbarWorldSpaceButton_Click);
            // 
            // ToolbarSnapButton
            // 
            this.ToolbarSnapButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarSnapButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarSnapToGroundButton,
            this.ToolbarSnapToGridButton,
            this.ToolbarSnapToGroundGridButton,
            this.ToolbarSnapGridSizeButton,
            this.ToolbarRotationSnappingButton});
            this.ToolbarSnapButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarSnapButton.Image")));
            this.ToolbarSnapButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarSnapButton.Name = "ToolbarSnapButton";
            this.ToolbarSnapButton.Size = new System.Drawing.Size(32, 22);
            this.ToolbarSnapButton.Text = "Snap to Ground";
            this.ToolbarSnapButton.ToolTipText = "Snap to Ground";
            this.ToolbarSnapButton.ButtonClick += new System.EventHandler(this.ToolbarSnapButton_ButtonClick);
            // 
            // ToolbarSnapToGroundButton
            // 
            this.ToolbarSnapToGroundButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarSnapToGroundButton.Image")));
            this.ToolbarSnapToGroundButton.Name = "ToolbarSnapToGroundButton";
            this.ToolbarSnapToGroundButton.Size = new System.Drawing.Size(205, 22);
            this.ToolbarSnapToGroundButton.Text = "Snap to Ground";
            this.ToolbarSnapToGroundButton.Click += new System.EventHandler(this.ToolbarSnapToGroundButton_Click);
            // 
            // ToolbarSnapToGridButton
            // 
            this.ToolbarSnapToGridButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarSnapToGridButton.Image")));
            this.ToolbarSnapToGridButton.Name = "ToolbarSnapToGridButton";
            this.ToolbarSnapToGridButton.Size = new System.Drawing.Size(205, 22);
            this.ToolbarSnapToGridButton.Text = "Snap to Grid";
            this.ToolbarSnapToGridButton.Click += new System.EventHandler(this.ToolbarSnapToGridButton_Click);
            // 
            // ToolbarSnapToGroundGridButton
            // 
            this.ToolbarSnapToGroundGridButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarSnapToGroundGridButton.Image")));
            this.ToolbarSnapToGroundGridButton.Name = "ToolbarSnapToGroundGridButton";
            this.ToolbarSnapToGroundGridButton.Size = new System.Drawing.Size(205, 22);
            this.ToolbarSnapToGroundGridButton.Text = "Snap to Grid and Ground";
            this.ToolbarSnapToGroundGridButton.Click += new System.EventHandler(this.ToolbarSnapToGroundGridButton_Click);
            // 
            // ToolbarSnapGridSizeButton
            // 
            this.ToolbarSnapGridSizeButton.Name = "ToolbarSnapGridSizeButton";
            this.ToolbarSnapGridSizeButton.Size = new System.Drawing.Size(205, 22);
            this.ToolbarSnapGridSizeButton.Text = "Grid Size...";
            this.ToolbarSnapGridSizeButton.Click += new System.EventHandler(this.ToolbarSnapGridSizeButton_Click);
            // 
            // ToolbarRotationSnappingButton
            // 
            this.ToolbarRotationSnappingButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarRotationSnappingOffButton,
            this.ToolbarRotationSnapping1Button,
            this.ToolbarRotationSnapping2Button,
            this.ToolbarRotationSnapping5Button,
            this.ToolbarRotationSnapping10Button,
            this.ToolbarRotationSnapping45Button,
            this.ToolbarRotationSnapping90Button,
            this.ToolbarRotationSnappingCustomButton});
            this.ToolbarRotationSnappingButton.Name = "ToolbarRotationSnappingButton";
            this.ToolbarRotationSnappingButton.Size = new System.Drawing.Size(205, 22);
            this.ToolbarRotationSnappingButton.Text = "Rotation Snapping";
            // 
            // ToolbarRotationSnappingOffButton
            // 
            this.ToolbarRotationSnappingOffButton.Name = "ToolbarRotationSnappingOffButton";
            this.ToolbarRotationSnappingOffButton.Size = new System.Drawing.Size(131, 22);
            this.ToolbarRotationSnappingOffButton.Text = "Off";
            this.ToolbarRotationSnappingOffButton.Click += new System.EventHandler(this.ToolbarRotationSnappingOffButton_Click);
            // 
            // ToolbarRotationSnapping1Button
            // 
            this.ToolbarRotationSnapping1Button.Name = "ToolbarRotationSnapping1Button";
            this.ToolbarRotationSnapping1Button.Size = new System.Drawing.Size(131, 22);
            this.ToolbarRotationSnapping1Button.Text = "1 Degree";
            this.ToolbarRotationSnapping1Button.Click += new System.EventHandler(this.ToolbarRotationSnapping1Button_Click);
            // 
            // ToolbarRotationSnapping2Button
            // 
            this.ToolbarRotationSnapping2Button.Name = "ToolbarRotationSnapping2Button";
            this.ToolbarRotationSnapping2Button.Size = new System.Drawing.Size(131, 22);
            this.ToolbarRotationSnapping2Button.Text = "2 Degrees";
            this.ToolbarRotationSnapping2Button.Click += new System.EventHandler(this.ToolbarRotationSnapping2Button_Click);
            // 
            // ToolbarRotationSnapping5Button
            // 
            this.ToolbarRotationSnapping5Button.Checked = true;
            this.ToolbarRotationSnapping5Button.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToolbarRotationSnapping5Button.Name = "ToolbarRotationSnapping5Button";
            this.ToolbarRotationSnapping5Button.Size = new System.Drawing.Size(131, 22);
            this.ToolbarRotationSnapping5Button.Text = "5 Degrees";
            this.ToolbarRotationSnapping5Button.Click += new System.EventHandler(this.ToolbarRotationSnapping5Button_Click);
            // 
            // ToolbarRotationSnapping10Button
            // 
            this.ToolbarRotationSnapping10Button.Name = "ToolbarRotationSnapping10Button";
            this.ToolbarRotationSnapping10Button.Size = new System.Drawing.Size(131, 22);
            this.ToolbarRotationSnapping10Button.Text = "10 Degrees";
            this.ToolbarRotationSnapping10Button.Click += new System.EventHandler(this.ToolbarRotationSnapping10Button_Click);
            // 
            // ToolbarRotationSnapping45Button
            // 
            this.ToolbarRotationSnapping45Button.Name = "ToolbarRotationSnapping45Button";
            this.ToolbarRotationSnapping45Button.Size = new System.Drawing.Size(131, 22);
            this.ToolbarRotationSnapping45Button.Text = "45 Degrees";
            this.ToolbarRotationSnapping45Button.Click += new System.EventHandler(this.ToolbarRotationSnapping45Button_Click);
            // 
            // ToolbarRotationSnapping90Button
            // 
            this.ToolbarRotationSnapping90Button.Name = "ToolbarRotationSnapping90Button";
            this.ToolbarRotationSnapping90Button.Size = new System.Drawing.Size(131, 22);
            this.ToolbarRotationSnapping90Button.Text = "90 Degrees";
            this.ToolbarRotationSnapping90Button.Click += new System.EventHandler(this.ToolbarRotationSnapping90Button_Click);
            // 
            // ToolbarRotationSnappingCustomButton
            // 
            this.ToolbarRotationSnappingCustomButton.Name = "ToolbarRotationSnappingCustomButton";
            this.ToolbarRotationSnappingCustomButton.Size = new System.Drawing.Size(131, 22);
            this.ToolbarRotationSnappingCustomButton.Text = "Custom...";
            this.ToolbarRotationSnappingCustomButton.Click += new System.EventHandler(this.ToolbarRotationSnappingCustomButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarUndoButton
            // 
            this.ToolbarUndoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarUndoButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarUndoListButton});
            this.ToolbarUndoButton.Enabled = false;
            this.ToolbarUndoButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarUndoButton.Image")));
            this.ToolbarUndoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarUndoButton.Name = "ToolbarUndoButton";
            this.ToolbarUndoButton.Size = new System.Drawing.Size(32, 22);
            this.ToolbarUndoButton.Text = "Undo";
            this.ToolbarUndoButton.ButtonClick += new System.EventHandler(this.ToolbarUndoButton_ButtonClick);
            // 
            // ToolbarUndoListButton
            // 
            this.ToolbarUndoListButton.Name = "ToolbarUndoListButton";
            this.ToolbarUndoListButton.Size = new System.Drawing.Size(121, 22);
            this.ToolbarUndoListButton.Text = "Undo list";
            this.ToolbarUndoListButton.Click += new System.EventHandler(this.ToolbarUndoListButton_Click);
            // 
            // ToolbarRedoButton
            // 
            this.ToolbarRedoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarRedoButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarRedoListButton});
            this.ToolbarRedoButton.Enabled = false;
            this.ToolbarRedoButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarRedoButton.Image")));
            this.ToolbarRedoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarRedoButton.Name = "ToolbarRedoButton";
            this.ToolbarRedoButton.Size = new System.Drawing.Size(32, 22);
            this.ToolbarRedoButton.Text = "Redo";
            this.ToolbarRedoButton.ButtonClick += new System.EventHandler(this.ToolbarRedoButton_ButtonClick);
            // 
            // ToolbarRedoListButton
            // 
            this.ToolbarRedoListButton.Name = "ToolbarRedoListButton";
            this.ToolbarRedoListButton.Size = new System.Drawing.Size(119, 22);
            this.ToolbarRedoListButton.Text = "Redo list";
            this.ToolbarRedoListButton.Click += new System.EventHandler(this.ToolbarRedoListButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarInfoWindowButton
            // 
            this.ToolbarInfoWindowButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarInfoWindowButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarInfoWindowButton.Image")));
            this.ToolbarInfoWindowButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarInfoWindowButton.Name = "ToolbarInfoWindowButton";
            this.ToolbarInfoWindowButton.Size = new System.Drawing.Size(23, 22);
            this.ToolbarInfoWindowButton.Text = "Selection info window";
            this.ToolbarInfoWindowButton.Click += new System.EventHandler(this.ToolbarInfoWindowButton_Click);
            // 
            // ToolbarProjectWindowButton
            // 
            this.ToolbarProjectWindowButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarProjectWindowButton.Enabled = false;
            this.ToolbarProjectWindowButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarProjectWindowButton.Image")));
            this.ToolbarProjectWindowButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarProjectWindowButton.Name = "ToolbarProjectWindowButton";
            this.ToolbarProjectWindowButton.Size = new System.Drawing.Size(23, 22);
            this.ToolbarProjectWindowButton.Text = "Project window";
            this.ToolbarProjectWindowButton.Click += new System.EventHandler(this.ToolbarProjectWindowButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarAddItemButton
            // 
            this.ToolbarAddItemButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarAddItemButton.Enabled = false;
            this.ToolbarAddItemButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarAddItemButton.Image")));
            this.ToolbarAddItemButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarAddItemButton.Name = "ToolbarAddItemButton";
            this.ToolbarAddItemButton.Size = new System.Drawing.Size(23, 22);
            this.ToolbarAddItemButton.Text = "Add entity";
            this.ToolbarAddItemButton.Click += new System.EventHandler(this.ToolbarAddItemButton_Click);
            // 
            // ToolbarDeleteItemButton
            // 
            this.ToolbarDeleteItemButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarDeleteItemButton.Enabled = false;
            this.ToolbarDeleteItemButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarDeleteItemButton.Image")));
            this.ToolbarDeleteItemButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarDeleteItemButton.Name = "ToolbarDeleteItemButton";
            this.ToolbarDeleteItemButton.Size = new System.Drawing.Size(23, 22);
            this.ToolbarDeleteItemButton.Text = "Delete entity";
            this.ToolbarDeleteItemButton.Click += new System.EventHandler(this.ToolbarDeleteItemButton_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarCopyButton
            // 
            this.ToolbarCopyButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarCopyButton.Enabled = false;
            this.ToolbarCopyButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarCopyButton.Image")));
            this.ToolbarCopyButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarCopyButton.Name = "ToolbarCopyButton";
            this.ToolbarCopyButton.Size = new System.Drawing.Size(23, 22);
            this.ToolbarCopyButton.Text = "Copy";
            this.ToolbarCopyButton.ToolTipText = "Copy (Ctrl+C)";
            this.ToolbarCopyButton.Click += new System.EventHandler(this.ToolbarCopyButton_Click);
            // 
            // ToolbarPasteButton
            // 
            this.ToolbarPasteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarPasteButton.Enabled = false;
            this.ToolbarPasteButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarPasteButton.Image")));
            this.ToolbarPasteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarPasteButton.Name = "ToolbarPasteButton";
            this.ToolbarPasteButton.Size = new System.Drawing.Size(23, 22);
            this.ToolbarPasteButton.Text = "Paste";
            this.ToolbarPasteButton.ToolTipText = "Paste (Ctrl+V)";
            this.ToolbarPasteButton.Click += new System.EventHandler(this.ToolbarPasteButton_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarCameraModeButton
            // 
            this.ToolbarCameraModeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarCameraModeButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarCameraPerspectiveButton,
            this.ToolbarCameraMapViewButton,
            this.ToolbarCameraOrthographicButton});
            this.ToolbarCameraModeButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarCameraModeButton.Image")));
            this.ToolbarCameraModeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarCameraModeButton.Name = "ToolbarCameraModeButton";
            this.ToolbarCameraModeButton.Size = new System.Drawing.Size(32, 22);
            this.ToolbarCameraModeButton.Text = "Camera Mode";
            this.ToolbarCameraModeButton.ButtonClick += new System.EventHandler(this.ToolbarCameraModeButton_ButtonClick);
            // 
            // ToolbarCameraPerspectiveButton
            // 
            this.ToolbarCameraPerspectiveButton.Checked = true;
            this.ToolbarCameraPerspectiveButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToolbarCameraPerspectiveButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarCameraPerspectiveButton.Image")));
            this.ToolbarCameraPerspectiveButton.Name = "ToolbarCameraPerspectiveButton";
            this.ToolbarCameraPerspectiveButton.Size = new System.Drawing.Size(145, 22);
            this.ToolbarCameraPerspectiveButton.Text = "Perspective";
            this.ToolbarCameraPerspectiveButton.Click += new System.EventHandler(this.ToolbarCameraPerspectiveButton_Click);
            // 
            // ToolbarCameraMapViewButton
            // 
            this.ToolbarCameraMapViewButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarCameraMapViewButton.Image")));
            this.ToolbarCameraMapViewButton.Name = "ToolbarCameraMapViewButton";
            this.ToolbarCameraMapViewButton.Size = new System.Drawing.Size(145, 22);
            this.ToolbarCameraMapViewButton.Text = "Map View";
            this.ToolbarCameraMapViewButton.Click += new System.EventHandler(this.ToolbarCameraMapViewButton_Click);
            // 
            // ToolbarCameraOrthographicButton
            // 
            this.ToolbarCameraOrthographicButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarCameraOrthographicButton.Image")));
            this.ToolbarCameraOrthographicButton.Name = "ToolbarCameraOrthographicButton";
            this.ToolbarCameraOrthographicButton.Size = new System.Drawing.Size(145, 22);
            this.ToolbarCameraOrthographicButton.Text = "Orthographic";
            this.ToolbarCameraOrthographicButton.Click += new System.EventHandler(this.ToolbarCameraOrthographicButton_Click);
            // 
            // SubtitleLabel
            // 
            this.SubtitleLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.SubtitleLabel.AutoSize = true;
            this.SubtitleLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.SubtitleLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SubtitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleLabel.Location = new System.Drawing.Point(455, 555);
            this.SubtitleLabel.Name = "SubtitleLabel";
            this.SubtitleLabel.Size = new System.Drawing.Size(83, 18);
            this.SubtitleLabel.TabIndex = 8;
            this.SubtitleLabel.Text = "Test Subtitle";
            this.SubtitleLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.SubtitleLabel.Visible = false;
            this.SubtitleLabel.SizeChanged += new System.EventHandler(this.SubtitleLabel_SizeChanged);
            // 
            // SubtitleTimer
            // 
            this.SubtitleTimer.Tick += new System.EventHandler(this.SubtitleTimer_Tick);
            // 
            // WorldForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MidnightBlue;
            this.ClientSize = new System.Drawing.Size(984, 711);
            this.Controls.Add(this.ToolbarPanel);
            this.Controls.Add(this.SelectedMarkerPanel);
            this.Controls.Add(this.ConsolePanel);
            this.Controls.Add(this.ToolsPanel);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.ToolsPanelShowButton);
            this.Controls.Add(this.SubtitleLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "WorldForm";
            this.Text = "CodeWalker";
            this.Deactivate += new System.EventHandler(this.WorldForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WorldForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WorldForm_FormClosed);
            this.Load += new System.EventHandler(this.WorldForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WorldForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.WorldForm_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WorldForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WorldForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WorldForm_MouseUp);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ToolsPanel.ResumeLayout(false);
            this.ToolsTabControl.ResumeLayout(false);
            this.ViewTabPage.ResumeLayout(false);
            this.ViewTabPage.PerformLayout();
            this.ViewTabControl.ResumeLayout(false);
            this.ViewWorldTabPage.ResumeLayout(false);
            this.ViewWorldTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WorldDetailDistTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WorldLodDistTrackBar)).EndInit();
            this.ViewYmapsTabPage.ResumeLayout(false);
            this.ViewYmapsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DetailTrackBar)).EndInit();
            this.ViewModelTabPage.ResumeLayout(false);
            this.ViewModelTabPage.PerformLayout();
            this.MarkersTabPage.ResumeLayout(false);
            this.MarkersTabPage.PerformLayout();
            this.SelectionTabPage.ResumeLayout(false);
            this.SelectionTabPage.PerformLayout();
            this.SelectionTabControl.ResumeLayout(false);
            this.SelectionEntityTabPage.ResumeLayout(false);
            this.SelectionArchetypeTabPage.ResumeLayout(false);
            this.SelectionDrawableTabPage.ResumeLayout(false);
            this.tabControl3.ResumeLayout(false);
            this.tabPage11.ResumeLayout(false);
            this.tabPage12.ResumeLayout(false);
            this.tabPage13.ResumeLayout(false);
            this.SelectionExtensionTabPage.ResumeLayout(false);
            this.OptionsTabPage.ResumeLayout(false);
            this.OptionsTabControl.ResumeLayout(false);
            this.OptionsGeneralTabPage.ResumeLayout(false);
            this.OptionsGeneralTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MapViewDetailTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CollisionMeshRangeTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FieldOfViewTrackBar)).EndInit();
            this.OptionsRenderTabPage.ResumeLayout(false);
            this.OptionsRenderTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FarClipUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NearClipUpDown)).EndInit();
            this.OptionsHelpersTabPage.ResumeLayout(false);
            this.OptionsHelpersTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SnapAngleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SnapGridSizeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BoundsRangeTrackBar)).EndInit();
            this.OptionsLightingTabPage.ResumeLayout(false);
            this.OptionsLightingTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CloudParamTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TimeSpeedTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TimeOfDayTrackBar)).EndInit();
            this.ConsolePanel.ResumeLayout(false);
            this.ConsolePanel.PerformLayout();
            this.SelectedMarkerPanel.ResumeLayout(false);
            this.SelectedMarkerPanel.PerformLayout();
            this.ToolsMenu.ResumeLayout(false);
            this.ToolbarPanel.ResumeLayout(false);
            this.ToolbarPanel.PerformLayout();
            this.Toolbar.ResumeLayout(false);
            this.Toolbar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.ComboBox ModelComboBox;
        private System.Windows.Forms.Panel ToolsPanel;
        private System.Windows.Forms.Label label2;
        private TextBoxFix YmapsTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ToolsPanelHideButton;
        private System.Windows.Forms.Button ToolsPanelShowButton;
        private System.Windows.Forms.CheckBox WireframeCheckBox;
        private System.Windows.Forms.TabControl ToolsTabControl;
        private System.Windows.Forms.TabPage ViewTabPage;
        private System.Windows.Forms.TabPage MarkersTabPage;
        private System.Windows.Forms.TabPage SelectionTabPage;
        private System.Windows.Forms.TabPage OptionsTabPage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox ViewModeComboBox;
        private System.Windows.Forms.CheckBox TimedEntitiesCheckBox;
        private System.Windows.Forms.CheckBox ErrorConsoleCheckBox;
        private System.Windows.Forms.Panel ConsolePanel;
        private TextBoxFix ConsoleTextBox;
        private System.Windows.Forms.Timer StatsUpdateTimer;
        private System.Windows.Forms.ToolStripStatusLabel StatsLabel;
        private System.Windows.Forms.TrackBar DetailTrackBar;
        private System.Windows.Forms.CheckBox DynamicLODCheckBox;
        private System.Windows.Forms.ComboBox MarkerStyleComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox LocatorStyleComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button ClearMarkersButton;
        private System.Windows.Forms.Button GoToButton;
        private System.Windows.Forms.CheckBox ShowLocatorCheckBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox LocateTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button AddMarkersButton;
        private TextBoxFix MultiFindTextBox;
        private System.Windows.Forms.CheckBox MarkerDepthClipCheckBox;
        private System.Windows.Forms.Button ResetMarkersButton;
        private System.Windows.Forms.Panel SelectedMarkerPanel;
        private System.Windows.Forms.TextBox SelectedMarkerPositionTextBox;
        private System.Windows.Forms.TextBox SelectedMarkerNameTextBox;
        private System.Windows.Forms.CheckBox SkydomeCheckBox;
        private System.Windows.Forms.CheckBox BoundsDepthClipCheckBox;
        private System.Windows.Forms.CheckBox MouseSelectCheckBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox BoundsStyleComboBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TrackBar BoundsRangeTrackBar;
        private System.Windows.Forms.Button AddCurrentPositonMarkerButton;
        private System.Windows.Forms.CheckBox SelectionBoundsCheckBox;
        private ReadOnlyPropertyGrid SelEntityPropertyGrid;
        private ReadOnlyPropertyGrid SelArchetypePropertyGrid;
        private System.Windows.Forms.TabControl SelectionTabControl;
        private System.Windows.Forms.TabPage SelectionEntityTabPage;
        private System.Windows.Forms.TabPage SelectionArchetypeTabPage;
        private System.Windows.Forms.ToolStripStatusLabel MousedLabel;
        private System.Windows.Forms.TabPage SelectionDrawableTabPage;
        private ReadOnlyPropertyGrid SelDrawablePropertyGrid;
        private System.Windows.Forms.Button ToolsPanelExpandButton;
        private System.Windows.Forms.Button AddSelectionMarkerButton;
        private System.Windows.Forms.CheckBox FullScreenCheckBox;
        private System.Windows.Forms.Button ResetSettingsButton;
        private System.Windows.Forms.Button SaveSettingsButton;
        private System.Windows.Forms.Button ToolsButton;
        private System.Windows.Forms.ContextMenuStrip ToolsMenu;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuConfigureGame;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuRPFBrowser;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuBinarySearch;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuJenkGen;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuExtractScripts;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuExtractTextures;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuExtractRawFiles;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuExtractShaders;
        private System.Windows.Forms.Button AboutButton;
        private System.Windows.Forms.CheckBox ShadowsCheckBox;
        private System.Windows.Forms.CheckBox StatusBarCheckBox;
        private System.Windows.Forms.CheckBox WaitForChildrenCheckBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox RenderModeComboBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox TextureSamplerComboBox;
        private System.Windows.Forms.TabControl OptionsTabControl;
        private System.Windows.Forms.TabPage OptionsGeneralTabPage;
        private System.Windows.Forms.TabPage OptionsHelpersTabPage;
        private System.Windows.Forms.CheckBox CollisionMeshesCheckBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TrackBar CollisionMeshRangeTrackBar;
        private System.Windows.Forms.CheckBox CollisionMeshLayer2CheckBox;
        private System.Windows.Forms.CheckBox CollisionMeshLayer1CheckBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox CollisionMeshLayer0CheckBox;
        private System.Windows.Forms.CheckBox ControlLightDirectionCheckBox;
        private System.Windows.Forms.TabControl ViewTabControl;
        private System.Windows.Forms.TabPage ViewWorldTabPage;
        private System.Windows.Forms.TabPage ViewYmapsTabPage;
        private System.Windows.Forms.TabPage ViewModelTabPage;
        private System.Windows.Forms.CheckBox ShowYmapChildrenCheckBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox TextureCoordsComboBox;
        private System.Windows.Forms.CheckBox AnisotropicFilteringCheckBox;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox WorldMaxLodComboBox;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TrackBar WorldLodDistTrackBar;
        private System.Windows.Forms.Label WorldLodDistLabel;
        private System.Windows.Forms.Label WorldDetailDistLabel;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TrackBar WorldDetailDistTrackBar;
        private System.Windows.Forms.CheckBox ProxiesCheckBox;
        private System.Windows.Forms.CheckBox CollisionMeshLayerDrawableCheckBox;
        private System.Windows.Forms.CheckBox InteriorsCheckBox;
        private System.Windows.Forms.TabPage OptionsLightingTabPage;
        private System.Windows.Forms.ComboBox WeatherComboBox;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label TimeOfDayLabel;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TrackBar TimeOfDayTrackBar;
        private System.Windows.Forms.CheckBox ControlTimeOfDayCheckBox;
        private System.Windows.Forms.CheckBox GrassCheckBox;
        private System.Windows.Forms.CheckBox TimedEntitiesAlwaysOnCheckBox;
        private System.Windows.Forms.CheckBox HDRRenderingCheckBox;
        private System.Windows.Forms.CheckBox ArtificialAmbientLightCheckBox;
        private System.Windows.Forms.CheckBox NaturalAmbientLightCheckBox;
        private System.Windows.Forms.CheckBox LODLightsCheckBox;
        private System.Windows.Forms.Label TimeSpeedLabel;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TrackBar TimeSpeedTrackBar;
        private System.Windows.Forms.Button TimeStartStopButton;
        private System.Windows.Forms.ComboBox CloudsComboBox;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label FieldOfViewLabel;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TrackBar FieldOfViewTrackBar;
        private System.Windows.Forms.TrackBar CloudParamTrackBar;
        private System.Windows.Forms.ComboBox CloudParamComboBox;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TabControl tabControl3;
        private System.Windows.Forms.TabPage tabPage11;
        private System.Windows.Forms.TabPage tabPage12;
        private System.Windows.Forms.TabPage tabPage13;
        private TreeViewFix SelDrawableModelsTreeView;
        private TreeViewFix SelDrawableTexturesTreeView;
        private System.Windows.Forms.CheckBox PathsCheckBox;
        private System.Windows.Forms.TabPage OptionsRenderTabPage;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuProjectWindow;
        private System.Windows.Forms.CheckBox WaterQuadsCheckBox;
        private System.Windows.Forms.ComboBox CameraModeComboBox;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.CheckBox SelectionWidgetCheckBox;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuSelectionInfo;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox CameraPositionTextBox;
        private System.Windows.Forms.Label MapViewDetailLabel;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.TrackBar MapViewDetailTrackBar;
        private System.Windows.Forms.CheckBox WorldScriptedYmapsCheckBox;
        private System.Windows.Forms.ComboBox WeatherRegionComboBox;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.CheckBox WorldYmapWeatherFilterCheckBox;
        private System.Windows.Forms.CheckBox WorldYmapTimeFilterCheckBox;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.ComboBox DlcLevelComboBox;
        private System.Windows.Forms.CheckBox EnableDlcCheckBox;
        private ToolStripFix Toolbar;
        private System.Windows.Forms.Panel ToolbarPanel;
        private System.Windows.Forms.ToolStripButton ToolbarMoveButton;
        private System.Windows.Forms.ToolStripButton ToolbarRotateButton;
        private System.Windows.Forms.ToolStripButton ToolbarScaleButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton ToolbarInfoWindowButton;
        private System.Windows.Forms.ToolStripButton ToolbarProjectWindowButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton ToolbarAddItemButton;
        private System.Windows.Forms.ToolStripButton ToolbarDeleteItemButton;
        private System.Windows.Forms.ToolStripSplitButton ToolbarNewButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarNewProjectButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarNewYmapButton;
        private System.Windows.Forms.ToolStripSplitButton ToolbarOpenButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarOpenProjectButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarOpenFilesButton;
        private System.Windows.Forms.ToolStripButton ToolbarSaveButton;
        private System.Windows.Forms.ToolStripButton ToolbarSaveAllButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSplitButton ToolbarUndoButton;
        private System.Windows.Forms.ToolStripSplitButton ToolbarRedoButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton ToolbarCopyButton;
        private System.Windows.Forms.ToolStripButton ToolbarPasteButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarUndoListButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarRedoListButton;
        private System.Windows.Forms.CheckBox ShowToolbarCheckBox;
        private System.Windows.Forms.ToolStripSplitButton ToolbarTransformSpaceButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarObjectSpaceButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarWorldSpaceButton;
        private System.Windows.Forms.TextBox SelectionNameTextBox;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.ComboBox SelectionModeComboBox;
        private System.Windows.Forms.TabPage SelectionExtensionTabPage;
        private ReadOnlyPropertyGrid SelExtensionPropertyGrid;
        private System.Windows.Forms.CheckBox EnableModsCheckBox;
        private System.Windows.Forms.Button AdvancedSettingsButton;
        private System.Windows.Forms.Button ControlSettingsButton;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuOptions;
        private ToolStripSplitButtonFix ToolbarSelectButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectEntityButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectEntityExtensionButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectArchetypeExtensionButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectTimeCycleModifierButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectCarGeneratorButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectGrassButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectWaterQuadButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectCollisionButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectPathButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectLodLightsButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectMloInstanceButton;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuJenkInd;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectTrainTrackButton;
        private System.Windows.Forms.CheckBox PathBoundsCheckBox;
        private System.Windows.Forms.ToolStripMenuItem ToolbarNewYndButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarNewTrainsButton;
        private System.Windows.Forms.CheckBox PathsDepthClipCheckBox;
        private System.Windows.Forms.CheckBox TrainPathsCheckBox;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectNavMeshButton;
        private System.Windows.Forms.CheckBox NavMeshesCheckBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSplitButton ToolbarCameraModeButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarCameraPerspectiveButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarCameraMapViewButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarCameraOrthographicButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectScenarioButton;
        private System.Windows.Forms.Panel ToolsDragPanel;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuWorldSearch;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuRPFExplorer;
        private System.Windows.Forms.ToolStripMenuItem ToolbarNewScenarioButton;
        private System.Windows.Forms.CheckBox PopZonesCheckBox;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectAudioButton;
        private System.Windows.Forms.CheckBox AudioOuterBoundsCheckBox;
        private System.Windows.Forms.CheckBox SkeletonsCheckBox;
        private ToolStripSplitButtonFix ToolbarSnapButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSnapToGroundButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSnapToGridButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSnapToGroundGridButton;
        private System.Windows.Forms.NumericUpDown SnapGridSizeUpDown;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.CheckBox RenderEntitiesCheckBox;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectOcclusionButton;
        private System.Windows.Forms.CheckBox CarGeneratorsCheckBox;
        private System.Windows.Forms.CheckBox HDTexturesCheckBox;
        private System.Windows.Forms.NumericUpDown FarClipUpDown;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.NumericUpDown NearClipUpDown;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuCutsceneViewer;
        private System.Windows.Forms.Label SubtitleLabel;
        private System.Windows.Forms.Timer SubtitleTimer;
        private System.Windows.Forms.CheckBox DeferredShadingCheckBox;
        private System.Windows.Forms.ToolStripMenuItem ToolbarNewYbnButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarNewYtypButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarOpenFolderButton;
        private System.Windows.Forms.NumericUpDown SnapAngleUpDown;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.ToolStripMenuItem ToolbarRotationSnappingButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarRotationSnappingOffButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarRotationSnapping1Button;
        private System.Windows.Forms.ToolStripMenuItem ToolbarRotationSnapping2Button;
        private System.Windows.Forms.ToolStripMenuItem ToolbarRotationSnapping5Button;
        private System.Windows.Forms.ToolStripMenuItem ToolbarRotationSnapping10Button;
        private System.Windows.Forms.ToolStripMenuItem ToolbarRotationSnappingCustomButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarRotationSnapping45Button;
        private System.Windows.Forms.ToolStripMenuItem ToolbarRotationSnapping90Button;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSnapGridSizeButton;
        private System.Windows.Forms.CheckBox HDLightsCheckBox;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectCalmingQuadButton;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSelectWaveQuadButton;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenuAudioExplorer;
        private System.Windows.Forms.CheckBox SaveTimeOfDayCheckBox;
        private System.Windows.Forms.CheckBox SavePositionCheckBox;
    }
}