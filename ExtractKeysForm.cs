using CodeWalker.GameFiles;
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

namespace CodeWalker
{
    public partial class ExtractKeysForm : Form
    {
        private volatile bool KeysLoaded = false;
        private volatile bool InProgress = false;
        private volatile bool AbortOperation = false;

        public ExtractKeysForm()
        {
            InitializeComponent();
        }

        private void ExtractKeysForm_Load(object sender, EventArgs e)
        {
            FolderTextBox.Text = GTAFolder.CurrentGTAFolder;

            try
            {
                GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
                KeysLoaded = true;
                UpdateStatus("Keys loaded. Nothing to do here!");
            }
            catch
            {
                //default label text has this case
                //UpdateStatus("Keys not found! Please scan a GTA 5 exe...");
            }
        }

        private void FolderBrowseButton_Click(object sender, EventArgs e)
        {
            GTAFolder.UpdateGTAFolder(false);
            FolderTextBox.Text = GTAFolder.CurrentGTAFolder;
            ExeTextBox.Text = GTAFolder.CurrentGTAFolder + @"\GTA5.exe";
        }

        private void ExeBrowseButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(FolderTextBox.Text) && !string.IsNullOrEmpty(ExeTextBox.Text))
            {
                OpenFileDialog.InitialDirectory = FolderTextBox.Text;
                OpenFileDialog.FileName = ExeTextBox.Text;
            }
            DialogResult res = OpenFileDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                ExeTextBox.Text = OpenFileDialog.FileName;
            }
        }


        private void UpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { UpdateStatus(text); }));
                }
                else
                {
                    StatusLabel.Text = text;
                }
            }
            catch { }
        }

        private void BeginButton_Click(object sender, EventArgs e)
        {
            if (InProgress) return;
            if (KeysLoaded)
            {
                if (MessageBox.Show("Keys are already loaded. Do you wish to do the extraction anyway?", "Keys already loaded", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    return;
                }
            }


            InProgress = true;
            AbortOperation = false;

            string exepath = ExeTextBox.Text;

            Task.Run(() =>
            {
                try
                {

                    if (AbortOperation)
                    {
                        UpdateStatus("Key extraction aborted.");
                        return;
                    }

                    FileInfo dmpfi = new FileInfo(exepath);

                    UpdateStatus(string.Format("Scanning {0} for keys...", dmpfi.Name));


                    byte[] exedat = File.ReadAllBytes(exepath);
                    GTA5Keys.GenerateV2(exedat, UpdateStatus);


                    UpdateStatus("Saving found keys...");

                    Settings.Default.Key = Convert.ToBase64String(GTA5Keys.PC_AES_KEY);
                    Settings.Default.Save();
                    //GTA5Keys.SaveToPath();

                    UpdateStatus("Keys extracted successfully.");
                    KeysLoaded = true;
                    InProgress = false;
                }
                catch (Exception ex)
                {
                    UpdateStatus("Error - " + ex.ToString());

                    InProgress = false;
                }
            });
        }
    }
}
