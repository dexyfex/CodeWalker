using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;


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



    //proudly mangled by dex

*/



namespace CodeWalker.GameFiles
{

    // pgBase
    // ptxFxList
    [TC(typeof(EXP))] public class ParticleEffectsList : ResourceFileBase
    {
        public override long BlockLength => 0x60;

        // structure data
        public ulong NamePointer { get; set; }
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ulong TextureDictionaryPointer { get; set; }
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ulong DrawableDictionaryPointer { get; set; }
        public ulong ParticleRuleDictionaryPointer { get; set; }
        public uint Unknown_40h { get; set; } // 0x00000000
        public uint Unknown_44h { get; set; } // 0x00000000
        public ulong EmitterRuleDictionaryPointer { get; set; }
        public ulong EffectRuleDictionaryPointer { get; set; }
        public uint Unknown_58h { get; set; } // 0x00000000
        public uint Unknown_5Ch { get; set; } // 0x00000000

        // reference data
        public string_r Name { get; set; }
        public TextureDictionary TextureDictionary { get; set; }
        public DrawableDictionary DrawableDictionary { get; set; }
        public ParticleRuleDictionary ParticleRuleDictionary { get; set; }
        public ParticleEffectRuleDictionary EffectRuleDictionary { get; set; }
        public ParticleEmitterRuleDictionary EmitterRuleDictionary { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.NamePointer = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.TextureDictionaryPointer = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.DrawableDictionaryPointer = reader.ReadUInt64();
            this.ParticleRuleDictionaryPointer = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.EmitterRuleDictionaryPointer = reader.ReadUInt64();
            this.EffectRuleDictionaryPointer = reader.ReadUInt64();
            this.Unknown_58h = reader.ReadUInt32();
            this.Unknown_5Ch = reader.ReadUInt32();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(
                this.NamePointer // offset
            );
            this.TextureDictionary = reader.ReadBlockAt<TextureDictionary>(
                this.TextureDictionaryPointer // offset
            );
            this.DrawableDictionary = reader.ReadBlockAt<DrawableDictionary>(
                this.DrawableDictionaryPointer // offset
            );
            this.ParticleRuleDictionary = reader.ReadBlockAt<ParticleRuleDictionary>(
                this.ParticleRuleDictionaryPointer // offset
            );
            this.EffectRuleDictionary = reader.ReadBlockAt<ParticleEffectRuleDictionary>(
                this.EmitterRuleDictionaryPointer // offset
            );
            this.EmitterRuleDictionary = reader.ReadBlockAt<ParticleEmitterRuleDictionary>(
                this.EffectRuleDictionaryPointer // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            //this.NamePointer = (ulong)(this.Name != null ? this.Name.Position : 0);//TODO: fix?!
            //this.TextureDictionaryPointer = (ulong)(this.TextureDictionary != null ? this.TextureDictionary.Position : 0);
            //this.DrawableDictionaryPointer = (ulong)(this.DrawableDictionary != null ? this.DrawableDictionary.Position : 0);
            //this.ParticleRuleDictionaryPointer = (ulong)(this.ParticleRuleDictionary != null ? this.ParticleRuleDictionary.Position : 0);
            //this.EmitterRuleDictionaryPointer = (ulong)(this.EffectRuleDictionary != null ? this.EffectRuleDictionary.Position : 0);
            //this.EffectRuleDictionaryPointer = (ulong)(this.EmitterRuleDictionary != null ? this.EmitterRuleDictionary.Position : 0);

            // write structure data
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.TextureDictionaryPointer);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.DrawableDictionaryPointer);
            writer.Write(this.ParticleRuleDictionaryPointer);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.EmitterRuleDictionaryPointer);
            writer.Write(this.EffectRuleDictionaryPointer);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences()); //TODO: fix!!
            //if (Name != null) list.Add(Name);
            //if (TextureDictionary != null) list.Add(TextureDictionary);
            //if (DrawableDictionary != null) list.Add(DrawableDictionary);
            //if (ParticleRuleDictionary != null) list.Add(ParticleRuleDictionary);
            //if (EffectRuleDictionary != null) list.Add(EffectRuleDictionary);
            //if (EmitterRuleDictionary != null) list.Add(EmitterRuleDictionary);
            return list.ToArray();
        }
    }





    // pgBase
    // pgDictionaryBase
    // pgDictionary<ptxParticleRule>
    [TC(typeof(EXP))] public class ParticleRuleDictionary : ResourceSystemBlock
    {
        public override long BlockLength => 0x40;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000001
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ResourceSimpleList64<uint_r> ParticleRuleNameHashes { get; set; }
        public ResourcePointerList64<ParticleRule> ParticleRules { get; set; }

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
            this.ParticleRuleNameHashes = reader.ReadBlock<ResourceSimpleList64<uint_r>>();
            this.ParticleRules = reader.ReadBlock<ResourcePointerList64<ParticleRule>>();
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
            writer.WriteBlock(this.ParticleRuleNameHashes);
            writer.WriteBlock(this.ParticleRules);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, ParticleRuleNameHashes),
                new Tuple<long, IResourceBlock>(0x30, ParticleRules)
            };
        }
    }





    // pgBase
    // pgDictionaryBase
    // pgDictionary<ptxEffectRule>
    [TC(typeof(EXP))] public class ParticleEffectRuleDictionary : ResourceSystemBlock
    {
        public override long BlockLength => 0x40;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000001
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ResourceSimpleList64<uint_r> EffectRuleNameHashes { get; set; }
        public ResourcePointerList64<ParticleEffectRule> EffectRules { get; set; }

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
            this.EffectRuleNameHashes = reader.ReadBlock<ResourceSimpleList64<uint_r>>();
            this.EffectRules = reader.ReadBlock<ResourcePointerList64<ParticleEffectRule>>();
        }

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
            writer.WriteBlock(this.EffectRuleNameHashes);
            writer.WriteBlock(this.EffectRules);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, EffectRuleNameHashes),
                new Tuple<long, IResourceBlock>(0x30, EffectRules)
            };
        }
    }





    // pgBase
    // pgDictionaryBase
    // pgDictionary<ptxEmitterRule>
    [TC(typeof(EXP))] public class ParticleEmitterRuleDictionary : ResourceSystemBlock
    {
        public override long BlockLength => 0x40;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000001
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ulong HashesPointer { get; set; }
        public ushort HashesCount1 { get; set; }
        public ushort HashesCount2 { get; set; }
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ulong EffectRulesPointer { get; set; }
        public ushort EffectRulesCount1 { get; set; }
        public ushort EffectRulesCount2 { get; set; }
        public uint Unknown_3Ch { get; set; } // 0x00000000

        // reference data
        public ResourceSimpleArray<uint_r> Hashes { get; set; }
        public ResourcePointerArray64<ParticleEmitterRule> EmitterRules { get; set; }

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
            this.HashesPointer = reader.ReadUInt64();
            this.HashesCount1 = reader.ReadUInt16();
            this.HashesCount2 = reader.ReadUInt16();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.EffectRulesPointer = reader.ReadUInt64();
            this.EffectRulesCount1 = reader.ReadUInt16();
            this.EffectRulesCount2 = reader.ReadUInt16();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            this.Hashes = reader.ReadBlockAt<ResourceSimpleArray<uint_r>>(
                this.HashesPointer, // offset
                this.HashesCount1
            );
            this.EmitterRules = reader.ReadBlockAt<ResourcePointerArray64<ParticleEmitterRule>>(
                this.EffectRulesPointer, // offset
                this.EffectRulesCount1
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            //this.HashesPointer = (ulong)(this.Hashes != null ? this.Hashes.Position : 0);
            ////this.HashesCount1 = (ushort)(this.Hashes != null ? this.Hashes.Count : 0);
            //this.EffectRulesPointer = (ulong)(this.EmitterRules != null ? this.EmitterRules.Position : 0);
            ////this.EffectRulesCount1 = (ushort)(this.EffectRules != null ? this.EffectRules.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.HashesPointer);
            writer.Write(this.HashesCount1);
            writer.Write(this.HashesCount2);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.EffectRulesPointer);
            writer.Write(this.EffectRulesCount1);
            writer.Write(this.EffectRulesCount2);
            writer.Write(this.Unknown_3Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Hashes != null) list.Add(Hashes);
            if (EmitterRules != null) list.Add(EmitterRules);
            return list.ToArray();
        }
    }








    // pgBase
    // pgBaseRefCounted
    // ptxParticleRule
    [TC(typeof(EXP))] public class ParticleRule : ResourceSystemBlock
    {
        public override long BlockLength => 0x240;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ParticleEffectSpawner emb1 { get; set; }
        public ParticleEffectSpawner emb2 { get; set; }
        public uint Unknown_100h { get; set; }
        public uint Unknown_104h { get; set; }
        public uint Unknown_108h { get; set; }
        public uint Unknown_10Ch { get; set; }
        public uint Unknown_110h { get; set; } // 0x00000000
        public uint Unknown_114h { get; set; }
        public uint Unknown_118h { get; set; }
        public uint Unknown_11Ch { get; set; }
        public ulong NamePointer { get; set; }
        public ResourcePointerList64<ParticleBehaviour> Unknown_128h { get; set; }
        public ResourcePointerList64<ParticleBehaviour> Unknown_138h { get; set; }
        public ResourcePointerList64<ParticleBehaviour> Unknown_148h { get; set; }
        public ResourcePointerList64<ParticleBehaviour> Unknown_158h { get; set; }
        public ResourcePointerList64<ParticleBehaviour> Unknown_168h { get; set; }
        public uint Unknown_178h { get; set; } // 0x00000000
        public uint Unknown_17Ch { get; set; } // 0x00000000
        public uint Unknown_180h { get; set; } // 0x00000000
        public uint Unknown_184h { get; set; } // 0x00000000
        public ResourceSimpleList64<ParticleUnknown_P_013> Unknown_188h { get; set; }
        public uint Unknown_198h { get; set; } // 0x00000000
        public uint Unknown_19Ch { get; set; } // 0x00000000
        public uint Unknown_1A0h { get; set; } // 0x00000000
        public uint Unknown_1A4h { get; set; } // 0x00000000
        public uint Unknown_1A8h { get; set; } // 0x00000000
        public uint Unknown_1ACh { get; set; } // 0x00000000
        public uint VFTx3 { get; set; }
        public uint Unknown_1B4h { get; set; } // 0x00000001
        public ulong p9 { get; set; }
        public ulong p10 { get; set; }
        public uint Unknown_1C8h { get; set; } // 0x00000000
        public uint Unknown_1CCh { get; set; } // 0x00000000
        public uint Unknown_1D0h { get; set; }
        public uint Unknown_1D4h { get; set; } // 0x00000000
        public uint VFTx4 { get; set; }
        public uint Unknown_1DCh { get; set; } // 0x00000001
        public uint Unknown_1E0h { get; set; }
        public uint Unknown_1E4h { get; set; }
        public uint Unknown_1E8h { get; set; }
        public uint Unknown_1ECh { get; set; }
        public ResourcePointerList64<ParticleShaderVar> ShaderVars { get; set; }
        public uint Unknown_200h { get; set; } // 0x00000001
        public uint Unknown_204h { get; set; } // 0x00000000
        public uint Unknown_208h { get; set; }
        public uint Unknown_20Ch { get; set; } // 0x00000000
        public ResourceSimpleList64<ParticleUnknown_P_012> Unknown_210h { get; set; }
        public uint Unknown_220h { get; set; }
        public uint Unknown_224h { get; set; } // 0x00000000
        public uint Unknown_228h { get; set; } // 0x00000000
        public uint Unknown_22Ch { get; set; } // 0x00000000
        public uint Unknown_230h { get; set; } // 0x00000000
        public uint Unknown_234h { get; set; } // 0x00000000
        public uint Unknown_238h { get; set; } // 0x00000000
        public uint Unknown_23Ch { get; set; } // 0x00000000

        // reference data
        public string_r Name { get; set; }
        public string_r p9data { get; set; }
        public string_r p10data { get; set; }

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
            this.emb1 = reader.ReadBlock<ParticleEffectSpawner>();
            this.emb2 = reader.ReadBlock<ParticleEffectSpawner>();
            this.Unknown_100h = reader.ReadUInt32();
            this.Unknown_104h = reader.ReadUInt32();
            this.Unknown_108h = reader.ReadUInt32();
            this.Unknown_10Ch = reader.ReadUInt32();
            this.Unknown_110h = reader.ReadUInt32();
            this.Unknown_114h = reader.ReadUInt32();
            this.Unknown_118h = reader.ReadUInt32();
            this.Unknown_11Ch = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.Unknown_128h = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            this.Unknown_138h = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            this.Unknown_148h = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            this.Unknown_158h = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            this.Unknown_168h = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            this.Unknown_178h = reader.ReadUInt32();
            this.Unknown_17Ch = reader.ReadUInt32();
            this.Unknown_180h = reader.ReadUInt32();
            this.Unknown_184h = reader.ReadUInt32();
            this.Unknown_188h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_013>>();
            this.Unknown_198h = reader.ReadUInt32();
            this.Unknown_19Ch = reader.ReadUInt32();
            this.Unknown_1A0h = reader.ReadUInt32();
            this.Unknown_1A4h = reader.ReadUInt32();
            this.Unknown_1A8h = reader.ReadUInt32();
            this.Unknown_1ACh = reader.ReadUInt32();
            this.VFTx3 = reader.ReadUInt32();
            this.Unknown_1B4h = reader.ReadUInt32();
            this.p9 = reader.ReadUInt64();
            this.p10 = reader.ReadUInt64();
            this.Unknown_1C8h = reader.ReadUInt32();
            this.Unknown_1CCh = reader.ReadUInt32();
            this.Unknown_1D0h = reader.ReadUInt32();
            this.Unknown_1D4h = reader.ReadUInt32();
            this.VFTx4 = reader.ReadUInt32();
            this.Unknown_1DCh = reader.ReadUInt32();
            this.Unknown_1E0h = reader.ReadUInt32();
            this.Unknown_1E4h = reader.ReadUInt32();
            this.Unknown_1E8h = reader.ReadUInt32();
            this.Unknown_1ECh = reader.ReadUInt32();
            this.ShaderVars = reader.ReadBlock<ResourcePointerList64<ParticleShaderVar>>();
            this.Unknown_200h = reader.ReadUInt32();
            this.Unknown_204h = reader.ReadUInt32();
            this.Unknown_208h = reader.ReadUInt32();
            this.Unknown_20Ch = reader.ReadUInt32();
            this.Unknown_210h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_012>>();
            this.Unknown_220h = reader.ReadUInt32();
            this.Unknown_224h = reader.ReadUInt32();
            this.Unknown_228h = reader.ReadUInt32();
            this.Unknown_22Ch = reader.ReadUInt32();
            this.Unknown_230h = reader.ReadUInt32();
            this.Unknown_234h = reader.ReadUInt32();
            this.Unknown_238h = reader.ReadUInt32();
            this.Unknown_23Ch = reader.ReadUInt32();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(
                this.NamePointer // offset
            );
            this.p9data = reader.ReadBlockAt<string_r>(
                this.p9 // offset
            );
            this.p10data = reader.ReadBlockAt<string_r>(
                this.p10 // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            //// update structure data //TODO: fix!
            //this.NamePointer = (ulong)(this.Name != null ? this.Name.Position : 0);
            //this.p9 = (ulong)(this.p9data != null ? this.p9data.Position : 0);
            //this.p10 = (ulong)(this.p10data != null ? this.p10data.Position : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteBlock(this.emb1);
            writer.WriteBlock(this.emb2);
            writer.Write(this.Unknown_100h);
            writer.Write(this.Unknown_104h);
            writer.Write(this.Unknown_108h);
            writer.Write(this.Unknown_10Ch);
            writer.Write(this.Unknown_110h);
            writer.Write(this.Unknown_114h);
            writer.Write(this.Unknown_118h);
            writer.Write(this.Unknown_11Ch);
            writer.Write(this.NamePointer);
            writer.WriteBlock(this.Unknown_128h);
            writer.WriteBlock(this.Unknown_138h);
            writer.WriteBlock(this.Unknown_148h);
            writer.WriteBlock(this.Unknown_158h);
            writer.WriteBlock(this.Unknown_168h);
            writer.Write(this.Unknown_178h);
            writer.Write(this.Unknown_17Ch);
            writer.Write(this.Unknown_180h);
            writer.Write(this.Unknown_184h);
            writer.WriteBlock(this.Unknown_188h);
            writer.Write(this.Unknown_198h);
            writer.Write(this.Unknown_19Ch);
            writer.Write(this.Unknown_1A0h);
            writer.Write(this.Unknown_1A4h);
            writer.Write(this.Unknown_1A8h);
            writer.Write(this.Unknown_1ACh);
            writer.Write(this.VFTx3);
            writer.Write(this.Unknown_1B4h);
            writer.Write(this.p9);
            writer.Write(this.p10);
            writer.Write(this.Unknown_1C8h);
            writer.Write(this.Unknown_1CCh);
            writer.Write(this.Unknown_1D0h);
            writer.Write(this.Unknown_1D4h);
            writer.Write(this.VFTx4);
            writer.Write(this.Unknown_1DCh);
            writer.Write(this.Unknown_1E0h);
            writer.Write(this.Unknown_1E4h);
            writer.Write(this.Unknown_1E8h);
            writer.Write(this.Unknown_1ECh);
            writer.WriteBlock(this.ShaderVars);
            writer.Write(this.Unknown_200h);
            writer.Write(this.Unknown_204h);
            writer.Write(this.Unknown_208h);
            writer.Write(this.Unknown_20Ch);
            writer.WriteBlock(this.Unknown_210h);
            writer.Write(this.Unknown_220h);
            writer.Write(this.Unknown_224h);
            writer.Write(this.Unknown_228h);
            writer.Write(this.Unknown_22Ch);
            writer.Write(this.Unknown_230h);
            writer.Write(this.Unknown_234h);
            writer.Write(this.Unknown_238h);
            writer.Write(this.Unknown_23Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            //if (Name != null) list.Add(Name);
            //if (p9data != null) list.Add(p9data);
            //if (p10data != null) list.Add(p10data);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(88, emb1),
                new Tuple<long, IResourceBlock>(96, emb2),
                new Tuple<long, IResourceBlock>(0x128, Unknown_128h),
                new Tuple<long, IResourceBlock>(0x138, Unknown_138h),
                new Tuple<long, IResourceBlock>(0x148, Unknown_148h),
                new Tuple<long, IResourceBlock>(0x158, Unknown_158h),
                new Tuple<long, IResourceBlock>(0x168, Unknown_168h),
                new Tuple<long, IResourceBlock>(0x188, Unknown_188h),
                new Tuple<long, IResourceBlock>(0x1F0, ShaderVars),
                new Tuple<long, IResourceBlock>(0x210, Unknown_210h)
            };
        }
    }



    // pgBase
    // ptxEffectSpawner
    [TC(typeof(EXP))] public class ParticleEffectSpawner : ResourceSystemBlock
    {
        public override long BlockLength => 0x70;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; }
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; }
        public uint Unknown_3Ch { get; set; }
        public uint Unknown_40h { get; set; }
        public uint Unknown_44h { get; set; }
        public uint Unknown_48h { get; set; }
        public uint Unknown_4Ch { get; set; } // 0x00000000
        public uint Unknown_50h { get; set; } // 0x00000000
        public uint Unknown_54h { get; set; } // 0x00000000
        public ulong EmitterRulePointer { get; set; }
        public ulong p1 { get; set; }
        public uint Unknown_68h { get; set; }
        public uint Unknown_6Ch { get; set; }

        // reference data
        public ParticleEffectRule EmitterRule { get; set; }
        public string_r p1data { get; set; }

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
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.EmitterRulePointer = reader.ReadUInt64();
            this.p1 = reader.ReadUInt64();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();

            // read reference data
            this.EmitterRule = reader.ReadBlockAt<ParticleEffectRule>(
                this.EmitterRulePointer // offset
            );
            this.p1data = reader.ReadBlockAt<string_r>(
                this.p1 // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data //TODO: fix!!
            //this.EmitterRulePointer = (ulong)(this.EmitterRule != null ? this.EmitterRule.Position : 0);
            //this.p1 = (ulong)(this.p1data != null ? this.p1data.Position : 0);

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
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.EmitterRulePointer);
            writer.Write(this.p1);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (EmitterRule != null) list.Add(EmitterRule);
            if (p1data != null) list.Add(p1data);
            return list.ToArray();
        }
    }


    // pgBase
    // pgBaseRefCounted
    // ptxEffectRule
    [TC(typeof(EXP))] public class ParticleEffectRule : ResourceSystemBlock
    {
        public override long BlockLength => 0x3C0;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000001
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x40866666
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ulong NamePointer { get; set; }
        public uint Unknown_28h { get; set; } // 0x50000000 -> ".?AVptxFxList@rage@@" pointer to itself
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; }
        public uint Unknown_34h { get; set; } // 0x00000001
        public ulong EventEmittersPointer { get; set; }
        public ushort EventEmittersCount1 { get; set; }
        public ushort EventEmittersCount2 { get; set; }
        public uint Unknown_44h { get; set; }
        public ulong p4 { get; set; }
        public uint Unknown_50h { get; set; }
        public uint Unknown_54h { get; set; }
        public uint Unknown_58h { get; set; } // 0x00000000
        public uint Unknown_5Ch { get; set; } // 0x00000000
        public uint Unknown_60h { get; set; } // 0x00000000
        public uint Unknown_64h { get; set; } // 0x00000000
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; }
        public uint Unknown_70h { get; set; }
        public uint Unknown_74h { get; set; }
        public uint Unknown_78h { get; set; }
        public uint Unknown_7Ch { get; set; }
        public uint Unknown_80h { get; set; }
        public uint Unknown_84h { get; set; }
        public uint Unknown_88h { get; set; }
        public uint Unknown_8Ch { get; set; }
        public uint Unknown_90h { get; set; }
        public uint Unknown_94h { get; set; }
        public uint Unknown_98h { get; set; }
        public uint Unknown_9Ch { get; set; }
        public uint Unknown_A0h { get; set; }
        public uint Unknown_A4h { get; set; }
        public uint Unknown_A8h { get; set; }
        public uint Unknown_ACh { get; set; }
        public uint Unknown_B0h { get; set; }
        public uint Unknown_B4h { get; set; }
        public uint Unknown_B8h { get; set; }
        public uint Unknown_BCh { get; set; }
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public ParticleKeyframeProp KeyframeProp4 { get; set; }
        public ulong KeyframePropsPointer { get; set; }
        public ushort KeyframePropsCount1 { get; set; }
        public ushort KeyframePropsCount2 { get; set; }
        public uint Unknown_39Ch { get; set; } // 0x00000000
        public uint Unknown_3A0h { get; set; }
        public uint Unknown_3A4h { get; set; } // 0x00000000
        public uint Unknown_3A8h { get; set; } // 0x42C80000
        public uint Unknown_3ACh { get; set; } // 0x00000000
        public uint Unknown_3B0h { get; set; } // 0x00000000
        public uint Unknown_3B4h { get; set; } // 0x00000000
        public uint Unknown_3B8h { get; set; } // 0x00000000
        public uint Unknown_3BCh { get; set; } // 0x00000000

        // reference data
        public string_r Name { get; set; }
        public ResourcePointerArray64<ParticleEventEmitter> EventEmitters { get; set; }
        public ParticleUnknown_P_004 p4data { get; set; }
        public ResourcePointerArray64<ParticleKeyframeProp> KeyframeProps { get; set; }


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
            this.NamePointer = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.EventEmittersPointer = reader.ReadUInt64();
            this.EventEmittersCount1 = reader.ReadUInt16();
            this.EventEmittersCount2 = reader.ReadUInt16();
            this.Unknown_44h = reader.ReadUInt32();
            this.p4 = reader.ReadUInt64();
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
            this.Unknown_80h = reader.ReadUInt32();
            this.Unknown_84h = reader.ReadUInt32();
            this.Unknown_88h = reader.ReadUInt32();
            this.Unknown_8Ch = reader.ReadUInt32();
            this.Unknown_90h = reader.ReadUInt32();
            this.Unknown_94h = reader.ReadUInt32();
            this.Unknown_98h = reader.ReadUInt32();
            this.Unknown_9Ch = reader.ReadUInt32();
            this.Unknown_A0h = reader.ReadUInt32();
            this.Unknown_A4h = reader.ReadUInt32();
            this.Unknown_A8h = reader.ReadUInt32();
            this.Unknown_ACh = reader.ReadUInt32();
            this.Unknown_B0h = reader.ReadUInt32();
            this.Unknown_B4h = reader.ReadUInt32();
            this.Unknown_B8h = reader.ReadUInt32();
            this.Unknown_BCh = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp4 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframePropsPointer = reader.ReadUInt64();
            this.KeyframePropsCount1 = reader.ReadUInt16();
            this.KeyframePropsCount2 = reader.ReadUInt16();
            this.Unknown_39Ch = reader.ReadUInt32();
            this.Unknown_3A0h = reader.ReadUInt32();
            this.Unknown_3A4h = reader.ReadUInt32();
            this.Unknown_3A8h = reader.ReadUInt32();
            this.Unknown_3ACh = reader.ReadUInt32();
            this.Unknown_3B0h = reader.ReadUInt32();
            this.Unknown_3B4h = reader.ReadUInt32();
            this.Unknown_3B8h = reader.ReadUInt32();
            this.Unknown_3BCh = reader.ReadUInt32();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(
                this.NamePointer // offset
            );
            this.EventEmitters = reader.ReadBlockAt<ResourcePointerArray64<ParticleEventEmitter>>(
                this.EventEmittersPointer, // offset
                this.EventEmittersCount1
            );
            this.p4data = reader.ReadBlockAt<ParticleUnknown_P_004>(
                this.p4 // offset
            );
            this.KeyframeProps = reader.ReadBlockAt<ResourcePointerArray64<ParticleKeyframeProp>>(
                this.KeyframePropsPointer, // offset
                this.KeyframePropsCount2
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            //// update structure data
            //this.NamePointer = (ulong)(this.Name != null ? this.Name.Position : 0);
            //this.EventEmittersPointer = (ulong)(this.EventEmitters != null ? this.EventEmitters.Position : 0);
            ////this.c3b = (ushort)(this.p3data != null ? this.p3data.Count : 0);
            //this.p4 = (ulong)(this.p4data != null ? this.p4data.Position : 0);
            //this.KeyframePropsPointer = (ulong)(this.KeyframeProps != null ? this.KeyframeProps.Position : 0);
            ////this.refcnt2 = (ushort)(this.refs != null ? this.refs.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.EventEmittersPointer);
            writer.Write(this.EventEmittersCount1);
            writer.Write(this.EventEmittersCount2);
            writer.Write(this.Unknown_44h);
            writer.Write(this.p4);
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
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_84h);
            writer.Write(this.Unknown_88h);
            writer.Write(this.Unknown_8Ch);
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
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.WriteBlock(this.KeyframeProp4);
            writer.Write(this.KeyframePropsPointer);
            writer.Write(this.KeyframePropsCount1);
            writer.Write(this.KeyframePropsCount2);
            writer.Write(this.Unknown_39Ch);
            writer.Write(this.Unknown_3A0h);
            writer.Write(this.Unknown_3A4h);
            writer.Write(this.Unknown_3A8h);
            writer.Write(this.Unknown_3ACh);
            writer.Write(this.Unknown_3B0h);
            writer.Write(this.Unknown_3B4h);
            writer.Write(this.Unknown_3B8h);
            writer.Write(this.Unknown_3BCh);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            //if (Name != null) list.Add(Name);
            //if (EventEmitters != null) list.Add(EventEmitters);
            //if (p4data != null) list.Add(p4data);
            //if (KeyframeProps != null) list.Add(KeyframeProps);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(192, KeyframeProp0),
                new Tuple<long, IResourceBlock>(336, KeyframeProp1),
                new Tuple<long, IResourceBlock>(480, KeyframeProp2),
                new Tuple<long, IResourceBlock>(624, KeyframeProp3),
                new Tuple<long, IResourceBlock>(768, KeyframeProp4)
            };
        }
    }


    // datBase
    // ptxKeyframeProp
    [TC(typeof(EXP))] public class ParticleKeyframeProp : ResourceSystemBlock
    {
        public override long BlockLength => 0x90;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
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
        public uint Unknown_50h { get; set; } // 0x00000000
        public uint Unknown_54h { get; set; } // 0x00000000
        public uint Unknown_58h { get; set; } // 0x00000000
        public uint Unknown_5Ch { get; set; } // 0x00000000
        public uint Unknown_60h { get; set; } // 0x00000000
        public uint Unknown_64h { get; set; } // 0x00000000
        public uint Unknown_68h { get; set; }
        public uint Unknown_6Ch { get; set; }
        public ResourceSimpleList64<ParticleUnknown_P_011> Unknown_70h { get; set; }
        public uint Unknown_80h { get; set; } // 0x00000000
        public uint Unknown_84h { get; set; } // 0x00000000
        public uint Unknown_88h { get; set; } // 0x00000000
        public uint Unknown_8Ch { get; set; } // 0x00000000


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
            this.Unknown_70h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_011>>();
            this.Unknown_80h = reader.ReadUInt32();
            this.Unknown_84h = reader.ReadUInt32();
            this.Unknown_88h = reader.ReadUInt32();
            this.Unknown_8Ch = reader.ReadUInt32();
        }

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
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_84h);
            writer.Write(this.Unknown_88h);
            writer.Write(this.Unknown_8Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x70, Unknown_70h)
            };
        }
    }



    [TC(typeof(EXP))] public class ParticleUnknown_P_011 : ResourceSystemBlock
    {
        public override long BlockLength => 0x20;

        // structure data
        public float Unknown_0h { get; set; }
        public float Unknown_4h { get; set; }
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public float Unknown_10h { get; set; }
        public float Unknown_14h { get; set; }
        public float Unknown_18h { get; set; }
        public float Unknown_1Ch { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadSingle();
            this.Unknown_4h = reader.ReadSingle();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadSingle();
            this.Unknown_14h = reader.ReadSingle();
            this.Unknown_18h = reader.ReadSingle();
            this.Unknown_1Ch = reader.ReadSingle();
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
        }
    }


    // ptxEvent
    // ptxEventEmitter
    [TC(typeof(EXP))] public class ParticleEventEmitter : ResourceSystemBlock
    {
        public override long BlockLength => 0x70;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; }
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public ulong p1 { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ulong p2 { get; set; }
        public ulong p3 { get; set; }
        public ulong p4 { get; set; }
        public ulong p5 { get; set; }
        public uint Unknown_50h { get; set; }
        public uint Unknown_54h { get; set; }
        public uint Unknown_58h { get; set; }
        public uint Unknown_5Ch { get; set; }
        public uint Unknown_60h { get; set; }
        public uint Unknown_64h { get; set; }
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000

        // reference data
        public ParticleUnknown_P_005 p1data { get; set; }
        public string_r p2data { get; set; }
        public string_r p3data { get; set; }
        public ParticleEmitterRule EmitterRule { get; set; }
        public ParticleRule ParticleRule { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.p1 = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.p2 = reader.ReadUInt64();
            this.p3 = reader.ReadUInt64();
            this.p4 = reader.ReadUInt64();
            this.p5 = reader.ReadUInt64();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.Unknown_58h = reader.ReadUInt32();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.Unknown_60h = reader.ReadUInt32();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();

            // read reference data
            this.p1data = reader.ReadBlockAt<ParticleUnknown_P_005>(
                this.p1 // offset
            );
            this.p2data = reader.ReadBlockAt<string_r>(
                this.p2 // offset
            );
            this.p3data = reader.ReadBlockAt<string_r>(
                this.p3 // offset
            );
            this.EmitterRule = reader.ReadBlockAt<ParticleEmitterRule>(
                this.p4 // offset
            );
            this.ParticleRule = reader.ReadBlockAt<ParticleRule>(
                this.p5 // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            //this.p1 = (ulong)(this.p1data != null ? this.p1data.Position : 0);
            //this.p2 = (ulong)(this.p2data != null ? this.p2data.Position : 0);
            //this.p3 = (ulong)(this.p3data != null ? this.p3data.Position : 0);
            //this.p4 = (ulong)(this.EmitterRule != null ? this.EmitterRule.Position : 0);
            //this.p5 = (ulong)(this.ParticleRule != null ? this.ParticleRule.Position : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.p1);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.p2);
            writer.Write(this.p3);
            writer.Write(this.p4);
            writer.Write(this.p5);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            //if (p1data != null) list.Add(p1data);
            //if (p2data != null) list.Add(p2data);
            //if (p3data != null) list.Add(p3data);
            //if (EmitterRule != null) list.Add(EmitterRule);
            //if (ParticleRule != null) list.Add(ParticleRule);
            return list.ToArray();
        }
    }


    [TC(typeof(EXP))] public class ParticleUnknown_P_005 : ResourceSystemBlock
    {
        public override long BlockLength => 0x40;

        // structure data
        public ResourceSimpleList64<ParticleUnknown_P_008> Unknown_0h { get; set; }
        public ResourceSimpleList64<ParticleUnknown_P_003> Unknown_10h { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000001
        public uint Unknown_24h { get; set; } // 0x00000000
        public ResourceSimpleList64<ParticleUnknown_P_010> Unknown_28h { get; set; }
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_008>>();
            this.Unknown_10h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_003>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_010>>();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(this.Unknown_0h);
            writer.WriteBlock(this.Unknown_10h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.WriteBlock(this.Unknown_28h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, Unknown_0h),
                new Tuple<long, IResourceBlock>(0x10, Unknown_10h),
                new Tuple<long, IResourceBlock>(0x28, Unknown_28h)
            };
        }
    }

    [TC(typeof(EXP))] public class ParticleUnknown_P_008 : ResourceSystemBlock
    {
        public override long BlockLength => 24;

        // structure data
        public ulong p1 { get; set; }
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000

        // reference data
        public string_r p1data { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.p1 = reader.ReadUInt64();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();

            // read reference data
            this.p1data = reader.ReadBlockAt<string_r>(
                this.p1 // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            //this.p1 = (ulong)(this.p1data != null ? this.p1data.Position : 0);

            // write structure data
            writer.Write(this.p1);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (p1data != null) list.Add(p1data);
            return list.ToArray();
        }
    }

    [TC(typeof(EXP))] public class ParticleUnknown_P_003 : ResourceSystemBlock
    {
        public override long BlockLength => 24;

        // structure data
        public ResourceSimpleList64<ParticleUnknown_P_006> Unknown_0h { get; set; }
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_006>>();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(this.Unknown_0h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, Unknown_0h)
            };
        }
    }

    [TC(typeof(EXP))] public class ParticleUnknown_P_006 : ResourceSystemBlock
    {
        public override long BlockLength => 0x30;

        // structure data
        public ResourceSimpleList64<ParticleUnknown_P_009> Unknown_0h { get; set; }
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_009>>();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(this.Unknown_0h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, Unknown_0h)
            };
        }
    }

    [TC(typeof(EXP))] public class ParticleUnknown_P_009 : ResourceSystemBlock
    {
        public override long BlockLength => 0x20;

        // structure data
        public uint Unknown_0h { get; set; }
        public uint Unknown_4h { get; set; }
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }

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
        }

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
        }
    }

    [TC(typeof(EXP))] public class ParticleUnknown_P_010 : ResourceSystemBlock
    {
        public override long BlockLength => 0x10;

        // structure data
        public uint Unknown_0h { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000000
        public ulong Unknown_8h_Pointer { get; set; }

        // reference data
        public ParticleUnknown_P_003 Unknown_8h_Data { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h_Pointer = reader.ReadUInt64();

            // read reference data
            this.Unknown_8h_Data = reader.ReadBlockAt<ParticleUnknown_P_003>(
                this.Unknown_8h_Pointer // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            //this.Unknown_8h_Pointer = (ulong)(this.Unknown_8h_Data != null ? this.Unknown_8h_Data.Position : 0);

            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h_Pointer);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Unknown_8h_Data != null) list.Add(Unknown_8h_Data);
            return list.ToArray();
        }
    }



    // pgBase
    // pgBaseRefCounted
    // ptxEmitterRule
    [TC(typeof(EXP))] public class ParticleEmitterRule : ResourceSystemBlock
    {
        public override long BlockLength => 0x630;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x40833333 
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ulong NamePointer { get; set; }
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; } // 0x00000000
        public ulong p2 { get; set; }
        public uint Unknown_40h { get; set; } // 0x00000000
        public uint Unknown_44h { get; set; } // 0x00000000
        public ulong p3 { get; set; }
        public uint Unknown_50h { get; set; } // 0x00000000
        public uint Unknown_54h { get; set; } // 0x00000000
        public ulong p4 { get; set; }
        public uint Unknown_60h { get; set; } // 0x00000000
        public uint Unknown_64h { get; set; } // 0x00000000
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public ParticleKeyframeProp KeyframeProp4 { get; set; }
        public ParticleKeyframeProp KeyframeProp5 { get; set; }
        public ParticleKeyframeProp KeyframeProp6 { get; set; }
        public ParticleKeyframeProp KeyframeProp7 { get; set; }
        public ParticleKeyframeProp KeyframeProp8 { get; set; }
        public ParticleKeyframeProp KeyframeProp9 { get; set; }
        public ulong KeyframePropsPointer { get; set; }
        public ushort KeyframePropsCount1 { get; set; } // 10
        public ushort KeyframePropsCount2 { get; set; } // 10
        public uint Unknown_624h { get; set; } // 0x00000000
        public uint Unknown_628h { get; set; }
        public uint Unknown_62Ch { get; set; } // 0x00000000

        // reference data
        public string_r Name { get; set; }
        public ParticleDomain p2data { get; set; }
        public ParticleDomain p3data { get; set; }
        public ParticleDomain p4data { get; set; }
        public ResourcePointerArray64<ParticleKeyframeProp> KeyframeProps { get; set; }

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
            this.NamePointer = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.p2 = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.p3 = reader.ReadUInt64();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.p4 = reader.ReadUInt64();
            this.Unknown_60h = reader.ReadUInt32();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp4 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp5 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp6 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp7 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp8 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp9 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframePropsPointer = reader.ReadUInt64();
            this.KeyframePropsCount1 = reader.ReadUInt16();
            this.KeyframePropsCount2 = reader.ReadUInt16();
            this.Unknown_624h = reader.ReadUInt32();
            this.Unknown_628h = reader.ReadUInt32();
            this.Unknown_62Ch = reader.ReadUInt32();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(
                this.NamePointer // offset
            );
            this.p2data = reader.ReadBlockAt<ParticleDomain>(
                this.p2 // offset
            );
            this.p3data = reader.ReadBlockAt<ParticleDomain>(
                this.p3 // offset
            );
            this.p4data = reader.ReadBlockAt<ParticleDomain>(
                this.p4 // offset
            );
            this.KeyframeProps = reader.ReadBlockAt<ResourcePointerArray64<ParticleKeyframeProp>>(
                this.KeyframePropsPointer, // offset
                this.KeyframePropsCount2
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            //this.NamePointer = (ulong)(this.Name != null ? this.Name.Position : 0);
            //this.p2 = (ulong)(this.p2data != null ? this.p2data.Position : 0);
            //this.p3 = (ulong)(this.p3data != null ? this.p3data.Position : 0);
            //this.p4 = (ulong)(this.p4data != null ? this.p4data.Position : 0);
            //this.KeyframePropsPointer = (ulong)(this.KeyframeProps != null ? this.KeyframeProps.Position : 0);
            ////this.refcnt2 = (ushort)(this.refs != null ? this.refs.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.p2);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.p3);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.p4);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.WriteBlock(this.KeyframeProp4);
            writer.WriteBlock(this.KeyframeProp5);
            writer.WriteBlock(this.KeyframeProp6);
            writer.WriteBlock(this.KeyframeProp7);
            writer.WriteBlock(this.KeyframeProp8);
            writer.WriteBlock(this.KeyframeProp9);
            writer.Write(this.KeyframePropsPointer);
            writer.Write(this.KeyframePropsCount1);
            writer.Write(this.KeyframePropsCount2);
            writer.Write(this.Unknown_624h);
            writer.Write(this.Unknown_628h);
            writer.Write(this.Unknown_62Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            //if (Name != null) list.Add(Name);
            //if (p2data != null) list.Add(p2data);
            //if (p3data != null) list.Add(p3data);
            //if (p4data != null) list.Add(p4data);
            //if (KeyframeProps != null) list.Add(KeyframeProps);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(120, KeyframeProp0),
                new Tuple<long, IResourceBlock>(264, KeyframeProp1),
                new Tuple<long, IResourceBlock>(408, KeyframeProp2),
                new Tuple<long, IResourceBlock>(552, KeyframeProp3),
                new Tuple<long, IResourceBlock>(696, KeyframeProp4),
                new Tuple<long, IResourceBlock>(840, KeyframeProp5),
                new Tuple<long, IResourceBlock>(984, KeyframeProp6),
                new Tuple<long, IResourceBlock>(1128, KeyframeProp7),
                new Tuple<long, IResourceBlock>(1272, KeyframeProp8),
                new Tuple<long, IResourceBlock>(1416, KeyframeProp9),
            };
        }
    }



    // datBase
    // ptxDomain
    [TC(typeof(EXP))] public class ParticleDomain : ResourceSystemBlock, IResourceXXSystemBlock
    {
        public override long BlockLength => 0x280;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; }
        public byte Unknown_Ch { get; set; }
        public byte Unknown_Dh { get; set; }
        public ushort Unknown_Eh { get; set; }
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public uint Unknown_258h { get; set; }
        public uint Unknown_25Ch { get; set; } // 0x00000000
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_270h { get; set; } // 0x00000000
        public uint Unknown_274h { get; set; } // 0x00000000
        public uint Unknown_278h { get; set; } // 0x00000000
        public uint Unknown_27Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadByte();
            this.Unknown_Dh = reader.ReadByte();
            this.Unknown_Eh = reader.ReadUInt16();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_258h = reader.ReadUInt32();
            this.Unknown_25Ch = reader.ReadUInt32();
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_270h = reader.ReadUInt32();
            this.Unknown_274h = reader.ReadUInt32();
            this.Unknown_278h = reader.ReadUInt32();
            this.Unknown_27Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_Dh);
            writer.Write(this.Unknown_Eh);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.Write(this.Unknown_258h);
            writer.Write(this.Unknown_25Ch);
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_270h);
            writer.Write(this.Unknown_274h);
            writer.Write(this.Unknown_278h);
            writer.Write(this.Unknown_27Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(24, KeyframeProp0),
                new Tuple<long, IResourceBlock>(168, KeyframeProp1),
                new Tuple<long, IResourceBlock>(312, KeyframeProp2),
                new Tuple<long, IResourceBlock>(456, KeyframeProp3),
                new Tuple<long, IResourceBlock>(0x260, KeyframeProps)
            };
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 12;
            byte type = reader.ReadByte();
            reader.Position -= 13;

            switch (type)
            {
                case 0: return new ParticleDomainBox();
                case 1: return new ParticleDomainSphere();
                case 2: return new ParticleDomainCylinder();
                case 3: return new ParticleDomainAttractor();
                default: return null;// throw new Exception("Unknown domain type");
            }
        }

    }

    // ptxDomainBox
    [TC(typeof(EXP))] public class ParticleDomainBox : ParticleDomain
    { }
    
    // ptxDomainSphere 
    [TC(typeof(EXP))] public class ParticleDomainSphere : ParticleDomain
    { }

    // ptxDomainCylinder   
    [TC(typeof(EXP))] public class ParticleDomainCylinder : ParticleDomain
    { }

    // ptxDomainAttractor
    [TC(typeof(EXP))] public class ParticleDomainAttractor : ParticleDomain
    { }






    [TC(typeof(EXP))] public class ParticleUnknown_P_004 : ResourceSystemBlock
    {
        public override long BlockLength => 0x40;

        // structure data
        public ResourceSimpleList64<ParticleUnknown_P_002> Unknown_0h { get; set; }
        public ResourceSimpleList64<ParticleUnknown_P_003> Unknown_10h { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000001
        public uint Unknown_24h { get; set; } // 0x00000000
        public ResourceSimpleList64<ParticleUnknown_P_007> Unknown_28h { get; set; }
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_002>>();
            this.Unknown_10h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_003>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_007>>();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(this.Unknown_0h);
            writer.WriteBlock(this.Unknown_10h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.WriteBlock(this.Unknown_28h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, Unknown_0h),
                new Tuple<long, IResourceBlock>(0x10, Unknown_10h),
                new Tuple<long, IResourceBlock>(0x28, Unknown_28h)
            };
        }
    }

    [TC(typeof(EXP))] public class ParticleUnknown_P_002 : ResourceSystemBlock
    {
        public override long BlockLength => 24;

        // structure data
        public ulong p1 { get; set; }
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000

        // reference data
        public string_r p1data { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.p1 = reader.ReadUInt64();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();

            // read reference data
            this.p1data = reader.ReadBlockAt<string_r>(
                this.p1 // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            ////this.p1 = (ulong)(this.p1data != null ? this.p1data.Position : 0);

            // write structure data
            writer.Write(this.p1);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (p1data != null) list.Add(p1data);
            return list.ToArray();
        }
    }

    [TC(typeof(EXP))] public class ParticleUnknown_P_007 : ResourceSystemBlock
    {
        public override long BlockLength => 0x10;

        // structure data
        public uint Unknown_0h { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000000
        public ulong p1 { get; set; }

        // reference data
        public ParticleUnknown_P_003 p1data { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.p1 = reader.ReadUInt64();

            // read reference data
            this.p1data = reader.ReadBlockAt<ParticleUnknown_P_003>(
                this.p1 // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            ////this.p1 = (ulong)(this.p1data != null ? this.p1data.Position : 0);

            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.p1);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (p1data != null) list.Add(p1data);
            return list.ToArray();
        }
    }










    public enum ParticleBehaviourType : uint
    {
        Age = 0xF5B33BAA,
        Acceleration = 0xD63D9F1B,
        Velocity = 0x6C0719BC,
        Rotation = 0x1EE64552,
        Size = 0x38B60240,
        Dampening = 0x052B1293,
        MatrixWeight = 0x64E5D702,
        Collision = 0x928A1C45,
        AnimateTexture = 0xECA84C1E,
        Colour = 0x164AEA72,
        Sprite = 0x68FA73F5,
        Wind = 0x38B63978,
        Light = 0x0544C710,
        Model = 0x6232E25A,
        Decal = 0x8F3B6036,
        ZCull = 0xA35C721F,
        Noise = 0xB77FED19,
        Attractor = 0x25AC9437,
        Trail = 0xC57377F8,
        FogVolume = 0xA05DA63E,
        River = 0xD4594BEF,
        DecalPool = 0xA2D6DC3F,
        Liquid = 0xDF229542
    }

    // ptxBehaviour
    [TC(typeof(EXP))] public class ParticleBehaviour : ResourceSystemBlock, IResourceXXSystemBlock
    {
        public override long BlockLength => 0x10;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Type { get; set; }
        public uint Unknown_Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Type = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Type);
            writer.Write(this.Unknown_Ch);
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {

            reader.Position += 8;
            ParticleBehaviourType type = (ParticleBehaviourType)reader.ReadUInt32();
            reader.Position -= 12;

            switch (type)
            {
                case ParticleBehaviourType.Age: return new ParticleBehaviourAge();
                case ParticleBehaviourType.Acceleration: return new ParticleBehaviourAcceleration();
                case ParticleBehaviourType.Velocity: return new ParticleBehaviourVelocity();
                case ParticleBehaviourType.Rotation: return new ParticleBehaviourRotation();
                case ParticleBehaviourType.Size: return new ParticleBehaviourSize();
                case ParticleBehaviourType.Dampening: return new ParticleBehaviourDampening();
                case ParticleBehaviourType.MatrixWeight: return new ParticleBehaviourMatrixWeight();
                case ParticleBehaviourType.Collision: return new ParticleBehaviourCollision();
                case ParticleBehaviourType.AnimateTexture: return new ParticleBehaviourAnimateTexture();
                case ParticleBehaviourType.Colour: return new ParticleBehaviourColour();
                case ParticleBehaviourType.Sprite: return new ParticleBehaviourSprite();
                case ParticleBehaviourType.Wind: return new ParticleBehaviourWind();
                case ParticleBehaviourType.Light: return new ParticleBehaviourLight();
                case ParticleBehaviourType.Model: return new ParticleBehaviourModel();
                case ParticleBehaviourType.Decal: return new ParticleBehaviourDecal();
                case ParticleBehaviourType.ZCull: return new ParticleBehaviourZCull();
                case ParticleBehaviourType.Noise: return new ParticleBehaviourNoise();
                case ParticleBehaviourType.Attractor: return new ParticleBehaviourAttractor();
                case ParticleBehaviourType.Trail: return new ParticleBehaviourTrail();
                case ParticleBehaviourType.FogVolume: return new ParticleBehaviourFogVolume();
                case ParticleBehaviourType.River: return new ParticleBehaviourRiver();
                case ParticleBehaviourType.DecalPool: return new ParticleBehaviourDecalPool();
                case ParticleBehaviourType.Liquid: return new ParticleBehaviourLiquid();
                default: return null;// throw new Exception("Unknown behaviour type");
            }
        }
    }

    // ptxu_Age
    [TC(typeof(EXP))] public class ParticleBehaviourAge : ParticleBehaviour
    {
        public override long BlockLength => 0x30;

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

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

    // ptxu_Acceleration
    [TC(typeof(EXP))] public class ParticleBehaviourAcceleration : ParticleBehaviour
    {
        public override long BlockLength => 0x170;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public uint Unknown_150h { get; set; } // 0x00000000
        public uint Unknown_154h { get; set; } // 0x00000000
        public uint Unknown_158h { get; set; }
        public uint Unknown_15Ch { get; set; }
        public uint Unknown_160h { get; set; } // 0x00000000
        public uint Unknown_164h { get; set; } // 0x00000000
        public uint Unknown_168h { get; set; } // 0x00000000
        public uint Unknown_16Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt32();
            this.Unknown_154h = reader.ReadUInt32();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt32();
            this.Unknown_164h = reader.ReadUInt32();
            this.Unknown_168h = reader.ReadUInt32();
            this.Unknown_16Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_154h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_164h);
            writer.Write(this.Unknown_168h);
            writer.Write(this.Unknown_16Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x10, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0),
                new Tuple<long, IResourceBlock>(192, KeyframeProp1)
            };
        }
    }

    // ptxu_Velocity
    [TC(typeof(EXP))] public class ParticleBehaviourVelocity : ParticleBehaviour
    {
        public override long BlockLength => 0x30;

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

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

    // ptxu_Rotation
    [TC(typeof(EXP))] public class ParticleBehaviourRotation : ParticleBehaviour
    {
        public override long BlockLength => 0x280;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public uint Unknown_270h { get; set; }
        public uint Unknown_274h { get; set; }
        public uint Unknown_278h { get; set; }
        public uint Unknown_27Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_270h = reader.ReadUInt32();
            this.Unknown_274h = reader.ReadUInt32();
            this.Unknown_278h = reader.ReadUInt32();
            this.Unknown_27Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.Write(this.Unknown_270h);
            writer.Write(this.Unknown_274h);
            writer.Write(this.Unknown_278h);
            writer.Write(this.Unknown_27Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0),
                new Tuple<long, IResourceBlock>(192, KeyframeProp1),
                new Tuple<long, IResourceBlock>(336, KeyframeProp2),
                new Tuple<long, IResourceBlock>(480, KeyframeProp3)
            };
        }
    }

    // ptxu_Size
    [TC(typeof(EXP))] public class ParticleBehaviourSize : ParticleBehaviour
    {
        public override long BlockLength => 0x280;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public uint Unknown_270h { get; set; }
        public uint Unknown_274h { get; set; }
        public uint Unknown_278h { get; set; } // 0x00000000
        public uint Unknown_27Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_270h = reader.ReadUInt32();
            this.Unknown_274h = reader.ReadUInt32();
            this.Unknown_278h = reader.ReadUInt32();
            this.Unknown_27Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.Write(this.Unknown_270h);
            writer.Write(this.Unknown_274h);
            writer.Write(this.Unknown_278h);
            writer.Write(this.Unknown_27Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0),
                new Tuple<long, IResourceBlock>(192, KeyframeProp1),
                new Tuple<long, IResourceBlock>(336, KeyframeProp2),
                new Tuple<long, IResourceBlock>(480, KeyframeProp3)
            };
        }
    }

    // ptxu_Dampening
    [TC(typeof(EXP))] public class ParticleBehaviourDampening : ParticleBehaviour
    {
        public override long BlockLength => 0x170;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public uint Unknown_150h { get; set; } // 0x00000000
        public uint Unknown_154h { get; set; } // 0x00000000
        public uint Unknown_158h { get; set; }
        public uint Unknown_15Ch { get; set; } // 0x00000000
        public uint Unknown_160h { get; set; } // 0x00000000
        public uint Unknown_164h { get; set; } // 0x00000000
        public uint Unknown_168h { get; set; } // 0x00000000
        public uint Unknown_16Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt32();
            this.Unknown_154h = reader.ReadUInt32();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt32();
            this.Unknown_164h = reader.ReadUInt32();
            this.Unknown_168h = reader.ReadUInt32();
            this.Unknown_16Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_154h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_164h);
            writer.Write(this.Unknown_168h);
            writer.Write(this.Unknown_16Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0),
                new Tuple<long, IResourceBlock>(192, KeyframeProp1)
            };
        }
    }

    // ptxu_MatrixWeight
    [TC(typeof(EXP))] public class ParticleBehaviourMatrixWeight : ParticleBehaviour
    {
        public override long BlockLength => 0xD0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public uint Unknown_C0h { get; set; }
        public uint Unknown_C4h { get; set; } // 0x00000000
        public uint Unknown_C8h { get; set; } // 0x00000000
        public uint Unknown_CCh { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_C0h = reader.ReadUInt32();
            this.Unknown_C4h = reader.ReadUInt32();
            this.Unknown_C8h = reader.ReadUInt32();
            this.Unknown_CCh = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C4h);
            writer.Write(this.Unknown_C8h);
            writer.Write(this.Unknown_CCh);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0)
            };
        }
    }

    // ptxu_Collision
    [TC(typeof(EXP))] public class ParticleBehaviourCollision : ParticleBehaviour
    {
        public override long BlockLength => 0x170;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public uint Unknown_150h { get; set; }
        public uint Unknown_154h { get; set; }
        public uint Unknown_158h { get; set; }
        public uint Unknown_15Ch { get; set; }
        public uint Unknown_160h { get; set; } // 0x00000000
        public uint Unknown_164h { get; set; } // 0x00000000
        public uint Unknown_168h { get; set; } // 0x00000000
        public uint Unknown_16Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt32();
            this.Unknown_154h = reader.ReadUInt32();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt32();
            this.Unknown_164h = reader.ReadUInt32();
            this.Unknown_168h = reader.ReadUInt32();
            this.Unknown_16Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_154h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_164h);
            writer.Write(this.Unknown_168h);
            writer.Write(this.Unknown_16Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0),
                new Tuple<long, IResourceBlock>(192, KeyframeProp1)
            };
        }
    }

    // ptxu_AnimateTexture
    [TC(typeof(EXP))] public class ParticleBehaviourAnimateTexture : ParticleBehaviour
    {
        public override long BlockLength => 0xD0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public uint Unknown_C0h { get; set; }
        public uint Unknown_C4h { get; set; }
        public uint Unknown_C8h { get; set; }
        public uint Unknown_CCh { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_C0h = reader.ReadUInt32();
            this.Unknown_C4h = reader.ReadUInt32();
            this.Unknown_C8h = reader.ReadUInt32();
            this.Unknown_CCh = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C4h);
            writer.Write(this.Unknown_C8h);
            writer.Write(this.Unknown_CCh);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0)
            };
        }
    }

    // ptxu_Colour
    [TC(typeof(EXP))] public class ParticleBehaviourColour : ParticleBehaviour
    {
        public override long BlockLength => 0x1F0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public uint Unknown_1E0h { get; set; }
        public uint Unknown_1E4h { get; set; }
        public uint Unknown_1E8h { get; set; } // 0x00000000
        public uint Unknown_1ECh { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_1E0h = reader.ReadUInt32();
            this.Unknown_1E4h = reader.ReadUInt32();
            this.Unknown_1E8h = reader.ReadUInt32();
            this.Unknown_1ECh = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.Write(this.Unknown_1E0h);
            writer.Write(this.Unknown_1E4h);
            writer.Write(this.Unknown_1E8h);
            writer.Write(this.Unknown_1ECh);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0),
                new Tuple<long, IResourceBlock>(192, KeyframeProp1),
                new Tuple<long, IResourceBlock>(336, KeyframeProp2)
            };
        }
    }

    // ptxd_Sprite
    [TC(typeof(EXP))] public class ParticleBehaviourSprite : ParticleBehaviour
    {
        public override long BlockLength => 0x70;

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
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
        public uint Unknown_50h { get; set; }
        public uint Unknown_54h { get; set; }
        public uint Unknown_58h { get; set; }
        public uint Unknown_5Ch { get; set; }
        public uint Unknown_60h { get; set; }
        public uint Unknown_64h { get; set; } // 0x00000000
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000

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
            this.Unknown_60h = reader.ReadUInt32();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();
        }

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
        }
    }

    // ptxu_Wind
    [TC(typeof(EXP))] public class ParticleBehaviourWind : ParticleBehaviour
    {
        public override long BlockLength => 0xF0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public uint Unknown_C0h { get; set; } // 0x00000000
        public uint Unknown_C4h { get; set; } // 0x00000000
        public uint Unknown_C8h { get; set; } // 0x00000000
        public uint Unknown_CCh { get; set; } // 0x00000000
        public uint Unknown_D0h { get; set; }
        public uint Unknown_D4h { get; set; }
        public uint Unknown_D8h { get; set; }
        public uint Unknown_DCh { get; set; }
        public uint Unknown_E0h { get; set; }
        public uint Unknown_E4h { get; set; } // 0x00000000
        public uint Unknown_E8h { get; set; } // 0x00000000
        public uint Unknown_ECh { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
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
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
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
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0)
            };
        }
    }

    // ptxu_Light
    [TC(typeof(EXP))] public class ParticleBehaviourLight : ParticleBehaviour
    {
        public override long BlockLength => 0x550;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public ParticleKeyframeProp KeyframeProp4 { get; set; }
        public ParticleKeyframeProp KeyframeProp5 { get; set; }
        public ParticleKeyframeProp KeyframeProp6 { get; set; }
        public ParticleKeyframeProp KeyframeProp7 { get; set; }
        public ParticleKeyframeProp KeyframeProp8 { get; set; }
        public uint Unknown_540h { get; set; }
        public uint Unknown_544h { get; set; }
        public uint Unknown_548h { get; set; }
        public uint Unknown_54Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp4 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp5 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp6 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp7 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp8 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_540h = reader.ReadUInt32();
            this.Unknown_544h = reader.ReadUInt32();
            this.Unknown_548h = reader.ReadUInt32();
            this.Unknown_54Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.WriteBlock(this.KeyframeProp4);
            writer.WriteBlock(this.KeyframeProp5);
            writer.WriteBlock(this.KeyframeProp6);
            writer.WriteBlock(this.KeyframeProp7);
            writer.WriteBlock(this.KeyframeProp8);
            writer.Write(this.Unknown_540h);
            writer.Write(this.Unknown_544h);
            writer.Write(this.Unknown_548h);
            writer.Write(this.Unknown_54Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0),
                new Tuple<long, IResourceBlock>(192, KeyframeProp1),
                new Tuple<long, IResourceBlock>(336, KeyframeProp2),
                new Tuple<long, IResourceBlock>(480, KeyframeProp3),
                new Tuple<long, IResourceBlock>(624, KeyframeProp4),
                new Tuple<long, IResourceBlock>(768, KeyframeProp5),
                new Tuple<long, IResourceBlock>(912, KeyframeProp6),
                new Tuple<long, IResourceBlock>(1056, KeyframeProp7),
                new Tuple<long, IResourceBlock>(1200, KeyframeProp8)
            };
        }
    }

    // ptxd_Model
    [TC(typeof(EXP))] public class ParticleBehaviourModel : ParticleBehaviour
    {
        public override long BlockLength => 0x40;

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; }
        public uint Unknown_38h { get; set; }
        public uint Unknown_3Ch { get; set; } // 0x00000000

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
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
        }

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
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }
    }

    // ptxu_Decal
    [TC(typeof(EXP))] public class ParticleBehaviourDecal : ParticleBehaviour
    {
        public override long BlockLength => 0x180;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public uint Unknown_150h { get; set; }
        public uint Unknown_154h { get; set; }
        public uint Unknown_158h { get; set; }
        public uint Unknown_15Ch { get; set; }
        public uint Unknown_160h { get; set; }
        public uint Unknown_164h { get; set; }
        public uint Unknown_168h { get; set; }
        public uint Unknown_16Ch { get; set; }
        public uint Unknown_170h { get; set; }
        public uint Unknown_174h { get; set; } // 0x3E99999A
        public uint Unknown_178h { get; set; } // 0x3F800000
        public uint Unknown_17Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt32();
            this.Unknown_154h = reader.ReadUInt32();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt32();
            this.Unknown_164h = reader.ReadUInt32();
            this.Unknown_168h = reader.ReadUInt32();
            this.Unknown_16Ch = reader.ReadUInt32();
            this.Unknown_170h = reader.ReadUInt32();
            this.Unknown_174h = reader.ReadUInt32();
            this.Unknown_178h = reader.ReadUInt32();
            this.Unknown_17Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
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

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0),
                new Tuple<long, IResourceBlock>(192, KeyframeProp1)
            };
        }
    }

    // ptxu_ZCull
    [TC(typeof(EXP))] public class ParticleBehaviourZCull : ParticleBehaviour
    {
        public override long BlockLength => 0x170;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public uint Unknown_150h { get; set; } // 0x00000000
        public uint Unknown_154h { get; set; } // 0x00000000
        public uint Unknown_158h { get; set; }
        public uint Unknown_15Ch { get; set; }
        public uint Unknown_160h { get; set; } // 0x00000000
        public uint Unknown_164h { get; set; } // 0x00000000
        public uint Unknown_168h { get; set; } // 0x00000000
        public uint Unknown_16Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt32();
            this.Unknown_154h = reader.ReadUInt32();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt32();
            this.Unknown_164h = reader.ReadUInt32();
            this.Unknown_168h = reader.ReadUInt32();
            this.Unknown_16Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_154h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_164h);
            writer.Write(this.Unknown_168h);
            writer.Write(this.Unknown_16Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0),
                new Tuple<long, IResourceBlock>(192, KeyframeProp1)
            };
        }
    }

    // ptxu_Noise
    [TC(typeof(EXP))] public class ParticleBehaviourNoise : ParticleBehaviour
    {
        public override long BlockLength => 0x280;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public uint Unknown_270h { get; set; } // 0x00000000
        public uint Unknown_274h { get; set; }
        public uint Unknown_278h { get; set; } // 0x00000000
        public uint Unknown_27Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_270h = reader.ReadUInt32();
            this.Unknown_274h = reader.ReadUInt32();
            this.Unknown_278h = reader.ReadUInt32();
            this.Unknown_27Ch = reader.ReadUInt32();

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.Write(this.Unknown_270h);
            writer.Write(this.Unknown_274h);
            writer.Write(this.Unknown_278h);
            writer.Write(this.Unknown_27Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0),
                new Tuple<long, IResourceBlock>(192, KeyframeProp1),
                new Tuple<long, IResourceBlock>(336, KeyframeProp2),
                new Tuple<long, IResourceBlock>(480, KeyframeProp3)
            };
        }
    }

    // ptxu_Attractor
    [TC(typeof(EXP))] public class ParticleBehaviourAttractor : ParticleBehaviour
    {
        public override long BlockLength => 0xC0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0)
            };
        }
    }

    // ptxd_Trail
    [TC(typeof(EXP))] public class ParticleBehaviourTrail : ParticleBehaviour
    {
        public override long BlockLength => 0xF0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public uint Unknown_C0h { get; set; } // 0x00000000
        public uint Unknown_C4h { get; set; }
        public uint Unknown_C8h { get; set; }
        public uint Unknown_CCh { get; set; }
        public uint Unknown_D0h { get; set; } // 0x00000000
        public uint Unknown_D4h { get; set; }
        public uint Unknown_D8h { get; set; }
        public uint Unknown_DCh { get; set; }
        public uint Unknown_E0h { get; set; }
        public uint Unknown_E4h { get; set; }
        public uint Unknown_E8h { get; set; } // 0x00000000
        public uint Unknown_ECh { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
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
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
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
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0)
            };
        }
    }

    // ptxu_FogVolume
    [TC(typeof(EXP))] public class ParticleBehaviourFogVolume : ParticleBehaviour
    {
        public override long BlockLength => 0x430;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public ParticleKeyframeProp KeyframeProp4 { get; set; }
        public ParticleKeyframeProp KeyframeProp5 { get; set; }
        public ParticleKeyframeProp KeyframeProp6 { get; set; }
        public uint Unknown_420h { get; set; }
        public uint Unknown_424h { get; set; }
        public uint Unknown_428h { get; set; }
        public uint Unknown_42Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp4 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp5 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp6 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_420h = reader.ReadUInt32();
            this.Unknown_424h = reader.ReadUInt32();
            this.Unknown_428h = reader.ReadUInt32();
            this.Unknown_42Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.WriteBlock(this.KeyframeProp4);
            writer.WriteBlock(this.KeyframeProp5);
            writer.WriteBlock(this.KeyframeProp6);
            writer.Write(this.Unknown_420h);
            writer.Write(this.Unknown_424h);
            writer.Write(this.Unknown_428h);
            writer.Write(this.Unknown_42Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0),
                new Tuple<long, IResourceBlock>(192, KeyframeProp1),
                new Tuple<long, IResourceBlock>(336, KeyframeProp2),
                new Tuple<long, IResourceBlock>(480, KeyframeProp3),
                new Tuple<long, IResourceBlock>(624, KeyframeProp4),
                new Tuple<long, IResourceBlock>(768, KeyframeProp5),
                new Tuple<long, IResourceBlock>(912, KeyframeProp6)
            };
        }
    }

    // ptxu_River
    [TC(typeof(EXP))] public class ParticleBehaviourRiver : ParticleBehaviour
    {
        public override long BlockLength => 0x40;

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x42C80000
        public uint Unknown_34h { get; set; }
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000

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
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
        }

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
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }
    }

    // ptxu_DecalPool
    [TC(typeof(EXP))] public class ParticleBehaviourDecalPool : ParticleBehaviour
    {
        public override long BlockLength => 0x50;

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; }
        public uint Unknown_38h { get; set; }
        public uint Unknown_3Ch { get; set; }
        public uint Unknown_40h { get; set; }
        public uint Unknown_44h { get; set; }
        public uint Unknown_48h { get; set; } // 0x00000000
        public uint Unknown_4Ch { get; set; } // 0x00000000

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
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
        }

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
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
        }
    }

    // ptxu_Liquid
    [TC(typeof(EXP))] public class ParticleBehaviourLiquid : ParticleBehaviour
    {
        public override long BlockLength => 0x50;

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000
        public uint Unknown_30h { get; set; } // 0x00000000
        public uint Unknown_34h { get; set; }
        public uint Unknown_38h { get; set; }
        public uint Unknown_3Ch { get; set; }
        public uint Unknown_40h { get; set; }
        public uint Unknown_44h { get; set; }
        public uint Unknown_48h { get; set; }
        public uint Unknown_4Ch { get; set; } // 0x00000000

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
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
        }

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
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
        }
    }








    [TC(typeof(EXP))] public class ParticleUnknown_P_013 : ResourceSystemBlock
    {
        public override long BlockLength => 0x58;

        // structure data
        public uint Unknown_0h { get; set; } // 0x73616942
        public uint Unknown_4h { get; set; } // 0x6E694C20
        public uint Unknown_8h { get; set; } // 0x6553206B
        public uint Unknown_Ch { get; set; } // 0x30305F74
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
        public ResourceSimpleList64<uint_r> Unknown_40h { get; set; }
        public uint Unknown_50h { get; set; }
        public uint Unknown_54h { get; set; } // 0x00000000

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
            this.Unknown_40h = reader.ReadBlock<ResourceSimpleList64<uint_r>>();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
        }

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
            writer.WriteBlock(this.Unknown_40h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x40, Unknown_40h)
            };
        }
    }







    // datBase
    // ptxShaderVar
    [TC(typeof(EXP))] public class ParticleShaderVar : ResourceSystemBlock, IResourceXXSystemBlock
    {
        public override long BlockLength => 24;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public byte Type { get; set; }
        public byte Unknown_15h { get; set; }
        public ushort Unknown_16h { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Type = reader.ReadByte();
            this.Unknown_15h = reader.ReadByte();
            this.Unknown_16h = reader.ReadUInt16();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Type);
            writer.Write(this.Unknown_15h);
            writer.Write(this.Unknown_16h);
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 20;
            var type = reader.ReadByte();
            reader.Position -= 21;

            switch (type)
            {
                case 2:
                case 4: return new ParticleShaderVarVector();
                case 6: return new ParticleShaderVarTexture();
                case 7: return new ParticleShaderVarKeyframe();
                default: return null;// throw new Exception("Unknown shader var type");
            }
        }
    }

    // ptxShaderVarVector
    [TC(typeof(EXP))] public class ParticleShaderVarVector : ParticleShaderVar
    {
        public override long BlockLength => 0x40;

        // structure data
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; }
        public uint Unknown_2Ch { get; set; }
        public uint Unknown_30h { get; set; }
        public uint Unknown_34h { get; set; }
        public uint Unknown_38h { get; set; }
        public uint Unknown_3Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
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

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
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

    // ptxShaderVarTexture
    [TC(typeof(EXP))] public class ParticleShaderVarTexture : ParticleShaderVar
    {
        public override long BlockLength => 0x40;

        // structure data
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public ulong TexturePointer { get; set; }
        public ulong NamePointer { get; set; }
        public uint NameHash { get; set; }
        public uint Unknown_3Ch { get; set; }

        // reference data
        public Texture Texture { get; set; }
        public string_r Name { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.TexturePointer = reader.ReadUInt64();
            this.NamePointer = reader.ReadUInt64();
            this.NameHash = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            this.Texture = reader.ReadBlockAt<Texture>(
                this.TexturePointer // offset
            );
            this.Name = reader.ReadBlockAt<string_r>(
                this.NamePointer // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            //this.TexturePointer = (ulong)(this.Texture != null ? this.Texture.Position : 0);
            //this.NamePointer = (ulong)(this.Name != null ? this.Name.Position : 0);

            // write structure data
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.TexturePointer);
            writer.Write(this.NamePointer);
            writer.Write(this.NameHash);
            writer.Write(this.Unknown_3Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            //if (Texture != null) list.Add(Texture);
            //if (Name != null) list.Add(Name);
            return list.ToArray();
        }
    }

    // ptxShaderVarKeyframe
    [TC(typeof(EXP))] public class ParticleShaderVarKeyframe : ParticleShaderVar
    {
        public override long BlockLength => 0x50;

        // structure data
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000001
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public ResourceSimpleList64<ParticleUnknown_P_001> Unknown_28h { get; set; }
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000
        public uint Unknown_40h { get; set; } // 0x00000000
        public uint Unknown_44h { get; set; } // 0x00000000
        public uint Unknown_48h { get; set; } // 0x00000000
        public uint Unknown_4Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown_P_001>>();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.WriteBlock(this.Unknown_28h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x28, Unknown_28h)
            };
        }
    }


    [TC(typeof(EXP))] public class ParticleUnknown_P_001 : ResourceSystemBlock
    {
        public override long BlockLength => 0x20;

        // structure data
        public uint Unknown_0h { get; set; }
        public uint Unknown_4h { get; set; }
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000

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
        }

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
        }
    }



    [TC(typeof(EXP))] public class ParticleUnknown_P_012 : ResourceSystemBlock
    {
        public override long BlockLength => 0x30;

        // structure data
        public uint Unknown_0h { get; set; }
        public uint Unknown_4h { get; set; }
        public uint Unknown_8h { get; set; }
        public uint Unknown_Ch { get; set; }
        public ulong Unknown_10h_Pointer { get; set; }
        public ulong DrawablePointer { get; set; }
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        // reference data
        public string_r Unknown_10h_Data { get; set; }
        public Drawable Drawable { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h_Pointer = reader.ReadUInt64();
            this.DrawablePointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();

            // read reference data
            this.Unknown_10h_Data = reader.ReadBlockAt<string_r>(
                this.Unknown_10h_Pointer // offset
            );
            this.Drawable = reader.ReadBlockAt<Drawable>(
                this.DrawablePointer // offset
            );
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            //// update structure data
            //this.Unknown_10h_Pointer = (ulong)(this.Unknown_10h_Data != null ? this.Unknown_10h_Data.Position : 0);
            //this.DrawablePointer = (ulong)(this.Drawable != null ? this.Drawable.Position : 0);

            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h_Pointer);
            writer.Write(this.DrawablePointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Unknown_10h_Data != null) list.Add(Unknown_10h_Data);
            if (Drawable != null) list.Add(Drawable);
            return list.ToArray();
        }
    }



























}
