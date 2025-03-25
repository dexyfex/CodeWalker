using CodeWalker.GameFiles;
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

namespace CodeWalker.Tools
{
    public partial class ConvertAssetsForm : Form
    {
        public ConvertAssetsForm()
        {
            InitializeComponent();
        }

        private void SelectFolder(TextBox tb)
        {
            FolderBrowserDialog.SelectedPath = tb.Text;
            var res = FolderBrowserDialog.ShowDialogNew();
            if (res == DialogResult.OK)
            {
                tb.Text = FolderBrowserDialog.SelectedPath;
            }
        }

        private void InputFolderBrowseButton_Click(object sender, EventArgs e)
        {
            SelectFolder(InputFolderTextBox);
        }

        private void OutputFolderBrowseButton_Click(object sender, EventArgs e)
        {
            SelectFolder(OutputFolderTextBox);
        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            LogTextBox.Clear();
            var inputFolder = InputFolderTextBox.Text?.Replace('/', '\\');
            var outputFolder = OutputFolderTextBox.Text?.Replace('/', '\\');
            var subfolders = SubfoldersCheckbox.Checked;
            var overwrite = OverwriteCheckbox.Checked;
            var copyunconverted = true;
            if (string.IsNullOrEmpty(inputFolder))
            {
                MessageBox.Show("Please select an input folder.");
                return;
            }
            if (string.IsNullOrEmpty(outputFolder))
            {
                MessageBox.Show("Please select an output folder.");
                return;
            }
            if (inputFolder.EndsWith("\\") == false)
            {
                inputFolder = inputFolder + "\\";
            }
            if (outputFolder.EndsWith("\\") == false)
            {
                outputFolder = outputFolder + "\\";
            }
            if (inputFolder.Equals(outputFolder, StringComparison.InvariantCultureIgnoreCase))
            {
                MessageBox.Show("Input folder and Output folder must be different.");
                return;
            }
            if (Directory.Exists(inputFolder) == false)
            {
                MessageBox.Show($"Input folder {inputFolder} does not exist.");
                return;
            }
            if (Directory.Exists(outputFolder) == false)
            {
                if (MessageBox.Show($"Output folder {outputFolder} does not exist.\nWould you like to create it?", "Create output folder?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(outputFolder);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating Output folder:\n{ex.Message}");
                        return;
                    }
                    if (Directory.Exists(outputFolder) == false)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            string[] allpaths = null;
            try
            {
                var soption = subfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                allpaths = Directory.GetFileSystemEntries(inputFolder, "*", soption);
                if ((allpaths == null) || (allpaths.Length == 0))
                {
                    MessageBox.Show($"Input folder {inputFolder} is empty.");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error listing Input folder contents:\n{ex.Message}");
                return;
            }


            Task.Run(new Action(() =>
            {
                EnableForm(false);
                Log($"Conversion process started at {DateTime.Now}");
                Log($"Input folder: {inputFolder}\r\nOutput folder: {outputFolder}");

                var exgen9 = RpfManager.IsGen9;
                RpfManager.IsGen9 = true;

                foreach (var path in allpaths)
                {
                    try
                    {
                        var pathl = path.ToLowerInvariant();
                        var relpath = path.Substring(inputFolder.Length);
                        var outpath = outputFolder + relpath;
                        if ((overwrite == false) && File.Exists(outpath))
                        {
                            Log($"{relpath} - output file already exists, skipping.");
                            continue;
                        }
                        if (File.Exists(path) == false)
                        {
                            //Log($"{relpath} - input file does not exist, skipping.");
                            continue;
                        }
                        var outdir = Path.GetDirectoryName(outpath);
                        if (Directory.Exists(outdir) == false)
                        {
                            Directory.CreateDirectory(outdir);
                        }

                        //Log($"{relpath}...");
                        var datain = File.ReadAllBytes(path);
                        var dataout = (byte[])null;
                        var rfe = (RpfResourceFileEntry)null;
                        var ext = Path.GetExtension(pathl);
                        switch (ext)
                        {
                            case ".ytd":
                                var ytd = new YtdFile();
                                ytd.Load(datain);
                                rfe = ytd.RpfFileEntry as RpfResourceFileEntry;
                                if (rfe?.Version == 5)
                                {
                                    Log($"{relpath} - already gen9 format, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - converting...");
                                    dataout = ytd.Save();
                                }
                                break;
                            case ".ydr":
                                var ydr = new YdrFile();
                                ydr.Load(datain);
                                rfe = ydr.RpfFileEntry as RpfResourceFileEntry;
                                if (rfe?.Version == 159)
                                {
                                    Log($"{relpath} - already gen9 format, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - converting...");
                                    dataout = ydr.Save();
                                }
                                break;
                            case ".ydd":
                                var ydd = new YddFile();
                                ydd.Load(datain);
                                rfe = ydd.RpfFileEntry as RpfResourceFileEntry;
                                if (rfe?.Version == 159)
                                {
                                    Log($"{relpath} - already gen9 format, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - converting...");
                                    dataout = ydd.Save();
                                }
                                break;
                            case ".yft":
                                var yft = new YftFile();
                                yft.Load(datain);
                                rfe = yft.RpfFileEntry as RpfResourceFileEntry;
                                if (rfe?.Version == 171)
                                {
                                    Log($"{relpath} - already gen9 format, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - converting...");
                                    dataout = yft.Save();
                                }
                                break;
                            case ".ypt":
                                var ypt = new YptFile();
                                ypt.Load(datain);
                                rfe = ypt.RpfFileEntry as RpfResourceFileEntry;
                                if (rfe?.Version == 71)
                                {
                                    Log($"{relpath} - already gen9 format, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - converting...");
                                    dataout = ypt.Save();
                                }
                                break;
                            case ".rpf":
                                Log($"{relpath} - Cannot convert RPF files! Extract the contents of the RPF and convert that instead.");
                                break;
                            default:
                                if (copyunconverted)
                                {
                                    Log($"{relpath} - conversion not required, directly copying file.");
                                    dataout = datain;
                                }
                                else
                                {
                                    Log($"{relpath} - conversion not required, skipping.");
                                }
                                break;
                        }
                        if (dataout != null)
                        {
                            File.WriteAllBytes(outpath, dataout);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Error processing {path}:\r\n{ex}");
                    }
                }

                RpfManager.IsGen9 = exgen9;

                Log($"Conversion process completed at {DateTime.Now}");
                EnableForm(true);
            }));


        }

        private void EnableForm(bool enable)
        {
            BeginInvoke(new Action(() =>
            {
                InputFolderTextBox.Enabled = enable;
                InputFolderBrowseButton.Enabled = enable;
                OutputFolderTextBox.Enabled = enable;
                OutputFolderBrowseButton.Enabled = enable;
                SubfoldersCheckbox.Enabled = enable;
                OverwriteCheckbox.Enabled = enable;
                ProcessButton.Enabled = enable;
            }));
        }

        private void Log(string text)
        {
            BeginInvoke(new Action(() =>
            {
                LogTextBox.AppendText(text + "\r\n");
                LogTextBox.ScrollToCaret();
            }));
        }


    }
}
