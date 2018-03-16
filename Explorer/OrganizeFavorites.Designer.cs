namespace CodeWalker.Explorer
{
    partial class OrganizeFavorites
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Favorites");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrganizeFavorites));
            this.SaveButton = new System.Windows.Forms.Button();
            this.RemoveFavoriteButton = new System.Windows.Forms.Button();
            this.ClearAllFavoritesButton = new System.Windows.Forms.Button();
            this.FavoritesTreeView = new System.Windows.Forms.TreeView();
            this.CancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SaveButton
            // 
            this.SaveButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.SaveButton.Location = new System.Drawing.Point(324, 439);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 1;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // RemoveFavoriteButton
            // 
            this.RemoveFavoriteButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.RemoveFavoriteButton.Location = new System.Drawing.Point(226, 439);
            this.RemoveFavoriteButton.Name = "RemoveFavoriteButton";
            this.RemoveFavoriteButton.Size = new System.Drawing.Size(75, 23);
            this.RemoveFavoriteButton.TabIndex = 2;
            this.RemoveFavoriteButton.Text = "Remove";
            this.RemoveFavoriteButton.UseVisualStyleBackColor = true;
            this.RemoveFavoriteButton.Click += new System.EventHandler(this.RemoveFavoriteButton_Click);
            // 
            // ClearAllFavoritesButton
            // 
            this.ClearAllFavoritesButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ClearAllFavoritesButton.Location = new System.Drawing.Point(130, 439);
            this.ClearAllFavoritesButton.Name = "ClearAllFavoritesButton";
            this.ClearAllFavoritesButton.Size = new System.Drawing.Size(75, 23);
            this.ClearAllFavoritesButton.TabIndex = 3;
            this.ClearAllFavoritesButton.Text = "Clear";
            this.ClearAllFavoritesButton.UseVisualStyleBackColor = true;
            this.ClearAllFavoritesButton.Click += new System.EventHandler(this.ClearAllFavoritesButton_Click);
            // 
            // FavoritesTreeView
            // 
            this.FavoritesTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FavoritesTreeView.Location = new System.Drawing.Point(12, 12);
            this.FavoritesTreeView.Name = "FavoritesTreeView";
            treeNode1.Name = "FavoritesRootNode";
            treeNode1.Text = "Favorites";
            this.FavoritesTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.FavoritesTreeView.ShowLines = false;
            this.FavoritesTreeView.Size = new System.Drawing.Size(480, 406);
            this.FavoritesTreeView.TabIndex = 4;
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(419, 439);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 5;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // OrganizeFavorites
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 474);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.FavoritesTreeView);
            this.Controls.Add(this.ClearAllFavoritesButton);
            this.Controls.Add(this.RemoveFavoriteButton);
            this.Controls.Add(this.SaveButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OrganizeFavorites";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "OrganizeFavorites";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button RemoveFavoriteButton;
        private System.Windows.Forms.Button ClearAllFavoritesButton;
        private System.Windows.Forms.TreeView FavoritesTreeView;
        private System.Windows.Forms.Button CancelButton;
    }
}