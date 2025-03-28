namespace CodeWalker.Forms
{
    partial class ModelLightForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelLightForm));
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.LightPropertiesPanel1 = new System.Windows.Forms.Panel();
            this.LightsTreeView = new CodeWalker.WinForms.TreeViewFix();
            this.LightMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newLightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteLightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LightTablePanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.NewLightButton = new System.Windows.Forms.Button();
            this.DeleteLightButton = new System.Windows.Forms.Button();
            this.DuplicateLightButton = new System.Windows.Forms.Button();
            this.LightPropertiesPanel2 = new System.Windows.Forms.Panel();
            this.PositionLabel = new System.Windows.Forms.Label();
            this.PositionTextBox = new System.Windows.Forms.TextBox();
            this.GoToButton = new System.Windows.Forms.Button();
            this.DirectionLabel = new System.Windows.Forms.Label();
            this.DirectionTextBox = new System.Windows.Forms.TextBox();
            this.NormalizeDirectionButton = new System.Windows.Forms.Button();
            this.ResetDirectionButton = new System.Windows.Forms.Button();
            this.TangentLabel = new System.Windows.Forms.Label();
            this.TangentTextBox = new System.Windows.Forms.TextBox();
            this.NormalizeTangentButton = new System.Windows.Forms.Button();
            this.CalculateTangentButton = new System.Windows.Forms.Button();
            this.TypeLabel = new System.Windows.Forms.Label();
            this.TypeComboBox = new System.Windows.Forms.ComboBox();
            this.ColourRGBLabel = new System.Windows.Forms.Label();
            this.ColourLabel = new System.Windows.Forms.Label();
            this.ColourRUpDown = new System.Windows.Forms.NumericUpDown();
            this.ColourGUpDown = new System.Windows.Forms.NumericUpDown();
            this.ColourBUpDown = new System.Windows.Forms.NumericUpDown();
            this.IntensityLabel = new System.Windows.Forms.Label();
            this.IntensityTextBox = new System.Windows.Forms.TextBox();
            this.FlashinessLabel = new System.Windows.Forms.Label();
            this.FlashinessComboBox = new System.Windows.Forms.ComboBox();
            this.LightHashLabel = new System.Windows.Forms.Label();
            this.LightHashUpDown = new System.Windows.Forms.NumericUpDown();
            this.BoneIDLabel = new System.Windows.Forms.Label();
            this.BoneIDUpDown = new System.Windows.Forms.NumericUpDown();
            this.GroupIDLabel = new System.Windows.Forms.Label();
            this.GroupIDUpDown = new System.Windows.Forms.NumericUpDown();
            this.FallofLabel = new System.Windows.Forms.Label();
            this.FalloffTextBox = new System.Windows.Forms.TextBox();
            this.FalloffExponentLabel = new System.Windows.Forms.Label();
            this.FalloffExponentTextBox = new System.Windows.Forms.TextBox();
            this.InnerAngleLabel = new System.Windows.Forms.Label();
            this.InnerAngleTextBox = new System.Windows.Forms.TextBox();
            this.OuterAngleLabel = new System.Windows.Forms.Label();
            this.OuterAngleTextBox = new System.Windows.Forms.TextBox();
            this.ExtentLabel = new System.Windows.Forms.Label();
            this.ExtentTextBox = new System.Windows.Forms.TextBox();
            this.TextureHashLabel = new System.Windows.Forms.Label();
            this.TextureHashTextBox = new System.Windows.Forms.TextBox();
            this.CoronaSizeLabel = new System.Windows.Forms.Label();
            this.CoronaSizeTextBox = new System.Windows.Forms.TextBox();
            this.CoronaIntensityLabel = new System.Windows.Forms.Label();
            this.CoronaIntensityTextBox = new System.Windows.Forms.TextBox();
            this.CoronaZBiasLabel = new System.Windows.Forms.Label();
            this.CoronaZBiasTextBox = new System.Windows.Forms.TextBox();
            this.ShadowNearClipLabel = new System.Windows.Forms.Label();
            this.ShadowNearClipTextBox = new System.Windows.Forms.TextBox();
            this.ShadowBlurLabel = new System.Windows.Forms.Label();
            this.ShadowBlurUpDown = new System.Windows.Forms.NumericUpDown();
            this.ShadowFadeDistanceLabel = new System.Windows.Forms.Label();
            this.ShadowFadeDistanceUpDown = new System.Windows.Forms.NumericUpDown();
            this.LightFadeDistanceLabel = new System.Windows.Forms.Label();
            this.LightFadeDistanceUpDown = new System.Windows.Forms.NumericUpDown();
            this.SpecularFadeDistanceLabel = new System.Windows.Forms.Label();
            this.SpecularFadeDistanceUpDown = new System.Windows.Forms.NumericUpDown();
            this.VolumetricFadeDistanceLabel = new System.Windows.Forms.Label();
            this.VolumetricFadeDistanceUpDown = new System.Windows.Forms.NumericUpDown();
            this.VolumeColorRGBLabel = new System.Windows.Forms.Label();
            this.VolumeColorLabel = new System.Windows.Forms.Label();
            this.VolumeColorRUpDown = new System.Windows.Forms.NumericUpDown();
            this.VolumeColorGUpDown = new System.Windows.Forms.NumericUpDown();
            this.VolumeColorBUpDown = new System.Windows.Forms.NumericUpDown();
            this.VolumeIntensityLabel = new System.Windows.Forms.Label();
            this.VolumeIntensityTextBox = new System.Windows.Forms.TextBox();
            this.VolumeSizeScaleLabel = new System.Windows.Forms.Label();
            this.VolumeSizeScaleTextBox = new System.Windows.Forms.TextBox();
            this.VolumeOuterExponentLabel = new System.Windows.Forms.Label();
            this.VolumeOuterExponentTextBox = new System.Windows.Forms.TextBox();
            this.CullingPlaneNormalLabel = new System.Windows.Forms.Label();
            this.CullingPlaneNormalTextBox = new System.Windows.Forms.TextBox();
            this.CullingPlaneOffsetLabel = new System.Windows.Forms.Label();
            this.CullingPlaneOffsetTextBox = new System.Windows.Forms.TextBox();
            this.TimeFlagsLabel = new System.Windows.Forms.Label();
            this.TimeFlagsTextBox = new System.Windows.Forms.TextBox();
            this.TimeFlagsAMCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.TimeFlagsPMCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.FlagsLabel = new System.Windows.Forms.Label();
            this.FlagsTextBox = new System.Windows.Forms.TextBox();
            this.FlagsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.MainMenu = new CodeWalker.WinForms.MenuStripFix();
            this.EditMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.EditNewLightMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.EditDeleteLightMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.EditDuplicateLightMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.OptionsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.OptionsShowOutlinesMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.MoveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RotateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ScaleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.LightPropertiesPanel1.SuspendLayout();
            this.LightMenuStrip.SuspendLayout();
            this.LightTablePanel1.SuspendLayout();
            this.LightPropertiesPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColourRUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColourGUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColourBUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LightHashUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BoneIDUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GroupIDUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShadowBlurUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShadowFadeDistanceUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LightFadeDistanceUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SpecularFadeDistanceUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.VolumetricFadeDistanceUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.VolumeColorRUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.VolumeColorGUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.VolumeColorBUpDown)).BeginInit();
            this.MainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 28);
            this.MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.LightPropertiesPanel1);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.LightPropertiesPanel2);
            this.MainSplitContainer.Size = new System.Drawing.Size(629, 746);
            this.MainSplitContainer.SplitterDistance = 128;
            this.MainSplitContainer.TabIndex = 0;
            // 
            // LightPropertiesPanel1
            // 
            this.LightPropertiesPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LightPropertiesPanel1.Controls.Add(this.LightTablePanel1);
            this.LightPropertiesPanel1.Controls.Add(this.LightsTreeView);
            this.LightPropertiesPanel1.Location = new System.Drawing.Point(3, 3);
            this.LightPropertiesPanel1.Name = "LightPropertiesPanel1";
            this.LightPropertiesPanel1.Size = new System.Drawing.Size(122, 740);
            this.LightPropertiesPanel1.TabIndex = 2;
            // 
            // LightsTreeView
            // 
            this.LightsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LightsTreeView.ContextMenuStrip = this.LightMenuStrip;
            this.LightsTreeView.FullRowSelect = true;
            this.LightsTreeView.HideSelection = false;
            this.LightsTreeView.Location = new System.Drawing.Point(3, 3);
            this.LightsTreeView.Name = "LightsTreeView";
            this.LightsTreeView.ShowRootLines = false;
            this.LightsTreeView.Size = new System.Drawing.Size(116, 672);
            this.LightsTreeView.TabIndex = 3;
            this.LightsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.LightsTreeView_AfterSelect);
            // 
            // LightMenuStrip
            // 
            this.LightMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.LightMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newLightToolStripMenuItem,
            this.deleteLightToolStripMenuItem});
            this.LightMenuStrip.Name = "LightMenuStrip";
            this.LightMenuStrip.Size = new System.Drawing.Size(138, 48);
            // 
            // newLightToolStripMenuItem
            // 
            this.newLightToolStripMenuItem.Name = "newLightToolStripMenuItem";
            this.newLightToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.newLightToolStripMenuItem.Text = "New Light";
            this.newLightToolStripMenuItem.Click += new System.EventHandler(this.newLightToolStripMenuItem_Click);
            // 
            // deleteLightToolStripMenuItem
            // 
            this.deleteLightToolStripMenuItem.Name = "deleteLightToolStripMenuItem";
            this.deleteLightToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.deleteLightToolStripMenuItem.Text = "Delete Light";
            this.deleteLightToolStripMenuItem.Click += new System.EventHandler(this.deleteLightToolStripMenuItem_Click);
            // 
            // LightTablePanel1
            // 
            this.LightTablePanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LightTablePanel1.ColumnCount = 2;
            this.LightTablePanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LightTablePanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LightTablePanel1.Controls.Add(this.NewLightButton, 0, 0);
            this.LightTablePanel1.Controls.Add(this.DeleteLightButton, 1, 0);
            this.LightTablePanel1.Controls.Add(this.DuplicateLightButton, 0, 1);
            this.LightTablePanel1.Location = new System.Drawing.Point(3, 677);
            this.LightTablePanel1.Name = "LightTablePanel1";
            this.LightTablePanel1.RowCount = 2;
            this.LightTablePanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LightTablePanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LightTablePanel1.Size = new System.Drawing.Size(116, 60);
            this.LightTablePanel1.TabIndex = 4;
            // 
            // NewLightButton
            // 
            this.NewLightButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NewLightButton.Location = new System.Drawing.Point(3, 4);
            this.NewLightButton.Name = "NewLightButton";
            this.NewLightButton.Size = new System.Drawing.Size(52, 23);
            this.NewLightButton.TabIndex = 5;
            this.NewLightButton.Text = "New Light";
            this.NewLightButton.UseVisualStyleBackColor = true;
            this.NewLightButton.Click += new System.EventHandler(this.NewLightButton_Click);
            // 
            // DeleteLightButton
            // 
            this.DeleteLightButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteLightButton.Enabled = false;
            this.DeleteLightButton.Location = new System.Drawing.Point(61, 4);
            this.DeleteLightButton.Name = "DeleteLightButton";
            this.DeleteLightButton.Size = new System.Drawing.Size(52, 23);
            this.DeleteLightButton.TabIndex = 6;
            this.DeleteLightButton.Text = "Delete Light";
            this.DeleteLightButton.UseVisualStyleBackColor = true;
            this.DeleteLightButton.Click += new System.EventHandler(this.DeleteLightButton_Click);
            // 
            // DuplicateLightButton
            // 
            this.DuplicateLightButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LightTablePanel1.SetColumnSpan(this.DuplicateLightButton, 2);
            this.DuplicateLightButton.Location = new System.Drawing.Point(3, 34);
            this.DuplicateLightButton.Name = "DuplicateLightButton";
            this.DuplicateLightButton.Size = new System.Drawing.Size(110, 23);
            this.DuplicateLightButton.TabIndex = 7;
            this.DuplicateLightButton.Text = "Duplicate Light";
            this.DuplicateLightButton.UseVisualStyleBackColor = true;
            this.DuplicateLightButton.Click += new System.EventHandler(this.DuplicateLightButton_Click);
            // 
            // LightPropertiesPanel2
            // 
            this.LightPropertiesPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LightPropertiesPanel2.AutoScroll = true;
            this.LightPropertiesPanel2.Controls.Add(this.PositionLabel);
            this.LightPropertiesPanel2.Controls.Add(this.PositionTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.GoToButton);
            this.LightPropertiesPanel2.Controls.Add(this.DirectionLabel);
            this.LightPropertiesPanel2.Controls.Add(this.DirectionTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.NormalizeDirectionButton);
            this.LightPropertiesPanel2.Controls.Add(this.ResetDirectionButton);
            this.LightPropertiesPanel2.Controls.Add(this.TangentLabel);
            this.LightPropertiesPanel2.Controls.Add(this.TangentTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.NormalizeTangentButton);
            this.LightPropertiesPanel2.Controls.Add(this.CalculateTangentButton);
            this.LightPropertiesPanel2.Controls.Add(this.TypeLabel);
            this.LightPropertiesPanel2.Controls.Add(this.TypeComboBox);
            this.LightPropertiesPanel2.Controls.Add(this.ColourRGBLabel);
            this.LightPropertiesPanel2.Controls.Add(this.ColourLabel);
            this.LightPropertiesPanel2.Controls.Add(this.ColourRUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.ColourGUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.ColourBUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.IntensityLabel);
            this.LightPropertiesPanel2.Controls.Add(this.IntensityTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.FlashinessLabel);
            this.LightPropertiesPanel2.Controls.Add(this.FlashinessComboBox);
            this.LightPropertiesPanel2.Controls.Add(this.LightHashLabel);
            this.LightPropertiesPanel2.Controls.Add(this.LightHashUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.BoneIDLabel);
            this.LightPropertiesPanel2.Controls.Add(this.BoneIDUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.GroupIDLabel);
            this.LightPropertiesPanel2.Controls.Add(this.GroupIDUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.FallofLabel);
            this.LightPropertiesPanel2.Controls.Add(this.FalloffTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.FalloffExponentLabel);
            this.LightPropertiesPanel2.Controls.Add(this.FalloffExponentTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.InnerAngleLabel);
            this.LightPropertiesPanel2.Controls.Add(this.InnerAngleTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.OuterAngleLabel);
            this.LightPropertiesPanel2.Controls.Add(this.OuterAngleTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.ExtentLabel);
            this.LightPropertiesPanel2.Controls.Add(this.ExtentTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.TextureHashLabel);
            this.LightPropertiesPanel2.Controls.Add(this.TextureHashTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.CoronaSizeLabel);
            this.LightPropertiesPanel2.Controls.Add(this.CoronaSizeTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.CoronaIntensityLabel);
            this.LightPropertiesPanel2.Controls.Add(this.CoronaIntensityTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.CoronaZBiasLabel);
            this.LightPropertiesPanel2.Controls.Add(this.CoronaZBiasTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.ShadowNearClipLabel);
            this.LightPropertiesPanel2.Controls.Add(this.ShadowNearClipTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.ShadowBlurLabel);
            this.LightPropertiesPanel2.Controls.Add(this.ShadowBlurUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.ShadowFadeDistanceLabel);
            this.LightPropertiesPanel2.Controls.Add(this.ShadowFadeDistanceUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.LightFadeDistanceLabel);
            this.LightPropertiesPanel2.Controls.Add(this.LightFadeDistanceUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.SpecularFadeDistanceLabel);
            this.LightPropertiesPanel2.Controls.Add(this.SpecularFadeDistanceUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.VolumetricFadeDistanceLabel);
            this.LightPropertiesPanel2.Controls.Add(this.VolumetricFadeDistanceUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.VolumeColorRGBLabel);
            this.LightPropertiesPanel2.Controls.Add(this.VolumeColorLabel);
            this.LightPropertiesPanel2.Controls.Add(this.VolumeColorRUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.VolumeColorGUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.VolumeColorBUpDown);
            this.LightPropertiesPanel2.Controls.Add(this.VolumeIntensityLabel);
            this.LightPropertiesPanel2.Controls.Add(this.VolumeIntensityTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.VolumeSizeScaleLabel);
            this.LightPropertiesPanel2.Controls.Add(this.VolumeSizeScaleTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.VolumeOuterExponentLabel);
            this.LightPropertiesPanel2.Controls.Add(this.VolumeOuterExponentTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.CullingPlaneNormalLabel);
            this.LightPropertiesPanel2.Controls.Add(this.CullingPlaneNormalTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.CullingPlaneOffsetLabel);
            this.LightPropertiesPanel2.Controls.Add(this.CullingPlaneOffsetTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.TimeFlagsLabel);
            this.LightPropertiesPanel2.Controls.Add(this.TimeFlagsTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.TimeFlagsAMCheckedListBox);
            this.LightPropertiesPanel2.Controls.Add(this.TimeFlagsPMCheckedListBox);
            this.LightPropertiesPanel2.Controls.Add(this.FlagsLabel);
            this.LightPropertiesPanel2.Controls.Add(this.FlagsTextBox);
            this.LightPropertiesPanel2.Controls.Add(this.FlagsCheckedListBox);
            this.LightPropertiesPanel2.Location = new System.Drawing.Point(3, 3);
            this.LightPropertiesPanel2.Name = "LightPropertiesPanel2";
            this.LightPropertiesPanel2.Size = new System.Drawing.Size(491, 740);
            this.LightPropertiesPanel2.TabIndex = 8;
            // 
            // PositionLabel
            // 
            this.PositionLabel.AutoSize = true;
            this.PositionLabel.Location = new System.Drawing.Point(3, 5);
            this.PositionLabel.Name = "PositionLabel";
            this.PositionLabel.Size = new System.Drawing.Size(47, 13);
            this.PositionLabel.TabIndex = 101;
            this.PositionLabel.Text = "Position:";
            // 
            // PositionTextBox
            // 
            this.PositionTextBox.Location = new System.Drawing.Point(88, 2);
            this.PositionTextBox.Name = "PositionTextBox";
            this.PositionTextBox.Size = new System.Drawing.Size(337, 20);
            this.PositionTextBox.TabIndex = 102;
            this.PositionTextBox.TextChanged += new System.EventHandler(this.PositionTextBox_TextChanged);
            // 
            // GoToButton
            // 
            this.GoToButton.Location = new System.Drawing.Point(427, 0);
            this.GoToButton.Name = "GoToButton";
            this.GoToButton.Size = new System.Drawing.Size(63, 23);
            this.GoToButton.TabIndex = 103;
            this.GoToButton.Text = "Go to";
            this.GoToButton.UseVisualStyleBackColor = true;
            this.GoToButton.Click += new System.EventHandler(this.GoToButton_Click);
            // 
            // DirectionLabel
            // 
            this.DirectionLabel.AutoSize = true;
            this.DirectionLabel.Location = new System.Drawing.Point(3, 28);
            this.DirectionLabel.Name = "DirectionLabel";
            this.DirectionLabel.Size = new System.Drawing.Size(52, 13);
            this.DirectionLabel.TabIndex = 111;
            this.DirectionLabel.Text = "Direction:";
            // 
            // DirectionTextBox
            // 
            this.DirectionTextBox.Location = new System.Drawing.Point(88, 26);
            this.DirectionTextBox.Name = "DirectionTextBox";
            this.DirectionTextBox.Size = new System.Drawing.Size(272, 20);
            this.DirectionTextBox.TabIndex = 112;
            this.DirectionTextBox.TextChanged += new System.EventHandler(this.DirectionTextBox_TextChanged);
            // 
            // NormalizeDirectionButton
            // 
            this.NormalizeDirectionButton.Location = new System.Drawing.Point(361, 24);
            this.NormalizeDirectionButton.Name = "NormalizeDirectionButton";
            this.NormalizeDirectionButton.Size = new System.Drawing.Size(63, 23);
            this.NormalizeDirectionButton.TabIndex = 113;
            this.NormalizeDirectionButton.Text = "Normalize";
            this.NormalizeDirectionButton.UseVisualStyleBackColor = true;
            this.NormalizeDirectionButton.Click += new System.EventHandler(this.NormalizeDirectionButton_Click);
            // 
            // ResetDirectionButton
            // 
            this.ResetDirectionButton.Location = new System.Drawing.Point(427, 24);
            this.ResetDirectionButton.Name = "ResetDirectionButton";
            this.ResetDirectionButton.Size = new System.Drawing.Size(63, 23);
            this.ResetDirectionButton.TabIndex = 114;
            this.ResetDirectionButton.Text = "Reset";
            this.ResetDirectionButton.UseVisualStyleBackColor = true;
            this.ResetDirectionButton.Click += new System.EventHandler(this.ResetDirectionButton_Click);
            // 
            // TangentLabel
            // 
            this.TangentLabel.AutoSize = true;
            this.TangentLabel.Location = new System.Drawing.Point(3, 52);
            this.TangentLabel.Name = "TangentLabel";
            this.TangentLabel.Size = new System.Drawing.Size(50, 13);
            this.TangentLabel.TabIndex = 121;
            this.TangentLabel.Text = "Tangent:";
            // 
            // TangentTextBox
            // 
            this.TangentTextBox.Location = new System.Drawing.Point(88, 50);
            this.TangentTextBox.Name = "TangentTextBox";
            this.TangentTextBox.Size = new System.Drawing.Size(272, 20);
            this.TangentTextBox.TabIndex = 122;
            this.TangentTextBox.TextChanged += new System.EventHandler(this.TangentTextBox_TextChanged);
            // 
            // NormalizeTangentButton
            // 
            this.NormalizeTangentButton.Location = new System.Drawing.Point(361, 47);
            this.NormalizeTangentButton.Name = "NormalizeTangentButton";
            this.NormalizeTangentButton.Size = new System.Drawing.Size(63, 23);
            this.NormalizeTangentButton.TabIndex = 123;
            this.NormalizeTangentButton.Text = "Normalize";
            this.NormalizeTangentButton.UseVisualStyleBackColor = true;
            this.NormalizeTangentButton.Click += new System.EventHandler(this.NormalizeTangentButton_Click);
            // 
            // CalculateTangentButton
            // 
            this.CalculateTangentButton.Location = new System.Drawing.Point(427, 47);
            this.CalculateTangentButton.Name = "CalculateTangentButton";
            this.CalculateTangentButton.Size = new System.Drawing.Size(63, 23);
            this.CalculateTangentButton.TabIndex = 124;
            this.CalculateTangentButton.Text = "Calculate";
            this.CalculateTangentButton.UseVisualStyleBackColor = true;
            this.CalculateTangentButton.Click += new System.EventHandler(this.CalculateTangentButton_Click);
            // 
            // TypeLabel
            // 
            this.TypeLabel.AutoSize = true;
            this.TypeLabel.Location = new System.Drawing.Point(3, 76);
            this.TypeLabel.Name = "TypeLabel";
            this.TypeLabel.Size = new System.Drawing.Size(34, 13);
            this.TypeLabel.TabIndex = 131;
            this.TypeLabel.Text = "Type:";
            // 
            // TypeComboBox
            // 
            this.TypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TypeComboBox.FormattingEnabled = true;
            this.TypeComboBox.Items.AddRange(new object[] {
            "Point",
            "Spot",
            "Capsule"});
            this.TypeComboBox.Location = new System.Drawing.Point(113, 73);
            this.TypeComboBox.Name = "TypeComboBox";
            this.TypeComboBox.Size = new System.Drawing.Size(166, 21);
            this.TypeComboBox.TabIndex = 132;
            this.TypeComboBox.SelectedIndexChanged += new System.EventHandler(this.TypeComboBox_SelectedIndexChanged);
            // 
            // ColourRGBLabel
            // 
            this.ColourRGBLabel.AutoSize = true;
            this.ColourRGBLabel.Location = new System.Drawing.Point(3, 101);
            this.ColourRGBLabel.Name = "ColourRGBLabel";
            this.ColourRGBLabel.Size = new System.Drawing.Size(72, 13);
            this.ColourRGBLabel.TabIndex = 141;
            this.ColourRGBLabel.Text = "Colour (RGB):";
            // 
            // ColourLabel
            // 
            this.ColourLabel.BackColor = System.Drawing.Color.White;
            this.ColourLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ColourLabel.Location = new System.Drawing.Point(113, 98);
            this.ColourLabel.Name = "ColourLabel";
            this.ColourLabel.Size = new System.Drawing.Size(30, 20);
            this.ColourLabel.TabIndex = 142;
            this.ColourLabel.Click += new System.EventHandler(this.ColourLabel_Click);
            // 
            // ColourRUpDown
            // 
            this.ColourRUpDown.Location = new System.Drawing.Point(149, 98);
            this.ColourRUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourRUpDown.Name = "ColourRUpDown";
            this.ColourRUpDown.Size = new System.Drawing.Size(38, 20);
            this.ColourRUpDown.TabIndex = 143;
            this.ColourRUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourRUpDown.ValueChanged += new System.EventHandler(this.ColourRUpDown_ValueChanged);
            // 
            // ColourGUpDown
            // 
            this.ColourGUpDown.Location = new System.Drawing.Point(194, 98);
            this.ColourGUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourGUpDown.Name = "ColourGUpDown";
            this.ColourGUpDown.Size = new System.Drawing.Size(38, 20);
            this.ColourGUpDown.TabIndex = 144;
            this.ColourGUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourGUpDown.ValueChanged += new System.EventHandler(this.ColourGUpDown_ValueChanged);
            // 
            // ColourBUpDown
            // 
            this.ColourBUpDown.Location = new System.Drawing.Point(239, 98);
            this.ColourBUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourBUpDown.Name = "ColourBUpDown";
            this.ColourBUpDown.Size = new System.Drawing.Size(38, 20);
            this.ColourBUpDown.TabIndex = 145;
            this.ColourBUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourBUpDown.ValueChanged += new System.EventHandler(this.ColourBUpDown_ValueChanged);
            // 
            // IntensityLabel
            // 
            this.IntensityLabel.AutoSize = true;
            this.IntensityLabel.Location = new System.Drawing.Point(3, 124);
            this.IntensityLabel.Name = "IntensityLabel";
            this.IntensityLabel.Size = new System.Drawing.Size(49, 13);
            this.IntensityLabel.TabIndex = 151;
            this.IntensityLabel.Text = "Intensity:";
            // 
            // IntensityTextBox
            // 
            this.IntensityTextBox.Location = new System.Drawing.Point(113, 122);
            this.IntensityTextBox.Name = "IntensityTextBox";
            this.IntensityTextBox.Size = new System.Drawing.Size(166, 20);
            this.IntensityTextBox.TabIndex = 152;
            this.IntensityTextBox.TextChanged += new System.EventHandler(this.IntensityTextBox_TextChanged);
            // 
            // FlashinessLabel
            // 
            this.FlashinessLabel.AutoSize = true;
            this.FlashinessLabel.Location = new System.Drawing.Point(3, 149);
            this.FlashinessLabel.Name = "FlashinessLabel";
            this.FlashinessLabel.Size = new System.Drawing.Size(59, 13);
            this.FlashinessLabel.TabIndex = 161;
            this.FlashinessLabel.Text = "Flashiness:";
            // 
            // FlashinessComboBox
            // 
            this.FlashinessComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FlashinessComboBox.FormattingEnabled = true;
            this.FlashinessComboBox.Items.AddRange(new object[] {
            "0 - Constant",
            "1 - Random",
            "2 - RandomOverrideIfWet",
            "3 - OnceSecond",
            "4 - TwiceSecond",
            "5 - FiveSecond",
            "6 - RandomFlashiness",
            "7 - Off",
            "8 - Unused1",
            "9 - Alarm",
            "10 - OnWhenRaining",
            "11 - Cycle1",
            "12 - Cycle2",
            "13 - Cycle3",
            "14 - Disco",
            "15 - Candle",
            "16 - Plane",
            "17 - Fire",
            "18 - Threshold",
            "19 - Electric",
            "20 - Strobe",
            "21 - Count"});
            this.FlashinessComboBox.Location = new System.Drawing.Point(113, 145);
            this.FlashinessComboBox.Name = "FlashinessComboBox";
            this.FlashinessComboBox.Size = new System.Drawing.Size(166, 21);
            this.FlashinessComboBox.TabIndex = 162;
            this.FlashinessComboBox.SelectedIndexChanged += new System.EventHandler(this.FlashinessComboBox_SelectedIndexChanged);
            // 
            // LightHashLabel
            // 
            this.LightHashLabel.AutoSize = true;
            this.LightHashLabel.Location = new System.Drawing.Point(3, 173);
            this.LightHashLabel.Name = "LightHashLabel";
            this.LightHashLabel.Size = new System.Drawing.Size(58, 13);
            this.LightHashLabel.TabIndex = 171;
            this.LightHashLabel.Text = "LightHash:";
            // 
            // LightHashUpDown
            // 
            this.LightHashUpDown.Location = new System.Drawing.Point(113, 171);
            this.LightHashUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.LightHashUpDown.Name = "LightHashUpDown";
            this.LightHashUpDown.Size = new System.Drawing.Size(165, 20);
            this.LightHashUpDown.TabIndex = 172;
            this.LightHashUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.LightHashUpDown.TextChanged += new System.EventHandler(this.LightHash_ValueChanged);
            // 
            // BoneIDLabel
            // 
            this.BoneIDLabel.AutoSize = true;
            this.BoneIDLabel.Location = new System.Drawing.Point(3, 197);
            this.BoneIDLabel.Name = "BoneIDLabel";
            this.BoneIDLabel.Size = new System.Drawing.Size(49, 13);
            this.BoneIDLabel.TabIndex = 181;
            this.BoneIDLabel.Text = "Bone ID:";
            // 
            // BoneIDUpDown
            // 
            this.BoneIDUpDown.Location = new System.Drawing.Point(113, 194);
            this.BoneIDUpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.BoneIDUpDown.Name = "BoneIDUpDown";
            this.BoneIDUpDown.Size = new System.Drawing.Size(165, 20);
            this.BoneIDUpDown.TabIndex = 182;
            this.BoneIDUpDown.Value = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.BoneIDUpDown.ValueChanged += new System.EventHandler(this.BoneIDUpDown_ValueChanged);
            // 
            // GroupIDLabel
            // 
            this.GroupIDLabel.AutoSize = true;
            this.GroupIDLabel.Location = new System.Drawing.Point(3, 220);
            this.GroupIDLabel.Name = "GroupIDLabel";
            this.GroupIDLabel.Size = new System.Drawing.Size(53, 13);
            this.GroupIDLabel.TabIndex = 191;
            this.GroupIDLabel.Text = "Group ID:";
            // 
            // GroupIDUpDown
            // 
            this.GroupIDUpDown.Location = new System.Drawing.Point(113, 218);
            this.GroupIDUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.GroupIDUpDown.Name = "GroupIDUpDown";
            this.GroupIDUpDown.Size = new System.Drawing.Size(165, 20);
            this.GroupIDUpDown.TabIndex = 192;
            this.GroupIDUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.GroupIDUpDown.ValueChanged += new System.EventHandler(this.GroupIDUpDown_ValueChanged);
            // 
            // FallofLabel
            // 
            this.FallofLabel.AutoSize = true;
            this.FallofLabel.Location = new System.Drawing.Point(3, 244);
            this.FallofLabel.Name = "FallofLabel";
            this.FallofLabel.Size = new System.Drawing.Size(38, 13);
            this.FallofLabel.TabIndex = 201;
            this.FallofLabel.Text = "Falloff:";
            // 
            // FalloffTextBox
            // 
            this.FalloffTextBox.Location = new System.Drawing.Point(113, 241);
            this.FalloffTextBox.Name = "FalloffTextBox";
            this.FalloffTextBox.Size = new System.Drawing.Size(166, 20);
            this.FalloffTextBox.TabIndex = 202;
            this.FalloffTextBox.TextChanged += new System.EventHandler(this.FalloffTextBox_TextChanged);
            // 
            // FalloffExponentLabel
            // 
            this.FalloffExponentLabel.AutoSize = true;
            this.FalloffExponentLabel.Location = new System.Drawing.Point(3, 267);
            this.FalloffExponentLabel.Name = "FalloffExponentLabel";
            this.FalloffExponentLabel.Size = new System.Drawing.Size(58, 13);
            this.FalloffExponentLabel.TabIndex = 211;
            this.FalloffExponentLabel.Text = "Falloff exp:";
            // 
            // FalloffExponentTextBox
            // 
            this.FalloffExponentTextBox.Location = new System.Drawing.Point(113, 265);
            this.FalloffExponentTextBox.Name = "FalloffExponentTextBox";
            this.FalloffExponentTextBox.Size = new System.Drawing.Size(166, 20);
            this.FalloffExponentTextBox.TabIndex = 212;
            this.FalloffExponentTextBox.TextChanged += new System.EventHandler(this.FalloffExponentTextBox_TextChanged);
            // 
            // InnerAngleLabel
            // 
            this.InnerAngleLabel.AutoSize = true;
            this.InnerAngleLabel.Location = new System.Drawing.Point(3, 291);
            this.InnerAngleLabel.Name = "InnerAngleLabel";
            this.InnerAngleLabel.Size = new System.Drawing.Size(63, 13);
            this.InnerAngleLabel.TabIndex = 221;
            this.InnerAngleLabel.Text = "Inner angle:";
            // 
            // InnerAngleTextBox
            // 
            this.InnerAngleTextBox.Location = new System.Drawing.Point(113, 288);
            this.InnerAngleTextBox.Name = "InnerAngleTextBox";
            this.InnerAngleTextBox.Size = new System.Drawing.Size(166, 20);
            this.InnerAngleTextBox.TabIndex = 222;
            this.InnerAngleTextBox.TextChanged += new System.EventHandler(this.InnerAngleTextBox_TextChanged);
            // 
            // OuterAngleLabel
            // 
            this.OuterAngleLabel.AutoSize = true;
            this.OuterAngleLabel.Location = new System.Drawing.Point(3, 314);
            this.OuterAngleLabel.Name = "OuterAngleLabel";
            this.OuterAngleLabel.Size = new System.Drawing.Size(65, 13);
            this.OuterAngleLabel.TabIndex = 231;
            this.OuterAngleLabel.Text = "Outer angle:";
            // 
            // OuterAngleTextBox
            // 
            this.OuterAngleTextBox.Location = new System.Drawing.Point(113, 312);
            this.OuterAngleTextBox.Name = "OuterAngleTextBox";
            this.OuterAngleTextBox.Size = new System.Drawing.Size(166, 20);
            this.OuterAngleTextBox.TabIndex = 232;
            this.OuterAngleTextBox.TextChanged += new System.EventHandler(this.OuterAngleTextBox_TextChanged);
            // 
            // ExtentLabel
            // 
            this.ExtentLabel.AutoSize = true;
            this.ExtentLabel.Location = new System.Drawing.Point(3, 338);
            this.ExtentLabel.Name = "ExtentLabel";
            this.ExtentLabel.Size = new System.Drawing.Size(40, 13);
            this.ExtentLabel.TabIndex = 241;
            this.ExtentLabel.Text = "Extent:";
            // 
            // ExtentTextBox
            // 
            this.ExtentTextBox.Location = new System.Drawing.Point(113, 336);
            this.ExtentTextBox.Name = "ExtentTextBox";
            this.ExtentTextBox.Size = new System.Drawing.Size(166, 20);
            this.ExtentTextBox.TabIndex = 242;
            this.ExtentTextBox.TextChanged += new System.EventHandler(this.ExtentTextBox_TextChanged);
            // 
            // TextureHashLabel
            // 
            this.TextureHashLabel.AutoSize = true;
            this.TextureHashLabel.Location = new System.Drawing.Point(3, 362);
            this.TextureHashLabel.Name = "TextureHashLabel";
            this.TextureHashLabel.Size = new System.Drawing.Size(72, 13);
            this.TextureHashLabel.TabIndex = 251;
            this.TextureHashLabel.Text = "Texture hash:";
            // 
            // TextureHashTextBox
            // 
            this.TextureHashTextBox.Location = new System.Drawing.Point(113, 359);
            this.TextureHashTextBox.Name = "TextureHashTextBox";
            this.TextureHashTextBox.Size = new System.Drawing.Size(166, 20);
            this.TextureHashTextBox.TabIndex = 252;
            this.TextureHashTextBox.TextChanged += new System.EventHandler(this.TextureHashTextBox_TextChanged);
            // 
            // CoronaSizeLabel
            // 
            this.CoronaSizeLabel.AutoSize = true;
            this.CoronaSizeLabel.Location = new System.Drawing.Point(3, 385);
            this.CoronaSizeLabel.Name = "CoronaSizeLabel";
            this.CoronaSizeLabel.Size = new System.Drawing.Size(65, 13);
            this.CoronaSizeLabel.TabIndex = 261;
            this.CoronaSizeLabel.Text = "Corona size:";
            // 
            // CoronaSizeTextBox
            // 
            this.CoronaSizeTextBox.Location = new System.Drawing.Point(113, 383);
            this.CoronaSizeTextBox.Name = "CoronaSizeTextBox";
            this.CoronaSizeTextBox.Size = new System.Drawing.Size(166, 20);
            this.CoronaSizeTextBox.TabIndex = 262;
            this.CoronaSizeTextBox.TextChanged += new System.EventHandler(this.CoronaSizeTextBox_TextChanged);
            // 
            // CoronaIntensityLabel
            // 
            this.CoronaIntensityLabel.AutoSize = true;
            this.CoronaIntensityLabel.Location = new System.Drawing.Point(3, 409);
            this.CoronaIntensityLabel.Name = "CoronaIntensityLabel";
            this.CoronaIntensityLabel.Size = new System.Drawing.Size(85, 13);
            this.CoronaIntensityLabel.TabIndex = 271;
            this.CoronaIntensityLabel.Text = "Corona intensity:";
            // 
            // CoronaIntensityTextBox
            // 
            this.CoronaIntensityTextBox.Location = new System.Drawing.Point(113, 406);
            this.CoronaIntensityTextBox.Name = "CoronaIntensityTextBox";
            this.CoronaIntensityTextBox.Size = new System.Drawing.Size(166, 20);
            this.CoronaIntensityTextBox.TabIndex = 272;
            this.CoronaIntensityTextBox.TextChanged += new System.EventHandler(this.CoronaIntensityTextBox_TextChanged);
            // 
            // CoronaZBiasLabel
            // 
            this.CoronaZBiasLabel.AutoSize = true;
            this.CoronaZBiasLabel.Location = new System.Drawing.Point(3, 432);
            this.CoronaZBiasLabel.Name = "CoronaZBiasLabel";
            this.CoronaZBiasLabel.Size = new System.Drawing.Size(71, 13);
            this.CoronaZBiasLabel.TabIndex = 281;
            this.CoronaZBiasLabel.Text = "Corona zbias:";
            // 
            // CoronaZBiasTextBox
            // 
            this.CoronaZBiasTextBox.Location = new System.Drawing.Point(113, 430);
            this.CoronaZBiasTextBox.Name = "CoronaZBiasTextBox";
            this.CoronaZBiasTextBox.Size = new System.Drawing.Size(166, 20);
            this.CoronaZBiasTextBox.TabIndex = 282;
            this.CoronaZBiasTextBox.TextChanged += new System.EventHandler(this.CoronaZBiasTextBox_TextChanged);
            // 
            // ShadowNearClipLabel
            // 
            this.ShadowNearClipLabel.AutoSize = true;
            this.ShadowNearClipLabel.Location = new System.Drawing.Point(3, 456);
            this.ShadowNearClipLabel.Name = "ShadowNearClipLabel";
            this.ShadowNearClipLabel.Size = new System.Drawing.Size(92, 13);
            this.ShadowNearClipLabel.TabIndex = 291;
            this.ShadowNearClipLabel.Text = "Shadow near clip:";
            // 
            // ShadowNearClipTextBox
            // 
            this.ShadowNearClipTextBox.Location = new System.Drawing.Point(113, 453);
            this.ShadowNearClipTextBox.Name = "ShadowNearClipTextBox";
            this.ShadowNearClipTextBox.Size = new System.Drawing.Size(166, 20);
            this.ShadowNearClipTextBox.TabIndex = 292;
            this.ShadowNearClipTextBox.TextChanged += new System.EventHandler(this.ShadowNearClipTextBox_TextChanged);
            // 
            // ShadowBlurLabel
            // 
            this.ShadowBlurLabel.AutoSize = true;
            this.ShadowBlurLabel.Location = new System.Drawing.Point(3, 479);
            this.ShadowBlurLabel.Name = "ShadowBlurLabel";
            this.ShadowBlurLabel.Size = new System.Drawing.Size(69, 13);
            this.ShadowBlurLabel.TabIndex = 301;
            this.ShadowBlurLabel.Text = "Shadow blur:";
            // 
            // ShadowBlurUpDown
            // 
            this.ShadowBlurUpDown.Location = new System.Drawing.Point(113, 477);
            this.ShadowBlurUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ShadowBlurUpDown.Name = "ShadowBlurUpDown";
            this.ShadowBlurUpDown.Size = new System.Drawing.Size(165, 20);
            this.ShadowBlurUpDown.TabIndex = 302;
            this.ShadowBlurUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ShadowBlurUpDown.ValueChanged += new System.EventHandler(this.ShadowBlurUpDown_ValueChanged);
            // 
            // ShadowFadeDistanceLabel
            // 
            this.ShadowFadeDistanceLabel.AutoSize = true;
            this.ShadowFadeDistanceLabel.Location = new System.Drawing.Point(3, 503);
            this.ShadowFadeDistanceLabel.Name = "ShadowFadeDistanceLabel";
            this.ShadowFadeDistanceLabel.Size = new System.Drawing.Size(90, 13);
            this.ShadowFadeDistanceLabel.TabIndex = 311;
            this.ShadowFadeDistanceLabel.Text = "Shadow fade dst:";
            // 
            // ShadowFadeDistanceUpDown
            // 
            this.ShadowFadeDistanceUpDown.Location = new System.Drawing.Point(113, 500);
            this.ShadowFadeDistanceUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ShadowFadeDistanceUpDown.Name = "ShadowFadeDistanceUpDown";
            this.ShadowFadeDistanceUpDown.Size = new System.Drawing.Size(165, 20);
            this.ShadowFadeDistanceUpDown.TabIndex = 312;
            this.ShadowFadeDistanceUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ShadowFadeDistanceUpDown.ValueChanged += new System.EventHandler(this.ShadowFadeDistanceUpDown_ValueChanged);
            // 
            // LightFadeDistanceLabel
            // 
            this.LightFadeDistanceLabel.AutoSize = true;
            this.LightFadeDistanceLabel.Location = new System.Drawing.Point(3, 526);
            this.LightFadeDistanceLabel.Name = "LightFadeDistanceLabel";
            this.LightFadeDistanceLabel.Size = new System.Drawing.Size(74, 13);
            this.LightFadeDistanceLabel.TabIndex = 321;
            this.LightFadeDistanceLabel.Text = "Light fade dst:";
            // 
            // LightFadeDistanceUpDown
            // 
            this.LightFadeDistanceUpDown.Location = new System.Drawing.Point(113, 524);
            this.LightFadeDistanceUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.LightFadeDistanceUpDown.Name = "LightFadeDistanceUpDown";
            this.LightFadeDistanceUpDown.Size = new System.Drawing.Size(165, 20);
            this.LightFadeDistanceUpDown.TabIndex = 322;
            this.LightFadeDistanceUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.LightFadeDistanceUpDown.ValueChanged += new System.EventHandler(this.LightFadeDistanceUpDown_ValueChanged);
            // 
            // SpecularFadeDistanceLabel
            // 
            this.SpecularFadeDistanceLabel.AutoSize = true;
            this.SpecularFadeDistanceLabel.Location = new System.Drawing.Point(3, 550);
            this.SpecularFadeDistanceLabel.Name = "SpecularFadeDistanceLabel";
            this.SpecularFadeDistanceLabel.Size = new System.Drawing.Size(93, 13);
            this.SpecularFadeDistanceLabel.TabIndex = 331;
            this.SpecularFadeDistanceLabel.Text = "Specular fade dst:";
            // 
            // SpecularFadeDistanceUpDown
            // 
            this.SpecularFadeDistanceUpDown.Location = new System.Drawing.Point(113, 548);
            this.SpecularFadeDistanceUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.SpecularFadeDistanceUpDown.Name = "SpecularFadeDistanceUpDown";
            this.SpecularFadeDistanceUpDown.Size = new System.Drawing.Size(165, 20);
            this.SpecularFadeDistanceUpDown.TabIndex = 332;
            this.SpecularFadeDistanceUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.SpecularFadeDistanceUpDown.ValueChanged += new System.EventHandler(this.SpecularFadeDistanceUpDown_ValueChanged);
            // 
            // VolumetricFadeDistanceLabel
            // 
            this.VolumetricFadeDistanceLabel.AutoSize = true;
            this.VolumetricFadeDistanceLabel.Location = new System.Drawing.Point(3, 574);
            this.VolumetricFadeDistanceLabel.Name = "VolumetricFadeDistanceLabel";
            this.VolumetricFadeDistanceLabel.Size = new System.Drawing.Size(100, 13);
            this.VolumetricFadeDistanceLabel.TabIndex = 341;
            this.VolumetricFadeDistanceLabel.Text = "Volumetric fade dst:";
            // 
            // VolumetricFadeDistanceUpDown
            // 
            this.VolumetricFadeDistanceUpDown.Location = new System.Drawing.Point(113, 571);
            this.VolumetricFadeDistanceUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.VolumetricFadeDistanceUpDown.Name = "VolumetricFadeDistanceUpDown";
            this.VolumetricFadeDistanceUpDown.Size = new System.Drawing.Size(165, 20);
            this.VolumetricFadeDistanceUpDown.TabIndex = 342;
            this.VolumetricFadeDistanceUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.VolumetricFadeDistanceUpDown.ValueChanged += new System.EventHandler(this.VolumetricFadeDistanceUpDown_ValueChanged);
            // 
            // VolumeColorRGBLabel
            // 
            this.VolumeColorRGBLabel.AutoSize = true;
            this.VolumeColorRGBLabel.Location = new System.Drawing.Point(3, 597);
            this.VolumeColorRGBLabel.Name = "VolumeColorRGBLabel";
            this.VolumeColorRGBLabel.Size = new System.Drawing.Size(109, 13);
            this.VolumeColorRGBLabel.TabIndex = 351;
            this.VolumeColorRGBLabel.Text = "Volume colour (RGB):";
            // 
            // VolumeColorLabel
            // 
            this.VolumeColorLabel.BackColor = System.Drawing.Color.White;
            this.VolumeColorLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.VolumeColorLabel.Location = new System.Drawing.Point(113, 595);
            this.VolumeColorLabel.Name = "VolumeColorLabel";
            this.VolumeColorLabel.Size = new System.Drawing.Size(30, 20);
            this.VolumeColorLabel.TabIndex = 352;
            this.VolumeColorLabel.Click += new System.EventHandler(this.VolumeColorLabel_Click);
            // 
            // VolumeColorRUpDown
            // 
            this.VolumeColorRUpDown.Location = new System.Drawing.Point(149, 595);
            this.VolumeColorRUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.VolumeColorRUpDown.Name = "VolumeColorRUpDown";
            this.VolumeColorRUpDown.Size = new System.Drawing.Size(38, 20);
            this.VolumeColorRUpDown.TabIndex = 353;
            this.VolumeColorRUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.VolumeColorRUpDown.ValueChanged += new System.EventHandler(this.VolumeColorRUpDown_ValueChanged);
            // 
            // VolumeColorGUpDown
            // 
            this.VolumeColorGUpDown.Location = new System.Drawing.Point(194, 595);
            this.VolumeColorGUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.VolumeColorGUpDown.Name = "VolumeColorGUpDown";
            this.VolumeColorGUpDown.Size = new System.Drawing.Size(38, 20);
            this.VolumeColorGUpDown.TabIndex = 354;
            this.VolumeColorGUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.VolumeColorGUpDown.ValueChanged += new System.EventHandler(this.VolumeColorGUpDown_ValueChanged);
            // 
            // VolumeColorBUpDown
            // 
            this.VolumeColorBUpDown.Location = new System.Drawing.Point(239, 595);
            this.VolumeColorBUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.VolumeColorBUpDown.Name = "VolumeColorBUpDown";
            this.VolumeColorBUpDown.Size = new System.Drawing.Size(38, 20);
            this.VolumeColorBUpDown.TabIndex = 355;
            this.VolumeColorBUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.VolumeColorBUpDown.ValueChanged += new System.EventHandler(this.VolumeColorBUpDown_ValueChanged);
            // 
            // VolumeIntensityLabel
            // 
            this.VolumeIntensityLabel.AutoSize = true;
            this.VolumeIntensityLabel.Location = new System.Drawing.Point(3, 621);
            this.VolumeIntensityLabel.Name = "VolumeIntensityLabel";
            this.VolumeIntensityLabel.Size = new System.Drawing.Size(86, 13);
            this.VolumeIntensityLabel.TabIndex = 361;
            this.VolumeIntensityLabel.Text = "Volume intensity:";
            // 
            // VolumeIntensityTextBox
            // 
            this.VolumeIntensityTextBox.Location = new System.Drawing.Point(113, 618);
            this.VolumeIntensityTextBox.Name = "VolumeIntensityTextBox";
            this.VolumeIntensityTextBox.Size = new System.Drawing.Size(166, 20);
            this.VolumeIntensityTextBox.TabIndex = 362;
            this.VolumeIntensityTextBox.TextChanged += new System.EventHandler(this.VolumeIntensityTextBox_TextChanged);
            // 
            // VolumeSizeScaleLabel
            // 
            this.VolumeSizeScaleLabel.AutoSize = true;
            this.VolumeSizeScaleLabel.Location = new System.Drawing.Point(3, 644);
            this.VolumeSizeScaleLabel.Name = "VolumeSizeScaleLabel";
            this.VolumeSizeScaleLabel.Size = new System.Drawing.Size(94, 13);
            this.VolumeSizeScaleLabel.TabIndex = 371;
            this.VolumeSizeScaleLabel.Text = "Volume size scale:";
            // 
            // VolumeSizeScaleTextBox
            // 
            this.VolumeSizeScaleTextBox.Location = new System.Drawing.Point(113, 642);
            this.VolumeSizeScaleTextBox.Name = "VolumeSizeScaleTextBox";
            this.VolumeSizeScaleTextBox.Size = new System.Drawing.Size(166, 20);
            this.VolumeSizeScaleTextBox.TabIndex = 372;
            this.VolumeSizeScaleTextBox.TextChanged += new System.EventHandler(this.VolumeSizeScaleTextBox_TextChanged);
            // 
            // VolumeOuterExponentLabel
            // 
            this.VolumeOuterExponentLabel.AutoSize = true;
            this.VolumeOuterExponentLabel.Location = new System.Drawing.Point(3, 668);
            this.VolumeOuterExponentLabel.Name = "VolumeOuterExponentLabel";
            this.VolumeOuterExponentLabel.Size = new System.Drawing.Size(92, 13);
            this.VolumeOuterExponentLabel.TabIndex = 381;
            this.VolumeOuterExponentLabel.Text = "Volume outer exp:";
            // 
            // VolumeOuterExponentTextBox
            // 
            this.VolumeOuterExponentTextBox.Location = new System.Drawing.Point(113, 665);
            this.VolumeOuterExponentTextBox.Name = "VolumeOuterExponentTextBox";
            this.VolumeOuterExponentTextBox.Size = new System.Drawing.Size(166, 20);
            this.VolumeOuterExponentTextBox.TabIndex = 382;
            this.VolumeOuterExponentTextBox.TextChanged += new System.EventHandler(this.VolumeOuterExponentTextBox_TextChanged);
            // 
            // CullingPlaneNormalLabel
            // 
            this.CullingPlaneNormalLabel.AutoSize = true;
            this.CullingPlaneNormalLabel.Location = new System.Drawing.Point(3, 691);
            this.CullingPlaneNormalLabel.Name = "CullingPlaneNormalLabel";
            this.CullingPlaneNormalLabel.Size = new System.Drawing.Size(57, 13);
            this.CullingPlaneNormalLabel.TabIndex = 391;
            this.CullingPlaneNormalLabel.Text = "Cull Plane:";
            // 
            // CullingPlaneNormalTextBox
            // 
            this.CullingPlaneNormalTextBox.Location = new System.Drawing.Point(66, 689);
            this.CullingPlaneNormalTextBox.Name = "CullingPlaneNormalTextBox";
            this.CullingPlaneNormalTextBox.Size = new System.Drawing.Size(213, 20);
            this.CullingPlaneNormalTextBox.TabIndex = 392;
            this.CullingPlaneNormalTextBox.TextChanged += new System.EventHandler(this.CullingPlaneNormalTextBox_TextChanged);
            // 
            // CullingPlaneOffsetLabel
            // 
            this.CullingPlaneOffsetLabel.AutoSize = true;
            this.CullingPlaneOffsetLabel.Location = new System.Drawing.Point(3, 715);
            this.CullingPlaneOffsetLabel.Name = "CullingPlaneOffsetLabel";
            this.CullingPlaneOffsetLabel.Size = new System.Drawing.Size(86, 13);
            this.CullingPlaneOffsetLabel.TabIndex = 401;
            this.CullingPlaneOffsetLabel.Text = "Cull Plane offset:";
            // 
            // CullingPlaneOffsetTextBox
            // 
            this.CullingPlaneOffsetTextBox.Location = new System.Drawing.Point(113, 713);
            this.CullingPlaneOffsetTextBox.Name = "CullingPlaneOffsetTextBox";
            this.CullingPlaneOffsetTextBox.Size = new System.Drawing.Size(166, 20);
            this.CullingPlaneOffsetTextBox.TabIndex = 402;
            this.CullingPlaneOffsetTextBox.TextChanged += new System.EventHandler(this.CullingPlaneOffsetTextBox_TextChanged);
            // 
            // TimeFlagsLabel
            // 
            this.TimeFlagsLabel.AutoSize = true;
            this.TimeFlagsLabel.Location = new System.Drawing.Point(292, 76);
            this.TimeFlagsLabel.Name = "TimeFlagsLabel";
            this.TimeFlagsLabel.Size = new System.Drawing.Size(58, 13);
            this.TimeFlagsLabel.TabIndex = 411;
            this.TimeFlagsLabel.Text = "Time flags:";
            // 
            // TimeFlagsTextBox
            // 
            this.TimeFlagsTextBox.Location = new System.Drawing.Point(354, 73);
            this.TimeFlagsTextBox.Name = "TimeFlagsTextBox";
            this.TimeFlagsTextBox.Size = new System.Drawing.Size(136, 20);
            this.TimeFlagsTextBox.TabIndex = 412;
            this.TimeFlagsTextBox.TextChanged += new System.EventHandler(this.TimeFlagsTextBox_TextChanged);
            // 
            // TimeFlagsAMCheckedListBox
            // 
            this.TimeFlagsAMCheckedListBox.CheckOnClick = true;
            this.TimeFlagsAMCheckedListBox.FormattingEnabled = true;
            this.TimeFlagsAMCheckedListBox.Items.AddRange(new object[] {
            "00:00 - 01:00",
            "01:00 - 02:00",
            "02:00 - 03:00",
            "03:00 - 04:00",
            "04:00 - 05:00",
            "05:00 - 06:00",
            "06:00 - 07:00",
            "07:00 - 08:00",
            "08:00 - 09:00",
            "09:00 - 10:00",
            "10:00 - 11:00",
            "11:00 - 12:00"});
            this.TimeFlagsAMCheckedListBox.Location = new System.Drawing.Point(295, 98);
            this.TimeFlagsAMCheckedListBox.Name = "TimeFlagsAMCheckedListBox";
            this.TimeFlagsAMCheckedListBox.Size = new System.Drawing.Size(95, 184);
            this.TimeFlagsAMCheckedListBox.TabIndex = 413;
            this.TimeFlagsAMCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.TimeFlagsAMCheckedListBox_ItemCheck);
            // 
            // TimeFlagsPMCheckedListBox
            // 
            this.TimeFlagsPMCheckedListBox.CheckOnClick = true;
            this.TimeFlagsPMCheckedListBox.FormattingEnabled = true;
            this.TimeFlagsPMCheckedListBox.Items.AddRange(new object[] {
            "12:00 - 13:00",
            "13:00 - 14:00",
            "14:00 - 15:00",
            "15:00 - 16:00",
            "16:00 - 17:00",
            "17:00 - 18:00",
            "18:00 - 19:00",
            "19:00 - 20:00",
            "20:00 - 21:00",
            "21:00 - 22:00",
            "22:00 - 23:00",
            "23:00 - 00:00"});
            this.TimeFlagsPMCheckedListBox.Location = new System.Drawing.Point(395, 98);
            this.TimeFlagsPMCheckedListBox.Name = "TimeFlagsPMCheckedListBox";
            this.TimeFlagsPMCheckedListBox.Size = new System.Drawing.Size(95, 184);
            this.TimeFlagsPMCheckedListBox.TabIndex = 414;
            this.TimeFlagsPMCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.TimeFlagsPMCheckedListBox_ItemCheck);
            // 
            // FlagsLabel
            // 
            this.FlagsLabel.AutoSize = true;
            this.FlagsLabel.Location = new System.Drawing.Point(293, 291);
            this.FlagsLabel.Name = "FlagsLabel";
            this.FlagsLabel.Size = new System.Drawing.Size(35, 13);
            this.FlagsLabel.TabIndex = 421;
            this.FlagsLabel.Text = "Flags:";
            // 
            // FlagsTextBox
            // 
            this.FlagsTextBox.Location = new System.Drawing.Point(333, 288);
            this.FlagsTextBox.Name = "FlagsTextBox";
            this.FlagsTextBox.Size = new System.Drawing.Size(157, 20);
            this.FlagsTextBox.TabIndex = 422;
            this.FlagsTextBox.TextChanged += new System.EventHandler(this.FlagsTextBox_TextChanged);
            // 
            // FlagsCheckedListBox
            // 
            this.FlagsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.FlagsCheckedListBox.CheckOnClick = true;
            this.FlagsCheckedListBox.FormattingEnabled = true;
            this.FlagsCheckedListBox.Items.AddRange(new object[] {
            "Interior Only",
            "Exterior Only",
            "Dont Use In Cutscene",
            "Vehicle",
            "Fx",
            "Texture Projection",
            "Cast Shadows",
            "Cast Static Geom Shadows",
            "Cast Dynamic Geom Shadows",
            "Calc From Sun",
            "Enable Buzzing",
            "Force Buzzing",
            "Draw Volume",
            "No Specular",
            "Both Interior And Exterior",
            "Corona Only",
            "Not In Reflection",
            "Only In Reflection",
            "Use Cull Plane",
            "Use Volume Outer Colour",
            "Cast Higher Res Shadows",
            "Cast Only Lowres Shadows",
            "Far Lod Light",
            "Dont Light Alpha",
            "Cast Shadows If Possible",
            "Cutscene",
            "Moving Light Source",
            "Use Vehicle Twin",
            "Force Medium Lod Light",
            "Corona Only Lod Light",
            "Delay Render",
            "Already Tested For Occlusion"});
            this.FlagsCheckedListBox.Location = new System.Drawing.Point(295, 313);
            this.FlagsCheckedListBox.Name = "FlagsCheckedListBox";
            this.FlagsCheckedListBox.Size = new System.Drawing.Size(195, 424);
            this.FlagsCheckedListBox.TabIndex = 423;
            this.FlagsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.FlagsCheckedListBox_ItemCheck);
            // 
            // MainMenu
            // 
            this.MainMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EditMenu,
            this.OptionsMenu,
            this.MoveMenuItem,
            this.RotateMenuItem,
            this.ScaleMenuItem});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.MainMenu.Size = new System.Drawing.Size(629, 28);
            this.MainMenu.TabIndex = 1;
            this.MainMenu.Text = "menuStripFix1";
            // 
            // EditMenu
            // 
            this.EditMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EditNewLightMenu,
            this.EditDeleteLightMenu,
            this.EditDuplicateLightMenu});
            this.EditMenu.Name = "EditMenu";
            this.EditMenu.Size = new System.Drawing.Size(39, 24);
            this.EditMenu.Text = "Edit";
            // 
            // EditNewLightMenu
            // 
            this.EditNewLightMenu.Name = "EditNewLightMenu";
            this.EditNewLightMenu.Size = new System.Drawing.Size(154, 22);
            this.EditNewLightMenu.Text = "New Light";
            this.EditNewLightMenu.Click += new System.EventHandler(this.EditNewLightMenu_Click);
            // 
            // EditDeleteLightMenu
            // 
            this.EditDeleteLightMenu.Enabled = false;
            this.EditDeleteLightMenu.Name = "EditDeleteLightMenu";
            this.EditDeleteLightMenu.Size = new System.Drawing.Size(154, 22);
            this.EditDeleteLightMenu.Text = "Delete Light";
            this.EditDeleteLightMenu.Click += new System.EventHandler(this.EditDeleteLightMenu_Click);
            // 
            // EditDuplicateLightMenu
            // 
            this.EditDuplicateLightMenu.Enabled = false;
            this.EditDuplicateLightMenu.Name = "EditDuplicateLightMenu";
            this.EditDuplicateLightMenu.Size = new System.Drawing.Size(154, 22);
            this.EditDuplicateLightMenu.Text = "Duplicate Light";
            this.EditDuplicateLightMenu.Click += new System.EventHandler(this.EditDuplicateLightMenu_Click);
            // 
            // OptionsMenu
            // 
            this.OptionsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OptionsShowOutlinesMenu});
            this.OptionsMenu.Name = "OptionsMenu";
            this.OptionsMenu.Size = new System.Drawing.Size(61, 24);
            this.OptionsMenu.Text = "Options";
            this.OptionsMenu.Visible = false;
            // 
            // OptionsShowOutlinesMenu
            // 
            this.OptionsShowOutlinesMenu.Checked = true;
            this.OptionsShowOutlinesMenu.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionsShowOutlinesMenu.Name = "OptionsShowOutlinesMenu";
            this.OptionsShowOutlinesMenu.Size = new System.Drawing.Size(180, 22);
            this.OptionsShowOutlinesMenu.Text = "Show Light Outlines";
            this.OptionsShowOutlinesMenu.Click += new System.EventHandler(this.OptionsShowOutlinesMenu_Click);
            // 
            // MoveMenuItem
            // 
            this.MoveMenuItem.BackColor = System.Drawing.SystemColors.Control;
            this.MoveMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("MoveMenuItem.Image")));
            this.MoveMenuItem.Name = "MoveMenuItem";
            this.MoveMenuItem.Size = new System.Drawing.Size(32, 24);
            this.MoveMenuItem.ToolTipText = "Move";
            this.MoveMenuItem.Click += new System.EventHandler(this.MoveMenuItem_Click);
            // 
            // RotateMenuItem
            // 
            this.RotateMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("RotateMenuItem.Image")));
            this.RotateMenuItem.Name = "RotateMenuItem";
            this.RotateMenuItem.Size = new System.Drawing.Size(32, 24);
            this.RotateMenuItem.ToolTipText = "Rotate";
            this.RotateMenuItem.Click += new System.EventHandler(this.RotateMenuItem_Click);
            // 
            // ScaleMenuItem
            // 
            this.ScaleMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ScaleMenuItem.Image")));
            this.ScaleMenuItem.Name = "ScaleMenuItem";
            this.ScaleMenuItem.Size = new System.Drawing.Size(32, 24);
            this.ScaleMenuItem.ToolTipText = "Scale";
            this.ScaleMenuItem.Click += new System.EventHandler(this.ScaleMenuItem_Click);
            // 
            // ModelLightForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 774);
            this.Controls.Add(this.MainSplitContainer);
            this.Controls.Add(this.MainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MainMenu;
            this.Name = "ModelLightForm";
            this.Text = "Light Editor - CodeWalker by dexyfex";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ModelLightForm_FormClosed);
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.LightPropertiesPanel1.ResumeLayout(false);
            this.LightMenuStrip.ResumeLayout(false);
            this.LightTablePanel1.ResumeLayout(false);
            this.LightPropertiesPanel2.ResumeLayout(false);
            this.LightPropertiesPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColourRUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColourGUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColourBUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LightHashUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BoneIDUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GroupIDUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShadowBlurUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShadowFadeDistanceUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LightFadeDistanceUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SpecularFadeDistanceUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.VolumetricFadeDistanceUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.VolumeColorRUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.VolumeColorGUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.VolumeColorBUpDown)).EndInit();
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer MainSplitContainer;
        private WinForms.TreeViewFix LightsTreeView;
        private System.Windows.Forms.Panel LightPropertiesPanel2;
        private System.Windows.Forms.NumericUpDown ColourBUpDown;
        private System.Windows.Forms.NumericUpDown ColourGUpDown;
        private System.Windows.Forms.NumericUpDown ColourRUpDown;
        private System.Windows.Forms.TextBox TimeFlagsTextBox;
        private System.Windows.Forms.Label TimeFlagsLabel;
        private System.Windows.Forms.CheckedListBox TimeFlagsPMCheckedListBox;
        private System.Windows.Forms.CheckedListBox TimeFlagsAMCheckedListBox;
        private System.Windows.Forms.Label CoronaIntensityLabel;
        private System.Windows.Forms.Label OuterAngleLabel;
        private System.Windows.Forms.Label InnerAngleLabel;
        private System.Windows.Forms.Label TextureHashLabel;
        private System.Windows.Forms.TextBox TextureHashTextBox;
        private System.Windows.Forms.Label FalloffExponentLabel;
        private System.Windows.Forms.TextBox FalloffExponentTextBox;
        private System.Windows.Forms.Label FallofLabel;
        private System.Windows.Forms.TextBox FalloffTextBox;
        private System.Windows.Forms.ComboBox TypeComboBox;
        private System.Windows.Forms.Label IntensityLabel;
        private System.Windows.Forms.Label ColourRGBLabel;
        private System.Windows.Forms.Label TypeLabel;
        private System.Windows.Forms.Label ColourLabel;
        private System.Windows.Forms.Button NormalizeDirectionButton;
        private System.Windows.Forms.Label DirectionLabel;
        private System.Windows.Forms.TextBox DirectionTextBox;
        private System.Windows.Forms.Button GoToButton;
        private System.Windows.Forms.TextBox PositionTextBox;
        private System.Windows.Forms.Label PositionLabel;
        private System.Windows.Forms.TextBox CoronaSizeTextBox;
        private System.Windows.Forms.Label CoronaSizeLabel;
        private System.Windows.Forms.Label FlashinessLabel;
        private System.Windows.Forms.Label BoneIDLabel;
        private System.Windows.Forms.Label GroupIDLabel;
        private System.Windows.Forms.NumericUpDown VolumeColorBUpDown;
        private System.Windows.Forms.NumericUpDown VolumeColorGUpDown;
        private System.Windows.Forms.NumericUpDown VolumeColorRUpDown;
        private System.Windows.Forms.Label VolumeColorLabel;
        private System.Windows.Forms.Label ShadowFadeDistanceLabel;
        private System.Windows.Forms.Label LightFadeDistanceLabel;
        private System.Windows.Forms.Label VolumeOuterExponentLabel;
        private System.Windows.Forms.TextBox VolumeOuterExponentTextBox;
        private System.Windows.Forms.Label VolumeColorRGBLabel;
        private System.Windows.Forms.Label VolumeSizeScaleLabel;
        private System.Windows.Forms.TextBox VolumeSizeScaleTextBox;
        private System.Windows.Forms.Label VolumeIntensityLabel;
        private System.Windows.Forms.TextBox VolumeIntensityTextBox;
        private System.Windows.Forms.Label ShadowBlurLabel;
        private System.Windows.Forms.Label CoronaZBiasLabel;
        private System.Windows.Forms.TextBox CoronaZBiasTextBox;
        private System.Windows.Forms.Label ShadowNearClipLabel;
        private System.Windows.Forms.TextBox ShadowNearClipTextBox;
        private System.Windows.Forms.Label VolumetricFadeDistanceLabel;
        private System.Windows.Forms.Label SpecularFadeDistanceLabel;
        private System.Windows.Forms.Label ExtentLabel;
        private System.Windows.Forms.TextBox ExtentTextBox;
        private WinForms.MenuStripFix MainMenu;
        private System.Windows.Forms.ToolStripMenuItem EditMenu;
        private System.Windows.Forms.ToolStripMenuItem EditNewLightMenu;
        private System.Windows.Forms.ToolStripMenuItem EditDeleteLightMenu;
        private System.Windows.Forms.ToolStripMenuItem EditDuplicateLightMenu;
        private System.Windows.Forms.ToolStripMenuItem OptionsMenu;
        private System.Windows.Forms.ToolStripMenuItem OptionsShowOutlinesMenu;
        private System.Windows.Forms.ToolStripMenuItem MoveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RotateMenuItem;
        private System.Windows.Forms.Label CullingPlaneNormalLabel;
        private System.Windows.Forms.TextBox CullingPlaneNormalTextBox;
        private System.Windows.Forms.Label CullingPlaneOffsetLabel;
        private System.Windows.Forms.TextBox CullingPlaneOffsetTextBox;
        private System.Windows.Forms.ToolStripMenuItem ScaleMenuItem;
        private System.Windows.Forms.Button NormalizeTangentButton;
        private System.Windows.Forms.Label TangentLabel;
        private System.Windows.Forms.TextBox TangentTextBox;
        private System.Windows.Forms.TextBox FlagsTextBox;
        private System.Windows.Forms.Label FlagsLabel;
        private System.Windows.Forms.TextBox IntensityTextBox;
        private System.Windows.Forms.TextBox OuterAngleTextBox;
        private System.Windows.Forms.TextBox InnerAngleTextBox;
        private System.Windows.Forms.TextBox CoronaIntensityTextBox;
        private System.Windows.Forms.NumericUpDown VolumetricFadeDistanceUpDown;
        private System.Windows.Forms.NumericUpDown SpecularFadeDistanceUpDown;
        private System.Windows.Forms.NumericUpDown ShadowFadeDistanceUpDown;
        private System.Windows.Forms.NumericUpDown LightFadeDistanceUpDown;
        private System.Windows.Forms.NumericUpDown ShadowBlurUpDown;
        private System.Windows.Forms.NumericUpDown GroupIDUpDown;
        private System.Windows.Forms.NumericUpDown BoneIDUpDown;
        private System.Windows.Forms.ComboBox FlashinessComboBox;
        private System.Windows.Forms.Button DeleteLightButton;
        private System.Windows.Forms.Button NewLightButton;
        private System.Windows.Forms.Button DuplicateLightButton;
        private System.Windows.Forms.ContextMenuStrip LightMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem newLightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteLightToolStripMenuItem;
        private System.Windows.Forms.Label LightHashLabel;
        private System.Windows.Forms.NumericUpDown LightHashUpDown;
        private System.Windows.Forms.CheckedListBox FlagsCheckedListBox;
        private System.Windows.Forms.Panel LightPropertiesPanel1;
        private System.Windows.Forms.Button ResetDirectionButton;
        private System.Windows.Forms.Button CalculateTangentButton;
        private System.Windows.Forms.TableLayoutPanel LightTablePanel1;
    }
}
