/*
    Copyright(c) 2015 Neodymium

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

//shamelessly stolen and mangled


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CodeWalker.GameFiles
{



    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragType : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 304; }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public float Unknown_20h { get; set; }
        public float Unknown_24h { get; set; }
        public float Unknown_28h { get; set; }
        public float Unknown_2Ch { get; set; }
        public ulong DrawablePointer { get; set; }
        public ulong Unknown_28h_Pointer { get; set; }
        public ulong Unknown_30h_Pointer { get; set; }
        public uint Count0 { get; set; }
        public uint Unknown_4Ch { get; set; } //pointer? 
        public uint Unknown_50h { get; set; } // 0x00000000
        public uint Unknown_54h { get; set; } // 0x00000000
        public ulong NamePointer { get; set; }
        public ResourcePointerList64<FragCloth> Clothes { get; set; }
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public uint Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000
        public uint Unknown_80h { get; set; } // 0x00000000
        public uint Unknown_84h { get; set; } // 0x00000000
        public uint Unknown_88h { get; set; } // 0x00000000
        public uint Unknown_8Ch { get; set; } // 0x00000000
        public uint Unknown_90h { get; set; } // 0x00000000
        public uint Unknown_94h { get; set; } // 0x00000000
        public uint Unknown_98h { get; set; } // 0x00000000
        public uint Unknown_9Ch { get; set; } // 0x00000000
        public uint Unknown_A0h { get; set; } // 0x00000000
        public uint Unknown_A4h { get; set; } // 0x00000000
        public ulong Unknown_A8h_Pointer { get; set; }
        public uint Unknown_B0h { get; set; } // 0x00000000
        public uint Unknown_B4h { get; set; } // 0x00000000
        public uint Unknown_B8h { get; set; }
        public uint Unknown_BCh { get; set; }
        public uint Unknown_C0h { get; set; }
        public uint Unknown_C4h { get; set; }
        public uint Unknown_C8h { get; set; }//pointer?
        public float Unknown_CCh { get; set; }
        public float Unknown_D0h { get; set; }
        public float Unknown_D4h { get; set; }
        public byte Unknown_D8h { get; set; }
        public byte Count3 { get; set; }
        public ushort Unknown_DAh { get; set; }
        public uint Unknown_DCh { get; set; } // 0x00000000
        public ulong Unknown_E0h_Pointer { get; set; }
        public uint Unknown_E8h { get; set; } // 0x00000000
        public uint Unknown_ECh { get; set; } // 0x00000000
        public ulong PhysicsLODGroupPointer { get; set; }
        public ulong Unknown_F8h_Pointer { get; set; }
        public uint Unknown_100h { get; set; } // 0x00000000
        public uint Unknown_104h { get; set; } // 0x00000000
        public uint Unknown_108h { get; set; } // 0x00000000
        public uint Unknown_10Ch { get; set; } // 0x00000000
        //public ResourceSimpleList64<LightAttributes> LightAttributes { get; set; }
        public ResourceSimpleList64Ptr LightAttributesPtr { get; set; }
        public LightAttributes_s[] LightAttributes { get; set; }
        public ulong Unknown_120h_Pointer { get; set; }
        public uint Unknown_128h { get; set; } // 0x00000000
        public uint Unknown_12Ch { get; set; } // 0x00000000

        // reference data
        public FragDrawable Drawable { get; set; }
        public ResourcePointerArray64<FragDrawable> Unknown_28h_Data { get; set; }
        public ResourcePointerArray64<string_r> Unknown_30h_Data { get; set; }
        public string Name { get; set; }
        public FragUnknown_F_004 Unknown_A8h_Data { get; set; }
        public ResourcePointerArray64<FragUnknown_F_006> Unknown_E0h_Data { get; set; }
        public FragPhysicsLODGroup PhysicsLODGroup { get; set; }
        public FragDrawable Unknown_F8h_Data { get; set; }
        public FragUnknown_F_003 Unknown_120h_Data { get; set; }

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
            this.Unknown_20h = reader.ReadSingle();
            this.Unknown_24h = reader.ReadSingle();
            this.Unknown_28h = reader.ReadSingle();
            this.Unknown_2Ch = reader.ReadSingle();
            this.DrawablePointer = reader.ReadUInt64();
            this.Unknown_28h_Pointer = reader.ReadUInt64();
            this.Unknown_30h_Pointer = reader.ReadUInt64();
            this.Count0 = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.Clothes = reader.ReadBlock<ResourcePointerList64<FragCloth>>();
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
            this.Unknown_A8h_Pointer = reader.ReadUInt64();
            this.Unknown_B0h = reader.ReadUInt32();
            this.Unknown_B4h = reader.ReadUInt32();
            this.Unknown_B8h = reader.ReadUInt32();
            this.Unknown_BCh = reader.ReadUInt32();
            this.Unknown_C0h = reader.ReadUInt32();
            this.Unknown_C4h = reader.ReadUInt32();
            this.Unknown_C8h = reader.ReadUInt32();
            this.Unknown_CCh = reader.ReadSingle();
            this.Unknown_D0h = reader.ReadSingle();
            this.Unknown_D4h = reader.ReadSingle();
            this.Unknown_D8h = reader.ReadByte();
            this.Count3 = reader.ReadByte();
            this.Unknown_DAh = reader.ReadUInt16();
            this.Unknown_DCh = reader.ReadUInt32();
            this.Unknown_E0h_Pointer = reader.ReadUInt64();
            this.Unknown_E8h = reader.ReadUInt32();
            this.Unknown_ECh = reader.ReadUInt32();
            this.PhysicsLODGroupPointer = reader.ReadUInt64();
            this.Unknown_F8h_Pointer = reader.ReadUInt64();
            this.Unknown_100h = reader.ReadUInt32();
            this.Unknown_104h = reader.ReadUInt32();
            this.Unknown_108h = reader.ReadUInt32();
            this.Unknown_10Ch = reader.ReadUInt32();
            //this.LightAttributes = reader.ReadBlock<ResourceSimpleList64<LightAttributes>>();
            this.LightAttributesPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.LightAttributes = reader.ReadStructsAt<LightAttributes_s>(LightAttributesPtr.EntriesPointer, LightAttributesPtr.EntriesCount);
            this.Unknown_120h_Pointer = reader.ReadUInt64();
            this.Unknown_128h = reader.ReadUInt32();
            this.Unknown_12Ch = reader.ReadUInt32();

            // read reference data
            this.Drawable = reader.ReadBlockAt<FragDrawable>(
                this.DrawablePointer // offset
            );
            if (this.Drawable != null)
            {
                this.Drawable.OwnerFragment = this;
            }

            this.Unknown_28h_Data = reader.ReadBlockAt<ResourcePointerArray64<FragDrawable>>(
                this.Unknown_28h_Pointer, // offset
                this.Count0
            );
            if ((this.Unknown_28h_Data != null) && (this.Unknown_28h_Data.data_items != null))
            {
                for (int i = 0; i < this.Unknown_28h_Data.data_items.Length; i++)
                {
                    var drwbl = Unknown_28h_Data.data_items[i];
                    if (drwbl != null)
                    {
                        drwbl.OwnerFragment = this;
                    }
                }
            }
            this.Unknown_30h_Data = reader.ReadBlockAt<ResourcePointerArray64<string_r>>(
                this.Unknown_30h_Pointer, // offset
                this.Count0
            );
            this.Name = reader.ReadStringAt(//BlockAt<string_r>(
                this.NamePointer // offset
            );
            this.Unknown_A8h_Data = reader.ReadBlockAt<FragUnknown_F_004>(
                this.Unknown_A8h_Pointer // offset
            );
            this.Unknown_E0h_Data = reader.ReadBlockAt<ResourcePointerArray64<FragUnknown_F_006>>(
                this.Unknown_E0h_Pointer, // offset
                this.Count3
            );
            this.PhysicsLODGroup = reader.ReadBlockAt<FragPhysicsLODGroup>(
                this.PhysicsLODGroupPointer // offset
            );
            this.Unknown_F8h_Data = reader.ReadBlockAt<FragDrawable>(
                this.Unknown_F8h_Pointer // offset
            );
            if (this.Unknown_F8h_Data != null)
            {
                this.Unknown_F8h_Data.OwnerFragment = this;
            }

            this.Unknown_120h_Data = reader.ReadBlockAt<FragUnknown_F_003>(
                this.Unknown_120h_Pointer // offset
            );




            //for vehicle wheels, the shaderGroup in the model seems to be missing, but have to use the main drawable's shaders.
            if ((Drawable != null) && (PhysicsLODGroup != null) && (PhysicsLODGroup.PhysicsLOD1 != null))
            {
                var pl1 = PhysicsLODGroup.PhysicsLOD1;
                if ((pl1.Children != null) && (pl1.Children.data_items != null))
                {
                    for (int i = 0; i < pl1.Children.data_items.Length; i++)
                    {
                        var pch = pl1.Children.data_items[i];
                        if ((pch.Drawable1 != null))
                        {
                            pch.Drawable1.AssignGeometryShaders(Drawable.ShaderGroup);
                        }
                    }
                }
            }


        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.DrawablePointer = (ulong)(this.Drawable != null ? this.Drawable.FilePosition : 0);
            this.Unknown_28h_Pointer = (ulong)(this.Unknown_28h_Data != null ? this.Unknown_28h_Data.FilePosition : 0);
            this.Unknown_30h_Pointer = (ulong)(this.Unknown_30h_Data != null ? this.Unknown_30h_Data.FilePosition : 0);
            //this.cc00 = (uint)(this.pxxxxx_0data != null ? this.pxxxxx_0data.Count : 0);
            ////this.NamePointer = (ulong)(this.Name != null ? this.Name.Position : 0); //TODO: fix!!!
            //this.cnt1 = (ushort)(this.pxxxxx_2data != null ? this.pxxxxx_2data.Count : 0);
            this.Unknown_A8h_Pointer = (ulong)(this.Unknown_A8h_Data != null ? this.Unknown_A8h_Data.FilePosition : 0);
            //this.anotherCount = (byte)(this.pxxxxx_3data != null ? this.pxxxxx_3data.Count : 0);
            this.Unknown_E0h_Pointer = (ulong)(this.Unknown_E0h_Data != null ? this.Unknown_E0h_Data.FilePosition : 0);
            this.PhysicsLODGroupPointer = (ulong)(this.PhysicsLODGroup != null ? this.PhysicsLODGroup.FilePosition : 0);
            this.Unknown_F8h_Pointer = (ulong)(this.Unknown_F8h_Data != null ? this.Unknown_F8h_Data.FilePosition : 0);
            //this.cntxx51a = (ushort)(this.pxxxxx_5data != null ? this.pxxxxx_5data.Count : 0);
            this.Unknown_120h_Pointer = (ulong)(this.Unknown_120h_Data != null ? this.Unknown_120h_Data.FilePosition : 0);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.DrawablePointer);
            writer.Write(this.Unknown_28h_Pointer);
            writer.Write(this.Unknown_30h_Pointer);
            writer.Write(this.Count0);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.NamePointer);
            writer.WriteBlock(this.Clothes);
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
            writer.Write(this.Unknown_A8h_Pointer);
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
            writer.Write(this.Count3);
            writer.Write(this.Unknown_DAh);
            writer.Write(this.Unknown_DCh);
            writer.Write(this.Unknown_E0h_Pointer);
            writer.Write(this.Unknown_E8h);
            writer.Write(this.Unknown_ECh);
            writer.Write(this.PhysicsLODGroupPointer);
            writer.Write(this.Unknown_F8h_Pointer);
            writer.Write(this.Unknown_100h);
            writer.Write(this.Unknown_104h);
            writer.Write(this.Unknown_108h);
            writer.Write(this.Unknown_10Ch);
            //writer.WriteBlock(this.LightAttributes); //TODO: fix!
            writer.Write(this.Unknown_120h_Pointer);
            writer.Write(this.Unknown_128h);
            writer.Write(this.Unknown_12Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Drawable != null) list.Add(Drawable);
            if (Unknown_28h_Data != null) list.Add(Unknown_28h_Data);
            if (Unknown_30h_Data != null) list.Add(Unknown_30h_Data);
            //if (Name != null) list.Add(Name); //TODO: fix!
            if (Unknown_A8h_Data != null) list.Add(Unknown_A8h_Data);
            if (Unknown_E0h_Data != null) list.Add(Unknown_E0h_Data);
            if (PhysicsLODGroup != null) list.Add(PhysicsLODGroup);
            if (Unknown_F8h_Data != null) list.Add(Unknown_F8h_Data);
            if (Unknown_120h_Data != null) list.Add(Unknown_120h_Data);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x60, Clothes),
                //new Tuple<long, IResourceBlock>(0x110, LightAttributes) //TODO: fix!
            };
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragCloth : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 128; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
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
        public FragClothInstanceTuning InstanceTuning { get; set; }
        public FragDrawable Drawable { get; set; }
        public FragClothController Controller { get; set; }
        public uint[] pxxxxx_2data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
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
            this.InstanceTuning = reader.ReadBlockAt<FragClothInstanceTuning>(
                this.InstanceTuningPointer // offset
            );
            this.Drawable = reader.ReadBlockAt<FragDrawable>(
                this.DrawablePointer // offset
            );
            if (this.Drawable != null)
            {
                this.Drawable.OwnerFragmentCloth = this;
            }

            this.Controller = reader.ReadBlockAt<FragClothController>(
                this.ControllerPointer // offset
            );
            //this.pxxxxx_2data = reader.ReadBlockAt<ResourceSimpleArray<uint_r>>(
            //    this.pxxxxx_2, // offset
            //    this.cntxx51a
            //);
            this.pxxxxx_2data = reader.ReadUintsAt(this.pxxxxx_2, this.cntxx51a);

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
            //this.pxxxxx_2 = (ulong)(this.pxxxxx_2data != null ? this.pxxxxx_2data.Position : 0); //TODO: fix
            //this.cntxx51a = (ushort)(this.pxxxxx_2data != null ? this.pxxxxx_2data.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
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
            //if (pxxxxx_2data != null) list.Add(pxxxxx_2data); //TODO: fix
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragClothInstanceTuning : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
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
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
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
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragClothController : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 128; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public ulong BridgeSimGfxPointer { get; set; }  // pointer
        public ulong MorphControllerPointer { get; set; }  // pointer
        public ulong VerletCloth1Pointer { get; set; }  // pointer
        public ulong VerletCloth2Pointer { get; set; }  // pointer
        public ulong VerletCloth3Pointer { get; set; }  // pointer
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000
        public uint Unknown_40h { get; set; } // 0x00000000
        public uint Unknown_44h { get; set; } // 0x00000000
        public uint Unknown_48h { get; set; } // 0x00000000
        public uint Unknown_4Ch { get; set; } // 0x00000000
        public uint Unknown_50h { get; set; } // 0x00000003
        public uint Unknown_54h { get; set; } // 0x00000000
        public uint Unknown_58h { get; set; }  // no float
        public uint Unknown_5Ch { get; set; }  // no float
        public uint Unknown_60h { get; set; }  // no float
        public uint Unknown_64h { get; set; }  // no float
        public uint Unknown_68h { get; set; }  // no float
        public uint Unknown_6Ch { get; set; }  // no float
        public uint Unknown_70h { get; set; }  // no float
        public uint Unknown_74h { get; set; } // 0x00000000
        public uint Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000

        // reference data
        public FragClothBridgeSimGfx BridgeSimGfx { get; set; }
        public FragClothMorphController MorphController { get; set; }
        public FragClothVerletCloth VerletCloth1 { get; set; }
        public FragClothVerletCloth VerletCloth2 { get; set; }
        public FragClothVerletCloth VerletCloth3 { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
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
            this.BridgeSimGfx = reader.ReadBlockAt<FragClothBridgeSimGfx>(
                this.BridgeSimGfxPointer // offset
            );
            this.MorphController = reader.ReadBlockAt<FragClothMorphController>(
                this.MorphControllerPointer // offset
            );
            this.VerletCloth1 = reader.ReadBlockAt<FragClothVerletCloth>(
                this.VerletCloth1Pointer // offset
            );
            this.VerletCloth2 = reader.ReadBlockAt<FragClothVerletCloth>(
                this.VerletCloth2Pointer // offset
            );
            this.VerletCloth3 = reader.ReadBlockAt<FragClothVerletCloth>(
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
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
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
            if (BridgeSimGfx != null) list.Add(BridgeSimGfx);
            if (MorphController != null) list.Add(MorphController);
            if (VerletCloth1 != null) list.Add(VerletCloth1);
            if (VerletCloth2 != null) list.Add(VerletCloth2);
            if (VerletCloth3 != null) list.Add(VerletCloth3);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragClothBridgeSimGfx : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 320; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x00000000
        //public ResourceSimpleList64<uint_r> Unknown_20h { get; set; }
        //public ResourceSimpleList64<uint_r> Unknown_30h { get; set; }
        //public ResourceSimpleList64<uint_r> Unknown_40h { get; set; }
        public ResourceSimpleList64Ptr Unknown_20hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_30hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_40hPtr { get; set; }
        public float[] Unknown_20h { get; set; }
        public float[] Unknown_30h { get; set; }
        public float[] Unknown_40h { get; set; }
        public uint Unknown_50h { get; set; } // 0x00000000
        public uint Unknown_54h { get; set; } // 0x00000000
        public uint Unknown_58h { get; set; } // 0x00000000
        public uint Unknown_5Ch { get; set; } // 0x00000000
        //public ResourceSimpleList64<float_r> Unknown_60h { get; set; }
        //public ResourceSimpleList64<uint_r> Unknown_70h { get; set; }
        //public ResourceSimpleList64<uint_r> Unknown_80h { get; set; }
        public ResourceSimpleList64Ptr Unknown_60hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_70hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_80hPtr { get; set; }
        public float[] Unknown_60h { get; set; }
        public uint[] Unknown_70h { get; set; }
        public uint[] Unknown_80h { get; set; }
        public uint Unknown_90h { get; set; } // 0x00000000
        public uint Unknown_94h { get; set; } // 0x00000000
        public uint Unknown_98h { get; set; } // 0x00000000
        public uint Unknown_9Ch { get; set; } // 0x00000000
        //public ResourceSimpleList64<float_r> Unknown_A0h { get; set; }
        //public ResourceSimpleList64<uint_r> Unknown_B0h { get; set; }
        //public ResourceSimpleList64<uint_r> Unknown_C0h { get; set; }
        public ResourceSimpleList64Ptr Unknown_A0hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_B0hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_C0hPtr { get; set; }
        public float[] Unknown_A0h { get; set; }
        public uint[] Unknown_B0h { get; set; }
        public uint[] Unknown_C0h { get; set; }
        public uint Unknown_D0h { get; set; } // 0x00000000
        public uint Unknown_D4h { get; set; } // 0x00000000
        public uint Unknown_D8h { get; set; } // 0x00000000
        public uint Unknown_DCh { get; set; } // 0x00000000
        //public ResourceSimpleList64<ushort_r> Unknown_E0h { get; set; }
        //public ResourceSimpleList64<ushort_r> Unknown_F0h { get; set; }
        //public ResourceSimpleList64<ushort_r> Unknown_100h { get; set; }
        public ResourceSimpleList64Ptr Unknown_E0hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_F0hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_100hPtr { get; set; }
        public ushort[] Unknown_E0h { get; set; }
        public ushort[] Unknown_F0h { get; set; }
        public ushort[] Unknown_100h { get; set; }
        public uint Unknown_110h { get; set; } // 0x00000000
        public uint Unknown_114h { get; set; } // 0x00000000
        public uint Unknown_118h { get; set; } // 0x00000000
        public uint Unknown_11Ch { get; set; } // 0x00000000
        public uint Unknown_120h { get; set; } // 0x00000000
        public uint Unknown_124h { get; set; } // 0x00000000
        //public ResourceSimpleList64<uint_r> Unknown_128h { get; set; }
        public ResourceSimpleList64Ptr Unknown_128hPtr { get; set; }
        public uint[] Unknown_128h { get; set; }
        public uint Unknown_138h { get; set; } // 0x00000000
        public uint Unknown_13Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_30hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_40hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_20h = reader.ReadFloatsAt(Unknown_20hPtr.EntriesPointer, Unknown_20hPtr.EntriesCount);
            this.Unknown_30h = reader.ReadFloatsAt(Unknown_30hPtr.EntriesPointer, Unknown_30hPtr.EntriesCount);
            this.Unknown_40h = reader.ReadFloatsAt(Unknown_40hPtr.EntriesPointer, Unknown_40hPtr.EntriesCount);
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.Unknown_58h = reader.ReadUInt32();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.Unknown_60hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_70hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_80hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_60h = reader.ReadFloatsAt(Unknown_60hPtr.EntriesPointer, Unknown_60hPtr.EntriesCount);
            this.Unknown_70h = reader.ReadUintsAt(Unknown_70hPtr.EntriesPointer, Unknown_70hPtr.EntriesCount);
            this.Unknown_80h = reader.ReadUintsAt(Unknown_80hPtr.EntriesPointer, Unknown_80hPtr.EntriesCount);
            this.Unknown_90h = reader.ReadUInt32();
            this.Unknown_94h = reader.ReadUInt32();
            this.Unknown_98h = reader.ReadUInt32();
            this.Unknown_9Ch = reader.ReadUInt32();
            this.Unknown_A0hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_B0hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_C0hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_A0h = reader.ReadFloatsAt(Unknown_A0hPtr.EntriesPointer, Unknown_A0hPtr.EntriesCount);
            this.Unknown_B0h = reader.ReadUintsAt(Unknown_B0hPtr.EntriesPointer, Unknown_B0hPtr.EntriesCount);
            this.Unknown_C0h = reader.ReadUintsAt(Unknown_C0hPtr.EntriesPointer, Unknown_C0hPtr.EntriesCount);
            this.Unknown_D0h = reader.ReadUInt32();
            this.Unknown_D4h = reader.ReadUInt32();
            this.Unknown_D8h = reader.ReadUInt32();
            this.Unknown_DCh = reader.ReadUInt32();
            this.Unknown_E0hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_F0hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_100hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_E0h = reader.ReadUshortsAt(Unknown_E0hPtr.EntriesPointer, Unknown_E0hPtr.EntriesCount);
            this.Unknown_F0h = reader.ReadUshortsAt(Unknown_F0hPtr.EntriesPointer, Unknown_F0hPtr.EntriesCount);
            this.Unknown_100h = reader.ReadUshortsAt(Unknown_100hPtr.EntriesPointer, Unknown_100hPtr.EntriesCount);
            this.Unknown_110h = reader.ReadUInt32();
            this.Unknown_114h = reader.ReadUInt32();
            this.Unknown_118h = reader.ReadUInt32();
            this.Unknown_11Ch = reader.ReadUInt32();
            this.Unknown_120h = reader.ReadUInt32();
            this.Unknown_124h = reader.ReadUInt32();
            this.Unknown_128hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_128h = reader.ReadUintsAt(Unknown_128hPtr.EntriesPointer, Unknown_128hPtr.EntriesCount);
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
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            //writer.WriteBlock(this.Unknown_20h); //TODO: fix!
            //writer.WriteBlock(this.Unknown_30h);
            //writer.WriteBlock(this.Unknown_40h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
            //writer.WriteBlock(this.Unknown_60h);
            //writer.WriteBlock(this.Unknown_70h);
            //writer.WriteBlock(this.Unknown_80h);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_94h);
            writer.Write(this.Unknown_98h);
            writer.Write(this.Unknown_9Ch);
            //writer.WriteBlock(this.Unknown_A0h);
            //writer.WriteBlock(this.Unknown_B0h);
            //writer.WriteBlock(this.Unknown_C0h);
            writer.Write(this.Unknown_D0h);
            writer.Write(this.Unknown_D4h);
            writer.Write(this.Unknown_D8h);
            writer.Write(this.Unknown_DCh);
            //writer.WriteBlock(this.Unknown_E0h);
            //writer.WriteBlock(this.Unknown_F0h);
            //writer.WriteBlock(this.Unknown_100h);
            writer.Write(this.Unknown_110h);
            writer.Write(this.Unknown_114h);
            writer.Write(this.Unknown_118h);
            writer.Write(this.Unknown_11Ch);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_124h);
            //writer.WriteBlock(this.Unknown_128h);
            writer.Write(this.Unknown_138h);
            writer.Write(this.Unknown_13Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                //new Tuple<long, IResourceBlock>(0x20, Unknown_20h), //TODO: fix!
                //new Tuple<long, IResourceBlock>(0x30, Unknown_30h),
                //new Tuple<long, IResourceBlock>(0x40, Unknown_40h),
                //new Tuple<long, IResourceBlock>(0x60, Unknown_60h),
                //new Tuple<long, IResourceBlock>(0x70, Unknown_70h),
                //new Tuple<long, IResourceBlock>(0x80, Unknown_80h),
                //new Tuple<long, IResourceBlock>(0xA0, Unknown_A0h),
                //new Tuple<long, IResourceBlock>(0xB0, Unknown_B0h),
                //new Tuple<long, IResourceBlock>(0xC0, Unknown_C0h),
                //new Tuple<long, IResourceBlock>(0xE0, Unknown_E0h),
                //new Tuple<long, IResourceBlock>(0xF0, Unknown_F0h),
                //new Tuple<long, IResourceBlock>(0x100, Unknown_100h),
                //new Tuple<long, IResourceBlock>(0x128, Unknown_128h)
            };
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragClothMorphController : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
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
        public FragClothUnknown_F_007 Unknown_18h_Data { get; set; }
        public FragClothUnknown_F_007 Unknown_20h_Data { get; set; }
        public FragClothUnknown_F_007 Unknown_28h_Data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
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
            this.Unknown_18h_Data = reader.ReadBlockAt<FragClothUnknown_F_007>(
                this.Unknown_18h_Pointer // offset
            );
            this.Unknown_20h_Data = reader.ReadBlockAt<FragClothUnknown_F_007>(
                this.Unknown_20h_Pointer // offset
            );
            this.Unknown_28h_Data = reader.ReadBlockAt<FragClothUnknown_F_007>(
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
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
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
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragClothUnknown_F_007 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 400; }
        }

        // structure data
        public uint Unknown_00h { get; set; } // 0x00000000
        public uint Unknown_04h { get; set; } // 0x00000000
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
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
        //public ResourceSimpleList64<Vector4_r> Unknown_50h { get; set; }
        //public ResourceSimpleList64<ushort_r> Unknown_60h { get; set; }
        //public ResourceSimpleList64<ushort_r> Unknown_70h { get; set; }
        //public ResourceSimpleList64<ushort_r> Unknown_80h { get; set; }
        //public ResourceSimpleList64<ushort_r> Unknown_90h { get; set; }
        //public ResourceSimpleList64<Vector4_r> Unknown_A0h { get; set; }
        //public ResourceSimpleList64<ushort_r> Unknown_B0h { get; set; }
        //public ResourceSimpleList64<ushort_r> Unknown_C0h { get; set; }
        //public ResourceSimpleList64<ushort_r> Unknown_D0h { get; set; }
        //public ResourceSimpleList64<ushort_r> Unknown_E0h { get; set; }
        public ResourceSimpleList64Ptr Unknown_50hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_60hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_70hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_80hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_90hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_A0hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_B0hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_C0hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_D0hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_E0hPtr { get; set; }
        public SharpDX.Vector4[] Unknown_50h { get; set; }
        public ushort[] Unknown_60h { get; set; }
        public ushort[] Unknown_70h { get; set; }
        public ushort[] Unknown_80h { get; set; }
        public ushort[] Unknown_90h { get; set; }
        public SharpDX.Vector4[] Unknown_A0h { get; set; }
        public ushort[] Unknown_B0h { get; set; }
        public ushort[] Unknown_C0h { get; set; }
        public ushort[] Unknown_D0h { get; set; }
        public ushort[] Unknown_E0h { get; set; }
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
        //public ResourceSimpleList64<ushort_r> Unknown_150h { get; set; }
        //public ResourceSimpleList64<ushort_r> Unknown_160h { get; set; }
        public ResourceSimpleList64Ptr Unknown_150hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_160hPtr { get; set; }
        public ushort[] Unknown_150h { get; set; }
        public ushort[] Unknown_160h { get; set; }
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
            this.Unknown_00h = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
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
            //this.Unknown_50h = reader.ReadBlock<ResourceSimpleList64<Vector4_r>>();
            //this.Unknown_60h = reader.ReadBlock<ResourceSimpleList64<ushort_r>>();
            //this.Unknown_70h = reader.ReadBlock<ResourceSimpleList64<ushort_r>>();
            //this.Unknown_80h = reader.ReadBlock<ResourceSimpleList64<ushort_r>>();
            //this.Unknown_90h = reader.ReadBlock<ResourceSimpleList64<ushort_r>>();
            //this.Unknown_A0h = reader.ReadBlock<ResourceSimpleList64<Vector4_r>>();
            //this.Unknown_B0h = reader.ReadBlock<ResourceSimpleList64<ushort_r>>();
            //this.Unknown_C0h = reader.ReadBlock<ResourceSimpleList64<ushort_r>>();
            //this.Unknown_D0h = reader.ReadBlock<ResourceSimpleList64<ushort_r>>();
            //this.Unknown_E0h = reader.ReadBlock<ResourceSimpleList64<ushort_r>>();
            this.Unknown_50hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_60hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_70hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_80hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_90hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_A0hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_B0hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_C0hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_D0hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_E0hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_50h = reader.ReadStructsAt<SharpDX.Vector4>(Unknown_50hPtr.EntriesPointer, Unknown_50hPtr.EntriesCount);
            this.Unknown_60h = reader.ReadUshortsAt(Unknown_60hPtr.EntriesPointer, Unknown_60hPtr.EntriesCount);
            this.Unknown_70h = reader.ReadUshortsAt(Unknown_70hPtr.EntriesPointer, Unknown_70hPtr.EntriesCount);
            this.Unknown_80h = reader.ReadUshortsAt(Unknown_80hPtr.EntriesPointer, Unknown_80hPtr.EntriesCount);
            this.Unknown_90h = reader.ReadUshortsAt(Unknown_90hPtr.EntriesPointer, Unknown_90hPtr.EntriesCount);
            this.Unknown_A0h = reader.ReadStructsAt<SharpDX.Vector4>(Unknown_A0hPtr.EntriesPointer, Unknown_A0hPtr.EntriesCount);
            this.Unknown_B0h = reader.ReadUshortsAt(Unknown_B0hPtr.EntriesPointer, Unknown_B0hPtr.EntriesCount);
            this.Unknown_C0h = reader.ReadUshortsAt(Unknown_C0hPtr.EntriesPointer, Unknown_C0hPtr.EntriesCount);
            this.Unknown_D0h = reader.ReadUshortsAt(Unknown_D0hPtr.EntriesPointer, Unknown_D0hPtr.EntriesCount);
            this.Unknown_E0h = reader.ReadUshortsAt(Unknown_E0hPtr.EntriesPointer, Unknown_E0hPtr.EntriesCount);
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
            //this.Unknown_150h = reader.ReadBlock<ResourceSimpleList64<ushort_r>>();
            //this.Unknown_160h = reader.ReadBlock<ResourceSimpleList64<ushort_r>>();
            this.Unknown_150hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_160hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_150h = reader.ReadUshortsAt(Unknown_150hPtr.EntriesPointer, Unknown_150hPtr.EntriesCount);
            this.Unknown_160h = reader.ReadUshortsAt(Unknown_160hPtr.EntriesPointer, Unknown_160hPtr.EntriesCount);
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
            //writer.WriteBlock(this.Unknown_50h); //TODO: fix this!
            //writer.WriteBlock(this.Unknown_60h);
            //writer.WriteBlock(this.Unknown_70h);
            //writer.WriteBlock(this.Unknown_80h);
            //writer.WriteBlock(this.Unknown_90h);
            //writer.WriteBlock(this.Unknown_A0h);
            //writer.WriteBlock(this.Unknown_B0h);
            //writer.WriteBlock(this.Unknown_C0h);
            //writer.WriteBlock(this.Unknown_D0h);
            //writer.WriteBlock(this.Unknown_E0h);
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
            //writer.WriteBlock(this.Unknown_150h); //TODO: fix
            //writer.WriteBlock(this.Unknown_160h);
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
                //new Tuple<long, IResourceBlock>(0x50, Unknown_50h), //TODO: fix this
                //new Tuple<long, IResourceBlock>(0x60, Unknown_60h),
                //new Tuple<long, IResourceBlock>(0x70, Unknown_70h),
                //new Tuple<long, IResourceBlock>(0x80, Unknown_80h),
                //new Tuple<long, IResourceBlock>(0x90, Unknown_90h),
                //new Tuple<long, IResourceBlock>(0xA0, Unknown_A0h),
                //new Tuple<long, IResourceBlock>(0xB0, Unknown_B0h),
                //new Tuple<long, IResourceBlock>(0xC0, Unknown_C0h),
                //new Tuple<long, IResourceBlock>(0xD0, Unknown_D0h),
                //new Tuple<long, IResourceBlock>(0xE0, Unknown_E0h),
                //new Tuple<long, IResourceBlock>(0x150, Unknown_150h),
                //new Tuple<long, IResourceBlock>(0x160, Unknown_160h)
            };
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragClothVerletCloth : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 384; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public ulong BoundsPointer { get; set; }
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
        public uint Unknown_54h { get; set; } // 0x00000001
        public uint Unknown_58h { get; set; } // 0x00000000
        public uint Unknown_5Ch { get; set; } // 0x00000000
        public uint Unknown_60h { get; set; } // 0x00000000
        public uint Unknown_64h { get; set; } // 0x00000000
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000
        public ResourceSimpleList64Ptr Unknown_70hPtr { get; set; }
        public SharpDX.Vector4[] Unknown_70h { get; set; }
        //public uint Unknown_70h { get; set; } // 0x00000000
        //public uint Unknown_74h { get; set; } // 0x00000000
        //public uint Unknown_78h { get; set; } // 0x00000000
        //public uint Unknown_7Ch { get; set; } // 0x00000000
        //public ResourceSimpleList64<Vector4_r> Unknown_80h { get; set; }
        public ResourceSimpleList64Ptr Unknown_80hPtr { get; set; }
        public SharpDX.Vector4[] Unknown_80h { get; set; }
        public uint Unknown_90h { get; set; } // 0x00000000
        public uint Unknown_94h { get; set; } // 0x00000000
        public uint Unknown_98h { get; set; } // 0x00000000
        public uint Unknown_9Ch { get; set; } // 0x00000000
        public uint Unknown_A0h { get; set; } // 0x00000000
        public uint Unknown_A4h { get; set; } // 0x00000000
        public uint Unknown_A8h { get; set; }
        public uint Unknown_ACh { get; set; }
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
        //public ResourceSimpleList64<Vector4_r> Unknown_100h { get; set; }
        //public ResourceSimpleList64<Vector4_r> Unknown_110h { get; set; }
        public ResourceSimpleList64Ptr Unknown_100hPtr { get; set; }
        public ResourceSimpleList64Ptr Unknown_110hPtr { get; set; }
        public SharpDX.Vector4[] Unknown_100h { get; set; }
        public SharpDX.Vector4[] Unknown_110h { get; set; }
        public uint Unknown_120h { get; set; } // 0x00000000
        public uint Unknown_124h { get; set; } // 0x00000000
        public uint Unknown_128h { get; set; } // 0x00000000
        public uint Unknown_12Ch { get; set; } // 0x00000000
        public ulong BehaviorPointer { get; set; }
        public uint Unknown_138h { get; set; } // 0x00100000
        public uint Unknown_13Ch { get; set; } // 0x00000000
        public ulong Unknown_140h_Pointer { get; set; }
        public uint Unknown_148h { get; set; }
        public uint Unknown_14Ch { get; set; } // 0x00000000
        public uint Unknown_150h { get; set; } // 0x00000000
        public uint Unknown_154h { get; set; } // 0x00000000
        public uint Unknown_158h { get; set; }
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
        public Bounds Bounds { get; set; }
        public FragClothVerletBehavior Behavior { get; set; }
        public FragClothUnknown_F_023 Unknown_140h_Data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.BoundsPointer = reader.ReadUInt64();
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
            this.Unknown_70hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_70h = reader.ReadStructsAt<SharpDX.Vector4>(Unknown_70hPtr.EntriesPointer, Unknown_70hPtr.EntriesCount);
            //this.Unknown_70h = reader.ReadUInt32();
            //this.Unknown_74h = reader.ReadUInt32();
            //this.Unknown_78h = reader.ReadUInt32();
            //this.Unknown_7Ch = reader.ReadUInt32();
            //this.Unknown_80h = reader.ReadBlock<ResourceSimpleList64<Vector4_r>>();
            this.Unknown_80hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_80h = reader.ReadStructsAt<SharpDX.Vector4>(Unknown_80hPtr.EntriesPointer, Unknown_80hPtr.EntriesCount);
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
            //this.Unknown_100h = reader.ReadBlock<ResourceSimpleList64<Vector4_r>>();
            //this.Unknown_110h = reader.ReadBlock<ResourceSimpleList64<Vector4_r>>();
            this.Unknown_100hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_110hPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.Unknown_100h = reader.ReadStructsAt<SharpDX.Vector4>(Unknown_100hPtr.EntriesPointer, Unknown_100hPtr.EntriesCount);
            this.Unknown_110h = reader.ReadStructsAt<SharpDX.Vector4>(Unknown_110hPtr.EntriesPointer, Unknown_110hPtr.EntriesCount);
            this.Unknown_120h = reader.ReadUInt32();
            this.Unknown_124h = reader.ReadUInt32();
            this.Unknown_128h = reader.ReadUInt32();
            this.Unknown_12Ch = reader.ReadUInt32();
            this.BehaviorPointer = reader.ReadUInt64();
            this.Unknown_138h = reader.ReadUInt32();
            this.Unknown_13Ch = reader.ReadUInt32();
            this.Unknown_140h_Pointer = reader.ReadUInt64();
            this.Unknown_148h = reader.ReadUInt32();
            this.Unknown_14Ch = reader.ReadUInt32();
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

            // read reference data
            this.Bounds = reader.ReadBlockAt<Bounds>(
                this.BoundsPointer // offset
            );
            this.Behavior = reader.ReadBlockAt<FragClothVerletBehavior>(
                this.BehaviorPointer // offset
            );
            this.Unknown_140h_Data = reader.ReadBlockAt<FragClothUnknown_F_023>(
                this.Unknown_140h_Pointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.BoundsPointer = (ulong)(this.Bounds != null ? this.Bounds.FilePosition : 0);
            this.BehaviorPointer = (ulong)(this.Behavior != null ? this.Behavior.FilePosition : 0);
            this.Unknown_140h_Pointer = (ulong)(this.Unknown_140h_Data != null ? this.Unknown_140h_Data.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.BoundsPointer);
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
            //writer.Write(this.Unknown_70h);
            //writer.Write(this.Unknown_74h);
            //writer.Write(this.Unknown_78h);
            //writer.Write(this.Unknown_7Ch);
            //writer.WriteBlock(this.Unknown_80h); //TODO: fix
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
            //writer.WriteBlock(this.Unknown_100h); //TODO: fix
            //writer.WriteBlock(this.Unknown_110h);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_124h);
            writer.Write(this.Unknown_128h);
            writer.Write(this.Unknown_12Ch);
            writer.Write(this.BehaviorPointer);
            writer.Write(this.Unknown_138h);
            writer.Write(this.Unknown_13Ch);
            writer.Write(this.Unknown_140h_Pointer);
            writer.Write(this.Unknown_148h);
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
            if (Bounds != null) list.Add(Bounds);
            if (Behavior != null) list.Add(Behavior);
            if (Unknown_140h_Data != null) list.Add(Unknown_140h_Data);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                //new Tuple<long, IResourceBlock>(0x80, Unknown_80h), //TODO: fix
                //new Tuple<long, IResourceBlock>(0x100, Unknown_100h),
                //new Tuple<long, IResourceBlock>(0x110, Unknown_110h)
            };
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragClothVerletBehavior : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint Unknown_00h { get; set; } // 0x00000000
        public uint Unknown_04h { get; set; } // 0x00000000
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
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
            this.Unknown_00h = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
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
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragClothUnknown_F_023 : ResourceSystemBlock
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
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragDrawable : DrawableBase
    {
        public override long BlockLength
        {
            get { return 336; }
        }

        // structure data
        public uint Unknown_0A8h { get; set; }
        public uint Unknown_0ACh { get; set; }
        public SharpDX.Matrix Unknown_0B0h { get; set; }
        public ulong BoundPointer { get; set; }
        public ulong Unknown_0F8h_Pointer { get; set; }
        public ushort Count1 { get; set; }
        public ushort Count2 { get; set; }
        public uint Unknown_104h { get; set; } // 0x00000000
        public ulong Unknown_108h_Pointer { get; set; }
        public ushort Count3 { get; set; }
        public ushort Count4 { get; set; }
        public uint Unknown_114h { get; set; } // 0x00000000
        public uint Unknown_118h { get; set; } // 0x00000000
        public uint Unknown_11Ch { get; set; } // 0x00000000
        public uint Unknown_120h { get; set; } // 0x00000000
        public uint Unknown_124h { get; set; } // 0x00000000
        public uint Unknown_128h { get; set; } // 0x00000000
        public uint Unknown_12Ch { get; set; } // 0x00000000
        public ulong NamePointer { get; set; }
        public uint Unknown_138h { get; set; } // 0x00000000
        public uint Unknown_13Ch { get; set; } // 0x00000000
        public uint Unknown_140h { get; set; } // 0x00000000
        public uint Unknown_144h { get; set; } // 0x00000000
        public uint Unknown_148h { get; set; } // 0x00000000
        public uint Unknown_14Ch { get; set; } // 0x00000000

        // reference data
        public Bounds Bound { get; set; }
        //public ResourceSimpleArray<ulong_r> Unknown_F8h_Data { get; set; }
        public ulong[] Unknown_F8h_Data { get; set; }
        //public ResourceSimpleArray<Matrix4_r> Unknown_108h_Data { get; set; }
        public SharpDX.Matrix[] Unknown_108h_Data { get; set; }
        public string Name { get; set; }

        public FragType OwnerFragment { get; set; } //for handy use
        public FragCloth OwnerFragmentCloth { get; set; }
        public FragPhysTypeChild OwnerFragmentPhys { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_0A8h = reader.ReadUInt32();
            this.Unknown_0ACh = reader.ReadUInt32();
            this.Unknown_0B0h = reader.ReadStruct<SharpDX.Matrix>();
            this.BoundPointer = reader.ReadUInt64();
            this.Unknown_0F8h_Pointer = reader.ReadUInt64();
            this.Count1 = reader.ReadUInt16();
            this.Count2 = reader.ReadUInt16();
            this.Unknown_104h = reader.ReadUInt32();
            this.Unknown_108h_Pointer = reader.ReadUInt64();
            this.Count3 = reader.ReadUInt16();
            this.Count4 = reader.ReadUInt16();
            this.Unknown_114h = reader.ReadUInt32();
            this.Unknown_118h = reader.ReadUInt32();
            this.Unknown_11Ch = reader.ReadUInt32();
            this.Unknown_120h = reader.ReadUInt32();
            this.Unknown_124h = reader.ReadUInt32();
            this.Unknown_128h = reader.ReadUInt32();
            this.Unknown_12Ch = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.Unknown_138h = reader.ReadUInt32();
            this.Unknown_13Ch = reader.ReadUInt32();
            this.Unknown_140h = reader.ReadUInt32();
            this.Unknown_144h = reader.ReadUInt32();
            this.Unknown_148h = reader.ReadUInt32();
            this.Unknown_14Ch = reader.ReadUInt32();

            // read reference data
            this.Bound = reader.ReadBlockAt<Bounds>(
                this.BoundPointer // offset
            );
            //this.Unknown_F8h_Data = reader.ReadBlockAt<ResourceSimpleArray<ulong_r>>(
            //    this.Unknown_F8h_Pointer, // offset
            //    this.Count1
            //);
            this.Unknown_F8h_Data = reader.ReadUlongsAt(this.Unknown_0F8h_Pointer, this.Count1);

            //this.Unknown_108h_Data = reader.ReadBlockAt<ResourceSimpleArray<Matrix4_r>>(
            //    this.Unknown_108h_Pointer, // offset
            //    this.Count2
            //);
            this.Unknown_108h_Data = reader.ReadStructsAt<SharpDX.Matrix>(this.Unknown_108h_Pointer, this.Count2);

            this.Name = reader.ReadStringAt(//BlockAt<string_r>(
                this.NamePointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);
            //this.Unknown_F8h_Pointer = (ulong)(this.Unknown_F8h_Data != null ? this.Unknown_F8h_Data.Position : 0); //TODO: fix
            //this.c1qqq = (ushort)(this.pxx2data != null ? this.pxx2data.Count : 0);
            //this.c2qqq = (ushort)(this.pxx3data != null ? this.pxx3data.Count : 0);
            //this.Unknown_108h_Pointer = (ulong)(this.Unknown_108h_Data != null ? this.Unknown_108h_Data.Position : 0);
            //this.NamePointer = (ulong)(this.Name != null ? this.Name.Position : 0); //TODO: fix

            // write structure data
            writer.Write(this.Unknown_0A8h);
            writer.Write(this.Unknown_0ACh);
            //writer.WriteBlock(this.Unknown_0B0h); //TODO: fix!
            writer.Write(this.BoundPointer);
            writer.Write(this.Unknown_0F8h_Pointer);
            writer.Write(this.Count1);
            writer.Write(this.Count2);
            writer.Write(this.Unknown_104h);
            writer.Write(this.Unknown_108h_Pointer);
            writer.Write(this.Count3);
            writer.Write(this.Count4);
            writer.Write(this.Unknown_114h);
            writer.Write(this.Unknown_118h);
            writer.Write(this.Unknown_11Ch);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_124h);
            writer.Write(this.Unknown_128h);
            writer.Write(this.Unknown_12Ch);
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_138h);
            writer.Write(this.Unknown_13Ch);
            writer.Write(this.Unknown_140h);
            writer.Write(this.Unknown_144h);
            writer.Write(this.Unknown_148h);
            writer.Write(this.Unknown_14Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Bound != null) list.Add(Bound);
            //if (Unknown_F8h_Data != null) list.Add(Unknown_F8h_Data); //TODO: fix
            //if (Unknown_108h_Data != null) list.Add(Unknown_108h_Data);
            //if (Name != null) list.Add(Name); //TODO: fix
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragUnknown_F_004 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32 + Data.Length; }
        }

        // structure data
        public uint Unknown_00h { get; set; } // 0x00000000
        public uint Unknown_04h { get; set; } // 0x00000000
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public byte cnt1 { get; set; }
        public byte cnt2 { get; set; }
        public ushort Unknown_12h { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public Matrix3_s[] Data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_00h = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.cnt1 = reader.ReadByte();
            this.cnt2 = reader.ReadByte();
            this.Unknown_12h = reader.ReadUInt16();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            //this.Data = reader.ReadBlock<ResourceSimpleArray<Matrix3_r>>(
            //    cnt1
            //    );
            this.Data = reader.ReadStructsAt<Matrix3_s>((ulong)reader.Position, cnt1);

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_00h);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.cnt1);
            writer.Write(this.cnt2);
            writer.Write(this.Unknown_12h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            //writer.WriteBlock(this.Data); //TODO: fix
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                //new Tuple<long, IResourceBlock>(32, Data) //TODO: FIX
            };
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragUnknown_F_006 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 112; }
        }

        // structure data
        public uint Unknown_00h { get; set; }
        public uint Unknown_04h { get; set; }
        public uint Unknown_08h { get; set; }
        public uint Unknown_0Ch { get; set; } // 0x7F800001
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; } // 0x7F800001
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; }
        public uint Unknown_2Ch { get; set; } // 0x7F800001
        public uint Unknown_30h { get; set; }
        public uint Unknown_34h { get; set; }
        public uint Unknown_38h { get; set; }
        public uint Unknown_3Ch { get; set; }
        public uint Unknown_40h { get; set; } // 0x000000D9
        public uint Unknown_44h { get; set; } // 0x0500002C
        public uint Unknown_48h { get; set; } // 0x55996996 looks like vertex types
        public uint Unknown_4Ch { get; set; } // 0x76555555
        public uint Unknown_50h { get; set; }
        public uint Unknown_54h { get; set; }
        public uint Unknown_58h { get; set; }
        public uint Unknown_5Ch { get; set; }
        public uint Unknown_60h { get; set; }
        public uint Unknown_64h { get; set; }
        public uint Unknown_68h { get; set; }
        public uint Unknown_6Ch { get; set; } // 0x7F800001

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_00h = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
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

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
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
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragUnknown_F_003 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16 + Data.Length; }
        }

        // structure data
        public uint Unknown_0h { get; set; } // 0x56475748
        public uint Unknown_4h { get; set; }
        public uint cnt1 { get; set; }
        public uint Unknown_Ch { get; set; }
        //public ResourceSimpleArray<byte_r> Data { get; set; }
        public byte[] Data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.cnt1 = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            //this.Data = reader.ReadBlock<ResourceSimpleArray<byte_r>>(
            //  cnt1 - 16
            //  );
            this.Data = reader.ReadBytesAt((ulong)this.FilePosition, cnt1 - 16);

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.cnt1);
            writer.Write(this.Unknown_Ch);
            //writer.WriteBlock(this.Data); //TODO: FIX!!
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                //new Tuple<long, IResourceBlock>(16, Data)
            };
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysicsLODGroup : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public ulong PhysicsLOD1Pointer { get; set; }
        public ulong PhysicsLOD2Pointer { get; set; }
        public ulong PhysicsLOD3Pointer { get; set; }
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        // reference data
        public FragPhysicsLOD PhysicsLOD1 { get; set; }
        public FragPhysicsLOD PhysicsLOD2 { get; set; }
        public FragPhysicsLOD PhysicsLOD3 { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.PhysicsLOD1Pointer = reader.ReadUInt64();
            this.PhysicsLOD2Pointer = reader.ReadUInt64();
            this.PhysicsLOD3Pointer = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();

            // read reference data
            this.PhysicsLOD1 = reader.ReadBlockAt<FragPhysicsLOD>(
                this.PhysicsLOD1Pointer // offset
            );
            this.PhysicsLOD2 = reader.ReadBlockAt<FragPhysicsLOD>(
                this.PhysicsLOD2Pointer // offset
            );
            this.PhysicsLOD3 = reader.ReadBlockAt<FragPhysicsLOD>(
                this.PhysicsLOD3Pointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.PhysicsLOD1Pointer = (ulong)(this.PhysicsLOD1 != null ? this.PhysicsLOD1.FilePosition : 0);
            this.PhysicsLOD2Pointer = (ulong)(this.PhysicsLOD2 != null ? this.PhysicsLOD2.FilePosition : 0);
            this.PhysicsLOD3Pointer = (ulong)(this.PhysicsLOD3 != null ? this.PhysicsLOD3.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.PhysicsLOD1Pointer);
            writer.Write(this.PhysicsLOD2Pointer);
            writer.Write(this.PhysicsLOD3Pointer);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (PhysicsLOD1 != null) list.Add(PhysicsLOD1);
            if (PhysicsLOD2 != null) list.Add(PhysicsLOD2);
            if (PhysicsLOD3 != null) list.Add(PhysicsLOD3);
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysicsLOD : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 304; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }
        public ulong ArticulatedBodyTypePointer { get; set; }
        public ulong Unknown_28h_Pointer { get; set; }
        public SharpDX.Vector4 Unknown_30h { get; set; }
        public SharpDX.Vector4 Unknown_40h { get; set; }
        public SharpDX.Vector4 Unknown_50h { get; set; }
        public SharpDX.Vector4 Unknown_60h { get; set; }
        public SharpDX.Vector4 Unknown_70h { get; set; }
        public SharpDX.Vector4 Unknown_80h { get; set; }
        public SharpDX.Vector4 Unknown_90h { get; set; }
        public SharpDX.Vector4 Unknown_A0h { get; set; }
        public SharpDX.Vector4 Unknown_B0h { get; set; }
        public ulong GroupNamesPointer { get; set; }
        public ulong GroupsPointer { get; set; }
        public ulong ChildrenPointer { get; set; }
        public ulong Archetype1Pointer { get; set; }
        public ulong Archetype2Pointer { get; set; }
        public ulong BoundPointer { get; set; }
        public ulong InertiaTensorsPointer { get; set; }
        public ulong Unknown_F8h_Pointer { get; set; }
        public ulong FragTransformsPointer { get; set; }
        public ulong Unknown_108h_Pointer { get; set; }
        public ulong Unknown_110h_Pointer { get; set; }
        public byte Count1 { get; set; }
        public byte Count2 { get; set; }
        public byte GroupsCount { get; set; }
        public byte Unknown_11Bh { get; set; }
        public byte Unknown_11Ch { get; set; }
        public byte ChildrenCount { get; set; }
        public byte Count3 { get; set; }
        public byte Unknown_11Fh { get; set; } // 0x00
        public uint Unknown_120h { get; set; } // 0x00000000
        public uint Unknown_124h { get; set; } // 0x00000000
        public uint Unknown_128h { get; set; } // 0x00000000
        public uint Unknown_12Ch { get; set; } // 0x00000000

        // reference data
        public FragPhysArticulatedBodyType ArticulatedBodyType { get; set; }
        public uint[] Unknown_28h_Data { get; set; }
        public ResourcePointerArray64_s<FragPhysNameStruct_s> GroupNames { get; set; }
        public ResourcePointerArray64_s<FragPhysTypeGroup_s> Groups { get; set; }
        public ResourcePointerArray64<FragPhysTypeChild> Children { get; set; }
        public FragPhysArchetype Archetype1 { get; set; }
        public FragPhysArchetype Archetype2 { get; set; }
        public Bounds Bound { get; set; }
        public SharpDX.Vector4[] InertiaTensors { get; set; }
        public SharpDX.Vector4[] Unknown_F8h_Data { get; set; }
        public FragPhysUnknown_F_002 FragTransforms { get; set; }
        public byte[] Unknown_108h_Data { get; set; }
        public byte[] Unknown_110h_Data { get; set; }


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.ArticulatedBodyTypePointer = reader.ReadUInt64();
            this.Unknown_28h_Pointer = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_40h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_50h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_60h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_70h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_80h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_90h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_A0h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_B0h = reader.ReadStruct<SharpDX.Vector4>();
            this.GroupNamesPointer = reader.ReadUInt64();
            this.GroupsPointer = reader.ReadUInt64();
            this.ChildrenPointer = reader.ReadUInt64();
            this.Archetype1Pointer = reader.ReadUInt64();
            this.Archetype2Pointer = reader.ReadUInt64();
            this.BoundPointer = reader.ReadUInt64();
            this.InertiaTensorsPointer = reader.ReadUInt64();
            this.Unknown_F8h_Pointer = reader.ReadUInt64();
            this.FragTransformsPointer = reader.ReadUInt64();
            this.Unknown_108h_Pointer = reader.ReadUInt64();
            this.Unknown_110h_Pointer = reader.ReadUInt64();
            this.Count1 = reader.ReadByte();
            this.Count2 = reader.ReadByte();
            this.GroupsCount = reader.ReadByte();
            this.Unknown_11Bh = reader.ReadByte();
            this.Unknown_11Ch = reader.ReadByte();
            this.ChildrenCount = reader.ReadByte();
            this.Count3 = reader.ReadByte();
            this.Unknown_11Fh = reader.ReadByte();
            this.Unknown_120h = reader.ReadUInt32();
            this.Unknown_124h = reader.ReadUInt32();
            this.Unknown_128h = reader.ReadUInt32();
            this.Unknown_12Ch = reader.ReadUInt32();

            // read reference data
            this.ArticulatedBodyType = reader.ReadBlockAt<FragPhysArticulatedBodyType>(
                this.ArticulatedBodyTypePointer // offset
            );
            //this.Unknown_28h_Data = reader.ReadBlockAt<ResourceSimpleArray<uint_r>>(
            //    this.Unknown_28h_Pointer, // offset
            //    this.ChildrenCount
            //);
            this.Unknown_28h_Data = reader.ReadUintsAt(this.Unknown_28h_Pointer, this.ChildrenCount);

            this.GroupNames = reader.ReadBlockAt<ResourcePointerArray64_s<FragPhysNameStruct_s>>(
                this.GroupNamesPointer, // offset
                this.GroupsCount
            );
            this.Groups = reader.ReadBlockAt<ResourcePointerArray64_s<FragPhysTypeGroup_s>>(
                this.GroupsPointer, // offset
                this.GroupsCount
            );
            this.Children = reader.ReadBlockAt<ResourcePointerArray64<FragPhysTypeChild>>(
                this.ChildrenPointer, // offset
                this.ChildrenCount
            );
            this.Archetype1 = reader.ReadBlockAt<FragPhysArchetype>(
                this.Archetype1Pointer // offset
            );
            this.Archetype2 = reader.ReadBlockAt<FragPhysArchetype>(
                this.Archetype2Pointer // offset
            );
            this.Bound = reader.ReadBlockAt<Bounds>(
                this.BoundPointer // offset
            );
            //this.Unknown_F0h_Data = reader.ReadBlockAt<ResourceSimpleArray<Vector4_r>>(
            //    this.Unknown_F0h_Pointer, // offset
            //    this.ChildrenCount
            //);
            //this.Unknown_F8h_Data = reader.ReadBlockAt<ResourceSimpleArray<Vector4_r>>(
            //    this.Unknown_F8h_Pointer, // offset
            //    this.ChildrenCount
            //);
            this.InertiaTensors = reader.ReadStructsAt<SharpDX.Vector4>(this.InertiaTensorsPointer, this.ChildrenCount);
            this.Unknown_F8h_Data = reader.ReadStructsAt<SharpDX.Vector4>(this.Unknown_F8h_Pointer, this.ChildrenCount);


            this.FragTransforms = reader.ReadBlockAt<FragPhysUnknown_F_002>(
                this.FragTransformsPointer // offset
            );
            //this.Unknown_108h_Data = reader.ReadBlockAt<ResourceSimpleArray<byte_r>>(
            //    this.Unknown_108h_Pointer, // offset
            //    this.Count1
            //);
            //this.Unknown_110h_Data = reader.ReadBlockAt<ResourceSimpleArray<byte_r>>(
            //    this.Unknown_110h_Pointer, // offset
            //    this.Count2
            //);

            this.Unknown_108h_Data = reader.ReadBytesAt(this.Unknown_108h_Pointer, this.Count1);
            this.Unknown_110h_Data = reader.ReadBytesAt(this.Unknown_110h_Pointer, this.Count2);



            if ((Children != null) && (Children.data_items != null))
            {
                for (int i = 0; i < Children.data_items.Length; i++)
                {
                    var child = Children.data_items[i];
                    child.OwnerFragPhysLod = this;
                    child.OwnerFragPhysIndex = i;
                }
            }
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.ArticulatedBodyTypePointer = (ulong)(this.ArticulatedBodyType != null ? this.ArticulatedBodyType.FilePosition : 0);
            //this.Unknown_28h_Pointer = (ulong)(this.Unknown_28h_Data != null ? this.Unknown_28h_Data.Position : 0); //TODO: fix
            this.GroupNamesPointer = (ulong)(this.GroupNames != null ? this.GroupNames.FilePosition : 0);
            this.GroupsPointer = (ulong)(this.Groups != null ? this.Groups.FilePosition : 0);
            this.ChildrenPointer = (ulong)(this.Children != null ? this.Children.FilePosition : 0);
            this.Archetype1Pointer = (ulong)(this.Archetype1 != null ? this.Archetype1.FilePosition : 0);
            this.Archetype2Pointer = (ulong)(this.Archetype2 != null ? this.Archetype2.FilePosition : 0);
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);
            //this.Unknown_F0h_Pointer = (ulong)(this.Unknown_F0h_Data != null ? this.Unknown_F0h_Data.Position : 0);
            //this.Unknown_F8h_Pointer = (ulong)(this.Unknown_F8h_Data != null ? this.Unknown_F8h_Data.Position : 0);
            this.FragTransformsPointer = (ulong)(this.FragTransforms != null ? this.FragTransforms.FilePosition : 0);
            //this.Unknown_108h_Pointer = (ulong)(this.Unknown_108h_Data != null ? this.Unknown_108h_Data.Position : 0);
            //this.Unknown_110h_Pointer = (ulong)(this.Unknown_110h_Data != null ? this.Unknown_110h_Data.Position : 0);

            //this.vvv1 = (byte)(this.pxxxxx_2data != null ? this.pxxxxx_2data.Count : 0);
            //this.vvv2 = (byte)(this.pxxxxx_3data != null ? this.pxxxxx_3data.Count : 0);
            //this.GroupsCount = (byte)(this.Groups != null ? this.Groups.Count : 0);
            //this.ChildrenCount = (byte)(this.p1data != null ? this.p1data.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.ArticulatedBodyTypePointer);
            writer.Write(this.Unknown_28h_Pointer);
            //writer.WriteBlock(this.Unknown_30h); //TODO: fix this!
            //writer.WriteBlock(this.Unknown_40h);
            //writer.WriteBlock(this.Unknown_50h);
            //writer.WriteBlock(this.Unknown_60h);
            //writer.WriteBlock(this.Unknown_70h);
            //writer.WriteBlock(this.Unknown_80h);
            //writer.WriteBlock(this.Unknown_90h);
            //writer.WriteBlock(this.Unknown_A0h);
            //writer.WriteBlock(this.Unknown_B0h);
            writer.Write(this.GroupNamesPointer);
            writer.Write(this.GroupsPointer);
            writer.Write(this.ChildrenPointer);
            writer.Write(this.Archetype1Pointer);
            writer.Write(this.Archetype2Pointer);
            writer.Write(this.BoundPointer);
            writer.Write(this.InertiaTensorsPointer);
            writer.Write(this.Unknown_F8h_Pointer);
            writer.Write(this.FragTransformsPointer);
            writer.Write(this.Unknown_108h_Pointer);
            writer.Write(this.Unknown_110h_Pointer);
            writer.Write(this.Count1);
            writer.Write(this.Count2);
            writer.Write(this.GroupsCount);
            writer.Write(this.Unknown_11Bh);
            writer.Write(this.Unknown_11Ch);
            writer.Write(this.ChildrenCount);
            writer.Write(this.Count3);
            writer.Write(this.Unknown_11Fh);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_124h);
            writer.Write(this.Unknown_128h);
            writer.Write(this.Unknown_12Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (ArticulatedBodyType != null) list.Add(ArticulatedBodyType);
            //if (Unknown_28h_Data != null) list.Add(Unknown_28h_Data); //TODO: fix
            if (Groups != null) list.Add(Groups);
            if (Children != null) list.Add(Children);
            if (Archetype1 != null) list.Add(Archetype1);
            if (Archetype2 != null) list.Add(Archetype2);
            if (Bound != null) list.Add(Bound);
            //if (Unknown_F0h_Data != null) list.Add(Unknown_F0h_Data);
            //if (Unknown_F8h_Data != null) list.Add(Unknown_F8h_Data);
            if (FragTransforms != null) list.Add(FragTransforms);
            //if (Unknown_108h_Data != null) list.Add(Unknown_108h_Data);
            //if (Unknown_110h_Data != null) list.Add(Unknown_110h_Data);
            if (GroupNames != null) list.Add(GroupNames);
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysArticulatedBodyType : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 176; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; }
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
        public uint Unknown_40h { get; set; }
        public uint Unknown_44h { get; set; }
        public uint Unknown_48h { get; set; }
        public uint Unknown_4Ch { get; set; }
        public uint Unknown_50h { get; set; }
        public uint Unknown_54h { get; set; }
        public uint Unknown_58h { get; set; }
        public uint Unknown_5Ch { get; set; }
        public uint Unknown_60h { get; set; }
        public uint Unknown_64h { get; set; }
        public uint Unknown_68h { get; set; }
        public uint Unknown_6Ch { get; set; }
        public uint Unknown_70h { get; set; }
        public uint Unknown_74h { get; set; }
        public ulong JointTypesPointer { get; set; }
        public ulong p2 { get; set; }
        public byte c1 { get; set; }
        public byte JointTypesCount { get; set; }
        public ushort Unknown_8Ah { get; set; }
        public uint Unknown_8Ch { get; set; }
        public uint Unknown_90h { get; set; }
        public uint Unknown_94h { get; set; }
        public uint Unknown_98h { get; set; }
        public uint Unknown_9Ch { get; set; }
        public uint Unknown_A0h { get; set; }
        public uint Unknown_A4h { get; set; }
        public uint Unknown_A8h { get; set; }
        public uint Unknown_ACh { get; set; }

        // reference data
        public ResourcePointerArray64<FragPhysJointType> JointTypes { get; set; }
        public SharpDX.Vector4[] p2data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
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
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.JointTypesPointer = reader.ReadUInt64();
            this.p2 = reader.ReadUInt64();
            this.c1 = reader.ReadByte();
            this.JointTypesCount = reader.ReadByte();
            this.Unknown_8Ah = reader.ReadUInt16();
            this.Unknown_8Ch = reader.ReadUInt32();
            this.Unknown_90h = reader.ReadUInt32();
            this.Unknown_94h = reader.ReadUInt32();
            this.Unknown_98h = reader.ReadUInt32();
            this.Unknown_9Ch = reader.ReadUInt32();
            this.Unknown_A0h = reader.ReadUInt32();
            this.Unknown_A4h = reader.ReadUInt32();
            this.Unknown_A8h = reader.ReadUInt32();
            this.Unknown_ACh = reader.ReadUInt32();

            // read reference data
            this.JointTypes = reader.ReadBlockAt<ResourcePointerArray64<FragPhysJointType>>(
                this.JointTypesPointer, // offset
                this.JointTypesCount
            );
            //this.p2data = reader.ReadBlockAt<ResourceSimpleArray<Vector4_r>>(
            //    this.p2, // offset
            //    this.c1
            //);
            this.p2data = reader.ReadStructsAt<SharpDX.Vector4>(this.p2, this.c1);

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.JointTypesPointer = (ulong)(this.JointTypes != null ? this.JointTypes.FilePosition : 0);
            //this.p2 = (ulong)(this.p2data != null ? this.p2data.Position : 0); //TODO:fix
            ////this.c1 = (byte)(this.p2data != null ? this.p2data.Count : 0);
            ////this.c2 = (byte)(this.p1data != null ? this.p1data.Count : 0);

            // write structure data
            writer.Write(this.VFT);
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
            writer.Write(this.JointTypesPointer);
            writer.Write(this.p2);
            writer.Write(this.c1);
            writer.Write(this.JointTypesCount);
            writer.Write(this.Unknown_8Ah);
            writer.Write(this.Unknown_8Ch);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_94h);
            writer.Write(this.Unknown_98h);
            writer.Write(this.Unknown_9Ch);
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_A4h);
            writer.Write(this.Unknown_A8h);
            writer.Write(this.Unknown_ACh);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (JointTypes != null) list.Add(JointTypes);
            //if (p2data != null) list.Add(p2data); //TODO: fix
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysJointType : ResourceSystemBlock, IResourceXXSystemBlock
    {
        public override long BlockLength
        {
            get { return 32; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public byte Unknown_14h { get; set; } // 0x3F533333
        public byte Type { get; set; }
        public ushort Unknown_16h { get; set; }
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadByte();
            this.Type = reader.ReadByte();
            this.Unknown_16h = reader.ReadUInt16();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Type);
            writer.Write(this.Unknown_16h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 21;
            var type = reader.ReadByte();
            reader.Position -= 22;

            switch (type)
            {
                case 0: return new FragPhysJoint1DofType();
                case 1: return new FragPhysJoint3DofType();
                default: return null;// throw new Exception("Unknown type");
            }
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysJoint1DofType : FragPhysJointType
    {
        public override long BlockLength
        {
            get { return 176; }
        }

        // structure data
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; }
        public uint Unknown_2Ch { get; set; }
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
        public uint Unknown_64h { get; set; }
        public uint Unknown_68h { get; set; }
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
        public uint Unknown_A8h { get; set; } // 0x4CBEBC20
        public uint Unknown_ACh { get; set; } // 0xCCBEBC20

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
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
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
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
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_A4h);
            writer.Write(this.Unknown_A8h);
            writer.Write(this.Unknown_ACh);
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysJoint3DofType : FragPhysJointType
    {
        public override long BlockLength
        {
            get { return 240; }
        }

        // structure data
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }
        public uint Unknown_28h { get; set; }
        public uint Unknown_2Ch { get; set; }
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
        public uint Unknown_64h { get; set; }
        public uint Unknown_68h { get; set; }
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
        public uint Unknown_ACh { get; set; } // 0x00000000
        public uint Unknown_B0h { get; set; } // 0x00000000
        public uint Unknown_B4h { get; set; } // 0x00000000
        public uint Unknown_B8h { get; set; } // 0x00000000
        public uint Unknown_BCh { get; set; } // 0x00000000
        public uint Unknown_C0h { get; set; } // 0x4CBEBC20
        public uint Unknown_C4h { get; set; } // 0x4CBEBC20
        public uint Unknown_C8h { get; set; } // 0x4CBEBC20
        public uint Unknown_CCh { get; set; } // 0x4CBEBC20
        public uint Unknown_D0h { get; set; } // 0xCCBEBC20
        public uint Unknown_D4h { get; set; } // 0xCCBEBC20
        public uint Unknown_D8h { get; set; } // 0xCCBEBC20
        public uint Unknown_DCh { get; set; } // 0xCCBEBC20
        public uint Unknown_E0h { get; set; } // 0x00000000
        public uint Unknown_E4h { get; set; } // 0x00000000
        public uint Unknown_E8h { get; set; } // 0x00000000
        public uint Unknown_ECh { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
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

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
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
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysUnknown_F_002 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32 + Data.Length; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint cnt { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        //public ResourceSimpleArray<Matrix4_r> Data { get; set; }
        public SharpDX.Matrix[] Data { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.cnt = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            //this.Data = reader.ReadBlock<ResourceSimpleArray<Matrix4_r>>(
            //  cnt
            //  );
            this.Data = reader.ReadStructsAt<SharpDX.Matrix>((ulong)reader.Position, cnt);

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.cnt);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            //writer.WriteBlock(this.Data); //TODO: fix
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                //new Tuple<long, IResourceBlock>(32, Data) //TODO: fix
            };
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysArchetype : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 224; }
        }

        // structure data
        public float Unknown_00h { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000002
        public uint Unknown_14h { get; set; } // 0x00000000
        public ulong NamePointer { get; set; }
        public ulong BoundPointer { get; set; }
        public uint Unknown_28h { get; set; } // 0x00000001
        public uint Unknown_2Ch { get; set; } // 0xFFFFFFFF
        public uint Unknown_30h { get; set; } // 0x00010000
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000
        public float Unknown_40h { get; set; }
        public float Unknown_44h { get; set; }
        public float Unknown_48h { get; set; } // 1.0f
        public float Unknown_4Ch { get; set; } // 150.0f
        public float Unknown_50h { get; set; } // 6.2831855f = 2*pi
        public float Unknown_54h { get; set; } // 1.0f
        public uint Unknown_58h { get; set; } // 0x00000000
        public uint Unknown_5Ch { get; set; } // 0x00000000
        public SharpDX.Vector4 Unknown_60h { get; set; }
        public SharpDX.Vector4 Unknown_70h { get; set; }
        public SharpDX.Vector4 Unknown_80h { get; set; } // 0.0 0.0 0.0 NaN
        public SharpDX.Vector4 Unknown_90h { get; set; } // 0.0 0.0 0.0 NaN
        public SharpDX.Vector4 Unknown_A0h { get; set; } // 0.0 0.0 0.0 NaN
        public SharpDX.Vector4 Unknown_B0h { get; set; } // 0.0 0.0 0.0 NaN
        public SharpDX.Vector4 Unknown_C0h { get; set; } // 0.0 0.0 0.0 NaN
        public SharpDX.Vector4 Unknown_D0h { get; set; } // 0.0 0.0 0.0 NaN

        // reference data
        public string Name { get; set; }
        public Bounds Bound { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_00h = reader.ReadSingle();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.BoundPointer = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadSingle();
            this.Unknown_44h = reader.ReadSingle();
            this.Unknown_48h = reader.ReadSingle();
            this.Unknown_4Ch = reader.ReadSingle();
            this.Unknown_50h = reader.ReadSingle();
            this.Unknown_54h = reader.ReadSingle();
            this.Unknown_58h = reader.ReadUInt32();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.Unknown_60h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_70h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_80h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_90h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_A0h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_B0h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_C0h = reader.ReadStruct<SharpDX.Vector4>();
            this.Unknown_D0h = reader.ReadStruct<SharpDX.Vector4>();

            // read reference data
            this.Name = reader.ReadStringAt(//BlockAt<string_r>(
                this.NamePointer // offset
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
            //this.NamePointer = (ulong)(this.Name != null ? this.Name.Position : 0); //TODO:fix
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);

            // write structure data
            writer.Write(this.Unknown_00h);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.NamePointer);
            writer.Write(this.BoundPointer);
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
            //writer.WriteBlock(this.Unknown_60h); //TODO: fix!
            //writer.WriteBlock(this.Unknown_70h);
            //writer.WriteBlock(this.Unknown_80h);
            //writer.WriteBlock(this.Unknown_90h);
            //writer.WriteBlock(this.Unknown_A0h);
            //writer.WriteBlock(this.Unknown_B0h);
            //writer.WriteBlock(this.Unknown_C0h);
            //writer.WriteBlock(this.Unknown_D0h);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            //if (Name != null) list.Add(Name); //TODO: fix!
            if (Bound != null) list.Add(Bound);
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysTypeChild : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 256; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public float Unknown_08h { get; set; }
        public float Unknown_0Ch { get; set; }
        public float Unknown_10h { get; set; }
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
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public uint Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000
        public uint Unknown_80h { get; set; } // 0x00000000
        public uint Unknown_84h { get; set; } // 0x00000000
        public uint Unknown_88h { get; set; } // 0x00000000
        public uint Unknown_8Ch { get; set; } // 0x00000000
        public uint Unknown_90h { get; set; } // 0x00000000
        public uint Unknown_94h { get; set; } // 0x00000000
        public uint Unknown_98h { get; set; } // 0x00000000
        public uint Unknown_9Ch { get; set; } // 0x00000000
        public ulong Drawable1Pointer { get; set; }
        public ulong Drawable2Pointer { get; set; }
        public ulong EvtSetPointer { get; set; }
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
        public uint Unknown_E8h { get; set; } // 0x00000000
        public uint Unknown_ECh { get; set; } // 0x00000000
        public uint Unknown_F0h { get; set; } // 0x00000000
        public uint Unknown_F4h { get; set; } // 0x00000000
        public uint Unknown_F8h { get; set; } // 0x00000000
        public uint Unknown_FCh { get; set; } // 0x00000000

        // reference data
        public FragDrawable Drawable1 { get; set; }
        public FragDrawable Drawable2 { get; set; }
        public FragPhysEvtSet EvtSet { get; set; }



        public FragPhysicsLOD OwnerFragPhysLod { get; set; }
        public int OwnerFragPhysIndex { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadSingle();
            this.Unknown_0Ch = reader.ReadSingle();
            this.Unknown_10h = reader.ReadSingle();
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
            this.Drawable1Pointer = reader.ReadUInt64();
            this.Drawable2Pointer = reader.ReadUInt64();
            this.EvtSetPointer = reader.ReadUInt64();
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

            // read reference data
            this.Drawable1 = reader.ReadBlockAt<FragDrawable>(
                this.Drawable1Pointer // offset
            );
            this.Drawable2 = reader.ReadBlockAt<FragDrawable>(
                this.Drawable2Pointer // offset
            );
            this.EvtSet = reader.ReadBlockAt<FragPhysEvtSet>(
                this.EvtSetPointer // offset
            );

            if (this.Drawable1 != null)
            {
                this.Drawable1.OwnerFragmentPhys = this;
            }
            if (this.Drawable2 != null)
            {
                this.Drawable2.OwnerFragmentPhys = this;
            }

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.Drawable1Pointer = (ulong)(this.Drawable1 != null ? this.Drawable1.FilePosition : 0);
            this.Drawable2Pointer = (ulong)(this.Drawable2 != null ? this.Drawable2.FilePosition : 0);
            this.EvtSetPointer = (ulong)(this.EvtSet != null ? this.EvtSet.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
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
            writer.Write(this.Drawable1Pointer);
            writer.Write(this.Drawable2Pointer);
            writer.Write(this.EvtSetPointer);
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
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Drawable1 != null) list.Add(Drawable1);
            if (Drawable2 != null) list.Add(Drawable2);
            if (EvtSet != null) list.Add(EvtSet);
            return list.ToArray();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysEvtSet : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public uint Unknown_28h { get; set; } // 0x00000000
        public uint Unknown_2Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt32();
            this.Unknown_0Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
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
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct FragPhysTypeGroup_s
    {
        // structure data
        public float Unknown_00h { get; set; } // 0x00000000
        public float Unknown_04h { get; set; } // 0x00000000
        public float Unknown_08h { get; set; } // 0x00000000
        public float Unknown_0Ch { get; set; } // 0x00000000
        public float Unknown_10h { get; set; }
        public float Unknown_14h { get; set; }
        public float Unknown_18h { get; set; }
        public float Unknown_1Ch { get; set; }
        public float Unknown_20h { get; set; }
        public float Unknown_24h { get; set; }
        public float Unknown_28h { get; set; }
        public float Unknown_2Ch { get; set; }
        public float Unknown_30h { get; set; }
        public float Unknown_34h { get; set; }
        public float Unknown_38h { get; set; }
        public float Unknown_3Ch { get; set; }
        public float Unknown_40h { get; set; }
        public float Unknown_44h { get; set; }
        public float Unknown_48h { get; set; } // 0x00000000
        public byte Unknown_4Cha { get; set; }
        public byte Unknown_4Chb { get; set; }
        public byte Unknown_4Chc { get; set; }
        public byte Unknown_4Chd { get; set; }
        //public uint Unknown_4Ch { get; set; }
        public byte Unknown_50ha { get; set; }
        public byte Unknown_50hb { get; set; }//0xFF
        public ushort Unknown_50hc { get; set; }//0
        //public uint Unknown_50h { get; set; }
        public float Unknown_54h { get; set; }
        public float Unknown_58h { get; set; }
        public float Unknown_5Ch { get; set; }
        public float Unknown_60h { get; set; }
        public float Unknown_64h { get; set; }
        public float Unknown_68h { get; set; }
        public float Unknown_6Ch { get; set; }
        public float Unknown_70h { get; set; }
        public float Unknown_74h { get; set; }
        public float Unknown_78h { get; set; }
        public float Unknown_7Ch { get; set; } // 0x00000000
        public FragPhysNameStruct_s Name { get; set; }
        public float Unknown_A0h { get; set; } // 0x00000000
        public float Unknown_A4h { get; set; } // 0x00000000
        public float Unknown_A8h { get; set; }
        public float Unknown_ACh { get; set; } // 0x00000000

        public override string ToString()
        {
            return Name.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public struct FragPhysNameStruct_s
    {
        // structure data
        public uint Unknown_00h { get; set; }
        public uint Unknown_04h { get; set; }
        public uint Unknown_08h { get; set; }
        public uint Unknown_0Ch { get; set; }
        public uint Unknown_10h { get; set; }
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch { get; set; }

        public override string ToString()
        {
            UintStringBuilder usb = new UintStringBuilder();
            usb.Add(Unknown_00h);
            usb.Add(Unknown_04h);
            usb.Add(Unknown_08h);
            usb.Add(Unknown_0Ch);
            usb.Add(Unknown_10h);
            usb.Add(Unknown_14h);
            usb.Add(Unknown_18h);
            usb.Add(Unknown_1Ch);
            return usb.ToString();
        }
    }


    public class UintStringBuilder
    {
        public StringBuilder sb = new StringBuilder();

        public void Add(uint u)
        {
            sb.Append((char)((u & 0x000000FF) >> 0));
            sb.Append((char)((u & 0x0000FF00) >> 8));
            sb.Append((char)((u & 0x00FF0000) >> 16));
            sb.Append((char)((u & 0xFF000000) >> 24));
        }

        public override string ToString()
        {
            return sb.ToString().Replace("\0", "");
        }
    }


}