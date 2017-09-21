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
                fileName = rel?.FileEntry?.Name;
            }

            UpdateFormTitle();

            RelPropertyGrid.SelectedObject = rel;


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

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
