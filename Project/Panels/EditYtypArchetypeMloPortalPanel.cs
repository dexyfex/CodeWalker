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
    public partial class EditYtypArchetypeMloPortalPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public MCMloPortalDef CurrentPortal { get; set; }

        public EditYtypArchetypeMloPortalPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetPortal(MCMloPortalDef portal)
        {
            CurrentPortal = portal;
            Tag = portal;
            UpdateFormTitle();
            MloInstanceData instance = ProjectForm.TryGetMloInstance(portal?.OwnerMlo);
            //ProjectForm.WorldForm?.SelectMloPortal(portal, instance);
            UpdateControls();
        }

        private void UpdateControls()
        {
            if (CurrentPortal != null)
            {
                //PortalNameTextBox.Text = CurrentPortal.Name;
            }
        }

        private void UpdateFormTitle()
        {
            Text = "Portal " + (CurrentPortal?.Name ?? "");
        }

    }
}
