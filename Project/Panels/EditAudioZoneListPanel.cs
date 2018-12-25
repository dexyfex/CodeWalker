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
    public partial class EditAudioZoneListPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public Dat151AmbientZoneList CurrentZoneList { get; set; }

        public EditAudioZoneListPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetZoneList(Dat151AmbientZoneList list)
        {
            CurrentZoneList = list;
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentZoneList?.NameHash.ToString() ?? "";
        }

    }
}
