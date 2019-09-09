namespace CodeWalker.Project.Panels
{
    partial class GenerateLODLightsPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenerateLODLightsPanel));
            this.StatusLabel = new System.Windows.Forms.Label();
            this.GenerateButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.label81 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(73, 218);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(95, 13);
            this.StatusLabel.TabIndex = 56;
            this.StatusLabel.Text = "Ready to generate";
            // 
            // GenerateButton
            // 
            this.GenerateButton.Location = new System.Drawing.Point(76, 167);
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.Size = new System.Drawing.Size(75, 23);
            this.GenerateButton.TabIndex = 55;
            this.GenerateButton.Text = "Generate";
            this.GenerateButton.UseVisualStyleBackColor = true;
            this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(459, 108);
            this.label2.TabIndex = 54;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(76, 130);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(177, 20);
            this.NameTextBox.TabIndex = 57;
            this.NameTextBox.Text = "myproject";
            // 
            // label81
            // 
            this.label81.AutoSize = true;
            this.label81.Location = new System.Drawing.Point(11, 133);
            this.label81.Name = "label81";
            this.label81.Size = new System.Drawing.Size(38, 13);
            this.label81.TabIndex = 58;
            this.label81.Text = "Name:";
            // 
            // GenerateLODLightsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 255);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.label81);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.GenerateButton);
            this.Controls.Add(this.label2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GenerateLODLightsPanel";
            this.Text = "Generate LOD Lights ymaps";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Button GenerateButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label label81;
    }
}