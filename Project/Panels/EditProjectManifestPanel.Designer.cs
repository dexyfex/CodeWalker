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
            this.label162 = new System.Windows.Forms.Label();
            this.ProjectManifestGenerateButton = new System.Windows.Forms.Button();
            this.ProjectManifestTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.ProjectManifestTextBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label162
            // 
            this.label162.AutoSize = true;
            this.label162.Location = new System.Drawing.Point(96, 7);
            this.label162.Name = "label162";
            this.label162.Size = new System.Drawing.Size(111, 13);
            this.label162.TabIndex = 5;
            this.label162.Text = "XML for _manifest.ymf";
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
            // ProjectManifestPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 451);
            this.Controls.Add(this.label162);
            this.Controls.Add(this.ProjectManifestGenerateButton);
            this.Controls.Add(this.ProjectManifestTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProjectManifestPanel";
            this.Text = "_manifest.ymf";
            ((System.ComponentModel.ISupportInitialize)(this.ProjectManifestTextBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label162;
        private System.Windows.Forms.Button ProjectManifestGenerateButton;
        private FastColoredTextBoxNS.FastColoredTextBox ProjectManifestTextBox;
    }
}