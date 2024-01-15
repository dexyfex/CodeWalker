namespace CodeWalker.Forms
{
    partial class ModelForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelForm));
            this.StatsUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MousedLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ConsolePanel = new System.Windows.Forms.Panel();
            this.ConsoleTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.ToolsPanel = new System.Windows.Forms.Panel();
            this.MainToolbarPanel = new System.Windows.Forms.Panel();
            this.MainToolbar = new System.Windows.Forms.ToolStrip();
            this.ToolbarMaterialEditorButton = new System.Windows.Forms.ToolStripButton();
            this.ToolbarTextureEditorButton = new System.Windows.Forms.ToolStripButton();
            this.ToolbarLightEditorButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarMoveButton = new System.Windows.Forms.ToolStripButton();
            this.ToolbarRotateButton = new System.Windows.Forms.ToolStripButton();
            this.ToolbarScaleButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.SaveButton = new System.Windows.Forms.ToolStripSplitButton();
            this.SaveMenuButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveAsMenuButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.SaveAllTexturesMenuButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveSharedTexturesMenuButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsTabControl = new System.Windows.Forms.TabControl();
            this.ToolsModelsTabPage = new System.Windows.Forms.TabPage();
            this.EnableRootMotionCheckBox = new System.Windows.Forms.CheckBox();
            this.label22 = new System.Windows.Forms.Label();
            this.ClipComboBox = new System.Windows.Forms.ComboBox();
            this.label21 = new System.Windows.Forms.Label();
            this.ClipDictComboBox = new System.Windows.Forms.ComboBox();
            this.ModelsTreeView = new CodeWalker.WinForms.TreeViewFix();
            this.ToolsMaterialsTabPage = new System.Windows.Forms.TabPage();
            this.SaveSharedTexturesButton = new System.Windows.Forms.Button();
            this.TexturesTreeView = new CodeWalker.WinForms.TreeViewFix();
            this.SaveAllTexturesButton = new System.Windows.Forms.Button();
            this.ToolsDetailsTabPage = new System.Windows.Forms.TabPage();
            this.DetailsPropertyGrid = new CodeWalker.WinForms.ReadOnlyPropertyGrid();
            this.ToolsOptionsTabPage = new System.Windows.Forms.TabPage();
            this.OptionsTabControl = new System.Windows.Forms.TabControl();
            this.OptionsRenderTabPage = new System.Windows.Forms.TabPage();
            this.RenderModeComboBox = new System.Windows.Forms.ComboBox();
            this.FragGlassCheckBox = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.HDTexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.AnisotropicFilteringCheckBox = new System.Windows.Forms.CheckBox();
            this.SkeletonsCheckBox = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.TextureCoordsComboBox = new System.Windows.Forms.ComboBox();
            this.TextureSamplerComboBox = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.WireframeCheckBox = new System.Windows.Forms.CheckBox();
            this.ShowCollisionMeshesCheckBox = new System.Windows.Forms.CheckBox();
            this.GridCheckBox = new System.Windows.Forms.CheckBox();
            this.GridCountComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ErrorConsoleCheckBox = new System.Windows.Forms.CheckBox();
            this.GridSizeComboBox = new System.Windows.Forms.ComboBox();
            this.StatusBarCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OptionsLightingTabPage = new System.Windows.Forms.TabPage();
            this.DeferredShadingCheckBox = new System.Windows.Forms.CheckBox();
            this.HDLightsCheckBox = new System.Windows.Forms.CheckBox();
            this.label19 = new System.Windows.Forms.Label();
            this.HDRRenderingCheckBox = new System.Windows.Forms.CheckBox();
            this.TimeOfDayTrackBar = new System.Windows.Forms.TrackBar();
            this.ShadowsCheckBox = new System.Windows.Forms.CheckBox();
            this.ControlLightDirCheckBox = new System.Windows.Forms.CheckBox();
            this.SkydomeCheckBox = new System.Windows.Forms.CheckBox();
            this.TimeOfDayLabel = new System.Windows.Forms.Label();
            this.OptionsHelperTabPage = new System.Windows.Forms.TabPage();
            this.SnapAngleUpDown = new System.Windows.Forms.NumericUpDown();
            this.label33 = new System.Windows.Forms.Label();
            this.ToolsPanelHideButton = new System.Windows.Forms.Button();
            this.ToolsDragPanel = new System.Windows.Forms.Panel();
            this.ToolsPanelShowButton = new System.Windows.Forms.Button();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.OptionsShowOutlinesCheckBox = new System.Windows.Forms.CheckBox();
            this.StatusStrip.SuspendLayout();
            this.ConsolePanel.SuspendLayout();
            this.ToolsPanel.SuspendLayout();
            this.MainToolbarPanel.SuspendLayout();
            this.MainToolbar.SuspendLayout();
            this.ToolsTabControl.SuspendLayout();
            this.ToolsModelsTabPage.SuspendLayout();
            this.ToolsMaterialsTabPage.SuspendLayout();
            this.ToolsDetailsTabPage.SuspendLayout();
            this.ToolsOptionsTabPage.SuspendLayout();
            this.OptionsTabControl.SuspendLayout();
            this.OptionsRenderTabPage.SuspendLayout();
            this.OptionsLightingTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimeOfDayTrackBar)).BeginInit();
            this.OptionsHelperTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SnapAngleUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // StatsUpdateTimer
            // 
            this.StatsUpdateTimer.Enabled = true;
            this.StatsUpdateTimer.Interval = 500;
            this.StatsUpdateTimer.Tick += new System.EventHandler(this.StatsUpdateTimer_Tick);
            // 
            // StatusStrip
            // 
            this.StatusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel,
            this.MousedLabel,
            this.StatsLabel});
            this.StatusStrip.Location = new System.Drawing.Point(0, 717);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.StatusStrip.Size = new System.Drawing.Size(1169, 26);
            this.StatusStrip.TabIndex = 1;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.BackColor = System.Drawing.SystemColors.Control;
            this.StatusLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(1032, 20);
            this.StatusLabel.Spring = true;
            this.StatusLabel.Text = "Initialising";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MousedLabel
            // 
            this.MousedLabel.BackColor = System.Drawing.SystemColors.Control;
            this.MousedLabel.Name = "MousedLabel";
            this.MousedLabel.Size = new System.Drawing.Size(21, 20);
            this.MousedLabel.Text = "   ";
            // 
            // StatsLabel
            // 
            this.StatsLabel.BackColor = System.Drawing.SystemColors.Control;
            this.StatsLabel.Name = "StatsLabel";
            this.StatsLabel.Size = new System.Drawing.Size(96, 20);
            this.StatsLabel.Text = "0 geometries";
            // 
            // ConsolePanel
            // 
            this.ConsolePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConsolePanel.BackColor = System.Drawing.SystemColors.Control;
            this.ConsolePanel.Controls.Add(this.ConsoleTextBox);
            this.ConsolePanel.Location = new System.Drawing.Point(363, 578);
            this.ConsolePanel.Margin = new System.Windows.Forms.Padding(4);
            this.ConsolePanel.Name = "ConsolePanel";
            this.ConsolePanel.Size = new System.Drawing.Size(791, 124);
            this.ConsolePanel.TabIndex = 3;
            this.ConsolePanel.Visible = false;
            // 
            // ConsoleTextBox
            // 
            this.ConsoleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConsoleTextBox.Location = new System.Drawing.Point(4, 4);
            this.ConsoleTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.ConsoleTextBox.Multiline = true;
            this.ConsoleTextBox.Name = "ConsoleTextBox";
            this.ConsoleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ConsoleTextBox.Size = new System.Drawing.Size(781, 116);
            this.ConsoleTextBox.TabIndex = 0;
            // 
            // ToolsPanel
            // 
            this.ToolsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ToolsPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ToolsPanel.Controls.Add(this.MainToolbarPanel);
            this.ToolsPanel.Controls.Add(this.ToolsTabControl);
            this.ToolsPanel.Controls.Add(this.ToolsPanelHideButton);
            this.ToolsPanel.Controls.Add(this.ToolsDragPanel);
            this.ToolsPanel.Location = new System.Drawing.Point(16, 15);
            this.ToolsPanel.Margin = new System.Windows.Forms.Padding(4);
            this.ToolsPanel.Name = "ToolsPanel";
            this.ToolsPanel.Size = new System.Drawing.Size(339, 688);
            this.ToolsPanel.TabIndex = 2;
            this.ToolsPanel.Visible = false;
            // 
            // MainToolbarPanel
            // 
            this.MainToolbarPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainToolbarPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.MainToolbarPanel.Controls.Add(this.MainToolbar);
            this.MainToolbarPanel.Location = new System.Drawing.Point(52, 2);
            this.MainToolbarPanel.Margin = new System.Windows.Forms.Padding(4);
            this.MainToolbarPanel.Name = "MainToolbarPanel";
            this.MainToolbarPanel.Size = new System.Drawing.Size(283, 30);
            this.MainToolbarPanel.TabIndex = 4;
            // 
            // MainToolbar
            // 
            this.MainToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.MainToolbar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MainToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarMaterialEditorButton,
            this.ToolbarTextureEditorButton,
            this.ToolbarLightEditorButton,
            this.toolStripSeparator1,
            this.ToolbarMoveButton,
            this.ToolbarRotateButton,
            this.ToolbarScaleButton,
            this.toolStripSeparator2,
            this.SaveButton});
            this.MainToolbar.Location = new System.Drawing.Point(0, 0);
            this.MainToolbar.Name = "MainToolbar";
            this.MainToolbar.Size = new System.Drawing.Size(283, 27);
            this.MainToolbar.TabIndex = 7;
            this.MainToolbar.Text = "Main Toolbar";
            // 
            // ToolbarMaterialEditorButton
            // 
            this.ToolbarMaterialEditorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarMaterialEditorButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarMaterialEditorButton.Image")));
            this.ToolbarMaterialEditorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarMaterialEditorButton.Name = "ToolbarMaterialEditorButton";
            this.ToolbarMaterialEditorButton.Size = new System.Drawing.Size(29, 24);
            this.ToolbarMaterialEditorButton.Text = "Material Editor";
            this.ToolbarMaterialEditorButton.Click += new System.EventHandler(this.ToolbarMaterialEditorButton_Click);
            // 
            // ToolbarTextureEditorButton
            // 
            this.ToolbarTextureEditorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarTextureEditorButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarTextureEditorButton.Image")));
            this.ToolbarTextureEditorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarTextureEditorButton.Name = "ToolbarTextureEditorButton";
            this.ToolbarTextureEditorButton.Size = new System.Drawing.Size(29, 24);
            this.ToolbarTextureEditorButton.Text = "Texture Editor";
            this.ToolbarTextureEditorButton.Click += new System.EventHandler(this.ToolbarTextureEditorButton_Click);
            // 
            // ToolbarLightEditorButton
            // 
            this.ToolbarLightEditorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarLightEditorButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarLightEditorButton.Image")));
            this.ToolbarLightEditorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarLightEditorButton.Name = "ToolbarLightEditorButton";
            this.ToolbarLightEditorButton.Size = new System.Drawing.Size(29, 24);
            this.ToolbarLightEditorButton.Text = "Light Editor";
            this.ToolbarLightEditorButton.Click += new System.EventHandler(this.ToolbarLightEditorButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // ToolbarMoveButton
            // 
            this.ToolbarMoveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarMoveButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarMoveButton.Image")));
            this.ToolbarMoveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarMoveButton.Name = "ToolbarMoveButton";
            this.ToolbarMoveButton.Size = new System.Drawing.Size(29, 24);
            this.ToolbarMoveButton.Text = "Move";
            this.ToolbarMoveButton.ToolTipText = "Move";
            this.ToolbarMoveButton.Click += new System.EventHandler(this.ToolbarMoveButton_Click);
            // 
            // ToolbarRotateButton
            // 
            this.ToolbarRotateButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarRotateButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarRotateButton.Image")));
            this.ToolbarRotateButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarRotateButton.Name = "ToolbarRotateButton";
            this.ToolbarRotateButton.Size = new System.Drawing.Size(29, 24);
            this.ToolbarRotateButton.Text = "Rotate";
            this.ToolbarRotateButton.ToolTipText = "Rotate";
            this.ToolbarRotateButton.Click += new System.EventHandler(this.ToolbarRotateButton_Click);
            // 
            // ToolbarScaleButton
            // 
            this.ToolbarScaleButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarScaleButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarScaleButton.Image")));
            this.ToolbarScaleButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolbarScaleButton.Name = "ToolbarScaleButton";
            this.ToolbarScaleButton.Size = new System.Drawing.Size(29, 24);
            this.ToolbarScaleButton.Text = "Scale";
            this.ToolbarScaleButton.ToolTipText = "Scale";
            this.ToolbarScaleButton.Click += new System.EventHandler(this.ToolbarScaleButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // SaveButton
            // 
            this.SaveButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.SaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SaveButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SaveMenuButton,
            this.SaveAsMenuButton,
            this.toolStripSeparator3,
            this.SaveAllTexturesMenuButton,
            this.SaveSharedTexturesMenuButton});
            this.SaveButton.Image = ((System.Drawing.Image)(resources.GetObject("SaveButton.Image")));
            this.SaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(39, 24);
            this.SaveButton.Text = "Save";
            this.SaveButton.ButtonClick += new System.EventHandler(this.SaveButton_ButtonClick);
            // 
            // SaveMenuButton
            // 
            this.SaveMenuButton.Image = ((System.Drawing.Image)(resources.GetObject("SaveMenuButton.Image")));
            this.SaveMenuButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveMenuButton.Name = "SaveMenuButton";
            this.SaveMenuButton.Size = new System.Drawing.Size(240, 26);
            this.SaveMenuButton.Text = "Save";
            this.SaveMenuButton.Click += new System.EventHandler(this.SaveMenuButton_Click);
            // 
            // SaveAsMenuButton
            // 
            this.SaveAsMenuButton.Name = "SaveAsMenuButton";
            this.SaveAsMenuButton.Size = new System.Drawing.Size(240, 26);
            this.SaveAsMenuButton.Text = "Save As...";
            this.SaveAsMenuButton.Click += new System.EventHandler(this.SaveAsMenuButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(237, 6);
            // 
            // SaveAllTexturesMenuButton
            // 
            this.SaveAllTexturesMenuButton.Name = "SaveAllTexturesMenuButton";
            this.SaveAllTexturesMenuButton.Size = new System.Drawing.Size(240, 26);
            this.SaveAllTexturesMenuButton.Text = "Save All Textures...";
            this.SaveAllTexturesMenuButton.Click += new System.EventHandler(this.SaveAllTexturesMenuButton_Click);
            // 
            // SaveSharedTexturesMenuButton
            // 
            this.SaveSharedTexturesMenuButton.Name = "SaveSharedTexturesMenuButton";
            this.SaveSharedTexturesMenuButton.Size = new System.Drawing.Size(240, 26);
            this.SaveSharedTexturesMenuButton.Text = "Save Shared Textures...";
            this.SaveSharedTexturesMenuButton.ToolTipText = "Save only the shared textures for this model (non-embedded)";
            this.SaveSharedTexturesMenuButton.Click += new System.EventHandler(this.SaveSharedTexturesMenuButton_Click);
            // 
            // ToolsTabControl
            // 
            this.ToolsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolsTabControl.Controls.Add(this.ToolsModelsTabPage);
            this.ToolsTabControl.Controls.Add(this.ToolsMaterialsTabPage);
            this.ToolsTabControl.Controls.Add(this.ToolsDetailsTabPage);
            this.ToolsTabControl.Controls.Add(this.ToolsOptionsTabPage);
            this.ToolsTabControl.Location = new System.Drawing.Point(3, 37);
            this.ToolsTabControl.Margin = new System.Windows.Forms.Padding(4);
            this.ToolsTabControl.Name = "ToolsTabControl";
            this.ToolsTabControl.SelectedIndex = 0;
            this.ToolsTabControl.Size = new System.Drawing.Size(332, 647);
            this.ToolsTabControl.TabIndex = 1;
            // 
            // ToolsModelsTabPage
            // 
            this.ToolsModelsTabPage.Controls.Add(this.EnableRootMotionCheckBox);
            this.ToolsModelsTabPage.Controls.Add(this.label22);
            this.ToolsModelsTabPage.Controls.Add(this.ClipComboBox);
            this.ToolsModelsTabPage.Controls.Add(this.label21);
            this.ToolsModelsTabPage.Controls.Add(this.ClipDictComboBox);
            this.ToolsModelsTabPage.Controls.Add(this.ModelsTreeView);
            this.ToolsModelsTabPage.Location = new System.Drawing.Point(4, 25);
            this.ToolsModelsTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.ToolsModelsTabPage.Name = "ToolsModelsTabPage";
            this.ToolsModelsTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.ToolsModelsTabPage.Size = new System.Drawing.Size(324, 618);
            this.ToolsModelsTabPage.TabIndex = 0;
            this.ToolsModelsTabPage.Text = "Models";
            this.ToolsModelsTabPage.UseVisualStyleBackColor = true;
            // 
            // EnableRootMotionCheckBox
            // 
            this.EnableRootMotionCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.EnableRootMotionCheckBox.AutoSize = true;
            this.EnableRootMotionCheckBox.Location = new System.Drawing.Point(67, 596);
            this.EnableRootMotionCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.EnableRootMotionCheckBox.Name = "EnableRootMotionCheckBox";
            this.EnableRootMotionCheckBox.Size = new System.Drawing.Size(141, 20);
            this.EnableRootMotionCheckBox.TabIndex = 37;
            this.EnableRootMotionCheckBox.Text = "Enable root motion";
            this.EnableRootMotionCheckBox.UseVisualStyleBackColor = true;
            this.EnableRootMotionCheckBox.CheckedChanged += new System.EventHandler(this.EnableRootMotionCheckBox_CheckedChanged);
            // 
            // label22
            // 
            this.label22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(31, 567);
            this.label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(33, 16);
            this.label22.TabIndex = 36;
            this.label22.Text = "Clip:";
            // 
            // ClipComboBox
            // 
            this.ClipComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ClipComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.ClipComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.ClipComboBox.FormattingEnabled = true;
            this.ClipComboBox.Location = new System.Drawing.Point(67, 564);
            this.ClipComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.ClipComboBox.Name = "ClipComboBox";
            this.ClipComboBox.Size = new System.Drawing.Size(253, 24);
            this.ClipComboBox.TabIndex = 35;
            this.ClipComboBox.TextChanged += new System.EventHandler(this.ClipComboBox_TextChanged);
            // 
            // label21
            // 
            this.label21.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(1, 534);
            this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(59, 16);
            this.label21.TabIndex = 34;
            this.label21.Text = "Clip Dict:";
            // 
            // ClipDictComboBox
            // 
            this.ClipDictComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ClipDictComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.ClipDictComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.ClipDictComboBox.FormattingEnabled = true;
            this.ClipDictComboBox.Location = new System.Drawing.Point(67, 530);
            this.ClipDictComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.ClipDictComboBox.Name = "ClipDictComboBox";
            this.ClipDictComboBox.Size = new System.Drawing.Size(253, 24);
            this.ClipDictComboBox.TabIndex = 33;
            this.ClipDictComboBox.TextChanged += new System.EventHandler(this.ClipDictComboBox_TextChanged);
            // 
            // ModelsTreeView
            // 
            this.ModelsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModelsTreeView.CheckBoxes = true;
            this.ModelsTreeView.Location = new System.Drawing.Point(0, 4);
            this.ModelsTreeView.Margin = new System.Windows.Forms.Padding(4);
            this.ModelsTreeView.Name = "ModelsTreeView";
            this.ModelsTreeView.ShowRootLines = false;
            this.ModelsTreeView.Size = new System.Drawing.Size(320, 518);
            this.ModelsTreeView.TabIndex = 1;
            this.ModelsTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.ModelsTreeView_AfterCheck);
            this.ModelsTreeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ModelsTreeView_NodeMouseDoubleClick);
            this.ModelsTreeView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ModelsTreeView_KeyPress);
            // 
            // ToolsMaterialsTabPage
            // 
            this.ToolsMaterialsTabPage.Controls.Add(this.SaveSharedTexturesButton);
            this.ToolsMaterialsTabPage.Controls.Add(this.TexturesTreeView);
            this.ToolsMaterialsTabPage.Controls.Add(this.SaveAllTexturesButton);
            this.ToolsMaterialsTabPage.Location = new System.Drawing.Point(4, 25);
            this.ToolsMaterialsTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.ToolsMaterialsTabPage.Name = "ToolsMaterialsTabPage";
            this.ToolsMaterialsTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.ToolsMaterialsTabPage.Size = new System.Drawing.Size(324, 618);
            this.ToolsMaterialsTabPage.TabIndex = 1;
            this.ToolsMaterialsTabPage.Text = "Materials";
            this.ToolsMaterialsTabPage.UseVisualStyleBackColor = true;
            // 
            // SaveSharedTexturesButton
            // 
            this.SaveSharedTexturesButton.Location = new System.Drawing.Point(145, 4);
            this.SaveSharedTexturesButton.Margin = new System.Windows.Forms.Padding(4);
            this.SaveSharedTexturesButton.Name = "SaveSharedTexturesButton";
            this.SaveSharedTexturesButton.Size = new System.Drawing.Size(176, 28);
            this.SaveSharedTexturesButton.TabIndex = 2;
            this.SaveSharedTexturesButton.Text = "Save shared textures...";
            this.SaveSharedTexturesButton.UseVisualStyleBackColor = true;
            this.SaveSharedTexturesButton.Click += new System.EventHandler(this.SaveSharedTexturesButton_Click);
            // 
            // TexturesTreeView
            // 
            this.TexturesTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TexturesTreeView.Location = new System.Drawing.Point(0, 39);
            this.TexturesTreeView.Margin = new System.Windows.Forms.Padding(4);
            this.TexturesTreeView.Name = "TexturesTreeView";
            this.TexturesTreeView.ShowRootLines = false;
            this.TexturesTreeView.Size = new System.Drawing.Size(320, 575);
            this.TexturesTreeView.TabIndex = 1;
            // 
            // SaveAllTexturesButton
            // 
            this.SaveAllTexturesButton.Location = new System.Drawing.Point(0, 4);
            this.SaveAllTexturesButton.Margin = new System.Windows.Forms.Padding(4);
            this.SaveAllTexturesButton.Name = "SaveAllTexturesButton";
            this.SaveAllTexturesButton.Size = new System.Drawing.Size(144, 28);
            this.SaveAllTexturesButton.TabIndex = 3;
            this.SaveAllTexturesButton.Text = "Save all textures...";
            this.SaveAllTexturesButton.UseVisualStyleBackColor = true;
            this.SaveAllTexturesButton.Click += new System.EventHandler(this.SaveAllTexturesButton_Click);
            // 
            // ToolsDetailsTabPage
            // 
            this.ToolsDetailsTabPage.Controls.Add(this.DetailsPropertyGrid);
            this.ToolsDetailsTabPage.Location = new System.Drawing.Point(4, 25);
            this.ToolsDetailsTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.ToolsDetailsTabPage.Name = "ToolsDetailsTabPage";
            this.ToolsDetailsTabPage.Size = new System.Drawing.Size(324, 618);
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
            this.DetailsPropertyGrid.Location = new System.Drawing.Point(0, 4);
            this.DetailsPropertyGrid.Margin = new System.Windows.Forms.Padding(4);
            this.DetailsPropertyGrid.Name = "DetailsPropertyGrid";
            this.DetailsPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.DetailsPropertyGrid.ReadOnly = true;
            this.DetailsPropertyGrid.Size = new System.Drawing.Size(321, 612);
            this.DetailsPropertyGrid.TabIndex = 1;
            this.DetailsPropertyGrid.ToolbarVisible = false;
            // 
            // ToolsOptionsTabPage
            // 
            this.ToolsOptionsTabPage.Controls.Add(this.OptionsTabControl);
            this.ToolsOptionsTabPage.Location = new System.Drawing.Point(4, 25);
            this.ToolsOptionsTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.ToolsOptionsTabPage.Name = "ToolsOptionsTabPage";
            this.ToolsOptionsTabPage.Size = new System.Drawing.Size(324, 618);
            this.ToolsOptionsTabPage.TabIndex = 3;
            this.ToolsOptionsTabPage.Text = "Options";
            this.ToolsOptionsTabPage.UseVisualStyleBackColor = true;
            // 
            // OptionsTabControl
            // 
            this.OptionsTabControl.Controls.Add(this.OptionsRenderTabPage);
            this.OptionsTabControl.Controls.Add(this.OptionsLightingTabPage);
            this.OptionsTabControl.Controls.Add(this.OptionsHelperTabPage);
            this.OptionsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OptionsTabControl.Location = new System.Drawing.Point(0, 0);
            this.OptionsTabControl.Margin = new System.Windows.Forms.Padding(4);
            this.OptionsTabControl.Name = "OptionsTabControl";
            this.OptionsTabControl.SelectedIndex = 0;
            this.OptionsTabControl.Size = new System.Drawing.Size(324, 618);
            this.OptionsTabControl.TabIndex = 26;
            // 
            // OptionsRenderTabPage
            // 
            this.OptionsRenderTabPage.Controls.Add(this.RenderModeComboBox);
            this.OptionsRenderTabPage.Controls.Add(this.FragGlassCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.label14);
            this.OptionsRenderTabPage.Controls.Add(this.HDTexturesCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.AnisotropicFilteringCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.SkeletonsCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.label10);
            this.OptionsRenderTabPage.Controls.Add(this.TextureCoordsComboBox);
            this.OptionsRenderTabPage.Controls.Add(this.TextureSamplerComboBox);
            this.OptionsRenderTabPage.Controls.Add(this.label11);
            this.OptionsRenderTabPage.Controls.Add(this.WireframeCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.ShowCollisionMeshesCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.GridCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.GridCountComboBox);
            this.OptionsRenderTabPage.Controls.Add(this.label2);
            this.OptionsRenderTabPage.Controls.Add(this.ErrorConsoleCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.GridSizeComboBox);
            this.OptionsRenderTabPage.Controls.Add(this.StatusBarCheckBox);
            this.OptionsRenderTabPage.Controls.Add(this.label1);
            this.OptionsRenderTabPage.Location = new System.Drawing.Point(4, 25);
            this.OptionsRenderTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.OptionsRenderTabPage.Name = "OptionsRenderTabPage";
            this.OptionsRenderTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.OptionsRenderTabPage.Size = new System.Drawing.Size(316, 589);
            this.OptionsRenderTabPage.TabIndex = 0;
            this.OptionsRenderTabPage.Text = "Render";
            this.OptionsRenderTabPage.UseVisualStyleBackColor = true;
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
            this.RenderModeComboBox.Location = new System.Drawing.Point(115, 18);
            this.RenderModeComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.RenderModeComboBox.Name = "RenderModeComboBox";
            this.RenderModeComboBox.Size = new System.Drawing.Size(175, 24);
            this.RenderModeComboBox.TabIndex = 12;
            this.RenderModeComboBox.SelectedIndexChanged += new System.EventHandler(this.RenderModeComboBox_SelectedIndexChanged);
            // 
            // FragGlassCheckBox
            // 
            this.FragGlassCheckBox.AutoSize = true;
            this.FragGlassCheckBox.Location = new System.Drawing.Point(8, 268);
            this.FragGlassCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.FragGlassCheckBox.Name = "FragGlassCheckBox";
            this.FragGlassCheckBox.Size = new System.Drawing.Size(218, 20);
            this.FragGlassCheckBox.TabIndex = 25;
            this.FragGlassCheckBox.Text = "Show Fragments Glass Outlines";
            this.FragGlassCheckBox.UseVisualStyleBackColor = true;
            this.FragGlassCheckBox.CheckedChanged += new System.EventHandler(this.FragGlassCheckBox_CheckedChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(8, 89);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(78, 16);
            this.label14.TabIndex = 15;
            this.label14.Text = "Tex coords:";
            // 
            // HDTexturesCheckBox
            // 
            this.HDTexturesCheckBox.AutoSize = true;
            this.HDTexturesCheckBox.Checked = true;
            this.HDTexturesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.HDTexturesCheckBox.Location = new System.Drawing.Point(8, 183);
            this.HDTexturesCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.HDTexturesCheckBox.Name = "HDTexturesCheckBox";
            this.HDTexturesCheckBox.Size = new System.Drawing.Size(98, 20);
            this.HDTexturesCheckBox.TabIndex = 10;
            this.HDTexturesCheckBox.Text = "HD textures";
            this.HDTexturesCheckBox.UseVisualStyleBackColor = true;
            this.HDTexturesCheckBox.CheckedChanged += new System.EventHandler(this.HDTexturesCheckBox_CheckedChanged);
            // 
            // AnisotropicFilteringCheckBox
            // 
            this.AnisotropicFilteringCheckBox.AutoSize = true;
            this.AnisotropicFilteringCheckBox.Checked = true;
            this.AnisotropicFilteringCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AnisotropicFilteringCheckBox.Location = new System.Drawing.Point(8, 155);
            this.AnisotropicFilteringCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.AnisotropicFilteringCheckBox.Name = "AnisotropicFilteringCheckBox";
            this.AnisotropicFilteringCheckBox.Size = new System.Drawing.Size(141, 20);
            this.AnisotropicFilteringCheckBox.TabIndex = 9;
            this.AnisotropicFilteringCheckBox.Text = "Anisotropic filtering";
            this.AnisotropicFilteringCheckBox.UseVisualStyleBackColor = true;
            this.AnisotropicFilteringCheckBox.CheckedChanged += new System.EventHandler(this.AnisotropicFilteringCheckBox_CheckedChanged);
            // 
            // SkeletonsCheckBox
            // 
            this.SkeletonsCheckBox.AutoSize = true;
            this.SkeletonsCheckBox.Location = new System.Drawing.Point(8, 240);
            this.SkeletonsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.SkeletonsCheckBox.Name = "SkeletonsCheckBox";
            this.SkeletonsCheckBox.Size = new System.Drawing.Size(125, 20);
            this.SkeletonsCheckBox.TabIndex = 22;
            this.SkeletonsCheckBox.Text = "Show Skeletons";
            this.SkeletonsCheckBox.UseVisualStyleBackColor = true;
            this.SkeletonsCheckBox.CheckedChanged += new System.EventHandler(this.SkeletonsCheckBox_CheckedChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 22);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(93, 16);
            this.label10.TabIndex = 11;
            this.label10.Text = "Render mode:";
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
            this.TextureCoordsComboBox.Location = new System.Drawing.Point(115, 85);
            this.TextureCoordsComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.TextureCoordsComboBox.Name = "TextureCoordsComboBox";
            this.TextureCoordsComboBox.Size = new System.Drawing.Size(175, 24);
            this.TextureCoordsComboBox.TabIndex = 16;
            this.TextureCoordsComboBox.SelectedIndexChanged += new System.EventHandler(this.TextureCoordsComboBox_SelectedIndexChanged);
            // 
            // TextureSamplerComboBox
            // 
            this.TextureSamplerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TextureSamplerComboBox.Enabled = false;
            this.TextureSamplerComboBox.FormattingEnabled = true;
            this.TextureSamplerComboBox.Location = new System.Drawing.Point(115, 52);
            this.TextureSamplerComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.TextureSamplerComboBox.Name = "TextureSamplerComboBox";
            this.TextureSamplerComboBox.Size = new System.Drawing.Size(175, 24);
            this.TextureSamplerComboBox.TabIndex = 14;
            this.TextureSamplerComboBox.SelectedIndexChanged += new System.EventHandler(this.TextureSamplerComboBox_SelectedIndexChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 55);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(85, 16);
            this.label11.TabIndex = 13;
            this.label11.Text = "Tex sampler:";
            // 
            // WireframeCheckBox
            // 
            this.WireframeCheckBox.AutoSize = true;
            this.WireframeCheckBox.Location = new System.Drawing.Point(8, 127);
            this.WireframeCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.WireframeCheckBox.Name = "WireframeCheckBox";
            this.WireframeCheckBox.Size = new System.Drawing.Size(91, 20);
            this.WireframeCheckBox.TabIndex = 8;
            this.WireframeCheckBox.Text = "Wireframe";
            this.WireframeCheckBox.UseVisualStyleBackColor = true;
            this.WireframeCheckBox.CheckedChanged += new System.EventHandler(this.WireframeCheckBox_CheckedChanged);
            // 
            // ShowCollisionMeshesCheckBox
            // 
            this.ShowCollisionMeshesCheckBox.AutoSize = true;
            this.ShowCollisionMeshesCheckBox.Location = new System.Drawing.Point(8, 212);
            this.ShowCollisionMeshesCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.ShowCollisionMeshesCheckBox.Name = "ShowCollisionMeshesCheckBox";
            this.ShowCollisionMeshesCheckBox.Size = new System.Drawing.Size(165, 20);
            this.ShowCollisionMeshesCheckBox.TabIndex = 7;
            this.ShowCollisionMeshesCheckBox.Text = "Show collision meshes";
            this.ShowCollisionMeshesCheckBox.UseVisualStyleBackColor = true;
            this.ShowCollisionMeshesCheckBox.CheckedChanged += new System.EventHandler(this.ShowCollisionMeshesCheckBox_CheckedChanged);
            // 
            // GridCheckBox
            // 
            this.GridCheckBox.AutoSize = true;
            this.GridCheckBox.Location = new System.Drawing.Point(8, 316);
            this.GridCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.GridCheckBox.Name = "GridCheckBox";
            this.GridCheckBox.Size = new System.Drawing.Size(54, 20);
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
            this.GridCountComboBox.Location = new System.Drawing.Point(115, 379);
            this.GridCountComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.GridCountComboBox.Name = "GridCountComboBox";
            this.GridCountComboBox.Size = new System.Drawing.Size(175, 24);
            this.GridCountComboBox.TabIndex = 21;
            this.GridCountComboBox.SelectedIndexChanged += new System.EventHandler(this.GridCountComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 383);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 16);
            this.label2.TabIndex = 20;
            this.label2.Text = "Grid count:";
            // 
            // ErrorConsoleCheckBox
            // 
            this.ErrorConsoleCheckBox.AutoSize = true;
            this.ErrorConsoleCheckBox.Location = new System.Drawing.Point(125, 586);
            this.ErrorConsoleCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.ErrorConsoleCheckBox.Name = "ErrorConsoleCheckBox";
            this.ErrorConsoleCheckBox.Size = new System.Drawing.Size(109, 20);
            this.ErrorConsoleCheckBox.TabIndex = 24;
            this.ErrorConsoleCheckBox.Text = "Error console";
            this.ErrorConsoleCheckBox.UseVisualStyleBackColor = true;
            this.ErrorConsoleCheckBox.CheckedChanged += new System.EventHandler(this.ErrorConsoleCheckBox_CheckedChanged);
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
            this.GridSizeComboBox.Location = new System.Drawing.Point(115, 346);
            this.GridSizeComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.GridSizeComboBox.Name = "GridSizeComboBox";
            this.GridSizeComboBox.Size = new System.Drawing.Size(175, 24);
            this.GridSizeComboBox.TabIndex = 19;
            this.GridSizeComboBox.SelectedIndexChanged += new System.EventHandler(this.GridSizeComboBox_SelectedIndexChanged);
            // 
            // StatusBarCheckBox
            // 
            this.StatusBarCheckBox.AutoSize = true;
            this.StatusBarCheckBox.Checked = true;
            this.StatusBarCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.StatusBarCheckBox.Location = new System.Drawing.Point(11, 586);
            this.StatusBarCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.StatusBarCheckBox.Name = "StatusBarCheckBox";
            this.StatusBarCheckBox.Size = new System.Drawing.Size(89, 20);
            this.StatusBarCheckBox.TabIndex = 23;
            this.StatusBarCheckBox.Text = "Status bar";
            this.StatusBarCheckBox.UseVisualStyleBackColor = true;
            this.StatusBarCheckBox.CheckedChanged += new System.EventHandler(this.StatusBarCheckBox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 350);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 16);
            this.label1.TabIndex = 18;
            this.label1.Text = "Grid unit size:";
            // 
            // OptionsLightingTabPage
            // 
            this.OptionsLightingTabPage.Controls.Add(this.DeferredShadingCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.HDLightsCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.label19);
            this.OptionsLightingTabPage.Controls.Add(this.HDRRenderingCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.TimeOfDayTrackBar);
            this.OptionsLightingTabPage.Controls.Add(this.ShadowsCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.ControlLightDirCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.SkydomeCheckBox);
            this.OptionsLightingTabPage.Controls.Add(this.TimeOfDayLabel);
            this.OptionsLightingTabPage.Location = new System.Drawing.Point(4, 25);
            this.OptionsLightingTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.OptionsLightingTabPage.Name = "OptionsLightingTabPage";
            this.OptionsLightingTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.OptionsLightingTabPage.Size = new System.Drawing.Size(316, 589);
            this.OptionsLightingTabPage.TabIndex = 1;
            this.OptionsLightingTabPage.Text = "Lighting";
            this.OptionsLightingTabPage.UseVisualStyleBackColor = true;
            // 
            // DeferredShadingCheckBox
            // 
            this.DeferredShadingCheckBox.AutoSize = true;
            this.DeferredShadingCheckBox.Location = new System.Drawing.Point(9, 7);
            this.DeferredShadingCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.DeferredShadingCheckBox.Name = "DeferredShadingCheckBox";
            this.DeferredShadingCheckBox.Size = new System.Drawing.Size(133, 20);
            this.DeferredShadingCheckBox.TabIndex = 36;
            this.DeferredShadingCheckBox.Text = "Deferred shading";
            this.DeferredShadingCheckBox.UseVisualStyleBackColor = true;
            this.DeferredShadingCheckBox.CheckedChanged += new System.EventHandler(this.DeferredShadingCheckBox_CheckedChanged);
            // 
            // HDLightsCheckBox
            // 
            this.HDLightsCheckBox.AutoSize = true;
            this.HDLightsCheckBox.Checked = true;
            this.HDLightsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.HDLightsCheckBox.Location = new System.Drawing.Point(9, 121);
            this.HDLightsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.HDLightsCheckBox.Name = "HDLightsCheckBox";
            this.HDLightsCheckBox.Size = new System.Drawing.Size(83, 20);
            this.HDLightsCheckBox.TabIndex = 35;
            this.HDLightsCheckBox.Text = "HD lights";
            this.HDLightsCheckBox.UseVisualStyleBackColor = true;
            this.HDLightsCheckBox.CheckedChanged += new System.EventHandler(this.HDLightsCheckBox_CheckedChanged);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(5, 181);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(81, 16);
            this.label19.TabIndex = 4;
            this.label19.Text = "Time of day:";
            // 
            // HDRRenderingCheckBox
            // 
            this.HDRRenderingCheckBox.AutoSize = true;
            this.HDRRenderingCheckBox.Checked = true;
            this.HDRRenderingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.HDRRenderingCheckBox.Location = new System.Drawing.Point(9, 36);
            this.HDRRenderingCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.HDRRenderingCheckBox.Name = "HDRRenderingCheckBox";
            this.HDRRenderingCheckBox.Size = new System.Drawing.Size(119, 20);
            this.HDRRenderingCheckBox.TabIndex = 0;
            this.HDRRenderingCheckBox.Text = "HDR rendering";
            this.HDRRenderingCheckBox.UseVisualStyleBackColor = true;
            this.HDRRenderingCheckBox.CheckedChanged += new System.EventHandler(this.HDRRenderingCheckBox_CheckedChanged);
            // 
            // TimeOfDayTrackBar
            // 
            this.TimeOfDayTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TimeOfDayTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.TimeOfDayTrackBar.LargeChange = 60;
            this.TimeOfDayTrackBar.Location = new System.Drawing.Point(4, 201);
            this.TimeOfDayTrackBar.Margin = new System.Windows.Forms.Padding(4);
            this.TimeOfDayTrackBar.Maximum = 1440;
            this.TimeOfDayTrackBar.Name = "TimeOfDayTrackBar";
            this.TimeOfDayTrackBar.Size = new System.Drawing.Size(308, 56);
            this.TimeOfDayTrackBar.TabIndex = 6;
            this.TimeOfDayTrackBar.TickFrequency = 60;
            this.TimeOfDayTrackBar.Value = 720;
            this.TimeOfDayTrackBar.Scroll += new System.EventHandler(this.TimeOfDayTrackBar_Scroll);
            // 
            // ShadowsCheckBox
            // 
            this.ShadowsCheckBox.AutoSize = true;
            this.ShadowsCheckBox.Checked = true;
            this.ShadowsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShadowsCheckBox.Location = new System.Drawing.Point(9, 64);
            this.ShadowsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.ShadowsCheckBox.Name = "ShadowsCheckBox";
            this.ShadowsCheckBox.Size = new System.Drawing.Size(85, 20);
            this.ShadowsCheckBox.TabIndex = 1;
            this.ShadowsCheckBox.Text = "Shadows";
            this.ShadowsCheckBox.UseVisualStyleBackColor = true;
            this.ShadowsCheckBox.CheckedChanged += new System.EventHandler(this.ShadowsCheckBox_CheckedChanged);
            // 
            // ControlLightDirCheckBox
            // 
            this.ControlLightDirCheckBox.AutoSize = true;
            this.ControlLightDirCheckBox.Location = new System.Drawing.Point(9, 149);
            this.ControlLightDirCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.ControlLightDirCheckBox.Name = "ControlLightDirCheckBox";
            this.ControlLightDirCheckBox.Size = new System.Drawing.Size(152, 20);
            this.ControlLightDirCheckBox.TabIndex = 3;
            this.ControlLightDirCheckBox.Text = "Control light direction";
            this.ControlLightDirCheckBox.UseVisualStyleBackColor = true;
            this.ControlLightDirCheckBox.CheckedChanged += new System.EventHandler(this.ControlLightDirCheckBox_CheckedChanged);
            // 
            // SkydomeCheckBox
            // 
            this.SkydomeCheckBox.AutoSize = true;
            this.SkydomeCheckBox.Checked = true;
            this.SkydomeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SkydomeCheckBox.Location = new System.Drawing.Point(9, 92);
            this.SkydomeCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.SkydomeCheckBox.Name = "SkydomeCheckBox";
            this.SkydomeCheckBox.Size = new System.Drawing.Size(87, 20);
            this.SkydomeCheckBox.TabIndex = 2;
            this.SkydomeCheckBox.Text = "Skydome";
            this.SkydomeCheckBox.UseVisualStyleBackColor = true;
            this.SkydomeCheckBox.CheckedChanged += new System.EventHandler(this.SkydomeCheckBox_CheckedChanged);
            // 
            // TimeOfDayLabel
            // 
            this.TimeOfDayLabel.AutoSize = true;
            this.TimeOfDayLabel.Location = new System.Drawing.Point(100, 181);
            this.TimeOfDayLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.TimeOfDayLabel.Name = "TimeOfDayLabel";
            this.TimeOfDayLabel.Size = new System.Drawing.Size(38, 16);
            this.TimeOfDayLabel.TabIndex = 5;
            this.TimeOfDayLabel.Text = "12:00";
            // 
            // OptionsHelperTabPage
            // 
            this.OptionsHelperTabPage.Controls.Add(this.OptionsShowOutlinesCheckBox);
            this.OptionsHelperTabPage.Controls.Add(this.SnapAngleUpDown);
            this.OptionsHelperTabPage.Controls.Add(this.label33);
            this.OptionsHelperTabPage.Location = new System.Drawing.Point(4, 25);
            this.OptionsHelperTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.OptionsHelperTabPage.Name = "OptionsHelperTabPage";
            this.OptionsHelperTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.OptionsHelperTabPage.Size = new System.Drawing.Size(316, 589);
            this.OptionsHelperTabPage.TabIndex = 2;
            this.OptionsHelperTabPage.Text = "Helpers";
            this.OptionsHelperTabPage.UseVisualStyleBackColor = true;
            // 
            // SnapAngleUpDown
            // 
            this.SnapAngleUpDown.DecimalPlaces = 1;
            this.SnapAngleUpDown.Location = new System.Drawing.Point(130, 52);
            this.SnapAngleUpDown.Margin = new System.Windows.Forms.Padding(4);
            this.SnapAngleUpDown.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.SnapAngleUpDown.Name = "SnapAngleUpDown";
            this.SnapAngleUpDown.Size = new System.Drawing.Size(160, 22);
            this.SnapAngleUpDown.TabIndex = 34;
            this.SnapAngleUpDown.ValueChanged += new System.EventHandler(this.SnapAngleUpDown_ValueChanged);
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(8, 54);
            this.label33.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(114, 16);
            this.label33.TabIndex = 33;
            this.label33.Text = "Snap angle (deg):";
            // 
            // ToolsPanelHideButton
            // 
            this.ToolsPanelHideButton.Location = new System.Drawing.Point(4, 4);
            this.ToolsPanelHideButton.Margin = new System.Windows.Forms.Padding(4);
            this.ToolsPanelHideButton.Name = "ToolsPanelHideButton";
            this.ToolsPanelHideButton.Size = new System.Drawing.Size(40, 28);
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
            this.ToolsDragPanel.Location = new System.Drawing.Point(335, 0);
            this.ToolsDragPanel.Margin = new System.Windows.Forms.Padding(4);
            this.ToolsDragPanel.Name = "ToolsDragPanel";
            this.ToolsDragPanel.Size = new System.Drawing.Size(5, 688);
            this.ToolsDragPanel.TabIndex = 17;
            this.ToolsDragPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ToolsDragPanel_MouseDown);
            this.ToolsDragPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ToolsDragPanel_MouseMove);
            this.ToolsDragPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolsDragPanel_MouseUp);
            // 
            // ToolsPanelShowButton
            // 
            this.ToolsPanelShowButton.Location = new System.Drawing.Point(20, 18);
            this.ToolsPanelShowButton.Margin = new System.Windows.Forms.Padding(4);
            this.ToolsPanelShowButton.Name = "ToolsPanelShowButton";
            this.ToolsPanelShowButton.Size = new System.Drawing.Size(40, 28);
            this.ToolsPanelShowButton.TabIndex = 0;
            this.ToolsPanelShowButton.Text = ">>";
            this.ToolsPanelShowButton.UseVisualStyleBackColor = true;
            this.ToolsPanelShowButton.Click += new System.EventHandler(this.ToolsPanelShowButton_Click);
            // 
            // SaveFileDialog
            // 
            this.SaveFileDialog.Filter = "All files|*.*";
            // 
            // OptionsShowOutlinesCheckBox
            // 
            this.OptionsShowOutlinesCheckBox.AutoSize = true;
            this.OptionsShowOutlinesCheckBox.Checked = true;
            this.OptionsShowOutlinesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionsShowOutlinesCheckBox.Location = new System.Drawing.Point(9, 7);
            this.OptionsShowOutlinesCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.OptionsShowOutlinesCheckBox.Name = "OptionsShowOutlinesCheckBox";
            this.OptionsShowOutlinesCheckBox.Size = new System.Drawing.Size(144, 20);
            this.OptionsShowOutlinesCheckBox.TabIndex = 37;
            this.OptionsShowOutlinesCheckBox.Text = "Show Light Outlines";
            this.OptionsShowOutlinesCheckBox.UseVisualStyleBackColor = true;
            this.OptionsShowOutlinesCheckBox.CheckedChanged += new System.EventHandler(this.OptionsShowOutlinesCheckBox_CheckedChanged);
            // 
            // ModelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MidnightBlue;
            this.ClientSize = new System.Drawing.Size(1169, 743);
            this.Controls.Add(this.ToolsPanel);
            this.Controls.Add(this.ConsolePanel);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.ToolsPanelShowButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ModelForm";
            this.Text = "View Model - CodeWalker by dexyfex";
            this.Deactivate += new System.EventHandler(this.ModelForm_Deactivate);
            this.Load += new System.EventHandler(this.ModelForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ModelForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ModelForm_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ModelForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ModelForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ModelForm_MouseUp);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ConsolePanel.ResumeLayout(false);
            this.ConsolePanel.PerformLayout();
            this.ToolsPanel.ResumeLayout(false);
            this.MainToolbarPanel.ResumeLayout(false);
            this.MainToolbarPanel.PerformLayout();
            this.MainToolbar.ResumeLayout(false);
            this.MainToolbar.PerformLayout();
            this.ToolsTabControl.ResumeLayout(false);
            this.ToolsModelsTabPage.ResumeLayout(false);
            this.ToolsModelsTabPage.PerformLayout();
            this.ToolsMaterialsTabPage.ResumeLayout(false);
            this.ToolsDetailsTabPage.ResumeLayout(false);
            this.ToolsOptionsTabPage.ResumeLayout(false);
            this.OptionsTabControl.ResumeLayout(false);
            this.OptionsRenderTabPage.ResumeLayout(false);
            this.OptionsRenderTabPage.PerformLayout();
            this.OptionsLightingTabPage.ResumeLayout(false);
            this.OptionsLightingTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimeOfDayTrackBar)).EndInit();
            this.OptionsHelperTabPage.ResumeLayout(false);
            this.OptionsHelperTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SnapAngleUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer StatsUpdateTimer;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel MousedLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatsLabel;
        private System.Windows.Forms.Panel ConsolePanel;
        private WinForms.TextBoxFix ConsoleTextBox;
        private System.Windows.Forms.Panel ToolsPanel;
        private System.Windows.Forms.Panel ToolsDragPanel;
        private System.Windows.Forms.Button ToolsPanelHideButton;
        private System.Windows.Forms.Button ToolsPanelShowButton;
        private System.Windows.Forms.TabControl ToolsTabControl;
        private System.Windows.Forms.TabPage ToolsModelsTabPage;
        private System.Windows.Forms.TabPage ToolsMaterialsTabPage;
        private System.Windows.Forms.TabPage ToolsDetailsTabPage;
        private WinForms.TreeViewFix ModelsTreeView;
        private WinForms.ReadOnlyPropertyGrid DetailsPropertyGrid;
        private WinForms.TreeViewFix TexturesTreeView;
        private System.Windows.Forms.TabPage ToolsOptionsTabPage;
        private System.Windows.Forms.CheckBox WireframeCheckBox;
        private System.Windows.Forms.ComboBox RenderModeComboBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox TextureSamplerComboBox;
        private System.Windows.Forms.ComboBox TextureCoordsComboBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox AnisotropicFilteringCheckBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckBox HDRRenderingCheckBox;
        private System.Windows.Forms.CheckBox SkydomeCheckBox;
        private System.Windows.Forms.CheckBox ShadowsCheckBox;
        private System.Windows.Forms.CheckBox StatusBarCheckBox;
        private System.Windows.Forms.CheckBox ErrorConsoleCheckBox;
        private System.Windows.Forms.CheckBox GridCheckBox;
        private System.Windows.Forms.ComboBox GridCountComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox GridSizeComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox ShowCollisionMeshesCheckBox;
        private System.Windows.Forms.CheckBox ControlLightDirCheckBox;
        private System.Windows.Forms.Label TimeOfDayLabel;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TrackBar TimeOfDayTrackBar;
        private System.Windows.Forms.CheckBox SkeletonsCheckBox;
        private System.Windows.Forms.Button SaveSharedTexturesButton;
        private System.Windows.Forms.CheckBox HDTexturesCheckBox;
        private System.Windows.Forms.Button SaveAllTexturesButton;
        private System.Windows.Forms.Panel MainToolbarPanel;
        private System.Windows.Forms.ToolStrip MainToolbar;
        private System.Windows.Forms.ToolStripSplitButton SaveButton;
        private System.Windows.Forms.ToolStripMenuItem SaveMenuButton;
        private System.Windows.Forms.ToolStripMenuItem SaveAsMenuButton;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
        private System.Windows.Forms.CheckBox EnableRootMotionCheckBox;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.ComboBox ClipComboBox;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.ComboBox ClipDictComboBox;
        private System.Windows.Forms.CheckBox FragGlassCheckBox;
        private System.Windows.Forms.TabControl OptionsTabControl;
        private System.Windows.Forms.TabPage OptionsRenderTabPage;
        private System.Windows.Forms.TabPage OptionsLightingTabPage;
        private System.Windows.Forms.CheckBox HDLightsCheckBox;
        private System.Windows.Forms.CheckBox DeferredShadingCheckBox;
        private System.Windows.Forms.ToolStripButton ToolbarMaterialEditorButton;
        private System.Windows.Forms.ToolStripButton ToolbarTextureEditorButton;
        private System.Windows.Forms.ToolStripButton ToolbarLightEditorButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton ToolbarMoveButton;
        private System.Windows.Forms.ToolStripButton ToolbarRotateButton;
        private System.Windows.Forms.ToolStripButton ToolbarScaleButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem SaveAllTexturesMenuButton;
        private System.Windows.Forms.ToolStripMenuItem SaveSharedTexturesMenuButton;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.TabPage OptionsHelperTabPage;
        private System.Windows.Forms.NumericUpDown SnapAngleUpDown;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.CheckBox OptionsShowOutlinesCheckBox;
    }
}
