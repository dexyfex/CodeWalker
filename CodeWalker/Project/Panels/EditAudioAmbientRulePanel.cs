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
    public partial class EditAudioAmbientRulePanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public AudioPlacement CurrentRule { get; set; }

        private bool populatingui = false;


        public EditAudioAmbientRulePanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();

            ExplicitSpawnCombo.Items.Clear();
            ExplicitSpawnCombo.Items.Add(Dat151AmbientRule.ExplicitSpawnType.Disabled);
            ExplicitSpawnCombo.Items.Add(Dat151AmbientRule.ExplicitSpawnType.WorldRelative);
            ExplicitSpawnCombo.Items.Add(Dat151AmbientRule.ExplicitSpawnType.InteriorRelative);
        }

        public void SetRule(AudioPlacement rule)
        {
            CurrentRule = rule;
            Tag = rule;
            UpdateFormTitle();
            UpdateUI();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentRule?.NameHash.ToString() ?? "";
        }


        private void UpdateUI()
        {
            if (CurrentRule?.AmbientRule == null)
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
                WeightTextBox.Text = string.Empty;
                StartTimeUpDown.Value = 0;
                EndTimeUpDown.Value = 0;
                MinRepeatUpDown.Value = 0;
                MinRepeatVarUpDown.Value = 0;
                SpawnHeightUpDown.Value = 0;
                ExplicitSpawnCombo.SelectedItem = null;
                MaxLocalInstsUpDown.Value = 0;
                MaxGlobalInstsUpDown.Value = 0;
                BlockabilityUpDown.Value = 0;
                MaxPathDepthUpDown.Value = 0;
                FlagsTextBox.Text = string.Empty;
                LastPlayedTextBox.Text = string.Empty;
                BankIDTextBox.Text = string.Empty;
                ConditionsTextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                AddToProjectButton.Enabled = CurrentRule?.RelFile != null ? !ProjectForm.AudioFileExistsInProject(CurrentRule.RelFile) : false;
                DeleteButton.Enabled = !AddToProjectButton.Enabled;

                populatingui = true;
                var e = CurrentRule.AmbientRule;
                NameTextBox.Text = e.NameHash.ToString();
                PositionTextBox.Text = FloatUtil.GetVector3String(e.Position);
                InnerRadiusTextBox.Text = FloatUtil.ToString(e.MinDist);
                OuterRadiusTextBox.Text = FloatUtil.ToString(e.MaxDist);
                ChildSoundTextBox.Text = e.ChildSound.ToString();
                CategoryTextBox.Text = e.Category.ToString();
                WeightTextBox.Text = FloatUtil.ToString(e.Weight);
                StartTimeUpDown.Value = e.MinTimeMinutes;
                EndTimeUpDown.Value = e.MaxTimeMinutes;
                MinRepeatUpDown.Value = e.MinRepeatTime;
                MinRepeatVarUpDown.Value = e.MinRepeatTimeVariance;
                SpawnHeightUpDown.Value = e.SpawnHeight;
                ExplicitSpawnCombo.SelectedItem = e.ExplicitSpawn;
                MaxLocalInstsUpDown.Value = e.MaxLocalInstances;
                MaxGlobalInstsUpDown.Value = e.MaxGlobalInstances;
                BlockabilityUpDown.Value = e.BlockabilityFactor;
                MaxPathDepthUpDown.Value = e.MaxPathDepth;
                FlagsTextBox.Text = e.Flags.Hex;
                LastPlayedTextBox.Text = FloatUtil.ToString(e.LastPlayTime);
                BankIDTextBox.Text = FloatUtil.ToString(e.DynamicBankID);

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
                ConditionsTextBox.Text = sb.ToString();

                populatingui = false;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectObject(CurrentRule);
                }

            }
        }

        private void ProjectItemChanged()
        {
            CurrentRule?.UpdateFromAmbientRule();//also update the placement wrapper

            if (CurrentRule?.RelFile != null)
            {
                ProjectForm.SetAudioFileHasChanged(true);
            }
        }


        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            uint hash = 0;
            string name = NameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRule.AmbientRule.NameHash != hash)
            {
                CurrentRule.AmbientRule.Name = NameTextBox.Text;
                CurrentRule.AmbientRule.NameHash = hash;

                ProjectItemChanged();
                UpdateFormTitle();
            }
        }

        private void PositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            var vec = FloatUtil.ParseVector3String(PositionTextBox.Text);
            if (CurrentRule.AmbientRule.Position != vec)
            {
                CurrentRule.AmbientRule.Position = vec;

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
            if (CurrentRule?.AmbientRule == null) return;

            float rad = FloatUtil.Parse(InnerRadiusTextBox.Text);
            if (CurrentRule.AmbientRule.MinDist != rad)
            {
                CurrentRule.AmbientRule.MinDist = rad;

                ProjectItemChanged();
            }
        }

        private void OuterRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            float rad = FloatUtil.Parse(OuterRadiusTextBox.Text);
            if (CurrentRule.AmbientRule.MaxDist != rad)
            {
                CurrentRule.AmbientRule.MaxDist = rad;

                ProjectItemChanged();
            }
        }

        private void ChildSoundTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            uint hash = 0;
            string name = ChildSoundTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRule.AmbientRule.ChildSound != hash)
            {
                CurrentRule.AmbientRule.ChildSound = hash;

                ProjectItemChanged();
            }
        }

        private void CategoryTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            uint hash = 0;
            string name = CategoryTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRule.AmbientRule.Category != hash)
            {
                CurrentRule.AmbientRule.Category = hash;

                ProjectItemChanged();
            }
        }

        private void WeightTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            float unk = FloatUtil.Parse(WeightTextBox.Text);
            if (CurrentRule.AmbientRule.Weight != unk)
            {
                CurrentRule.AmbientRule.Weight = unk;

                ProjectItemChanged();
            }
        }

        private void StartTimeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            ushort unk = (ushort)StartTimeUpDown.Value;
            if (CurrentRule.AmbientRule.MinTimeMinutes != unk)
            {
                CurrentRule.AmbientRule.MinTimeMinutes = unk;

                ProjectItemChanged();
            }
        }

        private void EndTimeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            ushort unk = (ushort)EndTimeUpDown.Value;
            if (CurrentRule.AmbientRule.MaxTimeMinutes != unk)
            {
                CurrentRule.AmbientRule.MaxTimeMinutes = unk;

                ProjectItemChanged();
            }
        }

        private void MinRepeatUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            ushort unk = (ushort)MinRepeatUpDown.Value;
            if (CurrentRule.AmbientRule.MinRepeatTime != unk)
            {
                CurrentRule.AmbientRule.MinRepeatTime = unk;

                ProjectItemChanged();
            }
        }

        private void MinRepeatVarUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            ushort unk = (ushort)MinRepeatVarUpDown.Value;
            if (CurrentRule.AmbientRule.MinRepeatTimeVariance != unk)
            {
                CurrentRule.AmbientRule.MinRepeatTimeVariance = unk;

                ProjectItemChanged();
            }
        }

        private void SpawnHeightUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            byte unk = (byte)SpawnHeightUpDown.Value;
            if (CurrentRule.AmbientRule.SpawnHeight != unk)
            {
                CurrentRule.AmbientRule.SpawnHeight = unk;

                ProjectItemChanged();
            }
        }

        private void ExplicitSpawnCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            var val = (Dat151AmbientRule.ExplicitSpawnType)ExplicitSpawnCombo.SelectedItem;
            if (CurrentRule.AmbientRule.ExplicitSpawn != val)
            {
                CurrentRule.AmbientRule.ExplicitSpawn = val;

                ProjectItemChanged();
            }
        }

        private void MaxLocalInstsUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            byte unk = (byte)MaxLocalInstsUpDown.Value;
            if (CurrentRule.AmbientRule.MaxLocalInstances != unk)
            {
                CurrentRule.AmbientRule.MaxLocalInstances = unk;

                ProjectItemChanged();
            }
        }

        private void MaxGlobalInstsUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            byte unk = (byte)MaxGlobalInstsUpDown.Value;
            if (CurrentRule.AmbientRule.MaxGlobalInstances != unk)
            {
                CurrentRule.AmbientRule.MaxGlobalInstances = unk;

                ProjectItemChanged();
            }
        }

        private void BlockabilityUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            byte unk = (byte)BlockabilityUpDown.Value;
            if (CurrentRule.AmbientRule.BlockabilityFactor != unk)
            {
                CurrentRule.AmbientRule.BlockabilityFactor = unk;

                ProjectItemChanged();
            }
        }

        private void MaxPathDepthUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            byte unk = (byte)MaxPathDepthUpDown.Value;
            if (CurrentRule.AmbientRule.MaxPathDepth != unk)
            {
                CurrentRule.AmbientRule.MaxPathDepth = unk;

                ProjectItemChanged();
            }
        }

        private void FlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            uint flags = 0;
            if (uint.TryParse(FlagsTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentRule.AmbientRule.Flags != flags)
                {
                    CurrentRule.AmbientRule.Flags = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void LastPlayedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            uint flags = 0;
            if (uint.TryParse(LastPlayedTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentRule.AmbientRule.LastPlayTime != flags)
                {
                    CurrentRule.AmbientRule.LastPlayTime = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void BankIDTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            uint flags = 0;
            if (uint.TryParse(BankIDTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentRule.AmbientRule.DynamicBankID != flags)
                {
                    CurrentRule.AmbientRule.DynamicBankID = (int)flags;

                    ProjectItemChanged();
                }
            }
        }

        private void ConditionsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRule?.AmbientRule == null) return;

            var paramstrs = ConditionsTextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
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

                CurrentRule.AmbientRule.Conditions = paramlist.ToArray();
                CurrentRule.AmbientRule.NumConditions = (ushort)paramlist.Count;

                ProjectItemChanged();
            }
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentRule == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentRule.Position, SharpDX.Vector3.One * CurrentRule.OuterRadius);
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentRule);
            ProjectForm.AddAudioFileToProject(CurrentRule.RelFile);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentRule);
            ProjectForm.DeleteAudioAmbientRule();
        }
    }
}
