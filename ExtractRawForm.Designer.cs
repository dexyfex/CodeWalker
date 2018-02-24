namespace CodeWalker
{
    partial class ExtractRawForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractRawForm));
            this.AbortButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.OutputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFolderTextBox = new System.Windows.Forms.TextBox();
            this.ExtractStatusLabel = new System.Windows.Forms.Label();
            this.ExtractButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.FolderBrowseButton = new System.Windows.Forms.Button();
            this.FolderTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.FileMatchTextBox = new System.Windows.Forms.TextBox();
            this.MatchEndsWithRadio = new System.Windows.Forms.RadioButton();
            this.MatchContainsRadio = new System.Windows.Forms.RadioButton();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.CompressCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // AbortButton
            // 
            this.AbortButton.Location = new System.Drawing.Point(186, 99);
            this.AbortButton.Name = "AbortButton";
            this.AbortButton.Size = new System.Drawing.Size(75, 23);
            this.AbortButton.TabIndex = 72;
            this.AbortButton.Text = "Abort";
            this.AbortButton.UseVisualStyleBackColor = true;
            this.AbortButton.Click += new System.EventHandler(this.AbortButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 71;
            this.label4.Text = "Output folder:";
            // 
            // OutputFolderBrowseButton
            // 
            this.OutputFolderBrowseButton.Location = new System.Drawing.Point(348, 36);
            this.OutputFolderBrowseButton.Name = "OutputFolderBrowseButton";
            this.OutputFolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.OutputFolderBrowseButton.TabIndex = 70;
            this.OutputFolderBrowseButton.Text = "...";
            this.OutputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFolderBrowseButton.Click += new System.EventHandler(this.OutputFolderBrowseButton_Click);
            // 
            // OutputFolderTextBox
            // 
            this.OutputFolderTextBox.Location = new System.Drawing.Point(91, 38);
            this.OutputFolderTextBox.Name = "OutputFolderTextBox";
            this.OutputFolderTextBox.Size = new System.Drawing.Size(251, 20);
            this.OutputFolderTextBox.TabIndex = 69;
            this.OutputFolderTextBox.TextChanged += new System.EventHandler(this.OutputFolderTextBox_TextChanged);
            // 
            // ExtractStatusLabel
            // 
            this.ExtractStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExtractStatusLabel.AutoEllipsis = true;
            this.ExtractStatusLabel.Location = new System.Drawing.Point(13, 136);
            this.ExtractStatusLabel.Name = "ExtractStatusLabel";
            this.ExtractStatusLabel.Size = new System.Drawing.Size(501, 37);
            this.ExtractStatusLabel.TabIndex = 68;
            this.ExtractStatusLabel.Text = "Initialising...";
            // 
            // ExtractButton
            // 
            this.ExtractButton.Location = new System.Drawing.Point(91, 99);
            this.ExtractButton.Name = "ExtractButton";
            this.ExtractButton.Size = new System.Drawing.Size(75, 23);
            this.ExtractButton.TabIndex = 67;
            this.ExtractButton.Text = "Extract";
            this.ExtractButton.UseVisualStyleBackColor = true;
            this.ExtractButton.Click += new System.EventHandler(this.ExtractButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 66;
            this.label1.Text = "GTAV folder:";
            // 
            // FolderBrowseButton
            // 
            this.FolderBrowseButton.Location = new System.Drawing.Point(348, 10);
            this.FolderBrowseButton.Name = "FolderBrowseButton";
            this.FolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.FolderBrowseButton.TabIndex = 65;
            this.FolderBrowseButton.Text = "...";
            this.FolderBrowseButton.UseVisualStyleBackColor = true;
            this.FolderBrowseButton.Click += new System.EventHandler(this.FolderBrowseButton_Click);
            // 
            // FolderTextBox
            // 
            this.FolderTextBox.Location = new System.Drawing.Point(91, 12);
            this.FolderTextBox.Name = "FolderTextBox";
            this.FolderTextBox.ReadOnly = true;
            this.FolderTextBox.Size = new System.Drawing.Size(251, 20);
            this.FolderTextBox.TabIndex = 64;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 74;
            this.label2.Text = "File match:";
            // 
            // FileMatchTextBox
            // 
            this.FileMatchTextBox.Location = new System.Drawing.Point(91, 64);
            this.FileMatchTextBox.Name = "FileMatchTextBox";
            this.FileMatchTextBox.Size = new System.Drawing.Size(198, 20);
            this.FileMatchTextBox.TabIndex = 73;
            this.FileMatchTextBox.Text = ".ymt";
            // 
            // MatchEndsWithRadio
            // 
            this.MatchEndsWithRadio.AutoSize = true;
            this.MatchEndsWithRadio.Checked = true;
            this.MatchEndsWithRadio.Location = new System.Drawing.Point(295, 65);
            this.MatchEndsWithRadio.Name = "MatchEndsWithRadio";
            this.MatchEndsWithRadio.Size = new System.Drawing.Size(71, 17);
            this.MatchEndsWithRadio.TabIndex = 75;
            this.MatchEndsWithRadio.TabStop = true;
            this.MatchEndsWithRadio.Text = "Ends with";
            this.MatchEndsWithRadio.UseVisualStyleBackColor = true;
            // 
            // MatchContainsRadio
            // 
            this.MatchContainsRadio.AutoSize = true;
            this.MatchContainsRadio.Location = new System.Drawing.Point(372, 65);
            this.MatchContainsRadio.Name = "MatchContainsRadio";
            this.MatchContainsRadio.Size = new System.Drawing.Size(66, 17);
            this.MatchContainsRadio.TabIndex = 76;
            this.MatchContainsRadio.Text = "Contains";
            this.MatchContainsRadio.UseVisualStyleBackColor = true;
            // 
            // CompressCheckBox
            // 
            this.CompressCheckBox.AutoSize = true;
            this.CompressCheckBox.Location = new System.Drawing.Point(348, 103);
            this.CompressCheckBox.Name = "CompressCheckBox";
            this.CompressCheckBox.Size = new System.Drawing.Size(93, 17);
            this.CompressCheckBox.TabIndex = 77;
            this.CompressCheckBox.Text = "Compress files";
            this.CompressCheckBox.UseVisualStyleBackColor = true;
            // 
            // ExtractRawForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 274);
            this.Controls.Add(this.CompressCheckBox);
            this.Controls.Add(this.MatchContainsRadio);
            this.Controls.Add(this.MatchEndsWithRadio);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FileMatchTextBox);
            this.Controls.Add(this.AbortButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.OutputFolderBrowseButton);
            this.Controls.Add(this.OutputFolderTextBox);
            this.Controls.Add(this.ExtractStatusLabel);
            this.Controls.Add(this.ExtractButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.FolderBrowseButton);
            this.Controls.Add(this.FolderTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExtractRawForm";
            this.Text = "Extract Raw Files - CodeWalker by dexyfex";
            this.Load += new System.EventHandler(this.ExtractRawForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button AbortButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button OutputFolderBrowseButton;
        private System.Windows.Forms.TextBox OutputFolderTextBox;
        private System.Windows.Forms.Label ExtractStatusLabel;
        private System.Windows.Forms.Button ExtractButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button FolderBrowseButton;
        private System.Windows.Forms.TextBox FolderTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox FileMatchTextBox;
        private System.Windows.Forms.RadioButton MatchEndsWithRadio;
        private System.Windows.Forms.RadioButton MatchContainsRadio;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.CheckBox CompressCheckBox;
    }
}