using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.Project.Panels
{
    public partial class DeleteGrassPanel : ProjectPanel
    {
        internal enum DeleteGrassMode
        {
            None,
            Batch,
            Ymap,
            Project,
            Any
        }

        public ProjectForm ProjectForm { get; set; }
        public ProjectFile CurrentProjectFile { get; set; }

        public DeleteGrassPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();

            if (ProjectForm?.WorldForm == null)
            {
                //could happen in some other startup mode - world form is required for this..
                brushModeGroupBox.Enabled = false;
            }

            this.DockStateChanged += onDockStateChanged;
            // currentYmapTextBox.DataBindings.Add("Text", ProjectForm, "CurrentYmapName", false, DataSourceUpdateMode.OnPropertyChanged);
            
        }

        private void onDockStateChanged(object sender, EventArgs e)
        {
            if(DockState == DockState.Hidden)
            {
                brushDisabledRadio.Checked = true;
                brushDisabledRadio.Focus();
            }
        }

        public void SetProject(ProjectFile project)
        {
            CurrentProjectFile = project;
        }

        internal DeleteGrassMode Mode
        {
            get
            {
                if (brushDeleteBatchRadio.Checked) return DeleteGrassMode.Batch;
                else if (brushDeleteYmapRadio.Checked) return DeleteGrassMode.Ymap;
                else if (brushDeleteProjectRadio.Checked) return DeleteGrassMode.Project;
                else if (brushDeleteAnyRadio.Checked) return DeleteGrassMode.Any;
                else return DeleteGrassMode.None;
            }
        }

        internal float BrushRadius => (float)RadiusNumericUpDown.Value;
    }
}
