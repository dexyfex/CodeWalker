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


        private ExploreForm exploreForm = null;
        public RpfFileEntry rpfFileEntry { get; private set; } = null;
        private MetaFormat metaFormat = MetaFormat.XML;


        public MetaForm(ExploreForm owner)
        {
            exploreForm = owner;

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
            rpfFileEntry = null;

            return true;
        }
        private void NewDocument()
        {
            if (!CloseDocument()) return;

            FileName = "New.xml";
            rpfFileEntry = null;

            //TODO: decide XML/RSC/PSO/RBF format..?
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
            else if ((FilePath.ToLowerInvariant().StartsWith(GTAFolder.CurrentGTAFolder.ToLowerInvariant()))) saveAs = true;
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
                if (ymf.Meta != null) metaFormat = MetaFormat.RSC;
                if (ymf.Pso != null) metaFormat = MetaFormat.PSO;
                if (ymf.Rbf != null) metaFormat = MetaFormat.RBF;
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
                if (ymap.Meta != null) metaFormat = MetaFormat.RSC;
                if (ymap.Pso != null) metaFormat = MetaFormat.PSO;
                if (ymap.Rbf != null) metaFormat = MetaFormat.RBF;
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
                if (ytyp.Meta != null) metaFormat = MetaFormat.RSC;
                if (ytyp.Pso != null) metaFormat = MetaFormat.PSO;
                if (ytyp.Rbf != null) metaFormat = MetaFormat.RBF;
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
                if (jpso.Pso != null) metaFormat = MetaFormat.PSO;
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
                if (cut.Pso != null) metaFormat = MetaFormat.PSO;
            }
        }
        public void LoadMeta(CacheDatFile cachedat)
        {
            var fn = ((cachedat?.FileEntry?.Name) ?? "") + ".xml";
            Xml = cachedat.GetXml();
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



        public bool SaveMeta(XmlDocument doc)
        {
            //if explorer is in edit mode, and the current RpfFileEntry is valid, convert XML to the 
            //current meta format and then save the file into the RPF.
            //otherwise, save the generated file to disk? 
            //(currently just return false and revert to XML file save)

            if (!(exploreForm?.EditMode ?? false)) return false;
            if (rpfFileEntry?.Parent == null) return false;

            byte[] data = null;

#if !DEBUG
            try
#endif
            {
                switch (metaFormat)
                {
                    default:
                    case MetaFormat.XML: return false;//what are we even doing here?
                    case MetaFormat.RSC:
                        var meta = XmlMeta.GetMeta(doc);
                        if ((meta.DataBlocks?.Data == null) || (meta.DataBlocks.Count == 0))
                        {
                            MessageBox.Show("Schema not supported.", "Cannot import Meta XML");
                            return false;
                        }
                        data = ResourceBuilder.Build(meta, 2); //meta is RSC "Version":2    (it's actually a type identifier, not a version!)
                        break;
                    case MetaFormat.PSO:
                        var pso = XmlPso.GetPso(doc);
                        if ((pso.DataSection == null) || (pso.DataMapSection == null) || (pso.SchemaSection == null))
                        {
                            MessageBox.Show("Schema not supported.", "Cannot import PSO XML");
                            return false;
                        }
                        data = pso.Save();
                        break;
                    case MetaFormat.RBF:
                        MessageBox.Show("Sorry, RBF import is not supported.", "Cannot import RBF XML");
                        return false;
                    case MetaFormat.CacheFile:
                        MessageBox.Show("Sorry, CacheFile import is not supported.", "Cannot import CacheFile XML");
                        return false;
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageBox.Show("Exception encountered!\r\n" + ex.ToString(), "Cannot convert XML");
                return false;
            }
#endif
            if (data == null)
            {
                MessageBox.Show("Schema not supported. (Unspecified error - data was null!)", "Cannot convert XML");
                return false;
            }

            if (!rpfFileEntry.Path.ToLowerInvariant().StartsWith("mods"))
            {
                if (MessageBox.Show("This file is NOT located in the mods folder - Are you SURE you want to save this file?\r\nWARNING: This could cause permanent damage to your game!!!", "WARNING: Are you sure about this?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return false;//that was a close one
                }
            }

            try
            {
                if (!(exploreForm?.EnsureRpfValidEncryption(rpfFileEntry.File) ?? false)) return false;

                var newentry = RpfFile.CreateFile(rpfFileEntry.Parent, rpfFileEntry.Name, data);
                if (newentry != rpfFileEntry)
                { }
                rpfFileEntry = newentry;

                exploreForm?.RefreshMainListViewInvoke(); //update the file details in explorer...

                modified = false;

                return true; //victory!
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file to RPF! The RPF archive may be corrupted...\r\n" + ex.ToString(), "Really Bad Error");
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
