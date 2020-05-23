namespace CodeWalker.Project.Panels
{
    partial class EditYtypMloRoomPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYtypMloRoomPanel));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.MinBoundsTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.MaxBoundsTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.label3 = new System.Windows.Forms.Label();
            this.NameTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.FlagsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.FlagsTextBox = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.BlendTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.label4 = new System.Windows.Forms.Label();
            this.TimecycleTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.label5 = new System.Windows.Forms.Label();
            this.Timecycle2TextBox = new CodeWalker.WinForms.TextBoxFix();
            this.label6 = new System.Windows.Forms.Label();
            this.PortalCountTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.label7 = new System.Windows.Forms.Label();
            this.FloorIDTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.label8 = new System.Windows.Forms.Label();
            this.ExteriorVisDepthTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.label9 = new System.Windows.Forms.Label();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.AddEntityButton = new System.Windows.Forms.Button();
            this.CalcPortalsButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(60, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "BB Min:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(57, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "BB Max:";
            // 
            // MinBoundsTextBox
            // 
            this.MinBoundsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MinBoundsTextBox.Location = new System.Drawing.Point(110, 41);
            this.MinBoundsTextBox.Name = "MinBoundsTextBox";
            this.MinBoundsTextBox.Size = new System.Drawing.Size(231, 20);
            this.MinBoundsTextBox.TabIndex = 4;
            this.MinBoundsTextBox.TextChanged += new System.EventHandler(this.MinBoundsTextBox_TextChanged);
            // 
            // MaxBoundsTextBox
            // 
            this.MaxBoundsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MaxBoundsTextBox.Location = new System.Drawing.Point(110, 67);
            this.MaxBoundsTextBox.Name = "MaxBoundsTextBox";
            this.MaxBoundsTextBox.Size = new System.Drawing.Size(231, 20);
            this.MaxBoundsTextBox.TabIndex = 6;
            this.MaxBoundsTextBox.TextChanged += new System.EventHandler(this.MaxBoundsTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(63, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Name: ";
            // 
            // NameTextBox
            // 
            this.NameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NameTextBox.Location = new System.Drawing.Point(110, 15);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(231, 20);
            this.NameTextBox.TabIndex = 2;
            this.NameTextBox.TextChanged += new System.EventHandler(this.NameTextBox_TextChanged);
            // 
            // FlagsCheckedListBox
            // 
            this.FlagsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FlagsCheckedListBox.CheckOnClick = true;
            this.FlagsCheckedListBox.FormattingEnabled = true;
            this.FlagsCheckedListBox.Items.AddRange(new object[] {
            "1 - Unk01",
            "2 - Unk02",
            "4 - Disable exterior shadows",
            "8 - Unk04",
            "16 - Unk05",
            "32 - Unk06",
            "64 - Unk07",
            "128 - Unk08",
            "256 - Disable limbo portals",
            "512 - Unk10"});
            this.FlagsCheckedListBox.Location = new System.Drawing.Point(352, 41);
            this.FlagsCheckedListBox.Name = "FlagsCheckedListBox";
            this.FlagsCheckedListBox.Size = new System.Drawing.Size(201, 154);
            this.FlagsCheckedListBox.TabIndex = 21;
            this.FlagsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.FlagsCheckedListBox_ItemCheck);
            // 
            // FlagsTextBox
            // 
            this.FlagsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FlagsTextBox.Location = new System.Drawing.Point(406, 15);
            this.FlagsTextBox.Name = "FlagsTextBox";
            this.FlagsTextBox.Size = new System.Drawing.Size(147, 20);
            this.FlagsTextBox.TabIndex = 20;
            this.FlagsTextBox.TextChanged += new System.EventHandler(this.FlagsTextBox_TextChanged);
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(365, 18);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(35, 13);
            this.label14.TabIndex = 19;
            this.label14.Text = "Flags:";
            // 
            // BlendTextBox
            // 
            this.BlendTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BlendTextBox.Location = new System.Drawing.Point(110, 93);
            this.BlendTextBox.Name = "BlendTextBox";
            this.BlendTextBox.Size = new System.Drawing.Size(231, 20);
            this.BlendTextBox.TabIndex = 8;
            this.BlendTextBox.TextChanged += new System.EventHandler(this.BlendTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(67, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Blend:";
            // 
            // TimecycleTextBox
            // 
            this.TimecycleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TimecycleTextBox.Location = new System.Drawing.Point(110, 119);
            this.TimecycleTextBox.Name = "TimecycleTextBox";
            this.TimecycleTextBox.Size = new System.Drawing.Size(231, 20);
            this.TimecycleTextBox.TabIndex = 10;
            this.TimecycleTextBox.TextChanged += new System.EventHandler(this.TimecycleTextBox_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(46, 122);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Timecycle:";
            // 
            // Timecycle2TextBox
            // 
            this.Timecycle2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Timecycle2TextBox.Location = new System.Drawing.Point(110, 145);
            this.Timecycle2TextBox.Name = "Timecycle2TextBox";
            this.Timecycle2TextBox.Size = new System.Drawing.Size(231, 20);
            this.Timecycle2TextBox.TabIndex = 12;
            this.Timecycle2TextBox.TextChanged += new System.EventHandler(this.Timecycle2TextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(37, 148);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Timecycle 2:";
            // 
            // PortalCountTextBox
            // 
            this.PortalCountTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PortalCountTextBox.Location = new System.Drawing.Point(110, 171);
            this.PortalCountTextBox.Name = "PortalCountTextBox";
            this.PortalCountTextBox.Size = new System.Drawing.Size(231, 20);
            this.PortalCountTextBox.TabIndex = 14;
            this.PortalCountTextBox.TextChanged += new System.EventHandler(this.PortalCountTextBox_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(36, 174);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(68, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Portal Count:";
            // 
            // FloorIDTextBox
            // 
            this.FloorIDTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FloorIDTextBox.Location = new System.Drawing.Point(110, 197);
            this.FloorIDTextBox.Name = "FloorIDTextBox";
            this.FloorIDTextBox.Size = new System.Drawing.Size(231, 20);
            this.FloorIDTextBox.TabIndex = 16;
            this.FloorIDTextBox.TextChanged += new System.EventHandler(this.FloorIDTextBox_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(57, 200);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Floor ID:";
            // 
            // ExteriorVisDepthTextBox
            // 
            this.ExteriorVisDepthTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExteriorVisDepthTextBox.Location = new System.Drawing.Point(110, 223);
            this.ExteriorVisDepthTextBox.Name = "ExteriorVisDepthTextBox";
            this.ExteriorVisDepthTextBox.Size = new System.Drawing.Size(231, 20);
            this.ExteriorVisDepthTextBox.TabIndex = 18;
            this.ExteriorVisDepthTextBox.TextChanged += new System.EventHandler(this.ExteriorVisDepthTextBox_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(10, 226);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "Exterior Vis Depth:";
            // 
            // DeleteButton
            // 
            this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteButton.Location = new System.Drawing.Point(390, 280);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(95, 23);
            this.DeleteButton.TabIndex = 23;
            this.DeleteButton.Text = "Delete Room";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // AddEntityButton
            // 
            this.AddEntityButton.Location = new System.Drawing.Point(75, 280);
            this.AddEntityButton.Name = "AddEntityButton";
            this.AddEntityButton.Size = new System.Drawing.Size(95, 23);
            this.AddEntityButton.TabIndex = 22;
            this.AddEntityButton.Text = "Add Entity";
            this.AddEntityButton.UseVisualStyleBackColor = true;
            this.AddEntityButton.Click += new System.EventHandler(this.AddEntityButton_Click);
            // 
            // CalcPortalsButton
            // 
            this.CalcPortalsButton.Location = new System.Drawing.Point(205, 280);
            this.CalcPortalsButton.Name = "CalcPortalsButton";
            this.CalcPortalsButton.Size = new System.Drawing.Size(150, 23);
            this.CalcPortalsButton.TabIndex = 24;
            this.CalcPortalsButton.Text = "Calculate Portal Count";
            this.CalcPortalsButton.UseVisualStyleBackColor = true;
            this.CalcPortalsButton.Click += new System.EventHandler(this.CalcPortalCountButton_Click);
            // 
            // EditYtypMloRoomPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 505);
            this.Controls.Add(this.CalcPortalsButton);
            this.Controls.Add(this.AddEntityButton);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.ExteriorVisDepthTextBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.FloorIDTextBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.PortalCountTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.Timecycle2TextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.TimecycleTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.BlendTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.FlagsCheckedListBox);
            this.Controls.Add(this.FlagsTextBox);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.MaxBoundsTextBox);
            this.Controls.Add(this.MinBoundsTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYtypMloRoomPanel";
            this.Text = "Room";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private WinForms.TextBoxFix MinBoundsTextBox;
        private WinForms.TextBoxFix MaxBoundsTextBox;
        private System.Windows.Forms.Label label3;
        private WinForms.TextBoxFix NameTextBox;
        private System.Windows.Forms.CheckedListBox FlagsCheckedListBox;
        private System.Windows.Forms.TextBox FlagsTextBox;
        private System.Windows.Forms.Label label14;
        private WinForms.TextBoxFix BlendTextBox;
        private System.Windows.Forms.Label label4;
        private WinForms.TextBoxFix TimecycleTextBox;
        private System.Windows.Forms.Label label5;
        private WinForms.TextBoxFix Timecycle2TextBox;
        private System.Windows.Forms.Label label6;
        private WinForms.TextBoxFix PortalCountTextBox;
        private System.Windows.Forms.Label label7;
        private WinForms.TextBoxFix FloorIDTextBox;
        private System.Windows.Forms.Label label8;
        private WinForms.TextBoxFix ExteriorVisDepthTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button AddEntityButton;
        private System.Windows.Forms.Button CalcPortalsButton;
    }
}