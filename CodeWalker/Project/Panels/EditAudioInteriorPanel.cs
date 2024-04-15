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
    public partial class EditAudioInteriorPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public Dat151InteriorSettings CurrentInterior { get; set; }

        private bool populatingui = false;


        public EditAudioInteriorPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }


        public void SetInterior(Dat151InteriorSettings interior)
        {
            CurrentInterior = interior;
            Tag = interior;
            UpdateFormTitle();
            UpdateUI();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentInterior?.NameHash.ToString() ?? "";
        }

        private void UpdateUI()
        {
            if (CurrentInterior == null)
            {
                //AddToProjectButton.Enabled = false;
                DeleteButton.Enabled = false;

                populatingui = true;
                NameTextBox.Text = string.Empty;
                FlagsTextBox.Text = string.Empty;
                WallaTextBox.Text = string.Empty;
                TunnelTextBox.Text = string.Empty;
                HashesTextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                //AddToProjectButton.Enabled = CurrentZoneList?.Rel != null ? !ProjectForm.AudioFileExistsInProject(CurrentZoneList.Rel) : false;
                //DeleteButton.Enabled = !AddToProjectButton.Enabled;

                populatingui = true;
                var ci = CurrentInterior;

                NameTextBox.Text = ci.NameHash.ToString();

                FlagsTextBox.Text = ci.Flags.Hex;
                WallaTextBox.Text = ci.InteriorWallaSoundSet.ToString();
                TunnelTextBox.Text = ci.InteriorReflections.ToString();

                StringBuilder sb = new StringBuilder();
                if (ci.Rooms != null)
                {
                    foreach (var hash in ci.Rooms)
                    {
                        sb.AppendLine(hash.ToString());
                    }
                }
                HashesTextBox.Text = sb.ToString();


                populatingui = false;


            }

        }

        private void ProjectItemChanged()
        {
            if (CurrentInterior?.Rel != null)
            {
                ProjectForm.SetAudioFileHasChanged(true);
            }
        }


        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentInterior == null) return;

            uint hash = 0;
            string name = NameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentInterior.NameHash != hash)
            {
                CurrentInterior.Name = NameTextBox.Text;
                CurrentInterior.NameHash = hash;

                ProjectItemChanged();
                UpdateFormTitle();
            }
        }

        private void FlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentInterior == null) return;

            uint flags = 0;
            if (uint.TryParse(FlagsTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentInterior.Flags != flags)
                {
                    CurrentInterior.Flags = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void WallaTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentInterior == null) return;

            uint hash = 0;
            string name = WallaTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }

            if (CurrentInterior.InteriorWallaSoundSet != hash)
            {
                CurrentInterior.InteriorWallaSoundSet = hash;

                ProjectItemChanged();
            }
        }

        private void TunnelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentInterior == null) return;

            uint hash = 0;
            string name = TunnelTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }

            if (CurrentInterior.InteriorReflections != hash)
            {
                CurrentInterior.InteriorReflections = hash;

                ProjectItemChanged();
            }
        }

        private void HashesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentInterior == null) return;

            var hashstrs = HashesTextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
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

                CurrentInterior.Rooms = hashlist.ToArray();
                CurrentInterior.NumRooms = (byte)hashlist.Count;

                ProjectItemChanged();
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentInterior);
            ProjectForm.DeleteAudioInterior();
        }
    }
}
