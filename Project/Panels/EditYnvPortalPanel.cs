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
    public partial class EditYnvPortalPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YnvPortal YnvPortal { get; set; }

        private bool populatingui = false;

        public EditYnvPortalPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetYnvPortal(YnvPortal ynvPortal)
        {
            YnvPortal = ynvPortal;
            Tag = ynvPortal;
            UpdateFormTitle();
            UpdateYnvPortalUI();
        }

        private void UpdateFormTitle()
        {
            Text = "Nav Portal " + YnvPortal.Index.ToString();
        }


        public void UpdateYnvPortalUI()
        {
            if (YnvPortal == null)
            {
                DeletePortalButton.Enabled = false;
                AddToProjectButton.Enabled = false;
                PositionFromTextBox.Text = string.Empty;
                PositionToTextBox.Text = string.Empty;
                AngleUpDown.Value = 0;
                TypeUpDown.Value = 0;
                AreaIDFromUpDown.Value = 0;
                AreaIDToUpDown.Value = 0;
                PolyIDFrom1UpDown.Value = 0;
                PolyIDTo1UpDown.Value = 0;
                PolyIDFrom2UpDown.Value = 0;
                PolyIDTo2UpDown.Value = 0;
                Unk1UpDown.Value = 0;
                Unk2UpDown.Value = 0;
            }
            else
            {
                populatingui = true;
                DeletePortalButton.Enabled = ProjectForm.YnvExistsInProject(YnvPortal.Ynv);
                AddToProjectButton.Enabled = !DeletePortalButton.Enabled;
                PositionFromTextBox.Text = FloatUtil.GetVector3String(YnvPortal.PositionFrom);
                PositionToTextBox.Text = FloatUtil.GetVector3String(YnvPortal.PositionTo);
                AngleUpDown.Value = YnvPortal.Angle;
                TypeUpDown.Value = YnvPortal.Type;
                AreaIDFromUpDown.Value = YnvPortal.AreaIDFrom;
                AreaIDToUpDown.Value = YnvPortal.AreaIDTo;
                PolyIDFrom1UpDown.Value = YnvPortal.PolyIDFrom1;
                PolyIDTo1UpDown.Value = YnvPortal.PolyIDTo1;
                PolyIDFrom2UpDown.Value = YnvPortal.PolyIDFrom2;
                PolyIDTo2UpDown.Value = YnvPortal.PolyIDTo2;
                Unk1UpDown.Value = YnvPortal.Unk1;
                Unk2UpDown.Value = YnvPortal.Unk2;
                populatingui = false;
            }
        }

        private void PositionFromTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            Vector3 v = FloatUtil.ParseVector3String(PositionFromTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.PositionFrom != v)
                {
                    YnvPortal.PositionFrom = v;
                    ProjectForm.SetYnvHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(YnvPortal.Position);
                    ProjectForm.WorldForm.UpdateNavPortalGraphics(YnvPortal, false);
                }
            }
        }

        private void PositionToTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            Vector3 v = FloatUtil.ParseVector3String(PositionToTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.PositionTo != v)
                {
                    YnvPortal.PositionTo = v;
                    ProjectForm.SetYnvHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(YnvPortal.Position);
                    ProjectForm.WorldForm.UpdateNavPortalGraphics(YnvPortal, false);
                }
            }
        }

        private void AngleUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            byte ang = (byte)AngleUpDown.Value;
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.Angle != ang)
                {
                    YnvPortal.Angle = ang;
                    ProjectForm.SetYnvHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetRotation(YnvPortal.Orientation);
                    //ProjectForm.WorldForm.UpdateNavPortalGraphics(YnvPortal, false);
                }
            }
        }

        private void TypeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            byte typ = (byte)TypeUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.Type != typ)
                {
                    YnvPortal.Type = typ;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void AreaIDFromUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            ushort id = (ushort)AreaIDFromUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.AreaIDFrom != id)
                {
                    YnvPortal.AreaIDFrom = id;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void AreaIDToUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            ushort id = (ushort)AreaIDToUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.AreaIDTo != id)
                {
                    YnvPortal.AreaIDTo = id;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void PolyIDFrom1UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            ushort id = (ushort)PolyIDFrom1UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.PolyIDFrom1 != id)
                {
                    YnvPortal.PolyIDFrom1 = id;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void PolyIDTo1UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            ushort id = (ushort)PolyIDTo1UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.PolyIDTo1 != id)
                {
                    YnvPortal.PolyIDTo1 = id;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void PolyIDFrom2UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            ushort id = (ushort)PolyIDFrom2UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.PolyIDFrom2 != id)
                {
                    YnvPortal.PolyIDFrom2 = id;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void PolyIDTo2UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            ushort id = (ushort)PolyIDTo2UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.PolyIDTo2 != id)
                {
                    YnvPortal.PolyIDTo2 = id;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void Unk1UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            ushort unk = (ushort)Unk1UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.Unk1 != unk)
                {
                    YnvPortal.Unk1 = unk;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void Unk2UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPortal == null) return;
            byte unk = (byte)Unk2UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPortal.Unk2 != unk)
                {
                    YnvPortal.Unk2 = unk;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            if (YnvPortal == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(YnvPortal.Position);
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            if (YnvPortal == null) return;
            ProjectForm.SetProjectItem(YnvPortal);
            ProjectForm.AddYnvToProject(YnvPortal.Ynv);
        }

        private void DeletePortalButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Delete Portal TODO!");
        }
    }
}
