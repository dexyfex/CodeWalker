using CodeWalker.GameFiles;
using CodeWalker.Properties;
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

namespace CodeWalker
{
    public partial class BrowseForm : Form
    {
        private volatile bool KeysLoaded = false;
        private volatile bool InProgress = false;
        private volatile bool AbortOperation = false;

        private int TotalFileCount = 0;
        private List<RpfFile> ScannedFiles = new List<RpfFile>();
        private List<RpfFile> RootFiles = new List<RpfFile>();

        private List<SearchResult> SearchResults = new List<SearchResult>();
        private RpfEntry SelectedEntry = null;
        private int SelectedOffset = -1;
        private int SelectedLength = 0;

        private bool TextureTabPageVisible = false;

        private bool FlatStructure = false;

        public BrowseForm()
        {
            InitializeComponent();
        }

        private void BrowseForm_Load(object sender, EventArgs e)
        {
            var info = DetailsPropertyGrid.GetType().GetProperty("Controls");
            var collection = info.GetValue(DetailsPropertyGrid, null) as Control.ControlCollection;
            foreach (var control in collection)
            {
                var ctyp = control.GetType();
                if (ctyp.Name == "PropertyGridView")
                {
                    var prop = ctyp.GetField("labelRatio");
                    var val = prop.GetValue(control);
                    prop.SetValue(control, 4.0); //somehow this sets the width of the property grid's label column...
                }
            }

            FolderTextBox.Text = GTAFolder.CurrentGTAFolder;
            DataHexLineCombo.Text = "16";

            DataTextBox.SetTabStopWidth(3);

            HideTexturesTab();

            try
            {
                GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
                KeysLoaded = true;
                UpdateStatus("Ready to scan...");
            }
            catch
            {
                UpdateStatus("Keys not loaded! This should not happen.");
            }
        }

