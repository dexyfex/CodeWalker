using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using SharpDX;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class CutFile : PackedFile
    {
        public RpfFileEntry FileEntry { get; set; }
        public PsoFile Pso { get; set; }


        public CutFile()
        { }
        public CutFile(RpfFileEntry entry)
        {
            FileEntry = entry;
        }


        public void Load(byte[] data, RpfFileEntry entry)
        {
            //MemoryStream ms = new MemoryStream(data);

            FileEntry = entry;

            MemoryStream ms = new MemoryStream(data);

            if (PsoFile.IsPSO(ms))
            {
                Pso = new PsoFile();
                Pso.Load(ms);

                //var xml = PsoXml.GetXml(Pso);


            }
            else
            {

            }
        }

    }




    [TC(typeof(EXP))] public class CutsceneFile2  // rage__cutfCutsceneFile2
    {
        public float fTotalDuration { get; set; } // PsoDataType.Float, 268, 0, 0),
        public string cFaceDir { get; set; } // PsoDataType.String, 272, 0, (MetaName)16777216),
        public uint[] iCutsceneFlags { get; set; } // PsoDataType.Array, 528, 4, (MetaName)262146),//ARRAYINFO, PsoDataType.UInt, 0, 0, 0),
        public Vector3 vOffset { get; set; } // PsoDataType.Float3, 544, 0, 0),
        public float fRotation { get; set; } // PsoDataType.Float, 560, 0, 0),
        public string cExtraRoom { get; set; } // PsoDataType.String, 564, 0, (MetaName)1572864),
        public Vector3 vExtraRoomPos { get; set; } // PsoDataType.Float3, 592, 0, 0),
        public Vector3 vTriggerOffset { get; set; } // PsoDataType.Float3, 608, 0, 0),
        public object[] pCutsceneObjects { get; set; } // PsoDataType.Array, 624, 0, (MetaName)9),//ARRAYINFO, PsoDataType.Structure, 0, 3, 0),
        public object[] pCutsceneLoadEventList { get; set; } // PsoDataType.Array, 640, 0, (MetaName)11),//ARRAYINFO, PsoDataType.Structure, 0, 3, 0),
        public object[] pCutsceneEventList { get; set; } // PsoDataType.Array, 656, 0, (MetaName)13),//ARRAYINFO, PsoDataType.Structure, 0, 3, 0),
        public object[] pCutsceneEventArgsList { get; set; } // PsoDataType.Array, 672, 0, (MetaName)15),//ARRAYINFO, PsoDataType.Structure, 0, 3, 0),
        public CutParAttributeList attributes { get; set; } // PsoDataType.Structure, 688, 0, MetaName.rage__parAttributeList),
        public CutFAttributeList cutfAttributes { get; set; } // PsoDataType.Structure, 696, 4, 0),
        public int iRangeStart { get; set; } // PsoDataType.SInt, 704, 0, 0),
        public int iRangeEnd { get; set; } // PsoDataType.SInt, 708, 0, 0),
        public int iAltRangeEnd { get; set; } // PsoDataType.SInt, 712, 0, 0),
        public float fSectionByTimeSliceDuration { get; set; } // PsoDataType.Float, 716, 0, 0),
        public float fFadeOutCutsceneDuration { get; set; } // PsoDataType.Float, 720, 0, 0),
        public float fFadeInGameDuration { get; set; } // PsoDataType.Float, 724, 0, 0),
        public uint fadeInColor { get; set; } // PsoDataType.UInt, 728, 1, 0),
        public int iBlendOutCutsceneDuration { get; set; } // PsoDataType.SInt, 732, 0, 0),
        public int iBlendOutCutsceneOffset { get; set; } // PsoDataType.SInt, 736, 0, 0),
        public float fFadeOutGameDuration { get; set; } // PsoDataType.Float, 740, 0, 0),
        public float fFadeInCutsceneDuration { get; set; } // PsoDataType.Float, 744, 0, 0),
        public uint fadeOutColor { get; set; } // PsoDataType.UInt, 748, 1, 0),
        public uint Unk_619896503 { get; set; } // PsoDataType.UInt, 728, 0, 0),
        public float[] cameraCutList { get; set; } // PsoDataType.Array, 752, 0, (MetaName)31),//ARRAYINFO, PsoDataType.Float, 0, 0, 0),
        public float[] sectionSplitList { get; set; } // PsoDataType.Array, 768, 0, (MetaName)MetaTypeName.FLOAT),//ARRAYINFO, PsoDataType.Float, 0, 0, 0),
        public CutConcatData[] concatDataList { get; set; } // PsoDataType.Array, 784, 1, (MetaName)2621475),//ARRAYINFO, PsoDataType.Structure, 0, 0, MetaName.rage__cutfCutsceneFile2__SConcatData),
        public CutHaltFrequency[] discardFrameList { get; set; } // PsoDataType.Array, 5280, 0, (MetaName)37)//ARRAYINFO, PsoDataType.Structure, 0, 0, MetaName.vHaltFrequency),




    }


    [TC(typeof(EXP))] public class CutParAttributeList  // rage__parAttributeList
    {
        public byte UserData1 { get; set; } // PsoDataType.UByte, 8, 0, 0),
        public byte UserData2 { get; set; } // PsoDataType.UByte, 9, 0, 0)

    }

    [TC(typeof(EXP))] public class CutFAttributeList  // rage__cutfAttributeList
    {
        public object[] Items { get; set; } // PsoDataType.Array, 0, 0, 0)//ARRAYINFO, PsoDataType.Structure, 0, 3, 0),
        //Cut_1626675902 (int attribute)
        //Cut_1674696498 (float attribute)
        //Cut_557437386 (string attribute)

    }
    [TC(typeof(EXP))] public class Cut_1626675902
    {
        public MetaHash Name { get; set; } // PsoDataType.String, 8, 8, 0),
        public int Value { get; set; } // PsoDataType.SInt, 16, 0, 0)

    }
    [TC(typeof(EXP))] public class Cut_1674696498
    {
        public MetaHash Name { get; set; } // PsoDataType.String, 8, 8, 0),
        public float Value { get; set; } // PsoDataType.Float, 16, 0, 0)

    }
    [TC(typeof(EXP))] public class Cut_557437386
    {
        public MetaHash Name { get; set; } // PsoDataType.String, 8, 8, 0),
        public string Value { get; set; } // PsoDataType.String, 16, 3, 0)

    }

    [TC(typeof(EXP))] public class CutConcatData  // rage__cutfCutsceneFile2__SConcatData
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




    }

    [TC(typeof(EXP))] public class CutHaltFrequency  // vHaltFrequency
    {
        public MetaHash cSceneName { get; set; } // PsoDataType.String, 0, 7, 0),
        public int[] frames { get; set; } // PsoDataType.Array, 8, 0, (MetaName)1)//ARRAYINFO, PsoDataType.SInt, 0, 0, 0),

    }


    [TC(typeof(EXP))] public class CutAssetManagerObject  // rage__cutfAssetManagerObject
    {
        public int iObjectId { get; set; } // PsoDataType.SInt, 8, 0, 0),
        public CutParAttributeList attributes { get; set; } // PsoDataType.Structure, 20, 0, MetaName.rage__parAttributeList),
        public CutFAttributeList cutfAttributes { get; set; } // PsoDataType.Structure, 32, 4, 0)

    }
    [TC(typeof(EXP))] public class CutAnimationManagerObject  // rage__cutfAnimationManagerObject
    {
        public int iObjectId { get; set; } // PsoDataType.SInt, 8, 0, 0),
        public CutParAttributeList attributeList { get; set; } // PsoDataType.Structure, 20, 0, MetaName.rage__parAttributeList),
        public CutFAttributeList cutfAttributes { get; set; } // PsoDataType.Structure, 32, 4, 0)

    }
    [TC(typeof(EXP))] public abstract class CutNamedObject : CutAnimationManagerObject
    {
        public MetaHash cName { get; set; } // PsoDataType.String, 40, 7, 0),

    }
    [TC(typeof(EXP))] public class CutCameraObject : CutNamedObject  // rage__cutfCameraObject
    {
        public uint AnimStreamingBase { get; set; } // PsoDataType.UInt, 48, 0, 0),
        public float fNearDrawDistance { get; set; } // PsoDataType.Float, 56, 0, 0),
        public float fFarDrawDistance { get; set; } // PsoDataType.Float, 60, 0, 0)

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

    }
    [TC(typeof(EXP))] public class CutBlockingBoundsObject : CutNamedObject  // rage__cutfBlockingBoundsObject
    {
        public Vector3[] vCorners { get; set; } // PsoDataType.Array, 48, 4, (MetaName)262148),//ARRAYINFO, PsoDataType.Float3, 0, 0, 0),
        public float fHeight { get; set; } // PsoDataType.Float, 112, 0, 0)

    }
    [TC(typeof(EXP))] public class CutAudioObject : CutNamedObject  // rage__cutfAudioObject
    {
        public float fOffset { get; set; } // PsoDataType.Float, 56, 0, 0)

    }
    [TC(typeof(EXP))] public class CutHiddenModelObject : CutNamedObject  // rage__cutfHiddenModelObject
    {
        public Vector3 vPosition { get; set; } // PsoDataType.Float3, 48, 0, 0),
        public float fRadius { get; set; } // PsoDataType.Float, 64, 0, 0)

    }
    [TC(typeof(EXP))] public class CutOverlayObject : CutNamedObject  // rage__cutfOverlayObject
    {
        public string cRenderTargetName { get; set; } // PsoDataType.String, 56, 3, 0),
        public uint iOverlayType { get; set; } // PsoDataType.UInt, 72, 0, 0),
        public MetaHash modelHashName { get; set; } // PsoDataType.String, 76, 7, 0)

    }
    [TC(typeof(EXP))] public class CutSubtitleObject : CutNamedObject  // rage__cutfSubtitleObject
    {
    }
    [TC(typeof(EXP))] public class CutLightObject : CutNamedObject  // rage__cutfLightObject
    {
        public Vector3 vDirection { get; set; } // PsoDataType.Float3, 112, 0, 0),
        public Vector3 vColour { get; set; } // PsoDataType.Float3, 128, 0, 0),
        public Vector3 vPosition { get; set; } // PsoDataType.Float3, 144, 0, 0),
        public float fIntensity { get; set; } // PsoDataType.Float, 160, 0, 0),
        public float fFallOff { get; set; } // PsoDataType.Float, 164, 0, 0),
        public float fConeAngle { get; set; } // PsoDataType.Float, 168, 0, 0),
        public float fVolumeIntensity { get; set; } // PsoDataType.Float, 172, 0, 0),
        public float fVolumeSizeScale { get; set; } // PsoDataType.Float, 176, 0, 0),
        public float fCoronaSize { get; set; } // PsoDataType.Float, 180, 0, 0),
        public float fCoronaIntensity { get; set; } // PsoDataType.Float, 184, 0, 0),
        public float fCoronaZBias { get; set; } // PsoDataType.Float, 188, 0, 0),
        public float fInnerConeAngle { get; set; } // PsoDataType.Float, 192, 0, 0),
        public float fExponentialFallOff { get; set; } // PsoDataType.Float, 196, 0, 0),
        public int iLightType { get; set; } // PsoDataType.SInt, 200, 0, 0),
        public int iLightProperty { get; set; } // PsoDataType.SInt, 204, 0, 0),
        public int TextureDictID { get; set; } // PsoDataType.SInt, 208, 0, 0),
        public int TextureKey { get; set; } // PsoDataType.SInt, 212, 0, 0),
        public int Unk_34975788 { get; set; } // PsoDataType.SInt, 216, 0, 0),
        public uint uLightFlags { get; set; } // PsoDataType.UInt, 220, 0, 0),
        public uint uHourFlags { get; set; } // PsoDataType.UInt, 224, 0, 0),
        public ushort Unk_1437992521 { get; set; } // PsoDataType.UShort, 228, 0, 0),
        public bool bStatic { get; set; } // PsoDataType.Bool, 230, 0, 0)

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
        public uint uLightFlags { get; set; } // PsoDataType.UInt, 176, 0, 0),
        public uint uHourFlags { get; set; } // PsoDataType.UInt, 180, 0, 0),
        public bool bStatic { get; set; } // PsoDataType.Bool, 186, 0, 0),
        public uint AnimStreamingBase { get; set; } // PsoDataType.UInt, 192, 0, 0)

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

    }
    [TC(typeof(EXP))] public class CutRayfireObject : CutNamedObject  // rage__cutfRayfireObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public Vector3 vStartPosition { get; set; } // PsoDataType.Float3, 64, 0, 0)

    }
    [TC(typeof(EXP))] public class CutParticleEffectObject : CutNamedObject  // rage__cutfParticleEffectObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public MetaHash athFxListHash { get; set; } // PsoDataType.String, 56, 7, 0)

    }
    [TC(typeof(EXP))] public class CutAnimatedParticleEffectObject : CutNamedObject  // rage__cutfAnimatedParticleEffectObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public uint AnimStreamingBase { get; set; } // PsoDataType.UInt, 56, 0, 0),
        public MetaHash athFxListHash { get; set; } // PsoDataType.String, 64, 7, 0)

    }
    [TC(typeof(EXP))] public class CutDecalObject : CutNamedObject  // rage__cutfDecalObject
    {
        public MetaHash StreamingName { get; set; } // PsoDataType.String, 48, 7, 0),
        public uint RenderId { get; set; } // PsoDataType.UInt, 56, 0, 0)

    }
    [TC(typeof(EXP))] public class CutScreenFadeObject : CutNamedObject  // rage__cutfScreenFadeObject
    {
    }
    [TC(typeof(EXP))] public class CutFixupModelObject : CutNamedObject  // rage__cutfFixupModelObject
    {
        public Vector3 vPosition { get; set; } // PsoDataType.Float3, 48, 0, 0),
        public float fRadius { get; set; } // PsoDataType.Float, 64, 0, 0)

    }


    [TC(typeof(EXP))] public class CutEvent  // rage__cutfEvent
    {
        public float fTime { get; set; } // PsoDataType.Float, 16, 0, 0),
        public int iEventId { get; set; } // PsoDataType.SInt, 20, 0, 0),
        public int iEventArgsIndex { get; set; } // PsoDataType.SInt, 24, 0, 0),
        public object pChildEvents { get; set; } // PsoDataType.Structure, 32, 3, 0),
        public uint StickyId { get; set; } // PsoDataType.UInt, 40, 0, 0),
        public bool IsChild { get; set; } // PsoDataType.Bool, 44, 0, 0)

    }
    [TC(typeof(EXP))] public class CutObjectIdEvent : CutEvent  // rage__cutfObjectIdEvent
    {
        public int iObjectId { get; set; } // PsoDataType.SInt, 48, 0, 0)

    }

    [TC(typeof(EXP))] public class CutEventArgs  // rage__cutfEventArgs
    {
        public CutParAttributeList attributeList { get; set; } // PsoDataType.Structure, 12, 0, MetaName.rage__parAttributeList),
        public CutFAttributeList cutfAttributes { get; set; } // PsoDataType.Structure, 24, 4, 0)

    }
    [TC(typeof(EXP))] public class CutNameEventArgs : CutEventArgs  // rage__cutfNameEventArgs
    {
        public MetaHash cName { get; set; } // PsoDataType.String, 32, 7, 0)

    }
    [TC(typeof(EXP))] public class CutFinalNameEventArgs : CutEventArgs  // rage__cutfFinalNameEventArgs
    {
        public string cName { get; set; } // PsoDataType.String, 32, 3, 0)

    }
    [TC(typeof(EXP))] public class CutObjectIdEventArgs : CutEventArgs  // rage__cutfObjectIdEventArgs
    {
        public int iObjectId { get; set; } // PsoDataType.SInt, 32, 0, 0)

    }
    [TC(typeof(EXP))] public class CutObjectIdListEventArgs : CutEventArgs  // rage__cutfObjectIdListEventArgs
    {
        public int[] iObjectIdList { get; set; } // PsoDataType.Array, 32, 0, (MetaName)2)//ARRAYINFO, PsoDataType.SInt, 0, 0, 0),

    }
    [TC(typeof(EXP))] public class CutFloatValueEventArgs : CutEventArgs  // rage__cutfFloatValueEventArgs
    {
        public float fValue { get; set; } // PsoDataType.Float, 32, 0, 0)

    }
    [TC(typeof(EXP))] public class CutBoolValueEventArgs : CutEventArgs  // rage__cutfBoolValueEventArgs
    {
        public bool bValue { get; set; } // PsoDataType.Bool, 32, 0, 0)

    }

    [TC(typeof(EXP))] public class CutLoadSceneEventArgs : CutNameEventArgs  // rage__cutfLoadSceneEventArgs
    {
        public Vector3 vOffset { get; set; } // PsoDataType.Float3, 48, 0, 0),
        public float fRotation { get; set; } // PsoDataType.Float, 64, 0, 0),
        public float fPitch { get; set; } // PsoDataType.Float, 68, 0, 0),
        public float fRoll { get; set; } // PsoDataType.Float, 72, 0, 0)

    }
    [TC(typeof(EXP))] public class CutSubtitleEventArgs : CutNameEventArgs  // rage__cutfSubtitleEventArgs
    {
        public int iLanguageID { get; set; } // PsoDataType.SInt, 40, 0, 0),
        public int iTransitionIn { get; set; } // PsoDataType.SInt, 44, 0, 0),
        public float fTransitionInDuration { get; set; } // PsoDataType.Float, 48, 0, 0),
        public int iTransitionOut { get; set; } // PsoDataType.SInt, 52, 0, 0),
        public float fTransitionOutDuration { get; set; } // PsoDataType.Float, 56, 0, 0),
        public float fSubtitleDuration { get; set; } // PsoDataType.Float, 60, 0, 0)

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

    }
    [TC(typeof(EXP))] public class CutCameraCutCharacterLightParams  // rage__cutfCameraCutCharacterLightParams
    {
        public bool bUseTimeCycleValues { get; set; } // PsoDataType.Bool, 8, 0, 0),
        public Vector3 vDirection { get; set; } // PsoDataType.Float3, 16, 0, 0),
        public Vector3 vColour { get; set; } // PsoDataType.Float3, 32, 0, 0),
        public float fIntensity { get; set; } // PsoDataType.Float, 48, 0, 0)

    }
    [TC(typeof(EXP))] public class CutCameraCutTimeOfDayDofModifier  // rage__cutfCameraCutTimeOfDayDofModifier
    {
        //no definition available for this??
    }

    [TC(typeof(EXP))] public class CutObjectIdNameEventArgs : CutObjectIdEventArgs  // rage__cutfObjectIdNameEventArgs
    {
        public MetaHash cName { get; set; } // PsoDataType.String, 40, 7, 0)

    }
    [TC(typeof(EXP))] public class CutObjectVariationEventArgs : CutObjectIdEventArgs  // rage__cutfObjectVariationEventArgs
    {
        public int iComponent { get; set; } // PsoDataType.SInt, 40, 0, 0),
        public int iDrawable { get; set; } // PsoDataType.SInt, 44, 0, 0),
        public int iTexture { get; set; } // PsoDataType.SInt, 48, 0, 0)

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

    }
    [TC(typeof(EXP))] public class CutVehicleExtraEventArgs : CutObjectIdEventArgs  // rage__cutfVehicleExtraEventArgs
    {
        public int[] pExtraBoneIds { get; set; } // PsoDataType.Array, 40, 0, (MetaName)3)//ARRAYINFO, PsoDataType.SInt, 0, 0, 0),

    }

    [TC(typeof(EXP))] public class CutDecalEventArgs : CutEventArgs  // rage__cutfDecalEventArgs
    {
        public Vector3 vPosition { get; set; } // PsoDataType.Float3, 32, 0, 0),
        public Quaternion vRotation { get; set; } // PsoDataType.Float4, 48, 0, 0),
        public float fWidth { get; set; } // PsoDataType.Float, 64, 0, 0),
        public float fHeight { get; set; } // PsoDataType.Float, 68, 0, 0),
        public uint Colour { get; set; } // PsoDataType.UInt, 72, 1, 0),
        public float fLifeTime { get; set; } // PsoDataType.Float, 76, 0, 0)

    }
    [TC(typeof(EXP))] public class CutScreenFadeEventArgs : CutEventArgs  // rage__cutfScreenFadeEventArgs
    {
        public float fValue { get; set; } // PsoDataType.Float, 32, 0, 0),
        public uint color { get; set; } // PsoDataType.UInt, 40, 1, 0)

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

    }
    [TC(typeof(EXP))] public class CutTriggerLightEffectEventArgs : CutEventArgs  // rage__cutfTriggerLightEffectEventArgs
    {
        public int iAttachParentId { get; set; } // PsoDataType.SInt, 32, 0, 0),
        public ushort iAttachBoneHash { get; set; } // PsoDataType.UShort, 36, 0, 0),
        public MetaHash AttachedParentName { get; set; } // PsoDataType.String, 40, 7, 0)

    }
    [TC(typeof(EXP))] public class CutPlayParticleEffectEventArgs : CutEventArgs  // rage__cutfPlayParticleEffectEventArgs
    {
        public Quaternion vInitialBoneRotation { get; set; } // PsoDataType.Float4, 32, 0, 0),
        public Vector3 vInitialBoneOffset { get; set; } // PsoDataType.Float3, 48, 0, 0),
        public int iAttachParentId { get; set; } // PsoDataType.SInt, 64, 0, 0),
        public ushort iAttachBoneHash { get; set; } // PsoDataType.UShort, 68, 0, 0)

    }


}
