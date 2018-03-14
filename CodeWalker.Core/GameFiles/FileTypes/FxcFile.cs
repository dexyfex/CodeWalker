using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//GTAV FXC file reader by dexyfex.
// use this however you want. some credit would be nice.

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FxcFile : PackedFile
    {
        public string Name { get; set; }
        public RpfFileEntry FileEntry { get; set; }
        public uint Hash { get; set; }

        public VertexType VertexType { get; set; }
        public FxcHeaderExt[] Exts { get; set; }
        public FxcHeaderChunk[] Chunks { get; set; }
        public FxcShader[] Shaders { get; set; }
        public FxcCBuffer[] CBuffers1 { get; set; }
        public FxcVariable[] Variables1 { get; set; }
        public FxcCBuffer[] CBuffers2 { get; set; }
        public FxcVariable[] Variables2 { get; set; }
        public FxcTechnique[] Techniques { get; set; }


        public FxcShader[] VertexShaders { get; set; }
        public FxcShader[] PixelShaders { get; set; }
        public FxcShader[] ComputeShaders { get; set; }
        public FxcShader[] DomainShaders { get; set; }
        public FxcShader[] GeometryShaders { get; set; }
        public FxcShader[] HullShaders { get; set; }

        public Dictionary<uint, FxcCBuffer> CBufferDict { get; set; }
        public FxcVariable[] GlobalVariables { get; set; }


        public string LastError { get; set; }






        public void Load(byte[] data, RpfFileEntry entry)
        {
            FileEntry = entry;
            Name = entry.Name;
            Hash = entry.ShortNameHash;

            LastError = string.Empty;

            MemoryStream ms = new MemoryStream(data);
            BinaryReader br = new BinaryReader(ms);

            uint magic_rgxe = br.ReadUInt32();
            if(magic_rgxe!= 1702389618) //"rgxe"
            {
                return;
            }

            VertexType = (VertexType)br.ReadUInt32();

            byte ec0 = br.ReadByte();
            var exts1 = new List<FxcHeaderExt>();
            for (int e = 0; e < ec0; e++)
            {
                FxcHeaderExt ext = new FxcHeaderExt();
                ext.Name = ReadString(br);
                ext.Unk0Byte = br.ReadByte(); //0
                ext.Unk1Uint = br.ReadUInt32(); //2
                exts1.Add(ext);
            }
            Exts = exts1.ToArray();


            List<FxcShader>[] shadergrps = new List<FxcShader>[6];

            var chunks = new List<FxcHeaderChunk>();
            var shaders = new List<FxcShader>();
            int gindex = 0;
            while (gindex<6)// (sc0 > 0)
            {
                var shadergrp = new List<FxcShader>();
                shadergrps[gindex] = shadergrp;

                gindex++;
                byte sc0 = br.ReadByte();
                if (sc0 == 0)
                {
                    sc0 = br.ReadByte(); //this is a little odd, sometimes a byte skip
                }
                FxcHeaderChunk chunk = new FxcHeaderChunk();
                chunk.Read(br);
                chunk.Gindex = gindex;
                chunk.ShaderCount = sc0;
                chunks.Add(chunk);
                for (int s = 1; s < sc0; s++)
                {
                    bool exbyteflag1 = (gindex==5); //GS seems to be in diff format??
                    bool vsgsps = (gindex == 1) || (gindex == 2) || (gindex == 5);
                    FxcShader shader = new FxcShader();
                    if (!shader.Read(br, exbyteflag1, vsgsps))
                    {
                        LastError += shader.LastError;
                        //gindex = 6; //get outta the loop?
                        //break;
                    }
                    shaders.Add(shader);
                    shadergrp.Add(shader);
                }
            }

            Shaders = shaders.ToArray();
            VertexShaders = shadergrps[0]?.ToArray();
            PixelShaders = shadergrps[1]?.ToArray();
            ComputeShaders = shadergrps[2]?.ToArray();
            DomainShaders = shadergrps[3]?.ToArray();
            GeometryShaders = shadergrps[4]?.ToArray();
            HullShaders = shadergrps[5]?.ToArray();


            Chunks = chunks.ToArray();

            //ms.Dispose();
            //return;

            List<FxcVariable> globalVars = new List<FxcVariable>();
            CBufferDict = new Dictionary<uint, FxcCBuffer>();
            FxcCBuffer cbtmp = null;

            try //things can be uncertain after this...
            {

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
                        if (CBufferDict.TryGetValue(vari.CBufferName, out cbtmp))
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
                        if (CBufferDict.TryGetValue(vari.CBufferName, out cbtmp))
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
                { }

            }
            catch (Exception ex)
            {
                LastError = ex.ToString();
            }

            ms.Dispose();
        }



        public string GetMetaString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name: " + Name);
            sb.AppendLine("Hash: " + Hash.ToString());
            sb.AppendLine("Vertex type: " + ((uint)VertexType).ToString());
            sb.AppendLine();

            if (Exts != null)
            {
                sb.AppendLine("Header");
                foreach (var ext in Exts)
                {
                    sb.AppendLine("  " + ext.ToString());
                }
                sb.AppendLine();
            }

            if (Chunks != null)
            {
                sb.AppendLine("Sections");
                foreach (var chunk in Chunks)
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
                    if ((shader.Params != null) && (shader.Params.Length > 0))
                    {
                        sb.AppendLine("    (Params)");
                        foreach (var parm in shader.Params)
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
                    if ((p.Values != null) && (p.Values.Length > 0))
                    {
                        sb.AppendLine("    (Values)");
                        foreach (var d in p.Values)
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
                    if ((p.Values != null) && (p.Values.Length > 0))
                    {
                        sb.AppendLine("    (Values)");
                        foreach (var d in p.Values)
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




        public static string ReadString(BinaryReader br)
        {
            byte sl = br.ReadByte();
            if (sl == 0) return string.Empty;
            byte[] ba = new byte[sl];
            br.Read(ba, 0, sl);
            return (sl > 1) ? ASCIIEncoding.ASCII.GetString(ba, 0, sl - 1) : string.Empty;
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
                    for (int n = 0; n < sl; n++)
                    {
                        sb.Append((char)br.ReadByte());
                    }
                    r[i] = sb.ToString().Replace("\0", "");
                }
            }

            return r;
        }

    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class FxcHeaderExt
    {
        public string Name { get; set; }
        public byte Unk0Byte { get; set; }
        public uint Unk1Uint { get; set; }

        public override string ToString()
        {
            return Name + ": " + Unk0Byte.ToString() + ": " + Unk1Uint.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FxcHeaderChunk
    {
        public string Name { get; set; }
        public byte Unk1Byte { get; set; }
        public byte Unk2Byte { get; set; }
        public uint Unk3Uint { get; set; }
        public int Gindex { get; set; } //index in the fxc file
        public byte ShaderCount { get; set; } //number of shaders in the section

        public void Read(BinaryReader br)
        {
            Name = FxcFile.ReadString(br); //usually "NULL"
            Unk1Byte = br.ReadByte();
            Unk2Byte = br.ReadByte();
            Unk3Uint = br.ReadUInt32();
        }

        public override string ToString()
        {
            return Name + ": " + Gindex.ToString() + ": " + ShaderCount.ToString() + ": " + Unk1Byte.ToString() + ": " + Unk2Byte.ToString() + ": " + Unk3Uint.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FxcShader
    {
        public long Offset { get; set; }
        public string Name { get; set; }
        public string[] Params { get; set; }
        public FxcShaderBufferRef[] Buffers { get; set; }//CBuffers
        public byte VersionMajor { get; set; }
        public byte VersionMinor { get; set; }
        public string VersionString { get; set; }
        public byte[] ByteCode { get; set; }
        //public ShaderBytecode ByteCodeObj { get; set; }
        //public ShaderProfile ShaderProfile { get; set; }
        public string Disassembly { get; set; }
        public string LastError { get; set; }

        public bool Read(BinaryReader br, bool exbyteflag, bool vsgsps)
        {
            Offset = br.BaseStream.Position;

            Name = FxcFile.ReadString(br);

            if (Name.Length == 0)
            {
                Name = FxcFile.ReadString(br); //why  (seems to be GS only)
                exbyteflag = true;
            }


            Params = FxcFile.ReadStringArray(br);

            byte bufferCount = br.ReadByte();
            var buffers = new List<FxcShaderBufferRef>();
            for (int e = 0; e < bufferCount; e++)
            {
                FxcShaderBufferRef ext = new FxcShaderBufferRef();
                ext.Name = FxcFile.ReadString(br);
                ext.Unk0Ushort = br.ReadUInt16();
                buffers.Add(ext);
            }
            Buffers = buffers.ToArray();

            byte exbyte = 0;
            if (exbyteflag)
            {
                exbyte = br.ReadByte(); //not sure what this is used for...
                if ((exbyte != 0))
                { }
            }


            uint datalength = br.ReadUInt32();

            if (datalength > 0)
            {
                uint magic_dxbc = br.ReadUInt32();
                if (magic_dxbc != 1128421444) //"DXBC" - directx bytecode header
                {
                    LastError += "Unexpected data found at DXBC header...\r\n";
                    return false; //didn't find the DXBC header... abort!
                }
                br.BaseStream.Position -= 4; //wind back because dx needs that header

                ByteCode = br.ReadBytes((int)datalength);

                if (vsgsps)
                {
                    VersionMajor = br.ReadByte();//4,5 //appears to be shader model version
                    VersionMinor = br.ReadByte(); //perhaps shader minor version
                }

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
            }
            else
            {
            }
            return true;
        }



        public override string ToString()
        {
            return Name;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FxcShaderBufferRef
    {
        public string Name { get; set; } //Buffer name
        public ushort Unk0Ushort { get; set; }

        public override string ToString()
        {
            return Name + ": " + Unk0Ushort.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FxcCBuffer
    {
        public uint u003 { get; set; }
        public ushort us001 { get; set; }
        public ushort us002 { get; set; }
        public ushort us003 { get; set; }
        public ushort us004 { get; set; }
        public ushort us005 { get; set; }
        public ushort us006 { get; set; }
        public string Name { get; set; }

        public uint NameHash { get { return JenkHash.GenHash(Name); } }
        public List<FxcVariable> VariablesList;
        public FxcVariable[] Variables { get; set; }

        public void Read(BinaryReader br)
        {
            u003 = br.ReadUInt32(); //176, 16        //256
            us001 = br.ReadUInt16(); //6, 5          
            us002 = br.ReadUInt16(); //6, 12
            us003 = br.ReadUInt16(); //6, 5
            us004 = br.ReadUInt16(); //6, 5
            us005 = br.ReadUInt16(); //6, 5
            us006 = br.ReadUInt16(); //6, 5
            Name = FxcFile.ReadString(br); // <fxc name> _locals   //"rage_matrices", "misc_globals", "lighting_globals", "more_stuff"
            JenkIndex.Ensure(Name); //why not :P
        }

        public override string ToString()
        {
            return Name + ": " + u003 + ", " + us001 + ", " + us002 + ", " + us003 + ", " + us004 + ", " + us005 + ", " + us006;
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
                Variables = VariablesList.ToArray();
                VariablesList = null;
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FxcVariable
    {
        public byte b0 { get; set; }
        public byte b1 { get; set; }
        public byte b2 { get; set; }
        public byte b3 { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public byte b4 { get; set; }
        public byte b5 { get; set; }
        public byte b6 { get; set; }
        public byte b7 { get; set; }
        public MetaHash CBufferName { get; set; }
        public byte ParamCount { get; set; }
        public FxcVariableParam[] Params { get; set; }
        public byte ValueCount { get; set; }
        public float[] Values { get; set; }


        public void Read(BinaryReader br)
        {
            b0 = br.ReadByte(); //5
            b1 = br.ReadByte(); //0,1
            b2 = br.ReadByte(); //19    //17
            b3 = br.ReadByte(); //2     //27
            Name1 = FxcFile.ReadString(br);
            Name2 = FxcFile.ReadString(br);
            b4 = br.ReadByte(); //32
            b5 = br.ReadByte(); //
            b6 = br.ReadByte(); //
            b7 = br.ReadByte(); //
            CBufferName = br.ReadUInt32(); //hash


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
                Values = new float[ValueCount];
                for (int i = 0; i < ValueCount; i++)
                {
                    Values[i] = br.ReadSingle(); //TODO: how to know what types to use?
                }
            }


            #region debug validation

            switch (b0)
            {
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 11:
                case 14:
                case 17:
                case 20:
                    break;
                case 15:
                case 18:
                case 19:
                case 21:
                case 22:
                    break;
                default:
                    break;
            }

            switch (b1)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 8:
                case 12:
                case 254:
                case 255:
                    break;
                case 14:
                case 16:
                case 24:
                case 40:
                case 117:
                    break;
                default:
                    break;
            }

            switch (CBufferName)
            {
                case 2165770756:
                case 3458315595:
                case 1059338858:
                case 3779850771:
                case 2988188919:
                case 713939274:
                case 3369176576:
                case 1927486087:
                case 4135739982:
                case 1032736383:
                case 3084860475:
                case 3844532749:
                case 3635751376:
                case 2172777116:
                case 4255900365:
                case 2725372071:
                case 3661207622:
                case 4213364555:
                case 1519748817:
                case 118958736:
                case 2397841684:
                case 1340365912:
                case 2531859994:
                case 0:
                    break;
                case 482774302:
                case 2300268537:
                case 2714887816:
                case 2049538169:
                case 1316881611:
                case 3367312321:
                case 4017086211:
                case 3743503190:
                case 938670440:
                case 2782027784:
                case 2865440919:
                case 2384984532:
                case 486482914:
                case 3162602184:
                case 1834530379:
                case 1554708878:
                case 1142002603:
                case 3049310097:
                case 764124013:
                case 2526104914:
                case 1561144077:
                case 970248680:
                case 3899781660:
                case 1853474951:
                case 2224880237:
                case 3766848419:
                case 2718810031:
                case 115655537:
                case 4224116138:
                case 3572088685:
                case 1438660507:
                case 4092686193:
                case 871214106:
                case 2121263542:
                case 3502503908:
                case 586499600:
                case 4046148196:
                case 2999112456:
                case 2355014976:
                case 579364910:
                case 2193272593:
                case 1641847936:
                case 1286180849:
                case 3291504934:
                case 278397346:
                case 3346871633:
                case 4091106477:
                case 832855465:
                case 3616072140:
                case 3977262900:
                case 2062541604:
                case 950211059:
                case 2380663322:
                case 2783177544:
                case 1100625170:
                case 1279142172:
                case 1004646027:
                case 2092585241:
                case 4165560568:
                case 2651790209:
                case 3453406875:
                case 488789527:
                case 3375019131:
                case 519785780:
                case 729415208:
                case 556501613:
                case 2829744882:
                case 1778702372:
                case 2564407213:
                case 3291498326:
                case 1275817784:
                case 962813362:
                case 2020034807:
                case 2017746823:
                case 1237102223:
                case 4029270406:
                case 673228990:
                case 201854132:
                case 1866965008:
                case 957783816:
                case 2522030664:
                case 1910375705:
                case 2656344872:
                    break;
                default:
                    break;
            }

            switch (b3)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 10:
                case 11:
                case 17:
                case 19:
                case 27:
                case 34:
                    break;
                case 6:
                case 16:
                case 26:
                case 32:
                case 33:
                case 35:
                case 39:
                case 49:
                case 51:
                    break;
                default:
                    break;
            }


            #endregion


        }

        public override string ToString()
        {
            return b0.ToString() + ", " + b1.ToString() + ", " + b2.ToString() + ", " + b3.ToString() + ", " + b4.ToString() + ", " + b5.ToString() + ", " + b6.ToString() + ", " + b7.ToString() + ", " + CBufferName.ToString() + ", " + Name1 + ", " + Name2;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FxcVariableParam
    {
        public string Name { get; set; }
        public byte Type { get; set; }
        public object Value { get; set; }

        public void Read(BinaryReader br)
        {
            Name = FxcFile.ReadString(br);
            Type = br.ReadByte();
            switch (Type)
            {
                case 0:
                    Value = br.ReadUInt32();
                    break;
                case 1:
                    Value = br.ReadSingle();
                    break;
                case 2:
                    Value = FxcFile.ReadString(br);
                    break;
                default:
                    break;
            }
        }

        public override string ToString()
        {
            return Name + ": " + Value?.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FxcTechnique
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

        public override string ToString()
        {
            return Name + " (" + PassCount.ToString() + " pass" + (PassCount != 1 ? "es" : "") + ")";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FxcPass
    {
        public byte VS { get; set; }
        public byte PS { get; set; }
        public byte CS { get; set; }
        public byte DS { get; set; }
        public byte GS { get; set; }
        public byte HS { get; set; }
        public byte ParamCount { get; set; }
        public FxcPassParam[] Params { get; set; }

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
                    p.u0 = br.ReadUInt32();
                    p.u1 = br.ReadUInt32();
                    Params[i] = p;
                }
            }
        }

        public override string ToString()
        {
            return VS.ToString() + ", " + PS.ToString() + ", " + CS.ToString() + ", " + DS.ToString() + ", " + GS.ToString() + ", " + HS.ToString();
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FxcPassParam
    {
        public uint u0 { get; set; }
        public uint u1 { get; set; }

        public override string ToString()
        {
            return u0.ToString() + ", " + u1.ToString();
        }
    }


}
