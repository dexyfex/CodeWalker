
namespace CodeWalker.Project.Panels
{
    partial class EditYmapOccludeModelPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditYmapOccludeModelPanel));
            this.OccludeModelTabControl = new System.Windows.Forms.TabControl();
            this.ModelTabPage = new System.Windows.Forms.TabPage();
            this.ModelDeleteButton = new System.Windows.Forms.Button();
            this.ModelAddToProjectButton = new System.Windows.Forms.Button();
            this.TriangleTabPage = new System.Windows.Forms.TabPage();
            this.TriangleDeleteButton = new System.Windows.Forms.Button();
            this.TriangleAddToProjectButton = new System.Windows.Forms.Button();
            this.TriangleGoToButton = new System.Windows.Forms.Button();
            this.TriangleCenterTextBox = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.TriangleCorner1TextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TriangleCorner2TextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TriangleCorner3TextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ModelFlagsTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.OccludeModelTabControl.SuspendLayout();
            this.ModelTabPage.SuspendLayout();
            this.TriangleTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // OccludeModelTabControl
            // 
            this.OccludeModelTabControl.Controls.Add(this.ModelTabPage);
            this.OccludeModelTabControl.Controls.Add(this.TriangleTabPage);
            this.OccludeModelTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OccludeModelTabControl.Location = new System.Drawing.Point(0, 0);
            this.OccludeModelTabControl.Name = "OccludeModelTabControl";
            this.OccludeModelTabControl.SelectedIndex = 0;
            this.OccludeModelTabControl.Size = new System.Drawing.Size(553, 407);
            this.OccludeModelTabControl.TabIndex = 0;
            // 
            // ModelTabPage
            // 
            this.ModelTabPage.Controls.Add(this.ModelFlagsTextBox);
            this.ModelTabPage.Controls.Add(this.label4);
            this.ModelTabPage.Controls.Add(this.ModelDeleteButton);
            this.ModelTabPage.Controls.Add(this.ModelAddToProjectButton);
            this.ModelTabPage.Location = new System.Drawing.Point(4, 22);
            this.ModelTabPage.Name = "ModelTabPage";
            this.ModelTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ModelTabPage.Size = new System.Drawing.Size(545, 381);
            this.ModelTabPage.TabIndex = 0;
            this.ModelTabPage.Text = "Model";
            this.ModelTabPage.UseVisualStyleBackColor = true;
            // 
            // ModelDeleteButton
            // 
            this.ModelDeleteButton.Location = new System.Drawing.Point(176, 89);
            this.ModelDeleteButton.Name = "ModelDeleteButton";
            this.ModelDeleteButton.Size = new System.Drawing.Size(95, 23);
            this.ModelDeleteButton.TabIndex = 111;
            this.ModelDeleteButton.Text = "Delete Model";
            this.ModelDeleteButton.UseVisualStyleBackColor = true;
            this.ModelDeleteButton.Click += new System.EventHandler(this.ModelDeleteButton_Click);
            // 
            // ModelAddToProjectButton
            // 
            this.ModelAddToProjectButton.Location = new System.Drawing.Point(75, 89);
            this.ModelAddToProjectButton.Name = "ModelAddToProjectButton";
            this.ModelAddToProjectButton.Size = new System.Drawing.Size(95, 23);
            this.ModelAddToProjectButton.TabIndex = 110;
            this.ModelAddToProjectButton.Text = "Add to Project";
            this.ModelAddToProjectButton.UseVisualStyleBackColor = true;
            this.ModelAddToProjectButton.Click += new System.EventHandler(this.ModelAddToProjectButton_Click);
            // 
            // TriangleTabPage
            // 
            this.TriangleTabPage.Controls.Add(this.TriangleCorner3TextBox);
            this.TriangleTabPage.Controls.Add(this.label3);
            this.TriangleTabPage.Controls.Add(this.TriangleCorner2TextBox);
            this.TriangleTabPage.Controls.Add(this.label2);
            this.TriangleTabPage.Controls.Add(this.TriangleCorner1TextBox);
            this.TriangleTabPage.Controls.Add(this.label1);
            this.TriangleTabPage.Controls.Add(this.TriangleDeleteButton);
            this.TriangleTabPage.Controls.Add(this.TriangleAddToProjectButton);
            this.TriangleTabPage.Controls.Add(this.TriangleGoToButton);
            this.TriangleTabPage.Controls.Add(this.TriangleCenterTextBox);
            this.TriangleTabPage.Controls.Add(this.label31);
            this.TriangleTabPage.Location = new System.Drawing.Point(4, 22);
            this.TriangleTabPage.Name = "TriangleTabPage";
            this.TriangleTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.TriangleTabPage.Size = new System.Drawing.Size(545, 381);
            this.TriangleTabPage.TabIndex = 1;
            this.TriangleTabPage.Text = "Triangle";
            this.TriangleTabPage.UseVisualStyleBackColor = true;
            // 
            // TriangleDeleteButton
            // 
            this.TriangleDeleteButton.Location = new System.Drawing.Point(176, 121);
            this.TriangleDeleteButton.Name = "TriangleDeleteButton";
            this.TriangleDeleteButton.Size = new System.Drawing.Size(95, 23);
            this.TriangleDeleteButton.TabIndex = 109;
            this.TriangleDeleteButton.Text = "Delete Triangle";
            this.TriangleDeleteButton.UseVisualStyleBackColor = true;
            this.TriangleDeleteButton.Click += new System.EventHandler(this.TriangleDeleteButton_Click);
            // 
            // TriangleAddToProjectButton
            // 
            this.TriangleAddToProjectButton.Location = new System.Drawing.Point(75, 121);
            this.TriangleAddToProjectButton.Name = "TriangleAddToProjectButton";
            this.TriangleAddToProjectButton.Size = new System.Drawing.Size(95, 23);
            this.TriangleAddToProjectButton.TabIndex = 108;
            this.TriangleAddToProjectButton.Text = "Add to Project";
            this.TriangleAddToProjectButton.UseVisualStyleBackColor = true;
            this.TriangleAddToProjectButton.Click += new System.EventHandler(this.TriangleAddToProjectButton_Click);
            // 
            // TriangleGoToButton
            // 
            this.TriangleGoToButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TriangleGoToButton.Location = new System.Drawing.Point(471, 4);
            this.TriangleGoToButton.Name = "TriangleGoToButton";
            this.TriangleGoToButton.Size = new System.Drawing.Size(68, 23);
            this.TriangleGoToButton.TabIndex = 87;
            this.TriangleGoToButton.Text = "Go to";
            this.TriangleGoToButton.UseVisualStyleBackColor = true;
            this.TriangleGoToButton.Click += new System.EventHandler(this.TriangleGoToButton_Click);
            // 
            // TriangleCenterTextBox
            // 
            this.TriangleCenterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TriangleCenterTextBox.Location = new System.Drawing.Point(75, 6);
            this.TriangleCenterTextBox.Name = "TriangleCenterTextBox";
            this.TriangleCenterTextBox.Size = new System.Drawing.Size(390, 20);
            this.TriangleCenterTextBox.TabIndex = 86;
            this.TriangleCenterTextBox.TextChanged += new System.EventHandler(this.TriangleCenterTextBox_TextChanged);
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(8, 9);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(41, 13);
            this.label31.TabIndex = 85;
            this.label31.Text = "Center:";
            // 
            // TriangleCorner1TextBox
            // 
            this.TriangleCorner1TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TriangleCorner1TextBox.Location = new System.Drawing.Point(75, 32);
            this.TriangleCorner1TextBox.Name = "TriangleCorner1TextBox";
            this.TriangleCorner1TextBox.Size = new System.Drawing.Size(390, 20);
            this.TriangleCorner1TextBox.TabIndex = 111;
            this.TriangleCorner1TextBox.TextChanged += new System.EventHandler(this.TriangleCorner1TextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 110;
            this.label1.Text = "Corner 1:";
            // 
            // TriangleCorner2TextBox
            // 
            this.TriangleCorner2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TriangleCorner2TextBox.Location = new System.Drawing.Point(75, 58);
            this.TriangleCorner2TextBox.Name = "TriangleCorner2TextBox";
            this.TriangleCorner2TextBox.Size = new System.Drawing.Size(390, 20);
            this.TriangleCorner2TextBox.TabIndex = 113;
            this.TriangleCorner2TextBox.TextChanged += new System.EventHandler(this.TriangleCorner2TextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 112;
            this.label2.Text = "Corner 2:";
            // 
            // TriangleCorner3TextBox
            // 
            this.TriangleCorner3TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TriangleCorner3TextBox.Location = new System.Drawing.Point(75, 84);
            this.TriangleCorner3TextBox.Name = "TriangleCorner3TextBox";
            this.TriangleCorner3TextBox.Size = new System.Drawing.Size(390, 20);
            this.TriangleCorner3TextBox.TabIndex = 115;
            this.TriangleCorner3TextBox.TextChanged += new System.EventHandler(this.TriangleCorner3TextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 114;
            this.label3.Text = "Corner 3:";
            // 
            // ModelFlagsTextBox
            // 
            this.ModelFlagsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModelFlagsTextBox.Location = new System.Drawing.Point(75, 6);
            this.ModelFlagsTextBox.Name = "ModelFlagsTextBox";
            this.ModelFlagsTextBox.Size = new System.Drawing.Size(390, 20);
            this.ModelFlagsTextBox.TabIndex = 119;
            this.ModelFlagsTextBox.TextChanged += new System.EventHandler(this.ModelFlagsTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 118;
            this.label4.Text = "Flags:";
            // 
            // EditYmapOccludeModelPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 407);
            this.Controls.Add(this.OccludeModelTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditYmapOccludeModelPanel";
            this.Text = "Occlude Model";
            this.OccludeModelTabControl.ResumeLayout(false);
            this.ModelTabPage.ResumeLayout(false);
            this.ModelTabPage.PerformLayout();
            this.TriangleTabPage.ResumeLayout(false);
            this.TriangleTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl OccludeModelTabControl;
        private System.Windows.Forms.TabPage ModelTabPage;
        private System.Windows.Forms.TabPage TriangleTabPage;
        private System.Windows.Forms.Button TriangleGoToButton;
        private System.Windows.Forms.TextBox TriangleCenterTextBox;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Button TriangleDeleteButton;
        private System.Windows.Forms.Button TriangleAddToProjectButton;
        private System.Windows.Forms.Button ModelDeleteButton;
        private System.Windows.Forms.Button ModelAddToProjectButton;
        private System.Windows.Forms.TextBox TriangleCorner3TextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TriangleCorner2TextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TriangleCorner1TextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ModelFlagsTextBox;
        private System.Windows.Forms.Label label4;
    }
}