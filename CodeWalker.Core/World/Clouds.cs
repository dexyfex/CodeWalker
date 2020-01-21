using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace CodeWalker.World
{
    public class Clouds
    {
        public volatile bool Inited = false;

        public Weather Weather;
        public Timecycle Timecycle;
        public Dictionary<string, CloudAnimSetting> AnimSettings { get; set; }
        public CloudAnimOverrides AnimOverrides = new CloudAnimOverrides();

        public CloudHatManager HatManager;
        public CloudSettingsMap SettingsMap;


        public Clouds()
        {
            AnimSettings = new Dictionary<string, CloudAnimSetting>();
            AddAnimSetting(new CloudAnimSetting("UVOffset1.X", "UV Offset 1 X", -1.0f, 1.0f, 0.0f));
            AddAnimSetting(new CloudAnimSetting("UVOffset1.Y", "UV Offset 1 Y", -1.0f, 1.0f, 0.0f));
            AddAnimSetting(new CloudAnimSetting("UVOffset2.X", "UV Offset 2 X", -1.0f, 1.0f, 0.0f));
            AddAnimSetting(new CloudAnimSetting("UVOffset2.Y", "UV Offset 2 Y", -1.0f, 1.0f, 0.0f));
            AddAnimSetting(new CloudAnimSetting("UVOffset3.X", "UV Offset 3 X", -1.0f, 1.0f, 0.0f));
            AddAnimSetting(new CloudAnimSetting("UVOffset3.Y", "UV Offset 3 Y", -1.0f, 1.0f, 0.0f));
            //AddAnimSetting(new CloudAnimSetting("RescaleUV1.X", "Rescale UV 1 X", 0.0f, 50.0f, 1.0f));
            //AddAnimSetting(new CloudAnimSetting("RescaleUV1.Y", "Rescale UV 1 Y", 0.0f, 50.0f, 1.0f));
            //AddAnimSetting(new CloudAnimSetting("RescaleUV2.X", "Rescale UV 2 X", 0.0f, 50.0f, 1.0f));
            //AddAnimSetting(new CloudAnimSetting("RescaleUV2.Y", "Rescale UV 2 Y", 0.0f, 50.0f, 1.0f));
            //AddAnimSetting(new CloudAnimSetting("RescaleUV3.X", "Rescale UV 3 X", 0.0f, 50.0f, 1.0f));
            //AddAnimSetting(new CloudAnimSetting("RescaleUV3.Y", "Rescale UV 3 Y", 0.0f, 50.0f, 1.0f));
            //AddAnimSetting(new CloudAnimSetting("cloudLayerAnimScale1.X", "Anim Scale 1 X", 0.0f, 8.0f, 1.0f));
            //AddAnimSetting(new CloudAnimSetting("cloudLayerAnimScale1.Y", "Anim Scale 1 Y", 0.0f, 8.0f, 1.0f));
            //AddAnimSetting(new CloudAnimSetting("cloudLayerAnimScale2.X", "Anim Scale 2 X", 0.0f, 8.0f, 1.0f));
            //AddAnimSetting(new CloudAnimSetting("cloudLayerAnimScale2.Y", "Anim Scale 2 Y", 0.0f, 8.0f, 1.0f));
            //AddAnimSetting(new CloudAnimSetting("cloudLayerAnimScale3.X", "Anim Scale 3 X", 0.0f, 8.0f, 1.0f));
            //AddAnimSetting(new CloudAnimSetting("cloudLayerAnimScale3.Y", "Anim Scale 3 Y", 0.0f, 8.0f, 1.0f));
        }
        private void AddAnimSetting(CloudAnimSetting setting)
        {
            AnimSettings[setting.Name] = setting;
        }
        private void UpdateAnimOverrides()
        {
            AnimOverrides.UVOffset1.X = AnimSettings["UVOffset1.X"].CurrentValue;
            AnimOverrides.UVOffset1.Y = AnimSettings["UVOffset1.Y"].CurrentValue;
            AnimOverrides.UVOffset2.X = AnimSettings["UVOffset2.X"].CurrentValue;
            AnimOverrides.UVOffset2.Y = AnimSettings["UVOffset2.Y"].CurrentValue;
            AnimOverrides.UVOffset3.X = AnimSettings["UVOffset3.X"].CurrentValue;
            AnimOverrides.UVOffset3.Y = AnimSettings["UVOffset3.Y"].CurrentValue;
            //AnimOverrides.RescaleUV1.X = AnimSettings["RescaleUV1.X"].CurrentValue;
            //AnimOverrides.RescaleUV1.Y = AnimSettings["RescaleUV1.Y"].CurrentValue;
            //AnimOverrides.RescaleUV2.X = AnimSettings["RescaleUV2.X"].CurrentValue;
            //AnimOverrides.RescaleUV2.Y = AnimSettings["RescaleUV2.Y"].CurrentValue;
            //AnimOverrides.RescaleUV3.X = AnimSettings["RescaleUV3.X"].CurrentValue;
            //AnimOverrides.RescaleUV3.Y = AnimSettings["RescaleUV3.Y"].CurrentValue;
            //AnimOverrides.cloudLayerAnimScale1.X = AnimSettings["cloudLayerAnimScale1.X"].CurrentValue;
            //AnimOverrides.cloudLayerAnimScale1.Y = AnimSettings["cloudLayerAnimScale1.Y"].CurrentValue;
            //AnimOverrides.cloudLayerAnimScale2.X = AnimSettings["cloudLayerAnimScale2.X"].CurrentValue;
            //AnimOverrides.cloudLayerAnimScale2.Y = AnimSettings["cloudLayerAnimScale2.Y"].CurrentValue;
            //AnimOverrides.cloudLayerAnimScale3.X = AnimSettings["cloudLayerAnimScale3.X"].CurrentValue;
            //AnimOverrides.cloudLayerAnimScale3.Y = AnimSettings["cloudLayerAnimScale3.Y"].CurrentValue;
        }

        public void Init(GameFileCache gameFileCache, Action<string> updateStatus, Weather weather)
        {
            Weather = weather;
            Timecycle = weather.Timecycle;
            var rpfman = gameFileCache.RpfMan;

            string filename = "common.rpf\\data\\clouds.xml";

            //TODO: RpfMan should be able to get the right version? or maybe let gameFileCache do it!
            string kffilename = "common.rpf\\data\\cloudkeyframes.xml";
            if (gameFileCache.EnableDlc)
            {
                kffilename = "update\\update.rpf\\common\\data\\cloudkeyframes.xml";
            }

            XmlDocument cloudsxml = rpfman.GetFileXml(filename);
            XmlDocument cloudskfxml = rpfman.GetFileXml(kffilename);

            HatManager = new CloudHatManager();
            HatManager.Init(cloudsxml.DocumentElement); //CloudHatManager

            SettingsMap = new CloudSettingsMap();
            SettingsMap.Init(cloudskfxml.DocumentElement); //CloudSettingsMap

            Inited = true;
        }


        public void Update(float elapsed)
        {
            UpdateAnimOverrides();
        }


    }


    public class CloudHatManager
    {
        public CloudHatFrag[] CloudHatFrags { get; set; }
        public float DesiredTransitionTimeSec { get; set; }
        public Vector3 CamPositionScaler { get; set; }
        public float AltitudeScrollScaler { get; set; }

        public void Init(XmlElement xml)
        {
            List<CloudHatFrag> fraglist = new List<CloudHatFrag>();
            XmlNodeList frags = xml.SelectNodes("mCloudHatFrags/Item");
            foreach (XmlNode node in frags)
            {
                XmlElement fragel = node as XmlElement;
                if (fragel != null)
                {
                    CloudHatFrag frag = new CloudHatFrag();
                    frag.Init(fragel);
                    fraglist.Add(frag);
                }
            }
            CloudHatFrags = fraglist.ToArray();

            DesiredTransitionTimeSec = Xml.GetChildFloatAttribute(xml, "mDesiredTransitionTimeSec", "value");
            CamPositionScaler = Xml.GetChildVector3Attributes(xml, "mCamPositionScaler");
            AltitudeScrollScaler = Xml.GetChildFloatAttribute(xml, "mAltitudeScrollScaler", "value");
        }

        public CloudHatFrag FindFrag(string name)
        {
            for (int i = 0; i < CloudHatFrags.Length; i++)
            {
                CloudHatFrag f = CloudHatFrags[i];
                if (f.Name == name)
                {
                    return f;
                }
            }
            return null;
        }

    }

    public class CloudHatFrag
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public string Name { get; set; }
        public CloudHatFragLayer[] Layers { get; set; }
        public float TransitionAlphaRange { get; set; }
        public float TransitionMidPoint { get; set; }
        public bool Enabled { get; set; }
        public Vector3 AngularVelocity { get; set; }
        public Vector3 AnimBlendWeights { get; set; }
        public Vector2[] UVVelocity { get; set; }
        public byte[] AnimMode { get; set; }
        public bool[] ShowLayer { get; set; }
        public bool EnableAnimations { get; set; }

        public void Init(XmlElement xml)
        {
            Position = Xml.GetChildVector3Attributes(xml, "mPosition");
            Rotation = Xml.GetChildVector3Attributes(xml, "mRotation");
            Scale = Xml.GetChildVector3Attributes(xml, "mScale");
            Name = Xml.GetChildInnerText(xml, "mName");

            List<CloudHatFragLayer> layerlist = new List<CloudHatFragLayer>();
            XmlNodeList layersxml = xml.SelectNodes("mLayers/Item");
            foreach (XmlNode node in layersxml)
            {
                XmlElement layerel = node as XmlElement;
                if (layerel != null)
                {
                    CloudHatFragLayer layer = new CloudHatFragLayer();
                    layer.Init(layerel);
                    layerlist.Add(layer);
                }
            }
            Layers = layerlist.ToArray();

            TransitionAlphaRange = Xml.GetChildFloatAttribute(xml, "mTransitionAlphaRange", "value");
            TransitionMidPoint = Xml.GetChildFloatAttribute(xml, "mTransitionMidPoint", "value");
            Enabled = Xml.GetChildBoolAttribute(xml, "mEnabled", "value");
            AngularVelocity = Xml.GetChildVector3Attributes(xml, "mAngularVelocity");
            AnimBlendWeights = Xml.GetChildVector3Attributes(xml, "mAnimBlendWeights");

            string uvvelocitystr = Xml.GetChildInnerText(xml, "mUVVelocity").Trim();
            string[] uvvelocities = uvvelocitystr.Split('\n');
            UVVelocity = new Vector2[uvvelocities.Length];
            for (int i = 0; i < uvvelocities.Length; i++)
            {
                Vector2 vel = Vector2.Zero;
                string uvvel = uvvelocities[i].Trim();
                string[] uvvelc = uvvel.Split('\t');
                if (uvvelc.Length == 2)
                {
                    FloatUtil.TryParse(uvvelc[0].Trim(), out vel.X);
                    FloatUtil.TryParse(uvvelc[1].Trim(), out vel.Y);
                }
                UVVelocity[i] = vel;
            }

            string animmodestr = Xml.GetChildInnerText(xml, "mAnimMode").Trim();
            string[] animmodes = animmodestr.Split('\n');
            AnimMode = new byte[animmodes.Length];
            for (int i = 0; i < animmodes.Length; i++)
            {
                byte.TryParse(animmodes[i].Trim(), out AnimMode[i]);
            }


            //string showlayerstr = Xml.GetChildInnerText(xml, "mShowLayer").Trim();
            XmlNodeList showlayersxml = xml.SelectNodes("mShowLayer/Item");
            ShowLayer = new bool[showlayersxml.Count];
            for (int i = 0; i < showlayersxml.Count; i++)
            {
                XmlNode slnode = showlayersxml[i];
                if (slnode is XmlElement)
                {
                    ShowLayer[i] = Xml.GetBoolAttribute(slnode, "value");
                }
            }


            EnableAnimations = Xml.GetChildBoolAttribute(xml, "mEnableAnimations", "value");

        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class CloudHatFragLayer
    {
        public string Filename { get; set; }
        public float CostFactor { get; set; }
        public float RotationScale { get; set; }
        public float CamPositionScalerAdjust { get; set; }
        public float TransitionInTimePercent { get; set; }
        public float TransitionOutTimePercent { get; set; }
        public float TransitionInDelayPercent { get; set; }
        public float TransitionOutDelayPercent { get; set; }
        public float HeightTigger { get; set; }
        public float HeightFadeRange { get; set; }

        public void Init(XmlElement xml)
        {
            Filename = Xml.GetChildInnerText(xml, "mFilename");
            CostFactor = Xml.GetChildFloatAttribute(xml, "mCostFactor", "value");
            RotationScale = Xml.GetChildFloatAttribute(xml, "mRotationScale", "value");
            CamPositionScalerAdjust = Xml.GetChildFloatAttribute(xml, "mCamPositionScalerAdjust", "value");
            TransitionInTimePercent = Xml.GetChildFloatAttribute(xml, "mTransitionInTimePercent", "value");
            TransitionOutTimePercent = Xml.GetChildFloatAttribute(xml, "mTransitionOutTimePercent", "value");
            TransitionInDelayPercent = Xml.GetChildFloatAttribute(xml, "mTransitionInDelayPercent", "value");
            TransitionOutDelayPercent = Xml.GetChildFloatAttribute(xml, "mTransitionOutDelayPercent", "value");
            HeightTigger = Xml.GetChildFloatAttribute(xml, "mHeightTigger", "value");
            HeightFadeRange = Xml.GetChildFloatAttribute(xml, "mHeightFadeRange", "value");
        }

        public override string ToString()
        {
            return Filename;
        }
    }


    public class CloudSettingsMap
    {
        public float[] KeyframeTimes { get; set; }
        public Dictionary<string, CloudSettingsMapItem> SettingsMap { get; set; }

        public void Init(XmlElement xml)
        {

            string kftstr = Xml.GetChildInnerText(xml, "KeyframeTimes").Trim();
            string[] kftarr = kftstr.Split('\n');
            KeyframeTimes = new float[kftarr.Length];
            for (int i = 0; i < kftarr.Length; i++)
            {
                FloatUtil.TryParse(kftarr[i].Trim(), out KeyframeTimes[i]);
            }


            SettingsMap = new Dictionary<string, CloudSettingsMapItem>();
            XmlNodeList mapxml = xml.SelectNodes("SettingsMap/Item");
            foreach (XmlNode node in mapxml)
            {
                XmlElement itemel = node as XmlElement;
                if (itemel != null)
                {
                    CloudSettingsMapItem item = new CloudSettingsMapItem();
                    item.Init(itemel);
                    SettingsMap[item.Name] = item;
                }
            }

        }
    }

    public class CloudSettingsMapItem
    {
        public string Name { get; set; }
        public CloudSettingsMapCloudList CloudList { get; set; } = new CloudSettingsMapCloudList();
        public CloudSettingsMapKeyData CloudColor { get; set; } = new CloudSettingsMapKeyData();
        public CloudSettingsMapKeyData CloudLightColor { get; set; } = new CloudSettingsMapKeyData();
        public CloudSettingsMapKeyData CloudAmbientColor { get; set; } = new CloudSettingsMapKeyData();
        public CloudSettingsMapKeyData CloudSkyColor { get; set; } = new CloudSettingsMapKeyData();
        public CloudSettingsMapKeyData CloudBounceColor { get; set; } = new CloudSettingsMapKeyData();
        public CloudSettingsMapKeyData CloudEastColor { get; set; } = new CloudSettingsMapKeyData();
        public CloudSettingsMapKeyData CloudWestColor { get; set; } = new CloudSettingsMapKeyData();
        public CloudSettingsMapKeyData CloudScaleFillColors { get; set; } = new CloudSettingsMapKeyData();
        public CloudSettingsMapKeyData CloudDensityShift_Scale_ScatteringConst_Scale { get; set; } = new CloudSettingsMapKeyData();
        public CloudSettingsMapKeyData CloudPiercingLightPower_Strength_NormalStrength_Thickness { get; set; } = new CloudSettingsMapKeyData();
        public CloudSettingsMapKeyData CloudScaleDiffuseFillAmbient_WrapAmount { get; set; } = new CloudSettingsMapKeyData();

        public void Init(XmlNode xml)
        {
            Name = Xml.GetChildInnerText(xml, "Name");
            var snode = xml.SelectSingleNode("Settings");
            CloudList.Init(snode.SelectSingleNode("CloudList"));
            CloudColor.Init(snode.SelectSingleNode("CloudColor"));
            CloudLightColor.Init(snode.SelectSingleNode("CloudLightColor"));
            CloudAmbientColor.Init(snode.SelectSingleNode("CloudAmbientColor"));
            CloudSkyColor.Init(snode.SelectSingleNode("CloudSkyColor"));
            CloudBounceColor.Init(snode.SelectSingleNode("CloudBounceColor"));
            CloudEastColor.Init(snode.SelectSingleNode("CloudEastColor"));
            CloudWestColor.Init(snode.SelectSingleNode("CloudWestColor"));
            CloudScaleFillColors.Init(snode.SelectSingleNode("CloudScaleFillColors"));
            CloudDensityShift_Scale_ScatteringConst_Scale.Init(snode.SelectSingleNode("CloudDensityShift_Scale_ScatteringConst_Scale"));
            CloudPiercingLightPower_Strength_NormalStrength_Thickness.Init(snode.SelectSingleNode("CloudPiercingLightPower_Strength_NormalStrength_Thickness"));
            CloudScaleDiffuseFillAmbient_WrapAmount.Init(snode.SelectSingleNode("CloudScaleDiffuseFillAmbient_WrapAmount"));
        }


        public override string ToString()
        {
            return Name;
        }
    }

    public class CloudSettingsMapCloudList
    {
        public int[] Probability { get; set; }
        public int[] Bits { get; set; } //one bit for each cloud hat frag

        public void Init(XmlNode xml)
        {
            string pstr = Xml.GetChildInnerText(xml, "mProbability").Trim();
            string bstr = Xml.GetChildInnerText(xml, "mBits").Trim();

            string[] parr = pstr.Split('\n');
            string[] barr = bstr.Split('\n');

            Probability = new int[parr.Length];
            Bits = new int[barr.Length];

            for (int i = 0; i < parr.Length; i++)
            {
                int.TryParse(parr[i].Trim(), out Probability[i]);
            }
            for (int i = 0; i < barr.Length; i++)
            {
                Bits[i] = Convert.ToInt32(barr[i].Trim(), 16);
            }
        }

    }

    public class CloudSettingsMapKeyData
    {
        public int numKeyEntries { get; set; }
        public Dictionary<float, Vector4> keyEntryData { get; set; }

        public void Init(XmlNode xml)
        {
            var kdxml = xml.SelectSingleNode("keyData");

            numKeyEntries = Xml.GetChildIntAttribute(kdxml, "numKeyEntries", "value");

            string kestr = Xml.GetChildInnerText(kdxml, "keyEntryData").Trim();
            string[] kearr = kestr.Split('\n');
            keyEntryData = new Dictionary<float, Vector4>();
            for (int i = 0; i < kearr.Length; i++)
            {
                string kvstr = kearr[i].Trim();
                string[] kvarr = kvstr.Split('\t');
                float key = 0.0f;
                Vector4 val = Vector4.Zero;
                if (kvarr.Length >= 5)
                {
                    FloatUtil.TryParse(kvarr[0].Trim(), out key);
                    FloatUtil.TryParse(kvarr[1].Trim(), out val.X);
                    FloatUtil.TryParse(kvarr[2].Trim(), out val.Y);
                    FloatUtil.TryParse(kvarr[3].Trim(), out val.Z);
                    FloatUtil.TryParse(kvarr[4].Trim(), out val.W);
                }
                else
                { }
                keyEntryData[key] = val;
            }

        }

    }


    public class CloudAnimSetting
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float DefaultValue { get; set; }
        public float CurrentValue { get; set; }


        public CloudAnimSetting(string name, string displayname, float minval, float maxval, float defaultval)
        {
            Name = name;
            DisplayName = displayname;
            MinValue = minval;
            MaxValue = maxval;
            DefaultValue = defaultval;
            CurrentValue = defaultval;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public class CloudAnimOverrides
    {
        public Vector2 UVOffset1 = Vector2.Zero;
        public Vector2 UVOffset2 = Vector2.Zero;
        public Vector2 UVOffset3 = Vector2.Zero;
        public Vector2 RescaleUV1 = Vector2.One;
        public Vector2 RescaleUV2 = Vector2.One;
        public Vector2 RescaleUV3 = Vector2.One;
        public Vector2 cloudLayerAnimScale1 = Vector2.One;
        public Vector2 cloudLayerAnimScale2 = Vector2.One;
        public Vector2 cloudLayerAnimScale3 = Vector2.One;
    }

}
