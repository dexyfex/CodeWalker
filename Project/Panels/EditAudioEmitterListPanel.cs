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
    public partial class EditAudioEmitterListPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public Dat151AmbientEmitterList CurrentEmitterList { get; set; }

        public EditAudioEmitterListPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetEmitterList(Dat151AmbientEmitterList list)
        {
            CurrentEmitterList = list;
            Tag = list;
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentEmitterList?.NameHash.ToString() ?? "";
        }
    }
}
