using CodeWalker.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Utils
{
    public partial class SelectFolderForm : Form
    {

        public string SelectedFolder { get; set; }
        public bool IsGen9 { get; set; }
        public DialogResult Result { get; set; } = DialogResult.Cancel;

        public SelectFolderForm()
        {
            InitializeComponent();
            SelectedFolder = GTAFolder.CurrentGTAFolder;
            IsGen9 = GTAFolder.IsGen9;
        }

        private void SelectFolderForm_Load(object sender, EventArgs e)
        {
            FolderTextBox.Text = SelectedFolder;
            RememberFolderCheckbox.Checked = Settings.Default.RememberGTAFolder;
            LegacyRadioButton.Checked = !IsGen9;
            EnhancedRadioButton.Checked = IsGen9;
        }

        private void FolderBrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog.SelectedPath = FolderTextBox.Text;
            DialogResult res = FolderBrowserDialog.ShowDialogNew();
            if (res == DialogResult.OK)
            {
                var path = FolderBrowserDialog.SelectedPath;
                var gen9 = GTAFolder.IsGen9Folder(path);
                FolderTextBox.Text = path;
                LegacyRadioButton.Checked = !gen9;
                EnhancedRadioButton.Checked = gen9;
            }
        }

        private void FolderTextBox_TextChanged(object sender, EventArgs e)
        {
            SelectedFolder = FolderTextBox.Text;
        }

        private void EnhancedRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            IsGen9 = EnhancedRadioButton.Checked;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if(!GTAFolder.ValidateGTAFolder(SelectedFolder, IsGen9, out string failReason))
            {
                MessageBox.Show("The selected folder could not be used:\n\n" + failReason, "Invalid GTA Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Result = DialogResult.OK;
            Close();
        }

        private void RememberFolderCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.RememberGTAFolder = RememberFolderCheckbox.Checked;
        }
    }
}
