namespace CodeWalker.Project.Panels
{
    partial class EditYtypArchetypeMloEntSetPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYtypArchetypeMloEntSetPanel));
            this.EntitySetNameTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.label3 = new System.Windows.Forms.Label();
            this.ForceVisibleCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.LocationsListBox = new System.Windows.Forms.ListBox();
            this.SelectedLocationGroupBox = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SelectedLocationEntityLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SelectedLocationRoomCombo = new System.Windows.Forms.ComboBox();
            this.SelectedLocationGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // EntitySetNameTextBox
            // 
            this.EntitySetNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EntitySetNameTextBox.Location = new System.Drawing.Point(72, 40);
            this.EntitySetNameTextBox.Name = "EntitySetNameTextBox";
            this.EntitySetNameTextBox.Size = new System.Drawing.Size(269, 20);
            this.EntitySetNameTextBox.TabIndex = 7;
            this.EntitySetNameTextBox.TextChanged += new System.EventHandler(this.EntitySetNameTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Name: ";
            // 
            // ForceVisibleCheckBox
            // 
            this.ForceVisibleCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ForceVisibleCheckBox.AutoSize = true;
            this.ForceVisibleCheckBox.Location = new System.Drawing.Point(403, 42);
            this.ForceVisibleCheckBox.Name = "ForceVisibleCheckBox";
            this.ForceVisibleCheckBox.Size = new System.Drawing.Size(125, 17);
            this.ForceVisibleCheckBox.TabIndex = 8;
            this.ForceVisibleCheckBox.Text = "Force visible in editor";
            this.ForceVisibleCheckBox.UseVisualStyleBackColor = true;
            this.ForceVisibleCheckBox.CheckedChanged += new System.EventHandler(this.ForceVisibleCheckBox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Locations:";
            // 
            // LocationsListBox
            // 
            this.LocationsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LocationsListBox.FormattingEnabled = true;
            this.LocationsListBox.Location = new System.Drawing.Point(72, 75);
            this.LocationsListBox.Name = "LocationsListBox";
            this.LocationsListBox.Size = new System.Drawing.Size(200, 342);
            this.LocationsListBox.TabIndex = 10;
            this.LocationsListBox.SelectedIndexChanged += new System.EventHandler(this.LocationsListBox_SelectedIndexChanged);
            // 
            // SelectedLocationGroupBox
            // 
            this.SelectedLocationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectedLocationGroupBox.Controls.Add(this.SelectedLocationRoomCombo);
            this.SelectedLocationGroupBox.Controls.Add(this.label4);
            this.SelectedLocationGroupBox.Controls.Add(this.SelectedLocationEntityLabel);
            this.SelectedLocationGroupBox.Controls.Add(this.label2);
            this.SelectedLocationGroupBox.Location = new System.Drawing.Point(278, 76);
            this.SelectedLocationGroupBox.Name = "SelectedLocationGroupBox";
            this.SelectedLocationGroupBox.Size = new System.Drawing.Size(275, 83);
            this.SelectedLocationGroupBox.TabIndex = 11;
            this.SelectedLocationGroupBox.TabStop = false;
            this.SelectedLocationGroupBox.Text = "Selected Location";
            this.SelectedLocationGroupBox.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Entity:";
            // 
            // SelectedLocationEntityLabel
            // 
            this.SelectedLocationEntityLabel.AutoSize = true;
            this.SelectedLocationEntityLabel.Location = new System.Drawing.Point(48, 25);
            this.SelectedLocationEntityLabel.Name = "SelectedLocationEntityLabel";
            this.SelectedLocationEntityLabel.Size = new System.Drawing.Size(10, 13);
            this.SelectedLocationEntityLabel.TabIndex = 11;
            this.SelectedLocationEntityLabel.Text = "-";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Room:";
            // 
            // SelectedLocationRoomCombo
            // 
            this.SelectedLocationRoomCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectedLocationRoomCombo.FormattingEnabled = true;
            this.SelectedLocationRoomCombo.Location = new System.Drawing.Point(51, 45);
            this.SelectedLocationRoomCombo.Name = "SelectedLocationRoomCombo";
            this.SelectedLocationRoomCombo.Size = new System.Drawing.Size(199, 21);
            this.SelectedLocationRoomCombo.TabIndex = 13;
            this.SelectedLocationRoomCombo.SelectedIndexChanged += new System.EventHandler(this.SelectedLocationRoomCombo_SelectedIndexChanged);
            // 
            // EditYtypArchetypeMloEntSetPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 505);
            this.Controls.Add(this.SelectedLocationGroupBox);
            this.Controls.Add(this.LocationsListBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ForceVisibleCheckBox);
            this.Controls.Add(this.EntitySetNameTextBox);
            this.Controls.Add(this.label3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYtypArchetypeMloEntSetPanel";
            this.Text = "Entity Set";
            this.SelectedLocationGroupBox.ResumeLayout(false);
            this.SelectedLocationGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private WinForms.TextBoxFix EntitySetNameTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox ForceVisibleCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox LocationsListBox;
        private System.Windows.Forms.GroupBox SelectedLocationGroupBox;
        private System.Windows.Forms.Label SelectedLocationEntityLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox SelectedLocationRoomCombo;
        private System.Windows.Forms.Label label4;
    }
}