using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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

    [TC(typeof(EXP))] public class ParticleEffectsList : ResourceFileBase
    {
        // pgBase
        // ptxFxList
        public override long BlockLength => 0x60;

        // structure data
        public ulong NamePointer { get; set; }
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong TextureDictionaryPointer { get; set; }
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong DrawableDictionaryPointer { get; set; }
        public ulong ParticleRuleDictionaryPointer { get; set; }
        public ulong Unknown_40h; // 0x0000000000000000
        public ulong EmitterRuleDictionaryPointer { get; set; }
        public ulong EffectRuleDictionaryPointer { get; set; }
        public ulong Unknown_58h; // 0x0000000000000000

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
            this.Unknown_18h = reader.ReadUInt64();
            this.TextureDictionaryPointer = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.DrawableDictionaryPointer = reader.ReadUInt64();
            this.ParticleRuleDictionaryPointer = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadUInt64();
            this.EmitterRuleDictionaryPointer = reader.ReadUInt64();
            this.EffectRuleDictionaryPointer = reader.ReadUInt64();
            this.Unknown_58h = reader.ReadUInt64();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(this.NamePointer);
            this.TextureDictionary = reader.ReadBlockAt<TextureDictionary>(this.TextureDictionaryPointer);
            this.DrawableDictionary = reader.ReadBlockAt<DrawableDictionary>(this.DrawableDictionaryPointer);
            this.ParticleRuleDictionary = reader.ReadBlockAt<ParticleRuleDictionary>(this.ParticleRuleDictionaryPointer);
            this.EffectRuleDictionary = reader.ReadBlockAt<ParticleEffectRuleDictionary>(this.EmitterRuleDictionaryPointer);
            this.EmitterRuleDictionary = reader.ReadBlockAt<ParticleEmitterRuleDictionary>(this.EffectRuleDictionaryPointer);



            if (Unknown_18h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_40h != 0)
            { }
            if (Unknown_58h != 0)
            { }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.NamePointer = (ulong)(this.Name != null ? this.Name.FilePosition : 0);
            this.TextureDictionaryPointer = (ulong)(this.TextureDictionary != null ? this.TextureDictionary.FilePosition : 0);
            this.DrawableDictionaryPointer = (ulong)(this.DrawableDictionary != null ? this.DrawableDictionary.FilePosition : 0);
            this.ParticleRuleDictionaryPointer = (ulong)(this.ParticleRuleDictionary != null ? this.ParticleRuleDictionary.FilePosition : 0);
            this.EmitterRuleDictionaryPointer = (ulong)(this.EffectRuleDictionary != null ? this.EffectRuleDictionary.FilePosition : 0);
            this.EffectRuleDictionaryPointer = (ulong)(this.EmitterRuleDictionary != null ? this.EmitterRuleDictionary.FilePosition : 0);

            // write structure data
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_18h);
            writer.Write(this.TextureDictionaryPointer);
            writer.Write(this.Unknown_28h);
            writer.Write(this.DrawableDictionaryPointer);
            writer.Write(this.ParticleRuleDictionaryPointer);
            writer.Write(this.Unknown_40h);
            writer.Write(this.EmitterRuleDictionaryPointer);
            writer.Write(this.EffectRuleDictionaryPointer);
            writer.Write(this.Unknown_58h);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            //TODO
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            //TODO
        }
        public static void WriteXmlNode(ParticleEffectsList p, StringBuilder sb, int indent, string ddsfolder, string name = "ParticleEffectsList")
        {
            if (p == null) return;
            YptXml.OpenTag(sb, indent, name);
            p.WriteXml(sb, indent + 1, ddsfolder);
            YptXml.CloseTag(sb, indent, name);
        }
        public static ParticleEffectsList ReadXmlNode(XmlNode node, string ddsfolder)
        {
            if (node == null) return null;
            var p = new ParticleEffectsList();
            p.ReadXml(node, ddsfolder);
            return p;
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Name != null) list.Add(Name);
            if (TextureDictionary != null) list.Add(TextureDictionary);
            if (DrawableDictionary != null) list.Add(DrawableDictionary);
            if (ParticleRuleDictionary != null) list.Add(ParticleRuleDictionary);
            if (EffectRuleDictionary != null) list.Add(EffectRuleDictionary);
            if (EmitterRuleDictionary != null) list.Add(EmitterRuleDictionary);
            return list.ToArray();
        }
    }


    [TC(typeof(EXP))] public class ParticleRuleDictionary : ResourceSystemBlock
    {
        // pgBase
        // pgDictionaryBase
        // pgDictionary<ptxParticleRule>
        public override long BlockLength => 0x40;

        // structure data
        public MetaHash VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h = 1; // 0x0000000000000001
        public ResourceSimpleList64<uint_r> ParticleRuleNameHashes { get; set; }
        public ResourcePointerList64<ParticleRule> ParticleRules { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.ParticleRuleNameHashes = reader.ReadBlock<ResourceSimpleList64<uint_r>>();
            this.ParticleRules = reader.ReadBlock<ResourcePointerList64<ParticleRule>>();

            if (Unknown_4h != 1)
            { }
            if (Unknown_8h != 0)
            { }
            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 1)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
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


    [TC(typeof(EXP))] public class ParticleEffectRuleDictionary : ResourceSystemBlock
    {
        // pgBase
        // pgDictionaryBase
        // pgDictionary<ptxEffectRule>
        public override long BlockLength => 0x40;

        // structure data
        public MetaHash VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h = 1; // 0x0000000000000001
        public ResourceSimpleList64<uint_r> EffectRuleNameHashes { get; set; }
        public ResourcePointerList64<ParticleEffectRule> EffectRules { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.EffectRuleNameHashes = reader.ReadBlock<ResourceSimpleList64<uint_r>>();
            this.EffectRules = reader.ReadBlock<ResourcePointerList64<ParticleEffectRule>>();

            if (Unknown_4h != 1)
            { }
            if (Unknown_8h != 0)
            { }
            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 1)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
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


    [TC(typeof(EXP))] public class ParticleEmitterRuleDictionary : ResourceSystemBlock
    {
        // pgBase
        // pgDictionaryBase
        // pgDictionary<ptxEmitterRule>
        public override long BlockLength => 0x40;

        // structure data
        public MetaHash VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h = 1; // 0x0000000000000001
        public ulong HashesPointer { get; set; }
        public ushort HashesCount1 { get; set; }
        public ushort HashesCount2 { get; set; }
        public uint Unknown_2Ch; // 0x00000000
        public ulong EffectRulesPointer { get; set; }
        public ushort EffectRulesCount1 { get; set; }
        public ushort EffectRulesCount2 { get; set; }
        public uint Unknown_3Ch; // 0x00000000

        // reference data
        public ResourceSimpleArray<uint_r> Hashes { get; set; }
        public ResourcePointerArray64<ParticleEmitterRule> EmitterRules { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.HashesPointer = reader.ReadUInt64();
            this.HashesCount1 = reader.ReadUInt16();
            this.HashesCount2 = reader.ReadUInt16();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.EffectRulesPointer = reader.ReadUInt64();
            this.EffectRulesCount1 = reader.ReadUInt16();
            this.EffectRulesCount2 = reader.ReadUInt16();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            this.Hashes = reader.ReadBlockAt<ResourceSimpleArray<uint_r>>(this.HashesPointer, this.HashesCount1);
            this.EmitterRules = reader.ReadBlockAt<ResourcePointerArray64<ParticleEmitterRule>>(this.EffectRulesPointer, this.EffectRulesCount1);

            if (Unknown_4h != 1)
            { }
            if (Unknown_8h != 0)
            { }
            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 1)
            { }
            if (Unknown_2Ch != 0)
            { }
            if (Unknown_3Ch != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.HashesPointer = (ulong)(this.Hashes != null ? this.Hashes.FilePosition : 0);
            this.HashesCount1 = (ushort)(this.Hashes != null ? this.Hashes.Count : 0);
            this.HashesCount2 = this.HashesCount1;
            this.EffectRulesPointer = (ulong)(this.EmitterRules != null ? this.EmitterRules.FilePosition : 0);
            this.EffectRulesCount1 = (ushort)(this.EmitterRules != null ? this.EmitterRules.Count : 0);
            this.EffectRulesCount2 = this.EffectRulesCount1;

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
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


    [TC(typeof(EXP))] public class ParticleRule : ResourceSystemBlock
    {
        // pgBase
        // pgBaseRefCounted
        // ptxParticleRule
        public override long BlockLength => 0x240;

        // structure data
        public MetaHash VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public MetaHash Unknown_10h { get; set; }
        public uint Unknown_14h; //0x00000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ParticleEffectSpawner Spawner1 { get; set; }
        public ParticleEffectSpawner Spawner2 { get; set; }
        public MetaHash Unknown_100h { get; set; }
        public MetaHash Unknown_104h { get; set; }
        public MetaHash Unknown_108h { get; set; }
        public MetaHash Unknown_10Ch { get; set; }
        public uint Unknown_110h; // 0x00000000
        public MetaHash Unknown_114h { get; set; }
        public MetaHash Unknown_118h { get; set; }
        public MetaHash Unknown_11Ch { get; set; }
        public ulong NamePointer { get; set; }
        public ResourcePointerList64<ParticleBehaviour> BehaviourList1 { get; set; }
        public ResourcePointerList64<ParticleBehaviour> BehaviourList2 { get; set; }
        public ResourcePointerList64<ParticleBehaviour> BehaviourList3 { get; set; }
        public ResourcePointerList64<ParticleBehaviour> BehaviourList4 { get; set; }
        public ResourcePointerList64<ParticleBehaviour> BehaviourList5 { get; set; }
        public ulong Unknown_178h; // 0x0000000000000000
        public ulong Unknown_180h; // 0x0000000000000000
        public ResourceSimpleList64<ParticleRuleUnknownItem> UnknownList1 { get; set; }
        public ulong Unknown_198h; // 0x0000000000000000
        public ulong Unknown_1A0h; // 0x0000000000000000
        public ulong Unknown_1A8h; // 0x0000000000000000
        public MetaHash VFTx3 { get; set; }
        public uint Unknown_1B4h = 1; // 0x00000001
        public ulong String1Pointer { get; set; }
        public ulong String2Pointer { get; set; }
        public ulong Unknown_1C8h; // 0x0000000000000000
        public MetaHash Unknown_1D0h { get; set; }
        public uint Unknown_1D4h; // 0x00000000
        public MetaHash VFTx4 { get; set; }
        public uint Unknown_1DCh = 1; // 0x00000001
        public MetaHash Unknown_1E0h { get; set; }
        public MetaHash Unknown_1E4h { get; set; }
        public MetaHash Unknown_1E8h { get; set; }
        public MetaHash Unknown_1ECh { get; set; }
        public ResourcePointerList64<ParticleShaderVar> ShaderVars { get; set; }
        public ulong Unknown_200h = 1; // 0x0000000000000001
        public MetaHash Unknown_208h { get; set; }
        public uint Unknown_20Ch; // 0x00000000
        public ResourceSimpleList64<ParticleRuleUnknownItem2> UnknownList2 { get; set; }
        public MetaHash Unknown_220h { get; set; }
        public uint Unknown_224h; // 0x00000000
        public ulong Unknown_228h; // 0x0000000000000000
        public ulong Unknown_230h; // 0x0000000000000000
        public ulong Unknown_238h; // 0x0000000000000000

        // reference data
        public string_r Name { get; set; }
        public string_r String1 { get; set; }
        public string_r String2 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            #region read data

            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt64();
            this.Spawner1 = reader.ReadBlock<ParticleEffectSpawner>();
            this.Spawner2 = reader.ReadBlock<ParticleEffectSpawner>();
            this.Unknown_100h = reader.ReadUInt32();
            this.Unknown_104h = reader.ReadUInt32();
            this.Unknown_108h = reader.ReadUInt32();
            this.Unknown_10Ch = reader.ReadUInt32();
            this.Unknown_110h = reader.ReadUInt32();
            this.Unknown_114h = reader.ReadUInt32();
            this.Unknown_118h = reader.ReadUInt32();
            this.Unknown_11Ch = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.BehaviourList1 = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            this.BehaviourList2 = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            this.BehaviourList3 = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            this.BehaviourList4 = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            this.BehaviourList5 = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            this.Unknown_178h = reader.ReadUInt64();
            this.Unknown_180h = reader.ReadUInt64();
            this.UnknownList1 = reader.ReadBlock<ResourceSimpleList64<ParticleRuleUnknownItem>>();
            this.Unknown_198h = reader.ReadUInt64();
            this.Unknown_1A0h = reader.ReadUInt64();
            this.Unknown_1A8h = reader.ReadUInt64();
            this.VFTx3 = reader.ReadUInt32();
            this.Unknown_1B4h = reader.ReadUInt32();
            this.String1Pointer = reader.ReadUInt64();
            this.String2Pointer = reader.ReadUInt64();
            this.Unknown_1C8h = reader.ReadUInt64();
            this.Unknown_1D0h = reader.ReadUInt32();
            this.Unknown_1D4h = reader.ReadUInt32();
            this.VFTx4 = reader.ReadUInt32();
            this.Unknown_1DCh = reader.ReadUInt32();
            this.Unknown_1E0h = reader.ReadUInt32();
            this.Unknown_1E4h = reader.ReadUInt32();
            this.Unknown_1E8h = reader.ReadUInt32();
            this.Unknown_1ECh = reader.ReadUInt32();
            this.ShaderVars = reader.ReadBlock<ResourcePointerList64<ParticleShaderVar>>();
            this.Unknown_200h = reader.ReadUInt64();
            this.Unknown_208h = reader.ReadUInt32();
            this.Unknown_20Ch = reader.ReadUInt32();
            this.UnknownList2 = reader.ReadBlock<ResourceSimpleList64<ParticleRuleUnknownItem2>>();
            this.Unknown_220h = reader.ReadUInt32();
            this.Unknown_224h = reader.ReadUInt32();
            this.Unknown_228h = reader.ReadUInt64();
            this.Unknown_230h = reader.ReadUInt64();
            this.Unknown_238h = reader.ReadUInt64();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(this.NamePointer);
            this.String1 = reader.ReadBlockAt<string_r>(this.String1Pointer);
            this.String2 = reader.ReadBlockAt<string_r>(this.String2Pointer);

            #endregion


            if (Unknown_4h != 1)
            { }
            if (Unknown_8h != 0)
            { }
            switch (Unknown_10h)
            {
                default:
                    break;
            }
            if (Unknown_14h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            switch (Unknown_100h)
            {
                default:
                    break;
            }
            switch (Unknown_104h)
            {
                default:
                    break;
            }
            switch (Unknown_108h)
            {
                default:
                    break;
            }
            switch (Unknown_10Ch)
            {
                default:
                    break;
            }
            if (Unknown_110h != 0)
            { }
            switch (Unknown_114h)
            {
                default:
                    break;
            }
            switch (Unknown_118h)
            {
                default:
                    break;
            }
            switch (Unknown_11Ch)
            {
                default:
                    break;
            }
            if (Unknown_178h != 0)
            { }
            if (Unknown_180h != 0)
            { }
            if (Unknown_198h != 0)
            { }
            if (Unknown_1A0h != 0)
            { }
            if (Unknown_1A8h != 0)
            { }
            switch (VFTx3)
            {
                default:
                    break;
            }
            if (Unknown_1B4h != 1)
            { }
            if (Unknown_1C8h != 0)
            { }
            switch (Unknown_1D0h)
            {
                default:
                    break;
            }
            if (Unknown_1D4h != 0)
            { }
            switch (VFTx4)
            {
                default:
                    break;
            }
            if (Unknown_1DCh != 1)
            { }
            switch (Unknown_1E0h)
            {
                default:
                    break;
            }
            switch (Unknown_1E4h)
            {
                default:
                    break;
            }
            switch (Unknown_1E8h)
            {
                default:
                    break;
            }
            switch (Unknown_1ECh)
            {
                default:
                    break;
            }
            if (Unknown_200h != 1)
            { }
            if (Unknown_208h != 0)
            { }
            if (Unknown_20Ch != 0)
            { }
            switch (Unknown_220h)
            {
                default:
                    break;
            }
            if (Unknown_224h != 0)
            { }
            if (Unknown_228h != 0)
            { }
            if (Unknown_230h != 0)
            { }
            if (Unknown_238h != 0)
            { }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.Name != null ? this.Name.FilePosition : 0);
            this.String1Pointer = (ulong)(this.String1 != null ? this.String1.FilePosition : 0);
            this.String2Pointer = (ulong)(this.String2 != null ? this.String2.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.WriteBlock(this.Spawner1);
            writer.WriteBlock(this.Spawner2);
            writer.Write(this.Unknown_100h);
            writer.Write(this.Unknown_104h);
            writer.Write(this.Unknown_108h);
            writer.Write(this.Unknown_10Ch);
            writer.Write(this.Unknown_110h);
            writer.Write(this.Unknown_114h);
            writer.Write(this.Unknown_118h);
            writer.Write(this.Unknown_11Ch);
            writer.Write(this.NamePointer);
            writer.WriteBlock(this.BehaviourList1);
            writer.WriteBlock(this.BehaviourList2);
            writer.WriteBlock(this.BehaviourList3);
            writer.WriteBlock(this.BehaviourList4);
            writer.WriteBlock(this.BehaviourList5);
            writer.Write(this.Unknown_178h);
            writer.Write(this.Unknown_180h);
            writer.WriteBlock(this.UnknownList1);
            writer.Write(this.Unknown_198h);
            writer.Write(this.Unknown_1A0h);
            writer.Write(this.Unknown_1A8h);
            writer.Write(this.VFTx3);
            writer.Write(this.Unknown_1B4h);
            writer.Write(this.String1Pointer);
            writer.Write(this.String2Pointer);
            writer.Write(this.Unknown_1C8h);
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
            writer.Write(this.Unknown_208h);
            writer.Write(this.Unknown_20Ch);
            writer.WriteBlock(this.UnknownList2);
            writer.Write(this.Unknown_220h);
            writer.Write(this.Unknown_224h);
            writer.Write(this.Unknown_228h);
            writer.Write(this.Unknown_230h);
            writer.Write(this.Unknown_238h);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            if (String1 != null) list.Add(String1);
            if (String2 != null) list.Add(String2);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(88, Spawner1),
                new Tuple<long, IResourceBlock>(96, Spawner2),
                new Tuple<long, IResourceBlock>(0x128, BehaviourList1),
                new Tuple<long, IResourceBlock>(0x138, BehaviourList2),
                new Tuple<long, IResourceBlock>(0x148, BehaviourList3),
                new Tuple<long, IResourceBlock>(0x158, BehaviourList4),
                new Tuple<long, IResourceBlock>(0x168, BehaviourList5),
                new Tuple<long, IResourceBlock>(0x188, UnknownList1),
                new Tuple<long, IResourceBlock>(0x1F0, ShaderVars),
                new Tuple<long, IResourceBlock>(0x210, UnknownList2)
            };
        }
    }


    [TC(typeof(EXP))] public class ParticleRuleUnknownItem : ResourceSystemBlock
    {
        public override long BlockLength => 0x58;

        // structure data
        public MetaHash Unknown_0h { get; set; } // 0x73616942 //text?
        public MetaHash Unknown_4h { get; set; } // 0x6E694C20
        public MetaHash Unknown_8h { get; set; } // 0x6553206B
        public MetaHash Unknown_Ch { get; set; } // 0x30305F74
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong Unknown_30h; // 0x0000000000000000
        public ulong Unknown_38h; // 0x0000000000000000
        public ResourceSimpleList64<uint_r> Unknown_40h { get; set; }
        public uint Unknown_50h { get; set; }
        public uint Unknown_54h; // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt64();
            this.Unknown_38h = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadBlock<ResourceSimpleList64<uint_r>>();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();

            switch (Unknown_0h)
            {
                default:
                    break;
            }
            switch (Unknown_4h)
            {
                default:
                    break;
            }
            switch (Unknown_8h)
            {
                default:
                    break;
            }
            switch (Unknown_Ch)
            {
                default:
                    break;
            }
            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_30h != 0)
            { }
            if (Unknown_38h != 0)
            { }
            switch (Unknown_50h)
            {
                default:
                    break;
            }
            if (Unknown_54h != 0)
            { }
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_38h);
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


    [TC(typeof(EXP))] public class ParticleRuleUnknownItem2 : ResourceSystemBlock
    {
        public override long BlockLength => 0x30;

        // structure data
        public MetaHash Unknown_0h { get; set; }
        public MetaHash Unknown_4h { get; set; }
        public MetaHash Unknown_8h { get; set; }
        public MetaHash Unknown_Ch { get; set; }
        public ulong String1Pointer { get; set; }
        public ulong DrawablePointer { get; set; }
        public MetaHash Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; } // 0x00000000
        public ulong Unknown_28h; // 0x0000000000000000

        // reference data
        public string_r String1 { get; set; }
        public Drawable Drawable { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.String1Pointer = reader.ReadUInt64();
            this.DrawablePointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt64();

            // read reference data
            this.String1 = reader.ReadBlockAt<string_r>(this.String1Pointer);
            this.Drawable = reader.ReadBlockAt<Drawable>(this.DrawablePointer);


            switch (Unknown_0h)
            {
                default:
                    break;
            }
            switch (Unknown_4h)
            {
                default:
                    break;
            }
            switch (Unknown_8h)
            {
                default:
                    break;
            }
            switch (Unknown_Ch)
            {
                default:
                    break;
            }
            switch (Unknown_20h)
            {
                default:
                    break;
            }
            if (Unknown_24h != 0)
            { }
            if (Unknown_28h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.String1Pointer = (ulong)(this.String1 != null ? this.String1.FilePosition : 0);
            this.DrawablePointer = (ulong)(this.Drawable != null ? this.Drawable.FilePosition : 0);

            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.String1Pointer);
            writer.Write(this.DrawablePointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (String1 != null) list.Add(String1);
            if (Drawable != null) list.Add(Drawable);
            return list.ToArray();
        }
    }


    [TC(typeof(EXP))] public class ParticleEffectSpawner : ResourceSystemBlock
    {
        // pgBase
        // ptxEffectSpawner
        public override long BlockLength => 0x70;

        // structure data
        public MetaHash VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public MetaHash Unknown_18h { get; set; }
        public MetaHash Unknown_1Ch { get; set; }
        public MetaHash Unknown_20h { get; set; }
        public MetaHash Unknown_24h { get; set; }
        public MetaHash Unknown_28h { get; set; }
        public uint Unknown_2Ch; // 0x00000000
        public ulong Unknown_30h; // 0x0000000000000000
        public MetaHash Unknown_38h { get; set; }
        public MetaHash Unknown_3Ch { get; set; }
        public MetaHash Unknown_40h { get; set; }
        public MetaHash Unknown_44h { get; set; }
        public MetaHash Unknown_48h { get; set; }
        public uint Unknown_4Ch; // 0x00000000
        public ulong Unknown_50h; // 0x0000000000000000
        public ulong EmitterRulePointer { get; set; }
        public ulong String1Pointer { get; set; }
        public uint Unknown_68h { get; set; }
        public uint Unknown_6Ch { get; set; }

        // reference data
        public ParticleEffectRule EmitterRule { get; set; }
        public string_r String1 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt64();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadUInt64();
            this.EmitterRulePointer = reader.ReadUInt64();
            this.String1Pointer = reader.ReadUInt64();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();

            // read reference data
            this.EmitterRule = reader.ReadBlockAt<ParticleEffectRule>(this.EmitterRulePointer);
            this.String1 = reader.ReadBlockAt<string_r>(this.String1Pointer);

            if (Unknown_40h != 1)
            { }
            if (Unknown_8h != 0)
            { }
            if (Unknown_10h != 0)
            { }
            switch (Unknown_18h)
            {
                default:
                    break;
            }
            switch (Unknown_1Ch)
            {
                default:
                    break;
            }
            switch (Unknown_20h)
            {
                default:
                    break;
            }
            switch (Unknown_24h)
            {
                default:
                    break;
            }
            switch (Unknown_28h)
            {
                default:
                    break;
            }
            if (Unknown_2Ch != 0)
            { }
            if (Unknown_30h != 0)
            { }
            switch (Unknown_38h)
            {
                default:
                    break;
            }
            switch (Unknown_3Ch)
            {
                default:
                    break;
            }
            switch (Unknown_40h)
            {
                default:
                    break;
            }
            switch (Unknown_44h)
            {
                default:
                    break;
            }
            switch (Unknown_48h)
            {
                default:
                    break;
            }
            if (Unknown_4Ch != 0)
            { }
            if (Unknown_50h != 0)
            { }
            switch (Unknown_68h)
            {
                default:
                    break;
            }
            switch (Unknown_6Ch)
            {
                default:
                    break;
            }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.EmitterRulePointer = (ulong)(this.EmitterRule != null ? this.EmitterRule.FilePosition : 0);
            this.String1Pointer = (ulong)(this.String1 != null ? this.String1.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Unknown_50h);
            writer.Write(this.EmitterRulePointer);
            writer.Write(this.String1Pointer);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (EmitterRule != null) list.Add(EmitterRule);
            if (String1 != null) list.Add(String1);
            return list.ToArray();
        }
    }


    [TC(typeof(EXP))] public class ParticleEffectRule : ResourceSystemBlock
    {
        // pgBase
        // pgBaseRefCounted
        // ptxEffectRule
        public override long BlockLength => 0x3C0;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h = 1; // 0x0000000000000001
        public MetaHash Unknown_18h { get; set; } = 0x40866666; // 0x40866666 //4.2f
        public uint Unknown_1Ch; // 0x00000000
        public ulong NamePointer { get; set; }
        public ulong Unknown_28h { get; set; } // 0x50000000 -> ".?AVptxFxList@rage@@" pointer to itself
        public MetaHash Unknown_30h { get; set; }
        public uint Unknown_34h = 1; // 0x00000001
        public ulong EventEmittersPointer { get; set; }
        public ushort EventEmittersCount1 { get; set; }
        public ushort EventEmittersCount2 { get; set; }
        public MetaHash Unknown_44h { get; set; }
        public ulong UnknownData1Pointer { get; set; }
        public MetaHash Unknown_50h { get; set; }
        public MetaHash Unknown_54h { get; set; }
        public ulong Unknown_58h; // 0x0000000000000000
        public ulong Unknown_60h; // 0x0000000000000000
        public uint Unknown_68h; // 0x00000000
        public MetaHash Unknown_6Ch { get; set; }
        public MetaHash Unknown_70h { get; set; }
        public MetaHash Unknown_74h { get; set; }
        public MetaHash Unknown_78h { get; set; }
        public MetaHash Unknown_7Ch { get; set; }
        public MetaHash Unknown_80h { get; set; }
        public MetaHash Unknown_84h { get; set; }
        public MetaHash Unknown_88h { get; set; }
        public MetaHash Unknown_8Ch { get; set; }
        public MetaHash Unknown_90h { get; set; }
        public MetaHash Unknown_94h { get; set; }
        public MetaHash Unknown_98h { get; set; }
        public MetaHash Unknown_9Ch { get; set; }
        public MetaHash Unknown_A0h { get; set; }
        public MetaHash Unknown_A4h { get; set; }
        public MetaHash Unknown_A8h { get; set; }
        public MetaHash Unknown_ACh { get; set; }
        public MetaHash Unknown_B0h { get; set; }
        public MetaHash Unknown_B4h { get; set; }
        public MetaHash Unknown_B8h { get; set; }
        public MetaHash Unknown_BCh { get; set; }
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public ParticleKeyframeProp KeyframeProp4 { get; set; }
        public ulong KeyframePropsPointer { get; set; }
        public ushort KeyframePropsCount1 { get; set; }
        public ushort KeyframePropsCount2 { get; set; }
        public uint Unknown_39Ch; // 0x00000000
        public MetaHash Unknown_3A0h { get; set; }
        public uint Unknown_3A4h; // 0x00000000
        public MetaHash Unknown_3A8h { get; set; } = 0x42C80000; // 0x42C80000 //100.0f
        public uint Unknown_3ACh { get; set; } // 0x00000000
        public ulong Unknown_3B0h { get; set; } // 0x0000000000000000
        public ulong Unknown_3B8h { get; set; } // 0x0000000000000000

        // reference data
        public string_r Name { get; set; }
        public ResourcePointerArray64<ParticleEventEmitter> EventEmitters { get; set; }
        public ParticleUnknown1 UnknownData { get; set; }
        public ResourcePointerArray64<ParticleKeyframeProp> KeyframeProps { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            #region read

            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.EventEmittersPointer = reader.ReadUInt64();
            this.EventEmittersCount1 = reader.ReadUInt16();
            this.EventEmittersCount2 = reader.ReadUInt16();
            this.Unknown_44h = reader.ReadUInt32();
            this.UnknownData1Pointer = reader.ReadUInt64();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.Unknown_58h = reader.ReadUInt64();
            this.Unknown_60h = reader.ReadUInt64();
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
            this.Unknown_3B0h = reader.ReadUInt64();
            this.Unknown_3B8h = reader.ReadUInt64();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(this.NamePointer);
            this.EventEmitters = reader.ReadBlockAt<ResourcePointerArray64<ParticleEventEmitter>>(this.EventEmittersPointer, this.EventEmittersCount1);
            this.UnknownData = reader.ReadBlockAt<ParticleUnknown1>(this.UnknownData1Pointer);
            this.KeyframeProps = reader.ReadBlockAt<ResourcePointerArray64<ParticleKeyframeProp>>(this.KeyframePropsPointer, this.KeyframePropsCount2);

            #endregion


            if (Unknown_4h != 1)
            { }
            if (Unknown_8h != 0)
            { }
            if (Unknown_10h != 1)
            { }
            if (Unknown_18h != 0x40866666)
            { }
            if (Unknown_1Ch != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_30h)
            {
                default:
                    break;
            }
            if (Unknown_34h != 1)
            { }
            switch (Unknown_44h)
            {
                default:
                    break;
            }
            switch (Unknown_50h)
            {
                default:
                    break;
            }
            switch (Unknown_54h)
            {
                default:
                    break;
            }
            if (Unknown_58h != 0)
            { }
            if (Unknown_60h != 0)
            { }
            if (Unknown_68h != 0)
            { }
            switch (Unknown_6Ch)
            {
                default:
                    break;
            }
            switch (Unknown_70h)
            {
                default:
                    break;
            }
            switch (Unknown_74h)
            {
                default:
                    break;
            }
            switch (Unknown_78h)
            {
                default:
                    break;
            }
            switch (Unknown_7Ch)
            {
                default:
                    break;
            }
            switch (Unknown_80h)
            {
                default:
                    break;
            }
            switch (Unknown_84h)
            {
                default:
                    break;
            }
            switch (Unknown_88h)
            {
                default:
                    break;
            }
            switch (Unknown_8Ch)
            {
                default:
                    break;
            }
            switch (Unknown_90h)
            {
                default:
                    break;
            }
            switch (Unknown_94h)
            {
                default:
                    break;
            }
            switch (Unknown_98h)
            {
                default:
                    break;
            }
            switch (Unknown_9Ch)
            {
                default:
                    break;
            }
            switch (Unknown_A0h)
            {
                default:
                    break;
            }
            switch (Unknown_A4h)
            {
                default:
                    break;
            }
            switch (Unknown_A8h)
            {
                default:
                    break;
            }
            switch (Unknown_ACh)
            {
                default:
                    break;
            }
            switch (Unknown_B0h)
            {
                default:
                    break;
            }
            switch (Unknown_B4h)
            {
                default:
                    break;
            }
            switch (Unknown_B8h)
            {
                default:
                    break;
            }
            switch (Unknown_BCh)
            {
                default:
                    break;
            }
            if (Unknown_39Ch != 0)
            { }
            switch (Unknown_3A0h)
            {
                default:
                    break;
            }
            if (Unknown_3A4h != 0)
            { }
            if (Unknown_3A8h != 0x42C80000)
            { }
            if (Unknown_3ACh != 0)
            { }
            if (Unknown_3B0h != 0)
            { }
            if (Unknown_3B8h != 0)
            { }

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.Name != null ? this.Name.FilePosition : 0);
            this.EventEmittersPointer = (ulong)(this.EventEmitters != null ? this.EventEmitters.FilePosition : 0);
            //this.c3b = (ushort)(this.p3data != null ? this.p3data.Count : 0);
            this.UnknownData1Pointer = (ulong)(this.UnknownData != null ? this.UnknownData.FilePosition : 0);
            this.KeyframePropsPointer = (ulong)(this.KeyframeProps != null ? this.KeyframeProps.FilePosition : 0);
            //this.refcnt2 = (ushort)(this.refs != null ? this.refs.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.EventEmittersPointer);
            writer.Write(this.EventEmittersCount1);
            writer.Write(this.EventEmittersCount2);
            writer.Write(this.Unknown_44h);
            writer.Write(this.UnknownData1Pointer);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_60h);
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
            writer.Write(this.Unknown_3B8h);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            if (EventEmitters != null) list.Add(EventEmitters);
            if (UnknownData != null) list.Add(UnknownData);
            if (KeyframeProps != null) list.Add(KeyframeProps);
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
    

    [TC(typeof(EXP))] public class ParticleKeyframeProp : ResourceSystemBlock
    {
        // datBase
        // ptxKeyframeProp
        public override long BlockLength => 0x90;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong Unknown_30h; // 0x0000000000000000
        public ulong Unknown_38h; // 0x0000000000000000
        public ulong Unknown_40h; // 0x0000000000000000
        public ulong Unknown_48h; // 0x0000000000000000
        public ulong Unknown_50h; // 0x0000000000000000
        public ulong Unknown_58h; // 0x0000000000000000
        public ulong Unknown_60h; // 0x0000000000000000
        public MetaHash Unknown_68h { get; set; }
        public MetaHash Unknown_6Ch { get; set; }
        public ResourceSimpleList64<ParticleKeyframePropItem> Items { get; set; }
        public ulong Unknown_80h; // 0x0000000000000000
        public ulong Unknown_88h; // 0x0000000000000000


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt64();
            this.Unknown_38h = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadUInt64();
            this.Unknown_48h = reader.ReadUInt64();
            this.Unknown_50h = reader.ReadUInt64();
            this.Unknown_58h = reader.ReadUInt64();
            this.Unknown_60h = reader.ReadUInt64();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Items = reader.ReadBlock<ResourceSimpleList64<ParticleKeyframePropItem>>();
            this.Unknown_80h = reader.ReadUInt64();
            this.Unknown_88h = reader.ReadUInt64();


            if (Unknown_4h != 1)
            { }
            if (Unknown_8h != 0)
            { }
            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_30h != 0)
            { }
            if (Unknown_38h != 0)
            { }
            if (Unknown_40h != 0)
            { }
            if (Unknown_48h != 0)
            { }
            if (Unknown_50h != 0)
            { }
            if (Unknown_58h != 0)
            { }
            if (Unknown_60h != 0)
            { }
            switch (Unknown_68h)
            {
                default:
                    break;
            }
            switch (Unknown_6Ch)
            {
                default:
                    break;
            }
            if (Unknown_80h != 0)
            { }
            if (Unknown_88h != 0)
            { }
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
            writer.WriteBlock(this.Items);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_88h);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x70, Items)
            };
        }
    }


    [TC(typeof(EXP))] public class ParticleKeyframePropItem : ResourceSystemBlock
    {
        public override long BlockLength => 0x20;

        // structure data
        public float Unknown_0h { get; set; }
        public float Unknown_4h { get; set; }
        public ulong Unknown_8h; // 0x0000000000000000
        public float Unknown_10h { get; set; }
        public float Unknown_14h { get; set; }
        public float Unknown_18h { get; set; }
        public float Unknown_1Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadSingle();
            this.Unknown_4h = reader.ReadSingle();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadSingle();
            this.Unknown_14h = reader.ReadSingle();
            this.Unknown_18h = reader.ReadSingle();
            this.Unknown_1Ch = reader.ReadSingle();

            switch (Unknown_0h)
            {
                default:
                    break;
            }
            switch (Unknown_4h)
            {
                default:
                    break;
            }
            if (Unknown_8h != 0)
            { }
            switch (Unknown_10h)
            {
                default:
                    break;
            }
            switch (Unknown_14h)
            {
                default:
                    break;
            }
            switch (Unknown_18h)
            {
                default:
                    break;
            }
            switch (Unknown_1Ch)
            {
                default:
                    break;
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
        }
    }
    [TC(typeof(EXP))] public class ParticleKeyframePropItem2 : ResourceSystemBlock
    {
        public override long BlockLength => 0x20;

        // structure data
        public MetaHash Unknown_0h { get; set; }
        public MetaHash Unknown_4h { get; set; }
        public ulong Unknown_8h; // 0x0000000000000000
        public MetaHash Unknown_10h { get; set; }
        public MetaHash Unknown_14h { get; set; }
        public MetaHash Unknown_18h { get; set; }
        public MetaHash Unknown_1Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();

            switch (Unknown_0h)
            {
                default:
                    break;
            }
            switch (Unknown_4h)
            {
                default:
                    break;
            }
            if (Unknown_8h != 0)
            { }
            switch (Unknown_10h)
            {
                default:
                    break;
            }
            switch (Unknown_14h)
            {
                default:
                    break;
            }
            switch (Unknown_18h)
            {
                default:
                    break;
            }
            switch (Unknown_1Ch)
            {
                default:
                    break;
            }
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
        }
    }


    [TC(typeof(EXP))] public class ParticleEventEmitter : ResourceSystemBlock
    {
        // ptxEvent
        // ptxEventEmitter
        public override long BlockLength => 0x70;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public MetaHash Unknown_8h { get; set; }
        public uint Unknown_Ch; // 0x00000000
        public MetaHash Unknown_10h { get; set; }
        public uint Unknown_14h; // 0x00000000
        public ulong UnknownDataPointer { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong String1Pointer { get; set; }
        public ulong String2Pointer { get; set; }
        public ulong EmitterRulePointer { get; set; }
        public ulong ParticleRulePointer { get; set; }
        public MetaHash Unknown_50h { get; set; }
        public MetaHash Unknown_54h { get; set; }
        public MetaHash Unknown_58h { get; set; }
        public MetaHash Unknown_5Ch { get; set; }
        public MetaHash Unknown_60h { get; set; }
        public MetaHash Unknown_64h { get; set; }
        public ulong Unknown_68h; // 0x0000000000000000

        // reference data
        public ParticleUnknown1 UnknownData { get; set; }
        public string_r String1 { get; set; }
        public string_r String2 { get; set; }
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
            this.UnknownDataPointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.String1Pointer = reader.ReadUInt64();
            this.String2Pointer = reader.ReadUInt64();
            this.EmitterRulePointer = reader.ReadUInt64();
            this.ParticleRulePointer = reader.ReadUInt64();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.Unknown_58h = reader.ReadUInt32();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.Unknown_60h = reader.ReadUInt32();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt64();

            // read reference data
            this.UnknownData = reader.ReadBlockAt<ParticleUnknown1>(this.UnknownDataPointer);
            this.String1 = reader.ReadBlockAt<string_r>(this.String1Pointer);
            this.String2 = reader.ReadBlockAt<string_r>(this.String2Pointer);
            this.EmitterRule = reader.ReadBlockAt<ParticleEmitterRule>(this.EmitterRulePointer);
            this.ParticleRule = reader.ReadBlockAt<ParticleRule>(this.ParticleRulePointer);

            if (Unknown_4h != 1)
            { }
            switch (Unknown_8h)
            {
                default:
                    break;
            }
            if (Unknown_Ch != 0)
            { }
            switch (Unknown_10h)
            {
                default:
                    break;
            }
            if (Unknown_14h != 0)
            { }
            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_50h)
            {
                default:
                    break;
            }
            switch (Unknown_54h)
            {
                default:
                    break;
            }
            switch (Unknown_58h)
            {
                default:
                    break;
            }
            switch (Unknown_5Ch)
            {
                default:
                    break;
            }
            switch (Unknown_60h)
            {
                default:
                    break;
            }
            switch (Unknown_64h)
            {
                default:
                    break;
            }
            if (Unknown_68h != 0)
            { }

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.UnknownDataPointer = (ulong)(this.UnknownData != null ? this.UnknownData.FilePosition : 0);
            this.String1Pointer = (ulong)(this.String1 != null ? this.String1.FilePosition : 0);
            this.String2Pointer = (ulong)(this.String2 != null ? this.String2.FilePosition : 0);
            this.EmitterRulePointer = (ulong)(this.EmitterRule != null ? this.EmitterRule.FilePosition : 0);
            this.ParticleRulePointer = (ulong)(this.ParticleRule != null ? this.ParticleRule.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.UnknownDataPointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.String1Pointer);
            writer.Write(this.String2Pointer);
            writer.Write(this.EmitterRulePointer);
            writer.Write(this.ParticleRulePointer);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (UnknownData != null) list.Add(UnknownData);
            if (String1 != null) list.Add(String1);
            if (String2 != null) list.Add(String2);
            if (EmitterRule != null) list.Add(EmitterRule);
            if (ParticleRule != null) list.Add(ParticleRule);
            return list.ToArray();
        }
    }


    [TC(typeof(EXP))] public class ParticleEmitterRule : ResourceSystemBlock
    {
        // pgBase
        // pgBaseRefCounted
        // ptxEmitterRule
        public override long BlockLength => 0x630;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public MetaHash Unknown_10h { get; set; }
        public uint Unknown_14h; // 0x00000000
        public float Unknown_18h { get; set; } = 4.1f; // 0x40833333  4.1f
        public uint Unknown_1Ch; // 0x00000000
        public ulong NamePointer { get; set; }
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong Unknown_30h; // 0x0000000000000000
        public ulong Domain1Pointer { get; set; }
        public ulong Unknown_40h; // 0x0000000000000000
        public ulong Domain2Pointer { get; set; }
        public ulong Unknown_50h; // 0x0000000000000000
        public ulong Domain3Pointer { get; set; }
        public ulong Unknown_60h; // 0x0000000000000000
        public ulong Unknown_68h; // 0x0000000000000000
        public ulong Unknown_70h; // 0x0000000000000000
        public ParticleKeyframeProp[] KeyframeProps1 { get; set; } = new ParticleKeyframeProp[10];
        public ulong KeyframeProps2Pointer { get; set; }
        public ushort KeyframePropsCount1 = 10; // 10
        public ushort KeyframePropsCount2 = 10; // 10
        public uint Unknown_624h; // 0x00000000
        public MetaHash Unknown_628h { get; set; }
        public uint Unknown_62Ch; // 0x00000000

        // reference data
        public string_r Name { get; set; }
        public ParticleDomain Domain1 { get; set; }
        public ParticleDomain Domain2 { get; set; }
        public ParticleDomain Domain3 { get; set; }
        public ResourcePointerArray64<ParticleKeyframeProp> KeyframeProps2 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadSingle();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt64();
            this.Domain1Pointer = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadUInt64();
            this.Domain2Pointer = reader.ReadUInt64();
            this.Unknown_50h = reader.ReadUInt64();
            this.Domain3Pointer = reader.ReadUInt64();
            this.Unknown_60h = reader.ReadUInt64();
            this.Unknown_68h = reader.ReadUInt64();
            this.Unknown_70h = reader.ReadUInt64();
            for (int i = 0; i < 10; i++)
            {
                this.KeyframeProps1[i] = reader.ReadBlock<ParticleKeyframeProp>();
            }
            this.KeyframeProps2Pointer = reader.ReadUInt64();
            this.KeyframePropsCount1 = reader.ReadUInt16();
            this.KeyframePropsCount2 = reader.ReadUInt16();
            this.Unknown_624h = reader.ReadUInt32();
            this.Unknown_628h = reader.ReadUInt32();
            this.Unknown_62Ch = reader.ReadUInt32();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(this.NamePointer);
            this.Domain1 = reader.ReadBlockAt<ParticleDomain>(this.Domain1Pointer);
            this.Domain2 = reader.ReadBlockAt<ParticleDomain>(this.Domain2Pointer);
            this.Domain3 = reader.ReadBlockAt<ParticleDomain>(this.Domain3Pointer);
            this.KeyframeProps2 = reader.ReadBlockAt<ResourcePointerArray64<ParticleKeyframeProp>>(this.KeyframeProps2Pointer, this.KeyframePropsCount2);


            if (Unknown_4h != 1)
            { }
            if (Unknown_8h != 0)
            { }
            switch (Unknown_10h)
            {
                default:
                    break;
            }
            if (Unknown_14h != 0)
            { }
            if (Unknown_18h != 4.1f)
            { }
            if (Unknown_1Ch != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_30h != 0)
            { }
            if (Unknown_40h != 0)
            { }
            if (Unknown_50h != 0)
            { }
            if (Unknown_60h != 0)
            { }
            if (Unknown_68h != 0)
            { }
            if (Unknown_70h != 0)
            { }
            if (KeyframePropsCount1 != 10)
            { }
            if (KeyframePropsCount2 != 10)
            { }
            if (Unknown_624h != 0)
            { }
            switch (Unknown_628h)
            {
                default:
                    break;
            }
            if (Unknown_62Ch != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.Name != null ? this.Name.FilePosition : 0);
            this.Domain1Pointer = (ulong)(this.Domain1 != null ? this.Domain1.FilePosition : 0);
            this.Domain2Pointer = (ulong)(this.Domain2 != null ? this.Domain2.FilePosition : 0);
            this.Domain3Pointer = (ulong)(this.Domain3 != null ? this.Domain3.FilePosition : 0);
            this.KeyframeProps2Pointer = (ulong)(this.KeyframeProps2 != null ? this.KeyframeProps2.FilePosition : 0);
            //this.refcnt2 = (ushort)(this.refs != null ? this.refs.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Domain1Pointer);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Domain2Pointer);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Domain3Pointer);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_70h);
            for (int i = 0; i < 10; i++)
            {
                writer.WriteBlock(this.KeyframeProps1[i]);
            }
            writer.Write(this.KeyframeProps2Pointer);
            writer.Write(this.KeyframePropsCount1);
            writer.Write(this.KeyframePropsCount2);
            writer.Write(this.Unknown_624h);
            writer.Write(this.Unknown_628h);
            writer.Write(this.Unknown_62Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            if (Domain1 != null) list.Add(Domain1);
            if (Domain2 != null) list.Add(Domain2);
            if (Domain3 != null) list.Add(Domain3);
            if (KeyframeProps2 != null) list.Add(KeyframeProps2);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(120, KeyframeProps1[0]),
                new Tuple<long, IResourceBlock>(264, KeyframeProps1[1]),
                new Tuple<long, IResourceBlock>(408, KeyframeProps1[2]),
                new Tuple<long, IResourceBlock>(552, KeyframeProps1[3]),
                new Tuple<long, IResourceBlock>(696, KeyframeProps1[4]),
                new Tuple<long, IResourceBlock>(840, KeyframeProps1[5]),
                new Tuple<long, IResourceBlock>(984, KeyframeProps1[6]),
                new Tuple<long, IResourceBlock>(1128, KeyframeProps1[7]),
                new Tuple<long, IResourceBlock>(1272, KeyframeProps1[8]),
                new Tuple<long, IResourceBlock>(1416, KeyframeProps1[9]),
            };
        }
    }












    [TC(typeof(EXP))] public class ParticleUnknown1 : ResourceSystemBlock
    {
        public override long BlockLength => 0x40;

        // structure data
        public ResourceSimpleList64<ParticleStringBlock> Unknown_0h { get; set; }
        public ResourceSimpleList64<ParticleUnknown2> Unknown_10h { get; set; }
        public ulong Unknown_20h = 1; // 0x0000000000000001
        public ResourceSimpleList64<ParticleUnknown2Block> Unknown_28h { get; set; }
        public ulong Unknown_38h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadBlock<ResourceSimpleList64<ParticleStringBlock>>();
            this.Unknown_10h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown2>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown2Block>>();
            this.Unknown_38h = reader.ReadUInt64();

            if (Unknown_20h != 1)
            { }
            if (Unknown_38h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(this.Unknown_0h);
            writer.WriteBlock(this.Unknown_10h);
            writer.Write(this.Unknown_20h);
            writer.WriteBlock(this.Unknown_28h);
            writer.Write(this.Unknown_38h);
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
    

    [TC(typeof(EXP))] public class ParticleStringBlock : ResourceSystemBlock
    {
        public override long BlockLength => 24;

        // structure data
        public ulong String1Pointer { get; set; }
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000

        // reference data
        public string_r String1 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.String1Pointer = reader.ReadUInt64();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();

            // read reference data
            this.String1 = reader.ReadBlockAt<string_r>(this.String1Pointer);


            if (Unknown_8h != 0)
            { }
            if (Unknown_10h != 0)
            { }
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.String1Pointer = (ulong)(this.String1 != null ? this.String1.FilePosition : 0);

            // write structure data
            writer.Write(this.String1Pointer);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (String1 != null) list.Add(String1);
            return list.ToArray();
        }
    }


    [TC(typeof(EXP))] public class ParticleUnknown2Block : ResourceSystemBlock
    {
        public override long BlockLength => 0x10;

        // structure data
        public MetaHash Unknown_0h { get; set; }
        public uint Unknown_4h; // 0x00000000
        public ulong ItemPointer { get; set; }

        // reference data
        public ParticleUnknown2 Item { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.ItemPointer = reader.ReadUInt64();

            // read reference data
            this.Item = reader.ReadBlockAt<ParticleUnknown2>(this.ItemPointer);

            switch (Unknown_0h)
            {
                default:
                    break;
            }
            if (Unknown_4h != 0)
            { }

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.ItemPointer = (ulong)(this.Item != null ? this.Item.FilePosition : 0);

            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.ItemPointer);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Item != null) list.Add(Item);
            return list.ToArray();
        }
    }


    [TC(typeof(EXP))] public class ParticleUnknown2 : ResourceSystemBlock
    {
        public override long BlockLength => 24;

        // structure data
        public ResourceSimpleList64<ParticleUnknown3> Unknown_0h { get; set; }
        public MetaHash Unknown_10h { get; set; }
        public MetaHash Unknown_14h { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown3>>();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(this.Unknown_0h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);

            switch (Unknown_10h)
            {
                default:
                    break;
            }
            switch (Unknown_14h)
            {
                default:
                    break;
            }
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, Unknown_0h)
            };
        }
    }


    [TC(typeof(EXP))] public class ParticleUnknown3 : ResourceSystemBlock
    {
        public override long BlockLength => 0x30;

        // structure data
        public ResourceSimpleList64<ParticleKeyframePropItem2> Unknown_0h { get; set; }
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public MetaHash Unknown_20h { get; set; }
        public MetaHash Unknown_24h { get; set; }
        public ulong Unknown_28h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadBlock<ResourceSimpleList64<ParticleKeyframePropItem2>>();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt64();

            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            switch (Unknown_20h)
            {
                default:
                    break;
            }
            switch (Unknown_24h)
            {
                default:
                    break;
            }
            if (Unknown_28h != 0)
            { }
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(this.Unknown_0h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, Unknown_0h)
            };
        }
    }

















    public enum ParticleDomainType : byte
    {
        Box = 0,
        Sphere = 1,
        Cylinder = 2,
        Attractor = 3,
    }

    [TC(typeof(EXP))] public class ParticleDomain : ResourceSystemBlock, IResourceXXSystemBlock
    {
        // datBase
        // ptxDomain
        public override long BlockLength => 0x280;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public MetaHash Unknown_8h { get; set; }
        public ParticleDomainType DomainType { get; set; }
        public byte Unknown_Dh { get; set; }
        public ushort Unknown_Eh { get; set; }
        public MetaHash Unknown_10h { get; set; }
        public uint Unknown_14h; // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public MetaHash Unknown_258h { get; set; }
        public uint Unknown_25Ch; // 0x00000000
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_270h; // 0x0000000000000000
        public ulong Unknown_278h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.DomainType = (ParticleDomainType)reader.ReadByte();
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
            this.Unknown_270h = reader.ReadUInt64();
            this.Unknown_278h = reader.ReadUInt64();

            if (Unknown_4h != 1)
            { }
            switch (Unknown_8h)
            {
                default:
                    break;
            }
            switch (Unknown_Dh)
            {
                default:
                    break;
            }
            switch (Unknown_Eh)
            {
                default:
                    break;
            }
            switch (Unknown_10h)
            {
                default:
                    break;
            }
            if (Unknown_14h != 0)
            { }
            switch (Unknown_258h)
            {
                default:
                    break;
            }
            if (Unknown_25Ch != 0)
            { }
            if (Unknown_270h != 0)
            { }
            if (Unknown_278h != 0)
            { }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write((byte)this.DomainType);
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
            writer.Write(this.Unknown_278h);
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
            var type = (ParticleDomainType)reader.ReadByte();
            reader.Position -= 13;

            switch (type)
            {
                case ParticleDomainType.Box: return new ParticleDomainBox();
                case ParticleDomainType.Sphere: return new ParticleDomainSphere();
                case ParticleDomainType.Cylinder: return new ParticleDomainCylinder();
                case ParticleDomainType.Attractor: return new ParticleDomainAttractor();
                default: return null;// throw new Exception("Unknown domain type");
            }
        }

    }

    [TC(typeof(EXP))] public class ParticleDomainBox : ParticleDomain
    {
        // ptxDomainBox
    }
    
    [TC(typeof(EXP))] public class ParticleDomainSphere : ParticleDomain
    {
        // ptxDomainSphere 
    }

    [TC(typeof(EXP))] public class ParticleDomainCylinder : ParticleDomain
    {
        // ptxDomainCylinder   
    }

    [TC(typeof(EXP))] public class ParticleDomainAttractor : ParticleDomain
    {
        // ptxDomainAttractor
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

    [TC(typeof(EXP))] public class ParticleBehaviour : ResourceSystemBlock, IResourceXXSystemBlock
    {
        // ptxBehaviour
        public override long BlockLength => 0x10;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ParticleBehaviourType Type { get; set; }
        public uint Unknown_Ch; // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Type = (ParticleBehaviourType)reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();

            if (Unknown_4h != 1)
            { }
            if (Unknown_Ch != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write((uint)this.Type);
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

    [TC(typeof(EXP))] public class ParticleBehaviourAge : ParticleBehaviour
    {
        // ptxu_Age
        public override long BlockLength => 0x30;

        // structure data
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();

            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourAcceleration : ParticleBehaviour
    {
        // ptxu_Acceleration
        public override long BlockLength => 0x170;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ulong Unknown_150h; // 0x0000000000000000
        public MetaHash Unknown_158h { get; set; }
        public MetaHash Unknown_15Ch { get; set; }
        public ulong Unknown_160h; // 0x0000000000000000
        public ulong Unknown_168h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt64();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt64();
            this.Unknown_168h = reader.ReadUInt64();

            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_150h != 0)
            { }
            switch (Unknown_158h)
            {
                default:
                    break;
            }
            switch (Unknown_15Ch)
            {
                default:
                    break;
            }
            if (Unknown_160h != 0)
            { }
            if (Unknown_168h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_168h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourVelocity : ParticleBehaviour
    {
        // ptxu_Velocity
        public override long BlockLength => 0x30;

        // structure data
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();

            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourRotation : ParticleBehaviour
    {
        // ptxu_Rotation
        public override long BlockLength => 0x280;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public MetaHash Unknown_270h { get; set; }
        public MetaHash Unknown_274h { get; set; }
        public MetaHash Unknown_278h { get; set; }
        public MetaHash Unknown_27Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_270h = reader.ReadUInt32();
            this.Unknown_274h = reader.ReadUInt32();
            this.Unknown_278h = reader.ReadUInt32();
            this.Unknown_27Ch = reader.ReadUInt32();

            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_270h)
            {
                default:
                    break;
            }
            switch (Unknown_274h)
            {
                default:
                    break;
            }
            switch (Unknown_278h)
            {
                default:
                    break;
            }
            switch (Unknown_27Ch)
            {
                default:
                    break;
            }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourSize : ParticleBehaviour
    {
        // ptxu_Size
        public override long BlockLength => 0x280;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public MetaHash Unknown_270h { get; set; }
        public MetaHash Unknown_274h { get; set; }
        public ulong Unknown_278h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_270h = reader.ReadUInt32();
            this.Unknown_274h = reader.ReadUInt32();
            this.Unknown_278h = reader.ReadUInt64();

            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_270h)
            {
                default:
                    break;
            }
            switch (Unknown_274h)
            {
                default:
                    break;
            }
            if (Unknown_278h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.Write(this.Unknown_270h);
            writer.Write(this.Unknown_274h);
            writer.Write(this.Unknown_278h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourDampening : ParticleBehaviour
    {
        // ptxu_Dampening
        public override long BlockLength => 0x170;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ulong Unknown_150h; // 0x0000000000000000
        public MetaHash Unknown_158h { get; set; }
        public uint Unknown_15Ch; // 0x00000000
        public ulong Unknown_160h; // 0x0000000000000000
        public ulong Unknown_168h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt64();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt64();
            this.Unknown_168h = reader.ReadUInt64();

            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_150h != 0)
            { }
            switch (Unknown_158h)
            {
                default:
                    break;
            }
            if (Unknown_15Ch != 0)
            { }
            if (Unknown_160h != 0)
            { }
            if (Unknown_168h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_168h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourMatrixWeight : ParticleBehaviour
    {
        // ptxu_MatrixWeight
        public override long BlockLength => 0xD0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public MetaHash Unknown_C0h { get; set; }
        public uint Unknown_C4h; // 0x00000000
        public ulong Unknown_C8h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_C0h = reader.ReadUInt32();
            this.Unknown_C4h = reader.ReadUInt32();
            this.Unknown_C8h = reader.ReadUInt64();


            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_C0h)
            {
                default:
                    break;
            }
            if (Unknown_C4h != 0)
            { }
            if (Unknown_C8h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.WriteBlock(this.KeyframeProp0);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C4h);
            writer.Write(this.Unknown_C8h);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0)
            };
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourCollision : ParticleBehaviour
    {
        // ptxu_Collision
        public override long BlockLength => 0x170;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public MetaHash Unknown_150h { get; set; }
        public MetaHash Unknown_154h { get; set; }
        public MetaHash Unknown_158h { get; set; }
        public MetaHash Unknown_15Ch { get; set; }
        public ulong Unknown_160h; // 0x0000000000000000
        public ulong Unknown_168h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt32();
            this.Unknown_154h = reader.ReadUInt32();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt64();
            this.Unknown_168h = reader.ReadUInt64();

            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_150h)
            {
                default:
                    break;
            }
            switch (Unknown_154h)
            {
                default:
                    break;
            }
            switch (Unknown_158h)
            {
                default:
                    break;
            }
            switch (Unknown_15Ch)
            {
                default:
                    break;
            }
            if (Unknown_160h != 0)
            { }
            if (Unknown_168h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_154h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_168h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourAnimateTexture : ParticleBehaviour
    {
        // ptxu_AnimateTexture
        public override long BlockLength => 0xD0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public MetaHash Unknown_C0h { get; set; }
        public MetaHash Unknown_C4h { get; set; }
        public MetaHash Unknown_C8h { get; set; }
        public MetaHash Unknown_CCh { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_C0h = reader.ReadUInt32();
            this.Unknown_C4h = reader.ReadUInt32();
            this.Unknown_C8h = reader.ReadUInt32();
            this.Unknown_CCh = reader.ReadUInt32();

            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_C0h)
            {
                default:
                    break;
            }
            switch (Unknown_C4h)
            {
                default:
                    break;
            }
            switch (Unknown_C8h)
            {
                default:
                    break;
            }
            switch (Unknown_CCh)
            {
                default:
                    break;
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourColour : ParticleBehaviour
    {
        // ptxu_Colour
        public override long BlockLength => 0x1F0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public MetaHash Unknown_1E0h { get; set; }
        public MetaHash Unknown_1E4h { get; set; }
        public ulong Unknown_1E8h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_1E0h = reader.ReadUInt32();
            this.Unknown_1E4h = reader.ReadUInt32();
            this.Unknown_1E8h = reader.ReadUInt64();

            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_1E0h)
            {
                default:
                    break;
            }
            switch (Unknown_1E4h)
            {
                default:
                    break;
            }
            if (Unknown_1E8h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.Write(this.Unknown_1E0h);
            writer.Write(this.Unknown_1E4h);
            writer.Write(this.Unknown_1E8h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourSprite : ParticleBehaviour
    {
        // ptxd_Sprite
        public override long BlockLength => 0x70;

        // structure data
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public MetaHash Unknown_30h { get; set; }
        public MetaHash Unknown_34h { get; set; }
        public MetaHash Unknown_38h { get; set; }
        public MetaHash Unknown_3Ch { get; set; }
        public MetaHash Unknown_40h { get; set; }
        public MetaHash Unknown_44h { get; set; }
        public MetaHash Unknown_48h { get; set; }
        public MetaHash Unknown_4Ch { get; set; }
        public MetaHash Unknown_50h { get; set; }
        public MetaHash Unknown_54h { get; set; }
        public MetaHash Unknown_58h { get; set; }
        public MetaHash Unknown_5Ch { get; set; }
        public MetaHash Unknown_60h { get; set; }
        public uint Unknown_64h; // 0x00000000
        public ulong Unknown_68h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
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
            this.Unknown_68h = reader.ReadUInt64();

            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_30h)
            {
                default:
                    break;
            }
            switch (Unknown_34h)
            {
                default:
                    break;
            }
            switch (Unknown_38h)
            {
                default:
                    break;
            }
            switch (Unknown_3Ch)
            {
                default:
                    break;
            }
            switch (Unknown_40h)
            {
                default:
                    break;
            }
            switch (Unknown_44h)
            {
                default:
                    break;
            }
            switch (Unknown_48h)
            {
                default:
                    break;
            }
            switch (Unknown_4Ch)
            {
                default:
                    break;
            }
            switch (Unknown_50h)
            {
                default:
                    break;
            }
            switch (Unknown_54h)
            {
                default:
                    break;
            }
            switch (Unknown_58h)
            {
                default:
                    break;
            }
            switch (Unknown_5Ch)
            {
                default:
                    break;
            }
            switch (Unknown_60h)
            {
                default:
                    break;
            }
            if (Unknown_64h != 0)
            { }
            if (Unknown_68h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
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
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourWind : ParticleBehaviour
    {
        // ptxu_Wind
        public override long BlockLength => 0xF0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ulong Unknown_C0h; // 0x0000000000000000
        public ulong Unknown_C8h; // 0x0000000000000000
        public MetaHash Unknown_D0h { get; set; }
        public MetaHash Unknown_D4h { get; set; }
        public MetaHash Unknown_D8h { get; set; }
        public MetaHash Unknown_DCh { get; set; }
        public MetaHash Unknown_E0h { get; set; }
        public uint Unknown_E4h; // 0x00000000
        public ulong Unknown_E8h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_C0h = reader.ReadUInt64();
            this.Unknown_C8h = reader.ReadUInt64();
            this.Unknown_D0h = reader.ReadUInt32();
            this.Unknown_D4h = reader.ReadUInt32();
            this.Unknown_D8h = reader.ReadUInt32();
            this.Unknown_DCh = reader.ReadUInt32();
            this.Unknown_E0h = reader.ReadUInt32();
            this.Unknown_E4h = reader.ReadUInt32();
            this.Unknown_E8h = reader.ReadUInt64();

            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_C0h != 0)
            { }
            if (Unknown_C8h != 0)
            { }
            switch (Unknown_D0h)
            {
                default:
                    break;
            }
            switch (Unknown_D4h)
            {
                default:
                    break;
            }
            switch (Unknown_D8h)
            {
                default:
                    break;
            }
            switch (Unknown_DCh)
            {
                default:
                    break;
            }
            switch (Unknown_E0h)
            {
                default:
                    break;
            }
            if (Unknown_E4h != 0)
            { }
            if (Unknown_E8h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.WriteBlock(this.KeyframeProp0);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C8h);
            writer.Write(this.Unknown_D0h);
            writer.Write(this.Unknown_D4h);
            writer.Write(this.Unknown_D8h);
            writer.Write(this.Unknown_DCh);
            writer.Write(this.Unknown_E0h);
            writer.Write(this.Unknown_E4h);
            writer.Write(this.Unknown_E8h);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, KeyframeProp0)
            };
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourLight : ParticleBehaviour
    {
        // ptxu_Light
        public override long BlockLength => 0x550;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public ParticleKeyframeProp KeyframeProp4 { get; set; }
        public ParticleKeyframeProp KeyframeProp5 { get; set; }
        public ParticleKeyframeProp KeyframeProp6 { get; set; }
        public ParticleKeyframeProp KeyframeProp7 { get; set; }
        public ParticleKeyframeProp KeyframeProp8 { get; set; }
        public MetaHash Unknown_540h { get; set; }
        public MetaHash Unknown_544h { get; set; }
        public MetaHash Unknown_548h { get; set; }
        public MetaHash Unknown_54Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
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


            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_540h)
            {
                default:
                    break;
            }
            switch (Unknown_544h)
            {
                default:
                    break;
            }
            switch (Unknown_548h)
            {
                default:
                    break;
            }
            switch (Unknown_54Ch)
            {
                default:
                    break;
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourModel : ParticleBehaviour
    {
        // ptxd_Model
        public override long BlockLength => 0x40;

        // structure data
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public uint Unknown_30h; // 0x00000000
        public MetaHash Unknown_34h { get; set; }
        public MetaHash Unknown_38h { get; set; }
        public uint Unknown_3Ch; // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();

            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_30h != 0)
            { }
            switch (Unknown_34h)
            {
                default:
                    break;
            }
            switch (Unknown_38h)
            {
                default:
                    break;
            }
            if (Unknown_3Ch != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourDecal : ParticleBehaviour
    {
        // ptxu_Decal
        public override long BlockLength => 0x180;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public MetaHash Unknown_150h { get; set; }
        public MetaHash Unknown_154h { get; set; }
        public MetaHash Unknown_158h { get; set; }
        public MetaHash Unknown_15Ch { get; set; }
        public MetaHash Unknown_160h { get; set; }
        public MetaHash Unknown_164h { get; set; }
        public MetaHash Unknown_168h { get; set; }
        public MetaHash Unknown_16Ch { get; set; }
        public MetaHash Unknown_170h { get; set; }
        public float Unknown_174h = 0.3f; // 0x3E99999A // 0.3f
        public float Unknown_178h = 1.0f; // 0x3F800000 // 1.0f
        public uint Unknown_17Ch; // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
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
            this.Unknown_174h = reader.ReadSingle();
            this.Unknown_178h = reader.ReadSingle();
            this.Unknown_17Ch = reader.ReadUInt32();

            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_150h)
            {
                default:
                    break;
            }
            switch (Unknown_154h)
            {
                default:
                    break;
            }
            switch (Unknown_158h)
            {
                default:
                    break;
            }
            switch (Unknown_15Ch)
            {
                default:
                    break;
            }
            switch (Unknown_160h)
            {
                default:
                    break;
            }
            switch (Unknown_164h)
            {
                default:
                    break;
            }
            switch (Unknown_168h)
            {
                default:
                    break;
            }
            switch (Unknown_16Ch)
            {
                default:
                    break;
            }
            switch (Unknown_170h)
            {
                default:
                    break;
            }
            if (Unknown_174h != 0.3f)
            { }
            if (Unknown_178h != 1.0f)
            { }
            if (Unknown_17Ch != 0)
            { }
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourZCull : ParticleBehaviour
    {
        // ptxu_ZCull
        public override long BlockLength => 0x170;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ulong Unknown_150h; // 0x0000000000000000
        public MetaHash Unknown_158h { get; set; }
        public MetaHash Unknown_15Ch { get; set; }
        public ulong Unknown_160h; // 0x0000000000000000
        public ulong Unknown_168h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt64();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt64();
            this.Unknown_168h = reader.ReadUInt64();

            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_150h != 0)
            { }
            switch (Unknown_158h)
            {
                default:
                    break;
            }
            switch (Unknown_15Ch)
            {
                default:
                    break;
            }
            if (Unknown_160h != 0)
            { }
            if (Unknown_168h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_168h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourNoise : ParticleBehaviour
    {
        // ptxu_Noise
        public override long BlockLength => 0x280;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public uint Unknown_270h; // 0x00000000
        public MetaHash Unknown_274h { get; set; }
        public ulong Unknown_278h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_270h = reader.ReadUInt32();
            this.Unknown_274h = reader.ReadUInt32();
            this.Unknown_278h = reader.ReadUInt64();


            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_270h != 0)
            { }
            switch (Unknown_274h)
            {
                default:
                    break;
            }
            if (Unknown_278h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.Write(this.Unknown_270h);
            writer.Write(this.Unknown_274h);
            writer.Write(this.Unknown_278h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourAttractor : ParticleBehaviour
    {
        // ptxu_Attractor
        public override long BlockLength => 0xC0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();


            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourTrail : ParticleBehaviour
    {
        // ptxd_Trail
        public override long BlockLength => 0xF0;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public uint Unknown_C0h; // 0x00000000
        public MetaHash Unknown_C4h { get; set; }
        public MetaHash Unknown_C8h { get; set; }
        public MetaHash Unknown_CCh { get; set; }
        public uint Unknown_D0h; // 0x00000000
        public MetaHash Unknown_D4h { get; set; }
        public MetaHash Unknown_D8h { get; set; }
        public MetaHash Unknown_DCh { get; set; }
        public MetaHash Unknown_E0h { get; set; }
        public MetaHash Unknown_E4h { get; set; }
        public uint Unknown_E8h; // 0x00000000
        public MetaHash Unknown_ECh { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
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

            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_C0h != 0)
            { }
            switch (Unknown_C4h)
            {
                default:
                    break;
            }
            switch (Unknown_C8h)
            {
                default:
                    break;
            }
            switch (Unknown_CCh)
            {
                default:
                    break;
            }
            if (Unknown_D0h != 0)
            { }
            switch (Unknown_D4h)
            {
                default:
                    break;
            }
            switch (Unknown_D8h)
            {
                default:
                    break;
            }
            switch (Unknown_DCh)
            {
                default:
                    break;
            }
            switch (Unknown_E0h)
            {
                default:
                    break;
            }
            switch (Unknown_E4h)
            {
                default:
                    break;
            }
            if (Unknown_E8h != 0)
            { }
            switch (Unknown_ECh)
            {
                default:
                    break;
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourFogVolume : ParticleBehaviour
    {
        // ptxu_FogVolume
        public override long BlockLength => 0x430;

        // structure data
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public ParticleKeyframeProp KeyframeProp4 { get; set; }
        public ParticleKeyframeProp KeyframeProp5 { get; set; }
        public ParticleKeyframeProp KeyframeProp6 { get; set; }
        public MetaHash Unknown_420h { get; set; }
        public MetaHash Unknown_424h { get; set; }
        public MetaHash Unknown_428h { get; set; }
        public MetaHash Unknown_42Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
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


            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            switch (Unknown_420h)
            {
                default:
                    break;
            }
            switch (Unknown_424h)
            {
                default:
                    break;
            }
            switch (Unknown_428h)
            {
                default:
                    break;
            }
            switch (Unknown_42Ch)
            {
                default:
                    break;
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourRiver : ParticleBehaviour
    {
        // ptxu_River
        public override long BlockLength => 0x40;

        // structure data
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public float Unknown_30h { get; set; } = 100.0f; // 0x42C80000 // 100.0f
        public MetaHash Unknown_34h { get; set; }
        public ulong Unknown_38h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt64();

            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_30h != 100.0f)
            { }
            switch (Unknown_34h)
            {
                default:
                    break;
            }
            if (Unknown_38h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourDecalPool : ParticleBehaviour
    {
        // ptxu_DecalPool
        public override long BlockLength => 0x50;

        // structure data
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public uint Unknown_30h; // 0x00000000
        public MetaHash Unknown_34h { get; set; }
        public MetaHash Unknown_38h { get; set; }
        public MetaHash Unknown_3Ch { get; set; }
        public MetaHash Unknown_40h { get; set; }
        public MetaHash Unknown_44h { get; set; }
        public ulong Unknown_48h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt64();

            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_30h != 0)
            { }
            switch (Unknown_34h)
            {
                default:
                    break;
            }
            switch (Unknown_38h)
            {
                default:
                    break;
            }
            switch (Unknown_3Ch)
            {
                default:
                    break;
            }
            switch (Unknown_40h)
            {
                default:
                    break;
            }
            switch (Unknown_44h)
            {
                default:
                    break;
            }
            if (Unknown_48h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourLiquid : ParticleBehaviour
    {
        // ptxu_Liquid
        public override long BlockLength => 0x50;

        // structure data
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public uint Unknown_30h; // 0x00000000
        public MetaHash Unknown_34h { get; set; }
        public MetaHash Unknown_38h { get; set; }
        public MetaHash Unknown_3Ch { get; set; }
        public MetaHash Unknown_40h { get; set; }
        public MetaHash Unknown_44h { get; set; }
        public MetaHash Unknown_48h { get; set; }
        public uint Unknown_4Ch; // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();

            if (Unknown_10h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            if (Unknown_20h != 0)
            { }
            if (Unknown_28h != 0)
            { }
            if (Unknown_30h != 0)
            { }
            switch (Unknown_34h)
            {
                default:
                    break;
            }
            switch (Unknown_38h)
            {
                default:
                    break;
            }
            switch (Unknown_3Ch)
            {
                default:
                    break;
            }
            switch (Unknown_40h)
            {
                default:
                    break;
            }
            switch (Unknown_44h)
            {
                default:
                    break;
            }
            switch (Unknown_48h)
            {
                default:
                    break;
            }
            if (Unknown_4Ch != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
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












    public enum ParticleShaderVarType : byte
    {
        Vector2 = 2,
        Vector4 = 4,
        Texture = 6,
        Keyframe = 7,
    }

    [TC(typeof(EXP))] public class ParticleShaderVar : ResourceSystemBlock, IResourceXXSystemBlock
    {
        // datBase
        // ptxShaderVar
        public override long BlockLength => 24;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public MetaHash Unknown_10h { get; set; }
        public ParticleShaderVarType Type { get; set; }
        public byte Unknown_15h { get; set; }
        public ushort Unknown_16h { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt32();
            this.Type = (ParticleShaderVarType)reader.ReadByte();
            this.Unknown_15h = reader.ReadByte();
            this.Unknown_16h = reader.ReadUInt16();

            if (Unknown_4h != 1)
            { }
            if (Unknown_8h != 0)
            { }
            switch (Unknown_10h)
            {
                default:
                    break;
            }
            switch (Unknown_15h)
            {
                default:
                    break;
            }
            switch (Unknown_16h)
            {
                default:
                    break;
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write((byte)this.Type);
            writer.Write(this.Unknown_15h);
            writer.Write(this.Unknown_16h);
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 20;
            var type = (ParticleShaderVarType)reader.ReadByte();
            reader.Position -= 21;

            switch (type)
            {
                case ParticleShaderVarType.Vector2:
                case ParticleShaderVarType.Vector4: return new ParticleShaderVarVector();
                case ParticleShaderVarType.Texture: return new ParticleShaderVarTexture();
                case ParticleShaderVarType.Keyframe: return new ParticleShaderVarKeyframe();
                default: return null;// throw new Exception("Unknown shader var type");
            }
        }
    }

    [TC(typeof(EXP))] public class ParticleShaderVarVector : ParticleShaderVar
    {
        // ptxShaderVarVector
        public override long BlockLength => 0x40;

        // structure data
        public MetaHash Unknown_18h { get; set; }
        public MetaHash Unknown_1Ch { get; set; }
        public MetaHash Unknown_20h { get; set; }
        public MetaHash Unknown_24h { get; set; }
        public MetaHash Unknown_28h { get; set; }
        public MetaHash Unknown_2Ch { get; set; }
        public MetaHash Unknown_30h { get; set; }
        public MetaHash Unknown_34h { get; set; }
        public MetaHash Unknown_38h { get; set; }
        public MetaHash Unknown_3Ch { get; set; }

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

            switch (Unknown_18h)
            {
                default:
                    break;
            }
            switch (Unknown_1Ch)
            {
                default:
                    break;
            }
            switch (Unknown_20h)
            {
                default:
                    break;
            }
            switch (Unknown_24h)
            {
                default:
                    break;
            }
            switch (Unknown_28h)
            {
                default:
                    break;
            }
            switch (Unknown_2Ch)
            {
                default:
                    break;
            }
            switch (Unknown_30h)
            {
                default:
                    break;
            }
            switch (Unknown_34h)
            {
                default:
                    break;
            }
            switch (Unknown_38h)
            {
                default:
                    break;
            }
            switch (Unknown_3Ch)
            {
                default:
                    break;
            }
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

    [TC(typeof(EXP))] public class ParticleShaderVarTexture : ParticleShaderVar
    {
        // ptxShaderVarTexture
        public override long BlockLength => 0x40;

        // structure data
        public MetaHash Unknown_18h { get; set; }
        public MetaHash Unknown_1Ch { get; set; }
        public MetaHash Unknown_20h { get; set; }
        public MetaHash Unknown_24h { get; set; }
        public ulong TexturePointer { get; set; }
        public ulong NamePointer { get; set; }
        public MetaHash NameHash { get; set; }
        public MetaHash Unknown_3Ch { get; set; }

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
            this.Texture = reader.ReadBlockAt<Texture>(this.TexturePointer);
            this.Name = reader.ReadBlockAt<string_r>(this.NamePointer);


            switch (Unknown_18h)
            {
                default:
                    break;
            }
            switch (Unknown_1Ch)
            {
                default:
                    break;
            }
            switch (Unknown_20h)
            {
                default:
                    break;
            }
            switch (Unknown_24h)
            {
                default:
                    break;
            }
            switch (NameHash)
            {
                default:
                    break;
            }
            switch (Unknown_3Ch)
            {
                default:
                    break;
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.TexturePointer = (ulong)(this.Texture != null ? this.Texture.FilePosition : 0);
            this.NamePointer = (ulong)(this.Name != null ? this.Name.FilePosition : 0);

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
            if (Texture != null) list.Add(Texture);
            if (Name != null) list.Add(Name);
            return list.ToArray();
        }
    }

    [TC(typeof(EXP))] public class ParticleShaderVarKeyframe : ParticleShaderVar
    {
        // ptxShaderVarKeyframe
        public override long BlockLength => 0x50;

        // structure data
        public MetaHash Unknown_18h { get; set; }
        public uint Unknown_1Ch = 1; // 0x00000001
        public ulong Unknown_20h; // 0x0000000000000000
        public ResourceSimpleList64<ParticleShaderVarKeyframeItem> Items { get; set; }
        public ulong Unknown_38h; // 0x0000000000000000
        public ulong Unknown_40h; // 0x0000000000000000
        public ulong Unknown_48h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt64();
            this.Items = reader.ReadBlock<ResourceSimpleList64<ParticleShaderVarKeyframeItem>>();
            this.Unknown_38h = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadUInt64();
            this.Unknown_48h = reader.ReadUInt64();

            switch (Unknown_18h)
            {
                default:
                    break;
            }
            if (Unknown_1Ch != 1)
            { }
            if (Unknown_20h != 0)
            { }
            if (Unknown_38h != 0)
            { }
            if (Unknown_40h != 0)
            { }
            if (Unknown_48h != 0)
            { }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.WriteBlock(this.Items);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_48h);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x28, Items)
            };
        }
    }

    [TC(typeof(EXP))] public class ParticleShaderVarKeyframeItem : ResourceSystemBlock
    {
        public override long BlockLength => 0x20;

        // structure data
        public MetaHash Unknown_0h { get; set; }
        public MetaHash Unknown_4h { get; set; }
        public ulong Unknown_8h; // 0x0000000000000000
        public MetaHash Unknown_10h { get; set; }
        public uint Unknown_14h; // 0x00000000
        public ulong Unknown_18h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt64();

            switch (Unknown_0h)
            {
                default:
                    break;
            }
            switch (Unknown_4h)
            {
                default:
                    break;
            }
            if (Unknown_8h != 0)
            { }
            switch (Unknown_10h)
            {
                default:
                    break;
            }
            if (Unknown_14h != 0)
            { }
            if (Unknown_18h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
        }
    }









}
