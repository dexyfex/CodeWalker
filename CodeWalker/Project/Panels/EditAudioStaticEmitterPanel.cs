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
    public partial class EditAudioStaticEmitterPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public AudioPlacement CurrentEmitter { get; set; }

        private bool populatingui = false;


        public EditAudioStaticEmitterPanel(ProjectForm owner)
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
            if (CurrentEmitter?.StaticEmitter == null)
            {
                AddToProjectButton.Enabled = false;
                DeleteButton.Enabled = false;

                populatingui = true;
                NameTextBox.Text = string.Empty;
                FlagsTextBox.Text = string.Empty;
                PositionTextBox.Text = string.Empty;
                InnerRadiusTextBox.Text = string.Empty;
                OuterRadiusTextBox.Text = string.Empty;
                ChildSoundTextBox.Text = string.Empty;
                RadioStationTextBox.Text = string.Empty;
                RadioScoreTextBox.Text = string.Empty;
                InteriorTextBox.Text = string.Empty;
                RoomTextBox.Text = string.Empty;
                AlarmTextBox.Text = string.Empty;
                OnBreakTextBox.Text = string.Empty;
                VolumeTextBox.Text = string.Empty;
                StartTimeUpDown.Value = 0;
                EndTimeUpDown.Value = 0;
                LPFCutoffUpDown.Value = 0;
                HPFCutoffUpDown.Value = 0;
                RolloffFactorUpDown.Value = 0;
                MaxLeakageTextBox.Text = string.Empty;
                MinLeakageDistUpDown.Value = 0;
                MaxLeakageDistUpDown.Value = 0;
                MaxPathDepthUpDown.Value = 0;
                SmallReverbUpDown.Value = 0;
                MediumReverbUpDown.Value = 0;
                LargeReverbUpDown.Value = 0;
                BrokenHealthTextBox.Text = string.Empty;
                UndamagedHealthTextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                AddToProjectButton.Enabled = CurrentEmitter?.RelFile != null ? !ProjectForm.AudioFileExistsInProject(CurrentEmitter.RelFile) : false;
                DeleteButton.Enabled = !AddToProjectButton.Enabled;

                populatingui = true;
                var e = CurrentEmitter.StaticEmitter;
                NameTextBox.Text = e.NameHash.ToString();
                FlagsTextBox.Text = e.Flags.Hex;
                PositionTextBox.Text = FloatUtil.GetVector3String(e.Position);
                InnerRadiusTextBox.Text = FloatUtil.ToString(e.MinDistance);
                OuterRadiusTextBox.Text = FloatUtil.ToString(e.MaxDistance);
                ChildSoundTextBox.Text = e.ChildSound.ToString();
                RadioStationTextBox.Text = e.RadioStation.ToString();
                RadioScoreTextBox.Text = e.RadioStationForScore.ToString();
                InteriorTextBox.Text = e.Interior.ToString();
                RoomTextBox.Text = e.Room.ToString();
                AlarmTextBox.Text = e.Alarm.ToString();
                OnBreakTextBox.Text = e.OnBreakOneShot.ToString();
                VolumeTextBox.Text = e.EmittedVolume.ToString();
                StartTimeUpDown.Value = e.MinTimeMinutes;
                EndTimeUpDown.Value = e.MaxTimeMinutes;
                LPFCutoffUpDown.Value = e.LPFCutoff;
                HPFCutoffUpDown.Value = e.HPFCutoff;
                RolloffFactorUpDown.Value = e.RolloffFactor;
                MaxLeakageTextBox.Text = FloatUtil.ToString(e.MaxLeakage);
                MinLeakageDistUpDown.Value = e.MinLeakageDistance;
                MaxLeakageDistUpDown.Value = e.MaxLeakageDistance;
                MaxPathDepthUpDown.Value = e.MaxPathDepth;
                SmallReverbUpDown.Value = e.SmallReverbSend;
                MediumReverbUpDown.Value = e.MediumReverbSend;
                LargeReverbUpDown.Value = e.LargeReverbSend;
                BrokenHealthTextBox.Text = FloatUtil.ToString(e.BrokenHealth);
                UndamagedHealthTextBox.Text = FloatUtil.ToString(e.UndamagedHealth);

                populatingui = false;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectObject(CurrentEmitter);
                }

            }
        }

        private void ProjectItemChanged()
        {
            CurrentEmitter?.UpdateFromStaticEmitter();//also update the placement wrapper

            if (CurrentEmitter?.RelFile != null)
            {
                ProjectForm.SetAudioFileHasChanged(true);
            }
        }



        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            uint hash = 0;
            string name = NameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.StaticEmitter.NameHash != hash)
            {
                CurrentEmitter.StaticEmitter.Name = NameTextBox.Text;
                CurrentEmitter.StaticEmitter.NameHash = hash;

                ProjectItemChanged();
                UpdateFormTitle();
            }
        }

        private void PositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var vec = FloatUtil.ParseVector3String(PositionTextBox.Text);
            if (CurrentEmitter.StaticEmitter.Position != vec)
            {
                CurrentEmitter.StaticEmitter.Position = vec;

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
            if (CurrentEmitter?.StaticEmitter == null) return;

            float rad = FloatUtil.Parse(InnerRadiusTextBox.Text);
            if (CurrentEmitter.StaticEmitter.MinDistance != rad)
            {
                CurrentEmitter.StaticEmitter.MinDistance = rad;

                ProjectItemChanged();
            }
        }

        private void OuterRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            float rad = FloatUtil.Parse(OuterRadiusTextBox.Text);
            if (CurrentEmitter.StaticEmitter.MaxDistance != rad)
            {
                CurrentEmitter.StaticEmitter.MaxDistance = rad;

                ProjectItemChanged();
            }
        }

        private void ChildSoundTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            uint hash = 0;
            string name = ChildSoundTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.StaticEmitter.ChildSound != hash)
            {
                CurrentEmitter.StaticEmitter.ChildSound = hash;

                ProjectItemChanged();
            }
        }

        private void RadioStationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            uint hash = 0;
            string name = RadioStationTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.StaticEmitter.RadioStation != hash)
            {
                CurrentEmitter.StaticEmitter.RadioStation = hash;

                ProjectItemChanged();
            }
        }

        private void RadioScoreTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            uint hash = 0;
            string name = RadioScoreTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.StaticEmitter.RadioStationForScore != hash)
            {
                CurrentEmitter.StaticEmitter.RadioStationForScore = hash;

                ProjectItemChanged();
            }
        }

        private void InteriorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            uint hash = 0;
            string name = InteriorTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.StaticEmitter.Interior != hash)
            {
                CurrentEmitter.StaticEmitter.Interior = hash;

                ProjectItemChanged();
            }
        }

        private void RoomTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            uint hash = 0;
            string name = RoomTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.StaticEmitter.Room != hash)
            {
                CurrentEmitter.StaticEmitter.Room = hash;

                ProjectItemChanged();
            }
        }

        private void AlarmTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            uint hash = 0;
            string name = AlarmTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.StaticEmitter.Alarm != hash)
            {
                CurrentEmitter.StaticEmitter.Alarm = hash;

                ProjectItemChanged();
            }
        }

        private void OnBreakTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            uint hash = 0;
            string name = OnBreakTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.StaticEmitter.OnBreakOneShot != hash)
            {
                CurrentEmitter.StaticEmitter.OnBreakOneShot = hash;

                ProjectItemChanged();
            }
        }

        private void VolumeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            int.TryParse(VolumeTextBox.Text, out var val);
            if (CurrentEmitter.StaticEmitter.EmittedVolume != val)
            {
                CurrentEmitter.StaticEmitter.EmittedVolume = val;

                ProjectItemChanged();
            }
        }

        private void StartTimeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = (ushort)StartTimeUpDown.Value;
            if (CurrentEmitter.StaticEmitter.MinTimeMinutes != val)
            {
                CurrentEmitter.StaticEmitter.MinTimeMinutes = val;

                ProjectItemChanged();
            }
        }

        private void EndTimeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = (ushort)EndTimeUpDown.Value;
            if (CurrentEmitter.StaticEmitter.MaxTimeMinutes != val)
            {
                CurrentEmitter.StaticEmitter.MaxTimeMinutes = val;

                ProjectItemChanged();
            }
        }

        private void FlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            uint flags = 0;
            if (uint.TryParse(FlagsTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentEmitter.StaticEmitter.Flags != flags)
                {
                    CurrentEmitter.StaticEmitter.Flags = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void LPFCutoffUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = (ushort)LPFCutoffUpDown.Value;
            if (CurrentEmitter.StaticEmitter.LPFCutoff != val)
            {
                CurrentEmitter.StaticEmitter.LPFCutoff = val;

                ProjectItemChanged();
            }
        }

        private void HPFCutoffUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = (ushort)HPFCutoffUpDown.Value;
            if (CurrentEmitter.StaticEmitter.HPFCutoff != val)
            {
                CurrentEmitter.StaticEmitter.HPFCutoff = val;

                ProjectItemChanged();
            }
        }

        private void RolloffFactorUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = (ushort)RolloffFactorUpDown.Value;
            if (CurrentEmitter.StaticEmitter.RolloffFactor != val)
            {
                CurrentEmitter.StaticEmitter.RolloffFactor = val;

                ProjectItemChanged();
            }
        }

        private void MaxLeakageTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = FloatUtil.Parse(MaxLeakageTextBox.Text);
            if (CurrentEmitter.StaticEmitter.MaxLeakage != val)
            {
                CurrentEmitter.StaticEmitter.MaxLeakage = val;

                ProjectItemChanged();
            }
        }

        private void MinLeakageDistUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = (ushort)MinLeakageDistUpDown.Value;
            if (CurrentEmitter.StaticEmitter.MinLeakageDistance != val)
            {
                CurrentEmitter.StaticEmitter.MinLeakageDistance = val;

                ProjectItemChanged();
            }
        }

        private void MaxLeakageDistUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = (ushort)MaxLeakageDistUpDown.Value;
            if (CurrentEmitter.StaticEmitter.MaxLeakageDistance != val)
            {
                CurrentEmitter.StaticEmitter.MaxLeakageDistance = val;

                ProjectItemChanged();
            }
        }

        private void MaxPathDepthUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = (byte)MaxPathDepthUpDown.Value;
            if (CurrentEmitter.StaticEmitter.MaxPathDepth != val)
            {
                CurrentEmitter.StaticEmitter.MaxPathDepth = val;

                ProjectItemChanged();
            }
        }

        private void SmallReverbUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = (byte)SmallReverbUpDown.Value;
            if (CurrentEmitter.StaticEmitter.SmallReverbSend != val)
            {
                CurrentEmitter.StaticEmitter.SmallReverbSend = val;

                ProjectItemChanged();
            }
        }

        private void MediumReverbUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = (byte)MediumReverbUpDown.Value;
            if (CurrentEmitter.StaticEmitter.MediumReverbSend != val)
            {
                CurrentEmitter.StaticEmitter.MediumReverbSend = val;

                ProjectItemChanged();
            }
        }

        private void LargeReverbUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = (byte)LargeReverbUpDown.Value;
            if (CurrentEmitter.StaticEmitter.LargeReverbSend != val)
            {
                CurrentEmitter.StaticEmitter.LargeReverbSend = val;

                ProjectItemChanged();
            }
        }

        private void BrokenHealthTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = FloatUtil.Parse(BrokenHealthTextBox.Text);
            if (CurrentEmitter.StaticEmitter.BrokenHealth != val)
            {
                CurrentEmitter.StaticEmitter.BrokenHealth = val;

                ProjectItemChanged();
            }
        }

        private void UndamagedHealthTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.StaticEmitter == null) return;

            var val = FloatUtil.Parse(UndamagedHealthTextBox.Text);
            if (CurrentEmitter.StaticEmitter.UndamagedHealth != val)
            {
                CurrentEmitter.StaticEmitter.UndamagedHealth = val;

                ProjectItemChanged();
            }
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentEmitter == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentEmitter.Position, SharpDX.Vector3.One * CurrentEmitter.InnerRadius * 2.0f);
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentEmitter);
            ProjectForm.AddAudioFileToProject(CurrentEmitter.RelFile);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentEmitter);
            ProjectForm.DeleteAudioStaticEmitter();
        }
    }
}
