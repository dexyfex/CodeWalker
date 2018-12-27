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
    public partial class EditAudioEmitterListPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public Dat151AmbientEmitterList CurrentEmitterList { get; set; }

        private bool populatingui = false;


        public EditAudioEmitterListPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetEmitterList(Dat151AmbientEmitterList list)
        {
            CurrentEmitterList = list;
            Tag = list;
            UpdateFormTitle();
            UpdateUI();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentEmitterList?.NameHash.ToString() ?? "";
        }

        private void UpdateUI()
        {

            if (CurrentEmitterList == null)
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
                var el = CurrentEmitterList;

                NameTextBox.Text = el.NameHash.ToString();

                StringBuilder sb = new StringBuilder();
                if (el.EmitterHashes != null)
                {
                    foreach (var hash in el.EmitterHashes)
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
            if (CurrentEmitterList?.Rel != null)
            {
                ProjectForm.SetAudioFileHasChanged(true);
            }
        }



        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitterList == null) return;

            uint hash = 0;
            string name = NameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitterList.NameHash != hash)
            {
                CurrentEmitterList.Name = NameTextBox.Text;
                CurrentEmitterList.NameHash = hash;

                ProjectItemChanged();
                UpdateFormTitle();
            }
        }

        private void HashesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitterList == null) return;

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

                CurrentEmitterList.EmitterHashes = hashlist.ToArray();
                CurrentEmitterList.EmitterCount = (byte)hashlist.Count;

                ProjectItemChanged();
            }
        }
    }
}
