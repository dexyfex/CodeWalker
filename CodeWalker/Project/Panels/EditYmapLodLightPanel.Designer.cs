
namespace CodeWalker.Project.Panels
{
    partial class EditYmapLodLightPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYmapLodLightPanel));
            this.GoToButton = new System.Windows.Forms.Button();
            this.PositionTextBox = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.AddToProjectButton = new System.Windows.Forms.Button();
            this.NormalizeDirectionButton = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.DirectionTextBox = new System.Windows.Forms.TextBox();
            this.ColourLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.IntensityUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.TypeComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.FalloffTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.FalloffExponentTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.HashTextBox = new System.Windows.Forms.TextBox();
            this.InnerAngleUpDown = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.OuterAngleUpDown = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.CoronaIntensityUpDown = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.TimeFlagsAMCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.TimeFlagsPMCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.label10 = new System.Windows.Forms.Label();
            this.TimeStateFlagsTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.StateFlags1CheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.label12 = new System.Windows.Forms.Label();
            this.StateFlags2CheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.ColourRUpDown = new System.Windows.Forms.NumericUpDown();
            this.ColourGUpDown = new System.Windows.Forms.NumericUpDown();
            this.ColourBUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.IntensityUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InnerAngleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OuterAngleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CoronaIntensityUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColourRUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColourGUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColourBUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // GoToButton
            // 
            this.GoToButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GoToButton.Location = new System.Drawing.Point(478, 6);
            this.GoToButton.Name = "GoToButton";
            this.GoToButton.Size = new System.Drawing.Size(68, 23);
            this.GoToButton.TabIndex = 87;
            this.GoToButton.Text = "Go to";
            this.GoToButton.UseVisualStyleBackColor = true;
            this.GoToButton.Click += new System.EventHandler(this.GoToButton_Click);
            // 
            // PositionTextBox
            // 
            this.PositionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PositionTextBox.Location = new System.Drawing.Point(82, 8);
            this.PositionTextBox.Name = "PositionTextBox";
            this.PositionTextBox.Size = new System.Drawing.Size(390, 20);
            this.PositionTextBox.TabIndex = 86;
            this.PositionTextBox.TextChanged += new System.EventHandler(this.PositionTextBox_TextChanged);
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(4, 11);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(47, 13);
            this.label31.TabIndex = 85;
            this.label31.Text = "Position:";
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(129, 314);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(95, 23);
            this.DeleteButton.TabIndex = 107;
            this.DeleteButton.Text = "Delete LodLight";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // AddToProjectButton
            // 
            this.AddToProjectButton.Location = new System.Drawing.Point(28, 314);
            this.AddToProjectButton.Name = "AddToProjectButton";
            this.AddToProjectButton.Size = new System.Drawing.Size(95, 23);
            this.AddToProjectButton.TabIndex = 106;
            this.AddToProjectButton.Text = "Add to Project";
            this.AddToProjectButton.UseVisualStyleBackColor = true;
            this.AddToProjectButton.Click += new System.EventHandler(this.AddToProjectButton_Click);
            // 
            // NormalizeDirectionButton
            // 
            this.NormalizeDirectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NormalizeDirectionButton.Location = new System.Drawing.Point(478, 32);
            this.NormalizeDirectionButton.Name = "NormalizeDirectionButton";
            this.NormalizeDirectionButton.Size = new System.Drawing.Size(68, 23);
            this.NormalizeDirectionButton.TabIndex = 110;
            this.NormalizeDirectionButton.Text = "Normalize";
            this.NormalizeDirectionButton.UseVisualStyleBackColor = true;
            this.NormalizeDirectionButton.Click += new System.EventHandler(this.NormalizeDirectionButton_Click);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(4, 37);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(52, 13);
            this.label17.TabIndex = 108;
            this.label17.Text = "Direction:";
            // 
            // DirectionTextBox
            // 
            this.DirectionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DirectionTextBox.Location = new System.Drawing.Point(82, 34);
            this.DirectionTextBox.Name = "DirectionTextBox";
            this.DirectionTextBox.Size = new System.Drawing.Size(390, 20);
            this.DirectionTextBox.TabIndex = 109;
            this.DirectionTextBox.TextChanged += new System.EventHandler(this.DirectionTextBox_TextChanged);
            // 
            // ColourLabel
            // 
            this.ColourLabel.BackColor = System.Drawing.Color.White;
            this.ColourLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ColourLabel.Location = new System.Drawing.Point(82, 113);
            this.ColourLabel.Name = "ColourLabel";
            this.ColourLabel.Size = new System.Drawing.Size(30, 21);
            this.ColourLabel.TabIndex = 111;
            this.ColourLabel.Click += new System.EventHandler(this.ColourLabel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 112;
            this.label1.Text = "Type:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 114;
            this.label2.Text = "Colour (RGB):";
            // 
            // IntensityUpDown
            // 
            this.IntensityUpDown.Location = new System.Drawing.Point(82, 87);
            this.IntensityUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.IntensityUpDown.Name = "IntensityUpDown";
            this.IntensityUpDown.Size = new System.Drawing.Size(154, 20);
            this.IntensityUpDown.TabIndex = 116;
            this.IntensityUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.IntensityUpDown.ValueChanged += new System.EventHandler(this.IntensityUpDown_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 115;
            this.label3.Text = "Intensity:";
            // 
            // TypeComboBox
            // 
            this.TypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TypeComboBox.FormattingEnabled = true;
            this.TypeComboBox.Items.AddRange(new object[] {
            "Point",
            "Spot",
            "Capsule"});
            this.TypeComboBox.Location = new System.Drawing.Point(82, 60);
            this.TypeComboBox.Name = "TypeComboBox";
            this.TypeComboBox.Size = new System.Drawing.Size(154, 21);
            this.TypeComboBox.TabIndex = 118;
            this.TypeComboBox.SelectedIndexChanged += new System.EventHandler(this.TypeComboBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 144);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 119;
            this.label4.Text = "Falloff:";
            // 
            // FalloffTextBox
            // 
            this.FalloffTextBox.Location = new System.Drawing.Point(82, 141);
            this.FalloffTextBox.Name = "FalloffTextBox";
            this.FalloffTextBox.Size = new System.Drawing.Size(154, 20);
            this.FalloffTextBox.TabIndex = 120;
            this.FalloffTextBox.TextChanged += new System.EventHandler(this.FalloffTextBox_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 170);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 121;
            this.label5.Text = "Falloff exp:";
            // 
            // FalloffExponentTextBox
            // 
            this.FalloffExponentTextBox.Location = new System.Drawing.Point(82, 167);
            this.FalloffExponentTextBox.Name = "FalloffExponentTextBox";
            this.FalloffExponentTextBox.Size = new System.Drawing.Size(154, 20);
            this.FalloffExponentTextBox.TabIndex = 122;
            this.FalloffExponentTextBox.TextChanged += new System.EventHandler(this.FalloffExponentTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 196);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 123;
            this.label6.Text = "Hash:";
            // 
            // HashTextBox
            // 
            this.HashTextBox.Location = new System.Drawing.Point(82, 193);
            this.HashTextBox.Name = "HashTextBox";
            this.HashTextBox.Size = new System.Drawing.Size(154, 20);
            this.HashTextBox.TabIndex = 124;
            this.HashTextBox.TextChanged += new System.EventHandler(this.HashTextBox_TextChanged);
            // 
            // InnerAngleUpDown
            // 
            this.InnerAngleUpDown.Location = new System.Drawing.Point(82, 219);
            this.InnerAngleUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.InnerAngleUpDown.Name = "InnerAngleUpDown";
            this.InnerAngleUpDown.Size = new System.Drawing.Size(154, 20);
            this.InnerAngleUpDown.TabIndex = 126;
            this.InnerAngleUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.InnerAngleUpDown.ValueChanged += new System.EventHandler(this.InnerAngleUpDown_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 221);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 13);
            this.label7.TabIndex = 125;
            this.label7.Text = "Inner angle:";
            // 
            // OuterAngleUpDown
            // 
            this.OuterAngleUpDown.Location = new System.Drawing.Point(82, 245);
            this.OuterAngleUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.OuterAngleUpDown.Name = "OuterAngleUpDown";
            this.OuterAngleUpDown.Size = new System.Drawing.Size(154, 20);
            this.OuterAngleUpDown.TabIndex = 128;
            this.OuterAngleUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.OuterAngleUpDown.ValueChanged += new System.EventHandler(this.OuterAngleUpDown_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 247);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 13);
            this.label8.TabIndex = 127;
            this.label8.Text = "Outer angle:";
            // 
            // CoronaIntensityUpDown
            // 
            this.CoronaIntensityUpDown.Location = new System.Drawing.Point(82, 271);
            this.CoronaIntensityUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.CoronaIntensityUpDown.Name = "CoronaIntensityUpDown";
            this.CoronaIntensityUpDown.Size = new System.Drawing.Size(154, 20);
            this.CoronaIntensityUpDown.TabIndex = 130;
            this.CoronaIntensityUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.CoronaIntensityUpDown.ValueChanged += new System.EventHandler(this.CoronaIntensityUpDown_ValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 273);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(44, 13);
            this.label9.TabIndex = 129;
            this.label9.Text = "Corona:";
            // 
            // TimeFlagsAMCheckedListBox
            // 
            this.TimeFlagsAMCheckedListBox.CheckOnClick = true;
            this.TimeFlagsAMCheckedListBox.FormattingEnabled = true;
            this.TimeFlagsAMCheckedListBox.Items.AddRange(new object[] {
            "00:00 - 01:00",
            "01:00 - 02:00",
            "02:00 - 03:00",
            "03:00 - 04:00",
            "04:00 - 05:00",
            "05:00 - 06:00",
            "06:00 - 07:00",
            "07:00 - 08:00",
            "08:00 - 09:00",
            "09:00 - 10:00",
            "10:00 - 11:00",
            "11:00 - 12:00"});
            this.TimeFlagsAMCheckedListBox.Location = new System.Drawing.Point(350, 87);
            this.TimeFlagsAMCheckedListBox.Name = "TimeFlagsAMCheckedListBox";
            this.TimeFlagsAMCheckedListBox.Size = new System.Drawing.Size(95, 184);
            this.TimeFlagsAMCheckedListBox.TabIndex = 131;
            this.TimeFlagsAMCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.TimeFlagsAMCheckedListBox_ItemCheck);
            // 
            // TimeFlagsPMCheckedListBox
            // 
            this.TimeFlagsPMCheckedListBox.CheckOnClick = true;
            this.TimeFlagsPMCheckedListBox.FormattingEnabled = true;
            this.TimeFlagsPMCheckedListBox.Items.AddRange(new object[] {
            "12:00 - 13:00",
            "13:00 - 14:00",
            "14:00 - 15:00",
            "15:00 - 16:00",
            "16:00 - 17:00",
            "17:00 - 18:00",
            "18:00 - 19:00",
            "19:00 - 20:00",
            "20:00 - 21:00",
            "21:00 - 22:00",
            "22:00 - 23:00",
            "23:00 - 00:00"});
            this.TimeFlagsPMCheckedListBox.Location = new System.Drawing.Point(451, 87);
            this.TimeFlagsPMCheckedListBox.Name = "TimeFlagsPMCheckedListBox";
            this.TimeFlagsPMCheckedListBox.Size = new System.Drawing.Size(95, 184);
            this.TimeFlagsPMCheckedListBox.TabIndex = 132;
            this.TimeFlagsPMCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.TimeFlagsPMCheckedListBox_ItemCheck);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(258, 63);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(86, 13);
            this.label10.TabIndex = 134;
            this.label10.Text = "Time/state flags:";
            // 
            // TimeStateFlagsTextBox
            // 
            this.TimeStateFlagsTextBox.Location = new System.Drawing.Point(350, 60);
            this.TimeStateFlagsTextBox.Name = "TimeStateFlagsTextBox";
            this.TimeStateFlagsTextBox.Size = new System.Drawing.Size(196, 20);
            this.TimeStateFlagsTextBox.TabIndex = 135;
            this.TimeStateFlagsTextBox.TextChanged += new System.EventHandler(this.TimeStateFlagsTextBox_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(286, 89);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(58, 13);
            this.label11.TabIndex = 136;
            this.label11.Text = "Time flags:";
            // 
            // StateFlags1CheckedListBox
            // 
            this.StateFlags1CheckedListBox.CheckOnClick = true;
            this.StateFlags1CheckedListBox.FormattingEnabled = true;
            this.StateFlags1CheckedListBox.Items.AddRange(new object[] {
            "Street light",
            "Unk2"});
            this.StateFlags1CheckedListBox.Location = new System.Drawing.Point(350, 277);
            this.StateFlags1CheckedListBox.Name = "StateFlags1CheckedListBox";
            this.StateFlags1CheckedListBox.Size = new System.Drawing.Size(95, 34);
            this.StateFlags1CheckedListBox.TabIndex = 137;
            this.StateFlags1CheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.StateFlags1CheckedListBox_ItemCheck);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(284, 279);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(60, 13);
            this.label12.TabIndex = 138;
            this.label12.Text = "State flags:";
            // 
            // StateFlags2CheckedListBox
            // 
            this.StateFlags2CheckedListBox.CheckOnClick = true;
            this.StateFlags2CheckedListBox.FormattingEnabled = true;
            this.StateFlags2CheckedListBox.Items.AddRange(new object[] {
            "Unk3",
            "Unk4",
            "Unk5"});
            this.StateFlags2CheckedListBox.Location = new System.Drawing.Point(451, 277);
            this.StateFlags2CheckedListBox.Name = "StateFlags2CheckedListBox";
            this.StateFlags2CheckedListBox.Size = new System.Drawing.Size(95, 49);
            this.StateFlags2CheckedListBox.TabIndex = 139;
            this.StateFlags2CheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.StateFlags2CheckedListBox_ItemCheck);
            // 
            // ColourRUpDown
            // 
            this.ColourRUpDown.Location = new System.Drawing.Point(116, 114);
            this.ColourRUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourRUpDown.Name = "ColourRUpDown";
            this.ColourRUpDown.Size = new System.Drawing.Size(38, 20);
            this.ColourRUpDown.TabIndex = 140;
            this.ColourRUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourRUpDown.ValueChanged += new System.EventHandler(this.ColourRUpDown_ValueChanged);
            // 
            // ColourGUpDown
            // 
            this.ColourGUpDown.Location = new System.Drawing.Point(157, 114);
            this.ColourGUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourGUpDown.Name = "ColourGUpDown";
            this.ColourGUpDown.Size = new System.Drawing.Size(38, 20);
            this.ColourGUpDown.TabIndex = 141;
            this.ColourGUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourGUpDown.ValueChanged += new System.EventHandler(this.ColourGUpDown_ValueChanged);
            // 
            // ColourBUpDown
            // 
            this.ColourBUpDown.Location = new System.Drawing.Point(198, 114);
            this.ColourBUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourBUpDown.Name = "ColourBUpDown";
            this.ColourBUpDown.Size = new System.Drawing.Size(38, 20);
            this.ColourBUpDown.TabIndex = 142;
            this.ColourBUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ColourBUpDown.ValueChanged += new System.EventHandler(this.ColourBUpDown_ValueChanged);
            // 
            // EditYmapLodLightPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 407);
            this.Controls.Add(this.ColourBUpDown);
            this.Controls.Add(this.ColourGUpDown);
            this.Controls.Add(this.ColourRUpDown);
            this.Controls.Add(this.StateFlags2CheckedListBox);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.StateFlags1CheckedListBox);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.TimeStateFlagsTextBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.TimeFlagsPMCheckedListBox);
            this.Controls.Add(this.TimeFlagsAMCheckedListBox);
            this.Controls.Add(this.CoronaIntensityUpDown);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.OuterAngleUpDown);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.InnerAngleUpDown);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.HashTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.FalloffExponentTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.FalloffTextBox);
            this.Controls.Add(this.TypeComboBox);
            this.Controls.Add(this.IntensityUpDown);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ColourLabel);
            this.Controls.Add(this.NormalizeDirectionButton);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.DirectionTextBox);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.AddToProjectButton);
            this.Controls.Add(this.GoToButton);
            this.Controls.Add(this.PositionTextBox);
            this.Controls.Add(this.label31);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYmapLodLightPanel";
            this.Text = "Lod Light";
            ((System.ComponentModel.ISupportInitialize)(this.IntensityUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InnerAngleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OuterAngleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CoronaIntensityUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColourRUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColourGUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColourBUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GoToButton;
        private System.Windows.Forms.TextBox PositionTextBox;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button AddToProjectButton;
        private System.Windows.Forms.Button NormalizeDirectionButton;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox DirectionTextBox;
        private System.Windows.Forms.Label ColourLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown IntensityUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox TypeComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox FalloffTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox FalloffExponentTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox HashTextBox;
        private System.Windows.Forms.NumericUpDown InnerAngleUpDown;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown OuterAngleUpDown;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown CoronaIntensityUpDown;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckedListBox TimeFlagsAMCheckedListBox;
        private System.Windows.Forms.CheckedListBox TimeFlagsPMCheckedListBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox TimeStateFlagsTextBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckedListBox StateFlags1CheckedListBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckedListBox StateFlags2CheckedListBox;
        private System.Windows.Forms.NumericUpDown ColourRUpDown;
        private System.Windows.Forms.NumericUpDown ColourGUpDown;
        private System.Windows.Forms.NumericUpDown ColourBUpDown;
    }
}