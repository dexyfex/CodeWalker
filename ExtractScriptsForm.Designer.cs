namespace CodeWalker
{
    partial class ExtractScriptsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractScriptsForm));
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.OutputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFolderTextBox = new System.Windows.Forms.TextBox();
            this.FindKeysButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.DumpBrowseButton = new System.Windows.Forms.Button();
            this.DumpTextBox = new System.Windows.Forms.TextBox();
            this.ExtractStatusLabel = new System.Windows.Forms.Label();
            this.DumpStatusLabel = new System.Windows.Forms.Label();
            this.ExtractScriptsButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.FolderBrowseButton = new System.Windows.Forms.Button();
            this.FolderTextBox = new System.Windows.Forms.TextBox();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(113, 13);
            this.label6.TabIndex = 56;
            this.label6.Text = "Extract all YSC scripts:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 170);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 55;
            this.label5.Text = "Decryption Keys:";
            this.label5.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 54;
            this.label4.Text = "Output folder:";
            // 
            // OutputFolderBrowseButton
            // 
            this.OutputFolderBrowseButton.Location = new System.Drawing.Point(355, 50);
            this.OutputFolderBrowseButton.Name = "OutputFolderBrowseButton";
            this.OutputFolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.OutputFolderBrowseButton.TabIndex = 53;
            this.OutputFolderBrowseButton.Text = "...";
            this.OutputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFolderBrowseButton.Click += new System.EventHandler(this.OutputFolderBrowseButton_Click);
            // 
            // OutputFolderTextBox
            // 
            this.OutputFolderTextBox.Location = new System.Drawing.Point(98, 52);
            this.OutputFolderTextBox.Name = "OutputFolderTextBox";
            this.OutputFolderTextBox.Size = new System.Drawing.Size(251, 20);
            this.OutputFolderTextBox.TabIndex = 52;
            this.OutputFolderTextBox.TextChanged += new System.EventHandler(this.OutputFolderTextBox_TextChanged);
            // 
            // FindKeysButton
            // 
            this.FindKeysButton.Location = new System.Drawing.Point(388, 185);
            this.FindKeysButton.Name = "FindKeysButton";
            this.FindKeysButton.Size = new System.Drawing.Size(75, 23);
            this.FindKeysButton.TabIndex = 51;
            this.FindKeysButton.Text = "Find keys";
            this.FindKeysButton.UseVisualStyleBackColor = true;
            this.FindKeysButton.Visible = false;
            this.FindKeysButton.Click += new System.EventHandler(this.FindKeysButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 190);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 50;
            this.label3.Text = "GTAV dump:";
            this.label3.Visible = false;
            // 
            // DumpBrowseButton
            // 
            this.DumpBrowseButton.Location = new System.Drawing.Point(355, 185);
            this.DumpBrowseButton.Name = "DumpBrowseButton";
            this.DumpBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.DumpBrowseButton.TabIndex = 49;
            this.DumpBrowseButton.Text = "...";
            this.DumpBrowseButton.UseVisualStyleBackColor = true;
            this.DumpBrowseButton.Visible = false;
            this.DumpBrowseButton.Click += new System.EventHandler(this.DumpBrowseButton_Click);
            // 
            // DumpTextBox
            // 
            this.DumpTextBox.Location = new System.Drawing.Point(98, 187);
            this.DumpTextBox.Name = "DumpTextBox";
            this.DumpTextBox.Size = new System.Drawing.Size(251, 20);
            this.DumpTextBox.TabIndex = 48;
            this.DumpTextBox.Text = "gta5_dump.exe";
            this.DumpTextBox.Visible = false;
            this.DumpTextBox.TextChanged += new System.EventHandler(this.DumpTextBox_TextChanged);
            // 
            // ExtractStatusLabel
            // 
            this.ExtractStatusLabel.AutoEllipsis = true;
            this.ExtractStatusLabel.Location = new System.Drawing.Point(45, 80);
            this.ExtractStatusLabel.Name = "ExtractStatusLabel";
            this.ExtractStatusLabel.Size = new System.Drawing.Size(455, 41);
            this.ExtractStatusLabel.TabIndex = 47;
            this.ExtractStatusLabel.Text = "Initialising...";
            // 
            // DumpStatusLabel
            // 
            this.DumpStatusLabel.AutoEllipsis = true;
            this.DumpStatusLabel.Location = new System.Drawing.Point(45, 215);
            this.DumpStatusLabel.Name = "DumpStatusLabel";
            this.DumpStatusLabel.Size = new System.Drawing.Size(455, 37);
            this.DumpStatusLabel.TabIndex = 46;
            this.DumpStatusLabel.Text = "Keys not found!";
            this.DumpStatusLabel.Visible = false;
            // 
            // ExtractScriptsButton
            // 
            this.ExtractScriptsButton.Location = new System.Drawing.Point(388, 50);
            this.ExtractScriptsButton.Name = "ExtractScriptsButton";
            this.ExtractScriptsButton.Size = new System.Drawing.Size(75, 23);
            this.ExtractScriptsButton.TabIndex = 45;
            this.ExtractScriptsButton.Text = "Extract";
            this.ExtractScriptsButton.UseVisualStyleBackColor = true;
            this.ExtractScriptsButton.Click += new System.EventHandler(this.ExtractScriptsButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 44;
            this.label1.Text = "GTAV folder:";
            // 
            // FolderBrowseButton
            // 
            this.FolderBrowseButton.Location = new System.Drawing.Point(355, 24);
            this.FolderBrowseButton.Name = "FolderBrowseButton";
            this.FolderBrowseButton.Size = new System.Drawing.Size(27, 23);
            this.FolderBrowseButton.TabIndex = 43;
            this.FolderBrowseButton.Text = "...";
            this.FolderBrowseButton.UseVisualStyleBackColor = true;
            this.FolderBrowseButton.Click += new System.EventHandler(this.FolderBrowseButton_Click);
            // 
            // FolderTextBox
            // 
            this.FolderTextBox.Location = new System.Drawing.Point(98, 26);
            this.FolderTextBox.Name = "FolderTextBox";
            this.FolderTextBox.ReadOnly = true;
            this.FolderTextBox.Size = new System.Drawing.Size(251, 20);
            this.FolderTextBox.TabIndex = 42;
            // 
            // ExtractScriptsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 143);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.OutputFolderBrowseButton);
            this.Controls.Add(this.OutputFolderTextBox);
            this.Controls.Add(this.FindKeysButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.DumpBrowseButton);
            this.Controls.Add(this.DumpTextBox);
            this.Controls.Add(this.ExtractStatusLabel);
            this.Controls.Add(this.DumpStatusLabel);
            this.Controls.Add(this.ExtractScriptsButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.FolderBrowseButton);
            this.Controls.Add(this.FolderTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExtractScriptsForm";
            this.Text = "Script Extractor - CodeWalker by dexyfex";
            this.Load += new System.EventHandler(this.ExtractForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button OutputFolderBrowseButton;
        private System.Windows.Forms.TextBox OutputFolderTextBox;
        private System.Windows.Forms.Button FindKeysButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button DumpBrowseButton;
        private System.Windows.Forms.TextBox DumpTextBox;
        private System.Windows.Forms.Label ExtractStatusLabel;
        private System.Windows.Forms.Label DumpStatusLabel;
        private System.Windows.Forms.Button ExtractScriptsButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button FolderBrowseButton;
        private System.Windows.Forms.TextBox FolderTextBox;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
    }
}