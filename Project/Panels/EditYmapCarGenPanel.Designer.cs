namespace CodeWalker.Project.Panels
{
    partial class EditYmapCarGenPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYmapCarGenPanel));
            this.CarFlagsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.CarDeleteButton = new System.Windows.Forms.Button();
            this.CarAddToProjectButton = new System.Windows.Forms.Button();
            this.label44 = new System.Windows.Forms.Label();
            this.CarLiveryTextBox = new System.Windows.Forms.TextBox();
            this.label43 = new System.Windows.Forms.Label();
            this.CarBodyColorRemap4TextBox = new System.Windows.Forms.TextBox();
            this.label42 = new System.Windows.Forms.Label();
            this.CarBodyColorRemap3TextBox = new System.Windows.Forms.TextBox();
            this.label41 = new System.Windows.Forms.Label();
            this.CarBodyColorRemap2TextBox = new System.Windows.Forms.TextBox();
            this.CarPopGroupTextBox = new System.Windows.Forms.TextBox();
            this.label39 = new System.Windows.Forms.Label();
            this.CarPopGroupHashLabel = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.CarBodyColorRemap1TextBox = new System.Windows.Forms.TextBox();
            this.label37 = new System.Windows.Forms.Label();
            this.CarFlagsTextBox = new System.Windows.Forms.TextBox();
            this.CarPerpendicularLengthTextBox = new System.Windows.Forms.TextBox();
            this.label36 = new System.Windows.Forms.Label();
            this.CarOrientYTextBox = new System.Windows.Forms.TextBox();
            this.label34 = new System.Windows.Forms.Label();
            this.CarOrientXTextBox = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.CarModelTextBox = new System.Windows.Forms.TextBox();
            this.label32 = new System.Windows.Forms.Label();
            this.CarModelHashLabel = new System.Windows.Forms.Label();
            this.CarGoToButton = new System.Windows.Forms.Button();
            this.CarPositionTextBox = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CarFlagsCheckedListBox
            // 
            this.CarFlagsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarFlagsCheckedListBox.CheckOnClick = true;
            this.CarFlagsCheckedListBox.FormattingEnabled = true;
            this.CarFlagsCheckedListBox.Items.AddRange(new object[] {
            "1 - Unk01",
            "2 - Unk02",
            "4 - Unk03",
            "8 - Unk04",
            "16 - Unk05",
            "32 - Unk06",
            "64 - Unk07",
            "128 - Unk08",
            "256 - Unk09",
            "512 - Unk10",
            "1024 - Unk11",
            "2048 - Unk12",
            "4096 - Unk13",
            "8192 - Unk14",
            "16384 - Unk15",
            "32768 - Unk16",
            "65536 - Unk17",
            "131072 - Unk18",
            "262144 - Unk19",
            "524288 - Unk20",
            "1048576 - Unk21",
            "2097152 - Unk22",
            "4194304 - Unk23",
            "8388608 - Unk24",
            "16777216 - Unk25",
            "33554432 - Unk26",
            "67108864 - Unk27",
            "134217728 - Unk28",
            "268435456 - Unk29",
            "536870912 - Unk30",
            "1073741824 - Unk31",
            "2147483648 - Unk32"});
            this.CarFlagsCheckedListBox.Location = new System.Drawing.Point(345, 112);
            this.CarFlagsCheckedListBox.Name = "CarFlagsCheckedListBox";
            this.CarFlagsCheckedListBox.Size = new System.Drawing.Size(201, 289);
            this.CarFlagsCheckedListBox.TabIndex = 103;
            this.CarFlagsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CarFlagsCheckedListBox_ItemCheck);
            // 
            // CarDeleteButton
            // 
            this.CarDeleteButton.Location = new System.Drawing.Point(163, 309);
            this.CarDeleteButton.Name = "CarDeleteButton";
            this.CarDeleteButton.Size = new System.Drawing.Size(95, 23);
            this.CarDeleteButton.TabIndex = 105;
            this.CarDeleteButton.Text = "Delete CarGen";
            this.CarDeleteButton.UseVisualStyleBackColor = true;
            this.CarDeleteButton.Click += new System.EventHandler(this.CarDeleteButton_Click);
            // 
            // CarAddToProjectButton
            // 
            this.CarAddToProjectButton.Location = new System.Drawing.Point(62, 309);
            this.CarAddToProjectButton.Name = "CarAddToProjectButton";
            this.CarAddToProjectButton.Size = new System.Drawing.Size(95, 23);
            this.CarAddToProjectButton.TabIndex = 104;
            this.CarAddToProjectButton.Text = "Add to Project";
            this.CarAddToProjectButton.UseVisualStyleBackColor = true;
            this.CarAddToProjectButton.Click += new System.EventHandler(this.CarAddToProjectButton_Click);
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(4, 271);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(38, 13);
            this.label44.TabIndex = 99;
            this.label44.Text = "Livery:";
            // 
            // CarLiveryTextBox
            // 
            this.CarLiveryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarLiveryTextBox.Location = new System.Drawing.Point(118, 268);
            this.CarLiveryTextBox.Name = "CarLiveryTextBox";
            this.CarLiveryTextBox.Size = new System.Drawing.Size(198, 20);
            this.CarLiveryTextBox.TabIndex = 100;
            this.CarLiveryTextBox.TextChanged += new System.EventHandler(this.CarLiveryTextBox_TextChanged);
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(4, 245);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(98, 13);
            this.label43.TabIndex = 97;
            this.label43.Text = "BodyColorRemap4:";
            // 
            // CarBodyColorRemap4TextBox
            // 
            this.CarBodyColorRemap4TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarBodyColorRemap4TextBox.Location = new System.Drawing.Point(118, 242);
            this.CarBodyColorRemap4TextBox.Name = "CarBodyColorRemap4TextBox";
            this.CarBodyColorRemap4TextBox.Size = new System.Drawing.Size(198, 20);
            this.CarBodyColorRemap4TextBox.TabIndex = 98;
            this.CarBodyColorRemap4TextBox.TextChanged += new System.EventHandler(this.CarBodyColorRemap4TextBox_TextChanged);
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(4, 219);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(98, 13);
            this.label42.TabIndex = 95;
            this.label42.Text = "BodyColorRemap3:";
            // 
            // CarBodyColorRemap3TextBox
            // 
            this.CarBodyColorRemap3TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarBodyColorRemap3TextBox.Location = new System.Drawing.Point(118, 216);
            this.CarBodyColorRemap3TextBox.Name = "CarBodyColorRemap3TextBox";
            this.CarBodyColorRemap3TextBox.Size = new System.Drawing.Size(198, 20);
            this.CarBodyColorRemap3TextBox.TabIndex = 96;
            this.CarBodyColorRemap3TextBox.TextChanged += new System.EventHandler(this.CarBodyColorRemap3TextBox_TextChanged);
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(4, 193);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(98, 13);
            this.label41.TabIndex = 93;
            this.label41.Text = "BodyColorRemap2:";
            // 
            // CarBodyColorRemap2TextBox
            // 
            this.CarBodyColorRemap2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarBodyColorRemap2TextBox.Location = new System.Drawing.Point(118, 190);
            this.CarBodyColorRemap2TextBox.Name = "CarBodyColorRemap2TextBox";
            this.CarBodyColorRemap2TextBox.Size = new System.Drawing.Size(198, 20);
            this.CarBodyColorRemap2TextBox.TabIndex = 94;
            this.CarBodyColorRemap2TextBox.TextChanged += new System.EventHandler(this.CarBodyColorRemap2TextBox_TextChanged);
            // 
            // CarPopGroupTextBox
            // 
            this.CarPopGroupTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarPopGroupTextBox.Location = new System.Drawing.Point(82, 60);
            this.CarPopGroupTextBox.Name = "CarPopGroupTextBox";
            this.CarPopGroupTextBox.Size = new System.Drawing.Size(310, 20);
            this.CarPopGroupTextBox.TabIndex = 80;
            this.CarPopGroupTextBox.TextChanged += new System.EventHandler(this.CarPopGroupTextBox_TextChanged);
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(4, 63);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(58, 13);
            this.label39.TabIndex = 79;
            this.label39.Text = "PopGroup:";
            // 
            // CarPopGroupHashLabel
            // 
            this.CarPopGroupHashLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CarPopGroupHashLabel.AutoSize = true;
            this.CarPopGroupHashLabel.Location = new System.Drawing.Point(398, 63);
            this.CarPopGroupHashLabel.Name = "CarPopGroupHashLabel";
            this.CarPopGroupHashLabel.Size = new System.Drawing.Size(44, 13);
            this.CarPopGroupHashLabel.TabIndex = 81;
            this.CarPopGroupHashLabel.Text = "Hash: 0";
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(4, 167);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(98, 13);
            this.label38.TabIndex = 91;
            this.label38.Text = "BodyColorRemap1:";
            // 
            // CarBodyColorRemap1TextBox
            // 
            this.CarBodyColorRemap1TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarBodyColorRemap1TextBox.Location = new System.Drawing.Point(118, 164);
            this.CarBodyColorRemap1TextBox.Name = "CarBodyColorRemap1TextBox";
            this.CarBodyColorRemap1TextBox.Size = new System.Drawing.Size(198, 20);
            this.CarBodyColorRemap1TextBox.TabIndex = 92;
            this.CarBodyColorRemap1TextBox.TextChanged += new System.EventHandler(this.CarBodyColorRemap1TextBox_TextChanged);
            // 
            // label37
            // 
            this.label37.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(362, 89);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(35, 13);
            this.label37.TabIndex = 101;
            this.label37.Text = "Flags:";
            // 
            // CarFlagsTextBox
            // 
            this.CarFlagsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CarFlagsTextBox.Location = new System.Drawing.Point(403, 86);
            this.CarFlagsTextBox.Name = "CarFlagsTextBox";
            this.CarFlagsTextBox.Size = new System.Drawing.Size(143, 20);
            this.CarFlagsTextBox.TabIndex = 102;
            this.CarFlagsTextBox.TextChanged += new System.EventHandler(this.CarFlagsTextBox_TextChanged);
            // 
            // CarPerpendicularLengthTextBox
            // 
            this.CarPerpendicularLengthTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarPerpendicularLengthTextBox.Location = new System.Drawing.Point(118, 138);
            this.CarPerpendicularLengthTextBox.Name = "CarPerpendicularLengthTextBox";
            this.CarPerpendicularLengthTextBox.Size = new System.Drawing.Size(198, 20);
            this.CarPerpendicularLengthTextBox.TabIndex = 90;
            this.CarPerpendicularLengthTextBox.TextChanged += new System.EventHandler(this.CarPerpendicularLengthTextBox_TextChanged);
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(4, 141);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(108, 13);
            this.label36.TabIndex = 89;
            this.label36.Text = "PerpendicularLength:";
            // 
            // CarOrientYTextBox
            // 
            this.CarOrientYTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarOrientYTextBox.Location = new System.Drawing.Point(82, 112);
            this.CarOrientYTextBox.Name = "CarOrientYTextBox";
            this.CarOrientYTextBox.Size = new System.Drawing.Size(234, 20);
            this.CarOrientYTextBox.TabIndex = 88;
            this.CarOrientYTextBox.TextChanged += new System.EventHandler(this.CarOrientYTextBox_TextChanged);
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(4, 115);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(45, 13);
            this.label34.TabIndex = 87;
            this.label34.Text = "OrientY:";
            // 
            // CarOrientXTextBox
            // 
            this.CarOrientXTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarOrientXTextBox.Location = new System.Drawing.Point(82, 86);
            this.CarOrientXTextBox.Name = "CarOrientXTextBox";
            this.CarOrientXTextBox.Size = new System.Drawing.Size(234, 20);
            this.CarOrientXTextBox.TabIndex = 86;
            this.CarOrientXTextBox.TextChanged += new System.EventHandler(this.CarOrientXTextBox_TextChanged);
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(4, 89);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(45, 13);
            this.label35.TabIndex = 85;
            this.label35.Text = "OrientX:";
            // 
            // CarModelTextBox
            // 
            this.CarModelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarModelTextBox.Location = new System.Drawing.Point(82, 34);
            this.CarModelTextBox.Name = "CarModelTextBox";
            this.CarModelTextBox.Size = new System.Drawing.Size(310, 20);
            this.CarModelTextBox.TabIndex = 77;
            this.CarModelTextBox.TextChanged += new System.EventHandler(this.CarModelTextBox_TextChanged);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(4, 37);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(55, 13);
            this.label32.TabIndex = 76;
            this.label32.Text = "CarModel:";
            // 
            // CarModelHashLabel
            // 
            this.CarModelHashLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CarModelHashLabel.AutoSize = true;
            this.CarModelHashLabel.Location = new System.Drawing.Point(398, 37);
            this.CarModelHashLabel.Name = "CarModelHashLabel";
            this.CarModelHashLabel.Size = new System.Drawing.Size(44, 13);
            this.CarModelHashLabel.TabIndex = 78;
            this.CarModelHashLabel.Text = "Hash: 0";
            // 
            // CarGoToButton
            // 
            this.CarGoToButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CarGoToButton.Location = new System.Drawing.Point(478, 6);
            this.CarGoToButton.Name = "CarGoToButton";
            this.CarGoToButton.Size = new System.Drawing.Size(68, 23);
            this.CarGoToButton.TabIndex = 84;
            this.CarGoToButton.Text = "Go to";
            this.CarGoToButton.UseVisualStyleBackColor = true;
            this.CarGoToButton.Click += new System.EventHandler(this.CarGoToButton_Click);
            // 
            // CarPositionTextBox
            // 
            this.CarPositionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CarPositionTextBox.Location = new System.Drawing.Point(82, 8);
            this.CarPositionTextBox.Name = "CarPositionTextBox";
            this.CarPositionTextBox.Size = new System.Drawing.Size(390, 20);
            this.CarPositionTextBox.TabIndex = 83;
            this.CarPositionTextBox.TextChanged += new System.EventHandler(this.CarPositionTextBox_TextChanged);
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(4, 11);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(47, 13);
            this.label31.TabIndex = 82;
            this.label31.Text = "Position:";
            // 
            // EditYmapCarGenPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 407);
            this.Controls.Add(this.CarFlagsCheckedListBox);
            this.Controls.Add(this.CarDeleteButton);
            this.Controls.Add(this.CarAddToProjectButton);
            this.Controls.Add(this.label44);
            this.Controls.Add(this.CarLiveryTextBox);
            this.Controls.Add(this.label43);
            this.Controls.Add(this.CarBodyColorRemap4TextBox);
            this.Controls.Add(this.label42);
            this.Controls.Add(this.CarBodyColorRemap3TextBox);
            this.Controls.Add(this.label41);
            this.Controls.Add(this.CarBodyColorRemap2TextBox);
            this.Controls.Add(this.CarPopGroupTextBox);
            this.Controls.Add(this.label39);
            this.Controls.Add(this.CarPopGroupHashLabel);
            this.Controls.Add(this.label38);
            this.Controls.Add(this.CarBodyColorRemap1TextBox);
            this.Controls.Add(this.label37);
            this.Controls.Add(this.CarFlagsTextBox);
            this.Controls.Add(this.CarPerpendicularLengthTextBox);
            this.Controls.Add(this.label36);
            this.Controls.Add(this.CarOrientYTextBox);
            this.Controls.Add(this.label34);
            this.Controls.Add(this.CarOrientXTextBox);
            this.Controls.Add(this.label35);
            this.Controls.Add(this.CarModelTextBox);
            this.Controls.Add(this.label32);
            this.Controls.Add(this.CarModelHashLabel);
            this.Controls.Add(this.CarGoToButton);
            this.Controls.Add(this.CarPositionTextBox);
            this.Controls.Add(this.label31);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYmapCarGenPanel";
            this.Text = "Car Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox CarFlagsCheckedListBox;
        private System.Windows.Forms.Button CarDeleteButton;
        private System.Windows.Forms.Button CarAddToProjectButton;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.TextBox CarLiveryTextBox;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.TextBox CarBodyColorRemap4TextBox;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.TextBox CarBodyColorRemap3TextBox;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.TextBox CarBodyColorRemap2TextBox;
        private System.Windows.Forms.TextBox CarPopGroupTextBox;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.Label CarPopGroupHashLabel;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.TextBox CarBodyColorRemap1TextBox;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.TextBox CarFlagsTextBox;
        private System.Windows.Forms.TextBox CarPerpendicularLengthTextBox;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.TextBox CarOrientYTextBox;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.TextBox CarOrientXTextBox;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.TextBox CarModelTextBox;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label CarModelHashLabel;
        private System.Windows.Forms.Button CarGoToButton;
        private System.Windows.Forms.TextBox CarPositionTextBox;
        private System.Windows.Forms.Label label31;
    }
}