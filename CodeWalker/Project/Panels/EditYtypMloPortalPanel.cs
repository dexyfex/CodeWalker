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
    public partial class EditYtypMloPortalPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public MCMloPortalDef CurrentPortal { get; set; }

        private bool populatingui = false;

        public EditYtypMloPortalPanel(ProjectForm owner)
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
            ProjectForm.WorldForm?.SelectObject(portal, instance);
            UpdateControls();
        }

        private void UpdateControls()
        {
            if (CurrentPortal != null)
            {
                populatingui = true;
                RoomFromTextBox.Text = CurrentPortal._Data.roomFrom.ToString();
                RoomToTextBox.Text = CurrentPortal._Data.roomTo.ToString();
                FlagsTextBox.Text = CurrentPortal._Data.flags.ToString();
                for (int i = 0; i < FlagsCheckedListBox.Items.Count; i++)
                {
                    var c = ((CurrentPortal._Data.flags & (1u << i)) > 0);
                    FlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
                }
                MirrorPriorityTextBox.Text = CurrentPortal._Data.mirrorPriority.ToString();
                OpacityTextBox.Text = CurrentPortal._Data.opacity.ToString();
                AudioOcclusionTextBox.Text = CurrentPortal._Data.audioOcclusion.ToString();

                var sb = new StringBuilder();
                if (CurrentPortal.Corners != null)
                {
                    foreach (var corner in CurrentPortal.Corners)
                    {
                        if (sb.Length > 0) sb.AppendLine();
                        sb.Append(FloatUtil.GetVector3String(corner.XYZ()));
                    }
                }
                CornersTextBox.Text = sb.ToString();
                populatingui = false;
            }
            else
            {
                RoomFromTextBox.Text = string.Empty;
                RoomToTextBox.Text = string.Empty;
                FlagsTextBox.Text = string.Empty;
                MirrorPriorityTextBox.Text = string.Empty;
                OpacityTextBox.Text = string.Empty;
                AudioOcclusionTextBox.Text = string.Empty;
                CornersTextBox.Text = string.Empty;
            }
        }

        private void UpdateFormTitle()
        {
            Text = "Portal " + (CurrentPortal?.Name ?? "");
        }

        private void UpdateProjectExplorer()
        {
            TreeNode tn = ProjectForm.ProjectExplorer?.FindMloPortalTreeNode(CurrentPortal);
            if (tn != null)
            {
                tn.Text = CurrentPortal.Name;
            }
        }


        private void RoomFromTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPortal == null) return;

            uint u = 0;
            uint.TryParse(RoomFromTextBox.Text, out u);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPortal._Data.roomFrom != u)
                {
                    CurrentPortal._Data.roomFrom = u;
                    CurrentPortal.OwnerMlo?.UpdatePortalCounts();
                    ProjectForm.SetYtypHasChanged(true);
                }
            }

            UpdateFormTitle();
            UpdateProjectExplorer();
        }

        private void RoomToTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPortal == null) return;

            uint u = 0;
            uint.TryParse(RoomToTextBox.Text, out u);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPortal._Data.roomTo != u)
                {
                    CurrentPortal._Data.roomTo = u;
                    CurrentPortal.OwnerMlo?.UpdatePortalCounts();
                    ProjectForm.SetYtypHasChanged(true);
                }
            }

            UpdateFormTitle();
            UpdateProjectExplorer();
        }

        private void FlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPortal == null) return;

            uint u = 0;
            uint.TryParse(FlagsTextBox.Text, out u);

            for (int i = 0; i < FlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((u & (1u << i)) > 0);
                FlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }

            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPortal._Data.flags != u)
                {
                    CurrentPortal._Data.flags = u;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }

        private void FlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentPortal == null) return;

            uint flags = 0;
            for (int i = 0; i < FlagsCheckedListBox.Items.Count; i++)
            {
                if (e.Index == i)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        flags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (FlagsCheckedListBox.GetItemChecked(i))
                    {
                        flags += (uint)(1 << i);
                    }
                }
            }
            FlagsTextBox.Text = flags.ToString();
        }

        private void MirrorPriorityTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPortal == null) return;

            uint u = 0;
            uint.TryParse(MirrorPriorityTextBox.Text, out u);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPortal._Data.mirrorPriority != u)
                {
                    CurrentPortal._Data.mirrorPriority = u;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }

        private void OpacityTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPortal == null) return;

            uint u = 0;
            uint.TryParse(OpacityTextBox.Text, out u);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPortal._Data.opacity != u)
                {
                    CurrentPortal._Data.opacity = u;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }

        private void AudioOcclusionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPortal == null) return;

            uint u = 0;
            uint.TryParse(AudioOcclusionTextBox.Text, out u);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPortal._Data.audioOcclusion != u)
                {
                    CurrentPortal._Data.audioOcclusion = u;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }

        private void CornersTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPortal == null) return;

            var corners = new List<Vector4>();
            var strs = CornersTextBox.Text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var str in strs)
            {
                var tstr = str.Trim();
                if (string.IsNullOrEmpty(tstr)) continue;
                var c = FloatUtil.ParseVector3String(tstr);
                corners.Add(new Vector4(c, float.NaN));
            }
            lock (ProjectForm.ProjectSyncRoot)
            {
                CurrentPortal.Corners = corners.ToArray();
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void AddEntityButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentPortal);
            ProjectForm.NewMloEntity();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentPortal);
            ProjectForm.DeleteMloPortal();
        }

        private void FlipPortalButton_Click(object sender, EventArgs e)
        {
            if (CurrentPortal == null || CurrentPortal.Corners == null || CurrentPortal.Corners.Length < 3) return;

            lock (ProjectForm.ProjectSyncRoot)
            {
                var corners = CurrentPortal.Corners.ToList();
                corners.Reverse();
                CurrentPortal.Corners = corners.ToArray();
                uint tempRoom = CurrentPortal._Data.roomFrom;
                CurrentPortal._Data.roomFrom = CurrentPortal._Data.roomTo;
                CurrentPortal._Data.roomTo = tempRoom;
                populatingui = true;
                StringBuilder sb = new StringBuilder();
                foreach (var corner in CurrentPortal.Corners)
                {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.Append(FloatUtil.GetVector3String(new Vector3(corner.X, corner.Y, corner.Z)));
                }
                CornersTextBox.Text = sb.ToString();
                RoomFromTextBox.Text = CurrentPortal._Data.roomFrom.ToString();
                RoomToTextBox.Text = CurrentPortal._Data.roomTo.ToString();
                populatingui = false;
                CurrentPortal.OwnerMlo?.UpdatePortalCounts(); //Is this necessary if the counter doesn't change?
                ProjectForm.SetYtypHasChanged(true);
                UpdateFormTitle();
                UpdateProjectExplorer();
            }
        }
    }
}
