using CodeWalker.GameFiles;
using CodeWalker.Properties;
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

namespace CodeWalker
{
    public partial class BinarySearchForm : Form
    {
        private volatile bool InProgress = false;
        private volatile bool AbortOperation = false;

        private GameFileCache FileCache = null;
        private RpfManager RpfMan = null;


        public BinarySearchForm(GameFileCache cache = null)
        {
            FileCache = cache;
            RpfMan = cache?.RpfMan;
            InitializeComponent();
        }

        private void BinarySearchForm_Load(object sender, EventArgs e)
        {
            FileSearchFolderTextBox.Text = Settings.Default.CompiledScriptFolder;

            DataHexLineCombo.Text = "16";
            DataTextBox.SetTabStopWidth(3);


            if (RpfMan == null)
            {
                Task.Run(() =>
                {
                    GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
                    RpfMan = new RpfManager();
                    RpfMan.Init(GTAFolder.CurrentGTAFolder, UpdateStatus, UpdateStatus, false, false);
                    RPFScanComplete();
                });
            }
            else
            {
                RPFScanComplete();
            }
        }





        private void UpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { UpdateStatus(text); }));
                }
                else
                {
                    StatusLabel.Text = text;
                }
            }
            catch { }
        }
        private void RPFScanComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { RPFScanComplete(); }));
                }
                else
                {
                    StatusLabel.Text = "Ready";
                    //RpfSearchPanel.Enabled = true;
                }
            }
            catch { }
        }







        private void FileSearchFolderBrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog.SelectedPath = FileSearchFolderTextBox.Text;
            DialogResult res = FolderBrowserDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                FileSearchFolderTextBox.Text = FolderBrowserDialog.SelectedPath;
            }
        }

        private void FileSearchButton_Click(object sender, EventArgs e)
        {
            string searchtxt = FileSearchTextBox.Text;
            string searchfolder = FileSearchFolderTextBox.Text;
            AbortOperation = false;

            if (InProgress) return;
            if (searchfolder.Length == 0)
            {
                MessageBox.Show("Please select a folder...");
                return;
            }
            if (!Directory.Exists(searchfolder))
            {
                MessageBox.Show("Please select a valid folder!");
                return;
            }

            FileSearchResultsTextBox.Clear();

            byte[] searchbytes1;
            byte[] searchbytes2;
            int bytelen;

            if (FileSearchHexRadio.Checked)
            {
                try
                {
                    bytelen = searchtxt.Length / 2;
                    searchbytes1 = new byte[bytelen];
                    searchbytes2 = new byte[bytelen];
                    for (int i = 0; i < bytelen; i++)
                    {
                        searchbytes1[i] = Convert.ToByte(searchtxt.Substring(i * 2, 2), 16);
                        searchbytes2[bytelen - i - 1] = searchbytes1[i];
                    }
                }
                catch
                {
                    MessageBox.Show("Please enter a valid hex string.");
                    return;
                }
            }
            else
            {
                bytelen = searchtxt.Length;
                searchbytes1 = new byte[bytelen];
                searchbytes2 = new byte[bytelen];
                for (int i = 0; i < bytelen; i++)
                {
                    searchbytes1[i] = (byte)searchtxt[i];
                    searchbytes2[bytelen - i - 1] = searchbytes1[i];
                }
            }

            FileSearchPanel.Enabled = false;

            InProgress = true;

            Task.Run(() =>
            {

                FileSearchAddResult("Searching " + searchfolder + "...");

                string[] filenames = Directory.GetFiles(searchfolder);

                int matchcount = 0;

                foreach (string filename in filenames)
                {


                    FileInfo finf = new FileInfo(filename);
                    byte[] filebytes = File.ReadAllBytes(filename);

                    int hitlen1 = 0;
                    int hitlen2 = 0;

                    for (int i = 0; i < filebytes.Length; i++)
                    {
                        byte b = filebytes[i];
                        byte b1 = searchbytes1[hitlen1]; //current test byte 1
                        byte b2 = searchbytes2[hitlen2];

                        if (b == b1) hitlen1++; else hitlen1 = 0;
                        if (b == b2) hitlen2++; else hitlen2 = 0;

                        if (hitlen1 == bytelen)
                        {
                            FileSearchAddResult(finf.Name + ":" + (i - bytelen));
                            matchcount++;
                            hitlen1 = 0;
                        }
                        if (hitlen2 == bytelen)
                        {
                            FileSearchAddResult(finf.Name + ":" + (i - bytelen));
                            matchcount++;
                            hitlen2 = 0;
                        }

                        if (AbortOperation)
                        {
                            FileSearchAddResult("Search aborted.");
                            FileSearchComplete();
                            InProgress = false;
                            return;
                        }

                    }

                }

                FileSearchAddResult(string.Format("Search complete. {0} results found.", matchcount));
                FileSearchComplete();
                InProgress = false;
            });
        }

        private void FileSearchAddResult(string result)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { FileSearchAddResult(result); }));
                }
                else
                {
                    FileSearchResultsTextBox.AppendText(result + "\r\n");
                }
            }
            catch { }
        }

        private void FileSearchComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { FileSearchComplete(); }));
                }
                else
                {
                    FileSearchPanel.Enabled = true;
                }
            }
            catch { }
        }

        private void FileSearchAbortButton_Click(object sender, EventArgs e)
        {
            AbortOperation = true;
        }












        private List<RpfSearchResult> RpfSearchResults = new List<RpfSearchResult>();
        private RpfEntry RpfSelectedEntry = null;
        private int RpfSelectedOffset = -1;
        private int RpfSelectedLength = 0;

        private class RpfSearchResult
        {
            public RpfFileEntry FileEntry { get; set; }
            public int Offset { get; set; }
            public int Length { get; set; }

            public RpfSearchResult(RpfFileEntry entry, int offset, int length)
            {
                FileEntry = entry;
                Offset = offset;
                Length = length;
            }
        }
        private byte LowerCaseByte(byte b)
        {
            if ((b >= 65) && (b <= 90)) //upper case alphabet...
            {
                b += 32;
            }
            return b;
        }

        private void RpfSearchAddResult(RpfSearchResult result)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { RpfSearchAddResult(result); }));
                }
                else
                {
                    RpfSearchResults.Add(result);
                    RpfSearchResultsListView.VirtualListSize = RpfSearchResults.Count;
                }
            }
            catch { }
        }

        private void RpfSearch()
        {
            if (InProgress) return;
            if (!(RpfMan?.IsInited ?? false))
            {
                MessageBox.Show("Please wait for the scan to complete.");
                return;
            }
            if (RpfSearchTextBox.Text.Length == 0)
            {
                MessageBox.Show("Please enter a search term.");
                return;
            }

            string searchtxt = RpfSearchTextBox.Text;
            bool hex = RpfSearchHexRadioButton.Checked;
            bool casesen = RpfSearchCaseSensitiveCheckBox.Checked || hex;
            bool bothdirs = RpfSearchBothDirectionsCheckBox.Checked;
            string[] ignoreexts = null;
            string[] onlyexts = null;
            byte[] searchbytes1;
            byte[] searchbytes2;
            int bytelen;

            if (!casesen) searchtxt = searchtxt.ToLowerInvariant(); //case sensitive search in lower case.

            if (RpfSearchIgnoreCheckBox.Checked)
            {
                ignoreexts = RpfSearchIgnoreTextBox.Text.Split(',');
                for (int i = 0; i < ignoreexts.Length; i++)
                {
                    ignoreexts[i] = ignoreexts[i].Trim();
                }
            }
            if (RpfSearchOnlyCheckBox.Checked)
            {
                onlyexts = RpfSearchOnlyTextBox.Text.Split(',');
                for (int i = 0; i < onlyexts.Length; i++)
                {
                    onlyexts[i] = onlyexts[i].Trim();
                }
            }

            if (hex)
            {
                if (searchtxt.Length < 2)
                {
                    MessageBox.Show("Please enter at least one byte of hex (2 characters).");
                    return;
                }
                try
                {
                    bytelen = searchtxt.Length / 2;
                    searchbytes1 = new byte[bytelen];
                    searchbytes2 = new byte[bytelen];
                    for (int i = 0; i < bytelen; i++)
                    {
                        searchbytes1[i] = Convert.ToByte(searchtxt.Substring(i * 2, 2), 16);
                        searchbytes2[bytelen - i - 1] = searchbytes1[i];
                    }
                }
                catch
                {
                    MessageBox.Show("Please enter a valid hex string.");
                    return;
                }
            }
            else
            {
                bytelen = searchtxt.Length;
                searchbytes1 = new byte[bytelen];
                searchbytes2 = new byte[bytelen]; //reversed text...
                for (int i = 0; i < bytelen; i++)
                {
                    searchbytes1[i] = (byte)searchtxt[i];
                    searchbytes2[bytelen - i - 1] = searchbytes1[i];
                }
            }

            RpfSearchTextBox.Enabled = false;
            RpfSearchHexRadioButton.Enabled = false;
            RpfSearchTextRadioButton.Enabled = false;
            RpfSearchCaseSensitiveCheckBox.Enabled = false;
            RpfSearchBothDirectionsCheckBox.Enabled = false;
            RpfSearchIgnoreCheckBox.Enabled = false;
            RpfSearchIgnoreTextBox.Enabled = false;
            RpfSearchButton.Enabled = false;
            RpfSearchSaveResultsButton.Enabled = false;

            InProgress = true;
            AbortOperation = false;
            RpfSearchResultsListView.VirtualListSize = 0;
            RpfSearchResults.Clear();
            uint totfiles = 0;
            uint curfile = 0;
            var scannedFiles = RpfMan.AllRpfs;
            Task.Run(() =>
            {

                DateTime starttime = DateTime.Now;
                int resultcount = 0;

                for (int f = 0; f < scannedFiles.Count; f++)
                {
                    var rpffile = scannedFiles[f];
                    totfiles += rpffile.TotalFileCount;
                }


                for (int f = 0; f < scannedFiles.Count; f++)
                {
                    var rpffile = scannedFiles[f];

                    foreach (var entry in rpffile.AllEntries)
                    {
                        var duration = DateTime.Now - starttime;
                        if (AbortOperation)
                        {
                            UpdateStatus(duration.ToString(@"hh\:mm\:ss") + " - Search aborted.");
                            InProgress = false;
                            RpfSearchComplete();
                            return;
                        }

                        RpfFileEntry fentry = entry as RpfFileEntry;
                        if (fentry == null) continue;

                        curfile++;

                        if (fentry.NameLower.EndsWith(".rpf"))
                        { continue; }

                        if (onlyexts != null)
                        {
                            bool ignore = true;
                            for (int i = 0; i < onlyexts.Length; i++)
                            {
                                if (fentry.NameLower.EndsWith(onlyexts[i]))
                                {
                                    ignore = false;
                                    break;
                                }
                            }
                            if (ignore)
                            { continue; }
                        }

                        if (ignoreexts != null)
                        {
                            bool ignore = false;
                            for (int i = 0; i < ignoreexts.Length; i++)
                            {
                                if (fentry.NameLower.EndsWith(ignoreexts[i]))
                                {
                                    ignore = true;
                                    break;
                                }
                            }
                            if (ignore)
                            { continue; }
                        }

                        UpdateStatus(string.Format("{0} - Searching {1}/{2} : {3}", duration.ToString(@"hh\:mm\:ss"), curfile, totfiles, fentry.Path));

                        byte[] filebytes = fentry.File.ExtractFile(fentry);
                        if (filebytes == null) continue;


                        int hitlen1 = 0;
                        int hitlen2 = 0;

                        for (int i = 0; i < filebytes.Length; i++)
                        {
                            byte b = casesen ? filebytes[i] : LowerCaseByte(filebytes[i]);
                            byte b1 = searchbytes1[hitlen1]; //current test byte 1
                            byte b2 = searchbytes2[hitlen2];

                            if (b == b1) hitlen1++; else hitlen1 = 0;
                            if (hitlen1 == bytelen)
                            {
                                RpfSearchAddResult(new RpfSearchResult(fentry, (i - bytelen), bytelen));
                                resultcount++;
                                hitlen1 = 0;
                            }
                            if (bothdirs)
                            {
                                if (b == b2) hitlen2++; else hitlen2 = 0;
                                if (hitlen2 == bytelen)
                                {
                                    RpfSearchAddResult(new RpfSearchResult(fentry, (i - bytelen), bytelen));
                                    resultcount++;
                                    hitlen2 = 0;
                                }
                            }
                        }
                    }
                }

                var totdur = DateTime.Now - starttime;
                UpdateStatus(totdur.ToString(@"hh\:mm\:ss") + " - Search complete. " + resultcount.ToString() + " results found.");
                InProgress = false;
                RpfSearchComplete();
            });
        }
        private void RpfSearchComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { RpfSearchComplete(); }));
                }
                else
                {
                    RpfSearchTextBox.Enabled = true;
                    RpfSearchHexRadioButton.Enabled = true;
                    RpfSearchTextRadioButton.Enabled = true;
                    RpfSearchCaseSensitiveCheckBox.Enabled = RpfSearchTextRadioButton.Checked;
                    RpfSearchBothDirectionsCheckBox.Enabled = true;
                    RpfSearchIgnoreCheckBox.Enabled = true;
                    RpfSearchIgnoreTextBox.Enabled = RpfSearchIgnoreCheckBox.Checked;
                    RpfSearchButton.Enabled = true;
                    RpfSearchSaveResultsButton.Enabled = true;
                }
            }
            catch { }
        }

        private void RpfSearchButton_Click(object sender, EventArgs e)
        {
            RpfSearch();
        }

        private void RpfSearchAbortButton_Click(object sender, EventArgs e)
        {
            AbortOperation = true;
        }

        private void RpfSearchSaveResultsButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog.FileName = "SearchResults.txt";
            if (SaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fpath = SaveFileDialog.FileName;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("CodeWalker Search Results for \"" + RpfSearchTextBox.Text + "\"");
                sb.AppendLine("[File path], [Byte offset]");
                if (RpfSearchResults != null)
                {
                    foreach (var r in RpfSearchResults)
                    {
                        sb.AppendLine(r.FileEntry.Path + ", " + r.Offset.ToString());
                    }
                }

                File.WriteAllText(fpath, sb.ToString());

            }
        }

        private void RpfSearchIgnoreCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RpfSearchIgnoreTextBox.Enabled = RpfSearchIgnoreCheckBox.Checked;
            if (RpfSearchIgnoreCheckBox.Checked)
            {
                RpfSearchOnlyCheckBox.Checked = false;
            }
        }

        private void RpfSearchOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RpfSearchOnlyTextBox.Enabled = RpfSearchOnlyCheckBox.Checked;
            if (RpfSearchOnlyCheckBox.Checked)
            {
                RpfSearchIgnoreCheckBox.Checked = false;
            }
        }

        private void RpfSearchTextRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RpfSearchCaseSensitiveCheckBox.Enabled = RpfSearchTextRadioButton.Checked;
        }

        private void RpfSearchResultsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RpfSearchResultsListView.SelectedIndices.Count == 1)
            {
                var i = RpfSearchResultsListView.SelectedIndices[0];
                if ((i >= 0) && (i < RpfSearchResults.Count))
                {
                    var r = RpfSearchResults[i];
                    SelectFile(r.FileEntry, r.Offset + 1, r.Length);
                }
                else
                {
                    SelectFile(null, -1, 0);
                }
            }
            else
            {
                SelectFile(null, -1, 0);
            }
        }

        private void RpfSearchResultsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var item = new ListViewItem();
            if (e.ItemIndex < RpfSearchResults.Count)
            {
                RpfSearchResult r = RpfSearchResults[e.ItemIndex];
                item.Text = r.FileEntry.Name;
                item.SubItems.Add(r.Offset.ToString());
                item.Tag = r;
            }
            e.Item = item;
        }





        private void SelectFile()
        {
            SelectFile(RpfSelectedEntry, RpfSelectedOffset, RpfSelectedLength);
        }
        private void SelectFile(RpfEntry entry, int offset, int length)
        {
            RpfSelectedEntry = entry;
            RpfSelectedOffset = offset;
            RpfSelectedLength = length;

            RpfFileEntry rfe = entry as RpfFileEntry;
            if (rfe == null)
            {
                RpfDirectoryEntry rde = entry as RpfDirectoryEntry;
                if (rde != null)
                {
                    FileInfoLabel.Text = rde.Path + " (Directory)";
                    DataTextBox.Text = "[Please select a data file]";
                }
                else
                {
                    FileInfoLabel.Text = "[Nothing selected]";
                    DataTextBox.Text = "[Please select a search result]";
                }
                return;
            }


            Cursor = Cursors.WaitCursor;

            string typestr = "Resource";
            if (rfe is RpfBinaryFileEntry)
            {
                typestr = "Binary";
            }

            byte[] data = rfe.File.ExtractFile(rfe);

            int datalen = (data != null) ? data.Length : 0;
            FileInfoLabel.Text = rfe.Path + " (" + typestr + " file)  -  " + TextUtil.GetBytesReadable(datalen);


            if (ShowLargeFileContentsCheckBox.Checked || (datalen < 524287)) //512K
            {
                DisplayFileContentsText(rfe, data, length, offset);
            }
            else
            {
                DataTextBox.Text = "[Filesize >512KB. Select the Show large files option to view its contents]";
            }



            //bool istexdict = false;
            //if (rfe.NameLower.EndsWith(".ymap"))
            //{
            //    YmapFile ymap = new YmapFile(rfe);
            //    ymap.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = ymap;
            //}
            //else if (rfe.NameLower.EndsWith(".ytyp"))
            //{
            //    YtypFile ytyp = new YtypFile();
            //    ytyp.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = ytyp;
            //}
            //else if (rfe.NameLower.EndsWith(".ymf"))
            //{
            //    YmfFile ymf = new YmfFile();
            //    ymf.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = ymf;
            //}
            //else if (rfe.NameLower.EndsWith(".ymt"))
            //{
            //    YmtFile ymt = new YmtFile();
            //    ymt.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = ymt;
            //}
            //else if (rfe.NameLower.EndsWith(".ybn"))
            //{
            //    YbnFile ybn = new YbnFile();
            //    ybn.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = ybn;
            //}
            //else if (rfe.NameLower.EndsWith(".fxc"))
            //{
            //    FxcFile fxc = new FxcFile();
            //    fxc.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = fxc;
            //}
            //else if (rfe.NameLower.EndsWith(".yft"))
            //{
            //    YftFile yft = new YftFile();
            //    yft.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = yft;
            //    if ((yft.Fragment != null) && (yft.Fragment.Drawable != null) && (yft.Fragment.Drawable.ShaderGroup != null) && (yft.Fragment.Drawable.ShaderGroup.TextureDictionary != null))
            //    {
            //        ShowTextures(yft.Fragment.Drawable.ShaderGroup.TextureDictionary);
            //        istexdict = true;
            //    }
            //}
            //else if (rfe.NameLower.EndsWith(".ydr"))
            //{
            //    YdrFile ydr = new YdrFile();
            //    ydr.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = ydr;
            //    if ((ydr.Drawable != null) && (ydr.Drawable.ShaderGroup != null) && (ydr.Drawable.ShaderGroup.TextureDictionary != null))
            //    {
            //        ShowTextures(ydr.Drawable.ShaderGroup.TextureDictionary);
            //        istexdict = true;
            //    }
            //}
            //else if (rfe.NameLower.EndsWith(".ydd"))
            //{
            //    YddFile ydd = new YddFile();
            //    ydd.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = ydd;
            //    //todo: show embedded texdicts in ydd's? is this possible?
            //}
            //else if (rfe.NameLower.EndsWith(".ytd"))
            //{
            //    YtdFile ytd = new YtdFile();
            //    ytd.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = ytd;
            //    ShowTextures(ytd.TextureDict);
            //    istexdict = true;
            //}
            //else if (rfe.NameLower.EndsWith(".ycd"))
            //{
            //    YcdFile ycd = new YcdFile();
            //    ycd.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = ycd;
            //}
            //else if (rfe.NameLower.EndsWith(".ynd"))
            //{
            //    YndFile ynd = new YndFile();
            //    ynd.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = ynd;
            //}
            //else if (rfe.NameLower.EndsWith(".ynv"))
            //{
            //    YnvFile ynv = new YnvFile();
            //    ynv.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = ynv;
            //}
            //else if (rfe.NameLower.EndsWith("_cache_y.dat"))
            //{
            //    CacheDatFile cdf = new CacheDatFile();
            //    cdf.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = cdf;
            //}
            //else if (rfe.NameLower.EndsWith(".rel"))
            //{
            //    RelFile rel = new RelFile(rfe);
            //    rel.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = rel;
            //}
            //else if (rfe.NameLower.EndsWith(".gxt2"))
            //{
            //    Gxt2File gxt2 = new Gxt2File();
            //    gxt2.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = gxt2;
            //}
            //else if (rfe.NameLower.EndsWith(".pso"))
            //{
            //    JPsoFile pso = new JPsoFile();
            //    pso.Load(data, rfe);
            //    DetailsPropertyGrid.SelectedObject = pso;
            //}
            //else
            //{
            //    DetailsPropertyGrid.SelectedObject = null;
            //}


            //if (!istexdict)
            //{
            //    ShowTextures(null);
            //}


            Cursor = Cursors.Default;


        }

        private void DisplayFileContentsText(RpfFileEntry rfe, byte[] data, int length, int offset)
        {
            if (data == null)
            {
                Cursor = Cursors.Default;
                DataTextBox.Text = "[Error extracting file! " + rfe.File.LastError + "]";
                return;
            }

            int selline = -1;
            int selstartc = -1;
            int selendc = -1;

            if (DataHexRadio.Checked)
            {
                int charsperln = int.Parse(DataHexLineCombo.Text);
                int lines = (data.Length / charsperln) + (((data.Length % charsperln) > 0) ? 1 : 0);
                StringBuilder hexb = new StringBuilder();
                StringBuilder texb = new StringBuilder();
                StringBuilder finb = new StringBuilder();

                if (offset > 0)
                {
                    selline = offset / charsperln;
                }
                for (int i = 0; i < lines; i++)
                {
                    int pos = i * charsperln;
                    int poslim = pos + charsperln;
                    hexb.Clear();
                    texb.Clear();
                    hexb.AppendFormat("{0:X4}: ", pos);
                    for (int c = pos; c < poslim; c++)
                    {
                        if (c < data.Length)
                        {
                            byte b = data[c];
                            hexb.AppendFormat("{0:X2} ", b);
                            if (char.IsControl((char)b))
                            {
                                texb.Append(".");
                            }
                            else
                            {
                                texb.Append(Encoding.ASCII.GetString(data, c, 1));
                            }
                        }
                        else
                        {
                            hexb.Append("   ");
                            texb.Append(" ");
                        }
                    }

                    if (i == selline) selstartc = finb.Length;

                    finb.AppendLine(hexb.ToString() + "| " + texb.ToString());

                    if (i == selline) selendc = finb.Length - 1;
                }

                DataTextBox.Text = finb.ToString();
            }
            else
            {

                string text = Encoding.UTF8.GetString(data);


                DataTextBox.Text = text;

                if (offset > 0)
                {
                    selstartc = offset;
                    selendc = offset + length;
                }
            }

            if ((selstartc > 0) && (selendc > 0))
            {
                DataTextBox.SelectionStart = selstartc;
                DataTextBox.SelectionLength = selendc - selstartc;
                DataTextBox.ScrollToCaret();
            }

        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (InProgress) return;
            if (!(RpfMan?.IsInited ?? false))
            {
                MessageBox.Show("Please wait for the scan to complete.");
                return;
            }

            RpfFileEntry rfe = RpfSelectedEntry as RpfFileEntry;
            if (rfe == null)
            {
                MessageBox.Show("Please select a file to export.");
                return;
            }

            SaveFileDialog.FileName = rfe.Name;
            if (SaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fpath = SaveFileDialog.FileName;

                byte[] data = rfe.File.ExtractFile(rfe);


                if (ExportCompressCheckBox.Checked)
                {
                    data = ResourceBuilder.Compress(data);
                }


                RpfResourceFileEntry rrfe = rfe as RpfResourceFileEntry;
                if (rrfe != null) //add resource header if this is a resource file.
                {
                    data = ResourceBuilder.AddResourceHeader(rrfe, data);
                }

                if (data == null)
                {
                    MessageBox.Show("Error extracting file! " + rfe.File.LastError);
                    return;
                }

                try
                {

                    File.WriteAllBytes(fpath, data);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving file! " + ex.ToString());
                }

            }
        }

        private void DataHexRadio_CheckedChanged(object sender, EventArgs e)
        {
            SelectFile();
        }

        private void DataHexLineCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectFile();
        }

        private void ShowLargeFileContentsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SelectFile();
        }
    }
}
