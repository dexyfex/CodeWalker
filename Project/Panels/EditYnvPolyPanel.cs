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
    public partial class EditYnvPolyPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YnvPoly YnvPoly { get; set; }

        private bool populatingui = false;

        public EditYnvPolyPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetYnvPoly(YnvPoly ynvPoly)
        {
            YnvPoly = ynvPoly;
            Tag = ynvPoly;
            UpdateFormTitle();
            UpdateYnvUI();
        }

        private void UpdateFormTitle()
        {
            Text = "Nav Poly " + YnvPoly.Index.ToString();
        }


        public void UpdateYnvUI()
        {
            if (YnvPoly == null)
            {
                ////YnvPolyPanel.Enabled = false;
                DeletePolyButton.Enabled = false;
                AddToProjectButton.Enabled = false;
                AreaIDUpDown.Value = 0;
                PartIDUpDown.Value = 0;
                PortalIDUpDown.Value = 0;
                PortalCountUpDown.Value = 0;
                SetCheckedListBoxValues(FlagsCheckedListBox1, 0);
                SetCheckedListBoxValues(FlagsCheckedListBox2, 0);
                SetCheckedListBoxValues(FlagsCheckedListBox3, 0);
                SetCheckedListBoxValues(FlagsCheckedListBox4, 0);
                UnkXUpDown.Value = 0;
                UnkYUpDown.Value = 0;
            }
            else
            {
                populatingui = true;
                ////YnvPolyPanel.Enabled = true;
                DeletePolyButton.Enabled = ProjectForm.YnvExistsInProject(YnvPoly.Ynv);
                AddToProjectButton.Enabled = !DeletePolyButton.Enabled;
                AreaIDUpDown.Value = YnvPoly.AreaID;
                PartIDUpDown.Value = YnvPoly.PartID;
                PortalIDUpDown.Value = YnvPoly.PortalLinkID;
                PortalCountUpDown.Value = YnvPoly.PortalLinkCount;
                SetCheckedListBoxValues(FlagsCheckedListBox1, YnvPoly.Flags1);
                SetCheckedListBoxValues(FlagsCheckedListBox2, YnvPoly.Flags2);
                SetCheckedListBoxValues(FlagsCheckedListBox3, YnvPoly.Flags3);
                SetCheckedListBoxValues(FlagsCheckedListBox4, YnvPoly.Flags4);
                UnkXUpDown.Value = YnvPoly.UnkX;
                UnkYUpDown.Value = YnvPoly.UnkY;
                populatingui = false;
            }
        }


        private void SetCheckedListBoxValues(CheckedListBox clb, byte flags)
        {
            for (int i = 0; i < clb.Items.Count; i++)
            {
                var c = ((flags & (1 << i)) > 0);
                clb.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
        }
        private byte GetCheckedListBoxValues(CheckedListBox clb, ItemCheckEventArgs e)
        {
            byte r = 0;
            for (int i = 0; i < clb.Items.Count; i++)
            {
                if ((e != null) && (e.Index == i))
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        r += (byte)(1 << i);
                    }
                }
                else
                {
                    bool v = clb.GetItemChecked(i);// == CheckState.Checked;
                    r = (byte)BitUtil.UpdateBit(r, i, v);
                }
            }
            return r;
        }


        private void AreaIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPoly == null) return;
            ushort areaid = (ushort)AreaIDUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoly.AreaID != areaid)
                {
                    YnvPoly.AreaID = areaid;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void PartIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPoly == null) return;
            ushort partid = (ushort)PartIDUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoly.PartID != partid)
                {
                    YnvPoly.PartID = partid;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void PortalIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPoly == null) return;
            ushort portalid = (ushort)PortalIDUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoly.PortalLinkID != portalid)
                {
                    YnvPoly.PortalLinkID = portalid;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void PortalCountUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPoly == null) return;
            byte portalcount = (byte)PortalCountUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoly.PortalLinkCount != portalcount)
                {
                    YnvPoly.PortalLinkCount = portalcount;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void FlagsCheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (YnvPoly == null) return;
            byte flags = GetCheckedListBoxValues(FlagsCheckedListBox1, e);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoly.Flags1 != flags)
                {
                    YnvPoly.Flags1 = flags;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
            if (ProjectForm.WorldForm != null)
            {
                ProjectForm.WorldForm.UpdateNavPolyGraphics(YnvPoly, false);
            }
        }

        private void FlagsCheckedListBox2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (YnvPoly == null) return;
            byte flags = GetCheckedListBoxValues(FlagsCheckedListBox2, e);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoly.Flags2 != flags)
                {
                    YnvPoly.Flags2 = flags;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
            if (ProjectForm.WorldForm != null)
            {
                ProjectForm.WorldForm.UpdateNavPolyGraphics(YnvPoly, false);
            }
        }

        private void FlagsCheckedListBox3_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (YnvPoly == null) return;
            byte flags = GetCheckedListBoxValues(FlagsCheckedListBox3, e);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoly.Flags3 != flags)
                {
                    YnvPoly.Flags3 = flags;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
            if (ProjectForm.WorldForm != null)
            {
                ProjectForm.WorldForm.UpdateNavPolyGraphics(YnvPoly, false);
            }
        }

        private void FlagsCheckedListBox4_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (YnvPoly == null) return;
            byte flags = GetCheckedListBoxValues(FlagsCheckedListBox4, e);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoly.Flags4 != flags)
                {
                    YnvPoly.Flags4 = flags;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
            if (ProjectForm.WorldForm != null)
            {
                ProjectForm.WorldForm.UpdateNavPolyGraphics(YnvPoly, false);
            }
        }

        private void UnkXUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPoly == null) return;
            byte unkx = (byte)UnkXUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoly.UnkX != unkx)
                {
                    YnvPoly.UnkX = unkx;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void UnkYUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (YnvPoly == null) return;
            byte unky = (byte)UnkYUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (YnvPoly.UnkY != unky)
                {
                    YnvPoly.UnkY = unky;
                    ProjectForm.SetYnvHasChanged(true);
                }
            }
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            if (YnvPoly == null) return;
            ProjectForm.SetProjectItem(YnvPoly);
            ProjectForm.AddYnvToProject(YnvPoly.Ynv);
        }

        private void DeletePolyButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Delete Polygon TODO!");
        }
    }
}
