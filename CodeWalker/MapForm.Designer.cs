namespace CodeWalker
{
    partial class MapForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapForm));
            this.MapComboBox = new System.Windows.Forms.ComboBox();
            this.LoadingLabel = new System.Windows.Forms.Label();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.MainPanelHideButton = new System.Windows.Forms.Button();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.ToolsTabPage = new System.Windows.Forms.TabPage();
            this.ResetMarkersButton = new System.Windows.Forms.Button();
            this.ClearMarkersButton = new System.Windows.Forms.Button();
            this.GoToButton = new System.Windows.Forms.Button();
            this.LocatorStatusLabel = new System.Windows.Forms.Label();
            this.ShowLocatorCheckBox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.LocateTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.AddMarkersButton = new System.Windows.Forms.Button();
            this.MultiFindTextBox = new System.Windows.Forms.TextBox();
            this.MarkersTabPage = new System.Windows.Forms.TabPage();
            this.GoToSelectedMarkerButton = new System.Windows.Forms.Button();
            this.CopyMarkersButton = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.MarkersListView = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.OptionsTabPage = new System.Windows.Forms.TabPage();
            this.LocatorStyleComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SmoothingUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.ZoomSpeedUpDown = new System.Windows.Forms.NumericUpDown();
            this.MarkerStyleComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CalibrateTabPage = new System.Windows.Forms.TabPage();
            this.label11 = new System.Windows.Forms.Label();
            this.UnitsPerTexelYTextBox = new System.Windows.Forms.TextBox();
            this.CalibrationErrorLabel = new System.Windows.Forms.Label();
            this.CalibrateButton = new System.Windows.Forms.Button();
            this.CalibrationPointsTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.SetCoordButton = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.WorldCoordTextBox = new System.Windows.Forms.TextBox();
            this.TextureFileLabel = new System.Windows.Forms.Label();
            this.TextureNameLabel = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.UnitsPerTexelXTextBox = new System.Windows.Forms.TextBox();
            this.SetOriginButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.TextureOriginTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.TextureCoordTextBox = new System.Windows.Forms.TextBox();
            this.MainPanelShowButton = new System.Windows.Forms.Button();
            this.SelectionPanel = new System.Windows.Forms.Panel();
            this.SelectionPositionTextBox = new System.Windows.Forms.TextBox();
            this.SelectionNameTextBox = new System.Windows.Forms.TextBox();
            this.MainPanel.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.ToolsTabPage.SuspendLayout();
            this.MarkersTabPage.SuspendLayout();
            this.OptionsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SmoothingUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ZoomSpeedUpDown)).BeginInit();
            this.CalibrateTabPage.SuspendLayout();
            this.SelectionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MapComboBox
            // 
            this.MapComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MapComboBox.FormattingEnabled = true;
            this.MapComboBox.Location = new System.Drawing.Point(12, 12);
            this.MapComboBox.Name = "MapComboBox";
            this.MapComboBox.Size = new System.Drawing.Size(82, 21);
            this.MapComboBox.TabIndex = 0;
            this.MapComboBox.SelectedIndexChanged += new System.EventHandler(this.MapComboBox_SelectedIndexChanged);
            // 
            // LoadingLabel
            // 
            this.LoadingLabel.AutoSize = true;
            this.LoadingLabel.BackColor = System.Drawing.Color.Transparent;
            this.LoadingLabel.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoadingLabel.ForeColor = System.Drawing.Color.White;
            this.LoadingLabel.Location = new System.Drawing.Point(315, 52);
            this.LoadingLabel.Name = "LoadingLabel";
            this.LoadingLabel.Size = new System.Drawing.Size(166, 18);
            this.LoadingLabel.TabIndex = 1;
            this.LoadingLabel.Text = "Loading texture...";
            this.LoadingLabel.Visible = false;
            // 
            // MainPanel
            // 
            this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainPanel.BackColor = System.Drawing.Color.Silver;
            this.MainPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MainPanel.Controls.Add(this.MainPanelHideButton);
            this.MainPanel.Controls.Add(this.MainTabControl);
            this.MainPanel.Location = new System.Drawing.Point(607, 2);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(240, 619);
            this.MainPanel.TabIndex = 2;
            this.MainPanel.Visible = false;
            // 
            // MainPanelHideButton
            // 
            this.MainPanelHideButton.Location = new System.Drawing.Point(198, 7);
            this.MainPanelHideButton.Name = "MainPanelHideButton";
            this.MainPanelHideButton.Size = new System.Drawing.Size(31, 23);
            this.MainPanelHideButton.TabIndex = 0;
            this.MainPanelHideButton.Text = ">>";
            this.MainPanelHideButton.UseVisualStyleBackColor = true;
            this.MainPanelHideButton.Click += new System.EventHandler(this.MainPanelHideButton_Click);
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTabControl.Controls.Add(this.ToolsTabPage);
            this.MainTabControl.Controls.Add(this.MarkersTabPage);
            this.MainTabControl.Controls.Add(this.OptionsTabPage);
            this.MainTabControl.Controls.Add(this.CalibrateTabPage);
            this.MainTabControl.Location = new System.Drawing.Point(-1, 36);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(240, 582);
            this.MainTabControl.TabIndex = 1;
            // 
            // ToolsTabPage
            // 
            this.ToolsTabPage.Controls.Add(this.ResetMarkersButton);
            this.ToolsTabPage.Controls.Add(this.ClearMarkersButton);
            this.ToolsTabPage.Controls.Add(this.GoToButton);
            this.ToolsTabPage.Controls.Add(this.LocatorStatusLabel);
            this.ToolsTabPage.Controls.Add(this.ShowLocatorCheckBox);
            this.ToolsTabPage.Controls.Add(this.label5);
            this.ToolsTabPage.Controls.Add(this.LocateTextBox);
            this.ToolsTabPage.Controls.Add(this.label4);
            this.ToolsTabPage.Controls.Add(this.AddMarkersButton);
            this.ToolsTabPage.Controls.Add(this.MultiFindTextBox);
            this.ToolsTabPage.Location = new System.Drawing.Point(4, 22);
            this.ToolsTabPage.Name = "ToolsTabPage";
            this.ToolsTabPage.Size = new System.Drawing.Size(232, 556);
            this.ToolsTabPage.TabIndex = 2;
            this.ToolsTabPage.Text = "Tools";
            this.ToolsTabPage.UseVisualStyleBackColor = true;
            // 
            // ResetMarkersButton
            // 
            this.ResetMarkersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetMarkersButton.Location = new System.Drawing.Point(149, 507);
            this.ResetMarkersButton.Name = "ResetMarkersButton";
            this.ResetMarkersButton.Size = new System.Drawing.Size(83, 23);
            this.ResetMarkersButton.TabIndex = 9;
            this.ResetMarkersButton.Text = "Reset markers";
            this.ResetMarkersButton.UseVisualStyleBackColor = true;
            this.ResetMarkersButton.Click += new System.EventHandler(this.ResetMarkersButton_Click);
            // 
            // ClearMarkersButton
            // 
            this.ClearMarkersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearMarkersButton.Location = new System.Drawing.Point(149, 478);
            this.ClearMarkersButton.Name = "ClearMarkersButton";
            this.ClearMarkersButton.Size = new System.Drawing.Size(83, 23);
            this.ClearMarkersButton.TabIndex = 8;
            this.ClearMarkersButton.Text = "Clear markers";
            this.ClearMarkersButton.UseVisualStyleBackColor = true;
            this.ClearMarkersButton.Click += new System.EventHandler(this.ClearMarkersButton_Click);
            // 
            // GoToButton
            // 
            this.GoToButton.Location = new System.Drawing.Point(189, 48);
            this.GoToButton.Name = "GoToButton";
            this.GoToButton.Size = new System.Drawing.Size(43, 22);
            this.GoToButton.TabIndex = 7;
            this.GoToButton.Text = "Go to";
            this.GoToButton.UseVisualStyleBackColor = true;
            this.GoToButton.Click += new System.EventHandler(this.GoToButton_Click);
            // 
            // LocatorStatusLabel
            // 
            this.LocatorStatusLabel.AutoSize = true;
            this.LocatorStatusLabel.Location = new System.Drawing.Point(3, 72);
            this.LocatorStatusLabel.Name = "LocatorStatusLabel";
            this.LocatorStatusLabel.Size = new System.Drawing.Size(193, 13);
            this.LocatorStatusLabel.TabIndex = 6;
            this.LocatorStatusLabel.Text = "Enter coord above or drag the marker...";
            // 
            // ShowLocatorCheckBox
            // 
            this.ShowLocatorCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowLocatorCheckBox.AutoSize = true;
            this.ShowLocatorCheckBox.Checked = true;
            this.ShowLocatorCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowLocatorCheckBox.Location = new System.Drawing.Point(101, 32);
            this.ShowLocatorCheckBox.Name = "ShowLocatorCheckBox";
            this.ShowLocatorCheckBox.Size = new System.Drawing.Size(88, 17);
            this.ShowLocatorCheckBox.TabIndex = 5;
            this.ShowLocatorCheckBox.Text = "Show marker";
            this.ShowLocatorCheckBox.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Locate: X, Y, Z";
            // 
            // LocateTextBox
            // 
            this.LocateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LocateTextBox.Location = new System.Drawing.Point(0, 49);
            this.LocateTextBox.Name = "LocateTextBox";
            this.LocateTextBox.Size = new System.Drawing.Size(189, 20);
            this.LocateTextBox.TabIndex = 3;
            this.LocateTextBox.Text = "0, 0, 0";
            this.LocateTextBox.TextChanged += new System.EventHandler(this.LocateTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 142);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Multi-find: X, Y, Z, Name";
            // 
            // AddMarkersButton
            // 
            this.AddMarkersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddMarkersButton.Location = new System.Drawing.Point(0, 478);
            this.AddMarkersButton.Name = "AddMarkersButton";
            this.AddMarkersButton.Size = new System.Drawing.Size(75, 23);
            this.AddMarkersButton.TabIndex = 1;
            this.AddMarkersButton.Text = "Add markers";
            this.AddMarkersButton.UseVisualStyleBackColor = true;
            this.AddMarkersButton.Click += new System.EventHandler(this.AddMarkersButton_Click);
            // 
            // MultiFindTextBox
            // 
            this.MultiFindTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MultiFindTextBox.Location = new System.Drawing.Point(0, 158);
            this.MultiFindTextBox.MaxLength = 999999;
            this.MultiFindTextBox.Multiline = true;
            this.MultiFindTextBox.Name = "MultiFindTextBox";
            this.MultiFindTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.MultiFindTextBox.Size = new System.Drawing.Size(232, 314);
            this.MultiFindTextBox.TabIndex = 0;
            // 
            // MarkersTabPage
            // 
            this.MarkersTabPage.Controls.Add(this.GoToSelectedMarkerButton);
            this.MarkersTabPage.Controls.Add(this.CopyMarkersButton);
            this.MarkersTabPage.Controls.Add(this.checkBox1);
            this.MarkersTabPage.Controls.Add(this.MarkersListView);
            this.MarkersTabPage.Location = new System.Drawing.Point(4, 22);
            this.MarkersTabPage.Name = "MarkersTabPage";
            this.MarkersTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.MarkersTabPage.Size = new System.Drawing.Size(232, 556);
            this.MarkersTabPage.TabIndex = 0;
            this.MarkersTabPage.Text = "Markers";
            this.MarkersTabPage.UseVisualStyleBackColor = true;
            // 
            // GoToSelectedMarkerButton
            // 
            this.GoToSelectedMarkerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GoToSelectedMarkerButton.Location = new System.Drawing.Point(141, 494);
            this.GoToSelectedMarkerButton.Name = "GoToSelectedMarkerButton";
            this.GoToSelectedMarkerButton.Size = new System.Drawing.Size(91, 23);
            this.GoToSelectedMarkerButton.TabIndex = 3;
            this.GoToSelectedMarkerButton.Text = "Go to selected";
            this.GoToSelectedMarkerButton.UseVisualStyleBackColor = true;
            this.GoToSelectedMarkerButton.Click += new System.EventHandler(this.GoToSelectedMarkerButton_Click);
            // 
            // CopyMarkersButton
            // 
            this.CopyMarkersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CopyMarkersButton.Location = new System.Drawing.Point(0, 494);
            this.CopyMarkersButton.Name = "CopyMarkersButton";
            this.CopyMarkersButton.Size = new System.Drawing.Size(75, 23);
            this.CopyMarkersButton.TabIndex = 2;
            this.CopyMarkersButton.Text = "Copy list";
            this.CopyMarkersButton.UseVisualStyleBackColor = true;
            this.CopyMarkersButton.Click += new System.EventHandler(this.CopyMarkersButton_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(3, 3);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(78, 17);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.Text = "Visible only";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // MarkersListView
            // 
            this.MarkersListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MarkersListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MarkersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.MarkersListView.FullRowSelect = true;
            this.MarkersListView.HideSelection = false;
            this.MarkersListView.Location = new System.Drawing.Point(0, 26);
            this.MarkersListView.Name = "MarkersListView";
            this.MarkersListView.Size = new System.Drawing.Size(232, 462);
            this.MarkersListView.TabIndex = 0;
            this.MarkersListView.UseCompatibleStateImageBehavior = false;
            this.MarkersListView.View = System.Windows.Forms.View.Details;
            this.MarkersListView.DoubleClick += new System.EventHandler(this.MarkersListView_DoubleClick);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Name";
            this.columnHeader4.Width = 230;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "X";
            this.columnHeader1.Width = 75;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Y";
            this.columnHeader2.Width = 75;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Z";
            this.columnHeader3.Width = 75;
            // 
            // OptionsTabPage
            // 
            this.OptionsTabPage.Controls.Add(this.LocatorStyleComboBox);
            this.OptionsTabPage.Controls.Add(this.label6);
            this.OptionsTabPage.Controls.Add(this.label3);
            this.OptionsTabPage.Controls.Add(this.SmoothingUpDown);
            this.OptionsTabPage.Controls.Add(this.label2);
            this.OptionsTabPage.Controls.Add(this.ZoomSpeedUpDown);
            this.OptionsTabPage.Controls.Add(this.MarkerStyleComboBox);
            this.OptionsTabPage.Controls.Add(this.label1);
            this.OptionsTabPage.Location = new System.Drawing.Point(4, 22);
            this.OptionsTabPage.Name = "OptionsTabPage";
            this.OptionsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.OptionsTabPage.Size = new System.Drawing.Size(232, 556);
            this.OptionsTabPage.TabIndex = 1;
            this.OptionsTabPage.Text = "Options";
            this.OptionsTabPage.UseVisualStyleBackColor = true;
            // 
            // LocatorStyleComboBox
            // 
            this.LocatorStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LocatorStyleComboBox.FormattingEnabled = true;
            this.LocatorStyleComboBox.Location = new System.Drawing.Point(79, 33);
            this.LocatorStyleComboBox.Name = "LocatorStyleComboBox";
            this.LocatorStyleComboBox.Size = new System.Drawing.Size(150, 21);
            this.LocatorStyleComboBox.TabIndex = 7;
            this.LocatorStyleComboBox.SelectedIndexChanged += new System.EventHandler(this.LocatorStyleComboBox_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 36);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Locator style:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Smoothing:";
            // 
            // SmoothingUpDown
            // 
            this.SmoothingUpDown.DecimalPlaces = 2;
            this.SmoothingUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.SmoothingUpDown.Location = new System.Drawing.Point(79, 86);
            this.SmoothingUpDown.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.SmoothingUpDown.Name = "SmoothingUpDown";
            this.SmoothingUpDown.Size = new System.Drawing.Size(150, 20);
            this.SmoothingUpDown.TabIndex = 4;
            this.SmoothingUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Zoom speed:";
            // 
            // ZoomSpeedUpDown
            // 
            this.ZoomSpeedUpDown.DecimalPlaces = 2;
            this.ZoomSpeedUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ZoomSpeedUpDown.Location = new System.Drawing.Point(79, 60);
            this.ZoomSpeedUpDown.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.ZoomSpeedUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.ZoomSpeedUpDown.Name = "ZoomSpeedUpDown";
            this.ZoomSpeedUpDown.Size = new System.Drawing.Size(150, 20);
            this.ZoomSpeedUpDown.TabIndex = 2;
            this.ZoomSpeedUpDown.Value = new decimal(new int[] {
            15,
            0,
            0,
            65536});
            // 
            // MarkerStyleComboBox
            // 
            this.MarkerStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MarkerStyleComboBox.FormattingEnabled = true;
            this.MarkerStyleComboBox.Location = new System.Drawing.Point(79, 6);
            this.MarkerStyleComboBox.Name = "MarkerStyleComboBox";
            this.MarkerStyleComboBox.Size = new System.Drawing.Size(150, 21);
            this.MarkerStyleComboBox.TabIndex = 1;
            this.MarkerStyleComboBox.SelectedIndexChanged += new System.EventHandler(this.MarkerStyleComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Marker style:";
            // 
            // CalibrateTabPage
            // 
            this.CalibrateTabPage.Controls.Add(this.label11);
            this.CalibrateTabPage.Controls.Add(this.UnitsPerTexelYTextBox);
            this.CalibrateTabPage.Controls.Add(this.CalibrationErrorLabel);
            this.CalibrateTabPage.Controls.Add(this.CalibrateButton);
            this.CalibrateTabPage.Controls.Add(this.CalibrationPointsTextBox);
            this.CalibrateTabPage.Controls.Add(this.label10);
            this.CalibrateTabPage.Controls.Add(this.SetCoordButton);
            this.CalibrateTabPage.Controls.Add(this.label12);
            this.CalibrateTabPage.Controls.Add(this.WorldCoordTextBox);
            this.CalibrateTabPage.Controls.Add(this.TextureFileLabel);
            this.CalibrateTabPage.Controls.Add(this.TextureNameLabel);
            this.CalibrateTabPage.Controls.Add(this.label9);
            this.CalibrateTabPage.Controls.Add(this.UnitsPerTexelXTextBox);
            this.CalibrateTabPage.Controls.Add(this.SetOriginButton);
            this.CalibrateTabPage.Controls.Add(this.label8);
            this.CalibrateTabPage.Controls.Add(this.TextureOriginTextBox);
            this.CalibrateTabPage.Controls.Add(this.label7);
            this.CalibrateTabPage.Controls.Add(this.TextureCoordTextBox);
            this.CalibrateTabPage.Location = new System.Drawing.Point(4, 22);
            this.CalibrateTabPage.Name = "CalibrateTabPage";
            this.CalibrateTabPage.Size = new System.Drawing.Size(232, 556);
            this.CalibrateTabPage.TabIndex = 3;
            this.CalibrateTabPage.Text = "Calibrate";
            this.CalibrateTabPage.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(0, 155);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(87, 13);
            this.label11.TabIndex = 32;
            this.label11.Text = "Units per texel Y:";
            // 
            // UnitsPerTexelYTextBox
            // 
            this.UnitsPerTexelYTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UnitsPerTexelYTextBox.Location = new System.Drawing.Point(93, 152);
            this.UnitsPerTexelYTextBox.Name = "UnitsPerTexelYTextBox";
            this.UnitsPerTexelYTextBox.Size = new System.Drawing.Size(139, 20);
            this.UnitsPerTexelYTextBox.TabIndex = 31;
            this.UnitsPerTexelYTextBox.Text = "1";
            // 
            // CalibrationErrorLabel
            // 
            this.CalibrationErrorLabel.Location = new System.Drawing.Point(6, 488);
            this.CalibrationErrorLabel.Name = "CalibrationErrorLabel";
            this.CalibrationErrorLabel.Size = new System.Drawing.Size(226, 36);
            this.CalibrationErrorLabel.TabIndex = 30;
            this.CalibrationErrorLabel.Text = "No calibration performed";
            this.CalibrationErrorLabel.Visible = false;
            // 
            // CalibrateButton
            // 
            this.CalibrateButton.Location = new System.Drawing.Point(157, 462);
            this.CalibrateButton.Name = "CalibrateButton";
            this.CalibrateButton.Size = new System.Drawing.Size(75, 23);
            this.CalibrateButton.TabIndex = 29;
            this.CalibrateButton.Text = "Calibrate";
            this.CalibrateButton.UseVisualStyleBackColor = true;
            this.CalibrateButton.Visible = false;
            this.CalibrateButton.Click += new System.EventHandler(this.CalibrateButton_Click);
            // 
            // CalibrationPointsTextBox
            // 
            this.CalibrationPointsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.CalibrationPointsTextBox.Location = new System.Drawing.Point(0, 374);
            this.CalibrationPointsTextBox.Multiline = true;
            this.CalibrationPointsTextBox.Name = "CalibrationPointsTextBox";
            this.CalibrationPointsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.CalibrationPointsTextBox.Size = new System.Drawing.Size(232, 82);
            this.CalibrationPointsTextBox.TabIndex = 28;
            this.CalibrationPointsTextBox.Visible = false;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(3, 236);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(226, 135);
            this.label10.TabIndex = 27;
            this.label10.Text = resources.GetString("label10.Text");
            // 
            // SetCoordButton
            // 
            this.SetCoordButton.Location = new System.Drawing.Point(157, 184);
            this.SetCoordButton.Name = "SetCoordButton";
            this.SetCoordButton.Size = new System.Drawing.Size(68, 23);
            this.SetCoordButton.TabIndex = 26;
            this.SetCoordButton.Text = "Set coord";
            this.SetCoordButton.UseVisualStyleBackColor = true;
            this.SetCoordButton.Click += new System.EventHandler(this.SetCoordButton_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(0, 63);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(68, 13);
            this.label12.TabIndex = 25;
            this.label12.Text = "World coord:";
            // 
            // WorldCoordTextBox
            // 
            this.WorldCoordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WorldCoordTextBox.Location = new System.Drawing.Point(93, 60);
            this.WorldCoordTextBox.Name = "WorldCoordTextBox";
            this.WorldCoordTextBox.Size = new System.Drawing.Size(139, 20);
            this.WorldCoordTextBox.TabIndex = 24;
            this.WorldCoordTextBox.Text = "0, 0";
            // 
            // TextureFileLabel
            // 
            this.TextureFileLabel.AutoSize = true;
            this.TextureFileLabel.Location = new System.Drawing.Point(2, 28);
            this.TextureFileLabel.Name = "TextureFileLabel";
            this.TextureFileLabel.Size = new System.Drawing.Size(59, 13);
            this.TextureFileLabel.TabIndex = 23;
            this.TextureFileLabel.Text = "Texture file";
            // 
            // TextureNameLabel
            // 
            this.TextureNameLabel.AutoSize = true;
            this.TextureNameLabel.Location = new System.Drawing.Point(2, 9);
            this.TextureNameLabel.Name = "TextureNameLabel";
            this.TextureNameLabel.Size = new System.Drawing.Size(72, 13);
            this.TextureNameLabel.TabIndex = 22;
            this.TextureNameLabel.Text = "Texture name";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(0, 132);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(87, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "Units per texel X:";
            // 
            // UnitsPerTexelXTextBox
            // 
            this.UnitsPerTexelXTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UnitsPerTexelXTextBox.Location = new System.Drawing.Point(93, 129);
            this.UnitsPerTexelXTextBox.Name = "UnitsPerTexelXTextBox";
            this.UnitsPerTexelXTextBox.Size = new System.Drawing.Size(139, 20);
            this.UnitsPerTexelXTextBox.TabIndex = 20;
            this.UnitsPerTexelXTextBox.Text = "1";
            // 
            // SetOriginButton
            // 
            this.SetOriginButton.Location = new System.Drawing.Point(81, 184);
            this.SetOriginButton.Name = "SetOriginButton";
            this.SetOriginButton.Size = new System.Drawing.Size(68, 23);
            this.SetOriginButton.TabIndex = 19;
            this.SetOriginButton.Text = "Set origin";
            this.SetOriginButton.UseVisualStyleBackColor = true;
            this.SetOriginButton.Click += new System.EventHandler(this.SetOriginButton_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(0, 109);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Texture origin:";
            // 
            // TextureOriginTextBox
            // 
            this.TextureOriginTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextureOriginTextBox.Location = new System.Drawing.Point(93, 106);
            this.TextureOriginTextBox.Name = "TextureOriginTextBox";
            this.TextureOriginTextBox.Size = new System.Drawing.Size(139, 20);
            this.TextureOriginTextBox.TabIndex = 17;
            this.TextureOriginTextBox.Text = "0, 0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(0, 86);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Texture coord:";
            // 
            // TextureCoordTextBox
            // 
            this.TextureCoordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextureCoordTextBox.Location = new System.Drawing.Point(93, 83);
            this.TextureCoordTextBox.Name = "TextureCoordTextBox";
            this.TextureCoordTextBox.Size = new System.Drawing.Size(139, 20);
            this.TextureCoordTextBox.TabIndex = 15;
            this.TextureCoordTextBox.Text = "0, 0";
            // 
            // MainPanelShowButton
            // 
            this.MainPanelShowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MainPanelShowButton.Location = new System.Drawing.Point(806, 10);
            this.MainPanelShowButton.Name = "MainPanelShowButton";
            this.MainPanelShowButton.Size = new System.Drawing.Size(31, 23);
            this.MainPanelShowButton.TabIndex = 3;
            this.MainPanelShowButton.Text = "<<";
            this.MainPanelShowButton.UseVisualStyleBackColor = true;
            this.MainPanelShowButton.Click += new System.EventHandler(this.MainPanelShowButton_Click);
            // 
            // SelectionPanel
            // 
            this.SelectionPanel.BackColor = System.Drawing.Color.White;
            this.SelectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SelectionPanel.Controls.Add(this.SelectionPositionTextBox);
            this.SelectionPanel.Controls.Add(this.SelectionNameTextBox);
            this.SelectionPanel.Location = new System.Drawing.Point(12, 73);
            this.SelectionPanel.Name = "SelectionPanel";
            this.SelectionPanel.Size = new System.Drawing.Size(180, 42);
            this.SelectionPanel.TabIndex = 4;
            this.SelectionPanel.Visible = false;
            // 
            // SelectionPositionTextBox
            // 
            this.SelectionPositionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectionPositionTextBox.BackColor = System.Drawing.Color.White;
            this.SelectionPositionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SelectionPositionTextBox.Location = new System.Drawing.Point(3, 22);
            this.SelectionPositionTextBox.Name = "SelectionPositionTextBox";
            this.SelectionPositionTextBox.ReadOnly = true;
            this.SelectionPositionTextBox.Size = new System.Drawing.Size(172, 13);
            this.SelectionPositionTextBox.TabIndex = 1;
            // 
            // SelectionNameTextBox
            // 
            this.SelectionNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectionNameTextBox.BackColor = System.Drawing.Color.White;
            this.SelectionNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SelectionNameTextBox.Location = new System.Drawing.Point(3, 3);
            this.SelectionNameTextBox.Name = "SelectionNameTextBox";
            this.SelectionNameTextBox.ReadOnly = true;
            this.SelectionNameTextBox.Size = new System.Drawing.Size(172, 13);
            this.SelectionNameTextBox.TabIndex = 0;
            // 
            // MapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(849, 623);
            this.Controls.Add(this.MainPanelShowButton);
            this.Controls.Add(this.MainPanel);
            this.Controls.Add(this.LoadingLabel);
            this.Controls.Add(this.MapComboBox);
            this.Controls.Add(this.SelectionPanel);
            this.DoubleBuffered = true;
            this.Name = "MapForm";
            this.Text = "Map - CodeWalker by dexyfex";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MapForm_FormClosing);
            this.ResizeBegin += new System.EventHandler(this.MapForm_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.MapForm_ResizeEnd);
            this.ClientSizeChanged += new System.EventHandler(this.MapForm_ClientSizeChanged);
            this.SizeChanged += new System.EventHandler(this.MapForm_SizeChanged);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MapForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MapForm_MouseUp);
            this.MainPanel.ResumeLayout(false);
            this.MainTabControl.ResumeLayout(false);
            this.ToolsTabPage.ResumeLayout(false);
            this.ToolsTabPage.PerformLayout();
            this.MarkersTabPage.ResumeLayout(false);
            this.MarkersTabPage.PerformLayout();
            this.OptionsTabPage.ResumeLayout(false);
            this.OptionsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SmoothingUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ZoomSpeedUpDown)).EndInit();
            this.CalibrateTabPage.ResumeLayout(false);
            this.CalibrateTabPage.PerformLayout();
            this.SelectionPanel.ResumeLayout(false);
            this.SelectionPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox MapComboBox;
        private System.Windows.Forms.Label LoadingLabel;
        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.Button MainPanelHideButton;
        private System.Windows.Forms.Button MainPanelShowButton;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage MarkersTabPage;
        private System.Windows.Forms.TabPage OptionsTabPage;
        private System.Windows.Forms.ComboBox MarkerStyleComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView MarkersListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button CopyMarkersButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown ZoomSpeedUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown SmoothingUpDown;
        private System.Windows.Forms.TabPage ToolsTabPage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button AddMarkersButton;
        private System.Windows.Forms.TextBox MultiFindTextBox;
        private System.Windows.Forms.TextBox LocateTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox ShowLocatorCheckBox;
        private System.Windows.Forms.ComboBox LocatorStyleComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label LocatorStatusLabel;
        private System.Windows.Forms.Button GoToButton;
        private System.Windows.Forms.TabPage CalibrateTabPage;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox WorldCoordTextBox;
        private System.Windows.Forms.Label TextureFileLabel;
        private System.Windows.Forms.Label TextureNameLabel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox UnitsPerTexelXTextBox;
        private System.Windows.Forms.Button SetOriginButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox TextureOriginTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox TextureCoordTextBox;
        private System.Windows.Forms.Button SetCoordButton;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button CalibrateButton;
        private System.Windows.Forms.TextBox CalibrationPointsTextBox;
        private System.Windows.Forms.Label CalibrationErrorLabel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox UnitsPerTexelYTextBox;
        private System.Windows.Forms.Button ClearMarkersButton;
        private System.Windows.Forms.Button ResetMarkersButton;
        private System.Windows.Forms.Button GoToSelectedMarkerButton;
        private System.Windows.Forms.Panel SelectionPanel;
        private System.Windows.Forms.TextBox SelectionPositionTextBox;
        private System.Windows.Forms.TextBox SelectionNameTextBox;
    }
}