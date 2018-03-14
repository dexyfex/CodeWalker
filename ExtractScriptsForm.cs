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
    public partial class ExtractScriptsForm : Form
    {
        private volatile bool KeysLoaded = false;
        private volatile bool InProgress = false;
        private volatile bool AbortOperation = false;


        public ExtractScriptsForm()
        {
            InitializeComponent();
        }

        private void ExtractForm_Load(object sender, EventArgs e)
        {
            DumpTextBox.Text = Settings.Default.GTAExeDumpFile;
            FolderTextBox.Text = GTAFolder.CurrentGTAFolder;
            OutputFolderTextBox.Text = Settings.Default.CompiledScriptFolder;

            try
            {
                GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
                KeysLoaded = true;
                UpdateDumpStatus("Ready.");
                UpdateExtractStatus("Ready to extract.");
            }
            catch
            {
                UpdateDumpStatus("Keys not found! This shouldn't happen.");
            }
        }

        private void DumpTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.GTAExeDumpFile = DumpTextBox.Text;
        }

        private void OutputFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.CompiledScriptFolder = OutputFolderTextBox.Text;
        }

        private void FolderBrowseButton_Click(object sender, EventArgs e)
        {
            GTAFolder.UpdateGTAFolder(false);
            FolderTextBox.Text = GTAFolder.CurrentGTAFolder;
        }

        private void OutputFolderBrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog.SelectedPath = OutputFolderTextBox.Text;
            DialogResult res = FolderBrowserDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                OutputFolderTextBox.Text = FolderBrowserDialog.SelectedPath;
            }
        }

        private void DumpBrowseButton_Click(object sender, EventArgs e)
        {

            DialogResult res = OpenFileDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                DumpTextBox.Text = OpenFileDialog.FileName;
            }

        }

        private void FindKeysButton_Click(object sender, EventArgs e)
        {
            if (InProgress) return;
            if (KeysLoaded)
            {
                if (MessageBox.Show("Keys are already loaded. Do you wish to scan the exe dump anyway?", "Keys already loaded", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    return;
                }
            }


            InProgress = true;
            AbortOperation = false;

            string dmppath = DumpTextBox.Text;

            Task.Run(() =>
            {
                try
                {

                    if (AbortOperation)
                    {
                        UpdateDumpStatus("Dump scan aborted.");
                        return;
                    }

                    FileInfo dmpfi = new FileInfo(dmppath);

                    UpdateDumpStatus(string.Format("Scanning {0} for keys...", dmpfi.Name));


                    byte[] exedat = File.ReadAllBytes(dmppath);
                    GTA5Keys.Generate(exedat, UpdateDumpStatus);


                    UpdateDumpStatus("Saving found keys...");

                    GTA5Keys.SaveToPath();

                    UpdateDumpStatus("Keys loaded.");
                    UpdateExtractStatus("Keys loaded, ready to extract.");
                    KeysLoaded = true;
                    InProgress = false;
                }
                catch (Exception ex)
                {
                    UpdateDumpStatus("Error - " + ex.ToString());

                    InProgress = false;
                }
            });
        }

        private void ExtractScriptsButton_Click(object sender, EventArgs e)
        {
            if (InProgress) return;

            if (!KeysLoaded)
            {
                MessageBox.Show("Please scan a GTA 5 exe dump for keys first, or include key files in this app's folder!");
                return;
            }
            if (!Directory.Exists(FolderTextBox.Text))
            {
                MessageBox.Show("Folder doesn't exist: " + FolderTextBox.Text);
                return;
            }
            if (!Directory.Exists(OutputFolderTextBox.Text))
            {
                MessageBox.Show("Folder doesn't exist: " + OutputFolderTextBox.Text);
                return;
            }
            if (Directory.GetFiles(OutputFolderTextBox.Text, "*.ysc", SearchOption.AllDirectories).Length > 0)
            {
                if (MessageBox.Show("Output folder already contains .ysc files. Are you sure you want to continue?", "Output folder already contains .ysc files", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    return;
                }
            }

            InProgress = true;
            AbortOperation = false;

            string searchpath = FolderTextBox.Text;
            string outputpath = OutputFolderTextBox.Text;
            string replpath = searchpath + "\\";

            Task.Run(() =>
            {

                UpdateExtractStatus("Keys loaded.");

                string[] allfiles = Directory.GetFiles(searchpath, "*.rpf", SearchOption.AllDirectories);
                foreach (string rpfpath in allfiles)
                {
                    RpfFile rf = new RpfFile(rpfpath, rpfpath.Replace(replpath, ""));
                    UpdateExtractStatus("Searching " + rf.Name + "...");
                    rf.ExtractScripts(outputpath, UpdateExtractStatus);
                }

                UpdateExtractStatus("Complete.");
                InProgress = false;
            });
        }



        private void UpdateDumpStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { UpdateDumpStatus(text); }));
                }
                else
                {
                    DumpStatusLabel.Text = text;
                }
            }
            catch { }
        }

        private void UpdateExtractStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { UpdateExtractStatus(text); }));
                }
                else
                {
                    ExtractStatusLabel.Text = text;
                }
            }
            catch { }
        }

    }
}
