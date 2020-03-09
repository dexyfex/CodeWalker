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
        public ushort Version { get; set; } = 1;
        public ushort Flags { get; set; } = 0xFF00;
        public int StreamCount { get; set; }
        public int DataOffset { get; set; }

        public bool ChunkIndicesFlag { get { return ((Flags & 1) == 1); } set { Flags = (ushort)((Flags & 0xFFFE) + (value ? 1 : 0)); } }
        public bool SingleChannelEncryptFlag { get { return ((Flags & 2) == 2); } set { Flags = (ushort)((Flags & 0xFFFD) + (value ? 2 : 0)); } }
        public bool MultiChannelFlag { get { return ((Flags & 4) == 4); } set { Flags = (ushort)((Flags & 0xFFFB) + (value ? 4 : 0)); } }
        public bool MultiChannelEncryptFlag { get { return ((Flags & 8) == 8); } set { Flags = (ushort)((Flags & 0xFFF7) + (value ? 8 : 0)); } }

        public ushort[] ChunkIndices { get; set; } //index of first chunk for each stream
        public AwcChunkInfo[] ChunkInfos { get; set; } // just for browsing convenience really

        public bool WholeFileEncrypted { get; set; }

        public AwcStreamInfo[] StreamInfos { get; set; }
        public AwcStream[] Streams { get; set; }
        public AwcStream MultiChannelSource { get; set; }

        public Dictionary<uint, AwcStream> StreamDict { get; set; }


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
            DataOffset = r.ReadInt32();


            if (ChunkIndicesFlag)
            {
                ChunkIndices = new ushort[StreamCount]; //index of first chunk for each stream
                for (int i = 0; i < StreamCount; i++)
                {
                    ChunkIndices[i] = r.ReadUInt16();
                }
            }


            //if ((Flags >> 8) != 0xFF)
            //{ }//no hit

            //var infoStart = 16 + (ChunkIndicesFlag ? (StreamCount * 2) : 0);
            //if (r.Position != infoStart)
            //{ }//no hit


            var infos = new List<AwcStreamInfo>();
            var chunks = new List<AwcChunkInfo>();

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
                    chunks.Add(chunk);
                    info.Chunks[j] = chunk;
                }
            }
            StreamInfos = infos.ToArray();
            ChunkInfos = chunks.ToArray();

            //var dataOffset = infoStart + StreamCount * 4;
            //foreach (var info in infos) dataOffset += (int)info.ChunkCount * 8;
            //if (dataOffset != DataOffset)
            //{ }//no hit
            //if (r.Position != DataOffset)
            //{ }//no hit



            var streams = new List<AwcStream>();

            for (int i = 0; i < StreamCount; i++)
            {
                var info = StreamInfos[i];
                var stream = new AwcStream(info, this);
                stream.Read(r);
                streams.Add(stream);

                if (MultiChannelFlag && (stream.DataChunk != null))
                {
                    MultiChannelSource = stream;
                }
            }

            Streams = streams.ToArray();

            MultiChannelSource?.AssignMultiChannelSources(Streams);

            BuildStreamDict();

            //BuildPeakChunks();//for testing purposes only
            //TestChunkOrdering(r.Length);
        }

        private void Write(DataWriter w)
        {
            StreamCount = StreamInfos?.Length ?? 0;
            var infoStart = 16 + (ChunkIndicesFlag ? (StreamCount * 2) : 0);
            var dataOffset = infoStart + StreamCount * 4;
            foreach (var info in StreamInfos) dataOffset += (int)info.ChunkCount * 8;
            DataOffset = dataOffset;

            w.Write(Magic);
            w.Write(Version);
            w.Write(Flags);
            w.Write(StreamCount);
            w.Write(DataOffset);

            if (ChunkIndicesFlag)
            {
                for (int i = 0; i < StreamCount; i++)
                {
                    w.Write((i < (ChunkIndices?.Length ?? 0)) ? ChunkIndices[i] : (ushort)0);
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
                    var chunkinfo = info.Chunks[j];
                    chunkinfo.Write(w);
                }
            }


            var chunks = GetSortedChunks();
            foreach (var chunk in chunks)
            {
                var chunkinfo = chunk.ChunkInfo;
                var align = chunkinfo.Align;
                if (align > 0)
                {
                    var padc = (align - (w.Position % align)) % align;
                    if (padc > 0) w.Write(new byte[padc]);
                }
                chunk.Write(w);
            }


        }



        public void WriteXml(StringBuilder sb, int indent, string wavfolder)
        {
            AwcXml.ValueTag(sb, indent, "Version", Version.ToString());
            if (ChunkIndicesFlag)
            {
                AwcXml.ValueTag(sb, indent, "ChunkIndices", true.ToString());
            }
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
            ChunkIndicesFlag = Xml.GetChildBoolAttribute(node, "ChunkIndices");
            MultiChannelFlag = Xml.GetChildBoolAttribute(node, "MultiChannel");

            var snode = node.SelectSingleNode("Streams");
            if (snode != null)
            {
                var slist = new List<AwcStream>();
                var inodes = snode.SelectNodes("Item");
                foreach (XmlNode inode in inodes)
                {
                    var stream = new AwcStream(this);
                    stream.ReadXml(inode, wavfolder);
                    slist.Add(stream);

                    if (MultiChannelFlag && (stream.StreamFormatChunk != null) && (stream.Hash == 0))
                    {
                        MultiChannelSource = stream;
                    }
                }
                slist.Sort((a, b) => a.Hash.Hash.CompareTo(b.Hash.Hash));
                Streams = slist.ToArray();
                StreamCount = Streams?.Length ?? 0;

                MultiChannelSource?.CompactMultiChannelSources(Streams);
                
            }

            BuildPeakChunks();

            BuildChunkIndices();

            BuildStreamInfos();

            BuildStreamDict();
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


        public AwcChunk[] GetSortedChunks()
        {
            var chunks = new List<AwcChunk>();

            if (Streams != null)
            {
                foreach (var stream in Streams)
                {
                    if (stream.Chunks != null)
                    {
                        chunks.AddRange(stream.Chunks);
                    }
                }
            }

            var issorted = MultiChannelFlag || !SingleChannelEncryptFlag;
            if (issorted)
            {
                chunks.Sort((a, b) => b.ChunkInfo?.SortOrder.CompareTo(a.ChunkInfo?.SortOrder ?? 0) ?? -1);
                chunks.Reverse();
            }

            return chunks.ToArray();
        }

        public void TestChunkOrdering(long datalength)
        {
            if (Streams == null) return;
            if (StreamInfos == null) return;

            var issorted = MultiChannelFlag || !SingleChannelEncryptFlag;

            var chunklist = ChunkInfos.ToList();
            chunklist.Sort((a, b) => a.Offset.CompareTo(b.Offset));
            var chunks = chunklist.ToArray();

            //var chunks2 = GetAllChunks();

            var infoStart = 16 + (ChunkIndicesFlag ? (StreamCount * 2) : 0);
            var offset = infoStart + (StreamCount * 4) + (chunks.Length * 8);
            foreach (var chunk in chunks)
            {
                if (issorted)
                {
                    var align = chunk.Align;
                    if (align != 0)
                    {
                        offset += ((align - (offset % align)) % align);
                    }
                }
                switch (chunk.Type)
                {
                    case AwcChunkType.animation:
                        if (issorted)
                        { }//no hit
                        switch (chunk.Offset - offset)
                        {
                            case 12:
                            case 8:
                            case 0:
                            case 4:
                                break;
                            default:
                                break;//no hit
                        }
                        offset = chunk.Offset;//what is correct padding for this? seems to be inconsistent
                        break;
                    case AwcChunkType.gesture:
                        if (issorted)
                        { }//no hit
                        switch (chunk.Offset - offset)
                        {
                            case 0:
                            case 2:
                                break;
                            default:
                                break;//no hit
                        }
                        offset = chunk.Offset;//what is correct padding for this? seems to be inconsistent
                        break;
                    case AwcChunkType.seektable:
                        break;
                }
                if ((chunk.Offset - offset) != 0)
                { offset = chunk.Offset; }//no hit
                offset += chunk.Size;
            }

            if (WholeFileEncrypted) offset += (4 - (offset % 4)) % 4;
            if ((datalength - offset) != 0)
            { }//no hit


            if (issorted)
            {

                bool datachunk = false;
                bool markerchunk = false;
                foreach (var chunk in chunks)
                {
                    if ((chunk.Type == AwcChunkType.data) || (chunk.Type == AwcChunkType.mid)) datachunk = true;
                    else if (datachunk)
                    { }//no hit
                    if ((chunk.Type == AwcChunkType.markers) || (chunk.Type == AwcChunkType.granulargrains) || (chunk.Type == AwcChunkType.granularloops)) markerchunk = true;
                    else if (markerchunk && !datachunk)
                    { }//no hit
                }
            }
            else
            {
                if (ChunkInfos[0].Type != AwcChunkType.data)
                { }//no hit
            }
        }

        public void BuildPeakChunks()
        {
            if (Streams == null) return;


            foreach (var stream in Streams)
            {
                if (stream.FormatChunk == null)
                { continue; } // peak chunk is always null here
                if (stream.FormatChunk.Peak == null)
                { continue; } //rare, only in boar_near.awc
                //if (stream.FormatChunk.PeakUnk != 0)
                //{ }//some hits??
                //if ((stream.PeakChunk != null) && (stream.FormatChunk.Peak == null))
                //{ }//no hit

                var pcmData = stream.GetPcmData();
                var codec = stream.FormatChunk.Codec;
                var smpCount = stream.FormatChunk.Samples;
                var peak0 = stream.FormatChunk.PeakVal;//orig value for comparison
                var peakvals0 = stream.PeakChunk?.Data;//orig values for comparison
                var peakSize = 4096;
                var peakCount = (smpCount - peakSize) / peakSize;
                if (peakCount != (peakvals0?.Length ?? 0))
                { }//no hit
                var peakvals = new ushort[peakCount];

                ushort getSample(int i)
                {
                    var ei = i * 2;
                    if ((ei + 2) >= pcmData.Length) return 0;
                    var smp = BitConverter.ToInt16(pcmData, i * 2);
                    return (ushort)Math.Min(Math.Abs((int)smp) * 2, 65535);
                }
                ushort getPeak(int n)
                {
                    var o = n * peakSize;
                    var p = (ushort)0;
                    for (int i = 0; i < peakSize; i++)
                    {
                        var s = getSample(i + o);
                        p = Math.Max(p, s);
                    }
                    return p;
                }

                var peak = getPeak(0);
                stream.FormatChunk.PeakVal = peak;


                //// testing
                //if (peak != peak0)
                //{
                //    if (codec == AwcCodecType.PCM)
                //    { }//just 1 hit here for a tiny file in melee.awc
                //    if (codec == AwcCodecType.ADPCM)
                //    {
                //        var diff = Math.Abs(peak - peak0);
                //        if (diff > 25000)
                //        { }//no hit
                //    }
                //}


                for (int n = 0; n < peakCount; n++)
                {
                    var peakn = getPeak(n + 1);
                    peakvals[n] = peakn;

                    //// testing
                    //if (peakvals0 != null)
                    //{
                    //    var peakc = peakvals0[n];
                    //    if (peakn != peakc)
                    //    {
                    //        if (codec == AwcCodecType.PCM)
                    //        { }//no hit
                    //        if (codec == AwcCodecType.ADPCM)
                    //        {
                    //            var diff = Math.Abs(peakn - peakc);
                    //            if (diff > 33000)
                    //            { }//no hit
                    //        }
                    //    }
                    //}
                }

                if (stream.PeakChunk == null)
                {
                    if (peakvals.Length > 0)
                    {
                        //need to add a new peak chunk for the extra data (could happen on XML import)
                        var chunk = AwcStream.CreateChunk(new AwcChunkInfo() { Type = AwcChunkType.peak });
                        var chunklist = stream.Chunks?.ToList() ?? new List<AwcChunk>();
                        chunklist.Add(chunk);
                        stream.Chunks = chunklist.ToArray();
                        stream.PeakChunk = chunk as AwcPeakChunk;
                    }
                }
                if (stream.PeakChunk != null)
                {
                    if (peakvals.Length > 0)
                    {
                        stream.PeakChunk.Data = peakvals;
                    }
                    else
                    {
                        //remove any unneeded peak chunk (could happen on XML import)
                        var chunklist = stream.Chunks?.ToList() ?? new List<AwcChunk>();
                        chunklist.Remove(stream.PeakChunk);
                        stream.Chunks = chunklist.ToArray();
                        stream.PeakChunk = null;
                    }
                }


            }

        }

        public void BuildChunkIndices()
        {
            if (Streams == null) return;

            var inds = new List<ushort>();
            ushort ind = 0;
            foreach (var stream in Streams)
            {
                inds.Add(ind);
                ind += (ushort)(stream.Chunks?.Length ?? 0);
            }

            //if (ChunkIndices != null)
            //{
            //    if (ChunkIndices.Length == inds.Count)
            //    {
            //        for (int i = 0; i < inds.Count; i++)
            //        {
            //            if (inds[i] != ChunkIndices[i])
            //            { }//no hit
            //        }
            //    }
            //    else
            //    { }//no hit
            //}

            if (ChunkIndicesFlag)
            {
                ChunkIndices = inds.ToArray();
            }
            else
            {
                ChunkIndices = null;
            }

        }

        public void BuildStreamInfos()
        {

            var streaminfos = new List<AwcStreamInfo>();
            var chunkinfos = new List<AwcChunkInfo>();
            if (Streams != null)
            {
                var streamCount = Streams.Length;
                var infoStart = 16 + (ChunkIndicesFlag ? (streamCount * 2) : 0);
                var dataOffset = infoStart + streamCount * 4;
                foreach (var stream in Streams)
                {
                    dataOffset += (stream?.Chunks?.Length ?? 0) * 8;
                }

                var chunks = GetSortedChunks();
                foreach (var chunk in chunks)
                {
                    var chunkinfo = chunk.ChunkInfo;
                    var size = chunk.ChunkSize;
                    var align = chunkinfo.Align;
                    if (align > 0)
                    {
                        var padc = (align - (dataOffset % align)) % align;
                        dataOffset += padc;
                    }
                    chunkinfo.Size = size;
                    chunkinfo.Offset = dataOffset;
                    dataOffset += size;
                }

                foreach (var stream in Streams)
                {
                    var streaminfo = stream.StreamInfo;
                    streaminfos.Add(streaminfo);
                    chunkinfos.Clear();
                    if (stream.Chunks != null)
                    {
                        foreach (var chunk in stream.Chunks)
                        {
                            var chunkinfo = chunk.ChunkInfo;
                            chunkinfos.Add(chunkinfo);
                        }
                    }
                    streaminfo.Chunks = chunkinfos.ToArray();
                    streaminfo.ChunkCount = (uint)chunkinfos.Count;
                }
            }
            StreamInfos = streaminfos.ToArray();

        }

        public void BuildStreamDict()
        {
            StreamDict = new Dictionary<uint, AwcStream>();
            if (Streams == null) return;
            foreach (var stream in Streams)
            {
                StreamDict[stream.Hash] = stream;
            }
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

        public int SortOrder
        {
            get
            {
                switch (Type)
                {
                    case AwcChunkType.data:
                    case AwcChunkType.mid:
                        return 3;
                    case AwcChunkType.markers:
                    case AwcChunkType.granulargrains:
                    case AwcChunkType.granularloops:
                    case AwcChunkType.animation:
                    case AwcChunkType.gesture:
                        return 2;
                    case AwcChunkType.seektable:
                        return 1;
                    case AwcChunkType.peak:
                    case AwcChunkType.format:
                    case AwcChunkType.streamformat:
                        return 0;
                }
                return 0;
            }
        }
        public int Align
        {
            get
            {
                switch (Type)
                {
                    case AwcChunkType.data:
                    case AwcChunkType.mid:
                        return 16;
                    case AwcChunkType.markers:
                    case AwcChunkType.granulargrains:
                    case AwcChunkType.granularloops:
                        return 4;
                }
                return 0;
            }
        }

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
        public AwcStream[] ChannelStreams { get; set; }
        public AwcStream StreamSource { get; set; }
        public AwcStreamFormat StreamFormat { get; set; }
        public AwcStreamDataBlock[] StreamBlocks { get; set; }
        public int StreamChannelIndex { get; set; }


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
                if ((StreamFormatChunk != null) && (StreamFormatChunk.Channels?.Length > 0))
                {
                    var chan = StreamFormatChunk.Channels[0];
                    return (float)chan.Samples / chan.SamplesPerSecond;
                }
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

                if ((r.Position - cinfo.Offset) != cinfo.Size)
                { }//make sure everything was read!
            }
            Chunks = chunklist.ToArray();

            ExpandChunks();
            DecodeData(r.Endianess);
        }

        public void Write(DataWriter w)
        {
            //not used since chunks are collected and sorted, then written separately
        }

        public void WriteXml(StringBuilder sb, int indent, string wavfolder)
        {
            AwcXml.StringTag(sb, indent, "Name", AwcXml.HashString(HashAdjusted));
            if (Hash != 0) //skip the wave file output for multichannel sources
            {
                var export = !string.IsNullOrEmpty(wavfolder);
                var fname = Name?.Replace("/", "")?.Replace("\\", "") ?? "0x0";
                byte[] fdata = null;
                if (MidiChunk != null)
                {
                    fname += ".midi";
                    fdata = export ? MidiChunk.Data : null;
                }
                else
                {
                    fname += ".wav";
                    fdata = export ? GetWavFile() : null;
                }
                AwcXml.StringTag(sb, indent, "FileName", AwcXml.XmlEscape(fname));
                try
                {
                    if (export)
                    {
                        if (!Directory.Exists(wavfolder))
                        {
                            Directory.CreateDirectory(wavfolder);
                        }
                        var filepath = Path.Combine(wavfolder, fname);
                        File.WriteAllBytes(filepath, fdata);
                    }
                }
                catch
                { }
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
            var fnode = node.SelectSingleNode("StreamFormat");
            if (fnode != null)
            {
                StreamFormat = new AwcStreamFormat();
                StreamFormat.ReadXml(fnode);
                StreamFormat.Id = Hash;
            }
            var cnode = node.SelectSingleNode("Chunks");
            if (cnode != null)
            {
                var clist = new List<AwcChunk>();
                var inodes = cnode.SelectNodes("Item");
                foreach (XmlNode inode in inodes)
                {
                    var type = Xml.GetChildEnumInnerText<AwcChunkType>(inode, "Type");
                    var info = new AwcChunkInfo() { Type = type };
                    var chunk = CreateChunk(info);
                    chunk?.ReadXml(inode);
                    clist.Add(chunk);
                }
                Chunks = clist.ToArray();
            }

            ExpandChunks();

            var filename = Xml.GetChildInnerText(node, "FileName")?.Replace("/", "")?.Replace("\\", "");
            if (!string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(wavfolder))
            {
                try
                {
                    var filepath = Path.Combine(wavfolder, filename);
                    if (File.Exists(filepath))
                    {
                        var fdata = File.ReadAllBytes(filepath);
                        if (MidiChunk != null)
                        {
                            MidiChunk.Data = fdata;
                        }
                        else
                        {
                            ParseWavFile(fdata);
                        }
                    }
                }
                catch
                { }

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


        public void ExpandChunks()
        {
            if (Chunks != null)
            {
                foreach (var chunk in Chunks)
                {
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
                }
            }
        }


        public void DecodeData(Endianess endianess)
        {
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
                        var blk = new AwcStreamDataBlock(bdat, StreamFormatChunk, endianess, mcsoff);
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

        public void AssignMultiChannelSources(AwcStream[] streams)
        {
            var cstreams = new List<AwcStream>();
            for (int i = 0; i < (streams?.Length ?? 0); i++)
            {
                var stream = streams[i];
                if (stream != this)
                {
                    var id = stream.StreamInfo?.Id ?? 0;
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

                    stream.StreamSource = this;
                    stream.StreamFormat = (srcind < chancnt) ? StreamFormatChunk.Channels[srcind] : null;
                    stream.StreamChannelIndex = srcind;
                    cstreams.Add(stream);
                }
            }
            ChannelStreams = cstreams.ToArray();
        }

        public void CompactMultiChannelSources(AwcStream[] streams)
        {
            var chanlist = new List<AwcStreamFormat>();
            var chandatas = new List<byte[]>();
            for (int i = 0; i < (streams?.Length ?? 0); i++)
            {
                var stream = streams[i];
                if (stream != this)
                {
                    chanlist.Add(stream.StreamFormat);
                    chandatas.Add(stream.DataChunk?.Data);
                }
            }
            StreamFormatChunk.Channels = chanlist.ToArray();
            StreamFormatChunk.ChannelCount = (uint)chanlist.Count;

            //figure out how many smaller blocks fit in the larger block
            var chancount = chanlist.Count;
            var blocksize = (int)(StreamFormatChunk?.BlockSize ?? 1032192);
            var hdrsize = 96 * chancount + (blocksize / 512) + 1024;
            hdrsize += (0x800 - (hdrsize % 0x800)) % 0x800;
            var smblockspace = (blocksize - hdrsize) / 2048;
            var smblockcount = smblockspace / chancount;

            //split the channel datas into their blocks
            var streamblocks = new List<AwcStreamDataBlock>();
            var seektable = new List<uint>();
            var chansmpoffs = new List<int>();
            for (int c = 0; c < chancount; c++)
            {
                var chaninfo = chanlist[c];
                var chandata = chandatas[c];
                var cdlen = chandata?.Length ?? 0;
                var totsmblockcount = (cdlen / 2048) + (((cdlen % 2048) != 0) ? 1 : 0);
                var totlgblockcount = (totsmblockcount / smblockcount) + (((totsmblockcount % smblockcount) != 0) ? 1 : 0);
                for (int i = streamblocks.Count; i < totlgblockcount; i++)
                {
                    var blk = new AwcStreamDataBlock();
                    blk.ChannelInfo = StreamFormatChunk;
                    blk.Channels = new AwcStreamDataChannel[chancount];
                    blk.ChannelCount = (uint)chancount;
                    blk.SampleOffset = i * smblockcount * 4088;
                    streamblocks.Add(blk);
                    seektable.Add((uint)blk.SampleOffset);
                }

                chansmpoffs.Clear();
                var samplesrem = (int)chaninfo.Samples+1;
                for (int i = 0; i < totsmblockcount; i++)
                {

                    chansmpoffs.Add(i * 4088);
                    var blkcnt = chansmpoffs.Count;
                    if ((blkcnt == smblockcount) || (i == totsmblockcount-1))
                    {
                        var lgblockind = i / smblockcount;
                        var blkstart = lgblockind * smblockcount;
                        var blk = streamblocks[lgblockind];
                        var chan = new AwcStreamDataChannel();
                        var smpcnt = blkcnt * 4088;
                        var bytcnt = blkcnt * 2048;
                        var srcoff = blkstart * 2048;
                        var srccnt = Math.Min(bytcnt, cdlen - srcoff);
                        var data = new byte[bytcnt];
                        Buffer.BlockCopy(chandata, srcoff, data, 0, srccnt);
                        chan.Data = data;
                        chan.SampleOffsets = chansmpoffs.ToArray();
                        chan.SampleCount = Math.Min(smpcnt, samplesrem);
                        chan.BlockCount = blkcnt;
                        chan.StartBlock = (c * smblockcount);

                        blk.Channels[c] = chan;
                        chansmpoffs.Clear();
                        samplesrem -= smpcnt;
                    }

                }

            }

            StreamBlocks = streamblocks.ToArray();

            StreamFormatChunk.BlockCount = (uint)streamblocks.Count;

            if (streams != null)
            {
                foreach (var stream in streams)
                {
                    if (stream.SeekTableChunk != null)
                    {
                        stream.SeekTableChunk.SeekTable = seektable.ToArray();
                    }
                    if (stream.StreamFormatChunk != null)
                    {
                        stream.StreamFormatChunk.BlockCount = StreamFormatChunk.BlockCount;
                        stream.StreamFormatChunk.BlockSize = StreamFormatChunk.BlockSize;
                        stream.StreamFormatChunk.ChannelCount = StreamFormatChunk.ChannelCount;
                        stream.StreamFormatChunk.Channels = StreamFormatChunk.Channels;
                    }
                }
            }


            //build stream blocks into final data chunk
            var ms = new MemoryStream();
            var w = new DataWriter(ms);
            foreach (var blk in streamblocks)
            {
                var start = w.Position;
                blk.Write(w);
                var offs = w.Position - start;
                var padc = blocksize - offs;
                if (padc < 0)
                { }
                if (padc > 0)
                {
                    w.Write(new byte[padc]);
                }
            }
            var bytes = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(bytes, 0, (int)ms.Length);

            if (DataChunk == null)
            {
                DataChunk = new AwcDataChunk(new AwcChunkInfo() { Type = AwcChunkType.data });
            }
            DataChunk.Data = bytes;

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


        public byte[] GetRawData()
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

        public byte[] GetPcmData()
        {
            var data = GetRawData();

            var codec = StreamFormat?.Codec ?? FormatChunk?.Codec ?? AwcCodecType.PCM;
            if (codec == AwcCodecType.ADPCM)//just convert ADPCM to PCM for compatibility reasons
            {
                data = ADPCMCodec.DecodeADPCM(data, SampleCount);
            }

            return data;
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
            byte[] dataPCM = GetPcmData();
            var bitsPerSample = 16;
            var channels = 1;
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

        public void ParseWavFile(byte[] wav)
        {
            var ms = new MemoryStream(wav);
            var r = new DataReader(ms);

            var RIFF = r.ReadUInt32(); // 0x46464952
            var wavLength = r.ReadInt32();
            var WAVE = r.ReadUInt32(); // 0x45564157
            var fmt_ = r.ReadUInt32(); // 0x20746D66
            var fmtLength = r.ReadInt32();
            var formatcodec = r.ReadInt16();
            var channels = r.ReadInt16();
            var sampleRate = r.ReadInt32();
            var byteRate = r.ReadInt32();
            var blockAlign = r.ReadInt16();
            var bitsPerSample = r.ReadInt16();
            var ext2 = (ushort)0;
            var samplesPerBlock = (ushort)0;
            if (fmtLength == 20)
            {
                ext2 = r.ReadUInt16();
                samplesPerBlock = r.ReadUInt16();
            }
            var datatag = r.ReadUInt32(); // 0x61746164
            var datalen = r.ReadInt32();
            var dataPCM = r.ReadBytes(datalen);

            if (r.Position != r.Length)
            { }

            if (formatcodec != 1)
            {
                throw new Exception("Only PCM format .wav files supported!");
            }
            if (channels != 1)
            {
                throw new Exception("Only mono .wav files supported!");
            }

            var sampleCount = datalen * 2; //assume 16bits per sample PCM

            var codec = StreamFormat?.Codec ?? FormatChunk?.Codec ?? AwcCodecType.PCM;
            if (codec == AwcCodecType.ADPCM)// convert PCM wav to ADPCM where required
            {
                dataPCM = ADPCMCodec.EncodeADPCM(dataPCM, sampleCount);
                bitsPerSample = 4;
            }


            if (Awc.MultiChannelFlag)
            {
                if (StreamFormat != null)
                {
                    //StreamFormat.Samples = (uint)sampleCount;
                    StreamFormat.SamplesPerSecond = (ushort)sampleRate;

                    DataChunk = new AwcDataChunk(null);
                    DataChunk.Data = dataPCM;
                }
            }
            else
            {
                if (FormatChunk != null)
                {
                    //FormatChunk.Samples = (uint)sampleCount;
                    FormatChunk.SamplesPerSecond = (ushort)sampleRate;

                    if (DataChunk == null) DataChunk = new AwcDataChunk(new AwcChunkInfo() { Type = AwcChunkType.data });

                    DataChunk.Data = dataPCM;
                }

            }

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
        public abstract int ChunkSize { get; }
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
        public override int ChunkSize => Data?.Length ?? 0;

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
        public override int ChunkSize => Peak.HasValue ? 24 : 20;

        public uint Samples { get; set; }
        public int LoopPoint { get; set; }
        public ushort SamplesPerSecond { get; set; }
        public short Headroom { get; set; }
        public ushort LoopBegin { get; set; }
        public ushort LoopEnd { get; set; }
        public ushort PlayEnd { get; set; }
        public byte PlayBegin { get; set; }
        public AwcCodecType Codec { get; set; }
        public uint? Peak { get; set; }
        public ushort PeakVal { get { return (ushort)((Peak ?? 0) & 0xFFFF); } set { Peak = ((Peak ?? 0) & 0xFFFF0000) + value; } }
        public ushort PeakUnk { get { return (ushort)((Peak ?? 0) >> 16); } set { Peak = ((Peak ?? 0) & 0xFFFF) + (ushort)(value << 16); } }


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
                    Peak = r.ReadUInt32();
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
            if (Peak.HasValue)
            {
                w.Write(Peak.Value);
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
            AwcXml.ValueTag(sb, indent, "LoopPoint", LoopPoint.ToString());
            if (Peak.HasValue)
            {
                AwcXml.ValueTag(sb, indent, "Peak", PeakUnk.ToString(), "unk");
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
            LoopPoint = Xml.GetChildIntAttribute(node, "LoopPoint");
            var pnode = node.SelectSingleNode("Peak");
            if (pnode != null)
            {
                PeakUnk = (ushort)Xml.GetUIntAttribute(pnode, "unk");
            }
        }

        public override string ToString()
        {
            return "format: " + Codec.ToString() + ": " + Samples.ToString() + " samples, " + SamplesPerSecond.ToString() + " samples/sec, headroom: " + Headroom.ToString();
        }
    }

    [TC(typeof(EXP))] public class AwcStreamFormatChunk : AwcChunk
    {
        public override int ChunkSize => 12 + (Channels?.Length ?? 0) * 16;

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


            //switch (BlockSize)
            //{
            //    case 1032192:
            //    case 524288:
            //    case 7225344:
            //    case 307200:
            //        break;
            //    default:
            //        break;//no hit
            //}
        }
        public override void Write(DataWriter w)
        {
            ChannelCount = (uint)(Channels?.Length ?? 0);

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
            AwcXml.ValueTag(sb, indent, "BlockSize", BlockSize.ToString());
            //this is mostly just a placeholder. in XML, channel format is written with each channel stream
        }
        public override void ReadXml(XmlNode node)
        {
            BlockSize = Xml.GetChildUIntAttribute(node, "BlockSize");
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
        public override int ChunkSize => Data?.Length ?? 0;

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

            w.Write(Data);

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

                Data = ResourceBuilder.Build(ClipDict, 46, false); //ycd is 46...
            }
        }

        public override string ToString()
        {
            return "animation: " + (ClipDict?.ClipsMapEntries ?? 0).ToString() + " entries";
        }
    }

    [TC(typeof(EXP))] public class AwcPeakChunk : AwcChunk
    {
        public override int ChunkSize => (Data?.Length ?? 0) * 2;

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
            if (Data != null)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    w.Write(Data[i]);
                }
            }
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            AwcXml.StringTag(sb, indent, "Type", ChunkInfo?.Type.ToString());
            ////this is just a placeholder. in XML, peak data is generated from imported WAV data
            //AwcXml.WriteRawArray(sb, Data, indent, "Data", "");
        }
        public override void ReadXml(XmlNode node)
        {
            //Data = Xml.GetChildRawUshortArray(node, "Data");
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
        public override int ChunkSize => (Gestures?.Length ?? 0) * 36;

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
        public override int ChunkSize => 4 + (GranularGrains?.Length ?? 0) * 12;

        public GranularGrain[] GranularGrains { get; set; }
        public float UnkFloat1 { get; set; }

        public class GranularGrain : IMetaXmlItem
        {
            public uint UnkUint1 { get; set; } //sample offset?
            public float UnkFloat1 { get; set; } // duration..? rpm..?
            public ushort UnkUshort1 { get; set; } //peak low?
            public ushort UnkUshort2 { get; set; } //peak high?

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
            public void WriteLine(StringBuilder sb)
            {
                sb.Append(UnkUint1.ToString());
                sb.Append(" ");
                sb.Append(FloatUtil.ToString(UnkFloat1));
                sb.Append(" ");
                sb.Append(UnkUshort1.ToString());
                sb.Append(" ");
                sb.Append(UnkUshort2.ToString());
                sb.AppendLine();
            }
            public void ReadLine(string s)
            {
                var split = s.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var list = new List<string>();
                foreach (var str in split)
                {
                    var tstr = str.Trim();
                    if (!string.IsNullOrEmpty(tstr))
                    {
                        list.Add(tstr);
                    }
                }
                if (list.Count >= 4)
                {
                    uint.TryParse(list[0], out uint u1);
                    FloatUtil.TryParse(list[1], out float f1);
                    ushort.TryParse(list[2], out ushort s1);
                    ushort.TryParse(list[3], out ushort s2);
                    UnkUint1 = u1;
                    UnkFloat1 = f1;
                    UnkUshort1 = s1;
                    UnkUshort2 = s2;
                }
            }
            public void ReadXml(XmlNode node)
            {
                UnkUint1 = Xml.GetChildUIntAttribute(node, "UnkUint1");
                UnkFloat1 = Xml.GetChildFloatAttribute(node, "UnkFloat1");
                UnkUshort1 = (ushort)Xml.GetChildUIntAttribute(node, "UnkUshort1");
                UnkUshort2 = (ushort)Xml.GetChildUIntAttribute(node, "UnkUshort2");
            }
            public void WriteXml(StringBuilder sb, int indent)
            {
                AwcXml.ValueTag(sb, indent, "UnkUint1", UnkUint1.ToString());
                AwcXml.ValueTag(sb, indent, "UnkFloat1", FloatUtil.ToString(UnkFloat1));
                AwcXml.ValueTag(sb, indent, "UnkUshort1", UnkUshort1.ToString());
                AwcXml.ValueTag(sb, indent, "UnkUshort2", UnkUshort2.ToString());
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
            //AwcXml.WriteCustomItemArray(sb, GranularGrains, indent, "GranularGrains");
            if (GranularGrains != null)
            {
                AwcXml.OpenTag(sb, indent, "GranularGrains");
                var cind = indent + 1;
                foreach (var grain in GranularGrains)
                {
                    AwcXml.Indent(sb, cind);
                    grain.WriteLine(sb);
                }
                AwcXml.CloseTag(sb, indent, "GranularGrains");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            UnkFloat1 = Xml.GetChildFloatAttribute(node, "UnkFloat1");
            //GranularGrains = XmlMeta.ReadItemArray<GranularGrain>(node, "GranularGrains");
            var ggnode = node.SelectSingleNode("GranularGrains");
            if (ggnode != null)
            {
                var gglist = new List<GranularGrain>();
                var ggstr = ggnode.InnerText.Trim();
                var ggstrs = ggstr.Split('\n');
                foreach (var ggrstr in ggstrs)
                {
                    var rstr = ggrstr.Trim();
                    var ggr = new GranularGrain();
                    ggr.ReadLine(rstr);
                    gglist.Add(ggr);
                }
                GranularGrains = gglist.ToArray();
            }
        }

        public override string ToString()
        {
            return "granulargrains: " + (GranularGrains?.Length ?? 0).ToString() + " items";
        }
    }

    [TC(typeof(EXP))] public class AwcGranularLoopsChunk : AwcChunk
    {
        public override int ChunkSize
        {
            get
            {
                int size = 4 + (GranularLoops?.Length ?? 0) * 12;
                if (GranularLoops != null)
                {
                    foreach (var loop in GranularLoops)
                    {
                        size += (loop?.Grains?.Length ?? 0) * 4;
                    }
                }
                return size;
            }
        }

        public uint GranularLoopsCount { get; set; }
        public GranularLoop[] GranularLoops { get; set; }

        public class GranularLoop : IMetaXmlItem
        {
            public uint UnkUint1 { get; set; } = 2; //style="walk"?
            public uint GrainCount { get; set; }
            public MetaHash Identifier { get; set; } = 0x4c633d07; // "loop"
            public uint[] Grains { get; set; }

            public void Read(DataReader r)
            {
                UnkUint1 = r.ReadUInt32();
                GrainCount = r.ReadUInt32();
                Identifier = r.ReadUInt32();
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
                w.Write(Identifier);
                for (int i = 0; i < GrainCount; i++)
                {
                    w.Write(Grains[i]);
                }
            }
            public void WriteXml(StringBuilder sb, int indent)
            {
                //AwcXml.ValueTag(sb, indent, "UnkUint1", UnkUint1.ToString());
                //AwcXml.StringTag(sb, indent, "Identifier", AwcXml.HashString(Hash));
                AwcXml.WriteRawArray(sb, Grains, indent, "Grains", "");
            }
            public void ReadXml(XmlNode node)
            {
                //UnkUint1 = Xml.GetChildUIntAttribute(node, "UnkUint1");
                //Hash = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Identifier"));
                Grains = Xml.GetChildRawUintArray(node, "Grains");
            }

            public override string ToString()
            {
                return Identifier.ToString() + ": " + UnkUint1.ToString() + ": " + GrainCount.ToString() + " items";
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
        public override int ChunkSize => (Markers?.Length ?? 0) * 16;

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
        public override int ChunkSize => Data?.Length ?? 0;

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
        public override int ChunkSize => (SeekTable?.Length ?? 0) * 4;

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
        public int DataLength { get; set; }//just for convenience
        public int SampleOffset { get; set; }//just for convenience
        public uint ChannelCount { get; set; }//just for convenience
        public AwcStreamFormatChunk ChannelInfo { get; set; } //just for convenience
        public AwcStreamDataChannel[] Channels { get; set; }

        public AwcStreamDataBlock()
        { }
        public AwcStreamDataBlock(byte[] data, AwcStreamFormatChunk channelInfo, Endianess endianess, int sampleOffset)
        {
            DataLength = data?.Length ?? 0;
            SampleOffset = sampleOffset;
            ChannelCount = channelInfo?.ChannelCount ?? 0;
            ChannelInfo = channelInfo;

            using (var ms = new MemoryStream(data))
            {
                var r = new DataReader(ms, endianess);
                Read(r);
            }

        }

        public void Read(DataReader r)
        {
            var ilist = new List<AwcStreamDataChannel>();
            for (int i = 0; i < ChannelCount; i++)
            {
                var channel = new AwcStreamDataChannel();
                channel.Read(r);
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
                channel.ReadData(r);
            }

            //if (r.Position != r.Length)
            //{ }//still more, just padding?


        }
        public void Write(DataWriter w)
        {
            foreach (var channel in Channels)
            {
                channel.Write(w);
            }
            foreach (var channel in Channels)
            {
                channel.WriteOffsets(w);
            }
            var padc = (0x800 - (w.Position % 0x800)) % 0x800;
            if (padc > 0)
            {
                w.Write(new byte[padc]);
            }
            foreach (var channel in Channels)
            {
                channel.WriteData(w);
            }
        }


        public override string ToString()
        {
            return DataLength.ToString() + " bytes";
        }
    }

    [TC(typeof(EXP))] public class AwcStreamDataChannel
    {
        public int StartBlock { get; set; }
        public int BlockCount { get; set; }
        public int Unused1 { get; set; }
        public int SampleCount { get; set; }
        public int Unused2 { get; set; }
        public int Unused3 { get; set; }

        public int[] SampleOffsets { get; set; }
        public byte[] Data { get; set; }



        public void Read(DataReader r)
        {
            StartBlock = r.ReadInt32();
            BlockCount = r.ReadInt32();
            Unused1 = r.ReadInt32();
            SampleCount = r.ReadInt32();
            Unused2 = r.ReadInt32();
            Unused3 = r.ReadInt32();

            //if (Unused1 != 0)
            //{ }//no hit
            //if (Unused2 != 0)
            //{ }//no hit
            //if (Unused3 != 0)
            //{ }//no hit
        }
        public void Write(DataWriter w)
        {
            w.Write(StartBlock);
            w.Write(BlockCount);
            w.Write(Unused1);
            w.Write(SampleCount);
            w.Write(Unused2);
            w.Write(Unused3);
        }

        public void ReadOffsets(DataReader r)
        {
            var olist = new List<int>();
            for (int i = 0; i < BlockCount; i++)
            {
                var v = r.ReadInt32();
                olist.Add(v);
            }
            SampleOffsets = olist.ToArray();
        }
        public void WriteOffsets(DataWriter w)
        {
            var smpoc = SampleOffsets?.Length ?? 0;
            for (int i = 0; i < BlockCount; i++)
            {
                w.Write((i < smpoc) ? SampleOffsets[i] : 0);
            }
        }

        public void ReadData(DataReader r)
        {
            var bcnt = BlockCount * 2048;
            Data = r.ReadBytes(bcnt);
        }
        public void WriteData(DataWriter w)
        {
            w.Write(Data);
        }


        public override string ToString()
        {
            return StartBlock.ToString() + ": " + BlockCount.ToString() + ", " + Unused1.ToString() + ", " + SampleCount.ToString() + ", " + Unused2.ToString() + ", " + Unused3.ToString();
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
