namespace CodeWalker.Project.Panels
{
    partial class EditYnvPolyPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYnvPolyPanel));
            this.AreaIDUpDown = new System.Windows.Forms.NumericUpDown();
            this.label92 = new System.Windows.Forms.Label();
            this.PartIDUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.PortalIDUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.FlagsCheckedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.FlagsCheckedListBox2 = new System.Windows.Forms.CheckedListBox();
            this.FlagsCheckedListBox3 = new System.Windows.Forms.CheckedListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.FlagsCheckedListBox4 = new System.Windows.Forms.CheckedListBox();
            this.UnkXUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.UnkYUpDown = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.DeletePolyButton = new System.Windows.Forms.Button();
            this.AddToProjectButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.AreaIDUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PartIDUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortalIDUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UnkXUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UnkYUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // AreaIDUpDown
            // 
            this.AreaIDUpDown.Location = new System.Drawing.Point(59, 12);
            this.AreaIDUpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.AreaIDUpDown.Name = "AreaIDUpDown";
            this.AreaIDUpDown.Size = new System.Drawing.Size(71, 20);
            this.AreaIDUpDown.TabIndex = 35;
            this.AreaIDUpDown.ValueChanged += new System.EventHandler(this.AreaIDUpDown_ValueChanged);
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.Location = new System.Drawing.Point(7, 14);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(46, 13);
            this.label92.TabIndex = 34;
            this.label92.Text = "Area ID:";
            // 
            // PartIDUpDown
            // 
            this.PartIDUpDown.Location = new System.Drawing.Point(193, 12);
            this.PartIDUpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.PartIDUpDown.Name = "PartIDUpDown";
            this.PartIDUpDown.Size = new System.Drawing.Size(71, 20);
            this.PartIDUpDown.TabIndex = 37;
            this.PartIDUpDown.ValueChanged += new System.EventHandler(this.PartIDUpDown_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(144, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 36;
            this.label1.Text = "Part ID:";
            // 
            // PortalIDUpDown
            // 
            this.PortalIDUpDown.Location = new System.Drawing.Point(338, 12);
            this.PortalIDUpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.PortalIDUpDown.Name = "PortalIDUpDown";
            this.PortalIDUpDown.Size = new System.Drawing.Size(71, 20);
            this.PortalIDUpDown.TabIndex = 39;
            this.PortalIDUpDown.ValueChanged += new System.EventHandler(this.PortalIDUpDown_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(281, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 38;
            this.label2.Text = "Portal ID:";
            // 
            // FlagsCheckedListBox1
            // 
            this.FlagsCheckedListBox1.CheckOnClick = true;
            this.FlagsCheckedListBox1.FormattingEnabled = true;
            this.FlagsCheckedListBox1.Items.AddRange(new object[] {
            "0 - AvoidUnk0",
            "1 - AvoidUnk1",
            "2 - IsFootpath",
            "3 - IsUnderground",
            "4 - [Not used]",
            "5 - [Not used]",
            "6 - IsSteepSlope",
            "7 - IsWater"});
            this.FlagsCheckedListBox1.Location = new System.Drawing.Point(10, 68);
            this.FlagsCheckedListBox1.Name = "FlagsCheckedListBox1";
            this.FlagsCheckedListBox1.Size = new System.Drawing.Size(131, 124);
            this.FlagsCheckedListBox1.TabIndex = 40;
            this.FlagsCheckedListBox1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.FlagsCheckedListBox1_ItemCheck);
            // 
            // FlagsCheckedListBox2
            // 
            this.FlagsCheckedListBox2.CheckOnClick = true;
            this.FlagsCheckedListBox2.FormattingEnabled = true;
            this.FlagsCheckedListBox2.Items.AddRange(new object[] {
            "0 - UndergroundUnk0",
            "1 - UndergroundUnk1",
            "2 - UndergroundUnk2",
            "3 - UndergroundUnk3",
            "4 - [Not used]",
            "5 - HasPathNode",
            "6 - IsInterior",
            "7 - InteractionUnk"});
            this.FlagsCheckedListBox2.Location = new System.Drawing.Point(147, 68);
            this.FlagsCheckedListBox2.Name = "FlagsCheckedListBox2";
            this.FlagsCheckedListBox2.Size = new System.Drawing.Size(131, 124);
            this.FlagsCheckedListBox2.TabIndex = 42;
            this.FlagsCheckedListBox2.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.FlagsCheckedListBox2_ItemCheck);
            // 
            // FlagsCheckedListBox3
            // 
            this.FlagsCheckedListBox3.CheckOnClick = true;
            this.FlagsCheckedListBox3.FormattingEnabled = true;
            this.FlagsCheckedListBox3.Items.AddRange(new object[] {
            "1 - IsFlatGround",
            "2 - IsRoad",
            "3 - IsCellEdge",
            "4 - IsTrainTrack",
            "5 - IsShallowWater",
            "6 - FootpathUnk1",
            "7 - FootpathUnk2",
            "8 - FootpathMall"});
            this.FlagsCheckedListBox3.Location = new System.Drawing.Point(284, 68);
            this.FlagsCheckedListBox3.Name = "FlagsCheckedListBox3";
            this.FlagsCheckedListBox3.Size = new System.Drawing.Size(131, 124);
            this.FlagsCheckedListBox3.TabIndex = 44;
            this.FlagsCheckedListBox3.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.FlagsCheckedListBox3_ItemCheck);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 45;
            this.label5.Text = "Flags:";
            // 
            // FlagsCheckedListBox4
            // 
            this.FlagsCheckedListBox4.CheckOnClick = true;
            this.FlagsCheckedListBox4.FormattingEnabled = true;
            this.FlagsCheckedListBox4.Items.AddRange(new object[] {
            "0 - SlopeSouth",
            "1 - SlopeSouthEast",
            "2 - SlopeEast",
            "3 - SlopeNorthEast",
            "4 - SlopeNorth",
            "5 - SlopeNorthWest",
            "6 - SlopeWest",
            "7 - SlopeSouthWest"});
            this.FlagsCheckedListBox4.Location = new System.Drawing.Point(421, 68);
            this.FlagsCheckedListBox4.Name = "FlagsCheckedListBox4";
            this.FlagsCheckedListBox4.Size = new System.Drawing.Size(131, 124);
            this.FlagsCheckedListBox4.TabIndex = 46;
            this.FlagsCheckedListBox4.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.FlagsCheckedListBox4_ItemCheck);
            // 
            // UnkXUpDown
            // 
            this.UnkXUpDown.Location = new System.Drawing.Point(82, 213);
            this.UnkXUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.UnkXUpDown.Name = "UnkXUpDown";
            this.UnkXUpDown.Size = new System.Drawing.Size(59, 20);
            this.UnkXUpDown.TabIndex = 48;
            this.UnkXUpDown.ValueChanged += new System.EventHandler(this.UnkXUpDown_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 215);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 47;
            this.label3.Text = "UnknownX:";
            // 
            // UnkYUpDown
            // 
            this.UnkYUpDown.Location = new System.Drawing.Point(219, 213);
            this.UnkYUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.UnkYUpDown.Name = "UnkYUpDown";
            this.UnkYUpDown.Size = new System.Drawing.Size(59, 20);
            this.UnkYUpDown.TabIndex = 50;
            this.UnkYUpDown.ValueChanged += new System.EventHandler(this.UnkYUpDown_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(150, 215);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 49;
            this.label4.Text = "UnknownY:";
            // 
            // DeletePolyButton
            // 
            this.DeletePolyButton.Enabled = false;
            this.DeletePolyButton.Location = new System.Drawing.Point(123, 263);
            this.DeletePolyButton.Name = "DeletePolyButton";
            this.DeletePolyButton.Size = new System.Drawing.Size(90, 23);
            this.DeletePolyButton.TabIndex = 52;
            this.DeletePolyButton.Text = "Delete Polygon";
            this.DeletePolyButton.UseVisualStyleBackColor = true;
            this.DeletePolyButton.Click += new System.EventHandler(this.DeletePolyButton_Click);
            // 
            // AddToProjectButton
            // 
            this.AddToProjectButton.Enabled = false;
            this.AddToProjectButton.Location = new System.Drawing.Point(27, 263);
            this.AddToProjectButton.Name = "AddToProjectButton";
            this.AddToProjectButton.Size = new System.Drawing.Size(90, 23);
            this.AddToProjectButton.TabIndex = 51;
            this.AddToProjectButton.Text = "Add to Project";
            this.AddToProjectButton.UseVisualStyleBackColor = true;
            this.AddToProjectButton.Click += new System.EventHandler(this.AddToProjectButton_Click);
            // 
            // EditYnvPolyPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 404);
            this.Controls.Add(this.DeletePolyButton);
            this.Controls.Add(this.AddToProjectButton);
            this.Controls.Add(this.UnkYUpDown);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.UnkXUpDown);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.FlagsCheckedListBox4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.FlagsCheckedListBox3);
            this.Controls.Add(this.FlagsCheckedListBox2);
            this.Controls.Add(this.FlagsCheckedListBox1);
            this.Controls.Add(this.PortalIDUpDown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PartIDUpDown);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.AreaIDUpDown);
            this.Controls.Add(this.label92);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYnvPolyPanel";
            this.Text = "Edit Ynv Poly";
            ((System.ComponentModel.ISupportInitialize)(this.AreaIDUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PartIDUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortalIDUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UnkXUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UnkYUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown AreaIDUpDown;
        private System.Windows.Forms.Label label92;
        private System.Windows.Forms.NumericUpDown PartIDUpDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown PortalIDUpDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckedListBox FlagsCheckedListBox1;
        private System.Windows.Forms.CheckedListBox FlagsCheckedListBox2;
        private System.Windows.Forms.CheckedListBox FlagsCheckedListBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckedListBox FlagsCheckedListBox4;
        private System.Windows.Forms.NumericUpDown UnkXUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown UnkYUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button DeletePolyButton;
        private System.Windows.Forms.Button AddToProjectButton;
    }
}