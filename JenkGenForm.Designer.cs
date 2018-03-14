namespace CodeWalker
{
    partial class JenkGenForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JenkGenForm));
            this.HashHexTextBox = new System.Windows.Forms.TextBox();
            this.HashUnsignedTextBox = new System.Windows.Forms.TextBox();
            this.UTF8RadioButton = new System.Windows.Forms.RadioButton();
            this.ASCIIRadioButton = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.HashSignedTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.InputTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // HashHexTextBox
            // 
            this.HashHexTextBox.Location = new System.Drawing.Point(303, 38);
            this.HashHexTextBox.Name = "HashHexTextBox";
            this.HashHexTextBox.Size = new System.Drawing.Size(119, 20);
            this.HashHexTextBox.TabIndex = 15;
            // 
            // HashUnsignedTextBox
            // 
            this.HashUnsignedTextBox.Location = new System.Drawing.Point(178, 38);
            this.HashUnsignedTextBox.Name = "HashUnsignedTextBox";
            this.HashUnsignedTextBox.Size = new System.Drawing.Size(119, 20);
            this.HashUnsignedTextBox.TabIndex = 14;
            // 
            // UTF8RadioButton
            // 
            this.UTF8RadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UTF8RadioButton.AutoSize = true;
            this.UTF8RadioButton.Checked = true;
            this.UTF8RadioButton.Location = new System.Drawing.Point(303, 13);
            this.UTF8RadioButton.Name = "UTF8RadioButton";
            this.UTF8RadioButton.Size = new System.Drawing.Size(55, 17);
            this.UTF8RadioButton.TabIndex = 13;
            this.UTF8RadioButton.TabStop = true;
            this.UTF8RadioButton.Text = "UTF-8";
            this.UTF8RadioButton.UseVisualStyleBackColor = true;
            // 
            // ASCIIRadioButton
            // 
            this.ASCIIRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ASCIIRadioButton.AutoSize = true;
            this.ASCIIRadioButton.Location = new System.Drawing.Point(364, 13);
            this.ASCIIRadioButton.Name = "ASCIIRadioButton";
            this.ASCIIRadioButton.Size = new System.Drawing.Size(52, 17);
            this.ASCIIRadioButton.TabIndex = 12;
            this.ASCIIRadioButton.Text = "ASCII";
            this.ASCIIRadioButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Hash:";
            // 
            // HashSignedTextBox
            // 
            this.HashSignedTextBox.Location = new System.Drawing.Point(53, 38);
            this.HashSignedTextBox.Name = "HashSignedTextBox";
            this.HashSignedTextBox.Size = new System.Drawing.Size(119, 20);
            this.HashSignedTextBox.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Input:";
            // 
            // InputTextBox
            // 
            this.InputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InputTextBox.Location = new System.Drawing.Point(52, 12);
            this.InputTextBox.Name = "InputTextBox";
            this.InputTextBox.Size = new System.Drawing.Size(245, 20);
            this.InputTextBox.TabIndex = 8;
            this.InputTextBox.TextChanged += new System.EventHandler(this.InputTextBox_TextChanged);
            // 
            // JenkGenForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 73);
            this.Controls.Add(this.HashHexTextBox);
            this.Controls.Add(this.HashUnsignedTextBox);
            this.Controls.Add(this.UTF8RadioButton);
            this.Controls.Add(this.ASCIIRadioButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.HashSignedTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.InputTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "JenkGenForm";
            this.Text = "Jenkins Hash Generator - CodeWalker by dexyfex";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox HashHexTextBox;
        private System.Windows.Forms.TextBox HashUnsignedTextBox;
        private System.Windows.Forms.RadioButton UTF8RadioButton;
        private System.Windows.Forms.RadioButton ASCIIRadioButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox HashSignedTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox InputTextBox;
    }
}