using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Xml;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class CutFile : PackedFile
    {
        public RpfFileEntry FileEntry { get; set; }
        public PsoFile Pso { get; set; }


        public CutsceneFile2 CutsceneFile2 { get; set; }


        public CutFile()
        { }
        public CutFile(RpfFileEntry entry)
        {
            FileEntry = entry;
        }


        public void Load(byte[] data, RpfFileEntry entry)
        {
            FileEntry = entry;

            MemoryStream ms = new MemoryStream(data);

            if (PsoFile.IsPSO(ms))
            {
                Pso = new PsoFile();
                Pso.Load(ms);

                var xml = PsoXml.GetXml(Pso);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                var node = doc.DocumentElement;

                CutsceneFile2 = new CutsceneFile2();
                CutsceneFile2.ReadXml(node);

            }
            else
            {

            }
        }

    }


    [TC(typeof(EXP))] public abstract class CutBase : IMetaXmlItem
    {
        public virtual void ReadXml(XmlNode node)
        { }
        public virtual void WriteXml(StringBuilder sb, int indent)
        { }
    }


    [TC(typeof(EXP))] public class CutsceneFile2 : CutBase  // rage__cutfCutsceneFile2
    {
        public float fTotalDuration { get; set; } //fTotalDuration, PsoDataType.Float, 268, 0, 0),
        public string cFaceDir { get; set; } //cFaceDir, PsoDataType.String, 272, 0, (MetaName)16777216),
        public uint[] iCutsceneFlags { get; set; } //iCutsceneFlags, PsoDataType.Array, 528, 4, (MetaName)262146),
        public Vector3 vOffset { get; set; } //vOffset, PsoDataType.Float3, 544, 0, 0),
        public float fRotation { get; set; } //fRotation, PsoDataType.Float, 560, 0, 0),
        public Vector3 vTriggerOffset { get; set; } //vTriggerOffset, PsoDataType.Float3, 576, 0, 0),
        public object[] pCutsceneObjects { get; set; } //pCutsceneObjects, PsoDataType.Array, 592, 0, (MetaName)7),
        public object[] pCutsceneLoadEventList { get; set; } //pCutsceneLoadEventList, PsoDataType.Array, 608, 0, (MetaName)9),
        public object[] pCutsceneEventList { get; set; } //pCutsceneEventList, PsoDataType.Array, 624, 0, (MetaName)11),
        public object[] pCutsceneEventArgsList { get; set; } //pCutsceneEventArgsList, PsoDataType.Array, 640, 0, (MetaName)13),
        public CutParAttributeList attributes { get; set; } //attributes, PsoDataType.Structure, 656, 0, MetaName.rage__parAttributeList),
        public CutFAttributeList cutfAttributes { get; set; } //cutfAttributes, PsoDataType.Structure, 672, 4, 0),
        public int iRangeStart { get; set; } //iRangeStart, PsoDataType.SInt, 680, 0, 0),
        public int iRangeEnd { get; set; } //iRangeEnd, PsoDataType.SInt, 684, 0, 0),
        public int iAltRangeEnd { get; set; } //iAltRangeEnd, PsoDataType.SInt, 688, 0, 0),
        public float fSectionByTimeSliceDuration { get; set; } //fSectionByTimeSliceDuration, PsoDataType.Float, 692, 0, 0),
        public float fFadeOutCutsceneDuration { get; set; } //fFadeOutCutsceneDuration, PsoDataType.Float, 696, 0, 0),
        public float fFadeInGameDuration { get; set; } //fFadeInGameDuration, PsoDataType.Float, 700, 0, 0),
        public uint fadeInColor { get; set; } //fadeInColor, PsoDataType.UInt, 704, 1, 0),
        public int iBlendOutCutsceneDuration { get; set; } //iBlendOutCutsceneDuration, PsoDataType.SInt, 708, 0, 0),
        public int iBlendOutCutsceneOffset { get; set; } //iBlendOutCutsceneOffset, PsoDataType.SInt, 712, 0, 0),
        public float fFadeOutGameDuration { get; set; } //fFadeOutGameDuration, PsoDataType.Float, 716, 0, 0),
        public float fFadeInCutsceneDuration { get; set; } //fFadeInCutsceneDuration, PsoDataType.Float, 720, 0, 0),
        public uint fadeOutColor { get; set; } //fadeOutColor, PsoDataType.UInt, 724, 1, 0),
        public uint DayCoCHours { get; set; } //DayCoCHours, PsoDataType.UInt, 728, 0, 0),
        public float[] cameraCutList { get; set; } //cameraCutList, PsoDataType.Array, 736, 0, (MetaName)30),
        public float[] sectionSplitList { get; set; } //sectionSplitList, PsoDataType.Array, 752, 0, (MetaName)32),
        public CutConcatData[] concatDataList { get; set; } //concatDataList, PsoDataType.Array, 768, 1, (MetaName)2621474),
        public CutHaltFrequency[] discardFrameList { get; set; } //discardFrameList, PsoDataType.Array, 3344, 0, (MetaName)36)



        public Dictionary<int, CutObject> ObjectsDict { get; set; } = new Dictionary<int, CutObject>();



        public override void ReadXml(XmlNode node)
        {
            fTotalDuration = Xml.GetChildFloatAttribute(node, "fTotalDuration", "value");
            cFaceDir = Xml.GetChildInnerText(node, "cFaceDir");
            iCutsceneFlags = Xml.GetChildRawUintArray(node, "iCutsceneFlags");
            vOffset = Xml.GetChildVector3Attributes(node, "vOffset");
            fRotation = Xml.GetChildFloatAttribute(node, "fRotation", "value");
            vTriggerOffset = Xml.GetChildVector3Attributes(node, "vTriggerOffset");
            pCutsceneObjects = ReadObjectArray(node, "pCutsceneObjects");
            pCutsceneLoadEventList = ReadObjectArray(node, "pCutsceneLoadEventList");
            pCutsceneEventList = ReadObjectArray(node, "pCutsceneEventList");
            pCutsceneEventArgsList = ReadObjectArray(node, "pCutsceneEventArgsList");
            attributes = ReadObject<CutParAttributeList>(node, "attributes");
            cutfAttributes = ReadObject<CutFAttributeList>(node, "cutfAttributes");
            iRangeStart = Xml.GetChildIntAttribute(node, "iRangeStart", "value");
            iRangeEnd = Xml.GetChildIntAttribute(node, "iRangeEnd", "value");
            iAltRangeEnd = Xml.GetChildIntAttribute(node, "iAltRangeEnd", "value");
            fSectionByTimeSliceDuration = Xml.GetChildFloatAttribute(node, "fSectionByTimeSliceDuration", "value");
            fFadeOutCutsceneDuration = Xml.GetChildFloatAttribute(node, "fFadeOutCutsceneDuration", "value");
            fFadeInGameDuration = Xml.GetChildFloatAttribute(node, "fFadeInGameDuration", "value");
            fadeInColor = Xml.GetChildUIntAttribute(node, "fadeInColor", "value");
            iBlendOutCutsceneDuration = Xml.GetChildIntAttribute(node, "iBlendOutCutsceneDuration", "value");
            iBlendOutCutsceneOffset = Xml.GetChildIntAttribute(node, "iBlendOutCutsceneOffset", "value");
            fFadeOutGameDuration = Xml.GetChildFloatAttribute(node, "fFadeOutGameDuration", "value");
            fFadeInCutsceneDuration = Xml.GetChildFloatAttribute(node, "fFadeInCutsceneDuration", "value");
            fadeOutColor = Xml.GetChildUIntAttribute(node, "fadeOutColor", "value");
            DayCoCHours = Xml.GetChildUIntAttribute(node, "DayCoCHours", "value");
            cameraCutList = Xml.GetChildRawFloatArray(node, "cameraCutList");
            sectionSplitList = Xml.GetChildRawFloatArray(node, "sectionSplitList");
            concatDataList = XmlMeta.ReadItemArrayNullable<CutConcatData>(node, "concatDataList");
            discardFrameList = XmlMeta.ReadItemArrayNullable<CutHaltFrequency>(node, "discardFrameList");

            AssociateObjects();
        }


        public void AssociateObjects()
        {
            ObjectsDict.Clear();
            if (pCutsceneObjects != null)
            {
                foreach (var obj in pCutsceneObjects)
                {
                    if (obj is CutObject cobj)
                    {
                        ObjectsDict[cobj.iObjectId] = cobj;
                    }
                }
            }


            CutEventArgs getEventArgs(int i)
            {
                if (i < 0) return null;
                if (i >= pCutsceneEventArgsList?.Length) return null;
                var args = pCutsceneEventArgsList[i];
                if (!(args is CutEventArgs))
                { }
                return args as CutEventArgs;
            }
            CutObject getObject(int i)
            {
                CutObject o = null;
                ObjectsDict.TryGetValue(i, out o);
                return o;
            }

            if (pCutsceneEventArgsList != null)
            {
                foreach (var arg in pCutsceneEventArgsList)
                {
                    if (arg is CutObjectIdEventArgs oarg)
                    {
                        oarg.Object = getObject(oarg.iObjectId);
                    }
                    if (arg is CutObjectIdListEventArgs larg)
                    {
                        var objs = new CutObject[larg.iObjectIdList?.Length ?? 0];
                        for (int i = 0; i < objs.Length; i++)
                        {
                            objs[i] = getObject(larg.iObjectIdList[i]);
                        }
                        larg.ObjectList = objs;
                    }
                }
            }
            if (pCutsceneEventList != null)
            {
                foreach (var evt in pCutsceneEventList)
                {
                    if (evt is CutObjectIdEvent oevt)
                    {
                        oevt.Object = getObject(oevt.iObjectId);
                    }
                    if (evt is CutEvent cevt)
                    {
                        cevt.EventArgs = getEventArgs(cevt.iEventArgsIndex);
                    }
                    else
                    { }
                }
            }
            if (pCutsceneLoadEventList != null)
            {
                foreach (var evt in pCutsceneLoadEventList)
                {
                    if (evt is CutObjectIdEvent oevt)
                    {
                        oevt.Object = getObject(oevt.iObjectId);
                    }
                    if (evt is CutEvent cevt)
                    {
                        cevt.EventArgs = getEventArgs(cevt.iEventArgsIndex);
                    }
                    else
                    { }
                }
            }

        }



        public static CutBase ConstructObject(string type)
        {
            switch (type)
            {
                case "rage__cutfAssetManagerObject": return new CutAssetManagerObject();
                case "rage__cutfAnimationManagerObject": return new CutAnimationManagerObject();
                case "rage__cutfCameraObject": return new CutCameraObject();
                case "rage__cutfPedModelObject": return new CutPedModelObject();
                case "rage__cutfPropModelObject": return new CutPropModelObject();
                case "rage__cutfBlockingBoundsObject": return new CutBlockingBoundsObject();
                case "rage__cutfAudioObject": return new CutAudioObject();
                case "rage__cutfHiddenModelObject": return new CutHiddenModelObject();
                case "rage__cutfOverlayObject": return new CutOverlayObject();
                case "rage__cutfSubtitleObject": return new CutSubtitleObject();
                case "rage__cutfLightObject": return new CutLightObject();
                case "rage__cutfAnimatedLightObject": return new CutAnimatedLightObject();
                case "rage__cutfObjectIdEvent": return new CutObjectIdEvent();
                case "rage__cutfObjectVariationEventArgs": return new CutObjectVariationEventArgs();
                case "rage__cutfEventArgs": return new CutEventArgs();
                case "rage__cutfLoadSceneEventArgs": return new CutLoadSceneEventArgs();
                case "rage__cutfObjectIdEventArgs": return new CutObjectIdEventArgs();
                case "rage__cutfObjectIdListEventArgs": return new CutObjectIdListEventArgs();
                case "rage__cutfNameEventArgs": return new CutNameEventArgs();
                case "rage__cutfCameraCutEventArgs": return new CutCameraCutEventArgs();
                case "rage__cutfSubtitleEventArgs": return new CutSubtitleEventArgs();
                case "rage__cutfFinalNameEventArgs": return new CutFinalNameEventArgs();
                case "rage__cutfObjectIdNameEventArgs": return new CutObjectIdNameEventArgs();
                case "rage__cutfVehicleModelObject": return new CutVehicleModelObject();
                case "rage__cutfEvent": return new CutEvent();
                case "rage__cutfCascadeShadowEventArgs": return new CutCascadeShadowEventArgs();
                case "rage__cutfFloatValueEventArgs": return new CutFloatValueEventArgs();
                case "rage__cutfAnimatedParticleEffectObject": return new CutAnimatedParticleEffectObject();
                case "rage__cutfWeaponModelObject": return new CutWeaponModelObject();
                case "rage__cutfPlayParticleEffectEventArgs": return new CutPlayParticleEffectEventArgs();
                case "rage__cutfBoolValueEventArgs": return new CutBoolValueEventArgs();
                case "rage__cutfRayfireObject": return new CutRayfireObject();
                case "rage__cutfParticleEffectObject": return new CutParticleEffectObject();
                case "rage__cutfDecalObject": return new CutDecalObject();
                case "rage__cutfDecalEventArgs": return new CutDecalEventArgs();
                case "rage__cutfScreenFadeObject": return new CutScreenFadeObject();
                case "rage__cutfVehicleVariationEventArgs": return new CutVehicleVariationEventArgs();
                case "rage__cutfScreenFadeEventArgs": return new CutScreenFadeEventArgs();
                case "rage__cutfTriggerLightEffectEventArgs": return new CutTriggerLightEffectEventArgs();
                case "rage__cutfVehicleExtraEventArgs": return new CutVehicleExtraEventArgs();
                case "rage__cutfFixupModelObject": return new CutFixupModelObject();
                case "cutf_float": return new CutFloat();
                case "cutf_int": return new CutInt();
                case "cutf_string": return new CutString();
                default: return null;
            }
        }
        public static T ReadObject<T>(XmlNode node, string name) where T : IMetaXmlItem, new()
        {
            var onode = node.SelectSingleNode(name);
            if (onode != null)
            {
                var o = new T();
                o.ReadXml(onode);
                return o;
            }
            return default(T);
        }
        public static object[] ReadObjectArray(XmlNode node, string name)
        {
            var aNode = node.SelectSingleNode(name);
            if (aNode != null)
            {
                var inodes = aNode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    var oList = new List<object>();
                    foreach (XmlNode inode in inodes)
                    {
                        var type = Xml.GetStringAttribute(inode, "type");
                        var o = ConstructObject(type);
                        o.ReadXml(inode);
                        oList.Add(o);
                    }
                    return oList.ToArray();
                }
            }
            return null;
        }

    }


    [TC(typeof(EXP))] public class CutParAttributeList : CutBase  // rage__parAttributeList
    {
        public byte UserData1 { get; set; } // PsoDataType.UByte, 8, 0, 0),
        public byte UserData2 { get; set; } // PsoDataType.UByte, 9, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            UserData1 = (byte)Xml.GetChildUIntAttribute(node, "UserData1", "value");
            UserData2 = (byte)Xml.GetChildUIntAttribute(node, "UserData2", "value");

            if ((UserData1 != 0) || (UserData2 != 0))
            { }
        }

        public override string ToString()
        {
            return UserData1.ToString() + ", " + UserData2.ToString();
        }
    }

    [TC(typeof(EXP))] public class CutFAttributeList : CutBase  // rage__cutfAttributeList
    {
        public object[] Items { get; set; } // PsoDataType.Array, 0, 0, 0)//ARRAYINFO, PsoDataType.Structure, 0, 3, 0),

        public override void ReadXml(XmlNode node)
        {
            Items = CutsceneFile2.ReadObjectArray(node, "Items");

            if (Items?.Length > 0)
            { }
        }

        public override string ToString()
        {
            return (Items?.Length ?? 0).ToString() + " items";
        }
    }
    [TC(typeof(EXP))] public class CutInt : CutBase
    {
        public MetaHash Name { get; set; } // PsoDataType.String, 8, 8, 0),
        public int Value { get; set; } // PsoDataType.SInt, 16, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
            Value = Xml.GetChildIntAttribute(node, "Value", "value");
        }

        public override string ToString()
        {
            return Name.ToString() + ": " + Value.ToString();
        }
    }
    [TC(typeof(EXP))] public class CutFloat : CutBase
    {
        public MetaHash Name { get; set; } // PsoDataType.String, 8, 8, 0),
        public float Value { get; set; } // PsoDataType.Float, 16, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
            Value = Xml.GetChildFloatAttribute(node, "Value", "value");
        }

        public override string ToString()
        {
            return Name.ToString() + ": " + Value.ToString();
        }
    }
    [TC(typeof(EXP))] public class CutString : CutBase
    {
        public MetaHash Name { get; set; } // PsoDataType.String, 8, 8, 0),
        public string Value { get; set; } // PsoDataType.String, 16, 3, 0)

        public override void ReadXml(XmlNode node)
        {
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
            Value = Xml.GetChildInnerText(node, "Value");
        }

        public override string ToString()
        {
            return Name.ToString() + ": " + Value.ToString();
        }
    }

    [TC(typeof(EXP))] public class CutConcatData : CutBase  // rage__cutfCutsceneFile2__SConcatData
    {
        public MetaHash cSceneName { get; set; } // PsoDataType.String, 0, 7, 0),
        public Vector3 vOffset { get; set; } // PsoDataType.Float3, 16, 0, 0),
        public float fStartTime { get; set; } // PsoDataType.Float, 32, 0, 0),
        public float fRotation { get; set; } // PsoDataType.Float, 36, 0, 0),
        public float fPitch { get; set; } // PsoDataType.Float, 40, 0, 0),
        public float fRoll { get; set; } // PsoDataType.Float, 44, 0, 0),
        public int iRangeStart { get; set; } // PsoDataType.SInt, 48, 0, 0),
        public int iRangeEnd { get; set; } // PsoDataType.SInt, 52, 0, 0),
        public bool bValidForPlayBack { get; set; } // PsoDataType.Bool, 56, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            cSceneName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cSceneName"));
            vOffset = Xml.GetChildVector3Attributes(node, "vOffset");
            fStartTime = Xml.GetChildFloatAttribute(node, "fStartTime", "value");
            fRotation = Xml.GetChildFloatAttribute(node, "fRotation", "value");
            fPitch = Xml.GetChildFloatAttribute(node, "fPitch", "value");
            fRoll = Xml.GetChildFloatAttribute(node, "fRoll", "value");
            iRangeStart = Xml.GetChildIntAttribute(node, "iRangeStart", "value");
            iRangeEnd = Xml.GetChildIntAttribute(node, "iRangeEnd", "value");
            bValidForPlayBack = Xml.GetChildBoolAttribute(node, "bValidForPlayBack", "value");
        }
    }

    [TC(typeof(EXP))] public class CutHaltFrequency : CutBase  // vHaltFrequency
    {
        public MetaHash cSceneName { get; set; } // PsoDataType.String, 0, 7, 0),
        public int[] frames { get; set; } // PsoDataType.Array, 8, 0, (MetaName)1)//ARRAYINFO, PsoDataType.SInt, 0, 0, 0),

        public override void ReadXml(XmlNode node)
        {
            cSceneName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cSceneName"));
            frames = Xml.GetChildRawIntArray(node, "frames");
        }
    }


    [TC(typeof(EXP))] public abstract class CutObject : CutBase
    {
        public int iObjectId { get; set; } // PsoDataType.SInt, 8, 0, 0),
        public CutParAttributeList attributeList { get; set; } // PsoDataType.Structure, 20, 0, MetaName.rage__parAttributeList),
        public CutFAttributeList cutfAttributes { get; set; } // PsoDataType.Structure, 32, 4, 0)

        public override void ReadXml(XmlNode node)
        {
            iObjectId = Xml.GetChildIntAttribute(node, "iObjectId", "value");
            attributeList = CutsceneFile2.ReadObject<CutParAttributeList>(node, "attributeList"); //might also be called "attributes" ?
            cutfAttributes = CutsceneFile2.ReadObject<CutFAttributeList>(node, "cutfAttributes");
        }

        public override string ToString()
        {
            return iObjectId.ToString() + ": " + base.ToString().Replace("CodeWalker.GameFiles.Cut", "");
        }
    }
    [TC(typeof(EXP))] public class CutAssetManagerObject : CutObject  // rage__cutfAssetManagerObject
    {
    }
    [TC(typeof(EXP))] public class CutAnimationManagerObject : CutObject  // rage__cutfAnimationManagerObject
    {
    }
    [TC(typeof(EXP))] public abstract class CutNamedObject : CutObject
    {
        public MetaHash cName { get; set; } // PsoDataType.String, 40, 7, 0),

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            cName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cName"));
        }

        public override string ToString()
        {
            return base.ToString() + ": " + cName.ToString();
        }
    }
    [TC(typeof(EXP))] public class CutCameraObject : CutNamedObject  // rage__cutfCameraObject
    {
        public uint AnimStreamingBase { get; set; } // PsoDataType.UInt, 48, 0, 0),
        public float fNearDrawDistance { get; set; } // PsoDataType.Float, 56, 0, 0),
        public float fFarDrawDistance { get; set; } // PsoDataType.Float, 60, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            AnimStreamingBase = Xml.GetChildUIntAttribute(node, "AnimStreamingBase", "value");
            fNearDrawDistance = Xml.GetChildFloatAttribute(node, "fNearDrawDistance", "value");
            fFarDrawDistance = Xml.GetChildFloatAttribute(node, "fFarDrawDistance", "value");
        }
    }
    [TC(typeof(EXP))] public class CutPedModelObject : CutNamedObject  // rage__cutfPedModelObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public uint AnimStreamingBase { get; set; } // PsoDataType.UInt, 56, 0, 0),
        public MetaHash cAnimExportCtrlSpecFile { get; set; } // PsoDataType.String, 64, 7, 0),
        public MetaHash cFaceExportCtrlSpecFile { get; set; } // PsoDataType.String, 68, 7, 0),
        public MetaHash cAnimCompressionFile { get; set; } // PsoDataType.String, 72, 7, 0),
        public MetaHash cHandle { get; set; } // PsoDataType.String, 84, 7, 0),
        public uint Unk_673165049 { get; set; } // PsoDataType.UInt, 352, 0, 0),
        public MetaHash typeFile { get; set; } // PsoDataType.String, 88, 7, 0),
        public MetaHash overrideFaceAnimationFilename { get; set; } // PsoDataType.String, 96, 7, 0),
        public bool bFoundFaceAnimation { get; set; } // PsoDataType.Bool, 104, 0, 0),
        public bool bFaceAndBodyAreMerged { get; set; } // PsoDataType.Bool, 105, 0, 0),
        public bool bOverrideFaceAnimation { get; set; } // PsoDataType.Bool, 106, 0, 0),
        public MetaHash faceAnimationNodeName { get; set; } // PsoDataType.String, 108, 7, 0),
        public MetaHash faceAttributesFilename { get; set; } // PsoDataType.String, 112, 7, 0)


        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            StreamingName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "StreamingName"));
            AnimStreamingBase = Xml.GetChildUIntAttribute(node, "AnimStreamingBase", "value");
            cAnimExportCtrlSpecFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cAnimExportCtrlSpecFile"));
            cFaceExportCtrlSpecFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cFaceExportCtrlSpecFile"));
            cAnimCompressionFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cAnimCompressionFile"));
            cHandle = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cHandle"));
            Unk_673165049 = Xml.GetChildUIntAttribute(node, "hash_281FAEF9", "value");
            typeFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "typeFile"));
            overrideFaceAnimationFilename = XmlMeta.GetHash(Xml.GetChildInnerText(node, "overrideFaceAnimationFilename"));
            bFoundFaceAnimation = Xml.GetChildBoolAttribute(node, "bFoundFaceAnimation", "value");
            bFaceAndBodyAreMerged = Xml.GetChildBoolAttribute(node, "bFaceAndBodyAreMerged", "value");
            bOverrideFaceAnimation = Xml.GetChildBoolAttribute(node, "bOverrideFaceAnimation", "value");
            faceAnimationNodeName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "faceAnimationNodeName"));
            faceAttributesFilename = XmlMeta.GetHash(Xml.GetChildInnerText(node, "faceAttributesFilename"));
        }
    }
    [TC(typeof(EXP))] public class CutPropModelObject : CutNamedObject  // rage__cutfPropModelObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public uint AnimStreamingBase { get; set; } // PsoDataType.UInt, 56, 0, 0),
        public MetaHash cAnimExportCtrlSpecFile { get; set; } // PsoDataType.String, 64, 7, 0),
        public MetaHash cFaceExportCtrlSpecFile { get; set; } // PsoDataType.String, 68, 7, 0),
        public MetaHash cAnimCompressionFile { get; set; } // PsoDataType.String, 72, 7, 0),
        public MetaHash cHandle { get; set; } // PsoDataType.String, 84, 7, 0),
        public MetaHash typeFile { get; set; } // PsoDataType.String, 88, 7, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            StreamingName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "StreamingName"));
            AnimStreamingBase = Xml.GetChildUIntAttribute(node, "AnimStreamingBase", "value");
            cAnimExportCtrlSpecFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cAnimExportCtrlSpecFile"));
            cFaceExportCtrlSpecFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cFaceExportCtrlSpecFile"));
            cAnimCompressionFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cAnimCompressionFile"));
            cHandle = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cHandle"));
            typeFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "typeFile"));
        }
    }
    [TC(typeof(EXP))] public class CutBlockingBoundsObject : CutNamedObject  // rage__cutfBlockingBoundsObject
    {
        public Vector3[] vCorners { get; set; } // PsoDataType.Array, 48, 4, (MetaName)262148),//ARRAYINFO, PsoDataType.Float3, 0, 0, 0),
        public float fHeight { get; set; } // PsoDataType.Float, 112, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            vCorners = Xml.GetChildRawVector3Array(node, "vCorners");
            fHeight = Xml.GetChildFloatAttribute(node, "fHeight", "value");
        }
    }
    [TC(typeof(EXP))] public class CutAudioObject : CutNamedObject  // rage__cutfAudioObject
    {
        public float fOffset { get; set; } // PsoDataType.Float, 56, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            fOffset = Xml.GetChildFloatAttribute(node, "fOffset", "value");
        }
    }
    [TC(typeof(EXP))] public class CutHiddenModelObject : CutNamedObject  // rage__cutfHiddenModelObject
    {
        public Vector3 vPosition { get; set; } // PsoDataType.Float3, 48, 0, 0),
        public float fRadius { get; set; } // PsoDataType.Float, 64, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            vPosition = Xml.GetChildVector3Attributes(node, "vPosition");
            fRadius = Xml.GetChildFloatAttribute(node, "fRadius", "value");
        }
    }
    [TC(typeof(EXP))] public class CutOverlayObject : CutNamedObject  // rage__cutfOverlayObject
    {
        public string cRenderTargetName { get; set; } // PsoDataType.String, 56, 3, 0),
        public uint iOverlayType { get; set; } // PsoDataType.UInt, 72, 0, 0),
        public MetaHash modelHashName { get; set; } // PsoDataType.String, 76, 7, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            cRenderTargetName = Xml.GetChildInnerText(node, "cRenderTargetName");
            iOverlayType = Xml.GetChildUIntAttribute(node, "iOverlayType", "value");
            modelHashName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "modelHashName"));
        }
    }
    [TC(typeof(EXP))] public class CutSubtitleObject : CutNamedObject  // rage__cutfSubtitleObject
    {
    }
    [TC(typeof(EXP))] public class CutLightObject : CutNamedObject  // rage__cutfLightObject
    {
        public Vector3 vDirection { get; set; } // PsoDataType.Float3, 64, 0, 0),
        public Vector3 vColour { get; set; } // PsoDataType.Float3, 80, 0, 0),
        public Vector3 vPosition { get; set; } // PsoDataType.Float3, 96, 0, 0),
        public float fIntensity { get; set; } // PsoDataType.Float, 112, 0, 0),
        public float fFallOff { get; set; } // PsoDataType.Float, 116, 0, 0),
        public float fConeAngle { get; set; } // PsoDataType.Float, 120, 0, 0),
        public float fVolumeIntensity { get; set; } // PsoDataType.Float, 124, 0, 0),
        public float fVolumeSizeScale { get; set; } // PsoDataType.Float, 128, 0, 0),
        public float fCoronaSize { get; set; } // PsoDataType.Float, 132, 0, 0),
        public float fCoronaIntensity { get; set; } // PsoDataType.Float, 136, 0, 0),
        public float fCoronaZBias { get; set; } // PsoDataType.Float, 140, 0, 0),
        public float fInnerConeAngle { get; set; } // PsoDataType.Float, 144, 0, 0),
        public float fExponentialFallOff { get; set; } // PsoDataType.Float, 148, 0, 0),
        public float fShadowBlur { get; set; } // PsoDataType.Float, 152, 0, 0),
        public int iLightType { get; set; } // PsoDataType.SInt, 156, 0, 0),
        public int iLightProperty { get; set; } // PsoDataType.SInt, 160, 0, 0),
        public int TextureDictID { get; set; } // PsoDataType.SInt, 164, 0, 0),
        public int TextureKey { get; set; } // PsoDataType.SInt, 168, 0, 0),
        public uint uLightFlags { get; set; } // PsoDataType.UInt, 176, 0, 0),
        public uint uHourFlags { get; set; } // PsoDataType.UInt, 180, 0, 0),
        public bool bStatic { get; set; } // PsoDataType.Bool, 186, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            vDirection = Xml.GetChildVector3Attributes(node, "vDirection");
            vColour = Xml.GetChildVector3Attributes(node, "vColour");
            vPosition = Xml.GetChildVector3Attributes(node, "vPosition");
            fIntensity = Xml.GetChildFloatAttribute(node, "fIntensity", "value");
            fFallOff = Xml.GetChildFloatAttribute(node, "fFallOff", "value");
            fConeAngle = Xml.GetChildFloatAttribute(node, "fConeAngle", "value");
            fVolumeIntensity = Xml.GetChildFloatAttribute(node, "fVolumeIntensity", "value");
            fVolumeSizeScale = Xml.GetChildFloatAttribute(node, "fVolumeSizeScale", "value");
            fCoronaSize = Xml.GetChildFloatAttribute(node, "fCoronaSize", "value");
            fCoronaIntensity = Xml.GetChildFloatAttribute(node, "fCoronaIntensity", "value");
            fCoronaZBias = Xml.GetChildFloatAttribute(node, "fCoronaZBias", "value");
            fInnerConeAngle = Xml.GetChildFloatAttribute(node, "fInnerConeAngle", "value");
            fExponentialFallOff = Xml.GetChildFloatAttribute(node, "fExponentialFallOff", "value");
            fShadowBlur = Xml.GetChildFloatAttribute(node, "fShadowBlur", "value");
            iLightType = Xml.GetChildIntAttribute(node, "iLightType", "value");
            iLightProperty = Xml.GetChildIntAttribute(node, "iLightProperty", "value");
            TextureDictID = Xml.GetChildIntAttribute(node, "TextureDictID", "value");
            TextureKey = Xml.GetChildIntAttribute(node, "TextureKey", "value");
            uLightFlags = Xml.GetChildUIntAttribute(node, "uLightFlags", "value");
            uHourFlags = Xml.GetChildUIntAttribute(node, "uHourFlags", "value");
            bStatic = Xml.GetChildBoolAttribute(node, "bStatic", "value");
        }
    }
    [TC(typeof(EXP))] public class CutAnimatedLightObject : CutNamedObject  // rage__cutfAnimatedLightObject
    {
        public Vector3 vDirection { get; set; } // PsoDataType.Float3, 64, 0, 0),
        public Vector3 vColour { get; set; } // PsoDataType.Float3, 80, 0, 0),
        public Vector3 vPosition { get; set; } // PsoDataType.Float3, 96, 0, 0),
        public float fIntensity { get; set; } // PsoDataType.Float, 112, 0, 0),
        public float fFallOff { get; set; } // PsoDataType.Float, 116, 0, 0),
        public float fConeAngle { get; set; } // PsoDataType.Float, 120, 0, 0),
        public float fVolumeIntensity { get; set; } // PsoDataType.Float, 124, 0, 0),
        public float fVolumeSizeScale { get; set; } // PsoDataType.Float, 128, 0, 0),
        public float fCoronaSize { get; set; } // PsoDataType.Float, 132, 0, 0),
        public float fCoronaIntensity { get; set; } // PsoDataType.Float, 136, 0, 0),
        public float fCoronaZBias { get; set; } // PsoDataType.Float, 140, 0, 0),
        public float fInnerConeAngle { get; set; } // PsoDataType.Float, 144, 0, 0),
        public float fExponentialFallOff { get; set; } // PsoDataType.Float, 148, 0, 0),
        public float fShadowBlur { get; set; } // PsoDataType.Float, 152, 0, 0),
        public int iLightType { get; set; } // PsoDataType.SInt, 156, 0, 0),
        public int iLightProperty { get; set; } // PsoDataType.SInt, 160, 0, 0),
        public int TextureDictID { get; set; } // PsoDataType.SInt, 164, 0, 0),
        public int TextureKey { get; set; } // PsoDataType.SInt, 168, 0, 0),
        //public int Unk_34975788 { get; set; } // PsoDataType.SInt, 216, 0, 0),
        public uint uLightFlags { get; set; } // PsoDataType.UInt, 176, 0, 0),
        public uint uHourFlags { get; set; } // PsoDataType.UInt, 180, 0, 0),
        //public ushort Unk_1437992521 { get; set; } // PsoDataType.UShort, 228, 0, 0),
        public bool bStatic { get; set; } // PsoDataType.Bool, 186, 0, 0),
        public uint AnimStreamingBase { get; set; } // PsoDataType.UInt, 192, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            vDirection = Xml.GetChildVector3Attributes(node, "vDirection");
            vColour = Xml.GetChildVector3Attributes(node, "vColour");
            vPosition = Xml.GetChildVector3Attributes(node, "vPosition");
            fIntensity = Xml.GetChildFloatAttribute(node, "fIntensity", "value");
            fFallOff = Xml.GetChildFloatAttribute(node, "fFallOff", "value");
            fConeAngle = Xml.GetChildFloatAttribute(node, "fConeAngle", "value");
            fVolumeIntensity = Xml.GetChildFloatAttribute(node, "fVolumeIntensity", "value");
            fVolumeSizeScale = Xml.GetChildFloatAttribute(node, "fVolumeSizeScale", "value");
            fCoronaSize = Xml.GetChildFloatAttribute(node, "fCoronaSize", "value");
            fCoronaIntensity = Xml.GetChildFloatAttribute(node, "fCoronaIntensity", "value");
            fCoronaZBias = Xml.GetChildFloatAttribute(node, "fCoronaZBias", "value");
            fInnerConeAngle = Xml.GetChildFloatAttribute(node, "fInnerConeAngle", "value");
            fExponentialFallOff = Xml.GetChildFloatAttribute(node, "fExponentialFallOff", "value");
            fShadowBlur = Xml.GetChildFloatAttribute(node, "fShadowBlur", "value");
            iLightType = Xml.GetChildIntAttribute(node, "iLightType", "value");
            iLightProperty = Xml.GetChildIntAttribute(node, "iLightProperty", "value");
            TextureDictID = Xml.GetChildIntAttribute(node, "TextureDictID", "value");
            TextureKey = Xml.GetChildIntAttribute(node, "TextureKey", "value");
            //Unk_34975788 = Xml.GetChildIntAttribute(node, "hash_0215B02C", "value");
            uLightFlags = Xml.GetChildUIntAttribute(node, "uLightFlags", "value");
            uHourFlags = Xml.GetChildUIntAttribute(node, "uHourFlags", "value");
            //Unk_1437992521 = (ushort)Xml.GetChildUIntAttribute(node, "hash_55B60649", "value");
            bStatic = Xml.GetChildBoolAttribute(node, "bStatic", "value");
            AnimStreamingBase = Xml.GetChildUIntAttribute(node, "AnimStreamingBase", "value");
        }
    }
    [TC(typeof(EXP))] public class CutVehicleModelObject : CutNamedObject  // rage__cutfVehicleModelObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public uint AnimStreamingBase { get; set; } // PsoDataType.UInt, 56, 0, 0),
        public MetaHash cAnimExportCtrlSpecFile { get; set; } // PsoDataType.String, 64, 7, 0),
        public MetaHash cFaceExportCtrlSpecFile { get; set; } // PsoDataType.String, 68, 7, 0),
        public MetaHash cAnimCompressionFile { get; set; } // PsoDataType.String, 72, 7, 0),
        public MetaHash cHandle { get; set; } // PsoDataType.String, 84, 7, 0),
        public MetaHash typeFile { get; set; } // PsoDataType.String, 88, 7, 0),
        public string[] cRemoveBoneNameList { get; set; } // PsoDataType.Array, 96, 0, (MetaName)11),//ARRAYINFO, PsoDataType.String, 0, 3, 0),
        public bool bCanApplyRealDamage { get; set; } // PsoDataType.Bool, 112, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            StreamingName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "StreamingName"));
            AnimStreamingBase = Xml.GetChildUIntAttribute(node, "AnimStreamingBase", "value");
            cAnimExportCtrlSpecFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cAnimExportCtrlSpecFile"));
            cFaceExportCtrlSpecFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cFaceExportCtrlSpecFile"));
            cAnimCompressionFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cAnimCompressionFile"));
            cHandle = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cHandle"));
            typeFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "typeFile"));
            cRemoveBoneNameList = XmlMeta.ReadStringItemArray(node, "cRemoveBoneNameList");
            bCanApplyRealDamage = Xml.GetChildBoolAttribute(node, "bCanApplyRealDamage", "value");
        }
    }
    [TC(typeof(EXP))] public class CutWeaponModelObject : CutNamedObject  // rage__cutfWeaponModelObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public uint AnimStreamingBase { get; set; } // PsoDataType.UInt, 56, 0, 0),
        public MetaHash cAnimExportCtrlSpecFile { get; set; } // PsoDataType.String, 64, 7, 0),
        public MetaHash cFaceExportCtrlSpecFile { get; set; } // PsoDataType.String, 68, 7, 0),
        public MetaHash cAnimCompressionFile { get; set; } // PsoDataType.String, 72, 7, 0),
        public MetaHash cHandle { get; set; } // PsoDataType.String, 84, 7, 0),
        public MetaHash typeFile { get; set; } // PsoDataType.String, 88, 7, 0),
        public uint GenericWeaponType { get; set; } // PsoDataType.UInt, 96, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            StreamingName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "StreamingName"));
            AnimStreamingBase = Xml.GetChildUIntAttribute(node, "AnimStreamingBase", "value");
            cAnimExportCtrlSpecFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cAnimExportCtrlSpecFile"));
            cFaceExportCtrlSpecFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cFaceExportCtrlSpecFile"));
            cAnimCompressionFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cAnimCompressionFile"));
            cHandle = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cHandle"));
            typeFile = XmlMeta.GetHash(Xml.GetChildInnerText(node, "typeFile"));
            GenericWeaponType = Xml.GetChildUIntAttribute(node, "GenericWeaponType", "value");
        }
    }
    [TC(typeof(EXP))] public class CutRayfireObject : CutNamedObject  // rage__cutfRayfireObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public Vector3 vStartPosition { get; set; } // PsoDataType.Float3, 64, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            StreamingName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "StreamingName"));
            vStartPosition = Xml.GetChildVector3Attributes(node, "vStartPosition");
        }
    }
    [TC(typeof(EXP))] public class CutParticleEffectObject : CutNamedObject  // rage__cutfParticleEffectObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public MetaHash athFxListHash { get; set; } // PsoDataType.String, 56, 7, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            StreamingName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "StreamingName"));
            athFxListHash = XmlMeta.GetHash(Xml.GetChildInnerText(node, "athFxListHash"));
        }
    }
    [TC(typeof(EXP))] public class CutAnimatedParticleEffectObject : CutNamedObject  // rage__cutfAnimatedParticleEffectObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public uint AnimStreamingBase { get; set; } // PsoDataType.UInt, 56, 0, 0),
        public MetaHash athFxListHash { get; set; } // PsoDataType.String, 64, 7, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            StreamingName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "StreamingName"));
            AnimStreamingBase = Xml.GetChildUIntAttribute(node, "AnimStreamingBase", "value");
            athFxListHash = XmlMeta.GetHash(Xml.GetChildInnerText(node, "athFxListHash"));
        }
    }
    [TC(typeof(EXP))] public class CutDecalObject : CutNamedObject  // rage__cutfDecalObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public uint RenderId { get; set; } // PsoDataType.UInt, 56, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            StreamingName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "StreamingName"));
            RenderId = Xml.GetChildUIntAttribute(node, "RenderId", "value");
        }
    }
    [TC(typeof(EXP))] public class CutScreenFadeObject : CutNamedObject  // rage__cutfScreenFadeObject
    {
    }
    [TC(typeof(EXP))] public class CutFixupModelObject : CutNamedObject  // rage__cutfFixupModelObject
    {
        public Vector3 vPosition { get; set; } // PsoDataType.Float3, 48, 0, 0),
        public float fRadius { get; set; } // PsoDataType.Float, 64, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            vPosition = Xml.GetChildVector3Attributes(node, "vPosition");
            fRadius = Xml.GetChildFloatAttribute(node, "fRadius", "value");
        }
    }


    public enum CutEventType : int
    {
        LoadScene = 0,
        LoadAnimation = 2,
        LoadAudio = 4,
        LoadModels = 6,
        UnloadModels = 7,
        LoadParticles = 8,
        LoadOverlays = 10,
        LoadGxt2 = 12,
        EnableHideObject = 14,
        DisableHideObject = 15,
        EnableFixupModel = 16,
        EnableBlockBounds = 18,
        DisableBlockBounds = 19,
        EnableScreenFade = 20,
        DisableScreenFade = 21,
        EnableAnimation = 22,
        DisableAnimation = 23,
        EnableParticleEffect = 24,
        DisableParticleEffect = 25,
        EnableOverlay = 26,
        DisableOverlay = 27,
        EnableAudio = 28,
        DisableAudio = 29,
        Subtitle = 30,
        PedVariation = 34,
        CameraCut = 43,
        LoadRayfireDes = 46,
        UnloadRayfireDes = 47,
        EnableCamera = 48,
        CameraUnk1 = 49,
        CameraUnk2 = 50,
        DisableCamera = 51,
        DecalUnk1 = 52,
        DecalUnk2 = 53,
        CameraShadowCascade = 54,
        CameraUnk3 = 55,
        CameraUnk4 = 59,
        CameraUnk5 = 63,
        CameraUnk6 = 64,
        PropUnk1 = 73,
        EnableLight = 74,
        DisableLight = 75,
        CameraUnk7 = 76,
        Unk1 = 77,
        Unk2 = 78,
        CameraUnk8 = 79,
        VehicleUnk1 = 258,
        PedUnk1 = 262,
    }
    [TC(typeof(EXP))] public class CutEvent : CutBase  // rage__cutfEvent
    {
        public float fTime { get; set; } // PsoDataType.Float, 16, 0, 0),
        public CutEventType iEventId { get; set; } // PsoDataType.SInt, 20, 0, 0),
        public int iEventArgsIndex { get; set; } // PsoDataType.SInt, 24, 0, 0),
        //public object pChildEvents { get; set; } // PsoDataType.Structure, 32, 3, 0),
        public uint StickyId { get; set; } // PsoDataType.UInt, 40, 0, 0),
        public bool IsChild { get; set; } // PsoDataType.Bool, 44, 0, 0)

        public CutEventArgs EventArgs { get; set; }



        public override void ReadXml(XmlNode node)
        {
            fTime = Xml.GetChildFloatAttribute(node, "fTime", "value");
            iEventId = (CutEventType)Xml.GetChildIntAttribute(node, "iEventId", "value");
            iEventArgsIndex = Xml.GetChildIntAttribute(node, "iEventArgsIndex", "value");
            //pChildEvents = CutsceneFile2.ReadObject(node, "pChildEvents"); //seems never used
            StickyId = Xml.GetChildUIntAttribute(node, "StickyId", "value");
            IsChild = Xml.GetChildBoolAttribute(node, "IsChild", "value");

            var cNode = node.SelectSingleNode("pChildEvents");
            if ((cNode?.ChildNodes?.Count > 0) || (cNode?.Attributes?.Count > 0))
            { }//nothing gets here?
        }

        public override string ToString()
        {
            return fTime.ToString("0.00") + " : " + iEventId.ToString();
        }
    }
    [TC(typeof(EXP))] public class CutObjectIdEvent : CutEvent  // rage__cutfObjectIdEvent
    {
        public int iObjectId { get; set; } // PsoDataType.SInt, 48, 0, 0)

        public CutObject Object { get; set; }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            iObjectId = Xml.GetChildIntAttribute(node, "iObjectId", "value");
        }
    }

    [TC(typeof(EXP))] public class CutEventArgs : CutBase  // rage__cutfEventArgs
    {
        public CutParAttributeList attributeList { get; set; } // PsoDataType.Structure, 12, 0, MetaName.rage__parAttributeList),
        public CutFAttributeList cutfAttributes { get; set; } // PsoDataType.Structure, 24, 4, 0)

        public override void ReadXml(XmlNode node)
        {
            attributeList = CutsceneFile2.ReadObject<CutParAttributeList>(node, "attributeList");
            cutfAttributes = CutsceneFile2.ReadObject<CutFAttributeList>(node, "cutfAttributes");
        }
    }
    [TC(typeof(EXP))] public class CutNameEventArgs : CutEventArgs  // rage__cutfNameEventArgs
    {
        public MetaHash cName { get; set; } // PsoDataType.String, 32, 7, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            cName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cName"));
        }
    }
    [TC(typeof(EXP))] public class CutFinalNameEventArgs : CutEventArgs  // rage__cutfFinalNameEventArgs
    {
        public string cName { get; set; } // PsoDataType.String, 32, 3, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            cName = Xml.GetChildInnerText(node, "cName");
        }
    }
    [TC(typeof(EXP))] public class CutObjectIdEventArgs : CutEventArgs  // rage__cutfObjectIdEventArgs
    {
        public int iObjectId { get; set; } // PsoDataType.SInt, 32, 0, 0)

        public CutObject Object { get; set; }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            iObjectId = Xml.GetChildIntAttribute(node, "iObjectId", "value");
        }
    }
    [TC(typeof(EXP))] public class CutObjectIdListEventArgs : CutEventArgs  // rage__cutfObjectIdListEventArgs
    {
        public int[] iObjectIdList { get; set; } // PsoDataType.Array, 32, 0, (MetaName)2)//ARRAYINFO, PsoDataType.SInt, 0, 0, 0),

        public CutObject[] ObjectList { get; set; }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            iObjectIdList = Xml.GetChildRawIntArray(node, "iObjectIdList");
        }
    }
    [TC(typeof(EXP))] public class CutFloatValueEventArgs : CutEventArgs  // rage__cutfFloatValueEventArgs
    {
        public float fValue { get; set; } // PsoDataType.Float, 32, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            fValue = Xml.GetChildFloatAttribute(node, "fValue", "value");
        }
    }
    [TC(typeof(EXP))] public class CutBoolValueEventArgs : CutEventArgs  // rage__cutfBoolValueEventArgs
    {
        public bool bValue { get; set; } // PsoDataType.Bool, 32, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            bValue = Xml.GetChildBoolAttribute(node, "bValue", "value");
        }
    }

    [TC(typeof(EXP))] public class CutLoadSceneEventArgs : CutNameEventArgs  // rage__cutfLoadSceneEventArgs
    {
        public Vector3 vOffset { get; set; } // PsoDataType.Float3, 48, 0, 0),
        public float fRotation { get; set; } // PsoDataType.Float, 64, 0, 0),
        public float fPitch { get; set; } // PsoDataType.Float, 68, 0, 0),
        public float fRoll { get; set; } // PsoDataType.Float, 72, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            vOffset = Xml.GetChildVector3Attributes(node, "vOffset");
            fRotation = Xml.GetChildFloatAttribute(node, "fRotation", "value");
            fPitch = Xml.GetChildFloatAttribute(node, "fPitch", "value");
            fRoll = Xml.GetChildFloatAttribute(node, "fRoll", "value");
        }
    }
    [TC(typeof(EXP))] public class CutSubtitleEventArgs : CutNameEventArgs  // rage__cutfSubtitleEventArgs
    {
        public int iLanguageID { get; set; } // PsoDataType.SInt, 40, 0, 0),
        public int iTransitionIn { get; set; } // PsoDataType.SInt, 44, 0, 0),
        public float fTransitionInDuration { get; set; } // PsoDataType.Float, 48, 0, 0),
        public int iTransitionOut { get; set; } // PsoDataType.SInt, 52, 0, 0),
        public float fTransitionOutDuration { get; set; } // PsoDataType.Float, 56, 0, 0),
        public float fSubtitleDuration { get; set; } // PsoDataType.Float, 60, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            iLanguageID = Xml.GetChildIntAttribute(node, "iLanguageID", "value");
            iTransitionIn = Xml.GetChildIntAttribute(node, "iTransitionIn", "value");
            fTransitionInDuration = Xml.GetChildFloatAttribute(node, "fTransitionInDuration", "value");
            iTransitionOut = Xml.GetChildIntAttribute(node, "iTransitionOut", "value");
            fTransitionOutDuration = Xml.GetChildFloatAttribute(node, "fTransitionOutDuration", "value");
            fSubtitleDuration = Xml.GetChildFloatAttribute(node, "fSubtitleDuration", "value");
        }
    }
    [TC(typeof(EXP))] public class CutCameraCutEventArgs : CutNameEventArgs  // rage__cutfCameraCutEventArgs
    {
        public Vector3 vPosition { get; set; } // PsoDataType.Float3, 48, 0, 0),
        public Quaternion vRotationQuaternion { get; set; } // PsoDataType.Float4, 64, 0, 0),
        public float fNearDrawDistance { get; set; } // PsoDataType.Float, 80, 0, 0),
        public float fFarDrawDistance { get; set; } // PsoDataType.Float, 84, 0, 0),
        public float fMapLodScale { get; set; } // PsoDataType.Float, 88, 0, 0),
        public float ReflectionLodRangeStart { get; set; } // PsoDataType.Float, 92, 0, 0),
        public float ReflectionLodRangeEnd { get; set; } // PsoDataType.Float, 96, 0, 0),
        public float ReflectionSLodRangeStart { get; set; } // PsoDataType.Float, 100, 0, 0),
        public float ReflectionSLodRangeEnd { get; set; } // PsoDataType.Float, 104, 0, 0),
        public float LodMultHD { get; set; } // PsoDataType.Float, 108, 0, 0),
        public float LodMultOrphanedHD { get; set; } // PsoDataType.Float, 112, 0, 0),
        public float LodMultLod { get; set; } // PsoDataType.Float, 116, 0, 0),
        public float LodMultSLod1 { get; set; } // PsoDataType.Float, 120, 0, 0),
        public float LodMultSLod2 { get; set; } // PsoDataType.Float, 124, 0, 0),
        public float LodMultSLod3 { get; set; } // PsoDataType.Float, 128, 0, 0),
        public float LodMultSLod4 { get; set; } // PsoDataType.Float, 132, 0, 0),
        public float WaterReflectionFarClip { get; set; } // PsoDataType.Float, 136, 0, 0),
        public float SSAOLightInten { get; set; } // PsoDataType.Float, 140, 0, 0),
        public float ExposurePush { get; set; } // PsoDataType.Float, 144, 0, 0),
        public float LightFadeDistanceMult { get; set; } // PsoDataType.Float, 148, 0, 0),
        public float LightShadowFadeDistanceMult { get; set; } // PsoDataType.Float, 152, 0, 0),
        public float LightSpecularFadeDistMult { get; set; } // PsoDataType.Float, 156, 0, 0),
        public float LightVolumetricFadeDistanceMult { get; set; } // PsoDataType.Float, 160, 0, 0),
        public float DirectionalLightMultiplier { get; set; } // PsoDataType.Float, 164, 0, 0),
        public float LensArtefactMultiplier { get; set; } // PsoDataType.Float, 168, 0, 0),
        public float BloomMax { get; set; } // PsoDataType.Float, 172, 0, 0),
        public bool DisableHighQualityDof { get; set; } // PsoDataType.Bool, 176, 0, 0),
        public bool FreezeReflectionMap { get; set; } // PsoDataType.Bool, 177, 0, 0),
        public bool DisableDirectionalLighting { get; set; } // PsoDataType.Bool, 178, 0, 0),
        public bool AbsoluteIntensityEnabled { get; set; } // PsoDataType.Bool, 179, 0, 0),
        public CutCameraCutCharacterLightParams CharacterLight { get; set; } // PsoDataType.Structure, 192, 0, MetaName.rage__cutfCameraCutCharacterLightParams),
        public CutCameraCutTimeOfDayDofModifier[] TimeOfDayDofModifers { get; set; } // PsoDataType.Array, 256, 0, (MetaName)34)//ARRAYINFO, PsoDataType.Structure, 0, 0, MetaName.rage__cutfCameraCutTimeOfDayDofModifier),

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            vPosition = Xml.GetChildVector3Attributes(node, "vPosition");
            vRotationQuaternion = Xml.GetChildVector4Attributes(node, "vRotationQuaternion").ToQuaternion();
            fNearDrawDistance = Xml.GetChildFloatAttribute(node, "fNearDrawDistance", "value");
            fFarDrawDistance = Xml.GetChildFloatAttribute(node, "fFarDrawDistance", "value");
            fMapLodScale = Xml.GetChildFloatAttribute(node, "fMapLodScale", "value");
            ReflectionLodRangeStart = Xml.GetChildFloatAttribute(node, "ReflectionLodRangeStart", "value");
            ReflectionLodRangeEnd = Xml.GetChildFloatAttribute(node, "ReflectionLodRangeEnd", "value");
            ReflectionSLodRangeStart = Xml.GetChildFloatAttribute(node, "ReflectionSLodRangeStart", "value");
            ReflectionSLodRangeEnd = Xml.GetChildFloatAttribute(node, "ReflectionSLodRangeEnd", "value");
            LodMultHD = Xml.GetChildFloatAttribute(node, "LodMultHD", "value");
            LodMultOrphanedHD = Xml.GetChildFloatAttribute(node, "LodMultOrphanedHD", "value");
            LodMultLod = Xml.GetChildFloatAttribute(node, "LodMultLod", "value");
            LodMultSLod1 = Xml.GetChildFloatAttribute(node, "LodMultSLod1", "value");
            LodMultSLod2 = Xml.GetChildFloatAttribute(node, "LodMultSLod2", "value");
            LodMultSLod3 = Xml.GetChildFloatAttribute(node, "LodMultSLod3", "value");
            LodMultSLod4 = Xml.GetChildFloatAttribute(node, "LodMultSLod4", "value");
            WaterReflectionFarClip = Xml.GetChildFloatAttribute(node, "WaterReflectionFarClip", "value");
            SSAOLightInten = Xml.GetChildFloatAttribute(node, "SSAOLightInten", "value");
            ExposurePush = Xml.GetChildFloatAttribute(node, "ExposurePush", "value");
            LightFadeDistanceMult = Xml.GetChildFloatAttribute(node, "LightFadeDistanceMult", "value");
            LightShadowFadeDistanceMult = Xml.GetChildFloatAttribute(node, "LightShadowFadeDistanceMult", "value");
            LightSpecularFadeDistMult = Xml.GetChildFloatAttribute(node, "LightSpecularFadeDistMult", "value");
            LightVolumetricFadeDistanceMult = Xml.GetChildFloatAttribute(node, "LightVolumetricFadeDistanceMult", "value");
            DirectionalLightMultiplier = Xml.GetChildFloatAttribute(node, "DirectionalLightMultiplier", "value");
            LensArtefactMultiplier = Xml.GetChildFloatAttribute(node, "LensArtefactMultiplier", "value");
            BloomMax = Xml.GetChildFloatAttribute(node, "BloomMax", "value");
            DisableHighQualityDof = Xml.GetChildBoolAttribute(node, "DisableHighQualityDof", "value");
            FreezeReflectionMap = Xml.GetChildBoolAttribute(node, "FreezeReflectionMap", "value");
            DisableDirectionalLighting = Xml.GetChildBoolAttribute(node, "DisableDirectionalLighting", "value");
            AbsoluteIntensityEnabled = Xml.GetChildBoolAttribute(node, "AbsoluteIntensityEnabled", "value");
            CharacterLight = CutsceneFile2.ReadObject<CutCameraCutCharacterLightParams>(node, "CharacterLight");
            TimeOfDayDofModifers = XmlMeta.ReadItemArrayNullable<CutCameraCutTimeOfDayDofModifier>(node, "TimeOfDayDofModifers");
        }
    }
    [TC(typeof(EXP))] public class CutCameraCutCharacterLightParams : CutBase  // rage__cutfCameraCutCharacterLightParams
    {
        public bool bUseTimeCycleValues { get; set; } // PsoDataType.Bool, 8, 0, 0),
        public Vector3 vDirection { get; set; } // PsoDataType.Float3, 16, 0, 0),
        public Vector3 vColour { get; set; } // PsoDataType.Float3, 32, 0, 0),
        public float fIntensity { get; set; } // PsoDataType.Float, 48, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            bUseTimeCycleValues = Xml.GetChildBoolAttribute(node, "bUseTimeCycleValues", "value");
            vDirection = Xml.GetChildVector3Attributes(node, "vDirection");
            vColour = Xml.GetChildVector3Attributes(node, "vColour");
            fIntensity = Xml.GetChildFloatAttribute(node, "fIntensity", "value");
        }
    }
    [TC(typeof(EXP))] public class CutCameraCutTimeOfDayDofModifier : CutBase  // rage__cutfCameraCutTimeOfDayDofModifier
    {
        //no definition available for this??
    }

    [TC(typeof(EXP))] public class CutObjectIdNameEventArgs : CutObjectIdEventArgs  // rage__cutfObjectIdNameEventArgs
    {
        public MetaHash cName { get; set; } // PsoDataType.String, 40, 7, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            cName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cName"));
        }
    }
    [TC(typeof(EXP))] public class CutObjectVariationEventArgs : CutObjectIdEventArgs  // rage__cutfObjectVariationEventArgs
    {
        public int iComponent { get; set; } // PsoDataType.SInt, 40, 0, 0),
        public int iDrawable { get; set; } // PsoDataType.SInt, 44, 0, 0),
        public int iTexture { get; set; } // PsoDataType.SInt, 48, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            iComponent = Xml.GetChildIntAttribute(node, "iComponent", "value");
            iDrawable = Xml.GetChildIntAttribute(node, "iDrawable", "value");
            iTexture = Xml.GetChildIntAttribute(node, "iTexture", "value");
        }
    }
    [TC(typeof(EXP))] public class CutVehicleVariationEventArgs : CutObjectIdEventArgs  // rage__cutfVehicleVariationEventArgs
    {
        public int iMainBodyColour { get; set; } // PsoDataType.SInt, 40, 0, 0),
        public int iSecondBodyColour { get; set; } // PsoDataType.SInt, 44, 0, 0),
        public int iSpecularColour { get; set; } // PsoDataType.SInt, 48, 0, 0),
        public int iWheelTrimColour { get; set; } // PsoDataType.SInt, 52, 0, 0),
        public int Unk_2747538743 { get; set; } // PsoDataType.SInt, 56, 0, 0),
        public int iLivery { get; set; } // PsoDataType.SInt, 60, 0, 0),
        public int iLivery2 { get; set; } // PsoDataType.SInt, 64, 0, 0),
        public float fDirtLevel { get; set; } // PsoDataType.Float, 68, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            iMainBodyColour = Xml.GetChildIntAttribute(node, "iMainBodyColour", "value");
            iSecondBodyColour = Xml.GetChildIntAttribute(node, "iSecondBodyColour", "value");
            iSpecularColour = Xml.GetChildIntAttribute(node, "iSpecularColour", "value");
            iWheelTrimColour = Xml.GetChildIntAttribute(node, "iWheelTrimColour", "value");
            Unk_2747538743 = Xml.GetChildIntAttribute(node, "hash_A3C41D37", "value");
            iLivery = Xml.GetChildIntAttribute(node, "iLivery", "value");
            iLivery2 = Xml.GetChildIntAttribute(node, "iLivery2", "value");
            fDirtLevel = Xml.GetChildFloatAttribute(node, "fDirtLevel", "value");
        }
    }
    [TC(typeof(EXP))] public class CutVehicleExtraEventArgs : CutObjectIdEventArgs  // rage__cutfVehicleExtraEventArgs
    {
        public int[] pExtraBoneIds { get; set; } // PsoDataType.Array, 40, 0, (MetaName)3)//ARRAYINFO, PsoDataType.SInt, 0, 0, 0),

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            pExtraBoneIds = Xml.GetChildRawIntArray(node, "pExtraBoneIds");
        }
    }

    [TC(typeof(EXP))] public class CutDecalEventArgs : CutEventArgs  // rage__cutfDecalEventArgs
    {
        public Vector3 vPosition { get; set; } // PsoDataType.Float3, 32, 0, 0),
        public Quaternion vRotation { get; set; } // PsoDataType.Float4, 48, 0, 0),
        public float fWidth { get; set; } // PsoDataType.Float, 64, 0, 0),
        public float fHeight { get; set; } // PsoDataType.Float, 68, 0, 0),
        public uint Colour { get; set; } // PsoDataType.UInt, 72, 1, 0),
        public float fLifeTime { get; set; } // PsoDataType.Float, 76, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            vPosition = Xml.GetChildVector3Attributes(node, "vPosition");
            vRotation = Xml.GetChildVector4Attributes(node, "vRotation").ToQuaternion();
            fWidth = Xml.GetChildFloatAttribute(node, "fWidth", "value");
            fHeight = Xml.GetChildFloatAttribute(node, "fHeight", "value");
            Colour = Xml.GetChildUIntAttribute(node, "Colour", "value");
            fLifeTime = Xml.GetChildFloatAttribute(node, "fLifeTime", "value");
        }
    }
    [TC(typeof(EXP))] public class CutScreenFadeEventArgs : CutEventArgs  // rage__cutfScreenFadeEventArgs
    {
        public float fValue { get; set; } // PsoDataType.Float, 32, 0, 0),
        public uint color { get; set; } // PsoDataType.UInt, 40, 1, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            fValue = Xml.GetChildFloatAttribute(node, "fValue", "value");
            color = Xml.GetChildUIntAttribute(node, "color", "value");
        }
    }
    [TC(typeof(EXP))] public class CutCascadeShadowEventArgs : CutEventArgs  // rage__cutfCascadeShadowEventArgs
    {
        public MetaHash cameraCutHashName { get; set; } // PsoDataType.String, 32, 7, 0),
        public Vector3 position { get; set; } // PsoDataType.Float3, 48, 0, 0),
        public float radius { get; set; } // PsoDataType.Float, 64, 0, 0),
        public float interpTime { get; set; } // PsoDataType.Float, 68, 0, 0),
        public int cascadeIndex { get; set; } // PsoDataType.SInt, 72, 0, 0),
        public bool enabled { get; set; } // PsoDataType.Bool, 76, 0, 0),
        public bool interpolateToDisabled { get; set; } // PsoDataType.Bool, 77, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            cameraCutHashName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "cameraCutHashName"));
            position = Xml.GetChildVector3Attributes(node, "position");
            radius = Xml.GetChildFloatAttribute(node, "radius", "value");
            interpTime = Xml.GetChildFloatAttribute(node, "interpTime", "value");
            cascadeIndex = Xml.GetChildIntAttribute(node, "cascadeIndex", "value");
            enabled = Xml.GetChildBoolAttribute(node, "enabled", "value");
            interpolateToDisabled = Xml.GetChildBoolAttribute(node, "interpolateToDisabled", "value");
        }
    }
    [TC(typeof(EXP))] public class CutTriggerLightEffectEventArgs : CutEventArgs  // rage__cutfTriggerLightEffectEventArgs
    {
        public int iAttachParentId { get; set; } // PsoDataType.SInt, 32, 0, 0),
        public ushort iAttachBoneHash { get; set; } // PsoDataType.UShort, 36, 0, 0),
        public MetaHash AttachedParentName { get; set; } // PsoDataType.String, 40, 7, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            iAttachParentId = Xml.GetChildIntAttribute(node, "iAttachParentId", "value");
            iAttachBoneHash = (ushort)Xml.GetChildUIntAttribute(node, "iAttachBoneHash", "value");
            AttachedParentName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "AttachedParentName"));
        }
    }
    [TC(typeof(EXP))] public class CutPlayParticleEffectEventArgs : CutEventArgs  // rage__cutfPlayParticleEffectEventArgs
    {
        public Quaternion vInitialBoneRotation { get; set; } // PsoDataType.Float4, 32, 0, 0),
        public Vector3 vInitialBoneOffset { get; set; } // PsoDataType.Float3, 48, 0, 0),
        public int iAttachParentId { get; set; } // PsoDataType.SInt, 64, 0, 0),
        public ushort iAttachBoneHash { get; set; } // PsoDataType.UShort, 68, 0, 0)

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            vInitialBoneRotation = Xml.GetChildVector4Attributes(node, "vInitialBoneRotation").ToQuaternion();
            vInitialBoneOffset = Xml.GetChildVector3Attributes(node, "vInitialBoneOffset");
            iAttachParentId = Xml.GetChildIntAttribute(node, "iAttachParentId", "value");
            iAttachBoneHash = (ushort)Xml.GetChildUIntAttribute(node, "iAttachBoneHash", "value");
        }
    }


}
