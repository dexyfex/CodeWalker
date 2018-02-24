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
    public partial class ExtractRawForm : Form
    {
        private volatile bool KeysLoaded = false;
        private volatile bool InProgress = false;
        private volatile bool AbortOperation = false;

        public ExtractRawForm()
        {
            InitializeComponent();
        }

        private void ExtractRawForm_Load(object sender, EventArgs e)
        {
            FolderTextBox.Text = GTAFolder.CurrentGTAFolder;
            OutputFolderTextBox.Text = Settings.Default.ExtractedRawFilesFolder;

            try
            {
                GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
                KeysLoaded = true;
                UpdateExtractStatus("Ready to extract.");
            }
            catch
            {
                UpdateExtractStatus("Keys not found! This shouldn't happen.");
            }
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

        private void OutputFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.ExtractedRawFilesFolder = OutputFolderTextBox.Text;
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

        private void ExtractButton_Click(object sender, EventArgs e)
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
            if(string.IsNullOrEmpty(FileMatchTextBox.Text) || (FileMatchTextBox.Text.Length < 3))
            {
                MessageBox.Show("Please enter at least 3 characters to match.");
                return;
            }

            InProgress = true;
            AbortOperation = false;

            string searchpath = FolderTextBox.Text;
            string outputpath = OutputFolderTextBox.Text;
            string replpath = searchpath + "\\";
            string matchstr = FileMatchTextBox.Text;
            bool endswith = MatchEndsWithRadio.Checked;
            bool compress = CompressCheckBox.Checked;

            Task.Run(() =>
            {

                UpdateExtractStatus("Keys loaded.");



                RpfManager rpfman = new RpfManager();
                rpfman.Init(searchpath, UpdateExtractStatus, UpdateExtractStatus);


                UpdateExtractStatus("Beginning file extraction...");
                StringBuilder errsb = new StringBuilder();
                foreach (RpfFile rpf in rpfman.AllRpfs)
                {
                    foreach (RpfEntry entry in rpf.AllEntries)
                    {
                        if (AbortOperation)
                        {
                            UpdateExtractStatus("Operation aborted");
                            InProgress = false;
                            return;
                        }
                        try
                        {
                            bool extract = false;
                            if (endswith)
                            {
                                extract = entry.NameLower.EndsWith(matchstr);
                            }
                            else
                            {
                                extract = entry.NameLower.Contains(matchstr);
                            }
                            var fentry = entry as RpfFileEntry;
                            if (fentry == null)
                            {
                                extract = false;
                            }
                            if (extract)
                            {
                                UpdateExtractStatus(entry.Path);

                                byte[] data = entry.File.ExtractFile(fentry);

                                if (compress)
                                {
                                    data = ResourceBuilder.Compress(data);
                                }

                                RpfResourceFileEntry rrfe = fentry as RpfResourceFileEntry;
                                if (rrfe != null) //add resource header if this is a resource file.
                                {
                                    data = ResourceBuilder.AddResourceHeader(rrfe, data);
                                }


                                if (data != null)
                                {
                                    var finf = new FileInfo(entry.Name);
                                    string bpath = outputpath + "\\" + entry.Name.Substring(0, entry.Name.Length - finf.Extension.Length);
                                    string fpath = bpath + finf.Extension;
                                    int c = 1;
                                    while (File.Exists(fpath))
                                    {
                                        fpath = bpath + "_" + c.ToString() + finf.Extension;
                                        c++;
                                    }

                                    File.WriteAllBytes(fpath, data);


                                }
                                else
                                {
                                    throw new Exception("Couldn't extract data.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string err = entry.Name + ": " + ex.Message;
                            UpdateExtractStatus(err);
                            errsb.AppendLine(err);
                        }

                    }
                }

                File.WriteAllText(outputpath + "\\_errors.txt", errsb.ToString());

                UpdateExtractStatus("Complete.");
                InProgress = false;
            });
        }

        private void AbortButton_Click(object sender, EventArgs e)
        {
            AbortOperation = true;
        }
    }
}
