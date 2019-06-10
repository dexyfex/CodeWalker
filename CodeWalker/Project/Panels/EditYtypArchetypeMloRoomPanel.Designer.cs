namespace CodeWalker.Project.Panels
{
    partial class EditYtypArchetypeMloRoomPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYtypArchetypeMloRoomPanel));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.MinBoundsTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.MaxBoundsTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.label3 = new System.Windows.Forms.Label();
            this.RoomNameTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.RoomFlagsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.RoomFlagsTextBox = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Min:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Max:";
            // 
            // MinBoundsTextBox
            // 
            this.MinBoundsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MinBoundsTextBox.Location = new System.Drawing.Point(66, 66);
            this.MinBoundsTextBox.Name = "MinBoundsTextBox";
            this.MinBoundsTextBox.Size = new System.Drawing.Size(275, 20);
            this.MinBoundsTextBox.TabIndex = 2;
            this.MinBoundsTextBox.TextChanged += new System.EventHandler(this.MinBoundsTextBox_TextChanged);
            // 
            // MaxBoundsTextBox
            // 
            this.MaxBoundsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MaxBoundsTextBox.Location = new System.Drawing.Point(66, 92);
            this.MaxBoundsTextBox.Name = "MaxBoundsTextBox";
            this.MaxBoundsTextBox.Size = new System.Drawing.Size(275, 20);
            this.MaxBoundsTextBox.TabIndex = 3;
            this.MaxBoundsTextBox.TextChanged += new System.EventHandler(this.MaxBoundsTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Name: ";
            // 
            // RoomNameTextBox
            // 
            this.RoomNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoomNameTextBox.Location = new System.Drawing.Point(66, 40);
            this.RoomNameTextBox.Name = "RoomNameTextBox";
            this.RoomNameTextBox.Size = new System.Drawing.Size(275, 20);
            this.RoomNameTextBox.TabIndex = 5;
            this.RoomNameTextBox.TextChanged += new System.EventHandler(this.RoomNameTextBox_TextChanged);
            // 
            // RoomFlagsCheckedListBox
            // 
            this.RoomFlagsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RoomFlagsCheckedListBox.CheckOnClick = true;
            this.RoomFlagsCheckedListBox.FormattingEnabled = true;
            this.RoomFlagsCheckedListBox.Items.AddRange(new object[] {
            "1 - Unk01",
            "2 - Unk02",
            "4 - Unk03",
            "8 - Unk04",
            "16 - Unk05",
            "32 - Unk06",
            "64 - Unk07"});
            this.RoomFlagsCheckedListBox.Location = new System.Drawing.Point(352, 62);
            this.RoomFlagsCheckedListBox.Name = "RoomFlagsCheckedListBox";
            this.RoomFlagsCheckedListBox.Size = new System.Drawing.Size(201, 244);
            this.RoomFlagsCheckedListBox.TabIndex = 35;
            this.RoomFlagsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.RoomFlagsCheckedListBox_ItemCheck);
            // 
            // RoomFlagsTextBox
            // 
            this.RoomFlagsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RoomFlagsTextBox.Location = new System.Drawing.Point(406, 36);
            this.RoomFlagsTextBox.Name = "RoomFlagsTextBox";
            this.RoomFlagsTextBox.Size = new System.Drawing.Size(147, 20);
            this.RoomFlagsTextBox.TabIndex = 34;
            this.RoomFlagsTextBox.TextChanged += new System.EventHandler(this.RoomFlagsTextBox_TextChanged);
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(365, 39);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(35, 13);
            this.label14.TabIndex = 33;
            this.label14.Text = "Flags:";
            // 
            // EditYtypArchetypeMloRoomPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 505);
            this.Controls.Add(this.RoomFlagsCheckedListBox);
            this.Controls.Add(this.RoomFlagsTextBox);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.RoomNameTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.MaxBoundsTextBox);
            this.Controls.Add(this.MinBoundsTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYtypArchetypeMloRoomPanel";
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
        private WinForms.TextBoxFix RoomNameTextBox;
        private System.Windows.Forms.CheckedListBox RoomFlagsCheckedListBox;
        private System.Windows.Forms.TextBox RoomFlagsTextBox;
        private System.Windows.Forms.Label label14;
    }
}