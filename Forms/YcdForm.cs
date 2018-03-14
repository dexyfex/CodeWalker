using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Forms
{
    public partial class YcdForm : Form
    {

        YcdFile Ycd;

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                UpdateFormTitle();
            }
        }
        public string FilePath { get; set; }



        public YcdForm()
        {
            InitializeComponent();
        }



        private void UpdateFormTitle()
        {
            Text = fileName + " - Clip Dictionary Inspector - CodeWalker by dexyfex";
        }


        public void LoadYcd(YcdFile ycd)
        {
            Ycd = ycd;

            fileName = ycd?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = ycd?.RpfFileEntry?.Name;
            }

            UpdateFormTitle();

            //MainPropertyGrid.SelectedObject = ycd;

            MainListView.Items.Clear();

            if (ycd?.ClipMapEntries != null)
            {
                foreach (var cme in ycd.ClipMapEntries)
                {
                    if (cme != null)
                    {
                        var lvi = MainListView.Items.Add(cme.Clip?.ShortName ?? cme.Hash.ToString());
                        lvi.Tag = cme.Clip;
                        lvi.Group = MainListView.Groups["Clips"];
                    }
                }
            }

            if (ycd?.AnimMapEntries != null)
            {
                foreach (var ame in ycd.AnimMapEntries)
                {
                    if (ame != null)
                    {
                        var lvi = MainListView.Items.Add(ame.Hash.ToString());
                        lvi.Tag = ame.Animation;
                        lvi.Group = MainListView.Groups["Anims"];
                    }
                }
            }


        }



        private void MainListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainListView.SelectedItems.Count == 1)
            {
                MainPropertyGrid.SelectedObject = MainListView.SelectedItems[0].Tag;
            }
            else
            {
                //MainPropertyGrid.SelectedObject = null;
            }
        }
    }
}
