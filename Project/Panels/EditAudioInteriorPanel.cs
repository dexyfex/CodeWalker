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
        public Dat151Interior CurrentInterior { get; set; }

        private bool populatingui = false;


        public EditAudioInteriorPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }


        public void SetInterior(Dat151Interior interior)
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
                Flags0TextBox.Text = string.Empty;
                Flags1TextBox.Text = string.Empty;
                Flags2TextBox.Text = string.Empty;
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

                Flags0TextBox.Text = ci.Unk0.Hex;
                Flags1TextBox.Text = ci.Unk1.Hex;
                Flags2TextBox.Text = ci.Unk2.Hex;

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

        private void Flags0TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentInterior == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags0TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentInterior.Unk0 != flags)
                {
                    CurrentInterior.Unk0 = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void Flags1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentInterior == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags1TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentInterior.Unk1 != flags)
                {
                    CurrentInterior.Unk1 = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void Flags2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentInterior == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags2TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentInterior.Unk2 != flags)
                {
                    CurrentInterior.Unk2 = flags;

                    ProjectItemChanged();
                }
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
                CurrentInterior.RoomsCount = (byte)hashlist.Count;

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
