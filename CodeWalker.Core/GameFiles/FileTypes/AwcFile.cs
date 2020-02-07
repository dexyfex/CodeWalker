using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))]public class AwcFile : PackedFile
    {
        public string Name { get; set; }
        public RpfFileEntry FileEntry { get; set; }
        public byte[] Data { get; set; }

        public string ErrorMessage { get; set; }

        public uint Magic { get; set; }
        public ushort Version { get; set; }
        public ushort Flags { get; set; }
        public int StreamCount { get; set; }
        public int InfoOffset { get; set; }

        public bool UnkUshortsFlag { get { return ((Flags & 1) == 1); } }
        public bool SingleChannelEncryptFlag { get { return ((Flags & 2) == 2); } }
        public bool MultiChannelFlag { get { return ((Flags & 4) == 4); }  }
        public bool MultiChannelEncryptFlag { get { return ((Flags & 8) == 8); } }

        public ushort[] UnkUshorts { get; set; } //offsets of some sort?


        public bool WholeFileEncrypted { get; set; }

        public AwcStreamInfo[] StreamInfos { get; set; }
        public uint[] AudioIds { get; set; }
        public AwcAudio[] Audios { get; set; }

        static public void Decrypt_RSXXTEA(byte[] data, uint[] key)
        {
            // Rockstar's modified version of XXTEA
            uint[] blocks = new uint[data.Length / 4];
            Buffer.BlockCopy(data, 0, blocks, 0, data.Length);

            int block_count = blocks.Length;
            uint a, b = blocks[0], i;

            i = (uint)(0x9E3779B9 * (6 + 52 / block_count));
            do
            {
                for (int block_index = block_count - 1; block_index >= 0; --block_index)
                {
                    a = blocks[(block_index > 0 ? block_index : block_count) - 1];
                    b = blocks[block_index] -= (a >> 5 ^ b << 2) + (b >> 3 ^ a << 4) ^ (i ^ b) + (key[block_index & 3 ^ (i >> 2 & 3)] ^ a ^ 0x7B3A207F);
                }
                i -= 0x9E3779B9;
            } while (i != 0);

            Buffer.BlockCopy(blocks, 0, data, 0, data.Length);
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {

            //adapted from libertyV code


            //MemoryStream ms = new MemoryStream(data);
            Name = entry.Name;
            FileEntry = entry;
            Data = data;

            if ((data == null) || (data.Length < 8))
            {
                ErrorMessage = "Data null or too short!";
                return; //nothing to do, not enough data...
            }

            Endianess endianess = Endianess.LittleEndian;

            Magic = BitConverter.ToUInt32(data, 0);
            if (Magic != 0x54414441 && Magic != 0x41444154)
            {
                if (data.Length % 4 == 0)
                {
                    Decrypt_RSXXTEA(data, GTA5Keys.PC_AWC_KEY);
                    Magic = BitConverter.ToUInt32(data, 0);
                    WholeFileEncrypted = true;
                }
                else
                {
                    ErrorMessage = "Corrupted data!";
                }
            }

            switch (Magic)
            {
                default:
                    ErrorMessage = "Unexpected Magic 0x" + Magic.ToString("X");
                    return;
                case 0x54414441:
                    endianess = Endianess.LittleEndian;
                    break;
                case 0x41444154:
                    endianess = Endianess.BigEndian;
                    break;
            }

            using (MemoryStream ms = new MemoryStream(data))
            {
                DataReader r = new DataReader(ms, endianess);

                Magic = r.ReadUInt32();
                Version = r.ReadUInt16();
                Flags = r.ReadUInt16();
                StreamCount = r.ReadInt32();
                InfoOffset = r.ReadInt32();


                if ((Flags >> 8) != 0xFF)
                {
                    ErrorMessage = "Flags 0 not supported!";
                    return;
                }

                if (UnkUshortsFlag)
                {
                    UnkUshorts = new ushort[StreamCount]; //offsets of some sort?
                    for (int i = 0; i < StreamCount; i++)
                    {
                        UnkUshorts[i] = r.ReadUInt16();
                    }
                }


                var infoStart = 16 + (UnkUshortsFlag ? (StreamCount * 2) : 0);
                if (ms.Position != infoStart)
                { }//no hit
                ms.Position = infoStart;


                List<AwcStreamInfo> infos = new List<AwcStreamInfo>();
                Dictionary<uint, AwcStreamInfo> infoDict = new Dictionary<uint, AwcStreamInfo>();

                for (int i = 0; i < StreamCount; i++)
                {
                    var info = new AwcStreamInfo(r);
                    infos.Add(info);
                    infoDict[info.Id] = info;
                }
                for (int i = 0; i < StreamCount; i++)
                {
                    var info = infos[i];
                    for (int j = 0; j < info.ChunkCount; j++)
                    {
                        var chunk = new AwcChunkInfo(r);
                        info.ChunkList.Add(chunk);
                        info.ChunkDict[chunk.Tag] = chunk;
                    }
                }

                StreamInfos = infos.ToArray();

                ReadAudios(r);


            }
        }

        private void ReadAudios(DataReader r)
        {

            List<uint> audioIds = new List<uint>();
            List<AwcAudio> audios = new List<AwcAudio>();
            AwcAudio multiaudio = null;

            for (int i = 0; i < StreamCount; i++)
            {
                var info = StreamInfos[i];
                var audio = new AwcAudio();
                audio.StreamInfo = info;

                for (int j = 0; j < info.ChunkList.Count; j++)
                {
                    var chunk = info.ChunkList[j];
                    r.Position = chunk.Offset;
                    switch (chunk.Tag)
                    {
                        case 85: //data
                            audio.DataInfo = chunk;
                            audio.Data = r.ReadBytes(chunk.Size);
                            break;
                        case 250://format
                            audio.Format = new AwcFormatChunk(r, chunk);
                            break;
                        case 92: //animation  - YCD resource chunk
                            audio.ClipDict = new AwcAudioAnimClipDict(r, chunk);
                            break;
                        case 43: //unk2B
                            audio.UnkData2B = new AwcAudioUnk(r, chunk);
                            break;
                        case 54: //unk36  - small number of bytes (2+)
                            audio.UnkData36 = new AwcAudioUnk(r, chunk);
                            break;
                        case 90: //unk5A
                            audio.UnkData5A = new AwcAudioUnk(r, chunk);
                            break;
                        case 104://MIDI
                            audio.MIDIData = new AwcAudioMIDI(r, chunk);
                            break;
                        case 217://unkD9  - length 200+
                            audio.UnkDataD9 = new AwcAudioUnk(r, chunk);
                            break;
                        case 72: //multi channel info
                            audio.MultiChannelInfo = new AwcChannelBlockInfo(r);
                            break;
                        case 163://multi unkA3
                            audio.MultiChannelSampleOffsets = new AwcChannelOffsetsInfo(r, chunk);
                            break;
                        case 189://multi unkBD
                            audio.UnkDataBD = new AwcAudioUnk(r, chunk);
                            break;
                        default:
                            break;
                    }
                    if ((r.Position - chunk.Offset) != chunk.Size)
                    { }//make sure everything was read!
                }


                //decrypt data where necessary
                if ((audio.Data != null) && (!WholeFileEncrypted))
                {
                    if (MultiChannelFlag)
                    {
                        var ocount = (int)(audio.MultiChannelSampleOffsets?.SampleOffsets?.Length ?? 0);
                        var ccount = (int)(audio.MultiChannelInfo?.ChannelCount ?? 0);
                        var bcount = (int)(audio.MultiChannelInfo?.BlockCount ?? 0);
                        var bsize = (int)(audio.MultiChannelInfo?.BlockSize ?? 0);
                        var blist = new List<AwcChannelDataBlock>();
                        for (int b = 0; b < bcount; b++)
                        {
                            int srcoff = b * bsize;
                            int mcsoff = (b < ocount) ? (int)audio.MultiChannelSampleOffsets.SampleOffsets[b] : 0;
                            int blen = Math.Max(Math.Min(bsize, audio.Data.Length - srcoff), 0);
                            var bdat = new byte[blen];
                            Buffer.BlockCopy(audio.Data, srcoff, bdat, 0, blen);
                            if (MultiChannelEncryptFlag)
                            {
                                AwcFile.Decrypt_RSXXTEA(bdat, GTA5Keys.PC_AWC_KEY);
                            }
                            var blk = new AwcChannelDataBlock(bdat, audio.MultiChannelInfo, r.Endianess, b, mcsoff);
                            blist.Add(blk);
                        }
                        audio.MultiChannelBlocks = blist.ToArray();
                        multiaudio = audio;
                    }
                    else
                    {
                        if (SingleChannelEncryptFlag)
                        {
                            AwcFile.Decrypt_RSXXTEA(audio.Data, GTA5Keys.PC_AWC_KEY);
                        }
                    }
                }
                if ((audio.Data != null) && WholeFileEncrypted && MultiChannelFlag)
                { }//no hit


                audios.Add(audio);
                audioIds.Add(info.Id);
            }

            if (multiaudio != null)
            {
                var multichaninfos = multiaudio.MultiChannelInfo;
                for (int i = 0; i < audios.Count; i++)
                {
                    var audio = audios[i];
                    if (audio != multiaudio)
                    {
                        var id = audio.StreamInfo?.Id ?? 0;
                        var srcind = 0;
                        var chancnt = multiaudio.MultiChannelInfo?.Channels?.Length ?? 0;
                        var found = false;
                        for (int ind = 0; ind < chancnt; ind++)
                        {
                            var mchan = multiaudio.MultiChannelInfo.Channels[ind];
                            if (mchan.Id == id)
                            {
                                srcind = ind;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        { }//no hit

                        audio.MultiChannelSource = multiaudio;
                        audio.MultiChannelFormat = (srcind < multichaninfos?.Channels?.Length) ? multichaninfos.Channels[srcind] : null;
                        audio.MultiChannelIndex = srcind;
                    }

                }
            }

            Audios = audios.ToArray();
            AudioIds = audioIds.ToArray();

        }

    }

    public enum AwcCodecFormat
    {
        PCM = 0,
        ADPCM = 4
    }

    [TC(typeof(EXP))] public class AwcStreamInfo
    {
        public uint RawVal { get; set; }
        public uint ChunkCount { get; set; }
        public uint Id { get; set; }

        public List<AwcChunkInfo> ChunkList { get; set; } = new List<AwcChunkInfo>();
        public Dictionary<byte, AwcChunkInfo> ChunkDict { get; set; } = new Dictionary<byte, AwcChunkInfo>();

        public AwcStreamInfo(DataReader r)
        {
            RawVal = r.ReadUInt32();
            ChunkCount = (RawVal >> 29);
            Id = (RawVal & 0x1FFFFFFF);
        }

        public override string ToString()
        {
            return Id.ToString("X") + ": " + ChunkCount.ToString() + " chunks";
        }
    }

    [TC(typeof(EXP))] public class AwcChunkInfo
    {
        public ulong RawVal { get; set; }
        public byte Tag { get; set; }
        public int Size { get; set; }
        public int Offset { get; set; }

        public AwcChunkInfo(DataReader r)
        {
            RawVal = r.ReadUInt64();
            Tag = (byte)(RawVal >> 56);
            Size = (int)((RawVal >> 28) & 0x0FFFFFFF);
            Offset = (int)(RawVal & 0x0FFFFFFF);
        }

        public override string ToString()
        {
            return Tag.ToString() + ": " + Size.ToString() + ", " + Offset.ToString();
        }
    }

    [TC(typeof(EXP))] public class AwcChannelBlockInfo
    {
        public uint BlockCount { get; set; }
        public uint BlockSize { get; set; }
        public uint ChannelCount { get; set; }
        public AwcChannelBlockItemInfo[] Channels { get; set; }

        public uint TotalSize { get { return BlockCount * BlockSize; } }

        public AwcChannelBlockInfo(DataReader r)
        {
            BlockCount = r.ReadUInt32();
            BlockSize = r.ReadUInt32();
            ChannelCount = r.ReadUInt32();

            List<AwcChannelBlockItemInfo> channels = new List<AwcChannelBlockItemInfo>();
            for (int i = 0; i < ChannelCount; i++)
            {
                var itemInfo = new AwcChannelBlockItemInfo(r);
                channels.Add(itemInfo);
            }
            Channels = channels.ToArray();

        }

        public override string ToString()
        {
            return BlockCount.ToString() + ": " + BlockSize.ToString() + ", " + ChannelCount.ToString() + " channels";
        }
    }

    [TC(typeof(EXP))] public class AwcChannelBlockItemInfo
    {
        public uint Id { get; set; }
        public uint Samples { get; set; }
        public ushort Unk0 { get; set; }//flags?
        public ushort SamplesPerSecond { get; set; }
        public AwcCodecFormat Codec { get; set; }
        public byte RoundSize { get; set; }
        public ushort Unk2 { get; set; }

        public AwcChannelBlockItemInfo(DataReader r)
        {
            Id = r.ReadUInt32();
            Samples = r.ReadUInt32();
            Unk0 = r.ReadUInt16();
            SamplesPerSecond = r.ReadUInt16();
            Codec = (AwcCodecFormat)r.ReadByte();
            RoundSize = r.ReadByte();
            Unk2 = r.ReadUInt16();

            //switch (Unk0)
            //{
            //    case 65395:
            //    case 65485:
            //    case 33:
            //    case 9:
            //    case 674:
            //    case 529:
            //    case 11:
            //    case 105:
            //    case 65396:
            //    case 437:
            //    case 477:
            //    case 65475:
            //    case 519:
            //    case 349:
            //    case 65517:
            //    case 106:
            //    case 219:
            //    case 58:
            //    case 63:
            //    case 65530:
            //    case 65511:
            //    case 191:
            //    case 65439:
            //    case 65345:
            //    case 560:
            //    case 640:
            //    case 65335:
            //    case 208:
            //    case 392:
            //    case 1968:
            //    case 2257:
            //    case 1101:
            //    case 906:
            //    case 1087:
            //    case 718:
            //    case 65362:
            //    case 259:
            //    case 234:
            //    case 299:
            //    case 307:
            //    case 75:
            //    case 67:
            //    case 4:
            //    case 290:
            //    case 587:
            //    case 65448:
            //    case 15:
            //    case 398:
            //    case 399:
            //    case 65375:
            //    case 65376:
            //    case 65336:
            //        break;
            //    default:
            //        break;//more... flags?
            //}
            //switch (Codec)
            //{
            //    case AwcCodecFormat.ADPCM:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unk2)
            //{
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}
        }

        public override string ToString()
        {
            return Id.ToString() + ", " + Codec.ToString() + ": " + Samples.ToString() + " samples, " + SamplesPerSecond.ToString() + " samples/sec, size: " + RoundSize.ToString() + " unk0: " + Unk0.ToString();
        }
    }

    [TC(typeof(EXP))] public class AwcFormatChunk
    {
        public uint Samples { get; set; }
        public int LoopPoint { get; set; }
        public ushort SamplesPerSecond { get; set; }
        public short Headroom { get; set; }
        public ushort LoopBegin { get; set; }
        public ushort LoopEnd { get; set; }
        public ushort PlayEnd { get; set; }
        public byte PlayBegin { get; set; }
        public uint UnkExtra { get; set; }

        public AwcCodecFormat Codec { get; set; }

        public AwcFormatChunk(DataReader r, AwcChunkInfo info)
        {
            Samples = r.ReadUInt32();
            LoopPoint = r.ReadInt32();
            SamplesPerSecond = r.ReadUInt16();
            Headroom = r.ReadInt16();
            LoopBegin = r.ReadUInt16();
            LoopEnd = r.ReadUInt16();
            PlayEnd = r.ReadUInt16();
            PlayBegin = r.ReadByte();
            Codec = (AwcCodecFormat)r.ReadByte();

            switch (info.Size) //Apparently sometimes this struct is longer?
            {
                case 20:
                    break;
                case 24:
                    UnkExtra = r.ReadUInt32();
                    break;
                default:
                    break;//no hit
            }

        }


        public override string ToString()
        {
            return Headroom.ToString() + ", " + Codec.ToString() + ": " + Samples.ToString() + " samples, " + SamplesPerSecond.ToString() + " samples/sec";
        }
    }

    [TC(typeof(EXP))] public class AwcAudio
    {
        public AwcStreamInfo StreamInfo { get; set; }
        public AwcFormatChunk Format { get; set; }
        public AwcChunkInfo DataInfo { get; set; }
        public byte[] Data { get; set; }

        public AwcAudioAnimClipDict ClipDict { get; set; }
        public AwcAudioUnk UnkData2B { get; set; }
        public AwcAudioUnk UnkData36 { get; set; }
        public AwcAudioUnk UnkData5A { get; set; }
        public AwcAudioMIDI MIDIData { get; set; }
        public AwcAudioUnk UnkDataD9 { get; set; }
        public AwcChannelBlockInfo MultiChannelInfo { get; set; }
        public AwcChannelOffsetsInfo MultiChannelSampleOffsets { get; set; }
        public AwcChannelDataBlock[] MultiChannelBlocks { get; set; }
        public AwcAudioUnk UnkDataBD { get; set; }

        public AwcAudio MultiChannelSource { get; set; }
        public AwcChannelBlockItemInfo MultiChannelFormat { get; set; }
        public int MultiChannelIndex { get; set; }

        public short Channels = 1;
        public short BitsPerSample = 16;
        public int SamplesPerSecond
        {
            get
            {
                return Format?.SamplesPerSecond ?? MultiChannelFormat?.SamplesPerSecond ?? 0;
            }
        }
        public int SampleCount
        {
            get
            {
                return (int)(Format?.Samples ?? MultiChannelFormat?.Samples ?? 0);
            }
        }


        public string Name
        {
            get
            {
                return "0x" + StreamInfo?.Id.ToString("X").PadLeft(8, '0') ?? "0";
            }
        }
        public string Type
        {
            get
            {
                if (MIDIData != null)
                {
                    return "MIDI";
                }

                var fc = AwcCodecFormat.PCM;
                var hz = 0;
                if (Format != null)
                {
                    fc = Format.Codec;
                    hz = Format.SamplesPerSecond;
                }
                if (MultiChannelFormat != null)
                {
                    fc = MultiChannelFormat.Codec;
                    hz = MultiChannelFormat.SamplesPerSecond;
                }
                string codec = fc.ToString();
                switch (fc)
                {
                    case AwcCodecFormat.PCM:
                    case AwcCodecFormat.ADPCM:
                        break;
                    default:
                        codec = "Unknown";
                        break;
                }

                return codec + ((hz > 0) ? (", " + hz.ToString() + " Hz") : "");
            }
        }

        public float Length
        {
            get
            {
                if (Format != null) return (float)Format.Samples / Format.SamplesPerSecond;
                if (MultiChannelFormat != null) return (float)MultiChannelFormat.Samples / MultiChannelFormat.SamplesPerSecond;
                return 0;
            }
        }
        public string LengthStr
        {
            get
            {
                return TimeSpan.FromSeconds(Length).ToString("m\\:ss");
            }
        }

        public int ByteLength
        {
            get
            {
                if (MIDIData?.Data != null)
                {
                    return MIDIData.Data?.Length ?? 0;
                }
                if (Data != null)
                {
                    return Data.Length;
                }
                if (MultiChannelSource != null)
                {
                    int c = 0;
                    if (MultiChannelSource?.MultiChannelBlocks != null)
                    {
                        foreach (var blk in MultiChannelSource.MultiChannelBlocks)
                        {
                            if (MultiChannelIndex < (blk?.Channels?.Length ?? 0))
                            {
                                var chan = blk.Channels[MultiChannelIndex];
                                c += chan?.ChannelData?.Length ?? 0;
                            }
                        }
                    }
                    return c;
                }
                return 0;
            }
        }


        public AwcAudio()
        {
        }
        public AwcAudio(DataReader r, AwcStreamInfo s, AwcFormatChunk f, AwcChunkInfo d)
        {
            StreamInfo = s;
            Format = f;
            DataInfo = d;

            Data = r.ReadBytes(d.Size);
        }

        public override string ToString()
        {
            var hash = "0x" + (StreamInfo?.Id.ToString("X") ?? "0").PadLeft(8, '0') + ": ";
            if (Format != null)
            {
                return hash + Format?.ToString() ?? "AwcAudio";
            }
            if (MultiChannelFormat != null)
            {
                return hash + MultiChannelFormat?.ToString() ?? "AwcAudio";
            }
            if (MIDIData != null)
            {
                return hash + MIDIData.ToString(); 
            }
            return hash + "Unknown";
        }


        public byte[] GetWavData()
        {
            if (MultiChannelFormat != null)
            {
                if (Data == null)
                {
                    var ms = new MemoryStream();
                    var bw = new BinaryWriter(ms);
                    if (MultiChannelSource?.MultiChannelBlocks != null)
                    {
                        foreach (var blk in MultiChannelSource.MultiChannelBlocks)
                        {
                            if (MultiChannelIndex < (blk?.Channels?.Length ?? 0))
                            {
                                var chan = blk.Channels[MultiChannelIndex];
                                var cdata = chan.ChannelData;
                                bw.Write(cdata);
                            }
                        }
                    }
                    bw.Flush();
                    ms.Position = 0;
                    Data = new byte[ms.Length];
                    ms.Read(Data, 0, (int)ms.Length);
                }
            }
            return Data;
        }

        public Stream GetWavStream()
        {
            byte[] dataPCM = GetWavData();
            var bitsPerSample = BitsPerSample;
            var channels = Channels;
            var codec = MultiChannelFormat?.Codec ?? Format?.Codec ?? AwcCodecFormat.PCM;

            if (codec == AwcCodecFormat.ADPCM)//just convert ADPCM to PCM for compatibility reasons
            {
                dataPCM = ADPCMCodec.DecodeADPCM(dataPCM, SampleCount);
                bitsPerSample = 16;
            }

            short formatcodec = 1; // 1 = WAVE_FORMAT_PCM
            int byteRate = SamplesPerSecond * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);
            short samplesPerBlock = 0;
            bool addextrafmt = false;

            //if (codec == AwcCodecFormat.ADPCM)//can't seem to get ADPCM wav files to work :(
            //{
            //    bitsPerSample = 4;
            //    formatcodec = 17;
            //    byteRate = (int)(SamplesPerSecond * 0.50685 * channels);
            //    blockAlign = 2048;// (short)(256 * (4 * channels));// (short)(36 * channels);//256;// 2048;// 
            //    samplesPerBlock = 4088;// (short)(((blockAlign - (4 * channels)) * 8) / (bitsPerSample * channels) + 1); // 2044;// 
            //    addextrafmt = true;
            //}


            MemoryStream stream = new MemoryStream();
            BinaryWriter w = new BinaryWriter(stream);
            int wavLength = 36 + dataPCM.Length;
            if (addextrafmt)
            {
                wavLength += 4;
            }

            // RIFF chunk
            w.Write("RIFF".ToCharArray());
            w.Write((int)wavLength);
            w.Write("WAVE".ToCharArray());

            // fmt sub-chunk     
            w.Write("fmt ".ToCharArray());
            w.Write((int)(addextrafmt ? 20 : 16)); // fmt size
            w.Write((short)formatcodec);
            w.Write((short)channels);
            w.Write((int)SamplesPerSecond);
            w.Write((int)byteRate);
            w.Write((short)blockAlign);
            w.Write((short)bitsPerSample);
            if (addextrafmt)
            {
                w.Write((ushort)0x0002);
                w.Write((ushort)samplesPerBlock);
            }

            // data sub-chunk
            w.Write("data".ToCharArray());
            w.Write((int)dataPCM.Length);
            w.Write(dataPCM);

            w.Flush();
            stream.Position = 0;
            return stream;
        }
    }

    [TC(typeof(EXP))] public class AwcAudioAnimClipDict
    {
        public byte[] Data { get; set; }
        public ClipDictionary ClipDict { get; set; }

        public AwcAudioAnimClipDict(DataReader r, AwcChunkInfo info)
        {
            Data = r.ReadBytes(info.Size);

            if ((Data == null) || (Data.Length < 16)) return;

            var data = Data;

            RpfResourceFileEntry resentry = new RpfResourceFileEntry();
            uint rsc7 = BitConverter.ToUInt32(data, 0);
            int version = BitConverter.ToInt32(data, 4);
            resentry.SystemFlags = BitConverter.ToUInt32(data, 8);
            resentry.GraphicsFlags = BitConverter.ToUInt32(data, 12);

            if (rsc7 != 0x37435352)
            { } //testing..
            if (version != 46) //46 is Clip Dictionary...
            { }

            int newlen = data.Length - 16; //trim the header from the data passed to the next step.
            int arrlen = Math.Max(newlen, resentry.SystemSize + resentry.GraphicsSize);//expand it as necessary for the reader.
            byte[] newdata = new byte[arrlen];
            Buffer.BlockCopy(data, 16, newdata, 0, newlen);
            data = newdata;

            ResourceDataReader rd = new ResourceDataReader(resentry, data);

            ClipDict = rd.ReadBlock<ClipDictionary>();


        }

        public override string ToString()
        {
            return (ClipDict?.ClipsMapEntries ?? 0).ToString() + " entries";
        }
    }

    [TC(typeof(EXP))] public class AwcAudioUnk
    {
        public AwcChunkInfo ChunkInfo { get; set; }
        public byte[] Data { get; set; }

        public AwcAudioUnk(DataReader r, AwcChunkInfo info)
        {
            ChunkInfo = info;
            Data = r.ReadBytes(info.Size);
        }

        public override string ToString()
        {
            if (Data == null) return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Data.Length; i++)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(Data[i].ToString());
            }
            return sb.ToString();
        }
    }

    
    [TC(typeof(EXP))] public class AwcAudioMIDI
    {
        public AwcChunkInfo ChunkInfo { get; set; }
        public byte[] Data { get; set; }

        public AwcAudioMIDI(DataReader r, AwcChunkInfo info)
        {
            ChunkInfo = info;
            Data = r.ReadBytes(info.Size);
        }

        public override string ToString()
        {
            return "MIDI - " + (Data?.Length??0).ToString() + " bytes";
        }
    }

    [TC(typeof(EXP))] public class AwcChannelOffsetsInfo
    {
        public AwcChunkInfo ChunkInfo { get; set; }
        public uint[] SampleOffsets { get; set; }

        public AwcChannelOffsetsInfo(DataReader r, AwcChunkInfo info)
        {
            ChunkInfo = info;
            var count = info.Size / 4;
            var rem = info.Size % 4;
            if (rem != 0)
            { }
            SampleOffsets = new uint[count];
            for (int i = 0; i < count; i++)
            {
                SampleOffsets[i] = r.ReadUInt32();
            }
        }

        public override string ToString()
        {
            return (SampleOffsets?.Length ?? 0).ToString() + " items";
        }
    }

    [TC(typeof(EXP))] public class AwcChannelDataBlock
    {
        public byte[] Data { get; set; }
        public int Index { get; set; }
        public int SampleOffset { get; set; }

        public AwcChannelDataItem[] Channels { get; set; }


        public AwcChannelDataBlock(byte[] data, AwcChannelBlockInfo channelInfo, Endianess endianess, int index, int sampleOffset)
        {
            Data = data;
            Index = index;
            SampleOffset = sampleOffset;

            using (var ms = new MemoryStream(data))
            {
                var r = new DataReader(ms, endianess);

                var channels = channelInfo?.Channels;
                var channelcount = channelInfo?.ChannelCount ?? 0;

                var ilist = new List<AwcChannelDataItem>();
                for (int i = 0; i < channelcount; i++)
                {
                    var channel = new AwcChannelDataItem(r);
                    channel.ChannelInfo = ((channels != null) && (i < channels.Length)) ? channels[i] : null;
                    ilist.Add(channel);
                }
                Channels = ilist.ToArray();

                foreach (var channel in Channels)
                {
                    channel.ReadOffsets(r);
                }

                var padc = (0x800 - (r.Position % 0x800)) % 0x800;
                var padb = r.ReadBytes((int)padc);

                foreach (var channel in Channels)
                {
                    var bcnt = channel.OffsetCount * 2048;
                    channel.ChannelData = r.ReadBytes(bcnt);
                }
                if (r.Position != r.Length)
                { }//still more, just padding?



            }

        }

        public override string ToString()
        {
            return (Data?.Length ?? 0).ToString() + " bytes";
        }
    }

    [TC(typeof(EXP))] public class AwcChannelDataItem
    {
        public int Unk0 { get; set; }
        public int OffsetCount { get; set; }
        public int Unk2 { get; set; }
        public int SampleCount { get; set; }
        public int Unk4 { get; set; }
        public int Unk5 { get; set; }

        public int[] SampleOffsets { get; set; }

        public AwcChannelBlockItemInfo ChannelInfo { get; set; } //for convenience

        public byte[] ChannelData { get; set; }


        public AwcChannelDataItem(DataReader r)
        {
            Unk0 = r.ReadInt32();
            OffsetCount = r.ReadInt32();
            Unk2 = r.ReadInt32();
            SampleCount = r.ReadInt32();
            Unk4 = r.ReadInt32();
            Unk5 = r.ReadInt32();
        }

        public void ReadOffsets(DataReader r)
        {
            var olist = new List<int>();
            for (int i = 0; i < OffsetCount; i++)
            {
                var v = r.ReadInt32();
                olist.Add(v);
            }
            SampleOffsets = olist.ToArray();
        }

        public override string ToString()
        {
            return Unk0.ToString() + ", " + OffsetCount.ToString() + ", " + Unk2.ToString() + ", " + SampleCount.ToString() + ", " + Unk4.ToString() + ", " + Unk5.ToString();
        }
    }





    public class ADPCMCodec
    {

        private static int[] ima_index_table = 
        {
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8
        };

        private static short[] ima_step_table = 
        {
            7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
            19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
            50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
            130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
            337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
            876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
            2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
            5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
        };

        private static int clip(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static byte[] DecodeADPCM(byte[] data, int sampleCount)
        {
            byte[] dataPCM = new byte[data.Length * 4];
            int predictor = 0, stepIndex = 0;
            int readingOffset = 0, writingOffset = 0, bytesInBlock = 0;

            void parseNibble(byte nibble)
            {
                var step = ima_step_table[stepIndex];
                int diff = ((((nibble & 7) << 1) + 1) * step) >> 3;
                if ((nibble & 8) != 0) diff = -diff;
                predictor = predictor + diff;
                stepIndex = clip(stepIndex + ima_index_table[nibble], 0, 88);
                int samplePCM = clip(predictor, -32768, 32767);

                dataPCM[writingOffset] = (byte)(samplePCM & 0xFF);
                dataPCM[writingOffset + 1] = (byte)((samplePCM >> 8) & 0xFF);
                writingOffset += 2;
            }

            while ((readingOffset < data.Length) && (sampleCount > 0))
            {
                if (bytesInBlock == 0)
                {
                    stepIndex = clip(data[readingOffset], 0, 88);
                    predictor = BitConverter.ToInt16(data, readingOffset + 2);
                    bytesInBlock = 2044;
                    readingOffset += 4;
                }
                else
                {
                    parseNibble((byte)(data[readingOffset] & 0x0F));
                    parseNibble((byte)((data[readingOffset] >> 4) & 0x0F));
                    bytesInBlock--;
                    sampleCount -= 2;
                    readingOffset++;
                }
            }

            return dataPCM;
        }

        public static byte[] EncodeADPCM(byte[] data, int sampleCount)
        {
            byte[] dataPCM = new byte[data.Length / 4];

            int predictor = 0, stepIndex = 0;
            int readingOffset = 0, writingOffset = 0, bytesInBlock = 0;

            short readSample()
            {
                var s = BitConverter.ToInt16(data, readingOffset);
                readingOffset += 2;
                return s;
            }

            void writeInt16(short v)
            {
                var ba = BitConverter.GetBytes(v);
                dataPCM[writingOffset++] = ba[0];
                dataPCM[writingOffset++] = ba[1];
            }

            byte encodeNibble(int pcm16)
            {
                int delta = pcm16 - predictor;
                uint value = 0;
                if (delta < 0)
                {
                    value = 8;
                    delta = -delta;
                }

                var step = ima_step_table[stepIndex];
                var diff = step >> 3;
                if (delta > step)
                {
                    value |= 4;
                    delta -= step;
                    diff += step;
                }
                step >>= 1;
                if (delta > step)
                {
                    value |= 2;
                    delta -= step;
                    diff += step;
                }
                step >>= 1;
                if (delta > step)
                {
                    value |= 1;
                    diff += step;
                }

                predictor += (((value & 8) != 0) ? -diff : diff);
                predictor = clip(predictor, short.MinValue, short.MaxValue);

                stepIndex += ima_index_table[value & 7];
                stepIndex = clip(stepIndex, 0, 88);

                return (byte)value;
            }

            while ((writingOffset < dataPCM.Length) && (sampleCount > 0))
            {
                if (bytesInBlock == 0)
                {
                    writeInt16((short)stepIndex);
                    writeInt16((short)predictor);
                    bytesInBlock = 2044;
                }
                else
                {
                    var s0 = readSample();
                    var s1 = readSample();
                    var b0 = encodeNibble(s0);
                    var b1 = encodeNibble(s1);
                    var b = (b0 & 0x0F) + ((b1 & 0x0F) << 4);
                    dataPCM[writingOffset++] = (byte)b;
                    bytesInBlock--;
                    sampleCount -= 2;
                }
            }

            return dataPCM;
        }

    }

}
