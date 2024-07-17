using CodeWalker.GameFiles;
using CodeWalker.World;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CodeWalker.Project.Panels
{
    public partial class EditAudioAmbientZonePanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public AudioPlacement CurrentZone { get; set; }

        private bool populatingui = false;


        public EditAudioAmbientZonePanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetZone(AudioPlacement zone)
        {
            CurrentZone = zone;
            Tag = zone;
            UpdateFormTitle();
            UpdateUI();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentZone?.NameHash.ToString() ?? "";
        }


        private void UpdateUI()
        {

            if (CurrentZone?.AmbientZone == null)
            {
                AddToProjectButton.Enabled = false;
                DeleteButton.Enabled = false;

                populatingui = true;
                NameTextBox.Text = string.Empty;
                ShapeComboBox.Text = string.Empty;
                FlagsTextBox.Text = string.Empty;

                ActivationZoneCentreTextBox.Text = string.Empty;
                ActivationSizeTextBox.Text = string.Empty;
                ActivationRotationOffsetTextBox.Text = string.Empty;
                ActivationSizeScaleTextBox.Text = string.Empty;

                PositioningCentreTextBox.Text = string.Empty;
                PositioningSizeTextBox.Text = string.Empty;
                PositioningRotationOffsetTextBox.Text = string.Empty;
                PositioningSizeScaleTextBox.Text = string.Empty;

                BuiltUpFactorTextBox.Text = string.Empty;
                MinPedDensityTextBox.Text = string.Empty;
                MaxPedDensityTextBox.Text = string.Empty;
                PedDensityTODTextBox.Text = string.Empty;
                PedDensityScalarTextBox.Text = string.Empty;
                ZoneWaterCalculationTextBox.Text = string.Empty;
                PositioningRotationAngleTextBox.Text = string.Empty;
                ActivationZoneRotationAngleTextBox.Text = string.Empty;
                MinWindInfluenceTextBox.Text = string.Empty;

                WindElevationSoundsTextBox.Text = string.Empty;
                RandomRadioSettingsTextBox.Text = string.Empty;
                EnviromentRuleTextBox.Text = string.Empty;
                AudioSceneTextBox.Text = string.Empty;
                RulesTextBox.Text = string.Empty;
                DirAmbiencesTextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                AddToProjectButton.Enabled = CurrentZone?.RelFile != null ? !ProjectForm.AudioFileExistsInProject(CurrentZone.RelFile) : false;
                DeleteButton.Enabled = !AddToProjectButton.Enabled;

                populatingui = true;
                var z = CurrentZone.AmbientZone;
                NameTextBox.Text = z.NameHash.ToString();
                ShapeComboBox.Text = z.Shape.ToString();
                FlagsTextBox.Text = z.Flags.Hex;
                ActivationZoneCentreTextBox.Text = FloatUtil.GetVector3String(z.ActivationZoneCentre);
                ActivationSizeTextBox.Text = FloatUtil.GetVector3String(z.ActivationZoneSize);
                ActivationRotationOffsetTextBox.Text = FloatUtil.GetVector3String(z.ActivationZonePostRotationOffset);
                ActivationSizeScaleTextBox.Text = FloatUtil.GetVector3String(z.ActivationZoneSizeScale);

                PositioningCentreTextBox.Text = FloatUtil.GetVector3String(z.PositioningZoneCentre);
                PositioningSizeTextBox.Text = FloatUtil.GetVector3String(z.PositioningZoneSize);
                PositioningRotationOffsetTextBox.Text = FloatUtil.GetVector3String(z.PositioningZonePostRotationOffset);
                PositioningSizeScaleTextBox.Text = FloatUtil.GetVector3String(z.PositioningZoneSizeScale);

                BuiltUpFactorTextBox.Text = z.BuiltUpFactor.ToString();
                MinPedDensityTextBox.Text = z.MinPedDensity.ToString();
                MaxPedDensityTextBox.Text = z.MaxPedDensity.ToString();
                PedDensityTODTextBox.Text = z.PedDensityTOD.ToString();
                PedDensityScalarTextBox.Text = z.PedDensityScalar.ToString();
                ZoneWaterCalculationTextBox.Text = z.ZoneWaterCalculation.ToString();
                PositioningRotationAngleTextBox.Text = z.PositioningZoneRotationAngle.ToString();
                ActivationZoneRotationAngleTextBox.Text = z.ActivationZoneRotationAngle.ToString();
                MinWindInfluenceTextBox.Text = z.MinWindInfluence.ToString();

                WindElevationSoundsTextBox.Text = z.WindElevationSounds.ToString();
                RandomRadioSettingsTextBox.Text = z.RandomisedRadioSettings.ToString();
                EnviromentRuleTextBox.Text = z.EnvironmentRule.ToString();
                AudioSceneTextBox.Text = z.AudioScene.ToString();

                StringBuilder sb = new StringBuilder();
                if (z.Rules != null)
                {
                    foreach (var hash in z.Rules)
                    {
                        sb.AppendLine(hash.ToString());
                    }
                }
                RulesTextBox.Text = sb.ToString();

                sb.Clear();
                if (z.DirAmbiences != null)
                {
                    foreach (var extparam in z.DirAmbiences)
                    {
                        sb.Append(extparam.Name.ToString());
                        sb.Append(", ");
                        sb.Append(FloatUtil.ToString(extparam.Volume));
                        sb.AppendLine();
                    }
                }
                DirAmbiencesTextBox.Text = sb.ToString();

                populatingui = false;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectObject(CurrentZone);
                }

            }

        }


        private void ProjectItemChanged()
        {
            CurrentZone?.UpdateFromAmbientZone();//also update the placement wrapper

            if (CurrentZone?.RelFile != null)
            {
                ProjectForm.SetAudioFileHasChanged(true);
            }
        }


        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            uint hash = 0;
            string name = NameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentZone.AmbientZone.NameHash != hash)
            {
                CurrentZone.AmbientZone.Name = NameTextBox.Text;
                CurrentZone.AmbientZone.NameHash = hash;

                ProjectItemChanged();
                UpdateFormTitle();
            }

        }

        private void ShapeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            Dat151ZoneShape shape = Dat151ZoneShape.Box;
            if (Enum.TryParse<Dat151ZoneShape>(ShapeComboBox.Text, out shape))
            {
                if (CurrentZone.AmbientZone.Shape != shape)
                {
                    CurrentZone.AmbientZone.Shape = shape;

                    ProjectItemChanged();
                }
            }

        }

        private void PositioningCentreTextBoxTextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.ParseVector3String(PositioningCentreTextBox.Text);
            if (CurrentZone.AmbientZone.PositioningZoneCentre != vec)
            {
                CurrentZone.AmbientZone.PositioningZoneCentre = vec;

                ProjectItemChanged();

            }
        }

        private void OuterPosTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.ParseVector3String(PositioningCentreTextBox.Text);
            if (CurrentZone.AmbientZone.ActivationZoneCentre != vec)
            {
                CurrentZone.AmbientZone.ActivationZoneCentre = vec;

                ProjectItemChanged();
            }
        }

        private void ActivationZoneRotationAngleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            uint ang;
            if (uint.TryParse(ActivationZoneRotationAngleTextBox.Text, out ang))
            {
                if (CurrentZone.AmbientZone.ActivationZoneRotationAngle != ang)
                {
                    CurrentZone.AmbientZone.ActivationZoneRotationAngle = (ushort)ang;

                    ProjectItemChanged();
                }
            }
        }

        private void FlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            uint flags = 0;
            if (uint.TryParse(FlagsTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentZone.AmbientZone.Flags != flags)
                {
                    CurrentZone.AmbientZone.Flags = flags;

                    ProjectItemChanged();
                }
            }

        }

        private void RandomisedRadioSettings_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var hashstr = RandomRadioSettingsTextBox.Text;
            uint hash = 0;
            if (!uint.TryParse(hashstr, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(hashstr);
                JenkIndex.Ensure(hashstr);
            }

            if (CurrentZone.AmbientZone.RandomisedRadioSettings != hash)
            {
                CurrentZone.AmbientZone.RandomisedRadioSettings = hash;

                ProjectItemChanged();
            }
        }

        private void EnviromentRuleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var hashstr = EnviromentRuleTextBox.Text;
            uint hash = 0;
            if (!uint.TryParse(hashstr, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(hashstr);
                JenkIndex.Ensure(hashstr);
            }

            if (CurrentZone.AmbientZone.EnvironmentRule != hash)
            {
                CurrentZone.AmbientZone.EnvironmentRule = hash;

                ProjectItemChanged();
            }
        }

        private void AudioSceneTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var hashstr = AudioSceneTextBox.Text;
            uint hash = 0;
            if (!uint.TryParse(hashstr, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(hashstr);
                JenkIndex.Ensure(hashstr);
            }

            if (CurrentZone.AmbientZone.AudioScene != hash)
            {
                CurrentZone.AmbientZone.AudioScene = hash;

                ProjectItemChanged();
            }
        }

        private void RulesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var hashstrs = RulesTextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (hashstrs?.Length > 0)
            {
                var hashlist = new List<MetaHash>();
                foreach (var hashstr in hashstrs)
                {
                    uint hash = 0;
                    if (!uint.TryParse(hashstr, out hash))//don't re-hash hashes
                    {
                        hash = JenkHash.GenHash(hashstr);
                        JenkIndex.Ensure(hashstr);
                    }
                    hashlist.Add(hash);
                }

                CurrentZone.AmbientZone.Rules = hashlist.ToArray();
                CurrentZone.AmbientZone.NumRules = (byte)hashlist.Count;

                ProjectItemChanged();
            }
        }

        private void DirAmbiencesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var paramstrs = DirAmbiencesTextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (paramstrs?.Length > 0)
            {
                var paramlist = new List<Dat151AmbientZone.DirAmbience>();
                foreach (var paramstr in paramstrs)
                {
                    var paramvals = paramstr.Split(',');
                    if (paramvals?.Length == 2)
                    {
                        var param = new Dat151AmbientZone.DirAmbience();
                        var hashstr = paramvals[0].Trim();
                        var valstr = paramvals[1].Trim();
                        uint hash = 0;
                        if (!uint.TryParse(hashstr, out hash))//don't re-hash hashes
                        {
                            hash = JenkHash.GenHash(hashstr);
                            JenkIndex.Ensure(hashstr);
                        }
                        param.Name = hash;
                        param.Volume = FloatUtil.Parse(valstr);
                        paramlist.Add(param);
                    }
                }

                CurrentZone.AmbientZone.DirAmbiences = paramlist.ToArray();
                CurrentZone.AmbientZone.NumDirAmbiences = (byte)paramlist.Count;

                ProjectItemChanged();
            }
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentZone == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentZone.Position, CurrentZone.AmbientZone.PositioningZoneSize);
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentZone);
            ProjectForm.AddAudioFileToProject(CurrentZone.RelFile);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentZone);
            ProjectForm.DeleteAudioAmbientZone();
        }

        private void BuiltUpFactorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.Parse(BuiltUpFactorTextBox.Text);
            if (CurrentZone.AmbientZone.BuiltUpFactor != vec)
            {
                CurrentZone.AmbientZone.BuiltUpFactor = vec;

                ProjectItemChanged();
            }
        }

        private void PedDensityTODTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.Parse(PedDensityTODTextBox.Text);
            if (CurrentZone.AmbientZone.PedDensityTOD != vec)
            {
                CurrentZone.AmbientZone.PedDensityTOD = (uint)vec;

                ProjectItemChanged();
            }
        }

        private void MinPedDensityTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.Parse(MinPedDensityTextBox.Text);
            if (CurrentZone.AmbientZone.MinPedDensity != vec)
            {
                CurrentZone.AmbientZone.MinPedDensity = vec;

                ProjectItemChanged();
            }
        }

        private void PedDensityScalarTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.Parse(PedDensityScalarTextBox.Text);
            if (CurrentZone.AmbientZone.PedDensityScalar != vec)
            {
                CurrentZone.AmbientZone.PedDensityScalar = vec;

                ProjectItemChanged();
            }
        }

        private void MaxPedDensityTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.Parse(MaxPedDensityTextBox.Text);
            if (CurrentZone.AmbientZone.MaxPedDensity != vec)
            {
                CurrentZone.AmbientZone.MaxPedDensity = vec;

                ProjectItemChanged();
            }
        }

        private void ZoneWaterCalculationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.Parse(ZoneWaterCalculationTextBox.Text);
            if (CurrentZone.AmbientZone.ZoneWaterCalculation != vec)
            {
                CurrentZone.AmbientZone.ZoneWaterCalculation = (byte)vec;

                ProjectItemChanged();
            }
        }

        private void PositioningSizeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.ParseVector3String(PositioningSizeTextBox.Text);
            if (CurrentZone.AmbientZone.PositioningZoneSize != vec)
            {
                CurrentZone.AmbientZone.PositioningZoneSize = vec;

                ProjectItemChanged();
            }
        }

        private void PositioningRotationAngleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            uint ang = 0;
            if (uint.TryParse(PositioningRotationAngleTextBox.Text, out ang))
            {
                if (CurrentZone.AmbientZone.PositioningZoneRotationAngle != ang)
                {
                    CurrentZone.AmbientZone.PositioningZoneRotationAngle = (ushort)ang;

                    ProjectItemChanged();
                }
            }
        }

        private void PositioningRotationOffsetTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.ParseVector3String(PositioningRotationOffsetTextBox.Text);
            if (CurrentZone.AmbientZone.PositioningZonePostRotationOffset != vec)
            {
                CurrentZone.AmbientZone.PositioningZonePostRotationOffset = vec;

                ProjectItemChanged();
            }
        }

        private void PositioningSizeScaleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.ParseVector3String(PositioningSizeTextBox.Text);
            if (CurrentZone.AmbientZone.PositioningZoneSizeScale != vec)
            {
                CurrentZone.AmbientZone.PositioningZoneSizeScale = vec;

                ProjectItemChanged();
            }
        }

        private void ActivationSizeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.ParseVector3String(ActivationSizeTextBox.Text);
            if (CurrentZone.AmbientZone.ActivationZoneSize != vec)
            {
                CurrentZone.AmbientZone.ActivationZoneSize = vec;

                ProjectItemChanged();
            }
        }

        private void ActivationRotationOffsetTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.ParseVector3String(ActivationRotationOffsetTextBox.Text);
            if (CurrentZone.AmbientZone.ActivationZonePostRotationOffset != vec)
            {
                CurrentZone.AmbientZone.ActivationZonePostRotationOffset = vec;

                ProjectItemChanged();
            }
        }

        private void ActivationSizeScaleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.ParseVector3String(ActivationSizeScaleTextBox.Text);
            if (CurrentZone.AmbientZone.ActivationZoneSizeScale != vec)
            {
                CurrentZone.AmbientZone.ActivationZoneSizeScale = vec;

                ProjectItemChanged();
            }
        }

        private void MinWindInfluenceTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.Parse(MinWindInfluenceTextBox.Text);
            if (CurrentZone.AmbientZone.MinWindInfluence != vec)
            {
                CurrentZone.AmbientZone.MinWindInfluence = vec;

                ProjectItemChanged();
            }
        }

        private void MaxWindInfluenceTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.Parse(MaxWindInfluenceTextBox.Text);
            if (CurrentZone.AmbientZone.MaxWindInfluence != vec)
            {
                CurrentZone.AmbientZone.MaxWindInfluence = vec;

                ProjectItemChanged();
            }
        }

        private void WindElevationSoundsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AmbientZone == null) return;

            var vec = FloatUtil.Parse(WindElevationSoundsTextBox.Text);
            if (CurrentZone.AmbientZone.WindElevationSounds != vec)
            {
                CurrentZone.AmbientZone.WindElevationSounds = (uint)vec;

                ProjectItemChanged();
            }
        }

        private void RulesTextBox_Enter(object sender, EventArgs e)
        {
            if (RulesTextBox.Text.Length == 0) return;
            SelectRuleButton.Enabled = true;
        }

        private void SelectRuleButton_Click(object sender, EventArgs e)
        {
            var txt = RulesTextBox.Text;
            var start = RulesTextBox.SelectionStart;
            if (txt.Length == 0) return;
            if (start >= txt.Length) start = txt.Length - 1;
            for (int i = start; i >= 0; i--)
            {
                if (txt[i] == '\n') break;
                start = i;
            }
            var end = start;
            for (int i = start; i < txt.Length; i++)
            {
                if ((txt[i] == '\n') || (txt[i] == '\r')) break;
                end = i;
            }
            var str = txt.Substring(start, end - start + 1);
            if (string.IsNullOrEmpty(str)) return;

            uint hash;
            if (!uint.TryParse(str, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(str);
            }

            ProjectForm?.ProjectExplorer?.TrySelectAudioAmbientRuleTreeNode(hash);

        }
    }
}
