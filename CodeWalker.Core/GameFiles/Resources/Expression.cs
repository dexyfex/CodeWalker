using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ExpressionDictionary : ResourceFileBase
    {
        // pgDictionaryBase
        // pgDictionary<crExpressions>
        public override long BlockLength => 0x40;

        // structure data
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }
        public ResourceSimpleList64_s<MetaHash> ExpressionNameHashes { get; set; }
        public ResourcePointerList64<Expression> Expressions { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.ExpressionNameHashes = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            this.Expressions = reader.ReadBlock<ResourcePointerList64<Expression>>();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteBlock(this.ExpressionNameHashes);
            writer.WriteBlock(this.Expressions);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            return list.ToArray();
        }
        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, ExpressionNameHashes),
                new Tuple<long, IResourceBlock>(0x30, Expressions)
            };
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class Expression : ResourceSystemBlock
    {
        // pgBase
        // crExpressions
        public override long BlockLength => 0x90;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } = 1;
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ResourcePointerList64<ExpressionUnk1> Unknown_20h { get; set; }
        public ResourceSimpleList64_s<ExpressionUnk3> Unknown_30h { get; set; } // bone tags / animation tracks..??
        public ResourceSimpleList64<ExpressionUnk2> Unknown_40h { get; set; }
        public ResourceSimpleList64_s<MetaHash> Unknown_50h { get; set; }  // only for: faceinit.expr, independent_mover.expr
        public ulong NamePointer { get; set; }
        public ushort NameLength { get; set; } // name len
        public ushort NameCapacity { get; set; } // name len+1
        public uint Unknown_6Ch { get; set; } // 0x00000000
        public uint Unknown_70h { get; set; } = 1;
        public MetaHash Unknown_74h { get; set; } // seems to be a hash?
        public ushort Unk1ItemLength { get; set; } // max length of any Unk1 item in Unknown_20h
        public ushort Unknown_7Ah { get; set; } // 0x0000
        public uint Unknown_7Ch { get; set; } // 3 or 2
        public uint Unknown_80h { get; set; } // 0x00000000
        public uint Unknown_84h { get; set; } // 0x00000000
        public uint Unknown_88h { get; set; } // 0x00000000
        public uint Unknown_8Ch { get; set; } // 0x00000000

        // reference data
        public string_r Name;

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
            this.Unknown_20h = reader.ReadBlock<ResourcePointerList64<ExpressionUnk1>>();
            this.Unknown_30h = reader.ReadBlock<ResourceSimpleList64_s<ExpressionUnk3>>();
            this.Unknown_40h = reader.ReadBlock<ResourceSimpleList64<ExpressionUnk2>>();
            this.Unknown_50h = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            this.NamePointer = reader.ReadUInt64();
            this.NameLength = reader.ReadUInt16();
            this.NameCapacity = reader.ReadUInt16();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.Unk1ItemLength = reader.ReadUInt16();
            this.Unknown_7Ah = reader.ReadUInt16();
            this.Unknown_7Ch = reader.ReadUInt32();
            this.Unknown_80h = reader.ReadUInt32();
            this.Unknown_84h = reader.ReadUInt32();
            this.Unknown_88h = reader.ReadUInt32();
            this.Unknown_8Ch = reader.ReadUInt32();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(
                this.NamePointer // offset
            );

            //if (Unknown_50h?.data_items?.Length > 0)
            //{ } // faceinit.expr, independent_mover.expr

            #region testing
            //long tlen = 0;
            //if (Unknown_20h?.data_items != null) foreach (var item in Unknown_20h.data_items) tlen = Math.Max(tlen, item.BlockLength);
            //if (Unk1ItemLength != tlen)
            //{ }//no hit

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_Ch != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_14h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
            //if (Unknown_1Ch != 0)
            //{ }//no hit
            //if (NameLength != (Name?.Value?.Length ?? 0))
            //{ }//no hit
            //if (NameCapacity != (NameLength + 1))
            //{ }//no hit
            //if (Unknown_6Ch != 0)
            //{ }//no hit
            //if (Unknown_70h != 1)
            //{ }//no hit
            //switch (Unknown_74h)
            //{
            //    default:
            //        break;
            //}
            //if (Unknown_7Ah != 0)
            //{ }//no hit
            //switch (Unknown_7Ch)
            //{
            //    case 3:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_80h != 0)
            //{ }//no hit
            //if (Unknown_84h != 0)
            //{ }//no hit
            //if (Unknown_88h != 0)
            //{ }//no hit
            //if (Unknown_8Ch != 0)
            //{ }//no hit
            #endregion
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.Name != null ? this.Name.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteBlock(this.Unknown_20h);
            writer.WriteBlock(this.Unknown_30h);
            writer.WriteBlock(this.Unknown_40h);
            writer.WriteBlock(this.Unknown_50h);
            writer.Write(this.NamePointer);
            writer.Write(this.NameLength);
            writer.Write(this.NameCapacity);
            writer.Write(this.Unknown_6Ch);
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.Write(this.Unk1ItemLength);
            writer.Write(this.Unknown_7Ah);
            writer.Write(this.Unknown_7Ch);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_84h);
            writer.Write(this.Unknown_88h);
            writer.Write(this.Unknown_8Ch);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            return list.ToArray();
        }
        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, Unknown_20h),
                new Tuple<long, IResourceBlock>(0x30, Unknown_30h),
                new Tuple<long, IResourceBlock>(0x40, Unknown_40h),
                new Tuple<long, IResourceBlock>(0x50, Unknown_50h)
            };
        }


        public override string ToString()
        {
            return (Name?.ToString() ?? base.ToString()) + "   " + Unknown_74h.ToString();
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class ExpressionUnk1 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16 + Data1.Length + Data2.Length + Data3.Length; }
        }

        // structure data
        public MetaHash Unknown_0h { get; set; }
        public uint len1 { get; set; }
        public uint len2 { get; set; }
        public ushort len3 { get; set; }
        public ushort Unknown_Eh { get; set; }
        public byte[] Data1 { get; set; }
        public byte[] Data2 { get; set; }
        public byte[] Data3 { get; set; }


        public ExpressionUnk1_Base[] Items { get; set; }



        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.len1 = reader.ReadUInt32();
            this.len2 = reader.ReadUInt32();
            this.len3 = reader.ReadUInt16();
            this.Unknown_Eh = reader.ReadUInt16();
            this.Data1 = reader.ReadBytes((int)len1);
            this.Data2 = reader.ReadBytes((int)len2);
            this.Data3 = reader.ReadBytes((int)len3);

