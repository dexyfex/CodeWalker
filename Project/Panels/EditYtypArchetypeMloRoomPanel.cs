using System;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using SharpDX;

namespace CodeWalker.Project.Panels
{
    public partial class EditYtypArchetypeMloRoomPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public MCMloRoomDef CurrentRoom { get; set; }
        
        public EditYtypArchetypeMloRoomPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetRoom(MCMloRoomDef room)
        {
            CurrentRoom = room;
            Tag = room;
            UpdateFormTitle();
            MloInstanceData instance = ProjectForm.TryGetMloInstance(room?.OwnerMlo);
            ProjectForm.WorldForm?.SelectMloRoom(room, instance);
            UpdateControls();
        }

        private void UpdateControls()
        {
            if (CurrentRoom != null)
            {
                RoomNameTextBox.Text = CurrentRoom.RoomName;
                MinBoundsTextBox.Text = FloatUtil.GetVector3String(CurrentRoom.BBMin);
                MaxBoundsTextBox.Text = FloatUtil.GetVector3String(CurrentRoom.BBMax);
                RoomFlagsTextBox.Text = CurrentRoom._Data.flags.ToString();
            }
        }

        private void UpdateFormTitle()
        {
            Text = CurrentRoom?.RoomName ?? "Room";
        }

        private void RoomNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CurrentRoom == null) return;

            if (CurrentRoom.RoomName != RoomNameTextBox.Text)
            {
                CurrentRoom.RoomName = RoomNameTextBox.Text;

                TreeNode tn = ProjectForm.ProjectExplorer?.FindMloRoomTreeNode(CurrentRoom);
                if (tn != null)
                {
                    tn.Text = CurrentRoom.RoomName;
                }

                UpdateFormTitle();
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void MinBoundsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CurrentRoom == null) return;
            Vector3 bb = FloatUtil.ParseVector3String(MinBoundsTextBox.Text);
            if (CurrentRoom._Data.bbMin != bb)
            {
                CurrentRoom._Data.bbMin = bb;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void MaxBoundsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CurrentRoom == null) return;
            Vector3 bb = FloatUtil.ParseVector3String(MaxBoundsTextBox.Text);
            if (CurrentRoom._Data.bbMax != bb)
            {
                CurrentRoom._Data.bbMax = bb;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void RoomFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CurrentRoom == null) return;
            uint.TryParse(RoomFlagsTextBox.Text, out uint flags);
            for (int i = 0; i < RoomFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((flags & (1u << i)) > 0);
                RoomFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
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

        private void RoomFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (CurrentRoom == null) return;
            uint flags = 0;
            for (int i = 0; i < RoomFlagsCheckedListBox.Items.Count; i++)
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
                    if (RoomFlagsCheckedListBox.GetItemChecked(i))
                    {
                        flags += (uint)(1 << i);
                    }
                }
            }
            RoomFlagsTextBox.Text = flags.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentRoom._Data.flags != flags)
                {
                    CurrentRoom._Data.flags = flags;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }
    }
}
