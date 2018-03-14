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
    public partial class ExtractShadersForm : Form
    {
        private volatile bool KeysLoaded = false;
        private volatile bool InProgress = false;
        private volatile bool AbortOperation = false;

        public ExtractShadersForm()
        {
            InitializeComponent();
        }

        private void ExtractShadersForm_Load(object sender, EventArgs e)
        {
            FolderTextBox.Text = GTAFolder.CurrentGTAFolder;
            OutputFolderTextBox.Text = Settings.Default.ExtractedShadersFolder;

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

        private void FolderTextBox_TextChanged(object sender, EventArgs e){}

        private void OutputFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.ExtractedShadersFolder = OutputFolderTextBox.Text;
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
            //if (Directory.GetFiles(OutputFolderTextBox.Text, "*.ysc", SearchOption.AllDirectories).Length > 0)
            //{
            //    if (MessageBox.Show("Output folder already contains .ysc files. Are you sure you want to continue?", "Output folder already contains .ysc files", MessageBoxButtons.OKCancel) != DialogResult.OK)
            //    {
            //        return;
            //    }
            //}

            InProgress = true;
            AbortOperation = false;

            string searchpath = FolderTextBox.Text;
            string outputpath = OutputFolderTextBox.Text;
            string replpath = searchpath + "\\";

            bool cso = CsoCheckBox.Checked;
            bool asm = AsmCheckBox.Checked;
            bool meta = MetaCheckBox.Checked;

            Task.Run(() =>
            {

                UpdateExtractStatus("Keys loaded.");



                RpfManager rpfman = new RpfManager();
                rpfman.Init(searchpath, UpdateExtractStatus, UpdateExtractStatus);


                UpdateExtractStatus("Beginning shader extraction...");
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
                            if (entry.NameLower.EndsWith(".fxc"))
                            {
                                UpdateExtractStatus(entry.Path);
                                FxcFile fxc = rpfman.GetFile<FxcFile>(entry);
                                if (fxc == null) throw new Exception("Couldn't load file.");

                                string basepath = outputpath + "\\" + rpf.Name.Replace(".rpf", "");


                                if (!Directory.Exists(basepath))
                                {
                                    Directory.CreateDirectory(basepath);
                                }

                                string pleft = entry.Path.Substring(0, entry.Path.Length - (entry.Name.Length + 1));
                                string ppart = pleft.Substring(pleft.LastIndexOf('\\'));
                                string opath = basepath + ppart;

                                if (!Directory.Exists(opath))
                                {
                                    Directory.CreateDirectory(opath);
                                }

                                string obase = opath + "\\" + entry.Name;

                                foreach (var shader in fxc.Shaders)
                                {
                                    string filebase = obase + "_" + shader.Name;
                                    if (cso)
                                    {
                                        string csofile = filebase + ".cso";
                                        File.WriteAllBytes(csofile, shader.ByteCode);
                                    }
                                    if (asm)
                                    {
                                        string asmfile = filebase + ".hlsl";
                                        FxcParser.ParseShader(shader);
                                        File.WriteAllText(asmfile, shader.Disassembly);
                                    }
                                }

                                if (meta)
                                {
                                    string metafile = obase + ".meta.txt";
                                    string metastr = fxc.GetMetaString();
                                    File.WriteAllText(metafile, metastr);
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
