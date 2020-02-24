using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.Project
{
    public class MenyooXml
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }




        public List<MenyooXmlPlacement> Placements { get; set; } = new List<MenyooXmlPlacement>();


        public void Init(string xmlstr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlstr);

            XmlElement root = doc.DocumentElement;


            //see:
            //https://github.com/sollaholla/me2ymap/blob/master/YMapExporter/SpoonerPlacements.cs
            //https://github.com/Guad/MapEditor/blob/master/MenyooCompatibility.cs



            //example:
            //<Note />
            //<AudioFile volume="400" />
            //<ClearDatabase>false</ClearDatabase>
            //<ClearWorld>0</ClearWorld>
            //<ClearMarkers>false</ClearMarkers>
            //<IPLsToLoad load_mp_maps="false" load_sp_maps="false" />
            //<IPLsToRemove />
            //<InteriorsToEnable />
            //<InteriorsToCap />
            //<WeatherToSet></WeatherToSet>
            //<StartTaskSequencesOnLoad>true</StartTaskSequencesOnLoad>
            //<ReferenceCoords>
            //	<X>-180.65478</X>
            //	<Y>100.87645</Y>
            //	<Z>100.05556</Z>
            //</ReferenceCoords>







            var placements = root.SelectNodes("Placement");

            foreach (XmlNode node in placements)
            {
                MenyooXmlPlacement pl = new MenyooXmlPlacement();
                pl.Init(node);

                Placements.Add(pl);
            }

        }




    }




    public class MenyooXmlPlacement
    {

        public uint ModelHash { get; set; }
        public int Type { get; set; }
        public bool Dynamic { get; set; }
        public bool FrozenPos { get; set; }
        public string HashName { get; set; }
        public int InitialHandle { get; set; }
        public List<MenyooXmlObjectProperty> ObjectProperties { get; set; }
        public int OpacityLevel { get; set; }
        public float LodDistance { get; set; }
        public bool IsVisible { get; set; }
        public int MaxHealth { get; set; }
        public int Health { get; set; }
        public bool HasGravity { get; set; }
        public bool IsOnFire { get; set; }
        public bool IsInvincible { get; set; }
        public bool IsBulletProof { get; set; }
        public bool IsCollisionProof { get; set; }
        public bool IsExplosionProof { get; set; }
        public bool IsFireProof { get; set; }
        public bool IsMeleeProof { get; set; }
        public bool IsOnlyDamagedByPlayer { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 RotationYawPitchRoll { get; set; }
        public bool Attachment_isAttached { get; set; }

        public Vector4 Rotation
        {
            get
            {
                var pry = RotationYawPitchRoll * -(float)(Math.PI / 180.0);
                return Quaternion.RotationYawPitchRoll(pry.Z, pry.Y, pry.X).ToVector4();
            }
        }



        public void Init(XmlNode node)
        {

            XmlElement enode = node as XmlElement;

            var hashstr = Xml.GetChildInnerText(node, "ModelHash").ToLowerInvariant();
            if (hashstr.StartsWith("0x")) hashstr = hashstr.Substring(2);
            ModelHash = Convert.ToUInt32(hashstr, 16);

            Type = Xml.GetChildIntInnerText(node, "Type");
            Dynamic = Xml.GetChildBoolInnerText(node, "Dynamic");
            FrozenPos = Xml.GetChildBoolInnerText(node, "FrozenPos");
            HashName = Xml.GetChildInnerText(node, "HashName");
            InitialHandle = Xml.GetChildIntInnerText(node, "InitialHandle");

            if (enode != null)
            {
                var objprops = Xml.GetChild(enode, "ObjectProperties");
                ObjectProperties = new List<MenyooXmlObjectProperty>();
                if (objprops != null)
                {
                    foreach (XmlNode objpropn in objprops.ChildNodes)
                    {
                        MenyooXmlObjectProperty pr = new MenyooXmlObjectProperty();
                        pr.Name = objpropn.Name;
                        pr.Value = objpropn.InnerText;
                        ObjectProperties.Add(pr);
                    }
                }

                var posrot = Xml.GetChild(enode, "PositionRotation");
                var px = Xml.GetChildFloatInnerText(posrot, "X");
                var py = Xml.GetChildFloatInnerText(posrot, "Y");
                var pz = Xml.GetChildFloatInnerText(posrot, "Z");
                var rp = Xml.GetChildFloatInnerText(posrot, "Pitch");
                var rr = Xml.GetChildFloatInnerText(posrot, "Roll");
                var ry = Xml.GetChildFloatInnerText(posrot, "Yaw");
                Position = new Vector3(px, py, pz);
                RotationYawPitchRoll = new Vector3(ry, rp, rr);
            }

            OpacityLevel = Xml.GetChildIntInnerText(node, "OpacityLevel");
            LodDistance = Xml.GetChildFloatInnerText(node, "LodDistance");
            IsVisible = Xml.GetChildBoolInnerText(node, "IsVisible");
            MaxHealth = Xml.GetChildIntInnerText(node, "MaxHealth");
            Health = Xml.GetChildIntInnerText(node, "Health");
            HasGravity = Xml.GetChildBoolInnerText(node, "HasGravity");
            IsOnFire = Xml.GetChildBoolInnerText(node, "IsOnFire");
            IsInvincible = Xml.GetChildBoolInnerText(node, "IsInvincible");
            IsBulletProof = Xml.GetChildBoolInnerText(node, "IsBulletProof");
            IsCollisionProof = Xml.GetChildBoolInnerText(node, "IsCollisionProof");
            IsExplosionProof = Xml.GetChildBoolInnerText(node, "IsExplosionProof");
            IsFireProof = Xml.GetChildBoolInnerText(node, "IsFireProof");
            IsMeleeProof = Xml.GetChildBoolInnerText(node, "IsMeleeProof");
            IsOnlyDamagedByPlayer = Xml.GetChildBoolInnerText(node, "IsOnlyDamagedByPlayer");
            Attachment_isAttached = Xml.GetChildBoolAttribute(node, "Attachment", "isAttached");


        }



        public override string ToString()
        {
            return Type.ToString() + ": " + HashName + ": " + Position.ToString();
        }

    }

    public class MenyooXmlObjectProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            return Name + ": " + Value;
        }
    }


}
