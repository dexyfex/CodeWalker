using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
    Copyright(c) 2017 Neodymium
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

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClothDictionary : ResourceFileBase
    {
        // pgBase
        // pgDictionaryBase
        // pgDictionary<characterCloth>
        public override long BlockLength => 0x40;

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000001
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ResourceSimpleList64_s<MetaHash> ClothNameHashes { get; set; }
        public ResourcePointerList64<CharacterCloth> Clothes { get; set; }

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
            this.ClothNameHashes = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            this.Clothes = reader.ReadBlock<ResourcePointerList64<CharacterCloth>>();
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
            writer.WriteBlock(this.ClothNameHashes);
            writer.WriteBlock(this.Clothes);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, ClothNameHashes),
                new Tuple<long, IResourceBlock>(0x30, Clothes)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClothController : ResourceSystemBlock
    {
        // clothController
        public override long BlockLength => 0x80;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public ulong BridgeSimGfxPointer { get; set; }
        public ulong MorphControllerPointer { get; set; }
        public ulong VerletCloth1Pointer { get; set; }
        public ulong VerletCloth2Pointer { get; set; }
        public ulong VerletCloth3Pointer { get; set; }
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000
        public uint Unknown_40h { get; set; } // 0x00000000
        public uint Unknown_44h { get; set; } // 0x00000000
        public uint Unknown_48h { get; set; } // 0x00000000
        public uint Unknown_4Ch { get; set; } // 0x00000000
        public uint Type { get; set; }
        public uint Unknown_54h { get; set; } // 0x00000000
        public PsoChar32 Name { get; set; }
        public float Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000

        // reference data
        public ClothBridgeSimGfx BridgeSimGfx { get; set; }
        public MorphController MorphController { get; set; }
        public VerletCloth VerletCloth1 { get; set; }
        public VerletCloth VerletCloth2 { get; set; }
        public VerletCloth VerletCloth3 { get; set; }

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
            this.BridgeSimGfxPointer = reader.ReadUInt64();
            this.MorphControllerPointer = reader.ReadUInt64();
            this.VerletCloth1Pointer = reader.ReadUInt64();
            this.VerletCloth2Pointer = reader.ReadUInt64();
            this.VerletCloth3Pointer = reader.ReadUInt64();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Type = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.Name = reader.ReadStruct<PsoChar32>();
            this.Unknown_78h = reader.ReadSingle();
            this.Unknown_7Ch = reader.ReadUInt32();

            // read reference data
            this.BridgeSimGfx = reader.ReadBlockAt<ClothBridgeSimGfx>(
                this.BridgeSimGfxPointer // offset
            );
            this.MorphController = reader.ReadBlockAt<MorphController>(
                this.MorphControllerPointer // offset
            );
            this.VerletCloth1 = reader.ReadBlockAt<VerletCloth>(
                this.VerletCloth1Pointer // offset
            );
            this.VerletCloth2 = reader.ReadBlockAt<VerletCloth>(
                this.VerletCloth2Pointer // offset
            );
            this.VerletCloth3 = reader.ReadBlockAt<VerletCloth>(
                this.VerletCloth3Pointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.BridgeSimGfxPointer = (ulong)(this.BridgeSimGfx != null ? this.BridgeSimGfx.FilePosition : 0);
            this.MorphControllerPointer = (ulong)(this.MorphController != null ? this.MorphController.FilePosition : 0);
            this.VerletCloth1Pointer = (ulong)(this.VerletCloth1 != null ? this.VerletCloth1.FilePosition : 0);
            this.VerletCloth2Pointer = (ulong)(this.VerletCloth2 != null ? this.VerletCloth2.FilePosition : 0);
            this.VerletCloth3Pointer = (ulong)(this.VerletCloth3 != null ? this.VerletCloth3.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.BridgeSimGfxPointer);
            writer.Write(this.MorphControllerPointer);
            writer.Write(this.VerletCloth1Pointer);
            writer.Write(this.VerletCloth2Pointer);
            writer.Write(this.VerletCloth3Pointer);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Type);
            writer.Write(this.Unknown_54h);
            writer.WriteStruct(this.Name);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_7Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (BridgeSimGfx != null) list.Add(BridgeSimGfx);
            if (MorphController != null) list.Add(MorphController);
            if (VerletCloth1 != null) list.Add(VerletCloth1);
            if (VerletCloth2 != null) list.Add(VerletCloth2);
            if (VerletCloth3 != null) list.Add(VerletCloth3);
            return list.ToArray();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClothBridgeSimGfx : ResourceSystemBlock
    {
        // pgBase
        // clothBridgeSimGfx
        public override long BlockLength => 0x140;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint VertexCount { get; set; }
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ResourceSimpleList64_float Unknown_20h { get; set; }
        public ResourceSimpleList64_float Unknown_30h { get; set; }
        public ResourceSimpleList64_float Unknown_40h { get; set; }
        public uint Unknown_50h { get; set; } // 0x00000000
        public uint Unknown_54h { get; set; } // 0x00000000
        public uint Unknown_58h { get; set; } // 0x00000000
        public uint Unknown_5Ch { get; set; } // 0x00000000
        public ResourceSimpleList64_float Unknown_60h { get; set; }
        public ResourceSimpleList64_uint Unknown_70h { get; set; }
        public ResourceSimpleList64_uint Unknown_80h { get; set; }
        public uint Unknown_90h { get; set; } // 0x00000000
        public uint Unknown_94h { get; set; } // 0x00000000
        public uint Unknown_98h { get; set; } // 0x00000000
        public uint Unknown_9Ch { get; set; } // 0x00000000
        public ResourceSimpleList64_float Unknown_A0h { get; set; }
        public ResourceSimpleList64_uint Unknown_B0h { get; set; }
        public ResourceSimpleList64_uint Unknown_C0h { get; set; }
        public uint Unknown_D0h { get; set; } // 0x00000000
        public uint Unknown_D4h { get; set; } // 0x00000000
        public uint Unknown_D8h { get; set; } // 0x00000000
        public uint Unknown_DCh { get; set; } // 0x00000000
        public ResourceSimpleList64_ushort Unknown_E0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_F0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_100h { get; set; }
        public uint Unknown_110h { get; set; } // 0x00000000
        public uint Unknown_114h { get; set; } // 0x00000000
        public uint Unknown_118h { get; set; } // 0x00000000
        public uint Unknown_11Ch { get; set; } // 0x00000000
        public uint Unknown_120h { get; set; } // 0x00000000
        public uint Unknown_124h { get; set; } // 0x00000000
        public ResourceSimpleList64_uint Unknown_128h { get; set; }
        public uint Unknown_138h { get; set; } // 0x00000000
        public uint Unknown_13Ch { get; set; } // 0x00000000

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
            this.VertexCount = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadBlock<ResourceSimpleList64_float>();
            this.Unknown_30h = reader.ReadBlock<ResourceSimpleList64_float>();
            this.Unknown_40h = reader.ReadBlock<ResourceSimpleList64_float>();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.Unknown_58h = reader.ReadUInt32();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.Unknown_60h = reader.ReadBlock<ResourceSimpleList64_float>();
            this.Unknown_70h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_80h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_90h = reader.ReadUInt32();
            this.Unknown_94h = reader.ReadUInt32();
            this.Unknown_98h = reader.ReadUInt32();
            this.Unknown_9Ch = reader.ReadUInt32();
            this.Unknown_A0h = reader.ReadBlock<ResourceSimpleList64_float>();
            this.Unknown_B0h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_C0h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_D0h = reader.ReadUInt32();
            this.Unknown_D4h = reader.ReadUInt32();
            this.Unknown_D8h = reader.ReadUInt32();
            this.Unknown_DCh = reader.ReadUInt32();
            this.Unknown_E0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_F0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_100h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_110h = reader.ReadUInt32();
            this.Unknown_114h = reader.ReadUInt32();
            this.Unknown_118h = reader.ReadUInt32();
            this.Unknown_11Ch = reader.ReadUInt32();
            this.Unknown_120h = reader.ReadUInt32();
            this.Unknown_124h = reader.ReadUInt32();
            this.Unknown_128h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_138h = reader.ReadUInt32();
            this.Unknown_13Ch = reader.ReadUInt32();
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
            writer.Write(this.VertexCount);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteBlock(this.Unknown_20h);
            writer.WriteBlock(this.Unknown_30h);
            writer.WriteBlock(this.Unknown_40h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
            writer.WriteBlock(this.Unknown_60h);
            writer.WriteBlock(this.Unknown_70h);
            writer.WriteBlock(this.Unknown_80h);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_94h);
            writer.Write(this.Unknown_98h);
            writer.Write(this.Unknown_9Ch);
            writer.WriteBlock(this.Unknown_A0h);
            writer.WriteBlock(this.Unknown_B0h);
            writer.WriteBlock(this.Unknown_C0h);
            writer.Write(this.Unknown_D0h);
            writer.Write(this.Unknown_D4h);
            writer.Write(this.Unknown_D8h);
            writer.Write(this.Unknown_DCh);
            writer.WriteBlock(this.Unknown_E0h);
            writer.WriteBlock(this.Unknown_F0h);
            writer.WriteBlock(this.Unknown_100h);
            writer.Write(this.Unknown_110h);
            writer.Write(this.Unknown_114h);
            writer.Write(this.Unknown_118h);
            writer.Write(this.Unknown_11Ch);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_124h);
            writer.WriteBlock(this.Unknown_128h);
            writer.Write(this.Unknown_138h);
            writer.Write(this.Unknown_13Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, Unknown_20h),
                new Tuple<long, IResourceBlock>(0x30, Unknown_30h),
                new Tuple<long, IResourceBlock>(0x40, Unknown_40h),
                new Tuple<long, IResourceBlock>(0x60, Unknown_60h),
                new Tuple<long, IResourceBlock>(0x70, Unknown_70h),
                new Tuple<long, IResourceBlock>(0x80, Unknown_80h),
                new Tuple<long, IResourceBlock>(0xA0, Unknown_A0h),
                new Tuple<long, IResourceBlock>(0xB0, Unknown_B0h),
                new Tuple<long, IResourceBlock>(0xC0, Unknown_C0h),
                new Tuple<long, IResourceBlock>(0xE0, Unknown_E0h),
                new Tuple<long, IResourceBlock>(0xF0, Unknown_F0h),
                new Tuple<long, IResourceBlock>(0x100, Unknown_100h),
                new Tuple<long, IResourceBlock>(0x128, Unknown_128h)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClothInstanceTuning : ResourceSystemBlock
    {
        // pgBase
        // clothInstanceTuning
        public override long BlockLength => 0x40;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }  // float
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; } // float
        public uint Unknown_2Ch { get; set; }
        public uint Unknown_30h { get; set; } // no float
        public uint Unknown_34h { get; set; } // float
        public uint Unknown_38h { get; set; }
        public uint Unknown_3Ch { get; set; }

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
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
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
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class VerletCloth : ResourceSystemBlock
    {
        // pgBase
        // phVerletCloth
        public override long BlockLength => 0x180;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public ulong BoundPointer { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; }
        public uint Unknown_34h { get; set; }
        public uint Unknown_38h { get; set; }
        public uint Unknown_3Ch { get; set; }
        public uint Unknown_40h { get; set; }
        public uint Unknown_44h { get; set; }
        public uint Unknown_48h { get; set; }
        public uint Unknown_4Ch { get; set; }
        public float Unknown_50h { get; set; }
        public uint Unknown_54h { get; set; } // 0x00000001
        public uint Unknown_58h { get; set; } // 0x00000000
        public uint Unknown_5Ch { get; set; } // 0x00000000
        public uint Unknown_60h { get; set; } // 0x00000000
        public uint Unknown_64h { get; set; } // 0x00000000
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000
        public ResourceSimpleList64_s<Vector4> Unknown_70h { get; set; }
        public ResourceSimpleList64_s<Vector4> Unknown_80h { get; set; }
        public uint Unknown_90h { get; set; } // 0x00000000
        public uint Unknown_94h { get; set; } // 0x00000000
        public uint Unknown_98h { get; set; } // 0x00000000
        public uint Unknown_9Ch { get; set; } // 0x00000000
        public uint Unknown_A0h { get; set; } // 0x00000000
        public uint Unknown_A4h { get; set; } // 0x00000000
        public float Unknown_A8h { get; set; }
        public float Unknown_ACh { get; set; }
        public uint Unknown_B0h { get; set; } // 0x00000000
        public uint Unknown_B4h { get; set; } // 0x00000000
        public uint Unknown_B8h { get; set; } // 0x00000000
        public uint Unknown_BCh { get; set; } // 0x00000000
        public uint Unknown_C0h { get; set; } // 0x00000000
        public uint Unknown_C4h { get; set; } // 0x00000000
        public uint Unknown_C8h { get; set; } // 0x00000000
        public uint Unknown_CCh { get; set; } // 0x00000000
        public uint Unknown_D0h { get; set; } // 0x00000000
        public uint Unknown_D4h { get; set; } // 0x00000000
        public uint Unknown_D8h { get; set; } // 0x00000000
        public uint Unknown_DCh { get; set; } // 0x00000000
        public uint Unknown_E0h { get; set; } // 0x00000000
        public uint Unknown_E4h { get; set; } // 0x00000000
        public uint Unknown_E8h { get; set; }
        public uint Unknown_ECh { get; set; }
        public uint Unknown_F0h { get; set; }
        public uint Unknown_F4h { get; set; } // 0x00000000
        public uint Unknown_F8h { get; set; }
        public uint Unknown_FCh { get; set; } // 0x00000000
        public ResourceSimpleList64<Unknown_C_004> Unknown_100h { get; set; }
        public ResourceSimpleList64<Unknown_C_004> Unknown_110h { get; set; }
        public uint Unknown_120h { get; set; } // 0x00000000
        public uint Unknown_124h { get; set; } // 0x00000000
        public uint Unknown_128h { get; set; } // 0x00000000
        public uint Unknown_12Ch { get; set; } // 0x00000000
        public ulong BehaviorPointer { get; set; }
        public uint Unknown_138h { get; set; } // 0x00100000
        public uint Unknown_13Ch { get; set; } // 0x00000000
        public ulong Unknown_140h_Pointer { get; set; }
        public ushort Unknown_148h { get; set; }
        public ushort Unknown_14Ah { get; set; }
        public uint Unknown_14Ch { get; set; } // 0x00000000
        public uint Unknown_150h { get; set; } // 0x00000000
        public uint Unknown_154h { get; set; } // 0x00000000
        public float Unknown_158h { get; set; }
        public uint Unknown_15Ch { get; set; } // 0x00000000
        public uint Unknown_160h { get; set; } // 0x00000000
        public uint Unknown_164h { get; set; } // 0x00000000
        public uint Unknown_168h { get; set; } // 0x00000000
        public uint Unknown_16Ch { get; set; } // 0x00000000
        public uint Unknown_170h { get; set; } // 0x00000000
        public uint Unknown_174h { get; set; } // 0x00000000
        public uint Unknown_178h { get; set; } // 0x00000000
        public uint Unknown_17Ch { get; set; } // 0x00000000

        // reference data
        public Bounds Bound { get; set; }
        public EnvClothVerletBehavior Behavior { get; set; }
        public Unknown_C_007 Unknown_140h_Data { get; set; }

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
            this.BoundPointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadSingle();
            this.Unknown_54h = reader.ReadUInt32();
            this.Unknown_58h = reader.ReadUInt32();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.Unknown_60h = reader.ReadUInt32();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Unknown_70h = reader.ReadBlock<ResourceSimpleList64_s<Vector4>>();
            this.Unknown_80h = reader.ReadBlock<ResourceSimpleList64_s<Vector4>>();
            this.Unknown_90h = reader.ReadUInt32();
            this.Unknown_94h = reader.ReadUInt32();
            this.Unknown_98h = reader.ReadUInt32();
            this.Unknown_9Ch = reader.ReadUInt32();
            this.Unknown_A0h = reader.ReadUInt32();
            this.Unknown_A4h = reader.ReadUInt32();
            this.Unknown_A8h = reader.ReadSingle();
            this.Unknown_ACh = reader.ReadSingle();
            this.Unknown_B0h = reader.ReadUInt32();
            this.Unknown_B4h = reader.ReadUInt32();
            this.Unknown_B8h = reader.ReadUInt32();
            this.Unknown_BCh = reader.ReadUInt32();
            this.Unknown_C0h = reader.ReadUInt32();
            this.Unknown_C4h = reader.ReadUInt32();
            this.Unknown_C8h = reader.ReadUInt32();
            this.Unknown_CCh = reader.ReadUInt32();
            this.Unknown_D0h = reader.ReadUInt32();
            this.Unknown_D4h = reader.ReadUInt32();
            this.Unknown_D8h = reader.ReadUInt32();
            this.Unknown_DCh = reader.ReadUInt32();
            this.Unknown_E0h = reader.ReadUInt32();
            this.Unknown_E4h = reader.ReadUInt32();
            this.Unknown_E8h = reader.ReadUInt32();
            this.Unknown_ECh = reader.ReadUInt32();
            this.Unknown_F0h = reader.ReadUInt32();
            this.Unknown_F4h = reader.ReadUInt32();
            this.Unknown_F8h = reader.ReadUInt32();
            this.Unknown_FCh = reader.ReadUInt32();
            this.Unknown_100h = reader.ReadBlock<ResourceSimpleList64<Unknown_C_004>>();
            this.Unknown_110h = reader.ReadBlock<ResourceSimpleList64<Unknown_C_004>>();
            this.Unknown_120h = reader.ReadUInt32();
            this.Unknown_124h = reader.ReadUInt32();
            this.Unknown_128h = reader.ReadUInt32();
            this.Unknown_12Ch = reader.ReadUInt32();
            this.BehaviorPointer = reader.ReadUInt64();
            this.Unknown_138h = reader.ReadUInt32();
            this.Unknown_13Ch = reader.ReadUInt32();
            this.Unknown_140h_Pointer = reader.ReadUInt64();
            this.Unknown_148h = reader.ReadUInt16();
            this.Unknown_14Ah = reader.ReadUInt16();
            this.Unknown_14Ch = reader.ReadUInt32();
            this.Unknown_150h = reader.ReadUInt32();
            this.Unknown_154h = reader.ReadUInt32();
            this.Unknown_158h = reader.ReadSingle();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt32();
            this.Unknown_164h = reader.ReadUInt32();
            this.Unknown_168h = reader.ReadUInt32();
            this.Unknown_16Ch = reader.ReadUInt32();
            this.Unknown_170h = reader.ReadUInt32();
            this.Unknown_174h = reader.ReadUInt32();
            this.Unknown_178h = reader.ReadUInt32();
            this.Unknown_17Ch = reader.ReadUInt32();

            // read reference data
            this.Bound = reader.ReadBlockAt<Bounds>(
              this.BoundPointer // offset
            );
            this.Behavior = reader.ReadBlockAt<EnvClothVerletBehavior>(
                this.BehaviorPointer // offset
            );
            this.Unknown_140h_Data = reader.ReadBlockAt<Unknown_C_007>(
              this.Unknown_140h_Pointer // offset
          );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);
            this.BehaviorPointer = (ulong)(this.Behavior != null ? this.Behavior.FilePosition : 0);
            this.Unknown_140h_Pointer = (ulong)(this.Unknown_140h_Data != null ? this.Unknown_140h_Data.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.BoundPointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
            writer.WriteBlock(this.Unknown_70h);
            writer.WriteBlock(this.Unknown_80h);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_94h);
            writer.Write(this.Unknown_98h);
            writer.Write(this.Unknown_9Ch);
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_A4h);
            writer.Write(this.Unknown_A8h);
            writer.Write(this.Unknown_ACh);
            writer.Write(this.Unknown_B0h);
            writer.Write(this.Unknown_B4h);
            writer.Write(this.Unknown_B8h);
            writer.Write(this.Unknown_BCh);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C4h);
            writer.Write(this.Unknown_C8h);
            writer.Write(this.Unknown_CCh);
            writer.Write(this.Unknown_D0h);
            writer.Write(this.Unknown_D4h);
            writer.Write(this.Unknown_D8h);
            writer.Write(this.Unknown_DCh);
            writer.Write(this.Unknown_E0h);
            writer.Write(this.Unknown_E4h);
            writer.Write(this.Unknown_E8h);
            writer.Write(this.Unknown_ECh);
            writer.Write(this.Unknown_F0h);
            writer.Write(this.Unknown_F4h);
            writer.Write(this.Unknown_F8h);
            writer.Write(this.Unknown_FCh);
            writer.WriteBlock(this.Unknown_100h);
            writer.WriteBlock(this.Unknown_110h);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_124h);
            writer.Write(this.Unknown_128h);
            writer.Write(this.Unknown_12Ch);
            writer.Write(this.BehaviorPointer);
            writer.Write(this.Unknown_138h);
            writer.Write(this.Unknown_13Ch);
            writer.Write(this.Unknown_140h_Pointer);
            writer.Write(this.Unknown_148h);
            writer.Write(this.Unknown_14Ah);
            writer.Write(this.Unknown_14Ch);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_154h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_164h);
            writer.Write(this.Unknown_168h);
            writer.Write(this.Unknown_16Ch);
            writer.Write(this.Unknown_170h);
            writer.Write(this.Unknown_174h);
            writer.Write(this.Unknown_178h);
            writer.Write(this.Unknown_17Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Bound != null) list.Add(Bound);
            if (Behavior != null) list.Add(Behavior);
            if (Unknown_140h_Data != null) list.Add(Unknown_140h_Data);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x70, Unknown_70h),
                new Tuple<long, IResourceBlock>(0x80, Unknown_80h),
                new Tuple<long, IResourceBlock>(0x100, Unknown_100h),
                new Tuple<long, IResourceBlock>(0x110, Unknown_110h)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class EnvClothVerletBehavior : ResourceSystemBlock
    {
        // datBase
        // phInstBehavior
        // phClothVerletBehavior
        // phEnvClothVerletBehavior
        public override long BlockLength => 0x40;

        // structure data
        public uint Unknown_0h { get; set; } // 0x00000000
        public uint Unknown_4h { get; set; } // 0x00000000
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
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
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
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
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class EnvironmentCloth : ResourceSystemBlock
    {
        // pgBase
        // clothBase (TODO)
        // environmentCloth
        public override long BlockLength => 0x80;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public ulong InstanceTuningPointer { get; set; }
        public ulong DrawablePointer { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public ulong ControllerPointer { get; set; }
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000
        public uint Unknown_40h { get; set; } // 0x00000000
        public uint Unknown_44h { get; set; } // 0x00000000
        public uint Unknown_48h { get; set; } // 0x00000000
        public uint Unknown_4Ch { get; set; } // 0x00000000
        public uint Unknown_50h { get; set; } // 0x00000000
        public uint Unknown_54h { get; set; } // 0x00000000
        public uint Unknown_58h { get; set; } // 0x00000000
        public uint Unknown_5Ch { get; set; } // 0x00000000
        public ulong pxxxxx_2 { get; set; }
        public ushort cntxx51a { get; set; }
        public ushort cntxx51b { get; set; }
        public uint Unknown_6Ch { get; set; } // 0x00000000
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public uint Unknown_78h { get; set; }
        public uint Unknown_7Ch { get; set; } // 0x00000000

        // reference data
        public ClothInstanceTuning InstanceTuning { get; set; }
        public FragDrawable Drawable { get; set; }
        public ClothController Controller { get; set; }
        public ResourceSimpleArray<uint_r> pxxxxx_2data { get; set; }

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
            this.InstanceTuningPointer = reader.ReadUInt64();
            this.DrawablePointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.ControllerPointer = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.Unknown_58h = reader.ReadUInt32();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.pxxxxx_2 = reader.ReadUInt64();
            this.cntxx51a = reader.ReadUInt16();
            this.cntxx51b = reader.ReadUInt16();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.Unknown_78h = reader.ReadUInt32();
            this.Unknown_7Ch = reader.ReadUInt32();

            // read reference data
            this.InstanceTuning = reader.ReadBlockAt<ClothInstanceTuning>(
                this.InstanceTuningPointer // offset
            );
            this.Drawable = reader.ReadBlockAt<FragDrawable>(
                this.DrawablePointer // offset
            );
            this.Controller = reader.ReadBlockAt<ClothController>(
                this.ControllerPointer // offset
            );
            this.pxxxxx_2data = reader.ReadBlockAt<ResourceSimpleArray<uint_r>>(
                this.pxxxxx_2, // offset
                this.cntxx51a
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.InstanceTuningPointer = (ulong)(this.InstanceTuning != null ? this.InstanceTuning.FilePosition : 0);
            this.DrawablePointer = (ulong)(this.Drawable != null ? this.Drawable.FilePosition : 0);
            this.ControllerPointer = (ulong)(this.Controller != null ? this.Controller.FilePosition : 0);
            this.pxxxxx_2 = (ulong)(this.pxxxxx_2data != null ? this.pxxxxx_2data.FilePosition : 0);
            this.cntxx51a = (ushort)(this.pxxxxx_2data != null ? this.pxxxxx_2data.Count : 0);
            this.cntxx51b = (ushort)(this.pxxxxx_2data != null ? this.pxxxxx_2data.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.InstanceTuningPointer);
            writer.Write(this.DrawablePointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.ControllerPointer);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
            writer.Write(this.pxxxxx_2);
            writer.Write(this.cntxx51a);
            writer.Write(this.cntxx51b);
            writer.Write(this.Unknown_6Ch);
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_7Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (InstanceTuning != null) list.Add(InstanceTuning);
            if (Drawable != null) list.Add(Drawable);
            if (Controller != null) list.Add(Controller);
            if (pxxxxx_2data != null) list.Add(pxxxxx_2data);
            return list.ToArray();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class CharacterCloth : ResourceSystemBlock
    {
        // pgBase
        // clothBase (TODO)
        // characterCloth
        public override long BlockLength => 0xD0;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public ResourceSimpleList64<Unknown_C_001> Unknown_10h { get; set; }
        public ulong ControllerPointer { get; set; }
        public ulong BoundPointer { get; set; }
        public ResourceSimpleList64_uint Unknown_30h { get; set; }
        public uint Unknown_40h { get; set; } // 0x00000000
        public uint Unknown_44h { get; set; } // 0x00000000
        public uint Unknown_48h { get; set; } // 0x00000000
        public uint Unknown_4Ch { get; set; } // 0x00000000
        public Matrix Transform { get; set; }
        public ResourceSimpleList64_uint Unknown_90h { get; set; }
        public uint Unknown_A0h { get; set; } // 0x00000000
        public uint Unknown_A4h { get; set; } // 0x00000000
        public uint Unknown_A8h { get; set; } // 0x00000000
        public uint Unknown_ACh { get; set; } // 0x00000000
        public uint Unknown_B0h { get; set; } // 0x00000000
        public uint Unknown_B4h { get; set; } // 0x00000000
        public uint Unknown_B8h { get; set; } // 0x00000000
        public uint Unknown_BCh { get; set; } // 0x00000000
        public uint Unknown_C0h { get; set; } // 0x00000001
        public uint Unknown_C4h { get; set; } // 0x00000000
        public uint Unknown_C8h { get; set; } // 0x00000000
        public uint Unknown_CCh { get; set; } // 0x00000000

        // reference data
        public CharacterClothController Controller { get; set; }
        public Bounds Bound { get; set; }

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
            this.Unknown_10h = reader.ReadBlock<ResourceSimpleList64<Unknown_C_001>>();
            this.ControllerPointer = reader.ReadUInt64();
            this.BoundPointer = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Transform = reader.ReadMatrix();
            this.Unknown_90h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_A0h = reader.ReadUInt32();
            this.Unknown_A4h = reader.ReadUInt32();
            this.Unknown_A8h = reader.ReadUInt32();
            this.Unknown_ACh = reader.ReadUInt32();
            this.Unknown_B0h = reader.ReadUInt32();
            this.Unknown_B4h = reader.ReadUInt32();
            this.Unknown_B8h = reader.ReadUInt32();
            this.Unknown_BCh = reader.ReadUInt32();
            this.Unknown_C0h = reader.ReadUInt32();
            this.Unknown_C4h = reader.ReadUInt32();
            this.Unknown_C8h = reader.ReadUInt32();
            this.Unknown_CCh = reader.ReadUInt32();

            // read reference data
            this.Controller = reader.ReadBlockAt<CharacterClothController>(
                this.ControllerPointer // offset
            );
            this.Bound = reader.ReadBlockAt<Bounds>(
                this.BoundPointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.ControllerPointer = (ulong)(this.Controller != null ? this.Controller.FilePosition : 0);
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.WriteBlock(this.Unknown_10h);
            writer.Write(this.ControllerPointer);
            writer.Write(this.BoundPointer);
            writer.WriteBlock(this.Unknown_30h);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Transform);
            writer.WriteBlock(this.Unknown_90h);
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_A4h);
            writer.Write(this.Unknown_A8h);
            writer.Write(this.Unknown_ACh);
            writer.Write(this.Unknown_B0h);
            writer.Write(this.Unknown_B4h);
            writer.Write(this.Unknown_B8h);
            writer.Write(this.Unknown_BCh);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C4h);
            writer.Write(this.Unknown_C8h);
            writer.Write(this.Unknown_CCh);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Controller != null) list.Add(Controller);
            if (Bound != null) list.Add(Bound);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x10, Unknown_10h),
                new Tuple<long, IResourceBlock>(0x30, Unknown_30h),
                new Tuple<long, IResourceBlock>(0x90, Unknown_90h)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class CharacterClothController : ClothController
    {
        // characterClothController
        public override long BlockLength => 0xF0;

        // structure data      
        public ResourceSimpleList64_ushort Indices { get; set; }
        public ResourceSimpleList64_s<Vector4> Vertices { get; set; }
        public float Unknown_A0h { get; set; } // 0x3D23D70A = 0.04f
        public uint Unknown_A4h { get; set; } // 0x00000000
        public uint Unknown_A8h { get; set; } // 0x00000000
        public uint Unknown_ACh { get; set; } // 0x00000000
        public ResourceSimpleList64_uint Unknown_B0h { get; set; }
        public ResourceSimpleList64_s<Unknown_C_003> Unknown_C0h { get; set; }
        public uint Unknown_D0h { get; set; } // 0x00000000
        public uint Unknown_D4h { get; set; } // 0x00000000
        public uint Unknown_D8h { get; set; } // 0x00000000
        public float Unknown_DCh { get; set; } // 0x3F800000 = 1.0f
        public ResourceSimpleList64_uint Unknown_E0h { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data         
            this.Indices = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Vertices = reader.ReadBlock<ResourceSimpleList64_s<Vector4>>();
            this.Unknown_A0h = reader.ReadSingle();
            this.Unknown_A4h = reader.ReadUInt32();
            this.Unknown_A8h = reader.ReadUInt32();
            this.Unknown_ACh = reader.ReadUInt32();
            this.Unknown_B0h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_C0h = reader.ReadBlock<ResourceSimpleList64_s<Unknown_C_003>>();
            this.Unknown_D0h = reader.ReadUInt32();
            this.Unknown_D4h = reader.ReadUInt32();
            this.Unknown_D8h = reader.ReadUInt32();
            this.Unknown_DCh = reader.ReadSingle();
            this.Unknown_E0h = reader.ReadBlock<ResourceSimpleList64_uint>();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data           
            writer.WriteBlock(this.Indices);
            writer.WriteBlock(this.Vertices);
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_A4h);
            writer.Write(this.Unknown_A8h);
            writer.Write(this.Unknown_ACh);
            writer.WriteBlock(this.Unknown_B0h);
            writer.WriteBlock(this.Unknown_C0h);
            writer.Write(this.Unknown_D0h);
            writer.Write(this.Unknown_D4h);
            writer.Write(this.Unknown_D8h);
            writer.Write(this.Unknown_DCh);
            writer.WriteBlock(this.Unknown_E0h);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x80, Indices),
                new Tuple<long, IResourceBlock>(0x90, Vertices),
                new Tuple<long, IResourceBlock>(0xB0, Unknown_B0h),
                new Tuple<long, IResourceBlock>(0xC0, Unknown_C0h),
                new Tuple<long, IResourceBlock>(0xE0, Unknown_E0h)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class MorphController : ResourceSystemBlock
    {
        // pgBase
        // phMorphController
        public override long BlockLength => 0x40;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public ulong Unknown_18h_Pointer { get; set; }
        public ulong Unknown_20h_Pointer { get; set; }
        public ulong Unknown_28h_Pointer { get; set; }
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000

        // reference data
        public Unknown_C_006 Unknown_18h_Data { get; set; }
        public Unknown_C_006 Unknown_20h_Data { get; set; }
        public Unknown_C_006 Unknown_28h_Data { get; set; }

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
            this.Unknown_18h_Pointer = reader.ReadUInt64();
            this.Unknown_20h_Pointer = reader.ReadUInt64();
            this.Unknown_28h_Pointer = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            this.Unknown_18h_Data = reader.ReadBlockAt<Unknown_C_006>(
                this.Unknown_18h_Pointer // offset
            );
            this.Unknown_20h_Data = reader.ReadBlockAt<Unknown_C_006>(
                this.Unknown_20h_Pointer // offset
            );
            this.Unknown_28h_Data = reader.ReadBlockAt<Unknown_C_006>(
                this.Unknown_28h_Pointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.Unknown_18h_Pointer = (ulong)(this.Unknown_18h_Data != null ? this.Unknown_18h_Data.FilePosition : 0);
            this.Unknown_20h_Pointer = (ulong)(this.Unknown_20h_Data != null ? this.Unknown_20h_Data.FilePosition : 0);
            this.Unknown_28h_Pointer = (ulong)(this.Unknown_28h_Data != null ? this.Unknown_28h_Data.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h_Pointer);
            writer.Write(this.Unknown_20h_Pointer);
            writer.Write(this.Unknown_28h_Pointer);
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
            if (Unknown_18h_Data != null) list.Add(Unknown_18h_Data);
            if (Unknown_20h_Data != null) list.Add(Unknown_20h_Data);
            if (Unknown_28h_Data != null) list.Add(Unknown_28h_Data);
            return list.ToArray();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_C_001 : ResourceSystemBlock
    {
        public override long BlockLength => 0x10;

        // structure data
        public uint Unknown_0h { get; set; }
        public uint Unknown_4h { get; set; }
        public uint Unknown_8h { get; set; }
        public uint Unknown_Ch { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
        }


        public override string ToString()
        {
            return Unknown_0h.ToString() + ", " + Unknown_4h.ToString() + ", " + Unknown_8h.ToString() + ", " + Unknown_Ch.ToString();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public struct Unknown_C_003
    {
        public Vector4 Unknown0 { get; set; }
        public Vector4 Unknown1 { get; set; }

        public override string ToString()
        {
            return Unknown0.ToString() + "   :   " + Unknown1.ToString();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_C_004 : ResourceSystemBlock
    {
        public override long BlockLength => 0x10;

        // structure data
        public ushort Unknown_0h { get; set; }
        public ushort Unknown_2h { get; set; }
        public float Unknown_4h { get; set; }
        public float Unknown_8h { get; set; }
        public float Unknown_Ch { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt16();
            this.Unknown_2h = reader.ReadUInt16();
            this.Unknown_4h = reader.ReadSingle();
            this.Unknown_8h = reader.ReadSingle();
            this.Unknown_Ch = reader.ReadSingle();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_2h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
        }

        public override string ToString()
        {
            return Unknown_0h.ToString() + ", " + Unknown_2h.ToString() + ", " + Unknown_4h.ToString() + ", " + Unknown_8h.ToString() + ", " + Unknown_Ch.ToString();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_C_006 : ResourceSystemBlock
    {
        public override long BlockLength => 0x190;

        // structure data
        public uint Unknown_0h { get; set; } // 0x00000000
        public uint Unknown_4h { get; set; } // 0x00000000
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000
        public uint Unknown_40h { get; set; } // 0x00000000
        public uint Unknown_44h { get; set; } // 0x00000000
        public uint Unknown_48h { get; set; } // 0x00000000
        public uint Unknown_4Ch { get; set; } // 0x00000000
        public ResourceSimpleList64_s<Vector4> Unknown_50h { get; set; }
        public ResourceSimpleList64_ushort Unknown_60h { get; set; }
        public ResourceSimpleList64_ushort Unknown_70h { get; set; }
        public ResourceSimpleList64_ushort Unknown_80h { get; set; }
        public ResourceSimpleList64_ushort Unknown_90h { get; set; }
        public ResourceSimpleList64_s<Vector4> Unknown_A0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_B0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_C0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_D0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_E0h { get; set; }
        public uint Unknown_F0h { get; set; } // 0x00000000
        public uint Unknown_F4h { get; set; } // 0x00000000
        public uint Unknown_F8h { get; set; } // 0x00000000
        public uint Unknown_FCh { get; set; } // 0x00000000
        public uint Unknown_100h { get; set; } // 0x00000000
        public uint Unknown_104h { get; set; } // 0x00000000
        public uint Unknown_108h { get; set; } // 0x00000000
        public uint Unknown_10Ch { get; set; } // 0x00000000
        public uint Unknown_110h { get; set; } // 0x00000000
        public uint Unknown_114h { get; set; } // 0x00000000
        public uint Unknown_118h { get; set; } // 0x00000000
        public uint Unknown_11Ch { get; set; } // 0x00000000
        public uint Unknown_120h { get; set; } // 0x00000000
        public uint Unknown_124h { get; set; } // 0x00000000
        public uint Unknown_128h { get; set; } // 0x00000000
        public uint Unknown_12Ch { get; set; } // 0x00000000
        public uint Unknown_130h { get; set; } // 0x00000000
        public uint Unknown_134h { get; set; } // 0x00000000
        public uint Unknown_138h { get; set; } // 0x00000000
        public uint Unknown_13Ch { get; set; } // 0x00000000
        public uint Unknown_140h { get; set; } // 0x00000000
        public uint Unknown_144h { get; set; } // 0x00000000
        public uint Unknown_148h { get; set; } // 0x00000000
        public uint Unknown_14Ch { get; set; } // 0x00000000
        public ResourceSimpleList64_ushort Unknown_150h { get; set; }
        public ResourceSimpleList64_ushort Unknown_160h { get; set; }
        public uint Unknown_170h { get; set; } // 0x00000000
        public uint Unknown_174h { get; set; } // 0x00000000
        public uint Unknown_178h { get; set; } // 0x00000000
        public uint Unknown_17Ch { get; set; } // 0x00000000
        public uint Unknown_180h { get; set; }
        public uint Unknown_184h { get; set; } // 0x00000000
        public uint Unknown_188h { get; set; } // 0x00000000
        public uint Unknown_18Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
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
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadBlock<ResourceSimpleList64_s<Vector4>>();
            this.Unknown_60h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_70h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_80h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_90h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_A0h = reader.ReadBlock<ResourceSimpleList64_s<Vector4>>();
            this.Unknown_B0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_C0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_D0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_E0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_F0h = reader.ReadUInt32();
            this.Unknown_F4h = reader.ReadUInt32();
            this.Unknown_F8h = reader.ReadUInt32();
            this.Unknown_FCh = reader.ReadUInt32();
            this.Unknown_100h = reader.ReadUInt32();
            this.Unknown_104h = reader.ReadUInt32();
            this.Unknown_108h = reader.ReadUInt32();
            this.Unknown_10Ch = reader.ReadUInt32();
            this.Unknown_110h = reader.ReadUInt32();
            this.Unknown_114h = reader.ReadUInt32();
            this.Unknown_118h = reader.ReadUInt32();
            this.Unknown_11Ch = reader.ReadUInt32();
            this.Unknown_120h = reader.ReadUInt32();
            this.Unknown_124h = reader.ReadUInt32();
            this.Unknown_128h = reader.ReadUInt32();
            this.Unknown_12Ch = reader.ReadUInt32();
            this.Unknown_130h = reader.ReadUInt32();
            this.Unknown_134h = reader.ReadUInt32();
            this.Unknown_138h = reader.ReadUInt32();
            this.Unknown_13Ch = reader.ReadUInt32();
            this.Unknown_140h = reader.ReadUInt32();
            this.Unknown_144h = reader.ReadUInt32();
            this.Unknown_148h = reader.ReadUInt32();
            this.Unknown_14Ch = reader.ReadUInt32();
            this.Unknown_150h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_160h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_170h = reader.ReadUInt32();
            this.Unknown_174h = reader.ReadUInt32();
            this.Unknown_178h = reader.ReadUInt32();
            this.Unknown_17Ch = reader.ReadUInt32();
            this.Unknown_180h = reader.ReadUInt32();
            this.Unknown_184h = reader.ReadUInt32();
            this.Unknown_188h = reader.ReadUInt32();
            this.Unknown_18Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
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
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
            writer.WriteBlock(this.Unknown_50h);
            writer.WriteBlock(this.Unknown_60h);
            writer.WriteBlock(this.Unknown_70h);
            writer.WriteBlock(this.Unknown_80h);
            writer.WriteBlock(this.Unknown_90h);
            writer.WriteBlock(this.Unknown_A0h);
            writer.WriteBlock(this.Unknown_B0h);
            writer.WriteBlock(this.Unknown_C0h);
            writer.WriteBlock(this.Unknown_D0h);
            writer.WriteBlock(this.Unknown_E0h);
            writer.Write(this.Unknown_F0h);
            writer.Write(this.Unknown_F4h);
            writer.Write(this.Unknown_F8h);
            writer.Write(this.Unknown_FCh);
            writer.Write(this.Unknown_100h);
            writer.Write(this.Unknown_104h);
            writer.Write(this.Unknown_108h);
            writer.Write(this.Unknown_10Ch);
            writer.Write(this.Unknown_110h);
            writer.Write(this.Unknown_114h);
            writer.Write(this.Unknown_118h);
            writer.Write(this.Unknown_11Ch);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_124h);
            writer.Write(this.Unknown_128h);
            writer.Write(this.Unknown_12Ch);
            writer.Write(this.Unknown_130h);
            writer.Write(this.Unknown_134h);
            writer.Write(this.Unknown_138h);
            writer.Write(this.Unknown_13Ch);
            writer.Write(this.Unknown_140h);
            writer.Write(this.Unknown_144h);
            writer.Write(this.Unknown_148h);
            writer.Write(this.Unknown_14Ch);
            writer.WriteBlock(this.Unknown_150h);
            writer.WriteBlock(this.Unknown_160h);
            writer.Write(this.Unknown_170h);
            writer.Write(this.Unknown_174h);
            writer.Write(this.Unknown_178h);
            writer.Write(this.Unknown_17Ch);
            writer.Write(this.Unknown_180h);
            writer.Write(this.Unknown_184h);
            writer.Write(this.Unknown_188h);
            writer.Write(this.Unknown_18Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x50, Unknown_50h),
                new Tuple<long, IResourceBlock>(0x60, Unknown_60h),
                new Tuple<long, IResourceBlock>(0x70, Unknown_70h),
                new Tuple<long, IResourceBlock>(0x80, Unknown_80h),
                new Tuple<long, IResourceBlock>(0x90, Unknown_90h),
                new Tuple<long, IResourceBlock>(0xA0, Unknown_A0h),
                new Tuple<long, IResourceBlock>(0xB0, Unknown_B0h),
                new Tuple<long, IResourceBlock>(0xC0, Unknown_C0h),
                new Tuple<long, IResourceBlock>(0xD0, Unknown_D0h),
                new Tuple<long, IResourceBlock>(0xE0, Unknown_E0h),
                new Tuple<long, IResourceBlock>(0x150, Unknown_150h),
                new Tuple<long, IResourceBlock>(0x160, Unknown_160h)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_C_007 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public uint Unknown_0h { get; set; } // 0x00000000
        public uint Unknown_4h { get; set; } // 0x00000000
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
        }


        public override string ToString()
        {
            return Unknown_0h.ToString() + ", " + Unknown_4h.ToString() + ", " + Unknown_8h.ToString() + ", " + Unknown_Ch.ToString();
        }
    }



}
