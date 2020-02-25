namespace CodeWalker.World
{
    partial class WorldSearchForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorldSearchForm));
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.EntitySearchTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.EntityResultsListView = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.EntityResultPanel = new System.Windows.Forms.Panel();
            this.EntityResultViewModelButton = new System.Windows.Forms.Button();
            this.EntityResultPropertyGrid = new CodeWalker.WinForms.ReadOnlyPropertyGrid();
            this.label7 = new System.Windows.Forms.Label();
            this.EntityResultGoToButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.EntityResultYmapTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.EntityResultNameTextBox = new System.Windows.Forms.TextBox();
            this.EntitySearchStatusLabel = new System.Windows.Forms.Label();
            this.EntitySearchAbortButton = new System.Windows.Forms.Button();
            this.EntitySearchButton = new System.Windows.Forms.Button();
            this.EntitySearchLoadedOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.EntitySearchHashLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.EntitySearchTextBox = new System.Windows.Forms.TextBox();
            this.ArchetypeSearchTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ArchetypeResultsListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ArchetypeResultPanel = new System.Windows.Forms.Panel();
            this.ArchetypeResultPropertyGrid = new CodeWalker.WinForms.ReadOnlyPropertyGrid();
            this.label6 = new System.Windows.Forms.Label();
            this.ArchetypeResultViewModelButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.ArchetypeResultFindEntitiesButton = new System.Windows.Forms.Button();
            this.ArchetypeResultYtypTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ArchetypeResultNameTextBox = new System.Windows.Forms.TextBox();
            this.ArchetypeSearchStatusLabel = new System.Windows.Forms.Label();
            this.ArchetypeSearchAbortButton = new System.Windows.Forms.Button();
            this.ArchetypeSearchButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.ArchetypeSearchTextBox = new System.Windows.Forms.TextBox();
            this.EntitySearchExportResultsButton = new System.Windows.Forms.Button();
            this.ArchetypeSearchExportResultsButton = new System.Windows.Forms.Button();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.MainTabControl.SuspendLayout();
            this.EntitySearchTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.EntityResultPanel.SuspendLayout();
            this.ArchetypeSearchTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.ArchetypeResultPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTabControl.Controls.Add(this.EntitySearchTabPage);
            this.MainTabControl.Controls.Add(this.ArchetypeSearchTabPage);
            this.MainTabControl.Location = new System.Drawing.Point(4, 4);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(530, 421);
            this.MainTabControl.TabIndex = 0;
            // 
            // EntitySearchTabPage
            // 
            this.EntitySearchTabPage.Controls.Add(this.EntitySearchExportResultsButton);
            this.EntitySearchTabPage.Controls.Add(this.splitContainer2);
            this.EntitySearchTabPage.Controls.Add(this.EntitySearchStatusLabel);
            this.EntitySearchTabPage.Controls.Add(this.EntitySearchAbortButton);
            this.EntitySearchTabPage.Controls.Add(this.EntitySearchButton);
            this.EntitySearchTabPage.Controls.Add(this.EntitySearchLoadedOnlyCheckBox);
            this.EntitySearchTabPage.Controls.Add(this.EntitySearchHashLabel);
            this.EntitySearchTabPage.Controls.Add(this.label1);
            this.EntitySearchTabPage.Controls.Add(this.EntitySearchTextBox);
            this.EntitySearchTabPage.Location = new System.Drawing.Point(4, 22);
            this.EntitySearchTabPage.Name = "EntitySearchTabPage";
            this.EntitySearchTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.EntitySearchTabPage.Size = new System.Drawing.Size(522, 395);
            this.EntitySearchTabPage.TabIndex = 0;
            this.EntitySearchTabPage.Text = "Entity Search";
            this.EntitySearchTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.Location = new System.Drawing.Point(0, 94);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.EntityResultsListView);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.EntityResultPanel);
            this.splitContainer2.Size = new System.Drawing.Size(522, 301);
            this.splitContainer2.SplitterDistance = 325;
            this.splitContainer2.TabIndex = 8;
            // 
            // EntityResultsListView
            // 
            this.EntityResultsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EntityResultsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            this.EntityResultsListView.FullRowSelect = true;
            this.EntityResultsListView.HideSelection = false;
            this.EntityResultsListView.Location = new System.Drawing.Point(3, 0);
            this.EntityResultsListView.MultiSelect = false;
            this.EntityResultsListView.Name = "EntityResultsListView";
            this.EntityResultsListView.Size = new System.Drawing.Size(319, 298);
            this.EntityResultsListView.TabIndex = 8;
            this.EntityResultsListView.UseCompatibleStateImageBehavior = false;
            this.EntityResultsListView.View = System.Windows.Forms.View.Details;
            this.EntityResultsListView.VirtualMode = true;
            this.EntityResultsListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.EntityResultsListView_RetrieveVirtualItem);
            this.EntityResultsListView.SelectedIndexChanged += new System.EventHandler(this.EntityResultsListView_SelectedIndexChanged);
            this.EntityResultsListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.EntityResultsListView_MouseDoubleClick);
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Name";
            this.columnHeader3.Width = 131;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "File";
            this.columnHeader4.Width = 161;
            // 
            // EntityResultPanel
            // 
            this.EntityResultPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EntityResultPanel.Controls.Add(this.EntityResultViewModelButton);
            this.EntityResultPanel.Controls.Add(this.EntityResultPropertyGrid);
            this.EntityResultPanel.Controls.Add(this.label7);
            this.EntityResultPanel.Controls.Add(this.EntityResultGoToButton);
            this.EntityResultPanel.Controls.Add(this.label8);
            this.EntityResultPanel.Controls.Add(this.EntityResultYmapTextBox);
            this.EntityResultPanel.Controls.Add(this.label9);
            this.EntityResultPanel.Controls.Add(this.EntityResultNameTextBox);
            this.EntityResultPanel.Enabled = false;
            this.EntityResultPanel.Location = new System.Drawing.Point(3, 0);
            this.EntityResultPanel.Name = "EntityResultPanel";
            this.EntityResultPanel.Size = new System.Drawing.Size(187, 298);
            this.EntityResultPanel.TabIndex = 9;
            // 
            // EntityResultViewModelButton
            // 
            this.EntityResultViewModelButton.Location = new System.Drawing.Point(94, 91);
            this.EntityResultViewModelButton.Name = "EntityResultViewModelButton";
            this.EntityResultViewModelButton.Size = new System.Drawing.Size(89, 23);
            this.EntityResultViewModelButton.TabIndex = 5;
            this.EntityResultViewModelButton.Text = "View model";
            this.EntityResultViewModelButton.UseVisualStyleBackColor = true;
            this.EntityResultViewModelButton.Click += new System.EventHandler(this.EntityResultViewModelButton_Click);
            // 
            // EntityResultPropertyGrid
            // 
            this.EntityResultPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EntityResultPropertyGrid.HelpVisible = false;
            this.EntityResultPropertyGrid.Location = new System.Drawing.Point(3, 146);
            this.EntityResultPropertyGrid.Name = "EntityResultPropertyGrid";
            this.EntityResultPropertyGrid.ReadOnly = true;
            this.EntityResultPropertyGrid.Size = new System.Drawing.Size(181, 152);
            this.EntityResultPropertyGrid.TabIndex = 7;
            this.EntityResultPropertyGrid.ToolbarVisible = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 130);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "Entity info:";
            // 
            // EntityResultGoToButton
            // 
            this.EntityResultGoToButton.Location = new System.Drawing.Point(3, 91);
            this.EntityResultGoToButton.Name = "EntityResultGoToButton";
            this.EntityResultGoToButton.Size = new System.Drawing.Size(89, 23);
            this.EntityResultGoToButton.TabIndex = 4;
            this.EntityResultGoToButton.Text = "Go to entity";
            this.EntityResultGoToButton.UseVisualStyleBackColor = true;
            this.EntityResultGoToButton.Click += new System.EventHandler(this.EntityResultGoToButton_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 48);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "Ymap file:";
            // 
            // EntityResultYmapTextBox
            // 
            this.EntityResultYmapTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EntityResultYmapTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.EntityResultYmapTextBox.Location = new System.Drawing.Point(3, 64);
            this.EntityResultYmapTextBox.Name = "EntityResultYmapTextBox";
            this.EntityResultYmapTextBox.ReadOnly = true;
            this.EntityResultYmapTextBox.Size = new System.Drawing.Size(180, 20);
            this.EntityResultYmapTextBox.TabIndex = 3;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 7);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(80, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Selected result:";
            // 
            // EntityResultNameTextBox
            // 
            this.EntityResultNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EntityResultNameTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.EntityResultNameTextBox.Location = new System.Drawing.Point(3, 23);
            this.EntityResultNameTextBox.Name = "EntityResultNameTextBox";
            this.EntityResultNameTextBox.ReadOnly = true;
            this.EntityResultNameTextBox.Size = new System.Drawing.Size(180, 20);
            this.EntityResultNameTextBox.TabIndex = 1;
            // 
            // EntitySearchStatusLabel
            // 
            this.EntitySearchStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EntitySearchStatusLabel.AutoEllipsis = true;
            this.EntitySearchStatusLabel.Location = new System.Drawing.Point(6, 73);
            this.EntitySearchStatusLabel.Name = "EntitySearchStatusLabel";
            this.EntitySearchStatusLabel.Size = new System.Drawing.Size(510, 18);
            this.EntitySearchStatusLabel.TabIndex = 7;
            this.EntitySearchStatusLabel.Text = "Ready";
            // 
            // EntitySearchAbortButton
            // 
            this.EntitySearchAbortButton.Enabled = false;
            this.EntitySearchAbortButton.Location = new System.Drawing.Point(152, 43);
            this.EntitySearchAbortButton.Name = "EntitySearchAbortButton";
            this.EntitySearchAbortButton.Size = new System.Drawing.Size(75, 23);
            this.EntitySearchAbortButton.TabIndex = 4;
            this.EntitySearchAbortButton.Text = "Abort";
            this.EntitySearchAbortButton.UseVisualStyleBackColor = true;
            this.EntitySearchAbortButton.Click += new System.EventHandler(this.EntitySearchAbortButton_Click);
            // 
            // EntitySearchButton
            // 
            this.EntitySearchButton.Location = new System.Drawing.Point(71, 43);
            this.EntitySearchButton.Name = "EntitySearchButton";
            this.EntitySearchButton.Size = new System.Drawing.Size(75, 23);
            this.EntitySearchButton.TabIndex = 3;
            this.EntitySearchButton.Text = "Search";
            this.EntitySearchButton.UseVisualStyleBackColor = true;
            this.EntitySearchButton.Click += new System.EventHandler(this.EntitySearchButton_Click);
            // 
            // EntitySearchLoadedOnlyCheckBox
            // 
            this.EntitySearchLoadedOnlyCheckBox.AutoSize = true;
            this.EntitySearchLoadedOnlyCheckBox.Checked = true;
            this.EntitySearchLoadedOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EntitySearchLoadedOnlyCheckBox.Location = new System.Drawing.Point(277, 47);
            this.EntitySearchLoadedOnlyCheckBox.Name = "EntitySearchLoadedOnlyCheckBox";
            this.EntitySearchLoadedOnlyCheckBox.Size = new System.Drawing.Size(105, 17);
            this.EntitySearchLoadedOnlyCheckBox.TabIndex = 5;
            this.EntitySearchLoadedOnlyCheckBox.Text = "Loaded files only";
            this.EntitySearchLoadedOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // EntitySearchHashLabel
            // 
            this.EntitySearchHashLabel.AutoSize = true;
            this.EntitySearchHashLabel.Location = new System.Drawing.Point(276, 16);
            this.EntitySearchHashLabel.Name = "EntitySearchHashLabel";
            this.EntitySearchHashLabel.Size = new System.Drawing.Size(22, 13);
            this.EntitySearchHashLabel.TabIndex = 1;
            this.EntitySearchHashLabel.Text = "     ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Search for:";
            // 
            // EntitySearchTextBox
            // 
            this.EntitySearchTextBox.Location = new System.Drawing.Point(71, 13);
            this.EntitySearchTextBox.Name = "EntitySearchTextBox";
            this.EntitySearchTextBox.Size = new System.Drawing.Size(199, 20);
            this.EntitySearchTextBox.TabIndex = 0;
            this.EntitySearchTextBox.TextChanged += new System.EventHandler(this.EntitySearchTextBox_TextChanged);
            this.EntitySearchTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EntitySearchTextBox_KeyPress);
            // 
            // ArchetypeSearchTabPage
            // 
            this.ArchetypeSearchTabPage.Controls.Add(this.ArchetypeSearchExportResultsButton);
            this.ArchetypeSearchTabPage.Controls.Add(this.splitContainer1);
            this.ArchetypeSearchTabPage.Controls.Add(this.ArchetypeSearchStatusLabel);
            this.ArchetypeSearchTabPage.Controls.Add(this.ArchetypeSearchAbortButton);
            this.ArchetypeSearchTabPage.Controls.Add(this.ArchetypeSearchButton);
            this.ArchetypeSearchTabPage.Controls.Add(this.label3);
            this.ArchetypeSearchTabPage.Controls.Add(this.ArchetypeSearchTextBox);
            this.ArchetypeSearchTabPage.Location = new System.Drawing.Point(4, 22);
            this.ArchetypeSearchTabPage.Name = "ArchetypeSearchTabPage";
            this.ArchetypeSearchTabPage.Size = new System.Drawing.Size(522, 395);
            this.ArchetypeSearchTabPage.TabIndex = 1;
            this.ArchetypeSearchTabPage.Text = "Archetype Search";
            this.ArchetypeSearchTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 94);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ArchetypeResultsListView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ArchetypeResultPanel);
            this.splitContainer1.Size = new System.Drawing.Size(522, 301);
            this.splitContainer1.SplitterDistance = 325;
            this.splitContainer1.TabIndex = 7;
            // 
            // ArchetypeResultsListView
            // 
            this.ArchetypeResultsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ArchetypeResultsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.ArchetypeResultsListView.FullRowSelect = true;
            this.ArchetypeResultsListView.HideSelection = false;
            this.ArchetypeResultsListView.Location = new System.Drawing.Point(3, 0);
            this.ArchetypeResultsListView.MultiSelect = false;
            this.ArchetypeResultsListView.Name = "ArchetypeResultsListView";
            this.ArchetypeResultsListView.Size = new System.Drawing.Size(319, 298);
            this.ArchetypeResultsListView.TabIndex = 6;
            this.ArchetypeResultsListView.UseCompatibleStateImageBehavior = false;
            this.ArchetypeResultsListView.View = System.Windows.Forms.View.Details;
            this.ArchetypeResultsListView.VirtualMode = true;
            this.ArchetypeResultsListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.ArchetypeResultsListView_RetrieveVirtualItem);
            this.ArchetypeResultsListView.SelectedIndexChanged += new System.EventHandler(this.ArchetypeResultsListView_SelectedIndexChanged);
            this.ArchetypeResultsListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ArchetypeResultsListView_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 131;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "File";
            this.columnHeader2.Width = 161;
            // 
            // ArchetypeResultPanel
            // 
            this.ArchetypeResultPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ArchetypeResultPanel.Controls.Add(this.ArchetypeResultPropertyGrid);
            this.ArchetypeResultPanel.Controls.Add(this.label6);
            this.ArchetypeResultPanel.Controls.Add(this.ArchetypeResultViewModelButton);
            this.ArchetypeResultPanel.Controls.Add(this.label5);
            this.ArchetypeResultPanel.Controls.Add(this.ArchetypeResultFindEntitiesButton);
            this.ArchetypeResultPanel.Controls.Add(this.ArchetypeResultYtypTextBox);
            this.ArchetypeResultPanel.Controls.Add(this.label4);
            this.ArchetypeResultPanel.Controls.Add(this.ArchetypeResultNameTextBox);
            this.ArchetypeResultPanel.Enabled = false;
            this.ArchetypeResultPanel.Location = new System.Drawing.Point(3, 0);
            this.ArchetypeResultPanel.Name = "ArchetypeResultPanel";
            this.ArchetypeResultPanel.Size = new System.Drawing.Size(187, 298);
            this.ArchetypeResultPanel.TabIndex = 7;
            // 
            // ArchetypeResultPropertyGrid
            // 
            this.ArchetypeResultPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ArchetypeResultPropertyGrid.HelpVisible = false;
            this.ArchetypeResultPropertyGrid.Location = new System.Drawing.Point(3, 146);
            this.ArchetypeResultPropertyGrid.Name = "ArchetypeResultPropertyGrid";
            this.ArchetypeResultPropertyGrid.ReadOnly = true;
            this.ArchetypeResultPropertyGrid.Size = new System.Drawing.Size(181, 152);
            this.ArchetypeResultPropertyGrid.TabIndex = 7;
            this.ArchetypeResultPropertyGrid.ToolbarVisible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 130);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Archetype info:";
            // 
            // ArchetypeResultViewModelButton
            // 
            this.ArchetypeResultViewModelButton.Location = new System.Drawing.Point(94, 91);
            this.ArchetypeResultViewModelButton.Name = "ArchetypeResultViewModelButton";
            this.ArchetypeResultViewModelButton.Size = new System.Drawing.Size(89, 23);
            this.ArchetypeResultViewModelButton.TabIndex = 5;
            this.ArchetypeResultViewModelButton.Text = "View model";
            this.ArchetypeResultViewModelButton.UseVisualStyleBackColor = true;
            this.ArchetypeResultViewModelButton.Click += new System.EventHandler(this.ArchetypeResultViewModelButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Ytyp file:";
            // 
            // ArchetypeResultFindEntitiesButton
            // 
            this.ArchetypeResultFindEntitiesButton.Location = new System.Drawing.Point(3, 91);
            this.ArchetypeResultFindEntitiesButton.Name = "ArchetypeResultFindEntitiesButton";
            this.ArchetypeResultFindEntitiesButton.Size = new System.Drawing.Size(89, 23);
            this.ArchetypeResultFindEntitiesButton.TabIndex = 4;
            this.ArchetypeResultFindEntitiesButton.Text = "Find entities";
            this.ArchetypeResultFindEntitiesButton.UseVisualStyleBackColor = true;
            this.ArchetypeResultFindEntitiesButton.Click += new System.EventHandler(this.ArchetypeResultFindEntitiesButton_Click);
            // 
            // ArchetypeResultYtypTextBox
            // 
            this.ArchetypeResultYtypTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ArchetypeResultYtypTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.ArchetypeResultYtypTextBox.Location = new System.Drawing.Point(3, 64);
            this.ArchetypeResultYtypTextBox.Name = "ArchetypeResultYtypTextBox";
            this.ArchetypeResultYtypTextBox.ReadOnly = true;
            this.ArchetypeResultYtypTextBox.Size = new System.Drawing.Size(180, 20);
            this.ArchetypeResultYtypTextBox.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 7);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Selected result:";
            // 
            // ArchetypeResultNameTextBox
            // 
            this.ArchetypeResultNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ArchetypeResultNameTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.ArchetypeResultNameTextBox.Location = new System.Drawing.Point(3, 23);
            this.ArchetypeResultNameTextBox.Name = "ArchetypeResultNameTextBox";
            this.ArchetypeResultNameTextBox.ReadOnly = true;
            this.ArchetypeResultNameTextBox.Size = new System.Drawing.Size(180, 20);
            this.ArchetypeResultNameTextBox.TabIndex = 1;
            // 
            // ArchetypeSearchStatusLabel
            // 
            this.ArchetypeSearchStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ArchetypeSearchStatusLabel.AutoEllipsis = true;
            this.ArchetypeSearchStatusLabel.Location = new System.Drawing.Point(6, 73);
            this.ArchetypeSearchStatusLabel.Name = "ArchetypeSearchStatusLabel";
            this.ArchetypeSearchStatusLabel.Size = new System.Drawing.Size(510, 18);
            this.ArchetypeSearchStatusLabel.TabIndex = 6;
            this.ArchetypeSearchStatusLabel.Text = "Ready";
            // 
            // ArchetypeSearchAbortButton
            // 
            this.ArchetypeSearchAbortButton.Enabled = false;
            this.ArchetypeSearchAbortButton.Location = new System.Drawing.Point(152, 43);
            this.ArchetypeSearchAbortButton.Name = "ArchetypeSearchAbortButton";
            this.ArchetypeSearchAbortButton.Size = new System.Drawing.Size(75, 23);
            this.ArchetypeSearchAbortButton.TabIndex = 4;
            this.ArchetypeSearchAbortButton.Text = "Abort";
            this.ArchetypeSearchAbortButton.UseVisualStyleBackColor = true;
            this.ArchetypeSearchAbortButton.Click += new System.EventHandler(this.ArchetypeSearchAbortButton_Click);
            // 
            // ArchetypeSearchButton
            // 
            this.ArchetypeSearchButton.Location = new System.Drawing.Point(71, 43);
            this.ArchetypeSearchButton.Name = "ArchetypeSearchButton";
            this.ArchetypeSearchButton.Size = new System.Drawing.Size(75, 23);
            this.ArchetypeSearchButton.TabIndex = 3;
            this.ArchetypeSearchButton.Text = "Search";
            this.ArchetypeSearchButton.UseVisualStyleBackColor = true;
            this.ArchetypeSearchButton.Click += new System.EventHandler(this.ArchetypeSearchButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Search for:";
            // 
            // ArchetypeSearchTextBox
            // 
            this.ArchetypeSearchTextBox.Location = new System.Drawing.Point(71, 13);
            this.ArchetypeSearchTextBox.Name = "ArchetypeSearchTextBox";
            this.ArchetypeSearchTextBox.Size = new System.Drawing.Size(199, 20);
            this.ArchetypeSearchTextBox.TabIndex = 0;
            this.ArchetypeSearchTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ArchetypeSearchTextBox_KeyPress);
            // 
            // EntitySearchExportResultsButton
            // 
            this.EntitySearchExportResultsButton.Enabled = false;
            this.EntitySearchExportResultsButton.Location = new System.Drawing.Point(426, 43);
            this.EntitySearchExportResultsButton.Name = "EntitySearchExportResultsButton";
            this.EntitySearchExportResultsButton.Size = new System.Drawing.Size(89, 23);
            this.EntitySearchExportResultsButton.TabIndex = 6;
            this.EntitySearchExportResultsButton.Text = "Export results...";
            this.EntitySearchExportResultsButton.UseVisualStyleBackColor = true;
            this.EntitySearchExportResultsButton.Click += new System.EventHandler(this.EntitySearchExportResultsButton_Click);
            // 
            // ArchetypeSearchExportResultsButton
            // 
            this.ArchetypeSearchExportResultsButton.Enabled = false;
            this.ArchetypeSearchExportResultsButton.Location = new System.Drawing.Point(426, 43);
            this.ArchetypeSearchExportResultsButton.Name = "ArchetypeSearchExportResultsButton";
            this.ArchetypeSearchExportResultsButton.Size = new System.Drawing.Size(89, 23);
            this.ArchetypeSearchExportResultsButton.TabIndex = 5;
            this.ArchetypeSearchExportResultsButton.Text = "Export results...";
            this.ArchetypeSearchExportResultsButton.UseVisualStyleBackColor = true;
            this.ArchetypeSearchExportResultsButton.Click += new System.EventHandler(this.ArchetypeSearchExportResultsButton_Click);
            // 
            // SaveFileDialog
            // 
            this.SaveFileDialog.AddExtension = false;
            this.SaveFileDialog.Filter = "Text files|*.txt|CSV files|*.csv|All files|*.*";
            // 
            // WorldSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 428);
            this.Controls.Add(this.MainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WorldSearchForm";
            this.Text = "World Search - CodeWalker by dexyfex";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WorldSearchForm_FormClosed);
            this.MainTabControl.ResumeLayout(false);
            this.EntitySearchTabPage.ResumeLayout(false);
            this.EntitySearchTabPage.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.EntityResultPanel.ResumeLayout(false);
            this.EntityResultPanel.PerformLayout();
            this.ArchetypeSearchTabPage.ResumeLayout(false);
            this.ArchetypeSearchTabPage.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ArchetypeResultPanel.ResumeLayout(false);
            this.ArchetypeResultPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage EntitySearchTabPage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox EntitySearchTextBox;
        private System.Windows.Forms.Label EntitySearchHashLabel;
        private System.Windows.Forms.TabPage ArchetypeSearchTabPage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ArchetypeSearchTextBox;
        private System.Windows.Forms.CheckBox EntitySearchLoadedOnlyCheckBox;
        private System.Windows.Forms.Panel ArchetypeResultPanel;
        private System.Windows.Forms.Button ArchetypeSearchButton;
        private System.Windows.Forms.ListView ArchetypeResultsListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button ArchetypeResultFindEntitiesButton;
        private System.Windows.Forms.TextBox ArchetypeResultYtypTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox ArchetypeResultNameTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button ArchetypeSearchAbortButton;
        private System.Windows.Forms.Label ArchetypeSearchStatusLabel;
        private WinForms.ReadOnlyPropertyGrid ArchetypeResultPropertyGrid;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button ArchetypeResultViewModelButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListView EntityResultsListView;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Panel EntityResultPanel;
        private WinForms.ReadOnlyPropertyGrid EntityResultPropertyGrid;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button EntityResultGoToButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox EntityResultYmapTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox EntityResultNameTextBox;
        private System.Windows.Forms.Label EntitySearchStatusLabel;
        private System.Windows.Forms.Button EntitySearchAbortButton;
        private System.Windows.Forms.Button EntitySearchButton;
        private System.Windows.Forms.Button EntityResultViewModelButton;
        private System.Windows.Forms.Button EntitySearchExportResultsButton;
        private System.Windows.Forms.Button ArchetypeSearchExportResultsButton;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
    }
}