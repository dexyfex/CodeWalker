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
using System.Xml;

namespace CodeWalker.GameFiles
{



    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragType : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 304; }
        }

        // structure data
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public Vector3 BoundingSphereCenter { get; set; }
        public float BoundingSphereRadius { get; set; }
        public ulong DrawablePointer { get; set; }
        public ulong DrawableArrayPointer { get; set; }
        public ulong DrawableArrayNamesPointer { get; set; }
        public uint DrawableArrayCount { get; set; }
        public int DrawableArrayFlag { get; set; }  // 0, -1   (DrawableArray flag: 0 when ArrayCount>0, -1 when not)
        public ulong Unknown_50h; // 0x0000000000000000
        public ulong NamePointer { get; set; }
        public ResourcePointerList64<EnvironmentCloth> Cloths { get; set; }
        public ulong Unknown_70h; // 0x0000000000000000
        public ulong Unknown_78h; // 0x0000000000000000
        public ulong Unknown_80h; // 0x0000000000000000
        public ulong Unknown_88h; // 0x0000000000000000
        public ulong Unknown_90h; // 0x0000000000000000
        public ulong Unknown_98h; // 0x0000000000000000
        public ulong Unknown_A0h; // 0x0000000000000000
        public ulong BoneTransformsPointer { get; set; }
        public int Unknown_B0h { get; set; } // 0, 944, 1088, 1200
        public int Unknown_B4h; //0x00000000
        public int Unknown_B8h { get; set; } // -364 to 16976, multiple of 16!
        public int Unknown_BCh { get; set; } // 0, -1
        public int Unknown_C0h { get; set; } // 0, 256, 512, 768, 1024, 65280
        public int Unknown_C4h { get; set; } // 1, 3, 65, 67
        public int Unknown_C8h = -1; // -1
        public float Unknown_CCh { get; set; }
        public float Unknown_D0h { get; set; }
        public float Unknown_D4h { get; set; }
        public byte Unknown_D8h; // 0x00
        public byte GlassWindowsCount { get; set; }
        public ushort Unknown_DAh; // 0x0000
        public uint Unknown_DCh; // 0x00000000
        public ulong GlassWindowsPointer { get; set; }
        public ulong Unknown_E8h; // 0x0000000000000000
        public ulong PhysicsLODGroupPointer { get; set; }
        public ulong Drawable2Pointer { get; set; }
        public ulong Unknown_100h; // 0x0000000000000000
        public ulong Unknown_108h; // 0x0000000000000000
        public ResourceSimpleList64_s<LightAttributes_s> LightAttributes { get; set; }
        public ulong VehicleGlassWindowsPointer { get; set; }
        public ulong Unknown_128h; // 0x0000000000000000

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


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.BoundingSphereCenter = reader.ReadVector3();
            this.BoundingSphereRadius = reader.ReadSingle();
            this.DrawablePointer = reader.ReadUInt64();
            this.DrawableArrayPointer = reader.ReadUInt64();
            this.DrawableArrayNamesPointer = reader.ReadUInt64();
            this.DrawableArrayCount = reader.ReadUInt32();
            this.DrawableArrayFlag = reader.ReadInt32();
            this.Unknown_50h = reader.ReadUInt64();
            this.NamePointer = reader.ReadUInt64();
            this.Cloths = reader.ReadBlock<ResourcePointerList64<EnvironmentCloth>>();
            this.Unknown_70h = reader.ReadUInt64();
            this.Unknown_78h = reader.ReadUInt64();
            this.Unknown_80h = reader.ReadUInt64();
            this.Unknown_88h = reader.ReadUInt64();
            this.Unknown_90h = reader.ReadUInt64();
            this.Unknown_98h = reader.ReadUInt64();
            this.Unknown_A0h = reader.ReadUInt64();
            this.BoneTransformsPointer = reader.ReadUInt64();
            this.Unknown_B0h = reader.ReadInt32();
            this.Unknown_B4h = reader.ReadInt32();
            this.Unknown_B8h = reader.ReadInt32();
            this.Unknown_BCh = reader.ReadInt32();
            this.Unknown_C0h = reader.ReadInt32();
            this.Unknown_C4h = reader.ReadInt32();
            this.Unknown_C8h = reader.ReadInt32();
            this.Unknown_CCh = reader.ReadSingle();
            this.Unknown_D0h = reader.ReadSingle();
            this.Unknown_D4h = reader.ReadSingle();
            this.Unknown_D8h = reader.ReadByte();
            this.GlassWindowsCount = reader.ReadByte();
            this.Unknown_DAh = reader.ReadUInt16();
            this.Unknown_DCh = reader.ReadUInt32();
            this.GlassWindowsPointer = reader.ReadUInt64();
            this.Unknown_E8h = reader.ReadUInt64();
            this.PhysicsLODGroupPointer = reader.ReadUInt64();
            this.Drawable2Pointer = reader.ReadUInt64();
            this.Unknown_100h = reader.ReadUInt64();
            this.Unknown_108h = reader.ReadUInt64();
            this.LightAttributes = reader.ReadBlock<ResourceSimpleList64_s<LightAttributes_s>>();
            this.VehicleGlassWindowsPointer = reader.ReadUInt64();
            this.Unknown_128h = reader.ReadUInt64();

            // read reference data
            Drawable = reader.ReadBlockAt<FragDrawable>(this.DrawablePointer);
            DrawableArray = reader.ReadBlockAt<ResourcePointerArray64<FragDrawable>>(DrawableArrayPointer, DrawableArrayCount);
            DrawableArrayNames = reader.ReadBlockAt<ResourcePointerArray64<string_r>>(DrawableArrayNamesPointer, DrawableArrayCount);
            Name = reader.ReadStringAt(NamePointer);
            BoneTransforms = reader.ReadBlockAt<FragBoneTransforms>(BoneTransformsPointer);
            GlassWindows = reader.ReadBlockAt<ResourcePointerArray64<FragGlassWindow>>(GlassWindowsPointer, GlassWindowsCount);
            PhysicsLODGroup = reader.ReadBlockAt<FragPhysicsLODGroup>(PhysicsLODGroupPointer);
            Drawable2 = reader.ReadBlockAt<FragDrawable>(Drawable2Pointer);
            VehicleGlassWindows = reader.ReadBlockAt<FragVehicleGlassWindows>(VehicleGlassWindowsPointer);

            if (Drawable != null)
            {
                Drawable.OwnerFragment = this;
            }
            if (Drawable2 != null)
            {
                Drawable2.OwnerFragment = this;
            }
            if (DrawableArray?.data_items != null)
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




            //just testing!!
            if (BoundingSphereRadius <= 0.0f)
            { }
            if (DrawableArrayFlag != ((DrawableArrayCount == 0) ? -1 : 0))
            { }
            switch (Unknown_B0h)
            {
                case 0:
                case 944:
                case 1088:
                case 1200:
                    break;
                default:
                    break;
            }
            switch (Unknown_B8h)
            {
                case -364:
                case -80:
                case -48:
                case -32:
                case -16:
                case 0:
                case 16:
                case 32:
                case 48:
                case 64:
                case 96:
                case 160:
                case 192:
                case 208:
                case 480:
                case 736:
                case 3356:
                case 5008:
                case 5024:
                case 5056:
                case 5520:
                case 5680:
                case 5856:
                case 6176:
                case 6432:
                case 6448:
                case 7088:
                case 7216:
                case 7776:
                case 8032:
                case 8048:
                case 8064:
                case 8080:
                case 8112:
                case 8144:
                case 8688:
                case 8928:
                case 9216:
                case 9808:
                case 9872:
                case 9888:
                case 10480:
                case 10704:
                case 11248:
                case 13696:
                case 14344:
                case 14640:
                case 14720:
                case 14752:
                case 14768:
                case 14944:
                case 15112:
                case 16656:
                case 16976:
                    break;
                default:
                    break;
            }
            switch (Unknown_BCh)
            {
                case 0:
                case -1:
                    break;
                default:
                    break;
            }
            switch (Unknown_C0h)
            {
                case 0:
                case 256:
                case 512:
                case 768:
                case 1024:
                case 65280:
                    break;
                default:
                    break;
            }
            switch (Unknown_C4h)
            {
                case 1:
                case 3:
                case 65:
                case 67:
                    break;
                default:
                    break;
            }


        }
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
            writer.Write(this.Unknown_18h);
            writer.Write(this.BoundingSphereCenter);
            writer.Write(this.BoundingSphereRadius);
            writer.Write(this.DrawablePointer);
            writer.Write(this.DrawableArrayPointer);
            writer.Write(this.DrawableArrayNamesPointer);
            writer.Write(this.DrawableArrayCount);
            writer.Write(this.DrawableArrayFlag);
            writer.Write(this.Unknown_50h);
            writer.Write(this.NamePointer);
            writer.WriteBlock(this.Cloths);
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_88h);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_98h);
            writer.Write(this.Unknown_A0h);
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
            writer.Write(this.PhysicsLODGroupPointer);
            writer.Write(this.Drawable2Pointer);
            writer.Write(this.Unknown_100h);
            writer.Write(this.Unknown_108h);
            writer.WriteBlock(this.LightAttributes);
            writer.Write(this.VehicleGlassWindowsPointer);
            writer.Write(this.Unknown_128h);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            YftXml.StringTag(sb, indent, "Name", YftXml.XmlEscape(Name));
            YftXml.SelfClosingTag(sb, indent, "BoundingSphereCenter " + FloatUtil.GetVector3XmlString(BoundingSphereCenter));
            YftXml.ValueTag(sb, indent, "BoundingSphereRadius", FloatUtil.ToString(BoundingSphereRadius));
            YftXml.ValueTag(sb, indent, "UnknownB0", Unknown_B0h.ToString());
            YftXml.ValueTag(sb, indent, "UnknownB8", Unknown_B8h.ToString());
            YftXml.ValueTag(sb, indent, "UnknownBC", Unknown_BCh.ToString());
            YftXml.ValueTag(sb, indent, "UnknownC0", Unknown_C0h.ToString());
            YftXml.ValueTag(sb, indent, "UnknownC4", Unknown_C4h.ToString());
            YftXml.ValueTag(sb, indent, "UnknownCC", FloatUtil.ToString(Unknown_CCh));
            YftXml.ValueTag(sb, indent, "UnknownD0", FloatUtil.ToString(Unknown_D0h));
            YftXml.ValueTag(sb, indent, "UnknownD4", FloatUtil.ToString(Unknown_D4h));
            if (Drawable != null)
            {
                FragDrawable.WriteXmlNode(Drawable, sb, indent, ddsfolder, "Drawable");
            }
            if (Drawable2 != null)
            {
                FragDrawable.WriteXmlNode(Drawable2, sb, indent, ddsfolder, "Drawable2");
            }
            if ((DrawableArray?.data_items?.Length ?? 0) > 0)
            {
                var danames = DrawableArrayNames?.data_items;
                YftXml.OpenTag(sb, indent, "DrawableArray");
                for (int i = 0; i < DrawableArray.data_items.Length; i++)
                {
                    var d = DrawableArray.data_items[i];
                    var name = (i < (danames?.Length ?? 0)) ? danames[i] : null;
                    if (d.Name != name.Value)
                    { }
                    FragDrawable.WriteXmlNode(d, sb, indent + 1, ddsfolder, "Item");
                }
                YftXml.CloseTag(sb, indent, "DrawableArray");
            }
            if (BoneTransforms != null)
            {
                BoneTransforms.WriteXml(sb, indent, "BoneTransforms");
            }
            if (PhysicsLODGroup != null)
            {
                YftXml.OpenTag(sb, indent, "Physics");
                PhysicsLODGroup.WriteXml(sb, indent + 1, ddsfolder);
                YftXml.CloseTag(sb, indent, "Physics");
            }
            if (VehicleGlassWindows != null)
            {
                YftXml.OpenTag(sb, indent, "VehicleGlassWindows");
                VehicleGlassWindows.WriteXml(sb, indent + 1);
                YftXml.CloseTag(sb, indent, "VehicleGlassWindows");
            }
            if (GlassWindows?.data_items != null)
            {
                YftXml.WriteItemArray(sb, GlassWindows.data_items, indent, "GlassWindows");
            }
            if (LightAttributes?.data_items != null)
            {
                YftXml.WriteItemArray(sb, LightAttributes.data_items, indent, "Lights");
            }
            if (Cloths?.data_items != null)
            {
                YftXml.WriteItemArray(sb, Cloths.data_items, indent, "Cloths");
            }
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            BoundingSphereCenter = Xml.GetChildVector3Attributes(node, "BoundingSphereCenter", "x", "y", "z");
            BoundingSphereRadius = Xml.GetChildFloatAttribute(node, "BoundingSphereRadius", "value");
            Unknown_B0h = Xml.GetChildIntAttribute(node, "UnknownB0", "value");
            Unknown_B8h = Xml.GetChildIntAttribute(node, "UnknownB8", "value");
            Unknown_BCh = Xml.GetChildIntAttribute(node, "UnknownBC", "value");
            Unknown_C0h = Xml.GetChildIntAttribute(node, "UnknownC0", "value");
            Unknown_CCh = Xml.GetChildFloatAttribute(node, "UnknownCC", "value");
            Unknown_D0h = Xml.GetChildFloatAttribute(node, "UnknownD0", "value");
            Unknown_D4h = Xml.GetChildFloatAttribute(node, "UnknownD4", "value");
            var dnode = node.SelectSingleNode("Drawable");
            if (dnode != null)
            {
                Drawable = FragDrawable.ReadXmlNode(dnode, ddsfolder);
            }
            var dnode2 = node.SelectSingleNode("Drawable2");
            if (dnode2 != null)
            {
                Drawable2 = FragDrawable.ReadXmlNode(dnode2, ddsfolder);
            }
            var danode = node.SelectSingleNode("DrawableArray");
            if (danode != null)
            {
                var dlist = new List<FragDrawable>();
                var nlist = new List<string_r>();
                var dnodes = danode.SelectNodes("Item");
                foreach (XmlNode dn in dnodes)
                {
                    var d = FragDrawable.ReadXmlNode(dn, ddsfolder);
                    dlist.Add(d);
                    nlist.Add((string_r)d?.Name);
                }
                DrawableArray = new ResourcePointerArray64<FragDrawable>();
                DrawableArray.data_items = dlist.ToArray();
                DrawableArrayNames = new ResourcePointerArray64<string_r>();
                DrawableArrayNames.data_items = nlist.ToArray();
            }
            var btnode = node.SelectSingleNode("BoneTransforms");
            if (btnode != null)
            {
                BoneTransforms = new FragBoneTransforms();
                BoneTransforms.ReadXml(btnode);
            }
            var pnode = node.SelectSingleNode("Physics");
            if (pnode != null)
            {
                PhysicsLODGroup = new FragPhysicsLODGroup();
                PhysicsLODGroup.ReadXml(pnode, ddsfolder);
            }
            var vgnode = node.SelectSingleNode("VehicleGlassWindows");
            if (vgnode != null)
            {
                VehicleGlassWindows = new FragVehicleGlassWindows();
                VehicleGlassWindows.ReadXml(vgnode);
            }
            var gwinds = XmlMeta.ReadItemArray<FragGlassWindow>(node, "GlassWindows");
            if ((gwinds != null) && (gwinds.Length > 0))
            {
                GlassWindows = new ResourcePointerArray64<FragGlassWindow>();
                GlassWindows.data_items = gwinds;
            }
            LightAttributes = new ResourceSimpleList64_s<LightAttributes_s>();
            LightAttributes.data_items = XmlMeta.ReadItemArray<LightAttributes_s>(node, "Lights");
            Cloths = new ResourcePointerList64<EnvironmentCloth>();
            Cloths.data_items = XmlMeta.ReadItemArray<EnvironmentCloth>(node, "Cloths");
        }
        public static void WriteXmlNode(FragType f, StringBuilder sb, int indent, string ddsfolder, string name = "Fragment")
        {
            if (f == null) return;
            YftXml.OpenTag(sb, indent, name);
            f.WriteXml(sb, indent + 1, ddsfolder);
            YftXml.CloseTag(sb, indent, name);
        }
        public static FragType ReadXmlNode(XmlNode node, string ddsfolder)
        {
            if (node == null) return null;
            var f = new FragType();
            f.ReadXml(node, ddsfolder);
            return f;
        }

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
                new Tuple<long, IResourceBlock>(0x60, Cloths),
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
        public ulong Unknown_0A8h; // 0x0000000000000000
        public Matrix FragMatrix { get; set; }
        public ulong BoundPointer { get; set; }
        public ulong FragMatricesIndsPointer { get; set; }
        public ushort FragMatricesIndsCount { get; set; }
        public ushort FragMatricesCapacity { get; set; }
        public uint Unknown_104h; // 0x00000000
        public ulong FragMatricesPointer { get; set; }
        public ushort FragMatricesCount { get; set; }
        public ushort Unknown_112h = 1; // 1
        public uint Unknown_114h; // 0x00000000
        public ulong Unknown_118h; // 0x0000000000000000
        public ulong Unknown_120h; // 0x0000000000000000
        public ulong Unknown_128h; // 0x0000000000000000
        public ulong NamePointer { get; set; }
        public ulong Unknown_138h; // 0x0000000000000000
        public ulong Unknown_140h; // 0x0000000000000000
        public ulong Unknown_148h; // 0x0000000000000000

        // reference data
        public Bounds Bound { get; set; }
        public ulong[] FragMatricesInds { get; set; }
        public Matrix[] FragMatrices { get; set; }
        public string Name { get; set; }

        public FragType OwnerFragment { get; set; } //for handy use
        public EnvironmentCloth OwnerCloth { get; set; }
        public FragPhysTypeChild OwnerFragmentPhys { get; set; }


        private ResourceSystemStructBlock<ulong> FragMatricesIndsBlock = null; //used for saving only
        private ResourceSystemStructBlock<Matrix> FragMatricesBlock = null;
        private string_r NameBlock = null;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_0A8h = reader.ReadUInt64();
            this.FragMatrix = reader.ReadStruct<Matrix>();
            this.BoundPointer = reader.ReadUInt64();
            this.FragMatricesIndsPointer = reader.ReadUInt64();
            this.FragMatricesIndsCount = reader.ReadUInt16();
            this.FragMatricesCapacity = reader.ReadUInt16();
            this.Unknown_104h = reader.ReadUInt32();
            this.FragMatricesPointer = reader.ReadUInt64();
            this.FragMatricesCount = reader.ReadUInt16();
            this.Unknown_112h = reader.ReadUInt16();
            this.Unknown_114h = reader.ReadUInt32();
            this.Unknown_118h = reader.ReadUInt64();
            this.Unknown_120h = reader.ReadUInt64();
            this.Unknown_128h = reader.ReadUInt64();
            this.NamePointer = reader.ReadUInt64();
            this.Unknown_138h = reader.ReadUInt64();
            this.Unknown_140h = reader.ReadUInt64();
            this.Unknown_148h = reader.ReadUInt64();

            // read reference data
            Bound = reader.ReadBlockAt<Bounds>(BoundPointer);
            FragMatricesInds = reader.ReadUlongsAt(FragMatricesIndsPointer, FragMatricesIndsCount);
            FragMatrices = reader.ReadStructsAt<Matrix>(FragMatricesPointer, FragMatricesCapacity);
            Name = reader.ReadStringAt(NamePointer);

            if (Bound != null)
            {
                Bound.Owner = this;
            }



            if (FragMatricesInds != null)
            { }
            if (FragMatrices != null)
            { }
            if ((FragMatrices != null) != (FragMatricesInds != null))
            { }

            //if ((FragMatricesCount != 0) != (FragMatricesCapacity != 0))
            //{ }
            //if (FragMatricesCount > FragMatricesCapacity)
            //{ }
            //if (Unknown_0A8h != 0)
            //{ }
            //if (Unknown_104h != 0)
            //{ }
            //if (Unknown_112h != 1)
            //{ }
            //if (Unknown_114h != 0)
            //{ }
            //if (Unknown_118h != 0)
            //{ }
            //if (Unknown_120h != 0)
            //{ }
            //if (Unknown_128h != 0)
            //{ }
            //if (Unknown_138h != 0)
            //{ }
            //if (Unknown_140h != 0)
            //{ }
            //if (Unknown_148h != 0)
            //{ }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);
            this.FragMatricesIndsPointer = (ulong)(this.FragMatricesIndsBlock != null ? this.FragMatricesIndsBlock.FilePosition : 0);
            this.FragMatricesIndsCount = (ushort)(this.FragMatricesIndsBlock != null ? this.FragMatricesIndsBlock.ItemCount : 0);
            this.FragMatricesCapacity = (ushort)(this.FragMatricesBlock != null ? this.FragMatricesBlock.ItemCount : 0);
            this.FragMatricesPointer = (ulong)(this.FragMatricesBlock != null ? this.FragMatricesBlock.FilePosition : 0);
            this.NamePointer = (ulong)(this.NameBlock != null ? this.NameBlock.FilePosition : 0);

            // write structure data
            writer.Write(this.Unknown_0A8h);
            writer.Write(this.FragMatrix);
            writer.Write(this.BoundPointer);
            writer.Write(this.FragMatricesIndsPointer);
            writer.Write(this.FragMatricesIndsCount);
            writer.Write(this.FragMatricesCapacity);
            writer.Write(this.Unknown_104h);
            writer.Write(this.FragMatricesPointer);
            writer.Write(this.FragMatricesCount);
            writer.Write(this.Unknown_112h);
            writer.Write(this.Unknown_114h);
            writer.Write(this.Unknown_118h);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_128h);
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_138h);
            writer.Write(this.Unknown_140h);
            writer.Write(this.Unknown_148h);
        }
        public override void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            YftXml.StringTag(sb, indent, "Name", YftXml.XmlEscape(Name));
            YftXml.WriteRawArray(sb, FragMatrix.ToArray(), indent, "Matrix", "", FloatUtil.ToString, 4);
            if ((FragMatrices != null) && (FragMatrices.Length > 0))
            {
                YftXml.OpenTag(sb, indent, "Matrices capacity=\"" + FragMatrices.Length.ToString() + "\"");
                var cind = indent + 1;
                var cnt = Math.Min(FragMatrices.Length, FragMatricesCount);
                for (int i = 0; i < FragMatrices.Length; i++)
                {
                    var idx = ((FragMatricesInds != null) && (i < FragMatricesInds.Length)) ? FragMatricesInds[i] : 0;
                    YftXml.OpenTag(sb, cind, "Item id=\"" + idx.ToString() + "\"");
                    YftXml.WriteRawArrayContent(sb, FragMatrices[i].ToArray(), cind + 1, FloatUtil.ToString, 4);
                    YftXml.CloseTag(sb, cind, "Item");
                }
                YftXml.CloseTag(sb, indent, "Matrices");
            }

            base.WriteXml(sb, indent, ddsfolder);

            Bounds.WriteXmlNode(Bound, sb, indent);
        }
        public override void ReadXml(XmlNode node, string ddsfolder)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            FragMatrix = Xml.GetChildMatrix(node, "Matrix");

            var msnode = node.SelectSingleNode("Matrices");
            if (msnode != null)
            {
                var mats = new List<Matrix>();
                var matinds = new List<ulong>();
                var cap = Xml.GetIntAttribute(msnode, "capacity");
                var inodes = msnode.SelectNodes("Item");
                foreach (XmlNode inode in inodes)
                {
                    var id = Xml.GetULongAttribute(inode, "id");
                    var mat = Xml.GetMatrix(inode);
                    matinds.Add(id);
                    mats.Add(mat);
                }
                for (int i = mats.Count; i < cap; i++)
                {
                    matinds.Add(0);
                    mats.Add(new Matrix(float.NaN));
                }
                FragMatrices = mats.ToArray();
                FragMatricesInds = matinds.ToArray();
            }

            base.ReadXml(node, ddsfolder);

            var bnode = node.SelectSingleNode("Bounds");
            if (bnode != null)
            {
                Bound = Bounds.ReadXmlNode(bnode, this);
            }
        }
        public static void WriteXmlNode(FragDrawable d, StringBuilder sb, int indent, string ddsfolder, string name = "FragDrawable")
        {
            if (d == null) return;
            YftXml.OpenTag(sb, indent, name);
            d.WriteXml(sb, indent + 1, ddsfolder);
            YftXml.CloseTag(sb, indent, name);
        }
        public static FragDrawable ReadXmlNode(XmlNode node, string ddsfolder)
        {
            if (node == null) return null;
            var d = new FragDrawable();
            d.ReadXml(node, ddsfolder);
            return d;
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if (Bound != null) list.Add(Bound);
            if (FragMatricesInds != null)
            {
                FragMatricesIndsBlock = new ResourceSystemStructBlock<ulong>(FragMatricesInds);
                list.Add(FragMatricesIndsBlock);
            }
            if (FragMatrices != null)
            {
                FragMatricesBlock = new ResourceSystemStructBlock<Matrix>(FragMatrices);
                list.Add(FragMatricesBlock);
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
        public ulong Unknown_00h; // 0x0000000000000000
        public ulong Unknown_08h; // 0x0000000000000000
        public byte ItemCount1 { get; set; }
        public byte ItemCount2 { get; set; }
        public ushort Unknown_12h { get; set; } // 0, 1
        public uint Unknown_14h; // 0x00000000
        public ulong Unknown_18h; // 0x0000000000000000
        public Matrix3_s[] Items { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_00h = reader.ReadUInt64();
            this.Unknown_08h = reader.ReadUInt64();
            this.ItemCount1 = reader.ReadByte();
            this.ItemCount2 = reader.ReadByte();
            this.Unknown_12h = reader.ReadUInt16();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt64();
            this.Items = reader.ReadStructs<Matrix3_s>(ItemCount1);

            //if (ItemCount1 != ItemCount2)
            //{ }
            //if ((Unknown_12h != 0) && (Unknown_12h != 1))
            //{ }
            //if (Unknown_00h != 0)
            //{ }
            //if (Unknown_08h != 0)
            //{ }
            //if (Unknown_14h != 0)
            //{ }
            //if (Unknown_18h != 0)
            //{ }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_00h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.ItemCount1);
            writer.Write(this.ItemCount2);
            writer.Write(this.Unknown_12h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.WriteStructs(Items);
        }
        public void WriteXml(StringBuilder sb, int indent, string name)
        {
            var tag = name + " unk=\"" + Unknown_12h.ToString() + "\"";
            if ((Items != null) && (Items.Length > 0))
            {
                YftXml.OpenTag(sb, indent, tag);
                var cind = indent + 1;
                foreach (var mat in Items)
                {
                    YftXml.OpenTag(sb, cind, "Item");
                    YftXml.WriteRawArrayContent(sb, mat.ToArray(), cind + 1, FloatUtil.ToString, 4);
                    YftXml.CloseTag(sb, cind, "Item");
                }
                YftXml.CloseTag(sb, indent, name);
            }
            else
            {
                YftXml.SelfClosingTag(sb, indent, tag);
            }
        }
        public void ReadXml(XmlNode node)
        {
            Unknown_12h = (ushort)Xml.GetUIntAttribute(node, "unk");
            var inodes = node.SelectNodes("Item");
            if (inodes != null)
            {
                var mats = new List<Matrix3_s>();
                foreach (XmlNode inode in inodes)
                {
                    var arr = Xml.GetRawFloatArray(inode);
                    var mat = new Matrix3_s(arr);
                    mats.Add(mat);
                }
                Items = (mats.Count > 0) ? mats.ToArray() : null;
                ItemCount1 = ItemCount2 = (byte)mats.Count;
            }

        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragGlassWindow : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength
        {
            get { return 112; }
        }

        // structure data
        public Matrix Matrix { get; set; } //column 4 is NaN,NaN,NaN,1
        public VertexDeclaration VertexDeclaration { get; set; } = new VertexDeclaration();
        public MetaHash Unknown_50h { get; set; } //looks floaty? flagsy? 0xXXXX0000
        public ushort Unknown_54h { get; set; } = 2; //2
        public ushort Flags { get; set; }//512, 768, 1280 etc ... flags
        public Vector3 Vector1 { get; set; }
        public Vector3 Vector2 { get; set; } // z = 0x7F800001 (NaN)

        public ushort Unknown50
        {
            get { return (ushort)(Unknown_50h.Hash >> 16); }
            set { Unknown_50h = (Unknown_50h & 0xFFFF) + ((uint)value << 16); }
        }

        public ulong VertexDeclId //this all equates to VertexTypePNCTT
        {
            get
            {
                return VertexDeclaration?.GetDeclarationId() ?? 0;
            }
        }
        public VertexType VertexType { get { return (VertexType)(VertexDeclaration?.Flags ?? 0); } }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Matrix = reader.ReadStruct<Matrix>();
            this.VertexDeclaration.Read(reader);
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
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Matrix);
            this.VertexDeclaration.Write(writer);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Flags);
            writer.Write(this.Vector1);
            writer.Write(this.Vector2);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YftXml.ValueTag(sb, indent, "Flags", Flags.ToString());
            YftXml.ValueTag(sb, indent, "Unknown50", Unknown50.ToString());
            YftXml.SelfClosingTag(sb, indent, "Vector1 " + FloatUtil.GetVector3XmlString(Vector1));
            YftXml.SelfClosingTag(sb, indent, "Vector2 " + FloatUtil.GetVector3XmlString(Vector2));
            YftXml.WriteRawArray(sb, Matrix.ToArray(), indent, "Matrix", "", FloatUtil.ToString, 4);
            VertexDeclaration.WriteXml(sb, indent, "Layout");
        }
        public void ReadXml(XmlNode node)
        {
            Flags = (ushort)Xml.GetChildUIntAttribute(node, "Flags", "value");
            Unknown50 = (ushort)Xml.GetChildUIntAttribute(node, "Unknown50", "value");
            Vector1 = Xml.GetChildVector3Attributes(node, "Vector1", "x", "y", "z");
            Vector2 = Xml.GetChildVector3Attributes(node, "Vector2", "x", "y", "z");
            Matrix = Xml.GetChildMatrix(node, "Matrix");
            VertexDeclaration.ReadXml(node.SelectSingleNode("Layout"));
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
            public Matrix Matrix { get; set; }
            public uint UnkUint1 { get; set; } = 0x56475743; // "VGWC"    vehicle glass window C..?
            public ushort ItemID { get; set; } //matches UnkStruct1.Item
            public ushort UnkUshort1 { get; set; }
            public ushort UnkUshort2 { get; set; }
            public ushort ItemDataCount { get; set; }//count of item data arrays
            public ushort ItemDataByteLength { get; set; }//total byte length of ItemDatas plus byte length of ItemDataOffsets
            public ushort UnkUshort3 { get; set; } = 0; //0
            public uint UnkUint2 { get; set; } = 0; //0
            public uint UnkUint3 { get; set; } = 0; //0
            public float UnkFloat1 { get; set; }
            public float UnkFloat2 { get; set; }
            public ushort UnkUshort4 { get; set; } //0, 1
            public ushort UnkUshort5 { get; set; } //2, 2050
            public float UnkFloat3 { get; set; }
            public uint UnkUint4 { get; set; } = 0; //0
            public uint UnkUint5 { get; set; } = 0; //0
            public ushort[] ItemDataOffsets { get; set; }//byte offsets for following array
            public ItemDataStruct[] ItemDatas { get; set; }

            public byte[] Padding { get; set; }//should just be leftover padding, TODO: getrid of this

            public uint ItemDataLength
            {
                get
                {
                    uint bc = (ItemDataOffsets != null) ? ItemDataCount * 2u : 0;
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
            public uint TotalLength
            {
                get
                {
                    uint bc = 112;
                    //bc += (uint)(Padding?.Length??0);
                    bc += ItemDataLength;
                    return bc;
                }
            }

            public void Read(ResourceDataReader reader)
            {
                Matrix = reader.ReadStruct<Matrix>();
                UnkUint1 = reader.ReadUInt32(); //0x56475743 "VGWC"
                ItemID = reader.ReadUInt16();
                UnkUshort1 = reader.ReadUInt16();
                UnkUshort2 = reader.ReadUInt16();
                ItemDataCount = reader.ReadUInt16();//count of item data arrays
                ItemDataByteLength = reader.ReadUInt16();//total byte length of ItemDatas plus byte length of ItemDataOffsets
                UnkUshort3 = reader.ReadUInt16();//0
                UnkUint2 = reader.ReadUInt32();//0
                UnkUint3 = reader.ReadUInt32();//0
                UnkFloat1 = reader.ReadSingle();
                UnkFloat2 = reader.ReadSingle();
                UnkUshort4 = reader.ReadUInt16();//0, 1
                UnkUshort5 = reader.ReadUInt16();//2, 2050
                UnkFloat3 = reader.ReadSingle();
                UnkUint4 = reader.ReadUInt32();//0
                UnkUint5 = reader.ReadUInt32();//0


                if (ItemDataByteLength != 0)//sometimes this is 0 and UnkUshort3>0, which is weird
                {
                    ItemDataOffsets = reader.ReadStructs<ushort>(ItemDataCount);//byte offsets for following array
                    ItemDatas = new ItemDataStruct[ItemDataCount];
                    for (int i = 0; i < ItemDataCount; i++)
                    {
                        //var toffset = ItemDataOffsets[i];
                        var u = new ItemDataStruct();
                        u.Read(reader);
                        ItemDatas[i] = u;
                    }
                }
                else
                { }


                //switch (UnkUshort1)
                //{
                //    case 3:
                //    case 4:
                //    case 5:
                //    case 6:
                //    case 7:
                //    case 8:
                //    case 9:
                //    case 10:
                //    case 11:
                //    case 12:
                //    case 13:
                //    case 14:
                //    case 15:
                //    case 16:
                //    case 17:
                //    case 18:
                //    case 19:
                //    case 20:
                //    case 21:
                //    case 22:
                //    case 23:
                //    case 24:
                //    case 25:
                //        break;
                //    default:
                //        break;
                //}
                //switch (UnkUshort2)
                //{
                //    case 1:
                //    case 10:
                //    case 15:
                //    case 20:
                //    case 22:
                //    case 23:
                //    case 25:
                //    case 26:
                //    case 28:
                //    case 29:
                //    case 32:
                //    case 34:
                //    case 36:
                //    case 37:
                //    case 39:
                //    case 41:
                //    case 46:
                //    case 47:
                //    case 48:
                //    case 49:
                //    case 50:
                //    case 51:
                //    case 52:
                //    case 53:
                //    case 54:
                //    case 55:
                //    case 56:
                //    case 57:
                //    case 58:
                //    case 59:
                //    case 61:
                //    case 66:
                //    case 74:
                //    case 77:
                //    case 106:
                //        break;
                //    default:
                //        break;//+more...
                //}
                //if (UnkUshort3 != 0)
                //{ }//no hit
                //if ((UnkUint2 != 0) || (UnkUint3 != 0) || (UnkUint4 != 0) || (UnkUint5 != 0))
                //{ }//no hit
                //if ((UnkUshort4 != 0) && (UnkUshort4 != 1))  //1 in carbonrs.yft, policeb.yft, vader.yft
                //{ }//no hit
                //if ((UnkUshort5 != 2) && (UnkUshort5 != 2050))  //2050 in cablecar.yft, submersible2.yft 
                //{ }//no hit

                ////testing!
                //BuildOffsets();
            }
            public void Write(ResourceDataWriter writer)
            {
                writer.Write(Matrix);
                writer.Write(UnkUint1);
                writer.Write(ItemID);
                writer.Write(UnkUshort1);
                writer.Write(UnkUshort2);
                writer.Write(ItemDataCount);
                writer.Write(ItemDataByteLength);
                writer.Write(UnkUshort3);
                writer.Write(UnkUint2);
                writer.Write(UnkUint3);
                writer.Write(UnkFloat1);
                writer.Write(UnkFloat2);
                writer.Write(UnkUshort4);
                writer.Write(UnkUshort5);
                writer.Write(UnkFloat3);
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

                //if (Padding != null)
                //{
                //    writer.Write(Padding);
                //}

            }
            public void WriteXml(StringBuilder sb, int indent)
            {
                YftXml.ValueTag(sb, indent, "ItemID", ItemID.ToString());
                YftXml.ValueTag(sb, indent, "UnkUshort1", UnkUshort1.ToString());
                YftXml.ValueTag(sb, indent, "UnkUshort2", UnkUshort2.ToString());
                YftXml.ValueTag(sb, indent, "UnkUshort4", UnkUshort4.ToString());
                YftXml.ValueTag(sb, indent, "UnkUshort5", UnkUshort5.ToString());
                YftXml.ValueTag(sb, indent, "UnkFloat1", FloatUtil.ToString(UnkFloat1));
                YftXml.ValueTag(sb, indent, "UnkFloat2", FloatUtil.ToString(UnkFloat2));
                YftXml.ValueTag(sb, indent, "UnkFloat3", FloatUtil.ToString(UnkFloat3));
                YftXml.WriteRawArray(sb, Matrix.ToArray(), indent, "Matrix", "", FloatUtil.ToString, 4);
                if (ItemDatas != null)
                {
                    YftXml.OpenTag(sb, indent, "Items");
                    var cind = indent + 1;
                    var cind2 = cind + 1;
                    foreach (var item in ItemDatas)
                    {
                        YftXml.OpenTag(sb, cind, "Item");
                        item.WriteXml(sb, cind2);
                        YftXml.CloseTag(sb, cind, "Item");
                    }
                    YftXml.CloseTag(sb, indent, "Items");
                }
            }
            public void ReadXml(XmlNode node)
            {
                ItemID = (ushort)Xml.GetChildUIntAttribute(node, "ItemID", "value");
                UnkUshort1 = (ushort)Xml.GetChildUIntAttribute(node, "UnkUshort1", "value");
                UnkUshort2 = (ushort)Xml.GetChildUIntAttribute(node, "UnkUshort2", "value");
                UnkUshort4 = (ushort)Xml.GetChildUIntAttribute(node, "UnkUshort4", "value");
                UnkUshort5 = (ushort)Xml.GetChildUIntAttribute(node, "UnkUshort5", "value");
                UnkFloat1 = Xml.GetChildFloatAttribute(node, "UnkFloat1", "value");
                UnkFloat2 = Xml.GetChildFloatAttribute(node, "UnkFloat2", "value");
                UnkFloat3 = Xml.GetChildFloatAttribute(node, "UnkFloat3", "value");
                Matrix = Xml.GetChildMatrix(node, "Matrix");
                var dnode = node.SelectSingleNode("Items");
                if (dnode != null)
                {
                    var ilist = new List<ItemDataStruct>();
                    var inodes = dnode.SelectNodes("Item");
                    if (inodes != null)
                    {
                        foreach (XmlNode inode in inodes)
                        {
                            var item = new ItemDataStruct();
                            item.ReadXml(inode);
                            ilist.Add(item);
                        }
                    }
                    ItemDatas = ilist.ToArray();
                }
                BuildOffsets();
            }

            public void BuildOffsets()
            {
                var o = 0u;
                var offs = new List<ushort>();
                //var maxlen = 0u;
                //var minlen = 0xFFFFFFFFu;
                if (ItemDatas != null)
                {
                    foreach (var item in ItemDatas)
                    {
                        offs.Add((ushort)o);
                        o += item.TotalLength;
                        var dl = item.DataLength + item.Start1;
                        //maxlen = Math.Max(maxlen, dl);
                        //minlen = Math.Min(minlen, dl);
                    }
                    o += (uint)(ItemDatas.Length * 2);
                }

                ////testing
                //if ((ItemDataOffsets?.Length ?? 0) != offs.Count)
                //{ }
                //else
                //{
                //    for (int i = 0; i < offs.Count; i++)
                //    {
                //        if (ItemDataOffsets[i] != offs[i])
                //        { }//no hit!
                //    }
                //}

                ItemDataOffsets = offs.ToArray();

                ////testing
                //if (ItemDataByteLength != o)
                //{ }//no hit!
                ItemDataByteLength = (ushort)o;

                ItemDataCount = (ushort)(ItemDatas?.Length ?? 0);

                //if (UnkUshort1 != minlen+1)
                //{ }
                //if (UnkUshort2 != maxlen)
                //{ }
                //UnkUshort2 = (ushort)maxunk;
            }

            public override string ToString()
            {
                return ItemID.ToString() + ": " + UnkUshort1.ToString() + ": " + UnkUshort2.ToString() + ": " + ItemDataCount.ToString() + ": " + ItemDataByteLength.ToString() + ": " + UnkUshort3.ToString();
            }
        }
        [TypeConverter(typeof(ExpandableObjectConverter))] public class ItemDataStruct
        {
            public byte Start1 { get; set; }
            public byte End1 { get; set; }
            public byte[] Data1 { get; set; }
            public byte Start2 { get; set; }
            public byte End2 { get; set; }
            public byte[] Data2 { get; set; }

            public uint DataLength
            {
                get
                {
                    uint bc = 0;
                    bc += (uint)(Data1?.Length ?? 0);
                    bc += (uint)(Data2?.Length ?? 0);
                    return bc;
                }
            }
            public uint TotalLength
            {
                get
                {
                    uint bc = 2;
                    bc += DataLength;
                    if (Data1 != null) bc++;
                    if (Start2 != 255) bc++;
                    return bc;
                }
            }


            public void Read(ResourceDataReader reader)
            {
                Start1 = reader.ReadByte();
                End1 = reader.ReadByte();

                int n = (End1 - Start1) + 1;
                if (n > 0)
                {
                    Data1 = reader.ReadBytes(n);
                    Start2 = reader.ReadByte();

                    if (Start2 != 255)
                    {
                        End2 = reader.ReadByte();

                        var n2 = (End2 - Start2) + 1;
                        if (n2 > 0)
                        {
                            Data2 = reader.ReadBytes(n2);
                        }
                    }
                }
                else
                { }

            }
            public void Write(ResourceDataWriter writer)
            {
                writer.Write(Start1);
                writer.Write(End1);

                if (Data1 != null)
                {
                    writer.Write(Data1);
                    writer.Write(Start2);

                    if (Start2 != 255)
                    {
                        writer.Write(End2);

                        if (Data2 != null)
                        {
                            writer.Write(Data2);
                        }
                    }
                }


            }
            public void WriteXml(StringBuilder sb, int indent)
            {
                YftXml.ValueTag(sb, indent, "Start1", Start1.ToString());
                if (Data1 != null)
                {
                    YftXml.WriteRawArray(sb, Data1, indent, "Data1", "");
                }
                if (Data2 != null)
                {
                    YftXml.ValueTag(sb, indent, "Start2", Start2.ToString());
                    YftXml.WriteRawArray(sb, Data2, indent, "Data2", "");
                }
            }
            public void ReadXml(XmlNode node)
            {
                Data1 = Xml.GetChildRawByteArray(node, "Data1", 10);
                Data2 = Xml.GetChildRawByteArray(node, "Data2", 10);
                Start1 = (byte)Xml.GetChildUIntAttribute(node, "Start1", "value");
                End1 = (byte)(Start1 + (Data1?.Length ?? 0) - 1);
                if (Data2 != null)
                {
                    Start2 = (byte)Xml.GetChildUIntAttribute(node, "Start2", "value");
                    End2 = (byte)(Start2 + (Data2?.Length ?? 0) - 1);
                }
                else
                {
                    Start2 = 255;
                    End2 = 0;
                }
            }

            public override string ToString()
            {
                return Start1.ToString() + ": " + End1.ToString() + ", " + Start2.ToString() + ": " + End2.ToString();
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
                var rpos = reader.Position;
                var u = new ItemStruct();
                u.Read(reader);
                Items[i] = u;
                coffset += reader.Position - rpos;

                var padd = (int)(16 - (coffset % 16)) % 16;
                if (padd > 0)
                {
                    u.Padding = reader.ReadBytes(padd);
                    coffset += padd;
                }
            }

            //if (Unknown_4h != 112)
            //{ }//no hit
            //if (UnkUint0 != 0)
            //{ }//no hit

            //// just testing
            //BuildOffsets();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            BuildOffsets();

            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.ItemCount);
            writer.Write(this.TotalLength);

            writer.WriteStructs(ItemOffsets);
            writer.Write(UnkUint0);

            long coffset = 16 + ItemOffsets.Length * 8;

            foreach (var item in Items)
            {
                var rpos = writer.Position;

                item.Write(writer);

                coffset += writer.Position - rpos;
                var padd = (int)(16 - (coffset % 16)) % 16;
                if (padd > 0)
                {
                    writer.Write(new byte[padd]);
                    coffset += padd;
                }
            }

        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    YftXml.OpenTag(sb, indent, "Item");
                    item.WriteXml(sb, indent + 1);
                    YftXml.CloseTag(sb, indent, "Item");
                }
            }
        }
        public void ReadXml(XmlNode node)
        {
            var inodes = node.SelectNodes("Item");
            var ilist = new List<ItemStruct>();
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var item = new ItemStruct();
                    item.ReadXml(inode);
                    ilist.Add(item);
                }
            }
            Items = ilist.ToArray();

            BuildOffsets();
        }



        public void BuildOffsets()
        {
            var offs = new List<ItemOffsetStruct>();
            var bc = 16u + (uint)ItemOffsets.Length*8u;
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    var off = new ItemOffsetStruct();
                    off.Item = item.ItemID;
                    off.Offset = bc;
                    offs.Add(off);
                    bc += item.TotalLength;
                    bc += (16 - (bc % 16)) % 16;//account for padding
                }
                if ((offs.Count % 2) != 0)
                {
                    offs.Add(new ItemOffsetStruct());
                }
            }

            //// just testing
            //if (TotalLength != bc)
            //{ }
            TotalLength = bc;

            //// just testing
            //if ((ItemOffsets?.Length ?? 0) != offs.Count)
            //{ }
            //else
            //{
            //    for (int i = 0; i < offs.Count; i++)
            //    {
            //        var oo = ItemOffsets[i];
            //        var no = offs[i];
            //        if ((no.Item != oo.Item) || (no.Offset != oo.Offset))
            //        { }
            //    }
            //}
            ItemOffsets = offs.ToArray();

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
        public uint Unknown_04h = 1; // 0x00000001
        public ulong Unknown_08h; // 0x0000000000000000
        public ulong PhysicsLOD1Pointer { get; set; }
        public ulong PhysicsLOD2Pointer { get; set; }
        public ulong PhysicsLOD3Pointer { get; set; }
        public ulong Unknown_28h; // 0x0000000000000000

        // reference data
        public FragPhysicsLOD PhysicsLOD1 { get; set; }
        public FragPhysicsLOD PhysicsLOD2 { get; set; }
        public FragPhysicsLOD PhysicsLOD3 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt64();
            this.PhysicsLOD1Pointer = reader.ReadUInt64();
            this.PhysicsLOD2Pointer = reader.ReadUInt64();
            this.PhysicsLOD3Pointer = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();

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

            //if (Unknown_04h != 1)
            //{ }//no hit
            //if (Unknown_08h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
        }
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
            writer.Write(this.PhysicsLOD1Pointer);
            writer.Write(this.PhysicsLOD2Pointer);
            writer.Write(this.PhysicsLOD3Pointer);
            writer.Write(this.Unknown_28h);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            if (PhysicsLOD1 != null)
            {
                YftXml.OpenTag(sb, indent, "LOD1");
                PhysicsLOD1.WriteXml(sb, indent + 1, ddsfolder);
                YftXml.CloseTag(sb, indent, "LOD1");
            }
            if (PhysicsLOD2 != null)
            {
                YftXml.OpenTag(sb, indent, "LOD2");
                PhysicsLOD2.WriteXml(sb, indent + 1, ddsfolder);
                YftXml.CloseTag(sb, indent, "LOD2");
            }
            if (PhysicsLOD3 != null)
            {
                YftXml.OpenTag(sb, indent, "LOD3");
                PhysicsLOD3.WriteXml(sb, indent + 1, ddsfolder);
                YftXml.CloseTag(sb, indent, "LOD3");
            }
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            var l1node = node.SelectSingleNode("LOD1");
            if (l1node != null)
            {
                PhysicsLOD1 = new FragPhysicsLOD();
                PhysicsLOD1.ReadXml(l1node, ddsfolder);
            }
            var l2node = node.SelectSingleNode("LOD2");
            if (l2node != null)
            {
                PhysicsLOD2 = new FragPhysicsLOD();
                PhysicsLOD2.ReadXml(l2node, ddsfolder);
            }
            var l3node = node.SelectSingleNode("LOD3");
            if (l3node != null)
            {
                PhysicsLOD3 = new FragPhysicsLOD();
                PhysicsLOD3.ReadXml(l3node, ddsfolder);
            }
        }

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
        public uint Unknown_04h = 1; // 0x00000001
        public ulong Unknown_08h; // 0x0000000000000000
        public uint Unknown_10h; // 0x00000000
        public float Unknown_14h { get; set; }
        public float Unknown_18h { get; set; }
        public float Unknown_1Ch { get; set; }
        public ulong ArticulatedBodyTypePointer { get; set; }
        public ulong ChildrenUnkUintsPointer { get; set; }
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
        public ulong ChildrenUnkVecsPointer { get; set; }
        public ulong FragTransformsPointer { get; set; }
        public ulong UnknownData1Pointer { get; set; }
        public ulong UnknownData2Pointer { get; set; }
        public byte UnknownData1Count { get; set; }
        public byte UnknownData2Count { get; set; }
        public byte GroupsCount { get; set; }
        public byte Unknown_11Bh { get; set; }
        public byte Unknown_11Ch { get; set; }
        public byte ChildrenCount { get; set; }
        public byte ChildrenCount2 { get; set; }
        public byte Unknown_11Fh; // 0x00
        public ulong Unknown_120h; // 0x0000000000000000
        public ulong Unknown_128h; // 0x0000000000000000

        // reference data
        public FragPhysArticulatedBodyType ArticulatedBodyType { get; set; }
        public uint[] ChildrenUnkUints { get; set; }
        public ResourcePointerArray64_s<FragPhysNameStruct_s> GroupNames { get; set; }
        public ResourcePointerArray64_s<FragPhysTypeGroup_s> Groups { get; set; }
        public ResourcePointerArray64<FragPhysTypeChild> Children { get; set; }
        public FragPhysArchetype Archetype1 { get; set; }
        public FragPhysArchetype Archetype2 { get; set; }
        public Bounds Bound { get; set; }
        public Vector4[] InertiaTensors { get; set; }
        public Vector4[] ChildrenUnkVecs { get; set; }
        public FragPhysUnknown_F_002 FragTransforms { get; set; }
        public byte[] UnknownData1 { get; set; }
        public byte[] UnknownData2 { get; set; }


        private ResourceSystemStructBlock<uint> ChildrenUnkUintsBlock = null; //used only for saving
        private ResourceSystemStructBlock<Vector4> InertiaTensorsBlock = null;
        private ResourceSystemStructBlock<Vector4> ChildrenUnkVecsBlock = null;
        private ResourceSystemStructBlock<byte> UnknownData1Block = null;
        private ResourceSystemStructBlock<byte> UnknownData2Block = null;


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadSingle();
            this.Unknown_18h = reader.ReadSingle();
            this.Unknown_1Ch = reader.ReadSingle();
            this.ArticulatedBodyTypePointer = reader.ReadUInt64();
            this.ChildrenUnkUintsPointer = reader.ReadUInt64();
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
            this.ChildrenUnkVecsPointer = reader.ReadUInt64();
            this.FragTransformsPointer = reader.ReadUInt64();
            this.UnknownData1Pointer = reader.ReadUInt64();
            this.UnknownData2Pointer = reader.ReadUInt64();
            this.UnknownData1Count = reader.ReadByte();
            this.UnknownData2Count = reader.ReadByte();
            this.GroupsCount = reader.ReadByte();
            this.Unknown_11Bh = reader.ReadByte();
            this.Unknown_11Ch = reader.ReadByte();
            this.ChildrenCount = reader.ReadByte();
            this.ChildrenCount2 = reader.ReadByte();
            this.Unknown_11Fh = reader.ReadByte();
            this.Unknown_120h = reader.ReadUInt64();
            this.Unknown_128h = reader.ReadUInt64();

            // read reference data
            this.ArticulatedBodyType = reader.ReadBlockAt<FragPhysArticulatedBodyType>(
                this.ArticulatedBodyTypePointer // offset
            );
            this.ChildrenUnkUints = reader.ReadUintsAt(this.ChildrenUnkUintsPointer, this.ChildrenCount);

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
            this.ChildrenUnkVecs = reader.ReadStructsAt<Vector4>(this.ChildrenUnkVecsPointer, this.ChildrenCount);
            this.FragTransforms = reader.ReadBlockAt<FragPhysUnknown_F_002>(
                this.FragTransformsPointer // offset
            );
            this.UnknownData1 = reader.ReadBytesAt(this.UnknownData1Pointer, this.UnknownData1Count);
            this.UnknownData2 = reader.ReadBytesAt(this.UnknownData2Pointer, this.UnknownData2Count);



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


            //if (Unknown_04h != 1)
            //{ }//no hit
            //if (Unknown_08h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_11Fh != 0)
            //{ }//no hit
            //if (Unknown_120h != 0)
            //{ }//no hit
            //if (Unknown_128h != 0)
            //{ }//no hit
            //if (ChildrenCount2 != ChildrenCount)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.ArticulatedBodyTypePointer = (ulong)(this.ArticulatedBodyType != null ? this.ArticulatedBodyType.FilePosition : 0);
            this.ChildrenUnkUintsPointer = (ulong)(this.ChildrenUnkUintsBlock != null ? this.ChildrenUnkUintsBlock.FilePosition : 0);
            this.GroupNamesPointer = (ulong)(this.GroupNames != null ? this.GroupNames.FilePosition : 0);
            this.GroupsPointer = (ulong)(this.Groups != null ? this.Groups.FilePosition : 0);
            this.ChildrenPointer = (ulong)(this.Children != null ? this.Children.FilePosition : 0);
            this.Archetype1Pointer = (ulong)(this.Archetype1 != null ? this.Archetype1.FilePosition : 0);
            this.Archetype2Pointer = (ulong)(this.Archetype2 != null ? this.Archetype2.FilePosition : 0);
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);
            this.InertiaTensorsPointer = (ulong)(this.InertiaTensorsBlock != null ? this.InertiaTensorsBlock.FilePosition : 0);
            this.ChildrenUnkVecsPointer = (ulong)(this.ChildrenUnkVecsBlock != null ? this.ChildrenUnkVecsBlock.FilePosition : 0);
            this.FragTransformsPointer = (ulong)(this.FragTransforms != null ? this.FragTransforms.FilePosition : 0);
            this.UnknownData1Pointer = (ulong)(this.UnknownData1Block != null ? this.UnknownData1Block.FilePosition : 0);
            this.UnknownData2Pointer = (ulong)(this.UnknownData2Block != null ? this.UnknownData2Block.FilePosition : 0);

            this.UnknownData1Count = (byte)(this.UnknownData1Block != null ? this.UnknownData1Block.ItemCount : 0);
            this.UnknownData2Count = (byte)(this.UnknownData2Block != null ? this.UnknownData2Block.ItemCount : 0);
            this.GroupsCount = (byte)(this.Groups != null ? this.Groups.Count : 0);
            this.ChildrenCount = (byte)(this.Children != null ? this.Children.Count : 0);
            this.ChildrenCount2 = this.ChildrenCount;


            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.ArticulatedBodyTypePointer);
            writer.Write(this.ChildrenUnkUintsPointer);
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
            writer.Write(this.ChildrenUnkVecsPointer);
            writer.Write(this.FragTransformsPointer);
            writer.Write(this.UnknownData1Pointer);
            writer.Write(this.UnknownData2Pointer);
            writer.Write(this.UnknownData1Count);
            writer.Write(this.UnknownData2Count);
            writer.Write(this.GroupsCount);
            writer.Write(this.Unknown_11Bh);
            writer.Write(this.Unknown_11Ch);
            writer.Write(this.ChildrenCount);
            writer.Write(this.ChildrenCount2);
            writer.Write(this.Unknown_11Fh);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_128h);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (ArticulatedBodyType != null) list.Add(ArticulatedBodyType);
            if (ChildrenUnkUints != null)
            {
                ChildrenUnkUintsBlock = new ResourceSystemStructBlock<uint>(ChildrenUnkUints);
                list.Add(ChildrenUnkUintsBlock);
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
            if (ChildrenUnkVecs != null)
            {
                ChildrenUnkVecsBlock = new ResourceSystemStructBlock<Vector4>(ChildrenUnkVecs);
                list.Add(ChildrenUnkVecsBlock);
            }
            if (FragTransforms != null) list.Add(FragTransforms);
            if (UnknownData1 != null)
            {
                UnknownData1Block = new ResourceSystemStructBlock<byte>(UnknownData1);
                list.Add(UnknownData1Block);
            }
            if (UnknownData2 != null)
            {
                UnknownData2Block = new ResourceSystemStructBlock<byte>(UnknownData2);
                list.Add(UnknownData2Block);
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
        public void WriteXml(StringBuilder sb, int indent)
        {
        }
        public void ReadXml(XmlNode node)
        {
        }

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
        public virtual void WriteXml(StringBuilder sb, int indent)
        {
        }
        public virtual void ReadXml(XmlNode node)
        {
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
        public override void WriteXml(StringBuilder sb, int indent)
        {
        }
        public override void ReadXml(XmlNode node)
        {
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
        public override void WriteXml(StringBuilder sb, int indent)
        {
        }
        public override void ReadXml(XmlNode node)
        {
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
        public void WriteXml(StringBuilder sb, int indent)
        {
        }
        public void ReadXml(XmlNode node)
        {
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
        public void WriteXml(StringBuilder sb, int indent)
        {
        }
        public void ReadXml(XmlNode node)
        {
        }

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
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
        }

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
        public void WriteXml(StringBuilder sb, int indent)
        {
        }
        public void ReadXml(XmlNode node)
        {
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct FragPhysTypeGroup_s : IMetaXmlItem
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


        public void WriteXml(StringBuilder sb, int indent)
        {
        }
        public void ReadXml(XmlNode node)
        {
        }

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