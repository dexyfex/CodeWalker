namespace CodeWalker
{
    partial class ExtractShadersForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractShadersForm));
            this.AbortButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.OutputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFolderTextBox = new System.Windows.Forms.TextBox();
            this.ExtractStatusLabel = new System.Windows.Forms.Label();
            this.ExtractButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.FolderBrowseButton = new System.Windows.Forms.Button();
            this.FolderTextBox = new System.Windows.Forms.TextBox();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.AsmCheckBox = new System.Windows.Forms.CheckBox();
            this.CsoCheckBox = new System.Windows.Forms.CheckBox();
            this.MetaCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // AbortButton
            // 
            this.AbortButton.Location = new System.Drawing.Point(188, 99);
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
            this.label4.Location = new System.Drawing.Point(15, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 71;
            this.label4.Text = "Output folder:";
            // 
            // OutputFolderBrowseButton
            // 
            this.OutputFolderBrowseButton.Location = new System.Drawing.Point(350, 36);
            this.OutputFolderBrowseButton.Name = "OutputFolderBrowseButton";
            this.OutputFolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.OutputFolderBrowseButton.TabIndex = 70;
            this.OutputFolderBrowseButton.Text = "...";
            this.OutputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFolderBrowseButton.Click += new System.EventHandler(this.OutputFolderBrowseButton_Click);
            // 
            // OutputFolderTextBox
            // 
            this.OutputFolderTextBox.Location = new System.Drawing.Point(93, 38);
            this.OutputFolderTextBox.Name = "OutputFolderTextBox";
            this.OutputFolderTextBox.Size = new System.Drawing.Size(251, 20);
            this.OutputFolderTextBox.TabIndex = 69;
            this.OutputFolderTextBox.TextChanged += new System.EventHandler(this.OutputFolderTextBox_TextChanged);
            // 
            // ExtractStatusLabel
            // 
            this.ExtractStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExtractStatusLabel.AutoEllipsis = true;
            this.ExtractStatusLabel.Location = new System.Drawing.Point(15, 136);
            this.ExtractStatusLabel.Name = "ExtractStatusLabel";
            this.ExtractStatusLabel.Size = new System.Drawing.Size(507, 37);
            this.ExtractStatusLabel.TabIndex = 68;
            this.ExtractStatusLabel.Text = "Initialising...";
            // 
            // ExtractButton
            // 
            this.ExtractButton.Location = new System.Drawing.Point(93, 99);
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
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 66;
            this.label1.Text = "GTAV folder:";
            // 
            // FolderBrowseButton
            // 
            this.FolderBrowseButton.Location = new System.Drawing.Point(350, 10);
            this.FolderBrowseButton.Name = "FolderBrowseButton";
            this.FolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.FolderBrowseButton.TabIndex = 65;
            this.FolderBrowseButton.Text = "...";
            this.FolderBrowseButton.UseVisualStyleBackColor = true;
            this.FolderBrowseButton.Click += new System.EventHandler(this.FolderBrowseButton_Click);
            // 
            // FolderTextBox
            // 
            this.FolderTextBox.Location = new System.Drawing.Point(93, 12);
            this.FolderTextBox.Name = "FolderTextBox";
            this.FolderTextBox.ReadOnly = true;
            this.FolderTextBox.Size = new System.Drawing.Size(251, 20);
            this.FolderTextBox.TabIndex = 64;
            this.FolderTextBox.TextChanged += new System.EventHandler(this.FolderTextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 75;
            this.label2.Text = "Output:";
            // 
            // AsmCheckBox
            // 
            this.AsmCheckBox.AutoSize = true;
            this.AsmCheckBox.Checked = true;
            this.AsmCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AsmCheckBox.Location = new System.Drawing.Point(142, 70);
            this.AsmCheckBox.Name = "AsmCheckBox";
            this.AsmCheckBox.Size = new System.Drawing.Size(63, 17);
            this.AsmCheckBox.TabIndex = 74;
            this.AsmCheckBox.Text = "hlsl asm";
            this.AsmCheckBox.UseVisualStyleBackColor = true;
            // 
            // CsoCheckBox
            // 
            this.CsoCheckBox.AutoSize = true;
            this.CsoCheckBox.Location = new System.Drawing.Point(93, 70);
            this.CsoCheckBox.Name = "CsoCheckBox";
            this.CsoCheckBox.Size = new System.Drawing.Size(43, 17);
            this.CsoCheckBox.TabIndex = 73;
            this.CsoCheckBox.Text = "cso";
            this.CsoCheckBox.UseVisualStyleBackColor = true;
            // 
            // MetaCheckBox
            // 
            this.MetaCheckBox.AutoSize = true;
            this.MetaCheckBox.Checked = true;
            this.MetaCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MetaCheckBox.Location = new System.Drawing.Point(211, 70);
            this.MetaCheckBox.Name = "MetaCheckBox";
            this.MetaCheckBox.Size = new System.Drawing.Size(49, 17);
            this.MetaCheckBox.TabIndex = 76;
            this.MetaCheckBox.Text = "meta";
            this.MetaCheckBox.UseVisualStyleBackColor = true;
            // 
            // ExtractShadersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 224);
            this.Controls.Add(this.MetaCheckBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.AsmCheckBox);
            this.Controls.Add(this.CsoCheckBox);
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
            this.Name = "ExtractShadersForm";
            this.Text = "Extract Shaders - CodeWalker by dexyfex";
            this.Load += new System.EventHandler(this.ExtractShadersForm_Load);
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
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox AsmCheckBox;
        private System.Windows.Forms.CheckBox CsoCheckBox;
        private System.Windows.Forms.CheckBox MetaCheckBox;
    }
}