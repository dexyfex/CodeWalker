namespace CodeWalker.Forms
{
    partial class AwcForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AwcForm));
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.PlayerTabPage = new System.Windows.Forms.TabPage();
            this.VolumeLabel = new System.Windows.Forms.Label();
            this.chbAutoJump = new System.Windows.Forms.CheckBox();
            this.PrevButton = new System.Windows.Forms.Button();
            this.NextButton = new System.Windows.Forms.Button();
            this.PlayButton = new System.Windows.Forms.Button();
            this.PlayListView = new System.Windows.Forms.ListView();
            this.PlaylistNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PlaylistTypeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PlaylistLengthHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.VolumeTrackBar = new System.Windows.Forms.TrackBar();
            this.PositionTrackBar = new System.Windows.Forms.TrackBar();
            this.DetailsTabPage = new System.Windows.Forms.TabPage();
            this.DetailsPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.Timer = new System.Windows.Forms.Timer(this.components);
            this.MainTabControl.SuspendLayout();
            this.PlayerTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VolumeTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PositionTrackBar)).BeginInit();
            this.DetailsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.PlayerTabPage);
            this.MainTabControl.Controls.Add(this.DetailsTabPage);
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.Location = new System.Drawing.Point(0, 0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(576, 358);
            this.MainTabControl.TabIndex = 0;
            // 
            // PlayerTabPage
            // 
            this.PlayerTabPage.Controls.Add(this.VolumeLabel);
            this.PlayerTabPage.Controls.Add(this.chbAutoJump);
            this.PlayerTabPage.Controls.Add(this.PrevButton);
            this.PlayerTabPage.Controls.Add(this.NextButton);
            this.PlayerTabPage.Controls.Add(this.PlayButton);
            this.PlayerTabPage.Controls.Add(this.PlayListView);
            this.PlayerTabPage.Controls.Add(this.VolumeTrackBar);
            this.PlayerTabPage.Controls.Add(this.PositionTrackBar);
            this.PlayerTabPage.Location = new System.Drawing.Point(4, 22);
            this.PlayerTabPage.Name = "PlayerTabPage";
            this.PlayerTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.PlayerTabPage.Size = new System.Drawing.Size(568, 332);
            this.PlayerTabPage.TabIndex = 0;
            this.PlayerTabPage.Text = "Player";
            this.PlayerTabPage.UseVisualStyleBackColor = true;
            // 
            // VolumeLabel
            // 
            this.VolumeLabel.AutoSize = true;
            this.VolumeLabel.Location = new System.Drawing.Point(414, 305);
            this.VolumeLabel.Name = "VolumeLabel";
            this.VolumeLabel.Size = new System.Drawing.Size(42, 13);
            this.VolumeLabel.TabIndex = 9;
            this.VolumeLabel.Text = "Volume";
            // 
            // chbAutoJump
            // 
            this.chbAutoJump.AutoSize = true;
            this.chbAutoJump.Enabled = false;
            this.chbAutoJump.Location = new System.Drawing.Point(17, 305);
            this.chbAutoJump.Name = "chbAutoJump";
            this.chbAutoJump.Size = new System.Drawing.Size(108, 17);
            this.chbAutoJump.TabIndex = 8;
            this.chbAutoJump.Text = "Auto-jump to next";
            this.chbAutoJump.UseVisualStyleBackColor = true;
            // 
            // PrevButton
            // 
            this.PrevButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.PrevButton.Location = new System.Drawing.Point(137, 301);
            this.PrevButton.Name = "PrevButton";
            this.PrevButton.Size = new System.Drawing.Size(31, 23);
            this.PrevButton.TabIndex = 2;
            this.PrevButton.Text = "<<";
            this.PrevButton.UseVisualStyleBackColor = true;
            this.PrevButton.Click += new System.EventHandler(this.PrevButton_Click);
            // 
            // NextButton
            // 
            this.NextButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.NextButton.Location = new System.Drawing.Point(255, 301);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(31, 23);
            this.NextButton.TabIndex = 4;
            this.NextButton.Text = ">>";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // PlayButton
            // 
            this.PlayButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.PlayButton.Location = new System.Drawing.Point(174, 301);
            this.PlayButton.Name = "PlayButton";
            this.PlayButton.Size = new System.Drawing.Size(75, 23);
            this.PlayButton.TabIndex = 3;
            this.PlayButton.Text = "Play/Pause";
            this.PlayButton.UseVisualStyleBackColor = true;
            this.PlayButton.Click += new System.EventHandler(this.PlayButton_Click);
            // 
            // PlayListView
            // 
            this.PlayListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.PlaylistNameHeader,
            this.PlaylistTypeHeader,
            this.PlaylistLengthHeader});
            this.PlayListView.FullRowSelect = true;
            this.PlayListView.HideSelection = false;
            this.PlayListView.Location = new System.Drawing.Point(6, 6);
            this.PlayListView.MultiSelect = false;
            this.PlayListView.Name = "PlayListView";
            this.PlayListView.Size = new System.Drawing.Size(556, 235);
            this.PlayListView.TabIndex = 0;
            this.PlayListView.UseCompatibleStateImageBehavior = false;
            this.PlayListView.View = System.Windows.Forms.View.Details;
            this.PlayListView.DoubleClick += new System.EventHandler(this.PlayListView_DoubleClick);
            // 
            // PlaylistNameHeader
            // 
            this.PlaylistNameHeader.Text = "Name";
            this.PlaylistNameHeader.Width = 303;
            // 
            // PlaylistTypeHeader
            // 
            this.PlaylistTypeHeader.Text = "Type";
            this.PlaylistTypeHeader.Width = 110;
            // 
            // PlaylistLengthHeader
            // 
            this.PlaylistLengthHeader.Text = "Length";
            this.PlaylistLengthHeader.Width = 110;
            // 
            // VolumeTrackBar
            // 
            this.VolumeTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.VolumeTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.VolumeTrackBar.LargeChange = 10;
            this.VolumeTrackBar.Location = new System.Drawing.Point(455, 301);
            this.VolumeTrackBar.Maximum = 100;
            this.VolumeTrackBar.Name = "VolumeTrackBar";
            this.VolumeTrackBar.Size = new System.Drawing.Size(105, 45);
            this.VolumeTrackBar.TabIndex = 6;
            this.VolumeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.VolumeTrackBar.Value = 50;
            this.VolumeTrackBar.Scroll += new System.EventHandler(this.VolumeTrackBar_Scroll);
            // 
            // PositionTrackBar
            // 
            this.PositionTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PositionTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.PositionTrackBar.LargeChange = 1000;
            this.PositionTrackBar.Location = new System.Drawing.Point(6, 263);
            this.PositionTrackBar.Maximum = 1000;
            this.PositionTrackBar.Name = "PositionTrackBar";
            this.PositionTrackBar.Size = new System.Drawing.Size(554, 45);
            this.PositionTrackBar.TabIndex = 1;
            this.PositionTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.PositionTrackBar.Scroll += new System.EventHandler(this.PositionTrackBar_Scroll);
            // 
            // DetailsTabPage
            // 
            this.DetailsTabPage.Controls.Add(this.DetailsPropertyGrid);
            this.DetailsTabPage.Location = new System.Drawing.Point(4, 22);
            this.DetailsTabPage.Name = "DetailsTabPage";
            this.DetailsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DetailsTabPage.Size = new System.Drawing.Size(568, 332);
            this.DetailsTabPage.TabIndex = 1;
            this.DetailsTabPage.Text = "Details";
            this.DetailsTabPage.UseVisualStyleBackColor = true;
            // 
            // DetailsPropertyGrid
            // 
            this.DetailsPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsPropertyGrid.HelpVisible = false;
            this.DetailsPropertyGrid.Location = new System.Drawing.Point(3, 3);
            this.DetailsPropertyGrid.Name = "DetailsPropertyGrid";
            this.DetailsPropertyGrid.Size = new System.Drawing.Size(562, 326);
            this.DetailsPropertyGrid.TabIndex = 0;
            // 
            // Timer
            // 
            this.Timer.Enabled = true;
            this.Timer.Tick += new System.EventHandler(this.Timer_Tick);
            // 
            // AwcForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 358);
            this.Controls.Add(this.MainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AwcForm";
            this.Text = "AWC Player - CodeWalker by dexyfex";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AwcForm_FormClosing);
            this.MainTabControl.ResumeLayout(false);
            this.PlayerTabPage.ResumeLayout(false);
            this.PlayerTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VolumeTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PositionTrackBar)).EndInit();
            this.DetailsTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage PlayerTabPage;
        private System.Windows.Forms.TabPage DetailsTabPage;
        private WinForms.PropertyGridFix DetailsPropertyGrid;
        private System.Windows.Forms.ListView PlayListView;
        private System.Windows.Forms.ColumnHeader PlaylistNameHeader;
        private System.Windows.Forms.ColumnHeader PlaylistTypeHeader;
        private System.Windows.Forms.ColumnHeader PlaylistLengthHeader;
        private System.Windows.Forms.Button PrevButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button PlayButton;
        private System.Windows.Forms.TrackBar VolumeTrackBar;
        private System.Windows.Forms.TrackBar PositionTrackBar;
        private System.Windows.Forms.Timer Timer;
        private System.Windows.Forms.CheckBox chbAutoJump;
        private System.Windows.Forms.Label VolumeLabel;
    }
}