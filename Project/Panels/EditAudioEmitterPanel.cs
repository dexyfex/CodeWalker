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
    public partial class EditAudioEmitterPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public AudioPlacement CurrentEmitter { get; set; }

        public EditAudioEmitterPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetEmitter(AudioPlacement emitter)
        {
            CurrentEmitter = emitter;
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentEmitter?.NameHash.ToString() ?? "";
        }

    }
}
