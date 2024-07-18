using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;


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
        public DrawablePtfxDictionary DrawableDictionary { get; set; }
        public ParticleRuleDictionary ParticleRuleDictionary { get; set; }
        public ParticleEffectRuleDictionary EffectRuleDictionary { get; set; }
        public ParticleEmitterRuleDictionary EmitterRuleDictionary { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            NamePointer = reader.ReadUInt64();
            Unknown_18h = reader.ReadUInt64();
            TextureDictionaryPointer = reader.ReadUInt64();
            Unknown_28h = reader.ReadUInt64();
            DrawableDictionaryPointer = reader.ReadUInt64();
            ParticleRuleDictionaryPointer = reader.ReadUInt64();
            Unknown_40h = reader.ReadUInt64();
            EmitterRuleDictionaryPointer = reader.ReadUInt64();
            EffectRuleDictionaryPointer = reader.ReadUInt64();
            Unknown_58h = reader.ReadUInt64();

            // read reference data
            Name = reader.ReadBlockAt<string_r>(NamePointer);
            TextureDictionary = reader.ReadBlockAt<TextureDictionary>(TextureDictionaryPointer);
            DrawableDictionary = reader.ReadBlockAt<DrawablePtfxDictionary>(DrawableDictionaryPointer);
            ParticleRuleDictionary = reader.ReadBlockAt<ParticleRuleDictionary>(ParticleRuleDictionaryPointer);
            EffectRuleDictionary = reader.ReadBlockAt<ParticleEffectRuleDictionary>(EmitterRuleDictionaryPointer);
            EmitterRuleDictionary = reader.ReadBlockAt<ParticleEmitterRuleDictionary>(EffectRuleDictionaryPointer);



            //if (Unknown_18h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
            //if (Unknown_40h != 0)
            //{ }//no hit
            //if (Unknown_58h != 0)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            NamePointer = (ulong)(Name != null ? Name.FilePosition : 0);
            TextureDictionaryPointer = (ulong)(TextureDictionary != null ? TextureDictionary.FilePosition : 0);
            DrawableDictionaryPointer = (ulong)(DrawableDictionary != null ? DrawableDictionary.FilePosition : 0);
            ParticleRuleDictionaryPointer = (ulong)(ParticleRuleDictionary != null ? ParticleRuleDictionary.FilePosition : 0);
            EmitterRuleDictionaryPointer = (ulong)(EffectRuleDictionary != null ? EffectRuleDictionary.FilePosition : 0);
            EffectRuleDictionaryPointer = (ulong)(EmitterRuleDictionary != null ? EmitterRuleDictionary.FilePosition : 0);

            // write structure data
            writer.Write(NamePointer);
            writer.Write(Unknown_18h);
            writer.Write(TextureDictionaryPointer);
            writer.Write(Unknown_28h);
            writer.Write(DrawableDictionaryPointer);
            writer.Write(ParticleRuleDictionaryPointer);
            writer.Write(Unknown_40h);
            writer.Write(EmitterRuleDictionaryPointer);
            writer.Write(EffectRuleDictionaryPointer);
            writer.Write(Unknown_58h);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            YptXml.StringTag(sb, indent, "Name", YptXml.XmlEscape(Name?.Value ?? ""));
            if (EffectRuleDictionary != null)
            {
                YptXml.OpenTag(sb, indent, "EffectRuleDictionary");
                EffectRuleDictionary.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "EffectRuleDictionary");
            }
            if (EmitterRuleDictionary != null)
            {
                YptXml.OpenTag(sb, indent, "EmitterRuleDictionary");
                EmitterRuleDictionary.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "EmitterRuleDictionary");
            }
            if (ParticleRuleDictionary != null)
            {
                YptXml.OpenTag(sb, indent, "ParticleRuleDictionary");
                ParticleRuleDictionary.WriteXml(sb, indent + 1, ddsfolder);
                YptXml.CloseTag(sb, indent, "ParticleRuleDictionary");
            }
            if (DrawableDictionary != null)
            {
                DrawablePtfxDictionary.WriteXmlNode(DrawableDictionary, sb, indent, ddsfolder, "DrawableDictionary");
            }
            if (TextureDictionary != null)
            {
                TextureDictionary.WriteXmlNode(TextureDictionary, sb, indent, ddsfolder, "TextureDictionary");
            }
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            Name = (string_r)Xml.GetChildInnerText(node, "Name");
            var efnode = node.SelectSingleNode("EffectRuleDictionary");
            if (efnode != null)
            {
                EffectRuleDictionary = new ParticleEffectRuleDictionary();
                EffectRuleDictionary.ReadXml(efnode);
            }
            var emnode = node.SelectSingleNode("EmitterRuleDictionary");
            if (emnode != null)
            {
                EmitterRuleDictionary = new ParticleEmitterRuleDictionary();
                EmitterRuleDictionary.ReadXml(emnode);
            }
            var ptnode = node.SelectSingleNode("ParticleRuleDictionary");
            if (ptnode != null)
            {
                ParticleRuleDictionary = new ParticleRuleDictionary();
                ParticleRuleDictionary.ReadXml(ptnode, ddsfolder);
            }
            var dnode = node.SelectSingleNode("DrawableDictionary");
            if (dnode != null)
            {
                DrawableDictionary = DrawablePtfxDictionary.ReadXmlNode(dnode, ddsfolder);
            }
            var tnode = node.SelectSingleNode("TextureDictionary");
            if (tnode != null)
            {
                TextureDictionary = TextureDictionary.ReadXmlNode(tnode, ddsfolder);
            }

            AssignChildren();
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


        public void AssignChildren()
        {
            //assigns any child references on objects that are stored in main dictionaries
            //but, build dictionaries first

            var texdict = new Dictionary<MetaHash, Texture>();
            if (TextureDictionary?.Dict != null)
            {
                foreach (var kvp in TextureDictionary.Dict)
                {
                    texdict[kvp.Key] = kvp.Value;
                }
            }

            var drwdict = new Dictionary<MetaHash, DrawablePtfx>();
            if (DrawableDictionary?.Drawables?.data_items != null)
            {
                var max = Math.Min(DrawableDictionary.Drawables.data_items.Length, (DrawableDictionary.Hashes?.Length ?? 0));
                for (int i = 0; i < max; i++)
                {
                    drwdict[DrawableDictionary.Hashes[i]] = DrawableDictionary.Drawables.data_items[i];
                }
            }

            var ptrdict = new Dictionary<MetaHash, ParticleRule>();
            if (ParticleRuleDictionary?.ParticleRules?.data_items != null)
            {
                foreach (var ptr in ParticleRuleDictionary.ParticleRules.data_items)
                {
                    ptrdict[ptr.NameHash] = ptr;
                }
            }

            var emrdict = new Dictionary<MetaHash, ParticleEmitterRule>();
            if (EmitterRuleDictionary?.EmitterRules?.data_items != null)
            {
                foreach (var emr in EmitterRuleDictionary.EmitterRules.data_items)
                {
                    emrdict[emr.NameHash] = emr;
                }
            }

            var efrdict = new Dictionary<MetaHash, ParticleEffectRule>();
            if (EffectRuleDictionary?.EffectRules?.data_items != null)
            {
                foreach (var efr in EffectRuleDictionary.EffectRules.data_items)
                {
                    efrdict[efr.NameHash] = efr;
                }
            }





            if (EffectRuleDictionary?.EffectRules?.data_items != null)
            {
                foreach (var efr in EffectRuleDictionary.EffectRules.data_items)
                {
                    if (efr?.EventEmitters?.data_items != null)
                    {
                        foreach (var em in efr.EventEmitters.data_items)
                        {
                            if (em == null) continue;
                            var ptrhash = JenkHash.GenHash(em.ParticleRuleName?.Value ?? "");
                            if (ptrdict.TryGetValue(ptrhash, out ParticleRule ptr))
                            {
                                em.ParticleRule = ptr;
                            }
                            else if (ptrhash != 0)
                            { }

                            var emrhash = JenkHash.GenHash(em.EmitterRuleName?.Value ?? "");
                            if (emrdict.TryGetValue(emrhash, out ParticleEmitterRule emr))
                            {
                                em.EmitterRule = emr;
                            }
                            else if (emrhash != 0)
                            { }

                        }
                    }
                }
            }

            if (ParticleRuleDictionary?.ParticleRules?.data_items != null)
            {
                foreach (var ptr in ParticleRuleDictionary.ParticleRules.data_items)
                {
                    if (ptr.EffectSpawnerAtRatio != null)
                    {
                        var efrhash = JenkHash.GenHash(ptr.EffectSpawnerAtRatio.EffectRuleName?.Value ?? "");
                        if (efrdict.TryGetValue(efrhash, out ParticleEffectRule efr))
                        {
                            ptr.EffectSpawnerAtRatio.EffectRule = efr;
                        }
                        else if (efrhash != 0)
                        { }
                    }
                    if (ptr.EffectSpawnerOnCollision != null)
                    {
                        var efrhash = JenkHash.GenHash(ptr.EffectSpawnerOnCollision.EffectRuleName?.Value ?? "");
                        if (efrdict.TryGetValue(efrhash, out ParticleEffectRule efr))
                        {
                            ptr.EffectSpawnerOnCollision.EffectRule = efr;
                        }
                        else if (efrhash != 0)
                        { }
                    }
                    if (ptr.Drawables?.data_items != null)
                    {
                        foreach (var pdrw in ptr.Drawables.data_items)
                        {
                            if (drwdict.TryGetValue(pdrw.NameHash, out DrawablePtfx drw))
                            {
                                pdrw.Drawable = drw;
                            }
                            else if (pdrw.NameHash != 0)
                            { }
                        }
                    }
                    if (ptr.ShaderVars?.data_items != null)
                    {
                        foreach (var svar in ptr.ShaderVars.data_items)
                        {
                            if (svar is ParticleShaderVarTexture texvar)
                            {
                                if (texdict.TryGetValue(texvar.TextureNameHash, out Texture tex))
                                {
                                    texvar.Texture = tex;
                                }
                                else if (texvar.TextureNameHash != 0)
                                { }
                            }
                        }
                    }
                }
            }


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
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h = 1; // 0x0000000000000001
        public ResourceSimpleList64_s<MetaHash> ParticleRuleNameHashes { get; set; }
        public ResourcePointerList64<ParticleRule> ParticleRules { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            VFT = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Unknown_8h = reader.ReadUInt64();
            Unknown_10h = reader.ReadUInt64();
            Unknown_18h = reader.ReadUInt64();
            ParticleRuleNameHashes = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            ParticleRules = reader.ReadBlock<ResourcePointerList64<ParticleRule>>();

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_18h != 1)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {

            // write structure data
            writer.Write(VFT);
            writer.Write(Unknown_4h);
            writer.Write(Unknown_8h);
            writer.Write(Unknown_10h);
            writer.Write(Unknown_18h);
            writer.WriteBlock(ParticleRuleNameHashes);
            writer.WriteBlock(ParticleRules);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            if (ParticleRules?.data_items != null)
            {
                var rules = ParticleRules.data_items.ToList();
                rules.Sort((a, b) => { return a.Name?.Value?.CompareTo(b.Name?.Value) ?? ((b.Name?.Value != null) ? 1 : 0); });
                foreach (var r in rules)
                {
                    YptXml.OpenTag(sb, indent, "Item");
                    r.WriteXml(sb, indent + 1, ddsfolder);
                    YptXml.CloseTag(sb, indent, "Item");
                }
            }
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            var rules = new List<ParticleRule>();
            var hashes = new List<MetaHash>();

            var inodes = node.SelectNodes("Item");
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var r = new ParticleRule();
                    r.ReadXml(inode, ddsfolder);
                    rules.Add(r);
                }
            }
            rules.Sort((a, b) => { return a.NameHash.Hash.CompareTo(b.NameHash.Hash); });
            foreach (var r in rules)
            {
                hashes.Add(r.NameHash);
            }

            ParticleRuleNameHashes = new ResourceSimpleList64_s<MetaHash>();
            ParticleRuleNameHashes.data_items = hashes.ToArray();
            ParticleRules = new ResourcePointerList64<ParticleRule>();
            ParticleRules.data_items = rules.ToArray();
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
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h = 1; // 0x0000000000000001
        public ResourceSimpleList64_s<MetaHash> EffectRuleNameHashes { get; set; }
        public ResourcePointerList64<ParticleEffectRule> EffectRules { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            VFT = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Unknown_8h = reader.ReadUInt64();
            Unknown_10h = reader.ReadUInt64();
            Unknown_18h = reader.ReadUInt64();
            EffectRuleNameHashes = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            EffectRules = reader.ReadBlock<ResourcePointerList64<ParticleEffectRule>>();

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_18h != 1)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(VFT);
            writer.Write(Unknown_4h);
            writer.Write(Unknown_8h);
            writer.Write(Unknown_10h);
            writer.Write(Unknown_18h);
            writer.WriteBlock(EffectRuleNameHashes);
            writer.WriteBlock(EffectRules);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            if (EffectRules?.data_items != null)
            {
                var rules = EffectRules.data_items.ToList();
                rules.Sort((a, b) => { return a.Name?.Value?.CompareTo(b.Name?.Value) ?? ((b.Name?.Value != null) ? 1 : 0); });
                foreach (var r in rules)
                {
                    YptXml.OpenTag(sb, indent, "Item");
                    r.WriteXml(sb, indent + 1);
                    YptXml.CloseTag(sb, indent, "Item");
                }
            }
        }
        public void ReadXml(XmlNode node)
        {
            var rules = new List<ParticleEffectRule>();
            var hashes = new List<MetaHash>();

            var inodes = node.SelectNodes("Item");
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var r = new ParticleEffectRule();
                    r.ReadXml(inode);
                    rules.Add(r);
                }
            }
            rules.Sort((a, b) => { return a.NameHash.Hash.CompareTo(b.NameHash.Hash); });
            foreach (var r in rules)
            {
                hashes.Add(r.NameHash);
            }

            EffectRuleNameHashes = new ResourceSimpleList64_s<MetaHash>();
            EffectRuleNameHashes.data_items = hashes.ToArray();
            EffectRules = new ResourcePointerList64<ParticleEffectRule>();
            EffectRules.data_items = rules.ToArray();
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
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h = 1; // 0x0000000000000001
        public ResourceSimpleList64_s<MetaHash> EmitterRuleNameHashes { get; set; }
        public ResourcePointerList64<ParticleEmitterRule> EmitterRules { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            VFT = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Unknown_8h = reader.ReadUInt64();
            Unknown_10h = reader.ReadUInt64();
            Unknown_18h = reader.ReadUInt64();
            EmitterRuleNameHashes = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            EmitterRules = reader.ReadBlock<ResourcePointerList64<ParticleEmitterRule>>();


            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_18h != 1)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {

            // write structure data
            writer.Write(VFT);
            writer.Write(Unknown_4h);
            writer.Write(Unknown_8h);
            writer.Write(Unknown_10h);
            writer.Write(Unknown_18h);
            writer.WriteBlock(EmitterRuleNameHashes);
            writer.WriteBlock(EmitterRules);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            if (EmitterRules?.data_items != null)
            {
                var rules = EmitterRules.data_items.ToList();
                rules.Sort((a, b) => { return a.Name?.Value?.CompareTo(b.Name?.Value) ?? ((b.Name?.Value != null) ? 1 : 0); });
                foreach (var r in rules)
                {
                    YptXml.OpenTag(sb, indent, "Item");
                    r.WriteXml(sb, indent + 1);
                    YptXml.CloseTag(sb, indent, "Item");
                }
            }
        }
        public void ReadXml(XmlNode node)
        {
            var rules = new List<ParticleEmitterRule>();
            var hashes = new List<MetaHash>();

            var inodes = node.SelectNodes("Item");
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var r = new ParticleEmitterRule();
                    r.ReadXml(inode);
                    rules.Add(r);
                }
            }
            rules.Sort((a, b) => { return a.NameHash.Hash.CompareTo(b.NameHash.Hash); });
            foreach (var r in rules)
            {
                hashes.Add(r.NameHash);
            }

            EmitterRuleNameHashes = new ResourceSimpleList64_s<MetaHash>();
            EmitterRuleNameHashes.data_items = hashes.ToArray();
            EmitterRules = new ResourcePointerList64<ParticleEmitterRule>();
            EmitterRules.data_items = rules.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, EmitterRuleNameHashes),
                new Tuple<long, IResourceBlock>(0x30, EmitterRules)
            };
        }
    }








    [TC(typeof(EXP))] public class ParticleRule : ResourceSystemBlock
    {
        // pgBase
        // pgBaseRefCounted
        public override long BlockLength => 0x240;

        // structure data
        public uint VFT { get; set; }
        public uint padding00 = 1;
        public ulong padding01;
        public uint RefCount { get; set; }
        public uint padding03;
        public ulong UIData;

        // ptxParticleRule
        public ParticleEffectSpawner EffectSpawnerAtRatio { get; set; }
        public ParticleEffectSpawner EffectSpawnerOnCollision { get; set; }

        // ptxRenderState
        public int CullMode { get; set; }
        public int BlendSet { get; set; }
        public int LightingMode { get; set; }
        public byte DepthWrite { get; set; }
        public byte DepthTest { get; set; }
        public byte AlphaBlend { get; set; }
        public byte padding04 { get; set; }
        public uint padding05 { get; set; }


        public float FileVersion { get; set; }
        public uint TexFrameIDMin { get; set; }
        public uint TexFrameIDMax { get; set; }
        public ulong NamePointer { get; set; }
        public ResourcePointerList64<ParticleBehaviour> AllBehaviours { get; set; }
        public ResourcePointerList64<ParticleBehaviour> InitBehaviours { get; set; }
        public ResourcePointerList64<ParticleBehaviour> UpdateBehaviours { get; set; }
        public ResourcePointerList64<ParticleBehaviour> UpdateFinalizeBehaviours { get; set; }
        public ResourcePointerList64<ParticleBehaviour> DrawBehaviours { get; set; }
        public ulong ReleaseBehaviours1 { get; set; }
        public ulong ReleaseBehaviours2 { get; set; }
        public ResourceSimpleList64<ParticleRuleBiasLink> BiasLinks { get; set; }
        public ulong PointPool { get; set; }
        public ulong FuncTable_UNUSED1 { get; set; }
        public ulong FuncTable_UNUSED2 { get; set; }

        // ShaderInst
        public uint VFT2 { get; set; } = 0x40605c50;
        public uint padding06 = 1;
        public ulong ShaderTemplateName { get; set; }
        public ulong ShaderTemplateTechniqueName { get; set; }
        public ulong ShaderTemplate { get; set; }
        public uint ShaderTemplateTechniqueID { get; set; }
        public uint padding07 { get; set; }

        // TechniqueDesc
        public uint VFT3 { get; set; } = 0x40605b48;
        public uint padding08 = 1;
        public uint DiffuseMode { get; set; }
        public uint ProjectionMode { get; set; }
        public byte IsLit { get; set; }
        public byte IsSoft { get; set; }
        public byte IsScreenSpace { get; set; }
        public byte IsRefract { get; set; }
        public byte IsNormalSpec { get; set; }
        public byte padding09 { get; set; }
        public short padding10 { get; set; }

        // InstVars
        public ResourcePointerList64<ParticleShaderVar> ShaderVars { get; set; }
        public byte IsDataInSync { get; set; }
        public byte padding11 { get; set; }
        public short padding12 { get; set; }
        public uint padding13 { get; set; }
        public MetaHash ShaderTemplateHashName { get; set; }
        public uint padding14 { get; set; }


        public ResourceSimpleList64<ParticleDrawable> Drawables { get; set; }
        public byte SortType { get; set; }
        public byte DrawType { get; set; }
        public byte Flags { get; set; }
        public byte RuntimeFlags { get; set; }
        public uint padding15 { get; set; }
        public ulong unused00 { get; set; }
        public ulong WindBehaviour { get; set; }
        public ulong padding16 { get; set; }

        // reference data
        public string_r Name { get; set; }
        public MetaHash NameHash { get; set; }
        public string_r ShaderFile { get; set; }
        public string_r ShaderTechnique { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {

            // read structure data
            VFT = reader.ReadUInt32();
            padding00 = reader.ReadUInt32();
            padding01 = reader.ReadUInt64();
            RefCount = reader.ReadUInt32();
            padding03 = reader.ReadUInt32();
            UIData = reader.ReadUInt64();

            EffectSpawnerAtRatio = reader.ReadBlock<ParticleEffectSpawner>();
            EffectSpawnerOnCollision = reader.ReadBlock<ParticleEffectSpawner>();


            CullMode = reader.ReadInt32();
            BlendSet = reader.ReadInt32();
            LightingMode = reader.ReadInt32();
            DepthWrite = reader.ReadByte();
            DepthTest = reader.ReadByte();
            AlphaBlend = reader.ReadByte();
            padding04 = reader.ReadByte();
            padding05 = reader.ReadUInt32();


            FileVersion = reader.ReadSingle();
            TexFrameIDMin = reader.ReadUInt32();
            TexFrameIDMax = reader.ReadUInt32();
            NamePointer = reader.ReadUInt64();
            AllBehaviours = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            InitBehaviours = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            UpdateBehaviours = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            UpdateFinalizeBehaviours = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            DrawBehaviours = reader.ReadBlock<ResourcePointerList64<ParticleBehaviour>>();
            ReleaseBehaviours1 = reader.ReadUInt64();
            ReleaseBehaviours2 = reader.ReadUInt64();
            BiasLinks = reader.ReadBlock<ResourceSimpleList64<ParticleRuleBiasLink>>();
            PointPool = reader.ReadUInt64();
            FuncTable_UNUSED1 = reader.ReadUInt64();
            FuncTable_UNUSED2 = reader.ReadUInt64();


            VFT2 = reader.ReadUInt32();
            padding06 = reader.ReadUInt32();
            ShaderTemplateName = reader.ReadUInt64();
            ShaderTemplateTechniqueName = reader.ReadUInt64();
            ShaderTemplate = reader.ReadUInt64();
            ShaderTemplateTechniqueID = reader.ReadUInt32();
            padding07 = reader.ReadUInt32();


            VFT3 = reader.ReadUInt32();
            padding08 = reader.ReadUInt32();
            DiffuseMode = reader.ReadUInt32();
            ProjectionMode = reader.ReadUInt32();
            IsLit = reader.ReadByte();
            IsSoft = reader.ReadByte();
            IsScreenSpace = reader.ReadByte();
            IsRefract = reader.ReadByte();
            IsNormalSpec = reader.ReadByte();
            padding09 = reader.ReadByte();
            padding10 = reader.ReadInt16();


            ShaderVars = reader.ReadBlock<ResourcePointerList64<ParticleShaderVar>>();
            IsDataInSync = reader.ReadByte();
            padding11 = reader.ReadByte();
            padding12 = reader.ReadInt16();
            padding13 = reader.ReadUInt32();
            ShaderTemplateHashName = reader.ReadUInt32();
            padding14 = reader.ReadUInt32();


            Drawables = reader.ReadBlock<ResourceSimpleList64<ParticleDrawable>>();
            SortType = reader.ReadByte();
            DrawType = reader.ReadByte();
            Flags = reader.ReadByte();
            RuntimeFlags = reader.ReadByte();
            padding15 = reader.ReadUInt32();
            unused00 = reader.ReadUInt64();
            WindBehaviour = reader.ReadUInt64();
            padding16 = reader.ReadUInt64();

            // read reference data
            Name = reader.ReadBlockAt<string_r>(NamePointer);
            ShaderFile = reader.ReadBlockAt<string_r>(ShaderTemplateName);
            ShaderTechnique = reader.ReadBlockAt<string_r>(ShaderTemplateTechniqueName);


            if (!string.IsNullOrEmpty(Name?.Value))
            {
                JenkIndex.Ensure(Name.Value);
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            NamePointer = (ulong)(Name != null ? Name.FilePosition : 0);
            ShaderTemplateName = (ulong)(ShaderFile != null ? ShaderFile.FilePosition : 0);
            ShaderTemplateTechniqueName = (ulong)(ShaderTechnique != null ? ShaderTechnique.FilePosition : 0);

            // write structure data
            writer.Write(VFT);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.Write(RefCount);
            writer.Write(padding03);
            writer.Write(UIData);


            writer.WriteBlock(EffectSpawnerAtRatio);
            writer.WriteBlock(EffectSpawnerOnCollision);


            writer.Write(CullMode);
            writer.Write(BlendSet);
            writer.Write(LightingMode);
            writer.Write(DepthWrite);
            writer.Write(DepthTest);
            writer.Write(AlphaBlend);
            writer.Write(padding04);
            writer.Write(padding05);


            writer.Write(FileVersion);
            writer.Write(TexFrameIDMin);
            writer.Write(TexFrameIDMax);
            writer.Write(NamePointer);
            writer.WriteBlock(AllBehaviours);
            writer.WriteBlock(InitBehaviours);
            writer.WriteBlock(UpdateBehaviours);
            writer.WriteBlock(UpdateFinalizeBehaviours);
            writer.WriteBlock(DrawBehaviours);
            writer.Write(ReleaseBehaviours1);
            writer.Write(ReleaseBehaviours2);
            writer.WriteBlock(BiasLinks);
            writer.Write(PointPool);
            writer.Write(FuncTable_UNUSED1);
            writer.Write(FuncTable_UNUSED2);


            writer.Write(VFT2);
            writer.Write(padding06);
            writer.Write(ShaderTemplateName);
            writer.Write(ShaderTemplateTechniqueName);
            writer.Write(ShaderTemplate);
            writer.Write(ShaderTemplateTechniqueID);
            writer.Write(padding07);


            writer.Write(VFT3);
            writer.Write(padding08);
            writer.Write(DiffuseMode);
            writer.Write(ProjectionMode);
            writer.Write(IsLit);
            writer.Write(IsSoft);
            writer.Write(IsScreenSpace);
            writer.Write(IsRefract);
            writer.Write(IsNormalSpec);
            writer.Write(padding09);
            writer.Write(padding10);


            writer.WriteBlock(ShaderVars);
            writer.Write(IsDataInSync);
            writer.Write(padding11);
            writer.Write(padding12);
            writer.Write(padding13);
            writer.Write(ShaderTemplateHashName);
            writer.Write(padding14);


            writer.WriteBlock(Drawables);
            writer.Write(SortType);
            writer.Write(DrawType);
            writer.Write(Flags);
            writer.Write(RuntimeFlags);
            writer.Write(padding15);
            writer.Write(unused00);
            writer.Write(WindBehaviour);
            writer.Write(padding16);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            YptXml.StringTag(sb, indent, "Name", YptXml.XmlEscape(Name?.Value ?? ""));
            YptXml.ValueTag(sb, indent, "RefCount", RefCount.ToString());
            YptXml.StringTag(sb, indent, "ShaderFile", YptXml.XmlEscape(ShaderFile?.Value ?? ""));
            YptXml.StringTag(sb, indent, "ShaderTechnique", YptXml.XmlEscape(ShaderTechnique?.Value ?? ""));
            YptXml.ValueTag(sb, indent, "CullMode", CullMode.ToString());
            YptXml.ValueTag(sb, indent, "BlendSet", BlendSet.ToString());
            YptXml.ValueTag(sb, indent, "LightingMode", LightingMode.ToString());
            YptXml.ValueTag(sb, indent, "DepthWrite", DepthWrite.ToString());
            YptXml.ValueTag(sb, indent, "DepthTest", DepthTest.ToString());
            YptXml.ValueTag(sb, indent, "AlphaBlend", AlphaBlend.ToString());
            YptXml.ValueTag(sb, indent, "TexFrameIDMin", TexFrameIDMin.ToString());
            YptXml.ValueTag(sb, indent, "TexFrameIDMax", TexFrameIDMax.ToString());
            YptXml.ValueTag(sb, indent, "ShaderTemplateTechniqueID", ShaderTemplateTechniqueID.ToString());
            YptXml.ValueTag(sb, indent, "DiffuseMode", DiffuseMode.ToString());
            YptXml.ValueTag(sb, indent, "ProjectionMode", ProjectionMode.ToString());
            YptXml.ValueTag(sb, indent, "IsLit", IsLit.ToString());
            YptXml.ValueTag(sb, indent, "IsSoft", IsSoft.ToString());
            YptXml.ValueTag(sb, indent, "IsScreenSpace", IsScreenSpace.ToString());
            YptXml.ValueTag(sb, indent, "IsRefract", IsRefract.ToString());
            YptXml.ValueTag(sb, indent, "IsNormalSpec", IsNormalSpec.ToString());
            YptXml.ValueTag(sb, indent, "SortType", SortType.ToString());
            YptXml.ValueTag(sb, indent, "DrawType", DrawType.ToString());
            YptXml.ValueTag(sb, indent, "Flags", Flags.ToString());
            YptXml.ValueTag(sb, indent, "RuntimeFlags", RuntimeFlags.ToString());
            if (EffectSpawnerAtRatio != null)
            {
                YptXml.OpenTag(sb, indent, "EffectSpawnerAtRatio");
                EffectSpawnerAtRatio.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "EffectSpawnerAtRatio");
            }
            if (EffectSpawnerOnCollision != null)
            {
                YptXml.OpenTag(sb, indent, "EffectSpawnerOnCollision");
                EffectSpawnerOnCollision.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "EffectSpawnerOnCollision");
            }
            if (AllBehaviours?.data_items?.Length > 0)
            {
                YptXml.WriteItemArray(sb, AllBehaviours.data_items, indent, "AllBehaviours");
            }
            if (BiasLinks?.data_items?.Length > 0)
            {
                YptXml.WriteItemArray(sb, BiasLinks.data_items, indent, "BiasLinks");
            }
            if (ShaderVars?.data_items?.Length > 0)
            {
                YptXml.WriteItemArray(sb, ShaderVars.data_items, indent, "ShaderVars");
            }
            if (Drawables?.data_items?.Length > 0)
            {
                YptXml.WriteItemArray(sb, Drawables.data_items, indent, "Drawables");
            }
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            Name = (string_r)Xml.GetChildInnerText(node, "Name"); if (Name.Value == null) Name = null;
            NameHash = JenkHash.GenHash(Name?.Value ?? "");
            RefCount = Xml.GetChildUIntAttribute(node, "RefCount");
            ShaderFile = (string_r)Xml.GetChildInnerText(node, "ShaderFile"); if (ShaderFile.Value == null) ShaderFile = null;
            ShaderTechnique = (string_r)Xml.GetChildInnerText(node, "ShaderTechnique"); if (ShaderTechnique.Value == null) ShaderTechnique = null;
            CullMode = Xml.GetChildIntAttribute(node, "CullMode");
            BlendSet = Xml.GetChildIntAttribute(node, "BlendSet");
            LightingMode = Xml.GetChildIntAttribute(node, "LightingMode");
            DepthWrite = (byte)Xml.GetChildUIntAttribute(node, "DepthWrite");
            DepthTest = (byte)Xml.GetChildUIntAttribute(node, "DepthTest");
            AlphaBlend = (byte)Xml.GetChildUIntAttribute(node, "AlphaBlend");
            TexFrameIDMin = Xml.GetChildUIntAttribute(node, "TexFrameIDMin");
            TexFrameIDMax = Xml.GetChildUIntAttribute(node, "TexFrameIDMax");
            ShaderTemplateTechniqueID = Xml.GetChildUIntAttribute(node, "ShaderTemplateTechniqueID");
            DiffuseMode = Xml.GetChildUIntAttribute(node, "DiffuseMode");
            ProjectionMode = Xml.GetChildUIntAttribute(node, "ProjectionMode");
            IsLit = (byte)Xml.GetChildUIntAttribute(node, "IsLit");
            IsSoft = (byte)Xml.GetChildUIntAttribute(node, "IsSoft");
            IsScreenSpace = (byte)Xml.GetChildUIntAttribute(node, "IsScreenSpace");
            IsRefract = (byte)Xml.GetChildUIntAttribute(node, "IsRefract");
            IsNormalSpec = (byte)Xml.GetChildUIntAttribute(node, "IsNormalSpec");
            SortType = (byte)Xml.GetChildUIntAttribute(node, "SortType");
            DrawType = (byte)Xml.GetChildUIntAttribute(node, "DrawType");
            Flags = (byte)Xml.GetChildUIntAttribute(node, "Flags");
            RuntimeFlags = (byte)Xml.GetChildUIntAttribute(node, "RuntimeFlags");
            EffectSpawnerAtRatio = new ParticleEffectSpawner();
            EffectSpawnerAtRatio.ReadXml(node.SelectSingleNode("EffectSpawnerAtRatio"));
            EffectSpawnerOnCollision = new ParticleEffectSpawner();
            EffectSpawnerOnCollision.ReadXml(node.SelectSingleNode("EffectSpawnerOnCollision"));



            var bnode = node.SelectSingleNode("AllBehaviours");
            var blist = new List<ParticleBehaviour>();
            if (bnode != null)
            {
                var inodes = bnode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    foreach (XmlNode inode in inodes)
                    {
                        var b = ParticleBehaviour.ReadXmlNode(inode);
                        blist.Add(b);
                    }
                }
            }
            BuildBehaviours(blist);




            BiasLinks = new ResourceSimpleList64<ParticleRuleBiasLink>();
            BiasLinks.data_items = XmlMeta.ReadItemArrayNullable<ParticleRuleBiasLink>(node, "BiasLinks");


            ResourcePointerList64<ParticleShaderVar> readShaderVars(string name)
            {
                var sha = new ResourcePointerList64<ParticleShaderVar>();
                var snode = node.SelectSingleNode(name);
                if (snode != null)
                {
                    var inodes = snode.SelectNodes("Item");
                    if (inodes?.Count > 0)
                    {
                        var slist = new List<ParticleShaderVar>();
                        foreach (XmlNode inode in inodes)
                        {
                            var s = ParticleShaderVar.ReadXmlNode(inode);
                            slist.Add(s);
                        }
                        sha.data_items = slist.ToArray();
                    }
                }
                return sha;
            }
            ShaderVars = readShaderVars("ShaderVars");


            Drawables = new ResourceSimpleList64<ParticleDrawable>();
            Drawables.data_items = XmlMeta.ReadItemArrayNullable<ParticleDrawable>(node, "Drawables");
        }


        public void BuildBehaviours(List<ParticleBehaviour> blist)
        {
            var blist2 = new List<ParticleBehaviour>();
            var blist3 = new List<ParticleBehaviour>();
            var blist4 = new List<ParticleBehaviour>();
            var blist5 = new List<ParticleBehaviour>();

            foreach (var b in blist)
            {
                if (b == null) continue;
                var render = false;
                var extra = false;
                var extra2 = false;
                switch (b.Type)
                {
                    case ParticleBehaviourType.Sprite:
                    case ParticleBehaviourType.Model:
                    case ParticleBehaviourType.Trail:
                        render = true;
                        break;
                }
                switch (b.Type)
                {
                    case ParticleBehaviourType.Collision:
                    case ParticleBehaviourType.Light:
                    case ParticleBehaviourType.Decal:
                    case ParticleBehaviourType.ZCull:
                    case ParticleBehaviourType.Trail:
                    case ParticleBehaviourType.FogVolume:
                    case ParticleBehaviourType.River:
                    case ParticleBehaviourType.DecalPool:
                    case ParticleBehaviourType.Liquid:
                        extra = true;
                        break;
                }
                switch (b.Type)
                {
                    case ParticleBehaviourType.Sprite:
                    case ParticleBehaviourType.Model:
                    case ParticleBehaviourType.Trail:
                    case ParticleBehaviourType.FogVolume:
                        extra2 = true;
                        break;
                }
                if (!render)
                {
                    blist2.Add(b);
                    blist3.Add(b);
                }
                if (extra)
                {
                    blist4.Add(b);
                }
                if (extra2)
                {
                    blist5.Add(b);
                }
            }

            AllBehaviours = new ResourcePointerList64<ParticleBehaviour>();
            AllBehaviours.data_items = blist.ToArray();
            InitBehaviours = new ResourcePointerList64<ParticleBehaviour>();
            InitBehaviours.data_items = blist2.ToArray();
            UpdateBehaviours = new ResourcePointerList64<ParticleBehaviour>();
            UpdateBehaviours.data_items = blist3.ToArray();
            UpdateFinalizeBehaviours = new ResourcePointerList64<ParticleBehaviour>();
            UpdateFinalizeBehaviours.data_items = blist4.ToArray();
            DrawBehaviours = new ResourcePointerList64<ParticleBehaviour>();
            DrawBehaviours.data_items = blist5.ToArray();


        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            if (ShaderFile != null) list.Add(ShaderFile);
            if (ShaderTechnique != null) list.Add(ShaderTechnique);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(88, EffectSpawnerAtRatio),
                new Tuple<long, IResourceBlock>(96, EffectSpawnerOnCollision),
                new Tuple<long, IResourceBlock>(0x128, AllBehaviours),
                new Tuple<long, IResourceBlock>(0x138, InitBehaviours),
                new Tuple<long, IResourceBlock>(0x148, UpdateBehaviours),
                new Tuple<long, IResourceBlock>(0x158, UpdateFinalizeBehaviours),
                new Tuple<long, IResourceBlock>(0x168, DrawBehaviours),
                new Tuple<long, IResourceBlock>(0x188, BiasLinks),
                new Tuple<long, IResourceBlock>(0x1F0, ShaderVars),
                new Tuple<long, IResourceBlock>(0x210, Drawables)
            };
        }

        public override string ToString()
        {
            return Name?.ToString() ?? base.ToString();
        }
    }


    [TC(typeof(EXP))] public class ParticleRuleBiasLink : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength => 0x58;




        // structure data
        public PsoChar32 Name { get; set; }
        public ulong padding00 { get; set; }
        public ulong padding01 { get; set; }
        public ulong padding02 { get; set; }
        public ulong padding03 { get; set; }
        public ResourceSimpleList64_s<MetaHash> KeyframePropIDs { get; set; }
        public byte RandomIndex { get; set; }
        public byte padding05 { get; set; }
        public short padding06 { get; set; }
        public uint padding07 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            Name = reader.ReadStruct<PsoChar32>();
            padding00 = reader.ReadUInt64();
            padding01 = reader.ReadUInt64();
            padding02 = reader.ReadUInt64();
            padding03 = reader.ReadUInt64();
            KeyframePropIDs = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            RandomIndex = reader.ReadByte();
            padding05 = reader.ReadByte();
            padding06 = reader.ReadInt16();
            padding07 = reader.ReadUInt32();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteStruct(Name);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.Write(padding02);
            writer.Write(padding03);
            writer.WriteBlock(KeyframePropIDs);
            writer.Write(RandomIndex);
            writer.Write(padding05);
            writer.Write(padding06);
            writer.Write(padding07);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", YptXml.XmlEscape(Name.ToString()));
            YptXml.ValueTag(sb, indent, "RandomIndex", RandomIndex.ToString());
            YptXml.WriteHashItemArray(sb, KeyframePropIDs?.data_items, indent, "KeyframePropIDs");
        }
        public void ReadXml(XmlNode node)
        {
            Name = new PsoChar32(Xml.GetChildInnerText(node, "Name"));
            RandomIndex = (byte)Xml.GetChildUIntAttribute(node, "RandomIndex");
            KeyframePropIDs = new ResourceSimpleList64_s<MetaHash>();
            KeyframePropIDs.data_items = XmlMeta.ReadHashItemArray(node, "KeyframePropIDs");
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x40, KeyframePropIDs)
            };
        }

        public override string ToString()
        {
            var n = Name.ToString();
            return (!string.IsNullOrEmpty(n)) ? n : base.ToString();
        }

    }


    [TC(typeof(EXP))] public class ParticleEffectSpawner : ResourceSystemBlock
    {
        // pgBase
        // ptxEffectSpawner
        public override long BlockLength => 0x70;

        // structure data
        public uint VFT { get; set; }
        public uint padding00 = 1;
        public ulong padding01 { get; set; }
        public ulong padding02 { get; set; }
        public float DurationScalarMin { get; set; }
        public float PlaybackRateScalarMin { get; set; }
        public uint ColourTintScalarMin { get; set; }
        public float ZoomScalarMin { get; set; }
        public uint FlagsMin { get; set; }
        public uint padding03 { get; set; }
        public ulong padding04 { get; set; }


        public float DurationScalarMax { get; set; }
        public float PlaybackRateScalarMax { get; set; }
        public uint ColourTintScalarMax { get; set; }
        public float ZoomScalarMax { get; set; }
        public uint FlagsMax { get; set; }
        public uint padding05 { get; set; }
        public ulong padding06 { get; set; }


        public ulong EffectRulePointer { get; set; }
        public ulong EffectRuleNamePointer { get; set; }
        public float TriggerInfo { get; set; }
        public byte InheritsPointLife { get; set; }
        public byte TracksPointPos { get; set; }
        public byte TracksPointDir { get; set; }
        public byte TracksPointNegDir { get; set; }

        // reference data
        public ParticleEffectRule EffectRule { get; set; }
        public string_r EffectRuleName { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            VFT = reader.ReadUInt32();
            padding00 = reader.ReadUInt32();
            padding01 = reader.ReadUInt64();
            padding02 = reader.ReadUInt64();
            DurationScalarMin = reader.ReadSingle();
            PlaybackRateScalarMin = reader.ReadSingle();
            ColourTintScalarMin = reader.ReadUInt32();
            ZoomScalarMin = reader.ReadSingle();
            FlagsMin = reader.ReadUInt32();
            padding03 = reader.ReadUInt32();
            padding04 = reader.ReadUInt64();
            DurationScalarMax = reader.ReadSingle();
            PlaybackRateScalarMax = reader.ReadSingle();
            ColourTintScalarMax = reader.ReadUInt32();
            ZoomScalarMax = reader.ReadSingle();
            FlagsMax = reader.ReadUInt32();
            padding05 = reader.ReadUInt32();
            padding06 = reader.ReadUInt64();
            EffectRulePointer = reader.ReadUInt64();
            EffectRuleNamePointer = reader.ReadUInt64();
            TriggerInfo = reader.ReadSingle();
            InheritsPointLife = reader.ReadByte();
            TracksPointPos = reader.ReadByte();
            TracksPointDir = reader.ReadByte();
            TracksPointNegDir = reader.ReadByte();

            // read reference data
            EffectRule = reader.ReadBlockAt<ParticleEffectRule>(EffectRulePointer);
            EffectRuleName = reader.ReadBlockAt<string_r>(EffectRuleNamePointer);
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            EffectRulePointer = (ulong)(EffectRule != null ? EffectRule.FilePosition : 0);
            EffectRuleNamePointer = (ulong)(EffectRuleName != null ? EffectRuleName.FilePosition : 0);

            // write structure data
            writer.Write(VFT);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.Write(padding02);
            writer.Write(DurationScalarMin);
            writer.Write(PlaybackRateScalarMin);
            writer.Write(ColourTintScalarMin);
            writer.Write(ZoomScalarMin);
            writer.Write(FlagsMin);
            writer.Write(padding03);
            writer.Write(padding04);
            writer.Write(DurationScalarMax);
            writer.Write(PlaybackRateScalarMax);
            writer.Write(ColourTintScalarMax);
            writer.Write(ZoomScalarMax);
            writer.Write(FlagsMax);
            writer.Write(padding05);
            writer.Write(padding06);
            writer.Write(EffectRulePointer);
            writer.Write(EffectRuleNamePointer);
            writer.Write(TriggerInfo);
            writer.Write(InheritsPointLife);
            writer.Write(TracksPointPos);
            writer.Write(TracksPointDir);
            writer.Write(TracksPointNegDir);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "EffectRule", EffectRule?.Name?.Value ?? "");
            YptXml.ValueTag(sb, indent, "DurationScalarMin", FloatUtil.ToString(DurationScalarMin));
            YptXml.ValueTag(sb, indent, "PlaybackRateScalarMin", FloatUtil.ToString(PlaybackRateScalarMin));
            YptXml.ValueTag(sb, indent, "ColourTintScalarMin", YptXml.UintString(ColourTintScalarMin));
            YptXml.ValueTag(sb, indent, "ZoomScalarMin", FloatUtil.ToString(ZoomScalarMin));
            YptXml.ValueTag(sb, indent, "FlagsMin", FlagsMin.ToString());
            YptXml.ValueTag(sb, indent, "DurationScalarMax", FloatUtil.ToString(DurationScalarMax));
            YptXml.ValueTag(sb, indent, "PlaybackRateScalarMax", FloatUtil.ToString(PlaybackRateScalarMax));
            YptXml.ValueTag(sb, indent, "ColourTintScalarMax", YptXml.UintString(ColourTintScalarMax));
            YptXml.ValueTag(sb, indent, "ZoomScalarMax", FloatUtil.ToString(ZoomScalarMax));
            YptXml.ValueTag(sb, indent, "FlagsMax", FlagsMax.ToString());
            YptXml.ValueTag(sb, indent, "TriggerInfo", FloatUtil.ToString(TriggerInfo));
            YptXml.ValueTag(sb, indent, "InheritsPointLife", InheritsPointLife.ToString());
            YptXml.ValueTag(sb, indent, "TracksPointPos", TracksPointPos.ToString());
            YptXml.ValueTag(sb, indent, "TracksPointDir", TracksPointDir.ToString());
            YptXml.ValueTag(sb, indent, "TracksPointNegDir", TracksPointNegDir.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            var ername = Xml.GetChildInnerText(node, "EffectRule");
            EffectRuleName = (string_r)(ername ?? "");
            DurationScalarMin = Xml.GetChildFloatAttribute(node, "DurationScalarMin");
            PlaybackRateScalarMin = Xml.GetChildFloatAttribute(node, "PlaybackRateScalarMin");
            ColourTintScalarMin = Xml.GetChildUIntAttribute(node, "ColourTintScalarMin");
            ZoomScalarMin = Xml.GetChildFloatAttribute(node, "ZoomScalarMin");
            FlagsMin = Xml.GetChildUIntAttribute(node, "FlagsMin");
            DurationScalarMax = Xml.GetChildFloatAttribute(node, "DurationScalarMax");
            PlaybackRateScalarMax = Xml.GetChildFloatAttribute(node, "PlaybackRateScalarMax");
            ColourTintScalarMax = Xml.GetChildUIntAttribute(node, "ColourTintScalarMax");
            ZoomScalarMax = Xml.GetChildFloatAttribute(node, "ZoomScalarMax");
            FlagsMax = Xml.GetChildUIntAttribute(node, "FlagsMax");
            TriggerInfo = Xml.GetChildFloatAttribute(node, "TriggerInfo");
            InheritsPointLife = (byte)Xml.GetChildUIntAttribute(node, "InheritsPointLife");
            TracksPointPos = (byte)Xml.GetChildUIntAttribute(node, "TracksPointPos");
            TracksPointDir = (byte)Xml.GetChildUIntAttribute(node, "TracksPointDir");
            TracksPointNegDir = (byte)Xml.GetChildUIntAttribute(node, "TracksPointNegDir");
        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (EffectRule != null) list.Add(EffectRule);
            if (EffectRuleName != null) list.Add(EffectRuleName);
            return list.ToArray();
        }

        public override string ToString()
        {
            var str = EffectRuleName?.ToString();
            return (!string.IsNullOrEmpty(str)) ? str : base.ToString();
        }
    }


    [TC(typeof(EXP))] public class ParticleDrawable : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength => 0x30;

        // structure data
        public float BoundBoxWidth { get; set; }
        public float BoundBoxHeight { get; set; }
        public float BoundBoxDepth { get; set; }
        public float BoundingSphereRadius { get; set; }
        public ulong NamePointer { get; set; }
        public ulong DrawablePointer { get; set; }
        public MetaHash NameHash { get; set; }
        public uint padding00 { get; set; }
        public ulong padding01 { get; set; }

        // reference data
        public string_r Name { get; set; }
        public DrawablePtfx Drawable { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            BoundBoxWidth = reader.ReadSingle();
            BoundBoxHeight = reader.ReadSingle();
            BoundBoxDepth = reader.ReadSingle();
            BoundingSphereRadius = reader.ReadSingle();
            NamePointer = reader.ReadUInt64();
            DrawablePointer = reader.ReadUInt64();
            NameHash = reader.ReadUInt32();
            padding00 = reader.ReadUInt32();
            padding01 = reader.ReadUInt64();

            // read reference data
            Name = reader.ReadBlockAt<string_r>(NamePointer);
            Drawable = reader.ReadBlockAt<DrawablePtfx>(DrawablePointer);

            if (!string.IsNullOrEmpty(Name?.Value))
            {
                JenkIndex.Ensure(Name.Value);
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            NamePointer = (ulong)(Name != null ? Name.FilePosition : 0);
            DrawablePointer = (ulong)(Drawable != null ? Drawable.FilePosition : 0);

            // write structure data
            writer.Write(BoundBoxWidth);
            writer.Write(BoundBoxHeight);
            writer.Write(BoundBoxDepth);
            writer.Write(BoundingSphereRadius);
            writer.Write(NamePointer);
            writer.Write(DrawablePointer);
            writer.Write(NameHash);
            writer.Write(padding00);
            writer.Write(padding01);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", YptXml.XmlEscape(Name?.Value ?? ""));
            YptXml.ValueTag(sb, indent, "BoundBoxWidth", FloatUtil.ToString(BoundBoxWidth));
            YptXml.ValueTag(sb, indent, "BoundBoxHeight", FloatUtil.ToString(BoundBoxHeight));
            YptXml.ValueTag(sb, indent, "BoundBoxDepth", FloatUtil.ToString(BoundBoxDepth));
            YptXml.ValueTag(sb, indent, "BoundingSphereRadius", FloatUtil.ToString(BoundingSphereRadius));
            if (Drawable != null)
            {
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name = (string_r)Xml.GetChildInnerText(node, "Name"); if (Name.Value == null) Name = null;
            NameHash = JenkHash.GenHash(Name?.Value ?? "");
            BoundBoxWidth = Xml.GetChildFloatAttribute(node, "BoundBoxWidth");
            BoundBoxHeight = Xml.GetChildFloatAttribute(node, "BoundBoxHeight");
            BoundBoxDepth = Xml.GetChildFloatAttribute(node, "BoundBoxDepth");
            BoundingSphereRadius = Xml.GetChildFloatAttribute(node, "BoundingSphereRadius");
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            if (Drawable != null) list.Add(Drawable);
            return list.ToArray();
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name?.Value)) return Name.Value;
            if (NameHash != 0) return NameHash.ToString();
            return base.ToString();
        }

    }










    [TC(typeof(EXP))] public class ParticleEffectRule : ResourceSystemBlock
    {
        public override long BlockLength => 0x3C0;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1;
        public ulong Unknown_8h { get; set; }
        public ulong RefCount { get; set; }

        // ptxEffectRule
        public float FileVersion { get; set; }
        public uint padding0 { get; set; }
        public ulong NamePointer { get; set; }
        public ulong EffectList { get; set; } = 0x0000000050000000;

        // pfxTimeline
        public uint VFT2 { get; set; } = 0x4060e3e8;
        public uint unused00 = 1;
        public ulong EventEmittersPointer { get; set; }
        public ushort EventEmittersCount { get; set; }
        public ushort EventEmittersCapacity { get; set; } = 32; //always 32
        public uint Unused01 { get; set; }


        public ulong EvolutionListPointer { get; set; }
        public int NumLoops { get; set; }
        public byte SortEventsByDistance { get; set; }
        public byte DrawListID { get; set; }
        public byte IsShortLived { get; set; }
        public byte HasNoShadows { get; set; }
        public ulong padding00 { get; set; }
        public Vector3 VRandomOffsetPos { get; set; }
        public uint padding01 { get; set; }
        public float PreUpdateTime { get; set; }
        public float PreUpdateTimeInterval { get; set; }
        public float DurationMin { get; set; }
        public float DurationMax { get; set; }
        public float PlaybackRateScalarMin { get; set; }
        public float PlaybackRateScalarMax { get; set; }
        public byte ViewportCullingMode { get; set; }
        public byte RenderWhenViewportCulled { get; set; }
        public byte UpdateWhenViewportCulled { get; set; }
        public byte EmitWhenViewportCulled { get; set; }
        public byte DistanceCullingMode { get; set; }
        public byte RenderWhenDistanceCulled { get; set; }
        public byte UpdateWhenDistanceCulled { get; set; }
        public byte EmitWhenDistanceCulled { get; set; }
        public Vector3 ViewportCullingSphereOffset { get; set; }
        public uint padding02 { get; set; } = 0x7f800001;
        public float ViewportCullingSphereRadius { get; set; }
        public float DistanceCullingFadeDist { get; set; }
        public float DistanceCullingCullDist { get; set; }
        public float LodEvoDistanceMin { get; set; }
        public float LodEvoDistanceMax { get; set; }
        public float CollisionRange { get; set; }
        public float CollisionProbeDistance { get; set; }
        public byte CollisionType { get; set; }
        public byte ShareEntityCollisions { get; set; }
        public byte OnlyUseBVHCollisions { get; set; }
        public byte GameFlags { get; set; }
        public ParticleKeyframeProp ColourTintMinKFP { get; set; }
        public ParticleKeyframeProp ColourTintMaxKFP { get; set; }
        public ParticleKeyframeProp ZoomScalarKFP { get; set; }
        public ParticleKeyframeProp DataSphereKFP { get; set; }
        public ParticleKeyframeProp DataCapsuleKFP { get; set; }
        public ulong KeyframePropsPointer { get; set; } // KeyframePropList
        public ushort KeyframePropsCount { get; set; } = 5; //always 5
        public ushort KeyframePropsCapacity { get; set; } = 16; //always 16
        public uint unused02 { get; set; }
        public byte ColourTintMaxEnable { get; set; }
        public byte UseDataVolume { get; set; }
        public byte DataVolumeType { get; set; }
        public byte padding03 { get; set; }
        public uint NumActiveInstances { get; set; }
        public float ZoomLevel { get; set; }
        public uint padding04 { get; set; }
        public ulong padding05 { get; set; }
        public ulong padding06 { get; set; }

        // reference data
        public string_r Name { get; set; }
        public MetaHash NameHash { get; set; }
        public ResourcePointerArray64<ParticleEventEmitter> EventEmitters { get; set; }
        public ParticleEvolutionList EvolutionList { get; set; }
        public ResourcePointerArray64<ParticleKeyframeProp> KeyframeProps { get; set; } // these just point to the 5x embedded KeyframeProps, padded to 16 items


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            #region read

            // read structure data
            VFT = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Unknown_8h = reader.ReadUInt64();
            RefCount = reader.ReadUInt64();
            FileVersion = reader.ReadSingle();
            padding0 = reader.ReadUInt32();
            NamePointer = reader.ReadUInt64();
            EffectList = reader.ReadUInt64();
            VFT2 = reader.ReadUInt32();
            unused00 = reader.ReadUInt32();
            EventEmittersPointer = reader.ReadUInt64();
            EventEmittersCount = reader.ReadUInt16();
            EventEmittersCapacity = reader.ReadUInt16();
            Unused01 = reader.ReadUInt32();
            EvolutionListPointer = reader.ReadUInt64();
            NumLoops = reader.ReadInt32();
            SortEventsByDistance = reader.ReadByte();
            DrawListID = reader.ReadByte();
            IsShortLived = reader.ReadByte();
            HasNoShadows = reader.ReadByte();
            padding00 = reader.ReadUInt64();
            VRandomOffsetPos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            padding01 = reader.ReadUInt32();
            PreUpdateTime = reader.ReadSingle();
            PreUpdateTimeInterval = reader.ReadSingle();
            DurationMin = reader.ReadSingle();
            DurationMax = reader.ReadSingle();
            PlaybackRateScalarMin = reader.ReadSingle();
            PlaybackRateScalarMax = reader.ReadSingle();
            ViewportCullingMode = reader.ReadByte();
            RenderWhenViewportCulled = reader.ReadByte();
            UpdateWhenViewportCulled = reader.ReadByte();
            EmitWhenViewportCulled = reader.ReadByte();
            DistanceCullingMode = reader.ReadByte();
            RenderWhenDistanceCulled = reader.ReadByte();
            UpdateWhenDistanceCulled = reader.ReadByte();
            EmitWhenDistanceCulled = reader.ReadByte();
            ViewportCullingSphereOffset = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            padding02 = reader.ReadUInt32();
            ViewportCullingSphereRadius = reader.ReadSingle();
            DistanceCullingFadeDist = reader.ReadSingle();
            DistanceCullingCullDist = reader.ReadSingle();
            LodEvoDistanceMin = reader.ReadSingle();
            LodEvoDistanceMax = reader.ReadSingle();
            CollisionRange = reader.ReadSingle();
            CollisionProbeDistance = reader.ReadSingle();
            CollisionType = reader.ReadByte();
            ShareEntityCollisions = reader.ReadByte();
            OnlyUseBVHCollisions = reader.ReadByte();
            GameFlags = reader.ReadByte();
            ColourTintMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            ColourTintMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            ZoomScalarKFP = reader.ReadBlock<ParticleKeyframeProp>();
            DataSphereKFP = reader.ReadBlock<ParticleKeyframeProp>();
            DataCapsuleKFP = reader.ReadBlock<ParticleKeyframeProp>();
            KeyframePropsPointer = reader.ReadUInt64();
            KeyframePropsCount = reader.ReadUInt16();
            KeyframePropsCapacity = reader.ReadUInt16();
            unused02 = reader.ReadUInt32();
            ColourTintMaxEnable = reader.ReadByte();
            UseDataVolume = reader.ReadByte();
            DataVolumeType = reader.ReadByte();
            padding03 = reader.ReadByte();
            NumActiveInstances = reader.ReadUInt32();
            ZoomLevel = reader.ReadSingle();
            padding04 = reader.ReadUInt32();
            padding05 = reader.ReadUInt64();
            padding06 = reader.ReadUInt64();

            // read reference data
            Name = reader.ReadBlockAt<string_r>(NamePointer);
            EventEmitters = reader.ReadBlockAt<ResourcePointerArray64<ParticleEventEmitter>>(EventEmittersPointer, EventEmittersCapacity);
            EvolutionList = reader.ReadBlockAt<ParticleEvolutionList>(EvolutionListPointer);
            KeyframeProps = reader.ReadBlockAt<ResourcePointerArray64<ParticleKeyframeProp>>(KeyframePropsPointer, KeyframePropsCapacity);

            if (!string.IsNullOrEmpty(Name?.Value))
            {
                JenkIndex.Ensure(Name.Value);
                NameHash = JenkHash.GenHash(Name.Value);
            }

            #endregion
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            NamePointer = (ulong)(Name != null ? Name.FilePosition : 0);
            EventEmittersPointer = (ulong)(EventEmitters != null ? EventEmitters.FilePosition : 0);
            EvolutionListPointer = (ulong)(EvolutionList != null ? EvolutionList.FilePosition : 0);
            KeyframePropsPointer = (ulong)(KeyframeProps != null ? KeyframeProps.FilePosition : 0);

            // write structure data
            writer.Write(VFT);
            writer.Write(Unknown_4h);
            writer.Write(Unknown_8h);
            writer.Write(RefCount);
            writer.Write(FileVersion);
            writer.Write(padding0);
            writer.Write(NamePointer);
            writer.Write(EffectList);
            writer.Write(VFT2);
            writer.Write(unused00);
            writer.Write(EventEmittersPointer);
            writer.Write(EventEmittersCount);
            writer.Write(EventEmittersCapacity);
            writer.Write(Unused01);
            writer.Write(EvolutionListPointer);
            writer.Write(NumLoops);
            writer.Write(SortEventsByDistance);
            writer.Write(DrawListID);
            writer.Write(IsShortLived);
            writer.Write(HasNoShadows);
            writer.Write(padding00);
            writer.Write(VRandomOffsetPos);
            writer.Write(padding01);
            writer.Write(PreUpdateTime);
            writer.Write(PreUpdateTimeInterval);
            writer.Write(DurationMin);
            writer.Write(DurationMax);
            writer.Write(PlaybackRateScalarMin);
            writer.Write(PlaybackRateScalarMax);
            writer.Write(ViewportCullingMode);
            writer.Write(RenderWhenViewportCulled);
            writer.Write(UpdateWhenViewportCulled);
            writer.Write(EmitWhenViewportCulled);
            writer.Write(DistanceCullingMode);
            writer.Write(RenderWhenDistanceCulled);
            writer.Write(UpdateWhenDistanceCulled);
            writer.Write(EmitWhenDistanceCulled);
            writer.Write(ViewportCullingSphereOffset);
            writer.Write(padding02);
            writer.Write(ViewportCullingSphereRadius);
            writer.Write(DistanceCullingFadeDist);
            writer.Write(DistanceCullingCullDist);
            writer.Write(LodEvoDistanceMin);
            writer.Write(LodEvoDistanceMax);
            writer.Write(CollisionRange);
            writer.Write(CollisionProbeDistance);
            writer.Write(CollisionType);
            writer.Write(ShareEntityCollisions);
            writer.Write(OnlyUseBVHCollisions);
            writer.Write(GameFlags);
            writer.WriteBlock(ColourTintMinKFP);
            writer.WriteBlock(ColourTintMaxKFP);
            writer.WriteBlock(ZoomScalarKFP);
            writer.WriteBlock(DataSphereKFP);
            writer.WriteBlock(DataCapsuleKFP);
            writer.Write(KeyframePropsPointer);
            writer.Write(KeyframePropsCount);
            writer.Write(KeyframePropsCapacity);
            writer.Write(unused02);
            writer.Write(ColourTintMaxEnable);
            writer.Write(UseDataVolume);
            writer.Write(DataVolumeType);
            writer.Write(padding03);
            writer.Write(NumActiveInstances);
            writer.Write(ZoomLevel);
            writer.Write(padding04);
            writer.Write(padding05);
            writer.Write(padding06);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", YptXml.XmlEscape(Name?.Value ?? ""));
            YptXml.ValueTag(sb, indent, "RefCount", RefCount.ToString());
            YptXml.ValueTag(sb, indent, "FileVersion", FloatUtil.ToString(FileVersion));
            YptXml.ValueTag(sb, indent, "NumLoops", YptXml.UintString((uint)NumLoops));
            YptXml.ValueTag(sb, indent, "SortEventsByDistance", FloatUtil.ToString(SortEventsByDistance));
            YptXml.ValueTag(sb, indent, "DrawListID", FloatUtil.ToString(DrawListID));
            YptXml.ValueTag(sb, indent, "IsShortLived", FloatUtil.ToString(IsShortLived));
            YptXml.ValueTag(sb, indent, "HasNoShadows", FloatUtil.ToString(HasNoShadows));
            RelXml.SelfClosingTag(sb, indent, "VRandomOffsetPos " + FloatUtil.GetVector3XmlString(VRandomOffsetPos));
            YptXml.ValueTag(sb, indent, "PreUpdateTime", FloatUtil.ToString(PreUpdateTime));
            YptXml.ValueTag(sb, indent, "PreUpdateTimeInterval", FloatUtil.ToString(PreUpdateTimeInterval));
            YptXml.ValueTag(sb, indent, "DurationMin", FloatUtil.ToString(DurationMin));
            YptXml.ValueTag(sb, indent, "DurationMax", FloatUtil.ToString(DurationMax));
            YptXml.ValueTag(sb, indent, "PlaybackRateScalarMin", FloatUtil.ToString(PlaybackRateScalarMin));
            YptXml.ValueTag(sb, indent, "PlaybackRateScalarMax", FloatUtil.ToString(PlaybackRateScalarMax));
            YptXml.ValueTag(sb, indent, "ViewportCullingMode", FloatUtil.ToString(ViewportCullingMode));
            YptXml.ValueTag(sb, indent, "RenderWhenViewportCulled", FloatUtil.ToString(RenderWhenViewportCulled));
            YptXml.ValueTag(sb, indent, "UpdateWhenViewportCulled", FloatUtil.ToString(UpdateWhenViewportCulled));
            YptXml.ValueTag(sb, indent, "EmitWhenViewportCulled", FloatUtil.ToString(EmitWhenViewportCulled));
            YptXml.ValueTag(sb, indent, "DistanceCullingMode", FloatUtil.ToString(DistanceCullingMode));
            YptXml.ValueTag(sb, indent, "RenderWhenDistanceCulled", FloatUtil.ToString(RenderWhenDistanceCulled));
            YptXml.ValueTag(sb, indent, "UpdateWhenDistanceCulled", FloatUtil.ToString(UpdateWhenDistanceCulled));
            YptXml.ValueTag(sb, indent, "EmitWhenDistanceCulled", FloatUtil.ToString(EmitWhenDistanceCulled));
            RelXml.SelfClosingTag(sb, indent, "ViewportCullingSphereOffset " + FloatUtil.GetVector3XmlString(ViewportCullingSphereOffset));
            YptXml.ValueTag(sb, indent, "ViewportCullingSphereRadius", FloatUtil.ToString(ViewportCullingSphereRadius));
            YptXml.ValueTag(sb, indent, "DistanceCullingFadeDist", FloatUtil.ToString(DistanceCullingFadeDist));
            YptXml.ValueTag(sb, indent, "DistanceCullingCullDist", FloatUtil.ToString(DistanceCullingCullDist));
            YptXml.ValueTag(sb, indent, "LodEvoDistanceMin", FloatUtil.ToString(LodEvoDistanceMin));
            YptXml.ValueTag(sb, indent, "LodEvoDistanceMax", FloatUtil.ToString(LodEvoDistanceMax));
            YptXml.ValueTag(sb, indent, "CollisionRange", FloatUtil.ToString(CollisionRange));
            YptXml.ValueTag(sb, indent, "CollisionProbeDistance", FloatUtil.ToString(CollisionProbeDistance));
            YptXml.ValueTag(sb, indent, "CollisionType", FloatUtil.ToString(CollisionType));
            YptXml.ValueTag(sb, indent, "ShareEntityCollisions", FloatUtil.ToString(ShareEntityCollisions));
            YptXml.ValueTag(sb, indent, "OnlyUseBVHCollisions", FloatUtil.ToString(OnlyUseBVHCollisions));
            YptXml.ValueTag(sb, indent, "GameFlags", FloatUtil.ToString(GameFlags));
            YptXml.ValueTag(sb, indent, "ColourTintMaxEnable", FloatUtil.ToString(ColourTintMaxEnable));
            YptXml.ValueTag(sb, indent, "UseDataVolume", FloatUtil.ToString(UseDataVolume));
            YptXml.ValueTag(sb, indent, "DataVolumeType", FloatUtil.ToString(DataVolumeType));
            YptXml.ValueTag(sb, indent, "ZoomLevel", FloatUtil.ToString(ZoomLevel));
            if (EventEmitters?.data_items != null)
            {
                var ee = new ParticleEventEmitter[EventEmittersCount];//trim the unused items from this array
                Array.Copy(EventEmitters.data_items, 0, ee, 0, EventEmittersCount);
                YptXml.WriteItemArray(sb, ee, indent, "EventEmitters");
            }
            if (KeyframeProps?.data_items != null)
            {
                var kp = new ParticleKeyframeProp[KeyframePropsCount];//trim the unused items from this array
                Array.Copy(KeyframeProps.data_items, 0, kp, 0, KeyframePropsCount);
                YptXml.WriteItemArray(sb, kp, indent, "KeyframeProps");
            }
            if (EvolutionList != null)
            {
                YptXml.OpenTag(sb, indent, "EvolutionList");
                EvolutionList.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "EvolutionList");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name = (string_r)Xml.GetChildInnerText(node, "Name"); if (Name.Value == null) Name = null;
            NameHash = JenkHash.GenHash(Name?.Value ?? "");
            RefCount = Xml.GetChildUIntAttribute(node, "RefCount");
            FileVersion = Xml.GetChildFloatAttribute(node, "FileVersion");
            NumLoops = (int)Xml.GetChildUIntAttribute(node, "NumLoops");
            SortEventsByDistance = (byte)Xml.GetChildFloatAttribute(node, "SortEventsByDistance");
            DrawListID = (byte)Xml.GetChildFloatAttribute(node, "DrawListID");
            IsShortLived = (byte)Xml.GetChildFloatAttribute(node, "IsShortLived");
            HasNoShadows = (byte)Xml.GetChildFloatAttribute(node, "HasNoShadows");
            VRandomOffsetPos = Xml.GetChildVector3Attributes(node, "VRandomOffsetPos");
            PreUpdateTime = Xml.GetChildFloatAttribute(node, "PreUpdateTime");
            PreUpdateTimeInterval = Xml.GetChildFloatAttribute(node, "PreUpdateTimeInterval");
            DurationMin = Xml.GetChildFloatAttribute(node, "DurationMin");
            DurationMax = Xml.GetChildFloatAttribute(node, "DurationMax");
            PlaybackRateScalarMin = Xml.GetChildFloatAttribute(node, "PlaybackRateScalarMin");
            PlaybackRateScalarMax = Xml.GetChildFloatAttribute(node, "PlaybackRateScalarMax");
            ViewportCullingMode = (byte)Xml.GetChildFloatAttribute(node, "ViewportCullingMode");
            RenderWhenViewportCulled = (byte)Xml.GetChildFloatAttribute(node, "RenderWhenViewportCulled");
            UpdateWhenViewportCulled = (byte)Xml.GetChildFloatAttribute(node, "UpdateWhenViewportCulled");
            EmitWhenViewportCulled = (byte)Xml.GetChildFloatAttribute(node, "EmitWhenViewportCulled");
            DistanceCullingMode = (byte)Xml.GetChildFloatAttribute(node, "DistanceCullingMode");
            RenderWhenDistanceCulled = (byte)Xml.GetChildFloatAttribute(node, "RenderWhenDistanceCulled");
            UpdateWhenDistanceCulled = (byte)Xml.GetChildFloatAttribute(node, "UpdateWhenDistanceCulled");
            EmitWhenDistanceCulled = (byte)Xml.GetChildFloatAttribute(node, "EmitWhenDistanceCulled");
            ViewportCullingSphereOffset = Xml.GetChildVector3Attributes(node, "ViewportCullingSphereOffset");
            ViewportCullingSphereRadius = Xml.GetChildFloatAttribute(node, "ViewportCullingSphereRadius");
            DistanceCullingFadeDist = Xml.GetChildFloatAttribute(node, "DistanceCullingFadeDist");
            DistanceCullingCullDist = Xml.GetChildFloatAttribute(node, "DistanceCullingCullDist");
            LodEvoDistanceMin = Xml.GetChildFloatAttribute(node, "LodEvoDistanceMin");
            LodEvoDistanceMax = Xml.GetChildFloatAttribute(node, "LodEvoDistanceMax");
            CollisionRange = Xml.GetChildFloatAttribute(node, "CollisionRange");
            CollisionProbeDistance = Xml.GetChildFloatAttribute(node, "CollisionProbeDistance");
            CollisionType = (byte)Xml.GetChildFloatAttribute(node, "CollisionType");
            ShareEntityCollisions = (byte)Xml.GetChildFloatAttribute(node, "ShareEntityCollisions");
            OnlyUseBVHCollisions = (byte)Xml.GetChildFloatAttribute(node, "OnlyUseBVHCollisions");
            GameFlags = (byte)Xml.GetChildFloatAttribute(node, "GameFlags");
            ColourTintMaxEnable = (byte)Xml.GetChildFloatAttribute(node, "ColourTintMaxEnable");
            UseDataVolume = (byte)Xml.GetChildFloatAttribute(node, "UseDataVolume");
            DataVolumeType = (byte)Xml.GetChildFloatAttribute(node, "DataVolumeType");
            ZoomLevel = Xml.GetChildFloatAttribute(node, "ZoomLevel");

            var emlist = XmlMeta.ReadItemArray<ParticleEventEmitter>(node, "EventEmitters")?.ToList() ?? new List<ParticleEventEmitter>();
            EventEmittersCount = (ushort)emlist.Count;
            for (int i = emlist.Count; i < 32; i++) emlist.Add(null);
            EventEmitters = new ResourcePointerArray64<ParticleEventEmitter>();
            EventEmitters.data_items = emlist.ToArray();
            for (int i = 0; i < (EventEmitters.data_items?.Length ?? 0); i++)
            {
                if (EventEmitters.data_items[i] != null)
                {
                    EventEmitters.data_items[i].Index = (uint)i;
                }
            }


            var kflist = XmlMeta.ReadItemArray<ParticleKeyframeProp>(node, "KeyframeProps")?.ToList() ?? new List<ParticleKeyframeProp>();
            ColourTintMinKFP = (kflist.Count > 0) ? kflist[0] : new ParticleKeyframeProp();
            ColourTintMaxKFP = (kflist.Count > 1) ? kflist[1] : new ParticleKeyframeProp();
            ZoomScalarKFP = (kflist.Count > 2) ? kflist[2] : new ParticleKeyframeProp();
            DataSphereKFP = (kflist.Count > 3) ? kflist[3] : new ParticleKeyframeProp();
            DataCapsuleKFP = (kflist.Count > 4) ? kflist[4] : new ParticleKeyframeProp();
            for (int i = kflist.Count; i < 16; i++) kflist.Add(null);
            KeyframeProps = new ResourcePointerArray64<ParticleKeyframeProp>();
            KeyframeProps.data_items = kflist.ToArray();
            KeyframeProps.ManualReferenceOverride = true;
            KeyframePropsCount = 5;//this should always be 5.......
            KeyframePropsCapacity = 16;//should always be 16...

            var udnode = node.SelectSingleNode("EvolutionList");
            if (udnode != null)
            {
                EvolutionList = new ParticleEvolutionList();
                EvolutionList.ReadXml(udnode);
            }

        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            if (EventEmitters != null) list.Add(EventEmitters);
            if (EvolutionList != null) list.Add(EvolutionList);
            if (KeyframeProps != null)
            {
                KeyframeProps.ManualReferenceOverride = true;
                list.Add(KeyframeProps);
            }
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(192, ColourTintMinKFP),
                new Tuple<long, IResourceBlock>(336, ColourTintMaxKFP),
                new Tuple<long, IResourceBlock>(480, ZoomScalarKFP),
                new Tuple<long, IResourceBlock>(624, DataSphereKFP),
                new Tuple<long, IResourceBlock>(768, DataCapsuleKFP)
            };
        }

        public override string ToString()
        {
            return Name?.ToString() ?? base.ToString();
        }
    }


    [TC(typeof(EXP))] public class ParticleEventEmitter : ResourceSystemBlock, IMetaXmlItem
    {
        // ptxEvent
        // ptxEventEmitter
        public override long BlockLength => 0x70;

        // structure data
        public uint VFT { get; set; }
        public uint padding00 = 1;
        public uint Index { get; set; }
        public uint EventType { get; set; }
        public float StartRatio { get; set; }
        public float EndRatio { get; set; }
        public ulong EvolutionListPointer { get; set; }
        public ulong Unknown_20h { get; set; }
        public ulong Unknown_28h { get; set; }
        public ulong EmitterRuleNamePointer { get; set; }
        public ulong ParticleRuleNamePointer { get; set; }
        public ulong EmitterRulePointer { get; set; }
        public ulong ParticleRulePointer { get; set; }
        public float PlaybackRateScalarMin { get; set; }
        public float PlaybackRateScalarMax { get; set; }
        public float ZoomScalarMin { get; set; }
        public float ZoomScalarMax { get; set; }
        public uint ColourTintMin { get; set; }
        public uint ColourTintMax { get; set; }
        public ulong padding04 { get; set; }

        // reference data
        public ParticleEvolutionList EvolutionList { get; set; }
        public string_r EmitterRuleName { get; set; }
        public string_r ParticleRuleName { get; set; }
        public ParticleEmitterRule EmitterRule { get; set; }
        public ParticleRule ParticleRule { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            VFT = reader.ReadUInt32();
            padding00 = reader.ReadUInt32();
            Index = reader.ReadUInt32();
            EventType = reader.ReadUInt32();
            StartRatio = reader.ReadSingle();
            EndRatio = reader.ReadSingle();
            EvolutionListPointer = reader.ReadUInt64();
            Unknown_20h = reader.ReadUInt64();
            Unknown_28h = reader.ReadUInt64();
            EmitterRuleNamePointer = reader.ReadUInt64();
            ParticleRuleNamePointer = reader.ReadUInt64();
            EmitterRulePointer = reader.ReadUInt64();
            ParticleRulePointer = reader.ReadUInt64();
            PlaybackRateScalarMin = reader.ReadSingle();
            PlaybackRateScalarMax = reader.ReadSingle();
            ZoomScalarMin = reader.ReadSingle();
            ZoomScalarMax = reader.ReadSingle();
            ColourTintMin = reader.ReadUInt32();
            ColourTintMax = reader.ReadUInt32();
            padding04 = reader.ReadUInt64();

            // read reference data
            EvolutionList = reader.ReadBlockAt<ParticleEvolutionList>(EvolutionListPointer);
            EmitterRuleName = reader.ReadBlockAt<string_r>(EmitterRuleNamePointer);
            ParticleRuleName = reader.ReadBlockAt<string_r>(ParticleRuleNamePointer);
            EmitterRule = reader.ReadBlockAt<ParticleEmitterRule>(EmitterRulePointer);
            ParticleRule = reader.ReadBlockAt<ParticleRule>(ParticleRulePointer);


            if (!string.IsNullOrEmpty(EmitterRuleName?.Value))
            {
                JenkIndex.Ensure(EmitterRuleName.Value);
            }
            if (!string.IsNullOrEmpty(ParticleRuleName?.Value))
            {
                JenkIndex.Ensure(ParticleRuleName.Value);
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            EvolutionListPointer = (ulong)(EvolutionList != null ? EvolutionList.FilePosition : 0);
            EmitterRuleNamePointer = (ulong)(EmitterRuleName != null ? EmitterRuleName.FilePosition : 0);
            ParticleRuleNamePointer = (ulong)(ParticleRuleName != null ? ParticleRuleName.FilePosition : 0);
            EmitterRulePointer = (ulong)(EmitterRule != null ? EmitterRule.FilePosition : 0);
            ParticleRulePointer = (ulong)(ParticleRule != null ? ParticleRule.FilePosition : 0);

            // write structure data
            writer.Write(VFT);
            writer.Write(padding00);
            writer.Write(Index);
            writer.Write(EventType);
            writer.Write(StartRatio);
            writer.Write(EndRatio);
            writer.Write(EvolutionListPointer);
            writer.Write(Unknown_20h);
            writer.Write(Unknown_28h);
            writer.Write(EmitterRuleNamePointer);
            writer.Write(ParticleRuleNamePointer);
            writer.Write(EmitterRulePointer);
            writer.Write(ParticleRulePointer);
            writer.Write(PlaybackRateScalarMin);
            writer.Write(PlaybackRateScalarMax);
            writer.Write(ZoomScalarMin);
            writer.Write(ZoomScalarMax);
            writer.Write(ColourTintMin);
            writer.Write(ColourTintMax);
            writer.Write(padding04);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "EmitterRule", YptXml.XmlEscape(EmitterRuleName?.Value ?? ""));
            YptXml.StringTag(sb, indent, "ParticleRule", YptXml.XmlEscape(ParticleRuleName?.Value ?? ""));
            YptXml.ValueTag(sb, indent, "EventType", EventType.ToString());
            YptXml.ValueTag(sb, indent, "StartRatio", FloatUtil.ToString(StartRatio));
            YptXml.ValueTag(sb, indent, "EndRatio", FloatUtil.ToString(EndRatio));
            YptXml.ValueTag(sb, indent, "PlaybackRateScalarMin", FloatUtil.ToString(PlaybackRateScalarMin));
            YptXml.ValueTag(sb, indent, "PlaybackRateScalarMax", FloatUtil.ToString(PlaybackRateScalarMax));
            YptXml.ValueTag(sb, indent, "ZoomScalarMin", FloatUtil.ToString(ZoomScalarMin));
            YptXml.ValueTag(sb, indent, "ZoomScalarMax", FloatUtil.ToString(ZoomScalarMax));
            YptXml.ValueTag(sb, indent, "ColourTintMin", YptXml.UintString(ColourTintMin));
            YptXml.ValueTag(sb, indent, "ColourTintMax", YptXml.UintString(ColourTintMax));
            if (EvolutionList != null)
            {
                YptXml.OpenTag(sb, indent, "EvolutionList");
                EvolutionList.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "EvolutionList");
            }
        }
        public void ReadXml(XmlNode node)
        {
            EmitterRuleName = (string_r)Xml.GetChildInnerText(node, "EmitterRule"); if (EmitterRuleName.Value == null) EmitterRuleName = null;
            ParticleRuleName = (string_r)Xml.GetChildInnerText(node, "ParticleRule"); if (ParticleRuleName.Value == null) ParticleRuleName = null;
            EventType = Xml.GetChildUIntAttribute(node, "EventType");
            StartRatio = Xml.GetChildFloatAttribute(node, "StartRatio");
            EndRatio = Xml.GetChildFloatAttribute(node, "EndRatio");
            PlaybackRateScalarMin = Xml.GetChildFloatAttribute(node, "PlaybackRateScalarMin");
            PlaybackRateScalarMax = Xml.GetChildFloatAttribute(node, "PlaybackRateScalarMax");
            ZoomScalarMin = Xml.GetChildFloatAttribute(node, "ZoomScalarMin");
            ZoomScalarMax = Xml.GetChildFloatAttribute(node, "ZoomScalarMax");
            ColourTintMin = Xml.GetChildUIntAttribute(node, "ColourTintMin");
            ColourTintMax = Xml.GetChildUIntAttribute(node, "ColourTintMax");
            var udnode = node.SelectSingleNode("EvolutionList");
            if (udnode != null)
            {
                EvolutionList = new ParticleEvolutionList();
                EvolutionList.ReadXml(udnode);
            }
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (EvolutionList != null) list.Add(EvolutionList);
            if (EmitterRuleName != null) list.Add(EmitterRuleName);
            if (ParticleRuleName != null) list.Add(ParticleRuleName);
            if (EmitterRule != null) list.Add(EmitterRule);
            if (ParticleRule != null) list.Add(ParticleRule);
            return list.ToArray();
        }

        public override string ToString()
        {
            return EmitterRuleName?.ToString() ?? ParticleRuleName?.ToString() ?? base.ToString();
        }

    }


    [TC(typeof(EXP))] public class ParticleEvolutionList : ResourceSystemBlock
    {
        public override long BlockLength => 0x40;

        // structure data
        public ResourceSimpleList64<ParticleEvolutions> Evolutions { get; set; }
        public ResourceSimpleList64<ParticleEvolvedKeyframeProps> EvolvedKeyframeProps { get; set; }
        public ulong Unknown_20h = 1;
        public ResourceSimpleList64<ParticleEvolvedKeyframePropMap> EvolvedKeyframePropMap { get; set; }
        public ulong Unknown_38h;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            Evolutions = reader.ReadBlock<ResourceSimpleList64<ParticleEvolutions>>();
            EvolvedKeyframeProps = reader.ReadBlock<ResourceSimpleList64<ParticleEvolvedKeyframeProps>>();
            Unknown_20h = reader.ReadUInt64();
            EvolvedKeyframePropMap = reader.ReadBlock<ResourceSimpleList64<ParticleEvolvedKeyframePropMap>>();
            Unknown_38h = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(Evolutions);
            writer.WriteBlock(EvolvedKeyframeProps);
            writer.Write(Unknown_20h);
            writer.WriteBlock(EvolvedKeyframePropMap);
            writer.Write(Unknown_38h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            if (Evolutions?.data_items != null)
            {
                if (Evolutions.data_items.Length > 0)
                {
                    YptXml.OpenTag(sb, indent, "Evolutions");
                    foreach (var item in Evolutions.data_items)
                    {
                        YptXml.StringTag(sb, indent + 1, "Item", YptXml.XmlEscape(item?.Name?.Value ?? ""));
                    }
                    YptXml.CloseTag(sb, indent, "Evolutions");
                }
                else
                {
                    YptXml.SelfClosingTag(sb, indent, "Evolutions");
                }
            }
            if (EvolvedKeyframeProps?.data_items != null)
            {
                YptXml.WriteItemArray(sb, EvolvedKeyframeProps.data_items, indent, "EvolvedKeyframeProps");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Evolutions = new ResourceSimpleList64<ParticleEvolutions>();
            var unode = node.SelectSingleNode("Evolutions");
            if (unode != null)
            {
                var inodes = unode.SelectNodes("Item");
                var ilist = new List<ParticleEvolutions>();
                foreach (XmlNode inode in inodes)
                {
                    var iname = inode.InnerText;
                    var blk = new ParticleEvolutions();
                    blk.Name = (string_r)iname;
                    ilist.Add(blk);
                }
                Evolutions.data_items = ilist.ToArray();
            }

            EvolvedKeyframeProps = new ResourceSimpleList64<ParticleEvolvedKeyframeProps>();
            EvolvedKeyframeProps.data_items = XmlMeta.ReadItemArray<ParticleEvolvedKeyframeProps>(node, "EvolvedKeyframeProps");

            EvolvedKeyframePropMap = new ResourceSimpleList64<ParticleEvolvedKeyframePropMap>();
            if (EvolvedKeyframeProps.data_items != null)
            {
                var blist = new List<ParticleEvolvedKeyframePropMap>();
                foreach (var item in EvolvedKeyframeProps.data_items)
                {
                    var blk = new ParticleEvolvedKeyframePropMap();
                    blk.Item = item;
                    blk.Name = item.Name;
                    blist.Add(blk);
                }
                blist.Sort((a, b) => a.Name.Hash.CompareTo(b.Name.Hash));
                EvolvedKeyframePropMap.data_items = blist.ToArray();
            }
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, Evolutions),
                new Tuple<long, IResourceBlock>(0x10, EvolvedKeyframeProps),
                new Tuple<long, IResourceBlock>(0x28, EvolvedKeyframePropMap)
            };
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }


    [TC(typeof(EXP))] public class ParticleEvolutions : ResourceSystemBlock
    {
        public override long BlockLength => 24;

        // structure data
        public ulong NamePointer { get; set; }
        public ulong padding00 { get; set; }
        public ulong padding01 { get; set; }

        // reference data
        public string_r Name { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            NamePointer = reader.ReadUInt64();
            padding00 = reader.ReadUInt64();
            padding01 = reader.ReadUInt64();

            // read reference data
            Name = reader.ReadBlockAt<string_r>(NamePointer);
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            NamePointer = (ulong)(Name != null ? Name.FilePosition : 0);

            // write structure data
            writer.Write(NamePointer);
            writer.Write(padding00);
            writer.Write(padding01);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            return list.ToArray();
        }

        public override string ToString()
        {
            return Name?.ToString() ?? base.ToString();
        }
    }


    [TC(typeof(EXP))] public class ParticleEvolvedKeyframePropMap : ResourceSystemBlock
    {
        public override long BlockLength => 0x10;

        // structure data
        public ParticleKeyframePropName Name { get; set; }
        public uint Unknown_4h; // 0x00000000
        public ulong ItemPointer { get; set; }

        // reference data
        public ParticleEvolvedKeyframeProps Item { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            Name = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            ItemPointer = reader.ReadUInt64();

            // read reference data
            Item = reader.ReadBlockAt<ParticleEvolvedKeyframeProps>(ItemPointer);

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            ItemPointer = (ulong)(Item != null ? Item.FilePosition : 0);

            // write structure data
            writer.Write(Name);
            writer.Write(Unknown_4h);
            writer.Write(ItemPointer);
        }

        public override string ToString()
        {
            return Name.ToString();
        }

    }


    [TC(typeof(EXP))] public class ParticleEvolvedKeyframeProps : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength => 24;

        // structure data
        public ResourceSimpleList64<ParticleEvolvedKeyframes> EvolvedKeyframes { get; set; }
        public ParticleKeyframePropName Name { get; set; }
        public uint BlendMode { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            EvolvedKeyframes = reader.ReadBlock<ResourceSimpleList64<ParticleEvolvedKeyframes>>();
            Name = reader.ReadUInt32();
            BlendMode = reader.ReadUInt32();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(EvolvedKeyframes);
            writer.Write(Name);
            writer.Write(BlendMode);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", Name.ToString());
            YptXml.ValueTag(sb, indent, "BlendMode", BlendMode.ToString());
            if (EvolvedKeyframes?.data_items != null)
            {
                YptXml.WriteItemArray(sb, EvolvedKeyframes.data_items, indent, "Items");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            BlendMode = Xml.GetChildUIntAttribute(node, "BlendMode");
            EvolvedKeyframes = new ResourceSimpleList64<ParticleEvolvedKeyframes>();
            EvolvedKeyframes.data_items = XmlMeta.ReadItemArray<ParticleEvolvedKeyframes>(node, "Items");
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, EvolvedKeyframes)
            };
        }

        public override string ToString()
        {
            return Name.ToString();
        }

    }


    [TC(typeof(EXP))] public class ParticleEvolvedKeyframes : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength => 0x30;

        // structure data
        public ResourceSimpleList64<ParticleKeyframePropValue> Keyframe { get; set; }
        public ulong padding00 { get; set; }
        public ulong padding01 { get; set; }
        public int EvolutionID { get; set; }
        public byte IsLodEvolution { get; set; }
        public byte padding02 { get; set; }
        public short padding03 { get; set; }
        public ulong padding04 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            Keyframe = reader.ReadBlock<ResourceSimpleList64<ParticleKeyframePropValue>>();
            padding00 = reader.ReadUInt64();
            padding01 = reader.ReadUInt64();
            EvolutionID = reader.ReadInt32();
            IsLodEvolution = reader.ReadByte();
            padding02 = reader.ReadByte();
            padding03 = reader.ReadInt16();
            padding04 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(Keyframe);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.Write(EvolutionID);
            writer.Write(IsLodEvolution);
            writer.Write(padding02);
            writer.Write(padding03);
            writer.Write(padding04);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.ValueTag(sb, indent, "EvolutionID", EvolutionID.ToString());
            YptXml.ValueTag(sb, indent, "IsLodEvolution", IsLodEvolution.ToString());
            if (Keyframe?.data_items != null)
            {
                YptXml.WriteItemArray(sb, Keyframe.data_items, indent, "Keyframes");
            }
        }
        public void ReadXml(XmlNode node)
        {
            EvolutionID = Xml.GetChildIntAttribute(node, "EvolutionID");
            IsLodEvolution = (byte)Xml.GetChildUIntAttribute(node, "IsLodEvolution");
            Keyframe = new ResourceSimpleList64<ParticleKeyframePropValue>();
            Keyframe.data_items = XmlMeta.ReadItemArray<ParticleKeyframePropValue>(node, "Keyframes");
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, Keyframe)
            };
        }

        public override string ToString()
        {
            return EvolutionID.ToString() + ", " + IsLodEvolution.ToString();
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
        public uint Unknown_4h = 1;
        public ulong Unknown_8h;
        public uint RefCount { get; set; }
        public uint Unknown_14h;


        public float FileVersion { get; set; }
        public uint padding02 { get; set; }
        public ulong NamePointer { get; set; }
        public ulong LastEvoList_UNUSED { get; set; }
        public ulong UIData { get; set; }
        public ulong CreationDomainObjPointer { get; set; }
        public ulong padding03 { get; set; }
        public ulong TargetDomainObjPointer { get; set; }
        public ulong padding04 { get; set; }
        public ulong AttractorDomainObjPointer { get; set; }
        public ulong padding05 { get; set; }
        public ulong padding06 { get; set; }
        public ulong padding07 { get; set; }
        public ParticleKeyframeProp[] KeyframeProps { get; set; } = new ParticleKeyframeProp[10];
        public ulong KeyframePropListPointer { get; set; }
        public ushort KeyframePropsCount1 = 10;
        public ushort KeyframePropsCount2 = 10;
        public uint padding08 { get; set; }
        public byte IsOneShot { get; set; }
        public byte padding09 { get; set; }
        public short padding10 { get; set; }
        public uint padding11 { get; set; }

        // reference data
        public string_r Name { get; set; }
        public MetaHash NameHash { get; set; }
        public ParticleDomain CreationDomainObj { get; set; }
        public ParticleDomain TargetDomainObj { get; set; }
        public ParticleDomain AttractorDomainObj { get; set; }
        public ResourcePointerArray64<ParticleKeyframeProp> KeyframePropList { get; set; }//just pointers to KeyframeProps1

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            VFT = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Unknown_8h = reader.ReadUInt64();
            RefCount = reader.ReadUInt32();
            Unknown_14h = reader.ReadUInt32();
            FileVersion = reader.ReadSingle();
            padding02 = reader.ReadUInt32();
            NamePointer = reader.ReadUInt64();
            LastEvoList_UNUSED = reader.ReadUInt64();
            UIData = reader.ReadUInt64();
            CreationDomainObjPointer = reader.ReadUInt64();
            padding03 = reader.ReadUInt64();
            TargetDomainObjPointer = reader.ReadUInt64();
            padding04 = reader.ReadUInt64();
            AttractorDomainObjPointer = reader.ReadUInt64();
            padding05 = reader.ReadUInt64();
            padding06 = reader.ReadUInt64();
            padding07 = reader.ReadUInt64();
            for (int i = 0; i < 10; i++)
            {
                KeyframeProps[i] = reader.ReadBlock<ParticleKeyframeProp>();
            }
            KeyframePropListPointer = reader.ReadUInt64();
            KeyframePropsCount1 = reader.ReadUInt16();
            KeyframePropsCount2 = reader.ReadUInt16();
            padding08 = reader.ReadUInt32();
            IsOneShot = reader.ReadByte();
            padding09 = reader.ReadByte();
            padding10 = reader.ReadInt16();
            padding11 = reader.ReadUInt32();

            // read reference data
            Name = reader.ReadBlockAt<string_r>(NamePointer);
            CreationDomainObj = reader.ReadBlockAt<ParticleDomain>(CreationDomainObjPointer);
            TargetDomainObj = reader.ReadBlockAt<ParticleDomain>(TargetDomainObjPointer);
            AttractorDomainObj = reader.ReadBlockAt<ParticleDomain>(AttractorDomainObjPointer);
            KeyframePropList = reader.ReadBlockAt<ResourcePointerArray64<ParticleKeyframeProp>>(KeyframePropListPointer, KeyframePropsCount2);


            if (!string.IsNullOrEmpty(Name?.Value))
            {
                JenkIndex.Ensure(Name.Value);
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            NamePointer = (ulong)(Name != null ? Name.FilePosition : 0);
            CreationDomainObjPointer = (ulong)(CreationDomainObj != null ? CreationDomainObj.FilePosition : 0);
            TargetDomainObjPointer = (ulong)(TargetDomainObj != null ? TargetDomainObj.FilePosition : 0);
            AttractorDomainObjPointer = (ulong)(AttractorDomainObj != null ? AttractorDomainObj.FilePosition : 0);
            KeyframePropListPointer = (ulong)(KeyframePropList != null ? KeyframePropList.FilePosition : 0);


            // write structure data
            writer.Write(VFT);
            writer.Write(Unknown_4h);
            writer.Write(Unknown_8h);
            writer.Write(RefCount);
            writer.Write(Unknown_14h);
            writer.Write(FileVersion);
            writer.Write(padding02);
            writer.Write(NamePointer);
            writer.Write(LastEvoList_UNUSED);
            writer.Write(UIData);
            writer.Write(CreationDomainObjPointer);
            writer.Write(padding03);
            writer.Write(TargetDomainObjPointer);
            writer.Write(padding04);
            writer.Write(AttractorDomainObjPointer);
            writer.Write(padding05);
            writer.Write(padding06);
            writer.Write(padding07);
            for (int i = 0; i < 10; i++)
            {
                writer.WriteBlock(KeyframeProps[i]);
            }
            writer.Write(KeyframePropListPointer);
            writer.Write(KeyframePropsCount1);
            writer.Write(KeyframePropsCount2);
            writer.Write(padding08);
            writer.Write(IsOneShot);
            writer.Write(padding09);
            writer.Write(padding10);
            writer.Write(padding11);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", YptXml.XmlEscape(Name?.Value ?? ""));
            YptXml.ValueTag(sb, indent, "RefCount", RefCount.ToString());
            YptXml.ValueTag(sb, indent, "IsOneShot", IsOneShot.ToString());
            ParticleDomain.WriteXmlNode(CreationDomainObj, sb, indent, "CreationDomainObj");
            ParticleDomain.WriteXmlNode(TargetDomainObj, sb, indent, "TargetDomainObj");
            ParticleDomain.WriteXmlNode(AttractorDomainObj, sb, indent, "AttractorDomainObj");
            if (KeyframeProps != null)
            {
                YptXml.WriteItemArray(sb, KeyframeProps, indent, "KeyframeProps");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name = (string_r)Xml.GetChildInnerText(node, "Name"); if (Name.Value == null) Name = null;
            NameHash = JenkHash.GenHash(Name?.Value ?? "");
            RefCount = Xml.GetChildUIntAttribute(node, "RefCount");
            IsOneShot = (byte)Xml.GetChildUIntAttribute(node, "IsOneShot");
            CreationDomainObj = ParticleDomain.ReadXmlNode(node.SelectSingleNode("CreationDomainObj")); if (CreationDomainObj != null) CreationDomainObj.Index = 0;
            TargetDomainObj = ParticleDomain.ReadXmlNode(node.SelectSingleNode("TargetDomainObj")); if (TargetDomainObj != null) TargetDomainObj.Index = 1;
            AttractorDomainObj = ParticleDomain.ReadXmlNode(node.SelectSingleNode("AttractorDomainObj")); if (AttractorDomainObj != null) AttractorDomainObj.Index = 2;

            var kflist = XmlMeta.ReadItemArray<ParticleKeyframeProp>(node, "KeyframeProps")?.ToList() ?? new List<ParticleKeyframeProp>();
            KeyframeProps = new ParticleKeyframeProp[10];
            for (int i = 0; i < 10; i++)
            {
                KeyframeProps[i] = (i < kflist.Count) ? kflist[i] : new ParticleKeyframeProp();
            }

            KeyframePropList = new ResourcePointerArray64<ParticleKeyframeProp>();
            KeyframePropList.data_items = KeyframeProps;
            KeyframePropList.ManualReferenceOverride = true;
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            if (CreationDomainObj != null) list.Add(CreationDomainObj);
            if (TargetDomainObj != null) list.Add(TargetDomainObj);
            if (AttractorDomainObj != null) list.Add(AttractorDomainObj);
            if (KeyframePropList != null)
            {
                KeyframePropList.ManualReferenceOverride = true;
                list.Add(KeyframePropList);
            }
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(120, KeyframeProps[0]),
                new Tuple<long, IResourceBlock>(264, KeyframeProps[1]),
                new Tuple<long, IResourceBlock>(408, KeyframeProps[2]),
                new Tuple<long, IResourceBlock>(552, KeyframeProps[3]),
                new Tuple<long, IResourceBlock>(696, KeyframeProps[4]),
                new Tuple<long, IResourceBlock>(840, KeyframeProps[5]),
                new Tuple<long, IResourceBlock>(984, KeyframeProps[6]),
                new Tuple<long, IResourceBlock>(1128, KeyframeProps[7]),
                new Tuple<long, IResourceBlock>(1272, KeyframeProps[8]),
                new Tuple<long, IResourceBlock>(1416, KeyframeProps[9]),
            };
        }

        public override string ToString()
        {
            return Name?.ToString() ?? base.ToString();
        }
    }








    [TC(typeof(EXP))] public struct ParticleKeyframePropName
    {
        public uint Hash { get; set; }

        public ParticleKeyframePropName(uint h) { Hash = h; }
        public ParticleKeyframePropName(string str)
        {
            var strl = str?.ToLowerInvariant() ?? "";
            if (strl.StartsWith("hash_"))
            {
                Hash = Convert.ToUInt32(strl.Substring(5), 16);
            }
            else
            {
                Hash = JenkHash.GenHash(strl);
            }
        }

        public override string ToString()
        {
            var str = ParticleKeyframeProp.GetName(Hash);
            if (!string.IsNullOrEmpty(str)) return str;
            return YptXml.HashString((MetaHash)Hash);
        }

        public string ToCleanString()
        {
            if (Hash == 0) return string.Empty;
            return ToString();
        }

        public static implicit operator uint(ParticleKeyframePropName h)
        {
            return h.Hash;  //implicit conversion
        }

        public static implicit operator ParticleKeyframePropName(uint v)
        {
            return new ParticleKeyframePropName(v);
        }
        public static implicit operator ParticleKeyframePropName(string s)
        {
            return new ParticleKeyframePropName(s);
        }
    }


    [TC(typeof(EXP))] public class ParticleKeyframeProp : ResourceSystemBlock, IMetaXmlItem
    {
        // datBase
        // ptxKeyframeProp
        public override long BlockLength => 0x90;

        // structure data
        public uint VFT { get; set; }
        public uint padding00 { get; set; }
        public ulong EvolvedKeyframeProp { get; set; } // padding 01 - 11 are duplicates of this
        public ulong padding01 { get; set; }
        public ulong padding02 { get; set; }
        public ulong padding03 { get; set; }
        public ulong padding04 { get; set; }
        public ulong padding05 { get; set; }
        public ulong padding06 { get; set; }
        public ulong padding07 { get; set; }
        public ulong padding08 { get; set; }
        public ulong padding09 { get; set; }
        public ulong padding10 { get; set; }
        public ulong padding11 { get; set; }
        public ParticleKeyframePropName Name { get; set; }
        public byte InvertBiasLink { get; set; }
        public byte RandomIndex { get; set; }
        public short unused00 { get; set; }
        public ResourceSimpleList64<ParticleKeyframePropValue> Values { get; set; }
        public ulong padding12 { get; set; }
        public ulong padding13 { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            VFT = reader.ReadUInt32();
            padding00 = reader.ReadUInt32();
            EvolvedKeyframeProp = reader.ReadUInt64();
            padding01 = reader.ReadUInt64();
            padding02 = reader.ReadUInt64();
            padding03 = reader.ReadUInt64();
            padding04 = reader.ReadUInt64();
            padding05 = reader.ReadUInt64();
            padding06 = reader.ReadUInt64();
            padding07 = reader.ReadUInt64();
            padding08 = reader.ReadUInt64();
            padding09 = reader.ReadUInt64();
            padding10 = reader.ReadUInt64();
            padding11 = reader.ReadUInt64();
            Name = reader.ReadUInt32();
            InvertBiasLink = reader.ReadByte();
            RandomIndex = reader.ReadByte();
            unused00 = reader.ReadInt16();
            Values = reader.ReadBlock<ResourceSimpleList64<ParticleKeyframePropValue>>();
            padding12 = reader.ReadUInt64();
            padding13 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(VFT);
            writer.Write(padding00);
            writer.Write(EvolvedKeyframeProp);
            writer.Write(padding01);
            writer.Write(padding02);
            writer.Write(padding03);
            writer.Write(padding04);
            writer.Write(padding05);
            writer.Write(padding06);
            writer.Write(padding07);
            writer.Write(padding08);
            writer.Write(padding09);
            writer.Write(padding10);
            writer.Write(padding11);
            writer.Write(Name);
            writer.Write(InvertBiasLink);
            writer.Write(RandomIndex);
            writer.Write(unused00);
            writer.WriteBlock(Values);
            writer.Write(padding12);
            writer.Write(padding13);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", Name.ToString());
            YptXml.ValueTag(sb, indent, "InvertBiasLink", InvertBiasLink.ToString());
            YptXml.ValueTag(sb, indent, "RandomIndex", RandomIndex.ToString());

            if (Values?.data_items != null)
            {
                YptXml.WriteItemArray(sb, Values.data_items, indent, "Keyframes");
            }

        }
        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            InvertBiasLink = (byte)Xml.GetChildUIntAttribute(node, "InvertBiasLink");
            RandomIndex = (byte)Xml.GetChildUIntAttribute(node, "RandomIndex");

            Values = new ResourceSimpleList64<ParticleKeyframePropValue>();
            Values.data_items = XmlMeta.ReadItemArray<ParticleKeyframePropValue>(node, "Keyframes");

        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x70, Values)
            };
        }

        public override string ToString()
        {
            return Name.ToString() + " (" + (Values?.data_items?.Length ?? 0).ToString() + " values)";
        }





        public static string GetName(uint hash)
        {
            if (NameDict == null)
            {
                //thanks to zirconium for this
                var d = new Dictionary<uint, string>();
                d[0x30e327d4] = "ptxu_Acceleration:m_xyzMinKFP"; 
                d[0x412a554c] = "ptxu_Acceleration:m_xyzMaxKFP"; 
                d[0x1f641348] = "ptxu_Size:m_whdMinKFP"; 
                d[0x3dc78098] = "ptxu_Size:m_whdMaxKFP"; 
                d[0xa67a1155] = "ptxu_Size:m_tblrScalarKFP"; 
                d[0xd5c0fce5] = "ptxu_Size:m_tblrVelScalarKFP"; 
                d[0xe7af1a2c] = "ptxu_MatrixWeight:m_mtxWeightKFP"; 
                d[0x7fae9df8] = "ptxu_Colour:m_rgbaMinKFP"; 
                d[0x60500691] = "ptxu_Colour:m_rgbaMaxKFP"; 
                d[0x8306b23a] = "ptxu_Colour:m_emissiveIntensityKFP"; 
                d[0x1c256ba4] = "ptxu_Rotation:m_initialAngleMinKFP"; 
                d[0x351ed852] = "ptxu_Rotation:m_initialAngleMaxKFP"; 
                d[0xf0274f77] = "ptxu_Rotation:m_angleMinKFP"; 
                d[0x687b4382] = "ptxu_Rotation:m_angleMaxKFP"; 
                d[0x61532d47] = "ptxu_Collision:m_bouncinessKFP"; 
                d[0x686f965f] = "ptxu_Collision:m_bounceDirVarKFP"; 
                d[0x2946e76f] = "ptxu_AnimateTexture:m_animRateKFP"; 
                d[0xd0ef73c5] = "ptxu_Dampening:m_xyzMinKFP"; 
                d[0x64c7fc25] = "ptxu_Dampening:m_xyzMaxKFP"; 
                d[0x0aadcbef] = "ptxu_Wind:m_influenceKFP"; 
                d[0xfb8eb4e6] = "ptxu_Decal:m_dimensionsKFP"; 
                d[0xa7228870] = "ptxu_Decal:m_alphaKFP"; 
                d[0xe5480b3b] = "ptxEffectRule:m_colourTintMinKFP"; 
                d[0xd7c1e22b] = "ptxEffectRule:m_colourTintMaxKFP"; 
                d[0xce8e57a7] = "ptxEffectRule:m_zoomScalarKFP"; 
                d[0x34d6ded7] = "ptxEffectRule:m_dataSphereKFP"; 
                d[0xff864d6c] = "ptxEffectRule:m_dataCapsuleKFP"; 
                d[0x61c50318] = "ptxEmitterRule:m_spawnRateOverTimeKFP"; 
                d[0xe00e5025] = "ptxEmitterRule:m_spawnRateOverDistKFP"; 
                d[0x9fc4652b] = "ptxEmitterRule:m_particleLifeKFP"; 
                d[0x60855078] = "ptxEmitterRule:m_playbackRateScalarKFP"; 
                d[0xc9fe6abb] = "ptxEmitterRule:m_speedScalarKFP"; 
                d[0x4af0ffa1] = "ptxEmitterRule:m_sizeScalarKFP"; 
                d[0xa83b53f0] = "ptxEmitterRule:m_accnScalarKFP"; 
                d[0xdd18b4f2] = "ptxEmitterRule:m_dampeningScalarKFP"; 
                d[0xe511bc23] = "ptxEmitterRule:m_matrixWeightScalarKFP"; 
                d[0xd2df1fa0] = "ptxEmitterRule:m_inheritVelocityKFP"; 
                d[0x45e377e9] = "ptxCreationDomain:m_positionKFP"; 
                d[0x5e692d43] = "ptxCreationDomain:m_rotationKFP"; 
                d[0x1104051e] = "ptxCreationDomain:m_sizeOuterKFP"; 
                d[0x841ab3da] = "ptxCreationDomain:m_sizeInnerKFP"; 
                d[0x41d49131] = "ptxTargetDomain:m_positionKFP"; 
                d[0x64c6c696] = "ptxTargetDomain:m_rotationKFP"; 
                d[0x13c0cac4] = "ptxTargetDomain:m_sizeOuterKFP"; 
                d[0xe7d61ff7] = "ptxTargetDomain:m_sizeInnerKFP"; 
                d[0xda8c99a6] = "ptxu_Light:m_rgbMinKFP"; 
                d[0x12bbe65e] = "ptxu_Light:m_rgbMaxKFP"; 
                d[0xef500a62] = "ptxu_Light:m_intensityKFP"; 
                d[0x75990186] = "ptxu_Light:m_rangeKFP"; 
                d[0xe364d5b2] = "ptxu_Light:m_coronaRgbMinKFP"; 
                d[0xf8561886] = "ptxu_Light:m_coronaRgbMaxKFP"; 
                d[0xe2c464a6] = "ptxu_Light:m_coronaIntensityKFP"; 
                d[0xc35aaf9b] = "ptxu_Light:m_coronaSizeKFP"; 
                d[0xb9410926] = "ptxu_Light:m_coronaFlareKFP"; 
                d[0xce9adbfd] = "ptxu_ZCull:m_heightKFP"; 
                d[0xea6afaba] = "ptxu_ZCull:m_fadeDistKFP"; 
                d[0x2d0d70b5] = "ptxu_Noise:m_posNoiseMinKFP"; 
                d[0xff31aaf3] = "ptxu_Noise:m_posNoiseMaxKFP"; 
                d[0xf256e579] = "ptxu_Noise:m_velNoiseMinKFP"; 
                d[0x513812a5] = "ptxu_Noise:m_velNoiseMaxKFP"; 
                d[0xd1be590a] = "ptxu_Acceleration:m_strengthKFP"; 
                d[0x72668c6f] = "ptxd_Trail:m_texInfoKFP"; 
                d[0x3c599207] = "ptxu_FogVolume:m_rgbTintMinKFP"; 
                d[0x23f55175] = "ptxu_FogVolume:m_rgbTintMaxKFP"; 
                d[0x3ee8e85e] = "ptxu_FogVolume:m_densityRangeKFP"; 
                d[0xdafe6982] = "ptxu_FogVolume:m_scaleMinKFP"; 
                d[0x5473d2fe] = "ptxu_FogVolume:m_scaleMaxKFP"; 
                d[0x9ef3ceec] = "ptxu_FogVolume:m_rotationMinKFP"; 
                d[0x570dc9cd] = "ptxu_FogVolume:m_rotationMaxKFP"; 
                d[0x68f00338] = "ptxAttractorDomain:m_positionKFP"; 
                d[0x8ace32c2] = "ptxAttractorDomain:m_rotationKFP"; 
                d[0xc248b5c9] = "ptxAttractorDomain:m_sizeOuterKFP"; 
                d[0x851d3d14] = "ptxAttractorDomain:m_sizeInnerKFP";
                NameDict = d;
            }
            if (NameDict.TryGetValue(hash, out string str))
            {
                return str;
            }
            return YptXml.HashString((MetaHash)hash);
        }
        private static Dictionary<uint, string> NameDict;


    }


    [TC(typeof(EXP))] public class ParticleKeyframePropValue : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength => 0x20;

        // structure data
        public Vector4 KeyframeTime { get; set; }
        public Vector4 KeyframeValue { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            KeyframeTime = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            KeyframeValue = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(KeyframeTime.X);
            writer.Write(KeyframeTime.Y);
            writer.Write(KeyframeTime.Z);
            writer.Write(KeyframeTime.W);
            writer.Write(KeyframeValue.X);
            writer.Write(KeyframeValue.Y);
            writer.Write(KeyframeValue.Z);
            writer.Write(KeyframeValue.W);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.SelfClosingTag(sb, indent, "KeyframeTime " + FloatUtil.GetVector4XmlString(KeyframeTime));
            YptXml.SelfClosingTag(sb, indent, "KeyframeValue " + FloatUtil.GetVector4XmlString(KeyframeValue));
        }
        public void ReadXml(XmlNode node)
        {
            KeyframeTime = Xml.GetChildVector4Attributes(node, "KeyframeTime");
            KeyframeValue = Xml.GetChildVector4Attributes(node, "KeyframeValue");
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", KeyframeTime.X, KeyframeTime.Y, KeyframeTime.Z, KeyframeTime.W, KeyframeValue.X, KeyframeValue.Y, KeyframeValue.Z, KeyframeValue.W);
        }
    }

    public enum ParticleDomainType : byte
    {
        Box = 0,
        Sphere = 1,
        Cylinder = 2,
        Attractor = 3,
    }

    [TC(typeof(EXP))] public class ParticleDomain : ResourceSystemBlock, IResourceXXSystemBlock, IMetaXmlItem
    {
        // datBase
        // ptxDomain
        public override long BlockLength => 0x280;

        // structure data
        public uint VFT { get; set; }
        public uint padding00 = 1;
        public uint Index { get; set; } // 0, 1, 2   - index of this domain in the ParticleEmitterRule
        public ParticleDomainType DomainType { get; set; }
        public byte padding01 { get; set; }
        public ushort padding02 { get; set; }
        public byte IsWorldSpace { get; set; }
        public byte IsPointRelative { get; set; }
        public byte IsCreationRelative { get; set; }
        public byte IsTargetRelatve { get; set; }
        public uint padding03 { get; set; }
        public ParticleKeyframeProp PositionKFP { get; set; }
        public ParticleKeyframeProp RotationKFP { get; set; }
        public ParticleKeyframeProp SizeOuterKFP { get; set; }
        public ParticleKeyframeProp SizeInnerKFP { get; set; }
        public float FileVersion { get; set; }
        public uint padding04 { get; set; }
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong padding05 { get; set; }
        public ulong oadding06 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            VFT = reader.ReadUInt32();
            padding00 = reader.ReadUInt32();
            Index = reader.ReadUInt32();
            DomainType = (ParticleDomainType)reader.ReadByte();
            padding01 = reader.ReadByte();
            padding02 = reader.ReadUInt16();
            IsWorldSpace = reader.ReadByte();
            IsPointRelative = reader.ReadByte();
            IsCreationRelative = reader.ReadByte();
            IsTargetRelatve = reader.ReadByte();
            padding03 = reader.ReadUInt32();
            PositionKFP = reader.ReadBlock<ParticleKeyframeProp>();
            RotationKFP = reader.ReadBlock<ParticleKeyframeProp>();
            SizeOuterKFP = reader.ReadBlock<ParticleKeyframeProp>();
            SizeInnerKFP = reader.ReadBlock<ParticleKeyframeProp>();
            FileVersion = reader.ReadSingle();
            padding04 = reader.ReadUInt32();
            KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            padding05 = reader.ReadUInt64();
            oadding06 = reader.ReadUInt64();

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(VFT);
            writer.Write(padding00);
            writer.Write(Index);
            writer.Write((byte)DomainType);
            writer.Write(padding01);
            writer.Write(padding02);
            writer.Write(IsWorldSpace);
            writer.Write(IsPointRelative);
            writer.Write(IsCreationRelative);
            writer.Write(IsTargetRelatve);
            writer.Write(padding03);
            writer.WriteBlock(PositionKFP);
            writer.WriteBlock(RotationKFP);
            writer.WriteBlock(SizeOuterKFP);
            writer.WriteBlock(SizeInnerKFP);
            writer.Write(FileVersion);
            writer.Write(padding04);
            writer.WriteBlock(KeyframeProps);
            writer.Write(padding05);
            writer.Write(oadding06);
        }
        public virtual void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.ValueTag(sb, indent, "DomainType", DomainType.ToString());
            YptXml.ValueTag(sb, indent, "IsWorldSpace", IsWorldSpace.ToString());
            YptXml.ValueTag(sb, indent, "IsPointRelative", IsPointRelative.ToString());
            YptXml.ValueTag(sb, indent, "IsCreationRelative", IsCreationRelative.ToString());
            YptXml.ValueTag(sb, indent, "IsTargetRelatve", IsTargetRelatve.ToString());
            YptXml.ValueTag(sb, indent, "FileVersion", FloatUtil.ToString(FileVersion));
            if (PositionKFP != null)
            {
                YptXml.OpenTag(sb, indent, "PositionKFP");
                PositionKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "PositionKFP");
            }
            if (RotationKFP != null)
            {
                YptXml.OpenTag(sb, indent, "RotationKFP");
                RotationKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "RotationKFP");
            }
            if (SizeOuterKFP != null)
            {
                YptXml.OpenTag(sb, indent, "SizeOuterKFP");
                SizeOuterKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "SizeOuterKFP");
            }
            if (SizeInnerKFP != null)
            {
                YptXml.OpenTag(sb, indent, "SizeInnerKFP");
                SizeInnerKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "SizeInnerKFP");
            }
        }
        public virtual void ReadXml(XmlNode node)
        {
            DomainType = Xml.GetEnumValue<ParticleDomainType>(Xml.GetChildStringAttribute(node, "DomainType"));
            IsWorldSpace = (byte)Xml.GetChildUIntAttribute(node, "IsWorldSpace");
            IsPointRelative = (byte)Xml.GetChildUIntAttribute(node, "IsPointRelative");
            IsCreationRelative = (byte)Xml.GetChildUIntAttribute(node, "IsCreationRelative");
            IsTargetRelatve = (byte)Xml.GetChildUIntAttribute(node, "IsTargetRelatve");
            FileVersion = Xml.GetChildFloatAttribute(node, "FileVersion");

            PositionKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("PositionKFP");
            if (pnode0 != null)
            {
                PositionKFP.ReadXml(pnode0);
            }

            RotationKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("RotationKFP");
            if (pnode1 != null)
            {
                RotationKFP.ReadXml(pnode1);
            }

            SizeOuterKFP = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("SizeOuterKFP");
            if (pnode2 != null)
            {
                SizeOuterKFP.ReadXml(pnode2);
            }

            SizeInnerKFP = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("SizeInnerKFP");
            if (pnode3 != null)
            {
                SizeInnerKFP.ReadXml(pnode3);
            }

            KeyframeProps = new ResourcePointerList64<ParticleKeyframeProp>();
            KeyframeProps.data_items = new[] { PositionKFP, RotationKFP, SizeInnerKFP, SizeOuterKFP, null, null, null, null, null, null, null, null, null, null, null, null };

        }
        public static void WriteXmlNode(ParticleDomain d, StringBuilder sb, int indent, string name)
        {
            if (d != null)
            {
                YptXml.OpenTag(sb, indent, name);
                d.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, name);
            }
        }
        public static ParticleDomain ReadXmlNode(XmlNode node)
        {
            if (node != null)
            {
                var typestr = Xml.GetChildStringAttribute(node, "Type");
                var type = Xml.GetEnumValue<ParticleDomainType>(typestr);
                var s = Create(type);
                if (s != null)
                {
                    s.ReadXml(node);
                }
                return s;
            }
            return null;
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            KeyframeProps.ManualCountOverride = true;
            KeyframeProps.ManualReferenceOverride = true;
            KeyframeProps.EntriesCount = 4;
            KeyframeProps.EntriesCapacity = 16;

            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(24, PositionKFP),
                new Tuple<long, IResourceBlock>(168, RotationKFP),
                new Tuple<long, IResourceBlock>(312, SizeOuterKFP),
                new Tuple<long, IResourceBlock>(456, SizeInnerKFP),
                new Tuple<long, IResourceBlock>(0x260, KeyframeProps)
            };
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 12;
            var type = (ParticleDomainType)reader.ReadByte();
            reader.Position -= 13;
            return Create(type);
        }
        public static ParticleDomain Create(ParticleDomainType type)
        {
            switch (type)
            {
                case ParticleDomainType.Box: return new ParticleDomainBox();
                case ParticleDomainType.Sphere: return new ParticleDomainSphere();
                case ParticleDomainType.Cylinder: return new ParticleDomainCylinder();
                case ParticleDomainType.Attractor: return new ParticleDomainAttractor();
                default: return null;// throw new Exception("Unknown domain type");
            }
        }

        public override string ToString()
        {
            return "Domain: " + DomainType.ToString();
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

    [TC(typeof(EXP))] public class ParticleBehaviour : ResourceSystemBlock, IResourceXXSystemBlock, IMetaXmlItem
    {
        // ptxBehaviour
        public override long BlockLength => 0x30;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ParticleBehaviourType Type { get; set; }
        public uint Unknown_Ch; // 0x00000000
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            VFT = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Type = (ParticleBehaviourType)reader.ReadUInt32();
            Unknown_Ch = reader.ReadUInt32();
            KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            Unknown_20h = reader.ReadUInt64();
            Unknown_28h = reader.ReadUInt64();

            KeyframeProps.ManualCountOverride = true; //incase re-saving again
            KeyframeProps.ManualReferenceOverride = true;

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_Ch != 0)
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
            //if ((KeyframeProps?.EntriesCount > 0) && (KeyframeProps.EntriesCapacity != 16))
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(VFT);
            writer.Write(Unknown_4h);
            writer.Write((uint)Type);
            writer.Write(Unknown_Ch);
            writer.WriteBlock(KeyframeProps);
            writer.Write(Unknown_20h);
            writer.Write(Unknown_28h);
        }
        public virtual void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.ValueTag(sb, indent, "Type", Type.ToString());
        }
        public virtual void ReadXml(XmlNode node)
        {
            Type = Xml.GetEnumValue<ParticleBehaviourType>(Xml.GetChildStringAttribute(node, "Type"));

            KeyframeProps = new ResourcePointerList64<ParticleKeyframeProp>();//incase subclass doesn't create it
        }
        public static void WriteXmlNode(ParticleBehaviour b, StringBuilder sb, int indent, string name)
        {
            if (b != null)
            {
                YptXml.OpenTag(sb, indent, name);
                b.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, name);
            }
        }
        public static ParticleBehaviour ReadXmlNode(XmlNode node)
        {
            if (node != null)
            {
                var typestr = Xml.GetChildStringAttribute(node, "Type");
                var type = Xml.GetEnumValue<ParticleBehaviourType>(typestr);
                var s = Create(type);
                if (s != null)
                {
                    s.ReadXml(node);
                }
                return s;
            }
            return null;
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {

            reader.Position += 8;
            ParticleBehaviourType type = (ParticleBehaviourType)reader.ReadUInt32();
            reader.Position -= 12;

            return Create(type);
        }
        public static ParticleBehaviour Create(ParticleBehaviourType type)
        {
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

        public override string ToString()
        {
            return "Behaviour: " + Type.ToString();
        }


        public void CreateKeyframeProps(params ParticleKeyframeProp[] props)
        {
            var plist = props.ToList();
            if (plist.Count > 0)
            {
                for (int i = plist.Count; i < 16; i++)
                {
                    plist.Add(null);
                }
            }

            KeyframeProps = new ResourcePointerList64<ParticleKeyframeProp>();
            KeyframeProps.data_items = plist.ToArray();
            KeyframeProps.ManualCountOverride = true;
            KeyframeProps.ManualReferenceOverride = true;
            KeyframeProps.EntriesCount = (ushort)(props?.Length ?? 0);
            KeyframeProps.EntriesCapacity = (ushort)((plist.Count > 0) ? 16 : 0);

        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x10, KeyframeProps)
            };
        }

    }

    [TC(typeof(EXP))] 
    public class ParticleBehaviourAge : ParticleBehaviour
    {
        // ptxu_Age
        public override long BlockLength => 0x30;


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourAcceleration : ParticleBehaviour
    {
        // ptxu_Acceleration
        public override long BlockLength => 0x170;

        // structure data
        public ParticleKeyframeProp XYZMinKFP { get; set; }
        public ParticleKeyframeProp XYZMaxKFP { get; set; }
        public ulong unused00 { get; set; }
        public int ReferenceSpace { get; set; }
        public byte IsAffectedByZoom { get; set; }
        public byte EnableGravity { get; set; }
        public short padding00 { get; set; }
        public ulong padding01 { get; set; }
        public ulong padding02 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            XYZMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            XYZMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            unused00 = reader.ReadUInt64();
            ReferenceSpace = reader.ReadInt32();
            IsAffectedByZoom = reader.ReadByte();
            EnableGravity = reader.ReadByte();
            padding00 = reader.ReadInt16();
            padding01 = reader.ReadUInt64();
            padding02 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(XYZMinKFP);
            writer.WriteBlock(XYZMaxKFP);
            writer.Write(unused00);
            writer.Write(ReferenceSpace);
            writer.Write(IsAffectedByZoom);
            writer.Write(EnableGravity);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.Write(padding02);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "ReferenceSpace", ReferenceSpace.ToString());
            YptXml.ValueTag(sb, indent, "IsAffectedByZoom", IsAffectedByZoom.ToString());
            YptXml.ValueTag(sb, indent, "EnableGravity", EnableGravity.ToString());
            if (XYZMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "XYZMinKFP");
                XYZMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "XYZMinKFP");
            }
            if (XYZMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "XYZMaxKFP");
                XYZMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "XYZMaxKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            ReferenceSpace = Xml.GetChildIntAttribute(node, "ReferenceSpace");
            IsAffectedByZoom = (byte)Xml.GetChildUIntAttribute(node, "IsAffectedByZoom");
            EnableGravity = (byte)Xml.GetChildUIntAttribute(node, "EnableGravity");

            XYZMinKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("XYZMinKFP");
            if (pnode0 != null)
            {
                XYZMinKFP.ReadXml(pnode0);
            }

            XYZMaxKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("XYZMaxKFP");
            if (pnode1 != null)
            {
                XYZMaxKFP.ReadXml(pnode1);
            }

            CreateKeyframeProps(XYZMinKFP, XYZMaxKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x10, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, XYZMinKFP),
                new Tuple<long, IResourceBlock>(192, XYZMaxKFP)
            };
        }
    }

    [TC(typeof(EXP))] 
    public class ParticleBehaviourVelocity : ParticleBehaviour
    {
        // ptxu_Velocity
        public override long BlockLength => 0x30;


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourRotation : ParticleBehaviour
    {
        // ptxu_Rotation
        public override long BlockLength => 0x280;

        // structure data
        public ParticleKeyframeProp InitialAngleMinKFP { get; set; }
        public ParticleKeyframeProp InitialAngleMaxKFP { get; set; }
        public ParticleKeyframeProp AngleMinKFP { get; set; }
        public ParticleKeyframeProp AngleMaxKFP { get; set; }
        public int InitRotationMode { get; set; }
        public int UpdateRotationMode { get; set; }
        public byte AccumulateAngle { get; set; }
        public byte RotateAngleAxes { get; set; }
        public byte RotateInitAngleAxes { get; set; }
        public byte padding00 { get; set; }
        public float SpeedFadeThreshold { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            InitialAngleMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            InitialAngleMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            AngleMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            AngleMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            InitRotationMode = reader.ReadInt32();
            UpdateRotationMode = reader.ReadInt32();
            AccumulateAngle = reader.ReadByte();
            RotateAngleAxes = reader.ReadByte();
            RotateInitAngleAxes = reader.ReadByte();
            padding00 = reader.ReadByte();
            SpeedFadeThreshold = reader.ReadSingle();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(InitialAngleMinKFP);
            writer.WriteBlock(InitialAngleMaxKFP);
            writer.WriteBlock(AngleMinKFP);
            writer.WriteBlock(AngleMaxKFP);
            writer.Write(InitRotationMode);
            writer.Write(UpdateRotationMode);
            writer.Write(AccumulateAngle);
            writer.Write(RotateAngleAxes);
            writer.Write(RotateInitAngleAxes);
            writer.Write(padding00);
            writer.Write(SpeedFadeThreshold);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "InitRotationMode", InitRotationMode.ToString());
            YptXml.ValueTag(sb, indent, "UpdateRotationMode", UpdateRotationMode.ToString());
            YptXml.ValueTag(sb, indent, "AccumulateAngle", AccumulateAngle.ToString());
            YptXml.ValueTag(sb, indent, "RotateAngleAxes", RotateAngleAxes.ToString());
            YptXml.ValueTag(sb, indent, "RotateInitAngleAxes", RotateInitAngleAxes.ToString());
            YptXml.ValueTag(sb, indent, "SpeedFadeThreshold", FloatUtil.ToString(SpeedFadeThreshold));
            if (InitialAngleMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "InitialAngleMinKFP");
                InitialAngleMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "InitialAngleMinKFP");
            }
            if (InitialAngleMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "InitialAngleMaxKFP");
                InitialAngleMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "InitialAngleMaxKFP");
            }
            if (AngleMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "AngleMinKFP");
                AngleMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "AngleMinKFP");
            }
            if (AngleMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "AngleMaxKFP");
                AngleMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "AngleMaxKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            InitRotationMode = Xml.GetChildIntAttribute(node, "InitRotationMode");
            UpdateRotationMode = Xml.GetChildIntAttribute(node, "UpdateRotationMode");
            AccumulateAngle = (byte)Xml.GetChildUIntAttribute(node, "AccumulateAngle");
            RotateAngleAxes = (byte)Xml.GetChildUIntAttribute(node, "RotateAngleAxes");
            RotateInitAngleAxes = (byte)Xml.GetChildUIntAttribute(node, "RotateInitAngleAxes");
            SpeedFadeThreshold = Xml.GetChildFloatAttribute(node, "SpeedFadeThreshold");

            InitialAngleMinKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("InitialAngleMinKFP");
            if (pnode0 != null)
            {
                InitialAngleMinKFP.ReadXml(pnode0);
            }

            InitialAngleMaxKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("InitialAngleMaxKFP");
            if (pnode1 != null)
            {
                InitialAngleMaxKFP.ReadXml(pnode1);
            }

            AngleMinKFP = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("AngleMinKFP");
            if (pnode2 != null)
            {
                AngleMinKFP.ReadXml(pnode2);
            }

            AngleMaxKFP = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("AngleMaxKFP");
            if (pnode3 != null)
            {
                AngleMaxKFP.ReadXml(pnode3);
            }

            CreateKeyframeProps(InitialAngleMinKFP, InitialAngleMaxKFP, AngleMinKFP, AngleMaxKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, InitialAngleMinKFP),
                new Tuple<long, IResourceBlock>(192, InitialAngleMaxKFP),
                new Tuple<long, IResourceBlock>(336, AngleMinKFP),
                new Tuple<long, IResourceBlock>(480, AngleMaxKFP)
            };
        }
    }

    [TC(typeof(EXP))] 
    public class ParticleBehaviourSize : ParticleBehaviour
    {
        // ptxu_Size
        public override long BlockLength => 0x280;

        // structure data
        public ParticleKeyframeProp WhdMinKFP { get; set; }
        public ParticleKeyframeProp WhdMaxKFP { get; set; }
        public ParticleKeyframeProp TblrScalarKFP { get; set; }
        public ParticleKeyframeProp TblrVelScalarKFP { get; set; }
        public int KeyframeMode { get; set; }
        public byte IsProportional { get; set; }
        public byte padding00 { get; set; }
        public short padding01 { get; set; }
        public ulong padding02 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            WhdMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            WhdMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            TblrScalarKFP = reader.ReadBlock<ParticleKeyframeProp>();
            TblrVelScalarKFP = reader.ReadBlock<ParticleKeyframeProp>();
            KeyframeMode = reader.ReadInt32();
            IsProportional = reader.ReadByte();
            padding00 = reader.ReadByte();
            padding01 = reader.ReadInt16();
            padding02 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(WhdMinKFP);
            writer.WriteBlock(WhdMaxKFP);
            writer.WriteBlock(TblrScalarKFP);
            writer.WriteBlock(TblrVelScalarKFP);
            writer.Write(KeyframeMode);
            writer.Write(IsProportional);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.Write(padding02);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "KeyframeMode", KeyframeMode.ToString());
            YptXml.ValueTag(sb, indent, "IsProportional", IsProportional.ToString());
            if (WhdMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "WhdMinKFP");
                WhdMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "WhdMinKFP");
            }
            if (WhdMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "WhdMaxKFP");
                WhdMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "WhdMaxKFP");
            }
            if (TblrScalarKFP != null)
            {
                YptXml.OpenTag(sb, indent, "TblrScalarKFP");
                TblrScalarKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "TblrScalarKFP");
            }
            if (TblrVelScalarKFP != null)
            {
                YptXml.OpenTag(sb, indent, "TblrVelScalarKFP");
                TblrVelScalarKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "TblrVelScalarKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            KeyframeMode = Xml.GetChildIntAttribute(node, "KeyframeMode");
            IsProportional = (byte)Xml.GetChildUIntAttribute(node, "IsProportional");

            WhdMinKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("WhdMinKFP");
            if (pnode0 != null)
            {
                WhdMinKFP.ReadXml(pnode0);
            }

            WhdMaxKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("WhdMaxKFP");
            if (pnode1 != null)
            {
                WhdMaxKFP.ReadXml(pnode1);
            }

            TblrScalarKFP = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("TblrScalarKFP");
            if (pnode2 != null)
            {
                TblrScalarKFP.ReadXml(pnode2);
            }

            TblrVelScalarKFP = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("TblrVelScalarKFP");
            if (pnode3 != null)
            {
                TblrVelScalarKFP.ReadXml(pnode3);
            }

            CreateKeyframeProps(WhdMinKFP, WhdMaxKFP, TblrScalarKFP, TblrVelScalarKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, WhdMinKFP),
                new Tuple<long, IResourceBlock>(192, WhdMaxKFP),
                new Tuple<long, IResourceBlock>(336, TblrScalarKFP),
                new Tuple<long, IResourceBlock>(480, TblrVelScalarKFP)
            };
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourDampening : ParticleBehaviour
    {
        // ptxu_Dampening
        public override long BlockLength => 0x170;

        // structure data
        public ParticleKeyframeProp XYZMinKFP { get; set; }
        public ParticleKeyframeProp XYZMaxKFP { get; set; }
        public ulong unused00 { get; set; }
        public int ReferenceSpace { get; set; }
        public byte EnableAirResistance { get; set; }
        public byte padding00 { get; set; }
        public short padding01 { get; set; }
        public ulong padding02 { get; set; }
        public ulong padding03 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            XYZMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            XYZMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            unused00 = reader.ReadUInt64();
            ReferenceSpace = reader.ReadInt32();
            EnableAirResistance = reader.ReadByte();
            padding00 = reader.ReadByte();
            padding01 = reader.ReadInt16();
            padding02 = reader.ReadUInt64();
            padding03 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(XYZMinKFP);
            writer.WriteBlock(XYZMaxKFP);
            writer.Write(unused00);
            writer.Write(ReferenceSpace);
            writer.Write(EnableAirResistance);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.Write(padding02);
            writer.Write(padding03);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "ReferenceSpace", ReferenceSpace.ToString());
            YptXml.ValueTag(sb, indent, "EnableAirResistance", EnableAirResistance.ToString());
            if (XYZMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "XYZMinKFP");
                XYZMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "XYZMinKFP");
            }
            if (XYZMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "XYZMaxKFP");
                XYZMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "XYZMaxKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            ReferenceSpace = Xml.GetChildIntAttribute(node, "ReferenceSpace");
            EnableAirResistance = (byte)Xml.GetChildIntAttribute(node, "EnableAirResistance");

            XYZMinKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("XYZMinKFP");
            if (pnode0 != null)
            {
                XYZMinKFP.ReadXml(pnode0);
            }

            XYZMaxKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("XYZMaxKFP");
            if (pnode1 != null)
            {
                XYZMaxKFP.ReadXml(pnode1);
            }

            CreateKeyframeProps(XYZMinKFP, XYZMaxKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, XYZMinKFP),
                new Tuple<long, IResourceBlock>(192, XYZMaxKFP)
            };
        }
    }
    [TC(typeof(EXP))]
    public class ParticleBehaviourMatrixWeight : ParticleBehaviour
    {
        // ptxu_MatrixWeight
        public override long BlockLength => 0xD0;

        // structure data
        public ParticleKeyframeProp mtxWeightKFP { get; set; }
        public int ReferenceSpace { get; set; }
        public uint padding00 { get; set; }
        public ulong padding01 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            mtxWeightKFP = reader.ReadBlock<ParticleKeyframeProp>();
            ReferenceSpace = reader.ReadInt32();
            padding00 = reader.ReadUInt32();
            padding01 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(mtxWeightKFP);
            writer.Write(ReferenceSpace);
            writer.Write(padding00);
            writer.Write(padding01);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "ReferenceSpace", ReferenceSpace.ToString());
            if (mtxWeightKFP != null)
            {
                YptXml.OpenTag(sb, indent, "mtxWeightKFP");
                mtxWeightKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "mtxWeightKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            ReferenceSpace = Xml.GetChildIntAttribute(node, "ReferenceSpace");

            mtxWeightKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("mtxWeightKFP");
            if (pnode0 != null)
            {
                mtxWeightKFP.ReadXml(pnode0);
            }

            CreateKeyframeProps(mtxWeightKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, mtxWeightKFP)
            };
        }
    }

    [TC(typeof(EXP))] 
    public class ParticleBehaviourCollision : ParticleBehaviour
    {
        // ptxu_Collision
        public override long BlockLength => 0x170;

        // structure data
        public ParticleKeyframeProp BouncinessKFP { get; set; }
        public ParticleKeyframeProp BounceDirVarKFP { get; set; }
        public float RadiusMult { get; set; }
        public float RestSpeed { get; set; }
        public int CollisionChance { get; set; }
        public int KillChance { get; set; }
        public byte DebugDraw { get; set; }
        public byte padding00 { get; set; }
        public short padding01 { get; set; }
        public float OverrideMinRadius { get; set; }
        public ulong padding02 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            BouncinessKFP = reader.ReadBlock<ParticleKeyframeProp>();
            BounceDirVarKFP = reader.ReadBlock<ParticleKeyframeProp>();
            RadiusMult = reader.ReadSingle();
            RestSpeed = reader.ReadSingle();
            CollisionChance = reader.ReadInt32();
            KillChance = reader.ReadInt32();
            DebugDraw = reader.ReadByte();
            padding00 =  reader.ReadByte();
            padding01 = reader.ReadInt16();
            OverrideMinRadius = reader.ReadSingle();
            padding02 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(BouncinessKFP);
            writer.WriteBlock(BounceDirVarKFP);
            writer.Write(RadiusMult);
            writer.Write(RestSpeed);
            writer.Write(CollisionChance);
            writer.Write(KillChance);
            writer.Write(DebugDraw);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.Write(OverrideMinRadius);
            writer.Write(padding02);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "RadiusMult", FloatUtil.ToString(RadiusMult));
            YptXml.ValueTag(sb, indent, "RestSpeed", FloatUtil.ToString(RestSpeed));
            YptXml.ValueTag(sb, indent, "CollisionChance", CollisionChance.ToString());
            YptXml.ValueTag(sb, indent, "KillChance", KillChance.ToString());
            YptXml.ValueTag(sb, indent, "OverrideMinRadius", FloatUtil.ToString(OverrideMinRadius));
            if (BouncinessKFP != null)
            {
                YptXml.OpenTag(sb, indent, "BouncinessKFP");
                BouncinessKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "BouncinessKFP");
            }
            if (BounceDirVarKFP != null)
            {
                YptXml.OpenTag(sb, indent, "BounceDirVarKFP");
                BounceDirVarKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "BounceDirVarKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            RadiusMult = Xml.GetChildFloatAttribute(node, "RadiusMult");
            RestSpeed = Xml.GetChildFloatAttribute(node, "RestSpeed");
            CollisionChance = Xml.GetChildIntAttribute(node, "CollisionChance");
            KillChance = Xml.GetChildIntAttribute(node, "KillChance");
            OverrideMinRadius = Xml.GetChildFloatAttribute(node, "OverrideMinRadius");

            BouncinessKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("BouncinessKFP");
            if (pnode0 != null)
            {
                BouncinessKFP.ReadXml(pnode0);
            }

            BounceDirVarKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("BounceDirVarKFP");
            if (pnode1 != null)
            {
                BounceDirVarKFP.ReadXml(pnode1);
            }

            CreateKeyframeProps(BouncinessKFP, BounceDirVarKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, BouncinessKFP),
                new Tuple<long, IResourceBlock>(192, BounceDirVarKFP)
            };
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourAnimateTexture : ParticleBehaviour
    {
        // ptxu_AnimateTexture
        public override long BlockLength => 0xD0;

        // structure data
        public ParticleKeyframeProp AnimRateKFP { get; set; }
        public int KeyframeMode { get; set; }
        public int LastFrameID { get; set; }
        public int LoopMode { get; set; }
        public byte IsRandomised { get; set; }
        public byte IsScaledOverParticleLife { get; set; }
        public byte IsHeldOnLastFrame { get; set; }
        public byte DoFrameBlending { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            AnimRateKFP = reader.ReadBlock<ParticleKeyframeProp>();
            KeyframeMode = reader.ReadInt32();
            LastFrameID = reader.ReadInt32();
            LoopMode = reader.ReadInt32();
            IsRandomised = reader.ReadByte();
            IsScaledOverParticleLife = reader.ReadByte();
            IsHeldOnLastFrame = reader.ReadByte();
            DoFrameBlending = reader.ReadByte();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(AnimRateKFP);
            writer.Write(KeyframeMode);
            writer.Write(LastFrameID);
            writer.Write(LoopMode);
            writer.Write(IsRandomised);
            writer.Write(IsScaledOverParticleLife);
            writer.Write(IsHeldOnLastFrame);
            writer.Write(DoFrameBlending);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "KeyframeMode", KeyframeMode.ToString());
            YptXml.ValueTag(sb, indent, "LastFrameID", LastFrameID.ToString());
            YptXml.ValueTag(sb, indent, "LoopMode", LoopMode.ToString());
            YptXml.ValueTag(sb, indent, "IsRandomised", IsRandomised.ToString());
            YptXml.ValueTag(sb, indent, "IsScaledOverParticleLife", IsScaledOverParticleLife.ToString());
            YptXml.ValueTag(sb, indent, "IsHeldOnLastFrame", IsHeldOnLastFrame.ToString());
            YptXml.ValueTag(sb, indent, "DoFrameBlending", DoFrameBlending.ToString());
            if (AnimRateKFP != null)
            {
                YptXml.OpenTag(sb, indent, "AnimRateKFP");
                AnimRateKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "AnimRateKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            KeyframeMode = Xml.GetChildIntAttribute(node, "KeyframeMode");
            LastFrameID = Xml.GetChildIntAttribute(node, "LastFrameID");
            LoopMode = Xml.GetChildIntAttribute(node, "LoopMode");
            IsRandomised = (byte)Xml.GetChildUIntAttribute(node, "IsRandomised");
            IsScaledOverParticleLife = (byte)Xml.GetChildUIntAttribute(node, "IsScaledOverParticleLife");
            IsHeldOnLastFrame = (byte)Xml.GetChildUIntAttribute(node, "IsHeldOnLastFrame");
            DoFrameBlending = (byte)Xml.GetChildUIntAttribute(node, "DoFrameBlending");

            AnimRateKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("AnimRateKFP");
            if (pnode0 != null)
            {
                AnimRateKFP.ReadXml(pnode0);
            }

            CreateKeyframeProps(AnimRateKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, AnimRateKFP)
            };
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourColour : ParticleBehaviour
    {
        // ptxu_Colour
        public override long BlockLength => 0x1F0;

        // structure data
        public ParticleKeyframeProp RGBAMinKFP { get; set; }
        public ParticleKeyframeProp RGBAMaxKFP { get; set; }
        public ParticleKeyframeProp EmissiveIntensityKFP { get; set; }
        public int KeyframeMode { get; set; }
        public byte RGBAMaxEnable { get; set; }
        public byte RGBAProportional { get; set; }
        public byte RGBCanTint { get; set; }
        public byte padding00 { get; set; }
        public ulong padding01 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            RGBAMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            RGBAMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            EmissiveIntensityKFP = reader.ReadBlock<ParticleKeyframeProp>();
            KeyframeMode = reader.ReadInt32();
            RGBAMaxEnable = reader.ReadByte();
            RGBAProportional = reader.ReadByte();
            RGBCanTint = reader.ReadByte();
            padding00 = reader.ReadByte();
            padding01 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(RGBAMinKFP);
            writer.WriteBlock(RGBAMaxKFP);
            writer.WriteBlock(EmissiveIntensityKFP);
            writer.Write(KeyframeMode);
            writer.Write(RGBAMaxEnable);
            writer.Write(RGBAProportional);
            writer.Write(RGBCanTint);
            writer.Write(padding00);
            writer.Write(padding01);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "KeyframeMode", KeyframeMode.ToString());
            YptXml.ValueTag(sb, indent, "RGBAMaxEnable", RGBAMaxEnable.ToString());
            YptXml.ValueTag(sb, indent, "RGBAProportional", RGBAProportional.ToString());
            YptXml.ValueTag(sb, indent, "RGBCanTint", RGBCanTint.ToString());
            if (RGBAMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "RGBAMinKFP");
                RGBAMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "RGBAMinKFP");
            }
            if (RGBAMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "RGBAMaxKFP");
                RGBAMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "RGBAMaxKFP");
            }
            if (EmissiveIntensityKFP != null)
            {
                YptXml.OpenTag(sb, indent, "EmissiveIntensityKFP");
                EmissiveIntensityKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "EmissiveIntensityKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            KeyframeMode = Xml.GetChildIntAttribute(node, "KeyframeMode");
            RGBAMaxEnable = (byte)Xml.GetChildUIntAttribute(node, "RGBAMaxEnable");
            RGBAProportional = (byte)Xml.GetChildUIntAttribute(node, "RGBAProportional");
            RGBCanTint = (byte)Xml.GetChildUIntAttribute(node, "RGBCanTint");

            RGBAMinKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("RGBAMinKFP");
            if (pnode0 != null)
            {
                RGBAMinKFP.ReadXml(pnode0);
            }

            RGBAMaxKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("RGBAMaxKFP");
            if (pnode1 != null)
            {
                RGBAMaxKFP.ReadXml(pnode1);
            }

            EmissiveIntensityKFP = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("EmissiveIntensityKFP");
            if (pnode2 != null)
            {
                EmissiveIntensityKFP.ReadXml(pnode2);
            }

            CreateKeyframeProps(RGBAMinKFP, RGBAMaxKFP, EmissiveIntensityKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, RGBAMinKFP),
                new Tuple<long, IResourceBlock>(192, RGBAMaxKFP),
                new Tuple<long, IResourceBlock>(336, EmissiveIntensityKFP)
            };
        }
    }

    [TC(typeof(EXP))] 
    public class ParticleBehaviourSprite : ParticleBehaviour
    {
        // ptxd_Sprite
        public override long BlockLength => 0x70;

        // structure data
        public Vector3 AlignAxis { get; set; }
        public uint padding00 { get; set; }
        public int AlignmentMode { get; set; }
        public float FlipChanceU { get; set; }
        public float FlipChanceV { get; set; }
        public float NearClipDist { get; set; }
        public float FarClipDist { get; set; }
        public float ProjectionDepth { get; set; }
        public float ShadowCastIntensity { get; set; }
        public byte IsScreenSpace { get; set; }
        public byte IsHighRes { get; set; }
        public byte NearClip { get; set; }
        public byte FarClip { get; set; }
        public byte UVClip { get; set; }
        public byte DisableDraw { get; set; }
        public short padding01 { get; set; }
        public uint padding02 { get; set; }
        public ulong padding03 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            AlignAxis = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            padding00 = reader.ReadUInt32();
            AlignmentMode = reader.ReadInt32();
            FlipChanceU = reader.ReadSingle();
            FlipChanceV = reader.ReadSingle();
            NearClipDist = reader.ReadSingle();
            FarClipDist = reader.ReadSingle();
            ProjectionDepth = reader.ReadSingle();
            ShadowCastIntensity = reader.ReadSingle();
            IsScreenSpace = reader.ReadByte();
            IsHighRes = reader.ReadByte();
            NearClip = reader.ReadByte();
            FarClip = reader.ReadByte();
            UVClip = reader.ReadByte();
            DisableDraw = reader.ReadByte();
            padding01 = reader.ReadInt16();
            padding02 = reader.ReadUInt32();
            padding03 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(AlignAxis.X);
            writer.Write(AlignAxis.Y);
            writer.Write(AlignAxis.Z);
            writer.Write(padding00);
            writer.Write(AlignmentMode);
            writer.Write(FlipChanceU);
            writer.Write(FlipChanceV);
            writer.Write(NearClipDist);
            writer.Write(FarClipDist);
            writer.Write(ProjectionDepth);
            writer.Write(ShadowCastIntensity);
            writer.Write(IsScreenSpace);
            writer.Write(IsHighRes);
            writer.Write(NearClip);
            writer.Write(FarClip);
            writer.Write(UVClip);
            writer.Write(DisableDraw);
            writer.Write(padding01);
            writer.Write(padding02);
            writer.Write(padding03);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            RelXml.SelfClosingTag(sb, indent, "AlignAxis " + FloatUtil.GetVector3XmlString(AlignAxis));
            YptXml.ValueTag(sb, indent, "AlignmentMode", AlignmentMode.ToString());
            YptXml.ValueTag(sb, indent, "FlipChanceU", FloatUtil.ToString(FlipChanceU));
            YptXml.ValueTag(sb, indent, "FlipChanceV", FloatUtil.ToString(FlipChanceV));
            YptXml.ValueTag(sb, indent, "NearClipDist", FloatUtil.ToString(NearClipDist));
            YptXml.ValueTag(sb, indent, "FarClipDist", FloatUtil.ToString(FarClipDist));
            YptXml.ValueTag(sb, indent, "ProjectionDepth", FloatUtil.ToString(ProjectionDepth));
            YptXml.ValueTag(sb, indent, "ShadowCastIntensity", FloatUtil.ToString(ShadowCastIntensity));
            YptXml.ValueTag(sb, indent, "IsScreenSpace", IsScreenSpace.ToString());
            YptXml.ValueTag(sb, indent, "IsHighRes", IsHighRes.ToString());
            YptXml.ValueTag(sb, indent, "NearClip", NearClip.ToString());
            YptXml.ValueTag(sb, indent, "FarClip", FarClip.ToString());
            YptXml.ValueTag(sb, indent, "UVClip", UVClip.ToString());
            YptXml.ValueTag(sb, indent, "DisableDraw", DisableDraw.ToString());
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            AlignAxis = Xml.GetChildVector3Attributes(node, "AlignAxis");;
            AlignmentMode = Xml.GetChildIntAttribute(node, "AlignmentMode");
            FlipChanceU = Xml.GetChildFloatAttribute(node, "FlipChanceU");
            FlipChanceV = Xml.GetChildFloatAttribute(node, "FlipChanceV");
            NearClipDist = Xml.GetChildFloatAttribute(node, "NearClipDist");
            FarClipDist = Xml.GetChildFloatAttribute(node, "FarClipDist");
            ProjectionDepth = Xml.GetChildFloatAttribute(node, "ProjectionDepth");
            ShadowCastIntensity = Xml.GetChildFloatAttribute(node, "ShadowCastIntensity");
            IsScreenSpace = (byte)Xml.GetChildUIntAttribute(node, "IsScreenSpace");
            IsHighRes = (byte)Xml.GetChildUIntAttribute(node, "IsHighRes");
            NearClip = (byte)Xml.GetChildUIntAttribute(node, "NearClip");
            FarClip = (byte)Xml.GetChildUIntAttribute(node, "FarClip");
            UVClip = (byte)Xml.GetChildUIntAttribute(node, "UVClip");
            DisableDraw = (byte)Xml.GetChildUIntAttribute(node, "DisableDraw");
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourWind : ParticleBehaviour
    {
        // ptxu_Wind
        public override long BlockLength => 0xF0;

        // structure data
        public ParticleKeyframeProp InfluenceKFP { get; set; }
        public ulong unused00 { get; set; }
        public ulong unused01 { get; set; }
        public float HighLodRange { get; set; }
        public float LowLodRange { get; set; }
        public int HighLodDisturbanceMode { get; set; }
        public int LodLodDisturbanceMode { get; set; }
        public byte IgnoreMtxWeight { get; set; }
        public byte padding00 { get; set; }
        public short padding01 { get; set; }
        public uint padding02 { get; set; }
        public ulong padding03 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            InfluenceKFP = reader.ReadBlock<ParticleKeyframeProp>();
            unused00 = reader.ReadUInt64();
            unused01 = reader.ReadUInt64();
            HighLodRange = reader.ReadSingle();
            LowLodRange = reader.ReadSingle();
            HighLodDisturbanceMode = reader.ReadInt32();
            LodLodDisturbanceMode = reader.ReadInt32();
            IgnoreMtxWeight = reader.ReadByte();
            padding00 = reader.ReadByte();
            padding01 = reader.ReadInt16();
            padding02 = reader.ReadUInt32();
            padding03 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(InfluenceKFP);
            writer.Write(unused00);
            writer.Write(unused01);
            writer.Write(HighLodRange);
            writer.Write(LowLodRange);
            writer.Write(HighLodDisturbanceMode);
            writer.Write(LodLodDisturbanceMode);
            writer.Write(IgnoreMtxWeight);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.Write(padding02);
            writer.Write(padding03);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "HighLodRange", FloatUtil.ToString(HighLodRange));
            YptXml.ValueTag(sb, indent, "LowLodRange", FloatUtil.ToString(LowLodRange));
            YptXml.ValueTag(sb, indent, "HighLodDisturbanceMode", HighLodDisturbanceMode.ToString());
            YptXml.ValueTag(sb, indent, "LodLodDisturbanceMode", LodLodDisturbanceMode.ToString());
            YptXml.ValueTag(sb, indent, "IgnoreMtxWeight", IgnoreMtxWeight.ToString());
            if (InfluenceKFP != null)
            {
                YptXml.OpenTag(sb, indent, "InfluenceKFP");
                InfluenceKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "InfluenceKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            HighLodRange = Xml.GetChildFloatAttribute(node, "HighLodRange");
            LowLodRange = Xml.GetChildFloatAttribute(node, "LowLodRange");
            HighLodDisturbanceMode = Xml.GetChildIntAttribute(node, "HighLodDisturbanceMode");
            LodLodDisturbanceMode = Xml.GetChildIntAttribute(node, "LodLodDisturbanceMode");
            IgnoreMtxWeight = (byte)Xml.GetChildUIntAttribute(node, "IgnoreMtxWeight");
            InfluenceKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("InfluenceKFP");
            if (pnode0 != null)
            {
                InfluenceKFP.ReadXml(pnode0);
            }

            CreateKeyframeProps(InfluenceKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, InfluenceKFP)
            };
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourLight : ParticleBehaviour
    {
        // ptxu_Light
        public override long BlockLength => 0x550;

        // structure data
        public ParticleKeyframeProp RGBMinKFP { get; set; }
        public ParticleKeyframeProp RGBMaxKFP { get; set; }
        public ParticleKeyframeProp IntensityKFP { get; set; }
        public ParticleKeyframeProp RangeKFP { get; set; }
        public ParticleKeyframeProp CoronaRGBMinKFP { get; set; }
        public ParticleKeyframeProp CoronaRGBMaxKFP { get; set; }
        public ParticleKeyframeProp CoronaIntensityKFP { get; set; }
        public ParticleKeyframeProp CoronaSizeKFP { get; set; }
        public ParticleKeyframeProp CoronaFlareKFP { get; set; }
        public float CoronaZBias { get; set; }
        public byte CoronaUseLightColour { get; set; }
        public byte ColourFromParticle { get; set; }
        public byte ColourPerFrame { get; set; }
        public byte IntensityPerFrame { get; set; }
        public byte RangePerFrame { get; set; }
        public byte CastsShadows { get; set; }
        public byte CoronaNotInReflection { get; set; }
        public byte CoronaOnlyInReflection { get; set; }
        public int LightType { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            RGBMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            RGBMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            IntensityKFP = reader.ReadBlock<ParticleKeyframeProp>();
            RangeKFP = reader.ReadBlock<ParticleKeyframeProp>();
            CoronaRGBMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            CoronaRGBMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            CoronaIntensityKFP = reader.ReadBlock<ParticleKeyframeProp>();
            CoronaSizeKFP = reader.ReadBlock<ParticleKeyframeProp>();
            CoronaFlareKFP = reader.ReadBlock<ParticleKeyframeProp>();
            CoronaZBias = reader.ReadSingle();
            CoronaUseLightColour = reader.ReadByte();
            ColourFromParticle = reader.ReadByte();
            ColourPerFrame = reader.ReadByte();
            IntensityPerFrame = reader.ReadByte();
            RangePerFrame = reader.ReadByte();
            CastsShadows = reader.ReadByte();
            CoronaNotInReflection = reader.ReadByte();
            CoronaOnlyInReflection = reader.ReadByte();
            LightType = reader.ReadInt32();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(RGBMinKFP);
            writer.WriteBlock(RGBMaxKFP);
            writer.WriteBlock(IntensityKFP);
            writer.WriteBlock(RangeKFP);
            writer.WriteBlock(CoronaRGBMinKFP);
            writer.WriteBlock(CoronaRGBMaxKFP);
            writer.WriteBlock(CoronaIntensityKFP);
            writer.WriteBlock(CoronaSizeKFP);
            writer.WriteBlock(CoronaFlareKFP);
            writer.Write(CoronaZBias);
            writer.Write(CoronaUseLightColour);
            writer.Write(ColourFromParticle);
            writer.Write(ColourPerFrame);
            writer.Write(IntensityPerFrame);
            writer.Write(RangePerFrame);
            writer.Write(CastsShadows);
            writer.Write(CoronaNotInReflection);
            writer.Write(CoronaOnlyInReflection);
            writer.Write(LightType);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "CoronaZBias", FloatUtil.ToString(CoronaZBias));
            YptXml.ValueTag(sb, indent, "CoronaUseLightColour", CoronaUseLightColour.ToString());
            YptXml.ValueTag(sb, indent, "ColourFromParticle", ColourFromParticle.ToString());
            YptXml.ValueTag(sb, indent, "ColourPerFrame", ColourPerFrame.ToString());
            YptXml.ValueTag(sb, indent, "IntensityPerFrame", IntensityPerFrame.ToString());
            YptXml.ValueTag(sb, indent, "RangePerFrame", RangePerFrame.ToString());
            YptXml.ValueTag(sb, indent, "CastsShadows", CastsShadows.ToString());
            YptXml.ValueTag(sb, indent, "CoronaNotInReflection", CoronaNotInReflection.ToString());
            YptXml.ValueTag(sb, indent, "CoronaOnlyInReflection", CoronaOnlyInReflection.ToString());
            YptXml.ValueTag(sb, indent, "LightType", LightType.ToString());
            if (RGBMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "RGBMinKFP");
                RGBMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "RGBMinKFP");
            }
            if (RGBMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "RGBMaxKFP");
                RGBMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "RGBMaxKFP");
            }
            if (IntensityKFP != null)
            {
                YptXml.OpenTag(sb, indent, "IntensityKFP");
                IntensityKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "IntensityKFP");
            }
            if (RangeKFP != null)
            {
                YptXml.OpenTag(sb, indent, "RangeKFP");
                RangeKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "RangeKFP");
            }
            if (CoronaRGBMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "CoronaRGBMinKFP");
                CoronaRGBMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "CoronaRGBMinKFP");
            }
            if (CoronaRGBMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "CoronaRGBMaxKFP");
                CoronaRGBMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "CoronaRGBMaxKFP");
            }
            if (CoronaIntensityKFP != null)
            {
                YptXml.OpenTag(sb, indent, "CoronaIntensityKFP");
                CoronaIntensityKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "CoronaIntensityKFP");
            }
            if (CoronaSizeKFP != null)
            {
                YptXml.OpenTag(sb, indent, "CoronaSizeKFP");
                CoronaSizeKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "CoronaSizeKFP");
            }
            if (CoronaFlareKFP != null)
            {
                YptXml.OpenTag(sb, indent, "CoronaFlareKFP");
                CoronaFlareKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "CoronaFlareKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            CoronaZBias = Xml.GetChildFloatAttribute(node, "CoronaZBias");
            CoronaUseLightColour = (byte)Xml.GetChildUIntAttribute(node, "CoronaUseLightColour");
            ColourFromParticle = (byte)Xml.GetChildUIntAttribute(node, "ColourFromParticle");
            ColourPerFrame = (byte)Xml.GetChildUIntAttribute(node, "ColourPerFrame");
            IntensityPerFrame = (byte)Xml.GetChildUIntAttribute(node, "IntensityPerFrame");
            RangePerFrame = (byte)Xml.GetChildUIntAttribute(node, "RangePerFrame");
            CastsShadows = (byte)Xml.GetChildUIntAttribute(node, "CastsShadows");
            CoronaNotInReflection = (byte)Xml.GetChildUIntAttribute(node, "CoronaNotInReflection");
            CoronaOnlyInReflection = (byte)Xml.GetChildUIntAttribute(node, "CoronaOnlyInReflection");
            LightType = Xml.GetChildIntAttribute(node, "LightType");

            RGBMinKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("RGBMinKFP");
            if (pnode0 != null)
            {
                RGBMinKFP.ReadXml(pnode0);
            }

            RGBMaxKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("RGBMaxKFP");
            if (pnode1 != null)
            {
                RGBMaxKFP.ReadXml(pnode1);
            }

            IntensityKFP = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("IntensityKFP");
            if (pnode2 != null)
            {
                IntensityKFP.ReadXml(pnode2);
            }

            RangeKFP = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("RangeKFP");
            if (pnode3 != null)
            {
                RangeKFP.ReadXml(pnode3);
            }

            CoronaRGBMinKFP = new ParticleKeyframeProp();
            var pnode4 = node.SelectSingleNode("CoronaRGBMinKFP");
            if (pnode4 != null)
            {
                CoronaRGBMinKFP.ReadXml(pnode4);
            }

            CoronaRGBMaxKFP = new ParticleKeyframeProp();
            var pnode5 = node.SelectSingleNode("CoronaRGBMaxKFP");
            if (pnode5 != null)
            {
                CoronaRGBMaxKFP.ReadXml(pnode5);
            }

            CoronaIntensityKFP = new ParticleKeyframeProp();
            var pnode6 = node.SelectSingleNode("CoronaIntensityKFP");
            if (pnode6 != null)
            {
                CoronaIntensityKFP.ReadXml(pnode6);
            }

            CoronaSizeKFP = new ParticleKeyframeProp();
            var pnode7 = node.SelectSingleNode("CoronaSizeKFP");
            if (pnode7 != null)
            {
                CoronaSizeKFP.ReadXml(pnode7);
            }

            CoronaFlareKFP = new ParticleKeyframeProp();
            var pnode8 = node.SelectSingleNode("CoronaFlareKFP");
            if (pnode8 != null)
            {
                CoronaFlareKFP.ReadXml(pnode8);
            }

            CreateKeyframeProps(RGBMinKFP, RGBMaxKFP, IntensityKFP, RangeKFP, CoronaRGBMinKFP, CoronaRGBMaxKFP, CoronaIntensityKFP, CoronaSizeKFP, CoronaFlareKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, RGBMinKFP),
                new Tuple<long, IResourceBlock>(192, RGBMaxKFP),
                new Tuple<long, IResourceBlock>(336, IntensityKFP),
                new Tuple<long, IResourceBlock>(480, RangeKFP),
                new Tuple<long, IResourceBlock>(624, CoronaRGBMinKFP),
                new Tuple<long, IResourceBlock>(768, CoronaRGBMaxKFP),
                new Tuple<long, IResourceBlock>(912, CoronaIntensityKFP),
                new Tuple<long, IResourceBlock>(1056, CoronaSizeKFP),
                new Tuple<long, IResourceBlock>(1200, CoronaFlareKFP)
            };
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourModel : ParticleBehaviour
    {
        // ptxd_Model
        public override long BlockLength => 0x40;

        // structure data
        public uint ColourControlShaderID { get; set; }
        public float CameraShrink { get; set; }
        public float ShadowCastIntensity { get; set; }
        public byte DisableDraw { get; set; }
        public byte padding00 { get; set; }
        public short padding01 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            ColourControlShaderID = reader.ReadUInt32();
            CameraShrink = reader.ReadSingle();
            ShadowCastIntensity = reader.ReadSingle();
            DisableDraw = reader.ReadByte();
            padding00 = reader.ReadByte();
            padding01 = reader.ReadInt16();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(ColourControlShaderID);
            writer.Write(CameraShrink);
            writer.Write(ShadowCastIntensity);
            writer.Write(DisableDraw);
            writer.Write(padding00);
            writer.Write(padding01);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "CameraShrink", FloatUtil.ToString(CameraShrink));
            YptXml.ValueTag(sb, indent, "ShadowCastIntensity", FloatUtil.ToString(ShadowCastIntensity));
            YptXml.ValueTag(sb, indent, "DisableDraw", DisableDraw.ToString());
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            CameraShrink = Xml.GetChildFloatAttribute(node, "CameraShrink");
            ShadowCastIntensity = Xml.GetChildFloatAttribute(node, "ShadowCastIntensity");
            DisableDraw = (byte)Xml.GetChildUIntAttribute(node, "DisableDraw");
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourDecal : ParticleBehaviour
    {
        // ptxu_Decal
        public override long BlockLength => 0x180;

        // structure data
        public ParticleKeyframeProp DimensionsKFP { get; set; }
        public ParticleKeyframeProp AlphaKFP { get; set; }
        public int DecalID { get; set; }
        public float VelocityThreshold { get; set; }
        public float TotalLife { get; set; }
        public float FadeInTime { get; set; }
        public float UVMultStart { get; set; }
        public float UVMultEnd { get; set; }
        public float UVMultTime { get; set; }
        public float DuplicateRejectDist { get; set; }
        public byte FlipU { get; set; }
        public byte FlipV { get; set; }
        public byte ProportionalSize { get; set; }
        public byte UseComplexCollision { get; set; }
        public float ProjectionDepth { get; set; }
        public float DistanceScale { get; set; }
        public byte IsDirectional { get; set; }
        public byte padding00 { get; set; }
        public short padding01 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            DimensionsKFP = reader.ReadBlock<ParticleKeyframeProp>();
            AlphaKFP = reader.ReadBlock<ParticleKeyframeProp>();
            DecalID = reader.ReadInt32();
            VelocityThreshold = reader.ReadSingle();
            TotalLife = reader.ReadSingle();
            FadeInTime = reader.ReadSingle();
            UVMultStart = reader.ReadSingle();
            UVMultEnd = reader.ReadSingle();
            UVMultTime = reader.ReadSingle();
            DuplicateRejectDist = reader.ReadSingle();
            FlipU = reader.ReadByte();
            FlipV = reader.ReadByte();
            ProportionalSize = reader.ReadByte();
            UseComplexCollision = reader.ReadByte();
            ProjectionDepth = reader.ReadSingle();
            DistanceScale = reader.ReadSingle();
            IsDirectional = reader.ReadByte();
            padding00 = reader.ReadByte();
            padding01 = reader.ReadInt16();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(DimensionsKFP);
            writer.WriteBlock(AlphaKFP);
            writer.Write(DecalID);
            writer.Write(VelocityThreshold);
            writer.Write(TotalLife);
            writer.Write(FadeInTime);
            writer.Write(UVMultStart);
            writer.Write(UVMultEnd);
            writer.Write(UVMultTime);
            writer.Write(DuplicateRejectDist);
            writer.Write(FlipU);
            writer.Write(FlipV);
            writer.Write(ProportionalSize);
            writer.Write(UseComplexCollision);
            writer.Write(ProjectionDepth);
            writer.Write(DistanceScale);
            writer.Write(IsDirectional);
            writer.Write(padding00);
            writer.Write(padding01);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "DecalID", DecalID.ToString());
            YptXml.ValueTag(sb, indent, "VelocityThreshold", FloatUtil.ToString(VelocityThreshold));
            YptXml.ValueTag(sb, indent, "TotalLife", FloatUtil.ToString(TotalLife));
            YptXml.ValueTag(sb, indent, "FadeInTime", FloatUtil.ToString(FadeInTime));
            YptXml.ValueTag(sb, indent, "UVMultStart", FloatUtil.ToString(UVMultStart));
            YptXml.ValueTag(sb, indent, "UVMultEnd", FloatUtil.ToString(UVMultEnd));
            YptXml.ValueTag(sb, indent, "UVMultTime", FloatUtil.ToString(UVMultTime));
            YptXml.ValueTag(sb, indent, "DuplicateRejectDist", FloatUtil.ToString(DuplicateRejectDist));
            YptXml.ValueTag(sb, indent, "FlipU", FlipU.ToString());
            YptXml.ValueTag(sb, indent, "FlipV", FlipV.ToString());
            YptXml.ValueTag(sb, indent, "ProportionalSize", ProportionalSize.ToString());
            YptXml.ValueTag(sb, indent, "UseComplexCollision", UseComplexCollision.ToString());
            YptXml.ValueTag(sb, indent, "ProjectionDepth", FloatUtil.ToString(ProjectionDepth));
            YptXml.ValueTag(sb, indent, "DistanceScale", FloatUtil.ToString(DistanceScale));
            if (DimensionsKFP != null)
            {
                YptXml.OpenTag(sb, indent, "DimensionsKFP");
                DimensionsKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "DimensionsKFP");
            }
            if (AlphaKFP != null)
            {
                YptXml.OpenTag(sb, indent, "AlphaKFP");
                AlphaKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "AlphaKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            DecalID = Xml.GetChildIntAttribute(node, "DecalID");
            VelocityThreshold = Xml.GetChildFloatAttribute(node, "VelocityThreshold");
            TotalLife = Xml.GetChildFloatAttribute(node, "TotalLife");
            FadeInTime = Xml.GetChildFloatAttribute(node, "FadeInTime");
            UVMultStart = Xml.GetChildFloatAttribute(node, "UVMultStart");
            UVMultEnd = Xml.GetChildFloatAttribute(node, "UVMultEnd");
            UVMultTime = Xml.GetChildFloatAttribute(node, "UVMultTime");
            DuplicateRejectDist = Xml.GetChildFloatAttribute(node, "DuplicateRejectDist");
            FlipU = (byte)Xml.GetChildUIntAttribute(node, "FlipU");
            FlipV = (byte)Xml.GetChildUIntAttribute(node, "FlipV");
            ProportionalSize = (byte)Xml.GetChildUIntAttribute(node, "ProportionalSize");
            UseComplexCollision = (byte)Xml.GetChildUIntAttribute(node, "UseComplexCollision");
            ProjectionDepth = Xml.GetChildFloatAttribute(node, "ProjectionDepth");
            DistanceScale = Xml.GetChildFloatAttribute(node, "DistanceScale");

            DimensionsKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("DimensionsKFP");
            if (pnode0 != null)
            {
                DimensionsKFP.ReadXml(pnode0);
            }

            AlphaKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("AlphaKFP");
            if (pnode1 != null)
            {
                AlphaKFP.ReadXml(pnode1);
            }

            CreateKeyframeProps(DimensionsKFP, AlphaKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, DimensionsKFP),
                new Tuple<long, IResourceBlock>(192, AlphaKFP)
            };
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourZCull : ParticleBehaviour
    {
        // ptxu_ZCull
        public override long BlockLength => 0x170;

        // structure data
        public ParticleKeyframeProp HeightKFP { get; set; }
        public ParticleKeyframeProp FadeDistKFP { get; set; }
        public ulong unsued00 { get; set; }
        public int CullMode { get; set; }
        public int ReferenceSpace { get; set; }
        public ulong padding00 { get; set; }
        public ulong padding01 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            HeightKFP = reader.ReadBlock<ParticleKeyframeProp>();
            FadeDistKFP = reader.ReadBlock<ParticleKeyframeProp>();
            unsued00 = reader.ReadUInt64();
            CullMode = reader.ReadInt32();
            ReferenceSpace = reader.ReadInt32();
            padding00 = reader.ReadUInt64();
            padding01 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(HeightKFP);
            writer.WriteBlock(FadeDistKFP);
            writer.Write(unsued00);
            writer.Write(CullMode);
            writer.Write(ReferenceSpace);
            writer.Write(padding00);
            writer.Write(padding01);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "CullMode", CullMode.ToString());
            YptXml.ValueTag(sb, indent, "ReferenceSpace", ReferenceSpace.ToString());
            if (HeightKFP != null)
            {
                YptXml.OpenTag(sb, indent, "HeightKFP");
                HeightKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "HeightKFP");
            }
            if (FadeDistKFP != null)
            {
                YptXml.OpenTag(sb, indent, "FadeDistKFP");
                FadeDistKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "FadeDistKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            CullMode = Xml.GetChildIntAttribute(node, "CullMode");
            ReferenceSpace = Xml.GetChildIntAttribute(node, "ReferenceSpace");

            HeightKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("HeightKFP");
            if (pnode0 != null)
            {
                HeightKFP.ReadXml(pnode0);
            }

            FadeDistKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("FadeDistKFP");
            if (pnode1 != null)
            {
                FadeDistKFP.ReadXml(pnode1);
            }

            CreateKeyframeProps(HeightKFP, FadeDistKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, HeightKFP),
                new Tuple<long, IResourceBlock>(192, FadeDistKFP)
            };
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourNoise : ParticleBehaviour
    {
        // ptxu_Noise
        public override long BlockLength => 0x280;

        // structure data
        public ParticleKeyframeProp PosNoiseMinKFP { get; set; }
        public ParticleKeyframeProp PosNoiseMaxKFP { get; set; }
        public ParticleKeyframeProp VelNoiseMinKFP { get; set; }
        public ParticleKeyframeProp VelNoiseMaxKFP { get; set; }
        public uint ReferenceSpace { get; set; }
        public byte KeepConstantSpeed { get; set; }
        public byte padding00 { get; set; }
        public short padding01 { get; set; }
        public ulong padding02 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            PosNoiseMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            PosNoiseMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            VelNoiseMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            VelNoiseMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            ReferenceSpace = reader.ReadUInt32();
            KeepConstantSpeed = reader.ReadByte();
            padding00 = reader.ReadByte();
            padding01 = reader.ReadInt16();
            padding02 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(PosNoiseMinKFP);
            writer.WriteBlock(PosNoiseMaxKFP);
            writer.WriteBlock(VelNoiseMinKFP);
            writer.WriteBlock(VelNoiseMaxKFP);
            writer.Write(ReferenceSpace);
            writer.Write(KeepConstantSpeed);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.Write(padding02);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "ReferenceSpace", ReferenceSpace.ToString());
            YptXml.ValueTag(sb, indent, "KeepConstantSpeed", KeepConstantSpeed.ToString());
            if (PosNoiseMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "PosNoiseMinKFP");
                PosNoiseMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "PosNoiseMinKFP");
            }
            if (PosNoiseMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "PosNoiseMaxKFP");
                PosNoiseMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "PosNoiseMaxKFP");
            }
            if (VelNoiseMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "VelNoiseMinKFP");
                VelNoiseMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "VelNoiseMinKFP");
            }
            if (VelNoiseMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "VelNoiseMaxKFP");
                VelNoiseMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "VelNoiseMaxKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            ReferenceSpace = Xml.GetChildUIntAttribute(node, "ReferenceSpace");
            KeepConstantSpeed = (byte)Xml.GetChildUIntAttribute(node, "KeepConstantSpeed");

            PosNoiseMinKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("PosNoiseMinKFP");
            if (pnode0 != null)
            {
                PosNoiseMinKFP.ReadXml(pnode0);
            }

            PosNoiseMaxKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("PosNoiseMaxKFP");
            if (pnode1 != null)
            {
                PosNoiseMaxKFP.ReadXml(pnode1);
            }

            VelNoiseMinKFP = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("VelNoiseMinKFP");
            if (pnode2 != null)
            {
                VelNoiseMinKFP.ReadXml(pnode2);
            }

            VelNoiseMaxKFP = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("VelNoiseMaxKFP");
            if (pnode3 != null)
            {
                VelNoiseMaxKFP.ReadXml(pnode3);
            }

            CreateKeyframeProps(PosNoiseMinKFP, PosNoiseMaxKFP, VelNoiseMinKFP, VelNoiseMaxKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, PosNoiseMinKFP),
                new Tuple<long, IResourceBlock>(192, PosNoiseMaxKFP),
                new Tuple<long, IResourceBlock>(336, VelNoiseMinKFP),
                new Tuple<long, IResourceBlock>(480, VelNoiseMaxKFP)
            };
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourAttractor : ParticleBehaviour
    {
        // ptxu_Attractor
        public override long BlockLength => 0xC0;

        // structure data
        public ParticleKeyframeProp StrengthKFP { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            StrengthKFP = reader.ReadBlock<ParticleKeyframeProp>();


        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(StrengthKFP);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            if (StrengthKFP != null)
            {
                YptXml.OpenTag(sb, indent, "StrengthKFP");
                StrengthKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "StrengthKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            StrengthKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("StrengthKFP");
            if (pnode0 != null)
            {
                StrengthKFP.ReadXml(pnode0);
            }

            CreateKeyframeProps(StrengthKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, StrengthKFP)
            };
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourTrail : ParticleBehaviour
    {
        // ptxd_Trail
        public override long BlockLength => 0xF0;

        // structure data
        public ParticleKeyframeProp TexInfoKFP { get; set; }
        public Vector3 AlignAxis { get; set; }
        public uint padding00 { get; set; }
        public int AlignmentMode { get; set; }
        public int TessellationU { get; set; }
        public int TessellationV { get; set; }
        public float SmoothnessX { get; set; }
        public float SmoothnessY { get; set; }
        public float ProjectionDepth { get; set; }
        public float ShadowCastIntensity { get; set; }
        public byte FlipU { get; set; }
        public byte FlipV { get; set; }
        public byte WrapTextureOverParticleLife { get; set; }
        public byte DisableDraw { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            TexInfoKFP = reader.ReadBlock<ParticleKeyframeProp>();
            AlignAxis = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            padding00 = reader.ReadUInt32();
            AlignmentMode = reader.ReadInt32();
            TessellationU = reader.ReadInt32();
            TessellationV = reader.ReadInt32();
            SmoothnessX = reader.ReadSingle();
            SmoothnessY = reader.ReadSingle();
            ProjectionDepth = reader.ReadSingle();
            ShadowCastIntensity = reader.ReadSingle();
            FlipU = reader.ReadByte();
            FlipV = reader.ReadByte();
            WrapTextureOverParticleLife = reader.ReadByte();
            DisableDraw = reader.ReadByte();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(TexInfoKFP);
            writer.Write(AlignAxis.X);
            writer.Write(AlignAxis.Y);
            writer.Write(AlignAxis.Z);
            writer.Write(padding00);
            writer.Write(AlignmentMode);
            writer.Write(TessellationU);
            writer.Write(TessellationV);
            writer.Write(SmoothnessX);
            writer.Write(SmoothnessY);
            writer.Write(ProjectionDepth);
            writer.Write(ShadowCastIntensity);
            writer.Write(FlipU);
            writer.Write(FlipV);
            writer.Write(WrapTextureOverParticleLife);
            writer.Write(DisableDraw);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            RelXml.SelfClosingTag(sb, indent, "AlignAxis " + FloatUtil.GetVector3XmlString(AlignAxis));
            YptXml.ValueTag(sb, indent, "AlignmentMode", AlignmentMode.ToString());
            YptXml.ValueTag(sb, indent, "TessellationU", TessellationU.ToString());
            YptXml.ValueTag(sb, indent, "TessellationV", TessellationV.ToString());
            YptXml.ValueTag(sb, indent, "SmoothnessX", FloatUtil.ToString(SmoothnessX));
            YptXml.ValueTag(sb, indent, "SmoothnessY", FloatUtil.ToString(SmoothnessY));
            YptXml.ValueTag(sb, indent, "ProjectionDepth", FloatUtil.ToString(ProjectionDepth));
            YptXml.ValueTag(sb, indent, "ShadowCastIntensity", FloatUtil.ToString(ShadowCastIntensity));
            YptXml.ValueTag(sb, indent, "FlipU", FlipU.ToString());
            YptXml.ValueTag(sb, indent, "FlipV", FlipV.ToString());
            YptXml.ValueTag(sb, indent, "WrapTextureOverParticleLife", WrapTextureOverParticleLife.ToString());
            YptXml.ValueTag(sb, indent, "DisableDraw", DisableDraw.ToString());
            if (TexInfoKFP != null)
            {
                YptXml.OpenTag(sb, indent, "TexInfoKFP");
                TexInfoKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "TexInfoKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            AlignAxis = Xml.GetChildVector3Attributes(node, "AlignAxis");
            AlignmentMode = Xml.GetChildIntAttribute(node, "AlignmentMode");
            TessellationU = Xml.GetChildIntAttribute(node, "TessellationU");
            TessellationV = Xml.GetChildIntAttribute(node, "TessellationV");
            SmoothnessX = Xml.GetChildFloatAttribute(node, "SmoothnessX");
            SmoothnessY = Xml.GetChildFloatAttribute(node, "SmoothnessY");
            ProjectionDepth = Xml.GetChildFloatAttribute(node, "ProjectionDepth");
            ShadowCastIntensity = Xml.GetChildFloatAttribute(node, "ShadowCastIntensity");
            FlipU = (byte)Xml.GetChildUIntAttribute(node, "FlipU");
            FlipV = (byte)Xml.GetChildUIntAttribute(node, "FlipV");
            WrapTextureOverParticleLife = (byte)Xml.GetChildUIntAttribute(node, "WrapTextureOverParticleLife");
            DisableDraw = (byte)Xml.GetChildUIntAttribute(node, "DisableDraw");

            TexInfoKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("TexInfoKFP");
            if (pnode0 != null)
            {
                TexInfoKFP.ReadXml(pnode0);
            }

            CreateKeyframeProps(TexInfoKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, TexInfoKFP)
            };
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourFogVolume : ParticleBehaviour
    {
        // ptxu_FogVolume
        public override long BlockLength => 0x430;

        // structure data
        public ParticleKeyframeProp RGBTintMinKFP { get; set; }
        public ParticleKeyframeProp RGBTintMaxKFP { get; set; }
        public ParticleKeyframeProp DensityRangeKFP { get; set; }
        public ParticleKeyframeProp ScaleMinKFP { get; set; }
        public ParticleKeyframeProp ScaleMaxKFP { get; set; }
        public ParticleKeyframeProp RotationMinKFP { get; set; }
        public ParticleKeyframeProp RotationMaxKFP { get; set; }
        public float Falloff { get; set; } // 1.0f, 3.0f
        public float HDRMult { get; set; } // 1.0f
        public int LightingType { get; set; }
        public byte ColourTintFromParticle { get; set; }
        public byte SortWithParticles { get; set; }
        public byte UseGroundFogColour { get; set; }
        public byte UseEffectEvoValues { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            RGBTintMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            RGBTintMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            DensityRangeKFP = reader.ReadBlock<ParticleKeyframeProp>();
            ScaleMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            ScaleMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            RotationMinKFP = reader.ReadBlock<ParticleKeyframeProp>();
            RotationMaxKFP = reader.ReadBlock<ParticleKeyframeProp>();
            Falloff = reader.ReadSingle();
            HDRMult = reader.ReadSingle();
            LightingType = reader.ReadInt32();
            ColourTintFromParticle = reader.ReadByte();
            SortWithParticles = reader.ReadByte();
            UseGroundFogColour = reader.ReadByte();
            UseEffectEvoValues = reader.ReadByte();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(RGBTintMinKFP);
            writer.WriteBlock(RGBTintMaxKFP);
            writer.WriteBlock(DensityRangeKFP);
            writer.WriteBlock(ScaleMinKFP);
            writer.WriteBlock(ScaleMaxKFP);
            writer.WriteBlock(RotationMinKFP);
            writer.WriteBlock(RotationMaxKFP);
            writer.Write(Falloff);
            writer.Write(HDRMult);
            writer.Write(LightingType);
            writer.Write(ColourTintFromParticle);
            writer.Write(SortWithParticles);
            writer.Write(UseGroundFogColour);
            writer.Write(UseEffectEvoValues);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Falloff", FloatUtil.ToString(Falloff));
            YptXml.ValueTag(sb, indent, "HDRMult", FloatUtil.ToString(HDRMult));
            YptXml.ValueTag(sb, indent, "LightingType", LightingType.ToString());
            YptXml.ValueTag(sb, indent, "ColourTintFromParticle", ColourTintFromParticle.ToString());
            YptXml.ValueTag(sb, indent, "SortWithParticles", SortWithParticles.ToString());
            YptXml.ValueTag(sb, indent, "UseGroundFogColour", UseGroundFogColour.ToString());
            YptXml.ValueTag(sb, indent, "UseEffectEvoValues", UseEffectEvoValues.ToString());
            if (RGBTintMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "RGBTintMinKFP");
                RGBTintMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "RGBTintMinKFP");
            }
            if (RGBTintMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "RGBTintMaxKFP");
                RGBTintMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "RGBTintMaxKFP");
            }
            if (DensityRangeKFP != null)
            {
                YptXml.OpenTag(sb, indent, "DensityRangeKFP");
                DensityRangeKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "DensityRangeKFP");
            }
            if (ScaleMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "ScaleMinKFP");
                ScaleMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "ScaleMinKFP");
            }
            if (ScaleMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "ScaleMaxKFP");
                ScaleMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "ScaleMaxKFP");
            }
            if (RotationMinKFP != null)
            {
                YptXml.OpenTag(sb, indent, "RotationMinKFP");
                RotationMinKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "RotationMinKFP");
            }
            if (RotationMaxKFP != null)
            {
                YptXml.OpenTag(sb, indent, "RotationMaxKFP");
                RotationMaxKFP.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "RotationMaxKFP");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Falloff = Xml.GetChildFloatAttribute(node, "Falloff");
            HDRMult = Xml.GetChildFloatAttribute(node, "HDRMult");
            LightingType = Xml.GetChildIntAttribute(node, "LightingType");
            ColourTintFromParticle = (byte)Xml.GetChildUIntAttribute(node, "ColourTintFromParticle");
            SortWithParticles = (byte)Xml.GetChildUIntAttribute(node, "SortWithParticles");
            UseGroundFogColour = (byte)Xml.GetChildUIntAttribute(node, "UseGroundFogColour");
            UseEffectEvoValues = (byte)Xml.GetChildUIntAttribute(node, "UseEffectEvoValues");

            RGBTintMinKFP = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("RGBTintMinKFP");
            if (pnode0 != null)
            {
                RGBTintMinKFP.ReadXml(pnode0);
            }

            RGBTintMaxKFP = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("RGBTintMaxKFP");
            if (pnode1 != null)
            {
                RGBTintMaxKFP.ReadXml(pnode1);
            }

            DensityRangeKFP = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("DensityRangeKFP");
            if (pnode2 != null)
            {
                DensityRangeKFP.ReadXml(pnode2);
            }

            ScaleMinKFP = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("ScaleMinKFP");
            if (pnode3 != null)
            {
                ScaleMinKFP.ReadXml(pnode3);
            }

            ScaleMaxKFP = new ParticleKeyframeProp();
            var pnode4 = node.SelectSingleNode("ScaleMaxKFP");
            if (pnode4 != null)
            {
                ScaleMaxKFP.ReadXml(pnode4);
            }

            RotationMinKFP = new ParticleKeyframeProp();
            var pnode5 = node.SelectSingleNode("RotationMinKFP");
            if (pnode5 != null)
            {
                RotationMinKFP.ReadXml(pnode5);
            }

            RotationMaxKFP = new ParticleKeyframeProp();
            var pnode6 = node.SelectSingleNode("RotationMaxKFP");
            if (pnode6 != null)
            {
                RotationMaxKFP.ReadXml(pnode6);
            }

            CreateKeyframeProps(RGBTintMinKFP, RGBTintMaxKFP, DensityRangeKFP, ScaleMinKFP, ScaleMaxKFP, RotationMinKFP, RotationMaxKFP);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(16, KeyframeProps),
                new Tuple<long, IResourceBlock>(48, RGBTintMinKFP),
                new Tuple<long, IResourceBlock>(192, RGBTintMaxKFP),
                new Tuple<long, IResourceBlock>(336, DensityRangeKFP),
                new Tuple<long, IResourceBlock>(480, ScaleMinKFP),
                new Tuple<long, IResourceBlock>(624, ScaleMaxKFP),
                new Tuple<long, IResourceBlock>(768, RotationMinKFP),
                new Tuple<long, IResourceBlock>(912, RotationMaxKFP)
            };
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourRiver : ParticleBehaviour
    {
        // ptxu_River
        public override long BlockLength => 0x40;

        // structure data
        public float VelocityMult { get; set; }
        public float Influence { get; set; }
        public ulong padding00 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            VelocityMult = reader.ReadSingle();
            Influence = reader.ReadSingle();
            padding00 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(VelocityMult);
            writer.Write(Influence);
            writer.Write(padding00);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "VelocityMult", FloatUtil.ToString(VelocityMult));
            YptXml.ValueTag(sb, indent, "Influence", FloatUtil.ToString(Influence));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            VelocityMult = Xml.GetChildFloatAttribute(node, "VelocityMult");
            Influence = Xml.GetChildFloatAttribute(node, "Influence");
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourDecalPool : ParticleBehaviour
    {
        // ptxu_DecalPool
        public override long BlockLength => 0x50;

        // structure data
        public float VelocityThreshold { get; set; }
        public int LiquidType { get; set; }
        public int DecalID { get; set; }
        public float StartSize { get; set; }
        public float EndSize { get; set; }
        public float GrowthRate { get; set; }
        public ulong padding00 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            VelocityThreshold = reader.ReadSingle();
            LiquidType = reader.ReadInt32();
            DecalID = reader.ReadInt32();
            StartSize = reader.ReadSingle();
            EndSize = reader.ReadSingle();
            GrowthRate = reader.ReadSingle();
            padding00 = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(VelocityThreshold);
            writer.Write(LiquidType);
            writer.Write(DecalID);
            writer.Write(StartSize);
            writer.Write(EndSize);
            writer.Write(GrowthRate);
            writer.Write(padding00);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "VelocityThreshold", FloatUtil.ToString(VelocityThreshold));
            YptXml.ValueTag(sb, indent, "LiquidType", LiquidType.ToString());
            YptXml.ValueTag(sb, indent, "DecalID", DecalID.ToString());
            YptXml.ValueTag(sb, indent, "StartSize", FloatUtil.ToString(StartSize));
            YptXml.ValueTag(sb, indent, "EndSize", FloatUtil.ToString(EndSize));
            YptXml.ValueTag(sb, indent, "GrowthRate", FloatUtil.ToString(GrowthRate));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            VelocityThreshold = Xml.GetChildFloatAttribute(node, "VelocityThreshold");
            LiquidType = Xml.GetChildIntAttribute(node, "LiquidType");
            DecalID = Xml.GetChildIntAttribute(node, "DecalID");
            StartSize = Xml.GetChildFloatAttribute(node, "StartSize");
            EndSize = Xml.GetChildFloatAttribute(node, "EndSize");
            GrowthRate = Xml.GetChildFloatAttribute(node, "GrowthRate");
        }
    }

    [TC(typeof(EXP))]
    public class ParticleBehaviourLiquid : ParticleBehaviour
    {
        // ptxu_Liquid
        public override long BlockLength => 0x50;

        // structure data
        public float VelocityThreshold { get; set; }
        public int LiquidType { get; set; }
        public float PoolStartSize { get; set; }
        public float PoolEndSize { get; set; }
        public float PoolGrowthRate { get; set; }
        public float TrailWidthMin { get; set; }
        public float TrailWidthMax { get; set; }
        public uint padding00 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            VelocityThreshold = reader.ReadSingle();
            LiquidType = reader.ReadInt32();
            PoolStartSize = reader.ReadSingle();
            PoolEndSize = reader.ReadSingle();
            PoolGrowthRate = reader.ReadSingle();
            TrailWidthMin = reader.ReadSingle();
            TrailWidthMax = reader.ReadSingle();
            padding00 = reader.ReadUInt32();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(VelocityThreshold);
            writer.Write(LiquidType);
            writer.Write(PoolStartSize);
            writer.Write(PoolEndSize);
            writer.Write(PoolGrowthRate);
            writer.Write(TrailWidthMin);
            writer.Write(TrailWidthMax);
            writer.Write(padding00);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "VelocityThreshold", FloatUtil.ToString(VelocityThreshold));
            YptXml.ValueTag(sb, indent, "LiquidType", LiquidType.ToString());
            YptXml.ValueTag(sb, indent, "PoolStartSize", FloatUtil.ToString(PoolStartSize));
            YptXml.ValueTag(sb, indent, "PoolEndSize", FloatUtil.ToString(PoolEndSize));
            YptXml.ValueTag(sb, indent, "PoolGrowthRate", FloatUtil.ToString(PoolGrowthRate));
            YptXml.ValueTag(sb, indent, "TrailWidthMin", FloatUtil.ToString(TrailWidthMin));
            YptXml.ValueTag(sb, indent, "TrailWidthMax", FloatUtil.ToString(TrailWidthMax));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            VelocityThreshold = Xml.GetChildFloatAttribute(node, "VelocityThreshold");
            LiquidType = Xml.GetChildIntAttribute(node, "LiquidType");
            PoolStartSize = Xml.GetChildFloatAttribute(node, "PoolStartSize");
            PoolEndSize = Xml.GetChildFloatAttribute(node, "PoolEndSize");
            PoolGrowthRate = Xml.GetChildFloatAttribute(node, "PoolGrowthRate");
            TrailWidthMin = Xml.GetChildFloatAttribute(node, "TrailWidthMin");
            TrailWidthMax = Xml.GetChildFloatAttribute(node, "TrailWidthMax");
        }
    }












    public enum ParticleShaderVarType : byte
    {
        Vector2 = 2,
        Vector4 = 4,
        Texture = 6,
        Keyframe = 7,
    }

    [TC(typeof(EXP))] public class ParticleShaderVar : ResourceSystemBlock, IResourceXXSystemBlock, IMetaXmlItem
    {
        // datBase
        // ptxShaderVar
        public override long BlockLength => 24;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public MetaHash Name { get; set; }
        public ParticleShaderVarType Type { get; set; }
        public byte Unknown_15h; // 0x00
        public ushort Unknown_16h; // 0x0000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            VFT = reader.ReadUInt32();
            Unknown_4h = reader.ReadUInt32();
            Unknown_8h = reader.ReadUInt64();
            Name = reader.ReadUInt32();
            Type = (ParticleShaderVarType)reader.ReadByte();
            Unknown_15h = reader.ReadByte();
            Unknown_16h = reader.ReadUInt16();

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //switch (Name) //parameter name
            //{
            //    case 0xea057402: // wraplightingterm
            //    case 0x0b3045be: // softness
            //    case 0x91bf3028: // superalpha
            //    case 0x4a8a0a28: // directionalmult
            //    case 0xf8338e85: // ambientmult
            //    case 0xbfd98c1d: // shadowamount
            //    case 0xc6fe034a: // extralightmult
            //    case 0xf03acb8c: // camerabias
            //    case 0x81634888: // camerashrink
            //    case 0xb695f45c: // normalarc
            //    case 0x403390ea: // dirnormalbias
            //    case 0x18ca6c12: // softnesscurve
            //    case 0x1458f27b: // softnessshadowmult
            //    case 0xa781a38b: // softnessshadowoffset
            //    case 0x77b842ed: // normalmapmult
            //    case 0x7b483bc5: // alphacutoffminmax
            //    case 0x6a1dbec3: // rg_blendstartdistance
            //    case 0xba5af058: // rg_blendenddistance
            //    case 0xdf7cc018: // refractionmap
            //    case 0xb36327d1: // normalspecmap
            //    case 0x0df47048: // diffusetex2
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_15h != 0)
            //{ }//no hit
            //if (Unknown_16h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(VFT);
            writer.Write(Unknown_4h);
            writer.Write(Unknown_8h);
            writer.Write(Name);
            writer.Write((byte)Type);
            writer.Write(Unknown_15h);
            writer.Write(Unknown_16h);
        }
        public virtual void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.ValueTag(sb, indent, "Type", Type.ToString());
            YptXml.StringTag(sb, indent, "Name", YptXml.HashString(Name));
        }
        public virtual void ReadXml(XmlNode node)
        {
            Type = Xml.GetEnumValue<ParticleShaderVarType>(Xml.GetChildStringAttribute(node, "Type"));
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
        }
        public static void WriteXmlNode(ParticleShaderVar v, StringBuilder sb, int indent, string name)
        {
            if (v != null)
            {
                YptXml.OpenTag(sb, indent, name);
                v.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, name);
            }
        }
        public static ParticleShaderVar ReadXmlNode(XmlNode node)
        {
            if (node != null)
            {
                var typestr = Xml.GetChildStringAttribute(node, "Type");
                var type = Xml.GetEnumValue<ParticleShaderVarType>(typestr);
                var s = Create(type);
                if (s != null)
                {
                    s.ReadXml(node);
                }
                return s;
            }
            return null;
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 20;
            var type = (ParticleShaderVarType)reader.ReadByte();
            reader.Position -= 21;

            return Create(type);
        }
        public static ParticleShaderVar Create(ParticleShaderVarType type)
        {
            switch (type)
            {
                case ParticleShaderVarType.Vector2:
                case ParticleShaderVarType.Vector4: return new ParticleShaderVarVector();
                case ParticleShaderVarType.Texture: return new ParticleShaderVarTexture();
                case ParticleShaderVarType.Keyframe: return new ParticleShaderVarKeyframe();
                default: return null;// throw new Exception("Unknown shader var type");
            }
        }

        public override string ToString()
        {
            return Name.ToString() + ": " + Type.ToString();
        }

    }

    [TC(typeof(EXP))]
    public class ParticleShaderVarVector : ParticleShaderVar
    {
        // ptxShaderVarVector
        public override long BlockLength => 0x40;

        // structure data
        public uint ShaderVarID { get; set; }
        public byte IsKeyFrameable { get; set; }
        public byte OwnsInfo { get; set; }
        public short padding00 { get; set; }
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; }
        public uint Unknown_2Ch { get; set; }
        public float VectorX { get; set; }
        public float VectorY { get; set; }
        public float VectorZ { get; set; }
        public float VectorW { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            ShaderVarID = reader.ReadUInt32();
            IsKeyFrameable = reader.ReadByte();
            OwnsInfo = reader.ReadByte();
            padding00 = reader.ReadInt16();
            Unknown_20h = reader.ReadUInt32();
            Unknown_24h = reader.ReadUInt32();
            Unknown_28h = reader.ReadUInt32();
            Unknown_2Ch = reader.ReadUInt32();
            VectorX = reader.ReadSingle();
            VectorY = reader.ReadSingle();
            VectorZ = reader.ReadSingle();
            VectorW = reader.ReadSingle();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(ShaderVarID);
            writer.Write(IsKeyFrameable);
            writer.Write(OwnsInfo);
            writer.Write(padding00);
            writer.Write(Unknown_20h);
            writer.Write(Unknown_24h);
            writer.Write(Unknown_28h);
            writer.Write(Unknown_2Ch);
            writer.Write(VectorX);
            writer.Write(VectorY);
            writer.Write(VectorZ);
            writer.Write(VectorW);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "ShaderVarID", ShaderVarID.ToString());
            YptXml.ValueTag(sb, indent, "IsKeyFrameable", IsKeyFrameable.ToString());
            YptXml.ValueTag(sb, indent, "VectorX", FloatUtil.ToString(VectorX));
            YptXml.ValueTag(sb, indent, "VectorY", FloatUtil.ToString(VectorY));
            YptXml.ValueTag(sb, indent, "VectorZ", FloatUtil.ToString(VectorZ));
            YptXml.ValueTag(sb, indent, "VectorW", FloatUtil.ToString(VectorW));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            ShaderVarID = Xml.GetChildUIntAttribute(node, "ShaderVarID");
            IsKeyFrameable = (byte)Xml.GetChildUIntAttribute(node, "IsKeyFrameable");
            VectorX = Xml.GetChildFloatAttribute(node, "VectorX");
            VectorY = Xml.GetChildFloatAttribute(node, "VectorY");
            VectorZ = Xml.GetChildFloatAttribute(node, "VectorZ");
            VectorW = Xml.GetChildFloatAttribute(node, "VectorW");
        }
    }


    [TC(typeof(EXP))] public class ParticleShaderVarTexture : ParticleShaderVar
    {
        // ptxShaderVarTexture
        public override long BlockLength => 0x40;

        // structure data
        public uint ShaderVarID { get; set; }
        public byte IsKeyframeable { get; set; }
        public byte OwnsInfo { get; set; }
        public short padding00 { get; set; }
        public uint padding01 { get; set; }
        public uint padding02 { get; set; }


        public ulong TexturePointer { get; set; }
        public ulong TextureNamePointer { get; set; }
        public MetaHash TextureNameHash { get; set; }
        public byte ExternalReference { get; set; }
        public byte padding05 { get; set; }
        public short padding06 { get; set; }

        // reference data
        public Texture Texture { get; set; }
        public string_r TextureName { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            ShaderVarID = reader.ReadUInt32();
            IsKeyframeable = reader.ReadByte();
            OwnsInfo = reader.ReadByte();
            padding00 = reader.ReadInt16();
            padding01 = reader.ReadUInt32();
            padding02 = reader.ReadUInt32();
            TexturePointer = reader.ReadUInt64();
            TextureNamePointer = reader.ReadUInt64();
            TextureNameHash = reader.ReadUInt32();
            ExternalReference = reader.ReadByte();
            padding05 = reader.ReadByte();
            padding06 = reader.ReadInt16();

            // read reference data
            Texture = reader.ReadBlockAt<Texture>(TexturePointer);
            TextureName = reader.ReadBlockAt<string_r>(TextureNamePointer);
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            TexturePointer = (ulong)(Texture != null ? Texture.FilePosition : 0);
            TextureNamePointer = (ulong)(TextureName != null ? TextureName.FilePosition : 0);

            // write structure data
            writer.Write(ShaderVarID);
            writer.Write(IsKeyframeable);
            writer.Write(OwnsInfo);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.Write(padding02);
            writer.Write(TexturePointer);
            writer.Write(TextureNamePointer);
            writer.Write(TextureNameHash);
            writer.Write(ExternalReference);
            writer.Write(padding05);
            writer.Write(padding06);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "ShaderVarID", ShaderVarID.ToString());
            YptXml.ValueTag(sb, indent, "IsKeyframeable", IsKeyframeable.ToString());
            YptXml.ValueTag(sb, indent, "ExternalReference", ExternalReference.ToString());
            YptXml.StringTag(sb, indent, "TextureName", YptXml.XmlEscape(TextureName?.Value ?? ""));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            ShaderVarID = Xml.GetChildUIntAttribute(node, "ShaderVarID");
            IsKeyframeable = (byte)Xml.GetChildUIntAttribute(node, "IsKeyframeable");
            ExternalReference = (byte)Xml.GetChildUIntAttribute(node, "ExternalReference");
            TextureName = (string_r)Xml.GetChildInnerText(node, "TextureName"); if (TextureName.Value == null) TextureName = null;
            TextureNameHash = JenkHash.GenHash(TextureName?.Value ?? "");
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Texture != null) list.Add(Texture);
            if (TextureName != null) list.Add(TextureName);
            return list.ToArray();
        }
    }

    [TC(typeof(EXP))] public class ParticleShaderVarKeyframe : ParticleShaderVar
    {
        // ptxShaderVarKeyframe
        public override long BlockLength => 0x50;

        // structure data
        public uint ShaderVarID { get; set; }
        public byte IsKeyframeable { get; set; }
        public byte OwnsInfo { get; set; }
        public short padding00 { get; set; }
        public ulong padding01 { get; set; }
        public ResourceSimpleList64<ParticleShaderVarKeyframeItem> Items { get; set; }
        public ulong padding02 { get; set; }
        public ulong padding03 { get; set; }
        public ulong padding04 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            ShaderVarID = reader.ReadUInt32();
            IsKeyframeable = reader.ReadByte();
            OwnsInfo = reader.ReadByte();
            padding00 = reader.ReadInt16();
            padding01 = reader.ReadUInt64();
            Items = reader.ReadBlock<ResourceSimpleList64<ParticleShaderVarKeyframeItem>>();
            padding02 = reader.ReadUInt64();
            padding03 = reader.ReadUInt64();
            padding04 = reader.ReadUInt64();

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(ShaderVarID);
            writer.Write(IsKeyframeable);
            writer.Write(OwnsInfo);
            writer.Write(padding00);
            writer.Write(padding01);
            writer.WriteBlock(Items);
            writer.Write(padding02);
            writer.Write(padding03);
            writer.Write(padding04);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "ShaderVarID", ShaderVarID.ToString());
            YptXml.ValueTag(sb, indent, "IsKeyframeable", IsKeyframeable.ToString());
            YptXml.WriteItemArray(sb, Items?.data_items, indent, "Items");
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            ShaderVarID = Xml.GetChildUIntAttribute(node, "ShaderVarID");
            IsKeyframeable = (byte)Xml.GetChildUIntAttribute(node, "IsKeyframeable");
            Items = new ResourceSimpleList64<ParticleShaderVarKeyframeItem>();
            Items.data_items = XmlMeta.ReadItemArray<ParticleShaderVarKeyframeItem>(node, "Items");
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x28, Items)
            };
        }
    }

    [TC(typeof(EXP))] public class ParticleShaderVarKeyframeItem : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength => 0x20;

        // structure data
        public float Unknown_0h { get; set; }
        public float Unknown_4h { get; set; }
        public ulong Unknown_8h; // 0x0000000000000000
        public float Unknown_10h { get; set; }
        public uint Unknown_14h; // 0x00000000
        public ulong Unknown_18h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            Unknown_0h = reader.ReadSingle();
            Unknown_4h = reader.ReadSingle();
            Unknown_8h = reader.ReadUInt64();
            Unknown_10h = reader.ReadSingle();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt64();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(Unknown_0h);
            writer.Write(Unknown_4h);
            writer.Write(Unknown_8h);
            writer.Write(Unknown_10h);
            writer.Write(Unknown_14h);
            writer.Write(Unknown_18h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.ValueTag(sb, indent, "Unknown0", FloatUtil.ToString(Unknown_0h));
            YptXml.ValueTag(sb, indent, "Unknown4", FloatUtil.ToString(Unknown_4h));
            YptXml.ValueTag(sb, indent, "Unknown10", FloatUtil.ToString(Unknown_10h));
        }
        public void ReadXml(XmlNode node)
        {
            Unknown_0h = Xml.GetChildFloatAttribute(node, "Unknown0");
            Unknown_4h = Xml.GetChildFloatAttribute(node, "Unknown4");
            Unknown_10h = Xml.GetChildFloatAttribute(node, "Unknown10");
        }
    }
}
