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
    public partial class GxtForm : Form
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



        public GxtForm()
        {
            InitializeComponent();
        }


        private void UpdateFormTitle()
        {
            Text = fileName + " - GXT Viewer - CodeWalker by dexyfex";
        }


        public void LoadGxt2(Gxt2File gxt)
        {

            fileName = gxt?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = gxt?.FileEntry?.Name;
            }

            UpdateFormTitle();

            StringBuilder sb = new StringBuilder();

            if ((gxt != null) && (gxt.TextEntries != null))
            {
                foreach (var entry in gxt.TextEntries)
                {
                    sb.Append("0x");
                    sb.Append(entry.Hash.ToString("X").PadLeft(8, '0'));
                    sb.Append(" = ");
                    sb.Append(entry.Text);
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
