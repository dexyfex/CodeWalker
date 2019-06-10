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
    public partial class EditYnvPointPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YnvPoint YnvPoint { get; set; }

        private bool populatingui = false;

        public EditYnvPointPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetYnvPoint(YnvPoint ynvPoint)
        {
            YnvPoint = ynvPoint;
            Tag = ynvPoint;
            UpdateFormTitle();
            UpdateYnvPointUI();
        }

        private void UpdateFormTitle()
        {
            Text = "Nav Point " + YnvPoint.Index.ToString();
        }


        public void UpdateYnvPointUI()
        {
            if (YnvPoint == null)
            {
                ////YnvPointPanel.Enabled = false;
                DeletePointButton.Enabled = false;
                AddToProjectButton.Enabled = false;
                YnvPointPositionTextBox.Text = string.Empty;
                YnvPointAngleUpDown.Value = 0;
                YnvPointTypeUpDown.Value = 0;
            }
            else
            {
                populatingui = true;
                ////YnvPortalPanel.Enabled = true;
                DeletePointButton.Enabled = ProjectForm.YnvExistsInProject(YnvPoint.Ynv);
                AddToProjectButton.Enabled = !DeletePointButton.Enabled;
                YnvPointPositionTextBox.Text = FloatUtil.GetVector3String(YnvPoint.Position);
                YnvPointAngleUpDown.Value = YnvPoint.Angle;
                YnvPointTypeUpDown.Value = YnvPoint.Type;
                populatingui = false;
            }
        }

        private void YnvPointPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPoint == null) return;
            Vector3 v = FloatUtil.ParseVector3String(YnvPointPositionTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoint.Position != v)
                {
                    YnvPoint.SetPosition(v);
                    ProjectForm.SetYnvHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(YnvPoint.Position);
                    ProjectForm.WorldForm.UpdateNavPointGraphics(YnvPoint, false);
                }
            }
        }

        private void YnvPointAngleUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPoint == null) return;
            byte ang = (byte)YnvPointAngleUpDown.Value;
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoint.Angle != ang)
                {
                    YnvPoint.Angle = ang;
                    ProjectForm.SetYnvHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetRotation(YnvPoint.Orientation);
                    //ProjectForm.WorldForm.UpdateNavPointGraphics(YnvPoint, false);
                }
            }
        }

        private void YnvPointTypeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPoint == null) return;
            byte typ = (byte)YnvPointTypeUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoint.Type != typ)
                {
                    YnvPoint.Type = typ;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void YnvPointGoToButton_Click(object sender, EventArgs e)
        {
            if (YnvPoint == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(YnvPoint.Position);
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            if (YnvPoint == null) return;
            ProjectForm.SetProjectItem(YnvPoint);
            ProjectForm.AddYnvToProject(YnvPoint.Ynv);
        }

        private void DeletePointButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Delete Point TODO!");
        }
    }
}
