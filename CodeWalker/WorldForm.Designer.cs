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
            base.Dispose(disposing);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label34;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorldForm));
            StatusStrip = new System.Windows.Forms.StatusStrip();
            StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            MousedLabel = new System.Windows.Forms.ToolStripStatusLabel();
            StatsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            ModelComboBox = new System.Windows.Forms.ComboBox();
            ToolsPanel = new System.Windows.Forms.Panel();
            ToolsDragPanel = new System.Windows.Forms.Panel();
            AboutButton = new System.Windows.Forms.Button();
            ToolsButton = new System.Windows.Forms.Button();
            ToolsPanelExpandButton = new System.Windows.Forms.Button();
            ToolsTabControl = new System.Windows.Forms.TabControl();
            ViewTabPage = new System.Windows.Forms.TabPage();
            ViewTabControl = new System.Windows.Forms.TabControl();
            ViewWorldTabPage = new System.Windows.Forms.TabPage();
            EnableModsCheckBox = new System.Windows.Forms.CheckBox();
            label30 = new System.Windows.Forms.Label();
            DlcLevelComboBox = new System.Windows.Forms.ComboBox();
            EnableDlcCheckBox = new System.Windows.Forms.CheckBox();
            WorldYmapWeatherFilterCheckBox = new System.Windows.Forms.CheckBox();
            WorldYmapTimeFilterCheckBox = new System.Windows.Forms.CheckBox();
            WorldScriptedYmapsCheckBox = new System.Windows.Forms.CheckBox();
            WorldDetailDistLabel = new System.Windows.Forms.Label();
            label18 = new System.Windows.Forms.Label();
            WorldDetailDistTrackBar = new System.Windows.Forms.TrackBar();
            WorldLodDistLabel = new System.Windows.Forms.Label();
            label16 = new System.Windows.Forms.Label();
            WorldLodDistTrackBar = new System.Windows.Forms.TrackBar();
            label15 = new System.Windows.Forms.Label();
            WorldMaxLodComboBox = new System.Windows.Forms.ComboBox();
            ViewYmapsTabPage = new System.Windows.Forms.TabPage();
            ShowYmapChildrenCheckBox = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            DetailTrackBar = new System.Windows.Forms.TrackBar();
            DynamicLODCheckBox = new System.Windows.Forms.CheckBox();
            YmapsTextBox = new TextBoxFix();
            ViewModelTabPage = new System.Windows.Forms.TabPage();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            ViewModeComboBox = new System.Windows.Forms.ComboBox();
            ShowToolbarCheckBox = new System.Windows.Forms.CheckBox();
            MarkersTabPage = new System.Windows.Forms.TabPage();
            label27 = new System.Windows.Forms.Label();
            CameraPositionTextBox = new System.Windows.Forms.TextBox();
            AddSelectionMarkerButton = new System.Windows.Forms.Button();
            AddCurrentPositonMarkerButton = new System.Windows.Forms.Button();
            ResetMarkersButton = new System.Windows.Forms.Button();
            ClearMarkersButton = new System.Windows.Forms.Button();
            GoToButton = new System.Windows.Forms.Button();
            ShowLocatorCheckBox = new System.Windows.Forms.CheckBox();
            label6 = new System.Windows.Forms.Label();
            LocateTextBox = new System.Windows.Forms.TextBox();
            label7 = new System.Windows.Forms.Label();
            AddMarkersButton = new System.Windows.Forms.Button();
            MultiFindTextBox = new TextBoxFix();
            SelectionTabPage = new System.Windows.Forms.TabPage();
            label25 = new System.Windows.Forms.Label();
            SelectionModeComboBox = new System.Windows.Forms.ComboBox();
            SelectionNameTextBox = new System.Windows.Forms.TextBox();
            SelectionTabControl = new System.Windows.Forms.TabControl();
            SelectionEntityTabPage = new System.Windows.Forms.TabPage();
            SelEntityPropertyGrid = new ReadOnlyPropertyGrid();
            SelectionArchetypeTabPage = new System.Windows.Forms.TabPage();
            SelArchetypePropertyGrid = new ReadOnlyPropertyGrid();
            SelectionDrawableTabPage = new System.Windows.Forms.TabPage();
            tabControl3 = new System.Windows.Forms.TabControl();
            tabPage11 = new System.Windows.Forms.TabPage();
            SelDrawablePropertyGrid = new ReadOnlyPropertyGrid();
            tabPage12 = new System.Windows.Forms.TabPage();
            SelDrawableModelsTreeView = new TreeViewFix();
            tabPage13 = new System.Windows.Forms.TabPage();
            SelDrawableTexturesTreeView = new TreeViewFix();
            SelectionExtensionTabPage = new System.Windows.Forms.TabPage();
            SelExtensionPropertyGrid = new ReadOnlyPropertyGrid();
            MouseSelectCheckBox = new System.Windows.Forms.CheckBox();
            OptionsTabPage = new System.Windows.Forms.TabPage();
            OptionsTabControl = new System.Windows.Forms.TabControl();
            OptionsGeneralTabPage = new System.Windows.Forms.TabPage();
            CarGeneratorsCheckBox = new System.Windows.Forms.CheckBox();
            RenderEntitiesCheckBox = new System.Windows.Forms.CheckBox();
            AdvancedSettingsButton = new System.Windows.Forms.Button();
            ControlSettingsButton = new System.Windows.Forms.Button();
            MapViewDetailLabel = new System.Windows.Forms.Label();
            label28 = new System.Windows.Forms.Label();
            MapViewDetailTrackBar = new System.Windows.Forms.TrackBar();
            CameraModeComboBox = new System.Windows.Forms.ComboBox();
            label24 = new System.Windows.Forms.Label();
            WaterQuadsCheckBox = new System.Windows.Forms.CheckBox();
            FieldOfViewLabel = new System.Windows.Forms.Label();
            label22 = new System.Windows.Forms.Label();
            TimedEntitiesAlwaysOnCheckBox = new System.Windows.Forms.CheckBox();
            GrassCheckBox = new System.Windows.Forms.CheckBox();
            InteriorsCheckBox = new System.Windows.Forms.CheckBox();
            CollisionMeshLayerDrawableCheckBox = new System.Windows.Forms.CheckBox();
            CollisionMeshLayer2CheckBox = new System.Windows.Forms.CheckBox();
            CollisionMeshLayer1CheckBox = new System.Windows.Forms.CheckBox();
            label13 = new System.Windows.Forms.Label();
            CollisionMeshLayer0CheckBox = new System.Windows.Forms.CheckBox();
            label12 = new System.Windows.Forms.Label();
            CollisionMeshRangeTrackBar = new System.Windows.Forms.TrackBar();
            CollisionMeshesCheckBox = new System.Windows.Forms.CheckBox();
            FullScreenCheckBox = new System.Windows.Forms.CheckBox();
            TimedEntitiesCheckBox = new System.Windows.Forms.CheckBox();
            FieldOfViewTrackBar = new System.Windows.Forms.TrackBar();
            OptionsRenderTabPage = new System.Windows.Forms.TabPage();
            AntiAliasingValue = new System.Windows.Forms.Label();
            AntiAliasingTrackBar = new System.Windows.Forms.TrackBar();
            FarClipUpDown = new System.Windows.Forms.NumericUpDown();
            label32 = new System.Windows.Forms.Label();
            NearClipUpDown = new System.Windows.Forms.NumericUpDown();
            label31 = new System.Windows.Forms.Label();
            HDTexturesCheckBox = new System.Windows.Forms.CheckBox();
            WireframeCheckBox = new System.Windows.Forms.CheckBox();
            RenderModeComboBox = new System.Windows.Forms.ComboBox();
            label11 = new System.Windows.Forms.Label();
            TextureSamplerComboBox = new System.Windows.Forms.ComboBox();
            TextureCoordsComboBox = new System.Windows.Forms.ComboBox();
            label10 = new System.Windows.Forms.Label();
            AnisotropicFilteringCheckBox = new System.Windows.Forms.CheckBox();
            ProxiesCheckBox = new System.Windows.Forms.CheckBox();
            WaitForChildrenCheckBox = new System.Windows.Forms.CheckBox();
            label14 = new System.Windows.Forms.Label();
            OptionsHelpersTabPage = new System.Windows.Forms.TabPage();
            SnapAngleUpDown = new System.Windows.Forms.NumericUpDown();
            label33 = new System.Windows.Forms.Label();
            SnapGridSizeUpDown = new System.Windows.Forms.NumericUpDown();
            label26 = new System.Windows.Forms.Label();
            SkeletonsCheckBox = new System.Windows.Forms.CheckBox();
            AudioOuterBoundsCheckBox = new System.Windows.Forms.CheckBox();
            PopZonesCheckBox = new System.Windows.Forms.CheckBox();
            NavMeshesCheckBox = new System.Windows.Forms.CheckBox();
            TrainPathsCheckBox = new System.Windows.Forms.CheckBox();
            PathsDepthClipCheckBox = new System.Windows.Forms.CheckBox();
            PathBoundsCheckBox = new System.Windows.Forms.CheckBox();
            SelectionWidgetCheckBox = new System.Windows.Forms.CheckBox();
            MarkerStyleComboBox = new System.Windows.Forms.ComboBox();
            label4 = new System.Windows.Forms.Label();
            LocatorStyleComboBox = new System.Windows.Forms.ComboBox();
            label5 = new System.Windows.Forms.Label();
            MarkerDepthClipCheckBox = new System.Windows.Forms.CheckBox();
            label9 = new System.Windows.Forms.Label();
            PathsCheckBox = new System.Windows.Forms.CheckBox();
            SelectionBoundsCheckBox = new System.Windows.Forms.CheckBox();
            BoundsDepthClipCheckBox = new System.Windows.Forms.CheckBox();
            BoundsRangeTrackBar = new System.Windows.Forms.TrackBar();
            BoundsStyleComboBox = new System.Windows.Forms.ComboBox();
            label8 = new System.Windows.Forms.Label();
            OptionsLightingTabPage = new System.Windows.Forms.TabPage();
            HDLightsCheckBox = new System.Windows.Forms.CheckBox();
            DeferredShadingCheckBox = new System.Windows.Forms.CheckBox();
            WeatherRegionComboBox = new System.Windows.Forms.ComboBox();
            label29 = new System.Windows.Forms.Label();
            CloudParamTrackBar = new System.Windows.Forms.TrackBar();
            CloudParamComboBox = new System.Windows.Forms.ComboBox();
            label23 = new System.Windows.Forms.Label();
            CloudsComboBox = new System.Windows.Forms.ComboBox();
            label21 = new System.Windows.Forms.Label();
            TimeSpeedLabel = new System.Windows.Forms.Label();
            label20 = new System.Windows.Forms.Label();
            TimeSpeedTrackBar = new System.Windows.Forms.TrackBar();
            TimeStartStopButton = new System.Windows.Forms.Button();
            ArtificialAmbientLightCheckBox = new System.Windows.Forms.CheckBox();
            NaturalAmbientLightCheckBox = new System.Windows.Forms.CheckBox();
            LODLightsCheckBox = new System.Windows.Forms.CheckBox();
            HDRRenderingCheckBox = new System.Windows.Forms.CheckBox();
            ControlTimeOfDayCheckBox = new System.Windows.Forms.CheckBox();
            TimeOfDayLabel = new System.Windows.Forms.Label();
            label19 = new System.Windows.Forms.Label();
            TimeOfDayTrackBar = new System.Windows.Forms.TrackBar();
            WeatherComboBox = new System.Windows.Forms.ComboBox();
            label17 = new System.Windows.Forms.Label();
            ControlLightDirectionCheckBox = new System.Windows.Forms.CheckBox();
            SkydomeCheckBox = new System.Windows.Forms.CheckBox();
            ShadowsCheckBox = new System.Windows.Forms.CheckBox();
            StatusBarCheckBox = new System.Windows.Forms.CheckBox();
            QuitButton = new System.Windows.Forms.Button();
            ReloadSettingsButton = new System.Windows.Forms.Button();
            SaveSettingsButton = new System.Windows.Forms.Button();
            ReloadShadersButton = new System.Windows.Forms.Button();
            ErrorConsoleCheckBox = new System.Windows.Forms.CheckBox();
            ToolsPanelHideButton = new System.Windows.Forms.Button();
            ToolsPanelShowButton = new System.Windows.Forms.Button();
            ConsolePanel = new System.Windows.Forms.Panel();
            ConsoleTextBox = new TextBoxFix();
            StatsUpdateTimer = new System.Windows.Forms.Timer(components);
            SelectedMarkerPanel = new System.Windows.Forms.Panel();
            SelectedMarkerPositionTextBox = new System.Windows.Forms.TextBox();
            SelectedMarkerNameTextBox = new System.Windows.Forms.TextBox();
            ToolsMenu = new System.Windows.Forms.ContextMenuStrip(components);
            ToolsMenuRPFBrowser = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuRPFExplorer = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuSelectionInfo = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuProjectWindow = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuCutsceneViewer = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuAudioExplorer = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuWorldSearch = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuBinarySearch = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuJenkGen = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuJenkInd = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuExtractScripts = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuExtractTextures = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuExtractRawFiles = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuExtractShaders = new System.Windows.Forms.ToolStripMenuItem();
            ToolsMenuOptions = new System.Windows.Forms.ToolStripMenuItem();
            Toolbar = new ToolStripFix();
            ToolbarNewButton = new System.Windows.Forms.ToolStripSplitButton();
            ToolbarNewProjectButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarNewYmapButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarNewYtypButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarNewYbnButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarNewYndButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarNewTrainsButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarNewScenarioButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarOpenButton = new System.Windows.Forms.ToolStripSplitButton();
            ToolbarOpenProjectButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarOpenFilesButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarOpenFolderButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSaveButton = new System.Windows.Forms.ToolStripButton();
            ToolbarSaveAllButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            ToolbarSelectButton = new ToolStripSplitButtonFix();
            ToolbarSelectEntityButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectEntityExtensionButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectArchetypeExtensionButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectTimeCycleModifierButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectCarGeneratorButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectGrassButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectWaterQuadButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectCalmingQuadButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectWaveQuadButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectCollisionButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectNavMeshButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectPathButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectTrainTrackButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectLodLightsButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectMloInstanceButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectScenarioButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectAudioButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSelectOcclusionButton = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            ToolbarMoveButton = new System.Windows.Forms.ToolStripButton();
            ToolbarRotateButton = new System.Windows.Forms.ToolStripButton();
            ToolbarScaleButton = new System.Windows.Forms.ToolStripButton();
            ToolbarTransformSpaceButton = new System.Windows.Forms.ToolStripSplitButton();
            ToolbarObjectSpaceButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarWorldSpaceButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSnapButton = new ToolStripSplitButtonFix();
            ToolbarSnapToGroundButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSnapToGridButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSnapToGroundGridButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarSnapGridSizeButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarRotationSnappingButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarRotationSnappingOffButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarRotationSnapping1Button = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarRotationSnapping2Button = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarRotationSnapping5Button = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarRotationSnapping10Button = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarRotationSnapping45Button = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarRotationSnapping90Button = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarRotationSnappingCustomButton = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            ToolbarUndoButton = new System.Windows.Forms.ToolStripSplitButton();
            ToolbarUndoListButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarRedoButton = new System.Windows.Forms.ToolStripSplitButton();
            ToolbarRedoListButton = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            ToolbarInfoWindowButton = new System.Windows.Forms.ToolStripButton();
            ToolbarProjectWindowButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            ToolbarAddItemButton = new System.Windows.Forms.ToolStripButton();
            ToolbarDeleteItemButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            ToolbarCopyButton = new System.Windows.Forms.ToolStripButton();
            ToolbarPasteButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            ToolbarCameraModeButton = new System.Windows.Forms.ToolStripSplitButton();
            ToolbarCameraPerspectiveButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarCameraMapViewButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarCameraOrthographicButton = new System.Windows.Forms.ToolStripMenuItem();
            ToolbarPanel = new System.Windows.Forms.Panel();
            SubtitleLabel = new System.Windows.Forms.Label();
            SubtitleTimer = new System.Windows.Forms.Timer(components);
            label34 = new System.Windows.Forms.Label();
            StatusStrip.SuspendLayout();
            ToolsPanel.SuspendLayout();
            ToolsTabControl.SuspendLayout();
            ViewTabPage.SuspendLayout();
            ViewTabControl.SuspendLayout();
            ViewWorldTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)WorldDetailDistTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)WorldLodDistTrackBar).BeginInit();
            ViewYmapsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DetailTrackBar).BeginInit();
            ViewModelTabPage.SuspendLayout();
            MarkersTabPage.SuspendLayout();
            SelectionTabPage.SuspendLayout();
            SelectionTabControl.SuspendLayout();
            SelectionEntityTabPage.SuspendLayout();
            SelectionArchetypeTabPage.SuspendLayout();
            SelectionDrawableTabPage.SuspendLayout();
            tabControl3.SuspendLayout();
            tabPage11.SuspendLayout();
            tabPage12.SuspendLayout();
            tabPage13.SuspendLayout();
            SelectionExtensionTabPage.SuspendLayout();
            OptionsTabPage.SuspendLayout();
            OptionsTabControl.SuspendLayout();
            OptionsGeneralTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MapViewDetailTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CollisionMeshRangeTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)FieldOfViewTrackBar).BeginInit();
            OptionsRenderTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)AntiAliasingTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)FarClipUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NearClipUpDown).BeginInit();
            OptionsHelpersTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)SnapAngleUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)SnapGridSizeUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BoundsRangeTrackBar).BeginInit();
            OptionsLightingTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CloudParamTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TimeSpeedTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TimeOfDayTrackBar).BeginInit();
            ConsolePanel.SuspendLayout();
            SelectedMarkerPanel.SuspendLayout();
            ToolsMenu.SuspendLayout();
            Toolbar.SuspendLayout();
            ToolbarPanel.SuspendLayout();
            SuspendLayout();
            // 
            // label34
            // 
            label34.AutoSize = true;
            label34.Location = new System.Drawing.Point(8, 292);
            label34.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label34.Name = "label34";
            label34.Size = new System.Drawing.Size(76, 15);
            label34.TabIndex = 63;
            label34.Text = "Anti-Aliasing";
            // 
            // StatusStrip
            // 
            StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { StatusLabel, MousedLabel, StatsLabel });
            StatusStrip.Location = new System.Drawing.Point(0, 798);
            StatusStrip.Name = "StatusStrip";
            StatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            StatusStrip.Size = new System.Drawing.Size(1148, 22);
            StatusStrip.TabIndex = 0;
            StatusStrip.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            StatusLabel.BackColor = System.Drawing.SystemColors.Control;
            StatusLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new System.Drawing.Size(1040, 17);
            StatusLabel.Spring = true;
            StatusLabel.Text = "Initialising";
            StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MousedLabel
            // 
            MousedLabel.BackColor = System.Drawing.SystemColors.Control;
            MousedLabel.Name = "MousedLabel";
            MousedLabel.Size = new System.Drawing.Size(16, 17);
            MousedLabel.Text = "   ";
            // 
            // StatsLabel
            // 
            StatsLabel.BackColor = System.Drawing.SystemColors.Control;
            StatsLabel.DoubleClickEnabled = true;
            StatsLabel.Name = "StatsLabel";
            StatsLabel.Size = new System.Drawing.Size(75, 17);
            StatsLabel.Text = "0 geometries";
            StatsLabel.DoubleClick += StatsLabel_DoubleClick;
            // 
            // ModelComboBox
            // 
            ModelComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ModelComboBox.FormattingEnabled = true;
            ModelComboBox.Items.AddRange(new object[] { "dt1_lod_slod3", "dt1_tc_dufo_core", "dt1_tc_ufocore", "ex_office_citymodel_01", "id1_30_build3_dtl2", "imp_prop_ship_01a", "prop_alien_egg_01", "prop_fruit_stand_02", "prop_fruit_stand_03", "dune", "dune2", "dune2_hi", "adder", "adder_hi", "kuruma2", "kuruma2_hi", "infernus", "infernus_hi", "buzzard", "buzzard_hi", "rhino", "rhino_hi", "lazer", "lazer_hi", "duster", "duster_hi", "marquis", "marquis_hi", "submersible", "submersible_hi", "cargobob", "cargobob_hi", "sanchez", "sanchez_hi" });
            ModelComboBox.Location = new System.Drawing.Point(51, 8);
            ModelComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ModelComboBox.Name = "ModelComboBox";
            ModelComboBox.Size = new System.Drawing.Size(174, 23);
            ModelComboBox.TabIndex = 11;
            ModelComboBox.SelectedIndexChanged += ModelComboBox_SelectedIndexChanged;
            ModelComboBox.TextUpdate += ModelComboBox_TextUpdate;
            // 
            // ToolsPanel
            // 
            ToolsPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            ToolsPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            ToolsPanel.Controls.Add(ToolsDragPanel);
            ToolsPanel.Controls.Add(AboutButton);
            ToolsPanel.Controls.Add(ToolsButton);
            ToolsPanel.Controls.Add(ToolsPanelExpandButton);
            ToolsPanel.Controls.Add(ToolsTabControl);
            ToolsPanel.Controls.Add(ToolsPanelHideButton);
            ToolsPanel.Location = new System.Drawing.Point(880, 14);
            ToolsPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ToolsPanel.Name = "ToolsPanel";
            ToolsPanel.Size = new System.Drawing.Size(254, 767);
            ToolsPanel.TabIndex = 2;
            ToolsPanel.Visible = false;
            // 
            // ToolsDragPanel
            // 
            ToolsDragPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ToolsDragPanel.Cursor = System.Windows.Forms.Cursors.VSplit;
            ToolsDragPanel.Location = new System.Drawing.Point(0, 0);
            ToolsDragPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ToolsDragPanel.Name = "ToolsDragPanel";
            ToolsDragPanel.Size = new System.Drawing.Size(5, 767);
            ToolsDragPanel.TabIndex = 16;
            ToolsDragPanel.MouseDown += ToolsDragPanel_MouseDown;
            ToolsDragPanel.MouseMove += ToolsDragPanel_MouseMove;
            ToolsDragPanel.MouseUp += ToolsDragPanel_MouseUp;
            // 
            // AboutButton
            // 
            AboutButton.Location = new System.Drawing.Point(75, 3);
            AboutButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AboutButton.Name = "AboutButton";
            AboutButton.Size = new System.Drawing.Size(64, 27);
            AboutButton.TabIndex = 15;
            AboutButton.Text = "About...";
            AboutButton.UseVisualStyleBackColor = true;
            AboutButton.Click += AboutButton_Click;
            // 
            // ToolsButton
            // 
            ToolsButton.Location = new System.Drawing.Point(4, 3);
            ToolsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ToolsButton.Name = "ToolsButton";
            ToolsButton.Size = new System.Drawing.Size(64, 27);
            ToolsButton.TabIndex = 14;
            ToolsButton.Text = "Tools...";
            ToolsButton.UseVisualStyleBackColor = true;
            ToolsButton.Click += ToolsButton_Click;
            // 
            // ToolsPanelExpandButton
            // 
            ToolsPanelExpandButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            ToolsPanelExpandButton.Location = new System.Drawing.Point(174, 3);
            ToolsPanelExpandButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ToolsPanelExpandButton.Name = "ToolsPanelExpandButton";
            ToolsPanelExpandButton.Size = new System.Drawing.Size(35, 27);
            ToolsPanelExpandButton.TabIndex = 13;
            ToolsPanelExpandButton.Text = "<<";
            ToolsPanelExpandButton.UseVisualStyleBackColor = true;
            ToolsPanelExpandButton.Click += ToolsPanelExpandButton_Click;
            // 
            // ToolsTabControl
            // 
            ToolsTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ToolsTabControl.Controls.Add(ViewTabPage);
            ToolsTabControl.Controls.Add(MarkersTabPage);
            ToolsTabControl.Controls.Add(SelectionTabPage);
            ToolsTabControl.Controls.Add(OptionsTabPage);
            ToolsTabControl.Location = new System.Drawing.Point(4, 35);
            ToolsTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ToolsTabControl.Name = "ToolsTabControl";
            ToolsTabControl.SelectedIndex = 0;
            ToolsTabControl.Size = new System.Drawing.Size(248, 729);
            ToolsTabControl.TabIndex = 12;
            // 
            // ViewTabPage
            // 
            ViewTabPage.Controls.Add(ViewTabControl);
            ViewTabPage.Controls.Add(label3);
            ViewTabPage.Controls.Add(ViewModeComboBox);
            ViewTabPage.Controls.Add(ShowToolbarCheckBox);
            ViewTabPage.Location = new System.Drawing.Point(4, 24);
            ViewTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ViewTabPage.Name = "ViewTabPage";
            ViewTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ViewTabPage.Size = new System.Drawing.Size(240, 701);
            ViewTabPage.TabIndex = 0;
            ViewTabPage.Text = "View";
            ViewTabPage.UseVisualStyleBackColor = true;
            // 
            // ViewTabControl
            // 
            ViewTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ViewTabControl.Controls.Add(ViewWorldTabPage);
            ViewTabControl.Controls.Add(ViewYmapsTabPage);
            ViewTabControl.Controls.Add(ViewModelTabPage);
            ViewTabControl.Location = new System.Drawing.Point(0, 37);
            ViewTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ViewTabControl.Name = "ViewTabControl";
            ViewTabControl.SelectedIndex = 0;
            ViewTabControl.Size = new System.Drawing.Size(236, 632);
            ViewTabControl.TabIndex = 12;
            // 
            // ViewWorldTabPage
            // 
            ViewWorldTabPage.Controls.Add(EnableModsCheckBox);
            ViewWorldTabPage.Controls.Add(label30);
            ViewWorldTabPage.Controls.Add(DlcLevelComboBox);
            ViewWorldTabPage.Controls.Add(EnableDlcCheckBox);
            ViewWorldTabPage.Controls.Add(WorldYmapWeatherFilterCheckBox);
            ViewWorldTabPage.Controls.Add(WorldYmapTimeFilterCheckBox);
            ViewWorldTabPage.Controls.Add(WorldScriptedYmapsCheckBox);
            ViewWorldTabPage.Controls.Add(WorldDetailDistLabel);
            ViewWorldTabPage.Controls.Add(label18);
            ViewWorldTabPage.Controls.Add(WorldDetailDistTrackBar);
            ViewWorldTabPage.Controls.Add(WorldLodDistLabel);
            ViewWorldTabPage.Controls.Add(label16);
            ViewWorldTabPage.Controls.Add(WorldLodDistTrackBar);
            ViewWorldTabPage.Controls.Add(label15);
            ViewWorldTabPage.Controls.Add(WorldMaxLodComboBox);
            ViewWorldTabPage.Location = new System.Drawing.Point(4, 24);
            ViewWorldTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ViewWorldTabPage.Name = "ViewWorldTabPage";
            ViewWorldTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ViewWorldTabPage.Size = new System.Drawing.Size(228, 604);
            ViewWorldTabPage.TabIndex = 0;
            ViewWorldTabPage.Text = "World";
            ViewWorldTabPage.UseVisualStyleBackColor = true;
            // 
            // EnableModsCheckBox
            // 
            EnableModsCheckBox.AutoSize = true;
            EnableModsCheckBox.Enabled = false;
            EnableModsCheckBox.Location = new System.Drawing.Point(7, 323);
            EnableModsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            EnableModsCheckBox.Name = "EnableModsCheckBox";
            EnableModsCheckBox.Size = new System.Drawing.Size(94, 19);
            EnableModsCheckBox.TabIndex = 68;
            EnableModsCheckBox.Text = "Enable Mods";
            EnableModsCheckBox.UseVisualStyleBackColor = true;
            EnableModsCheckBox.CheckedChanged += EnableModsCheckBox_CheckedChanged;
            // 
            // label30
            // 
            label30.AutoSize = true;
            label30.Location = new System.Drawing.Point(1, 389);
            label30.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label30.Name = "label30";
            label30.Size = new System.Drawing.Size(62, 15);
            label30.TabIndex = 70;
            label30.Text = "DLC Level:";
            // 
            // DlcLevelComboBox
            // 
            DlcLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            DlcLevelComboBox.Enabled = false;
            DlcLevelComboBox.FormattingEnabled = true;
            DlcLevelComboBox.Items.AddRange(new object[] { "<Loading...>" });
            DlcLevelComboBox.Location = new System.Drawing.Point(72, 385);
            DlcLevelComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            DlcLevelComboBox.Name = "DlcLevelComboBox";
            DlcLevelComboBox.Size = new System.Drawing.Size(146, 23);
            DlcLevelComboBox.TabIndex = 70;
            DlcLevelComboBox.SelectedIndexChanged += DlcLevelComboBox_SelectedIndexChanged;
            DlcLevelComboBox.KeyPress += DlcLevelComboBox_KeyPress;
            // 
            // EnableDlcCheckBox
            // 
            EnableDlcCheckBox.AutoSize = true;
            EnableDlcCheckBox.Enabled = false;
            EnableDlcCheckBox.Location = new System.Drawing.Point(7, 359);
            EnableDlcCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            EnableDlcCheckBox.Name = "EnableDlcCheckBox";
            EnableDlcCheckBox.Size = new System.Drawing.Size(86, 19);
            EnableDlcCheckBox.TabIndex = 69;
            EnableDlcCheckBox.Text = "Enable DLC";
            EnableDlcCheckBox.UseVisualStyleBackColor = true;
            EnableDlcCheckBox.CheckedChanged += EnableDlcCheckBox_CheckedChanged;
            // 
            // WorldYmapWeatherFilterCheckBox
            // 
            WorldYmapWeatherFilterCheckBox.AutoSize = true;
            WorldYmapWeatherFilterCheckBox.Checked = true;
            WorldYmapWeatherFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            WorldYmapWeatherFilterCheckBox.Location = new System.Drawing.Point(7, 273);
            WorldYmapWeatherFilterCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WorldYmapWeatherFilterCheckBox.Name = "WorldYmapWeatherFilterCheckBox";
            WorldYmapWeatherFilterCheckBox.Size = new System.Drawing.Size(151, 19);
            WorldYmapWeatherFilterCheckBox.TabIndex = 67;
            WorldYmapWeatherFilterCheckBox.Text = "Filter ymaps by weather";
            WorldYmapWeatherFilterCheckBox.UseVisualStyleBackColor = true;
            WorldYmapWeatherFilterCheckBox.CheckedChanged += WorldYmapWeatherFilterCheckBox_CheckedChanged;
            // 
            // WorldYmapTimeFilterCheckBox
            // 
            WorldYmapTimeFilterCheckBox.AutoSize = true;
            WorldYmapTimeFilterCheckBox.Checked = true;
            WorldYmapTimeFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            WorldYmapTimeFilterCheckBox.Location = new System.Drawing.Point(7, 247);
            WorldYmapTimeFilterCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WorldYmapTimeFilterCheckBox.Name = "WorldYmapTimeFilterCheckBox";
            WorldYmapTimeFilterCheckBox.Size = new System.Drawing.Size(169, 19);
            WorldYmapTimeFilterCheckBox.TabIndex = 66;
            WorldYmapTimeFilterCheckBox.Text = "Filter ymaps by time of day";
            WorldYmapTimeFilterCheckBox.UseVisualStyleBackColor = true;
            WorldYmapTimeFilterCheckBox.CheckedChanged += WorldYmapTimeFilterCheckBox_CheckedChanged;
            // 
            // WorldScriptedYmapsCheckBox
            // 
            WorldScriptedYmapsCheckBox.AutoSize = true;
            WorldScriptedYmapsCheckBox.Checked = true;
            WorldScriptedYmapsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            WorldScriptedYmapsCheckBox.Location = new System.Drawing.Point(7, 210);
            WorldScriptedYmapsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WorldScriptedYmapsCheckBox.Name = "WorldScriptedYmapsCheckBox";
            WorldScriptedYmapsCheckBox.Size = new System.Drawing.Size(138, 19);
            WorldScriptedYmapsCheckBox.TabIndex = 65;
            WorldScriptedYmapsCheckBox.Text = "Show scripted ymaps";
            WorldScriptedYmapsCheckBox.UseVisualStyleBackColor = true;
            WorldScriptedYmapsCheckBox.CheckedChanged += WorldScriptedYmapsCheckBox_CheckedChanged;
            // 
            // WorldDetailDistLabel
            // 
            WorldDetailDistLabel.AutoSize = true;
            WorldDetailDistLabel.Location = new System.Drawing.Point(102, 108);
            WorldDetailDistLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            WorldDetailDistLabel.Name = "WorldDetailDistLabel";
            WorldDetailDistLabel.Size = new System.Drawing.Size(22, 15);
            WorldDetailDistLabel.TabIndex = 64;
            WorldDetailDistLabel.Text = "1.0";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new System.Drawing.Point(1, 108);
            label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label18.Name = "label18";
            label18.Size = new System.Drawing.Size(87, 15);
            label18.TabIndex = 63;
            label18.Text = "Detail distance:";
            // 
            // WorldDetailDistTrackBar
            // 
            WorldDetailDistTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            WorldDetailDistTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            WorldDetailDistTrackBar.LargeChange = 10;
            WorldDetailDistTrackBar.Location = new System.Drawing.Point(7, 127);
            WorldDetailDistTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WorldDetailDistTrackBar.Maximum = 50;
            WorldDetailDistTrackBar.Name = "WorldDetailDistTrackBar";
            WorldDetailDistTrackBar.Size = new System.Drawing.Size(212, 45);
            WorldDetailDistTrackBar.TabIndex = 62;
            WorldDetailDistTrackBar.TickFrequency = 2;
            WorldDetailDistTrackBar.Value = 10;
            WorldDetailDistTrackBar.Scroll += WorldDetailDistTrackBar_Scroll;
            // 
            // WorldLodDistLabel
            // 
            WorldLodDistLabel.AutoSize = true;
            WorldLodDistLabel.Location = new System.Drawing.Point(96, 45);
            WorldLodDistLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            WorldLodDistLabel.Name = "WorldLodDistLabel";
            WorldLodDistLabel.Size = new System.Drawing.Size(22, 15);
            WorldLodDistLabel.TabIndex = 61;
            WorldLodDistLabel.Text = "1.0";
            WorldLodDistLabel.Visible = false;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new System.Drawing.Point(1, 45);
            label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label16.Name = "label16";
            label16.Size = new System.Drawing.Size(80, 15);
            label16.TabIndex = 60;
            label16.Text = "LOD distance:";
            label16.Visible = false;
            // 
            // WorldLodDistTrackBar
            // 
            WorldLodDistTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            WorldLodDistTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            WorldLodDistTrackBar.LargeChange = 10;
            WorldLodDistTrackBar.Location = new System.Drawing.Point(7, 63);
            WorldLodDistTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WorldLodDistTrackBar.Maximum = 30;
            WorldLodDistTrackBar.Name = "WorldLodDistTrackBar";
            WorldLodDistTrackBar.Size = new System.Drawing.Size(212, 45);
            WorldLodDistTrackBar.TabIndex = 59;
            WorldLodDistTrackBar.TickFrequency = 2;
            WorldLodDistTrackBar.Value = 10;
            WorldLodDistTrackBar.Visible = false;
            WorldLodDistTrackBar.Scroll += WorldLodDistTrackBar_Scroll;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new System.Drawing.Point(1, 10);
            label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(59, 15);
            label15.TabIndex = 58;
            label15.Text = "Max LOD:";
            // 
            // WorldMaxLodComboBox
            // 
            WorldMaxLodComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            WorldMaxLodComboBox.FormattingEnabled = true;
            WorldMaxLodComboBox.Items.AddRange(new object[] { "ORPHANHD", "HD", "LOD", "SLOD1", "SLOD2", "SLOD3", "SLOD4" });
            WorldMaxLodComboBox.Location = new System.Drawing.Point(72, 7);
            WorldMaxLodComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WorldMaxLodComboBox.Name = "WorldMaxLodComboBox";
            WorldMaxLodComboBox.Size = new System.Drawing.Size(146, 23);
            WorldMaxLodComboBox.TabIndex = 57;
            WorldMaxLodComboBox.SelectedIndexChanged += WorldMaxLodComboBox_SelectedIndexChanged;
            WorldMaxLodComboBox.KeyPress += WorldMaxLodComboBox_KeyPress;
            // 
            // ViewYmapsTabPage
            // 
            ViewYmapsTabPage.Controls.Add(ShowYmapChildrenCheckBox);
            ViewYmapsTabPage.Controls.Add(label2);
            ViewYmapsTabPage.Controls.Add(DetailTrackBar);
            ViewYmapsTabPage.Controls.Add(DynamicLODCheckBox);
            ViewYmapsTabPage.Controls.Add(YmapsTextBox);
            ViewYmapsTabPage.Location = new System.Drawing.Point(4, 24);
            ViewYmapsTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ViewYmapsTabPage.Name = "ViewYmapsTabPage";
            ViewYmapsTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ViewYmapsTabPage.Size = new System.Drawing.Size(228, 604);
            ViewYmapsTabPage.TabIndex = 1;
            ViewYmapsTabPage.Text = "Ymaps";
            ViewYmapsTabPage.UseVisualStyleBackColor = true;
            // 
            // ShowYmapChildrenCheckBox
            // 
            ShowYmapChildrenCheckBox.AutoSize = true;
            ShowYmapChildrenCheckBox.Enabled = false;
            ShowYmapChildrenCheckBox.Location = new System.Drawing.Point(7, 68);
            ShowYmapChildrenCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ShowYmapChildrenCheckBox.Name = "ShowYmapChildrenCheckBox";
            ShowYmapChildrenCheckBox.Size = new System.Drawing.Size(101, 19);
            ShowYmapChildrenCheckBox.TabIndex = 35;
            ShowYmapChildrenCheckBox.Text = "Show children";
            ShowYmapChildrenCheckBox.UseVisualStyleBackColor = true;
            ShowYmapChildrenCheckBox.CheckedChanged += ShowYmapChildrenCheckBox_CheckedChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(4, 102);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(85, 15);
            label2.TabIndex = 8;
            label2.Text = "Ymaps to load:";
            // 
            // DetailTrackBar
            // 
            DetailTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            DetailTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            DetailTrackBar.Location = new System.Drawing.Point(7, 28);
            DetailTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            DetailTrackBar.Maximum = 20;
            DetailTrackBar.Name = "DetailTrackBar";
            DetailTrackBar.Size = new System.Drawing.Size(212, 45);
            DetailTrackBar.TabIndex = 34;
            DetailTrackBar.Value = 5;
            DetailTrackBar.Scroll += DetailTrackBar_Scroll;
            // 
            // DynamicLODCheckBox
            // 
            DynamicLODCheckBox.AutoSize = true;
            DynamicLODCheckBox.Checked = true;
            DynamicLODCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            DynamicLODCheckBox.Location = new System.Drawing.Point(7, 7);
            DynamicLODCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            DynamicLODCheckBox.Name = "DynamicLODCheckBox";
            DynamicLODCheckBox.Size = new System.Drawing.Size(99, 19);
            DynamicLODCheckBox.TabIndex = 33;
            DynamicLODCheckBox.Text = "Dynamic LOD";
            DynamicLODCheckBox.UseVisualStyleBackColor = true;
            DynamicLODCheckBox.CheckedChanged += DynamicLODCheckBox_CheckedChanged;
            // 
            // YmapsTextBox
            // 
            YmapsTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            YmapsTextBox.Location = new System.Drawing.Point(0, 120);
            YmapsTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            YmapsTextBox.Multiline = true;
            YmapsTextBox.Name = "YmapsTextBox";
            YmapsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            YmapsTextBox.Size = new System.Drawing.Size(226, 479);
            YmapsTextBox.TabIndex = 36;
            YmapsTextBox.Text = resources.GetString("YmapsTextBox.Text");
            YmapsTextBox.TextChanged += YmapsTextBox_TextChanged;
            // 
            // ViewModelTabPage
            // 
            ViewModelTabPage.Controls.Add(label1);
            ViewModelTabPage.Controls.Add(ModelComboBox);
            ViewModelTabPage.Location = new System.Drawing.Point(4, 24);
            ViewModelTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ViewModelTabPage.Name = "ViewModelTabPage";
            ViewModelTabPage.Size = new System.Drawing.Size(228, 604);
            ViewModelTabPage.TabIndex = 2;
            ViewModelTabPage.Text = "Model";
            ViewModelTabPage.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(-1, 12);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(44, 15);
            label1.TabIndex = 5;
            label1.Text = "Model:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(4, 9);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(41, 15);
            label3.TabIndex = 11;
            label3.Text = "Mode:";
            // 
            // ViewModeComboBox
            // 
            ViewModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ViewModeComboBox.FormattingEnabled = true;
            ViewModeComboBox.Items.AddRange(new object[] { "World view", "Ymap view", "Model view" });
            ViewModeComboBox.Location = new System.Drawing.Point(56, 6);
            ViewModeComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ViewModeComboBox.Name = "ViewModeComboBox";
            ViewModeComboBox.Size = new System.Drawing.Size(129, 23);
            ViewModeComboBox.TabIndex = 10;
            ViewModeComboBox.SelectedIndexChanged += ViewModeComboBox_SelectedIndexChanged;
            ViewModeComboBox.KeyPress += ViewModeComboBox_KeyPress;
            // 
            // ShowToolbarCheckBox
            // 
            ShowToolbarCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ShowToolbarCheckBox.AutoSize = true;
            ShowToolbarCheckBox.Location = new System.Drawing.Point(12, 677);
            ShowToolbarCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ShowToolbarCheckBox.Name = "ShowToolbarCheckBox";
            ShowToolbarCheckBox.Size = new System.Drawing.Size(114, 19);
            ShowToolbarCheckBox.TabIndex = 47;
            ShowToolbarCheckBox.Text = "Show Toolbar (T)";
            ShowToolbarCheckBox.UseVisualStyleBackColor = true;
            ShowToolbarCheckBox.CheckedChanged += ShowToolbarCheckBox_CheckedChanged;
            // 
            // MarkersTabPage
            // 
            MarkersTabPage.Controls.Add(label27);
            MarkersTabPage.Controls.Add(CameraPositionTextBox);
            MarkersTabPage.Controls.Add(AddSelectionMarkerButton);
            MarkersTabPage.Controls.Add(AddCurrentPositonMarkerButton);
            MarkersTabPage.Controls.Add(ResetMarkersButton);
            MarkersTabPage.Controls.Add(ClearMarkersButton);
            MarkersTabPage.Controls.Add(GoToButton);
            MarkersTabPage.Controls.Add(ShowLocatorCheckBox);
            MarkersTabPage.Controls.Add(label6);
            MarkersTabPage.Controls.Add(LocateTextBox);
            MarkersTabPage.Controls.Add(label7);
            MarkersTabPage.Controls.Add(AddMarkersButton);
            MarkersTabPage.Controls.Add(MultiFindTextBox);
            MarkersTabPage.Location = new System.Drawing.Point(4, 24);
            MarkersTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MarkersTabPage.Name = "MarkersTabPage";
            MarkersTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MarkersTabPage.Size = new System.Drawing.Size(240, 701);
            MarkersTabPage.TabIndex = 1;
            MarkersTabPage.Text = "Markers";
            MarkersTabPage.UseVisualStyleBackColor = true;
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Location = new System.Drawing.Point(-2, 58);
            label27.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label27.Name = "label27";
            label27.Size = new System.Drawing.Size(138, 15);
            label27.TabIndex = 22;
            label27.Text = "Current camera position:";
            // 
            // CameraPositionTextBox
            // 
            CameraPositionTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            CameraPositionTextBox.Location = new System.Drawing.Point(0, 77);
            CameraPositionTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CameraPositionTextBox.Name = "CameraPositionTextBox";
            CameraPositionTextBox.Size = new System.Drawing.Size(238, 23);
            CameraPositionTextBox.TabIndex = 16;
            CameraPositionTextBox.Text = "0, 0, 0";
            // 
            // AddSelectionMarkerButton
            // 
            AddSelectionMarkerButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            AddSelectionMarkerButton.Location = new System.Drawing.Point(0, 618);
            AddSelectionMarkerButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AddSelectionMarkerButton.Name = "AddSelectionMarkerButton";
            AddSelectionMarkerButton.Size = new System.Drawing.Size(113, 27);
            AddSelectionMarkerButton.TabIndex = 22;
            AddSelectionMarkerButton.Text = "Add selection";
            AddSelectionMarkerButton.UseVisualStyleBackColor = true;
            AddSelectionMarkerButton.Click += AddSelectionMarkerButton_Click;
            // 
            // AddCurrentPositonMarkerButton
            // 
            AddCurrentPositonMarkerButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            AddCurrentPositonMarkerButton.Location = new System.Drawing.Point(0, 585);
            AddCurrentPositonMarkerButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AddCurrentPositonMarkerButton.Name = "AddCurrentPositonMarkerButton";
            AddCurrentPositonMarkerButton.Size = new System.Drawing.Size(113, 27);
            AddCurrentPositonMarkerButton.TabIndex = 20;
            AddCurrentPositonMarkerButton.Text = "Add current pos";
            AddCurrentPositonMarkerButton.UseVisualStyleBackColor = true;
            AddCurrentPositonMarkerButton.Click += AddCurrentPositonMarkerButton_Click;
            // 
            // ResetMarkersButton
            // 
            ResetMarkersButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ResetMarkersButton.Location = new System.Drawing.Point(126, 585);
            ResetMarkersButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ResetMarkersButton.Name = "ResetMarkersButton";
            ResetMarkersButton.Size = new System.Drawing.Size(113, 27);
            ResetMarkersButton.TabIndex = 21;
            ResetMarkersButton.Text = "Default markers";
            ResetMarkersButton.UseVisualStyleBackColor = true;
            ResetMarkersButton.Click += ResetMarkersButton_Click;
            // 
            // ClearMarkersButton
            // 
            ClearMarkersButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ClearMarkersButton.Location = new System.Drawing.Point(126, 552);
            ClearMarkersButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ClearMarkersButton.Name = "ClearMarkersButton";
            ClearMarkersButton.Size = new System.Drawing.Size(113, 27);
            ClearMarkersButton.TabIndex = 19;
            ClearMarkersButton.Text = "Clear markers";
            ClearMarkersButton.UseVisualStyleBackColor = true;
            ClearMarkersButton.Click += ClearMarkersButton_Click;
            // 
            // GoToButton
            // 
            GoToButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            GoToButton.Location = new System.Drawing.Point(189, 28);
            GoToButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            GoToButton.Name = "GoToButton";
            GoToButton.Size = new System.Drawing.Size(50, 25);
            GoToButton.TabIndex = 15;
            GoToButton.Text = "Go to";
            GoToButton.UseVisualStyleBackColor = true;
            GoToButton.Click += GoToButton_Click;
            // 
            // ShowLocatorCheckBox
            // 
            ShowLocatorCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            ShowLocatorCheckBox.AutoSize = true;
            ShowLocatorCheckBox.Location = new System.Drawing.Point(126, 9);
            ShowLocatorCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ShowLocatorCheckBox.Name = "ShowLocatorCheckBox";
            ShowLocatorCheckBox.Size = new System.Drawing.Size(95, 19);
            ShowLocatorCheckBox.TabIndex = 13;
            ShowLocatorCheckBox.Text = "Show marker";
            ShowLocatorCheckBox.UseVisualStyleBackColor = true;
            ShowLocatorCheckBox.CheckedChanged += ShowLocatorCheckBox_CheckedChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(-2, 9);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(81, 15);
            label6.TabIndex = 13;
            label6.Text = "Locate: X, Y, Z";
            // 
            // LocateTextBox
            // 
            LocateTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            LocateTextBox.Location = new System.Drawing.Point(0, 29);
            LocateTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LocateTextBox.Name = "LocateTextBox";
            LocateTextBox.Size = new System.Drawing.Size(181, 23);
            LocateTextBox.TabIndex = 14;
            LocateTextBox.Text = "0, 0, 0";
            LocateTextBox.TextChanged += LocateTextBox_TextChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(-2, 117);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(138, 15);
            label7.TabIndex = 11;
            label7.Text = "Multi-find: X, Y, Z, Name";
            // 
            // AddMarkersButton
            // 
            AddMarkersButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            AddMarkersButton.Location = new System.Drawing.Point(0, 552);
            AddMarkersButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AddMarkersButton.Name = "AddMarkersButton";
            AddMarkersButton.Size = new System.Drawing.Size(113, 27);
            AddMarkersButton.TabIndex = 18;
            AddMarkersButton.Text = "Add markers";
            AddMarkersButton.UseVisualStyleBackColor = true;
            AddMarkersButton.Click += AddMarkersButton_Click;
            // 
            // MultiFindTextBox
            // 
            MultiFindTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            MultiFindTextBox.Location = new System.Drawing.Point(0, 135);
            MultiFindTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MultiFindTextBox.MaxLength = 1048576;
            MultiFindTextBox.Multiline = true;
            MultiFindTextBox.Name = "MultiFindTextBox";
            MultiFindTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            MultiFindTextBox.Size = new System.Drawing.Size(238, 409);
            MultiFindTextBox.TabIndex = 17;
            // 
            // SelectionTabPage
            // 
            SelectionTabPage.Controls.Add(label25);
            SelectionTabPage.Controls.Add(SelectionModeComboBox);
            SelectionTabPage.Controls.Add(SelectionNameTextBox);
            SelectionTabPage.Controls.Add(SelectionTabControl);
            SelectionTabPage.Controls.Add(MouseSelectCheckBox);
            SelectionTabPage.Location = new System.Drawing.Point(4, 24);
            SelectionTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectionTabPage.Name = "SelectionTabPage";
            SelectionTabPage.Size = new System.Drawing.Size(240, 701);
            SelectionTabPage.TabIndex = 2;
            SelectionTabPage.Text = "Selection";
            SelectionTabPage.UseVisualStyleBackColor = true;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new System.Drawing.Point(7, 38);
            label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label25.Name = "label25";
            label25.Size = new System.Drawing.Size(41, 15);
            label25.TabIndex = 28;
            label25.Text = "Mode:";
            // 
            // SelectionModeComboBox
            // 
            SelectionModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            SelectionModeComboBox.FormattingEnabled = true;
            SelectionModeComboBox.Items.AddRange(new object[] { "Entity", "Entity Extension", "Archetype Extension", "Time Cycle Modifier", "Car Generator", "Grass", "Water Quad", "Water Calming Quad", "Water Wave Quad", "Collision", "Nav Mesh", "Path", "Train Track", "Lod Lights", "Mlo Instance", "Scenario", "Audio", "Occlusion" });
            SelectionModeComboBox.Location = new System.Drawing.Point(59, 35);
            SelectionModeComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectionModeComboBox.Name = "SelectionModeComboBox";
            SelectionModeComboBox.Size = new System.Drawing.Size(140, 23);
            SelectionModeComboBox.TabIndex = 23;
            SelectionModeComboBox.SelectedIndexChanged += SelectionModeComboBox_SelectedIndexChanged;
            SelectionModeComboBox.KeyPress += SelectionModeComboBox_KeyPress;
            // 
            // SelectionNameTextBox
            // 
            SelectionNameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SelectionNameTextBox.BackColor = System.Drawing.Color.White;
            SelectionNameTextBox.Location = new System.Drawing.Point(4, 76);
            SelectionNameTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectionNameTextBox.Name = "SelectionNameTextBox";
            SelectionNameTextBox.ReadOnly = true;
            SelectionNameTextBox.Size = new System.Drawing.Size(231, 23);
            SelectionNameTextBox.TabIndex = 26;
            SelectionNameTextBox.Text = "Nothing selected";
            // 
            // SelectionTabControl
            // 
            SelectionTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SelectionTabControl.Controls.Add(SelectionEntityTabPage);
            SelectionTabControl.Controls.Add(SelectionArchetypeTabPage);
            SelectionTabControl.Controls.Add(SelectionDrawableTabPage);
            SelectionTabControl.Controls.Add(SelectionExtensionTabPage);
            SelectionTabControl.Location = new System.Drawing.Point(0, 110);
            SelectionTabControl.Margin = new System.Windows.Forms.Padding(0);
            SelectionTabControl.Name = "SelectionTabControl";
            SelectionTabControl.SelectedIndex = 0;
            SelectionTabControl.Size = new System.Drawing.Size(239, 590);
            SelectionTabControl.TabIndex = 28;
            // 
            // SelectionEntityTabPage
            // 
            SelectionEntityTabPage.Controls.Add(SelEntityPropertyGrid);
            SelectionEntityTabPage.Location = new System.Drawing.Point(4, 24);
            SelectionEntityTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectionEntityTabPage.Name = "SelectionEntityTabPage";
            SelectionEntityTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectionEntityTabPage.Size = new System.Drawing.Size(231, 562);
            SelectionEntityTabPage.TabIndex = 0;
            SelectionEntityTabPage.Text = "Entity";
            SelectionEntityTabPage.UseVisualStyleBackColor = true;
            // 
            // SelEntityPropertyGrid
            // 
            SelEntityPropertyGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SelEntityPropertyGrid.HelpVisible = false;
            SelEntityPropertyGrid.Location = new System.Drawing.Point(0, 7);
            SelEntityPropertyGrid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelEntityPropertyGrid.Name = "SelEntityPropertyGrid";
            SelEntityPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            SelEntityPropertyGrid.ReadOnly = true;
            SelEntityPropertyGrid.Size = new System.Drawing.Size(230, 547);
            SelEntityPropertyGrid.TabIndex = 35;
            SelEntityPropertyGrid.ToolbarVisible = false;
            // 
            // SelectionArchetypeTabPage
            // 
            SelectionArchetypeTabPage.Controls.Add(SelArchetypePropertyGrid);
            SelectionArchetypeTabPage.Location = new System.Drawing.Point(4, 24);
            SelectionArchetypeTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectionArchetypeTabPage.Name = "SelectionArchetypeTabPage";
            SelectionArchetypeTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectionArchetypeTabPage.Size = new System.Drawing.Size(231, 562);
            SelectionArchetypeTabPage.TabIndex = 1;
            SelectionArchetypeTabPage.Text = "Archetype";
            SelectionArchetypeTabPage.UseVisualStyleBackColor = true;
            // 
            // SelArchetypePropertyGrid
            // 
            SelArchetypePropertyGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SelArchetypePropertyGrid.HelpVisible = false;
            SelArchetypePropertyGrid.Location = new System.Drawing.Point(0, 7);
            SelArchetypePropertyGrid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelArchetypePropertyGrid.Name = "SelArchetypePropertyGrid";
            SelArchetypePropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            SelArchetypePropertyGrid.ReadOnly = true;
            SelArchetypePropertyGrid.Size = new System.Drawing.Size(230, 547);
            SelArchetypePropertyGrid.TabIndex = 36;
            SelArchetypePropertyGrid.ToolbarVisible = false;
            // 
            // SelectionDrawableTabPage
            // 
            SelectionDrawableTabPage.Controls.Add(tabControl3);
            SelectionDrawableTabPage.Location = new System.Drawing.Point(4, 24);
            SelectionDrawableTabPage.Margin = new System.Windows.Forms.Padding(0);
            SelectionDrawableTabPage.Name = "SelectionDrawableTabPage";
            SelectionDrawableTabPage.Size = new System.Drawing.Size(231, 562);
            SelectionDrawableTabPage.TabIndex = 2;
            SelectionDrawableTabPage.Text = "Drawable";
            SelectionDrawableTabPage.UseVisualStyleBackColor = true;
            // 
            // tabControl3
            // 
            tabControl3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl3.Controls.Add(tabPage11);
            tabControl3.Controls.Add(tabPage12);
            tabControl3.Controls.Add(tabPage13);
            tabControl3.Location = new System.Drawing.Point(-5, 8);
            tabControl3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabControl3.Name = "tabControl3";
            tabControl3.SelectedIndex = 0;
            tabControl3.Size = new System.Drawing.Size(239, 549);
            tabControl3.TabIndex = 28;
            // 
            // tabPage11
            // 
            tabPage11.Controls.Add(SelDrawablePropertyGrid);
            tabPage11.Location = new System.Drawing.Point(4, 24);
            tabPage11.Margin = new System.Windows.Forms.Padding(0);
            tabPage11.Name = "tabPage11";
            tabPage11.Size = new System.Drawing.Size(231, 521);
            tabPage11.TabIndex = 0;
            tabPage11.Text = "Info";
            tabPage11.UseVisualStyleBackColor = true;
            // 
            // SelDrawablePropertyGrid
            // 
            SelDrawablePropertyGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SelDrawablePropertyGrid.HelpVisible = false;
            SelDrawablePropertyGrid.Location = new System.Drawing.Point(0, 0);
            SelDrawablePropertyGrid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelDrawablePropertyGrid.Name = "SelDrawablePropertyGrid";
            SelDrawablePropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            SelDrawablePropertyGrid.ReadOnly = true;
            SelDrawablePropertyGrid.Size = new System.Drawing.Size(230, 517);
            SelDrawablePropertyGrid.TabIndex = 37;
            SelDrawablePropertyGrid.ToolbarVisible = false;
            // 
            // tabPage12
            // 
            tabPage12.Controls.Add(SelDrawableModelsTreeView);
            tabPage12.Location = new System.Drawing.Point(4, 24);
            tabPage12.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabPage12.Name = "tabPage12";
            tabPage12.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabPage12.Size = new System.Drawing.Size(231, 521);
            tabPage12.TabIndex = 1;
            tabPage12.Text = "Models";
            tabPage12.UseVisualStyleBackColor = true;
            // 
            // SelDrawableModelsTreeView
            // 
            SelDrawableModelsTreeView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SelDrawableModelsTreeView.CheckBoxes = true;
            SelDrawableModelsTreeView.Location = new System.Drawing.Point(0, 0);
            SelDrawableModelsTreeView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelDrawableModelsTreeView.Name = "SelDrawableModelsTreeView";
            SelDrawableModelsTreeView.ShowRootLines = false;
            SelDrawableModelsTreeView.Size = new System.Drawing.Size(229, 519);
            SelDrawableModelsTreeView.TabIndex = 39;
            SelDrawableModelsTreeView.AfterCheck += SelDrawableModelsTreeView_AfterCheck;
            SelDrawableModelsTreeView.NodeMouseDoubleClick += SelDrawableModelsTreeView_NodeMouseDoubleClick;
            SelDrawableModelsTreeView.KeyPress += SelDrawableModelsTreeView_KeyPress;
            // 
            // tabPage13
            // 
            tabPage13.Controls.Add(SelDrawableTexturesTreeView);
            tabPage13.Location = new System.Drawing.Point(4, 24);
            tabPage13.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabPage13.Name = "tabPage13";
            tabPage13.Size = new System.Drawing.Size(231, 521);
            tabPage13.TabIndex = 2;
            tabPage13.Text = "Textures";
            tabPage13.UseVisualStyleBackColor = true;
            // 
            // SelDrawableTexturesTreeView
            // 
            SelDrawableTexturesTreeView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SelDrawableTexturesTreeView.Location = new System.Drawing.Point(0, 0);
            SelDrawableTexturesTreeView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelDrawableTexturesTreeView.Name = "SelDrawableTexturesTreeView";
            SelDrawableTexturesTreeView.ShowRootLines = false;
            SelDrawableTexturesTreeView.Size = new System.Drawing.Size(229, 519);
            SelDrawableTexturesTreeView.TabIndex = 40;
            // 
            // SelectionExtensionTabPage
            // 
            SelectionExtensionTabPage.Controls.Add(SelExtensionPropertyGrid);
            SelectionExtensionTabPage.Location = new System.Drawing.Point(4, 24);
            SelectionExtensionTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectionExtensionTabPage.Name = "SelectionExtensionTabPage";
            SelectionExtensionTabPage.Size = new System.Drawing.Size(231, 562);
            SelectionExtensionTabPage.TabIndex = 3;
            SelectionExtensionTabPage.Text = "Ext";
            SelectionExtensionTabPage.UseVisualStyleBackColor = true;
            // 
            // SelExtensionPropertyGrid
            // 
            SelExtensionPropertyGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SelExtensionPropertyGrid.HelpVisible = false;
            SelExtensionPropertyGrid.Location = new System.Drawing.Point(0, 7);
            SelExtensionPropertyGrid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelExtensionPropertyGrid.Name = "SelExtensionPropertyGrid";
            SelExtensionPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            SelExtensionPropertyGrid.ReadOnly = true;
            SelExtensionPropertyGrid.Size = new System.Drawing.Size(230, 547);
            SelExtensionPropertyGrid.TabIndex = 36;
            SelExtensionPropertyGrid.ToolbarVisible = false;
            // 
            // MouseSelectCheckBox
            // 
            MouseSelectCheckBox.AutoSize = true;
            MouseSelectCheckBox.Location = new System.Drawing.Point(9, 8);
            MouseSelectCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MouseSelectCheckBox.Name = "MouseSelectCheckBox";
            MouseSelectCheckBox.Size = new System.Drawing.Size(158, 19);
            MouseSelectCheckBox.TabIndex = 22;
            MouseSelectCheckBox.Text = "Mouse select (right click)";
            MouseSelectCheckBox.UseVisualStyleBackColor = true;
            MouseSelectCheckBox.CheckedChanged += MouseSelectCheckBox_CheckedChanged;
            // 
            // OptionsTabPage
            // 
            OptionsTabPage.Controls.Add(OptionsTabControl);
            OptionsTabPage.Controls.Add(StatusBarCheckBox);
            OptionsTabPage.Controls.Add(QuitButton);
            OptionsTabPage.Controls.Add(ReloadSettingsButton);
            OptionsTabPage.Controls.Add(SaveSettingsButton);
            OptionsTabPage.Controls.Add(ReloadShadersButton);
            OptionsTabPage.Controls.Add(ErrorConsoleCheckBox);
            OptionsTabPage.Location = new System.Drawing.Point(4, 24);
            OptionsTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OptionsTabPage.Name = "OptionsTabPage";
            OptionsTabPage.Size = new System.Drawing.Size(240, 701);
            OptionsTabPage.TabIndex = 3;
            OptionsTabPage.Text = "Options";
            OptionsTabPage.UseVisualStyleBackColor = true;
            // 
            // OptionsTabControl
            // 
            OptionsTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            OptionsTabControl.Controls.Add(OptionsGeneralTabPage);
            OptionsTabControl.Controls.Add(OptionsRenderTabPage);
            OptionsTabControl.Controls.Add(OptionsHelpersTabPage);
            OptionsTabControl.Controls.Add(OptionsLightingTabPage);
            OptionsTabControl.Location = new System.Drawing.Point(0, 3);
            OptionsTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OptionsTabControl.Name = "OptionsTabControl";
            OptionsTabControl.SelectedIndex = 0;
            OptionsTabControl.Size = new System.Drawing.Size(243, 586);
            OptionsTabControl.TabIndex = 50;
            // 
            // OptionsGeneralTabPage
            // 
            OptionsGeneralTabPage.Controls.Add(CarGeneratorsCheckBox);
            OptionsGeneralTabPage.Controls.Add(RenderEntitiesCheckBox);
            OptionsGeneralTabPage.Controls.Add(AdvancedSettingsButton);
            OptionsGeneralTabPage.Controls.Add(ControlSettingsButton);
            OptionsGeneralTabPage.Controls.Add(MapViewDetailLabel);
            OptionsGeneralTabPage.Controls.Add(label28);
            OptionsGeneralTabPage.Controls.Add(MapViewDetailTrackBar);
            OptionsGeneralTabPage.Controls.Add(CameraModeComboBox);
            OptionsGeneralTabPage.Controls.Add(label24);
            OptionsGeneralTabPage.Controls.Add(WaterQuadsCheckBox);
            OptionsGeneralTabPage.Controls.Add(FieldOfViewLabel);
            OptionsGeneralTabPage.Controls.Add(label22);
            OptionsGeneralTabPage.Controls.Add(TimedEntitiesAlwaysOnCheckBox);
            OptionsGeneralTabPage.Controls.Add(GrassCheckBox);
            OptionsGeneralTabPage.Controls.Add(InteriorsCheckBox);
            OptionsGeneralTabPage.Controls.Add(CollisionMeshLayerDrawableCheckBox);
            OptionsGeneralTabPage.Controls.Add(CollisionMeshLayer2CheckBox);
            OptionsGeneralTabPage.Controls.Add(CollisionMeshLayer1CheckBox);
            OptionsGeneralTabPage.Controls.Add(label13);
            OptionsGeneralTabPage.Controls.Add(CollisionMeshLayer0CheckBox);
            OptionsGeneralTabPage.Controls.Add(label12);
            OptionsGeneralTabPage.Controls.Add(CollisionMeshRangeTrackBar);
            OptionsGeneralTabPage.Controls.Add(CollisionMeshesCheckBox);
            OptionsGeneralTabPage.Controls.Add(FullScreenCheckBox);
            OptionsGeneralTabPage.Controls.Add(TimedEntitiesCheckBox);
            OptionsGeneralTabPage.Controls.Add(FieldOfViewTrackBar);
            OptionsGeneralTabPage.Location = new System.Drawing.Point(4, 24);
            OptionsGeneralTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OptionsGeneralTabPage.Name = "OptionsGeneralTabPage";
            OptionsGeneralTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OptionsGeneralTabPage.Size = new System.Drawing.Size(235, 558);
            OptionsGeneralTabPage.TabIndex = 0;
            OptionsGeneralTabPage.Text = "General";
            OptionsGeneralTabPage.UseVisualStyleBackColor = true;
            // 
            // CarGeneratorsCheckBox
            // 
            CarGeneratorsCheckBox.AutoSize = true;
            CarGeneratorsCheckBox.Location = new System.Drawing.Point(12, 83);
            CarGeneratorsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CarGeneratorsCheckBox.Name = "CarGeneratorsCheckBox";
            CarGeneratorsCheckBox.Size = new System.Drawing.Size(133, 19);
            CarGeneratorsCheckBox.TabIndex = 31;
            CarGeneratorsCheckBox.Text = "Show car generators";
            CarGeneratorsCheckBox.UseVisualStyleBackColor = true;
            CarGeneratorsCheckBox.CheckedChanged += CarGeneratorsCheckBox_CheckedChanged;
            // 
            // RenderEntitiesCheckBox
            // 
            RenderEntitiesCheckBox.AutoSize = true;
            RenderEntitiesCheckBox.Checked = true;
            RenderEntitiesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            RenderEntitiesCheckBox.Location = new System.Drawing.Point(12, 35);
            RenderEntitiesCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            RenderEntitiesCheckBox.Name = "RenderEntitiesCheckBox";
            RenderEntitiesCheckBox.Size = new System.Drawing.Size(96, 19);
            RenderEntitiesCheckBox.TabIndex = 29;
            RenderEntitiesCheckBox.Text = "Show entities";
            RenderEntitiesCheckBox.UseVisualStyleBackColor = true;
            RenderEntitiesCheckBox.CheckedChanged += RenderEntitiesCheckBox_CheckedChanged;
            // 
            // AdvancedSettingsButton
            // 
            AdvancedSettingsButton.Location = new System.Drawing.Point(118, 526);
            AdvancedSettingsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AdvancedSettingsButton.Name = "AdvancedSettingsButton";
            AdvancedSettingsButton.Size = new System.Drawing.Size(108, 27);
            AdvancedSettingsButton.TabIndex = 46;
            AdvancedSettingsButton.Text = "Advanced...";
            AdvancedSettingsButton.UseVisualStyleBackColor = true;
            AdvancedSettingsButton.Click += AdvancedSettingsButton_Click;
            // 
            // ControlSettingsButton
            // 
            ControlSettingsButton.Location = new System.Drawing.Point(2, 526);
            ControlSettingsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ControlSettingsButton.Name = "ControlSettingsButton";
            ControlSettingsButton.Size = new System.Drawing.Size(108, 27);
            ControlSettingsButton.TabIndex = 45;
            ControlSettingsButton.Text = "Controls...";
            ControlSettingsButton.UseVisualStyleBackColor = true;
            ControlSettingsButton.Click += ControlSettingsButton_Click;
            // 
            // MapViewDetailLabel
            // 
            MapViewDetailLabel.AutoSize = true;
            MapViewDetailLabel.Location = new System.Drawing.Point(110, 451);
            MapViewDetailLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            MapViewDetailLabel.Name = "MapViewDetailLabel";
            MapViewDetailLabel.Size = new System.Drawing.Size(22, 15);
            MapViewDetailLabel.TabIndex = 66;
            MapViewDetailLabel.Text = "1.0";
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Location = new System.Drawing.Point(5, 451);
            label28.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label28.Name = "label28";
            label28.Size = new System.Drawing.Size(93, 15);
            label28.TabIndex = 65;
            label28.Text = "Map view detail:";
            // 
            // MapViewDetailTrackBar
            // 
            MapViewDetailTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            MapViewDetailTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            MapViewDetailTrackBar.Enabled = false;
            MapViewDetailTrackBar.LargeChange = 1;
            MapViewDetailTrackBar.Location = new System.Drawing.Point(7, 470);
            MapViewDetailTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MapViewDetailTrackBar.Maximum = 30;
            MapViewDetailTrackBar.Minimum = 2;
            MapViewDetailTrackBar.Name = "MapViewDetailTrackBar";
            MapViewDetailTrackBar.Size = new System.Drawing.Size(219, 45);
            MapViewDetailTrackBar.TabIndex = 44;
            MapViewDetailTrackBar.TickFrequency = 2;
            MapViewDetailTrackBar.Value = 10;
            MapViewDetailTrackBar.Scroll += MapViewDetailTrackBar_Scroll;
            // 
            // CameraModeComboBox
            // 
            CameraModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            CameraModeComboBox.FormattingEnabled = true;
            CameraModeComboBox.Items.AddRange(new object[] { "Perspective", "Orthographic", "2D Map" });
            CameraModeComboBox.Location = new System.Drawing.Point(96, 352);
            CameraModeComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CameraModeComboBox.Name = "CameraModeComboBox";
            CameraModeComboBox.Size = new System.Drawing.Size(130, 23);
            CameraModeComboBox.TabIndex = 42;
            CameraModeComboBox.SelectedIndexChanged += CameraModeComboBox_SelectedIndexChanged;
            CameraModeComboBox.KeyPress += CameraModeComboBox_KeyPress;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new System.Drawing.Point(5, 355);
            label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label24.Name = "label24";
            label24.Size = new System.Drawing.Size(85, 15);
            label24.TabIndex = 63;
            label24.Text = "Camera mode:";
            // 
            // WaterQuadsCheckBox
            // 
            WaterQuadsCheckBox.AutoSize = true;
            WaterQuadsCheckBox.Checked = true;
            WaterQuadsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            WaterQuadsCheckBox.Location = new System.Drawing.Point(12, 156);
            WaterQuadsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WaterQuadsCheckBox.Name = "WaterQuadsCheckBox";
            WaterQuadsCheckBox.Size = new System.Drawing.Size(122, 19);
            WaterQuadsCheckBox.TabIndex = 35;
            WaterQuadsCheckBox.Text = "Show water quads";
            WaterQuadsCheckBox.UseVisualStyleBackColor = true;
            WaterQuadsCheckBox.CheckedChanged += WaterQuadsCheckBox_CheckedChanged;
            // 
            // FieldOfViewLabel
            // 
            FieldOfViewLabel.AutoSize = true;
            FieldOfViewLabel.Location = new System.Drawing.Point(92, 387);
            FieldOfViewLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            FieldOfViewLabel.Name = "FieldOfViewLabel";
            FieldOfViewLabel.Size = new System.Drawing.Size(22, 15);
            FieldOfViewLabel.TabIndex = 59;
            FieldOfViewLabel.Text = "1.0";
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new System.Drawing.Point(5, 387);
            label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label22.Name = "label22";
            label22.Size = new System.Drawing.Size(76, 15);
            label22.TabIndex = 58;
            label22.Text = "Field of view:";
            // 
            // TimedEntitiesAlwaysOnCheckBox
            // 
            TimedEntitiesAlwaysOnCheckBox.AutoSize = true;
            TimedEntitiesAlwaysOnCheckBox.Location = new System.Drawing.Point(153, 107);
            TimedEntitiesAlwaysOnCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TimedEntitiesAlwaysOnCheckBox.Name = "TimedEntitiesAlwaysOnCheckBox";
            TimedEntitiesAlwaysOnCheckBox.Size = new System.Drawing.Size(61, 19);
            TimedEntitiesAlwaysOnCheckBox.TabIndex = 33;
            TimedEntitiesAlwaysOnCheckBox.Text = "always";
            TimedEntitiesAlwaysOnCheckBox.UseVisualStyleBackColor = true;
            TimedEntitiesAlwaysOnCheckBox.CheckedChanged += TimedEntitiesAlwaysOnCheckBox_CheckedChanged;
            // 
            // GrassCheckBox
            // 
            GrassCheckBox.AutoSize = true;
            GrassCheckBox.Checked = true;
            GrassCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            GrassCheckBox.Location = new System.Drawing.Point(12, 59);
            GrassCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            GrassCheckBox.Name = "GrassCheckBox";
            GrassCheckBox.Size = new System.Drawing.Size(85, 19);
            GrassCheckBox.TabIndex = 30;
            GrassCheckBox.Text = "Show grass";
            GrassCheckBox.UseVisualStyleBackColor = true;
            GrassCheckBox.CheckedChanged += GrassCheckBox_CheckedChanged;
            // 
            // InteriorsCheckBox
            // 
            InteriorsCheckBox.AutoSize = true;
            InteriorsCheckBox.Checked = true;
            InteriorsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            InteriorsCheckBox.Location = new System.Drawing.Point(12, 132);
            InteriorsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            InteriorsCheckBox.Name = "InteriorsCheckBox";
            InteriorsCheckBox.Size = new System.Drawing.Size(101, 19);
            InteriorsCheckBox.TabIndex = 34;
            InteriorsCheckBox.Text = "Show interiors";
            InteriorsCheckBox.UseVisualStyleBackColor = true;
            InteriorsCheckBox.CheckedChanged += InteriorsCheckBox_CheckedChanged;
            // 
            // CollisionMeshLayerDrawableCheckBox
            // 
            CollisionMeshLayerDrawableCheckBox.AutoSize = true;
            CollisionMeshLayerDrawableCheckBox.Checked = true;
            CollisionMeshLayerDrawableCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            CollisionMeshLayerDrawableCheckBox.Location = new System.Drawing.Point(138, 302);
            CollisionMeshLayerDrawableCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CollisionMeshLayerDrawableCheckBox.Name = "CollisionMeshLayerDrawableCheckBox";
            CollisionMeshLayerDrawableCheckBox.Size = new System.Drawing.Size(75, 19);
            CollisionMeshLayerDrawableCheckBox.TabIndex = 41;
            CollisionMeshLayerDrawableCheckBox.Text = "Drawable";
            CollisionMeshLayerDrawableCheckBox.UseVisualStyleBackColor = true;
            CollisionMeshLayerDrawableCheckBox.CheckedChanged += CollisionMeshLayerDrawableCheckBox_CheckedChanged;
            // 
            // CollisionMeshLayer2CheckBox
            // 
            CollisionMeshLayer2CheckBox.AutoSize = true;
            CollisionMeshLayer2CheckBox.Checked = true;
            CollisionMeshLayer2CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            CollisionMeshLayer2CheckBox.Location = new System.Drawing.Point(96, 302);
            CollisionMeshLayer2CheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CollisionMeshLayer2CheckBox.Name = "CollisionMeshLayer2CheckBox";
            CollisionMeshLayer2CheckBox.Size = new System.Drawing.Size(32, 19);
            CollisionMeshLayer2CheckBox.TabIndex = 40;
            CollisionMeshLayer2CheckBox.Text = "2";
            CollisionMeshLayer2CheckBox.UseVisualStyleBackColor = true;
            CollisionMeshLayer2CheckBox.CheckedChanged += CollisionMeshLayer2CheckBox_CheckedChanged;
            // 
            // CollisionMeshLayer1CheckBox
            // 
            CollisionMeshLayer1CheckBox.AutoSize = true;
            CollisionMeshLayer1CheckBox.Checked = true;
            CollisionMeshLayer1CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            CollisionMeshLayer1CheckBox.Location = new System.Drawing.Point(54, 302);
            CollisionMeshLayer1CheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CollisionMeshLayer1CheckBox.Name = "CollisionMeshLayer1CheckBox";
            CollisionMeshLayer1CheckBox.Size = new System.Drawing.Size(32, 19);
            CollisionMeshLayer1CheckBox.TabIndex = 39;
            CollisionMeshLayer1CheckBox.Text = "1";
            CollisionMeshLayer1CheckBox.UseVisualStyleBackColor = true;
            CollisionMeshLayer1CheckBox.CheckedChanged += CollisionMeshLayer1CheckBox_CheckedChanged;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(5, 282);
            label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(121, 15);
            label13.TabIndex = 54;
            label13.Text = "Collision mesh layers:";
            // 
            // CollisionMeshLayer0CheckBox
            // 
            CollisionMeshLayer0CheckBox.AutoSize = true;
            CollisionMeshLayer0CheckBox.Checked = true;
            CollisionMeshLayer0CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            CollisionMeshLayer0CheckBox.Location = new System.Drawing.Point(12, 302);
            CollisionMeshLayer0CheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CollisionMeshLayer0CheckBox.Name = "CollisionMeshLayer0CheckBox";
            CollisionMeshLayer0CheckBox.Size = new System.Drawing.Size(32, 19);
            CollisionMeshLayer0CheckBox.TabIndex = 38;
            CollisionMeshLayer0CheckBox.Text = "0";
            CollisionMeshLayer0CheckBox.UseVisualStyleBackColor = true;
            CollisionMeshLayer0CheckBox.CheckedChanged += CollisionMeshLayer0CheckBox_CheckedChanged;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(5, 223);
            label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(145, 15);
            label12.TabIndex = 51;
            label12.Text = "Collision/nav mesh range:";
            // 
            // CollisionMeshRangeTrackBar
            // 
            CollisionMeshRangeTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            CollisionMeshRangeTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            CollisionMeshRangeTrackBar.LargeChange = 1;
            CollisionMeshRangeTrackBar.Location = new System.Drawing.Point(7, 241);
            CollisionMeshRangeTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CollisionMeshRangeTrackBar.Maximum = 15;
            CollisionMeshRangeTrackBar.Minimum = 1;
            CollisionMeshRangeTrackBar.Name = "CollisionMeshRangeTrackBar";
            CollisionMeshRangeTrackBar.Size = new System.Drawing.Size(219, 45);
            CollisionMeshRangeTrackBar.TabIndex = 37;
            CollisionMeshRangeTrackBar.Value = 6;
            CollisionMeshRangeTrackBar.Scroll += CollisionMeshRangeTrackBar_Scroll;
            // 
            // CollisionMeshesCheckBox
            // 
            CollisionMeshesCheckBox.AutoSize = true;
            CollisionMeshesCheckBox.Location = new System.Drawing.Point(12, 197);
            CollisionMeshesCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CollisionMeshesCheckBox.Name = "CollisionMeshesCheckBox";
            CollisionMeshesCheckBox.Size = new System.Drawing.Size(145, 19);
            CollisionMeshesCheckBox.TabIndex = 36;
            CollisionMeshesCheckBox.Text = "Show collision meshes";
            CollisionMeshesCheckBox.UseVisualStyleBackColor = true;
            CollisionMeshesCheckBox.CheckedChanged += CollisionMeshesCheckBox_CheckedChanged;
            // 
            // FullScreenCheckBox
            // 
            FullScreenCheckBox.AutoSize = true;
            FullScreenCheckBox.Location = new System.Drawing.Point(12, 10);
            FullScreenCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            FullScreenCheckBox.Name = "FullScreenCheckBox";
            FullScreenCheckBox.Size = new System.Drawing.Size(192, 19);
            FullScreenCheckBox.TabIndex = 28;
            FullScreenCheckBox.Text = "Full screen (borderless window)";
            FullScreenCheckBox.UseVisualStyleBackColor = true;
            FullScreenCheckBox.CheckedChanged += FullScreenCheckBox_CheckedChanged;
            // 
            // TimedEntitiesCheckBox
            // 
            TimedEntitiesCheckBox.AutoSize = true;
            TimedEntitiesCheckBox.Checked = true;
            TimedEntitiesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            TimedEntitiesCheckBox.Location = new System.Drawing.Point(12, 107);
            TimedEntitiesCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TimedEntitiesCheckBox.Name = "TimedEntitiesCheckBox";
            TimedEntitiesCheckBox.Size = new System.Drawing.Size(130, 19);
            TimedEntitiesCheckBox.TabIndex = 32;
            TimedEntitiesCheckBox.Text = "Show timed entities";
            TimedEntitiesCheckBox.UseVisualStyleBackColor = true;
            TimedEntitiesCheckBox.CheckedChanged += TimedEntitiesCheckBox_CheckedChanged;
            // 
            // FieldOfViewTrackBar
            // 
            FieldOfViewTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            FieldOfViewTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            FieldOfViewTrackBar.LargeChange = 1;
            FieldOfViewTrackBar.Location = new System.Drawing.Point(7, 405);
            FieldOfViewTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            FieldOfViewTrackBar.Maximum = 200;
            FieldOfViewTrackBar.Minimum = 10;
            FieldOfViewTrackBar.Name = "FieldOfViewTrackBar";
            FieldOfViewTrackBar.Size = new System.Drawing.Size(219, 45);
            FieldOfViewTrackBar.TabIndex = 43;
            FieldOfViewTrackBar.TickFrequency = 10;
            FieldOfViewTrackBar.Value = 100;
            FieldOfViewTrackBar.Scroll += FieldOfViewTrackBar_Scroll;
            // 
            // OptionsRenderTabPage
            // 
            OptionsRenderTabPage.Controls.Add(AntiAliasingValue);
            OptionsRenderTabPage.Controls.Add(label34);
            OptionsRenderTabPage.Controls.Add(AntiAliasingTrackBar);
            OptionsRenderTabPage.Controls.Add(FarClipUpDown);
            OptionsRenderTabPage.Controls.Add(label32);
            OptionsRenderTabPage.Controls.Add(NearClipUpDown);
            OptionsRenderTabPage.Controls.Add(label31);
            OptionsRenderTabPage.Controls.Add(HDTexturesCheckBox);
            OptionsRenderTabPage.Controls.Add(WireframeCheckBox);
            OptionsRenderTabPage.Controls.Add(RenderModeComboBox);
            OptionsRenderTabPage.Controls.Add(label11);
            OptionsRenderTabPage.Controls.Add(TextureSamplerComboBox);
            OptionsRenderTabPage.Controls.Add(TextureCoordsComboBox);
            OptionsRenderTabPage.Controls.Add(label10);
            OptionsRenderTabPage.Controls.Add(AnisotropicFilteringCheckBox);
            OptionsRenderTabPage.Controls.Add(ProxiesCheckBox);
            OptionsRenderTabPage.Controls.Add(WaitForChildrenCheckBox);
            OptionsRenderTabPage.Controls.Add(label14);
            OptionsRenderTabPage.Location = new System.Drawing.Point(4, 24);
            OptionsRenderTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OptionsRenderTabPage.Name = "OptionsRenderTabPage";
            OptionsRenderTabPage.Size = new System.Drawing.Size(235, 558);
            OptionsRenderTabPage.TabIndex = 3;
            OptionsRenderTabPage.Text = "Render";
            OptionsRenderTabPage.UseVisualStyleBackColor = true;
            // 
            // AntiAliasingValue
            // 
            AntiAliasingValue.AutoSize = true;
            AntiAliasingValue.Location = new System.Drawing.Point(172, 292);
            AntiAliasingValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            AntiAliasingValue.Name = "AntiAliasingValue";
            AntiAliasingValue.Size = new System.Drawing.Size(13, 15);
            AntiAliasingValue.TabIndex = 64;
            AntiAliasingValue.Text = "1";
            // 
            // AntiAliasingTrackBar
            // 
            AntiAliasingTrackBar.LargeChange = 1;
            AntiAliasingTrackBar.Location = new System.Drawing.Point(12, 310);
            AntiAliasingTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AntiAliasingTrackBar.Maximum = 8;
            AntiAliasingTrackBar.Minimum = 1;
            AntiAliasingTrackBar.Name = "AntiAliasingTrackBar";
            AntiAliasingTrackBar.Size = new System.Drawing.Size(215, 45);
            AntiAliasingTrackBar.TabIndex = 62;
            AntiAliasingTrackBar.Value = 1;
            AntiAliasingTrackBar.ValueChanged += AntiAliasingTrackBar_ValueChanged;
            // 
            // FarClipUpDown
            // 
            FarClipUpDown.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            FarClipUpDown.Location = new System.Drawing.Point(93, 399);
            FarClipUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            FarClipUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            FarClipUpDown.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            FarClipUpDown.Name = "FarClipUpDown";
            FarClipUpDown.Size = new System.Drawing.Size(133, 23);
            FarClipUpDown.TabIndex = 61;
            FarClipUpDown.Value = new decimal(new int[] { 100000, 0, 0, 0 });
            FarClipUpDown.ValueChanged += FarClipUpDown_ValueChanged;
            // 
            // label32
            // 
            label32.AutoSize = true;
            label32.Location = new System.Drawing.Point(5, 402);
            label32.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label32.Name = "label32";
            label32.Size = new System.Drawing.Size(50, 15);
            label32.TabIndex = 60;
            label32.Text = "Far Clip:";
            // 
            // NearClipUpDown
            // 
            NearClipUpDown.DecimalPlaces = 3;
            NearClipUpDown.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            NearClipUpDown.Location = new System.Drawing.Point(93, 369);
            NearClipUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NearClipUpDown.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            NearClipUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 196608 });
            NearClipUpDown.Name = "NearClipUpDown";
            NearClipUpDown.Size = new System.Drawing.Size(133, 23);
            NearClipUpDown.TabIndex = 59;
            NearClipUpDown.Value = new decimal(new int[] { 1, 0, 0, 131072 });
            NearClipUpDown.ValueChanged += NearClipUpDown_ValueChanged;
            // 
            // label31
            // 
            label31.AutoSize = true;
            label31.Location = new System.Drawing.Point(5, 372);
            label31.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label31.Name = "label31";
            label31.Size = new System.Drawing.Size(59, 15);
            label31.TabIndex = 58;
            label31.Text = "Near Clip:";
            // 
            // HDTexturesCheckBox
            // 
            HDTexturesCheckBox.AutoSize = true;
            HDTexturesCheckBox.Checked = true;
            HDTexturesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            HDTexturesCheckBox.Location = new System.Drawing.Point(12, 267);
            HDTexturesCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            HDTexturesCheckBox.Name = "HDTexturesCheckBox";
            HDTexturesCheckBox.Size = new System.Drawing.Size(88, 19);
            HDTexturesCheckBox.TabIndex = 57;
            HDTexturesCheckBox.Text = "HD textures";
            HDTexturesCheckBox.UseVisualStyleBackColor = true;
            HDTexturesCheckBox.CheckedChanged += HDTexturesCheckBox_CheckedChanged;
            // 
            // WireframeCheckBox
            // 
            WireframeCheckBox.AutoSize = true;
            WireframeCheckBox.Location = new System.Drawing.Point(12, 133);
            WireframeCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WireframeCheckBox.Name = "WireframeCheckBox";
            WireframeCheckBox.Size = new System.Drawing.Size(81, 19);
            WireframeCheckBox.TabIndex = 49;
            WireframeCheckBox.Text = "Wireframe";
            WireframeCheckBox.UseVisualStyleBackColor = true;
            WireframeCheckBox.CheckedChanged += WireframeCheckBox_CheckedChanged;
            // 
            // RenderModeComboBox
            // 
            RenderModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            RenderModeComboBox.FormattingEnabled = true;
            RenderModeComboBox.Items.AddRange(new object[] { "Default", "Single texture", "Vertex normals", "Vertex tangents", "Vertex colour 1", "Vertex colour 2", "Texture coord 1", "Texture coord 2", "Texture coord 3" });
            RenderModeComboBox.Location = new System.Drawing.Point(93, 18);
            RenderModeComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            RenderModeComboBox.Name = "RenderModeComboBox";
            RenderModeComboBox.Size = new System.Drawing.Size(132, 23);
            RenderModeComboBox.TabIndex = 46;
            RenderModeComboBox.SelectedIndexChanged += RenderModeComboBox_SelectedIndexChanged;
            RenderModeComboBox.KeyPress += RenderModeComboBox_KeyPress;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(5, 53);
            label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(72, 15);
            label11.TabIndex = 50;
            label11.Text = "Tex sampler:";
            // 
            // TextureSamplerComboBox
            // 
            TextureSamplerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            TextureSamplerComboBox.Enabled = false;
            TextureSamplerComboBox.FormattingEnabled = true;
            TextureSamplerComboBox.Location = new System.Drawing.Point(93, 50);
            TextureSamplerComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TextureSamplerComboBox.Name = "TextureSamplerComboBox";
            TextureSamplerComboBox.Size = new System.Drawing.Size(132, 23);
            TextureSamplerComboBox.TabIndex = 47;
            TextureSamplerComboBox.SelectedIndexChanged += TextureSamplerComboBox_SelectedIndexChanged;
            TextureSamplerComboBox.KeyPress += TextureSamplerComboBox_KeyPress;
            // 
            // TextureCoordsComboBox
            // 
            TextureCoordsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            TextureCoordsComboBox.Enabled = false;
            TextureCoordsComboBox.FormattingEnabled = true;
            TextureCoordsComboBox.Items.AddRange(new object[] { "Texture coord 1", "Texture coord 2", "Texture coord 3" });
            TextureCoordsComboBox.Location = new System.Drawing.Point(93, 81);
            TextureCoordsComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TextureCoordsComboBox.Name = "TextureCoordsComboBox";
            TextureCoordsComboBox.Size = new System.Drawing.Size(132, 23);
            TextureCoordsComboBox.TabIndex = 48;
            TextureCoordsComboBox.SelectedIndexChanged += TextureCoordsComboBox_SelectedIndexChanged;
            TextureCoordsComboBox.KeyPress += TextureCoordsComboBox_KeyPress;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(5, 22);
            label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(81, 15);
            label10.TabIndex = 48;
            label10.Text = "Render mode:";
            // 
            // AnisotropicFilteringCheckBox
            // 
            AnisotropicFilteringCheckBox.AutoSize = true;
            AnisotropicFilteringCheckBox.Checked = true;
            AnisotropicFilteringCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            AnisotropicFilteringCheckBox.Location = new System.Drawing.Point(12, 159);
            AnisotropicFilteringCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AnisotropicFilteringCheckBox.Name = "AnisotropicFilteringCheckBox";
            AnisotropicFilteringCheckBox.Size = new System.Drawing.Size(131, 19);
            AnisotropicFilteringCheckBox.TabIndex = 50;
            AnisotropicFilteringCheckBox.Text = "Anisotropic filtering";
            AnisotropicFilteringCheckBox.UseVisualStyleBackColor = true;
            AnisotropicFilteringCheckBox.CheckedChanged += AnisotropicFilteringCheckBox_CheckedChanged;
            // 
            // ProxiesCheckBox
            // 
            ProxiesCheckBox.AutoSize = true;
            ProxiesCheckBox.Location = new System.Drawing.Point(12, 230);
            ProxiesCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ProxiesCheckBox.Name = "ProxiesCheckBox";
            ProxiesCheckBox.Size = new System.Drawing.Size(96, 19);
            ProxiesCheckBox.TabIndex = 52;
            ProxiesCheckBox.Text = "Show proxies";
            ProxiesCheckBox.UseVisualStyleBackColor = true;
            ProxiesCheckBox.CheckedChanged += ProxiesCheckBox_CheckedChanged;
            // 
            // WaitForChildrenCheckBox
            // 
            WaitForChildrenCheckBox.AutoSize = true;
            WaitForChildrenCheckBox.Checked = true;
            WaitForChildrenCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            WaitForChildrenCheckBox.Location = new System.Drawing.Point(12, 186);
            WaitForChildrenCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WaitForChildrenCheckBox.Name = "WaitForChildrenCheckBox";
            WaitForChildrenCheckBox.Size = new System.Drawing.Size(154, 19);
            WaitForChildrenCheckBox.TabIndex = 51;
            WaitForChildrenCheckBox.Text = "Wait for children to load";
            WaitForChildrenCheckBox.UseVisualStyleBackColor = true;
            WaitForChildrenCheckBox.CheckedChanged += WaitForChildrenCheckBox_CheckedChanged;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new System.Drawing.Point(5, 84);
            label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(66, 15);
            label14.TabIndex = 56;
            label14.Text = "Tex coords:";
            // 
            // OptionsHelpersTabPage
            // 
            OptionsHelpersTabPage.Controls.Add(SnapAngleUpDown);
            OptionsHelpersTabPage.Controls.Add(label33);
            OptionsHelpersTabPage.Controls.Add(SnapGridSizeUpDown);
            OptionsHelpersTabPage.Controls.Add(label26);
            OptionsHelpersTabPage.Controls.Add(SkeletonsCheckBox);
            OptionsHelpersTabPage.Controls.Add(AudioOuterBoundsCheckBox);
            OptionsHelpersTabPage.Controls.Add(PopZonesCheckBox);
            OptionsHelpersTabPage.Controls.Add(NavMeshesCheckBox);
            OptionsHelpersTabPage.Controls.Add(TrainPathsCheckBox);
            OptionsHelpersTabPage.Controls.Add(PathsDepthClipCheckBox);
            OptionsHelpersTabPage.Controls.Add(PathBoundsCheckBox);
            OptionsHelpersTabPage.Controls.Add(SelectionWidgetCheckBox);
            OptionsHelpersTabPage.Controls.Add(MarkerStyleComboBox);
            OptionsHelpersTabPage.Controls.Add(label4);
            OptionsHelpersTabPage.Controls.Add(LocatorStyleComboBox);
            OptionsHelpersTabPage.Controls.Add(label5);
            OptionsHelpersTabPage.Controls.Add(MarkerDepthClipCheckBox);
            OptionsHelpersTabPage.Controls.Add(label9);
            OptionsHelpersTabPage.Controls.Add(PathsCheckBox);
            OptionsHelpersTabPage.Controls.Add(SelectionBoundsCheckBox);
            OptionsHelpersTabPage.Controls.Add(BoundsDepthClipCheckBox);
            OptionsHelpersTabPage.Controls.Add(BoundsRangeTrackBar);
            OptionsHelpersTabPage.Controls.Add(BoundsStyleComboBox);
            OptionsHelpersTabPage.Controls.Add(label8);
            OptionsHelpersTabPage.Location = new System.Drawing.Point(4, 24);
            OptionsHelpersTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OptionsHelpersTabPage.Name = "OptionsHelpersTabPage";
            OptionsHelpersTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OptionsHelpersTabPage.Size = new System.Drawing.Size(235, 558);
            OptionsHelpersTabPage.TabIndex = 1;
            OptionsHelpersTabPage.Text = "Helpers";
            OptionsHelpersTabPage.UseVisualStyleBackColor = true;
            // 
            // SnapAngleUpDown
            // 
            SnapAngleUpDown.DecimalPlaces = 1;
            SnapAngleUpDown.Location = new System.Drawing.Point(114, 322);
            SnapAngleUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SnapAngleUpDown.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
            SnapAngleUpDown.Name = "SnapAngleUpDown";
            SnapAngleUpDown.Size = new System.Drawing.Size(112, 23);
            SnapAngleUpDown.TabIndex = 32;
            SnapAngleUpDown.Value = new decimal(new int[] { 50, 0, 0, 65536 });
            SnapAngleUpDown.ValueChanged += SnapAngleUpDown_ValueChanged;
            // 
            // label33
            // 
            label33.AutoSize = true;
            label33.Location = new System.Drawing.Point(5, 324);
            label33.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label33.Name = "label33";
            label33.Size = new System.Drawing.Size(99, 15);
            label33.TabIndex = 31;
            label33.Text = "Snap angle (deg):";
            // 
            // SnapGridSizeUpDown
            // 
            SnapGridSizeUpDown.DecimalPlaces = 2;
            SnapGridSizeUpDown.Location = new System.Drawing.Point(114, 292);
            SnapGridSizeUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SnapGridSizeUpDown.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            SnapGridSizeUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            SnapGridSizeUpDown.Name = "SnapGridSizeUpDown";
            SnapGridSizeUpDown.Size = new System.Drawing.Size(112, 23);
            SnapGridSizeUpDown.TabIndex = 30;
            SnapGridSizeUpDown.Value = new decimal(new int[] { 100, 0, 0, 131072 });
            SnapGridSizeUpDown.ValueChanged += SnapGridSizeUpDown_ValueChanged;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new System.Drawing.Point(5, 294);
            label26.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label26.Name = "label26";
            label26.Size = new System.Drawing.Size(82, 15);
            label26.TabIndex = 29;
            label26.Text = "Snap grid size:";
            // 
            // SkeletonsCheckBox
            // 
            SkeletonsCheckBox.AutoSize = true;
            SkeletonsCheckBox.Location = new System.Drawing.Point(12, 474);
            SkeletonsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SkeletonsCheckBox.Name = "SkeletonsCheckBox";
            SkeletonsCheckBox.Size = new System.Drawing.Size(107, 19);
            SkeletonsCheckBox.TabIndex = 38;
            SkeletonsCheckBox.Text = "Show skeletons";
            SkeletonsCheckBox.UseVisualStyleBackColor = true;
            SkeletonsCheckBox.CheckedChanged += SkeletonsCheckBox_CheckedChanged;
            // 
            // AudioOuterBoundsCheckBox
            // 
            AudioOuterBoundsCheckBox.AutoSize = true;
            AudioOuterBoundsCheckBox.Checked = true;
            AudioOuterBoundsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            AudioOuterBoundsCheckBox.Location = new System.Drawing.Point(12, 527);
            AudioOuterBoundsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AudioOuterBoundsCheckBox.Name = "AudioOuterBoundsCheckBox";
            AudioOuterBoundsCheckBox.Size = new System.Drawing.Size(162, 19);
            AudioOuterBoundsCheckBox.TabIndex = 40;
            AudioOuterBoundsCheckBox.Text = "Show audio outer bounds";
            AudioOuterBoundsCheckBox.UseVisualStyleBackColor = true;
            AudioOuterBoundsCheckBox.CheckedChanged += AudioOuterBoundsCheckBox_CheckedChanged;
            // 
            // PopZonesCheckBox
            // 
            PopZonesCheckBox.AutoSize = true;
            PopZonesCheckBox.Location = new System.Drawing.Point(12, 448);
            PopZonesCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            PopZonesCheckBox.Name = "PopZonesCheckBox";
            PopZonesCheckBox.Size = new System.Drawing.Size(149, 19);
            PopZonesCheckBox.TabIndex = 37;
            PopZonesCheckBox.Text = "Show population zones";
            PopZonesCheckBox.UseVisualStyleBackColor = true;
            PopZonesCheckBox.CheckedChanged += PopZonesCheckBox_CheckedChanged;
            // 
            // NavMeshesCheckBox
            // 
            NavMeshesCheckBox.AutoSize = true;
            NavMeshesCheckBox.Location = new System.Drawing.Point(12, 421);
            NavMeshesCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NavMeshesCheckBox.Name = "NavMeshesCheckBox";
            NavMeshesCheckBox.Size = new System.Drawing.Size(120, 19);
            NavMeshesCheckBox.TabIndex = 36;
            NavMeshesCheckBox.Text = "Show nav meshes";
            NavMeshesCheckBox.UseVisualStyleBackColor = true;
            NavMeshesCheckBox.CheckedChanged += NavMeshesCheckBox_CheckedChanged;
            // 
            // TrainPathsCheckBox
            // 
            TrainPathsCheckBox.AutoSize = true;
            TrainPathsCheckBox.Location = new System.Drawing.Point(12, 395);
            TrainPathsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TrainPathsCheckBox.Name = "TrainPathsCheckBox";
            TrainPathsCheckBox.Size = new System.Drawing.Size(114, 19);
            TrainPathsCheckBox.TabIndex = 35;
            TrainPathsCheckBox.Text = "Show train paths";
            TrainPathsCheckBox.UseVisualStyleBackColor = true;
            TrainPathsCheckBox.CheckedChanged += TrainPathsCheckBox_CheckedChanged;
            // 
            // PathsDepthClipCheckBox
            // 
            PathsDepthClipCheckBox.AutoSize = true;
            PathsDepthClipCheckBox.Checked = true;
            PathsDepthClipCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            PathsDepthClipCheckBox.Location = new System.Drawing.Point(12, 501);
            PathsDepthClipCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            PathsDepthClipCheckBox.Name = "PathsDepthClipCheckBox";
            PathsDepthClipCheckBox.Size = new System.Drawing.Size(111, 19);
            PathsDepthClipCheckBox.TabIndex = 39;
            PathsDepthClipCheckBox.Text = "Paths depth clip";
            PathsDepthClipCheckBox.UseVisualStyleBackColor = true;
            PathsDepthClipCheckBox.CheckedChanged += PathsDepthClipCheckBox_CheckedChanged;
            // 
            // PathBoundsCheckBox
            // 
            PathBoundsCheckBox.AutoSize = true;
            PathBoundsCheckBox.Checked = true;
            PathBoundsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            PathBoundsCheckBox.Location = new System.Drawing.Point(114, 368);
            PathBoundsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            PathBoundsCheckBox.Name = "PathBoundsCheckBox";
            PathBoundsCheckBox.Size = new System.Drawing.Size(93, 19);
            PathBoundsCheckBox.TabIndex = 34;
            PathBoundsCheckBox.Text = "Path bounds";
            PathBoundsCheckBox.UseVisualStyleBackColor = true;
            PathBoundsCheckBox.CheckedChanged += PathBoundsCheckBox_CheckedChanged;
            // 
            // SelectionWidgetCheckBox
            // 
            SelectionWidgetCheckBox.AutoSize = true;
            SelectionWidgetCheckBox.Checked = true;
            SelectionWidgetCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            SelectionWidgetCheckBox.Location = new System.Drawing.Point(12, 267);
            SelectionWidgetCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectionWidgetCheckBox.Name = "SelectionWidgetCheckBox";
            SelectionWidgetCheckBox.Size = new System.Drawing.Size(94, 19);
            SelectionWidgetCheckBox.TabIndex = 28;
            SelectionWidgetCheckBox.Text = "Show widget";
            SelectionWidgetCheckBox.UseVisualStyleBackColor = true;
            SelectionWidgetCheckBox.CheckedChanged += SelectionWidgetCheckBox_CheckedChanged;
            // 
            // MarkerStyleComboBox
            // 
            MarkerStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            MarkerStyleComboBox.FormattingEnabled = true;
            MarkerStyleComboBox.Location = new System.Drawing.Point(93, 7);
            MarkerStyleComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MarkerStyleComboBox.Name = "MarkerStyleComboBox";
            MarkerStyleComboBox.Size = new System.Drawing.Size(132, 23);
            MarkerStyleComboBox.TabIndex = 18;
            MarkerStyleComboBox.SelectedIndexChanged += MarkerStyleComboBox_SelectedIndexChanged;
            MarkerStyleComboBox.KeyPress += MarkerStyleComboBox_KeyPress;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(5, 10);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(74, 15);
            label4.TabIndex = 17;
            label4.Text = "Marker style:";
            // 
            // LocatorStyleComboBox
            // 
            LocatorStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            LocatorStyleComboBox.FormattingEnabled = true;
            LocatorStyleComboBox.Location = new System.Drawing.Point(93, 38);
            LocatorStyleComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LocatorStyleComboBox.Name = "LocatorStyleComboBox";
            LocatorStyleComboBox.Size = new System.Drawing.Size(132, 23);
            LocatorStyleComboBox.TabIndex = 20;
            LocatorStyleComboBox.SelectedIndexChanged += LocatorStyleComboBox_SelectedIndexChanged;
            LocatorStyleComboBox.KeyPress += LocatorStyleComboBox_KeyPress;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(5, 42);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(77, 15);
            label5.TabIndex = 19;
            label5.Text = "Locator style:";
            // 
            // MarkerDepthClipCheckBox
            // 
            MarkerDepthClipCheckBox.AutoSize = true;
            MarkerDepthClipCheckBox.Location = new System.Drawing.Point(12, 69);
            MarkerDepthClipCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MarkerDepthClipCheckBox.Name = "MarkerDepthClipCheckBox";
            MarkerDepthClipCheckBox.Size = new System.Drawing.Size(119, 19);
            MarkerDepthClipCheckBox.TabIndex = 21;
            MarkerDepthClipCheckBox.Text = "Marker depth clip";
            MarkerDepthClipCheckBox.UseVisualStyleBackColor = true;
            MarkerDepthClipCheckBox.Visible = false;
            MarkerDepthClipCheckBox.CheckedChanged += MarkerDepthClipCheckBox_CheckedChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(5, 157);
            label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(83, 15);
            label9.TabIndex = 25;
            label9.Text = "Bounds range:";
            // 
            // PathsCheckBox
            // 
            PathsCheckBox.AutoSize = true;
            PathsCheckBox.Location = new System.Drawing.Point(12, 368);
            PathsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            PathsCheckBox.Name = "PathsCheckBox";
            PathsCheckBox.Size = new System.Drawing.Size(87, 19);
            PathsCheckBox.TabIndex = 33;
            PathsCheckBox.Text = "Show paths";
            PathsCheckBox.UseVisualStyleBackColor = true;
            PathsCheckBox.CheckedChanged += PathsCheckBox_CheckedChanged;
            // 
            // SelectionBoundsCheckBox
            // 
            SelectionBoundsCheckBox.AutoSize = true;
            SelectionBoundsCheckBox.Checked = true;
            SelectionBoundsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            SelectionBoundsCheckBox.Location = new System.Drawing.Point(12, 224);
            SelectionBoundsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectionBoundsCheckBox.Name = "SelectionBoundsCheckBox";
            SelectionBoundsCheckBox.Size = new System.Drawing.Size(148, 19);
            SelectionBoundsCheckBox.TabIndex = 27;
            SelectionBoundsCheckBox.Text = "Show selection bounds";
            SelectionBoundsCheckBox.UseVisualStyleBackColor = true;
            SelectionBoundsCheckBox.CheckedChanged += SelectionBoundsCheckBox_CheckedChanged;
            // 
            // BoundsDepthClipCheckBox
            // 
            BoundsDepthClipCheckBox.AutoSize = true;
            BoundsDepthClipCheckBox.Checked = true;
            BoundsDepthClipCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            BoundsDepthClipCheckBox.Location = new System.Drawing.Point(12, 132);
            BoundsDepthClipCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BoundsDepthClipCheckBox.Name = "BoundsDepthClipCheckBox";
            BoundsDepthClipCheckBox.Size = new System.Drawing.Size(122, 19);
            BoundsDepthClipCheckBox.TabIndex = 24;
            BoundsDepthClipCheckBox.Text = "Bounds depth clip";
            BoundsDepthClipCheckBox.UseVisualStyleBackColor = true;
            BoundsDepthClipCheckBox.CheckedChanged += BoundsDepthClipCheckBox_CheckedChanged;
            // 
            // BoundsRangeTrackBar
            // 
            BoundsRangeTrackBar.AutoSize = false;
            BoundsRangeTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            BoundsRangeTrackBar.LargeChange = 10;
            BoundsRangeTrackBar.Location = new System.Drawing.Point(7, 175);
            BoundsRangeTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BoundsRangeTrackBar.Maximum = 100;
            BoundsRangeTrackBar.Minimum = 1;
            BoundsRangeTrackBar.Name = "BoundsRangeTrackBar";
            BoundsRangeTrackBar.Size = new System.Drawing.Size(219, 38);
            BoundsRangeTrackBar.TabIndex = 26;
            BoundsRangeTrackBar.TickFrequency = 10;
            BoundsRangeTrackBar.Value = 100;
            BoundsRangeTrackBar.Scroll += BoundsRangeTrackBar_Scroll;
            // 
            // BoundsStyleComboBox
            // 
            BoundsStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            BoundsStyleComboBox.FormattingEnabled = true;
            BoundsStyleComboBox.Items.AddRange(new object[] { "None", "Boxes", "Spheres" });
            BoundsStyleComboBox.Location = new System.Drawing.Point(93, 100);
            BoundsStyleComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BoundsStyleComboBox.Name = "BoundsStyleComboBox";
            BoundsStyleComboBox.Size = new System.Drawing.Size(132, 23);
            BoundsStyleComboBox.TabIndex = 23;
            BoundsStyleComboBox.SelectedIndexChanged += BoundsStyleComboBox_SelectedIndexChanged;
            BoundsStyleComboBox.KeyPress += BoundsStyleComboBox_KeyPress;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(5, 104);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(77, 15);
            label8.TabIndex = 22;
            label8.Text = "Bounds style:";
            // 
            // OptionsLightingTabPage
            // 
            OptionsLightingTabPage.Controls.Add(HDLightsCheckBox);
            OptionsLightingTabPage.Controls.Add(DeferredShadingCheckBox);
            OptionsLightingTabPage.Controls.Add(WeatherRegionComboBox);
            OptionsLightingTabPage.Controls.Add(label29);
            OptionsLightingTabPage.Controls.Add(CloudParamTrackBar);
            OptionsLightingTabPage.Controls.Add(CloudParamComboBox);
            OptionsLightingTabPage.Controls.Add(label23);
            OptionsLightingTabPage.Controls.Add(CloudsComboBox);
            OptionsLightingTabPage.Controls.Add(label21);
            OptionsLightingTabPage.Controls.Add(TimeSpeedLabel);
            OptionsLightingTabPage.Controls.Add(label20);
            OptionsLightingTabPage.Controls.Add(TimeSpeedTrackBar);
            OptionsLightingTabPage.Controls.Add(TimeStartStopButton);
            OptionsLightingTabPage.Controls.Add(ArtificialAmbientLightCheckBox);
            OptionsLightingTabPage.Controls.Add(NaturalAmbientLightCheckBox);
            OptionsLightingTabPage.Controls.Add(LODLightsCheckBox);
            OptionsLightingTabPage.Controls.Add(HDRRenderingCheckBox);
            OptionsLightingTabPage.Controls.Add(ControlTimeOfDayCheckBox);
            OptionsLightingTabPage.Controls.Add(TimeOfDayLabel);
            OptionsLightingTabPage.Controls.Add(label19);
            OptionsLightingTabPage.Controls.Add(TimeOfDayTrackBar);
            OptionsLightingTabPage.Controls.Add(WeatherComboBox);
            OptionsLightingTabPage.Controls.Add(label17);
            OptionsLightingTabPage.Controls.Add(ControlLightDirectionCheckBox);
            OptionsLightingTabPage.Controls.Add(SkydomeCheckBox);
            OptionsLightingTabPage.Controls.Add(ShadowsCheckBox);
            OptionsLightingTabPage.Location = new System.Drawing.Point(4, 24);
            OptionsLightingTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OptionsLightingTabPage.Name = "OptionsLightingTabPage";
            OptionsLightingTabPage.Size = new System.Drawing.Size(235, 558);
            OptionsLightingTabPage.TabIndex = 2;
            OptionsLightingTabPage.Text = "Lighting";
            OptionsLightingTabPage.UseVisualStyleBackColor = true;
            // 
            // HDLightsCheckBox
            // 
            HDLightsCheckBox.AutoSize = true;
            HDLightsCheckBox.Checked = true;
            HDLightsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            HDLightsCheckBox.Location = new System.Drawing.Point(12, 107);
            HDLightsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            HDLightsCheckBox.Name = "HDLightsCheckBox";
            HDLightsCheckBox.Size = new System.Drawing.Size(75, 19);
            HDLightsCheckBox.TabIndex = 34;
            HDLightsCheckBox.Text = "HD lights";
            HDLightsCheckBox.UseVisualStyleBackColor = true;
            HDLightsCheckBox.CheckedChanged += HDLightsCheckBox_CheckedChanged;
            // 
            // DeferredShadingCheckBox
            // 
            DeferredShadingCheckBox.AutoSize = true;
            DeferredShadingCheckBox.Checked = true;
            DeferredShadingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            DeferredShadingCheckBox.Location = new System.Drawing.Point(12, 6);
            DeferredShadingCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            DeferredShadingCheckBox.Name = "DeferredShadingCheckBox";
            DeferredShadingCheckBox.Size = new System.Drawing.Size(116, 19);
            DeferredShadingCheckBox.TabIndex = 30;
            DeferredShadingCheckBox.Text = "Deferred shading";
            DeferredShadingCheckBox.UseVisualStyleBackColor = true;
            DeferredShadingCheckBox.CheckedChanged += DeferredShadingCheckBox_CheckedChanged;
            // 
            // WeatherRegionComboBox
            // 
            WeatherRegionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            WeatherRegionComboBox.FormattingEnabled = true;
            WeatherRegionComboBox.Items.AddRange(new object[] { "GLOBAL", "URBAN" });
            WeatherRegionComboBox.Location = new System.Drawing.Point(71, 410);
            WeatherRegionComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WeatherRegionComboBox.Name = "WeatherRegionComboBox";
            WeatherRegionComboBox.Size = new System.Drawing.Size(154, 23);
            WeatherRegionComboBox.TabIndex = 50;
            WeatherRegionComboBox.SelectedIndexChanged += WeatherRegionComboBox_SelectedIndexChanged;
            WeatherRegionComboBox.KeyPress += WeatherRegionComboBox_KeyPress;
            // 
            // label29
            // 
            label29.AutoSize = true;
            label29.Location = new System.Drawing.Point(5, 413);
            label29.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label29.Name = "label29";
            label29.Size = new System.Drawing.Size(47, 15);
            label29.TabIndex = 49;
            label29.Text = "Region:";
            // 
            // CloudParamTrackBar
            // 
            CloudParamTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            CloudParamTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            CloudParamTrackBar.LargeChange = 10;
            CloudParamTrackBar.Location = new System.Drawing.Point(7, 503);
            CloudParamTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CloudParamTrackBar.Maximum = 200;
            CloudParamTrackBar.Name = "CloudParamTrackBar";
            CloudParamTrackBar.Size = new System.Drawing.Size(219, 45);
            CloudParamTrackBar.TabIndex = 55;
            CloudParamTrackBar.TickFrequency = 10;
            CloudParamTrackBar.Value = 100;
            CloudParamTrackBar.Scroll += CloudParamTrackBar_Scroll;
            // 
            // CloudParamComboBox
            // 
            CloudParamComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            CloudParamComboBox.FormattingEnabled = true;
            CloudParamComboBox.Items.AddRange(new object[] { "<Loading...>" });
            CloudParamComboBox.Location = new System.Drawing.Point(91, 472);
            CloudParamComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CloudParamComboBox.Name = "CloudParamComboBox";
            CloudParamComboBox.Size = new System.Drawing.Size(135, 23);
            CloudParamComboBox.TabIndex = 54;
            CloudParamComboBox.SelectedIndexChanged += CloudParamComboBox_SelectedIndexChanged;
            CloudParamComboBox.KeyPress += CloudParamComboBox_KeyPress;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new System.Drawing.Point(5, 475);
            label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label23.Name = "label23";
            label23.Size = new System.Drawing.Size(79, 15);
            label23.TabIndex = 53;
            label23.Text = "Cloud param:";
            // 
            // CloudsComboBox
            // 
            CloudsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            CloudsComboBox.FormattingEnabled = true;
            CloudsComboBox.Items.AddRange(new object[] { "<Loading...>" });
            CloudsComboBox.Location = new System.Drawing.Point(71, 441);
            CloudsComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CloudsComboBox.Name = "CloudsComboBox";
            CloudsComboBox.Size = new System.Drawing.Size(154, 23);
            CloudsComboBox.TabIndex = 52;
            CloudsComboBox.SelectedIndexChanged += CloudsComboBox_SelectedIndexChanged;
            CloudsComboBox.KeyPress += CloudsComboBox_KeyPress;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new System.Drawing.Point(5, 444);
            label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label21.Name = "label21";
            label21.Size = new System.Drawing.Size(47, 15);
            label21.TabIndex = 51;
            label21.Text = "Clouds:";
            // 
            // TimeSpeedLabel
            // 
            TimeSpeedLabel.AutoSize = true;
            TimeSpeedLabel.Location = new System.Drawing.Point(91, 303);
            TimeSpeedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            TimeSpeedLabel.Name = "TimeSpeedLabel";
            TimeSpeedLabel.Size = new System.Drawing.Size(68, 15);
            TimeSpeedLabel.TabIndex = 44;
            TimeSpeedLabel.Text = "0.5 min/sec";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new System.Drawing.Point(4, 303);
            label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label20.Name = "label20";
            label20.Size = new System.Drawing.Size(70, 15);
            label20.TabIndex = 43;
            label20.Text = "Time speed:";
            // 
            // TimeSpeedTrackBar
            // 
            TimeSpeedTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            TimeSpeedTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            TimeSpeedTrackBar.Location = new System.Drawing.Point(71, 322);
            TimeSpeedTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TimeSpeedTrackBar.Maximum = 100;
            TimeSpeedTrackBar.Minimum = 40;
            TimeSpeedTrackBar.Name = "TimeSpeedTrackBar";
            TimeSpeedTrackBar.Size = new System.Drawing.Size(155, 45);
            TimeSpeedTrackBar.TabIndex = 46;
            TimeSpeedTrackBar.TickFrequency = 5;
            TimeSpeedTrackBar.Value = 50;
            TimeSpeedTrackBar.Scroll += TimeSpeedTrackBar_Scroll;
            // 
            // TimeStartStopButton
            // 
            TimeStartStopButton.Location = new System.Drawing.Point(12, 322);
            TimeStartStopButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TimeStartStopButton.Name = "TimeStartStopButton";
            TimeStartStopButton.Size = new System.Drawing.Size(52, 27);
            TimeStartStopButton.TabIndex = 45;
            TimeStartStopButton.Text = "Start";
            TimeStartStopButton.UseVisualStyleBackColor = true;
            TimeStartStopButton.Click += TimeStartStopButton_Click;
            // 
            // ArtificialAmbientLightCheckBox
            // 
            ArtificialAmbientLightCheckBox.AutoSize = true;
            ArtificialAmbientLightCheckBox.Checked = true;
            ArtificialAmbientLightCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            ArtificialAmbientLightCheckBox.Location = new System.Drawing.Point(12, 158);
            ArtificialAmbientLightCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ArtificialAmbientLightCheckBox.Name = "ArtificialAmbientLightCheckBox";
            ArtificialAmbientLightCheckBox.Size = new System.Drawing.Size(144, 19);
            ArtificialAmbientLightCheckBox.TabIndex = 37;
            ArtificialAmbientLightCheckBox.Text = "Artificial ambient light";
            ArtificialAmbientLightCheckBox.UseVisualStyleBackColor = true;
            ArtificialAmbientLightCheckBox.CheckedChanged += ArtificialAmbientLightCheckBox_CheckedChanged;
            // 
            // NaturalAmbientLightCheckBox
            // 
            NaturalAmbientLightCheckBox.AutoSize = true;
            NaturalAmbientLightCheckBox.Checked = true;
            NaturalAmbientLightCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            NaturalAmbientLightCheckBox.Location = new System.Drawing.Point(12, 133);
            NaturalAmbientLightCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NaturalAmbientLightCheckBox.Name = "NaturalAmbientLightCheckBox";
            NaturalAmbientLightCheckBox.Size = new System.Drawing.Size(139, 19);
            NaturalAmbientLightCheckBox.TabIndex = 36;
            NaturalAmbientLightCheckBox.Text = "Natural ambient light";
            NaturalAmbientLightCheckBox.UseVisualStyleBackColor = true;
            NaturalAmbientLightCheckBox.CheckedChanged += NaturalAmbientLightCheckBox_CheckedChanged;
            // 
            // LODLightsCheckBox
            // 
            LODLightsCheckBox.AutoSize = true;
            LODLightsCheckBox.Checked = true;
            LODLightsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            LODLightsCheckBox.Location = new System.Drawing.Point(104, 107);
            LODLightsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LODLightsCheckBox.Name = "LODLightsCheckBox";
            LODLightsCheckBox.Size = new System.Drawing.Size(81, 19);
            LODLightsCheckBox.TabIndex = 35;
            LODLightsCheckBox.Text = "LOD lights";
            LODLightsCheckBox.UseVisualStyleBackColor = true;
            LODLightsCheckBox.CheckedChanged += LODLightsCheckBox_CheckedChanged;
            // 
            // HDRRenderingCheckBox
            // 
            HDRRenderingCheckBox.AutoSize = true;
            HDRRenderingCheckBox.Checked = true;
            HDRRenderingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            HDRRenderingCheckBox.Location = new System.Drawing.Point(12, 31);
            HDRRenderingCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            HDRRenderingCheckBox.Name = "HDRRenderingCheckBox";
            HDRRenderingCheckBox.Size = new System.Drawing.Size(104, 19);
            HDRRenderingCheckBox.TabIndex = 31;
            HDRRenderingCheckBox.Text = "HDR rendering";
            HDRRenderingCheckBox.UseVisualStyleBackColor = true;
            HDRRenderingCheckBox.CheckedChanged += HDRRenderingCheckBox_CheckedChanged;
            // 
            // ControlTimeOfDayCheckBox
            // 
            ControlTimeOfDayCheckBox.AutoSize = true;
            ControlTimeOfDayCheckBox.Checked = true;
            ControlTimeOfDayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            ControlTimeOfDayCheckBox.Location = new System.Drawing.Point(12, 209);
            ControlTimeOfDayCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ControlTimeOfDayCheckBox.Name = "ControlTimeOfDayCheckBox";
            ControlTimeOfDayCheckBox.Size = new System.Drawing.Size(194, 19);
            ControlTimeOfDayCheckBox.TabIndex = 39;
            ControlTimeOfDayCheckBox.Text = "Control time of day (right-drag)";
            ControlTimeOfDayCheckBox.UseVisualStyleBackColor = true;
            ControlTimeOfDayCheckBox.CheckedChanged += ControlTimeOfDayCheckBox_CheckedChanged;
            // 
            // TimeOfDayLabel
            // 
            TimeOfDayLabel.AutoSize = true;
            TimeOfDayLabel.Location = new System.Drawing.Point(88, 240);
            TimeOfDayLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            TimeOfDayLabel.Name = "TimeOfDayLabel";
            TimeOfDayLabel.Size = new System.Drawing.Size(34, 15);
            TimeOfDayLabel.TabIndex = 41;
            TimeOfDayLabel.Text = "12:00";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new System.Drawing.Point(5, 240);
            label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label19.Name = "label19";
            label19.Size = new System.Drawing.Size(72, 15);
            label19.TabIndex = 40;
            label19.Text = "Time of day:";
            // 
            // TimeOfDayTrackBar
            // 
            TimeOfDayTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            TimeOfDayTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            TimeOfDayTrackBar.LargeChange = 60;
            TimeOfDayTrackBar.Location = new System.Drawing.Point(7, 258);
            TimeOfDayTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TimeOfDayTrackBar.Maximum = 1440;
            TimeOfDayTrackBar.Name = "TimeOfDayTrackBar";
            TimeOfDayTrackBar.Size = new System.Drawing.Size(219, 45);
            TimeOfDayTrackBar.TabIndex = 42;
            TimeOfDayTrackBar.TickFrequency = 60;
            TimeOfDayTrackBar.Value = 720;
            TimeOfDayTrackBar.Scroll += TimeOfDayTrackBar_Scroll;
            // 
            // WeatherComboBox
            // 
            WeatherComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            WeatherComboBox.FormattingEnabled = true;
            WeatherComboBox.Items.AddRange(new object[] { "<Loading...>" });
            WeatherComboBox.Location = new System.Drawing.Point(71, 378);
            WeatherComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WeatherComboBox.Name = "WeatherComboBox";
            WeatherComboBox.Size = new System.Drawing.Size(154, 23);
            WeatherComboBox.TabIndex = 48;
            WeatherComboBox.SelectedIndexChanged += WeatherComboBox_SelectedIndexChanged;
            WeatherComboBox.KeyPress += WeatherComboBox_KeyPress;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new System.Drawing.Point(5, 382);
            label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label17.Name = "label17";
            label17.Size = new System.Drawing.Size(54, 15);
            label17.TabIndex = 47;
            label17.Text = "Weather:";
            // 
            // ControlLightDirectionCheckBox
            // 
            ControlLightDirectionCheckBox.AutoSize = true;
            ControlLightDirectionCheckBox.Location = new System.Drawing.Point(12, 183);
            ControlLightDirectionCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ControlLightDirectionCheckBox.Name = "ControlLightDirectionCheckBox";
            ControlLightDirectionCheckBox.Size = new System.Drawing.Size(208, 19);
            ControlLightDirectionCheckBox.TabIndex = 38;
            ControlLightDirectionCheckBox.Text = "Control light direction (right-drag)";
            ControlLightDirectionCheckBox.UseVisualStyleBackColor = true;
            ControlLightDirectionCheckBox.CheckedChanged += ControlLightDirectionCheckBox_CheckedChanged;
            // 
            // SkydomeCheckBox
            // 
            SkydomeCheckBox.AutoSize = true;
            SkydomeCheckBox.Checked = true;
            SkydomeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            SkydomeCheckBox.Location = new System.Drawing.Point(12, 82);
            SkydomeCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SkydomeCheckBox.Name = "SkydomeCheckBox";
            SkydomeCheckBox.Size = new System.Drawing.Size(75, 19);
            SkydomeCheckBox.TabIndex = 33;
            SkydomeCheckBox.Text = "Skydome";
            SkydomeCheckBox.UseVisualStyleBackColor = true;
            SkydomeCheckBox.CheckedChanged += SkydomeCheckbox_CheckedChanged;
            // 
            // ShadowsCheckBox
            // 
            ShadowsCheckBox.AutoSize = true;
            ShadowsCheckBox.Location = new System.Drawing.Point(12, 57);
            ShadowsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ShadowsCheckBox.Name = "ShadowsCheckBox";
            ShadowsCheckBox.Size = new System.Drawing.Size(73, 19);
            ShadowsCheckBox.TabIndex = 32;
            ShadowsCheckBox.Text = "Shadows";
            ShadowsCheckBox.UseVisualStyleBackColor = true;
            ShadowsCheckBox.CheckedChanged += ShadowsCheckBox_CheckedChanged;
            // 
            // StatusBarCheckBox
            // 
            StatusBarCheckBox.AutoSize = true;
            StatusBarCheckBox.Checked = true;
            StatusBarCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            StatusBarCheckBox.Location = new System.Drawing.Point(139, 597);
            StatusBarCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            StatusBarCheckBox.Name = "StatusBarCheckBox";
            StatusBarCheckBox.Size = new System.Drawing.Size(78, 19);
            StatusBarCheckBox.TabIndex = 145;
            StatusBarCheckBox.Text = "Status bar";
            StatusBarCheckBox.UseVisualStyleBackColor = true;
            StatusBarCheckBox.CheckedChanged += StatusBarCheckBox_CheckedChanged;
            // 
            // QuitButton
            // 
            QuitButton.Location = new System.Drawing.Point(122, 657);
            QuitButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            QuitButton.Name = "QuitButton";
            QuitButton.Size = new System.Drawing.Size(108, 27);
            QuitButton.TabIndex = 149;
            QuitButton.Text = "Quit";
            QuitButton.UseVisualStyleBackColor = true;
            QuitButton.Click += QuitButton_Click;
            // 
            // ReloadSettingsButton
            // 
            ReloadSettingsButton.Enabled = false;
            ReloadSettingsButton.Location = new System.Drawing.Point(7, 623);
            ReloadSettingsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ReloadSettingsButton.Name = "ReloadSettingsButton";
            ReloadSettingsButton.Size = new System.Drawing.Size(108, 27);
            ReloadSettingsButton.TabIndex = 146;
            ReloadSettingsButton.Text = "Reload settings";
            ReloadSettingsButton.UseVisualStyleBackColor = true;
            ReloadSettingsButton.Visible = false;
            ReloadSettingsButton.Click += ReloadSettingsButton_Click;
            // 
            // SaveSettingsButton
            // 
            SaveSettingsButton.Location = new System.Drawing.Point(122, 623);
            SaveSettingsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SaveSettingsButton.Name = "SaveSettingsButton";
            SaveSettingsButton.Size = new System.Drawing.Size(108, 27);
            SaveSettingsButton.TabIndex = 147;
            SaveSettingsButton.Text = "Save settings";
            SaveSettingsButton.UseVisualStyleBackColor = true;
            SaveSettingsButton.Click += SaveSettingsButton_Click;
            // 
            // ReloadShadersButton
            // 
            ReloadShadersButton.Location = new System.Drawing.Point(7, 657);
            ReloadShadersButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ReloadShadersButton.Name = "ReloadShadersButton";
            ReloadShadersButton.Size = new System.Drawing.Size(108, 27);
            ReloadShadersButton.TabIndex = 148;
            ReloadShadersButton.Text = "Reload shaders";
            ReloadShadersButton.UseVisualStyleBackColor = true;
            ReloadShadersButton.Click += ReloadShadersButton_Click;
            // 
            // ErrorConsoleCheckBox
            // 
            ErrorConsoleCheckBox.AutoSize = true;
            ErrorConsoleCheckBox.Location = new System.Drawing.Point(16, 597);
            ErrorConsoleCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ErrorConsoleCheckBox.Name = "ErrorConsoleCheckBox";
            ErrorConsoleCheckBox.Size = new System.Drawing.Size(95, 19);
            ErrorConsoleCheckBox.TabIndex = 144;
            ErrorConsoleCheckBox.Text = "Error console";
            ErrorConsoleCheckBox.UseVisualStyleBackColor = true;
            ErrorConsoleCheckBox.CheckedChanged += ErrorConsoleCheckBox_CheckedChanged;
            // 
            // ToolsPanelHideButton
            // 
            ToolsPanelHideButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            ToolsPanelHideButton.Location = new System.Drawing.Point(216, 3);
            ToolsPanelHideButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ToolsPanelHideButton.Name = "ToolsPanelHideButton";
            ToolsPanelHideButton.Size = new System.Drawing.Size(35, 27);
            ToolsPanelHideButton.TabIndex = 4;
            ToolsPanelHideButton.Text = ">>";
            ToolsPanelHideButton.UseVisualStyleBackColor = true;
            ToolsPanelHideButton.Click += ToolsPanelHideButton_Click;
            // 
            // ToolsPanelShowButton
            // 
            ToolsPanelShowButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            ToolsPanelShowButton.Location = new System.Drawing.Point(1096, 17);
            ToolsPanelShowButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ToolsPanelShowButton.Name = "ToolsPanelShowButton";
            ToolsPanelShowButton.Size = new System.Drawing.Size(35, 27);
            ToolsPanelShowButton.TabIndex = 0;
            ToolsPanelShowButton.Text = "<<";
            ToolsPanelShowButton.UseVisualStyleBackColor = true;
            ToolsPanelShowButton.Click += ToolsPanelShowButton_Click;
            // 
            // ConsolePanel
            // 
            ConsolePanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ConsolePanel.BackColor = System.Drawing.SystemColors.Control;
            ConsolePanel.Controls.Add(ConsoleTextBox);
            ConsolePanel.Location = new System.Drawing.Point(14, 665);
            ConsolePanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConsolePanel.Name = "ConsolePanel";
            ConsolePanel.Size = new System.Drawing.Size(859, 117);
            ConsolePanel.TabIndex = 3;
            ConsolePanel.Visible = false;
            // 
            // ConsoleTextBox
            // 
            ConsoleTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ConsoleTextBox.Location = new System.Drawing.Point(4, 3);
            ConsoleTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConsoleTextBox.Multiline = true;
            ConsoleTextBox.Name = "ConsoleTextBox";
            ConsoleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            ConsoleTextBox.Size = new System.Drawing.Size(851, 109);
            ConsoleTextBox.TabIndex = 0;
            // 
            // StatsUpdateTimer
            // 
            StatsUpdateTimer.Enabled = true;
            StatsUpdateTimer.Interval = 500;
            StatsUpdateTimer.Tick += StatsUpdateTimer_Tick;
            // 
            // SelectedMarkerPanel
            // 
            SelectedMarkerPanel.BackColor = System.Drawing.Color.White;
            SelectedMarkerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            SelectedMarkerPanel.Controls.Add(SelectedMarkerPositionTextBox);
            SelectedMarkerPanel.Controls.Add(SelectedMarkerNameTextBox);
            SelectedMarkerPanel.Location = new System.Drawing.Point(14, 74);
            SelectedMarkerPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectedMarkerPanel.Name = "SelectedMarkerPanel";
            SelectedMarkerPanel.Size = new System.Drawing.Size(210, 48);
            SelectedMarkerPanel.TabIndex = 5;
            SelectedMarkerPanel.Visible = false;
            // 
            // SelectedMarkerPositionTextBox
            // 
            SelectedMarkerPositionTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SelectedMarkerPositionTextBox.BackColor = System.Drawing.Color.White;
            SelectedMarkerPositionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            SelectedMarkerPositionTextBox.Location = new System.Drawing.Point(4, 25);
            SelectedMarkerPositionTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectedMarkerPositionTextBox.Name = "SelectedMarkerPositionTextBox";
            SelectedMarkerPositionTextBox.ReadOnly = true;
            SelectedMarkerPositionTextBox.Size = new System.Drawing.Size(201, 16);
            SelectedMarkerPositionTextBox.TabIndex = 1;
            // 
            // SelectedMarkerNameTextBox
            // 
            SelectedMarkerNameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SelectedMarkerNameTextBox.BackColor = System.Drawing.Color.White;
            SelectedMarkerNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            SelectedMarkerNameTextBox.Location = new System.Drawing.Point(4, 3);
            SelectedMarkerNameTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectedMarkerNameTextBox.Name = "SelectedMarkerNameTextBox";
            SelectedMarkerNameTextBox.ReadOnly = true;
            SelectedMarkerNameTextBox.Size = new System.Drawing.Size(201, 16);
            SelectedMarkerNameTextBox.TabIndex = 0;
            // 
            // ToolsMenu
            // 
            ToolsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolsMenuRPFBrowser, ToolsMenuRPFExplorer, ToolsMenuSelectionInfo, ToolsMenuProjectWindow, ToolsMenuCutsceneViewer, ToolsMenuAudioExplorer, ToolsMenuWorldSearch, ToolsMenuBinarySearch, ToolsMenuJenkGen, ToolsMenuJenkInd, ToolsMenuExtractScripts, ToolsMenuExtractTextures, ToolsMenuExtractRawFiles, ToolsMenuExtractShaders, ToolsMenuOptions });
            ToolsMenu.Name = "ToolsMenu";
            ToolsMenu.Size = new System.Drawing.Size(170, 334);
            // 
            // ToolsMenuRPFBrowser
            // 
            ToolsMenuRPFBrowser.Name = "ToolsMenuRPFBrowser";
            ToolsMenuRPFBrowser.Size = new System.Drawing.Size(169, 22);
            ToolsMenuRPFBrowser.Text = "RPF Browser...";
            ToolsMenuRPFBrowser.Visible = false;
            ToolsMenuRPFBrowser.Click += ToolsMenuRPFBrowser_Click;
            // 
            // ToolsMenuRPFExplorer
            // 
            ToolsMenuRPFExplorer.Name = "ToolsMenuRPFExplorer";
            ToolsMenuRPFExplorer.Size = new System.Drawing.Size(169, 22);
            ToolsMenuRPFExplorer.Text = "RPF Explorer...";
            ToolsMenuRPFExplorer.Click += ToolsMenuRPFExplorer_Click;
            // 
            // ToolsMenuSelectionInfo
            // 
            ToolsMenuSelectionInfo.Name = "ToolsMenuSelectionInfo";
            ToolsMenuSelectionInfo.Size = new System.Drawing.Size(169, 22);
            ToolsMenuSelectionInfo.Text = "Selection info...";
            ToolsMenuSelectionInfo.Click += ToolsMenuSelectionInfo_Click;
            // 
            // ToolsMenuProjectWindow
            // 
            ToolsMenuProjectWindow.Enabled = false;
            ToolsMenuProjectWindow.Name = "ToolsMenuProjectWindow";
            ToolsMenuProjectWindow.Size = new System.Drawing.Size(169, 22);
            ToolsMenuProjectWindow.Text = "Project window...";
            ToolsMenuProjectWindow.Click += ToolsMenuProjectWindow_Click;
            // 
            // ToolsMenuCutsceneViewer
            // 
            ToolsMenuCutsceneViewer.Enabled = false;
            ToolsMenuCutsceneViewer.Name = "ToolsMenuCutsceneViewer";
            ToolsMenuCutsceneViewer.Size = new System.Drawing.Size(169, 22);
            ToolsMenuCutsceneViewer.Text = "Cutscene viewer...";
            ToolsMenuCutsceneViewer.Click += ToolsMenuCutsceneViewer_Click;
            // 
            // ToolsMenuAudioExplorer
            // 
            ToolsMenuAudioExplorer.Enabled = false;
            ToolsMenuAudioExplorer.Name = "ToolsMenuAudioExplorer";
            ToolsMenuAudioExplorer.Size = new System.Drawing.Size(169, 22);
            ToolsMenuAudioExplorer.Text = "Audio explorer...";
            ToolsMenuAudioExplorer.Click += ToolsMenuAudioExplorer_Click;
            // 
            // ToolsMenuWorldSearch
            // 
            ToolsMenuWorldSearch.Name = "ToolsMenuWorldSearch";
            ToolsMenuWorldSearch.Size = new System.Drawing.Size(169, 22);
            ToolsMenuWorldSearch.Text = "World search...";
            ToolsMenuWorldSearch.Click += ToolsMenuWorldSearch_Click;
            // 
            // ToolsMenuBinarySearch
            // 
            ToolsMenuBinarySearch.Enabled = false;
            ToolsMenuBinarySearch.Name = "ToolsMenuBinarySearch";
            ToolsMenuBinarySearch.Size = new System.Drawing.Size(169, 22);
            ToolsMenuBinarySearch.Text = "Binary search...";
            ToolsMenuBinarySearch.Click += ToolsMenuBinarySearch_Click;
            // 
            // ToolsMenuJenkGen
            // 
            ToolsMenuJenkGen.Name = "ToolsMenuJenkGen";
            ToolsMenuJenkGen.Size = new System.Drawing.Size(169, 22);
            ToolsMenuJenkGen.Text = "JenkGen...";
            ToolsMenuJenkGen.Click += ToolsMenuJenkGen_Click;
            // 
            // ToolsMenuJenkInd
            // 
            ToolsMenuJenkInd.Enabled = false;
            ToolsMenuJenkInd.Name = "ToolsMenuJenkInd";
            ToolsMenuJenkInd.Size = new System.Drawing.Size(169, 22);
            ToolsMenuJenkInd.Text = "JenkInd...";
            ToolsMenuJenkInd.Click += ToolsMenuJenkInd_Click;
            // 
            // ToolsMenuExtractScripts
            // 
            ToolsMenuExtractScripts.Name = "ToolsMenuExtractScripts";
            ToolsMenuExtractScripts.Size = new System.Drawing.Size(169, 22);
            ToolsMenuExtractScripts.Text = "Extract scripts...";
            ToolsMenuExtractScripts.Click += ToolsMenuExtractScripts_Click;
            // 
            // ToolsMenuExtractTextures
            // 
            ToolsMenuExtractTextures.Name = "ToolsMenuExtractTextures";
            ToolsMenuExtractTextures.Size = new System.Drawing.Size(169, 22);
            ToolsMenuExtractTextures.Text = "Extract textures...";
            ToolsMenuExtractTextures.Click += ToolsMenuExtractTextures_Click;
            // 
            // ToolsMenuExtractRawFiles
            // 
            ToolsMenuExtractRawFiles.Name = "ToolsMenuExtractRawFiles";
            ToolsMenuExtractRawFiles.Size = new System.Drawing.Size(169, 22);
            ToolsMenuExtractRawFiles.Text = "Extract raw files...";
            ToolsMenuExtractRawFiles.Click += ToolsMenuExtractRawFiles_Click;
            // 
            // ToolsMenuExtractShaders
            // 
            ToolsMenuExtractShaders.Name = "ToolsMenuExtractShaders";
            ToolsMenuExtractShaders.Size = new System.Drawing.Size(169, 22);
            ToolsMenuExtractShaders.Text = "Extract shaders...";
            ToolsMenuExtractShaders.Click += ToolsMenuExtractShaders_Click;
            // 
            // ToolsMenuOptions
            // 
            ToolsMenuOptions.Name = "ToolsMenuOptions";
            ToolsMenuOptions.Size = new System.Drawing.Size(169, 22);
            ToolsMenuOptions.Text = "Options...";
            ToolsMenuOptions.Click += ToolsMenuOptions_Click;
            // 
            // Toolbar
            // 
            Toolbar.Dock = System.Windows.Forms.DockStyle.None;
            Toolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            Toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolbarNewButton, ToolbarOpenButton, ToolbarSaveButton, ToolbarSaveAllButton, toolStripSeparator5, ToolbarSelectButton, toolStripSeparator1, ToolbarMoveButton, ToolbarRotateButton, ToolbarScaleButton, ToolbarTransformSpaceButton, ToolbarSnapButton, toolStripSeparator2, ToolbarUndoButton, ToolbarRedoButton, toolStripSeparator3, ToolbarInfoWindowButton, ToolbarProjectWindowButton, toolStripSeparator4, ToolbarAddItemButton, ToolbarDeleteItemButton, toolStripSeparator6, ToolbarCopyButton, ToolbarPasteButton, toolStripSeparator7, ToolbarCameraModeButton });
            Toolbar.Location = new System.Drawing.Point(1, 0);
            Toolbar.Name = "Toolbar";
            Toolbar.Size = new System.Drawing.Size(554, 25);
            Toolbar.TabIndex = 6;
            Toolbar.Text = "toolStrip1";
            // 
            // ToolbarNewButton
            // 
            ToolbarNewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarNewButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolbarNewProjectButton, ToolbarNewYmapButton, ToolbarNewYtypButton, ToolbarNewYbnButton, ToolbarNewYndButton, ToolbarNewTrainsButton, ToolbarNewScenarioButton });
            ToolbarNewButton.Enabled = false;
            ToolbarNewButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarNewButton.Image");
            ToolbarNewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarNewButton.Name = "ToolbarNewButton";
            ToolbarNewButton.Size = new System.Drawing.Size(32, 22);
            ToolbarNewButton.Text = "New...";
            ToolbarNewButton.ToolTipText = "New... (Ctrl+N)";
            ToolbarNewButton.ButtonClick += ToolbarNewButton_ButtonClick;
            // 
            // ToolbarNewProjectButton
            // 
            ToolbarNewProjectButton.Name = "ToolbarNewProjectButton";
            ToolbarNewProjectButton.Size = new System.Drawing.Size(167, 22);
            ToolbarNewProjectButton.Text = "New Project";
            ToolbarNewProjectButton.Click += ToolbarNewProjectButton_Click;
            // 
            // ToolbarNewYmapButton
            // 
            ToolbarNewYmapButton.Name = "ToolbarNewYmapButton";
            ToolbarNewYmapButton.Size = new System.Drawing.Size(167, 22);
            ToolbarNewYmapButton.Text = "New Ymap File";
            ToolbarNewYmapButton.Click += ToolbarNewYmapButton_Click;
            // 
            // ToolbarNewYtypButton
            // 
            ToolbarNewYtypButton.Name = "ToolbarNewYtypButton";
            ToolbarNewYtypButton.Size = new System.Drawing.Size(167, 22);
            ToolbarNewYtypButton.Text = "New Ytyp File";
            ToolbarNewYtypButton.Click += ToolbarNewYtypButton_Click;
            // 
            // ToolbarNewYbnButton
            // 
            ToolbarNewYbnButton.Name = "ToolbarNewYbnButton";
            ToolbarNewYbnButton.Size = new System.Drawing.Size(167, 22);
            ToolbarNewYbnButton.Text = "New Ybn File";
            ToolbarNewYbnButton.Click += ToolbarNewYbnButton_Click;
            // 
            // ToolbarNewYndButton
            // 
            ToolbarNewYndButton.Name = "ToolbarNewYndButton";
            ToolbarNewYndButton.Size = new System.Drawing.Size(167, 22);
            ToolbarNewYndButton.Text = "New Ynd File";
            ToolbarNewYndButton.Click += ToolbarNewYndButton_Click;
            // 
            // ToolbarNewTrainsButton
            // 
            ToolbarNewTrainsButton.Name = "ToolbarNewTrainsButton";
            ToolbarNewTrainsButton.Size = new System.Drawing.Size(167, 22);
            ToolbarNewTrainsButton.Text = "New Trains File";
            ToolbarNewTrainsButton.Click += ToolbarNewTrainsButton_Click;
            // 
            // ToolbarNewScenarioButton
            // 
            ToolbarNewScenarioButton.Name = "ToolbarNewScenarioButton";
            ToolbarNewScenarioButton.Size = new System.Drawing.Size(167, 22);
            ToolbarNewScenarioButton.Text = "New Scenario File";
            ToolbarNewScenarioButton.Click += ToolbarNewScenarioButton_Click;
            // 
            // ToolbarOpenButton
            // 
            ToolbarOpenButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarOpenButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolbarOpenProjectButton, ToolbarOpenFilesButton, ToolbarOpenFolderButton });
            ToolbarOpenButton.Enabled = false;
            ToolbarOpenButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarOpenButton.Image");
            ToolbarOpenButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarOpenButton.Name = "ToolbarOpenButton";
            ToolbarOpenButton.Size = new System.Drawing.Size(32, 22);
            ToolbarOpenButton.Text = "Open...";
            ToolbarOpenButton.ToolTipText = "Open... (Ctrl+O)";
            ToolbarOpenButton.ButtonClick += ToolbarOpenButton_ButtonClick;
            // 
            // ToolbarOpenProjectButton
            // 
            ToolbarOpenProjectButton.Name = "ToolbarOpenProjectButton";
            ToolbarOpenProjectButton.Size = new System.Drawing.Size(152, 22);
            ToolbarOpenProjectButton.Text = "Open Project...";
            ToolbarOpenProjectButton.Click += ToolbarOpenProjectButton_Click;
            // 
            // ToolbarOpenFilesButton
            // 
            ToolbarOpenFilesButton.Name = "ToolbarOpenFilesButton";
            ToolbarOpenFilesButton.Size = new System.Drawing.Size(152, 22);
            ToolbarOpenFilesButton.Text = "Open Files...";
            ToolbarOpenFilesButton.Click += ToolbarOpenFilesButton_Click;
            // 
            // ToolbarOpenFolderButton
            // 
            ToolbarOpenFolderButton.Name = "ToolbarOpenFolderButton";
            ToolbarOpenFolderButton.Size = new System.Drawing.Size(152, 22);
            ToolbarOpenFolderButton.Text = "Open Folder...";
            ToolbarOpenFolderButton.Click += ToolbarOpenFolderButton_Click;
            // 
            // ToolbarSaveButton
            // 
            ToolbarSaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarSaveButton.Enabled = false;
            ToolbarSaveButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarSaveButton.Image");
            ToolbarSaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarSaveButton.Name = "ToolbarSaveButton";
            ToolbarSaveButton.Size = new System.Drawing.Size(23, 22);
            ToolbarSaveButton.Text = "Save";
            ToolbarSaveButton.ToolTipText = "Save (Ctrl+S)";
            ToolbarSaveButton.Click += ToolbarSaveButton_Click;
            // 
            // ToolbarSaveAllButton
            // 
            ToolbarSaveAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarSaveAllButton.Enabled = false;
            ToolbarSaveAllButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarSaveAllButton.Image");
            ToolbarSaveAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarSaveAllButton.Name = "ToolbarSaveAllButton";
            ToolbarSaveAllButton.Size = new System.Drawing.Size(23, 22);
            ToolbarSaveAllButton.Text = "Save All";
            ToolbarSaveAllButton.ToolTipText = "Save All (Ctrl+Shift+S)";
            ToolbarSaveAllButton.Click += ToolbarSaveAllButton_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarSelectButton
            // 
            ToolbarSelectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarSelectButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolbarSelectEntityButton, ToolbarSelectEntityExtensionButton, ToolbarSelectArchetypeExtensionButton, ToolbarSelectTimeCycleModifierButton, ToolbarSelectCarGeneratorButton, ToolbarSelectGrassButton, ToolbarSelectWaterQuadButton, ToolbarSelectCalmingQuadButton, ToolbarSelectWaveQuadButton, ToolbarSelectCollisionButton, ToolbarSelectNavMeshButton, ToolbarSelectPathButton, ToolbarSelectTrainTrackButton, ToolbarSelectLodLightsButton, ToolbarSelectMloInstanceButton, ToolbarSelectScenarioButton, ToolbarSelectAudioButton, ToolbarSelectOcclusionButton });
            ToolbarSelectButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarSelectButton.Image");
            ToolbarSelectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarSelectButton.Name = "ToolbarSelectButton";
            ToolbarSelectButton.Size = new System.Drawing.Size(32, 22);
            ToolbarSelectButton.Text = "Select objects / Exit edit mode";
            ToolbarSelectButton.ToolTipText = "Select objects / Exit edit mode (C, Q)";
            ToolbarSelectButton.ButtonClick += ToolbarSelectButton_ButtonClick;
            // 
            // ToolbarSelectEntityButton
            // 
            ToolbarSelectEntityButton.Checked = true;
            ToolbarSelectEntityButton.CheckState = System.Windows.Forms.CheckState.Checked;
            ToolbarSelectEntityButton.Name = "ToolbarSelectEntityButton";
            ToolbarSelectEntityButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectEntityButton.Text = "Entity";
            ToolbarSelectEntityButton.Click += ToolbarSelectEntityButton_Click;
            // 
            // ToolbarSelectEntityExtensionButton
            // 
            ToolbarSelectEntityExtensionButton.Name = "ToolbarSelectEntityExtensionButton";
            ToolbarSelectEntityExtensionButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectEntityExtensionButton.Text = "Entity Extension";
            ToolbarSelectEntityExtensionButton.Click += ToolbarSelectEntityExtensionButton_Click;
            // 
            // ToolbarSelectArchetypeExtensionButton
            // 
            ToolbarSelectArchetypeExtensionButton.Name = "ToolbarSelectArchetypeExtensionButton";
            ToolbarSelectArchetypeExtensionButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectArchetypeExtensionButton.Text = "Archetype Extension";
            ToolbarSelectArchetypeExtensionButton.Click += ToolbarSelectArchetypeExtensionButton_Click;
            // 
            // ToolbarSelectTimeCycleModifierButton
            // 
            ToolbarSelectTimeCycleModifierButton.Name = "ToolbarSelectTimeCycleModifierButton";
            ToolbarSelectTimeCycleModifierButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectTimeCycleModifierButton.Text = "Time Cycle Modifier";
            ToolbarSelectTimeCycleModifierButton.Click += ToolbarSelectTimeCycleModifierButton_Click;
            // 
            // ToolbarSelectCarGeneratorButton
            // 
            ToolbarSelectCarGeneratorButton.Name = "ToolbarSelectCarGeneratorButton";
            ToolbarSelectCarGeneratorButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectCarGeneratorButton.Text = "Car Generator";
            ToolbarSelectCarGeneratorButton.Click += ToolbarSelectCarGeneratorButton_Click;
            // 
            // ToolbarSelectGrassButton
            // 
            ToolbarSelectGrassButton.Name = "ToolbarSelectGrassButton";
            ToolbarSelectGrassButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectGrassButton.Text = "Grass";
            ToolbarSelectGrassButton.Click += ToolbarSelectGrassButton_Click;
            // 
            // ToolbarSelectWaterQuadButton
            // 
            ToolbarSelectWaterQuadButton.Name = "ToolbarSelectWaterQuadButton";
            ToolbarSelectWaterQuadButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectWaterQuadButton.Text = "Water Quad";
            ToolbarSelectWaterQuadButton.Click += ToolbarSelectWaterQuadButton_Click;
            // 
            // ToolbarSelectCalmingQuadButton
            // 
            ToolbarSelectCalmingQuadButton.Name = "ToolbarSelectCalmingQuadButton";
            ToolbarSelectCalmingQuadButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectCalmingQuadButton.Text = "Water Calming Quad";
            ToolbarSelectCalmingQuadButton.Click += ToolbarSelectCalmingQuadButton_Click;
            // 
            // ToolbarSelectWaveQuadButton
            // 
            ToolbarSelectWaveQuadButton.Name = "ToolbarSelectWaveQuadButton";
            ToolbarSelectWaveQuadButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectWaveQuadButton.Text = "Water Wave Quad";
            ToolbarSelectWaveQuadButton.Click += ToolbarSelectWaveQuadButton_Click;
            // 
            // ToolbarSelectCollisionButton
            // 
            ToolbarSelectCollisionButton.Name = "ToolbarSelectCollisionButton";
            ToolbarSelectCollisionButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectCollisionButton.Text = "Collision";
            ToolbarSelectCollisionButton.Click += ToolbarSelectCollisionButton_Click;
            // 
            // ToolbarSelectNavMeshButton
            // 
            ToolbarSelectNavMeshButton.Name = "ToolbarSelectNavMeshButton";
            ToolbarSelectNavMeshButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectNavMeshButton.Text = "Nav Mesh";
            ToolbarSelectNavMeshButton.Click += ToolbarSelectNavMeshButton_Click;
            // 
            // ToolbarSelectPathButton
            // 
            ToolbarSelectPathButton.Name = "ToolbarSelectPathButton";
            ToolbarSelectPathButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectPathButton.Text = "Traffic Path";
            ToolbarSelectPathButton.Click += ToolbarSelectPathButton_Click;
            // 
            // ToolbarSelectTrainTrackButton
            // 
            ToolbarSelectTrainTrackButton.Name = "ToolbarSelectTrainTrackButton";
            ToolbarSelectTrainTrackButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectTrainTrackButton.Text = "Train Track";
            ToolbarSelectTrainTrackButton.Click += ToolbarSelectTrainTrackButton_Click;
            // 
            // ToolbarSelectLodLightsButton
            // 
            ToolbarSelectLodLightsButton.Name = "ToolbarSelectLodLightsButton";
            ToolbarSelectLodLightsButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectLodLightsButton.Text = "Lod Lights";
            ToolbarSelectLodLightsButton.Click += ToolbarSelectLodLightsButton_Click;
            // 
            // ToolbarSelectMloInstanceButton
            // 
            ToolbarSelectMloInstanceButton.Name = "ToolbarSelectMloInstanceButton";
            ToolbarSelectMloInstanceButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectMloInstanceButton.Text = "Interior Instance";
            ToolbarSelectMloInstanceButton.Click += ToolbarSelectMloInstanceButton_Click;
            // 
            // ToolbarSelectScenarioButton
            // 
            ToolbarSelectScenarioButton.Name = "ToolbarSelectScenarioButton";
            ToolbarSelectScenarioButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectScenarioButton.Text = "Scenario";
            ToolbarSelectScenarioButton.Click += ToolbarSelectScenarioButton_Click;
            // 
            // ToolbarSelectAudioButton
            // 
            ToolbarSelectAudioButton.Name = "ToolbarSelectAudioButton";
            ToolbarSelectAudioButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectAudioButton.Text = "Audio";
            ToolbarSelectAudioButton.Click += ToolbarSelectAudioButton_Click;
            // 
            // ToolbarSelectOcclusionButton
            // 
            ToolbarSelectOcclusionButton.Name = "ToolbarSelectOcclusionButton";
            ToolbarSelectOcclusionButton.Size = new System.Drawing.Size(185, 22);
            ToolbarSelectOcclusionButton.Text = "Occlusion";
            ToolbarSelectOcclusionButton.Click += ToolbarSelectOcclusionButton_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarMoveButton
            // 
            ToolbarMoveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarMoveButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarMoveButton.Image");
            ToolbarMoveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarMoveButton.Name = "ToolbarMoveButton";
            ToolbarMoveButton.Size = new System.Drawing.Size(23, 22);
            ToolbarMoveButton.Text = "Move";
            ToolbarMoveButton.ToolTipText = "Move (W)";
            ToolbarMoveButton.Click += ToolbarMoveButton_Click;
            // 
            // ToolbarRotateButton
            // 
            ToolbarRotateButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarRotateButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarRotateButton.Image");
            ToolbarRotateButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarRotateButton.Name = "ToolbarRotateButton";
            ToolbarRotateButton.Size = new System.Drawing.Size(23, 22);
            ToolbarRotateButton.Text = "Rotate";
            ToolbarRotateButton.ToolTipText = "Rotate (E)";
            ToolbarRotateButton.Click += ToolbarRotateButton_Click;
            // 
            // ToolbarScaleButton
            // 
            ToolbarScaleButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarScaleButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarScaleButton.Image");
            ToolbarScaleButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarScaleButton.Name = "ToolbarScaleButton";
            ToolbarScaleButton.Size = new System.Drawing.Size(23, 22);
            ToolbarScaleButton.Text = "Scale";
            ToolbarScaleButton.ToolTipText = "Scale (R)";
            ToolbarScaleButton.Click += ToolbarScaleButton_Click;
            // 
            // ToolbarTransformSpaceButton
            // 
            ToolbarTransformSpaceButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarTransformSpaceButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolbarObjectSpaceButton, ToolbarWorldSpaceButton });
            ToolbarTransformSpaceButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarTransformSpaceButton.Image");
            ToolbarTransformSpaceButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarTransformSpaceButton.Name = "ToolbarTransformSpaceButton";
            ToolbarTransformSpaceButton.Size = new System.Drawing.Size(32, 22);
            ToolbarTransformSpaceButton.Text = "Toggle transform space";
            ToolbarTransformSpaceButton.ButtonClick += ToolbarTransformSpaceButton_ButtonClick;
            // 
            // ToolbarObjectSpaceButton
            // 
            ToolbarObjectSpaceButton.Checked = true;
            ToolbarObjectSpaceButton.CheckState = System.Windows.Forms.CheckState.Checked;
            ToolbarObjectSpaceButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarObjectSpaceButton.Image");
            ToolbarObjectSpaceButton.Name = "ToolbarObjectSpaceButton";
            ToolbarObjectSpaceButton.Size = new System.Drawing.Size(142, 22);
            ToolbarObjectSpaceButton.Text = "Object space";
            ToolbarObjectSpaceButton.Click += ToolbarObjectSpaceButton_Click;
            // 
            // ToolbarWorldSpaceButton
            // 
            ToolbarWorldSpaceButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarWorldSpaceButton.Image");
            ToolbarWorldSpaceButton.Name = "ToolbarWorldSpaceButton";
            ToolbarWorldSpaceButton.Size = new System.Drawing.Size(142, 22);
            ToolbarWorldSpaceButton.Text = "World space";
            ToolbarWorldSpaceButton.Click += ToolbarWorldSpaceButton_Click;
            // 
            // ToolbarSnapButton
            // 
            ToolbarSnapButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarSnapButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolbarSnapToGroundButton, ToolbarSnapToGridButton, ToolbarSnapToGroundGridButton, ToolbarSnapGridSizeButton, ToolbarRotationSnappingButton });
            ToolbarSnapButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarSnapButton.Image");
            ToolbarSnapButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarSnapButton.Name = "ToolbarSnapButton";
            ToolbarSnapButton.Size = new System.Drawing.Size(32, 22);
            ToolbarSnapButton.Text = "Snap to Ground";
            ToolbarSnapButton.ToolTipText = "Snap to Ground";
            ToolbarSnapButton.ButtonClick += ToolbarSnapButton_ButtonClick;
            // 
            // ToolbarSnapToGroundButton
            // 
            ToolbarSnapToGroundButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarSnapToGroundButton.Image");
            ToolbarSnapToGroundButton.Name = "ToolbarSnapToGroundButton";
            ToolbarSnapToGroundButton.Size = new System.Drawing.Size(205, 22);
            ToolbarSnapToGroundButton.Text = "Snap to Ground";
            ToolbarSnapToGroundButton.Click += ToolbarSnapToGroundButton_Click;
            // 
            // ToolbarSnapToGridButton
            // 
            ToolbarSnapToGridButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarSnapToGridButton.Image");
            ToolbarSnapToGridButton.Name = "ToolbarSnapToGridButton";
            ToolbarSnapToGridButton.Size = new System.Drawing.Size(205, 22);
            ToolbarSnapToGridButton.Text = "Snap to Grid";
            ToolbarSnapToGridButton.Click += ToolbarSnapToGridButton_Click;
            // 
            // ToolbarSnapToGroundGridButton
            // 
            ToolbarSnapToGroundGridButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarSnapToGroundGridButton.Image");
            ToolbarSnapToGroundGridButton.Name = "ToolbarSnapToGroundGridButton";
            ToolbarSnapToGroundGridButton.Size = new System.Drawing.Size(205, 22);
            ToolbarSnapToGroundGridButton.Text = "Snap to Grid and Ground";
            ToolbarSnapToGroundGridButton.Click += ToolbarSnapToGroundGridButton_Click;
            // 
            // ToolbarSnapGridSizeButton
            // 
            ToolbarSnapGridSizeButton.Name = "ToolbarSnapGridSizeButton";
            ToolbarSnapGridSizeButton.Size = new System.Drawing.Size(205, 22);
            ToolbarSnapGridSizeButton.Text = "Grid Size...";
            ToolbarSnapGridSizeButton.Click += ToolbarSnapGridSizeButton_Click;
            // 
            // ToolbarRotationSnappingButton
            // 
            ToolbarRotationSnappingButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolbarRotationSnappingOffButton, ToolbarRotationSnapping1Button, ToolbarRotationSnapping2Button, ToolbarRotationSnapping5Button, ToolbarRotationSnapping10Button, ToolbarRotationSnapping45Button, ToolbarRotationSnapping90Button, ToolbarRotationSnappingCustomButton });
            ToolbarRotationSnappingButton.Name = "ToolbarRotationSnappingButton";
            ToolbarRotationSnappingButton.Size = new System.Drawing.Size(205, 22);
            ToolbarRotationSnappingButton.Text = "Rotation Snapping";
            // 
            // ToolbarRotationSnappingOffButton
            // 
            ToolbarRotationSnappingOffButton.Name = "ToolbarRotationSnappingOffButton";
            ToolbarRotationSnappingOffButton.Size = new System.Drawing.Size(131, 22);
            ToolbarRotationSnappingOffButton.Text = "Off";
            ToolbarRotationSnappingOffButton.Click += ToolbarRotationSnappingOffButton_Click;
            // 
            // ToolbarRotationSnapping1Button
            // 
            ToolbarRotationSnapping1Button.Name = "ToolbarRotationSnapping1Button";
            ToolbarRotationSnapping1Button.Size = new System.Drawing.Size(131, 22);
            ToolbarRotationSnapping1Button.Text = "1 Degree";
            ToolbarRotationSnapping1Button.Click += ToolbarRotationSnapping1Button_Click;
            // 
            // ToolbarRotationSnapping2Button
            // 
            ToolbarRotationSnapping2Button.Name = "ToolbarRotationSnapping2Button";
            ToolbarRotationSnapping2Button.Size = new System.Drawing.Size(131, 22);
            ToolbarRotationSnapping2Button.Text = "2 Degrees";
            ToolbarRotationSnapping2Button.Click += ToolbarRotationSnapping2Button_Click;
            // 
            // ToolbarRotationSnapping5Button
            // 
            ToolbarRotationSnapping5Button.Checked = true;
            ToolbarRotationSnapping5Button.CheckState = System.Windows.Forms.CheckState.Checked;
            ToolbarRotationSnapping5Button.Name = "ToolbarRotationSnapping5Button";
            ToolbarRotationSnapping5Button.Size = new System.Drawing.Size(131, 22);
            ToolbarRotationSnapping5Button.Text = "5 Degrees";
            ToolbarRotationSnapping5Button.Click += ToolbarRotationSnapping5Button_Click;
            // 
            // ToolbarRotationSnapping10Button
            // 
            ToolbarRotationSnapping10Button.Name = "ToolbarRotationSnapping10Button";
            ToolbarRotationSnapping10Button.Size = new System.Drawing.Size(131, 22);
            ToolbarRotationSnapping10Button.Text = "10 Degrees";
            ToolbarRotationSnapping10Button.Click += ToolbarRotationSnapping10Button_Click;
            // 
            // ToolbarRotationSnapping45Button
            // 
            ToolbarRotationSnapping45Button.Name = "ToolbarRotationSnapping45Button";
            ToolbarRotationSnapping45Button.Size = new System.Drawing.Size(131, 22);
            ToolbarRotationSnapping45Button.Text = "45 Degrees";
            ToolbarRotationSnapping45Button.Click += ToolbarRotationSnapping45Button_Click;
            // 
            // ToolbarRotationSnapping90Button
            // 
            ToolbarRotationSnapping90Button.Name = "ToolbarRotationSnapping90Button";
            ToolbarRotationSnapping90Button.Size = new System.Drawing.Size(131, 22);
            ToolbarRotationSnapping90Button.Text = "90 Degrees";
            ToolbarRotationSnapping90Button.Click += ToolbarRotationSnapping90Button_Click;
            // 
            // ToolbarRotationSnappingCustomButton
            // 
            ToolbarRotationSnappingCustomButton.Name = "ToolbarRotationSnappingCustomButton";
            ToolbarRotationSnappingCustomButton.Size = new System.Drawing.Size(131, 22);
            ToolbarRotationSnappingCustomButton.Text = "Custom...";
            ToolbarRotationSnappingCustomButton.Click += ToolbarRotationSnappingCustomButton_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarUndoButton
            // 
            ToolbarUndoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarUndoButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolbarUndoListButton });
            ToolbarUndoButton.Enabled = false;
            ToolbarUndoButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarUndoButton.Image");
            ToolbarUndoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarUndoButton.Name = "ToolbarUndoButton";
            ToolbarUndoButton.Size = new System.Drawing.Size(32, 22);
            ToolbarUndoButton.Text = "Undo";
            ToolbarUndoButton.ButtonClick += ToolbarUndoButton_ButtonClick;
            // 
            // ToolbarUndoListButton
            // 
            ToolbarUndoListButton.Name = "ToolbarUndoListButton";
            ToolbarUndoListButton.Size = new System.Drawing.Size(121, 22);
            ToolbarUndoListButton.Text = "Undo list";
            ToolbarUndoListButton.Click += ToolbarUndoListButton_Click;
            // 
            // ToolbarRedoButton
            // 
            ToolbarRedoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarRedoButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolbarRedoListButton });
            ToolbarRedoButton.Enabled = false;
            ToolbarRedoButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarRedoButton.Image");
            ToolbarRedoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarRedoButton.Name = "ToolbarRedoButton";
            ToolbarRedoButton.Size = new System.Drawing.Size(32, 22);
            ToolbarRedoButton.Text = "Redo";
            ToolbarRedoButton.ButtonClick += ToolbarRedoButton_ButtonClick;
            // 
            // ToolbarRedoListButton
            // 
            ToolbarRedoListButton.Name = "ToolbarRedoListButton";
            ToolbarRedoListButton.Size = new System.Drawing.Size(119, 22);
            ToolbarRedoListButton.Text = "Redo list";
            ToolbarRedoListButton.Click += ToolbarRedoListButton_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarInfoWindowButton
            // 
            ToolbarInfoWindowButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarInfoWindowButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarInfoWindowButton.Image");
            ToolbarInfoWindowButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarInfoWindowButton.Name = "ToolbarInfoWindowButton";
            ToolbarInfoWindowButton.Size = new System.Drawing.Size(23, 22);
            ToolbarInfoWindowButton.Text = "Selection info window";
            ToolbarInfoWindowButton.Click += ToolbarInfoWindowButton_Click;
            // 
            // ToolbarProjectWindowButton
            // 
            ToolbarProjectWindowButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarProjectWindowButton.Enabled = false;
            ToolbarProjectWindowButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarProjectWindowButton.Image");
            ToolbarProjectWindowButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarProjectWindowButton.Name = "ToolbarProjectWindowButton";
            ToolbarProjectWindowButton.Size = new System.Drawing.Size(23, 22);
            ToolbarProjectWindowButton.Text = "Project window";
            ToolbarProjectWindowButton.Click += ToolbarProjectWindowButton_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarAddItemButton
            // 
            ToolbarAddItemButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarAddItemButton.Enabled = false;
            ToolbarAddItemButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarAddItemButton.Image");
            ToolbarAddItemButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarAddItemButton.Name = "ToolbarAddItemButton";
            ToolbarAddItemButton.Size = new System.Drawing.Size(23, 22);
            ToolbarAddItemButton.Text = "Add entity";
            ToolbarAddItemButton.Click += ToolbarAddItemButton_Click;
            // 
            // ToolbarDeleteItemButton
            // 
            ToolbarDeleteItemButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarDeleteItemButton.Enabled = false;
            ToolbarDeleteItemButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarDeleteItemButton.Image");
            ToolbarDeleteItemButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarDeleteItemButton.Name = "ToolbarDeleteItemButton";
            ToolbarDeleteItemButton.Size = new System.Drawing.Size(23, 22);
            ToolbarDeleteItemButton.Text = "Delete entity";
            ToolbarDeleteItemButton.Click += ToolbarDeleteItemButton_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarCopyButton
            // 
            ToolbarCopyButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarCopyButton.Enabled = false;
            ToolbarCopyButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarCopyButton.Image");
            ToolbarCopyButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarCopyButton.Name = "ToolbarCopyButton";
            ToolbarCopyButton.Size = new System.Drawing.Size(23, 22);
            ToolbarCopyButton.Text = "Copy";
            ToolbarCopyButton.ToolTipText = "Copy (Ctrl+C)";
            ToolbarCopyButton.Click += ToolbarCopyButton_Click;
            // 
            // ToolbarPasteButton
            // 
            ToolbarPasteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarPasteButton.Enabled = false;
            ToolbarPasteButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarPasteButton.Image");
            ToolbarPasteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarPasteButton.Name = "ToolbarPasteButton";
            ToolbarPasteButton.Size = new System.Drawing.Size(23, 22);
            ToolbarPasteButton.Text = "Paste";
            ToolbarPasteButton.ToolTipText = "Paste (Ctrl+V)";
            ToolbarPasteButton.Click += ToolbarPasteButton_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolbarCameraModeButton
            // 
            ToolbarCameraModeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolbarCameraModeButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolbarCameraPerspectiveButton, ToolbarCameraMapViewButton, ToolbarCameraOrthographicButton });
            ToolbarCameraModeButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarCameraModeButton.Image");
            ToolbarCameraModeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolbarCameraModeButton.Name = "ToolbarCameraModeButton";
            ToolbarCameraModeButton.Size = new System.Drawing.Size(32, 22);
            ToolbarCameraModeButton.Text = "Camera Mode";
            ToolbarCameraModeButton.ButtonClick += ToolbarCameraModeButton_ButtonClick;
            // 
            // ToolbarCameraPerspectiveButton
            // 
            ToolbarCameraPerspectiveButton.Checked = true;
            ToolbarCameraPerspectiveButton.CheckState = System.Windows.Forms.CheckState.Checked;
            ToolbarCameraPerspectiveButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarCameraPerspectiveButton.Image");
            ToolbarCameraPerspectiveButton.Name = "ToolbarCameraPerspectiveButton";
            ToolbarCameraPerspectiveButton.Size = new System.Drawing.Size(145, 22);
            ToolbarCameraPerspectiveButton.Text = "Perspective";
            ToolbarCameraPerspectiveButton.Click += ToolbarCameraPerspectiveButton_Click;
            // 
            // ToolbarCameraMapViewButton
            // 
            ToolbarCameraMapViewButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarCameraMapViewButton.Image");
            ToolbarCameraMapViewButton.Name = "ToolbarCameraMapViewButton";
            ToolbarCameraMapViewButton.Size = new System.Drawing.Size(145, 22);
            ToolbarCameraMapViewButton.Text = "Map View";
            ToolbarCameraMapViewButton.Click += ToolbarCameraMapViewButton_Click;
            // 
            // ToolbarCameraOrthographicButton
            // 
            ToolbarCameraOrthographicButton.Image = (System.Drawing.Image)resources.GetObject("ToolbarCameraOrthographicButton.Image");
            ToolbarCameraOrthographicButton.Name = "ToolbarCameraOrthographicButton";
            ToolbarCameraOrthographicButton.Size = new System.Drawing.Size(145, 22);
            ToolbarCameraOrthographicButton.Text = "Orthographic";
            ToolbarCameraOrthographicButton.Click += ToolbarCameraOrthographicButton_Click;
            // 
            // ToolbarPanel
            // 
            ToolbarPanel.BackColor = System.Drawing.SystemColors.Control;
            ToolbarPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            ToolbarPanel.Controls.Add(Toolbar);
            ToolbarPanel.Location = new System.Drawing.Point(14, 14);
            ToolbarPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ToolbarPanel.Name = "ToolbarPanel";
            ToolbarPanel.Size = new System.Drawing.Size(650, 30);
            ToolbarPanel.TabIndex = 7;
            ToolbarPanel.Visible = false;
            // 
            // SubtitleLabel
            // 
            SubtitleLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            SubtitleLabel.AutoSize = true;
            SubtitleLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            SubtitleLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            SubtitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            SubtitleLabel.Location = new System.Drawing.Point(531, 640);
            SubtitleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            SubtitleLabel.Name = "SubtitleLabel";
            SubtitleLabel.Size = new System.Drawing.Size(83, 18);
            SubtitleLabel.TabIndex = 8;
            SubtitleLabel.Text = "Test Subtitle";
            SubtitleLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            SubtitleLabel.Visible = false;
            SubtitleLabel.SizeChanged += SubtitleLabel_SizeChanged;
            // 
            // SubtitleTimer
            // 
            SubtitleTimer.Tick += SubtitleTimer_Tick;
            // 
            // WorldForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.MidnightBlue;
            ClientSize = new System.Drawing.Size(1148, 820);
            Controls.Add(ToolbarPanel);
            Controls.Add(SelectedMarkerPanel);
            Controls.Add(ConsolePanel);
            Controls.Add(ToolsPanel);
            Controls.Add(StatusStrip);
            Controls.Add(ToolsPanelShowButton);
            Controls.Add(SubtitleLabel);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "WorldForm";
            Text = "CodeWalker";
            Deactivate += WorldForm_Deactivate;
            FormClosing += WorldForm_FormClosing;
            Load += WorldForm_Load;
            KeyDown += WorldForm_KeyDown;
            KeyUp += WorldForm_KeyUp;
            MouseDown += WorldForm_MouseDown;
            MouseMove += WorldForm_MouseMove;
            MouseUp += WorldForm_MouseUp;
            StatusStrip.ResumeLayout(false);
            StatusStrip.PerformLayout();
            ToolsPanel.ResumeLayout(false);
            ToolsTabControl.ResumeLayout(false);
            ViewTabPage.ResumeLayout(false);
            ViewTabPage.PerformLayout();
            ViewTabControl.ResumeLayout(false);
            ViewWorldTabPage.ResumeLayout(false);
            ViewWorldTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)WorldDetailDistTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)WorldLodDistTrackBar).EndInit();
            ViewYmapsTabPage.ResumeLayout(false);
            ViewYmapsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)DetailTrackBar).EndInit();
            ViewModelTabPage.ResumeLayout(false);
            ViewModelTabPage.PerformLayout();
            MarkersTabPage.ResumeLayout(false);
            MarkersTabPage.PerformLayout();
            SelectionTabPage.ResumeLayout(false);
            SelectionTabPage.PerformLayout();
            SelectionTabControl.ResumeLayout(false);
            SelectionEntityTabPage.ResumeLayout(false);
            SelectionArchetypeTabPage.ResumeLayout(false);
            SelectionDrawableTabPage.ResumeLayout(false);
            tabControl3.ResumeLayout(false);
            tabPage11.ResumeLayout(false);
            tabPage12.ResumeLayout(false);
            tabPage13.ResumeLayout(false);
            SelectionExtensionTabPage.ResumeLayout(false);
            OptionsTabPage.ResumeLayout(false);
            OptionsTabPage.PerformLayout();
            OptionsTabControl.ResumeLayout(false);
            OptionsGeneralTabPage.ResumeLayout(false);
            OptionsGeneralTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)MapViewDetailTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)CollisionMeshRangeTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)FieldOfViewTrackBar).EndInit();
            OptionsRenderTabPage.ResumeLayout(false);
            OptionsRenderTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)AntiAliasingTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)FarClipUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)NearClipUpDown).EndInit();
            OptionsHelpersTabPage.ResumeLayout(false);
            OptionsHelpersTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)SnapAngleUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)SnapGridSizeUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)BoundsRangeTrackBar).EndInit();
            OptionsLightingTabPage.ResumeLayout(false);
            OptionsLightingTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)CloudParamTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)TimeSpeedTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)TimeOfDayTrackBar).EndInit();
            ConsolePanel.ResumeLayout(false);
            ConsolePanel.PerformLayout();
            SelectedMarkerPanel.ResumeLayout(false);
            SelectedMarkerPanel.PerformLayout();
            ToolsMenu.ResumeLayout(false);
            Toolbar.ResumeLayout(false);
            Toolbar.PerformLayout();
            ToolbarPanel.ResumeLayout(false);
            ToolbarPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.Button ReloadShadersButton;
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
        private System.Windows.Forms.Button QuitButton;
        private System.Windows.Forms.Button ReloadSettingsButton;
        private System.Windows.Forms.Button SaveSettingsButton;
        private System.Windows.Forms.Button ToolsButton;
        private System.Windows.Forms.ContextMenuStrip ToolsMenu;
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
        private System.Windows.Forms.TrackBar AntiAliasingTrackBar;
        private System.Windows.Forms.Label AntiAliasingValue;
    }
}