//#if DEBUG
            ParseDatas();
//#endif

            //switch (Unknown_Eh)
            //{
            //    case 2:
            //    case 8:
            //    case 4:
            //    case 6:
            //    case 59:
            //    case 5:
            //    case 12:
            //    case 58:
            //    case 20:
            //    case 10:
            //    case 9:
            //    case 1:
            //    case 50:
            //    case 14:
            //    case 19:
            //    case 7:
            //    case 25:
            //    case 15:
            //    case 13:
            //    case 28:
            //    case 17:
            //    case 22:
            //    case 26:
            //    case 18:
            //    case 21:
            //    case 23:
            //    case 11:
            //    case 24:
            //    case 27:
            //    case 30:
            //    case 16:
            //    case 377:
            //    case 31:
            //    case 125:
            //    case 32:
            //    case 34:
            //    case 52:
            //    case 51:
            //    case 345:
            //    case 399:
            //        break;
            //    default:
            //        break;//no hit
            //}

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.len1);
            writer.Write(this.len2);
            writer.Write(this.len3);
            writer.Write(this.Unknown_Eh);
            writer.Write(this.Data1);
            writer.Write(this.Data2);
            writer.Write(this.Data3);
        }


        public void ParseDatas()
        {

            var ms1 = new MemoryStream(Data1);
            var ms2 = new MemoryStream(Data2);
            var ms3 = new MemoryStream(Data3);
            var dr1 = new DataReader(ms1);
            var dr2 = new DataReader(ms2);
            var dr3 = new DataReader(ms3);

            var items = new List<ExpressionUnk1_Base>();
            while (ms3.Position < ms3.Length)
            {
                var type = dr3.ReadByte();
                if (type == 0)
                {
                    if (ms3.Position != ms3.Length)
                    { }
                    break;
                }
                var item = CreateItem(type);
                item.Type = type;
                item.Read(dr1, dr2, dr3);
                items.Add(item);
            }
            Items = items.ToArray();

            if ((dr1.Length - dr1.Position) != 0)
            { }
            if ((dr2.Length - dr2.Position) != 0)
            { }
            if ((dr3.Length - dr3.Position) != 0)
            { }


        }


        public static ExpressionUnk1_Base CreateItem(byte type)
        {
            switch (type)
            {
                //case 0:
                case 0x21:
                case 0x30:
                case 0x3D:
                case 0x03:
                case 0x01:
                case 0x33:
                case 0x2E:
                case 0x31:
                case 0x32:
                case 0x2F:
                case 0x04:
                case 0x10:
                case 0x3B:
                case 0x36:
                case 0x35:
                case 0x39:
                case 0x38:
                case 0x37:
                case 0x1D:
                case 0x1E:
                case 0x46:
                case 0x1F:
                case 0x3A:
                case 0x22:
                case 0x49:
                case 0x3F:
                case 0x1B:
                case 0x40:
                case 0x3C:
                case 0x11:
                    return new ExpressionUnk1_Base();

                case 0x45:
                case 0x44:
                    return new ExpressionUnk1_S1();

                case 0x07:
                case 0x09:
                case 0x0A:
                case 0x23:
                case 0x26:
                case 0x27: 
                case 0x28:
                case 0x20:
                case 0x42:
                case 0x43:
                case 0x2A:
                case 0x06:
                    return new ExpressionUnk1_S2();

                case 0x2B:
                case 0x2C:
                case 0x2D:
                    return new ExpressionUnk1_2B2C2D();


                case 0x0B: return new ExpressionUnk1_0B();
                case 0x05: return new ExpressionUnk1_05();
                case 0x3E: return new ExpressionUnk1_3E();
                case 0x0E: return new ExpressionUnk1_0E();

            }
            return new ExpressionUnk1_Base();
        }


        public override string ToString()
        {
            return Unknown_0h.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ExpressionUnk1_Base
    {
        public byte Type { get; set; }
        public string TypeStr { get => Type.ToString("X").PadLeft(2, '0'); }

        public virtual void Read(DataReader r1, DataReader r2, DataReader r3)
        { }
        
        public override string ToString()
        {
            return TypeStr;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ExpressionUnk1_S1 : ExpressionUnk1_Base
    {
        public uint Length { get; set; }
        public uint ItemCount { get; set; }
        public uint ItemType { get; set; }
        public uint Unk1 { get; set; } // 0x00000000

        [TypeConverter(typeof(ExpandableObjectConverter))] public class Struct1
        {
            public ushort Unk1 { get; set; }//bone index?
            public ushort Unk2 { get; set; }

            public float[] Values { get; set; }

            public void Read(DataReader r)
            {
                Unk1 = r.ReadUInt16();
                Unk2 = r.ReadUInt16();

                switch (Unk2)
                {
                    case 0:
                    case 4:
                    case 8:
                        break;
                    default:
                        break;//no hit
                }
            }
            public override string ToString()
            {
                var str = Unk1.ToString() + ", " + Unk2.ToString();
                if (Values != null)
                {
                    str += "   -   ";
                    for (int i = 0; i < Values.Length; i++)
                    {
                        if (i != 0) str += ", ";
                        str += Values[i].ToString();
                    }
                }
                return str;
            }
        }
        public Struct1[] Items { get; set; }


        public override void Read(DataReader r1, DataReader r2, DataReader r3)
        {
            Length = r1.ReadUInt32();
            ItemCount = r1.ReadUInt32();
            ItemType = r1.ReadUInt32();
            Unk1 = r1.ReadUInt32();

            var valcnt = ((int)ItemType - 1) * 9 + 6;
            var hlen = (int)ItemCount * 4 + 16;
            var dlen = (int)Length - hlen;
            var tlen = (int)ItemCount * valcnt * 4;
            if (tlen != dlen)
            { }//no hitting


            Items = new Struct1[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                var item1 = new Struct1();
                item1.Read(r1);
                Items[i] = item1;
                Items[i].Values = new float[valcnt];
            }
            for (int n = 0; n < valcnt; n++)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    Items[i].Values[n] = r1.ReadSingle();
                }
            }


            switch (ItemCount)
            {
                case 0:
                case 4:
                case 8:
                case 12:
                case 20:
                case 24:
                case 48:
                case 52:
                case 76:
                case 32:
                case 16:
                case 40:
                case 44:
                case 60:
                case 72:
                case 56:
                case 28:
                case 68:
                    break;
                default:
                    break;//no hit
            }
            switch (ItemType)
            {
                case 1: break;
                case 2: break;
                case 3: break;
                case 4: break;
                default:
                    break;//no hit
            }
            if (Unk1 != 0)
            { }//no hit
        }
        public override string ToString()
        {
            var str = TypeStr + ", 1:   -   " + Length.ToString() + ", " + ItemCount.ToString() + ", " + ItemType.ToString();
            if (Items != null)
            {
                str += "   -   " + Items.Length.ToString() + " items";
            }
            return str;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ExpressionUnk1_S2 : ExpressionUnk1_Base
    {
        public ushort Unk00 { get; set; }
        public ushort Unk02 { get; set; }
        public byte Unk04 { get; set; }
        public byte Unk05 { get; set; }
        public byte Unk06 { get; set; }
        public byte Unk07 { get; set; }

        public override void Read(DataReader r1, DataReader r2, DataReader r3)
        {
            Unk00 = r2.ReadUInt16();
            Unk02 = r2.ReadUInt16();
            Unk04 = r2.ReadByte();
            Unk05 = r2.ReadByte();
            Unk06 = r2.ReadByte();
            Unk07 = r2.ReadByte();
        }
        public override string ToString()
        {
            return TypeStr + ", 2:   -   " + Unk00.ToString() + ", " + Unk02.ToString() + ", " + Unk04.ToString() + ", " + Unk05.ToString() + ", " + Unk06.ToString() + ", " + Unk07.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ExpressionUnk1_2B2C2D : ExpressionUnk1_Base
    {
        public uint UnkUint1 { get; set; }
        public uint UnkUint2 { get; set; }
        public uint UnkUint3 { get; set; }

        public override void Read(DataReader r1, DataReader r2, DataReader r3)
        {
            UnkUint1 = r2.ReadUInt32();
            UnkUint2 = r2.ReadUInt32();
            UnkUint3 = r2.ReadUInt32();

            switch (UnkUint1)//flags?
            {
                case 0:
                case 128:
                case 464:
                case 352:
                case 240:
                case 16:
                    break;
                default:
                    break;//no hit
            }
            switch (UnkUint2)//some index?
            {
                case 24:
                case 12:
                case 0:
                case 8:
                case 60:
                case 48:
                case 20:
                case 44:
                case 16:
                case 52:
                case 28:
                case 4:
                case 88:
                case 64:
                case 128:
                case 104:
                case 68:
                case 80:
                case 92:
                case 84:
                case 56:
                case 108:
                case 116:
                case 72:
                case 32:
                case 40:
                case 512:
                case 36:
                case 100:
                case 140:
                    break;
                default:
                    break;//no hit
            }
            switch (UnkUint3)//some index?
            {
                case 5:
                case 4:
                case 2:
                case 3:
                case 6:
                case 17:
                case 16:
                case 15:
                case 14:
                case 12:
                case 7:
                case 19:
                case 34:
                case 26:
                case 22:
                case 18:
                case 11:
                case 20:
                case 8:
                case 30:
                case 9:
                case 10:
                case 24:
                case 77:
                case 38:
                    break;
                default:
                    break;//no hit
            }

        }
        public override string ToString()
        {
            return TypeStr + ", 2:   -   " + UnkUint1.ToString() + ", " + UnkUint2.ToString() + ", " + UnkUint3.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ExpressionUnk1_0B : ExpressionUnk1_Base
    {
        public Vector4 UnkVec1 { get; set; }

        public override void Read(DataReader r1, DataReader r2, DataReader r3)
        {
            UnkVec1 = r1.ReadVector4();
        }

        public override string ToString()
        {
            var str = TypeStr + ", 1:   -   " + UnkVec1.ToString();
            return str;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ExpressionUnk1_05 : ExpressionUnk1_Base
    {
        public float UnkFloat { get; set; }

        public override void Read(DataReader r1, DataReader r2, DataReader r3)
        {
            UnkFloat = r2.ReadSingle();
        }

        public override string ToString()
        {
            return TypeStr + ", 2:   -   " + UnkFloat.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ExpressionUnk1_3E : ExpressionUnk1_Base
    {
        public Vector4 UnkVec1 { get; set; }
        public uint UnkUint1 { get; set; } // 0, 1, 2
        public uint UnkUint2 { get; set; } // 0, 2
        public uint UnkUint3 { get; set; } // 0, 2
        public uint UnkUint4 { get; set; } // 0x00000000

        public override void Read(DataReader r1, DataReader r2, DataReader r3)
        {
            UnkVec1 = r1.ReadVector4();
            UnkUint1 = r1.ReadUInt32();
            UnkUint2 = r1.ReadUInt32();
            UnkUint3 = r1.ReadUInt32();
            UnkUint4 = r1.ReadUInt32();

            switch (UnkUint1)
            {
                case 0:
                case 2:
                case 1:
                    break;
                default:
                    break;//no hit
            }
            switch (UnkUint2)
            {
                case 2:
                case 0:
                    break;
                default:
                    break;//no hit
            }
            switch (UnkUint3)
            {
                case 2:
                case 0:
                    break;
                default:
                    break;//no hit
            }
            switch (UnkUint4)
            {
                case 0:
                    break;
                default:
                    break;//no hit
            }
        }
        public override string ToString()
        {
            return TypeStr + ", 1:   -   " + UnkVec1.ToString() + "   -   " + UnkUint1.ToString() + ", " + UnkUint2.ToString() + ", " + UnkUint3.ToString() + ", " + UnkUint4.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ExpressionUnk1_0E : ExpressionUnk1_Base
    {
        public Vector3 UnkVec1 { get; set; }
        public MetaHash UnkUint1 { get; set; }
        public Vector3 UnkVec2 { get; set; }
        public MetaHash UnkUint2 { get; set; }
        public Vector3 UnkVec3 { get; set; }
        public MetaHash UnkUint3 { get; set; }
        public Vector3 UnkVec4 { get; set; }
        public MetaHash UnkUint4 { get; set; }
        public Vector3 UnkVec5 { get; set; }
        public MetaHash UnkUint5 { get; set; }
        public Vector3 UnkVec6 { get; set; }
        public MetaHash UnkUint6 { get; set; }
        public Vector3 UnkVec7 { get; set; }
        public MetaHash UnkUint7 { get; set; }
        public Vector3 UnkVec8 { get; set; }
        public MetaHash UnkUint8 { get; set; }
        public Vector3 UnkVec9 { get; set; }
        public MetaHash UnkUint9 { get; set; }
        public Vector3 UnkVec10 { get; set; }
        public MetaHash UnkUint10 { get; set; }
        public MetaHash UnkUint11 { get; set; }
        public MetaHash UnkUint12 { get; set; }
        public MetaHash UnkUint13 { get; set; }
        public MetaHash UnkUint14 { get; set; }

        public override void Read(DataReader r1, DataReader r2, DataReader r3)
        {
            UnkVec1 = r1.ReadVector3();
            UnkUint1 = r1.ReadUInt32();//2x short
            UnkVec2 = r1.ReadVector3();
            UnkUint2 = r1.ReadUInt32();//2x short
            UnkVec3 = r1.ReadVector3();
            UnkUint3 = r1.ReadUInt32();//2x short
            UnkVec4 = r1.ReadVector3();
            UnkUint4 = r1.ReadUInt32();//1x short, 2 bytes (first 0)
            UnkVec5 = r1.ReadVector3();//0
            UnkUint5 = r1.ReadUInt32();//1x short, 2 bytes (first 0)
            UnkVec6 = r1.ReadVector3();//0
            UnkUint6 = r1.ReadUInt32();//0
            UnkVec7 = r1.ReadVector3();//0
            UnkUint7 = r1.ReadUInt32();//1x short, 2 bytes (first 0)
            UnkVec8 = r1.ReadVector3();//0
            UnkUint8 = r1.ReadUInt32();//1x short (0), 2 bytes
            UnkVec9 = r1.ReadVector3();//down vector (0,0,-1)
            UnkUint9 = r1.ReadUInt32();//1x short, 2 bytes (first 0)
            UnkVec10 = r1.ReadVector3();//gravity vector? (0, 0, -9.8)
            UnkUint10 = r1.ReadUInt32();//2x short (2nd 0)
            UnkUint11 = r1.ReadUInt32();
            UnkUint12 = r1.ReadUInt32();
            UnkUint13 = r1.ReadUInt32();
            UnkUint14 = r1.ReadUInt32();
        }
        public override string ToString()
        {
            return TypeStr + ", 1:   -   " + UnkVec1.ToString() + ",   " + UnkVec2.ToString() + ",   " + UnkVec3.ToString() + ",   " + UnkVec4.ToString() + "   ...";
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ExpressionUnk2 : ResourceSystemBlock
    {
        public override long BlockLength => 0xA0;

        // structure data
        public float Unknown_00h { get; set; }
        public float Unknown_04h { get; set; }
        public float Unknown_08h { get; set; }
        public MetaHash Unknown_0Ch { get; set; }
        public float Unknown_10h { get; set; }
        public float Unknown_14h { get; set; }
        public float Unknown_18h { get; set; }
        public MetaHash Unknown_1Ch { get; set; }
        public float Unknown_20h { get; set; }
        public float Unknown_24h { get; set; }
        public float Unknown_28h { get; set; }
        public MetaHash Unknown_2Ch { get; set; }
        public float Unknown_30h { get; set; }
        public float Unknown_34h { get; set; }
        public float Unknown_38h { get; set; }
        public MetaHash Unknown_3Ch { get; set; }
        public float Unknown_40h { get; set; }
        public float Unknown_44h { get; set; }
        public float Unknown_48h { get; set; }
        public MetaHash Unknown_4Ch { get; set; }
        public float Unknown_50h { get; set; }
        public float Unknown_54h { get; set; }
        public float Unknown_58h { get; set; }
        public MetaHash Unknown_5Ch { get; set; }
        public float Unknown_60h { get; set; }
        public float Unknown_64h { get; set; }
        public float Unknown_68h { get; set; }
        public MetaHash Unknown_6Ch { get; set; }
        public float Unknown_70h { get; set; }
        public float Unknown_74h { get; set; }
        public float Unknown_78h { get; set; }
        public MetaHash Unknown_7Ch { get; set; }
        public float Unknown_80h { get; set; }
        public float Unknown_84h { get; set; }
        public float Unknown_88h { get; set; }
        public MetaHash Unknown_8Ch { get; set; }
        public float Unknown_90h { get; set; }
        public float Unknown_94h { get; set; }
        public float Unknown_98h { get; set; }
        public MetaHash Unknown_9Ch { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_00h = reader.ReadSingle();
            this.Unknown_04h = reader.ReadSingle();
            this.Unknown_08h = reader.ReadSingle();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadSingle();
            this.Unknown_14h = reader.ReadSingle();
            this.Unknown_18h = reader.ReadSingle();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadSingle();
            this.Unknown_24h = reader.ReadSingle();
            this.Unknown_28h = reader.ReadSingle();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadSingle();
            this.Unknown_34h = reader.ReadSingle();
            this.Unknown_38h = reader.ReadSingle();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadSingle();
            this.Unknown_44h = reader.ReadSingle();
            this.Unknown_48h = reader.ReadSingle();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadSingle();
            this.Unknown_54h = reader.ReadSingle();
            this.Unknown_58h = reader.ReadSingle();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.Unknown_60h = reader.ReadSingle();
            this.Unknown_64h = reader.ReadSingle();
            this.Unknown_68h = reader.ReadSingle();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Unknown_70h = reader.ReadSingle();
            this.Unknown_74h = reader.ReadSingle();
            this.Unknown_78h = reader.ReadSingle();
            this.Unknown_7Ch = reader.ReadUInt32();
            this.Unknown_80h = reader.ReadSingle();
            this.Unknown_84h = reader.ReadSingle();
            this.Unknown_88h = reader.ReadSingle();
            this.Unknown_8Ch = reader.ReadUInt32();
            this.Unknown_90h = reader.ReadSingle();
            this.Unknown_94h = reader.ReadSingle();
            this.Unknown_98h = reader.ReadSingle();
            this.Unknown_9Ch = reader.ReadUInt32();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_00h);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
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
        }
    }




    [TypeConverter(typeof(ExpandableObjectConverter))] public struct ExpressionUnk3
    {
        public ushort Unk0 { get; set; } // bone tag? need to check
        public byte Unk2 { get; set; } // animation track?
        public byte Unk3 { get; set; } // ..flags?

        public override string ToString()
        {
            return Unk0.ToString() + ", " + Unk2.ToString() + ", " + Unk3.ToString();
        }
    }


}
