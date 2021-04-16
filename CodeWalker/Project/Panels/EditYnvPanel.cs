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
    public partial class EditYnvPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YnvFile Ynv { get; set; }

        private bool populatingui = false;
        private bool waschanged = false;

        public EditYnvPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetYnv(YnvFile ynv)
        {
            Ynv = ynv;
            Tag = ynv;
            UpdateFormTitle();
            UpdateYnvUI();
            waschanged = ynv?.HasChanged ?? false;
        }

        public void UpdateFormTitleYnvChanged()
        {
            bool changed = Ynv.HasChanged;
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
            string fn = Ynv.RpfFileEntry?.Name ?? Ynv.Name;
            if (string.IsNullOrEmpty(fn)) fn = "untitled.ynv";
            Text = fn + (Ynv.HasChanged ? "*" : "");
        }


        public void UpdateYnvUI()
        {
            if (Ynv?.Nav == null)
            {
                ////YnvPanel.Enabled = false;
                YnvRpfPathTextBox.Text = string.Empty;
                YnvProjectPathTextBox.Text = string.Empty;
                YnvAreaIDXUpDown.Value = 0;
                YnvAreaIDYUpDown.Value = 0;
                YnvAreaIDInfoLabel.Text = "ID: -";
                YnvAABBSizeTextBox.Text = string.Empty;
                YnvVertexCountLabel.Text = "Vertex count: -";
                YnvPolyCountLabel.Text = "Poly count: -";
                YnvPortalCountLabel.Text = "Portal count: -";
                YnvPortalLinkCountLabel.Text = "Portal link count: -";
                YnvPointCountLabel.Text = "Sector unk count: -";
                YnvByteCountLabel.Text = "Byte count: -";
                YnvVersionUnkHashTextBox.Text = string.Empty;
            }
            else
            {
                var nv = Ynv.Nav;
                populatingui = true;
                ////YnvPanel.Enabled = true;
                YnvRpfPathTextBox.Text = Ynv.RpfFileEntry.Path;
                YnvProjectPathTextBox.Text = (Ynv != null) ? ProjectForm.CurrentProjectFile.GetRelativePath(Ynv.FilePath) : Ynv.FilePath;
                YnvAreaIDXUpDown.Value = Ynv.CellX;
                YnvAreaIDYUpDown.Value = Ynv.CellY;
                YnvAreaIDInfoLabel.Text = "ID: " + Ynv.AreaID.ToString();
                YnvAABBSizeTextBox.Text = FloatUtil.GetVector3String(nv.AABBSize);
                YnvFlagsPolygonsCheckBox.Checked = nv.ContentFlags.HasFlag(NavMeshFlags.Polygons);
                YnvFlagsPortalsCheckBox.Checked = nv.ContentFlags.HasFlag(NavMeshFlags.Portals);
                YnvFlagsVehicleCheckBox.Checked = nv.ContentFlags.HasFlag(NavMeshFlags.Vehicle);
                YnvFlagsUnknownCheckBox.Checked = nv.ContentFlags.HasFlag(NavMeshFlags.Unknown8);
                YnvVertexCountLabel.Text = "Vertex count: " + nv.VerticesCount.ToString();
                YnvPolyCountLabel.Text = "Poly count: " + nv.PolysCount.ToString();
                YnvPortalCountLabel.Text = "Portal count: " + nv.PortalsCount.ToString();
                YnvPortalLinkCountLabel.Text = "Portal link count: " + nv.PortalLinksCount.ToString();
                YnvPointCountLabel.Text = "Point count: " + nv.PointsCount.ToString();
                YnvByteCountLabel.Text = "Byte count: " + nv.TotalBytes.ToString();
                YnvVersionUnkHashTextBox.Text = nv.VersionUnk2.Hash.ToString();
                YnvAdjAreaIDsTextBox.Text = GetAdjAreaIDsString(nv.AdjAreaIDs.Values);
                populatingui = false;
            }
        }



        private string GetAdjAreaIDsString(uint[] vals)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < vals.Length; i++)
            {
                var adjid = vals[i];
                sb.AppendLine(adjid.ToString());
            }
            return sb.ToString();
        }
        private uint[] GetAdjAreaIdsArray(string text)
        {
            var rsplit = new[] { '\n' };
            var csplit = new[] { ' ' };
            string[] rowstrs = text.Split(rsplit, StringSplitOptions.RemoveEmptyEntries);
            uint[] vals = new uint[rowstrs.Length];
            for (int i = 0; i < rowstrs.Length; i++)
            {
                string rowstr = rowstrs[i].Trim();
                uint.TryParse(rowstr, out vals[i]);
            }
            return vals;
        }


        private void YnvAreaIDUpDownChange()
        {
            if (populatingui) return;
            if (Ynv?.Nav == null) return;
            int x = (int)YnvAreaIDXUpDown.Value;
            int y = (int)YnvAreaIDYUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                var areaid = y * 100 + x;
                if (Ynv.AreaID != areaid)
                {
                    Ynv.AreaID = areaid;
                    //Ynv.Name = "nodes" + areaid.ToString() + ".ynd";
                    YnvAreaIDInfoLabel.Text = "ID: " + areaid.ToString();
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
            UpdateFormTitleYnvChanged();
        }

        private void YnvFlagsCheckBoxChange()
        {
            if (populatingui) return;
            if (Ynv?.Nav == null) return;
            var verts = YnvFlagsPolygonsCheckBox.Checked ? NavMeshFlags.Polygons : NavMeshFlags.None;
            var ports = YnvFlagsPortalsCheckBox.Checked ? NavMeshFlags.Portals : NavMeshFlags.None;
            var vehcs = YnvFlagsVehicleCheckBox.Checked ? NavMeshFlags.Vehicle : NavMeshFlags.None;
            var unk8s = YnvFlagsUnknownCheckBox.Checked ? NavMeshFlags.Unknown8 : NavMeshFlags.None;
            var f = verts | ports | vehcs | unk8s;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (Ynv.Nav.ContentFlags != f)
                {
                    Ynv.Nav.ContentFlags = f;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
            UpdateFormTitleYnvChanged();
        }


        private void YnvAreaIDXUpDown_ValueChanged(object sender, EventArgs e)
        {
            YnvAreaIDUpDownChange();
        }

        private void YnvAreaIDYUpDown_ValueChanged(object sender, EventArgs e)
        {
            YnvAreaIDUpDownChange();
        }

        private void YnvAABBSizeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (Ynv?.Nav == null) return;
            Vector3 v = FloatUtil.ParseVector3String(YnvAABBSizeTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (Ynv.Nav.AABBSize != v)
                {
                    Ynv.Nav.AABBSize = v;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
            UpdateFormTitleYnvChanged();
        }

        private void YnvFlagsVerticesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            YnvFlagsCheckBoxChange();
        }

        private void YnvFlagsPortalsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            YnvFlagsCheckBoxChange();
        }

        private void YnvFlagsVehicleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            YnvFlagsCheckBoxChange();
        }

        private void YnvFlagsUnknownCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            YnvFlagsCheckBoxChange();
        }

        private void YnvVersionUnkHashTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (Ynv?.Nav == null) return;
            uint v = 0;
            uint.TryParse(YnvVersionUnkHashTextBox.Text, out v);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (Ynv.Nav.VersionUnk2 != v)
                {
                    Ynv.Nav.VersionUnk2 = v;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
            UpdateFormTitleYnvChanged();
        }

        private void YnvAdjAreaIDsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (Ynv?.Nav == null) return;
            var areaids = new NavMeshUintArray();
            areaids.Values = GetAdjAreaIdsArray(YnvAdjAreaIDsTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                Ynv.Nav.AdjAreaIDs = areaids;
                ProjectForm.SetYnvHasChanged(true);
            }
            UpdateFormTitleYnvChanged();
        }
    }
}
