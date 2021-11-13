namespace CodeWalker
{
    partial class MenuForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuForm));
            this.ExtractScriptsButton = new System.Windows.Forms.Button();
            this.BinarySearchButton = new System.Windows.Forms.Button();
            this.RPFBrowserButton = new System.Windows.Forms.Button();
            this.WorldButton = new System.Windows.Forms.Button();
            this.ExtractTexturesButton = new System.Windows.Forms.Button();
            this.GCCollectButton = new System.Windows.Forms.Button();
            this.ExtractRawFilesButton = new System.Windows.Forms.Button();
            this.ExtractShadersButton = new System.Windows.Forms.Button();
            this.AboutButton = new System.Windows.Forms.Button();
            this.JenkGenButton = new System.Windows.Forms.Button();
            this.ExtractKeysButton = new System.Windows.Forms.Button();
            this.ProjectButton = new System.Windows.Forms.Button();
            this.JenkIndButton = new System.Windows.Forms.Button();
            this.RPFExplorerButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ExtractScriptsButton
            // 
            this.ExtractScriptsButton.Location = new System.Drawing.Point(12, 41);
            this.ExtractScriptsButton.Name = "ExtractScriptsButton";
            this.ExtractScriptsButton.Size = new System.Drawing.Size(108, 23);
            this.ExtractScriptsButton.TabIndex = 2;
            this.ExtractScriptsButton.Text = "Extract scripts...";
            this.ExtractScriptsButton.UseVisualStyleBackColor = true;
            this.ExtractScriptsButton.Click += new System.EventHandler(this.ExtractScriptsButton_Click);
            // 
            // BinarySearchButton
            // 
            this.BinarySearchButton.Location = new System.Drawing.Point(12, 157);
            this.BinarySearchButton.Name = "BinarySearchButton";
            this.BinarySearchButton.Size = new System.Drawing.Size(108, 23);
            this.BinarySearchButton.TabIndex = 6;
            this.BinarySearchButton.Text = "Binary search...";
            this.BinarySearchButton.UseVisualStyleBackColor = true;
            this.BinarySearchButton.Click += new System.EventHandler(this.BinarySearchButton_Click);
            // 
            // RPFBrowserButton
            // 
            this.RPFBrowserButton.Location = new System.Drawing.Point(188, 41);
            this.RPFBrowserButton.Name = "RPFBrowserButton";
            this.RPFBrowserButton.Size = new System.Drawing.Size(108, 23);
            this.RPFBrowserButton.TabIndex = 13;
            this.RPFBrowserButton.Text = "RPF Browser...";
            this.RPFBrowserButton.UseVisualStyleBackColor = true;
            this.RPFBrowserButton.Click += new System.EventHandler(this.RPFBrowserButton_Click);
            // 
            // WorldButton
            // 
            this.WorldButton.Location = new System.Drawing.Point(12, 186);
            this.WorldButton.Name = "WorldButton";
            this.WorldButton.Size = new System.Drawing.Size(108, 23);
            this.WorldButton.TabIndex = 7;
            this.WorldButton.Text = "World...";
            this.WorldButton.UseVisualStyleBackColor = true;
            this.WorldButton.Click += new System.EventHandler(this.WorldButton_Click);
            // 
            // ExtractTexturesButton
            // 
            this.ExtractTexturesButton.Location = new System.Drawing.Point(12, 70);
            this.ExtractTexturesButton.Name = "ExtractTexturesButton";
            this.ExtractTexturesButton.Size = new System.Drawing.Size(108, 23);
            this.ExtractTexturesButton.TabIndex = 3;
            this.ExtractTexturesButton.Text = "Extract textures...";
            this.ExtractTexturesButton.UseVisualStyleBackColor = true;
            this.ExtractTexturesButton.Click += new System.EventHandler(this.ExtractTexturesButton_Click);
            // 
            // GCCollectButton
            // 
            this.GCCollectButton.Location = new System.Drawing.Point(188, 186);
            this.GCCollectButton.Name = "GCCollectButton";
            this.GCCollectButton.Size = new System.Drawing.Size(108, 23);
            this.GCCollectButton.TabIndex = 10;
            this.GCCollectButton.Text = "GC Collect";
            this.GCCollectButton.UseVisualStyleBackColor = true;
            this.GCCollectButton.Click += new System.EventHandler(this.GCCollectButton_Click);
            // 
            // ExtractRawFilesButton
            // 
            this.ExtractRawFilesButton.Location = new System.Drawing.Point(12, 99);
            this.ExtractRawFilesButton.Name = "ExtractRawFilesButton";
            this.ExtractRawFilesButton.Size = new System.Drawing.Size(108, 23);
            this.ExtractRawFilesButton.TabIndex = 4;
            this.ExtractRawFilesButton.Text = "Extract raw files...";
            this.ExtractRawFilesButton.UseVisualStyleBackColor = true;
            this.ExtractRawFilesButton.Click += new System.EventHandler(this.ExtractRawFilesButton_Click);
            // 
            // ExtractShadersButton
            // 
            this.ExtractShadersButton.Location = new System.Drawing.Point(12, 128);
            this.ExtractShadersButton.Name = "ExtractShadersButton";
            this.ExtractShadersButton.Size = new System.Drawing.Size(108, 23);
            this.ExtractShadersButton.TabIndex = 5;
            this.ExtractShadersButton.Text = "Extract shaders...";
            this.ExtractShadersButton.UseVisualStyleBackColor = true;
            this.ExtractShadersButton.Click += new System.EventHandler(this.ExtractShadersButton_Click);
            // 
            // AboutButton
            // 
            this.AboutButton.Location = new System.Drawing.Point(188, 215);
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(108, 23);
            this.AboutButton.TabIndex = 9;
            this.AboutButton.Text = "About...";
            this.AboutButton.UseVisualStyleBackColor = true;
            this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
            // 
            // JenkGenButton
            // 
            this.JenkGenButton.Location = new System.Drawing.Point(188, 99);
            this.JenkGenButton.Name = "JenkGenButton";
            this.JenkGenButton.Size = new System.Drawing.Size(108, 23);
            this.JenkGenButton.TabIndex = 12;
            this.JenkGenButton.Text = "JenkGen...";
            this.JenkGenButton.UseVisualStyleBackColor = true;
            this.JenkGenButton.Click += new System.EventHandler(this.JenkGenButton_Click);
            // 
            // ExtractKeysButton
            // 
            this.ExtractKeysButton.Location = new System.Drawing.Point(12, 12);
            this.ExtractKeysButton.Name = "ExtractKeysButton";
            this.ExtractKeysButton.Size = new System.Drawing.Size(108, 23);
            this.ExtractKeysButton.TabIndex = 1;
            this.ExtractKeysButton.Text = "Extract keys...";
            this.ExtractKeysButton.UseVisualStyleBackColor = true;
            this.ExtractKeysButton.Click += new System.EventHandler(this.ExtractKeysButton_Click);
            // 
            // ProjectButton
            // 
            this.ProjectButton.Location = new System.Drawing.Point(12, 215);
            this.ProjectButton.Name = "ProjectButton";
            this.ProjectButton.Size = new System.Drawing.Size(108, 23);
            this.ProjectButton.TabIndex = 8;
            this.ProjectButton.Text = "Project...";
            this.ProjectButton.UseVisualStyleBackColor = true;
            this.ProjectButton.Click += new System.EventHandler(this.ProjectButton_Click);
            // 
            // JenkIndButton
            // 
            this.JenkIndButton.Location = new System.Drawing.Point(188, 128);
            this.JenkIndButton.Name = "JenkIndButton";
            this.JenkIndButton.Size = new System.Drawing.Size(108, 23);
            this.JenkIndButton.TabIndex = 11;
            this.JenkIndButton.Text = "JenkInd...";
            this.JenkIndButton.UseVisualStyleBackColor = true;
            this.JenkIndButton.Click += new System.EventHandler(this.JenkIndButton_Click);
            // 
            // RPFExplorerButton
            // 
            this.RPFExplorerButton.Location = new System.Drawing.Point(188, 12);
            this.RPFExplorerButton.Name = "RPFExplorerButton";
            this.RPFExplorerButton.Size = new System.Drawing.Size(108, 23);
            this.RPFExplorerButton.TabIndex = 0;
            this.RPFExplorerButton.Text = "RPF Explorer...";
            this.RPFExplorerButton.UseVisualStyleBackColor = true;
            this.RPFExplorerButton.Click += new System.EventHandler(this.RPFExplorerButton_Click);
            // 
            // MenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(308, 250);
            this.Controls.Add(this.RPFExplorerButton);
            this.Controls.Add(this.JenkIndButton);
            this.Controls.Add(this.ProjectButton);
            this.Controls.Add(this.ExtractKeysButton);
            this.Controls.Add(this.JenkGenButton);
            this.Controls.Add(this.AboutButton);
            this.Controls.Add(this.ExtractShadersButton);
            this.Controls.Add(this.ExtractRawFilesButton);
            this.Controls.Add(this.GCCollectButton);
            this.Controls.Add(this.ExtractTexturesButton);
            this.Controls.Add(this.WorldButton);
            this.Controls.Add(this.RPFBrowserButton);
            this.Controls.Add(this.BinarySearchButton);
            this.Controls.Add(this.ExtractScriptsButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MenuForm";
            this.Text = "CodeWalker Menu";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button ExtractScriptsButton;
        private System.Windows.Forms.Button BinarySearchButton;
        private System.Windows.Forms.Button RPFBrowserButton;
        private System.Windows.Forms.Button WorldButton;
        private System.Windows.Forms.Button ExtractTexturesButton;
        private System.Windows.Forms.Button GCCollectButton;
        private System.Windows.Forms.Button ExtractRawFilesButton;
        private System.Windows.Forms.Button ExtractShadersButton;
        private System.Windows.Forms.Button AboutButton;
        private System.Windows.Forms.Button JenkGenButton;
        private System.Windows.Forms.Button ExtractKeysButton;
        private System.Windows.Forms.Button ProjectButton;
        private System.Windows.Forms.Button JenkIndButton;
        private System.Windows.Forms.Button RPFExplorerButton;
    }
}

