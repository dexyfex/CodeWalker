using CodeWalker.GameFiles;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private bool Playing = false;


        public AwcForm()
        {
            InitializeComponent();
        }



        private void UpdateFormTitle()
        {
            Text = fileName + " - AWC Player - CodeWalker by dexyfex";
        }


        public void LoadAwc(AwcFile awc)
        {
            Awc = awc;
            DetailsPropertyGrid.SelectedObject = awc;

            //MainTabControl.SelectedTab = DetailsTabPage;//remove this

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


        private void Play()
        {
            if (PlayListView.SelectedItems.Count != 1) return;

            var item = PlayListView.SelectedItems[0];
            var audio = item.Tag as AwcAudio;

            if (audio == null) return;



            //see https://github.com/sharpdx/SharpDX-Samples/blob/master/Desktop/XAudio2/PlaySound/Program.cs
            //see https://github.com/sharpdx/SharpDX-Samples/blob/master/Desktop/XAudio2/AudioPlayerApp/AudioPlayer.cs

            //var mstrm = new MemoryStream(audio.Data);
            //var sstrm = new SoundStream(mstrm);
            //SourceVoice sv=new SourceVoice()
            var mstrm = audio.GetWavStream();

            ////var mdata = ((MemoryStream)mstrm).GetBuffer();
            ////File.WriteAllBytes("C:\\test2.wav", mdata);
            ////return;

            //var sstrm = new SoundStream(mstrm);
            //var waveFormat = sstrm.Format;
            //var buffer = new AudioBuffer
            //{
            //    Stream = sstrm.ToDataStream(),
            //    AudioBytes = (int)sstrm.Length,
            //    Flags = BufferFlags.EndOfStream
            //};
            //sstrm.Close();


            //var xaudio2 = new XAudio2();//cache this...
            //var masteringVoice = new MasteringVoice(xaudio2);//cache this...
            //var sourceVoice = new SourceVoice(xaudio2, waveFormat, true);
            ////sourceVoice.BufferEnd += (context) => Console.WriteLine(" => event received: end of buffer");
            //sourceVoice.SubmitSourceBuffer(buffer, sstrm.DecodedPacketsInfo);
            //sourceVoice.Start();
            //while (sourceVoice.State.BuffersQueued > 0) // && !IsKeyPressed(ConsoleKey.Escape))
            //{
            //    Thread.Sleep(10);
            //}
            //sourceVoice.DestroyVoice();
            //sourceVoice.Dispose();
            //buffer.Stream.Dispose();

            //masteringVoice.Dispose();//on form exit?
            //xaudio2.Dispose();//on form exit?



            Playing = true;
        }

        private void Pause()
        {

            Playing = false;
        }

        private void Prev()
        {
        }

        private void Next()
        {
        }



        private void PositionTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (Playing) Pause();
            else Play();
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            Prev();
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            Next();
        }

        private void VolumeButton_Click(object sender, EventArgs e)
        {

        }

        private void VolumeTrackBar_Scroll(object sender, EventArgs e)
        {

        }
    }






    public class AudioPlayer
    {

    }



}
