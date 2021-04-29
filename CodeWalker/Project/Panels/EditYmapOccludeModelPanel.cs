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
    public partial class EditYmapOccludeModelPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YmapOccludeModel CurrentOccludeModel { get; set; }
        public YmapOccludeModelTriangle CurrentTriangle { get; set; }

        private bool populatingui = false;


        public EditYmapOccludeModelPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }


        public void SetOccludeModel(YmapOccludeModel model)
        {
            CurrentOccludeModel = model;
            CurrentTriangle = null;
            Tag = model;
            LoadOccludeModel();
            LoadTriangle();
            UpdateFormTitle();
            OccludeModelTabControl.SelectedTab = ModelTabPage;
        }
        public void SetOccludeModelTriangle(YmapOccludeModelTriangle tri)
        {
            CurrentTriangle = tri;
            CurrentOccludeModel = tri?.Model;
            Tag = tri?.Model;
            LoadOccludeModel();
            LoadTriangle();
            UpdateFormTitle();
            OccludeModelTabControl.SelectedTab = TriangleTabPage;
        }

        private void UpdateGraphics()
        {
            var m = CurrentOccludeModel ?? CurrentTriangle?.Model;
            if (m == null) return;
            if (ProjectForm?.WorldForm == null) return;

            ProjectForm.WorldForm.UpdateOccludeModelGraphics(m);
        }

        private void UpdateFormTitle()
        {
            Text = "OccludeModel: " + (CurrentOccludeModel?.Index.ToString() ?? "(none)");
        }


        private void LoadOccludeModel()
        {

            if (CurrentOccludeModel == null)
            {
                ////Panel.Enabled = false;
                ModelAddToProjectButton.Enabled = false;
                ModelDeleteButton.Enabled = false;
                ModelFlagsTextBox.Text = string.Empty;
            }
            else
            {
                populatingui = true;
                var m = CurrentOccludeModel;
                ////Panel.Enabled = true;
                ModelAddToProjectButton.Enabled = !ProjectForm.YmapExistsInProject(CurrentOccludeModel.Ymap);
                ModelDeleteButton.Enabled = !ModelAddToProjectButton.Enabled;
                ModelFlagsTextBox.Text = m.Flags.Value.ToString();
                populatingui = false;

                //if (ProjectForm.WorldForm != null)
                //{
                //    ProjectForm.WorldForm.SelectObject(CurrentOccludeModel);
                //}

            }
        }
        private void LoadTriangle()
        {

            if (CurrentTriangle == null)
            {
                ////Panel.Enabled = false;
                TriangleAddToProjectButton.Enabled = false;
                TriangleDeleteButton.Enabled = false;
                TriangleCenterTextBox.Text = string.Empty;
                TriangleCorner1TextBox.Text = string.Empty;
                TriangleCorner2TextBox.Text = string.Empty;
                TriangleCorner3TextBox.Text = string.Empty;
            }
            else
            {
                populatingui = true;
                var t = CurrentTriangle;
                ////Panel.Enabled = true;
                TriangleAddToProjectButton.Enabled = !ProjectForm.YmapExistsInProject(CurrentTriangle.Ymap);
                TriangleDeleteButton.Enabled = !TriangleAddToProjectButton.Enabled;
                TriangleCenterTextBox.Text = FloatUtil.GetVector3String(t.Center);
                TriangleCorner1TextBox.Text = FloatUtil.GetVector3String(t.Corner1);
                TriangleCorner2TextBox.Text = FloatUtil.GetVector3String(t.Corner2);
                TriangleCorner3TextBox.Text = FloatUtil.GetVector3String(t.Corner3);
                populatingui = false;

                //if (ProjectForm.WorldForm != null)
                //{
                //    ProjectForm.WorldForm.SelectObject(CurrentTriangle);
                //}

            }
        }

        private void TriangleGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentTriangle == null) return;
            if (ProjectForm?.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentTriangle.Center, CurrentTriangle.Box.Size * 2.0f);
        }

        private void TriangleAddToProjectButton_Click(object sender, EventArgs e)
        {
            if (CurrentTriangle == null) return;
            if (ProjectForm == null) return;
            ProjectForm.SetProjectItem(CurrentTriangle);
            ProjectForm.AddOccludeModelTriangleToProject();
        }

        private void TriangleDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentTriangle);
            ProjectForm.DeleteOccludeModelTriangle();
        }

        private void ModelAddToProjectButton_Click(object sender, EventArgs e)
        {
            if (CurrentOccludeModel == null) return;
            if (ProjectForm == null) return;
            ProjectForm.SetProjectItem(CurrentOccludeModel);
            ProjectForm.AddOccludeModelToProject();
        }

        private void ModelDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentOccludeModel);
            ProjectForm.DeleteOccludeModel();
        }

        private void ModelFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentOccludeModel == null) return;
            uint.TryParse(ModelFlagsTextBox.Text, out uint f);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentOccludeModel.Flags != f)
                {
                    CurrentOccludeModel.Flags = f;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void TriangleCenterTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentTriangle == null) return;
            var v = FloatUtil.ParseVector3String(TriangleCenterTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentTriangle.Center != v)
                {
                    CurrentTriangle.Center = v;
                    UpdateGraphics();
                    ProjectForm.SetYmapHasChanged(true);
                    ProjectForm.WorldForm?.SetWidgetPosition(v);
                }
            }
        }

        private void TriangleCorner1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentTriangle == null) return;
            var v = FloatUtil.ParseVector3String(TriangleCorner1TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentTriangle.Corner1 != v)
                {
                    CurrentTriangle.Corner1 = v;
                    UpdateGraphics();
                    ProjectForm.SetYmapHasChanged(true);
                    ProjectForm.WorldForm?.SetWidgetPosition(CurrentTriangle.Center);
                }
            }
        }

        private void TriangleCorner2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentTriangle == null) return;
            var v = FloatUtil.ParseVector3String(TriangleCorner2TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentTriangle.Corner2 != v)
                {
                    CurrentTriangle.Corner2 = v;
                    UpdateGraphics();
                    ProjectForm.SetYmapHasChanged(true);
                    ProjectForm.WorldForm?.SetWidgetPosition(CurrentTriangle.Center);
                }
            }
        }

        private void TriangleCorner3TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentTriangle == null) return;
            var v = FloatUtil.ParseVector3String(TriangleCorner3TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentTriangle.Corner3 != v)
                {
                    CurrentTriangle.Corner3 = v;
                    UpdateGraphics();
                    ProjectForm.SetYmapHasChanged(true);
                    ProjectForm.WorldForm?.SetWidgetPosition(CurrentTriangle.Center);
                }
            }
        }
    }
}
