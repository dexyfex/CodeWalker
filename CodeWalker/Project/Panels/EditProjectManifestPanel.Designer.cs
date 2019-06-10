namespace CodeWalker.Project.Panels
{
    partial class EditProjectManifestPanel
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditProjectManifestPanel));
            this.ProjectManifestGenerateButton = new System.Windows.Forms.Button();
            this.ProjectManifestTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
            this.SaveManifestButton = new System.Windows.Forms.Button();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.ProjectManifestTextBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ProjectManifestGenerateButton
            // 
            this.ProjectManifestGenerateButton.Location = new System.Drawing.Point(3, 2);
            this.ProjectManifestGenerateButton.Name = "ProjectManifestGenerateButton";
            this.ProjectManifestGenerateButton.Size = new System.Drawing.Size(75, 23);
            this.ProjectManifestGenerateButton.TabIndex = 4;
            this.ProjectManifestGenerateButton.Text = "Generate";
            this.ProjectManifestGenerateButton.UseVisualStyleBackColor = true;
            this.ProjectManifestGenerateButton.Click += new System.EventHandler(this.ProjectManifestGenerateButton_Click);
            // 
            // ProjectManifestTextBox
            // 
            this.ProjectManifestTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProjectManifestTextBox.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.ProjectManifestTextBox.AutoIndentCharsPatterns = "";
            this.ProjectManifestTextBox.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            this.ProjectManifestTextBox.BackBrush = null;
            this.ProjectManifestTextBox.CharHeight = 14;
            this.ProjectManifestTextBox.CharWidth = 8;
            this.ProjectManifestTextBox.CommentPrefix = null;
            this.ProjectManifestTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ProjectManifestTextBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.ProjectManifestTextBox.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.ProjectManifestTextBox.IsReplaceMode = false;
            this.ProjectManifestTextBox.Language = FastColoredTextBoxNS.Language.XML;
            this.ProjectManifestTextBox.LeftBracket = '<';
            this.ProjectManifestTextBox.LeftBracket2 = '(';
            this.ProjectManifestTextBox.Location = new System.Drawing.Point(0, 31);
            this.ProjectManifestTextBox.Name = "ProjectManifestTextBox";
            this.ProjectManifestTextBox.Paddings = new System.Windows.Forms.Padding(0);
            this.ProjectManifestTextBox.RightBracket = '>';
            this.ProjectManifestTextBox.RightBracket2 = ')';
            this.ProjectManifestTextBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.ProjectManifestTextBox.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("ProjectManifestTextBox.ServiceColors")));
            this.ProjectManifestTextBox.Size = new System.Drawing.Size(605, 420);
            this.ProjectManifestTextBox.TabIndex = 3;
            this.ProjectManifestTextBox.Zoom = 100;
            // 
            // SaveManifestButton
            // 
            this.SaveManifestButton.Location = new System.Drawing.Point(94, 2);
            this.SaveManifestButton.Name = "SaveManifestButton";
            this.SaveManifestButton.Size = new System.Drawing.Size(113, 23);
            this.SaveManifestButton.TabIndex = 6;
            this.SaveManifestButton.Text = "Save _manifest.ymf";
            this.SaveManifestButton.UseVisualStyleBackColor = true;
            this.SaveManifestButton.Click += new System.EventHandler(this.SaveManifestButton_Click);
            // 
            // SaveFileDialog
            // 
            this.SaveFileDialog.FileName = "_manifest.ymf";
            this.SaveFileDialog.Filter = "Manifest files|*.ymf";
            // 
            // EditProjectManifestPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 451);
            this.Controls.Add(this.SaveManifestButton);
            this.Controls.Add(this.ProjectManifestGenerateButton);
            this.Controls.Add(this.ProjectManifestTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditProjectManifestPanel";
            this.Text = "_manifest.ymf";
            ((System.ComponentModel.ISupportInitialize)(this.ProjectManifestTextBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button ProjectManifestGenerateButton;
        private FastColoredTextBoxNS.FastColoredTextBox ProjectManifestTextBox;
        private System.Windows.Forms.Button SaveManifestButton;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
    }
}