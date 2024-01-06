namespace CodeWalker.Project.Panels
{
    partial class EditYtypMloPortalPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYtypMloPortalPanel));
            this.FlagsTextBox = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.RoomFromTextBox = new System.Windows.Forms.TextBox();
            this.RoomToTextBox = new System.Windows.Forms.TextBox();
            this.MirrorPriorityTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.OpacityTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.AudioOcclusionTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.CornersTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.label6 = new System.Windows.Forms.Label();
            this.FlagsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.AddEntityButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FlagsTextBox
            // 
            this.FlagsTextBox.Location = new System.Drawing.Point(107, 67);
            this.FlagsTextBox.Name = "FlagsTextBox";
            this.FlagsTextBox.Size = new System.Drawing.Size(172, 20);
            this.FlagsTextBox.TabIndex = 6;
            this.FlagsTextBox.TextChanged += new System.EventHandler(this.FlagsTextBox_TextChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(12, 70);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(35, 13);
            this.label14.TabIndex = 5;
            this.label14.Text = "Flags:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Room From: ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Room To: ";
            // 
            // RoomFromTextBox
            // 
            this.RoomFromTextBox.Location = new System.Drawing.Point(107, 15);
            this.RoomFromTextBox.Name = "RoomFromTextBox";
            this.RoomFromTextBox.Size = new System.Drawing.Size(172, 20);
            this.RoomFromTextBox.TabIndex = 2;
            this.RoomFromTextBox.TextChanged += new System.EventHandler(this.RoomFromTextBox_TextChanged);
            // 
            // RoomToTextBox
            // 
            this.RoomToTextBox.Location = new System.Drawing.Point(107, 41);
            this.RoomToTextBox.Name = "RoomToTextBox";
            this.RoomToTextBox.Size = new System.Drawing.Size(172, 20);
            this.RoomToTextBox.TabIndex = 4;
            this.RoomToTextBox.TextChanged += new System.EventHandler(this.RoomToTextBox_TextChanged);
            // 
            // MirrorPriorityTextBox
            // 
            this.MirrorPriorityTextBox.Location = new System.Drawing.Point(107, 93);
            this.MirrorPriorityTextBox.Name = "MirrorPriorityTextBox";
            this.MirrorPriorityTextBox.Size = new System.Drawing.Size(172, 20);
            this.MirrorPriorityTextBox.TabIndex = 8;
            this.MirrorPriorityTextBox.TextChanged += new System.EventHandler(this.MirrorPriorityTextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Mirror Priority: ";
            // 
            // OpacityTextBox
            // 
            this.OpacityTextBox.Location = new System.Drawing.Point(107, 119);
            this.OpacityTextBox.Name = "OpacityTextBox";
            this.OpacityTextBox.Size = new System.Drawing.Size(172, 20);
            this.OpacityTextBox.TabIndex = 10;
            this.OpacityTextBox.TextChanged += new System.EventHandler(this.OpacityTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 122);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Opacity: ";
            // 
            // AudioOcclusionTextBox
            // 
            this.AudioOcclusionTextBox.Location = new System.Drawing.Point(107, 145);
            this.AudioOcclusionTextBox.Name = "AudioOcclusionTextBox";
            this.AudioOcclusionTextBox.Size = new System.Drawing.Size(172, 20);
            this.AudioOcclusionTextBox.TabIndex = 12;
            this.AudioOcclusionTextBox.TextChanged += new System.EventHandler(this.AudioOcclusionTextBox_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 148);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Audio Occlusion: ";
            // 
            // CornersTextBox
            // 
            this.CornersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CornersTextBox.Location = new System.Drawing.Point(107, 243);
            this.CornersTextBox.Multiline = true;
            this.CornersTextBox.Name = "CornersTextBox";
            this.CornersTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.CornersTextBox.Size = new System.Drawing.Size(336, 112);
            this.CornersTextBox.TabIndex = 14;
            this.CornersTextBox.WordWrap = false;
            this.CornersTextBox.TextChanged += new System.EventHandler(this.CornersTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 246);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Corners: ";
            // 
            // FlagsCheckedListBox
            // 
            this.FlagsCheckedListBox.CheckOnClick = true;
            this.FlagsCheckedListBox.FormattingEnabled = true;
            this.FlagsCheckedListBox.Items.AddRange(new object[] {
            "1 - One-Way",
            "2 - Link Interiors together",
            "4 - Mirror",
            "8 - Disable Timecycle Modifier",
            "16 - Mirror Using Expensive Shaders",
            "32 - Low LOD Only",
            "64 - Hide when door closed",
            "128 - Mirror Can See Directional",
            "256 - Mirror Using Portal Traversal",
            "512 - Mirror Floor",
            "1024 - Mirror Can See Exterior View",
            "2048 - Water Surface",
            "4096 - Water Surface Extend To Horizon",
            "8192 - Use Light Bleed"});
            this.FlagsCheckedListBox.Location = new System.Drawing.Point(318, 15);
            this.FlagsCheckedListBox.Name = "FlagsCheckedListBox";
            this.FlagsCheckedListBox.Size = new System.Drawing.Size(184, 214);
            this.FlagsCheckedListBox.TabIndex = 36;
            this.FlagsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.FlagsCheckedListBox_ItemCheck);
            // 
            // AddEntityButton
            // 
            this.AddEntityButton.Location = new System.Drawing.Point(107, 372);
            this.AddEntityButton.Name = "AddEntityButton";
            this.AddEntityButton.Size = new System.Drawing.Size(95, 23);
            this.AddEntityButton.TabIndex = 37;
            this.AddEntityButton.Text = "Add Entity";
            this.AddEntityButton.UseVisualStyleBackColor = true;
            this.AddEntityButton.Click += new System.EventHandler(this.AddEntityButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteButton.Location = new System.Drawing.Point(348, 372);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(95, 23);
            this.DeleteButton.TabIndex = 38;
            this.DeleteButton.Text = "Delete Portal";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // EditYtypMloPortalPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 505);
            this.Controls.Add(this.AddEntityButton);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.FlagsCheckedListBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.CornersTextBox);
            this.Controls.Add(this.AudioOcclusionTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.OpacityTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.MirrorPriorityTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.RoomToTextBox);
            this.Controls.Add(this.RoomFromTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.FlagsTextBox);
            this.Controls.Add(this.label14);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYtypMloPortalPanel";
            this.Text = "Portal";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox FlagsTextBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox RoomFromTextBox;
        private System.Windows.Forms.TextBox RoomToTextBox;
        private System.Windows.Forms.TextBox MirrorPriorityTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox OpacityTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox AudioOcclusionTextBox;
        private System.Windows.Forms.Label label5;
        private WinForms.TextBoxFix CornersTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckedListBox FlagsCheckedListBox;
        private System.Windows.Forms.Button AddEntityButton;
        private System.Windows.Forms.Button DeleteButton;
    }
}