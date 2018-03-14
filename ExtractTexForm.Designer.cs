namespace CodeWalker
{
    partial class ExtractTexForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractTexForm));
            this.label4 = new System.Windows.Forms.Label();
            this.OutputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFolderTextBox = new System.Windows.Forms.TextBox();
            this.ExtractStatusLabel = new System.Windows.Forms.Label();
            this.ExtractButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.FolderBrowseButton = new System.Windows.Forms.Button();
            this.FolderTextBox = new System.Windows.Forms.TextBox();
            this.AbortButton = new System.Windows.Forms.Button();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.YtdChecBox = new System.Windows.Forms.CheckBox();
            this.YdrCheckBox = new System.Windows.Forms.CheckBox();
            this.YddCheckBox = new System.Windows.Forms.CheckBox();
            this.YftCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 62;
            this.label4.Text = "Output folder:";
            // 
            // OutputFolderBrowseButton
            // 
            this.OutputFolderBrowseButton.Location = new System.Drawing.Point(347, 30);
            this.OutputFolderBrowseButton.Name = "OutputFolderBrowseButton";
            this.OutputFolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.OutputFolderBrowseButton.TabIndex = 61;
            this.OutputFolderBrowseButton.Text = "...";
            this.OutputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFolderBrowseButton.Click += new System.EventHandler(this.OutputFolderBrowseButton_Click);
            // 
            // OutputFolderTextBox
            // 
            this.OutputFolderTextBox.Location = new System.Drawing.Point(90, 32);
            this.OutputFolderTextBox.Name = "OutputFolderTextBox";
            this.OutputFolderTextBox.Size = new System.Drawing.Size(251, 20);
            this.OutputFolderTextBox.TabIndex = 60;
            this.OutputFolderTextBox.TextChanged += new System.EventHandler(this.OutputFolderTextBox_TextChanged);
            // 
            // ExtractStatusLabel
            // 
            this.ExtractStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExtractStatusLabel.AutoEllipsis = true;
            this.ExtractStatusLabel.Location = new System.Drawing.Point(12, 130);
            this.ExtractStatusLabel.Name = "ExtractStatusLabel";
            this.ExtractStatusLabel.Size = new System.Drawing.Size(501, 65);
            this.ExtractStatusLabel.TabIndex = 59;
            this.ExtractStatusLabel.Text = "Initialising...";
            // 
            // ExtractButton
            // 
            this.ExtractButton.Location = new System.Drawing.Point(90, 93);
            this.ExtractButton.Name = "ExtractButton";
            this.ExtractButton.Size = new System.Drawing.Size(75, 23);
            this.ExtractButton.TabIndex = 58;
            this.ExtractButton.Text = "Extract";
            this.ExtractButton.UseVisualStyleBackColor = true;
            this.ExtractButton.Click += new System.EventHandler(this.ExtractButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 57;
            this.label1.Text = "GTAV folder:";
            // 
            // FolderBrowseButton
            // 
            this.FolderBrowseButton.Location = new System.Drawing.Point(347, 4);
            this.FolderBrowseButton.Name = "FolderBrowseButton";
            this.FolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.FolderBrowseButton.TabIndex = 56;
            this.FolderBrowseButton.Text = "...";
            this.FolderBrowseButton.UseVisualStyleBackColor = true;
            this.FolderBrowseButton.Click += new System.EventHandler(this.FolderBrowseButton_Click);
            // 
            // FolderTextBox
            // 
            this.FolderTextBox.Location = new System.Drawing.Point(90, 6);
            this.FolderTextBox.Name = "FolderTextBox";
            this.FolderTextBox.ReadOnly = true;
            this.FolderTextBox.Size = new System.Drawing.Size(251, 20);
            this.FolderTextBox.TabIndex = 55;
            // 
            // AbortButton
            // 
            this.AbortButton.Location = new System.Drawing.Point(185, 93);
            this.AbortButton.Name = "AbortButton";
            this.AbortButton.Size = new System.Drawing.Size(75, 23);
            this.AbortButton.TabIndex = 63;
            this.AbortButton.Text = "Abort";
            this.AbortButton.UseVisualStyleBackColor = true;
            this.AbortButton.Click += new System.EventHandler(this.AbortButton_Click);
            // 
            // YtdChecBox
            // 
            this.YtdChecBox.AutoSize = true;
            this.YtdChecBox.Location = new System.Drawing.Point(90, 64);
            this.YtdChecBox.Name = "YtdChecBox";
            this.YtdChecBox.Size = new System.Drawing.Size(40, 17);
            this.YtdChecBox.TabIndex = 64;
            this.YtdChecBox.Text = "ytd";
            this.YtdChecBox.UseVisualStyleBackColor = true;
            // 
            // YdrCheckBox
            // 
            this.YdrCheckBox.AutoSize = true;
            this.YdrCheckBox.Checked = true;
            this.YdrCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.YdrCheckBox.Location = new System.Drawing.Point(136, 64);
            this.YdrCheckBox.Name = "YdrCheckBox";
            this.YdrCheckBox.Size = new System.Drawing.Size(40, 17);
            this.YdrCheckBox.TabIndex = 65;
            this.YdrCheckBox.Text = "ydr";
            this.YdrCheckBox.UseVisualStyleBackColor = true;
            // 
            // YddCheckBox
            // 
            this.YddCheckBox.AutoSize = true;
            this.YddCheckBox.Checked = true;
            this.YddCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.YddCheckBox.Location = new System.Drawing.Point(182, 64);
            this.YddCheckBox.Name = "YddCheckBox";
            this.YddCheckBox.Size = new System.Drawing.Size(43, 17);
            this.YddCheckBox.TabIndex = 66;
            this.YddCheckBox.Text = "ydd";
            this.YddCheckBox.UseVisualStyleBackColor = true;
            // 
            // YftCheckBox
            // 
            this.YftCheckBox.AutoSize = true;
            this.YftCheckBox.Checked = true;
            this.YftCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.YftCheckBox.Location = new System.Drawing.Point(231, 64);
            this.YftCheckBox.Name = "YftCheckBox";
            this.YftCheckBox.Size = new System.Drawing.Size(37, 17);
            this.YftCheckBox.TabIndex = 67;
            this.YftCheckBox.Text = "yft";
            this.YftCheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 68;
            this.label2.Text = "From files:";
            // 
            // ExtractTexForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 236);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.YftCheckBox);
            this.Controls.Add(this.YddCheckBox);
            this.Controls.Add(this.YdrCheckBox);
            this.Controls.Add(this.YtdChecBox);
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
            this.Name = "ExtractTexForm";
            this.Text = "Extract Textures - CodeWalker by dexyfex";
            this.Load += new System.EventHandler(this.ExtractTexForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button OutputFolderBrowseButton;
        private System.Windows.Forms.TextBox OutputFolderTextBox;
        private System.Windows.Forms.Label ExtractStatusLabel;
        private System.Windows.Forms.Button ExtractButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button FolderBrowseButton;
        private System.Windows.Forms.TextBox FolderTextBox;
        private System.Windows.Forms.Button AbortButton;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.CheckBox YtdChecBox;
        private System.Windows.Forms.CheckBox YdrCheckBox;
        private System.Windows.Forms.CheckBox YddCheckBox;
        private System.Windows.Forms.CheckBox YftCheckBox;
        private System.Windows.Forms.Label label2;
    }
}