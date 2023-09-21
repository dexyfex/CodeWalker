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
            this.DrawableDictionary = reader.ReadBlockAt<DrawablePtfxDictionary>(this.DrawableDictionaryPointer);
            this.ParticleRuleDictionary = reader.ReadBlockAt<ParticleRuleDictionary>(this.ParticleRuleDictionaryPointer);
            this.EffectRuleDictionary = reader.ReadBlockAt<ParticleEffectRuleDictionary>(this.EmitterRuleDictionaryPointer);
            this.EmitterRuleDictionary = reader.ReadBlockAt<ParticleEmitterRuleDictionary>(this.EffectRuleDictionaryPointer);



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
                    if (ptr.Spawner1 != null)
                    {
                        var efrhash = JenkHash.GenHash(ptr.Spawner1.EffectRuleName?.Value ?? "");
                        if (efrdict.TryGetValue(efrhash, out ParticleEffectRule efr))
                        {
                            ptr.Spawner1.EffectRule = efr;
                        }
                        else if (efrhash != 0)
                        { }
                    }
                    if (ptr.Spawner2 != null)
                    {
                        var efrhash = JenkHash.GenHash(ptr.Spawner2.EffectRuleName?.Value ?? "");
                        if (efrdict.TryGetValue(efrhash, out ParticleEffectRule efr))
                        {
                            ptr.Spawner2.EffectRule = efr;
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
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.ParticleRuleNameHashes = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            this.ParticleRules = reader.ReadBlock<ResourcePointerList64<ParticleRule>>();

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
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.WriteBlock(this.ParticleRuleNameHashes);
            writer.WriteBlock(this.ParticleRules);
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
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.EffectRuleNameHashes = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            this.EffectRules = reader.ReadBlock<ResourcePointerList64<ParticleEffectRule>>();

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
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.WriteBlock(this.EffectRuleNameHashes);
            writer.WriteBlock(this.EffectRules);
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
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.EmitterRuleNameHashes = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            this.EmitterRules = reader.ReadBlock<ResourcePointerList64<ParticleEmitterRule>>();


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
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.WriteBlock(this.EmitterRuleNameHashes);
            writer.WriteBlock(this.EmitterRules);
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
        // ptxParticleRule
        public override long BlockLength => 0x240;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public uint Unknown_10h { get; set; }   // 2, 3, 4, 5, 6, 7, 10, 21
        public uint Unknown_14h; //0x00000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ParticleEffectSpawner Spawner1 { get; set; }
        public ParticleEffectSpawner Spawner2 { get; set; }
        public uint Unknown_100h { get; set; }  // 0, 1, 2
        public uint Unknown_104h { get; set; }  // 0, 1, 7
        public uint Unknown_108h { get; set; }  // 0, 1, 2
        public uint Unknown_10Ch { get; set; }  // eg. 0x00010100
        public uint Unknown_110h; // 0x00000000
        public float Unknown_114h { get; set; } = 1.0f;
        public uint Unknown_118h { get; set; } //index/id
        public uint Unknown_11Ch { get; set; } //index/id
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
        public uint VFT2 { get; set; } = 0x40605c50; // 0x40605c50, 0x40607c70
        public uint Unknown_1B4h = 1; // 0x00000001
        public ulong FxcFilePointer { get; set; }
        public ulong FxcTechniquePointer { get; set; }
        public ulong Unknown_1C8h; // 0x0000000000000000
        public uint Unknown_1D0h { get; set; } //index/id
        public uint Unknown_1D4h; // 0x00000000
        public uint VFT3 { get; set; } = 0x40605b48; // 0x40605b48, 0x40607b68
        public uint Unknown_1DCh = 1; // 0x00000001
        public uint Unknown_1E0h { get; set; }  // 0, 4
        public uint Unknown_1E4h { get; set; }  // 0, 1
        public uint Unknown_1E8h { get; set; }  // eg. 0x00000101
        public uint Unknown_1ECh { get; set; }  // 0, 1
        public ResourcePointerList64<ParticleShaderVar> ShaderVars { get; set; }
        public ulong Unknown_200h = 1; // 0x0000000000000001
        public MetaHash FxcFileHash { get; set; } // ptfx_sprite, ptfx_trail
        public uint Unknown_20Ch; // 0x00000000
        public ResourceSimpleList64<ParticleDrawable> Drawables { get; set; }
        public uint Unknown_220h { get; set; }  // eg. 0x00000202
        public uint Unknown_224h; // 0x00000000
        public ulong Unknown_228h; // 0x0000000000000000
        public ulong Unknown_230h; // 0x0000000000000000
        public ulong Unknown_238h; // 0x0000000000000000

        // reference data
        public string_r Name { get; set; }
        public MetaHash NameHash { get; set; }
        public string_r FxcFile { get; set; } // ptfx_sprite, ptfx_trail
        public string_r FxcTechnique { get; set; }

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
            this.Unknown_114h = reader.ReadSingle();
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
            this.VFT2 = reader.ReadUInt32();
            this.Unknown_1B4h = reader.ReadUInt32();
            this.FxcFilePointer = reader.ReadUInt64();
            this.FxcTechniquePointer = reader.ReadUInt64();
            this.Unknown_1C8h = reader.ReadUInt64();
            this.Unknown_1D0h = reader.ReadUInt32();
            this.Unknown_1D4h = reader.ReadUInt32();
            this.VFT3 = reader.ReadUInt32();
            this.Unknown_1DCh = reader.ReadUInt32();
            this.Unknown_1E0h = reader.ReadUInt32();
            this.Unknown_1E4h = reader.ReadUInt32();
            this.Unknown_1E8h = reader.ReadUInt32();
            this.Unknown_1ECh = reader.ReadUInt32();
            this.ShaderVars = reader.ReadBlock<ResourcePointerList64<ParticleShaderVar>>();
            this.Unknown_200h = reader.ReadUInt64();
            this.FxcFileHash = reader.ReadUInt32();
            this.Unknown_20Ch = reader.ReadUInt32();
            this.Drawables = reader.ReadBlock<ResourceSimpleList64<ParticleDrawable>>();
            this.Unknown_220h = reader.ReadUInt32();
            this.Unknown_224h = reader.ReadUInt32();
            this.Unknown_228h = reader.ReadUInt64();
            this.Unknown_230h = reader.ReadUInt64();
            this.Unknown_238h = reader.ReadUInt64();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(this.NamePointer);
            this.FxcFile = reader.ReadBlockAt<string_r>(this.FxcFilePointer);
            this.FxcTechnique = reader.ReadBlockAt<string_r>(this.FxcTechniquePointer);

            #endregion


            if (!string.IsNullOrEmpty(Name?.Value))
            {
                JenkIndex.Ensure(Name.Value);
            }

            if ((Drawables?.data_items?.Length ?? 0) != 0)
            { }


            #region test

            //var bl1 = BehaviourList1?.data_items?.ToList() ?? new List<ParticleBehaviour>();
            //var bl2 = BehaviourList2?.data_items?.ToList() ?? new List<ParticleBehaviour>();
            //var bl3 = BehaviourList3?.data_items?.ToList() ?? new List<ParticleBehaviour>();
            //var bl4 = BehaviourList4?.data_items?.ToList() ?? new List<ParticleBehaviour>();
            //var bl5 = BehaviourList5?.data_items?.ToList() ?? new List<ParticleBehaviour>();
            //if (bl2.Count != bl3.Count)
            //{ }//no hit
            //foreach (var b in bl1)
            //{
            //    var t = b.Type;
            //    var il2 = bl2.Contains(b);
            //    var il3 = bl3.Contains(b);
            //    var il4 = bl4.Contains(b);
            //    var il5 = bl5.Contains(b);
            //    var render = false;
            //    var extra = false;
            //    var extra2 = false;
            //    switch (t)
            //    {
            //        case ParticleBehaviourType.Sprite:
            //        case ParticleBehaviourType.Model:
            //        case ParticleBehaviourType.Trail:
            //            render = true;
            //            break;
            //    }
            //    switch (t)
            //    {
            //        case ParticleBehaviourType.Collision:
            //        case ParticleBehaviourType.Light:
            //        case ParticleBehaviourType.Decal:
            //        case ParticleBehaviourType.ZCull:
            //        case ParticleBehaviourType.Trail:
            //        case ParticleBehaviourType.FogVolume:
            //        case ParticleBehaviourType.River:
            //        case ParticleBehaviourType.DecalPool:
            //        case ParticleBehaviourType.Liquid:
            //            extra = true;
            //            break;
            //    }
            //    switch (t)
            //    {
            //        case ParticleBehaviourType.Sprite:
            //        case ParticleBehaviourType.Model:
            //        case ParticleBehaviourType.Trail:
            //        case ParticleBehaviourType.FogVolume:
            //            extra2 = true;
            //            break;
            //    }
            //    if (il2 != il3)
            //    { }//no hit
            //    if (il2 == render)
            //    { }//no hit
            //    if (il4 != extra)
            //    { }//no hit
            //    if (il5 != extra2)
            //    { }//no hit
            //}

            //var blc1 = BehaviourList1?.data_items?.Length ?? 0;
            //var blc2 = BehaviourList2?.data_items?.Length ?? 0;
            //for (int i = 0; i < blc2; i++)
            //{
            //    var b = BehaviourList2.data_items[i];
            //    if (!bl1.Contains(b))
            //    { }//no hit
            //}
            //var blc3 = BehaviourList3?.data_items?.Length ?? 0;
            //for (int i = 0; i < blc3; i++)
            //{
            //    var b = BehaviourList3.data_items[i];
            //    if (!bl1.Contains(b))
            //    { }//no hit
            //}
            //var blc4 = BehaviourList4?.data_items?.Length ?? 0;
            //for (int i = 0; i < blc4; i++)
            //{
            //    var b = BehaviourList4.data_items[i];
            //    if (!bl1.Contains(b))
            //    { }//no hit
            //}
            //var blc5 = BehaviourList5?.data_items?.Length ?? 0;
            //for (int i = 0; i < blc5; i++)
            //{
            //    var b = BehaviourList5.data_items[i];
            //    if (!bl1.Contains(b))
            //    { }//no hit
            //}




            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //switch (Unknown_10h)
            //{
            //    case 4:
            //    case 2:
            //    case 3:
            //    case 6:
            //    case 7:
            //    case 5:
            //    case 10:
            //    case 21:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_14h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
            //switch (Unknown_100h)
            //{
            //    case 2:
            //    case 0:
            //    case 1:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_104h)
            //{
            //    case 0:
            //    case 1:
            //    case 7:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_108h)
            //{
            //    case 2:
            //    case 1:
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_10Ch)
            //{
            //    case 0x00010100:
            //    case 0x00010000:
            //    case 0x00010101:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_110h != 0)
            //{ }//no hit
            //if (Unknown_114h != 1.0f)
            //{ }//no hit
            //switch (Unknown_118h)
            //{
            //    case 0:
            //    case 8:
            //    case 13:
            //    case 15:
            //    case 16:
            //    case 1:
            //    case 20:
            //    case 9:
            //    case 5:
            //    case 11:
            //    case 22:
            //    case 2:
            //    case 12:
            //    case 10:
            //    case 6:
            //    case 14:
            //    case 23:
            //    case 3:
            //    case 19:
            //    case 18:
            //    case 4:
            //    case 7:
            //    case 25:
            //    case 26:
            //    case 21:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_11Ch)
            //{
            //    case 2:
            //    case 3:
            //    case 14:
            //    case 23:
            //    case 48:
            //    case 22:
            //    case 1:
            //    case 12:
            //    case 11:
            //    case 0:
            //    case 25:
            //    case 7:
            //    case 8:
            //    case 21:
            //    case 15:
            //    case 28:
            //    case 18:
            //    case 20:
            //    case 33:
            //    case 5:
            //    case 26:
            //    case 24:
            //    case 9:
            //    case 35:
            //    case 10:
            //    case 38:
            //    case 27:
            //    case 13:
            //    case 16:
            //    case 17:
            //    case 36:
            //    case 4:
            //    case 19:
            //    case 31:
            //    case 47:
            //    case 32:
            //    case 34:
            //    case 6:
            //    case 30:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_178h != 0)
            //{ }//no hit
            //if (Unknown_180h != 0)
            //{ }//no hit
            //if (Unknown_198h != 0)
            //{ }//no hit
            //if (Unknown_1A0h != 0)
            //{ }//no hit
            //if (Unknown_1A8h != 0)
            //{ }//no hit
            //switch (VFT2)
            //{
            //    case 0x40605c50:
            //    case 0x40607c70:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_1B4h != 1)
            //{ }//no hit
            //if (Unknown_1C8h != 0)
            //{ }//no hit
            //switch (Unknown_1D0h)
            //{
            //    case 5:
            //    case 2:
            //    case 8:
            //    case 6:
            //    case 13:
            //    case 16:
            //    case 20:
            //    case 3:
            //    case 12:
            //    case 1:
            //    case 14:
            //    case 27:
            //    case 21:
            //    case 9:
            //    case 4:
            //    case 19:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_1D4h != 0)
            //{ }//no hit
            //switch (VFT3)
            //{
            //    case 0x40605b48:
            //    case 0x40607b68:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_1DCh != 1)
            //{ }//no hit
            //switch (Unknown_1E0h)
            //{
            //    case 0:
            //    case 4:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_1E4h)
            //{
            //    case 0:
            //    case 1:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_1E8h)
            //{
            //    case 0x00000101:
            //    case 1:
            //    case 0x00010001:
            //    case 0x01000000:
            //    case 0x00000100:
            //    case 0x01000100:
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_1ECh)
            //{
            //    case 0:
            //    case 1:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_200h != 1)
            //{ }//no hit
            //switch (FxcFileHash) // .fxc shader file name
            //{
            //    case 0x0eb0d762: // ptfx_sprite
            //    case 0xe7b0585f: // ptfx_trail
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (FxcFileHash != JenkHash.GenHash(FxcFile?.ToString() ?? ""))
            //{ }//no hit
            //if (Unknown_20Ch != 0)
            //{ }//no hit
            //switch (Unknown_220h)
            //{
            //    case 1:
            //    case 2:
            //    case 0:
            //    case 0x00000202:
            //    case 0x00000102:
            //    case 0x00000101:
            //    case 3:
            //    case 4:
            //    case 0x00000100:
            //    case 0x00000103:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_224h != 0)
            //{ }//no hit
            //if (Unknown_228h != 0)
            //{ }//no hit
            //if (Unknown_230h != 0)
            //{ }//no hit
            //if (Unknown_238h != 0)
            //{ }//no hit
            #endregion
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.Name != null ? this.Name.FilePosition : 0);
            this.FxcFilePointer = (ulong)(this.FxcFile != null ? this.FxcFile.FilePosition : 0);
            this.FxcTechniquePointer = (ulong)(this.FxcTechnique != null ? this.FxcTechnique.FilePosition : 0);

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
            writer.Write(this.VFT2);
            writer.Write(this.Unknown_1B4h);
            writer.Write(this.FxcFilePointer);
            writer.Write(this.FxcTechniquePointer);
            writer.Write(this.Unknown_1C8h);
            writer.Write(this.Unknown_1D0h);
            writer.Write(this.Unknown_1D4h);
            writer.Write(this.VFT3);
            writer.Write(this.Unknown_1DCh);
            writer.Write(this.Unknown_1E0h);
            writer.Write(this.Unknown_1E4h);
            writer.Write(this.Unknown_1E8h);
            writer.Write(this.Unknown_1ECh);
            writer.WriteBlock(this.ShaderVars);
            writer.Write(this.Unknown_200h);
            writer.Write(this.FxcFileHash);
            writer.Write(this.Unknown_20Ch);
            writer.WriteBlock(this.Drawables);
            writer.Write(this.Unknown_220h);
            writer.Write(this.Unknown_224h);
            writer.Write(this.Unknown_228h);
            writer.Write(this.Unknown_230h);
            writer.Write(this.Unknown_238h);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            YptXml.StringTag(sb, indent, "Name", YptXml.XmlEscape(Name?.Value ?? ""));
            YptXml.StringTag(sb, indent, "FxcFile", YptXml.XmlEscape(FxcFile?.Value ?? ""));
            YptXml.StringTag(sb, indent, "FxcTechnique", YptXml.XmlEscape(FxcTechnique?.Value ?? ""));
            YptXml.ValueTag(sb, indent, "Unknown10", Unknown_10h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown100", Unknown_100h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown104", Unknown_104h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown108", Unknown_108h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown10C", YptXml.UintString(Unknown_10Ch));
            YptXml.ValueTag(sb, indent, "Unknown118", Unknown_118h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown11C", Unknown_11Ch.ToString());
            YptXml.ValueTag(sb, indent, "Unknown1D0", Unknown_1D0h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown1E0", Unknown_1E0h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown1E4", Unknown_1E4h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown1E8", YptXml.UintString(Unknown_1E8h));
            YptXml.ValueTag(sb, indent, "Unknown1EC", Unknown_1ECh.ToString());
            YptXml.ValueTag(sb, indent, "Unknown220", YptXml.UintString(Unknown_220h));
            if (Spawner1 != null)
            {
                YptXml.OpenTag(sb, indent, "Spawner1");
                Spawner1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "Spawner1");
            }
            if (Spawner2 != null)
            {
                YptXml.OpenTag(sb, indent, "Spawner2");
                Spawner2.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "Spawner2");
            }
            if (BehaviourList1?.data_items?.Length > 0)
            {
                YptXml.WriteItemArray(sb, BehaviourList1.data_items, indent, "Behaviours");
            }
            if (UnknownList1?.data_items?.Length > 0)
            {
                YptXml.WriteItemArray(sb, UnknownList1.data_items, indent, "UnknownList1");
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
            FxcFile = (string_r)Xml.GetChildInnerText(node, "FxcFile"); if (FxcFile.Value == null) FxcFile = null;
            FxcTechnique = (string_r)Xml.GetChildInnerText(node, "FxcTechnique"); if (FxcTechnique.Value == null) FxcTechnique = null;
            Unknown_10h = Xml.GetChildUIntAttribute(node, "Unknown10");
            Unknown_100h = Xml.GetChildUIntAttribute(node, "Unknown100");
            Unknown_104h = Xml.GetChildUIntAttribute(node, "Unknown104");
            Unknown_108h = Xml.GetChildUIntAttribute(node, "Unknown108");
            Unknown_10Ch = Xml.GetChildUIntAttribute(node, "Unknown10C");
            Unknown_118h = Xml.GetChildUIntAttribute(node, "Unknown118");
            Unknown_11Ch = Xml.GetChildUIntAttribute(node, "Unknown11C");
            Unknown_1D0h = Xml.GetChildUIntAttribute(node, "Unknown1D0");
            Unknown_1E0h = Xml.GetChildUIntAttribute(node, "Unknown1E0");
            Unknown_1E4h = Xml.GetChildUIntAttribute(node, "Unknown1E4");
            Unknown_1E8h = Xml.GetChildUIntAttribute(node, "Unknown1E8");
            Unknown_1ECh = Xml.GetChildUIntAttribute(node, "Unknown1EC");
            Unknown_220h = Xml.GetChildUIntAttribute(node, "Unknown220");
            Spawner1 = new ParticleEffectSpawner();
            Spawner1.ReadXml(node.SelectSingleNode("Spawner1"));
            Spawner2 = new ParticleEffectSpawner();
            Spawner2.ReadXml(node.SelectSingleNode("Spawner2"));



            var bnode = node.SelectSingleNode("Behaviours");
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




            UnknownList1 = new ResourceSimpleList64<ParticleRuleUnknownItem>();
            UnknownList1.data_items = XmlMeta.ReadItemArrayNullable<ParticleRuleUnknownItem>(node, "UnknownList1");


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

            BehaviourList1 = new ResourcePointerList64<ParticleBehaviour>();
            BehaviourList1.data_items = blist.ToArray();
            BehaviourList2 = new ResourcePointerList64<ParticleBehaviour>();
            BehaviourList2.data_items = blist2.ToArray();
            BehaviourList3 = new ResourcePointerList64<ParticleBehaviour>();
            BehaviourList3.data_items = blist3.ToArray();
            BehaviourList4 = new ResourcePointerList64<ParticleBehaviour>();
            BehaviourList4.data_items = blist4.ToArray();
            BehaviourList5 = new ResourcePointerList64<ParticleBehaviour>();
            BehaviourList5.data_items = blist5.ToArray();


        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            if (FxcFile != null) list.Add(FxcFile);
            if (FxcTechnique != null) list.Add(FxcTechnique);
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
                new Tuple<long, IResourceBlock>(0x210, Drawables)
            };
        }

        public override string ToString()
        {
            return Name?.ToString() ?? base.ToString();
        }
    }


    [TC(typeof(EXP))] public class ParticleRuleUnknownItem : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength => 0x58;




        // structure data
        public PsoChar32 Name { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong Unknown_30h; // 0x0000000000000000
        public ulong Unknown_38h; // 0x0000000000000000
        public ResourceSimpleList64_s<MetaHash> Unknown_40h { get; set; }
        public uint Unknown_50h { get; set; }
        public uint Unknown_54h; // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Name = reader.ReadStruct<PsoChar32>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt64();
            this.Unknown_38h = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();

            //if (Name.ToString() != "Bias Link Set_00")
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
            //if (Unknown_30h != 0)
            //{ }//no hit
            //if (Unknown_38h != 0)
            //{ }//no hit
            switch (Unknown_50h) // ..index?
            {
                case 0x000000f6:
                case 0x000000f7:
                case 0x000000d5:
                case 0x000000f0:
                case 0x000000f1:
                case 0x000000f2:
                case 0x000000f3:
                case 0x000000f4:
                case 0x000000ed:
                case 0x000000a6:
                case 0x000000a7:
                case 0x000000e7:
                case 0x00000081:
                case 0x00000082:
                case 0x00000083:
                case 0x000000e5:
                case 0x000000e6:
                case 0x000000e8:
                case 0x000000e9:
                case 0x000000ea:
                    break;
                default:
                    break;//more
            }
            //if (Unknown_54h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteStruct(this.Name);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_38h);
            writer.WriteBlock(this.Unknown_40h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", YptXml.XmlEscape(Name.ToString()));
            YptXml.ValueTag(sb, indent, "Unknown50", Unknown_50h.ToString());
            YptXml.WriteHashItemArray(sb, Unknown_40h?.data_items, indent, "Unknown40");
        }
        public void ReadXml(XmlNode node)
        {
            Name = new PsoChar32(Xml.GetChildInnerText(node, "Name"));
            Unknown_50h = Xml.GetChildUIntAttribute(node, "Unknown50");
            Unknown_40h = new ResourceSimpleList64_s<MetaHash>();
            Unknown_40h.data_items = XmlMeta.ReadHashItemArray(node, "Unknown40");
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x40, Unknown_40h)
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
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public float Unknown_18h { get; set; } // 0, 0.1f, 1.0f
        public float Unknown_1Ch { get; set; } // 0, 0.8f, 1.0f, 1.1f, ...
        public uint Unknown_20h { get; set; } // eg. 0xff736626 - colour?
        public float Unknown_24h { get; set; } // 1.0f, 7.0f, 100.0f, ...
        public uint Unknown_28h { get; set; }  // 0, 4, 8, 9, 10, 11, 12, 14     //index/id
        public uint Unknown_2Ch; // 0x00000000
        public ulong Unknown_30h; // 0x0000000000000000
        public float Unknown_38h { get; set; } // 0, 0.1f, 0.3f, 1.0f
        public float Unknown_3Ch { get; set; } // 0, 1.0f, 1.1f, 1.2f, 1.4f, 1.5f
        public uint Unknown_40h { get; set; } // eg. 0xffffffff, 0xffffeca8  - colour?
        public float Unknown_44h { get; set; } // 0, 0.4f, 1.0f, 100.0f, ....
        public uint Unknown_48h { get; set; } // 0, 4, 8, 9, 10, 11, 12, 14     //index/id
        public uint Unknown_4Ch; // 0x00000000
        public ulong Unknown_50h; // 0x0000000000000000
        public ulong EffectRulePointer { get; set; }
        public ulong EffectRuleNamePointer { get; set; }
        public float Unknown_68h { get; set; } // 0, 0.5f, 1.0f
        public uint Unknown_6Ch { get; set; } // eg. 0x01010100

        // reference data
        public ParticleEffectRule EffectRule { get; set; }
        public string_r EffectRuleName { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadSingle();
            this.Unknown_1Ch = reader.ReadSingle();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadSingle();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt64();
            this.Unknown_38h = reader.ReadSingle();
            this.Unknown_3Ch = reader.ReadSingle();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadSingle();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadUInt64();
            this.EffectRulePointer = reader.ReadUInt64();
            this.EffectRuleNamePointer = reader.ReadUInt64();
            this.Unknown_68h = reader.ReadSingle();
            this.Unknown_6Ch = reader.ReadUInt32();

            // read reference data
            this.EffectRule = reader.ReadBlockAt<ParticleEffectRule>(this.EffectRulePointer);
            this.EffectRuleName = reader.ReadBlockAt<string_r>(this.EffectRuleNamePointer);

            //if (EffectRuleName?.Value != (EffectRule?.Name?.Value ?? ""))
            //{ }//no hit

            //if (Unknown_4h != 1)
            //{ }
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //switch (Unknown_18h)
            //{
            //    case 0:
            //    case 1.0f:
            //    case 0.1f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            switch (Unknown_1Ch)
            {
                case 0:
                case 1.0f:
                case 1.1f:
                case 0.8f:
                case 0.9f:
                case 1.5f:
                    break;
                default:
                    break;//more
            }
            //switch (Unknown_20h)
            //{
            //    case 0:
            //    case 0xffffffff:
            //    case 0x00ffffff:
            //    case 0xff736626:
            //    case 0xff404040:
            //    case 0xfffaf7c8:
            //    case 0xfffc42f9:
            //    case 0xff4f3535:
            //    case 0xff321a1a:
            //    case 0xffffd591:
            //        break;
            //    default:
            //        break;//no hit
            //}
            switch (Unknown_24h)
            {
                case 0:
                case 100.0f:
                case 0.6f:
                case 1.0f:
                case 0.3f:
                case 1.2f:
                case 7.0f:
                    break;
                default:
                    break;//more
            }
            //switch (Unknown_28h)
            //{
            //    case 0:
            //    case 8:
            //    case 11:
            //    case 9:
            //    case 12:
            //    case 10:
            //    case 14:
            //    case 4:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_2Ch != 0)
            //{ }//no hit
            //if (Unknown_30h != 0)
            //{ }//no hit
            //switch (Unknown_38h)
            //{
            //    case 0:
            //    case 1.0f:
            //    case 0.1f:
            //    case 0.3f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_3Ch)
            //{
            //    case 0:
            //    case 1.0f:
            //    case 1.1f:
            //    case 1.2f:
            //    case 1.4f:
            //    case 1.5f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_40h)
            //{
            //    case 0:
            //    case 0xffffffff:
            //    case 0xffffeca8:
            //    case 0xff8c7d2e:
            //    case 0xffd1d1d1:
            //    case 0xfff0dfb6:
            //    case 0xffcc16b4:
            //    case 0xff4c3434:
            //    case 0xff24341a:
            //    case 0xfffff1bd:
            //        break;
            //    default:
            //        break;//no hit
            //}
            switch (Unknown_44h)
            {
                case 0:
                case 100.0f:
                case 0.8f:
                case 1.0f:
                case 0.4f:
                case 1.8f:
                    break;
                default:
                    break;//more
            }
            //switch (Unknown_48h)
            //{
            //    case 0:
            //    case 8:
            //    case 11:
            //    case 9:
            //    case 12:
            //    case 10:
            //    case 14:
            //    case 4:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_4Ch != 0)
            //{ }//no hit
            //if (Unknown_50h != 0)
            //{ }//no hit
            //switch (Unknown_68h)
            //{
            //    case 0:
            //    case 1.0f:
            //    case 0.5f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_6Ch)
            //{
            //    case 0:
            //    case 1:
            //    case 0x00010000:
            //    case 0x00000100:
            //    case 0x00010101:
            //    case 0x01010100:
            //    case 0x00010100:
            //    case 0x01010101:
            //        break;
            //    default:
            //        break;//no hit
            //}

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.EffectRulePointer = (ulong)(this.EffectRule != null ? this.EffectRule.FilePosition : 0);
            this.EffectRuleNamePointer = (ulong)(this.EffectRuleName != null ? this.EffectRuleName.FilePosition : 0);

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
            writer.Write(this.EffectRulePointer);
            writer.Write(this.EffectRuleNamePointer);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            //YptXml.StringTag(sb, indent, "EffectRuleName", YptXml.XmlEscape(EffectRuleName?.Value ?? ""));
            YptXml.StringTag(sb, indent, "EffectRule", EffectRule?.Name?.Value ?? "");
            YptXml.ValueTag(sb, indent, "Unknown18", FloatUtil.ToString(Unknown_18h));
            YptXml.ValueTag(sb, indent, "Unknown1C", FloatUtil.ToString(Unknown_1Ch));
            YptXml.ValueTag(sb, indent, "Unknown20", YptXml.UintString(Unknown_20h));
            YptXml.ValueTag(sb, indent, "Unknown24", FloatUtil.ToString(Unknown_24h));
            YptXml.ValueTag(sb, indent, "Unknown28", Unknown_28h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown38", FloatUtil.ToString(Unknown_38h));
            YptXml.ValueTag(sb, indent, "Unknown3C", FloatUtil.ToString(Unknown_3Ch));
            YptXml.ValueTag(sb, indent, "Unknown40", YptXml.UintString(Unknown_40h));
            YptXml.ValueTag(sb, indent, "Unknown44", FloatUtil.ToString(Unknown_44h));
            YptXml.ValueTag(sb, indent, "Unknown48", Unknown_48h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown68", FloatUtil.ToString(Unknown_68h));
            YptXml.ValueTag(sb, indent, "Unknown6C", YptXml.UintString(Unknown_6Ch));
        }
        public void ReadXml(XmlNode node)
        {
            //EffectRuleName = (string_r)Xml.GetChildInnerText(node, "EffectRuleName"); if (EffectRuleName.Value == null) EffectRuleName = null;
            var ername = Xml.GetChildInnerText(node, "EffectRule");
            EffectRuleName = (string_r)(ername ?? "");
            Unknown_18h = Xml.GetChildFloatAttribute(node, "Unknown18");
            Unknown_1Ch = Xml.GetChildFloatAttribute(node, "Unknown1C");
            Unknown_20h = Xml.GetChildUIntAttribute(node, "Unknown20");
            Unknown_24h = Xml.GetChildFloatAttribute(node, "Unknown24");
            Unknown_28h = Xml.GetChildUIntAttribute(node, "Unknown28");
            Unknown_38h = Xml.GetChildFloatAttribute(node, "Unknown38");
            Unknown_3Ch = Xml.GetChildFloatAttribute(node, "Unknown3C");
            Unknown_40h = Xml.GetChildUIntAttribute(node, "Unknown40");
            Unknown_44h = Xml.GetChildFloatAttribute(node, "Unknown44");
            Unknown_48h = Xml.GetChildUIntAttribute(node, "Unknown48");
            Unknown_68h = Xml.GetChildFloatAttribute(node, "Unknown68");
            Unknown_6Ch = Xml.GetChildUIntAttribute(node, "Unknown6C");
            if (!string.IsNullOrEmpty(ername))
            { }
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
        public float Unknown_0h { get; set; }
        public float Unknown_4h { get; set; }
        public float Unknown_8h { get; set; }
        public float Unknown_Ch { get; set; }
        public ulong NamePointer { get; set; }
        public ulong DrawablePointer { get; set; }
        public MetaHash NameHash { get; set; }
        public uint Unknown_24h { get; set; } // 0x00000000
        public ulong Unknown_28h; // 0x0000000000000000

        // reference data
        public string_r Name { get; set; }
        public DrawablePtfx Drawable { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadSingle();
            this.Unknown_4h = reader.ReadSingle();
            this.Unknown_8h = reader.ReadSingle();
            this.Unknown_Ch = reader.ReadSingle();
            this.NamePointer = reader.ReadUInt64();
            this.DrawablePointer = reader.ReadUInt64();
            this.NameHash = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt64();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(this.NamePointer);
            this.Drawable = reader.ReadBlockAt<DrawablePtfx>(this.DrawablePointer);

            if (!string.IsNullOrEmpty(Name?.Value))
            {
                JenkIndex.Ensure(Name.Value);
            }

            switch (Unknown_0h)
            {
                case 0.355044f:
                case 1.0f:
                case 0.308508f:
                    break;
                default:
                    break;//more
            }
            switch (Unknown_4h)
            {
                case 0.894308f:
                case 1.0f:
                case 0.127314f:
                    break;
                default:
                    break;//more
            }
            switch (Unknown_8h)
            {
                case 0.894308f:
                case 1.0f:
                case 0.127314f:
                    break;
                default:
                    break;//more
            }
            switch (Unknown_Ch)
            {
                case 0.4f:
                case 0.5f:
                case 0.178602f:
                    break;
                default:
                    break;//more
            }
            if (NameHash != JenkHash.GenHash(Name?.Value ?? ""))
            { }//no hit
            //if (Unknown_24h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.Name != null ? this.Name.FilePosition : 0);
            this.DrawablePointer = (ulong)(this.Drawable != null ? this.Drawable.FilePosition : 0);

            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.NamePointer);
            writer.Write(this.DrawablePointer);
            writer.Write(this.NameHash);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", YptXml.XmlEscape(Name?.Value ?? ""));
            YptXml.ValueTag(sb, indent, "Unknown0", FloatUtil.ToString(Unknown_0h));
            YptXml.ValueTag(sb, indent, "Unknown4", FloatUtil.ToString(Unknown_4h));
            YptXml.ValueTag(sb, indent, "Unknown8", FloatUtil.ToString(Unknown_8h));
            YptXml.ValueTag(sb, indent, "UnknownC", FloatUtil.ToString(Unknown_Ch));
            if (Drawable != null)
            {
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name = (string_r)Xml.GetChildInnerText(node, "Name"); if (Name.Value == null) Name = null;
            NameHash = JenkHash.GenHash(Name?.Value ?? "");
            Unknown_0h = Xml.GetChildFloatAttribute(node, "Unknown0");
            Unknown_4h = Xml.GetChildFloatAttribute(node, "Unknown4");
            Unknown_8h = Xml.GetChildFloatAttribute(node, "Unknown8");
            Unknown_Ch = Xml.GetChildFloatAttribute(node, "UnknownC");
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
        // pgBase
        // pgBaseRefCounted
        // ptxEffectRule
        public override long BlockLength => 0x3C0;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h = 1; // 0x0000000000000001
        public float Unknown_18h { get; set; } = 4.2f;
        public uint Unknown_1Ch; // 0x00000000
        public ulong NamePointer { get; set; }
        public ulong Unknown_28h { get; set; } = 0x0000000050000000; // 0x50000000 -> ".?AVptxFxList@rage@@" pointer to itself
        public uint VFT2 { get; set; } = 0x4060e3e8; // 0x4060e3e8, 0x40610408
        public uint Unknown_34h = 1; // 0x00000001
        public ulong EventEmittersPointer { get; set; }
        public ushort EventEmittersCount { get; set; }
        public ushort EventEmittersCapacity { get; set; } = 32; //always 32
        public uint Unknown_44h; // 0x00000000
        public ulong UnknownData1Pointer { get; set; }
        public uint Unknown_50h { get; set; } // 0, 0xffffffff
        public uint Unknown_54h { get; set; } // eg. 0x01010200
        public ulong Unknown_58h; // 0x0000000000000000
        public ulong Unknown_60h; // 0x0000000000000000
        public uint Unknown_68h; // 0x00000000
        public uint Unknown_6Ch { get; set; } = 0x7f800001; // 0x7f800001
        public float Unknown_70h { get; set; }
        public float Unknown_74h { get; set; } // 0, 0.1f, 0.25f, 1.0f
        public float PlaybackDelay { get; set; }
        public float PlaybackDelayModifier { get; set; }
        public float PlaybackSpeedScale { get; set; }
        public float PlaybackSpeedScaleModifier { get; set; }
        public uint Unknown_88h { get; set; } // eg. 0x01010105
        public uint Unknown_8Ch { get; set; } // eg. 0x01010002
        public float CullRadius { get; set; }
        public float CullDistance { get; set; }
        public float Unknown_98h { get; set; }
        public uint Unknown_9Ch { get; set; } = 0x7f800001;// 0x7f800001
        public float Unknown_A0h { get; set; }
        public float Unknown_A4h { get; set; }
        public float Unknown_A8h { get; set; }
        public float Unknown_ACh { get; set; }
        public float Unknown_B0h { get; set; }
        public float Unknown_B4h { get; set; }
        public float Unknown_B8h { get; set; }
        public uint Unknown_BCh { get; set; } // eg. 0x00010103
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public ParticleKeyframeProp KeyframeProp4 { get; set; }
        public ulong KeyframePropsPointer { get; set; } //pointer to a list, which is pointing back to above items
        public ushort KeyframePropsCount { get; set; } = 5; //always 5
        public ushort KeyframePropsCapacity { get; set; } = 16; //always 16
        public uint Unknown_39Ch; // 0x00000000
        public uint Unknown_3A0h { get; set; } // eg. 0x00090100
        public uint Unknown_3A4h; // 0x00000000
        public float Unknown_3A8h { get; set; } = 100.0f;
        public uint Unknown_3ACh { get; set; } // 0x00000000
        public ulong Unknown_3B0h { get; set; } // 0x0000000000000000
        public ulong Unknown_3B8h { get; set; } // 0x0000000000000000

        // reference data
        public string_r Name { get; set; }
        public MetaHash NameHash { get; set; }
        public ResourcePointerArray64<ParticleEventEmitter> EventEmitters { get; set; }
        public ParticleUnknown1 UnknownData { get; set; }
        public ResourcePointerArray64<ParticleKeyframeProp> KeyframeProps { get; set; } // these just point to the 5x embedded KeyframeProps, padded to 16 items


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            #region read

            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadSingle();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.VFT2 = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.EventEmittersPointer = reader.ReadUInt64();
            this.EventEmittersCount = reader.ReadUInt16();
            this.EventEmittersCapacity = reader.ReadUInt16();
            this.Unknown_44h = reader.ReadUInt32();
            this.UnknownData1Pointer = reader.ReadUInt64();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.Unknown_58h = reader.ReadUInt64();
            this.Unknown_60h = reader.ReadUInt64();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Unknown_70h = reader.ReadSingle();
            this.Unknown_74h = reader.ReadSingle();
            this.PlaybackDelay = reader.ReadSingle();
            this.PlaybackDelayModifier = reader.ReadSingle();
            this.PlaybackSpeedScale = reader.ReadSingle();
            this.PlaybackSpeedScaleModifier = reader.ReadSingle();
            this.Unknown_88h = reader.ReadUInt32();
            this.Unknown_8Ch = reader.ReadUInt32();
            this.CullRadius = reader.ReadSingle();
            this.CullDistance = reader.ReadSingle();
            this.Unknown_98h = reader.ReadSingle();
            this.Unknown_9Ch = reader.ReadUInt32();
            this.Unknown_A0h = reader.ReadSingle();
            this.Unknown_A4h = reader.ReadSingle();
            this.Unknown_A8h = reader.ReadSingle();
            this.Unknown_ACh = reader.ReadSingle();
            this.Unknown_B0h = reader.ReadSingle();
            this.Unknown_B4h = reader.ReadSingle();
            this.Unknown_B8h = reader.ReadSingle();
            this.Unknown_BCh = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp4 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframePropsPointer = reader.ReadUInt64();
            this.KeyframePropsCount = reader.ReadUInt16();
            this.KeyframePropsCapacity = reader.ReadUInt16();
            this.Unknown_39Ch = reader.ReadUInt32();
            this.Unknown_3A0h = reader.ReadUInt32();
            this.Unknown_3A4h = reader.ReadUInt32();
            this.Unknown_3A8h = reader.ReadSingle();
            this.Unknown_3ACh = reader.ReadUInt32();
            this.Unknown_3B0h = reader.ReadUInt64();
            this.Unknown_3B8h = reader.ReadUInt64();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(this.NamePointer);
            this.EventEmitters = reader.ReadBlockAt<ResourcePointerArray64<ParticleEventEmitter>>(this.EventEmittersPointer, this.EventEmittersCapacity);
            this.UnknownData = reader.ReadBlockAt<ParticleUnknown1>(this.UnknownData1Pointer);
            this.KeyframeProps = reader.ReadBlockAt<ResourcePointerArray64<ParticleKeyframeProp>>(this.KeyframePropsPointer, this.KeyframePropsCapacity);

            if (!string.IsNullOrEmpty(Name?.Value))
            {
                JenkIndex.Ensure(Name.Value);
                NameHash = JenkHash.GenHash(Name.Value);
            }

            #endregion


            #region testing

            //for (int i = 0; i < (EventEmitters?.data_items?.Length??0); i++)
            //{
            //    if (EventEmitters.data_items[i].Index != i)
            //    { }//no hit
            //}

            //if (EventEmittersCount2 != 32)
            //{ }//no hit
            //if (KeyframePropsCount2 != 16)
            //{ }//no hit
            //if (KeyframePropsCount1 != 5)
            //{ }//no hit

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_10h != 1)
            //{ }//no hit
            //if (Unknown_18h != 4.2f)
            //{ }//no hit
            //if (Unknown_1Ch != 0)
            //{ }//no hit
            //switch (Unknown_28h)
            //{
            //    case 0x0000000050000000:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (VFT2) //some VFT
            //{
            //    case 0x4060e3e8: 
            //    case 0x40610408:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_34h != 1)
            //{ }//no hit
            //if (Unknown_44h != 0)
            //{ }//no hit
            //switch (Unknown_50h)
            //{
            //    case 0xffffffff:
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_54h)
            //{
            //    case 0x01000000:
            //    case 0x01010001:
            //    case 0x01010200:
            //    case 0x01010000:
            //    case 0x01000200:
            //    case 0x01000001:
            //    case 0x01000201:
            //    case 0x01000100:
            //        break;
            //    default:
            //        break;//more
            //}
            //if (Unknown_58h != 0)
            //{ }//no hit
            //if (Unknown_60h != 0)
            //{ }//no hit
            //if ((Unknown_68h != 0) && (Unknown_68h != 0x80000000))//float?
            //{ }//no hit
            //if (Unknown_6Ch != 0x7f800001)
            //{ }//no hit
            //switch (Unknown_70h)
            //{
            //    case 0:
            //    case 1.0f:
            //    case 0.5f:
            //    case 0.2f:
            //    case 0.1f:
            //        break;
            //    default:
            //        break;//more
            //}
            //switch (Unknown_74h)
            //{
            //    case 0.25f:
            //    case 0:
            //    case 1.0f:
            //    case 0.1f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_78h)
            //{
            //    case 0.2f:
            //    case 0.5f:
            //    case 1.0f:
            //        break;
            //    default:
            //        break;//more
            //}
            //switch (Unknown_7Ch)
            //{
            //    case 0.2f:
            //    case 0.5f:
            //    case 1.0f:
            //        break;
            //    default:
            //        break;//more
            //}
            //switch (Unknown_80h)
            //{
            //    case 1.0f:
            //    case 2.0f:
            //    case 1.2f:
            //    case 1.5f:
            //        break;
            //    default:
            //        break;//more
            //}
            //switch (Unknown_84h)
            //{
            //    case 1.0f:
            //    case 2.0f:
            //        break;
            //    default:
            //        break;//more
            //}
            //switch (Unknown_88h)
            //{
            //    case 0x01010100:
            //    case 0x01010101:
            //    case 0x00010004:
            //    case 0x01010002:
            //    case 0x00000003:
            //    case 0x01010105:
            //    case 0x00010105:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_8Ch)
            //{
            //    case 0x00010004:
            //    case 0x01010101:
            //    case 0x01010100:
            //    case 0x01010002:
            //    case 0x00000003:
            //    case 0x00010105:
            //    case 0x00000005:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_90h)
            //{
            //    case 0:
            //    case 1.1f:
            //    case 1.5f:
            //    case 1.2f:
            //    case 6.0f:
            //        break;
            //    default:
            //        break;//more
            //}
            //switch (Unknown_94h)
            //{
            //    case 0:
            //    case 1.8f:
            //    case 10.0f:
            //    case 0.4f:
            //    case -1.0f:
            //    case -9.0f:
            //        break;
            //    default:
            //        break;//more
            //}
            //switch (Unknown_98h)
            //{
            //    case 0:
            //    case 5.0f:
            //    case 1.5f:
            //    case -1.0f:
            //    case 0.5f:
            //    case 0.2f:
            //    case 1.0f:
            //    case 12.0f:
            //        break;
            //    default:
            //        break;//more
            //}
            //if (Unknown_9Ch != 0x7f800001)
            //{ }//no hit
            //switch (Unknown_A0h)
            //{
            //    case 0:
            //    case 4.5f:
            //    case 11.0f:
            //    case 5.0f:
            //        break;
            //    default:
            //        break;//and more
            //}
            //switch (Unknown_A4h)
            //{
            //    case 38.0f:
            //    case 25.0f:
            //        break;
            //    default:
            //        break;//and more
            //}
            //switch (Unknown_A8h)
            //{
            //    case 40.0f:
            //    case 30.0f:
            //        break;
            //    default:
            //        break;//and more
            //}
            //switch (Unknown_ACh)
            //{
            //    case 15.0f:
            //    case 4.0f:
            //        break;
            //    default:
            //        break;//and more
            //}
            //switch (Unknown_B0h)
            //{
            //    case 40.0f:
            //    case 12.0f:
            //        break;
            //    default:
            //        break;//and more
            //}
            //switch (Unknown_B4h)
            //{
            //    case 3.0f:
            //    case 0:
            //    case 0.500002f:
            //    case 1.5f:
            //        break;
            //    default:
            //        break;//more
            //}
            //switch (Unknown_B8h)
            //{
            //    case 2.0f:
            //    case 0:
            //    case 1.5f:
            //    case 1.0f:
            //    case 3.0f:
            //    case 5.0f:
            //    case 9.0f:
            //        break;
            //    default:
            //        break;//more
            //}
            //switch (Unknown_BCh)
            //{
            //    case 0x00010103:
            //    case 0:
            //    case 0x01000000:
            //    case 0x01010003:
            //    case 0x00000103:
            //    case 0x00000002:
            //    case 0x00000003:
            //    case 0x00010100:
            //    case 0x01000002:
            //    case 0x00010002:
            //    case 0x01010002:
            //        break;
            //    default:
            //        break;//more
            //}
            //if (Unknown_39Ch != 0)
            //{ }//no hit
            //switch (Unknown_3A0h)
            //{
            //    case 0:
            //    case 1:
            //    case 0x00000100:
            //    case 0x00010100:
            //    case 0x00020100:
            //    case 0x00080000:
            //    case 0x00090100:
            //    case 0x000b0100:
            //    case 0x000c0100: //setting the 5th digit to C (eg 0x000C0000) for Unknown3A0 in EffectRuleDictionary enables damage for volumetric particles -Monika
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_3A4h != 0)
            //{ }//no hit
            //if (Unknown_3A8h != 100.0f)
            //{ }//no hit
            //if (Unknown_3ACh != 0)
            //{ }//no hit
            //if (Unknown_3B0h != 0)
            //{ }//no hit
            //if (Unknown_3B8h != 0)
            //{ }//no hit

            #endregion

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.Name != null ? this.Name.FilePosition : 0);
            this.EventEmittersPointer = (ulong)(this.EventEmitters != null ? this.EventEmitters.FilePosition : 0);
            this.UnknownData1Pointer = (ulong)(this.UnknownData != null ? this.UnknownData.FilePosition : 0);
            this.KeyframePropsPointer = (ulong)(this.KeyframeProps != null ? this.KeyframeProps.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_28h);
            writer.Write(this.VFT2);
            writer.Write(this.Unknown_34h);
            writer.Write(this.EventEmittersPointer);
            writer.Write(this.EventEmittersCount);
            writer.Write(this.EventEmittersCapacity);
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
            writer.Write(this.PlaybackDelay);
            writer.Write(this.PlaybackDelayModifier);
            writer.Write(this.PlaybackSpeedScale);
            writer.Write(this.PlaybackSpeedScaleModifier);
            writer.Write(this.Unknown_88h);
            writer.Write(this.Unknown_8Ch);
            writer.Write(this.CullRadius);
            writer.Write(this.CullDistance);
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
            writer.Write(this.KeyframePropsCount);
            writer.Write(this.KeyframePropsCapacity);
            writer.Write(this.Unknown_39Ch);
            writer.Write(this.Unknown_3A0h);
            writer.Write(this.Unknown_3A4h);
            writer.Write(this.Unknown_3A8h);
            writer.Write(this.Unknown_3ACh);
            writer.Write(this.Unknown_3B0h);
            writer.Write(this.Unknown_3B8h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", YptXml.XmlEscape(Name?.Value ?? ""));
            YptXml.ValueTag(sb, indent, "Unknown50", YptXml.UintString(Unknown_50h));
            YptXml.ValueTag(sb, indent, "Unknown54", YptXml.UintString(Unknown_54h));
            YptXml.ValueTag(sb, indent, "Unknown70", FloatUtil.ToString(Unknown_70h));
            YptXml.ValueTag(sb, indent, "Unknown74", FloatUtil.ToString(Unknown_74h));
            YptXml.ValueTag(sb, indent, "PlaybackDelay", FloatUtil.ToString(PlaybackDelay));
            YptXml.ValueTag(sb, indent, "PlaybackDelayModifier", FloatUtil.ToString(PlaybackDelayModifier));
            YptXml.ValueTag(sb, indent, "PlaybackSpeedScale", FloatUtil.ToString(PlaybackSpeedScale));
            YptXml.ValueTag(sb, indent, "PlaybackSpeedScaleModifier", FloatUtil.ToString(PlaybackSpeedScaleModifier));
            YptXml.ValueTag(sb, indent, "Unknown88", YptXml.UintString(Unknown_88h));
            YptXml.ValueTag(sb, indent, "Unknown8C", YptXml.UintString(Unknown_8Ch));
            YptXml.ValueTag(sb, indent, "CullRadius", FloatUtil.ToString(CullRadius));
            YptXml.ValueTag(sb, indent, "CullDistance", FloatUtil.ToString(CullDistance));
            YptXml.ValueTag(sb, indent, "Unknown98", FloatUtil.ToString(Unknown_98h));
            YptXml.ValueTag(sb, indent, "UnknownA0", FloatUtil.ToString(Unknown_A0h));
            YptXml.ValueTag(sb, indent, "UnknownA4", FloatUtil.ToString(Unknown_A4h));
            YptXml.ValueTag(sb, indent, "UnknownA8", FloatUtil.ToString(Unknown_A8h));
            YptXml.ValueTag(sb, indent, "UnknownAC", FloatUtil.ToString(Unknown_ACh));
            YptXml.ValueTag(sb, indent, "UnknownB0", FloatUtil.ToString(Unknown_B0h));
            YptXml.ValueTag(sb, indent, "UnknownB4", FloatUtil.ToString(Unknown_B4h));
            YptXml.ValueTag(sb, indent, "UnknownB8", FloatUtil.ToString(Unknown_B8h));
            YptXml.ValueTag(sb, indent, "UnknownBC", YptXml.UintString(Unknown_BCh));
            YptXml.ValueTag(sb, indent, "Unknown3A0", YptXml.UintString(Unknown_3A0h));
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
                YptXml.WriteItemArray(sb, kp, indent, "KeyframeProperties");
            }
            if (UnknownData != null)
            {
                YptXml.OpenTag(sb, indent, "UnknownData");
                UnknownData.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "UnknownData");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name = (string_r)Xml.GetChildInnerText(node, "Name"); if (Name.Value == null) Name = null;
            NameHash = JenkHash.GenHash(Name?.Value ?? "");
            Unknown_50h = Xml.GetChildUIntAttribute(node, "Unknown50");
            Unknown_54h = Xml.GetChildUIntAttribute(node, "Unknown54");
            Unknown_70h = Xml.GetChildFloatAttribute(node, "Unknown70");
            Unknown_74h = Xml.GetChildFloatAttribute(node, "Unknown74");
            PlaybackDelay = Xml.GetChildFloatAttribute(node, "PlaybackDelay");
            PlaybackDelayModifier = Xml.GetChildFloatAttribute(node, "PlaybackDelayModifier");
            PlaybackSpeedScale = Xml.GetChildFloatAttribute(node, "PlaybackSpeedScale");
            PlaybackSpeedScaleModifier = Xml.GetChildFloatAttribute(node, "PlaybackSpeedScaleModifier");
            Unknown_88h = Xml.GetChildUIntAttribute(node, "Unknown88");
            Unknown_8Ch = Xml.GetChildUIntAttribute(node, "Unknown8C");
            CullRadius = Xml.GetChildFloatAttribute(node, "CullRadius");
            CullDistance = Xml.GetChildFloatAttribute(node, "CullDistance");
            Unknown_98h = Xml.GetChildFloatAttribute(node, "Unknown98");
            Unknown_A0h = Xml.GetChildFloatAttribute(node, "UnknownA0");
            Unknown_A4h = Xml.GetChildFloatAttribute(node, "UnknownA4");
            Unknown_A8h = Xml.GetChildFloatAttribute(node, "UnknownA8");
            Unknown_ACh = Xml.GetChildFloatAttribute(node, "UnknownAC");
            Unknown_B0h = Xml.GetChildFloatAttribute(node, "UnknownB0");
            Unknown_B4h = Xml.GetChildFloatAttribute(node, "UnknownB4");
            Unknown_B8h = Xml.GetChildFloatAttribute(node, "UnknownB8");
            Unknown_BCh = Xml.GetChildUIntAttribute(node, "UnknownBC");
            Unknown_3A0h = Xml.GetChildUIntAttribute(node, "Unknown3A0");

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


            var kflist = XmlMeta.ReadItemArray<ParticleKeyframeProp>(node, "KeyframeProperties")?.ToList() ?? new List<ParticleKeyframeProp>();
            KeyframeProp0 = (kflist.Count > 0) ? kflist[0] : new ParticleKeyframeProp();
            KeyframeProp1 = (kflist.Count > 1) ? kflist[1] : new ParticleKeyframeProp();
            KeyframeProp2 = (kflist.Count > 2) ? kflist[2] : new ParticleKeyframeProp();
            KeyframeProp3 = (kflist.Count > 3) ? kflist[3] : new ParticleKeyframeProp();
            KeyframeProp4 = (kflist.Count > 4) ? kflist[4] : new ParticleKeyframeProp();
            for (int i = kflist.Count; i < 16; i++) kflist.Add(null);
            KeyframeProps = new ResourcePointerArray64<ParticleKeyframeProp>();
            KeyframeProps.data_items = kflist.ToArray();
            KeyframeProps.ManualReferenceOverride = true;
            KeyframePropsCount = 5;//this should always be 5.......
            KeyframePropsCapacity = 16;//should always be 16...

            var udnode = node.SelectSingleNode("UnknownData");
            if (udnode != null)
            {
                UnknownData = new ParticleUnknown1();
                UnknownData.ReadXml(udnode);
            }

        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            if (EventEmitters != null) list.Add(EventEmitters);
            if (UnknownData != null) list.Add(UnknownData);
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
                new Tuple<long, IResourceBlock>(192, KeyframeProp0),
                new Tuple<long, IResourceBlock>(336, KeyframeProp1),
                new Tuple<long, IResourceBlock>(480, KeyframeProp2),
                new Tuple<long, IResourceBlock>(624, KeyframeProp3),
                new Tuple<long, IResourceBlock>(768, KeyframeProp4)
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
        public uint Unknown_4h = 1; // 0x00000001
        public uint Index { get; set; } // 0, 1, 2, 3, 4, 5, 6  -index?
        public uint Unknown_Ch; // 0x00000000
        public float Unknown_10h { get; set; }
        public float Unknown_14h { get; set; }
        public ulong UnknownDataPointer { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong EmitterRuleNamePointer { get; set; }
        public ulong ParticleRuleNamePointer { get; set; }
        public ulong EmitterRulePointer { get; set; }
        public ulong ParticleRulePointer { get; set; }
        public float MoveSpeedScale { get; set; }
        public float MoveSpeedScaleModifier { get; set; }
        public float ParticleScale { get; set; }
        public float ParticleScaleModifier { get; set; }
        public uint Colour1 { get; set; } // eg. 0xfffafafa - colour?
        public uint Colour2 { get; set; } // eg. 0x5affffff - colour?
        public ulong Unknown_68h; // 0x0000000000000000

        // reference data
        public ParticleUnknown1 UnknownData { get; set; }
        public string_r EmitterRuleName { get; set; }
        public string_r ParticleRuleName { get; set; }
        public ParticleEmitterRule EmitterRule { get; set; }
        public ParticleRule ParticleRule { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Index = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadSingle();
            this.Unknown_14h = reader.ReadSingle();
            this.UnknownDataPointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.EmitterRuleNamePointer = reader.ReadUInt64();
            this.ParticleRuleNamePointer = reader.ReadUInt64();
            this.EmitterRulePointer = reader.ReadUInt64();
            this.ParticleRulePointer = reader.ReadUInt64();
            this.MoveSpeedScale = reader.ReadSingle();
            this.MoveSpeedScaleModifier = reader.ReadSingle();
            this.ParticleScale = reader.ReadSingle();
            this.ParticleScaleModifier = reader.ReadSingle();
            this.Colour1 = reader.ReadUInt32();
            this.Colour2 = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt64();

            // read reference data
            this.UnknownData = reader.ReadBlockAt<ParticleUnknown1>(this.UnknownDataPointer);
            this.EmitterRuleName = reader.ReadBlockAt<string_r>(this.EmitterRuleNamePointer);
            this.ParticleRuleName = reader.ReadBlockAt<string_r>(this.ParticleRuleNamePointer);
            this.EmitterRule = reader.ReadBlockAt<ParticleEmitterRule>(this.EmitterRulePointer);
            this.ParticleRule = reader.ReadBlockAt<ParticleRule>(this.ParticleRulePointer);


            if (!string.IsNullOrEmpty(EmitterRuleName?.Value))
            {
                JenkIndex.Ensure(EmitterRuleName.Value);
            }
            if (!string.IsNullOrEmpty(ParticleRuleName?.Value))
            {
                JenkIndex.Ensure(ParticleRuleName.Value);
            }

            if (EmitterRuleName?.Value != EmitterRule?.Name?.Value)
            { }//no hit
            if (ParticleRuleName?.Value != ParticleRule?.Name?.Value)
            { }//no hit

            //if (Unknown_4h != 1)
            //{ }//no hit
            //switch (Unknown_8h)
            //{
            //    case 0:
            //    case 1:
            //    case 2:
            //    case 3:
            //    case 4:
            //    case 5:
            //    case 6:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_Ch != 0)
            //{ }//no hit
            switch (Unknown_10h)
            {
                case 0:
                case 0.015f:
                case 0.1f:
                case 0.3f:
                case 0.8f:
                    break;
                default:
                    break;//more
            }
            switch (Unknown_14h)
            {
                case 1.0f:
                case 0.15f:
                case 0.01f:
                case 0.1f:
                case 0.3f:
                    break;
                default:
                    break;//more
            }
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
            switch (MoveSpeedScale)
            {
                case 1.0f:
                case 2.0f:
                case 1.2f:
                case 0.8f:
                    break;
                default:
                    break;//more
            }
            switch (MoveSpeedScaleModifier)
            {
                case 1.0f:
                case 2.0f:
                case 1.2f:
                case 0.8f:
                    break;
                default:
                    break;//and more
            }
            switch (ParticleScale)
            {
                case 1.0f:
                case 0.5f:
                case 0.95f:
                case 1.2f:
                case 0.4f:
                    break;
                default:
                    break;//more
            }
            switch (ParticleScaleModifier)
            {
                case 1.0f:
                case 1.2f:
                case 0.5f:
                case 0.4f:
                    break;
                default:
                    break;//more
            }
            switch (Colour1)
            {
                case 0xffffffff:
                case 0xfffafafa:
                case 0xb4ffffff:
                case 0xffffdcc8:
                case 0xc8ffdcc8:
                case 0x5affffff:
                case 0xfffff2d1:
                case 0xc8ffffff:
                    break;
                default:
                    break;//more
            }
            switch (Colour2)
            {
                case 0xffffffff:
                case 0xffffefc2:
                case 0x32ffffff:
                case 0x78ffa680:
                case 0x50ffa680:
                case 0x96f7b068:
                case 0x5affffff:
                case 0xa0ffd280:
                case 0xb4ffffff:
                case 0xffffebba:
                case 0xffffb47a:
                case 0xbeffffff:
                    break;
                default:
                    break;//more
            }
            //if (Unknown_68h != 0)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.UnknownDataPointer = (ulong)(this.UnknownData != null ? this.UnknownData.FilePosition : 0);
            this.EmitterRuleNamePointer = (ulong)(this.EmitterRuleName != null ? this.EmitterRuleName.FilePosition : 0);
            this.ParticleRuleNamePointer = (ulong)(this.ParticleRuleName != null ? this.ParticleRuleName.FilePosition : 0);
            this.EmitterRulePointer = (ulong)(this.EmitterRule != null ? this.EmitterRule.FilePosition : 0);
            this.ParticleRulePointer = (ulong)(this.ParticleRule != null ? this.ParticleRule.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Index);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.UnknownDataPointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.EmitterRuleNamePointer);
            writer.Write(this.ParticleRuleNamePointer);
            writer.Write(this.EmitterRulePointer);
            writer.Write(this.ParticleRulePointer);
            writer.Write(this.MoveSpeedScale);
            writer.Write(this.MoveSpeedScaleModifier);
            writer.Write(this.ParticleScale);
            writer.Write(this.ParticleScaleModifier);
            writer.Write(this.Colour1);
            writer.Write(this.Colour2);
            writer.Write(this.Unknown_68h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "EmitterRule", YptXml.XmlEscape(EmitterRuleName?.Value ?? ""));
            YptXml.StringTag(sb, indent, "ParticleRule", YptXml.XmlEscape(ParticleRuleName?.Value ?? ""));
            YptXml.ValueTag(sb, indent, "Unknown10", FloatUtil.ToString(Unknown_10h));
            YptXml.ValueTag(sb, indent, "Unknown14", FloatUtil.ToString(Unknown_14h));
            YptXml.ValueTag(sb, indent, "MoveSpeedScale", FloatUtil.ToString(MoveSpeedScale));
            YptXml.ValueTag(sb, indent, "MoveSpeedScaleModifier", FloatUtil.ToString(MoveSpeedScaleModifier));
            YptXml.ValueTag(sb, indent, "ParticleScale", FloatUtil.ToString(ParticleScale));
            YptXml.ValueTag(sb, indent, "ParticleScaleModifier", FloatUtil.ToString(ParticleScaleModifier));
            YptXml.ValueTag(sb, indent, "Colour1", YptXml.UintString(Colour1));
            YptXml.ValueTag(sb, indent, "Colour2", YptXml.UintString(Colour2));
            if (UnknownData != null)
            {
                YptXml.OpenTag(sb, indent, "UnknownData");
                UnknownData.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "UnknownData");
            }
        }
        public void ReadXml(XmlNode node)
        {
            EmitterRuleName = (string_r)Xml.GetChildInnerText(node, "EmitterRule"); if (EmitterRuleName.Value == null) EmitterRuleName = null;
            ParticleRuleName = (string_r)Xml.GetChildInnerText(node, "ParticleRule"); if (ParticleRuleName.Value == null) ParticleRuleName = null;
            Unknown_10h = Xml.GetChildFloatAttribute(node, "Unknown10");
            Unknown_14h = Xml.GetChildFloatAttribute(node, "Unknown14");
            MoveSpeedScale = Xml.GetChildFloatAttribute(node, "MoveSpeedScale");
            MoveSpeedScaleModifier = Xml.GetChildFloatAttribute(node, "MoveSpeedScaleModifier");
            ParticleScale = Xml.GetChildFloatAttribute(node, "ParticleScale");
            ParticleScaleModifier = Xml.GetChildFloatAttribute(node, "ParticleScaleModifier");
            Colour1 = Xml.GetChildUIntAttribute(node, "Colour1");
            Colour2 = Xml.GetChildUIntAttribute(node, "Colour2");
            var udnode = node.SelectSingleNode("UnknownData");
            if (udnode != null)
            {
                UnknownData = new ParticleUnknown1();
                UnknownData.ReadXml(udnode);
            }
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (UnknownData != null) list.Add(UnknownData);
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


    [TC(typeof(EXP))] public class ParticleUnknown1 : ResourceSystemBlock
    {
        public override long BlockLength => 0x40;

        // structure data
        public ResourceSimpleList64<ParticleStringBlock> EventEmitterFlags { get; set; }
        public ResourceSimpleList64<ParticleUnknown2> Unknown_10h { get; set; }
        public ulong Unknown_20h = 1; // 0x0000000000000001
        public ResourceSimpleList64<ParticleUnknown2Block> Unknown_28h { get; set; }
        public ulong Unknown_38h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.EventEmitterFlags = reader.ReadBlock<ResourceSimpleList64<ParticleStringBlock>>();
            this.Unknown_10h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown2>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown2Block>>();
            this.Unknown_38h = reader.ReadUInt64();

            //if (Unknown_20h != 1)
            //{ }//no hit
            //if (Unknown_38h != 0)
            //{ }//no hit

            var cnt1 = (EventEmitterFlags?.data_items?.Length ?? 0);
            var cnt2 = (Unknown_10h?.data_items?.Length ?? 0);
            var cnt3 = (Unknown_28h?.data_items?.Length ?? 0);

            if (cnt2 != cnt3)
            { }//no hit
            if ((cnt2 != 0) && (cnt2 != cnt1))
            { }//hit
            if ((cnt3 != 0) && (cnt3 != cnt1))
            { }//hit


            //var dic = new Dictionary<MetaHash, ParticleUnknown2>();
            //if (Unknown_10h?.data_items != null)
            //{
            //    foreach (var item in Unknown_10h.data_items)
            //    {
            //        dic[item.NameHash] = item;
            //    }
            //}
            //if (Unknown_28h?.data_items != null)
            //{
            //    MetaHash lasthash = 0;
            //    foreach (var item in Unknown_28h.data_items)
            //    {
            //        if (item.NameHash < lasthash)
            //        { }//no hit! - this array is a sorted dictionary of the items!
            //        lasthash = item.NameHash;
            //        if (dic.TryGetValue(item.NameHash, out ParticleUnknown2 oitem))
            //        {
            //            if (item.Item != oitem)
            //            { }//no hit
            //        }
            //        else
            //        { }//no hit
            //    }
            //}


        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(this.EventEmitterFlags);
            writer.WriteBlock(this.Unknown_10h);
            writer.Write(this.Unknown_20h);
            writer.WriteBlock(this.Unknown_28h);
            writer.Write(this.Unknown_38h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            if (EventEmitterFlags?.data_items != null)
            {
                if (EventEmitterFlags.data_items.Length > 0)
                {
                    YptXml.OpenTag(sb, indent, "EventEmitterFlags");
                    foreach (var item in EventEmitterFlags.data_items)
                    {
                        YptXml.StringTag(sb, indent + 1, "Item", YptXml.XmlEscape(item?.Name?.Value ?? ""));
                    }
                    YptXml.CloseTag(sb, indent, "EventEmitterFlags");
                }
                else
                {
                    YptXml.SelfClosingTag(sb, indent, "EventEmitterFlags");
                }
            }
            if (Unknown_10h?.data_items != null)
            {
                YptXml.WriteItemArray(sb, Unknown_10h.data_items, indent, "Unknown10");
            }
            //if (Unknown_28h?.data_items != null)
            //{
            //    YptXml.WriteItemArray(sb, Unknown_28h.data_items, indent, "Unknown28");
            //}
        }
        public void ReadXml(XmlNode node)
        {
            EventEmitterFlags = new ResourceSimpleList64<ParticleStringBlock>();
            //EventEmitterFlags.data_items = XmlMeta.ReadItemArray<ParticleStringBlock>(node, "EventEmitterFlags");
            var unode = node.SelectSingleNode("EventEmitterFlags");
            if (unode != null)
            {
                var inodes = unode.SelectNodes("Item");
                var ilist = new List<ParticleStringBlock>();
                foreach (XmlNode inode in inodes)
                {
                    var iname = inode.InnerText;
                    var blk = new ParticleStringBlock();
                    blk.Name = (string_r)iname;
                    ilist.Add(blk);
                }
                EventEmitterFlags.data_items = ilist.ToArray();
            }

            Unknown_10h = new ResourceSimpleList64<ParticleUnknown2>();
            Unknown_10h.data_items = XmlMeta.ReadItemArray<ParticleUnknown2>(node, "Unknown10");

            Unknown_28h = new ResourceSimpleList64<ParticleUnknown2Block>();
            //Unknown_28h.data_items = XmlMeta.ReadItemArray<ParticleUnknown2Block>(node, "Unknown28");
            if (Unknown_10h.data_items != null)
            {
                var blist = new List<ParticleUnknown2Block>();
                foreach (var item in Unknown_10h.data_items)
                {
                    var blk = new ParticleUnknown2Block();
                    blk.Item = item;
                    blk.Name = item.Name;
                    blist.Add(blk);
                }
                blist.Sort((a, b) => a.Name.Hash.CompareTo(b.Name.Hash));
                Unknown_28h.data_items = blist.ToArray();
            }
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, EventEmitterFlags),
                new Tuple<long, IResourceBlock>(0x10, Unknown_10h),
                new Tuple<long, IResourceBlock>(0x28, Unknown_28h)
            };
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }


    [TC(typeof(EXP))] public class ParticleStringBlock : ResourceSystemBlock
    {
        public override long BlockLength => 24;

        // structure data
        public ulong NamePointer { get; set; }
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000

        // reference data
        public string_r Name { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.NamePointer = reader.ReadUInt64();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(this.NamePointer);

            //if (!string.IsNullOrEmpty(String1?.Value))
            //{
            //    JenkIndex.Ensure(String1.Value);
            //}


            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.Name != null ? this.Name.FilePosition : 0);

            // write structure data
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
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


    [TC(typeof(EXP))] public class ParticleUnknown2Block : ResourceSystemBlock
    {
        public override long BlockLength => 0x10;

        // structure data
        public ParticleKeyframePropName Name { get; set; }
        public uint Unknown_4h; // 0x00000000
        public ulong ItemPointer { get; set; }

        // reference data
        public ParticleUnknown2 Item { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Name = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.ItemPointer = reader.ReadUInt64();

            // read reference data
            this.Item = reader.ReadBlockAt<ParticleUnknown2>(this.ItemPointer);

            if (Item != null)
            { }
            if ((Item?.Name ?? 0) != Name)
            { }//no hit! so this is just a "dictionary" entry for an Item!

            //if (Unknown_4h != 0)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.ItemPointer = (ulong)(this.Item != null ? this.Item.FilePosition : 0);

            // write structure data
            writer.Write(this.Name);
            writer.Write(this.Unknown_4h);
            writer.Write(this.ItemPointer);
        }

        public override string ToString()
        {
            return Name.ToString();
        }

    }


    [TC(typeof(EXP))] public class ParticleUnknown2 : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength => 24;

        // structure data
        public ResourceSimpleList64<ParticleUnknown3> Unknown_0h { get; set; }
        public ParticleKeyframePropName Name { get; set; }
        public uint Unknown_14h { get; set; } // 0, 1

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadBlock<ResourceSimpleList64<ParticleUnknown3>>();
            this.Name = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();

            //switch (Unknown_14h)
            //{
            //    case 1:
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.WriteBlock(this.Unknown_0h);
            writer.Write(this.Name);
            writer.Write(this.Unknown_14h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", Name.ToString());
            YptXml.ValueTag(sb, indent, "Unknown14", Unknown_14h.ToString());
            if (Unknown_0h?.data_items != null)
            {
                YptXml.WriteItemArray(sb, Unknown_0h.data_items, indent, "Items");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            Unknown_14h = Xml.GetChildUIntAttribute(node, "Unknown14");
            Unknown_0h = new ResourceSimpleList64<ParticleUnknown3>();
            Unknown_0h.data_items = XmlMeta.ReadItemArray<ParticleUnknown3>(node, "Items");
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, Unknown_0h)
            };
        }

        public override string ToString()
        {
            return Name.ToString();
        }

    }


    [TC(typeof(EXP))] public class ParticleUnknown3 : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength => 0x30;

        // structure data
        public ResourceSimpleList64<ParticleKeyframePropValue> Unknown_0h { get; set; }
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public uint Unknown_20h { get; set; } // 0, 1, 2, 3, 4
        public uint Unknown_24h { get; set; } // 0, 1
        public ulong Unknown_28h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadBlock<ResourceSimpleList64<ParticleKeyframePropValue>>();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt64();

            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
            //switch (Unknown_20h)
            //{
            //    case 3:
            //    case 2:
            //    case 1:
            //    case 0:
            //    case 4:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_24h)
            //{
            //    case 0:
            //    case 1:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_28h != 0)
            //{ }//no hit
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
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.ValueTag(sb, indent, "Unknown20", Unknown_20h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown24", Unknown_24h.ToString());
            if (Unknown_0h?.data_items != null)
            {
                YptXml.WriteItemArray(sb, Unknown_0h.data_items, indent, "Keyframes");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Unknown_20h = Xml.GetChildUIntAttribute(node, "Unknown20");
            Unknown_24h = Xml.GetChildUIntAttribute(node, "Unknown24");
            Unknown_0h = new ResourceSimpleList64<ParticleKeyframePropValue>();
            Unknown_0h.data_items = XmlMeta.ReadItemArray<ParticleKeyframePropValue>(node, "Keyframes");
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0, Unknown_0h)
            };
        }

        public override string ToString()
        {
            return Unknown_20h.ToString() + ", " + Unknown_24h.ToString();
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
        public uint Unknown_10h { get; set; } // 2, 3, 4, 5, 6, 10, 21
        public uint Unknown_14h; // 0x00000000
        public float Unknown_18h { get; set; } = 4.1f; // 4.1f
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
        public uint Unknown_628h { get; set; } // 0, 1
        public uint Unknown_62Ch; // 0x00000000

        // reference data
        public string_r Name { get; set; }
        public MetaHash NameHash { get; set; }
        public ParticleDomain Domain1 { get; set; }
        public ParticleDomain Domain2 { get; set; }
        public ParticleDomain Domain3 { get; set; }
        public ResourcePointerArray64<ParticleKeyframeProp> KeyframeProps2 { get; set; }//just pointers to KeyframeProps1

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


            if (!string.IsNullOrEmpty(Name?.Value))
            {
                JenkIndex.Ensure(Name.Value);
            }

            //if ((Domain1 != null) && (Domain1.Index != 0))
            //{ }//no hit
            //if ((Domain2 != null) && (Domain2.Index != 1))
            //{ }//no hit
            //if ((Domain3 != null) && (Domain3.Index != 2))
            //{ }//no hit

            //if (KeyframeProps2?.data_items != null)
            //{
            //    if (KeyframeProps2.data_items.Length != 10)
            //    { }//no hit
            //    else
            //    {
            //        for (int i = 0; i < 10; i++)
            //        {
            //            if (KeyframeProps2.data_items[i] != KeyframeProps1[i])
            //            { }//no hit
            //        }
            //    }
            //}
            //else
            //{ }//no hit

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //switch (Unknown_10h)
            //{
            //    case 3:
            //    case 2:
            //    case 4:
            //    case 5:
            //    case 10:
            //    case 21:
            //    case 6:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_14h != 0)
            //{ }//no hit
            //if (Unknown_18h != 4.1f)
            //{ }//no hit
            //if (Unknown_1Ch != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
            //if (Unknown_30h != 0)
            //{ }//no hit
            //if (Unknown_40h != 0)
            //{ }//no hit
            //if (Unknown_50h != 0)
            //{ }//no hit
            //if (Unknown_60h != 0)
            //{ }//no hit
            //if (Unknown_68h != 0)
            //{ }//no hit
            //if (Unknown_70h != 0)
            //{ }//no hit
            //if (KeyframePropsCount1 != 10)
            //{ }//no hit
            //if (KeyframePropsCount2 != 10)
            //{ }//no hit
            //if (Unknown_624h != 0)
            //{ }//no hit
            //switch (Unknown_628h)
            //{
            //    case 0:
            //    case 1:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_62Ch != 0)
            //{ }//no hit
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
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", YptXml.XmlEscape(Name?.Value ?? ""));
            YptXml.ValueTag(sb, indent, "Unknown10", Unknown_10h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown628", Unknown_628h.ToString());
            ParticleDomain.WriteXmlNode(Domain1, sb, indent, "Domain1");
            ParticleDomain.WriteXmlNode(Domain2, sb, indent, "Domain2");
            ParticleDomain.WriteXmlNode(Domain3, sb, indent, "Domain3");
            if (KeyframeProps1 != null)
            {
                YptXml.WriteItemArray(sb, KeyframeProps1, indent, "KeyframeProperties");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name = (string_r)Xml.GetChildInnerText(node, "Name"); if (Name.Value == null) Name = null;
            NameHash = JenkHash.GenHash(Name?.Value ?? "");
            Unknown_10h = Xml.GetChildUIntAttribute(node, "Unknown10");
            Unknown_628h = Xml.GetChildUIntAttribute(node, "Unknown628");
            Domain1 = ParticleDomain.ReadXmlNode(node.SelectSingleNode("Domain1")); if (Domain1 != null) Domain1.Index = 0;
            Domain2 = ParticleDomain.ReadXmlNode(node.SelectSingleNode("Domain2")); if (Domain2 != null) Domain2.Index = 1;
            Domain3 = ParticleDomain.ReadXmlNode(node.SelectSingleNode("Domain3")); if (Domain3 != null) Domain3.Index = 2;

            var kflist = XmlMeta.ReadItemArray<ParticleKeyframeProp>(node, "KeyframeProperties")?.ToList() ?? new List<ParticleKeyframeProp>();
            KeyframeProps1 = new ParticleKeyframeProp[10];
            for (int i = 0; i < 10; i++)
            {
                KeyframeProps1[i] = (i < kflist.Count) ? kflist[i] : new ParticleKeyframeProp();
            }

            KeyframeProps2 = new ResourcePointerArray64<ParticleKeyframeProp>();
            KeyframeProps2.data_items = KeyframeProps1;
            KeyframeProps2.ManualReferenceOverride = true;
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            if (Domain1 != null) list.Add(Domain1);
            if (Domain2 != null) list.Add(Domain2);
            if (Domain3 != null) list.Add(Domain3);
            if (KeyframeProps2 != null)
            {
                KeyframeProps2.ManualReferenceOverride = true;
                list.Add(KeyframeProps2);
            }
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
        public ParticleKeyframePropName Name { get; set; } // name hash?
        public uint Unknown_6Ch { get; set; } //offset..?
        public ResourceSimpleList64<ParticleKeyframePropValue> Values { get; set; }
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
            this.Name = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Values = reader.ReadBlock<ResourceSimpleList64<ParticleKeyframePropValue>>();
            this.Unknown_80h = reader.ReadUInt64();
            this.Unknown_88h = reader.ReadUInt64();


            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
            //if (Unknown_30h != 0)
            //{ }//no hit
            //if (Unknown_38h != 0)
            //{ }//no hit
            //if (Unknown_40h != 0)
            //{ }//no hit
            //if (Unknown_48h != 0)
            //{ }//no hit
            //if (Unknown_50h != 0)
            //{ }//no hit
            //if (Unknown_58h != 0)
            //{ }//no hit
            //if (Unknown_60h != 0)
            //{ }//no hit
            switch (Unknown_6Ch)//some offset..?
            {
                case 0x00007a00:
                case 0x00007b00:
                case 0x00007c00:
                case 0x00007d00:
                case 0x00007e00:
                case 0x00007f00:
                case 0x00008000:
                case 0x00008100:
                case 0x00008200:
                case 0x00008300:
                case 0x0000e400:
                case 0x0000e500:
                case 0x0000e600:
                case 0x0000e700:
                case 0x0000e800:
                case 0x0000e900:
                case 0x0000ea00:
                case 0x0000eb00:
                case 0x0000ec00:
                case 0x0000ed00:
                case 0x0000ee00:
                case 0x0000ef00:
                case 0x0000f000:
                case 0x0000f100:
                case 0x0000f200:
                case 0x0000f300:
                case 0x0000f400:
                case 0x00000600:
                case 0x00000700:
                case 0x00000800:
                    break;
                default:
                    break;///and more......
            }
            //if (Unknown_80h != 0)
            //{ }//no hit
            //if (Unknown_88h != 0)
            //{ }//no hit
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
            writer.Write(this.Name);
            writer.Write(this.Unknown_6Ch);
            writer.WriteBlock(this.Values);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_88h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.StringTag(sb, indent, "Name", Name.ToString());
            YptXml.ValueTag(sb, indent, "Unknown6C", Unknown_6Ch.ToString());

            if (Values?.data_items != null)
            {
                YptXml.WriteItemArray(sb, Values.data_items, indent, "Keyframes");
            }

        }
        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            Unknown_6Ch = Xml.GetChildUIntAttribute(node, "Unknown6C");

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
        public float InterpolationInterval { get; set; }
        public float KeyFrameMultiplier { get; set; }
        public ulong Unknown_8h; // 0x0000000000000000
        public float RedChannelColour { get; set; }
        public float GreenChannelColour { get; set; }
        public float BlueChannelColour { get; set; }
        public float AlphaChannelColour { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.InterpolationInterval = reader.ReadSingle();
            this.KeyFrameMultiplier = reader.ReadSingle();
            this.Unknown_8h = reader.ReadUInt64();
            this.RedChannelColour = reader.ReadSingle();
            this.GreenChannelColour = reader.ReadSingle();
            this.BlueChannelColour = reader.ReadSingle();
            this.AlphaChannelColour = reader.ReadSingle();

            switch (InterpolationInterval)
            {
                case 0:
                case 1.0f:
                case 0.6f:
                case 0.010234f:
                case 0.12f:
                case 0.8f:
                    break;
                default:
                    break; //and more..
            }
            switch (KeyFrameMultiplier)
            {
                case 0:
                case 1.0f:
                case 1.66666663f:
                case 97.7135f:
                case 8.333334f:
                case 1.47058821f:
                case 5.00000048f:
                    break;
                default:
                    break; //and more...
            }
            //if (Unknown_8h != 0)
            //{ }//no hit
            switch (RedChannelColour)
            {
                case 0:
                case 1.2f:
                case 5.0f:
                case 2.4f:
                case 7.0f:
                case 1.0f:
                case 0.6f:
                case 0.931395f:
                case 0.45f:
                case 0.55f:
                case 0.5f:
                    break;
                default:
                    break; //and more..
            }
            switch (GreenChannelColour)
            {
                case 0:
                case 1.2f:
                case 5.0f:
                case 2.4f:
                case 7.0f:
                case 1.0f:
                case 0.6f:
                case 0.73913f:
                case 0.3f:
                case 0.5f:
                    break;
                default:
                    break; //and more...
            }
            switch (BlueChannelColour)
            {
                case -0.8f:
                case -0.5f:
                case 0:
                case 1.0f:
                case 0.213439f:
                case 4.000001f:
                case 0.05f:
                    break;
                default:
                    break; //and more...
            }
            switch (AlphaChannelColour)
            {
                case 0:
                case 1.0f:
                case 0.669767f:
                case 0.945107f:
                case 0.798588f:
                case 0.03f:
                case 0.6f:
                    break;
                default:
                    break;// and more..
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.InterpolationInterval);
            writer.Write(this.KeyFrameMultiplier);
            writer.Write(this.Unknown_8h);
            writer.Write(this.RedChannelColour);
            writer.Write(this.GreenChannelColour);
            writer.Write(this.BlueChannelColour);
            writer.Write(this.AlphaChannelColour);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.ValueTag(sb, indent, "InterpolationInterval", FloatUtil.ToString(InterpolationInterval));
            YptXml.ValueTag(sb, indent, "KeyFrameMultiplier", FloatUtil.ToString(KeyFrameMultiplier));
            YptXml.ValueTag(sb, indent, "RedChannelColour", FloatUtil.ToString(RedChannelColour));
            YptXml.ValueTag(sb, indent, "GreenChannelColour", FloatUtil.ToString(GreenChannelColour));
            YptXml.ValueTag(sb, indent, "BlueChannelColour", FloatUtil.ToString(BlueChannelColour));
            YptXml.ValueTag(sb, indent, "AlphaChannelColour", FloatUtil.ToString(AlphaChannelColour));
        }
        public void ReadXml(XmlNode node)
        {
            InterpolationInterval = Xml.GetChildFloatAttribute(node, "InterpolationInterval");
            KeyFrameMultiplier = Xml.GetChildFloatAttribute(node, "KeyFrameMultiplier");
            RedChannelColour = Xml.GetChildFloatAttribute(node, "RedChannelColour");
            GreenChannelColour = Xml.GetChildFloatAttribute(node, "GreenChannelColour");
            BlueChannelColour = Xml.GetChildFloatAttribute(node, "BlueChannelColour");
            AlphaChannelColour = Xml.GetChildFloatAttribute(node, "AlphaChannelColour");
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}, {5}", InterpolationInterval, KeyFrameMultiplier, RedChannelColour, GreenChannelColour, BlueChannelColour, AlphaChannelColour);
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
        public uint Unknown_4h = 1; // 0x00000001
        public uint Index { get; set; } // 0, 1, 2   - index of this domain in the ParticleEmitterRule
        public ParticleDomainType DomainType { get; set; }
        public byte Unknown_Dh; // 0x00
        public ushort Unknown_Eh; // 0x0000
        public uint Unknown_10h { get; set; } // eg. 0x00010100
        public uint Unknown_14h; // 0x00000000
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public float Unknown_258h { get; set; } // -1.0f, 2.0f, 2.1f
        public uint Unknown_25Ch; // 0x00000000
        public ResourcePointerList64<ParticleKeyframeProp> KeyframeProps { get; set; }
        public ulong Unknown_270h; // 0x0000000000000000
        public ulong Unknown_278h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Index = reader.ReadUInt32();
            this.DomainType = (ParticleDomainType)reader.ReadByte();
            this.Unknown_Dh = reader.ReadByte();
            this.Unknown_Eh = reader.ReadUInt16();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_258h = reader.ReadSingle();
            this.Unknown_25Ch = reader.ReadUInt32();
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_270h = reader.ReadUInt64();
            this.Unknown_278h = reader.ReadUInt64();


            //if (KeyframeProps?.data_items?.Length != 4)
            //{ }//no hit
            //else
            //{
            //    if (KeyframeProps.data_items[0] != KeyframeProp0)
            //    { }//no hit
            //    if (KeyframeProps.data_items[1] != KeyframeProp1)
            //    { }//no hit
            //    if (KeyframeProps.data_items[3] != KeyframeProp2)
            //    { }//no hit - note stupid ordering
            //    if (KeyframeProps.data_items[2] != KeyframeProp3)
            //    { }//no hit - note stupid ordering
            //}
            //if (KeyframeProps?.EntriesCapacity != 16)
            //{ }//no hit  ... how to handle this when saving???



            //if (Unknown_4h != 1)
            //{ }//no hit
            //switch (Index)
            //{
            //    case 0:
            //    case 1:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_Dh != 0)
            //{ }//no hit
            //if (Unknown_Eh != 0)
            //{ }//no hit
            //switch (Unknown_10h)
            //{
            //    case 0:
            //    case 0x00000100:
            //    case 0x00000101:
            //    case 1:
            //    case 0x00010001:
            //    case 0x00010000:
            //    case 0x00010100:
            //    case 0x00010101:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_14h != 0)
            //{ }//no hit
            //switch (Unknown_258h)
            //{
            //    case 2.0f:
            //    case 2.1f:
            //    case -1.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_25Ch != 0)
            //{ }//no hit
            //if (Unknown_270h != 0)
            //{ }//no hit
            //if (Unknown_278h != 0)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Index);
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
        public virtual void WriteXml(StringBuilder sb, int indent)
        {
            YptXml.ValueTag(sb, indent, "Type", DomainType.ToString());
            YptXml.ValueTag(sb, indent, "Unknown10", YptXml.UintString(Unknown_10h));
            YptXml.ValueTag(sb, indent, "Unknown258", FloatUtil.ToString(Unknown_258h));
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
            if (KeyframeProp2 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty2");
                KeyframeProp2.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty2");
            }
            if (KeyframeProp3 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty3");
                KeyframeProp3.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty3");
            }
        }
        public virtual void ReadXml(XmlNode node)
        {
            DomainType = Xml.GetEnumValue<ParticleDomainType>(Xml.GetChildStringAttribute(node, "Type"));
            Unknown_10h = Xml.GetChildUIntAttribute(node, "Unknown10");
            Unknown_258h = Xml.GetChildFloatAttribute(node, "Unknown258");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            KeyframeProp2 = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("KeyframeProperty2");
            if (pnode2 != null)
            {
                KeyframeProp2.ReadXml(pnode2);
            }

            KeyframeProp3 = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("KeyframeProperty3");
            if (pnode3 != null)
            {
                KeyframeProp3.ReadXml(pnode3);
            }

            KeyframeProps = new ResourcePointerList64<ParticleKeyframeProp>();
            KeyframeProps.data_items = new[] { KeyframeProp0, KeyframeProp1, KeyframeProp3, KeyframeProp2, null, null, null, null, null, null, null, null, null, null, null, null };

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
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Type = (ParticleBehaviourType)reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.KeyframeProps = reader.ReadBlock<ResourcePointerList64<ParticleKeyframeProp>>();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();

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
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write((uint)this.Type);
            writer.Write(this.Unknown_Ch);
            writer.WriteBlock(this.KeyframeProps);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
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

    [TC(typeof(EXP))] public class ParticleBehaviourAge : ParticleBehaviour
    {
        // ptxu_Age
        public override long BlockLength => 0x30;


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data

            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourAcceleration : ParticleBehaviour
    {
        // ptxu_Acceleration
        public override long BlockLength => 0x170;

        // structure data
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ulong Unknown_150h; // 0x0000000000000000
        public uint Unknown_158h { get; set; } // 0, 1, 2
        public uint Unknown_15Ch { get; set; } // 0, 1
        public ulong Unknown_160h; // 0x0000000000000000
        public ulong Unknown_168h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt64();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt64();
            this.Unknown_168h = reader.ReadUInt64();

            //if (Unknown_150h != 0)
            //{ }//no hit
            //switch (Unknown_158h)
            //{
            //    case 0:
            //    case 1:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_15Ch)
            //{
            //    case 0:
            //    case 1:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_160h != 0)
            //{ }//no hit
            //if (Unknown_168h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_168h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown158", Unknown_158h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown15C", Unknown_15Ch.ToString());
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_158h = Xml.GetChildUIntAttribute(node, "Unknown158");
            Unknown_15Ch = Xml.GetChildUIntAttribute(node, "Unknown15C");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            CreateKeyframeProps(KeyframeProp0, KeyframeProp1);
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


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data

            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourRotation : ParticleBehaviour
    {
        // ptxu_Rotation
        public override long BlockLength => 0x280;

        // structure data
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public uint Unknown_270h { get; set; } // 0, 1, 2
        public uint Unknown_274h { get; set; } // 0, 1, 2
        public uint Unknown_278h { get; set; } // eg. 0x00010101
        public float Unknown_27Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_270h = reader.ReadUInt32();
            this.Unknown_274h = reader.ReadUInt32();
            this.Unknown_278h = reader.ReadUInt32();
            this.Unknown_27Ch = reader.ReadSingle();

            //switch (Unknown_270h)
            //{
            //    case 1:
            //    case 0:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_274h)
            //{
            //    case 1:
            //    case 0:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_278h)
            //{
            //    case 0x00010000:
            //    case 1:
            //    case 0:
            //    case 0x00010001:
            //    case 0x00000101:
            //    case 0x00010101:
            //    case 0x00010100:
            //        break;
            //    default:
            //        break;//no hit
            //}
            switch (Unknown_27Ch)
            {
                case 0:
                case 0.5f:
                case 1.0f:
                case 0.001f:
                case 0.01f:
                case 0.1f:
                    break;
                default:
                    break;//and more..
            }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.Write(this.Unknown_270h);
            writer.Write(this.Unknown_274h);
            writer.Write(this.Unknown_278h);
            writer.Write(this.Unknown_27Ch);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown270", Unknown_270h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown274", Unknown_274h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown278", YptXml.UintString(Unknown_278h));
            YptXml.ValueTag(sb, indent, "Unknown27C", FloatUtil.ToString(Unknown_27Ch));
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
            if (KeyframeProp2 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty2");
                KeyframeProp2.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty2");
            }
            if (KeyframeProp3 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty3");
                KeyframeProp3.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty3");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_270h = Xml.GetChildUIntAttribute(node, "Unknown270");
            Unknown_274h = Xml.GetChildUIntAttribute(node, "Unknown274");
            Unknown_278h = Xml.GetChildUIntAttribute(node, "Unknown278");
            Unknown_27Ch = Xml.GetChildFloatAttribute(node, "Unknown27C");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            KeyframeProp2 = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("KeyframeProperty2");
            if (pnode2 != null)
            {
                KeyframeProp2.ReadXml(pnode2);
            }

            KeyframeProp3 = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("KeyframeProperty3");
            if (pnode3 != null)
            {
                KeyframeProp3.ReadXml(pnode3);
            }

            CreateKeyframeProps(KeyframeProp0, KeyframeProp1, KeyframeProp2, KeyframeProp3);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public uint Unknown_270h { get; set; } // 0, 1, 2
        public uint Unknown_274h { get; set; } // 0, 1
        public ulong Unknown_278h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_270h = reader.ReadUInt32();
            this.Unknown_274h = reader.ReadUInt32();
            this.Unknown_278h = reader.ReadUInt64();

            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
            //switch (Unknown_270h)
            //{
            //    case 0:
            //    case 1:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_274h)
            //{
            //    case 1:
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_278h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.Write(this.Unknown_270h);
            writer.Write(this.Unknown_274h);
            writer.Write(this.Unknown_278h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown270", Unknown_270h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown274", Unknown_274h.ToString());
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
            if (KeyframeProp2 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty2");
                KeyframeProp2.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty2");
            }
            if (KeyframeProp3 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty3");
                KeyframeProp3.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty3");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_270h = Xml.GetChildUIntAttribute(node, "Unknown270");
            Unknown_274h = Xml.GetChildUIntAttribute(node, "Unknown274");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            KeyframeProp2 = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("KeyframeProperty2");
            if (pnode2 != null)
            {
                KeyframeProp2.ReadXml(pnode2);
            }

            KeyframeProp3 = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("KeyframeProperty3");
            if (pnode3 != null)
            {
                KeyframeProp3.ReadXml(pnode3);
            }

            CreateKeyframeProps(KeyframeProp0, KeyframeProp1, KeyframeProp2, KeyframeProp3);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ulong Unknown_150h; // 0x0000000000000000
        public uint Unknown_158h { get; set; } // 0, 1, 2
        public uint Unknown_15Ch; // 0x00000000
        public ulong Unknown_160h; // 0x0000000000000000
        public ulong Unknown_168h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt64();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt64();
            this.Unknown_168h = reader.ReadUInt64();

            //if (Unknown_150h != 0)
            //{ }//no hit
            //switch (Unknown_158h)
            //{
            //    case 0:
            //    case 1:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_15Ch != 0)
            //{ }//no hit
            //if (Unknown_160h != 0)
            //{ }//no hit
            //if (Unknown_168h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_168h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown158", Unknown_158h.ToString());
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_158h = Xml.GetChildUIntAttribute(node, "Unknown158");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            CreateKeyframeProps(KeyframeProp0, KeyframeProp1);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public uint Unknown_C0h { get; set; } // 0, 1
        public uint Unknown_C4h; // 0x00000000
        public ulong Unknown_C8h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_C0h = reader.ReadUInt32();
            this.Unknown_C4h = reader.ReadUInt32();
            this.Unknown_C8h = reader.ReadUInt64();


            //switch (Unknown_C0h)
            //{
            //    case 1:
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_C4h != 0)
            //{ }//no hit
            //if (Unknown_C8h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProp0);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C4h);
            writer.Write(this.Unknown_C8h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "UnknownC0", Unknown_C0h.ToString());
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_C0h = Xml.GetChildUIntAttribute(node, "UnknownC0");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            CreateKeyframeProps(KeyframeProp0);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public float Unknown_150h { get; set; }
        public float Unknown_154h { get; set; }
        public uint Unknown_158h { get; set; } // 30, 50, 60, 70, 100
        public uint Unknown_15Ch { get; set; } // 0, 20, 25, 40, 50, 60, 65, 75, 100
        public ulong Unknown_160h; // 0x0000000000000000
        public ulong Unknown_168h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadSingle();
            this.Unknown_154h = reader.ReadSingle();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt64();
            this.Unknown_168h = reader.ReadUInt64();

            switch (Unknown_150h)
            {
                case 0.001f:
                case 0.02f:
                case 0.1f:
                case 0.5f:
                case 0.4f:
                case 0.01f:
                case 0:
                    break;
                default:
                    break;//more
            }
            switch (Unknown_154h)
            {
                case 0.05f:
                case 0.2f:
                case 0.1f:
                case 0.4f:
                case 0:
                    break;
                default:
                    break;//more
            }
            //switch (Unknown_158h)//percentage
            //{
            //    case 100:
            //    case 70:
            //    case 50:
            //    case 60:
            //    case 30:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_15Ch)//percentage
            //{
            //    case 0:
            //    case 100:
            //    case 60:
            //    case 40:
            //    case 50:
            //    case 75:
            //    case 65:
            //    case 20:
            //    case 25:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_160h != 0)
            //{ }//no hit
            //if (Unknown_168h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_154h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_168h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown150", FloatUtil.ToString(Unknown_150h));
            YptXml.ValueTag(sb, indent, "Unknown154", FloatUtil.ToString(Unknown_154h));
            YptXml.ValueTag(sb, indent, "Unknown158", Unknown_158h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown15C", Unknown_15Ch.ToString());
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_150h = Xml.GetChildFloatAttribute(node, "Unknown150");
            Unknown_154h = Xml.GetChildFloatAttribute(node, "Unknown154");
            Unknown_158h = Xml.GetChildUIntAttribute(node, "Unknown158");
            Unknown_15Ch = Xml.GetChildUIntAttribute(node, "Unknown15C");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            CreateKeyframeProps(KeyframeProp0, KeyframeProp1);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public uint Unknown_C0h { get; set; } // 0, 2
        public int Unknown_C4h { get; set; }
        public uint Unknown_C8h { get; set; } // 0, 1, 2
        public uint Unknown_CCh { get; set; } // eg. 0x01010100

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_C0h = reader.ReadUInt32();
            this.Unknown_C4h = reader.ReadInt32();
            this.Unknown_C8h = reader.ReadUInt32();
            this.Unknown_CCh = reader.ReadUInt32();

            //switch (Unknown_C0h)
            //{
            //    case 0:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            switch (Unknown_C4h)
            {
                case 3:
                case 48:
                case 0:
                case 11:
                case 35:
                case 43:
                case 24:
                case 7:
                case 37:
                case -1:  //0xffffffff..
                case 2:
                    break;
                default:
                    break;//more
            }
            //switch (Unknown_C8h)
            //{
            //    case 1:
            //    case 0:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            switch (Unknown_CCh)
            {
                case 0x01000001:
                case 0x01000101:
                case 0x01010100:
                case 0x01010000:
                case 0x01000000:
                case 0x01010101:
                case 0x01000100:
                case 0x01010001:
                case 1:
                    break;
                default:
                    break;//more
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProp0);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C4h);
            writer.Write(this.Unknown_C8h);
            writer.Write(this.Unknown_CCh);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "UnknownC0", Unknown_C0h.ToString());
            YptXml.ValueTag(sb, indent, "UnknownC4", Unknown_C4h.ToString());
            YptXml.ValueTag(sb, indent, "UnknownC8", Unknown_C8h.ToString());
            YptXml.ValueTag(sb, indent, "UnknownCC", YptXml.UintString(Unknown_CCh));
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_C0h = Xml.GetChildUIntAttribute(node, "UnknownC0");
            Unknown_C4h = Xml.GetChildIntAttribute(node, "UnknownC4");
            Unknown_C8h = Xml.GetChildUIntAttribute(node, "UnknownC8");
            Unknown_CCh = Xml.GetChildUIntAttribute(node, "UnknownCC");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            CreateKeyframeProps(KeyframeProp0);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public uint Unknown_1E0h { get; set; } // 0, 2
        public uint Unknown_1E4h { get; set; } // eg. 0x00010101
        public ulong Unknown_1E8h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_1E0h = reader.ReadUInt32();
            this.Unknown_1E4h = reader.ReadUInt32();
            this.Unknown_1E8h = reader.ReadUInt64();

            //switch (Unknown_1E0h)
            //{
            //    case 0:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_1E4h)
            //{
            //    case 0x00000100: // 256
            //    case 0x00000101:
            //    case 0x00010101:
            //    case 0x00010100:
            //    case 1:
            //    case 0:
            //    case 0x00010001:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_1E8h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.Write(this.Unknown_1E0h);
            writer.Write(this.Unknown_1E4h);
            writer.Write(this.Unknown_1E8h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown1E0", Unknown_1E0h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown1E4", YptXml.UintString(Unknown_1E4h));
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
            if (KeyframeProp2 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty2");
                KeyframeProp2.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty2");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_1E0h = Xml.GetChildUIntAttribute(node, "Unknown1E0");
            Unknown_1E4h = Xml.GetChildUIntAttribute(node, "Unknown1E4");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            KeyframeProp2 = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("KeyframeProperty2");
            if (pnode2 != null)
            {
                KeyframeProp2.ReadXml(pnode2);
            }

            CreateKeyframeProps(KeyframeProp0, KeyframeProp1, KeyframeProp2);
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
        public float Unknown_30h { get; set; } // 0, -0.1f, -1.0f, 1.0f, 0.57735f
        public float Unknown_34h { get; set; } // 0, -1.0f, 1.0f, 0.77f, 0.57735f
        public float Unknown_38h { get; set; } // 0, -0.125f, 1.0f, 0.77f, 0.57735f
        public uint Unknown_3Ch { get; set; } = 0x7f800001; // 0x7f800001
        public uint Unknown_40h { get; set; } // 0, 1, 2, 3, 4
        public float Unknown_44h { get; set; } // 0, 0.1f, 0.2f, 0.25f, 0.5f, 1.0f
        public float Unknown_48h { get; set; } // 0, 0.1f, 0.2f, 0.25f, 0.5f, 1.0f
        public float Unknown_4Ch { get; set; } // 0, -1.0f, -0.1f, ..., 0.15f, .., 3.0f, ...
        public float Unknown_50h { get; set; } // 0, 0.07f, 5.0f, 10.0f
        public float Unknown_54h { get; set; } // 0, 0.5f, 1.0f, 2.0f
        public float Unknown_58h { get; set; } // 0, 0.1f, 0.2f, ..., 0.75f, 1.0f
        public uint Unknown_5Ch { get; set; } // eg. 0x01010100
        public uint Unknown_60h { get; set; } // 0, 1, 0x100
        public uint Unknown_64h; // 0x00000000
        public ulong Unknown_68h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_30h = reader.ReadSingle();
            this.Unknown_34h = reader.ReadSingle();
            this.Unknown_38h = reader.ReadSingle();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadSingle();
            this.Unknown_48h = reader.ReadSingle();
            this.Unknown_4Ch = reader.ReadSingle();
            this.Unknown_50h = reader.ReadSingle();
            this.Unknown_54h = reader.ReadSingle();
            this.Unknown_58h = reader.ReadSingle();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.Unknown_60h = reader.ReadUInt32();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt64();

            //switch (Unknown_30h)
            //{
            //    case 0:
            //    case 0.57735f:
            //    case -0.1f:
            //    case 1.0f:
            //    case -1.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_34h)
            //{
            //    case 0:
            //    case 1.0f:
            //    case 0.77f:
            //    case 0.57735f:
            //    case -1.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_38h)
            //{
            //    case 0:
            //    case 1.0f:
            //    case 0.77f:
            //    case 0.57735f:
            //    case -0.125f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_3Ch != 0x7f800001)
            //{ }//no hit
            //switch (Unknown_40h)
            //{
            //    case 0:
            //    case 1:
            //    case 2:
            //    case 4:
            //    case 3:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_44h)
            //{
            //    case 0:
            //    case 0.5f:
            //    case 0.25f:
            //    case 1.0f:
            //    case 0.2f:
            //    case 0.1f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_48h)
            //{
            //    case 0:
            //    case 0.5f:
            //    case 1.0f:
            //    case 0.2f:
            //    case 0.1f:
            //    case 0.25f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_4Ch)
            //{
            //    case 0:
            //    case 1.0f:
            //    case -0.35f:
            //    case -0.5f:
            //    case -1.0f:
            //    case 0.15f:
            //    case 3.0f:
            //    case -0.1f:
            //    case -0.2f:
            //    case 0.001f:
            //    case 0.25f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_50h)
            //{
            //    case 0:
            //    case 5.0f:
            //    case 0.07f:
            //    case 10.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_54h)
            //{
            //    case 0:
            //    case 0.5f:
            //    case 1.0f:
            //    case 2.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_58h)
            //{
            //    case 0:
            //    case 0.6f:
            //    case 0.25f:
            //    case 0.75f:
            //    case 0.5f:
            //    case 0.65f:
            //    case 0.2f:
            //    case 0.4f:
            //    case 0.3f:
            //    case 0.1f:
            //    case 1.0f:
            //    case 0.7f:
            //    case 0.05f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_5Ch)
            //{
            //    case 0x00000100:
            //    case 0:
            //    case 0x00010100:
            //    case 0x00000101:
            //    case 0x01010100:
            //    case 0x01000100:
            //    case 0x00010000:
            //    case 0x00000001:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_60h)
            //{
            //    case 0:
            //    case 1:
            //    case 0x00000100:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_64h != 0)
            //{ }//no hit
            //if (Unknown_68h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
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
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown30", FloatUtil.ToString(Unknown_30h));
            YptXml.ValueTag(sb, indent, "Unknown34", FloatUtil.ToString(Unknown_34h));
            YptXml.ValueTag(sb, indent, "Unknown38", FloatUtil.ToString(Unknown_38h));
            YptXml.ValueTag(sb, indent, "Unknown40", Unknown_40h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown44", FloatUtil.ToString(Unknown_44h));
            YptXml.ValueTag(sb, indent, "Unknown48", FloatUtil.ToString(Unknown_48h));
            YptXml.ValueTag(sb, indent, "Unknown4C", FloatUtil.ToString(Unknown_4Ch));
            YptXml.ValueTag(sb, indent, "Unknown50", FloatUtil.ToString(Unknown_50h));
            YptXml.ValueTag(sb, indent, "Unknown54", FloatUtil.ToString(Unknown_54h));
            YptXml.ValueTag(sb, indent, "Unknown58", FloatUtil.ToString(Unknown_58h));
            YptXml.ValueTag(sb, indent, "Unknown5C", YptXml.UintString(Unknown_5Ch));
            YptXml.ValueTag(sb, indent, "Unknown60", YptXml.UintString(Unknown_60h));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_30h = Xml.GetChildFloatAttribute(node, "Unknown30");
            Unknown_34h = Xml.GetChildFloatAttribute(node, "Unknown34");
            Unknown_38h = Xml.GetChildFloatAttribute(node, "Unknown38");
            Unknown_40h = Xml.GetChildUIntAttribute(node, "Unknown40");
            Unknown_44h = Xml.GetChildFloatAttribute(node, "Unknown44");
            Unknown_48h = Xml.GetChildFloatAttribute(node, "Unknown48");
            Unknown_4Ch = Xml.GetChildFloatAttribute(node, "Unknown4C");
            Unknown_50h = Xml.GetChildFloatAttribute(node, "Unknown50");
            Unknown_54h = Xml.GetChildFloatAttribute(node, "Unknown54");
            Unknown_58h = Xml.GetChildFloatAttribute(node, "Unknown58");
            Unknown_5Ch = Xml.GetChildUIntAttribute(node, "Unknown5C");
            Unknown_60h = Xml.GetChildUIntAttribute(node, "Unknown60");
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourWind : ParticleBehaviour
    {
        // ptxu_Wind
        public override long BlockLength => 0xF0;

        // structure data
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ulong Unknown_C0h; // 0x0000000000000000
        public ulong Unknown_C8h; // 0x0000000000000000
        public float Unknown_D0h { get; set; } // 15.0f, 20.0f, ..., 100.0f
        public float Unknown_D4h { get; set; } // 30.0f, 50.0f, ..., 200.0f
        public uint Unknown_D8h { get; set; } // 0, 1, 2
        public uint Unknown_DCh { get; set; } // 0, 1, 2
        public uint Unknown_E0h { get; set; } // 0, 1
        public uint Unknown_E4h; // 0x00000000
        public ulong Unknown_E8h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_C0h = reader.ReadUInt64();
            this.Unknown_C8h = reader.ReadUInt64();
            this.Unknown_D0h = reader.ReadSingle();
            this.Unknown_D4h = reader.ReadSingle();
            this.Unknown_D8h = reader.ReadUInt32();
            this.Unknown_DCh = reader.ReadUInt32();
            this.Unknown_E0h = reader.ReadUInt32();
            this.Unknown_E4h = reader.ReadUInt32();
            this.Unknown_E8h = reader.ReadUInt64();

            //if (Unknown_C0h != 0)
            //{ }//no hit
            //if (Unknown_C8h != 0)
            //{ }//no hit
            switch (Unknown_D0h)
            {
                case 15.0f:
                case 20.0f:
                case 30.0f:
                case 100.0f:
                    break;
                default:
                    break;//more
            }
            switch (Unknown_D4h)
            {
                case 30.0f:
                case 50.0f:
                case 40.0f:
                case 200.0f:
                    break;
                default:
                    break;//more
            }
            //switch (Unknown_D8h)
            //{
            //    case 1:
            //    case 2:
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_DCh)
            //{
            //    case 1:
            //    case 0:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_E0h)
            //{
            //    case 0:
            //    case 1:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_E4h != 0)
            //{ }//no hit
            //if (Unknown_E8h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
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
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "UnknownD0", FloatUtil.ToString(Unknown_D0h));
            YptXml.ValueTag(sb, indent, "UnknownD4", FloatUtil.ToString(Unknown_D4h));
            YptXml.ValueTag(sb, indent, "UnknownD8", Unknown_D8h.ToString());
            YptXml.ValueTag(sb, indent, "UnknownDC", Unknown_DCh.ToString());
            YptXml.ValueTag(sb, indent, "UnknownE0", Unknown_E0h.ToString());
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_D0h = Xml.GetChildFloatAttribute(node, "UnknownD0");
            Unknown_D4h = Xml.GetChildFloatAttribute(node, "UnknownD4");
            Unknown_D8h = Xml.GetChildUIntAttribute(node, "UnknownD8");
            Unknown_DCh = Xml.GetChildUIntAttribute(node, "UnknownDC");
            Unknown_E0h = Xml.GetChildUIntAttribute(node, "UnknownE0");
            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            CreateKeyframeProps(KeyframeProp0);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public ParticleKeyframeProp KeyframeProp4 { get; set; }
        public ParticleKeyframeProp KeyframeProp5 { get; set; }
        public ParticleKeyframeProp KeyframeProp6 { get; set; }
        public ParticleKeyframeProp KeyframeProp7 { get; set; }
        public ParticleKeyframeProp KeyframeProp8 { get; set; }
        public float Unknown_540h { get; set; }
        public uint Unknown_544h { get; set; } // eg. 0x01010101
        public uint Unknown_548h { get; set; } // eg. 0x01000101
        public uint Unknown_54Ch { get; set; } // 0, 2, 4, 5, 6

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp4 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp5 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp6 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp7 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp8 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_540h = reader.ReadSingle();
            this.Unknown_544h = reader.ReadUInt32();
            this.Unknown_548h = reader.ReadUInt32();
            this.Unknown_54Ch = reader.ReadUInt32();


            switch (Unknown_540h)
            {
                case 0:
                case 0.2f:
                case 0.01f:
                case 1.0f:
                case 0.014f:
                case 0.1f:
                    break;
                default:
                    break;//more
            }
            //switch (Unknown_544h)
            //{
            //    case 0x00000100:
            //    case 0x01010101:
            //    case 0x00000001:
            //    case 0x00000101:
            //    case 0x01000101:
            //    case 0x01000100:
            //    case 0:
            //    case 0x01000001:
            //    case 0x01000000:
            //    case 0x00010100:
            //    case 0x00010000:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_548h)
            //{
            //    case 0:
            //    case 1:
            //    case 0x01000100:
            //    case 0x01000000:
            //    case 0x00000101:
            //    case 0x00000100:
            //    case 0x01000101:
            //    case 0x01000001:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_54Ch)
            //{
            //    case 0:
            //    case 6:
            //    case 5:
            //    case 4:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
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
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown540", FloatUtil.ToString(Unknown_540h));
            YptXml.ValueTag(sb, indent, "Unknown544", YptXml.UintString(Unknown_544h));
            YptXml.ValueTag(sb, indent, "Unknown548", YptXml.UintString(Unknown_548h));
            YptXml.ValueTag(sb, indent, "Unknown54C", Unknown_54Ch.ToString());
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
            if (KeyframeProp2 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty2");
                KeyframeProp2.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty2");
            }
            if (KeyframeProp3 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty3");
                KeyframeProp3.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty3");
            }
            if (KeyframeProp4 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty4");
                KeyframeProp4.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty4");
            }
            if (KeyframeProp5 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty5");
                KeyframeProp5.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty5");
            }
            if (KeyframeProp6 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty6");
                KeyframeProp6.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty6");
            }
            if (KeyframeProp7 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty7");
                KeyframeProp7.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty7");
            }
            if (KeyframeProp8 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty8");
                KeyframeProp8.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty8");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_540h = Xml.GetChildFloatAttribute(node, "Unknown540");
            Unknown_544h = Xml.GetChildUIntAttribute(node, "Unknown544");
            Unknown_548h = Xml.GetChildUIntAttribute(node, "Unknown548");
            Unknown_54Ch = Xml.GetChildUIntAttribute(node, "Unknown54C");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            KeyframeProp2 = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("KeyframeProperty2");
            if (pnode2 != null)
            {
                KeyframeProp2.ReadXml(pnode2);
            }

            KeyframeProp3 = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("KeyframeProperty3");
            if (pnode3 != null)
            {
                KeyframeProp3.ReadXml(pnode3);
            }

            KeyframeProp4 = new ParticleKeyframeProp();
            var pnode4 = node.SelectSingleNode("KeyframeProperty4");
            if (pnode4 != null)
            {
                KeyframeProp4.ReadXml(pnode4);
            }

            KeyframeProp5 = new ParticleKeyframeProp();
            var pnode5 = node.SelectSingleNode("KeyframeProperty5");
            if (pnode5 != null)
            {
                KeyframeProp5.ReadXml(pnode5);
            }

            KeyframeProp6 = new ParticleKeyframeProp();
            var pnode6 = node.SelectSingleNode("KeyframeProperty6");
            if (pnode6 != null)
            {
                KeyframeProp6.ReadXml(pnode6);
            }

            KeyframeProp7 = new ParticleKeyframeProp();
            var pnode7 = node.SelectSingleNode("KeyframeProperty7");
            if (pnode7 != null)
            {
                KeyframeProp7.ReadXml(pnode7);
            }

            KeyframeProp8 = new ParticleKeyframeProp();
            var pnode8 = node.SelectSingleNode("KeyframeProperty8");
            if (pnode8 != null)
            {
                KeyframeProp8.ReadXml(pnode8);
            }

            CreateKeyframeProps(KeyframeProp0, KeyframeProp1, KeyframeProp2, KeyframeProp3, KeyframeProp4, KeyframeProp5, KeyframeProp6, KeyframeProp7, KeyframeProp8);
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
        public uint Unknown_30h; // 0x00000000
        public float Unknown_34h { get; set; } // 0, 0.2f, 0.5f, 1.0f, 2.0f, 3.0f, 5.0f
        public float Unknown_38h { get; set; } // 0, 1.0f
        public uint Unknown_3Ch; // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadSingle();
            this.Unknown_38h = reader.ReadSingle();
            this.Unknown_3Ch = reader.ReadUInt32();

            //if (Unknown_30h != 0)
            //{ }//no hit
            //switch (Unknown_34h)
            //{
            //    case 0:
            //    case 2.0f:
            //    case 0.5f:
            //    case 3.0f:
            //    case 1.0f:
            //    case 5.0f:
            //    case 0.2f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_38h)
            //{
            //    case 0:
            //    case 1.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_3Ch != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown34", FloatUtil.ToString(Unknown_34h));
            YptXml.ValueTag(sb, indent, "Unknown38", FloatUtil.ToString(Unknown_38h));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_34h = Xml.GetChildFloatAttribute(node, "Unknown34");
            Unknown_38h = Xml.GetChildFloatAttribute(node, "Unknown38");
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourDecal : ParticleBehaviour
    {
        // ptxu_Decal
        public override long BlockLength => 0x180;

        // structure data
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public uint Unknown_150h { get; set; } // 1010, 1015, 1020, 1030, 1040, 9000, 9001, 9010
        public uint Unknown_154h; // 0x00000000
        public float Unknown_158h { get; set; } // 20.0f, 100.0f, 6.5f, ...
        public float Unknown_15Ch { get; set; } // 0, 0.001f, 0.025f, 0.1f, 0.125f, 0.25f, 0.3f
        public float Unknown_160h { get; set; } // 0, 0.5f, 1.0f
        public float Unknown_164h { get; set; } // 1.0f, 4.0f
        public float Unknown_168h { get; set; } // 0, 0.025, 0.05
        public float Unknown_16Ch { get; set; } // 0.3f, 0.8f, 1.0f, ...
        public uint Unknown_170h { get; set; } // eg. 0x01010000
        public float Unknown_174h { get; set; } = 0.3f;
        public float Unknown_178h { get; set; } = 1.0f;
        public uint Unknown_17Ch; // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt32();
            this.Unknown_154h = reader.ReadUInt32();
            this.Unknown_158h = reader.ReadSingle();
            this.Unknown_15Ch = reader.ReadSingle();
            this.Unknown_160h = reader.ReadSingle();
            this.Unknown_164h = reader.ReadSingle();
            this.Unknown_168h = reader.ReadSingle();
            this.Unknown_16Ch = reader.ReadSingle();
            this.Unknown_170h = reader.ReadUInt32();
            this.Unknown_174h = reader.ReadSingle();
            this.Unknown_178h = reader.ReadSingle();
            this.Unknown_17Ch = reader.ReadUInt32();

            //switch (Unknown_150h)
            //{
            //    case 0x000003fc: // 1020
            //    case 0x00002328: // 9000
            //    case 0x00002332: // 9010
            //    case 0x00000410: // 1040
            //    case 0x000003f2: // 1010
            //    case 0x00000406: // 1030
            //    case 0x00002329: // 9001
            //    case 0x000003f7: // 1015
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_154h != 0)
            //{ }//no hit
            switch (Unknown_158h)
            {
                case 20.0f:
                case 100.0f:
                case 6.5f:
                    break;
                default:
                    break;//more
            }
            //switch (Unknown_15Ch)
            //{
            //    case 0:
            //    case 0.25f:
            //    case 0.1f:
            //    case 0.001f:
            //    case 0.3f:
            //    case 0.025f:
            //    case 0.125f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_160h)
            //{
            //    case 1.0f:
            //    case 0:
            //    case 0.5f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_164h)
            //{
            //    case 1.0f:
            //    case 4.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_168h)
            //{
            //    case 0:
            //    case 0.05f:
            //    case 0.025f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            switch (Unknown_16Ch)
            {
                case 0.55f:
                case 1.0f:
                case 0.7f:
                case 0.3f:
                case 0.8f:
                    break;
                default:
                    break;//more
            }
            //switch (Unknown_170h)
            //{
            //    case 0x01010000:
            //    case 0x00010000:
            //    case 0x00000101:
            //    case 0x00010101:
            //    case 0x01000000:
            //    case 0:
            //    case 0x00010001:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_174h != 0.3f)
            //{ }//no hit
            //if (Unknown_178h != 1.0f)
            //{ }//no hit
            //if (Unknown_17Ch != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
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
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown150", Unknown_150h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown158", FloatUtil.ToString(Unknown_158h));
            YptXml.ValueTag(sb, indent, "Unknown15C", FloatUtil.ToString(Unknown_15Ch));
            YptXml.ValueTag(sb, indent, "Unknown160", FloatUtil.ToString(Unknown_160h));
            YptXml.ValueTag(sb, indent, "Unknown164", FloatUtil.ToString(Unknown_164h));
            YptXml.ValueTag(sb, indent, "Unknown168", FloatUtil.ToString(Unknown_168h));
            YptXml.ValueTag(sb, indent, "Unknown16C", FloatUtil.ToString(Unknown_16Ch));
            YptXml.ValueTag(sb, indent, "Unknown170", YptXml.UintString(Unknown_170h));
            YptXml.ValueTag(sb, indent, "Unknown174", FloatUtil.ToString(Unknown_174h));
            YptXml.ValueTag(sb, indent, "Unknown178", FloatUtil.ToString(Unknown_178h));
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_150h = Xml.GetChildUIntAttribute(node, "Unknown150");
            Unknown_158h = Xml.GetChildFloatAttribute(node, "Unknown158");
            Unknown_15Ch = Xml.GetChildFloatAttribute(node, "Unknown15C");
            Unknown_160h = Xml.GetChildFloatAttribute(node, "Unknown160");
            Unknown_164h = Xml.GetChildFloatAttribute(node, "Unknown164");
            Unknown_168h = Xml.GetChildFloatAttribute(node, "Unknown168");
            Unknown_16Ch = Xml.GetChildFloatAttribute(node, "Unknown16C");
            Unknown_170h = Xml.GetChildUIntAttribute(node, "Unknown170");
            Unknown_174h = Xml.GetChildFloatAttribute(node, "Unknown174");
            Unknown_178h = Xml.GetChildFloatAttribute(node, "Unknown178");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            CreateKeyframeProps(KeyframeProp0, KeyframeProp1);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ulong Unknown_150h; // 0x0000000000000000
        public uint Unknown_158h { get; set; } // 0, 1, 2, 3
        public uint Unknown_15Ch { get; set; } // 0, 1, 2, 3, 4
        public ulong Unknown_160h; // 0x0000000000000000
        public ulong Unknown_168h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_150h = reader.ReadUInt64();
            this.Unknown_158h = reader.ReadUInt32();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt64();
            this.Unknown_168h = reader.ReadUInt64();

            //if (Unknown_150h != 0)
            //{ }//no hit
            //switch (Unknown_158h)
            //{
            //    case 2:
            //    case 1:
            //    case 0:
            //    case 3:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_15Ch)
            //{
            //    case 4:
            //    case 1:
            //    case 3:
            //    case 0:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_160h != 0)
            //{ }//no hit
            //if (Unknown_168h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_168h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown158", Unknown_158h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown15C", Unknown_15Ch.ToString());
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_158h = Xml.GetChildUIntAttribute(node, "Unknown158");
            Unknown_15Ch = Xml.GetChildUIntAttribute(node, "Unknown15C");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            CreateKeyframeProps(KeyframeProp0, KeyframeProp1);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public uint Unknown_270h { get; set; } // 0, 2
        public uint Unknown_274h { get; set; } // 0, 1
        public ulong Unknown_278h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_270h = reader.ReadUInt32();
            this.Unknown_274h = reader.ReadUInt32();
            this.Unknown_278h = reader.ReadUInt64();


            //if ((Unknown_270h != 0) && (Unknown_270h != 2))
            //{ }//no hit
            //switch (Unknown_274h)
            //{
            //    case 0:
            //    case 1:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_278h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProp0);
            writer.WriteBlock(this.KeyframeProp1);
            writer.WriteBlock(this.KeyframeProp2);
            writer.WriteBlock(this.KeyframeProp3);
            writer.Write(this.Unknown_270h);
            writer.Write(this.Unknown_274h);
            writer.Write(this.Unknown_278h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown270", Unknown_270h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown274", Unknown_274h.ToString());
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
            if (KeyframeProp2 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty2");
                KeyframeProp2.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty2");
            }
            if (KeyframeProp3 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty3");
                KeyframeProp3.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty3");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_270h = Xml.GetChildUIntAttribute(node, "Unknown270");
            Unknown_274h = Xml.GetChildUIntAttribute(node, "Unknown274");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            KeyframeProp2 = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("KeyframeProperty2");
            if (pnode2 != null)
            {
                KeyframeProp2.ReadXml(pnode2);
            }

            KeyframeProp3 = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("KeyframeProperty3");
            if (pnode3 != null)
            {
                KeyframeProp3.ReadXml(pnode3);
            }

            CreateKeyframeProps(KeyframeProp0, KeyframeProp1, KeyframeProp2, KeyframeProp3);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();


        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.WriteBlock(this.KeyframeProp0);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            CreateKeyframeProps(KeyframeProp0);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public uint Unknown_C0h; // 0x00000000
        public float Unknown_C4h { get; set; } // 0, 1.0f
        public float Unknown_C8h { get; set; } // 0, 1.0f
        public uint Unknown_CCh { get; set; } = 0x7f800001; // 0x7f800001
        public uint Unknown_D0h; // 0x00000000
        public uint Unknown_D4h { get; set; } // 0, 1, 2, 3, 4, 5
        public uint Unknown_D8h { get; set; } // 1, 2, 3, 4, 6
        public float Unknown_DCh { get; set; } // 0, 0.1f, 0.2f, 0.4f, 1.0f
        public float Unknown_E0h { get; set; } // 0, 0.1f, 0.4f, 1.0f
        public float Unknown_E4h { get; set; } // 0, 0.5f
        public uint Unknown_E8h; // 0x00000000
        public uint Unknown_ECh { get; set; } // eg. 0x01000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_C0h = reader.ReadUInt32();
            this.Unknown_C4h = reader.ReadSingle();
            this.Unknown_C8h = reader.ReadSingle();
            this.Unknown_CCh = reader.ReadUInt32();
            this.Unknown_D0h = reader.ReadUInt32();
            this.Unknown_D4h = reader.ReadUInt32();
            this.Unknown_D8h = reader.ReadUInt32();
            this.Unknown_DCh = reader.ReadSingle();
            this.Unknown_E0h = reader.ReadSingle();
            this.Unknown_E4h = reader.ReadSingle();
            this.Unknown_E8h = reader.ReadUInt32();
            this.Unknown_ECh = reader.ReadUInt32();

            //if (Unknown_C0h != 0)
            //{ }//no hit
            //switch (Unknown_C4h)
            //{
            //    case 0:
            //    case 1.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_C8h)
            //{
            //    case 1.0f:
            //    case 0:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_CCh)
            //{
            //    case 0x7f800001: // NaN
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_D0h != 0)
            //{ }//no hit
            //switch (Unknown_D4h)
            //{
            //    case 1:
            //    case 2:
            //    case 0:
            //    case 3:
            //    case 5:
            //    case 4:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_D8h)
            //{
            //    case 1:
            //    case 2:
            //    case 4:
            //    case 3:
            //    case 6:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_DCh)
            //{
            //    case 0:
            //    case 0.2f:
            //    case 0.1f:
            //    case 0.4f:
            //    case 1.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_E0h)
            //{
            //    case 0:
            //    case 0.1f:
            //    case 0.4f:
            //    case 1.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_E4h)
            //{
            //    case 0:
            //    case 0.5f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_E8h != 0)
            //{ }//no hit
            //switch (Unknown_ECh)
            //{
            //    case 0x00010000:
            //    case 0x00000101:
            //    case 0:
            //    case 0x01000000:
            //        break;
            //    default:
            //        break;//no hit
            //}
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
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
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "UnknownC4", FloatUtil.ToString(Unknown_C4h));
            YptXml.ValueTag(sb, indent, "UnknownC8", FloatUtil.ToString(Unknown_C8h));
            YptXml.ValueTag(sb, indent, "UnknownD4", Unknown_D4h.ToString());
            YptXml.ValueTag(sb, indent, "UnknownD8", Unknown_D8h.ToString());
            YptXml.ValueTag(sb, indent, "UnknownDC", FloatUtil.ToString(Unknown_DCh));
            YptXml.ValueTag(sb, indent, "UnknownE0", FloatUtil.ToString(Unknown_E0h));
            YptXml.ValueTag(sb, indent, "UnknownE4", FloatUtil.ToString(Unknown_E4h));
            YptXml.ValueTag(sb, indent, "UnknownEC", YptXml.UintString(Unknown_ECh));
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_C4h = Xml.GetChildFloatAttribute(node, "UnknownC4");
            Unknown_C8h = Xml.GetChildFloatAttribute(node, "UnknownC8");
            Unknown_D4h = Xml.GetChildUIntAttribute(node, "UnknownD4");
            Unknown_D8h = Xml.GetChildUIntAttribute(node, "UnknownD8");
            Unknown_DCh = Xml.GetChildFloatAttribute(node, "UnknownDC");
            Unknown_E0h = Xml.GetChildFloatAttribute(node, "UnknownE0");
            Unknown_E4h = Xml.GetChildFloatAttribute(node, "UnknownE4");
            Unknown_ECh = Xml.GetChildUIntAttribute(node, "UnknownEC");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            CreateKeyframeProps(KeyframeProp0);
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
        public ParticleKeyframeProp KeyframeProp0 { get; set; }
        public ParticleKeyframeProp KeyframeProp1 { get; set; }
        public ParticleKeyframeProp KeyframeProp2 { get; set; }
        public ParticleKeyframeProp KeyframeProp3 { get; set; }
        public ParticleKeyframeProp KeyframeProp4 { get; set; }
        public ParticleKeyframeProp KeyframeProp5 { get; set; }
        public ParticleKeyframeProp KeyframeProp6 { get; set; }
        public float Unknown_420h { get; set; } // 1.0f, 3.0f
        public float Unknown_424h { get; set; } // 1.0f
        public uint Unknown_428h { get; set; } // 0, 1, 2
        public uint Unknown_42Ch { get; set; } // 0x00000101, 0x00010101

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.KeyframeProp0 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp1 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp2 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp3 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp4 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp5 = reader.ReadBlock<ParticleKeyframeProp>();
            this.KeyframeProp6 = reader.ReadBlock<ParticleKeyframeProp>();
            this.Unknown_420h = reader.ReadSingle();
            this.Unknown_424h = reader.ReadSingle();
            this.Unknown_428h = reader.ReadUInt32();
            this.Unknown_42Ch = reader.ReadUInt32();


            //switch (Unknown_420h)
            //{
            //    case 3.0f:
            //    case 1.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_424h != 1.0f)
            //{ }//no hit
            //switch (Unknown_428h)
            //{
            //    case 1:
            //    case 0:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_42Ch)
            //{
            //    case 0x00000101:
            //    case 0x00010101:
            //        break;
            //    default:
            //        break;//no hit
            //}
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
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
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown420", FloatUtil.ToString(Unknown_420h));
            YptXml.ValueTag(sb, indent, "Unknown424", FloatUtil.ToString(Unknown_424h));
            YptXml.ValueTag(sb, indent, "Unknown428", Unknown_428h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown42C", YptXml.UintString(Unknown_42Ch));
            if (KeyframeProp0 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty0");
                KeyframeProp0.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty0");
            }
            if (KeyframeProp1 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty1");
                KeyframeProp1.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty1");
            }
            if (KeyframeProp2 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty2");
                KeyframeProp2.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty2");
            }
            if (KeyframeProp3 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty3");
                KeyframeProp3.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty3");
            }
            if (KeyframeProp4 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty4");
                KeyframeProp4.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty4");
            }
            if (KeyframeProp5 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty5");
                KeyframeProp5.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty5");
            }
            if (KeyframeProp6 != null)
            {
                YptXml.OpenTag(sb, indent, "KeyframeProperty6");
                KeyframeProp6.WriteXml(sb, indent + 1);
                YptXml.CloseTag(sb, indent, "KeyframeProperty6");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_420h = Xml.GetChildFloatAttribute(node, "Unknown420");
            Unknown_424h = Xml.GetChildFloatAttribute(node, "Unknown424");
            Unknown_428h = Xml.GetChildUIntAttribute(node, "Unknown428");
            Unknown_42Ch = Xml.GetChildUIntAttribute(node, "Unknown42C");

            KeyframeProp0 = new ParticleKeyframeProp();
            var pnode0 = node.SelectSingleNode("KeyframeProperty0");
            if (pnode0 != null)
            {
                KeyframeProp0.ReadXml(pnode0);
            }

            KeyframeProp1 = new ParticleKeyframeProp();
            var pnode1 = node.SelectSingleNode("KeyframeProperty1");
            if (pnode1 != null)
            {
                KeyframeProp1.ReadXml(pnode1);
            }

            KeyframeProp2 = new ParticleKeyframeProp();
            var pnode2 = node.SelectSingleNode("KeyframeProperty2");
            if (pnode2 != null)
            {
                KeyframeProp2.ReadXml(pnode2);
            }

            KeyframeProp3 = new ParticleKeyframeProp();
            var pnode3 = node.SelectSingleNode("KeyframeProperty3");
            if (pnode3 != null)
            {
                KeyframeProp3.ReadXml(pnode3);
            }

            KeyframeProp4 = new ParticleKeyframeProp();
            var pnode4 = node.SelectSingleNode("KeyframeProperty4");
            if (pnode4 != null)
            {
                KeyframeProp4.ReadXml(pnode4);
            }

            KeyframeProp5 = new ParticleKeyframeProp();
            var pnode5 = node.SelectSingleNode("KeyframeProperty5");
            if (pnode5 != null)
            {
                KeyframeProp5.ReadXml(pnode5);
            }

            KeyframeProp6 = new ParticleKeyframeProp();
            var pnode6 = node.SelectSingleNode("KeyframeProperty6");
            if (pnode6 != null)
            {
                KeyframeProp6.ReadXml(pnode6);
            }

            CreateKeyframeProps(KeyframeProp0, KeyframeProp1, KeyframeProp2, KeyframeProp3, KeyframeProp4, KeyframeProp5, KeyframeProp6);
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
        public float Unknown_30h { get; set; } = 100.0f; // 100.0f
        public float Unknown_34h { get; set; } = 2.0f; // 2.0f
        public ulong Unknown_38h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_30h = reader.ReadSingle();
            this.Unknown_34h = reader.ReadSingle();
            this.Unknown_38h = reader.ReadUInt64();

            //if (Unknown_30h != 100.0f)
            //{ }//no hit
            //if (Unknown_34h != 2.0f)
            //{ }//no hit
            //if (Unknown_38h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown30", FloatUtil.ToString(Unknown_30h));
            YptXml.ValueTag(sb, indent, "Unknown34", FloatUtil.ToString(Unknown_34h));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_30h = Xml.GetChildFloatAttribute(node, "Unknown30");
            Unknown_34h = Xml.GetChildFloatAttribute(node, "Unknown34");
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourDecalPool : ParticleBehaviour
    {
        // ptxu_DecalPool
        public override long BlockLength => 0x50;

        // structure data
        public uint Unknown_30h; // 0x00000000
        public int Unknown_34h { get; set; } // 0, 1, 3, 0xffffffff
        public int Unknown_38h { get; set; } // 9000, 9001, 9003, 9007, 0xffffffff
        public float Unknown_3Ch { get; set; } // 0, 0.05f, 0.15f, 0.2f, 0.75f
        public float Unknown_40h { get; set; } // 0.5f, 1.0f, 1.5f, 1.6f, 1.75f, 2.0f
        public float Unknown_44h { get; set; } // 0.01f, 0.03f, 0.08f, 0.5f
        public ulong Unknown_48h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadInt32();
            this.Unknown_38h = reader.ReadInt32();
            this.Unknown_3Ch = reader.ReadSingle();
            this.Unknown_40h = reader.ReadSingle();
            this.Unknown_44h = reader.ReadSingle();
            this.Unknown_48h = reader.ReadUInt64();

            //if (Unknown_30h != 0)
            //{ }//no hit
            //switch (Unknown_34h)
            //{
            //    case 0:
            //    case 0xffffffff:
            //    case 1:
            //    case 3:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_38h)
            //{
            //    case 9007:
            //    case 9001:
            //    case 0xffffffff:
            //    case 9000:
            //    case 9003:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_3Ch)
            //{
            //    case 0.75f:
            //    case 0:
            //    case 0.2f:
            //    case 0.15f:
            //    case 0.05f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_40h)
            //{
            //    case 1.75f:
            //    case 1.0f:
            //    case 1.5f:
            //    case 1.6f:
            //    case 0.5f:
            //    case 2.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_44h)
            //{
            //    case 0.08f:
            //    case 0.03f:
            //    case 0.5f:
            //    case 0.01f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_48h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown34", Unknown_34h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown38", Unknown_38h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown3C", FloatUtil.ToString(Unknown_3Ch));
            YptXml.ValueTag(sb, indent, "Unknown40", FloatUtil.ToString(Unknown_40h));
            YptXml.ValueTag(sb, indent, "Unknown44", FloatUtil.ToString(Unknown_44h));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_34h = Xml.GetChildIntAttribute(node, "Unknown34");
            Unknown_38h = Xml.GetChildIntAttribute(node, "Unknown38");
            Unknown_3Ch = Xml.GetChildFloatAttribute(node, "Unknown3C");
            Unknown_40h = Xml.GetChildFloatAttribute(node, "Unknown40");
            Unknown_44h = Xml.GetChildFloatAttribute(node, "Unknown44");
        }
    }

    [TC(typeof(EXP))] public class ParticleBehaviourLiquid : ParticleBehaviour
    {
        // ptxu_Liquid
        public override long BlockLength => 0x50;

        // structure data
        public uint Unknown_30h; // 0x00000000
        public uint Unknown_34h { get; set; } = 2; // 2
        public float Unknown_38h { get; set; } = 0.75f; // 0.75f
        public float Unknown_3Ch { get; set; } = 2.0f; // 2.0f
        public float Unknown_40h { get; set; } = 0.025f; // 0.025f
        public float Unknown_44h { get; set; } = 0.2f; // 0.2f
        public float Unknown_48h { get; set; } = 0.25f; // 0.25f
        public uint Unknown_4Ch; // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadSingle();
            this.Unknown_3Ch = reader.ReadSingle();
            this.Unknown_40h = reader.ReadSingle();
            this.Unknown_44h = reader.ReadSingle();
            this.Unknown_48h = reader.ReadSingle();
            this.Unknown_4Ch = reader.ReadUInt32();

            //if (Unknown_30h != 0)
            //{ }//no hit
            //switch (Unknown_34h)
            //{
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_38h)
            //{
            //    case 0.75f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_3Ch)
            //{
            //    case 2.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_40h)
            //{
            //    case 0.025f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_44h)
            //{
            //    case 0.2f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_48h)
            //{
            //    case 0.25f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_4Ch != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown34", Unknown_34h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown38", FloatUtil.ToString(Unknown_38h));
            YptXml.ValueTag(sb, indent, "Unknown3C", FloatUtil.ToString(Unknown_3Ch));
            YptXml.ValueTag(sb, indent, "Unknown40", FloatUtil.ToString(Unknown_40h));
            YptXml.ValueTag(sb, indent, "Unknown44", FloatUtil.ToString(Unknown_44h));
            YptXml.ValueTag(sb, indent, "Unknown48", FloatUtil.ToString(Unknown_48h));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_34h = Xml.GetChildUIntAttribute(node, "Unknown34");
            Unknown_38h = Xml.GetChildFloatAttribute(node, "Unknown38");
            Unknown_3Ch = Xml.GetChildFloatAttribute(node, "Unknown3C");
            Unknown_40h = Xml.GetChildFloatAttribute(node, "Unknown40");
            Unknown_44h = Xml.GetChildFloatAttribute(node, "Unknown44");
            Unknown_48h = Xml.GetChildFloatAttribute(node, "Unknown48");
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
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Name = reader.ReadUInt32();
            this.Type = (ParticleShaderVarType)reader.ReadByte();
            this.Unknown_15h = reader.ReadByte();
            this.Unknown_16h = reader.ReadUInt16();

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //switch (Name) //parameter name
            //{
            //    case 0xea057402: // 
            //    case 0x0b3045be: // softness
            //    case 0x91bf3028: // superalpha
            //    case 0x4a8a0a28: // directionalmult
            //    case 0xf8338e85: // ambientmult
            //    case 0xbfd98c1d: // shadowamount
            //    case 0xc6fe034a: // extralightmult
            //    case 0xf03acb8c: // camerabias
            //    case 0x81634888: // camerashrink
            //    case 0xb695f45c: // normalarc
            //    case 0x403390ea: // 
            //    case 0x18ca6c12: // softnesscurve
            //    case 0x1458f27b: // softnessshadowmult
            //    case 0xa781a38b: // softnessshadowoffset
            //    case 0x77b842ed: // normalmapmult
            //    case 0x7b483bc5: // 
            //    case 0x6a1dbec3: // 
            //    case 0xba5af058: // 
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
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Name);
            writer.Write((byte)this.Type);
            writer.Write(this.Unknown_15h);
            writer.Write(this.Unknown_16h);
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

    [TC(typeof(EXP))] public class ParticleShaderVarVector : ParticleShaderVar
    {
        // ptxShaderVarVector
        public override long BlockLength => 0x40;

        // structure data
        public uint Unknown_18h { get; set; } // 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 18, 19, 21, 22, 32       //shader var index..?
        public uint Unknown_1Ch; // 0x00000000
        public uint Unknown_20h; // 0x00000000
        public uint Unknown_24h; // 0x00000000
        public uint Unknown_28h; // 0x00000000
        public uint Unknown_2Ch; // 0x00000000
        public float Unknown_30h { get; set; } // 0, 0.1f, 0.2f, ..., 1.0f, 2.0f, ...
        public float Unknown_34h { get; set; } // 0, 0.5f, 0.996f, 1.0f
        public float Unknown_38h { get; set; } // 0, 0.1f, 0.2f, ..., 1.0f, ...
        public uint Unknown_3Ch; // 0x00000000

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
            this.Unknown_30h = reader.ReadSingle();
            this.Unknown_34h = reader.ReadSingle();
            this.Unknown_38h = reader.ReadSingle();
            this.Unknown_3Ch = reader.ReadUInt32();

            //switch (Unknown_18h) //shader var index..?
            //{
            //    case 32:
            //    case 22:
            //    case 21:
            //    case 19:
            //    case 18:
            //    case 14:
            //    case 13:
            //    case 12:
            //    case 11:
            //    case 10:
            //    case 9:
            //    case 8:
            //    case 7:
            //    case 6:
            //    case 5:
            //    case 4:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_1Ch != 0)
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_24h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
            //if (Unknown_2Ch != 0)
            //{ }//no hit
            switch (Unknown_30h)
            {
                case 1.0f:
                case 0.1f:
                case 0.2f:
                case 0.02f:
                case 0.01f:
                case 2.0f:
                case 0.4f:
                case 0:
                    break;
                default:
                    break;//and more..
            }
            //switch (Unknown_34h)
            //{
            //    case 0:
            //    case 1.0f:
            //    case 0.5f:
            //    case 0.996f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            switch (Unknown_38h)
            {
                case 0:
                case 1.0f:
                case 0.5f:
                case 0.1f:
                case 0.2f:
                case 0.7f:
                    break;
                default:
                    break;//more
            }
            //if (Unknown_3Ch != 0)
            //{ }//no hit
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
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown18", Unknown_18h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown30", FloatUtil.ToString(Unknown_30h));
            YptXml.ValueTag(sb, indent, "Unknown34", FloatUtil.ToString(Unknown_34h));
            YptXml.ValueTag(sb, indent, "Unknown38", FloatUtil.ToString(Unknown_38h));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_18h = Xml.GetChildUIntAttribute(node, "Unknown18");
            Unknown_30h = Xml.GetChildFloatAttribute(node, "Unknown30");
            Unknown_34h = Xml.GetChildFloatAttribute(node, "Unknown34");
            Unknown_38h = Xml.GetChildFloatAttribute(node, "Unknown38");
        }
    }

    [TC(typeof(EXP))] public class ParticleShaderVarTexture : ParticleShaderVar
    {
        // ptxShaderVarTexture
        public override long BlockLength => 0x40;

        // structure data
        public uint Unknown_18h { get; set; } // 3, 4, 6, 7       //shader var index..?
        public uint Unknown_1Ch; // 0x00000000
        public uint Unknown_20h; // 0x00000000
        public uint Unknown_24h; // 0x00000000
        public ulong TexturePointer { get; set; }
        public ulong TextureNamePointer { get; set; }
        public MetaHash TextureNameHash { get; set; }
        public uint Unknown_3Ch { get; set; } // 0, 1

        // reference data
        public Texture Texture { get; set; }
        public string_r TextureName { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.TexturePointer = reader.ReadUInt64();
            this.TextureNamePointer = reader.ReadUInt64();
            this.TextureNameHash = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            this.Texture = reader.ReadBlockAt<Texture>(this.TexturePointer);
            this.TextureName = reader.ReadBlockAt<string_r>(this.TextureNamePointer);


            //switch (Unknown_18h) //shader var index..?
            //{
            //    case 7:
            //    case 6:
            //    case 4:
            //    case 3:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_1Ch != 0)
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_24h != 0)
            //{ }//no hit
            //switch (TextureNameHash)
            //{
            //    case 0:
            //    case 0xda1c24ad: // ptfx_gloop
            //    case 0xc4e50054: // ptfx_water_splashes_sheet
            //        break;
            //    default:
            //        break;//and more...
            //}
            //if (TextureNameHash != JenkHash.GenHash(TextureName?.ToString() ?? ""))
            //{ }//no hit
            //switch (Unknown_3Ch)
            //{
            //    case 0:
            //    case 1:
            //        break;
            //    default:
            //        break;//no hit
            //}
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.TexturePointer = (ulong)(this.Texture != null ? this.Texture.FilePosition : 0);
            this.TextureNamePointer = (ulong)(this.TextureName != null ? this.TextureName.FilePosition : 0);

            // write structure data
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.TexturePointer);
            writer.Write(this.TextureNamePointer);
            writer.Write(this.TextureNameHash);
            writer.Write(this.Unknown_3Ch);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown18", Unknown_18h.ToString());
            YptXml.ValueTag(sb, indent, "Unknown3C", Unknown_3Ch.ToString());
            YptXml.StringTag(sb, indent, "TextureName", YptXml.XmlEscape(TextureName?.Value ?? ""));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_18h = Xml.GetChildUIntAttribute(node, "Unknown18");
            Unknown_3Ch = Xml.GetChildUIntAttribute(node, "Unknown3C");
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
        public uint Unknown_18h { get; set; } // 9, 14, 15, 16, 17, 20, 23, 31       //shader var index..?
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

            //switch (Unknown_18h) //shader var index..?
            //{
            //    case 31:
            //    case 23:
            //    case 20:
            //    case 17:
            //    case 16:
            //    case 15:
            //    case 14:
            //    case 9:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_1Ch != 1)
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_38h != 0)
            //{ }//no hit
            //if (Unknown_40h != 0)
            //{ }//no hit
            //if (Unknown_48h != 0)
            //{ }//no hit

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
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YptXml.ValueTag(sb, indent, "Unknown18", Unknown_18h.ToString());
            YptXml.WriteItemArray(sb, Items?.data_items, indent, "Items");
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_18h = Xml.GetChildUIntAttribute(node, "Unknown18");
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
            this.Unknown_0h = reader.ReadSingle();
            this.Unknown_4h = reader.ReadSingle();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadSingle();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt64();

            switch (Unknown_0h)
            {
                case 0:
                case 0.2f:
                case 1.0f:
                case 0.149759f:
                case 0.63285f:
                    break;
                default:
                    break;//and more..
            }
            switch (Unknown_4h)
            {
                case 0:
                case 5.0f:
                case 1.25f:
                case 6.67739534f:
                case 2.07000327f:
                    break;
                default:
                    break;//and more..
            }
            //if (Unknown_8h != 0)
            //{ }//no hit
            switch (Unknown_10h)
            {
                case 20.0f:
                case 1.0f:
                case 0.2f:
                case 0.8f:
                case 1.080267f:
                case 0:
                    break;
                default:
                    break;//and more..
            }
            //if (Unknown_14h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
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
