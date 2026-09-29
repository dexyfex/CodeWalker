using GigneCoder.GameFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GigneCoder.Forms
{
    public partial class GenericForm : Form
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

        ExploreForm ExploreForm;
        object CurrentFile;


        public GenericForm(ExploreForm exploreForm)
        {
            ExploreForm = exploreForm;
            InitializeComponent();
        }


        public void LoadFile(object file, RpfFileEntry fileEntry)
        {
            CurrentFile = file;

            DetailsPropertyGrid.SelectedObject = file;

            fileName = fileEntry?.Name;

            UpdateFormTitle();
        }


        private void UpdateFormTitle()
        {
            Text = fileName + " - File Inspector - GigneCoder by dexyfex";
        }

    }
}
