using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
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

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
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

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
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
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
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

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
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

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.AnimationsPointer = (ulong)(this.Animations != null ? this.Animations.FilePosition : 0);
            //this.c1 = (ushort)(this.Anims != null ? this.Anims.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.AnimationsPointer);
            writer.Write(this.AnimationsMapCapacity);
            writer.Write(this.AnimationsMapEntries);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
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
        public uint Unknown_4h { get; set; } // 0x00000000
        public ulong AnimationPtr { get; set; }
        public ulong NextEntryPtr { get; set; }
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000

        // reference data
        public Animation Animation { get; set; }
        public AnimationMapEntry NextEntry { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Hash = new MetaHash(reader.ReadUInt32());
            this.Unknown_4h = reader.ReadUInt32();
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

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.AnimationPtr = (ulong)(this.Animation != null ? this.Animation.FilePosition : 0);
            this.NextEntryPtr = (ulong)(this.NextEntry != null ? this.NextEntry.FilePosition : 0);

            // write structure data
            writer.Write(this.Hash);
            writer.Write(this.Unknown_4h);
            writer.Write(this.AnimationPtr);
            writer.Write(this.NextEntryPtr);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
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
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public ushort Unknown_10h { get; set; }
        public ushort Unknown_12h { get; set; }
        public ushort Unknown_14h { get; set; }
        public ushort Unknown_16h { get; set; }
        public float Unknown_18h { get; set; }
        public byte Unknown_1Ch { get; set; }
        public byte Unknown_1Dh { get; set; }
        public byte Unknown_1Eh { get; set; }
        public byte Unknown_1Fh { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; }
        public uint Unknown_3Ch { get; set; }
        public ResourcePointerList64<Sequence> Sequences { get; set; }
        //public ResourceSimpleList64<uint_r> Unknown_50h { get; set; }
        public ResourceSimpleList64Ptr BoneIdsPtr { get; set; }
        public AnimationBoneId[] BoneIds { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();  //1     1       1       1
            this.Unknown_8h = reader.ReadUInt32();  //0     0       0       0
            this.Unknown_Ch = reader.ReadUInt32();  //0     0       0       0
            this.Unknown_10h = reader.ReadUInt16(); //257   257     257     257     flags?
            this.Unknown_12h = reader.ReadUInt16(); //0     0       0       0
            this.Unknown_14h = reader.ReadUInt16(); //221   17      151     201     frames
            this.Unknown_16h = reader.ReadUInt16(); //223   31      159     207     sequence limit?
            this.Unknown_18h = reader.ReadSingle(); //7.34  0.53    5.0     6.66    duration
            this.Unknown_1Ch = reader.ReadByte();   //118   0       216     116
            this.Unknown_1Dh = reader.ReadByte();   //152   36      130     182
            this.Unknown_1Eh = reader.ReadByte();   //99    0       66      180
            this.Unknown_1Fh = reader.ReadByte();   //205   107     44      26
            this.Unknown_20h = reader.ReadUInt32(); //0     0       0       0
            this.Unknown_24h = reader.ReadUInt32(); //0     0       0       0
            this.Unknown_28h = reader.ReadUInt32(); //0     0       0       0
            this.Unknown_2Ch = reader.ReadUInt32(); //0     0       0       0
            this.Unknown_30h = reader.ReadUInt32(); //0     0       0       0
            this.Unknown_34h = reader.ReadUInt32(); //0     0       0       0
            this.Unknown_38h = reader.ReadUInt32(); //314   174     1238    390     sequences length?
            this.Unknown_3Ch = reader.ReadUInt32(); //2     2       2       2       material/type?
            this.Sequences = reader.ReadBlock<ResourcePointerList64<Sequence>>();
            //this.Unknown_50h = reader.ReadBlock<ResourceSimpleList64<uint_r>>();
            this.BoneIdsPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            //this.BoneIds = reader.ReadUintsAt(this.BoneIdsPtr.EntriesPointer, this.BoneIdsPtr.EntriesCount);
            this.BoneIds = reader.ReadStructsAt<AnimationBoneId>(this.BoneIdsPtr.EntriesPointer, this.BoneIdsPtr.EntriesCount);
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_12h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_16h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_1Eh);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.WriteBlock(this.Sequences);
            //writer.WriteBlock(this.Unknown_50h);//todo: fix!!
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x40, Sequences),
                //new Tuple<long, IResourceBlock>(0x50, Unknown_50h)//todo: fix!
            };
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public struct AnimationBoneId
    {
        public ushort BoneId { get; set; }
        public byte Unk0 { get; set; }
        public byte Unk1 { get; set; }
        public override string ToString()
        {
            return BoneId.ToString() + ": " + Unk0.ToString() + ", " + Unk1.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Sequence : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32 + Data.Length; }
        }

        // structure data
        public byte Unknown_00h { get; set; }
        public byte Unknown_01h { get; set; }
        public byte Unknown_02h { get; set; }
        public byte Unknown_03h { get; set; }
        public uint DataLength { get; set; }
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; }
        public uint Unknown_10h { get; set; }
        public ushort Unknown_14h { get; set; }
        public ushort Unknown_16h { get; set; }
        //public uint Unknown_18h { get; set; }
        public ushort Unknown_18h { get; set; }
        public ushort Unknown_1Ah { get; set; }
        public ushort Unknown_1Ch { get; set; }
        public ushort Unknown_1Eh { get; set; }
        public byte[] Data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            //this.Unknown_00h = reader.ReadUInt32();//2965995365  2837183178
            this.Unknown_00h = reader.ReadByte();  //101        202        97         120
            this.Unknown_01h = reader.ReadByte();  //127        250        202        168
            this.Unknown_02h = reader.ReadByte();  //201        27         115        126
            this.Unknown_03h = reader.ReadByte();  //176        169        131        74
            this.DataLength = reader.ReadUInt32(); //282        142        1206       358
            this.Unknown_08h = reader.ReadUInt32();//0          0          0          0
            this.Unknown_0Ch = reader.ReadUInt32();//224 (E0)   32 (20)    536 (218)  300     offset in data to?
            this.Unknown_10h = reader.ReadUInt32();//314        174        1238       390 (=Length)
            this.Unknown_14h = reader.ReadUInt16();//0          0          0          0
            this.Unknown_16h = reader.ReadUInt16();//221 (DD)   17 (11)    151 (97)   201
            this.Unknown_18h = reader.ReadUInt16();//0          4          4          0      type?
            this.Unknown_1Ah = reader.ReadUInt16();//0          0          106        0      
            this.Unknown_1Ch = reader.ReadUInt16();//0          17         0          0      bone?
            this.Unknown_1Eh = reader.ReadUInt16();//64         255        255        64


            var pos = reader.Position;

            this.Data = reader.ReadBytes((int)DataLength);

            reader.Position = pos;
            float[] fvals = reader.ReadFloatsAt((ulong)pos, DataLength / 4);
            ushort[] svals = reader.ReadUshortsAt((ulong)pos, DataLength / 2);
            if (fvals != null)
            { }

            //reader.Position = pos;
            //float f0 = reader.ReadSingle();   //     0        0        0        0
            //var v0 = reader.ReadStruct<Vector3>();// 0,0,0    1,1,1    0,0,0    0,0,0
            //float f1 = reader.ReadSingle();   //     0        0        0        0
            //uint u0 = reader.ReadUInt32();    //     0        17       0        0
            //short s1 = reader.ReadInt16();    //     0        112      0        0
            //short s2 = reader.ReadInt16();    //     0        14048    0        0
            ////float f2 = reader.ReadSingle();   //     0   6.675771E-06  0
            //float f3 = reader.ReadSingle();   //     0       -0.875    0
            //short u2 = reader.ReadInt16();    //     0       -1
            //short u3 = reader.ReadInt16();    //     0       1
            //short u4 = reader.ReadInt16();    //     0       -1
            //short u5 = reader.ReadInt16();    //     0       1
            //short u6 = reader.ReadInt16();    //     0       -18725
            //short u7 = reader.ReadInt16();    //     0       1
            //short u8 = reader.ReadInt16();    //     23      -18725   
            //short u9 = reader.ReadInt16();    //     0       1
            //short uA = reader.ReadInt16();    //     2825    28086
            //short uB = reader.ReadInt16();    //     0       1



        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_00h);
            writer.Write(this.DataLength);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_1Eh);
            writer.Write(this.Data);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipMapEntry : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32; }
        }

        // structure data
        public MetaHash Hash { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000000
        public ulong ClipPointer { get; set; }
        public ulong NextPointer { get; set; }
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000

        // reference data
        public ClipBase Clip { get; set; }
        public ClipMapEntry Next { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Hash = new MetaHash(reader.ReadUInt32());
            this.Unknown_4h = reader.ReadUInt32();
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

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.ClipPointer = (ulong)(this.Clip != null ? this.Clip.FilePosition : 0);
            this.NextPointer = (ulong)(this.Next != null ? this.Next.FilePosition : 0);

            // write structure data
            writer.Write(this.Hash);
            writer.Write(this.Unknown_4h);
            writer.Write(this.ClipPointer);
            writer.Write(this.NextPointer);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Clip != null) list.Add(Clip);
            if (Next != null) list.Add(Next);
            return list.ToArray();
        }

        public override string ToString()
        {
            return Hash.ToString();
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
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public ulong NamePointer { get; set; }
        //public uint Unknown_20h { get; set; } // short, short -> name length (+1)
        public ushort Unknown_20h { get; set; } // short, name length
        public ushort Unknown_22h { get; set; } // short, name length +1
        public uint Unknown_24h { get; set; } // 0x00000000
        public ulong Unknown_28hPtr { get; set; } // 0x50000000
        //public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; }
        public uint Unknown_34h { get; set; } // 0x00000000
        public ulong p2 { get; set; }
        public ulong p3 { get; set; }
        public uint Unknown_48h { get; set; } // 0x00000001
        public uint Unknown_4Ch { get; set; } // 0x00000000       

        // reference data
        public string Name { get; set; }
        public Unknown_CL_200 p2data { get; set; }
        public Unknown_CL_001 p3data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            //this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt16();
            this.Unknown_22h = reader.ReadUInt16();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28hPtr = reader.ReadUInt64();
            //this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.p2 = reader.ReadUInt64();
            this.p3 = reader.ReadUInt64();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();

            // read reference data
            //this.Name = reader.ReadBlockAt<string_r>(
            //    this.NamePointer // offset
            //);
            this.Name = reader.ReadStringAt(this.NamePointer);
            this.p2data = reader.ReadBlockAt<Unknown_CL_200>(
                this.p2 // offset
            );
            this.p3data = reader.ReadBlockAt<Unknown_CL_001>(
                this.p3 // offset
            );

            if ((Unknown_28hPtr != 0) && (Unknown_28hPtr != 0x50000000))
            {
            }

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            //this.NamePointer = (ulong)(this.Name != null ? this.Name.Position : 0);
            this.p2 = (ulong)(this.p2data != null ? this.p2data.FilePosition : 0);
            this.p3 = (ulong)(this.p3data != null ? this.p3data.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28hPtr);
            //writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.p2);
            writer.Write(this.p3);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            //if (Name != null) list.Add(Name);
            if (p2data != null) list.Add(p2data);
            if (p3data != null) list.Add(p3data);
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
                case 2: return new ClipAnimations();
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
        public float Unknown_58h { get; set; }
        public float Unknown_5Ch { get; set; }
        public float Unknown_60h { get; set; }
        public uint Unknown_64h { get; set; } // 0x00000000
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000

        // reference data
        public Animation Animation { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);
            this.AnimationPointer = reader.ReadUInt64();
            this.Unknown_58h = reader.ReadSingle();
            this.Unknown_5Ch = reader.ReadSingle();
            this.Unknown_60h = reader.ReadSingle();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();

            this.Animation = reader.ReadBlockAt<Animation>(
                this.AnimationPointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            this.AnimationPointer = (ulong)(this.Animation != null ? this.Animation.FilePosition : 0);

            writer.Write(this.AnimationPointer);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            list.AddRange(base.GetReferences());
            if (Animation != null) list.Add(Animation);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipAnimations : ClipBase
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
        public uint Unknown_60h { get; set; }
        public uint Unknown_64h { get; set; } // 0x00000001
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000

        // reference data
        public ResourceSimpleArray<ClipAnimationsEntry> Animations { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);
            this.AnimationsPointer = reader.ReadUInt64();
            this.AnimationsCount1 = reader.ReadUInt16();
            this.AnimationsCount2 = reader.ReadUInt16();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.Unknown_60h = reader.ReadUInt32();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();

            this.Animations = reader.ReadBlockAt<ResourceSimpleArray<ClipAnimationsEntry>>(
                this.AnimationsPointer, // offset
                this.AnimationsCount1
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
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
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            list.AddRange(base.GetReferences());
            if (Animations != null) list.Add(Animations);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClipAnimationsEntry : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 24; }
        }

        // structure data
        public float Unknown_0h { get; set; }
        public float Unknown_4h { get; set; }
        public float Unknown_8h { get; set; }
        public uint Unknown_Ch { get; set; } // 0x00000000
        public ulong AnimationPointer { get; set; }

        // reference data
        public Animation Animation { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadSingle();
            this.Unknown_4h = reader.ReadSingle();
            this.Unknown_8h = reader.ReadSingle();
            this.Unknown_Ch = reader.ReadUInt32();
            this.AnimationPointer = reader.ReadUInt64();

            // read reference data
            this.Animation = reader.ReadBlockAt<Animation>(
                this.AnimationPointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.AnimationPointer = (ulong)(this.Animation != null ? this.Animation.FilePosition : 0);

            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.AnimationPointer);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Animation != null) list.Add(Animation);
            return list.ToArray();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_001 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong p1 { get; set; }
        public ushort c1 { get; set; }
        public ushort c2 { get; set; }
        public uint Unknown_Ch { get; set; } // 0x01000000

        // reference data
        public ResourcePointerArray64<Unknown_CL_002> p1data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.p1 = reader.ReadUInt64();
            this.c1 = reader.ReadUInt16();
            this.c2 = reader.ReadUInt16();
            this.Unknown_Ch = reader.ReadUInt32();

            // read reference data
            this.p1data = reader.ReadBlockAt<ResourcePointerArray64<Unknown_CL_002>>(
                this.p1, // offset
                this.c1
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.p1 = (ulong)(this.p1data != null ? this.p1data.FilePosition : 0);
            //this.c1 = (ushort)(this.p1data != null ? this.p1data.Count : 0);

            // write structure data
            writer.Write(this.p1);
            writer.Write(this.c1);
            writer.Write(this.c2);
            writer.Write(this.Unknown_Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (p1data != null) list.Add(p1data);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_002 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32; }
        }

        // structure data
        public uint Unknown_0h { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000000
        public ulong DataPointer { get; set; }
        public ulong NextPointer { get; set; }
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000

        // reference data
        public Unknown_CL_003 Data { get; set; }
        public Unknown_CL_002 Next { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.DataPointer = reader.ReadUInt64();
            this.NextPointer = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();

            // read reference data
            this.Data = reader.ReadBlockAt<Unknown_CL_003>(
                this.DataPointer // offset
            );
            this.Next = reader.ReadBlockAt<Unknown_CL_002>(
                this.NextPointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.DataPointer = (ulong)(this.Data != null ? this.Data.FilePosition : 0);
            this.NextPointer = (ulong)(this.Next != null ? this.Next.FilePosition : 0);

            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.DataPointer);
            writer.Write(this.NextPointer);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Data != null) list.Add(Data);
            if (Next != null) list.Add(Next);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_003 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ulong p1 { get; set; }
        public ushort c1 { get; set; }
        public ushort c2 { get; set; }
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; }
        public uint Unknown_3Ch { get; set; } // 0x00000000

        // reference data
        public ResourcePointerArray64<Unknown_CL_004> p1data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.p1 = reader.ReadUInt64();
            this.c1 = reader.ReadUInt16();
            this.c2 = reader.ReadUInt16();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            this.p1data = reader.ReadBlockAt<ResourcePointerArray64<Unknown_CL_004>>(
                this.p1, // offset
                this.c1
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.p1 = (ulong)(this.p1data != null ? this.p1data.FilePosition : 0);
            //this.c1 = (ushort)(this.p1data != null ? this.p1data.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.p1);
            writer.Write(this.c1);
            writer.Write(this.c2);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (p1data != null) list.Add(p1data);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_004 : ResourceSystemBlock, IResourceXXSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public byte type { get; set; }
        public byte Unknown_9h { get; set; }
        public ushort Unknown_Ah { get; set; }
        public uint Unknown_Ch { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.type = reader.ReadByte();
            this.Unknown_9h = reader.ReadByte();
            this.Unknown_Ah = reader.ReadUInt16();
            this.Unknown_Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.type);
            writer.Write(this.Unknown_9h);
            writer.Write(this.Unknown_Ah);
            writer.Write(this.Unknown_Ch);
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 8;
            var type = reader.ReadByte();
            reader.Position -= 9;

            switch (type)
            {
                case 1: return new Unknown_CL_004_type1();
                case 2: return new Unknown_CL_004_type2();
                case 3: return new Unknown_CL_004_type3();
                case 4: return new Unknown_CL_004_type4();
                case 6: return new Unknown_CL_004_type6();
                case 8: return new Unknown_CL_004_type8();
                default: return null;// throw new Exception("Unknown type");
            }
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_004_type1 : Unknown_CL_004
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_004_type2 : Unknown_CL_004
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_004_type3 : Unknown_CL_004
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x6D7255E7
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_004_type4 : Unknown_CL_004
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ulong p1 { get; set; }
        public ushort c1 { get; set; }
        public ushort c2 { get; set; }
        public uint Unknown_2Ch { get; set; } // 0x00000000

        // reference data
        //public ResourceSimpleArray<byte_r> p1data { get; set; } // string (byte array...)
        public byte[] p1data { get; set; } // string (byte array...)

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.p1 = reader.ReadUInt64();
            this.c1 = reader.ReadUInt16();
            this.c2 = reader.ReadUInt16();
            this.Unknown_2Ch = reader.ReadUInt32();

            // read reference data
            //this.p1data = reader.ReadBlockAt<ResourceSimpleArray<byte_r>>(
            //    this.p1, // offset
            //    this.c2
            //);
            this.p1data = reader.ReadBytesAt(this.p1, this.c2);
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            //this.p1 = (ulong)(this.p1data != null ? this.p1data.Position : 0);//todo:fix!~
            ////this.c2 = (ushort)(this.p1data != null ? this.p1data.Count : 0);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.p1);
            writer.Write(this.c1);
            writer.Write(this.c2);
            writer.Write(this.Unknown_2Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            //if (p1data != null) list.Add(p1data);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_004_type6 : Unknown_CL_004
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; }
        public uint Unknown_2Ch { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_004_type8 : Unknown_CL_004
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x443D96E7
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; }
        public uint Unknown_2Ch { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_200 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32; }
        }

        // structure data
        public ulong p1 { get; set; }
        public ushort c1 { get; set; }
        public ushort c2 { get; set; }
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000

        // reference data
        public ResourcePointerArray64<Unknown_CL_201> p1data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.p1 = reader.ReadUInt64();
            this.c1 = reader.ReadUInt16();
            this.c2 = reader.ReadUInt16();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();

            // read reference data
            this.p1data = reader.ReadBlockAt<ResourcePointerArray64<Unknown_CL_201>>(
                this.p1, // offset
                this.c1
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.p1 = (ulong)(this.p1data != null ? this.p1data.FilePosition : 0);
            //this.c1 = (ushort)(this.p1data != null ? this.p1data.Count : 0);

            // write structure data
            writer.Write(this.p1);
            writer.Write(this.c1);
            writer.Write(this.c2);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (p1data != null) list.Add(p1data);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_201 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 80; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ulong p0 { get; set; }
        public ushort c1 { get; set; }
        public ushort c2 { get; set; }
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; }
        public uint Unknown_3Ch { get; set; } // 0x00000000
        public uint Unknown_40h { get; set; }
        public uint Unknown_44h { get; set; }
        public ulong p1 { get; set; }

        // reference data
        public ResourcePointerArray64<Unknown_CL_202> p0data { get; set; }
        public Unknown_CL_200 p1data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.p0 = reader.ReadUInt64();
            this.c1 = reader.ReadUInt16();
            this.c2 = reader.ReadUInt16();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.p1 = reader.ReadUInt64();

            // read reference data
            this.p0data = reader.ReadBlockAt<ResourcePointerArray64<Unknown_CL_202>>(
                this.p0, // offset
                this.c1
            );
            this.p1data = reader.ReadBlockAt<Unknown_CL_200>(
                this.p1 // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.p0 = (ulong)(this.p0data != null ? this.p0data.FilePosition : 0);
            //this.c1 = (ushort)(this.p0data != null ? this.p0data.Count : 0);
            this.p1 = (ulong)(this.p1data != null ? this.p1data.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.p0);
            writer.Write(this.c1);
            writer.Write(this.c2);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.p1);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (p0data != null) list.Add(p0data);
            if (p1data != null) list.Add(p1data);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_CL_202 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; }
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; }
        public uint Unknown_2Ch { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }
    }



}
