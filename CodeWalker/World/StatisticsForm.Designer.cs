namespace CodeWalker.World
{
    partial class StatisticsForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatisticsForm));
            this.MainTimer = new System.Windows.Forms.Timer(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.GFCMemoryUsageLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.GFCItemCountLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.GFCQueueLengthLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.RCVramUsageLabel = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.RCItemCountLabel = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.RCQueueLengthLabel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.DoneButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTimer
            // 
            this.MainTimer.Enabled = true;
            this.MainTimer.Interval = 200;
            this.MainTimer.Tick += new System.EventHandler(this.MainTimer_Tick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.GFCMemoryUsageLabel);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.GFCItemCountLabel);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.GFCQueueLengthLabel);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 100);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "GameFileCache";
            // 
            // GFCMemoryUsageLabel
            // 
            this.GFCMemoryUsageLabel.AutoSize = true;
            this.GFCMemoryUsageLabel.Location = new System.Drawing.Point(90, 53);
            this.GFCMemoryUsageLabel.Name = "GFCMemoryUsageLabel";
            this.GFCMemoryUsageLabel.Size = new System.Drawing.Size(13, 13);
            this.GFCMemoryUsageLabel.TabIndex = 5;
            this.GFCMemoryUsageLabel.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 53);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Memory usage:";
            // 
            // GFCItemCountLabel
            // 
            this.GFCItemCountLabel.AutoSize = true;
            this.GFCItemCountLabel.Location = new System.Drawing.Point(90, 36);
            this.GFCItemCountLabel.Name = "GFCItemCountLabel";
            this.GFCItemCountLabel.Size = new System.Drawing.Size(13, 13);
            this.GFCItemCountLabel.TabIndex = 3;
            this.GFCItemCountLabel.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Item count:";
            // 
            // GFCQueueLengthLabel
            // 
            this.GFCQueueLengthLabel.AutoSize = true;
            this.GFCQueueLengthLabel.Location = new System.Drawing.Point(90, 19);
            this.GFCQueueLengthLabel.Name = "GFCQueueLengthLabel";
            this.GFCQueueLengthLabel.Size = new System.Drawing.Size(13, 13);
            this.GFCQueueLengthLabel.TabIndex = 1;
            this.GFCQueueLengthLabel.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Queue length:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.RCVramUsageLabel);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.RCItemCountLabel);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.RCQueueLengthLabel);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(218, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 100);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "RenderableCache";
            // 
            // RCVramUsageLabel
            // 
            this.RCVramUsageLabel.AutoSize = true;
            this.RCVramUsageLabel.Location = new System.Drawing.Point(90, 53);
            this.RCVramUsageLabel.Name = "RCVramUsageLabel";
            this.RCVramUsageLabel.Size = new System.Drawing.Size(13, 13);
            this.RCVramUsageLabel.TabIndex = 9;
            this.RCVramUsageLabel.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 53);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(73, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "VRAM usage:";
            // 
            // RCItemCountLabel
            // 
            this.RCItemCountLabel.AutoSize = true;
            this.RCItemCountLabel.Location = new System.Drawing.Point(90, 36);
            this.RCItemCountLabel.Name = "RCItemCountLabel";
            this.RCItemCountLabel.Size = new System.Drawing.Size(13, 13);
            this.RCItemCountLabel.TabIndex = 7;
            this.RCItemCountLabel.Text = "0";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 36);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(60, 13);
            this.label12.TabIndex = 6;
            this.label12.Text = "Item count:";
            // 
            // RCQueueLengthLabel
            // 
            this.RCQueueLengthLabel.AutoSize = true;
            this.RCQueueLengthLabel.Location = new System.Drawing.Point(90, 19);
            this.RCQueueLengthLabel.Name = "RCQueueLengthLabel";
            this.RCQueueLengthLabel.Size = new System.Drawing.Size(13, 13);
            this.RCQueueLengthLabel.TabIndex = 3;
            this.RCQueueLengthLabel.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 19);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "Queue length:";
            // 
            // DoneButton
            // 
            this.DoneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DoneButton.Location = new System.Drawing.Point(340, 130);
            this.DoneButton.Name = "DoneButton";
            this.DoneButton.Size = new System.Drawing.Size(75, 23);
            this.DoneButton.TabIndex = 2;
            this.DoneButton.Text = "Done";
            this.DoneButton.UseVisualStyleBackColor = true;
            this.DoneButton.Click += new System.EventHandler(this.DoneButton_Click);
            // 
            // StatisticsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 165);
            this.Controls.Add(this.DoneButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StatisticsForm";
            this.Text = "Performance Statistics - CodeWalker by dexyfex";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer MainTimer;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label GFCQueueLengthLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button DoneButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label GFCItemCountLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label RCQueueLengthLabel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label RCVramUsageLabel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label RCItemCountLabel;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label GFCMemoryUsageLabel;
    }
}