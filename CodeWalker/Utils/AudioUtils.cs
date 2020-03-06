using CodeWalker.GameFiles;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Utils
{



    public class AudioPlayer
    {

        private AwcStream currentAudio;
        private Stream wavStream;
        private SoundStream soundStream;
        private XAudio2 xAudio2;
        private MasteringVoice masteringVoice;
        private AudioBuffer audioBuffer;
        private SourceVoice sourceVoice;

        public enum PlayerState { Stopped, Playing, Paused };
        public PlayerState State { get; private set; } = PlayerState.Stopped;

        private Stopwatch playtimer = new Stopwatch();
        private int playBeginMs;
        private float trackLength;
        public bool trackFinished;

        private float volume = 1.0f;


        public int PlayTimeMS
        {
            get
            {
                return (int)playtimer.Elapsed.TotalMilliseconds + playBeginMs;
            }
        }
        public int TotalTimeMS
        {
            get
            {
                return (int)(trackLength * 1000);
            }
        }


        public void LoadAudio(AwcStream audio)
        {
            if (audio != currentAudio)
            {
                CloseStreams();

                currentAudio = audio;
                trackLength = audio.Length;
                wavStream = audio.GetWavStream();
                soundStream = new SoundStream(wavStream);
            }
        }

        private void CloseStreams()
        {
            if (soundStream != null)
            {
                soundStream.Close();
            }
            if (wavStream != null)
            {
                wavStream.Close();//is this necessary?
            }
        }

        private void InitializeAudio(float playBegin = 0)
        {
            if (xAudio2 == null)
            {
                xAudio2 = new XAudio2();
                masteringVoice = new MasteringVoice(xAudio2);
            }

            wavStream.Position = 0;
            soundStream.Position = 0;
            audioBuffer = new AudioBuffer
            {
                Stream = soundStream.ToDataStream(),
                AudioBytes = (int)soundStream.Length,
                Flags = BufferFlags.EndOfStream
            };
            if (playBegin > 0)
            {
                audioBuffer.PlayBegin = (int)(soundStream.Format.SampleRate * playBegin) / 128 * 128;
                if (playtimer.IsRunning)
                {
                    playtimer.Restart();
                }
                else
                {
                    playtimer.Reset();
                }
                playBeginMs = (int)(playBegin * 1000);
            }
            else
            {
                playBeginMs = 0;
            }

            trackFinished = false;
            sourceVoice = new SourceVoice(xAudio2, soundStream.Format, true);
            sourceVoice.SubmitSourceBuffer(audioBuffer, soundStream.DecodedPacketsInfo);
            sourceVoice.BufferEnd += (context) => trackFinished = true;
            sourceVoice.SetVolume(volume);
        }

        private void SetPlayerState(PlayerState newState)
        {
            if (State != newState)
            {
                switch (newState)
                {
                    case PlayerState.Playing:
                        if (State == PlayerState.Stopped)
                        {
                            playtimer.Reset();
                        }
                        playtimer.Start();
                        break;
                    case PlayerState.Paused:
                        playtimer.Stop();
                        break;
                    case PlayerState.Stopped:
                        playtimer.Stop();
                        break;
                }

                State = newState;
            }
        }

        public void SetVolume(float v)
        {
            volume = v;
            if (State == PlayerState.Playing)
            {
                sourceVoice.SetVolume(v);
            }
        }

        public void DisposeAudio()
        {
            CloseStreams();
            if (xAudio2 != null)
            {
                masteringVoice.Dispose();
                xAudio2.Dispose();
            }
        }


        public void Stop()
        {
            if (State != PlayerState.Stopped)
            {
                sourceVoice.DestroyVoice();
                sourceVoice.Dispose();
                audioBuffer.Stream.Dispose();
                SetPlayerState(PlayerState.Stopped);
            }
        }

        public void Play(float playBegin = 0)
        {
            Stop();
            InitializeAudio(playBegin);
            sourceVoice.Start();
            SetPlayerState(PlayerState.Playing);
        }

        public void Seek(float playBegin = 0)
        {
            if (State == PlayerState.Playing)
            {
                Play(playBegin);
            }
            else if (State == PlayerState.Paused)
            {
                var state = State;
                Stop();
                InitializeAudio(playBegin);
                State = state;
            }
        }

        public void Pause()
        {
            if (State == PlayerState.Playing)
            {
                sourceVoice.Stop();
                SetPlayerState(PlayerState.Paused);
            }
        }

        public void Resume()
        {
            if (State == PlayerState.Paused)
            {
                sourceVoice.Start();
                SetPlayerState(PlayerState.Playing);
            }
        }


    }



}
