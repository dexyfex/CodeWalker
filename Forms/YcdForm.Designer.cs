namespace CodeWalker.Forms
{
    partial class YcdForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(YcdForm));
            this.MainPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SuspendLayout();
            // 
            // MainPropertyGrid
            // 
            this.MainPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainPropertyGrid.HelpVisible = false;
            this.MainPropertyGrid.Location = new System.Drawing.Point(12, 12);
            this.MainPropertyGrid.Name = "MainPropertyGrid";
            this.MainPropertyGrid.Size = new System.Drawing.Size(631, 403);
            this.MainPropertyGrid.TabIndex = 0;
            // 
            // YcdForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 427);
            this.Controls.Add(this.MainPropertyGrid);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "YcdForm";
            this.Text = "Clip Dictionary Inspector - CodeWalker by dexyfex";
            this.ResumeLayout(false);

        }

        #endregion

        private WinForms.PropertyGridFix MainPropertyGrid;
    }
}