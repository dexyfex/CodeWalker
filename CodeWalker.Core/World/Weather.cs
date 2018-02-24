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
    public class Weather
    {

        public Dictionary<string, WeatherGpuFx> WeatherGpuFx { get; set; } = new Dictionary<string, WeatherGpuFx>();
        public Dictionary<string, WeatherType> WeatherTypes { get; set; } = new Dictionary<string, WeatherType>();
        public List<WeatherCycle> WeatherCycles { get; set; } = new List<WeatherCycle>();
        public WeatherValues CurrentValues;

        public volatile bool Inited = false;
        public WeatherType CurrentWeatherType;
        public WeatherType NextWeatherType;
        public float WeatherChangeTime = 0.33f;
        public float CurrentWeatherChangeTime = 0.0f;
        public float CurrentWeatherChangeBlend = 0.0f;
        public string Region = "GLOBAL"; //URBAN or GLOBAL..
        public WeatherCycleKeyframeRegion CurrentWeatherRegion;
        public WeatherCycleKeyframeRegion NextWeatherRegion;

        public Timecycle Timecycle;
        public TimecycleMods TimecycleMods;

        public void Init(GameFileCache gameFileCache, Action<string> updateStatus, Timecycle timecycle)
        {
            Timecycle = timecycle;
            var rpfman = gameFileCache.RpfMan;

            //TODO: RpfMan should be able to get the right version? or maybe let gameFileCache do it!
            string filename = "common.rpf\\data\\levels\\gta5\\weather.xml";
            if (gameFileCache.EnableDlc)
            {
                filename = "update\\update.rpf\\common\\data\\levels\\gta5\\weather.xml";
            }

            XmlDocument weatherxml = rpfman.GetFileXml(filename);

            XmlElement weather = weatherxml.DocumentElement;

            XmlNodeList weathergpufx = weather.SelectNodes("WeatherGpuFx/Item");
            WeatherGpuFx.Clear();
            for (int i = 0; i < weathergpufx.Count; i++)
            {
                var weathergpufxi = new WeatherGpuFx();
                weathergpufxi.Init(weathergpufx[i]);
                WeatherGpuFx[weathergpufxi.Name] = weathergpufxi;
            }

            XmlNodeList weathertypes = weather.SelectNodes("WeatherTypes/Item");
            WeatherTypes.Clear();
            for (int i = 0; i < weathertypes.Count; i++)
            {
                var weathertype = new WeatherType();
                weathertype.Init(gameFileCache, weathertypes[i]);
                WeatherTypes[weathertype.Name] = weathertype;
            }

            XmlNodeList weathercycles = weather.SelectNodes("WeatherCycles/Item");
            WeatherCycles.Clear();
            for (int i = 0; i < weathercycles.Count; i++)
            {
                var weathercycle = new WeatherCycle();
                weathercycle.Init(weathercycles[i]);
                WeatherCycles.Add(weathercycle);
            }



            if (WeatherTypes.Count > 0)
            {
                CurrentWeatherType = WeatherTypes.Values.First();
                CurrentWeatherRegion = CurrentWeatherType.GetRegion(Region);
                NextWeatherType = CurrentWeatherType;
                NextWeatherRegion = NextWeatherType.GetRegion(Region);
            }


            TimecycleMods = new TimecycleMods();
            TimecycleMods.Init(gameFileCache, updateStatus);


            Inited = true;
        }

        public void Update(float elapsed)
        {
            if (!Inited) return;

            if (CurrentWeatherType != NextWeatherType)
            {
                CurrentWeatherChangeTime += elapsed;
                if (CurrentWeatherChangeTime >= WeatherChangeTime)
                {
                    CurrentWeatherType = NextWeatherType;
                    CurrentWeatherChangeTime = 0.0f;
                }
                CurrentWeatherChangeBlend = Math.Min(CurrentWeatherChangeTime / WeatherChangeTime, 1.0f);
            }
            if (CurrentWeatherType != null)
            {
                CurrentWeatherRegion = CurrentWeatherType.GetRegion(Region);
            }
            if (NextWeatherType != null)
            {
                NextWeatherRegion = NextWeatherType.GetRegion(Region);
            }

            CurrentValues.Update(this);
        }

        public void SetNextWeather(string name)
        {
            WeatherTypes.TryGetValue(name, out NextWeatherType);
            if (NextWeatherType == null)
            {
                NextWeatherType = CurrentWeatherType;
            }
            else
            {
                CurrentWeatherChangeTime = 0.0f;
            }
        }

        public float GetDynamicValue(string name)
        {
            int csi = Timecycle.CurrentSampleIndex;
            float csb = Timecycle.CurrentSampleBlend;
            if ((CurrentWeatherRegion != null) && (NextWeatherRegion != null))
            {
                float cv = CurrentWeatherRegion.GetCurrentValue(name, csi, csb);
                float nv = NextWeatherRegion.GetCurrentValue(name, csi, csb);
                return cv * (1.0f - CurrentWeatherChangeBlend) + nv * CurrentWeatherChangeBlend;
            }
            else if (CurrentWeatherRegion != null)
            {
                return CurrentWeatherRegion.GetCurrentValue(name, csi, csb);
            }
            //throw new Exception("CurrentWeatherRegion was null.");
            return 0.0f;
        }
        public Vector3 GetDynamicRGB(string rname, string gname, string bname)
        {
            int csi = Timecycle.CurrentSampleIndex;
            float csb = Timecycle.CurrentSampleBlend;
            if ((CurrentWeatherRegion != null) && (NextWeatherRegion != null))
            {
                float cvr = CurrentWeatherRegion.GetCurrentValue(rname, csi, csb);
                float cvg = CurrentWeatherRegion.GetCurrentValue(gname, csi, csb);
                float cvb = CurrentWeatherRegion.GetCurrentValue(bname, csi, csb);
                float nvr = NextWeatherRegion.GetCurrentValue(rname, csi, csb);
                float nvg = NextWeatherRegion.GetCurrentValue(gname, csi, csb);
                float nvb = NextWeatherRegion.GetCurrentValue(bname, csi, csb);
                Vector3 cv = new Vector3(cvr, cvg, cvb);
                Vector3 nv = new Vector3(nvr, nvg, nvb);
                return cv * (1.0f - CurrentWeatherChangeBlend) + nv * CurrentWeatherChangeBlend;
            }
            else if (CurrentWeatherRegion != null)
            {
                float cvr = CurrentWeatherRegion.GetCurrentValue(rname, csi, csb);
                float cvg = CurrentWeatherRegion.GetCurrentValue(gname, csi, csb);
                float cvb = CurrentWeatherRegion.GetCurrentValue(bname, csi, csb);
                return new Vector3(cvr, cvg, cvb);
            }
            //throw new Exception("CurrentWeatherRegion was null.");
            return Vector3.Zero;
        }
        public Vector4 GetDynamicRGBA(string rname, string gname, string bname, string aname)
        {
            int csi = Timecycle.CurrentSampleIndex;
            float csb = Timecycle.CurrentSampleBlend;
            if ((CurrentWeatherRegion != null) && (NextWeatherRegion != null))
            {
                float cvr = CurrentWeatherRegion.GetCurrentValue(rname, csi, csb);
                float cvg = CurrentWeatherRegion.GetCurrentValue(gname, csi, csb);
                float cvb = CurrentWeatherRegion.GetCurrentValue(bname, csi, csb);
                float cva = CurrentWeatherRegion.GetCurrentValue(aname, csi, csb);
                float nvr = NextWeatherRegion.GetCurrentValue(rname, csi, csb);
                float nvg = NextWeatherRegion.GetCurrentValue(gname, csi, csb);
                float nvb = NextWeatherRegion.GetCurrentValue(bname, csi, csb);
                float nva = NextWeatherRegion.GetCurrentValue(aname, csi, csb);
                Vector4 cv = new Vector4(cvr, cvg, cvb, cva);
                Vector4 nv = new Vector4(nvr, nvg, nvb, nva);
                return cv * (1.0f - CurrentWeatherChangeBlend) + nv * CurrentWeatherChangeBlend;
            }
            else if (CurrentWeatherRegion != null)
            {
                float cvr = CurrentWeatherRegion.GetCurrentValue(rname, csi, csb);
                float cvg = CurrentWeatherRegion.GetCurrentValue(gname, csi, csb);
                float cvb = CurrentWeatherRegion.GetCurrentValue(bname, csi, csb);
                float cva = CurrentWeatherRegion.GetCurrentValue(aname, csi, csb);
                return new Vector4(cvr, cvg, cvb, cva);
            }
            //throw new Exception("CurrentWeatherRegion was null.");
            return Vector4.Zero;
        }

    }

    public class WeatherGpuFx
    {
        public string Name { get; set; }
        public string SystemType { get; set; }
        public string diffuseName { get; set; }
        public string distortionTexture { get; set; }
        public string diffuseSplashName { get; set; }
        public string driveType { get; set; }
        public float windInfluence { get; set; }
        public float gravity { get; set; }
        public string emitterSettingsName { get; set; }
        public string renderSettingsName { get; set; }

        public void Init(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            SystemType = Xml.GetChildInnerText(node, "SystemType");
            diffuseName = Xml.GetChildInnerText(node, "diffuseName");
            distortionTexture = Xml.GetChildInnerText(node, "distortionTexture");
            diffuseSplashName = Xml.GetChildInnerText(node, "diffuseSplashName");
            driveType = Xml.GetChildInnerText(node, "driveType");
            windInfluence = Xml.GetChildFloatAttribute(node, "windInfluence", "value");
            gravity = Xml.GetChildFloatAttribute(node, "gravity", "value");
            emitterSettingsName = Xml.GetChildInnerText(node, "emitterSettingsName");
            renderSettingsName = Xml.GetChildInnerText(node, "renderSettingsName");
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class WeatherType
    {
        public MetaHash NameHash { get; set; }
        public string Name { get; set; }
        public float Sun { get; set; }
        public float Cloud { get; set; }
        public float WindMin { get; set; }
        public float WindMax { get; set; }
        public float Rain { get; set; }
        public float Snow { get; set; }
        public float SnowMist { get; set; }
        public float Fog { get; set; }
        public float RippleBumpiness { get; set; }
        public float RippleMinBumpiness { get; set; }
        public float RippleMaxBumpiness { get; set; }
        public float RippleBumpinessWindScale { get; set; }
        public float RippleScale { get; set; }
        public float RippleSpeed { get; set; }
        public float RippleVelocityTransfer { get; set; }
        public float OceanBumpiness { get; set; }
        public float DeepOceanScale { get; set; }
        public float OceanNoiseMinAmplitude { get; set; }
        public float OceanWaveAmplitude { get; set; }
        public float ShoreWaveAmplitude { get; set; }
        public float OceanWaveWindScale { get; set; }
        public float ShoreWaveWindScale { get; set; }
        public float OceanWaveMinAmplitude { get; set; }
        public float ShoreWaveMinAmplitude { get; set; }
        public float OceanWaveMaxAmplitude { get; set; }
        public float ShoreWaveMaxAmplitude { get; set; }
        public float OceanFoamIntensity { get; set; }
        public float OceanFoamScale { get; set; }
        public float RippleDisturb { get; set; }
        public float Lightning { get; set; }
        public float Sandstorm { get; set; }
        public string OldSettingName { get; set; }
        public string DropSettingName { get; set; }
        public string MistSettingName { get; set; }
        public string GroundSettingName { get; set; }
        public string TimeCycleFilename { get; set; }
        public string CloudSettingsName { get; set; }

        public WeatherCycleKeyframeData TimeCycleData;

        public void Init(GameFileCache gameFileCache, XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            NameHash = new MetaHash(JenkHash.GenHash(Name.ToLowerInvariant()));
            Sun = Xml.GetChildFloatAttribute(node, "Sun", "value");
            Cloud = Xml.GetChildFloatAttribute(node, "Cloud", "value");
            WindMin = Xml.GetChildFloatAttribute(node, "WindMin", "value");
            WindMax = Xml.GetChildFloatAttribute(node, "WindMax", "value");
            Rain = Xml.GetChildFloatAttribute(node, "Rain", "value");
            Snow = Xml.GetChildFloatAttribute(node, "Snow", "value");
            SnowMist = Xml.GetChildFloatAttribute(node, "SnowMist", "value");
            Fog = Xml.GetChildFloatAttribute(node, "Fog", "value");
            RippleBumpiness = Xml.GetChildFloatAttribute(node, "RippleBumpiness", "value");
            RippleMinBumpiness = Xml.GetChildFloatAttribute(node, "RippleMinBumpiness", "value");
            RippleMaxBumpiness = Xml.GetChildFloatAttribute(node, "RippleMaxBumpiness", "value");
            RippleBumpinessWindScale = Xml.GetChildFloatAttribute(node, "RippleBumpinessWindScale", "value");
            RippleScale = Xml.GetChildFloatAttribute(node, "RippleScale", "value");
            RippleSpeed = Xml.GetChildFloatAttribute(node, "RippleSpeed", "value");
            RippleVelocityTransfer = Xml.GetChildFloatAttribute(node, "RippleVelocityTransfer", "value");
            OceanBumpiness = Xml.GetChildFloatAttribute(node, "OceanBumpiness", "value");
            DeepOceanScale = Xml.GetChildFloatAttribute(node, "DeepOceanScale", "value");
            OceanNoiseMinAmplitude = Xml.GetChildFloatAttribute(node, "OceanNoiseMinAmplitude", "value");
            OceanWaveAmplitude = Xml.GetChildFloatAttribute(node, "OceanWaveAmplitude", "value");
            ShoreWaveAmplitude = Xml.GetChildFloatAttribute(node, "ShoreWaveAmplitude", "value");
            OceanWaveWindScale = Xml.GetChildFloatAttribute(node, "OceanWaveWindScale", "value");
            ShoreWaveWindScale = Xml.GetChildFloatAttribute(node, "ShoreWaveWindScale", "value");
            OceanWaveMinAmplitude = Xml.GetChildFloatAttribute(node, "OceanWaveMinAmplitude", "value");
            ShoreWaveMinAmplitude = Xml.GetChildFloatAttribute(node, "ShoreWaveMinAmplitude", "value");
            OceanWaveMaxAmplitude = Xml.GetChildFloatAttribute(node, "OceanWaveMaxAmplitude", "value");
            ShoreWaveMaxAmplitude = Xml.GetChildFloatAttribute(node, "ShoreWaveMaxAmplitude", "value");
            OceanFoamIntensity = Xml.GetChildFloatAttribute(node, "OceanFoamIntensity", "value");
            OceanFoamScale = Xml.GetChildFloatAttribute(node, "OceanFoamScale", "value");
            RippleDisturb = Xml.GetChildFloatAttribute(node, "RippleDisturb", "value");
            Lightning = Xml.GetChildFloatAttribute(node, "Lightning", "value");
            Sandstorm = Xml.GetChildFloatAttribute(node, "Sandstorm", "value");
            OldSettingName = Xml.GetChildInnerText(node, "OldSettingName");
            DropSettingName = Xml.GetChildInnerText(node, "DropSettingName");
            MistSettingName = Xml.GetChildInnerText(node, "MistSettingName");
            GroundSettingName = Xml.GetChildInnerText(node, "GroundSettingName");
            TimeCycleFilename = Xml.GetChildInnerText(node, "TimeCycleFilename");
            CloudSettingsName = Xml.GetChildInnerText(node, "CloudSettingsName");


            if (!string.IsNullOrEmpty(TimeCycleFilename))
            {

                //TODO: RpfMan should be able to get the right version? or maybe let gameFileCache do it!
                string fname = TimeCycleFilename.ToLowerInvariant();
                bool useupd = gameFileCache.EnableDlc;
                if (useupd)
                {
                    fname = fname.Replace("common:", "update/update.rpf/common");
                }
                XmlDocument tcxml = gameFileCache.RpfMan.GetFileXml(fname);
                if (useupd && !tcxml.HasChildNodes)
                {
                    fname = TimeCycleFilename.ToLowerInvariant();
                    tcxml = gameFileCache.RpfMan.GetFileXml(fname);
                }

                foreach (XmlNode cycle in tcxml.DocumentElement.ChildNodes)
                {
                    TimeCycleData = new WeatherCycleKeyframeData();
                    TimeCycleData.Init(cycle);
                }
            }

        }

        public WeatherCycleKeyframeRegion GetRegion(string name)
        {
            if ((TimeCycleData != null) && (TimeCycleData.Regions != null))
            {
                WeatherCycleKeyframeRegion r;
                if (TimeCycleData.Regions.TryGetValue(name, out r))
                {
                    return r;
                }
                return TimeCycleData.Regions.Values.FirstOrDefault();
            }
            return null;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class WeatherCycle
    {
        public string CycleName { get; set; }
        public float TimeMult { get; set; }

        public void Init(XmlNode node)
        {
            CycleName = Xml.GetChildInnerText(node, "CycleName");
            TimeMult = Xml.GetChildFloatAttribute(node, "TimeMult", "value");
        }

        public override string ToString()
        {
            return CycleName + ", " + TimeMult.ToString();
        }
    }

    public class WeatherCycleKeyframeData
    {
        public string Name { get; set; }
        public int RegionCount { get; set; }
        public Dictionary<string, WeatherCycleKeyframeRegion> Regions { get; set; }

        public void Init(XmlNode node)
        {
            //read cycle node
            Name = Xml.GetStringAttribute(node, "name");
            RegionCount = Xml.GetIntAttribute(node, "regions");
            Regions = new Dictionary<string, WeatherCycleKeyframeRegion>();
            foreach (XmlNode child in node.ChildNodes)
            {
                WeatherCycleKeyframeRegion r = new WeatherCycleKeyframeRegion();
                r.Init(child);
                Regions[r.Name] = r;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
    public class WeatherCycleKeyframeRegion
    {
        public string Name { get; set; }
        public Dictionary<string, WeatherCycleKeyframeDataEntry> Data { get; set; }

        public void Init(XmlNode node)
        {
            //read region node
            Name = Xml.GetStringAttribute(node, "name");
            Data = new Dictionary<string, WeatherCycleKeyframeDataEntry>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child != null)
                {
                    WeatherCycleKeyframeDataEntry d = new WeatherCycleKeyframeDataEntry();
                    d.Init(child);
                    Data[d.Name] = d;
                }
            }
        }

        public float GetCurrentValue(string name, int sample, float curblend)
        {
            WeatherCycleKeyframeDataEntry e;
            if (Data.TryGetValue(name, out e))
            {
                if (sample >= e.Values.Length)
                {
                    //System.Windows.Forms.MessageBox.Show("Sample index was out of range: " + sample.ToString());
                    sample = e.Values.Length - 1;
                }
                int nxtsample = (sample < e.Values.Length - 1) ? sample + 1 : 0;
                float cv = e.Values[sample];
                float nv = e.Values[nxtsample];
                return cv * curblend + nv * (1.0f - curblend);
            }
            //throw new Exception("WeatherCycleKeyframeDataEntry " + name + " not found in region " + Name + ".");
            return 0.0f;
        }

        public override string ToString()
        {
            return Name;
        }
    }
    public class WeatherCycleKeyframeDataEntry
    {
        public string Name { get; set; }
        public float[] Values { get; set; }

        public void Init(XmlNode node)
        {
            //read data node
            Name = node.Name;
            string[] strvals = node.InnerText.Trim().Split(' ');
            Values = new float[strvals.Length];
            for (int i = 0; i < strvals.Length; i++)
            {
                if (!FloatUtil.TryParse(strvals[i], out Values[i]))
                {
                    //System.Windows.Forms.MessageBox.Show("Error parsing float value: " + strvals[i] + "\n" +
                    //    "Node: " + node.OuterXml.ToString());
                    //throw new Exception();
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }


    public struct WeatherValues
    {
        public Vector3 sunDirection;
        public Vector3 moonDirection;
        public Vector4 skyZenithCol;
        public Vector4 skyZenithTransitionCol;
        public Vector4 skyZenithTransition;
        public Vector4 skyAzimuthEastCol;
        public Vector4 skyAzimuthWestCol;
        public Vector4 skyAzimuthTransitionCol;
        public float skyAzimuthTransition;
        public float skyHdr;
        public Vector4 skyPlane;
        public Vector3 skySunCol;
        public Vector3 skySunDiscCol;
        public float skySunDiscSize;
        public float skySunHdr;
        public Vector3 skySunMie;
        public float skySunInfluenceRadius;
        public float skySunScatterInten;
        public Vector3 skyMoonCol;
        public float skyMoonDiscSize;
        public float skyMoonIten;
        public float skyMoonInfluenceRadius;
        public float skyMoonScatterInten;
        public float skyStarsIten;
        public Vector4 lightDirCol;
        public Vector4 lightDirAmbCol;
        public float lightDirAmbIntensityMult;
        public float lightDirAmbBounce;
        public Vector4 lightNaturalAmbDown;
        public Vector4 lightNaturalAmbUp;
        public float lightNaturalAmbUpIntensityMult;
        public Vector4 lightArtificialIntDown;
        public Vector4 lightArtificialIntUp;
        public Vector4 lightArtificialExtDown;
        public Vector4 lightArtificialExtUp;

        public void Update(Weather w)
        {
            sunDirection = w.GetDynamicRGB("sun_direction_x", "sun_direction_y", "sun_direction_z");
            moonDirection = w.GetDynamicRGB("moon_direction_x", "moon_direction_y", "moon_direction_z");
            skyZenithCol = w.GetDynamicRGBA("sky_zenith_col_r", "sky_zenith_col_g", "sky_zenith_col_b", "sky_zenith_col_inten");
            skyZenithTransitionCol = w.GetDynamicRGBA("sky_zenith_transition_col_r", "sky_zenith_transition_col_g", "sky_zenith_transition_col_b", "sky_zenith_transition_col_inten");
            //skyZenithTransition = w.GetDynamicRGBA("sky_zenith_transition_position", "sky_zenith_transition_east_blend", "sky_zenith_transition_west_blend", "sky_zenith_blend_start");
            skyZenithTransition = w.GetDynamicRGBA("sky_zenith_blend_start", "sky_zenith_transition_east_blend", "sky_zenith_transition_west_blend", "sky_zenith_transition_position");
            skyAzimuthEastCol = w.GetDynamicRGBA("sky_azimuth_east_col_r", "sky_azimuth_east_col_g", "sky_azimuth_east_col_b", "sky_azimuth_east_col_inten");
            skyAzimuthWestCol = w.GetDynamicRGBA("sky_azimuth_west_col_r", "sky_azimuth_west_col_g", "sky_azimuth_west_col_b", "sky_azimuth_west_col_inten");
            skyAzimuthTransitionCol = w.GetDynamicRGBA("sky_azimuth_transition_col_r", "sky_azimuth_transition_col_g", "sky_azimuth_transition_col_b", "sky_azimuth_transition_col_inten");
            skyAzimuthTransition = w.GetDynamicValue("sky_azimuth_transition_position");
            skyHdr = w.GetDynamicValue("sky_hdr");
            skyPlane = w.GetDynamicRGBA("sky_plane_r", "sky_plane_g", "sky_plane_b", "sky_plane_inten");
            skySunCol = w.GetDynamicRGB("sky_sun_col_r", "sky_sun_col_g", "sky_sun_col_b");
            skySunDiscCol = w.GetDynamicRGB("sky_sun_disc_col_r", "sky_sun_disc_col_g", "sky_sun_disc_col_b");
            skySunDiscSize = w.GetDynamicValue("sky_sun_disc_size");
            skySunHdr = w.GetDynamicValue("sky_sun_hdr");
            skySunMie = w.GetDynamicRGB("sky_sun_miephase", "sky_sun_miescatter", "sky_sun_mie_intensity_mult");
            skySunInfluenceRadius = w.GetDynamicValue("sky_sun_influence_radius");
            skySunScatterInten = w.GetDynamicValue("sky_sun_scatter_inten");
            skyMoonCol = w.GetDynamicRGB("sky_moon_col_r", "sky_moon_col_g", "sky_moon_col_b");
            skyMoonDiscSize = w.GetDynamicValue("sky_moon_disc_size");
            skyMoonIten = w.GetDynamicValue("sky_moon_iten");
            skyMoonInfluenceRadius = w.GetDynamicValue("sky_moon_influence_radius");
            skyMoonScatterInten = w.GetDynamicValue("sky_moon_scatter_inten");
            skyStarsIten = w.GetDynamicValue("sky_stars_iten");
            lightDirCol = w.GetDynamicRGBA("light_dir_col_r", "light_dir_col_g", "light_dir_col_b", "light_dir_mult");
            lightDirAmbCol = w.GetDynamicRGBA("light_directional_amb_col_r", "light_directional_amb_col_g", "light_directional_amb_col_b", "light_directional_amb_intensity");
            lightDirAmbIntensityMult = w.GetDynamicValue("light_directional_amb_intensity_mult");
            lightDirAmbBounce = w.GetDynamicValue("light_directional_amb_bounce_enabled");
            lightNaturalAmbDown = w.GetDynamicRGBA("light_natural_amb_down_col_r", "light_natural_amb_down_col_g", "light_natural_amb_down_col_b", "light_natural_amb_down_intensity");
            lightNaturalAmbUp = w.GetDynamicRGBA("light_natural_amb_up_col_r", "light_natural_amb_up_col_g", "light_natural_amb_up_col_b", "light_natural_amb_up_intensity");
            lightNaturalAmbUpIntensityMult = w.GetDynamicValue("light_natural_amb_up_intensity_mult");
            lightArtificialIntDown = w.GetDynamicRGBA("light_artificial_int_down_col_r", "light_artificial_int_down_col_g", "light_artificial_int_down_col_b", "light_artificial_int_down_intensity");
            lightArtificialIntUp = w.GetDynamicRGBA("light_artificial_int_up_col_r", "light_artificial_int_up_col_g", "light_artificial_int_up_col_b", "light_artificial_int_up_intensity");
            lightArtificialExtDown = w.GetDynamicRGBA("light_artificial_ext_down_col_r", "light_artificial_ext_down_col_g", "light_artificial_ext_down_col_b", "light_artificial_ext_down_intensity");
            lightArtificialExtUp = w.GetDynamicRGBA("light_artificial_ext_up_col_r", "light_artificial_ext_up_col_g", "light_artificial_ext_up_col_b", "light_artificial_ext_up_intensity");

        }
    }
}
