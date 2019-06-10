namespace CodeWalker.Project.Panels
{
    partial class EditYnvPointPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYnvPointPanel));
            this.DeletePointButton = new System.Windows.Forms.Button();
            this.AddToProjectButton = new System.Windows.Forms.Button();
            this.YnvPointGoToButton = new System.Windows.Forms.Button();
            this.YnvPointPositionTextBox = new System.Windows.Forms.TextBox();
            this.label55 = new System.Windows.Forms.Label();
            this.YnvPointTypeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label49 = new System.Windows.Forms.Label();
            this.YnvPointAngleUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.YnvPointTypeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.YnvPointAngleUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // DeletePointButton
            // 
            this.DeletePointButton.Enabled = false;
            this.DeletePointButton.Location = new System.Drawing.Point(120, 119);
            this.DeletePointButton.Name = "DeletePointButton";
            this.DeletePointButton.Size = new System.Drawing.Size(90, 23);
            this.DeletePointButton.TabIndex = 8;
            this.DeletePointButton.Text = "Delete Point";
            this.DeletePointButton.UseVisualStyleBackColor = true;
            this.DeletePointButton.Click += new System.EventHandler(this.DeletePointButton_Click);
            // 
            // AddToProjectButton
            // 
            this.AddToProjectButton.Enabled = false;
            this.AddToProjectButton.Location = new System.Drawing.Point(24, 119);
            this.AddToProjectButton.Name = "AddToProjectButton";
            this.AddToProjectButton.Size = new System.Drawing.Size(90, 23);
            this.AddToProjectButton.TabIndex = 7;
            this.AddToProjectButton.Text = "Add to Project";
            this.AddToProjectButton.UseVisualStyleBackColor = true;
            this.AddToProjectButton.Click += new System.EventHandler(this.AddToProjectButton_Click);
            // 
            // YnvPointGoToButton
            // 
            this.YnvPointGoToButton.Location = new System.Drawing.Point(280, 10);
            this.YnvPointGoToButton.Name = "YnvPointGoToButton";
            this.YnvPointGoToButton.Size = new System.Drawing.Size(68, 23);
            this.YnvPointGoToButton.TabIndex = 2;
            this.YnvPointGoToButton.Text = "Go to";
            this.YnvPointGoToButton.UseVisualStyleBackColor = true;
            this.YnvPointGoToButton.Click += new System.EventHandler(this.YnvPointGoToButton_Click);
            // 
            // YnvPointPositionTextBox
            // 
            this.YnvPointPositionTextBox.Location = new System.Drawing.Point(74, 12);
            this.YnvPointPositionTextBox.Name = "YnvPointPositionTextBox";
            this.YnvPointPositionTextBox.Size = new System.Drawing.Size(200, 20);
            this.YnvPointPositionTextBox.TabIndex = 1;
            this.YnvPointPositionTextBox.TextChanged += new System.EventHandler(this.YnvPointPositionTextBox_TextChanged);
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(21, 15);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(47, 13);
            this.label55.TabIndex = 0;
            this.label55.Text = "Position:";
            // 
            // YnvPointTypeUpDown
            // 
            this.YnvPointTypeUpDown.Location = new System.Drawing.Point(74, 64);
            this.YnvPointTypeUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.YnvPointTypeUpDown.Name = "YnvPointTypeUpDown";
            this.YnvPointTypeUpDown.Size = new System.Drawing.Size(74, 20);
            this.YnvPointTypeUpDown.TabIndex = 6;
            this.YnvPointTypeUpDown.ValueChanged += new System.EventHandler(this.YnvPointTypeUpDown_ValueChanged);
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(34, 66);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(34, 13);
            this.label49.TabIndex = 5;
            this.label49.Text = "Type:";
            // 
            // YnvPointAngleUpDown
            // 
            this.YnvPointAngleUpDown.Location = new System.Drawing.Point(74, 38);
            this.YnvPointAngleUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.YnvPointAngleUpDown.Name = "YnvPointAngleUpDown";
            this.YnvPointAngleUpDown.Size = new System.Drawing.Size(74, 20);
            this.YnvPointAngleUpDown.TabIndex = 4;
            this.YnvPointAngleUpDown.ValueChanged += new System.EventHandler(this.YnvPointAngleUpDown_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Rotation:";
            // 
            // EditYnvPointPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 243);
            this.Controls.Add(this.YnvPointAngleUpDown);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.YnvPointTypeUpDown);
            this.Controls.Add(this.label49);
            this.Controls.Add(this.DeletePointButton);
            this.Controls.Add(this.AddToProjectButton);
            this.Controls.Add(this.YnvPointGoToButton);
            this.Controls.Add(this.YnvPointPositionTextBox);
            this.Controls.Add(this.label55);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYnvPointPanel";
            this.Text = "Edit Ynv Point";
            ((System.ComponentModel.ISupportInitialize)(this.YnvPointTypeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.YnvPointAngleUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button DeletePointButton;
        private System.Windows.Forms.Button AddToProjectButton;
        private System.Windows.Forms.Button YnvPointGoToButton;
        private System.Windows.Forms.TextBox YnvPointPositionTextBox;
        private System.Windows.Forms.Label label55;
        private System.Windows.Forms.NumericUpDown YnvPointTypeUpDown;
        private System.Windows.Forms.Label label49;
        private System.Windows.Forms.NumericUpDown YnvPointAngleUpDown;
        private System.Windows.Forms.Label label1;
    }
}