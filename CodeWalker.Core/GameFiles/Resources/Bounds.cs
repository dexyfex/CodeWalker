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
using System.Runtime.InteropServices;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using CodeWalker.World;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{


    [TC(typeof(EXP))] public class BoundsDictionary : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000001
        public uint Unknown_14h { get; set; } // 0x00000001
        public uint Unknown_18h { get; set; } // 0x00000001
        public uint Unknown_1Ch { get; set; } // 0x00000001
        public ResourceSimpleList64_uint BoundNameHashes;
        public ResourcePointerList64<Bounds> Bounds { get; set; }

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
            this.BoundNameHashes = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Bounds = reader.ReadBlock<ResourcePointerList64<Bounds>>();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteBlock(this.BoundNameHashes);
            writer.WriteBlock(this.Bounds);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, BoundNameHashes),
                new Tuple<long, IResourceBlock>(0x30, Bounds)
            };
        }
    }

    public enum BoundsType : byte
    {
        None = 255, //not contained in files, but used as a placeholder in XML conversion
        Sphere = 0,
        Capsule = 1,
        Box = 3,
        Geometry = 4,
        GeometryBVH = 8,
        Composite = 10,
        Disc = 12,
        Cylinder = 13,
        Cloth = 15,
    }

    [TC(typeof(EXP))] public class Bounds : ResourceFileBase, IResourceXXSystemBlock
    {
        public override long BlockLength
        {
            get { return 112; }
        }

        // structure data
        public BoundsType Type { get; set; }
        public byte Unknown_11h { get; set; } // 0x00000000
        public ushort Unknown_12h { get; set; } // 0x00000000
        public float SphereRadius { get; set; }
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public Vector3 BoxMax { get; set; }
        public float Margin { get; set; }
        public Vector3 BoxMin { get; set; }
        public uint Unknown_3Ch { get; set; } = 1; //1, 2 (yft only)
        public Vector3 BoxCenter { get; set; }
        public byte MaterialIndex { get; set; }
        public byte ProceduralId { get; set; }
        public byte RoomId_and_PedDensity { get; set; } //5bits for RoomID and then 3bits for PedDensity
        public byte UnkFlags { get; set; } //    (bit5 related to PolyFlags, should be a flag called "Has PolyFlags")[check this?]
        public Vector3 SphereCenter { get; set; }
        public byte PolyFlags { get; set; }
        public byte MaterialColorIndex { get; set; }
        public ushort Unknown_5Eh { get; set; } // 0x00000000
        public Vector3 Unknown_60h { get; set; }
        public float Volume { get; set; }

        public byte RoomId
        {
            get
            {
                return (byte)(RoomId_and_PedDensity & 0x1F);
            }
            set
            {
                RoomId_and_PedDensity = (byte)((RoomId_and_PedDensity & 0xE0) + (value & 0x1F));
            }
        }
        public byte PedDensity
        {
            get
            {
                return (byte)(RoomId_and_PedDensity >> 5);
            }
            set
            {
                RoomId_and_PedDensity = (byte)((RoomId_and_PedDensity & 0x1F) + ((value & 0x7) << 5));
            }
        }

        public bool HasChanged { get; set; } = false;
        public BoundComposite Parent { get; set; }
        public YbnFile OwnerYbn { get; set; }
        public object Owner { get; set; }
        public string OwnerName { get; set; }
        public bool OwnerIsFragment
        {
            get
            {
                return ((Owner is FragPhysicsLOD) || (Owner is FragPhysArchetype) || (Owner is FragDrawable));
            }
        }
        public string GetName()
        {
            string n = OwnerName;
            var p = Parent;
            while (p != null)
            {
                n = p.OwnerName;
                p = p.Parent;
            }
            return n;
        }
        public string GetTitle()
        {
            var n = GetName();
            var t = Type.ToString();
            return t + ": " + n;
        }
        public YbnFile GetRootYbn()
        {
            var r = OwnerYbn;
            var p = Parent;
            while ((p != null) && (r == null))
            {
                r = p.OwnerYbn;
                p = p.Parent;
            }
            return r;
        }
        public object GetRootOwner()
        {
            var r = Owner;
            var p = Parent;
            while ((p != null) && (r == null))
            {
                r = p.Owner;
                p = p.Parent;
            }
            return r;
        }

        public Matrix Transform { get; set; } = Matrix.Identity; //when it's the child of a bound composite
        public Matrix TransformInv { get; set; } = Matrix.Identity;

        public BoundCompositeChildrenFlags CompositeFlags1 { get; set; }
        public BoundCompositeChildrenFlags CompositeFlags2 { get; set; }


        public virtual Vector3 Scale
        {
            get
            {
                return Transform.ScaleVector;
            }
            set
            {
                var m = Transform;
                m.ScaleVector = value;
                Transform = m;
                TransformInv = Matrix.Invert(m);
            }
        }
        public virtual Vector3 Position
        {
            get
            {
                return Transform.TranslationVector;
            }
            set
            {
                var m = Transform;
                m.TranslationVector = value;
                Transform = m;
                TransformInv = Matrix.Invert(m);
            }
        }
        public virtual Quaternion Orientation
        {
            get
            {
                return Transform.ToQuaternion();
            }
            set
            {
                var m = value.ToMatrix();
                m.TranslationVector = Transform.TranslationVector;
                m.ScaleVector = Transform.ScaleVector;
                Transform = m;
                TransformInv = Matrix.Invert(m);
            }
        }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Type = (BoundsType)reader.ReadByte();
            this.Unknown_11h = reader.ReadByte();
            this.Unknown_12h = reader.ReadUInt16();
            this.SphereRadius = reader.ReadSingle();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.BoxMax = reader.ReadVector3();
            this.Margin = reader.ReadSingle();
            this.BoxMin = reader.ReadVector3();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.BoxCenter = reader.ReadVector3();
            this.MaterialIndex = reader.ReadByte();
            this.ProceduralId = reader.ReadByte();
            this.RoomId_and_PedDensity = reader.ReadByte();
            this.UnkFlags = reader.ReadByte();
            this.SphereCenter = reader.ReadVector3();
            this.PolyFlags = reader.ReadByte();
            this.MaterialColorIndex = reader.ReadByte();
            this.Unknown_5Eh = reader.ReadUInt16();
            this.Unknown_60h = reader.ReadVector3();
            this.Volume = reader.ReadSingle();

            if (Unknown_11h != 0)
            { }
            if (Unknown_12h != 0)
            { }
            if (Unknown_18h != 0)
            { }
            if (Unknown_1Ch != 0)
            { }
            if (Unknown_5Eh != 0)
            { }


            switch (Unknown_3Ch)
            {
                case 1:
                case 2: // only found in .yft
                case 0: //only found in .ypt
                    break;
                default:
                    break;
            }
            switch (UnkFlags)//yeah it's probably flags
            {
                case 0://for all .ybn files
                case 26: //v_corp_banktrolley.ydr
                case 18: //v_corp_banktrolley.ydr
                case 16: //v_corp_banktrolley.ydr
                case 2:  //v_corp_bk_bust.ydr
                case 10: //v_corp_bk_bust.ydr
                case 130://v_corp_bk_chair2.ydr
                case 30: //v_corp_bk_flag.ydr
                case 144://v_corp_bombbin.ydr
                case 8:  //v_corp_conftable2.ydr
                case 12: //v_corp_conftable3.ydr
                case 4:  //v_corp_cubiclefd.ydr
                case 22: //v_corp_hicksdoor.ydr
                case 150://v_corp_hicksdoor.ydr
                case 128://v_corp_officedesk003.ydr
                case 24: //v_corp_potplant1.ydr
                case 14: //v_ind_cm_aircomp.ydr
                case 146://v_ind_rc_rubbish.ydr
                case 134://v_ilev_bk_door.ydr
                case 64: //v_ilev_carmod3lamp.ydr
                case 28: //v_ilev_cbankvaulgate02.ydr
                case 132://v_ilev_ch_glassdoor.ydr
                case 6:  //v_ilev_cs_door01.ydr
                case 20: //v_ilev_fib_atrgl1s.ydr
                case 94: //v_ilev_uvcheetah.ydr
                case 148://v_serv_metro_elecpole_singlel.ydr
                case 48: //v_serv_metro_statseat1.ydr
                case 50: //v_serv_securitycam_03.ydr
                case 80: //prop_bmu_02_ld.ydr
                case 92: //prop_bmu_02_ld.ydr
                case 82: //prop_roofvent_08a.ydr
                case 1:  //prop_portasteps_02.ydr
                case 65: //prop_storagetank_01_cr.ydr
                case 90: //prop_storagetank_01_cr.ydr
                case 68: //prop_sub_frame_01a.ydr
                case 66: //prop_ld_cable.ydr
                case 78: //prop_ld_cable.ydr
                case 72: //prop_snow_oldlight_01b.ydr
                case 13: //prop_conslift_steps.ydr
                case 86: //prop_scafold_01a.ydr
                case 84: //prop_scafold_04a.ydr
                case 76: //prop_fruitstand_b_nite.ydr
                case 88: //prop_telegwall_01a.ydr
                case 17: //prop_air_stair_04a_cr.ydr
                case 196://prop_dock_rtg_ld.ydr
                case 70: //prop_fnclink_02gate5.ydr
                case 214://prop_facgate_04_l.ydr
                case 198://prop_fncsec_01a.ydr
                case 210://prop_rub_cardpile_01.yft
                case 212://prop_streetlight_01b.yft
                case 208://prop_streetlight_03e.yft
                    break;
                default:
                    break;
            }

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write((byte)this.Type);
            writer.Write(this.Unknown_11h);
            writer.Write(this.Unknown_12h);
            writer.Write(this.SphereRadius);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.BoxMax);
            writer.Write(this.Margin);
            writer.Write(this.BoxMin);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.BoxCenter);
            writer.Write(this.MaterialIndex);
            writer.Write(this.ProceduralId);
            writer.Write(this.RoomId_and_PedDensity);
            writer.Write(this.UnkFlags);
            writer.Write(this.SphereCenter);
            writer.Write(this.PolyFlags);
            writer.Write(this.MaterialColorIndex);
            writer.Write(this.Unknown_5Eh);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Volume);
        }
        public virtual void WriteXml(StringBuilder sb, int indent)
        {
            YbnXml.SelfClosingTag(sb, indent, "BoxMin " + FloatUtil.GetVector3XmlString(BoxMin));
            YbnXml.SelfClosingTag(sb, indent, "BoxMax " + FloatUtil.GetVector3XmlString(BoxMax));
            YbnXml.SelfClosingTag(sb, indent, "BoxCenter " + FloatUtil.GetVector3XmlString(BoxCenter));
            YbnXml.SelfClosingTag(sb, indent, "SphereCenter " + FloatUtil.GetVector3XmlString(SphereCenter));
            YbnXml.ValueTag(sb, indent, "SphereRadius", FloatUtil.ToString(SphereRadius));
            YbnXml.ValueTag(sb, indent, "Margin", FloatUtil.ToString(Margin));
            YbnXml.ValueTag(sb, indent, "Volume", FloatUtil.ToString(Volume));
            YbnXml.SelfClosingTag(sb, indent, "Inertia " + FloatUtil.GetVector3XmlString(Unknown_60h));
            YbnXml.ValueTag(sb, indent, "MaterialIndex", MaterialIndex.ToString());
            YbnXml.ValueTag(sb, indent, "MaterialColourIndex", MaterialColorIndex.ToString());
            YbnXml.ValueTag(sb, indent, "ProceduralID", ProceduralId.ToString());
            YbnXml.ValueTag(sb, indent, "RoomID", RoomId.ToString());
            YbnXml.ValueTag(sb, indent, "PedDensity", PedDensity.ToString());
            YbnXml.ValueTag(sb, indent, "UnkFlags", UnkFlags.ToString());
            YbnXml.ValueTag(sb, indent, "PolyFlags", PolyFlags.ToString());
            YbnXml.ValueTag(sb, indent, "UnkType", Unknown_3Ch.ToString());
            if (Parent != null)
            {
                YbnXml.WriteRawArray(sb, Transform.ToArray(), indent, "CompositeTransform", "", FloatUtil.ToString, 4);
                if (!Parent.OwnerIsFragment)
                {
                    YbnXml.StringTag(sb, indent, "CompositeFlags1", CompositeFlags1.Flags1.ToString());
                    YbnXml.StringTag(sb, indent, "CompositeFlags2", CompositeFlags1.Flags2.ToString());
                }
            }
        }
        public virtual void ReadXml(XmlNode node)
        {
            BoxMin = Xml.GetChildVector3Attributes(node, "BoxMin");
            BoxMax = Xml.GetChildVector3Attributes(node, "BoxMax");
            BoxCenter = Xml.GetChildVector3Attributes(node, "BoxCenter");
            SphereCenter = Xml.GetChildVector3Attributes(node, "SphereCenter");
            SphereRadius = Xml.GetChildFloatAttribute(node, "SphereRadius", "value");
            Margin = Xml.GetChildFloatAttribute(node, "Margin", "value");
            Volume = Xml.GetChildFloatAttribute(node, "Volume", "value");
            Unknown_60h = Xml.GetChildVector3Attributes(node, "Inertia");
            MaterialIndex = (byte)Xml.GetChildUIntAttribute(node, "MaterialIndex", "value");
            MaterialColorIndex = (byte)Xml.GetChildUIntAttribute(node, "MaterialColourIndex", "value");
            ProceduralId = (byte)Xml.GetChildUIntAttribute(node, "ProceduralID", "value");
            RoomId = (byte)Xml.GetChildUIntAttribute(node, "RoomID", "value");
            PedDensity = (byte)Xml.GetChildUIntAttribute(node, "PedDensity", "value");
            UnkFlags = (byte)Xml.GetChildUIntAttribute(node, "UnkFlags", "value");
            PolyFlags = (byte)Xml.GetChildUIntAttribute(node, "PolyFlags", "value");
            Unknown_3Ch = (byte)Xml.GetChildUIntAttribute(node, "UnkType", "value");
            if (Parent != null)
            {
                Transform = new Matrix(Xml.GetChildRawFloatArray(node, "CompositeTransform"));
                TransformInv = Matrix.Invert(Transform);
                if (!Parent.OwnerIsFragment)
                {
                    var f = new BoundCompositeChildrenFlags();
                    f.Flags1 = Xml.GetChildEnumInnerText<EBoundCompositeFlags>(node, "CompositeFlags1");
                    f.Flags2 = Xml.GetChildEnumInnerText<EBoundCompositeFlags>(node, "CompositeFlags2");
                    CompositeFlags1 = f;
                    CompositeFlags2 = f;
                }
            }
        }
        public static void WriteXmlNode(Bounds b, StringBuilder sb, int indent, string name = "Bounds")
        {
            if (b == null)
            {
                YbnXml.SelfClosingTag(sb, indent, name + " type=\"" + BoundsType.None.ToString() + "\"");
            }
            else
            {
                YbnXml.OpenTag(sb, indent, name + " type=\"" + b.Type.ToString() + "\"");
                b.WriteXml(sb, indent + 1);
                YbnXml.CloseTag(sb, indent, name);
            }
        }
        public static Bounds ReadXmlNode(XmlNode node, object owner = null, BoundComposite parent = null)
        {
            if (node == null) return null;
            var typestr = Xml.GetStringAttribute(node, "type");
            var type = Xml.GetEnumValue<BoundsType>(typestr);
            var b = Create(type);
            if (b != null)
            {
                b.Type = type;
                b.Owner = owner;
                b.Parent = parent;
                b.ReadXml(node);
            }
            return b;
        }

        public IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters)
        {
            reader.Position += 16;
            var type = (BoundsType)reader.ReadByte();
            reader.Position -= 17;
            return Create(type);
        }
        public static Bounds Create(BoundsType type)
        {
            switch (type)
            {
                case BoundsType.None: return null;
                case BoundsType.Sphere: return new BoundSphere();
                case BoundsType.Capsule: return new BoundCapsule();
                case BoundsType.Box: return new BoundBox();
                case BoundsType.Geometry: return new BoundGeometry();
                case BoundsType.GeometryBVH: return new BoundBVH();
                case BoundsType.Composite: return new BoundComposite();
                case BoundsType.Disc: return new BoundDisc();
                case BoundsType.Cylinder: return new BoundCylinder();
                case BoundsType.Cloth: return new BoundCloth();
                default: return null; // throw new Exception("Unknown bound type");
            }
        }

        public virtual void CopyFrom(Bounds other)
        {
            if (other == null) return;
            SphereRadius = other.SphereRadius;
            SphereCenter = other.SphereCenter;
            BoxCenter = other.BoxCenter;
            BoxMin = other.BoxMin;
            BoxMax = other.BoxMax;
            Margin = other.Margin;
            Unknown_3Ch = other.Unknown_3Ch;
            MaterialIndex = other.MaterialIndex;
            MaterialColorIndex = other.MaterialColorIndex;
            ProceduralId = other.ProceduralId;
            RoomId_and_PedDensity = other.RoomId_and_PedDensity;
            UnkFlags = other.UnkFlags;
            PolyFlags = other.PolyFlags;
            Unknown_60h = other.Unknown_60h;
            Volume = other.Volume;
        }

        public virtual SpaceSphereIntersectResult SphereIntersect(ref BoundingSphere sph)
        {
            return new SpaceSphereIntersectResult();
        }
        public virtual SpaceRayIntersectResult RayIntersect(ref Ray ray, float maxdist = float.MaxValue)
        {
            return new SpaceRayIntersectResult();
        }

    }
    [TC(typeof(EXP))] public class BoundSphere : Bounds
    {
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            FileVFT = 1080221960;
        }
        public override SpaceSphereIntersectResult SphereIntersect(ref BoundingSphere sph)
        {
            var res = new SpaceSphereIntersectResult();
            var bsph = new BoundingSphere();
            bsph.Center = SphereCenter;
            bsph.Radius = SphereRadius;
            if (sph.Intersects(ref bsph))
            {
                res.Hit = true;
                res.Normal = Vector3.Normalize(sph.Center - SphereCenter);
            }
            return res;
        }
        public override SpaceRayIntersectResult RayIntersect(ref Ray ray, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult();
            var bsph = new BoundingSphere();
            bsph.Center = SphereCenter;
            bsph.Radius = SphereRadius;
            float testdist;
            if (ray.Intersects(ref bsph, out testdist) && (testdist < maxdist))
            {
                res.Hit = true;
                res.HitDist = testdist;
                res.HitBounds = this;
                res.Position = ray.Position + ray.Direction * testdist;
                res.Normal = Vector3.Normalize(res.Position - SphereCenter);
                res.Material.Type = MaterialIndex;
            }
            return res;
        }
    }
    [TC(typeof(EXP))] public class BoundCapsule : Bounds
    {
        public override long BlockLength
        {
            get { return 128; }
        }

        // structure data
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public uint Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.Unknown_78h = reader.ReadUInt32();
            this.Unknown_7Ch = reader.ReadUInt32();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_7Ch);
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            FileVFT = 1080213112;
        }

        public override SpaceSphereIntersectResult SphereIntersect(ref BoundingSphere sph)
        {
            var res = new SpaceSphereIntersectResult();
            var bcap = new BoundingCapsule();
            var extent = new Vector3(0, SphereRadius - Margin, 0);
            bcap.PointA = SphereCenter - extent;
            bcap.PointB = SphereCenter + extent;
            bcap.Radius = Margin;
            if (sph.Intersects(ref bcap, out res.Normal))
            {
                res.Hit = true;
            }
            return res;
        }
        public override SpaceRayIntersectResult RayIntersect(ref Ray ray, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult();
            var bcap = new BoundingCapsule();
            var extent = new Vector3(0, SphereRadius - Margin, 0);
            bcap.PointA = SphereCenter - extent;
            bcap.PointB = SphereCenter + extent;
            bcap.Radius = Margin;
            float testdist;
            if (ray.Intersects(ref bcap, out testdist) && (testdist < maxdist))
            {
                res.Hit = true;
                res.HitDist = testdist;
                res.HitBounds = this;
                res.Position = ray.Position + ray.Direction * testdist;
                res.Normal = bcap.Normal(ref res.Position);
                res.Material.Type = MaterialIndex;
            }
            return res;
        }
    }
    [TC(typeof(EXP))] public class BoundBox : Bounds
    {
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            FileVFT = 1080221016;
        }
        public override SpaceSphereIntersectResult SphereIntersect(ref BoundingSphere sph)
        {
            var res = new SpaceSphereIntersectResult();
            var bbox = new BoundingBox(BoxMin, BoxMax);
            if (sph.Intersects(ref bbox))
            {
                var sphmin = sph.Center - sph.Radius;
                var sphmax = sph.Center + sph.Radius;
                var n = Vector3.Zero;
                float eps = sph.Radius * 0.8f;
                if (Math.Abs(sphmax.X - BoxMin.X) < eps) n -= Vector3.UnitX;
                else if (Math.Abs(sphmin.X - BoxMax.X) < eps) n += Vector3.UnitX;
                else if (Math.Abs(sphmax.Y - BoxMin.Y) < eps) n -= Vector3.UnitY;
                else if (Math.Abs(sphmin.Y - BoxMax.Y) < eps) n += Vector3.UnitY;
                else if (Math.Abs(sphmax.Z - BoxMin.Z) < eps) n -= Vector3.UnitZ;
                else if (Math.Abs(sphmin.Z - BoxMax.Z) < eps) n += Vector3.UnitZ;
                else
                { n = Vector3.UnitZ; } //ray starts inside the box...
                res.Normal = Vector3.Normalize(n);
                res.Hit = true;
            }
            return res;
        }
        public override SpaceRayIntersectResult RayIntersect(ref Ray ray, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult();
            var bbox = new BoundingBox(BoxMin, BoxMax);
            float testdist;
            if (ray.Intersects(ref bbox, out testdist) && (testdist < maxdist))
            {
                const float eps = 0.002f;
                var n = Vector3.Zero;
                var hpt = ray.Position + ray.Direction * testdist;
                if (Math.Abs(hpt.X - BoxMin.X) < eps) n = -Vector3.UnitX;
                else if (Math.Abs(hpt.X - BoxMax.X) < eps) n = Vector3.UnitX;
                else if (Math.Abs(hpt.Y - BoxMin.Y) < eps) n = -Vector3.UnitY;
                else if (Math.Abs(hpt.Y - BoxMax.Y) < eps) n = Vector3.UnitY;
                else if (Math.Abs(hpt.Z - BoxMin.Z) < eps) n = -Vector3.UnitZ;
                else if (Math.Abs(hpt.Z - BoxMax.Z) < eps) n = Vector3.UnitZ;
                else
                { n = Vector3.UnitZ; } //ray starts inside the box...
                res.Hit = true;
                res.HitDist = testdist;
                res.HitBounds = this;
                res.Position = hpt;
                res.Normal = n;
                res.Material.Type = MaterialIndex;
            }
            return res;
        }
    }
    [TC(typeof(EXP))] public class BoundDisc : Bounds
    {
        public override long BlockLength
        {
            get { return 128; }
        }

        // structure data
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public uint Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.Unknown_78h = reader.ReadUInt32();
            this.Unknown_7Ch = reader.ReadUInt32();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_7Ch);
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            FileVFT = 1080229960;
        }

        public override SpaceSphereIntersectResult SphereIntersect(ref BoundingSphere sph)
        {
            //as a temporary hack, just use the sphere-sphere intersection //TODO: sphere-disc intersection
            var res = new SpaceSphereIntersectResult();
            var bsph = new BoundingSphere();
            bsph.Center = SphereCenter;
            bsph.Radius = SphereRadius;
            if (sph.Intersects(ref bsph))
            {
                res.Hit = true;
                res.Normal = Vector3.Normalize(sph.Center - SphereCenter);
            }
            return res;
        }
        public override SpaceRayIntersectResult RayIntersect(ref Ray ray, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult();
            var bcyl = new BoundingCylinder();
            var size = new Vector3(Margin, 0, 0);
            bcyl.PointA = SphereCenter - size;
            bcyl.PointB = SphereCenter + size;
            bcyl.Radius = SphereRadius;
            Vector3 n;
            float testdist;
            if (ray.Intersects(ref bcyl, out testdist, out n) && (testdist < maxdist))
            {
                res.Hit = true;
                res.HitDist = testdist;
                res.HitBounds = this;
                res.Position = ray.Position + ray.Direction * testdist;
                res.Normal = n;
                res.Material.Type = MaterialIndex;
            }
            return res;
        }
    }
    [TC(typeof(EXP))] public class BoundCylinder : Bounds
    {
        public override long BlockLength
        {
            get { return 128; }
        }

        // structure data
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public uint Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.Unknown_78h = reader.ReadUInt32();
            this.Unknown_7Ch = reader.ReadUInt32();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_7Ch);
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            FileVFT = 1080202872;
        }

        public override SpaceSphereIntersectResult SphereIntersect(ref BoundingSphere sph)
        {
            //as a temporary hack, just use the sphere-capsule intersection //TODO: sphere-cylinder intersection
            var res = new SpaceSphereIntersectResult();
            var bcap = new BoundingCapsule();
            var extent = (BoxMax - BoxMin).Abs();
            var size = new Vector3(0, extent.Y * 0.5f, 0);
            bcap.PointA = SphereCenter - size;
            bcap.PointB = SphereCenter + size;
            bcap.Radius = extent.X * 0.5f;
            if (sph.Intersects(ref bcap, out res.Normal))
            {
                res.Hit = true;
            }
            return res;
        }
        public override SpaceRayIntersectResult RayIntersect(ref Ray ray, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult();
            var bcyl = new BoundingCylinder();
            var extent = (BoxMax - BoxMin).Abs();
            var size = new Vector3(0, extent.Y * 0.5f, 0);
            bcyl.PointA = SphereCenter - size;
            bcyl.PointB = SphereCenter + size;
            bcyl.Radius = extent.X * 0.5f;
            Vector3 n;
            float testdist;
            if (ray.Intersects(ref bcyl, out testdist, out n) && (testdist < maxdist))
            {
                res.Hit = true;
                res.HitDist = testdist;
                res.HitBounds = this;
                res.Position = ray.Position + ray.Direction * testdist;
                res.Normal = n;
                res.Material.Type = MaterialIndex;
            }
            return res;
        }
    }
    [TC(typeof(EXP))] public class BoundCloth : Bounds
    {
        //public override long BlockLength
        //{
        //    get { return 128; }
        //}

        //// TODO! length currently unknown!
        //public uint Unknown_70h { get; set; }
        //public uint Unknown_74h { get; set; }
        //public uint Unknown_78h { get; set; }
        //public uint Unknown_7Ch { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            //// read structure data
            //this.Unknown_70h = reader.ReadUInt32();
            //this.Unknown_74h = reader.ReadUInt32();
            //this.Unknown_78h = reader.ReadUInt32();
            //this.Unknown_7Ch = reader.ReadUInt32();


            // found in eg:
            //dt1_16_build4_cloth11.yft
            //dt1_16_build4_cloth12.yft
            //dt1_16_build4_cloth8.yft
            //dt1_16_build4_clothb.yft
            //dt1_17_cloth1.yft
            //dt1_17_cloth1b.yft
            //dt1_17_cloth3.yft
            //dt1_21_dtzz_cloth_a.yft
            //dt1_21_dtzz_cloth_b.yft
            //dt1_21_dtzz_cloth_bx.yft
            //dt1_21_dtzz_cloth_c.yft
            //dt1_21_dtzz_cloth_d.yft
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            //// write structure data
            //writer.Write(this.Unknown_70h);
            //writer.Write(this.Unknown_74h);
            //writer.Write(this.Unknown_78h);
            //writer.Write(this.Unknown_7Ch);
        }

        public override SpaceSphereIntersectResult SphereIntersect(ref BoundingSphere sph)
        {
            var res = new SpaceSphereIntersectResult(); //TODO...
            return res;
        }
        public override SpaceRayIntersectResult RayIntersect(ref Ray ray, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult(); //TODO...
            return res;
        }
    }
    [TC(typeof(EXP))] public class BoundGeometry : Bounds
    {
        public override long BlockLength
        {
            get { return 304; }
        }

        // structure data
        public uint Unknown_70h { get; set; } // 0x00000000
        public uint Unknown_74h { get; set; } // 0x00000000
        public ulong VerticesShrunkPointer { get; set; }
        public ushort Unknown_80h { get; set; } // 0x0000
        public ushort Unknown_82h { get; set; } //des_.ydr's? some extra data to read..?? is this some extra poly count?
        public uint VerticesShrunkCount { get; set; } //always equal to VerticesCount
        public ulong PolygonsPointer { get; set; }
        public Vector3 Quantum { get; set; }
        public float Unknown_9Ch { get; set; }
        public Vector3 CenterGeom { get; set; }
        public float Unknown_ACh { get; set; }
        public ulong VerticesPointer { get; set; }
        public ulong VertexColoursPointer { get; set; }
        public ulong OctantsPointer { get; set; }
        public ulong OctantItemsPointer { get; set; }
        public uint VerticesCount { get; set; }
        public uint PolygonsCount { get; set; }
        public uint Unknown_D8h { get; set; } // 0x00000000
        public uint Unknown_DCh { get; set; } // 0x00000000
        public uint Unknown_E0h { get; set; } // 0x00000000
        public uint Unknown_E4h { get; set; } // 0x00000000
        public uint Unknown_E8h { get; set; } // 0x00000000
        public uint Unknown_ECh { get; set; } // 0x00000000
        public ulong MaterialsPointer { get; set; }
        public ulong MaterialColoursPointer { get; set; }
        public uint Unknown_100h { get; set; } // 0x00000000
        public uint Unknown_104h { get; set; } // 0x00000000
        public uint Unknown_108h { get; set; } // 0x00000000
        public uint Unknown_10Ch { get; set; } // 0x00000000
        public uint Unknown_110h { get; set; } // 0x00000000
        public uint Unknown_114h { get; set; } // 0x00000000
        public ulong PolygonMaterialIndicesPointer { get; set; }
        public byte MaterialsCount { get; set; }
        public byte MaterialColoursCount { get; set; }
        public ushort Unknown_122h { get; set; } // 0x0000
        public uint Unknown_124h { get; set; } // 0x00000000
        public uint Unknown_128h { get; set; } // 0x00000000
        public uint Unknown_12Ch { get; set; } // 0x00000000


        public Vector3[] VerticesShrunk { get; set; } // Vertices but shrunk by margin along normal
        public BoundPolygon[] Polygons { get; set; }
        public Vector3[] Vertices { get; set; }
        public BoundMaterialColour[] VertexColours { get; set; }//not sure, it seems like colours anyway, see eg. prologue03_10.ybn
        public BoundGeomOctants Octants { get; set; }
        public BoundMaterial_s[] Materials { get; set; }
        public BoundMaterialColour[] MaterialColours { get; set; }
        public byte[] PolygonMaterialIndices { get; set; }

        private ResourceSystemStructBlock<BoundVertex_s> VerticesShrunkBlock = null;
        private ResourceSystemDataBlock PolygonsBlock = null;
        private ResourceSystemStructBlock<BoundVertex_s> VerticesBlock = null;
        private ResourceSystemStructBlock<BoundMaterialColour> VertexColoursBlock = null;
        private ResourceSystemStructBlock<BoundMaterial_s> MaterialsBlock = null;
        private ResourceSystemStructBlock<BoundMaterialColour> MaterialColoursBlock = null;
        private ResourceSystemStructBlock<byte> PolygonMaterialIndicesBlock = null;

        private BoundVertex[] VertexObjects = null; //for use by the editor, created as needed by GetVertexObject()


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.VerticesShrunkPointer = reader.ReadUInt64();
            this.Unknown_80h = reader.ReadUInt16();
            this.Unknown_82h = reader.ReadUInt16();
            this.VerticesShrunkCount = reader.ReadUInt32();
            this.PolygonsPointer = reader.ReadUInt64();
            this.Quantum = reader.ReadVector3();
            this.Unknown_9Ch = reader.ReadSingle();
            this.CenterGeom = reader.ReadVector3();
            this.Unknown_ACh = reader.ReadSingle();
            this.VerticesPointer = reader.ReadUInt64();
            this.VertexColoursPointer = reader.ReadUInt64();
            this.OctantsPointer = reader.ReadUInt64();
            this.OctantItemsPointer = reader.ReadUInt64();
            this.VerticesCount = reader.ReadUInt32();
            this.PolygonsCount = reader.ReadUInt32();
            this.Unknown_D8h = reader.ReadUInt32();
            this.Unknown_DCh = reader.ReadUInt32();
            this.Unknown_E0h = reader.ReadUInt32();
            this.Unknown_E4h = reader.ReadUInt32();
            this.Unknown_E8h = reader.ReadUInt32();
            this.Unknown_ECh = reader.ReadUInt32();
            this.MaterialsPointer = reader.ReadUInt64();
            this.MaterialColoursPointer = reader.ReadUInt64();
            this.Unknown_100h = reader.ReadUInt32();
            this.Unknown_104h = reader.ReadUInt32();
            this.Unknown_108h = reader.ReadUInt32();
            this.Unknown_10Ch = reader.ReadUInt32();
            this.Unknown_110h = reader.ReadUInt32();
            this.Unknown_114h = reader.ReadUInt32();
            this.PolygonMaterialIndicesPointer = reader.ReadUInt64();
            this.MaterialsCount = reader.ReadByte();
            this.MaterialColoursCount = reader.ReadByte();
            this.Unknown_122h = reader.ReadUInt16();
            this.Unknown_124h = reader.ReadUInt32();
            this.Unknown_128h = reader.ReadUInt32();
            this.Unknown_12Ch = reader.ReadUInt32();


            var vertsShrunk = reader.ReadStructsAt<BoundVertex_s>(this.VerticesShrunkPointer, this.VerticesShrunkCount);
            if (vertsShrunk != null) //seems to be in YFT's
            {
                VerticesShrunk = new Vector3[vertsShrunk.Length];
                for (int i = 0; i < vertsShrunk.Length; i++)
                {
                    var bv = vertsShrunk[i];
                    VerticesShrunk[i] = bv.Vector * Quantum;
                }
            }

            ReadPolygons(reader);

            var verts = reader.ReadStructsAt<BoundVertex_s>(this.VerticesPointer, this.VerticesCount);
            if (verts != null)
            {
                Vertices = new Vector3[verts.Length];
                for (int i = 0; i < verts.Length; i++)
                {
                    var bv = verts[i];
                    Vertices[i] = bv.Vector * Quantum;
                }
            }

            this.VertexColours = reader.ReadStructsAt<BoundMaterialColour>(this.VertexColoursPointer, this.VerticesCount);

            this.Octants = reader.ReadBlockAt<BoundGeomOctants>(this.OctantsPointer, this.OctantItemsPointer);

            this.Materials = reader.ReadStructsAt<BoundMaterial_s>(this.MaterialsPointer, (this.MaterialsCount < 4) ? 4u : MaterialsCount);

            this.MaterialColours = reader.ReadStructsAt<BoundMaterialColour>(this.MaterialColoursPointer, this.MaterialColoursCount);

            this.PolygonMaterialIndices = reader.ReadBytesAt(this.PolygonMaterialIndicesPointer, (uint)PolygonsCount);



            if ((MaterialsPointer != 0) && (MaterialsCount < 4))
            {
                //for (var i = MaterialsCount; i < 4; i++)
                //{
                //    var m = Materials[i];
                //    if ((m.Data1 != 0) || (m.Data2 != 0))
                //    { }//no hit
                //}


                //the read array was padded, so remove the padding from this array. will re-add padding in BuildMaterials...
                var mats = new BoundMaterial_s[MaterialsCount];
                for (int i = 0; i < MaterialsCount; i++)
                {
                    mats[i] = Materials[i];
                }
                Materials = mats;

            }


            //if (Vertices2Count != VerticesCount)
            //{ }//no hit here
            //if (Unknown_9Ch != 0)
            //{ }
            //if (Unknown_ACh != 0)
            //{ }

            //switch (Unknown_82h)
            //{
            //    case 0:
            //    case 1://ybns
            //        break;
            //    default://des_.ydr's? some extra data to read..?? is this some extra poly count?
            //        break;
            //}

            //if (OctantsPointer != 0)
            //{
            //    if (OctantItemsPointer != OctantsPointer + 32)
            //    { }//no hit
            //}

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.VerticesShrunkPointer = (ulong)(this.VerticesShrunkBlock != null ? this.VerticesShrunkBlock.FilePosition : 0);
            this.PolygonsPointer = (ulong)(this.PolygonsBlock != null ? this.PolygonsBlock.FilePosition : 0);
            this.VerticesPointer = (ulong)(this.VerticesBlock != null ? this.VerticesBlock.FilePosition : 0);
            this.VertexColoursPointer = (ulong)(this.VertexColoursBlock != null ? this.VertexColoursBlock.FilePosition : 0);
            this.OctantsPointer = (ulong)(this.Octants != null ? this.Octants.FilePosition : 0);
            this.OctantItemsPointer = (OctantsPointer != 0) ? OctantsPointer + 32 : 0;
            this.VerticesCount = (uint)(this.VerticesBlock != null ? this.VerticesBlock.ItemCount : 0);
            this.VerticesShrunkCount = this.VerticesCount;
            this.PolygonsCount = (uint)(this.Polygons != null ? this.Polygons.Length : 0);
            this.MaterialsPointer = (ulong)(this.MaterialsBlock != null ? this.MaterialsBlock.FilePosition : 0);
            this.MaterialColoursPointer = (ulong)(this.MaterialColoursBlock != null ? this.MaterialColoursBlock.FilePosition : 0);
            this.PolygonMaterialIndicesPointer = (ulong)(this.PolygonMaterialIndicesBlock != null ? this.PolygonMaterialIndicesBlock.FilePosition : 0);
            //this.MaterialsCount = (byte)(this.MaterialsBlock != null ? this.MaterialsBlock.ItemCount : 0);//this is updated by BuildMaterials, and the array could include padding!
            this.MaterialColoursCount = (byte)(this.MaterialColoursBlock != null ? this.MaterialColoursBlock.ItemCount : 0);


            // write structure data
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.Write(this.VerticesShrunkPointer);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_82h);
            writer.Write(this.VerticesShrunkCount);
            writer.Write(this.PolygonsPointer);
            writer.Write(this.Quantum);
            writer.Write(this.Unknown_9Ch);
            writer.Write(this.CenterGeom);
            writer.Write(this.Unknown_ACh);
            writer.Write(this.VerticesPointer);
            writer.Write(this.VertexColoursPointer);
            writer.Write(this.OctantsPointer);
            writer.Write(this.OctantItemsPointer);
            writer.Write(this.VerticesCount);
            writer.Write(this.PolygonsCount);
            writer.Write(this.Unknown_D8h);
            writer.Write(this.Unknown_DCh);
            writer.Write(this.Unknown_E0h);
            writer.Write(this.Unknown_E4h);
            writer.Write(this.Unknown_E8h);
            writer.Write(this.Unknown_ECh);
            writer.Write(this.MaterialsPointer);
            writer.Write(this.MaterialColoursPointer);
            writer.Write(this.Unknown_100h);
            writer.Write(this.Unknown_104h);
            writer.Write(this.Unknown_108h);
            writer.Write(this.Unknown_10Ch);
            writer.Write(this.Unknown_110h);
            writer.Write(this.Unknown_114h);
            writer.Write(this.PolygonMaterialIndicesPointer);
            writer.Write(this.MaterialsCount);
            writer.Write(this.MaterialColoursCount);
            writer.Write(this.Unknown_122h);
            writer.Write(this.Unknown_124h);
            writer.Write(this.Unknown_128h);
            writer.Write(this.Unknown_12Ch);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            YbnXml.SelfClosingTag(sb, indent, "GeometryCenter " + FloatUtil.GetVector3XmlString(CenterGeom));
            YbnXml.ValueTag(sb, indent, "UnkFloat1", FloatUtil.ToString(Unknown_9Ch));
            YbnXml.ValueTag(sb, indent, "UnkFloat2", FloatUtil.ToString(Unknown_ACh));

            if (Materials != null)
            {
                YbnXml.WriteItemArray(sb, Materials, indent, "Materials");
            }
            if (MaterialColours != null)
            {
                YbnXml.WriteRawArray(sb, MaterialColours, indent, "MaterialColours", "", YbnXml.FormatBoundMaterialColour, 1);
            }
            if (Vertices != null)
            {
                YbnXml.WriteRawArray(sb, Vertices, indent, "Vertices", "", YbnXml.FormatVector3, 1);
            }
            if (VertexColours != null)
            {
                YbnXml.WriteRawArray(sb, VertexColours, indent, "VertexColours", "", YbnXml.FormatBoundMaterialColour, 1);
            }
            if (Polygons != null)
            {
                YbnXml.WriteCustomItemArray(sb, Polygons, indent, "Polygons");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            CenterGeom = Xml.GetChildVector3Attributes(node, "GeometryCenter");
            Unknown_9Ch = Xml.GetChildFloatAttribute(node, "UnkFloat1", "value");
            Unknown_ACh = Xml.GetChildFloatAttribute(node, "UnkFloat2", "value");

            Materials = XmlMeta.ReadItemArray<BoundMaterial_s>(node, "Materials");
            MaterialColours = XmlYbn.GetChildRawBoundMaterialColourArray(node, "MaterialColours");
            Vertices = Xml.GetChildRawVector3ArrayNullable(node, "Vertices");
            VertexColours = XmlYbn.GetChildRawBoundMaterialColourArray(node, "VertexColours");

            var pnode = node.SelectSingleNode("Polygons");
            if (pnode != null)
            {
                var inodes = pnode.ChildNodes;
                if (inodes?.Count > 0)
                {
                    var polylist = new List<BoundPolygon>();
                    foreach (XmlNode inode in inodes)
                    {
                        var typestr = inode.Name;
                        var type = Xml.GetEnumValue<BoundPolygonType>(typestr);
                        var poly = CreatePolygon(type);
                        if (poly != null)
                        {
                            poly.ReadXml(inode);
                            polylist.Add(poly);
                        }
                    }
                    Polygons = polylist.ToArray();
                }
            }

            CalculateVertsShrunkByMargin();
            CalculateOctants();

            BuildMaterials();
            CalculateQuantum();
            UpdateEdgeIndices();
            UpdateTriangleAreas();

            FileVFT = 1080226408;
        }

        public override IResourceBlock[] GetReferences()
        {
            BuildMaterials();
            CalculateQuantum();
            UpdateEdgeIndices();
            UpdateTriangleAreas();

            var list = new List<IResourceBlock>(base.GetReferences());
            if (VerticesShrunk != null)
            {
                var verts = new List<BoundVertex_s>();
                foreach (var v in VerticesShrunk)
                {
                    var vq = v / Quantum;
                    var vs = new BoundVertex_s(vq);
                    verts.Add(vs);
                }
                VerticesShrunkBlock = new ResourceSystemStructBlock<BoundVertex_s>(verts.ToArray());
                list.Add(VerticesShrunkBlock);
            }
            if (Polygons != null)
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                foreach (var poly in Polygons)
                {
                    poly.Write(bw);
                }
                var polydata = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(polydata, 0, polydata.Length);
                for (int i = 0; i < Polygons.Length; i++)
                {
                    var o = i * 16;
                    var t = (byte)Polygons[i].Type;
                    var b = polydata[o];
                    polydata[o] = (byte)((b & 0xF8) | (t & 7)); //add the poly types back in!
                }

                if (polydata.Length != (Polygons.Length * 16))
                { }

                PolygonsBlock = new ResourceSystemDataBlock(polydata);
                list.Add(PolygonsBlock);
            }
            if (Vertices != null)
            {
                var verts = new List<BoundVertex_s>();
                foreach (var v in Vertices)
                {
                    var vq = v / Quantum;
                    var vs = new BoundVertex_s(vq);
                    verts.Add(vs);
                }
                VerticesBlock = new ResourceSystemStructBlock<BoundVertex_s>(verts.ToArray());
                list.Add(VerticesBlock);
            }
            if (VertexColours != null)
            {
                VertexColoursBlock = new ResourceSystemStructBlock<BoundMaterialColour>(VertexColours);
                list.Add(VertexColoursBlock);
            }
            if (Octants != null)
            {
                list.Add(Octants);//this one is already a resource block!
            }
            if (Materials != null)
            {
                MaterialsBlock = new ResourceSystemStructBlock<BoundMaterial_s>(Materials);
                list.Add(MaterialsBlock);
            }
            if (MaterialColours != null)
            {
                MaterialColoursBlock = new ResourceSystemStructBlock<BoundMaterialColour>(MaterialColours);
                list.Add(MaterialColoursBlock);
            }
            if (PolygonMaterialIndices != null)
            {
                PolygonMaterialIndicesBlock = new ResourceSystemStructBlock<byte>(PolygonMaterialIndices);
                list.Add(PolygonMaterialIndicesBlock);
            }
            return list.ToArray();
        }



        private void ReadPolygons(ResourceDataReader reader)
        {
            if(PolygonsCount==0)
            { return; }

            Polygons = new BoundPolygon[PolygonsCount];
            uint polybytecount = PolygonsCount * 16;
            var polygonData = reader.ReadBytesAt(PolygonsPointer, polybytecount);
            for (int i = 0; i < PolygonsCount; i++)
            {
                var offset = i * 16;
                byte b0 = polygonData[offset];
                polygonData[offset] = (byte)(b0 & 0xF8);//mask it off
                BoundPolygonType type = (BoundPolygonType)(b0 & 7);
                BoundPolygon p = CreatePolygon(type);
                if (p != null)
                {
                    p.Index = i;
                    p.Read(polygonData, offset);
                }
                Polygons[i] = p;
            }
        }

        public BoundVertex GetVertexObject(int index)
        {
            //gets a cached object which references a single vertex in this geometry
            if (Vertices == null) return null;
            if ((index < 0) || (index >= Vertices.Length)) return null;
            if ((VertexObjects == null) || (VertexObjects.Length != Vertices.Length))
            {
                VertexObjects = new BoundVertex[Vertices.Length];
            }
            if (index >= VertexObjects.Length) return null;
            var r = VertexObjects[index];
            if (r == null)
            {
                r = new BoundVertex(this, index);
                VertexObjects[index] = r;
            }
            return r;
        }
        public Vector3 GetVertex(int index)
        {
            return ((index >= 0) && (index < Vertices.Length)) ? Vertices[index] : Vector3.Zero;
        }
        public Vector3 GetVertexPos(int index)
        {
            var v = GetVertex(index) + CenterGeom;
            return Vector3.Transform(v, Transform).XYZ();
        }
        public void SetVertexPos(int index, Vector3 v)
        {
            if ((index >= 0) && (index < Vertices.Length))
            {
                var t = Vector3.Transform(v, TransformInv).XYZ() - CenterGeom;
                Vertices[index] = t;
            }
        }
        public BoundMaterialColour GetVertexColour(int index)
        {
            return ((VertexColours != null) && (index >= 0) && (index < VertexColours.Length)) ? VertexColours[index] : new BoundMaterialColour();
        }
        public void SetVertexColour(int index, BoundMaterialColour c)
        {
            if ((VertexColours != null) && (index >= 0) && (index < VertexColours.Length))
            {
                VertexColours[index] = c;
            }
        }




        public override SpaceSphereIntersectResult SphereIntersect(ref BoundingSphere sph)
        {
            var res = new SpaceSphereIntersectResult();
            var box = new BoundingBox();

            if (Polygons == null)
            { return res; }

            box.Minimum = BoxMin;
            box.Maximum = BoxMax;
            if (!sph.Intersects(ref box))
            { return res; }

            SphereIntersectPolygons(ref sph, ref res, 0, Polygons.Length);

            return res;
        }
        public override SpaceRayIntersectResult RayIntersect(ref Ray ray, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult();
            var box = new BoundingBox();

            if (Polygons == null)
            { return res; }

            box.Minimum = BoxMin;
            box.Maximum = BoxMax;
            float bvhboxhittest;
            if (!ray.Intersects(ref box, out bvhboxhittest))
            { return res; }
            if (bvhboxhittest > maxdist)
            { return res; } //already a closer hit.

            res.HitDist = maxdist;

            RayIntersectPolygons(ref ray, ref res, 0, Polygons.Length);

            return res;
        }


        protected void SphereIntersectPolygons(ref BoundingSphere sph, ref SpaceSphereIntersectResult res, int startIndex, int endIndex)
        {
            var box = new BoundingBox();
            var tsph = new BoundingSphere();
            var spht = new BoundingSphere();
            var sp = sph.Center;
            var sr = sph.Radius;
            Vector3 p1, p2, p3, p4, a1, a2, a3;
            Vector3 n1 = Vector3.Zero;

            for (int p = startIndex; p < endIndex; p++)
            {
                var polygon = Polygons[p];
                bool polyhit = false;
                switch (polygon.Type)
                {
                    case BoundPolygonType.Triangle:
                        var ptri = polygon as BoundPolygonTriangle;
                        p1 = GetVertexPos(ptri.vertIndex1);
                        p2 = GetVertexPos(ptri.vertIndex2);
                        p3 = GetVertexPos(ptri.vertIndex3);
                        polyhit = sph.Intersects(ref p1, ref p2, ref p3);
                        if (polyhit) n1 = Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));
                        break;
                    case BoundPolygonType.Sphere:
                        var psph = polygon as BoundPolygonSphere;
                        tsph.Center = GetVertexPos(psph.sphereIndex);
                        tsph.Radius = psph.sphereRadius;
                        polyhit = sph.Intersects(ref tsph);
                        if (polyhit) n1 = Vector3.Normalize(sph.Center - tsph.Center);
                        break;
                    case BoundPolygonType.Capsule:
                        var pcap = polygon as BoundPolygonCapsule;
                        var tcap = new BoundingCapsule();
                        tcap.PointA = GetVertexPos(pcap.capsuleIndex1);
                        tcap.PointB = GetVertexPos(pcap.capsuleIndex2);
                        tcap.Radius = pcap.capsuleRadius;
                        polyhit = sph.Intersects(ref tcap, out n1);
                        break;
                    case BoundPolygonType.Box:
                        var pbox = polygon as BoundPolygonBox;
                        p1 = GetVertexPos(pbox.boxIndex1);//corner
                        p2 = GetVertexPos(pbox.boxIndex2);
                        p3 = GetVertexPos(pbox.boxIndex3);
                        p4 = GetVertexPos(pbox.boxIndex4);
                        a1 = ((p3 + p4) - (p1 + p2)) * 0.5f;
                        a2 = p3 - (p1 + a1);
                        a3 = p4 - (p1 + a1);
                        Vector3 bs = new Vector3(a1.Length(), a2.Length(), a3.Length());
                        Vector3 m1 = a1 / bs.X;
                        Vector3 m2 = a2 / bs.Y;
                        Vector3 m3 = a3 / bs.Z;
                        if ((bs.X < bs.Y) && (bs.X < bs.Z)) m1 = Vector3.Cross(m2, m3);
                        else if (bs.Y < bs.Z) m2 = Vector3.Cross(m3, m1);
                        else m3 = Vector3.Cross(m1, m2);
                        Vector3 tp = sp - (p1);//+cg
                        spht.Center = new Vector3(Vector3.Dot(tp, m1), Vector3.Dot(tp, m2), Vector3.Dot(tp, m3));
                        spht.Radius = sph.Radius;
                        box.Minimum = Vector3.Zero;
                        box.Maximum = bs;
                        polyhit = spht.Intersects(ref box);
                        if (polyhit)
                        {
                            Vector3 smin = spht.Center - spht.Radius;
                            Vector3 smax = spht.Center + spht.Radius;
                            float eps = spht.Radius * 0.8f;
                            n1 = Vector3.Zero;
                            if (Math.Abs(smax.X) < eps) n1 -= m1;
                            else if (Math.Abs(smin.X - bs.X) < eps) n1 += m1;
                            if (Math.Abs(smax.Y) < eps) n1 -= m2;
                            else if (Math.Abs(smin.Y - bs.Y) < eps) n1 += m2;
                            if (Math.Abs(smax.Z) < eps) n1 -= m3;
                            else if (Math.Abs(smin.Z - bs.Z) < eps) n1 += m3;
                            float n1l = n1.Length();
                            if (n1l > 0.0f) n1 = n1 / n1l;
                            else n1 = Vector3.UnitZ;
                        }
                        break;
                    case BoundPolygonType.Cylinder:
                        var pcyl = polygon as BoundPolygonCylinder;
                        //var tcyl = new BoundingCylinder();
                        //tcyl.PointA = GetVertexPos(pcyl.cylinderIndex1);
                        //tcyl.PointB = GetVertexPos(pcyl.cylinderIndex2);
                        //tcyl.Radius = pcyl.cylinderRadius;
                        //////polyhit = sph.Intersects(ref tcyl, out polyhittestdist, out n1);
                        ////////TODO
                        var ttcap = new BoundingCapsule();//just use the capsule intersection for now...
                        ttcap.PointA = GetVertexPos(pcyl.cylinderIndex1);
                        ttcap.PointB = GetVertexPos(pcyl.cylinderIndex2);
                        ttcap.Radius = pcyl.cylinderRadius;
                        polyhit = sph.Intersects(ref ttcap, out n1);
                        break;
                    default:
                        break;
                }
                if (polyhit) // && (polyhittestdist < itemhitdist) && (polyhittestdist < maxdist))
                {
                    res.HitPolygon = polygon;
                    //itemhitdist = polyhittestdist;
                    //ybnhit = true;
                    res.Hit = true;
                    res.Normal = n1;
                }
                res.TestedPolyCount++;
            }
        }
        protected void RayIntersectPolygons(ref Ray ray, ref SpaceRayIntersectResult res, int startIndex, int endIndex)
        {
            var box = new BoundingBox();
            var tsph = new BoundingSphere();
            var rayt = new Ray();
            var rp = ray.Position;
            var rd = ray.Direction;
            Vector3 p1, p2, p3, p4, a1, a2, a3;
            Vector3 n1 = Vector3.Zero;

            for (int p = startIndex; p < endIndex; p++)
            {
                var polygon = Polygons[p];
                float polyhittestdist = float.MaxValue;
                bool polyhit = false;
                switch (polygon.Type)
                {
                    case BoundPolygonType.Triangle:
                        var ptri = polygon as BoundPolygonTriangle;
                        p1 = GetVertexPos(ptri.vertIndex1);
                        p2 = GetVertexPos(ptri.vertIndex2);
                        p3 = GetVertexPos(ptri.vertIndex3);
                        polyhit = ray.Intersects(ref p1, ref p2, ref p3, out polyhittestdist);
                        if (polyhit) n1 = Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));
                        break;
                    case BoundPolygonType.Sphere:
                        var psph = polygon as BoundPolygonSphere;
                        tsph.Center = GetVertexPos(psph.sphereIndex);
                        tsph.Radius = psph.sphereRadius;
                        polyhit = ray.Intersects(ref tsph, out polyhittestdist);
                        if (polyhit) n1 = Vector3.Normalize((ray.Position + ray.Direction * polyhittestdist) - tsph.Center);
                        break;
                    case BoundPolygonType.Capsule:
                        var pcap = polygon as BoundPolygonCapsule;
                        var tcap = new BoundingCapsule();
                        tcap.PointA = GetVertexPos(pcap.capsuleIndex1);
                        tcap.PointB = GetVertexPos(pcap.capsuleIndex2);
                        tcap.Radius = pcap.capsuleRadius;
                        polyhit = ray.Intersects(ref tcap, out polyhittestdist);
                        res.Position = (ray.Position + ray.Direction * polyhittestdist);
                        if (polyhit) n1 = tcap.Normal(ref res.Position);
                        break;
                    case BoundPolygonType.Box:
                        var pbox = polygon as BoundPolygonBox;
                        p1 = GetVertexPos(pbox.boxIndex1);//corner
                        p2 = GetVertexPos(pbox.boxIndex2);
                        p3 = GetVertexPos(pbox.boxIndex3);
                        p4 = GetVertexPos(pbox.boxIndex4);
                        a1 = ((p3 + p4) - (p1 + p2)) * 0.5f;
                        a2 = p3 - (p1 + a1);
                        a3 = p4 - (p1 + a1);
                        Vector3 bs = new Vector3(a1.Length(), a2.Length(), a3.Length());
                        Vector3 m1 = a1 / bs.X;
                        Vector3 m2 = a2 / bs.Y;
                        Vector3 m3 = a3 / bs.Z;
                        if ((bs.X < bs.Y) && (bs.X < bs.Z)) m1 = Vector3.Cross(m2, m3);
                        else if (bs.Y < bs.Z) m2 = Vector3.Cross(m3, m1);
                        else m3 = Vector3.Cross(m1, m2);
                        Vector3 tp = rp - (p1);//+cg
                        rayt.Position = new Vector3(Vector3.Dot(tp, m1), Vector3.Dot(tp, m2), Vector3.Dot(tp, m3));
                        rayt.Direction = new Vector3(Vector3.Dot(rd, m1), Vector3.Dot(rd, m2), Vector3.Dot(rd, m3));
                        box.Minimum = Vector3.Zero;
                        box.Maximum = bs;
                        polyhit = rayt.Intersects(ref box, out polyhittestdist);
                        if (polyhit)
                        {
                            Vector3 hpt = rayt.Position + rayt.Direction * polyhittestdist;
                            const float eps = 0.002f;
                            if (Math.Abs(hpt.X) < eps) n1 = -m1;
                            else if (Math.Abs(hpt.X - bs.X) < eps) n1 = m1;
                            else if (Math.Abs(hpt.Y) < eps) n1 = -m2;
                            else if (Math.Abs(hpt.Y - bs.Y) < eps) n1 = m2;
                            else if (Math.Abs(hpt.Z) < eps) n1 = -m3;
                            else if (Math.Abs(hpt.Z - bs.Z) < eps) n1 = m3;
                            else
                            { n1 = Vector3.UnitZ; } //ray starts inside the box...
                        }
                        break;
                    case BoundPolygonType.Cylinder:
                        var pcyl = polygon as BoundPolygonCylinder;
                        var tcyl = new BoundingCylinder();
                        tcyl.PointA = GetVertexPos(pcyl.cylinderIndex1);
                        tcyl.PointB = GetVertexPos(pcyl.cylinderIndex2);
                        tcyl.Radius = pcyl.cylinderRadius;
                        polyhit = ray.Intersects(ref tcyl, out polyhittestdist, out n1);
                        break;
                    default:
                        break;
                }
                if (polyhit && (polyhittestdist < res.HitDist))
                {
                    res.HitDist = polyhittestdist;
                    res.Hit = true;
                    res.Position = (ray.Position + ray.Direction * polyhittestdist);
                    res.Normal = n1;
                    res.HitVertex = (polygon != null) ? polygon.NearestVertex(res.Position) : new BoundVertexRef(-1, float.MaxValue);
                    res.HitPolygon = polygon;
                    res.HitBounds = this;
                    res.Material = polygon.Material;
                }
                res.TestedPolyCount++;
            }
        }



        public int GetMaterialIndex(int polyIndex)
        {
            var matind = 0;
            if ((PolygonMaterialIndices != null) && (polyIndex < PolygonMaterialIndices.Length))
            {
                matind = PolygonMaterialIndices[polyIndex];
            }
            return matind;
        }
        public BoundMaterial_s GetMaterialByIndex(int matIndex)
        {
            if ((Materials != null) && (matIndex < Materials.Length))
            {
                return Materials[matIndex];
            }
            return new BoundMaterial_s();
        }
        public BoundMaterial_s GetMaterial(int polyIndex)
        {
            var matind = GetMaterialIndex(polyIndex);
            return GetMaterialByIndex(matind);
        }
        public void SetMaterial(int polyIndex, BoundMaterial_s mat)
        {
            //updates the shared material for the given poly.
            var matind = 0;
            if ((PolygonMaterialIndices != null) && (polyIndex < PolygonMaterialIndices.Length))
            {
                matind = PolygonMaterialIndices[polyIndex];
            }
            if ((Materials != null) && (matind < Materials.Length))
            {
                Materials[matind] = mat;
            }
        }

        public void CalculateOctants()
        {
            Octants = new BoundGeomOctants();

            Vector3[] flipDirection = new Vector3[8]
                {
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(-1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, -1.0f, 1.0f),
                    new Vector3(-1.0f, -1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, -1.0f),
                    new Vector3(-1.0f, 1.0f, -1.0f),
                    new Vector3(1.0f, -1.0f, -1.0f),
                    new Vector3(-1.0f, -1.0f, -1.0f)
                };

            bool isShadowed(Vector3 v1, Vector3 v2, int octant)
            {
                Vector3 direction = v2 - v1;
                Vector3 flip = flipDirection[octant];
                direction *= flip;

                return direction.X >= 0.0 && direction.Y >= 0.0 && direction.Z >= 0.0;
            }

            uint[] getVerticesInOctant(int octant)
            {
                List<uint> octantIndices = new List<uint>();

                for (uint ind1 = 0; ind1 < VerticesShrunk.Length; ind1++)
                {
                    Vector3 vertex = VerticesShrunk[ind1];

                    bool shouldAdd = true;
                    List<uint> octantIndices2 = new List<uint>();

                    foreach (uint ind2 in octantIndices)
                    {
                        Vector3 vertex2 = VerticesShrunk[ind2];

                        if (isShadowed(vertex, vertex2, octant))
                        {
                            shouldAdd = false;
                            octantIndices2 = octantIndices;
                            break;
                        }

                        if (!isShadowed(vertex2, vertex, octant))
                        {
                            octantIndices2.Add(ind2);
                        }

                    }

                    if (shouldAdd)
                    {
                        octantIndices2.Add(ind1);
                    }

                    octantIndices = octantIndices2;
                }

                return octantIndices.ToArray();
            }


            for (int i = 0; i < 8; i++)
            {
                Octants.Items[i] = getVerticesInOctant(i);
                Octants.UpdateCounts();
            }
        }

        public void CalculateVertsShrunkByMargin()
        {
            Vector3[] vertNormals = CalculateVertNormals();
            VerticesShrunk = new Vector3[Vertices.Length];

            for (int i = 0; i < Vertices.Length; i++)
            {
                Vector3 normalShrunk = vertNormals[i] * -Margin;
                VerticesShrunk[i] = Vertices[i] + normalShrunk;
            }
        }

        public Vector3[] CalculateVertNormals()
        {
            Vector3[] vertNormals = new Vector3[Vertices.Length];

            for (int i = 0; i < Polygons.Length; i++)
            {
                var tri = Polygons[i] as BoundPolygonTriangle;
                if (tri == null) { continue; }

                var p1 = tri.Vertex1;
                var p2 = tri.Vertex2;
                var p3 = tri.Vertex3;
                var p1Local = p1 - p2;
                var p3Local = p3 - p2;
                var normal = Vector3.Cross(p1Local, p3Local);
                normal.Normalize();

                vertNormals[tri.vertIndex1] += normal;
                vertNormals[tri.vertIndex2] += normal;
                vertNormals[tri.vertIndex3] += normal;
            }

            for (int i = 0; i < vertNormals.Length; i++)
            {
                if (vertNormals[i].IsZero) { continue; }

                vertNormals[i].Normalize();
            }

            return vertNormals;
        }

        public void CalculateQuantum()
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            if (Vertices != null)
            {
                foreach (var v in Vertices)
                {
                    min = Vector3.Min(min, v);
                    max = Vector3.Max(max, v);
                }
            }
            if (VerticesShrunk != null)
            {
                foreach (var v in VerticesShrunk)
                {
                    min = Vector3.Min(min, v);
                    max = Vector3.Max(max, v);
                }
            }
            var maxsiz = Vector3.Max(min.Abs(), max.Abs());
            var q = (maxsiz+Margin) / 32767.0f;
            Quantum = q;
        }

        public void BuildMaterials()
        {
            //update Materials and PolygonMaterialIndices arrays, using custom materials from polys and existing materials

            var matdict = new Dictionary<BoundMaterial_s, byte>();
            var matlist = new List<BoundMaterial_s>();
            var polymats = new List<byte>();

            if (Polygons != null)
            {
                foreach (var poly in Polygons)
                {
                    var mat = poly.Material;
                    if (matdict.TryGetValue(mat, out byte matidx))
                    {
                        polymats.Add(matidx);
                    }
                    else
                    {
                        matidx = (byte)matlist.Count;
                        matdict.Add(mat, matidx);
                        matlist.Add(mat);
                        polymats.Add(matidx);
                    }
                }
            }

            MaterialsCount = (byte)matlist.Count;

            //add padding to the array for writing
            for (int i = matlist.Count; i < 4; i++)
            {
                matlist.Add(new BoundMaterial_s());
            }

            Materials = matlist.ToArray();
            PolygonMaterialIndices = polymats.ToArray();
        }

        public void UpdateEdgeIndices()
        {
            //update all triangle edge indices, based on shared vertex indices

            if (Polygons == null)
            { return; }

            for (int i = 0; i < Polygons.Length; i++)
            {
                var poly = Polygons[i];
                if (poly != null)
                {
                    poly.Index = i;
                }
            }

            var edgedict = new Dictionary<BoundEdgeRef, BoundEdge>();
            foreach (var poly in Polygons)
            {
                if (poly is BoundPolygonTriangle btri)
                {
                    var e1 = new BoundEdgeRef(btri.vertIndex1, btri.vertIndex2);
                    var e2 = new BoundEdgeRef(btri.vertIndex2, btri.vertIndex3);
                    var e3 = new BoundEdgeRef(btri.vertIndex3, btri.vertIndex1);

                    if (edgedict.TryGetValue(e1, out BoundEdge edge1))
                    {
                        if (edge1.Triangle2 != null)
                        {
                            btri.SetEdgeIndex(1, (short)edge1.Triangle1.Index);
                        }
                        else
                        {
                            edge1.Triangle2 = btri;
                            edge1.EdgeID2 = 1;
                        }
                    }
                    else
                    {
                        edgedict[e1] = new BoundEdge(btri, 1);
                    }
                    if (edgedict.TryGetValue(e2, out BoundEdge edge2))
                    {
                        if (edge2.Triangle2 != null)
                        {
                            btri.SetEdgeIndex(2, (short)edge2.Triangle1.Index);
                        }
                        else
                        {
                            edge2.Triangle2 = btri;
                            edge2.EdgeID2 = 2;
                        }
                    }
                    else
                    {
                        edgedict[e2] = new BoundEdge(btri, 2);
                    }
                    if (edgedict.TryGetValue(e3, out BoundEdge edge3))
                    {
                        if (edge3.Triangle2 != null)
                        {
                            btri.SetEdgeIndex(3, (short)edge3.Triangle1.Index);
                        }
                        else
                        {
                            edge3.Triangle2 = btri;
                            edge3.EdgeID2 = 3;
                        }
                    }
                    else
                    {
                        edgedict[e3] = new BoundEdge(btri, 3);
                    }

                }
            }

            foreach (var kvp in edgedict)
            {
                var eref = kvp.Key;
                var edge = kvp.Value;

                if (edge.Triangle1 == null)
                { continue; }


                if (edge.Triangle2 == null)
                {
                    edge.Triangle1.SetEdgeIndex(edge.EdgeID1, -1);
                }
                else
                {
                    edge.Triangle1.SetEdgeIndex(edge.EdgeID1, (short)edge.Triangle2.Index);
                    edge.Triangle2.SetEdgeIndex(edge.EdgeID2, (short)edge.Triangle1.Index);
                }
            }


            foreach (var poly in Polygons)
            {
                if (poly is BoundPolygonTriangle btri)
                {
                    if (btri.edgeIndex1 >= Polygons.Length)
                    { } //just checking....
                    if (btri.edgeIndex2 >= Polygons.Length)
                    { }
                    if (btri.edgeIndex3 >= Polygons.Length)
                    { }
                }
            }

        }

        public void UpdateTriangleAreas()
        {
            //update all triangle areas, based on vertex positions

            if (Polygons == null)
            { return; }

            var edgedict = new Dictionary<BoundEdgeRef, BoundEdge>();
            foreach (var poly in Polygons)
            {
                if (poly is BoundPolygonTriangle btri)
                {
                    var v1 = btri.Vertex1;
                    var v2 = btri.Vertex2;
                    var v3 = btri.Vertex3;
                    var area = TriangleMath.Area(ref v1, ref v2, ref v3);
                    //if (Math.Abs(btri.triArea - area) > Math.Max(area*0.1f,0.1f))
                    //{ }//ehh good enough
                    btri.triArea = area;
                }
            }
        }

        public bool DeletePolygon(BoundPolygon p)
        {
            if (Polygons != null)
            {
                var polys = Polygons.ToList();
                var polymats = PolygonMaterialIndices.ToList();
                var idx = polys.IndexOf(p);
                if (idx >= 0)
                {
                    polys.RemoveAt(idx);
                    polymats.RemoveAt(idx);
                    Polygons = polys.ToArray();
                    PolygonMaterialIndices = polymats.ToArray();
                    PolygonsCount = (uint)polys.Count;

                    for (int i = 0; i < Polygons.Length; i++)
                    {
                        var poly = Polygons[i];
                        if (poly is BoundPolygonTriangle btri)
                        {
                            if (btri.edgeIndex1 == idx) btri.edgeIndex1 = -1;
                            if (btri.edgeIndex1 > idx) btri.edgeIndex1--;
                            if (btri.edgeIndex2 == idx) btri.edgeIndex2 = -1;
                            if (btri.edgeIndex2 > idx) btri.edgeIndex2--;
                            if (btri.edgeIndex3 == idx) btri.edgeIndex3 = -1;
                            if (btri.edgeIndex3 > idx) btri.edgeIndex3--;
                        }
                        poly.Index = i;
                    }

                    var verts = p.VertexIndices;
                    for (int i = 0; i < verts.Length; i++)
                    {
                        if (DeleteVertex(verts[i], false)) //delete any orphaned vertices
                        {
                            verts = p.VertexIndices;//vertex indices will have changed, need to continue with the new ones!
                        }
                    }

                    return true;
                }

            }
            return false;
        }

        public bool DeleteVertex(int index, bool deletePolys = true)
        {
            if (Vertices != null)
            {
                if (!deletePolys)
                {
                    //if not deleting polys, make sure this vertex isn't used by any
                    foreach (var poly in Polygons)
                    {
                        var pverts = poly.VertexIndices;
                        for (int i = 0; i < pverts.Length; i++)
                        {
                            if (pverts[i] == index)
                            {
                                return false;
                            }
                        }
                    }
                }

                var verts = Vertices.ToList();
                var verts2 = VerticesShrunk?.ToList();
                var vertcols = VertexColours?.ToList();
                var vertobjs = VertexObjects?.ToList();
                verts.RemoveAt(index);
                verts2?.RemoveAt(index);
                vertcols?.RemoveAt(index);
                vertobjs?.RemoveAt(index);
                Vertices = verts.ToArray();
                VerticesShrunk = verts2?.ToArray();
                VertexColours = vertcols?.ToArray();
                VertexObjects = vertobjs?.ToArray();
                VerticesCount = (uint)verts.Count;
                VerticesShrunkCount = VerticesCount;

                if (VertexObjects != null)
                {
                    for (int i = 0; i < VertexObjects.Length; i++)
                    {
                        if (VertexObjects[i] != null) VertexObjects[i].Index = i;
                    }
                }

                if (Polygons != null)
                {
                    var delpolys = new List<BoundPolygon>();

                    foreach (var poly in Polygons)
                    {
                        var pverts = poly.VertexIndices;
                        for (int i = 0; i < pverts.Length; i++)
                        {
                            if (pverts[i] == index)
                            {
                                delpolys.Add(poly);
                            }
                            if (pverts[i] > index)
                            {
                                pverts[i]--;
                            }
                        }
                        poly.VertexIndices = pverts;
                    }

                    if (deletePolys)
                    {
                        foreach (var delpoly in delpolys)
                        {
                            DeletePolygon(delpoly);
                        }
                    }
                    else
                    {
                        if (delpolys.Count > 0)
                        { } //this shouldn't happen! shouldn't have deleted the vertex if it is used by polys
                    }
                }

                return true;
            }
            return false;
        }


        public int AddVertex()
        {
            var verts = Vertices?.ToList() ?? new List<Vector3>();
            var verts2 = VerticesShrunk?.ToList();
            var vertcols = VertexColours?.ToList();
            var vertobjs = VertexObjects?.ToList();
            var index = verts.Count;

            verts.Add(Vector3.Zero);
            verts2?.Add(Vector3.Zero);
            vertcols?.Add(new BoundMaterialColour());
            vertobjs?.Add(null);

            Vertices = verts.ToArray();
            VerticesShrunk = verts2?.ToArray();
            VertexColours = vertcols?.ToArray();
            VertexObjects = vertobjs?.ToArray();
            VerticesCount = (uint)verts.Count;
            VerticesShrunkCount = VerticesCount;

            return index;
        }

        public BoundPolygon AddPolygon(BoundPolygonType type)
        {
            var p = CreatePolygon(type);

            var polys = Polygons?.ToList() ?? new List<BoundPolygon>();
            var polymats = PolygonMaterialIndices?.ToList() ?? new List<byte>();

            p.Index = polys.Count;
            polys.Add(p);
            polymats.Add(0);

            Polygons = polys.ToArray();
            PolygonMaterialIndices = polymats.ToArray();
            PolygonsCount = (uint)polys.Count;

            var vinds = p.VertexIndices; //just get the required array size
            if (vinds != null)
            {
                for (int i = 0; i < vinds.Length; i++)
                {
                    vinds[i] = AddVertex();
                }
                p.VertexIndices = vinds;
            }

            return p;
        }

        private BoundPolygon CreatePolygon(BoundPolygonType type)
        {
            BoundPolygon p = null;
            switch (type)
            {
                case BoundPolygonType.Triangle:
                    p = new BoundPolygonTriangle();
                    break;
                case BoundPolygonType.Sphere:
                    p = new BoundPolygonSphere();
                    break;
                case BoundPolygonType.Capsule:
                    p = new BoundPolygonCapsule();
                    break;
                case BoundPolygonType.Box:
                    p = new BoundPolygonBox();
                    break;
                case BoundPolygonType.Cylinder:
                    p = new BoundPolygonCylinder();
                    break;
                default:
                    break;
            }
            if (p != null)
            {
                p.Owner = this;
            }
            return p;
        }
    }
    [TC(typeof(EXP))] public class BoundBVH : BoundGeometry
    {
        public override long BlockLength
        {
            get { return 336; }
        }

        // structure data
        public ulong BvhPointer { get; set; }
        public uint Unknown_138h { get; set; } // 0x00000000
        public uint Unknown_13Ch { get; set; } // 0x00000000
        public ushort Unknown_140h { get; set; } = 0xFFFF; // 0xFFFF
        public ushort Unknown_142h { get; set; } // 0x0000
        public uint Unknown_144h { get; set; } // 0x00000000
        public uint Unknown_148h { get; set; } // 0x00000000
        public uint Unknown_14Ch { get; set; } // 0x00000000

        // reference data
        public BVH BVH { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.BvhPointer = reader.ReadUInt64();
            this.Unknown_138h = reader.ReadUInt32();
            this.Unknown_13Ch = reader.ReadUInt32();
            this.Unknown_140h = reader.ReadUInt16();
            this.Unknown_142h = reader.ReadUInt16();
            this.Unknown_144h = reader.ReadUInt32();
            this.Unknown_148h = reader.ReadUInt32();
            this.Unknown_14Ch = reader.ReadUInt32();

            // read reference data
            if (this.BvhPointer > 65535)
            {
                this.BVH = reader.ReadBlockAt<BVH>(
                    this.BvhPointer // offset
                );

                //var cap = BVH.Nodes.EntriesCount;//how to calc this?
                //var diff = BVH.Nodes.EntriesCapacity - cap;
                //switch (diff)
                //{
                //    case 0:
                //    case 1:
                //    case 2:
                //        break;
                //    default:
                //        break;//no hit
                //}

            }
            else
            {
                //this can happen in some ydr's for some reason
            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.BvhPointer = (ulong)(this.BVH != null ? this.BVH.FilePosition : 0);

            // write structure data
            writer.Write(this.BvhPointer);
            writer.Write(this.Unknown_138h);
            writer.Write(this.Unknown_13Ch);
            writer.Write(this.Unknown_140h);
            writer.Write(this.Unknown_142h);
            writer.Write(this.Unknown_144h);
            writer.Write(this.Unknown_148h);
            writer.Write(this.Unknown_14Ch);
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            FileVFT = 1080228536;
        }

        public override IResourceBlock[] GetReferences()
        {
            BuildBVH(false);

            var list = new List<IResourceBlock>(base.GetReferences());
            if (BVH != null) list.Add(BVH);
            return list.ToArray();
        }




        public void BuildBVH(bool updateParent = true)
        {
            if ((Polygons?.Length ?? 0) <= 0) //in some des_ drawables?
            {
                if (BVH != null)
                { }
                BVH = null;
                return;
            }
            if (BVH != null)
            {
                //var tnodes = BVHBuilder.Unbuild(BVH);
            }

            var items = new List<BVHBuilderItem>();
            for (int i = 0; i < Polygons.Length; i++)
            {
                var poly = Polygons[i];
                if (poly != null)
                {
                    var it = new BVHBuilderItem();
                    it.Min = poly.BoxMin;
                    it.Max = poly.BoxMax;
                    it.Index = i;
                    it.Polygon = poly;
                    items.Add(it);
                }
            }

            var bvh = BVHBuilder.Build(items, 4); //geometries have BVH item threshold of 4

            var newpolys = new BoundPolygon[items.Count];
            var newpolymats = new byte[items.Count];
            var itemlookup = new short[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var poly = items[i]?.Polygon;
                if (poly != null)
                {
                    if (poly.Index < itemlookup.Length)
                    {
                        itemlookup[poly.Index] = (short)i;
                    }
                    else
                    { }//shouldn't happen
                }
                else
                { }//shouldn't happen
            }
            for (int i = 0; i < items.Count; i++)
            {
                var poly = items[i]?.Polygon;
                if (poly != null)
                {
                    newpolys[i] = poly;
                    newpolymats[i] = (poly.Index < PolygonMaterialIndices.Length) ? PolygonMaterialIndices[poly.Index] : (byte)0;//is this necessary?
                    poly.Index = i;
                    if (poly is BoundPolygonTriangle ptri)
                    {
                        ptri.edgeIndex1 = ((ptri.edgeIndex1 >= 0) && (ptri.edgeIndex1 < itemlookup.Length)) ? itemlookup[ptri.edgeIndex1] : (short)-1;
                        ptri.edgeIndex2 = ((ptri.edgeIndex2 >= 0) && (ptri.edgeIndex2 < itemlookup.Length)) ? itemlookup[ptri.edgeIndex2] : (short)-1;
                        ptri.edgeIndex3 = ((ptri.edgeIndex3 >= 0) && (ptri.edgeIndex3 < itemlookup.Length)) ? itemlookup[ptri.edgeIndex3] : (short)-1;
                    }
                }
            }
            Polygons = newpolys;
            PolygonMaterialIndices = newpolymats;

            BoxMin = bvh.BoundingBoxMin.XYZ();
            BoxMax = bvh.BoundingBoxMax.XYZ();
            BoxCenter = bvh.BoundingBoxCenter.XYZ();
            SphereCenter = BoxCenter;
            SphereRadius = (BoxMax - BoxCenter).Length();

            BVH = bvh;

            if (updateParent && (Parent != null)) //only update parent when live editing in world view!
            {
                Parent.BuildBVH();
            }
        }



        public override SpaceSphereIntersectResult SphereIntersect(ref BoundingSphere sph)
        {
            var res = new SpaceSphereIntersectResult();
            var box = new BoundingBox();

            if (Polygons == null)
            { return res; }
            if ((BVH?.Nodes?.data_items == null) || (BVH?.Trees?.data_items == null))
            { return res; }

            box.Minimum = BoxMin;
            box.Maximum = BoxMax;
            if (!sph.Intersects(ref box))
            { return res; }

            var q = BVH.Quantum.XYZ();
            var c = BVH.BoundingBoxCenter.XYZ();
            for (int t = 0; t < BVH.Trees.data_items.Length; t++)
            {
                var tree = BVH.Trees.data_items[t];
                box.Minimum = tree.Min * q + c;
                box.Maximum = tree.Max * q + c;
                if (!sph.Intersects(ref box))
                { continue; }

                int nodeind = tree.NodeIndex1;
                int lastind = tree.NodeIndex2;
                while (nodeind < lastind)
                {
                    var node = BVH.Nodes.data_items[nodeind];
                    box.Minimum = node.Min * q + c;
                    box.Maximum = node.Max * q + c;
                    bool nodehit = sph.Intersects(ref box);
                    bool nodeskip = !nodehit;
                    if (node.ItemCount <= 0) //intermediate node with child nodes
                    {
                        if (nodeskip)
                        {
                            nodeind += node.ItemId; //(child node count)
                        }
                        else
                        {
                            nodeind++;
                        }
                    }
                    else //leaf node, with polygons
                    {
                        if (!nodeskip)
                        {
                            var lastp = Math.Min(node.ItemId + node.ItemCount, (int)PolygonsCount);

                            SphereIntersectPolygons(ref sph, ref res, node.ItemId, lastp);

                        }
                        nodeind++;
                    }
                    res.TestedNodeCount++;
                }
            }

            return res;
        }
        public override SpaceRayIntersectResult RayIntersect(ref Ray ray, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult();
            var box = new BoundingBox();

            if (Polygons == null)
            { return res; }
            if ((BVH?.Nodes?.data_items == null) || (BVH?.Trees?.data_items == null))
            { return res; }

            box.Minimum = BoxMin;
            box.Maximum = BoxMax;
            float bvhboxhittest;
            if (!ray.Intersects(ref box, out bvhboxhittest))
            { return res; }
            if (bvhboxhittest > maxdist)
            { return res; } //already a closer hit.

            res.HitDist = maxdist;

            var q = BVH.Quantum.XYZ();
            var c = BVH.BoundingBoxCenter.XYZ();
            for (int t = 0; t < BVH.Trees.data_items.Length; t++)
            {
                var tree = BVH.Trees.data_items[t];
                box.Minimum = tree.Min * q + c;
                box.Maximum = tree.Max * q + c;
                if (!ray.Intersects(ref box, out bvhboxhittest))
                { continue; }
                if (bvhboxhittest > res.HitDist)
                { continue; } //already a closer hit.

                int nodeind = tree.NodeIndex1;
                int lastind = tree.NodeIndex2;
                while (nodeind < lastind)
                {
                    var node = BVH.Nodes.data_items[nodeind];
                    box.Minimum = node.Min * q + c;
                    box.Maximum = node.Max * q + c;
                    bool nodehit = ray.Intersects(ref box, out bvhboxhittest);
                    bool nodeskip = !nodehit || (bvhboxhittest > res.HitDist);
                    if (node.ItemCount <= 0) //intermediate node with child nodes
                    {
                        if (nodeskip)
                        {
                            nodeind += node.ItemId; //(child node count)
                        }
                        else
                        {
                            nodeind++;
                        }
                    }
                    else //leaf node, with polygons
                    {
                        if (!nodeskip)
                        {
                            var lastp = Math.Min(node.ItemId + node.ItemCount, (int)PolygonsCount);

                            RayIntersectPolygons(ref ray, ref res, node.ItemId, lastp);

                        }
                        nodeind++;
                    }
                    res.TestedNodeCount++;
                }
            }

            return res;
        }

    }
    [TC(typeof(EXP))] public class BoundComposite : Bounds
    {
        public override long BlockLength
        {
            get { return 176; }
        }

        // structure data
        public ulong ChildrenPointer { get; set; }
        public ulong ChildrenTransformation1Pointer { get; set; }
        public ulong ChildrenTransformation2Pointer { get; set; }
        public ulong ChildrenBoundingBoxesPointer { get; set; }
        public ulong ChildrenFlags1Pointer { get; set; }
        public ulong ChildrenFlags2Pointer { get; set; }
        public ushort ChildrenCount1 { get; set; }
        public ushort ChildrenCount2 { get; set; }
        public uint Unknown_A4h { get; set; } // 0x00000000
        public ulong BVHPointer { get; set; }

        // reference data
        public ResourcePointerArray64<Bounds> Children { get; set; }
        public Matrix4F_s[] ChildrenTransformation1 { get; set; }
        public Matrix4F_s[] ChildrenTransformation2 { get; set; }
        public AABB_s[] ChildrenBoundingBoxes { get; set; }
        public BoundCompositeChildrenFlags[] ChildrenFlags1 { get; set; }
        public BoundCompositeChildrenFlags[] ChildrenFlags2 { get; set; }

        public BVH BVH { get; set; }


        private ResourceSystemStructBlock<Matrix4F_s> ChildrenTransformation1Block = null;
        private ResourceSystemStructBlock<Matrix4F_s> ChildrenTransformation2Block = null;
        private ResourceSystemStructBlock<AABB_s> ChildrenBoundingBoxesBlock = null;
        private ResourceSystemStructBlock<BoundCompositeChildrenFlags> ChildrenFlags1Block = null;
        private ResourceSystemStructBlock<BoundCompositeChildrenFlags> ChildrenFlags2Block = null;


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.ChildrenPointer = reader.ReadUInt64();
            this.ChildrenTransformation1Pointer = reader.ReadUInt64();
            this.ChildrenTransformation2Pointer = reader.ReadUInt64();
            this.ChildrenBoundingBoxesPointer = reader.ReadUInt64();
            this.ChildrenFlags1Pointer = reader.ReadUInt64();
            this.ChildrenFlags2Pointer = reader.ReadUInt64();
            this.ChildrenCount1 = reader.ReadUInt16();
            this.ChildrenCount2 = reader.ReadUInt16();
            this.Unknown_A4h = reader.ReadUInt32();
            this.BVHPointer = reader.ReadUInt64();

            // read reference data
            this.Children = reader.ReadBlockAt<ResourcePointerArray64<Bounds>>(this.ChildrenPointer, this.ChildrenCount1);
            this.ChildrenTransformation1 = reader.ReadStructsAt<Matrix4F_s>(this.ChildrenTransformation1Pointer, this.ChildrenCount1);
            this.ChildrenTransformation2 = reader.ReadStructsAt<Matrix4F_s>(this.ChildrenTransformation2Pointer, this.ChildrenCount1);
            this.ChildrenBoundingBoxes = reader.ReadStructsAt<AABB_s>(this.ChildrenBoundingBoxesPointer, this.ChildrenCount1);
            this.ChildrenFlags1 = reader.ReadStructsAt<BoundCompositeChildrenFlags>(this.ChildrenFlags1Pointer, this.ChildrenCount1);
            this.ChildrenFlags2 = reader.ReadStructsAt<BoundCompositeChildrenFlags>(this.ChildrenFlags2Pointer, this.ChildrenCount1);
            this.BVH = reader.ReadBlockAt<BVH>(this.BVHPointer);

            //if (BVH != null)
            //{
            //    var cap = Math.Max(BVH.Nodes.EntriesCount + 2, ChildrenCount1 * 2 + 1);
            //    var diff = BVH.Nodes.EntriesCapacity - cap;
            //    if (diff != 0)
            //    { }//no hit
            //}

            var childTransforms = ChildrenTransformation1 ?? ChildrenTransformation2;
            if ((Children != null) && (Children.data_items != null))
            {
                for (int i = 0; i < Children.data_items.Length; i++)
                {
                    var child = Children.data_items[i];
                    if (child != null)
                    {
                        child.Parent = this;

                        var xform = ((childTransforms != null) && (i < childTransforms.Length)) ? childTransforms[i].ToMatrix() : Matrix.Identity;
                        child.Transform = xform;
                        child.TransformInv = Matrix.Invert(xform);
                        child.CompositeFlags1 = ((ChildrenFlags1 != null) && (i < ChildrenFlags1.Length)) ? ChildrenFlags1[i] : new BoundCompositeChildrenFlags();
                        child.CompositeFlags2 = ((ChildrenFlags2 != null) && (i < ChildrenFlags2.Length)) ? ChildrenFlags2[i] : new BoundCompositeChildrenFlags();

                        //if ((child.CompositeFlags1.Flags1 != child.CompositeFlags2.Flags1) || (child.CompositeFlags1.Flags2 != child.CompositeFlags2.Flags2))
                        //{ } //no hits
                    }
                }
            }

            //if (ChildrenTransformation1 != null)
            //{
            //    foreach (var m in ChildrenTransformation1)
            //    {
            //        switch (m.Flags1)
            //        {
            //            case 0:
            //            case 0x7f800001: //only in yft's!
            //            case 0x80000000: //only in yft's! = -0
            //                break;
            //            default:
            //                break;//no hit
            //        }
            //        switch (m.Flags2)
            //        {
            //            case 1:
            //            case 0: //only in yft's!
            //            case 0x7f800001: //only in yft's!
            //            case 0x80000000: //only in yft's! = -0
            //                break;
            //            default:
            //                break;//no hit
            //        }
            //        switch (m.Flags3)
            //        {
            //            case 1:
            //            case 0: //only in yft's!
            //            case 0x7f800001: //only in yft's!
            //            case 0x80000000: //only in yft's! = -0
            //                break;
            //            default:
            //                break;//no hit
            //        }
            //        switch (m.Flags4)
            //        {
            //            case 0:
            //            case 0x7f800001: //only in yft's!
            //            case 0x42f9c6a4: //only in yft!  = 124.888f
            //            case 0x42f94c3a: //only in yft!  = 124.649f
            //            case 0x42f33400: //only in yft!  = 121.602f
            //            case 0x42f4793c: //only in yft!  = 122.237f
            //            case 0x42f2d6a2: //only in yft!  = 121.419f
            //            case 0x42eca103:
            //            case 0x42ede882:
            //            case 0x42e6ec74:
            //            case 0x42e26655:
            //            case 0x42e2ac9e:
            //            case 0x42eb1c90:
            //            case 0x4474bd4d:
            //            case 0x433b2027:
            //                break;
            //            default:
            //                break;//+ more! in yft's! it really seems like a float...
            //        }
            //    }
            //}


            //if ((ChildrenTransformation1 != null) && (ChildrenTransformation2 != null))
            //{
            //    //if (ChildrenTransformation1.Length != ChildrenTransformation2.Length)
            //    //{ }//no hits
            //    //else
            //    //{
            //    //    for (int i = 0; i < ChildrenTransformation1.Length; i++)
            //    //    {
            //    //        if (ChildrenTransformation1[i] != ChildrenTransformation2[i])
            //    //        { }//no hits
            //    //    }
            //    //}
            //}
            //else
            //{ }//no hits
            //if (ChildrenFlags1 != null)
            //{ }
            //else
            //{ }//some props ydr's
            //if (ChildrenFlags2 != null)
            //{ }
            //else
            //{ }//some props ydr's

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.ChildrenPointer = (ulong)(this.Children != null ? this.Children.FilePosition : 0);
            this.ChildrenTransformation1Pointer = (ulong)(this.ChildrenTransformation1Block != null ? this.ChildrenTransformation1Block.FilePosition : 0);
            this.ChildrenTransformation2Pointer = (ulong)(this.ChildrenTransformation2Block != null ? this.ChildrenTransformation2Block.FilePosition : (long)ChildrenTransformation1Pointer);
            this.ChildrenBoundingBoxesPointer = (ulong)(this.ChildrenBoundingBoxesBlock != null ? this.ChildrenBoundingBoxesBlock.FilePosition : 0);
            this.ChildrenFlags1Pointer = (ulong)(this.ChildrenFlags1Block != null ? this.ChildrenFlags1Block.FilePosition : 0);
            this.ChildrenFlags2Pointer = (ulong)(this.ChildrenFlags2Block != null ? this.ChildrenFlags2Block.FilePosition : 0);
            this.ChildrenCount1 = (ushort)(this.Children != null ? this.Children.Count : 0);
            this.ChildrenCount2 = (ushort)(this.Children != null ? this.Children.Count : 0);
            this.BVHPointer = (ulong)(this.BVH != null ? this.BVH.FilePosition : 0);

            // write structure data
            writer.Write(this.ChildrenPointer);
            writer.Write(this.ChildrenTransformation1Pointer);
            writer.Write(this.ChildrenTransformation2Pointer);
            writer.Write(this.ChildrenBoundingBoxesPointer);
            writer.Write(this.ChildrenFlags1Pointer);
            writer.Write(this.ChildrenFlags2Pointer);
            writer.Write(this.ChildrenCount1);
            writer.Write(this.ChildrenCount2);
            writer.Write(this.Unknown_A4h);
            writer.Write(this.BVHPointer);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            var c = Children?.data_items;
            if ((c == null) || (c.Length == 0))
            {
                YbnXml.SelfClosingTag(sb, indent, "Children");
            }
            else
            {
                var cind = indent + 1;
                YbnXml.OpenTag(sb, indent, "Children");
                foreach (var child in c)
                {
                    Bounds.WriteXmlNode(child, sb, cind, "Item");
                }
                YbnXml.CloseTag(sb, indent, "Children");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            var cnode = node.SelectSingleNode("Children");
            if (cnode != null)
            {
                var cnodes = cnode.SelectNodes("Item");
                if (cnodes?.Count > 0)
                {
                    var blist = new List<Bounds>();
                    foreach (XmlNode inode in cnodes)
                    {
                        var b = Bounds.ReadXmlNode(inode, Owner, this);
                        blist.Add(b);
                    }
                    var arr = blist.ToArray();
                    Children = new ResourcePointerArray64<Bounds>();
                    Children.data_items = arr;

                    BuildBVH();
                    UpdateChildrenFlags();
                    UpdateChildrenBounds();
                    UpdateChildrenTransformations();
                }
            }

            FileVFT = 1080212136;
        }

        public override IResourceBlock[] GetReferences()
        {
            BuildBVH();
            UpdateChildrenFlags();
            UpdateChildrenBounds();
            UpdateChildrenTransformations();

            var list = new List<IResourceBlock>(base.GetReferences());
            if (Children != null) list.Add(Children);
            if (ChildrenTransformation1 != null)
            {
                ChildrenTransformation1Block = new ResourceSystemStructBlock<Matrix4F_s>(ChildrenTransformation1);
                list.Add(ChildrenTransformation1Block);
            }
            if (ChildrenTransformation2 != null)
            {
                ChildrenTransformation2Block = new ResourceSystemStructBlock<Matrix4F_s>(ChildrenTransformation2);
                list.Add(ChildrenTransformation2Block);
            }
            if (ChildrenBoundingBoxes != null)
            {
                ChildrenBoundingBoxesBlock = new ResourceSystemStructBlock<AABB_s>(ChildrenBoundingBoxes);
                list.Add(ChildrenBoundingBoxesBlock);
            }
            if (ChildrenFlags1 != null)
            {
                ChildrenFlags1Block = new ResourceSystemStructBlock<BoundCompositeChildrenFlags>(ChildrenFlags1);
                list.Add(ChildrenFlags1Block);
            }
            if (ChildrenFlags2 != null)
            {
                ChildrenFlags2Block = new ResourceSystemStructBlock<BoundCompositeChildrenFlags>(ChildrenFlags2);
                list.Add(ChildrenFlags2Block);
            }
            if (BVH != null) list.Add(BVH);
            return list.ToArray();
        }




        public void BuildBVH()
        {

            if (Children?.data_items == null)
            {
                BVH = null;
                return;
            }
            if (Children.data_items.Length <= 5) //composites only get BVHs if they have 6 or more children.
            {
                if (BVH != null)
                { }
                BVH = null;
                return;
            }
            if (BVH != null)
            {
                //var tnodes = BVHBuilder.Unbuild(BVH);
            }
            else
            {
                //why are we here? yft's hit this... (and when loading XML!)
                if (!(Owner is FragPhysicsLOD) && !(Owner is FragPhysArchetype) && !(Owner is VerletCloth))
                { }
            }
            if (Owner is FragPhysArchetype fpa)
            {
                if (fpa == fpa.Owner?.Archetype2) //for destroyed yft archetype, don't use a BVH.
                {
                    BVH = null;
                    return;
                }
            }

            var items = new List<BVHBuilderItem>();
            for (int i = 0; i < Children.data_items.Length; i++)
            {
                var child = Children.data_items[i];
                if (child != null)
                {
                    var cbox = new BoundingBox(child.BoxMin, child.BoxMax);
                    var tcbox = cbox.Transform(child.Transform);
                    var it = new BVHBuilderItem();
                    it.Min = tcbox.Minimum;
                    it.Max = tcbox.Maximum;
                    it.Index = i;
                    it.Bounds = child;
                    items.Add(it);
                }
                else
                {
                    items.Add(null);//items need to have correct count to set the correct capacity for the BVH!
                }
            }

            BVH = BVHBuilder.Build(items, 1); //composites have BVH item threshold of 1
        }

        public void UpdateChildrenFlags()
        {
            if (Children?.data_items == null)
            {
                ChildrenFlags1 = null;
                ChildrenFlags2 = null;
                return;
            }
            if (OwnerIsFragment)//don't use flags in fragments
            {
                ChildrenFlags1 = null;
                ChildrenFlags2 = null;
                return;
            }

            var cf1 = new List<BoundCompositeChildrenFlags>();
            var cf2 = new List<BoundCompositeChildrenFlags>();

            foreach (var child in Children.data_items)
            {
                var f1 = new BoundCompositeChildrenFlags();
                var f2 = new BoundCompositeChildrenFlags();
                if (child != null)
                {
                    f1 = child.CompositeFlags1;
                    f2 = child.CompositeFlags2;
                }
                cf1.Add(f1);
                cf2.Add(f2);
            }

            ChildrenFlags1 = cf1.ToArray();
            ChildrenFlags2 = cf2.ToArray();
        }

        public void UpdateChildrenBounds()
        {
            if (Children?.data_items == null)
            {
                ChildrenBoundingBoxes = null;
                return;
            }

            var cbl = new List<AABB_s>();
            foreach (var child in Children.data_items)
            {
                var aabb = new AABB_s();
                if (child != null)
                {
                    aabb.Min = new Vector4(child.BoxMin, float.Epsilon);
                    aabb.Max = new Vector4(child.BoxMax, child.Margin);
                }
                cbl.Add(aabb);
            }

            ChildrenBoundingBoxes = cbl.ToArray();

        }

        public void UpdateChildrenTransformations()
        {
            if (Children?.data_items == null)
            {
                ChildrenTransformation1 = null;
                ChildrenTransformation2 = null;
                return;
            }

            var ct1 = new List<Matrix4F_s>();
            foreach (var child in Children.data_items)
            {
                var m = Matrix4F_s.Identity;
                if (child != null)
                {
                    m = new Matrix4F_s(child.Transform);
                }

                if (OwnerIsFragment)
                {
                    m.Flags1 = 0x7f800001;
                    m.Flags2 = 0x7f800001;
                    m.Flags3 = 0x7f800001;
                    m.Flags4 = 0x7f800001;
                }
                else
                {
                    //m.Column4 = new Vector4(0.0f, float.Epsilon, float.Epsilon, 0.0f);//is this right? TODO: check!
                    m.Flags1 = 0;
                    m.Flags2 = 1;
                    m.Flags3 = 1;
                    m.Flags4 = 0;
                }

                ct1.Add(m);
            }

            ChildrenTransformation1 = ct1.ToArray();
            ChildrenTransformation2 = null;

        }


        public override SpaceSphereIntersectResult SphereIntersect(ref BoundingSphere sph)
        {
            var res = new SpaceSphereIntersectResult();
            var tsph = sph;

            var compchilds = Children?.data_items;
            if (compchilds == null)
            { return res; }

            for (int i = 0; i < compchilds.Length; i++)
            {
                var c = compchilds[i];
                if (c == null) continue;

                tsph.Center = c.TransformInv.Multiply(sph.Center);

                var chit = c.SphereIntersect(ref tsph);

                chit.Normal = c.Transform.MultiplyRot(chit.Normal);

                res.TryUpdate(ref chit);
            }

            return res;
        }
        public override SpaceRayIntersectResult RayIntersect(ref Ray ray, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult();
            res.HitDist = maxdist;

            var tray = ray;

            var compchilds = Children?.data_items;
            if (compchilds == null)
            { return res; }

            for (int i = 0; i < compchilds.Length; i++)
            {
                var c = compchilds[i];
                if (c == null) continue;

                tray.Position = c.TransformInv.Multiply(ray.Position);
                tray.Direction = c.TransformInv.MultiplyRot(ray.Direction);

                var chit = c.RayIntersect(ref tray, res.HitDist);

                chit.Position = c.Transform.Multiply(chit.Position);
                chit.Normal = c.Transform.MultiplyRot(chit.Normal);

                res.TryUpdate(ref chit);
            }

            return res;
        }




        public bool DeleteChild(Bounds child)
        {
            if (Children?.data_items != null)
            {
                var children = Children.data_items.ToList();
                var transforms1 = ChildrenTransformation1?.ToList();
                var transforms2 = ChildrenTransformation2?.ToList();
                var bboxes = ChildrenBoundingBoxes?.ToList();
                var flags1 = ChildrenFlags1?.ToList();
                var flags2 = ChildrenFlags2?.ToList();
                var idx = children.IndexOf(child);
                if (idx >= 0)
                {
                    children.RemoveAt(idx);
                    transforms1?.RemoveAt(idx);
                    transforms2?.RemoveAt(idx);
                    bboxes?.RemoveAt(idx);
                    flags1?.RemoveAt(idx);
                    flags2?.RemoveAt(idx);
                    Children.data_items = children.ToArray();
                    ChildrenTransformation1 = transforms1?.ToArray();
                    ChildrenTransformation2 = transforms2?.ToArray();
                    ChildrenBoundingBoxes = bboxes?.ToArray();
                    ChildrenFlags1 = flags1?.ToArray();
                    ChildrenFlags2 = flags2?.ToArray();
                    BuildBVH();
                    return true;
                }
            }
            return false;
        }

        public void AddChild(Bounds child)
        {
            if (Children == null) Children = new ResourcePointerArray64<Bounds>();

            var children = Children.data_items?.ToList() ?? new List<Bounds>();
            var transforms1 = ChildrenTransformation1?.ToList() ?? new List<Matrix4F_s>();
            var transforms2 = ChildrenTransformation2?.ToList() ?? new List<Matrix4F_s>();
            var bboxes = ChildrenBoundingBoxes?.ToList() ?? new List<AABB_s>();
            var flags1 = ChildrenFlags1?.ToList();
            var flags2 = ChildrenFlags2?.ToList();
            var idx = children.Count;

            child.Parent = this;

            children.Add(child);
            transforms1.Add(Matrix4F_s.Identity);
            transforms2.Add(Matrix4F_s.Identity);
            bboxes.Add(new AABB_s());//will get updated later
            flags1?.Add(new BoundCompositeChildrenFlags());
            flags2?.Add(new BoundCompositeChildrenFlags());
            Children.data_items = children.ToArray();
            ChildrenTransformation1 = transforms1.ToArray();
            ChildrenTransformation2 = transforms2.ToArray();
            ChildrenBoundingBoxes = bboxes.ToArray();
            ChildrenFlags1 = flags1?.ToArray();
            ChildrenFlags2 = flags2?.ToArray();
            BuildBVH();
            UpdateChildrenBounds();

        }

    }




    public enum BoundPolygonType : byte
    {
        Triangle = 0,
        Sphere = 1,
        Capsule = 2,
        Box = 3,
        Cylinder = 4,
    }
    [TC(typeof(EXP))] public abstract class BoundPolygon : IMetaXmlItem
    {
        public BoundPolygonType Type { get; set; }
        public BoundGeometry Owner { get; set; } //for browsing/editing convenience
        public BoundMaterial_s Material
        {
            get
            {
                if (MaterialCustom.HasValue) return MaterialCustom.Value;
                return Owner?.GetMaterial(Index) ?? new BoundMaterial_s();
            }
            set
            {
                MaterialCustom = value;
            }
        }
        public BoundMaterial_s? MaterialCustom; //for editing, when assigning a new material.
        public int MaterialIndex
        {
            get { return Owner?.GetMaterialIndex(Index) ?? -1; }
        }
        public Vector3[] VertexPositions
        {
            get
            {
                var inds = VertexIndices;
                var va = new Vector3[inds.Length];
                if (Owner != null)
                {
                    for (int i = 0; i < inds.Length; i++)
                    {
                        va[i] = Owner.GetVertexPos(inds[i]);
                    }
                }
                return va;
            }
            set
            {
                if (value == null) return;
                var inds = VertexIndices;
                if (Owner != null)
                {
                    var imax = Math.Min(inds.Length, value.Length);
                    for (int i = 0; i < imax; i++)
                    {
                        Owner.SetVertexPos(inds[i], value[i]);
                    }
                }
            }
        }
        public int Index { get; set; } //for editing convenience, not stored
        public abstract Vector3 BoxMin { get; }
        public abstract Vector3 BoxMax { get; }
        public abstract Vector3 Scale { get; set; }
        public abstract Vector3 Position { get; set; }
        public abstract Quaternion Orientation { get; set; }
        public abstract int[] VertexIndices { get; set; }
        public abstract BoundVertexRef NearestVertex(Vector3 p);
        public abstract void GatherVertices(Dictionary<BoundVertex, int> verts);
        public abstract void Read(byte[] bytes, int offset);
        public abstract void Write(BinaryWriter bw);
        public abstract void WriteXml(StringBuilder sb, int indent);
        public abstract void ReadXml(XmlNode node);
        public virtual string Title
        {
            get
            {
                return Type.ToString() + " " + Index.ToString();
            }
        }
        public override string ToString()
        {
            return Type.ToString();
        }
    }
    [TC(typeof(EXP))] public class BoundPolygonTriangle : BoundPolygon
    {
        public float triArea { get; set; }
        public ushort triIndex1 { get; set; }
        public ushort triIndex2 { get; set; }
        public ushort triIndex3 { get; set; }
        public short edgeIndex1 { get; set; }
        public short edgeIndex2 { get; set; }
        public short edgeIndex3 { get; set; }

        public int vertIndex1 { get { return (triIndex1 & 0x7FFF); } set { triIndex1 = (ushort)((value & 0x7FFF) + (vertFlag1 ? 0x8000 : 0)); } }
        public int vertIndex2 { get { return (triIndex2 & 0x7FFF); } set { triIndex2 = (ushort)((value & 0x7FFF) + (vertFlag2 ? 0x8000 : 0)); } }
        public int vertIndex3 { get { return (triIndex3 & 0x7FFF); } set { triIndex3 = (ushort)((value & 0x7FFF) + (vertFlag3 ? 0x8000 : 0)); } }
        public bool vertFlag1 { get { return (triIndex1 & 0x8000) > 0; } set { triIndex1 = (ushort)(vertIndex1 + (value ? 0x8000 : 0)); } }
        public bool vertFlag2 { get { return (triIndex2 & 0x8000) > 0; } set { triIndex2 = (ushort)(vertIndex2 + (value ? 0x8000 : 0)); } }
        public bool vertFlag3 { get { return (triIndex3 & 0x8000) > 0; } set { triIndex3 = (ushort)(vertIndex3 + (value ? 0x8000 : 0)); } }

        public Vector3 Vertex1
        {
            get { return (Owner != null) ? Owner.GetVertexPos(vertIndex1) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(vertIndex1, value); }
        }
        public Vector3 Vertex2
        {
            get { return (Owner != null) ? Owner.GetVertexPos(vertIndex2) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(vertIndex2, value); }
        }
        public Vector3 Vertex3
        {
            get { return (Owner != null) ? Owner.GetVertexPos(vertIndex3) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(vertIndex3, value); }
        }

        public override Vector3 BoxMin
        {
            get
            {
                return Vector3.Min(Vector3.Min(Vertex1, Vertex2), Vertex3);
            }
        }
        public override Vector3 BoxMax
        {
            get
            {
                return Vector3.Max(Vector3.Max(Vertex1, Vertex2), Vertex3);
            }
        }
        public override Vector3 Scale
        {
            get
            {
                if (ScaleCached.HasValue) return ScaleCached.Value;
                ScaleCached = Vector3.One;
                return Vector3.One;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var cen = (v1 + v2 + v3) * (1.0f / 3.0f);
                var trans = value / Scale;
                var ori = Orientation;
                var orinv = Quaternion.Invert(ori);
                Vertex1 = cen + ori.Multiply(trans * orinv.Multiply(v1 - cen));
                Vertex2 = cen + ori.Multiply(trans * orinv.Multiply(v2 - cen));
                Vertex3 = cen + ori.Multiply(trans * orinv.Multiply(v3 - cen));
                ScaleCached = value;
            }
        }
        public override Vector3 Position
        {
            get
            {
                return (Vertex1 + Vertex2 + Vertex3) * (1.0f / 3.0f);
            }
            set
            {
                var offset = value - Position;
                Vertex1 += offset;
                Vertex2 += offset;
                Vertex3 += offset;
            }
        }
        public override Quaternion Orientation
        {
            get
            {
                if (OrientationCached.HasValue) return OrientationCached.Value;
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var dir = v2 - v1;
                var side = Vector3.Cross((v3 - v1), dir);
                var up = Vector3.Normalize(Vector3.Cross(dir, side));
                var ori = Quaternion.Invert(Quaternion.LookAtRH(Vector3.Zero, side, up));
                OrientationCached = ori;
                return ori;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var cen = (v1 + v2 + v3) * (1.0f / 3.0f);
                var trans = value * Quaternion.Invert(Orientation);
                Vertex1 = cen + trans.Multiply(v1 - cen);
                Vertex2 = cen + trans.Multiply(v2 - cen);
                Vertex3 = cen + trans.Multiply(v3 - cen);
                OrientationCached = value;
            }
        }
        private Quaternion? OrientationCached;
        private Vector3? ScaleCached;

        public override int[] VertexIndices
        {
            get
            {
                return new[] { vertIndex1, vertIndex2, vertIndex3 };
            }
            set
            {
                if (value?.Length >= 3)
                {
                    vertIndex1 = value[0];
                    vertIndex2 = value[1];
                    vertIndex3 = value[2];
                }
            }
        }
        public override BoundVertexRef NearestVertex(Vector3 p)
        {
            var d1 = (p - Vertex1).Length();
            var d2 = (p - Vertex2).Length();
            var d3 = (p - Vertex3).Length();
            if ((d1 <= d2) && (d1 <= d3)) return new BoundVertexRef(vertIndex1, d1);
            if (d2 <= d3) return new BoundVertexRef(vertIndex2, d2);
            return new BoundVertexRef(vertIndex3, d3);
        }
        public override void GatherVertices(Dictionary<BoundVertex, int> verts)
        {
            if (Owner != null)
            {
                verts[Owner.GetVertexObject(vertIndex1)] = vertIndex1;
                verts[Owner.GetVertexObject(vertIndex2)] = vertIndex2;
                verts[Owner.GetVertexObject(vertIndex3)] = vertIndex3;
            }
        }

        public void SetEdgeIndex(int edgeid, short polyindex)
        {
            switch (edgeid)
            {
                case 1:
                    if (edgeIndex1 != polyindex)
                    { }
                    edgeIndex1 = polyindex;
                    break;
                case 2:
                    if (edgeIndex2 != polyindex)
                    { }
                    edgeIndex2 = polyindex;
                    break;
                case 3:
                    if (edgeIndex3 != polyindex)
                    { }
                    edgeIndex3 = polyindex;
                    break;
                default:
                    break;
            }
        }

        public BoundPolygonTriangle()
        {
            Type = BoundPolygonType.Triangle;
        }
        public override void Read(byte[] bytes, int offset)
        {
            triArea = BitConverter.ToSingle(bytes, offset + 0);
            triIndex1 = BitConverter.ToUInt16(bytes, offset + 4);
            triIndex2 = BitConverter.ToUInt16(bytes, offset + 6);
            triIndex3 = BitConverter.ToUInt16(bytes, offset + 8);
            edgeIndex1 = BitConverter.ToInt16(bytes, offset + 10);
            edgeIndex2 = BitConverter.ToInt16(bytes, offset + 12);
            edgeIndex3 = BitConverter.ToInt16(bytes, offset + 14);
        }
        public override void Write(BinaryWriter bw)
        {
            bw.Write(triArea);
            bw.Write(triIndex1);
            bw.Write(triIndex2);
            bw.Write(triIndex3);
            bw.Write(edgeIndex1);
            bw.Write(edgeIndex2);
            bw.Write(edgeIndex3);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            var s = string.Format("{0} m=\"{1}\" v1=\"{2}\" v2=\"{3}\" v3=\"{4}\" f1=\"{5}\" f2=\"{6}\" f3=\"{7}\"", 
                Type,
                MaterialIndex,
                vertIndex1, 
                vertIndex2, 
                vertIndex3,
                vertFlag1 ? 1 : 0,
                vertFlag2 ? 1 : 0,
                vertFlag3 ? 1 : 0
                );
            YbnXml.SelfClosingTag(sb, indent, s);
        }
        public override void ReadXml(XmlNode node)
        {
            Material = Owner?.GetMaterialByIndex(Xml.GetIntAttribute(node, "m")) ?? new BoundMaterial_s();
            vertIndex1 = Xml.GetIntAttribute(node, "v1");
            vertIndex2 = Xml.GetIntAttribute(node, "v2");
            vertIndex3 = Xml.GetIntAttribute(node, "v3");
            vertFlag1 = Xml.GetIntAttribute(node, "f1") != 0;
            vertFlag2 = Xml.GetIntAttribute(node, "f2") != 0;
            vertFlag3 = Xml.GetIntAttribute(node, "f3") != 0;
        }
        public override string ToString()
        {
            return base.ToString() + ": " + vertIndex1.ToString() + ", " + vertIndex2.ToString() + ", " + vertIndex3.ToString();
        }
    }
    [TC(typeof(EXP))] public class BoundPolygonSphere : BoundPolygon
    {
        public ushort sphereType { get; set; }
        public ushort sphereIndex { get; set; }
        public float sphereRadius { get; set; }
        public uint unused0 { get; set; }
        public uint unused1 { get; set; }

        public override Vector3 BoxMin
        {
            get
            {
                return Position - sphereRadius;
            }
        }
        public override Vector3 BoxMax
        {
            get
            {
                return Position + sphereRadius;
            }
        }
        public override Vector3 Scale
        {
            get
            {
                return new Vector3(sphereRadius);
            }
            set
            {
                sphereRadius = value.X;
            }
        }
        public override Vector3 Position
        {
            get { return (Owner != null) ? Owner.GetVertexPos(sphereIndex) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(sphereIndex, value); }
        }
        public override Quaternion Orientation
        {
            get
            {
                return Quaternion.Identity;
            }
            set
            {
            }
        }

        public override int[] VertexIndices
        {
            get
            {
                return new[] { (int)sphereIndex };
            }
            set
            {
                if (value?.Length >= 1)
                {
                    sphereIndex = (ushort)value[0];
                }
            }
        }
        public override BoundVertexRef NearestVertex(Vector3 p)
        {
            return new BoundVertexRef(sphereIndex, sphereRadius);
        }
        public override void GatherVertices(Dictionary<BoundVertex, int> verts)
        {
            if (Owner != null)
            {
                verts[Owner.GetVertexObject(sphereIndex)] = sphereIndex;
            }
        }

        public BoundPolygonSphere()
        {
            Type = BoundPolygonType.Sphere;
        }
        public override void Read(byte[] bytes, int offset)
        {
            sphereType = BitConverter.ToUInt16(bytes, offset + 0);
            sphereIndex = BitConverter.ToUInt16(bytes, offset + 2);
            sphereRadius = BitConverter.ToSingle(bytes, offset + 4);
            unused0 = BitConverter.ToUInt32(bytes, offset + 8);
            unused1 = BitConverter.ToUInt32(bytes, offset + 12);
        }
        public override void Write(BinaryWriter bw)
        {
            bw.Write(sphereType);
            bw.Write(sphereIndex);
            bw.Write(sphereRadius);
            bw.Write(unused0);
            bw.Write(unused1);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            var s = string.Format("{0} m=\"{1}\" v=\"{2}\" radius=\"{3}\"",
                Type,
                MaterialIndex,
                sphereIndex,
                FloatUtil.ToString(sphereRadius)
                );
            YbnXml.SelfClosingTag(sb, indent, s);
        }
        public override void ReadXml(XmlNode node)
        {
            Material = Owner?.GetMaterialByIndex(Xml.GetIntAttribute(node, "m")) ?? new BoundMaterial_s();
            sphereIndex = (ushort)Xml.GetUIntAttribute(node, "v");
            sphereRadius = Xml.GetFloatAttribute(node, "radius");
        }
        public override string ToString()
        {
            return base.ToString() + ": " + sphereIndex.ToString() + ", " + sphereRadius.ToString();
        }
    }
    [TC(typeof(EXP))] public class BoundPolygonCapsule : BoundPolygon
    {
        public ushort capsuleType { get; set; }
        public ushort capsuleIndex1 { get; set; }
        public float capsuleRadius { get; set; }
        public ushort capsuleIndex2 { get; set; }
        public ushort unused0 { get; set; }
        public uint unused1 { get; set; }

        public Vector3 Vertex1
        {
            get { return (Owner != null) ? Owner.GetVertexPos(capsuleIndex1) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(capsuleIndex1, value); }
        }
        public Vector3 Vertex2
        {
            get { return (Owner != null) ? Owner.GetVertexPos(capsuleIndex2) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(capsuleIndex2, value); }
        }

        public override Vector3 BoxMin
        {
            get
            {
                return Vector3.Min(Vertex1, Vertex2) - capsuleRadius;
            }
        }
        public override Vector3 BoxMax
        {
            get
            {
                return Vector3.Max(Vertex1, Vertex2) + capsuleRadius;
            }
        }
        public override Vector3 Scale
        {
            get
            {
                if (ScaleCached.HasValue) return ScaleCached.Value;
                ScaleCached = Vector3.One;
                return Vector3.One;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var cen = (v1 + v2) * 0.5f;
                var trans = value / Scale;
                var ori = Orientation;
                var orinv = Quaternion.Invert(ori);
                Vertex1 = cen + ori.Multiply(trans * orinv.Multiply(v1 - cen));
                Vertex2 = cen + ori.Multiply(trans * orinv.Multiply(v2 - cen));
                capsuleRadius = trans.X * capsuleRadius;
                ScaleCached = value;
            }
        }
        public override Vector3 Position
        {
            get
            {
                return (Vertex1 + Vertex2) * 0.5f;
            }
            set
            {
                var offset = value - Position;
                Vertex1 += offset;
                Vertex2 += offset;
            }
        }
        public override Quaternion Orientation
        {
            get
            {
                if (OrientationCached.HasValue) return OrientationCached.Value;
                var v1 = Vertex1;
                var v2 = Vertex2;
                var dir = v2 - v1;
                var up = Vector3.Normalize(dir.GetPerpVec());
                var ori = Quaternion.Invert(Quaternion.LookAtRH(Vector3.Zero, dir, up));
                OrientationCached = ori;
                return ori;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var cen = (v1 + v2) * 0.5f;
                var trans = value * Quaternion.Invert(Orientation);
                Vertex1 = cen + trans.Multiply(v1 - cen);
                Vertex2 = cen + trans.Multiply(v2 - cen);
                OrientationCached = value;
            }
        }
        private Quaternion? OrientationCached;
        private Vector3? ScaleCached;

        public override int[] VertexIndices
        {
            get
            {
                return new[] { (int)capsuleIndex1, (int)capsuleIndex2 };
            }
            set
            {
                if (value?.Length >= 2)
                {
                    capsuleIndex1 = (ushort)value[0];
                    capsuleIndex2 = (ushort)value[1];
                }
            }
        }
        public override BoundVertexRef NearestVertex(Vector3 p)
        {
            var d1 = (p - Vertex1).Length();
            var d2 = (p - Vertex2).Length();
            if (d1 <= d2) return new BoundVertexRef(capsuleIndex1, d1);
            return new BoundVertexRef(capsuleIndex2, d2);
        }
        public override void GatherVertices(Dictionary<BoundVertex, int> verts)
        {
            if (Owner != null)
            {
                verts[Owner.GetVertexObject(capsuleIndex1)] = capsuleIndex1;
                verts[Owner.GetVertexObject(capsuleIndex2)] = capsuleIndex2;
            }
        }

        public BoundPolygonCapsule()
        {
            Type = BoundPolygonType.Capsule;
        }
        public override void Read(byte[] bytes, int offset)
        {
            capsuleType = BitConverter.ToUInt16(bytes, offset + 0);
            capsuleIndex1 = BitConverter.ToUInt16(bytes, offset + 2);
            capsuleRadius = BitConverter.ToSingle(bytes, offset + 4);
            capsuleIndex2 = BitConverter.ToUInt16(bytes, offset + 8);
            unused0 = BitConverter.ToUInt16(bytes, offset + 10);
            unused1 = BitConverter.ToUInt32(bytes, offset + 12);
        }
        public override void Write(BinaryWriter bw)
        {
            bw.Write(capsuleType);
            bw.Write(capsuleIndex1);
            bw.Write(capsuleRadius);
            bw.Write(capsuleIndex2);
            bw.Write(unused0);
            bw.Write(unused1);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            var s = string.Format("{0} m=\"{1}\" v1=\"{2}\" v2=\"{3}\" radius=\"{4}\"",
                Type,
                MaterialIndex,
                capsuleIndex1,
                capsuleIndex2,
                FloatUtil.ToString(capsuleRadius)
                );
            YbnXml.SelfClosingTag(sb, indent, s);
        }
        public override void ReadXml(XmlNode node)
        {
            Material = Owner?.GetMaterialByIndex(Xml.GetIntAttribute(node, "m")) ?? new BoundMaterial_s();
            capsuleIndex1 = (ushort)Xml.GetUIntAttribute(node, "v1");
            capsuleIndex2 = (ushort)Xml.GetUIntAttribute(node, "v2");
            capsuleRadius = Xml.GetFloatAttribute(node, "radius");
        }
        public override string ToString()
        {
            return base.ToString() + ": " + capsuleIndex1.ToString() + ", " + capsuleIndex2.ToString() + ", " + capsuleRadius.ToString();
        }
    }
    [TC(typeof(EXP))] public class BoundPolygonBox : BoundPolygon
    {
        public uint boxType { get; set; }
        public short boxIndex1 { get; set; }
        public short boxIndex2 { get; set; }
        public short boxIndex3 { get; set; }
        public short boxIndex4 { get; set; }
        public uint unused0 { get; set; }

        public Vector3 Vertex1
        {
            get { return (Owner != null) ? Owner.GetVertexPos(boxIndex1) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(boxIndex1, value); }
        }
        public Vector3 Vertex2
        {
            get { return (Owner != null) ? Owner.GetVertexPos(boxIndex2) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(boxIndex2, value); }
        }
        public Vector3 Vertex3
        {
            get { return (Owner != null) ? Owner.GetVertexPos(boxIndex3) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(boxIndex3, value); }
        }
        public Vector3 Vertex4
        {
            get { return (Owner != null) ? Owner.GetVertexPos(boxIndex4) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(boxIndex4, value); }
        }

        public override Vector3 BoxMin
        {
            get
            {
                return Vector3.Min(Vector3.Min(Vector3.Min(Vertex1, Vertex2), Vertex3), Vertex4);
            }
        }
        public override Vector3 BoxMax
        {
            get
            {
                return Vector3.Max(Vector3.Max(Vector3.Max(Vertex1, Vertex2), Vertex3), Vertex4);
            }
        }
        public override Vector3 Scale
        {
            get
            {
                if (ScaleCached.HasValue) return ScaleCached.Value;
                ScaleCached = Vector3.One;
                return Vector3.One;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var v4 = Vertex4;
                var cen = (v1 + v2 + v3 + v4) * 0.25f;
                var trans = value / Scale;
                var ori = Orientation;
                var orinv = Quaternion.Invert(ori);
                Vertex1 = cen + ori.Multiply(trans * orinv.Multiply(v1 - cen));
                Vertex2 = cen + ori.Multiply(trans * orinv.Multiply(v2 - cen));
                Vertex3 = cen + ori.Multiply(trans * orinv.Multiply(v3 - cen));
                Vertex4 = cen + ori.Multiply(trans * orinv.Multiply(v4 - cen));
                ScaleCached = value;
            }
        }
        public override Vector3 Position
        {
            get
            {
                return (Vertex1 + Vertex2 + Vertex3 + Vertex4) * 0.25f;
            }
            set
            {
                var offset = value - Position;
                Vertex1 += offset;
                Vertex2 += offset;
                Vertex3 += offset;
                Vertex4 += offset;
            }
        }
        public override Quaternion Orientation
        {
            get
            {
                if (OrientationCached.HasValue) return OrientationCached.Value;
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var v4 = Vertex4;
                var dir = (v1+v4) - (v2+v3);
                var up = Vector3.Normalize((v3+v4) - (v1+v2));
                var ori = Quaternion.Invert(Quaternion.LookAtRH(Vector3.Zero, dir, up));
                OrientationCached = ori;
                return ori;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var v3 = Vertex3;
                var v4 = Vertex4;
                var cen = (v1 + v2 + v3 + v4) * 0.25f;
                var trans = value * Quaternion.Invert(Orientation);
                Vertex1 = cen + trans.Multiply(v1 - cen);
                Vertex2 = cen + trans.Multiply(v2 - cen);
                Vertex3 = cen + trans.Multiply(v3 - cen);
                Vertex4 = cen + trans.Multiply(v4 - cen);
                OrientationCached = value;
            }
        }
        private Quaternion? OrientationCached;
        private Vector3? ScaleCached;

        public override int[] VertexIndices
        {
            get
            {
                return new[] { (int)boxIndex1, (int)boxIndex2, (int)boxIndex3, (int)boxIndex4 };
            }
            set
            {
                if (value?.Length >= 2)
                {
                    boxIndex1 = (short)value[0];
                    boxIndex2 = (short)value[1];
                    boxIndex3 = (short)value[2];
                    boxIndex4 = (short)value[3];
                }
            }
        }
        public override BoundVertexRef NearestVertex(Vector3 p)
        {
            var d1 = (p - Vertex1).Length();
            var d2 = (p - Vertex2).Length();
            var d3 = (p - Vertex3).Length();
            var d4 = (p - Vertex4).Length();
            if ((d1 <= d2) && (d1 <= d3) && (d1 <= d4)) return new BoundVertexRef(boxIndex1, d1);
            if ((d2 <= d3) && (d2 <= d4)) return new BoundVertexRef(boxIndex2, d2);
            if (d3 <= d4) return new BoundVertexRef(boxIndex3, d3);
            return new BoundVertexRef(boxIndex4, d4);
        }
        public override void GatherVertices(Dictionary<BoundVertex, int> verts)
        {
            if (Owner != null)
            {
                verts[Owner.GetVertexObject(boxIndex1)] = boxIndex1;
                verts[Owner.GetVertexObject(boxIndex2)] = boxIndex2;
                verts[Owner.GetVertexObject(boxIndex3)] = boxIndex3;
                verts[Owner.GetVertexObject(boxIndex4)] = boxIndex4;
            }
        }

        public BoundPolygonBox()
        {
            Type = BoundPolygonType.Box;
        }
        public override void Read(byte[] bytes, int offset)
        {
            boxType = BitConverter.ToUInt32(bytes, offset + 0);
            boxIndex1 = BitConverter.ToInt16(bytes, offset + 4);
            boxIndex2 = BitConverter.ToInt16(bytes, offset + 6);
            boxIndex3 = BitConverter.ToInt16(bytes, offset + 8);
            boxIndex4 = BitConverter.ToInt16(bytes, offset + 10);
            unused0 = BitConverter.ToUInt32(bytes, offset + 12);
        }
        public override void Write(BinaryWriter bw)
        {
            bw.Write(boxType);
            bw.Write(boxIndex1);
            bw.Write(boxIndex2);
            bw.Write(boxIndex3);
            bw.Write(boxIndex4);
            bw.Write(unused0);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            var s = string.Format("{0} m=\"{1}\" v1=\"{2}\" v2=\"{3}\" v3=\"{4}\" v4=\"{5}\"",
                Type,
                MaterialIndex,
                boxIndex1,
                boxIndex2,
                boxIndex3,
                boxIndex4
                );
            YbnXml.SelfClosingTag(sb, indent, s);
        }
        public override void ReadXml(XmlNode node)
        {
            Material = Owner?.GetMaterialByIndex(Xml.GetIntAttribute(node, "m")) ?? new BoundMaterial_s();
            boxIndex1 = (short)Xml.GetIntAttribute(node, "v1");
            boxIndex2 = (short)Xml.GetIntAttribute(node, "v2");
            boxIndex3 = (short)Xml.GetIntAttribute(node, "v3");
            boxIndex4 = (short)Xml.GetIntAttribute(node, "v4");
        }
        public override string ToString()
        {
            return base.ToString() + ": " + boxIndex1.ToString() + ", " + boxIndex2.ToString() + ", " + boxIndex3.ToString() + ", " + boxIndex4.ToString();
        }
    }
    [TC(typeof(EXP))] public class BoundPolygonCylinder : BoundPolygon
    {
        public ushort cylinderType { get; set; }
        public ushort cylinderIndex1 { get; set; }
        public float cylinderRadius { get; set; }
        public ushort cylinderIndex2 { get; set; }
        public ushort unused0 { get; set; }
        public uint unused1 { get; set; }

        public Vector3 Vertex1
        {
            get { return (Owner != null) ? Owner.GetVertexPos(cylinderIndex1) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(cylinderIndex1, value); }
        }
        public Vector3 Vertex2
        {
            get { return (Owner != null) ? Owner.GetVertexPos(cylinderIndex2) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(cylinderIndex2, value); }
        }

        public override Vector3 BoxMin
        {
            get
            {
                return Vector3.Min(Vertex1, Vertex2) - cylinderRadius;//not perfect but meh
            }
        }
        public override Vector3 BoxMax
        {
            get
            {
                return Vector3.Max(Vertex1, Vertex2) + cylinderRadius;//not perfect but meh
            }
        }
        public override Vector3 Scale
        {
            get
            {
                if (ScaleCached.HasValue) return ScaleCached.Value;
                ScaleCached = Vector3.One;
                return Vector3.One;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var cen = (v1 + v2) * 0.5f;
                var trans = value / Scale;
                var ori = Orientation;
                var orinv = Quaternion.Invert(ori);
                Vertex1 = cen + ori.Multiply(trans * orinv.Multiply(v1 - cen));
                Vertex2 = cen + ori.Multiply(trans * orinv.Multiply(v2 - cen));
                cylinderRadius = trans.X * cylinderRadius;
                ScaleCached = value;
            }
        }
        public override Vector3 Position
        {
            get
            {
                return (Vertex1 + Vertex2) * 0.5f;
            }
            set
            {
                var offset = value - Position;
                Vertex1 += offset;
                Vertex2 += offset;
            }
        }
        public override Quaternion Orientation
        {
            get
            {
                if (OrientationCached.HasValue) return OrientationCached.Value;
                var v1 = Vertex1;
                var v2 = Vertex2;
                var dir = v2 - v1;
                var up = Vector3.Normalize(dir.GetPerpVec());
                var ori = Quaternion.Invert(Quaternion.LookAtRH(Vector3.Zero, dir, up));
                OrientationCached = ori;
                return ori;
            }
            set
            {
                var v1 = Vertex1;
                var v2 = Vertex2;
                var cen = (v1 + v2) * 0.5f;
                var trans = value * Quaternion.Invert(Orientation);
                Vertex1 = cen + trans.Multiply(v1 - cen);
                Vertex2 = cen + trans.Multiply(v2 - cen);
                OrientationCached = value;
            }
        }
        private Quaternion? OrientationCached;
        private Vector3? ScaleCached;

        public override int[] VertexIndices
        {
            get
            {
                return new[] { (int)cylinderIndex1, (int)cylinderIndex2 };
            }
            set
            {
                if (value?.Length >= 2)
                {
                    cylinderIndex1 = (ushort)value[0];
                    cylinderIndex2 = (ushort)value[1];
                }
            }
        }
        public override BoundVertexRef NearestVertex(Vector3 p)
        {
            var d1 = (p - Vertex1).Length();
            var d2 = (p - Vertex2).Length();
            if (d1 <= d2) return new BoundVertexRef(cylinderIndex1, d1);
            return new BoundVertexRef(cylinderIndex2, d2);
        }
        public override void GatherVertices(Dictionary<BoundVertex, int> verts)
        {
            if (Owner != null)
            {
                verts[Owner.GetVertexObject(cylinderIndex1)] = cylinderIndex1;
                verts[Owner.GetVertexObject(cylinderIndex2)] = cylinderIndex2;
            }
        }

        public BoundPolygonCylinder()
        {
            Type = BoundPolygonType.Cylinder;
        }
        public override void Read(byte[] bytes, int offset)
        {
            cylinderType = BitConverter.ToUInt16(bytes, offset + 0);
            cylinderIndex1 = BitConverter.ToUInt16(bytes, offset + 2);
            cylinderRadius = BitConverter.ToSingle(bytes, offset + 4);
            cylinderIndex2 = BitConverter.ToUInt16(bytes, offset + 8);
            unused0 = BitConverter.ToUInt16(bytes, offset + 10);
            unused1 = BitConverter.ToUInt32(bytes, offset + 12);
        }
        public override void Write(BinaryWriter bw)
        {
            bw.Write(cylinderType);
            bw.Write(cylinderIndex1);
            bw.Write(cylinderRadius);
            bw.Write(cylinderIndex2);
            bw.Write(unused0);
            bw.Write(unused1);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            var s = string.Format("{0} m=\"{1}\" v1=\"{2}\" v2=\"{3}\" radius=\"{4}\"",
                Type,
                MaterialIndex,
                cylinderIndex1,
                cylinderIndex2,
                FloatUtil.ToString(cylinderRadius)
                );
            YbnXml.SelfClosingTag(sb, indent, s);
        }
        public override void ReadXml(XmlNode node)
        {
            Material = Owner?.GetMaterialByIndex(Xml.GetIntAttribute(node, "m")) ?? new BoundMaterial_s();
            cylinderIndex1 = (ushort)Xml.GetUIntAttribute(node, "v1");
            cylinderIndex2 = (ushort)Xml.GetUIntAttribute(node, "v2");
            cylinderRadius = Xml.GetFloatAttribute(node, "radius");
        }
        public override string ToString()
        {
            return base.ToString() + ": " + cylinderIndex1.ToString() + ", " + cylinderIndex2.ToString() + ", " + cylinderRadius.ToString();
        }
    }


    [TC(typeof(EXP))] public struct BoundEdgeRef //convenience struct for updating edge indices
    {
        public int Vertex1 { get; set; }
        public int Vertex2 { get; set; }

        public BoundEdgeRef(int i1, int i2)
        {
            Vertex1 = Math.Min(i1, i2);
            Vertex2 = Math.Max(i1, i2);
        }
    }
    [TC(typeof(EXP))] public class BoundEdge //convenience class for updating edge indices
    {
        public BoundPolygonTriangle Triangle1 { get; set; }
        public BoundPolygonTriangle Triangle2 { get; set; }
        public int EdgeID1 { get; set; }
        public int EdgeID2 { get; set; }

        public BoundEdge(BoundPolygonTriangle t1, int e1)
        {
            Triangle1 = t1;
            EdgeID1 = e1;
        }
    }

    [TC(typeof(EXP))] public struct BoundVertexRef //convenience struct for BoundPolygon.NearestVertex and SpaceRayIntersectResult
    {
        public int Index { get; set; }
        public float Distance { get; set; }

        public BoundVertexRef(int index, float dist)
        {
            Index = index;
            Distance = dist;
        }
    }
    [TC(typeof(EXP))] public class BoundVertex //class for editing convenience, to hold a reference to a BoundGeometry vertex
    {
        public BoundGeometry Owner { get; set; }
        public int Index { get; set; }

        public Vector3 Position
        {
            get { return (Owner != null) ? Owner.GetVertexPos(Index) : Vector3.Zero; }
            set { if (Owner != null) Owner.SetVertexPos(Index, value); }
        }
        public BoundMaterialColour Colour
        {
            get { return (Owner != null) ? Owner.GetVertexColour(Index) : new BoundMaterialColour(); }
            set { if (Owner != null) Owner.SetVertexColour(Index, value); }
        }

        public BoundVertex(BoundGeometry owner, int index)
        {
            Owner = owner;
            Index = index;
        }

        public virtual string Title
        {
            get
            {
                return "Vertex " + Index.ToString();
            }
        }
    }

    [TC(typeof(EXP))] public struct BoundVertex_s
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }

        public BoundVertex_s(Vector3 v)
        {
            X = (short)v.X;
            Y = (short)v.Y;
            Z = (short)v.Z;
        }

        public Vector3 Vector
        {
            get { return new Vector3(X, Y, Z); }
            set { X = (short)value.X; Y = (short)value.Y; Z = (short)value.Z; }
        }
    }

    [TC(typeof(EXP))] public class BoundGeomOctants : ResourceSystemBlock
    {
        public uint[] Counts { get; set; } = new uint[8];
        public uint[][] Items { get; private set; } = new uint[8][];


        public override long BlockLength
        {
            get
            {
                long len = 128; // (8*(4 + 8)) + 32
                for (int i = 0; i < 8; i++)
                {
                    len += (Counts[i] * 4);
                }
                return len;
            }
        }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            if ((parameters?.Length ?? 0) < 1)
            { return; } //shouldn't happen!

            ulong ptr = (ulong)parameters[0]; //pointer array pointer

            for (int i = 0; i < 8; i++)
            {
                Counts[i] = reader.ReadUInt32();
            }

            ulong[] ptrlist = reader.ReadUlongsAt(ptr, 8, false);

            //if (ptr != (ulong)reader.Position)
            //{ }//no hit
            //ptr += 64;

            for (int i = 0; i < 8; i++)
            {
                Items[i] = reader.ReadUintsAt(ptrlist[i], Counts[i], false);

                //if (ptrlist[i] != ptr)
                //{ ptr = ptrlist[i]; }//no hit
                //ptr += (Counts[i] * 4);
            }

            //reader.Position = (long)ptr;
            //var b = reader.ReadBytes(32);
            //for (int i = 0; i < b.Length; i++)
            //{
            //    if (b[i] != 0)
            //    { }//no hit
            //}
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            var ptr = writer.Position + 96;
            for (int i = 0; i < 8; i++)
            {
                writer.Write(Counts[i]);
            }
            for (int i = 0; i < 8; i++)
            {
                writer.Write((ulong)ptr);
                ptr += (Counts[i] * 4);
            }
            for (int i = 0; i < 8; i++)
            {
                var items = (i < Items.Length) ? Items[i] : null;
                if (items == null)
                { continue; }
                var c = Counts[i];
                for (int n = 0; n < c; n++)
                {
                    var v = (n < items.Length) ? items[n] : 0;
                    writer.Write(v);
                }
            }
            writer.Write(new byte[32]);
        }

        public void UpdateCounts()
        {
            for (int i = 0; i < 8; i++)
            {
                Counts[i] = (i < (Items?.Length ?? 0)) ? (uint)(Items[i]?.Length ?? 0) : 0;
            }
        }


    }


    [Flags] public enum EBoundCompositeFlags : uint
    {
        NONE = 0u,
        UNKNOWN = 1u,
        MAP_WEAPON = 1u << 1,
        MAP_DYNAMIC = 1u << 2,
        MAP_ANIMAL = 1u << 3,
        MAP_COVER = 1u << 4,
        MAP_VEHICLE = 1u << 5,
        VEHICLE_NOT_BVH = 1u << 6,
        VEHICLE_BVH = 1u << 7,
        VEHICLE_BOX = 1u << 8,
        PED = 1u << 9,
        RAGDOLL = 1u << 10,
        ANIMAL = 1u << 11,
        ANIMAL_RAGDOLL = 1u << 12,
        OBJECT = 1u << 13,
        OBJECT_ENV_CLOTH = 1u << 14,
        PLANT = 1u << 15,
        PROJECTILE = 1u << 16,
        EXPLOSION = 1u << 17,
        PICKUP = 1u << 18,
        FOLIAGE = 1u << 19,
        FORKLIFT_FORKS = 1u << 20,
        TEST_WEAPON = 1u << 21,
        TEST_CAMERA = 1u << 22,
        TEST_AI = 1u << 23,
        TEST_SCRIPT = 1u << 24,
        TEST_VEHICLE_WHEEL = 1u << 25,
        GLASS = 1u << 26,
        MAP_RIVER = 1u << 27,
        SMOKE = 1u << 28,
        UNSMASHED = 1u << 29,
        MAP_STAIRS = 1u << 30,
        MAP_DEEP_SURFACE = 1u << 31,
    }
    [TC(typeof(EXP))] public struct BoundCompositeChildrenFlags
    {
        public EBoundCompositeFlags Flags1 { get; set; }
        public EBoundCompositeFlags Flags2 { get; set; }
        public override string ToString()
        {
            return Flags1.ToString() + ", " + Flags2.ToString();
        }
    }



    [TC(typeof(EXP))] public class BVH : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 128; }
        }

        // structure data
        public ResourceSimpleList64b_s<BVHNode_s> Nodes { get; set; }
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public Vector4 BoundingBoxMin { get; set; }
        public Vector4 BoundingBoxMax { get; set; }
        public Vector4 BoundingBoxCenter { get; set; }
        public Vector4 QuantumInverse { get; set; }
        public Vector4 Quantum { get; set; } // bounding box dimension / 2^16
        public ResourceSimpleList64_s<BVHTreeInfo_s> Trees { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Nodes = reader.ReadBlock<ResourceSimpleList64b_s<BVHNode_s>>();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.BoundingBoxMin = reader.ReadVector4();
            this.BoundingBoxMax = reader.ReadVector4();
            this.BoundingBoxCenter = reader.ReadVector4();
            this.QuantumInverse = reader.ReadVector4();
            this.Quantum = reader.ReadVector4();
            this.Trees = reader.ReadBlock<ResourceSimpleList64_s<BVHTreeInfo_s>>();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {

            // write structure data
            writer.WriteBlock(this.Nodes);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.BoundingBoxMin);
            writer.Write(this.BoundingBoxMax);
            writer.Write(this.BoundingBoxCenter);
            writer.Write(this.QuantumInverse);
            writer.Write(this.Quantum);
            writer.WriteBlock(this.Trees);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            //if (Nodes != null) list.Add(Nodes);
            //if (Trees != null) list.Add(Trees);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x0, Nodes),
                new Tuple<long, IResourceBlock>(0x70, Trees)
            };
        }
    }
    [TC(typeof(EXP))] public struct BVHTreeInfo_s
    {
        public short MinX { get; set; }
        public short MinY { get; set; }
        public short MinZ { get; set; }
        public short MaxX { get; set; }
        public short MaxY { get; set; }
        public short MaxZ { get; set; }
        public short NodeIndex1 { get; set; }
        public short NodeIndex2 { get; set; }

        public Vector3 Min
        {
            get { return new Vector3(MinX, MinY, MinZ); }
            set { MinX = (short)value.X; MinY = (short)value.Y; MinZ = (short)value.Z; }
        }
        public Vector3 Max
        {
            get { return new Vector3(MaxX, MaxY, MaxZ); }
            set { MaxX = (short)value.X; MaxY = (short)value.Y; MaxZ = (short)value.Z; }
        }

        public override string ToString()
        {
            return NodeIndex1.ToString() + ", " + NodeIndex2.ToString() + "  (" + (NodeIndex2 - NodeIndex1).ToString() + " nodes)";
        }
    }
    [TC(typeof(EXP))] public struct BVHNode_s
    {
        public short MinX { get; set; }
        public short MinY { get; set; }
        public short MinZ { get; set; }
        public short MaxX { get; set; }
        public short MaxY { get; set; }
        public short MaxZ { get; set; }
        public short ItemId { get; set; }
        public short ItemCount { get; set; }

        public Vector3 Min
        {
            get { return new Vector3(MinX, MinY, MinZ); }
            set { MinX = (short)value.X; MinY = (short)value.Y; MinZ = (short)value.Z; }
        }
        public Vector3 Max
        {
            get { return new Vector3(MaxX, MaxY, MaxZ); }
            set { MaxX = (short)value.X; MaxY = (short)value.Y; MaxZ = (short)value.Z; }
        }

        public override string ToString()
        {
            return ItemId.ToString() + ": " + ItemCount.ToString();
        }
    }


    public class BVHBuilder
    {
        public static int MaxNodeItemCount = 4; //item threshold: 1 for composites, 4 for geometries
        public static int MaxTreeNodeCount = 127; //max number of nodes found in any tree


        public static BVH Build(List<BVHBuilderItem> items, int itemThreshold)
        {
            if (items == null) return null;
            var bvh = new BVH();
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            var nodes = new List<BVHBuilderNode>();
            var trees = new List<BVHBuilderNode>();
            var iteml = new List<BVHBuilderItem>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) continue;
                iteml.Add(item);
                min = Vector3.Min(min, item.Min);
                max = Vector3.Max(max, item.Max);
            }
            var cen = (min + max) * 0.5f;
            bvh.BoundingBoxMin = new Vector4(min, float.NaN);
            bvh.BoundingBoxMax = new Vector4(max, float.NaN);
            bvh.BoundingBoxCenter = new Vector4(cen, float.NaN);
            bvh.Quantum = new Vector4(Vector3.Max((min - cen).Abs(), (max - cen).Abs()) / 32767.0f, float.NaN);
            bvh.QuantumInverse = new Vector4(1.0f / bvh.Quantum.XYZ(), float.NaN);

            var root = new BVHBuilderNode();
            root.Items = iteml.ToList();
            root.Build(itemThreshold);
            root.GatherNodes(nodes);
            root.GatherTrees(trees);


            if (itemThreshold > 1) //need to reorder items, since they need to be grouped by node for the node's item index
            {
                items.Clear();
                foreach (var node in nodes)
                {
                    if (node.Items != null)
                    {
                        foreach (var item in node.Items)
                        {
                            item.Index = items.Count;
                            items.Add(item);
                        }
                    }
                }
            }
            else //don't need to reorder items, since nodes only have one item and one item index
            { }

            var bvhtrees = new List<BVHTreeInfo_s>();
            var bvhnodes = new List<BVHNode_s>();
            var qi = bvh.QuantumInverse.XYZ();
            var c = bvh.BoundingBoxCenter.XYZ();
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var id = ((node.Items?.Count ?? 0) > 0) ? node.Items[0].Index : 0;
                var tn = node.TotalNodes;
                var bn = new BVHNode_s();
                bn.Min = (node.Min - c) * qi;
                bn.Max = (node.Max - c) * qi;
                bn.ItemCount = (short)((tn <= 1) ? node.TotalItems : 0);
                bn.ItemId = (short)((tn <= 1) ? id : node.TotalNodes);
                bvhnodes.Add(bn);
            }

            for (int i = 0; i < trees.Count; i++)
            {
                var tree = trees[i];
                var bt = new BVHTreeInfo_s();
                bt.Min = (tree.Min - c) * qi;
                bt.Max = (tree.Max - c) * qi;
                bt.NodeIndex1 = (short)tree.Index;
                bt.NodeIndex2 = (short)(tree.Index + tree.TotalNodes);
                bvhtrees.Add(bt);
            }


            var nodecount = bvhnodes.Count;
            if (itemThreshold <= 1) //for composites, capacity needs to be (numchildren*2)+1, with empty nodes filling up the space..
            {
                var capacity = (items.Count * 2) + 1;
                var emptynode = new BVHNode_s();
                emptynode.ItemId = 1;
                while (bvhnodes.Count < capacity)
                {
                    bvhnodes.Add(emptynode);
                }
            }

            bvh.Nodes = new ResourceSimpleList64b_s<BVHNode_s>();
            bvh.Nodes.data_items = bvhnodes.ToArray();
            bvh.Nodes.EntriesCount = (uint)nodecount;

            bvh.Trees = new ResourceSimpleList64_s<BVHTreeInfo_s>();
            bvh.Trees.data_items = bvhtrees.ToArray();

            return bvh;
        }

        public static BVHBuilderNode[] Unbuild(BVH bvh)
        {
            if ((bvh?.Trees?.data_items == null) || (bvh?.Nodes?.data_items == null)) return null;

            var nodes = new List<BVHBuilderNode>();
            foreach (var tree in bvh.Trees.data_items)
            {
                var bnode = new BVHBuilderNode();
                bnode.Unbuild(bvh, tree.NodeIndex1, tree.NodeIndex2);
                nodes.Add(bnode);
                //MaxTreeNodeCount = Math.Max(MaxTreeNodeCount, tree.NodeIndex2 - tree.NodeIndex1);
            }
            return nodes.ToArray();
        }

    }
    public class BVHBuilderNode
    {
        public List<BVHBuilderNode> Children;
        public List<BVHBuilderItem> Items;
        public Vector3 Min;
        public Vector3 Max;
        public int Index;

        public int TotalNodes
        {
            get
            {
                int c = 1;
                if (Children != null)
                {
                    foreach (var child in Children)
                    {
                        c += child.TotalNodes;
                    }
                }
                return c;
            }
        }
        public int TotalItems
        {
            get
            {
                int c = Items?.Count ?? 0;
                if (Children != null)
                {
                    foreach (var child in Children)
                    {
                        c += child.TotalItems;
                    }
                }
                return c;
            }
        }

        public void Build(int itemThreshold)
        {
            UpdateMinMax();
            if (Items == null) return;
            if (Items.Count <= itemThreshold) return;

            var avgsum = Vector3.Zero;
            foreach (var item in Items)
            {
                avgsum += item.Min;
                avgsum += item.Max;
            }
            var avg = avgsum * (0.5f / Items.Count);
            int countx = 0, county = 0, countz = 0;
            foreach (var item in Items)
            {
                var icen = (item.Min + item.Max) * 0.5f;
                if (icen.X < avg.X) countx++;
                if (icen.Y < avg.Y) county++;
                if (icen.Z < avg.Z) countz++;
            }
            var target = Items.Count / 2.0f;
            var dx = Math.Abs(target - countx);
            var dy = Math.Abs(target - county);
            var dz = Math.Abs(target - countz);
            int axis = -1;
            if ((dx <= dy) && (dx <= dz)) axis = 0; //x seems best
            else if (dy <= dz) axis = 1; //y seems best
            else axis = 2; //z seems best

            var l1 = new List<BVHBuilderItem>();
            var l2 = new List<BVHBuilderItem>();
            foreach (var item in Items)
            {
                var icen = (item.Min + item.Max) * 0.5f;
                bool s = false;
                switch (axis)
                {
                    default:
                    case 0: s = (icen.X > avg.X); break;
                    case 1: s = (icen.Y > avg.Y); break;
                    case 2: s = (icen.Z > avg.Z); break;
                }
                if (s) l1.Add(item);
                else l2.Add(item);
            }

            if ((l1.Count == 0) || (l2.Count == 0)) //don't get stuck in a stack overflow...
            {
                var l3 = new List<BVHBuilderItem>();//we can recover from this...
                l3.AddRange(l1);
                l3.AddRange(l2);
                if (l3.Count > 0)
                {
                    l3.Sort((a, b) =>
                    {
                        var c = a.Min.CompareTo(b.Min); if (c != 0) return c;
                        return a.Max.CompareTo(b.Max);
                    });
                    l1.Clear();
                    l2.Clear();
                    var hidx = l3.Count / 2;
                    for (int i = 0; i < hidx; i++) l1.Add(l3[i]);
                    for (int i = hidx; i < l3.Count; i++) l2.Add(l3[i]);
                }
                else
                { return; }//nothing to see here?
            }

            Items = null;
            Children = new List<BVHBuilderNode>();

            var n1 = new BVHBuilderNode();
            n1.Items = l1;
            n1.Build(itemThreshold);
            Children.Add(n1);

            var n2 = new BVHBuilderNode();
            n2.Items = l2;
            n2.Build(itemThreshold);
            Children.Add(n2);

            Children.Sort((a, b) =>
            {
                return b.TotalItems.CompareTo(a.TotalItems);
            }); //is this necessary?

        }
        public void UpdateMinMax()
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    min = Vector3.Min(min, item.Min);
                    max = Vector3.Max(max, item.Max);
                }
            }
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.UpdateMinMax();
                    min = Vector3.Min(min, child.Min);
                    max = Vector3.Max(max, child.Max);
                }
            }
            Min = min;
            Max = max;
        }
        public void GatherNodes(List<BVHBuilderNode> nodes)
        {
            Index = nodes.Count;
            nodes.Add(this);
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.GatherNodes(nodes);
                }
            }
        }
        public void GatherTrees(List<BVHBuilderNode> trees)
        {
            if ((TotalNodes > BVHBuilder.MaxTreeNodeCount) && ((Children?.Count ?? 0) > 0))
            {
                foreach (var child in Children)
                {
                    child.GatherTrees(trees);
                }
            }
            else
            {
                trees.Add(this);
            }
        }

        public void Unbuild(BVH bvh, int nodeIndex1, int nodeIndex2)
        {
            var q = bvh.Quantum.XYZ();
            var c = bvh.BoundingBoxCenter.XYZ();
            int nodeind = nodeIndex1;
            int lastind = nodeIndex2;
            while (nodeind < lastind)
            {
                var node = bvh.Nodes.data_items[nodeind];
                if (node.ItemCount <= 0) //intermediate node with child nodes
                {
                    Children = new List<BVHBuilderNode>();
                    var cind1 = nodeind + 1;
                    var lcind = nodeind + node.ItemId; //(child node count)
                    while (cind1 < lcind)
                    {
                        var cnode = bvh.Nodes.data_items[cind1];
                        var ccount = (cnode.ItemCount <= 0) ? cnode.ItemId : 1;
                        var cind2 = cind1 + ccount;
                        var chi = new BVHBuilderNode();
                        chi.Unbuild(bvh, cind1, cind2);
                        Children.Add(chi);
                        cind1 = cind2;
                    }
                    nodeind += node.ItemId;
                }
                else //leaf node, with polygons
                {
                    Items = new List<BVHBuilderItem>();
                    for (int i = 0; i < node.ItemCount; i++)
                    {
                        var item = new BVHBuilderItem();
                        item.Index = node.ItemId + i;
                        Items.Add(item);
                    }
                    //BVHBuilder.MaxNodeItemCount = Math.Max(BVHBuilder.MaxNodeItemCount, node.ItemCount);
                    nodeind++;
                }
                Min = node.Min * q + c;
                Max = node.Max * q + c;
            }
        }

        public override string ToString()
        {
            var fstr = (Children != null) ? (TotalNodes.ToString() + ", 0 - ") : (Items != null) ? ("i, " + TotalItems.ToString() + " - ") : "error!";
            var cstr = (Children != null) ? (Children.Count.ToString() + " children") : "";
            var istr = (Items != null) ? (Items.Count.ToString() + " items") : "";
            if (string.IsNullOrEmpty(cstr)) return fstr + istr;
            if (string.IsNullOrEmpty(istr)) return fstr + cstr;
            return cstr + ", " + istr;
        }
    }
    public class BVHBuilderItem
    {
        public Vector3 Min;
        public Vector3 Max;
        public int Index;
        public Bounds Bounds;
        public BoundPolygon Polygon;
    }



    [Flags] public enum EBoundMaterialFlags : ushort
    {
        NONE = 0,
        FLAG_STAIRS = 1,
        FLAG_NOT_CLIMBABLE = 1 << 1,
        FLAG_SEE_THROUGH = 1 << 2,
        FLAG_SHOOT_THROUGH = 1 << 3,
        FLAG_NOT_COVER = 1 << 4,
        FLAG_WALKABLE_PATH = 1 << 5,
        FLAG_NO_CAM_COLLISION = 1 << 6,
        FLAG_SHOOT_THROUGH_FX = 1 << 7,
        FLAG_NO_DECAL = 1 << 8,
        FLAG_NO_NAVMESH = 1 << 9,
        FLAG_NO_RAGDOLL = 1 << 10,
        FLAG_VEHICLE_WHEEL = 1 << 11,
        FLAG_NO_PTFX = 1 << 12,
        FLAG_TOO_STEEP_FOR_PLAYER = 1 << 13,
        FLAG_NO_NETWORK_SPAWN = 1 << 14,
        FLAG_NO_CAM_COLLISION_ALLOW_CLIPPING = 1 << 15,
    }
    [TC(typeof(EXP))] public struct BoundMaterial_s : IMetaXmlItem
    {

        public uint Data1;
        public uint Data2;

        public BoundsMaterialType Type
        {
            get => (BoundsMaterialType)(Data1 & 0xFFu);
            set => Data1 = ((Data1 & 0xFFFFFF00u) | ((byte)value & 0xFFu));
        }

        public byte ProceduralId
        {
            get => (byte)((Data1 >> 8) & 0xFFu);
            set => Data1 = ((Data1 & 0xFFFF00FFu) | ((value & 0xFFu) << 8));
        }

        public byte RoomId
        {
            get => (byte)((Data1 >> 16) & 0x1Fu);
            set => Data1 = ((Data1 & 0xFFE0FFFFu) | ((value & 0x1Fu) << 16));
        }

        public byte PedDensity
        {
            get => (byte)((Data1 >> 21) & 0x7u);
            set => Data1 = ((Data1 & 0xFF1FFFFFu) | ((value & 0x7u) << 21));
        }

        public EBoundMaterialFlags Flags
        {
            get => (EBoundMaterialFlags)(((Data1 >> 24) & 0xFFu) | ((Data2 & 0xFFu) << 8));
            set
            {
                Data1 = (Data1 & 0x00FFFFFFu) | (((ushort)value & 0x00FFu) << 24);
                Data2 = (Data2 & 0xFFFFFF00u) | (((ushort)value & 0xFF00u) >> 8);
            }
        }

        public byte MaterialColorIndex
        {
            get => (byte)((Data2 >> 8) & 0xFFu);
            set => Data2 = ((Data2 & 0xFFFF00FFu) | ((value & 0xFFu) << 8));
        }

        public ushort Unk4
        {
            get => (ushort)((Data2 >> 16) & 0xFFFFu);
            set => Data2 = ((Data2 & 0x0000FFFFu) | ((value & 0xFFFFu) << 16));
        }


        public void WriteXml(StringBuilder sb, int indent)
        {
            YbnXml.ValueTag(sb, indent, "Type", Type.Index.ToString());
            YbnXml.ValueTag(sb, indent, "ProceduralID", ProceduralId.ToString());
            YbnXml.ValueTag(sb, indent, "RoomID", RoomId.ToString());
            YbnXml.ValueTag(sb, indent, "PedDensity", PedDensity.ToString());
            YbnXml.StringTag(sb, indent, "Flags", Flags.ToString());
            YbnXml.ValueTag(sb, indent, "MaterialColourIndex", MaterialColorIndex.ToString());
            YbnXml.ValueTag(sb, indent, "Unk", Unk4.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Type = (byte)Xml.GetChildUIntAttribute(node, "Type", "value");
            ProceduralId = (byte)Xml.GetChildUIntAttribute(node, "ProceduralID", "value");
            RoomId = (byte)Xml.GetChildUIntAttribute(node, "RoomID", "value");
            PedDensity = (byte)Xml.GetChildUIntAttribute(node, "PedDensity", "value");
            Flags = Xml.GetChildEnumInnerText<EBoundMaterialFlags>(node, "Flags");
            MaterialColorIndex = (byte)Xml.GetChildUIntAttribute(node, "MaterialColourIndex", "value");
            Unk4 = (ushort)Xml.GetChildUIntAttribute(node, "Unk", "value");
        }

        public override string ToString()
        {
            return Data1.ToString() + ", " + Data2.ToString() + ", "
                + Type.ToString() + ", " + ProceduralId.ToString() + ", " + RoomId.ToString() + ", " + PedDensity.ToString() + ", "
                + Flags.ToString() + ", " + MaterialColorIndex.ToString() + ", " + Unk4.ToString();
        }

    }
    [TC(typeof(EXP))] public struct BoundMaterialColour
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; } //GIMS EVO saves this as "opacity" 0-100
        public override string ToString()
        {
            //return Type.ToString() + ", " + Unk0.ToString() + ", " + Unk1.ToString() + ", " + Unk2.ToString();
            return R.ToString() + ", " + G.ToString() + ", " + B.ToString() + ", " + A.ToString();
        }
    }
    [TC(typeof(EXP))] public struct BoundsMaterialType
    {
        public byte Index { get; set; }

        public BoundsMaterialData MaterialData
        {
            get
            {
                return BoundsMaterialTypes.GetMaterial(this);
            }
        }

        public override string ToString()
        {
            return BoundsMaterialTypes.GetMaterialName(this);
        }

        public static implicit operator byte(BoundsMaterialType matType)
        {
            return matType.Index;  //implicit conversion
        }

        public static implicit operator BoundsMaterialType(byte b)
        {
            return new BoundsMaterialType() { Index = b };
        }
    }
    [TC(typeof(EXP))] public class BoundsMaterialData
    {
        public string Name { get; set; }
        public string Filter { get; set; }
        public string FXGroup { get; set; }
        public string VFXDisturbanceType { get; set; }
        public string RumbleProfile { get; set; }
        public string ReactWeaponType { get; set; }
        public string Friction { get; set; }
        public string Elasticity { get; set; }
        public string Density { get; set; }
        public string TyreGrip { get; set; }
        public string WetGrip { get; set; }
        public string TyreDrag { get; set; }
        public string TopSpeedMult { get; set; }
        public string Softness { get; set; }
        public string Noisiness { get; set; }
        public string PenetrationResistance { get; set; }
        public string SeeThru { get; set; }
        public string ShootThru { get; set; }
        public string ShootThruFX { get; set; }
        public string NoDecal { get; set; }
        public string Porous { get; set; }
        public string HeatsTyre { get; set; }
        public string Material { get; set; }

        public Color Colour { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class BoundsMaterialTypes
    {
        private static Dictionary<string, Color> ColourDict;
        public static List<BoundsMaterialData> Materials;

        public static void Init(GameFileCache gameFileCache)
        {
            var rpfman = gameFileCache.RpfMan;

            var dic = new Dictionary<string,Color>();
            string filename2 = "common.rpf\\data\\effects\\materialfx.dat";
            string txt2 = rpfman.GetFileUTF8Text(filename2);
            AddMaterialfxDat(txt2, dic);

            ColourDict = dic;

            var list = new List<BoundsMaterialData>();
            string filename = "common.rpf\\data\\materials\\materials.dat";
            if (gameFileCache.EnableDlc)
            {
                filename = "update\\update.rpf\\common\\data\\materials\\materials.dat";
            }
            string txt = rpfman.GetFileUTF8Text(filename);
            AddMaterialsDat(txt, list);

            Materials = list;
        }

        //Only gets the colors
        private static void AddMaterialfxDat(string txt, Dictionary<string, Color> dic)
        {
            dic.Clear();
            if (txt == null) return;

            string[] lines = txt.Split('\n');
            string startLine = "MTLFX_TABLE_START";
            string endLine = "MTLFX_TABLE_END";

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];

                if (line[0] == '#') continue;
                if (line.StartsWith(startLine)) continue;
                if (line.StartsWith(endLine)) break;

                string[] parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 5) continue; // FXGroup R G B ...

                int cp = 0;
                Color c = new Color();
                c.A = 0xFF;
                string fxgroup = string.Empty;
                for (int p = 0; p < parts.Length; p++)
                {
                    string part = parts[p].Trim();
                    if (string.IsNullOrWhiteSpace(part)) continue;
                    switch (cp)
                    {
                        case 0: fxgroup = part; break;
                        case 1: c.R = byte.Parse(part); break;
                        case 2: c.G = byte.Parse(part); break;
                        case 3: c.B = byte.Parse(part); break;
                    }
                    cp++;
                }
                dic.Add(fxgroup, c);
            }
        }

        private static void AddMaterialsDat(string txt, List<BoundsMaterialData> list)
        {
            list.Clear();
            if (txt == null) return;
            string[] lines = txt.Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length < 20) continue;
                if (line[0] == '#') continue;
                string[] parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 10) continue;
                int cp = 0;
                BoundsMaterialData d = new BoundsMaterialData();
                for (int p = 0; p < parts.Length; p++)
                {
                    string part = parts[p].Trim();
                    if (string.IsNullOrWhiteSpace(part)) continue;
                    switch (cp)
                    {
                        case 0: d.Name = part; break;
                        case 1: d.Filter = part; break;
                        case 2: d.FXGroup = part; break;
                        case 3: d.VFXDisturbanceType = part; break;
                        case 4: d.RumbleProfile = part; break;
                        case 5: d.ReactWeaponType = part; break;
                        case 6: d.Friction = part; break;
                        case 7: d.Elasticity = part; break;
                        case 8: d.Density = part; break;
                        case 9: d.TyreGrip = part; break;
                        case 10: d.WetGrip = part; break;
                        case 11: d.TyreDrag = part; break;
                        case 12: d.TopSpeedMult = part; break;
                        case 13: d.Softness = part; break;
                        case 14: d.Noisiness = part; break;
                        case 15: d.PenetrationResistance = part; break;
                        case 16: d.SeeThru = part; break;
                        case 17: d.ShootThru = part; break;
                        case 18: d.ShootThruFX = part; break;
                        case 19: d.NoDecal = part; break;
                        case 20: d.Porous = part; break;
                        case 21: d.HeatsTyre = part; break;
                        case 22: d.Material = part; break;
                    }
                    cp++;
                }
                if (cp != 23)
                { }

                Color c;
                if ((ColourDict != null) && (ColourDict.TryGetValue(d.FXGroup, out c)))
                {
                    d.Colour = c;
                }
                else
                {
                    d.Colour = new Color(0xFFCCCCCC);
                }


                list.Add(d);
            }


            //StringBuilder sb = new StringBuilder();
            //foreach (var d in list)
            //{
            //    sb.AppendLine(d.Name);
            //}
            //string names = sb.ToString();

        }


        public static BoundsMaterialData GetMaterial(BoundsMaterialType type)
        {
            if (Materials == null) return null;
            if (type.Index >= Materials.Count) return null;
            return Materials[type.Index];
        }

        public static BoundsMaterialData GetMaterial(byte index)
        {
            if (Materials == null) return null;
            if ((int)index >= Materials.Count) return null;
            return Materials[index];
        }

        public static string GetMaterialName(BoundsMaterialType type)
        {
            var m = GetMaterial(type);
            if (m == null) return string.Empty;
            return m.Name;
        }

        public static Color GetMaterialColour(BoundsMaterialType type)
        {
            var m = GetMaterial(type);
            if (m == null) return new Color(0xFFCCCCCC);
            return m.Colour;
        }
    }


}
