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
    public partial class YcdForm : Form
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



        public YcdForm()
        {
            InitializeComponent();
        }



        private void UpdateFormTitle()
        {
            Text = fileName + " - Clip Dictionary Inspector - CodeWalker by dexyfex";
        }


        public void LoadYcd(YcdFile ycd)
        {
            fileName = ycd?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = ycd?.RpfFileEntry?.Name;
            }

            UpdateFormTitle();


            MainPropertyGrid.SelectedObject = ycd;

        }


    }
}
