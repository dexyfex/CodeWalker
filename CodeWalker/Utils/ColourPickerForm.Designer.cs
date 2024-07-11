namespace CodeWalker.Utils
{
    partial class ColourPickerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColourPickerForm));
            Picker = new ColourPicker();
            ButtonOk = new System.Windows.Forms.Button();
            ButtonCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // Picker
            // 
            Picker.Location = new System.Drawing.Point(0, 0);
            Picker.Name = "Picker";
            Picker.Size = new System.Drawing.Size(450, 360);
            Picker.TabIndex = 0;
            // 
            // ButtonOk
            // 
            ButtonOk.Location = new System.Drawing.Point(229, 362);
            ButtonOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ButtonOk.Name = "ButtonOk";
            ButtonOk.Size = new System.Drawing.Size(88, 27);
            ButtonOk.TabIndex = 4;
            ButtonOk.Text = "Ok";
            ButtonOk.UseVisualStyleBackColor = true;
            ButtonOk.Click += ButtonOk_Click;
            // 
            // ButtonCancel
            // 
            ButtonCancel.Location = new System.Drawing.Point(352, 362);
            ButtonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ButtonCancel.Name = "ButtonCancel";
            ButtonCancel.Size = new System.Drawing.Size(88, 27);
            ButtonCancel.TabIndex = 3;
            ButtonCancel.Text = "Cancel";
            ButtonCancel.UseVisualStyleBackColor = true;
            ButtonCancel.Click += ButtonCancel_Click;
            // 
            // ColourPickerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(454, 398);
            Controls.Add(ButtonOk);
            Controls.Add(ButtonCancel);
            Controls.Add(Picker);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ColourPickerForm";
            Text = "Colour Picker - CodeWalker by dexyfex";
            FormClosing += ColourPickerForm_FormClosing;
            Load += ColourPickerForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private ColourPicker Picker;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Button ButtonCancel;
    }
}