using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWalker.World
{
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Vehicle
    {
        public string Name { get; set; } = string.Empty;
        public MetaHash NameHash { get; set; } = 0;//base vehicle name hash
        public MetaHash ModelHash { get; set; } = 0;//vehicle model name hash, can be _hi

        public VehicleInitData InitData { get; set; } = null;
        public YftFile Yft { get; set; } = null;

        public YcdFile ConvRoofDict { get; set; } = null;
        public ClipMapEntry ConvRoofClip { get; set; } = null;

        public string DisplayMake { get; set; } = string.Empty;//make display name
        public string DisplayName { get; set; } = string.Empty;//model display name

        public YmapEntityDef RenderEntity = new YmapEntityDef(); //placeholder entity object for rendering

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;


        public void Init(string name, GameFileCache gfc, bool hidef = true)
        {
            Name = name;
            var modelnamel = name.ToLowerInvariant();
            MetaHash modelhash = JenkHash.GenHash(modelnamel);
            MetaHash modelhashhi = JenkHash.GenHash(modelnamel + "_hi");
            var yfthash = hidef ? modelhashhi : modelhash;

            VehicleInitData vid = null;
            if (gfc.VehiclesInitDict.TryGetValue(modelhash, out vid))
            {
                bool vehiclechange = NameHash != modelhash;
                ConvRoofDict = null;
                ConvRoofClip = null;
                ModelHash = yfthash;
                NameHash = modelhash;
                InitData = vid;
                Yft = gfc.GetYft(ModelHash);
                while ((Yft != null) && (!Yft.Loaded))
                {
                    Thread.Sleep(1);//kinda hacky
                    Yft = gfc.GetYft(ModelHash);
                }

                DisplayMake = GlobalText.TryGetString(JenkHash.GenHash(vid.vehicleMakeName.ToLowerInvariant()));
                DisplayName = GlobalText.TryGetString(JenkHash.GenHash(vid.gameName.ToLowerInvariant()));

                if (!string.IsNullOrEmpty(vid.animConvRoofDictName) && (vid.animConvRoofDictName.ToLowerInvariant() != "null"))
                {
                    var ycdhash = JenkHash.GenHash(vid.animConvRoofDictName.ToLowerInvariant());
                    var cliphash = JenkHash.GenHash(vid.animConvRoofName?.ToLowerInvariant());
                    ConvRoofDict = gfc.GetYcd(ycdhash);
                    while ((ConvRoofDict != null) && (!ConvRoofDict.Loaded))
                    {
                        Thread.Sleep(1);//kinda hacky
                        ConvRoofDict = gfc.GetYcd(ycdhash);
                    }
                    ClipMapEntry cme = null;
                    ConvRoofDict?.ClipMap?.TryGetValue(cliphash, out cme);
                    ConvRoofClip = cme;
                }
            }
            else
            {
                ModelHash = 0;
                NameHash = 0;
                InitData = null;
                Yft = null;
                DisplayMake = "-";
                DisplayName = "-";
                ConvRoofDict = null;
                ConvRoofClip = null;
            }


            UpdateEntity();
        }



        public void UpdateEntity()
        {
            RenderEntity.SetPosition(Position);
            RenderEntity.SetOrientation(Rotation);
        }


    }
}
