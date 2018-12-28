using CodeWalker.GameFiles;
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
    public partial class EditAudioZoneListPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public Dat151AmbientZoneList CurrentZoneList { get; set; }

        private bool populatingui = false;


        public EditAudioZoneListPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetZoneList(Dat151AmbientZoneList list)
        {
            CurrentZoneList = list;
            Tag = list;
            UpdateFormTitle();
            UpdateUI();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentZoneList?.NameHash.ToString() ?? "";
        }

        private void UpdateUI()
        {

            if (CurrentZoneList == null)
            {
                //AddToProjectButton.Enabled = false;
                //DeleteButton.Enabled = false;

                populatingui = true;
                NameTextBox.Text = string.Empty;
                HashesTextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                //AddToProjectButton.Enabled = CurrentZoneList?.Rel != null ? !ProjectForm.AudioFileExistsInProject(CurrentZoneList.Rel) : false;
                //DeleteButton.Enabled = !AddToProjectButton.Enabled;

                populatingui = true;
                var zl = CurrentZoneList;

                NameTextBox.Text = zl.NameHash.ToString();

                StringBuilder sb = new StringBuilder();
                if (zl.ZoneHashes != null)
                {
                    foreach (var hash in zl.ZoneHashes)
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
            if (CurrentZoneList?.Rel != null)
            {
                ProjectForm.SetAudioFileHasChanged(true);
            }
        }



        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZoneList == null) return;

            uint hash = 0;
            string name = NameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentZoneList.NameHash != hash)
            {
                CurrentZoneList.Name = NameTextBox.Text;
                CurrentZoneList.NameHash = hash;

                ProjectItemChanged();
                UpdateFormTitle();
            }
        }

        private void HashesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZoneList == null) return;

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

                CurrentZoneList.ZoneHashes = hashlist.ToArray();
                CurrentZoneList.ZoneCount = (byte)hashlist.Count;

                ProjectItemChanged();
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentZoneList);
            ProjectForm.DeleteAudioZoneList();
        }
    }
}
