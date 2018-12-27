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
    public partial class EditAudioFilePanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public RelFile CurrentFile { get; set; }

        private bool populatingui = false;


        public EditAudioFilePanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetFile(RelFile file)
        {
            CurrentFile = file;
            Tag = file;
            UpdateFormTitle();
            UpdateUI();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentFile?.Name ?? "";
        }

        private void UpdateUI()
        {
            if (CurrentFile == null)
            {
                populatingui = true;
                FileTypeComboBox.Text = string.Empty;
                UnkVersionUpDown.Value = 0;
                FileLocationTextBox.Text = string.Empty;
                ProjectPathTextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                populatingui = true;
                FileTypeComboBox.Text = CurrentFile.RelType.ToString();
                UnkVersionUpDown.Value = CurrentFile.DataUnkVal;
                var project = ProjectForm?.CurrentProjectFile;
                FileLocationTextBox.Text = CurrentFile.RpfFileEntry?.Path ?? CurrentFile.FilePath;
                ProjectPathTextBox.Text = (project != null) ? project.GetRelativePath(CurrentFile.FilePath) : CurrentFile.FilePath;
                populatingui = false;
            }
        }

        private void ProjectItemChanged()
        {
            if (CurrentFile != null)
            {
                ProjectForm.SetAudioFileHasChanged(true);
            }
        }

        private void FileTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentFile == null) return;

            var type = RelDatFileType.Dat151;
            if (Enum.TryParse(FileTypeComboBox.Text, out type))
            {
                if (CurrentFile.RelType != type)
                {
                    CurrentFile.RelType = type;

                    ProjectItemChanged();
                }
            }
        }

        private void UnkVersionUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentFile == null) return;

            byte unk = (byte)UnkVersionUpDown.Value;
            if (CurrentFile.DataUnkVal != unk)
            {
                CurrentFile.DataUnkVal = unk;

                ProjectItemChanged();
            }
        }
    }
}
