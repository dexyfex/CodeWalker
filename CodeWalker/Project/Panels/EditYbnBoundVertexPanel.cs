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
    public partial class EditYbnBoundVertexPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public BoundVertex CollisionVertex { get; set; }

        private bool populatingui = false;
        private bool waschanged = false;

        public EditYbnBoundVertexPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetCollisionVertex(BoundVertex v)
        {
            CollisionVertex = v;
            Tag = v;
            UpdateFormTitle();
            UpdateUI();
            waschanged = v?.Owner?.HasChanged ?? false;
        }

        public void UpdateFormTitleYbnChanged()
        {
            bool changed = CollisionVertex?.Owner?.HasChanged ?? false;
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
            string fn = CollisionVertex?.Title ?? "untitled";
            Text = fn + ((CollisionVertex?.Owner?.HasChanged ?? false) ? "*" : "");
        }


        public void UpdateUI()
        {
            if (CollisionVertex == null)
            {
                AddToProjectButton.Enabled = false;
                DeleteButton.Enabled = false;
                PositionTextBox.Text = string.Empty;
                ColourTextBox.Text = string.Empty;
            }
            else
            {
                populatingui = true;

                PositionTextBox.Text = FloatUtil.GetVector3String(CollisionVertex.Position);
                ColourTextBox.Text = CollisionVertex.Colour.ToString();

                var ybn = CollisionVertex.Owner?.GetRootYbn();
                AddToProjectButton.Enabled = (ybn != null) ? !ProjectForm.YbnExistsInProject(ybn) : false;
                DeleteButton.Enabled = !AddToProjectButton.Enabled;

                populatingui = false;
            }
        }

        private void PositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionVertex == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(PositionTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionVertex.Position != v)
                {
                    CollisionVertex.Position = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void ColourTextBox_TextChanged(object sender, EventArgs e)
        {
            //TODO!!
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CollisionVertex);
            ProjectForm.AddCollisionVertexToProject();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CollisionVertex);
            ProjectForm.DeleteCollisionVertex();
        }
    }
}
