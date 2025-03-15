namespace CodeWalker.Tools
{
    partial class ConvertAssetsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConvertAssetsForm));
            this.label1 = new System.Windows.Forms.Label();
            this.InputFolderTextBox = new System.Windows.Forms.TextBox();
            this.InputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFolderTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SubfoldersCheckbox = new System.Windows.Forms.CheckBox();
            this.OverwriteCheckbox = new System.Windows.Forms.CheckBox();
            this.ProcessButton = new System.Windows.Forms.Button();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Input folder:";
            // 
            // InputFolderTextBox
            // 
            this.InputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InputFolderTextBox.Location = new System.Drawing.Point(92, 38);
            this.InputFolderTextBox.Name = "InputFolderTextBox";
            this.InputFolderTextBox.Size = new System.Drawing.Size(327, 20);
            this.InputFolderTextBox.TabIndex = 2;
            // 
            // InputFolderBrowseButton
            // 
            this.InputFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.InputFolderBrowseButton.Location = new System.Drawing.Point(434, 37);
            this.InputFolderBrowseButton.Name = "InputFolderBrowseButton";
            this.InputFolderBrowseButton.Size = new System.Drawing.Size(31, 22);
            this.InputFolderBrowseButton.TabIndex = 3;
            this.InputFolderBrowseButton.Text = "...";
            this.InputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.InputFolderBrowseButton.Click += new System.EventHandler(this.InputFolderBrowseButton_Click);
            // 
            // OutputFolderBrowseButton
            // 
            this.OutputFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputFolderBrowseButton.Location = new System.Drawing.Point(434, 63);
            this.OutputFolderBrowseButton.Name = "OutputFolderBrowseButton";
            this.OutputFolderBrowseButton.Size = new System.Drawing.Size(31, 22);
            this.OutputFolderBrowseButton.TabIndex = 6;
            this.OutputFolderBrowseButton.Text = "...";
            this.OutputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFolderBrowseButton.Click += new System.EventHandler(this.OutputFolderBrowseButton_Click);
            // 
            // OutputFolderTextBox
            // 
            this.OutputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputFolderTextBox.Location = new System.Drawing.Point(92, 64);
            this.OutputFolderTextBox.Name = "OutputFolderTextBox";
            this.OutputFolderTextBox.Size = new System.Drawing.Size(327, 20);
            this.OutputFolderTextBox.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Output folder:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(331, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Convert files from GTAV Legacy formats to GTAV Enhanced formats.";
            // 
            // SubfoldersCheckbox
            // 
            this.SubfoldersCheckbox.AutoSize = true;
            this.SubfoldersCheckbox.Checked = true;
            this.SubfoldersCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SubfoldersCheckbox.Location = new System.Drawing.Point(92, 91);
            this.SubfoldersCheckbox.Name = "SubfoldersCheckbox";
            this.SubfoldersCheckbox.Size = new System.Drawing.Size(112, 17);
            this.SubfoldersCheckbox.TabIndex = 7;
            this.SubfoldersCheckbox.Text = "Include subfolders";
            this.SubfoldersCheckbox.UseVisualStyleBackColor = true;
            // 
            // OverwriteCheckbox
            // 
            this.OverwriteCheckbox.AutoSize = true;
            this.OverwriteCheckbox.Checked = true;
            this.OverwriteCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OverwriteCheckbox.Location = new System.Drawing.Point(246, 91);
            this.OverwriteCheckbox.Name = "OverwriteCheckbox";
            this.OverwriteCheckbox.Size = new System.Drawing.Size(130, 17);
            this.OverwriteCheckbox.TabIndex = 8;
            this.OverwriteCheckbox.Text = "Overwrite existing files";
            this.OverwriteCheckbox.UseVisualStyleBackColor = true;
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(92, 126);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(75, 23);
            this.ProcessButton.TabIndex = 9;
            this.ProcessButton.Text = "Process";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // LogTextBox
            // 
            this.LogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogTextBox.Location = new System.Drawing.Point(92, 166);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(327, 108);
            this.LogTextBox.TabIndex = 10;
            // 
            // ConvertAssetsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 293);
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.ProcessButton);
            this.Controls.Add(this.OverwriteCheckbox);
            this.Controls.Add(this.SubfoldersCheckbox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.OutputFolderBrowseButton);
            this.Controls.Add(this.OutputFolderTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.InputFolderBrowseButton);
            this.Controls.Add(this.InputFolderTextBox);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConvertAssetsForm";
            this.Text = "Convert Assets - CodeWalker by dexyfex";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox InputFolderTextBox;
        private System.Windows.Forms.Button InputFolderBrowseButton;
        private System.Windows.Forms.Button OutputFolderBrowseButton;
        private System.Windows.Forms.TextBox OutputFolderTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox SubfoldersCheckbox;
        private System.Windows.Forms.CheckBox OverwriteCheckbox;
        private System.Windows.Forms.Button ProcessButton;
        private System.Windows.Forms.TextBox LogTextBox;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
    }
}