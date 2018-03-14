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

        public bool MultiChannel { get; set; }
        public byte[] MultiChannelData { get; set; }

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
                } else
                    ErrorMessage = "Corrupted data!";
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


                //notes from libertyV:
                // first bit - means that there are unknown word for each stream after this header
                // second bit - I think that it means that not all the tags are in the start of the file, but all the tags of a stream are near the data tag
                // third bit - Multi channel audio

                if ((Flags >> 8) != 0xFF)
                {
                    ErrorMessage = "Flags 0 not supported!";
                    return;
                }
                if ((Flags & 0xF8) != 0)
                {
                    //ErrorMessage = "Flags 1 not supported!";
                    //return;
                }

                MultiChannel = ((Flags & 4) == 4);


                var flag0 = ((Flags & 1) == 1);
                var infoStart = 16 + (flag0 ? (StreamCount * 2) : 0);

                ms.Position = infoStart;

                List<AwcStreamInfo> infos = new List<AwcStreamInfo>();
                Dictionary<uint, AwcStreamInfo> infoDict = new Dictionary<uint, AwcStreamInfo>();
                List<uint> audioIds = new List<uint>();
                List<AwcAudio> audios = new List<AwcAudio>();

                for (int i = 0; i < StreamCount; i++)
                {
                    var info = new AwcStreamInfo(r);
                    infos.Add(info);
                    infoDict[info.Id] = info;
                }
                for (int i = 0; i < StreamCount; i++)
                {
                    var info = infos[i];
                    for (int j = 0; j < info.TagCount; j++)
                    {
                        var chunk = new AwcChunkInfo(r);
                        info.Chunks[chunk.Tag] = chunk;
                    }
                }

                StreamInfos = infos.ToArray();



                byte hformat = 0xFA;// 250  0x6061D4FA & 0xFF; //JenkHash.GenHash("format");
                byte hdata = 0x55;// 85  0x5EB5E655 & 0xFF; //JenkHash.GenHash("data");
                byte hycd = 0x5C;// 92  YCD resource chunk... lip sync anims?
                byte hunk = 0x36;// 54  unk chunk? small number of bytes (2+)



                if (MultiChannel)
                {
                    AwcStreamInfo stream0 = null;
                    if (!infoDict.TryGetValue(0, out stream0))
                    {
                        ErrorMessage = "Couldn't find MultiChannel stream0";
                        return;
                    }

                    AwcChunkInfo chunk72 = null;
                    if (!stream0.Chunks.TryGetValue(72, out chunk72))
                    {
                        ErrorMessage = "Couldn't find MultiChannel chunk72";
                        return;
                    }

                    ms.Position = chunk72.Offset;

                    AwcChannelChunkInfo chanInfo = new AwcChannelChunkInfo(r);
                    if (chanInfo.ChannelCount != StreamCount - 1)
                    {
                        ErrorMessage = "Channel Count did not match Stream Count";
                        return;
                    }

                    List<AwcChannelChunkItemInfo> chunkItems = new List<AwcChannelChunkItemInfo>();
                    for (int i = 0; i < chanInfo.ChannelCount; i++)
                    {
                        var itemInfo = new AwcChannelChunkItemInfo(r);
                        chunkItems.Add(itemInfo);
                        audioIds.Add(infos[i + 1].Id);
                    }

                    //AudioStreams.Add(new MultiChannelAudio(new ChunkStream(this.Stream, streamsChunks[0][Tag("data")]), channelsInfoHeader, streamsInfo, header.BigEndian));

                    AwcChunkInfo cdata = null;
                    if (!stream0.Chunks.TryGetValue(hdata, out cdata))
                    {
                        ErrorMessage = "Couldn't find Stream 0 data chunk";
                        return;
                    }

                    ms.Position = cdata.Offset;
                    var lastPos = cdata.Offset + cdata.Size;
                    //int chunkSize = 0x800;
                    uint bigChunkSize = chanInfo.ChunkSize;
                    var chanCount = chanInfo.ChannelCount;

                    MultiChannelData = r.ReadBytes(cdata.Size);
                    ms.Position = cdata.Offset;

                    //var d = data;//temporary

                    ////this doesn't seem to work :(
                    //while (ms.Position < lastPos)
                    //{
                    //    uint totalChunks = 0;
                    //    var startPos = ms.Position;
                    //    var curPos = startPos;
                    //    //byte[] chunkdata = r.ReadBytes(chunkSize);
                    //    //ms.Position = startPos;
                    //    AwcChannelChunkHeader[] chanHeaders = new AwcChannelChunkHeader[chanCount];
                    //    for (int i = 0; i < chanCount; i++)
                    //    {
                    //        var chanHeader = new AwcChannelChunkHeader(r);
                    //        chanHeaders[i] = chanHeader;
                    //        totalChunks += chanHeader.ChunkCount;
                    //    }
                    //    int headerSize = (int)(totalChunks * 4 + chanInfo.ChannelCount * AwcChannelChunkHeader.Size);
                    //    headerSize += (((-headerSize) % chunkSize) + chunkSize) % chunkSize; //todo: simplify this!
                    //    curPos += headerSize;
                    //    AwcChannelChunk[] chanChunks = new AwcChannelChunk[chanCount];
                    //    for (int i = 0; i < chanCount; i++)
                    //    {
                    //        var chanChunk = new AwcChannelChunk(r, chanHeaders[i], chunkItems[i]);
                    //        chanChunks[i] = chanChunk;
                    //        curPos += chanChunk.TotalDataSize;
                    //    }
                    //    if (curPos - startPos > chanInfo.ChunkSize)
                    //    {
                    //        ErrorMessage = "Chunk was bigger than the chunk size";
                    //        break;
                    //    }
                    //    if ((totalChunks == 0) || ((startPos + chanInfo.ChunkSize) > lastPos))
                    //    {
                    //        ErrorMessage = "Unable to read chunk";
                    //        break;
                    //    }
                    //    var newPos = startPos + bigChunkSize;
                    //    if (newPos >= lastPos) break;
                    //    ms.Position = newPos;
                    //}



                }
                else
                {

                    for (int i = 0; i < StreamCount; i++)
                    {
                        var info = infos[i];

                        AwcChunkInfo cformat = null;
                        if (!info.Chunks.TryGetValue(hformat, out cformat))
                        {
                            ErrorMessage = "Couldn't find Stream " + i.ToString() + " format chunk";
                            continue;
                        }

                        AwcChunkInfo cdata = null;
                        if (!info.Chunks.TryGetValue(hdata, out cdata))
                        {
                            ErrorMessage = "Couldn't find Stream " + i.ToString() + " data chunk";
                            continue;
                        }

                        AwcChunkInfo cycd = null;
                        AwcAudioAnimClipDict oycd = null;
                        if (info.Chunks.TryGetValue(hycd, out cycd))
                        {
                            ms.Position = cycd.Offset;
                            oycd = new AwcAudioAnimClipDict(r, cycd);
                        }

                        AwcChunkInfo cunk = null;
                        AwcAudioUnk ounk = null;
                        if (info.Chunks.TryGetValue(hunk, out cunk))
                        {
                            ms.Position = cunk.Offset;
                            ounk = new AwcAudioUnk(r, cunk);
                        }


                        ms.Position = cformat.Offset;
                        AwcFormatChunk formatChunk = new AwcFormatChunk(r);

                        ms.Position = cdata.Offset;
                        AwcAudio audio = new AwcAudio(r, info, formatChunk, cdata);

                        audio.ClipDict = oycd;
                        audio.UnkData = ounk;

                        audios.Add(audio);
                        audioIds.Add(info.Id);
                    }
                }


                Audios = audios.ToArray();
                AudioIds = audioIds.ToArray();


            }

        }

    }


    [TC(typeof(EXP))] public class AwcStreamInfo
    {
        public uint RawVal { get; set; }
        public uint TagCount { get; set; }
        public uint Id { get; set; }

        public Dictionary<byte, AwcChunkInfo> Chunks { get; set; } = new Dictionary<byte, AwcChunkInfo>();

        public AwcStreamInfo(DataReader r)
        {
            RawVal = r.ReadUInt32();
            TagCount = (RawVal >> 29);
            Id = (RawVal & 0x1FFFFFFF);
        }

        public override string ToString()
        {
            return Id.ToString("X") + ": " + TagCount.ToString() + " tags";
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

    [TC(typeof(EXP))] public class AwcChannelChunkInfo
    {
        public uint Unk0 { get; set; }
        public uint ChunkSize { get; set; }
        public uint ChannelCount { get; set; }

        public AwcChannelChunkInfo(DataReader r)
        {
            Unk0 = r.ReadUInt32();
            ChunkSize = r.ReadUInt32();
            ChannelCount = r.ReadUInt32();
        }

        public override string ToString()
        {
            return Unk0.ToString() + ": " + ChunkSize.ToString() + ", " + ChannelCount.ToString() + " channels";
        }
    }

    [TC(typeof(EXP))] public class AwcChannelChunkItemInfo
    {
        public uint Id { get; set; }
        public uint Samples { get; set; }
        public ushort Unk0 { get; set; }
        public ushort SamplesPerSecond { get; set; }
        public byte Unk1 { get; set; }
        public byte RoundSize { get; set; }
        public ushort Unk2 { get; set; }

        public AwcChannelChunkItemInfo(DataReader r)
        {
            Id = r.ReadUInt32();
            Samples = r.ReadUInt32();
            Unk0 = r.ReadUInt16();
            SamplesPerSecond = r.ReadUInt16();
            Unk1 = r.ReadByte();
            RoundSize = r.ReadByte();
            Unk2 = r.ReadUInt16();
        }

        public override string ToString()
        {
            return Id.ToString() + ": " + Samples.ToString() + " samples, " + SamplesPerSecond.ToString() + " samples/sec, size: " + RoundSize.ToString();
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

        public enum CodecFormat {
            PCM = 0,
            ADPCM = 4
        }
        public CodecFormat Codec { get; set; }

        public AwcFormatChunk(DataReader r)
        {
            Samples = r.ReadUInt32();
            LoopPoint = r.ReadInt32();
            SamplesPerSecond = r.ReadUInt16();
            Headroom = r.ReadInt16();
            LoopBegin = r.ReadUInt16();
            LoopEnd = r.ReadUInt16();
            PlayEnd = r.ReadUInt16();
            PlayBegin = r.ReadByte();
            Codec = (CodecFormat)r.ReadByte();

            //Apparently sometimes this struct is longer? TODO: fix??
            //r.ReadUInt16();
            //r.ReadUInt16();
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
        public AwcAudioUnk UnkData { get; set; }


        public short Channels = 1;
        public short BitsPerSample = 16;
        public int SamplesPerSecond
        {
            get
            {
                return Format?.SamplesPerSecond ?? 0;
            }
        }
        public int SampleCount
        {
            get
            {
                return (int)(Format?.Samples ?? 0);
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
                if (Format == null) return "Unknown";

                string codec;
                switch (Format.Codec)
                {
                    case AwcFormatChunk.CodecFormat.PCM:
                        codec = "PCM";
                        break;
                    case AwcFormatChunk.CodecFormat.ADPCM:
                        codec = "ADPCM";
                        break;
                    default:
                        codec = "Unknown";
                        break;
                }

                var hz = Format.SamplesPerSecond;

                return codec + ((hz > 0) ? (", " + hz.ToString() + " Hz") : "");
            }
        }

        public float Length
        {
            get
            {
                return Format == null ? 0 : (float)Format.Samples / Format.SamplesPerSecond;                
            }
        }

        public string LengthStr
        {
            get
            {
                return TimeSpan.FromSeconds(Length).ToString("m\\:ss");
            }
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
            var hash = (StreamInfo?.Id.ToString("X") ?? "0").PadLeft(8, '0');
            return "0x" + hash + ": " + Format?.ToString() ?? "AwcAudio";
        }

        public byte[] DecodeADPCM(byte[] data, int sampleCount)
        {
            byte[] dataPCM = new byte[data.Length * 4];
            int predictor = 0, step_index = 0, step = 0;
            int readingOffset = 0, writingOffset = 0, bytesInBlock = 0;

            int[] ima_index_table = {
                -1, -1, -1, -1, 2, 4, 6, 8,
                -1, -1, -1, -1, 2, 4, 6, 8
            };

            short[] ima_step_table = {
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

            int clip(int value, int min, int max)
            {
                if (value < min)
                    return min;
                if (value > max)
                    return max;
                return value;
            }

            void parseNibble(byte nibble)
            {
                step_index = clip(step_index + ima_index_table[nibble], 0, 88);

                int diff = step >> 3;
                if ((nibble & 4) > 0) diff += step;
                if ((nibble & 2) > 0) diff += step >> 1;
                if ((nibble & 1) > 0) diff += step >> 2;
                if ((nibble & 8) > 0) predictor -= diff;
                else predictor += diff;

                step = ima_step_table[step_index];

                int samplePCM = clip(predictor, -32768, 32767);
                dataPCM[writingOffset] = (byte)(samplePCM & 0xFF);
                dataPCM[writingOffset + 1] = (byte)((samplePCM >> 8) & 0xFF);
                writingOffset += 2;
            }

            while ((readingOffset < data.Length) && (sampleCount > 0))
            {
                if (bytesInBlock == 0)
                {
                    step_index = clip(data[readingOffset], 0, 88);
                    predictor = BitConverter.ToInt16(data, readingOffset + 2);
                    step = ima_step_table[step_index];
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

        public Stream GetWavStream()
        {
            byte[] dataPCM = null;
            int bitsPerSample = BitsPerSample;

            switch (Format.Codec)
            {
                case AwcFormatChunk.CodecFormat.PCM:
                    dataPCM = Data;
                    break;
                case AwcFormatChunk.CodecFormat.ADPCM:
                    dataPCM = new byte[Data.Length];
                    Buffer.BlockCopy(Data, 0, dataPCM, 0, Data.Length);
                    AwcFile.Decrypt_RSXXTEA(dataPCM, GTA5Keys.PC_AWC_KEY);
                    dataPCM = DecodeADPCM(dataPCM, SampleCount);
                    bitsPerSample = 16;
                    break;
            }

            int byteRate = SamplesPerSecond * Channels * bitsPerSample / 8;
            short blockAlign = (short)(Channels * bitsPerSample / 8);

            MemoryStream stream = new MemoryStream();
            BinaryWriter w = new BinaryWriter(stream);
            int wavLength = 12 + 24 + 8 + Data.Length;

            // RIFF chunk
            w.Write("RIFF".ToCharArray());
            w.Write((int)(wavLength - 8));
            w.Write("WAVE".ToCharArray());

            // fmt sub-chunk     
            w.Write("fmt ".ToCharArray());
            w.Write((int)16); // fmt size
            w.Write((short)1); // 1 = WAVE_FORMAT_PCM
            w.Write((short)Channels);
            w.Write((int)SamplesPerSecond);
            w.Write((int)byteRate);
            w.Write((short)blockAlign);
            w.Write((short)bitsPerSample);

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
        public byte[] Data { get; set; }

        public AwcAudioUnk(DataReader r, AwcChunkInfo info)
        {
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


    [TC(typeof(EXP))] public class AwcChannelChunkHeader
    {
        public static uint Size = 16; //24 for ps3...

        public uint StartChunk { get; set; }
        public uint ChunkCount { get; set; }
        public uint SamplesToSkip { get; set; } //mostly 0
        public uint SamplesPerChunk { get; set; }
        public uint DataSize { get; set; }

        public AwcChannelChunkHeader(DataReader r)
        {
            StartChunk = r.ReadUInt32();
            ChunkCount = r.ReadUInt32();
            SamplesToSkip = r.ReadUInt32();
            SamplesPerChunk = r.ReadUInt32();
            DataSize = ChunkCount * 0x800;

            //for ps3, two extra ints:
            //uint unk0 = r.ReadUint32();
            //DataSize = r.ReadUint32();


        }

    }

    [TC(typeof(EXP))] public class AwcChannelChunk
    {
        public AwcChannelChunkHeader Header { get; set; }
        public AwcChannelChunkItemInfo Info { get; set; }
        public byte[] Data { get; set; }

        public uint TotalDataSize { get; set; }

        public AwcChannelChunk(DataReader r, AwcChannelChunkHeader h, AwcChannelChunkItemInfo i)
        {
            Header = h;
            Info = i;

            TotalDataSize = h.DataSize;

            var rs = i?.RoundSize ?? 0;
            int ds = (int)h.DataSize;
            if (rs != 0)
            {
                TotalDataSize = (uint)(TotalDataSize + (((-ds) % rs) + rs) % rs);
            }
        }

    }

}
