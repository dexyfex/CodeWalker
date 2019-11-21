namespace CodeWalker.Vehicles
{
    partial class VehicleForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VehicleForm));
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MousedLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatsUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.ToolsPanel = new System.Windows.Forms.Panel();
            this.ToolsTabControl = new System.Windows.Forms.TabControl();
            this.ToolsVehicleTabPage = new System.Windows.Forms.TabPage();
            this.VehicleHighDetailCheckBox = new System.Windows.Forms.CheckBox();
            this.VehicleNameLabel = new System.Windows.Forms.Label();
            this.VehicleMakeLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.VehicleModelComboBox = new System.Windows.Forms.ComboBox();
            this.ToolsModelsTabPage = new System.Windows.Forms.TabPage();
            this.ModelsTreeView = new CodeWalker.WinForms.TreeViewFix();
            this.ToolsTexturesTabPage = new System.Windows.Forms.TabPage();
            this.TextureViewerButton = new System.Windows.Forms.Button();
            this.TexturesTreeView = new CodeWalker.WinForms.TreeViewFix();
            this.ToolsDetailsTabPage = new System.Windows.Forms.TabPage();
            this.DetailsPropertyGrid = new CodeWalker.WinForms.ReadOnlyPropertyGrid();
            this.ToolsOptionsTabPage = new System.Windows.Forms.TabPage();
            this.HDTexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.SkeletonsCheckBox = new System.Windows.Forms.CheckBox();
            this.TimeOfDayLabel = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.TimeOfDayTrackBar = new System.Windows.Forms.TrackBar();
            this.ControlLightDirCheckBox = new System.Windows.Forms.CheckBox();
            this.ShowCollisionMeshesCheckBox = new System.Windows.Forms.CheckBox();
            this.GridCheckBox = new System.Windows.Forms.CheckBox();
            this.GridCountComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.GridSizeComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.StatusBarCheckBox = new System.Windows.Forms.CheckBox();
            this.ErrorConsoleCheckBox = new System.Windows.Forms.CheckBox();
            this.HDRRenderingCheckBox = new System.Windows.Forms.CheckBox();
            this.SkydomeCheckBox = new System.Windows.Forms.CheckBox();
            this.ShadowsCheckBox = new System.Windows.Forms.CheckBox();
            this.WireframeCheckBox = new System.Windows.Forms.CheckBox();
            this.RenderModeComboBox = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.TextureSamplerComboBox = new System.Windows.Forms.ComboBox();
            this.TextureCoordsComboBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.AnisotropicFilteringCheckBox = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.ToolsPanelHideButton = new System.Windows.Forms.Button();
            this.ToolsDragPanel = new System.Windows.Forms.Panel();
            this.ToolsPanelShowButton = new System.Windows.Forms.Button();
            this.ConsolePanel = new System.Windows.Forms.Panel();
            this.ConsoleTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.ConvRoofPanel = new System.Windows.Forms.Panel();
            this.ConvRoofButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.ConvRoofDictNameLabel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.ConvRoofNameLabel = new System.Windows.Forms.Label();
            this.StatusStrip.SuspendLayout();
            this.ToolsPanel.SuspendLayout();
            this.ToolsTabControl.SuspendLayout();
            this.ToolsVehicleTabPage.SuspendLayout();
            this.ToolsModelsTabPage.SuspendLayout();
            this.ToolsTexturesTabPage.SuspendLayout();
            this.ToolsDetailsTabPage.SuspendLayout();
            this.ToolsOptionsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimeOfDayTrackBar)).BeginInit();
            this.ConsolePanel.SuspendLayout();
            this.ConvRoofPanel.SuspendLayout();
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
            this.StatusStrip.TabIndex = 2;
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
            this.StatsLabel.Name = "StatsLabel";
            this.StatsLabel.Size = new System.Drawing.Size(75, 17);
            this.StatsLabel.Text = "0 geometries";
            // 
            // StatsUpdateTimer
            // 
            this.StatsUpdateTimer.Enabled = true;
            this.StatsUpdateTimer.Interval = 500;
            this.StatsUpdateTimer.Tick += new System.EventHandler(this.StatsUpdateTimer_Tick);
            // 
            // ToolsPanel
            // 
            this.ToolsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ToolsPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ToolsPanel.Controls.Add(this.ToolsTabControl);
            this.ToolsPanel.Controls.Add(this.ToolsPanelHideButton);
            this.ToolsPanel.Controls.Add(this.ToolsDragPanel);
            this.ToolsPanel.Location = new System.Drawing.Point(12, 12);
            this.ToolsPanel.Name = "ToolsPanel";
            this.ToolsPanel.Size = new System.Drawing.Size(252, 666);
            this.ToolsPanel.TabIndex = 3;
            this.ToolsPanel.Visible = false;
            // 
            // ToolsTabControl
            // 
            this.ToolsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolsTabControl.Controls.Add(this.ToolsVehicleTabPage);
            this.ToolsTabControl.Controls.Add(this.ToolsModelsTabPage);
            this.ToolsTabControl.Controls.Add(this.ToolsTexturesTabPage);
            this.ToolsTabControl.Controls.Add(this.ToolsDetailsTabPage);
            this.ToolsTabControl.Controls.Add(this.ToolsOptionsTabPage);
            this.ToolsTabControl.Location = new System.Drawing.Point(2, 30);
            this.ToolsTabControl.Name = "ToolsTabControl";
            this.ToolsTabControl.SelectedIndex = 0;
            this.ToolsTabControl.Size = new System.Drawing.Size(247, 633);
            this.ToolsTabControl.TabIndex = 1;
            // 
            // ToolsVehicleTabPage
            // 
            this.ToolsVehicleTabPage.Controls.Add(this.ConvRoofPanel);
            this.ToolsVehicleTabPage.Controls.Add(this.VehicleHighDetailCheckBox);
            this.ToolsVehicleTabPage.Controls.Add(this.VehicleNameLabel);
            this.ToolsVehicleTabPage.Controls.Add(this.VehicleMakeLabel);
            this.ToolsVehicleTabPage.Controls.Add(this.label5);
            this.ToolsVehicleTabPage.Controls.Add(this.label4);
            this.ToolsVehicleTabPage.Controls.Add(this.label3);
            this.ToolsVehicleTabPage.Controls.Add(this.VehicleModelComboBox);
            this.ToolsVehicleTabPage.Location = new System.Drawing.Point(4, 22);
            this.ToolsVehicleTabPage.Name = "ToolsVehicleTabPage";
            this.ToolsVehicleTabPage.Size = new System.Drawing.Size(239, 607);
            this.ToolsVehicleTabPage.TabIndex = 4;
            this.ToolsVehicleTabPage.Text = "Vehicle";
            this.ToolsVehicleTabPage.UseVisualStyleBackColor = true;
            // 
            // VehicleHighDetailCheckBox
            // 
            this.VehicleHighDetailCheckBox.AutoSize = true;
            this.VehicleHighDetailCheckBox.Checked = true;
            this.VehicleHighDetailCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.VehicleHighDetailCheckBox.Location = new System.Drawing.Point(54, 90);
            this.VehicleHighDetailCheckBox.Name = "VehicleHighDetailCheckBox";
            this.VehicleHighDetailCheckBox.Size = new System.Drawing.Size(120, 17);
            this.VehicleHighDetailCheckBox.TabIndex = 6;
            this.VehicleHighDetailCheckBox.Text = "High detail fragment";
            this.VehicleHighDetailCheckBox.UseVisualStyleBackColor = true;
            this.VehicleHighDetailCheckBox.CheckedChanged += new System.EventHandler(this.VehicleHighDetailCheckBox_CheckedChanged);
            // 
            // VehicleNameLabel
            // 
            this.VehicleNameLabel.AutoSize = true;
            this.VehicleNameLabel.Location = new System.Drawing.Point(51, 63);
            this.VehicleNameLabel.Name = "VehicleNameLabel";
            this.VehicleNameLabel.Size = new System.Drawing.Size(10, 13);
            this.VehicleNameLabel.TabIndex = 5;
            this.VehicleNameLabel.Text = "-";
            // 
            // VehicleMakeLabel
            // 
            this.VehicleMakeLabel.AutoSize = true;
            this.VehicleMakeLabel.Location = new System.Drawing.Point(51, 41);
            this.VehicleMakeLabel.Name = "VehicleMakeLabel";
            this.VehicleMakeLabel.Size = new System.Drawing.Size(10, 13);
            this.VehicleMakeLabel.TabIndex = 4;
            this.VehicleMakeLabel.Text = "-";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 63);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Name:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Make:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Model:";
            // 
            // VehicleModelComboBox
            // 
            this.VehicleModelComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.VehicleModelComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.VehicleModelComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.VehicleModelComboBox.FormattingEnabled = true;
            this.VehicleModelComboBox.Location = new System.Drawing.Point(54, 8);
            this.VehicleModelComboBox.Name = "VehicleModelComboBox";
            this.VehicleModelComboBox.Size = new System.Drawing.Size(182, 21);
            this.VehicleModelComboBox.TabIndex = 0;
            this.VehicleModelComboBox.TextChanged += new System.EventHandler(this.VehicleModelComboBox_TextChanged);
            // 
            // ToolsModelsTabPage
            // 
            this.ToolsModelsTabPage.Controls.Add(this.ModelsTreeView);
            this.ToolsModelsTabPage.Location = new System.Drawing.Point(4, 22);
            this.ToolsModelsTabPage.Name = "ToolsModelsTabPage";
            this.ToolsModelsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ToolsModelsTabPage.Size = new System.Drawing.Size(239, 607);
            this.ToolsModelsTabPage.TabIndex = 0;
            this.ToolsModelsTabPage.Text = "Models";
            this.ToolsModelsTabPage.UseVisualStyleBackColor = true;
            // 
            // ModelsTreeView
            // 
            this.ModelsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModelsTreeView.CheckBoxes = true;
            this.ModelsTreeView.Location = new System.Drawing.Point(0, 3);
            this.ModelsTreeView.Name = "ModelsTreeView";
            this.ModelsTreeView.ShowRootLines = false;
            this.ModelsTreeView.Size = new System.Drawing.Size(239, 604);
            this.ModelsTreeView.TabIndex = 2;
            this.ModelsTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.ModelsTreeView_AfterCheck);
            this.ModelsTreeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ModelsTreeView_NodeMouseDoubleClick);
            this.ModelsTreeView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ModelsTreeView_KeyPress);
            // 
            // ToolsTexturesTabPage
            // 
            this.ToolsTexturesTabPage.Controls.Add(this.TextureViewerButton);
            this.ToolsTexturesTabPage.Controls.Add(this.TexturesTreeView);
            this.ToolsTexturesTabPage.Location = new System.Drawing.Point(4, 22);
            this.ToolsTexturesTabPage.Name = "ToolsTexturesTabPage";
            this.ToolsTexturesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ToolsTexturesTabPage.Size = new System.Drawing.Size(239, 607);
            this.ToolsTexturesTabPage.TabIndex = 1;
            this.ToolsTexturesTabPage.Text = "Textures";
            this.ToolsTexturesTabPage.UseVisualStyleBackColor = true;
            // 
            // TextureViewerButton
            // 
            this.TextureViewerButton.Location = new System.Drawing.Point(6, 6);
            this.TextureViewerButton.Name = "TextureViewerButton";
            this.TextureViewerButton.Size = new System.Drawing.Size(113, 23);
            this.TextureViewerButton.TabIndex = 2;
            this.TextureViewerButton.Text = "Open texture viewer";
            this.TextureViewerButton.UseVisualStyleBackColor = true;
            this.TextureViewerButton.Click += new System.EventHandler(this.TextureViewerButton_Click);
            // 
            // TexturesTreeView
            // 
            this.TexturesTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TexturesTreeView.Location = new System.Drawing.Point(0, 34);
            this.TexturesTreeView.Name = "TexturesTreeView";
            this.TexturesTreeView.ShowRootLines = false;
            this.TexturesTreeView.Size = new System.Drawing.Size(239, 573);
            this.TexturesTreeView.TabIndex = 1;
            // 
            // ToolsDetailsTabPage
            // 
            this.ToolsDetailsTabPage.Controls.Add(this.DetailsPropertyGrid);
            this.ToolsDetailsTabPage.Location = new System.Drawing.Point(4, 22);
            this.ToolsDetailsTabPage.Name = "ToolsDetailsTabPage";
            this.ToolsDetailsTabPage.Size = new System.Drawing.Size(239, 607);
            this.ToolsDetailsTabPage.TabIndex = 2;
            this.ToolsDetailsTabPage.Text = "Details";
            this.ToolsDetailsTabPage.UseVisualStyleBackColor = true;
            // 
            // DetailsPropertyGrid
            // 
            this.DetailsPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DetailsPropertyGrid.HelpVisible = false;
            this.DetailsPropertyGrid.Location = new System.Drawing.Point(0, 3);
            this.DetailsPropertyGrid.Name = "DetailsPropertyGrid";
            this.DetailsPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.DetailsPropertyGrid.ReadOnly = true;
            this.DetailsPropertyGrid.Size = new System.Drawing.Size(239, 604);
            this.DetailsPropertyGrid.TabIndex = 1;
            this.DetailsPropertyGrid.ToolbarVisible = false;
            // 
            // ToolsOptionsTabPage
            // 
            this.ToolsOptionsTabPage.Controls.Add(this.HDTexturesCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.SkeletonsCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.TimeOfDayLabel);
            this.ToolsOptionsTabPage.Controls.Add(this.label19);
            this.ToolsOptionsTabPage.Controls.Add(this.TimeOfDayTrackBar);
            this.ToolsOptionsTabPage.Controls.Add(this.ControlLightDirCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.ShowCollisionMeshesCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.GridCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.GridCountComboBox);
            this.ToolsOptionsTabPage.Controls.Add(this.label2);
            this.ToolsOptionsTabPage.Controls.Add(this.GridSizeComboBox);
            this.ToolsOptionsTabPage.Controls.Add(this.label1);
            this.ToolsOptionsTabPage.Controls.Add(this.StatusBarCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.ErrorConsoleCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.HDRRenderingCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.SkydomeCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.ShadowsCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.WireframeCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.RenderModeComboBox);
            this.ToolsOptionsTabPage.Controls.Add(this.label11);
            this.ToolsOptionsTabPage.Controls.Add(this.TextureSamplerComboBox);
            this.ToolsOptionsTabPage.Controls.Add(this.TextureCoordsComboBox);
            this.ToolsOptionsTabPage.Controls.Add(this.label10);
            this.ToolsOptionsTabPage.Controls.Add(this.AnisotropicFilteringCheckBox);
            this.ToolsOptionsTabPage.Controls.Add(this.label14);
            this.ToolsOptionsTabPage.Location = new System.Drawing.Point(4, 22);
            this.ToolsOptionsTabPage.Name = "ToolsOptionsTabPage";
            this.ToolsOptionsTabPage.Size = new System.Drawing.Size(239, 607);
            this.ToolsOptionsTabPage.TabIndex = 3;
            this.ToolsOptionsTabPage.Text = "Options";
            this.ToolsOptionsTabPage.UseVisualStyleBackColor = true;
            // 
            // HDTexturesCheckBox
            // 
            this.HDTexturesCheckBox.AutoSize = true;
            this.HDTexturesCheckBox.Checked = true;
            this.HDTexturesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.HDTexturesCheckBox.Location = new System.Drawing.Point(19, 242);
            this.HDTexturesCheckBox.Name = "HDTexturesCheckBox";
            this.HDTexturesCheckBox.Size = new System.Drawing.Size(82, 17);
            this.HDTexturesCheckBox.TabIndex = 10;
            this.HDTexturesCheckBox.Text = "HD textures";
            this.HDTexturesCheckBox.UseVisualStyleBackColor = true;
            this.HDTexturesCheckBox.CheckedChanged += new System.EventHandler(this.HDTexturesCheckBox_CheckedChanged);
            // 
            // SkeletonsCheckBox
            // 
            this.SkeletonsCheckBox.AutoSize = true;
            this.SkeletonsCheckBox.Location = new System.Drawing.Point(19, 444);
            this.SkeletonsCheckBox.Name = "SkeletonsCheckBox";
            this.SkeletonsCheckBox.Size = new System.Drawing.Size(103, 17);
            this.SkeletonsCheckBox.TabIndex = 22;
            this.SkeletonsCheckBox.Text = "Show Skeletons";
            this.SkeletonsCheckBox.UseVisualStyleBackColor = true;
            this.SkeletonsCheckBox.CheckedChanged += new System.EventHandler(this.SkeletonsCheckBox_CheckedChanged);
            // 
            // TimeOfDayLabel
            // 
            this.TimeOfDayLabel.AutoSize = true;
            this.TimeOfDayLabel.Location = new System.Drawing.Point(78, 109);
            this.TimeOfDayLabel.Name = "TimeOfDayLabel";
            this.TimeOfDayLabel.Size = new System.Drawing.Size(34, 13);
            this.TimeOfDayLabel.TabIndex = 5;
            this.TimeOfDayLabel.Text = "12:00";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(7, 109);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(65, 13);
            this.label19.TabIndex = 4;
            this.label19.Text = "Time of day:";
            // 
            // TimeOfDayTrackBar
            // 
            this.TimeOfDayTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TimeOfDayTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.TimeOfDayTrackBar.LargeChange = 60;
            this.TimeOfDayTrackBar.Location = new System.Drawing.Point(9, 125);
            this.TimeOfDayTrackBar.Maximum = 1440;
            this.TimeOfDayTrackBar.Name = "TimeOfDayTrackBar";
            this.TimeOfDayTrackBar.Size = new System.Drawing.Size(222, 45);
            this.TimeOfDayTrackBar.TabIndex = 6;
            this.TimeOfDayTrackBar.TickFrequency = 60;
            this.TimeOfDayTrackBar.Value = 720;
            this.TimeOfDayTrackBar.Scroll += new System.EventHandler(this.TimeOfDayTrackBar_Scroll);
            // 
            // ControlLightDirCheckBox
            // 
            this.ControlLightDirCheckBox.AutoSize = true;
            this.ControlLightDirCheckBox.Location = new System.Drawing.Point(19, 83);
            this.ControlLightDirCheckBox.Name = "ControlLightDirCheckBox";
            this.ControlLightDirCheckBox.Size = new System.Drawing.Size(124, 17);
            this.ControlLightDirCheckBox.TabIndex = 3;
            this.ControlLightDirCheckBox.Text = "Control light direction";
            this.ControlLightDirCheckBox.UseVisualStyleBackColor = true;
            this.ControlLightDirCheckBox.CheckedChanged += new System.EventHandler(this.ControlLightDirCheckBox_CheckedChanged);
            // 
            // ShowCollisionMeshesCheckBox
            // 
            this.ShowCollisionMeshesCheckBox.AutoSize = true;
            this.ShowCollisionMeshesCheckBox.Location = new System.Drawing.Point(19, 173);
            this.ShowCollisionMeshesCheckBox.Name = "ShowCollisionMeshesCheckBox";
            this.ShowCollisionMeshesCheckBox.Size = new System.Drawing.Size(132, 17);
            this.ShowCollisionMeshesCheckBox.TabIndex = 7;
            this.ShowCollisionMeshesCheckBox.Text = "Show collision meshes";
            this.ShowCollisionMeshesCheckBox.UseVisualStyleBackColor = true;
            this.ShowCollisionMeshesCheckBox.CheckedChanged += new System.EventHandler(this.ShowCollisionMeshesCheckBox_CheckedChanged);
            // 
            // GridCheckBox
            // 
            this.GridCheckBox.AutoSize = true;
            this.GridCheckBox.Location = new System.Drawing.Point(19, 364);
            this.GridCheckBox.Name = "GridCheckBox";
            this.GridCheckBox.Size = new System.Drawing.Size(45, 17);
            this.GridCheckBox.TabIndex = 17;
            this.GridCheckBox.Text = "Grid";
            this.GridCheckBox.UseVisualStyleBackColor = true;
            this.GridCheckBox.CheckedChanged += new System.EventHandler(this.GridCheckBox_CheckedChanged);
            // 
            // GridCountComboBox
            // 
            this.GridCountComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.GridCountComboBox.FormattingEnabled = true;
            this.GridCountComboBox.Items.AddRange(new object[] {
            "20",
            "40",
            "60",
            "100"});
            this.GridCountComboBox.Location = new System.Drawing.Point(83, 411);
            this.GridCountComboBox.Name = "GridCountComboBox";
            this.GridCountComboBox.Size = new System.Drawing.Size(114, 21);
            this.GridCountComboBox.TabIndex = 21;
            this.GridCountComboBox.SelectedIndexChanged += new System.EventHandler(this.GridCountComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 414);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Grid count:";
            // 
            // GridSizeComboBox
            // 
            this.GridSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.GridSizeComboBox.FormattingEnabled = true;
            this.GridSizeComboBox.Items.AddRange(new object[] {
            "0.1",
            "1.0",
            "10",
            "100"});
            this.GridSizeComboBox.Location = new System.Drawing.Point(83, 384);
            this.GridSizeComboBox.Name = "GridSizeComboBox";
            this.GridSizeComboBox.Size = new System.Drawing.Size(114, 21);
            this.GridSizeComboBox.TabIndex = 19;
            this.GridSizeComboBox.SelectedIndexChanged += new System.EventHandler(this.GridSizeComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 387);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Grid unit size:";
            // 
            // StatusBarCheckBox
            // 
            this.StatusBarCheckBox.AutoSize = true;
            this.StatusBarCheckBox.Checked = true;
            this.StatusBarCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.StatusBarCheckBox.Location = new System.Drawing.Point(19, 480);
            this.StatusBarCheckBox.Name = "StatusBarCheckBox";
            this.StatusBarCheckBox.Size = new System.Drawing.Size(74, 17);
            this.StatusBarCheckBox.TabIndex = 23;
            this.StatusBarCheckBox.Text = "Status bar";
            this.StatusBarCheckBox.UseVisualStyleBackColor = true;
            this.StatusBarCheckBox.CheckedChanged += new System.EventHandler(this.StatusBarCheckBox_CheckedChanged);
            // 
            // ErrorConsoleCheckBox
            // 
            this.ErrorConsoleCheckBox.AutoSize = true;
            this.ErrorConsoleCheckBox.Location = new System.Drawing.Point(105, 480);
            this.ErrorConsoleCheckBox.Name = "ErrorConsoleCheckBox";
            this.ErrorConsoleCheckBox.Size = new System.Drawing.Size(88, 17);
            this.ErrorConsoleCheckBox.TabIndex = 24;
            this.ErrorConsoleCheckBox.Text = "Error console";
            this.ErrorConsoleCheckBox.UseVisualStyleBackColor = true;
            this.ErrorConsoleCheckBox.CheckedChanged += new System.EventHandler(this.ErrorConsoleCheckBox_CheckedChanged);
            // 
            // HDRRenderingCheckBox
            // 
            this.HDRRenderingCheckBox.AutoSize = true;
            this.HDRRenderingCheckBox.Checked = true;
            this.HDRRenderingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.HDRRenderingCheckBox.Location = new System.Drawing.Point(19, 14);
            this.HDRRenderingCheckBox.Name = "HDRRenderingCheckBox";
            this.HDRRenderingCheckBox.Size = new System.Drawing.Size(97, 17);
            this.HDRRenderingCheckBox.TabIndex = 0;
            this.HDRRenderingCheckBox.Text = "HDR rendering";
            this.HDRRenderingCheckBox.UseVisualStyleBackColor = true;
            this.HDRRenderingCheckBox.CheckedChanged += new System.EventHandler(this.HDRRenderingCheckBox_CheckedChanged);
            // 
            // SkydomeCheckBox
            // 
            this.SkydomeCheckBox.AutoSize = true;
            this.SkydomeCheckBox.Location = new System.Drawing.Point(19, 60);
            this.SkydomeCheckBox.Name = "SkydomeCheckBox";
            this.SkydomeCheckBox.Size = new System.Drawing.Size(70, 17);
            this.SkydomeCheckBox.TabIndex = 2;
            this.SkydomeCheckBox.Text = "Skydome";
            this.SkydomeCheckBox.UseVisualStyleBackColor = true;
            this.SkydomeCheckBox.CheckedChanged += new System.EventHandler(this.SkydomeCheckBox_CheckedChanged);
            // 
            // ShadowsCheckBox
            // 
            this.ShadowsCheckBox.AutoSize = true;
            this.ShadowsCheckBox.Checked = true;
            this.ShadowsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShadowsCheckBox.Location = new System.Drawing.Point(19, 37);
            this.ShadowsCheckBox.Name = "ShadowsCheckBox";
            this.ShadowsCheckBox.Size = new System.Drawing.Size(70, 17);
            this.ShadowsCheckBox.TabIndex = 1;
            this.ShadowsCheckBox.Text = "Shadows";
            this.ShadowsCheckBox.UseVisualStyleBackColor = true;
            this.ShadowsCheckBox.CheckedChanged += new System.EventHandler(this.ShadowsCheckBox_CheckedChanged);
            // 
            // WireframeCheckBox
            // 
            this.WireframeCheckBox.AutoSize = true;
            this.WireframeCheckBox.Location = new System.Drawing.Point(19, 196);
            this.WireframeCheckBox.Name = "WireframeCheckBox";
            this.WireframeCheckBox.Size = new System.Drawing.Size(74, 17);
            this.WireframeCheckBox.TabIndex = 8;
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
            this.RenderModeComboBox.Location = new System.Drawing.Point(83, 274);
            this.RenderModeComboBox.Name = "RenderModeComboBox";
            this.RenderModeComboBox.Size = new System.Drawing.Size(114, 21);
            this.RenderModeComboBox.TabIndex = 12;
            this.RenderModeComboBox.SelectedIndexChanged += new System.EventHandler(this.RenderModeComboBox_SelectedIndexChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 304);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(67, 13);
            this.label11.TabIndex = 13;
            this.label11.Text = "Tex sampler:";
            // 
            // TextureSamplerComboBox
            // 
            this.TextureSamplerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TextureSamplerComboBox.Enabled = false;
            this.TextureSamplerComboBox.FormattingEnabled = true;
            this.TextureSamplerComboBox.Location = new System.Drawing.Point(83, 301);
            this.TextureSamplerComboBox.Name = "TextureSamplerComboBox";
            this.TextureSamplerComboBox.Size = new System.Drawing.Size(114, 21);
            this.TextureSamplerComboBox.TabIndex = 14;
            this.TextureSamplerComboBox.SelectedIndexChanged += new System.EventHandler(this.TextureSamplerComboBox_SelectedIndexChanged);
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
            this.TextureCoordsComboBox.Location = new System.Drawing.Point(83, 328);
            this.TextureCoordsComboBox.Name = "TextureCoordsComboBox";
            this.TextureCoordsComboBox.Size = new System.Drawing.Size(114, 21);
            this.TextureCoordsComboBox.TabIndex = 16;
            this.TextureCoordsComboBox.SelectedIndexChanged += new System.EventHandler(this.TextureCoordsComboBox_SelectedIndexChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 277);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(74, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "Render mode:";
            // 
            // AnisotropicFilteringCheckBox
            // 
            this.AnisotropicFilteringCheckBox.AutoSize = true;
            this.AnisotropicFilteringCheckBox.Checked = true;
            this.AnisotropicFilteringCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AnisotropicFilteringCheckBox.Location = new System.Drawing.Point(19, 219);
            this.AnisotropicFilteringCheckBox.Name = "AnisotropicFilteringCheckBox";
            this.AnisotropicFilteringCheckBox.Size = new System.Drawing.Size(114, 17);
            this.AnisotropicFilteringCheckBox.TabIndex = 9;
            this.AnisotropicFilteringCheckBox.Text = "Anisotropic filtering";
            this.AnisotropicFilteringCheckBox.UseVisualStyleBackColor = true;
            this.AnisotropicFilteringCheckBox.CheckedChanged += new System.EventHandler(this.AnisotropicFilteringCheckBox_CheckedChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(7, 331);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(63, 13);
            this.label14.TabIndex = 15;
            this.label14.Text = "Tex coords:";
            // 
            // ToolsPanelHideButton
            // 
            this.ToolsPanelHideButton.Location = new System.Drawing.Point(3, 3);
            this.ToolsPanelHideButton.Name = "ToolsPanelHideButton";
            this.ToolsPanelHideButton.Size = new System.Drawing.Size(30, 23);
            this.ToolsPanelHideButton.TabIndex = 0;
            this.ToolsPanelHideButton.Text = "<<";
            this.ToolsPanelHideButton.UseVisualStyleBackColor = true;
            this.ToolsPanelHideButton.Click += new System.EventHandler(this.ToolsPanelHideButton_Click);
            // 
            // ToolsDragPanel
            // 
            this.ToolsDragPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolsDragPanel.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.ToolsDragPanel.Location = new System.Drawing.Point(249, 0);
            this.ToolsDragPanel.Name = "ToolsDragPanel";
            this.ToolsDragPanel.Size = new System.Drawing.Size(4, 666);
            this.ToolsDragPanel.TabIndex = 17;
            this.ToolsDragPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ToolsDragPanel_MouseDown);
            this.ToolsDragPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ToolsDragPanel_MouseMove);
            this.ToolsDragPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolsDragPanel_MouseUp);
            // 
            // ToolsPanelShowButton
            // 
            this.ToolsPanelShowButton.Location = new System.Drawing.Point(15, 15);
            this.ToolsPanelShowButton.Name = "ToolsPanelShowButton";
            this.ToolsPanelShowButton.Size = new System.Drawing.Size(30, 23);
            this.ToolsPanelShowButton.TabIndex = 4;
            this.ToolsPanelShowButton.Text = ">>";
            this.ToolsPanelShowButton.UseVisualStyleBackColor = true;
            this.ToolsPanelShowButton.Click += new System.EventHandler(this.ToolsPanelShowButton_Click);
            // 
            // ConsolePanel
            // 
            this.ConsolePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConsolePanel.BackColor = System.Drawing.SystemColors.Control;
            this.ConsolePanel.Controls.Add(this.ConsoleTextBox);
            this.ConsolePanel.Location = new System.Drawing.Point(271, 577);
            this.ConsolePanel.Name = "ConsolePanel";
            this.ConsolePanel.Size = new System.Drawing.Size(701, 101);
            this.ConsolePanel.TabIndex = 5;
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
            this.ConsoleTextBox.Size = new System.Drawing.Size(695, 95);
            this.ConsoleTextBox.TabIndex = 0;
            // 
            // ConvRoofPanel
            // 
            this.ConvRoofPanel.Controls.Add(this.ConvRoofNameLabel);
            this.ConvRoofPanel.Controls.Add(this.label8);
            this.ConvRoofPanel.Controls.Add(this.ConvRoofDictNameLabel);
            this.ConvRoofPanel.Controls.Add(this.label7);
            this.ConvRoofPanel.Controls.Add(this.label6);
            this.ConvRoofPanel.Controls.Add(this.ConvRoofButton);
            this.ConvRoofPanel.Location = new System.Drawing.Point(3, 164);
            this.ConvRoofPanel.Name = "ConvRoofPanel";
            this.ConvRoofPanel.Size = new System.Drawing.Size(233, 89);
            this.ConvRoofPanel.TabIndex = 7;
            this.ConvRoofPanel.Visible = false;
            // 
            // ConvRoofButton
            // 
            this.ConvRoofButton.Location = new System.Drawing.Point(13, 63);
            this.ConvRoofButton.Name = "ConvRoofButton";
            this.ConvRoofButton.Size = new System.Drawing.Size(75, 23);
            this.ConvRoofButton.TabIndex = 0;
            this.ConvRoofButton.Text = "Play";
            this.ConvRoofButton.UseVisualStyleBackColor = true;
            this.ConvRoofButton.Click += new System.EventHandler(this.ConvRoofButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 4);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(86, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Convertible Roof";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Clip dict:";
            // 
            // ConvRoofDictNameLabel
            // 
            this.ConvRoofDictNameLabel.AutoSize = true;
            this.ConvRoofDictNameLabel.Location = new System.Drawing.Point(60, 21);
            this.ConvRoofDictNameLabel.Name = "ConvRoofDictNameLabel";
            this.ConvRoofDictNameLabel.Size = new System.Drawing.Size(124, 13);
            this.ConvRoofDictNameLabel.TabIndex = 3;
            this.ConvRoofDictNameLabel.Text = "animConvRoofDictName";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 41);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(27, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Clip:";
            // 
            // ConvRoofNameLabel
            // 
            this.ConvRoofNameLabel.AutoSize = true;
            this.ConvRoofNameLabel.Location = new System.Drawing.Point(40, 41);
            this.ConvRoofNameLabel.Name = "ConvRoofNameLabel";
            this.ConvRoofNameLabel.Size = new System.Drawing.Size(105, 13);
            this.ConvRoofNameLabel.TabIndex = 5;
            this.ConvRoofNameLabel.Text = "animConvRoofName";
            // 
            // VehicleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MidnightBlue;
            this.ClientSize = new System.Drawing.Size(984, 711);
            this.Controls.Add(this.ConsolePanel);
            this.Controls.Add(this.ToolsPanel);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.ToolsPanelShowButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "VehicleForm";
            this.Text = "Vehicle Viewer - CodeWalker by dexyfex";
            this.Deactivate += new System.EventHandler(this.VehicleForm_Deactivate);
            this.Load += new System.EventHandler(this.VehicleForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VehicleForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VehicleForm_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.VehicleForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.VehicleForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.VehicleForm_MouseUp);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ToolsPanel.ResumeLayout(false);
            this.ToolsTabControl.ResumeLayout(false);
            this.ToolsVehicleTabPage.ResumeLayout(false);
            this.ToolsVehicleTabPage.PerformLayout();
            this.ToolsModelsTabPage.ResumeLayout(false);
            this.ToolsTexturesTabPage.ResumeLayout(false);
            this.ToolsDetailsTabPage.ResumeLayout(false);
            this.ToolsOptionsTabPage.ResumeLayout(false);
            this.ToolsOptionsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimeOfDayTrackBar)).EndInit();
            this.ConsolePanel.ResumeLayout(false);
            this.ConsolePanel.PerformLayout();
            this.ConvRoofPanel.ResumeLayout(false);
            this.ConvRoofPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel MousedLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatsLabel;
        private System.Windows.Forms.Timer StatsUpdateTimer;
        private System.Windows.Forms.Panel ToolsPanel;
        private System.Windows.Forms.TabControl ToolsTabControl;
        private System.Windows.Forms.TabPage ToolsModelsTabPage;
        private System.Windows.Forms.TabPage ToolsTexturesTabPage;
        private System.Windows.Forms.Button TextureViewerButton;
        private WinForms.TreeViewFix TexturesTreeView;
        private System.Windows.Forms.TabPage ToolsDetailsTabPage;
        private WinForms.ReadOnlyPropertyGrid DetailsPropertyGrid;
        private System.Windows.Forms.TabPage ToolsOptionsTabPage;
        private System.Windows.Forms.CheckBox HDTexturesCheckBox;
        private System.Windows.Forms.CheckBox SkeletonsCheckBox;
        private System.Windows.Forms.Label TimeOfDayLabel;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TrackBar TimeOfDayTrackBar;
        private System.Windows.Forms.CheckBox ControlLightDirCheckBox;
        private System.Windows.Forms.CheckBox ShowCollisionMeshesCheckBox;
        private System.Windows.Forms.CheckBox GridCheckBox;
        private System.Windows.Forms.ComboBox GridCountComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox GridSizeComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox StatusBarCheckBox;
        private System.Windows.Forms.CheckBox ErrorConsoleCheckBox;
        private System.Windows.Forms.CheckBox HDRRenderingCheckBox;
        private System.Windows.Forms.CheckBox SkydomeCheckBox;
        private System.Windows.Forms.CheckBox ShadowsCheckBox;
        private System.Windows.Forms.CheckBox WireframeCheckBox;
        private System.Windows.Forms.ComboBox RenderModeComboBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox TextureSamplerComboBox;
        private System.Windows.Forms.ComboBox TextureCoordsComboBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox AnisotropicFilteringCheckBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button ToolsPanelHideButton;
        private System.Windows.Forms.Panel ToolsDragPanel;
        private WinForms.TreeViewFix ModelsTreeView;
        private System.Windows.Forms.Button ToolsPanelShowButton;
        private System.Windows.Forms.Panel ConsolePanel;
        private WinForms.TextBoxFix ConsoleTextBox;
        private System.Windows.Forms.TabPage ToolsVehicleTabPage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox VehicleModelComboBox;
        private System.Windows.Forms.Label VehicleNameLabel;
        private System.Windows.Forms.Label VehicleMakeLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox VehicleHighDetailCheckBox;
        private System.Windows.Forms.Panel ConvRoofPanel;
        private System.Windows.Forms.Label ConvRoofDictNameLabel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button ConvRoofButton;
        private System.Windows.Forms.Label ConvRoofNameLabel;
        private System.Windows.Forms.Label label8;
    }
}