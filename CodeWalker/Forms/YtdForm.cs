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
        private string fileName;
        private YtdFile Ytd { get; set; }
        private TextureDictionary TexDict { get; set; }
        private Texture CurrentTexture = null;
        private float CurrentZoom = 0.0f; //1.0 = 100%, 0.0 = stretch


        public YtdForm()
        {
            InitializeComponent();
        }


        public void LoadYtd(YtdFile ytd)
        {
            Ytd = ytd;

            fileName = ytd?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = ytd?.RpfFileEntry?.Name;
            }

            LoadTexDict(ytd.TextureDict, fileName);
        }
        public void LoadTexDict(TextureDictionary texdict, string filename)
        {
            TexDict = texdict;
            fileName = filename;

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
        }

        private string GetTexCountStr()
        {
            var texs = TexDict?.Textures?.data_items;
            if (texs == null) return "";
            return texs.Length.ToString() + " texture" + ((texs.Length != 1) ? "s" : "");
        }



        private void ShowTextureMip(Texture tex, int mip, bool mipchange)
        {
            CurrentTexture = tex;
            UpdateSaveAs();

            if (tex == null)
            {
                SelTexturePictureBox.Image = null;
                SelTextureNameTextBox.Text = string.Empty;
                SelTextureDimensionsLabel.Text = "-";
                SelTextureMipLabel.Text = "0";
                SelTextureMipTrackBar.Value = 0;
                SelTextureMipTrackBar.Maximum = 0;
                DetailsPropertyGrid.SelectedObject = null;
                UpdateStatus(GetTexCountStr());
                return;
            }


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



        private void UpdateFormTitle()
        {
            Text = fileName + " - Texture Dictionary - CodeWalker by dexyfex";
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





        private void UpdateSaveAs()
        {
            if (CurrentTexture == null)
            {
                FileSaveAsMenu.Text = "Save As...";
                ToolbarSaveAsMenu.Text = "Save As...";
                FileSaveAsMenu.Enabled = false;
                ToolbarSaveAsMenu.Enabled = false;
            }
            else
            {
                string fname = CurrentTexture.Name + ".dds";
                string sas = "Save " + fname + " As...";
                FileSaveAsMenu.Text = sas;
                ToolbarSaveAsMenu.Text = sas;
                FileSaveAsMenu.Enabled = true;
                ToolbarSaveAsMenu.Enabled = true;
            }
        }


        private void SaveAs()
        {
            if (CurrentTexture == null) return;
            string fname = CurrentTexture.Name + ".dds";
            SaveFileDialog.FileName = fname;
            if (SaveFileDialog.ShowDialog() != DialogResult.OK) return;
            string fpath = SaveFileDialog.FileName;
            byte[] dds = DDSIO.GetDDSFile(CurrentTexture);
            File.WriteAllBytes(fpath, dds);
        }

        private void SaveAll()
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

        private void FileSaveAllMenu_Click(object sender, EventArgs e)
        {
            SaveAll();
        }

        private void FileSaveAsMenu_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void SaveButton_ButtonClick(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void ToolbarSaveAsMenu_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void ToolbarSaveAllMenu_Click(object sender, EventArgs e)
        {
            SaveAll();
        }
    }
}
