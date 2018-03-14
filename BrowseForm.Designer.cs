namespace CodeWalker
{
    partial class BrowseForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrowseForm));
            this.label1 = new System.Windows.Forms.Label();
            this.FolderBrowseButton = new System.Windows.Forms.Button();
            this.FolderTextBox = new System.Windows.Forms.TextBox();
            this.ScanButton = new System.Windows.Forms.Button();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.FlattenStructureCheckBox = new System.Windows.Forms.CheckBox();
            this.FindButton = new System.Windows.Forms.Button();
            this.MainTreeView = new System.Windows.Forms.TreeView();
            this.label2 = new System.Windows.Forms.Label();
            this.FindTextBox = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.SearchSaveResultsButton = new System.Windows.Forms.Button();
            this.SearchIgnoreTextBox = new System.Windows.Forms.TextBox();
            this.SearchIgnoreCheckBox = new System.Windows.Forms.CheckBox();
            this.SearchBothDirectionsCheckBox = new System.Windows.Forms.CheckBox();
            this.SearchCaseSensitiveCheckBox = new System.Windows.Forms.CheckBox();
            this.SearchHexRadioButton = new System.Windows.Forms.RadioButton();
            this.SearchTextRadioButton = new System.Windows.Forms.RadioButton();
            this.SearchResultsListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SearchAbortButton = new System.Windows.Forms.Button();
            this.SearchButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.SearchTextBox = new System.Windows.Forms.TextBox();
            this.ExportCompressCheckBox = new System.Windows.Forms.CheckBox();
            this.ExportButton = new System.Windows.Forms.Button();
            this.FileInfoLabel = new System.Windows.Forms.Label();
            this.SelectionTabControl = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.ShowLargeFileContentsCheckBox = new System.Windows.Forms.CheckBox();
            this.DataHexLineCombo = new System.Windows.Forms.ComboBox();
            this.DataTextRadio = new System.Windows.Forms.RadioButton();
            this.DataHexRadio = new System.Windows.Forms.RadioButton();
            this.DataTextBox = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.DetailsPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.TexturesTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.SelTexturesListView = new System.Windows.Forms.ListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SelTextureMipLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SelTextureDimensionsLabel = new System.Windows.Forms.Label();
            this.SelTextureMipTrackBar = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.SelTextureNameTextBox = new System.Windows.Forms.TextBox();
            this.SelTexturePictureBox = new System.Windows.Forms.PictureBox();
            this.MainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.TestAllButton = new System.Windows.Forms.Button();
            this.AbortButton = new System.Windows.Forms.Button();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SelectionTabControl.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.TexturesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SelTextureMipTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SelTexturePictureBox)).BeginInit();
            this.MainStatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 47;
            this.label1.Text = "GTAV folder:";
            // 
            // FolderBrowseButton
            // 
            this.FolderBrowseButton.Location = new System.Drawing.Point(347, 4);
            this.FolderBrowseButton.Name = "FolderBrowseButton";
            this.FolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.FolderBrowseButton.TabIndex = 2;
            this.FolderBrowseButton.Text = "...";
            this.FolderBrowseButton.UseVisualStyleBackColor = true;
            this.FolderBrowseButton.Click += new System.EventHandler(this.FolderBrowseButton_Click);
            // 
            // FolderTextBox
            // 
            this.FolderTextBox.Location = new System.Drawing.Point(90, 6);
            this.FolderTextBox.Name = "FolderTextBox";
            this.FolderTextBox.ReadOnly = true;
            this.FolderTextBox.Size = new System.Drawing.Size(251, 20);
            this.FolderTextBox.TabIndex = 1;
            // 
            // ScanButton
            // 
            this.ScanButton.Location = new System.Drawing.Point(380, 4);
            this.ScanButton.Name = "ScanButton";
            this.ScanButton.Size = new System.Drawing.Size(75, 23);
            this.ScanButton.TabIndex = 0;
            this.ScanButton.Text = "Scan";
            this.ScanButton.UseVisualStyleBackColor = true;
            this.ScanButton.Click += new System.EventHandler(this.ScanButton_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 32);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ExportCompressCheckBox);
            this.splitContainer1.Panel2.Controls.Add(this.ExportButton);
            this.splitContainer1.Panel2.Controls.Add(this.FileInfoLabel);
            this.splitContainer1.Panel2.Controls.Add(this.SelectionTabControl);
            this.splitContainer1.Size = new System.Drawing.Size(855, 492);
            this.splitContainer1.SplitterDistance = 281;
            this.splitContainer1.TabIndex = 5;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(278, 489);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.FlattenStructureCheckBox);
            this.tabPage1.Controls.Add(this.FindButton);
            this.tabPage1.Controls.Add(this.MainTreeView);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.FindTextBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(270, 463);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Browse";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // FlattenStructureCheckBox
            // 
            this.FlattenStructureCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.FlattenStructureCheckBox.AutoSize = true;
            this.FlattenStructureCheckBox.Location = new System.Drawing.Point(3, 443);
            this.FlattenStructureCheckBox.Name = "FlattenStructureCheckBox";
            this.FlattenStructureCheckBox.Size = new System.Drawing.Size(128, 17);
            this.FlattenStructureCheckBox.TabIndex = 50;
            this.FlattenStructureCheckBox.Text = "Flatten RPF Structure";
            this.FlattenStructureCheckBox.UseVisualStyleBackColor = true;
            this.FlattenStructureCheckBox.CheckedChanged += new System.EventHandler(this.FlattenStructureCheckBox_CheckedChanged);
            // 
            // FindButton
            // 
            this.FindButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FindButton.Location = new System.Drawing.Point(240, 4);
            this.FindButton.Name = "FindButton";
            this.FindButton.Size = new System.Drawing.Size(27, 22);
            this.FindButton.TabIndex = 11;
            this.FindButton.Text = ">";
            this.FindButton.UseVisualStyleBackColor = true;
            this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
            // 
            // MainTreeView
            // 
            this.MainTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTreeView.HideSelection = false;
            this.MainTreeView.Location = new System.Drawing.Point(0, 32);
            this.MainTreeView.Name = "MainTreeView";
            this.MainTreeView.Size = new System.Drawing.Size(267, 408);
            this.MainTreeView.TabIndex = 12;
            this.MainTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.MainTreeView_AfterSelect);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 49;
            this.label2.Text = "Find:";
            // 
            // FindTextBox
            // 
            this.FindTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FindTextBox.Location = new System.Drawing.Point(38, 5);
            this.FindTextBox.Name = "FindTextBox";
            this.FindTextBox.Size = new System.Drawing.Size(199, 20);
            this.FindTextBox.TabIndex = 10;
            this.FindTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FindTextBox_KeyPress);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.SearchSaveResultsButton);
            this.tabPage2.Controls.Add(this.SearchIgnoreTextBox);
            this.tabPage2.Controls.Add(this.SearchIgnoreCheckBox);
            this.tabPage2.Controls.Add(this.SearchBothDirectionsCheckBox);
            this.tabPage2.Controls.Add(this.SearchCaseSensitiveCheckBox);
            this.tabPage2.Controls.Add(this.SearchHexRadioButton);
            this.tabPage2.Controls.Add(this.SearchTextRadioButton);
            this.tabPage2.Controls.Add(this.SearchResultsListView);
            this.tabPage2.Controls.Add(this.SearchAbortButton);
            this.tabPage2.Controls.Add(this.SearchButton);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.SearchTextBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(270, 463);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Search";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // SearchSaveResultsButton
            // 
            this.SearchSaveResultsButton.Enabled = false;
            this.SearchSaveResultsButton.Location = new System.Drawing.Point(178, 95);
            this.SearchSaveResultsButton.Name = "SearchSaveResultsButton";
            this.SearchSaveResultsButton.Size = new System.Drawing.Size(86, 22);
            this.SearchSaveResultsButton.TabIndex = 19;
            this.SearchSaveResultsButton.Text = "Save results...";
            this.SearchSaveResultsButton.UseVisualStyleBackColor = true;
            this.SearchSaveResultsButton.Click += new System.EventHandler(this.SearchSaveResultsButton_Click);
            // 
            // SearchIgnoreTextBox
            // 
            this.SearchIgnoreTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchIgnoreTextBox.Location = new System.Drawing.Point(65, 70);
            this.SearchIgnoreTextBox.Name = "SearchIgnoreTextBox";
            this.SearchIgnoreTextBox.Size = new System.Drawing.Size(202, 20);
            this.SearchIgnoreTextBox.TabIndex = 16;
            this.SearchIgnoreTextBox.Text = ".ydr, .ydd, .ytd, .yft, .ybn, .ycd, .awc, .bik";
            // 
            // SearchIgnoreCheckBox
            // 
            this.SearchIgnoreCheckBox.AutoSize = true;
            this.SearchIgnoreCheckBox.Checked = true;
            this.SearchIgnoreCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SearchIgnoreCheckBox.Location = new System.Drawing.Point(6, 72);
            this.SearchIgnoreCheckBox.Name = "SearchIgnoreCheckBox";
            this.SearchIgnoreCheckBox.Size = new System.Drawing.Size(59, 17);
            this.SearchIgnoreCheckBox.TabIndex = 15;
            this.SearchIgnoreCheckBox.Text = "Ignore:";
            this.SearchIgnoreCheckBox.UseVisualStyleBackColor = true;
            this.SearchIgnoreCheckBox.CheckedChanged += new System.EventHandler(this.SearchIgnoreCheckBox_CheckedChanged);
            // 
            // SearchBothDirectionsCheckBox
            // 
            this.SearchBothDirectionsCheckBox.AutoSize = true;
            this.SearchBothDirectionsCheckBox.Checked = true;
            this.SearchBothDirectionsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SearchBothDirectionsCheckBox.Location = new System.Drawing.Point(106, 50);
            this.SearchBothDirectionsCheckBox.Name = "SearchBothDirectionsCheckBox";
            this.SearchBothDirectionsCheckBox.Size = new System.Drawing.Size(96, 17);
            this.SearchBothDirectionsCheckBox.TabIndex = 14;
            this.SearchBothDirectionsCheckBox.Text = "Both directions";
            this.SearchBothDirectionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // SearchCaseSensitiveCheckBox
            // 
            this.SearchCaseSensitiveCheckBox.AutoSize = true;
            this.SearchCaseSensitiveCheckBox.Location = new System.Drawing.Point(6, 50);
            this.SearchCaseSensitiveCheckBox.Name = "SearchCaseSensitiveCheckBox";
            this.SearchCaseSensitiveCheckBox.Size = new System.Drawing.Size(94, 17);
            this.SearchCaseSensitiveCheckBox.TabIndex = 13;
            this.SearchCaseSensitiveCheckBox.Text = "Case-sensitive";
            this.SearchCaseSensitiveCheckBox.UseVisualStyleBackColor = true;
            // 
            // SearchHexRadioButton
            // 
            this.SearchHexRadioButton.AutoSize = true;
            this.SearchHexRadioButton.Location = new System.Drawing.Point(155, 7);
            this.SearchHexRadioButton.Name = "SearchHexRadioButton";
            this.SearchHexRadioButton.Size = new System.Drawing.Size(44, 17);
            this.SearchHexRadioButton.TabIndex = 12;
            this.SearchHexRadioButton.Text = "Hex";
            this.SearchHexRadioButton.UseVisualStyleBackColor = true;
            this.SearchHexRadioButton.CheckedChanged += new System.EventHandler(this.SearchHexRadioButton_CheckedChanged);
            // 
            // SearchTextRadioButton
            // 
            this.SearchTextRadioButton.AutoSize = true;
            this.SearchTextRadioButton.Checked = true;
            this.SearchTextRadioButton.Location = new System.Drawing.Point(103, 7);
            this.SearchTextRadioButton.Name = "SearchTextRadioButton";
            this.SearchTextRadioButton.Size = new System.Drawing.Size(46, 17);
            this.SearchTextRadioButton.TabIndex = 11;
            this.SearchTextRadioButton.TabStop = true;
            this.SearchTextRadioButton.Text = "Text";
            this.SearchTextRadioButton.UseVisualStyleBackColor = true;
            this.SearchTextRadioButton.CheckedChanged += new System.EventHandler(this.SearchTextRadioButton_CheckedChanged);
            // 
            // SearchResultsListView
            // 
            this.SearchResultsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchResultsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.SearchResultsListView.FullRowSelect = true;
            this.SearchResultsListView.HideSelection = false;
            this.SearchResultsListView.Location = new System.Drawing.Point(0, 123);
            this.SearchResultsListView.MultiSelect = false;
            this.SearchResultsListView.Name = "SearchResultsListView";
            this.SearchResultsListView.Size = new System.Drawing.Size(267, 340);
            this.SearchResultsListView.TabIndex = 20;
            this.SearchResultsListView.UseCompatibleStateImageBehavior = false;
            this.SearchResultsListView.View = System.Windows.Forms.View.Details;
            this.SearchResultsListView.VirtualMode = true;
            this.SearchResultsListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.SearchResultsListView_RetrieveVirtualItem);
            this.SearchResultsListView.SelectedIndexChanged += new System.EventHandler(this.SearchResultsListView_SelectedIndexChanged);
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
            // SearchAbortButton
            // 
            this.SearchAbortButton.Location = new System.Drawing.Point(87, 95);
            this.SearchAbortButton.Name = "SearchAbortButton";
            this.SearchAbortButton.Size = new System.Drawing.Size(75, 22);
            this.SearchAbortButton.TabIndex = 18;
            this.SearchAbortButton.Text = "Abort";
            this.SearchAbortButton.UseVisualStyleBackColor = true;
            this.SearchAbortButton.Click += new System.EventHandler(this.SearchAbortButton_Click);
            // 
            // SearchButton
            // 
            this.SearchButton.Location = new System.Drawing.Point(6, 95);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(75, 22);
            this.SearchButton.TabIndex = 17;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 13);
            this.label3.TabIndex = 52;
            this.label3.Text = "Search in files for:";
            // 
            // SearchTextBox
            // 
            this.SearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchTextBox.Location = new System.Drawing.Point(3, 27);
            this.SearchTextBox.Name = "SearchTextBox";
            this.SearchTextBox.Size = new System.Drawing.Size(264, 20);
            this.SearchTextBox.TabIndex = 10;
            // 
            // ExportCompressCheckBox
            // 
            this.ExportCompressCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportCompressCheckBox.AutoSize = true;
            this.ExportCompressCheckBox.Location = new System.Drawing.Point(414, 5);
            this.ExportCompressCheckBox.Name = "ExportCompressCheckBox";
            this.ExportCompressCheckBox.Size = new System.Drawing.Size(72, 17);
            this.ExportCompressCheckBox.TabIndex = 105;
            this.ExportCompressCheckBox.Text = "Compress";
            this.ExportCompressCheckBox.UseVisualStyleBackColor = true;
            // 
            // ExportButton
            // 
            this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportButton.Location = new System.Drawing.Point(488, 2);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(75, 23);
            this.ExportButton.TabIndex = 104;
            this.ExportButton.Text = "Export...";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // FileInfoLabel
            // 
            this.FileInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileInfoLabel.AutoEllipsis = true;
            this.FileInfoLabel.Location = new System.Drawing.Point(7, 6);
            this.FileInfoLabel.Name = "FileInfoLabel";
            this.FileInfoLabel.Size = new System.Drawing.Size(401, 16);
            this.FileInfoLabel.TabIndex = 51;
            this.FileInfoLabel.Text = "[Nothing selected]";
            // 
            // SelectionTabControl
            // 
            this.SelectionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectionTabControl.Controls.Add(this.tabPage3);
            this.SelectionTabControl.Controls.Add(this.tabPage4);
            this.SelectionTabControl.Controls.Add(this.TexturesTabPage);
            this.SelectionTabControl.Location = new System.Drawing.Point(3, 30);
            this.SelectionTabControl.Name = "SelectionTabControl";
            this.SelectionTabControl.SelectedIndex = 0;
            this.SelectionTabControl.Size = new System.Drawing.Size(564, 462);
            this.SelectionTabControl.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.ShowLargeFileContentsCheckBox);
            this.tabPage3.Controls.Add(this.DataHexLineCombo);
            this.tabPage3.Controls.Add(this.DataTextRadio);
            this.tabPage3.Controls.Add(this.DataHexRadio);
            this.tabPage3.Controls.Add(this.DataTextBox);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(556, 436);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Data";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // ShowLargeFileContentsCheckBox
            // 
            this.ShowLargeFileContentsCheckBox.AutoSize = true;
            this.ShowLargeFileContentsCheckBox.Location = new System.Drawing.Point(395, 7);
            this.ShowLargeFileContentsCheckBox.Name = "ShowLargeFileContentsCheckBox";
            this.ShowLargeFileContentsCheckBox.Size = new System.Drawing.Size(139, 17);
            this.ShowLargeFileContentsCheckBox.TabIndex = 104;
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
            this.DataHexLineCombo.Location = new System.Drawing.Point(56, 5);
            this.DataHexLineCombo.Name = "DataHexLineCombo";
            this.DataHexLineCombo.Size = new System.Drawing.Size(49, 21);
            this.DataHexLineCombo.TabIndex = 101;
            this.DataHexLineCombo.SelectedIndexChanged += new System.EventHandler(this.DataHexLineCombo_SelectedIndexChanged);
            // 
            // DataTextRadio
            // 
            this.DataTextRadio.AutoSize = true;
            this.DataTextRadio.Location = new System.Drawing.Point(176, 6);
            this.DataTextRadio.Name = "DataTextRadio";
            this.DataTextRadio.Size = new System.Drawing.Size(46, 17);
            this.DataTextRadio.TabIndex = 102;
            this.DataTextRadio.Text = "Text";
            this.DataTextRadio.UseVisualStyleBackColor = true;
            this.DataTextRadio.CheckedChanged += new System.EventHandler(this.DataTextRadio_CheckedChanged);
            // 
            // DataHexRadio
            // 
            this.DataHexRadio.AutoSize = true;
            this.DataHexRadio.Checked = true;
            this.DataHexRadio.Location = new System.Drawing.Point(6, 6);
            this.DataHexRadio.Name = "DataHexRadio";
            this.DataHexRadio.Size = new System.Drawing.Size(44, 17);
            this.DataHexRadio.TabIndex = 100;
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
            this.DataTextBox.Location = new System.Drawing.Point(6, 29);
            this.DataTextBox.Multiline = true;
            this.DataTextBox.Name = "DataTextBox";
            this.DataTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.DataTextBox.Size = new System.Drawing.Size(544, 401);
            this.DataTextBox.TabIndex = 103;
            this.DataTextBox.Text = "[Please select a data file]";
            this.DataTextBox.WordWrap = false;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.DetailsPropertyGrid);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(556, 436);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "Details";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // DetailsPropertyGrid
            // 
            this.DetailsPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DetailsPropertyGrid.Location = new System.Drawing.Point(6, 6);
            this.DetailsPropertyGrid.Name = "DetailsPropertyGrid";
            this.DetailsPropertyGrid.Size = new System.Drawing.Size(544, 424);
            this.DetailsPropertyGrid.TabIndex = 104;
            // 
            // TexturesTabPage
            // 
            this.TexturesTabPage.Controls.Add(this.splitContainer2);
            this.TexturesTabPage.Location = new System.Drawing.Point(4, 22);
            this.TexturesTabPage.Name = "TexturesTabPage";
            this.TexturesTabPage.Size = new System.Drawing.Size(556, 436);
            this.TexturesTabPage.TabIndex = 2;
            this.TexturesTabPage.Text = "Textures";
            this.TexturesTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.SelTexturesListView);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.SelTextureMipLabel);
            this.splitContainer2.Panel2.Controls.Add(this.label5);
            this.splitContainer2.Panel2.Controls.Add(this.SelTextureDimensionsLabel);
            this.splitContainer2.Panel2.Controls.Add(this.SelTextureMipTrackBar);
            this.splitContainer2.Panel2.Controls.Add(this.label4);
            this.splitContainer2.Panel2.Controls.Add(this.SelTextureNameTextBox);
            this.splitContainer2.Panel2.Controls.Add(this.SelTexturePictureBox);
            this.splitContainer2.Size = new System.Drawing.Size(556, 436);
            this.splitContainer2.SplitterDistance = 177;
            this.splitContainer2.TabIndex = 0;
            // 
            // SelTexturesListView
            // 
            this.SelTexturesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelTexturesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5});
            this.SelTexturesListView.FullRowSelect = true;
            this.SelTexturesListView.HideSelection = false;
            this.SelTexturesListView.Location = new System.Drawing.Point(3, 3);
            this.SelTexturesListView.MultiSelect = false;
            this.SelTexturesListView.Name = "SelTexturesListView";
            this.SelTexturesListView.Size = new System.Drawing.Size(171, 430);
            this.SelTexturesListView.TabIndex = 21;
            this.SelTexturesListView.UseCompatibleStateImageBehavior = false;
            this.SelTexturesListView.View = System.Windows.Forms.View.Details;
            this.SelTexturesListView.SelectedIndexChanged += new System.EventHandler(this.SelTexturesListView_SelectedIndexChanged);
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Texture";
            this.columnHeader5.Width = 146;
            // 
            // SelTextureMipLabel
            // 
            this.SelTextureMipLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelTextureMipLabel.AutoSize = true;
            this.SelTextureMipLabel.Location = new System.Drawing.Point(41, 401);
            this.SelTextureMipLabel.Name = "SelTextureMipLabel";
            this.SelTextureMipLabel.Size = new System.Drawing.Size(13, 13);
            this.SelTextureMipLabel.TabIndex = 44;
            this.SelTextureMipLabel.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 43;
            this.label5.Text = "Name:";
            // 
            // SelTextureDimensionsLabel
            // 
            this.SelTextureDimensionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SelTextureDimensionsLabel.AutoSize = true;
            this.SelTextureDimensionsLabel.Location = new System.Drawing.Point(301, 401);
            this.SelTextureDimensionsLabel.Name = "SelTextureDimensionsLabel";
            this.SelTextureDimensionsLabel.Size = new System.Drawing.Size(10, 13);
            this.SelTextureDimensionsLabel.TabIndex = 42;
            this.SelTextureDimensionsLabel.Text = "-";
            // 
            // SelTextureMipTrackBar
            // 
            this.SelTextureMipTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelTextureMipTrackBar.AutoSize = false;
            this.SelTextureMipTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.SelTextureMipTrackBar.LargeChange = 1;
            this.SelTextureMipTrackBar.Location = new System.Drawing.Point(60, 395);
            this.SelTextureMipTrackBar.Maximum = 0;
            this.SelTextureMipTrackBar.Name = "SelTextureMipTrackBar";
            this.SelTextureMipTrackBar.Size = new System.Drawing.Size(234, 31);
            this.SelTextureMipTrackBar.TabIndex = 41;
            this.SelTextureMipTrackBar.Scroll += new System.EventHandler(this.SelTextureMipTrackBar_Scroll);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 401);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 40;
            this.label4.Text = "Mip:";
            // 
            // SelTextureNameTextBox
            // 
            this.SelTextureNameTextBox.Location = new System.Drawing.Point(60, 7);
            this.SelTextureNameTextBox.Name = "SelTextureNameTextBox";
            this.SelTextureNameTextBox.Size = new System.Drawing.Size(192, 20);
            this.SelTextureNameTextBox.TabIndex = 39;
            // 
            // SelTexturePictureBox
            // 
            this.SelTexturePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelTexturePictureBox.BackColor = System.Drawing.Color.DarkGray;
            this.SelTexturePictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SelTexturePictureBox.Location = new System.Drawing.Point(3, 33);
            this.SelTexturePictureBox.Name = "SelTexturePictureBox";
            this.SelTexturePictureBox.Size = new System.Drawing.Size(369, 351);
            this.SelTexturePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.SelTexturePictureBox.TabIndex = 38;
            this.SelTexturePictureBox.TabStop = false;
            // 
            // MainStatusStrip
            // 
            this.MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.MainStatusStrip.Location = new System.Drawing.Point(0, 527);
            this.MainStatusStrip.Name = "MainStatusStrip";
            this.MainStatusStrip.Size = new System.Drawing.Size(855, 22);
            this.MainStatusStrip.TabIndex = 50;
            this.MainStatusStrip.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(840, 17);
            this.StatusLabel.Spring = true;
            this.StatusLabel.Text = "Scan the GTAV folder to begin.";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TestAllButton
            // 
            this.TestAllButton.Location = new System.Drawing.Point(537, 4);
            this.TestAllButton.Name = "TestAllButton";
            this.TestAllButton.Size = new System.Drawing.Size(75, 23);
            this.TestAllButton.TabIndex = 3;
            this.TestAllButton.Text = "Test all files";
            this.TestAllButton.UseVisualStyleBackColor = true;
            this.TestAllButton.Click += new System.EventHandler(this.TestAllButton_Click);
            // 
            // AbortButton
            // 
            this.AbortButton.Location = new System.Drawing.Point(618, 4);
            this.AbortButton.Name = "AbortButton";
            this.AbortButton.Size = new System.Drawing.Size(75, 23);
            this.AbortButton.TabIndex = 4;
            this.AbortButton.Text = "Abort";
            this.AbortButton.UseVisualStyleBackColor = true;
            this.AbortButton.Click += new System.EventHandler(this.AbortButton_Click);
            // 
            // SaveFileDialog
            // 
            this.SaveFileDialog.AddExtension = false;
            // 
            // BrowseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(855, 549);
            this.Controls.Add(this.AbortButton);
            this.Controls.Add(this.TestAllButton);
            this.Controls.Add(this.MainStatusStrip);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.ScanButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.FolderBrowseButton);
            this.Controls.Add(this.FolderTextBox);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BrowseForm";
            this.Text = "RPF Browser - CodeWalker by dexyfex";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BrowseForm_FormClosed);
            this.Load += new System.EventHandler(this.BrowseForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.SelectionTabControl.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.TexturesTabPage.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SelTextureMipTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SelTexturePictureBox)).EndInit();
            this.MainStatusStrip.ResumeLayout(false);
            this.MainStatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button FolderBrowseButton;
        private System.Windows.Forms.TextBox FolderTextBox;
        private System.Windows.Forms.Button ScanButton;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView MainTreeView;
        private System.Windows.Forms.StatusStrip MainStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabControl SelectionTabControl;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.RadioButton DataTextRadio;
        private System.Windows.Forms.RadioButton DataHexRadio;
        private System.Windows.Forms.TextBox DataTextBox;
        private System.Windows.Forms.ComboBox DataHexLineCombo;
        private System.Windows.Forms.Label FileInfoLabel;
        private System.Windows.Forms.Button TestAllButton;
        private System.Windows.Forms.Button AbortButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox FindTextBox;
        private System.Windows.Forms.Button FindButton;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
        private CodeWalker.WinForms.PropertyGridFix DetailsPropertyGrid;
        private System.Windows.Forms.Button SearchAbortButton;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox SearchTextBox;
        private System.Windows.Forms.RadioButton SearchHexRadioButton;
        private System.Windows.Forms.RadioButton SearchTextRadioButton;
        private System.Windows.Forms.ListView SearchResultsListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.CheckBox SearchBothDirectionsCheckBox;
        private System.Windows.Forms.CheckBox SearchCaseSensitiveCheckBox;
        private System.Windows.Forms.TextBox SearchIgnoreTextBox;
        private System.Windows.Forms.CheckBox SearchIgnoreCheckBox;
        private System.Windows.Forms.TabPage TexturesTabPage;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label SelTextureDimensionsLabel;
        private System.Windows.Forms.TrackBar SelTextureMipTrackBar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox SelTextureNameTextBox;
        private System.Windows.Forms.PictureBox SelTexturePictureBox;
        private System.Windows.Forms.ListView SelTexturesListView;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label SelTextureMipLabel;
        private System.Windows.Forms.CheckBox ShowLargeFileContentsCheckBox;
        private System.Windows.Forms.CheckBox ExportCompressCheckBox;
        private System.Windows.Forms.CheckBox FlattenStructureCheckBox;
        private System.Windows.Forms.Button SearchSaveResultsButton;
    }
}