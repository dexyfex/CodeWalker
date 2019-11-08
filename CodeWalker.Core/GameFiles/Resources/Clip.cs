using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
    Copyright(c) 2016 Neodymium

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/


//ruthlessly stolen


namespace CodeWalker.GameFiles
{


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipDictionary : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public ulong AnimationsPointer { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000101
        public uint Unknown_24h { get; set; } // 0x00000000
        public ulong ClipsPointer { get; set; }
        public ushort ClipsMapCapacity { get; set; }
        public ushort ClipsMapEntries { get; set; }
        public uint Unknown_34h { get; set; } // 0x01000000
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000

        // reference data
        public AnimationMap Animations { get; set; }
        public ResourcePointerArray64<ClipMapEntry> Clips { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.AnimationsPointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.ClipsPointer = reader.ReadUInt64();
            this.ClipsMapCapacity = reader.ReadUInt16();
            this.ClipsMapEntries = reader.ReadUInt16();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            this.Animations = reader.ReadBlockAt<AnimationMap>(
                this.AnimationsPointer // offset
            );
            this.Clips = reader.ReadBlockAt<ResourcePointerArray64<ClipMapEntry>>(
                this.ClipsPointer, // offset
                this.ClipsMapCapacity
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.AnimationsPointer = (ulong)(this.Animations != null ? this.Animations.FilePosition : 0);
            this.ClipsPointer = (ulong)(this.Clips != null ? this.Clips.FilePosition : 0);
            //this.c1 = (ushort)(this.Clips != null ? this.Clips.Count : 0);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.AnimationsPointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.ClipsPointer);
            writer.Write(this.ClipsMapCapacity);
            writer.Write(this.ClipsMapEntries);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Animations != null) list.Add(Animations);
            if (Clips != null) list.Add(Clips);
            return list.ToArray();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class AnimationMap : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public ulong AnimationsPointer { get; set; }
        public ushort AnimationsMapCapacity { get; set; }
        public ushort AnimationsMapEntries { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; } // 0x00000001
        public uint Unknown_2Ch { get; set; } // 0x00000000

        // reference data
        public ResourcePointerArray64<AnimationMapEntry> Animations { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.AnimationsPointer = reader.ReadUInt64();
            this.AnimationsMapCapacity = reader.ReadUInt16();
            this.AnimationsMapEntries = reader.ReadUInt16();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();

            // read reference data
            this.Animations = reader.ReadBlockAt<ResourcePointerArray64<AnimationMapEntry>>(
                this.AnimationsPointer, // offset
                this.AnimationsMapCapacity
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.AnimationsPointer = (ulong)(this.Animations != null ? this.Animations.FilePosition : 0);
            //this.c1 = (ushort)(this.Anims != null ? this.Anims.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.AnimationsPointer);
            writer.Write(this.AnimationsMapCapacity);
            writer.Write(this.AnimationsMapEntries);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Animations != null) list.Add(Animations);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class AnimationMapEntry : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32; }
        }

        // structure data
        public MetaHash Hash { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000000
        public ulong AnimationPtr { get; set; }
        public ulong NextEntryPtr { get; set; }
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000

        // reference data
        public Animation Animation { get; set; }
        public AnimationMapEntry NextEntry { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Hash = new MetaHash(reader.ReadUInt32());
            this.Unknown_04h = reader.ReadUInt32();
            this.AnimationPtr = reader.ReadUInt64();
            this.NextEntryPtr = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();

            // read reference data
            this.Animation = reader.ReadBlockAt<Animation>(
                this.AnimationPtr // offset
            );
            this.NextEntry = reader.ReadBlockAt<AnimationMapEntry>(
                this.NextEntryPtr // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.AnimationPtr = (ulong)(this.Animation != null ? this.Animation.FilePosition : 0);
            this.NextEntryPtr = (ulong)(this.NextEntry != null ? this.NextEntry.FilePosition : 0);

            // write structure data
            writer.Write(this.Hash);
            writer.Write(this.Unknown_04h);
            writer.Write(this.AnimationPtr);
            writer.Write(this.NextEntryPtr);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Animation != null) list.Add(Animation);
            if (NextEntry != null) list.Add(NextEntry);
            return list.ToArray();
        }

        public override string ToString()
        {
            return Hash.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Animation : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 96; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public ushort Unknown_10h { get; set; }
        public ushort Unknown_12h { get; set; }
        public ushort Frames { get; set; }
        public ushort SequenceFrameLimit { get; set; }
        public float Duration { get; set; }
        public MetaHash Unknown_1Ch { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint RawDataSize { get; set; }
        public uint UsageCount { get; set; }
        public ResourcePointerList64<Sequence> Sequences { get; set; }
        public ResourceSimpleList64_s<AnimationBoneId> BoneIds { get; set; }
        //public ResourceSimpleList64Ptr BoneIdsPtr { get; set; }
        //public AnimationBoneId[] BoneIds { get; set; }

        public YcdFile Ycd { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();  //1     1       1       1
            this.Unknown_08h = reader.ReadUInt32();  //0     0       0       0
            this.Unknown_0Ch = reader.ReadUInt32();  //0     0       0       0
            this.Unknown_10h = reader.ReadUInt16(); //257   257     257     257     flags?
            this.Unknown_12h = reader.ReadUInt16(); //0     0       0       0
            this.Frames = reader.ReadUInt16(); //221   17      151     201     frames
            this.SequenceFrameLimit = reader.ReadUInt16(); //223   31      159     207     sequence limit?
            this.Duration = reader.ReadSingle(); //7.34  0.53    5.0     6.66    duration
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32(); //0     0       0       0
            this.Unknown_24h = reader.ReadUInt32(); //0     0       0       0
            this.Unknown_28h = reader.ReadUInt32(); //0     0       0       0
            this.Unknown_2Ch = reader.ReadUInt32(); //0     0       0       0
            this.Unknown_30h = reader.ReadUInt32(); //0     0       0       0
            this.Unknown_34h = reader.ReadUInt32(); //0     0       0       0
            this.RawDataSize = reader.ReadUInt32(); //314   174     1238    390     sequences length?
            this.UsageCount = reader.ReadUInt32(); //2     2       2       2       material/type?
            this.Sequences = reader.ReadBlock<ResourcePointerList64<Sequence>>();
            this.BoneIds = reader.ReadBlock<ResourceSimpleList64_s<AnimationBoneId>>();
            //this.BoneIdsPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            ////this.BoneIds = reader.ReadUintsAt(this.BoneIdsPtr.EntriesPointer, this.BoneIdsPtr.EntriesCount);
            //this.BoneIds = reader.ReadStructsAt<AnimationBoneId>(this.BoneIdsPtr.EntriesPointer, this.BoneIdsPtr.EntriesCount);
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_12h);
            writer.Write(this.Frames);
            writer.Write(this.SequenceFrameLimit);
            writer.Write(this.Duration);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.RawDataSize);
            writer.Write(this.UsageCount);
            writer.WriteBlock(this.Sequences);
            writer.WriteBlock(this.BoneIds);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x40, Sequences),
                new Tuple<long, IResourceBlock>(0x50, BoneIds)
            };
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public struct AnimationBoneId
    {
        public ushort BoneId { get; set; }
        public byte Unk0 { get; set; }
        public byte Track { get; set; }
        public override string ToString()
        {
            return BoneId.ToString() + ": " + Unk0.ToString() + ", " + Track.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public abstract class AnimChannel
    {
        public int Sequence { get; private set; }
        public int Index { get; private set; }

        public abstract void Read(Sequence blockStream, ref int channelOffset);

        public virtual void ReadData(Sequence blockStream, ref int channelOffset)
        {

        }

        public virtual void ReadFrame(Sequence blockStream, int frame, ref int frameOffset)
        {

        }

        public virtual float EvaluateFloat(int frame) => 0.0f;

        public void Associate(int sequence, int index)
        {
            Sequence = sequence;
            Index = index;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class AnimChannelStaticFloat : AnimChannel
    {
        public float FloatValue { get; set; }

        public override void Read(Sequence blockStream, ref int channelOffset)
        {
            FloatValue = BitConverter.ToSingle(blockStream.Data, channelOffset);
            channelOffset += 4;
        }

        public override float EvaluateFloat(int frame)
        {
            return FloatValue;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class AnimChannelStaticVector3 : AnimChannel
    {
        public Vector3 Value { get; set; }

        public override void Read(Sequence blockStream, ref int channelOffset)
        {
            Value = new Vector3(
                BitConverter.ToSingle(blockStream.Data, channelOffset),
                BitConverter.ToSingle(blockStream.Data, channelOffset + 4),
                BitConverter.ToSingle(blockStream.Data, channelOffset + 8)
            );

            channelOffset += 12;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class AnimChannelStaticSmallestThreeQuaternion : AnimChannel
    {
        public Quaternion Value { get; set; }

        public override void Read(Sequence blockStream, ref int channelOffset)
        {
            var vec = new Vector3(
                BitConverter.ToSingle(blockStream.Data, channelOffset),
                BitConverter.ToSingle(blockStream.Data, channelOffset + 4),
                BitConverter.ToSingle(blockStream.Data, channelOffset + 8)
            );

            Value = new Quaternion(
                vec,
                (float)Math.Sqrt(Math.Max(1.0f - vec.LengthSquared(), 0.0))
            );

            channelOffset += 12;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class AnimChannelIndirectQuantizeFloat : AnimChannel
    {
        public int FrameBits { get; set; }
        public int ValueBits { get; set; }
        public float Quantum { get; set; }
        public float Offset { get; set; }
        public float[] Values { get; set; }
        public uint[] Frames { get; set; }

        private int numInts;

        public override void Read(Sequence blockStream, ref int channelOffset)
        {
            FrameBits = BitConverter.ToInt32(blockStream.Data, channelOffset);
            ValueBits = BitConverter.ToInt32(blockStream.Data, channelOffset + 4);
            numInts = BitConverter.ToInt32(blockStream.Data, channelOffset + 8);
            Quantum = BitConverter.ToSingle(blockStream.Data, channelOffset + 12);
            Offset = BitConverter.ToSingle(blockStream.Data, channelOffset + 16);

            channelOffset += 20;

            var bit = channelOffset * 8;
            var endBit = bit + (numInts * 32);

            channelOffset += numInts * 4;

            var valueList = new List<float>();

            while (bit < endBit)
            {
                valueList.Add((blockStream.GetBit(bit, ValueBits) * Quantum) + Offset);

                bit += ValueBits;
            }

            Values = valueList.ToArray();

            Frames = new uint[blockStream.NumFrames];
        }

        public override void ReadData(Sequence blockStream, ref int channelOffset)
        {

        }

        public override void ReadFrame(Sequence blockStream, int frame, ref int frameOffset)
        {
            Frames[frame] = blockStream.GetBit(frameOffset, FrameBits);

            frameOffset += FrameBits;
        }

        public override float EvaluateFloat(int frame)
        {
            if (Frames?.Length > 0) return Values[Frames[frame % Frames.Length]];
            return Offset;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class AnimChannelQuantizeFloat : AnimChannel
    {
        public int ValueBits { get; set; }
        public float Quantum { get; set; }
        public float Offset { get; set; }
        public float[] Values { get; set; }

        public override void Read(Sequence blockStream, ref int channelOffset)
        {
            ValueBits = BitConverter.ToInt32(blockStream.Data, channelOffset);
            Quantum = BitConverter.ToSingle(blockStream.Data, channelOffset + 4);
            Offset = BitConverter.ToSingle(blockStream.Data, channelOffset + 8);
            Values = new float[blockStream.NumFrames];

            channelOffset += 12;
        }

        public override void ReadFrame(Sequence blockStream, int frame, ref int frameOffset)
        {
            Values[frame] = (blockStream.GetBit(frameOffset, ValueBits) * Quantum) + Offset;
            frameOffset += ValueBits;
        }

        public override float EvaluateFloat(int frame)
        {
            if (Values?.Length > 0) return Values[frame%Values.Length];
            return Offset;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class AnimChannelLinearFloat : AnimChannel
    {
        public float Quantum { get; set; }
        public float Offset { get; set; }
        public float[] Values { get; set; }

        private Sequence blockStream;

        private int NumInts { get; set; }

        private int Count1 { get; set; }
        private int Count2 { get; set; }
        private int Count3 { get; set; }

        private int Bit { get; set; }

        public override void Read(Sequence blockStream, ref int channelOffset)
        {
            NumInts = BitConverter.ToInt32(blockStream.Data, channelOffset);
            var counts = BitConverter.ToInt32(blockStream.Data, channelOffset + 4);
            Quantum = BitConverter.ToSingle(blockStream.Data, channelOffset + 8);
            Offset = BitConverter.ToSingle(blockStream.Data, channelOffset + 12);

            var count1 = counts & 0xFF;
            var count2 = (counts >> 8) & 0xFF;
            var count3 = (counts >> 16) & 0xFF;

            Count1 = count1;
            Count2 = count2;
            Count3 = count3;

            Values = new float[blockStream.NumFrames];

            Bit = (channelOffset * 8) + 128;

            this.blockStream = blockStream;

            var numChunks = (blockStream.ChunkSize + blockStream.NumFrames) / blockStream.ChunkSize;

            for (int i = 0; i < blockStream.NumFrames; i++)
            {
                Values[i] = ReconstructFloat(i);
            }

            channelOffset += NumInts * 4;
        }

        private float ReconstructFloat(int frame)
        {
            var chunkSize = blockStream.ChunkSize;
            var numChunks = (blockStream.ChunkSize + blockStream.NumFrames) / chunkSize;

            var startBit = Bit;
            var startBit2 = (startBit + (numChunks * Count1));
            uint startBit3 = (uint)(startBit2 + (numChunks * Count2));

            var chunkIdx = (frame / chunkSize);
            var chunkOff = (frame % chunkSize);

            var offset = (Count1 != 0) ? blockStream.GetBit(startBit + (chunkIdx * Count1), Count1) : 0;
            var value = (Count2 != 0) ? (int)blockStream.GetBit(startBit2 + (chunkIdx * Count2), Count2) : 0;

            var inc = 0;

            for (int i = 0; i < chunkOff; i++)
            {
                var delta = (Count3 != 0) ? (int)blockStream.GetBit((int)(startBit3 + offset), Count3) : 0;
                offset += (uint)Count3;

                // scan for a '1' bit
                uint bitIdx = 0;

                {
                    uint b = 0;

                    do
                    {
                        b = blockStream.GetBit((int)(startBit3 + offset + bitIdx), 1);
                        bitIdx++;
                    } while (b == 0);
                }

                offset += bitIdx;

                delta |= (int)(((bitIdx - 1) << Count3));

                if (delta != 0)
                {
                    var sign = blockStream.GetBit((int)(startBit3 + offset), 1);
                    offset += 1;

                    if (sign == 1)
                    {
                        delta = -delta;
                    }
                }

                inc += delta;
                value += inc;
            }

            return (value * Quantum) + Offset;
        }

        public override float EvaluateFloat(int frame)
        {
            if (Values?.Length > 0) return Values[frame % Values.Length];
            return Offset;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class AnimChannelType7 : AnimChannel
    {
        private Sequence blockStream;

        private float[] valueCache;

        public float[] Values
        {
            get
            {
                if (valueCache != null)
                {
                    return valueCache;
                }

                valueCache = new float[blockStream.NumFrames];

                var channels = new AnimChannel[3];
                var ch = 0;

                for (int i = 0; i < 4; i++)
                {
                    if (i != Index)
                    {
                        channels[ch] = blockStream.Sequences[Sequence].Channels[i];
                        ch++;
                    }
                }

                for (int i = 0; i < valueCache.Length; i++)
                {
                    var vec = new Vector3(
                        channels[0].EvaluateFloat(i),
                        channels[1].EvaluateFloat(i),
                        channels[2].EvaluateFloat(i)
                    );

                    valueCache[i] = (float)Math.Sqrt(Math.Max(1.0f - vec.LengthSquared(), 0.0));
                }

                return valueCache;
            }
        }

        public int QuatIndex { get; internal set; }

        public override float EvaluateFloat(int frame)
        {
            if (Values?.Length > 0) return Values[frame % Values.Length];
            return 0.0f;
        }

        public override void Read(Sequence blockStream, ref int channelOffset)
        {
            this.blockStream = blockStream;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class AnimSequence
    {
        public AnimChannel[] Channels { get; set; }
        public bool IsType7Quat { get; internal set; }

        public Quaternion EvaluateQuaternion(int frame)
        {
            if (!IsType7Quat)
            {
                return new Quaternion(
                    Channels[0].EvaluateFloat(frame),
                    Channels[1].EvaluateFloat(frame),
                    Channels[2].EvaluateFloat(frame),
                    Channels[3].EvaluateFloat(frame)
                );
            }

            var t7 = (AnimChannelType7)Channels[3];

            var x = Channels[0].EvaluateFloat(frame);
            var y = Channels[1].EvaluateFloat(frame);
            var z = Channels[2].EvaluateFloat(frame);
            var normalized = t7.EvaluateFloat(frame);

            switch (t7.QuatIndex)
            {
                case 0:
                    return new Quaternion(normalized, x, y, z);
                case 1:
                    return new Quaternion(x, normalized, y, z);
                case 2:
                    return new Quaternion(x, y, normalized, z);
                case 3:
                    return new Quaternion(x, y, z, normalized);
                default:
                    return Quaternion.Identity;
            }
        }

        public Vector4 EvaluateVector(int frame)
        {
            if (Channels == null) return Vector4.Zero;
            if (IsType7Quat) return Quaternion.Normalize(EvaluateQuaternion(frame)).ToVector4();//normalization shouldn't be necessary, but saves explosions in case of incorrectness
            var v = Vector4.Zero;
            int c = 0;
            for (int i = 0; i < Channels.Length; i++)
            {
                if (c >= 4) break;
                var channel = Channels[i];
                var sv3c = channel as AnimChannelStaticVector3;
                var ssqc = channel as AnimChannelStaticSmallestThreeQuaternion;
                if (sv3c != null)
                {
                    for (int n = 0; n < 3; n++)
                    {
                        if ((c + n) >= 4) break;
                        v[c + n] = sv3c.Value[n];
                    }
                    c += 3;
                }
                else if (ssqc != null)
                {
                    for (int n = 0; n < 4; n++)
                    {
                        if ((c + n) >= 4) break;
                        v[c + n] = ssqc.Value[n];
                    }
                    c += 4;
                }
                else
                {
                    v[c] = channel.EvaluateFloat(frame);
                    c++;
                }
            }
            return v;
        }


    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Sequence : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32 + Data.Length; }
        }

        // structure data
        public MetaHash Unknown_00h { get; set; } //identifier / name?
        public uint DataLength { get; set; }
        public uint Unused_08h { get; set; } // 0x00000000
        public uint FrameOffset { get; set; } //offset to data items / bytes used by "Part0"?
        public uint UnkLength { get; set; } //total block length? usually == BlockLength
        public ushort Unused_14h { get; set; } //0x0000
        public ushort NumFrames { get; set; } // count of data items
        public ushort FrameLength { get; set; } //stride of data item
        public ushort Unknown_1Ah { get; set; } //?
        public ushort Unknown_1Ch { get; set; } //?
        public byte ChunkSize { get; set; } //64|255                 0x40|0xFF
        public byte Unknown_1Fh_Type { get; set; } //0|17|20|21|49|52|53    0x11|0x14|0x15|0x31|0x34|0x35
        public byte[] Data { get; set; }



        // parsed data
        public AnimSequence[] Sequences { get; set; }


        // //Original testing parsed data
        //public SequencePart1[] FrameData { get; set; }
        //public ushort[] Part2 { get; set; }
        //public int Part2Count { get; set; }
        //public int Part2Offset { get; set; }
        //public static Dictionary<ushort, int> SeqDict = new Dictionary<ushort, int>();


        class AnimChannelListItem
        {
            public int Sequence;
            public int Index;
            public AnimChannel Channel;
            public AnimChannelListItem(int seq, int ind, AnimChannel channel)
            {
                Sequence = seq;
                Index = ind;
                Channel = channel;
            }
        }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_00h = reader.ReadUInt32();//2965995365  2837183178
            this.DataLength = reader.ReadUInt32(); //282        142        1206       358
            this.Unused_08h = reader.ReadUInt32();//0          0          0          0
            this.FrameOffset = reader.ReadUInt32();//224 (E0)   32 (20)    536 (218)  300    
            this.UnkLength = reader.ReadUInt32();//314        174        1238       390 (=Length)
            this.Unused_14h = reader.ReadUInt16();//0          0          0          0
            this.NumFrames = reader.ReadUInt16();//221 (DD)   17 (11)    151 (97)   201
            this.FrameLength = reader.ReadUInt16();//0          4          4          0      
            this.Unknown_1Ah = reader.ReadUInt16();//0          0          106        0      
            this.Unknown_1Ch = reader.ReadUInt16();//0          17         0          0      bone?
            this.ChunkSize = reader.ReadByte();  //64         255        255        64
            this.Unknown_1Fh_Type = reader.ReadByte();  //0          0          0          0


            this.Data = reader.ReadBytes((int)DataLength);

            #region //old dexyfex testing code
            /* 
            if (Unused_08h != 0)
            { }
            if (Unused_14h != 0)
            { }
            if (UnkLength != (DataLength + 32)) //sometimes this is true
            { }
            if ((FrameLength % 4) > 0)
            { }
            int offset = (int)FrameOffset;
            if (FrameLength > 0)
            {
                FrameData = new SequencePart1[NumFrames];
                for (int i = 0; i < NumFrames; i++)
                {
                    var sp = new SequencePart1();
                    sp.Init(Data, offset, FrameLength);
                    FrameData[i] = sp;
                    offset += FrameLength;
                }
            }
            else if (NumFrames != 0)
            { }
            int brem = (int)DataLength - offset;
            int p2cnt = brem / 2;
            if (p2cnt > 0)
            {
                Part2Offset = offset;
                Part2Count = p2cnt;
                Part2 = new ushort[p2cnt];
                for (int i = 0; i < p2cnt; i++)
                {
                    Part2[i] = BitConverter.ToUInt16(Data, offset);
                    offset += 2;
                }
            }
            else
            { }
            if (offset != DataLength)
            { } //no hits here!
            //if (SeqDict.ContainsKey(Unknown_1Ah)) SeqDict[Unknown_1Ah]++;
            //else SeqDict[Unknown_1Ah] = 1;
            if ((Unknown_1Ah != 0) && (Unknown_1Ah > FrameOffset))
            { }
            if ((Unknown_1Ch != 0) && (Unknown_1Ch > FrameOffset))
            { }
            switch (ChunkSize)
            {
                case 64: //0x40
                case 255: //0xFF
                    break;
                default://no hits
                    break;
            }
            switch (Unknown_1Fh_Type)
            {
                case 0:
                case 17: //0x11
                case 20: //0x14
                case 21: //0x15
                case 49: //0x31
                case 52: //0x34
                case 53: //0x35
                    break;
                default: //no hits
                    break;
            }
            */
            #endregion


            int Part2Offset = 0;//replacement calculation from old dexyfex parsing code
            int offset = (int)FrameOffset + (FrameLength * NumFrames);
            int p2cnt = ((int)DataLength - offset) / 2;
            if (p2cnt > 0)
            {
                Part2Offset = offset;
            }





            int channelBitOffset = 0;
            int channelFrameOffset = unchecked((int)(FrameOffset * 8));
            int channelListOffset = Part2Offset;
            int channelDataOffset = Part2Offset + (9 * 2);

            var animChannelList = new List<AnimChannelListItem>();

            var channelLists = new AnimChannel[9][];

            for (int i = 0; i < 9; i++)
            {
                int channelCount = BitConverter.ToUInt16(Data, channelListOffset);

                /*if (channelCount > 4)
                {
                    Debug.Assert(false, "More than 4 channels per type are currently unsupported!");
                }*/

                var channels = new AnimChannel[channelCount];

                for (int c = 0; c < channelCount; c++)
                {
                    AnimChannel channel = null;

                    switch (i)
                    {
                        case 0:
                            channel = new AnimChannelStaticSmallestThreeQuaternion();
                            break;
                        case 1:
                            channel = new AnimChannelStaticVector3();
                            break;
                        case 2:
                            channel = new AnimChannelStaticFloat();
                            break;
                        case 3:
                            // crAnimChannelRawFloat
                            break;
                        case 4:
                            channel = new AnimChannelQuantizeFloat();
                            break;
                        case 5:
                            channel = new AnimChannelIndirectQuantizeFloat();
                            break;
                        case 6:
                            channel = new AnimChannelLinearFloat();
                            break;
                        case 7:
                            // normalized W from quaternion (evaluate first three channels, calculate W)
                            channel = new AnimChannelType7();
                            break;
                        case 8:
                            // unknown extra
                            // kind of the same as above but different at runtime?
                            channel = new AnimChannelType7();
                            break;
                        default:
                            break;
                    }

                    //Debug.Assert(channel != null, "Unsupported channel");

                    if (channel != null)
                    {
                        channel.Read(this, ref channelBitOffset);
                    }

                    channels[c] = channel;
                }

                for (int c = 0; c < channelCount; c++)
                {
                    var channel = channels[c];

                    if (channel != null)
                    {
                        channel.ReadData(this, ref channelBitOffset);
                    }

                    var channelDataBit = BitConverter.ToUInt16(Data, channelDataOffset + (c * 2));
                    var sequence = channelDataBit >> 2;
                    var index = channelDataBit & 3;

                    if (channel != null)
                    {
                        if (i == 7 || i == 8)
                        {
                            if (channel is AnimChannelType7 t7)
                            {
                                t7.QuatIndex = index;
                            }

                            index = 3;
                        }

                        channel.Associate(sequence, index);

                        animChannelList.Add(new AnimChannelListItem(sequence, index, channel));
                    }
                }

                if (channelCount > 0)
                {
                    var listSize = ((channelCount + 3) / 4) * 4;

                    channelDataOffset += (2 * listSize);
                }

                channelListOffset += 2;

                channelLists[i] = channels;
            }

            for (int f = 0; f < NumFrames; f++)
            {
                channelFrameOffset = unchecked((int)((FrameOffset + (FrameLength * f)) * 8));

                for (int i = 0; i < channelLists.Length; i++)
                {
                    var channels = channelLists[i];

                    for (int c = 0; c < channels.Length; c++)
                    {
                        var channel = channels[c];

                        if (channel != null)
                        {
                            channel.ReadFrame(this, f, ref channelFrameOffset);
                        }
                    }
                }
            }

            Sequences = new AnimSequence[animChannelList.Max(a => a.Sequence) + 1];

            for (int i = 0; i < Sequences.Length; i++)
            {
                Sequences[i] = new AnimSequence();

                var thisSeq = animChannelList.Where(a => a.Sequence == i);
                if (thisSeq.Count() == 0)
                { continue; }


                Sequences[i].Channels = new AnimChannel[thisSeq.Max(a => a.Index) + 1];
                
                for (int j = 0; j < Sequences[i].Channels.Length; j++)
                {
                    Sequences[i].Channels[j] = thisSeq.First(a => a.Index == j).Channel;

                    if (Sequences[i].Channels[j] is AnimChannelType7)
                    {
                        Sequences[i].IsType7Quat = true;
                    }
                }
            }
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_00h);
            writer.Write(this.DataLength);
            writer.Write(this.Unused_08h);
            writer.Write(this.FrameOffset);
            writer.Write(this.UnkLength);
            writer.Write(this.Unused_14h);
            writer.Write(this.FrameLength);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.ChunkSize);
            writer.Write(this.Data);
        }

        public override string ToString()
        {
            return Unknown_00h.ToString() + ": " + DataLength.ToString();
        }

        public uint GetBit(int startBit, int length)
        {
            var mask = MaskTable[length];

            var lowByte = BitConverter.ToUInt32(Data, (startBit / 32) * 4);
            var highByte = BitConverter.ToUInt32(Data, ((startBit / 32) + 1) * 4);

            var pair = ((ulong)highByte << 32) | lowByte;

            return (uint)((pair >> (startBit % 32)) & mask);
        }

        private static uint[] MaskTable = new uint[]
        {
            0, 1, 3, 7, 0xF, 0x1F, 0x3F, 0x7F, 0xFF, 0x1FF, 0x3FF,
            0x7FF, 0xFFF, 0x1FFF, 0x3FFF, 0x7FFF, 0xFFFF, 0x1FFFF,
            0x3FFFF, 0x7FFFF, 0xFFFFF, 0x1FFFFF, 0x3FFFFF, 0x7FFFFF,
            0xFFFFFF, 0x1FFFFFF, 0x3FFFFFF, 0x7FFFFFF, 0xFFFFFFF,
            0x1FFFFFFF, 0x3FFFFFFF, 0x7FFFFFFF, 0xFFFFFFFF
        };
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipMapEntry : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32; }
        }

        // structure data
        public MetaHash Hash { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000000
        public ulong ClipPointer { get; set; }
        public ulong NextPointer { get; set; }
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000

        // reference data
        public ClipBase Clip { get; set; }
        public ClipMapEntry Next { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Hash = new MetaHash(reader.ReadUInt32());
            this.Unknown_04h = reader.ReadUInt32();
            this.ClipPointer = reader.ReadUInt64();
            this.NextPointer = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();

            // read reference data
            this.Clip = reader.ReadBlockAt<ClipBase>(
                this.ClipPointer // offset
            );
            this.Next = reader.ReadBlockAt<ClipMapEntry>(
                this.NextPointer // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.ClipPointer = (ulong)(this.Clip != null ? this.Clip.FilePosition : 0);
            this.NextPointer = (ulong)(this.Next != null ? this.Next.FilePosition : 0);

            // write structure data
            writer.Write(this.Hash);
            writer.Write(this.Unknown_04h);
            writer.Write(this.ClipPointer);
            writer.Write(this.NextPointer);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Clip != null) list.Add(Clip);
            if (Next != null) list.Add(Next);
            return list.ToArray();
        }

        public override string ToString()
        {
            return Clip?.Name ?? Hash.ToString();
        }

    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipBase : ResourceSystemBlock, IResourceXXSystemBlock
    {
        public override long BlockLength
        {
            get { return 112; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public ulong NamePointer { get; set; }
        public ushort NameLength { get; set; } // short, name length
        public ushort NameCapacity { get; set; } // short, name length +1
        public uint Unknown_24h { get; set; } // 0x00000000
        public ulong Unknown_28hPtr { get; set; } // 0x50000000
        public uint Unknown_30h { get; set; }
        public uint Unknown_34h { get; set; } // 0x00000000
        public ulong TagsPointer { get; set; }
        public ulong PropertiesPointer { get; set; }
        public uint Unknown_48h { get; set; } // 0x00000001
        public uint Unknown_4Ch { get; set; } // 0x00000000       

        // reference data
        public string Name { get; set; }
        public ClipTagList Tags { get; set; }
        public ClipPropertyMap Properties { get; set; }

        public YcdFile Ycd { get; set; }
        public string ShortName { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.NameLength = reader.ReadUInt16();
            this.NameCapacity = reader.ReadUInt16();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28hPtr = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.TagsPointer = reader.ReadUInt64();
            this.PropertiesPointer = reader.ReadUInt64();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();


            this.Name = reader.ReadStringAt(this.NamePointer);
            this.Tags = reader.ReadBlockAt<ClipTagList>(
                this.TagsPointer // offset
            );
            this.Properties = reader.ReadBlockAt<ClipPropertyMap>(
                this.PropertiesPointer // offset
            );

            if ((Unknown_28hPtr != 0) && (Unknown_28hPtr != 0x50000000))
            {
            }

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            //this.NamePointer = (ulong)(this.Name != null ? this.Name.Position : 0);
            this.TagsPointer = (ulong)(this.Tags != null ? this.Tags.FilePosition : 0);
            this.PropertiesPointer = (ulong)(this.Properties != null ? this.Properties.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.NamePointer);
            writer.Write(this.NameLength);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28hPtr);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.TagsPointer);
            writer.Write(this.PropertiesPointer);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            //if (Name != null) list.Add(Name);
            if (Tags != null) list.Add(Tags);
            if (Properties != null) list.Add(Properties);
            return list.ToArray();
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 16;
            var type = reader.ReadByte();
            reader.Position -= 17;

            switch (type)
            {
                case 1: return new ClipAnimation();
                case 2: return new ClipAnimationList();
                default: return null;// throw new Exception("Unknown type");
            }
        }


        public override string ToString()
        {
            return Name;
        }

    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipAnimation : ClipBase
    {
        public override long BlockLength
        {
            get { return 112; }
        }

        // structure data
        public ulong AnimationPointer { get; set; }
        public float StartTime { get; set; } //start time
        public float EndTime { get; set; } //end time
        public float Rate { get; set; } //1.0  rate..?
        public uint Unknown_64h { get; set; } // 0x00000000
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000

        // reference data
        public Animation Animation { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);
            this.AnimationPointer = reader.ReadUInt64();
            this.StartTime = reader.ReadSingle();
            this.EndTime = reader.ReadSingle();
            this.Rate = reader.ReadSingle();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();

            this.Animation = reader.ReadBlockAt<Animation>(
                this.AnimationPointer // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            this.AnimationPointer = (ulong)(this.Animation != null ? this.Animation.FilePosition : 0);

            writer.Write(this.AnimationPointer);
            writer.Write(this.StartTime);
            writer.Write(this.EndTime);
            writer.Write(this.Rate);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            list.AddRange(base.GetReferences());
            if (Animation != null) list.Add(Animation);
            return list.ToArray();
        }

        public float GetPlaybackTime(double currentTime)
        {
            double scaledTime = currentTime * Rate;
            double duration = EndTime - StartTime;
            double curpos = scaledTime % duration;
            return StartTime + (float)curpos;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipAnimationList : ClipBase
    {
        public override long BlockLength
        {
            get { return 112; }
        }

        // structure data
        public ulong AnimationsPointer { get; set; }
        public ushort AnimationsCount1 { get; set; }
        public ushort AnimationsCount2 { get; set; }
        public uint Unknown_5Ch { get; set; } // 0x00000000
        public float Duration { get; set; }
        public uint Unknown_64h { get; set; } // 0x00000001
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000

        // reference data
        public ResourceSimpleArray<ClipAnimationsEntry> Animations { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);
            this.AnimationsPointer = reader.ReadUInt64();
            this.AnimationsCount1 = reader.ReadUInt16();
            this.AnimationsCount2 = reader.ReadUInt16();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.Duration = reader.ReadSingle();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();

            this.Animations = reader.ReadBlockAt<ResourceSimpleArray<ClipAnimationsEntry>>(
                this.AnimationsPointer, // offset
                this.AnimationsCount1
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            this.AnimationsPointer = (ulong)(this.Animations != null ? this.Animations.FilePosition : 0);
            //this.p4 = (ulong)(this.p4data != null ? this.p4data.Position : 0);
            //this.c1 = (ushort)(this.p4data != null ? this.p4data.Count : 0);

            writer.Write(this.AnimationsPointer);
            writer.Write(this.AnimationsCount1);
            writer.Write(this.AnimationsCount2);
            writer.Write(this.Unknown_5Ch);
            writer.Write(this.Duration);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            list.AddRange(base.GetReferences());
            if (Animations != null) list.Add(Animations);
            return list.ToArray();
        }


        public float GetPlaybackTime(double currentTime)
        {
            double scaledTime = currentTime;// * Rate;
            double duration = Duration;// EndTime - StartTime;
            double curpos = scaledTime % duration;
            return /*StartTime +*/ (float)curpos;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipAnimationsEntry : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 24; }
        }

        // structure data
        public float StartTime { get; set; }
        public float EndTime { get; set; }
        public float Rate { get; set; }
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public ulong AnimationPointer { get; set; }

        // reference data
        public Animation Animation { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.StartTime = reader.ReadSingle();
            this.EndTime = reader.ReadSingle();
            this.Rate = reader.ReadSingle();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.AnimationPointer = reader.ReadUInt64();

            // read reference data
            this.Animation = reader.ReadBlockAt<Animation>(
                this.AnimationPointer // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.AnimationPointer = (ulong)(this.Animation != null ? this.Animation.FilePosition : 0);

            // write structure data
            writer.Write(this.StartTime);
            writer.Write(this.EndTime);
            writer.Write(this.Rate);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.AnimationPointer);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Animation != null) list.Add(Animation);
            return list.ToArray();
        }


        public float GetPlaybackTime(double currentTime)
        {
            double scaledTime = currentTime * Rate;
            double duration = EndTime - StartTime;
            double curpos = scaledTime % duration;
            return StartTime + (float)curpos;
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipPropertyMap : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong PropertyEntriesPointer { get; set; }
        public ushort PropertyEntriesCount { get; set; }
        public ushort PropertyEntriesCapacity { get; set; }
        public uint Unknown_0Ch { get; set; } // 0x01000000

        // reference data
        public ResourcePointerArray64<ClipPropertyMapEntry> Properties { get; set; }

        public ClipProperty[] AllProperties { get; set; }
        public Dictionary<MetaHash, ClipProperty> PropertyMap { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.PropertyEntriesPointer = reader.ReadUInt64();
            this.PropertyEntriesCount = reader.ReadUInt16();
            this.PropertyEntriesCapacity = reader.ReadUInt16();
            this.Unknown_0Ch = reader.ReadUInt32();

            // read reference data
            this.Properties = reader.ReadBlockAt<ResourcePointerArray64<ClipPropertyMapEntry>>(
                this.PropertyEntriesPointer, // offset
                this.PropertyEntriesCount
            );



            if ((Properties != null) && (Properties.data_items != null))
            {
                List<ClipProperty> pl = new List<ClipProperty>();
                foreach (var pme in Properties.data_items)
                {
                    ClipPropertyMapEntry cpme = pme;
                    while (cpme?.Data != null)
                    {
                        pl.Add(cpme.Data);
                        cpme = cpme.Next;
                    }
                }
                AllProperties = pl.ToArray();

                PropertyMap = new Dictionary<MetaHash, ClipProperty>();
                foreach (var cp in AllProperties)
                {
                    PropertyMap[cp.NameHash] = cp;
                }
            }

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.PropertyEntriesPointer = (ulong)(this.Properties != null ? this.Properties.FilePosition : 0);
            //this.c1 = (ushort)(this.p1data != null ? this.p1data.Count : 0);

            // write structure data
            writer.Write(this.PropertyEntriesPointer);
            writer.Write(this.PropertyEntriesCount);
            writer.Write(this.PropertyEntriesCapacity);
            writer.Write(this.Unknown_0Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Properties != null) list.Add(Properties);
            return list.ToArray();
        }

        public override string ToString()
        {
            return "Count: " + (AllProperties?.Length ?? 0).ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipPropertyMapEntry : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32; }
        }

        // structure data
        public MetaHash PropertyNameHash { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000000
        public ulong DataPointer { get; set; }
        public ulong NextPointer { get; set; }
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000

        // reference data
        public ClipProperty Data { get; set; }
        public ClipPropertyMapEntry Next { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.PropertyNameHash = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.DataPointer = reader.ReadUInt64();
            this.NextPointer = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();

            // read reference data
            this.Data = reader.ReadBlockAt<ClipProperty>(
                this.DataPointer // offset
            );
            this.Next = reader.ReadBlockAt<ClipPropertyMapEntry>(
                this.NextPointer // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.DataPointer = (ulong)(this.Data != null ? this.Data.FilePosition : 0);
            this.NextPointer = (ulong)(this.Next != null ? this.Next.FilePosition : 0);

            // write structure data
            writer.Write(this.PropertyNameHash);
            writer.Write(this.Unknown_04h);
            writer.Write(this.DataPointer);
            writer.Write(this.NextPointer);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Data != null) list.Add(Data);
            if (Next != null) list.Add(Next);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipProperty : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public MetaHash NameHash { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ulong AttributesPointer { get; set; }
        public ushort AttributesCount { get; set; }
        public ushort AttributesCapacity { get; set; }
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public MetaHash UnkHash { get; set; }
        public uint Unknown_3Ch { get; set; } // 0x00000000

        // reference data
        public ResourcePointerArray64<ClipPropertyAttribute> Attributes { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.NameHash = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.AttributesPointer = reader.ReadUInt64();
            this.AttributesCount = reader.ReadUInt16();
            this.AttributesCapacity = reader.ReadUInt16();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.UnkHash = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            this.Attributes = reader.ReadBlockAt<ResourcePointerArray64<ClipPropertyAttribute>>(
                this.AttributesPointer, // offset
                this.AttributesCount
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.AttributesPointer = (ulong)(this.Attributes != null ? this.Attributes.FilePosition : 0);
            //this.c1 = (ushort)(this.p1data != null ? this.p1data.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.NameHash);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.AttributesPointer);
            writer.Write(this.AttributesCount);
            writer.Write(this.AttributesCapacity);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.UnkHash);
            writer.Write(this.Unknown_3Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Attributes != null) list.Add(Attributes);
            return list.ToArray();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if ((Attributes != null) && (Attributes.data_items != null))
            {
                foreach (var item in Attributes.data_items)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(item.ToString());
                }
            }
            return NameHash.ToString() + ": " + UnkHash.ToString() + ": " + sb.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipPropertyAttribute : ResourceSystemBlock, IResourceXXSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public byte Type { get; set; }
        public byte Unknown_09h { get; set; } // 0x00
        public ushort Unknown_Ah { get; set; } // 0x0000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public MetaHash NameHash { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Type = reader.ReadByte();
            this.Unknown_09h = reader.ReadByte();
            this.Unknown_Ah = reader.ReadUInt16();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.NameHash = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Type);
            writer.Write(this.Unknown_09h);
            writer.Write(this.Unknown_Ah);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.NameHash);
            writer.Write(this.Unknown_1Ch);
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 8;
            var type = reader.ReadByte();
            reader.Position -= 9;

            switch (type)
            {
                case 1: return new ClipPropertyAttributeFloat();
                case 2: return new ClipPropertyAttributeInt();
                case 3: return new ClipPropertyAttributeBool();
                case 4: return new ClipPropertyAttributeString();
                case 6: return new ClipPropertyAttributeVector3();
                case 8: return new ClipPropertyAttributeVector4();
                case 12: return new ClipPropertyAttributeHashString();
                default: return null;// throw new Exception("Unknown type");
            }
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipPropertyAttributeFloat : ClipPropertyAttribute
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        public float Value { get; set; }
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Value = reader.ReadSingle();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Value);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }

        public override string ToString()
        {
            return "Float:" + FloatUtil.ToString(Value);
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipPropertyAttributeInt : ClipPropertyAttribute
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        public int Value { get; set; }
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Value = reader.ReadInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Value);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }

        public override string ToString()
        {
            return "Int:" + Value.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipPropertyAttributeBool : ClipPropertyAttribute
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        public uint Value { get; set; }
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Value = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Value);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }

        public override string ToString()
        {
            return "Uint:" + Value.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipPropertyAttributeString : ClipPropertyAttribute
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        public ulong ValuePointer { get; set; }
        public ushort ValueLength1 { get; set; }
        public ushort ValueLength2 { get; set; }
        public uint Unknown_02Ch { get; set; } // 0x00000000

        public string Value;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.ValuePointer = reader.ReadUInt64();
            this.ValueLength1 = reader.ReadUInt16();
            this.ValueLength2 = reader.ReadUInt16();
            this.Unknown_02Ch = reader.ReadUInt32();

            //// read reference data
            //this.Value = reader.ReadBlockAt<string_r>(
            //    this.ValuePointer // offset
            //);
            Value = reader.ReadStringAt(ValuePointer);
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            //this.ValuePointer = (ulong)(this.Value != null ? this.Value.Position : 0);
            //this.ValueLength1 = (ushort)(this.Value != null ? this.Value.Value.Length : 0);
            //this.ValueLength2 = (ushort)(this.Value != null ? this.Value.Value.Length + 1 : 0);

            // write structure data
            writer.Write(this.ValuePointer);
            writer.Write(this.ValueLength1);
            writer.Write(this.ValueLength2);
            writer.Write(this.Unknown_02Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            //if (p1data != null) list.Add(p1data);
            return list.ToArray();
        }

        public override string ToString()
        {
            return "String:" + Value;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipPropertyAttributeVector3 : ClipPropertyAttribute
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Unknown_02Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.X = reader.ReadSingle();
            this.Y = reader.ReadSingle();
            this.Z = reader.ReadSingle();
            this.Unknown_02Ch = reader.ReadSingle();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data          
            writer.Write(this.X);
            writer.Write(this.Y);
            writer.Write(this.Z);
            writer.Write(this.Unknown_02Ch);
        }

        public override string ToString()
        {
            return "Vector3:" + FloatUtil.GetVector3String(new Vector3(X, Y, Z));
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipPropertyAttributeVector4 : ClipPropertyAttribute
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.X = reader.ReadSingle();
            this.Y = reader.ReadSingle();
            this.Z = reader.ReadSingle();
            this.W = reader.ReadSingle();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.X);
            writer.Write(this.Y);
            writer.Write(this.Z);
            writer.Write(this.W);
        }

        public override string ToString()
        {
            return "Vector4:" + FloatUtil.GetVector4String(new Vector4(X, Y, Z, W));
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipPropertyAttributeHashString : ClipPropertyAttribute
    {
        public override long BlockLength => 0x30;

        public MetaHash Value { get; set; }
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Value = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Value);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }

        public override string ToString()
        {
            return "Hash:" + Value.ToString();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipTagList : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32; }
        }

        // structure data
        public ulong TagsPointer { get; set; }
        public ushort TagCount1 { get; set; }
        public ushort TagCount2 { get; set; }
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000

        // reference data
        public ResourcePointerArray64<ClipTag> Tags { get; set; }

        public ClipTag[] AllTags { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.TagsPointer = reader.ReadUInt64();
            this.TagCount1 = reader.ReadUInt16();
            this.TagCount2 = reader.ReadUInt16();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();

            // read reference data
            this.Tags = reader.ReadBlockAt<ResourcePointerArray64<ClipTag>>(
                this.TagsPointer, // offset
                this.TagCount1
            );

            if ((Tags != null) && (Tags.data_items != null))
            {
                List<ClipTag> tl = new List<ClipTag>();
                foreach (var te in Tags.data_items)
                {
                    if (te != null)
                    {
                        tl.Add(te);
                    }
                }
                AllTags = tl.ToArray();
            }
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.TagsPointer = (ulong)(this.Tags != null ? this.Tags.FilePosition : 0);
            //this.c1 = (ushort)(this.p1data != null ? this.p1data.Count : 0);

            // write structure data
            writer.Write(this.TagsPointer);
            writer.Write(this.TagCount1);
            writer.Write(this.TagCount2);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Tags != null) list.Add(Tags);
            return list.ToArray();
        }

        public override string ToString()
        {
            return "Count: " + (AllTags?.Length ?? 0).ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipTag : ClipProperty
    {
        public override long BlockLength
        {
            get { return 80; }
        }

        public MetaHash Unknown_40h { get; set; }
        public MetaHash Unknown_44h { get; set; }
        public ulong TagsPointer { get; set; }

        // reference data
        public ClipTagList Tags { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.TagsPointer = reader.ReadUInt64();

            // read reference data
            this.Tags = reader.ReadBlockAt<ClipTagList>(
                this.TagsPointer // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.TagsPointer = (ulong)(this.Tags != null ? this.Tags.FilePosition : 0);

            // write structure data         
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.TagsPointer);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Tags != null) list.Add(Tags);
            return list.ToArray();
        }

        public override string ToString()
        {
            return base.ToString() + ": " + Unknown_40h.ToString() + ", " + Unknown_44h.ToString();
        }
    }


}