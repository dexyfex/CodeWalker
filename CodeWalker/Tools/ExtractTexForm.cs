using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.Utils;
using CommunityToolkit.Diagnostics;
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
    public partial class ExtractTexForm : Form
    {
        private volatile bool KeysLoaded = false;
        private volatile bool InProgress = false;
        private volatile bool AbortOperation = false;

        public ExtractTexForm()
        {
            InitializeComponent();
        }

        private void ExtractTexForm_Load(object sender, EventArgs e)
        {
            FolderTextBox.Text = GTAFolder.CurrentGTAFolder;
            OutputFolderTextBox.Text = Settings.Default.ExtractedTexturesFolder;

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

        private void OutputFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.ExtractedTexturesFolder = OutputFolderTextBox.Text;
        }

        private void FolderBrowseButton_Click(object sender, EventArgs e)
        {
            GTAFolder.UpdateGTAFolder(false);
            FolderTextBox.Text = GTAFolder.CurrentGTAFolder;
        }

        private void OutputFolderBrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog.SelectedPath = OutputFolderTextBox.Text;
            DialogResult res = FolderBrowserDialog.ShowDialogNew();
            if (res == DialogResult.OK)
            {
                OutputFolderTextBox.Text = FolderBrowserDialog.SelectedPath;
            }
        }


        private async void UpdateExtractStatus(string text)
        {
            try
            {
                await this.SwitchToUiContext();
                ExtractStatusLabel.Text = text;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
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
                MessageBox.Show($"Folder doesn't exist: {FolderTextBox.Text}");
                return;
            }
            if (!Directory.Exists(OutputFolderTextBox.Text))
            {
                MessageBox.Show($"Folder doesn't exist: {OutputFolderTextBox.Text}");
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
            bool bytd = YtdChecBox.Checked;
            bool bydr = YdrCheckBox.Checked;
            bool bydd = YddCheckBox.Checked;
            bool byft = YftCheckBox.Checked;

            Task.Run(async () =>
            {

                UpdateExtractStatus("Keys loaded.");



                RpfManager rpfman = RpfManager.GetInstance();
                rpfman.Init(searchpath, UpdateExtractStatus, UpdateExtractStatus);


                UpdateExtractStatus("Beginning texture extraction...");
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
                            if (bytd && entry.IsExtension(".ytd"))
                            {
                                UpdateExtractStatus(entry.Path);
                                YtdFile? ytd = await RpfManager.GetFileAsync<YtdFile>(entry);
                                if (ytd is null)
                                {
                                    ThrowHelper.ThrowInvalidOperationException($"Couldn't load ytd file {entry.Path ?? entry.File.Path}");
                                }
                                if (ytd.TextureDict is null)
                                {
                                    ThrowHelper.ThrowInvalidOperationException($"Couldn't load texture dictionary for file {entry.Path ?? entry.File.Path}");
                                }
                                if (ytd.TextureDict.Textures is null)
                                {
                                    ThrowHelper.ThrowInvalidOperationException($"Couldn't load texture dictionary texture array for file {entry.Path ?? entry.File.Path}");
                                }
                                if (ytd.TextureDict.Textures.data_items is null)
                                {
                                    ThrowHelper.ThrowInvalidOperationException($"Texture dictionary had no entries... for file {entry.Path ?? entry.File.Path}");
                                }
                                foreach (var tex in ytd.TextureDict.Textures.data_items)
                                {
                                    await SaveTextureAsync(tex, entry, outputpath);
                                }
                            }
                            else if (bydr && entry.IsExtension(".ydr"))
                            {
                                UpdateExtractStatus(entry.Path);
                                YdrFile? ydr = await RpfManager.GetFileAsync<YdrFile>(entry);
                                if (ydr is null)
                                {
                                    ThrowHelper.ThrowInvalidOperationException($"Couldn't load ydr file {entry.Path ?? entry.File.Path}");
                                }
                                if (ydr.Drawable is null) {
                                    ThrowHelper.ThrowInvalidOperationException($"Couldn't load drawable. array for file {entry.Path ?? entry.File.Path}");
                                }
                                if (ydr.Drawable.ShaderGroup is not null)
                                {
                                    var ydrtd = ydr.Drawable.ShaderGroup.TextureDictionary;
                                    if ((ydrtd != null) && (ydrtd.Textures != null) && (ydrtd.Textures.data_items != null))
                                    {
                                        foreach (var tex in ydrtd.Textures.data_items)
                                        {
                                            await SaveTextureAsync(tex, entry, outputpath);
                                        }
                                    }
                                }
                            }
                            else if (bydd && entry.IsExtension(".ydd"))
                            {
                                UpdateExtractStatus(entry.Path);
                                YddFile? ydd = await RpfManager.GetFileAsync<YddFile>(entry);

                                if (ydd is null)
                                {
                                    ThrowHelper.ThrowInvalidOperationException($"Couldn't load ydd file {entry.Path ?? entry.File.Path}");
                                }

                                //if (ydd.DrawableDict == null) throw new Exception("Couldn't load drawable dictionary.");
                                //if (ydd.DrawableDict.Drawables == null) throw new Exception("Drawable dictionary had no items...");
                                //if (ydd.DrawableDict.Drawables.data_items == null) throw new Exception("Drawable dictionary had no items...");
                                if (ydd.Dict is null || ydd.Dict.Count == 0)
                                {
                                    ThrowHelper.ThrowInvalidDataException("Drawable dictionary has no items...");
                                }
                                foreach (var drawable in ydd.Dict.Values)
                                {
                                    if (drawable.ShaderGroup != null)
                                    {
                                        var ydrtd = drawable.ShaderGroup.TextureDictionary;
                                        if (ydrtd?.Textures?.data_items != null)
                                        {
                                            foreach (var tex in ydrtd.Textures.data_items)
                                            {
                                                await SaveTextureAsync(tex, entry, outputpath);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (byft && entry.IsExtension(".yft"))
                            {
                                UpdateExtractStatus(entry.Path);
                                YftFile? yft = await RpfManager.GetFileAsync<YftFile>(entry);
                                if (yft is null)
                                {
                                    ThrowHelper.ThrowInvalidOperationException($"Couldn't load yft file {entry.Path ?? entry.File.Path}");
                                }
                                if (yft.Fragment is null)
                                {
                                    ThrowHelper.ThrowInvalidDataException($"Couldn't load fragment for file {entry.Path ?? entry.File.Path}");
                                }
                                if (yft.Fragment.Drawable != null)
                                {
                                    if (yft.Fragment.Drawable.ShaderGroup != null)
                                    {
                                        var ydrtd = yft.Fragment.Drawable.ShaderGroup.TextureDictionary;
                                        if ((ydrtd != null) && (ydrtd.Textures != null) && (ydrtd.Textures.data_items != null))
                                        {
                                            foreach (var tex in ydrtd.Textures.data_items)
                                            {
                                                await SaveTextureAsync(tex, entry, outputpath);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            string err = $"{entry.Name}: {ex.Message}";
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

        private static async Task SaveTextureAsync(Texture tex, RpfEntry entry, string folder)
        {

            //DirectXTex

            byte[] dds = DDSIO.GetDDSFile(tex);

            string bpath = $"{folder}\\{entry.Name}_{tex.Name}";
            string fpath = bpath + ".dds";
            int c = 1;
            while (File.Exists(fpath))
            {
                fpath = bpath + "_Copy" + c.ToString() + ".dds";
                c++;
            }

            await File.WriteAllBytesAsync(fpath, dds);
        }


        private void AbortButton_Click(object sender, EventArgs e)
        {
            AbortOperation = true;
        }

    }
}
