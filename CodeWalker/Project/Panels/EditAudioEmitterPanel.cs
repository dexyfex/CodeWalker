using CodeWalker.GameFiles;
using CodeWalker.World;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Project.Panels
{
    public partial class EditAudioEmitterPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public AudioPlacement CurrentEmitter { get; set; }

        private bool populatingui = false;


        public EditAudioEmitterPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetEmitter(AudioPlacement emitter)
        {
            CurrentEmitter = emitter;
            Tag = emitter;
            UpdateFormTitle();
            UpdateUI();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentEmitter?.NameHash.ToString() ?? "";
        }


        private void UpdateUI()
        {
            if (CurrentEmitter?.AudioEmitter == null)
            {
                AddToProjectButton.Enabled = false;
                DeleteButton.Enabled = false;

                populatingui = true;
                NameTextBox.Text = string.Empty;
                PositionTextBox.Text = string.Empty;
                InnerRadiusTextBox.Text = string.Empty;
                OuterRadiusTextBox.Text = string.Empty;
                ChildSoundTextBox.Text = string.Empty;
                CategoryTextBox.Text = string.Empty;
                Unk01TextBox.Text = string.Empty;
                StartTimeUpDown.Value = 0;
                EndTimeUpDown.Value = 0;
                FrequencyUpDown.Value = 0;
                Unk07UpDown.Value = 0;
                Unk08UpDown.Value = 0;
                Unk09UpDown.Value = 0;
                Unk10UpDown.Value = 0;
                Unk11UpDown.Value = 0;
                Unk12UpDown.Value = 0;
                Unk13UpDown.Value = 0;
                Flags2TextBox.Text = string.Empty;
                Flags4TextBox.Text = string.Empty;
                Flags5TextBox.Text = string.Empty;
                VariablesTextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                AddToProjectButton.Enabled = CurrentEmitter?.RelFile != null ? !ProjectForm.AudioFileExistsInProject(CurrentEmitter.RelFile) : false;
                DeleteButton.Enabled = !AddToProjectButton.Enabled;

                populatingui = true;
                var e = CurrentEmitter.AudioEmitter;
                NameTextBox.Text = e.NameHash.ToString();
                PositionTextBox.Text = FloatUtil.GetVector3String(e.Position);
                InnerRadiusTextBox.Text = FloatUtil.ToString(e.MinDist);
                OuterRadiusTextBox.Text = FloatUtil.ToString(e.MaxDist);
                ChildSoundTextBox.Text = e.ChildSound.ToString();
                CategoryTextBox.Text = e.Category.ToString();
                Unk01TextBox.Text = FloatUtil.ToString(e.Weight);
                StartTimeUpDown.Value = e.MinTimeMinutes;
                EndTimeUpDown.Value = e.MaxTimeMinutes;
                FrequencyUpDown.Value = e.MinRepeatTime;
                Unk07UpDown.Value = e.MinRepeatTimeVariance;
                Unk08UpDown.Value = e.SpawnHeight;
                Unk09UpDown.Value = e.PositionUsage;
                Unk10UpDown.Value = e.MaxLocalInstances;
                Unk11UpDown.Value = e.MaxGlobalInstances;
                Unk12UpDown.Value = e.BlockabilityFactor;
                Unk13UpDown.Value = e.MaxPathDepth;
                Flags2TextBox.Text = e.Flags.Hex;
                Flags4TextBox.Text = FloatUtil.ToString(e.LastPlayTime);
                Flags5TextBox.Text = FloatUtil.ToString(e.DynamicBankID);

                StringBuilder sb = new StringBuilder();
                if (e.Conditions != null)
                {
                    foreach (var extparam in e.Conditions)
                    {
                        sb.Append(extparam.Name.ToString());
                        sb.Append(", ");
                        sb.Append(FloatUtil.ToString(extparam.Value));
                        sb.Append(", ");
                        sb.Append(extparam.ConditionType.ToString());
                        sb.AppendLine();
                    }
                }
                VariablesTextBox.Text = sb.ToString();

                populatingui = false;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectObject(CurrentEmitter);
                }

            }
        }

        private void ProjectItemChanged()
        {
            CurrentEmitter?.UpdateFromEmitter();//also update the placement wrapper

            if (CurrentEmitter?.RelFile != null)
            {
                ProjectForm.SetAudioFileHasChanged(true);
            }
        }


        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint hash = 0;
            string name = NameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.AudioEmitter.NameHash != hash)
            {
                CurrentEmitter.AudioEmitter.Name = NameTextBox.Text;
                CurrentEmitter.AudioEmitter.NameHash = hash;

                ProjectItemChanged();
                UpdateFormTitle();
            }
        }

        private void PositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            var vec = FloatUtil.ParseVector3String(PositionTextBox.Text);
            if (CurrentEmitter.AudioEmitter.Position != vec)
            {
                CurrentEmitter.AudioEmitter.Position = vec;

                ProjectItemChanged();

                //var wf = ProjectForm.WorldForm;
                //if (wf != null)
                //{
                //    wf.BeginInvoke(new Action(() =>
                //    {
                //        wf.SetWidgetPosition(CurrentEmitter.Position, true);
                //    }));
                //}
            }
        }

        private void InnerRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            float rad = FloatUtil.Parse(InnerRadiusTextBox.Text);
            if (CurrentEmitter.AudioEmitter.MinDist != rad)
            {
                CurrentEmitter.AudioEmitter.MinDist = rad;

                ProjectItemChanged();
            }
        }

        private void OuterRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            float rad = FloatUtil.Parse(OuterRadiusTextBox.Text);
            if (CurrentEmitter.AudioEmitter.MaxDist != rad)
            {
                CurrentEmitter.AudioEmitter.MaxDist = rad;

                ProjectItemChanged();
            }
        }

        private void ChildSoundTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint hash = 0;
            string name = ChildSoundTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.AudioEmitter.ChildSound != hash)
            {
                CurrentEmitter.AudioEmitter.ChildSound = hash;

                ProjectItemChanged();
            }
        }

        private void CategoryTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint hash = 0;
            string name = CategoryTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.AudioEmitter.Category != hash)
            {
                CurrentEmitter.AudioEmitter.Category = hash;

                ProjectItemChanged();
            }
        }

        private void Unk01TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            float unk = FloatUtil.Parse(Unk01TextBox.Text);
            if (CurrentEmitter.AudioEmitter.Weight != unk)
            {
                CurrentEmitter.AudioEmitter.Weight = unk;

                ProjectItemChanged();
            }
        }

        private void StartTimeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            ushort unk = (ushort)StartTimeUpDown.Value;
            if (CurrentEmitter.AudioEmitter.MinTimeMinutes != unk)
            {
                CurrentEmitter.AudioEmitter.MinTimeMinutes = unk;

                ProjectItemChanged();
            }
        }

        private void EndTimeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            ushort unk = (ushort)EndTimeUpDown.Value;
            if (CurrentEmitter.AudioEmitter.MaxTimeMinutes != unk)
            {
                CurrentEmitter.AudioEmitter.MaxTimeMinutes = unk;

                ProjectItemChanged();
            }
        }

        private void FrequencyUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            ushort unk = (ushort)FrequencyUpDown.Value;
            if (CurrentEmitter.AudioEmitter.MinRepeatTime != unk)
            {
                CurrentEmitter.AudioEmitter.MinRepeatTime = unk;

                ProjectItemChanged();
            }
        }

        private void Unk07UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            ushort unk = (ushort)Unk07UpDown.Value;
            if (CurrentEmitter.AudioEmitter.MinRepeatTimeVariance != unk)
            {
                CurrentEmitter.AudioEmitter.MinRepeatTimeVariance = unk;

                ProjectItemChanged();
            }
        }

        private void Unk08UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk08UpDown.Value;
            if (CurrentEmitter.AudioEmitter.SpawnHeight != unk)
            {
                CurrentEmitter.AudioEmitter.SpawnHeight = unk;

                ProjectItemChanged();
            }
        }

        private void Unk09UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk09UpDown.Value;
            if (CurrentEmitter.AudioEmitter.PositionUsage != unk)
            {
                CurrentEmitter.AudioEmitter.PositionUsage = unk;

                ProjectItemChanged();
            }
        }

        private void Unk10UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk10UpDown.Value;
            if (CurrentEmitter.AudioEmitter.MaxLocalInstances != unk)
            {
                CurrentEmitter.AudioEmitter.MaxLocalInstances = unk;

                ProjectItemChanged();
            }
        }

        private void Unk11UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk11UpDown.Value;
            if (CurrentEmitter.AudioEmitter.MaxGlobalInstances != unk)
            {
                CurrentEmitter.AudioEmitter.MaxGlobalInstances = unk;

                ProjectItemChanged();
            }
        }

        private void Unk12UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk12UpDown.Value;
            if (CurrentEmitter.AudioEmitter.BlockabilityFactor != unk)
            {
                CurrentEmitter.AudioEmitter.BlockabilityFactor = unk;

                ProjectItemChanged();
            }
        }

        private void Unk13UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk13UpDown.Value;
            if (CurrentEmitter.AudioEmitter.MaxPathDepth != unk)
            {
                CurrentEmitter.AudioEmitter.MaxPathDepth = unk;

                ProjectItemChanged();
            }
        }

        private void Flags2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags2TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentEmitter.AudioEmitter.Flags != flags)
                {
                    CurrentEmitter.AudioEmitter.Flags = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void Flags4TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags4TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentEmitter.AudioEmitter.LastPlayTime != flags)
                {
                    CurrentEmitter.AudioEmitter.LastPlayTime = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void Flags5TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags5TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentEmitter.AudioEmitter.DynamicBankID != flags)
                {
                    CurrentEmitter.AudioEmitter.DynamicBankID = (int)flags;

                    ProjectItemChanged();
                }
            }
        }

        private void VariablesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            var paramstrs = VariablesTextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (paramstrs?.Length > 0)
            {
                var paramlist = new List<Dat151AmbientRule.Condition>();
                foreach (var paramstr in paramstrs)
                {
                    var paramvals = paramstr.Split(',');
                    if (paramvals?.Length == 3)
                    {
                        var param = new Dat151AmbientRule.Condition();
                        var hashstr = paramvals[0].Trim();
                        var valstr = paramvals[1].Trim();
                        var flgstr = paramvals[2].Trim();
                        uint hash = 0;
                        if (!uint.TryParse(hashstr, out hash))//don't re-hash hashes
                        {
                            hash = JenkHash.GenHash(hashstr);
                            JenkIndex.Ensure(hashstr);
                        }
                        uint flags = 0;
                        uint.TryParse(flgstr, out flags);
                        param.Name = hash;
                        param.Value = FloatUtil.Parse(valstr);
                        param.ConditionType = (byte)flags;
                        paramlist.Add(param);
                    }
                }

                CurrentEmitter.AudioEmitter.Conditions = paramlist.ToArray();
                CurrentEmitter.AudioEmitter.NumConditions = (ushort)paramlist.Count;

                ProjectItemChanged();
            }
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentEmitter == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentEmitter.Position, CurrentEmitter.AudioZone.PositioningZoneSize);
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentEmitter);
            ProjectForm.AddAudioFileToProject(CurrentEmitter.RelFile);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentEmitter);
            ProjectForm.DeleteAudioEmitter();
        }

    }
}
