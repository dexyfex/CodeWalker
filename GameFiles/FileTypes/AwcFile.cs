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

        public void Decrypt_RSXXTEA(ref byte[] data, uint[] key)
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
                    Decrypt_RSXXTEA(ref data, GTA5Keys.PC_AWC_KEY);
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
        public ushort Unk2 { get; set; }
        public ushort Unk3 { get; set; }
        public ushort Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public byte Unk6 { get; set; }

        public AwcFormatChunk(DataReader r)
        {
            Samples = r.ReadUInt32();
            LoopPoint = r.ReadInt32();
            SamplesPerSecond = r.ReadUInt16();
            Headroom = r.ReadInt16();
            Unk2 = r.ReadUInt16();
            Unk3 = r.ReadUInt16();
            Unk4 = r.ReadUInt16();
            Unk5 = r.ReadByte();
            Unk6 = r.ReadByte();

            //Apparently sometimes this struct is longer? TODO: fix??
            //r.ReadUInt16();
            //r.ReadUInt16();
        }


        public override string ToString()
        {
            return Headroom.ToString() + ", " + Unk6.ToString() + ": " + Samples.ToString() + " samples, " + SamplesPerSecond.ToString() + " samples/sec";
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
        public short BitsPerSample = 4;//16;
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

                string fmt = "ADPCM";
                switch (Format.Unk6)
                {
                    case 4:
                        break;
                    default:
                        break;
                }

                var hz = Format?.SamplesPerSecond ?? 0;

                return fmt + ((hz > 0) ? (", " + hz.ToString() + " Hz") : "");
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



        public Stream GetWavStream()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter w = new BinaryWriter(stream);


            //see http://icculus.org/SDL_sound/downloads/external_documentation/wavecomp.htm
            //see https://github.com/naudio/NAudio/blob/master/NAudio/Wave/WaveFormats/AdpcmWaveFormat.cs
            //see https://msdn.microsoft.com/en-us/library/windows/desktop/ff538799(v=vs.85).aspx


            int samplesPerSec = SamplesPerSecond;

            Channels = 1;
            BitsPerSample = 16;
            int byteRate = samplesPerSec * Channels * BitsPerSample / 8;
            short blockAlign = (short)(Channels * BitsPerSample / 8);

            // RIFF chunk
            var fileSize = 4 + 24 + 8 + Data.Length;
            w.Write("RIFF".ToCharArray()); // 0x00 - "RIFF" magic
            w.Write(fileSize); // 0x04 - file size
            w.Write("WAVE".ToCharArray()); // 0x08 - "WAVE" magic

            // fmt sub-chunk
            w.Write("fmt ".ToCharArray()); // 0x0C - "fmt " magic
            w.Write(16); // 0x10 - header size (16 bytes)
            w.Write((short)1); // 0x14 - audio format (1=PCM)
            w.Write(Channels); // 0x16 - number of channels
            w.Write(samplesPerSec); // 0x18
            w.Write(byteRate); // 0x1C
            w.Write(blockAlign);// sampleSize); // 0x20
            w.Write(BitsPerSample); // 0x22

            // data sub-chunk
            w.Write("data".ToCharArray());
            w.Write(Data.Length);
            w.Write(Data);            

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
