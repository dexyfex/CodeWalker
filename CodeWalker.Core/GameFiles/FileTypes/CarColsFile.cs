using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.IO;
using System.Xml;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class CarColsFile : GameFile, PackedFile
    {
        public PsoFile Pso { get; set; }
        public string Xml { get; set; }

        public CVehicleModelInfoVarGlobal VehicleModelInfo { get; set; }

        public CarColsFile() : base(null, GameFileType.CarCols)
        { }
        public CarColsFile(RpfFileEntry entry) : base(entry, GameFileType.CarCols)
        {
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;



            //can be PSO .ymt or XML .meta
            MemoryStream ms = new MemoryStream(data);
            if (PsoFile.IsPSO(ms))
            {
                Pso = new PsoFile();
                Pso.Load(data);
                Xml = PsoXml.GetXml(Pso); //yep let's just convert that to XML :P
            }
            else
            {
                Xml = Encoding.UTF8.GetString(data);
            }

            XmlDocument xdoc = new XmlDocument();
            if (!string.IsNullOrEmpty(Xml))
            {
                try
                {
                    xdoc.LoadXml(Xml);
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
            }
            else
            { }


            if (xdoc.DocumentElement != null)
            {
                VehicleModelInfo = new CVehicleModelInfoVarGlobal(xdoc.DocumentElement);
            }




            Loaded = true;
        }

    }


    [TC(typeof(EXP))] public class CVehicleModelInfoVarGlobal
    {
        public CVehicleModelInfoVarGlobal_465922034 VehiclePlates { get; set; }
        public CVehicleModelColor[] Colors { get; set; }
        public CVehicleMetallicSetting[] MetallicSettings { get; set; }
        public CVehicleWindowColor[] WindowColors { get; set; }
        public vehicleLightSettings[] Lights { get; set; }
        public sirenSettings[] Sirens { get; set; }
        public CVehicleKit[] Kits { get; set; }
        public CVehicleWheel[][] Wheels { get; set; }
        public CVehicleModelInfoVarGlobal_3062246906 GlobalVariationData { get; set; }
        public CVehicleXenonLightColor[] XenonLightColors { get; set; }

        public CVehicleModelInfoVarGlobal(XmlNode node)
        {

            //for carcols wheels:
            //< Item /> < !--VWT_SPORT-- >
            //< Item /> < !--VWT_MUSCLE-- >
            //< Item /> < !--VWT_LOWRIDER-- >
            //< Item /> < !--VWT_SUV-- >
            //< Item /> < !--VWT_OFFROAD-- >
            //< Item /> < !--VWT_TUNER-- >
            //< Item /> < !--VWT_BIKE-- >
            //< Item /> < !--VWT_HIEND-- >
            //< Item /> < !--VWT_SUPERMOD1-- >
            //< Item >  < !--VWT_SUPERMOD2-- >

            XmlNode cnode;
            cnode = node.SelectSingleNode("VehiclePlates");
            if (cnode != null)
            {
                VehiclePlates = new CVehicleModelInfoVarGlobal_465922034(cnode);
            }
            cnode = node.SelectSingleNode("Colors");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    Colors = new CVehicleModelColor[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        Colors[i] = new CVehicleModelColor(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("MetallicSettings");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    MetallicSettings = new CVehicleMetallicSetting[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        MetallicSettings[i] = new CVehicleMetallicSetting(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("WindowColors");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    WindowColors = new CVehicleWindowColor[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        WindowColors[i] = new CVehicleWindowColor(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("Lights");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    Lights = new vehicleLightSettings[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        Lights[i] = new vehicleLightSettings(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("Sirens");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    Sirens = new sirenSettings[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        Sirens[i] = new sirenSettings(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("Kits");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    Kits = new CVehicleKit[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        Kits[i] = new CVehicleKit(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("Wheels");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    Wheels = new CVehicleWheel[items.Count][];
                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        var items2 = item.SelectNodes("Item");
                        if (items2.Count > 0)
                        {
                            var wheelarr = new CVehicleWheel[items2.Count];
                            for (int j = 0; j < items2.Count; j++)
                            {
                                wheelarr[j] = new CVehicleWheel(items2[j]);
                            }
                            Wheels[i] = wheelarr;
                        }
                    }
                }
            }
            cnode = node.SelectSingleNode("GlobalVariationData");
            if (cnode != null)
            {
                GlobalVariationData = new CVehicleModelInfoVarGlobal_3062246906(cnode);
            }
            cnode = node.SelectSingleNode("XenonLightColors");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    XenonLightColors = new CVehicleXenonLightColor[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        XenonLightColors[i] = new CVehicleXenonLightColor(items[i]);
                    }
                }
            }

        }
    }
    [TC(typeof(EXP))] public class CVehicleModelInfoVarGlobal_465922034 //VehiclePlates
    {
        public CVehicleModelInfoVarGlobal_3027500557[] Textures { get; set; }
        public int DefaultTexureIndex { get; set; }
        public byte NumericOffset { get; set; }
        public byte AlphabeticOffset { get; set; }
        public byte SpaceOffset { get; set; }
        public byte RandomCharOffset { get; set; }
        public byte NumRandomChar { get; set; }

        public CVehicleModelInfoVarGlobal_465922034(XmlNode node)
        {
            XmlNode cnode = node.SelectSingleNode("Textures");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    Textures = new CVehicleModelInfoVarGlobal_3027500557[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        Textures[i] = new CVehicleModelInfoVarGlobal_3027500557(items[i]);
                    }
                }
            }
            DefaultTexureIndex = Xml.GetChildIntAttribute(node, "DefaultTextureIndex", "value");
            NumericOffset = (byte)Xml.GetChildIntAttribute(node, "NumericOffset", "value");
            AlphabeticOffset = (byte)Xml.GetChildIntAttribute(node, "AlphabeticOffset", "value");
            SpaceOffset = (byte)Xml.GetChildIntAttribute(node, "SpaceOffset", "value");
            RandomCharOffset = (byte)Xml.GetChildIntAttribute(node, "RandomCharOffset", "value");
            NumRandomChar = (byte)Xml.GetChildIntAttribute(node, "NumRandomChar", "value");
        }

        public override string ToString()
        {
            return (Textures?.Length ?? 0).ToString() + " Textures";
        }
    }
    [TC(typeof(EXP))] public class CVehicleModelInfoVarGlobal_3027500557 //VehiclePlates Texture
    {
        public MetaHash TextureSetName { get; set; }
        public MetaHash DiffuseMapName { get; set; }
        public MetaHash NormalMapName { get; set; }
        public Vector4 FontExtents { get; set; }
        public Vector2 MaxLettersOnPlate { get; set; }
        public uint FontColor { get; set; }
        public uint FontOutlineColor { get; set; }
        public bool IsFontOutlineEnabled { get; set; }
        public Vector2 FontOutlineMinMaxDepth { get; set; }

        public CVehicleModelInfoVarGlobal_3027500557(XmlNode node)
        {
            TextureSetName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "TextureSetName"));
            DiffuseMapName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "DiffuseMapName"));
            NormalMapName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "NormalMapName"));
            FontExtents = Xml.GetChildVector4Attributes(node, "FontExtents", "x", "y", "z", "w");
            MaxLettersOnPlate = Xml.GetChildVector2Attributes(node, "MaxLettersOnPlate", "x", "y");
            FontColor = Xml.GetChildUIntAttribute(node, "FontColor", "value");
            FontOutlineColor = Xml.GetChildUIntAttribute(node, "FontOutlineColor", "value");
            IsFontOutlineEnabled = Xml.GetChildBoolAttribute(node, "IsFontOutlineEnabled", "value");
            FontOutlineMinMaxDepth = Xml.GetChildVector2Attributes(node, "FontOutlineMinMaxDepth", "x", "y");
        }

        public override string ToString()
        {
            return TextureSetName.ToString() + ", " + DiffuseMapName.ToString() + ", " + NormalMapName.ToString();
        }
    }
    [TC(typeof(EXP))] public class CVehicleModelColor
    {
        public uint color { get; set; }
        public CVehicleModelColor_360458334 metallicID { get; set; }
        public CVehicleModelColor_544262540 audioColor { get; set; }
        public CVehicleModelColor_2065815796 audioPrefix { get; set; }
        public MetaHash audioColorHash { get; set; }
        public MetaHash audioPrefixHash { get; set; }
        public string colorName { get; set; }

        public CVehicleModelColor(XmlNode node)
        {
            color = Xml.GetChildUIntAttribute(node, "color", "value");
            metallicID = Xml.GetChildEnumInnerText<CVehicleModelColor_360458334>(node, "metallicID");
            audioColor = Xml.GetChildEnumInnerText<CVehicleModelColor_544262540>(node, "audioColor");
            audioPrefix = Xml.GetChildEnumInnerText<CVehicleModelColor_2065815796>(node, "audioPrefix");
            audioColorHash = (uint)Xml.GetChildIntAttribute(node, "audioColorHash", "value");
            audioPrefixHash = (uint)Xml.GetChildIntAttribute(node, "audioPrefixHash", "value");
            colorName = Xml.GetChildInnerText(node, "colorName");
        }

        public override string ToString()
        {
            return colorName;
        }
    }
    [TC(typeof(EXP))] public class CVehicleMetallicSetting
    {
        public float specInt { get; set; }
        public float specFalloff { get; set; }
        public float specFresnel { get; set; }

        public CVehicleMetallicSetting(XmlNode node)
        {
            specInt = Xml.GetChildFloatAttribute(node, "specInt", "value");
            specFalloff = Xml.GetChildFloatAttribute(node, "specFalloff", "value");
            specFresnel = Xml.GetChildFloatAttribute(node, "specFresnel", "value");
        }
        public override string ToString()
        {
            return specInt.ToString() + ", " + specFalloff.ToString() + ", " + specFresnel.ToString();
        }
    }
    [TC(typeof(EXP))] public class CVehicleWindowColor
    {
        public uint color { get; set; }
        public MetaHash name { get; set; }

        public CVehicleWindowColor(XmlNode node)
        {
            color = Xml.GetChildUIntAttribute(node, "color", "value");
            name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "name"));
        }
        public override string ToString()
        {
            return name.ToString();
        }
    }
    [TC(typeof(EXP))] public class vehicleLightSettings
    {
        public byte id { get; set; }
        public vehicleLight indicator { get; set; }
        public vehicleCorona rearIndicatorCorona { get; set; }
        public vehicleCorona frontIndicatorCorona { get; set; }
        public vehicleLight tailLight { get; set; }
        public vehicleCorona tailLightCorona { get; set; }
        public vehicleCorona tailLightMiddleCorona { get; set; }
        public vehicleLight headLight { get; set; }
        public vehicleCorona headLightCorona { get; set; }
        public vehicleLight reversingLight { get; set; }
        public vehicleCorona reversingLightCorona { get; set; }
        public string name { get; set; }

        public vehicleLightSettings(XmlNode node)
        {
            id = (byte)Xml.GetChildIntAttribute(node, "id", "value");
            XmlNode cnode;
            cnode = node.SelectSingleNode("indicator");
            if (cnode != null)
            {
                indicator = new vehicleLight(cnode);
            }
            cnode = node.SelectSingleNode("rearIndicatorCorona");
            if (cnode != null)
            {
                rearIndicatorCorona = new vehicleCorona(cnode);
            }
            cnode = node.SelectSingleNode("frontIndicatorCorona");
            if (cnode != null)
            {
                frontIndicatorCorona = new vehicleCorona(cnode);
            }
            cnode = node.SelectSingleNode("tailLight");
            if (cnode != null)
            {
                tailLight = new vehicleLight(cnode);
            }
            cnode = node.SelectSingleNode("tailLightCorona");
            if (cnode != null)
            {
                tailLightCorona = new vehicleCorona(cnode);
            }
            cnode = node.SelectSingleNode("tailLightMiddleCorona");
            if (cnode != null)
            {
                tailLightMiddleCorona = new vehicleCorona(cnode);
            }
            cnode = node.SelectSingleNode("headLight");
            if (cnode != null)
            {
                headLight = new vehicleLight(cnode);
            }
            cnode = node.SelectSingleNode("headLightCorona");
            if (cnode != null)
            {
                headLightCorona = new vehicleCorona(cnode);
            }
            cnode = node.SelectSingleNode("reversingLight");
            if (cnode != null)
            {
                reversingLight = new vehicleLight(cnode);
            }
            cnode = node.SelectSingleNode("reversingLightCorona");
            if (cnode != null)
            {
                reversingLightCorona = new vehicleCorona(cnode);
            }
            name = Xml.GetChildInnerText(node, "name");
        }
        public override string ToString()
        {
            return id.ToString() + ": " + name;
        }
    }
    [TC(typeof(EXP))] public class vehicleLight
    {
        public float intensity { get; set; }
        public float falloffMax { get; set; }
        public float falloffExponent { get; set; }
        public float innerConeAngle { get; set; }
        public float outerConeAngle { get; set; }
        public bool emmissiveBoost { get; set; }
        public uint color { get; set; }
        public MetaHash textureName { get; set; }
        public bool mirrorTexture { get; set; }

        public vehicleLight(XmlNode node)
        {
            intensity = Xml.GetChildFloatAttribute(node, "intensity", "value");
            falloffMax = Xml.GetChildFloatAttribute(node, "falloffMax", "value");
            falloffExponent = Xml.GetChildFloatAttribute(node, "falloffExponent", "value");
            innerConeAngle = Xml.GetChildFloatAttribute(node, "innerConeAngle", "value");
            outerConeAngle = Xml.GetChildFloatAttribute(node, "outerConeAngle", "value");
            emmissiveBoost = Xml.GetChildBoolAttribute(node, "emmissiveBoost", "value");
            color = Xml.GetChildUIntAttribute(node, "color", "value");
            textureName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "textureName"));
            mirrorTexture = Xml.GetChildBoolAttribute(node, "mirrorTexture", "value");
        }
    }
    [TC(typeof(EXP))] public class vehicleCorona
    {
        public float size { get; set; }
        public float size_far { get; set; }
        public float intensity { get; set; }
        public float intensity_far { get; set; }
        public uint color { get; set; }
        public byte numCoronas { get; set; }
        public byte distBetweenCoronas { get; set; }
        public byte distBetweenCoronas_far { get; set; }
        public float xRotation { get; set; }
        public float yRotation { get; set; }
        public float zRotation { get; set; }
        public float zBias { get; set; }
        public bool pullCoronaIn { get; set; }

        public vehicleCorona(XmlNode node)
        {
            size = Xml.GetChildFloatAttribute(node, "size", "value");
            size_far = Xml.GetChildFloatAttribute(node, "size_far", "value");
            intensity = Xml.GetChildFloatAttribute(node, "intensity", "value");
            intensity_far = Xml.GetChildFloatAttribute(node, "intensity_far", "value");
            color = Xml.GetChildUIntAttribute(node, "color", "value");
            numCoronas = (byte)Xml.GetChildIntAttribute(node, "numCoronas", "value");
            distBetweenCoronas = (byte)Xml.GetChildIntAttribute(node, "distBetweenCoronas", "value");
            distBetweenCoronas_far = (byte)Xml.GetChildIntAttribute(node, "distBetweenCoronas_far", "value");
            xRotation = Xml.GetChildFloatAttribute(node, "xRotation", "value");
            yRotation = Xml.GetChildFloatAttribute(node, "yRotation", "value");
            zRotation = Xml.GetChildFloatAttribute(node, "zRotation", "value");
            zBias = Xml.GetChildFloatAttribute(node, "zBias", "value");
            pullCoronaIn = Xml.GetChildBoolAttribute(node, "pullCoronaIn", "value");
        }
    }
    [TC(typeof(EXP))] public class sirenSettings
    {
        public byte id { get; set; }
        public string name { get; set; }
        public float timeMultiplier { get; set; }
        public float lightFalloffMax { get; set; }
        public float lightFalloffExponent { get; set; }
        public float lightInnerConeAngle { get; set; }
        public float lightOuterConeAngle { get; set; }
        public float lightOffset { get; set; }
        public MetaHash textureName { get; set; }
        public uint sequencerBpm { get; set; }
        public sirenSettings_188820339 leftHeadLight { get; set; }
        public sirenSettings_188820339 rightHeadLight { get; set; }
        public sirenSettings_188820339 leftTailLight { get; set; }
        public sirenSettings_188820339 rightTailLight { get; set; }
        public byte leftHeadLightMultiples { get; set; }
        public byte rightHeadLightMultiples { get; set; }
        public byte leftTailLightMultiples { get; set; }
        public byte rightTailLightMultiples { get; set; }
        public bool useRealLights { get; set; }
        public sirenLight[] sirens { get; set; }

        public sirenSettings(XmlNode node)
        {
            id = (byte)Xml.GetChildIntAttribute(node, "id", "value");
            name = Xml.GetChildInnerText(node, "name");
            timeMultiplier = Xml.GetChildFloatAttribute(node, "timeMultiplier", "value");
            lightFalloffMax = Xml.GetChildFloatAttribute(node, "lightFalloffMax", "value");
            lightFalloffExponent = Xml.GetChildFloatAttribute(node, "lightFalloffExponent", "value");
            lightInnerConeAngle = Xml.GetChildFloatAttribute(node, "lightInnerConeAngle", "value");
            lightOuterConeAngle = Xml.GetChildFloatAttribute(node, "lightOuterConeAngle", "value");
            lightOffset = Xml.GetChildFloatAttribute(node, "lightOffset", "value");
            textureName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "textureName"));
            sequencerBpm = Xml.GetChildUIntAttribute(node, "sequencerBpm", "value");
            XmlNode cnode;
            cnode = node.SelectSingleNode("leftHeadLight");
            if (cnode != null)
            {
                leftHeadLight = new sirenSettings_188820339(cnode);
            }
            cnode = node.SelectSingleNode("rightHeadLight");
            if (cnode != null)
            {
                rightHeadLight = new sirenSettings_188820339(cnode);
            }
            cnode = node.SelectSingleNode("leftTailLight");
            if (cnode != null)
            {
                leftTailLight = new sirenSettings_188820339(cnode);
            }
            cnode = node.SelectSingleNode("rightTailLight");
            if (cnode != null)
            {
                rightTailLight = new sirenSettings_188820339(cnode);
            }
            leftHeadLightMultiples = (byte)Xml.GetChildIntAttribute(node, "leftHeadLightMultiples", "value");
            rightHeadLightMultiples = (byte)Xml.GetChildIntAttribute(node, "rightHeadLightMultiples", "value");
            leftTailLightMultiples = (byte)Xml.GetChildIntAttribute(node, "leftTailLightMultiples", "value");
            rightTailLightMultiples = (byte)Xml.GetChildIntAttribute(node, "rightTailLightMultiples", "value");
            useRealLights = Xml.GetChildBoolAttribute(node, "useRealLights", "value");
            cnode = node.SelectSingleNode("sirens");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    sirens = new sirenLight[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        sirens[i] = new sirenLight(items[i]);
                    }
                }
            }
        }

        public override string ToString()
        {
            return id.ToString() + ": " + name;
        }
    }
    [TC(typeof(EXP))] public class sirenSettings_188820339
    {
        public uint sequencer { get; set; }

        public sirenSettings_188820339(XmlNode node)
        {
            sequencer = Xml.GetChildUIntAttribute(node, "sequencer", "value");
        }
    }
    [TC(typeof(EXP))] public class sirenLight
    {
        public sirenLight_1356743507 rotation { get; set; }
        public sirenLight_1356743507 flashiness { get; set; }
        public sirenCorona corona { get; set; }
        public uint color { get; set; }
        public float intensity { get; set; }
        public byte lightGroup { get; set; }
        public bool rotate { get; set; }
        public bool scale { get; set; }
        public byte scaleFactor { get; set; }
        public bool flash { get; set; }
        public bool light { get; set; }
        public bool spotLight { get; set; }
        public bool castShadows { get; set; }

        public sirenLight(XmlNode node)
        {
            XmlNode cnode;
            cnode = node.SelectSingleNode("rotation");
            if (cnode != null)
            {
                rotation = new sirenLight_1356743507(cnode);
            }
            cnode = node.SelectSingleNode("flashiness");
            if (cnode != null)
            {
                flashiness = new sirenLight_1356743507(cnode);
            }
            cnode = node.SelectSingleNode("corona");
            if (cnode != null)
            {
                corona = new sirenCorona(cnode);
            }
            color = Xml.GetChildUIntAttribute(node, "color", "value");
            intensity = Xml.GetChildFloatAttribute(node, "intensity", "value");
            lightGroup = (byte)Xml.GetChildIntAttribute(node, "lightGroup", "value");
            rotate = Xml.GetChildBoolAttribute(node, "rotate", "value");
            scale = Xml.GetChildBoolAttribute(node, "scale", "value");
            scaleFactor = (byte)Xml.GetChildIntAttribute(node, "scaleFactor", "value");
            flash = Xml.GetChildBoolAttribute(node, "flash", "value");
            light = Xml.GetChildBoolAttribute(node, "light", "value");
            spotLight = Xml.GetChildBoolAttribute(node, "spotLight", "value");
            castShadows = Xml.GetChildBoolAttribute(node, "castShadows", "value");

        }
    }
    [TC(typeof(EXP))] public class sirenLight_1356743507
    {
        public float delta { get; set; }
        public float start { get; set; }
        public float speed { get; set; }
        public uint sequencer { get; set; }
        public byte multiples { get; set; }
        public bool direction { get; set; }
        public bool syncToBpm { get; set; }

        public sirenLight_1356743507(XmlNode node)
        {
            delta = Xml.GetChildFloatAttribute(node, "delta", "value");
            start = Xml.GetChildFloatAttribute(node, "start", "value");
            speed = Xml.GetChildFloatAttribute(node, "speed", "value");
            sequencer = Xml.GetChildUIntAttribute(node, "sequencer", "value");
            multiples = (byte)Xml.GetChildIntAttribute(node, "multiples", "value");
            direction = Xml.GetChildBoolAttribute(node, "direction", "value");
            syncToBpm = Xml.GetChildBoolAttribute(node, "syncToBpm", "value");
        }
    }
    [TC(typeof(EXP))] public class sirenCorona
    {
        public float intensity { get; set; }
        public float size { get; set; }
        public float pull { get; set; }
        public bool faceCamera { get; set; }

        public sirenCorona(XmlNode node)
        {
            intensity = Xml.GetChildFloatAttribute(node, "intensity", "value");
            size = Xml.GetChildFloatAttribute(node, "size", "value");
            pull = Xml.GetChildFloatAttribute(node, "pull", "value");
            faceCamera = Xml.GetChildBoolAttribute(node, "faceCamera", "value");
        }
    }
    [TC(typeof(EXP))] public class CVehicleKit
    {
        public MetaHash kitName { get; set; }
        public ushort id { get; set; }
        public CVehicleKit_3865430600 kitType { get; set; }
        public CVehicleModVisible[] visibleMods { get; set; }
        public CVehicleModLink[] linkMods { get; set; }
        public CVehicleModStat[] statMods { get; set; }
        public CVehicleKit_427606548[] slotNames { get; set; }
        public MetaHash[] liveryNames { get; set; }
        public MetaHash[] livery2Names { get; set; }

        public CVehicleKit(XmlNode node)
        {
            kitName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "kitName"));
            id = (ushort)Xml.GetChildUIntAttribute(node, "id", "value");
            kitType = Xml.GetChildEnumInnerText<CVehicleKit_3865430600>(node, "kitType");
            XmlNode cnode;
            cnode = node.SelectSingleNode("visibleMods");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    visibleMods = new CVehicleModVisible[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        visibleMods[i] = new CVehicleModVisible(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("linkMods");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    linkMods = new CVehicleModLink[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        linkMods[i] = new CVehicleModLink(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("statMods");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    statMods = new CVehicleModStat[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        statMods[i] = new CVehicleModStat(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("slotNames");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    slotNames = new CVehicleKit_427606548[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        slotNames[i] = new CVehicleKit_427606548(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("liveryNames");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    liveryNames = new MetaHash[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        liveryNames[i] = XmlMeta.GetHash(items[i].InnerText);
                    }
                }
            }
            cnode = node.SelectSingleNode("livery2Names");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    livery2Names = new MetaHash[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        livery2Names[i] = XmlMeta.GetHash(items[i].InnerText);
                    }
                }
            }

        }

        public override string ToString()
        {
            return id.ToString() + ": " + kitName.ToString();
        }
    }
    [TC(typeof(EXP))] public class CVehicleModVisible
    {
        public MetaHash modelName { get; set; }
        public string modShopLabel { get; set; }
        public MetaHash[] linkedModels { get; set; }
        public CVehicleMod_3635907608[] turnOffBones { get; set; }
        public CVehicleMod_2363989491 type { get; set; }
        public CVehicleMod_3635907608 bone { get; set; }
        public CVehicleMod_3635907608 collisionBone { get; set; }
        public CVehicleMod_3413962745 cameraPos { get; set; }
        public float audioApply { get; set; }
        public byte weight { get; set; }
        public bool turnOffExtra { get; set; }
        public bool disableBonnetCamera { get; set; }
        public bool allowBonnetSlide { get; set; }
        public sbyte weaponSlot { get; set; }
        public sbyte Unk_2656206330 { get; set; }
        public bool disableProjectileDriveby { get; set; }
        public bool disableDriveby { get; set; }
        public int Unk_161724223 { get; set; }
        public int Unk_484538291 { get; set; }

        public CVehicleModVisible(XmlNode node)
        {
            modelName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "modelName"));
            modShopLabel = Xml.GetChildInnerText(node, "modShopLabel");
            XmlNode cnode;
            cnode = node.SelectSingleNode("linkedModels");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    linkedModels = new MetaHash[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        linkedModels[i] = XmlMeta.GetHash(items[i].InnerText);
                    }
                }
            }
            cnode = node.SelectSingleNode("turnOffBones");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    turnOffBones = new CVehicleMod_3635907608[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        turnOffBones[i] = Xml.GetEnumValue<CVehicleMod_3635907608>(items[i].InnerText);
                    }
                }
            }
            type = Xml.GetChildEnumInnerText<CVehicleMod_2363989491>(node, "type");
            bone = Xml.GetChildEnumInnerText<CVehicleMod_3635907608>(node, "bone");
            collisionBone = Xml.GetChildEnumInnerText<CVehicleMod_3635907608>(node, "collisionBone");
            cameraPos = Xml.GetChildEnumInnerText<CVehicleMod_3413962745>(node, "cameraPos");
            audioApply = Xml.GetChildFloatAttribute(node, "audioApply", "value");
            weight = (byte)Xml.GetChildIntAttribute(node, "weight", "value");
            turnOffExtra = Xml.GetChildBoolAttribute(node, "turnOffExtra", "value");
            disableBonnetCamera = Xml.GetChildBoolAttribute(node, "disableBonnetCamera", "value");
            allowBonnetSlide = Xml.GetChildBoolAttribute(node, "allowBonnetSlide", "value");
            weaponSlot = (sbyte)Xml.GetChildIntAttribute(node, "weaponSlot", "value");
            Unk_2656206330 = (sbyte)Xml.GetChildIntAttribute(node, "hash_9E527DFA", "value");//TODO: fix
            disableProjectileDriveby = Xml.GetChildBoolAttribute(node, "disableProjectileDriveby", "value");
            disableDriveby = Xml.GetChildBoolAttribute(node, "disableDriveby", "value");
            Unk_161724223 = Xml.GetChildIntAttribute(node, "hash_09A3B73F", "value");//TODO: fix
            Unk_484538291 = Xml.GetChildIntAttribute(node, "hash_1CE177B3", "value");//TODO: fix
        }

        public override string ToString()
        {
            return modelName.ToString() + ": " + modShopLabel + ": " + type.ToString() + ": " + bone.ToString();
        }
    }
    [TC(typeof(EXP))] public class CVehicleModLink
    {
        public MetaHash modelName { get; set; }
        public CVehicleMod_3635907608 bone { get; set; }
        public bool turnOffExtra { get; set; }

        public CVehicleModLink(XmlNode node)
        {
            modelName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "modelName"));
            bone = Xml.GetChildEnumInnerText<CVehicleMod_3635907608>(node, "bone");
            turnOffExtra = Xml.GetChildBoolAttribute(node, "turnOffExtra", "value");
        }

        public override string ToString()
        {
            return modelName.ToString() + ": " + bone.ToString();
        }
    }
    [TC(typeof(EXP))] public class CVehicleModStat
    {
        public MetaHash identifier { get; set; }
        public uint modifier { get; set; }
        public float audioApply { get; set; }
        public byte weight { get; set; }
        public CVehicleMod_2363989491 type { get; set; }

        public CVehicleModStat(XmlNode node)
        {
            identifier = XmlMeta.GetHash(Xml.GetChildInnerText(node, "identifier"));
            modifier = Xml.GetChildUIntAttribute(node, "modifier", "value");
            audioApply = Xml.GetChildFloatAttribute(node, "audioApply", "value");
            weight = (byte)Xml.GetChildIntAttribute(node, "weight", "value");
            type = Xml.GetChildEnumInnerText<CVehicleMod_2363989491>(node, "type");
        }
        public override string ToString()
        {
            return identifier.ToString() + ": " + type.ToString();
        }
    }
    [TC(typeof(EXP))] public class CVehicleKit_427606548
    {
        public CVehicleMod_2363989491 slot { get; set; }
        public string name { get; set; }

        public CVehicleKit_427606548(XmlNode node)
        {
            slot = Xml.GetChildEnumInnerText<CVehicleMod_2363989491>(node, "slot");
            name = Xml.GetChildInnerText(node, "name");
        }
        public override string ToString()
        {
            return name + ": " + slot.ToString();
        }
    }
    [TC(typeof(EXP))] public class CVehicleWheel
    {
        public MetaHash wheelName { get; set; }
        public MetaHash wheelVariation { get; set; }
        public string modShopLabel { get; set; }
        public float rimRadius { get; set; }
        public bool rear { get; set; }

        public CVehicleWheel(XmlNode node)
        {
            wheelName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "wheelName"));
            wheelVariation = XmlMeta.GetHash(Xml.GetChildInnerText(node, "wheelVariation"));
            modShopLabel = Xml.GetChildInnerText(node, "modShopLabel");
            rimRadius = Xml.GetChildFloatAttribute(node, "rimRadius", "value");
            rear = Xml.GetChildBoolAttribute(node, "rear", "value");
        }
        public override string ToString()
        {
            return wheelName.ToString() + ": " + wheelVariation.ToString() + ": " + modShopLabel;
        }
    }
    [TC(typeof(EXP))] public class CVehicleModelInfoVarGlobal_3062246906 //GlobalVariationData
    {
        public uint xenonLightColor { get; set; }
        public uint xenonCoronaColor { get; set; }
        public float xenonLightIntensityModifier { get; set; }
        public float xenonCoronaIntensityModifier { get; set; }

        public CVehicleModelInfoVarGlobal_3062246906(XmlNode node)
        {
            xenonLightColor = Xml.GetChildUIntAttribute(node, "xenonLightColor", "value");
            xenonCoronaColor = Xml.GetChildUIntAttribute(node, "xenonCoronaColor", "value");
            xenonLightIntensityModifier = Xml.GetChildFloatAttribute(node, "xenonLightIntensityModifier", "value");
            xenonCoronaIntensityModifier = Xml.GetChildFloatAttribute(node, "xenonCoronaIntensityModifier", "value");
        }
    }
    [TC(typeof(EXP))] public class CVehicleXenonLightColor
    {
        public uint lightColor { get; set; }
        public uint coronaColor { get; set; }
        public float lightIntensityModifier { get; set; }
        public float coronaIntensityModifier { get; set; }

        public CVehicleXenonLightColor(XmlNode node)
        {
            lightColor = Xml.GetChildUIntAttribute(node, "lightColor", "value");
            coronaColor = Xml.GetChildUIntAttribute(node, "coronaColor", "value");
            lightIntensityModifier = Xml.GetChildFloatAttribute(node, "lightIntensityModifier", "value");
            coronaIntensityModifier = Xml.GetChildFloatAttribute(node, "coronaIntensityModifier", "value");
        }
    }


    public enum CVehicleModelColor_360458334 //vehicle mod color metallic id
    {
        none = -1,
        Unk_815563434 = 0,
        Unk_969376721 = 1,
        Unk_731244402 = 2,
        Unk_3537679873 = 3,
        Unk_3183119293 = 4,
        Unk_4039766491 = 5,
        Unk_3777516184 = 6,
        Unk_229878718 = 7,
        Unk_4285337393 = 8,
        Unk_847803751 = 9
    }
    public enum CVehicleModelColor_544262540 //vehicle mod color audio color
    {
        Unk_52583446 = 0,
        Unk_2253400966 = 1,
        Unk_4012089111 = 2,
        Unk_2477037232 = 3,
        Unk_437487559 = 4,
        Unk_730308174 = 5,
        Unk_3129410619 = 6,
        Unk_1762671403 = 7,
        Unk_2615326998 = 8,
        Unk_4157715503 = 9,
        Unk_2969646890 = 10,
        Unk_1645071976 = 11,
        Unk_1358390657 = 12
    }
    public enum CVehicleModelColor_2065815796 //vehicle mod color audio prefix
    {
        none = 0,
        Unk_2318284788 = 1,
        Unk_840729009 = 2,
        Unk_3921902053 = 3
    }

    public enum CVehicleKit_3865430600 //vehicle mod kit type
    {
        MKT_STANDARD = 0,
        MKT_SPORT = 1,
        MKT_SUV = 2,
        MKT_SPECIAL = 3
    }
    public enum CVehicleMod_3635907608 //vehicle mod bone
    {
        none = -1,
        chassis = 0,
        Unk_1970334199 = 48,
        bumper_f = 49,
        bumper_r = 50,
        wing_rf = 51,
        wing_lf = 52,
        bonnet = 53,
        boot = 54,
        exhaust = 56,
        exhaust_2 = 57,
        exhaust_3 = 58,
        exhaust_4 = 59,
        Unk_1666652977 = 60,
        Unk_1972682668 = 61,
        Unk_3204862618 = 62,
        Unk_3503093287 = 63,
        Unk_442599751 = 64,
        Unk_2600343066 = 65,
        Unk_3598945568 = 66,
        Unk_3896619164 = 67,
        Unk_2989016171 = 68,
        Unk_3320572913 = 69,
        Unk_529211190 = 70,
        Unk_823345734 = 71,
        extra_1 = 401,
        extra_2 = 402,
        extra_3 = 403,
        extra_4 = 404,
        extra_5 = 405,
        Unk_3145755371 = 406,
        Unk_2360446286 = 407,
        Unk_2661527858 = 408,
        Unk_1913477126 = 409,
        Unk_2125833567 = 410,
        Unk_2431830489 = 411,
        Unk_2720558148 = 412,
        Unk_903844796 = 413,
        Unk_1202337617 = 414,
        Unk_2190258525 = 417,
        Unk_1950749904 = 418,
        Unk_1709504522 = 419,
        Unk_1480285367 = 420,
        Unk_1213709552 = 421,
        Unk_848662892 = 422,
        Unk_3990325233 = 423,
        Unk_367056899 = 424,
        Unk_3413787451 = 425,
        Unk_2734584581 = 426,
        mod_col_1 = 427,
        mod_col_2 = 428,
        mod_col_3 = 429,
        mod_col_4 = 430,
        mod_col_5 = 431,
        Unk_1547719720 = 432,
        Unk_580018373 = 433,
        Unk_4191162177 = 434,
        Unk_3257212908 = 435,
        Unk_3033709986 = 436,
        Unk_2249285664 = 437,
        Unk_1498318491 = 438,
        Unk_1787603223 = 439,
        Unk_1573326728 = 440,
        Unk_1862676998 = 441,
        Unk_1044598917 = 442,
        misc_a = 369,
        misc_b = 370,
        misc_c = 371,
        misc_d = 372,
        misc_e = 373,
        misc_f = 374,
        misc_g = 375,
        misc_h = 376,
        misc_i = 377,
        misc_j = 378,
        misc_k = 379,
        misc_l = 380,
        misc_m = 381,
        misc_n = 382,
        misc_o = 383,
        misc_p = 384,
        misc_q = 385,
        misc_r = 386,
        misc_s = 387,
        misc_t = 388,
        misc_u = 389,
        misc_v = 390,
        misc_w = 391,
        misc_x = 392,
        misc_y = 393,
        misc_z = 394,
        Unk_2690009945 = 395,
        Unk_1385115596 = 396,
        Unk_2921986035 = 79,
        steeringwheel = 80,
        Unk_3929033220 = 29,
        Unk_837923007 = 21,
        Unk_991413023 = 22,
        headlight_l = 91,
        headlight_r = 92,
        Unk_3979005071 = 97,
        indicator_lf = 95,
        Unk_112742354 = 98,
        indicator_rf = 96,
        Unk_3008822349 = 93,
        Unk_914195096 = 94,
        window_lf = 42,
        window_rf = 43,
        Unk_2355575415 = 45,
        Unk_2639884295 = 44,
        Unk_2675373130 = 46,
        Unk_3523659197 = 47,
        hub_lf = 30,
        hub_rf = 31,
        windscreen_r = 41,
        Unk_4084563217 = 104,
        Unk_556062831 = 105,
        Unk_3850133799 = 106,
        Unk_728001778 = 107,
        door_dside_f = 3,
        door_dside_r = 4,
        door_pside_f = 5,
        door_pside_r = 6,
        bobble_head = 361,
        bobble_base = 362,
        Unk_785162942 = 363,
        engineblock = 364,
        Unk_3654959273 = 474,
        Unk_808381781 = 475,
        Unk_4268165574 = 476,
        Unk_3579951032 = 477,
        Unk_2733953759 = 478,
        Unk_4123490435 = 479,
        Unk_3342441320 = 480,
        Unk_1726175929 = 481,
        Unk_1947366679 = 482,
        Unk_2065498928 = 483,
        Unk_2572238744 = 484,
        Unk_730555402 = 485,
        Unk_4194074861 = 486,
        Unk_1343696161 = 487,
        Unk_506677594 = 488,
        Unk_4167728581 = 489,
        Unk_3326122354 = 490,
        Unk_217851628 = 491,
        Unk_3666821647 = 492,
        Unk_2168885119 = 493,
        Unk_2399611648 = 494,
        Unk_3519328378 = 495,
        Unk_2958716326 = 496,
        Unk_3292501248 = 497,
        Unk_2651277456 = 498,
        Unk_3888995355 = 499,
        Unk_3843353624 = 500,
        Unk_3135215534 = 501,
        Unk_3364434689 = 502,
        Unk_2723112578 = 503,
        Unk_2970026993 = 504,
        Unk_2245832093 = 505,
        Unk_2492156666 = 506,
        Unk_1765012564 = 507,
        Unk_2014155263 = 508,
        Unk_1286290243 = 509,
        Unk_1535367412 = 510,
        Unk_3992710274 = 511,
        Unk_1077448950 = 512,
        Unk_1382757723 = 513,
        Unk_614062521 = 514,
        Unk_3783807571 = 524,
        Unk_2839830988 = 525,
        Unk_3138651499 = 526,
        Unk_2400824695 = 527,
        Unk_4141162370 = 560,
        Unk_451472348 = 561,
        Unk_1768767594 = 562,
        Unk_3599310922 = 563,
        Unk_1496111458 = 564,
        Unk_3929521030 = 565,
        Unk_1157497495 = 566,
        Unk_1032498025 = 567,
        Unk_2573434709 = 568,
        Unk_930609265 = 569,
        Unk_879315983 = 570,
        Unk_411536741 = 571,
        Unk_1086719913 = 572,
        Unk_3237490897 = 573,
        Unk_3375838140 = 574,
        Unk_2381840182 = 575,
        Unk_3607058940 = 576,
        Unk_3607058940_again = 577,
        Unk_1208798824 = 578,
        Unk_303656220 = 579,
        Unk_660207018 = 580,
        Unk_2796607005 = 581,
        Unk_3045655218 = 582,
        Unk_2017296145 = 583,
        Unk_3978311755 = 584,
        Unk_1122332083 = 585,
        Unk_1123212214 = 586,
        Unk_4063416457 = 587,
        Unk_4011591561 = 588,
        Unk_2320654166 = 589,
        Unk_2292220415 = 590,
        Unk_2166816405 = 591,
        Unk_2486991428 = 592
    }
    public enum CVehicleMod_2363989491 //vehicle mod type
    {
        VMT_SPOILER = 0,
        VMT_BUMPER_F = 1,
        VMT_BUMPER_R = 2,
        VMT_SKIRT = 3,
        VMT_EXHAUST = 4,
        VMT_CHASSIS = 5,
        VMT_GRILL = 6,
        VMT_BONNET = 7,
        VMT_WING_L = 8,
        VMT_WING_R = 9,
        VMT_ROOF = 10,
        VMT_PLTHOLDER = 11,
        VMT_PLTVANITY = 12,
        VMT_INTERIOR1 = 13,
        VMT_INTERIOR2 = 14,
        VMT_INTERIOR3 = 15,
        VMT_INTERIOR4 = 16,
        VMT_INTERIOR5 = 17,
        VMT_SEATS = 18,
        VMT_STEERING = 19,
        VMT_KNOB = 20,
        VMT_PLAQUE = 21,
        VMT_ICE = 22,
        VMT_TRUNK = 23,
        VMT_HYDRO = 24,
        VMT_ENGINEBAY1 = 25,
        VMT_ENGINEBAY2 = 26,
        VMT_ENGINEBAY3 = 27,
        VMT_CHASSIS2 = 28,
        VMT_CHASSIS3 = 29,
        VMT_CHASSIS4 = 30,
        VMT_CHASSIS5 = 31,
        VMT_DOOR_L = 32,
        Unk_514696742 = 33,
        VMT_LIVERY_MOD = 34,
        Unk_3409280882 = 35,
        VMT_ENGINE = 36,
        VMT_BRAKES = 37,
        VMT_GEARBOX = 38,
        VMT_HORN = 39,
        VMT_SUSPENSION = 40,
        VMT_ARMOUR = 41,
        Unk_3278520444 = 42,
        Unk_2334449168 = 43,
        Unk_1675686396 = 44,
        Unk_3968559424 = 45,
        Unk_3919652883 = 46,
        Unk_2656522436 = 47,
        VMT_WHEELS = 48,
        VMT_WHEELS_REAR_OR_HYDRAULICS = 49
    }
    public enum CVehicleMod_3413962745 //vehicle mod camera position
    {
        VMCP_DEFAULT = 0,
        Unk_3001706694 = 1,
        Unk_1148399176 = 2,
        Unk_2057559994 = 3,
        Unk_1556139353 = 4,
        Unk_1830737308 = 5,
        Unk_457854237 = 6,
        Unk_1629770278 = 7,
        Unk_2879151926 = 8,
        Unk_3193071790 = 9,
        Unk_2770495034 = 10
    }


}

