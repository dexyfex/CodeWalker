using CodeWalker.GameFiles;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace CodeWalker.Forms
{
    public partial class AwcForm : Form
    {
        public AwcFile Awc { get; set; }

        private AwcAudio currentAudio;
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

        public AwcForm()
        {
            InitializeComponent();

            playtime = new Stopwatch();
        }

        private void UpdateFormTitle()
        {
            Text = fileName + " - AWC Player - CodeWalker by dexyfex";
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
            if (awc.Audios != null)
            {
                foreach (var audio in awc.Audios)
                {
                    var item = PlayListView.Items.Add(audio.Name);
                    item.SubItems.Add(audio.Type);
                    item.SubItems.Add(audio.LengthStr);
                    item.SubItems.Add(TextUtil.GetBytesReadable(audio.Data.Length));
                    item.Tag = audio;
                    totalLength += audio.Length;
                }
            }

            LabelInfo.Text = awc.Audios.Length.ToString() + " track(s), Length: " + TimeSpan.FromSeconds((float)totalLength).ToString("m\\:ss");
            UpdateFormTitle();
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

        private void InitializeAudio(AwcAudio audio, float playBegin = 0)
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
                var audio = item.Tag as AwcAudio;

                if (audio != null)
                {
                    InitializeAudio(audio);
                    sourceVoice.Start();
                    SetPlayerState(PlayerState.Playing);
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

            sourceVoice.Stop();
            InitializeAudio(currentAudio, PositionTrackBar.Value / 1000);
            sourceVoice.Start();
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
                var audio = item.Tag as AwcAudio;

                saveFileDialog.FileName = audio.Name + ".wav";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
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
}
