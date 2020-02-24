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
    public partial class EditYbnPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YbnFile Ybn { get; set; }

        //private bool populatingui = false;
        private bool waschanged = false;

        public EditYbnPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetYbn(YbnFile ybn)
        {
            Ybn = ybn;
            Tag = ybn;
            UpdateFormTitle();
            UpdateUI();
            waschanged = ybn?.HasChanged ?? false;
        }

        public void UpdateFormTitleYbnChanged()
        {
            bool changed = Ybn?.HasChanged ?? false;
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
            string fn = Ybn?.RpfFileEntry?.Name ?? Ybn?.Name;
            if (string.IsNullOrEmpty(fn)) fn = "untitled.ybn";
            Text = fn + ((Ybn?.HasChanged??false) ? "*" : "");
        }


        public void UpdateUI()
        {
            if (Ybn?.Bounds == null)
            {


            }
            else
            {
                var b = Ybn.Bounds;
                //populatingui = true;


                //populatingui = false;
            }
        }

    }
}
