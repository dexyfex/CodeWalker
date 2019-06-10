namespace CodeWalker.Forms
{
    partial class MetaForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MetaForm));
            this.MainToolbar = new System.Windows.Forms.ToolStrip();
            this.NewButton = new System.Windows.Forms.ToolStripSplitButton();
            this.OpenButton = new System.Windows.Forms.ToolStripSplitButton();
            this.SaveButton = new System.Windows.Forms.ToolStripSplitButton();
            this.MainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.FileNewMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.FileOpenMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.FileSaveMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.FileSaveAsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.FileCloseMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.EditMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.wIPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.wIPToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.XmlTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.RawPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.MainToolbar.SuspendLayout();
            this.MainStatusStrip.SuspendLayout();
            this.MainMenu.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XmlTextBox)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainToolbar
            // 
            this.MainToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewButton,
            this.OpenButton,
            this.SaveButton});
            this.MainToolbar.Location = new System.Drawing.Point(0, 24);
            this.MainToolbar.Name = "MainToolbar";
            this.MainToolbar.Size = new System.Drawing.Size(839, 25);
            this.MainToolbar.TabIndex = 6;
            this.MainToolbar.Text = "Main Toolbar";
            // 
            // NewButton
            // 
            this.NewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.NewButton.Image = ((System.Drawing.Image)(resources.GetObject("NewButton.Image")));
            this.NewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.NewButton.Name = "NewButton";
            this.NewButton.Size = new System.Drawing.Size(32, 22);
            this.NewButton.Text = "New...";
            this.NewButton.ButtonClick += new System.EventHandler(this.NewButton_ButtonClick);
            // 
            // OpenButton
            // 
            this.OpenButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.OpenButton.Image = ((System.Drawing.Image)(resources.GetObject("OpenButton.Image")));
            this.OpenButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(32, 22);
            this.OpenButton.Text = "Open...";
            this.OpenButton.ButtonClick += new System.EventHandler(this.OpenButton_ButtonClick);
            // 
            // SaveButton
            // 
            this.SaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SaveButton.Image = ((System.Drawing.Image)(resources.GetObject("SaveButton.Image")));
            this.SaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(32, 22);
            this.SaveButton.Text = "Save";
            this.SaveButton.ButtonClick += new System.EventHandler(this.SaveButton_ButtonClick);
            // 
            // MainStatusStrip
            // 
            this.MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.MainStatusStrip.Location = new System.Drawing.Point(0, 535);
            this.MainStatusStrip.Name = "MainStatusStrip";
            this.MainStatusStrip.Size = new System.Drawing.Size(839, 22);
            this.MainStatusStrip.TabIndex = 4;
            this.MainStatusStrip.Text = "Main Status Strip";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(824, 17);
            this.StatusLabel.Spring = true;
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainMenu
            // 
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.EditMenu,
            this.ViewMenu});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(839, 24);
            this.MainMenu.TabIndex = 5;
            this.MainMenu.Text = "Main Menu";
            // 
            // FileMenu
            // 
            this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileNewMenu,
            this.FileOpenMenu,
            this.FileSaveMenu,
            this.FileSaveAsMenu,
            this.toolStripSeparator1,
            this.FileCloseMenu});
            this.FileMenu.Name = "FileMenu";
            this.FileMenu.Size = new System.Drawing.Size(37, 20);
            this.FileMenu.Text = "File";
            // 
            // FileNewMenu
            // 
            this.FileNewMenu.Name = "FileNewMenu";
            this.FileNewMenu.Size = new System.Drawing.Size(145, 22);
            this.FileNewMenu.Text = "New";
            this.FileNewMenu.Click += new System.EventHandler(this.FileNewMenu_Click);
            // 
            // FileOpenMenu
            // 
            this.FileOpenMenu.Name = "FileOpenMenu";
            this.FileOpenMenu.Size = new System.Drawing.Size(145, 22);
            this.FileOpenMenu.Text = "Open...";
            this.FileOpenMenu.Click += new System.EventHandler(this.FileOpenMenu_Click);
            // 
            // FileSaveMenu
            // 
            this.FileSaveMenu.Name = "FileSaveMenu";
            this.FileSaveMenu.Size = new System.Drawing.Size(145, 22);
            this.FileSaveMenu.Text = "Save";
            this.FileSaveMenu.Click += new System.EventHandler(this.FileSaveMenu_Click);
            // 
            // FileSaveAsMenu
            // 
            this.FileSaveAsMenu.Name = "FileSaveAsMenu";
            this.FileSaveAsMenu.Size = new System.Drawing.Size(145, 22);
            this.FileSaveAsMenu.Text = "Save As...";
            this.FileSaveAsMenu.Click += new System.EventHandler(this.FileSaveAsMenu_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(142, 6);
            // 
            // FileCloseMenu
            // 
            this.FileCloseMenu.Name = "FileCloseMenu";
            this.FileCloseMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.FileCloseMenu.Size = new System.Drawing.Size(145, 22);
            this.FileCloseMenu.Text = "Close";
            this.FileCloseMenu.Click += new System.EventHandler(this.FileCloseMenu_Click);
            // 
            // EditMenu
            // 
            this.EditMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wIPToolStripMenuItem});
            this.EditMenu.Enabled = false;
            this.EditMenu.Name = "EditMenu";
            this.EditMenu.Size = new System.Drawing.Size(39, 20);
            this.EditMenu.Text = "Edit";
            // 
            // wIPToolStripMenuItem
            // 
            this.wIPToolStripMenuItem.Name = "wIPToolStripMenuItem";
            this.wIPToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.wIPToolStripMenuItem.Text = "[WIP!]";
            // 
            // ViewMenu
            // 
            this.ViewMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wIPToolStripMenuItem1});
            this.ViewMenu.Enabled = false;
            this.ViewMenu.Name = "ViewMenu";
            this.ViewMenu.Size = new System.Drawing.Size(44, 20);
            this.ViewMenu.Text = "View";
            // 
            // wIPToolStripMenuItem1
            // 
            this.wIPToolStripMenuItem1.Name = "wIPToolStripMenuItem1";
            this.wIPToolStripMenuItem1.Size = new System.Drawing.Size(106, 22);
            this.wIPToolStripMenuItem1.Text = "[WIP!]";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 49);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(839, 486);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.XmlTextBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(831, 460);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "XML";
            this.tabPage1.UseVisualStyleBackColor = true;
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
            this.XmlTextBox.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.XmlTextBox.IsReplaceMode = false;
            this.XmlTextBox.Language = FastColoredTextBoxNS.Language.XML;
            this.XmlTextBox.LeftBracket = '<';
            this.XmlTextBox.LeftBracket2 = '(';
            this.XmlTextBox.Location = new System.Drawing.Point(3, 3);
            this.XmlTextBox.Name = "XmlTextBox";
            this.XmlTextBox.Paddings = new System.Windows.Forms.Padding(0);
            this.XmlTextBox.RightBracket = '>';
            this.XmlTextBox.RightBracket2 = ')';
            this.XmlTextBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.XmlTextBox.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("XmlTextBox.ServiceColors")));
            this.XmlTextBox.Size = new System.Drawing.Size(825, 454);
            this.XmlTextBox.TabIndex = 1;
            this.XmlTextBox.Zoom = 100;
            this.XmlTextBox.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.XmlTextBox_TextChanged);
            this.XmlTextBox.VisibleRangeChangedDelayed += new System.EventHandler(this.XmlTextBox_VisibleRangeChangedDelayed);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.RawPropertyGrid);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(831, 460);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Raw Data";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // RawPropertyGrid
            // 
            this.RawPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RawPropertyGrid.HelpVisible = false;
            this.RawPropertyGrid.Location = new System.Drawing.Point(3, 3);
            this.RawPropertyGrid.Name = "RawPropertyGrid";
            this.RawPropertyGrid.Size = new System.Drawing.Size(825, 454);
            this.RawPropertyGrid.TabIndex = 0;
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.Filter = "XML files|*.xml|All files|*.*";
            // 
            // SaveFileDialog
            // 
            this.SaveFileDialog.Filter = "XML files|*.xml|All files|*.*";
            // 
            // MetaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(839, 557);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.MainToolbar);
            this.Controls.Add(this.MainStatusStrip);
            this.Controls.Add(this.MainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MetaForm";
            this.Text = "Meta Editor - CodeWalker by dexyfex";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MetaForm_FormClosing);
            this.MainToolbar.ResumeLayout(false);
            this.MainToolbar.PerformLayout();
            this.MainStatusStrip.ResumeLayout(false);
            this.MainStatusStrip.PerformLayout();
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.XmlTextBox)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip MainToolbar;
        private System.Windows.Forms.ToolStripSplitButton NewButton;
        private System.Windows.Forms.ToolStripSplitButton OpenButton;
        private System.Windows.Forms.ToolStripSplitButton SaveButton;
        private System.Windows.Forms.StatusStrip MainStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem FileMenu;
        private System.Windows.Forms.ToolStripMenuItem EditMenu;
        private System.Windows.Forms.ToolStripMenuItem ViewMenu;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private FastColoredTextBoxNS.FastColoredTextBox XmlTextBox;
        private WinForms.PropertyGridFix RawPropertyGrid;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
        private System.Windows.Forms.ToolStripMenuItem FileNewMenu;
        private System.Windows.Forms.ToolStripMenuItem FileOpenMenu;
        private System.Windows.Forms.ToolStripMenuItem FileSaveMenu;
        private System.Windows.Forms.ToolStripMenuItem FileSaveAsMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem FileCloseMenu;
        private System.Windows.Forms.ToolStripMenuItem wIPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wIPToolStripMenuItem1;
    }
}