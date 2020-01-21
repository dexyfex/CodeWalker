using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    public class VehiclesFile : GameFile, PackedFile
    {


        public string ResidentTxd { get; set; }
        public List<VehicleInitData> InitDatas { get; set; }
        public Dictionary<string, string> TxdRelationships { get; set; }




        public VehiclesFile() : base(null, GameFileType.Vehicles)
        {
        }
        public VehiclesFile(RpfFileEntry entry) : base(entry, GameFileType.Vehicles)
        {
        }



        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;


            if (entry.NameLower.EndsWith(".meta"))
            {
                string xml = TextUtil.GetUTF8Text(data);

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);


                ResidentTxd = Xml.GetChildInnerText(xmldoc.SelectSingleNode("CVehicleModelInfo__InitDataList"), "residentTxd");

                LoadInitDatas(xmldoc);

                LoadTxdRelationships(xmldoc);

                Loaded = true;
            }
        }


        private void LoadInitDatas(XmlDocument xmldoc)
        {
            XmlNodeList items = xmldoc.SelectNodes("CVehicleModelInfo__InitDataList/InitDatas/Item | CVehicleModelInfo__InitDataList/InitDatas/item");

            InitDatas = new List<VehicleInitData>();
            for (int i = 0; i < items.Count; i++)
            {
                var node = items[i];
                VehicleInitData d = new VehicleInitData();
                d.Load(node);
                InitDatas.Add(d);
            }
        }

        private void LoadTxdRelationships(XmlDocument xmldoc)
        {
            XmlNodeList items = xmldoc.SelectNodes("CVehicleModelInfo__InitDataList/txdRelationships/Item | CVehicleModelInfo__InitDataList/txdRelationships/item");

            TxdRelationships = new Dictionary<string, string>();
            for (int i = 0; i < items.Count; i++)
            {
                string parentstr = Xml.GetChildInnerText(items[i], "parent");
                string childstr = Xml.GetChildInnerText(items[i], "child");

                if ((!string.IsNullOrEmpty(parentstr)) && (!string.IsNullOrEmpty(childstr)))
                {
                    if (!TxdRelationships.ContainsKey(childstr))
                    {
                        TxdRelationships.Add(childstr, parentstr);
                    }
                    else
                    { }
                }
            }
        }


    }


    public class VehicleInitData
    {
        
        public string modelName { get; set; }                   //<modelName>impaler3</modelName>
        public string txdName { get; set; }                     //<txdName>impaler3</txdName>
        public string handlingId { get; set; }                  //<handlingId>IMPALER3</handlingId>
        public string gameName { get; set; }                    //<gameName>IMPALER3</gameName>
        public string vehicleMakeName { get; set; }             //<vehicleMakeName>DECLASSE</vehicleMakeName>
        public string expressionDictName { get; set; }          //<expressionDictName>null</expressionDictName>
        public string expressionName { get; set; }              //<expressionName>null</expressionName>
        public string animConvRoofDictName { get; set; }        //<animConvRoofDictName>null</animConvRoofDictName>
        public string animConvRoofName { get; set; }            //<animConvRoofName>null</animConvRoofName>
        public string animConvRoofWindowsAffected { get; set; } //<animConvRoofWindowsAffected />
        public string ptfxAssetName { get; set; }               //<ptfxAssetName>weap_xs_vehicle_weapons</ptfxAssetName>
        public string audioNameHash { get; set; }               //<audioNameHash />
        public string layout { get; set; }                      //<layout>LAYOUT_STD_ARENA_1HONLY</layout>
        public string coverBoundOffsets { get; set; }           //<coverBoundOffsets>IMPALER_COVER_OFFSET_INFO</coverBoundOffsets>
        public string explosionInfo { get; set; }               //<explosionInfo>EXPLOSION_INFO_DEFAULT</explosionInfo>
        public string scenarioLayout { get; set; }              //<scenarioLayout />
        public string cameraName { get; set; }                  //<cameraName>FOLLOW_CHEETAH_CAMERA</cameraName>
        public string aimCameraName { get; set; }               //<aimCameraName>DEFAULT_THIRD_PERSON_VEHICLE_AIM_CAMERA</aimCameraName>
        public string bonnetCameraName { get; set; }            //<bonnetCameraName>VEHICLE_BONNET_CAMERA_STANDARD_LONG_DEVIANT</bonnetCameraName>
        public string povCameraName { get; set; }               //<povCameraName>REDUCED_NEAR_CLIP_POV_CAMERA</povCameraName>
        public Vector3 FirstPersonDriveByIKOffset { get; set; }                     //<FirstPersonDriveByIKOffset x="0.020000" y="-0.065000" z="-0.050000" />
        public Vector3 FirstPersonDriveByUnarmedIKOffset { get; set; }              //<FirstPersonDriveByUnarmedIKOffset x="0.000000" y="-0.100000" z="0.000000" />
        public Vector3 FirstPersonProjectileDriveByIKOffset { get; set; }           //<FirstPersonProjectileDriveByIKOffset x="0.000000" y="-0.130000" z="-0.050000" />
        public Vector3 FirstPersonProjectileDriveByPassengerIKOffset { get; set; }  //<FirstPersonProjectileDriveByPassengerIKOffset x="0.000000" y="-0.100000" z="0.000000" />
        public Vector3 FirstPersonDriveByRightPassengerIKOffset { get; set; }       //<FirstPersonDriveByRightPassengerIKOffset x="-0.020000" y="-0.065000" z="-0.050000" />
        public Vector3 FirstPersonDriveByRightPassengerUnarmedIKOffset { get; set; }//<FirstPersonDriveByRightPassengerUnarmedIKOffset x="0.000000" y="-0.100000" z="0.000000" />
        public Vector3 FirstPersonMobilePhoneOffset { get; set; }                   //<FirstPersonMobilePhoneOffset x="0.146000" y="0.220000" z="0.510000" />
        public Vector3 FirstPersonPassengerMobilePhoneOffset { get; set; }          //<FirstPersonPassengerMobilePhoneOffset x="0.234000" y="0.169000" z="0.395000" />
        public Vector3 PovCameraOffset { get; set; }                                //<PovCameraOffset x="0.000000" y="-0.195000" z="0.640000" />
        public Vector3 PovCameraVerticalAdjustmentForRollCage { get; set; }         //<PovCameraVerticalAdjustmentForRollCage value="0.000000" />
        public Vector3 PovPassengerCameraOffset { get; set; }                       //<PovPassengerCameraOffset x="0.000000" y="0.000000" z="0.000000" />
        public Vector3 PovRearPassengerCameraOffset { get; set; }                   //<PovRearPassengerCameraOffset x="0.000000" y="0.000000" z="0.000000" />
        public string vfxInfoName { get; set; }                         //<vfxInfoName>VFXVEHICLEINFO_CAR_GENERIC</vfxInfoName>
        public bool shouldUseCinematicViewMode { get; set; }            //<shouldUseCinematicViewMode value="true" />
        public bool shouldCameraTransitionOnClimbUpDown { get; set; }   //<shouldCameraTransitionOnClimbUpDown value="false" />
        public bool shouldCameraIgnoreExiting { get; set; }             //<shouldCameraIgnoreExiting value="false" />
        public bool AllowPretendOccupants { get; set; }                 //<AllowPretendOccupants value="true" />
        public bool AllowJoyriding { get; set; }                        //<AllowJoyriding value="true" />
        public bool AllowSundayDriving { get; set; }                    //<AllowSundayDriving value="true" />
        public bool AllowBodyColorMapping { get; set; }                 //<AllowBodyColorMapping value="true" />
        public float wheelScale { get; set; }                           //<wheelScale value="0.202300" />
        public float wheelScaleRear { get; set; }                       //<wheelScaleRear value="0.0.201800" />
        public float dirtLevelMin { get; set; }                         //<dirtLevelMin value="0.000000" />
        public float dirtLevelMax { get; set; }                         //<dirtLevelMax value="0.450000" />
        public float envEffScaleMin { get; set; }                       //<envEffScaleMin value="0.000000" />
        public float envEffScaleMax { get; set; }                       //<envEffScaleMax value="1.000000" />
        public float envEffScaleMin2 { get; set; }                      //<envEffScaleMin2 value="0.000000" />
        public float envEffScaleMax2 { get; set; }                      //<envEffScaleMax2 value="1.000000" />
        public float damageMapScale { get; set; }                       //<damageMapScale value="0.000000" />
        public float damageOffsetScale { get; set; }                    //<damageOffsetScale value="0.100000" />
        public Color4 diffuseTint { get; set; }                         //<diffuseTint value="0x00FFFFFF" />
        public float steerWheelMult { get; set; }                       //<steerWheelMult value="0.700000" />
        public float HDTextureDist { get; set; }                        //<HDTextureDist value="5.000000" />
        public float[] lodDistances { get; set; }                       //<lodDistances content="float_array">//  10.000000//  25.000000//  60.000000//  120.000000//  500.000000//  500.000000//</lodDistances>
        public float minSeatHeight { get; set; }                        //<minSeatHeight value="0.844" />
        public float identicalModelSpawnDistance { get; set; }          //<identicalModelSpawnDistance value="20" />
        public int maxNumOfSameColor { get; set; }                      //<maxNumOfSameColor value="1" />
        public float defaultBodyHealth { get; set; }                    //<defaultBodyHealth value="1000.000000" />
        public float pretendOccupantsScale { get; set; }                //<pretendOccupantsScale value="1.000000" />
        public float visibleSpawnDistScale { get; set; }                //<visibleSpawnDistScale value="1.000000" />
        public float trackerPathWidth { get; set; }                     //<trackerPathWidth value="2.000000" />
        public float weaponForceMult { get; set; }                      //<weaponForceMult value="1.000000" />
        public float frequency { get; set; }                            //<frequency value="30" />
        public string swankness { get; set; }                           //<swankness>SWANKNESS_4</swankness>
        public int maxNum { get; set; }                                 //<maxNum value="10" />
        public string[] flags { get; set; }                             //<flags>FLAG_RECESSED_HEADLIGHT_CORONAS FLAG_EXTRAS_STRONG FLAG_AVERAGE_CAR FLAG_HAS_INTERIOR_EXTRAS FLAG_CAN_HAVE_NEONS FLAG_HAS_JUMP_MOD FLAG_HAS_NITROUS_MOD FLAG_HAS_RAMMING_SCOOP_MOD FLAG_USE_AIRCRAFT_STYLE_WEAPON_TARGETING FLAG_HAS_SIDE_SHUNT FLAG_HAS_WEAPON_SPIKE_MODS FLAG_HAS_SUPERCHARGER FLAG_INCREASE_CAMBER_WITH_SUSPENSION_MOD FLAG_DISABLE_DEFORMATION</flags>
        public string type { get; set; }                                //<type>VEHICLE_TYPE_CAR</type>
        public string plateType { get; set; }                           //<plateType>VPT_FRONT_AND_BACK_PLATES</plateType>
        public string dashboardType { get; set; }                       //<dashboardType>VDT_DUKES</dashboardType>
        public string vehicleClass { get; set; }                        //<vehicleClass>VC_MUSCLE</vehicleClass>
        public string wheelType { get; set; }                           //<wheelType>VWT_MUSCLE</wheelType>
        public string[] trailers { get; set; }                          //<trailers />
        public string[] additionalTrailers { get; set; }                //<additionalTrailers />
        public VehicleDriver[] drivers { get; set; }                    //<drivers />
        public string[] extraIncludes { get; set; }                     //<extraIncludes />
        public string[] doorsWithCollisionWhenClosed { get; set; }      //<doorsWithCollisionWhenClosed />
        public string[] driveableDoors { get; set; }                    //<driveableDoors />
        public bool bumpersNeedToCollideWithMap { get; set; }           //<bumpersNeedToCollideWithMap value="false" />
        public bool needsRopeTexture { get; set; }                      //<needsRopeTexture value="false" />
        public string[] requiredExtras { get; set; }                    //<requiredExtras>EXTRA_1 EXTRA_2 EXTRA_3</requiredExtras>
        public string[] rewards { get; set; }                           //<rewards />
        public string[] cinematicPartCamera { get; set; }               //<cinematicPartCamera>//  <Item>WHEEL_FRONT_RIGHT_CAMERA</Item>//  <Item>WHEEL_FRONT_LEFT_CAMERA</Item>//  <Item>WHEEL_REAR_RIGHT_CAMERA</Item>//  <Item>WHEEL_REAR_LEFT_CAMERA</Item>//</cinematicPartCamera>
        public string NmBraceOverrideSet { get; set; }                  //<NmBraceOverrideSet />
        public Vector3 buoyancySphereOffset { get; set; }               //<buoyancySphereOffset x="0.000000" y="0.000000" z="0.000000" />
        public float buoyancySphereSizeScale { get; set; }              //<buoyancySphereSizeScale value="1.000000" />
        public VehicleOverrideRagdollThreshold pOverrideRagdollThreshold { get; set; }  //<pOverrideRagdollThreshold type="NULL" />
        public string[] firstPersonDrivebyData { get; set; }            //<firstPersonDrivebyData>//  <Item>STD_IMPALER2_FRONT_LEFT</Item>//  <Item>STD_IMPALER2_FRONT_RIGHT</Item>//</firstPersonDrivebyData>


        public void Load(XmlNode node)
        {
            modelName = Xml.GetChildInnerText(node, "modelName");
            txdName = Xml.GetChildInnerText(node, "txdName");
            handlingId = Xml.GetChildInnerText(node, "handlingId");
            gameName = Xml.GetChildInnerText(node, "gameName");
            vehicleMakeName = Xml.GetChildInnerText(node, "vehicleMakeName");
            expressionDictName = Xml.GetChildInnerText(node, "expressionDictName");
            expressionName = Xml.GetChildInnerText(node, "expressionName");
            animConvRoofDictName = Xml.GetChildInnerText(node, "animConvRoofDictName");
            animConvRoofName = Xml.GetChildInnerText(node, "animConvRoofName");
            animConvRoofWindowsAffected = Xml.GetChildInnerText(node, "animConvRoofWindowsAffected");//?
            ptfxAssetName = Xml.GetChildInnerText(node, "ptfxAssetName");
            audioNameHash = Xml.GetChildInnerText(node, "audioNameHash");
            layout = Xml.GetChildInnerText(node, "layout");
            coverBoundOffsets = Xml.GetChildInnerText(node, "coverBoundOffsets");
            explosionInfo = Xml.GetChildInnerText(node, "explosionInfo");
            scenarioLayout = Xml.GetChildInnerText(node, "scenarioLayout");
            cameraName = Xml.GetChildInnerText(node, "cameraName");
            aimCameraName = Xml.GetChildInnerText(node, "aimCameraName");
            bonnetCameraName = Xml.GetChildInnerText(node, "bonnetCameraName");
            povCameraName = Xml.GetChildInnerText(node, "povCameraName");
            FirstPersonDriveByIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonDriveByIKOffset");
            FirstPersonDriveByUnarmedIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonDriveByUnarmedIKOffset");
            FirstPersonProjectileDriveByIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonProjectileDriveByIKOffset");
            FirstPersonProjectileDriveByPassengerIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonProjectileDriveByPassengerIKOffset");
            FirstPersonDriveByRightPassengerIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonDriveByRightPassengerIKOffset");
            FirstPersonDriveByRightPassengerUnarmedIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonDriveByRightPassengerUnarmedIKOffset");
            FirstPersonMobilePhoneOffset = Xml.GetChildVector3Attributes(node, "FirstPersonMobilePhoneOffset");
            FirstPersonPassengerMobilePhoneOffset = Xml.GetChildVector3Attributes(node, "FirstPersonPassengerMobilePhoneOffset");
            PovCameraOffset = Xml.GetChildVector3Attributes(node, "PovCameraOffset");
            PovCameraVerticalAdjustmentForRollCage = Xml.GetChildVector3Attributes(node, "PovCameraVerticalAdjustmentForRollCage");
            PovPassengerCameraOffset = Xml.GetChildVector3Attributes(node, "PovPassengerCameraOffset");
            PovRearPassengerCameraOffset = Xml.GetChildVector3Attributes(node, "PovRearPassengerCameraOffset");
            vfxInfoName = Xml.GetChildInnerText(node, "vfxInfoName");
            shouldUseCinematicViewMode = Xml.GetChildBoolAttribute(node, "shouldUseCinematicViewMode", "value");
            shouldCameraTransitionOnClimbUpDown = Xml.GetChildBoolAttribute(node, "shouldCameraTransitionOnClimbUpDown", "value");
            shouldCameraIgnoreExiting = Xml.GetChildBoolAttribute(node, "shouldCameraIgnoreExiting", "value");
            AllowPretendOccupants = Xml.GetChildBoolAttribute(node, "AllowPretendOccupants", "value");
            AllowJoyriding = Xml.GetChildBoolAttribute(node, "AllowJoyriding", "value");
            AllowSundayDriving = Xml.GetChildBoolAttribute(node, "AllowSundayDriving", "value");
            AllowBodyColorMapping = Xml.GetChildBoolAttribute(node, "AllowBodyColorMapping", "value");
            wheelScale = Xml.GetChildFloatAttribute(node, "wheelScale", "value");
            wheelScaleRear = Xml.GetChildFloatAttribute(node, "wheelScaleRear", "value");
            dirtLevelMin = Xml.GetChildFloatAttribute(node, "dirtLevelMin", "value");
            dirtLevelMax = Xml.GetChildFloatAttribute(node, "dirtLevelMax", "value");
            envEffScaleMin = Xml.GetChildFloatAttribute(node, "envEffScaleMin", "value");
            envEffScaleMax = Xml.GetChildFloatAttribute(node, "envEffScaleMax", "value");
            envEffScaleMin2 = Xml.GetChildFloatAttribute(node, "envEffScaleMin2", "value");
            envEffScaleMax2 = Xml.GetChildFloatAttribute(node, "envEffScaleMax2", "value");
            damageMapScale = Xml.GetChildFloatAttribute(node, "damageMapScale", "value");
            damageOffsetScale = Xml.GetChildFloatAttribute(node, "damageOffsetScale", "value");
            diffuseTint = new Color4(Convert.ToUInt32(Xml.GetChildStringAttribute(node, "diffuseTint", "value").Replace("0x", ""), 16));
            steerWheelMult = Xml.GetChildFloatAttribute(node, "steerWheelMult", "value");
            HDTextureDist = Xml.GetChildFloatAttribute(node, "HDTextureDist", "value");
            lodDistances = GetFloatArray(node, "lodDistances", '\n');
            minSeatHeight = Xml.GetChildFloatAttribute(node, "minSeatHeight", "value");
            identicalModelSpawnDistance = Xml.GetChildFloatAttribute(node, "identicalModelSpawnDistance", "value");
            maxNumOfSameColor = Xml.GetChildIntAttribute(node, "maxNumOfSameColor", "value");
            defaultBodyHealth = Xml.GetChildFloatAttribute(node, "defaultBodyHealth", "value");
            pretendOccupantsScale = Xml.GetChildFloatAttribute(node, "pretendOccupantsScale", "value");
            visibleSpawnDistScale = Xml.GetChildFloatAttribute(node, "visibleSpawnDistScale", "value");
            trackerPathWidth = Xml.GetChildFloatAttribute(node, "trackerPathWidth", "value");
            weaponForceMult = Xml.GetChildFloatAttribute(node, "weaponForceMult", "value");
            frequency = Xml.GetChildFloatAttribute(node, "frequency", "value");
            swankness = Xml.GetChildInnerText(node, "swankness");
            maxNum = Xml.GetChildIntAttribute(node, "maxNum", "value");
            flags = GetStringArray(node, "flags", ' ');
            type = Xml.GetChildInnerText(node, "type");
            plateType = Xml.GetChildInnerText(node, "plateType");
            dashboardType = Xml.GetChildInnerText(node, "dashboardType");
            vehicleClass = Xml.GetChildInnerText(node, "vehicleClass");
            wheelType = Xml.GetChildInnerText(node, "wheelType");
            trailers = GetStringItemArray(node, "trailers");
            additionalTrailers = GetStringItemArray(node, "additionalTrailers");
            var dnode = node.SelectSingleNode("drivers");
            if (dnode != null)
            {
                var items = dnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    drivers = new VehicleDriver[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        var driver = new VehicleDriver();
                        driver.driverName = Xml.GetChildInnerText(item, "driverName");
                        driver.npcName = Xml.GetChildInnerText(item, "npcName");
                        drivers[i] = driver;
                    }
                }
            }
            extraIncludes = GetStringItemArray(node, "extraIncludes");
            doorsWithCollisionWhenClosed = GetStringItemArray(node, "doorsWithCollisionWhenClosed");
            driveableDoors = GetStringItemArray(node, "driveableDoors");
            bumpersNeedToCollideWithMap = Xml.GetChildBoolAttribute(node, "bumpersNeedToCollideWithMap", "value");
            needsRopeTexture = Xml.GetChildBoolAttribute(node, "needsRopeTexture", "value");
            requiredExtras = GetStringArray(node, "requiredExtras", ' ');
            rewards = GetStringItemArray(node, "rewards");
            cinematicPartCamera = GetStringItemArray(node, "cinematicPartCamera");
            NmBraceOverrideSet = Xml.GetChildInnerText(node, "NmBraceOverrideSet");
            buoyancySphereOffset = Xml.GetChildVector3Attributes(node, "buoyancySphereOffset");
            buoyancySphereSizeScale = Xml.GetChildFloatAttribute(node, "buoyancySphereSizeScale", "value");
            var tnode = node.SelectSingleNode("pOverrideRagdollThreshold");
            if (tnode != null)
            {
                var ttype = tnode.Attributes["type"]?.Value;
                switch (ttype)
                {
                    case "NULL": break;
                    case "CVehicleModelInfo__CVehicleOverrideRagdollThreshold":
                        pOverrideRagdollThreshold = new VehicleOverrideRagdollThreshold();
                        pOverrideRagdollThreshold.MinComponent = Xml.GetChildIntAttribute(tnode, "MinComponent", "value");
                        pOverrideRagdollThreshold.MaxComponent = Xml.GetChildIntAttribute(tnode, "MaxComponent", "value");
                        pOverrideRagdollThreshold.ThresholdMult = Xml.GetChildFloatAttribute(tnode, "ThresholdMult", "value");
                        break;
                    default:
                        break;
                }
            }
            firstPersonDrivebyData = GetStringItemArray(node, "firstPersonDrivebyData");
        }

        private string[] GetStringItemArray(XmlNode node, string childName)
        {
            var cnode = node.SelectSingleNode(childName);
            if (cnode == null) return null;
            var items = cnode.SelectNodes("Item");
            if (items == null) return null;
            getStringArrayList.Clear();
            foreach (XmlNode inode in items)
            {
                var istr = inode.InnerText;
                if (!string.IsNullOrEmpty(istr))
                {
                    getStringArrayList.Add(istr);
                }
            }
            if (getStringArrayList.Count == 0) return null;
            return getStringArrayList.ToArray();
        }
        private string[] GetStringArray(XmlNode node, string childName, char delimiter)
        {
            var ldastr = Xml.GetChildInnerText(node, childName);
            var ldarr = ldastr?.Split(delimiter);
            if (ldarr == null) return null;
            getStringArrayList.Clear();
            foreach (var ldstr in ldarr)
            {
                var ldt = ldstr?.Trim();
                if (!string.IsNullOrEmpty(ldt))
                {
                    getStringArrayList.Add(ldt);
                }
            }
            if (getStringArrayList.Count == 0) return null;
            return getStringArrayList.ToArray();
        }
        private float[] GetFloatArray(XmlNode node, string childName, char delimiter)
        {
            var ldastr = Xml.GetChildInnerText(node, childName);
            var ldarr = ldastr?.Split(delimiter);
            if (ldarr == null) return null;
            getFloatArrayList.Clear();
            foreach (var ldstr in ldarr)
            {
                var ldt = ldstr?.Trim();
                if (!string.IsNullOrEmpty(ldt))
                {
                    float f;
                    if (FloatUtil.TryParse(ldt, out f))
                    {
                        getFloatArrayList.Add(f);
                    }
                }
            }
            if (getFloatArrayList.Count == 0) return null;
            return getFloatArrayList.ToArray();
        }

        private static List<string> getStringArrayList = new List<string>(); //kinda hacky..
        private static List<float> getFloatArrayList = new List<float>(); //kinda hacky..


        public override string ToString()
        {
            return modelName;
        }
    }

    public class VehicleOverrideRagdollThreshold
    {
        public int MinComponent { get; set; }
        public int MaxComponent { get; set; }
        public float ThresholdMult { get; set; }

        public override string ToString()
        {
            return MinComponent.ToString() + ", " + MaxComponent.ToString() + ", " + ThresholdMult.ToString();
        }
    }
    public class VehicleDriver
    {
        public string driverName { get; set; }
        public string npcName { get; set; }

        public override string ToString()
        {
            return driverName + ", " + npcName;
        }
    }

}
