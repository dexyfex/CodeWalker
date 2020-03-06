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
        private XAudio2 xAudio2;
        private MasteringVoice masteringVoice;

        public class AudioVoice
        {
            public AwcStream audio;
            public SoundStream soundStream;
            public AudioBuffer audioBuffer;
            public SourceVoice sourceVoice;
            public float[] outputMatrix = new[] { 1.0f, 1.0f }; //left/right channel output levels
            public float trackLength;
        }
        private AudioVoice[] voices = new AudioVoice[0];

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


        public void LoadAudio(params AwcStream[] audios)
        {
            if (xAudio2 == null)
            {
                xAudio2 = new XAudio2();
                masteringVoice = new MasteringVoice(xAudio2);
            }

            if ((voices == null) || (voices.Length != audios.Length))
            {
                voices = new AudioVoice[audios.Length];
                for (int i = 0; i < audios.Length; i++)
                {
                    voices[i] = new AudioVoice();
                }
            }

            trackLength = 0;
            for (int i = 0; i < audios.Length; i++)
            {
                var voice = voices[i];
                var audio = audios[i];
                if (audio != voice.audio)
                {
                    voice.audio = audio;
                    voice.trackLength = audio.Length;
                    trackLength = Math.Max(trackLength, voice.trackLength);
                    var wavStream = audio.GetWavStream();
                    var soundStream = new SoundStream(wavStream);
                    voice.soundStream = soundStream;
                    voice.audioBuffer = new AudioBuffer
                    {
                        Stream = soundStream.ToDataStream(),
                        AudioBytes = (int)soundStream.Length,
                        Flags = BufferFlags.EndOfStream
                    };
                    soundStream.Close();
                    wavStream.Close();
                }
            }

        }

        private void CreateSourceVoices(float playBegin = 0)
        {
            if (playBegin > 0)
            {
                foreach (var voice in voices)
                {
                    voice.audioBuffer.PlayBegin = (int)(voice.soundStream.Format.SampleRate * playBegin) / 128 * 128;
                }
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
            foreach (var voice in voices)
            {
                var sourceVoice = new SourceVoice(xAudio2, voice.soundStream.Format, true);
                sourceVoice.SubmitSourceBuffer(voice.audioBuffer, voice.soundStream.DecodedPacketsInfo);
                sourceVoice.BufferEnd += (context) => trackFinished = true;
                sourceVoice.SetVolume(volume);
                sourceVoice.SetOutputMatrix(1, 2, voice.outputMatrix);
                voice.sourceVoice = sourceVoice;
            }
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
                foreach (var voice in voices)
                {
                    voice.sourceVoice.SetVolume(v);
                }
            }
        }

        public void SetOutputMatrix(int v, float l, float r)
        {
            var voice = voices[v];
            voice.outputMatrix[0] = l;
            voice.outputMatrix[1] = r;
            if (State == PlayerState.Playing)
            {
                voice.sourceVoice.SetOutputMatrix(1, 2, voice.outputMatrix);
            }
        }

        public void DisposeAudio()
        {
            if (xAudio2 != null)
            {
                masteringVoice.Dispose();
                xAudio2.Dispose();
            }
            foreach (var voice in voices)
            {
                voice?.audioBuffer?.Stream?.Dispose();
            }
        }


        public void Stop()
        {
            if (State != PlayerState.Stopped)
            {
                foreach (var voice in voices)
                {
                    voice.sourceVoice.DestroyVoice();
                    voice.sourceVoice.Dispose();
                }
                SetPlayerState(PlayerState.Stopped);
            }
        }

        public void Play(float playBegin = 0)
        {
            Stop();
            CreateSourceVoices(playBegin);
            foreach (var voice in voices)
            {
                voice.sourceVoice.Start();
            }
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
                CreateSourceVoices(playBegin);
                State = state;
            }
        }

        public void Pause()
        {
            if (State == PlayerState.Playing)
            {
                foreach (var voice in voices)
                {
                    voice.sourceVoice.Stop();
                }
                SetPlayerState(PlayerState.Paused);
            }
        }

        public void Resume()
        {
            if (State == PlayerState.Paused)
            {
                foreach (var voice in voices)
                {
                    voice.sourceVoice.Start();
                }
                SetPlayerState(PlayerState.Playing);
            }
        }

    }



    public class AudioDatabase
    {

        public bool IsInited { get; set; }

        public Dictionary<uint, Dat54Sound> SoundsDB { get; set; }
        public Dictionary<uint, Dat151RelData> GameDB { get; set; }
        public Dictionary<uint, RpfFileEntry> ContainerDB { get; set; }

        public void Init(GameFileCache gameFileCache, bool sounds = true, bool game = true)
        {


            var rpfman = gameFileCache.RpfMan;

            var datrelentries = new Dictionary<uint, RpfFileEntry>();
            var awcentries = new Dictionary<uint, RpfFileEntry>();
            void addRpfDatRels(RpfFile rpffile)
            {
                if (rpffile.AllEntries == null) return;
                foreach (var entry in rpffile.AllEntries)
                {
                    if (entry is RpfFileEntry)
                    {
                        var fentry = entry as RpfFileEntry;
                        //if (entry.NameLower.EndsWith(".rel"))
                        //{
                        //    datrels[entry.NameHash] = fentry;
                        //}
                        if (sounds && entry.NameLower.EndsWith(".dat54.rel"))
                        {
                            datrelentries[entry.NameHash] = fentry;
                        }
                        if (game && entry.NameLower.EndsWith(".dat151.rel"))
                        {
                            datrelentries[entry.NameHash] = fentry;
                        }
                    }
                }
            }
            void addRpfAwcs(RpfFile rpffile)
            {
                if (rpffile.AllEntries == null) return;
                foreach (var entry in rpffile.AllEntries)
                {
                    if (entry is RpfFileEntry)
                    {
                        var fentry = entry as RpfFileEntry;
                        if (entry.NameLower.EndsWith(".awc"))
                        {
                            var shortname = entry.GetShortNameLower();
                            var parentname = entry.Parent?.GetShortNameLower() ?? "";
                            if (string.IsNullOrEmpty(parentname) && (entry.Parent?.File != null))
                            {
                                parentname = entry.Parent.File.NameLower;
                                int ind = parentname.LastIndexOf('.');
                                if (ind > 0)
                                {
                                    parentname = parentname.Substring(0, ind);
                                }
                            }
                            var contname = parentname + "/" + shortname;
                            var hash = JenkHash.GenHash(contname);
                            awcentries[hash] = fentry;
                        }
                    }
                }
            }

            var audrpf = rpfman.FindRpfFile("x64\\audio\\audio_rel.rpf");
            if (audrpf != null)
            {
                addRpfDatRels(audrpf);
            }
            foreach (var baserpf in gameFileCache.BaseRpfs)
            {
                addRpfAwcs(baserpf);
            }
            if (gameFileCache.EnableDlc)
            {
                var updrpf = rpfman.FindRpfFile("update\\update.rpf");
                if (updrpf != null)
                {
                    addRpfDatRels(updrpf);
                }
                foreach (var dlcrpf in gameFileCache.DlcActiveRpfs) //load from current dlc rpfs
                {
                    addRpfDatRels(dlcrpf);
                    addRpfAwcs(dlcrpf);
                }
            }



            var soundsdb = new Dictionary<uint, Dat54Sound>();
            var gamedb = new Dictionary<uint, Dat151RelData>();
            foreach (var datentry in datrelentries.Values)
            {
                var relfile = rpfman.GetFile<RelFile>(datentry);
                if (relfile?.RelDatas != null)
                {
                    foreach (var rd in relfile.RelDatas)
                    {
                        if (rd is Dat54Sound sd)
                        {
                            soundsdb[sd.NameHash] = sd;
                        }
                        else if (rd is Dat151RelData gd)
                        {
                            gamedb[gd.NameHash] = gd;
                        }
                    }
                }
            }

            ContainerDB = awcentries;
            if (sounds) SoundsDB = soundsdb;
            if (game) GameDB = gamedb;

            IsInited = true;
        }


    }


}
