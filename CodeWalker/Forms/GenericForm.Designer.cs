namespace CodeWalker.Forms
{
    partial class GenericForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenericForm));
            this.DetailsPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SuspendLayout();
            // 
            // DetailsPropertyGrid
            // 
            this.DetailsPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsPropertyGrid.HelpVisible = false;
            this.DetailsPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.DetailsPropertyGrid.Name = "DetailsPropertyGrid";
            this.DetailsPropertyGrid.Size = new System.Drawing.Size(692, 449);
            this.DetailsPropertyGrid.TabIndex = 1;
            // 
            // GenericForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 449);
            this.Controls.Add(this.DetailsPropertyGrid);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GenericForm";
            this.Text = "File Inspector - CodeWalker by dexyfex";
            this.ResumeLayout(false);

        }

        #endregion

        private WinForms.PropertyGridFix DetailsPropertyGrid;
    }
}