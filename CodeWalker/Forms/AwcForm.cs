using CodeWalker.GameFiles;
using CodeWalker.Utils;
using FastColoredTextBoxNS;
using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using Range = FastColoredTextBoxNS.Range;

namespace CodeWalker.Forms
{
    public partial class AwcForm : Form
    {
        public AwcFile Awc { get; set; }

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                UpdateFormTitle();
            }
        }
        public string FilePath { get; set; }

        private bool LoadingXml = false;
        private bool DelayHighlight = false;

        private AudioPlayer Player = new AudioPlayer();

        private bool PositionScrolled = false;

        public AwcForm()
        {
            InitializeComponent();
        }

        private void UpdateFormTitle()
        {
            Text = fileName + " - AWC Player - CodeWalker by dexyfex";
        }

        private void UpdateXmlTextBox(string xml)
        {
            LoadingXml = true;
            XmlTextBox.Text = "";
            XmlTextBox.Language = Language.XML;
            DelayHighlight = false;

            if (string.IsNullOrEmpty(xml))
            {
                LoadingXml = false;
                return;
            }
            //if (xml.Length > (1048576 * 5))
            //{
            //    XmlTextBox.Language = Language.Custom;
            //    XmlTextBox.Text = "[XML size > 10MB - Not shown due to performance limitations - Please use an external viewer for this file.]";
            //    return;
            //}
            //else 
            if (xml.Length > (1024 * 512))
            {
                XmlTextBox.Language = Language.Custom;
                DelayHighlight = true;
            }
            //else
            //{
            //    XmlTextBox.Language = Language.XML;
            //}


            Cursor = Cursors.WaitCursor;



            XmlTextBox.Text = xml;
            //XmlTextBox.IsChanged = false;
            XmlTextBox.ClearUndo();

            Cursor = Cursors.Default;
            LoadingXml = false;
        }

        public void LoadAwc(AwcFile awc)
        {
            Awc = awc;
            DetailsPropertyGrid.SelectedObject = awc;

            fileName = awc?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = awc?.FileEntry?.Name;
            }

            PlayListView.Items.Clear();

            float totalLength = 0;
            if (awc.Streams != null)
            {
                var strlist = awc.Streams.ToList();
                strlist.Sort((a, b) => a.Name.CompareTo(b.Name));
                foreach (var audio in strlist)
                {
                    if (audio.StreamBlocks != null) continue;//don't display multichannel source audios
                    var item = PlayListView.Items.Add(audio.Name);
                    item.SubItems.Add(audio.Type);
                    item.SubItems.Add(audio.LengthStr);
                    item.SubItems.Add(TextUtil.GetBytesReadable(audio.ByteLength));
                    item.Tag = audio;
                    totalLength += audio.Length;
                }
            }

            LabelInfo.Text = awc.Streams.Length.ToString() + " track(s), Length: " + TimeSpan.FromSeconds((float)totalLength).ToString("h\\:mm\\:ss");
            UpdateFormTitle();
        }

        public void LoadXml()
        {
            if (Awc != null)
            {
                var xml = AwcXml.GetXml(Awc);
                UpdateXmlTextBox(xml);
            }
        }

        private void HTMLSyntaxHighlight(Range range)
        {
            try
            {
                Style BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
                Style RedStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
                Style MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);

                //clear style of changed range
                range.ClearStyle(BlueStyle, MaroonStyle, RedStyle);
                //tag brackets highlighting
                range.SetStyle(BlueStyle, @"<|/>|</|>");
                //tag name
                range.SetStyle(MaroonStyle, @"<(?<range>[!\w]+)");
                //end of tag
                range.SetStyle(MaroonStyle, @"</(?<range>\w+)>");
                //attributes
                range.SetStyle(RedStyle, @"(?<range>\S+?)='[^']*'|(?<range>\S+)=""[^""]*""|(?<range>\S+)=\S+");
                //attribute values
                range.SetStyle(BlueStyle, @"\S+?=(?<range>'[^']*')|\S+=(?<range>""[^""]*"")|\S+=(?<range>\S+)");
            }
            catch
            { }
        }


        private void Stop()
        {
            Player.Stop();
            UpdateUI();
            UpdatePlayerButtons();
        }

        private void Play()
        {
            Stop();

            if (PlayListView.SelectedItems.Count == 1)
            {
                var item = PlayListView.SelectedItems[0];
                var audio = item.Tag as AwcStream;

                if ((audio?.FormatChunk != null) || (audio?.StreamFormat != null))
                {
                    Player.SetVolume(VolumeTrackBar.Value / 100.0f);
                    Player.LoadAudio(audio);
                    Player.Play();
                }
                else if (audio.MidiChunk != null)
                {
                    //todo: play MIDI?
                }
            }

            UpdateUI();
            UpdatePlayerButtons();
        }

        private void PlayPrevious()
        {
            Stop();
            if (PlayListView.SelectedIndices.Count > 0)
            {
                var nextIndex = PlayListView.SelectedIndices[0] - 1;
                if (nextIndex >= 0)
                {
                    PlayListView.Items[nextIndex].Selected = true;
                    PlayListView.Items[nextIndex].Focused = true;
                    Play();
                }
            }
        }

        private void PlayNext()
        {
            Stop();
            if (PlayListView.SelectedIndices.Count > 0)
            {
                var nextIndex = PlayListView.SelectedIndices[0] + 1;
                if (nextIndex < PlayListView.Items.Count)
                {
                    PlayListView.Items[nextIndex].Selected = true;
                    PlayListView.Items[nextIndex].Focused = true;
                    Play();
                }
            }
        }

        private void Pause()
        {
            Player.Pause();
            UpdatePlayerButtons();
        }

        private void Resume()
        {
            Player.Resume();
            UpdatePlayerButtons();
        }

        private void UpdateUI()
        {
            if ((Player.State != AudioPlayer.PlayerState.Stopped) && Player.trackFinished)
            {
                if (chbAutoJump.Checked)
                    PlayNext();
                else
                    Stop();
            }

            if (Player.State != AudioPlayer.PlayerState.Stopped)
            {
                int playedMs = Player.PlayTimeMS;
                int totalMs = Player.TotalTimeMS;
                PositionTrackBar.Maximum = totalMs;
                PositionTrackBar.Value = playedMs < totalMs ? playedMs : totalMs;

                LabelTime.Text = TimeSpan.FromSeconds(playedMs / 1000).ToString("m\\:ss")
                    + " / " + TimeSpan.FromSeconds(totalMs / 1000).ToString("m\\:ss");
            }
            else
            {
                PositionTrackBar.Value = 0;
            }
        }

        private void UpdatePlayerButtons()
        {
            switch (Player.State)
            {
                case AudioPlayer.PlayerState.Playing:
                    PlayButton.Text = "\u275A\u275A";
                    StopButton.Enabled = true;
                    LabelTime.Visible = true;
                    break;
                case AudioPlayer.PlayerState.Paused:
                    PlayButton.Text = "\u25B6";
                    StopButton.Enabled = true;
                    LabelTime.Visible = true;
                    break;
                case AudioPlayer.PlayerState.Stopped:
                    PlayButton.Text = "\u25B6";
                    StopButton.Enabled = true;
                    LabelTime.Visible = false;
                    break;
            }
        }


        private void PlayButton_Click(object sender, EventArgs e)
        {
            switch (Player.State)
            {
                case AudioPlayer.PlayerState.Stopped:
                    Play();
                    break;
                case AudioPlayer.PlayerState.Playing:
                    Pause();
                    break;
                case AudioPlayer.PlayerState.Paused:
                    Resume();
                    break;
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            PlayPrevious();
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            PlayNext();
        }

        private void PositionTrackBar_Scroll(object sender, EventArgs e)
        {
            PositionScrolled = true;

            var t = PositionTrackBar.Value / 1000.0f;

            Player.Seek(t);
        }

        private void PositionTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (PositionScrolled)
            {
                PositionScrolled = false;
                return;
            }
            PositionScrolled = false;

            var f = Math.Min(Math.Max((e.X-13.0f) / (PositionTrackBar.Width-26.0f), 0.0f), 1.0f);
            var v = f * (PositionTrackBar.Maximum / 1000.0f);

            Player.Seek(v);
        }

        private void VolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            Player.SetVolume((float)VolumeTrackBar.Value / 100);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void PlayListView_DoubleClick(object sender, EventArgs e)
        {
            Play();
        }

        private void AwcForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
            Player.DisposeAudio();
        }

        private void ExportAsWav_Click(object sender, EventArgs e)
        {
            if (PlayListView.SelectedItems.Count == 1)
            {
                var item = PlayListView.SelectedItems[0];
                var audio = item.Tag as AwcStream;

                var ext = ".wav";
                if (audio?.MidiChunk != null)
                {
                    ext = ".midi";
                }

                saveFileDialog.FileName = audio.Name + ext;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (audio?.MidiChunk != null)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, audio.MidiChunk.Data);
                    }
                    else if ((audio?.FormatChunk != null) || (audio?.StreamFormat != null))
                    {
                        Stream wavStream = audio.GetWavStream();
                        FileStream stream = File.Create(saveFileDialog.FileName);
                        wavStream.CopyTo(stream);
                        stream.Close();
                        wavStream.Close();
                    }

                }
            }
        }

        private void PlayListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ExportAsWav.Text = "Export as .wav";
            if (PlayListView.SelectedItems.Count == 1)
            {
                var item = PlayListView.SelectedItems[0];
                var audio = item.Tag as AwcStream;
                if (audio?.MidiChunk != null)
                {
                    ExportAsWav.Text = "Export as .midi";
                }
            }
        }

        private void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainTabControl.SelectedTab == XmlTabPage)
            {
                if (string.IsNullOrEmpty(XmlTextBox.Text))
                {
                    LoadXml();
                }
            }
        }

        private void XmlTextBox_VisibleRangeChangedDelayed(object sender, EventArgs e)
        {
            //this approach is much faster to load, but no outlining is available

            //highlight only visible area of text
            if (DelayHighlight)
            {
                HTMLSyntaxHighlight(XmlTextBox.VisibleRange);
            }
        }

        private void XmlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!LoadingXml)
            {

            }
        }
    }
}
