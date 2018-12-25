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
    public partial class EditAudioFilePanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public RelFile CurrentFile { get; set; }

        public EditAudioFilePanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetFile(RelFile file)
        {
            CurrentFile = file;
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentFile?.Name ?? "";
        }

    }
}
