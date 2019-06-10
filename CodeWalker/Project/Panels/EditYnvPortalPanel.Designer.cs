namespace CodeWalker.Project.Panels
{
    partial class EditYnvPortalPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYnvPortalPanel));
            this.AngleUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.TypeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label49 = new System.Windows.Forms.Label();
            this.DeletePortalButton = new System.Windows.Forms.Button();
            this.AddToProjectButton = new System.Windows.Forms.Button();
            this.GoToButton = new System.Windows.Forms.Button();
            this.PositionFromTextBox = new System.Windows.Forms.TextBox();
            this.label55 = new System.Windows.Forms.Label();
            this.PositionToTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.AreaIDFromUpDown = new System.Windows.Forms.NumericUpDown();
            this.label92 = new System.Windows.Forms.Label();
            this.AreaIDToUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.PolyIDTo1UpDown = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.PolyIDFrom1UpDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.PolyIDTo2UpDown = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.PolyIDFrom2UpDown = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.Unk2UpDown = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.Unk1UpDown = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.AngleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TypeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AreaIDFromUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AreaIDToUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PolyIDTo1UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PolyIDFrom1UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PolyIDTo2UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PolyIDFrom2UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Unk2UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Unk1UpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // AngleUpDown
            // 
            this.AngleUpDown.Location = new System.Drawing.Point(96, 64);
            this.AngleUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.AngleUpDown.Name = "AngleUpDown";
            this.AngleUpDown.Size = new System.Drawing.Size(71, 20);
            this.AngleUpDown.TabIndex = 13;
            this.AngleUpDown.ValueChanged += new System.EventHandler(this.AngleUpDown_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Rotation:";
            // 
            // TypeUpDown
            // 
            this.TypeUpDown.Location = new System.Drawing.Point(256, 64);
            this.TypeUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.TypeUpDown.Name = "TypeUpDown";
            this.TypeUpDown.Size = new System.Drawing.Size(71, 20);
            this.TypeUpDown.TabIndex = 15;
            this.TypeUpDown.ValueChanged += new System.EventHandler(this.TypeUpDown_ValueChanged);
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(216, 66);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(34, 13);
            this.label49.TabIndex = 14;
            this.label49.Text = "Type:";
            // 
            // DeletePortalButton
            // 
            this.DeletePortalButton.Enabled = false;
            this.DeletePortalButton.Location = new System.Drawing.Point(133, 215);
            this.DeletePortalButton.Name = "DeletePortalButton";
            this.DeletePortalButton.Size = new System.Drawing.Size(90, 23);
            this.DeletePortalButton.TabIndex = 17;
            this.DeletePortalButton.Text = "Delete Portal";
            this.DeletePortalButton.UseVisualStyleBackColor = true;
            this.DeletePortalButton.Click += new System.EventHandler(this.DeletePortalButton_Click);
            // 
            // AddToProjectButton
            // 
            this.AddToProjectButton.Enabled = false;
            this.AddToProjectButton.Location = new System.Drawing.Point(37, 215);
            this.AddToProjectButton.Name = "AddToProjectButton";
            this.AddToProjectButton.Size = new System.Drawing.Size(90, 23);
            this.AddToProjectButton.TabIndex = 16;
            this.AddToProjectButton.Text = "Add to Project";
            this.AddToProjectButton.UseVisualStyleBackColor = true;
            this.AddToProjectButton.Click += new System.EventHandler(this.AddToProjectButton_Click);
            // 
            // GoToButton
            // 
            this.GoToButton.Location = new System.Drawing.Point(302, 10);
            this.GoToButton.Name = "GoToButton";
            this.GoToButton.Size = new System.Drawing.Size(68, 23);
            this.GoToButton.TabIndex = 11;
            this.GoToButton.Text = "Go to";
            this.GoToButton.UseVisualStyleBackColor = true;
            this.GoToButton.Click += new System.EventHandler(this.GoToButton_Click);
            // 
            // PositionFromTextBox
            // 
            this.PositionFromTextBox.Location = new System.Drawing.Point(96, 12);
            this.PositionFromTextBox.Name = "PositionFromTextBox";
            this.PositionFromTextBox.Size = new System.Drawing.Size(200, 20);
            this.PositionFromTextBox.TabIndex = 10;
            this.PositionFromTextBox.TextChanged += new System.EventHandler(this.PositionFromTextBox_TextChanged);
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(17, 15);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(73, 13);
            this.label55.TabIndex = 9;
            this.label55.Text = "From Position:";
            // 
            // PositionToTextBox
            // 
            this.PositionToTextBox.Location = new System.Drawing.Point(96, 38);
            this.PositionToTextBox.Name = "PositionToTextBox";
            this.PositionToTextBox.Size = new System.Drawing.Size(200, 20);
            this.PositionToTextBox.TabIndex = 19;
            this.PositionToTextBox.TextChanged += new System.EventHandler(this.PositionToTextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "To Position:";
            // 
            // AreaIDFromUpDown
            // 
            this.AreaIDFromUpDown.Location = new System.Drawing.Point(96, 90);
            this.AreaIDFromUpDown.Maximum = new decimal(new int[] {
            16383,
            0,
            0,
            0});
            this.AreaIDFromUpDown.Name = "AreaIDFromUpDown";
            this.AreaIDFromUpDown.Size = new System.Drawing.Size(71, 20);
            this.AreaIDFromUpDown.TabIndex = 21;
            this.AreaIDFromUpDown.ValueChanged += new System.EventHandler(this.AreaIDFromUpDown_ValueChanged);
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.Location = new System.Drawing.Point(18, 92);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(72, 13);
            this.label92.TabIndex = 20;
            this.label92.Text = "From Area ID:";
            // 
            // AreaIDToUpDown
            // 
            this.AreaIDToUpDown.Location = new System.Drawing.Point(256, 90);
            this.AreaIDToUpDown.Maximum = new decimal(new int[] {
            16383,
            0,
            0,
            0});
            this.AreaIDToUpDown.Name = "AreaIDToUpDown";
            this.AreaIDToUpDown.Size = new System.Drawing.Size(71, 20);
            this.AreaIDToUpDown.TabIndex = 23;
            this.AreaIDToUpDown.ValueChanged += new System.EventHandler(this.AreaIDToUpDown_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(188, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "To Area ID:";
            // 
            // PolyIDTo1UpDown
            // 
            this.PolyIDTo1UpDown.Location = new System.Drawing.Point(256, 116);
            this.PolyIDTo1UpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.PolyIDTo1UpDown.Name = "PolyIDTo1UpDown";
            this.PolyIDTo1UpDown.Size = new System.Drawing.Size(71, 20);
            this.PolyIDTo1UpDown.TabIndex = 27;
            this.PolyIDTo1UpDown.ValueChanged += new System.EventHandler(this.PolyIDTo1UpDown_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(181, 118);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "To Poly ID 1:";
            // 
            // PolyIDFrom1UpDown
            // 
            this.PolyIDFrom1UpDown.Location = new System.Drawing.Point(96, 116);
            this.PolyIDFrom1UpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.PolyIDFrom1UpDown.Name = "PolyIDFrom1UpDown";
            this.PolyIDFrom1UpDown.Size = new System.Drawing.Size(71, 20);
            this.PolyIDFrom1UpDown.TabIndex = 25;
            this.PolyIDFrom1UpDown.ValueChanged += new System.EventHandler(this.PolyIDFrom1UpDown_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 118);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 13);
            this.label5.TabIndex = 24;
            this.label5.Text = "From Poly ID 1:";
            // 
            // PolyIDTo2UpDown
            // 
            this.PolyIDTo2UpDown.Location = new System.Drawing.Point(256, 142);
            this.PolyIDTo2UpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.PolyIDTo2UpDown.Name = "PolyIDTo2UpDown";
            this.PolyIDTo2UpDown.Size = new System.Drawing.Size(71, 20);
            this.PolyIDTo2UpDown.TabIndex = 31;
            this.PolyIDTo2UpDown.ValueChanged += new System.EventHandler(this.PolyIDTo2UpDown_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(181, 144);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(69, 13);
            this.label6.TabIndex = 30;
            this.label6.Text = "To Poly ID 2:";
            // 
            // PolyIDFrom2UpDown
            // 
            this.PolyIDFrom2UpDown.Location = new System.Drawing.Point(96, 142);
            this.PolyIDFrom2UpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.PolyIDFrom2UpDown.Name = "PolyIDFrom2UpDown";
            this.PolyIDFrom2UpDown.Size = new System.Drawing.Size(71, 20);
            this.PolyIDFrom2UpDown.TabIndex = 29;
            this.PolyIDFrom2UpDown.ValueChanged += new System.EventHandler(this.PolyIDFrom2UpDown_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 144);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 13);
            this.label7.TabIndex = 28;
            this.label7.Text = "From Poly ID 2:";
            // 
            // Unk2UpDown
            // 
            this.Unk2UpDown.Location = new System.Drawing.Point(256, 168);
            this.Unk2UpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.Unk2UpDown.Name = "Unk2UpDown";
            this.Unk2UpDown.Size = new System.Drawing.Size(71, 20);
            this.Unk2UpDown.TabIndex = 35;
            this.Unk2UpDown.ValueChanged += new System.EventHandler(this.Unk2UpDown_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(211, 170);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 13);
            this.label8.TabIndex = 34;
            this.label8.Text = "Unk 2:";
            // 
            // Unk1UpDown
            // 
            this.Unk1UpDown.Location = new System.Drawing.Point(96, 168);
            this.Unk1UpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.Unk1UpDown.Name = "Unk1UpDown";
            this.Unk1UpDown.Size = new System.Drawing.Size(71, 20);
            this.Unk1UpDown.TabIndex = 33;
            this.Unk1UpDown.ValueChanged += new System.EventHandler(this.Unk1UpDown_ValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(51, 170);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(39, 13);
            this.label9.TabIndex = 32;
            this.label9.Text = "Unk 1:";
            // 
            // EditYnvPortalPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 295);
            this.Controls.Add(this.Unk2UpDown);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.Unk1UpDown);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.PolyIDTo2UpDown);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.PolyIDFrom2UpDown);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.PolyIDTo1UpDown);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.PolyIDFrom1UpDown);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.AreaIDToUpDown);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.AreaIDFromUpDown);
            this.Controls.Add(this.label92);
            this.Controls.Add(this.PositionToTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.AngleUpDown);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TypeUpDown);
            this.Controls.Add(this.label49);
            this.Controls.Add(this.DeletePortalButton);
            this.Controls.Add(this.AddToProjectButton);
            this.Controls.Add(this.GoToButton);
            this.Controls.Add(this.PositionFromTextBox);
            this.Controls.Add(this.label55);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYnvPortalPanel";
            this.Text = "Edit Ynv Portal";
            ((System.ComponentModel.ISupportInitialize)(this.AngleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TypeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AreaIDFromUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AreaIDToUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PolyIDTo1UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PolyIDFrom1UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PolyIDTo2UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PolyIDFrom2UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Unk2UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Unk1UpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown AngleUpDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown TypeUpDown;
        private System.Windows.Forms.Label label49;
        private System.Windows.Forms.Button DeletePortalButton;
        private System.Windows.Forms.Button AddToProjectButton;
        private System.Windows.Forms.Button GoToButton;
        private System.Windows.Forms.TextBox PositionFromTextBox;
        private System.Windows.Forms.Label label55;
        private System.Windows.Forms.TextBox PositionToTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown AreaIDFromUpDown;
        private System.Windows.Forms.Label label92;
        private System.Windows.Forms.NumericUpDown AreaIDToUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown PolyIDTo1UpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown PolyIDFrom1UpDown;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown PolyIDTo2UpDown;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown PolyIDFrom2UpDown;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown Unk2UpDown;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown Unk1UpDown;
        private System.Windows.Forms.Label label9;
    }
}