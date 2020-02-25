namespace CodeWalker.Tools
{
    partial class BinarySearchForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BinarySearchForm));
            this.FileSearchTextRadio = new System.Windows.Forms.RadioButton();
            this.FileSearchHexRadio = new System.Windows.Forms.RadioButton();
            this.FileSearchAbortButton = new System.Windows.Forms.Button();
            this.FileSearchResultsTextBox = new System.Windows.Forms.TextBox();
            this.FileSearchTextBox = new System.Windows.Forms.TextBox();
            this.FileSearchFolderBrowseButton = new System.Windows.Forms.Button();
            this.FileSearchButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.FileSearchFolderTextBox = new System.Windows.Forms.TextBox();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.SearchRPFTabPage = new System.Windows.Forms.TabPage();
            this.SearchFileSystemTab = new System.Windows.Forms.TabPage();
            this.MainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.FileSearchPanel = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.RpfSearchSaveResultsButton = new System.Windows.Forms.Button();
            this.RpfSearchIgnoreTextBox = new System.Windows.Forms.TextBox();
            this.RpfSearchIgnoreCheckBox = new System.Windows.Forms.CheckBox();
            this.RpfSearchBothDirectionsCheckBox = new System.Windows.Forms.CheckBox();
            this.RpfSearchCaseSensitiveCheckBox = new System.Windows.Forms.CheckBox();
            this.RpfSearchHexRadioButton = new System.Windows.Forms.RadioButton();
            this.RpfSearchTextRadioButton = new System.Windows.Forms.RadioButton();
            this.RpfSearchResultsListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RpfSearchAbortButton = new System.Windows.Forms.Button();
            this.RpfSearchButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.RpfSearchTextBox = new System.Windows.Forms.TextBox();
            this.RpfSearchOnlyTextBox = new System.Windows.Forms.TextBox();
            this.RpfSearchOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.ShowLargeFileContentsCheckBox = new System.Windows.Forms.CheckBox();
            this.DataHexLineCombo = new System.Windows.Forms.ComboBox();
            this.DataTextRadio = new System.Windows.Forms.RadioButton();
            this.DataHexRadio = new System.Windows.Forms.RadioButton();
            this.DataTextBox = new System.Windows.Forms.TextBox();
            this.FileInfoLabel = new System.Windows.Forms.Label();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.ExportCompressCheckBox = new System.Windows.Forms.CheckBox();
            this.ExportButton = new System.Windows.Forms.Button();
            this.MainTabControl.SuspendLayout();
            this.SearchRPFTabPage.SuspendLayout();
            this.SearchFileSystemTab.SuspendLayout();
            this.MainStatusStrip.SuspendLayout();
            this.FileSearchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // FileSearchTextRadio
            // 
            this.FileSearchTextRadio.AutoSize = true;
            this.FileSearchTextRadio.Location = new System.Drawing.Point(162, 30);
            this.FileSearchTextRadio.Name = "FileSearchTextRadio";
            this.FileSearchTextRadio.Size = new System.Drawing.Size(46, 17);
            this.FileSearchTextRadio.TabIndex = 45;
            this.FileSearchTextRadio.Text = "Text";
            this.FileSearchTextRadio.UseVisualStyleBackColor = true;
            // 
            // FileSearchHexRadio
            // 
            this.FileSearchHexRadio.AutoSize = true;
            this.FileSearchHexRadio.Checked = true;
            this.FileSearchHexRadio.Location = new System.Drawing.Point(112, 30);
            this.FileSearchHexRadio.Name = "FileSearchHexRadio";
            this.FileSearchHexRadio.Size = new System.Drawing.Size(44, 17);
            this.FileSearchHexRadio.TabIndex = 44;
            this.FileSearchHexRadio.TabStop = true;
            this.FileSearchHexRadio.Text = "Hex";
            this.FileSearchHexRadio.UseVisualStyleBackColor = true;
            // 
            // FileSearchAbortButton
            // 
            this.FileSearchAbortButton.Location = new System.Drawing.Point(420, 27);
            this.FileSearchAbortButton.Name = "FileSearchAbortButton";
            this.FileSearchAbortButton.Size = new System.Drawing.Size(75, 23);
            this.FileSearchAbortButton.TabIndex = 43;
            this.FileSearchAbortButton.Text = "Abort";
            this.FileSearchAbortButton.UseVisualStyleBackColor = true;
            this.FileSearchAbortButton.Click += new System.EventHandler(this.FileSearchAbortButton_Click);
            // 
            // FileSearchResultsTextBox
            // 
            this.FileSearchResultsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileSearchResultsTextBox.Location = new System.Drawing.Point(85, 78);
            this.FileSearchResultsTextBox.Multiline = true;
            this.FileSearchResultsTextBox.Name = "FileSearchResultsTextBox";
            this.FileSearchResultsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.FileSearchResultsTextBox.Size = new System.Drawing.Size(685, 403);
            this.FileSearchResultsTextBox.TabIndex = 42;
            this.FileSearchResultsTextBox.WordWrap = false;
            // 
            // FileSearchTextBox
            // 
            this.FileSearchTextBox.Location = new System.Drawing.Point(214, 29);
            this.FileSearchTextBox.Name = "FileSearchTextBox";
            this.FileSearchTextBox.Size = new System.Drawing.Size(119, 20);
            this.FileSearchTextBox.TabIndex = 41;
            this.FileSearchTextBox.Text = "4a03746e";
            // 
            // FileSearchFolderBrowseButton
            // 
            this.FileSearchFolderBrowseButton.Location = new System.Drawing.Point(339, 1);
            this.FileSearchFolderBrowseButton.Name = "FileSearchFolderBrowseButton";
            this.FileSearchFolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.FileSearchFolderBrowseButton.TabIndex = 40;
            this.FileSearchFolderBrowseButton.Text = "...";
            this.FileSearchFolderBrowseButton.UseVisualStyleBackColor = true;
            this.FileSearchFolderBrowseButton.Click += new System.EventHandler(this.FileSearchFolderBrowseButton_Click);
            // 
            // FileSearchButton
            // 
            this.FileSearchButton.Location = new System.Drawing.Point(339, 27);
            this.FileSearchButton.Name = "FileSearchButton";
            this.FileSearchButton.Size = new System.Drawing.Size(75, 23);
            this.FileSearchButton.TabIndex = 39;
            this.FileSearchButton.Text = "Search";
            this.FileSearchButton.UseVisualStyleBackColor = true;
            this.FileSearchButton.Click += new System.EventHandler(this.FileSearchButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 38;
            this.label2.Text = "Search folder:";
            // 
            // FileSearchFolderTextBox
            // 
            this.FileSearchFolderTextBox.Location = new System.Drawing.Point(82, 3);
            this.FileSearchFolderTextBox.Name = "FileSearchFolderTextBox";
            this.FileSearchFolderTextBox.Size = new System.Drawing.Size(251, 20);
            this.FileSearchFolderTextBox.TabIndex = 37;
            this.FileSearchFolderTextBox.Text = "Compiled944";
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTabControl.Controls.Add(this.SearchRPFTabPage);
            this.MainTabControl.Controls.Add(this.SearchFileSystemTab);
            this.MainTabControl.Location = new System.Drawing.Point(3, 5);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(846, 525);
            this.MainTabControl.TabIndex = 46;
            // 
            // SearchRPFTabPage
            // 
            this.SearchRPFTabPage.Controls.Add(this.splitContainer1);
            this.SearchRPFTabPage.Location = new System.Drawing.Point(4, 22);
            this.SearchRPFTabPage.Name = "SearchRPFTabPage";
            this.SearchRPFTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SearchRPFTabPage.Size = new System.Drawing.Size(838, 499);
            this.SearchRPFTabPage.TabIndex = 0;
            this.SearchRPFTabPage.Text = "Search RPF contents";
            this.SearchRPFTabPage.UseVisualStyleBackColor = true;
            // 
            // SearchFileSystemTab
            // 
            this.SearchFileSystemTab.Controls.Add(this.FileSearchPanel);
            this.SearchFileSystemTab.Controls.Add(this.FileSearchResultsTextBox);
            this.SearchFileSystemTab.Location = new System.Drawing.Point(4, 22);
            this.SearchFileSystemTab.Name = "SearchFileSystemTab";
            this.SearchFileSystemTab.Padding = new System.Windows.Forms.Padding(3);
            this.SearchFileSystemTab.Size = new System.Drawing.Size(849, 499);
            this.SearchFileSystemTab.TabIndex = 1;
            this.SearchFileSystemTab.Text = "Search file system";
            this.SearchFileSystemTab.UseVisualStyleBackColor = true;
            // 
            // MainStatusStrip
            // 
            this.MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.MainStatusStrip.Location = new System.Drawing.Point(0, 533);
            this.MainStatusStrip.Name = "MainStatusStrip";
            this.MainStatusStrip.Size = new System.Drawing.Size(852, 22);
            this.MainStatusStrip.TabIndex = 47;
            this.MainStatusStrip.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(837, 17);
            this.StatusLabel.Spring = true;
            this.StatusLabel.Text = "Initialising...";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FileSearchPanel
            // 
            this.FileSearchPanel.Controls.Add(this.FileSearchFolderTextBox);
            this.FileSearchPanel.Controls.Add(this.FileSearchFolderBrowseButton);
            this.FileSearchPanel.Controls.Add(this.FileSearchTextRadio);
            this.FileSearchPanel.Controls.Add(this.FileSearchTextBox);
            this.FileSearchPanel.Controls.Add(this.FileSearchButton);
            this.FileSearchPanel.Controls.Add(this.FileSearchHexRadio);
            this.FileSearchPanel.Controls.Add(this.FileSearchAbortButton);
            this.FileSearchPanel.Controls.Add(this.label2);
            this.FileSearchPanel.Location = new System.Drawing.Point(3, 6);
            this.FileSearchPanel.Name = "FileSearchPanel";
            this.FileSearchPanel.Size = new System.Drawing.Size(536, 66);
            this.FileSearchPanel.TabIndex = 46;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchOnlyTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchOnlyCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchSaveResultsButton);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchIgnoreTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchIgnoreCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchBothDirectionsCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchCaseSensitiveCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchHexRadioButton);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchTextRadioButton);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchResultsListView);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchAbortButton);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchButton);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.RpfSearchTextBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ExportCompressCheckBox);
            this.splitContainer1.Panel2.Controls.Add(this.ExportButton);
            this.splitContainer1.Panel2.Controls.Add(this.FileInfoLabel);
            this.splitContainer1.Panel2.Controls.Add(this.ShowLargeFileContentsCheckBox);
            this.splitContainer1.Panel2.Controls.Add(this.DataHexLineCombo);
            this.splitContainer1.Panel2.Controls.Add(this.DataTextRadio);
            this.splitContainer1.Panel2.Controls.Add(this.DataHexRadio);
            this.splitContainer1.Panel2.Controls.Add(this.DataTextBox);
            this.splitContainer1.Size = new System.Drawing.Size(832, 493);
            this.splitContainer1.SplitterDistance = 270;
            this.splitContainer1.TabIndex = 1;
            // 
            // RpfSearchSaveResultsButton
            // 
            this.RpfSearchSaveResultsButton.Enabled = false;
            this.RpfSearchSaveResultsButton.Location = new System.Drawing.Point(181, 118);
            this.RpfSearchSaveResultsButton.Name = "RpfSearchSaveResultsButton";
            this.RpfSearchSaveResultsButton.Size = new System.Drawing.Size(86, 22);
            this.RpfSearchSaveResultsButton.TabIndex = 62;
            this.RpfSearchSaveResultsButton.Text = "Save results...";
            this.RpfSearchSaveResultsButton.UseVisualStyleBackColor = true;
            this.RpfSearchSaveResultsButton.Click += new System.EventHandler(this.RpfSearchSaveResultsButton_Click);
            // 
            // RpfSearchIgnoreTextBox
            // 
            this.RpfSearchIgnoreTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RpfSearchIgnoreTextBox.Location = new System.Drawing.Point(68, 68);
            this.RpfSearchIgnoreTextBox.Name = "RpfSearchIgnoreTextBox";
            this.RpfSearchIgnoreTextBox.Size = new System.Drawing.Size(198, 20);
            this.RpfSearchIgnoreTextBox.TabIndex = 59;
            this.RpfSearchIgnoreTextBox.Text = ".ydr, .ydd, .ytd, .yft, .ybn, .ycd, .awc, .bik";
            // 
            // RpfSearchIgnoreCheckBox
            // 
            this.RpfSearchIgnoreCheckBox.AutoSize = true;
            this.RpfSearchIgnoreCheckBox.Checked = true;
            this.RpfSearchIgnoreCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RpfSearchIgnoreCheckBox.Location = new System.Drawing.Point(9, 70);
            this.RpfSearchIgnoreCheckBox.Name = "RpfSearchIgnoreCheckBox";
            this.RpfSearchIgnoreCheckBox.Size = new System.Drawing.Size(59, 17);
            this.RpfSearchIgnoreCheckBox.TabIndex = 58;
            this.RpfSearchIgnoreCheckBox.Text = "Ignore:";
            this.RpfSearchIgnoreCheckBox.UseVisualStyleBackColor = true;
            this.RpfSearchIgnoreCheckBox.CheckedChanged += new System.EventHandler(this.RpfSearchIgnoreCheckBox_CheckedChanged);
            // 
            // RpfSearchBothDirectionsCheckBox
            // 
            this.RpfSearchBothDirectionsCheckBox.AutoSize = true;
            this.RpfSearchBothDirectionsCheckBox.Checked = true;
            this.RpfSearchBothDirectionsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RpfSearchBothDirectionsCheckBox.Location = new System.Drawing.Point(109, 48);
            this.RpfSearchBothDirectionsCheckBox.Name = "RpfSearchBothDirectionsCheckBox";
            this.RpfSearchBothDirectionsCheckBox.Size = new System.Drawing.Size(96, 17);
            this.RpfSearchBothDirectionsCheckBox.TabIndex = 57;
            this.RpfSearchBothDirectionsCheckBox.Text = "Both directions";
            this.RpfSearchBothDirectionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // RpfSearchCaseSensitiveCheckBox
            // 
            this.RpfSearchCaseSensitiveCheckBox.AutoSize = true;
            this.RpfSearchCaseSensitiveCheckBox.Location = new System.Drawing.Point(9, 48);
            this.RpfSearchCaseSensitiveCheckBox.Name = "RpfSearchCaseSensitiveCheckBox";
            this.RpfSearchCaseSensitiveCheckBox.Size = new System.Drawing.Size(94, 17);
            this.RpfSearchCaseSensitiveCheckBox.TabIndex = 56;
            this.RpfSearchCaseSensitiveCheckBox.Text = "Case-sensitive";
            this.RpfSearchCaseSensitiveCheckBox.UseVisualStyleBackColor = true;
            // 
            // RpfSearchHexRadioButton
            // 
            this.RpfSearchHexRadioButton.AutoSize = true;
            this.RpfSearchHexRadioButton.Location = new System.Drawing.Point(158, 5);
            this.RpfSearchHexRadioButton.Name = "RpfSearchHexRadioButton";
            this.RpfSearchHexRadioButton.Size = new System.Drawing.Size(44, 17);
            this.RpfSearchHexRadioButton.TabIndex = 55;
            this.RpfSearchHexRadioButton.Text = "Hex";
            this.RpfSearchHexRadioButton.UseVisualStyleBackColor = true;
            // 
            // RpfSearchTextRadioButton
            // 
            this.RpfSearchTextRadioButton.AutoSize = true;
            this.RpfSearchTextRadioButton.Checked = true;
            this.RpfSearchTextRadioButton.Location = new System.Drawing.Point(106, 5);
            this.RpfSearchTextRadioButton.Name = "RpfSearchTextRadioButton";
            this.RpfSearchTextRadioButton.Size = new System.Drawing.Size(46, 17);
            this.RpfSearchTextRadioButton.TabIndex = 54;
            this.RpfSearchTextRadioButton.TabStop = true;
            this.RpfSearchTextRadioButton.Text = "Text";
            this.RpfSearchTextRadioButton.UseVisualStyleBackColor = true;
            this.RpfSearchTextRadioButton.CheckedChanged += new System.EventHandler(this.RpfSearchTextRadioButton_CheckedChanged);
            // 
            // RpfSearchResultsListView
            // 
            this.RpfSearchResultsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RpfSearchResultsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.RpfSearchResultsListView.FullRowSelect = true;
            this.RpfSearchResultsListView.HideSelection = false;
            this.RpfSearchResultsListView.Location = new System.Drawing.Point(3, 146);
            this.RpfSearchResultsListView.MultiSelect = false;
            this.RpfSearchResultsListView.Name = "RpfSearchResultsListView";
            this.RpfSearchResultsListView.Size = new System.Drawing.Size(263, 347);
            this.RpfSearchResultsListView.TabIndex = 63;
            this.RpfSearchResultsListView.UseCompatibleStateImageBehavior = false;
            this.RpfSearchResultsListView.View = System.Windows.Forms.View.Details;
            this.RpfSearchResultsListView.VirtualMode = true;
            this.RpfSearchResultsListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.RpfSearchResultsListView_RetrieveVirtualItem);
            this.RpfSearchResultsListView.SelectedIndexChanged += new System.EventHandler(this.RpfSearchResultsListView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "File";
            this.columnHeader1.Width = 176;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Offset";
            // 
            // RpfSearchAbortButton
            // 
            this.RpfSearchAbortButton.Location = new System.Drawing.Point(90, 118);
            this.RpfSearchAbortButton.Name = "RpfSearchAbortButton";
            this.RpfSearchAbortButton.Size = new System.Drawing.Size(75, 22);
            this.RpfSearchAbortButton.TabIndex = 61;
            this.RpfSearchAbortButton.Text = "Abort";
            this.RpfSearchAbortButton.UseVisualStyleBackColor = true;
            this.RpfSearchAbortButton.Click += new System.EventHandler(this.RpfSearchAbortButton_Click);
            // 
            // RpfSearchButton
            // 
            this.RpfSearchButton.Location = new System.Drawing.Point(9, 119);
            this.RpfSearchButton.Name = "RpfSearchButton";
            this.RpfSearchButton.Size = new System.Drawing.Size(75, 22);
            this.RpfSearchButton.TabIndex = 60;
            this.RpfSearchButton.Text = "Search";
            this.RpfSearchButton.UseVisualStyleBackColor = true;
            this.RpfSearchButton.Click += new System.EventHandler(this.RpfSearchButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 13);
            this.label3.TabIndex = 64;
            this.label3.Text = "Search in files for:";
            // 
            // RpfSearchTextBox
            // 
            this.RpfSearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RpfSearchTextBox.Location = new System.Drawing.Point(3, 23);
            this.RpfSearchTextBox.Name = "RpfSearchTextBox";
            this.RpfSearchTextBox.Size = new System.Drawing.Size(263, 20);
            this.RpfSearchTextBox.TabIndex = 53;
            // 
            // RpfSearchOnlyTextBox
            // 
            this.RpfSearchOnlyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RpfSearchOnlyTextBox.Enabled = false;
            this.RpfSearchOnlyTextBox.Location = new System.Drawing.Point(68, 92);
            this.RpfSearchOnlyTextBox.Name = "RpfSearchOnlyTextBox";
            this.RpfSearchOnlyTextBox.Size = new System.Drawing.Size(198, 20);
            this.RpfSearchOnlyTextBox.TabIndex = 66;
            this.RpfSearchOnlyTextBox.Text = ".ysc, .rel";
            // 
            // RpfSearchOnlyCheckBox
            // 
            this.RpfSearchOnlyCheckBox.AutoSize = true;
            this.RpfSearchOnlyCheckBox.Location = new System.Drawing.Point(9, 94);
            this.RpfSearchOnlyCheckBox.Name = "RpfSearchOnlyCheckBox";
            this.RpfSearchOnlyCheckBox.Size = new System.Drawing.Size(50, 17);
            this.RpfSearchOnlyCheckBox.TabIndex = 65;
            this.RpfSearchOnlyCheckBox.Text = "Only:";
            this.RpfSearchOnlyCheckBox.UseVisualStyleBackColor = true;
            this.RpfSearchOnlyCheckBox.CheckedChanged += new System.EventHandler(this.RpfSearchOnlyCheckBox_CheckedChanged);
            // 
            // ShowLargeFileContentsCheckBox
            // 
            this.ShowLargeFileContentsCheckBox.AutoSize = true;
            this.ShowLargeFileContentsCheckBox.Location = new System.Drawing.Point(392, 62);
            this.ShowLargeFileContentsCheckBox.Name = "ShowLargeFileContentsCheckBox";
            this.ShowLargeFileContentsCheckBox.Size = new System.Drawing.Size(139, 17);
            this.ShowLargeFileContentsCheckBox.TabIndex = 109;
            this.ShowLargeFileContentsCheckBox.Text = "Show large file contents";
            this.ShowLargeFileContentsCheckBox.UseVisualStyleBackColor = true;
            this.ShowLargeFileContentsCheckBox.CheckedChanged += new System.EventHandler(this.ShowLargeFileContentsCheckBox_CheckedChanged);
            // 
            // DataHexLineCombo
            // 
            this.DataHexLineCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DataHexLineCombo.FormattingEnabled = true;
            this.DataHexLineCombo.Items.AddRange(new object[] {
            "8",
            "16",
            "32"});
            this.DataHexLineCombo.Location = new System.Drawing.Point(56, 60);
            this.DataHexLineCombo.Name = "DataHexLineCombo";
            this.DataHexLineCombo.Size = new System.Drawing.Size(49, 21);
            this.DataHexLineCombo.TabIndex = 106;
            this.DataHexLineCombo.SelectedIndexChanged += new System.EventHandler(this.DataHexLineCombo_SelectedIndexChanged);
            // 
            // DataTextRadio
            // 
            this.DataTextRadio.AutoSize = true;
            this.DataTextRadio.Location = new System.Drawing.Point(135, 61);
            this.DataTextRadio.Name = "DataTextRadio";
            this.DataTextRadio.Size = new System.Drawing.Size(46, 17);
            this.DataTextRadio.TabIndex = 107;
            this.DataTextRadio.Text = "Text";
            this.DataTextRadio.UseVisualStyleBackColor = true;
            // 
            // DataHexRadio
            // 
            this.DataHexRadio.AutoSize = true;
            this.DataHexRadio.Checked = true;
            this.DataHexRadio.Location = new System.Drawing.Point(6, 61);
            this.DataHexRadio.Name = "DataHexRadio";
            this.DataHexRadio.Size = new System.Drawing.Size(44, 17);
            this.DataHexRadio.TabIndex = 105;
            this.DataHexRadio.TabStop = true;
            this.DataHexRadio.Text = "Hex";
            this.DataHexRadio.UseVisualStyleBackColor = true;
            this.DataHexRadio.CheckedChanged += new System.EventHandler(this.DataHexRadio_CheckedChanged);
            // 
            // DataTextBox
            // 
            this.DataTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DataTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DataTextBox.HideSelection = false;
            this.DataTextBox.Location = new System.Drawing.Point(4, 92);
            this.DataTextBox.Multiline = true;
            this.DataTextBox.Name = "DataTextBox";
            this.DataTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.DataTextBox.Size = new System.Drawing.Size(552, 401);
            this.DataTextBox.TabIndex = 108;
            this.DataTextBox.Text = "[Please select a search result]";
            this.DataTextBox.WordWrap = false;
            // 
            // FileInfoLabel
            // 
            this.FileInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileInfoLabel.AutoEllipsis = true;
            this.FileInfoLabel.Location = new System.Drawing.Point(3, 7);
            this.FileInfoLabel.Name = "FileInfoLabel";
            this.FileInfoLabel.Size = new System.Drawing.Size(549, 16);
            this.FileInfoLabel.TabIndex = 110;
            this.FileInfoLabel.Text = "[Nothing selected]";
            // 
            // SaveFileDialog
            // 
            this.SaveFileDialog.AddExtension = false;
            // 
            // ExportCompressCheckBox
            // 
            this.ExportCompressCheckBox.AutoSize = true;
            this.ExportCompressCheckBox.Location = new System.Drawing.Point(87, 35);
            this.ExportCompressCheckBox.Name = "ExportCompressCheckBox";
            this.ExportCompressCheckBox.Size = new System.Drawing.Size(104, 17);
            this.ExportCompressCheckBox.TabIndex = 112;
            this.ExportCompressCheckBox.Text = "Compress export";
            this.ExportCompressCheckBox.UseVisualStyleBackColor = true;
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(6, 31);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(75, 23);
            this.ExportButton.TabIndex = 111;
            this.ExportButton.Text = "Export...";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // BinarySearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(852, 555);
            this.Controls.Add(this.MainStatusStrip);
            this.Controls.Add(this.MainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BinarySearchForm";
            this.Text = "Binary Search - CodeWalker by dexyfex";
            this.Load += new System.EventHandler(this.BinarySearchForm_Load);
            this.MainTabControl.ResumeLayout(false);
            this.SearchRPFTabPage.ResumeLayout(false);
            this.SearchFileSystemTab.ResumeLayout(false);
            this.SearchFileSystemTab.PerformLayout();
            this.MainStatusStrip.ResumeLayout(false);
            this.MainStatusStrip.PerformLayout();
            this.FileSearchPanel.ResumeLayout(false);
            this.FileSearchPanel.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton FileSearchTextRadio;
        private System.Windows.Forms.RadioButton FileSearchHexRadio;
        private System.Windows.Forms.Button FileSearchAbortButton;
        private System.Windows.Forms.TextBox FileSearchResultsTextBox;
        private System.Windows.Forms.TextBox FileSearchTextBox;
        private System.Windows.Forms.Button FileSearchFolderBrowseButton;
        private System.Windows.Forms.Button FileSearchButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox FileSearchFolderTextBox;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage SearchRPFTabPage;
        private System.Windows.Forms.TabPage SearchFileSystemTab;
        private System.Windows.Forms.StatusStrip MainStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.Panel FileSearchPanel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button RpfSearchSaveResultsButton;
        private System.Windows.Forms.TextBox RpfSearchIgnoreTextBox;
        private System.Windows.Forms.CheckBox RpfSearchIgnoreCheckBox;
        private System.Windows.Forms.CheckBox RpfSearchBothDirectionsCheckBox;
        private System.Windows.Forms.CheckBox RpfSearchCaseSensitiveCheckBox;
        private System.Windows.Forms.RadioButton RpfSearchHexRadioButton;
        private System.Windows.Forms.RadioButton RpfSearchTextRadioButton;
        private System.Windows.Forms.ListView RpfSearchResultsListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button RpfSearchAbortButton;
        private System.Windows.Forms.Button RpfSearchButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox RpfSearchTextBox;
        private System.Windows.Forms.TextBox RpfSearchOnlyTextBox;
        private System.Windows.Forms.CheckBox RpfSearchOnlyCheckBox;
        private System.Windows.Forms.CheckBox ShowLargeFileContentsCheckBox;
        private System.Windows.Forms.ComboBox DataHexLineCombo;
        private System.Windows.Forms.RadioButton DataTextRadio;
        private System.Windows.Forms.RadioButton DataHexRadio;
        private System.Windows.Forms.TextBox DataTextBox;
        private System.Windows.Forms.Label FileInfoLabel;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
        private System.Windows.Forms.CheckBox ExportCompressCheckBox;
        private System.Windows.Forms.Button ExportButton;
    }
}