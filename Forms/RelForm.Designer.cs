namespace CodeWalker.Forms
{
    partial class RelForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RelForm));
            this.RelPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.DetailsTabPage = new System.Windows.Forms.TabPage();
            this.NameTableTabPage = new System.Windows.Forms.TabPage();
            this.MainTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.SearchTabPage = new System.Windows.Forms.TabPage();
            this.SearchTextRadio = new System.Windows.Forms.RadioButton();
            this.SearchHashRadio = new System.Windows.Forms.RadioButton();
            this.label12 = new System.Windows.Forms.Label();
            this.SearchTextBox = new System.Windows.Forms.TextBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.SearchResultsGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.CloseButton = new System.Windows.Forms.Button();
            this.XmlTabPage = new System.Windows.Forms.TabPage();
            this.XmlTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
            this.MainTabControl.SuspendLayout();
            this.DetailsTabPage.SuspendLayout();
            this.NameTableTabPage.SuspendLayout();
            this.SearchTabPage.SuspendLayout();
            this.XmlTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XmlTextBox)).BeginInit();
            this.SuspendLayout();
            // 
            // RelPropertyGrid
            // 
            this.RelPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RelPropertyGrid.HelpVisible = false;
            this.RelPropertyGrid.Location = new System.Drawing.Point(6, 6);
            this.RelPropertyGrid.Name = "RelPropertyGrid";
            this.RelPropertyGrid.Size = new System.Drawing.Size(644, 356);
            this.RelPropertyGrid.TabIndex = 0;
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTabControl.Controls.Add(this.XmlTabPage);
            this.MainTabControl.Controls.Add(this.DetailsTabPage);
            this.MainTabControl.Controls.Add(this.NameTableTabPage);
            this.MainTabControl.Controls.Add(this.SearchTabPage);
            this.MainTabControl.Location = new System.Drawing.Point(5, 5);
            this.MainTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(664, 394);
            this.MainTabControl.TabIndex = 1;
            // 
            // DetailsTabPage
            // 
            this.DetailsTabPage.Controls.Add(this.RelPropertyGrid);
            this.DetailsTabPage.Location = new System.Drawing.Point(4, 22);
            this.DetailsTabPage.Name = "DetailsTabPage";
            this.DetailsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DetailsTabPage.Size = new System.Drawing.Size(656, 368);
            this.DetailsTabPage.TabIndex = 1;
            this.DetailsTabPage.Text = "Details";
            this.DetailsTabPage.UseVisualStyleBackColor = true;
            // 
            // NameTableTabPage
            // 
            this.NameTableTabPage.Controls.Add(this.MainTextBox);
            this.NameTableTabPage.Location = new System.Drawing.Point(4, 22);
            this.NameTableTabPage.Name = "NameTableTabPage";
            this.NameTableTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.NameTableTabPage.Size = new System.Drawing.Size(656, 368);
            this.NameTableTabPage.TabIndex = 0;
            this.NameTableTabPage.Text = "Names";
            this.NameTableTabPage.UseVisualStyleBackColor = true;
            // 
            // MainTextBox
            // 
            this.MainTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainTextBox.HideSelection = false;
            this.MainTextBox.Location = new System.Drawing.Point(6, 6);
            this.MainTextBox.Multiline = true;
            this.MainTextBox.Name = "MainTextBox";
            this.MainTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.MainTextBox.Size = new System.Drawing.Size(644, 356);
            this.MainTextBox.TabIndex = 1;
            this.MainTextBox.WordWrap = false;
            // 
            // SearchTabPage
            // 
            this.SearchTabPage.Controls.Add(this.SearchTextRadio);
            this.SearchTabPage.Controls.Add(this.SearchHashRadio);
            this.SearchTabPage.Controls.Add(this.label12);
            this.SearchTabPage.Controls.Add(this.SearchTextBox);
            this.SearchTabPage.Controls.Add(this.SearchButton);
            this.SearchTabPage.Controls.Add(this.SearchResultsGrid);
            this.SearchTabPage.Location = new System.Drawing.Point(4, 22);
            this.SearchTabPage.Name = "SearchTabPage";
            this.SearchTabPage.Size = new System.Drawing.Size(656, 368);
            this.SearchTabPage.TabIndex = 2;
            this.SearchTabPage.Text = "Search";
            this.SearchTabPage.UseVisualStyleBackColor = true;
            // 
            // SearchTextRadio
            // 
            this.SearchTextRadio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchTextRadio.AutoSize = true;
            this.SearchTextRadio.Location = new System.Drawing.Point(343, 4);
            this.SearchTextRadio.Name = "SearchTextRadio";
            this.SearchTextRadio.Size = new System.Drawing.Size(46, 17);
            this.SearchTextRadio.TabIndex = 36;
            this.SearchTextRadio.Text = "Text";
            this.SearchTextRadio.UseVisualStyleBackColor = true;
            // 
            // SearchHashRadio
            // 
            this.SearchHashRadio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchHashRadio.AutoSize = true;
            this.SearchHashRadio.Checked = true;
            this.SearchHashRadio.Location = new System.Drawing.Point(287, 4);
            this.SearchHashRadio.Name = "SearchHashRadio";
            this.SearchHashRadio.Size = new System.Drawing.Size(50, 17);
            this.SearchHashRadio.TabIndex = 35;
            this.SearchHashRadio.TabStop = true;
            this.SearchHashRadio.Text = "Hash";
            this.SearchHashRadio.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 6);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(30, 13);
            this.label12.TabIndex = 32;
            this.label12.Text = "Find:";
            // 
            // SearchTextBox
            // 
            this.SearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchTextBox.Location = new System.Drawing.Point(44, 3);
            this.SearchTextBox.Name = "SearchTextBox";
            this.SearchTextBox.Size = new System.Drawing.Size(237, 20);
            this.SearchTextBox.TabIndex = 33;
            this.SearchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchTextBox_KeyDown);
            // 
            // SearchButton
            // 
            this.SearchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchButton.Location = new System.Drawing.Point(395, 2);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(68, 23);
            this.SearchButton.TabIndex = 34;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // SearchResultsGrid
            // 
            this.SearchResultsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchResultsGrid.HelpVisible = false;
            this.SearchResultsGrid.Location = new System.Drawing.Point(3, 31);
            this.SearchResultsGrid.Name = "SearchResultsGrid";
            this.SearchResultsGrid.Size = new System.Drawing.Size(647, 331);
            this.SearchResultsGrid.TabIndex = 1;
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.Location = new System.Drawing.Point(584, 409);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 2;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // XmlTabPage
            // 
            this.XmlTabPage.Controls.Add(this.XmlTextBox);
            this.XmlTabPage.Location = new System.Drawing.Point(4, 22);
            this.XmlTabPage.Name = "XmlTabPage";
            this.XmlTabPage.Size = new System.Drawing.Size(656, 368);
            this.XmlTabPage.TabIndex = 3;
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
            this.XmlTextBox.Size = new System.Drawing.Size(656, 368);
            this.XmlTextBox.TabIndex = 2;
            this.XmlTextBox.Zoom = 100;
            // 
            // RelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 441);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.MainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RelForm";
            this.Text = "REL Viewer - CodeWalker by dexyfex";
            this.MainTabControl.ResumeLayout(false);
            this.DetailsTabPage.ResumeLayout(false);
            this.NameTableTabPage.ResumeLayout(false);
            this.NameTableTabPage.PerformLayout();
            this.SearchTabPage.ResumeLayout(false);
            this.SearchTabPage.PerformLayout();
            this.XmlTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.XmlTextBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private WinForms.PropertyGridFix RelPropertyGrid;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage NameTableTabPage;
        private System.Windows.Forms.TabPage DetailsTabPage;
        private WinForms.TextBoxFix MainTextBox;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.TabPage SearchTabPage;
        private WinForms.PropertyGridFix SearchResultsGrid;
        private System.Windows.Forms.RadioButton SearchTextRadio;
        private System.Windows.Forms.RadioButton SearchHashRadio;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox SearchTextBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.TabPage XmlTabPage;
        private FastColoredTextBoxNS.FastColoredTextBox XmlTextBox;
    }
}