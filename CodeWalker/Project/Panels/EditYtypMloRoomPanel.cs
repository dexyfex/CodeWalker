using System;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using SharpDX;

namespace CodeWalker.Project.Panels
{
    public partial class EditYtypMloRoomPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public MCMloRoomDef CurrentRoom { get; set; }
        public MloArchetype CurrentMLO { get; set; }
        private bool populatingui = false;

        public EditYtypMloRoomPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            CurrentMLO = ProjectForm.GetMloArchetype();
            InitializeComponent();
        }

        public void SetRoom(MCMloRoomDef room)
        {
            CurrentRoom = room;
            Tag = room;
            UpdateFormTitle();
            MloInstanceData instance = ProjectForm.TryGetMloInstance(room?.OwnerMlo);
            ProjectForm.WorldForm?.SelectObject(room, instance);
            UpdateControls();
        }

        private void UpdateControls()
        {
            if (CurrentRoom != null)
            {
                populatingui = true;
                NameTextBox.Text = CurrentRoom.RoomName;
                MinBoundsTextBox.Text = FloatUtil.GetVector3String(CurrentRoom.BBMin);
                MaxBoundsTextBox.Text = FloatUtil.GetVector3String(CurrentRoom.BBMax);
                FlagsTextBox.Text = CurrentRoom._Data.flags.ToString();
                for (int i = 0; i < FlagsCheckedListBox.Items.Count; i++)
                {
                    var c = ((CurrentRoom._Data.flags & (1u << i)) > 0);
                    FlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
                }
                BlendTextBox.Text = FloatUtil.ToString(CurrentRoom._Data.blend);
                TimecycleTextBox.Text = CurrentRoom._Data.timecycleName.ToCleanString();
                Timecycle2TextBox.Text = CurrentRoom._Data.secondaryTimecycleName.ToCleanString();
                PortalCountTextBox.Text = CurrentRoom._Data.portalCount.ToString();
                FloorIDTextBox.Text = CurrentRoom._Data.floorId.ToString();
                ExteriorVisDepthTextBox.Text = CurrentRoom._Data.exteriorVisibiltyDepth.ToString();
                populatingui = false;
            }
            else
            {
                NameTextBox.Text = string.Empty;
                MinBoundsTextBox.Text = string.Empty;
                MaxBoundsTextBox.Text = string.Empty;
                FlagsTextBox.Text = string.Empty;
                BlendTextBox.Text = string.Empty;
                TimecycleTextBox.Text = string.Empty;
                Timecycle2TextBox.Text = string.Empty;
                PortalCountTextBox.Text = string.Empty;
                FloorIDTextBox.Text = string.Empty;
                ExteriorVisDepthTextBox.Text = string.Empty;
            }
        }

        private void UpdateFormTitle()
        {
            Text = CurrentRoom?.RoomName ?? "Room";
        }

        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            if (CurrentRoom.RoomName != NameTextBox.Text)
            {
                CurrentRoom.RoomName = NameTextBox.Text;

                TreeNode tn = ProjectForm.ProjectExplorer?.FindMloRoomTreeNode(CurrentRoom);
                if (tn != null)
                {
                    tn.Text = CurrentRoom.Index.ToString() + ": " + CurrentRoom.RoomName;
                }

                UpdateFormTitle();
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void MinBoundsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            Vector3 bb = FloatUtil.ParseVector3String(MinBoundsTextBox.Text);
            if (CurrentRoom._Data.bbMin != bb)
            {
                CurrentRoom._Data.bbMin = bb;
                CurrentRoom.BBMin_CW = bb;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void MaxBoundsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            Vector3 bb = FloatUtil.ParseVector3String(MaxBoundsTextBox.Text);
            if (CurrentRoom._Data.bbMax != bb)
            {
                CurrentRoom._Data.bbMax = bb;
                CurrentRoom.BBMax_CW = bb;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void FlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint.TryParse(FlagsTextBox.Text, out uint flags);
            for (int i = 0; i < FlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((flags & (1u << i)) > 0);
                FlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentRoom._Data.flags != flags)
                {
                    CurrentRoom._Data.flags = flags;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }

        private void FlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

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

        private void BlendTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            FloatUtil.TryParse(BlendTextBox.Text, out float blend);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentRoom._Data.blend != blend)
                {
                    CurrentRoom._Data.blend = blend;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }

        private void TimecycleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            var hash = JenkHash.GenHash(TimecycleTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentRoom._Data.timecycleName != hash)
                {
                    CurrentRoom._Data.timecycleName = hash;
                    ProjectForm.SetYtypHasChanged(true);
                    JenkIndex.Ensure(TimecycleTextBox.Text);
                }
            }
        }

        private void Timecycle2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            var hash = JenkHash.GenHash(Timecycle2TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentRoom._Data.secondaryTimecycleName != hash)
                {
                    CurrentRoom._Data.secondaryTimecycleName = hash;
                    ProjectForm.SetYtypHasChanged(true);
                    JenkIndex.Ensure(Timecycle2TextBox.Text);
                }
            }
        }

        private void PortalCountTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint.TryParse(PortalCountTextBox.Text, out uint count);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentRoom._Data.portalCount != count)
                {
                    CurrentRoom._Data.portalCount = count;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }

        private void FloorIDTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            int.TryParse(FloorIDTextBox.Text, out int floor);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentRoom._Data.floorId != floor)
                {
                    CurrentRoom._Data.floorId = floor;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }

        private void ExteriorVisDepthTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            int.TryParse(ExteriorVisDepthTextBox.Text, out int depth);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentRoom._Data.exteriorVisibiltyDepth != depth)
                {
                    CurrentRoom._Data.exteriorVisibiltyDepth = depth;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }

        private void AddEntityButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentRoom);
            ProjectForm.NewMloEntity();
        }

        private void CalcPortalCountButton_Click(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;
            if (CurrentMLO == null) return;
            uint portalCount = 0;
            foreach (MCMloPortalDef portal in CurrentMLO.portals)
            {
                if (portal.Data.roomFrom == CurrentRoom.Index || portal.Data.roomTo == CurrentRoom.Index)
                {
                    portalCount++;
                }
            }
            if (portalCount == CurrentRoom._Data.portalCount && PortalCountTextBox.Text == portalCount.ToString()) return;
            CurrentRoom._Data.portalCount = portalCount;
            PortalCountTextBox.Text = portalCount.ToString();
            ProjectForm.SetYtypHasChanged(true);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentRoom);
            ProjectForm.DeleteMloRoom();
        }
    }
}
