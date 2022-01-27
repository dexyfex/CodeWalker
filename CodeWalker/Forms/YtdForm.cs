using CodeWalker.GameFiles;
using CodeWalker.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Forms
{
    public partial class YtdForm : Form
    {
        private string FileName;
        private YtdFile Ytd { get; set; }
        private TextureDictionary TexDict { get; set; }
        private Texture CurrentTexture = null;
        private float CurrentZoom = 0.0f; //1.0 = 100%, 0.0 = stretch
        private bool Modified = false;
        private ExploreForm ExploreForm = null;
        private ModelForm ModelForm = null;


        public YtdForm(ExploreForm exploreForm = null, ModelForm modelForm = null)
        {
            ExploreForm = exploreForm;
            ModelForm = modelForm;
            InitializeComponent();
        }


        public void LoadYtd(YtdFile ytd)
        {
            Ytd = ytd;

            FileName = ytd?.Name;
            if (string.IsNullOrEmpty(FileName))
            {
                FileName = ytd?.RpfFileEntry?.Name;
            }

            LoadTexDict(ytd.TextureDict, FileName);
        }
        public void LoadTexDict(TextureDictionary texdict, string filename)
        {
            TexDict = texdict;
            FileName = filename;

            TexturesListView.Items.Clear();
            SelTexturePictureBox.Image = null;
            SelTextureNameTextBox.Text = string.Empty;
            SelTextureDimensionsLabel.Text = "-";
            SelTextureMipLabel.Text = "0";
            SelTextureMipTrackBar.Value = 0;
            SelTextureMipTrackBar.Maximum = 0;

            if (TexDict == null)
            {
                return;
            }


            if ((TexDict.Textures == null) || (TexDict.Textures.data_items == null)) return;
            var texs = TexDict.Textures.data_items;
            List<Texture> texlist = new List<Texture>(texs);
            texlist.Sort((a, b) => { return a.Name?.CompareTo(b.Name) ?? 0; });

            foreach (var tex in texlist)
            {
                ListViewItem lvi = TexturesListView.Items.Add(tex.Name);
                lvi.ToolTipText = tex.Name;
                lvi.Tag = tex;
                lvi.SubItems.Add(tex.Width.ToString() + " x " + tex.Height.ToString());
            }

            if (TexturesListView.Items.Count > 0)
            {
                TexturesListView.Items[0].Selected = true;
            }
            UpdateStatus(GetTexCountStr());


            UpdateFormTitle();
            UpdateSaveYTDAs();
        }

        private string GetTexCountStr()
        {
            var texs = TexDict?.Textures?.data_items;
            if (texs == null) return "";
            return texs.Length.ToString() + " texture" + ((texs.Length != 1) ? "s" : "");
        }


        private void SelectTexture(Texture tex)
        {
            TexturesListView.SelectedItems.Clear();
            if (tex == null) return;
            foreach (ListViewItem lvi in TexturesListView.Items)
            {
                if (lvi.Tag == tex)
                {
                    lvi.Selected = true;
                    break;
                }
            }
        }

        private void ShowTextureMip(Texture tex, int mip, bool mipchange)
        {
            CurrentTexture = tex;
            UpdateSaveTextureAs();

            if (tex == null)
            {
                SelTexturePictureBox.Image = null;
                SelTextureNameTextBox.Text = string.Empty;
                SelTextureDimensionsLabel.Text = "-";
                SelTextureMipLabel.Text = "0";
                SelTextureMipTrackBar.Value = 0;
                SelTextureMipTrackBar.Maximum = 0;
                DetailsPropertyGrid.SelectedObject = null;
                RemoveTextureButton.Enabled = false;
                ReplaceTextureButton.Enabled = false;
                UpdateStatus(GetTexCountStr());
                return;
            }

            RemoveTextureButton.Enabled = true;
            ReplaceTextureButton.Enabled = true;


            if (mipchange)
            {
                if (mip >= tex.Levels) mip = tex.Levels - 1;
            }
            else
            {
                SelTextureMipTrackBar.Maximum = tex.Levels - 1;
            }

            SelTextureNameTextBox.Text = tex.Name;
            DetailsPropertyGrid.SelectedObject = tex;


            try
            {
                int cmip = Math.Min(Math.Max(mip, 0), tex.Levels - 1);
                byte[] pixels = DDSIO.GetPixels(tex, cmip);
                int w = tex.Width >> cmip;
                int h = tex.Height >> cmip;
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);

                if (pixels != null)
                {
                    var BoundsRect = new System.Drawing.Rectangle(0, 0, w, h);
                    BitmapData bmpData = bmp.LockBits(BoundsRect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                    IntPtr ptr = bmpData.Scan0;
                    int bytes = bmpData.Stride * bmp.Height;
                    Marshal.Copy(pixels, 0, ptr, bytes);
                    bmp.UnlockBits(bmpData);
                }

                var dimstr = w.ToString() + " x " + h.ToString();

                SelTexturePictureBox.Image = bmp;
                SelTextureDimensionsLabel.Text = dimstr;

                var str1 = GetTexCountStr();
                var str2 = tex.Name + ", mip " + cmip.ToString() + ", " + dimstr;
                if (!string.IsNullOrEmpty(str1))
                {
                    UpdateStatus(str1 + ". " + str2);
                }
                else
                {
                    UpdateStatus(str2);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error reading texture mip: " + ex.ToString());
                SelTexturePictureBox.Image = null;
            }

            UpdateZoom();
        }


        private void AddTexture()
        {
            if (TexDict.Textures?.data_items == null) return;

            var tex = OpenDDSFile();
            if (tex == null) return;

            var textures = new List<Texture>();
            textures.AddRange(TexDict.Textures.data_items);
            textures.Add(tex);

            TexDict.BuildFromTextureList(textures);

            Modified = true;

            LoadTexDict(TexDict, FileName);

            SelectTexture(tex);

            UpdateModelFormTextures();
        }

        private void RemoveTexture()
        {
            if (TexDict?.Textures?.data_items == null) return;
            if (CurrentTexture == null) return;

            var textures = new List<Texture>();
            foreach (var tex in TexDict.Textures.data_items)
            {
                if (tex != CurrentTexture)
                {
                    textures.Add(tex);
                }
            }

            TexDict.BuildFromTextureList(textures);

            Modified = true;

            LoadTexDict(TexDict, FileName);

            SelectTexture(null);

            UpdateModelFormTextures();
        }

        private void ReplaceTexture()
        {
            if (TexDict?.Textures?.data_items == null) return;
            if (CurrentTexture == null) return;

            var tex = OpenDDSFile();
            if (tex == null) return;

            tex.Name = CurrentTexture.Name;
            tex.NameHash = CurrentTexture.NameHash;
            tex.Usage = CurrentTexture.Usage;
            tex.UsageFlags = CurrentTexture.UsageFlags;
            tex.Unknown_32h = CurrentTexture.Unknown_32h;

            var textures = new List<Texture>();
            foreach (var t in TexDict.Textures.data_items)
            {
                if (t != CurrentTexture)
                {
                    textures.Add(t);
                }
            }
            textures.Add(tex);

            TexDict.BuildFromTextureList(textures);

            Modified = true;

            LoadTexDict(TexDict, FileName);

            SelectTexture(tex);

            UpdateModelFormTextures();
        }

        private void RenameTexture(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (TexDict?.Textures?.data_items == null) return;
            if (CurrentTexture == null) return;
            if (CurrentTexture.Name == name) return;

            var tex = CurrentTexture;

            tex.Name = name;
            tex.NameHash = JenkHash.GenHash(name.ToLowerInvariant());

            var textures = new List<Texture>();
            textures.AddRange(TexDict.Textures.data_items);

            TexDict.BuildFromTextureList(textures);

            Modified = true;

            foreach (ListViewItem lvi in TexturesListView.Items)
            {
                if (lvi.Tag == tex)
                {
                    lvi.Text = tex.Name;
                    lvi.ToolTipText = tex.Name;
                    break;
                }
            }

            UpdateFormTitle();
            UpdateSaveYTDAs();
            UpdateModelFormTextures();
        }


        private Texture OpenDDSFile()
        {
            if (OpenDDSFileDialog.ShowDialog() != DialogResult.OK) return null;
            
            var fn = OpenDDSFileDialog.FileName;

            if (!File.Exists(fn)) return null; //couldn't find file?

            try
            {
                var dds = File.ReadAllBytes(fn);
                var tex = DDSIO.GetTexture(dds);
                tex.Name = Path.GetFileNameWithoutExtension(fn);
                tex.NameHash = JenkHash.GenHash(tex.Name?.ToLowerInvariant());
                JenkIndex.Ensure(tex.Name?.ToLowerInvariant());
                return tex;
            }
            catch
            {
                MessageBox.Show("Unable to load " + fn + ".\nAre you sure it's a valid .dds file?");
            }

            return null;
        }


        private void UpdateModelFormTextures()
        {
            if (ModelForm == null) return;

            ModelForm.UpdateEmbeddedTextures();
        }

        private void UpdateFormTitle()
        {
            Text = FileName + (Modified ? "*" : "") + " - Texture Dictionary - CodeWalker by dexyfex";
        }

        private void UpdateStatus(string text)
        {
            StatusLabel.Text = text;
        }

        private void UpdateZoom()
        {
            //update the image controls for the current zoom level

            var img = SelTexturePictureBox.Image;

            if (CurrentZoom <= 0.0f)
            {
                //stretch image to fit the area available.
                SelTexturePanel.AutoScroll = false;
                SelTexturePictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                SelTexturePictureBox.Width = SelTexturePanel.Width - 2;
                SelTexturePictureBox.Height = SelTexturePanel.Height - 2;
                SelTexturePictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            }
            else
            {
                //zoom to the given pixel ratio...
                var w = (int)(img.Width * CurrentZoom);
                var h = (int)(img.Height * CurrentZoom);
                SelTexturePictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                SelTexturePictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                SelTexturePictureBox.Width = w;
                SelTexturePictureBox.Height = h;
                SelTexturePanel.AutoScroll = true;
            }

        }



        private void UpdateSaveYTDAs()
        {
            if (Ytd == null)
            {
                FileSaveMenu.Text = "Save YTD";
                FileSaveAsMenu.Text = "Save YTD As...";
                ToolbarSaveMenu.Text = "Save YTD";
                ToolbarSaveAsMenu.Text = "Save YTD As...";
                FileSaveMenu.Enabled = false;
                FileSaveAsMenu.Enabled = false;
                ToolbarSaveMenu.Enabled = false;
                ToolbarSaveAsMenu.Enabled = false;
            }
            else
            {
                var cansave = (ExploreForm?.EditMode ?? false);
                var s = "Save " + FileName;
                var sas = "Save " + FileName + " As...";
                FileSaveMenu.Text = s;
                FileSaveAsMenu.Text = sas;
                ToolbarSaveMenu.Text = s;
                ToolbarSaveAsMenu.Text = sas;
                FileSaveMenu.Enabled = cansave;
                FileSaveAsMenu.Enabled = true;
                ToolbarSaveMenu.Enabled = cansave;
                ToolbarSaveAsMenu.Enabled = true;
            }
        }

        private void UpdateSaveTextureAs()
        {
            if (CurrentTexture == null)
            {
                FileSaveTextureAsMenu.Text = "Save Texture As...";
                ToolbarSaveTextureAsMenu.Text = "Save Texture As...";
                FileSaveTextureAsMenu.Enabled = false;
                ToolbarSaveTextureAsMenu.Enabled = false;
            }
            else
            {
                string fname = CurrentTexture.Name + ".dds";
                string sas = "Save " + fname + " As...";
                FileSaveTextureAsMenu.Text = sas;
                ToolbarSaveTextureAsMenu.Text = sas;
                FileSaveTextureAsMenu.Enabled = true;
                ToolbarSaveTextureAsMenu.Enabled = true;
            }
        }



        private void SaveYTD(bool saveas = false)
        {
            if (Ytd == null) return;
            if (!(ExploreForm?.EditMode ?? false))
            {
                saveas = true;
            }

            var isinrpf = false;
            var rpfFileEntry = Ytd.RpfFileEntry;
            if (rpfFileEntry == null)
            {
                saveas = true;
            }
            else
            {
                if (rpfFileEntry?.Parent != null)
                {
                    if (!saveas)
                    {
                        isinrpf = true;
                        if (!rpfFileEntry.Path.ToLowerInvariant().StartsWith("mods"))
                        {
                            if (MessageBox.Show("This file is NOT located in the mods folder - Are you SURE you want to save this file?\r\nWARNING: This could cause permanent damage to your game!!!", "WARNING: Are you sure about this?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                            {
                                saveas = true;//that was a close one
                                isinrpf = false;
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(rpfFileEntry?.Path))
                {
                    isinrpf = false; //saving direct to filesystem in RPF explorer...
                }
                else
                {
                    saveas = true;//this probably shouldn't happen
                }
            }

            var data = Ytd.Save();

            if (saveas) //save direct to filesystem in external location
            {
                SaveYTDFileDialog.FileName = FileName;
                if (SaveYTDFileDialog.ShowDialog() != DialogResult.OK) return;
                string fpath = SaveYTDFileDialog.FileName;
                File.WriteAllBytes(fpath, data);
            }
            else if (!isinrpf) //save direct to filesystem in RPF explorer
            {
                File.WriteAllBytes(rpfFileEntry.Path, data);
                ExploreForm?.RefreshMainListViewInvoke(); //update the file details in explorer...
            }
            else //save to RPF...
            {
                if (!(ExploreForm?.EnsureRpfValidEncryption(rpfFileEntry.File) ?? false))
                {
                    MessageBox.Show("Unable to save file, RPF encryption needs to be OPEN for this operation!");
                    return;
                }

                Ytd.RpfFileEntry = RpfFile.CreateFile(rpfFileEntry.Parent, rpfFileEntry.Name, data);
                ExploreForm?.RefreshMainListViewInvoke(); //update the file details in explorer...
            }

            Modified = false;
            UpdateStatus("YTD file saved successfully at " + DateTime.Now.ToString());
            UpdateFormTitle();
        }

        private void SaveTextureAs()
        {
            if (CurrentTexture == null) return;
            string fname = CurrentTexture.Name + ".dds";
            SaveDDSFileDialog.FileName = fname;
            if (SaveDDSFileDialog.ShowDialog() != DialogResult.OK) return;
            string fpath = SaveDDSFileDialog.FileName;
            byte[] dds = DDSIO.GetDDSFile(CurrentTexture);
            File.WriteAllBytes(fpath, dds);
        }

        private void SaveAllTextures()
        {
            if (TexDict?.Textures?.data_items == null) return;
            if (FolderBrowserDialog.ShowDialogNew() != DialogResult.OK) return;
            var folder = FolderBrowserDialog.SelectedPath;
            foreach (var tex in TexDict.Textures.data_items)
            {
                byte[] dds = DDSIO.GetDDSFile(tex);
                string bpath = folder + "\\" + tex.Name;
                string fpath = bpath + ".dds";
                int c = 1;
                while (File.Exists(fpath))
                {
                    fpath = bpath + "_Copy" + c.ToString() + ".dds";
                    c++;
                }
                File.WriteAllBytes(fpath, dds);
            }
        }



        private void TexturesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            Texture tex = null;
            if (TexturesListView.SelectedItems.Count == 1)
            {
                tex = TexturesListView.SelectedItems[0].Tag as Texture;
            }
            ShowTextureMip(tex, 0, false);
        }

        private void SelTextureMipTrackBar_Scroll(object sender, EventArgs e)
        {
            Texture tex = null;
            if (TexturesListView.SelectedItems.Count == 1)
            {
                tex = TexturesListView.SelectedItems[0].Tag as Texture;
            }
            SelTextureMipLabel.Text = SelTextureMipTrackBar.Value.ToString();
            ShowTextureMip(tex, SelTextureMipTrackBar.Value, true);
        }

        private void SelTextureZoomCombo_TextChanged(object sender, EventArgs e)
        {
            string s = SelTextureZoomCombo.Text;
            if (s.EndsWith("%")) s = s.Substring(0, s.Length - 1);

            float f;
            if (!float.TryParse(s, out f))
            {
                CurrentZoom = 0.0f;
            }
            else
            {
                CurrentZoom = Math.Min(Math.Max(f, 0.0f), 5000.0f) * 0.01f;
            }
            UpdateZoom();
        }

        private void FileSaveMenu_Click(object sender, EventArgs e)
        {
            SaveYTD();
        }

        private void FileSaveAsMenu_Click(object sender, EventArgs e)
        {
            SaveYTD(true);
        }

        private void FileSaveTextureAsMenu_Click(object sender, EventArgs e)
        {
            SaveTextureAs();
        }

        private void FileSaveAllTexturesMenu_Click(object sender, EventArgs e)
        {
            SaveAllTextures();
        }

        private void SaveButton_ButtonClick(object sender, EventArgs e)
        {
            if (Ytd != null)
            {
                SaveYTD();
            }
            else
            {
                SaveTextureAs();
            }
        }

        private void ToolbarSaveMenu_Click(object sender, EventArgs e)
        {
            SaveYTD();
        }

        private void ToolbarSaveAsMenu_Click(object sender, EventArgs e)
        {
            SaveYTD(true);
        }

        private void ToolbarSaveTextureAsMenu_Click(object sender, EventArgs e)
        {
            SaveTextureAs();
        }

        private void ToolbarSaveAllTexturesMenu_Click(object sender, EventArgs e)
        {
            SaveAllTextures();
        }

        private void AddTextureButton_Click(object sender, EventArgs e)
        {
            AddTexture();
        }

        private void RemoveTextureButton_Click(object sender, EventArgs e)
        {
            RemoveTexture();
        }

        private void ReplaceTextureButton_Click(object sender, EventArgs e)
        {
            ReplaceTexture();
        }

        private void SelTextureNameTextBox_TextChanged(object sender, EventArgs e)
        {
            RenameTexture(SelTextureNameTextBox.Text);
        }
    }
}
