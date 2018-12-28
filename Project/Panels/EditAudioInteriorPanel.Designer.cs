namespace CodeWalker.Project.Panels
{
    partial class EditAudioInteriorPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditAudioInteriorPanel));
            this.label12 = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.HashesTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.label21 = new System.Windows.Forms.Label();
            this.Flags2TextBox = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.Flags1TextBox = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.Flags0TextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(31, 15);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(64, 13);
            this.label12.TabIndex = 7;
            this.label12.Text = "Name hash:";
            // 
            // NameTextBox
            // 
            this.NameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NameTextBox.Location = new System.Drawing.Point(108, 12);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(428, 20);
            this.NameTextBox.TabIndex = 8;
            this.NameTextBox.TextChanged += new System.EventHandler(this.NameTextBox_TextChanged);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(23, 111);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(75, 13);
            this.label19.TabIndex = 15;
            this.label19.Text = "Room hashes:";
            // 
            // HashesTextBox
            // 
            this.HashesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HashesTextBox.Location = new System.Drawing.Point(108, 108);
            this.HashesTextBox.Multiline = true;
            this.HashesTextBox.Name = "HashesTextBox";
            this.HashesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.HashesTextBox.Size = new System.Drawing.Size(428, 330);
            this.HashesTextBox.TabIndex = 16;
            this.HashesTextBox.WordWrap = false;
            this.HashesTextBox.TextChanged += new System.EventHandler(this.HashesTextBox_TextChanged);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteButton.Location = new System.Drawing.Point(443, 55);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(93, 23);
            this.DeleteButton.TabIndex = 17;
            this.DeleteButton.Text = "Delete interior";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(58, 87);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(44, 13);
            this.label21.TabIndex = 13;
            this.label21.Text = "Flags 2:";
            // 
            // Flags2TextBox
            // 
            this.Flags2TextBox.Location = new System.Drawing.Point(108, 84);
            this.Flags2TextBox.Name = "Flags2TextBox";
            this.Flags2TextBox.Size = new System.Drawing.Size(155, 20);
            this.Flags2TextBox.TabIndex = 14;
            this.Flags2TextBox.TextChanged += new System.EventHandler(this.Flags2TextBox_TextChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(58, 63);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(44, 13);
            this.label14.TabIndex = 11;
            this.label14.Text = "Flags 1:";
            // 
            // Flags1TextBox
            // 
            this.Flags1TextBox.Location = new System.Drawing.Point(108, 60);
            this.Flags1TextBox.Name = "Flags1TextBox";
            this.Flags1TextBox.Size = new System.Drawing.Size(155, 20);
            this.Flags1TextBox.TabIndex = 12;
            this.Flags1TextBox.TextChanged += new System.EventHandler(this.Flags1TextBox_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(58, 39);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(44, 13);
            this.label13.TabIndex = 9;
            this.label13.Text = "Flags 0:";
            // 
            // Flags0TextBox
            // 
            this.Flags0TextBox.Location = new System.Drawing.Point(108, 36);
            this.Flags0TextBox.Name = "Flags0TextBox";
            this.Flags0TextBox.Size = new System.Drawing.Size(155, 20);
            this.Flags0TextBox.TabIndex = 10;
            this.Flags0TextBox.TextChanged += new System.EventHandler(this.Flags0TextBox_TextChanged);
            // 
            // EditAudioInteriorPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 450);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.Flags2TextBox);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.Flags1TextBox);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.Flags0TextBox);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.HashesTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditAudioInteriorPanel";
            this.Text = "EditAudioInteriorPanel";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label label19;
        private WinForms.TextBoxFix HashesTextBox;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox Flags2TextBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox Flags1TextBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox Flags0TextBox;
    }
}