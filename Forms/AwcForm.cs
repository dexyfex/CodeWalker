using CodeWalker.GameFiles;
using FastColoredTextBoxNS;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace CodeWalker.Forms
{
    public partial class AwcForm : Form
    {
        public AwcFile Awc { get; set; }

        private AwcStream currentAudio;
        private XAudio2 xAudio2;
        private MasteringVoice masteringVoice;
        private AudioBuffer audioBuffer;
        private SourceVoice sourceVoice;

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

        private enum PlayerState { Stopped, Playing, Paused };
        private PlayerState playerState = PlayerState.Stopped;

        private Stopwatch playtime;
        private int playBeginMs;
        private float trackLength;
        private bool trackFinished;

        private bool LoadingXml = false;
        private bool DelayHighlight = false;


        public AwcForm()
        {
            InitializeComponent();

            playtime = new Stopwatch();
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
                foreach (var audio in awc.Streams)
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
            if (playerState != PlayerState.Stopped)
            {
                sourceVoice.DestroyVoice();
                sourceVoice.Dispose();
                audioBuffer.Stream.Dispose();
                SetPlayerState(PlayerState.Stopped);
            }
        }

        private void SetPlayerState(PlayerState newState)
        {
            if (playerState != newState)
            {
                switch (newState)
                {
                    case PlayerState.Playing:
                        if (playerState == PlayerState.Stopped)
                            playtime.Reset();
                        playtime.Start();

                        PlayButton.Text = "\u275A\u275A";
                        StopButton.Enabled = true;
                        LabelTime.Visible = true;
                        break;
                    case PlayerState.Paused:
                        playtime.Stop();
                        PlayButton.Text = "\u25B6";
                        StopButton.Enabled = true;
                        LabelTime.Visible = true;
                        break;
                    case PlayerState.Stopped:
                        playtime.Stop();
                        PlayButton.Text = "\u25B6";
                        LabelTime.Visible = false;
                        StopButton.Enabled = true;
                        break;
                }

                playerState = newState;
                UpdateUI();
            }
        }

        private void InitializeAudio(AwcStream audio, float playBegin = 0)
        {
            currentAudio = audio;
            trackLength = audio.Length;

            if (xAudio2 == null)
            {
                xAudio2 = new XAudio2();
                masteringVoice = new MasteringVoice(xAudio2);
            }

            Stream wavStream = audio.GetWavStream();
            SoundStream soundStream = new SoundStream(wavStream);
            audioBuffer = new AudioBuffer
            {
                Stream = soundStream.ToDataStream(),
                AudioBytes = (int)soundStream.Length,
                Flags = BufferFlags.EndOfStream
            };
            if (playBegin > 0)
            {
                audioBuffer.PlayBegin = (int)(soundStream.Format.SampleRate * playBegin) / 128 * 128;
                if (playtime.IsRunning)
                    playtime.Restart();
                else
                    playtime.Reset();
                playBeginMs = (int)(playBegin * 1000);
            }
            else
                playBeginMs = 0;
            soundStream.Close();
            wavStream.Close();

            trackFinished = false;
            sourceVoice = new SourceVoice(xAudio2, soundStream.Format, true);
            sourceVoice.SubmitSourceBuffer(audioBuffer, soundStream.DecodedPacketsInfo);
            sourceVoice.BufferEnd += (context) => trackFinished = true;
            sourceVoice.SetVolume((float)VolumeTrackBar.Value / 100);
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
                    InitializeAudio(audio);
                    sourceVoice.Start();
                    SetPlayerState(PlayerState.Playing);
                }
                else if (audio.MidiChunk != null)
                {
                    //todo: play MIDI?
                }
            }
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
            if (playerState == PlayerState.Playing)
            {
                sourceVoice.Stop();
                SetPlayerState(PlayerState.Paused);
            }
        }

        private void Resume()
        {
            if (playerState == PlayerState.Paused)
            {
                sourceVoice.Start();
                SetPlayerState(PlayerState.Playing);
            }
        }

        private void PositionTrackBar_Scroll(object sender, EventArgs e)
        {

            //sourceVoice.Stop();
            //InitializeAudio(currentAudio, PositionTrackBar.Value / 1000);
            //sourceVoice.Start();
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            switch (playerState)
            {
                case PlayerState.Stopped:
                    Play();
                    break;
                case PlayerState.Playing:
                    Pause();
                    break;
                case PlayerState.Paused:
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

        private void VolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            if (playerState == PlayerState.Playing)
                sourceVoice.SetVolume((float)VolumeTrackBar.Value / 100);
        }

        private void UpdateUI()
        {
            if (playerState != PlayerState.Stopped && trackFinished)
            {
                if (chbAutoJump.Checked)
                    PlayNext();
                else
                    Stop();
            }

            if (playerState != PlayerState.Stopped)
            {
                int playedMs = (int)playtime.Elapsed.TotalMilliseconds + playBeginMs;
                int totalMs = (int)(trackLength * 1000);
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void PlayListView_DoubleClick(object sender, EventArgs e)
        {
            if (playerState == PlayerState.Playing)
                Stop();
            Play();
        }

        private void AwcForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
            if (xAudio2 != null)
            {
                masteringVoice.Dispose();
                xAudio2.Dispose();
            }
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
