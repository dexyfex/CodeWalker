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
    public partial class EditYtypPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YtypFile Ytyp { get; set; }

        //private bool populatingui = false;
        private bool waschanged = false;

        public EditYtypPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetYtyp(YtypFile ytyp)
        {
            Ytyp = ytyp;
            Tag = ytyp;
            UpdateFormTitle();
            UpdateYtypUI();
            waschanged = ytyp?.HasChanged ?? false;
        }

        public void UpdateFormTitleYtypChanged()
        {
            bool changed = Ytyp.HasChanged;
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
            string fn = Ytyp.RpfFileEntry?.Name ?? Ytyp.Name;
            if (string.IsNullOrEmpty(fn)) fn = "untitled.ytyp";
            Text = fn + (Ytyp.HasChanged ? "*" : "");
        }


        public void UpdateYtypUI()
        {
        }
    }
}
