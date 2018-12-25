using CodeWalker.World;
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
    public partial class EditAudioZonePanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public AudioPlacement CurrentZone { get; set; }

        public EditAudioZonePanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetZone(AudioPlacement zone)
        {
            CurrentZone = zone;
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentZone?.NameHash.ToString() ?? "";
        }


    }
}
