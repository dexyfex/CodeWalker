using CodeWalker.GameFiles;
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
    public partial class EditAudioInteriorRoomPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public Dat151InteriorRoom CurrentRoom { get; set; }

        private bool populatingui = false;


        public EditAudioInteriorRoomPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetRoom(Dat151InteriorRoom room)
        {
            CurrentRoom = room;
            Tag = room;
            UpdateFormTitle();
            UpdateUI();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentRoom?.NameHash.ToString() ?? "";
        }

        private void UpdateUI()
        {
            if (CurrentRoom == null)
            {
                //AddToProjectButton.Enabled = false;
                DeleteButton.Enabled = false;

                populatingui = true;
                NameTextBox.Text = string.Empty;
                MloRoomTextBox.Text = string.Empty;
                ZoneTextBox.Text = string.Empty;
                Unk02TextBox.Text = string.Empty;
                Unk03TextBox.Text = string.Empty;
                ReverbTextBox.Text = string.Empty;
                EchoTextBox.Text = string.Empty;
                SoundTextBox.Text = string.Empty;
                Unk07TextBox.Text = string.Empty;
                Unk08TextBox.Text = string.Empty;
                Unk09TextBox.Text = string.Empty;
                Unk10TextBox.Text = string.Empty;
                Unk11TextBox.Text = string.Empty;
                Unk12TextBox.Text = string.Empty;
                Unk13TextBox.Text = string.Empty;
                SoundSetTextBox.Text = string.Empty;
                Flags0TextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                //AddToProjectButton.Enabled = CurrentZoneList?.Rel != null ? !ProjectForm.AudioFileExistsInProject(CurrentZoneList.Rel) : false;
                DeleteButton.Enabled = !(CurrentRoom?.Rel != null ? !ProjectForm.AudioFileExistsInProject(CurrentRoom.Rel) : false);// AddToProjectButton.Enabled;

                populatingui = true;
                var cr = CurrentRoom;

                NameTextBox.Text = cr.NameHash.ToString();
                MloRoomTextBox.Text = cr.RoomName.ToString();
                ZoneTextBox.Text = cr.AmbientZone.ToString();
                Unk03TextBox.Text = FloatUtil.ToString(cr.ReverbSmall);
                ReverbTextBox.Text = FloatUtil.ToString(cr.ReverbMedium);
                EchoTextBox.Text = FloatUtil.ToString(cr.ReverbLarge);
                SoundTextBox.Text = cr.RoomToneSound.ToString();
                Unk08TextBox.Text = FloatUtil.ToString(cr.ExteriorAudibility);
                Unk09TextBox.Text = FloatUtil.ToString(cr.RoomOcclusionDamping);
                Unk10TextBox.Text = FloatUtil.ToString(cr.NonMarkedPortalOcclusion);
                Unk11TextBox.Text = FloatUtil.ToString(cr.DistanceFromPortalForOcclusion);
                Unk12TextBox.Text = FloatUtil.ToString(cr.DistanceFromPortalFadeDistance);
                Unk13TextBox.Text = cr.WeaponMetrics.ToString();
                SoundSetTextBox.Text = cr.InteriorWallaSoundSet.ToString();
                Flags0TextBox.Text = cr.Flags.Hex;
                populatingui = false;


            }

        }

        private void ProjectItemChanged()
        {
            if (CurrentRoom?.Rel != null)
            {
                ProjectForm.SetAudioFileHasChanged(true);
            }
        }

        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint hash = 0;
            string name = NameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRoom.NameHash != hash)
            {
                CurrentRoom.Name = NameTextBox.Text;
                CurrentRoom.NameHash = hash;

                ProjectItemChanged();
                UpdateFormTitle();
            }
        }

        private void Unk00TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint hash = 0;
            string name = MloRoomTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRoom.RoomName != hash)
            {
                CurrentRoom.RoomName = hash;

                ProjectItemChanged();
            }
        }

        private void Hash1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint hash = 0;
            string name = ZoneTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRoom.AmbientZone != hash)
            {
                CurrentRoom.AmbientZone = hash;

                ProjectItemChanged();
            }
        }

        private void Unk02TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

        }

        private void Unk03TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            float val = 0;
            if (FloatUtil.TryParse(Unk03TextBox.Text, out val))
            {
                if (CurrentRoom.ReverbSmall != val)
                {
                    CurrentRoom.ReverbSmall = val;

                    ProjectItemChanged();
                }
            }
        }

        private void ReverbTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            float val = 0;
            if (FloatUtil.TryParse(ReverbTextBox.Text, out val))
            {
                if (CurrentRoom.ReverbMedium != val)
                {
                    CurrentRoom.ReverbMedium = val;

                    ProjectItemChanged();
                }
            }
        }

        private void EchoTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            float val = 0;
            if (FloatUtil.TryParse(EchoTextBox.Text, out val))
            {
                if (CurrentRoom.ReverbLarge != val)
                {
                    CurrentRoom.ReverbLarge = val;

                    ProjectItemChanged();
                }
            }
        }

        private void Unk06TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint hash = 0;
            string name = SoundTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRoom.RoomToneSound != hash)
            {
                CurrentRoom.RoomToneSound = hash;

                ProjectItemChanged();
            }
        }

        private void Unk07TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

        }

        private void Unk08TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            float val = 0;
            if (FloatUtil.TryParse(Unk08TextBox.Text, out val))
            {
                if (CurrentRoom.ExteriorAudibility != val)
                {
                    CurrentRoom.ExteriorAudibility = val;

                    ProjectItemChanged();
                }
            }
        }

        private void Unk09TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            float val = 0;
            if (FloatUtil.TryParse(Unk09TextBox.Text, out val))
            {
                if (CurrentRoom.RoomOcclusionDamping != val)
                {
                    CurrentRoom.RoomOcclusionDamping = val;

                    ProjectItemChanged();
                }
            }
        }

        private void Unk10TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            float val = 0;
            if (FloatUtil.TryParse(Unk10TextBox.Text, out val))
            {
                if (CurrentRoom.NonMarkedPortalOcclusion != val)
                {
                    CurrentRoom.NonMarkedPortalOcclusion = val;

                    ProjectItemChanged();
                }
            }
        }

        private void Unk11TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            float val = 0;
            if (FloatUtil.TryParse(Unk11TextBox.Text, out val))
            {
                if (CurrentRoom.DistanceFromPortalForOcclusion != val)
                {
                    CurrentRoom.DistanceFromPortalForOcclusion = val;

                    ProjectItemChanged();
                }
            }
        }

        private void Unk12TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            float val = 0;
            if (FloatUtil.TryParse(Unk12TextBox.Text, out val))
            {
                if (CurrentRoom.DistanceFromPortalFadeDistance != val)
                {
                    CurrentRoom.DistanceFromPortalFadeDistance = val;

                    ProjectItemChanged();
                }
            }
        }

        private void Unk13TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint hash = 0;
            string name = Unk13TextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRoom.WeaponMetrics != hash)
            {
                CurrentRoom.WeaponMetrics = hash;

                ProjectItemChanged();
            }
        }

        private void Unk14TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint hash = 0;
            string name = SoundSetTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRoom.InteriorWallaSoundSet != hash)
            {
                CurrentRoom.InteriorWallaSoundSet = hash;

                ProjectItemChanged();
            }
        }

        private void Flags0TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags0TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentRoom.Flags != flags)
                {
                    CurrentRoom.Flags = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentRoom);
            ProjectForm.DeleteAudioInteriorRoom();
        }
    }
}
