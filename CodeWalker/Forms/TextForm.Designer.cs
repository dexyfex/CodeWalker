namespace CodeWalker.Forms
{
    partial class TextForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextForm));
            this.MainTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
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
            this.ViewMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.MainToolbar = new System.Windows.Forms.ToolStrip();
            this.NewButton = new System.Windows.Forms.ToolStripSplitButton();
            this.OpenButton = new System.Windows.Forms.ToolStripSplitButton();
            this.SaveButton = new System.Windows.Forms.ToolStripSplitButton();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.MainTextBox)).BeginInit();
            this.MainStatusStrip.SuspendLayout();
            this.MainMenu.SuspendLayout();
            this.MainToolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTextBox
            // 
            this.MainTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTextBox.AutoCompleteBracketsList = new char[] {
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
            this.MainTextBox.AutoIndentChars = false;
            this.MainTextBox.AutoIndentCharsPatterns = "";
            this.MainTextBox.AutoIndentExistingLines = false;
            this.MainTextBox.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            this.MainTextBox.BackBrush = null;
            this.MainTextBox.CharHeight = 14;
            this.MainTextBox.CharWidth = 8;
            this.MainTextBox.CommentPrefix = null;
            this.MainTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.MainTextBox.DelayedEventsInterval = 10;
            this.MainTextBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.MainTextBox.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.MainTextBox.IsReplaceMode = false;
            this.MainTextBox.LeftBracket = '<';
            this.MainTextBox.LeftBracket2 = '(';
            this.MainTextBox.Location = new System.Drawing.Point(0, 52);
            this.MainTextBox.Name = "MainTextBox";
            this.MainTextBox.Paddings = new System.Windows.Forms.Padding(0);
            this.MainTextBox.RightBracket = '>';
            this.MainTextBox.RightBracket2 = ')';
            this.MainTextBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.MainTextBox.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("MainTextBox.ServiceColors")));
            this.MainTextBox.Size = new System.Drawing.Size(823, 461);
            this.MainTextBox.TabIndex = 0;
            this.MainTextBox.Zoom = 100;
            this.MainTextBox.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.MainTextBox_TextChanged);
            // 
            // MainStatusStrip
            // 
            this.MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.MainStatusStrip.Location = new System.Drawing.Point(0, 516);
            this.MainStatusStrip.Name = "MainStatusStrip";
            this.MainStatusStrip.Size = new System.Drawing.Size(823, 22);
            this.MainStatusStrip.TabIndex = 1;
            this.MainStatusStrip.Text = "Main Status Strip";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(808, 17);
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
            this.MainMenu.Size = new System.Drawing.Size(823, 24);
            this.MainMenu.TabIndex = 2;
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
            // MainToolbar
            // 
            this.MainToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewButton,
            this.OpenButton,
            this.SaveButton});
            this.MainToolbar.Location = new System.Drawing.Point(0, 24);
            this.MainToolbar.Name = "MainToolbar";
            this.MainToolbar.Size = new System.Drawing.Size(823, 25);
            this.MainToolbar.TabIndex = 3;
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
            // OpenFileDialog
            // 
            this.OpenFileDialog.Filter = "Text files|*.txt;*.gxt2;*.nametable|All files|*.*";
            // 
            // SaveFileDialog
            // 
            this.SaveFileDialog.Filter = "Text files|*.txt;*.gxt2;*.nametable|All files|*.*";
            this.SaveFileDialog.FilterIndex = 2;
            // 
            // TextForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 538);
            this.Controls.Add(this.MainToolbar);
            this.Controls.Add(this.MainStatusStrip);
            this.Controls.Add(this.MainMenu);
            this.Controls.Add(this.MainTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MainMenu;
            this.Name = "TextForm";
            this.Text = "Text Editor - CodeWalker by dexyfex";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TextForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.MainTextBox)).EndInit();
            this.MainStatusStrip.ResumeLayout(false);
            this.MainStatusStrip.PerformLayout();
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.MainToolbar.ResumeLayout(false);
            this.MainToolbar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FastColoredTextBoxNS.FastColoredTextBox MainTextBox;
        private System.Windows.Forms.StatusStrip MainStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem FileMenu;
        private System.Windows.Forms.ToolStripMenuItem EditMenu;
        private System.Windows.Forms.ToolStripMenuItem ViewMenu;
        private System.Windows.Forms.ToolStrip MainToolbar;
        private System.Windows.Forms.ToolStripSplitButton NewButton;
        private System.Windows.Forms.ToolStripSplitButton OpenButton;
        private System.Windows.Forms.ToolStripSplitButton SaveButton;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
        private System.Windows.Forms.ToolStripMenuItem FileNewMenu;
        private System.Windows.Forms.ToolStripMenuItem FileOpenMenu;
        private System.Windows.Forms.ToolStripMenuItem FileSaveMenu;
        private System.Windows.Forms.ToolStripMenuItem FileSaveAsMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem FileCloseMenu;
    }
}