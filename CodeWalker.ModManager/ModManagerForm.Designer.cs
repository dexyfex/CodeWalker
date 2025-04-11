namespace CodeWalker.ModManager
{
    partial class ModManagerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModManagerForm));
            this.MainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.TopPanel = new System.Windows.Forms.Panel();
            this.CleanButton = new System.Windows.Forms.Button();
            this.BuildButton = new System.Windows.Forms.Button();
            this.GameButton = new System.Windows.Forms.Button();
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.InstallModButton = new System.Windows.Forms.Button();
            this.InstalledModsLabel = new System.Windows.Forms.Label();
            this.ModPanel = new System.Windows.Forms.Panel();
            this.ModFilesLabel = new System.Windows.Forms.Label();
            this.ModFilesListBox = new System.Windows.Forms.ListBox();
            this.ModStatusLabel = new System.Windows.Forms.Label();
            this.ModNameLabel = new System.Windows.Forms.Label();
            this.MoveDownButton = new System.Windows.Forms.Button();
            this.MoveUpButton = new System.Windows.Forms.Button();
            this.UninstallModButton = new System.Windows.Forms.Button();
            this.SplashPanel = new System.Windows.Forms.Panel();
            this.SplashLabel2 = new System.Windows.Forms.Label();
            this.SplashLabel1 = new System.Windows.Forms.Label();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.InstalledModsListView = new System.Windows.Forms.ListView();
            this.InstalledModsNameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.InstalledModsStatusColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MainStatusStrip.SuspendLayout();
            this.TopPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.ModPanel.SuspendLayout();
            this.SplashPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusStrip
            // 
            this.MainStatusStrip.BackColor = System.Drawing.SystemColors.Control;
            this.MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.MainStatusStrip.Location = new System.Drawing.Point(0, 431);
            this.MainStatusStrip.Name = "MainStatusStrip";
            this.MainStatusStrip.Size = new System.Drawing.Size(800, 22);
            this.MainStatusStrip.TabIndex = 0;
            this.MainStatusStrip.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(785, 17);
            this.StatusLabel.Spring = true;
            this.StatusLabel.Text = "Initialising...";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TopPanel
            // 
            this.TopPanel.Controls.Add(this.CleanButton);
            this.TopPanel.Controls.Add(this.BuildButton);
            this.TopPanel.Controls.Add(this.GameButton);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopPanel.Location = new System.Drawing.Point(0, 0);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = new System.Drawing.Size(800, 40);
            this.TopPanel.TabIndex = 1;
            // 
            // CleanButton
            // 
            this.CleanButton.Location = new System.Drawing.Point(389, 9);
            this.CleanButton.Name = "CleanButton";
            this.CleanButton.Size = new System.Drawing.Size(110, 23);
            this.CleanButton.TabIndex = 2;
            this.CleanButton.Text = "Clean";
            this.CleanButton.UseVisualStyleBackColor = true;
            // 
            // BuildButton
            // 
            this.BuildButton.Location = new System.Drawing.Point(273, 9);
            this.BuildButton.Name = "BuildButton";
            this.BuildButton.Size = new System.Drawing.Size(110, 23);
            this.BuildButton.TabIndex = 1;
            this.BuildButton.Text = "Build mods folder";
            this.BuildButton.UseVisualStyleBackColor = true;
            // 
            // GameButton
            // 
            this.GameButton.Location = new System.Drawing.Point(12, 9);
            this.GameButton.Name = "GameButton";
            this.GameButton.Size = new System.Drawing.Size(150, 23);
            this.GameButton.TabIndex = 0;
            this.GameButton.Text = "Game: (None selected)";
            this.GameButton.UseVisualStyleBackColor = true;
            this.GameButton.Click += new System.EventHandler(this.GameButton_Click);
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 40);
            this.MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.InstalledModsListView);
            this.MainSplitContainer.Panel1.Controls.Add(this.InstallModButton);
            this.MainSplitContainer.Panel1.Controls.Add(this.InstalledModsLabel);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.ModPanel);
            this.MainSplitContainer.Panel2.Controls.Add(this.SplashPanel);
            this.MainSplitContainer.Size = new System.Drawing.Size(800, 391);
            this.MainSplitContainer.SplitterDistance = 266;
            this.MainSplitContainer.TabIndex = 2;
            // 
            // InstallModButton
            // 
            this.InstallModButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.InstallModButton.Location = new System.Drawing.Point(153, 3);
            this.InstallModButton.Name = "InstallModButton";
            this.InstallModButton.Size = new System.Drawing.Size(110, 23);
            this.InstallModButton.TabIndex = 0;
            this.InstallModButton.Text = "Install new mod...";
            this.InstallModButton.UseVisualStyleBackColor = true;
            this.InstallModButton.Click += new System.EventHandler(this.InstallModButton_Click);
            // 
            // InstalledModsLabel
            // 
            this.InstalledModsLabel.AutoSize = true;
            this.InstalledModsLabel.Location = new System.Drawing.Point(9, 8);
            this.InstalledModsLabel.Name = "InstalledModsLabel";
            this.InstalledModsLabel.Size = new System.Drawing.Size(131, 13);
            this.InstalledModsLabel.TabIndex = 0;
            this.InstalledModsLabel.Text = "Installed mods: (drop here)";
            // 
            // ModPanel
            // 
            this.ModPanel.Controls.Add(this.ModFilesLabel);
            this.ModPanel.Controls.Add(this.ModFilesListBox);
            this.ModPanel.Controls.Add(this.ModStatusLabel);
            this.ModPanel.Controls.Add(this.ModNameLabel);
            this.ModPanel.Controls.Add(this.MoveDownButton);
            this.ModPanel.Controls.Add(this.MoveUpButton);
            this.ModPanel.Controls.Add(this.UninstallModButton);
            this.ModPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModPanel.Location = new System.Drawing.Point(0, 0);
            this.ModPanel.Name = "ModPanel";
            this.ModPanel.Size = new System.Drawing.Size(530, 391);
            this.ModPanel.TabIndex = 0;
            this.ModPanel.Visible = false;
            // 
            // ModFilesLabel
            // 
            this.ModFilesLabel.AutoSize = true;
            this.ModFilesLabel.Location = new System.Drawing.Point(18, 88);
            this.ModFilesLabel.Name = "ModFilesLabel";
            this.ModFilesLabel.Size = new System.Drawing.Size(31, 13);
            this.ModFilesLabel.TabIndex = 6;
            this.ModFilesLabel.Text = "Files:";
            // 
            // ModFilesListBox
            // 
            this.ModFilesListBox.FormattingEnabled = true;
            this.ModFilesListBox.HorizontalScrollbar = true;
            this.ModFilesListBox.Location = new System.Drawing.Point(18, 104);
            this.ModFilesListBox.Name = "ModFilesListBox";
            this.ModFilesListBox.Size = new System.Drawing.Size(157, 212);
            this.ModFilesListBox.TabIndex = 7;
            // 
            // ModStatusLabel
            // 
            this.ModStatusLabel.AutoSize = true;
            this.ModStatusLabel.Location = new System.Drawing.Point(18, 55);
            this.ModStatusLabel.Name = "ModStatusLabel";
            this.ModStatusLabel.Size = new System.Drawing.Size(83, 13);
            this.ModStatusLabel.TabIndex = 5;
            this.ModStatusLabel.Text = "Ready, Enabled";
            // 
            // ModNameLabel
            // 
            this.ModNameLabel.AutoSize = true;
            this.ModNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ModNameLabel.Location = new System.Drawing.Point(15, 33);
            this.ModNameLabel.Name = "ModNameLabel";
            this.ModNameLabel.Size = new System.Drawing.Size(84, 17);
            this.ModNameLabel.TabIndex = 4;
            this.ModNameLabel.Text = "Mod Name";
            // 
            // MoveDownButton
            // 
            this.MoveDownButton.Location = new System.Drawing.Point(235, 3);
            this.MoveDownButton.Name = "MoveDownButton";
            this.MoveDownButton.Size = new System.Drawing.Size(110, 23);
            this.MoveDownButton.TabIndex = 2;
            this.MoveDownButton.Text = "Move down";
            this.MoveDownButton.UseVisualStyleBackColor = true;
            // 
            // MoveUpButton
            // 
            this.MoveUpButton.Location = new System.Drawing.Point(119, 3);
            this.MoveUpButton.Name = "MoveUpButton";
            this.MoveUpButton.Size = new System.Drawing.Size(110, 23);
            this.MoveUpButton.TabIndex = 1;
            this.MoveUpButton.Text = "Move up";
            this.MoveUpButton.UseVisualStyleBackColor = true;
            // 
            // UninstallModButton
            // 
            this.UninstallModButton.Location = new System.Drawing.Point(3, 3);
            this.UninstallModButton.Name = "UninstallModButton";
            this.UninstallModButton.Size = new System.Drawing.Size(110, 23);
            this.UninstallModButton.TabIndex = 0;
            this.UninstallModButton.Text = "Uninstall mod";
            this.UninstallModButton.UseVisualStyleBackColor = true;
            this.UninstallModButton.Click += new System.EventHandler(this.UninstallModButton_Click);
            // 
            // SplashPanel
            // 
            this.SplashPanel.Controls.Add(this.SplashLabel2);
            this.SplashPanel.Controls.Add(this.SplashLabel1);
            this.SplashPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplashPanel.Location = new System.Drawing.Point(0, 0);
            this.SplashPanel.Name = "SplashPanel";
            this.SplashPanel.Size = new System.Drawing.Size(530, 391);
            this.SplashPanel.TabIndex = 1;
            // 
            // SplashLabel2
            // 
            this.SplashLabel2.AutoSize = true;
            this.SplashLabel2.Location = new System.Drawing.Point(168, 27);
            this.SplashLabel2.Name = "SplashLabel2";
            this.SplashLabel2.Size = new System.Drawing.Size(57, 13);
            this.SplashLabel2.TabIndex = 1;
            this.SplashLabel2.Text = "by dexyfex";
            // 
            // SplashLabel1
            // 
            this.SplashLabel1.AutoSize = true;
            this.SplashLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SplashLabel1.Location = new System.Drawing.Point(147, 6);
            this.SplashLabel1.Name = "SplashLabel1";
            this.SplashLabel1.Size = new System.Drawing.Size(198, 17);
            this.SplashLabel1.TabIndex = 0;
            this.SplashLabel1.Text = "CodeWalker Mod Manager";
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.Multiselect = true;
            // 
            // InstalledModsListView
            // 
            this.InstalledModsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InstalledModsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.InstalledModsNameColumn,
            this.InstalledModsStatusColumn});
            this.InstalledModsListView.FullRowSelect = true;
            this.InstalledModsListView.HideSelection = false;
            this.InstalledModsListView.Location = new System.Drawing.Point(3, 32);
            this.InstalledModsListView.Name = "InstalledModsListView";
            this.InstalledModsListView.Size = new System.Drawing.Size(260, 356);
            this.InstalledModsListView.TabIndex = 2;
            this.InstalledModsListView.UseCompatibleStateImageBehavior = false;
            this.InstalledModsListView.View = System.Windows.Forms.View.Details;
            this.InstalledModsListView.SelectedIndexChanged += new System.EventHandler(this.InstalledModsListView_SelectedIndexChanged);
            // 
            // InstalledModsNameColumn
            // 
            this.InstalledModsNameColumn.Text = "Name";
            this.InstalledModsNameColumn.Width = 174;
            // 
            // InstalledModsStatusColumn
            // 
            this.InstalledModsStatusColumn.Text = "Status";
            // 
            // ModManagerForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(800, 453);
            this.Controls.Add(this.MainSplitContainer);
            this.Controls.Add(this.TopPanel);
            this.Controls.Add(this.MainStatusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ModManagerForm";
            this.Text = "CodeWalker Mod Manager";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ModManagerForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ModManagerForm_DragEnter);
            this.MainStatusStrip.ResumeLayout(false);
            this.MainStatusStrip.PerformLayout();
            this.TopPanel.ResumeLayout(false);
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel1.PerformLayout();
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.ModPanel.ResumeLayout(false);
            this.ModPanel.PerformLayout();
            this.SplashPanel.ResumeLayout(false);
            this.SplashPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip MainStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.Panel TopPanel;
        private System.Windows.Forms.SplitContainer MainSplitContainer;
        private System.Windows.Forms.Button GameButton;
        private System.Windows.Forms.Button InstallModButton;
        private System.Windows.Forms.Label InstalledModsLabel;
        private System.Windows.Forms.Panel ModPanel;
        private System.Windows.Forms.Button MoveDownButton;
        private System.Windows.Forms.Button MoveUpButton;
        private System.Windows.Forms.Button UninstallModButton;
        private System.Windows.Forms.Panel SplashPanel;
        private System.Windows.Forms.Label SplashLabel2;
        private System.Windows.Forms.Label SplashLabel1;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
        private System.Windows.Forms.Button BuildButton;
        private System.Windows.Forms.Button CleanButton;
        private System.Windows.Forms.Label ModFilesLabel;
        private System.Windows.Forms.ListBox ModFilesListBox;
        private System.Windows.Forms.Label ModStatusLabel;
        private System.Windows.Forms.Label ModNameLabel;
        private System.Windows.Forms.ListView InstalledModsListView;
        private System.Windows.Forms.ColumnHeader InstalledModsNameColumn;
        private System.Windows.Forms.ColumnHeader InstalledModsStatusColumn;
    }
}