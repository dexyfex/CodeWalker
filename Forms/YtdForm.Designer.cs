namespace CodeWalker.Forms
{
    partial class YtdForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(YtdForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.FileSaveAllMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.EditMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.NewButton = new System.Windows.Forms.ToolStripSplitButton();
            this.OpenButton = new System.Windows.Forms.ToolStripSplitButton();
            this.SaveButton = new System.Windows.Forms.ToolStripSplitButton();
            this.MainToolbar = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.TexturesListView = new System.Windows.Forms.ListView();
            this.TextureNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TextureSizeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label5 = new System.Windows.Forms.Label();
            this.SelTextureNameTextBox = new System.Windows.Forms.TextBox();
            this.TextureTabControl = new System.Windows.Forms.TabControl();
            this.TextureTabPage = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.SelTextureZoomCombo = new System.Windows.Forms.ComboBox();
            this.SelTexturePanel = new System.Windows.Forms.Panel();
            this.SelTexturePictureBox = new System.Windows.Forms.PictureBox();
            this.SelTextureMipLabel = new System.Windows.Forms.Label();
            this.SelTextureDimensionsLabel = new System.Windows.Forms.Label();
            this.SelTextureMipTrackBar = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.DetailsTabPage = new System.Windows.Forms.TabPage();
            this.DetailsPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.FileSaveAsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSaveAsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolbarSaveAllMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.MainToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.TextureTabControl.SuspendLayout();
            this.TextureTabPage.SuspendLayout();
            this.SelTexturePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SelTexturePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SelTextureMipTrackBar)).BeginInit();
            this.DetailsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.EditMenu,
            this.ViewMenu});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(845, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // FileMenu
            // 
            this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileSaveAsMenu,
            this.FileSaveAllMenu});
            this.FileMenu.Name = "FileMenu";
            this.FileMenu.Size = new System.Drawing.Size(37, 20);
            this.FileMenu.Text = "File";
            // 
            // FileSaveAllMenu
            // 
            this.FileSaveAllMenu.Image = ((System.Drawing.Image)(resources.GetObject("FileSaveAllMenu.Image")));
            this.FileSaveAllMenu.Name = "FileSaveAllMenu";
            this.FileSaveAllMenu.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.FileSaveAllMenu.Size = new System.Drawing.Size(196, 22);
            this.FileSaveAllMenu.Text = "Save All...";
            this.FileSaveAllMenu.Click += new System.EventHandler(this.FileSaveAllMenu_Click);
            // 
            // EditMenu
            // 
            this.EditMenu.Enabled = false;
            this.EditMenu.Name = "EditMenu";
            this.EditMenu.Size = new System.Drawing.Size(39, 20);
            this.EditMenu.Text = "Edit";
            // 
            // ViewMenu
            // 
            this.ViewMenu.Enabled = false;
            this.ViewMenu.Name = "ViewMenu";
            this.ViewMenu.Size = new System.Drawing.Size(44, 20);
            this.ViewMenu.Text = "View";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewButton,
            this.OpenButton,
            this.SaveButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(845, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // NewButton
            // 
            this.NewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.NewButton.Enabled = false;
            this.NewButton.Image = ((System.Drawing.Image)(resources.GetObject("NewButton.Image")));
            this.NewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.NewButton.Name = "NewButton";
            this.NewButton.Size = new System.Drawing.Size(32, 22);
            this.NewButton.Text = "New YTD...";
            // 
            // OpenButton
            // 
            this.OpenButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.OpenButton.Enabled = false;
            this.OpenButton.Image = ((System.Drawing.Image)(resources.GetObject("OpenButton.Image")));
            this.OpenButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(32, 22);
            this.OpenButton.Text = "Open YTD...";
            // 
            // SaveButton
            // 
            this.SaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SaveButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarSaveAsMenu,
            this.ToolbarSaveAllMenu});
            this.SaveButton.Image = ((System.Drawing.Image)(resources.GetObject("SaveButton.Image")));
            this.SaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(32, 22);
            this.SaveButton.Text = "Save YTD...";
            this.SaveButton.ButtonClick += new System.EventHandler(this.SaveButton_ButtonClick);
            // 
            // MainToolbar
            // 
            this.MainToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.MainToolbar.Location = new System.Drawing.Point(0, 538);
            this.MainToolbar.Name = "MainToolbar";
            this.MainToolbar.Size = new System.Drawing.Size(845, 22);
            this.MainToolbar.TabIndex = 2;
            this.MainToolbar.Text = "Toolbar";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(830, 17);
            this.StatusLabel.Spring = true;
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 49);
            this.MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.TexturesListView);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.label5);
            this.MainSplitContainer.Panel2.Controls.Add(this.SelTextureNameTextBox);
            this.MainSplitContainer.Panel2.Controls.Add(this.TextureTabControl);
            this.MainSplitContainer.Size = new System.Drawing.Size(845, 489);
            this.MainSplitContainer.SplitterDistance = 257;
            this.MainSplitContainer.TabIndex = 3;
            // 
            // TexturesListView
            // 
            this.TexturesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TexturesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TextureNameColumnHeader,
            this.TextureSizeColumnHeader});
            this.TexturesListView.FullRowSelect = true;
            this.TexturesListView.HideSelection = false;
            this.TexturesListView.Location = new System.Drawing.Point(3, 3);
            this.TexturesListView.Name = "TexturesListView";
            this.TexturesListView.ShowItemToolTips = true;
            this.TexturesListView.Size = new System.Drawing.Size(251, 483);
            this.TexturesListView.TabIndex = 0;
            this.TexturesListView.UseCompatibleStateImageBehavior = false;
            this.TexturesListView.View = System.Windows.Forms.View.Details;
            this.TexturesListView.SelectedIndexChanged += new System.EventHandler(this.TexturesListView_SelectedIndexChanged);
            // 
            // TextureNameColumnHeader
            // 
            this.TextureNameColumnHeader.Text = "Name";
            this.TextureNameColumnHeader.Width = 153;
            // 
            // TextureSizeColumnHeader
            // 
            this.TextureSizeColumnHeader.Text = "Size";
            this.TextureSizeColumnHeader.Width = 72;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(262, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 50;
            this.label5.Text = "Name:";
            // 
            // SelTextureNameTextBox
            // 
            this.SelTextureNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SelTextureNameTextBox.Location = new System.Drawing.Point(306, 3);
            this.SelTextureNameTextBox.Name = "SelTextureNameTextBox";
            this.SelTextureNameTextBox.Size = new System.Drawing.Size(271, 20);
            this.SelTextureNameTextBox.TabIndex = 46;
            // 
            // TextureTabControl
            // 
            this.TextureTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextureTabControl.Controls.Add(this.TextureTabPage);
            this.TextureTabControl.Controls.Add(this.DetailsTabPage);
            this.TextureTabControl.Location = new System.Drawing.Point(3, 3);
            this.TextureTabControl.Name = "TextureTabControl";
            this.TextureTabControl.SelectedIndex = 0;
            this.TextureTabControl.Size = new System.Drawing.Size(578, 483);
            this.TextureTabControl.TabIndex = 0;
            // 
            // TextureTabPage
            // 
            this.TextureTabPage.Controls.Add(this.label1);
            this.TextureTabPage.Controls.Add(this.SelTextureZoomCombo);
            this.TextureTabPage.Controls.Add(this.SelTexturePanel);
            this.TextureTabPage.Controls.Add(this.SelTextureMipLabel);
            this.TextureTabPage.Controls.Add(this.SelTextureDimensionsLabel);
            this.TextureTabPage.Controls.Add(this.SelTextureMipTrackBar);
            this.TextureTabPage.Controls.Add(this.label4);
            this.TextureTabPage.Location = new System.Drawing.Point(4, 22);
            this.TextureTabPage.Name = "TextureTabPage";
            this.TextureTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.TextureTabPage.Size = new System.Drawing.Size(570, 457);
            this.TextureTabPage.TabIndex = 0;
            this.TextureTabPage.Text = "Texture";
            this.TextureTabPage.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(452, 426);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 54;
            this.label1.Text = "Zoom:";
            // 
            // SelTextureZoomCombo
            // 
            this.SelTextureZoomCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SelTextureZoomCombo.FormattingEnabled = true;
            this.SelTextureZoomCombo.Items.AddRange(new object[] {
            "Fill",
            "100%",
            "200%"});
            this.SelTextureZoomCombo.Location = new System.Drawing.Point(495, 423);
            this.SelTextureZoomCombo.Name = "SelTextureZoomCombo";
            this.SelTextureZoomCombo.Size = new System.Drawing.Size(69, 21);
            this.SelTextureZoomCombo.TabIndex = 53;
            this.SelTextureZoomCombo.Text = "Fill";
            this.SelTextureZoomCombo.TextChanged += new System.EventHandler(this.SelTextureZoomCombo_TextChanged);
            // 
            // SelTexturePanel
            // 
            this.SelTexturePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelTexturePanel.AutoScroll = true;
            this.SelTexturePanel.BackColor = System.Drawing.Color.DarkGray;
            this.SelTexturePanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SelTexturePanel.BackgroundImage")));
            this.SelTexturePanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SelTexturePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SelTexturePanel.Controls.Add(this.SelTexturePictureBox);
            this.SelTexturePanel.Location = new System.Drawing.Point(6, 6);
            this.SelTexturePanel.Name = "SelTexturePanel";
            this.SelTexturePanel.Size = new System.Drawing.Size(559, 408);
            this.SelTexturePanel.TabIndex = 52;
            // 
            // SelTexturePictureBox
            // 
            this.SelTexturePictureBox.BackColor = System.Drawing.Color.Transparent;
            this.SelTexturePictureBox.Location = new System.Drawing.Point(0, 0);
            this.SelTexturePictureBox.Name = "SelTexturePictureBox";
            this.SelTexturePictureBox.Size = new System.Drawing.Size(133, 96);
            this.SelTexturePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.SelTexturePictureBox.TabIndex = 45;
            this.SelTexturePictureBox.TabStop = false;
            // 
            // SelTextureMipLabel
            // 
            this.SelTextureMipLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelTextureMipLabel.AutoSize = true;
            this.SelTextureMipLabel.Location = new System.Drawing.Point(44, 426);
            this.SelTextureMipLabel.Name = "SelTextureMipLabel";
            this.SelTextureMipLabel.Size = new System.Drawing.Size(13, 13);
            this.SelTextureMipLabel.TabIndex = 51;
            this.SelTextureMipLabel.Text = "0";
            // 
            // SelTextureDimensionsLabel
            // 
            this.SelTextureDimensionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SelTextureDimensionsLabel.AutoSize = true;
            this.SelTextureDimensionsLabel.Location = new System.Drawing.Point(304, 426);
            this.SelTextureDimensionsLabel.Name = "SelTextureDimensionsLabel";
            this.SelTextureDimensionsLabel.Size = new System.Drawing.Size(10, 13);
            this.SelTextureDimensionsLabel.TabIndex = 49;
            this.SelTextureDimensionsLabel.Text = "-";
            // 
            // SelTextureMipTrackBar
            // 
            this.SelTextureMipTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelTextureMipTrackBar.AutoSize = false;
            this.SelTextureMipTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.SelTextureMipTrackBar.LargeChange = 1;
            this.SelTextureMipTrackBar.Location = new System.Drawing.Point(63, 420);
            this.SelTextureMipTrackBar.Maximum = 0;
            this.SelTextureMipTrackBar.Name = "SelTextureMipTrackBar";
            this.SelTextureMipTrackBar.Size = new System.Drawing.Size(234, 31);
            this.SelTextureMipTrackBar.TabIndex = 48;
            this.SelTextureMipTrackBar.Scroll += new System.EventHandler(this.SelTextureMipTrackBar_Scroll);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 426);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 47;
            this.label4.Text = "Mip:";
            // 
            // DetailsTabPage
            // 
            this.DetailsTabPage.Controls.Add(this.DetailsPropertyGrid);
            this.DetailsTabPage.Location = new System.Drawing.Point(4, 22);
            this.DetailsTabPage.Name = "DetailsTabPage";
            this.DetailsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DetailsTabPage.Size = new System.Drawing.Size(570, 457);
            this.DetailsTabPage.TabIndex = 1;
            this.DetailsTabPage.Text = "Details";
            this.DetailsTabPage.UseVisualStyleBackColor = true;
            // 
            // DetailsPropertyGrid
            // 
            this.DetailsPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DetailsPropertyGrid.HelpVisible = false;
            this.DetailsPropertyGrid.Location = new System.Drawing.Point(6, 6);
            this.DetailsPropertyGrid.Name = "DetailsPropertyGrid";
            this.DetailsPropertyGrid.Size = new System.Drawing.Size(558, 445);
            this.DetailsPropertyGrid.TabIndex = 0;
            // 
            // SaveFileDialog
            // 
            this.SaveFileDialog.Filter = "DDS files|*.dds|All files|*.*";
            // 
            // FileSaveAsMenu
            // 
            this.FileSaveAsMenu.Image = ((System.Drawing.Image)(resources.GetObject("FileSaveAsMenu.Image")));
            this.FileSaveAsMenu.Name = "FileSaveAsMenu";
            this.FileSaveAsMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.FileSaveAsMenu.Size = new System.Drawing.Size(196, 22);
            this.FileSaveAsMenu.Text = "Save As...";
            this.FileSaveAsMenu.Click += new System.EventHandler(this.FileSaveAsMenu_Click);
            // 
            // ToolbarSaveAsMenu
            // 
            this.ToolbarSaveAsMenu.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarSaveAsMenu.Image")));
            this.ToolbarSaveAsMenu.Name = "ToolbarSaveAsMenu";
            this.ToolbarSaveAsMenu.Size = new System.Drawing.Size(152, 22);
            this.ToolbarSaveAsMenu.Text = "Save as...";
            this.ToolbarSaveAsMenu.Click += new System.EventHandler(this.ToolbarSaveAsMenu_Click);
            // 
            // ToolbarSaveAllMenu
            // 
            this.ToolbarSaveAllMenu.Image = ((System.Drawing.Image)(resources.GetObject("ToolbarSaveAllMenu.Image")));
            this.ToolbarSaveAllMenu.Name = "ToolbarSaveAllMenu";
            this.ToolbarSaveAllMenu.Size = new System.Drawing.Size(152, 22);
            this.ToolbarSaveAllMenu.Text = "Save All...";
            this.ToolbarSaveAllMenu.Click += new System.EventHandler(this.ToolbarSaveAllMenu_Click);
            // 
            // YtdForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(845, 560);
            this.Controls.Add(this.MainSplitContainer);
            this.Controls.Add(this.MainToolbar);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "YtdForm";
            this.Text = "Texture Dictionary - CodeWalker by dexyfex";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.MainToolbar.ResumeLayout(false);
            this.MainToolbar.PerformLayout();
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            this.MainSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.TextureTabControl.ResumeLayout(false);
            this.TextureTabPage.ResumeLayout(false);
            this.TextureTabPage.PerformLayout();
            this.SelTexturePanel.ResumeLayout(false);
            this.SelTexturePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SelTexturePictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SelTextureMipTrackBar)).EndInit();
            this.DetailsTabPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem FileMenu;
        private System.Windows.Forms.ToolStripMenuItem EditMenu;
        private System.Windows.Forms.ToolStripMenuItem ViewMenu;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripSplitButton NewButton;
        private System.Windows.Forms.ToolStripSplitButton OpenButton;
        private System.Windows.Forms.ToolStripSplitButton SaveButton;
        private System.Windows.Forms.StatusStrip MainToolbar;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.SplitContainer MainSplitContainer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox SelTextureNameTextBox;
        private System.Windows.Forms.TabControl TextureTabControl;
        private System.Windows.Forms.TabPage TextureTabPage;
        private System.Windows.Forms.Label SelTextureMipLabel;
        private System.Windows.Forms.Label SelTextureDimensionsLabel;
        private System.Windows.Forms.TrackBar SelTextureMipTrackBar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox SelTexturePictureBox;
        private System.Windows.Forms.TabPage DetailsTabPage;
        private WinForms.PropertyGridFix DetailsPropertyGrid;
        private System.Windows.Forms.ListView TexturesListView;
        private System.Windows.Forms.ColumnHeader TextureNameColumnHeader;
        private System.Windows.Forms.ColumnHeader TextureSizeColumnHeader;
        private System.Windows.Forms.Panel SelTexturePanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox SelTextureZoomCombo;
        private System.Windows.Forms.ToolStripMenuItem FileSaveAllMenu;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.ToolStripMenuItem FileSaveAsMenu;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSaveAsMenu;
        private System.Windows.Forms.ToolStripMenuItem ToolbarSaveAllMenu;
    }
}