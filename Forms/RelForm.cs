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
    public partial class RelForm : Form
    {


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

        private RelFile CurrentFile { get; set; }



        public RelForm()
        {
            InitializeComponent();
        }



        private void UpdateFormTitle()
        {
            Text = fileName + " - REL Viewer - CodeWalker by dexyfex";
        }


        public void LoadRel(RelFile rel)
        {

            fileName = rel?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = rel?.RpfFileEntry?.Name;
            }

            UpdateFormTitle();

            RelPropertyGrid.SelectedObject = rel;

            CurrentFile = rel;


            StringBuilder sb = new StringBuilder();
            if (rel != null)
            {
                if (rel.NameTable != null)
                {
                    sb.AppendLine("NameTable - " + rel.NameTable.Length.ToString() + " entries");
                    foreach (var name in rel.NameTable)
                    {
                        sb.AppendLine(name);
                    }
                    sb.AppendLine();
                }
                if (rel.IndexStrings != null)
                {
                    sb.AppendLine("IndexStrings - " + rel.IndexStrings.Length.ToString() + " entries");
                    foreach (var rstr in rel.IndexStrings)
                    {
                        sb.AppendLine(rstr.Name);
                    }
                    sb.AppendLine();
                }
                if (rel.IndexHashes != null)
                {
                    sb.AppendLine("IndexHashes - " + rel.IndexHashes.Length.ToString() + " entries");
                    foreach (var rhash in rel.IndexHashes)
                    {
                        uint h = rhash.Name;
                        var jstr = JenkIndex.TryGetString(h);
                        if (!string.IsNullOrEmpty(jstr))
                        {
                            sb.AppendLine(jstr);
                        }
                        else
                        {
                            sb.AppendLine("0x" + h.ToString("X").PadLeft(8, '0'));
                        }
                    }
                    sb.AppendLine();
                }
            }
            MainTextBox.Text = sb.ToString();

        }


        private void Search()
        {
            SearchResultsGrid.SelectedObject = null;

            if (CurrentFile?.RelDatasSorted == null) return;


            bool textsearch = SearchTextRadio.Checked;
            var text = SearchTextBox.Text;
            var textl = text.ToLowerInvariant();

            uint hash = 0;
            uint hashl = 0;
            if (!uint.TryParse(text, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(text);
                JenkIndex.Ensure(text);
                hashl = JenkHash.GenHash(textl);
                JenkIndex.Ensure(textl);
            }
            else
            {
                hashl = hash;
            }


            var results = new List<RelData>();

            foreach (var rd in CurrentFile.RelDatasSorted)
            {
                if (textsearch)
                {
                    if (((rd.Name?.ToLowerInvariant().Contains(textl))??false) || (rd.NameHash == hash) || (rd.NameHash == hashl) ||
                        (rd.NameHash.ToString().ToLowerInvariant().Contains(textl)))
                    {
                        results.Add(rd);
                    }
                }
                else
                {
                    if ((rd.NameHash == hash)||(rd.NameHash == hashl))
                    {
                        SearchResultsGrid.SelectedObject = rd;
                        return;
                    }
                }
            }

            if (textsearch && (results.Count > 0))
            {
                SearchResultsGrid.SelectedObject = results.ToArray();
            }
            else
            {
                SearchResultsGrid.SelectedObject = null;
            }



        }


        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search();
            }
        }
    }
}
