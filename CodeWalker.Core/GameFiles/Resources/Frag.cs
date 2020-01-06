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


using SharpDX;
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
        public ulong DrawableArrayPointer { get; set; }
        public ulong DrawableArrayNamesPointer { get; set; }
        public uint DrawableArrayCount { get; set; }
        public uint Unknown_4Ch { get; set; } //pointer? 
        public uint Unknown_50h { get; set; } // 0x00000000
        public uint Unknown_54h { get; set; } // 0x00000000
        public ulong NamePointer { get; set; }
        public ResourcePointerList64<EnvironmentCloth> Clothes { get; set; }
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
        public ulong BoneTransformsPointer { get; set; }
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
        public byte GlassWindowsCount { get; set; }
        public ushort Unknown_DAh { get; set; }
        public uint Unknown_DCh { get; set; } // 0x00000000
        public ulong GlassWindowsPointer { get; set; }
        public uint Unknown_E8h { get; set; } // 0x00000000
        public uint Unknown_ECh { get; set; } // 0x00000000
        public ulong PhysicsLODGroupPointer { get; set; }
        public ulong Drawable2Pointer { get; set; }
        public uint Unknown_100h { get; set; } // 0x00000000
        public uint Unknown_104h { get; set; } // 0x00000000
        public uint Unknown_108h { get; set; } // 0x00000000
        public uint Unknown_10Ch { get; set; } // 0x00000000
        public ResourceSimpleList64_s<LightAttributes_s> LightAttributes { get; set; }
        public ulong VehicleGlassWindowsPointer { get; set; }
        public uint Unknown_128h { get; set; } // 0x00000000
        public uint Unknown_12Ch { get; set; } // 0x00000000

        // reference data
        public FragDrawable Drawable { get; set; }
        public ResourcePointerArray64<FragDrawable> DrawableArray { get; set; }
        public ResourcePointerArray64<string_r> DrawableArrayNames { get; set; }
        public string Name { get; set; }
        public FragBoneTransforms BoneTransforms { get; set; }
        public ResourcePointerArray64<FragGlassWindow> GlassWindows { get; set; }
        public FragPhysicsLODGroup PhysicsLODGroup { get; set; }
        public FragDrawable Drawable2 { get; set; }
        public FragVehicleGlassWindows VehicleGlassWindows { get; set; }


        private string_r NameBlock = null; //only used for saving

        public YftFile Yft { get; set; }


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
            this.DrawableArrayPointer = reader.ReadUInt64();
            this.DrawableArrayNamesPointer = reader.ReadUInt64();
            this.DrawableArrayCount = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.Clothes = reader.ReadBlock<ResourcePointerList64<EnvironmentCloth>>();
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
            this.BoneTransformsPointer = reader.ReadUInt64();
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
            this.GlassWindowsCount = reader.ReadByte();
            this.Unknown_DAh = reader.ReadUInt16();
            this.Unknown_DCh = reader.ReadUInt32();
            this.GlassWindowsPointer = reader.ReadUInt64();
            this.Unknown_E8h = reader.ReadUInt32();
            this.Unknown_ECh = reader.ReadUInt32();
            this.PhysicsLODGroupPointer = reader.ReadUInt64();
            this.Drawable2Pointer = reader.ReadUInt64();
            this.Unknown_100h = reader.ReadUInt32();
            this.Unknown_104h = reader.ReadUInt32();
            this.Unknown_108h = reader.ReadUInt32();
            this.Unknown_10Ch = reader.ReadUInt32();
            this.LightAttributes = reader.ReadBlock<ResourceSimpleList64_s<LightAttributes_s>>();
            this.VehicleGlassWindowsPointer = reader.ReadUInt64();
            this.Unknown_128h = reader.ReadUInt32();
            this.Unknown_12Ch = reader.ReadUInt32();

            // read reference data
            Drawable = reader.ReadBlockAt<FragDrawable>(this.DrawablePointer);
            if (Drawable != null)
            {
                Drawable.OwnerFragment = this;
            }

            DrawableArray = reader.ReadBlockAt<ResourcePointerArray64<FragDrawable>>(DrawableArrayPointer, DrawableArrayCount);
            if ((DrawableArray != null) && (DrawableArray.data_items != null))
            {
                for (int i = 0; i < DrawableArray.data_items.Length; i++)
                {
                    var drwbl = DrawableArray.data_items[i];
                    if (drwbl != null)
                    {
                        drwbl.OwnerFragment = this;
                    }
                }
            }
            DrawableArrayNames = reader.ReadBlockAt<ResourcePointerArray64<string_r>>(DrawableArrayNamesPointer, DrawableArrayCount);
            Name = reader.ReadStringAt(NamePointer);
            BoneTransforms = reader.ReadBlockAt<FragBoneTransforms>(BoneTransformsPointer);
            GlassWindows = reader.ReadBlockAt<ResourcePointerArray64<FragGlassWindow>>(GlassWindowsPointer, GlassWindowsCount);
            PhysicsLODGroup = reader.ReadBlockAt<FragPhysicsLODGroup>(PhysicsLODGroupPointer);
            Drawable2 = reader.ReadBlockAt<FragDrawable>(Drawable2Pointer);
            if (Drawable2 != null)
            {
                Drawable2.OwnerFragment = this;
            }

            VehicleGlassWindows = reader.ReadBlockAt<FragVehicleGlassWindows>(VehicleGlassWindowsPointer);



            if (GlassWindows != null)
            { }
            if (VehicleGlassWindows != null)
            { }


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
            this.DrawableArrayPointer = (ulong)(this.DrawableArray != null ? this.DrawableArray.FilePosition : 0);
            this.DrawableArrayNamesPointer = (ulong)(this.DrawableArrayNames != null ? this.DrawableArrayNames.FilePosition : 0);
            this.DrawableArrayCount = (uint)(this.DrawableArray != null ? this.DrawableArray.Count : 0);
            this.NamePointer = (ulong)(this.NameBlock != null ? this.NameBlock.FilePosition : 0);
            this.BoneTransformsPointer = (ulong)(this.BoneTransforms != null ? this.BoneTransforms.FilePosition : 0);
            this.GlassWindowsCount = (byte)(this.GlassWindows != null ? this.GlassWindows.Count : 0);
            this.GlassWindowsPointer = (ulong)(this.GlassWindows != null ? this.GlassWindows.FilePosition : 0);
            this.PhysicsLODGroupPointer = (ulong)(this.PhysicsLODGroup != null ? this.PhysicsLODGroup.FilePosition : 0);
            this.Drawable2Pointer = (ulong)(this.Drawable2 != null ? this.Drawable2.FilePosition : 0);
            this.VehicleGlassWindowsPointer = (ulong)(this.VehicleGlassWindows != null ? this.VehicleGlassWindows.FilePosition : 0);

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
            writer.Write(this.DrawableArrayPointer);
            writer.Write(this.DrawableArrayNamesPointer);
            writer.Write(this.DrawableArrayCount);
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
            writer.Write(this.BoneTransformsPointer);
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
            writer.Write(this.GlassWindowsCount);
            writer.Write(this.Unknown_DAh);
            writer.Write(this.Unknown_DCh);
            writer.Write(this.GlassWindowsPointer);
            writer.Write(this.Unknown_E8h);
            writer.Write(this.Unknown_ECh);
            writer.Write(this.PhysicsLODGroupPointer);
            writer.Write(this.Drawable2Pointer);
            writer.Write(this.Unknown_100h);
            writer.Write(this.Unknown_104h);
            writer.Write(this.Unknown_108h);
            writer.Write(this.Unknown_10Ch);
            writer.WriteBlock(this.LightAttributes);
            writer.Write(this.VehicleGlassWindowsPointer);
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
            if (DrawableArray != null) list.Add(DrawableArray);
            if (DrawableArrayNames != null) list.Add(DrawableArrayNames);
            if (Name != null)
            {
                NameBlock = (string_r)Name;
                list.Add(NameBlock);
            }
            if (BoneTransforms != null) list.Add(BoneTransforms);
            if (GlassWindows != null) list.Add(GlassWindows);
            if (PhysicsLODGroup != null) list.Add(PhysicsLODGroup);
            if (Drawable2 != null) list.Add(Drawable2);
            if (VehicleGlassWindows != null) list.Add(VehicleGlassWindows);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x60, Clothes),
                new Tuple<long, IResourceBlock>(0x110, LightAttributes)
            };
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
        public Matrix FragMatrix { get; set; } //unknown?
        public ulong BoundPointer { get; set; }
        public ulong FragMatricesIndsPointer { get; set; }
        public ushort FragMatricesIndsCount { get; set; }
        public ushort FragMatricesCount { get; set; }
        public uint Unknown_104h { get; set; } // 0x00000000
        public ulong FragMatricesPointer { get; set; }
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
        public ulong[] FragMatricesInds { get; set; }
        public Matrix[] FragMatrices { get; set; }
        public string Name { get; set; }

        public FragType OwnerFragment { get; set; } //for handy use
        public EnvironmentCloth OwnerCloth { get; set; }
        public FragPhysTypeChild OwnerFragmentPhys { get; set; }


        private ResourceSystemStructBlock<ulong> Unknown_F8h_DataBlock = null; //used for saving only
        private ResourceSystemStructBlock<Matrix> Unknown_108h_DataBlock = null;
        private string_r NameBlock = null;

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_0A8h = reader.ReadUInt32();
            this.Unknown_0ACh = reader.ReadUInt32();
            this.FragMatrix = reader.ReadStruct<Matrix>();
            this.BoundPointer = reader.ReadUInt64();
            this.FragMatricesIndsPointer = reader.ReadUInt64();
            this.FragMatricesIndsCount = reader.ReadUInt16();
            this.FragMatricesCount = reader.ReadUInt16();
            this.Unknown_104h = reader.ReadUInt32();
            this.FragMatricesPointer = reader.ReadUInt64();
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
            Bound = reader.ReadBlockAt<Bounds>(BoundPointer);
            FragMatricesInds = reader.ReadUlongsAt(FragMatricesIndsPointer, FragMatricesIndsCount);
            FragMatrices = reader.ReadStructsAt<Matrix>(FragMatricesPointer, FragMatricesCount);
            Name = reader.ReadStringAt(NamePointer);

            if (Bound != null)
            {
                Bound.Owner = this;
            }

            if ((Count3 != Count4)&&(Count4!=1)&&(Count3!=0))
            { }
            if (FragMatricesInds != null)
            { }
            if (FragMatrices != null)
            { }
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);
            this.FragMatricesIndsPointer = (ulong)(this.Unknown_F8h_DataBlock != null ? this.Unknown_F8h_DataBlock.FilePosition : 0);
            this.FragMatricesIndsCount = (ushort)(this.Unknown_F8h_DataBlock != null ? this.Unknown_F8h_DataBlock.ItemCount : 0);
            this.FragMatricesCount = (ushort)(this.Unknown_108h_DataBlock != null ? this.Unknown_108h_DataBlock.ItemCount : 0);
            this.FragMatricesPointer = (ulong)(this.Unknown_108h_DataBlock != null ? this.Unknown_108h_DataBlock.FilePosition : 0);
            this.NamePointer = (ulong)(this.NameBlock != null ? this.NameBlock.FilePosition : 0);

            // write structure data
            writer.Write(this.Unknown_0A8h);
            writer.Write(this.Unknown_0ACh);
            writer.Write(this.FragMatrix);
            writer.Write(this.BoundPointer);
            writer.Write(this.FragMatricesIndsPointer);
            writer.Write(this.FragMatricesIndsCount);
            writer.Write(this.FragMatricesCount);
            writer.Write(this.Unknown_104h);
            writer.Write(this.FragMatricesPointer);
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
            if (FragMatricesInds != null)
            {
                Unknown_F8h_DataBlock = new ResourceSystemStructBlock<ulong>(FragMatricesInds);
                list.Add(Unknown_F8h_DataBlock);
            }
            if (FragMatrices != null)
            {
                Unknown_108h_DataBlock = new ResourceSystemStructBlock<Matrix>(FragMatrices);
                list.Add(Unknown_108h_DataBlock);
            }
            if (Name != null)
            {
                NameBlock = (string_r)Name;
                list.Add(NameBlock);
            }
            return list.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragBoneTransforms : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32 + ((Items?.Length ?? 0) * 48); }
        }

        // structure data
        public uint Unknown_00h { get; set; } // 0x00000000
        public uint Unknown_04h { get; set; } // 0x00000000
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public byte ItemCount1 { get; set; }
        public byte ItemCount2 { get; set; }
        public ushort Unknown_12h { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public Matrix3_s[] Items { get; set; }

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
            this.ItemCount1 = reader.ReadByte();
            this.ItemCount2 = reader.ReadByte();
            this.Unknown_12h = reader.ReadUInt16();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Items = reader.ReadStructs<Matrix3_s>(ItemCount1);

            if ((Unknown_12h != 0) && (Unknown_12h != 1))
            { }
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
            writer.Write(this.ItemCount1);
            writer.Write(this.ItemCount2);
            writer.Write(this.Unknown_12h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteStructs(Items);
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragGlassWindow : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 112; }
        }

        // structure data
        public Matrix Matrix { get; set; } //column 4 is NaN,NaN,NaN,1
        public uint VertexDeclFlags { get; set; } // 0x000000D9
        public ushort VertexDeclStride { get; set; } // 0x002C
        public byte VertexDeclUnk { get; set; } //0x00
        public byte VertexDeclCount { get; set; } //0x05
        public ulong VertexDeclTypes { get; set; } // 0x7655555555996996
        public MetaHash Unknown_50h { get; set; } //looks floaty? flagsy? 0xXXXX0000
        public ushort Unknown_54h { get; set; }//2
        public ushort Flags { get; set; }//512, 768, 1280 etc ... flags
        public Vector3 Vector1 { get; set; }
        public Vector3 Vector2 { get; set; } // z = 0x7F800001 (NaN)


        public ulong VertexDeclId //this all equates to VertexTypePNCTT
        {
            get
            {
                ulong res = 0;
                for (int i = 0; i < 16; i++)
                {
                    if (((VertexDeclFlags >> i) & 1) == 1)
                    {
                        res += (VertexDeclTypes & (0xFu << (i * 4)));
                    }
                }
                return res;
            }
        }
        public VertexType VertexType { get { return (VertexType)VertexDeclFlags; } }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Matrix = reader.ReadStruct<Matrix>();
            this.VertexDeclFlags = reader.ReadUInt32();
            this.VertexDeclStride = reader.ReadUInt16();
            this.VertexDeclUnk = reader.ReadByte();
            this.VertexDeclCount = reader.ReadByte();
            this.VertexDeclTypes = reader.ReadUInt64();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt16();
            this.Flags = reader.ReadUInt16();
            this.Vector1 = reader.ReadStruct<Vector3>();
            this.Vector2 = reader.ReadStruct<Vector3>();

            if (Unknown_50h != 0)
            { }
            if (Unknown_54h != 2)
            { }
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Matrix);
            writer.Write(this.VertexDeclFlags);
            writer.Write(this.VertexDeclStride);
            writer.Write(this.VertexDeclUnk);
            writer.Write(this.VertexDeclCount);
            writer.Write(this.VertexDeclTypes);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Flags);
            writer.Write(this.Vector1);
            writer.Write(this.Vector2);
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragVehicleGlassWindows : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return TotalLength; }
        }

        [TypeConverter(typeof(ExpandableObjectConverter))] public struct ItemOffsetStruct
        {
            public uint Item { get; set; }
            public uint Offset { get; set; }
            public override string ToString()
            {
                return Item.ToString() + ": " + Offset.ToString();
            }
        }
        [TypeConverter(typeof(ExpandableObjectConverter))] public class ItemStruct
        {
            public Matrix UnkMatrix { get; set; }
            public uint UnkUint1 { get; set; } = 0x56475743; // "VGWC"    vehicle glass window C..?
            public ushort ItemID { get; set; } //matches UnkStruct1.Item
            public ushort UnkUshort1 { get; set; }
            public ushort UnkUshort2 { get; set; }
            public ushort ItemDataCount { get; set; }//count of item data arrays
            public ushort ItemDataByteLength { get; set; }//total byte length of ItemDatas plus byte length of ItemDataOffsets
            public ushort UnkUshort3 { get; set; }
            public uint UnkUint2 { get; set; } = 0; //0
            public uint UnkUint3 { get; set; } = 0; //0
            public float UnkFloat0 { get; set; }
            public float UnkFloat1 { get; set; }
            public ushort UnkUshort4 { get; set; } //0, 1
            public ushort UnkUshort5 { get; set; } //2, 2050
            public float UnkFloat2 { get; set; }
            public uint UnkUint4 { get; set; } = 0; //0
            public uint UnkUint5 { get; set; } = 0; //0
            public ushort[] ItemDataOffsets { get; set; }//byte offsets for following array
            public ItemDataStruct[] ItemDatas { get; set; }

            public byte[] Leftovers { get; set; }//should just be leftover padding, TODO: getrid of this

            public uint TotalLength
            {
                get
                {
                    uint bc = 112;
                    bc += (ItemDataOffsets != null) ? ItemDataCount * 2u : 0;
                    bc += (uint)(Leftovers?.Length??0);
                    if (ItemDatas != null)
                    {
                        foreach (var u in ItemDatas)
                        {
                            bc += u.TotalLength;
                        }
                    }
                    return bc;
                }
            }

            public void Read(ResourceDataReader reader)
            {
                UnkMatrix = reader.ReadStruct<Matrix>();
                UnkUint1 = reader.ReadUInt32(); //0x56475743 "VGWC"
                ItemID = reader.ReadUInt16();
                UnkUshort1 = reader.ReadUInt16();
                UnkUshort2 = reader.ReadUInt16();
                ItemDataCount = reader.ReadUInt16();//count of item data arrays
                ItemDataByteLength = reader.ReadUInt16();//total byte length of ItemDatas plus byte length of ItemDataOffsets
                UnkUshort3 = reader.ReadUInt16();
                UnkUint2 = reader.ReadUInt32();//0
                UnkUint3 = reader.ReadUInt32();//0
                UnkFloat0 = reader.ReadSingle();
                UnkFloat1 = reader.ReadSingle();
                UnkUshort4 = reader.ReadUInt16();//0, 1
                UnkUshort5 = reader.ReadUInt16();//2, 2050
                UnkFloat2 = reader.ReadSingle();
                UnkUint4 = reader.ReadUInt32();//0
                UnkUint5 = reader.ReadUInt32();//0


                if (ItemDataByteLength != 0)//sometimes this is 0 and UnkUshort3>0, which is weird
                {
                    ItemDataOffsets = reader.ReadStructs<ushort>(ItemDataCount);//byte offsets for following array

                    long coffset = 0;
                    ItemDatas = new ItemDataStruct[ItemDataCount];
                    for (int i = 0; i < ItemDataCount; i++)
                    {
                        var toffset = ItemDataOffsets[i];
                        var cbrem = toffset - coffset;
                        if (cbrem > 0)
                        {
                            var leftovers = reader.ReadBytes((int)cbrem);
                            if (i > 0)
                            {
                                ItemDatas[i - 1].Leftovers = leftovers;
                            }
                            else
                            { }
                            coffset += cbrem;
                        }
                        else if (cbrem < 0)
                        { }

                        var rpos = reader.Position;
                        var u = new ItemDataStruct();
                        u.Read(reader);
                        ItemDatas[i] = u;
                        coffset += reader.Position - rpos;
                    }

                }
                else
                { }


                if ((UnkUint2 != 0) || (UnkUint3 != 0) || (UnkUint4 != 0) || (UnkUint5 != 0))
                { }
                if ((UnkUshort4 != 0) && (UnkUshort4 != 1))  //1 in carbonrs.yft, policeb.yft, vader.yft
                { }
                if ((UnkUshort5 != 2) && (UnkUshort5 != 2050))  //2050 in cablecar.yft, submersible2.yft 
                { }

            }
            public void Write(ResourceDataWriter writer)
            {
                writer.Write(UnkMatrix);
                writer.Write(UnkUint1);
                writer.Write(ItemID);
                writer.Write(UnkUshort1);
                writer.Write(UnkUshort2);
                writer.Write(ItemDataCount);
                writer.Write(ItemDataByteLength);
                writer.Write(UnkUshort3);
                writer.Write(UnkUint2);
                writer.Write(UnkUint3);
                writer.Write(UnkFloat0);
                writer.Write(UnkFloat1);
                writer.Write(UnkUshort4);
                writer.Write(UnkUshort5);
                writer.Write(UnkFloat2);
                writer.Write(UnkUint4);
                writer.Write(UnkUint5);
                writer.WriteStructs(ItemDataOffsets);

                if (ItemDatas != null)
                {
                    foreach (var ud in ItemDatas)
                    {
                        ud.Write(writer);
                    }
                }

                if (Leftovers != null)
                {
                    writer.Write(Leftovers);
                }

            }

            public override string ToString()
            {
                return ItemID.ToString() + ": " + UnkUshort1.ToString() + ": " + UnkUshort2.ToString() + ": " + ItemDataCount.ToString() + ": " + ItemDataByteLength.ToString() + ": " + UnkUshort3.ToString();
            }
        }
        [TypeConverter(typeof(ExpandableObjectConverter))] public class ItemDataStruct
        {
            public byte UnkByte0 { get; set; }
            public byte UnkByte1 { get; set; }
            public byte[] UnkBytes { get; set; }
            public byte[] Leftovers { get; set; }//still contains some data. how to read it properly? TODO: getrid of this

            public uint TotalLength
            {
                get
                {
                    uint bc = 2;
                    bc += (uint)(UnkBytes?.Length ?? 0);
                    bc += (uint)(Leftovers?.Length ?? 0);
                    return bc;
                }
            }

            public void Read(ResourceDataReader reader)
            {
                UnkByte0 = reader.ReadByte();//start?
                UnkByte1 = reader.ReadByte();//end?

                int n = (UnkByte1 - UnkByte0) + 2;
                if (n > 0)
                {
                    UnkBytes = reader.ReadBytes(n);
                }
                else if (n < 0)
                { }

            }
            public void Write(ResourceDataWriter writer)
            {
                writer.Write(UnkByte0);
                writer.Write(UnkByte1);

                if (UnkBytes != null)
                {
                    writer.Write(UnkBytes);
                }
                if (Leftovers != null)
                {
                    writer.Write(Leftovers);
                }
            }

            public override string ToString()
            {
                return UnkByte0.ToString() + ": " + UnkByte1.ToString();
            }
        }


        // structure data
        public uint Unknown_0h { get; set; } = 0x56475748; // "VGWH"   ...vehicle glass window H..?
        public ushort Unknown_4h { get; set; } = 112;// = length of item headers
        public ushort ItemCount { get; set; }
        public uint TotalLength { get; set; }
        public ItemOffsetStruct[] ItemOffsets { get; set; }
        public uint UnkUint0 { get; set; } = 0;
        public ItemStruct[] Items { get; set; }

        public byte[] Leftovers { get; set; } //leftover (unparsed) data, should just be padding. TODO: getrid of this!


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            Unknown_0h = reader.ReadUInt32(); // "VGWH"   ...vehicle glass window H..?
            Unknown_4h = reader.ReadUInt16(); //112 = length of item headers
            ItemCount = reader.ReadUInt16();
            TotalLength = reader.ReadUInt32();
            ItemOffsets = reader.ReadStructs<ItemOffsetStruct>(ItemCount + (ItemCount & 1u)); //offsets in here start at just after UnkUint0
            UnkUint0 = reader.ReadUInt32();//0

            long coffset = 16 + ItemOffsets.Length*8;

            Items = new ItemStruct[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                var us1 = ItemOffsets[i];
                var cbrem = us1.Offset - coffset;
                if (cbrem > 0)
                {
                    var leftovers = reader.ReadBytes((int)cbrem);
                    if (i > 0)
                    {
                        Items[i - 1].Leftovers = leftovers;
                    }
                    else
                    { }
                    coffset += cbrem;
                }
                else if (cbrem < 0)
                { }

                var rpos = reader.Position;
                var u = new ItemStruct();
                u.Read(reader);
                Items[i] = u;
                coffset += reader.Position - rpos;
            }

            var leftover = (int)(TotalLength - coffset);
            if (leftover > 0)
            {
                Leftovers = reader.ReadBytes(leftover);
                if (ItemCount > 0)
                {
                    Items[ItemCount - 1].Leftovers = Leftovers;
                    Leftovers = null;//hackity hack
                }
                else
                { }
            }
            else if (leftover < 0)
            { }


            if (Unknown_4h != 112)
            { }
            if (UnkUint0 != 0)
            { }

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            long bc = 16;
            bc += ItemOffsets.Length*8;
            foreach (var ud2 in Items)
            {
                bc += ud2.TotalLength;
            }
            bc += (uint)(Leftovers?.Length ?? 0);

            if (TotalLength != bc)
            { }
            TotalLength = (uint)bc;

            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.ItemCount);
            writer.Write(this.TotalLength);


            writer.WriteStructs(ItemOffsets);
            writer.Write(UnkUint0);

            foreach (var ud2 in Items)
            {
                ud2.Write(writer);
            }

            if (Leftovers != null)
            {
                writer.Write(Leftovers);
            }
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
        public Vector4 Unknown_30h { get; set; }
        public Vector4 Unknown_40h { get; set; }
        public Vector4 Unknown_50h { get; set; }
        public Vector4 Unknown_60h { get; set; }
        public Vector4 Unknown_70h { get; set; }
        public Vector4 Unknown_80h { get; set; }
        public Vector4 Unknown_90h { get; set; }
        public Vector4 Unknown_A0h { get; set; }
        public Vector4 Unknown_B0h { get; set; }
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
        public Vector4[] InertiaTensors { get; set; }
        public Vector4[] Unknown_F8h_Data { get; set; }
        public FragPhysUnknown_F_002 FragTransforms { get; set; }
        public byte[] Unknown_108h_Data { get; set; }
        public byte[] Unknown_110h_Data { get; set; }


        private ResourceSystemStructBlock<uint> Unknown_28h_DataBlock = null; //used only for saving
        private ResourceSystemStructBlock<Vector4> InertiaTensorsBlock = null;
        private ResourceSystemStructBlock<Vector4> Unknown_F8h_DataBlock = null;
        private ResourceSystemStructBlock<byte> Unknown_108h_DataBlock = null;
        private ResourceSystemStructBlock<byte> Unknown_110h_DataBlock = null;


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
            this.Unknown_30h = reader.ReadStruct<Vector4>();
            this.Unknown_40h = reader.ReadStruct<Vector4>();
            this.Unknown_50h = reader.ReadStruct<Vector4>();
            this.Unknown_60h = reader.ReadStruct<Vector4>();
            this.Unknown_70h = reader.ReadStruct<Vector4>();
            this.Unknown_80h = reader.ReadStruct<Vector4>();
            this.Unknown_90h = reader.ReadStruct<Vector4>();
            this.Unknown_A0h = reader.ReadStruct<Vector4>();
            this.Unknown_B0h = reader.ReadStruct<Vector4>();
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
            this.InertiaTensors = reader.ReadStructsAt<Vector4>(this.InertiaTensorsPointer, this.ChildrenCount);
            this.Unknown_F8h_Data = reader.ReadStructsAt<Vector4>(this.Unknown_F8h_Pointer, this.ChildrenCount);
            this.FragTransforms = reader.ReadBlockAt<FragPhysUnknown_F_002>(
                this.FragTransformsPointer // offset
            );
            this.Unknown_108h_Data = reader.ReadBytesAt(this.Unknown_108h_Pointer, this.Count1);
            this.Unknown_110h_Data = reader.ReadBytesAt(this.Unknown_110h_Pointer, this.Count2);



            if ((Children != null) && (Children.data_items != null))
            {
                for (int i = 0; i < Children.data_items.Length; i++)
                {
                    var child = Children.data_items[i];
                    var gi = child.GroupIndex;
                    child.OwnerFragPhysLod = this;
                    child.OwnerFragPhysIndex = i;

                    if ((Groups?.data_items != null) && (gi < Groups.data_items.Length))
                    {
                        var group = Groups.data_items[gi];
                        var str = group.Name.ToString().ToLowerInvariant();
                        JenkIndex.Ensure(str);
                        child.GroupNameHash = JenkHash.GenHash(str);
                    }
                }
            }

            if (Bound != null)
            {
                Bound.Owner = this;
            }
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.ArticulatedBodyTypePointer = (ulong)(this.ArticulatedBodyType != null ? this.ArticulatedBodyType.FilePosition : 0);
            this.Unknown_28h_Pointer = (ulong)(this.Unknown_28h_DataBlock != null ? this.Unknown_28h_DataBlock.FilePosition : 0);
            this.GroupNamesPointer = (ulong)(this.GroupNames != null ? this.GroupNames.FilePosition : 0);
            this.GroupsPointer = (ulong)(this.Groups != null ? this.Groups.FilePosition : 0);
            this.ChildrenPointer = (ulong)(this.Children != null ? this.Children.FilePosition : 0);
            this.Archetype1Pointer = (ulong)(this.Archetype1 != null ? this.Archetype1.FilePosition : 0);
            this.Archetype2Pointer = (ulong)(this.Archetype2 != null ? this.Archetype2.FilePosition : 0);
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);
            this.InertiaTensorsPointer = (ulong)(this.InertiaTensorsBlock != null ? this.InertiaTensorsBlock.FilePosition : 0);
            this.Unknown_F8h_Pointer = (ulong)(this.Unknown_F8h_DataBlock != null ? this.Unknown_F8h_DataBlock.FilePosition : 0);
            this.FragTransformsPointer = (ulong)(this.FragTransforms != null ? this.FragTransforms.FilePosition : 0);
            this.Unknown_108h_Pointer = (ulong)(this.Unknown_108h_DataBlock != null ? this.Unknown_108h_DataBlock.FilePosition : 0);
            this.Unknown_110h_Pointer = (ulong)(this.Unknown_110h_DataBlock != null ? this.Unknown_110h_DataBlock.FilePosition : 0);

            this.Count1 = (byte)(this.Unknown_108h_DataBlock != null ? this.Unknown_108h_DataBlock.ItemCount : 0);
            this.Count2 = (byte)(this.Unknown_110h_DataBlock != null ? this.Unknown_110h_DataBlock.ItemCount : 0);
            this.GroupsCount = (byte)(this.Groups != null ? this.Groups.Count : 0);
            this.ChildrenCount = (byte)(this.Children != null ? this.Children.Count : 0);


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
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_B0h);
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
            if (Unknown_28h_Data != null)
            {
                Unknown_28h_DataBlock = new ResourceSystemStructBlock<uint>(Unknown_28h_Data);
                list.Add(Unknown_28h_DataBlock);
            }
            if (Groups != null) list.Add(Groups);
            if (Children != null) list.Add(Children);
            if (Archetype1 != null) list.Add(Archetype1);
            if (Archetype2 != null) list.Add(Archetype2);
            if (Bound != null) list.Add(Bound);
            if (InertiaTensors != null)
            {
                InertiaTensorsBlock = new ResourceSystemStructBlock<Vector4>(InertiaTensors);
                list.Add(InertiaTensorsBlock);
            }
            if (Unknown_F8h_Data != null)
            {
                Unknown_F8h_DataBlock = new ResourceSystemStructBlock<Vector4>(Unknown_F8h_Data);
                list.Add(Unknown_F8h_DataBlock);
            }
            if (FragTransforms != null) list.Add(FragTransforms);
            if (Unknown_108h_Data != null)
            {
                Unknown_108h_DataBlock = new ResourceSystemStructBlock<byte>(Unknown_108h_Data);
                list.Add(Unknown_108h_DataBlock);
            }
            if (Unknown_110h_Data != null)
            {
                Unknown_110h_DataBlock = new ResourceSystemStructBlock<byte>(Unknown_110h_Data);
                list.Add(Unknown_110h_DataBlock);
            }
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
        public ulong UnknownPointer { get; set; }
        public byte UnknownCount { get; set; }
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
        public Vector4[] UnknownData { get; set; }



        private ResourceSystemStructBlock<Vector4> UnknownDataBlock = null;//only used for saving


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
            this.UnknownPointer = reader.ReadUInt64();
            this.UnknownCount = reader.ReadByte();
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
            this.UnknownData = reader.ReadStructsAt<Vector4>(this.UnknownPointer, this.UnknownCount);

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.JointTypesPointer = (ulong)(this.JointTypes != null ? this.JointTypes.FilePosition : 0);
            this.UnknownPointer = (ulong)(this.UnknownDataBlock != null ? this.UnknownDataBlock.FilePosition : 0);
            this.UnknownCount = (byte)(this.UnknownDataBlock != null ? this.UnknownDataBlock.ItemCount : 0);
            this.JointTypesCount = (byte)(this.JointTypes != null ? this.JointTypes.Count : 0);

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
            writer.Write(this.UnknownPointer);
            writer.Write(this.UnknownCount);
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
            if (UnknownData != null)
            {
                UnknownDataBlock = new ResourceSystemStructBlock<Vector4>(UnknownData);
                list.Add(UnknownDataBlock);
            }
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
        public uint Unknown_A8h { get; set; } // 0x4CBEBC20  (float: 1.0E8)
        public uint Unknown_ACh { get; set; } // 0xCCBEBC20  (float:-1.0E8)

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
            get { return 32 + ((Data?.Length ?? 0) * 64); }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h { get; set; } // 0x00000001
        public uint Unknown_08h { get; set; } // 0x00000000
        public uint Unknown_0Ch { get; set; } // 0x00000000
        public uint DataCount { get; set; }
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public Matrix[] Data { get; set; }

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
            this.DataCount = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Data = reader.ReadStructsAt<Matrix>((ulong)reader.Position, DataCount);

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            DataCount = (uint)(Data?.Length ?? 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_0Ch);
            writer.Write(this.DataCount);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteStructs(Data);
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
        public Vector4 Unknown_60h { get; set; }
        public Vector4 Unknown_70h { get; set; }
        public Vector4 Unknown_80h { get; set; } // 0.0 0.0 0.0 NaN
        public Vector4 Unknown_90h { get; set; } // 0.0 0.0 0.0 NaN
        public Vector4 Unknown_A0h { get; set; } // 0.0 0.0 0.0 NaN
        public Vector4 Unknown_B0h { get; set; } // 0.0 0.0 0.0 NaN
        public Vector4 Unknown_C0h { get; set; } // 0.0 0.0 0.0 NaN
        public Vector4 Unknown_D0h { get; set; } // 0.0 0.0 0.0 NaN

        // reference data
        public string Name { get; set; }
        public Bounds Bound { get; set; }


        private string_r NameBlock = null;//used only when saving


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
            this.Unknown_60h = reader.ReadStruct<Vector4>();
            this.Unknown_70h = reader.ReadStruct<Vector4>();
            this.Unknown_80h = reader.ReadStruct<Vector4>();
            this.Unknown_90h = reader.ReadStruct<Vector4>();
            this.Unknown_A0h = reader.ReadStruct<Vector4>();
            this.Unknown_B0h = reader.ReadStruct<Vector4>();
            this.Unknown_C0h = reader.ReadStruct<Vector4>();
            this.Unknown_D0h = reader.ReadStruct<Vector4>();

            // read reference data
            this.Name = reader.ReadStringAt(//BlockAt<string_r>(
                this.NamePointer // offset
            );
            this.Bound = reader.ReadBlockAt<Bounds>(
                this.BoundPointer // offset
            );

            if (Bound != null)
            {
                Bound.Owner = this;
            }
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.NameBlock != null ? this.NameBlock.FilePosition : 0);
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
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_B0h);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_D0h);
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
        public ushort GroupIndex { get; set; }
        public ushort BoneTag { get; set; }
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
        public MetaHash GroupNameHash { get; set; }

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
            this.GroupIndex = reader.ReadUInt16();
            this.BoneTag = reader.ReadUInt16();
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
            writer.Write(this.GroupIndex);
            writer.Write(this.BoneTag);
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


        public override string ToString()
        {
            return GroupNameHash.ToString();
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
        public byte Unknown_50ha { get; set; }
        public byte Unknown_50hb { get; set; }//0xFF
        public ushort Unknown_50hc { get; set; }//0
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