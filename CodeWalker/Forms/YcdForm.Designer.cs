namespace CodeWalker.Forms
{
    partial class YcdForm
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Clips", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Animations", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(YcdForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.MainListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MainPropertyGrid = new CodeWalker.WinForms.ReadOnlyPropertyGrid();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.DetailsTabPage = new System.Windows.Forms.TabPage();
            this.XmlTabPage = new System.Windows.Forms.TabPage();
            this.XmlTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.DetailsTabPage.SuspendLayout();
            this.XmlTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XmlTextBox)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.MainListView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.MainPropertyGrid);
            this.splitContainer1.Size = new System.Drawing.Size(751, 442);
            this.splitContainer1.SplitterDistance = 254;
            this.splitContainer1.TabIndex = 1;
            // 
            // MainListView
            // 
            this.MainListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.MainListView.FullRowSelect = true;
            listViewGroup1.Header = "Clips";
            listViewGroup1.Name = "Clips";
            listViewGroup2.Header = "Animations";
            listViewGroup2.Name = "Anims";
            this.MainListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.MainListView.HideSelection = false;
            this.MainListView.Location = new System.Drawing.Point(3, 3);
            this.MainListView.MultiSelect = false;
            this.MainListView.Name = "MainListView";
            this.MainListView.Size = new System.Drawing.Size(248, 436);
            this.MainListView.TabIndex = 0;
            this.MainListView.UseCompatibleStateImageBehavior = false;
            this.MainListView.View = System.Windows.Forms.View.Details;
            this.MainListView.SelectedIndexChanged += new System.EventHandler(this.MainListView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 221;
            // 
            // MainPropertyGrid
            // 
            this.MainPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainPropertyGrid.HelpVisible = false;
            this.MainPropertyGrid.Location = new System.Drawing.Point(3, 3);
            this.MainPropertyGrid.Name = "MainPropertyGrid";
            this.MainPropertyGrid.ReadOnly = false;
            this.MainPropertyGrid.Size = new System.Drawing.Size(487, 436);
            this.MainPropertyGrid.TabIndex = 0;
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTabControl.Controls.Add(this.DetailsTabPage);
            this.MainTabControl.Controls.Add(this.XmlTabPage);
            this.MainTabControl.Location = new System.Drawing.Point(2, 3);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(759, 468);
            this.MainTabControl.TabIndex = 2;
            this.MainTabControl.SelectedIndexChanged += new System.EventHandler(this.MainTabControl_SelectedIndexChanged);
            // 
            // DetailsTabPage
            // 
            this.DetailsTabPage.Controls.Add(this.splitContainer1);
            this.DetailsTabPage.Location = new System.Drawing.Point(4, 22);
            this.DetailsTabPage.Name = "DetailsTabPage";
            this.DetailsTabPage.Size = new System.Drawing.Size(751, 442);
            this.DetailsTabPage.TabIndex = 0;
            this.DetailsTabPage.Text = "Details";
            this.DetailsTabPage.UseVisualStyleBackColor = true;
            // 
            // XmlTabPage
            // 
            this.XmlTabPage.Controls.Add(this.XmlTextBox);
            this.XmlTabPage.Location = new System.Drawing.Point(4, 22);
            this.XmlTabPage.Name = "XmlTabPage";
            this.XmlTabPage.Size = new System.Drawing.Size(751, 442);
            this.XmlTabPage.TabIndex = 1;
            this.XmlTabPage.Text = "XML";
            this.XmlTabPage.UseVisualStyleBackColor = true;
            // 
            // XmlTextBox
            // 
            this.XmlTextBox.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.XmlTextBox.AutoIndentChars = false;
            this.XmlTextBox.AutoIndentCharsPatterns = "";
            this.XmlTextBox.AutoIndentExistingLines = false;
            this.XmlTextBox.AutoScrollMinSize = new System.Drawing.Size(2, 14);
            this.XmlTextBox.BackBrush = null;
            this.XmlTextBox.CharHeight = 14;
            this.XmlTextBox.CharWidth = 8;
            this.XmlTextBox.CommentPrefix = null;
            this.XmlTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.XmlTextBox.DelayedEventsInterval = 1;
            this.XmlTextBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.XmlTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XmlTextBox.IsReplaceMode = false;
            this.XmlTextBox.Language = FastColoredTextBoxNS.Language.XML;
            this.XmlTextBox.LeftBracket = '<';
            this.XmlTextBox.LeftBracket2 = '(';
            this.XmlTextBox.Location = new System.Drawing.Point(0, 0);
            this.XmlTextBox.Name = "XmlTextBox";
            this.XmlTextBox.Paddings = new System.Windows.Forms.Padding(0);
            this.XmlTextBox.RightBracket = '>';
            this.XmlTextBox.RightBracket2 = ')';
            this.XmlTextBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.XmlTextBox.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("XmlTextBox.ServiceColors")));
            this.XmlTextBox.Size = new System.Drawing.Size(751, 442);
            this.XmlTextBox.TabIndex = 1;
            this.XmlTextBox.Zoom = 100;
            this.XmlTextBox.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.XmlTextBox_TextChanged);
            this.XmlTextBox.VisibleRangeChangedDelayed += new System.EventHandler(this.XmlTextBox_VisibleRangeChangedDelayed);
            // 
            // YcdForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 474);
            this.Controls.Add(this.MainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "YcdForm";
            this.Text = "Clip Dictionary Inspector - CodeWalker by dexyfex";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.MainTabControl.ResumeLayout(false);
            this.DetailsTabPage.ResumeLayout(false);
            this.XmlTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.XmlTextBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView MainListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private WinForms.ReadOnlyPropertyGrid MainPropertyGrid;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage DetailsTabPage;
        private System.Windows.Forms.TabPage XmlTabPage;
        private FastColoredTextBoxNS.FastColoredTextBox XmlTextBox;
    }
}