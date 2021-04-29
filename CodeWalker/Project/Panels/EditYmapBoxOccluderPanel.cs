using CodeWalker.GameFiles;
using SharpDX;
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
    public partial class EditYmapBoxOccluderPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YmapBoxOccluder CurrentBoxOccluder { get; set; }

        private bool populatingui = false;


        public EditYmapBoxOccluderPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }


        public void SetBoxOccluder(YmapBoxOccluder box)
        {
            CurrentBoxOccluder = box;
            Tag = box;
            LoadBoxOccluder();
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            Text = "BoxOccluder: " + (CurrentBoxOccluder?.Index.ToString() ?? "(none)");
        }


        private void LoadBoxOccluder()
        {

            if (CurrentBoxOccluder == null)
            {
                ////Panel.Enabled = false;
                AddToProjectButton.Enabled = false;
                DeleteButton.Enabled = false;
                CenterTextBox.Text = string.Empty;
                SizeTextBox.Text = string.Empty;
                SinCosZTextBox.Text = string.Empty;
            }
            else
            {
                populatingui = true;
                var b = CurrentBoxOccluder;
                ////Panel.Enabled = true;
                AddToProjectButton.Enabled = !ProjectForm.YmapExistsInProject(CurrentBoxOccluder.Ymap);
                DeleteButton.Enabled = !AddToProjectButton.Enabled;
                CenterTextBox.Text = FloatUtil.GetVector3String(b.Position);
                SizeTextBox.Text = FloatUtil.GetVector3String(b.Size);
                var dir = b.Orientation.Multiply(Vector3.UnitX) * 0.5f;
                SinCosZTextBox.Text = FloatUtil.GetVector2String(new Vector2(dir.X, dir.Y));
                populatingui = false;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectObject(CurrentBoxOccluder);
                }

            }
        }

        private void UpdateGraphics()
        {
            if (CurrentBoxOccluder == null) return;
            if (ProjectForm?.WorldForm == null) return;

            ProjectForm.WorldForm.UpdateBoxOccluderGraphics(CurrentBoxOccluder);
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentBoxOccluder == null) return;
            if (ProjectForm?.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentBoxOccluder.Position, CurrentBoxOccluder.Size * 2.0f);
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            if (CurrentBoxOccluder == null) return;
            if (ProjectForm == null) return;
            ProjectForm.SetProjectItem(CurrentBoxOccluder);
            ProjectForm.AddBoxOccluderToProject();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentBoxOccluder);
            ProjectForm.DeleteBoxOccluder();
        }

        private void CenterTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentBoxOccluder == null) return;
            var v = FloatUtil.ParseVector3String(CenterTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentBoxOccluder.Position != v)
                {
                    CurrentBoxOccluder.Position = v;
                    UpdateGraphics();
                    ProjectForm.SetYmapHasChanged(true);
                    ProjectForm.WorldForm?.SetWidgetPosition(v);
                }
            }

            ProjectForm.ProjectExplorer?.UpdateBoxOccluderTreeNode(CurrentBoxOccluder);

        }

        private void SizeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentBoxOccluder == null) return;
            var v = FloatUtil.ParseVector3String(SizeTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentBoxOccluder.Size != v)
                {
                    CurrentBoxOccluder.SetSize(v);
                    UpdateGraphics();
                    ProjectForm.SetYmapHasChanged(true);
                    ProjectForm.WorldForm?.SetWidgetScale(v);
                }
            }
        }

        private void SinCosZTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentBoxOccluder == null) return;
            var v = FloatUtil.ParseVector2String(SinCosZTextBox.Text);
            float angl = (float)Math.Atan2(v.Y, v.X);
            var q = Quaternion.RotationYawPitchRoll(0.0f, 0.0f, angl);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentBoxOccluder.Orientation != q)
                {
                    CurrentBoxOccluder.Orientation = q;
                    UpdateGraphics();
                    ProjectForm.SetYmapHasChanged(true);
                    ProjectForm.WorldForm?.SetWidgetRotation(q);
                }
            }
        }
    }
}
