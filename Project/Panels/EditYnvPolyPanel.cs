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
    public partial class EditYnvPolyPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YnvPoly YnvPoly { get; set; }

        //private bool populatingui = false;

        public EditYnvPolyPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetYnvPoly(YnvPoly ynvPoly)
        {
            YnvPoly = ynvPoly;
            Tag = ynvPoly;
            UpdateFormTitle();
            UpdateYnvUI();
        }

        private void UpdateFormTitle()
        {
            Text = "Nav Poly " + YnvPoly.Index.ToString();
        }


        public void UpdateYnvUI()
        {
        }

    }
}
