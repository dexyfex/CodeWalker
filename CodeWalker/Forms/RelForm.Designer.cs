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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RelForm));
            RelPropertyGrid = new WinForms.PropertyGridFix();
            MainTabControl = new System.Windows.Forms.TabControl();
            XmlTabPage = new System.Windows.Forms.TabPage();
            XmlTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
            DetailsTabPage = new System.Windows.Forms.TabPage();
            NameTableTabPage = new System.Windows.Forms.TabPage();
            MainTextBox = new WinForms.TextBoxFix();
            SearchTabPage = new System.Windows.Forms.TabPage();
            SearchTextRadio = new System.Windows.Forms.RadioButton();
            SearchHashRadio = new System.Windows.Forms.RadioButton();
            label12 = new System.Windows.Forms.Label();
            SearchTextBox = new System.Windows.Forms.TextBox();
            SearchButton = new System.Windows.Forms.Button();
            SearchResultsGrid = new WinForms.PropertyGridFix();
            SynthsTabPage = new System.Windows.Forms.TabPage();
            SynthStopButton = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            SynthVariablesTextBox = new System.Windows.Forms.TextBox();
            SynthOutputsTextBox = new System.Windows.Forms.TextBox();
            SynthPlayButton = new System.Windows.Forms.Button();
            SynthCopyXMLButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            SynthsComboBox = new System.Windows.Forms.ComboBox();
            SynthTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
            MainToolbar = new System.Windows.Forms.ToolStrip();
            NewButton = new System.Windows.Forms.ToolStripSplitButton();
            OpenButton = new System.Windows.Forms.ToolStripSplitButton();
            SaveButton = new System.Windows.Forms.ToolStripSplitButton();
            MainStatusStrip = new System.Windows.Forms.StatusStrip();
            StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            MainMenu = new System.Windows.Forms.MenuStrip();
            FileMenu = new System.Windows.Forms.ToolStripMenuItem();
            FileNewMenu = new System.Windows.Forms.ToolStripMenuItem();
            FileOpenMenu = new System.Windows.Forms.ToolStripMenuItem();
            FileSaveMenu = new System.Windows.Forms.ToolStripMenuItem();
            FileSaveAsMenu = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            FileCloseMenu = new System.Windows.Forms.ToolStripMenuItem();
            EditMenu = new System.Windows.Forms.ToolStripMenuItem();
            wIPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ViewMenu = new System.Windows.Forms.ToolStripMenuItem();
            wIPToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            MainTabControl.SuspendLayout();
            XmlTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)XmlTextBox).BeginInit();
            DetailsTabPage.SuspendLayout();
            NameTableTabPage.SuspendLayout();
            SearchTabPage.SuspendLayout();
            SynthsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)SynthTextBox).BeginInit();
            MainToolbar.SuspendLayout();
            MainStatusStrip.SuspendLayout();
            MainMenu.SuspendLayout();
            SuspendLayout();
            // 
            // RelPropertyGrid
            // 
            RelPropertyGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            RelPropertyGrid.HelpVisible = false;
            RelPropertyGrid.Location = new System.Drawing.Point(7, 7);
            RelPropertyGrid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            RelPropertyGrid.Name = "RelPropertyGrid";
            RelPropertyGrid.Size = new System.Drawing.Size(955, 517);
            RelPropertyGrid.TabIndex = 0;
            // 
            // MainTabControl
            // 
            MainTabControl.Controls.Add(XmlTabPage);
            MainTabControl.Controls.Add(DetailsTabPage);
            MainTabControl.Controls.Add(NameTableTabPage);
            MainTabControl.Controls.Add(SearchTabPage);
            MainTabControl.Controls.Add(SynthsTabPage);
            MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            MainTabControl.Location = new System.Drawing.Point(0, 49);
            MainTabControl.Margin = new System.Windows.Forms.Padding(0);
            MainTabControl.Name = "MainTabControl";
            MainTabControl.SelectedIndex = 0;
            MainTabControl.Size = new System.Drawing.Size(979, 572);
            MainTabControl.TabIndex = 1;
            // 
            // XmlTabPage
            // 
            XmlTabPage.Controls.Add(XmlTextBox);
            XmlTabPage.Location = new System.Drawing.Point(4, 24);
            XmlTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            XmlTabPage.Name = "XmlTabPage";
            XmlTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            XmlTabPage.Size = new System.Drawing.Size(971, 544);
            XmlTabPage.TabIndex = 3;
            XmlTabPage.Text = "XML";
            XmlTabPage.UseVisualStyleBackColor = true;
            // 
            // XmlTextBox
            // 
            XmlTextBox.AutoCompleteBracketsList = new char[] { '(', ')', '{', '}', '[', ']', '"', '"', '\'', '\'' };
            XmlTextBox.AutoIndentChars = false;
            XmlTextBox.AutoIndentCharsPatterns = "";
            XmlTextBox.AutoIndentExistingLines = false;
            XmlTextBox.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            XmlTextBox.BackBrush = null;
            XmlTextBox.CharHeight = 14;
            XmlTextBox.CharWidth = 8;
            XmlTextBox.CommentPrefix = null;
            XmlTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            XmlTextBox.DelayedEventsInterval = 1;
            XmlTextBox.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            XmlTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            XmlTextBox.IsReplaceMode = false;
            XmlTextBox.Language = FastColoredTextBoxNS.Language.XML;
            XmlTextBox.LeftBracket = '<';
            XmlTextBox.LeftBracket2 = '(';
            XmlTextBox.Location = new System.Drawing.Point(4, 3);
            XmlTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            XmlTextBox.Name = "XmlTextBox";
            XmlTextBox.Paddings = new System.Windows.Forms.Padding(0);
            XmlTextBox.RightBracket = '>';
            XmlTextBox.RightBracket2 = ')';
            XmlTextBox.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            XmlTextBox.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("XmlTextBox.ServiceColors");
            XmlTextBox.Size = new System.Drawing.Size(963, 538);
            XmlTextBox.TabIndex = 2;
            XmlTextBox.Zoom = 100;
            XmlTextBox.TextChanged += XmlTextBox_TextChanged;
            XmlTextBox.VisibleRangeChangedDelayed += XmlTextBox_VisibleRangeChangedDelayed;
            // 
            // DetailsTabPage
            // 
            DetailsTabPage.Controls.Add(RelPropertyGrid);
            DetailsTabPage.Location = new System.Drawing.Point(4, 24);
            DetailsTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            DetailsTabPage.Name = "DetailsTabPage";
            DetailsTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            DetailsTabPage.Size = new System.Drawing.Size(971, 533);
            DetailsTabPage.TabIndex = 1;
            DetailsTabPage.Text = "Details";
            DetailsTabPage.UseVisualStyleBackColor = true;
            // 
            // NameTableTabPage
            // 
            NameTableTabPage.Controls.Add(MainTextBox);
            NameTableTabPage.Location = new System.Drawing.Point(4, 24);
            NameTableTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NameTableTabPage.Name = "NameTableTabPage";
            NameTableTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NameTableTabPage.Size = new System.Drawing.Size(971, 544);
            NameTableTabPage.TabIndex = 0;
            NameTableTabPage.Text = "Names";
            NameTableTabPage.UseVisualStyleBackColor = true;
            // 
            // MainTextBox
            // 
            MainTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            MainTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            MainTextBox.HideSelection = false;
            MainTextBox.Location = new System.Drawing.Point(7, 7);
            MainTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MainTextBox.Multiline = true;
            MainTextBox.Name = "MainTextBox";
            MainTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            MainTextBox.Size = new System.Drawing.Size(955, 527);
            MainTextBox.TabIndex = 1;
            MainTextBox.WordWrap = false;
            // 
            // SearchTabPage
            // 
            SearchTabPage.Controls.Add(SearchTextRadio);
            SearchTabPage.Controls.Add(SearchHashRadio);
            SearchTabPage.Controls.Add(label12);
            SearchTabPage.Controls.Add(SearchTextBox);
            SearchTabPage.Controls.Add(SearchButton);
            SearchTabPage.Controls.Add(SearchResultsGrid);
            SearchTabPage.Location = new System.Drawing.Point(4, 24);
            SearchTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SearchTabPage.Name = "SearchTabPage";
            SearchTabPage.Size = new System.Drawing.Size(971, 533);
            SearchTabPage.TabIndex = 2;
            SearchTabPage.Text = "Search";
            SearchTabPage.UseVisualStyleBackColor = true;
            // 
            // SearchTextRadio
            // 
            SearchTextRadio.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            SearchTextRadio.AutoSize = true;
            SearchTextRadio.Location = new System.Drawing.Point(612, 5);
            SearchTextRadio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SearchTextRadio.Name = "SearchTextRadio";
            SearchTextRadio.Size = new System.Drawing.Size(46, 19);
            SearchTextRadio.TabIndex = 36;
            SearchTextRadio.Text = "Text";
            SearchTextRadio.UseVisualStyleBackColor = true;
            // 
            // SearchHashRadio
            // 
            SearchHashRadio.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            SearchHashRadio.AutoSize = true;
            SearchHashRadio.Checked = true;
            SearchHashRadio.Location = new System.Drawing.Point(546, 5);
            SearchHashRadio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SearchHashRadio.Name = "SearchHashRadio";
            SearchHashRadio.Size = new System.Drawing.Size(52, 19);
            SearchHashRadio.TabIndex = 35;
            SearchHashRadio.TabStop = true;
            SearchHashRadio.Text = "Hash";
            SearchHashRadio.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(9, 7);
            label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(33, 15);
            label12.TabIndex = 32;
            label12.Text = "Find:";
            // 
            // SearchTextBox
            // 
            SearchTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SearchTextBox.Location = new System.Drawing.Point(51, 3);
            SearchTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SearchTextBox.Name = "SearchTextBox";
            SearchTextBox.Size = new System.Drawing.Size(480, 23);
            SearchTextBox.TabIndex = 33;
            SearchTextBox.KeyDown += SearchTextBox_KeyDown;
            // 
            // SearchButton
            // 
            SearchButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            SearchButton.Location = new System.Drawing.Point(665, 2);
            SearchButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SearchButton.Name = "SearchButton";
            SearchButton.Size = new System.Drawing.Size(79, 27);
            SearchButton.TabIndex = 34;
            SearchButton.Text = "Search";
            SearchButton.UseVisualStyleBackColor = true;
            SearchButton.Click += SearchButton_Click;
            // 
            // SearchResultsGrid
            // 
            SearchResultsGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SearchResultsGrid.HelpVisible = false;
            SearchResultsGrid.Location = new System.Drawing.Point(4, 36);
            SearchResultsGrid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SearchResultsGrid.Name = "SearchResultsGrid";
            SearchResultsGrid.Size = new System.Drawing.Size(959, 488);
            SearchResultsGrid.TabIndex = 1;
            // 
            // SynthsTabPage
            // 
            SynthsTabPage.Controls.Add(SynthStopButton);
            SynthsTabPage.Controls.Add(label3);
            SynthsTabPage.Controls.Add(label2);
            SynthsTabPage.Controls.Add(SynthVariablesTextBox);
            SynthsTabPage.Controls.Add(SynthOutputsTextBox);
            SynthsTabPage.Controls.Add(SynthPlayButton);
            SynthsTabPage.Controls.Add(SynthCopyXMLButton);
            SynthsTabPage.Controls.Add(label1);
            SynthsTabPage.Controls.Add(SynthsComboBox);
            SynthsTabPage.Controls.Add(SynthTextBox);
            SynthsTabPage.Location = new System.Drawing.Point(4, 24);
            SynthsTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SynthsTabPage.Name = "SynthsTabPage";
            SynthsTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SynthsTabPage.Size = new System.Drawing.Size(971, 533);
            SynthsTabPage.TabIndex = 4;
            SynthsTabPage.Text = "Synths";
            SynthsTabPage.UseVisualStyleBackColor = true;
            // 
            // SynthStopButton
            // 
            SynthStopButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            SynthStopButton.Enabled = false;
            SynthStopButton.Location = new System.Drawing.Point(845, 2);
            SynthStopButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SynthStopButton.Name = "SynthStopButton";
            SynthStopButton.Size = new System.Drawing.Size(121, 27);
            SynthStopButton.TabIndex = 43;
            SynthStopButton.Text = "Stop";
            SynthStopButton.UseVisualStyleBackColor = true;
            SynthStopButton.Click += SynthStopButton_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(9, 68);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(56, 15);
            label3.TabIndex = 42;
            label3.Text = "Variables:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(9, 38);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(53, 15);
            label2.TabIndex = 41;
            label2.Text = "Outputs:";
            // 
            // SynthVariablesTextBox
            // 
            SynthVariablesTextBox.Location = new System.Drawing.Point(75, 65);
            SynthVariablesTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SynthVariablesTextBox.Multiline = true;
            SynthVariablesTextBox.Name = "SynthVariablesTextBox";
            SynthVariablesTextBox.Size = new System.Drawing.Size(341, 100);
            SynthVariablesTextBox.TabIndex = 40;
            SynthVariablesTextBox.TextChanged += SynthVariablesTextBox_TextChanged;
            // 
            // SynthOutputsTextBox
            // 
            SynthOutputsTextBox.Location = new System.Drawing.Point(75, 35);
            SynthOutputsTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SynthOutputsTextBox.Name = "SynthOutputsTextBox";
            SynthOutputsTextBox.Size = new System.Drawing.Size(341, 23);
            SynthOutputsTextBox.TabIndex = 39;
            SynthOutputsTextBox.TextChanged += SynthOutputsTextBox_TextChanged;
            // 
            // SynthPlayButton
            // 
            SynthPlayButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            SynthPlayButton.Location = new System.Drawing.Point(718, 2);
            SynthPlayButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SynthPlayButton.Name = "SynthPlayButton";
            SynthPlayButton.Size = new System.Drawing.Size(121, 27);
            SynthPlayButton.TabIndex = 37;
            SynthPlayButton.Text = "Play";
            SynthPlayButton.UseVisualStyleBackColor = true;
            SynthPlayButton.Click += SynthPlayButton_Click;
            // 
            // SynthCopyXMLButton
            // 
            SynthCopyXMLButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            SynthCopyXMLButton.Location = new System.Drawing.Point(540, 2);
            SynthCopyXMLButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SynthCopyXMLButton.Name = "SynthCopyXMLButton";
            SynthCopyXMLButton.Size = new System.Drawing.Size(170, 27);
            SynthCopyXMLButton.TabIndex = 35;
            SynthCopyXMLButton.Text = "Copy XML to clipboard";
            SynthCopyXMLButton.UseVisualStyleBackColor = true;
            SynthCopyXMLButton.Click += SynthCopyXMLButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(9, 7);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(40, 15);
            label1.TabIndex = 33;
            label1.Text = "Synth:";
            // 
            // SynthsComboBox
            // 
            SynthsComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SynthsComboBox.FormattingEnabled = true;
            SynthsComboBox.Location = new System.Drawing.Point(75, 3);
            SynthsComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SynthsComboBox.Name = "SynthsComboBox";
            SynthsComboBox.Size = new System.Drawing.Size(458, 23);
            SynthsComboBox.TabIndex = 4;
            SynthsComboBox.SelectedIndexChanged += SynthsComboBox_SelectedIndexChanged;
            // 
            // SynthTextBox
            // 
            SynthTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SynthTextBox.AutoCompleteBracketsList = new char[] { '(', ')', '{', '}', '[', ']', '"', '"', '\'', '\'' };
            SynthTextBox.AutoIndentChars = false;
            SynthTextBox.AutoIndentCharsPatterns = "";
            SynthTextBox.AutoIndentExistingLines = false;
            SynthTextBox.AutoScrollMinSize = new System.Drawing.Size(2, 14);
            SynthTextBox.BackBrush = null;
            SynthTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            SynthTextBox.CharHeight = 14;
            SynthTextBox.CharWidth = 8;
            SynthTextBox.CommentPrefix = null;
            SynthTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            SynthTextBox.DelayedEventsInterval = 1;
            SynthTextBox.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            SynthTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            SynthTextBox.IsReplaceMode = false;
            SynthTextBox.Language = FastColoredTextBoxNS.Language.XML;
            SynthTextBox.LeftBracket = '<';
            SynthTextBox.LeftBracket2 = '(';
            SynthTextBox.Location = new System.Drawing.Point(4, 172);
            SynthTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SynthTextBox.Name = "SynthTextBox";
            SynthTextBox.Paddings = new System.Windows.Forms.Padding(0);
            SynthTextBox.RightBracket = '>';
            SynthTextBox.RightBracket2 = ')';
            SynthTextBox.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            SynthTextBox.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("SynthTextBox.ServiceColors");
            SynthTextBox.Size = new System.Drawing.Size(962, 355);
            SynthTextBox.TabIndex = 3;
            SynthTextBox.Zoom = 100;
            SynthTextBox.TextChangedDelayed += SynthTextBox_TextChangedDelayed;
            // 
            // MainToolbar
            // 
            MainToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { NewButton, OpenButton, SaveButton });
            MainToolbar.Location = new System.Drawing.Point(0, 24);
            MainToolbar.Name = "MainToolbar";
            MainToolbar.Size = new System.Drawing.Size(979, 25);
            MainToolbar.TabIndex = 9;
            MainToolbar.Text = "Main Toolbar";
            // 
            // NewButton
            // 
            NewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            NewButton.Image = (System.Drawing.Image)resources.GetObject("NewButton.Image");
            NewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            NewButton.Name = "NewButton";
            NewButton.Size = new System.Drawing.Size(32, 22);
            NewButton.Text = "New...";
            NewButton.ButtonClick += NewButton_ButtonClick;
            // 
            // OpenButton
            // 
            OpenButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            OpenButton.Image = (System.Drawing.Image)resources.GetObject("OpenButton.Image");
            OpenButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            OpenButton.Name = "OpenButton";
            OpenButton.Size = new System.Drawing.Size(32, 22);
            OpenButton.Text = "Open...";
            OpenButton.ButtonClick += OpenButton_ButtonClick;
            // 
            // SaveButton
            // 
            SaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            SaveButton.Image = (System.Drawing.Image)resources.GetObject("SaveButton.Image");
            SaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new System.Drawing.Size(32, 22);
            SaveButton.Text = "Save";
            SaveButton.ButtonClick += SaveButton_ButtonClick;
            // 
            // MainStatusStrip
            // 
            MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { StatusLabel });
            MainStatusStrip.Location = new System.Drawing.Point(0, 621);
            MainStatusStrip.Name = "MainStatusStrip";
            MainStatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            MainStatusStrip.Size = new System.Drawing.Size(979, 22);
            MainStatusStrip.TabIndex = 7;
            MainStatusStrip.Text = "Main Status Strip";
            // 
            // StatusLabel
            // 
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new System.Drawing.Size(962, 17);
            StatusLabel.Spring = true;
            StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainMenu
            // 
            MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { FileMenu, EditMenu, ViewMenu });
            MainMenu.Location = new System.Drawing.Point(0, 0);
            MainMenu.Name = "MainMenu";
            MainMenu.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            MainMenu.Size = new System.Drawing.Size(979, 24);
            MainMenu.TabIndex = 8;
            MainMenu.Text = "Main Menu";
            // 
            // FileMenu
            // 
            FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { FileNewMenu, FileOpenMenu, FileSaveMenu, FileSaveAsMenu, toolStripSeparator1, FileCloseMenu });
            FileMenu.Name = "FileMenu";
            FileMenu.Size = new System.Drawing.Size(37, 20);
            FileMenu.Text = "File";
            // 
            // FileNewMenu
            // 
            FileNewMenu.Name = "FileNewMenu";
            FileNewMenu.Size = new System.Drawing.Size(145, 22);
            FileNewMenu.Text = "New";
            FileNewMenu.Click += FileNewMenu_Click;
            // 
            // FileOpenMenu
            // 
            FileOpenMenu.Name = "FileOpenMenu";
            FileOpenMenu.Size = new System.Drawing.Size(145, 22);
            FileOpenMenu.Text = "Open...";
            FileOpenMenu.Click += FileOpenMenu_Click;
            // 
            // FileSaveMenu
            // 
            FileSaveMenu.Name = "FileSaveMenu";
            FileSaveMenu.Size = new System.Drawing.Size(145, 22);
            FileSaveMenu.Text = "Save";
            FileSaveMenu.Click += FileSaveMenu_Click;
            // 
            // FileSaveAsMenu
            // 
            FileSaveAsMenu.Name = "FileSaveAsMenu";
            FileSaveAsMenu.Size = new System.Drawing.Size(145, 22);
            FileSaveAsMenu.Text = "Save As...";
            FileSaveAsMenu.Click += FileSaveAsMenu_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(142, 6);
            // 
            // FileCloseMenu
            // 
            FileCloseMenu.Name = "FileCloseMenu";
            FileCloseMenu.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4;
            FileCloseMenu.Size = new System.Drawing.Size(145, 22);
            FileCloseMenu.Text = "Close";
            FileCloseMenu.Click += FileCloseMenu_Click;
            // 
            // EditMenu
            // 
            EditMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { wIPToolStripMenuItem });
            EditMenu.Enabled = false;
            EditMenu.Name = "EditMenu";
            EditMenu.Size = new System.Drawing.Size(39, 20);
            EditMenu.Text = "Edit";
            // 
            // wIPToolStripMenuItem
            // 
            wIPToolStripMenuItem.Name = "wIPToolStripMenuItem";
            wIPToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            wIPToolStripMenuItem.Text = "[WIP!]";
            // 
            // ViewMenu
            // 
            ViewMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { wIPToolStripMenuItem1 });
            ViewMenu.Enabled = false;
            ViewMenu.Name = "ViewMenu";
            ViewMenu.Size = new System.Drawing.Size(44, 20);
            ViewMenu.Text = "View";
            // 
            // wIPToolStripMenuItem1
            // 
            wIPToolStripMenuItem1.Name = "wIPToolStripMenuItem1";
            wIPToolStripMenuItem1.Size = new System.Drawing.Size(106, 22);
            wIPToolStripMenuItem1.Text = "[WIP!]";
            // 
            // OpenFileDialog
            // 
            OpenFileDialog.Filter = "XML files|*.xml|All files|*.*";
            // 
            // SaveFileDialog
            // 
            SaveFileDialog.Filter = "XML files|*.xml|All files|*.*";
            // 
            // RelForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(979, 643);
            Controls.Add(MainTabControl);
            Controls.Add(MainToolbar);
            Controls.Add(MainStatusStrip);
            Controls.Add(MainMenu);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "RelForm";
            Text = "Audio dat.rel Editor - CodeWalker by dexyfex";
            MainTabControl.ResumeLayout(false);
            XmlTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)XmlTextBox).EndInit();
            DetailsTabPage.ResumeLayout(false);
            NameTableTabPage.ResumeLayout(false);
            NameTableTabPage.PerformLayout();
            SearchTabPage.ResumeLayout(false);
            SearchTabPage.PerformLayout();
            SynthsTabPage.ResumeLayout(false);
            SynthsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)SynthTextBox).EndInit();
            MainToolbar.ResumeLayout(false);
            MainToolbar.PerformLayout();
            MainStatusStrip.ResumeLayout(false);
            MainStatusStrip.PerformLayout();
            MainMenu.ResumeLayout(false);
            MainMenu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private WinForms.PropertyGridFix RelPropertyGrid;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage NameTableTabPage;
        private System.Windows.Forms.TabPage DetailsTabPage;
        private WinForms.TextBoxFix MainTextBox;
        private System.Windows.Forms.TabPage SearchTabPage;
        private WinForms.PropertyGridFix SearchResultsGrid;
        private System.Windows.Forms.RadioButton SearchTextRadio;
        private System.Windows.Forms.RadioButton SearchHashRadio;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox SearchTextBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.TabPage XmlTabPage;
        private FastColoredTextBoxNS.FastColoredTextBox XmlTextBox;
        private System.Windows.Forms.ToolStrip MainToolbar;
        private System.Windows.Forms.ToolStripSplitButton NewButton;
        private System.Windows.Forms.ToolStripSplitButton OpenButton;
        private System.Windows.Forms.ToolStripSplitButton SaveButton;
        private System.Windows.Forms.StatusStrip MainStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem FileMenu;
        private System.Windows.Forms.ToolStripMenuItem FileNewMenu;
        private System.Windows.Forms.ToolStripMenuItem FileOpenMenu;
        private System.Windows.Forms.ToolStripMenuItem FileSaveMenu;
        private System.Windows.Forms.ToolStripMenuItem FileSaveAsMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem FileCloseMenu;
        private System.Windows.Forms.ToolStripMenuItem EditMenu;
        private System.Windows.Forms.ToolStripMenuItem wIPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewMenu;
        private System.Windows.Forms.ToolStripMenuItem wIPToolStripMenuItem1;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
        private System.Windows.Forms.TabPage SynthsTabPage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox SynthsComboBox;
        private FastColoredTextBoxNS.FastColoredTextBox SynthTextBox;
        private System.Windows.Forms.Button SynthCopyXMLButton;
        private System.Windows.Forms.Button SynthPlayButton;
        //private System.Windows.Forms.DataVisualization.Charting.Chart SynthBufferChart;
        private System.Windows.Forms.TextBox SynthOutputsTextBox;
        private System.Windows.Forms.TextBox SynthVariablesTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button SynthStopButton;
    }
}