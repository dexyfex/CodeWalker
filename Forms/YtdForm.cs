using CodeWalker.GameFiles;
using CodeWalker.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
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

            for (int i = 0; i < texs.Length; i++)
            {
                var tex = texs[i];
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

        }



        private void UpdateFormTitle()
        {
            Text = fileName + " - Texture Dictionary - CodeWalker by dexyfex";
        }

        private void UpdateStatus(string text)
        {
            StatusLabel.Text = text;
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
    }
}
