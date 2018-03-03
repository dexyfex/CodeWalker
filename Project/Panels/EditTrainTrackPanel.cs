using CodeWalker.World;
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
    public partial class EditTrainTrackPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public TrainTrack Track { get; set; }

        //private bool populatingui = false;
        private bool waschanged = false;

        public EditTrainTrackPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetTrainTrack(TrainTrack track)
        {
            Track = track;
            Tag = track;
            UpdateFormTitle();
            UpdateTrainTrackUI();
            waschanged = track?.HasChanged ?? false;
        }

        public void UpdateFormTitleYnvChanged()
        {
            bool changed = Track.HasChanged;
            if (!waschanged && changed)
            {
                UpdateFormTitle();
                waschanged = true;
            }
            else if (waschanged && !changed)
            {
                UpdateFormTitle();
                waschanged = false;
            }
        }
        private void UpdateFormTitle()
        {
            string fn = Track.RpfFileEntry?.Name ?? Track.Name;
            if (string.IsNullOrEmpty(fn)) fn = "Edit Train Track";
            Text = fn + (Track.HasChanged ? "*" : "");
        }


        public void UpdateTrainTrackUI()
        {
            if (Track == null)
            {
                //TrainTrackFilePanel.Enabled = false;
                TrainTrackFilenameTextBox.Text = string.Empty;
                TrainTrackConfigNameTextBox.Text = string.Empty;
                TrainTrackIsPingPongCheckBox.Checked = false;
                TrainTrackStopsAtStationsCheckBox.Checked = false;
                TrainTrackMPStopsAtStationsCheckBox.Checked = false;
                TrainTrackSpeedTextBox.Text = string.Empty;
                TrainTrackBrakingDistTextBox.Text = string.Empty;
                TrainTrackRpfPathTextBox.Text = string.Empty;
                TrainTrackFilePathTextBox.Text = string.Empty;
                TrainTrackProjectPathTextBox.Text = string.Empty;
                TrainTrackInfoLabel.Text = string.Empty;
            }
            else
            {
                //populatingui = true;
                //TrainTrackFilePanel.Enabled = true;
                TrainTrackFilenameTextBox.Text = Track.filename;
                TrainTrackConfigNameTextBox.Text = Track.trainConfigName;
                TrainTrackIsPingPongCheckBox.Checked = Track.isPingPongTrack;
                TrainTrackStopsAtStationsCheckBox.Checked = Track.stopsAtStations;
                TrainTrackMPStopsAtStationsCheckBox.Checked = Track.MPstopsAtStations;
                TrainTrackSpeedTextBox.Text = FloatUtil.ToString(Track.speed);
                TrainTrackBrakingDistTextBox.Text = FloatUtil.ToString(Track.brakingDist);
                TrainTrackRpfPathTextBox.Text = Track.RpfFileEntry?.Path ?? string.Empty;
                TrainTrackFilePathTextBox.Text = Track.FilePath;
                TrainTrackProjectPathTextBox.Text = string.Empty; //todo
                TrainTrackInfoLabel.Text = Track.StationCount.ToString() + " stations";
                //populatingui = false;
            }
        }
    }
}
