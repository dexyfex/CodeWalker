namespace CodeWalker.Project.Panels
{
    partial class EditYnvPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYnvPanel));
            this.YnvAABBSizeTextBox = new System.Windows.Forms.TextBox();
            this.label91 = new System.Windows.Forms.Label();
            this.label89 = new System.Windows.Forms.Label();
            this.YnvAreaIDYUpDown = new System.Windows.Forms.NumericUpDown();
            this.label90 = new System.Windows.Forms.Label();
            this.YnvAreaIDXUpDown = new System.Windows.Forms.NumericUpDown();
            this.YnvAreaIDInfoLabel = new System.Windows.Forms.Label();
            this.label92 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.YnvAreaIDYUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.YnvAreaIDXUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // YnvAABBSizeTextBox
            // 
            this.YnvAABBSizeTextBox.Location = new System.Drawing.Point(110, 66);
            this.YnvAABBSizeTextBox.Name = "YnvAABBSizeTextBox";
            this.YnvAABBSizeTextBox.Size = new System.Drawing.Size(138, 20);
            this.YnvAABBSizeTextBox.TabIndex = 37;
            // 
            // label91
            // 
            this.label91.AutoSize = true;
            this.label91.Location = new System.Drawing.Point(43, 69);
            this.label91.Name = "label91";
            this.label91.Size = new System.Drawing.Size(61, 13);
            this.label91.TabIndex = 38;
            this.label91.Text = "AABB Size:";
            // 
            // label89
            // 
            this.label89.AutoSize = true;
            this.label89.Location = new System.Drawing.Point(169, 32);
            this.label89.Name = "label89";
            this.label89.Size = new System.Drawing.Size(17, 13);
            this.label89.TabIndex = 36;
            this.label89.Text = "Y:";
            // 
            // YnvAreaIDYUpDown
            // 
            this.YnvAreaIDYUpDown.Location = new System.Drawing.Point(192, 30);
            this.YnvAreaIDYUpDown.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.YnvAreaIDYUpDown.Name = "YnvAreaIDYUpDown";
            this.YnvAreaIDYUpDown.Size = new System.Drawing.Size(48, 20);
            this.YnvAreaIDYUpDown.TabIndex = 35;
            // 
            // label90
            // 
            this.label90.AutoSize = true;
            this.label90.Location = new System.Drawing.Point(87, 32);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(17, 13);
            this.label90.TabIndex = 34;
            this.label90.Text = "X:";
            // 
            // YnvAreaIDXUpDown
            // 
            this.YnvAreaIDXUpDown.Location = new System.Drawing.Point(110, 30);
            this.YnvAreaIDXUpDown.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.YnvAreaIDXUpDown.Name = "YnvAreaIDXUpDown";
            this.YnvAreaIDXUpDown.Size = new System.Drawing.Size(48, 20);
            this.YnvAreaIDXUpDown.TabIndex = 33;
            // 
            // YnvAreaIDInfoLabel
            // 
            this.YnvAreaIDInfoLabel.AutoSize = true;
            this.YnvAreaIDInfoLabel.Location = new System.Drawing.Point(257, 32);
            this.YnvAreaIDInfoLabel.Name = "YnvAreaIDInfoLabel";
            this.YnvAreaIDInfoLabel.Size = new System.Drawing.Size(30, 13);
            this.YnvAreaIDInfoLabel.TabIndex = 32;
            this.YnvAreaIDInfoLabel.Text = "ID: 0";
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.Location = new System.Drawing.Point(31, 32);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(46, 13);
            this.label92.TabIndex = 31;
            this.label92.Text = "Area ID:";
            // 
            // EditYnvPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 427);
            this.Controls.Add(this.YnvAABBSizeTextBox);
            this.Controls.Add(this.label91);
            this.Controls.Add(this.label89);
            this.Controls.Add(this.YnvAreaIDYUpDown);
            this.Controls.Add(this.label90);
            this.Controls.Add(this.YnvAreaIDXUpDown);
            this.Controls.Add(this.YnvAreaIDInfoLabel);
            this.Controls.Add(this.label92);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYnvPanel";
            this.Text = "Edit Ynv";
            ((System.ComponentModel.ISupportInitialize)(this.YnvAreaIDYUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.YnvAreaIDXUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox YnvAABBSizeTextBox;
        private System.Windows.Forms.Label label91;
        private System.Windows.Forms.Label label89;
        private System.Windows.Forms.NumericUpDown YnvAreaIDYUpDown;
        private System.Windows.Forms.Label label90;
        private System.Windows.Forms.NumericUpDown YnvAreaIDXUpDown;
        private System.Windows.Forms.Label YnvAreaIDInfoLabel;
        private System.Windows.Forms.Label label92;
    }
}