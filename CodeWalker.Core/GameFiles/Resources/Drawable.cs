using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ShaderGroup : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public ulong TextureDictionaryPointer { get; set; }
        public ulong ShadersPointer { get; set; }
        public ushort ShadersCount1 { get; set; }
        public ushort ShadersCount2 { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; }
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000

        // reference data
        public TextureDictionary TextureDictionary { get; set; }
        public ResourcePointerArray64<ShaderFX> Shaders { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.TextureDictionaryPointer = reader.ReadUInt64();
            this.ShadersPointer = reader.ReadUInt64();
            this.ShadersCount1 = reader.ReadUInt16();
            this.ShadersCount2 = reader.ReadUInt16();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            this.TextureDictionary = reader.ReadBlockAt<TextureDictionary>(
                this.TextureDictionaryPointer // offset
            );
            this.Shaders = reader.ReadBlockAt<ResourcePointerArray64<ShaderFX>>(
                this.ShadersPointer, // offset
                this.ShadersCount1
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.TextureDictionaryPointer = (ulong)(this.TextureDictionary != null ? this.TextureDictionary.FilePosition : 0);
            this.ShadersPointer = (ulong)(this.Shaders != null ? this.Shaders.FilePosition : 0);
            this.ShadersCount1 = (ushort)(this.Shaders != null ? this.Shaders.Count : 0);
            this.ShadersCount2 = this.ShadersCount1;

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.TextureDictionaryPointer);
            writer.Write(this.ShadersPointer);
            writer.Write(this.ShadersCount1);
            writer.Write(this.ShadersCount2);
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

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (TextureDictionary != null) list.Add(TextureDictionary);
            if (Shaders != null) list.Add(Shaders);
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ShaderFX : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public ulong ParametersPointer { get; set; }
        public MetaHash Name { get; set; } //530103687, 2401522793, 1912906641
        public uint Unknown_Ch { get; set; } // 0x00000000
        public byte ParameterCount { get; set; }
        public byte RenderBucket { get; set; } // 2, 0, 
        public ushort Unknown_12h { get; set; } // 32768    HasComment?
        public ushort ParameterSize { get; set; } //112, 208, 320    (with 16h) 10485872, 17826000, 26214720
        public ushort ParameterDataSize { get; set; } //160, 272, 400 
        public MetaHash FileName { get; set; } //2918136469, 2635608835, 2247429097
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint RenderBucketMask { get; set; } //65284, 65281  DrawBucketMask?   (1<<bucket) | 0xFF00
        public ushort Unknown_24h { get; set; } //0   Instanced?
        public byte Unknown_26h { get; set; } //0
        public byte TextureParametersCount { get; set; }
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        // reference data
        //public ResourceSimpleArray<ShaderParameter_GTA5_pc> Parameters { get; set; }
        //public SimpleArrayOFFSET<uint_r> ParameterHashes { get; set; }
        public ShaderParametersBlock ParametersList { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.ParametersPointer = reader.ReadUInt64();
            this.Name = new MetaHash(reader.ReadUInt32());
            this.Unknown_Ch = reader.ReadUInt32();
            this.ParameterCount = reader.ReadByte();
            this.RenderBucket = reader.ReadByte();
            this.Unknown_12h = reader.ReadUInt16();
            this.ParameterSize = reader.ReadUInt16();
            this.ParameterDataSize = reader.ReadUInt16();
            this.FileName = new MetaHash(reader.ReadUInt32());
            this.Unknown_1Ch = reader.ReadUInt32();
            this.RenderBucketMask = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt16();
            this.Unknown_26h = reader.ReadByte();
            this.TextureParametersCount = reader.ReadByte();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();

            // read reference data
            this.ParametersList = reader.ReadBlockAt<ShaderParametersBlock>(
                this.ParametersPointer, // offset
                this.ParameterCount
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.ParametersPointer = (ulong)(this.ParametersList != null ? this.ParametersList.FilePosition : 0);
            this.ParameterCount = (byte)(this.ParametersList != null ? this.ParametersList.Count : 0);

            // write structure data
            writer.Write(this.ParametersPointer);
            writer.Write(this.Name.Hash);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.ParameterCount);
            writer.Write(this.RenderBucket);
            writer.Write(this.Unknown_12h);
            writer.Write(this.ParameterSize);
            writer.Write(this.ParameterDataSize);
            writer.Write(this.FileName.Hash);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.RenderBucketMask);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_26h);
            writer.Write(this.TextureParametersCount);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (ParametersList != null) list.Add(ParametersList);
            return list.ToArray();
        }


        public override string ToString()
        {
            return Name.ToString() + " (" + FileName.ToString() + ")";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ShaderParameter : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public byte DataType { get; set; }
        public byte Unknown_1h { get; set; }
        public ushort Unknown_2h { get; set; }
        public uint Unknown_4h { get; set; }
        public ulong DataPointer { get; set; }

        //public IResourceBlock Data { get; set; }
        public object Data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.DataType = reader.ReadByte();
            this.Unknown_1h = reader.ReadByte();
            this.Unknown_2h = reader.ReadUInt16();
            this.Unknown_4h = reader.ReadUInt32();
            this.DataPointer = reader.ReadUInt64();

            // DONT READ DATA...
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.DataType);
            writer.Write(this.Unknown_1h);
            writer.Write(this.Unknown_2h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.DataPointer);

            // DONT WRITE DATA
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            //if (Data != null) list.Add(Data);
            return list.ToArray();
        }

        public override string ToString()
        {
            return (Data != null) ? Data.ToString() : (DataType.ToString() + ": " + DataPointer.ToString());
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ShaderParametersBlock : ResourceSystemBlock
    {

        public override long BlockLength
        {
            get
            {
                long offset = 0;
                foreach (var x in Parameters)
                {
                    offset += 16;
                }

                foreach (var x in Parameters)
                {
                    offset += 16 * x.DataType;
                }

                offset += Parameters.Length * 4;

                return offset;
            }
        }

        public ushort ParametersSize
        {
            get
            {
                ushort size = (ushort)((Parameters?.Length??0) * 16);
                foreach (var x in Parameters)
                {
                    size += (ushort)(16 * x.DataType);
                }
                return size;
            }
        }

        public byte TextureParamsCount
        {
            get
            {
                byte c = 0;
                foreach (var x in Parameters)
                {
                    if (x.DataType == 0) c++;
                }
                return c;
            }
        }

        public ShaderParameter[] Parameters { get; set; }
        public MetaName[] Hashes { get; set; }
        public int Count { get; set; }

        private ResourceSystemStructBlock<Vector4>[] ParameterDataBlocks = null;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            Count = Convert.ToInt32(parameters[0]);

            var paras = new List<ShaderParameter>();
            for (int i = 0; i < Count; i++)
            {
                paras.Add(reader.ReadBlock<ShaderParameter>());
            }

            int offset = 0;
            for (int i = 0; i < Count; i++)
            {
                var p = paras[i];

                // read reference data
                switch (p.DataType)
                {
                    case 0:
                        offset += 0;
                        p.Data = reader.ReadBlockAt<TextureBase>(p.DataPointer);
                        break;
                    case 1:
                        offset += 16;
                        p.Data = reader.ReadStructAt<Vector4>((long)p.DataPointer);
                        break;
                    default:
                        offset += 16 * p.DataType;
                        p.Data = reader.ReadStructsAt<Vector4>(p.DataPointer, p.DataType);
                        break;
                }
            }


            reader.Position += offset; //Vector4 data gets embedded here... but why pointers in params also???

            var hashes = new List<MetaName>();
            for (int i = 0; i < Count; i++)
            {
                hashes.Add((MetaName)reader.ReadUInt32());
            }

            Parameters = paras.ToArray();
            Hashes = hashes.ToArray();

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {

            // update pointers...
            //foreach (var f in Parameters)
            //    if (f.Data != null)
            //        f.DataPointer = (ulong)f.Data.Position;
            //    else
            //        f.DataPointer = 0;
            for (int i = 0; i < Parameters.Length; i++)
            {
                var param = Parameters[i];
                if (param.DataType == 0)
                {
                    param.DataPointer = (ulong)((param.Data as TextureBase)?.FilePosition ?? 0);
                }
                else
                {
                    var block = (i < ParameterDataBlocks?.Length) ? ParameterDataBlocks[i] : null;
                    if (block != null)
                    {
                        param.DataPointer = (ulong)block.FilePosition;
                    }
                    else
                    {
                        param.DataPointer = 0;//shouldn't happen!
                    }
                }
            }



            // write parameter infos
            foreach (var f in Parameters)
                writer.WriteBlock(f);

            // write vector data
            //foreach (var f in Parameters)
            //{
            //    if (f.DataType != 0)
            //        writer.WriteBlock(f.Data);
            //}
            for (int i = 0; i < Parameters.Length; i++)
            {
                var param = Parameters[i];
                if (param.DataType != 0)
                {
                    var block = (i < ParameterDataBlocks?.Length) ? ParameterDataBlocks[i] : null;
                    if (block != null)
                    {
                        writer.WriteBlock(block);
                    }
                    else
                    { } //shouldn't happen!
                }
            }

            // write hashes
            foreach (var h in Hashes)
                writer.Write((uint)h);
        }




        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            list.AddRange(base.GetReferences());

            foreach (var x in Parameters)
                if (x.DataType == 0)
                    list.Add(x.Data as TextureBase);

            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            var list = new List<Tuple<long, IResourceBlock>>();
            list.AddRange(base.GetParts());

            long offset = 0;
            foreach (var x in Parameters)
            {
                list.Add(new Tuple<long, IResourceBlock>(offset, x));
                offset += 16;
            }


            var blist = new List<ResourceSystemStructBlock<Vector4>>();
            foreach (var x in Parameters)
            {
                if (x.DataType != 0)
                {
                    var vecs = x.Data as Vector4[];
                    if (vecs == null)
                    {
                        vecs = new[] { (Vector4)x.Data };
                    }
                    if (vecs == null)
                    { }
                    var block = new ResourceSystemStructBlock<Vector4>(vecs);
                    list.Add(new Tuple<long, IResourceBlock>(offset, block));
                    blist.Add(block);
                }
                else
                {
                    blist.Add(null);
                }
                offset += 16 * x.DataType;
            }
            ParameterDataBlocks = blist.ToArray();

            return list.ToArray();
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class Skeleton : ResourceSystemBlock
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
        public ulong Unknown_10h_Pointer { get; set; }
        public ushort Count1 { get; set; }
        public ushort Count2 { get; set; }
        public uint Unknown_1Ch { get; set; }
        public ulong BonesPointer { get; set; }
        public ulong TransformationsInvertedPointer { get; set; }
        public ulong TransformationsPointer { get; set; }
        public ulong ParentIndicesPointer { get; set; }
        public ulong Unknown_40h_Pointer { get; set; }
        public uint Unknown_48h { get; set; } // 0x00000000
        public uint Unknown_4Ch { get; set; } // 0x00000000
        public MetaHash Unknown_50h { get; set; }
        public MetaHash Unknown_54h { get; set; }
        public MetaHash Unknown_58h { get; set; }
        public ushort Unknown_5Ch { get; set; } // 0x0001
        public ushort BonesCount { get; set; }
        public ushort Count4 { get; set; }
        public ushort Unknown_62h { get; set; } // 0x0000
        public uint Unknown_64h { get; set; } // 0x00000000
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000

        // reference data
        public ResourcePointerArray64<Skeleton_Unknown_D_001> Unknown_10h_Data { get; set; }
        public ResourceSimpleArray<Bone> Bones { get; set; }

        public Matrix[] TransformationsInverted { get; set; }
        public Matrix[] Transformations { get; set; }
        public ushort[] ParentIndices { get; set; }
        public ushort[] Unknown_40h_Data { get; set; }

        private ResourceSystemStructBlock<Matrix> TransformationsInvertedBlock = null;//for saving only
        private ResourceSystemStructBlock<Matrix> TransformationsBlock = null;
        private ResourceSystemStructBlock<ushort> ParentIndicesBlock = null;
        private ResourceSystemStructBlock<ushort> Unknown_40h_DataBlock = null;


        public Dictionary<ushort, Bone> BonesMap { get; set; }//for convienience finding bones by tag



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
            this.Unknown_10h_Pointer = reader.ReadUInt64();
            this.Count1 = reader.ReadUInt16();
            this.Count2 = reader.ReadUInt16();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.BonesPointer = reader.ReadUInt64();
            this.TransformationsInvertedPointer = reader.ReadUInt64();
            this.TransformationsPointer = reader.ReadUInt64();
            this.ParentIndicesPointer = reader.ReadUInt64();
            this.Unknown_40h_Pointer = reader.ReadUInt64();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = new MetaHash(reader.ReadUInt32());
            this.Unknown_54h = new MetaHash(reader.ReadUInt32());
            this.Unknown_58h = new MetaHash(reader.ReadUInt32());
            this.Unknown_5Ch = reader.ReadUInt16();
            this.BonesCount = reader.ReadUInt16();
            this.Count4 = reader.ReadUInt16();
            this.Unknown_62h = reader.ReadUInt16();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();

            // read reference data
            this.Unknown_10h_Data = reader.ReadBlockAt<ResourcePointerArray64<Skeleton_Unknown_D_001>>(
                this.Unknown_10h_Pointer, // offset
                this.Count1
            );
            this.Bones = reader.ReadBlockAt<ResourceSimpleArray<Bone>>(
                this.BonesPointer, // offset
                this.BonesCount
            );
            this.TransformationsInverted = reader.ReadStructsAt<Matrix>(this.TransformationsInvertedPointer, this.BonesCount);
            this.Transformations = reader.ReadStructsAt<Matrix>(this.TransformationsPointer, this.BonesCount);
            this.ParentIndices = reader.ReadUshortsAt(this.ParentIndicesPointer, this.BonesCount);
            this.Unknown_40h_Data = reader.ReadUshortsAt(this.Unknown_40h_Pointer, this.Count4);


            if ((Bones != null) && (ParentIndices != null))
            {
                var maxcnt = Math.Min(Bones.Count, ParentIndices.Length);
                for (int i = 0; i < maxcnt; i++)
                {
                    var bone = Bones[i];
                    var pind = ParentIndices[i];
                    if (pind < Bones.Count)
                    {
                        bone.Parent = Bones[pind];
                    }
                }
            }

            BonesMap = new Dictionary<ushort, Bone>();
            if (Bones != null)
            {
                for (int i = 0; i < Bones.Count; i++)
                {
                    var bone = Bones[i];
                    BonesMap[bone.Id] = bone;
                }
            }

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.Unknown_10h_Pointer = (ulong)(this.Unknown_10h_Data != null ? this.Unknown_10h_Data.FilePosition : 0);
            this.Count1 = (ushort)(this.Unknown_10h_Data != null ? this.Unknown_10h_Data.Count : 0);
            this.BonesPointer = (ulong)(this.Bones != null ? this.Bones.FilePosition : 0);
            this.TransformationsInvertedPointer = (ulong)(this.TransformationsInvertedBlock != null ? this.TransformationsInvertedBlock.FilePosition : 0);
            this.TransformationsPointer = (ulong)(this.TransformationsBlock != null ? this.TransformationsBlock.FilePosition : 0);
            this.ParentIndicesPointer = (ulong)(this.ParentIndicesBlock != null ? this.ParentIndicesBlock.FilePosition : 0);
            this.Unknown_40h_Pointer = (ulong)(this.Unknown_40h_DataBlock != null ? this.Unknown_40h_DataBlock.FilePosition : 0);
            this.BonesCount = (ushort)(this.Bones != null ? this.Bones.Count : 0);
            this.Count4 = (ushort)(this.Unknown_40h_DataBlock != null ? this.Unknown_40h_DataBlock.ItemCount : 0);
            //this.Count2 = BonesCount;//?


            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h_Pointer);
            writer.Write(this.Count1);
            writer.Write(this.Count2);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.BonesPointer);
            writer.Write(this.TransformationsInvertedPointer);
            writer.Write(this.TransformationsPointer);
            writer.Write(this.ParentIndicesPointer);
            writer.Write(this.Unknown_40h_Pointer);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
            writer.Write(this.BonesCount);
            writer.Write(this.Count4);
            writer.Write(this.Unknown_62h);
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
            if (Unknown_10h_Data != null) list.Add(Unknown_10h_Data);
            if (Bones != null) list.Add(Bones);
            if (TransformationsInverted != null)
            {
                TransformationsInvertedBlock = new ResourceSystemStructBlock<Matrix>(TransformationsInverted);
                list.Add(TransformationsInvertedBlock);
            }
            if (Transformations != null)
            {
                TransformationsBlock = new ResourceSystemStructBlock<Matrix>(Transformations);
                list.Add(TransformationsBlock);
            }
            if (ParentIndices != null)
            {
                ParentIndicesBlock = new ResourceSystemStructBlock<ushort>(ParentIndices);
                list.Add(ParentIndicesBlock);
            }
            if (Unknown_40h_Data != null)
            {
                Unknown_40h_DataBlock = new ResourceSystemStructBlock<ushort>(Unknown_40h_Data);
                list.Add(Unknown_40h_DataBlock);
            }
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class Skeleton_Unknown_D_001 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public uint Unknown_0h { get; set; }
        public uint Unknown_4h { get; set; }
        public ulong Unknown_8h_Pointer { get; set; }

        // reference data
        public Skeleton_Unknown_D_001 p1data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h_Pointer = reader.ReadUInt64();

            // read reference data
            this.p1data = reader.ReadBlockAt<Skeleton_Unknown_D_001>(
                this.Unknown_8h_Pointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.Unknown_8h_Pointer = (ulong)(this.p1data != null ? this.p1data.FilePosition : 0);

            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h_Pointer);
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

    [TypeConverter(typeof(ExpandableObjectConverter))] public class Bone : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 80; }
        }

        // structure data
        //public float RotationX { get; set; }
        //public float RotationY { get; set; }
        //public float RotationZ { get; set; }
        //public float RotationW { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Translation { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000 RHW?
        public float ScaleX { get; set; } // 1.0
        public float ScaleY { get; set; } // 1.0
        public float ScaleZ { get; set; } // 1.0
        public float Unknown_2Ch { get; set; } // 1.0  RHW?
        public ushort NextSiblingIndex { get; set; } //limb end index? IK chain?
        public short ParentIndex { get; set; }
        public uint Unknown_34h { get; set; } // 0x00000000
        public ulong NamePointer { get; set; }
        public ushort Flags { get; set; }
        public ushort Unknown_42h { get; set; }
        public ushort Id { get; set; }
        public ushort Unknown_46h { get; set; }
        public uint Unknown_48h { get; set; } // 0x00000000
        public uint Unknown_4Ch { get; set; } // 0x00000000

        // reference data
        public string Name { get; set; }

        public Bone Parent { get; set; }

        private string_r NameBlock = null;


        //used by CW for animating skeletons.
        public Quaternion AnimRotation;
        public Vector3 AnimTranslation;



        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            //this.RotationX = reader.ReadSingle();
            //this.RotationY = reader.ReadSingle();
            //this.RotationZ = reader.ReadSingle();
            //this.RotationW = reader.ReadSingle();
            this.Rotation = new Quaternion(reader.ReadVector4());
            this.Translation = reader.ReadVector3();
            //this.TranslationX = reader.ReadSingle();
            //this.TranslationY = reader.ReadSingle();
            //this.TranslationZ = reader.ReadSingle();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.ScaleX = reader.ReadSingle();
            this.ScaleY = reader.ReadSingle();
            this.ScaleZ = reader.ReadSingle();
            this.Unknown_2Ch = reader.ReadSingle();
            this.NextSiblingIndex = reader.ReadUInt16();
            this.ParentIndex = reader.ReadInt16();
            this.Unknown_34h = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.Flags = reader.ReadUInt16();
            this.Unknown_42h = reader.ReadUInt16();
            this.Id = reader.ReadUInt16();
            this.Unknown_46h = reader.ReadUInt16();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();

            // read reference data
            this.Name = reader.ReadStringAt(//BlockAt<string_r>(
                this.NamePointer // offset
            );


            AnimRotation = Rotation;
            AnimTranslation = Translation;
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.NameBlock != null ? this.NameBlock.FilePosition : 0);

            // write structure data
            writer.Write(this.Rotation.ToVector4());
            writer.Write(this.Translation);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.ScaleX);
            writer.Write(this.ScaleY);
            writer.Write(this.ScaleZ);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.NextSiblingIndex);
            writer.Write(this.ParentIndex);
            writer.Write(this.Unknown_34h);
            writer.Write(this.NamePointer);
            writer.Write(this.Flags);
            writer.Write(this.Unknown_42h);
            writer.Write(this.Id);
            writer.Write(this.Unknown_46h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null)
            {
                NameBlock = (string_r)Name;
                list.Add(NameBlock);
            }
            return list.ToArray();
        }

        public override string ToString()
        {
            return Id.ToString() + ": " + Name;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class Joints : ResourceSystemBlock
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
        public ulong RotationLimitsPointer { get; set; }
        public ulong TranslationLimitsPointer { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ushort RotationLimitsCount { get; set; }
        public ushort TranslationLimitsCount { get; set; }
        public ushort Unknown_34h { get; set; } // 0x0000
        public ushort Unknown_36h { get; set; } // 0x0001
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000

        // reference data
        public JointRotationLimit_s[] RotationLimits { get; set; }
        public JointTranslationLimit_s[] TranslationLimits { get; set; }

        private ResourceSystemStructBlock<JointRotationLimit_s> RotationLimitsBlock = null; //for saving only
        private ResourceSystemStructBlock<JointTranslationLimit_s> TranslationLimitsBlock = null;


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
            this.RotationLimitsPointer = reader.ReadUInt64();
            this.TranslationLimitsPointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.RotationLimitsCount = reader.ReadUInt16();
            this.TranslationLimitsCount = reader.ReadUInt16();
            this.Unknown_34h = reader.ReadUInt16();
            this.Unknown_36h = reader.ReadUInt16();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            //this.RotationLimits = reader.ReadBlockAt<ResourceSimpleArray<JointRotationLimit>>(
            //    this.RotationLimitsPointer, // offset
            //    this.RotationLimitsCount
            //);
            //this.TranslationLimits = reader.ReadBlockAt<ResourceSimpleArray<JointTranslationLimit>>(
            //    this.TranslationLimitsPointer, // offset
            //    this.TranslationLimitsCount
            //);
            this.RotationLimits = reader.ReadStructsAt<JointRotationLimit_s>(this.RotationLimitsPointer, this.RotationLimitsCount);
            this.TranslationLimits = reader.ReadStructsAt<JointTranslationLimit_s>(this.TranslationLimitsPointer, this.TranslationLimitsCount);

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.RotationLimitsPointer = (ulong)(this.RotationLimitsBlock != null ? this.RotationLimitsBlock.FilePosition : 0);
            this.TranslationLimitsPointer = (ulong)(this.TranslationLimitsBlock != null ? this.TranslationLimitsBlock.FilePosition : 0);
            this.RotationLimitsCount = (ushort)(this.RotationLimitsBlock != null ? this.RotationLimitsBlock.ItemCount : 0);
            this.TranslationLimitsCount = (ushort)(this.TranslationLimitsBlock != null ? this.TranslationLimitsBlock.ItemCount : 0);


            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.RotationLimitsPointer);
            writer.Write(this.TranslationLimitsPointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.RotationLimitsCount);
            writer.Write(this.TranslationLimitsCount);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_36h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (RotationLimits != null)
            {
                RotationLimitsBlock = new ResourceSystemStructBlock<JointRotationLimit_s>(RotationLimits);
                list.Add(RotationLimitsBlock);
            }
            if (TranslationLimits != null)
            {
                TranslationLimitsBlock = new ResourceSystemStructBlock<JointTranslationLimit_s>(TranslationLimits);
                list.Add(TranslationLimitsBlock);
            }
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct JointRotationLimit_s
    {
        // structure data
        public uint Unknown_0h { get; set; } // 0x00000000
        public uint Unknown_4h { get; set; } // 0x00000000
        public ushort BoneId { get; set; }
        public ushort Unknown_Ah { get; set; }
        public uint Unknown_Ch { get; set; } // 0x00000001
        public uint Unknown_10h { get; set; } // 0x00000003
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public float Unknown_2Ch { get; set; } // 1.0
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000
        public float Unknown_40h { get; set; } // 1.0
        public uint Unknown_44h { get; set; } // 0x00000000
        public uint Unknown_48h { get; set; } // 0x00000000
        public uint Unknown_4Ch { get; set; } // 0x00000000
        public float Unknown_50h { get; set; } // -pi
        public float Unknown_54h { get; set; } // pi
        public float Unknown_58h { get; set; } // 1.0
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }
        public float Unknown_74h { get; set; } // pi
        public float Unknown_78h { get; set; } // -pi
        public float Unknown_7Ch { get; set; } // pi
        public float Unknown_80h { get; set; } // pi
        public float Unknown_84h { get; set; } // -pi
        public float Unknown_88h { get; set; } // pi
        public float Unknown_8Ch { get; set; } // pi
        public float Unknown_90h { get; set; } // -pi
        public float Unknown_94h { get; set; } // pi
        public float Unknown_98h { get; set; } // pi
        public float Unknown_9Ch { get; set; } // -pi
        public float Unknown_A0h { get; set; } // pi
        public float Unknown_A4h { get; set; } // pi
        public float Unknown_A8h { get; set; } // -pi
        public float Unknown_ACh { get; set; } // pi
        public float Unknown_B0h { get; set; } // pi
        public float Unknown_B4h { get; set; } // -pi
        public float Unknown_B8h { get; set; } // pi
        public uint Unknown_BCh { get; set; } // 0x00000100
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct JointTranslationLimit_s
    {
        public uint Unknown_0h { get; set; } // 0x00000000
        public uint Unknown_4h { get; set; } // 0x00000000
        public uint BoneId { get; set; }
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public Vector3 Min { get; set; }
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public Vector3 Max { get; set; }
        public uint Unknown_3Ch { get; set; } // 0x00000000
    }







    [TypeConverter(typeof(ExpandableObjectConverter))] public class DrawableModel : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public ulong GeometriesPointer { get; set; }
        public ushort GeometriesCount1 { get; set; }
        public ushort GeometriesCount2 { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public ulong BoundsPointer { get; set; }
        public ulong ShaderMappingPointer { get; set; }
        public uint SkeletonBinding { get; set; }//4th byte is bone index, 2nd byte for skin meshes
        public ushort RenderMaskFlags { get; set; } //First byte is called "Mask" in GIMS EVO
        public ushort GeometriesCount3 { get; set; } //always equal to GeometriesCount, is it ShaderMappingCount?

        // reference data
        public ResourcePointerArray64<DrawableGeometry> Geometries { get; set; }
        public AABB_s[] BoundsData { get; set; }
        public ushort[] ShaderMapping { get; set; }


        private ResourceSystemStructBlock<AABB_s> BoundsDataBlock = null; //for saving only
        private ResourceSystemStructBlock<ushort> ShaderMappingBlock = null;



        public long MemoryUsage
        {
            get
            {
                long val = 0;
                if ((Geometries != null) && (Geometries.data_items != null))
                {
                    foreach(var geom in Geometries.data_items)
                    {
                        if (geom == null) continue;
                        if (geom.VertexData != null)
                        {
                            val += geom.VertexData.MemoryUsage;
                        }
                        if (geom.IndexBuffer != null)
                        {
                            val += geom.IndexBuffer.IndicesCount * 4;
                        }
                        if (geom.VertexBuffer != null)
                        {
                            if ((geom.VertexBuffer.Data1 != null) && (geom.VertexBuffer.Data1 != geom.VertexData))
                            {
                                val += geom.VertexBuffer.Data1.MemoryUsage;
                            }
                            if ((geom.VertexBuffer.Data2 != null) && (geom.VertexBuffer.Data2 != geom.VertexData))
                            {
                                val += geom.VertexBuffer.Data2.MemoryUsage;
                            }
                        }
                    }
                }
                if (BoundsData != null)
                {
                    val += BoundsData.Length * 32;
                }
                return val;
            }
        }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.GeometriesPointer = reader.ReadUInt64();
            this.GeometriesCount1 = reader.ReadUInt16();
            this.GeometriesCount2 = reader.ReadUInt16();
            this.Unknown_14h = reader.ReadUInt32();
            this.BoundsPointer = reader.ReadUInt64();
            this.ShaderMappingPointer = reader.ReadUInt64();
            this.SkeletonBinding = reader.ReadUInt32();
            this.RenderMaskFlags = reader.ReadUInt16();
            this.GeometriesCount3 = reader.ReadUInt16();


            // read reference data
            this.Geometries = reader.ReadBlockAt<ResourcePointerArray64<DrawableGeometry>>(
                this.GeometriesPointer, // offset
                this.GeometriesCount1
            );
            this.BoundsData = reader.ReadStructsAt<AABB_s>(this.BoundsPointer, (uint)(this.GeometriesCount1 > 1 ? this.GeometriesCount1 + 1 : this.GeometriesCount1));
            this.ShaderMapping = reader.ReadUshortsAt(this.ShaderMappingPointer, this.GeometriesCount1);

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.GeometriesPointer = (ulong)(this.Geometries != null ? this.Geometries.FilePosition : 0);
            this.GeometriesCount1 = (ushort)(this.Geometries != null ? this.Geometries.Count : 0);
            this.GeometriesCount2 = this.GeometriesCount1;//is this correct?
            this.GeometriesCount3 = this.GeometriesCount1;//is this correct?
            this.BoundsPointer = (ulong)(this.BoundsDataBlock != null ? this.BoundsDataBlock.FilePosition : 0);
            this.ShaderMappingPointer = (ulong)(this.ShaderMappingBlock != null ? this.ShaderMappingBlock.FilePosition : 0);
            

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.GeometriesPointer);
            writer.Write(this.GeometriesCount1);
            writer.Write(this.GeometriesCount2);
            writer.Write(this.Unknown_14h);
            writer.Write(this.BoundsPointer);
            writer.Write(this.ShaderMappingPointer);
            writer.Write(this.SkeletonBinding);
            writer.Write(this.RenderMaskFlags);
            writer.Write(this.GeometriesCount3);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Geometries != null) list.Add(Geometries);
            if (BoundsData != null)
            {
                BoundsDataBlock = new ResourceSystemStructBlock<AABB_s>(BoundsData);
                list.Add(BoundsDataBlock);
            }
            if (ShaderMapping != null)
            {
                ShaderMappingBlock = new ResourceSystemStructBlock<ushort>(ShaderMapping);
                list.Add(ShaderMappingBlock);
            }
            return list.ToArray();
        }

        public override string ToString()
        {
            return "(" + Geometries.Count + " geometr" + (Geometries.Count != 1 ? "ies)" : "y)");
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class DrawableGeometry : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 152; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public ulong VertexBufferPointer { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public ulong IndexBufferPointer { get; set; }
        public uint Unknown_40h { get; set; } // 0x00000000
        public uint Unknown_44h { get; set; } // 0x00000000
        public uint Unknown_48h { get; set; } // 0x00000000
        public uint Unknown_4Ch { get; set; } // 0x00000000
        public uint Unknown_50h { get; set; } // 0x00000000
        public uint Unknown_54h { get; set; } // 0x00000000
        public uint IndicesCount { get; set; }
        public uint TrianglesCount { get; set; }
        public ushort VerticesCount { get; set; }
        public ushort Unknown_62h { get; set; } // 0x0003
        public uint Unknown_64h { get; set; } // 0x00000000
        public ulong BoneIdsPointer { get; set; }
        public ushort VertexStride { get; set; }
        public ushort BoneIdsCount { get; set; }
        public uint Unknown_74h { get; set; } // 0x00000000
        public ulong VertexDataPointer { get; set; }
        public uint Unknown_80h { get; set; } // 0x00000000
        public uint Unknown_84h { get; set; } // 0x00000000
        public uint Unknown_88h { get; set; } // 0x00000000
        public uint Unknown_8Ch { get; set; } // 0x00000000
        public uint Unknown_90h { get; set; } // 0x00000000
        public uint Unknown_94h { get; set; } // 0x00000000

        // reference data
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public ushort[] BoneIds { get; set; }
        public VertexData VertexData { get; set; }
        public ShaderFX Shader { get; set; }

        private ResourceSystemStructBlock<ushort> BoneIdsBlock = null;//for saving only


        public bool UpdateRenderableParameters { get; set; } = false; //used by model material editor...


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
            this.VertexBufferPointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.IndexBufferPointer = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.IndicesCount = reader.ReadUInt32();
            this.TrianglesCount = reader.ReadUInt32();
            this.VerticesCount = reader.ReadUInt16();
            this.Unknown_62h = reader.ReadUInt16();
            this.Unknown_64h = reader.ReadUInt32();
            this.BoneIdsPointer = reader.ReadUInt64();
            this.VertexStride = reader.ReadUInt16();
            this.BoneIdsCount = reader.ReadUInt16();
            this.Unknown_74h = reader.ReadUInt32();
            this.VertexDataPointer = reader.ReadUInt64();
            this.Unknown_80h = reader.ReadUInt32();
            this.Unknown_84h = reader.ReadUInt32();
            this.Unknown_88h = reader.ReadUInt32();
            this.Unknown_8Ch = reader.ReadUInt32();
            this.Unknown_90h = reader.ReadUInt32();
            this.Unknown_94h = reader.ReadUInt32();

            // read reference data
            this.VertexBuffer = reader.ReadBlockAt<VertexBuffer>(
                this.VertexBufferPointer // offset
            );
            this.IndexBuffer = reader.ReadBlockAt<IndexBuffer>(
                this.IndexBufferPointer // offset
            );
            this.BoneIds = reader.ReadUshortsAt(this.BoneIdsPointer, this.BoneIdsCount);
            if (this.BoneIds != null) //skinned mesh bones to use? peds, also yft props...
            {
            }

            if (this.VertexBuffer != null)
            {
                this.VertexData = this.VertexBuffer.Data1;
                if (this.VertexData == null)
                {
                    this.VertexData = this.VertexBuffer.Data2;
                }
                if ((this.VertexDataPointer != 0) && (VertexDataPointer != VertexBuffer.DataPointer1))
                {
                    //some mods hit here!
                //    try
                //    {
                //        this.VertexData = reader.ReadBlockAt<VertexData>(
                //            this.VertexDataPointer, // offset
                //            this.VertexStride,
                //            this.VerticesCount,
                //            this.VertexBuffer.Info
                //        );
                //    }
                //    catch
                //    { }
                }
            }
            else
            { }
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.VertexBufferPointer = (ulong)(this.VertexBuffer != null ? this.VertexBuffer.FilePosition : 0);
            this.IndexBufferPointer = (ulong)(this.IndexBuffer != null ? this.IndexBuffer.FilePosition : 0);
            this.BoneIdsPointer = (ulong)(this.BoneIdsBlock != null ? this.BoneIdsBlock.FilePosition : 0);
            this.VerticesCount = (ushort)(this.VertexData != null ? this.VertexData.VertexCount : 0); //TODO: fix?
            this.BoneIdsCount = (ushort)(this.BoneIdsBlock != null ? this.BoneIdsBlock.ItemCount : 0);
            this.VertexDataPointer = (ulong)(this.VertexData != null ? this.VertexData.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.VertexBufferPointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.IndexBufferPointer);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.IndicesCount);
            writer.Write(this.TrianglesCount);
            writer.Write(this.VerticesCount);
            writer.Write(this.Unknown_62h);
            writer.Write(this.Unknown_64h);
            writer.Write(this.BoneIdsPointer);
            writer.Write(this.VertexStride);
            writer.Write(this.BoneIdsCount);
            writer.Write(this.Unknown_74h);
            writer.Write(this.VertexDataPointer);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_84h);
            writer.Write(this.Unknown_88h);
            writer.Write(this.Unknown_8Ch);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_94h);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (VertexBuffer != null) list.Add(VertexBuffer);
            if (IndexBuffer != null) list.Add(IndexBuffer);
            if (BoneIds != null)
            {
                BoneIdsBlock = new ResourceSystemStructBlock<ushort>(BoneIds);
                list.Add(BoneIdsBlock);
            }
            if (VertexData != null) list.Add(VertexData);
            return list.ToArray();
        }

        public override string ToString()
        {
            return VerticesCount.ToString() + " verts, " + Shader.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class VertexBuffer : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 128; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public ushort VertexStride { get; set; }
        public ushort Unknown_Ah { get; set; }
        public uint Unknown_Ch { get; set; } // 0x00000000
        public ulong DataPointer1 { get; set; }
        public uint VertexCount { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ulong DataPointer2 { get; set; }
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ulong InfoPointer { get; set; }
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
        public uint Unknown_60h { get; set; } // 0x00000000
        public uint Unknown_64h { get; set; } // 0x00000000
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public uint Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000

        // reference data
        public VertexData Data1 { get; set; }
        public VertexData Data2 { get; set; }
        public VertexDeclaration Info { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.VertexStride = reader.ReadUInt16();
            this.Unknown_Ah = reader.ReadUInt16();
            this.Unknown_Ch = reader.ReadUInt32();
            this.DataPointer1 = reader.ReadUInt64();
            this.VertexCount = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.DataPointer2 = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.InfoPointer = reader.ReadUInt64();
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
            this.Unknown_60h = reader.ReadUInt32();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.Unknown_78h = reader.ReadUInt32();
            this.Unknown_7Ch = reader.ReadUInt32();

            // read reference data
            this.Info = reader.ReadBlockAt<VertexDeclaration>(
                this.InfoPointer // offset
            );
            this.Data1 = reader.ReadBlockAt<VertexData>(
                this.DataPointer1, // offset
                this.VertexStride,
                this.VertexCount,
                this.Info
            );
            this.Data2 = reader.ReadBlockAt<VertexData>(
                this.DataPointer2, // offset
                this.VertexStride,
                this.VertexCount,
                this.Info
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.VertexCount = (uint)(this.Data1 != null ? this.Data1.VertexCount : 0);
            this.DataPointer1 = (ulong)(this.Data1 != null ? this.Data1.FilePosition : 0);
            this.DataPointer2 = (ulong)(this.Data2 != null ? this.Data2.FilePosition : 0);
            this.InfoPointer = (ulong)(this.Info != null ? this.Info.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.VertexStride);
            writer.Write(this.Unknown_Ah);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.DataPointer1);
            writer.Write(this.VertexCount);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.DataPointer2);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.InfoPointer);
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
            if (Data1 != null) list.Add(Data1);
            if (Data2 != null) list.Add(Data2);
            if (Info != null) list.Add(Info);
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class VertexData : ResourceSystemBlock
    {


        //private int length = 0;
        public override long BlockLength
        {
            get
            {
                return VertexBytes?.Length ?? 0; //this.length;
            }
        }



        public VertexDeclaration info { get; set; }
        public object[] Data { get; set; }
        public uint[] Types { get; set; }


        public VertexType VertexType { get; set; }
        public byte[] VertexBytes { get; set; }

        public int VertexCount { get; set; }
        public int VertexStride { get; set; }

        public long MemoryUsage
        {
            get
            {
                return (long)VertexCount * (long)VertexStride;
            }
        }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            VertexStride = Convert.ToInt32(parameters[0]);
            VertexCount = Convert.ToInt32(parameters[1]);
            info = (VertexDeclaration)parameters[2];

            VertexType = (VertexType)info.Flags;

            VertexBytes = reader.ReadBytes(VertexCount * VertexStride);


            switch (info.Types)
            {
                case 8598872888530528662: //YDR - 0x7755555555996996
                    break;
                case 216172782140628998:  //YFT - 0x030000000199A006
                    switch (info.Flags)
                    {
                        case 16473: VertexType = VertexType.PCCH2H4; break;  //  PCCH2H4 
                        default:break;
                    }
                    break;
                case 216172782140612614:  //YFT - 0x0300000001996006  PNCH2H4
                    switch (info.Flags)
                    {
                        case 89: VertexType = VertexType.PNCH2; break;     //  PNCH2
                        default: break;
                    }
                    break;
                default:
                    break;
            }

        }

        public /*override*/ void ReadOrig(ResourceDataReader reader, params object[] parameters)
        {
            int stride = Convert.ToInt32(parameters[0]);
            int count = Convert.ToInt32(parameters[1]);
            var info = (VertexDeclaration)parameters[2];
            this.VertexCount = count;
            this.info = info;


            bool[] IsUsed = new bool[16];
            for (int i = 0; i < 16; i++)
                IsUsed[i] = ((info.Flags >> i) & 0x1) == 1;

            Types = new uint[16];
            for (int i = 0; i < 16; i++)
                Types[i] = (uint)((info.Types >> (int)(4 * i)) & 0xF);



            Data = new object[16];
            for (int i = 0; i < 16; i++)
            {
                if (IsUsed[i])
                {
                    switch (Types[i])
                    {
                        case 0: Data[i] = new ushort[1 * count]; break;
                        case 1: Data[i] = new ushort[2 * count]; break;
                        case 2: Data[i] = new ushort[3 * count]; break;
                        case 3: Data[i] = new ushort[4 * count]; break;
                        case 4: Data[i] = new float[1 * count]; break;
                        case 5: Data[i] = new float[2 * count]; break;
                        case 6: Data[i] = new float[3 * count]; break;
                        case 7: Data[i] = new float[4 * count]; break;
                        case 8: Data[i] = new uint[count]; break;
                        case 9: Data[i] = new uint[count]; break;
                        case 10: Data[i] = new uint[count]; break;
                        default:
                            throw new Exception();
                    }
                }
            }



            long pos = reader.Position;

            // read...
            for (int i = 0; i < count; i++)
            {

                for (int k = 0; k < 16; k++)
                {
                    if (IsUsed[k])
                    {
                        switch (Types[k])
                        {
                            // float16
                            case 0:
                                {
                                    var buf = Data[k] as ushort[];
                                    buf[i * 1 + 0] = reader.ReadUInt16();
                                    break;
                                }
                            case 1:
                                {
                                    var buf = Data[k] as ushort[];
                                    buf[i * 2 + 0] = reader.ReadUInt16();
                                    buf[i * 2 + 1] = reader.ReadUInt16();
                                    break;
                                }
                            case 2:
                                {
                                    var buf = Data[k] as ushort[];
                                    buf[i * 3 + 0] = reader.ReadUInt16();
                                    buf[i * 3 + 1] = reader.ReadUInt16();
                                    buf[i * 3 + 2] = reader.ReadUInt16();
                                    break;
                                }
                            case 3:
                                {
                                    var buf = Data[k] as ushort[];
                                    buf[i * 4 + 0] = reader.ReadUInt16();
                                    buf[i * 4 + 1] = reader.ReadUInt16();
                                    buf[i * 4 + 2] = reader.ReadUInt16();
                                    buf[i * 4 + 3] = reader.ReadUInt16();
                                    break;
                                }

                            // float32
                            case 4:
                                {
                                    var buf = Data[k] as float[];
                                    buf[i * 1 + 0] = reader.ReadSingle();
                                    break;
                                }
                            case 5:
                                {
                                    var buf = Data[k] as float[];
                                    buf[i * 2 + 0] = reader.ReadSingle();
                                    buf[i * 2 + 1] = reader.ReadSingle();
                                    break;
                                }
                            case 6:
                                {
                                    var buf = Data[k] as float[];
                                    buf[i * 3 + 0] = reader.ReadSingle();
                                    buf[i * 3 + 1] = reader.ReadSingle();
                                    buf[i * 3 + 2] = reader.ReadSingle();
                                    break;
                                }
                            case 7:
                                {
                                    var buf = Data[k] as float[];
                                    buf[i * 4 + 0] = reader.ReadSingle();
                                    buf[i * 4 + 1] = reader.ReadSingle();
                                    buf[i * 4 + 2] = reader.ReadSingle();
                                    buf[i * 4 + 3] = reader.ReadSingle();
                                    break;
                                }

                            case 8:
                            case 9:
                            case 10:
                                {
                                    var buf = Data[k] as uint[];
                                    buf[i * 1 + 0] = reader.ReadUInt32();
                                    break;
                                }

                            default:
                                throw new Exception();
                        }
                    }
                }

            }

            //this.length = stride * count;
        }

        public /*override*/ void WriteOrig(ResourceDataWriter writer, params object[] parameters)
        {

            // write...
            for (int i = 0; i < VertexCount; i++)
            {

                for (int k = 0; k < 16; k++)
                {
                    if (Data[k] != null)
                    {
                        switch (Types[k])
                        {
                            // float16
                            case 0:
                                {
                                    var buf = Data[k] as ushort[];
                                    writer.Write(buf[i * 1 + 0]);
                                    break;
                                }
                            case 1:
                                {
                                    var buf = Data[k] as ushort[];
                                    writer.Write(buf[i * 2 + 0]);
                                    writer.Write(buf[i * 2 + 1]);
                                    break;
                                }
                            case 2:
                                {
                                    var buf = Data[k] as ushort[];
                                    writer.Write(buf[i * 3 + 0]);
                                    writer.Write(buf[i * 3 + 1]);
                                    writer.Write(buf[i * 3 + 2]);
                                    break;
                                }
                            case 3:
                                {
                                    var buf = Data[k] as ushort[];
                                    writer.Write(buf[i * 4 + 0]);
                                    writer.Write(buf[i * 4 + 1]);
                                    writer.Write(buf[i * 4 + 2]);
                                    writer.Write(buf[i * 4 + 3]);
                                    break;
                                }

                            // float32
                            case 4:
                                {
                                    var buf = Data[k] as float[];
                                    writer.Write(buf[i * 1 + 0]);
                                    break;
                                }
                            case 5:
                                {
                                    var buf = Data[k] as float[];
                                    writer.Write(buf[i * 2 + 0]);
                                    writer.Write(buf[i * 2 + 1]);
                                    break;
                                }
                            case 6:
                                {
                                    var buf = Data[k] as float[];
                                    writer.Write(buf[i * 3 + 0]);
                                    writer.Write(buf[i * 3 + 1]);
                                    writer.Write(buf[i * 3 + 2]);
                                    break;
                                }
                            case 7:
                                {
                                    var buf = Data[k] as float[];
                                    writer.Write(buf[i * 4 + 0]);
                                    writer.Write(buf[i * 4 + 1]);
                                    writer.Write(buf[i * 4 + 2]);
                                    writer.Write(buf[i * 4 + 3]);
                                    break;
                                }

                            case 8:
                            case 9:
                            case 10:
                                {
                                    var buf = Data[k] as uint[];
                                    writer.Write(buf[i * 1 + 0]);
                                    break;
                                }

                            default:
                                throw new Exception();
                        }
                    }
                }

            }

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            if (VertexBytes != null)
            {
                writer.Write(VertexBytes); //not dealing with individual vertex data here any more!
            }
        }

        public override string ToString()
        {
            return "Type: " + VertexType.ToString() + ", Count: " + VertexCount.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class VertexDeclaration : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public uint Flags { get; set; }
        public ushort Stride { get; set; }
        public byte Unknown_6h { get; set; }
        public byte Count { get; set; }
        public ulong Types { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Flags = reader.ReadUInt32();
            this.Stride = reader.ReadUInt16();
            this.Unknown_6h = reader.ReadByte();
            this.Count = reader.ReadByte();
            this.Types = reader.ReadUInt64();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Flags);
            writer.Write(this.Stride);
            writer.Write(this.Unknown_6h);
            writer.Write(this.Count);
            writer.Write(this.Types);
        }

        public ulong GetDeclarationId()
        {
            ulong res = 0;
            for(int i=0; i < 16; i++)
            {
                if (((Flags >> i) & 1) == 1)
                {
                    res += (Types & (0xFu << (i * 4)));
                }
            }
            return res;
        }

        public override string ToString()
        {
            return Stride.ToString() + ": " + Count.ToString() + ": " + Flags.ToString() + ": " + Types.ToString(); 
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class IndexBuffer : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 96; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint IndicesCount { get; set; }
        public uint Unknown_Ch { get; set; } // 0x00000000
        public ulong IndicesPointer { get; set; }
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
        public uint Unknown_50h { get; set; } // 0x00000000
        public uint Unknown_54h { get; set; } // 0x00000000
        public uint Unknown_58h { get; set; } // 0x00000000
        public uint Unknown_5Ch { get; set; } // 0x00000000

        // reference data
        //public ResourceSimpleArray<ushort_r> Indices;
        public ushort[] Indices { get; set; }


        private ResourceSystemStructBlock<ushort> IndicesBlock = null; //only used when saving


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.IndicesCount = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.IndicesPointer = reader.ReadUInt64();
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
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.Unknown_58h = reader.ReadUInt32();
            this.Unknown_5Ch = reader.ReadUInt32();

            // read reference data
            //this.Indices = reader.ReadBlockAt<ResourceSimpleArray<ushort_r>>(
            //    this.IndicesPointer, // offset
            //    this.IndicesCount
            //);
            this.Indices = reader.ReadUshortsAt(this.IndicesPointer, this.IndicesCount);
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.IndicesCount = (uint)(this.IndicesBlock != null ? this.IndicesBlock.ItemCount : 0);
            this.IndicesPointer = (ulong)(this.IndicesBlock != null ? this.IndicesBlock.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.IndicesCount);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.IndicesPointer);
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
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Indices != null)
            {
                IndicesBlock = new ResourceSystemStructBlock<ushort>(Indices);
                list.Add(IndicesBlock);
            }
            return list.ToArray();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public struct LightAttributes_s
    {
        // structure data
        public uint Unknown_0h { get; set; } // 0x00000000
        public uint Unknown_4h { get; set; } // 0x00000000
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        public byte Flashiness { get; set; }
        public float Intensity { get; set; }
        public uint Flags { get; set; }
        public ushort BoneId { get; set; }
        public byte Type { get; set; }
        public byte GroupId { get; set; }
        public uint TimeFlags { get; set; }
        public float Falloff { get; set; }
        public float FalloffExponent { get; set; }
        public float CullingPlaneNormalX { get; set; }
        public float CullingPlaneNormalY { get; set; }
        public float CullingPlaneNormalZ { get; set; }
        public float CullingPlaneOffset { get; set; }
        public byte ShadowBlur { get; set; }
        public byte Unknown_45h { get; set; }
        public ushort Unknown_46h { get; set; }
        public uint Unknown_48h { get; set; } // 0x00000000
        public float VolumeIntensity { get; set; }
        public float VolumeSizeScale { get; set; }
        public byte VolumeOuterColorR { get; set; }
        public byte VolumeOuterColorG { get; set; }
        public byte VolumeOuterColorB { get; set; }
        public byte LightHash { get; set; }
        public float VolumeOuterIntensity { get; set; }
        public float CoronaSize { get; set; }
        public float VolumeOuterExponent { get; set; }
        public byte LightFadeDistance { get; set; }
        public byte ShadowFadeDistance { get; set; }
        public byte SpecularFadeDistance { get; set; }
        public byte VolumetricFadeDistance { get; set; }
        public float ShadowNearClip { get; set; }
        public float CoronaIntensity { get; set; }
        public float CoronaZBias { get; set; }
        public float DirectionX { get; set; }
        public float DirectionY { get; set; }
        public float DirectionZ { get; set; }
        public float TangentX { get; set; }
        public float TangentY { get; set; }
        public float TangentZ { get; set; }
        public float ConeInnerAngle { get; set; }
        public float ConeOuterAngle { get; set; }
        public float ExtentX { get; set; }
        public float ExtentY { get; set; }
        public float ExtentZ { get; set; }
        public uint ProjectedTextureHash { get; set; }
        public uint Unknown_A4h { get; set; } // 0x00000000
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class DrawableBase : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 168; }
        }

        // structure data
        public ulong ShaderGroupPointer { get; set; }
        public ulong SkeletonPointer { get; set; }
        public Vector3 BoundingCenter { get; set; }
        public float BoundingSphereRadius { get; set; }
        public Vector4 BoundingBoxMin { get; set; }
        public Vector4 BoundingBoxMax { get; set; }
        public ulong DrawableModelsHighPointer { get; set; }
        public ulong DrawableModelsMediumPointer { get; set; }
        public ulong DrawableModelsLowPointer { get; set; }
        public ulong DrawableModelsVeryLowPointer { get; set; }
        public float LodDistHigh { get; set; }
        public float LodDistMed { get; set; }
        public float LodDistLow { get; set; }
        public float LodDistVlow { get; set; }
        public uint Unknown_80h { get; set; }
        public uint Unknown_84h { get; set; }
        public uint Unknown_88h { get; set; }
        public uint Unknown_8Ch { get; set; }
        public ulong JointsPointer { get; set; }
        public ushort Unknown_98h { get; set; }
        public ushort Unknown_9Ah { get; set; }
        public uint Unknown_9Ch { get; set; } // 0x00000000
        public ulong DrawableModelsXPointer { get; set; }

        // reference data
        public ShaderGroup ShaderGroup { get; set; }
        public Skeleton Skeleton { get; set; }
        public ResourcePointerList64<DrawableModel> DrawableModelsHigh { get; set; }
        public ResourcePointerList64<DrawableModel> DrawableModelsMedium { get; set; }
        public ResourcePointerList64<DrawableModel> DrawableModelsLow { get; set; }
        public ResourcePointerList64<DrawableModel> DrawableModelsVeryLow { get; set; }
        public Joints Joints { get; set; }
        public ResourcePointerList64<DrawableModel> DrawableModelsX { get; set; }

        public DrawableModel[] AllModels { get; set; }
        public Dictionary<ulong, VertexDeclaration> VertexDecls { get; set; }

        public object Owner { get; set; }

        public long MemoryUsage
        {
            get
            {
                long val = 0;
                if (AllModels != null)
                {
                    foreach(DrawableModel m in AllModels)
                    {
                        if (m != null)
                        {
                            val += m.MemoryUsage;
                        }
                    }
                }
                if ((ShaderGroup != null) && (ShaderGroup.TextureDictionary != null))
                {
                    val += ShaderGroup.TextureDictionary.MemoryUsage;
                }
                return val;
            }
        }


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.ShaderGroupPointer = reader.ReadUInt64();
            this.SkeletonPointer = reader.ReadUInt64();
            this.BoundingCenter = reader.ReadStruct<Vector3>();
            this.BoundingSphereRadius = reader.ReadSingle();
            this.BoundingBoxMin = reader.ReadStruct<Vector4>();
            this.BoundingBoxMax = reader.ReadStruct<Vector4>();
            this.DrawableModelsHighPointer = reader.ReadUInt64();
            this.DrawableModelsMediumPointer = reader.ReadUInt64();
            this.DrawableModelsLowPointer = reader.ReadUInt64();
            this.DrawableModelsVeryLowPointer = reader.ReadUInt64();
            this.LodDistHigh = reader.ReadSingle();
            this.LodDistMed = reader.ReadSingle();
            this.LodDistLow = reader.ReadSingle();
            this.LodDistVlow = reader.ReadSingle();
            this.Unknown_80h = reader.ReadUInt32();
            this.Unknown_84h = reader.ReadUInt32();
            this.Unknown_88h = reader.ReadUInt32();
            this.Unknown_8Ch = reader.ReadUInt32();
            this.JointsPointer = reader.ReadUInt64();
            this.Unknown_98h = reader.ReadUInt16();
            this.Unknown_9Ah = reader.ReadUInt16();
            this.Unknown_9Ch = reader.ReadUInt32();
            this.DrawableModelsXPointer = reader.ReadUInt64();

            // read reference data
            this.ShaderGroup = reader.ReadBlockAt<ShaderGroup>(
                this.ShaderGroupPointer // offset
            );
            this.Skeleton = reader.ReadBlockAt<Skeleton>(
                this.SkeletonPointer // offset
            );
            this.DrawableModelsHigh = reader.ReadBlockAt<ResourcePointerList64<DrawableModel>>(
                this.DrawableModelsHighPointer // offset
            );
            this.DrawableModelsMedium = reader.ReadBlockAt<ResourcePointerList64<DrawableModel>>(
                this.DrawableModelsMediumPointer // offset
            );
            this.DrawableModelsLow = reader.ReadBlockAt<ResourcePointerList64<DrawableModel>>(
                this.DrawableModelsLowPointer // offset
            );
            this.DrawableModelsVeryLow = reader.ReadBlockAt<ResourcePointerList64<DrawableModel>>(
                this.DrawableModelsVeryLowPointer // offset
            );
            this.Joints = reader.ReadBlockAt<Joints>(
                this.JointsPointer // offset
            );
            this.DrawableModelsX = reader.ReadBlockAt<ResourcePointerList64<DrawableModel>>(
                this.DrawableModelsXPointer // offset
            );


            var allModels = new List<DrawableModel>();
            if (DrawableModelsHigh != null) allModels.AddRange(DrawableModelsHigh.data_items);
            if (DrawableModelsMedium != null) allModels.AddRange(DrawableModelsMedium.data_items);
            if (DrawableModelsLow != null) allModels.AddRange(DrawableModelsLow.data_items);
            if (DrawableModelsVeryLow != null) allModels.AddRange(DrawableModelsVeryLow.data_items);
            if ((DrawableModelsX != null) && (DrawableModelsX != DrawableModelsHigh))
            {
                allModels.AddRange(DrawableModelsX.data_items);
            }
            AllModels = allModels.ToArray();


            var vds = new Dictionary<ulong, VertexDeclaration>();
            foreach (DrawableModel model in AllModels)
            {
                foreach (var geom in model.Geometries.data_items)
                {
                    var info = geom.VertexBuffer.Info;
                    var declid = info.GetDeclarationId();

                    if (!vds.ContainsKey(declid))
                    {
                        vds.Add(declid, info);
                    }
                    //else //debug test
                    //{
                    //    if ((VertexDecls[declid].Stride != info.Stride)||(VertexDecls[declid].Types != info.Types))
                    //    {
                    //    }
                    //}
                }
            }
            VertexDecls = new Dictionary<ulong, VertexDeclaration>(vds);

            AssignGeometryShaders(ShaderGroup);
        }

        public void AssignGeometryShaders(ShaderGroup shaderGrp)
        {
            if (shaderGrp != null)
            {
                ShaderGroup = shaderGrp;
            }

            //map the shaders to the geometries
            if (ShaderGroup != null)
            {
                var shaders = ShaderGroup.Shaders.data_items;
                foreach (DrawableModel model in AllModels)
                {
                    if (model.Geometries == null) continue;
                    if (model.Geometries.data_items == null) continue;
                    if (model.ShaderMapping == null) continue;

                    int geomcount = model.Geometries.data_items.Length;
                    for (int i = 0; i < geomcount; i++)
                    {
                        var geom = model.Geometries.data_items[i];
                        ushort sid = (i < model.ShaderMapping.Length) ? model.ShaderMapping[i] : (ushort)0;
                        geom.Shader = (sid < shaders.Length) ? shaders[sid] : null;
                    }
                }
            }
            else
            {
            }

        }


        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.ShaderGroupPointer = (ulong)(this.ShaderGroup != null ? this.ShaderGroup.FilePosition : 0);
            this.SkeletonPointer = (ulong)(this.Skeleton != null ? this.Skeleton.FilePosition : 0);
            this.DrawableModelsHighPointer = (ulong)(this.DrawableModelsHigh != null ? this.DrawableModelsHigh.FilePosition : 0);
            this.DrawableModelsMediumPointer = (ulong)(this.DrawableModelsMedium != null ? this.DrawableModelsMedium.FilePosition : 0);
            this.DrawableModelsLowPointer = (ulong)(this.DrawableModelsLow != null ? this.DrawableModelsLow.FilePosition : 0);
            this.DrawableModelsVeryLowPointer = (ulong)(this.DrawableModelsVeryLow != null ? this.DrawableModelsVeryLow.FilePosition : 0);
            this.JointsPointer = (ulong)(this.Joints != null ? this.Joints.FilePosition : 0);
            this.DrawableModelsXPointer = (ulong)(this.DrawableModelsX != null ? this.DrawableModelsX.FilePosition : 0);

            // write structure data
            writer.Write(this.ShaderGroupPointer);
            writer.Write(this.SkeletonPointer);
            writer.Write(this.BoundingCenter);
            writer.Write(this.BoundingSphereRadius);
            writer.Write(this.BoundingBoxMin);
            writer.Write(this.BoundingBoxMax);
            writer.Write(this.DrawableModelsHighPointer);
            writer.Write(this.DrawableModelsMediumPointer);
            writer.Write(this.DrawableModelsLowPointer);
            writer.Write(this.DrawableModelsVeryLowPointer);
            writer.Write(this.LodDistHigh);
            writer.Write(this.LodDistMed);
            writer.Write(this.LodDistLow);
            writer.Write(this.LodDistVlow);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_84h);
            writer.Write(this.Unknown_88h);
            writer.Write(this.Unknown_8Ch);
            writer.Write(this.JointsPointer);
            writer.Write(this.Unknown_98h);
            writer.Write(this.Unknown_9Ah);
            writer.Write(this.Unknown_9Ch);
            writer.Write(this.DrawableModelsXPointer);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (ShaderGroup != null) list.Add(ShaderGroup);
            if (Skeleton != null) list.Add(Skeleton);
            if (DrawableModelsHigh != null) list.Add(DrawableModelsHigh);
            if (DrawableModelsMedium != null) list.Add(DrawableModelsMedium);
            if (DrawableModelsLow != null) list.Add(DrawableModelsLow);
            if (DrawableModelsVeryLow != null) list.Add(DrawableModelsVeryLow);
            if (Joints != null) list.Add(Joints);
            if (DrawableModelsX != null) list.Add(DrawableModelsX);
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class Drawable : DrawableBase
    {
        public override long BlockLength
        {
            get { return 208; }
        }

        // structure data
        public ulong NamePointer { get; set; }
        public ResourceSimpleList64_s<LightAttributes_s> LightAttributes { get; set; }
        public uint Unknown_C0h { get; set; } // 0x00000000
        public uint Unknown_C4h { get; set; } // 0x00000000
        public ulong BoundPointer { get; set; }

        // reference data
        public string Name { get; set; }
        public Bounds Bound { get; set; }

        public string ErrorMessage { get; set; }


        private string_r NameBlock = null;//only used when saving..


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.NamePointer = reader.ReadUInt64();
            this.LightAttributes = reader.ReadBlock<ResourceSimpleList64_s<LightAttributes_s>>();
            this.Unknown_C0h = reader.ReadUInt32();
            this.Unknown_C4h = reader.ReadUInt32();
            this.BoundPointer = reader.ReadUInt64();

            try
            {

                // read reference data
                this.Name = reader.ReadStringAt(//BlockAt<string_r>(
                    this.NamePointer // offset
                );

                this.Bound = reader.ReadBlockAt<Bounds>(
                    this.BoundPointer // offset
                );
            }
            catch (Exception ex) //sometimes error here for loading particles! different drawable type? base only?
            {
                ErrorMessage = ex.ToString();
            }
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.NamePointer = (ulong)(this.NameBlock != null ? this.NameBlock.FilePosition : 0);
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);

            // write structure data
            writer.Write(this.NamePointer);
            writer.WriteBlock(this.LightAttributes);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C4h);
            writer.Write(this.BoundPointer);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Name != null)
            {
                NameBlock = (string_r)Name;
                list.Add(NameBlock);
            }
            if (Bound != null) list.Add(Bound);
            return list.ToArray();
        }
        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0xB0, LightAttributes),
            };
        }


        public override string ToString()
        {
            return Name;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class DrawableBaseDictionary : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }
        public ulong HashesPointer { get; set; }
        public ushort HashesCount1 { get; set; }
        public ushort HashesCount2 { get; set; }
        public uint Unknown_2Ch { get; set; }
        public ulong DrawablesPointer { get; set; }
        public ushort DrawablesCount1 { get; set; }
        public ushort DrawablesCount2 { get; set; }
        public uint Unknown_3Ch { get; set; }

        // reference data
        //public ResourceSimpleArray<uint_r> Hashes { get; set; }
        public uint[] Hashes { get; set; }
        public ResourcePointerArray64<DrawableBase> Drawables { get; set; }


        private ResourceSystemStructBlock<uint> HashesBlock = null;//only used for saving


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
            this.HashesPointer = reader.ReadUInt64();
            this.HashesCount1 = reader.ReadUInt16();
            this.HashesCount2 = reader.ReadUInt16();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.DrawablesPointer = reader.ReadUInt64();
            this.DrawablesCount1 = reader.ReadUInt16();
            this.DrawablesCount2 = reader.ReadUInt16();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            //this.Hashes = reader.ReadBlockAt<ResourceSimpleArray<uint_r>>(
            //    this.HashesPointer, // offset
            //    this.HashesCount1
            //);
            this.Hashes = reader.ReadUintsAt(this.HashesPointer, this.HashesCount1);

            this.Drawables = reader.ReadBlockAt<ResourcePointerArray64<DrawableBase>>(
                this.DrawablesPointer, // offset
                this.DrawablesCount1
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.HashesPointer = (ulong)(this.HashesBlock != null ? this.HashesBlock.FilePosition : 0);
            this.HashesCount1 = (ushort)(this.HashesBlock != null ? this.HashesBlock.ItemCount : 0);
            this.HashesCount2 = (ushort)(this.HashesBlock != null ? this.HashesBlock.ItemCount : 0);
            this.DrawablesPointer = (ulong)(this.Drawables != null ? this.Drawables.FilePosition : 0);
            this.DrawablesCount1 = (ushort)(this.Drawables != null ? this.Drawables.Count : 0);
            this.DrawablesCount2 = (ushort)(this.Drawables != null ? this.Drawables.Count : 0);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.HashesPointer);
            writer.Write(this.HashesCount1);
            writer.Write(this.HashesCount2);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.DrawablesPointer);
            writer.Write(this.DrawablesCount1);
            writer.Write(this.DrawablesCount2);
            writer.Write(this.Unknown_3Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Hashes != null)
            {
                HashesBlock = new ResourceSystemStructBlock<uint>(Hashes);
                list.Add(HashesBlock);
            }
            if (Drawables != null) list.Add(Drawables);
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class DrawableDictionary : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000001
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ulong HashesPointer { get; set; }
        public ushort HashesCount1 { get; set; }
        public ushort HashesCount2 { get; set; }
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ulong DrawablesPointer { get; set; }
        public ushort DrawablesCount1 { get; set; }
        public ushort DrawablesCount2 { get; set; }
        public uint Unknown_3Ch { get; set; } // 0x00000000

        // reference data
        //public ResourceSimpleArray<uint_r> Hashes { get; set; }
        public uint[] Hashes { get; set; }
        public ResourcePointerArray64<Drawable> Drawables { get; set; }


        private ResourceSystemStructBlock<uint> HashesBlock = null;//only used for saving


        public long MemoryUsage
        {
            get
            {
                long val = 0;
                if ((Drawables != null) && (Drawables.data_items != null))
                {
                    foreach(var drawable in Drawables.data_items)
                    {
                        val += drawable.MemoryUsage;
                    }
                }
                return val;
            }
        }

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
            this.HashesPointer = reader.ReadUInt64();
            this.HashesCount1 = reader.ReadUInt16();
            this.HashesCount2 = reader.ReadUInt16();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.DrawablesPointer = reader.ReadUInt64();
            this.DrawablesCount1 = reader.ReadUInt16();
            this.DrawablesCount2 = reader.ReadUInt16();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            this.Hashes = reader.ReadUintsAt(this.HashesPointer, this.HashesCount1);

            this.Drawables = reader.ReadBlockAt<ResourcePointerArray64<Drawable>>(
                this.DrawablesPointer, // offset
                this.DrawablesCount1
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.HashesPointer = (ulong)(this.HashesBlock != null ? this.HashesBlock.FilePosition : 0);
            this.HashesCount1 = (ushort)(this.HashesBlock != null ? this.HashesBlock.ItemCount : 0);
            this.HashesCount2 = (ushort)(this.HashesBlock != null ? this.HashesBlock.ItemCount : 0);
            this.DrawablesPointer = (ulong)(this.Drawables != null ? this.Drawables.FilePosition : 0);
            this.DrawablesCount1 = (ushort)(this.Drawables != null ? this.Drawables.Count : 0);
            this.DrawablesCount2 = (ushort)(this.Drawables != null ? this.Drawables.Count : 0);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.HashesPointer);
            writer.Write(this.HashesCount1);
            writer.Write(this.HashesCount2);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.DrawablesPointer);
            writer.Write(this.DrawablesCount1);
            writer.Write(this.DrawablesCount2);
            writer.Write(this.Unknown_3Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Hashes != null)
            {
                HashesBlock = new ResourceSystemStructBlock<uint>(Hashes);
                list.Add(HashesBlock);
            }
            if (Drawables != null) list.Add(Drawables);
            return list.ToArray();
        }
    }


}
