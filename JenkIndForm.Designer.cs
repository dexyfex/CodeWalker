namespace CodeWalker
{
    partial class JenkIndForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JenkIndForm));
            this.label1 = new System.Windows.Forms.Label();
            this.HashTextBox = new System.Windows.Forms.TextBox();
            this.HexRadioButton = new System.Windows.Forms.RadioButton();
            this.UnsignedRadioButton = new System.Windows.Forms.RadioButton();
            this.SignedRadioButton = new System.Windows.Forms.RadioButton();
            this.MatchTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.label2 = new System.Windows.Forms.Label();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.LoadStringsButton = new System.Windows.Forms.Button();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.MainPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Hash:";
            // 
            // HashTextBox
            // 
            this.HashTextBox.Location = new System.Drawing.Point(44, 9);
            this.HashTextBox.Name = "HashTextBox";
            this.HashTextBox.Size = new System.Drawing.Size(134, 20);
            this.HashTextBox.TabIndex = 1;
            this.HashTextBox.TextChanged += new System.EventHandler(this.HashTextBox_TextChanged);
            // 
            // HexRadioButton
            // 
            this.HexRadioButton.AutoSize = true;
            this.HexRadioButton.Checked = true;
            this.HexRadioButton.Location = new System.Drawing.Point(184, 10);
            this.HexRadioButton.Name = "HexRadioButton";
            this.HexRadioButton.Size = new System.Drawing.Size(44, 17);
            this.HexRadioButton.TabIndex = 2;
            this.HexRadioButton.TabStop = true;
            this.HexRadioButton.Text = "Hex";
            this.HexRadioButton.UseVisualStyleBackColor = true;
            this.HexRadioButton.CheckedChanged += new System.EventHandler(this.HexRadioButton_CheckedChanged);
            // 
            // UnsignedRadioButton
            // 
            this.UnsignedRadioButton.AutoSize = true;
            this.UnsignedRadioButton.Location = new System.Drawing.Point(234, 10);
            this.UnsignedRadioButton.Name = "UnsignedRadioButton";
            this.UnsignedRadioButton.Size = new System.Drawing.Size(70, 17);
            this.UnsignedRadioButton.TabIndex = 3;
            this.UnsignedRadioButton.Text = "Unsigned";
            this.UnsignedRadioButton.UseVisualStyleBackColor = true;
            this.UnsignedRadioButton.CheckedChanged += new System.EventHandler(this.UnsignedRadioButton_CheckedChanged);
            // 
            // SignedRadioButton
            // 
            this.SignedRadioButton.AutoSize = true;
            this.SignedRadioButton.Location = new System.Drawing.Point(310, 10);
            this.SignedRadioButton.Name = "SignedRadioButton";
            this.SignedRadioButton.Size = new System.Drawing.Size(58, 17);
            this.SignedRadioButton.TabIndex = 4;
            this.SignedRadioButton.Text = "Signed";
            this.SignedRadioButton.UseVisualStyleBackColor = true;
            this.SignedRadioButton.CheckedChanged += new System.EventHandler(this.SignedRadioButton_CheckedChanged);
            // 
            // MatchTextBox
            // 
            this.MatchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MatchTextBox.Location = new System.Drawing.Point(44, 57);
            this.MatchTextBox.Multiline = true;
            this.MatchTextBox.Name = "MatchTextBox";
            this.MatchTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.MatchTextBox.Size = new System.Drawing.Size(472, 115);
            this.MatchTextBox.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Text:";
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(41, 33);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(10, 13);
            this.StatusLabel.TabIndex = 7;
            this.StatusLabel.Text = "-";
            // 
            // MainPanel
            // 
            this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainPanel.Controls.Add(this.LoadStringsButton);
            this.MainPanel.Controls.Add(this.HashTextBox);
            this.MainPanel.Controls.Add(this.StatusLabel);
            this.MainPanel.Controls.Add(this.label1);
            this.MainPanel.Controls.Add(this.label2);
            this.MainPanel.Controls.Add(this.HexRadioButton);
            this.MainPanel.Controls.Add(this.MatchTextBox);
            this.MainPanel.Controls.Add(this.UnsignedRadioButton);
            this.MainPanel.Controls.Add(this.SignedRadioButton);
            this.MainPanel.Location = new System.Drawing.Point(12, 12);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(550, 200);
            this.MainPanel.TabIndex = 8;
            // 
            // LoadStringsButton
            // 
            this.LoadStringsButton.Location = new System.Drawing.Point(410, 7);
            this.LoadStringsButton.Name = "LoadStringsButton";
            this.LoadStringsButton.Size = new System.Drawing.Size(106, 23);
            this.LoadStringsButton.TabIndex = 8;
            this.LoadStringsButton.Text = "Load strings file...";
            this.LoadStringsButton.UseVisualStyleBackColor = true;
            this.LoadStringsButton.Click += new System.EventHandler(this.LoadStringsButton_Click);
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.Filter = "Text files|*.txt|All files|*.*";
            // 
            // JenkIndForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 224);
            this.Controls.Add(this.MainPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "JenkIndForm";
            this.Text = "Jenkins Hash Lookup - CodeWalker by dexyfex";
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox HashTextBox;
        private System.Windows.Forms.RadioButton HexRadioButton;
        private System.Windows.Forms.RadioButton UnsignedRadioButton;
        private System.Windows.Forms.RadioButton SignedRadioButton;
        private WinForms.TextBoxFix MatchTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.Button LoadStringsButton;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
    }
}