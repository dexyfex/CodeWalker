namespace CodeWalker.World
{
    partial class CutsceneForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CutsceneForm));
            this.label3 = new System.Windows.Forms.Label();
            this.CutsceneComboBox = new System.Windows.Forms.ComboBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.CutsceneTreeView = new System.Windows.Forms.TreeView();
            this.InfoPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.AnimateCameraCheckBox = new System.Windows.Forms.CheckBox();
            this.TimeTrackBar = new System.Windows.Forms.TrackBar();
            this.TimeLabel = new System.Windows.Forms.Label();
            this.PlayStopButton = new System.Windows.Forms.Button();
            this.PlaybackTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimeTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Cutscene:";
            // 
            // CutsceneComboBox
            // 
            this.CutsceneComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CutsceneComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CutsceneComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CutsceneComboBox.FormattingEnabled = true;
            this.CutsceneComboBox.Location = new System.Drawing.Point(71, 7);
            this.CutsceneComboBox.Name = "CutsceneComboBox";
            this.CutsceneComboBox.Size = new System.Drawing.Size(529, 21);
            this.CutsceneComboBox.TabIndex = 2;
            this.CutsceneComboBox.SelectedIndexChanged += new System.EventHandler(this.CutsceneComboBox_SelectedIndexChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(1, 70);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.CutsceneTreeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.InfoPropertyGrid);
            this.splitContainer1.Size = new System.Drawing.Size(722, 330);
            this.splitContainer1.SplitterDistance = 259;
            this.splitContainer1.TabIndex = 4;
            // 
            // CutsceneTreeView
            // 
            this.CutsceneTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.CutsceneTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CutsceneTreeView.FullRowSelect = true;
            this.CutsceneTreeView.HideSelection = false;
            this.CutsceneTreeView.Location = new System.Drawing.Point(0, 0);
            this.CutsceneTreeView.Name = "CutsceneTreeView";
            this.CutsceneTreeView.ShowLines = false;
            this.CutsceneTreeView.ShowRootLines = false;
            this.CutsceneTreeView.Size = new System.Drawing.Size(259, 330);
            this.CutsceneTreeView.TabIndex = 1;
            this.CutsceneTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.CutsceneTreeView_AfterSelect);
            // 
            // InfoPropertyGrid
            // 
            this.InfoPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InfoPropertyGrid.HelpVisible = false;
            this.InfoPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.InfoPropertyGrid.Name = "InfoPropertyGrid";
            this.InfoPropertyGrid.Size = new System.Drawing.Size(459, 330);
            this.InfoPropertyGrid.TabIndex = 0;
            // 
            // AnimateCameraCheckBox
            // 
            this.AnimateCameraCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AnimateCameraCheckBox.AutoSize = true;
            this.AnimateCameraCheckBox.Checked = true;
            this.AnimateCameraCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AnimateCameraCheckBox.Location = new System.Drawing.Point(617, 9);
            this.AnimateCameraCheckBox.Name = "AnimateCameraCheckBox";
            this.AnimateCameraCheckBox.Size = new System.Drawing.Size(102, 17);
            this.AnimateCameraCheckBox.TabIndex = 5;
            this.AnimateCameraCheckBox.Text = "Animate camera";
            this.AnimateCameraCheckBox.UseVisualStyleBackColor = true;
            this.AnimateCameraCheckBox.CheckedChanged += new System.EventHandler(this.AnimateCameraCheckBox_CheckedChanged);
            // 
            // TimeTrackBar
            // 
            this.TimeTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TimeTrackBar.AutoSize = false;
            this.TimeTrackBar.LargeChange = 0;
            this.TimeTrackBar.Location = new System.Drawing.Point(91, 34);
            this.TimeTrackBar.Maximum = 100;
            this.TimeTrackBar.Name = "TimeTrackBar";
            this.TimeTrackBar.Size = new System.Drawing.Size(509, 30);
            this.TimeTrackBar.TabIndex = 7;
            this.TimeTrackBar.Scroll += new System.EventHandler(this.TimeTrackBar_Scroll);
            this.TimeTrackBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TimeTrackBar_MouseUp);
            // 
            // TimeLabel
            // 
            this.TimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TimeLabel.AutoSize = true;
            this.TimeLabel.Location = new System.Drawing.Point(614, 41);
            this.TimeLabel.Name = "TimeLabel";
            this.TimeLabel.Size = new System.Drawing.Size(60, 13);
            this.TimeLabel.TabIndex = 8;
            this.TimeLabel.Text = "0.00 / 0.00";
            // 
            // PlayStopButton
            // 
            this.PlayStopButton.Location = new System.Drawing.Point(10, 36);
            this.PlayStopButton.Name = "PlayStopButton";
            this.PlayStopButton.Size = new System.Drawing.Size(75, 23);
            this.PlayStopButton.TabIndex = 9;
            this.PlayStopButton.Text = "Play";
            this.PlayStopButton.UseVisualStyleBackColor = true;
            this.PlayStopButton.Click += new System.EventHandler(this.PlayStopButton_Click);
            // 
            // PlaybackTimer
            // 
            this.PlaybackTimer.Tick += new System.EventHandler(this.PlaybackTimer_Tick);
            // 
            // CutsceneForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(724, 401);
            this.Controls.Add(this.PlayStopButton);
            this.Controls.Add(this.TimeLabel);
            this.Controls.Add(this.TimeTrackBar);
            this.Controls.Add(this.AnimateCameraCheckBox);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.CutsceneComboBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CutsceneForm";
            this.Text = "Cutscene Viewer - CodeWalker by dexyfex";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CutsceneForm_FormClosed);
            this.Load += new System.EventHandler(this.CutsceneForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TimeTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox CutsceneComboBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView CutsceneTreeView;
        private System.Windows.Forms.PropertyGrid InfoPropertyGrid;
        private System.Windows.Forms.CheckBox AnimateCameraCheckBox;
        private System.Windows.Forms.TrackBar TimeTrackBar;
        private System.Windows.Forms.Label TimeLabel;
        private System.Windows.Forms.Button PlayStopButton;
        private System.Windows.Forms.Timer PlaybackTimer;
    }
}