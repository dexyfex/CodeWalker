
namespace CodeWalker.WinForms
{
    partial class QuaternionBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.QuaternionTextBox = new System.Windows.Forms.TextBox();
            this.EulerXUpDown = new System.Windows.Forms.NumericUpDown();
            this.EulerYUpDown = new System.Windows.Forms.NumericUpDown();
            this.EulerZUpDown = new System.Windows.Forms.NumericUpDown();
            this.NormalizeButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.EulerXUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EulerYUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EulerZUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // QuaternionTextBox
            // 
            this.QuaternionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QuaternionTextBox.Location = new System.Drawing.Point(0, 2);
            this.QuaternionTextBox.Name = "QuaternionTextBox";
            this.QuaternionTextBox.Size = new System.Drawing.Size(190, 20);
            this.QuaternionTextBox.TabIndex = 0;
            this.QuaternionTextBox.Text = "0, 0, 0, 1";
            this.QuaternionTextBox.TextChanged += new System.EventHandler(this.QuaternionTextBox_TextChanged);
            // 
            // EulerXUpDown
            // 
            this.EulerXUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EulerXUpDown.DecimalPlaces = 2;
            this.EulerXUpDown.Location = new System.Drawing.Point(196, 2);
            this.EulerXUpDown.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.EulerXUpDown.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.EulerXUpDown.Name = "EulerXUpDown";
            this.EulerXUpDown.Size = new System.Drawing.Size(56, 20);
            this.EulerXUpDown.TabIndex = 1;
            this.EulerXUpDown.ValueChanged += new System.EventHandler(this.EulerXUpDown_ValueChanged);
            // 
            // EulerYUpDown
            // 
            this.EulerYUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EulerYUpDown.DecimalPlaces = 2;
            this.EulerYUpDown.Location = new System.Drawing.Point(258, 2);
            this.EulerYUpDown.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.EulerYUpDown.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.EulerYUpDown.Name = "EulerYUpDown";
            this.EulerYUpDown.Size = new System.Drawing.Size(56, 20);
            this.EulerYUpDown.TabIndex = 2;
            this.EulerYUpDown.ValueChanged += new System.EventHandler(this.EulerYUpDown_ValueChanged);
            // 
            // EulerZUpDown
            // 
            this.EulerZUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EulerZUpDown.DecimalPlaces = 2;
            this.EulerZUpDown.Location = new System.Drawing.Point(320, 2);
            this.EulerZUpDown.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.EulerZUpDown.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.EulerZUpDown.Name = "EulerZUpDown";
            this.EulerZUpDown.Size = new System.Drawing.Size(56, 20);
            this.EulerZUpDown.TabIndex = 3;
            this.EulerZUpDown.ValueChanged += new System.EventHandler(this.EulerZUpDown_ValueChanged);
            // 
            // NormalizeButton
            // 
            this.NormalizeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NormalizeButton.Location = new System.Drawing.Point(382, 0);
            this.NormalizeButton.Name = "NormalizeButton";
            this.NormalizeButton.Size = new System.Drawing.Size(68, 23);
            this.NormalizeButton.TabIndex = 4;
            this.NormalizeButton.Text = "Normalize";
            this.NormalizeButton.UseVisualStyleBackColor = true;
            this.NormalizeButton.Click += new System.EventHandler(this.NormalizeButton_Click);
            // 
            // QuaternionBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.NormalizeButton);
            this.Controls.Add(this.EulerZUpDown);
            this.Controls.Add(this.EulerYUpDown);
            this.Controls.Add(this.EulerXUpDown);
            this.Controls.Add(this.QuaternionTextBox);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "QuaternionBox";
            this.Size = new System.Drawing.Size(450, 24);
            ((System.ComponentModel.ISupportInitialize)(this.EulerXUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EulerYUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EulerZUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox QuaternionTextBox;
        private System.Windows.Forms.NumericUpDown EulerXUpDown;
        private System.Windows.Forms.NumericUpDown EulerYUpDown;
        private System.Windows.Forms.NumericUpDown EulerZUpDown;
        private System.Windows.Forms.Button NormalizeButton;
    }
}
