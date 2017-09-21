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

        public BinarySearchForm()
        {
            InitializeComponent();
        }

        private void SearchForm_Load(object sender, EventArgs e)
        {
            BinSearchFolderTextBox.Text = Settings.Default.CompiledScriptFolder;
        }


        private void BinSearchFolderBrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog.SelectedPath = BinSearchFolderTextBox.Text;
            DialogResult res = FolderBrowserDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                BinSearchFolderTextBox.Text = FolderBrowserDialog.SelectedPath;
            }
        }

        private void BinSearchButton_Click(object sender, EventArgs e)
        {
            string searchtxt = BinSearchTextBox.Text;
            string searchfolder = BinSearchFolderTextBox.Text;
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

            SearchResultsTextBox.Clear();

            byte[] searchbytes1;
            byte[] searchbytes2;
            int bytelen;

            if (HexRadio.Checked)
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

            InProgress = true;

            Task.Run(() =>
            {

                AddSearchResult("Searching " + searchfolder + "...");

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
                            AddSearchResult(finf.Name + ":" + (i - bytelen));
                            matchcount++;
                            hitlen1 = 0;
                        }
                        if (hitlen2 == bytelen)
                        {
                            AddSearchResult(finf.Name + ":" + (i - bytelen));
                            matchcount++;
                            hitlen2 = 0;
                        }

                        if (AbortOperation)
                        {
                            InProgress = false;
                            AddSearchResult("Search aborted.");
                            return;
                        }

                    }

                }

                AddSearchResult(string.Format("Search complete. {0} results found.", matchcount));
                InProgress = false;
            });
        }

        private void AddSearchResult(string result)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { AddSearchResult(result); }));
                }
                else
                {
                    SearchResultsTextBox.AppendText(result + "\r\n");
                }
            }
            catch { }
        }

        private void SearchAbortButton_Click(object sender, EventArgs e)
        {
            AbortOperation = true;
        }

    }
}
