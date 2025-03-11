using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ShaderGroup : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint VFT { get; set; } = 1080113136;
        public uint Unknown_4h = 1; // 0x00000001
        public ulong TextureDictionaryPointer { get; set; }
        public ulong ShadersPointer { get; set; }
        public ushort ShadersCount1 { get; set; }
        public ushort ShadersCount2 { get; set; }
        public uint Unknown_1Ch; // 0x00000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public uint ShaderGroupBlocksSize { get; set; } // divided by 16
        public uint Unknown_34h; // 0x00000000
        public ulong Unknown_38h; // 0x0000000000000000

        // reference data
        public TextureDictionary TextureDictionary { get; set; }
        public ResourcePointerArray64<ShaderFX> Shaders { get; set; }


        public int TotalParameters
        {
            get
            {
                int c = 0;
                if (Shaders?.data_items != null)
                {
                    foreach (var s in Shaders.data_items)
                    {
                        c += s.ParameterCount;
                    }
                }
                return c;
            }
        }


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
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.ShaderGroupBlocksSize = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt64();

            // read reference data
            this.TextureDictionary = reader.ReadBlockAt<TextureDictionary>(
                this.TextureDictionaryPointer // offset
            );
            this.Shaders = reader.ReadBlockAt<ResourcePointerArray64<ShaderFX>>(
                this.ShadersPointer, // offset
                this.ShadersCount1
            );

            //if (Unknown_4h != 1)
            //{ }
            //if (Unknown_1Ch != 0)
            //{ }
            //if (Unknown_20h != 0)
            //{ }
            //if (Unknown_28h != 0)
            //{ }
            //if (Unknown_34h != 0)
            //{ }
            //if (Unknown_38h != 0)
            //{ }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.TextureDictionaryPointer = (ulong)(this.TextureDictionary != null ? this.TextureDictionary.FilePosition : 0);
            this.ShadersPointer = (ulong)(this.Shaders != null ? this.Shaders.FilePosition : 0);
            this.ShadersCount1 = (ushort)(this.Shaders != null ? this.Shaders.Count : 0);
            this.ShadersCount2 = this.ShadersCount1;
            // In vanilla files this includes the size of the Shaders array, ShaderFX blocks and, sometimes,
            // ShaderParametersBlocks since they are placed contiguously after the ShaderGroup in the file.
            // But CW doesn't always do this so we only include the ShaderGroup size.
            this.ShaderGroupBlocksSize = (uint)this.BlockLength / 16;

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.TextureDictionaryPointer);
            writer.Write(this.ShadersPointer);
            writer.Write(this.ShadersCount1);
            writer.Write(this.ShadersCount2);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.ShaderGroupBlocksSize);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            if (TextureDictionary != null)
            {
                TextureDictionary.WriteXmlNode(TextureDictionary, sb, indent, ddsfolder, "TextureDictionary");
            }
            YdrXml.WriteItemArray(sb, Shaders?.data_items, indent, "Shaders");
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            var tnode = node.SelectSingleNode("TextureDictionary");
            if (tnode != null)
            {
                TextureDictionary = TextureDictionary.ReadXmlNode(tnode, ddsfolder);
            }
            var shaders = XmlMeta.ReadItemArray<ShaderFX>(node, "Shaders");
            if (shaders != null)
            {
                Shaders = new ResourcePointerArray64<ShaderFX>();
                Shaders.data_items = shaders;
            }


            if ((shaders != null) && (TextureDictionary != null))
            {
                foreach (var shader in shaders)
                {
                    var sparams = shader?.ParametersList?.Parameters;
                    if (sparams != null)
                    {
                        foreach (var sparam in sparams)
                        {
                            if (sparam.Data is TextureBase tex)
                            {
                                var tex2 = TextureDictionary.Lookup(tex.NameHash);
                                if (tex2 != null)
                                {
                                    sparam.Data = tex2;//swap the parameter out for the embedded texture
                                }
                            }
                        }
                    }
                }
            }
        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (TextureDictionary != null) list.Add(TextureDictionary);
            if (Shaders != null) list.Add(Shaders);
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ShaderFX : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength => 48;
        public override long BlockLength_Gen9 => 64;

        // structure data
        public ulong ParametersPointer { get; set; }
        public MetaHash Name { get; set; } //decal_emissive_only, emissive, spec
        public uint Unknown_Ch; // 0x00000000
        public byte ParameterCount { get; set; }
        public byte RenderBucket { get; set; } // 2, 0, 
        public ushort Unknown_12h { get; set; } = 32768; // 32768    HasComment?
        public ushort ParameterSize { get; set; } //112, 208, 320    (with 16h) 10485872, 17826000, 26214720
        public ushort ParameterDataSize { get; set; } //160, 272, 400 
        public MetaHash FileName { get; set; } //decal_emissive_only.sps, emissive.sps, spec.sps
        public uint Unknown_1Ch; // 0x00000000
        public uint RenderBucketMask { get; set; } //65284, 65281  DrawBucketMask?   (1<<bucket) | 0xFF00
        public ushort Unknown_24h; // 0x0000
        public byte Unknown_26h; // 0x00
        public byte TextureParametersCount { get; set; }
        public ulong Unknown_28h; // 0x0000000000000000

        // reference data
        public ShaderParametersBlock ParametersList { get; set; }

        // gen9 structure data
        public MetaHash G9_Preset { get; set; } = 0x6D657461;
        public ulong G9_TextureRefsPointer { get; set; }
        public ulong G9_UnknownParamsPointer { get; set; }
        public ulong G9_ParametersListPointer { get; set; }
        public ulong G9_Unknown_28h;
        public ulong G9_Unknown_30h;
        public byte G9_Unknown_38h;
        public ShaderParamInfosG9 G9_ParamInfos { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            if (reader.IsGen9)
            {
                Name = new MetaHash(reader.ReadUInt32());
                G9_Preset = reader.ReadUInt32();
                ParametersPointer = reader.ReadUInt64();                   // m_parameters
                G9_TextureRefsPointer = reader.ReadUInt64();
                G9_UnknownParamsPointer = reader.ReadUInt64();//something to do with grass_batch (instance data?)
                G9_ParametersListPointer = reader.ReadUInt64();                // m_parameterData (sgaShaderParamData)
                G9_Unknown_28h = reader.ReadUInt64();//pad
                G9_Unknown_30h = reader.ReadUInt64();//pad
                G9_Unknown_38h = reader.ReadByte();
                RenderBucket = reader.ReadByte();
                ParameterDataSize = reader.ReadUInt16();//==ParametersList.G9_DataSize
                RenderBucketMask = reader.ReadUInt32();

                G9_ParamInfos = reader.ReadBlockAt<ShaderParamInfosG9>(G9_ParametersListPointer);
                ParametersList = reader.ReadBlockAt<ShaderParametersBlock>(ParametersPointer, 0, this);
                FileName = JenkHash.GenHash(Name.ToCleanString() + ".sps");//TODO: get mapping from G9_Preset to legacy FileName

                if (G9_UnknownParamsPointer != 0)
                { }
                if (G9_Unknown_28h != 0)
                { }
                if (G9_Unknown_30h != 0)
                { }
                if (G9_Unknown_38h != 0)
                { }
                switch (G9_Preset)
                {
                    case 0x6D657461:
                        break;
                    default:
                        break;
                }

            }
            else
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
                this.Unknown_28h = reader.ReadUInt64();

                // read reference data
                this.ParametersList = reader.ReadBlockAt<ShaderParametersBlock>(
                    this.ParametersPointer, // offset
                    this.ParameterCount,
                    this
                );

                //// just testing...
                //if (Unknown_12h != 32768)
                //{
                //    if (Unknown_12h != 0)//des_aquaduct_root, rig_root_skin.... destructions?
                //    { }//no hit
                //}
                //if (RenderBucketMask != ((1 << RenderBucket) | 0xFF00))
                //{ }//no hit
                //if (ParameterSize != ParametersList?.ParametersSize)
                //{ }//no hit
                ////if (ParameterDataSize != ParametersList?.ParametersDataSize)
                //{
                //    var diff = ParameterDataSize - (ParametersList?.BlockLength ?? 0);
                //    switch (diff)
                //    {
                //        case 32:
                //        case 36:
                //        case 40:
                //        case 44:
                //            break;
                //        default:
                //            break;//no hit
                //    }
                //}
                //if (Unknown_24h != 0)
                //{ }//no hit
                //if (Unknown_26h != 0)
                //{ }//no hit
                //if (Unknown_Ch != 0)
                //{ }//no hit
                //if (Unknown_1Ch != 0)
                //{ }//no hit
                //if (Unknown_28h != 0)
                //{ }//no hit

            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            if (writer.IsGen9)
            {
                ParametersPointer = (ulong)(ParametersList != null ? ParametersList.FilePosition : 0);
                ParameterCount = (byte)(ParametersList != null ? ParametersList.Count : 0);
                //TODO: update G9_TextureRefsPointer, G9_UnknownParamsPointer, G9_ParametersListPointer

                writer.Write((uint)Name);
                writer.Write((uint)G9_Preset);
                writer.Write(ParametersPointer);
                writer.Write(G9_TextureRefsPointer);
                writer.Write(G9_UnknownParamsPointer);
                writer.Write(G9_ParametersListPointer);
                writer.Write(G9_Unknown_28h);
                writer.Write(G9_Unknown_30h);
                writer.Write(G9_Unknown_38h);
                writer.Write(RenderBucket);
                writer.Write(ParameterDataSize);
                writer.Write(RenderBucketMask);

            }
            else
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
            }
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YdrXml.StringTag(sb, indent, "Name", YdrXml.HashString(Name));
            YdrXml.StringTag(sb, indent, "FileName", YdrXml.HashString(FileName));
            YdrXml.ValueTag(sb, indent, "RenderBucket", RenderBucket.ToString());
            if (ParametersList != null)
            {
                YdrXml.OpenTag(sb, indent, "Parameters");
                ParametersList.WriteXml(sb, indent + 1);
                YdrXml.CloseTag(sb, indent, "Parameters");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
            FileName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "FileName"));
            RenderBucket = (byte)Xml.GetChildUIntAttribute(node, "RenderBucket", "value");
            RenderBucketMask = ((1u << RenderBucket) | 0xFF00u);
            var pnode = node.SelectSingleNode("Parameters");
            if (pnode != null)
            {
                ParametersList = new ShaderParametersBlock();
                ParametersList.Owner = this;
                ParametersList.ReadXml(pnode);

                ParameterCount = (byte)ParametersList.Count;
                ParameterSize = ParametersList.ParametersSize;
                ParameterDataSize = ParametersList.ParametersDataSize;//is it right?
                TextureParametersCount = ParametersList.TextureParamsCount;
            }
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (G9_ParamInfos != null) list.Add(G9_ParamInfos);
            if (ParametersList != null) list.Add(ParametersList);
            return list.ToArray();
        }


        public override string ToString()
        {
            return Name.ToString() + " (" + FileName.ToString() + ")";
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ShaderParameter
    {
        public byte DataType { get; set; } //0: texture, 1: vector4
        public byte Unknown_1h { get; set; }
        public ushort Unknown_2h; // 0x0000
        public uint Unknown_4h; // 0x00000000
        public ulong DataPointer { get; set; }

        public object Data { get; set; }

        public void Read(ResourceDataReader reader)
        {
            this.DataType = reader.ReadByte();
            this.Unknown_1h = reader.ReadByte();
            this.Unknown_2h = reader.ReadUInt16();
            this.Unknown_4h = reader.ReadUInt32();
            this.DataPointer = reader.ReadUInt64();
        }
        public void Write(ResourceDataWriter writer)
        {
            writer.Write(this.DataType);
            writer.Write(this.Unknown_1h);
            writer.Write(this.Unknown_2h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.DataPointer);
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
                var bsize = BaseSize;
                var psize = ParametersDataSize;
                return bsize + psize*4;
            }
        }
        public override long BlockLength_Gen9 => G9_DataSize;

        public long BaseSize
        {
            get
            {
                long offset = 32;
                if (Parameters != null)
                {
                    foreach (var x in Parameters)
                    {
                        offset += 16;
                        offset += 16 * x.DataType;
                    }
                    offset += Parameters.Length * 4;
                }
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
        public ushort ParametersDataSize
        {
            get
            {
                var size = BaseSize;
                if ((size % 16) != 0) size += (16 - (size % 16));
                return (ushort)size;
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

        public ShaderFX Owner { get; set; }

        private ResourceSystemStructBlock<Vector4>[] ParameterDataBlocks = null;


        // gen9 data
        public long G9_DataSize { get; set; }
        public ShaderParamInfosG9 G9_ParamInfos { get; set; }
        public ulong[] G9_BufferPtrs { get; set; }//4x copies of buffers.. buffer data immediately follows pointers array
        public uint[] G9_BufferSizes { get; set; }//sizes of all buffers
        public uint G9_BuffersDataSize { get; set; }
        public ulong[] G9_TexturePtrs { get; set; }
        public ulong[] G9_UnknownData { get; set; }
        public byte[] G9_Samplers { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            Count = Convert.ToInt32(parameters[0]);
            Owner = parameters[1] as ShaderFX;

            if (reader.IsGen9)
            {
                GameFileCache.EnsureShadersGen9ConversionData();
                GameFileCache.ShadersGen9ConversionData.TryGetValue(Owner.Name, out var dc);
                var paramap = dc?.ParamsMapGen9ToLegacy;

                G9_ParamInfos = Owner.G9_ParamInfos;
                var multi = (int)G9_ParamInfos.Unknown2;//12  ... wtf
                var mult = (uint)multi;

                var bcnt = G9_ParamInfos.NumBuffers;
                var spos = reader.Position;
                G9_BufferPtrs = reader.ReadStructs<ulong>(bcnt * mult);//12x copies of buffers...... !!
                G9_BufferSizes = new uint[bcnt];//this might affect load performance slightly, but needed for XML and saving
                for (int i = 0; i < bcnt; i++)
                {
                    G9_BufferSizes[i] = (uint)(G9_BufferPtrs[i + 1] - G9_BufferPtrs[i]);
                }
                var texturesOffset = Math.Max(0, (long)Owner.G9_TextureRefsPointer - spos);
                var unknownsOffset = Math.Max(0, (long)Owner.G9_UnknownParamsPointer - spos);
                var p0 = 0ul;
                var p1 = 0ul;
                if ((G9_BufferPtrs != null) && (G9_BufferPtrs.Length > bcnt))
                {
                    p0 = G9_BufferPtrs[0];
                    p1 = G9_BufferPtrs[bcnt];
                }
                var ptrslen = bcnt * 8 * multi;
                var bufslen = (int)(p1 - p0) * multi;
                var texslen = G9_ParamInfos.NumTextures * 8 * multi;
                var unkslen = G9_ParamInfos.NumUnknowns * 8 * multi;
                var smpslen = G9_ParamInfos.NumSamplers;
                var totlen = ptrslen + bufslen + texslen + unkslen + smpslen;
                G9_BuffersDataSize = (uint)bufslen;
                G9_DataSize = totlen;

                if (Owner.G9_TextureRefsPointer != 0)
                {
                    G9_TexturePtrs = reader.ReadUlongsAt(Owner.G9_TextureRefsPointer, G9_ParamInfos.NumTextures * mult, false);
                }
                if (Owner.G9_UnknownParamsPointer != 0)
                {
                    G9_UnknownData = reader.ReadUlongsAt(Owner.G9_UnknownParamsPointer, G9_ParamInfos.NumUnknowns * mult, false);
                }
                if (G9_ParamInfos.NumSamplers > 0)
                {
                    G9_Samplers = reader.ReadBytesAt((ulong)(spos + (ptrslen + bufslen + texslen + unkslen)), G9_ParamInfos.NumSamplers, false);
                }

                var paras = new List<ShaderParameter>();
                var hashes = new List<MetaName>();
                foreach (var info in G9_ParamInfos.Params)
                {
                    var hash = info.Name.Hash;
                    if ((paramap != null) && paramap.TryGetValue(hash, out var oldhash))
                    {
                        hash = oldhash;
                    }

                    if (info.Type == ShaderParamTypeG9.Texture)
                    {
                        var p = new ShaderParameter();
                        p.DataType = 0;
                        p.DataPointer = G9_TexturePtrs[info.TextureIndex];
                        p.Data = reader.ReadBlockAt<TextureBase>(p.DataPointer);
                        paras.Add(p);
                        hashes.Add((MetaName)hash);
                    }
                    else if (info.Type == ShaderParamTypeG9.CBuffer)
                    {
                        uint fcnt = info.ParamLength / 4u;
                        uint arrsiz = info.ParamLength / 16u;
                        var p = new ShaderParameter();
                        p.DataType = (byte)Math.Max(arrsiz, 1);
                        if ((info.ParamLength) % 4 != 0)
                        { }
                        var cbi = info.CBufferIndex;
                        var baseptr = ((G9_BufferPtrs != null) && (G9_BufferPtrs.Length > cbi)) ? (long)G9_BufferPtrs[cbi] : 0;
                        if (baseptr != 0)
                        {
                            var ptr = baseptr + info.ParamOffset;
                            switch (fcnt)
                            {
                                case 0:
                                    break;
                                case 1: p.Data = new Vector4(reader.ReadStructAt<float>(ptr), 0, 0, 0); break;
                                case 2: p.Data = new Vector4(reader.ReadStructAt<Vector2>(ptr), 0, 0); break;
                                case 3: p.Data = new Vector4(reader.ReadStructAt<Vector3>(ptr), 0); break;
                                case 4: p.Data = reader.ReadStructAt<Vector4>(ptr); break;
                                default:
                                    if (arrsiz == 0)
                                    { }
                                    p.Data = reader.ReadStructsAt<Vector4>((ulong)ptr, arrsiz, false);
                                    break;
                            }
                        }
                        else
                        { }
                        paras.Add(p);
                        hashes.Add((MetaName)hash);
                    }
                    else
                    { }//todo?
                }
                Parameters = paras.ToArray();
                Hashes = hashes.ToArray();
                Count = paras.Count;
            }
            else
            {

                var paras = new List<ShaderParameter>();
                for (int i = 0; i < Count; i++)
                {
                    var p = new ShaderParameter();
                    p.Read(reader);
                    paras.Add(p);
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
                            p.Data = reader.ReadStructsAt<Vector4>(p.DataPointer, p.DataType, false);
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


                ////testing padding area at the end of the block...
                //var psiz1 = Owner.ParameterDataSize;
                //var psiz2 = ParametersDataSize;
                //if (psiz1 != psiz2)
                //{ }//no hit
                //var unk0 = reader.ReadStructs<MetaHash>(8);
                //foreach (var u0i in unk0)
                //{
                //    if (u0i != 0)
                //    { }//no hit
                //}
                //if (Owner.Unknown_12h != 0)
                //{
                //    var unk1 = reader.ReadStructs<MetaHash>(psiz1);
                //    foreach (var u1i in unk1)
                //    {
                //        if (u1i != 0)
                //        { break; }//no hit
                //    }
                //}


                //// just testing...
                //for (int i = 0; i < Parameters.Length; i++)
                //{
                //    var param = Parameters[i];
                //    if (param.DataType == 0)
                //    {
                //        if (param.Unknown_1h != ((param.Data == null) ? 10 : (i + 2)))
                //        { }
                //    }
                //    else
                //    {
                //        if (param.Unknown_1h != (160 + ((Parameters.Length - 1) - i)))
                //        { }
                //    }
                //}
                //if (Parameters.Length > 0)
                //{
                //    var lparam = Parameters[Parameters.Length - 1];
                //    switch(lparam.Unknown_1h)
                //    {
                //        case 192:
                //        case 160:
                //        case 177:
                //        case 161:
                //        case 156:
                //        case 162:
                //        case 157:
                //        case 149:
                //        case 178:
                //        case 72:
                //        case 153:
                //        case 133:
                //            break;
                //        case 64://in ydd's
                //        case 130:
                //        case 180:
                //            break;
                //        default:
                //            break;
                //    }
                //}

            }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            if (writer.IsGen9)
            {
                GameFileCache.EnsureShadersGen9ConversionData();
                //TODO: update shader params mappings

                //TODO

            }
            else
            {

                // update pointers...
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
                {
                    f.Write(writer);
                }

                // write vector data
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
                {
                    writer.Write((uint)h);
                }


                //write end padding stuff
                var psiz = ParametersDataSize;
                writer.Write(new byte[32 + psiz*4]);

            }
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            var cind = indent + 1;
            for (int i = 0; i < Count; i++)
            {
                var param = Parameters[i];
                var name = (ShaderParamNames)Hashes[i];
                var typestr = "";
                if (param.DataType == 0) typestr = "Texture";
                else if (param.DataType == 1) typestr = "Vector";
                else if (param.DataType > 1) typestr = "Array";
                var otstr = "Item name=\"" + name.ToString() + "\" type=\"" + typestr + "\"";

                if (param.DataType == 0)
                {
                    if (param.Data is TextureBase tex)
                    {
                        YdrXml.OpenTag(sb, indent, otstr);
                        YdrXml.StringTag(sb, cind, "Name", YdrXml.XmlEscape(tex.Name));
                        YdrXml.CloseTag(sb, indent, "Item");
                    }
                    else
                    {
                        YdrXml.SelfClosingTag(sb, indent, otstr);
                    }
                }
                else if (param.DataType == 1)
                {
                    if (param.Data is Vector4 vec)
                    {
                        YdrXml.SelfClosingTag(sb, indent, otstr + " " + FloatUtil.GetVector4XmlString(vec));
                    }
                    else
                    {
                        YdrXml.SelfClosingTag(sb, indent, otstr);
                    }
                }
                else
                {
                    if (param.Data is Vector4[] arr)
                    {
                        YdrXml.OpenTag(sb, indent, otstr);
                        foreach (var vec in arr)
                        {
                            YdrXml.SelfClosingTag(sb, cind, "Value " + FloatUtil.GetVector4XmlString(vec));
                        }
                        YdrXml.CloseTag(sb, indent, "Item");
                    }
                    else
                    {
                        YdrXml.SelfClosingTag(sb, indent, otstr);
                    }
                }
            }
        }
        public void ReadXml(XmlNode node)
        {
            var plist = new List<ShaderParameter>();
            var hlist = new List<MetaName>();
            var pnodes = node.SelectNodes("Item");
            foreach (XmlNode pnode in pnodes)
            {
                var p = new ShaderParameter();
                var h = (MetaName)(uint)XmlMeta.GetHash(Xml.GetStringAttribute(pnode, "name")?.ToLowerInvariant());
                var type = Xml.GetStringAttribute(pnode, "type");
                if (type == "Texture")
                {
                    p.DataType = 0;
                    if (pnode.SelectSingleNode("Name") != null)
                    {
                        var tex = new TextureBase();
                        tex.ReadXml(pnode, null);//embedded textures will get replaced in ShaderFX ReadXML
                        tex.Unknown_32h = 2;
                        p.Data = tex;
                    }
                }
                else if (type == "Vector")
                {
                    p.DataType = 1;
                    float fx = Xml.GetFloatAttribute(pnode, "x");
                    float fy = Xml.GetFloatAttribute(pnode, "y");
                    float fz = Xml.GetFloatAttribute(pnode, "z");
                    float fw = Xml.GetFloatAttribute(pnode, "w");
                    p.Data = new Vector4(fx, fy, fz, fw);
                }
                else if (type == "Array")
                {
                    var vecs = new List<Vector4>();
                    var inodes = pnode.SelectNodes("Value");
                    foreach (XmlNode inode in inodes)
                    {
                        float fx = Xml.GetFloatAttribute(inode, "x");
                        float fy = Xml.GetFloatAttribute(inode, "y");
                        float fz = Xml.GetFloatAttribute(inode, "z");
                        float fw = Xml.GetFloatAttribute(inode, "w");
                        vecs.Add(new Vector4(fx, fy, fz, fw));
                    }
                    p.Data = vecs.ToArray();
                    p.DataType = (byte)vecs.Count;
                }
                plist.Add(p);
                hlist.Add(h);
            }

            Parameters = plist.ToArray();
            Hashes = hlist.ToArray();
            Count = plist.Count;

            for (int i = 0; i < Parameters.Length; i++)
            {
                var param = Parameters[i];
                if (param.DataType == 0)
                {
                    param.Unknown_1h = (byte)(i + 2);//wtf and why
                }
            }
            var offset = 160;
            for (int i = Parameters.Length - 1; i >= 0; i--)
            {
                var param = Parameters[i];
                if (param.DataType != 0)
                {
                    param.Unknown_1h = (byte)offset;//wtf and why
                    offset += param.DataType;
                }
            }

        }




        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            list.AddRange(base.GetReferences());

            foreach (var x in Parameters)
            {
                if (x.DataType == 0)
                {
                    list.Add(x.Data as TextureBase);
                }
            }

            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            var list = new List<Tuple<long, IResourceBlock>>();
            list.AddRange(base.GetParts());

            long offset = Parameters.Length * 16;

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

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ShaderParamInfosG9 : ResourceSystemBlock
    {
        public override long BlockLength => 8 + NumParams * 8;

        public byte NumBuffers { get; set; }
        public byte NumTextures { get; set; }
        public byte NumUnknowns { get; set; }
        public byte NumSamplers { get; set; }
        public byte NumParams { get; set; }
        public byte Unknown0 { get; set; }
        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }
        public ShaderParamInfoG9[] Params { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            NumBuffers = reader.ReadByte();
            NumTextures = reader.ReadByte();
            NumUnknowns = reader.ReadByte();
            NumSamplers = reader.ReadByte();
            NumParams = reader.ReadByte();
            Unknown0 = reader.ReadByte();
            Unknown1 = reader.ReadByte();
            Unknown2 = reader.ReadByte();
            Params = reader.ReadStructs<ShaderParamInfoG9>(NumParams);

            if (Unknown0 != 0)
            { }
            if (Unknown1 != 0)
            { }
            if (Unknown2 != 0xc)
            { }
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            writer.Write(NumBuffers);
            writer.Write(NumTextures);
            writer.Write(NumUnknowns);
            writer.Write(NumSamplers);
            writer.Write(NumParams);
            writer.Write(Unknown0);
            writer.Write(Unknown1);
            writer.Write(Unknown2);
            writer.WriteStructs(Params);
        }

    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public struct ShaderParamInfoG9
    {
        public MetaHash Name { get; set; }
        public uint Data { get; set; }

        public ShaderParamTypeG9 Type { get => (ShaderParamTypeG9)(Data & 0x3); set { Data = (Data & 0xFFFFFFF8) + (((uint)value) & 0x3); } }
        public byte TextureIndex { get => (byte)((Data >> 2) & 0xFF); set { Data = (Data & 0xFFFFFC03) + (((uint)value & 0xFF) << 2); } }
        public byte SamplerIndex { get => (byte)((Data >> 2) & 0xFF); set { Data = (Data & 0xFFFFFC03) + (((uint)value & 0xFF) << 2); } }
        public byte CBufferIndex { get => (byte)((Data >> 2) & 0x3F); set { Data = (Data & 0xFFFFFF03) + (((uint)value & 0x3F) << 2); } }
        public ushort ParamOffset { get => (ushort)((Data >> 8) & 0xFFF); set { Data = (Data & 0xFFF000FF) + (((uint)value & 0xFFF) << 8); } }
        public ushort ParamLength { get => (ushort)((Data >> 20) & 0xFFF); set { Data = (Data & 0x000FFFFF) + (((uint)value & 0xFFF) << 20); } }

        public override string ToString()
        {
            return $"{Name}: {Type}, {TextureIndex}, {ParamOffset}, {ParamLength}";
        }
    }
    public enum ShaderParamTypeG9 : byte
    {
        Texture = 0,
        Unknown = 1,
        Sampler = 2,
        CBuffer = 3,
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class Skeleton : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 112; }
        }

        // structure data
        public uint VFT { get; set; } = 1080114336;
        public uint Unknown_4h { get; set; } = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong BoneTagsPointer { get; set; }
        public ushort BoneTagsCapacity { get; set; }
        public ushort BoneTagsCount { get; set; }
        public FlagsUint Unknown_1Ch { get; set; }
        public ulong BonesPointer { get; set; }
        public ulong TransformationsInvertedPointer { get; set; }
        public ulong TransformationsPointer { get; set; }
        public ulong ParentIndicesPointer { get; set; }
        public ulong ChildIndicesPointer { get; set; }
        public ulong Unknown_48h; // 0x0000000000000000
        public MetaHash Unknown_50h { get; set; }
        public MetaHash Unknown_54h { get; set; }
        public MetaHash Unknown_58h { get; set; }
        public ushort Unknown_5Ch { get; set; } = 1; // 0x0001
        public ushort BonesCount { get; set; }
        public ushort ChildIndicesCount { get; set; }
        public ushort Unknown_62h; // 0x0000
        public uint Unknown_64h; // 0x00000000
        public ulong Unknown_68h; // 0x0000000000000000

        // reference data
        public ResourcePointerArray64<SkeletonBoneTag> BoneTags { get; set; }
        public SkeletonBonesBlock Bones { get; set; }

        public Matrix[] TransformationsInverted { get; set; }
        public Matrix[] Transformations { get; set; }
        public short[] ParentIndices { get; set; }
        public short[] ChildIndices { get; set; }//mapping child->parent indices, first child index, then parent

        private ResourceSystemStructBlock<Matrix> TransformationsInvertedBlock = null;//for saving only
        private ResourceSystemStructBlock<Matrix> TransformationsBlock = null;
        private ResourceSystemStructBlock<short> ParentIndicesBlock = null;
        private ResourceSystemStructBlock<short> ChildIndicesBlock = null;


        public Dictionary<ushort, Bone> BonesMap { get; set; }//for convienience finding bones by tag
        public Bone[] BonesSorted { get; set; } //sometimes bones aren't in parent>child order in the files! (eg player chars)


        public Matrix3_s[] BoneTransforms; //for rendering


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.BoneTagsPointer = reader.ReadUInt64();
            this.BoneTagsCapacity = reader.ReadUInt16();
            this.BoneTagsCount = reader.ReadUInt16();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.BonesPointer = reader.ReadUInt64();
            this.TransformationsInvertedPointer = reader.ReadUInt64();
            this.TransformationsPointer = reader.ReadUInt64();
            this.ParentIndicesPointer = reader.ReadUInt64();
            this.ChildIndicesPointer = reader.ReadUInt64();
            this.Unknown_48h = reader.ReadUInt64();
            this.Unknown_50h = new MetaHash(reader.ReadUInt32());
            this.Unknown_54h = new MetaHash(reader.ReadUInt32());
            this.Unknown_58h = new MetaHash(reader.ReadUInt32());
            this.Unknown_5Ch = reader.ReadUInt16();
            this.BonesCount = reader.ReadUInt16();
            this.ChildIndicesCount = reader.ReadUInt16();
            this.Unknown_62h = reader.ReadUInt16();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt64();

            // read reference data
            this.BoneTags = reader.ReadBlockAt<ResourcePointerArray64<SkeletonBoneTag>>(this.BoneTagsPointer, this.BoneTagsCapacity);
            this.Bones = reader.ReadBlockAt<SkeletonBonesBlock>((this.BonesPointer != 0) ? (BonesPointer - 16) : 0, (uint)this.BonesCount);
            this.TransformationsInverted = reader.ReadStructsAt<Matrix>(this.TransformationsInvertedPointer, this.BonesCount);
            this.Transformations = reader.ReadStructsAt<Matrix>(this.TransformationsPointer, this.BonesCount);
            this.ParentIndices = reader.ReadShortsAt(this.ParentIndicesPointer, this.BonesCount);
            this.ChildIndices = reader.ReadShortsAt(this.ChildIndicesPointer, this.ChildIndicesCount);


            AssignBoneParents();

            BuildBonesMap();

            //BuildIndices();//testing!
            //BuildBoneTags();//testing!
            //BuildTransformations();//testing!
            //if (BoneTagsCount != Math.Min(BonesCount, BoneTagsCapacity))
            //{ }//no hits

            //if (BonesPointer != 0)
            //{
            //    var bhdr = reader.ReadStructAt<ResourcePointerListHeader>((long)BonesPointer - 16);
            //    if (bhdr.Pointer != BonesCount)
            //    { }//no hit
            //    if ((bhdr.Count != 0) || (bhdr.Capacity != 0) || (bhdr.Unknown != 0))
            //    { }//no hit
            //}

            //if (Unknown_8h != 0)
            //{ }
            //if (Unknown_48h != 0)
            //{ }
            //if (Unknown_62h != 0)
            //{ }
            //if (Unknown_64h != 0)
            //{ }
            //if (Unknown_68h != 0)
            //{ }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.BoneTagsPointer = (ulong)(this.BoneTags != null ? this.BoneTags.FilePosition : 0);
            this.BoneTagsCapacity = (ushort)(this.BoneTags != null ? this.BoneTags.Count : 0);
            this.BonesPointer = (ulong)(this.Bones != null ? this.Bones.FilePosition+16 : 0);
            this.TransformationsInvertedPointer = (ulong)(this.TransformationsInvertedBlock != null ? this.TransformationsInvertedBlock.FilePosition : 0);
            this.TransformationsPointer = (ulong)(this.TransformationsBlock != null ? this.TransformationsBlock.FilePosition : 0);
            this.ParentIndicesPointer = (ulong)(this.ParentIndicesBlock != null ? this.ParentIndicesBlock.FilePosition : 0);
            this.ChildIndicesPointer = (ulong)(this.ChildIndicesBlock != null ? this.ChildIndicesBlock.FilePosition : 0);
            this.BonesCount = (ushort)(this.Bones?.Items != null ? this.Bones.Items.Length : 0);
            this.ChildIndicesCount = (ushort)(this.ChildIndicesBlock != null ? this.ChildIndicesBlock.ItemCount : 0);
            this.BoneTagsCount = Math.Min(BonesCount, BoneTagsCapacity);


            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.BoneTagsPointer);
            writer.Write(this.BoneTagsCapacity);
            writer.Write(this.BoneTagsCount);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.BonesPointer);
            writer.Write(this.TransformationsInvertedPointer);
            writer.Write(this.TransformationsPointer);
            writer.Write(this.ParentIndicesPointer);
            writer.Write(this.ChildIndicesPointer);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
            writer.Write(this.BonesCount);
            writer.Write(this.ChildIndicesCount);
            writer.Write(this.Unknown_62h);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YdrXml.ValueTag(sb, indent, "Unknown1C", Unknown_1Ch.Value.ToString());
            YdrXml.ValueTag(sb, indent, "Unknown50", Unknown_50h.Hash.ToString());
            YdrXml.ValueTag(sb, indent, "Unknown54", Unknown_54h.Hash.ToString());
            YdrXml.ValueTag(sb, indent, "Unknown58", Unknown_58h.Hash.ToString());

            if (Bones?.Items != null)
            {
                YdrXml.WriteItemArray(sb, Bones.Items, indent, "Bones");
            }

        }
        public void ReadXml(XmlNode node)
        {
            Unknown_1Ch = Xml.GetChildUIntAttribute(node, "Unknown1C", "value");
            Unknown_50h = Xml.GetChildUIntAttribute(node, "Unknown50", "value");
            Unknown_54h = Xml.GetChildUIntAttribute(node, "Unknown54", "value");
            Unknown_58h = Xml.GetChildUIntAttribute(node, "Unknown58", "value");

            var bones = XmlMeta.ReadItemArray<Bone>(node, "Bones");
            if (bones != null)
            {
                Bones = new SkeletonBonesBlock();
                Bones.Items = bones;
            }

            BuildIndices();
            BuildBoneTags();
            AssignBoneParents();
            BuildTransformations();
            BuildBonesMap();
        }

        public override IResourceBlock[] GetReferences()
        {
            BuildTransformations();

            var list = new List<IResourceBlock>();
            if (BoneTags != null) list.Add(BoneTags);
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
                ParentIndicesBlock = new ResourceSystemStructBlock<short>(ParentIndices);
                list.Add(ParentIndicesBlock);
            }
            if (ChildIndices != null)
            {
                ChildIndicesBlock = new ResourceSystemStructBlock<short>(ChildIndices);
                list.Add(ChildIndicesBlock);
            }
            return list.ToArray();
        }






        public void AssignBoneParents()
        {
            if ((Bones?.Items != null) && (ParentIndices != null))
            {
                var maxcnt = Math.Min(Bones.Items.Length, ParentIndices.Length);
                for (int i = 0; i < maxcnt; i++)
                {
                    var bone = Bones.Items[i];
                    var pind = ParentIndices[i];
                    if ((pind >= 0) && (pind < Bones.Items.Length))
                    {
                        bone.Parent = Bones.Items[pind];
                    }
                }
            }
        }

        public void BuildBonesMap()
        {
            BonesMap = new Dictionary<ushort, Bone>();
            if (Bones?.Items != null)
            {
                var bonesSorted = new List<Bone>();
                for (int i = 0; i < Bones.Items.Length; i++)
                {
                    var bone = Bones.Items[i];
                    BonesMap[bone.Tag] = bone;
                    bonesSorted.Add(bone);

                    bone.UpdateAnimTransform();
                    bone.AbsTransform = bone.AnimTransform;
                    bone.BindTransformInv = (i < (TransformationsInverted?.Length ?? 0)) ? TransformationsInverted[i] : Matrix.Invert(bone.AnimTransform);
                    bone.BindTransformInv.M44 = 1.0f;
                    bone.UpdateSkinTransform();
                    bone.TransformUnk = (i < (Transformations?.Length ?? 0)) ? Transformations[i].Column4 : Vector4.Zero;//still dont know what this is
                }
                bonesSorted.Sort((a, b) => a.Index.CompareTo(b.Index));
                BonesSorted = bonesSorted.ToArray();
            }
        }

        public void BuildIndices()
        {
            var parents = new List<short>();
            var childs = new List<short>();
            if (Bones?.Items != null)
            {

                //crazy breadth-wise limited to 4 algorithm for generating the ChildIndices

                var tbones = Bones.Items.ToList();
                var rootbones = tbones.Where(b => (b.ParentIndex < 0)).ToList();
                for (int i = 0; i < tbones.Count; i++)
                {
                    var bone = Bones.Items[i];
                    var pind = bone.ParentIndex;
                    parents.Add(pind);
                }

                List<Bone> getChildren(Bone b)
                {
                    var r = new List<Bone>();
                    if (b == null) return r;
                    for (int i = 0; i < tbones.Count; i++)
                    {
                        var tb = tbones[i];
                        if (tb.ParentIndex == b.Index)
                        {
                            r.Add(tb);
                        }
                    }
                    return r;
                }
                List<Bone> getAllChildren(List<Bone> bones)
                {
                    var l = new List<Bone>();
                    foreach (var b in bones)
                    {
                        var children = getChildren(b);
                        l.AddRange(children);
                    }
                    return l;
                }
                
                var layers = new List<List<Bone>>();
                var layer = getAllChildren(rootbones);
                while (layer.Count > 0)
                {
                    var numbones = Math.Min(layer.Count, 4);
                    var inslayer = layer.GetRange(0, numbones);
                    var extlayer = getAllChildren(inslayer);
                    layers.Add(inslayer);
                    layer.RemoveRange(0, numbones);
                    layer.InsertRange(0, extlayer);
                }



                foreach (var l in layers)
                {
                    Bone lastbone = null;
                    foreach (var b in l)
                    {
                        childs.Add(b.Index);
                        childs.Add(b.ParentIndex);
                        lastbone = b;
                    }
                    if (lastbone != null)
                    {
                        var npad = 8 - (childs.Count % 8);
                        if (npad < 8)
                        {
                            for (int i = 0; i < npad; i += 2)
                            {
                                childs.Add(lastbone.Index);
                                childs.Add(lastbone.ParentIndex);
                            }
                        }
                    }
                }





                //////just testing
                //var numchilds = ChildIndices?.Length ?? 0;
                //int diffstart = -1;
                //int diffend = -1;
                //int ndiff = Math.Abs(numchilds - childs.Count);
                //int maxchilds = Math.Min(numchilds, childs.Count);
                //for (int i = 0; i < maxchilds; i++)
                //{
                //    var oc = ChildIndices[i];
                //    var nc = childs[i];
                //    if (nc != oc)
                //    {
                //        if (diffstart < 0) diffstart = i;
                //        diffend = i;
                //        ndiff++; 
                //    }
                //}
                //if (ndiff > 0)
                //{
                //    var difffrac = ((float)ndiff) / ((float)numchilds);
                //}
                //if (numchilds != childs.Count)
                //{ }




                //var numbones = Bones.Items.Length;
                //var numchilds = ChildIndices?.Length ?? 0;
                //for (int i = 0; i < numchilds; i += 2)
                //{
                //    var bind = ChildIndices[i];
                //    var pind = ChildIndices[i + 1];
                //    if (bind > numbones)
                //    { continue; }//shouldn't happen
                //    var bone = Bones.Items[bind];
                //    if (bone == null)
                //    { continue; }//shouldn't happen
                //    if (pind != bone.ParentIndex)
                //    { }//shouldn't happen?
                //}




            }

            ParentIndices = (parents.Count > 0) ? parents.ToArray() : null;
            ChildIndices = (childs.Count > 0) ? childs.ToArray() : null;

        }

        public void BuildBoneTags()
        {
            var tags = new List<SkeletonBoneTag>();
            if (Bones?.Items != null)
            {
                for (int i = 0; i < Bones.Items.Length; i++)
                {
                    var bone = Bones.Items[i];
                    var tag = new SkeletonBoneTag();
                    tag.BoneTag = bone.Tag;
                    tag.BoneIndex = (uint)i;
                    tags.Add(tag);
                }
            }

            if (tags.Count < 2)
            {
                if (BoneTags != null)
                { }
                BoneTags = null;
                return;
            }

            var numbuckets = GetNumHashBuckets(tags.Count);

            var buckets = new List<SkeletonBoneTag>[numbuckets];
            foreach (var tag in tags)
            {
                var b = tag.BoneTag % numbuckets;
                var bucket = buckets[b];
                if (bucket == null)
                {
                    bucket = new List<SkeletonBoneTag>();
                    buckets[b] = bucket;
                }
                bucket.Add(tag);
            }

            var newtags = new List<SkeletonBoneTag>();
            foreach (var b in buckets)
            {
                if ((b?.Count ?? 0) == 0) newtags.Add(null);
                else
                {
                    b.Reverse();
                    newtags.Add(b[0]);
                    var p = b[0];
                    for (int i = 1; i < b.Count; i++)
                    {
                        var c = b[i];
                        c.Next = null;
                        p.Next = c;
                        p = c;
                    }
                }
            }


            //if (BoneTags?.data_items != null) //just testing - all ok
            //{
            //    var numtags = BoneTags.data_items.Length;
            //    if (numbuckets != numtags)
            //    { }
            //    else
            //    {
            //        for (int i = 0; i < numtags; i++)
            //        {
            //            var ot = BoneTags.data_items[i];
            //            var nt = newtags[i];
            //            if ((ot == null) != (nt == null))
            //            { }
            //            else if (ot != null)
            //            {
            //                if (ot.BoneIndex != nt.BoneIndex)
            //                { }
            //                if (ot.BoneTag != nt.BoneTag)
            //                { }
            //            }
            //        }
            //    }
            //}


            BoneTags = new ResourcePointerArray64<SkeletonBoneTag>();
            BoneTags.data_items = newtags.ToArray();


        }

        public void BuildTransformations()
        {
            var transforms = new List<Matrix>();
            var transformsinv = new List<Matrix>();
            if (Bones?.Items != null)
            {
                foreach (var bone in Bones.Items)
                {
                    var pos = bone.Translation;
                    var ori = bone.Rotation;
                    var sca = bone.Scale;
                    var m = Matrix.AffineTransformation(1.0f, ori, pos);//(local transform)
                    m.ScaleVector *= sca;
                    m.Column4 = bone.TransformUnk;// new Vector4(0, 4, -3, 0);//???

                    var pbone = bone.Parent;
                    while (pbone != null)
                    {
                        pos = pbone.Rotation.Multiply(pos /** pbone.Scale*/) + pbone.Translation;
                        ori = pbone.Rotation * ori;
                        pbone = pbone.Parent;
                    }
                    var m2 = Matrix.AffineTransformation(1.0f, ori, pos);//(global transform)
                    var mi = Matrix.Invert(m2);
                    mi.Column4 = Vector4.Zero;

                    transforms.Add(m);
                    transformsinv.Add(mi);
                }
            }

            //if (Transformations != null) //just testing! - all ok
            //{
            //    if (Transformations.Length != transforms.Count)
            //    { }
            //    else
            //    {
            //        for (int i = 0; i < Transformations.Length; i++)
            //        {
            //            if (Transformations[i].Column1 != transforms[i].Column1)
            //            { }
            //            if (Transformations[i].Column2 != transforms[i].Column2)
            //            { }
            //            if (Transformations[i].Column3 != transforms[i].Column3)
            //            { }
            //            if (Transformations[i].Column4 != transforms[i].Column4)
            //            { }
            //        }
            //    }
            //    if (TransformationsInverted.Length != transformsinv.Count)
            //    { }
            //    else
            //    {
            //        for (int i = 0; i < TransformationsInverted.Length; i++)
            //        {
            //            if (TransformationsInverted[i].Column4 != transformsinv[i].Column4)
            //            { }
            //        }
            //    }
            //}

            Transformations = (transforms.Count > 0) ? transforms.ToArray() : null;
            TransformationsInverted = (transformsinv.Count > 0) ? transformsinv.ToArray() : null;

        }


        public static uint GetNumHashBuckets(int nHashes)
        {
            //todo: refactor with same in Clip.cs?
            if (nHashes < 11) return 11;
            else if (nHashes < 29) return 29;
            else if (nHashes < 59) return 59;
            else if (nHashes < 107) return 107;
            else if (nHashes < 191) return 191;
            else if (nHashes < 331) return 331;
            else if (nHashes < 563) return 563;
            else if (nHashes < 953) return 953;
            else if (nHashes < 1609) return 1609;
            else if (nHashes < 2729) return 2729;
            else if (nHashes < 4621) return 4621;
            else if (nHashes < 7841) return 7841;
            else if (nHashes < 13297) return 13297;
            else if (nHashes < 22571) return 22571;
            else if (nHashes < 38351) return 38351;
            else if (nHashes < 65167) return 65167;
            else /*if (nHashes < 65521)*/ return 65521;
            //return ((uint)nHashes / 4) * 4 + 3;
        }



        public void ResetBoneTransforms()
        {
            if (Bones?.Items == null) return;
            foreach (var bone in Bones.Items)
            {
                bone.ResetAnimTransform();
            }
            UpdateBoneTransforms();
        }
        public void UpdateBoneTransforms()
        {
            if (Bones?.Items == null) return;
            if ((BoneTransforms == null) || (BoneTransforms.Length != Bones.Items.Length))
            {
                BoneTransforms = new Matrix3_s[Bones.Items.Length];
            }
            for (int i = 0; i < Bones.Items.Length; i++)
            {
                var bone = Bones.Items[i];
                Matrix b = bone.SkinTransform;
                Matrix3_s bt = new Matrix3_s();
                bt.Row1 = b.Column1;
                bt.Row2 = b.Column2;
                bt.Row3 = b.Column3;
                BoneTransforms[i] = bt;
            }
        }






        public Skeleton Clone()
        {
            var skel = new Skeleton();

            skel.BoneTagsCapacity = BoneTagsCapacity;
            skel.BoneTagsCount = BoneTagsCount;
            skel.Unknown_1Ch = Unknown_1Ch;
            skel.Unknown_50h = Unknown_50h;
            skel.Unknown_54h = Unknown_54h;
            skel.Unknown_58h = Unknown_58h;
            skel.BonesCount = BonesCount;
            skel.ChildIndicesCount = ChildIndicesCount;

            if (BoneTags != null)
            {
                skel.BoneTags = new ResourcePointerArray64<SkeletonBoneTag>();
                if (BoneTags.data_items != null)
                {
                    skel.BoneTags.data_items = new SkeletonBoneTag[BoneTags.data_items.Length];
                    for (int i = 0; i < BoneTags.data_items.Length; i++)
                    {
                        var obt = BoneTags.data_items[i];
                        var nbt = new SkeletonBoneTag();
                        skel.BoneTags.data_items[i] = nbt;
                        while (obt != null)
                        {
                            nbt.BoneTag = obt.BoneTag;
                            nbt.BoneIndex = obt.BoneIndex;
                            obt = obt.Next;
                            if (obt != null)
                            {
                                var nxt = new SkeletonBoneTag();
                                nbt.Next = nxt;
                                nbt = nxt;
                            }
                        }
                    }
                }
            }
            if (Bones != null)
            {
                skel.Bones = new SkeletonBonesBlock();
                if (Bones.Items != null)
                {
                    skel.Bones.Items = new Bone[Bones.Items.Length];
                    for (int i = 0; i < Bones.Items.Length; i++)
                    {
                        var ob = Bones.Items[i];
                        var nb = new Bone();
                        nb.Rotation = ob.Rotation;
                        nb.Translation = ob.Translation;
                        nb.Scale = ob.Scale;
                        nb.NextSiblingIndex = ob.NextSiblingIndex;
                        nb.ParentIndex = ob.ParentIndex;
                        nb.Flags = ob.Flags;
                        nb.Index = ob.Index;
                        nb.Tag = ob.Tag;
                        nb.Index2 = ob.Index2;
                        nb.Name = ob.Name;
                        nb.AnimRotation = ob.AnimRotation;
                        nb.AnimTranslation = ob.AnimTranslation;
                        nb.AnimScale = ob.AnimScale;
                        nb.AnimTransform = ob.AnimTransform;
                        nb.BindTransformInv = ob.BindTransformInv;
                        nb.SkinTransform = ob.SkinTransform;
                        skel.Bones.Items[i] = nb;
                    }
                }
            }

            skel.TransformationsInverted = (Matrix[])TransformationsInverted?.Clone();
            skel.Transformations = (Matrix[])Transformations?.Clone();
            skel.ParentIndices = (short[])ParentIndices?.Clone();
            skel.ChildIndices = (short[])ChildIndices?.Clone();

            skel.AssignBoneParents();
            skel.BuildBonesMap();

            return skel;
        }



    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class SkeletonBonesBlock : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get
            {
                long length = 16;
                if (Items != null)
                {
                    foreach (var b in Items)
                    {
                        length += b.BlockLength;
                    }
                }
                return length;
            }
        }

        public uint Count { get; set; }
        public uint Unk0; // 0
        public uint Unk1; // 0
        public uint Unk2; // 0
        public Bone[] Items { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            Count = reader.ReadUInt32();
            Unk0 = reader.ReadUInt32();
            Unk1 = reader.ReadUInt32();
            Unk2 = reader.ReadUInt32();

            var count = (uint)parameters[0];
            var items = new Bone[count];
            for (uint i = 0; i < count; i++)
            {
                items[i] = reader.ReadBlock<Bone>();
            }
            Items = items;


            //if (Count != count)
            //{ }//no hit
            //if (Unk0 != 0)
            //{ }//no hit
            //if (Unk1 != 0)
            //{ }//no hit
            //if (Unk2 != 0)
            //{ }//no hit

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            Count = (uint)(Items?.Length ?? 0);

            writer.Write(Count);
            writer.Write(Unk0);
            writer.Write(Unk1);
            writer.Write(Unk2);

            foreach (var b in Items)
            {
                b.Write(writer);
            }
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            var list = new List<Tuple<long, IResourceBlock>>();
            long length = 16;
            if (Items != null)
            {
                foreach (var b in Items)
                {
                    list.Add(new Tuple<long, IResourceBlock>(length, b));
                    length += b.BlockLength;
                }
            }
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class SkeletonBoneTag : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public uint BoneTag { get; set; }
        public uint BoneIndex { get; set; }
        public ulong NextPointer { get; set; }

        // reference data
        public SkeletonBoneTag Next { get; set; } //don't know why it's linked here

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.BoneTag = reader.ReadUInt32();
            this.BoneIndex = reader.ReadUInt32();
            this.NextPointer = reader.ReadUInt64();

            // read reference data
            this.Next = reader.ReadBlockAt<SkeletonBoneTag>(
                this.NextPointer // offset
            );
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NextPointer = (ulong)(this.Next != null ? this.Next.FilePosition : 0);

            // write structure data
            writer.Write(this.BoneTag);
            writer.Write(this.BoneIndex);
            writer.Write(this.NextPointer);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Next != null) list.Add(Next);
            return list.ToArray();
        }

        public override string ToString()
        {
            return BoneTag.ToString() + ": " + BoneIndex.ToString();
        }

    }

    [Flags] public enum EBoneFlags : ushort
    {
        None = 0,
        RotX = 0x1,
        RotY = 0x2,
        RotZ = 0x4,
        LimitRotation = 0x8,
        TransX = 0x10,
        TransY = 0x20,
        TransZ = 0x40,
        LimitTranslation = 0x80,
        ScaleX = 0x100,
        ScaleY = 0x200,
        ScaleZ = 0x400,
        LimitScale = 0x800,
        Unk0 = 0x1000,
        Unk1 = 0x2000,
        Unk2 = 0x4000,
        Unk3 = 0x8000,
    }

    // List of BoneTags which are hardcoded/not calculated using ElfHash and CalculateBoneHash
    enum EPedBoneId : ushort
    {
        SKEL_ROOT = 0x0,
        SKEL_Pelvis = 0x2E28,
        SKEL_L_Thigh = 0xE39F,
        SKEL_L_Calf = 0xF9BB,
        SKEL_L_Foot = 0x3779,
        SKEL_L_Toe0 = 0x83C,
        EO_L_Foot = 0x84C5,
        EO_L_Toe = 0x68BD,
        IK_L_Foot = 0xFEDD,
        PH_L_Foot = 0xE175,
        MH_L_Knee = 0xB3FE,
        SKEL_R_Thigh = 0xCA72,
        SKEL_R_Calf = 0x9000,
        SKEL_R_Foot = 0xCC4D,
        SKEL_R_Toe0 = 0x512D,
        EO_R_Foot = 0x1096,
        EO_R_Toe = 0x7163,
        IK_R_Foot = 0x8AAE,
        PH_R_Foot = 0x60E6,
        MH_R_Knee = 0x3FCF,
        RB_L_ThighRoll = 0x5C57,
        RB_R_ThighRoll = 0x192A,
        SKEL_Spine_Root = 0xE0FD,
        SKEL_Spine0 = 0x5C01,
        SKEL_Spine1 = 0x60F0,
        SKEL_Spine2 = 0x60F1,
        SKEL_Spine3 = 0x60F2,
        SKEL_L_Clavicle = 0xFCD9,
        SKEL_L_UpperArm = 0xB1C5,
        SKEL_L_Forearm = 0xEEEB,
        SKEL_L_Hand = 0x49D9,
        SKEL_L_Finger00 = 0x67F2,
        SKEL_L_Finger01 = 0xFF9,
        SKEL_L_Finger02 = 0xFFA,
        SKEL_L_Finger10 = 0x67F3,
        SKEL_L_Finger11 = 0x1049,
        SKEL_L_Finger12 = 0x104A,
        SKEL_L_Finger20 = 0x67F4,
        SKEL_L_Finger21 = 0x1059,
        SKEL_L_Finger22 = 0x105A,
        SKEL_L_Finger30 = 0x67F5,
        SKEL_L_Finger31 = 0x1029,
        SKEL_L_Finger32 = 0x102A,
        SKEL_L_Finger40 = 0x67F6,
        SKEL_L_Finger41 = 0x1039,
        SKEL_L_Finger42 = 0x103A,
        PH_L_Hand = 0xEB95,
        IK_L_Hand = 0x8CBD,
        RB_L_ForeArmRoll = 0xEE4F,
        RB_L_ArmRoll = 0x1470,
        MH_L_Elbow = 0x58B7,
        SKEL_R_Clavicle = 0x29D2,
        SKEL_R_UpperArm = 0x9D4D,
        SKEL_R_Forearm = 0x6E5C,
        SKEL_R_Hand = 0xDEAD,
        SKEL_R_Finger00 = 0xE5F2,
        SKEL_R_Finger01 = 0xFA10,
        SKEL_R_Finger02 = 0xFA11,
        SKEL_R_Finger10 = 0xE5F3,
        SKEL_R_Finger11 = 0xFA60,
        SKEL_R_Finger12 = 0xFA61,
        SKEL_R_Finger20 = 0xE5F4,
        SKEL_R_Finger21 = 0xFA70,
        SKEL_R_Finger22 = 0xFA71,
        SKEL_R_Finger30 = 0xE5F5,
        SKEL_R_Finger31 = 0xFA40,
        SKEL_R_Finger32 = 0xFA41,
        SKEL_R_Finger40 = 0xE5F6,
        SKEL_R_Finger41 = 0xFA50,
        SKEL_R_Finger42 = 0xFA51,
        PH_R_Hand = 0x6F06,
        IK_R_Hand = 0x188E,
        RB_R_ForeArmRoll = 0xAB22,
        RB_R_ArmRoll = 0x90FF,
        MH_R_Elbow = 0xBB0,
        SKEL_Neck_1 = 0x9995,
        SKEL_Head = 0x796E,
        IK_Head = 0x322C,
        FACIAL_facialRoot = 0xFE2C,
        FB_L_Brow_Out_000 = 0xE3DB,
        FB_L_Lid_Upper_000 = 0xB2B6,
        FB_L_Eye_000 = 0x62AC,
        FB_L_CheekBone_000 = 0x542E,
        FB_L_Lip_Corner_000 = 0x74AC,
        FB_R_Lid_Upper_000 = 0xAA10,
        FB_R_Eye_000 = 0x6B52,
        FB_R_CheekBone_000 = 0x4B88,
        FB_R_Brow_Out_000 = 0x54C,
        FB_R_Lip_Corner_000 = 0x2BA6,
        FB_Brow_Centre_000 = 0x9149,
        FB_UpperLipRoot_000 = 0x4ED2,
        FB_UpperLip_000 = 0xF18F,
        FB_L_Lip_Top_000 = 0x4F37,
        FB_R_Lip_Top_000 = 0x4537,
        FB_Jaw_000 = 0xB4A0,
        FB_LowerLipRoot_000 = 0x4324,
        FB_LowerLip_000 = 0x508F,
        FB_L_Lip_Bot_000 = 0xB93B,
        FB_R_Lip_Bot_000 = 0xC33B,
        FB_Tongue_000 = 0xB987,
        RB_Neck_1 = 0x8B93,
        SPR_L_Breast = 0xFC8E,
        SPR_R_Breast = 0x885F,
        IK_Root = 0xDD1C,
        SKEL_Neck_2 = 0x5FD4,
        SKEL_Pelvis1 = 0xD003,
        SKEL_PelvisRoot = 0x45FC,
        SKEL_SADDLE = 0x9524,
        MH_L_CalfBack = 0x1013,
        MH_L_ThighBack = 0x600D,
        SM_L_Skirt = 0xC419,
        MH_R_CalfBack = 0xB013,
        MH_R_ThighBack = 0x51A3,
        SM_R_Skirt = 0x7712,
        SM_M_BackSkirtRoll = 0xDBB,
        SM_L_BackSkirtRoll = 0x40B2,
        SM_R_BackSkirtRoll = 0xC141,
        SM_M_FrontSkirtRoll = 0xCDBB,
        SM_L_FrontSkirtRoll = 0x9B69,
        SM_R_FrontSkirtRoll = 0x86F1,
        SM_CockNBalls_ROOT = 0xC67D,
        SM_CockNBalls = 0x9D34,
        MH_L_Finger00 = 0x8C63,
        MH_L_FingerBulge00 = 0x5FB8,
        MH_L_Finger10 = 0x8C53,
        MH_L_FingerTop00 = 0xA244,
        MH_L_HandSide = 0xC78A,
        MH_Watch = 0x2738,
        MH_L_Sleeve = 0x933C,
        MH_R_Finger00 = 0x2C63,
        MH_R_FingerBulge00 = 0x69B8,
        MH_R_Finger10 = 0x2C53,
        MH_R_FingerTop00 = 0xEF4B,
        MH_R_HandSide = 0x68FB,
        MH_R_Sleeve = 0x92DC,
        FACIAL_jaw = 0xB21,
        FACIAL_underChin = 0x8A95,
        FACIAL_L_underChin = 0x234E,
        FACIAL_chin = 0xB578,
        FACIAL_chinSkinBottom = 0x98BC,
        FACIAL_L_chinSkinBottom = 0x3E8F,
        FACIAL_R_chinSkinBottom = 0x9E8F,
        FACIAL_tongueA = 0x4A7C,
        FACIAL_tongueB = 0x4A7D,
        FACIAL_tongueC = 0x4A7E,
        FACIAL_tongueD = 0x4A7F,
        FACIAL_tongueE = 0x4A80,
        FACIAL_L_tongueE = 0x35F2,
        FACIAL_R_tongueE = 0x2FF2,
        FACIAL_L_tongueD = 0x35F1,
        FACIAL_R_tongueD = 0x2FF1,
        FACIAL_L_tongueC = 0x35F0,
        FACIAL_R_tongueC = 0x2FF0,
        FACIAL_L_tongueB = 0x35EF,
        FACIAL_R_tongueB = 0x2FEF,
        FACIAL_L_tongueA = 0x35EE,
        FACIAL_R_tongueA = 0x2FEE,
        FACIAL_chinSkinTop = 0x7226,
        FACIAL_L_chinSkinTop = 0x3EB3,
        FACIAL_chinSkinMid = 0x899A,
        FACIAL_L_chinSkinMid = 0x4427,
        FACIAL_L_chinSide = 0x4A5E,
        FACIAL_R_chinSkinMid = 0xF5AF,
        FACIAL_R_chinSkinTop = 0xF03B,
        FACIAL_R_chinSide = 0xAA5E,
        FACIAL_R_underChin = 0x2BF4,
        FACIAL_L_lipLowerSDK = 0xB9E1,
        FACIAL_L_lipLowerAnalog = 0x244A,
        FACIAL_L_lipLowerThicknessV = 0xC749,
        FACIAL_L_lipLowerThicknessH = 0xC67B,
        FACIAL_lipLowerSDK = 0x7285,
        FACIAL_lipLowerAnalog = 0xD97B,
        FACIAL_lipLowerThicknessV = 0xC5BB,
        FACIAL_lipLowerThicknessH = 0xC5ED,
        FACIAL_R_lipLowerSDK = 0xA034,
        FACIAL_R_lipLowerAnalog = 0xC2D9,
        FACIAL_R_lipLowerThicknessV = 0xC6E9,
        FACIAL_R_lipLowerThicknessH = 0xC6DB,
        FACIAL_nose = 0x20F1,
        FACIAL_L_nostril = 0x7322,
        FACIAL_L_nostrilThickness = 0xC15F,
        FACIAL_noseLower = 0xE05A,
        FACIAL_L_noseLowerThickness = 0x79D5,
        FACIAL_R_noseLowerThickness = 0x7975,
        FACIAL_noseTip = 0x6A60,
        FACIAL_R_nostril = 0x7922,
        FACIAL_R_nostrilThickness = 0x36FF,
        FACIAL_noseUpper = 0xA04F,
        FACIAL_L_noseUpper = 0x1FB8,
        FACIAL_noseBridge = 0x9BA3,
        FACIAL_L_nasolabialFurrow = 0x5ACA,
        FACIAL_L_nasolabialBulge = 0xCD78,
        FACIAL_L_cheekLower = 0x6907,
        FACIAL_L_cheekLowerBulge1 = 0xE3FB,
        FACIAL_L_cheekLowerBulge2 = 0xE3FC,
        FACIAL_L_cheekInner = 0xE7AB,
        FACIAL_L_cheekOuter = 0x8161,
        FACIAL_L_eyesackLower = 0x771B,
        FACIAL_L_eyeball = 0x1744,
        FACIAL_L_eyelidLower = 0x998C,
        FACIAL_L_eyelidLowerOuterSDK = 0xFE4C,
        FACIAL_L_eyelidLowerOuterAnalog = 0xB9AA,
        FACIAL_L_eyelashLowerOuter = 0xD7F6,
        FACIAL_L_eyelidLowerInnerSDK = 0xF151,
        FACIAL_L_eyelidLowerInnerAnalog = 0x8242,
        FACIAL_L_eyelashLowerInner = 0x4CCF,
        FACIAL_L_eyelidUpper = 0x97C1,
        FACIAL_L_eyelidUpperOuterSDK = 0xAF15,
        FACIAL_L_eyelidUpperOuterAnalog = 0x67FA,
        FACIAL_L_eyelashUpperOuter = 0x27B7,
        FACIAL_L_eyelidUpperInnerSDK = 0xD341,
        FACIAL_L_eyelidUpperInnerAnalog = 0xF092,
        FACIAL_L_eyelashUpperInner = 0x9B1F,
        FACIAL_L_eyesackUpperOuterBulge = 0xA559,
        FACIAL_L_eyesackUpperInnerBulge = 0x2F2A,
        FACIAL_L_eyesackUpperOuterFurrow = 0xC597,
        FACIAL_L_eyesackUpperInnerFurrow = 0x52A7,
        FACIAL_forehead = 0x9218,
        FACIAL_L_foreheadInner = 0x843,
        FACIAL_L_foreheadInnerBulge = 0x767C,
        FACIAL_L_foreheadOuter = 0x8DCB,
        FACIAL_skull = 0x4221,
        FACIAL_foreheadUpper = 0xF7D6,
        FACIAL_L_foreheadUpperInner = 0xCF13,
        FACIAL_L_foreheadUpperOuter = 0x509B,
        FACIAL_R_foreheadUpperInner = 0xCEF3,
        FACIAL_R_foreheadUpperOuter = 0x507B,
        FACIAL_L_temple = 0xAF79,
        FACIAL_L_ear = 0x19DD,
        FACIAL_L_earLower = 0x6031,
        FACIAL_L_masseter = 0x2810,
        FACIAL_L_jawRecess = 0x9C7A,
        FACIAL_L_cheekOuterSkin = 0x14A5,
        FACIAL_R_cheekLower = 0xF367,
        FACIAL_R_cheekLowerBulge1 = 0x599B,
        FACIAL_R_cheekLowerBulge2 = 0x599C,
        FACIAL_R_masseter = 0x810,
        FACIAL_R_jawRecess = 0x93D4,
        FACIAL_R_ear = 0x1137,
        FACIAL_R_earLower = 0x8031,
        FACIAL_R_eyesackLower = 0x777B,
        FACIAL_R_nasolabialBulge = 0xD61E,
        FACIAL_R_cheekOuter = 0xD32,
        FACIAL_R_cheekInner = 0x737C,
        FACIAL_R_noseUpper = 0x1CD6,
        FACIAL_R_foreheadInner = 0xE43,
        FACIAL_R_foreheadInnerBulge = 0x769C,
        FACIAL_R_foreheadOuter = 0x8FCB,
        FACIAL_R_cheekOuterSkin = 0xB334,
        FACIAL_R_eyesackUpperInnerFurrow = 0x9FAE,
        FACIAL_R_eyesackUpperOuterFurrow = 0x140F,
        FACIAL_R_eyesackUpperInnerBulge = 0xA359,
        FACIAL_R_eyesackUpperOuterBulge = 0x1AF9,
        FACIAL_R_nasolabialFurrow = 0x2CAA,
        FACIAL_R_temple = 0xAF19,
        FACIAL_R_eyeball = 0x1944,
        FACIAL_R_eyelidUpper = 0x7E14,
        FACIAL_R_eyelidUpperOuterSDK = 0xB115,
        FACIAL_R_eyelidUpperOuterAnalog = 0xF25A,
        FACIAL_R_eyelashUpperOuter = 0xE0A,
        FACIAL_R_eyelidUpperInnerSDK = 0xD541,
        FACIAL_R_eyelidUpperInnerAnalog = 0x7C63,
        FACIAL_R_eyelashUpperInner = 0x8172,
        FACIAL_R_eyelidLower = 0x7FDF,
        FACIAL_R_eyelidLowerOuterSDK = 0x1BD,
        FACIAL_R_eyelidLowerOuterAnalog = 0x457B,
        FACIAL_R_eyelashLowerOuter = 0xBE49,
        FACIAL_R_eyelidLowerInnerSDK = 0xF351,
        FACIAL_R_eyelidLowerInnerAnalog = 0xE13,
        FACIAL_R_eyelashLowerInner = 0x3322,
        FACIAL_L_lipUpperSDK = 0x8F30,
        FACIAL_L_lipUpperAnalog = 0xB1CF,
        FACIAL_L_lipUpperThicknessH = 0x37CE,
        FACIAL_L_lipUpperThicknessV = 0x38BC,
        FACIAL_lipUpperSDK = 0x1774,
        FACIAL_lipUpperAnalog = 0xE064,
        FACIAL_lipUpperThicknessH = 0x7993,
        FACIAL_lipUpperThicknessV = 0x7981,
        FACIAL_L_lipCornerSDK = 0xB1C,
        FACIAL_L_lipCornerAnalog = 0xE568,
        FACIAL_L_lipCornerThicknessUpper = 0x7BC,
        FACIAL_L_lipCornerThicknessLower = 0xDD42,
        FACIAL_R_lipUpperSDK = 0x7583,
        FACIAL_R_lipUpperAnalog = 0x51CF,
        FACIAL_R_lipUpperThicknessH = 0x382E,
        FACIAL_R_lipUpperThicknessV = 0x385C,
        FACIAL_R_lipCornerSDK = 0xB3C,
        FACIAL_R_lipCornerAnalog = 0xEE0E,
        FACIAL_R_lipCornerThicknessUpper = 0x54C3,
        FACIAL_R_lipCornerThicknessLower = 0x2BBA,
        MH_MulletRoot = 0x3E73,
        MH_MulletScaler = 0xA1C2,
        MH_Hair_Scale = 0xC664,
        MH_Hair_Crown = 0x1675,
        SM_Torch = 0x8D6,
        FX_Light = 0x8959,
        FX_Light_Scale = 0x5038,
        FX_Light_Switch = 0xE18E,
        BagRoot = 0xAD09,
        BagPivotROOT = 0xB836,
        BagPivot = 0x4D11,
        BagBody = 0xAB6D,
        BagBone_R = 0x937,
        BagBone_L = 0x991,
        SM_LifeSaver_Front = 0x9420,
        SM_R_Pouches_ROOT = 0x2962,
        SM_R_Pouches = 0x4141,
        SM_L_Pouches_ROOT = 0x2A02,
        SM_L_Pouches = 0x4B41,
        SM_Suit_Back_Flapper = 0xDA2D,
        SPR_CopRadio = 0x8245,
        SM_LifeSaver_Back = 0x2127,
        MH_BlushSlider = 0xA0CE,
        SKEL_Tail_01 = 0x347,
        SKEL_Tail_02 = 0x348,
        MH_L_Concertina_B = 0xC988,
        MH_L_Concertina_A = 0xC987,
        MH_R_Concertina_B = 0xC8E8,
        MH_R_Concertina_A = 0xC8E7,
        MH_L_ShoulderBladeRoot = 0x8711,
        MH_L_ShoulderBlade = 0x4EAF,
        MH_R_ShoulderBladeRoot = 0x3A0A,
        MH_R_ShoulderBlade = 0x54AF,
        FB_R_Ear_000 = 0x6CDF,
        SPR_R_Ear = 0x63B6,
        FB_L_Ear_000 = 0x6439,
        SPR_L_Ear = 0x5B10,
        FB_TongueA_000 = 0x4206,
        FB_TongueB_000 = 0x4207,
        FB_TongueC_000 = 0x4208,
        SKEL_L_Toe1 = 0x1D6B,
        SKEL_R_Toe1 = 0xB23F,
        SKEL_Tail_03 = 0x349,
        SKEL_Tail_04 = 0x34A,
        SKEL_Tail_05 = 0x34B,
        SPR_Gonads_ROOT = 0xBFDE,
        SPR_Gonads = 0x1C00,
        FB_L_Brow_Out_001 = 0xE3DB,
        FB_L_Lid_Upper_001 = 0xB2B6,
        FB_L_Eye_001 = 0x62AC,
        FB_L_CheekBone_001 = 0x542E,
        FB_L_Lip_Corner_001 = 0x74AC,
        FB_R_Lid_Upper_001 = 0xAA10,
        FB_R_Eye_001 = 0x6B52,
        FB_R_CheekBone_001 = 0x4B88,
        FB_R_Brow_Out_001 = 0x54C,
        FB_R_Lip_Corner_001 = 0x2BA6,
        FB_Brow_Centre_001 = 0x9149,
        FB_UpperLipRoot_001 = 0x4ED2,
        FB_UpperLip_001 = 0xF18F,
        FB_L_Lip_Top_001 = 0x4F37,
        FB_R_Lip_Top_001 = 0x4537,
        FB_Jaw_001 = 0xB4A0,
        FB_LowerLipRoot_001 = 0x4324,
        FB_LowerLip_001 = 0x508F,
        FB_L_Lip_Bot_001 = 0xB93B,
        FB_R_Lip_Bot_001 = 0xC33B,
        FB_Tongue_001 = 0xB987
    };

    [TypeConverter(typeof(ExpandableObjectConverter))] public class Bone : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength
        {
            get { return 80; }
        }

        // structure data
        public Quaternion Rotation { get; set; }
        public Vector3 Translation { get; set; }
        public uint Unknown_1Ch; // 0x00000000 RHW?
        public Vector3 Scale { get; set; }
        public float Unknown_2Ch { get; set; } = 1.0f; // 1.0  RHW?
        public short NextSiblingIndex { get; set; } //limb end index? IK chain?
        public short ParentIndex { get; set; }
        public uint Unknown_34h; // 0x00000000
        public ulong NamePointer { get; set; }
        public EBoneFlags Flags { get; set; }
        public short Index { get; set; }
        public ushort Tag { get; set; }
        public short Index2 { get; set; }//always same as Index
        public ulong Unknown_48h; // 0x0000000000000000

        // reference data
        public string Name { get; set; }

        public Bone Parent { get; set; }

        private string_r NameBlock = null;


        //used by CW for animating skeletons.
        public Quaternion AnimRotation;//relative to parent
        public Vector3 AnimTranslation;//relative to parent
        public Vector3 AnimScale;
        public Matrix AnimTransform;//absolute world transform, animated
        public Matrix BindTransformInv;//inverse of bind pose transform
        public Matrix SkinTransform;//transform to use for skin meshes
        public Matrix AbsTransform;//original absolute transform from loaded file, calculated from bones hierarchy
        public Vector4 TransformUnk { get; set; } //unknown value (column 4) from skeleton's transform array, used for IO purposes

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Rotation = new Quaternion(reader.ReadVector4());
            this.Translation = reader.ReadVector3();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Scale = reader.ReadVector3();
            this.Unknown_2Ch = reader.ReadSingle();
            this.NextSiblingIndex = reader.ReadInt16();
            this.ParentIndex = reader.ReadInt16();
            this.Unknown_34h = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.Flags = (EBoneFlags)reader.ReadUInt16();
            this.Index = reader.ReadInt16();
            this.Tag = reader.ReadUInt16();
            this.Index2 = reader.ReadInt16();
            this.Unknown_48h = reader.ReadUInt64();

            // read reference data
            this.Name = reader.ReadStringAt(//BlockAt<string_r>(
                this.NamePointer // offset
            );

            //if (Index2 != Index)
            //{ }//no hits

            AnimRotation = Rotation;
            AnimTranslation = Translation;
            AnimScale = Scale;


            //if (Unknown_1Ch != 0)
            //{ }
            //if (Unknown_34h != 0)
            //{ }
            //if (Unknown_48h != 0)
            //{ }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.NameBlock != null ? this.NameBlock.FilePosition : 0);

            // write structure data
            writer.Write(this.Rotation.ToVector4());
            writer.Write(this.Translation);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Scale);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.NextSiblingIndex);
            writer.Write(this.ParentIndex);
            writer.Write(this.Unknown_34h);
            writer.Write(this.NamePointer);
            writer.Write((ushort)this.Flags);
            writer.Write(this.Index);
            writer.Write(this.Tag);
            writer.Write(this.Index2);
            writer.Write(this.Unknown_48h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YdrXml.StringTag(sb, indent, "Name", Name);
            YdrXml.ValueTag(sb, indent, "Tag", Tag.ToString());
            YdrXml.ValueTag(sb, indent, "Index", Index.ToString());
            YdrXml.ValueTag(sb, indent, "ParentIndex", ParentIndex.ToString());
            YdrXml.ValueTag(sb, indent, "SiblingIndex", NextSiblingIndex.ToString());
            YdrXml.StringTag(sb, indent, "Flags", Flags.ToString());
            YdrXml.SelfClosingTag(sb, indent, "Translation " + FloatUtil.GetVector3XmlString(Translation));
            YdrXml.SelfClosingTag(sb, indent, "Rotation " + FloatUtil.GetVector4XmlString(Rotation.ToVector4()));
            YdrXml.SelfClosingTag(sb, indent, "Scale " + FloatUtil.GetVector3XmlString(Scale));
            YdrXml.SelfClosingTag(sb, indent, "TransformUnk " + FloatUtil.GetVector4XmlString(TransformUnk));
        }
        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            Tag = (ushort)Xml.GetChildUIntAttribute(node, "Tag", "value");
            Index = (short)Xml.GetChildIntAttribute(node, "Index", "value");
            Index2 = Index;
            ParentIndex = (short)Xml.GetChildIntAttribute(node, "ParentIndex", "value");
            NextSiblingIndex = (short)Xml.GetChildIntAttribute(node, "SiblingIndex", "value");
            Flags = Xml.GetChildEnumInnerText<EBoneFlags>(node, "Flags");
            Translation = Xml.GetChildVector3Attributes(node, "Translation");
            Rotation = Xml.GetChildVector4Attributes(node, "Rotation").ToQuaternion();
            Scale = Xml.GetChildVector3Attributes(node, "Scale");
            TransformUnk = Xml.GetChildVector4Attributes(node, "TransformUnk");
        }

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
            return Tag.ToString() + ": " + Name;
        }


        public void UpdateAnimTransform()
        {
            AnimTransform = Matrix.AffineTransformation(1.0f, AnimRotation, AnimTranslation);
            AnimTransform.ScaleVector *= AnimScale;
            if (Parent != null)
            {
                AnimTransform = AnimTransform * Parent.AnimTransform;
            }

            ////AnimTransform = Matrix.AffineTransformation(1.0f, AnimRotation, AnimTranslation);//(local transform)

            //var pos = AnimTranslation;
            //var ori = AnimRotation;
            //var sca = AnimScale;
            //var pbone = Parent;
            //while (pbone != null)
            //{
            //    pos = pbone.AnimRotation.Multiply(pos /** pbone.AnimScale*/) + pbone.AnimTranslation;
            //    ori = pbone.AnimRotation * ori;
            //    pbone = pbone.Parent;
            //}
            //AnimTransform = Matrix.AffineTransformation(1.0f, ori, pos);//(global transform)
            //AnimTransform.ScaleVector *= sca;
        }
        public void UpdateSkinTransform()
        {
            SkinTransform = BindTransformInv * AnimTransform;
            //SkinTransform = Matrix.Identity;//(for testing)
        }

        public void ResetAnimTransform()
        {
            AnimRotation = Rotation;
            AnimTranslation = Translation;
            AnimScale = Scale;
            UpdateAnimTransform();
            UpdateSkinTransform();
        }

        public static uint ElfHash_Uppercased(string str)
        {
            uint hash = 0;
            uint x = 0;
            uint i = 0;

            for (i = 0; i < str.Length; i++)
            {
                var c = ((byte)str[(int)i]);
                if ((byte)(c - 'a') <= 25u) // to uppercase
                    c -= 32;

                hash = (hash << 4) + c;

                if ((x = hash & 0xF0000000) != 0)
                {
                    hash ^= (x >> 24);
                }

                hash &= ~x;
            }

            return hash;
        }
        public static ushort CalculateBoneHash(string boneName)
        {
            return (ushort)(ElfHash_Uppercased(boneName) % 0xFE8F + 0x170);
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class Joints : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint VFT { get; set; } = 1080130656;
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong RotationLimitsPointer { get; set; }
        public ulong TranslationLimitsPointer { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ushort RotationLimitsCount { get; set; }
        public ushort TranslationLimitsCount { get; set; }
        public ushort Unknown_34h; // 0x0000
        public ushort Unknown_36h = 1; // 0x0001
        public ulong Unknown_38h; // 0x0000000000000000

        // reference data
        public JointRotationLimit_s[] RotationLimits { get; set; }
        public JointTranslationLimit_s[] TranslationLimits { get; set; }

        private ResourceSystemStructBlock<JointRotationLimit_s> RotationLimitsBlock = null; //for saving only
        private ResourceSystemStructBlock<JointTranslationLimit_s> TranslationLimitsBlock = null;


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.RotationLimitsPointer = reader.ReadUInt64();
            this.TranslationLimitsPointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.RotationLimitsCount = reader.ReadUInt16();
            this.TranslationLimitsCount = reader.ReadUInt16();
            this.Unknown_34h = reader.ReadUInt16();
            this.Unknown_36h = reader.ReadUInt16();
            this.Unknown_38h = reader.ReadUInt64();

            // read reference data
            this.RotationLimits = reader.ReadStructsAt<JointRotationLimit_s>(this.RotationLimitsPointer, this.RotationLimitsCount);
            this.TranslationLimits = reader.ReadStructsAt<JointTranslationLimit_s>(this.TranslationLimitsPointer, this.TranslationLimitsCount);

            //if (Unknown_4h != 1)
            //{ }
            //if (Unknown_8h != 0)
            //{ }
            //if (Unknown_20h != 0)
            //{ }
            //if (Unknown_28h != 0)
            //{ }
            //if (Unknown_34h != 0)
            //{ }
            //if (Unknown_36h != 1)
            //{ }
            //if (Unknown_38h != 0)
            //{ }


        }
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
            writer.Write(this.RotationLimitsPointer);
            writer.Write(this.TranslationLimitsPointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.RotationLimitsCount);
            writer.Write(this.TranslationLimitsCount);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_36h);
            writer.Write(this.Unknown_38h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            if (RotationLimits != null)
            {
                YdrXml.WriteItemArray(sb, RotationLimits, indent, "RotationLimits");
            }
            if (TranslationLimits != null)
            {
                YdrXml.WriteItemArray(sb, TranslationLimits, indent, "TranslationLimits");
            }
        }
        public void ReadXml(XmlNode node)
        {
            RotationLimits = XmlMeta.ReadItemArray<JointRotationLimit_s>(node, "RotationLimits");
            TranslationLimits = XmlMeta.ReadItemArray<JointTranslationLimit_s>(node, "TranslationLimits");
        }

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

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct JointRotationLimit_s : IMetaXmlItem
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

        private void Init()
        {
            var pi = (float)Math.PI;
            Unknown_0h = 0;
            Unknown_4h = 0;
            BoneId = 0;
            Unknown_Ah = 0;
            Unknown_Ch = 1;
            Unknown_10h = 3;
            Unknown_14h = 0;
            Unknown_18h = 0;
            Unknown_1Ch = 0;
            Unknown_20h = 0;
            Unknown_24h = 0;
            Unknown_28h = 0;
            Unknown_2Ch = 1.0f;
            Unknown_30h = 0;
            Unknown_34h = 0;
            Unknown_38h = 0;
            Unknown_3Ch = 0;
            Unknown_40h = 1.0f;
            Unknown_44h = 0;
            Unknown_48h = 0;
            Unknown_4Ch = 0;
            Unknown_50h = -pi;
            Unknown_54h = pi;
            Unknown_58h = 1.0f;
            Min = Vector3.Zero;
            Max = Vector3.Zero;
            Unknown_74h = pi;
            Unknown_78h = -pi;
            Unknown_7Ch = pi;
            Unknown_80h = pi;
            Unknown_84h = -pi;
            Unknown_88h = pi;
            Unknown_8Ch = pi;
            Unknown_90h = -pi;
            Unknown_94h = pi;
            Unknown_98h = pi;
            Unknown_9Ch = -pi;
            Unknown_A0h = pi;
            Unknown_A4h = pi;
            Unknown_A8h = -pi;
            Unknown_ACh = pi;
            Unknown_B0h = pi;
            Unknown_B4h = -pi;
            Unknown_B8h = pi;
            Unknown_BCh = 0x100;
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            YdrXml.ValueTag(sb, indent, "BoneId", BoneId.ToString());
            YdrXml.ValueTag(sb, indent, "UnknownA", Unknown_Ah.ToString());
            YdrXml.SelfClosingTag(sb, indent, "Min " + FloatUtil.GetVector3XmlString(Min));
            YdrXml.SelfClosingTag(sb, indent, "Max " + FloatUtil.GetVector3XmlString(Max));
        }
        public void ReadXml(XmlNode node)
        {
            Init();
            BoneId = (ushort)Xml.GetChildUIntAttribute(node, "BoneId", "value");
            Unknown_Ah = (ushort)Xml.GetChildUIntAttribute(node, "UnknownA", "value");
            Min = Xml.GetChildVector3Attributes(node, "Min");
            Max = Xml.GetChildVector3Attributes(node, "Max");
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct JointTranslationLimit_s : IMetaXmlItem
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

        public void WriteXml(StringBuilder sb, int indent)
        {
            YdrXml.ValueTag(sb, indent, "BoneId", BoneId.ToString());
            YdrXml.SelfClosingTag(sb, indent, "Min " + FloatUtil.GetVector3XmlString(Min));
            YdrXml.SelfClosingTag(sb, indent, "Max " + FloatUtil.GetVector3XmlString(Max));
        }
        public void ReadXml(XmlNode node)
        {
            BoneId = (ushort)Xml.GetChildUIntAttribute(node, "BoneId", "value");
            Min = Xml.GetChildVector3Attributes(node, "Min");
            Max = Xml.GetChildVector3Attributes(node, "Max");
        }
    }





    [TypeConverter(typeof(ExpandableObjectConverter))] public class DrawableModelsBlock : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get
            {
                long len = 0;
                len += ListLength(High, len);
                len += ListLength(Med, len);
                len += ListLength(Low, len);
                len += ListLength(VLow, len);
                len += ListLength(Extra, len);
                return len;
            }
        }

        public DrawableBase Owner;

        public DrawableModel[] High { get; set; }
        public DrawableModel[] Med { get; set; }
        public DrawableModel[] Low { get; set; }
        public DrawableModel[] VLow { get; set; }
        public DrawableModel[] Extra { get; set; } //shouldn't be used

        public ResourcePointerListHeader HighHeader { get; set; }
        public ResourcePointerListHeader MedHeader { get; set; }
        public ResourcePointerListHeader LowHeader { get; set; }
        public ResourcePointerListHeader VLowHeader { get; set; }
        public ResourcePointerListHeader ExtraHeader { get; set; }

        public ulong[] HighPointers { get; set; }
        public ulong[] MedPointers { get; set; }
        public ulong[] LowPointers { get; set; }
        public ulong[] VLowPointers { get; set; }
        public ulong[] ExtraPointers { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            Owner = parameters[0] as DrawableBase;
            var pos = (ulong)reader.Position;
            var highPointer = (Owner?.DrawableModelsHighPointer ?? 0);
            var medPointer = (Owner?.DrawableModelsMediumPointer ?? 0);
            var lowPointer = (Owner?.DrawableModelsLowPointer ?? 0);
            var vlowPointer = (Owner?.DrawableModelsVeryLowPointer ?? 0);
            var extraPointer = (pos != highPointer) ? pos : 0;

            if (highPointer != 0)
            {
                HighHeader = reader.ReadStructAt<ResourcePointerListHeader>((long)highPointer);
                HighPointers = reader.ReadUlongsAt(HighHeader.Pointer, HighHeader.Capacity, false);
                High = reader.ReadBlocks<DrawableModel>(HighPointers);
            }
            if (medPointer != 0)
            {
                MedHeader = reader.ReadStructAt<ResourcePointerListHeader>((long)medPointer);
                MedPointers = reader.ReadUlongsAt(MedHeader.Pointer, MedHeader.Capacity, false);
                Med = reader.ReadBlocks<DrawableModel>(MedPointers);
            }
            if (lowPointer != 0)
            {
                LowHeader = reader.ReadStructAt<ResourcePointerListHeader>((long)lowPointer);
                LowPointers = reader.ReadUlongsAt(LowHeader.Pointer, LowHeader.Capacity, false);
                Low = reader.ReadBlocks<DrawableModel>(LowPointers);
            }
            if (vlowPointer != 0)
            {
                VLowHeader = reader.ReadStructAt<ResourcePointerListHeader>((long)vlowPointer);
                VLowPointers = reader.ReadUlongsAt(VLowHeader.Pointer, VLowHeader.Capacity, false);
                VLow = reader.ReadBlocks<DrawableModel>(VLowPointers);
            }
            if (extraPointer != 0)
            {
                ExtraHeader = reader.ReadStructAt<ResourcePointerListHeader>((long)extraPointer);
                ExtraPointers = reader.ReadUlongsAt(ExtraHeader.Pointer, ExtraHeader.Capacity, false);
                Extra = reader.ReadBlocks<DrawableModel>(ExtraPointers);
            }
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {

            ResourcePointerListHeader makeHeader(ref long p, int c)
            {
                p += Pad(p);
                var h = new ResourcePointerListHeader() { Pointer = (ulong)(p + 16), Count = (ushort)c, Capacity = (ushort)c };
                p += HeaderLength(c);
                return h;
            }
            ulong[] makePointers(ref long p, DrawableModel[] a)
            {
                var ptrs = new ulong[a.Length];
                for (int i = 0; i < a.Length; i++)
                {
                    p += Pad(p);
                    ptrs[i] = (ulong)p;
                    p += a[i].BlockLength;
                }
                return ptrs;
            }
            void write(ResourcePointerListHeader h, ulong[] p, DrawableModel[] a)
            {
                writer.WritePadding(16);
                writer.WriteStruct(h);
                writer.WriteUlongs(p);
                for (int i = 0; i < a.Length; i++)
                {
                    writer.WritePadding(16);
                    writer.WriteBlock(a[i]);
                }
            }

            var ptr = writer.Position;
            if (High != null)
            {
                HighHeader = makeHeader(ref ptr, High.Length);
                HighPointers = makePointers(ref ptr, High);
                write(HighHeader, HighPointers, High);
            }
            if (Med != null)
            {
                MedHeader = makeHeader(ref ptr, Med.Length);
                MedPointers = makePointers(ref ptr, Med);
                write(MedHeader, MedPointers, Med);
            }
            if (Low != null)
            {
                LowHeader = makeHeader(ref ptr, Low.Length);
                LowPointers = makePointers(ref ptr, Low);
                write(LowHeader, LowPointers, Low);
            }
            if (VLow != null)
            {
                VLowHeader = makeHeader(ref ptr, VLow.Length);
                VLowPointers = makePointers(ref ptr, VLow);
                write(VLowHeader, VLowPointers, VLow);
            }
            if (Extra != null)
            {
                ExtraHeader = makeHeader(ref ptr, Extra.Length);
                ExtraPointers = makePointers(ref ptr, Extra);
                write(ExtraHeader, ExtraPointers, Extra);
            }

        }


        private long Pad(long o) => ((16 - (o % 16)) % 16);
        private long HeaderLength(int listlength) => 16 + ((listlength) * 8);
        private long ListLength(DrawableModel[] list, long o)
        {
            if (list == null) return 0;
            long l = 0;
            l += HeaderLength(list.Length);
            foreach (var m in list) l += Pad(l) + m.BlockLength;
            return Pad(o) + l;
        }


        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            var parts = new List<Tuple<long, IResourceBlock>>();
            parts.AddRange(base.GetParts());

            void addParts(ref long p, DrawableModel[] a)
            {
                if (a == null) return;
                p += Pad(p);
                p += HeaderLength(a.Length);
                foreach (var m in a)
                {
                    p += Pad(p);
                    parts.Add(new Tuple<long, IResourceBlock>(p, m));
                    p += m.BlockLength;
                }
            }

            var ptr = (long)0;
            addParts(ref ptr, High);
            addParts(ref ptr, Med);
            addParts(ref ptr, Low);
            addParts(ref ptr, VLow);
            addParts(ref ptr, Extra);

            return parts.ToArray();
        }


        public long GetHighPointer()
        {
            if (High == null) return 0;
            return FilePosition;
        }
        public long GetMedPointer()
        {
            if (Med == null) return 0;
            var p = FilePosition;
            p += ListLength(High, p);
            p += Pad(p);
            return p;
        }
        public long GetLowPointer()
        {
            if (Low == null) return 0;
            var p = GetMedPointer();
            p += ListLength(Med, p);
            p += Pad(p);
            return p;
        }
        public long GetVLowPointer()
        {
            if (VLow == null) return 0;
            var p = GetLowPointer();
            p += ListLength(Low, p);
            p += Pad(p);
            return p;
        }
        public long GetExtraPointer()
        {
            if (Extra == null) return 0;
            var p = GetVLowPointer();
            p += ListLength(VLow, p);
            p += Pad(p);
            return p;
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class DrawableModel : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength
        {
            get 
            {
                var off = (long)48;
                off += (GeometriesCount1 * 2); //ShaderMapping
                if (GeometriesCount1 == 1) off += 6;
                else off += ((16 - (off % 16)) % 16);
                off += (GeometriesCount1 * 8); //Geometries pointers
                off += ((16 - (off % 16)) % 16);
                off += (GeometriesCount1 + ((GeometriesCount1 > 1) ? 1 : 0)) * 32; //BoundsData
                for (int i = 0; i < GeometriesCount1; i++)
                {
                    var geom = (Geometries != null) ? Geometries[i] : null;
                    if (geom != null)
                    {
                        off += ((16 - (off % 16)) % 16);
                        off += geom.BlockLength; //Geometries
                    }
                }
                return off;
            }
        }

        // structure data
        public uint VFT { get; set; } = 1080101528;
        public uint Unknown_4h = 1; // 0x00000001
        public ulong GeometriesPointer { get; set; }
        public ushort GeometriesCount1 { get; set; }
        public ushort GeometriesCount2 { get; set; }//always equal to GeometriesCount1
        public uint Unknown_14h; // 0x00000000
        public ulong BoundsPointer { get; set; }
        public ulong ShaderMappingPointer { get; set; }
        public uint SkeletonBinding { get; set; }//4th byte is bone index, 2nd byte for skin meshes
        public ushort RenderMaskFlags { get; set; } //First byte is called "Mask" in GIMS EVO
        public ushort GeometriesCount3 { get; set; } //always equal to GeometriesCount1, is it ShaderMappingCount?
        public ushort[] ShaderMapping { get; set; }
        public ulong[] GeometryPointers { get; set; }
        public AABB_s[] BoundsData { get; set; }
        public DrawableGeometry[] Geometries { get; set; }

        public byte BoneIndex
        {
            get  { return (byte)((SkeletonBinding >> 24) & 0xFF); }
            set { SkeletonBinding = (SkeletonBinding & 0x00FFFFFF) + ((value & 0xFFu) << 24); }
        }
        public byte SkeletonBindUnk2 //always 0
        {
            get { return (byte)((SkeletonBinding >> 16) & 0xFF); }
            set { SkeletonBinding = (SkeletonBinding & 0xFF00FFFF) + ((value & 0xFFu) << 16); }
        }
        public byte HasSkin //only 0 or 1
        {
            get { return (byte)((SkeletonBinding >> 8) & 0xFF); }
            set { SkeletonBinding = (SkeletonBinding & 0xFFFF00FF) + ((value & 0xFFu) << 8); }
        }
        public byte SkeletonBindUnk1 //only 0 or 43 (in rare cases, see below)
        {
            get { return (byte)((SkeletonBinding >> 0) & 0xFF); }
            set { SkeletonBinding = (SkeletonBinding & 0xFFFFFF00) + ((value & 0xFFu) << 0); }
        }

        public byte RenderMask
        {
            get { return (byte)((RenderMaskFlags >> 0) & 0xFF); }
            set { RenderMaskFlags = (ushort)((RenderMaskFlags & 0xFF00u) + ((value & 0xFFu) << 0)); }
        }
        public byte Flags
        {
            get { return (byte)((RenderMaskFlags >> 8) & 0xFF); }
            set { RenderMaskFlags = (ushort)((RenderMaskFlags & 0xFFu) + ((value & 0xFFu) << 8)); }
        }






        public long MemoryUsage
        {
            get
            {
                long val = 0;
                if (Geometries != null)
                {
                    foreach(var geom in Geometries)
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

            this.ShaderMapping = reader.ReadUshortsAt(this.ShaderMappingPointer, this.GeometriesCount1, false);
            this.GeometryPointers = reader.ReadUlongsAt(this.GeometriesPointer, this.GeometriesCount1, false);
            this.BoundsData = reader.ReadStructsAt<AABB_s>(this.BoundsPointer, (uint)(this.GeometriesCount1 > 1 ? this.GeometriesCount1 + 1 : this.GeometriesCount1), false);
            this.Geometries = reader.ReadBlocks<DrawableGeometry>(this.GeometryPointers);

            if (Geometries != null)
            {
                for (int i = 0; i < Geometries.Length; i++)
                {
                    var geom = Geometries[i];
                    if (geom != null)
                    {
                        geom.ShaderID = ((ShaderMapping != null) && (i < ShaderMapping.Length)) ? ShaderMapping[i] : (ushort)0;
                        geom.AABB = (BoundsData != null) ? ((BoundsData.Length > 1) && ((i + 1) < BoundsData.Length)) ? BoundsData[i + 1] : BoundsData[0] : new AABB_s();
                    }
                }
            }


            ////just testing!
            /*
            //var pos = (ulong)reader.Position;
            //var off = (ulong)0;
            //if (ShaderMappingPointer != (pos + off))
            //{ }//no hit
            //off += (ulong)(GeometriesCount1 * 2); //ShaderMapping
            //if (GeometriesCount1 == 1) off += 6;
            //else off += ((16 - (off % 16)) % 16);
            //if (GeometriesPointer != (pos + off))
            //{ }//no hit
            //off += (ulong)(GeometriesCount1 * 8); //Geometries pointers
            //off += ((16 - (off % 16)) % 16);
            //if (BoundsPointer != (pos + off))
            //{ }//no hit
            //off += (ulong)((GeometriesCount1 + ((GeometriesCount1 > 1) ? 1 : 0)) * 32); //BoundsData
            //if ((GeometryPointers != null) && (Geometries != null))
            //{
            //    for (int i = 0; i < GeometriesCount1; i++)
            //    {
            //        var geomptr = GeometryPointers[i];
            //        var geom = Geometries[i];
            //        if (geom != null)
            //        {
            //            off += ((16 - (off % 16)) % 16);
            //            if (geomptr != (pos + off))
            //            { }//no hit
            //            off += (ulong)geom.BlockLength;
            //        }
            //        else
            //        { }//no hit
            //    }
            //}
            //else
            //{ }//no hit

            //if (SkeletonBindUnk2 != 0)
            //{ }//no hit
            //switch (SkeletonBindUnk1)
            //{
            //    case 0:
            //        break;
            //    case 43://des_plog_light_root.ydr, des_heli_scrapyard_skin002.ydr, v_74_it1_ceiling_smoke_02_skin.ydr, buzzard2.yft, vader.yft, zombiea.yft
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (HasSkin)
            //{
            //    case 0:
            //    case 1:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_14h != 0)
            //{ }//no hit
            */
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.GeometriesCount1 = (ushort)(this.Geometries != null ? this.Geometries.Length : 0);
            this.GeometriesCount2 = this.GeometriesCount1;//is this correct?
            this.GeometriesCount3 = this.GeometriesCount1;//is this correct?
            
            long pad(long o) => ((16 - (o % 16)) % 16);
            var off = writer.Position + 48;
            this.ShaderMappingPointer = (ulong)off;
            off += (GeometriesCount1 * 2); //ShaderMapping
            if (GeometriesCount1 == 1) off += 6;
            else off += pad(off);
            this.GeometriesPointer = (ulong)off;
            off += (GeometriesCount1 * 8); //Geometries pointers
            off += pad(off);
            this.BoundsPointer = (ulong)off;
            off += (BoundsData.Length) * 32; //BoundsData
            this.GeometryPointers = new ulong[GeometriesCount1];
            for (int i = 0; i < GeometriesCount1; i++)
            {
                var geom = (Geometries != null) ? Geometries[i] : null;
                if (geom != null)
                {
                    off += pad(off);
                    this.GeometryPointers[i] = (ulong)off;
                    off += geom.BlockLength; //Geometries
                }
            }



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


            for (int i = 0; i < GeometriesCount1; i++)
            {
                writer.Write(ShaderMapping[i]);
            }
            if (GeometriesCount1 == 1)
            {
                writer.Write(new byte[6]);
            }
            else
            {
                writer.WritePadding(16);
            }
            for (int i = 0; i < GeometriesCount1; i++)
            {
                writer.Write(GeometryPointers[i]);
            }
            writer.WritePadding(16);
            for (int i = 0; i < BoundsData.Length; i++)
            {
                writer.WriteStruct(BoundsData[i]);
            }
            for (int i = 0; i < GeometriesCount1; i++)
            {
                var geom = (Geometries != null) ? Geometries[i] : null;
                if (geom != null)
                {
                    writer.WritePadding(16);
                    writer.WriteBlock(geom);
                }
            }

        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YdrXml.ValueTag(sb, indent, "RenderMask", RenderMask.ToString());
            YdrXml.ValueTag(sb, indent, "Flags", Flags.ToString());
            YdrXml.ValueTag(sb, indent, "HasSkin", HasSkin.ToString());
            YdrXml.ValueTag(sb, indent, "BoneIndex", BoneIndex.ToString());
            YdrXml.ValueTag(sb, indent, "Unknown1", SkeletonBindUnk1.ToString());

            if (Geometries != null)
            {
                YdrXml.WriteItemArray(sb, Geometries, indent, "Geometries");
            }

        }
        public void ReadXml(XmlNode node)
        {
            RenderMask = (byte)Xml.GetChildUIntAttribute(node, "RenderMask", "value");
            Flags = (byte)Xml.GetChildUIntAttribute(node, "Flags", "value");
            HasSkin = (byte)Xml.GetChildUIntAttribute(node, "HasSkin", "value");
            BoneIndex = (byte)Xml.GetChildUIntAttribute(node, "BoneIndex", "value");
            SkeletonBindUnk1 = (byte)Xml.GetChildUIntAttribute(node, "Unknown1", "value");

            var aabbs = new List<AABB_s>();
            var shids = new List<ushort>();
            var min = new Vector4(float.MaxValue);
            var max = new Vector4(float.MinValue);
            var geoms = XmlMeta.ReadItemArray<DrawableGeometry>(node, "Geometries");
            if (geoms != null)
            {
                Geometries = geoms;
                foreach (var geom in geoms)
                {
                    aabbs.Add(geom.AABB);
                    shids.Add(geom.ShaderID);
                    min = Vector4.Min(min, geom.AABB.Min);
                    max = Vector4.Max(max, geom.AABB.Max);
                }
                GeometriesCount1 = GeometriesCount2 = GeometriesCount3 = (ushort)geoms.Length;
            }
            if (aabbs.Count > 1)
            {
                var outeraabb = new AABB_s() { Min = min, Max = max };
                aabbs.Insert(0, outeraabb);
            }

            BoundsData = (aabbs.Count > 0) ? aabbs.ToArray() : null;
            ShaderMapping = (shids.Count > 0) ? shids.ToArray() : null;
        }


        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            var parts = new List<Tuple<long, IResourceBlock>>();
            parts.AddRange(base.GetParts());

            var off = (long)48;
            off += (GeometriesCount1 * 2); //ShaderMapping
            if (GeometriesCount1 == 1) off += 6;
            else off += ((16 - (off % 16)) % 16);
            off += (GeometriesCount1 * 8); //Geometries pointers
            off += ((16 - (off % 16)) % 16);
            off += (GeometriesCount1 + ((GeometriesCount1 > 1) ? 1 : 0)) * 32; //BoundsData
            for (int i = 0; i < GeometriesCount1; i++)
            {
                var geom = (Geometries != null) ? Geometries[i] : null;
                if (geom != null)
                {
                    off += ((16 - (off % 16)) % 16);
                    parts.Add(new Tuple<long, IResourceBlock>(off, geom));
                    off += geom.BlockLength; //Geometries
                }
            }

            return parts.ToArray();
        }

        public override string ToString()
        {
            return "(" + (Geometries?.Length ?? 0).ToString() + " geometr" + ((Geometries?.Length ?? 0) != 1 ? "ies)" : "y)");
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class DrawableGeometry : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength
        {
            get 
            {
                long l = 152;
                if (BoneIds != null)
                {
                    if (BoneIds.Length > 4) l += 8;
                    l += (BoneIds.Length) * 2;
                }
                return l;
            }
        }

        // structure data
        public uint VFT { get; set; } = 1080133528;
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong VertexBufferPointer { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong Unknown_30h; // 0x0000000000000000
        public ulong IndexBufferPointer { get; set; }
        public ulong Unknown_40h; // 0x0000000000000000
        public ulong Unknown_48h; // 0x0000000000000000
        public ulong Unknown_50h; // 0x0000000000000000
        public uint IndicesCount { get; set; }
        public uint TrianglesCount { get; set; }
        public ushort VerticesCount { get; set; }
        public ushort Unknown_62h = 3; // 0x0003 // indices per primitive (triangle)
        public uint Unknown_64h; // 0x00000000
        public ulong BoneIdsPointer { get; set; }
        public ushort VertexStride { get; set; }
        public ushort BoneIdsCount { get; set; }
        public uint Unknown_74h; // 0x00000000
        public ulong VertexDataPointer { get; set; }
        public ulong Unknown_80h; // 0x0000000000000000
        public ulong Unknown_88h; // 0x0000000000000000
        public ulong Unknown_90h; // 0x0000000000000000

        // reference data
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public VertexData VertexData { get; set; }
        public ushort[] BoneIds { get; set; }//embedded at the end of this struct
        public ShaderFX Shader { get; set; }//written by parent DrawableBase, using ShaderID
        public ushort ShaderID { get; set; }//read/written by parent model
        public AABB_s AABB { get; set; }//read/written by parent model


        public bool UpdateRenderableParameters = false; //used by model material editor...


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.VertexBufferPointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt64();
            this.IndexBufferPointer = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadUInt64();
            this.Unknown_48h = reader.ReadUInt64();
            this.Unknown_50h = reader.ReadUInt64();
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
            this.Unknown_80h = reader.ReadUInt64();
            this.Unknown_88h = reader.ReadUInt64();
            this.Unknown_90h = reader.ReadUInt64();

            // read reference data
            this.VertexBuffer = reader.ReadBlockAt<VertexBuffer>(
                this.VertexBufferPointer // offset
            );
            this.IndexBuffer = reader.ReadBlockAt<IndexBuffer>(
                this.IndexBufferPointer // offset
            );
            this.BoneIds = reader.ReadUshortsAt(this.BoneIdsPointer, this.BoneIdsCount, false);
            if (this.BoneIds != null) //skinned mesh bones to use? peds, also yft props...
            {
            }
            //if (BoneIdsPointer != 0)
            //{
            //    var pos = (ulong)reader.Position;
            //    if (BoneIdsCount > 4) pos += 8;
            //    if (BoneIdsPointer != pos)
            //    { }//no hit - interesting alignment, boneids array always packed after this struct
            //}

            if (this.VertexBuffer != null)
            {
                this.VertexData = this.VertexBuffer.Data1 ?? this.VertexBuffer.Data2;

                if (this.VerticesCount == 0)
                {
                    this.VerticesCount = (ushort)(this.VertexData?.VertexCount ?? 0);
                }

                //if (VertexBuffer.Data1 != VertexBuffer.Data2)
                //{ }//no hit
                //if (VertexDataPointer == 0)
                //{ }//no hit
                //else if (VertexDataPointer != VertexBuffer.DataPointer1)
                //{
                //    ////some mods hit here!
                //    //try
                //    //{
                //    //    this.VertexData = reader.ReadBlockAt<VertexData>(
                //    //        this.VertexDataPointer, // offset
                //    //        this.VertexStride,
                //    //        this.VerticesCount,
                //    //        this.VertexBuffer.Info
                //    //    );
                //    //}
                //    //catch
                //    //{ }
                //}
                //if (VertexStride != VertexBuffer.VertexStride)
                //{ }//no hit
                //if (VertexStride != (VertexBuffer.Info?.Stride ?? 0))
                //{ }//no hit
            }
            //else
            //{ }//no hit


            //if (Unknown_4h != 1)
            //{ }
            //if (Unknown_8h != 0)
            //{ }
            //if (Unknown_10h != 0)
            //{ }
            //if (Unknown_20h != 0)
            //{ }
            //if (Unknown_28h != 0)
            //{ }
            //if (Unknown_30h != 0)
            //{ }
            //if (Unknown_40h != 0)
            //{ }
            //if (Unknown_48h != 0)
            //{ }
            //if (Unknown_50h != 0)
            //{ }
            //if (Unknown_64h != 0)
            //{ }
            //if (Unknown_74h != 0)
            //{ }
            //if (Unknown_80h != 0)
            //{ }
            //if (Unknown_88h != 0)
            //{ }
            //if (Unknown_90h != 0)
            //{ }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.VertexBufferPointer = (ulong)(this.VertexBuffer != null ? this.VertexBuffer.FilePosition : 0);
            this.IndexBufferPointer = (ulong)(this.IndexBuffer != null ? this.IndexBuffer.FilePosition : 0);
            this.VertexDataPointer = (ulong)(this.VertexData != null ? this.VertexData.FilePosition : 0);
            this.VerticesCount = (ushort)(this.VertexData != null ? this.VertexData.VertexCount : 0); //TODO: fix?
            this.VertexStride = (ushort)(this.VertexBuffer != null ? this.VertexBuffer.VertexStride : 0); //TODO: fix?
            this.IndicesCount = (this.IndexBuffer != null ? this.IndexBuffer.IndicesCount : 0); //TODO: fix?
            this.TrianglesCount = this.IndicesCount / 3; //TODO: fix?
            this.BoneIdsPointer = (BoneIds != null) ? (ulong)(writer.Position + 152 + ((BoneIds.Length > 4) ? 8 : 0)) : 0;
            this.BoneIdsCount = (ushort)(BoneIds?.Length ?? 0);
            

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.VertexBufferPointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.IndexBufferPointer);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_50h);
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
            writer.Write(this.Unknown_88h);
            writer.Write(this.Unknown_90h);

            if (BoneIds != null)
            {
                if (BoneIds.Length > 4)
                {
                    writer.Write((ulong)0);
                }
                for (int i = 0; i < BoneIds.Length; i++)
                {
                    writer.Write(BoneIds[i]);
                }
            }

        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YdrXml.ValueTag(sb, indent, "ShaderIndex", ShaderID.ToString());
            YdrXml.SelfClosingTag(sb, indent, "BoundingBoxMin " + FloatUtil.GetVector4XmlString(AABB.Min));
            YdrXml.SelfClosingTag(sb, indent, "BoundingBoxMax " + FloatUtil.GetVector4XmlString(AABB.Max));
            if (BoneIds != null)
            {
                var ids = new StringBuilder();
                foreach (var id in BoneIds)
                {
                    if (ids.Length > 0) ids.Append(", ");
                    ids.Append(id.ToString());
                }
                YdrXml.StringTag(sb, indent, "BoneIDs", ids.ToString());
            }
            if (VertexBuffer != null)
            {
                YdrXml.OpenTag(sb, indent, "VertexBuffer");
                VertexBuffer.WriteXml(sb, indent + 1);
                YdrXml.CloseTag(sb, indent, "VertexBuffer");
            }
            if (IndexBuffer != null)
            {
                YdrXml.OpenTag(sb, indent, "IndexBuffer");
                IndexBuffer.WriteXml(sb, indent + 1);
                YdrXml.CloseTag(sb, indent, "IndexBuffer");
            }
        }
        public void ReadXml(XmlNode node)
        {
            ShaderID = (ushort)Xml.GetChildUIntAttribute(node, "ShaderIndex", "value");
            var aabb = new AABB_s();
            aabb.Min = Xml.GetChildVector4Attributes(node, "BoundingBoxMin");
            aabb.Max = Xml.GetChildVector4Attributes(node, "BoundingBoxMax");
            AABB = aabb;
            var bnode = node.SelectSingleNode("BoneIDs");
            if (bnode != null)
            {
                var astr = bnode.InnerText;
                var arr = astr.Split(',');
                var blist = new List<ushort>();
                foreach (var bstr in arr)
                {
                    var tstr = bstr?.Trim();
                    if (string.IsNullOrEmpty(tstr)) continue;
                    if (ushort.TryParse(tstr, out ushort u))
                    {
                        blist.Add(u);
                    }
                }
                BoneIds = (blist.Count > 0) ? blist.ToArray() : null;
            }
            var vnode = node.SelectSingleNode("VertexBuffer");
            if (vnode != null)
            {
                VertexBuffer = new VertexBuffer();
                VertexBuffer.ReadXml(vnode);
                VertexData = VertexBuffer.Data1 ?? VertexBuffer.Data2;
            }
            var inode = node.SelectSingleNode("IndexBuffer");
            if (inode != null)
            {
                IndexBuffer = new IndexBuffer();
                IndexBuffer.ReadXml(inode);
            }
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (VertexBuffer != null) list.Add(VertexBuffer);
            if (IndexBuffer != null) list.Add(IndexBuffer);
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
        public override long BlockLength => 128;
        public override long BlockLength_Gen9 => 64;

        // structure data
        public uint VFT { get; set; } = 1080153080;
        public uint Unknown_4h = 1; // 0x00000001
        public ushort VertexStride { get; set; }
        public ushort Flags { get; set; } //only 0 or 1024
        public uint Unknown_Ch; // 0x00000000
        public ulong DataPointer1 { get; set; }
        public uint VertexCount { get; set; }
        public uint Unknown_1Ch; // 0x00000000
        public ulong DataPointer2 { get; set; }
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong InfoPointer { get; set; }
        public ulong Unknown_38h; // 0x0000000000000000
        public ulong Unknown_40h; // 0x0000000000000000
        public ulong Unknown_48h; // 0x0000000000000000
        public ulong Unknown_50h; // 0x0000000000000000
        public ulong Unknown_58h; // 0x0000000000000000
        public ulong Unknown_60h; // 0x0000000000000000
        public ulong Unknown_68h; // 0x0000000000000000
        public ulong Unknown_70h; // 0x0000000000000000
        public ulong Unknown_78h; // 0x0000000000000000

        // gen9 structure data
        public ushort G9_Unknown_Eh;
        public uint G9_BindFlags { get; set; }   // m_bindFlags    0x00580409 or 0x00586409
        public uint G9_Unknown_14h;
        public ulong G9_Unknown_20h;
        public ulong G9_SRVPointer { get; set; }
        public ShaderResourceViewG9 G9_SRV { get; set; }
        public VertexDeclarationG9 G9_Info { get; set; }


        // reference data
        public VertexData Data1 { get; set; }
        public VertexData Data2 { get; set; }
        public VertexDeclaration Info { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();

            if (reader.IsGen9)
            {
                VertexCount = reader.ReadUInt32();
                VertexStride = reader.ReadUInt16();         // m_vertexSize
                G9_Unknown_Eh = reader.ReadUInt16();
                G9_BindFlags = reader.ReadUInt32();
                G9_Unknown_14h = reader.ReadUInt32();
                DataPointer1 = reader.ReadUInt64();    // m_vertexData
                G9_Unknown_20h = reader.ReadUInt64();             // m_pad
                Unknown_28h = reader.ReadUInt64();     // m_pad2
                G9_SRVPointer = reader.ReadUInt64();     // m_srv
                InfoPointer = reader.ReadUInt64();     // m_vertexFormat (rage::grcFvf)

                G9_SRV = reader.ReadBlockAt<ShaderResourceViewG9>(G9_SRVPointer);
                G9_Info = reader.ReadBlockAt<VertexDeclarationG9>(InfoPointer);

                var datalen = VertexCount * VertexStride;
                var vertexBytes = reader.ReadBytesAt(DataPointer1, datalen);
                InitVertexDataFromGen9Data(vertexBytes);

                if (G9_Unknown_Eh != 0)
                { }
                switch (G9_BindFlags)
                {
                    case 0x00580409:
                    case 0x00586409:
                        break;
                    default:
                        break;
                }
                if (G9_Unknown_14h != 0)
                { }
                if (G9_Unknown_20h != 0)
                { }

            }
            else
            {
                this.VertexStride = reader.ReadUInt16();
                this.Flags = reader.ReadUInt16();
                this.Unknown_Ch = reader.ReadUInt32();
                this.DataPointer1 = reader.ReadUInt64();
                this.VertexCount = reader.ReadUInt32();
                this.Unknown_1Ch = reader.ReadUInt32();
                this.DataPointer2 = reader.ReadUInt64();
                this.Unknown_28h = reader.ReadUInt64();
                this.InfoPointer = reader.ReadUInt64();
                this.Unknown_38h = reader.ReadUInt64();
                this.Unknown_40h = reader.ReadUInt64();
                this.Unknown_48h = reader.ReadUInt64();
                this.Unknown_50h = reader.ReadUInt64();
                this.Unknown_58h = reader.ReadUInt64();
                this.Unknown_60h = reader.ReadUInt64();
                this.Unknown_68h = reader.ReadUInt64();
                this.Unknown_70h = reader.ReadUInt64();
                this.Unknown_78h = reader.ReadUInt64();

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


                //switch (Flags)
                //{
                //    case 0:
                //        break;
                //    case 1024://micro flag? //micro_brow_down.ydr, micro_chin_pointed.ydr
                //        break;
                //    default:
                //        break;
                //}

                //if (Unknown_4h != 1)
                //{ }
                //if (Unknown_Ch != 0)
                //{ }
                //if (Unknown_1Ch != 0)
                //{ }
                //if (Unknown_28h != 0)
                //{ }
                //if (Unknown_38h != 0)
                //{ }
                //if (Unknown_40h != 0)
                //{ }
                //if (Unknown_48h != 0)
                //{ }
                //if (Unknown_50h != 0)
                //{ }
                //if (Unknown_58h != 0)
                //{ }
                //if (Unknown_60h != 0)
                //{ }
                //if (Unknown_68h != 0)
                //{ }
                //if (Unknown_70h != 0)
                //{ }
                //if (Unknown_78h != 0)
                //{ }

            }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.VertexCount = (uint)(this.Data1 != null ? this.Data1.VertexCount : this.Data2 != null ? this.Data2.VertexCount : 0);
            this.DataPointer1 = (ulong)(this.Data1 != null ? this.Data1.FilePosition : 0);
            this.DataPointer2 = (ulong)(this.Data2 != null ? this.Data2.FilePosition : 0);
            this.InfoPointer = (ulong)(this.Info != null ? this.Info.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);

            if (writer.IsGen9)
            {
                //TODO

            }
            else
            {
                writer.Write(this.VertexStride);
                writer.Write(this.Flags);
                writer.Write(this.Unknown_Ch);
                writer.Write(this.DataPointer1);
                writer.Write(this.VertexCount);
                writer.Write(this.Unknown_1Ch);
                writer.Write(this.DataPointer2);
                writer.Write(this.Unknown_28h);
                writer.Write(this.InfoPointer);
                writer.Write(this.Unknown_38h);
                writer.Write(this.Unknown_40h);
                writer.Write(this.Unknown_48h);
                writer.Write(this.Unknown_50h);
                writer.Write(this.Unknown_58h);
                writer.Write(this.Unknown_60h);
                writer.Write(this.Unknown_68h);
                writer.Write(this.Unknown_70h);
                writer.Write(this.Unknown_78h);
            }

        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YdrXml.ValueTag(sb, indent, "Flags", Flags.ToString());

            if (Info != null)
            {
                Info.WriteXml(sb, indent, "Layout");
            }
            if (Data1 != null)
            {
                YdrXml.OpenTag(sb, indent, "Data");
                Data1.WriteXml(sb, indent + 1);
                YdrXml.CloseTag(sb, indent, "Data");
            }
            if ((Data2 != null) && (Data2 != Data1))
            {
                YdrXml.OpenTag(sb, indent, "Data2");
                Data2.WriteXml(sb, indent + 1);
                YdrXml.CloseTag(sb, indent, "Data2");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Flags = (ushort)Xml.GetChildUIntAttribute(node, "Flags", "value");

            var inode = node.SelectSingleNode("Layout");
            if (inode != null)
            {
                Info = new VertexDeclaration();
                Info.ReadXml(inode);
                VertexStride = Info.Stride;
            }
            var dnode = node.SelectSingleNode("Data");
            if (dnode != null)
            {
                Data1 = new VertexData();
                Data1.ReadXml(dnode, Info);
                Data2 = Data1;
                VertexCount = (uint)Data1.VertexCount;
            }
            var dnode2 = node.SelectSingleNode("Data2");
            if (dnode2 != null)
            {
                Data2 = new VertexData();
                Data2.ReadXml(dnode2, Info);
            }
        }



        public void InitVertexDataFromGen9Data(byte[] gen9bytes)
        {
            if (gen9bytes == null) return;
            if (G9_Info == null) return;

            //create VertexDeclaration (Info) from G9_Info
            //and remap vertex data into Data1.VertexBytes (and Data2)

            var vdtypes = VertexDeclarationTypes.GTAV1;
            var vdflags = 0u;
            var g9types = G9_Info.Types;
            var g9sizes = G9_Info.Sizes;//these seem to just contain the vertex stride - not sizes but offsets to next item
            var g9offs = G9_Info.Offsets;
            var g9cnt = G9_Info.ElementCount;
            for (int i = 0; i < g9types.Length; i++)//52
            {
                var t = g9types[i];
                if (t == 0) continue;
                var lci = VertexDeclarationG9.GetLegacyComponentIndexGTAV1(i);
                if (lci < 0)
                {
                    //this component type won't work for GTAV1 type...
                    //TODO: try a different type! eg GTAV4
                    continue;
                }
                vdflags = BitUtil.SetBit(vdflags, lci);
            }
            var vtype = (VertexType)vdflags;
            switch (vtype)//just testing converted flags
            {
                case VertexType.Default:
                case VertexType.DefaultEx:
                case VertexType.PCTT:
                case VertexType.PNCCT:
                case VertexType.PNCCTTTX:
                case VertexType.PNCTTX:
                case VertexType.PNCCTT:
                case VertexType.PNCTTTX:
                case VertexType.PNCCTTX_2:
                case VertexType.PNCCTX:
                case VertexType.PNCCTTX:
                case VertexType.PBBNCTX:
                case VertexType.PBBNCT:
                case VertexType.PBBNCCTX:
                case VertexType.PBBCCT:
                case VertexType.PBBNCTTX:
                case VertexType.PNC:
                case VertexType.PCT:
                case VertexType.PNCTTTX_2:
                case VertexType.PNCTTTTX:
                case VertexType.PBBNCCT:
                case VertexType.PT:
                case VertexType.PNCCTTTT:
                case VertexType.PNCTTTX_3:
                case VertexType.PCC:
                case (VertexType)113://PCCT: decal_diff_only_um, ch_chint02_floor_mural_01.ydr
                case (VertexType)1://P: farlods.ydd
                case VertexType.PTT:
                case VertexType.PC:
                case VertexType.PBBNCCTTX:
                case VertexType.PBBNCCTT:
                case VertexType.PBBNCTT:
                case VertexType.PBBNCTTT:
                case VertexType.PNCTT:
                case VertexType.PNCTTT:
                case VertexType.PBBNCTTTX:
                    break;
                default:
                    break;
            }

            var vd = new VertexDeclaration();
            vd.Types = vdtypes;
            vd.Flags = vdflags;
            vd.UpdateCountAndStride();
            if (vd.Count != g9cnt)
            { }//just testing converted component count actually matches
            if (vd.Stride != VertexStride)
            { }//just testing converted stride actually matches


            //this really sucks that we have to rebuild the vertex data, but component ordering is different!
            //maybe some layouts still have the same ordering so this could be bypassed, but probably not many.
            var buf = new byte[gen9bytes.Length];
            for (int i = 0; i < g9types.Length; i++)//52
            {
                var t = g9types[i];
                if (t == 0) continue;
                var lci = VertexDeclarationG9.GetLegacyComponentIndexGTAV1(i);//TODO: handle other vdtypes
                if (lci < 0) continue;
                var cssize = (int)g9sizes[i];
                var csoff = (int)g9offs[i];
                var cdoff = vd.GetComponentOffset(lci);
                var cdtype = vd.GetComponentType(lci);
                var cdsize = VertexComponentTypes.GetSizeInBytes(cdtype);
                for (int v = 0; v < VertexCount; v++)
                {
                    var srcoff = csoff + (cssize * v);
                    var dstoff = cdoff + (VertexStride * v);
                    Buffer.BlockCopy(gen9bytes, srcoff, buf, dstoff, cdsize);
                }
            }

            var data = new VertexData();
            data.VertexStride = VertexStride;
            data.VertexCount = (int)VertexCount;
            data.Info = vd;
            data.VertexType = vtype;
            data.VertexBytes = buf;

            Data1 = data;
            Data2 = data;
            Info = vd;

        }
        public byte[] InitGen9DataFromVertexData()
        {
            //TODO: create G9_Info from Info
            //TODO: and remap vertex data from Data1.VertexBytes into the result



            return null;
        }


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


        public int VertexStride { get; set; }
        public int VertexCount { get; set; }
        public VertexDeclaration Info { get; set; }
        public VertexType VertexType { get; set; }

        public byte[] VertexBytes { get; set; }


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
            Info = (VertexDeclaration)parameters[2];
            VertexType = (VertexType)Info.Flags;

            VertexBytes = reader.ReadBytes(VertexCount * VertexStride);

            switch (Info.Types)
            {
                case VertexDeclarationTypes.GTAV1: //YDR - 0x7755555555996996
                    break;
                case VertexDeclarationTypes.GTAV2:  //YFT - 0x030000000199A006
                    switch (Info.Flags)
                    {
                        case 16473: VertexType = VertexType.PCCH2H4; break;  //  PCCH2H4 
                        default:break;
                    }
                    break;
                case VertexDeclarationTypes.GTAV3:  //YFT - 0x0300000001996006  PNCH2H4
                    switch (Info.Flags)
                    {
                        case 89: VertexType = VertexType.PNCH2; break;     //  PNCH2
                        default: break;
                    }
                    break;
                default:
                    break;
            }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            if (VertexBytes != null)
            {
                writer.Write(VertexBytes); //not dealing with individual vertex data here any more!
            }
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            var flags = Info?.Flags ?? 0;
            var row = new StringBuilder();
            for (int v = 0; v < VertexCount; v++)
            {
                row.Clear();
                for (int k = 0; k < 16; k++)
                {
                    if (((flags >> k) & 0x1) == 1)
                    {
                        if (row.Length > 0) row.Append("   ");
                        var str = GetString(v, k, " ");
                        row.Append(str);
                    }
                }
                YdrXml.Indent(sb, indent);
                sb.AppendLine(row.ToString());
            }
        }
        public void ReadXml(XmlNode node, VertexDeclaration info)
        {
            Info = info;
            VertexType = (VertexType)(info?.Flags ?? 0);

            if (Info != null)
            {
                var flags = Info.Flags;
                var stride = Info.Stride;
                var vstrs = new List<string[]>();
                var coldelim = new[] { ' ', '\t' };
                var rowdelim = new[] { '\n' };
                var rows = node?.InnerText?.Trim()?.Split(rowdelim, StringSplitOptions.RemoveEmptyEntries);
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        var rowt = row.Trim();
                        if (string.IsNullOrEmpty(rowt)) continue;
                        var cols = row.Split(coldelim, StringSplitOptions.RemoveEmptyEntries);
                        vstrs.Add(cols);
                    }
                }
                if (vstrs.Count > 0)
                {
                    AllocateData(vstrs.Count);
                    for (int v = 0; v < vstrs.Count; v++)
                    {
                        var vstr = vstrs[v];
                        var sind = 0;
                        for (int k = 0; k < 16; k++)
                        {
                            if (((flags >> k) & 0x1) == 1)
                            {
                                SetString(v, k, vstr, ref sind);
                            }
                        }
                    }
                }
            }
        }


        public void AllocateData(int vertexCount)
        {
            if (Info != null)
            {
                var stride = Info.Stride;
                var byteCount = vertexCount * stride;
                VertexBytes = new byte[byteCount];
                VertexCount = vertexCount;
            }
        }

        public void SetString(int v, int c, string[] strs, ref int sind)
        {
            if ((Info != null) && (VertexBytes != null) && (strs != null))
            {
                var ind = sind;
                float f(int i) => FloatUtil.Parse(strs[ind + i].Trim());
                byte b(int i) { if (byte.TryParse(strs[ind + i].Trim(), out byte x)) return x; else return 0; }
                var ct = Info.GetComponentType(c);
                var cc = VertexComponentTypes.GetComponentCount(ct);
                if (sind + cc > strs.Length)
                { return; }
                switch (ct)
                {
                    case VertexComponentType.Float: SetFloat(v, c, f(0)); break;
                    case VertexComponentType.Float2: SetVector2(v, c, new Vector2(f(0), f(1))); break;
                    case VertexComponentType.Float3: SetVector3(v, c, new Vector3(f(0), f(1), f(2))); break;
                    case VertexComponentType.Float4: SetVector4(v, c, new Vector4(f(0), f(1), f(2), f(3))); break;
                    case VertexComponentType.RGBA8SNorm: SetRGBA8SNorm(v, c, new Vector4(f(0), f(1), f(2), f(3))); break;
                    case VertexComponentType.Half2: SetHalf2(v, c, new Half2(f(0), f(1))); break;
                    case VertexComponentType.Half4: SetHalf4(v, c, new Half4(f(0), f(1), f(2), f(3))); break;
                    case VertexComponentType.Colour: SetColour(v, c, new Color(b(0), b(1), b(2), b(3))); break;
                    case VertexComponentType.UByte4: SetUByte4(v, c, new Color(b(0), b(1), b(2), b(3))); break;
                    default:
                        break;
                }
                sind += cc;
            }
        }
        public void SetFloat(int v, int c, float val)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;//sizeof(float)
                if (e <= VertexBytes.Length)
                {
                    var b = BitConverter.GetBytes(val);
                    Buffer.BlockCopy(b, 0, VertexBytes, o, 4);
                }
            }
        }
        public void SetVector2(int v, int c, Vector2 val)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 8;//sizeof(Vector2)
                if (e <= VertexBytes.Length)
                {
                    var x = BitConverter.GetBytes(val.X);
                    var y = BitConverter.GetBytes(val.Y);
                    Buffer.BlockCopy(x, 0, VertexBytes, o + 0, 4);
                    Buffer.BlockCopy(y, 0, VertexBytes, o + 4, 4);
                }
            }
        }
        public void SetVector3(int v, int c, Vector3 val)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 12;//sizeof(Vector3)
                if (e <= VertexBytes.Length)
                {
                    var x = BitConverter.GetBytes(val.X);
                    var y = BitConverter.GetBytes(val.Y);
                    var z = BitConverter.GetBytes(val.Z);
                    Buffer.BlockCopy(x, 0, VertexBytes, o + 0, 4);
                    Buffer.BlockCopy(y, 0, VertexBytes, o + 4, 4);
                    Buffer.BlockCopy(z, 0, VertexBytes, o + 8, 4);
                }
            }
        }
        public void SetVector4(int v, int c, Vector4 val)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 16;//sizeof(Vector4)
                if (e <= VertexBytes.Length)
                {
                    var x = BitConverter.GetBytes(val.X);
                    var y = BitConverter.GetBytes(val.Y);
                    var z = BitConverter.GetBytes(val.Z);
                    var w = BitConverter.GetBytes(val.W);
                    Buffer.BlockCopy(x, 0, VertexBytes, o + 0, 4);
                    Buffer.BlockCopy(y, 0, VertexBytes, o + 4, 4);
                    Buffer.BlockCopy(z, 0, VertexBytes, o + 8, 4);
                    Buffer.BlockCopy(w, 0, VertexBytes, o + 12, 4);
                }
            }
        }
        public void SetRGBA8SNorm(int v, int c, Vector4 val)
        {
            // Equivalent to DXGI_FORMAT_R8G8B8A8_SNORM
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;//sizeof(RGBA8SNorm)
                if (e <= VertexBytes.Length)
                {
                    var x = (byte)Math.Max(-127.0f, Math.Min(val.X * 127.0f, 127.0f));
                    var y = (byte)Math.Max(-127.0f, Math.Min(val.Y * 127.0f, 127.0f));
                    var z = (byte)Math.Max(-127.0f, Math.Min(val.Z * 127.0f, 127.0f));
                    var w = (byte)Math.Max(-127.0f, Math.Min(val.W * 127.0f, 127.0f));
                    var u = x | (y << 8) | (z << 16) | (w << 24);
                    var b = BitConverter.GetBytes(u);
                    Buffer.BlockCopy(b, 0, VertexBytes, o, 4);
                }
            }
        }
        public void SetHalf2(int v, int c, Half2 val)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;//sizeof(Half2)
                if (e <= VertexBytes.Length)
                {
                    var x = BitConverter.GetBytes(val.X.RawValue);
                    var y = BitConverter.GetBytes(val.Y.RawValue);
                    Buffer.BlockCopy(x, 0, VertexBytes, o + 0, 2);
                    Buffer.BlockCopy(y, 0, VertexBytes, o + 2, 2);
                }
            }
        }
        public void SetHalf4(int v, int c, Half4 val)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 8;//sizeof(Half4)
                if (e <= VertexBytes.Length)
                {
                    var x = BitConverter.GetBytes(val.X.RawValue);
                    var y = BitConverter.GetBytes(val.Y.RawValue);
                    var z = BitConverter.GetBytes(val.Z.RawValue);
                    var w = BitConverter.GetBytes(val.W.RawValue);
                    Buffer.BlockCopy(x, 0, VertexBytes, o + 0, 2);
                    Buffer.BlockCopy(y, 0, VertexBytes, o + 2, 2);
                    Buffer.BlockCopy(z, 0, VertexBytes, o + 4, 2);
                    Buffer.BlockCopy(w, 0, VertexBytes, o + 6, 2);
                }
            }
        }
        public void SetColour(int v, int c, Color val)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;//sizeof(Color)
                if (e <= VertexBytes.Length)
                {
                    var u = val.ToRgba();
                    var b = BitConverter.GetBytes(u);
                    Buffer.BlockCopy(b, 0, VertexBytes, o, 4);
                }
            }
        }
        public void SetUByte4(int v, int c, Color val)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;//sizeof(UByte4)
                if (e <= VertexBytes.Length)
                {
                    var u = val.ToRgba();
                    var b = BitConverter.GetBytes(u);
                    Buffer.BlockCopy(b, 0, VertexBytes, o, 4);
                }
            }
        }

        public string GetString(int v, int c, string d = ", ")
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var ct = Info.GetComponentType(c);
                switch (ct)
                {
                    case VertexComponentType.Float: return FloatUtil.ToString(GetFloat(v, c));
                    case VertexComponentType.Float2: return FloatUtil.GetVector2String(GetVector2(v, c), d);
                    case VertexComponentType.Float3: return FloatUtil.GetVector3String(GetVector3(v, c), d);
                    case VertexComponentType.Float4: return FloatUtil.GetVector4String(GetVector4(v, c), d);
                    case VertexComponentType.RGBA8SNorm: return FloatUtil.GetVector4String(GetRGBA8SNorm(v, c), d);
                    case VertexComponentType.Half2: return FloatUtil.GetHalf2String(GetHalf2(v, c), d);
                    case VertexComponentType.Half4: return FloatUtil.GetHalf4String(GetHalf4(v, c), d);
                    case VertexComponentType.Colour: return FloatUtil.GetColourString(GetColour(v, c), d);
                    case VertexComponentType.UByte4: return FloatUtil.GetColourString(GetUByte4(v, c), d);
                    default:
                        break;
                }
            }
            return string.Empty;
        }
        public float GetFloat(int v, int c)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;//sizeof(float)
                if (e <= VertexBytes.Length)
                {
                    var f = BitConverter.ToSingle(VertexBytes, o);
                    return f;
                }
            }
            return 0;
        }
        public Vector2 GetVector2(int v, int c)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 8;//sizeof(Vector2)
                if (e <= VertexBytes.Length)
                {
                    var x = BitConverter.ToSingle(VertexBytes, o + 0);
                    var y = BitConverter.ToSingle(VertexBytes, o + 4);
                    return new Vector2(x, y);
                }
            }
            return Vector2.Zero;
        }
        public Vector3 GetVector3(int v, int c)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 12;//sizeof(Vector3)
                if (e <= VertexBytes.Length)
                {
                    var x = BitConverter.ToSingle(VertexBytes, o + 0);
                    var y = BitConverter.ToSingle(VertexBytes, o + 4);
                    var z = BitConverter.ToSingle(VertexBytes, o + 8);
                    return new Vector3(x, y, z);
                }
            }
            return Vector3.Zero;
        }
        public Vector4 GetVector4(int v, int c)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 16;//sizeof(Vector4)
                if (e <= VertexBytes.Length)
                {
                    var x = BitConverter.ToSingle(VertexBytes, o + 0);
                    var y = BitConverter.ToSingle(VertexBytes, o + 4);
                    var z = BitConverter.ToSingle(VertexBytes, o + 8);
                    var w = BitConverter.ToSingle(VertexBytes, o + 12);
                    return new Vector4(x, y, z, w);
                }
            }
            return Vector4.Zero;
        }
        public Vector4 GetRGBA8SNorm(int v, int c)
        {
            // Equivalent to DXGI_FORMAT_R8G8B8A8_SNORM
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;//sizeof(RGBA8SNorm)
                if (e <= VertexBytes.Length)
                {
                    var xyzw = BitConverter.ToUInt32(VertexBytes, o);
                    var x = (sbyte)(xyzw & 0xFF) / 127.0f;
                    var y = (sbyte)((xyzw >> 8) & 0xFF) / 127.0f;
                    var z = (sbyte)((xyzw >> 16) & 0xFF) / 127.0f;
                    var w = (sbyte)((xyzw >> 24) & 0xFF) / 127.0f;
                    return new Vector4(x, y, z, w);
                }
            }
            return Vector4.Zero;
        }
        public Half2 GetHalf2(int v, int c)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;//sizeof(Half2)
                if (e <= VertexBytes.Length)
                {
                    var x = BitConverter.ToUInt16(VertexBytes, o + 0);
                    var y = BitConverter.ToUInt16(VertexBytes, o + 2);
                    return new Half2(x, y);
                }
            }
            return new Half2(0, 0);
        }
        public Half4 GetHalf4(int v, int c)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 8;//sizeof(Half4)
                if (e <= VertexBytes.Length)
                {
                    var x = BitConverter.ToUInt16(VertexBytes, o + 0);
                    var y = BitConverter.ToUInt16(VertexBytes, o + 2);
                    var z = BitConverter.ToUInt16(VertexBytes, o + 4);
                    var w = BitConverter.ToUInt16(VertexBytes, o + 6);
                    return new Half4(x, y, z, w);
                }
            }
            return new Half4(0, 0, 0, 0);
        }
        public Color GetColour(int v, int c)
        {
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;//sizeof(Color)
                if (e <= VertexBytes.Length)
                {
                    var rgba = BitConverter.ToUInt32(VertexBytes, o);
                    return new Color(rgba);
                }
            }
            return Color.Black;
        }
        public Color GetUByte4(int v, int c)
        {
            //Color is the same as UByte4 really
            if ((Info != null) && (VertexBytes != null))
            {
                var s = Info.Stride;
                var co = Info.GetComponentOffset(c);
                var o = (v * s) + co;
                var e = o + 4;//sizeof(UByte4)
                if (e <= VertexBytes.Length)
                {
                    var rgba = BitConverter.ToUInt32(VertexBytes, o);
                    return new Color(rgba);
                }
            }
            return new Color(0, 0, 0, 0);
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
        public byte Unknown_6h { get; set; }//0
        public byte Count { get; set; }
        public VertexDeclarationTypes Types { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Flags = reader.ReadUInt32();
            this.Stride = reader.ReadUInt16();
            this.Unknown_6h = reader.ReadByte();
            this.Count = reader.ReadByte();
            this.Types = (VertexDeclarationTypes)reader.ReadUInt64();

            ////just testing!
            //UpdateCountAndStride();
            //if (Unknown_6h != 0)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Flags);
            writer.Write(this.Stride);
            writer.Write(this.Unknown_6h);
            writer.Write(this.Count);
            writer.Write((ulong)this.Types);
        }
        public void WriteXml(StringBuilder sb, int indent, string name)
        {
            YdrXml.OpenTag(sb, indent, name + " type=\"" + Types.ToString() + "\"");

            for (int k = 0; k < 16; k++)
            {
                if (((Flags >> k) & 0x1) == 1)
                {
                    var componentSemantic = (VertexSemantics)k;
                    var tag = componentSemantic.ToString();
                    YdrXml.SelfClosingTag(sb, indent + 1, tag);
                }
            }

            YdrXml.CloseTag(sb, indent, name);
        }
        public void ReadXml(XmlNode node)
        {
            if (node == null) return;

            Types = Xml.GetEnumValue<VertexDeclarationTypes>(Xml.GetStringAttribute(node, "type"));

            uint f = 0;
            foreach (XmlNode cnode in node.ChildNodes)
            {
                if (cnode is XmlElement celem)
                {
                    var componentSematic = Xml.GetEnumValue<VertexSemantics>(celem.Name);
                    var idx = (int)componentSematic;
                    f = f | (1u << idx);
                }
            }
            Flags = f;

            UpdateCountAndStride();
        }

        public ulong GetDeclarationId()
        {
            ulong res = 0;
            for(int i=0; i < 16; i++)
            {
                if (((Flags >> i) & 1) == 1)
                {
                    res += ((ulong)Types & (0xFu << (i * 4)));
                }
            }
            return res;
        }

        public VertexComponentType GetComponentType(int index)
        {
            //index is the flags bit index
            return (VertexComponentType)(((ulong)Types >> (index * 4)) & 0x0000000F);
        }

        public int GetComponentOffset(int index)
        {
            //index is the flags bit index
            var offset = 0;
            for (int k = 0; k < index; k++)
            {
                if (((Flags >> k) & 0x1) == 1)
                {
                    var componentType = GetComponentType(k);
                    offset += VertexComponentTypes.GetSizeInBytes(componentType);
                }
            }
            return offset;
        }

        public void UpdateCountAndStride()
        {
            var cnt = 0;
            var str = 0;
            for (int k = 0; k < 16; k++)
            {
                if (((Flags >> k) & 0x1) == 1)
                {
                    var componentType = GetComponentType(k);
                    str += VertexComponentTypes.GetSizeInBytes(componentType);
                    cnt++;
                }
            }

            ////just testing
            //if (Count != cnt)
            //{ }//no hit
            //if (Stride != str)
            //{ }//no hit

            Count = (byte)cnt;
            Stride = (ushort)str;
        }

        public override string ToString()
        {
            return Stride.ToString() + ": " + Count.ToString() + ": " + Flags.ToString() + ": " + Types.ToString(); 
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class VertexDeclarationG9 : ResourceSystemBlock
    {
        public override long BlockLength => 320;//316;
        public uint[] Offsets { get; set; }//[52]
        public byte[] Sizes { get; set; }//[52]
        public byte[] Types { get; set; }//[52] //(VertexDeclarationG9ElementFormat)
        public ulong Data { get; set; }

        public bool HasSOA //seems to always be false for GTAV gen9  (but true for RDR2)
        {
            get => (Data & 1) > 0;
        }
        public bool Flag //seems to always be false
        {
            get => ((Data >> 1) & 1) > 0;
        }
        public byte VertexSize
        {
            get => (byte)((Data >> 2) & 0xFF);
            set => Data = (Data & 0xFFFFFC03) + ((value & 0xFFu) << 2);
        }
        public uint VertexCount
        {
            get => (uint)((Data >> 10) & 0x3FFFFF);
            set => Data = (Data & 0x3FF) + ((value & 0x3FFFFF) << 10);
        }
        public uint ElementCount
        {
            get
            {
                if (Types == null) return 0;
                var n = 0u;
                foreach (var t in Types)
                {
                    if (t != 0) n++;
                }
                return n;
            }
        }

        public VertexDeclarationG9ElementFormat[] G9Formats
        {
            get
            {
                if (Types == null) return null;
                var n = ElementCount;
                var a = new VertexDeclarationG9ElementFormat[n];
                var c = 0;
                foreach (var t in Types)
                {
                    if (t == 0) continue;
                    a[c] = (VertexDeclarationG9ElementFormat)t;
                    c++;
                }
                return a;
            }
        }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            Offsets = reader.ReadStructs<uint>(52);
            Sizes = reader.ReadBytes(52);
            Types = reader.ReadBytes(52);
            Data = reader.ReadUInt64();


            //if (Types != null)
            //{
            //    foreach (var t in Types)
            //    {
            //        if (t == 0) continue;
            //        var f = (VertexDeclarationG9ElementFormat)t;
            //        switch (f)
            //        {
            //            case VertexDeclarationG9ElementFormat.R32G32B32_FLOAT:
            //            case VertexDeclarationG9ElementFormat.R32G32B32A32_FLOAT:
            //            case VertexDeclarationG9ElementFormat.R8G8B8A8_UNORM:
            //            case VertexDeclarationG9ElementFormat.R32G32_TYPELESS:
            //            case VertexDeclarationG9ElementFormat.R8G8B8A8_UINT:
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}
            //if (HasSOA == true)
            //{ }
            //if (Flag == true)
            //{ }
            //if ((Data >> 32) != 0)
            //{ }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            writer.WriteStructs<uint>(Offsets);
            writer.Write(Sizes);
            writer.Write(Types);
            writer.Write(Data);

        }


        public static VertexComponentType GetLegacyComponentType(VertexDeclarationG9ElementFormat f)
        {
            switch (f)
            {
                case VertexDeclarationG9ElementFormat.R32G32B32_FLOAT: return VertexComponentType.Float3;
                case VertexDeclarationG9ElementFormat.R32G32B32A32_FLOAT: return VertexComponentType.Float4;
                case VertexDeclarationG9ElementFormat.R8G8B8A8_UNORM: return VertexComponentType.Colour;
                case VertexDeclarationG9ElementFormat.R32G32_TYPELESS: return VertexComponentType.Float2;
                case VertexDeclarationG9ElementFormat.R8G8B8A8_UINT: return VertexComponentType.Colour;//for bone inds
                default: return VertexComponentType.Float4;
            }
        }
        public static int GetLegacyComponentIndexGTAV1(int i)
        {
            //GTAV1 = 0x7755555555996996, // GTAV - used by most drawables
            switch (i)
            {
                case 0: return 0;//POSITION0
                case 4: return 3;//NORMAL0
                case 8: return 14;//TANGENT0
                case 16: return 1;//BLENDWEIGHTS0
                case 20: return 2;//BLENDINDICES0
                case 24: return 4;//COLOR0
                case 25: return 5;//COLOR1
                case 28: return 6;//TEXCOORD0
                case 29: return 7;//TEXCOORD1
                case 30: return 8;//TEXCOORD2
                case 31: return 9;//TEXCOORD3
                case 32: return 10;//TEXCOORD4
                case 33: return 11;//TEXCOORD5
                default: return -1;
            }
            /*
            private static string[] RageSemanticNames =
            {
                00"POSITION",
                01"POSITION1",
                02"POSITION2",
                03"POSITION3",
                04"NORMAL",
                05"NORMAL1",
                06"NORMAL2",
                07"NORMAL3",
                08"TANGENT",
                09"TANGENT1",
                10"TANGENT2",
                11"TANGENT3",
                12"BINORMAL",
                13"BINORMAL1",
                14"BINORMAL2",
                15"BINORMAL3",
                16"BLENDWEIGHT",
                17"BLENDWEIGHT1",
                18"BLENDWEIGHT2",
                19"BLENDWEIGHT3",
                20"BLENDINDICIES",
                21"BLENDINDICIES1",
                22"BLENDINDICIES2",
                23"BLENDINDICIES3",
                24"COLOR0",
                25"COLOR1",
                26"COLOR2",
                27"COLOR3",
                28"TEXCOORD0",
                29"TEXCOORD1",
                30"TEXCOORD2",
                31"TEXCOORD3",
                32"TEXCOORD4",
                33"TEXCOORD5",
                34"TEXCOORD6",
                35"TEXCOORD7",
                36"TEXCOORD8",
                37"TEXCOORD9",
                38"TEXCOORD10",
                39"TEXCOORD11",
                40"TEXCOORD12",
                41"TEXCOORD13",
                42"TEXCOORD14",
                43"TEXCOORD15",
                44"TEXCOORD16",
                45"TEXCOORD17",
                46"TEXCOORD18",
                47"TEXCOORD19",
                48"TEXCOORD20",
                49"TEXCOORD21",
                50"TEXCOORD22",
                51"TEXCOORD23",
            };
             */
        }


    }
    public enum VertexDeclarationG9ElementFormat : byte
    {
        NONE = 0,
        R32G32B32A32_FLOAT = 2,
        R32G32B32_FLOAT = 6,
        R16G16B16A16_FLOAT = 10,
        R32G32_TYPELESS = 16,
        D3DX_R10G10B10A2 = 24,
        R8G8B8A8_UNORM = 28,
        R8G8B8A8_UINT = 30,
        R16G16_FLOAT = 34,
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class IndexBuffer : ResourceSystemBlock
    {
        public override long BlockLength => 96;
        public override long BlockLength_Gen9 => 64;

        // structure data
        public uint VFT { get; set; } = 1080152408;
        public uint Unknown_4h = 1; // 0x00000001
        public uint IndicesCount { get; set; }
        public uint Unknown_Ch; // 0x00000000
        public ulong IndicesPointer { get; set; }
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong Unknown_30h; // 0x0000000000000000
        public ulong Unknown_38h; // 0x0000000000000000
        public ulong Unknown_40h; // 0x0000000000000000
        public ulong Unknown_48h; // 0x0000000000000000
        public ulong Unknown_50h; // 0x0000000000000000
        public ulong Unknown_58h; // 0x0000000000000000

        // gen9 structure data
        public ushort G9_IndexSize { get; set; } = 2; // m_indexSize  //TODO: do we need to support 32bit indices?
        public ushort G9_Unknown_Eh;
        public uint G9_BindFlags { get; set; }   // m_bindFlags
        public uint G9_Unknown_14h;
        public ulong G9_SRVPointer { get; set; }
        public ShaderResourceViewG9 G9_SRV { get; set; }



        // reference data
        //public ResourceSimpleArray<ushort_r> Indices;
        public ushort[] Indices { get; set; }


        private ResourceSystemStructBlock<ushort> IndicesBlock = null; //only used when saving


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.IndicesCount = reader.ReadUInt32();

            if (reader.IsGen9)
            {
                G9_IndexSize = reader.ReadUInt16();
                G9_Unknown_Eh = reader.ReadUInt16();
                G9_BindFlags = reader.ReadUInt32();
                G9_Unknown_14h = reader.ReadUInt32();
                IndicesPointer = reader.ReadUInt64();
                Unknown_20h = reader.ReadUInt64();
                Unknown_28h = reader.ReadUInt64();
                G9_SRVPointer = reader.ReadUInt64();
                Unknown_38h = reader.ReadUInt64();

                Indices = reader.ReadUshortsAt(IndicesPointer, IndicesCount);
                G9_SRV = reader.ReadBlockAt<ShaderResourceViewG9>(G9_SRVPointer);

                if (G9_IndexSize != 2)
                { }
                if (G9_Unknown_Eh != 0)
                { }
                switch (G9_BindFlags)
                {
                    case 0x0058020a:
                        break;
                    default:
                        break;
                }
                if (G9_Unknown_14h != 0)
                { }
                if (Unknown_20h != 0)
                { }
                if (Unknown_28h != 0)
                { }
                if (Unknown_38h != 0)
                { }

            }
            else
            {

                this.Unknown_Ch = reader.ReadUInt32();
                this.IndicesPointer = reader.ReadUInt64();
                this.Unknown_18h = reader.ReadUInt64();
                this.Unknown_20h = reader.ReadUInt64();
                this.Unknown_28h = reader.ReadUInt64();
                this.Unknown_30h = reader.ReadUInt64();
                this.Unknown_38h = reader.ReadUInt64();
                this.Unknown_40h = reader.ReadUInt64();
                this.Unknown_48h = reader.ReadUInt64();
                this.Unknown_50h = reader.ReadUInt64();
                this.Unknown_58h = reader.ReadUInt64();

                // read reference data
                //this.Indices = reader.ReadBlockAt<ResourceSimpleArray<ushort_r>>(
                //    this.IndicesPointer, // offset
                //    this.IndicesCount
                //);
                this.Indices = reader.ReadUshortsAt(this.IndicesPointer, this.IndicesCount);


                //if (Unknown_4h != 1)
                //{ }
                //if (Unknown_Ch != 0)
                //{ }
                //if (Unknown_18h != 0)
                //{ }
                //if (Unknown_20h != 0)
                //{ }
                //if (Unknown_28h != 0)
                //{ }
                //if (Unknown_30h != 0)
                //{ }
                //if (Unknown_38h != 0)
                //{ }
                //if (Unknown_40h != 0)
                //{ }
                //if (Unknown_48h != 0)
                //{ }
                //if (Unknown_50h != 0)
                //{ }
                //if (Unknown_58h != 0)
                //{ }

            }

        }
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
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_58h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            if (Indices != null)
            {
                YdrXml.WriteRawArray(sb, Indices, indent, "Data", "", null, 24);
            }
        }
        public void ReadXml(XmlNode node)
        {
            var inode = node.SelectSingleNode("Data");
            if (inode != null)
            {
                Indices = Xml.GetRawUshortArray(node);
                IndicesCount = (uint)(Indices?.Length ?? 0);
            }
        }


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


    public enum LightType : byte
    {
        Point = 1,
        Spot = 2,
        Capsule = 4,
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class LightAttributes : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength
        {
            get { return 168; }
        }

        // structure data
        public uint Unknown_0h { get; set; } // 0x00000000
        public uint Unknown_4h { get; set; } // 0x00000000
        public Vector3 Position { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        public byte Flashiness { get; set; }
        public float Intensity { get; set; }
        public uint Flags { get; set; }
        public ushort BoneId { get; set; }
        public LightType Type { get; set; }
        public byte GroupId { get; set; }
        public uint TimeFlags { get; set; }
        public float Falloff { get; set; }
        public float FalloffExponent { get; set; }
        public Vector3 CullingPlaneNormal { get; set; }
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
        public Vector3 Direction { get; set; }
        public Vector3 Tangent { get; set; }
        public float ConeInnerAngle { get; set; }
        public float ConeOuterAngle { get; set; }
        public Vector3 Extent { get; set; }
        public MetaHash ProjectedTextureHash { get; set; }
        public uint Unknown_A4h { get; set; } // 0x00000000

        public bool UpdateRenderable = false; //used by model light form


        public Quaternion Orientation
        {
            get
            {
                Vector3 tx = new Vector3();
                Vector3 ty = new Vector3();

                switch (Type)
                {
                    case LightType.Point:
                        return Quaternion.Identity;
                    case LightType.Spot:
                    case LightType.Capsule:
                        tx = Vector3.Normalize(Tangent);
                        ty = Vector3.Normalize(Vector3.Cross(Direction, Tangent));
                        break;
                }

                var m = new Matrix();
                m.Row1 = new Vector4(tx, 0);
                m.Row2 = new Vector4(ty, 0);
                m.Row3 = new Vector4(Direction, 0);
                return Quaternion.RotationMatrix(m);
            }
            set
            {
                var inv = Quaternion.Invert(Orientation);
                var delta = value * inv;
                Direction = Vector3.Normalize(delta.Multiply(Direction));
                Tangent = Vector3.Normalize(delta.Multiply(Tangent));
            }
        }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            //read structure data
            Unknown_0h = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Position = reader.ReadVector3();
            Unknown_14h = reader.ReadUInt32();
            ColorR = reader.ReadByte();
            ColorG = reader.ReadByte();
            ColorB = reader.ReadByte();
            Flashiness = reader.ReadByte();
            Intensity = reader.ReadSingle();
            Flags = reader.ReadUInt32();
            BoneId = reader.ReadUInt16();
            Type = (LightType)reader.ReadByte();
            GroupId = reader.ReadByte();
            TimeFlags = reader.ReadUInt32();
            Falloff = reader.ReadSingle();
            FalloffExponent = reader.ReadSingle();
            CullingPlaneNormal = reader.ReadVector3();
            CullingPlaneOffset = reader.ReadSingle();
            ShadowBlur = reader.ReadByte();
            Unknown_45h = reader.ReadByte();
            Unknown_46h = reader.ReadUInt16();
            Unknown_48h = reader.ReadUInt32();
            VolumeIntensity = reader.ReadSingle();
            VolumeSizeScale = reader.ReadSingle();
            VolumeOuterColorR = reader.ReadByte();
            VolumeOuterColorG = reader.ReadByte();
            VolumeOuterColorB = reader.ReadByte();
            LightHash = reader.ReadByte();
            VolumeOuterIntensity = reader.ReadSingle();
            CoronaSize = reader.ReadSingle();
            VolumeOuterExponent = reader.ReadSingle();
            LightFadeDistance = reader.ReadByte();
            ShadowFadeDistance = reader.ReadByte();
            SpecularFadeDistance = reader.ReadByte();
            VolumetricFadeDistance = reader.ReadByte();
            ShadowNearClip = reader.ReadSingle();
            CoronaIntensity = reader.ReadSingle();
            CoronaZBias = reader.ReadSingle();
            Direction = reader.ReadVector3();
            Tangent = reader.ReadVector3();
            ConeInnerAngle = reader.ReadSingle();
            ConeOuterAngle = reader.ReadSingle();
            Extent = reader.ReadVector3();
            ProjectedTextureHash = new MetaHash(reader.ReadUInt32());
            Unknown_A4h = reader.ReadUInt32();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            //write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Position);
            writer.Write(this.Unknown_14h);
            writer.Write(this.ColorR);
            writer.Write(this.ColorG);
            writer.Write(this.ColorB);
            writer.Write(this.Flashiness);
            writer.Write(this.Intensity);
            writer.Write(this.Flags);
            writer.Write(this.BoneId);
            writer.Write((byte)this.Type);
            writer.Write(this.GroupId);
            writer.Write(this.TimeFlags);
            writer.Write(this.Falloff);
            writer.Write(this.FalloffExponent);
            writer.Write(this.CullingPlaneNormal);
            writer.Write(this.CullingPlaneOffset);
            writer.Write(this.ShadowBlur);
            writer.Write(this.Unknown_45h);
            writer.Write(this.Unknown_46h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.VolumeIntensity);
            writer.Write(this.VolumeSizeScale);
            writer.Write(this.VolumeOuterColorR);
            writer.Write(this.VolumeOuterColorG);
            writer.Write(this.VolumeOuterColorB);
            writer.Write(this.LightHash);
            writer.Write(this.VolumeOuterIntensity);
            writer.Write(this.CoronaSize);
            writer.Write(this.VolumeOuterExponent);
            writer.Write(this.LightFadeDistance);
            writer.Write(this.ShadowFadeDistance);
            writer.Write(this.SpecularFadeDistance);
            writer.Write(this.VolumetricFadeDistance);
            writer.Write(this.ShadowNearClip);
            writer.Write(this.CoronaIntensity);
            writer.Write(this.CoronaZBias);
            writer.Write(this.Direction);
            writer.Write(this.Tangent);
            writer.Write(this.ConeInnerAngle);
            writer.Write(this.ConeOuterAngle);
            writer.Write(this.Extent);
            writer.Write(this.ProjectedTextureHash.Hash);
            writer.Write(this.Unknown_A4h);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            YdrXml.SelfClosingTag(sb, indent, "Position " + FloatUtil.GetVector3XmlString(Position));
            YdrXml.SelfClosingTag(sb, indent, string.Format("Colour r=\"{0}\" g=\"{1}\" b=\"{2}\"", ColorR, ColorG, ColorB));
            YdrXml.ValueTag(sb, indent, "Flashiness", Flashiness.ToString());
            YdrXml.ValueTag(sb, indent, "Intensity", FloatUtil.ToString(Intensity));
            YdrXml.ValueTag(sb, indent, "Flags", Flags.ToString());
            YdrXml.ValueTag(sb, indent, "BoneId", BoneId.ToString());
            YdrXml.StringTag(sb, indent, "Type", Type.ToString());
            YdrXml.ValueTag(sb, indent, "GroupId", GroupId.ToString());
            YdrXml.ValueTag(sb, indent, "TimeFlags", TimeFlags.ToString());
            YdrXml.ValueTag(sb, indent, "Falloff", FloatUtil.ToString(Falloff));
            YdrXml.ValueTag(sb, indent, "FalloffExponent", FloatUtil.ToString(FalloffExponent));
            YdrXml.SelfClosingTag(sb, indent, "CullingPlaneNormal " + FloatUtil.GetVector3XmlString(CullingPlaneNormal));
            YdrXml.ValueTag(sb, indent, "CullingPlaneOffset", FloatUtil.ToString(CullingPlaneOffset));
            YdrXml.ValueTag(sb, indent, "Unknown45", Unknown_45h.ToString());
            YdrXml.ValueTag(sb, indent, "Unknown46", Unknown_46h.ToString());
            YdrXml.ValueTag(sb, indent, "VolumeIntensity", FloatUtil.ToString(VolumeIntensity));
            YdrXml.ValueTag(sb, indent, "VolumeSizeScale", FloatUtil.ToString(VolumeSizeScale));
            YdrXml.SelfClosingTag(sb, indent, string.Format("VolumeOuterColour r=\"{0}\" g=\"{1}\" b=\"{2}\"", VolumeOuterColorR, VolumeOuterColorG, VolumeOuterColorB));
            YdrXml.ValueTag(sb, indent, "LightHash", LightHash.ToString());
            YdrXml.ValueTag(sb, indent, "VolumeOuterIntensity", FloatUtil.ToString(VolumeOuterIntensity));
            YdrXml.ValueTag(sb, indent, "CoronaSize", FloatUtil.ToString(CoronaSize));
            YdrXml.ValueTag(sb, indent, "VolumeOuterExponent", FloatUtil.ToString(VolumeOuterExponent));
            YdrXml.ValueTag(sb, indent, "LightFadeDistance", LightFadeDistance.ToString());
            YdrXml.ValueTag(sb, indent, "ShadowBlur", ShadowBlur.ToString());
            YdrXml.ValueTag(sb, indent, "ShadowFadeDistance", ShadowFadeDistance.ToString());
            YdrXml.ValueTag(sb, indent, "SpecularFadeDistance", SpecularFadeDistance.ToString());
            YdrXml.ValueTag(sb, indent, "VolumetricFadeDistance", VolumetricFadeDistance.ToString());
            YdrXml.ValueTag(sb, indent, "ShadowNearClip", FloatUtil.ToString(ShadowNearClip));
            YdrXml.ValueTag(sb, indent, "CoronaIntensity", FloatUtil.ToString(CoronaIntensity));
            YdrXml.ValueTag(sb, indent, "CoronaZBias", FloatUtil.ToString(CoronaZBias));
            YdrXml.SelfClosingTag(sb, indent, "Direction " + FloatUtil.GetVector3XmlString(Direction));
            YdrXml.SelfClosingTag(sb, indent, "Tangent " + FloatUtil.GetVector3XmlString(Tangent));
            YdrXml.ValueTag(sb, indent, "ConeInnerAngle", FloatUtil.ToString(ConeInnerAngle));
            YdrXml.ValueTag(sb, indent, "ConeOuterAngle", FloatUtil.ToString(ConeOuterAngle));
            YdrXml.SelfClosingTag(sb, indent, "Extent " + FloatUtil.GetVector3XmlString(Extent));
            YdrXml.StringTag(sb, indent, "ProjectedTextureHash", YdrXml.HashString(ProjectedTextureHash));
        }
        public void ReadXml(XmlNode node)
        {
            Position = Xml.GetChildVector3Attributes(node, "Position");
            ColorR = (byte)Xml.GetChildUIntAttribute(node, "Colour", "r");
            ColorG = (byte)Xml.GetChildUIntAttribute(node, "Colour", "g");
            ColorB = (byte)Xml.GetChildUIntAttribute(node, "Colour", "b");
            Flashiness = (byte)Xml.GetChildUIntAttribute(node, "Flashiness", "value");
            Intensity = Xml.GetChildFloatAttribute(node, "Intensity", "value");
            Flags = Xml.GetChildUIntAttribute(node, "Flags", "value");
            BoneId = (ushort)Xml.GetChildUIntAttribute(node, "BoneId", "value");
            Type = Xml.GetChildEnumInnerText<LightType>(node, "Type");
            GroupId = (byte)Xml.GetChildUIntAttribute(node, "GroupId", "value");
            TimeFlags = Xml.GetChildUIntAttribute(node, "TimeFlags", "value");
            Falloff = Xml.GetChildFloatAttribute(node, "Falloff", "value");
            FalloffExponent = Xml.GetChildFloatAttribute(node, "FalloffExponent", "value");
            CullingPlaneNormal = Xml.GetChildVector3Attributes(node, "CullingPlaneNormal");
            CullingPlaneOffset = Xml.GetChildFloatAttribute(node, "CullingPlaneOffset", "value");
            Unknown_45h = (byte)Xml.GetChildUIntAttribute(node, "Unknown45", "value");
            Unknown_46h = (ushort)Xml.GetChildUIntAttribute(node, "Unknown46", "value");
            VolumeIntensity = Xml.GetChildFloatAttribute(node, "VolumeIntensity", "value");
            VolumeSizeScale = Xml.GetChildFloatAttribute(node, "VolumeSizeScale", "value");
            VolumeOuterColorR = (byte)Xml.GetChildUIntAttribute(node, "VolumeOuterColour", "r");
            VolumeOuterColorG = (byte)Xml.GetChildUIntAttribute(node, "VolumeOuterColour", "g");
            VolumeOuterColorB = (byte)Xml.GetChildUIntAttribute(node, "VolumeOuterColour", "b");
            LightHash = (byte)Xml.GetChildUIntAttribute(node, "LightHash", "value");
            VolumeOuterIntensity = Xml.GetChildFloatAttribute(node, "VolumeOuterIntensity", "value");
            CoronaSize = Xml.GetChildFloatAttribute(node, "CoronaSize", "value");
            VolumeOuterExponent = Xml.GetChildFloatAttribute(node, "VolumeOuterExponent", "value");
            LightFadeDistance = (byte)Xml.GetChildUIntAttribute(node, "LightFadeDistance", "value");
            ShadowBlur = (byte)Xml.GetChildUIntAttribute(node, "ShadowBlur", "value");
            ShadowFadeDistance = (byte)Xml.GetChildUIntAttribute(node, "ShadowFadeDistance", "value");
            SpecularFadeDistance = (byte)Xml.GetChildUIntAttribute(node, "SpecularFadeDistance", "value");
            VolumetricFadeDistance = (byte)Xml.GetChildUIntAttribute(node, "VolumetricFadeDistance", "value");
            ShadowNearClip = Xml.GetChildFloatAttribute(node, "ShadowNearClip", "value");
            CoronaIntensity = Xml.GetChildFloatAttribute(node, "CoronaIntensity", "value");
            CoronaZBias = Xml.GetChildFloatAttribute(node, "CoronaZBias", "value");
            Direction = Xml.GetChildVector3Attributes(node, "Direction");
            Tangent = Xml.GetChildVector3Attributes(node, "Tangent");
            ConeInnerAngle = Xml.GetChildFloatAttribute(node, "ConeInnerAngle", "value");
            ConeOuterAngle = Xml.GetChildFloatAttribute(node, "ConeOuterAngle", "value");
            Extent = Xml.GetChildVector3Attributes(node, "Extent");
            ProjectedTextureHash = XmlMeta.GetHash(Xml.GetChildInnerText(node, "ProjectedTextureHash"));
        }

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
        public Vector3 BoundingBoxMin { get; set; }
        public uint Unknown_3Ch { get; set; } = 0x7f800001;
        public Vector3 BoundingBoxMax { get; set; }
        public uint Unknown_4Ch { get; set; } = 0x7f800001;
        public ulong DrawableModelsHighPointer { get; set; }
        public ulong DrawableModelsMediumPointer { get; set; }
        public ulong DrawableModelsLowPointer { get; set; }
        public ulong DrawableModelsVeryLowPointer { get; set; }
        public float LodDistHigh { get; set; }
        public float LodDistMed { get; set; }
        public float LodDistLow { get; set; }
        public float LodDistVlow { get; set; }
        public uint RenderMaskFlagsHigh { get; set; }
        public uint RenderMaskFlagsMed { get; set; }
        public uint RenderMaskFlagsLow { get; set; }
        public uint RenderMaskFlagsVlow { get; set; }
        public ulong JointsPointer { get; set; }
        public ushort Unknown_98h { get; set; } // 0x0000
        public ushort DrawableModelsBlocksSize { get; set; } // divided by 16
        public uint Unknown_9Ch { get; set; } // 0x00000000
        public ulong DrawableModelsPointer { get; set; }

        public byte FlagsHigh
        {
            get { return (byte)(RenderMaskFlagsHigh & 0xFF); }
            set { RenderMaskFlagsHigh = (RenderMaskFlagsHigh & 0xFFFFFF00) + (value & 0xFFu); }
        }
        public byte FlagsMed
        {
            get { return (byte)(RenderMaskFlagsMed & 0xFF); }
            set { RenderMaskFlagsMed = (RenderMaskFlagsMed & 0xFFFFFF00) + (value & 0xFFu); }
        }
        public byte FlagsLow
        {
            get { return (byte)(RenderMaskFlagsLow & 0xFF); }
            set { RenderMaskFlagsLow = (RenderMaskFlagsLow & 0xFFFFFF00) + (value & 0xFFu); }
        }
        public byte FlagsVlow
        {
            get { return (byte)(RenderMaskFlagsVlow & 0xFF); }
            set { RenderMaskFlagsVlow = (RenderMaskFlagsVlow & 0xFFFFFF00) + (value & 0xFFu); }
        }
        public byte RenderMaskHigh
        {
            get { return (byte)((RenderMaskFlagsHigh >> 8) & 0xFF); }
            set { RenderMaskFlagsHigh = (RenderMaskFlagsHigh & 0xFFFF00FF) + ((value & 0xFFu) << 8); }
        }
        public byte RenderMaskMed
        {
            get { return (byte)((RenderMaskFlagsMed >> 8) & 0xFF); }
            set { RenderMaskFlagsMed = (RenderMaskFlagsMed & 0xFFFF00FF) + ((value & 0xFFu) << 8); }
        }
        public byte RenderMaskLow
        {
            get { return (byte)((RenderMaskFlagsLow >> 8) & 0xFF); }
            set { RenderMaskFlagsLow = (RenderMaskFlagsLow & 0xFFFF00FF) + ((value & 0xFFu) << 8); }
        }
        public byte RenderMaskVlow
        {
            get { return (byte)((RenderMaskFlagsVlow >> 8) & 0xFF); }
            set { RenderMaskFlagsVlow = (RenderMaskFlagsVlow & 0xFFFF00FF) + ((value & 0xFFu) << 8); }
        }


        // reference data
        public ShaderGroup ShaderGroup { get; set; }
        public Skeleton Skeleton { get; set; }
        public Joints Joints { get; set; }
        public DrawableModelsBlock DrawableModels { get; set; }


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


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.ShaderGroupPointer = reader.ReadUInt64();
            this.SkeletonPointer = reader.ReadUInt64();
            this.BoundingCenter = reader.ReadVector3();
            this.BoundingSphereRadius = reader.ReadSingle();
            this.BoundingBoxMin = reader.ReadVector3();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.BoundingBoxMax = reader.ReadVector3();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.DrawableModelsHighPointer = reader.ReadUInt64();
            this.DrawableModelsMediumPointer = reader.ReadUInt64();
            this.DrawableModelsLowPointer = reader.ReadUInt64();
            this.DrawableModelsVeryLowPointer = reader.ReadUInt64();
            this.LodDistHigh = reader.ReadSingle();
            this.LodDistMed = reader.ReadSingle();
            this.LodDistLow = reader.ReadSingle();
            this.LodDistVlow = reader.ReadSingle();
            this.RenderMaskFlagsHigh = reader.ReadUInt32();
            this.RenderMaskFlagsMed = reader.ReadUInt32();
            this.RenderMaskFlagsLow = reader.ReadUInt32();
            this.RenderMaskFlagsVlow = reader.ReadUInt32();
            this.JointsPointer = reader.ReadUInt64();
            this.Unknown_98h = reader.ReadUInt16();
            this.DrawableModelsBlocksSize = reader.ReadUInt16();
            this.Unknown_9Ch = reader.ReadUInt32();
            this.DrawableModelsPointer = reader.ReadUInt64();

            // read reference data
            this.ShaderGroup = reader.ReadBlockAt<ShaderGroup>(this.ShaderGroupPointer);
            this.Skeleton = reader.ReadBlockAt<Skeleton>(this.SkeletonPointer);
            this.Joints = reader.ReadBlockAt<Joints>(this.JointsPointer);
            this.DrawableModels = reader.ReadBlockAt<DrawableModelsBlock>((DrawableModelsPointer == 0) ? DrawableModelsHighPointer : DrawableModelsPointer, this);


            BuildAllModels();
            BuildVertexDecls();
            AssignGeometryShaders(ShaderGroup);


            ////just testing!!!

            //long pad(long o) => ((16 - (o % 16)) % 16);
            //long listlength(DrawableModel[] list)
            //{
            //    long l = 16;
            //    l += (list.Length) * 8;
            //    foreach (var m in list) l += pad(l) + m.BlockLength;
            //    return l;
            //}
            //var ptr = (long)DrawableModelsPointer;
            //if (DrawableModels?.High != null)
            //{
            //    if (ptr != (long)DrawableModelsHighPointer)
            //    { }//no hit
            //    ptr += listlength(DrawableModels?.High);
            //}
            //if (DrawableModels?.Med != null)
            //{
            //    ptr += pad(ptr);
            //    if (ptr != (long)DrawableModelsMediumPointer)
            //    { }//no hit
            //    ptr += listlength(DrawableModels?.Med);
            //}
            //if (DrawableModels?.Low != null)
            //{
            //    ptr += pad(ptr);
            //    if (ptr != (long)DrawableModelsLowPointer)
            //    { }//no hit
            //    ptr += listlength(DrawableModels?.Low);
            //}
            //if (DrawableModels?.VLow != null)
            //{
            //    ptr += pad(ptr);
            //    if (ptr != (long)DrawableModelsVeryLowPointer)
            //    { }//no hit
            //    ptr += listlength(DrawableModels?.VLow);
            //}


            //switch (Unknown_3Ch)
            //{
            //    case 0x7f800001:
            //    case 0: //only in yft's!
            //        break;
            //    default:
            //        break;
            //}
            //switch (Unknown_4Ch)
            //{
            //    case 0x7f800001:
            //    case 0: //only in yft's!
            //        break;
            //    default:
            //        break;
            //}
            //if ((DrawableModelsHigh?.data_items != null) != (Unknown_80h != 0))
            //{ }//no hit
            //if ((DrawableModelsMedium?.data_items != null) != (Unknown_84h != 0))
            //{ }//no hit
            //if ((DrawableModelsLow?.data_items != null) != (Unknown_88h != 0))
            //{ }//no hit
            //if ((DrawableModelsVeryLow?.data_items != null) != (Unknown_8Ch != 0))
            //{ }//no hit
            //if ((Unknown_80h & 0xFFFF0000) > 0)
            //{ }//no hit
            //if ((Unknown_84h & 0xFFFF0000) > 0)
            //{ }//no hit
            //if ((Unknown_88h & 0xFFFF0000) > 0)
            //{ }//no hit
            //if ((Unknown_8Ch & 0xFFFF0000) > 0)
            //{ }//no hit
            //BuildRenderMasks();

            //switch (FlagsHigh)
            //{
            //    case 2:
            //    case 1:
            //    case 13:
            //    case 4:
            //    case 12:
            //    case 5:
            //    case 3:
            //    case 8:
            //    case 9:
            //    case 15:
            //    case 130:
            //    case 11:
            //    case 10:
            //    case 7:
            //    case 131:
            //    case 129:
            //    case 75:
            //    case 69:
            //    case 6:
            //    case 64:
            //    case 14:
            //    case 77:
            //    case 73:
            //    case 76:
            //    case 71:
            //    case 79:
            //    case 65:
            //    case 0://some yft's have null HD models
            //    case 19:
            //    case 51:
            //        break;
            //    default:
            //        break;
            //}
            //switch (FlagsMed)
            //{
            //    case 0:
            //    case 1:
            //    case 9:
            //    case 8:
            //    case 13:
            //    case 3:
            //    case 2:
            //    case 5:
            //    case 11:
            //    case 15:
            //    case 10:
            //    case 12:
            //    case 4:
            //    case 7:
            //    case 51:
            //        break;
            //    default:
            //        break;
            //}
            //switch (FlagsLow)
            //{
            //    case 0:
            //    case 9:
            //    case 1:
            //    case 8:
            //    case 5:
            //    case 3:
            //    case 13:
            //    case 2:
            //    case 11:
            //    case 15:
            //    case 4:
            //    case 7:
            //    case 51:
            //        break;
            //    default:
            //        break;
            //}
            //switch (FlagsVlow)
            //{
            //    case 0:
            //    case 1:
            //    case 9:
            //    case 3:
            //    case 7:
            //    case 5:
            //    case 49:
            //    case 51:
            //    case 11:
            //        break;
            //    default:
            //        break;
            //}
            //switch (Unknown_98h)
            //{
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.ShaderGroupPointer = (ulong)(this.ShaderGroup != null ? this.ShaderGroup.FilePosition : 0);
            this.SkeletonPointer = (ulong)(this.Skeleton != null ? this.Skeleton.FilePosition : 0);
            this.DrawableModelsHighPointer = (ulong)(DrawableModels?.GetHighPointer() ?? 0);
            this.DrawableModelsMediumPointer = (ulong)(DrawableModels?.GetMedPointer() ?? 0);
            this.DrawableModelsLowPointer = (ulong)(DrawableModels?.GetLowPointer() ?? 0);
            this.DrawableModelsVeryLowPointer = (ulong)(DrawableModels?.GetVLowPointer() ?? 0);
            this.JointsPointer = (ulong)(this.Joints != null ? this.Joints.FilePosition : 0);
            this.DrawableModelsPointer = (ulong)(DrawableModels?.FilePosition ?? 0);
            this.DrawableModelsBlocksSize = (ushort)Math.Ceiling((DrawableModels?.BlockLength ?? 0) / 16.0);

            // write structure data
            writer.Write(this.ShaderGroupPointer);
            writer.Write(this.SkeletonPointer);
            writer.Write(this.BoundingCenter);
            writer.Write(this.BoundingSphereRadius);
            writer.Write(this.BoundingBoxMin);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.BoundingBoxMax);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.DrawableModelsHighPointer);
            writer.Write(this.DrawableModelsMediumPointer);
            writer.Write(this.DrawableModelsLowPointer);
            writer.Write(this.DrawableModelsVeryLowPointer);
            writer.Write(this.LodDistHigh);
            writer.Write(this.LodDistMed);
            writer.Write(this.LodDistLow);
            writer.Write(this.LodDistVlow);
            writer.Write(this.RenderMaskFlagsHigh);
            writer.Write(this.RenderMaskFlagsMed);
            writer.Write(this.RenderMaskFlagsLow);
            writer.Write(this.RenderMaskFlagsVlow);
            writer.Write(this.JointsPointer);
            writer.Write(this.Unknown_98h);
            writer.Write(this.DrawableModelsBlocksSize);
            writer.Write(this.Unknown_9Ch);
            writer.Write(this.DrawableModelsPointer);
        }
        public virtual void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            YdrXml.SelfClosingTag(sb, indent, "BoundingSphereCenter " + FloatUtil.GetVector3XmlString(BoundingCenter));
            YdrXml.ValueTag(sb, indent, "BoundingSphereRadius", FloatUtil.ToString(BoundingSphereRadius));
            YdrXml.SelfClosingTag(sb, indent, "BoundingBoxMin " + FloatUtil.GetVector3XmlString(BoundingBoxMin));
            YdrXml.SelfClosingTag(sb, indent, "BoundingBoxMax " + FloatUtil.GetVector3XmlString(BoundingBoxMax));
            YdrXml.ValueTag(sb, indent, "LodDistHigh", FloatUtil.ToString(LodDistHigh));
            YdrXml.ValueTag(sb, indent, "LodDistMed", FloatUtil.ToString(LodDistMed));
            YdrXml.ValueTag(sb, indent, "LodDistLow", FloatUtil.ToString(LodDistLow));
            YdrXml.ValueTag(sb, indent, "LodDistVlow", FloatUtil.ToString(LodDistVlow));
            YdrXml.ValueTag(sb, indent, "FlagsHigh", FlagsHigh.ToString());
            YdrXml.ValueTag(sb, indent, "FlagsMed", FlagsMed.ToString());
            YdrXml.ValueTag(sb, indent, "FlagsLow", FlagsLow.ToString());
            YdrXml.ValueTag(sb, indent, "FlagsVlow", FlagsVlow.ToString());
            if (ShaderGroup != null)
            {
                YdrXml.OpenTag(sb, indent, "ShaderGroup");
                ShaderGroup.WriteXml(sb, indent + 1, ddsfolder);
                YdrXml.CloseTag(sb, indent, "ShaderGroup");
            }
            if (Skeleton != null)
            {
                YdrXml.OpenTag(sb, indent, "Skeleton");
                Skeleton.WriteXml(sb, indent + 1);
                YdrXml.CloseTag(sb, indent, "Skeleton");
            }
            if (Joints != null)
            {
                YdrXml.OpenTag(sb, indent, "Joints");
                Joints.WriteXml(sb, indent + 1);
                YdrXml.CloseTag(sb, indent, "Joints");
            }
            if (DrawableModels?.High != null)
            {
                YdrXml.WriteItemArray(sb, DrawableModels.High, indent, "DrawableModelsHigh");
            }
            if (DrawableModels?.Med != null)
            {
                YdrXml.WriteItemArray(sb, DrawableModels.Med, indent, "DrawableModelsMedium");
            }
            if (DrawableModels?.Low != null)
            {
                YdrXml.WriteItemArray(sb, DrawableModels.Low, indent, "DrawableModelsLow");
            }
            if (DrawableModels?.VLow != null)
            {
                YdrXml.WriteItemArray(sb, DrawableModels.VLow, indent, "DrawableModelsVeryLow");
            }
            if (DrawableModels?.Extra != null)//is this right? duplicates..?
            {
                YdrXml.WriteItemArray(sb, DrawableModels.Extra, indent, "DrawableModelsX");
            }
        }
        public virtual void ReadXml(XmlNode node, string ddsfolder)
        {
            BoundingCenter = Xml.GetChildVector3Attributes(node, "BoundingSphereCenter");
            BoundingSphereRadius = Xml.GetChildFloatAttribute(node, "BoundingSphereRadius", "value");
            BoundingBoxMin = Xml.GetChildVector3Attributes(node, "BoundingBoxMin");
            BoundingBoxMax = Xml.GetChildVector3Attributes(node, "BoundingBoxMax");
            LodDistHigh = Xml.GetChildFloatAttribute(node, "LodDistHigh", "value");
            LodDistMed = Xml.GetChildFloatAttribute(node, "LodDistMed", "value");
            LodDistLow = Xml.GetChildFloatAttribute(node, "LodDistLow", "value");
            LodDistVlow = Xml.GetChildFloatAttribute(node, "LodDistVlow", "value");
            FlagsHigh = (byte)Xml.GetChildUIntAttribute(node, "FlagsHigh", "value");
            FlagsMed = (byte)Xml.GetChildUIntAttribute(node, "FlagsMed", "value");
            FlagsLow = (byte)Xml.GetChildUIntAttribute(node, "FlagsLow", "value");
            FlagsVlow = (byte)Xml.GetChildUIntAttribute(node, "FlagsVlow", "value");
            var sgnode = node.SelectSingleNode("ShaderGroup");
            if (sgnode != null)
            {
                ShaderGroup = new ShaderGroup();
                ShaderGroup.ReadXml(sgnode, ddsfolder);
            }
            var sknode = node.SelectSingleNode("Skeleton");
            if (sknode != null)
            {
                Skeleton = new Skeleton();
                Skeleton.ReadXml(sknode);
            }
            var jnode = node.SelectSingleNode("Joints");
            if (jnode != null)
            {
                Joints = new Joints();
                Joints.ReadXml(jnode);
            }
            this.DrawableModels = new DrawableModelsBlock();
            this.DrawableModels.High = XmlMeta.ReadItemArray<DrawableModel>(node, "DrawableModelsHigh");
            this.DrawableModels.Med = XmlMeta.ReadItemArray<DrawableModel>(node, "DrawableModelsMedium");
            this.DrawableModels.Low = XmlMeta.ReadItemArray<DrawableModel>(node, "DrawableModelsLow");
            this.DrawableModels.VLow = XmlMeta.ReadItemArray<DrawableModel>(node, "DrawableModelsVeryLow");
            this.DrawableModels.Extra = XmlMeta.ReadItemArray<DrawableModel>(node, "DrawableModelsX");
            if (DrawableModels.BlockLength == 0)
            {
                DrawableModels = null;
            }

            BuildRenderMasks();
            BuildAllModels();
            BuildVertexDecls();

            FileVFT = 1079456120;
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (ShaderGroup != null) list.Add(ShaderGroup);
            if (Skeleton != null) list.Add(Skeleton);
            if (Joints != null) list.Add(Joints);
            if (DrawableModels != null) list.Add(DrawableModels);
            return list.ToArray();
        }


        public void AssignGeometryShaders(ShaderGroup shaderGrp)
        {
            //if (shaderGrp != null)
            //{
            //    ShaderGroup = shaderGrp;
            //}

            //map the shaders to the geometries
            if (shaderGrp?.Shaders?.data_items != null)
            {
                var shaders = shaderGrp.Shaders.data_items;
                foreach (DrawableModel model in AllModels)
                {
                    if (model?.Geometries == null) continue;

                    int geomcount = model.Geometries.Length;
                    for (int i = 0; i < geomcount; i++)
                    {
                        var geom = model.Geometries[i];
                        var sid = geom.ShaderID;
                        geom.Shader = (sid < shaders.Length) ? shaders[sid] : null;
                    }
                }
            }
            else
            {
            }

        }



        public void BuildAllModels()
        {
            var allModels = new List<DrawableModel>();
            if (DrawableModels?.High != null) allModels.AddRange(DrawableModels.High);
            if (DrawableModels?.Med != null) allModels.AddRange(DrawableModels.Med);
            if (DrawableModels?.Low != null) allModels.AddRange(DrawableModels.Low);
            if (DrawableModels?.VLow != null) allModels.AddRange(DrawableModels.VLow);
            if (DrawableModels?.Extra != null) allModels.AddRange(DrawableModels.Extra);
            AllModels = allModels.ToArray();
        }

        public void BuildVertexDecls()
        {
            var vds = new Dictionary<ulong, VertexDeclaration>();
            foreach (DrawableModel model in AllModels)
            {
                if (model.Geometries == null) continue;
                foreach (var geom in model.Geometries)
                {
                    var info = geom.VertexBuffer.Info;
                    if (info == null) continue;
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
        }


        public void BuildRenderMasks()
        {
            var hmask = BuildRenderMask(DrawableModels?.High);
            var mmask = BuildRenderMask(DrawableModels?.Med);
            var lmask = BuildRenderMask(DrawableModels?.Low);
            var vmask = BuildRenderMask(DrawableModels?.VLow);

            ////just testing
            //if (hmask != RenderMaskHigh)
            //{ }//no hit
            //if (mmask != RenderMaskMed)
            //{ }//no hit
            //if (lmask != RenderMaskLow)
            //{ }//no hit
            //if (vmask != RenderMaskVlow)
            //{ }//no hit

            RenderMaskHigh = hmask;
            RenderMaskMed = mmask;
            RenderMaskLow = lmask;
            RenderMaskVlow = vmask;

        }
        private byte BuildRenderMask(DrawableModel[] models)
        {
            byte mask = 0;
            if (models != null)
            {
                foreach (var model in models)
                {
                    mask = (byte)(mask | model.RenderMask);
                }
            }
            return mask;
        }


        public DrawableBase ShallowCopy()
        {
            DrawableBase r = null;
            if (this is FragDrawable fd)
            {
                var f = new FragDrawable();
                f.FragMatrix = fd.FragMatrix;
                f.FragMatricesIndsCount = fd.FragMatricesIndsCount;
                f.FragMatricesCapacity = fd.FragMatricesCapacity;
                f.FragMatricesCount = fd.FragMatricesCount;
                f.Bound = fd.Bound;
                f.FragMatricesInds = fd.FragMatricesInds;
                f.FragMatrices = fd.FragMatrices;
                f.Name = fd.Name;
                f.OwnerFragment = fd.OwnerFragment;
                f.OwnerFragmentPhys = fd.OwnerFragmentPhys;
                r = f;
            }
            if (this is Drawable dd)
            {
                var d = new Drawable();
                d.LightAttributes = dd.LightAttributes;
                d.Name = dd.Name;
                d.Bound = dd.Bound;
                r = d;
            }
            if (r != null)
            {
                r.BoundingCenter = BoundingCenter;
                r.BoundingSphereRadius = BoundingSphereRadius;
                r.BoundingBoxMin = BoundingBoxMin;
                r.BoundingBoxMax = BoundingBoxMax;
                r.LodDistHigh = LodDistHigh;
                r.LodDistMed = LodDistMed;
                r.LodDistLow = LodDistLow;
                r.LodDistVlow = LodDistVlow;
                r.RenderMaskFlagsHigh = RenderMaskFlagsHigh;
                r.RenderMaskFlagsMed = RenderMaskFlagsMed;
                r.RenderMaskFlagsLow = RenderMaskFlagsLow;
                r.RenderMaskFlagsVlow = RenderMaskFlagsVlow;
                r.Unknown_98h = Unknown_98h;
                r.DrawableModelsBlocksSize = DrawableModelsBlocksSize;
                r.ShaderGroup = ShaderGroup;
                r.Skeleton = Skeleton?.Clone();
                r.DrawableModels = new DrawableModelsBlock();
                r.DrawableModels.High = DrawableModels?.High;
                r.DrawableModels.Med = DrawableModels?.Med;
                r.DrawableModels.Low = DrawableModels?.Low;
                r.DrawableModels.VLow = DrawableModels?.VLow;
                r.DrawableModels.Extra = DrawableModels?.Extra;
                r.Joints = Joints;
                r.AllModels = AllModels;
                r.VertexDecls = VertexDecls;
                r.Owner = Owner;
            }
            return r;
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
        public ResourceSimpleList64<LightAttributes> LightAttributes { get; set; }
        public ulong UnkPointer { get; set; } 
        public ulong BoundPointer { get; set; }

        // reference data
        public string Name { get; set; }
        public Bounds Bound { get; set; }

        public string ErrorMessage { get; set; }


        private string_r NameBlock = null;//only used when saving..


#if DEBUG
        public ResourceAnalyzer Analyzer { get; set; }
#endif


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.NamePointer = reader.ReadUInt64();
            this.LightAttributes = reader.ReadBlock<ResourceSimpleList64<LightAttributes>>();
            this.UnkPointer = reader.ReadUInt64();
            this.BoundPointer = reader.ReadUInt64();

            try
            {

                // read reference data
                this.Name = reader.ReadStringAt(this.NamePointer);
                this.Bound = reader.ReadBlockAt<Bounds>(this.BoundPointer);
                if (Bound != null)
                {
                    Bound.Owner = this;
                }

            }
            catch (Exception ex) 
            {
                ErrorMessage = ex.ToString();
            }

            if (UnkPointer != 0)
            { }


#if DEBUG
            Analyzer = new ResourceAnalyzer(reader);
#endif

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.NamePointer = (ulong)(this.NameBlock != null ? this.NameBlock.FilePosition : 0);
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);

            // write structure data
            writer.Write(this.NamePointer);
            writer.WriteBlock(this.LightAttributes);
            writer.Write(this.UnkPointer);
            writer.Write(this.BoundPointer);
        }
        public override void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            YdrXml.StringTag(sb, indent, "Name", YdrXml.XmlEscape(Name));
            base.WriteXml(sb, indent, ddsfolder);
            if (Bound != null)
            {
                Bounds.WriteXmlNode(Bound, sb, indent);
            }
            if (LightAttributes?.data_items != null)
            {
                YdrXml.WriteItemArray(sb, LightAttributes.data_items, indent, "Lights");
            }
        }
        public override void ReadXml(XmlNode node, string ddsfolder)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            base.ReadXml(node, ddsfolder);
            var bnode = node.SelectSingleNode("Bounds");
            if (bnode != null)
            {
                Bound = Bounds.ReadXmlNode(bnode, this);
            }

            LightAttributes = new ResourceSimpleList64<LightAttributes>();
            LightAttributes.data_items = XmlMeta.ReadItemArray<LightAttributes>(node, "Lights");

        }
        public static void WriteXmlNode(Drawable d, StringBuilder sb, int indent, string ddsfolder, string name = "Drawable")
        {
            if (d == null) return;
            YdrXml.OpenTag(sb, indent, name);
            d.WriteXml(sb, indent + 1, ddsfolder);
            YdrXml.CloseTag(sb, indent, name);
        }
        public static Drawable ReadXmlNode(XmlNode node, string ddsfolder)
        {
            if (node == null) return null;
            var d = new Drawable();
            d.ReadXml(node, ddsfolder);
            return d;
        }


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

    [TypeConverter(typeof(ExpandableObjectConverter))] public class DrawablePtfx : DrawableBase
    {
        public override long BlockLength
        {
            get { return 176; }
        }

        // structure data
        public ulong UnkPointer { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.UnkPointer = reader.ReadUInt64();

            if (UnkPointer != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.UnkPointer);
        }
        public override void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            base.WriteXml(sb, indent, ddsfolder);
        }
        public override void ReadXml(XmlNode node, string ddsfolder)
        {
            base.ReadXml(node, ddsfolder);
        }
        public static void WriteXmlNode(DrawablePtfx d, StringBuilder sb, int indent, string ddsfolder, string name = "Drawable")
        {
            if (d == null) return;
            YdrXml.OpenTag(sb, indent, name);
            d.WriteXml(sb, indent + 1, ddsfolder);
            YdrXml.CloseTag(sb, indent, name);
        }
        public static DrawablePtfx ReadXmlNode(XmlNode node, string ddsfolder)
        {
            if (node == null) return null;
            var d = new DrawablePtfx();
            d.ReadXml(node, ddsfolder);
            return d;
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class DrawablePtfxDictionary : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h = 1; // 0x0000000000000001
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
        public ResourcePointerArray64<DrawablePtfx> Drawables { get; set; }


        private ResourceSystemStructBlock<uint> HashesBlock = null;//only used for saving


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
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
            this.Drawables = reader.ReadBlockAt<ResourcePointerArray64<DrawablePtfx>>(this.DrawablesPointer, this.DrawablesCount1);

            //if (Unknown_10h != 0)
            //{ }
            //if (Unknown_18h != 1)
            //{ }
            //if (Unknown_2Ch != 0)
            //{ }
            //if (Unknown_3Ch != 0)
            //{ }
        }
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
            writer.Write(this.Unknown_18h);
            writer.Write(this.HashesPointer);
            writer.Write(this.HashesCount1);
            writer.Write(this.HashesCount2);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.DrawablesPointer);
            writer.Write(this.DrawablesCount1);
            writer.Write(this.DrawablesCount2);
            writer.Write(this.Unknown_3Ch);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            if (Drawables?.data_items != null)
            {
                for (int i=0; i< Drawables.data_items.Length; i++)
                {
                    var d = Drawables.data_items[i];
                    var h = (MetaHash)((i < (Hashes?.Length ?? 0)) ? Hashes[i] : 0);
                    YddXml.OpenTag(sb, indent, "Item");
                    YddXml.StringTag(sb, indent + 1, "Name", YddXml.XmlEscape(h.ToCleanString()));
                    d.WriteXml(sb, indent + 1, ddsfolder);
                    YddXml.CloseTag(sb, indent, "Item");
                }
            }
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            var drawables = new List<DrawablePtfx>();
            var drawablehashes = new List<uint>();

            var inodes = node.SelectNodes("Item");
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var h = XmlMeta.GetHash(Xml.GetChildInnerText(inode, "Name"));
                    var d = new DrawablePtfx();
                    d.ReadXml(inode, ddsfolder);
                    drawables.Add(d);
                    drawablehashes.Add(h);
                }
            }
            if (drawables.Count > 0)
            {
                Hashes = drawablehashes.ToArray();
                Drawables = new ResourcePointerArray64<DrawablePtfx>();
                Drawables.data_items = drawables.ToArray();
            }
        }
        public static void WriteXmlNode(DrawablePtfxDictionary d, StringBuilder sb, int indent, string ddsfolder, string name = "DrawableDictionary")
        {
            if (d == null) return;
            YddXml.OpenTag(sb, indent, name);
            d.WriteXml(sb, indent + 1, ddsfolder);
            YddXml.CloseTag(sb, indent, name);
        }
        public static DrawablePtfxDictionary ReadXmlNode(XmlNode node, string ddsfolder)
        {
            if (node == null) return null;
            var d = new DrawablePtfxDictionary();
            d.ReadXml(node, ddsfolder);
            return d;
        }

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
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h = 1; // 0x0000000000000001
        public ulong HashesPointer { get; set; }
        public ushort HashesCount1 { get; set; }
        public ushort HashesCount2 { get; set; }
        public uint Unknown_2Ch; // 0x00000000
        public ulong DrawablesPointer { get; set; }
        public ushort DrawablesCount1 { get; set; }
        public ushort DrawablesCount2 { get; set; }
        public uint Unknown_3Ch; // 0x00000000

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

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
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

            //if (Unknown_10h != 0)
            //{ }
            //if (Unknown_18h != 1)
            //{ }
            //if (Unknown_2Ch != 0)
            //{ }
            //if (Unknown_3Ch != 0)
            //{ }
        }
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
            writer.Write(this.Unknown_18h);
            writer.Write(this.HashesPointer);
            writer.Write(this.HashesCount1);
            writer.Write(this.HashesCount2);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.DrawablesPointer);
            writer.Write(this.DrawablesCount1);
            writer.Write(this.DrawablesCount2);
            writer.Write(this.Unknown_3Ch);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            if (Drawables?.data_items != null)
            {
                foreach (var d in Drawables.data_items)
                {
                    YddXml.OpenTag(sb, indent, "Item");
                    d.WriteXml(sb, indent + 1, ddsfolder);
                    YddXml.CloseTag(sb, indent, "Item");
                }
            }
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            var drawables = new List<Drawable>();
            var drawablehashes = new List<uint>();

            var inodes = node.SelectNodes("Item");
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var d = new Drawable();
                    d.ReadXml(inode, ddsfolder);
                    drawables.Add(d);
                    drawablehashes.Add(XmlMeta.GetHash(d.Name));
                }
            }

            Hashes = drawablehashes.ToArray();
            Drawables = new ResourcePointerArray64<Drawable>();
            Drawables.data_items = drawables.ToArray();
        }
        public static void WriteXmlNode(DrawableDictionary d, StringBuilder sb, int indent, string ddsfolder, string name = "DrawableDictionary")
        {
            if (d == null) return;
            YddXml.OpenTag(sb, indent, name);
            d.WriteXml(sb, indent + 1, ddsfolder);
            YddXml.CloseTag(sb, indent, name);
        }
        public static DrawableDictionary ReadXmlNode(XmlNode node, string ddsfolder)
        {
            if (node == null) return null;
            var d = new DrawableDictionary();
            d.ReadXml(node, ddsfolder);
            return d;
        }

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
