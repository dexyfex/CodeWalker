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
                Hash1TextBox.Text = string.Empty;
                Unk02TextBox.Text = string.Empty;
                Unk03TextBox.Text = string.Empty;
                ReverbTextBox.Text = string.Empty;
                EchoTextBox.Text = string.Empty;
                Unk06TextBox.Text = string.Empty;
                Unk07TextBox.Text = string.Empty;
                Unk08TextBox.Text = string.Empty;
                Unk09TextBox.Text = string.Empty;
                Unk10TextBox.Text = string.Empty;
                Unk11TextBox.Text = string.Empty;
                Unk12TextBox.Text = string.Empty;
                Unk13TextBox.Text = string.Empty;
                Unk14TextBox.Text = string.Empty;
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
                MloRoomTextBox.Text = cr.MloRoom.ToString();
                Hash1TextBox.Text = cr.Hash1.ToString();
                Unk02TextBox.Text = cr.Unk02.ToString();
                Unk03TextBox.Text = FloatUtil.ToString(cr.Unk03);
                ReverbTextBox.Text = FloatUtil.ToString(cr.Reverb);
                EchoTextBox.Text = FloatUtil.ToString(cr.Echo);
                Unk06TextBox.Text = cr.Unk06.ToString();
                Unk07TextBox.Text = FloatUtil.ToString(cr.Unk07);
                Unk08TextBox.Text = FloatUtil.ToString(cr.Unk08);
                Unk09TextBox.Text = FloatUtil.ToString(cr.Unk09);
                Unk10TextBox.Text = FloatUtil.ToString(cr.Unk10);
                Unk11TextBox.Text = FloatUtil.ToString(cr.Unk11);
                Unk12TextBox.Text = FloatUtil.ToString(cr.Unk12);
                Unk13TextBox.Text = cr.Unk13.ToString();
                Unk14TextBox.Text = cr.Unk14.ToString();
                Flags0TextBox.Text = cr.Flags0.Hex;
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

            if (CurrentRoom.MloRoom != hash)
            {
                CurrentRoom.MloRoom = hash;

                ProjectItemChanged();
            }
        }

        private void Hash1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint hash = 0;
            string name = Hash1TextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRoom.Hash1 != hash)
            {
                CurrentRoom.Hash1 = hash;

                ProjectItemChanged();
            }
        }

        private void Unk02TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint val = 0;
            if (uint.TryParse(Unk02TextBox.Text, out val))
            {
                if (CurrentRoom.Unk02 != val)
                {
                    CurrentRoom.Unk02 = val;

                    ProjectItemChanged();
                }
            }
        }

        private void Unk03TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            float val = 0;
            if (FloatUtil.TryParse(Unk03TextBox.Text, out val))
            {
                if (CurrentRoom.Unk03 != val)
                {
                    CurrentRoom.Unk03 = val;

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
                if (CurrentRoom.Reverb != val)
                {
                    CurrentRoom.Reverb = val;

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
                if (CurrentRoom.Echo != val)
                {
                    CurrentRoom.Echo = val;

                    ProjectItemChanged();
                }
            }
        }

        private void Unk06TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint hash = 0;
            string name = Unk06TextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRoom.Unk06 != hash)
            {
                CurrentRoom.Unk06 = hash;

                ProjectItemChanged();
            }
        }

        private void Unk07TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            float val = 0;
            if (FloatUtil.TryParse(Unk07TextBox.Text, out val))
            {
                if (CurrentRoom.Unk07 != val)
                {
                    CurrentRoom.Unk07 = val;

                    ProjectItemChanged();
                }
            }
        }

        private void Unk08TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            float val = 0;
            if (FloatUtil.TryParse(Unk08TextBox.Text, out val))
            {
                if (CurrentRoom.Unk08 != val)
                {
                    CurrentRoom.Unk08 = val;

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
                if (CurrentRoom.Unk09 != val)
                {
                    CurrentRoom.Unk09 = val;

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
                if (CurrentRoom.Unk10 != val)
                {
                    CurrentRoom.Unk10 = val;

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
                if (CurrentRoom.Unk11 != val)
                {
                    CurrentRoom.Unk11 = val;

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
                if (CurrentRoom.Unk12 != val)
                {
                    CurrentRoom.Unk12 = val;

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

            if (CurrentRoom.Unk13 != hash)
            {
                CurrentRoom.Unk13 = hash;

                ProjectItemChanged();
            }
        }

        private void Unk14TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentRoom == null) return;

            uint hash = 0;
            string name = Unk14TextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentRoom.Unk14 != hash)
            {
                CurrentRoom.Unk14 = hash;

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
                if (CurrentRoom.Flags0 != flags)
                {
                    CurrentRoom.Flags0 = flags;

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
