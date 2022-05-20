using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Xml;

//GTAV FXC file reader by dexyfex.
// use this however you want. some credit would be nice.

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class FxcFile : PackedFile
    {
        public string Name { get; set; }
        public RpfFileEntry FileEntry { get; set; }
        public uint Hash { get; set; }

        public VertexType VertexType { get; set; }
        public FxcPresetParam[] PresetParams { get; set; }
        public FxcShaderGroup[] ShaderGroups { get; set; }
        public FxcCBuffer[] CBuffers1 { get; set; }
        public FxcVariable[] Variables1 { get; set; }
        public FxcCBuffer[] CBuffers2 { get; set; }
        public FxcVariable[] Variables2 { get; set; }
        public FxcTechnique[] Techniques { get; set; }


        public FxcShader[] Shaders { get; set; }
        public FxcShader[] VertexShaders { get; set; }
        public FxcShader[] PixelShaders { get; set; }
        public FxcShader[] ComputeShaders { get; set; }
        public FxcShader[] DomainShaders { get; set; }
        public FxcShader[] GeometryShaders { get; set; }
        public FxcShader[] HullShaders { get; set; }

        public Dictionary<uint, FxcCBuffer> CBufferDict { get; set; }
        public FxcVariable[] GlobalVariables { get; set; }






        public void Load(byte[] data, RpfFileEntry entry)
        {
            FileEntry = entry;
            Name = entry.Name;
            Hash = entry.ShortNameHash;

            MemoryStream ms = new MemoryStream(data);
            BinaryReader br = new BinaryReader(ms);

            uint magic_rgxe = br.ReadUInt32();
            if (magic_rgxe != 1702389618) //"rgxe"
            {
                return;
            }

            VertexType = (VertexType)br.ReadUInt32();

            byte ppCount = br.ReadByte();
            if (ppCount > 0)
            {
                var pparams = new List<FxcPresetParam>();
                for (int i = 0; i < ppCount; i++)
                {
                    FxcPresetParam pparam = new FxcPresetParam();
                    pparam.Read(br);
                    pparams.Add(pparam);
                }
                PresetParams = pparams.ToArray();
            }


            var groups = new List<FxcShaderGroup>();
            var shaders = new List<FxcShader>();
            for (int i = 0; i < 6; i++)
            {

                FxcShaderGroup group = new FxcShaderGroup();
                group.Read(br, i);
                groups.Add(group);
                if (group.Shaders != null)
                {
                    shaders.AddRange(group.Shaders);
                }
            }

            ShaderGroups = groups.ToArray();
            Shaders = shaders.ToArray();
            VertexShaders = groups[0].Shaders;
            PixelShaders = groups[1].Shaders;
            ComputeShaders = groups[2].Shaders;
            DomainShaders = groups[3].Shaders;
            GeometryShaders = groups[4].Shaders;
            HullShaders = groups[5].Shaders;



            List<FxcVariable> globalVars = new List<FxcVariable>();
            CBufferDict = new Dictionary<uint, FxcCBuffer>();


            byte cbCount1 = br.ReadByte();
            if (cbCount1 > 0)
            {
                var cbuffers1 = new List<FxcCBuffer>();
                for (int i = 0; i < cbCount1; i++) //cbuffers? includes?
                {
                    FxcCBuffer cbuf = new FxcCBuffer();
                    cbuf.Read(br);
                    cbuffers1.Add(cbuf);
                    CBufferDict[cbuf.NameHash] = cbuf;
                }
                CBuffers1 = cbuffers1.ToArray();
            }

            byte varCount1 = br.ReadByte();
            if (varCount1 > 0)
            {
                var vars1 = new List<FxcVariable>(); //cbuffer contents/vars
                for (int i = 0; i < varCount1; i++)
                {
                    FxcVariable vari = new FxcVariable();
                    vari.Read(br);
                    vars1.Add(vari);
                    if (CBufferDict.TryGetValue(vari.CBufferName, out var cbtmp))
                    {
                        cbtmp.AddVariable(vari);
                    }
                    else
                    {
                        globalVars.Add(vari);
                    }
                }
                Variables1 = vars1.ToArray();
            }

            byte cbCount2 = br.ReadByte(); //0,1, +?
            if (cbCount2 > 0)
            {
                var cbuffers2 = new List<FxcCBuffer>(); //more cbuffers..
                for (int i = 0; i < cbCount2; i++)
                {
                    FxcCBuffer cbuf = new FxcCBuffer();
                    cbuf.Read(br);
                    cbuffers2.Add(cbuf);
                    CBufferDict[cbuf.NameHash] = cbuf;
                }
                CBuffers2 = cbuffers2.ToArray();
            }

            byte varCount2 = br.ReadByte();
            if (varCount2 > 0)
            {
                var vars2 = new List<FxcVariable>();
                for (int i = 0; i < varCount2; i++) //textures/samplers
                {
                    FxcVariable vari = new FxcVariable();
                    vari.Read(br);
                    vars2.Add(vari);
                    if (CBufferDict.TryGetValue(vari.CBufferName, out var cbtmp))
                    {
                        cbtmp.AddVariable(vari);
                    }
                    else
                    {
                        globalVars.Add(vari);
                    }
                }
                Variables2 = vars2.ToArray();
            }

            byte techCount = br.ReadByte();
            if (techCount > 0)
            {
                var techniques = new List<FxcTechnique>();
                for (int i = 0; i < techCount; i++)
                {
                    FxcTechnique tech = new FxcTechnique();
                    tech.Read(br);
                    tech.GetNamesFromIndices(this);
                    techniques.Add(tech);
                }
                Techniques = techniques.ToArray();
            }


            foreach (var cbuf in CBufferDict.Values)
            {
                cbuf.ConsolidateVariables();
            }
            GlobalVariables = globalVars.ToArray();


            if (ms.Position != ms.Length)
            { }//no hit


            ms.Dispose();
        }

        public byte[] Save()
        {
            var s = new MemoryStream();
            var w = new BinaryWriter(s);

            w.Write((uint)1702389618); //"rgxe"
            w.Write((uint)VertexType);

            var ppCount = (byte)(PresetParams?.Length ?? 0);
            w.Write(ppCount);
            for (int i = 0; i < ppCount; i++)
            {
                PresetParams[i].Write(w);
            }


            ShaderGroups[0].Shaders = VertexShaders;
            ShaderGroups[1].Shaders = PixelShaders;
            ShaderGroups[2].Shaders = ComputeShaders;
            ShaderGroups[3].Shaders = DomainShaders;
            ShaderGroups[4].Shaders = GeometryShaders;
            ShaderGroups[5].Shaders = HullShaders;

            for (int i = 0; i < 6; i++)
            {
                var group = ShaderGroups[i];
                group.Write(w, i);
            }


            var cbCount1 = (byte)(CBuffers1?.Length ?? 0);
            w.Write(cbCount1);
            for (int i = 0; i < cbCount1; i++)
            {
                CBuffers1[i].Write(w);
            }

            var varCount1 = (byte)(Variables1?.Length ?? 0);
            w.Write(varCount1);
            for (int i = 0; i < varCount1; i++)
            {
                Variables1[i].Write(w);
            }

            var cbCount2 = (byte)(CBuffers2?.Length ?? 0);
            w.Write(cbCount2);
            for (int i = 0; i < cbCount2; i++)
            {
                CBuffers2[i].Write(w);
            }

            var varCount2 = (byte)(Variables2?.Length ?? 0);
            w.Write(varCount2);
            for (int i = 0; i < varCount2; i++)
            {
                Variables2[i].Write(w);
            }

            var techCount = (byte)(Techniques?.Length ?? 0);
            w.Write(techCount);
            for (int i = 0; i < techCount; i++)
            {
                Techniques[i].Write(w);
            }


            var buf = new byte[s.Length];
            s.Position = 0;
            s.Read(buf, 0, buf.Length);
            return buf;
        }


        public void WriteXml(StringBuilder sb, int indent, string csofolder)
        {
            if ((ShaderGroups == null) || (ShaderGroups.Length < 6)) return;

            FxcXml.StringTag(sb, indent, "VertexType", VertexType.ToString());

            if (PresetParams != null)
            {
                FxcXml.WriteItemArray(sb, PresetParams, indent, "PresetParams");
            }

            var ci = indent + 1;
            var gi = ci + 1;
            FxcXml.OpenTag(sb, indent, "Shaders");
            for (int i = 0; i < 6; i++)
            {
                var group = ShaderGroups[i];
                var typen = group.Type.ToString() + "s";
                var tagn = typen;
                if (group.OffsetBy1)
                {
                    tagn += " OffsetBy1=\"" + group.OffsetBy1.ToString() + "\"";
                }
                if ((group.Shaders?.Length ?? 0) > 0)
                {
                    FxcXml.OpenTag(sb, ci, tagn);
                    group.WriteXml(sb, gi, csofolder);
                    FxcXml.CloseTag(sb, ci, typen);
                }
                else //if (group.OffsetBy1)
                {
                    FxcXml.SelfClosingTag(sb, ci, tagn);
                }
            }
            FxcXml.CloseTag(sb, indent, "Shaders");

            if (CBuffers1 != null)
            {
                FxcXml.WriteItemArray(sb, CBuffers1, indent, "CBuffers1");
            }
            if (Variables1 != null)
            {
                FxcXml.WriteItemArray(sb, Variables1, indent, "Variables1");
            }
            if (CBuffers2 != null)
            {
                FxcXml.WriteItemArray(sb, CBuffers2, indent, "CBuffers2");
            }
            if (Variables2 != null)
            {
                FxcXml.WriteItemArray(sb, Variables2, indent, "Variables2");
            }
            if (Techniques != null)
            {
                FxcXml.WriteItemArray(sb, Techniques, indent, "Techniques");
            }
        }
        public void ReadXml(XmlNode node, string csofolder)
        {
            VertexType = Xml.GetChildEnumInnerText<VertexType>(node, "VertexType");
            PresetParams = XmlMeta.ReadItemArray<FxcPresetParam>(node, "PresetParams");

            var snode = node.SelectSingleNode("Shaders");
            if (snode != null)
            {
                var shaders = new List<FxcShader>();
                FxcShaderGroup getShaderGroup(FxcShaderType type)
                {
                    var gname = type.ToString() + "s";
                    var gnode = snode.SelectSingleNode(gname);
                    if (gnode == null) return null;
                    var group = new FxcShaderGroup();
                    group.Type = type;
                    group.OffsetBy1 = Xml.GetBoolAttribute(gnode, "OffsetBy1");
                    group.ReadXml(gnode, csofolder);
                    return group;
                }

                var groups = new FxcShaderGroup[6];
                for (int i = 0; i < 6; i++)
                {
                    var group = getShaderGroup((FxcShaderType)i);
                    groups[i] = group;
                    if (group.Shaders != null)
                    {
                        shaders.AddRange(group.Shaders);
                    }
                }

                ShaderGroups = groups;
                Shaders = shaders.ToArray();
                VertexShaders = groups[0].Shaders;
                PixelShaders = groups[1].Shaders;
                ComputeShaders = groups[2].Shaders;
                DomainShaders = groups[3].Shaders;
                GeometryShaders = groups[4].Shaders;
                HullShaders = groups[5].Shaders;
            }

            CBuffers1 = XmlMeta.ReadItemArray<FxcCBuffer>(node, "CBuffers1");
            Variables1 = XmlMeta.ReadItemArray<FxcVariable>(node, "Variables1");
            CBuffers2 = XmlMeta.ReadItemArray<FxcCBuffer>(node, "CBuffers2");
            Variables2 = XmlMeta.ReadItemArray<FxcVariable>(node, "Variables2");
            Techniques = XmlMeta.ReadItemArray<FxcTechnique>(node, "Techniques");

            if (Techniques != null)
            {
                foreach (var t in Techniques)
                {
                    t.GetIndicesFromNames(this);
                }
            }
        }



        public string GetMetaString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name: " + Name);
            sb.AppendLine("Hash: " + Hash.ToString());
            sb.AppendLine("Vertex type: " + ((uint)VertexType).ToString());
            sb.AppendLine();

            if (PresetParams != null)
            {
                sb.AppendLine("Header");
                foreach (var ext in PresetParams)
                {
                    sb.AppendLine("  " + ext.ToString());
                }
                sb.AppendLine();
            }

            if (ShaderGroups != null)
            {
                sb.AppendLine("Sections");
                foreach (var chunk in ShaderGroups)
                {
                    sb.AppendLine("  " + chunk.ToString());
                }
                sb.AppendLine();
            }

            if (Shaders != null)
            {
                sb.AppendLine("Shaders (" + Shaders.Length.ToString() + ")");
                foreach (var shader in Shaders)
                {
                    sb.AppendLine("  " + shader.Name);
                    if ((shader.Variables != null) && (shader.Variables.Length > 0))
                    {
                        sb.AppendLine("    (Params)");
                        foreach (var parm in shader.Variables)
                        {
                            sb.AppendLine("      " + parm);
                        }
                    }
                    if ((shader.Buffers != null) && (shader.Buffers.Length > 0))
                    {
                        sb.AppendLine("    (Buffers)");
                        foreach (var ext in shader.Buffers)
                        {
                            sb.AppendLine("      " + ext.ToString());
                        }
                    }
                }
                sb.AppendLine();
            }

            if (CBuffers1 != null)
            {
                sb.AppendLine("CBuffers 1 (" + CBuffers1.Length.ToString() + ")");
                foreach (var p in CBuffers1)
                {
                    sb.AppendLine("  " + p.ToString());
                }
                sb.AppendLine();
            }

            if (Variables1 != null)
            {
                sb.AppendLine("Variables 1 (" + Variables1.Length.ToString() + ")");
                foreach (var p in Variables1)
                {
                    sb.AppendLine("  " + p.ToString());
                    if ((p.Params != null) && (p.Params.Length > 0))
                    {
                        sb.AppendLine("    (Params)");
                        foreach (var c in p.Params)
                        {
                            sb.AppendLine("      " + c.ToString());
                        }
                    }
                    if ((p.ValuesF != null) && (p.ValuesF.Length > 0))
                    {
                        sb.AppendLine("    (Values)");
                        foreach (var d in p.ValuesF)
                        {
                            sb.AppendLine("      " + d.ToString());
                        }
                    }
                }
                sb.AppendLine();
            }

            if (CBuffers2 != null)
            {
                sb.AppendLine("CBuffers 2 (" + CBuffers2.Length.ToString() + ")");
                foreach (var p in CBuffers2)
                {
                    sb.AppendLine("  " + p.ToString());
                }
                sb.AppendLine();
            }

            if (Variables2 != null)
            {
                sb.AppendLine("Variables 2 (" + Variables2.Length.ToString() + ")");
                foreach (var p in Variables2)
                {
                    sb.AppendLine("  " + p.ToString());
                    if ((p.Params != null) && (p.Params.Length > 0))
                    {
                        sb.AppendLine("    (Params)");
                        foreach (var c in p.Params)
                        {
                            sb.AppendLine("      " + c.ToString());
                        }
                    }
                    if ((p.ValuesF != null) && (p.ValuesF.Length > 0))
                    {
                        sb.AppendLine("    (Values)");
                        foreach (var d in p.ValuesF)
                        {
                            sb.AppendLine("      " + d.ToString());
                        }
                    }
                }
                sb.AppendLine();
            }

            if (Techniques != null)
            {
                sb.AppendLine("Techniques (" + Techniques.Length.ToString() + ")");
                foreach (var t in Techniques)
                {
                    sb.AppendLine("  " + t.Name);
                    if ((t.PassCount > 0) && (t.Passes != null))
                    {
                        sb.AppendLine("    (Passes)");
                        foreach (var p in t.Passes)
                        {
                            sb.AppendLine("      " + p.ToString());
                            if ((p.ParamCount > 0) && (p.Params != null))
                            {
                                //sb.AppendLine("    Params");
                                foreach (var v in p.Params)
                                {
                                    sb.AppendLine("         " + v.ToString());
                                }
                            }
                        }
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }


        public FxcShader GetVS(int id)
        {
            int i = id - 1;
            if ((i < 0) || (VertexShaders == null) || (i >= VertexShaders.Length))
            {
                return null;
            }
            return VertexShaders[i];
        }
        public FxcShader GetPS(int id)
        {
            int i = id - 1;
            if ((i < 0) || (PixelShaders == null) || (i >= PixelShaders.Length))
            {
                return null;
            }
            return PixelShaders[i];
        }
        public FxcShader GetCS(int id)
        {
            int i = id - 1;
            if ((i < 0) || (ComputeShaders == null) || (i >= ComputeShaders.Length))
            {
                return null;
            }
            return ComputeShaders[i];
        }
        public FxcShader GetDS(int id)
        {
            int i = id - 1;
            if ((i < 0) || (DomainShaders == null) || (i >= DomainShaders.Length))
            {
                return null;
            }
            return DomainShaders[i];
        }
        public FxcShader GetGS(int id)
        {
            int i = id - 1;
            if ((i < 0) || (GeometryShaders == null) || (i >= GeometryShaders.Length))
            {
                return null;
            }
            return GeometryShaders[i];
        }
        public FxcShader GetHS(int id)
        {
            int i = id - 1;
            if ((i < 0) || (HullShaders == null) || (i >= HullShaders.Length))
            {
                return null;
            }
            return HullShaders[i];
        }

        public byte GetVSID(string name)
        {
            if (VertexShaders == null) return 0;
            for (int i = 0; i < VertexShaders.Length; i++)
            {
                if (VertexShaders[i].Name == name)
                {
                    return (byte)(i + 1);
                }
            }
            return 0;
        }
        public byte GetPSID(string name)
        {
            if (PixelShaders == null) return 0;
            for (int i = 0; i < PixelShaders.Length; i++)
            {
                if (PixelShaders[i].Name == name)
                {
                    return (byte)(i + 1);
                }
            }
            return 0;
        }
        public byte GetCSID(string name)
        {
            if (ComputeShaders == null) return 0;
            for (int i = 0; i < ComputeShaders.Length; i++)
            {
                if (ComputeShaders[i].Name == name)
                {
                    return (byte)(i + 1);
                }
            }
            return 0;
        }
        public byte GetDSID(string name)
        {
            if (DomainShaders == null) return 0;
            for (int i = 0; i < DomainShaders.Length; i++)
            {
                if (DomainShaders[i].Name == name)
                {
                    return (byte)(i + 1);
                }
            }
            return 0;
        }
        public byte GetGSID(string name)
        {
            if (GeometryShaders == null) return 0;
            for (int i = 0; i < GeometryShaders.Length; i++)
            {
                if (GeometryShaders[i].Name == name)
                {
                    return (byte)(i + 1);
                }
            }
            return 0;
        }
        public byte GetHSID(string name)
        {
            if (HullShaders == null) return 0;
            for (int i = 0; i < HullShaders.Length; i++)
            {
                if (HullShaders[i].Name == name)
                {
                    return (byte)(i + 1);
                }
            }
            return 0;
        }



        public static string ReadString(BinaryReader br)
        {
            byte sl = br.ReadByte();
            if (sl == 0) return string.Empty;
            byte[] ba = br.ReadBytes(sl);
            return (sl > 1) ? Encoding.ASCII.GetString(ba, 0, sl - 1) : string.Empty;
        }
        public static string[] ReadStringArray(BinaryReader br)
        {
            string[] r = null;

            byte sc = br.ReadByte();
            if (sc > 0)
            {
                StringBuilder sb = new StringBuilder();
                r = new string[sc];
                for (int i = 0; i < sc; i++)
                {
                    sb.Clear();
                    byte sl = br.ReadByte();
                    var ba = br.ReadBytes(sl);
                    r[i] = (sl > 1) ? Encoding.ASCII.GetString(ba, 0, sl - 1) : string.Empty;
                }
            }

            return r;
        }

        public static void WriteString(BinaryWriter bw, string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                bw.Write((byte)0);
            }
            else
            {
                if (s.Length > 255)
                { s = s.Substring(0, 255); }
                var bytes = Encoding.ASCII.GetBytes(s);
                bw.Write((byte)(bytes.Length + 1));
                bw.Write(bytes);
                bw.Write((byte)0);
            }
        }
        public static void WriteStringArray(BinaryWriter bw, string[] a)
        {
            byte sc = (byte)(a?.Length ?? 0);
            bw.Write(sc);
            for (int i = 0; i < sc; i++)
            {
                var s = a[i];
                if (s.Length > 255)
                { s = s.Substring(0, 255); }
                var bytes = Encoding.ASCII.GetBytes(s);
                bw.Write((byte)(bytes.Length + 1));
                bw.Write(bytes);
                bw.Write((byte)0);
            }
        }

    }


    [TC(typeof(EXP))] public class FxcPresetParam : IMetaXmlItem
    {
        public string Name { get; set; } //eg. __rage_drawbucket, ExposeAlphaMap
        public byte Unused0 { get; set; } //always 0  - possibly type identifier?
        public uint Value { get; set; }

        public void Read(BinaryReader br)
        {
            Name = FxcFile.ReadString(br);
            Unused0 = br.ReadByte(); //always 0
            Value = br.ReadUInt32(); //eg 1, 2
        }
        public void Write(BinaryWriter bw)
        {
            FxcFile.WriteString(bw, Name);
            bw.Write(Unused0);
            bw.Write(Value);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            FxcXml.StringTag(sb, indent, "Name", FxcXml.XmlEscape(Name));
            FxcXml.ValueTag(sb, indent, "Value", Value.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            Value = Xml.GetChildUIntAttribute(node, "Value");
        }

        public override string ToString()
        {
            return Name + ": " + Value.ToString();
        }
    }

    public enum FxcShaderType
    {
        VertexShader = 0,
        PixelShader = 1,
        ComputeShader = 2,
        DomainShader = 3,
        GeometryShader = 4,
        HullShader = 5,
    }

    [TC(typeof(EXP))] public class FxcShaderGroup
    {
        public FxcShaderType Type { get; set; } //index in the fxc file
        public byte ShaderCount { get; set; } //number of shaders in the section +1 (why +1 though? no idea)
        public bool OffsetBy1 { get; set; }//don't know why, sometimes hull shaders get offset by 1 byte at the start
        public string Name { get; set; } //"NULL"
        public byte Unk1Byte { get; set; } //0
        public byte Unk2Byte { get; set; } //0
        public uint Unk3Uint { get; set; } //0
        public FxcShader[] Shaders { get; set; }

        public void Read(BinaryReader br, int gindex)
        {
            Type = (FxcShaderType)gindex;
            ShaderCount = br.ReadByte();
            if (ShaderCount == 0)
            {
                ShaderCount = br.ReadByte(); //sometimes a byte skip, but only happens for hull shaders (gindex=5)
                OffsetBy1 = true;
            }
            Name = FxcFile.ReadString(br); //always "NULL"
            Unk1Byte = br.ReadByte();
            Unk2Byte = br.ReadByte();
            Unk3Uint = br.ReadUInt32();

            if (ShaderCount > 1)
            {
                Shaders = new FxcShader[ShaderCount-1];
                for (int i = 1; i < ShaderCount; i++)
                {
                    FxcShader shader = new FxcShader();
                    shader.Read(br, gindex);
                    Shaders[i-1] = shader;
                }
            }

        }
        public void Write(BinaryWriter bw, int gindex)
        {
            ShaderCount = (byte)((Shaders?.Length ?? 0) + 1);

            if (OffsetBy1)
            {
                bw.Write((byte)0);
            }
            bw.Write(ShaderCount);
            FxcFile.WriteString(bw, Name);
            bw.Write(Unk1Byte);
            bw.Write(Unk2Byte);
            bw.Write(Unk3Uint);
            for (int i = 1; i < ShaderCount; i++)
            {
                Shaders[i-1].Write(bw, gindex);
            }
        }

        public void WriteXml(StringBuilder sb, int indent, string csofolder)
        {
            var sc = Shaders?.Length ?? 0;
            var ci = indent + 1;
            var si = ci + 1;
            for (int i = 0; i < sc; i++)
            {
                var typen = "Item";// Index=\"" + i.ToString() + "\"";
                FxcXml.OpenTag(sb, ci, typen);
                Shaders[i].WriteXml(sb, si, csofolder);
                FxcXml.CloseTag(sb, ci, "Item");
            }
        }
        public void ReadXml(XmlNode node, string csofolder)
        {
            Name = "NULL";
            var inodes = node.SelectNodes("Item");
            if (inodes?.Count > 0)
            {
                var slist = new List<FxcShader>();
                foreach (XmlNode inode in inodes)
                {
                    var s = new FxcShader();
                    s.Type = Type;
                    s.ReadXml(inode, csofolder);
                    slist.Add(s);
                }
                Shaders = slist.ToArray();
            }
        }

        public override string ToString()
        {
            return Name + ": " + Type.ToString() + ": " + ShaderCount.ToString() + ": " + Unk1Byte.ToString() + ": " + Unk2Byte.ToString() + ": " + Unk3Uint.ToString();
        }
    }

    [TC(typeof(EXP))] public class FxcShader
    {
        public FxcShaderType Type { get; set; }
        public long Offset { get; set; } //just for informational purposes
        public bool OffsetBy1 { get; set; }//don't know why, sometimes geometry shaders get offset by 1 byte here
        public string Name { get; set; }
        public string[] Variables { get; set; }
        public FxcShaderBufferRef[] Buffers { get; set; }//CBuffers
        public byte VersionMajor { get; set; }
        public byte VersionMinor { get; set; }
        public string VersionString { get; set; }
        public byte[] ByteCode { get; set; }
        //public ShaderBytecode ByteCodeObj { get; set; }
        //public ShaderProfile ShaderProfile { get; set; }
        public string Disassembly { get; set; }//see FxcParser
        public string LastError { get; set; } //see FxcParser

        public void Read(BinaryReader br, int gindex)
        {
            Type = (FxcShaderType)gindex;
            Offset = br.BaseStream.Position;

            Name = FxcFile.ReadString(br);
            if (Name.Length == 0)
            {
                Name = FxcFile.ReadString(br); //why  (seems to be GS only)
                OffsetBy1 = true;
            }

            Variables = FxcFile.ReadStringArray(br);

            byte bufferCount = br.ReadByte();
            if (bufferCount > 0)
            {
                var buffers = new List<FxcShaderBufferRef>();
                for (int e = 0; e < bufferCount; e++)
                {
                    FxcShaderBufferRef ext = new FxcShaderBufferRef();
                    ext.Read(br);
                    buffers.Add(ext);
                }
                Buffers = buffers.ToArray();
            }

            if (Type == FxcShaderType.GeometryShader) //GS seems to be slightly different format??
            {
                var exbyte = br.ReadByte(); //not sure what this is used for...
                if (exbyte != 0)
                { }//no hit
            }


            uint datalength = br.ReadUInt32();
            if (datalength > 0)
            {
                uint magic_dxbc = br.ReadUInt32();
                if (magic_dxbc != 1128421444) //"DXBC" - directx bytecode header
                {
                    //LastError += "Unexpected data found at DXBC header...\r\n";
                    return; //false; //didn't find the DXBC header... abort! (no hit here)
                }
                br.BaseStream.Position -= 4; //wind back because dx needs that header

                ByteCode = br.ReadBytes((int)datalength);

                switch (Type)
                {
                    case FxcShaderType.VertexShader:
                    case FxcShaderType.PixelShader:
                    case FxcShaderType.GeometryShader: //only for VS,PS,GS
                        VersionMajor = br.ReadByte();//4,5 //appears to be shader model version
                        VersionMinor = br.ReadByte();
                        break;
                }

                #region disassembly not done here - see FxcParser.cs
                //try
                //{
                //    ByteCodeObj = new ShaderBytecode(ByteCode);
                //    ShaderProfile = ByteCodeObj.GetVersion();
                //    switch (ShaderProfile.Version)
                //    {
                //        case ShaderVersion.VertexShader:
                //        case ShaderVersion.PixelShader:
                //        case ShaderVersion.GeometryShader:
                //            VersionMajor = br.ReadByte();//4,5 //appears to be shader model version
                //            VersionMinor = br.ReadByte(); //perhaps shader minor version
                //            break;
                //        default:
                //            VersionMajor = (byte)ShaderProfile.Major;
                //            VersionMinor = (byte)ShaderProfile.Minor;
                //            break;
                //    }
                //    //do disassembly last, so any errors won't cause the file read to break
                //    Disassembly = ByteCodeObj.Disassemble();
                //}
                //catch (Exception ex)
                //{
                //    LastError += ex.ToString() + "\r\n";
                //    return false;
                //}
                #endregion
            }
        }
        public void Write(BinaryWriter bw, int gindex)
        {
            Type = (FxcShaderType)gindex; //not sure if this should be here, shouldn't be necessary

            if (OffsetBy1)
            {
                bw.Write((byte)0);
            }
            FxcFile.WriteString(bw, Name);
            FxcFile.WriteStringArray(bw, Variables);

            var bufferCount = (byte)(Buffers?.Length ?? 0);
            bw.Write(bufferCount);
            for (int i = 0; i < bufferCount; i++)
            {
                Buffers[i].Write(bw);
            }

            if (Type == FxcShaderType.GeometryShader) //GS seems to be slightly different format??
            {
                bw.Write((byte)0); //why is this here..? crazy GS
            }

            var dataLength = (uint)(ByteCode?.Length ?? 0);
            bw.Write(dataLength);
            if (dataLength > 0)
            {
                bw.Write(ByteCode);

                switch (Type)
                {
                    case FxcShaderType.VertexShader:
                    case FxcShaderType.PixelShader:
                    case FxcShaderType.GeometryShader: //only for VS,PS,GS
                        bw.Write(VersionMajor);
                        bw.Write(VersionMinor);
                        break;
                }
            }

        }

        public void WriteXml(StringBuilder sb, int indent, string csofolder)
        {
            var fname = (Name?.Replace("/", "")?.Replace("\\", "") ?? "NameError") + ".cso";
            FxcXml.StringTag(sb, indent, "Name", FxcXml.XmlEscape(Name));
            FxcXml.StringTag(sb, indent, "File", FxcXml.XmlEscape(fname));
            if (OffsetBy1)
            {
                FxcXml.ValueTag(sb, indent, "OffsetBy1", OffsetBy1.ToString());
            }
            switch (Type)
            {
                case FxcShaderType.VertexShader:
                case FxcShaderType.PixelShader:
                case FxcShaderType.GeometryShader: //only for VS,PS,GS
                    FxcXml.ValueTag(sb, indent, "VersionMajor", VersionMajor.ToString());
                    FxcXml.ValueTag(sb, indent, "VersionMinor", VersionMinor.ToString());
                    break;
            }
            if (Variables != null)
            {
                FxcXml.OpenTag(sb, indent, "Variables");
                for (int i = 0; i < Variables.Length; i++)
                {
                    FxcXml.StringTag(sb, indent + 1, "Item", FxcXml.XmlEscape(Variables[i]));
                }
                FxcXml.CloseTag(sb, indent, "Variables");
            }
            if (Buffers != null)
            {
                FxcXml.WriteItemArray(sb, Buffers, indent, "Buffers");
            }
            if (ByteCode != null)
            {
                var export = !string.IsNullOrEmpty(csofolder);
                if (export)
                {
                    try
                    {
                        if (!Directory.Exists(csofolder))
                        {
                            Directory.CreateDirectory(csofolder);
                        }
                        var filepath = Path.Combine(csofolder, fname);
                        File.WriteAllBytes(filepath, ByteCode);
                    }
                    catch
                    { }
                }
            }
        }
        public void ReadXml(XmlNode node, string csofolder)
        {
            //Type should be set before calling this!
            Name = Xml.GetChildInnerText(node, "Name");
            OffsetBy1 = Xml.GetChildBoolAttribute(node, "OffsetBy1");
            VersionMajor = (byte)Xml.GetChildUIntAttribute(node, "VersionMajor");
            VersionMinor = (byte)Xml.GetChildUIntAttribute(node, "VersionMinor");

            var vnode = node.SelectSingleNode("Variables");
            if (vnode != null)
            {
                var inodes = vnode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    var slist = new List<string>();
                    foreach (XmlNode inode in inodes)
                    {
                        slist.Add(inode.InnerText);
                    }
                    Variables = slist.ToArray();
                }
            }

            Buffers = XmlMeta.ReadItemArray<FxcShaderBufferRef>(node, "Buffers");


            var filename = Xml.GetChildInnerText(node, "File")?.Replace("/", "")?.Replace("\\", "");
            if (!string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(csofolder))
            {
                var filepath = Path.Combine(csofolder, filename);
                if (File.Exists(filepath))
                {
                    ByteCode = File.ReadAllBytes(filepath);
                }
                else
                {
                    throw new Exception("Shader file not found:\n" + filepath);
                }
            }

        }



        public override string ToString()
        {
            return Name;
        }
    }

    [TC(typeof(EXP))] public class FxcShaderBufferRef : IMetaXmlItem
    {
        public string Name { get; set; }
        public ushort Slot { get; set; }

        public void Read(BinaryReader br)
        {
            Name = FxcFile.ReadString(br);
            Slot = br.ReadUInt16();
        }
        public void Write(BinaryWriter bw)
        {
            FxcFile.WriteString(bw, Name);
            bw.Write(Slot);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            FxcXml.StringTag(sb, indent, "Name", FxcXml.XmlEscape(Name));
            FxcXml.ValueTag(sb, indent, "Slot", Slot.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            Slot = (ushort)Xml.GetChildUIntAttribute(node, "Slot");
        }

        public override string ToString()
        {
            return Name + ": " + Slot.ToString();
        }
    }

    [TC(typeof(EXP))] public class FxcCBuffer : IMetaXmlItem
    {
        public uint Size { get; set; }
        public ushort SlotVS { get; set; }
        public ushort SlotPS { get; set; }
        public ushort SlotCS { get; set; }
        public ushort SlotDS { get; set; }
        public ushort SlotGS { get; set; }
        public ushort SlotHS { get; set; }
        public string Name { get; set; }

        public uint NameHash { get { return JenkHash.GenHash(Name?.ToLowerInvariant()); } }
        public List<FxcVariable> VariablesList;
        public FxcVariable[] Variables { get; set; }

        public void Read(BinaryReader br)
        {
            Size = br.ReadUInt32(); //176, 16        //256
            SlotVS = br.ReadUInt16(); //6, 5          
            SlotPS = br.ReadUInt16(); //6, 12
            SlotCS = br.ReadUInt16(); //6, 5
            SlotDS = br.ReadUInt16(); //6, 5
            SlotGS = br.ReadUInt16(); //6, 5
            SlotHS = br.ReadUInt16(); //6, 5
            Name = FxcFile.ReadString(br); // <fxc name> _locals   //"rage_matrices", "misc_globals", "lighting_globals", "more_stuff"
            JenkIndex.Ensure(Name?.ToLowerInvariant()); //why not :P
        }
        public void Write(BinaryWriter bw)
        {
            bw.Write(Size);
            bw.Write(SlotVS);
            bw.Write(SlotPS);
            bw.Write(SlotCS);
            bw.Write(SlotDS);
            bw.Write(SlotGS);
            bw.Write(SlotHS);
            FxcFile.WriteString(bw, Name);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            FxcXml.StringTag(sb, indent, "Name", FxcXml.XmlEscape(Name));
            FxcXml.ValueTag(sb, indent, "Size", Size.ToString());
            FxcXml.ValueTag(sb, indent, "SlotVS", SlotVS.ToString());
            FxcXml.ValueTag(sb, indent, "SlotPS", SlotPS.ToString());
            FxcXml.ValueTag(sb, indent, "SlotCS", SlotCS.ToString());
            FxcXml.ValueTag(sb, indent, "SlotDS", SlotDS.ToString());
            FxcXml.ValueTag(sb, indent, "SlotGS", SlotGS.ToString());
            FxcXml.ValueTag(sb, indent, "SlotHS", SlotHS.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            Size = Xml.GetChildUIntAttribute(node, "Size");
            SlotVS = (ushort)Xml.GetChildUIntAttribute(node, "SlotVS");
            SlotPS = (ushort)Xml.GetChildUIntAttribute(node, "SlotPS");
            SlotCS = (ushort)Xml.GetChildUIntAttribute(node, "SlotCS");
            SlotDS = (ushort)Xml.GetChildUIntAttribute(node, "SlotDS");
            SlotGS = (ushort)Xml.GetChildUIntAttribute(node, "SlotGS");
            SlotHS = (ushort)Xml.GetChildUIntAttribute(node, "SlotHS");
        }

        public override string ToString()
        {
            return Name + ": " + Size + ", " + SlotVS + ", " + SlotPS + ", " + SlotCS + ", " + SlotDS + ", " + SlotGS + ", " + SlotHS;
        }

        public void AddVariable(FxcVariable vari)
        {
            if (VariablesList == null) VariablesList = new List<FxcVariable>();
            VariablesList.Add(vari);
        }
        public void ConsolidateVariables()
        {
            if (VariablesList != null)
            {
                //VariablesList.Sort((a, b) => a.Offset.CompareTo(b.Offset));
                Variables = VariablesList.ToArray();
                VariablesList = null;
            }
        }
    }

    public enum FxcVariableType : byte
    {
        Float = 2,
        Float2 = 3,
        Float3 = 4,
        Float4 = 5,
        Texture = 6, //also sampler
        Boolean = 7,
        Float3x4 = 8,
        Float4x4 = 9,
        Int = 11,
        Int4 = 14,
        Buffer = 15,
        Unused1 = 17,
        Unused2 = 18,
        Unused3 = 19,
        Unused4 = 20,
        UAVBuffer = 21,
        UAVTexture = 22,
    }

    [TC(typeof(EXP))] public class FxcVariable : IMetaXmlItem
    {
        public FxcVariableType Type { get; set; }
        public byte Count { get; set; } //array size
        public byte Slot { get; set; } //possibly GPU variable slot index for other platforms? or for CPU side
        public byte Group { get; set; } //maybe - variables in same buffer usually have same value
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public byte Offset { get; set; } //base offset (aligned to 16?)
        public byte Variant { get; set; } //255,0,1,2,3,4,5,7,9  seems to be used when multiple variables at same offset in the buffer
        public byte Unused0 { get; set; }//0
        public byte Unused1 { get; set; }//0
        public MetaHash CBufferName { get; set; }
        public byte ParamCount { get; set; }
        public FxcVariableParam[] Params { get; set; }
        public byte ValueCount { get; set; }
        public float[] ValuesF { get; set; }//optional default value for the variable, should match up with Type?
        public uint[] ValuesU { get; set; }

        private bool UseUIntValues
        {
            get
            {
                switch (Type)
                {
                    case FxcVariableType.Int:
                    case FxcVariableType.Int4:
                    case FxcVariableType.Boolean:
                    case FxcVariableType.Texture:
                    case FxcVariableType.Buffer:
                    case FxcVariableType.UAVTexture:
                    case FxcVariableType.UAVBuffer: return true;
                    default: return false;
                }
            }
        }

        public void Read(BinaryReader br)
        {
            Type = (FxcVariableType)br.ReadByte();
            Count = br.ReadByte();
            Slot = br.ReadByte();
            Group = br.ReadByte();
            Name1 = FxcFile.ReadString(br);
            Name2 = FxcFile.ReadString(br);
            Offset = br.ReadByte();
            Variant = br.ReadByte();
            Unused0 = br.ReadByte(); //always 0
            Unused1 = br.ReadByte(); //always 0
            CBufferName = br.ReadUInt32();


            ParamCount = br.ReadByte(); //1
            if (ParamCount > 0)
            {
                List<FxcVariableParam> parms = new List<FxcVariableParam>();
                for (int i = 0; i < ParamCount; i++)
                {
                    FxcVariableParam parm = new FxcVariableParam();
                    parm.Read(br);
                    parms.Add(parm);
                }
                Params = parms.ToArray();
            }

            ValueCount = br.ReadByte();
            if (ValueCount > 0)
            {
                if (UseUIntValues)
                {
                    ValuesU = new uint[ValueCount];
                    for (int i = 0; i < ValueCount; i++)
                    {
                        ValuesU[i] = br.ReadUInt32();
                    }
                }
                else
                {
                    ValuesF = new float[ValueCount];
                    for (int i = 0; i < ValueCount; i++)
                    {
                        ValuesF[i] = br.ReadSingle();
                    }
                }
            }



            //switch (Group)
            //{
            //    case 0:
            //    case 1:
            //    case 2:
            //    case 3:
            //    case 10:
            //    case 11:
            //    case 17:
            //    case 19:
            //    case 27:
            //    case 34:
            //        break;
            //    case 6:
            //    case 16:
            //    case 26:
            //    case 32:
            //    case 33:
            //    case 35:
            //    case 39:
            //    case 49:
            //    case 51:
            //        break;
            //    default:
            //        break;
            //}


        }
        public void Write(BinaryWriter bw)
        {
            bw.Write((byte)Type);
            bw.Write(Count);
            bw.Write(Slot);
            bw.Write(Group);
            FxcFile.WriteString(bw, Name1);
            FxcFile.WriteString(bw, Name2);
            bw.Write(Offset);
            bw.Write(Variant);
            bw.Write(Unused0);
            bw.Write(Unused1);
            bw.Write(CBufferName);

            ParamCount = (byte)(Params?.Length ?? 0);
            bw.Write(ParamCount);
            for (int i = 0; i < ParamCount; i++)
            {
                Params[i].Write(bw);
            }

            if (UseUIntValues)
            {
                ValueCount = (byte)(ValuesU?.Length ?? 0);
                bw.Write(ValueCount);
                for (int i = 0; i < ValueCount; i++)
                {
                    bw.Write(ValuesU[i]);
                }
            }
            else
            {
                ValueCount = (byte)(ValuesF?.Length ?? 0);
                bw.Write(ValueCount);
                for (int i = 0; i < ValueCount; i++)
                {
                    bw.Write(ValuesF[i]);
                }
            }


        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            FxcXml.StringTag(sb, indent, "Name1", FxcXml.XmlEscape(Name1));
            FxcXml.StringTag(sb, indent, "Name2", FxcXml.XmlEscape(Name2));
            FxcXml.StringTag(sb, indent, "Buffer", FxcXml.HashString(CBufferName));
            FxcXml.StringTag(sb, indent, "Type", Type.ToString());
            FxcXml.ValueTag(sb, indent, "Count", Count.ToString());
            FxcXml.ValueTag(sb, indent, "Slot", Slot.ToString());
            FxcXml.ValueTag(sb, indent, "Group", Group.ToString());
            FxcXml.ValueTag(sb, indent, "Offset", Offset.ToString());
            FxcXml.ValueTag(sb, indent, "Variant", Variant.ToString());
            if (Params != null)
            {
                FxcXml.WriteItemArray(sb, Params, indent, "Params");
            }
            if (ValuesF != null)
            {
                FxcXml.WriteRawArray(sb, ValuesF, indent, "Values", "", FloatUtil.ToString);
            }
            if (ValuesU != null)
            {
                FxcXml.WriteRawArray(sb, ValuesU, indent, "Values", "");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name1 = Xml.GetChildInnerText(node, "Name1");
            Name2 = Xml.GetChildInnerText(node, "Name2");
            CBufferName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Buffer"));
            Type = Xml.GetChildEnumInnerText<FxcVariableType>(node, "Type");
            Count = (byte)Xml.GetChildUIntAttribute(node, "Count");
            Slot = (byte)Xml.GetChildUIntAttribute(node, "Slot");
            Group = (byte)Xml.GetChildUIntAttribute(node, "Group");
            Offset = (byte)Xml.GetChildUIntAttribute(node, "Offset");
            Variant = (byte)Xml.GetChildUIntAttribute(node, "Variant");
            Params = XmlMeta.ReadItemArray<FxcVariableParam>(node, "Params");
            if (UseUIntValues)
            {
                ValuesU = Xml.GetChildRawUintArrayNullable(node, "Values");
            }
            else
            {
                ValuesF = Xml.GetChildRawFloatArrayNullable(node, "Values");
            }
        }


        public override string ToString()
        {
            var pstr = (ParamCount > 0) ? (", " + ParamCount.ToString() + " params") : "";
            var vstr = (ValueCount > 0) ? (", " + ValueCount.ToString() + " values") : "";
            return Type.ToString() + ", " + Count.ToString() + ", " + Slot.ToString() + ", " + Group.ToString() + ", " + Offset.ToString() + ", " + Variant.ToString() + ", " + CBufferName.ToString() + ", " + Name1 + ", " + Name2 + pstr + vstr;
        }
    }

    public enum FxcVariableParamType : byte
    {
        Int = 0,
        Float = 1,
        String = 2,
    }

    [TC(typeof(EXP))] public class FxcVariableParam : IMetaXmlItem
    {
        public string Name { get; set; }
        public FxcVariableParamType Type { get; set; }
        public object Value { get; set; }

        public void Read(BinaryReader br)
        {
            Name = FxcFile.ReadString(br);
            Type = (FxcVariableParamType)br.ReadByte();
            switch (Type)
            {
                case FxcVariableParamType.Int:
                    Value = br.ReadInt32();
                    break;
                case FxcVariableParamType.Float:
                    Value = br.ReadSingle();
                    break;
                case FxcVariableParamType.String:
                    Value = FxcFile.ReadString(br);
                    break;
                default:
                    break;
            }
        }
        public void Write(BinaryWriter bw)
        {
            FxcFile.WriteString(bw, Name);
            bw.Write((byte)Type);
            switch (Type)
            {
                case FxcVariableParamType.Int:
                    bw.Write((int)Value);
                    break;
                case FxcVariableParamType.Float:
                    bw.Write((float)Value);
                    break;
                case FxcVariableParamType.String:
                    FxcFile.WriteString(bw, (string)Value);
                    break;
                default:
                    break;
            }
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            FxcXml.StringTag(sb, indent, "Name", FxcXml.XmlEscape(Name));
            FxcXml.StringTag(sb, indent, "Type", Type.ToString());
            switch (Type)
            {
                case FxcVariableParamType.Int:
                    FxcXml.ValueTag(sb, indent, "Value", ((int)Value).ToString());
                    break;
                case FxcVariableParamType.Float:
                    FxcXml.ValueTag(sb, indent, "Value", FloatUtil.ToString((float)Value));
                    break;
                case FxcVariableParamType.String:
                    FxcXml.StringTag(sb, indent, "Value", FxcXml.XmlEscape(Value as string));
                    break;
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            Type = Xml.GetChildEnumInnerText<FxcVariableParamType>(node, "Type");
            switch (Type)
            {
                case FxcVariableParamType.Int:
                    Value = Xml.GetChildIntAttribute(node, "Value");
                    break;
                case FxcVariableParamType.Float:
                    Value = Xml.GetChildFloatAttribute(node, "Value");
                    break;
                case FxcVariableParamType.String:
                    Value = Xml.GetChildInnerText(node, "Value");
                    break;
            }
        }

        public override string ToString()
        {
            return Name + ": " + Value?.ToString();
        }
    }

    [TC(typeof(EXP))] public class FxcTechnique : IMetaXmlItem
    {
        public string Name { get; set; }
        public byte PassCount { get; set; }
        public FxcPass[] Passes { get; set; }

        public void Read(BinaryReader br)
        {
            Name = FxcFile.ReadString(br); //"draw", "deferred_draw", etc..

            PassCount = br.ReadByte();//
            if (PassCount > 0)
            {
                Passes = new FxcPass[PassCount];
                for (int i = 0; i < PassCount; i++)
                {
                    FxcPass p = new FxcPass();
                    p.Read(br);
                    Passes[i] = p;
                }
            }

        }
        public void Write(BinaryWriter bw)
        {
            FxcFile.WriteString(bw, Name);
            
            PassCount = (byte)(Passes?.Length ?? 0);
            bw.Write(PassCount);
            for (int i = 0; i < PassCount; i++)
            {
                Passes[i].Write(bw);
            }

        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            FxcXml.StringTag(sb, indent, "Name", FxcXml.XmlEscape(Name));
            FxcXml.WriteItemArray(sb, Passes, indent, "Passes");
        }
        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            Passes = XmlMeta.ReadItemArray<FxcPass>(node, "Passes");
        }

        public void GetNamesFromIndices(FxcFile fxc)
        {
            if (Passes == null) return;
            foreach (var pass in Passes)
            {
                pass.GetNamesFromIndices(fxc);
            }
        }
        public void GetIndicesFromNames(FxcFile fxc)
        {
            if (Passes == null) return;
            foreach (var pass in Passes)
            {
                pass.GetIndicesFromNames(fxc);
            }
        }

        public override string ToString()
        {
            return Name + " (" + PassCount.ToString() + " pass" + (PassCount != 1 ? "es" : "") + ")";
        }
    }

    [TC(typeof(EXP))] public class FxcPass : IMetaXmlItem
    {
        public byte VS { get; set; }
        public byte PS { get; set; }
        public byte CS { get; set; }
        public byte DS { get; set; }
        public byte GS { get; set; }
        public byte HS { get; set; }
        public byte ParamCount { get; set; }
        public FxcPassParam[] Params { get; set; } //probably referring to eg SetBlendState, SetRasterizerState etc

        public string VSName { get; set; }
        public string PSName { get; set; }
        public string CSName { get; set; }
        public string DSName { get; set; }
        public string GSName { get; set; }
        public string HSName { get; set; }

        public void Read(BinaryReader br)
        {
            VS = br.ReadByte();
            PS = br.ReadByte();
            CS = br.ReadByte();
            DS = br.ReadByte();
            GS = br.ReadByte();
            HS = br.ReadByte();

            ParamCount = br.ReadByte();
            if (ParamCount > 0)
            {
                Params = new FxcPassParam[ParamCount];
                for (int i = 0; i < ParamCount; i++)
                {
                    FxcPassParam p = new FxcPassParam();
                    p.Read(br);
                    Params[i] = p;
                }
            }
        }
        public void Write(BinaryWriter bw)
        {
            bw.Write(VS);
            bw.Write(PS);
            bw.Write(CS);
            bw.Write(DS);
            bw.Write(GS);
            bw.Write(HS);

            ParamCount = (byte)(Params?.Length ?? 0);
            bw.Write(ParamCount);
            for (int i = 0; i < ParamCount; i++)
            {
                Params[i].Write(bw);
            }
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            if (!string.IsNullOrEmpty(VSName)) FxcXml.StringTag(sb, indent, "VertexShader", FxcXml.XmlEscape(VSName));
            if (!string.IsNullOrEmpty(PSName)) FxcXml.StringTag(sb, indent, "PixelShader", FxcXml.XmlEscape(PSName));
            if (!string.IsNullOrEmpty(CSName)) FxcXml.StringTag(sb, indent, "ComputeShader", FxcXml.XmlEscape(CSName));
            if (!string.IsNullOrEmpty(DSName)) FxcXml.StringTag(sb, indent, "DomainShader", FxcXml.XmlEscape(DSName));
            if (!string.IsNullOrEmpty(GSName)) FxcXml.StringTag(sb, indent, "GeometryShader", FxcXml.XmlEscape(GSName));
            if (!string.IsNullOrEmpty(HSName)) FxcXml.StringTag(sb, indent, "HullShader", FxcXml.XmlEscape(HSName));

            if (Params != null)
            {
                FxcXml.WriteItemArray(sb, Params, indent, "Params");
            }
        }
        public void ReadXml(XmlNode node)
        {
            VSName = Xml.GetChildInnerText(node, "VertexShader");
            PSName = Xml.GetChildInnerText(node, "PixelShader");
            CSName = Xml.GetChildInnerText(node, "ComputeShader");
            DSName = Xml.GetChildInnerText(node, "DomainShader");
            GSName = Xml.GetChildInnerText(node, "GeometryShader");
            HSName = Xml.GetChildInnerText(node, "HullShader");
            Params = XmlMeta.ReadItemArray<FxcPassParam>(node, "Params");
        }


        public void GetNamesFromIndices(FxcFile fxc)
        {
            VSName = fxc.GetVS(VS)?.Name;
            PSName = fxc.GetPS(PS)?.Name;
            CSName = fxc.GetCS(CS)?.Name;
            DSName = fxc.GetDS(DS)?.Name;
            GSName = fxc.GetGS(GS)?.Name;
            HSName = fxc.GetHS(HS)?.Name;
        }
        public void GetIndicesFromNames(FxcFile fxc)
        {
            VS = fxc.GetVSID(VSName);
            PS = fxc.GetPSID(PSName);
            CS = fxc.GetCSID(CSName);
            DS = fxc.GetDSID(DSName);
            GS = fxc.GetGSID(GSName);
            HS = fxc.GetHSID(HSName);
        }


        public override string ToString()
        {
            return VS.ToString() + ", " + PS.ToString() + ", " + CS.ToString() + ", " + DS.ToString() + ", " + GS.ToString() + ", " + HS.ToString();
        }
    }

    [TC(typeof(EXP))] public class FxcPassParam : IMetaXmlItem
    {
        public uint Type { get; set; } //probably referring to eg SetBlendState, SetRasterizerState etc
        public uint Value { get; set; }

        public void Read(BinaryReader br)
        {
            Type = br.ReadUInt32();
            Value = br.ReadUInt32();

            //switch (Type)
            //{
            //    case 10:
            //    case 3:
            //    case 6:
            //    case 7:
            //    case 19:
            //    case 20:
            //    case 21:
            //    case 22:
            //    case 2:
            //    case 11:
            //    case 25:
            //    case 23:
            //    case 4:
            //    case 5:
            //    case 24:
            //    case 26:
            //    case 27:
            //    case 14:
            //    case 12:
            //    case 13:
            //    case 15:
            //    case 16:
            //    case 17:
            //    case 18:
            //    case 0:
            //    case 9:
            //    case 8:
            //    case 29:
            //        break;
            //    default:
            //        break;
            //}
            //switch (Value)
            //{
            //    case 0: //type 10, 3, 20, 21, 22, 2, 11, 18
            //    case 1: //type 6, 25, 23, 5, 24, 14, 12, 13, 0
            //    case 8: //type 7, 19
            //    case 5: //type 7, 9
            //    case 2: //type 4, 26
            //    case 6: //type 27
            //    case 7: //type 7, 16, 17
            //    case 3: //type 15
            //    case 4: //type 22
            //    case 15://type 20
            //    case 100://type 8
            //    case 9: //type 4
            //    case 12://type 16
            //    case 67://type 16
            //    case 64://type 17
            //    case 191://type 18
            //    case 0xBDCCCCCD://(-0.1f) type 29
            //    case 0x3E4CCCCD://(0.2f) type 29
            //    case 10://type 8
            //        break;
            //    default:
            //        break;
            //}
        }
        public void Write(BinaryWriter bw)
        {
            bw.Write(Type);
            bw.Write(Value);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            FxcXml.ValueTag(sb, indent, "Type", Type.ToString());
            FxcXml.ValueTag(sb, indent, "Value", Value.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Type = Xml.GetChildUIntAttribute(node, "Type");
            Value = Xml.GetChildUIntAttribute(node, "Value");
        }

        public override string ToString()
        {
            return Type.ToString() + ", " + Value.ToString();
        }
    }








    public class FxcXml : MetaXmlBase
    {

        public static string GetXml(FxcFile fxc, string outputFolder = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if ((fxc != null) && (fxc.Shaders != null))
            {
                var name = "Effects";
                OpenTag(sb, 0, name);
                fxc.WriteXml(sb, 1, outputFolder);
                CloseTag(sb, 0, name);
            }

            return sb.ToString();
        }

    }

    public class XmlFxc
    {

        public static FxcFile GetFxc(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetFxc(doc, inputFolder);
        }

        public static FxcFile GetFxc(XmlDocument doc, string inputFolder = "")
        {
            FxcFile fxc = new FxcFile();
            fxc.ReadXml(doc.DocumentElement, inputFolder);
            return fxc;
        }

    }



}
