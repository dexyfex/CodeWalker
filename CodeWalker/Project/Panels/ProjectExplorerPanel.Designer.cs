namespace CodeWalker.Project.Panels
{
    partial class ProjectExplorerPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectExplorerPanel));
            this.ProjectTreeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // ProjectTreeView
            // 
            this.ProjectTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ProjectTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProjectTreeView.FullRowSelect = true;
            this.ProjectTreeView.HideSelection = false;
            this.ProjectTreeView.Location = new System.Drawing.Point(0, 0);
            this.ProjectTreeView.Name = "ProjectTreeView";
            this.ProjectTreeView.ShowLines = false;
            this.ProjectTreeView.ShowRootLines = false;
            this.ProjectTreeView.Size = new System.Drawing.Size(270, 559);
            this.ProjectTreeView.TabIndex = 0;
            this.ProjectTreeView.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.ProjectTreeView_BeforeCollapse);
            this.ProjectTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.ProjectTreeView_BeforeExpand);
            this.ProjectTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ProjectTreeView_AfterSelect);
            this.ProjectTreeView.DoubleClick += new System.EventHandler(this.ProjectTreeView_DoubleClick);
            this.ProjectTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ProjectTreeView_MouseDown);
            // 
            // ProjectExplorerPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 559);
            this.Controls.Add(this.ProjectTreeView);
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProjectExplorerPanel";
            this.Text = "Project Explorer";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView ProjectTreeView;
    }
}