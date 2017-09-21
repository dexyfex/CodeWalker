using CodeWalker.GameFiles;
using CodeWalker.Properties;
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

namespace CodeWalker.Forms
{
    public partial class MetaForm : Form
    {
        private string xml;
        public string Xml
        {
            get { return xml; }
            set
            {
                xml = value;
                UpdateTextBoxFromData();
            }
        }

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

        private bool modified = false;
        private bool LoadingXml = false;
        private bool DelayHighlight = false;


        public MetaForm()
        {
            InitializeComponent();
        }


        private void UpdateFormTitle()
        {
            string ro = "";// " [Read-Only]";
            Text = fileName + " - Meta Editor" + ro + " - CodeWalker by dexyfex";
        }

        private void UpdateTextBoxFromData()
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


        private bool CloseDocument()
        {
            if (modified)
            {
                var res = MessageBox.Show("Do you want to save the current document before closing it?", "Save before closing", MessageBoxButtons.YesNoCancel);
                switch (res)
                {
                    case DialogResult.Yes:
                        SaveDocument();
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }

            FilePath = "";
            FileName = "";
            Xml = "";
            RawPropertyGrid.SelectedObject = null;
            modified = false;

            return true;
        }
        private void NewDocument()
        {
            if (!CloseDocument()) return; //same thing really..

            FileName = "New.xml";
        }
        private void OpenDocument()
        {
            if (OpenFileDialog.ShowDialog() != DialogResult.OK) return;

            if (!CloseDocument()) return;

            var fn = OpenFileDialog.FileName;

            if (!File.Exists(fn)) return; //couldn't find file?

            Xml = File.ReadAllText(fn);

            modified = false;
            FilePath = fn;
            FileName = new FileInfo(fn).Name;
            RawPropertyGrid.SelectedObject = null;
        }
        private void SaveDocument(bool saveAs = false)
        {
            if (string.IsNullOrEmpty(FileName)) saveAs = true;
            if (string.IsNullOrEmpty(FilePath)) saveAs = true;
            else if ((FilePath.ToLowerInvariant().StartsWith(Settings.Default.GTAFolder.ToLowerInvariant()))) saveAs = true;
            if (!File.Exists(FilePath)) saveAs = true;

            var fn = FilePath;
            if (saveAs)
            {
                if (!string.IsNullOrEmpty(fn))
                {
                    var dir = new FileInfo(fn).DirectoryName;
                    if (!Directory.Exists(dir)) dir = "";
                    SaveFileDialog.InitialDirectory = dir;
                }
                SaveFileDialog.FileName = FileName;
                if (SaveFileDialog.ShowDialog() != DialogResult.OK) return;
                fn = SaveFileDialog.FileName;
            }

            File.WriteAllText(fn, xml);

            modified = false;
            FilePath = fn;
            FileName = new FileInfo(fn).Name;
        }



        public void LoadMeta(YmtFile ymt)
        {
            var fn = (ymt?.RpfFileEntry?.Name) ?? "";
            if (ymt.Meta != null) { LoadMeta(ymt.Meta); fn += ".xml"; }
            else if (ymt.Pso != null) { LoadMeta(ymt.Pso); fn += ".pso.xml"; }
            else if (ymt.Rbf != null) { LoadMeta(ymt.Rbf); fn += ".rbf.xml"; }
            FileName = fn;
            RawPropertyGrid.SelectedObject = ymt;
        }
        public void LoadMeta(YmfFile ymf)
        {
            var fn = (ymf?.FileEntry?.Name) ?? "";
            if (ymf.Meta != null) { LoadMeta(ymf.Meta); fn += ".xml"; }
            else if (ymf.Pso != null) { LoadMeta(ymf.Pso); fn += ".pso.xml"; }
            else if (ymf.Rbf != null) { LoadMeta(ymf.Rbf); fn += ".rbf.xml"; }
            FileName = fn;
            RawPropertyGrid.SelectedObject = ymf;
        }
        public void LoadMeta(YmapFile ymap)
        {
            var fn = (ymap?.RpfFileEntry?.Name) ?? "";
            if (ymap.Meta != null) { LoadMeta(ymap.Meta); fn += ".xml"; }
            else if (ymap.Pso != null) { LoadMeta(ymap.Pso); fn += ".pso.xml"; }
            else if (ymap.Rbf != null) { LoadMeta(ymap.Rbf); fn += ".rbf.xml"; }
            FileName = fn;
            RawPropertyGrid.SelectedObject = ymap;
        }
        public void LoadMeta(YtypFile ytyp)
        {
            var fn = (ytyp?.FileEntry?.Name) ?? "";
            if (ytyp.Meta != null) { LoadMeta(ytyp.Meta); fn += ".xml"; }
            else if (ytyp.Pso != null) { LoadMeta(ytyp.Pso); fn += ".pso.xml"; }
            else if (ytyp.Rbf != null) { LoadMeta(ytyp.Rbf); fn += ".rbf.xml"; }
            FileName = fn;
            RawPropertyGrid.SelectedObject = ytyp;
        }
        public void LoadMeta(JPsoFile jpso)
        {
            var fn = jpso?.FileEntry?.Name ?? "";
            if (jpso.Pso != null) { LoadMeta(jpso.Pso); fn += ".pso.xml"; }
            FileName = fn;
            RawPropertyGrid.SelectedObject = jpso;
        }
        public void LoadMeta(CutFile cut)
        {
            var fn = cut?.FileEntry?.Name ?? "";
            if (cut.Pso != null) { LoadMeta(cut.Pso); fn += ".pso.xml"; }
            FileName = fn;
            RawPropertyGrid.SelectedObject = cut;
        }
        public void LoadMeta(Meta meta)
        {
            Xml = MetaXml.GetXml(meta);
            modified = false;
        }
        public void LoadMeta(PsoFile pso)
        {
            Xml = PsoXml.GetXml(pso);
            modified = false;
        }
        public void LoadMeta(RbfFile rbf)
        {
            Xml = RbfXml.GetXml(rbf);
            modified = false;
        }




        Style BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        Style RedStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
        Style MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);

        private void HTMLSyntaxHighlight(Range range)
        {
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


        private void XmlTextBox_VisibleRangeChangedDelayed(object sender, EventArgs e)
        {
            //this approach is much faster to load, but no outlining is available

            //highlight only visible area of text
            if (DelayHighlight)
            {
                HTMLSyntaxHighlight(XmlTextBox.VisibleRange);
            }
        }

        private void XmlTextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            if (!LoadingXml)
            {
                xml = XmlTextBox.Text;
                modified = true;
            }
        }

        private void NewButton_ButtonClick(object sender, EventArgs e)
        {
            NewDocument();
        }

        private void OpenButton_ButtonClick(object sender, EventArgs e)
        {
            OpenDocument();
        }

        private void SaveButton_ButtonClick(object sender, EventArgs e)
        {
            SaveDocument();
        }

        private void FileNewMenu_Click(object sender, EventArgs e)
        {
            NewDocument();
        }

        private void FileOpenMenu_Click(object sender, EventArgs e)
        {
            OpenDocument();
        }

        private void FileSaveMenu_Click(object sender, EventArgs e)
        {
            SaveDocument();
        }

        private void FileSaveAsMenu_Click(object sender, EventArgs e)
        {
            SaveDocument(true);
        }

        private void FileCloseMenu_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MetaForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseDocument();
        }
    }
}
