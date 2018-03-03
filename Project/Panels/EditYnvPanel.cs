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

namespace CodeWalker.Project.Panels
{
    public partial class EditYnvPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YnvFile Ynv { get; set; }

        //private bool populatingui = false;
        private bool waschanged = false;

        public EditYnvPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetYnv(YnvFile ynv)
        {
            Ynv = ynv;
            Tag = ynv;
            UpdateFormTitle();
            UpdateYnvUI();
            waschanged = ynv?.HasChanged ?? false;
        }

        public void UpdateFormTitleYnvChanged()
        {
            bool changed = Ynv.HasChanged;
            if (!waschanged && changed)
            {
                UpdateFormTitle();
                waschanged = true;
            }
            else if (waschanged && !changed)
            {
                UpdateFormTitle();
                waschanged = false;
            }
        }
        private void UpdateFormTitle()
        {
            string fn = Ynv.RpfFileEntry?.Name ?? Ynv.Name;
            if (string.IsNullOrEmpty(fn)) fn = "untitled.ynv";
            Text = fn + (Ynv.HasChanged ? "*" : "");
        }


        public void UpdateYnvUI()
        {
        }

    }
}
