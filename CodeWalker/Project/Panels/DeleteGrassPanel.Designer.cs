namespace CodeWalker.Project.Panels
{
    partial class DeleteGrassPanel
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
            this.brushModeGroupBox = new System.Windows.Forms.GroupBox();
            this.brushDeleteAnyRadio = new System.Windows.Forms.RadioButton();
            this.brushDeleteYmapRadio = new System.Windows.Forms.RadioButton();
            this.brushDisabledRadio = new System.Windows.Forms.RadioButton();
            this.brushSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.RadiusNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.radiusLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.brushDeleteBatchRadio = new System.Windows.Forms.RadioButton();
            this.currentYmapTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.brushDeleteProjectRadio = new System.Windows.Forms.RadioButton();
            this.brushModeGroupBox.SuspendLayout();
            this.brushSettingsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RadiusNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // brushModeGroupBox
            // 
            this.brushModeGroupBox.Controls.Add(this.brushDeleteProjectRadio);
            this.brushModeGroupBox.Controls.Add(this.brushDeleteBatchRadio);
            this.brushModeGroupBox.Controls.Add(this.brushDeleteAnyRadio);
            this.brushModeGroupBox.Controls.Add(this.brushDeleteYmapRadio);
            this.brushModeGroupBox.Controls.Add(this.brushDisabledRadio);
            this.brushModeGroupBox.Location = new System.Drawing.Point(12, 12);
            this.brushModeGroupBox.Name = "brushModeGroupBox";
            this.brushModeGroupBox.Size = new System.Drawing.Size(187, 138);
            this.brushModeGroupBox.TabIndex = 70;
            this.brushModeGroupBox.TabStop = false;
            this.brushModeGroupBox.Text = "Delete Grass Brush Mode";
            // 
            // brushDeleteAnyRadio
            // 
            this.brushDeleteAnyRadio.AutoSize = true;
            this.brushDeleteAnyRadio.Location = new System.Drawing.Point(16, 109);
            this.brushDeleteAnyRadio.Name = "brushDeleteAnyRadio";
            this.brushDeleteAnyRadio.Size = new System.Drawing.Size(122, 17);
            this.brushDeleteAnyRadio.TabIndex = 4;
            this.brushDeleteAnyRadio.Text = "Delete In A&ny YMAP";
            this.brushDeleteAnyRadio.UseVisualStyleBackColor = true;
            // 
            // brushDeleteYmapRadio
            // 
            this.brushDeleteYmapRadio.AutoSize = true;
            this.brushDeleteYmapRadio.Location = new System.Drawing.Point(16, 65);
            this.brushDeleteYmapRadio.Name = "brushDeleteYmapRadio";
            this.brushDeleteYmapRadio.Size = new System.Drawing.Size(124, 17);
            this.brushDeleteYmapRadio.TabIndex = 3;
            this.brushDeleteYmapRadio.Text = "Delete In This &YMAP";
            this.brushDeleteYmapRadio.UseVisualStyleBackColor = true;
            // 
            // brushDisabledRadio
            // 
            this.brushDisabledRadio.AutoSize = true;
            this.brushDisabledRadio.Checked = true;
            this.brushDisabledRadio.Location = new System.Drawing.Point(16, 21);
            this.brushDisabledRadio.Name = "brushDisabledRadio";
            this.brushDisabledRadio.Size = new System.Drawing.Size(66, 17);
            this.brushDisabledRadio.TabIndex = 0;
            this.brushDisabledRadio.TabStop = true;
            this.brushDisabledRadio.Text = "&Disabled";
            this.brushDisabledRadio.UseVisualStyleBackColor = true;
            // 
            // brushSettingsGroupBox
            // 
            this.brushSettingsGroupBox.Controls.Add(this.RadiusNumericUpDown);
            this.brushSettingsGroupBox.Controls.Add(this.radiusLabel);
            this.brushSettingsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.brushSettingsGroupBox.Location = new System.Drawing.Point(221, 13);
            this.brushSettingsGroupBox.Name = "brushSettingsGroupBox";
            this.brushSettingsGroupBox.Size = new System.Drawing.Size(140, 59);
            this.brushSettingsGroupBox.TabIndex = 71;
            this.brushSettingsGroupBox.TabStop = false;
            this.brushSettingsGroupBox.Text = "Delete Brush Settings";
            // 
            // RadiusNumericUpDown
            // 
            this.RadiusNumericUpDown.DecimalPlaces = 1;
            this.RadiusNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.RadiusNumericUpDown.Location = new System.Drawing.Point(52, 28);
            this.RadiusNumericUpDown.Name = "RadiusNumericUpDown";
            this.RadiusNumericUpDown.Size = new System.Drawing.Size(65, 20);
            this.RadiusNumericUpDown.TabIndex = 11;
            this.RadiusNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // radiusLabel
            // 
            this.radiusLabel.AutoSize = true;
            this.radiusLabel.Location = new System.Drawing.Point(6, 30);
            this.radiusLabel.Name = "radiusLabel";
            this.radiusLabel.Size = new System.Drawing.Size(40, 13);
            this.radiusLabel.TabIndex = 10;
            this.radiusLabel.Text = "Radius";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 163);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(397, 48);
            this.label1.TabIndex = 72;
            this.label1.Text = "Hold CTRL to use delete brush. In \"Any YMAP\" mode new YMAPs will be automatically" +
    " added to the project.";
            // 
            // brushDeleteBatchRadio
            // 
            this.brushDeleteBatchRadio.AutoSize = true;
            this.brushDeleteBatchRadio.Location = new System.Drawing.Point(16, 43);
            this.brushDeleteBatchRadio.Name = "brushDeleteBatchRadio";
            this.brushDeleteBatchRadio.Size = new System.Drawing.Size(122, 17);
            this.brushDeleteBatchRadio.TabIndex = 5;
            this.brushDeleteBatchRadio.Text = "Delete In This &Batch";
            this.brushDeleteBatchRadio.UseVisualStyleBackColor = true;
            // 
            // currentYmapTextBox
            // 
            this.currentYmapTextBox.Enabled = false;
            this.currentYmapTextBox.Location = new System.Drawing.Point(221, 118);
            this.currentYmapTextBox.Name = "currentYmapTextBox";
            this.currentYmapTextBox.ReadOnly = true;
            this.currentYmapTextBox.Size = new System.Drawing.Size(188, 20);
            this.currentYmapTextBox.TabIndex = 73;
            this.currentYmapTextBox.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(218, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 74;
            this.label2.Text = "Current YMAP";
            this.label2.Visible = false;
            // 
            // brushDeleteProjectRadio
            // 
            this.brushDeleteProjectRadio.AutoSize = true;
            this.brushDeleteProjectRadio.Location = new System.Drawing.Point(16, 87);
            this.brushDeleteProjectRadio.Name = "brushDeleteProjectRadio";
            this.brushDeleteProjectRadio.Size = new System.Drawing.Size(158, 17);
            this.brushDeleteProjectRadio.TabIndex = 6;
            this.brushDeleteProjectRadio.Text = "Delete In Any &Project YMAP";
            this.brushDeleteProjectRadio.UseVisualStyleBackColor = true;
            // 
            // DeleteGrassPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 228);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.currentYmapTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.brushSettingsGroupBox);
            this.Controls.Add(this.brushModeGroupBox);
            this.Name = "DeleteGrassPanel";
            this.Text = "Delete Grass Instances";
            this.brushModeGroupBox.ResumeLayout(false);
            this.brushModeGroupBox.PerformLayout();
            this.brushSettingsGroupBox.ResumeLayout(false);
            this.brushSettingsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RadiusNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox brushModeGroupBox;
        private System.Windows.Forms.RadioButton brushDeleteAnyRadio;
        private System.Windows.Forms.RadioButton brushDeleteYmapRadio;
        private System.Windows.Forms.RadioButton brushDisabledRadio;
        private System.Windows.Forms.GroupBox brushSettingsGroupBox;
        private System.Windows.Forms.NumericUpDown RadiusNumericUpDown;
        private System.Windows.Forms.Label radiusLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton brushDeleteBatchRadio;
        private System.Windows.Forms.TextBox currentYmapTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton brushDeleteProjectRadio;
    }
}