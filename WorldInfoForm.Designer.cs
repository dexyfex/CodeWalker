using CodeWalker.WinForms;

namespace CodeWalker
{
    partial class WorldInfoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorldInfoForm));
            this.SelectionTabControl = new System.Windows.Forms.TabControl();
            this.SelectionEntityTabPage = new System.Windows.Forms.TabPage();
            this.SelEntityPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionArchetypeTabPage = new System.Windows.Forms.TabPage();
            this.SelArchetypePropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionDrawableTabPage = new System.Windows.Forms.TabPage();
            this.SelDrawablePropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionExtensionTabPage = new System.Windows.Forms.TabPage();
            this.SelExtensionPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionModelsTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.SelDrawableModelsTreeView = new CodeWalker.WinForms.TreeViewFix();
            this.SelDrawableModelPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionTexturesTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.SelDrawableTexturesTreeView = new CodeWalker.WinForms.TreeViewFix();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.SelTextureDimensionsLabel = new System.Windows.Forms.Label();
            this.SelTextureMipTrackBar = new System.Windows.Forms.TrackBar();
            this.SelTextureMipLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SelTextureDictionaryTextBox = new System.Windows.Forms.TextBox();
            this.SelTextureNameTextBox = new System.Windows.Forms.TextBox();
            this.SelDrawableTexturePictureBox = new System.Windows.Forms.PictureBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.SelDrawableTexturePropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionHierarchyTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.HierarchyTreeView = new System.Windows.Forms.TreeView();
            this.HierarchyPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.MouseSelectCheckBox = new System.Windows.Forms.CheckBox();
            this.SelectionNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.SelectionModeComboBox = new System.Windows.Forms.ComboBox();
            this.SelectionTabControl.SuspendLayout();
            this.SelectionEntityTabPage.SuspendLayout();
            this.SelectionArchetypeTabPage.SuspendLayout();
            this.SelectionDrawableTabPage.SuspendLayout();
            this.SelectionExtensionTabPage.SuspendLayout();
            this.SelectionModelsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SelectionTexturesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SelTextureMipTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SelDrawableTexturePictureBox)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.SelectionHierarchyTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // SelectionTabControl
            // 
            this.SelectionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectionTabControl.Controls.Add(this.SelectionEntityTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionArchetypeTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionDrawableTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionExtensionTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionModelsTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionTexturesTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionHierarchyTabPage);
            this.SelectionTabControl.Location = new System.Drawing.Point(10, 48);
            this.SelectionTabControl.Name = "SelectionTabControl";
            this.SelectionTabControl.SelectedIndex = 0;
            this.SelectionTabControl.Size = new System.Drawing.Size(735, 480);
            this.SelectionTabControl.TabIndex = 28;
            // 
            // SelectionEntityTabPage
            // 
            this.SelectionEntityTabPage.Controls.Add(this.SelEntityPropertyGrid);
            this.SelectionEntityTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionEntityTabPage.Name = "SelectionEntityTabPage";
            this.SelectionEntityTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SelectionEntityTabPage.Size = new System.Drawing.Size(727, 454);
            this.SelectionEntityTabPage.TabIndex = 0;
            this.SelectionEntityTabPage.Text = "Entity";
            this.SelectionEntityTabPage.UseVisualStyleBackColor = true;
            // 
            // SelEntityPropertyGrid
            // 
            this.SelEntityPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelEntityPropertyGrid.HelpVisible = false;
            this.SelEntityPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.SelEntityPropertyGrid.Name = "SelEntityPropertyGrid";
            this.SelEntityPropertyGrid.Size = new System.Drawing.Size(727, 454);
            this.SelEntityPropertyGrid.TabIndex = 25;
            this.SelEntityPropertyGrid.ToolbarVisible = false;
            // 
            // SelectionArchetypeTabPage
            // 
            this.SelectionArchetypeTabPage.Controls.Add(this.SelArchetypePropertyGrid);
            this.SelectionArchetypeTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionArchetypeTabPage.Name = "SelectionArchetypeTabPage";
            this.SelectionArchetypeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SelectionArchetypeTabPage.Size = new System.Drawing.Size(727, 454);
            this.SelectionArchetypeTabPage.TabIndex = 1;
            this.SelectionArchetypeTabPage.Text = "Archetype";
            this.SelectionArchetypeTabPage.UseVisualStyleBackColor = true;
            // 
            // SelArchetypePropertyGrid
            // 
            this.SelArchetypePropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelArchetypePropertyGrid.HelpVisible = false;
            this.SelArchetypePropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.SelArchetypePropertyGrid.Name = "SelArchetypePropertyGrid";
            this.SelArchetypePropertyGrid.Size = new System.Drawing.Size(727, 454);
            this.SelArchetypePropertyGrid.TabIndex = 26;
            this.SelArchetypePropertyGrid.ToolbarVisible = false;
            // 
            // SelectionDrawableTabPage
            // 
            this.SelectionDrawableTabPage.Controls.Add(this.SelDrawablePropertyGrid);
            this.SelectionDrawableTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionDrawableTabPage.Name = "SelectionDrawableTabPage";
            this.SelectionDrawableTabPage.Size = new System.Drawing.Size(727, 454);
            this.SelectionDrawableTabPage.TabIndex = 2;
            this.SelectionDrawableTabPage.Text = "Drawable";
            this.SelectionDrawableTabPage.UseVisualStyleBackColor = true;
            // 
            // SelDrawablePropertyGrid
            // 
            this.SelDrawablePropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawablePropertyGrid.HelpVisible = false;
            this.SelDrawablePropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.SelDrawablePropertyGrid.Name = "SelDrawablePropertyGrid";
            this.SelDrawablePropertyGrid.Size = new System.Drawing.Size(727, 454);
            this.SelDrawablePropertyGrid.TabIndex = 28;
            this.SelDrawablePropertyGrid.ToolbarVisible = false;
            // 
            // SelectionExtensionTabPage
            // 
            this.SelectionExtensionTabPage.Controls.Add(this.SelExtensionPropertyGrid);
            this.SelectionExtensionTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionExtensionTabPage.Name = "SelectionExtensionTabPage";
            this.SelectionExtensionTabPage.Size = new System.Drawing.Size(727, 454);
            this.SelectionExtensionTabPage.TabIndex = 5;
            this.SelectionExtensionTabPage.Text = "Extension";
            this.SelectionExtensionTabPage.UseVisualStyleBackColor = true;
            // 
            // SelExtensionPropertyGrid
            // 
            this.SelExtensionPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelExtensionPropertyGrid.HelpVisible = false;
            this.SelExtensionPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.SelExtensionPropertyGrid.Name = "SelExtensionPropertyGrid";
            this.SelExtensionPropertyGrid.Size = new System.Drawing.Size(727, 454);
            this.SelExtensionPropertyGrid.TabIndex = 29;
            this.SelExtensionPropertyGrid.ToolbarVisible = false;
            // 
            // SelectionModelsTabPage
            // 
            this.SelectionModelsTabPage.Controls.Add(this.splitContainer1);
            this.SelectionModelsTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionModelsTabPage.Name = "SelectionModelsTabPage";
            this.SelectionModelsTabPage.Size = new System.Drawing.Size(727, 454);
            this.SelectionModelsTabPage.TabIndex = 3;
            this.SelectionModelsTabPage.Text = "Models";
            this.SelectionModelsTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.SelDrawableModelsTreeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.SelDrawableModelPropertyGrid);
            this.splitContainer1.Size = new System.Drawing.Size(727, 454);
            this.splitContainer1.SplitterDistance = 303;
            this.splitContainer1.TabIndex = 2;
            // 
            // SelDrawableModelsTreeView
            // 
            this.SelDrawableModelsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableModelsTreeView.CheckBoxes = true;
            this.SelDrawableModelsTreeView.Location = new System.Drawing.Point(0, 0);
            this.SelDrawableModelsTreeView.Name = "SelDrawableModelsTreeView";
            this.SelDrawableModelsTreeView.Size = new System.Drawing.Size(300, 454);
            this.SelDrawableModelsTreeView.TabIndex = 0;
            this.SelDrawableModelsTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.SelDrawableModelsTreeView_AfterCheck);
            this.SelDrawableModelsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SelDrawableModelsTreeView_AfterSelect);
            // 
            // SelDrawableModelPropertyGrid
            // 
            this.SelDrawableModelPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableModelPropertyGrid.HelpVisible = false;
            this.SelDrawableModelPropertyGrid.Location = new System.Drawing.Point(3, 0);
            this.SelDrawableModelPropertyGrid.Name = "SelDrawableModelPropertyGrid";
            this.SelDrawableModelPropertyGrid.Size = new System.Drawing.Size(414, 454);
            this.SelDrawableModelPropertyGrid.TabIndex = 27;
            this.SelDrawableModelPropertyGrid.ToolbarVisible = false;
            // 
            // SelectionTexturesTabPage
            // 
            this.SelectionTexturesTabPage.Controls.Add(this.splitContainer2);
            this.SelectionTexturesTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionTexturesTabPage.Name = "SelectionTexturesTabPage";
            this.SelectionTexturesTabPage.Size = new System.Drawing.Size(727, 454);
            this.SelectionTexturesTabPage.TabIndex = 4;
            this.SelectionTexturesTabPage.Text = "Textures";
            this.SelectionTexturesTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.SelDrawableTexturesTreeView);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer2.Size = new System.Drawing.Size(727, 454);
            this.splitContainer2.SplitterDistance = 303;
            this.splitContainer2.TabIndex = 1;
            // 
            // SelDrawableTexturesTreeView
            // 
            this.SelDrawableTexturesTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableTexturesTreeView.HideSelection = false;
            this.SelDrawableTexturesTreeView.Location = new System.Drawing.Point(0, 0);
            this.SelDrawableTexturesTreeView.Name = "SelDrawableTexturesTreeView";
            this.SelDrawableTexturesTreeView.Size = new System.Drawing.Size(308, 454);
            this.SelDrawableTexturesTreeView.TabIndex = 2;
            this.SelDrawableTexturesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SelDrawableTexturesTreeView_AfterSelect);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(414, 451);
            this.tabControl1.TabIndex = 31;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.SelTextureDimensionsLabel);
            this.tabPage3.Controls.Add(this.SelTextureMipTrackBar);
            this.tabPage3.Controls.Add(this.SelTextureMipLabel);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Controls.Add(this.SelTextureDictionaryTextBox);
            this.tabPage3.Controls.Add(this.SelTextureNameTextBox);
            this.tabPage3.Controls.Add(this.SelDrawableTexturePictureBox);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(406, 425);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Texture";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // SelTextureDimensionsLabel
            // 
            this.SelTextureDimensionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelTextureDimensionsLabel.AutoSize = true;
            this.SelTextureDimensionsLabel.Location = new System.Drawing.Point(334, 400);
            this.SelTextureDimensionsLabel.Name = "SelTextureDimensionsLabel";
            this.SelTextureDimensionsLabel.Size = new System.Drawing.Size(11, 15);
            this.SelTextureDimensionsLabel.TabIndex = 37;
            this.SelTextureDimensionsLabel.Text = "-";
            // 
            // SelTextureMipTrackBar
            // 
            this.SelTextureMipTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelTextureMipTrackBar.AutoSize = false;
            this.SelTextureMipTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.SelTextureMipTrackBar.LargeChange = 1;
            this.SelTextureMipTrackBar.Location = new System.Drawing.Point(59, 394);
            this.SelTextureMipTrackBar.Maximum = 0;
            this.SelTextureMipTrackBar.Name = "SelTextureMipTrackBar";
            this.SelTextureMipTrackBar.Size = new System.Drawing.Size(265, 31);
            this.SelTextureMipTrackBar.TabIndex = 36;
            this.SelTextureMipTrackBar.Scroll += new System.EventHandler(this.SelTextureMipTrackBar_Scroll);
            // 
            // SelTextureMipLabel
            // 
            this.SelTextureMipLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelTextureMipLabel.AutoSize = true;
            this.SelTextureMipLabel.Location = new System.Drawing.Point(40, 400);
            this.SelTextureMipLabel.Name = "SelTextureMipLabel";
            this.SelTextureMipLabel.Size = new System.Drawing.Size(14, 15);
            this.SelTextureMipLabel.TabIndex = 35;
            this.SelTextureMipLabel.Text = "0";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 400);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 15);
            this.label3.TabIndex = 34;
            this.label3.Text = "Mip:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(207, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 15);
            this.label2.TabIndex = 33;
            this.label2.Text = "Dictionary:";
            // 
            // SelTextureDictionaryTextBox
            // 
            this.SelTextureDictionaryTextBox.Location = new System.Drawing.Point(270, 6);
            this.SelTextureDictionaryTextBox.Name = "SelTextureDictionaryTextBox";
            this.SelTextureDictionaryTextBox.Size = new System.Drawing.Size(130, 20);
            this.SelTextureDictionaryTextBox.TabIndex = 32;
            // 
            // SelTextureNameTextBox
            // 
            this.SelTextureNameTextBox.Location = new System.Drawing.Point(6, 6);
            this.SelTextureNameTextBox.Name = "SelTextureNameTextBox";
            this.SelTextureNameTextBox.Size = new System.Drawing.Size(192, 20);
            this.SelTextureNameTextBox.TabIndex = 31;
            // 
            // SelDrawableTexturePictureBox
            // 
            this.SelDrawableTexturePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableTexturePictureBox.BackColor = System.Drawing.Color.DarkGray;
            this.SelDrawableTexturePictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SelDrawableTexturePictureBox.Location = new System.Drawing.Point(0, 36);
            this.SelDrawableTexturePictureBox.Name = "SelDrawableTexturePictureBox";
            this.SelDrawableTexturePictureBox.Size = new System.Drawing.Size(406, 351);
            this.SelDrawableTexturePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.SelDrawableTexturePictureBox.TabIndex = 29;
            this.SelDrawableTexturePictureBox.TabStop = false;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.SelDrawableTexturePropertyGrid);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(406, 425);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "Info";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // SelDrawableTexturePropertyGrid
            // 
            this.SelDrawableTexturePropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableTexturePropertyGrid.HelpVisible = false;
            this.SelDrawableTexturePropertyGrid.LineColor = System.Drawing.SystemColors.ControlDark;
            this.SelDrawableTexturePropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.SelDrawableTexturePropertyGrid.Name = "SelDrawableTexturePropertyGrid";
            this.SelDrawableTexturePropertyGrid.Size = new System.Drawing.Size(406, 425);
            this.SelDrawableTexturePropertyGrid.TabIndex = 28;
            this.SelDrawableTexturePropertyGrid.ToolbarVisible = false;
            // 
            // SelectionHierarchyTabPage
            // 
            this.SelectionHierarchyTabPage.Controls.Add(this.splitContainer3);
            this.SelectionHierarchyTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionHierarchyTabPage.Name = "SelectionHierarchyTabPage";
            this.SelectionHierarchyTabPage.Size = new System.Drawing.Size(727, 454);
            this.SelectionHierarchyTabPage.TabIndex = 6;
            this.SelectionHierarchyTabPage.Text = "Hierarchy";
            this.SelectionHierarchyTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.HierarchyTreeView);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.HierarchyPropertyGrid);
            this.splitContainer3.Size = new System.Drawing.Size(727, 454);
            this.splitContainer3.SplitterDistance = 291;
            this.splitContainer3.TabIndex = 0;
            // 
            // HierarchyTreeView
            // 
            this.HierarchyTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HierarchyTreeView.FullRowSelect = true;
            this.HierarchyTreeView.HideSelection = false;
            this.HierarchyTreeView.Location = new System.Drawing.Point(3, 3);
            this.HierarchyTreeView.Name = "HierarchyTreeView";
            this.HierarchyTreeView.Size = new System.Drawing.Size(285, 448);
            this.HierarchyTreeView.TabIndex = 0;
            this.HierarchyTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.HierarchyTreeView_AfterSelect);
            // 
            // HierarchyPropertyGrid
            // 
            this.HierarchyPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HierarchyPropertyGrid.HelpVisible = false;
            this.HierarchyPropertyGrid.Location = new System.Drawing.Point(3, 3);
            this.HierarchyPropertyGrid.Name = "HierarchyPropertyGrid";
            this.HierarchyPropertyGrid.Size = new System.Drawing.Size(426, 448);
            this.HierarchyPropertyGrid.TabIndex = 26;
            this.HierarchyPropertyGrid.ToolbarVisible = false;
            // 
            // MouseSelectCheckBox
            // 
            this.MouseSelectCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MouseSelectCheckBox.AutoSize = true;
            this.MouseSelectCheckBox.Location = new System.Drawing.Point(580, 14);
            this.MouseSelectCheckBox.Name = "MouseSelectCheckBox";
            this.MouseSelectCheckBox.Size = new System.Drawing.Size(161, 19);
            this.MouseSelectCheckBox.TabIndex = 26;
            this.MouseSelectCheckBox.Text = "Mouse select (right click)";
            this.MouseSelectCheckBox.UseVisualStyleBackColor = true;
            this.MouseSelectCheckBox.CheckedChanged += new System.EventHandler(this.MouseSelectCheckBox_CheckedChanged);
            // 
            // SelectionNameTextBox
            // 
            this.SelectionNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectionNameTextBox.Location = new System.Drawing.Point(56, 12);
            this.SelectionNameTextBox.Name = "SelectionNameTextBox";
            this.SelectionNameTextBox.Size = new System.Drawing.Size(317, 20);
            this.SelectionNameTextBox.TabIndex = 29;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 15);
            this.label1.TabIndex = 30;
            this.label1.Text = "Name:";
            // 
            // label25
            // 
            this.label25.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(408, 15);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(42, 15);
            this.label25.TabIndex = 32;
            this.label25.Text = "Mode:";
            // 
            // SelectionModeComboBox
            // 
            this.SelectionModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectionModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SelectionModeComboBox.FormattingEnabled = true;
            this.SelectionModeComboBox.Items.AddRange(new object[] {
            "Entity",
            "Entity Extension",
            "Archetype Extension",
            "Time Cycle Modifier",
            "Car Generator",
            "Grass",
            "Water Quad",
            "Collision",
            "Nav Mesh",
            "Path",
            "Train Track",
            "Distant Lod Lights",
            "Mlo Instance",
            "Scenario",
            "Audio"});
            this.SelectionModeComboBox.Location = new System.Drawing.Point(453, 12);
            this.SelectionModeComboBox.Name = "SelectionModeComboBox";
            this.SelectionModeComboBox.Size = new System.Drawing.Size(121, 21);
            this.SelectionModeComboBox.TabIndex = 31;
            this.SelectionModeComboBox.SelectedIndexChanged += new System.EventHandler(this.SelectionModeComboBox_SelectedIndexChanged);
            // 
            // WorldInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(757, 540);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.SelectionModeComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SelectionNameTextBox);
            this.Controls.Add(this.SelectionTabControl);
            this.Controls.Add(this.MouseSelectCheckBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WorldInfoForm";
            this.Text = "Info - CodeWalker by dexyfex";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WorldInfoForm_FormClosed);
            this.Load += new System.EventHandler(this.WorldInfoForm_Load);
            this.SelectionTabControl.ResumeLayout(false);
            this.SelectionEntityTabPage.ResumeLayout(false);
            this.SelectionArchetypeTabPage.ResumeLayout(false);
            this.SelectionDrawableTabPage.ResumeLayout(false);
            this.SelectionExtensionTabPage.ResumeLayout(false);
            this.SelectionModelsTabPage.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.SelectionTexturesTabPage.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SelTextureMipTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SelDrawableTexturePictureBox)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.SelectionHierarchyTabPage.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl SelectionTabControl;
        private System.Windows.Forms.TabPage SelectionEntityTabPage;
        private CodeWalker.WinForms.PropertyGridFix SelEntityPropertyGrid;
        private System.Windows.Forms.TabPage SelectionArchetypeTabPage;
        private CodeWalker.WinForms.PropertyGridFix SelArchetypePropertyGrid;
        private System.Windows.Forms.TabPage SelectionDrawableTabPage;
        private System.Windows.Forms.CheckBox MouseSelectCheckBox;
        private System.Windows.Forms.TextBox SelectionNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage SelectionModelsTabPage;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private TreeViewFix SelDrawableModelsTreeView;
        private CodeWalker.WinForms.PropertyGridFix SelDrawableModelPropertyGrid;
        private System.Windows.Forms.TabPage SelectionTexturesTabPage;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private TreeViewFix SelDrawableTexturesTreeView;
        private System.Windows.Forms.PictureBox SelDrawableTexturePictureBox;
        private CodeWalker.WinForms.PropertyGridFix SelDrawableTexturePropertyGrid;
        private CodeWalker.WinForms.PropertyGridFix SelDrawablePropertyGrid;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TrackBar SelTextureMipTrackBar;
        private System.Windows.Forms.Label SelTextureMipLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox SelTextureDictionaryTextBox;
        private System.Windows.Forms.TextBox SelTextureNameTextBox;
        private System.Windows.Forms.Label SelTextureDimensionsLabel;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.ComboBox SelectionModeComboBox;
        private System.Windows.Forms.TabPage SelectionExtensionTabPage;
        private CodeWalker.WinForms.PropertyGridFix SelExtensionPropertyGrid;
        private System.Windows.Forms.TabPage SelectionHierarchyTabPage;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TreeView HierarchyTreeView;
        private PropertyGridFix HierarchyPropertyGrid;
    }
}