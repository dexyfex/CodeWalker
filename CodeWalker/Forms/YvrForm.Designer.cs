namespace CodeWalker.Forms
{
    partial class YvrForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(YvrForm));
            this.CloseButton = new System.Windows.Forms.Button();
            this.MainListView = new System.Windows.Forms.ListView();
            this.PosXColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PosYColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PosZColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TimeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.VelocityXColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.VelocityYColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.VelocityZColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RightXColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RightYColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RightZColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ForwardXColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ForwardYColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ForwardZColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SteeringAngleColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.GasPedalPowerColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BrakePedalPowerColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HandbrakeUsedColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ExportButton = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.CopyClipboardButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = new System.Drawing.Point(845, 389);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 5;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // MainListView
            // 
            this.MainListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.PosXColumn,
            this.PosYColumn,
            this.PosZColumn,
            this.TimeColumn,
            this.VelocityXColumn,
            this.VelocityYColumn,
            this.VelocityZColumn,
            this.RightXColumn,
            this.RightYColumn,
            this.RightZColumn,
            this.ForwardXColumn,
            this.ForwardYColumn,
            this.ForwardZColumn,
            this.SteeringAngleColumn,
            this.GasPedalPowerColumn,
            this.BrakePedalPowerColumn,
            this.HandbrakeUsedColumn});
            this.MainListView.HideSelection = false;
            this.MainListView.Location = new System.Drawing.Point(12, 12);
            this.MainListView.Name = "MainListView";
            this.MainListView.Size = new System.Drawing.Size(908, 371);
            this.MainListView.TabIndex = 6;
            this.MainListView.UseCompatibleStateImageBehavior = false;
            this.MainListView.View = System.Windows.Forms.View.Details;
            // 
            // PosXColumn
            // 
            this.PosXColumn.Text = "X Pos";
            this.PosXColumn.Width = 72;
            // 
            // PosYColumn
            // 
            this.PosYColumn.Text = "Y Pos";
            this.PosYColumn.Width = 72;
            // 
            // PosZColumn
            // 
            this.PosZColumn.Text = "Z Pos";
            this.PosZColumn.Width = 72;
            // 
            // TimeColumn
            // 
            this.TimeColumn.Text = "Time";
            this.TimeColumn.Width = 55;
            // 
            // VelocityXColumn
            // 
            this.VelocityXColumn.Text = "X Velocity";
            this.VelocityXColumn.Width = 48;
            // 
            // VelocityYColumn
            // 
            this.VelocityYColumn.Text = "Y Velocity";
            this.VelocityYColumn.Width = 48;
            // 
            // VelocityZColumn
            // 
            this.VelocityZColumn.Text = "Z Velocity";
            this.VelocityZColumn.Width = 48;
            // 
            // RightXColumn
            // 
            this.RightXColumn.Text = "Right X";
            this.RightXColumn.Width = 48;
            // 
            // RightYColumn
            // 
            this.RightYColumn.Text = "Right Y";
            this.RightYColumn.Width = 48;
            // 
            // RightZColumn
            // 
            this.RightZColumn.Text = "Right Z";
            this.RightZColumn.Width = 48;
            // 
            // ForwardXColumn
            // 
            this.ForwardXColumn.Text = "Fwd X";
            this.ForwardXColumn.Width = 44;
            // 
            // ForwardYColumn
            // 
            this.ForwardYColumn.Text = "Fwd Y";
            this.ForwardYColumn.Width = 44;
            // 
            // ForwardZColumn
            // 
            this.ForwardZColumn.Text = "Fwd Z";
            this.ForwardZColumn.Width = 44;
            // 
            // SteeringAngleColumn
            // 
            this.SteeringAngleColumn.Text = "Steer Angle";
            this.SteeringAngleColumn.Width = 47;
            // 
            // GasPedalPowerColumn
            // 
            this.GasPedalPowerColumn.Text = "Gas Power";
            this.GasPedalPowerColumn.Width = 42;
            // 
            // BrakePedalPowerColumn
            // 
            this.BrakePedalPowerColumn.Text = "Brake Power";
            this.BrakePedalPowerColumn.Width = 50;
            // 
            // HandbrakeUsedColumn
            // 
            this.HandbrakeUsedColumn.Text = "Handbrake";
            this.HandbrakeUsedColumn.Width = 65;
            // 
            // ExportButton
            // 
            this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportButton.Enabled = false;
            this.ExportButton.Location = new System.Drawing.Point(764, 389);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(75, 23);
            this.ExportButton.TabIndex = 7;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // CopyClipboardButton
            // 
            this.CopyClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CopyClipboardButton.Enabled = false;
            this.CopyClipboardButton.Location = new System.Drawing.Point(656, 389);
            this.CopyClipboardButton.Name = "CopyClipboardButton";
            this.CopyClipboardButton.Size = new System.Drawing.Size(102, 23);
            this.CopyClipboardButton.TabIndex = 8;
            this.CopyClipboardButton.Text = "Copy to clipboard";
            this.CopyClipboardButton.UseVisualStyleBackColor = true;
            this.CopyClipboardButton.Click += new System.EventHandler(this.CopyClipboardButton_Click);
            // 
            // YvrForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CloseButton;
            this.ClientSize = new System.Drawing.Size(932, 421);
            this.Controls.Add(this.CopyClipboardButton);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.MainListView);
            this.Controls.Add(this.CloseButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "YvrForm";
            this.Text = "Vehicle Records Viewer - CodeWalker by dexyfex";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.ListView MainListView;
        private System.Windows.Forms.ColumnHeader PosXColumn;
        private System.Windows.Forms.ColumnHeader PosYColumn;
        private System.Windows.Forms.ColumnHeader PosZColumn;
        private System.Windows.Forms.ColumnHeader TimeColumn;
        private System.Windows.Forms.ColumnHeader VelocityXColumn;
        private System.Windows.Forms.ColumnHeader VelocityYColumn;
        private System.Windows.Forms.ColumnHeader VelocityZColumn;
        private System.Windows.Forms.ColumnHeader RightXColumn;
        private System.Windows.Forms.ColumnHeader RightYColumn;
        private System.Windows.Forms.ColumnHeader RightZColumn;
        private System.Windows.Forms.ColumnHeader ForwardXColumn;
        private System.Windows.Forms.ColumnHeader ForwardYColumn;
        private System.Windows.Forms.ColumnHeader ForwardZColumn;
        private System.Windows.Forms.ColumnHeader SteeringAngleColumn;
        private System.Windows.Forms.ColumnHeader GasPedalPowerColumn;
        private System.Windows.Forms.ColumnHeader BrakePedalPowerColumn;
        private System.Windows.Forms.ColumnHeader HandbrakeUsedColumn;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Button CopyClipboardButton;
    }
}