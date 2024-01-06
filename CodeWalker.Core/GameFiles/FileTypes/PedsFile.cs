using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.IO;
using System.Xml;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Xml.Linq;
using Collections.Pooled;


namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class PedsFile : GameFile, PackedFile
    {
        private static XmlNameTable cachedNameTable = new XmlNameTableThreadSafe();
        public PsoFile Pso { get; set; }
        public string Xml { get; set; }

        public CPedModelInfo__InitDataList InitDataList { get; set; }

        public PedsFile() : base(null, GameFileType.Peds)
        { }
        public PedsFile(RpfFileEntry entry) : base(entry, GameFileType.Peds)
        { }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;



            //can be PSO .ymt or XML .meta
            //using MemoryStream ms = new MemoryStream(data);
            if (PsoFile.IsPSO(data.AsSpan(0, 4)))
            {
                Pso = new PsoFile();
                Pso.Load(data);
                Xml = PsoXml.GetXml(Pso); //yep let's just convert that to XML :P
            }
            else
            {
                Xml = TextUtil.GetUTF8Text(data);
            }

            using var textReader = new StringReader(Xml);

            //XmlDocument xdoc = new XmlDocument();
            //if (!string.IsNullOrEmpty(Xml))
            //{
            //    try
            //    {
            //        xdoc.LoadXml(Xml);
            //    }
            //    catch (Exception ex)
            //    {
            //        var msg = ex.Message;
            //    }
            //}

            using var xmlReader = XmlReader.Create(textReader, new XmlReaderSettings { NameTable = cachedNameTable });


            //if (xdoc.DocumentElement != null)
            //{
                InitDataList = new CPedModelInfo__InitDataList(xmlReader);
            //}




            Loaded = true;
        }
    }



    [TC(typeof(EXP))] public class CPedModelInfo__InitDataList
    {
        public string residentTxd { get; set; }
        public string[] residentAnims { get; set; }
        public CPedModelInfo__InitData[] InitDatas { get; set; }
        public CTxdRelationship[] txdRelationships { get; set; }
        public CMultiTxdRelationship[] multiTxdRelationships { get; set; }

        public CPedModelInfo__InitDataList(XmlReader reader)
        {
            while (reader.Read())
            {
                reader.MoveToContent();

                //var _ = xmlReader.Name switch
                //{
                //    "residentTxd" => ResidentTxd = Xml.GetChildInnerText(xmlReader, "residentTxd"),
                //    "InitDatas" => LoadInitDatas(xmlReader),
                //    "txdRelationships" => LoadTxdRelationships(xmlReader),
                //    _ => throw new Exception()
                //};

                switch (reader.Name)
                {
                    case string Name when Name.Equals("residentTxd", StringComparison.OrdinalIgnoreCase):
                        residentTxd = Xml.GetChildInnerText(reader, "residentTxd");
                        break;
                    case string Name when Name.Equals("InitDatas", StringComparison.OrdinalIgnoreCase):
                        if (reader.IsEmptyElement)
                        {
                            reader.ReadStartElement();
                            break;
                        }
                        reader.ReadStartElement();
                        using (var initDatasList = new PooledList<CPedModelInfo__InitData>())
                        {
                            while (reader.IsItemElement())
                            {
                                initDatasList.Add(new CPedModelInfo__InitData(reader));
                            }
                            if (initDatasList.Count > 0)
                            {
                                InitDatas = initDatasList.ToArray();
                            }
                        }

                        reader.ReadEndElement();
                        break;
                    case string Name when Name.Equals("txdRelationships", StringComparison.OrdinalIgnoreCase):
                        if (reader.IsEmptyElement)
                        {
                            reader.ReadStartElement();
                            break;
                        }
                        reader.ReadStartElement();
                        using (var txdRelationshipsList = new PooledList<CTxdRelationship>())
                        {
                            while (reader.IsItemElement())
                            {
                                txdRelationshipsList.Add(new CTxdRelationship(reader));
                            }
                            reader.ReadEndElement();
                            if (txdRelationshipsList.Count > 0)
                            {
                                txdRelationships = txdRelationshipsList.ToArray();
                            }
                        }

                        break;
                    case string Name when Name.Equals("multiTxdRelationships", StringComparison.OrdinalIgnoreCase):
                        if (reader.IsEmptyElement)
                        {
                            reader.ReadStartElement();
                            break;
                        }
                        reader.ReadStartElement();
                        using (var multiTxdList = new PooledList<CMultiTxdRelationship>())
                        {
                            while (reader.IsItemElement())
                            {
                                multiTxdList.Add(new CMultiTxdRelationship(reader));
                            }
                            reader.ReadEndElement();
                            if (multiTxdList.Count > 0)
                            {
                                multiTxdRelationships = multiTxdList.ToArray();
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        public CPedModelInfo__InitDataList(XmlNode node)
        {
            XmlNodeList items;

            residentTxd = Xml.GetChildInnerText(node, "residentTxd");

            items = node.SelectSingleNode("residentAnims")?.SelectNodes("Item");
            if (items?.Count > 0)
            {
                residentAnims = new string[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    residentAnims[i] = items[i].InnerText;
                }
            }
            items = node.SelectSingleNode("InitDatas")?.SelectNodes("Item");
            if (items?.Count > 0)
            {
                InitDatas = new CPedModelInfo__InitData[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    InitDatas[i] = new CPedModelInfo__InitData(items[i]);
                }
            }
            items = node.SelectSingleNode("txdRelationships")?.SelectNodes("Item");
            if (items?.Count > 0)
            {
                txdRelationships = new CTxdRelationship[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    txdRelationships[i] = new CTxdRelationship(items[i]);
                }
            }
            items = node.SelectSingleNode("multiTxdRelationships")?.SelectNodes("Item");
            if (items?.Count > 0)
            {
                multiTxdRelationships = new CMultiTxdRelationship[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    multiTxdRelationships[i] = new CMultiTxdRelationship(items[i]);
                }
            }
        }
    }

    [TC(typeof(EXP))] public class CPedModelInfo__InitData
    {
        public string Name { get; set; }
        public string PropsName { get; set; }
        public string ClipDictionaryName { get; set; }
        public string BlendShapeFileName { get; set; }
        public string ExpressionSetName { get; set; }
        public string ExpressionDictionaryName { get; set; }
        public string ExpressionName { get; set; }
        public string Pedtype { get; set; }
        public string MovementClipSet { get; set; }
        public string[] MovementClipSets { get; set; }
        public string StrafeClipSet { get; set; }
        public string MovementToStrafeClipSet { get; set; }
        public string InjuredStrafeClipSet { get; set; }
        public string FullBodyDamageClipSet { get; set; }
        public string AdditiveDamageClipSet { get; set; }
        public string DefaultGestureClipSet { get; set; }
        public string FacialClipsetGroupName { get; set; }
        public string DefaultVisemeClipSet { get; set; }
        public string SidestepClipSet { get; set; }
        public string PoseMatcherName { get; set; }
        public string PoseMatcherProneName { get; set; }
        public string GetupSetHash { get; set; }
        public string CreatureMetadataName { get; set; }
        public string DecisionMakerName { get; set; }
        public string MotionTaskDataSetName { get; set; }
        public string DefaultTaskDataSetName { get; set; }
        public string PedCapsuleName { get; set; }
        public string PedLayoutName { get; set; }
        public string PedComponentSetName { get; set; }
        public string PedComponentClothName { get; set; }
        public string PedIKSettingsName { get; set; }
        public string TaskDataName { get; set; }
        public bool IsStreamedGfx { get; set; }
        public bool AmbulanceShouldRespondTo { get; set; }
        public bool CanRideBikeWithNoHelmet { get; set; }
        public bool CanSpawnInCar { get; set; }
        public bool IsHeadBlendPed { get; set; }
        public bool bOnlyBulkyItemVariations { get; set; }
        public string RelationshipGroup { get; set; }
        public string NavCapabilitiesName { get; set; }
        public string PerceptionInfo { get; set; }
        public string DefaultBrawlingStyle { get; set; }
        public string DefaultUnarmedWeapon { get; set; }
        public string Personality { get; set; }
        public string CombatInfo { get; set; }
        public string VfxInfoName { get; set; }
        public string AmbientClipsForFlee { get; set; }
        public string Radio1 { get; set; } // MetaName.ePedRadioGenre
        public string Radio2 { get; set; } // MetaName.ePedRadioGenre
        public float FUpOffset { get; set; }
        public float RUpOffset { get; set; }
        public float FFrontOffset { get; set; }
        public float RFrontOffset { get; set; }
        public float MinActivationImpulse { get; set; }
        public float Stubble { get; set; }
        public float HDDist { get; set; }
        public float TargetingThreatModifier { get; set; }
        public float KilledPerceptionRangeModifer { get; set; }
        public string Sexiness { get; set; } // MetaTypeName.ARRAYINFO MetaName.eSexinessFlags
        public byte Age { get; set; }
        public byte MaxPassengersInCar { get; set; }
        public string ExternallyDrivenDOFs { get; set; } // MetaTypeName.ARRAYINFO MetaName.eExternallyDrivenDOFs
        public string PedVoiceGroup { get; set; }
        public string AnimalAudioObject { get; set; }
        public string AbilityType { get; set; } // MetaName.SpecialAbilityType
        public string ThermalBehaviour { get; set; } // MetaName.ThermalBehaviour
        public string SuperlodType { get; set; } // MetaName.eSuperlodType
        public string ScenarioPopStreamingSlot { get; set; } // MetaName.eScenarioPopStreamingSlot
        public string DefaultSpawningPreference { get; set; } // MetaName.DefaultSpawnPreference
        public float DefaultRemoveRangeMultiplier { get; set; }
        public bool AllowCloseSpawning { get; set; }

        public CPedModelInfo__InitData(XmlReader reader)
        {
            reader.ReadStartElement("Item");
            while (reader.Name != "Item" && reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Item")
                {
                    reader.ReadEndElement();
                    return;
                }

                while (reader.MoveToContent() != XmlNodeType.Element && reader.Read())
                {

                }

                switch (reader.Name)
                {
                    case "Name":
                        Name = Xml.GetChildInnerText(reader, "Name");
                        break;
                    case "PropsName":
                        PropsName = Xml.GetChildInnerText(reader, "PropsName");
                        break;
                    case "ClipDictionaryName":
                        ClipDictionaryName = Xml.GetChildInnerText(reader, "ClipDictionaryName");
                        break;
                    case "BlendShapeFileName":
                        BlendShapeFileName = Xml.GetChildInnerText(reader, "BlendShapeFileName");
                        break;
                    case "ExpressionSetName":
                        ExpressionSetName = Xml.GetChildInnerText(reader, "ExpressionSetName");
                        break;
                    case "ExpressionDictionaryName":
                        ExpressionDictionaryName = Xml.GetChildInnerText(reader, "ExpressionDictionaryName");
                        break;
                    case "ExpressionName":
                        ExpressionName = Xml.GetChildInnerText(reader, "ExpressionName");
                        break;
                    case "Pedtype":
                        Pedtype = Xml.GetChildInnerText(reader, "Pedtype");
                        break;
                    case "MovementClipSet":
                        MovementClipSet = Xml.GetChildInnerText(reader, "MovementClipSet");
                        break;
                    case "MovementClipSets":
                        using (var clipSetsList = new PooledList<string>())
                        {
                            foreach (var item in Xml.IterateItems(reader, "MovementClipSets"))
                            {
                                clipSetsList.Add(item.Value);
                            }

                            MovementClipSets = clipSetsList.ToArray();
                        }

                        break;
                    case "StrafeClipSet":
                        StrafeClipSet = Xml.GetChildInnerText(reader, "StrafeClipSet");
                        break;
                    case "MovementToStrafeClipSet":
                        MovementToStrafeClipSet = Xml.GetChildInnerText(reader, "MovementToStrafeClipSet");
                        break;
                    case "InjuredStrafeClipSet":
                        InjuredStrafeClipSet = Xml.GetChildInnerText(reader, "InjuredStrafeClipSet");
                        break;
                    case "FullBodyDamageClipSet":
                        FullBodyDamageClipSet = Xml.GetChildInnerText(reader, "FullBodyDamageClipSet");
                        break;
                    case "AdditiveDamageClipSet":
                        AdditiveDamageClipSet = Xml.GetChildInnerText(reader, "AdditiveDamageClipSet");
                        break;
                    case "DefaultGestureClipSet":
                        DefaultGestureClipSet = Xml.GetChildInnerText(reader, "DefaultGestureClipSet");
                        break;
                    case "FacialClipsetGroupName":
                        FacialClipsetGroupName = Xml.GetChildInnerText(reader, "FacialClipsetGroupName");
                        break;
                    case "DefaultVisemeClipSet":
                        DefaultVisemeClipSet = Xml.GetChildInnerText(reader, "DefaultVisemeClipSet");
                        break;
                    case "SidestepClipSet":
                        SidestepClipSet = Xml.GetChildInnerText(reader, "SidestepClipSet");
                        break;
                    case "PoseMatcherName":
                        PoseMatcherName = Xml.GetChildInnerText(reader, "PoseMatcherName");
                        break;
                    case "PoseMatcherProneName":
                        PoseMatcherProneName = Xml.GetChildInnerText(reader, "PoseMatcherProneName");
                        break;
                    case "GetupSetHash":
                        GetupSetHash = Xml.GetChildInnerText(reader, "GetupSetHash");
                        break;
                    case "CreatureMetadataName":
                        CreatureMetadataName = Xml.GetChildInnerText(reader, "CreatureMetadataName");
                        break;
                    case "DecisionMakerName":
                        DecisionMakerName = Xml.GetChildInnerText(reader, "DecisionMakerName");
                        break;
                    case "MotionTaskDataSetName":
                        MotionTaskDataSetName = Xml.GetChildInnerText(reader, "MotionTaskDataSetName");
                        break;
                    case "DefaultTaskDataSetName":
                        DefaultTaskDataSetName = Xml.GetChildInnerText(reader, "DefaultTaskDataSetName");
                        break;
                    case "PedCapsuleName":
                        PedCapsuleName = Xml.GetChildInnerText(reader, "PedCapsuleName");
                        break;
                    case "PedLayoutName":
                        PedLayoutName = Xml.GetChildInnerText(reader, "PedLayoutName");
                        break;
                    case "PedComponentSetName":
                        PedComponentSetName = Xml.GetChildInnerText(reader, "PedComponentSetName");
                        break;
                    case "PedComponentClothName":
                        PedComponentClothName = Xml.GetChildInnerText(reader, "PedComponentClothName");
                        break;
                    case "PedIKSettingsName":
                        PedIKSettingsName = Xml.GetChildInnerText(reader, "PedIKSettingsName");
                        break;
                    case "TaskDataName":
                        TaskDataName = Xml.GetChildInnerText(reader, "TaskDataName");
                        break;
                    case "IsStreamedGfx":
                        IsStreamedGfx = Xml.GetChildBoolAttribute(reader, "IsStreamedGfx", "value");
                        break;
                    case "AmbulanceShouldRespondTo":
                        AmbulanceShouldRespondTo = Xml.GetChildBoolAttribute(reader, "AmbulanceShouldRespondTo", "value");
                        break;
                    case "CanRideBikeWithNoHelmet":
                        CanRideBikeWithNoHelmet = Xml.GetChildBoolAttribute(reader, "CanRideBikeWithNoHelmet", "value");
                        break;
                    case "CanSpawnInCar":
                        CanSpawnInCar = Xml.GetChildBoolAttribute(reader, "CanSpawnInCar", "value");
                        break;
                    case "IsHeadBlendPed":
                        IsHeadBlendPed = Xml.GetChildBoolAttribute(reader, "IsHeadBlendPed", "value");
                        break;
                    case "bOnlyBulkyItemVariations":
                        bOnlyBulkyItemVariations = Xml.GetChildBoolAttribute(reader, "bOnlyBulkyItemVariations", "value");
                        break;
                    case "RelationshipGroup":
                        RelationshipGroup = Xml.GetChildInnerText(reader, "RelationshipGroup");
                        break;
                    case "NavCapabilitiesName":
                        NavCapabilitiesName = Xml.GetChildInnerText(reader, "NavCapabilitiesName");
                        break;
                    case "PerceptionInfo":
                        PerceptionInfo = Xml.GetChildInnerText(reader, "PerceptionInfo");
                        break;
                    case "DefaultBrawlingStyle":
                        DefaultBrawlingStyle = Xml.GetChildInnerText(reader, "DefaultBrawlingStyle");
                        break;
                    case "DefaultUnarmedWeapon":
                        DefaultUnarmedWeapon = Xml.GetChildInnerText(reader, "DefaultUnarmedWeapon");
                        break;
                    case "Personality":
                        Personality = Xml.GetChildInnerText(reader, "Personality");
                        break;
                    case "CombatInfo":
                        CombatInfo = Xml.GetChildInnerText(reader, "CombatInfo");
                        break;
                    case "VfxInfoName":
                        VfxInfoName = Xml.GetChildInnerText(reader, "VfxInfoName");
                        break;
                    case "AmbientClipsForFlee":
                        AmbientClipsForFlee = Xml.GetChildInnerText(reader, "AmbientClipsForFlee");
                        break;
                    case "Radio1":
                        Radio1 = Xml.GetChildInnerText(reader, "Radio1"); // MetaName.ePedRadioGenre
                        break;
                    case "Radio2":
                        Radio2 = Xml.GetChildInnerText(reader, "Radio2"); // MetaName.ePedRadioGenre
                        break;
                    case "FUpOffset":
                        FUpOffset = Xml.GetChildFloatAttribute(reader, "FUpOffset", "value");
                        break;
                    case "RUpOffset":
                        RUpOffset = Xml.GetChildFloatAttribute(reader, "RUpOffset", "value");
                        break;
                    case "FFrontOffset":
                        FFrontOffset = Xml.GetChildFloatAttribute(reader, "FFrontOffset", "value");
                        break;
                    case "RFrontOffset":
                        RFrontOffset = Xml.GetChildFloatAttribute(reader, "RFrontOffset", "value");
                        break;
                    case "MinActivationImpulse":
                        MinActivationImpulse = Xml.GetChildFloatAttribute(reader, "MinActivationImpulse", "value");
                        break;
                    case "Stubble":
                        Stubble = Xml.GetChildFloatAttribute(reader, "Stubble", "value");
                        break;
                    case "HDDist":
                        HDDist = Xml.GetChildFloatAttribute(reader, "HDDist", "value");
                        break;
                    case "TargetingThreatModifier":
                        TargetingThreatModifier = Xml.GetChildFloatAttribute(reader, "TargetingThreatModifier", "value");
                        break;
                    case "KilledPerceptionRangeModifer":
                        KilledPerceptionRangeModifer = Xml.GetChildFloatAttribute(reader, "KilledPerceptionRangeModifer", "value");
                        break;
                    case "Sexiness":
                        Sexiness = Xml.GetChildInnerText(reader, "Sexiness"); // MetaTypeName.ARRAYINFO MetaName.eSexinessFlags
                        break;
                    case "Age":
                        Age = (byte)Xml.GetChildUIntAttribute(reader, "Age", "value");
                        break;
                    case "MaxPassengersInCar":
                        MaxPassengersInCar = (byte)Xml.GetChildUIntAttribute(reader, "MaxPassengersInCar", "value");
                        break;
                    case "ExternallyDrivenDOFs":
                        ExternallyDrivenDOFs = Xml.GetChildInnerText(reader, "ExternallyDrivenDOFs"); // MetaTypeName.ARRAYINFO MetaName.eExternallyDrivenDOFs
                        break;
                    case "PedVoiceGroup":
                        PedVoiceGroup = Xml.GetChildInnerText(reader, "PedVoiceGroup");
                        break;
                    case "AnimalAudioObject":
                        AnimalAudioObject = Xml.GetChildInnerText(reader, "AnimalAudioObject");
                        break;
                    case "AbilityType":
                        AbilityType = Xml.GetChildInnerText(reader, "AbilityType"); // MetaName.SpecialAbilityType
                        break;
                    case "ThermalBehaviour":
                        ThermalBehaviour = Xml.GetChildInnerText(reader, "ThermalBehaviour"); // MetaName.ThermalBehaviour
                        break;
                    case "SuperlodType":
                        SuperlodType = Xml.GetChildInnerText(reader, "SuperlodType"); // MetaName.eSuperlodType
                        break;
                    case "ScenarioPopStreamingSlot":
                        ScenarioPopStreamingSlot = Xml.GetChildInnerText(reader, "ScenarioPopStreamingSlot"); // MetaName.eScenarioPopStreamingSlot
                        break;
                    case "DefaultSpawningPreference":
                        DefaultSpawningPreference = Xml.GetChildInnerText(reader, "DefaultSpawningPreference"); // MetaName.DefaultSpawnPreference
                        break;
                    case "DefaultRemoveRangeMultiplier":
                        DefaultRemoveRangeMultiplier = Xml.GetChildFloatAttribute(reader, "DefaultRemoveRangeMultiplier", "value");
                        break;
                    case "AllowCloseSpawning":
                        AllowCloseSpawning = Xml.GetChildBoolAttribute(reader, "AllowCloseSpawning", "value");
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        public CPedModelInfo__InitData(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            PropsName = Xml.GetChildInnerText(node, "PropsName");
            ClipDictionaryName = Xml.GetChildInnerText(node, "ClipDictionaryName");
            BlendShapeFileName = Xml.GetChildInnerText(node, "BlendShapeFileName");
            ExpressionSetName = Xml.GetChildInnerText(node, "ExpressionSetName");
            ExpressionDictionaryName = Xml.GetChildInnerText(node, "ExpressionDictionaryName");
            ExpressionName = Xml.GetChildInnerText(node, "ExpressionName");
            Pedtype = Xml.GetChildInnerText(node, "Pedtype");
            MovementClipSet = Xml.GetChildInnerText(node, "MovementClipSet");
            var items = node.SelectSingleNode("MovementClipSets")?.SelectNodes("Item");
            if (items?.Count > 0)
            {
                MovementClipSets = new string[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    MovementClipSets[i] = items[i].InnerText;
                }
            }
            StrafeClipSet = Xml.GetChildInnerText(node, "StrafeClipSet");
            MovementToStrafeClipSet = Xml.GetChildInnerText(node, "MovementToStrafeClipSet");
            InjuredStrafeClipSet = Xml.GetChildInnerText(node, "InjuredStrafeClipSet");
            FullBodyDamageClipSet = Xml.GetChildInnerText(node, "FullBodyDamageClipSet");
            AdditiveDamageClipSet = Xml.GetChildInnerText(node, "AdditiveDamageClipSet");
            DefaultGestureClipSet = Xml.GetChildInnerText(node, "DefaultGestureClipSet");
            FacialClipsetGroupName = Xml.GetChildInnerText(node, "FacialClipsetGroupName");
            DefaultVisemeClipSet = Xml.GetChildInnerText(node, "DefaultVisemeClipSet");
            SidestepClipSet = Xml.GetChildInnerText(node, "SidestepClipSet");
            PoseMatcherName = Xml.GetChildInnerText(node, "PoseMatcherName");
            PoseMatcherProneName = Xml.GetChildInnerText(node, "PoseMatcherProneName");
            GetupSetHash = Xml.GetChildInnerText(node, "GetupSetHash");
            CreatureMetadataName = Xml.GetChildInnerText(node, "CreatureMetadataName");
            DecisionMakerName = Xml.GetChildInnerText(node, "DecisionMakerName");
            MotionTaskDataSetName = Xml.GetChildInnerText(node, "MotionTaskDataSetName");
            DefaultTaskDataSetName = Xml.GetChildInnerText(node, "DefaultTaskDataSetName");
            PedCapsuleName = Xml.GetChildInnerText(node, "PedCapsuleName");
            PedLayoutName = Xml.GetChildInnerText(node, "PedLayoutName");
            PedComponentSetName = Xml.GetChildInnerText(node, "PedComponentSetName");
            PedComponentClothName = Xml.GetChildInnerText(node, "PedComponentClothName");
            PedIKSettingsName = Xml.GetChildInnerText(node, "PedIKSettingsName");
            TaskDataName = Xml.GetChildInnerText(node, "TaskDataName");
            IsStreamedGfx = Xml.GetChildBoolAttribute(node, "IsStreamedGfx", "value");
            AmbulanceShouldRespondTo = Xml.GetChildBoolAttribute(node, "AmbulanceShouldRespondTo", "value");
            CanRideBikeWithNoHelmet = Xml.GetChildBoolAttribute(node, "CanRideBikeWithNoHelmet", "value");
            CanSpawnInCar = Xml.GetChildBoolAttribute(node, "CanSpawnInCar", "value");
            IsHeadBlendPed = Xml.GetChildBoolAttribute(node, "IsHeadBlendPed", "value");
            bOnlyBulkyItemVariations = Xml.GetChildBoolAttribute(node, "bOnlyBulkyItemVariations", "value");
            RelationshipGroup = Xml.GetChildInnerText(node, "RelationshipGroup");
            NavCapabilitiesName = Xml.GetChildInnerText(node, "NavCapabilitiesName");
            PerceptionInfo = Xml.GetChildInnerText(node, "PerceptionInfo");
            DefaultBrawlingStyle = Xml.GetChildInnerText(node, "DefaultBrawlingStyle");
            DefaultUnarmedWeapon = Xml.GetChildInnerText(node, "DefaultUnarmedWeapon");
            Personality = Xml.GetChildInnerText(node, "Personality");
            CombatInfo = Xml.GetChildInnerText(node, "CombatInfo");
            VfxInfoName = Xml.GetChildInnerText(node, "VfxInfoName");
            AmbientClipsForFlee = Xml.GetChildInnerText(node, "AmbientClipsForFlee");
            Radio1 = Xml.GetChildInnerText(node, "Radio1"); // MetaName.ePedRadioGenre
            Radio2 = Xml.GetChildInnerText(node, "Radio2"); // MetaName.ePedRadioGenre
            FUpOffset = Xml.GetChildFloatAttribute(node, "FUpOffset", "value");
            RUpOffset = Xml.GetChildFloatAttribute(node, "RUpOffset", "value");
            FFrontOffset = Xml.GetChildFloatAttribute(node, "FFrontOffset", "value");
            RFrontOffset = Xml.GetChildFloatAttribute(node, "RFrontOffset", "value");
            MinActivationImpulse = Xml.GetChildFloatAttribute(node, "MinActivationImpulse", "value");
            Stubble = Xml.GetChildFloatAttribute(node, "Stubble", "value");
            HDDist = Xml.GetChildFloatAttribute(node, "HDDist", "value");
            TargetingThreatModifier = Xml.GetChildFloatAttribute(node, "TargetingThreatModifier", "value");
            KilledPerceptionRangeModifer = Xml.GetChildFloatAttribute(node, "KilledPerceptionRangeModifer", "value");
            Sexiness = Xml.GetChildInnerText(node, "Sexiness"); // MetaTypeName.ARRAYINFO MetaName.eSexinessFlags
            Age = (byte)Xml.GetChildUIntAttribute(node, "Age", "value");
            MaxPassengersInCar = (byte)Xml.GetChildUIntAttribute(node, "MaxPassengersInCar", "value");
            ExternallyDrivenDOFs = Xml.GetChildInnerText(node, "ExternallyDrivenDOFs"); // MetaTypeName.ARRAYINFO MetaName.eExternallyDrivenDOFs
            PedVoiceGroup = Xml.GetChildInnerText(node, "PedVoiceGroup");
            AnimalAudioObject = Xml.GetChildInnerText(node, "AnimalAudioObject");
            AbilityType = Xml.GetChildInnerText(node, "AbilityType"); // MetaName.SpecialAbilityType
            ThermalBehaviour = Xml.GetChildInnerText(node, "ThermalBehaviour"); // MetaName.ThermalBehaviour
            SuperlodType = Xml.GetChildInnerText(node, "SuperlodType"); // MetaName.eSuperlodType
            ScenarioPopStreamingSlot = Xml.GetChildInnerText(node, "ScenarioPopStreamingSlot"); // MetaName.eScenarioPopStreamingSlot
            DefaultSpawningPreference = Xml.GetChildInnerText(node, "DefaultSpawningPreference"); // MetaName.DefaultSpawnPreference
            DefaultRemoveRangeMultiplier = Xml.GetChildFloatAttribute(node, "DefaultRemoveRangeMultiplier", "value");
            AllowCloseSpawning = Xml.GetChildBoolAttribute(node, "AllowCloseSpawning", "value");
        }


        public override string ToString()
        {
            return Name;
        }
    }

    [TC(typeof(EXP))] public class CTxdRelationship
    {
        public string parent { get; set; }
        public string child { get; set; }


        public CTxdRelationship(XmlReader reader)
        {
            if (reader.Name == "parent")
            {
                parent = Xml.GetChildInnerText(reader, "parent");
            } else if (reader.Name == "child")
            {
                child = Xml.GetChildInnerText(reader, "child");
            }
            if (reader.Name == "parent")
            {
                parent = Xml.GetChildInnerText(reader, "parent");
            }
            else if (reader.Name == "child")
            {
                child = Xml.GetChildInnerText(reader, "child");
            }
        }
        public CTxdRelationship(XmlNode node)
        {
            parent = Xml.GetChildInnerText(node, "parent");
            child = Xml.GetChildInnerText(node, "child");
        }

        public override string ToString()
        {
            return parent + ": " + child;
        }
    }

    [TC(typeof(EXP))] public class CMultiTxdRelationship
    {
        public string parent { get; set; }
        public string[] children { get; set; }

        public CMultiTxdRelationship(XmlNode node)
        {
            parent = Xml.GetChildInnerText(node, "parent");
            var items = node.SelectSingleNode("children")?.SelectNodes("Item");
            if (items?.Count > 0)
            {
                children = new string[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    children[i] = items[i].InnerText;
                }
            }
        }

        public CMultiTxdRelationship(XmlReader reader)
        {
            reader.ReadStartElement("Item");
            while (reader.MoveToContent() == XmlNodeType.Element && reader.Name != "Item")
            {
                switch (reader.Name)
                {
                    case "children":
                        var childrenList = new List<string>();
                        foreach (var item in Xml.IterateItems(reader, "children"))
                        {
                            childrenList.Add(item.Value);
                        }
                        if (childrenList.Count > 0)
                        {
                            children = childrenList.ToArray();
                        } 
                        break;
                    case "parent":
                        parent = Xml.GetChildInnerText(reader, "parent");
                        break;
                    default:
                        throw new InvalidOperationException($"Found invalid XML Element \"{reader.Name}\" of type \"{reader.NodeType}\"");
                }
            }
            reader.ReadEndElement();
        }

        public override string ToString()
        {
            return $"{parent}: {(children?.Length ?? 0)} children";
        }
    }






}


