namespace CodeWalker.Forms
{
    partial class RelForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RelForm));
            this.RelPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.NameTableTabPage = new System.Windows.Forms.TabPage();
            this.DetailsTabPage = new System.Windows.Forms.TabPage();
            this.MainTextBox = new CodeWalker.WinForms.TextBoxFix();
            this.CloseButton = new System.Windows.Forms.Button();
            this.MainTabControl.SuspendLayout();
            this.NameTableTabPage.SuspendLayout();
            this.DetailsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // RelPropertyGrid
            // 
            this.RelPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RelPropertyGrid.HelpVisible = false;
            this.RelPropertyGrid.Location = new System.Drawing.Point(6, 6);
            this.RelPropertyGrid.Name = "RelPropertyGrid";
            this.RelPropertyGrid.Size = new System.Drawing.Size(644, 356);
            this.RelPropertyGrid.TabIndex = 0;
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTabControl.Controls.Add(this.NameTableTabPage);
            this.MainTabControl.Controls.Add(this.DetailsTabPage);
            this.MainTabControl.Location = new System.Drawing.Point(5, 5);
            this.MainTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(664, 394);
            this.MainTabControl.TabIndex = 1;
            // 
            // NameTableTabPage
            // 
            this.NameTableTabPage.Controls.Add(this.MainTextBox);
            this.NameTableTabPage.Location = new System.Drawing.Point(4, 22);
            this.NameTableTabPage.Name = "NameTableTabPage";
            this.NameTableTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.NameTableTabPage.Size = new System.Drawing.Size(656, 368);
            this.NameTableTabPage.TabIndex = 0;
            this.NameTableTabPage.Text = "Names";
            this.NameTableTabPage.UseVisualStyleBackColor = true;
            // 
            // DetailsTabPage
            // 
            this.DetailsTabPage.Controls.Add(this.RelPropertyGrid);
            this.DetailsTabPage.Location = new System.Drawing.Point(4, 22);
            this.DetailsTabPage.Name = "DetailsTabPage";
            this.DetailsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DetailsTabPage.Size = new System.Drawing.Size(656, 368);
            this.DetailsTabPage.TabIndex = 1;
            this.DetailsTabPage.Text = "Details";
            this.DetailsTabPage.UseVisualStyleBackColor = true;
            // 
            // MainTextBox
            // 
            this.MainTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainTextBox.HideSelection = false;
            this.MainTextBox.Location = new System.Drawing.Point(6, 6);
            this.MainTextBox.Multiline = true;
            this.MainTextBox.Name = "MainTextBox";
            this.MainTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.MainTextBox.Size = new System.Drawing.Size(644, 356);
            this.MainTextBox.TabIndex = 1;
            this.MainTextBox.WordWrap = false;
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.Location = new System.Drawing.Point(584, 409);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 2;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // RelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 441);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.MainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RelForm";
            this.Text = "REL Viewer - CodeWalker by dexyfex";
            this.MainTabControl.ResumeLayout(false);
            this.NameTableTabPage.ResumeLayout(false);
            this.NameTableTabPage.PerformLayout();
            this.DetailsTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private WinForms.PropertyGridFix RelPropertyGrid;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage NameTableTabPage;
        private System.Windows.Forms.TabPage DetailsTabPage;
        private WinForms.TextBoxFix MainTextBox;
        private System.Windows.Forms.Button CloseButton;
    }
}