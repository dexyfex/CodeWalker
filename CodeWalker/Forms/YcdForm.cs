using CodeWalker.GameFiles;
using FastColoredTextBoxNS;
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
using Range = FastColoredTextBoxNS.Range;

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

        private bool LoadingXml = false;
        private bool DelayHighlight = false;


        public YcdForm()
        {
            InitializeComponent();

            MainListView.ContextMenuStrip = new ContextMenuStrip();
            MainListView.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Export to openFormats (.onim)...", null, ExportOnim_Click));
        }

        private void ExportOnim_Click(object sender, EventArgs e)
        {
            if (MainListView.SelectedItems[0].Tag is Animation anim)
            {
                var saveFileDialog = new SaveFileDialog();

                string newfn = $"{Path.GetFileNameWithoutExtension(Ycd.Name)}_{MainListView.SelectedItems[0].Text}.onim";

                saveFileDialog.FileName = newfn;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string path = saveFileDialog.FileName;

                    try
                    {
                        using (var file = File.OpenWrite(path))
                        {
                            Ycd.SaveOpenFormatsAnimation(anim, file);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving file " + path + ":\n" + ex.ToString());
                    }
                }
            }
        }

        private void UpdateFormTitle()
        {
            Text = fileName + " - Clip Dictionary Inspector - CodeWalker by dexyfex";
        }

        private void UpdateXmlTextBox(string xml)
        {
            LoadingXml = true;
            XmlTextBox.Text = "";
            XmlTextBox.Language = Language.XML;
            DelayHighlight = false;

            if (string.IsNullOrEmpty(xml))
            {
                LoadingXml = false;
                return;
            }
            //if (xml.Length > (1048576 * 5))
            //{
            //    XmlTextBox.Language = Language.Custom;
            //    XmlTextBox.Text = "[XML size > 10MB - Not shown due to performance limitations - Please use an external viewer for this file.]";
            //    return;
            //}
            //else 
            if (xml.Length > (1024 * 512))
            {
                XmlTextBox.Language = Language.Custom;
                DelayHighlight = true;
            }
            //else
            //{
            //    XmlTextBox.Language = Language.XML;
            //}


            Cursor = Cursors.WaitCursor;



            XmlTextBox.Text = xml;
            //XmlTextBox.IsChanged = false;
            XmlTextBox.ClearUndo();

            Cursor = Cursors.Default;
            LoadingXml = false;
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

        public void LoadXml()
        {
            if (Ycd != null)
            {
                var xml = YcdXml.GetXml(Ycd);
                UpdateXmlTextBox(xml);
            }
        }


        private void HTMLSyntaxHighlight(Range range)
        {
            try
            {
                Style BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
                Style RedStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
                Style MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);

                //clear style of changed range
                range.ClearStyle(BlueStyle, MaroonStyle, RedStyle);
                //tag brackets highlighting
                range.SetStyle(BlueStyle, @"<|/>|</|>");
                //tag name
                range.SetStyle(MaroonStyle, @"<(?<range>[!\w]+)");
                //end of tag
                range.SetStyle(MaroonStyle, @"</(?<range>\w+)>");
                //attributes
                range.SetStyle(RedStyle, @"(?<range>\S+?)='[^']*'|(?<range>\S+)=""[^""]*""|(?<range>\S+)=\S+");
                //attribute values
                range.SetStyle(BlueStyle, @"\S+?=(?<range>'[^']*')|\S+=(?<range>""[^""]*"")|\S+=(?<range>\S+)");
            }
            catch
            { }
        }

        private void XmlTextBox_VisibleRangeChangedDelayed(object sender, EventArgs e)
        {
            //this approach is much faster to load, but no outlining is available

            //highlight only visible area of text
            if (DelayHighlight)
            {
                HTMLSyntaxHighlight(XmlTextBox.VisibleRange);
            }
        }

        private void XmlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!LoadingXml)
            {

            }
        }

        private void MainListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainListView.SelectedItems.Count == 1)
            {
                MainPropertyGrid.SelectedObject = MainListView.SelectedItems[0].Tag;

                if (MainPropertyGrid.SelectedObject is Animation)
                {
                    MainListView.ContextMenuStrip.Items[0].Enabled = true;
                }
                else
                {
                    MainListView.ContextMenuStrip.Items[0].Enabled = false;
                }
            }
            else
            {
                MainListView.ContextMenuStrip.Items[0].Enabled = false;
                //MainPropertyGrid.SelectedObject = null;
            }
        }

        private void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainTabControl.SelectedTab == XmlTabPage)
            {
                if (string.IsNullOrEmpty(XmlTextBox.Text))
                {
                    LoadXml();
                }
            }
        }
    }
}