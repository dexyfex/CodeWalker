namespace CodeWalker
{
    partial class BinarySearchForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BinarySearchForm));
            this.TextRadio = new System.Windows.Forms.RadioButton();
            this.HexRadio = new System.Windows.Forms.RadioButton();
            this.SearchAbortButton = new System.Windows.Forms.Button();
            this.SearchResultsTextBox = new System.Windows.Forms.TextBox();
            this.BinSearchTextBox = new System.Windows.Forms.TextBox();
            this.BinSearchFolderBrowseButton = new System.Windows.Forms.Button();
            this.BinSearchButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.BinSearchFolderTextBox = new System.Windows.Forms.TextBox();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // TextRadio
            // 
            this.TextRadio.AutoSize = true;
            this.TextRadio.Location = new System.Drawing.Point(169, 39);
            this.TextRadio.Name = "TextRadio";
            this.TextRadio.Size = new System.Drawing.Size(46, 17);
            this.TextRadio.TabIndex = 45;
            this.TextRadio.Text = "Text";
            this.TextRadio.UseVisualStyleBackColor = true;
            // 
            // HexRadio
            // 
            this.HexRadio.AutoSize = true;
            this.HexRadio.Checked = true;
            this.HexRadio.Location = new System.Drawing.Point(119, 39);
            this.HexRadio.Name = "HexRadio";
            this.HexRadio.Size = new System.Drawing.Size(44, 17);
            this.HexRadio.TabIndex = 44;
            this.HexRadio.TabStop = true;
            this.HexRadio.Text = "Hex";
            this.HexRadio.UseVisualStyleBackColor = true;
            // 
            // SearchAbortButton
            // 
            this.SearchAbortButton.Location = new System.Drawing.Point(427, 36);
            this.SearchAbortButton.Name = "SearchAbortButton";
            this.SearchAbortButton.Size = new System.Drawing.Size(75, 23);
            this.SearchAbortButton.TabIndex = 43;
            this.SearchAbortButton.Text = "Abort";
            this.SearchAbortButton.UseVisualStyleBackColor = true;
            this.SearchAbortButton.Click += new System.EventHandler(this.SearchAbortButton_Click);
            // 
            // SearchResultsTextBox
            // 
            this.SearchResultsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchResultsTextBox.Location = new System.Drawing.Point(14, 64);
            this.SearchResultsTextBox.Multiline = true;
            this.SearchResultsTextBox.Name = "SearchResultsTextBox";
            this.SearchResultsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.SearchResultsTextBox.Size = new System.Drawing.Size(487, 247);
            this.SearchResultsTextBox.TabIndex = 42;
            this.SearchResultsTextBox.WordWrap = false;
            // 
            // BinSearchTextBox
            // 
            this.BinSearchTextBox.Location = new System.Drawing.Point(221, 38);
            this.BinSearchTextBox.Name = "BinSearchTextBox";
            this.BinSearchTextBox.Size = new System.Drawing.Size(119, 20);
            this.BinSearchTextBox.TabIndex = 41;
            this.BinSearchTextBox.Text = "4a03746e";
            // 
            // BinSearchFolderBrowseButton
            // 
            this.BinSearchFolderBrowseButton.Location = new System.Drawing.Point(346, 10);
            this.BinSearchFolderBrowseButton.Name = "BinSearchFolderBrowseButton";
            this.BinSearchFolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.BinSearchFolderBrowseButton.TabIndex = 40;
            this.BinSearchFolderBrowseButton.Text = "...";
            this.BinSearchFolderBrowseButton.UseVisualStyleBackColor = true;
            this.BinSearchFolderBrowseButton.Click += new System.EventHandler(this.BinSearchFolderBrowseButton_Click);
            // 
            // BinSearchButton
            // 
            this.BinSearchButton.Location = new System.Drawing.Point(346, 36);
            this.BinSearchButton.Name = "BinSearchButton";
            this.BinSearchButton.Size = new System.Drawing.Size(75, 23);
            this.BinSearchButton.TabIndex = 39;
            this.BinSearchButton.Text = "Search";
            this.BinSearchButton.UseVisualStyleBackColor = true;
            this.BinSearchButton.Click += new System.EventHandler(this.BinSearchButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 38;
            this.label2.Text = "Binary Search:";
            // 
            // BinSearchFolderTextBox
            // 
            this.BinSearchFolderTextBox.Location = new System.Drawing.Point(89, 12);
            this.BinSearchFolderTextBox.Name = "BinSearchFolderTextBox";
            this.BinSearchFolderTextBox.Size = new System.Drawing.Size(251, 20);
            this.BinSearchFolderTextBox.TabIndex = 37;
            this.BinSearchFolderTextBox.Text = "Compiled944";
            // 
            // BinarySearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 323);
            this.Controls.Add(this.TextRadio);
            this.Controls.Add(this.HexRadio);
            this.Controls.Add(this.SearchAbortButton);
            this.Controls.Add(this.SearchResultsTextBox);
            this.Controls.Add(this.BinSearchTextBox);
            this.Controls.Add(this.BinSearchFolderBrowseButton);
            this.Controls.Add(this.BinSearchButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BinSearchFolderTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BinarySearchForm";
            this.Text = "Binary Search - CodeWalker by dexyfex";
            this.Load += new System.EventHandler(this.SearchForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton TextRadio;
        private System.Windows.Forms.RadioButton HexRadio;
        private System.Windows.Forms.Button SearchAbortButton;
        private System.Windows.Forms.TextBox SearchResultsTextBox;
        private System.Windows.Forms.TextBox BinSearchTextBox;
        private System.Windows.Forms.Button BinSearchFolderBrowseButton;
        private System.Windows.Forms.Button BinSearchButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox BinSearchFolderTextBox;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
    }
}