        private void BrowseForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!TextureTabPageVisible)
            {
                TexturesTabPage.Dispose();
            }
        }

        private void FolderBrowseButton_Click(object sender, EventArgs e)
        {
            GTAFolder.UpdateGTAFolder(false);
            FolderTextBox.Text = GTAFolder.CurrentGTAFolder;
        }

        private void ScanButton_Click(object sender, EventArgs e)
        {
            if (InProgress) return;

            if (!KeysLoaded) //this shouldn't ever really happen anymore
            {
                MessageBox.Show("Please scan a GTA 5 exe dump for keys first, or include key files in this app's folder!");
                return;
            }
            if (!Directory.Exists(FolderTextBox.Text))
            {
                MessageBox.Show("Folder doesn't exist: " + FolderTextBox.Text);
                return;
            }

            InProgress = true;
            AbortOperation = false;
            ScannedFiles.Clear();
            RootFiles.Clear();

            MainTreeView.Nodes.Clear();

            string searchpath = FolderTextBox.Text;
            string replpath = searchpath + "\\";

            Task.Run(() =>
            {

                UpdateStatus("Starting scan...");

                string[] allfiles = Directory.GetFiles(searchpath, "*.rpf", SearchOption.AllDirectories);

                uint totrpfs = 0;
                uint totfiles = 0;
                uint totfolders = 0;
                uint totresfiles = 0;
                uint totbinfiles = 0;

                foreach (string rpfpath in allfiles)
                {
                    if (AbortOperation)
                    {
                        UpdateStatus("Scan aborted!");
                        InProgress = false;
                        return;
                    }

                    RpfFile rf = new RpfFile(rpfpath, rpfpath.Replace(replpath, ""));

                    UpdateStatus("Scanning " + rf.Name + "...");

                    rf.ScanStructure(UpdateStatus, UpdateStatus);

                    totrpfs += rf.GrandTotalRpfCount;
                    totfiles += rf.GrandTotalFileCount;
                    totfolders += rf.GrandTotalFolderCount;
                    totresfiles += rf.GrandTotalResourceCount;
                    totbinfiles += rf.GrandTotalBinaryFileCount;

                    AddScannedFile(rf, null, true);

                    RootFiles.Add(rf);
                }

                UpdateStatus(string.Format("Scan complete. {0} RPF files, {1} total files, {2} total folders, {3} resources, {4} binary files.", totrpfs, totfiles, totfolders, totresfiles, totbinfiles));
                InProgress = false;
                TotalFileCount = (int)totfiles;
            });

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

        private void ClearFiles()
        {
            MainTreeView.Nodes.Clear();
        }
        private void AddScannedFile(RpfFile file, TreeNode node, bool addToList = false)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { AddScannedFile(file, node, addToList); }));
                }
                else
                {

                    var cnode = AddFileNode(file, node);

                    if (FlatStructure) cnode = null;

                    foreach (RpfFile cfile in file.Children)
                    {
                        AddScannedFile(cfile, cnode, addToList);
                    }
                    if (addToList)
                    {
                        ScannedFiles.Add(file);
                    }
                }
            }
            catch { }
        }
        private TreeNode AddFileNode(RpfFile file, TreeNode n)
        {
            var nodes = (n == null) ? MainTreeView.Nodes : n.Nodes;
            TreeNode node = nodes.Add(file.Path);
            node.Tag = file;

            foreach (RpfEntry entry in file.AllEntries)
            {
                if (entry is RpfFileEntry)
                {
                    bool show = !entry.NameLower.EndsWith(".rpf"); //rpf entries get their own root node..
                    if (show)
                    {
                        //string text = entry.Path.Substring(file.Path.Length + 1); //includes \ on the end
                        //TreeNode cnode = node.Nodes.Add(text);
                        //cnode.Tag = entry;
                        TreeNode cnode = AddEntryNode(entry, node);
                    }
                }
            }


            //make sure it's all in jenkindex...
            JenkIndex.Ensure(file.Name);
            foreach (RpfEntry entry in file.AllEntries)
            {
                if (string.IsNullOrEmpty(entry.Name)) continue;
                JenkIndex.Ensure(entry.Name);
                JenkIndex.Ensure(entry.NameLower);
                int ind = entry.Name.LastIndexOf('.');
                if (ind > 0)
                {
                    JenkIndex.Ensure(entry.Name.Substring(0, ind));
                    JenkIndex.Ensure(entry.NameLower.Substring(0, ind));
                }
            }

            return node;




            //TreeNode lastNode = null;
            //string subPathAgg;
            //subPathAgg = string.Empty;
            //foreach (string subPath in file.Path.Split('\\'))
            //{
            //    subPathAgg += subPath + '\\';
            //    TreeNode[] nodes = MainTreeView.Nodes.Find(subPathAgg, true);
            //    if (nodes.Length == 0)
            //    {
            //        if (lastNode == null)
            //        {
            //            lastNode = MainTreeView.Nodes.Add(subPathAgg, subPath);
            //        }
            //        else
            //        {
            //            lastNode = lastNode.Nodes.Add(subPathAgg, subPath);
            //        }
            //    }
            //    else
            //    {
            //        lastNode = nodes[0];
            //    }
            //}
            //lastNode.Tag = file;

        }
        private TreeNode AddEntryNode(RpfEntry entry, TreeNode node)
        {
            string text = entry.Path.Substring(entry.File.Path.Length + 1); //includes \ on the end
            TreeNode cnode = (node != null) ? node.Nodes.Add(text) : MainTreeView.Nodes.Add(text);
            cnode.Tag = entry;
            return cnode;
        }


        private void MainTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;
            if (!e.Node.IsSelected) return; //only do this for selected node
            if (MainTreeView.SelectedNode == null) return;

            SelectedEntry = MainTreeView.SelectedNode.Tag as RpfEntry;
            SelectedOffset = -1;
            SelectedLength = 0;

            SelectFile();
        }
        private void DataTextRadio_CheckedChanged(object sender, EventArgs e)
        {
            SelectFile();
        }
        private void DataHexLineCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectFile();
        }
        private void DataHexRadio_CheckedChanged(object sender, EventArgs e)
        {
            SelectFile();
        }


        private void SelectFile()
        {
            SelectFile(SelectedEntry, SelectedOffset, SelectedLength);
        }
        private void SelectFile(RpfEntry entry, int offset, int length)
        {
            SelectedEntry = entry;
            SelectedOffset = offset;
            SelectedLength = length;

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
                    DataTextBox.Text = "[Please select a data file]";
                }
                ShowTextures(null);
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



            bool istexdict = false;


            if (rfe.NameLower.EndsWith(".ymap"))
            {
                YmapFile ymap = new YmapFile(rfe);
                ymap.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = ymap;
            }
            else if (rfe.NameLower.EndsWith(".ytyp"))
            {
                YtypFile ytyp = new YtypFile();
                ytyp.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = ytyp;
            }
            else if (rfe.NameLower.EndsWith(".ymf"))
            {
                YmfFile ymf = new YmfFile();
                ymf.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = ymf;
            }
            else if (rfe.NameLower.EndsWith(".ymt"))
            {
                YmtFile ymt = new YmtFile();
                ymt.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = ymt;
            }
            else if (rfe.NameLower.EndsWith(".ybn"))
            {
                YbnFile ybn = new YbnFile();
                ybn.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = ybn;
            }
            else if (rfe.NameLower.EndsWith(".fxc"))
            {
                FxcFile fxc = new FxcFile();
                fxc.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = fxc;
            }
            else if (rfe.NameLower.EndsWith(".yft"))
            {
                YftFile yft = new YftFile();
                yft.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = yft;

                if ((yft.Fragment != null) && (yft.Fragment.Drawable != null) && (yft.Fragment.Drawable.ShaderGroup != null) && (yft.Fragment.Drawable.ShaderGroup.TextureDictionary != null))
                {
                    ShowTextures(yft.Fragment.Drawable.ShaderGroup.TextureDictionary);
                    istexdict = true;
                }
            }
            else if (rfe.NameLower.EndsWith(".ydr"))
            {
                YdrFile ydr = new YdrFile();
                ydr.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = ydr;

                if ((ydr.Drawable != null) && (ydr.Drawable.ShaderGroup != null) && (ydr.Drawable.ShaderGroup.TextureDictionary != null))
                {
                    ShowTextures(ydr.Drawable.ShaderGroup.TextureDictionary);
                    istexdict = true;
                }
            }
            else if (rfe.NameLower.EndsWith(".ydd"))
            {
                YddFile ydd = new YddFile();
                ydd.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = ydd;
                //todo: show embedded texdicts in ydd's? is this possible?
            }
            else if (rfe.NameLower.EndsWith(".ytd"))
            {
                YtdFile ytd = new YtdFile();
                ytd.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = ytd;
                ShowTextures(ytd.TextureDict);
                istexdict = true;
            }
            else if (rfe.NameLower.EndsWith(".ycd"))
            {
                YcdFile ycd = new YcdFile();
                ycd.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = ycd;
            }
            else if (rfe.NameLower.EndsWith(".ynd"))
            {
                YndFile ynd = new YndFile();
                ynd.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = ynd;
            }
            else if (rfe.NameLower.EndsWith(".ynv"))
            {
                YnvFile ynv = new YnvFile();
                ynv.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = ynv;
            }
            else if (rfe.NameLower.EndsWith("_cache_y.dat"))
            {
                CacheDatFile cdf = new CacheDatFile();
                cdf.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = cdf;
            }
            else if (rfe.NameLower.EndsWith(".rel"))
            {
                RelFile rel = new RelFile(rfe);
                rel.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = rel;
            }
            else if (rfe.NameLower.EndsWith(".gxt2"))
            {
                Gxt2File gxt2 = new Gxt2File();
                gxt2.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = gxt2;
            }
            else if (rfe.NameLower.EndsWith(".pso"))
            {
                JPsoFile pso = new JPsoFile();
                pso.Load(data, rfe);
                DetailsPropertyGrid.SelectedObject = pso;
            }
            else
            {
                DetailsPropertyGrid.SelectedObject = null;

            }


            if (!istexdict)
            {
                ShowTextures(null);
            }


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

        private void ShowTextures(TextureDictionary td)
        {
            SelTexturesListView.Items.Clear();
            SelTexturePictureBox.Image = null;
            SelTextureNameTextBox.Text = string.Empty;
            SelTextureDimensionsLabel.Text = "-";
            SelTextureMipLabel.Text = "0";
            SelTextureMipTrackBar.Value = 0;
            SelTextureMipTrackBar.Maximum = 0;

            if (td == null)
            {
                HideTexturesTab();
                return;
            }
            if (!SelectionTabControl.TabPages.Contains(TexturesTabPage))
            {
                SelectionTabControl.TabPages.Add(TexturesTabPage);
            }


            if ((td.Textures == null) || (td.Textures.data_items == null)) return;
            var texs = td.Textures.data_items;

            for (int i = 0; i < texs.Length; i++)
            {
                var tex = texs[i];
                ListViewItem lvi = SelTexturesListView.Items.Add(tex.Name);
                lvi.Tag = tex;
            }

        }

        private void HideTexturesTab()
        {
            if (SelectionTabControl.TabPages.Contains(TexturesTabPage))
            {
                SelectionTabControl.TabPages.Remove(TexturesTabPage);
            }
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

                SelTexturePictureBox.Image = bmp;
                SelTextureDimensionsLabel.Text = w.ToString() + " x " + h.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading texture mip:\n" + ex.ToString());
                SelTexturePictureBox.Image = null;
            }

        }



        private void AbortButton_Click(object sender, EventArgs e)
        {
            AbortOperation = true;
        }


        private void TestAllButton_Click(object sender, EventArgs e)
        {
            if (InProgress) return;
            if (ScannedFiles.Count == 0)
            {
                MessageBox.Show("Please scan the GTAV folder first.");
                return;
            }

            AbortOperation = false;
            InProgress = true;

            DataTextBox.Text = string.Empty;
            FileInfoLabel.Text = "[File test results]";

            Task.Run(() =>
            {

                UpdateStatus("Starting test...");

                StringBuilder sbout = new StringBuilder();
                int errcount = 0;
                int curfile = 1;
                int totrpfs = ScannedFiles.Count;
                long totbytes = 0;

                foreach (RpfFile file in ScannedFiles)
                {
                    if (AbortOperation)
                    {
                        UpdateStatus("Test aborted.");
                        InProgress = false;
                        return;
                    }

                    UpdateStatus(curfile.ToString() + "/" + totrpfs.ToString() + ": Testing " + file.FilePath + "...");

                    string errorstr = file.TestExtractAllFiles();

                    if (!string.IsNullOrEmpty(errorstr))
                    {
                        AddTestError(errorstr);
                        sbout.Append(errorstr);
                        errcount++;
                    }

                    totbytes += file.ExtractedByteCount;
                    curfile++;
                }


                UpdateStatus("Test complete. " + errcount.ToString() + " problems encountered, " + totbytes.ToString() + " total bytes extracted.");
                InProgress = false;
            });
        }
        private void AddTestError(string error)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { AddTestError(error); }));
                }
                else
                {
                    DataTextBox.AppendText(error);

                }
            }
            catch { }
        }






        private void Find()
        {
            if (InProgress) return;
            if (ScannedFiles.Count == 0)
            {
                MessageBox.Show("Please scan the GTAV folder first.");
                return;
            }


            string find = FindTextBox.Text.ToLowerInvariant();
            Cursor = Cursors.WaitCursor;
            if (string.IsNullOrEmpty(find))
            {
                ClearFiles();
                foreach (RpfFile file in RootFiles)
                {
                    AddScannedFile(file, null); //reset the file tree... slow :(
                }
            }
            else
            {
                ClearFiles();
                int count = 0;
                int max = 500;
                foreach (RpfFile file in ScannedFiles)
                {
                    if (file.Name.ToLowerInvariant().Contains(find))
                    {
                        AddFileNode(file, null);
                        count++;
                    }
                    foreach (RpfEntry entry in file.AllEntries)
                    {
                        if (entry.NameLower.Contains(find))
                        {
                            if (entry is RpfDirectoryEntry)
                            {
                                RpfDirectoryEntry direntry = entry as RpfDirectoryEntry;

                                TreeNode node = AddEntryNode(entry, null);

                                foreach (RpfFileEntry cfentry in direntry.Files)
                                {
                                    //if (cfentry.Name.EndsWith(".rpf", StringComparison.InvariantCultureIgnoreCase)) continue;
                                    AddEntryNode(cfentry, node);
                                }
                                count++;
                            }
                            else if (entry is RpfBinaryFileEntry)
                            {
                                if (entry.NameLower.EndsWith(".rpf", StringComparison.InvariantCultureIgnoreCase)) continue;
                                AddEntryNode(entry, null);
                                count++;
                            }
                            else if (entry is RpfResourceFileEntry)
                            {
                                AddEntryNode(entry, null);
                                count++;
                            }
                        }
                        if (count >= max)
                        {
                            MessageBox.Show("Search results limited to " + max.ToString() + " entries.");
                            break;
                        }
                    }
                    if (count >= max)
                    {
                        break;
                    }
                }
            }
            Cursor = Cursors.Default;
        }
        private void FindTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Find();
                e.Handled = true;
            }
        }
        private void FindButton_Click(object sender, EventArgs e)
        {
            Find();
        }



        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (InProgress) return;
            if (ScannedFiles.Count == 0)
            {
                MessageBox.Show("Please scan the GTAV folder first.");
                return;
            }
            TreeNode node = MainTreeView.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("Please select a file to export.");
                return;
            }

            RpfFileEntry rfe = node.Tag as RpfFileEntry;
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




        private class SearchResult
        {
            public RpfFileEntry FileEntry { get; set; }
            public int Offset { get; set; }
            public int Length { get; set; }

            public SearchResult(RpfFileEntry entry, int offset, int length)
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

        private void AddSearchResult(SearchResult result)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { AddSearchResult(result); }));
                }
                else
                {
                    SearchResults.Add(result);
                    SearchResultsListView.VirtualListSize = SearchResults.Count;
                }
            }
            catch { }
        }

        private void Search()
        {
            if (InProgress) return;
            if (ScannedFiles.Count == 0)
            {
                MessageBox.Show("Please scan the GTAV folder first.");
                return;
            }
            if (SearchTextBox.Text.Length == 0)
            {
                MessageBox.Show("Please enter a search term.");
                return;
            }

            string searchtxt = SearchTextBox.Text;
            bool hex = SearchHexRadioButton.Checked;
            bool casesen = SearchCaseSensitiveCheckBox.Checked || hex;
            bool bothdirs = SearchBothDirectionsCheckBox.Checked;
            string[] ignoreexts = null;
            byte[] searchbytes1;
            byte[] searchbytes2;
            int bytelen;

            if (!casesen) searchtxt = searchtxt.ToLowerInvariant(); //case sensitive search in lower case.

            if (SearchIgnoreCheckBox.Checked)
            {
                ignoreexts = SearchIgnoreTextBox.Text.Split(',');
                for (int i = 0; i < ignoreexts.Length; i++)
                {
                    ignoreexts[i] = ignoreexts[i].Trim();
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

            SearchTextBox.Enabled = false;
            SearchHexRadioButton.Enabled = false;
            SearchTextRadioButton.Enabled = false;
            SearchCaseSensitiveCheckBox.Enabled = false;
            SearchBothDirectionsCheckBox.Enabled = false;
            SearchIgnoreCheckBox.Enabled = false;
            SearchIgnoreTextBox.Enabled = false;
            SearchButton.Enabled = false;
            SearchSaveResultsButton.Enabled = false;

            InProgress = true;
            AbortOperation = false;
            SearchResultsListView.VirtualListSize = 0;
            SearchResults.Clear();
            int totfiles = TotalFileCount;
            int curfile = 0;
            Task.Run(() =>
            {

                DateTime starttime = DateTime.Now;
                int resultcount = 0;

                for (int f = 0; f < ScannedFiles.Count; f++)
                {
                    var rpffile = ScannedFiles[f];

                    //UpdateStatus(string.Format("Searching {0}/{1} : {2}", f, ScannedFiles.Count, rpffile.Path));

                    foreach (var entry in rpffile.AllEntries)
                    {
                        var duration = DateTime.Now - starttime;
                        if (AbortOperation)
                        {
                            UpdateStatus(duration.ToString(@"hh\:mm\:ss") + " - Search aborted.");
                            InProgress = false;
                            SearchComplete();
                            return;
                        }

                        RpfFileEntry fentry = entry as RpfFileEntry;
                        if (fentry == null) continue;

                        curfile++;

                        if (fentry.NameLower.EndsWith(".rpf"))
                        { continue; }

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
                                AddSearchResult(new SearchResult(fentry, (i - bytelen), bytelen));
                                resultcount++;
                                hitlen1 = 0;
                            }
                            if (bothdirs)
                            {
                                if (b == b2) hitlen2++; else hitlen2 = 0;
                                if (hitlen2 == bytelen)
                                {
                                    AddSearchResult(new SearchResult(fentry, (i - bytelen), bytelen));
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
                SearchComplete();
            });
        }
        private void SearchComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { SearchComplete(); }));
                }
                else
                {
                    SearchTextBox.Enabled = true;
                    SearchHexRadioButton.Enabled = true;
                    SearchTextRadioButton.Enabled = true;
                    SearchCaseSensitiveCheckBox.Enabled = SearchTextRadioButton.Checked;
                    SearchBothDirectionsCheckBox.Enabled = true;
                    SearchIgnoreCheckBox.Enabled = true;
                    SearchIgnoreTextBox.Enabled = SearchIgnoreCheckBox.Checked;
                    SearchButton.Enabled = true;
                    SearchSaveResultsButton.Enabled = true;
                }
            }
            catch { }
        }
        private void SearchButton_Click(object sender, EventArgs e)
        {
            Search();
        }
        private void SearchAbortButton_Click(object sender, EventArgs e)
        {
            AbortOperation = true;
        }
        private void SearchSaveResultsButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog.FileName = "SearchResults.txt";
            if (SaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fpath = SaveFileDialog.FileName;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("CodeWalker Search Results for \"" + SearchTextBox.Text + "\"");
                sb.AppendLine("[File path], [Byte offset]");
                if (SearchResults != null)
                {
                    foreach (var r in SearchResults)
                    {
                        sb.AppendLine(r.FileEntry.Path + ", " + r.Offset.ToString());
                    }
                }

                File.WriteAllText(fpath, sb.ToString());

            }
        }
        private void SearchTextRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SearchCaseSensitiveCheckBox.Enabled = SearchTextRadioButton.Checked;
        }
        private void SearchHexRadioButton_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void SearchResultsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var item = new ListViewItem();
            if (e.ItemIndex < SearchResults.Count)
            {
                SearchResult r = SearchResults[e.ItemIndex];
                item.Text = r.FileEntry.Name;
                item.SubItems.Add(r.Offset.ToString());
                item.Tag = r;
            }
            e.Item = item;
        }
        private void SearchResultsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SearchResultsListView.SelectedIndices.Count == 1)
            {
                var i = SearchResultsListView.SelectedIndices[0];
                if ((i >= 0) && (i < SearchResults.Count))
                {
                    var r = SearchResults[i];
                    SelectFile(r.FileEntry, r.Offset+1, r.Length);
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
        private void SearchIgnoreCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SearchIgnoreTextBox.Enabled = SearchIgnoreCheckBox.Checked;
        }

        private void SelTexturesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            Texture tex = null;
            if (SelTexturesListView.SelectedItems.Count == 1)
            {
                tex = SelTexturesListView.SelectedItems[0].Tag as Texture;
            }
            ShowTextureMip(tex, 0, false);
        }

        private void SelTextureMipTrackBar_Scroll(object sender, EventArgs e)
        {
            Texture tex = null;
            if (SelTexturesListView.SelectedItems.Count == 1)
            {
                tex = SelTexturesListView.SelectedItems[0].Tag as Texture;
            }
            SelTextureMipLabel.Text = SelTextureMipTrackBar.Value.ToString();
            ShowTextureMip(tex, SelTextureMipTrackBar.Value, true);
        }

        private void ShowLargeFileContentsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SelectFile();
        }

        private void FlattenStructureCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FlatStructure = FlattenStructureCheckBox.Checked;

            if (InProgress) return;
            if (ScannedFiles.Count == 0) return;

            Cursor = Cursors.WaitCursor;

            SearchTextBox.Clear();

            ClearFiles();
            foreach (RpfFile file in RootFiles)
            {
                AddScannedFile(file, null);
            }

            Cursor = Cursors.Default;
        }
    }
}
