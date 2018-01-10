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
        private float trackLength;

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
            if (awc.Audios != null)
            {
                foreach (var audio in awc.Audios)
                {
                    var item = PlayListView.Items.Add(audio.Name);
                    item.SubItems.Add(audio.Type);
                    item.SubItems.Add(audio.LengthStr);
                    item.Tag = audio;
                }
            }

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
                        break;
                    default:
                        playtime.Stop();
                        break;
                }                

                playerState = newState;
            }
        }

        private void InitializeAudio(AwcAudio audio)
        {
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
            soundStream.Close();
            wavStream.Close();

            sourceVoice = new SourceVoice(xAudio2, soundStream.Format, true);
            sourceVoice.SubmitSourceBuffer(audioBuffer, soundStream.DecodedPacketsInfo);
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (playerState != PlayerState.Stopped)
            {
                int playedMs = (int)playtime.Elapsed.TotalMilliseconds;
                int totalMs = (int)(trackLength * 1000);
                PositionTrackBar.Maximum = totalMs;
                PositionTrackBar.Value = playedMs < totalMs ? playedMs : totalMs;
            }
            else
            {
                PositionTrackBar.Value = 0;
            }
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
    }
}
