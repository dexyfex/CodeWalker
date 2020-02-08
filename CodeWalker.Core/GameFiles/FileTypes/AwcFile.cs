using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Xml;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))]public class AwcFile : PackedFile
    {
        public string Name { get; set; }
        public RpfFileEntry FileEntry { get; set; }

        public string ErrorMessage { get; set; }

        public uint Magic { get; set; } = 0x54414441;
        public ushort Version { get; set; }
        public ushort Flags { get; set; } = 0xFF00;
        public int StreamCount { get; set; }
        public int InfoOffset { get; set; }

        public bool UnkUshortsFlag { get { return ((Flags & 1) == 1); } set { Flags = (ushort)((Flags & 0xFFFE) + (value ? 1 : 0)); } }
        public bool SingleChannelEncryptFlag { get { return ((Flags & 2) == 2); } set { Flags = (ushort)((Flags & 0xFFFD) + (value ? 2 : 0)); } }
        public bool MultiChannelFlag { get { return ((Flags & 4) == 4); } set { Flags = (ushort)((Flags & 0xFFFB) + (value ? 4 : 0)); } }
        public bool MultiChannelEncryptFlag { get { return ((Flags & 8) == 8); } set { Flags = (ushort)((Flags & 0xFFF7) + (value ? 8 : 0)); } }

        public ushort[] UnkUshorts { get; set; } //offsets of some sort?


        public bool WholeFileEncrypted { get; set; }

        public AwcStreamInfo[] StreamInfos { get; set; }
        public AwcStream[] Streams { get; set; }

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
            Name = entry.Name;
            FileEntry = entry;

            if (!string.IsNullOrEmpty(Name))
            {
                var nl = Name.ToLowerInvariant();
                var fn = Path.GetFileNameWithoutExtension(nl);
                JenkIndex.Ensure(fn + "_left");
                JenkIndex.Ensure(fn + "_right");
            }

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

                Read(r);
            }
        }

        public byte[] Save()
        {
            MemoryStream s = new MemoryStream();
            DataWriter w = new DataWriter(s);

            Write(w);

            var buf = new byte[s.Length];
            s.Position = 0;
            s.Read(buf, 0, buf.Length);
            return buf;
        }






        private void Read(DataReader r)
        {
            Magic = r.ReadUInt32();
            Version = r.ReadUInt16();
            Flags = r.ReadUInt16();
            StreamCount = r.ReadInt32();
            InfoOffset = r.ReadInt32();


            //if ((Flags >> 8) != 0xFF)
            //{ }//no hit

            if (UnkUshortsFlag)
            {
                UnkUshorts = new ushort[StreamCount]; //offsets of some sort?
                for (int i = 0; i < StreamCount; i++)
                {
                    UnkUshorts[i] = r.ReadUInt16();
                }
            }


            //var infoStart = 16 + (UnkUshortsFlag ? (StreamCount * 2) : 0);
            //if (r.Position != infoStart)
            //{ }//no hit


            var infos = new List<AwcStreamInfo>();

            for (int i = 0; i < StreamCount; i++)
            {
                var info = new AwcStreamInfo();
                info.Read(r);
                infos.Add(info);
            }
            for (int i = 0; i < StreamCount; i++)
            {
                var info = infos[i];
                for (int j = 0; j < info.ChunkCount; j++)
                {
                    var chunk = new AwcChunkInfo();
                    chunk.Read(r);
                    info.Chunks[j] = chunk;
                }
            }
            StreamInfos = infos.ToArray();


            //var infoOffset = infoStart + StreamCount * 4;
            //foreach (var info in infos) infoOffset += (int)info.ChunkCount * 8;
            //if (infoOffset != InfoOffset)
            //{ }//no hit
            //if (r.Position != InfoOffset)
            //{ }//no hit



            var streams = new List<AwcStream>();
            AwcStream multisource = null;

            for (int i = 0; i < StreamCount; i++)
            {
                var info = StreamInfos[i];
                var stream = new AwcStream(info, this);
                stream.Read(r);
                streams.Add(stream);

                stream.UnkUshort = (UnkUshorts != null) ? (ushort?)UnkUshorts[i] : null;

                if (MultiChannelFlag && (stream.DataChunk != null))
                {
                    multisource = stream;
                }
            }
            if (multisource != null)
            {
                multisource.AssignMultiChannelSources(streams);
            }

            Streams = streams.ToArray();

        }

        private void Write(DataWriter w)
        {
            var infoStart = 16 + (UnkUshortsFlag ? (StreamCount * 2) : 0);
            var infoOffset = infoStart + StreamCount * 4;
            foreach (var info in StreamInfos) infoOffset += (int)info.ChunkCount * 8;
            InfoOffset = infoOffset;
            StreamCount = StreamInfos?.Length ?? 0;

            w.Write(Magic);
            w.Write(Version);
            w.Write(Flags);
            w.Write(StreamCount);
            w.Write(InfoOffset);

            if (UnkUshortsFlag)
            {
                for (int i = 0; i < StreamCount; i++)
                {
                    w.Write((i < (UnkUshorts?.Length ?? 0)) ? UnkUshorts[i] : 0);
                }
            }

            for (int i = 0; i < StreamCount; i++)
            {
                var info = StreamInfos[i];
                info.Write(w);
            }
            for (int i = 0; i < StreamCount; i++)
            {
                var info = StreamInfos[i];
                for (int j = 0; j < info.ChunkCount; j++)
                {
                    var chunk = info.Chunks[j];
                    chunk.Write(w);
                }
            }

            for (int i = 0; i < StreamCount; i++)
            {
                var stream = Streams[i];
                stream.Write(w);
            }



        }



        public void WriteXml(StringBuilder sb, int indent, string wavfolder)
        {
            AwcXml.ValueTag(sb, indent, "Version", Version.ToString());
            if (MultiChannelFlag)
            {
                AwcXml.ValueTag(sb, indent, "MultiChannel", true.ToString());
            }
            if ((Streams?.Length ?? 0) > 0)
            {
                AwcXml.OpenTag(sb, indent, "Streams");
                var strlist = Streams.ToList();
                strlist.Sort((a, b) => a.Name.CompareTo(b.Name));
                foreach (var stream in strlist)
                {
                    AwcXml.OpenTag(sb, indent + 1, "Item");
                    stream.WriteXml(sb, indent + 2, wavfolder);
                    AwcXml.CloseTag(sb, indent + 1, "Item");
                }
                AwcXml.CloseTag(sb, indent, "Streams");
            }

        }
        public void ReadXml(XmlNode node, string wavfolder)
        {
            Version = (ushort)Xml.GetChildUIntAttribute(node, "Version");
            MultiChannelFlag = Xml.GetChildBoolAttribute(node, "MultiChannel");

            var unkUshorts = new List<ushort>();
            var hasUshorts = false;

            var snode = node.SelectSingleNode("Streams");
            if (snode != null)
            {
                var slist = new List<AwcStream>();
                var inodes = node.SelectNodes("Item");
                foreach (XmlNode inode in inodes)
                {
                    var stream = new AwcStream(this);
                    stream.ReadXml(inode, wavfolder);
                    slist.Add(stream);

                    hasUshorts = hasUshorts || stream.UnkUshort.HasValue;
                    unkUshorts.Add(stream.UnkUshort ?? 0);
                }
                slist.Sort((a, b) => a.Hash.Hash.CompareTo(b.Hash.Hash));
                Streams = slist.ToArray();
            }

            if (hasUshorts)
            {
                UnkUshorts = unkUshorts.ToArray();
                UnkUshortsFlag = true;
            }


        }
        public static void WriteXmlNode(AwcFile f, StringBuilder sb, int indent, string wavfolder, string name = "AudioWaveContainer")
        {
            if (f == null) return;
            AwcXml.OpenTag(sb, indent, name);
            f.WriteXml(sb, indent + 1, wavfolder);
            AwcXml.CloseTag(sb, indent, name);
        }
        public static AwcFile ReadXmlNode(XmlNode node, string wavfolder)
        {
            if (node == null) return null;
            var f = new AwcFile();
            f.ReadXml(node, wavfolder);
            return f;
        }



    }

    public enum AwcCodecType
    {
        PCM = 0,
        ADPCM = 4
    }

    [TC(typeof(EXP))] public class AwcStreamInfo
    {
        public uint RawVal { get; set; }
        public uint ChunkCount { get; set; }
        public uint Id { get; set; }
        public AwcChunkInfo[] Chunks { get; set; }

        public void Read(DataReader r)
        {
            RawVal = r.ReadUInt32();
            ChunkCount = (RawVal >> 29);
            Id = (RawVal & 0x1FFFFFFF);
            Chunks = new AwcChunkInfo[ChunkCount];
        }
        public void Write(DataWriter w)
        {
            ChunkCount = (uint)(Chunks?.Length ?? 0);
            RawVal = (Id & 0x1FFFFFFF) + (ChunkCount << 29);
            w.Write(RawVal);
        }

        public override string ToString()
        {
            return Id.ToString("X") + ": " + ChunkCount.ToString() + " chunks";
        }
    }

    [TC(typeof(EXP))] public class AwcChunkInfo
    {
        public ulong RawVal { get; set; }
        public AwcChunkType Type { get; set; }
        public int Size { get; set; }
        public int Offset { get; set; }

        public void Read(DataReader r)
        {
            RawVal = r.ReadUInt64();
            Type = (AwcChunkType)(RawVal >> 56);
            Size = (int)((RawVal >> 28) & 0x0FFFFFFF);
            Offset = (int)(RawVal & 0x0FFFFFFF);
        }
        public void Write(DataWriter w)
        {
            RawVal = (((ulong)Offset) & 0x0FFFFFFF) + ((((ulong)Size) & 0x0FFFFFFF) << 28) + (((ulong)Type) << 56);
            w.Write(RawVal);
        }

        public override string ToString()
        {
            return Type.ToString() + ": " + Size.ToString() + ", " + Offset.ToString();
        }
    }

    [TC(typeof(EXP))] public class AwcStream
    {
        public AwcFile Awc { get; set; }
        public AwcStreamInfo StreamInfo { get; set; }
        public AwcChunk[] Chunks { get; set; }
        public AwcFormatChunk FormatChunk { get; set; }
        public AwcDataChunk DataChunk { get; set; }
        public AwcAnimationChunk AnimationChunk { get; set; }
        public AwcGestureChunk GestureChunk { get; set; }
        public AwcPeakChunk PeakChunk { get; set; }
        public AwcMIDIChunk MidiChunk { get; set; }
        public AwcMarkersChunk MarkersChunk { get; set; }
        public AwcGranularGrainsChunk GranularGrainsChunk { get; set; }
        public AwcGranularLoopsChunk GranularLoopsChunk { get; set; }
        public AwcStreamFormatChunk StreamFormatChunk { get; set; }
        public AwcSeekTableChunk SeekTableChunk { get; set; }
        public AwcStream StreamSource { get; set; }
        public AwcStreamFormat StreamFormat { get; set; }
        public AwcStreamDataBlock[] StreamBlocks { get; set; }
        public int StreamChannelIndex { get; set; }


        public short Channels = 1;
        public short BitsPerSample = 16;
        public int SamplesPerSecond
        {
            get
            {
                return FormatChunk?.SamplesPerSecond ?? StreamFormat?.SamplesPerSecond ?? 0;
            }
        }
        public int SampleCount
        {
            get
            {
                return (int)(FormatChunk?.Samples ?? StreamFormat?.Samples ?? 0);
            }
        }

        public ushort? UnkUshort { get; set; } // stored in root of AWC, will have value if present

        public MetaHash Hash
        {
            get
            {
                return StreamInfo?.Id ?? 0;
            }
            set
            {
                if (StreamInfo != null)
                {
                    StreamInfo.Id = value & 0x1FFFFFFF;
                }
            }
        }
        public MetaHash HashAdjusted
        {
            get
            {
                var h = (uint)Hash;
                if (h == 0) return h;
                for (uint i = 0; i < 8; i++)
                {
                    var th = h + (i << 29);
                    if (!string.IsNullOrEmpty(JenkIndex.TryGetString(th))) return th;
                    if (MetaNames.TryGetString(th, out string str)) return th;
                }
                return h;
            }
        }
        public string Name
        {
            get
            {
                if (CachedName != null) return CachedName;
                var ha = HashAdjusted;
                var str = JenkIndex.TryGetString(ha);
                if (!string.IsNullOrEmpty(str)) CachedName = str;
                else if (MetaNames.TryGetString(ha, out str)) CachedName = str;
                else CachedName = "0x" + Hash.Hex;
                return CachedName;
            }
        }
        private string CachedName;
        public string Type
        {
            get
            {
                if (MidiChunk != null)
                {
                    return "MIDI";
                }

                var fc = AwcCodecType.PCM;
                var hz = 0;
                if (FormatChunk != null)
                {
                    fc = FormatChunk.Codec;
                    hz = FormatChunk.SamplesPerSecond;
                }
                if (StreamFormat != null)
                {
                    fc = StreamFormat.Codec;
                    hz = StreamFormat.SamplesPerSecond;
                }
                string codec = fc.ToString();
                switch (fc)
                {
                    case AwcCodecType.PCM:
                    case AwcCodecType.ADPCM:
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
                if (FormatChunk != null) return (float)FormatChunk.Samples / FormatChunk.SamplesPerSecond;
                if (StreamFormat != null) return (float)StreamFormat.Samples / StreamFormat.SamplesPerSecond;
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
                if (MidiChunk?.Data != null)
                {
                    return MidiChunk.Data.Length;
                }
                if (DataChunk?.Data != null)
                {
                    return DataChunk.Data.Length;
                }
                if (StreamSource != null)
                {
                    int c = 0;
                    if (StreamSource?.StreamBlocks != null)
                    {
                        foreach (var blk in StreamSource.StreamBlocks)
                        {
                            if (StreamChannelIndex < (blk?.Channels?.Length ?? 0))
                            {
                                var chan = blk.Channels[StreamChannelIndex];
                                c += chan?.Data?.Length ?? 0;
                            }
                        }
                    }
                    return c;
                }
                return 0;
            }
        }


        public AwcStream(AwcFile awc)
        {
            Awc = awc;
            StreamInfo = new AwcStreamInfo();
        }
        public AwcStream(AwcStreamInfo s, AwcFile awc)
        {
            Awc = awc;
            StreamInfo = s;
        }

        public void AssignMultiChannelSources(List<AwcStream> audios)
        {
            for (int i = 0; i < audios.Count; i++)
            {
                var audio = audios[i];
                if (audio != this)
                {
                    var id = audio.StreamInfo?.Id ?? 0;
                    var srcind = 0;
                    var chancnt = StreamFormatChunk?.Channels?.Length ?? 0;
                    var found = false;
                    for (int ind = 0; ind < chancnt; ind++)
                    {
                        var mchan = StreamFormatChunk.Channels[ind];
                        if (mchan.Id == id)
                        {
                            srcind = ind;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    { }//no hit

                    audio.StreamSource = this;
                    audio.StreamFormat = (srcind < chancnt) ? StreamFormatChunk.Channels[srcind] : null;
                    audio.StreamChannelIndex = srcind;
                }

            }
        }


        public void Read(DataReader r)
        {

            var chunklist = new List<AwcChunk>();
            for (int j = 0; j < StreamInfo.Chunks?.Length; j++)
            {
                var cinfo = StreamInfo.Chunks[j];
                r.Position = cinfo.Offset;

                var chunk = CreateChunk(cinfo);
                chunk?.Read(r);
                chunklist.Add(chunk);

                if (chunk is AwcDataChunk dataChunk) DataChunk = dataChunk;
                if (chunk is AwcFormatChunk formatChunk) FormatChunk = formatChunk;
                if (chunk is AwcAnimationChunk animChunk) AnimationChunk = animChunk;
                if (chunk is AwcMarkersChunk markersChunk) MarkersChunk = markersChunk;
                if (chunk is AwcGestureChunk gestureChunk) GestureChunk = gestureChunk;
                if (chunk is AwcPeakChunk peakChunk) PeakChunk = peakChunk;
                if (chunk is AwcMIDIChunk midiChunk) MidiChunk = midiChunk;
                if (chunk is AwcStreamFormatChunk streamformatChunk) StreamFormatChunk = streamformatChunk;
                if (chunk is AwcSeekTableChunk seektableChunk) SeekTableChunk = seektableChunk;
                if (chunk is AwcGranularGrainsChunk ggChunk) GranularGrainsChunk = ggChunk;
                if (chunk is AwcGranularLoopsChunk glChunk) GranularLoopsChunk = glChunk;

                if ((r.Position - cinfo.Offset) != cinfo.Size)
                { }//make sure everything was read!
            }
            Chunks = chunklist.ToArray();


            //create multichannel blocks and decrypt data where necessary
            if (DataChunk?.Data != null)
            {
                if (Awc.MultiChannelFlag)
                {
                    var ocount = (int)(SeekTableChunk?.SeekTable?.Length ?? 0);
                    var ccount = (int)(StreamFormatChunk?.ChannelCount ?? 0);
                    var bcount = (int)(StreamFormatChunk?.BlockCount ?? 0);
                    var bsize = (int)(StreamFormatChunk?.BlockSize ?? 0);
                    var blist = new List<AwcStreamDataBlock>();
                    for (int b = 0; b < bcount; b++)
                    {
                        int srcoff = b * bsize;
                        int mcsoff = (b < ocount) ? (int)SeekTableChunk.SeekTable[b] : 0;
                        int blen = Math.Max(Math.Min(bsize, DataChunk.Data.Length - srcoff), 0);
                        var bdat = new byte[blen];
                        Buffer.BlockCopy(DataChunk.Data, srcoff, bdat, 0, blen);
                        if (Awc.MultiChannelEncryptFlag && !Awc.WholeFileEncrypted)
                        {
                            AwcFile.Decrypt_RSXXTEA(bdat, GTA5Keys.PC_AWC_KEY);
                        }
                        var blk = new AwcStreamDataBlock(bdat, StreamFormatChunk, r.Endianess, b, mcsoff);
                        blist.Add(blk);
                    }
                    StreamBlocks = blist.ToArray();
                }
                else
                {
                    if (Awc.SingleChannelEncryptFlag && !Awc.WholeFileEncrypted)
                    {
                        AwcFile.Decrypt_RSXXTEA(DataChunk.Data, GTA5Keys.PC_AWC_KEY);
                    }
                }
            }
            //if ((Data != null) && awc.WholeFileEncrypted && awc.MultiChannelFlag)
            //{ }//no hit


        }

        public void Write(DataWriter w)
        {

        }

        public void WriteXml(StringBuilder sb, int indent, string wavfolder)
        {
            AwcXml.StringTag(sb, indent, "Name", AwcXml.HashString(HashAdjusted));
            if (StreamFormatChunk == null)
            {
                //skip the wave file output for multichannel sources
                AwcXml.StringTag(sb, indent, "FileName", AwcXml.XmlEscape(Name + ".wav"));
                try
                {
                    if (!string.IsNullOrEmpty(wavfolder))
                    {
                        if (!Directory.Exists(wavfolder))
                        {
                            Directory.CreateDirectory(wavfolder);
                        }
                        var filepath = Path.Combine(wavfolder, (Name ?? "null") + ".wav");
                        var wav = GetWavFile();
                        File.WriteAllBytes(filepath, wav);
                    }
                }
                catch { }
            }
            if (UnkUshort.HasValue)
            {
                AwcXml.ValueTag(sb, indent, "UnkUshort", UnkUshort.Value.ToString());
            }
            if (StreamFormat != null)
            {
                AwcXml.OpenTag(sb, indent, "StreamFormat");
                StreamFormat.WriteXml(sb, indent + 1);
                AwcXml.CloseTag(sb, indent, "StreamFormat");
            }
            if ((Chunks?.Length ?? 0) > 0)
            {
                AwcXml.OpenTag(sb, indent, "Chunks");
                for (int i = 0; i < Chunks.Length; i++)
                {
                    AwcXml.OpenTag(sb, indent + 1, "Item");
                    Chunks[i].WriteXml(sb, indent + 2);
                    AwcXml.CloseTag(sb, indent + 1, "Item");
                }
                AwcXml.CloseTag(sb, indent, "Chunks");
            }
        }

        public void ReadXml(XmlNode node, string wavfolder)
        {
            Hash = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
            var filename = Xml.GetChildInnerText(node, "FileName");
            if (!string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(wavfolder))
            {
                try
                {
                    var filepath = Path.Combine(wavfolder, filename);
                    if (File.Exists(filepath))
                    {
                        var wav = File.ReadAllBytes(filepath);

                        //TODO: deal with wave file!!

                    }
                }
                catch { }

            }
            var unode = node.SelectSingleNode("UnkUshort");
            if (unode != null)
            {
                UnkUshort = (ushort)Xml.GetUIntAttribute(unode, "value");
            }
            var fnode = node.SelectSingleNode("StreamFormat");
            if (fnode != null)
            {
                StreamFormat = new AwcStreamFormat();
                StreamFormat.ReadXml(fnode);
            }
            var cnode = node.SelectSingleNode("Chunks");
            if (cnode != null)
            {
                var clist = new List<AwcChunk>();
                var inodes = node.SelectNodes("Item");
                foreach (XmlNode inode in inodes)
                {
                    var type = Xml.GetChildEnumInnerText<AwcChunkType>(node, "Type");
                    var info = new AwcChunkInfo() { Type = type };
                    var chunk = CreateChunk(info);
                    chunk?.ReadXml(inode);
                    clist.Add(chunk);
                }
                Chunks = clist.ToArray();
            }

        }


        public static AwcChunk CreateChunk(AwcChunkInfo info)
        {
            switch (info.Type)
            {
                case AwcChunkType.data: return new AwcDataChunk(info);
                case AwcChunkType.format: return new AwcFormatChunk(info);
                case AwcChunkType.animation: return new AwcAnimationChunk(info);
                case AwcChunkType.peak: return new AwcPeakChunk(info);
                case AwcChunkType.mid: return new AwcMIDIChunk(info);
                case AwcChunkType.gesture: return new AwcGestureChunk(info);
                case AwcChunkType.granulargrains: return new AwcGranularGrainsChunk(info);
                case AwcChunkType.granularloops: return new AwcGranularLoopsChunk(info);
                case AwcChunkType.markers: return new AwcMarkersChunk(info);
                case AwcChunkType.streamformat: return new AwcStreamFormatChunk(info);
                case AwcChunkType.seektable: return new AwcSeekTableChunk(info);
            }
            return null;
        }



        public override string ToString()
        {
            var hash = "0x" + (StreamInfo?.Id.ToString("X") ?? "0").PadLeft(8, '0') + ": ";
            if (FormatChunk != null)
            {
                return hash + FormatChunk?.ToString() ?? "AwcAudio";
            }
            if (StreamFormat != null)
            {
                return hash + StreamFormat?.ToString() ?? "AwcAudio";
            }
            if (MidiChunk != null)
            {
                return hash + MidiChunk.ToString(); 
            }
            return hash + "Unknown";
        }


        public byte[] GetWavData()
        {
            if (StreamFormat != null)
            {
                if (DataChunk?.Data == null)
                {
                    var ms = new MemoryStream();
                    var bw = new BinaryWriter(ms);
                    if (StreamSource?.StreamBlocks != null)
                    {
                        foreach (var blk in StreamSource.StreamBlocks)
                        {
                            if (StreamChannelIndex < (blk?.Channels?.Length ?? 0))
                            {
                                var chan = blk.Channels[StreamChannelIndex];
                                var cdata = chan.Data;
                                bw.Write(cdata);
                            }
                        }
                    }
                    bw.Flush();
                    ms.Position = 0;
                    DataChunk = new AwcDataChunk(null);
                    DataChunk.Data = new byte[ms.Length];
                    ms.Read(DataChunk.Data, 0, (int)ms.Length);
                }
            }
            return DataChunk.Data;
        }

        public byte[] GetWavFile()
        {
            var ms = GetWavStream();
            var data = new byte[ms.Length];
            ms.Read(data, 0, (int)ms.Length);
            return data;
        }

        public Stream GetWavStream()
        {
            byte[] dataPCM = GetWavData();
            var bitsPerSample = BitsPerSample;
            var channels = Channels;
            var codec = StreamFormat?.Codec ?? FormatChunk?.Codec ?? AwcCodecType.PCM;

            if (codec == AwcCodecType.ADPCM)//just convert ADPCM to PCM for compatibility reasons
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

    public enum AwcChunkType : byte
    {
        //should be the last byte of the hash of the name.
        data = 0x55,            // 0x5EB5E655
        format = 0xFA,          // 0x6061D4FA
        animation = 0x5C,       // 0x938C925C   not correct
        peak = 0x36,            // 0x8B946236
        mid = 0x68,             // 0x71DE4C68
        gesture = 0x2B,         // 0x23097A2B
        granulargrains = 0x5A,  // 0xE787895A
        granularloops = 0xD9,   // 0x252C20D9
        markers = 0xBD,         // 0xD4CB98BD
        streamformat = 0x48,    // 0x81F95048
        seektable = 0xA3,       // 0x021E86A3
    }

    [TC(typeof(EXP))] public abstract class AwcChunk : IMetaXmlItem
    {
        public AwcChunkInfo ChunkInfo { get; set; }

        public AwcChunk(AwcChunkInfo info)
        {
            ChunkInfo = info;
        }

        public abstract void Read(DataReader r);
        public abstract void Write(DataWriter w);
        public abstract void WriteXml(StringBuilder sb, int indent);
        public abstract void ReadXml(XmlNode node);
    }

    [TC(typeof(EXP))] public class AwcDataChunk : AwcChunk
    {
        public byte[] Data { get; set; }

        public AwcDataChunk(AwcChunkInfo info) : base(info)
        { }

        public override void Read(DataReader r)
        {
            Data = r.ReadBytes(ChunkInfo.Size);
        }
        public override void Write(DataWriter w)
        {
            w.Write(Data);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            //this is just a placeholder. in XML, channel data is written as WAV files
        }
        public override void ReadXml(XmlNode node)
        {
        }

        public override string ToString()
        {
            return "data: " + (Data?.Length ?? 0).ToString() + " bytes";
        }
    }

    [TC(typeof(EXP))] public class AwcFormatChunk : AwcChunk
    {
        public uint Samples { get; set; }
        public int LoopPoint { get; set; }
        public ushort SamplesPerSecond { get; set; }
        public short Headroom { get; set; }
        public ushort LoopBegin { get; set; }
        public ushort LoopEnd { get; set; }
        public ushort PlayEnd { get; set; }
        public byte PlayBegin { get; set; }
        public AwcCodecType Codec { get; set; }
        public uint? UnkExtra { get; set; }


        public AwcFormatChunk(AwcChunkInfo info) : base(info)
        { }

        public override void Read(DataReader r)
        {
            Samples = r.ReadUInt32();
            LoopPoint = r.ReadInt32();
            SamplesPerSecond = r.ReadUInt16();
            Headroom = r.ReadInt16();
            LoopBegin = r.ReadUInt16();
            LoopEnd = r.ReadUInt16();
            PlayEnd = r.ReadUInt16();
            PlayBegin = r.ReadByte();
            Codec = (AwcCodecType)r.ReadByte();

            switch (ChunkInfo.Size) //Apparently sometimes this struct is longer?
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
        public override void Write(DataWriter w)
        {
            w.Write(Samples);
            w.Write(LoopPoint);
            w.Write(SamplesPerSecond);
            w.Write(Headroom);
            w.Write(LoopBegin);
            w.Write(LoopEnd);
            w.Write(PlayEnd);
            w.Write(PlayBegin);
            w.Write((byte)Codec);
            if (UnkExtra.HasValue)
            {
                w.Write(UnkExtra.Value);
            }
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            AwcXml.StringTag(sb, indent, "Codec", Codec.ToString());
            AwcXml.ValueTag(sb, indent, "Samples", Samples.ToString());
            AwcXml.ValueTag(sb, indent, "SampleRate", SamplesPerSecond.ToString());
            AwcXml.ValueTag(sb, indent, "Headroom", Headroom.ToString());
            AwcXml.ValueTag(sb, indent, "PlayBegin", PlayBegin.ToString());
            AwcXml.ValueTag(sb, indent, "PlayEnd", PlayEnd.ToString());
            AwcXml.ValueTag(sb, indent, "LoopBegin", LoopBegin.ToString());
            AwcXml.ValueTag(sb, indent, "LoopEnd", LoopEnd.ToString());
            if (UnkExtra.HasValue)
            {
                AwcXml.ValueTag(sb, indent, "UnkExtra", UnkExtra.Value.ToString());
            }
        }
        public override void ReadXml(XmlNode node)
        {
            Codec = Xml.GetChildEnumInnerText<AwcCodecType>(node, "Codec");
            Samples = Xml.GetChildUIntAttribute(node, "Samples");
            SamplesPerSecond = (ushort)Xml.GetChildUIntAttribute(node, "SampleRate");
            Headroom = (short)Xml.GetChildIntAttribute(node, "Headroom");
            PlayBegin = (byte)Xml.GetChildUIntAttribute(node, "PlayBegin");
            PlayEnd = (ushort)Xml.GetChildUIntAttribute(node, "PlayEnd");
            LoopBegin = (ushort)Xml.GetChildUIntAttribute(node, "LoopBegin");
            LoopEnd = (ushort)Xml.GetChildUIntAttribute(node, "LoopEnd");
            var unode = node.SelectSingleNode("UnkExtra");
            if (unode != null)
            {
                UnkExtra = Xml.GetUIntAttribute(unode, "value");
            }
        }

        public override string ToString()
        {
            return "format: " + Codec.ToString() + ": " + Samples.ToString() + " samples, " + SamplesPerSecond.ToString() + " samples/sec, headroom: " + Headroom.ToString();
        }
    }

    [TC(typeof(EXP))] public class AwcStreamFormatChunk : AwcChunk
    {
        public uint BlockCount { get; set; }
        public uint BlockSize { get; set; }
        public uint ChannelCount { get; set; }
        public AwcStreamFormat[] Channels { get; set; }

        public AwcStreamFormatChunk(AwcChunkInfo info) : base(info)
        { }

        public override void Read(DataReader r)
        {
            BlockCount = r.ReadUInt32();
            BlockSize = r.ReadUInt32();
            ChannelCount = r.ReadUInt32();

            var channels = new List<AwcStreamFormat>();
            for (int i = 0; i < ChannelCount; i++)
            {
                var itemInfo = new AwcStreamFormat();
                itemInfo.Read(r);
                channels.Add(itemInfo);
            }
            Channels = channels.ToArray();
        }
        public override void Write(DataWriter w)
        {
            w.Write(BlockCount);
            w.Write(BlockSize);
            w.Write(ChannelCount);
            for (int i = 0; i < ChannelCount; i++)
            {
                Channels[i].Write(w);
            }
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            //this is just a placeholder. in XML, channel format is written with each channel stream
        }
        public override void ReadXml(XmlNode node)
        {
        }

        public override string ToString()
        {
            return "streamformat: " + ChannelCount.ToString() + " channels, " + BlockCount.ToString() + " blocks, " + BlockSize.ToString() + " bytes per block";
        }
    }

    [TC(typeof(EXP))] public class AwcStreamFormat
    {
        public uint Id { get; set; }
        public uint Samples { get; set; }
        public short Headroom { get; set; }
        public ushort SamplesPerSecond { get; set; }
        public AwcCodecType Codec { get; set; } = AwcCodecType.ADPCM;
        public byte Unused1 { get; set; }
        public ushort Unused2 { get; set; }


        public void Read(DataReader r)
        {
            Id = r.ReadUInt32();
            Samples = r.ReadUInt32();
            Headroom = r.ReadInt16();
            SamplesPerSecond = r.ReadUInt16();
            Codec = (AwcCodecType)r.ReadByte();
            Unused1 = r.ReadByte();
            Unused2 = r.ReadUInt16();

            #region test
            //switch (Codec)
            //{
            //    case AwcCodecFormat.ADPCM:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unused1)
            //{
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unused2)
            //{
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}
            #endregion
        }
        public void Write(DataWriter w)
        {
            w.Write(Id);
            w.Write(Samples);
            w.Write(Headroom);
            w.Write(SamplesPerSecond);
            w.Write((byte)Codec);
            w.Write(Unused1);
            w.Write(Unused2);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Codec", Codec.ToString());
            AwcXml.ValueTag(sb, indent, "Samples", Samples.ToString());
            AwcXml.ValueTag(sb, indent, "SampleRate", SamplesPerSecond.ToString());
            AwcXml.ValueTag(sb, indent, "Headroom", Headroom.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Codec = Xml.GetChildEnumInnerText<AwcCodecType>(node, "Codec");
            Samples = Xml.GetChildUIntAttribute(node, "Samples");
            SamplesPerSecond = (ushort)Xml.GetChildUIntAttribute(node, "SampleRate");
            Headroom = (short)Xml.GetChildIntAttribute(node, "Headroom");
        }

        public override string ToString()
        {
            return Id.ToString() + ", " + Codec.ToString() + ": " + Samples.ToString() + " samples, " + SamplesPerSecond.ToString() + " samples/sec, headroom: " + Headroom.ToString();
        }
    }

    [TC(typeof(EXP))] public class AwcAnimationChunk : AwcChunk
    {
        public byte[] Data { get; set; }
        public ClipDictionary ClipDict { get; set; }

        public AwcAnimationChunk(AwcChunkInfo info) : base(info)
        { }

        public override void Read(DataReader r)
        {

            Data = r.ReadBytes(ChunkInfo.Size);

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
        public override void Write(DataWriter w)
        {
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            if (ClipDict != null)
            {
                AwcXml.OpenTag(sb, indent, "ClipDictionary");
                ClipDict.WriteXml(sb, indent + 1);
                AwcXml.CloseTag(sb, indent, "ClipDictionary");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            var dnode = node.SelectSingleNode("ClipDictionary");
            if (dnode != null)
            {
                ClipDict = new ClipDictionary();
                ClipDict.ReadXml(dnode);
            }
        }

        public override string ToString()
        {
            return "animation: " + (ClipDict?.ClipsMapEntries ?? 0).ToString() + " entries";
        }
    }

    [TC(typeof(EXP))] public class AwcPeakChunk : AwcChunk
    {
        public ushort[] Data { get; set; }

        public AwcPeakChunk(AwcChunkInfo info) : base(info)
        { }

        public override void Read(DataReader r)
        {

            //if ((ChunkInfo.Size % 2) != 0)
            //{ }//no hit
            var count = ChunkInfo.Size / 2;
            Data = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                Data[i] = r.ReadUInt16();
            }

        }
        public override void Write(DataWriter w)
        {
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            AwcXml.WriteRawArray(sb, Data, indent, "Data", "");
        }
        public override void ReadXml(XmlNode node)
        {
            Data = Xml.GetChildRawUshortArray(node, "Data");
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
            return "peak: " + sb.ToString();
        }
    }

    [TC(typeof(EXP))] public class AwcGestureChunk : AwcChunk
    {
        public Gesture[] Gestures { get; set; }

        public class Gesture : IMetaXmlItem
        {
            public MetaHash Name { get; set; }
            public uint UnkUint1 { get; set; }
            public float UnkFloat1 { get; set; }
            public float UnkFloat2 { get; set; }
            public float UnkFloat3 { get; set; }
            public float UnkFloat4 { get; set; }
            public float UnkFloat5 { get; set; }
            public float UnkFloat6 { get; set; }
            public uint UnkUint2 { get; set; }

            public void Read(DataReader r)
            {
                Name = r.ReadUInt32();
                UnkUint1 = r.ReadUInt32();
                UnkFloat1 = r.ReadSingle();
                UnkFloat2 = r.ReadSingle();
                UnkFloat3 = r.ReadSingle();
                UnkFloat4 = r.ReadSingle();
                UnkFloat5 = r.ReadSingle();
                UnkFloat6 = r.ReadSingle();
                UnkUint2 = r.ReadUInt32();

                //switch (Name)
                //{
                //    case 0xceda50be: // 
                //    case 0x452c06fc: // 
                //    case 0xba377ce2: // 
                //    case 0xbe4c6d06: // 
                //    case 0x9db051b4: // 
                //    case 0x8a726e9f: // 
                //    case 0x1f60ea95: // 
                //    case 0x14e63a65: // 
                //    case 0x32b4abf4: // 
                //    case 0xe2b1dd62: // 
                //    case 0x482d3572: // 
                //    case 0x32f8d7a7: // 
                //    case 0x9144296b: // 
                //    case 0x3c73a8a4: // 
                //    case 0x057c10c5: // 
                //    case 0x981a4da0: // 
                //    case 0x519d5f74: // 
                //    case 0x0de43bc8: // 
                //    case 0x89c16359: // 
                //    case 0xd8884a6b: // 
                //    case 0xfec7eb20: // 
                //    case 0x06f0f709: // 
                //    case 0x788a8abd: //
                //        break;
                //    default:
                //        break;//and more...
                //}

                //switch (UnkUint2)
                //{
                //    case 1:
                //    case 0:
                //        break;
                //    default:
                //        break;//no hit
                //}

            }
            public void Write(DataWriter w)
            {
                w.Write(Name);
                w.Write(UnkUint1);
                w.Write(UnkFloat1);
                w.Write(UnkFloat2);
                w.Write(UnkFloat3);
                w.Write(UnkFloat4);
                w.Write(UnkFloat5);
                w.Write(UnkFloat6);
                w.Write(UnkUint2);
            }
            public void WriteXml(StringBuilder sb, int indent)
            {
                AwcXml.StringTag(sb, indent, "Name", AwcXml.HashString(Name));
                AwcXml.ValueTag(sb, indent, "UnkUint1", UnkUint1.ToString());
                AwcXml.ValueTag(sb, indent, "UnkFloat1", FloatUtil.ToString(UnkFloat1));
                AwcXml.ValueTag(sb, indent, "UnkFloat2", FloatUtil.ToString(UnkFloat2));
                AwcXml.ValueTag(sb, indent, "UnkFloat3", FloatUtil.ToString(UnkFloat3));
                AwcXml.ValueTag(sb, indent, "UnkFloat4", FloatUtil.ToString(UnkFloat4));
                AwcXml.ValueTag(sb, indent, "UnkFloat5", FloatUtil.ToString(UnkFloat5));
                AwcXml.ValueTag(sb, indent, "UnkFloat6", FloatUtil.ToString(UnkFloat6));
                AwcXml.ValueTag(sb, indent, "UnkUint2", UnkUint2.ToString());
            }
            public void ReadXml(XmlNode node)
            {
                Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
                UnkUint1 = Xml.GetChildUIntAttribute(node, "UnkUint1");
                UnkFloat1 = Xml.GetChildFloatAttribute(node, "UnkFloat1");
                UnkFloat2 = Xml.GetChildFloatAttribute(node, "UnkFloat2");
                UnkFloat3 = Xml.GetChildFloatAttribute(node, "UnkFloat3");
                UnkFloat4 = Xml.GetChildFloatAttribute(node, "UnkFloat4");
                UnkFloat5 = Xml.GetChildFloatAttribute(node, "UnkFloat5");
                UnkFloat6 = Xml.GetChildFloatAttribute(node, "UnkFloat6");
                UnkUint2 = Xml.GetChildUIntAttribute(node, "UnkUint2");
            }

            public override string ToString()
            {
                return Name.ToString() + ": " + UnkUint1.ToString() + ", " + UnkFloat1.ToString() + ", " + UnkFloat2.ToString() + ", " + UnkFloat3.ToString() + ", " + UnkFloat4.ToString() + ", " + UnkFloat5.ToString() + ", " + UnkFloat6.ToString() + ", " + UnkUint2.ToString();
            }
        }

        public AwcGestureChunk(AwcChunkInfo info) : base(info)
        { }

        public override void Read(DataReader r)
        {

            // (hash, uint, 6x floats, uint) * n

            //if ((ChunkInfo.Size % 36) != 0)
            //{ }//no hit
            var count = ChunkInfo.Size / 36;
            Gestures = new Gesture[count];
            for (int i = 0; i < count; i++)
            {
                var g = new Gesture();
                g.Read(r);
                Gestures[i] = g;
            }

        }
        public override void Write(DataWriter w)
        {
            for (int i = 0; i < (Gestures?.Length ?? 0); i++)
            {
                Gestures[i].Write(w);
            }
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            AwcXml.WriteItemArray(sb, Gestures, indent, "Gestures");
        }
        public override void ReadXml(XmlNode node)
        {
            Gestures = XmlMeta.ReadItemArray<Gesture>(node, "Gestures");
        }

        public override string ToString()
        {
            return "gesture: " + (Gestures?.Length ?? 0).ToString() + " items";
        }
    }

    [TC(typeof(EXP))] public class AwcGranularGrainsChunk : AwcChunk
    {
        public GranularGrain[] GranularGrains { get; set; }
        public float UnkFloat1 { get; set; }

        public class GranularGrain : IMetaXmlItem
        {
            public uint UnkUint1 { get; set; }
            public float UnkFloat1 { get; set; }
            public ushort UnkUshort1 { get; set; }
            public ushort UnkUshort2 { get; set; }

            public void Read(DataReader r)
            {
                UnkUint1 = r.ReadUInt32();
                UnkFloat1 = r.ReadSingle();
                UnkUshort1 = r.ReadUInt16();
                UnkUshort2 = r.ReadUInt16();
            }
            public void Write(DataWriter w)
            {
                w.Write(UnkUint1);
                w.Write(UnkFloat1);
                w.Write(UnkUshort1);
                w.Write(UnkUshort2);
            }
            public void WriteXml(StringBuilder sb, int indent)
            {
                AwcXml.ValueTag(sb, indent, "UnkUint1", UnkUint1.ToString());
                AwcXml.ValueTag(sb, indent, "UnkFloat1", FloatUtil.ToString(UnkFloat1));
                AwcXml.ValueTag(sb, indent, "UnkUshort1", UnkUshort1.ToString());
                AwcXml.ValueTag(sb, indent, "UnkUshort2", UnkUshort2.ToString());
            }
            public void ReadXml(XmlNode node)
            {
                UnkUint1 = Xml.GetChildUIntAttribute(node, "UnkUint1");
                UnkFloat1 = Xml.GetChildFloatAttribute(node, "UnkFloat1");
                UnkUshort1 = (ushort)Xml.GetChildUIntAttribute(node, "UnkUshort1");
                UnkUshort2 = (ushort)Xml.GetChildUIntAttribute(node, "UnkUshort2");
            }

            public override string ToString()
            {
                return UnkUint1.ToString() + ", " + UnkFloat1.ToString() + ", " + UnkUshort1.ToString() + ", " + UnkUshort2.ToString();
            }
        }

        public AwcGranularGrainsChunk(AwcChunkInfo info) : base(info)
        {
        }

        public override void Read(DataReader r)
        {

            //int, (2x floats, int) * n ?

            //if ((ChunkInfo.Size % 12) != 4)
            //{ }//no hit
            var count = (ChunkInfo.Size - 4) / 12;
            GranularGrains = new GranularGrain[count];
            for (int i = 0; i < count; i++)
            {
                var g = new GranularGrain();
                g.Read(r);
                GranularGrains[i] = g;
            }
            UnkFloat1 = r.ReadSingle();

            //if (UnkFloat1 > 1.0f)
            //{ }//no hit
            //if (UnkFloat1 < 0.45833f)
            //{ }//no hit

        }
        public override void Write(DataWriter w)
        {
            for (int i = 0; i < (GranularGrains?.Length ?? 0); i++)
            {
                GranularGrains[i].Write(w);
            }
            w.Write(UnkFloat1);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            AwcXml.ValueTag(sb, indent, "UnkFloat1", FloatUtil.ToString(UnkFloat1));
            AwcXml.WriteItemArray(sb, GranularGrains, indent, "GranularGrains");
        }
        public override void ReadXml(XmlNode node)
        {
            UnkFloat1 = Xml.GetChildFloatAttribute(node, "UnkFloat1");
            GranularGrains = XmlMeta.ReadItemArray<GranularGrain>(node, "GranularGrains");
        }

        public override string ToString()
        {
            return "granulargrains: " + (GranularGrains?.Length ?? 0).ToString() + " items";
        }
    }

    [TC(typeof(EXP))] public class AwcGranularLoopsChunk : AwcChunk
    {
        public uint GranularLoopsCount { get; set; }
        public GranularLoop[] GranularLoops { get; set; }

        public class GranularLoop : IMetaXmlItem
        {
            public uint UnkUint1 { get; set; } = 2;
            public uint GrainCount { get; set; }
            public MetaHash Hash { get; set; } = 0x4c633d07;
            public uint[] Grains { get; set; }

            public void Read(DataReader r)
            {
                UnkUint1 = r.ReadUInt32();
                GrainCount = r.ReadUInt32();
                Hash = r.ReadUInt32();
                Grains = new uint[GrainCount];
                for (int i = 0; i < GrainCount; i++)
                {
                    Grains[i] = r.ReadUInt32();
                }

                //switch (UnkUint1)
                //{
                //    case 2:
                //        break;
                //    default:
                //        break;//no hit
                //}
                //switch (Hash)
                //{
                //    case 0x4c633d07:
                //        break;
                //    default:
                //        break;//no hit
                //}
            }
            public void Write(DataWriter w)
            {
                GrainCount = (uint)(Grains?.Length ?? 0);
                w.Write(UnkUint1);
                w.Write(GrainCount);
                w.Write(Hash);
                for (int i = 0; i < GrainCount; i++)
                {
                    w.Write(Grains[i]);
                }
            }
            public void WriteXml(StringBuilder sb, int indent)
            {
                //AwcXml.ValueTag(sb, indent, "UnkUint1", UnkUint1.ToString());
                //AwcXml.StringTag(sb, indent, "Hash", AwcXml.HashString(Hash));
                AwcXml.WriteRawArray(sb, Grains, indent, "Grains", "");
            }
            public void ReadXml(XmlNode node)
            {
                //UnkUint1 = Xml.GetChildUIntAttribute(node, "UnkUint1");
                //Hash = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Hash"));
                Grains = Xml.GetChildRawUintArray(node, "Grains");
            }

            public override string ToString()
            {
                return Hash.ToString() + ": " + UnkUint1.ToString() + ": " + GrainCount.ToString() + " items";
            }

        }

        public AwcGranularLoopsChunk(AwcChunkInfo info) : base(info)
        {
        }

        public override void Read(DataReader r)
        {

            //uint count
            // [count*items]: uint(type?), uint(count2), hash, [count2*uint]

            GranularLoopsCount = r.ReadUInt32();
            GranularLoops = new GranularLoop[GranularLoopsCount];
            for (int i = 0; i < GranularLoopsCount; i++)
            {
                var g = new GranularLoop();
                g.Read(r);
                GranularLoops[i] = g;
            }

        }
        public override void Write(DataWriter w)
        {
            GranularLoopsCount = (uint)(GranularLoops?.Length ?? 0);
            w.Write(GranularLoopsCount);
            for (int i = 0; i < GranularLoopsCount; i++)
            {
                GranularLoops[i].Write(w);
            }
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            AwcXml.WriteItemArray(sb, GranularLoops, indent, "GranularLoops");
        }
        public override void ReadXml(XmlNode node)
        {
            GranularLoops = XmlMeta.ReadItemArray<GranularLoop>(node, "GranularLoops");
        }

        public override string ToString()
        {
            return "granularloops: " + (GranularLoops?.Length ?? 0).ToString() + " items";
        }
    }

    [TC(typeof(EXP))] public class AwcMarkersChunk : AwcChunk
    {
        public Marker[] Markers { get; set; }

        public class Marker : IMetaXmlItem
        {
            public MetaHash Name { get; set; }
            public MetaHash Value { get; set; }//usually a float, but in some cases a hash, or other value
            public uint SampleOffset { get; set; }
            public uint Unused { get; set; }

            public void Read(DataReader r)
            {
                Name = r.ReadUInt32();
                Value = r.ReadUInt32();
                SampleOffset = r.ReadUInt32();
                Unused = r.ReadUInt32();

                //switch (Name)
                //{
                //    case 0:
                //    case 0xa6d93246: // trackid
                //    case 0xe89ae78c: // beat
                //    case 0xf31b4f6a: // rockout
                //    case 0x08dba0f8: // dj
                //    case 0x7a495db3: // tempo
                //    case 0x14d857be: // g_s
                //        break;
                //    case 0xcd171e55: // 
                //    case 0x806b80c9: // 1
                //    case 0x91aa2346: // 2
                //    case 0x11976678: // r_p
                //    case 0x91be54cb: // 
                //    case 0xab2238c0: // 
                //    case 0xdb599288: // 
                //    case 0x2ce40eb5: // 
                //    case 0xa35e1092: // 01
                //    case 0x1332b405: // tank_jump
                //    case 0x2b20b891: // 
                //    case 0x8aa726e7: // tank_jump_land
                //    case 0xe0bfba99: // tank_turret_move
                //    case 0x1d91339e: // 
                //    case 0xa5344b07: // 
                //    case 0x7a7cba39: // tank_weapon_main_cannon_hit
                //    case 0xd66a90c3: // 
                //    case 0x1fd18857: // 14
                //    case 0x65a52c67: // 
                //    case 0xd8846402: // uihit
                //    case 0x8958bce4: // m_p
                //        if (Value != 0)
                //        { }//no hit
                //        break;
                //    default:
                //        break;//no hit
                //}

                //if (Unused != 0)
                //{ }//no hit
            }
            public void Write(DataWriter w)
            {
                w.Write(Name);
                w.Write(Value);
                w.Write(SampleOffset);
                w.Write(Unused);
            }
            public void WriteXml(StringBuilder sb, int indent)
            {
                AwcXml.StringTag(sb, indent, "Name", AwcXml.HashString(Name));
                switch (Name)
                {
                    case 0xf31b4f6a: // rockout
                    case 0x08dba0f8: // dj
                    case 0x14d857be: // g_s
                        AwcXml.StringTag(sb, indent, "Value", AwcXml.HashString(Value));
                        break;
                    default:
                        AwcXml.ValueTag(sb, indent, "Value", FloatUtil.ToString(Value.Float));
                        break;
                }
                AwcXml.ValueTag(sb, indent, "SampleOffset", SampleOffset.ToString());

            }
            public void ReadXml(XmlNode node)
            {
                Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
                switch (Name)
                {
                    case 0xf31b4f6a: // rockout
                    case 0x08dba0f8: // dj
                    case 0x14d857be: // g_s
                        Value = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Value"));
                        break;
                    default:
                        var f = Xml.GetChildFloatAttribute(node, "Value");
                        Value = MetaTypes.ConvertData<uint>(BitConverter.GetBytes(f));
                        break;
                }
                SampleOffset = Xml.GetChildUIntAttribute(node, "SampleOffset");

            }

            public override string ToString()
            {
                var valstr = Value.Float.ToString();
                switch (Name)
                {
                    case 0xf31b4f6a: // rockout
                    case 0x08dba0f8: // dj
                    case 0x14d857be: // g_s
                        valstr = Value.ToString();
                        break;
                }

                return Name.ToString() + ": " + valstr + ", " + SampleOffset.ToString() + ", " + Unused.ToString();
            }

        }
        

        public AwcMarkersChunk(AwcChunkInfo info) : base(info)
        { }

        public override void Read(DataReader r)
        {

            //if ((ChunkInfo.Size % 16) != 0)
            //{ }//no hit
            var count = ChunkInfo.Size / 16;
            Markers = new Marker[count];
            for (int i = 0; i < count; i++)
            {
                var m = new Marker();
                m.Read(r);
                Markers[i] = m;
            }

        }
        public override void Write(DataWriter w)
        {
            for (int i = 0; i < (Markers?.Length ?? 0); i++)
            {
                Markers[i].Write(w);
            }
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            AwcXml.WriteItemArray(sb, Markers, indent, "Markers");
        }
        public override void ReadXml(XmlNode node)
        {
            Markers = XmlMeta.ReadItemArray<Marker>(node, "Markers");
        }

        public override string ToString()
        {
            return "markers: " + (Markers?.Length ?? 0).ToString() + " markers";
        }
    }
   
    [TC(typeof(EXP))] public class AwcMIDIChunk : AwcChunk
    {
        public byte[] Data { get; set; }

        public AwcMIDIChunk(AwcChunkInfo info) : base(info)
        {
        }

        public override void Read(DataReader r)
        {
            Data = r.ReadBytes(ChunkInfo.Size);
        }
        public override void Write(DataWriter w)
        {
            w.Write(Data);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            //this is just a placeholder, as midi data will be written as a midi file
        }
        public override void ReadXml(XmlNode node)
        {
        }

        public override string ToString()
        {
            return "mid: " + (Data?.Length??0).ToString() + " bytes";
        }
    }

    [TC(typeof(EXP))] public class AwcSeekTableChunk : AwcChunk
    {
        public uint[] SeekTable { get; set; }

        public AwcSeekTableChunk(AwcChunkInfo info) : base(info)
        { }

        public override void Read(DataReader r)
        {

            //if ((ChunkInfo.Size % 4) != 0)
            //{ }//no hit
            var count = ChunkInfo.Size / 4;
            SeekTable = new uint[count];
            for (int i = 0; i < count; i++)
            {
                SeekTable[i] = r.ReadUInt32();
            }

        }
        public override void Write(DataWriter w)
        {
            for (int i = 0; i < (SeekTable?.Length ?? 0); i++)
            {
                w.Write(SeekTable[i]);
            }
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            //this is just a placeholder, since the seek table will be built dynamically by CW.
        }
        public override void ReadXml(XmlNode node)
        {
        }

        public override string ToString()
        {
            return "seektable: " + (SeekTable?.Length ?? 0).ToString() + " items";
        }
    }

    [TC(typeof(EXP))] public class AwcStreamDataBlock
    {
        public byte[] Data { get; set; }
        public int Index { get; set; }
        public int SampleOffset { get; set; }

        public AwcStreamDataChannel[] Channels { get; set; }


        public AwcStreamDataBlock(byte[] data, AwcStreamFormatChunk channelInfo, Endianess endianess, int index, int sampleOffset)
        {
            Data = data;
            Index = index;
            SampleOffset = sampleOffset;

            using (var ms = new MemoryStream(data))
            {
                var r = new DataReader(ms, endianess);

                var channelcount = channelInfo?.ChannelCount ?? 0;

                var ilist = new List<AwcStreamDataChannel>();
                for (int i = 0; i < channelcount; i++)
                {
                    var channel = new AwcStreamDataChannel(r);
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
                    channel.Data = r.ReadBytes(bcnt);
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

    [TC(typeof(EXP))] public class AwcStreamDataChannel
    {
        public int Unk0 { get; set; }
        public int OffsetCount { get; set; }
        public int Unk2 { get; set; }
        public int SampleCount { get; set; }
        public int Unk4 { get; set; }
        public int Unk5 { get; set; }

        public int[] SampleOffsets { get; set; }

        public byte[] Data { get; set; }


        public AwcStreamDataChannel(DataReader r)
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





    public class AwcXml : MetaXmlBase
    {

        public static string GetXml(AwcFile awc, string outputFolder = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if (awc != null)
            {
                AwcFile.WriteXmlNode(awc, sb, 0, outputFolder);
            }

            return sb.ToString();
        }

    }

    public class XmlAwc
    {

        public static AwcFile AwcYft(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetAwc(doc, inputFolder);
        }

        public static AwcFile GetAwc(XmlDocument doc, string inputFolder = "")
        {
            AwcFile r = null;

            var node = doc.DocumentElement;
            if (node != null)
            {
                r = AwcFile.ReadXmlNode(node, inputFolder);
            }

            r.Name = Path.GetFileName(inputFolder);

            return r;
        }

    }




}
