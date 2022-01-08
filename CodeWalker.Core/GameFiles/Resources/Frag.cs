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
        public float GravityFactor { get; set; }
        public float BuoyancyFactor { get; set; }
        public byte Unknown_D8h; // 0x00
        public byte GlassWindowsCount { get; set; }
        public ushort Unknown_DAh; // 0x0000
        public uint Unknown_DCh; // 0x00000000
        public ulong GlassWindowsPointer { get; set; }
        public ulong Unknown_E8h; // 0x0000000000000000
        public ulong PhysicsLODGroupPointer { get; set; }
        public ulong DrawableClothPointer { get; set; }
        public ulong Unknown_100h; // 0x0000000000000000
        public ulong Unknown_108h; // 0x0000000000000000
        public ResourceSimpleList64<LightAttributes> LightAttributes { get; set; }
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
        public FragDrawable DrawableCloth { get; set; }
        public FragVehicleGlassWindows VehicleGlassWindows { get; set; }


        private string_r NameBlock = null; //only used for saving

        public YftFile Yft { get; set; }

#if DEBUG
        public ResourceAnalyzer Analyzer { get; set; }
#endif

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
            this.GravityFactor = reader.ReadSingle();
            this.BuoyancyFactor = reader.ReadSingle();
            this.Unknown_D8h = reader.ReadByte();
            this.GlassWindowsCount = reader.ReadByte();
            this.Unknown_DAh = reader.ReadUInt16();
            this.Unknown_DCh = reader.ReadUInt32();
            this.GlassWindowsPointer = reader.ReadUInt64();
            this.Unknown_E8h = reader.ReadUInt64();
            this.PhysicsLODGroupPointer = reader.ReadUInt64();
            this.DrawableClothPointer = reader.ReadUInt64();
            this.Unknown_100h = reader.ReadUInt64();
            this.Unknown_108h = reader.ReadUInt64();
            this.LightAttributes = reader.ReadBlock<ResourceSimpleList64<LightAttributes>>();
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
            DrawableCloth = reader.ReadBlockAt<FragDrawable>(DrawableClothPointer);
            VehicleGlassWindows = reader.ReadBlockAt<FragVehicleGlassWindows>(VehicleGlassWindowsPointer);

            if (Drawable != null)
            {
                Drawable.OwnerFragment = this;
            }
            if (DrawableCloth != null)
            {
                DrawableCloth.OwnerFragment = this;
                if (DrawableCloth.OwnerCloth == null)
                { }//no hit!
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


            AssignChildrenShaders();
            AssignGlassWindowsGroups();

#if DEBUG
            Analyzer = new ResourceAnalyzer(reader);
#endif

            ////just testing!!
            //if (BoundingSphereRadius <= 0.0f)
            //{ }//no hit
            //if (DrawableArrayFlag != ((DrawableArrayCount == 0) ? -1 : 0))
            //{ }//no hit
            //switch (Unknown_B0h)
            //{
            //    case 0:
            //    case 944:
            //    case 1088:
            //    case 1200:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_B8h)
            //{
            //    case -364:
            //    case -80:
            //    case -48:
            //    case -32:
            //    case -16:
            //    case 0:
            //    case 16:
            //    case 32:
            //    case 48:
            //    case 64:
            //    case 96:
            //    case 160:
            //    case 192:
            //    case 208:
            //    case 480:
            //    case 736:
            //    case 3356:
            //    case 5008:
            //    case 5024:
            //    case 5056:
            //    case 5520:
            //    case 5680:
            //    case 5856:
            //    case 6176:
            //    case 6432:
            //    case 6448:
            //    case 7088:
            //    case 7216:
            //    case 7776:
            //    case 8032:
            //    case 8048:
            //    case 8064:
            //    case 8080:
            //    case 8112:
            //    case 8144:
            //    case 8688:
            //    case 8928:
            //    case 9216:
            //    case 9808:
            //    case 9872:
            //    case 9888:
            //    case 10480:
            //    case 10704:
            //    case 11248:
            //    case 13696:
            //    case 14344:
            //    case 14640:
            //    case 14720:
            //    case 14752:
            //    case 14768:
            //    case 14944:
            //    case 15112:
            //    case 16656:
            //    case 16976:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_BCh)
            //{
            //    case 0:
            //    case -1:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_C0h)
            //{
            //    case 0:
            //    case 256:
            //    case 512:
            //    case 768:
            //    case 1024:
            //    case 65280:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_C4h)
            //{
            //    case 1:
            //    case 3:
            //    case 65:
            //    case 67:
            //        break;
            //    default:
            //        break;//no hit
            //}
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.DrawablePointer = (ulong)(this.Drawable != null ? this.Drawable.FilePosition : 0);
            this.DrawableArrayPointer = (ulong)(this.DrawableArray != null ? this.DrawableArray.FilePosition : 0);
            this.DrawableArrayNamesPointer = (ulong)(this.DrawableArrayNames != null ? this.DrawableArrayNames.FilePosition : 0);
            this.DrawableArrayCount = (uint)(this.DrawableArray != null ? this.DrawableArray.Count : 0);
            this.DrawableArrayFlag = ((this.DrawableArray?.data_items?.Length ?? 0) > 0) ? 0 : -1;
            this.NamePointer = (ulong)(this.NameBlock != null ? this.NameBlock.FilePosition : 0);
            this.BoneTransformsPointer = (ulong)(this.BoneTransforms != null ? this.BoneTransforms.FilePosition : 0);
            this.GlassWindowsCount = (byte)(this.GlassWindows != null ? this.GlassWindows.Count : 0);
            this.GlassWindowsPointer = (ulong)(this.GlassWindows != null ? this.GlassWindows.FilePosition : 0);
            this.PhysicsLODGroupPointer = (ulong)(this.PhysicsLODGroup != null ? this.PhysicsLODGroup.FilePosition : 0);
            this.DrawableClothPointer = (ulong)(this.DrawableCloth != null ? this.DrawableCloth.FilePosition : 0);
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
            writer.Write(this.GravityFactor);
            writer.Write(this.BuoyancyFactor);
            writer.Write(this.Unknown_D8h);
            writer.Write(this.GlassWindowsCount);
            writer.Write(this.Unknown_DAh);
            writer.Write(this.Unknown_DCh);
            writer.Write(this.GlassWindowsPointer);
            writer.Write(this.Unknown_E8h);
            writer.Write(this.PhysicsLODGroupPointer);
            writer.Write(this.DrawableClothPointer);
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
            YftXml.ValueTag(sb, indent, "GravityFactor", FloatUtil.ToString(GravityFactor));
            YftXml.ValueTag(sb, indent, "BuoyancyFactor", FloatUtil.ToString(BuoyancyFactor));
            if ((Drawable != null) && (Drawable.OwnerCloth == null))
            {
                FragDrawable.WriteXmlNode(Drawable, sb, indent, ddsfolder, "Drawable");
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
                    {
                        d.Name = name.Value;
                    }
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
            if ((Cloths?.data_items?.Length ?? 0) > 0)
            {
                YftXml.OpenTag(sb, indent, "Cloths");
                var cind = indent + 1;
                var cind2 = indent + 2;
                for (int i = 0; i < Cloths.data_items.Length; i++)
                {
                    if (Cloths.data_items[i] != null)
                    {
                        YftXml.OpenTag(sb, cind, "Item");
                        Cloths.data_items[i].WriteXml(sb, cind2, ddsfolder);
                        YftXml.CloseTag(sb, cind, "Item");
                    }
                    else
                    {
                        YftXml.SelfClosingTag(sb, cind, "Item");
                    }
                }
                YftXml.CloseTag(sb, indent, "Cloths");
            }
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            BoundingSphereCenter = Xml.GetChildVector3Attributes(node, "BoundingSphereCenter");
            BoundingSphereRadius = Xml.GetChildFloatAttribute(node, "BoundingSphereRadius", "value");
            Unknown_B0h = Xml.GetChildIntAttribute(node, "UnknownB0", "value");
            Unknown_B8h = Xml.GetChildIntAttribute(node, "UnknownB8", "value");
            Unknown_BCh = Xml.GetChildIntAttribute(node, "UnknownBC", "value");
            Unknown_C0h = Xml.GetChildIntAttribute(node, "UnknownC0", "value");
            Unknown_C4h = Xml.GetChildIntAttribute(node, "UnknownC4", "value");
            Unknown_CCh = Xml.GetChildFloatAttribute(node, "UnknownCC", "value");
            GravityFactor = Xml.GetChildFloatAttribute(node, "GravityFactor", "value");
            BuoyancyFactor = Xml.GetChildFloatAttribute(node, "BuoyancyFactor", "value");
            var dnode = node.SelectSingleNode("Drawable");
            if (dnode != null)
            {
                Drawable = FragDrawable.ReadXmlNode(dnode, ddsfolder);
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
            if ((gwinds?.Length ?? 0) > 0)
            {
                GlassWindows = new ResourcePointerArray64<FragGlassWindow>();
                GlassWindows.data_items = gwinds;
            }
            LightAttributes = new ResourceSimpleList64<LightAttributes>();
            LightAttributes.data_items = XmlMeta.ReadItemArray<LightAttributes>(node, "Lights");
            Cloths = new ResourcePointerList64<EnvironmentCloth>();
            var cnode = node.SelectSingleNode("Cloths");
            if (cnode != null)
            {
                var inodes = cnode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    var vlist = new List<EnvironmentCloth>();
                    foreach (XmlNode inode in inodes)
                    {
                        var v = new EnvironmentCloth();
                        v.ReadXml(inode, ddsfolder);
                        vlist.Add(v);

                        if (Drawable == null)
                        {
                            Drawable = v.Drawable;
                        }
                        else
                        {
                            DrawableCloth = v.Drawable;
                        }
                    }
                    Cloths.data_items = vlist.ToArray();
                }
            }

            AssignChildrenSkeletonsAndBounds();
            AssignChildrenShaders();

            FileVFT = 1079456040;
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



        public void AssignChildrenShaders()
        {
            if (PhysicsLODGroup == null) return;

            //for things like vehicle wheels, the shaderGroup in the model is missing, so use the main drawable's shaders.
            var pdrwbl = Drawable ?? DrawableCloth;
            var pskel = pdrwbl?.Skeleton;

            void assigndr(FragDrawable dr, BoundComposite pbcmp, int i)
            {
                if (dr == null) return;
                if (pdrwbl == null) return;
                dr.OwnerDrawable = pdrwbl;//this is also the signal for XML export to skip the skeleton and bounds
                dr.AssignGeometryShaders(pdrwbl.ShaderGroup);

                //// just testing
                //if (dr.Skeleton != pskel)
                //{ }//no hit
                //var pbch = pbcmp?.Children?.data_items;
                //if (pbch != null)
                //{
                //    if (i >= pbch.Length)
                //    { }//no hit
                //    else
                //    {
                //        if (pbch[i] != dr.Bound)
                //        { }//no hit
                //    }
                //}
                //else
                //{ }//no hit
            };
            void assign(FragPhysicsLOD lod)
            {
                var children = lod?.Children?.data_items;
                var pbcmp1 = (lod?.Archetype1?.Bound ?? pdrwbl?.Bound) as BoundComposite;
                var pbcmp2 = (lod?.Archetype2?.Bound ?? pdrwbl?.Bound) as BoundComposite;

                //if (lod?.Archetype1?.Bound != lod?.Bound)
                //{ }//no hit!
                //if (lod?.Archetype2?.Bound != lod?.Bound)
                //{ }//hit

                if (children == null) return;
                for (int i = 0; i < children.Length; i++)
                {
                    var child = children[i];
                    assigndr(child?.Drawable1, pbcmp1, i);
                    assigndr(child?.Drawable2, pbcmp2, i);
                }
            };

            assign(PhysicsLODGroup.PhysicsLOD1);
            assign(PhysicsLODGroup.PhysicsLOD2);
            assign(PhysicsLODGroup.PhysicsLOD3);



            if (DrawableArray?.data_items != null)
            {
                foreach (var arrd in DrawableArray.data_items)
                {
                    assigndr(arrd, null, 0);
                }
            }


        }

        public void AssignChildrenSkeletonsAndBounds()
        {
            if (PhysicsLODGroup == null) return;

            var pdrwbl = Drawable ?? DrawableCloth;
            var pskel = pdrwbl?.Skeleton;

            void assignskb(FragDrawable dr, BoundComposite pbcmp, int i)
            {
                if (dr == null) return;
                if (pdrwbl == null) return;
                var pbch = pbcmp?.Children?.data_items;
                dr.Skeleton = pskel;
                dr.Bound = ((pbch != null) && (i < pbch.Length)) ? pbch[i] : null;
            };
            void assign(FragPhysicsLOD lod)
            {
                if (lod == null) return;
                lod.Bound = lod.Archetype1?.Bound;
                var children = lod.Children?.data_items;
                var pbcmp1 = (lod.Archetype1?.Bound ?? pdrwbl?.Bound) as BoundComposite;
                var pbcmp2 = (lod.Archetype2?.Bound ?? pdrwbl?.Bound) as BoundComposite;
                if (children == null) return;
                for (int i = 0; i < children.Length; i++)
                {
                    var child = children[i];
                    assignskb(child?.Drawable1, pbcmp1, i);
                    assignskb(child?.Drawable2, pbcmp2, i);
                }
            };

            assign(PhysicsLODGroup.PhysicsLOD1);
            assign(PhysicsLODGroup.PhysicsLOD2);
            assign(PhysicsLODGroup.PhysicsLOD3);

        }

        public void AssignGlassWindowsGroups()
        {

            void assign(FragPhysicsLOD lod)
            {
                if (lod?.Groups?.data_items == null) return;
                foreach (var grp in lod.Groups.data_items)
                {
                    var windx = grp.GlassWindowIndex;
                    var flags = grp.GlassFlags;
                    if ((flags & 2) > 0)
                    {
                        if (GlassWindows?.data_items != null)
                        {
                            if (windx < GlassWindows.data_items.Length)
                            {
                                var wind = GlassWindows.data_items[windx];
                                wind.Group = grp;
                                wind.GroupLOD = lod;
                            }
                        }
                    }
                }
            }
            assign(PhysicsLODGroup?.PhysicsLOD1);
            assign(PhysicsLODGroup?.PhysicsLOD2);
            assign(PhysicsLODGroup?.PhysicsLOD3);

            //if (VehicleGlassWindows?.Windows != null)
            //{
            //    var groups = PhysicsLODGroup?.PhysicsLOD1?.Groups?.data_items;
            //    if (groups != null)
            //    {
            //        var groupdict = new Dictionary<int, FragPhysTypeGroup>();
            //        foreach (var grp in groups)
            //        {
            //            groupdict[grp.Index] = grp;
            //        }
            //        foreach (var wind in VehicleGlassWindows.Windows)
            //        {
            //            groupdict.TryGetValue(wind.ItemID, out var grp);
            //            wind.Group = grp;
            //            wind.GroupLOD = PhysicsLODGroup.PhysicsLOD1;
            //        }
            //    }
            //}

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
            if (DrawableCloth != null) list.Add(DrawableCloth);
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
        public Matrix4F_s FragMatrix { get; set; }
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
        public Matrix4F_s[] FragMatrices { get; set; }
        public string Name { get; set; }

        public FragType OwnerFragment { get; set; } //for handy use
        public EnvironmentCloth OwnerCloth { get; set; }
        public FragPhysTypeChild OwnerFragmentPhys { get; set; }
        public FragDrawable OwnerDrawable { get; set; } //if inheriting shaders, skeletons and bounds

        private ResourceSystemStructBlock<ulong> FragMatricesIndsBlock = null; //used for saving only
        private ResourceSystemStructBlock<Matrix4F_s> FragMatricesBlock = null;
        private string_r NameBlock = null;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_0A8h = reader.ReadUInt64();
            this.FragMatrix = reader.ReadStruct<Matrix4F_s>();
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
            FragMatrices = reader.ReadStructsAt<Matrix4F_s>(FragMatricesPointer, FragMatricesCapacity);
            Name = reader.ReadStringAt(NamePointer);

            if (Bound != null)
            {
                Bound.Owner = this;
            }

            //if (Bound is BoundComposite bcmp)
            //{//no hit
            //    if ((bcmp.ChildrenFlags1 != null) || (bcmp.ChildrenFlags2 != null))
            //    { }//no hit
            //}



            if (FragMatricesInds != null)
            { }
            if (FragMatrices != null)
            { }
            //if ((FragMatrices != null) != (FragMatricesInds != null))
            //{ }//no hit
            //if ((FragMatricesCount != 0) != (FragMatricesCapacity != 0))
            //{ }//no hit
            //if (FragMatricesCount > FragMatricesCapacity)
            //{ }//no hit
            //if (FragMatricesCapacity != FragMatricesIndsCount)
            //{ }//no hit
            //if (Unknown_0A8h != 0)
            //{ }//no hit
            //if (Unknown_104h != 0)
            //{ }//no hit
            //if (Unknown_112h != 1)
            //{ }//no hit
            //if (Unknown_114h != 0)
            //{ }//no hit
            //if (Unknown_118h != 0)
            //{ }//no hit
            //if (Unknown_120h != 0)
            //{ }//no hit
            //if (Unknown_128h != 0)
            //{ }//no hit
            //if (Unknown_138h != 0)
            //{ }//no hit
            //if (Unknown_140h != 0)
            //{ }//no hit
            //if (Unknown_148h != 0)
            //{ }//no hit

            //if (FragMatrix.Flags1 != 0x7f800001)
            //{ }//no hit
            //if (FragMatrix.Flags2 != 0x7f800001)
            //{ }//no hit
            //if (FragMatrix.Flags3 != 0x7f800001)
            //{ }//no hit
            //if (FragMatrix.Flags4 != 0x7f800001)
            //{ }//no hit
            //if (FragMatrices != null)
            //{
            //    foreach (var fm in FragMatrices)
            //    {
            //        if (fm.Flags1 != 0x7f800001)
            //        { }//no hit
            //        if (fm.Flags2 != 0x7f800001)
            //        { }//no hit
            //        if (fm.Flags3 != 0x7f800001)
            //        { }//no hit
            //        if (fm.Flags4 != 0x7f800001)
            //        { }//no hit
            //    }
            //}

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
            writer.WriteStruct(this.FragMatrix);
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
            YftXml.WriteRawArray(sb, FragMatrix.ToArray(), indent, "Matrix", "", FloatUtil.ToString, 3);
            if ((FragMatrices != null) && (FragMatrices.Length > 0))
            {
                YftXml.OpenTag(sb, indent, "Matrices capacity=\"" + FragMatrices.Length.ToString() + "\"");
                var cind = indent + 1;
                var cnt = Math.Min(FragMatrices.Length, FragMatricesCount);
                for (int i = 0; i < cnt; i++)
                {
                    var idx = ((FragMatricesInds != null) && (i < FragMatricesInds.Length)) ? FragMatricesInds[i] : 0;
                    YftXml.OpenTag(sb, cind, "Item id=\"" + idx.ToString() + "\"");
                    YftXml.WriteRawArrayContent(sb, FragMatrices[i].ToArray(), cind + 1, FloatUtil.ToString, 3);
                    YftXml.CloseTag(sb, cind, "Item");
                }
                YftXml.CloseTag(sb, indent, "Matrices");
            }


            var skel = Skeleton;
            var bnds = Bound;
            if (OwnerDrawable != null) //don't export skeleton or bounds if this is a frag child!
            {
                Skeleton = null;
                Bound = null;
            }

            base.WriteXml(sb, indent, ddsfolder);

            if (Bound != null)
            {
                Bounds.WriteXmlNode(Bound, sb, indent);
            }

            Skeleton = skel;
            Bound = bnds;
        }
        public override void ReadXml(XmlNode node, string ddsfolder)
        {
            Name = Xml.GetChildInnerText(node, "Name"); if (string.IsNullOrEmpty(Name)) Name = null;
            FragMatrix = new Matrix4F_s(Xml.GetChildRawFloatArray(node, "Matrix"));

            var msnode = node.SelectSingleNode("Matrices");
            if (msnode != null)
            {
                var mats = new List<Matrix4F_s>();
                var matinds = new List<ulong>();
                var cap = Xml.GetIntAttribute(msnode, "capacity");
                var inodes = msnode.SelectNodes("Item");
                foreach (XmlNode inode in inodes)
                {
                    var id = Xml.GetULongAttribute(inode, "id");
                    var mat = new Matrix4F_s(Xml.GetRawFloatArray(inode));
                    matinds.Add(id);
                    mats.Add(mat);
                }
                FragMatricesCount = (ushort)mats.Count;
                for (int i = mats.Count; i < cap; i++)
                {
                    matinds.Add(0);
                    mats.Add(new Matrix4F_s(float.NaN));
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

            FileVFT = 1080060872;
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
                FragMatricesBlock = new ResourceSystemStructBlock<Matrix4F_s>(FragMatrices);
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
            //{ }//no hit
            //if ((Unknown_12h != 0) && (Unknown_12h != 1))
            //{ }//no hit
            //if (Unknown_00h != 0)
            //{ }//no hit
            //if (Unknown_08h != 0)
            //{ }//no hit
            //if (Unknown_14h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit

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
        public Vector3 ProjectionRow1 { get; set; }
        public uint UnkUint1 = 0x7f800001; // 0x7f800001
        public Vector3 ProjectionRow2 { get; set; }
        public uint UnkUint2 = 0x7f800001; // 0x7f800001
        public Vector3 ProjectionRow3 { get; set; }
        public uint UnkUint3 = 0x7f800001; // 0x7f800001
        public float UnkFloat13 { get; set; } //offset? Vector2
        public float UnkFloat14 { get; set; } //offset?
        public float UnkFloat15 { get; set; } //scale? sum of this and above often gives integers eg 1, 6
        public float UnkFloat16 { get; set; } //(as above, Vector2)
        public VertexDeclaration VertexDeclaration { get; set; } = new VertexDeclaration(); //this all equates to VertexTypePNCTT
        public float Thickness { get; set; } //probably
        public ushort UnkUshort1 = 2; //2
        public ushort Flags { get; set; }//512, 768, 1280 etc ... flags
        public float UnkFloat18 { get; set; } //another scale in UV space..?
        public float UnkFloat19 { get; set; } //(as above, Vector2)
        public Vector3 Tangent { get; set; }
        public uint UnkUint4 = 0x7f800001; // 0x7f800001

        public byte FlagsLo { get { return (byte)((Flags >> 0) & 0xFF); } }
        public byte FlagsHi { get { return (byte)((Flags >> 8) & 0xFF); } }
        public FragPhysTypeGroup Group { get; set; }
        public FragPhysicsLOD GroupLOD { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.ProjectionRow1 = reader.ReadVector3();
            this.UnkUint1 = reader.ReadUInt32();
            this.ProjectionRow2 = reader.ReadVector3();
            this.UnkUint2 = reader.ReadUInt32();
            this.ProjectionRow3 = reader.ReadVector3();
            this.UnkUint3 = reader.ReadUInt32();
            this.UnkFloat13 = reader.ReadSingle();
            this.UnkFloat14 = reader.ReadSingle();
            this.UnkFloat15 = reader.ReadSingle();
            this.UnkFloat16 = reader.ReadSingle();
            this.VertexDeclaration.Read(reader);
            this.Thickness = reader.ReadSingle();
            this.UnkUshort1 = reader.ReadUInt16();
            this.Flags = reader.ReadUInt16();
            this.UnkFloat18 = reader.ReadSingle();
            this.UnkFloat19 = reader.ReadSingle();
            this.Tangent = reader.ReadVector3();
            this.UnkUint4 = reader.ReadUInt32();

            //if (UnkUint1 != 0x7f800001)
            //{ }//no hit
            //if (UnkUint2 != 0x7f800001)
            //{ }//no hit
            //if (UnkUint3 != 0x7f800001)
            //{ }//no hit
            //if (UnkUint4 != 0x7f800001)
            //{ }//no hit
            //if (UnkUshort1 != 2)
            //{ }//no hit
            //if (UnkFloat17 > 1.0f)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.ProjectionRow1);
            writer.Write(this.UnkUint1);
            writer.Write(this.ProjectionRow2);
            writer.Write(this.UnkUint2);
            writer.Write(this.ProjectionRow3);
            writer.Write(this.UnkUint3);
            writer.Write(this.UnkFloat13);
            writer.Write(this.UnkFloat14);
            writer.Write(this.UnkFloat15);
            writer.Write(this.UnkFloat16);
            this.VertexDeclaration.Write(writer);
            writer.Write(this.Thickness);
            writer.Write(this.UnkUshort1);
            writer.Write(this.Flags);
            writer.Write(this.UnkFloat18);
            writer.Write(this.UnkFloat19);
            writer.Write(this.Tangent);
            writer.Write(this.UnkUint4);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YftXml.ValueTag(sb, indent, "Flags", Flags.ToString());
            YftXml.OpenTag(sb, indent, "Projection");
            YftXml.WriteRawArrayContent(sb, new Matrix3x3() { Row1 = ProjectionRow1, Row2 = ProjectionRow2, Row3 = ProjectionRow3 }.ToArray(), indent + 1, FloatUtil.ToString, 3);
            YftXml.CloseTag(sb, indent, "Projection");
            YftXml.ValueTag(sb, indent, "UnkFloat13", FloatUtil.ToString(UnkFloat13));
            YftXml.ValueTag(sb, indent, "UnkFloat14", FloatUtil.ToString(UnkFloat14));
            YftXml.ValueTag(sb, indent, "UnkFloat15", FloatUtil.ToString(UnkFloat15));
            YftXml.ValueTag(sb, indent, "UnkFloat16", FloatUtil.ToString(UnkFloat16));
            YftXml.ValueTag(sb, indent, "Thickness", FloatUtil.ToString(Thickness));
            YftXml.ValueTag(sb, indent, "UnkFloat18", FloatUtil.ToString(UnkFloat18));
            YftXml.ValueTag(sb, indent, "UnkFloat19", FloatUtil.ToString(UnkFloat19));
            YftXml.SelfClosingTag(sb, indent, "Tangent " + FloatUtil.GetVector3XmlString(Tangent));
            VertexDeclaration.WriteXml(sb, indent, "Layout");
        }
        public void ReadXml(XmlNode node)
        {
            Flags = (ushort)Xml.GetChildUIntAttribute(node, "Flags", "value");
            var proj = Xml.GetChildRawFloatArray(node, "Projection");
            if ((proj?.Length ?? 0) == 9)
            {
                ProjectionRow1 = new Vector3(proj[0], proj[1], proj[2]);
                ProjectionRow2 = new Vector3(proj[3], proj[4], proj[5]);
                ProjectionRow3 = new Vector3(proj[6], proj[7], proj[8]);
            }
            UnkFloat13 = Xml.GetChildFloatAttribute(node, "UnkFloat13", "value");
            UnkFloat14 = Xml.GetChildFloatAttribute(node, "UnkFloat14", "value");
            UnkFloat15 = Xml.GetChildFloatAttribute(node, "UnkFloat15", "value");
            UnkFloat16 = Xml.GetChildFloatAttribute(node, "UnkFloat16", "value");
            Thickness = Xml.GetChildFloatAttribute(node, "Thickness", "value");
            UnkFloat18 = Xml.GetChildFloatAttribute(node, "UnkFloat18", "value");
            UnkFloat19 = Xml.GetChildFloatAttribute(node, "UnkFloat19", "value");
            Tangent = Xml.GetChildVector3Attributes(node, "Tangent");
            VertexDeclaration.ReadXml(node.SelectSingleNode("Layout"));
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragVehicleGlassWindows : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return TotalLength; }
        }

        [TypeConverter(typeof(ExpandableObjectConverter))] public struct WindowOffset
        {
            public uint ItemID { get; set; }
            public uint Offset { get; set; }
            public override string ToString()
            {
                return ItemID.ToString() + ": " + Offset.ToString();
            }
        }
        [TypeConverter(typeof(ExpandableObjectConverter))] public class Window
        {
            public Matrix Projection { get; set; } //NOTE: M44 is not actually part of this matrix, what actually is it? the value needs to be 1.0 for projection to be correct, but file contains other values. maybe some scaling factor?
            public uint UnkUint1 { get; set; } = 0x56475743; // "VGWC"    vehicle glass window C..?
            public ushort ItemID { get; set; }
            public ushort UnkUshort1 { get; set; }
            public ushort ShatterMapWidth { get; set; } //max value of all End1 and End2 in ItemDatas, plus 1
            public ushort ItemDataCount { get; set; }//count of item data arrays
            public ushort ItemDataByteLength { get; set; }//total byte length of ItemDatas plus byte length of ItemDataOffsets
            public ushort UnkUshort3; // 0
            public uint UnkUint2; // 0
            public uint UnkUint3; // 0
            public float UnkFloat17 { get; set; }
            public float UnkFloat18 { get; set; }
            public ushort UnkUshort4 { get; set; } //0, 1
            public ushort UnkUshort5 { get; set; } //2, 2050
            public float CracksTextureTiling { get; set; } // UV multiplier for the "shattered" cracks texture that is applied when the window is broken
            public uint UnkUint4; // 0
            public uint UnkUint5; // 0
            public ushort[] ShatterMapRowOffsets { get; set; }//byte offsets for shatter map array
            public WindowShatterMapRow[] ShatterMap { get; set; }

            public byte[] Padding { get; set; }//should just be leftover padding, TODO: getrid of this

            public uint ItemDataLength
            {
                get
                {
                    uint bc = (ShatterMapRowOffsets != null) ? ItemDataCount * 2u : 0;
                    if (ShatterMap != null)
                    {
                        foreach (var u in ShatterMap)
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
                Projection = reader.ReadMatrix();
                UnkUint1 = reader.ReadUInt32(); //0x56475743 "VGWC"
                ItemID = reader.ReadUInt16();
                UnkUshort1 = reader.ReadUInt16();
                ShatterMapWidth = reader.ReadUInt16();
                ItemDataCount = reader.ReadUInt16();//count of item data arrays
                ItemDataByteLength = reader.ReadUInt16();//total byte length of ItemDatas plus byte length of ItemDataOffsets
                UnkUshort3 = reader.ReadUInt16();//0
                UnkUint2 = reader.ReadUInt32();//0
                UnkUint3 = reader.ReadUInt32();//0
                UnkFloat17 = reader.ReadSingle();
                UnkFloat18 = reader.ReadSingle();
                UnkUshort4 = reader.ReadUInt16();//0, 1
                UnkUshort5 = reader.ReadUInt16();//2, 2050
                CracksTextureTiling = reader.ReadSingle();
                UnkUint4 = reader.ReadUInt32();//0
                UnkUint5 = reader.ReadUInt32();//0


                if (ItemDataByteLength != 0)//sometimes this is 0 and UnkUshort3>0, which is weird
                {
                    ShatterMapRowOffsets = reader.ReadStructs<ushort>(ItemDataCount);//byte offsets for following array
                    ShatterMap = new WindowShatterMapRow[ItemDataCount];
                    for (int i = 0; i < ItemDataCount; i++)
                    {
                        //var toffset = ItemDataOffsets[i];
                        var u = new WindowShatterMapRow();
                        u.Read(reader);
                        ShatterMap[i] = u;
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
                //if (UnkFloat4 != 0)
                //{ }//no hit
                //if (UnkFloat8 != 0)
                //{ }//no hit
                //if (UnkFloat12 != 0)
                //{ }//no hit

                ////testing!
                //BuildOffsets();
            }
            public void Write(ResourceDataWriter writer)
            {
                writer.Write(Projection);
                writer.Write(UnkUint1);
                writer.Write(ItemID);
                writer.Write(UnkUshort1);
                writer.Write(ShatterMapWidth);
                writer.Write(ItemDataCount);
                writer.Write(ItemDataByteLength);
                writer.Write(UnkUshort3);
                writer.Write(UnkUint2);
                writer.Write(UnkUint3);
                writer.Write(UnkFloat17);
                writer.Write(UnkFloat18);
                writer.Write(UnkUshort4);
                writer.Write(UnkUshort5);
                writer.Write(CracksTextureTiling);
                writer.Write(UnkUint4);
                writer.Write(UnkUint5);
                writer.WriteStructs(ShatterMapRowOffsets);

                if (ShatterMap != null)
                {
                    foreach (var ud in ShatterMap)
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
                YftXml.ValueTag(sb, indent, "UnkUshort4", UnkUshort4.ToString());
                YftXml.ValueTag(sb, indent, "UnkUshort5", UnkUshort5.ToString());
                YftXml.OpenTag(sb, indent, "Projection");
                YftXml.WriteRawArrayContent(sb, Projection.ToArray(), indent + 1, FloatUtil.ToString, 4);
                YftXml.CloseTag(sb, indent, "Projection");
                YftXml.ValueTag(sb, indent, "UnkFloat17", FloatUtil.ToString(UnkFloat17));
                YftXml.ValueTag(sb, indent, "UnkFloat18", FloatUtil.ToString(UnkFloat18));
                YftXml.ValueTag(sb, indent, "CracksTextureTiling", FloatUtil.ToString(CracksTextureTiling));
                if (ShatterMap != null)
                {
                    YftXml.OpenTag(sb, indent, "ShatterMap");
                    var cind = indent + 1;
                    foreach (var item in ShatterMap)
                    {
                        YftXml.Indent(sb, cind);
                        item.WriteLine(sb, ShatterMapWidth);
                    }
                    YftXml.CloseTag(sb, indent, "ShatterMap");
                }
            }
            public void ReadXml(XmlNode node)
            {
                ItemID = (ushort)Xml.GetChildUIntAttribute(node, "ItemID", "value");
                UnkUshort1 = (ushort)Xml.GetChildUIntAttribute(node, "UnkUshort1", "value");
                UnkUshort4 = (ushort)Xml.GetChildUIntAttribute(node, "UnkUshort4", "value");
                UnkUshort5 = (ushort)Xml.GetChildUIntAttribute(node, "UnkUshort5", "value");
                Projection = Xml.GetChildMatrix(node, "Projection");
                UnkFloat17 = Xml.GetChildFloatAttribute(node, "UnkFloat17", "value");
                UnkFloat18 = Xml.GetChildFloatAttribute(node, "UnkFloat18", "value");
                CracksTextureTiling = Xml.GetChildFloatAttribute(node, "CracksTextureTiling", "value");
                var smnode = node.SelectSingleNode("ShatterMap");
                if (smnode != null)
                {
                    var smlist = new List<WindowShatterMapRow>();
                    var smstr = smnode.InnerText.Trim();
                    var smstrs = smstr.Split('\n');
                    foreach (var smrstr in smstrs)
                    {
                        var rstr = smrstr.Trim();
                        var smr = new WindowShatterMapRow();
                        smr.ReadLine(rstr);
                        smlist.Add(smr);
                    }
                    ShatterMap = smlist.ToArray();
                }
                BuildOffsets();
            }

            public void BuildOffsets()
            {
                var o = 0u;
                var offs = new List<ushort>();
                var maxend = 0;

                if (ShatterMap != null)
                {
                    foreach (var item in ShatterMap)
                    {
                        offs.Add((ushort)o);
                        o += item.TotalLength;
                        var dl = item.DataLength + item.Start1;
                        maxend = Math.Max(Math.Max(maxend, item.End1), item.End2);
                    }
                    o += (uint)(ShatterMap.Length * 2);
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

                ShatterMapRowOffsets = offs.ToArray();

                ////testing
                //if (ItemDataByteLength != o)
                //{ }//no hit!
                ItemDataByteLength = (ushort)o;

                ItemDataCount = (ushort)(ShatterMap?.Length ?? 0);

                //if (UnkUshort2 != maxend + 1)
                //{ }//no hit
                ShatterMapWidth = (ushort)(maxend + 1);

            }

            public override string ToString()
            {
                return ItemID.ToString() + ": " + UnkUshort1.ToString() + ": " + ShatterMapWidth.ToString() + ": " + ItemDataCount.ToString() + ": " + ItemDataByteLength.ToString() + ": " + UnkUshort3.ToString();
            }
        }
        [TypeConverter(typeof(ExpandableObjectConverter))] public class WindowShatterMapRow
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
                Data1 = Xml.GetChildRawByteArray(node, "Data1", 10); if ((Data1?.Length ?? 0) == 0) Data1 = null;
                Data2 = Xml.GetChildRawByteArray(node, "Data2", 10); if ((Data2?.Length ?? 0) == 0) Data2 = null;
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


            public void WriteLine(StringBuilder sb, int width)
            {
                int cpos = 0;
                if (Data1 != null)
                {
                    for (int i = 0; i < Start1; i++)
                    {
                        sb.Append("##");
                    }
                    for (int i = 0; i < Data1.Length; i++)
                    {
                        sb.Append(MetaXml.FormatHexByte(Data1[i]));
                    }
                    cpos = Start1 + Data1.Length;
                }
                if (Data2 != null)
                {
                    for (int i = cpos; i < Start2; i++)
                    {
                        sb.Append("--");
                    }
                    for (int i = 0; i < Data2.Length; i++)
                    {
                        sb.Append(MetaXml.FormatHexByte(Data2[i]));
                    }
                    cpos = Start2 + Data2.Length;
                }
                for (int i = cpos; i < width; i++)
                {
                    sb.Append("##");
                }
                sb.AppendLine();
            }
            public void ReadLine(string s)
            {

                var d1 = new List<byte>();
                var d2 = new List<byte>();
                bool b1 = false;
                bool b2 = false;
                var s1 = 0;
                var s2 = 255;
                var e1 = 0;
                var e2 = 0;
                var l = s.Length / 2;
                for (int i = 0; i < l; i++)
                {
                    var sc = s.Substring(i * 2, 2);
                    if (sc == "##")
                    {
                        if (!b1)
                        {
                            s1 = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (sc == "--")
                    {
                        s2 = i + 1;
                        b2 = true;
                    }
                    else
                    {
                        b1 = true;
                        byte val = Convert.ToByte(sc, 16);
                        if (b2)
                        {
                            d2.Add(val);
                            e2 = i;
                        }
                        else
                        {
                            d1.Add(val);
                            e1 = i;
                            if ((val == 255) && (i == 0))
                            {
                                s2 = i + 1;
                                b2 = true;
                            }
                        }
                    }
                }

                Start1 = (byte)s1;
                Start2 = (byte)s2;
                End1 = (byte)e1;
                End2 = (byte)e2;
                Data1 = (d1.Count > 0) ? d1.ToArray() : null;
                Data2 = (d2.Count > 0) ? d2.ToArray() : null;

            }

            public int GetValue(int x)
            {
                if (x < 0) return -1;
                if (Data1 != null)
                {
                    if (x < Start1) return -1;
                    var cpos = x - Start1;
                    if (cpos < Data1.Length) return Data1[cpos];
                }
                if (Data2 != null)
                {
                    if (x < Start2) return 256;
                    var cpos = x - Start2;
                    if (cpos < Data2.Length) return Data2[cpos];
                }
                return -1;
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
        public WindowOffset[] WindowOffsets { get; set; }
        public uint UnkUint0 { get; set; } = 0;
        public Window[] Windows { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            Unknown_0h = reader.ReadUInt32(); // "VGWH"   ...vehicle glass window H..?
            Unknown_4h = reader.ReadUInt16(); //112 = length of item headers
            ItemCount = reader.ReadUInt16();
            TotalLength = reader.ReadUInt32();
            WindowOffsets = reader.ReadStructs<WindowOffset>(ItemCount + (ItemCount & 1u)); //offsets in here start at just after UnkUint0
            UnkUint0 = reader.ReadUInt32();//0

            long coffset = 16 + WindowOffsets.Length*8;

            Windows = new Window[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                var rpos = reader.Position;
                var u = new Window();
                u.Read(reader);
                Windows[i] = u;
                coffset += reader.Position - rpos;

                var padd = (16 - (coffset % 16)) % 16;
                if (padd > 0)
                {
                    u.Padding = reader.ReadBytes((int)padd);
                    coffset += padd;

                    //foreach (var b in u.Padding)
                    //{
                    //    if (b != 0)
                    //    { }
                    //}
                }
            }

            //if (coffset != TotalLength)
            //{ }
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

            writer.WriteStructs(WindowOffsets);
            writer.Write(UnkUint0);

            long coffset = 16 + WindowOffsets.Length * 8;

            foreach (var item in Windows)
            {
                var rpos = writer.Position;

                item.Write(writer);

                coffset += writer.Position - rpos;
                var padd = (16 - (coffset % 16)) % 16;
                if (padd > 0)
                {
                    writer.Write(new byte[padd]);
                    coffset += padd;
                }
            }

            //if (coffset != TotalLength)
            //{ }

        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            if (Windows != null)
            {
                foreach (var item in Windows)
                {
                    YftXml.OpenTag(sb, indent, "Window");
                    item.WriteXml(sb, indent + 1);
                    YftXml.CloseTag(sb, indent, "Window");
                }
            }
        }
        public void ReadXml(XmlNode node)
        {
            var inodes = node.SelectNodes("Window");
            var ilist = new List<Window>();
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var item = new Window();
                    item.ReadXml(inode);
                    ilist.Add(item);
                }
            }
            Windows = ilist.ToArray();
            ItemCount = (ushort)ilist.Count;

            BuildOffsets();
        }



        public void BuildOffsets()
        {
            var offs = new List<WindowOffset>();
            var bc = 16u;
            if (Windows != null)
            {
                bc += (uint)((Windows.Length + (Windows.Length & 1)) * 8);
                foreach (var item in Windows)
                {
                    var off = new WindowOffset();
                    off.ItemID = item.ItemID;
                    off.Offset = bc;
                    offs.Add(off);
                    bc += item.TotalLength;
                    bc += (16 - (bc % 16)) % 16;//account for padding
                }
                if ((offs.Count & 1) != 0)
                {
                    offs.Add(new WindowOffset());
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
            WindowOffsets = offs.ToArray();

        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysicsLODGroup : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint VFT { get; set; } = 1080055472;
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
            this.PhysicsLOD1 = reader.ReadBlockAt<FragPhysicsLOD>(this.PhysicsLOD1Pointer);
            this.PhysicsLOD2 = reader.ReadBlockAt<FragPhysicsLOD>(this.PhysicsLOD2Pointer);
            this.PhysicsLOD3 = reader.ReadBlockAt<FragPhysicsLOD>(this.PhysicsLOD3Pointer);

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
        public uint VFT { get; set; } = 1080055512;
        public uint Unknown_04h = 1; // 0x00000001
        public ulong Unknown_08h; // 0x0000000000000000
        public uint Unknown_10h; // 0x00000000
        public float Unknown_14h { get; set; }
        public float Unknown_18h { get; set; }
        public float Unknown_1Ch { get; set; }
        public ulong ArticulatedBodyTypePointer { get; set; }
        public ulong ChildrenUnkFloatsPointer { get; set; }
        public Vector3 PositionOffset { get; set; }
        public uint Unknown_3Ch { get; set; } = 0x7fc00001; // 0x7f800001, 0x7fc00001
        public Vector3 Unknown_40h { get; set; }
        public uint Unknown_4Ch { get; set; } = 0x7fc00001; // 0x7f800001, 0x7fc00001
        public Vector3 Unknown_50h { get; set; }
        public uint Unknown_5Ch = 0x7f800001; // 0x7f800001
        public Vector3 DampingLinearC { get; set; }
        public uint Unknown_6Ch = 0x7f800001; // 0x7f800001
        public Vector3 DampingLinearV { get; set; }
        public uint Unknown_7Ch = 0x7f800001; // 0x7f800001
        public Vector3 DampingLinearV2 { get; set; }
        public uint Unknown_8Ch = 0x7f800001; // 0x7f800001
        public Vector3 DampingAngularC { get; set; }
        public uint Unknown_9Ch = 0x7f800001; // 0x7f800001
        public Vector3 DampingAngularV { get; set; }
        public uint Unknown_ACh = 0x7f800001; // 0x7f800001
        public Vector3 DampingAngularV2 { get; set; }
        public uint Unknown_BCh = 0x7f800001; // 0x7f800001
        public ulong GroupNamesPointer { get; set; }
        public ulong GroupsPointer { get; set; }
        public ulong ChildrenPointer { get; set; }
        public ulong Archetype1Pointer { get; set; }
        public ulong Archetype2Pointer { get; set; }
        public ulong BoundPointer { get; set; }
        public ulong ChildrenInertiaTensorsPointer { get; set; }
        public ulong ChildrenUnkVecsPointer { get; set; }
        public ulong FragTransformsPointer { get; set; }
        public ulong UnknownData1Pointer { get; set; }
        public ulong UnknownData2Pointer { get; set; }
        public byte UnknownData1Count { get; set; }
        public byte UnknownData2Count { get; set; }
        public byte GroupsCount { get; set; }
        public byte RootGroupsCount { get; set; }
        public byte Unknown_11Ch = 1; // 0x01
        public byte ChildrenCount { get; set; }
        public byte ChildrenCount2 { get; set; }
        public byte Unknown_11Fh; // 0x00
        public ulong Unknown_120h; // 0x0000000000000000
        public ulong Unknown_128h; // 0x0000000000000000

        // reference data
        public FragPhysArticulatedBodyType ArticulatedBodyType { get; set; }
        public float[] ChildrenUnkFloats { get; set; }
        public FragPhysGroupNamesBlock GroupNames { get; set; }
        public ResourcePointerArray64<FragPhysTypeGroup> Groups { get; set; }
        public ResourcePointerArray64<FragPhysTypeChild> Children { get; set; }
        public FragPhysArchetype Archetype1 { get; set; }
        public FragPhysArchetype Archetype2 { get; set; }
        public Bounds Bound { get; set; }
        public Vector4[] ChildrenInertiaTensors { get; set; }
        public Vector4[] ChildrenUnkVecs { get; set; }
        public FragPhysTransforms FragTransforms { get; set; }
        public byte[] UnknownData1 { get; set; }
        public byte[] UnknownData2 { get; set; }


        private ResourceSystemStructBlock<float> ChildrenUnkFloatsBlock = null; //used only for saving
        private ResourceSystemStructBlock<Vector4> ChildrenInertiaTensorsBlock = null;
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
            this.ChildrenUnkFloatsPointer = reader.ReadUInt64();
            this.PositionOffset = reader.ReadVector3();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.Unknown_40h = reader.ReadVector3();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadVector3();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.DampingLinearC = reader.ReadVector3();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.DampingLinearV = reader.ReadVector3();
            this.Unknown_7Ch = reader.ReadUInt32();
            this.DampingLinearV2 = reader.ReadVector3();
            this.Unknown_8Ch = reader.ReadUInt32();
            this.DampingAngularC = reader.ReadVector3();
            this.Unknown_9Ch = reader.ReadUInt32();
            this.DampingAngularV = reader.ReadVector3();
            this.Unknown_ACh = reader.ReadUInt32();
            this.DampingAngularV2 = reader.ReadVector3();
            this.Unknown_BCh = reader.ReadUInt32();
            this.GroupNamesPointer = reader.ReadUInt64();
            this.GroupsPointer = reader.ReadUInt64();
            this.ChildrenPointer = reader.ReadUInt64();
            this.Archetype1Pointer = reader.ReadUInt64();
            this.Archetype2Pointer = reader.ReadUInt64();
            this.BoundPointer = reader.ReadUInt64();
            this.ChildrenInertiaTensorsPointer = reader.ReadUInt64();
            this.ChildrenUnkVecsPointer = reader.ReadUInt64();
            this.FragTransformsPointer = reader.ReadUInt64();
            this.UnknownData1Pointer = reader.ReadUInt64();
            this.UnknownData2Pointer = reader.ReadUInt64();
            this.UnknownData1Count = reader.ReadByte();
            this.UnknownData2Count = reader.ReadByte();
            this.GroupsCount = reader.ReadByte();
            this.RootGroupsCount = reader.ReadByte();
            this.Unknown_11Ch = reader.ReadByte();
            this.ChildrenCount = reader.ReadByte();
            this.ChildrenCount2 = reader.ReadByte();
            this.Unknown_11Fh = reader.ReadByte();
            this.Unknown_120h = reader.ReadUInt64();
            this.Unknown_128h = reader.ReadUInt64();

            // read reference data
            this.ArticulatedBodyType = reader.ReadBlockAt<FragPhysArticulatedBodyType>(this.ArticulatedBodyTypePointer);
            this.ChildrenUnkFloats = reader.ReadFloatsAt(this.ChildrenUnkFloatsPointer, this.ChildrenCount);
            this.Groups = reader.ReadBlockAt<ResourcePointerArray64<FragPhysTypeGroup>>(this.GroupsPointer, this.GroupsCount);
            this.GroupNames = reader.ReadBlockAt<FragPhysGroupNamesBlock>(this.GroupNamesPointer, this.GroupsCount, this.Groups?.data_items);
            this.Children = reader.ReadBlockAt<ResourcePointerArray64<FragPhysTypeChild>>(this.ChildrenPointer, this.ChildrenCount);
            this.Archetype1 = reader.ReadBlockAt<FragPhysArchetype>(this.Archetype1Pointer);
            this.Archetype2 = reader.ReadBlockAt<FragPhysArchetype>(this.Archetype2Pointer);
            this.Bound = reader.ReadBlockAt<Bounds>(this.BoundPointer);
            this.ChildrenInertiaTensors = reader.ReadStructsAt<Vector4>(this.ChildrenInertiaTensorsPointer, this.ChildrenCount);
            this.ChildrenUnkVecs = reader.ReadStructsAt<Vector4>(this.ChildrenUnkVecsPointer, this.ChildrenCount);
            this.FragTransforms = reader.ReadBlockAt<FragPhysTransforms>(this.FragTransformsPointer);
            this.UnknownData1 = reader.ReadBytesAt(this.UnknownData1Pointer, this.UnknownData1Count);
            this.UnknownData2 = reader.ReadBytesAt(this.UnknownData2Pointer, this.UnknownData2Count);



            if ((Children != null) && (Children.data_items != null))
            {
                for (int i = 0; i < Children.data_items.Length; i++)
                {
                    var child = Children.data_items[i];
                    child.OwnerFragPhysLod = this;
                    child.OwnerFragPhysIndex = i;

                    child.UnkFloatFromParent = ((ChildrenUnkFloats != null) && (i < ChildrenUnkFloats.Length)) ? ChildrenUnkFloats[i] : 0;
                    child.UnkVecFromParent = ((ChildrenUnkVecs != null) && (i < ChildrenUnkVecs.Length)) ? ChildrenUnkVecs[i] : Vector4.Zero;
                    child.InertiaTensorFromParent = ((ChildrenInertiaTensors != null) && (i < ChildrenInertiaTensors.Length)) ? ChildrenInertiaTensors[i] : Vector4.Zero;

                    var gi = child.GroupIndex;
                    if ((Groups?.data_items != null) && (gi < Groups.data_items.Length))
                    {
                        var group = Groups.data_items[gi];
                        child.Group = group;
                    }
                }
            }


            if ((Groups?.data_items != null) && (GroupNames?.data_items != null) && (Groups.data_items.Length == GroupNames.data_items.Length))
            {
                //this fixes up broken group names caused by zmod, but it's not necessary for vanilla files, 
                //since the group name pointers should point at the names embedded in the groups themselves.
                for (int i = 0; i < Groups.data_items.Length; i++)
                {
                    Groups.data_items[i].Name = GroupNames.data_items[i];
                }
            }

            if (Archetype1 != null)
            {
                Archetype1.Owner = this;
            }
            if (Archetype2 != null)
            {
                Archetype2.Owner = this;
            }
            if (Bound != null)
            {
                Bound.Owner = this;
            }

            //if (Bound is BoundComposite bcmp)
            //{
            //    if ((bcmp.ChildrenFlags1 != null) || (bcmp.ChildrenFlags2 != null))
            //    { }//no hit
            //}


            //if ((Unknown_3Ch != 0x7f800001) && (Unknown_3Ch != 0x7fc00001))
            //{ }//no hit
            //if ((Unknown_4Ch != 0x7f800001) && (Unknown_4Ch != 0x7fc00001))
            //{ }//no hit
            //if (Unknown_5Ch != 0x7f800001)
            //{ }//no hit
            //if (Unknown_6Ch != 0x7f800001)
            //{ }//no hit
            //if (Unknown_7Ch != 0x7f800001)
            //{ }//no hit
            //if (Unknown_8Ch != 0x7f800001)
            //{ }//no hit
            //if (Unknown_9Ch != 0x7f800001)
            //{ }//no hit
            //if (Unknown_ACh != 0x7f800001)
            //{ }//no hit
            //if (Unknown_BCh != 0x7f800001)
            //{ }//no hit
            //if (Unknown_04h != 1)
            //{ }//no hit
            //if (Unknown_08h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_11Ch != 1)
            //{ }//no hit
            //if (Unknown_11Fh != 0)
            //{ }//no hit
            //if (Unknown_120h != 0)
            //{ }//no hit
            //if (Unknown_128h != 0)
            //{ }//no hit
            //if (ChildrenCount2 != ChildrenCount)
            //{ }//no hit
            //if (ArticulatedBodyType == null)
            //{ }//hit
            //if (GroupNames?.data_items == null)
            //{ }//no hit
            //if (Groups?.data_items == null)
            //{ }//no hit
            //if (Children?.data_items == null)
            //{ }//no hit
            //if (Archetype1 == null)
            //{ }//no hit
            //if (Archetype2 == null)
            //{ }//hit
            //if (Archetype2 == Archetype1)
            //{ }//no hit
            //if (Bound == null)
            //{ }//no hit
            //if (ChildrenInertiaTensors == null)
            //{ }//no hit
            //if (ChildrenUnkFloats == null)
            //{ }//no hit
            //if (ChildrenUnkVecs == null)
            //{ }//no hit
            //if (FragTransforms == null)
            //{ }//no hit
            //if (UnknownData1 == null)
            //{ }//hit
            //if (UnknownData2 == null)
            //{ }//hit
            //BuildGroupsData();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.ArticulatedBodyTypePointer = (ulong)(this.ArticulatedBodyType != null ? this.ArticulatedBodyType.FilePosition : 0);
            this.ChildrenUnkFloatsPointer = (ulong)(this.ChildrenUnkFloatsBlock != null ? this.ChildrenUnkFloatsBlock.FilePosition : 0);
            this.GroupNamesPointer = (ulong)(this.GroupNames != null ? this.GroupNames.FilePosition : 0);
            this.GroupsPointer = (ulong)(this.Groups != null ? this.Groups.FilePosition : 0);
            this.ChildrenPointer = (ulong)(this.Children != null ? this.Children.FilePosition : 0);
            this.Archetype1Pointer = (ulong)(this.Archetype1 != null ? this.Archetype1.FilePosition : 0);
            this.Archetype2Pointer = (ulong)(this.Archetype2 != null ? this.Archetype2.FilePosition : 0);
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);
            this.ChildrenInertiaTensorsPointer = (ulong)(this.ChildrenInertiaTensorsBlock != null ? this.ChildrenInertiaTensorsBlock.FilePosition : 0);
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
            writer.Write(this.ChildrenUnkFloatsPointer);
            writer.Write(this.PositionOffset);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_5Ch);
            writer.Write(this.DampingLinearC);
            writer.Write(this.Unknown_6Ch);
            writer.Write(this.DampingLinearV);
            writer.Write(this.Unknown_7Ch);
            writer.Write(this.DampingLinearV2);
            writer.Write(this.Unknown_8Ch);
            writer.Write(this.DampingAngularC);
            writer.Write(this.Unknown_9Ch);
            writer.Write(this.DampingAngularV);
            writer.Write(this.Unknown_ACh);
            writer.Write(this.DampingAngularV2);
            writer.Write(this.Unknown_BCh);
            writer.Write(this.GroupNamesPointer);
            writer.Write(this.GroupsPointer);
            writer.Write(this.ChildrenPointer);
            writer.Write(this.Archetype1Pointer);
            writer.Write(this.Archetype2Pointer);
            writer.Write(this.BoundPointer);
            writer.Write(this.ChildrenInertiaTensorsPointer);
            writer.Write(this.ChildrenUnkVecsPointer);
            writer.Write(this.FragTransformsPointer);
            writer.Write(this.UnknownData1Pointer);
            writer.Write(this.UnknownData2Pointer);
            writer.Write(this.UnknownData1Count);
            writer.Write(this.UnknownData2Count);
            writer.Write(this.GroupsCount);
            writer.Write(this.RootGroupsCount);
            writer.Write(this.Unknown_11Ch);
            writer.Write(this.ChildrenCount);
            writer.Write(this.ChildrenCount2);
            writer.Write(this.Unknown_11Fh);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_128h);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            YftXml.ValueTag(sb, indent, "Unknown14", FloatUtil.ToString(Unknown_14h));
            YftXml.ValueTag(sb, indent, "Unknown18", FloatUtil.ToString(Unknown_18h));
            YftXml.ValueTag(sb, indent, "Unknown1C", FloatUtil.ToString(Unknown_1Ch));
            YftXml.SelfClosingTag(sb, indent, "PositionOffset " + FloatUtil.GetVector3XmlString(PositionOffset));
            YftXml.SelfClosingTag(sb, indent, "Unknown40 " + FloatUtil.GetVector3XmlString(Unknown_40h));
            YftXml.SelfClosingTag(sb, indent, "Unknown50 " + FloatUtil.GetVector3XmlString(Unknown_50h));
            YftXml.SelfClosingTag(sb, indent, "DampingLinearC " + FloatUtil.GetVector3XmlString(DampingLinearC));
            YftXml.SelfClosingTag(sb, indent, "DampingLinearV " + FloatUtil.GetVector3XmlString(DampingLinearV));
            YftXml.SelfClosingTag(sb, indent, "DampingLinearV2 " + FloatUtil.GetVector3XmlString(DampingLinearV2));
            YftXml.SelfClosingTag(sb, indent, "DampingAngularC " + FloatUtil.GetVector3XmlString(DampingAngularC));
            YftXml.SelfClosingTag(sb, indent, "DampingAngularV " + FloatUtil.GetVector3XmlString(DampingAngularV));
            YftXml.SelfClosingTag(sb, indent, "DampingAngularV2 " + FloatUtil.GetVector3XmlString(DampingAngularV2));
            if (Archetype1 != null)
            {
                YftXml.OpenTag(sb, indent, "Archetype");
                Archetype1.WriteXml(sb, indent + 1);
                YftXml.CloseTag(sb, indent, "Archetype");
            }
            if (Archetype2 != null)
            {
                YftXml.OpenTag(sb, indent, "Archetype2");
                Archetype2.WriteXml(sb, indent + 1);
                YftXml.CloseTag(sb, indent, "Archetype2");
            }
            if (ArticulatedBodyType != null)
            {
                YftXml.OpenTag(sb, indent, "ArticulatedBody");
                ArticulatedBodyType.WriteXml(sb, indent + 1);
                YftXml.CloseTag(sb, indent, "ArticulatedBody");
            }
            if (FragTransforms != null)
            {
                YftXml.OpenTag(sb, indent, "Transforms");
                FragTransforms.WriteXml(sb, indent + 1);
                YftXml.CloseTag(sb, indent, "Transforms");
            }
            if (Groups?.data_items != null)
            {
                YftXml.WriteItemArray(sb, Groups.data_items, indent, "Groups");
            }
            if (Children?.data_items != null)
            {
                YftXml.OpenTag(sb, indent, "Children");
                var cind = indent + 1;
                var cind2 = cind + 1;
                foreach (var child in Children?.data_items)
                {
                    if (child != null)
                    {
                        YftXml.OpenTag(sb, cind, "Item");
                        child.WriteXml(sb, cind2, ddsfolder);
                        YftXml.CloseTag(sb, cind, "Item");
                    }
                    else
                    {
                        YftXml.SelfClosingTag(sb, cind, "Item");
                    }
                }
                YftXml.CloseTag(sb, indent, "Children");
            }
            //if (Bound != null)
            //{
            //    Bounds.WriteXmlNode(Bound, sb, indent);
            //}
            if (UnknownData1 != null)
            {
                YftXml.WriteRawArray(sb, UnknownData1, indent, "UnknownData1", "");
            }
            if (UnknownData2 != null)
            {
                YftXml.WriteRawArray(sb, UnknownData2, indent, "UnknownData2", "");
            }
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            Unknown_14h = Xml.GetChildFloatAttribute(node, "Unknown14", "value");
            Unknown_18h = Xml.GetChildFloatAttribute(node, "Unknown18", "value");
            Unknown_1Ch = Xml.GetChildFloatAttribute(node, "Unknown1C", "value");
            PositionOffset = Xml.GetChildVector3Attributes(node, "PositionOffset");
            Unknown_40h = Xml.GetChildVector3Attributes(node, "Unknown40");
            Unknown_50h = Xml.GetChildVector3Attributes(node, "Unknown50");
            DampingLinearC = Xml.GetChildVector3Attributes(node, "DampingLinearC");
            DampingLinearV = Xml.GetChildVector3Attributes(node, "DampingLinearV");
            DampingLinearV2 = Xml.GetChildVector3Attributes(node, "DampingLinearV2");
            DampingAngularC = Xml.GetChildVector3Attributes(node, "DampingAngularC");
            DampingAngularV = Xml.GetChildVector3Attributes(node, "DampingAngularV");
            DampingAngularV2 = Xml.GetChildVector3Attributes(node, "DampingAngularV2");
            var a1node = node.SelectSingleNode("Archetype");
            if (a1node != null)
            {
                Archetype1 = new FragPhysArchetype();
                Archetype1.Owner = this;
                Archetype1.ReadXml(a1node);
            }
            var a2node = node.SelectSingleNode("Archetype2");
            if (a2node != null)
            {
                Archetype2 = new FragPhysArchetype();
                Archetype2.Owner = this;
                Archetype2.ReadXml(a2node);
            }
            var abnode = node.SelectSingleNode("ArticulatedBody");
            if (abnode != null)
            {
                ArticulatedBodyType = new FragPhysArticulatedBodyType();
                ArticulatedBodyType.ReadXml(abnode);
            }
            var tnode = node.SelectSingleNode("Transforms");
            if (tnode != null)
            {
                FragTransforms = new FragPhysTransforms();
                FragTransforms.ReadXml(tnode);
            }
            var grps = XmlMeta.ReadItemArray<FragPhysTypeGroup>(node, "Groups");
            if ((grps?.Length ?? 0) > 0)
            {
                Groups = new ResourcePointerArray64<FragPhysTypeGroup>();
                Groups.data_items = grps;
            }
            var chnode = node.SelectSingleNode("Children");
            if (chnode != null)
            {
                var clist = new List<FragPhysTypeChild>();
                var cnodes = chnode.SelectNodes("Item");
                if ((cnodes?.Count ?? 0) > 0)
                {
                    foreach (XmlNode cnode in cnodes)
                    {
                        if (cnode.HasChildNodes)
                        {
                            var c = new FragPhysTypeChild();
                            c.ReadXml(cnode, ddsfolder);
                            clist.Add(c);
                        }
                        else
                        {
                            clist.Add(null);
                        }
                    }
                }
                Children = new ResourcePointerArray64<FragPhysTypeChild>();
                Children.data_items = clist.ToArray();
            }
            //var bnode = node.SelectSingleNode("Bounds");
            //if (bnode != null)
            //{
            //    Bound = Bounds.ReadXmlNode(bnode, this);
            //}
            var ud1 = Xml.GetChildRawByteArray(node, "UnknownData1", 10);
            var ud2 = Xml.GetChildRawByteArray(node, "UnknownData2", 10);
            UnknownData1 = ((ud1?.Length ?? 0) > 0) ? ud1 : null;
            UnknownData2 = ((ud2?.Length ?? 0) > 0) ? ud2 : null;

            BuildChildrenData();
            BuildGroupsData();
        }


        public void BuildChildrenData()
        {
            if (Children?.data_items == null) return;

            var unkfloats = new List<float>();
            var unkvecs = new List<Vector4>();
            var tensors = new List<Vector4>();
            foreach (var child in Children.data_items)
            {
                unkfloats.Add(child?.UnkFloatFromParent ?? 0);
                unkvecs.Add(child?.UnkVecFromParent ?? Vector4.Zero);
                tensors.Add(child?.InertiaTensorFromParent ?? Vector4.Zero);
            }

            ChildrenUnkFloats = unkfloats.ToArray();
            ChildrenUnkVecs = unkvecs.ToArray();
            ChildrenInertiaTensors = tensors.ToArray();

        }

        public void BuildGroupsData()
        {
            var grpnames = new List<FragPhysNameStruct_s>();
            var rootgrps = 0;
            if (Groups?.data_items != null)
            {
                for (int i = 0; i < Groups.data_items.Length; i++)
                {
                    var grp = Groups.data_items[i];
                    grpnames.Add(grp.Name);
                    if (grp.ParentIndex == 255)
                    {
                        rootgrps++;
                    }


                    ////just testing
                    //var childGroupIndex = grp.ChildGroupIndex;
                    //var childGroupCount = grp.ChildGroupCount;
                    //var childIndex = grp.ChildIndex;
                    //var childCount = grp.ChildCount;


                    grp.ChildGroupIndex = 255;
                    grp.ChildGroupCount = 0;
                    grp.ChildIndex = 255;
                    grp.ChildCount = 0;
                    bool childfound = false;
                    for(int ii = 0; ii < Groups.data_items.Length; ii++)
                    {
                        var grp2 = Groups.data_items[ii];
                        if (grp2.ParentIndex == i)
                        {
                            grp.ChildGroupCount++;
                            if (childfound == false)
                            {
                                grp.ChildGroupIndex = (byte)ii;
                                childfound = true;
                            }
                        }
                    }
                    childfound = false;
                    var childrencount = Children?.data_items?.Length ?? 0;
                    for (int ii = 0; ii < childrencount; ii++)
                    {
                        var child = Children.data_items[ii];
                        if (child == null) continue;
                        if (child.GroupIndex == i)
                        {
                            grp.ChildCount++;
                            if (childfound == false)
                            {
                                grp.ChildIndex = (byte)ii;
                                childfound = true;
                            }
                        }
                    }

                    ////just testing
                    //if (grp.ChildGroupIndex != childGroupIndex)
                    //{ }
                    //if (grp.ChildGroupCount != childGroupCount)
                    //{ }
                    //if (grp.ChildIndex != childIndex)
                    //{ }
                    //if (grp.ChildCount != childCount)
                    //{ }
                }
            }

            ////just testing
            //if (RootGroupsCount != rootgrps)
            //{ }
            RootGroupsCount = (byte)rootgrps;

            if (grpnames.Count > 0)
            {
                if (GroupNames == null) GroupNames = new FragPhysGroupNamesBlock();
                GroupNames.data_items = grpnames.ToArray();
                GroupNames.Groups = Groups?.data_items;
            }
            else
            {
                GroupNames = null;
            }

        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (ArticulatedBodyType != null) list.Add(ArticulatedBodyType);
            if (ChildrenUnkFloats != null)
            {
                ChildrenUnkFloatsBlock = new ResourceSystemStructBlock<float>(ChildrenUnkFloats);
                list.Add(ChildrenUnkFloatsBlock);
            }
            if (Groups != null) list.Add(Groups);
            if (Children != null) list.Add(Children);
            if (Archetype1 != null) list.Add(Archetype1);
            if (Archetype2 != null) list.Add(Archetype2);
            if (Bound != null) list.Add(Bound);
            if (ChildrenInertiaTensors != null)
            {
                ChildrenInertiaTensorsBlock = new ResourceSystemStructBlock<Vector4>(ChildrenInertiaTensors);
                list.Add(ChildrenInertiaTensorsBlock);
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
            if (GroupNames != null)
            {
                list.Add(GroupNames);
            }
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
        public uint VFT { get; set; } = 1080211704;
        public uint Unknown_04h = 1; // 0x00000001
        public ulong Unknown_08h; // 0x0000000000000000
        public uint[] ItemIndices { get; set; } // array of 22 uints
        public uint Unknown_68h; // 0x00000000
        public float Unknown_6Ch = 1.0f; // 1.0f
        public ulong Unknown_70h; // 0x0000000000000000
        public ulong JointsPointer { get; set; }
        public ulong UnknownVectorsPointer { get; set; }
        public byte UnknownVectorsCount { get; set; }
        public byte JointsCount { get; set; }
        public byte[] ItemFlags { get; set; } //array of 22 bytes, could be joint types?
        public ulong Unknown_A0h; // 0x0000000000000000
        public ulong Unknown_A8h; // 0x0000000000000000

        // reference data
        public ResourcePointerArray64<FragPhysJointType> Joints { get; set; }
        public Vector4[] UnknownVectors { get; set; }

        private ResourceSystemStructBlock<Vector4> UnknownVectorsBlock = null;//only used for saving


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt64();
            this.ItemIndices = reader.ReadStructs<uint>(22);
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadSingle();
            this.Unknown_70h = reader.ReadUInt64();
            this.JointsPointer = reader.ReadUInt64();
            this.UnknownVectorsPointer = reader.ReadUInt64();
            this.UnknownVectorsCount = reader.ReadByte();
            this.JointsCount = reader.ReadByte();
            this.ItemFlags = reader.ReadBytes(22);
            this.Unknown_A0h = reader.ReadUInt64();
            this.Unknown_A8h = reader.ReadUInt64();

            // read reference data
            this.Joints = reader.ReadBlockAt<ResourcePointerArray64<FragPhysJointType>>(this.JointsPointer, this.JointsCount);
            this.UnknownVectors = reader.ReadStructsAt<Vector4>(this.UnknownVectorsPointer, this.UnknownVectorsCount);


            ////testing!!
            //if (Unknown_04h != 1)
            //{ }//no hit
            //if (Unknown_08h != 0)
            //{ }//no hit
            //if (Unknown_68h != 0)
            //{ }//no hit
            //if (Unknown_6Ch != 1.0f)
            //{ }//no hit
            //if (Unknown_70h != 0)
            //{ }//no hit
            //if (Unknown_A0h != 0)
            //{ }//no hit
            //if (Unknown_A8h != 0)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.JointsPointer = (ulong)(this.Joints != null ? this.Joints.FilePosition : 0);
            this.UnknownVectorsPointer = (ulong)(this.UnknownVectorsBlock != null ? this.UnknownVectorsBlock.FilePosition : 0);
            this.UnknownVectorsCount = (byte)(this.UnknownVectorsBlock != null ? this.UnknownVectorsBlock.ItemCount : 0);
            this.JointsCount = (byte)(this.Joints != null ? this.Joints.Count : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.WriteStructs(ItemIndices);//22 uints
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
            writer.Write(this.Unknown_70h);
            writer.Write(this.JointsPointer);
            writer.Write(this.UnknownVectorsPointer);
            writer.Write(this.UnknownVectorsCount);
            writer.Write(this.JointsCount);
            writer.Write(this.ItemFlags);//22 bytes
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_A8h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            if (ItemIndices != null)
            {
                YftXml.WriteRawArray(sb, ItemIndices, indent, "ItemIndices", "", null, 22);
            }
            if (ItemFlags != null)
            {
                YftXml.WriteRawArray(sb, ItemFlags, indent, "ItemFlags", "", null, 22);
            }
            if (UnknownVectors != null)
            {
                YftXml.WriteRawArray(sb, UnknownVectors, indent, "UnknownVectors", "", v => FloatUtil.GetVector4String(v), 1);
            }
            if (Joints?.data_items != null)
            {
                var itemCount = Joints.data_items.Length;
                var cind = indent + 1;
                var cind2 = cind + 1;
                YftXml.OpenTag(sb, indent, "Joints");
                for (int i = 0; i < itemCount; i++)
                {
                    var j = Joints.data_items[i];
                    if (j != null)
                    {
                        YftXml.OpenTag(sb, cind, "Item type=\"" + j.Type.ToString() + "\"");
                        j.WriteXml(sb, cind2);
                        YftXml.CloseTag(sb, cind, "Item");
                    }
                    else
                    {
                        YftXml.SelfClosingTag(sb, cind, "Item");
                    }
                }
                YftXml.CloseTag(sb, indent, "Joints");
            }
        }
        public void ReadXml(XmlNode node)
        {
            var ii = Xml.GetChildRawUintArray(node, "ItemIndices");
            var fi = Xml.GetChildRawByteArray(node, "ItemFlags", 10);
            var uv = Xml.GetChildRawVector4Array(node, "UnknownVectors");
            ItemIndices = ((ii?.Length ?? 0) == 22) ? ii : new uint[22];
            ItemFlags = ((fi?.Length ?? 0) == 22) ? fi : new byte[22];
            UnknownVectors = ((uv?.Length ?? 0) > 0) ? uv : null;
            var jsnode = node.SelectSingleNode("Joints");
            if (jsnode != null)
            {
                var jlist = new List<FragPhysJointType>();
                var jnodes = jsnode.SelectNodes("Item");
                foreach (XmlNode jnode in jnodes)
                {
                    var type = Xml.GetEnumValue<FragJointType>(Xml.GetStringAttribute(jnode, "type"));
                    var j = FragPhysJointType.Create(type);
                    j?.ReadXml(jnode);
                    jlist.Add(j);
                }
                Joints = new ResourcePointerArray64<FragPhysJointType>();
                Joints.data_items = jlist.ToArray();
            }
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Joints != null) list.Add(Joints);
            if (UnknownVectors != null)
            {
                UnknownVectorsBlock = new ResourceSystemStructBlock<Vector4>(UnknownVectors);
                list.Add(UnknownVectorsBlock);
            }
            return list.ToArray();
        }
    }

    public enum FragJointType : byte
    {
        DOF1 = 0,
        DOF3 = 1,
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysJointType : ResourceSystemBlock, IResourceXXSystemBlock
    {
        public override long BlockLength
        {
            get { return 32; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_04h = 1; // 0x00000001
        public ulong Unknown_08h; // 0x0000000000000000
        public float Unknown_10h = 0.825f; // 0x3F533333 (0.825f)
        public byte Unknown_14h; // 0x00
        public FragJointType Type { get; set; }
        public byte FragIndex1 { get; set; }
        public byte FragIndex2 { get; set; }
        public ulong Unknown_18h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadSingle();
            this.Unknown_14h = reader.ReadByte();
            this.Type = (FragJointType)reader.ReadByte();
            this.FragIndex1 = reader.ReadByte();
            this.FragIndex2 = reader.ReadByte();
            this.Unknown_18h = reader.ReadUInt64();

            //if (Unknown_04h != 1)
            //{ }//no hit
            //if (Unknown_08h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0.825f)
            //{ }//no hit
            //if (Unknown_14h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
            //if (Index1 == Index2)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write((byte)this.Type);
            writer.Write(this.FragIndex1);
            writer.Write(this.FragIndex2);
            writer.Write(this.Unknown_18h);
        }
        public virtual void WriteXml(StringBuilder sb, int indent)
        {
            YftXml.ValueTag(sb, indent, "FragIndex1", FragIndex1.ToString());
            YftXml.ValueTag(sb, indent, "FragIndex2", FragIndex2.ToString());
            YftXml.ValueTag(sb, indent, "Unknown10", FloatUtil.ToString(Unknown_10h));
        }
        public virtual void ReadXml(XmlNode node)
        {
            FragIndex1 = (byte)Xml.GetChildUIntAttribute(node, "FragIndex1", "value");
            FragIndex2 = (byte)Xml.GetChildUIntAttribute(node, "FragIndex2", "value");
            Unknown_10h = Xml.GetChildFloatAttribute(node, "Unknown10", "value");
        }

        public static FragPhysJointType Create(FragJointType type)
        {
            switch (type)
            {
                case FragJointType.DOF1: return new FragPhysJoint1DofType();
                case FragJointType.DOF3: return new FragPhysJoint3DofType();
                default: return null;// throw new Exception("Unknown type");
            }
        }
        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 21;
            var type = (FragJointType)reader.ReadByte();
            reader.Position -= 22;
            return Create(type);
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysJoint1DofType : FragPhysJointType
    {
        public override long BlockLength
        {
            get { return 176; }
        }

        // structure data
        public Vector4 Unknown_20h { get; set; }
        public Vector4 Unknown_30h { get; set; }
        public Vector4 Unknown_40h { get; set; }
        public Vector4 Unknown_50h { get; set; }
        public Vector4 Unknown_60h { get; set; }
        public Vector4 Unknown_70h { get; set; }
        public Vector4 Unknown_80h { get; set; }
        public Vector4 Unknown_90h { get; set; }
        public Vector4 Unknown_A0h { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_20h = reader.ReadVector4();
            this.Unknown_30h = reader.ReadVector4();
            this.Unknown_40h = reader.ReadVector4();
            this.Unknown_50h = reader.ReadVector4();
            this.Unknown_60h = reader.ReadVector4();
            this.Unknown_70h = reader.ReadVector4();
            this.Unknown_80h = reader.ReadVector4();
            this.Unknown_90h = reader.ReadVector4();
            this.Unknown_A0h = reader.ReadVector4();

            //if (Unknown_20h.W != 0)
            //{ }//no hit
            //if (Unknown_30h.W != 0)
            //{ }//no hit
            //if (Unknown_40h.W != 0)
            //{ }//no hit
            //if (!float.IsNaN(Unknown_50h.W))//todo: check lower bytes for more info! 
            //{ }//no hit
            //if (Unknown_60h.W != 0)
            //{ }//no hit
            //if (Unknown_70h.W != 0)
            //{ }//no hit
            //if (Unknown_80h.W != 0)
            //{ }//no hit
            //if (!float.IsNaN(Unknown_90h.W))//todo: check lower bytes for more info!
            //{ }//no hit
            //if (Unknown_A0h.Z != 1e8f)
            //{ }//no hit
            //if (Unknown_A0h.W != -1e8f)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_A0h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YftXml.SelfClosingTag(sb, indent, "Unknown20 " + FloatUtil.GetVector4XmlString(Unknown_20h));
            YftXml.SelfClosingTag(sb, indent, "Unknown30 " + FloatUtil.GetVector4XmlString(Unknown_30h));
            YftXml.SelfClosingTag(sb, indent, "Unknown40 " + FloatUtil.GetVector4XmlString(Unknown_40h));
            YftXml.SelfClosingTag(sb, indent, "Unknown50 " + FloatUtil.GetVector4XmlString(Unknown_50h));
            YftXml.SelfClosingTag(sb, indent, "Unknown60 " + FloatUtil.GetVector4XmlString(Unknown_60h));
            YftXml.SelfClosingTag(sb, indent, "Unknown70 " + FloatUtil.GetVector4XmlString(Unknown_70h));
            YftXml.SelfClosingTag(sb, indent, "Unknown80 " + FloatUtil.GetVector4XmlString(Unknown_80h));
            YftXml.SelfClosingTag(sb, indent, "Unknown90 " + FloatUtil.GetVector4XmlString(Unknown_90h));
            YftXml.SelfClosingTag(sb, indent, "UnknownA0 " + FloatUtil.GetVector4XmlString(Unknown_A0h));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_20h = Xml.GetChildVector4Attributes(node, "Unknown20");
            Unknown_30h = Xml.GetChildVector4Attributes(node, "Unknown30");
            Unknown_40h = Xml.GetChildVector4Attributes(node, "Unknown40");
            Unknown_50h = Xml.GetChildVector4Attributes(node, "Unknown50");
            Unknown_60h = Xml.GetChildVector4Attributes(node, "Unknown60");
            Unknown_70h = Xml.GetChildVector4Attributes(node, "Unknown70");
            Unknown_80h = Xml.GetChildVector4Attributes(node, "Unknown80");
            Unknown_90h = Xml.GetChildVector4Attributes(node, "Unknown90");
            Unknown_A0h = Xml.GetChildVector4Attributes(node, "UnknownA0");

            VFT = 1080212656;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysJoint3DofType : FragPhysJointType
    {
        public override long BlockLength
        {
            get { return 240; }
        }

        // structure data
        public Vector4 Unknown_20h { get; set; }
        public Vector4 Unknown_30h { get; set; }
        public Vector4 Unknown_40h { get; set; }
        public Vector4 Unknown_50h { get; set; }
        public Vector4 Unknown_60h { get; set; }
        public Vector4 Unknown_70h { get; set; }
        public Vector4 Unknown_80h { get; set; }
        public Vector4 Unknown_90h { get; set; }
        public Vector4 Unknown_A0h { get; set; }
        public Vector4 Unknown_B0h { get; set; } // 0x00000000
        public Vector4 Unknown_C0h { get; set; } = new Vector4( 1e8f); // 0x4CBEBC20  1e8f
        public Vector4 Unknown_D0h { get; set; } = new Vector4(-1e8f); // 0xCCBEBC20 -1e8f
        public Vector4 Unknown_E0h { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_20h = reader.ReadVector4();
            this.Unknown_30h = reader.ReadVector4();
            this.Unknown_40h = reader.ReadVector4();
            this.Unknown_50h = reader.ReadVector4();
            this.Unknown_60h = reader.ReadVector4();
            this.Unknown_70h = reader.ReadVector4();
            this.Unknown_80h = reader.ReadVector4();
            this.Unknown_90h = reader.ReadVector4();
            this.Unknown_A0h = reader.ReadVector4();
            this.Unknown_B0h = reader.ReadVector4();
            this.Unknown_C0h = reader.ReadVector4();
            this.Unknown_D0h = reader.ReadVector4();
            this.Unknown_E0h = reader.ReadVector4();

            //if (Unknown_20h.W != 0)
            //{ }//no hit
            //if (Unknown_30h.W != 0)
            //{ }//no hit
            //if (Unknown_40h.W != 0)
            //{ }//no hit
            //if (!float.IsNaN(Unknown_50h.W))
            //{ }//no hit
            //if (Unknown_60h.W != 0)
            //{ }//no hit
            //if (Unknown_70h.W != 0)
            //{ }//no hit
            //if (Unknown_80h.W != 0)
            //{ }//no hit
            //if (!float.IsNaN(Unknown_90h.W))
            //{ }//no hit
            //if (Unknown_A0h.W != 0)
            //{ }//no hit
            //if (Unknown_B0h != Vector4.Zero)
            //{ }//no hit
            //if (Unknown_C0h != new Vector4(1e8f))
            //{ }//no hit
            //if (Unknown_D0h != new Vector4(-1e8f))
            //{ }//no hit
            //if (Unknown_E0h != Vector4.Zero)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_B0h);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_D0h);
            writer.Write(this.Unknown_E0h);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YftXml.SelfClosingTag(sb, indent, "Unknown20 " + FloatUtil.GetVector4XmlString(Unknown_20h));
            YftXml.SelfClosingTag(sb, indent, "Unknown30 " + FloatUtil.GetVector4XmlString(Unknown_30h));
            YftXml.SelfClosingTag(sb, indent, "Unknown40 " + FloatUtil.GetVector4XmlString(Unknown_40h));
            YftXml.SelfClosingTag(sb, indent, "Unknown50 " + FloatUtil.GetVector4XmlString(Unknown_50h));
            YftXml.SelfClosingTag(sb, indent, "Unknown60 " + FloatUtil.GetVector4XmlString(Unknown_60h));
            YftXml.SelfClosingTag(sb, indent, "Unknown70 " + FloatUtil.GetVector4XmlString(Unknown_70h));
            YftXml.SelfClosingTag(sb, indent, "Unknown80 " + FloatUtil.GetVector4XmlString(Unknown_80h));
            YftXml.SelfClosingTag(sb, indent, "Unknown90 " + FloatUtil.GetVector4XmlString(Unknown_90h));
            YftXml.SelfClosingTag(sb, indent, "UnknownA0 " + FloatUtil.GetVector4XmlString(Unknown_A0h));
            //YftXml.SelfClosingTag(sb, indent, "UnknownB0 " + FloatUtil.GetVector4XmlString(Unknown_B0h));
            //YftXml.SelfClosingTag(sb, indent, "UnknownC0 " + FloatUtil.GetVector4XmlString(Unknown_C0h));
            //YftXml.SelfClosingTag(sb, indent, "UnknownD0 " + FloatUtil.GetVector4XmlString(Unknown_D0h));
            //YftXml.SelfClosingTag(sb, indent, "UnknownE0 " + FloatUtil.GetVector4XmlString(Unknown_E0h));
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_20h = Xml.GetChildVector4Attributes(node, "Unknown20");
            Unknown_30h = Xml.GetChildVector4Attributes(node, "Unknown30");
            Unknown_40h = Xml.GetChildVector4Attributes(node, "Unknown40");
            Unknown_50h = Xml.GetChildVector4Attributes(node, "Unknown50");
            Unknown_60h = Xml.GetChildVector4Attributes(node, "Unknown60");
            Unknown_70h = Xml.GetChildVector4Attributes(node, "Unknown70");
            Unknown_80h = Xml.GetChildVector4Attributes(node, "Unknown80");
            Unknown_90h = Xml.GetChildVector4Attributes(node, "Unknown90");
            Unknown_A0h = Xml.GetChildVector4Attributes(node, "UnknownA0");
            //Unknown_B0h = Xml.GetChildVector4Attributes(node, "UnknownB0");
            //Unknown_C0h = Xml.GetChildVector4Attributes(node, "UnknownC0");
            //Unknown_D0h = Xml.GetChildVector4Attributes(node, "UnknownD0");
            //Unknown_E0h = Xml.GetChildVector4Attributes(node, "UnknownE0");

            VFT = 1080212544;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysTransforms : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32 + ((Matrices?.Length ?? 0) * 64); }
        }

        // structure data
        public uint VFT { get; set; } = 1080043536;
        public uint Unknown_04h = 1; // 0x00000001
        public ulong Unknown_08h; // 0x0000000000000000
        public uint MatricesCount { get; set; }
        public uint Unknown_14h; // 0x00000000
        public ulong Unknown_18h; // 0x0000000000000000
        public Matrix[] Matrices { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt64();
            this.MatricesCount = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt64();
            this.Matrices = reader.ReadStructsAt<Matrix>((ulong)reader.Position, MatricesCount);

            //if (Unknown_04h != 1)
            //{ }//no hit
            //if (Unknown_08h != 0)
            //{ }//no hit
            //if (Unknown_14h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            MatricesCount = (uint)(Matrices?.Length ?? 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.MatricesCount);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.WriteStructs(Matrices);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            if ((Matrices != null) && (Matrices.Length > 0))
            {
                for (int i = 0; i < Matrices.Length; i++)
                {
                    YftXml.OpenTag(sb, indent, "Item");
                    YftXml.WriteRawArrayContent(sb, Matrices[i].ToArray(), indent + 1, FloatUtil.ToString, 4);
                    YftXml.CloseTag(sb, indent, "Item");
                }
            }
        }
        public void ReadXml(XmlNode node)
        {
            var mats = new List<Matrix>();
            var matinds = new List<ulong>();
            var inodes = node.SelectNodes("Item");
            foreach (XmlNode inode in inodes)
            {
                var mat = Xml.GetMatrix(inode);
                mats.Add(mat);
            }
            Matrices = (mats.Count > 0) ? mats.ToArray() : null;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysArchetype : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 224; }
        }

        // structure data
        public uint VFT { get; set; } = 1080215944;
        public uint Unknown_04h = 1; // 0x00000001
        public ulong Unknown_08h; // 0x0000000000000000
        public ulong Unknown_10h = 2; // 0x0000000000000002
        public ulong NamePointer { get; set; }
        public ulong BoundPointer { get; set; }
        public uint Unknown_28h = 1; // 0x00000001
        public uint Unknown_2Ch = 0xFFFFFFFF; // 0xFFFFFFFF
        public uint Unknown_30h = 0x00010000; // 0x00010000
        public uint Unknown_34h; // 0x00000000
        public ulong Unknown_38h; // 0x0000000000000000
        public float Mass { get; set; } //in pounds, of course
        public float MassInv { get; set; } // 1.0 / Mass
        public float Unknown_48h { get; set; } = 1.0f; // 1.0f
        public float Unknown_4Ch { get; set; } = 150.0f; // 150.0f
        public float Unknown_50h { get; set; } = 6.2831855f; // 6.2831855f = 2*pi
        public float Unknown_54h { get; set; } = 1.0f; // 1.0f
        public ulong Unknown_58h; // 0x0000000000000000
        public Vector3 InertiaTensor { get; set; }
        public uint Unknown_6Ch = 0x7fc00001; // 0x7fc00001
        public Vector3 InertiaTensorInv { get; set; } // 1.0 / InertiaTensor
        public uint Unknown_7Ch = 0x7fc00001; // 0x7fc00001
        public Vector3 Unknown_80h { get; set; } = Vector3.Zero; // 0.0 0.0 0.0 NaN
        public uint Unknown_8Ch = 0x7f800001; // 0x7f800001
        public Vector3 Unknown_90h { get; set; } = Vector3.Zero; // 0.0 0.0 0.0 NaN
        public uint Unknown_9Ch = 0x7f800001; // 0x7f800001
        public Vector3 Unknown_A0h { get; set; } = Vector3.Zero; // 0.0 0.0 0.0 NaN
        public uint Unknown_ACh = 0x7f800001; // 0x7f800001
        public Vector3 Unknown_B0h { get; set; } = Vector3.Zero; // 0.0 0.0 0.0 NaN
        public uint Unknown_BCh = 0x7f800001; // 0x7f800001
        public Vector3 Unknown_C0h { get; set; } = Vector3.Zero; // 0.0 0.0 0.0 NaN
        public uint Unknown_CCh = 0x7f800001; // 0x7f800001
        public Vector3 Unknown_D0h { get; set; } = Vector3.Zero; // 0.0 0.0 0.0 NaN
        public uint Unknown_DCh = 0x7f800001; // 0x7f800001

        // reference data
        public string Name { get; set; }
        public Bounds Bound { get; set; }


        private string_r NameBlock = null;//used only when saving

        public FragPhysicsLOD Owner { get; set; } //required for correct bounds BVH generation


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.NamePointer = reader.ReadUInt64();
            this.BoundPointer = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt32();
            this.Unknown_2Ch = reader.ReadUInt32();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt64();
            this.Mass = reader.ReadSingle();
            this.MassInv = reader.ReadSingle();
            this.Unknown_48h = reader.ReadSingle();
            this.Unknown_4Ch = reader.ReadSingle();
            this.Unknown_50h = reader.ReadSingle();
            this.Unknown_54h = reader.ReadSingle();
            this.Unknown_58h = reader.ReadUInt64();
            this.InertiaTensor = reader.ReadVector3();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.InertiaTensorInv = reader.ReadVector3();
            this.Unknown_7Ch = reader.ReadUInt32();
            this.Unknown_80h = reader.ReadVector3();
            this.Unknown_8Ch = reader.ReadUInt32();
            this.Unknown_90h = reader.ReadVector3();
            this.Unknown_9Ch = reader.ReadUInt32();
            this.Unknown_A0h = reader.ReadVector3();
            this.Unknown_ACh = reader.ReadUInt32();
            this.Unknown_B0h = reader.ReadVector3();
            this.Unknown_BCh = reader.ReadUInt32();
            this.Unknown_C0h = reader.ReadVector3();
            this.Unknown_CCh = reader.ReadUInt32();
            this.Unknown_D0h = reader.ReadVector3();
            this.Unknown_DCh = reader.ReadUInt32();

            // read reference data
            this.Name = reader.ReadStringAt(this.NamePointer);
            this.Bound = reader.ReadBlockAt<Bounds>(this.BoundPointer);

            if (Bound != null)
            {
                Bound.Owner = this;
            }

            //if (Bound is BoundComposite bcmp)
            //{
            //    if ((bcmp.ChildrenFlags1 != null) || (bcmp.ChildrenFlags2 != null))
            //    { }//no hit
            //}

            //switch (VFT)
            //{
            //    case 0x4062a988:
            //    case 0x4062c4b8:
            //    case 0x4062c988:
            //    case 0x40593978:
            //    case 0x4061b548:
            //    case 0x4061ad68:
            //    case 0x4061e1a8:
            //    case 0x4061b9c8:
            //    case 0x4062c9a8:
            //    case 0x40620c18:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_04h != 1)
            //{ }//no hit
            //if (Unknown_08h != 0)
            //{ }//no hit
            //if (Unknown_10h != 2)
            //{ }//no hit
            //if (Unknown_28h != 1)
            //{ }//no hit
            //if (Unknown_2Ch != 0xFFFFFFFF)
            //{ }//no hit
            //if (Unknown_30h != 0x00010000)
            //{ }//no hit
            //if (Unknown_34h != 0)
            //{ }//no hit
            //if (Unknown_38h != 0)
            //{ }//no hit
            ////if (Unknown_40h != 97.0000153f)
            ////{ }//hits
            ////if (Unknown_44h != 0.0103092771f)
            ////{ }//hits
            //if (Unknown_48h != 1.0f)
            //{ }//no hit
            //if (Unknown_4Ch != 150.0f)
            //{ }//no hit
            //if (Unknown_50h != 6.2831855f)
            //{ }//no hit
            //if (Unknown_54h != 1.0f)
            //{ }//no hit
            //if (Unknown_58h != 0)
            //{ }//no hit
            //if (Unknown_80h != Vector3.Zero)
            //{ }//no hit
            //if (Unknown_90h != Vector3.Zero)
            //{ }//no hit
            //if (Unknown_A0h != Vector3.Zero)
            //{ }//no hit
            //if (Unknown_B0h != Vector3.Zero)
            //{ }//no hit
            //if (Unknown_C0h != Vector3.Zero)
            //{ }//no hit
            //if (Unknown_D0h != Vector3.Zero)
            //{ }//no hit
            //if (Unknown_6Ch != 0x7fc00001)
            //{ }//no hit
            //if (Unknown_7Ch != 0x7fc00001)
            //{ }//no hit
            //if (Unknown_8Ch != 0x7f800001)
            //{ }//no hit
            //if (Unknown_9Ch != 0x7f800001)
            //{ }//no hit
            //if (Unknown_ACh != 0x7f800001)
            //{ }//no hit
            //if (Unknown_BCh != 0x7f800001)
            //{ }//no hit
            //if (Unknown_CCh != 0x7f800001)
            //{ }//no hit
            //if (Unknown_DCh != 0x7f800001)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.NameBlock != null ? this.NameBlock.FilePosition : 0);
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.NamePointer);
            writer.Write(this.BoundPointer);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Mass);
            writer.Write(this.MassInv);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.InertiaTensor);
            writer.Write(this.Unknown_6Ch);
            writer.Write(this.InertiaTensorInv);
            writer.Write(this.Unknown_7Ch);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_8Ch);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_9Ch);
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_ACh);
            writer.Write(this.Unknown_B0h);
            writer.Write(this.Unknown_BCh);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_CCh);
            writer.Write(this.Unknown_D0h);
            writer.Write(this.Unknown_DCh);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YftXml.StringTag(sb, indent, "Name", YftXml.XmlEscape(Name));
            YftXml.ValueTag(sb, indent, "Mass", FloatUtil.ToString(Mass));
            YftXml.ValueTag(sb, indent, "MassInv", FloatUtil.ToString(MassInv));
            YftXml.ValueTag(sb, indent, "Unknown48", FloatUtil.ToString(Unknown_48h));
            YftXml.ValueTag(sb, indent, "Unknown4C", FloatUtil.ToString(Unknown_4Ch));
            YftXml.ValueTag(sb, indent, "Unknown50", FloatUtil.ToString(Unknown_50h));
            YftXml.ValueTag(sb, indent, "Unknown54", FloatUtil.ToString(Unknown_54h));
            YftXml.SelfClosingTag(sb, indent, "InertiaTensor " + FloatUtil.GetVector3XmlString(InertiaTensor));
            YftXml.SelfClosingTag(sb, indent, "InertiaTensorInv " + FloatUtil.GetVector3XmlString(InertiaTensorInv));
            if (Bound != null)
            {
                Bounds.WriteXmlNode(Bound, sb, indent);
            }
        }
        public void ReadXml(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            Mass = Xml.GetChildFloatAttribute(node, "Mass", "value");
            MassInv = Xml.GetChildFloatAttribute(node, "MassInv", "value");
            Unknown_48h = Xml.GetChildFloatAttribute(node, "Unknown48", "value");
            Unknown_4Ch = Xml.GetChildFloatAttribute(node, "Unknown4C", "value");
            Unknown_50h = Xml.GetChildFloatAttribute(node, "Unknown50", "value");
            Unknown_54h = Xml.GetChildFloatAttribute(node, "Unknown54", "value");
            InertiaTensor = Xml.GetChildVector3Attributes(node, "InertiaTensor");
            InertiaTensorInv = Xml.GetChildVector3Attributes(node, "InertiaTensorInv");
            var bnode = node.SelectSingleNode("Bounds");
            if (bnode != null)
            {
                Bound = Bounds.ReadXmlNode(bnode, this);
            }
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

        public override string ToString()
        {
            return Name ?? "(FragPhysArchetype: no name)";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysTypeChild : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 256; }
        }

        // structure data
        public uint VFT { get; set; } = 1080061712;
        public uint Unknown_04h = 1; // 0x00000001
        public float PristineMass { get; set; }
        public float DamagedMass { get; set; }
        public ushort GroupIndex { get; set; }
        public ushort BoneTag { get; set; }
        public uint Unknown_14h; // 0x00000000
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
        public ulong Unknown_68h; // 0x0000000000000000
        public ulong Unknown_70h; // 0x0000000000000000
        public ulong Unknown_78h; // 0x0000000000000000
        public ulong Unknown_80h; // 0x0000000000000000
        public ulong Unknown_88h; // 0x0000000000000000
        public ulong Unknown_90h; // 0x0000000000000000
        public ulong Unknown_98h; // 0x0000000000000000
        public ulong Drawable1Pointer { get; set; }
        public ulong Drawable2Pointer { get; set; }
        public ulong EvtSetPointer { get; set; }
        public ulong Unknown_B8h; // 0x0000000000000000
        public ulong Unknown_C0h; // 0x0000000000000000
        public ulong Unknown_C8h; // 0x0000000000000000
        public ulong Unknown_D0h; // 0x0000000000000000
        public ulong Unknown_D8h; // 0x0000000000000000
        public ulong Unknown_E0h; // 0x0000000000000000
        public ulong Unknown_E8h; // 0x0000000000000000
        public ulong Unknown_F0h; // 0x0000000000000000
        public ulong Unknown_F8h; // 0x0000000000000000

        // reference data
        public FragDrawable Drawable1 { get; set; }
        public FragDrawable Drawable2 { get; set; }
        public FragPhysEvtSet EvtSet { get; set; }


        public float UnkFloatFromParent { get; set; }//is this mass..?
        public Vector4 UnkVecFromParent { get; set; }
        public Vector4 InertiaTensorFromParent { get; set; }//is this really an inertia tensor?

        public FragPhysicsLOD OwnerFragPhysLod { get; set; }
        public int OwnerFragPhysIndex { get; set; }
        public FragPhysTypeGroup Group { get; set; }
        public string GroupName { get { return Group?.ToString(); } }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.PristineMass = reader.ReadSingle();
            this.DamagedMass = reader.ReadSingle();
            this.GroupIndex = reader.ReadUInt16();
            this.BoneTag = reader.ReadUInt16();
            this.Unknown_14h = reader.ReadUInt32();
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
            this.Unknown_68h = reader.ReadUInt64();
            this.Unknown_70h = reader.ReadUInt64();
            this.Unknown_78h = reader.ReadUInt64();
            this.Unknown_80h = reader.ReadUInt64();
            this.Unknown_88h = reader.ReadUInt64();
            this.Unknown_90h = reader.ReadUInt64();
            this.Unknown_98h = reader.ReadUInt64();
            this.Drawable1Pointer = reader.ReadUInt64();
            this.Drawable2Pointer = reader.ReadUInt64();
            this.EvtSetPointer = reader.ReadUInt64();
            this.Unknown_B8h = reader.ReadUInt64();
            this.Unknown_C0h = reader.ReadUInt64();
            this.Unknown_C8h = reader.ReadUInt64();
            this.Unknown_D0h = reader.ReadUInt64();
            this.Unknown_D8h = reader.ReadUInt64();
            this.Unknown_E0h = reader.ReadUInt64();
            this.Unknown_E8h = reader.ReadUInt64();
            this.Unknown_F0h = reader.ReadUInt64();
            this.Unknown_F8h = reader.ReadUInt64();

            // read reference data
            this.Drawable1 = reader.ReadBlockAt<FragDrawable>(this.Drawable1Pointer);
            this.Drawable2 = reader.ReadBlockAt<FragDrawable>(this.Drawable2Pointer);
            this.EvtSet = reader.ReadBlockAt<FragPhysEvtSet>(this.EvtSetPointer);

            if (this.Drawable1 != null)
            {
                this.Drawable1.OwnerFragmentPhys = this;
            }
            if (this.Drawable2 != null)
            {
                this.Drawable2.OwnerFragmentPhys = this;
            }


            //if (Unknown_04h != 1)
            //{ }//no hit
            ////if (Unknown_08h != 13.8f)
            ////{ }//hits..
            ////if ((Unknown_0Ch != -1.0f) && (Unknown_0Ch != 1.0f)) //1.85f, 4.0f, 3.58300781f ...
            ////{ }//hits..
            //if (Unknown_14h != 0)
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
            //if (Unknown_68h != 0)
            //{ }//no hit
            //if (Unknown_70h != 0)
            //{ }//no hit
            //if (Unknown_78h != 0)
            //{ }//no hit
            //if (Unknown_80h != 0)
            //{ }//no hit
            //if (Unknown_88h != 0)
            //{ }//no hit
            //if (Unknown_90h != 0)
            //{ }//no hit
            //if (Unknown_98h != 0)
            //{ }//no hit
            //if (Unknown_B8h != 0)
            //{ }//no hit
            //if (Unknown_C0h != 0)
            //{ }//no hit
            //if (Unknown_C8h != 0)
            //{ }//no hit
            //if (Unknown_D0h != 0)
            //{ }//no hit
            //if (Unknown_D8h != 0)
            //{ }//no hit
            //if (Unknown_E0h != 0)
            //{ }//no hit
            //if (Unknown_E8h != 0)
            //{ }//no hit
            //if (Unknown_F0h != 0)
            //{ }//no hit
            //if (Unknown_F8h != 0)
            //{ }//no hit
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
            writer.Write(this.PristineMass);
            writer.Write(this.DamagedMass);
            writer.Write(this.GroupIndex);
            writer.Write(this.BoneTag);
            writer.Write(this.Unknown_14h);
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
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_88h);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_98h);
            writer.Write(this.Drawable1Pointer);
            writer.Write(this.Drawable2Pointer);
            writer.Write(this.EvtSetPointer);
            writer.Write(this.Unknown_B8h);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C8h);
            writer.Write(this.Unknown_D0h);
            writer.Write(this.Unknown_D8h);
            writer.Write(this.Unknown_E0h);
            writer.Write(this.Unknown_E8h);
            writer.Write(this.Unknown_F0h);
            writer.Write(this.Unknown_F8h);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            YftXml.ValueTag(sb, indent, "GroupIndex", GroupIndex.ToString());
            YftXml.ValueTag(sb, indent, "BoneTag", BoneTag.ToString());
            YftXml.ValueTag(sb, indent, "PristineMass", FloatUtil.ToString(PristineMass));
            YftXml.ValueTag(sb, indent, "DamagedMass", FloatUtil.ToString(DamagedMass));
            YftXml.ValueTag(sb, indent, "UnkFloat", FloatUtil.ToString(UnkFloatFromParent));
            YftXml.SelfClosingTag(sb, indent, "UnkVec " + FloatUtil.GetVector4XmlString(UnkVecFromParent));
            YftXml.SelfClosingTag(sb, indent, "InertiaTensor " + FloatUtil.GetVector4XmlString(InertiaTensorFromParent));
            if (EvtSet != null)
            {
                YftXml.SelfClosingTag(sb, indent, "EventSet");
                //EvtSet.WriteXml(sb, indent);//nothing to write..
            }
            if (Drawable1 != null)
            {
                FragDrawable.WriteXmlNode(Drawable1, sb, indent, ddsfolder, "Drawable");
            }
            if (Drawable2 != null)
            {
                FragDrawable.WriteXmlNode(Drawable2, sb, indent, ddsfolder, "Drawable2");
            }
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            GroupIndex = (ushort)Xml.GetChildUIntAttribute(node, "GroupIndex", "value");
            BoneTag = (ushort)Xml.GetChildUIntAttribute(node, "BoneTag", "value");
            PristineMass = Xml.GetChildFloatAttribute(node, "PristineMass", "value");
            DamagedMass = Xml.GetChildFloatAttribute(node, "DamagedMass", "value");
            UnkFloatFromParent = Xml.GetChildFloatAttribute(node, "UnkFloat", "value");
            UnkVecFromParent = Xml.GetChildVector4Attributes(node, "UnkVec");
            InertiaTensorFromParent = Xml.GetChildVector4Attributes(node, "InertiaTensor");
            var esnode = node.SelectSingleNode("EventSet");
            if (esnode != null)
            {
                EvtSet = new FragPhysEvtSet();
                //EvtSet.ReadXml(esnode);//nothing to read...
            }
            var dnode = node.SelectSingleNode("Drawable");
            if (dnode != null)
            {
                Drawable1 = FragDrawable.ReadXmlNode(dnode, ddsfolder);
            }
            var dnode2 = node.SelectSingleNode("Drawable2");
            if (dnode2 != null)
            {
                Drawable2 = FragDrawable.ReadXmlNode(dnode2, ddsfolder);
            }
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
            return GroupName ?? "(FragPhysTypeChild: no group)";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysEvtSet : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 48; }
        }

        // structure data
        public uint VFT { get; set; } = 1080060072;
        public uint Unknown_04h = 1; // 0x00000001
        public ulong Unknown_08h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_04h = reader.ReadUInt32();
            this.Unknown_08h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();

            //if ((VFT != 0x406048a8) && (VFT != 0x406068a8) && (VFT != 0x40606888) && (VFT != 0x405f8698) && (VFT != 0x405f6138) && (VFT != 0x406068c8))
            //{ }//no hit
            //if (Unknown_04h != 1)
            //{ }//no hit
            //if (Unknown_08h != 0)
            //{ }//no hit
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
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_04h);
            writer.Write(this.Unknown_08h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            //nothing to write...
        }
        public void ReadXml(XmlNode node)
        {
            //nothing to read...
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysTypeGroup : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength => 176;

        // structure data
        public float Unknown_00h; // 0x00000000
        public float Unknown_04h; // 0x00000000
        public float Unknown_08h; // 0x00000000
        public float Unknown_0Ch; // 0x00000000
        public float Strength { get; set; }
        public float ForceTransmissionScaleUp { get; set; }
        public float ForceTransmissionScaleDown { get; set; }
        public float JointStiffness { get; set; }
        public float MinSoftAngle1 { get; set; }
        public float MaxSoftAngle1 { get; set; }
        public float MaxSoftAngle2 { get; set; }
        public float MaxSoftAngle3 { get; set; }
        public float RotationSpeed { get; set; }
        public float RotationStrength { get; set; }
        public float RestoringStrength { get; set; }
        public float RestoringMaxTorque { get; set; }
        public float LatchStrength { get; set; }
        public float Mass { get; set; }
        public float Unknown_48h; // 0x00000000
        public byte ChildGroupIndex { get; set; } //index of the first child group of this group, 255 if no children - calc on XML import! (from ParentIndex)
        public byte ParentIndex { get; set; } //index of the parent group.
        public byte ChildIndex { get; set; } //index of first BoundComposite child, AND fragment child - calc on XML import! (from FragPhysTypeChild.GroupIndex)
        public byte ChildCount { get; set; } //number of BoundComposite children, AND fragment children - calc on XML import! (from FragPhysTypeChild.GroupIndex)
        public byte ChildGroupCount { get; set; } //number of groups with this as the direct parent - calc on XML import! (from ParentIndex)
        public byte UnkByte51 { get; set; } = 255; //0xFF  (always)
        public byte GlassWindowIndex { get; set; }//GlassWindows index
        public byte GlassFlags { get; set; }//flags: 1=?, 2=glass, 4=?, ...
        public float MinDamageForce { get; set; }
        public float DamageHealth { get; set; }
        public float UnkFloat5C { get; set; }
        public float UnkFloat60 { get; set; }
        public float UnkFloat64 { get; set; }
        public float UnkFloat68 { get; set; }
        public float UnkFloat6C { get; set; }
        public float UnkFloat70 { get; set; }
        public float UnkFloat74 { get; set; }
        public float UnkFloat78 { get; set; }
        public float Unknown_7Ch; // 0x00000000
        public FragPhysNameStruct_s Name { get; set; }
        public float UnkFloatA8 { get; set; }
        public float Unknown_ACh; // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            this.Unknown_00h = reader.ReadSingle();
            this.Unknown_04h = reader.ReadSingle();
            this.Unknown_08h = reader.ReadSingle();
            this.Unknown_0Ch = reader.ReadSingle();
            this.Strength = reader.ReadSingle();
            this.ForceTransmissionScaleUp = reader.ReadSingle();
            this.ForceTransmissionScaleDown = reader.ReadSingle();
            this.JointStiffness = reader.ReadSingle();
            this.MinSoftAngle1 = reader.ReadSingle();
            this.MaxSoftAngle1 = reader.ReadSingle();
            this.MaxSoftAngle2 = reader.ReadSingle();
            this.MaxSoftAngle3 = reader.ReadSingle();
            this.RotationSpeed = reader.ReadSingle();
            this.RotationStrength = reader.ReadSingle();
            this.RestoringStrength = reader.ReadSingle();
            this.RestoringMaxTorque = reader.ReadSingle();
            this.LatchStrength = reader.ReadSingle();
            this.Mass = reader.ReadSingle();
            this.Unknown_48h = reader.ReadSingle();
            this.ChildGroupIndex = reader.ReadByte();
            this.ParentIndex = reader.ReadByte();
            this.ChildIndex = reader.ReadByte();
            this.ChildCount = reader.ReadByte();
            this.ChildGroupCount = reader.ReadByte();
            this.UnkByte51 = reader.ReadByte();
            this.GlassWindowIndex = reader.ReadByte();
            this.GlassFlags = reader.ReadByte();
            this.MinDamageForce = reader.ReadSingle();
            this.DamageHealth = reader.ReadSingle();
            this.UnkFloat5C = reader.ReadSingle();
            this.UnkFloat60 = reader.ReadSingle();
            this.UnkFloat64 = reader.ReadSingle();
            this.UnkFloat68 = reader.ReadSingle();
            this.UnkFloat6C = reader.ReadSingle();
            this.UnkFloat70 = reader.ReadSingle();
            this.UnkFloat74 = reader.ReadSingle();
            this.UnkFloat78 = reader.ReadSingle();
            this.Unknown_7Ch = reader.ReadSingle();
            this.Name = reader.ReadStruct<FragPhysNameStruct_s>();
            this.UnkFloatA8 = reader.ReadSingle();
            this.Unknown_ACh = reader.ReadSingle();

            //if (Unknown_00h != 0)
            //{ }//no hit
            //if (Unknown_04h != 0)
            //{ }//no hit
            //if (Unknown_08h != 0)
            //{ }//no hit
            //if (Unknown_0Ch != 0)
            //{ }//no hit
            //if (Unknown_48h != 0)
            //{ }//no hit
            //if (UnkByte51 != 255) 
            //{ }//no hit
            //if (Unknown_7Ch != 0)
            //{ }//no hit
            //if (Unknown_ACh != 0)
            //{ }//no hit


            //if ((UnkFloat10 != 0.0f) && (UnkFloat10 != -1.0f) && (UnkFloat10 != 100.0f))
            //{ }//hit
            //if ((UnkFloat14 != 0.2f) && (UnkFloat14 != 0.25f) && (UnkFloat14 != 0.0f) && (UnkFloat14 != 1.1f))
            //{ }//hit
            //if ((UnkFloat18 != 0.2f) && (UnkFloat18 != 0.25f) && (UnkFloat18 != 0.0f) && (UnkFloat18 != 1.1f))
            //{ }//hit
            //if ((UnkFloat1C != 0) && (UnkFloat1C != 0.5f) && (UnkFloat1C != 0.6f) && (UnkFloat1C != 0.2f) && (UnkFloat1C != 0.1f))
            //{ }//no hit
            //if ((UnkFloat20 != 1.0f) && (UnkFloat20 != -1.0f) && (UnkFloat20 != 0.0f))
            //{ }//no hit
            //if ((UnkFloat24 != -1.0f) && (UnkFloat24 != 1.0f) && (UnkFloat24 != 0.0f))
            //{ }//no hit
            //if ((UnkFloat28 != 1.0f) && (UnkFloat28 != 0.0f))
            //{ }//no hit
            //if ((UnkFloat2C != 1.0f) && (UnkFloat2C != 0.0f))
            //{ }//no hit
            //if ((UnkFloat30 != 0) && (UnkFloat30 != 20))
            //{ }//no hit
            //if ((UnkFloat34 != 0) && (UnkFloat34 != 30.0f) && (UnkFloat34 != 15.0f))
            //{ }//no hit
            //if ((UnkFloat38 != 0) && (UnkFloat38 != 500.0f) && (UnkFloat38 != 200.0f) && (UnkFloat38 != 13.0f) && (UnkFloat38 != 400.0f))
            //{ }//hit
            //if ((UnkFloat3C != 0) && (UnkFloat3C != 133.0f) && (UnkFloat3C != 1.0f) && (UnkFloat3C != 300.0f) && (UnkFloat3C != 10.0f))
            //{ }//no hit
            //if ((UnkFloat40 != 0) && (UnkFloat40 != -1.0f) && (UnkFloat40 != 1.0f) && (UnkFloat40 != 100.0f) && (UnkFloat40 != 1000.0f) && (UnkFloat40 != 1500.0f))
            //{ }//hit
            //if (UnkFloat44 != 13.8f) //9.5f, 5.0f, 2.0f, 11.4832773 ...
            //{ }//hit
            //if ((UnkByte4C != 255) && (UnkByte4C != 1) && (UnkByte4C != 2))
            //{ }//hit
            //if ((UnkByte4F != 1) && (UnkByte4F != 2) && (UnkByte4F != 5) && (UnkByte4F != 7) && (UnkByte4F != 3) && (UnkByte4F != 4))
            //{ }//hit
            //if ((UnkByte50 != 0) && (UnkByte50 != 1) && (UnkByte50 != 2) && (UnkByte50 != 3))
            //{ }//hit
            //if ((UnkByte52 != 0) && (UnkByte52 != 1) && (UnkByte52 != 2) && (UnkByte52 != 3))
            //{ }//hit
            //if ((UnkByte53 != 0) && (UnkByte53 != 1) && (UnkByte53 != 5) && (UnkByte53 != 2))
            //{ }//hit
            //if ((UnkFloat54 != 0) && (UnkFloat54 != 10.0f) && (UnkFloat54 != 100.0f))
            //{ }//hit
            //if ((UnkFloat58 != 0) && (UnkFloat58 != 10.0f) && (UnkFloat58 != 100.0f) && (UnkFloat58 != 200.0f) && (UnkFloat58 != 1000.0f))
            //{ }//hit
            //if ((UnkFloat5C != 0) && (UnkFloat5C != 100.0f) && (UnkFloat5C != 900.0f) && (UnkFloat5C != 300.0f) && (UnkFloat5C != 500.0f))
            //{ }//hit
            //if ((UnkFloat60 != 1.0f) && (UnkFloat60 != 0.0f) && (UnkFloat60 != 0.6f) && (UnkFloat60 != 0.25f) && (UnkFloat60 != 0.5f))
            //{ }//hit
            //if ((UnkFloat64 != 1.0f) && (UnkFloat64 != 0.0f) && (UnkFloat64 != 3.0f) && (UnkFloat64 != 0.96f))
            //{ }//hit
            //if ((UnkFloat68 != 1.0f) && (UnkFloat68 != 0.0f) && (UnkFloat68 != 0.01f) && (UnkFloat68 != 1.04f))
            //{ }//hit
            //if ((UnkFloat6C != 1.0f) && (UnkFloat6C != 0.0f) && (UnkFloat6C != 0.01f) && (UnkFloat6C != 0.67f) && (UnkFloat6C != 0.2f))
            //{ }//hit
            //if ((UnkFloat70 != 1.0f) && (UnkFloat70 != 0.0f) && (UnkFloat70 != 0.5f) && (UnkFloat70 != 3.0f) && (UnkFloat70 != 1.5f))
            //{ }//hit
            //if ((UnkFloat74 != 1.0f) && (UnkFloat74 != 0.0f) && (UnkFloat74 != 10.0f) && (UnkFloat74 != 1.82f))
            //{ }//hit
            //if ((UnkFloat78 != 1.0f) && (UnkFloat78 != 0.0f) && (UnkFloat78 != 0.2f) && (UnkFloat78 != 0.25f))
            //{ }//hit
            //if ((UnkFloatA8 != 1.0f) && (UnkFloatA8 != 0.0f) && (UnkFloatA8 != 0.6f) && (UnkFloatA8 != 0.5f) && (UnkFloatA8 != 0.1f))
            //{ }//hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            writer.Write(Unknown_00h);
            writer.Write(Unknown_04h);
            writer.Write(Unknown_08h);
            writer.Write(Unknown_0Ch);
            writer.Write(Strength);
            writer.Write(ForceTransmissionScaleUp);
            writer.Write(ForceTransmissionScaleDown);
            writer.Write(JointStiffness);
            writer.Write(MinSoftAngle1);
            writer.Write(MaxSoftAngle1);
            writer.Write(MaxSoftAngle2);
            writer.Write(MaxSoftAngle3);
            writer.Write(RotationSpeed);
            writer.Write(RotationStrength);
            writer.Write(RestoringStrength);
            writer.Write(RestoringMaxTorque);
            writer.Write(LatchStrength);
            writer.Write(Mass);
            writer.Write(Unknown_48h);
            writer.Write(ChildGroupIndex);
            writer.Write(ParentIndex);
            writer.Write(ChildIndex);
            writer.Write(ChildCount);
            writer.Write(ChildGroupCount);
            writer.Write(UnkByte51);
            writer.Write(GlassWindowIndex);
            writer.Write(GlassFlags);
            writer.Write(MinDamageForce);
            writer.Write(DamageHealth);
            writer.Write(UnkFloat5C);
            writer.Write(UnkFloat60);
            writer.Write(UnkFloat64);
            writer.Write(UnkFloat68);
            writer.Write(UnkFloat6C);
            writer.Write(UnkFloat70);
            writer.Write(UnkFloat74);
            writer.Write(UnkFloat78);
            writer.Write(Unknown_7Ch);
            writer.WriteStruct(Name);
            writer.Write(UnkFloatA8);
            writer.Write(Unknown_ACh);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YftXml.StringTag(sb, indent, "Name", YftXml.XmlEscape(Name.ToString()));
            YftXml.ValueTag(sb, indent, "ParentIndex", ParentIndex.ToString());
            YftXml.ValueTag(sb, indent, "GlassWindowIndex", GlassWindowIndex.ToString());
            YftXml.ValueTag(sb, indent, "GlassFlags", GlassFlags.ToString());
            YftXml.ValueTag(sb, indent, "Strength", FloatUtil.ToString(Strength));
            YftXml.ValueTag(sb, indent, "ForceTransmissionScaleUp", FloatUtil.ToString(ForceTransmissionScaleUp));
            YftXml.ValueTag(sb, indent, "ForceTransmissionScaleDown", FloatUtil.ToString(ForceTransmissionScaleDown));
            YftXml.ValueTag(sb, indent, "JointStiffness", FloatUtil.ToString(JointStiffness));
            YftXml.ValueTag(sb, indent, "MinSoftAngle1", FloatUtil.ToString(MinSoftAngle1));
            YftXml.ValueTag(sb, indent, "MaxSoftAngle1", FloatUtil.ToString(MaxSoftAngle1));
            YftXml.ValueTag(sb, indent, "MaxSoftAngle2", FloatUtil.ToString(MaxSoftAngle2));
            YftXml.ValueTag(sb, indent, "MaxSoftAngle3", FloatUtil.ToString(MaxSoftAngle3));
            YftXml.ValueTag(sb, indent, "RotationSpeed", FloatUtil.ToString(RotationSpeed));
            YftXml.ValueTag(sb, indent, "RotationStrength", FloatUtil.ToString(RotationStrength));
            YftXml.ValueTag(sb, indent, "RestoringStrength", FloatUtil.ToString(RestoringStrength));
            YftXml.ValueTag(sb, indent, "RestoringMaxTorque", FloatUtil.ToString(RestoringMaxTorque));
            YftXml.ValueTag(sb, indent, "LatchStrength", FloatUtil.ToString(LatchStrength));
            YftXml.ValueTag(sb, indent, "Mass", FloatUtil.ToString(Mass));
            YftXml.ValueTag(sb, indent, "MinDamageForce", FloatUtil.ToString(MinDamageForce));
            YftXml.ValueTag(sb, indent, "DamageHealth", FloatUtil.ToString(DamageHealth));
            YftXml.ValueTag(sb, indent, "UnkFloat5C", FloatUtil.ToString(UnkFloat5C));
            YftXml.ValueTag(sb, indent, "UnkFloat60", FloatUtil.ToString(UnkFloat60));
            YftXml.ValueTag(sb, indent, "UnkFloat64", FloatUtil.ToString(UnkFloat64));
            YftXml.ValueTag(sb, indent, "UnkFloat68", FloatUtil.ToString(UnkFloat68));
            YftXml.ValueTag(sb, indent, "UnkFloat6C", FloatUtil.ToString(UnkFloat6C));
            YftXml.ValueTag(sb, indent, "UnkFloat70", FloatUtil.ToString(UnkFloat70));
            YftXml.ValueTag(sb, indent, "UnkFloat74", FloatUtil.ToString(UnkFloat74));
            YftXml.ValueTag(sb, indent, "UnkFloat78", FloatUtil.ToString(UnkFloat78));
            YftXml.ValueTag(sb, indent, "UnkFloatA8", FloatUtil.ToString(UnkFloatA8));
        }
        public void ReadXml(XmlNode node)
        {
            Name = new FragPhysNameStruct_s(Xml.GetChildInnerText(node, "Name"));
            ParentIndex = (byte)Xml.GetChildUIntAttribute(node, "ParentIndex", "value");
            GlassWindowIndex = (byte)Xml.GetChildUIntAttribute(node, "GlassWindowIndex", "value");
            GlassFlags = (byte)Xml.GetChildUIntAttribute(node, "GlassFlags", "value");
            Strength = Xml.GetChildFloatAttribute(node, "Strength", "value");
            ForceTransmissionScaleUp = Xml.GetChildFloatAttribute(node, "ForceTransmissionScaleUp", "value");
            ForceTransmissionScaleDown = Xml.GetChildFloatAttribute(node, "ForceTransmissionScaleDown", "value");
            JointStiffness = Xml.GetChildFloatAttribute(node, "JointStiffness", "value");
            MinSoftAngle1 = Xml.GetChildFloatAttribute(node, "MinSoftAngle1", "value");
            MaxSoftAngle1 = Xml.GetChildFloatAttribute(node, "MaxSoftAngle1", "value");
            MaxSoftAngle2 = Xml.GetChildFloatAttribute(node, "MaxSoftAngle2", "value");
            MaxSoftAngle3 = Xml.GetChildFloatAttribute(node, "MaxSoftAngle3", "value");
            RotationSpeed = Xml.GetChildFloatAttribute(node, "RotationSpeed", "value");
            RotationStrength = Xml.GetChildFloatAttribute(node, "RotationStrength", "value");
            RestoringStrength = Xml.GetChildFloatAttribute(node, "RestoringStrength", "value");
            RestoringMaxTorque = Xml.GetChildFloatAttribute(node, "RestoringMaxTorque", "value");
            LatchStrength = Xml.GetChildFloatAttribute(node, "LatchStrength", "value");
            Mass = Xml.GetChildFloatAttribute(node, "Mass", "value");
            MinDamageForce = Xml.GetChildFloatAttribute(node, "MinDamageForce", "value");
            DamageHealth = Xml.GetChildFloatAttribute(node, "DamageHealth", "value");
            UnkFloat5C = Xml.GetChildFloatAttribute(node, "UnkFloat5C", "value");
            UnkFloat60 = Xml.GetChildFloatAttribute(node, "UnkFloat60", "value");
            UnkFloat64 = Xml.GetChildFloatAttribute(node, "UnkFloat64", "value");
            UnkFloat68 = Xml.GetChildFloatAttribute(node, "UnkFloat68", "value");
            UnkFloat6C = Xml.GetChildFloatAttribute(node, "UnkFloat6C", "value");
            UnkFloat70 = Xml.GetChildFloatAttribute(node, "UnkFloat70", "value");
            UnkFloat74 = Xml.GetChildFloatAttribute(node, "UnkFloat74", "value");
            UnkFloat78 = Xml.GetChildFloatAttribute(node, "UnkFloat78", "value");
            UnkFloatA8 = Xml.GetChildFloatAttribute(node, "UnkFloatA8", "value");
        }

        public override string ToString()
        {
            return Name.ToString();
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FragPhysGroupNamesBlock : ResourceSystemBlock
    {

        public override long BlockLength
        {
            get { return (data_items?.Length ?? 0) * 8 + 8; }
        }


        public ulong[] data_pointers { get; set; }
        public FragPhysNameStruct_s[] data_items { get; set; }

        public uint UnkVFT { get; set; } = 1095046985;
        public uint UnkUint1 { get; set; } = 1;


        public FragPhysTypeGroup[] Groups;//for writing purposes



        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            int numElements = Convert.ToInt32(parameters[0]);
            Groups = parameters[1] as FragPhysTypeGroup[];

            data_pointers = new ulong[numElements];
            for (int i = 0; i < numElements; i++)
            {
                data_pointers[i] = reader.ReadUInt64();
            }

            UnkVFT = reader.ReadUInt32();
            UnkUint1 = reader.ReadUInt32();

            //switch (UnkVFT)
            //{
            //    case 1095030569:
            //    case 1080035264:
            //    case 1080043424:
            //    case 1080043456:
            //    case 1079473328:
            //    case 1079970608:
            //    case 1079970704:
            //    case 1079985584:
            //    case 1079976016:
            //    case 1080043488:
            //    case 1079992976:
            //    case 1095046985:
            //    case 1095046905:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (UnkUint1)
            //{
            //    case 1:
            //        break;
            //    default:
            //        break;//no hit
            //}

            data_items = new FragPhysNameStruct_s[numElements];
            for (int i = 0; i < numElements; i++)
            {
                data_items[i] = reader.ReadStructAt<FragPhysNameStruct_s>((long)data_pointers[i]);
            }

        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {


            var gnplist = new List<ulong>();
            foreach (var grp in Groups)
            {
                gnplist.Add((ulong)grp.FilePosition + 128);//manually write group names pointers as offsets to the groups
            }
            data_pointers = gnplist.ToArray();



            foreach (var x in data_pointers)
            {
                writer.Write(x);
            }

            writer.Write(UnkVFT);
            writer.Write(UnkUint1);

        }




        public override string ToString()
        {
            return "(Count: " + ((data_items != null) ? data_items.Length : 0).ToString() + ")";
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
        public uint Unknown_20h { get; set; }
        public uint Unknown_24h { get; set; }


        public FragPhysNameStruct_s(string s)
        {
            s = s.PadRight(40, '\0');
            uint u(int i) => ((s[i]&0xFFu) + ((s[i+1]&0xFFu)<<8) + ((s[i+2]&0xFFu)<<16) + ((s[i+3]&0xFFu)<<24));
            Unknown_00h = u(0);
            Unknown_04h = u(4);
            Unknown_08h = u(8);
            Unknown_0Ch = u(12);
            Unknown_10h = u(16);
            Unknown_14h = u(20);
            Unknown_18h = u(24);
            Unknown_1Ch = u(28);
            Unknown_20h = u(32);
            Unknown_24h = u(36);
        }

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
            usb.Add(Unknown_20h);
            usb.Add(Unknown_24h);
            return usb.ToString().Replace("\0", "");
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