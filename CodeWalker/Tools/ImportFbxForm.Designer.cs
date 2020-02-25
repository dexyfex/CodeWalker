namespace CodeWalker.Tools
{
    partial class ImportFbxForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportFbxForm));
            this.CancelThisButton = new System.Windows.Forms.Button();
            this.ImportButton = new System.Windows.Forms.Button();
            this.FbxFilesListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OutputTypeCombo = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CancelThisButton
            // 
            this.CancelThisButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelThisButton.Location = new System.Drawing.Point(392, 310);
            this.CancelThisButton.Name = "CancelThisButton";
            this.CancelThisButton.Size = new System.Drawing.Size(75, 23);
            this.CancelThisButton.TabIndex = 4;
            this.CancelThisButton.Text = "Cancel";
            this.CancelThisButton.UseVisualStyleBackColor = true;
            this.CancelThisButton.Click += new System.EventHandler(this.CancelThisButton_Click);
            // 
            // ImportButton
            // 
            this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ImportButton.Location = new System.Drawing.Point(473, 310);
            this.ImportButton.Name = "ImportButton";
            this.ImportButton.Size = new System.Drawing.Size(75, 23);
            this.ImportButton.TabIndex = 3;
            this.ImportButton.Text = "Import";
            this.ImportButton.UseVisualStyleBackColor = true;
            this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // FbxFilesListBox
            // 
            this.FbxFilesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FbxFilesListBox.FormattingEnabled = true;
            this.FbxFilesListBox.Location = new System.Drawing.Point(12, 25);
            this.FbxFilesListBox.Name = "FbxFilesListBox";
            this.FbxFilesListBox.Size = new System.Drawing.Size(246, 121);
            this.FbxFilesListBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "FBX files to import:";
            // 
            // OutputTypeCombo
            // 
            this.OutputTypeCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.OutputTypeCombo.FormattingEnabled = true;
            this.OutputTypeCombo.Items.AddRange(new object[] {
            "YDR"});
            this.OutputTypeCombo.Location = new System.Drawing.Point(427, 25);
            this.OutputTypeCombo.Name = "OutputTypeCombo";
            this.OutputTypeCombo.Size = new System.Drawing.Size(121, 21);
            this.OutputTypeCombo.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(356, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Output type:";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(12, 315);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(81, 13);
            this.StatusLabel.TabIndex = 9;
            this.StatusLabel.Text = "Ready to import";
            // 
            // ImportFbxForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 345);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.OutputTypeCombo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.FbxFilesListBox);
            this.Controls.Add(this.CancelThisButton);
            this.Controls.Add(this.ImportButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ImportFbxForm";
            this.Text = "Import FBX - CodeWalker by dexyfex";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CancelThisButton;
        private System.Windows.Forms.Button ImportButton;
        private System.Windows.Forms.ListBox FbxFilesListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox OutputTypeCombo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label StatusLabel;
    }
}