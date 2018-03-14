namespace CodeWalker
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.ControlsTabPage = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.MouseInvertCheckBox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CameraSmoothingUpDown = new System.Windows.Forms.NumericUpDown();
            this.CameraSensitivityUpDown = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.KeyBindingNameLabel = new System.Windows.Forms.Label();
            this.KeyBindingsListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.KeyBindingComboBox = new System.Windows.Forms.ComboBox();
            this.KeyBindButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.AdvancedTabPage = new System.Windows.Forms.TabPage();
            this.label22 = new System.Windows.Forms.Label();
            this.CollisionCacheSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label23 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.TextureCacheSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label21 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.GeometryCacheSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.GPUFlushTimeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label17 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.GPUCacheTimeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.ExcludeFoldersTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.CacheTimeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.CacheSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.ShadowCascadesUpDown = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.FolderBrowseButton = new System.Windows.Forms.Button();
            this.FolderTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.DoneButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.MainTabControl.SuspendLayout();
            this.ControlsTabPage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CameraSmoothingUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CameraSensitivityUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.AdvancedTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CollisionCacheSizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TextureCacheSizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GeometryCacheSizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GPUFlushTimeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GPUCacheTimeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CacheTimeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CacheSizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShadowCascadesUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTabControl.Controls.Add(this.ControlsTabPage);
            this.MainTabControl.Controls.Add(this.AdvancedTabPage);
            this.MainTabControl.Location = new System.Drawing.Point(12, 12);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(460, 451);
            this.MainTabControl.TabIndex = 0;
            // 
            // ControlsTabPage
            // 
            this.ControlsTabPage.Controls.Add(this.groupBox2);
            this.ControlsTabPage.Controls.Add(this.groupBox1);
            this.ControlsTabPage.Location = new System.Drawing.Point(4, 22);
            this.ControlsTabPage.Name = "ControlsTabPage";
            this.ControlsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ControlsTabPage.Size = new System.Drawing.Size(452, 425);
            this.ControlsTabPage.TabIndex = 0;
            this.ControlsTabPage.Text = "Controls";
            this.ControlsTabPage.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.MouseInvertCheckBox);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.CameraSmoothingUpDown);
            this.groupBox2.Controls.Add(this.CameraSensitivityUpDown);
            this.groupBox2.Location = new System.Drawing.Point(6, 322);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(440, 90);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Mouse settings";
            // 
            // MouseInvertCheckBox
            // 
            this.MouseInvertCheckBox.AutoSize = true;
            this.MouseInvertCheckBox.Location = new System.Drawing.Point(240, 55);
            this.MouseInvertCheckBox.Name = "MouseInvertCheckBox";
            this.MouseInvertCheckBox.Size = new System.Drawing.Size(118, 17);
            this.MouseInvertCheckBox.TabIndex = 9;
            this.MouseInvertCheckBox.Text = "Invert mouse Y axis";
            this.MouseInvertCheckBox.UseVisualStyleBackColor = true;
            this.MouseInvertCheckBox.CheckedChanged += new System.EventHandler(this.MouseInvertCheckBox_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(237, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(181, 26);
            this.label4.TabIndex = 8;
            this.label4.Text = "Change speed / zoom: Mouse wheel\r\nRotate camera: Left drag";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Camera smoothing:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Camera sensitivity:";
            // 
            // CameraSmoothingUpDown
            // 
            this.CameraSmoothingUpDown.DecimalPlaces = 1;
            this.CameraSmoothingUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.CameraSmoothingUpDown.Location = new System.Drawing.Point(116, 54);
            this.CameraSmoothingUpDown.Name = "CameraSmoothingUpDown";
            this.CameraSmoothingUpDown.Size = new System.Drawing.Size(77, 20);
            this.CameraSmoothingUpDown.TabIndex = 7;
            this.CameraSmoothingUpDown.ValueChanged += new System.EventHandler(this.CameraSmoothingUpDown_ValueChanged);
            // 
            // CameraSensitivityUpDown
            // 
            this.CameraSensitivityUpDown.DecimalPlaces = 1;
            this.CameraSensitivityUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.CameraSensitivityUpDown.Location = new System.Drawing.Point(116, 28);
            this.CameraSensitivityUpDown.Name = "CameraSensitivityUpDown";
            this.CameraSensitivityUpDown.Size = new System.Drawing.Size(77, 20);
            this.CameraSensitivityUpDown.TabIndex = 6;
            this.CameraSensitivityUpDown.ValueChanged += new System.EventHandler(this.CameraSensitivityUpDown_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.KeyBindingNameLabel);
            this.groupBox1.Controls.Add(this.KeyBindingsListView);
            this.groupBox1.Controls.Add(this.KeyBindingComboBox);
            this.groupBox1.Controls.Add(this.KeyBindButton);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(440, 309);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Key bindings";
            // 
            // KeyBindingNameLabel
            // 
            this.KeyBindingNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.KeyBindingNameLabel.AutoSize = true;
            this.KeyBindingNameLabel.Location = new System.Drawing.Point(259, 28);
            this.KeyBindingNameLabel.Name = "KeyBindingNameLabel";
            this.KeyBindingNameLabel.Size = new System.Drawing.Size(107, 13);
            this.KeyBindingNameLabel.TabIndex = 6;
            this.KeyBindingNameLabel.Text = "(No binding selected)";
            // 
            // KeyBindingsListView
            // 
            this.KeyBindingsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.KeyBindingsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.KeyBindingsListView.FullRowSelect = true;
            this.KeyBindingsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.KeyBindingsListView.HideSelection = false;
            this.KeyBindingsListView.Location = new System.Drawing.Point(6, 19);
            this.KeyBindingsListView.Name = "KeyBindingsListView";
            this.KeyBindingsListView.Size = new System.Drawing.Size(237, 284);
            this.KeyBindingsListView.TabIndex = 3;
            this.KeyBindingsListView.UseCompatibleStateImageBehavior = false;
            this.KeyBindingsListView.View = System.Windows.Forms.View.Details;
            this.KeyBindingsListView.SelectedIndexChanged += new System.EventHandler(this.KeyBindingsListView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Action";
            this.columnHeader1.Width = 147;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Key";
            // 
            // KeyBindingComboBox
            // 
            this.KeyBindingComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.KeyBindingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.KeyBindingComboBox.Enabled = false;
            this.KeyBindingComboBox.FormattingEnabled = true;
            this.KeyBindingComboBox.Location = new System.Drawing.Point(293, 54);
            this.KeyBindingComboBox.Name = "KeyBindingComboBox";
            this.KeyBindingComboBox.Size = new System.Drawing.Size(104, 21);
            this.KeyBindingComboBox.TabIndex = 4;
            this.KeyBindingComboBox.SelectedIndexChanged += new System.EventHandler(this.KeyBindingComboBox_SelectedIndexChanged);
            // 
            // KeyBindButton
            // 
            this.KeyBindButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.KeyBindButton.Enabled = false;
            this.KeyBindButton.Location = new System.Drawing.Point(403, 53);
            this.KeyBindButton.Name = "KeyBindButton";
            this.KeyBindButton.Size = new System.Drawing.Size(31, 23);
            this.KeyBindButton.TabIndex = 5;
            this.KeyBindButton.Text = "...";
            this.KeyBindButton.UseVisualStyleBackColor = true;
            this.KeyBindButton.Click += new System.EventHandler(this.KeyBindButton_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(259, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Key:";
            // 
            // AdvancedTabPage
            // 
            this.AdvancedTabPage.Controls.Add(this.label22);
            this.AdvancedTabPage.Controls.Add(this.CollisionCacheSizeUpDown);
            this.AdvancedTabPage.Controls.Add(this.label23);
            this.AdvancedTabPage.Controls.Add(this.label20);
            this.AdvancedTabPage.Controls.Add(this.TextureCacheSizeUpDown);
            this.AdvancedTabPage.Controls.Add(this.label21);
            this.AdvancedTabPage.Controls.Add(this.label18);
            this.AdvancedTabPage.Controls.Add(this.GeometryCacheSizeUpDown);
            this.AdvancedTabPage.Controls.Add(this.label19);
            this.AdvancedTabPage.Controls.Add(this.label16);
            this.AdvancedTabPage.Controls.Add(this.GPUFlushTimeUpDown);
            this.AdvancedTabPage.Controls.Add(this.label17);
            this.AdvancedTabPage.Controls.Add(this.label14);
            this.AdvancedTabPage.Controls.Add(this.GPUCacheTimeUpDown);
            this.AdvancedTabPage.Controls.Add(this.label15);
            this.AdvancedTabPage.Controls.Add(this.label13);
            this.AdvancedTabPage.Controls.Add(this.ExcludeFoldersTextBox);
            this.AdvancedTabPage.Controls.Add(this.label11);
            this.AdvancedTabPage.Controls.Add(this.CacheTimeUpDown);
            this.AdvancedTabPage.Controls.Add(this.label12);
            this.AdvancedTabPage.Controls.Add(this.label9);
            this.AdvancedTabPage.Controls.Add(this.CacheSizeUpDown);
            this.AdvancedTabPage.Controls.Add(this.label10);
            this.AdvancedTabPage.Controls.Add(this.label8);
            this.AdvancedTabPage.Controls.Add(this.ShadowCascadesUpDown);
            this.AdvancedTabPage.Controls.Add(this.label7);
            this.AdvancedTabPage.Controls.Add(this.label6);
            this.AdvancedTabPage.Controls.Add(this.FolderBrowseButton);
            this.AdvancedTabPage.Controls.Add(this.FolderTextBox);
            this.AdvancedTabPage.Controls.Add(this.label5);
            this.AdvancedTabPage.Location = new System.Drawing.Point(4, 22);
            this.AdvancedTabPage.Name = "AdvancedTabPage";
            this.AdvancedTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.AdvancedTabPage.Size = new System.Drawing.Size(452, 425);
            this.AdvancedTabPage.TabIndex = 1;
            this.AdvancedTabPage.Text = "Advanced";
            this.AdvancedTabPage.UseVisualStyleBackColor = true;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(184, 337);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(246, 13);
            this.label22.TabIndex = 76;
            this.label22.Text = "Maximum collisions graphics memory usage, in MB.";
            // 
            // CollisionCacheSizeUpDown
            // 
            this.CollisionCacheSizeUpDown.Increment = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.CollisionCacheSizeUpDown.Location = new System.Drawing.Point(110, 335);
            this.CollisionCacheSizeUpDown.Maximum = new decimal(new int[] {
            512,
            0,
            0,
            0});
            this.CollisionCacheSizeUpDown.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.CollisionCacheSizeUpDown.Name = "CollisionCacheSizeUpDown";
            this.CollisionCacheSizeUpDown.Size = new System.Drawing.Size(58, 20);
            this.CollisionCacheSizeUpDown.TabIndex = 77;
            this.CollisionCacheSizeUpDown.ThousandsSeparator = true;
            this.CollisionCacheSizeUpDown.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.CollisionCacheSizeUpDown.ValueChanged += new System.EventHandler(this.CollisionCacheSizeUpDown_ValueChanged);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(6, 337);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(81, 13);
            this.label23.TabIndex = 75;
            this.label23.Text = "Collision cache:";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(184, 305);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(236, 13);
            this.label20.TabIndex = 73;
            this.label20.Text = "Maximum texture graphics memory usage, in MB.";
            // 
            // TextureCacheSizeUpDown
            // 
            this.TextureCacheSizeUpDown.Increment = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.TextureCacheSizeUpDown.Location = new System.Drawing.Point(110, 303);
            this.TextureCacheSizeUpDown.Maximum = new decimal(new int[] {
            8192,
            0,
            0,
            0});
            this.TextureCacheSizeUpDown.Minimum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.TextureCacheSizeUpDown.Name = "TextureCacheSizeUpDown";
            this.TextureCacheSizeUpDown.Size = new System.Drawing.Size(58, 20);
            this.TextureCacheSizeUpDown.TabIndex = 74;
            this.TextureCacheSizeUpDown.ThousandsSeparator = true;
            this.TextureCacheSizeUpDown.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.TextureCacheSizeUpDown.ValueChanged += new System.EventHandler(this.TextureCacheSizeUpDown_ValueChanged);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(6, 305);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(79, 13);
            this.label21.TabIndex = 72;
            this.label21.Text = "Texture cache:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(184, 273);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(247, 13);
            this.label18.TabIndex = 70;
            this.label18.Text = "Maximum geometry graphics memory usage, in MB.";
            // 
            // GeometryCacheSizeUpDown
            // 
            this.GeometryCacheSizeUpDown.Increment = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.GeometryCacheSizeUpDown.Location = new System.Drawing.Point(110, 271);
            this.GeometryCacheSizeUpDown.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.GeometryCacheSizeUpDown.Minimum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.GeometryCacheSizeUpDown.Name = "GeometryCacheSizeUpDown";
            this.GeometryCacheSizeUpDown.Size = new System.Drawing.Size(58, 20);
            this.GeometryCacheSizeUpDown.TabIndex = 71;
            this.GeometryCacheSizeUpDown.ThousandsSeparator = true;
            this.GeometryCacheSizeUpDown.Value = new decimal(new int[] {
            512,
            0,
            0,
            0});
            this.GeometryCacheSizeUpDown.ValueChanged += new System.EventHandler(this.GeometryCacheSizeUpDown_ValueChanged);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(6, 273);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(88, 13);
            this.label19.TabIndex = 69;
            this.label19.Text = "Geometry cache:";
            // 
            // label16
            // 
            this.label16.Location = new System.Drawing.Point(175, 236);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(269, 34);
            this.label16.TabIndex = 68;
            this.label16.Text = "The minimum duration, in seconds, between unloading batches of resources from gra" +
    "phics memory.";
            // 
            // GPUFlushTimeUpDown
            // 
            this.GPUFlushTimeUpDown.DecimalPlaces = 2;
            this.GPUFlushTimeUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.GPUFlushTimeUpDown.Location = new System.Drawing.Point(110, 239);
            this.GPUFlushTimeUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.GPUFlushTimeUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.GPUFlushTimeUpDown.Name = "GPUFlushTimeUpDown";
            this.GPUFlushTimeUpDown.Size = new System.Drawing.Size(48, 20);
            this.GPUFlushTimeUpDown.TabIndex = 67;
            this.GPUFlushTimeUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.GPUFlushTimeUpDown.ValueChanged += new System.EventHandler(this.GPUFlushTimeUpDown_ValueChanged);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(6, 241);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(80, 13);
            this.label17.TabIndex = 66;
            this.label17.Text = "GPU flush time:";
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(175, 204);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(269, 34);
            this.label14.TabIndex = 65;
            this.label14.Text = "The minimum duration, in seconds, that resources will remain loaded in graphics m" +
    "emory.";
            // 
            // GPUCacheTimeUpDown
            // 
            this.GPUCacheTimeUpDown.DecimalPlaces = 1;
            this.GPUCacheTimeUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.GPUCacheTimeUpDown.Location = new System.Drawing.Point(110, 207);
            this.GPUCacheTimeUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.GPUCacheTimeUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.GPUCacheTimeUpDown.Name = "GPUCacheTimeUpDown";
            this.GPUCacheTimeUpDown.Size = new System.Drawing.Size(48, 20);
            this.GPUCacheTimeUpDown.TabIndex = 64;
            this.GPUCacheTimeUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.GPUCacheTimeUpDown.ValueChanged += new System.EventHandler(this.GPUCacheTimeUpDown_ValueChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 209);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(88, 13);
            this.label15.TabIndex = 63;
            this.label15.Text = "GPU cache time:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 64);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(82, 13);
            this.label13.TabIndex = 62;
            this.label13.Text = "Exclude folders:";
            // 
            // ExcludeFoldersTextBox
            // 
            this.ExcludeFoldersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExcludeFoldersTextBox.Location = new System.Drawing.Point(110, 61);
            this.ExcludeFoldersTextBox.Name = "ExcludeFoldersTextBox";
            this.ExcludeFoldersTextBox.Size = new System.Drawing.Size(303, 20);
            this.ExcludeFoldersTextBox.TabIndex = 50;
            this.ExcludeFoldersTextBox.TextChanged += new System.EventHandler(this.ExcludeFoldersTextBox_TextChanged);
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(175, 132);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(269, 34);
            this.label11.TabIndex = 59;
            this.label11.Text = "The minimum duration, in seconds, that resources will remain loaded in memory.";
            // 
            // CacheTimeUpDown
            // 
            this.CacheTimeUpDown.DecimalPlaces = 1;
            this.CacheTimeUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.CacheTimeUpDown.Location = new System.Drawing.Point(110, 135);
            this.CacheTimeUpDown.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.CacheTimeUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.CacheTimeUpDown.Name = "CacheTimeUpDown";
            this.CacheTimeUpDown.Size = new System.Drawing.Size(48, 20);
            this.CacheTimeUpDown.TabIndex = 58;
            this.CacheTimeUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.CacheTimeUpDown.ValueChanged += new System.EventHandler(this.CacheTimeUpDown_ValueChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 137);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(88, 13);
            this.label12.TabIndex = 57;
            this.label12.Text = "Main cache time:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(192, 169);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(226, 13);
            this.label9.TabIndex = 56;
            this.label9.Text = "Maximum cache system memory usage, in MB.";
            // 
            // CacheSizeUpDown
            // 
            this.CacheSizeUpDown.Increment = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.CacheSizeUpDown.Location = new System.Drawing.Point(110, 167);
            this.CacheSizeUpDown.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.CacheSizeUpDown.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.CacheSizeUpDown.Name = "CacheSizeUpDown";
            this.CacheSizeUpDown.Size = new System.Drawing.Size(64, 20);
            this.CacheSizeUpDown.TabIndex = 60;
            this.CacheSizeUpDown.ThousandsSeparator = true;
            this.CacheSizeUpDown.Value = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.CacheSizeUpDown.ValueChanged += new System.EventHandler(this.CacheSizeUpDown_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 169);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(87, 13);
            this.label10.TabIndex = 54;
            this.label10.Text = "Main cache size:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(174, 95);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(236, 26);
            this.label8.TabIndex = 53;
            this.label8.Text = "The total number of shadow cascades to render.\r\nLess cascades = better performanc" +
    "e";
            // 
            // ShadowCascadesUpDown
            // 
            this.ShadowCascadesUpDown.Location = new System.Drawing.Point(110, 98);
            this.ShadowCascadesUpDown.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.ShadowCascadesUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ShadowCascadesUpDown.Name = "ShadowCascadesUpDown";
            this.ShadowCascadesUpDown.Size = new System.Drawing.Size(48, 20);
            this.ShadowCascadesUpDown.TabIndex = 52;
            this.ShadowCascadesUpDown.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.ShadowCascadesUpDown.ValueChanged += new System.EventHandler(this.ShadowCascadesUpDown_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 100);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(98, 13);
            this.label7.TabIndex = 51;
            this.label7.Text = "Shadow cascades:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 31);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 13);
            this.label6.TabIndex = 50;
            this.label6.Text = "GTA V folder:";
            // 
            // FolderBrowseButton
            // 
            this.FolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FolderBrowseButton.Location = new System.Drawing.Point(419, 27);
            this.FolderBrowseButton.Name = "FolderBrowseButton";
            this.FolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.FolderBrowseButton.TabIndex = 49;
            this.FolderBrowseButton.Text = "...";
            this.FolderBrowseButton.UseVisualStyleBackColor = true;
            this.FolderBrowseButton.Click += new System.EventHandler(this.FolderBrowseButton_Click);
            // 
            // FolderTextBox
            // 
            this.FolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FolderTextBox.Location = new System.Drawing.Point(110, 28);
            this.FolderTextBox.Name = "FolderTextBox";
            this.FolderTextBox.ReadOnly = true;
            this.FolderTextBox.Size = new System.Drawing.Size(303, 20);
            this.FolderTextBox.TabIndex = 48;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(349, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "NOTE: These settings require CodeWalker to be restarted to take effect.";
            // 
            // DoneButton
            // 
            this.DoneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DoneButton.Location = new System.Drawing.Point(397, 469);
            this.DoneButton.Name = "DoneButton";
            this.DoneButton.Size = new System.Drawing.Size(75, 23);
            this.DoneButton.TabIndex = 3;
            this.DoneButton.Text = "Done";
            this.DoneButton.UseVisualStyleBackColor = true;
            this.DoneButton.Click += new System.EventHandler(this.DoneButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveButton.Location = new System.Drawing.Point(284, 469);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(107, 23);
            this.SaveButton.TabIndex = 2;
            this.SaveButton.Text = "Save settings";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetButton.Location = new System.Drawing.Point(158, 469);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(107, 23);
            this.ResetButton.TabIndex = 1;
            this.ResetButton.Text = "Reset all settings";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 504);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.DoneButton);
            this.Controls.Add(this.MainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.Text = "Settings - CodeWalker by dexyfex";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingsForm_FormClosed);
            this.MainTabControl.ResumeLayout(false);
            this.ControlsTabPage.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CameraSmoothingUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CameraSensitivityUpDown)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.AdvancedTabPage.ResumeLayout(false);
            this.AdvancedTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CollisionCacheSizeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TextureCacheSizeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GeometryCacheSizeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GPUFlushTimeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GPUCacheTimeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CacheTimeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CacheSizeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShadowCascadesUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage ControlsTabPage;
        private System.Windows.Forms.TabPage AdvancedTabPage;
        private System.Windows.Forms.Button DoneButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox KeyBindingComboBox;
        private System.Windows.Forms.Button KeyBindButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView KeyBindingsListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.NumericUpDown CameraSensitivityUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown CameraSmoothingUpDown;
        private System.Windows.Forms.Label KeyBindingNameLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button FolderBrowseButton;
        private System.Windows.Forms.TextBox FolderTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown ShadowCascadesUpDown;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown CacheSizeUpDown;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown CacheTimeUpDown;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox ExcludeFoldersTextBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown GPUCacheTimeUpDown;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.NumericUpDown GPUFlushTimeUpDown;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.NumericUpDown GeometryCacheSizeUpDown;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.NumericUpDown CollisionCacheSizeUpDown;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.NumericUpDown TextureCacheSizeUpDown;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.CheckBox MouseInvertCheckBox;
    }
}