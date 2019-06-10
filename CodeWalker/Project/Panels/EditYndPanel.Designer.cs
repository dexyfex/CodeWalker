namespace CodeWalker.Project.Panels
{
    partial class EditYndPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYndPanel));
            this.label88 = new System.Windows.Forms.Label();
            this.YndAreaIDYUpDown = new System.Windows.Forms.NumericUpDown();
            this.label87 = new System.Windows.Forms.Label();
            this.YndAreaIDXUpDown = new System.Windows.Forms.NumericUpDown();
            this.label48 = new System.Windows.Forms.Label();
            this.YndProjectPathTextBox = new System.Windows.Forms.TextBox();
            this.label46 = new System.Windows.Forms.Label();
            this.YndFilePathTextBox = new System.Windows.Forms.TextBox();
            this.label47 = new System.Windows.Forms.Label();
            this.YndTotalNodesLabel = new System.Windows.Forms.Label();
            this.YndPedNodesUpDown = new System.Windows.Forms.NumericUpDown();
            this.label45 = new System.Windows.Forms.Label();
            this.YndVehicleNodesUpDown = new System.Windows.Forms.NumericUpDown();
            this.label40 = new System.Windows.Forms.Label();
            this.YndAreaIDInfoLabel = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.YndRpfPathTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.YndAreaIDYUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.YndAreaIDXUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.YndPedNodesUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.YndVehicleNodesUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label88
            // 
            this.label88.AutoSize = true;
            this.label88.Location = new System.Drawing.Point(183, 20);
            this.label88.Name = "label88";
            this.label88.Size = new System.Drawing.Size(17, 13);
            this.label88.TabIndex = 36;
            this.label88.Text = "Y:";
            // 
            // YndAreaIDYUpDown
            // 
            this.YndAreaIDYUpDown.Location = new System.Drawing.Point(206, 18);
            this.YndAreaIDYUpDown.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.YndAreaIDYUpDown.Name = "YndAreaIDYUpDown";
            this.YndAreaIDYUpDown.Size = new System.Drawing.Size(48, 20);
            this.YndAreaIDYUpDown.TabIndex = 35;
            this.YndAreaIDYUpDown.ValueChanged += new System.EventHandler(this.YndAreaIDYUpDown_ValueChanged);
            // 
            // label87
            // 
            this.label87.AutoSize = true;
            this.label87.Location = new System.Drawing.Point(101, 20);
            this.label87.Name = "label87";
            this.label87.Size = new System.Drawing.Size(17, 13);
            this.label87.TabIndex = 34;
            this.label87.Text = "X:";
            // 
            // YndAreaIDXUpDown
            // 
            this.YndAreaIDXUpDown.Location = new System.Drawing.Point(124, 18);
            this.YndAreaIDXUpDown.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.YndAreaIDXUpDown.Name = "YndAreaIDXUpDown";
            this.YndAreaIDXUpDown.Size = new System.Drawing.Size(48, 20);
            this.YndAreaIDXUpDown.TabIndex = 33;
            this.YndAreaIDXUpDown.ValueChanged += new System.EventHandler(this.YndAreaIDXUpDown_ValueChanged);
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(23, 216);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(68, 13);
            this.label48.TabIndex = 32;
            this.label48.Text = "Project Path:";
            // 
            // YndProjectPathTextBox
            // 
            this.YndProjectPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.YndProjectPathTextBox.Location = new System.Drawing.Point(97, 213);
            this.YndProjectPathTextBox.Name = "YndProjectPathTextBox";
            this.YndProjectPathTextBox.ReadOnly = true;
            this.YndProjectPathTextBox.Size = new System.Drawing.Size(450, 20);
            this.YndProjectPathTextBox.TabIndex = 31;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(23, 190);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(51, 13);
            this.label46.TabIndex = 30;
            this.label46.Text = "File Path:";
            // 
            // YndFilePathTextBox
            // 
            this.YndFilePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.YndFilePathTextBox.Location = new System.Drawing.Point(97, 187);
            this.YndFilePathTextBox.Name = "YndFilePathTextBox";
            this.YndFilePathTextBox.ReadOnly = true;
            this.YndFilePathTextBox.Size = new System.Drawing.Size(450, 20);
            this.YndFilePathTextBox.TabIndex = 29;
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(23, 164);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(52, 13);
            this.label47.TabIndex = 28;
            this.label47.Text = "Rpf Path:";
            // 
            // YndTotalNodesLabel
            // 
            this.YndTotalNodesLabel.AutoSize = true;
            this.YndTotalNodesLabel.Location = new System.Drawing.Point(23, 59);
            this.YndTotalNodesLabel.Name = "YndTotalNodesLabel";
            this.YndTotalNodesLabel.Size = new System.Drawing.Size(77, 13);
            this.YndTotalNodesLabel.TabIndex = 27;
            this.YndTotalNodesLabel.Text = "Total Nodes: 0";
            // 
            // YndPedNodesUpDown
            // 
            this.YndPedNodesUpDown.Location = new System.Drawing.Point(377, 57);
            this.YndPedNodesUpDown.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.YndPedNodesUpDown.Name = "YndPedNodesUpDown";
            this.YndPedNodesUpDown.Size = new System.Drawing.Size(74, 20);
            this.YndPedNodesUpDown.TabIndex = 26;
            this.YndPedNodesUpDown.ValueChanged += new System.EventHandler(this.YndPedNodesUpDown_ValueChanged);
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(308, 59);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(63, 13);
            this.label45.TabIndex = 25;
            this.label45.Text = "Ped Nodes:";
            // 
            // YndVehicleNodesUpDown
            // 
            this.YndVehicleNodesUpDown.Location = new System.Drawing.Point(206, 57);
            this.YndVehicleNodesUpDown.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.YndVehicleNodesUpDown.Name = "YndVehicleNodesUpDown";
            this.YndVehicleNodesUpDown.Size = new System.Drawing.Size(74, 20);
            this.YndVehicleNodesUpDown.TabIndex = 24;
            this.YndVehicleNodesUpDown.ValueChanged += new System.EventHandler(this.YndVehicleNodesUpDown_ValueChanged);
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(121, 59);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(79, 13);
            this.label40.TabIndex = 23;
            this.label40.Text = "Vehicle Nodes:";
            // 
            // YndAreaIDInfoLabel
            // 
            this.YndAreaIDInfoLabel.AutoSize = true;
            this.YndAreaIDInfoLabel.Location = new System.Drawing.Point(271, 20);
            this.YndAreaIDInfoLabel.Name = "YndAreaIDInfoLabel";
            this.YndAreaIDInfoLabel.Size = new System.Drawing.Size(30, 13);
            this.YndAreaIDInfoLabel.TabIndex = 22;
            this.YndAreaIDInfoLabel.Text = "ID: 0";
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(45, 20);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(46, 13);
            this.label33.TabIndex = 21;
            this.label33.Text = "Area ID:";
            // 
            // YndRpfPathTextBox
            // 
            this.YndRpfPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.YndRpfPathTextBox.Location = new System.Drawing.Point(97, 161);
            this.YndRpfPathTextBox.Name = "YndRpfPathTextBox";
            this.YndRpfPathTextBox.ReadOnly = true;
            this.YndRpfPathTextBox.Size = new System.Drawing.Size(450, 20);
            this.YndRpfPathTextBox.TabIndex = 20;
            // 
            // EditYndPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 499);
            this.Controls.Add(this.label88);
            this.Controls.Add(this.YndAreaIDYUpDown);
            this.Controls.Add(this.label87);
            this.Controls.Add(this.YndAreaIDXUpDown);
            this.Controls.Add(this.label48);
            this.Controls.Add(this.YndProjectPathTextBox);
            this.Controls.Add(this.label46);
            this.Controls.Add(this.YndFilePathTextBox);
            this.Controls.Add(this.label47);
            this.Controls.Add(this.YndTotalNodesLabel);
            this.Controls.Add(this.YndPedNodesUpDown);
            this.Controls.Add(this.label45);
            this.Controls.Add(this.YndVehicleNodesUpDown);
            this.Controls.Add(this.label40);
            this.Controls.Add(this.YndAreaIDInfoLabel);
            this.Controls.Add(this.label33);
            this.Controls.Add(this.YndRpfPathTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYndPanel";
            this.Text = "Edit Ynd";
            ((System.ComponentModel.ISupportInitialize)(this.YndAreaIDYUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.YndAreaIDXUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.YndPedNodesUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.YndVehicleNodesUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label88;
        private System.Windows.Forms.NumericUpDown YndAreaIDYUpDown;
        private System.Windows.Forms.Label label87;
        private System.Windows.Forms.NumericUpDown YndAreaIDXUpDown;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.TextBox YndProjectPathTextBox;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.TextBox YndFilePathTextBox;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.Label YndTotalNodesLabel;
        private System.Windows.Forms.NumericUpDown YndPedNodesUpDown;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.NumericUpDown YndVehicleNodesUpDown;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.Label YndAreaIDInfoLabel;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.TextBox YndRpfPathTextBox;
    }
}