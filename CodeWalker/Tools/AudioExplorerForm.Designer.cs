
namespace CodeWalker.Tools
{
    partial class AudioExplorerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioExplorerForm));
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.HierarchyTreeView = new CodeWalker.WinForms.TreeViewFix();
            this.PropertiesTabControl = new System.Windows.Forms.TabControl();
            this.DetailsTabPage = new System.Windows.Forms.TabPage();
            this.DetailsPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.XmlTabPage = new System.Windows.Forms.TabPage();
            this.XmlTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
            this.NameComboBox = new System.Windows.Forms.ComboBox();
            this.TypeComboBox = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.PropertiesTabControl.SuspendLayout();
            this.DetailsTabPage.SuspendLayout();
            this.XmlTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XmlTextBox)).BeginInit();
            this.SuspendLayout();
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 37);
            this.MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.HierarchyTreeView);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.PropertiesTabControl);
            this.MainSplitContainer.Size = new System.Drawing.Size(823, 527);
            this.MainSplitContainer.SplitterDistance = 366;
            this.MainSplitContainer.TabIndex = 0;
            // 
            // HierarchyTreeView
            // 
            this.HierarchyTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HierarchyTreeView.FullRowSelect = true;
            this.HierarchyTreeView.HideSelection = false;
            this.HierarchyTreeView.Location = new System.Drawing.Point(0, 0);
            this.HierarchyTreeView.Name = "HierarchyTreeView";
            this.HierarchyTreeView.ShowRootLines = false;
            this.HierarchyTreeView.Size = new System.Drawing.Size(366, 527);
            this.HierarchyTreeView.TabIndex = 0;
            this.HierarchyTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.HierarchyTreeView_AfterSelect);
            // 
            // PropertiesTabControl
            // 
            this.PropertiesTabControl.Controls.Add(this.XmlTabPage);
            this.PropertiesTabControl.Controls.Add(this.DetailsTabPage);
            this.PropertiesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertiesTabControl.Location = new System.Drawing.Point(0, 0);
            this.PropertiesTabControl.Name = "PropertiesTabControl";
            this.PropertiesTabControl.SelectedIndex = 0;
            this.PropertiesTabControl.Size = new System.Drawing.Size(453, 527);
            this.PropertiesTabControl.TabIndex = 0;
            // 
            // DetailsTabPage
            // 
            this.DetailsTabPage.Controls.Add(this.DetailsPropertyGrid);
            this.DetailsTabPage.Location = new System.Drawing.Point(4, 22);
            this.DetailsTabPage.Name = "DetailsTabPage";
            this.DetailsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DetailsTabPage.Size = new System.Drawing.Size(445, 501);
            this.DetailsTabPage.TabIndex = 0;
            this.DetailsTabPage.Text = "Details";
            this.DetailsTabPage.UseVisualStyleBackColor = true;
            // 
            // DetailsPropertyGrid
            // 
            this.DetailsPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsPropertyGrid.HelpVisible = false;
            this.DetailsPropertyGrid.Location = new System.Drawing.Point(3, 3);
            this.DetailsPropertyGrid.Name = "DetailsPropertyGrid";
            this.DetailsPropertyGrid.Size = new System.Drawing.Size(439, 495);
            this.DetailsPropertyGrid.TabIndex = 26;
            this.DetailsPropertyGrid.ToolbarVisible = false;
            // 
            // XmlTabPage
            // 
            this.XmlTabPage.Controls.Add(this.XmlTextBox);
            this.XmlTabPage.Location = new System.Drawing.Point(4, 22);
            this.XmlTabPage.Name = "XmlTabPage";
            this.XmlTabPage.Size = new System.Drawing.Size(445, 501);
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
            this.XmlTextBox.AutoScrollMinSize = new System.Drawing.Size(27, 14);
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
            this.XmlTextBox.Size = new System.Drawing.Size(445, 501);
            this.XmlTextBox.TabIndex = 1;
            this.XmlTextBox.Zoom = 100;
            // 
            // NameComboBox
            // 
            this.NameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NameComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.NameComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.NameComboBox.FormattingEnabled = true;
            this.NameComboBox.Location = new System.Drawing.Point(12, 8);
            this.NameComboBox.Name = "NameComboBox";
            this.NameComboBox.Size = new System.Drawing.Size(416, 21);
            this.NameComboBox.TabIndex = 1;
            this.NameComboBox.TextChanged += new System.EventHandler(this.NameComboBox_TextChanged);
            // 
            // TypeComboBox
            // 
            this.TypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TypeComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.TypeComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.TypeComboBox.FormattingEnabled = true;
            this.TypeComboBox.Location = new System.Drawing.Point(448, 8);
            this.TypeComboBox.Name = "TypeComboBox";
            this.TypeComboBox.Size = new System.Drawing.Size(205, 21);
            this.TypeComboBox.TabIndex = 2;
            this.TypeComboBox.TextChanged += new System.EventHandler(this.TypeComboBox_TextChanged);
            // 
            // AudioExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 564);
            this.Controls.Add(this.TypeComboBox);
            this.Controls.Add(this.NameComboBox);
            this.Controls.Add(this.MainSplitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AudioExplorerForm";
            this.Text = "Audio Explorer - CodeWalker by dexyfex";
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.PropertiesTabControl.ResumeLayout(false);
            this.DetailsTabPage.ResumeLayout(false);
            this.XmlTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.XmlTextBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer MainSplitContainer;
        private System.Windows.Forms.ComboBox NameComboBox;
        private WinForms.TreeViewFix HierarchyTreeView;
        private System.Windows.Forms.TabControl PropertiesTabControl;
        private System.Windows.Forms.TabPage DetailsTabPage;
        private WinForms.PropertyGridFix DetailsPropertyGrid;
        private System.Windows.Forms.TabPage XmlTabPage;
        private FastColoredTextBoxNS.FastColoredTextBox XmlTextBox;
        private System.Windows.Forms.ComboBox TypeComboBox;
    }
}