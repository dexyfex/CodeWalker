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
    public partial class EditYmapLodLightPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YmapLODLight CurrentLodLight { get; set; }

        private bool populatingui = false;

        public EditYmapLodLightPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
            TypeComboBox.Items.Clear();
            TypeComboBox.Items.Add(LightType.Point);
            TypeComboBox.Items.Add(LightType.Spot);
            TypeComboBox.Items.Add(LightType.Capsule);
        }

        public void SetLodLight(YmapLODLight lodlight)
        {
            CurrentLodLight = lodlight;
            Tag = lodlight;
            LoadLodLight();
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            Text = "LodLight: " + (CurrentLodLight?.Index.ToString() ?? "(none)");
        }


        private void LoadLodLight()
        {

            if (CurrentLodLight == null)
            {
                ////Panel.Enabled = false;
                AddToProjectButton.Enabled = false;
                DeleteButton.Enabled = false;
                PositionTextBox.Text = string.Empty;
                DirectionTextBox.Text = string.Empty;
                TypeComboBox.SelectedItem = LightType.Point;
                IntensityUpDown.Value = 0;
                ColourRUpDown.Value = 0;
                ColourGUpDown.Value = 0;
                ColourBUpDown.Value = 0;
                ColourLabel.BackColor = System.Drawing.Color.White;
                FalloffTextBox.Text = "";
                FalloffExponentTextBox.Text = "";
                HashTextBox.Text = "";
                InnerAngleUpDown.Value = 0;
                OuterAngleUpDown.Value = 0;
                CoronaIntensityUpDown.Value = 0;
                TimeStateFlagsTextBox.Text = "";
                foreach (int i in TimeFlagsAMCheckedListBox.CheckedIndices)
                {
                    TimeFlagsAMCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
                foreach (int i in TimeFlagsPMCheckedListBox.CheckedIndices)
                {
                    TimeFlagsPMCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
                foreach (int i in StateFlags1CheckedListBox.CheckedIndices)
                {
                    StateFlags1CheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
                foreach (int i in StateFlags2CheckedListBox.CheckedIndices)
                {
                    StateFlags2CheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            else
            {
                populatingui = true;
                var l = CurrentLodLight;
                ////Panel.Enabled = true;
                AddToProjectButton.Enabled = !ProjectForm.YmapExistsInProject(CurrentLodLight.Ymap);
                DeleteButton.Enabled = !AddToProjectButton.Enabled;
                PositionTextBox.Text = FloatUtil.GetVector3String(l.Position);
                DirectionTextBox.Text = FloatUtil.GetVector3String(l.Direction);
                TypeComboBox.SelectedItem = l.Type;
                IntensityUpDown.Value = l.Colour.A;
                ColourRUpDown.Value = l.Colour.R;
                ColourGUpDown.Value = l.Colour.G;
                ColourBUpDown.Value = l.Colour.B;
                ColourLabel.BackColor = System.Drawing.Color.FromArgb(l.Colour.R, l.Colour.G, l.Colour.B);
                FalloffTextBox.Text = FloatUtil.ToString(l.Falloff);
                FalloffExponentTextBox.Text = FloatUtil.ToString(l.FalloffExponent);
                HashTextBox.Text = l.Hash.ToString();
                InnerAngleUpDown.Value = l.ConeInnerAngle;
                OuterAngleUpDown.Value = l.ConeOuterAngleOrCapExt;
                CoronaIntensityUpDown.Value = l.CoronaIntensity;
                TimeStateFlagsTextBox.Text = l.TimeAndStateFlags.ToString();
                UpdateFlagsCheckBoxes();
                populatingui = false;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectObject(CurrentLodLight);
                }

            }
        }

        private void UpdateFlagsCheckBoxes()
        {
            var l = CurrentLodLight;
            var tfam = (l.TimeFlags >> 0) & 0xFFF;
            var tfpm = (l.TimeFlags >> 12) & 0xFFF;
            var sf1 = l.StateFlags1;
            var sf2 = l.StateFlags2;
            for (int i = 0; i < TimeFlagsAMCheckedListBox.Items.Count; i++)
            {
                TimeFlagsAMCheckedListBox.SetItemCheckState(i, ((tfam & (1u << i)) > 0) ? CheckState.Checked : CheckState.Unchecked);
            }
            for (int i = 0; i < TimeFlagsPMCheckedListBox.Items.Count; i++)
            {
                TimeFlagsPMCheckedListBox.SetItemCheckState(i, ((tfpm & (1u << i)) > 0) ? CheckState.Checked : CheckState.Unchecked);
            }
            for (int i = 0; i < StateFlags1CheckedListBox.Items.Count; i++)
            {
                StateFlags1CheckedListBox.SetItemCheckState(i, ((sf1 & (1u << i)) > 0) ? CheckState.Checked : CheckState.Unchecked);
            }
            for (int i = 0; i < StateFlags2CheckedListBox.Items.Count; i++)
            {
                StateFlags2CheckedListBox.SetItemCheckState(i, ((sf2 & (1u << i)) > 0) ? CheckState.Checked : CheckState.Unchecked);
            }
        }

        private uint GetFlagsFromItemCheck(CheckedListBox clb, ItemCheckEventArgs e)
        {
            uint flags = 0;
            for (int i = 0; i < clb.Items.Count; i++)
            {
                if ((e != null) && (e.Index == i))
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        flags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (clb.GetItemChecked(i))
                    {
                        flags += (uint)(1 << i);
                    }
                }
            }
            return flags;
        }

        private void ProjectItemChanged()
        {
            if (CurrentLodLight == null) return;
            if (ProjectForm == null) return;

            ProjectForm.SetProjectItem(CurrentLodLight);
            ProjectForm.SetYmapHasChanged(true);
        }

        private void UpdateGraphics()
        {
            if (CurrentLodLight == null) return;
            if (ProjectForm?.WorldForm == null) return;

            ProjectForm.WorldForm.UpdateLodLightGraphics(CurrentLodLight);
        }

        private void UpdateColour()
        {
            if (populatingui) return;

            var r = (byte)ColourRUpDown.Value;
            var g = (byte)ColourGUpDown.Value;
            var b = (byte)ColourBUpDown.Value;
            var i = (byte)IntensityUpDown.Value;

            ColourLabel.BackColor = System.Drawing.Color.FromArgb(r, g, b);

            if (CurrentLodLight != null)
            {
                CurrentLodLight.SetColour(new SharpDX.Color(r, g, b, i));
                UpdateGraphics();
                ProjectItemChanged();
            }
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            if (CurrentLodLight == null) return;
            if (ProjectForm == null) return;
            ProjectForm.SetProjectItem(CurrentLodLight);
            ProjectForm.AddLodLightToProject();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentLodLight);
            ProjectForm.DeleteLodLight();
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentLodLight == null) return;
            if (ProjectForm?.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentLodLight.Position, Vector3.One * CurrentLodLight.Falloff * 2.0f);
        }

        private void NormalizeDirectionButton_Click(object sender, EventArgs e)
        {
            Vector3 d = Vector3.Normalize(FloatUtil.ParseVector3String(DirectionTextBox.Text));
            DirectionTextBox.Text = FloatUtil.GetVector3String(d);
        }

        private void PositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            Vector3 v = FloatUtil.ParseVector3String(PositionTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.Position != v)
                {
                    CurrentLodLight.SetPosition(v);
                    ProjectItemChanged();
                    UpdateGraphics();
                    var wf = ProjectForm.WorldForm;
                    if (wf != null)
                    {
                        wf.BeginInvoke(new Action(() =>
                        {
                            wf.SetWidgetPosition(CurrentLodLight.Position, true);
                        }));
                    }
                }
            }
        }

        private void DirectionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            Vector3 v = FloatUtil.ParseVector3String(DirectionTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.Direction != v)
                {
                    CurrentLodLight.Direction = v;
                    CurrentLodLight.UpdateTangentsAndOrientation();
                    ProjectItemChanged();
                    UpdateGraphics();
                    var wf = ProjectForm.WorldForm;
                    if (wf != null)
                    {
                        wf.BeginInvoke(new Action(() =>
                        {
                            wf.SetWidgetRotation(CurrentLodLight.Orientation, true);
                        }));
                    }
                }
            }
        }

        private void TypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            var t = (LightType)TypeComboBox.SelectedItem;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.Type != t)
                {
                    CurrentLodLight.Type = t;
                    ProjectItemChanged();
                    UpdateGraphics();
                }
            }
            populatingui = true;
            TimeStateFlagsTextBox.Text = CurrentLodLight.TimeAndStateFlags.ToString();
            populatingui = false;
        }

        private void IntensityUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateColour();
        }

        private void ColourRUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateColour();
        }

        private void ColourGUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateColour();
        }

        private void ColourBUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateColour();
        }

        private void ColourLabel_Click(object sender, EventArgs e)
        {
            var colDiag = new ColorDialog { Color = ColourLabel.BackColor };
            if (colDiag.ShowDialog(this) == DialogResult.OK)
            {
                var c = colDiag.Color;
                populatingui = true;
                ColourRUpDown.Value = c.R;
                ColourGUpDown.Value = c.G;
                ColourBUpDown.Value = c.B;
                populatingui = false;
                UpdateColour();
            }
        }

        private void FalloffTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            float v = FloatUtil.Parse(FalloffTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.Falloff != v)
                {
                    CurrentLodLight.Falloff = v;
                    ProjectItemChanged();
                    UpdateGraphics();
                }
            }
        }

        private void FalloffExponentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            float v = FloatUtil.Parse(FalloffExponentTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.FalloffExponent != v)
                {
                    CurrentLodLight.FalloffExponent = v;
                    ProjectItemChanged();
                    UpdateGraphics();
                }
            }
        }

        private void HashTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            uint.TryParse(HashTextBox.Text, out uint v);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.Hash != v)
                {
                    CurrentLodLight.Hash = v;
                    ProjectItemChanged();
                }
            }
        }

        private void InnerAngleUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            byte v = (byte)InnerAngleUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.ConeInnerAngle != v)
                {
                    CurrentLodLight.ConeInnerAngle = v;
                    ProjectItemChanged();
                    UpdateGraphics();
                }
            }
        }

        private void OuterAngleUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            byte v = (byte)OuterAngleUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.ConeOuterAngleOrCapExt != v)
                {
                    CurrentLodLight.ConeOuterAngleOrCapExt = v;
                    ProjectItemChanged();
                    UpdateGraphics();
                }
            }
        }

        private void CoronaIntensityUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            byte v = (byte)CoronaIntensityUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.CoronaIntensity != v)
                {
                    CurrentLodLight.CoronaIntensity = v;
                    ProjectItemChanged();
                }
            }
        }

        private void TimeStateFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            uint.TryParse(TimeStateFlagsTextBox.Text, out uint v);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.TimeAndStateFlags != v)
                {
                    CurrentLodLight.TimeAndStateFlags = v;
                    ProjectItemChanged();
                }
            }
            populatingui = true;
            UpdateFlagsCheckBoxes();
            populatingui = false;
        }

        private void TimeFlagsAMCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            var tfam = GetFlagsFromItemCheck(TimeFlagsAMCheckedListBox, e);
            var tfpm = GetFlagsFromItemCheck(TimeFlagsPMCheckedListBox, null);
            var v = tfam + (tfpm << 12);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.TimeFlags != v)
                {
                    CurrentLodLight.TimeFlags = v;
                    ProjectItemChanged();
                }
            }
            populatingui = true;
            TimeStateFlagsTextBox.Text = CurrentLodLight.TimeAndStateFlags.ToString();
            populatingui = false;
        }

        private void TimeFlagsPMCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            var tfam = GetFlagsFromItemCheck(TimeFlagsAMCheckedListBox, null);
            var tfpm = GetFlagsFromItemCheck(TimeFlagsPMCheckedListBox, e);
            var v = tfam + (tfpm << 12);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.TimeFlags != v)
                {
                    CurrentLodLight.TimeFlags = v;
                    ProjectItemChanged();
                }
            }
            populatingui = true;
            TimeStateFlagsTextBox.Text = CurrentLodLight.TimeAndStateFlags.ToString();
            populatingui = false;
        }

        private void StateFlags1CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            var v = GetFlagsFromItemCheck(StateFlags1CheckedListBox, e);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.StateFlags1 != v)
                {
                    CurrentLodLight.StateFlags1 = v;
                    ProjectItemChanged();
                }
            }
            populatingui = true;
            TimeStateFlagsTextBox.Text = CurrentLodLight.TimeAndStateFlags.ToString();
            populatingui = false;
        }

        private void StateFlags2CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentLodLight == null) return;
            var v = GetFlagsFromItemCheck(StateFlags2CheckedListBox, e);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentLodLight.StateFlags2 != v)
                {
                    CurrentLodLight.StateFlags2 = v;
                    ProjectItemChanged();
                }
            }
            populatingui = true;
            TimeStateFlagsTextBox.Text = CurrentLodLight.TimeAndStateFlags.ToString();
            populatingui = false;
        }
    }
}
