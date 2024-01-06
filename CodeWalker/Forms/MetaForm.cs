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
using System.Xml;
using Range = FastColoredTextBoxNS.Range;

namespace CodeWalker.Forms
{
    public partial class MetaForm : Form
    {
        private string xml;
        public string Xml
        {
            get => xml;
            set
            {
                xml = value;
                UpdateTextBoxFromData();
            }
        }

        private string fileName;
        public string FileName
        {
            get => fileName;
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

        public RpfFileEntry? rpfFileEntry { get; private set; } = null;
        private MetaFormat metaFormat = MetaFormat.XML;


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
            if (InvokeRequired)
            {
                BeginInvoke(UpdateTextBoxFromData);
                return;
            }
            LoadingXml = true;
            Cursor = Cursors.WaitCursor;
            try
            {
                XmlTextBox.BeginUpdate();
                XmlTextBox.Clear();
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

                XmlTextBox.Text = xml;
                //XmlTextBox.IsChanged = false;
                XmlTextBox.ClearUndo();
            }
            finally
            {
                Cursor = Cursors.Default;
                LoadingXml = false;
                XmlTextBox.EndUpdate();
            }
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
            rpfFileEntry = null;

            return true;
        }
        private void NewDocument()
        {
            if (!CloseDocument())
                return;

            FileName = "New.xml";
            rpfFileEntry = null;

            //TODO: decide XML/RSC/PSO/RBF format..?
        }
        private async ValueTask OpenDocument()
        {
            if (OpenFileDialog.ShowDialog() != DialogResult.OK)
                return;

            if (!CloseDocument())
                return;

            var fn = OpenFileDialog.FileName;

            if (!File.Exists(fn))
                return; //couldn't find file?

            Xml = await File.ReadAllTextAsync(fn);

            modified = false;
            FilePath = fn;
            FileName = new FileInfo(fn).Name;
            RawPropertyGrid.SelectedObject = null;
            rpfFileEntry = null;

            //TODO: open RSC/PSO/RBF..?
        }
        private void SaveDocument(bool saveAs = false)
        {
            if ((metaFormat != MetaFormat.XML) && (saveAs == false))
            {
                var doc = new XmlDocument();
                try
                {
                    doc.LoadXml(xml);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There's something wrong with your XML document:\r\n" + ex.Message, "Unable to parse XML");
                    return;
                }
                if (SaveMeta(doc))
                {
                    return;
                }
                //if Meta saving failed for whatever reason, fallback to saving the XML in the filesystem.
                saveAs = true;
            }

            if (string.IsNullOrEmpty(FileName)) saveAs = true;
            if (string.IsNullOrEmpty(FilePath)) saveAs = true;
            else if (FilePath.StartsWith(GTAFolder.CurrentGTAFolder, StringComparison.OrdinalIgnoreCase)) saveAs = true;
            if (!File.Exists(FilePath)) saveAs = true;

            var fn = FilePath;
            if (saveAs)
            {
                if (!string.IsNullOrEmpty(fn))
                {
                    var dir = new FileInfo(fn).DirectoryName;
                    if (!Directory.Exists(dir))
                        dir = "";
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
            metaFormat = MetaFormat.XML;
        }



        public void LoadMeta(YmtFile ymt)
        {
            string fn;
            Xml = MetaXml.GetXml(ymt, out fn);
            FileName = fn;
            RawPropertyGrid.SelectedObject = ymt;
            rpfFileEntry = ymt?.RpfFileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (ymt != null)
            {
                if (ymt.Meta != null) metaFormat = MetaFormat.RSC;
                if (ymt.Pso != null) metaFormat = MetaFormat.PSO;
                if (ymt.Rbf != null) metaFormat = MetaFormat.RBF;
            }
        }
        public void LoadMeta(YmfFile ymf)
        {
            string fn;
            Xml = MetaXml.GetXml(ymf, out fn);
            FileName = fn;
            RawPropertyGrid.SelectedObject = ymf;
            rpfFileEntry = ymf?.FileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (ymf != null)
            {
                if (ymf.Meta != null)
                    metaFormat = MetaFormat.RSC;
                if (ymf.Pso != null)
                    metaFormat = MetaFormat.PSO;
                if (ymf.Rbf != null)
                    metaFormat = MetaFormat.RBF;
            }
        }
        public void LoadMeta(YmapFile ymap)
        {
            string fn;
            Xml = MetaXml.GetXml(ymap, out fn);
            FileName = fn;
            RawPropertyGrid.SelectedObject = ymap;
            rpfFileEntry = ymap?.RpfFileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (ymap != null)
            {
                if (ymap.Meta != null)
                    metaFormat = MetaFormat.RSC;
                if (ymap.Pso != null)
                    metaFormat = MetaFormat.PSO;
                if (ymap.Rbf != null)
                    metaFormat = MetaFormat.RBF;
            }
        }
        public void LoadMeta(YtypFile ytyp)
        {
            string fn;
            Xml = MetaXml.GetXml(ytyp, out fn);
            FileName = fn;
            RawPropertyGrid.SelectedObject = ytyp;
            rpfFileEntry = ytyp?.RpfFileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (ytyp != null)
            {
                if (ytyp.Meta != null)
                    metaFormat = MetaFormat.RSC;
                if (ytyp.Pso != null)
                    metaFormat = MetaFormat.PSO;
                if (ytyp.Rbf != null)
                    metaFormat = MetaFormat.RBF;
            }
        }
        public void LoadMeta(JPsoFile jpso)
        {
            string fn;
            Xml = MetaXml.GetXml(jpso, out fn);
            FileName = fn;
            RawPropertyGrid.SelectedObject = jpso;
            rpfFileEntry = jpso?.FileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (jpso != null)
            {
                if (jpso.Pso != null)
                    metaFormat = MetaFormat.PSO;
            }
        }
        public void LoadMeta(CutFile cut)
        {
            string fn;
            Xml = MetaXml.GetXml(cut, out fn);
            FileName = fn;
            RawPropertyGrid.SelectedObject = cut;
            rpfFileEntry = cut?.FileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (cut != null)
            {
                if (cut.Pso != null)
                    metaFormat = MetaFormat.PSO;
            }
        }
        public void LoadMeta(YndFile ynd)
        {
            var fn = ((ynd?.RpfFileEntry?.Name) ?? "") + ".xml";
            Xml = MetaXml.GetXml(ynd, out fn);
            FileName = fn;
            RawPropertyGrid.SelectedObject = ynd;
            rpfFileEntry = ynd?.RpfFileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (ynd?.RpfFileEntry != null)
            {
                metaFormat = MetaFormat.Ynd;
            }
        }
        public void LoadMeta(YldFile yld)
        {
            var fn = ((yld?.RpfFileEntry?.Name) ?? "") + ".xml";
            Xml = MetaXml.GetXml(yld, out fn);
            FileName = fn;
            RawPropertyGrid.SelectedObject = yld;
            rpfFileEntry = yld?.RpfFileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (yld?.RpfFileEntry != null)
            {
                metaFormat = MetaFormat.Yld;
            }
        }
        public void LoadMeta(YedFile yed)
        {
            var fn = ((yed?.RpfFileEntry?.Name) ?? "") + ".xml";
            Xml = MetaXml.GetXml(yed, out fn);
            FileName = fn;
            RawPropertyGrid.SelectedObject = yed;
            rpfFileEntry = yed?.RpfFileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (yed?.RpfFileEntry != null)
            {
                metaFormat = MetaFormat.Yed;
            }
        }
        public void LoadMeta(CacheDatFile cachedat)
        {
            var fn = ((cachedat?.FileEntry?.Name) ?? "") + ".xml";
            Xml = MetaXml.GetXml(cachedat, out fn, "");
            FileName = fn;
            RawPropertyGrid.SelectedObject = cachedat;
            rpfFileEntry = cachedat?.FileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (cachedat?.FileEntry != null)
            {
                metaFormat = MetaFormat.CacheFile;
            }
        }
        public void LoadMeta(HeightmapFile heightmap)
        {
            var fn = ((heightmap?.RpfFileEntry?.Name) ?? "") + ".xml";
            Xml = HmapXml.GetXml(heightmap);
            FileName = fn;
            RawPropertyGrid.SelectedObject = heightmap;
            rpfFileEntry = heightmap?.RpfFileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (heightmap?.RpfFileEntry != null)
            {
                metaFormat = MetaFormat.Heightmap;
            }
        }
        public void LoadMeta(YpdbFile ypdb)
        {
            var fn = ((ypdb?.RpfFileEntry?.Name) ?? "") + ".xml";
            Xml = MetaXml.GetXml(ypdb, out fn);
            FileName = fn;
            RawPropertyGrid.SelectedObject = ypdb;
            rpfFileEntry = ypdb?.RpfFileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (ypdb?.RpfFileEntry != null)
            {
                metaFormat = MetaFormat.Ypdb;
            }
        }
        public void LoadMeta(YfdFile yfd)
        {
            var fn = ((yfd?.RpfFileEntry?.Name) ?? "") + ".xml";
            Xml = MetaXml.GetXml(yfd, out fn);
            FileName = fn;
            RawPropertyGrid.SelectedObject = yfd;
            rpfFileEntry = yfd?.RpfFileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (yfd?.RpfFileEntry != null)
            {
                metaFormat = MetaFormat.Yfd;
            }
        }
        public void LoadMeta(MrfFile mrf)
        {
            var fn = ((mrf?.RpfFileEntry?.Name) ?? "") + ".xml";
            Xml = MrfXml.GetXml(mrf);
            FileName = fn;
            RawPropertyGrid.SelectedObject = mrf;
            rpfFileEntry = mrf?.RpfFileEntry;
            modified = false;
            metaFormat = MetaFormat.XML;
            if (mrf?.RpfFileEntry != null)
            {
                metaFormat = MetaFormat.Mrf;
            }
        }

        public void LoadMeta(PackedFile gameFile)
        {
            ArgumentNullException.ThrowIfNull(gameFile, nameof(gameFile));

            if (gameFile is YmfFile ymfFile)
            {
                LoadMeta(ymfFile);
            }
            else if (gameFile is MrfFile mrfFile)
            {
                LoadMeta(mrfFile);
            }
            else if (gameFile is YfdFile yfdFile)
            {
                LoadMeta(yfdFile);
            }
            else if (gameFile is YpdbFile ypdbFile)
            {
                LoadMeta(ypdbFile);
            }
            else if (gameFile is HeightmapFile heightmap)
            {
                LoadMeta(heightmap);
            }
            else if (gameFile is CacheDatFile cacheDatFile)
            {
                LoadMeta(cacheDatFile);
            }
            else if (gameFile is YedFile yedFile)
            {
                LoadMeta(yedFile);
            }
            else if (gameFile is YldFile yldFile)
            {
                LoadMeta(yldFile);
            }
            else if (gameFile is YndFile yndFile)
            {
                LoadMeta(yndFile);
            }
            else if (gameFile is CutFile cutFile)
            {
                LoadMeta(cutFile);
            }
            else if (gameFile is JPsoFile jpsoFile)
            {
                LoadMeta(jpsoFile);
            }
            else if (gameFile is YtypFile ytypFile)
            {
                LoadMeta(ytypFile);
            }
            else if (gameFile is YmapFile ymapFile)
            {
                LoadMeta(ymapFile);
            }
            else if (gameFile is YmtFile ymtFile)
            {
                LoadMeta(ymtFile);
            }
        }



        public bool SaveMeta(XmlDocument doc)
        {
            //if explorer is in edit mode, and the current RpfFileEntry is valid, convert XML to the 
            //current meta format and then save the file into the RPF.
            //otherwise, save the generated file to disk? 
            //(currently just return false and revert to XML file save)

            if (!(ExploreForm.Instance?.EditMode ?? false))
                return false;

            if(metaFormat == MetaFormat.XML)
                return false;//what are we even doing here?

            byte[] data;

            try
            {

                data = XmlMeta.GetData(doc, metaFormat, string.Empty);

                if (data.Length == 0)
                {
                    MessageBox.Show("Schema not supported.", "Cannot import " + XmlMeta.GetXMLFormatName(metaFormat));
                    return false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception encountered!\r\n" + ex.ToString(), "Cannot convert XML");
                Console.WriteLine(ex);
                return false;
            }

            if (rpfFileEntry?.Parent != null)
            {
                if (!rpfFileEntry.Path.StartsWith("mods", StringComparison.OrdinalIgnoreCase))
                {
                    if (MessageBox.Show("This file is NOT located in the mods folder - Are you SURE you want to save this file?\r\nWARNING: This could cause permanent damage to your game!!!", "WARNING: Are you sure about this?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        return false;//that was a close one
                    }
                }

                try
                {
                    if (!ExploreForm.EnsureRpfValidEncryption(rpfFileEntry.File))
                        return false;

                    var newentry = RpfFile.CreateFile(rpfFileEntry.Parent, rpfFileEntry.Name, data);
                    rpfFileEntry = newentry;

                    ExploreForm.RefreshMainListViewInvoke(); //update the file details in explorer...

                    modified = false;

                    StatusLabel.Text = metaFormat.ToString() + " file saved successfully at " + DateTime.Now.ToString();

                    return true; //victory!
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    MessageBox.Show("Error saving file to RPF! The RPF archive may be corrupted...\r\n" + ex.ToString(), "Really Bad Error");
                }
            }
            else if (!string.IsNullOrEmpty(rpfFileEntry?.Path))
            {
                try
                {
                    File.WriteAllBytes(rpfFileEntry.Path, data);

                    ExploreForm.RefreshMainListViewInvoke(); //update the file details in explorer...

                    modified = false;

                    StatusLabel.Text = metaFormat.ToString() + " file saved successfully at " + DateTime.Now.ToString();

                    return true; //victory!
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    MessageBox.Show("Error saving file to filesystem!\r\n" + ex.ToString(), "File I/O Error");
                }
            }

            return false;
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

        private async void OpenButton_ButtonClick(object sender, EventArgs e)
        {
            await OpenDocument();
        }

        private void SaveButton_ButtonClick(object sender, EventArgs e)
        {
            SaveDocument();
        }

        private void FileNewMenu_Click(object sender, EventArgs e)
        {
            NewDocument();
        }

        private async void FileOpenMenu_Click(object sender, EventArgs e)
        {
            await OpenDocument();
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
